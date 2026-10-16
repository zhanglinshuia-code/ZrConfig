using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Serilog;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

public sealed record ExportResult(bool Success, string RunId, string LogFile, string DiagnosticFile,
    string? VersionId, IReadOnlyList<Diagnostic> Diagnostics)
{
    public int Errors => Diagnostics.Count(x => x.Severity == DiagnosticSeverity.Error);
    public int Warnings => Diagnostics.Count(x => x.Severity == DiagnosticSeverity.Warning);
    public bool Cancelled
    {
        get; init;
    }
    public bool ValidationOnly
    {
        get; init;
    }
    public bool Published => Success && !ValidationOnly && VersionId != null;
    public string? InputFingerprint
    {
        get; init;
    }
    public TimeSpan Elapsed
    {
        get; init;
    }
    public string LogText { get; init; } = "";
    public string? RunDirectory
    {
        get; init;
    }
}

public static class ExportPipeline
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
    };

    public static ExportResult Run(string dataFolder, string outputFolder, Assembly modelAssembly,
        params Assembly[] validatorAssemblies) => Run(dataFolder, outputFolder, modelAssembly, ConfigTarget.Client, validatorAssemblies);

    public static ExportResult Run(string dataFolder, string outputFolder, Assembly modelAssembly, ConfigTarget target,
        params Assembly[] validatorAssemblies) => RunCore(dataFolder, outputFolder, modelAssembly, null, null, false, validatorAssemblies, target);

    public static ExportResult Run(string dataFolder, string outputFolder, Assembly modelAssembly, ConfigTarget target,
        string dataFormat, params Assembly[] validatorAssemblies) =>
        RunCore(dataFolder, outputFolder, modelAssembly, null, null, false, validatorAssemblies, target, dataFormat: dataFormat);

    public static ExportResult RunWithRules(string dataFolder, string outputFolder, Assembly modelAssembly,
        string rulesFolder, bool debugRules = false, ConfigTarget target = ConfigTarget.Client, string dataFormat = "binary") =>
        RunCore(dataFolder, outputFolder, modelAssembly, rulesFolder, null, debugRules, [], target, dataFormat: dataFormat);

    public static ExportResult RunWithSources(string dataFolder, string outputFolder, string generatedFolder,
        string rulesFolder, bool debugRules = false, ConfigTarget target = ConfigTarget.Client, WorkflowOperation? operation = null, string dataFormat = "binary") =>
        RunCore(dataFolder, outputFolder, null, rulesFolder, generatedFolder, debugRules, [], target, operation, dataFormat: dataFormat);

    public static ExportResult ValidateWithSources(WorkspaceSettings settings, bool debugRules = false, WorkflowOperation? operation = null)
    {
        settings = settings.ValidateOnly();
        string temporary = Path.Combine(Path.GetTempPath(), "ZrConfig-validate-" + Guid.NewGuid().ToString("N"));
        try
        {
            return RunCore(settings.DataFolder, temporary, null, settings.RulesFolder, settings.CodeFolder,
                debugRules, [], settings.Target, operation, validationOnly: true, dataFormat: settings.DataFormat);
        }
        finally
        {
            if (Directory.Exists(temporary))
            {
                try
                {
                    Directory.Delete(temporary, recursive: true);
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                {
                    System.Diagnostics.Trace.TraceWarning("Validation temporary cleanup failed: {0}", error.Message);
                }
            }
        }
    }

    private static ExportResult RunCore(string dataFolder, string outputFolder, Assembly? modelAssembly,
        string? rulesFolder, string? generatedFolder, bool debugRules, Assembly[] validatorAssemblies,
        ConfigTarget target, WorkflowOperation? operation = null, bool validationOnly = false, string dataFormat = "binary")
    {
        _ = ConfigTargets.Marker(target);
        operation ??= new WorkflowOperation();
        var settings = new WorkspaceSettings(Path.GetFullPath(dataFolder), Path.GetFullPath(outputFolder),
            generatedFolder == null ? "" : Path.GetFullPath(generatedFolder),
            string.IsNullOrWhiteSpace(rulesFolder) ? "" : Path.GetFullPath(rulesFolder))
        {
            Target = target,
            DataFormat = ExportOptions.Format(dataFormat)
        };
        WorkspaceSettings.RequireSeparateDirectories(settings, includeOutput: true);
        string runId = operation.RunId;
        string logsFolder = Path.Combine(outputFolder, "Logs");
        using FileStream exportLease = ReleaseHistory.AcquireExportLock(outputFolder);
        ReleaseHistory.RejectLinks(logsFolder);
        Directory.CreateDirectory(logsFolder);
        string logFile = Path.Combine(logsFolder, "export.log");
        string diagnosticFile = Path.Combine(logsFolder, "diagnostics.json");
        string runDirectory = Path.Combine(logsFolder, runId);
        ReleaseHistory.RejectLinks(logFile);
        ReleaseHistory.RejectLinks(diagnosticFile);
        ReleaseHistory.RejectLinks(runDirectory);
        ReleaseHistory.RejectLinks(Path.Combine(outputFolder, ".rules-cache"));
        Directory.CreateDirectory(runDirectory);
        File.WriteAllText(logFile, string.Empty, new UTF8Encoding(false));
        var collector = new DiagnosticCollector();
        collector.Added += operation.ReportDiagnostic;
        bool success = false;
        bool published = false;
        bool cancelled = false;
        string? versionId = null;
        ProjectInputSnapshot? snapshot = null;
        using (Serilog.Core.Logger logger = new LoggerConfiguration().MinimumLevel.Information()
            .WriteTo.Async(sink => sink.File(new LocalizedLogFormatter(ConfigText.Language), logFile, encoding: new UTF8Encoding(false)), blockWhenFull: true)
            .CreateLogger())
        {
            try
            {
                operation.Report("preparing", ConfigText.Get("核验本轮输入", "Fingerprinting inputs"));
                snapshot = ProjectInputSnapshot.Capture(settings, operation.CancellationToken);
                logger.Information(ConfigText.Get("运行 {RunId} | Excel 目录：{DataFolder}", "Run {RunId} | Excel directory: {DataFolder}"), runId, dataFolder);
                logger.Information(ConfigText.Get("目标端：{Target}", "Target: {Target}"), ConfigTargets.Marker(target));
                Action<string> progress = message =>
                {
                    logger.Information("{Message:l}", message);
                    operation.Report("compiling", message);
                };
                operation.Report("compiling", ConfigText.Get("编译配置和规则", "Compiling models and rules"));
                using LoadedRules? compiledRules = generatedFolder != null ? RuleCompiler.PrepareSources(generatedFolder,
                    rulesFolder!, Path.Combine(outputFolder, ".rules-cache"), collector, progress, debugRules) :
                    rulesFolder == null ? null : RuleCompiler.Prepare(rulesFolder, Path.Combine(outputFolder, ".rules-cache"),
                        modelAssembly!, collector, progress, debugRules);
                operation.Checkpoint();
                Assembly? activeModels = generatedFolder == null ? modelAssembly : compiledRules?.Assembly;
                if (rulesFolder == null || compiledRules != null)
                {
                    var codecs = activeModels!.GetTypes()
                        .Where(type => !type.IsAbstract && !type.ContainsGenericParameters && typeof(ITableCodec).IsAssignableFrom(type))
                        .Select(type => (ITableCodec)Activator.CreateInstance(type)!).ToDictionary(codec => codec.TableName, StringComparer.Ordinal);
                    LoadResult data = ExcelDataLoader.Load(dataFolder, new ConfigTypeRegistry(activeModels, codecs.Values.Select(c => c.RowType)),
                        codecs, collector, out IReadOnlyList<TableSchema> schemas,
                        generatedFolder == null ? null : Path.Combine(generatedFolder, "Tables.json"), target, operation);
                    foreach (string missing in codecs.Keys.Except(schemas.Select(x => x.Name), StringComparer.Ordinal))
                    {
                        collector.Add(new Diagnostic(DiagnosticSeverity.Error, "EXCEL_TABLE_MISSING",
                            ConfigText.Get($"生成的模型“{missing}”没有对应的 Excel 表。请补齐 Excel，或重新生成配表代码。",
                                $"Generated model '{missing}' has no matching Excel table. Restore Excel or regenerate table code.")));
                    }

                    ValidationEngine.Validate(data, collector, operation, compiledRules == null ? validatorAssemblies : [compiledRules.Assembly]);
                    operation.Checkpoint();
                    if (!HasErrors(collector))
                    {
                        if (validationOnly)
                        {
                            string roundtrip = Path.Combine(outputFolder, "roundtrip");
                            Directory.CreateDirectory(roundtrip);
                            WriteTables(roundtrip, data.Tables, operation, settings.DataFormat, ExportOptions.NativeSources(settings.CodeFolder).Any());
                            snapshot.RequireCurrent(settings, operation.CancellationToken);
                            success = true;
                        }
                        else
                        {
                            versionId = Publish(settings, runId, data.Tables, snapshot, operation, collector,
                                candidate =>
                                {
                                    SaveDiagnostics(diagnosticFile, runId, true, candidate, SortedDiagnostics(collector));
                                    File.Copy(diagnosticFile, Path.Combine(runDirectory, "diagnostics.json"), overwrite: true);
                                });
                            // No cancellation checkpoints after the atomic activation.
                            success = published = true;
                            logger.Information(ConfigText.Get("已发布版本 {VersionId}，共 {TableCount} 张表。", "Published version {VersionId} with {TableCount} tables."), versionId, data.Tables.Count);
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (operation.CancellationToken.IsCancellationRequested)
            {
                cancelled = true;
                collector.Add(new Diagnostic(DiagnosticSeverity.Info, "OPERATION_CANCELLED",
                    ConfigText.Get("操作已取消，当前发布版本未切换。", "Operation cancelled; the active version was not switched.")));
            }
            catch (Exception ex)
            {
                collector.Add(new Diagnostic(DiagnosticSeverity.Error, "PIPELINE_EXCEPTION",
                    ConfigText.Get("处理失败。详情：", "Pipeline failed. Details:") + Environment.NewLine + ex));
                logger.Error(ex, ConfigText.Get("操作因异常而中止。", "Operation aborted by an unexpected error."));
            }

            Diagnostic[] diagnostics = SortedDiagnostics(collector);
            foreach (Diagnostic diagnostic in diagnostics)
            {
                ILogger scoped = logger.ForContext("DiagnosticSeverity", diagnostic.Severity.ToString());
                string template = ConfigText.Get("{Code} {File}:{Sheet} 第{Row}行第{Column}列 {Message}", "{Code} {File}:{Sheet}!R{Row}C{Column} {Message}");
                object[] values = [diagnostic.Code, diagnostic.File, diagnostic.Sheet, diagnostic.Row, diagnostic.Column, diagnostic.Message];
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    scoped.Error(template, values);
                }
                else if (diagnostic.Severity == DiagnosticSeverity.Warning)
                {
                    scoped.Warning(template, values);
                }
                else
                {
                    scoped.Information(template, values);
                }
            }

            logger.Information(ConfigText.Get("运行结束。成功={Success:l}；错误={Errors}；警告={Warnings}", "Run finished. Success={Success:l}; Errors={Errors}; Warnings={Warnings}"),
                ConfigText.Boolean(success), diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error), diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning));
            if (!published)
            {
                SaveDiagnostics(diagnosticFile, runId, success, null, diagnostics);
            }
        }

        // Post-commit bookkeeping must not report a successful publication as failed.
        try
        {
            File.Copy(logFile, Path.Combine(runDirectory, "export.log"), overwrite: true);
            File.Copy(diagnosticFile, Path.Combine(runDirectory, "diagnostics.json"), overwrite: true);
            ReleaseHistory.WriteJson(Path.Combine(runDirectory, "run.json"), new
            {
                format = 1,
                runId,
                operation = validationOnly ? "validate" : "export",
                success,
                cancelled,
                published,
                versionId,
                target = ConfigTargets.Marker(target),
                inputFingerprint = snapshot?.Fingerprint,
                elapsedMilliseconds = operation.Elapsed.TotalMilliseconds,
                finishedUtc = DateTime.UtcNow
            });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Warning, "RUN_ARCHIVE_FAILED", ex.Message, runDirectory));
        }

        string logText;
        try
        {
            logText = File.ReadAllText(logFile);
        }
        catch (IOException)
        {
            logText = "";
        }

        return new(success, runId, validationOnly ? "" : logFile, validationOnly ? "" : diagnosticFile, versionId, SortedDiagnostics(collector))
        {
            Cancelled = cancelled,
            ValidationOnly = validationOnly,
            InputFingerprint = snapshot?.Fingerprint,
            Elapsed = operation.Elapsed,
            LogText = logText,
            RunDirectory = validationOnly ? null : runDirectory
        };
    }

    private static bool HasErrors(DiagnosticCollector collector) => collector.Snapshot().Any(d => d.Severity == DiagnosticSeverity.Error);
    private static Diagnostic[] SortedDiagnostics(DiagnosticCollector collector) => collector.Snapshot()
        .OrderBy(d => d.File, StringComparer.Ordinal).ThenBy(d => d.Sheet, StringComparer.Ordinal).ThenBy(d => d.Row).ThenBy(d => d.Column).ThenBy(d => d.Code, StringComparer.Ordinal).ToArray();

    private static void SaveDiagnostics(string path, string runId, bool success, string? versionId, IReadOnlyList<Diagnostic> diagnostics)
    {
        using FileStream stream = new(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        JsonSerializer.Serialize(stream, new
        {
            runId,
            success,
            versionId,
            language = ConfigText.Language,
            diagnostics
        }, s_jsonOptions);
        stream.Flush(flushToDisk: true);
    }

    private static ReleaseTable[] WriteTables(string folder, IReadOnlyList<LoadedTable> tables, WorkflowOperation operation, string dataFormat, bool portable = false)
    {
        var entries = new List<ReleaseTable>(tables.Count);
        foreach (LoadedTable table in tables.OrderBy(t => t.Schema.Name, StringComparer.Ordinal))
        {
            operation.Report("writing", table.Schema.Name, entries.Count, tables.Count);
            bool jsonOnly = dataFormat == "json";
            string fileName = table.Schema.Name + (jsonOnly ? ".json" : ".bytes");
            string path = Path.Combine(folder, fileName);
            using (FileStream stream = new(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                if (jsonOnly)
                {
                    byte[] json = WriteJsonTable(table);
                    stream.Write(json);
                }
                else
                    table.Codec.Write(stream, table.Rows);
                stream.Flush(flushToDisk: true);
            }

            operation.Report("verifying", table.Schema.Name, entries.Count, tables.Count);
            using (FileStream stream = File.OpenRead(path))
            {
                using MemoryStream? decoded = jsonOnly ? new MemoryStream(new ConfigWire(PortableSchema.Create(table.Schema).Serialize()).JsonToBinary(File.ReadAllBytes(path)), false) : null;
                if (table.Codec.Read(decoded ?? (Stream)stream).Count != table.Rows.Count)
                {
                    throw new InvalidDataException(ConfigText.Get($"表 {table.Schema.Name} 导出后的回读验证失败。", $"Post-export roundtrip failed for {table.Schema.Name}."));
                }
            }

            operation.Checkpoint();
            using FileStream hashStream = File.OpenRead(path);
            string? jsonFile = null, jsonHash = null;
            if (dataFormat == "both")
            {
                jsonFile = table.Schema.Name + ".json";
                byte[] json = WriteJsonTable(table, File.ReadAllBytes(path));
                using var jsonStream = new FileStream(Path.Combine(folder, jsonFile), FileMode.CreateNew, FileAccess.Write, FileShare.None);
                jsonStream.Write(json);
                jsonStream.Flush(flushToDisk: true);
                jsonHash = Convert.ToHexString(SHA256.HashData(json));
            }
            else if (portable && dataFormat == "binary")
            {
                // 原生后端也依赖标准布局；纯二进制发布同样验证声明与真实转换器一致。
                _ = WriteJsonTable(table, File.ReadAllBytes(path));
            }
            entries.Add(new(table.Schema.Name, fileName, table.Rows.Count, table.Schema.SchemaHash,
                Convert.ToHexString(SHA256.HashData(hashStream)), jsonFile, jsonHash));
        }

        return entries.ToArray();
    }

    // 正式 JSON 从实际编码值生成，并比较回写字节，验证转换器声明与其真实 Write 实现一致。
    private static byte[] WriteJsonTable(LoadedTable table, byte[]? original = null)
    {
        var wire = new ConfigWire(PortableSchema.Create(table.Schema).Serialize());
        if (original == null)
        {
            using var binary = new MemoryStream();
            table.Codec.Write(binary, table.Rows);
            original = binary.ToArray();
        }
        byte[] json = wire.BinaryToJson(original);
        byte[] restored = wire.JsonToBinary(json);
        if (!original.AsSpan().SequenceEqual(restored))
            throw new InvalidDataException("JSON 回读与原始编码不一致，请检查转换器编码声明：" + table.Schema.Name);
        using var decoded = new MemoryStream(restored, false);
        if (table.Codec.Read(decoded).Count != table.Rows.Count)
            throw new InvalidDataException("JSON 回读行数不一致：" + table.Schema.Name);
        return json;
    }

    private static string Publish(WorkspaceSettings settings, string runId, IReadOnlyList<LoadedTable> tables,
        ProjectInputSnapshot snapshot, WorkflowOperation operation, DiagnosticCollector collector, Action<string> beforeActivate)
    {
        string versions = Path.Combine(settings.OutputFolder, "versions");
        ReleaseHistory.RejectLinks(versions);
        Directory.CreateDirectory(versions);
        string staging = Path.Combine(versions, ".staging-" + runId);
        string versionId = "v" + runId;
        string published = Path.Combine(versions, versionId);
        string manifestPath = Path.Combine(settings.OutputFolder, "current.json");
        string temporary = Path.Combine(settings.OutputFolder, ".current-" + runId + ".tmp");
        using FileStream lease = ReleaseHistory.AcquirePublishLock(settings.OutputFolder);
        Directory.CreateDirectory(staging);
        try
        {
            ReleaseTable[] entries = WriteTables(staging, tables, operation, settings.DataFormat, ExportOptions.NativeSources(settings.CodeFolder).Any());
            var manifest = new ReleaseManifest(ExportOptions.ManifestFormat(settings.DataFormat), ConfigTargets.Marker(settings.Target), versionId, DateTime.UtcNow, entries, ReleaseHistory.CodeFingerprint(snapshot));
            ReleaseHistory.WriteCandidate(staging, manifest, tables, settings, snapshot, collector.Snapshot(), operation);
            snapshot.RequireCurrent(settings, operation.CancellationToken);
            Directory.Move(staging, published);
            ReleaseHistory.WriteJson(temporary, manifest);
            beforeActivate(versionId);
            operation.Checkpoint();
            operation.Report("activating", ConfigText.Get("正在激活完整版本", "Activating the complete version"), committing: true);
            // Same-directory rename is the commit point on supported local file systems.
            File.Move(temporary, manifestPath, overwrite: true);
            return versionId;
        }
        finally
        {
            if (Directory.Exists(staging))
            {
                Directory.Delete(staging, recursive: true);
            }

            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }
}
