using System.Text;
using System.Text.Json;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

// 实例级文件操作用于确定性注入部分写入和回滚故障，不影响其他生成任务。
internal class GenerationFileOperations
{
    internal virtual void Write(string path, byte[] bytes, bool restoring) => File.WriteAllBytes(path, bytes);
    internal virtual void Delete(string path, bool restoring) => File.Delete(path);
}

internal sealed class GenerationFileTransaction(string root, GenerationFileOperations? operations = null)
{
    private readonly string _root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
    private readonly GenerationFileOperations _operations = operations ?? new();
    private readonly Dictionary<string, string?> _changes = new(StringComparer.Ordinal);
    internal void Set(string path, string? content) => _changes.Add(Path.GetFullPath(path), content);

    internal bool Commit(DiagnosticCollector diagnostics)
    {
        string temporary = Path.Combine(Path.GetTempPath(), "zrconfig-generation-" + Guid.NewGuid().ToString("N"));
        var prepared = new List<PreparedFile>();
        var attempted = new List<PreparedFile>();
        var createdDirectories = new List<string>();
        bool retain = false;
        try
        {
            Directory.CreateDirectory(temporary);
            foreach ((string path, string? content) in _changes)
            {
                CheckPath(path);
                bool exists = File.Exists(path);
                if (exists && content != null && File.ReadAllText(path) == content)
                {
                    continue;
                }

                string backup = Path.Combine(temporary, prepared.Count + ".before");
                string staged = Path.Combine(temporary, prepared.Count + ".after");
                if (exists)
                {
                    // 不截断地打开目标；实际写入前拒绝不可访问或只读文件。
                    using var readable = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                    using FileStream saved = File.Create(backup);
                    readable.CopyTo(saved);
                }

                if (content != null)
                {
                    File.WriteAllText(staged, content, new UTF8Encoding(false));
                }

                prepared.Add(new PreparedFile(path, exists ? backup : null, content == null ? null : staged));
            }

            // 回滚再次失败时，保留的备份清单可供人工恢复。
            File.WriteAllText(Path.Combine(temporary, "files.json"), JsonSerializer.Serialize(prepared));
            foreach (PreparedFile file in prepared)
            {
                EnsureDirectory(Path.GetDirectoryName(file.Path)!, createdDirectories);
            }

            // 全部目标目录准备成功后，才开始修改文件；目录准备失败也走统一清理。
            foreach (PreparedFile file in prepared)
            {
                attempted.Add(file); // 当前操作即使只写入一部分，也必须参与回滚。
                if (file.Staged == null)
                {
                    _operations.Delete(file.Path, restoring: false);
                }
                else
                {
                    _operations.Write(file.Path, File.ReadAllBytes(file.Staged), restoring: false);
                }
            }

            return true;
        }
        catch (Exception error)
        {
            var failures = new List<string>();
            foreach (PreparedFile file in attempted.AsEnumerable().Reverse())
            {
                try
                {
                    if (file.Backup == null)
                    {
                        _operations.Delete(file.Path, restoring: true);
                    }
                    else
                    {
                        _operations.Write(file.Path, File.ReadAllBytes(file.Backup), restoring: true);
                    }
                }
                catch (Exception recovery)
                {
                    failures.Add(file.Path + ": " + recovery.Message);
                }
            }

            foreach (string directory in createdDirectories.AsEnumerable().Reverse())
            {
                try
                {
                    if (!Directory.EnumerateFileSystemEntries(directory).Any())
                    {
                        Directory.Delete(directory);
                    }
                }
                catch (Exception cleanup) when (cleanup is IOException or UnauthorizedAccessException)
                {
                    failures.Add(directory + ": " + cleanup.Message);
                }
            }

            retain = failures.Count != 0;
            diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "GENERATION_WRITE_FAILED",
                ConfigText.Get("生成写入失败：", "Generation write failed: ") + error.Message));
            if (retain)
            {
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "GENERATION_ROLLBACK_FAILED",
                    ConfigText.Get("部分文件未恢复；原文件备份和 files.json 保留在：", "Rollback incomplete; backups and files.json retained at: ") +
                    temporary + Environment.NewLine + string.Join(Environment.NewLine, failures), temporary));
            }

            return false;
        }
        finally
        {
            if (!retain && Directory.Exists(temporary))
            {
                try
                {
                    Directory.Delete(temporary, recursive: true);
                }
                catch (Exception cleanup) when (cleanup is IOException or UnauthorizedAccessException)
                {
                    diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, "GENERATION_CLEANUP_FAILED", cleanup.Message, temporary));
                }
            }
        }
    }

    private void CheckPath(string path)
    {
        if (Directory.Exists(path))
        {
            throw new IOException("A directory occupies the generated file path: " + path);
        }

        string relative = Path.GetRelativePath(_root, path);
        if (Path.IsPathRooted(relative) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new IOException("Generation target is outside its code root: " + path);
        }

        for (string? part = path; part != null; part = Path.GetDirectoryName(part))
        {
            if (part != path && File.Exists(part))
            {
                throw new IOException("A file occupies the generated directory path: " + part);
            }

            if ((File.Exists(part) || Directory.Exists(part)) && (File.GetAttributes(part) & FileAttributes.ReparsePoint) != 0)
            {
                throw new IOException("Generation cannot write through a symbolic link: " + part);
            }

            if (string.Equals(part, _root, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            {
                break;
            }
        }
    }

    private static void EnsureDirectory(string path, List<string> created)
    {
        if (Directory.Exists(path))
        {
            return;
        }

        EnsureDirectory(Path.GetDirectoryName(path)!, created);
        Directory.CreateDirectory(path);
        created.Add(path);
    }

    private sealed record PreparedFile(string Path, string? Backup, string? Staged);
}
