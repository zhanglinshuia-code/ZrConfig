using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using ZrConfig.Runtime;
using ZrConfig.Validation;
using Diagnostic = ZrConfig.Validation.Diagnostic;
using DiagnosticSeverity = ZrConfig.Validation.DiagnosticSeverity;

namespace ZrConfig.Core;
/// <summary>编译规则源码并加载到独立上下文；只缓存编译产物，不保留上次运行的规则实例。</summary>
public static class RuleCompiler
{
    // 与普通 SDK 类库的隐式 using 保持一致，规则文件仍能直接在 Rider 中编辑和编译。
    private const string GlobalUsings = """
        global using System;
        global using System.Collections.Generic;
        global using System.IO;
        global using System.Linq;
        global using System.Net.Http;
        global using System.Threading;
        global using System.Threading.Tasks;
        """;
    // 仅缓存当前进程的固定运行时元数据，不缓存可卸载的规则程序集或类型。
    private static readonly Lazy<IReadOnlyDictionary<string, MetadataReference>> s_frameworkReferences = new(CreateFrameworkReferences);
    private enum SourceKind
    {
        BusinessType,
        Generated,
        Support,
        Rule,
        Project
    }

    private sealed record Source(string Path, string Text, SourceKind Kind);
    private sealed record CachedRules(byte[] Image, byte[] Symbols, Diagnostic[] Diagnostics);
    private sealed record TypeCacheEntry(string Fingerprint, CachedRules Rules, long Size);
    private const int TypeCacheCapacity = 8;
    private const long TypeCacheByteLimit = 32 * 1024 * 1024;
    private static readonly object s_typeCacheGate = new();
    private static readonly Dictionary<string, LinkedListNode<TypeCacheEntry>> s_typeCache = new(StringComparer.Ordinal);
    private static readonly LinkedList<TypeCacheEntry> s_typeCacheRecency = new();
    private static long s_typeCacheBytes;
    public static LoadedRules? Prepare(string rulesFolder,
        string cacheFolder,
        Assembly models,
        DiagnosticCollector collector,
        Action<string>? progress = null,
        bool debug = false) => PrepareCore(rulesFolder,
        null,
        cacheFolder,
        new[] { models, typeof(ITableRules).Assembly, typeof(ConfigText).Assembly },
        collector,
        progress,
        debug);
    /// <summary>外部模型与规则一起编译，共享同一份类型，无需项目 Models DLL。</summary>
    public static LoadedRules? PrepareSources(string generatedFolder,
        string rulesFolder,
        string cacheFolder,
        DiagnosticCollector collector,
        Action<string>? progress = null,
        bool debug = false) => PrepareSources(generatedFolder, rulesFolder, cacheFolder, collector, progress, debug, null);

    internal static LoadedRules? PrepareSources(string generatedFolder,
        string rulesFolder,
        string cacheFolder,
        DiagnosticCollector collector,
        Action<string>? progress,
        bool debug,
        CacheFileOperations? cacheOperations)
    {
        if (!string.IsNullOrWhiteSpace(generatedFolder) && Directory.Exists(generatedFolder))
        {
            if (ConfigSourceLayout.IsGeneratedDirectory(generatedFolder))
            {
                collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                    "CODE_ROOT_GENERATED",
                    ConfigText.Get("请选择 Config 根目录，不是 Generated 子目录。",
                    "Select the Config root, not its Generated subdirectory."),
                    generatedFolder));
                return null;
            }

