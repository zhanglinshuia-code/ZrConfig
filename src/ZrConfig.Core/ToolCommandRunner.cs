using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;
/// <summary>CLI generation, support updates and export, including help and exit codes.</summary>
public static class ToolCommandRunner
{
    public static int Run(string[] args, TextWriter? output = null, TextWriter? error = null, CancellationToken cancellationToken = default)
    {
        output ??= Console.Out;
        error ??= Console.Error;
        try
        {
            if (args.Length != 0 && args[0].Equals("release", StringComparison.OrdinalIgnoreCase))
            {
                return ReleaseCommand.Run(args.Skip(1).ToArray(), output);
            }

            var operation = new WorkflowOperation(cancellationToken, new CallbackProgress<OperationProgress>(p =>
            {
                if (p.Diagnostic == null)
                {
                    output.WriteLine($"[{p.Elapsed:hh\\:mm\\:ss}] {p.Stage}: {p.Message}" + (p.Total.HasValue ? $" ({p.Completed}/{p.Total})" : ""));
                }
            }));
            var options = ToolCommandLine.Parse(args);
            operation.ApiChangeId = options.ApiChangeId;
            if(options.SourceMappingsFile!=null)
            {
                operation.SourceMappings=System.Text.Json.JsonSerializer.Deserialize<SourceNameMapping[]>(File.ReadAllText(options.SourceMappingsFile))??throw new InvalidDataException("Empty source mapping file.");
                GenerationSourceContract.ValidateMappings(operation.SourceMappings);
            }
            if (options.ApiChangeId != null && options.MigrationReceiptFile != null)
                operation.MigrationReceipt = System.Text.Json.JsonSerializer.Deserialize<ApiMigrationReceipt>(File.ReadAllText(options.MigrationReceiptFile));
            if (options.Help)
            {
                Help(output);
                return 0;
            }

            WorkspaceSettings? project = options.ProjectFile == null ? null : WorkspaceSettings.LoadProject(options.ProjectFile);
            if (options.Command == "support")
            {
                var diagnostics = new DiagnosticCollector();
                ConfigSourceLayout.Initialize(project?.CodeFolder ?? Path.GetFullPath(options.Paths[0]), diagnostics);
                foreach (Diagnostic diagnostic in diagnostics.Snapshot())
                {
                    output.WriteLine(diagnostic);
                }

                output.WriteLine(ConfigText.Get("支持源码检查完成；如有合并提示，请处理后再导出。",
                    "Support check complete. Resolve any merge warnings before exporting."));
                return diagnostics.Snapshot().Any(d => d.Severity == DiagnosticSeverity.Error) ? 1 : 0;
            }

            ConfigTarget target = options.Target ?? project?.Target ?? ConfigTarget.Client;
            string dataFormat = options.DataFormat ?? project?.DataFormat ?? "binary";
            string runtimeFamily = options.RuntimeFamily ?? project?.RuntimeFamily ?? "auto";
            string[] languages = options.CodeLanguages ?? project?.CodeLanguages ?? ["csharp"];
            string data = project?.DataFolder ?? Path.GetFullPath(options.PathAt(0, Path.Combine(ToolPaths.DefaultBaseDirectory, "Data")));
            string code = project?.CodeFolder ?? options.PathAt(options.Command is "codegen" or "validate" ? 1 : 2, ToolPaths.DefaultGeneratedDirectory);
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException(ConfigText.Get("请指定 Config 根目录或 --project 项目配置；省略全部目录仅适用于仓库示例。",
                    "Specify the Config root or --project settings; omitting all directories is only supported for repository samples."));
            }

            code = Path.GetFullPath(code);
            Task<bool> ConfirmGenerationAsync(ModelGenerationPlan plan)
            {
                output.WriteLine(plan.DescribeChanges());
                // Apply reports the dedicated API diagnostic; ordinary confirmation cannot approve it.
                if (plan.HasApiChanges && options.ApiChangeId == null ||
                    !plan.InputChanged && plan.RemovedFiles.Count == 0 || options.AcceptChanges)
                {
                    return Task.FromResult(true);
                }

                output.WriteLine(new Diagnostic(DiagnosticSeverity.Error,
                    "GENERATION_CONFIRM_REQUIRED",
                    ConfigText.Get("操作已停止，项目文件和当前发布版本保持不变；核对上述清单后添加 --accept-changes 重试。",
                    "Stopped; project files and the current version are unchanged. Review the list, then retry with --accept-changes."),
                    code));
                return Task.FromResult(false);
            }

            if (options.Command == "codegen")
            {
                if (options.ApiVerificationFile != null)
                {
                    var verificationDiagnostics = new DiagnosticCollector();
                    var candidate = ModelGenerator.PrepareWithNamespaces(project?.Namespaces,runtimeFamily,data,code,verificationDiagnostics,target:target,operation:operation,languages:languages,dataFormat:dataFormat);
                    foreach (var diagnostic in verificationDiagnostics.Snapshot()) output.WriteLine(diagnostic);
                    if (candidate?.MigrationRequest == null) { if(candidate!=null)output.WriteLine(candidate.DescribeChanges());return 1; }
                    string receiptPath = Path.GetFullPath(options.MigrationReceiptFile!);
                    // Review evidence is external to Config and Data; normal generation never creates or overwrites it.
                    if (Path.GetRelativePath(code,receiptPath) is var relative && !relative.StartsWith(".."+Path.DirectorySeparatorChar,StringComparison.Ordinal) && !Path.IsPathRooted(relative))
                        throw new ArgumentException("Migration receipts must be stored outside Config.");
                    ApiMigrationReceipt receipt = ApiMigrationVerifier.VerifyAsync(candidate,options.ApiVerificationFile,operation.CancellationToken).GetAwaiter().GetResult();
                    using var receiptFile = new FileStream(receiptPath,FileMode.CreateNew,FileAccess.Write,FileShare.None);
                    System.Text.Json.JsonSerializer.Serialize(receiptFile,receipt,new System.Text.Json.JsonSerializerOptions{WriteIndented=true});
                    output.WriteLine("消费方验收已完成，项目文件未写入。审阅迁移凭据："+receiptPath);return 0;
                }
                // 生成不使用输出/规则目录；只校验这两个实际参与生成的路径。
                var generationSettings = new WorkspaceSettings(data, "", code, "")
                {
                    Target = target, DataFormat = dataFormat, CodeLanguages = languages, RuntimeFamily = runtimeFamily, Namespaces=project?.Namespaces
                };
                ProjectWorkflowResult generation = Task.Run(() => ProjectWorkflow.GenerateAsync(generationSettings,
                    ConfirmGenerationAsync, operation: operation))
                    .GetAwaiter()
                    .GetResult();
                foreach (Diagnostic diagnostic in generation.PreparationDiagnostics)
                {
                    output.WriteLine(diagnostic);
                }

                output.WriteLine(generation.Success ? ConfigText.Get("代码生成成功，可直接导出；Unity 端需编译对应脚本。",
                    "Generated successfully; export is ready. Compile the matching scripts in Unity.") : ConfigText.Get("代码生成失败，请查看诊断。",
                    "Generation failed; review the diagnostics."));
                return generation.Success ? 0 : 1;
            }

            if (options.Command == "validate")
            {
                string validationRules = project?.RulesFolder ?? options.PathAt(2, options.Paths.Length >= 2 ? "" : ToolPaths.DefaultRulesDirectory);
                var validationSettings = new WorkspaceSettings(data, "", code, validationRules) { Target = target, DataFormat = dataFormat, CodeLanguages = languages, RuntimeFamily = runtimeFamily, Namespaces=project?.Namespaces };
                ProjectWorkflowResult validation = Task.Run(() => ProjectWorkflow.ValidateAsync(validationSettings, options.RulesDebug, operation: operation)).GetAwaiter().GetResult();
                foreach (Diagnostic diagnostic in validation.PreparationDiagnostics.Concat(validation.Export?.Diagnostics ?? []))
                {
                    output.WriteLine(diagnostic);
                }

                if (validation.NeedsGeneration)
                {
                    output.WriteLine(validation.Plan!.DescribeChanges());
                }

                if (options.ReportFile != null)
                {
                    OperationReport.Save(options.ReportFile, validation, validationSettings, "validate");
                }

                output.WriteLine(validation.Success ? ConfigText.Get("校验通过，未发布数据。", "Validation passed; no data was published.") : ConfigText.Get("校验未完成或未通过，未发布数据。", "Validation is incomplete or failed; no data was published."));
                return validation.Cancelled ? 130 : validation.NeedsGeneration ? 3 : validation.Success ? 0 : 1;
            }

            string destination = project?.OutputFolder ?? Path.GetFullPath(options.PathAt(1,
                Path.Combine(ToolPaths.DefaultBaseDirectory,
                "Output")));
            string rules = project?.RulesFolder ?? options.PathAt(3, options.Paths.Length > 2 ? "" : ToolPaths.DefaultRulesDirectory);
            ExportResult result;
            if (options.Generate)
            {
                var settings = new WorkspaceSettings(data, destination, code, rules)
                {
                    Target = target, DataFormat = dataFormat, CodeLanguages = languages, RuntimeFamily = runtimeFamily, Namespaces=project?.Namespaces
                };
                ProjectWorkflowResult workflow = Task.Run(() => ProjectWorkflow.ExportAsync(settings,
                    ConfirmGenerationAsync,
                    options.RulesDebug, operation: operation))
                    .GetAwaiter()
                    .GetResult();
                foreach (Diagnostic diagnostic in workflow.PreparationDiagnostics)
                {
                    output.WriteLine(diagnostic);
                }

                if (workflow.Export == null)
                {
                    output.WriteLine(ConfigText.Get("未执行导出，请查看生成检查结果。", "Export was not run; review the generation checks."));
                    return 1;
                }

                result = workflow.Export;
            }
            else
            {
                result = ExportPipeline.RunWithSources(data, destination, code, rules, options.RulesDebug, target, operation, dataFormat);
            }

            foreach (Diagnostic diagnostic in result.Diagnostics)
            {
                output.WriteLine(diagnostic);
            }

            output.WriteLine(ConfigText.Get($"成功={ConfigText.Boolean(result.Success)}；错误={result.Errors}；警告={result.Warnings}",
                $"Success={result.Success}; Errors={result.Errors}; Warnings={result.Warnings}"));
            output.WriteLine(ConfigText.Get($"日志：{result.LogFile}", $"Log: {result.LogFile}"));
            output.WriteLine(ConfigText.Get($"诊断报告：{result.DiagnosticFile}", $"Diagnostics: {result.DiagnosticFile}"));
            if (result.VersionId != null)
            {
                output.WriteLine(ConfigText.Get($"当前版本：{result.VersionId}", $"Active version: {result.VersionId}"));
            }

            if (options.ReportFile != null)
            {
                OperationReport.Save(options.ReportFile, new ProjectWorkflowResult(result.Success, result.Cancelled, null, [], result),
                    new WorkspaceSettings(data, destination, code, rules) { Target = target, DataFormat = dataFormat, CodeLanguages = languages, RuntimeFamily = runtimeFamily, Namespaces=project?.Namespaces }, "export");
            }

            return result.Cancelled ? 130 : result.Success ? 0 : 1;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            output.WriteLine(ConfigText.Get("操作已取消。", "Operation cancelled."));
            return 130;
        }
        catch (ArgumentException ex)
        {
            error.WriteLine(ex.Message);
            Help(output);
            return 2;
        }
        catch (Exception ex)
        {
            error.WriteLine(ConfigText.Get("执行失败。异常详情：", "Command failed. Exception details:"));
            error.WriteLine(ex);
            return 2;
        }
    }

    private static void Help(TextWriter output)
    {
        output.WriteLine(ConfigText.Get("ZrConfig 命令行工具", "ZrConfig CLI"));
        output.WriteLine("  validate <Data> <ConfigRoot> [Rules] [--target C|S] [--report report.json]");
        output.WriteLine("  validate --project <project.zrconfig.json> [--target C|S] [--report report.json]");
        output.WriteLine("  release --help");
        output.WriteLine("  --languages csharp,java,go,cpp,lua,typescript,rust | all    目标语言，与 C/S 独立选择");
        output.WriteLine("  --source-map <JSON>                                   codegen 显式绑定源表/字段新名；须经过精确迁移验收");
        output.WriteLine("  --runtime-family auto | legacy-v1 | server-v1           auto 保留已有项目家族；server-v1 增加服务器入口");
        output.WriteLine("  --format binary|json|both             正式数据格式，默认 binary");
        output.WriteLine("  codegen <Data> <ConfigRoot> [--target C|S] [--accept-changes]");
        output.WriteLine("  codegen --project <project.zrconfig.json> [--target C|S] [--accept-changes]");
        output.WriteLine("  codegen ... --verify-api <checks.json> --migration-receipt <新文件>    在临时目录验证消费方并生成迁移凭据");
        output.WriteLine("  codegen ... --accept-api-change <完整摘要> --migration-receipt <凭据>    仅批准已验收的精确迁移");
        output.WriteLine("  --accept-changes 不批准 API 破坏；接口迁移须先 codegen 审阅并提交，再 export。");
        output.WriteLine("  support <ConfigRoot> | support --project <project.zrconfig.json>");
        output.WriteLine("  export <Data> <Output> <ConfigRoot> [Rules] [--generate [--accept-changes]] [--rules-debug] [--target C|S]");
        output.WriteLine("  export --project <project.zrconfig.json> [--generate [--accept-changes]] [--rules-debug] [--target C|S]");
        output.WriteLine(ConfigText.Get("--target C 生成/导出 C、CS 字段；--target S 生成/导出 S、CS 字段。省略时使用项目 Target，默认 C。",
            "--target C selects C/CS fields; --target S selects S/CS fields. Defaults to the project's Target, or C."));
        output.WriteLine(ConfigText.Get("所有表固定五行表头；第 5 行必须保留，每个字段明确声明 C/S/CS，缺失标记报错，第 6 行起数据。主键必须 CS。无需配置 headerRows。",
            "All tables require five header rows. Explicitly declare C/S/CS in row 5 (missing markers are errors), and start data in row 6. Keys must be shared. No headerRows setting is needed."));
        output.WriteLine(ConfigText.Get("--generate 先检查并更新必要的代码，再导出；改变输入根目录或删除生成文件仍需 --accept-changes。不带 --generate 时只导出，不改源码。",
            "--generate checks and updates code before export; input-root changes or generated-file removals require --accept-changes. Without --generate, export does not write source code."));
        output.WriteLine(ConfigText.Get("App 与命令行共用项目配置；相对目录以配置文件所在目录为基准。--project 不能与目录参数混用。",
            "The App and command line share project settings; relative directories resolve from the settings file. Do not mix --project with positional directories."));
        output.WriteLine(ConfigText.Get("codegen/export 省略全部目录时使用仓库示例；不能只省略部分必填目录。",
            "codegen/export with no directories use repository samples; required directories cannot be partially omitted."));
        output.WriteLine(ConfigText.Get("输入范围变化或删除旧文件时生成会停止；核对清单后再加 --accept-changes。预览不会修改项目文件。",
            "Generation stops on input-root changes or removals; review before adding --accept-changes. Preview does not modify project files."));
        output.WriteLine(ConfigText.Get("support 升级未修改的支持源码，保留手写修改并提示合并。",
            "support updates unmodified support and preserves custom edits with merge warnings."));
        output.WriteLine(ConfigText.Get("validate 不写 Config/Output，不自动生成；结构过期返回 3，校验失败返回 1，取消返回 130。--report 主动保存去除项目根路径的诊断报告。Ctrl+C 请求安全取消。",
            "validate never writes Config/Output or generates code; stale code returns 3, failed checks 1, cancellation 130. --report explicitly saves a report with project roots redacted. Ctrl+C requests safe cancellation."));
        output.WriteLine(ConfigText.Get("未知、多余和重复参数会报错；不支持 --dry-run。目录以 - 开头时先加 --。",
            "Unknown, extra and duplicate arguments are rejected; --dry-run is not supported. Use -- before directories beginning with -."));
        output.WriteLine(ConfigText.Get("Excel 输入递归读取 .xlsx/.xls：第 1 行字段名，第 2 行类型，第 3 行主键，第 4 行说明，第 5 行所属端（明确声明 C/S/CS），第 6 行起数据。",
            "Excel input recursively reads .xlsx/.xls: row 1 names, row 2 types, row 3 keys, row 4 notes, row 5 targets (explicit C/S/CS), row 6+ data."));
        output.WriteLine(ConfigText.Get("只支持五行表头；第 5 行不能省略，字段缺少所属端会报错，主键缺失不会自动切换格式。",
            "Only five header rows are supported. Row 5 must be reserved and fields must explicitly declare their targets; missing key markers never switch layouts automatically."));
    }
}
