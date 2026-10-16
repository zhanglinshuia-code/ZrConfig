using System.Text.Json;

namespace ZrConfig.Core;

internal static class ReleaseCommand
{
    internal static int Run(string[] args, TextWriter output)
    {
        if (args.Length == 0 || args[0] is "--help" or "help" or "-h")
        {
            output.WriteLine("release <list|diff|rows|rollback|pin|unpin|cleanup> --project <project.zrconfig.json>");
            output.WriteLine("  diff --from <version> --to <version>");
            output.WriteLine("  rows --from <version> --to <version> --table <table> [--offset 0] [--limit 100]");
            output.WriteLine("  rollback --version <version> --compatibility <deployed-manifest.json> --reason <text> [--apply]");
            output.WriteLine("  pin|unpin --version <version> --apply");
            output.WriteLine("  cleanup [--keep 10] [--days 30] [--apply]");
            output.WriteLine("Rollback and cleanup preview by default. --apply executes the reviewed policy. The compatibility manifest must describe the deployed client; the tool cannot discover running clients.");
            return 0;
        }

        string command = args[0];
        string[] allowed = command switch
        {
            "list" => ["--project"],
            "diff" => ["--project", "--from", "--to"],
            "rows" => ["--project", "--from", "--to", "--table", "--offset", "--limit"],
            "rollback" => ["--project", "--version", "--compatibility", "--reason", "--apply"],
            "pin" or "unpin" => ["--project", "--version", "--apply"],
            "cleanup" => ["--project", "--keep", "--days", "--apply"],
            _ => throw new ArgumentException("Unknown release command: " + command)
        };
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int i = 1; i < args.Length; i++)
        {
            string key = args[i];
            if (!allowed.Contains(key) || options.ContainsKey(key))
            {
                throw new ArgumentException("Unknown or duplicate release option: " + key);
            }

            string value = key == "--apply" ? "true" : ++i < args.Length && !args[i].StartsWith("--", StringComparison.Ordinal) ? args[i] : throw new ArgumentException("Missing value: " + key);
            options.Add(key, value);
        }

        string Required(string name) => options.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Required option: " + name);
        int Number(string name, int fallback) => options.TryGetValue(name, out string? value) ? int.TryParse(value, out int number) ? number : throw new ArgumentException("Invalid number: " + name) : fallback;
        var project = WorkspaceSettings.LoadProject(Required("--project"));
        bool apply = options.ContainsKey("--apply");
        object result;
        switch (command)
        {
            case "list":
                result = ReleaseHistory.List(project.OutputFolder);
                break;
            case "diff":
                result = ReleaseHistory.Compare(project.OutputFolder, Required("--from"), Required("--to"));
                break;
            case "rows":
                result = ReleaseHistory.CompareRows(project.OutputFolder, Required("--from"), Required("--to"), Required("--table"), Number("--offset", 0), Number("--limit", 100));
                break;
            case "rollback":
                RollbackPlan rollback = ReleaseHistory.PrepareRollback(project.OutputFolder, Required("--version"), Required("--compatibility"), project.Target);
                string reason = Required("--reason");
                if (apply)
                {
                    ReleaseHistory.Activate(rollback, reason);
                }

                result = new
                {
                    applied = apply,
                    rollback,
                    reason
                };
                break;
            case "pin":
            case "unpin":
                string version = Required("--version");
                if (apply)
                {
                    ReleaseHistory.SetPinned(project.OutputFolder, version, command == "pin");
                }

                result = new
                {
                    applied = apply,
                    version,
                    pinned = command == "pin"
                };
                break;
            default:
                CleanupPlan cleanup = ReleaseHistory.PreviewCleanup(project.OutputFolder, Number("--keep", 10), Number("--days", 30));
                if (apply)
                {
                    ReleaseHistory.Clean(cleanup);
                }

                result = new
                {
                    applied = apply,
                    cleanup
                };
                break;
        }

        output.WriteLine(JsonSerializer.Serialize(result, ReleaseHistory.JsonOptions));
        return 0;
    }
}