            ConfigSourceLayout.Check(generatedFolder, collector);
            if (collector.Snapshot().Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                return null;
            }
        }

        return PrepareCore(rulesFolder,
            generatedFolder,
            cacheFolder,
            new[] { typeof(ITableRules).Assembly, typeof(ConfigText).Assembly },
            collector,
            progress,
            debug,
            cacheOperations: cacheOperations);
    }

    internal static LoadedRules? PrepareTypes(string codeRoot,
        DiagnosticCollector collector,
        IReadOnlyDictionary<string, string> replacements,
        Action<string>? progress = null,
        GenerationInputSnapshot? inputs = null) => PrepareCore(codeRoot,
        null,
        Path.Combine(codeRoot,
        ".types-cache"),
        new[] { typeof(ITableRules).Assembly, typeof(ConfigText).Assembly },
        collector,
        progress,
        false,
        true,
        replacements,
        inputs);

    // Compile the entire candidate in memory. The old Generated tree is excluded and no
    // candidate source or API baseline is written before the compiler has accepted it.
    internal static LoadedRules? PrepareGeneration(string codeRoot, IReadOnlyDictionary<string,string> files,
        IReadOnlyDictionary<string,string> support, DiagnosticCollector diagnostics, Action<string>? progress)
    {
        var sources = new Dictionary<string,string>(support, StringComparer.Ordinal);
        foreach (var file in files.Where(f => f.Key.EndsWith(".cs",StringComparison.Ordinal)))
            sources.Add(Path.Combine(codeRoot,"Generated",file.Key),file.Value);
        return PrepareTypes(codeRoot,diagnostics,sources,progress);
    }
    private static LoadedRules? PrepareCore(string rulesFolder,
        string? generatedFolder,
        string cacheFolder,
        Assembly[] shared,
        DiagnosticCollector collector,
        Action<string>? progress,
        bool debug,
        bool typesOnly = false,
        IReadOnlyDictionary<string, string>? replacements = null,
        GenerationInputSnapshot? inputs = null,
        CacheFileOperations? cacheOperations = null)
    {
        cacheOperations ??= new CacheFileOperations();
        try
        {
            bool modelsOnly = generatedFolder != null && string.IsNullOrWhiteSpace(rulesFolder);
            bool requireRules = !typesOnly && !modelsOnly;
            Source[] sources = Array.Empty<Source>();
            if (!modelsOnly)
            {
                if (string.IsNullOrWhiteSpace(rulesFolder))
                {
                    throw new ArgumentException(ConfigText.Get("请指定外部规则源码目录。", "Specify the external rules source directory."));
                }

                rulesFolder = Path.GetFullPath(rulesFolder);
                progress?.Invoke(ConfigText.Get(debug ? "源码编译模式：Debug（关闭优化，支持断点调试）。" : "源码编译模式：Release。",
                    debug ? "Source compilation: Debug (unoptimized, for breakpoint debugging)." : "Source compilation: Release."));
                progress?.Invoke(ConfigText.Get($"正在检查{(typesOnly ? "类型与支持" : "业务规则")}源码：{rulesFolder}",
                    $"Checking {(typesOnly ? "type and support" : "validation rule")} sources: {rulesFolder}"));
                if (!Directory.Exists(rulesFolder) && replacements == null)
                {
                    collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                        typesOnly ? "TYPE_SOURCE_MISSING" : "RULE_SOURCE_MISSING",
                        typesOnly ? ConfigText.Get("代码根目录不存在，请重新选择 Config。",
                        "Code root does not exist. Select Config again.") : ConfigText.Get("规则源码目录不存在，请检查规则目录设置。",
                        "Rule source directory does not exist. Check the rules directory setting."),
                        rulesFolder));
                    return null;
                }

                // Discover handwritten sources from the selected root, regardless of folder names.
                // Old generated models must not prevent repairing a schema through regeneration.
                IEnumerable<string> paths = EnumerateSources(rulesFolder, typesOnly ? Path.Combine(rulesFolder, "Generated") : null);
                sources = paths.Concat(replacements?.Keys ?? Enumerable.Empty<string>())
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .Select(path => new Source(path,
                    replacements != null && replacements.TryGetValue(path,
                    out string? text) ? text : File.ReadAllText(path),
                    typesOnly ? Classify(path,
                    rulesFolder) : SourceKind.Rule))
                    .ToArray();
                if (sources.Length == 0)
                {
                    collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                        typesOnly ? "TYPE_SOURCE_EMPTY" : "RULE_SOURCE_EMPTY",
                        typesOnly ? ConfigText.Get("所选代码根目录中没有可编译的手写源码，请先初始化支持源码。",
                        "The selected code root contains no compilable handwritten sources. Initialize shared support first.") : ConfigText.Get("规则目录中没有 C# 文件，已阻止导出。",
                        "No C# files were found in the rules directory. Export is blocked."),
                        rulesFolder));
                    return null;
                }
            }

            if (generatedFolder != null)
            {
                if (string.IsNullOrWhiteSpace(generatedFolder) || !Directory.Exists(generatedFolder))
                {
                    throw new DirectoryNotFoundException(ConfigText.Get("Config 代码根目录不存在或未填写，请选择 Config 并先生成配表代码。",
                        "The Config code root is missing or blank. Select Config and generate table code first."));
                }

                string[] generated = EnumerateSources(Path.GetFullPath(generatedFolder)).ToArray();
                if (generated.Length == 0)
                {
                    throw new InvalidOperationException(ConfigText.Get("模型目录为空，请先生成配表代码。",
                        "Model directory is empty; generate table code first."));
                }

                sources = sources.Concat(generated.Select(path => new Source(path,
                    File.ReadAllText(path),
                    Classify(path,
                    generatedFolder))))
                    .DistinctBy(source => source.Path,
                    StringComparer.Ordinal)
                    .OrderBy(source => source.Path,
                    StringComparer.Ordinal)
                    .ToArray();
            }

            foreach (Source source in sources)
            {
                inputs?.RecordSource(source.Path, source.Text);
            }

            string fingerprint = Fingerprint(sources, shared, debug);
            string cacheFile = Path.Combine(cacheFolder, fingerprint + ".json");
            CachedRules? cached = typesOnly ? ReadTypeCache(fingerprint) : ReadCache(cacheFile, collector, cacheOperations);
            if (cached != null)
            {
                try
                {
                    LoadedRules loaded = Load(cached.Image, cached.Symbols, shared, requireRules);
                    foreach (Diagnostic diagnostic in cached.Diagnostics)
                    {
                        collector.Add(diagnostic);
                    }

                    progress?.Invoke(typesOnly ? ConfigText.Get("类型源码未变化，已加载内存编译缓存。",
                        "Type sources unchanged; loaded the in-memory compilation cache.") : ConfigText.Get("项目源码未变化，已加载编译缓存。",
                        "Project sources unchanged; loaded the cached compilation."));
                    return loaded;
                }
                catch (Exception ex) when (ex is BadImageFormatException or FileLoadException or TypeLoadException or ReflectionTypeLoadException)
                {
                    // 缓存损坏时重新编译；绝不退回旧版本的规则。
                    if (!typesOnly)
                    {
                        CacheWarning(collector, "RULE_CACHE_READ_FAILED", cacheFile, ex.Message);
                    }
                }
            }

            progress?.Invoke(ConfigText.Get("项目源码已变化或尚无缓存，正在自动编译……",
                "Project sources changed or are not cached; compiling automatically..."));
            var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp14,
                preprocessorSymbols: new[] { debug ? "DEBUG" : "RELEASE", "TRACE", "NET", "NET10_0", "NETCOREAPP", "NET10_0_OR_GREATER" });
            SyntaxTree[] trees = sources.Select(source => CSharpSyntaxTree.ParseText(source.Text,
                parseOptions,
                source.Path,
                Encoding.UTF8))
                .Append(CSharpSyntaxTree.ParseText(GlobalUsings,
                parseOptions,
                "ZrConfig.ImplicitUsings.g.cs",
                Encoding.UTF8))
                .ToArray();
            var compilation = CSharpCompilation.Create("ZrConfig.DynamicRules_" + fingerprint,
                trees,
                References(shared),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: debug ? OptimizationLevel.Debug : OptimizationLevel.Release,
                nullableContextOptions: NullableContextOptions.Enable,
                deterministic: true));
            using MemoryStream image = new();
            using MemoryStream symbols = new();
            EmitResult emitted = compilation.Emit(image,
                symbols,
                options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));
            Dictionary<string, SourceKind> sourceKinds = sources.ToDictionary(source => source.Path,
                source => source.Kind,
                StringComparer.Ordinal);
            Diagnostic[] diagnostics = emitted.Diagnostics.Where(d => d.Severity is Microsoft.CodeAnalysis.DiagnosticSeverity.Error or Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
                .Select(d => ToDiagnostic(d,
                sourceKinds))
                .ToArray();
            foreach (Diagnostic diagnostic in diagnostics)
            {
                collector.Add(diagnostic);
            }

            if (!emitted.Success)
            {
                return null;
            }

            byte[] bytes = image.ToArray();
            byte[] pdb = symbols.ToArray();
            LoadedRules result = Load(bytes, pdb, shared, requireRules);
            // 预览只缓存字节和诊断，不写项目、不持有可卸载的程序集/类型/实例。
            try
            {
                var compiled = new CachedRules(bytes, pdb, diagnostics);
                if (typesOnly)
                {
                    SaveTypeCache(fingerprint, compiled);
                }
                else
                {
                    SaveCache(cacheFile, compiled, collector, cacheOperations);
                }
            }
            catch
            {
                result.Dispose();
                throw;
            }

            progress?.Invoke(ConfigText.Get("项目源码编译完成，已加载最新代码。", "Project sources compiled successfully; loaded the latest code."));
            return result;
        }
        catch (Exception ex)
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                typesOnly ? "TYPE_PREPARE_FAILED" : generatedFolder != null ? "SOURCE_PREPARE_FAILED" : "RULE_PREPARE_FAILED",
                ConfigText.Get("项目源码准备失败：",
                "Project source preparation failed: ") + ex.Message + Environment.NewLine + ConfigText.Get("请核对代码/规则目录及本次编译错误。异常详情：",
                "Check code/rules directories and the compilation errors above. Exception details:") + Environment.NewLine + ex,
                typesOnly || generatedFolder == null ? rulesFolder : generatedFolder));
            return null;
        }
    }

    internal static IEnumerable<string> EnumerateSources(string directory, string? excludedDirectory = null)
    {
        if (!Directory.Exists(directory))
        {
            yield break;
        }

        foreach (string path in Directory.EnumerateFiles(directory, "*.cs"))
        {
            if (!Path.GetFileName(path).StartsWith('.') && (File.GetAttributes(path) & FileAttributes.ReparsePoint) == 0)
            {
                yield return path;
            }
        }

        foreach (string child in Directory.EnumerateDirectories(directory))
        {
            string name = Path.GetFileName(child);
            if (name.Equals("bin",
                StringComparison.OrdinalIgnoreCase) || name.Equals("obj",
                StringComparison.OrdinalIgnoreCase) || name.StartsWith('.') || (File.GetAttributes(child) & FileAttributes.ReparsePoint) != 0 || string.Equals(child,
                excludedDirectory,
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            {
                continue;
            }

            foreach (string path in EnumerateSources(child, excludedDirectory))
            {
                yield return path;
            }
        }
    }

    private static string Fingerprint(Source[] sources, Assembly[] shared, bool debug)
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write("ZrConfig.Rules/4/ExternalSources/CSharp14/PortablePdb");
            writer.Write(debug);
            writer.Write(GlobalUsings);
            writer.Write(ConfigText.Language); // 缓存中的编译诊断也要跟随本次语言。
            foreach (Assembly assembly in shared.Append(typeof(object).Assembly)
                .Append(typeof(CSharpCompilation).Assembly)
                .Append(typeof(RuleCompiler).Assembly))
            {
                writer.Write(assembly.ManifestModule.ModuleVersionId.ToString());
            }

            foreach (Source source in sources)
            {
                writer.Write(source.Path);
                writer.Write(source.Text);
                writer.Write((int)source.Kind);
            }
        }

        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    private static IReadOnlyDictionary<string, MetadataReference> CreateFrameworkReferences()
    {
        string runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        string[] paths = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "").Split(Path.PathSeparator);
        Dictionary<string, MetadataReference> references = new(StringComparer.Ordinal);
        foreach (string path in paths)
        {
            if (Path.GetDirectoryName(path) == runtimeDirectory && !references.ContainsKey(path))
            {
                references.Add(path, MetadataReference.CreateFromFile(path));
            }
        }

        return references;
    }

    private static IEnumerable<MetadataReference> References(Assembly[] shared)
    {
        IReadOnlyDictionary<string, MetadataReference> framework = s_frameworkReferences.Value;
        foreach (MetadataReference reference in framework.Values)
        {
            yield return reference;
        }

        foreach (Assembly assembly in shared)
        {
            if (!framework.ContainsKey(assembly.Location))
            {
                yield return MetadataReference.CreateFromFile(assembly.Location);
            }
        }
    }

    private static SourceKind Classify(string path,
        string root) => Path.GetRelativePath(Path.GetFullPath(root),
        path)
        .Split(Path.DirectorySeparatorChar)[0] switch
        {
            "Generated" => SourceKind.Generated,
            "Serialization" => SourceKind.Support,
            _ => SourceKind.BusinessType
        };
    private static Diagnostic ToDiagnostic(Microsoft.CodeAnalysis.Diagnostic diagnostic,
        IReadOnlyDictionary<string, SourceKind> sourceKinds)
    {
        FileLinePositionSpan location = diagnostic.Location.GetLineSpan();
        int row = diagnostic.Location.IsInSource ? location.StartLinePosition.Line + 1 : 0;
        int column = diagnostic.Location.IsInSource ? location.StartLinePosition.Character + 1 : 0;
        string message = diagnostic.GetMessage(CultureInfo.GetCultureInfo(ConfigText.Language));
        if (!sourceKinds.TryGetValue(location.Path ?? "", out SourceKind kind))
        {
            kind = SourceKind.Project;
        }

        (string label, string advice) = kind switch
        {
            SourceKind.BusinessType => (ConfigText.Get("业务类型",
                "Business type"), ConfigText.Get("请修正此文件中的手写类型。",
                "Fix the handwritten type in this file.")),
            SourceKind.Generated => (ConfigText.Get("生成代码",
                "Generated code"), ConfigText.Get("请重新生成；仍失败时检查支持源码版本，不要手改 .g.cs。",
                "Regenerate; if it still fails, check support versions instead of editing .g.cs.")),
            SourceKind.Support => (ConfigText.Get("序列化支持/转换器",
                "Serialization support/converter"), ConfigText.Get("请检查支持源码升级提示或修正手写转换器。",
                "Check support upgrade notices or fix the custom converter.")),
            SourceKind.Rule => (ConfigText.Get("业务规则",
                "Validation rule"), ConfigText.Get("请修正 Rules 中的业务规则。",
                "Fix the validation rule in Rules.")),
            _ => (ConfigText.Get("项目代码", "Project code"), ConfigText.Get("请检查此文件及代码根目录。", "Check this file and the code root."))
        };
        return new Diagnostic(diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning,
            diagnostic.Id,
            ConfigText.Get($"{label}编译（第 {row} 行，第 {column} 列）：{message} {advice}",
            $"{label} compilation (line {row}, column {column}): {message} {advice}"),
            location.Path ?? "",
            row: row,
            column: column);
    }

    private static CachedRules? ReadTypeCache(string fingerprint)
    {
        lock (s_typeCacheGate)
        {
            if (!s_typeCache.TryGetValue(fingerprint, out LinkedListNode<TypeCacheEntry>? node))
            {
                return null;
            }

            s_typeCacheRecency.Remove(node);
            s_typeCacheRecency.AddFirst(node);
            return node.Value.Rules;
        }
    }

    private static void SaveTypeCache(string fingerprint, CachedRules rules)
    {
        long size = rules.Image.LongLength + rules.Symbols.LongLength + 256;
        foreach (Diagnostic diagnostic in rules.Diagnostics)
        {
            size += 256L + 2L * (diagnostic.Message.Length + diagnostic.File.Length + diagnostic.Code.Length + diagnostic.Table.Length + diagnostic.Sheet.Length + diagnostic.Field.Length);
        }

        if (size > TypeCacheByteLimit)
        {
            return;
        }

        lock (s_typeCacheGate)
        {
            if (s_typeCache.Remove(fingerprint, out LinkedListNode<TypeCacheEntry>? previous))
            {
                s_typeCacheRecency.Remove(previous);
                s_typeCacheBytes -= previous.Value.Size;
            }

            s_typeCache.Add(fingerprint, s_typeCacheRecency.AddFirst(new TypeCacheEntry(fingerprint, rules, size)));
            s_typeCacheBytes += size;
            while (s_typeCache.Count > TypeCacheCapacity || s_typeCacheBytes > TypeCacheByteLimit)
            {
                LinkedListNode<TypeCacheEntry> oldest = s_typeCacheRecency.Last!;
                s_typeCache.Remove(oldest.Value.Fingerprint);
                s_typeCacheBytes -= oldest.Value.Size;
                s_typeCacheRecency.RemoveLast();
            }
        }
    }

    internal class CacheFileOperations
    {
        internal virtual Stream OpenRead(string path) => File.OpenRead(path);
        internal virtual Stream Create(string path) => new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        internal virtual void Move(string source, string destination) => File.Move(source, destination, overwrite: true);
        internal virtual void Delete(string path) => File.Delete(path);
    }

    private static CachedRules? ReadCache(string file, DiagnosticCollector collector, CacheFileOperations operations)
    {
        try
        {
            using Stream stream = operations.OpenRead(file);
            CachedRules? cached = JsonSerializer.Deserialize<CachedRules>(stream);
            if (cached is not { Image.Length: > 0, Symbols.Length: > 0, Diagnostics: not null } || cached.Diagnostics.Any(d => d == null))
            {
                throw new InvalidDataException("Invalid compilation cache contents.");
            }

            return cached;
        }
        catch (Exception error) when (error is FileNotFoundException or DirectoryNotFoundException)
        {
            return null; // An absent cache is a normal miss.
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidDataException or JsonException or ArgumentException)
        {
            CacheWarning(collector, "RULE_CACHE_READ_FAILED", file, error.Message);
            return null;
        }
    }

    private static void SaveCache(string file, CachedRules rules, DiagnosticCollector collector, CacheFileOperations operations)
    {
        string temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
        var failures = new List<string>();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            using (Stream stream = operations.Create(temporary))
            {
                JsonSerializer.Serialize(stream, rules);
            }

            operations.Move(temporary, file);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            failures.Add(error.Message);
        }
        finally
        {
            try
            {
                if (File.Exists(temporary))
                {
                    operations.Delete(temporary);
                }
            }
            catch (Exception cleanup) when (cleanup is IOException or UnauthorizedAccessException)
            {
                failures.Add(ConfigText.Get("临时缓存清理失败：", "Temporary cache cleanup failed: ") + temporary + ": " + cleanup.Message);
            }

            if (failures.Count != 0)
            {
                CacheWarning(collector, "RULE_CACHE_WRITE_FAILED", file, string.Join(Environment.NewLine, failures));
            }
        }
    }

    private static void CacheWarning(DiagnosticCollector collector, string code, string file, string detail)
    {
        collector.Add(new Diagnostic(DiagnosticSeverity.Warning, code,
            ConfigText.Get("编译缓存不可用；将使用本次编译结果。", "Compilation cache unavailable; using this compilation's result.") +
            Environment.NewLine + detail, file));
    }

    private static LoadedRules Load(byte[] image, byte[] symbols, Assembly[] shared, bool requireRules)
    {
        RuleLoadContext context = new(shared);
        try
        {
            using MemoryStream stream = new(image, writable: false);
            using MemoryStream pdb = new(symbols, writable: false);
            Assembly assembly = context.LoadFromStream(stream, pdb);
            if (requireRules && !assembly.GetTypes()
                .Any(type => !type.IsAbstract && !type.ContainsGenericParameters && typeof(ITableRules).IsAssignableFrom(type)))
            {
                throw new TypeLoadException(ConfigText.Get("源码中没有可用的 TableRules<T> 检查规则。",
                    "The sources contain no usable TableRules<T> validation rules."));
            }

            return new LoadedRules(context, assembly);
        }
        catch
        {
            context.Unload();
            throw;
        }
    }

    private sealed class RuleLoadContext(Assembly[] shared) : AssemblyLoadContext(isCollectible: true)
    {
        protected override Assembly? Load(AssemblyName name) => shared.FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(),
            name));
    }
}

public sealed class LoadedRules : IDisposable
{
    private AssemblyLoadContext? _context;
    private Assembly? _assembly;
    public Assembly Assembly => _assembly ?? throw new ObjectDisposedException(nameof(LoadedRules));

    internal LoadedRules(AssemblyLoadContext context, Assembly assembly)
    {
        _context = context;
        _assembly = assembly;
    }

    public void Dispose()
    {
        _assembly = null;
        _context?.Unload();
        _context = null;
    }
}
