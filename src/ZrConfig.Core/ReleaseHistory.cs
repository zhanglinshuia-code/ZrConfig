using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

public sealed record ReleaseTable(string Name, string File, int Rows, string SchemaHash, string Sha256, string? JsonFile = null, string? JsonSha256 = null);
public sealed record ReleaseManifest(string Format, string Target, string VersionId, DateTime CreatedUtc, ReleaseTable[] Tables, string? CodeFingerprint = null);
public sealed record ReleaseInfo(string VersionId, DateTime CreatedUtc, string Target, int Tables, long Bytes,
    bool Current, bool Pinned, bool Managed, string? Problem);
public sealed record TableDifference(string Table, string Change, int BeforeRows, int AfterRows, long BeforeBytes, long AfterBytes, bool SchemaChanged);
public sealed record FieldDifference(string Field, string? Before, string? After);
public sealed record RowDifference(string Key, string Change, FieldDifference[] Fields);
public sealed record RowDifferencePage(int Total, int Offset, RowDifference[] Items);
public sealed record RollbackPlan(string OutputFolder, string VersionId, string ExpectedCurrentHash, string CandidateHash,
    string CompatibilityFile, string CompatibilityHash, string Target);
public sealed record CleanupCandidate(string VersionId, string ManifestHash, long Bytes);
public sealed record CleanupPlan(string OutputFolder, string ExpectedCurrentHash, int KeepNewest, int MinimumAgeDays, CleanupCandidate[] Candidates);
internal sealed record ReleaseRecord(int Format, string ToolVersion, string InputFingerprint, string CodeFingerprint,
    string? PreviousVersion, IReadOnlyDictionary<string, string> Inputs, TableDifference[] Changes);
internal sealed record RowSnapshot(string SchemaHash, Dictionary<string, Dictionary<string, string>> Rows);

