using System.Text.Json;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

public enum ProjectWorkflowStage
{
    Preparing,
    Generating,
    Exporting,
    Validating
}

public sealed record ProjectWorkflowResult(bool Success, bool Cancelled, ModelGenerationPlan? Plan,
    IReadOnlyList<Diagnostic> PreparationDiagnostics, ExportResult? Export = null)
{
    public bool NeedsGeneration
    {
        get; init;
    }
    public bool CodeCommitted
    {
        get; init;
    }
}

/// <summary>Shared App/CLI operations. Code and data have separate atomic commit points.</summary>
public static class ProjectWorkflow
{
    public static Task<ProjectWorkflowResult> GenerateAsync(WorkspaceSettings settings,
        Func<ModelGenerationPlan, Task<bool>> confirmGeneration, Action<ProjectWorkflowStage>? progress = null,
        WorkflowOperation? operation = null) => RunAsync(settings, confirmGeneration, "generate", false, progress, operation);

    public static Task<ProjectWorkflowResult> ExportAsync(WorkspaceSettings settings,
        Func<ModelGenerationPlan, Task<bool>> confirmGeneration, bool rulesDebug = false,
        Action<ProjectWorkflowStage>? progress = null, WorkflowOperation? operation = null) =>
        RunAsync(settings, confirmGeneration, "export", rulesDebug, progress, operation);

    public static Task<ProjectWorkflowResult> ValidateAsync(WorkspaceSettings settings, bool rulesDebug = false,
        Action<ProjectWorkflowStage>? progress = null, WorkflowOperation? operation = null) =>
        RunAsync(settings, _ => Task.FromResult(false), "validate", rulesDebug, progress, operation);

    private static async Task<ProjectWorkflowResult> RunAsync(WorkspaceSettings settings,
        Func<ModelGenerationPlan, Task<bool>> confirmGeneration, string mode, bool rulesDebug,
        Action<ProjectWorkflowStage>? progress, WorkflowOperation? operation)
    {
        ArgumentNullException.ThrowIfNull(confirmGeneration);
        operation ??= new WorkflowOperation();
        var diagnostics = new DiagnosticCollector();
        diagnostics.Added += operation.ReportDiagnostic;
        ModelGenerationPlan? plan = null;
        bool codeCommitted = false;
        try
        {
            operation.Checkpoint();
            settings = mode == "validate" ? settings.ValidateOnly() : settings.Validate(generation: mode == "generate", allowMissingCode: true);
            progress?.Invoke(ProjectWorkflowStage.Preparing);
            operation.Report("preparing", ConfigText.Get("检查结构和生成代码", "Checking schema and generated code"));
            plan = await Task.Run(() => ModelGenerator.PrepareWithNamespaces(settings.Namespaces,settings.RuntimeFamily, settings.DataFolder, settings.CodeFolder, diagnostics,
                message => operation.Report("compiling", message), settings.Target, operation, PreparationLanguages(settings, operation), settings.DataFormat));
            operation.Checkpoint();
            if (plan == null)
            {
                return new(false, false, null, diagnostics.Snapshot());
            }

            if (operation.ApiChangeId != null && (operation.MigrationReceipt == null || !plan.AcceptApiMigration(operation.ApiChangeId,operation.MigrationReceipt)))
            {
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "API_CHANGE_ID_MISMATCH",
                    "API 变更摘要不匹配或基线无效；未写入文件，请重新预览并审阅本次变化。\n" + plan.DescribeChanges(), settings.CodeFolder));
                return new(false, false, plan, diagnostics.Snapshot());
            }

            if (mode == "validate" && plan.HasChanges)
            {
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "GENERATION_REQUIRED",
                    ConfigText.Get("代码缺失或结构已经变化；完整校验尚未完成。请先查看生成预览并生成代码，再重新校验。",
                        "Code is missing or out of date; full validation is incomplete. Review and generate code, then validate again."), settings.CodeFolder));
                return new(false, false, plan, diagnostics.Snapshot())
                {
                    NeedsGeneration = true
                };
            }

            bool needsConfirmation = mode == "export" ? plan.HasChanges : mode == "generate" && plan.RequiresConfirmation;
            if (needsConfirmation && !await confirmGeneration(plan))
            {
                return new(false, true, plan, diagnostics.Snapshot());
            }

            operation.Checkpoint();
            if (mode == "generate" || (mode == "export" && plan.HasChanges))
            {
                progress?.Invoke(ProjectWorkflowStage.Generating);
                operation.Report("generating", ConfigText.Get("正在提交生成事务", "Committing generated code"), committing: true);
                if (!await Task.Run(() => plan.Apply(diagnostics, acceptChanges: true)))
                {
                    return new(false, false, plan, diagnostics.Snapshot());
                }

                codeCommitted = true;
            }

            if (mode == "generate")
            {
                return new(true, false, plan, diagnostics.Snapshot())
                {
                    CodeCommitted = codeCommitted
                };
            }

            operation.Checkpoint();
            progress?.Invoke(mode == "validate" ? ProjectWorkflowStage.Validating : ProjectWorkflowStage.Exporting);
            ExportResult result = await Task.Run(() => mode == "validate" ? ExportPipeline.ValidateWithSources(settings, rulesDebug, operation) :
                ExportPipeline.RunWithSources(settings.DataFolder, settings.OutputFolder, settings.CodeFolder, settings.RulesFolder, rulesDebug, settings.Target, operation, settings.DataFormat, settings.CodeLanguages));
            return new(result.Success, result.Cancelled, plan, diagnostics.Snapshot(), result)
            {
                CodeCommitted = codeCommitted
            };
        }
        catch (OperationCanceledException) when (operation.CancellationToken.IsCancellationRequested)
        {
            return new(false, true, plan, diagnostics.Snapshot())
            {
                CodeCommitted = codeCommitted
            };
        }
        catch (Exception error) when (codeCommitted)
        {
            diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "EXPORT_AFTER_GENERATION_FAILED",
                ConfigText.Get("代码已经更新，数据未发布：", "Code was updated; data was not published: ") + error.Message));
            return new(false, false, plan, diagnostics.Snapshot())
            {
                CodeCommitted = true
            };
        }
    }

    internal static string[] PreparationLanguages(WorkspaceSettings settings, WorkflowOperation operation)
    {
        string[] selected = settings.CodeLanguages;
        if (operation.PruneLanguages) return selected;
        // Generation/export/validation keeps the accepted output set current. Deselecting a
        // language must not implicitly request deletion of its public API.
        // Only explicit pruning reviews language removals through the API gate.
        string summary = Path.Combine(settings.CodeFolder, ".schema.json");
        if (!File.Exists(summary)) return selected;
        try
        {
            var contract = JsonSerializer.Deserialize<SchemaSummary>(File.ReadAllText(summary))?.ApiContract;
            if (contract?.Policy == null) return selected;
            GeneratedApiContracts.Validate(contract);
            return ExportOptions.Languages(selected.Concat(contract.Policy.Profiles.Select(profile => profile.Language))
                .Distinct(StringComparer.Ordinal));
        }
        catch (Exception error) when (error is JsonException or InvalidDataException or ArgumentException)
        {
            // ModelGenerationPlan reports an invalid baseline and prevents writes.
            return selected;
        }
    }
}
