using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ZrConfig.Maintenance;

internal static class UnitySample
{
    public static void Prepare()
    {
        string project = Maintenance.At("unity/ZrConfig.Sample");
        Maintenance.Build("src/ZrConfig.Cli");
        Maintenance.RunDotNet([Maintenance.At("src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll"), "export", "--project", Maintenance.At("samples/project.zrconfig.json"), "--generate"]);
        DeployData(Maintenance.At("samples/Output"), Path.Combine(project, "Assets/StreamingAssets/ZrConfig"));
        Console.WriteLine("Unity 示例已准备：" + project);
        Console.WriteLine("用 Unity 打开工程，再打开 Assets/ZrConfigSample.unity 并 Play。编辑器直接读取 samples/Output/CSharp。");
        Console.WriteLine("Assets/HotUpdate/Config 编译为 ZrConfig.Models；Assets/Editor/ConfigRules 仅编译为 ZrConfig.Sample.Rules.Editor。");
        Console.WriteLine("Runtime、Validation 与 Unity 适配器直接编译 Assets/ZrConfig 中的源码，无需安装 ZrConfig DLL 或 Package。");
    }

    // 校验全部输入，仅写入变化文件；清单最后提交，失败恢复原文件。
    internal static void DeployData(string source, string destination)
    {
        Maintenance.NoOverlap(source, destination);
        SafeDestination(destination);
        source = Maintenance.PhysicalPath(source);
        if (File.Exists(Path.Combine(source, "CSharp", "Common", "current.json"))) source = Path.Combine(source, "CSharp");
        string manifestName = File.Exists(Path.Combine(source, "Common", "current.json")) ? "Common/current.json" : "current.json";
        SafeDestination(Path.Combine(source, manifestName));
        byte[] manifest = File.ReadAllBytes(Path.Combine(source, manifestName));
        using var document = JsonDocument.Parse(manifest);
        JsonElement root = document.RootElement;
        Maintenance.Require(root.GetProperty("format").GetString() is "ZrConfig.Binary/1" or "ZrConfig.Json/1" or "ZrConfig.Mixed/1", "不支持的数据格式。");
        string version = Segment(root.GetProperty("versionId").GetString());
        string? layout = root.TryGetProperty("layout", out var layoutValue) ? layoutValue.GetString() : null;
        Maintenance.Require(layout is null or "" or "flat" or "grouped", "不支持的数据布局。");
        string sourceFolder = layout is "flat" or "grouped" ? source : Path.Combine(source, "versions", version);
        var payloads = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        foreach (JsonElement table in root.GetProperty("tables").EnumerateArray())
        {
            string name = Segment(table.GetProperty("file").GetString());
            byte[] bytes = ReadSource(name);
            Maintenance.Require(Convert.ToHexString(SHA256.HashData(bytes))
                .Equals(table.GetProperty("sha256")
                .GetString(),
                StringComparison.OrdinalIgnoreCase),
                "示例数据哈希不匹配：" + name);
            Maintenance.Require(payloads.TryAdd(DataPath(name), bytes), "重复的数据文件：" + name);
            if (table.TryGetProperty("jsonFile", out JsonElement alternate) && alternate.ValueKind == JsonValueKind.String)
            {
                string jsonName = Segment(alternate.GetString());
                byte[] json = ReadSource(jsonName);
                Maintenance.Require(Convert.ToHexString(SHA256.HashData(json)).Equals(table.GetProperty("jsonSha256").GetString(), StringComparison.OrdinalIgnoreCase), "JSON 数据哈希不匹配：" + jsonName);
                Maintenance.Require(payloads.TryAdd(DataPath(jsonName), json), "重复的数据文件：" + jsonName);
            }
        }

        Maintenance.Require(payloads.Count > 0, "示例清单没有表。");
        var current = JsonNode.Parse(manifest)!;
        current["layout"] = "grouped";
        payloads.Add("Common/current.json", System.Text.Encoding.UTF8.GetBytes(current.ToJsonString()));
        Directory.CreateDirectory(destination);
        var changes = new Dictionary<string, byte[]?>(StringComparer.OrdinalIgnoreCase);
        string currentPath = Path.Combine(destination, "Common", "current.json");
        if (!File.Exists(currentPath)) currentPath = Path.Combine(destination, "current.json");
        SafeDestination(currentPath);
        if (File.Exists(currentPath))
        {
            var previous = JsonNode.Parse(File.ReadAllBytes(currentPath))!;
            if (previous["layout"]?.GetValue<string>() is "flat" or "grouped")
                foreach (var table in previous["tables"]!.AsArray())
                    foreach (string field in new[] { "file", "jsonFile" })
                        if (table![field] is { } value)
                        {
                            string name = Segment(value.GetValue<string>());
                            if (previous["layout"]?.GetValue<string>() == "grouped") name = DataPath(name);
                            if (!payloads.ContainsKey(name)) changes[name] = null;
                        }
        }
        if (File.Exists(currentPath) && Path.GetFileName(Path.GetDirectoryName(currentPath)) != "Common") changes["current.json"] = null;
        foreach (var file in payloads)
        {
            string path = Path.Combine(destination, file.Key);
            SafeDestination(path);
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(file.Value)) changes[file.Key] = file.Value;
        }
        var before = new Dictionary<string, (byte[]? Bytes, DateTime Written)>();
        foreach (var file in changes)
        {
            string path = Path.Combine(destination, file.Key);
            SafeDestination(path);
            Maintenance.Require(!Directory.Exists(path), "数据文件路径被目录占用：" + path);
            before[path] = File.Exists(path) ? (File.ReadAllBytes(path), File.GetLastWriteTimeUtc(path)) : (null, default);
        }
        var attempted = new List<string>();
        try
        {
            foreach (var file in changes)
            {
                string path = Path.Combine(destination, file.Key);
                attempted.Add(path);
                Write(path, file.Value);
            }
        }
        catch (Exception error)
        {
            var failures = new List<Exception> { error };
            foreach (string path in attempted.AsEnumerable().Reverse())
                try
                {
                    Write(path, before[path].Bytes);
                    if (before[path].Bytes != null) File.SetLastWriteTimeUtc(path, before[path].Written);
                }
                catch (Exception restore) { failures.Add(restore); }
            if (failures.Count > 1)
            {
                string backup = Maintenance.Temporary("deployment-recovery");
                foreach (var file in before.Where(p => p.Value.Bytes != null))
                {
                    string saved = Path.Combine(backup, Path.GetRelativePath(destination, file.Key));
                    Directory.CreateDirectory(Path.GetDirectoryName(saved)!);
                    File.WriteAllBytes(saved, file.Value.Bytes!);
                }
                throw new AggregateException("部署恢复未完成，原文件保存在：" + backup, failures);
            }
            throw;
        }
        byte[] ReadSource(string file)
        {
            string path = Path.Combine(sourceFolder, layout == "grouped" ? DataPath(file) : file);
            SafeDestination(path);
            return File.ReadAllBytes(path);
        }
        static string DataPath(string file) => "Tables/" + (file.EndsWith(".json", StringComparison.Ordinal) ? "json/" : "bytes/") + file;
        static void Write(string path, byte[]? bytes)
        {
            if (bytes == null) { File.Delete(path); return; }
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string pending = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try { File.WriteAllBytes(pending, bytes); File.Move(pending, path, overwrite: true); }
            finally { if (File.Exists(pending)) File.Delete(pending); }
        }
    }

    private static string Segment(string? value)
    {
        Maintenance.Require(!string.IsNullOrWhiteSpace(value) && value is not "." and not ".." && value.IndexOfAny(['/', '\\', ':']) < 0 && value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0,
            "数据清单路径无效。");
        return value!;
    }

    private static void SafeDestination(string path) => Maintenance.Require(Maintenance.PhysicalPath(path) == Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)),
        "示例输出路径不能经过链接：" + path);
}