/// <summary>Local immutable versions. All mutations use the same export/publish lock order as exporting.</summary>
public static class ReleaseHistory
{
    internal static JsonSerializerOptions JsonOptions
    {
        get;
    } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
    };

    internal static FileStream AcquireExportLock(string outputFolder)
    {
        RejectLinks(outputFolder);
        RejectLinks(Path.Combine(outputFolder, "export.lock"));
        Directory.CreateDirectory(outputFolder);
        return new FileStream(Path.Combine(outputFolder, "export.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }

    internal static FileStream AcquirePublishLock(string outputFolder)
    {
        RejectLinks(outputFolder);
        RejectLinks(Path.Combine(outputFolder, "publish.lock"));
        return new FileStream(Path.Combine(outputFolder, "publish.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }

    internal static void WriteJson<T>(string path, T value)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        JsonSerializer.Serialize(stream, value, JsonOptions);
        stream.Flush(flushToDisk: true);
    }

    internal static void WriteCandidate(string folder, ReleaseManifest manifest, IReadOnlyList<LoadedTable> tables,
        WorkspaceSettings settings, ProjectInputSnapshot snapshot, IReadOnlyList<Diagnostic> diagnostics, WorkflowOperation operation)
    {
        WriteJson(Path.Combine(folder, "manifest.json"), manifest);
        WriteJson(Path.Combine(folder, "schema.json"), SchemaSummary.Create(settings.Target, tables.Select(t => t.Schema).ToArray()));
        WriteJson(Path.Combine(folder, "diagnostics.json"), new
        {
            runId = operation.RunId,
            diagnostics
        });
        ReleaseManifest? previous = null;
        try
        {
            previous = TryReadCurrent(settings.OutputFolder);
        }
        catch (Exception error) when (error is IOException or JsonException or InvalidDataException)
        {
            System.Diagnostics.Trace.TraceWarning("Previous manifest unavailable for comparison: {0}", error.Message);
        }
        string codeFingerprint = CodeFingerprint(snapshot) ?? "unknown";
        var record = new ReleaseRecord(1, typeof(ExportPipeline).Assembly.GetName().Version?.ToString() ?? "unknown",
            snapshot.Fingerprint, codeFingerprint, previous?.VersionId, snapshot.Files,
            CompareManifests(previous, manifest, settings.OutputFolder, folder));
        WriteJson(Path.Combine(folder, "release.json"), record);
        foreach (LoadedTable table in tables)
        {
            operation.Report("history", ConfigText.Get($"记录 {table.Schema.Name} 的变更依据", $"Recording change data for {table.Schema.Name}"));
            var rows = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            PropertyInfo[] fields = table.Schema.Fields.Select(f => table.Codec.RowType.GetProperty(f.Name)
                ?? throw new InvalidDataException("Missing field: " + f.Name)).ToArray();
            PropertyInfo[] keys = table.Schema.KeyFields.Select(f => table.Codec.RowType.GetProperty(f.Name)!).ToArray();
            foreach (object row in table.Rows)
            {
                operation.Checkpoint();
                string key = JsonSerializer.Serialize(keys.Select(p => Canonical(p.GetValue(row))).ToArray());
                rows.Add(key, fields.ToDictionary(p => p.Name, p => Canonical(p.GetValue(row)), StringComparer.Ordinal));
            }

            WriteJson(Path.Combine(folder, table.Schema.Name + ".rows.json"), new RowSnapshot(table.Schema.SchemaHash, rows));
        }
    }

    public static ReleaseManifest? TryReadCurrent(string outputFolder)
    {
        string path = Path.Combine(outputFolder, "current.json");
        return File.Exists(path) ? ReadManifest(path) : null;
    }

    public static IReadOnlyList<ReleaseInfo> List(string outputFolder)
    {
        string versions = Path.Combine(outputFolder, "versions");
        if (!Directory.Exists(versions))
        {
            return [];
        }

        RejectLinks(versions);
        string? current = TryReadCurrent(outputFolder)?.VersionId;
        var result = new List<ReleaseInfo>();
        foreach (string folder in Directory.EnumerateDirectories(versions).Order(StringComparer.Ordinal))
        {
            string id = Path.GetFileName(folder);
            if (id.StartsWith('.'))
            {
                continue;
            }

            try
            {
                RejectLinks(folder);
                string manifestFile = Path.Combine(folder, "manifest.json");
                ReleaseManifest manifest = ReadManifest(manifestFile);
                RequireIdentity(manifest, id);
                bool managed = File.Exists(Path.Combine(folder, "release.json"));
                if (managed)
                {
                    RejectLinks(Path.Combine(folder, "release.json"));
                    ReleaseRecord? record = JsonSerializer.Deserialize<ReleaseRecord>(File.ReadAllText(Path.Combine(folder, "release.json")), JsonOptions);
                    managed = record is { Format: 1 } && !string.IsNullOrEmpty(record.InputFingerprint) && record.Inputs != null && record.Changes != null;
                }
                long bytes = Directory.EnumerateFiles(folder).Where(p => (File.GetAttributes(p) & FileAttributes.ReparsePoint) == 0).Sum(p => new FileInfo(p).Length);
                result.Add(new(id, manifest.CreatedUtc, manifest.Target, manifest.Tables.Length, bytes, id == current,
                    File.Exists(Path.Combine(folder, "pinned.json")), managed, managed ? null : "Release record missing; automatic cleanup unavailable."));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException or ArgumentException)
            {
                result.Add(new(id, Directory.GetCreationTimeUtc(folder), "?", 0, 0, id == current, true, false,
                    "Incomplete or legacy version; automatic rollback/cleanup unavailable. " + ex.Message));
            }
        }

        return result.OrderByDescending(v => v.CreatedUtc).ThenBy(v => v.VersionId, StringComparer.Ordinal).ToArray();
    }

    public static IReadOnlyList<TableDifference> Compare(string outputFolder, string beforeVersion, string afterVersion)
    {
        string before = ResolveVersion(outputFolder, beforeVersion);
        string after = ResolveVersion(outputFolder, afterVersion);
        ReleaseManifest left = ReadManifest(Path.Combine(before, "manifest.json"));
        ReleaseManifest right = ReadManifest(Path.Combine(after, "manifest.json"));
        return CompareManifests(left, right, outputFolder, after);
    }

    public static RowDifferencePage CompareRows(string outputFolder, string beforeVersion, string afterVersion, string table, int offset = 0, int limit = 100)
    {
        if (offset < 0 || limit < 1 || limit > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Offset must be nonnegative; limit is 1..1000.");
        }

        RequireName(table);
        RowSnapshot Read(string version)
        {
            string folder = ResolveVersion(outputFolder, version);
            string path = Path.Combine(folder, table + ".rows.json");
            RejectLinks(path);
            return JsonSerializer.Deserialize<RowSnapshot>(File.ReadAllText(path), JsonOptions) ?? throw new InvalidDataException("Row snapshot missing.");
        }

        RowSnapshot left = Read(beforeVersion);
        RowSnapshot right = Read(afterVersion);
        if (left.SchemaHash != right.SchemaHash)
        {
            throw new InvalidDataException(ConfigText.Get("结构不同，不能直接逐字段比较。请查看表结构变化。", "Schemas differ; field comparison is unavailable. Review schema changes."));
        }

        var items = new List<RowDifference>();
        int total = 0;
        foreach (string key in left.Rows.Keys.Union(right.Rows.Keys).Order(StringComparer.Ordinal))
        {
            left.Rows.TryGetValue(key, out Dictionary<string, string>? a);
            right.Rows.TryGetValue(key, out Dictionary<string, string>? b);
            FieldDifference[] fields = (a?.Keys ?? Enumerable.Empty<string>()).Union(b?.Keys ?? Enumerable.Empty<string>()).Order(StringComparer.Ordinal)
                .Where(f => a?.GetValueOrDefault(f) != b?.GetValueOrDefault(f))
                .Select(f => new FieldDifference(f, a?.GetValueOrDefault(f), b?.GetValueOrDefault(f))).ToArray();
            if (fields.Length == 0 && a != null && b != null)
            {
                continue;
            }

            if (total++ >= offset && items.Count < limit)
            {
                items.Add(new(key, a == null ? "added" : b == null ? "removed" : "modified", fields));
            }
        }

        return new(total, offset, items.ToArray());
    }

    public static RollbackPlan PrepareRollback(string outputFolder, string versionId, string compatibilityFile, ConfigTarget target)
    {
        outputFolder = Path.GetFullPath(outputFolder);
        compatibilityFile = Path.GetFullPath(compatibilityFile);
        string folder = ResolveVersion(outputFolder, versionId);
        string candidate = Path.Combine(folder, "manifest.json");
        ReleaseManifest manifest = ReadManifest(candidate);
        RequireIdentity(manifest, versionId);
        VerifyFiles(folder, manifest);
        RequireCompatible(manifest, ReadManifest(compatibilityFile), ConfigTargets.Marker(target));
        return new(outputFolder, versionId, HashFile(Path.Combine(outputFolder, "current.json")), HashFile(candidate),
            compatibilityFile, HashFile(compatibilityFile), ConfigTargets.Marker(target));
    }

    public static void Activate(RollbackPlan plan, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A rollback reason is required.", nameof(reason));
        }

        using FileStream exportLock = AcquireExportLock(plan.OutputFolder);
        using FileStream publishLock = AcquirePublishLock(plan.OutputFolder);
        string folder = ResolveVersion(plan.OutputFolder, plan.VersionId);
        string candidate = Path.Combine(folder, "manifest.json");
        if (HashFile(Path.Combine(plan.OutputFolder, "current.json")) != plan.ExpectedCurrentHash || HashFile(candidate) != plan.CandidateHash ||
            HashFile(plan.CompatibilityFile) != plan.CompatibilityHash)
        {
            throw new IOException("Rollback preview expired; prepare and review it again.");
        }

        ReleaseManifest manifest = ReadManifest(candidate);
        RequireIdentity(manifest, plan.VersionId);
        VerifyFiles(folder, manifest);
        RequireCompatible(manifest, ReadManifest(plan.CompatibilityFile), plan.Target);
        string runId = new WorkflowOperation().RunId;
        string logFolder = Path.Combine(plan.OutputFolder, "Logs", runId);
        Directory.CreateDirectory(logFolder);
        WriteJson(Path.Combine(logFolder, "activation.json"), new
        {
            format = 1,
            operation = "rollback",
            runId,
            previousVersion = TryReadCurrent(plan.OutputFolder)?.VersionId,
            versionId = plan.VersionId,
            reason,
            createdUtc = DateTime.UtcNow,
            state = "prepared",
            compatibilityHash = plan.CompatibilityHash
        });
        string temporary = Path.Combine(plan.OutputFolder, ".current-" + runId + ".tmp");
        try
        {
            WriteJson(temporary, manifest);
            File.Move(temporary, Path.Combine(plan.OutputFolder, "current.json"), overwrite: true);
            // Current is authoritative; interrupted receipts never imply a failed activation.
            try
            {
                WriteJson(Path.Combine(logFolder, "activated.json"), new
                {
                    runId,
                    versionId = plan.VersionId,
                    activatedUtc = DateTime.UtcNow
                });
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                System.Diagnostics.Trace.TraceWarning("Activation receipt could not be saved: {0}", error.Message);
            }
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }

    public static void SetPinned(string outputFolder, string versionId, bool pinned)
    {
        using FileStream exportLock = AcquireExportLock(outputFolder);
        using FileStream publishLock = AcquirePublishLock(outputFolder);
        string path = Path.Combine(ResolveVersion(outputFolder, versionId), "pinned.json");
        RejectLinks(path);
        if (pinned)
        {
            WriteJson(path, new
            {
                pinned = true,
                createdUtc = DateTime.UtcNow
            });
        }
        else if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static CleanupPlan PreviewCleanup(string outputFolder, int keepNewest = 10, int minimumAgeDays = 30)
    {
        if (keepNewest < 1 || minimumAgeDays < 1)
        {
            throw new ArgumentException("Keep at least one version and a one-day reader retention window.");
        }

        outputFolder = Path.GetFullPath(outputFolder);
        ReleaseInfo[] versions = List(outputFolder).ToArray();
        var keep = versions.Where(v => v.Managed).Take(keepNewest).Select(v => v.VersionId).ToHashSet(StringComparer.Ordinal);
        CleanupCandidate[] candidates = versions.Where(v => v.Managed && v.Problem == null && !v.Current && !v.Pinned && !keep.Contains(v.VersionId) &&
            v.CreatedUtc < DateTime.UtcNow.AddDays(-minimumAgeDays))
            .Select(v => new CleanupCandidate(v.VersionId, HashFile(Path.Combine(ResolveVersion(outputFolder, v.VersionId), "manifest.json")), v.Bytes)).ToArray();
        return new(outputFolder, HashFile(Path.Combine(outputFolder, "current.json")), keepNewest, minimumAgeDays, candidates);
    }

    public static void Clean(CleanupPlan plan)
    {
        using FileStream exportLock = AcquireExportLock(plan.OutputFolder);
        using FileStream publishLock = AcquirePublishLock(plan.OutputFolder);
        CleanupPlan current = PreviewCleanup(plan.OutputFolder, plan.KeepNewest, plan.MinimumAgeDays);
        if (current.ExpectedCurrentHash != plan.ExpectedCurrentHash || !plan.Candidates.All(c => current.Candidates.Contains(c)))
        {
            throw new IOException("Cleanup preview expired; prepare and review it again.");
        }

        foreach (CleanupCandidate candidate in plan.Candidates)
        {
            string folder = ResolveVersion(plan.OutputFolder, candidate.VersionId);
            ReleaseManifest manifest = ReadManifest(Path.Combine(folder, "manifest.json"));
            var expected = manifest.Tables.SelectMany(t => new[] { t.File, t.Name + ".rows.json" })
                .Concat(["manifest.json", "release.json", "schema.json", "diagnostics.json", "pinned.json"]).ToHashSet(StringComparer.Ordinal);
            // Managed versions have a flat layout. Unknown subdirectories are never traversed or deleted.
            if (Directory.EnumerateDirectories(folder).Any() || Directory.EnumerateFiles(folder).Any(p => !expected.Contains(Path.GetFileName(p)) || (File.GetAttributes(p) & FileAttributes.ReparsePoint) != 0))
            {
                throw new IOException("Unexpected directory/link in version; cleanup stopped: " + candidate.VersionId);
            }
        }

        foreach (CleanupCandidate candidate in plan.Candidates)
        {
            Directory.Delete(ResolveVersion(plan.OutputFolder, candidate.VersionId), recursive: true);
        }
    }

    internal static string? CodeFingerprint(ProjectInputSnapshot snapshot)
    {
        KeyValuePair<string, string>[] sources = snapshot.Files.Where(p => p.Key.StartsWith("Config/", StringComparison.Ordinal)).ToArray();
        return sources.Length == 0 ? null : Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(string.Join("\n", sources.Select(p => p.Key + ":" + p.Value)))));
    }

    private static void RequireCompatible(ReleaseManifest candidate, ReleaseManifest contract, string target)
    {
        if (candidate.Target != target || contract.Target != target || candidate.Format != contract.Format ||
            string.IsNullOrWhiteSpace(candidate.CodeFingerprint) || candidate.CodeFingerprint != contract.CodeFingerprint ||
            candidate.Tables.Length != contract.Tables.Length || candidate.Tables.Any(t => !contract.Tables.Any(c => c.Name == t.Name && c.SchemaHash == t.SchemaHash)))
        {
            throw new InvalidDataException(ConfigText.Get("目标版本与接入方提供的部署清单不兼容，已阻止回滚。", "Version is incompatible with the supplied deployed manifest; rollback blocked."));
        }
    }

    private static void VerifyFiles(string folder, ReleaseManifest manifest)
    {
        foreach (ReleaseTable table in manifest.Tables)
        {
            if (table == null)
            {
                throw new InvalidDataException("Invalid null table manifest.");
            }
            if (table.JsonFile != null)
            {
                string jsonPath = Path.Combine(folder, table.JsonFile);
                RejectLinks(jsonPath);
                if (!string.Equals(HashFile(jsonPath), table.JsonSha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("JSON 版本文件缺失或损坏：" + table.Name);
            }
            string path = Path.Combine(folder, table.File);
            RejectLinks(path);
            if (!string.Equals(HashFile(path), table.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Version file missing or corrupt: " + table.Name);
            }
        }
    }

    private static TableDifference[] CompareManifests(ReleaseManifest? before, ReleaseManifest after, string outputFolder, string afterFolder)
    {
        long Size(string folder, string file) => File.Exists(Path.Combine(folder, file)) ? new FileInfo(Path.Combine(folder, file)).Length : 0;
        var result = new List<TableDifference>();
        foreach (string name in (before?.Tables.Select(t => t.Name) ?? []).Union(after.Tables.Select(t => t.Name)).Order(StringComparer.Ordinal))
        {
            ReleaseTable? a = before?.Tables.FirstOrDefault(t => t.Name == name);
            ReleaseTable? b = after.Tables.FirstOrDefault(t => t.Name == name);
            result.Add(new(name, a == null ? "added" : b == null ? "removed" : a.Sha256 == b.Sha256 ? "unchanged" : "modified", a?.Rows ?? 0, b?.Rows ?? 0,
                a == null ? 0 : Size(ResolveVersion(outputFolder, before!.VersionId), a.File), b == null ? 0 : Size(afterFolder, b.File), a != null && b != null && a.SchemaHash != b.SchemaHash));
        }

        return result.ToArray();
    }

    private static ReleaseManifest ReadManifest(string path)
    {
        RejectLinks(path);
        ReleaseManifest manifest = JsonSerializer.Deserialize<ReleaseManifest>(File.ReadAllText(path), JsonOptions) ?? throw new InvalidDataException("Manifest missing.");
        if (manifest.Format is not ("ZrConfig.Binary/1" or "ZrConfig.Json/1" or "ZrConfig.Mixed/1") || manifest.Target is not ("C" or "S") || manifest.Tables == null)
        {
            throw new InvalidDataException("Unsupported release manifest.");
        }

        RequireName(manifest.VersionId);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (ReleaseTable table in manifest.Tables)
        {
            if (table == null)
            {
                throw new InvalidDataException("Invalid null table manifest.");
            }
            RequireName(table.Name);
            RequireName(table.File);
            bool mixed = manifest.Format == "ZrConfig.Mixed/1";
            if (mixed ? table.JsonFile != table.Name + ".json" || table.JsonSha256 is not { Length: 64 } || !table.JsonSha256.All(Uri.IsHexDigit)
                : table.JsonFile != null || table.JsonSha256 != null)
                throw new InvalidDataException("JSON 清单条目无效：" + table.Name);
            if (table.File != table.Name + (manifest.Format == "ZrConfig.Json/1" ? ".json" : ".bytes") || !names.Add(table.Name) || table.Rows < 0 || string.IsNullOrWhiteSpace(table.SchemaHash) ||
                table.Sha256 is not { Length: 64 } || !table.Sha256.All(Uri.IsHexDigit))
            {
                throw new InvalidDataException("Invalid table manifest: " + table.Name);
            }
        }

        return manifest;
    }

    private static void RequireIdentity(ReleaseManifest manifest, string versionId)
    {
        if (manifest.VersionId != versionId)
        {
            throw new InvalidDataException("Version directory/manifest identity mismatch.");
        }
    }

    private static string ResolveVersion(string outputFolder, string versionId)
    {
        RequireName(versionId);
        string folder = Path.Combine(Path.GetFullPath(outputFolder), "versions", versionId);
        RejectLinks(folder);
        return folder;
    }

    private static void RequireName(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.StartsWith('.') || value.IndexOfAny(['/', '\\', ':']) >= 0 || value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new InvalidDataException("Invalid release path segment.");
        }
    }

    internal static void RejectLinks(string path)
    {
        for (string? current = Path.GetFullPath(path); current != null; current = Path.GetDirectoryName(current))
        {
            if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
            {
                // macOS /tmp and /var are system aliases; callers resolve their output root once.
                if (current is "/tmp" or "/var")
                {
                    continue;
                }

                throw new IOException("Linked release paths are not supported: " + current);
            }
        }
    }

    private static string HashFile(string path)
    {
        RejectLinks(path);
        if (!File.Exists(path))
        {
            return "missing";
        }

        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static string Canonical(object? value)
    {
        var ancestors = new HashSet<object>(ReferenceEqualityComparer.Instance);
        JsonNode? ConvertValue(object? item)
        {
            if (item == null)
            {
                return null;
            }

            Type type = item.GetType();
            if (item is IConfigKeyRow reference)
            {
                return new JsonObject { ["$table"] = type.FullName, ["$id"] = reference.ConfigId };
            }

            if (type.IsPrimitive || item is string or decimal || type.IsEnum)
            {
                return JsonValue.Create(Convert.ToString(item, CultureInfo.InvariantCulture));
            }

            if (!ancestors.Add(item))
            {
                return new JsonObject { ["$cycle"] = type.FullName };
            }

            try
            {
                if (item is IDictionary dictionary)
                {
                    var entries = new SortedDictionary<string, JsonNode?>(StringComparer.Ordinal);
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        entries.Add(entry.Key.GetType().FullName + ":" + (ConvertValue(entry.Key)?.ToJsonString() ?? "null"), ConvertValue(entry.Value));
                    }

                    return new JsonObject(entries);
                }

                if (item is IEnumerable sequence)
                {
                    var array = new JsonArray();
                    foreach (object? element in sequence)
                    {
                        array.Add(ConvertValue(element));
                    }

                    if (item is Array rectangular && rectangular.Rank > 1)
                    {
                        return new JsonObject { ["$shape"] = JsonSerializer.SerializeToNode(Enumerable.Range(0, rectangular.Rank).Select(rectangular.GetLength)), ["values"] = array };
                    }

                    return array;
                }

                var members = new SortedDictionary<string, JsonNode?>(StringComparer.Ordinal) { ["$type"] = JsonValue.Create(type.FullName) };
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    members[field.Name] = ConvertValue(field.GetValue(item));
                }

                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
                {
                    members[property.Name] = ConvertValue(property.GetValue(item));
                }

                return new JsonObject(members);
            }
            finally
            {
                ancestors.Remove(item);
            }
        }

        return ConvertValue(value)?.ToJsonString() ?? "null";
    }
}
