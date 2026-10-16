using System.Text.Json;

namespace ZrConfig.App;

internal sealed record RecentProject(string Path, string Name, DateTime OpenedUtc)
{
    public override string ToString() => Name + " · " + Path + (File.Exists(Path) ? "" : " (missing)");
}

internal sealed class ProjectPreferences
{
    private readonly string? _path;
    internal List<RecentProject> Recent { get; private set; } = [];
    internal ProjectPreferences(string? workspaceSettingsPath)
    {
        _path = workspaceSettingsPath == null ? null : Path.Combine(Path.GetDirectoryName(Path.GetFullPath(workspaceSettingsPath))!, "recent-projects.json");
        try
        {
            if (_path != null && File.Exists(_path))
            {
                Recent = (JsonSerializer.Deserialize<List<RecentProject>>(File.ReadAllText(_path)) ?? [])
                    .Where(p => p != null && !string.IsNullOrWhiteSpace(p.Path) && !string.IsNullOrWhiteSpace(p.Name))
                    .Take(12).ToList();
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Recent = [];
        }
    }

    internal void Remember(string path, string? name)
    {
        path = Path.GetFullPath(path);
        Recent.RemoveAll(p => string.Equals(p.Path, path, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
        Recent.Insert(0, new(path, string.IsNullOrWhiteSpace(name) ? Path.GetFileNameWithoutExtension(path) : name, DateTime.UtcNow));
        Recent = Recent.Take(12).ToList();
        Save();
    }

    internal void Remove(string path)
    {
        Recent.RemoveAll(p => p.Path == path);
        Save();
    }

    private void Save()
    {
        if (_path == null)
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        string temporary = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(Recent));
            File.Move(temporary, _path, overwrite: true);
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
