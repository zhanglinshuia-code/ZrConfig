using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

public static class ConfigSourceLayout
{
    private sealed record SupportState(string Version, Dictionary<string, string> Files);
    // 同时识别一表一文件和旧拆分布局，避免把 Generated 当作手写代码根目录。
    internal static bool IsGeneratedDirectory(string root) => Directory.Exists(root) &&
        (File.Exists(Path.Combine(root, "TableCodecs.g.cs")) || Directory.EnumerateFiles(root, "*Config.g.cs").Any() ||
            Directory.EnumerateFiles(root, "*ConfigCodec.g.cs").Any());
    /// <summary>Update only files that still match their recorded tool-provided contents.</summary>
    public static void Initialize(string root, DiagnosticCollector diagnostics) => Prepare(root, diagnostics).Apply(diagnostics);
    /// <summary>Export checks support versions without modifying project sources.</summary>
    internal static void Check(string root, DiagnosticCollector diagnostics)
    {
        SupportPlan plan = Prepare(root, diagnostics);
        foreach (string path in plan.Replacements.Keys)
        {
            diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error,
                "SUPPORT_UPDATE_REQUIRED",
                ConfigText.Get("通用支持源码缺失或版本较旧，请先点击“生成配表代码”自动补齐/升级，再导出。也可执行 support <ConfigRoot>。",
                "Shared support is missing or outdated. Generate table code to update it before exporting, or run support <ConfigRoot>."),
                path));
        }
    }

    internal static SupportPlan Prepare(string root, DiagnosticCollector diagnostics)
    {
        root = Path.GetFullPath(root);
        if (IsGeneratedDirectory(root))
        {
            throw new ArgumentException(ConfigText.Get("请选择 Config 根目录，不是 Generated 子目录。",
                "Select the Config root, not its Generated subdirectory."));
        }

        string serialization = Path.Combine(root, "Serialization");
        string statePath = Path.Combine(serialization, ".support.json");
        var originals = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [statePath] = ReadOptional(statePath)
        };
        var replacements = new Dictionary<string, string>(StringComparer.Ordinal);
        Dictionary<string, string> baseline = new(StringComparer.Ordinal);
        if (originals[statePath] is { } state)
        {
            try
            {
                baseline = JsonSerializer.Deserialize<SupportState>(state)?.Files ?? baseline;
            }
            catch (JsonException)
            {
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning,
                    "SUPPORT_STATE_INVALID",
                    ConfigText.Get("支持源码版本记录损坏；仅接管内容匹配已知工具模板的文件，其余文件保留并提示人工合并。",
                    "Support version metadata is invalid. Only files matching known tool templates can be adopted; other files are preserved for manual merging."),
                    statePath));
            }
        }

        Dictionary<string, string> expectedHashes = new(StringComparer.Ordinal);
        Assembly assembly = typeof(ConfigSourceLayout).Assembly;
        foreach (string resource in assembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".cs.txt",
            StringComparison.Ordinal))
            .Order(StringComparer.Ordinal))
        {
            string name = resource[(resource.IndexOf("Templates.", StringComparison.Ordinal) + 10)..^4];
            string target = Path.Combine(serialization, name);
            using Stream stream = assembly.GetManifestResourceStream(resource)!;
            using StreamReader reader = new(stream, Encoding.UTF8);
            string template = reader.ReadToEnd().ReplaceLineEndings("\n");
            string expected = Hash(template);
            expectedHashes[name] = expected;
            originals[target] = ReadOptional(target);
            string? actual = originals[target] is { } content ? Hash(content) : null;
            if (actual == expected)
            {
                baseline[name] = expected;
                continue;
            }

            if (actual == null || (baseline.TryGetValue(name,
                out string? previous) && actual == previous))
            {
                replacements[target] = template;
                baseline[name] = expected;
            }
            else
            {
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning,
                    "SUPPORT_MERGE_REQUIRED",
                    ConfigText.Get($"支持源码与当前工具不一致（当前 {actual[..12]}，工具 {expected[..12]}），已保留原文件。请由程序员在临时空 Config 运行 support <ConfigRoot> 取得当前支持源码，对比合并本文件，保留手写修改后重新生成和导出。",
                    $"Support differs from this tool (local {actual[..12]}, tool {expected[..12]}). File preserved. Run support <ConfigRoot> in an empty temporary Config, compare and merge this file while preserving custom changes, then regenerate and export."),
                    target));
            }
        }

        string version = Hash(string.Join("\n", expectedHashes.Select(x => x.Key + ":" + x.Value)));
        return new SupportPlan(root,
            originals,
            replacements,
            JsonSerializer.Serialize(new SupportState(version,
            baseline),
            new JsonSerializerOptions { WriteIndented = true, IndentSize = 4 }) + "\n");
    }

    private static string? ReadOptional(string path) => File.Exists(path) ? File.ReadAllText(path) : null;
    internal sealed class SupportPlan(string root,
        Dictionary<string, string?> originals,
        Dictionary<string, string> replacements,
        string state)
    {
        public IReadOnlyDictionary<string, string> Replacements => replacements;
        public bool IsCurrent => originals.All(pair => ReadOptional(pair.Key) == pair.Value);

        public bool Apply(DiagnosticCollector diagnostics) => Apply(diagnostics, null);

        internal bool Apply(DiagnosticCollector diagnostics, GenerationFileOperations? operations)
        {
            try
            {
                using IDisposable lease = AcquireWriteLocks(root);
                if (!IsCurrent)
                {
                    diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "GENERATION_CHANGED",
                        ConfigText.Get("预览后支持源码已变化，请重新生成并核对清单。",
                            "Shared support changed after preview. Generate again and review the new list."), root));
                    return false;
                }

                var transaction = new GenerationFileTransaction(root, operations);
                AddChanges(transaction);
                if (!transaction.Commit(diagnostics))
                {
                    return false;
                }

                ReportUpdates(diagnostics);
                return true;
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "GENERATION_WRITE_FAILED", error.Message, root));
                return false;
            }
        }

        internal void AddChanges(GenerationFileTransaction transaction)
        {
            foreach (KeyValuePair<string, string> pair in replacements)
            {
                transaction.Set(pair.Key, pair.Value);
            }

            transaction.Set(Path.Combine(root, "Serialization", ".support.json"), state);
        }

        internal void ReportUpdates(DiagnosticCollector diagnostics)
        {
            foreach (string path in replacements.Keys.Where(path => originals[path] != null))
            {
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Info, "SUPPORT_UPDATED",
                    ConfigText.Get("已更新未经手写修改的通用支持源码；请同步 Unity 代码。",
                        "Updated unmodified shared support; synchronize Unity code."), path));
            }
        }
    }

    internal static IDisposable AcquireWriteLocks(string root)
    {
        Directory.CreateDirectory(root);
        var generation = new FileStream(Path.Combine(root, ".generation.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        try
        {
            string serialization = Path.Combine(root, "Serialization");
            Directory.CreateDirectory(serialization);
            var support = new FileStream(Path.Combine(serialization, ".support.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            return new WriteLocks(generation, support);
        }
        catch
        {
            generation.Dispose();
            throw;
        }
    }

    private sealed class WriteLocks(FileStream generation, FileStream support) : IDisposable
    {
        public void Dispose()
        {
            support.Dispose();
            generation.Dispose();
        }
    }

    private static string Hash(string source) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source.ReplaceLineEndings("\n"))));
}
