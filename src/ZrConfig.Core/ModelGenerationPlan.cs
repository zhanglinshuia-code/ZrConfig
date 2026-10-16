using System.Text;
using System.Text.Json;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

public sealed class ModelGenerationPlan
{
    private sealed record GenerationBinding(int Format, string InputRoot, int RequiredApiDescriptor);
    private readonly string _root;
    private readonly Dictionary<string, string> _files;
    private readonly Dictionary<string, string> _previous;
    private readonly string? _binding;
    private readonly string? _previousSummary;
    private SchemaSummary _summary;
    private readonly string? _apiBaselineError;
    private bool _apiAccepted;
    public IReadOnlyList<string> ApiChanges { get; }
    public string? ApiChangeId { get; }
    private readonly ApiMigrationRequest? _migrationRequest;
    private readonly GeneratedApiContract? _migrationPrevious;
    public ApiMigrationRequest? MigrationRequest => _migrationRequest is {} request
        ? request with { Languages=request.Languages.ToArray(), Changes=request.Changes.ToArray() } : null;
    public bool HasApiChanges => ApiChanges.Count != 0 || _apiBaselineError != null;
    private string SummaryPath => Path.Combine(_root, ".schema.json");
    public IReadOnlyList<string> SchemaChanges
    {
        get;
    }
    private readonly ConfigSourceLayout.SupportPlan _support;
    private readonly GenerationInputSnapshot _inputs;
    private string BindingPath => Path.Combine(_root, ".generation.json");
    private string OutputPath => Path.Combine(_root, "Generated");
    public ConfigTarget Target
    {
        get;
    }
    public string InputDirectory
    {
        get;
    }
    public string? PreviousInputDirectory
    {
        get;
    }
    public IReadOnlyList<string> RemovedFiles
    {
        get;
    }
    public IReadOnlyList<string> ChangedFiles
    {
        get;
    }
    private string BindingContent => JsonSerializer.Serialize(new GenerationBinding(2, Path.GetRelativePath(_root, InputDirectory), 3)) + "\n";
    public bool HasChanges => RequiresConfirmation || ChangedFiles.Count != 0 || _support.Replacements.Count != 0 || _previousSummary != _summary.Serialize() || _binding != BindingContent;
    public bool InputChanged => PreviousInputDirectory != null && !string.Equals(PreviousInputDirectory,
        InputDirectory,
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    public bool RequiresConfirmation => InputChanged || RemovedFiles.Count != 0 || HasApiChanges;

    internal ModelGenerationPlan(string input,
        string root,
        Dictionary<string, string> files,
        ConfigSourceLayout.SupportPlan support,
        ConfigTarget target,
        GenerationInputSnapshot inputs,
        SchemaSummary summary, GeneratedSymbolPlan names, string[] languages, System.Reflection.Assembly models, System.Reflection.Assembly candidate)
    {
        Target = target;
        _inputs = inputs;
        InputDirectory = input;
        _root = root;
        _summary = summary;
        _previousSummary = File.Exists(SummaryPath) ? File.ReadAllText(SummaryPath) : null;
        SchemaChanges = summary.Compare(_previousSummary);
        _support = support;
        _files = files.ToDictionary(x => x.Key, x => x.Value.ReplaceLineEndings("\n"), StringComparer.Ordinal);
        _previous = ReadGenerated();
        try
        {
            GeneratedApiContract next = GeneratedApiContracts.Capture(ConfigTargets.Marker(target), _files);
            GeneratedApiContract? previous = null;
            ApiMigrationReceipt? lastMigration = null;
            ApiMigrationRevision[]? migrationHistory = null;
            if (_previousSummary != null)
            {
                SchemaSummary saved = JsonSerializer.Deserialize<SchemaSummary>(_previousSummary) ?? throw new InvalidDataException("结构/API 基线为空。");
                if (!saved.SupportedFormat || saved.Tables == null)
                    throw new InvalidDataException("结构/API 基线格式不受支持。");
                previous = saved.ApiContract;
                lastMigration = saved.LastMigration;
                migrationHistory = saved.ApiMigrationHistory;
                ApiReleaseVerifier.ValidateHistory(saved);
                if (previous != null)
                {
                    GeneratedApiContracts.Validate(previous);
                    if (previous.Target != saved.Target)
                        throw new InvalidDataException("结构与 API 基线的目标端不一致。");
                    GeneratedApiContract actualSurface = GeneratedApiContracts.Capture(saved.Target, _previous);
                    GeneratedApiContract actual = actualSurface with { NativeSymbols=previous.NativeSymbols==null?null:actualSurface.NativeSymbols, Policy = previous.Policy, Format = previous.Format, CompiledSymbols = previous.CompiledSymbols,
                        DescriptorDigest = previous.DescriptorDigest, AcceptedParentDigest = previous.AcceptedParentDigest };
                    if (GeneratedApiContracts.Compare(previous, actual).Length != 0 || GeneratedApiContracts.Compare(actual, previous).Length != 0)
                        throw new InvalidDataException("生成源码与已接受 API 基线不一致，请先恢复或核对生成文件。");
                }
                else
                {
                    if (_previous.Values.Any(s => s.Contains(GeneratedApiContracts.Marker, StringComparison.Ordinal)))
                        throw new InvalidDataException("已启用 API 保护的项目缺少契约记录，请从版本控制恢复 .schema.json。");
                    // Import the actual legacy output, never the newly generated candidate.
                    previous = GeneratedApiContracts.Capture(saved.Target, _previous);
                }
            }
            else if (_previous.Count != 0)
                throw new InvalidDataException("已有生成源码但缺少 .schema.json，不能按新项目重置 API 基线。");
            next = next with { Format = 2, Policy = GeneratedApiPolicies.Create(names, languages, models, previous?.Policy, previous == null ? null : _previousSummary),
                CompiledSymbols = GeneratedApiPolicies.Surface(candidate.GetExportedTypes().Where(t=>t.Namespace == names.Namespaces.ModelRoot || t.Namespace == names.Namespaces.ModelRoot+".Server")) };
            next = GeneratedApiContracts.Seal(next,previous);
            GeneratedApiContracts.Validate(next);
            ApiChanges = Array.AsReadOnly(previous == null ? [] : GeneratedApiContracts.Compare(previous, next));
            if (ApiChanges.Count != 0)
            {
                int major = Version.Parse(previous?.Policy?.ContractVersion ?? "1.0").Major;
                next = GeneratedApiContracts.Seal(next with { Policy = next.Policy! with { ContractVersion = checked(major+1)+".0" } }, previous);
                ApiChanges = Array.AsReadOnly(GeneratedApiContracts.Compare(previous!, next));
            }
            _summary = summary with { ApiContract = next, LastMigration = lastMigration, ApiMigrationHistory = migrationHistory };
            if (ApiChanges.Count != 0)
            {
                _migrationPrevious = previous;
                string inputDigest = _inputs.MigrationDigest();
                string tool = GeneratedApiContracts.Hash(typeof(ModelGenerationPlan).Assembly.ManifestModule.ModuleVersionId + "\n" + typeof(ZrConfig.Runtime.ConfigManager).Assembly.ManifestModule.ModuleVersionId);
                ApiChangeId = GeneratedApiContracts.Hash((_previousSummary ?? "") + "\n" + tool + "\n" + inputDigest + "\n" + _summary.Serialize() + "\n" + string.Join("\n", _files.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => x.Key + ":" + GeneratedApiContracts.Hash(x.Value))));
                _migrationRequest = new(1,ApiChangeId,previous!.DescriptorDigest ?? GeneratedApiContracts.Digest(previous),next.DescriptorDigest!,inputDigest,tool,ConfigTargets.Marker(target),
                    next.Policy!.Family,next.Policy.ContractVersion,next.Policy.Profiles.Select(p=>p.Language).Order(StringComparer.Ordinal).ToArray(),ApiChanges.ToArray());
            }
        }
        catch (Exception error) when (error is InvalidDataException or JsonException or ArgumentException)
        {
            _apiBaselineError = error.Message;
            ApiChanges = Array.Empty<string>();
        }
        ChangedFiles = Array.AsReadOnly(_files.Where(x => !_previous.TryGetValue(x.Key,
            out string? text) || text.ReplaceLineEndings("\n") != x.Value)
            .Select(x => x.Key)
            .Order(StringComparer.Ordinal)
            .ToArray());
        RemovedFiles = Array.AsReadOnly(_previous.Keys
            .Where(name => !_files.ContainsKey(name.EndsWith(".meta", StringComparison.Ordinal) ? name[..^5] : name))
            .Order(StringComparer.Ordinal).ToArray());
        _binding = File.Exists(BindingPath) ? File.ReadAllText(BindingPath) : null;
        if (_binding != null)
        {
            using var document = JsonDocument.Parse(_binding);
            string relative;
            if (document.RootElement.ValueKind == JsonValueKind.String)
                relative = document.RootElement.GetString()!;
            else
            {
                GenerationBinding saved = JsonSerializer.Deserialize<GenerationBinding>(_binding) ?? throw new InvalidDataException("生成目录绑定为空。");
                if (saved.Format != 2 || saved.RequiredApiDescriptor != 3 || string.IsNullOrWhiteSpace(saved.InputRoot))
                    throw new InvalidDataException("生成目录绑定需要受支持的 API 契约工具版本，请恢复 .generation.json。");
                relative = saved.InputRoot;
            }
            PreviousInputDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(relative, root));
        }
    }

    public string DescribeChanges()
    {
        var text = new StringBuilder();
        text.AppendLine(ConfigText.Get($"目标端：{ConfigTargets.Marker(Target)}", $"Target: {ConfigTargets.Marker(Target)}"));
        text.AppendLine(ConfigText.Get($"本次完整输入目录：{InputDirectory}", $"Complete input directory: {InputDirectory}"));
        if (InputChanged)
        {
            text.AppendLine(ConfigText.Get($"上次输入目录：{PreviousInputDirectory}", $"Previous input directory: {PreviousInputDirectory}"));
        }

        foreach (string change in SchemaChanges)
        {
            text.AppendLine("  " + change);
        }

        if (_apiBaselineError != null)
            text.AppendLine("API_BASELINE_INVALID: " + _apiBaselineError);
        foreach (string change in ApiChanges)
            text.AppendLine("API: " + change);
        if (ApiChangeId != null)
            text.AppendLine("公开 API 变更需单独审阅及消费方验收；普通 --accept-changes 不放行。使用 codegen --verify-api <checks.json> --migration-receipt <receipt.json> 生成精确凭据；审阅后使用 codegen --accept-api-change " + ApiChangeId + " --migration-receipt <receipt.json>。");

        if (ChangedFiles.Count != 0)
        {
            text.AppendLine(ConfigText.Get("将新增或更新以下生成文件：", "The following generated files will be added or updated:"));
            foreach (string file in ChangedFiles)
            {
                text.AppendLine("  " + file);
            }
        }

        if (_support.Replacements.Count != 0)
        {
            text.AppendLine(ConfigText.Get("将补齐或升级以下支持源码：", "The following shared support will be installed or updated:"));
            foreach (string file in _support.Replacements.Keys)
            {
                text.AppendLine("  " + Path.GetRelativePath(_root, file));
            }
        }

        if (RemovedFiles.Count != 0)
        {
            text.AppendLine(ConfigText.Get("将删除以下生成文件：", "The following generated files will be removed:"));
            foreach (string file in RemovedFiles)
            {
                text.AppendLine("  " + file);
            }
        }

        return text.ToString().TrimEnd();
    }

    public bool Apply(DiagnosticCollector diagnostics, bool acceptChanges = false) => Apply(diagnostics, acceptChanges, null);

    /// <summary>Accept only this exact API migration. Apply still verifies all inputs and the old baseline under the write lock.</summary>
    public bool AcceptApiChanges(string changeId)
    {
        if (!_apiAccepted || ApiChangeId == null || _apiBaselineError != null || !string.Equals(ApiChangeId, changeId, StringComparison.Ordinal))
            return false;
        return true;
    }

    public bool AcceptApiMigration(string changeId, ApiMigrationReceipt receipt)
    {
        if (_migrationRequest == null || receipt == null || changeId != ApiChangeId || _apiBaselineError != null || !receipt.Matches(_migrationRequest)) return false;
        if (_apiAccepted) return true;
        var accepted = receipt with { Request=MigrationRequest!, Consumers=receipt.Consumers.ToArray() };
        var revision = new ApiMigrationRevision(_migrationPrevious!, _summary.ApiContract!, accepted);
        _summary = _summary with { LastMigration = accepted, ApiMigrationHistory = [.._summary.ApiMigrationHistory ?? [], revision] };
        _apiAccepted = true;
        return true;
    }

    internal void RequireMigrationCurrent()
    {
        if (_migrationRequest == null || !_inputs.IsCurrent() || _migrationRequest.InputDigest != _inputs.MigrationDigest() ||
            _previousSummary != (File.Exists(SummaryPath)?File.ReadAllText(SummaryPath):null))
            throw new IOException("Migration inputs or baseline changed; repeat preview and consumer verification.");
    }

    internal void WriteVerificationCandidate(string destination)
    {
        RequireMigrationCurrent();
        foreach (string source in RuleCompiler.EnumerateSources(_root,OutputPath))
        {
            string path = Path.Combine(destination,Path.GetRelativePath(_root,source));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.Copy(source,path);
        }
        foreach (var replacement in _support.Replacements)
        {
            string path = Path.Combine(destination,Path.GetRelativePath(_root,replacement.Key));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path,replacement.Value);
        }
        foreach (var file in _files)
        {
            string path = Path.Combine(destination,"Generated",file.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path,file.Value);
        }
        File.WriteAllText(Path.Combine(destination,".schema.json"),_summary.Serialize());
    }

    internal bool Apply(DiagnosticCollector diagnostics, bool acceptChanges, GenerationFileOperations? operations)
    {
        if (_apiBaselineError != null || ApiChanges.Count != 0 && !_apiAccepted)
        {
            diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, _apiBaselineError != null ? "API_BASELINE_INVALID" : "API_CHANGE_REQUIRED",
                DescribeChanges(), _root));
            return false;
        }
        if ((InputChanged || RemovedFiles.Count != 0) && !acceptChanges)
        {
            diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error,
                "GENERATION_CONFIRM_REQUIRED",
                DescribeChanges() + Environment.NewLine + ConfigText.Get("生成已停止，项目文件保持不变。请检查是否选错子目录；确需调整时，在 App 确认清单，或核对后给 codegen 添加 --accept-changes。",
                "Generation stopped; project files are unchanged. Check for an incorrectly selected subdirectory. Confirm the list in the App, or review it before adding --accept-changes to codegen."),
                _root));
            return false;
        }

        try
        {
            using IDisposable lease = ConfigSourceLayout.AcquireWriteLocks(_root);
            Dictionary<string, string> current = ReadGenerated();
            if (!_support.IsCurrent || !_inputs.IsCurrent() || _apiAccepted && _migrationRequest!.InputDigest != _inputs.MigrationDigest() ||
                _previousSummary != (File.Exists(SummaryPath) ? File.ReadAllText(SummaryPath) : null) ||
                _binding != (File.Exists(BindingPath) ? File.ReadAllText(BindingPath) : null) ||
                current.Count != _previous.Count || current.Any(x => !_previous.TryGetValue(x.Key, out string? content) || content != x.Value))
            {
                return Changed(diagnostics);
            }

            var transaction = new GenerationFileTransaction(_root, operations);
            _support.AddChanges(transaction);
            foreach ((string name, string content) in _files)
            {
                if (name.Contains('/') && !_previous.ContainsKey(name) && File.Exists(Path.Combine(OutputPath, name)))
                    return Changed(diagnostics, "目标路径已有未登记文件，不能覆盖：" + name);
                if (!_previous.TryGetValue(name, out string? previous) || previous != content)
                {
                    transaction.Set(Path.Combine(OutputPath, name), content);
                }
            }

            foreach (string file in RemovedFiles)
            {
                transaction.Set(Path.Combine(OutputPath, file), null);
            }

            transaction.Set(SummaryPath, _summary.Serialize());
            // Older generators deserialize this file as a JSON string. A versioned object
            // makes them stop before their write transaction, even if they ignore ApiContract.
            transaction.Set(BindingPath, BindingContent);
            if (!transaction.Commit(diagnostics))
            {
                return false;
            }

            _support.ReportUpdates(diagnostics);
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidDataException or JsonException or FormatException)
        {
            return Changed(diagnostics, error.Message);
        }
    }

    private bool Changed(DiagnosticCollector diagnostics, string? detail = null)
    {
        diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "GENERATION_CHANGED",
            ConfigText.Get("生成输入或输出已变化或暂时无法访问，请重新生成并核对清单。",
                "Generation inputs or outputs changed or are inaccessible. Prepare and review a new generation plan.") +
            (detail == null ? "" : Environment.NewLine + detail), _root));
        return false;
    }

    private Dictionary<string, string> ReadGenerated()
    {
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!Directory.Exists(OutputPath))
        {
            return files;
        }

        foreach (string path in Directory.EnumerateFiles(OutputPath, "*.g.cs"))
        {
            files.Add(Path.GetFileName(path), File.ReadAllText(path));
            // 保留仍存在的脚本 GUID；退役脚本的 .meta 随源码预览、复核、删除及回滚。
            // 不扫描或清理没有对应生成源码的其他 .meta。
            string metadata = path + ".meta";
            if (File.Exists(metadata))
            {
                files.Add(Path.GetFileName(metadata), File.ReadAllText(metadata));
            }
        }

        string nativeManifest = Path.Combine(OutputPath, ".native-files.json");
        if (File.Exists(nativeManifest))
        {
            string contents = File.ReadAllText(nativeManifest);
            files.Add(".native-files.json", contents);
            foreach (string name in JsonSerializer.Deserialize<string[]>(contents) ?? [])
            {
                if (name.Contains('\\') || name.Split('/').Any(s => s is "" or "." or "..") ||
                    !(name.StartsWith("Java/", StringComparison.Ordinal) || name.StartsWith("Go/", StringComparison.Ordinal) || name.StartsWith("Cpp/", StringComparison.Ordinal) || name.StartsWith("Lua/", StringComparison.Ordinal) || name.StartsWith("TypeScript/", StringComparison.Ordinal) || name.StartsWith("Rust/", StringComparison.Ordinal)))
                    throw new InvalidDataException("生成文件清单路径无效：" + name);
                string path = Path.Combine(OutputPath, name);
                ReleaseHistory.RejectLinks(path);
                if (File.Exists(path))
                {
                    files.Add(name, File.ReadAllText(path));
                    if (File.Exists(path + ".meta"))
                        files.Add(name + ".meta", File.ReadAllText(path + ".meta"));
                }
            }
        }
        return files;
    }
}
