namespace ZrConfig.Core;

public sealed class CallbackProgress<T>(Action<T> callback) : IProgress<T>
{
    public void Report(T value) => callback(value);
}

public static class OperationReport
{
    public static void Save(string path, ProjectWorkflowResult result, WorkspaceSettings settings, string operation)
    {
        path = Path.GetFullPath(path);
        if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Diagnostic reports must use a .json file.", nameof(path));
        }

        ReleaseHistory.RejectLinks(path);
        StringComparison comparison = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        bool Inside(string root) => !string.IsNullOrWhiteSpace(root) && (path.Equals(Path.GetFullPath(root), comparison) ||
            path.StartsWith(Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar, comparison));
        if (Inside(settings.DataFolder) || Inside(settings.CodeFolder) || Inside(settings.RulesFolder) || Inside(settings.OutputFolder))
        {
            throw new ArgumentException("Save the diagnostic report outside Data, Config, Rules and managed Output directories.", nameof(path));
        }

        string Redact(string text)
        {
            foreach ((string root, string replacement) in new[] { (settings.DataFolder, "<Data>"), (settings.CodeFolder, "<Config>"),
                (settings.RulesFolder, "<Rules>"), (settings.OutputFolder, "<Output>") }.OrderByDescending(p => p.Item1.Length))
            {
                if (!string.IsNullOrWhiteSpace(root))
                {
                    text = text.Replace(Path.GetFullPath(root), replacement, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                }
            }

            return text;
        }

        object[] diagnostics = result.PreparationDiagnostics.Concat(result.Export?.Diagnostics ?? []).Select(d => (object)new
        {
            severity = d.Severity.ToString(),
            d.Code,
            message = Redact(d.Message),
            file = Redact(d.File),
            d.Sheet,
            d.Table,
            d.Row,
            d.Column,
            d.Field
        }).ToArray();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            ReleaseHistory.WriteJson(temporary, new
            {
                format = 1,
                operation,
                result.Success,
                result.Cancelled,
                result.NeedsGeneration,
                result.CodeCommitted,
                published = result.Export?.Published ?? false,
                reusedVersion = result.Export?.ReusedVersion ?? false,
                updatedTables = result.Export?.UpdatedTables ?? 0,
                runId = result.Export?.RunId,
                versionId = result.Export?.VersionId,
                inputFingerprint = result.Export?.InputFingerprint,
                elapsedMilliseconds = result.Export?.Elapsed.TotalMilliseconds,
                target = ConfigTargets.Marker(settings.Target),
                toolVersion = typeof(ProjectWorkflow).Assembly.GetName().Version?.ToString(),
                diagnostics
            });
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }
}
