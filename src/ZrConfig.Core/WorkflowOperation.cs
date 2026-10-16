using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using ZrConfig.Validation;

namespace ZrConfig.Core;

public sealed record OperationProgress(string RunId, string Stage, string Message,
    int Completed, int? Total, TimeSpan Elapsed, Diagnostic? Diagnostic = null, bool Committing = false);

/// <summary>One operation owns its cancellation, event stream and elapsed time; no shared mutable run state.</summary>
public sealed class WorkflowOperation
{
    // One-shot CLI migration approval; never persisted in project settings.
    internal SourceNameMapping[]? SourceMappings { get; set; }
    internal string? ApiChangeId { get; set; }
    internal ApiMigrationReceipt? MigrationReceipt { get; set; }
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly IProgress<OperationProgress>? _progress;
    public string RunId { get; } = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ") + "_" + Guid.NewGuid().ToString("N")[..8];
    public CancellationToken CancellationToken
    {
        get;
    }
    public TimeSpan Elapsed => _clock.Elapsed;

    public WorkflowOperation(CancellationToken cancellationToken = default, IProgress<OperationProgress>? progress = null)
    {
        CancellationToken = cancellationToken;
        _progress = progress;
    }

    public void Checkpoint() => CancellationToken.ThrowIfCancellationRequested();

    public void Report(string stage, string message, int completed = 0, int? total = null, bool committing = false)
    {
        if (!committing)
        {
            Checkpoint();
        }

        Notify(new(RunId, stage, message, completed, total, Elapsed, Committing: committing));
    }

    public void ReportDiagnostic(Diagnostic diagnostic) => Notify(new(RunId, "diagnostic", diagnostic.Message,
        0, null, Elapsed, diagnostic));

    private void Notify(OperationProgress progress)
    {
        // An observer cannot turn an already committed version into a failed export.
        try
        {
            _progress?.Report(progress);
        }
        catch (Exception error)
        {
            Trace.TraceWarning("Operation progress observer failed: {0}", error.Message);
        }
    }
}

/// <summary>Content fingerprint of the actual inputs, excluding caches and locks.</summary>
public sealed record ProjectInputSnapshot(string Fingerprint, IReadOnlyDictionary<string, string> Files)
{
    public static ProjectInputSnapshot Capture(WorkspaceSettings settings, CancellationToken cancellationToken = default)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal);
        void Add(string category, string root, IEnumerable<string> paths)
        {
            foreach (string path in paths.Order(StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();
                using FileStream input = File.OpenRead(path);
                files.Add(category + "/" + Path.GetRelativePath(root, path).Replace('\\', '/'), Convert.ToHexString(SHA256.HashData(input)));
            }
        }

        Add("Data", settings.DataFolder, ExcelWorkbookInput.FindFiles(settings.DataFolder));
        if (Directory.Exists(settings.CodeFolder))
        {
            Add("Config", settings.CodeFolder, RuleCompiler.EnumerateSources(settings.CodeFolder).Concat(ExportOptions.NativeSources(settings.CodeFolder)));
            string definitions = Path.Combine(settings.CodeFolder, "Tables.json");
            if (File.Exists(definitions))
            {
                Add("Config", settings.CodeFolder, [definitions]);
            }
        }

        if (!string.IsNullOrWhiteSpace(settings.RulesFolder) && Directory.Exists(settings.RulesFolder))
        {
            Add("Rules", settings.RulesFolder, RuleCompiler.EnumerateSources(settings.RulesFolder));
        }

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(ConfigTargets.Marker(settings.Target));
            writer.Write(settings.DataFormat);
            writer.Write(settings.RuntimeFamily);
            foreach (string language in settings.CodeLanguages) writer.Write(language);
            foreach ((string name, string hash) in files)
            {
                writer.Write(name);
                writer.Write(hash);
            }
        }

        return new(Convert.ToHexString(SHA256.HashData(stream.ToArray())), files);
    }

    public void RequireCurrent(WorkspaceSettings settings, CancellationToken cancellationToken = default)
    {
        if (Fingerprint != Capture(settings, cancellationToken).Fingerprint)
        {
            throw new IOException(ZrConfig.Runtime.ConfigText.Get("输入在运行期间发生变化，请保存 Excel 后重试。当前版本未切换。",
                "Inputs changed during this run. Save Excel and retry. The current version was not switched."));
        }
    }
}
