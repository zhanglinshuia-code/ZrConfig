using System.Security.Cryptography;
using System.Text.Json;

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
        Console.WriteLine("用 Unity 打开工程，再打开 Assets/ZrConfigSample.unity 并 Play。编辑器直接读取 samples/Output。");
        Console.WriteLine("Assets/HotUpdate/Config 编译为 ZrConfig.Models；Assets/Editor/ConfigRules 仅编译为 ZrConfig.Sample.Rules.Editor。");
        Console.WriteLine("Runtime、Validation 与 Unity 适配器直接编译 Assets/ZrConfig 中的源码，无需安装 ZrConfig DLL 或 Package。");
    }

    // 先验证和发布完整版本，最后原子切换清单；失败时保留上次有效清单及版本。
    internal static void DeployData(string source, string destination)
    {
        Maintenance.NoOverlap(source, destination);
        SafeDestination(destination);
        byte[] manifest = File.ReadAllBytes(Path.Combine(source, "current.json"));
        using var document = JsonDocument.Parse(manifest);
        JsonElement root = document.RootElement;
        Maintenance.Require(root.GetProperty("format").GetString() is "ZrConfig.Binary/1" or "ZrConfig.Json/1" or "ZrConfig.Mixed/1", "不支持的数据格式。");
        string version = Segment(root.GetProperty("versionId").GetString());
        var payloads = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        foreach (JsonElement table in root.GetProperty("tables").EnumerateArray())
        {
            string name = Segment(table.GetProperty("file").GetString());
            byte[] bytes = File.ReadAllBytes(Path.Combine(source, "versions", version, name));
            Maintenance.Require(Convert.ToHexString(SHA256.HashData(bytes))
                .Equals(table.GetProperty("sha256")
                .GetString(),
                StringComparison.OrdinalIgnoreCase),
                "示例数据哈希不匹配：" + name);
            Maintenance.Require(payloads.TryAdd(name, bytes), "重复的数据文件：" + name);
            if (table.TryGetProperty("jsonFile", out JsonElement alternate) && alternate.ValueKind == JsonValueKind.String)
            {
                string jsonName = Segment(alternate.GetString());
                byte[] json = File.ReadAllBytes(Path.Combine(source, "versions", version, jsonName));
                Maintenance.Require(Convert.ToHexString(SHA256.HashData(json)).Equals(table.GetProperty("jsonSha256").GetString(), StringComparison.OrdinalIgnoreCase), "JSON 数据哈希不匹配：" + jsonName);
                Maintenance.Require(payloads.TryAdd(jsonName, json), "重复的数据文件：" + jsonName);
            }
        }

        Maintenance.Require(payloads.Count > 0, "示例清单没有表。");
        string versions = Path.Combine(destination, "versions");
        string target = Path.Combine(versions, version);
        SafeDestination(target);
        Directory.CreateDirectory(versions);
        string stage = Path.Combine(versions, ".staging-" + Guid.NewGuid().ToString("N"));
        string pending = Path.Combine(destination, ".current-" + Guid.NewGuid().ToString("N"));
        try
        {
            if (Directory.Exists(target))
            {
                foreach (KeyValuePair<string, byte[]> file in payloads)
                {
                    string path = Path.Combine(target, file.Key);
                    SafeDestination(path);
                    Maintenance.Require(File.Exists(path) && File.ReadAllBytes(path)
                        .SequenceEqual(file.Value),
                        "已发布版本内容不同，拒绝覆盖：" + version);
                }
            }
            else
            {
                Directory.CreateDirectory(stage);
                foreach (KeyValuePair<string, byte[]> file in payloads)
                {
                    File.WriteAllBytes(Path.Combine(stage, file.Key), file.Value);
                }

                Directory.Move(stage, target);
            }

            string current = Path.Combine(destination, "current.json");
            SafeDestination(current);
            File.WriteAllBytes(pending, manifest);
            File.Move(pending, current, overwrite: true);
        }
        finally
        {
            if (Directory.Exists(stage))
            {
                Directory.Delete(stage, true);
            }

            if (File.Exists(pending))
            {
                File.Delete(pending);
            }
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
