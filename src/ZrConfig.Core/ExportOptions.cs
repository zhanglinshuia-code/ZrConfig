namespace ZrConfig.Core;

/// <summary>目标语言与数据格式独立选择，旧项目默认值保持 C# 和二进制。</summary>
public static class ExportOptions
{
    public static string Format(string value) => (value ?? "").Trim().ToLowerInvariant() switch
    {
        "binary" => "binary",
        "json" => "json",
        "both" => "both",
        _ => throw new ArgumentException("数据格式只允许 binary、json 或 both。")
    };
    public static string ManifestFormat(string format) => Format(format) switch
    {
        "json" => "ZrConfig.Json/1",
        "both" => "ZrConfig.Mixed/1",
        _ => "ZrConfig.Binary/1"
    };
    public static string[] Languages(IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        string[] languages = values.SelectMany(v => (v ?? "").Split(',')).Select(v => v.Trim().ToLowerInvariant()).ToArray();
        if (languages.Contains("all"))
        {
            if (languages.Length != 1)
                throw new ArgumentException("all 不能与其他语言重复指定。");
            return ["cpp", "csharp", "go", "java", "lua", "rust", "typescript"];
        }
        if (languages.Length == 0 || languages.Any(v => v is not ("csharp" or "java" or "go" or "cpp" or "lua" or "typescript" or "rust")) || languages.Distinct().Count() != languages.Length)
            throw new ArgumentException("目标语言只允许 csharp、java、go、cpp、lua、typescript、rust，多个语言以逗号分隔，不能重复。");
        return languages.Order(StringComparer.Ordinal).ToArray();
    }
    // 只读取生成事务登记的原生源码；这些文件也参与发布指纹和输入变更检查。
    internal static IEnumerable<string> NativeSources(string codeFolder)
    {
        if (string.IsNullOrEmpty(codeFolder)) yield break;
        string generated = Path.Combine(codeFolder, "Generated");
        string manifest = Path.Combine(generated, ".native-files.json");
        if (!File.Exists(manifest)) yield break;
        ReleaseHistory.RejectLinks(manifest);
        yield return manifest;
        foreach (string name in System.Text.Json.JsonSerializer.Deserialize<string[]>(File.ReadAllText(manifest)) ?? [])
        {
            string[] parts = name.Split('/');
            if (parts.Length != 2 || parts[0] is not ("Java" or "Go" or "Cpp" or "Lua" or "TypeScript" or "Rust") || string.IsNullOrEmpty(parts[1]) || parts[1] is "." or ".." || name.Contains('\\') || name.Contains(':'))
                throw new InvalidDataException("生成文件清单路径无效：" + name);
            string path = Path.Combine(generated, name);
            ReleaseHistory.RejectLinks(path);
            yield return path;
        }
    }
}
