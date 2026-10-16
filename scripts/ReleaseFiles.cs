using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ZrConfig.Maintenance;

internal static partial class ReleaseFiles
{
    private static readonly HashSet<string> s_roots = new(StringComparer.Ordinal)
    {
        ".github",
        "docs",
        "samples",
        "scripts",
        "src",
        "unity",
        "tests"
    };
    private static readonly HashSet<string> s_rootFiles = new(StringComparer.Ordinal)
    {
        ".gitattributes",
        ".gitignore",
        "Directory.Build.props",
        "Directory.Build.targets",
        "global.json",
        "LICENSE",
        "README.md",
        "ZrConfig.slnx"
    };
    private static readonly HashSet<string> s_publicDocs = new(StringComparer.Ordinal)
    {
        "README.md",
        "config-types.md",
        "debugging.md",
        "development.md",
        "project-development.md",
        "rules-development.md",
        "table-indexes.md",
        "type-examples.md",
        "user-guide.md",
        "release-and-acceptance.md",
        "server-generation.md",
        "server-runtime-design.md",
        "project-layout.md"
    };
    private static readonly HashSet<string> s_publicScripts = new(StringComparer.Ordinal)
    {
        "Maintenance.cs",
        "ServerRuntimeChecks.cs",
        "DeliveryChecks.cs",
        "Packages.cs",
        "ReleaseFiles.cs",
        "UnitySample.cs",
        "ZrConfig.Maintenance.csproj",
        "clean.sh",
        "dotnet.sh",
        "maintain.ps1",
        "maintain.sh",
        "package-macos.sh",
        "package-rules-sdk.sh",
        "package-unity.sh",
        "package-windows.ps1"
    };
    private static readonly HashSet<string> s_caches = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin",
        "obj",
        "Library",
        "Temp",
        "Logs",
        "UserSettings",
        "Builds",
        "Output",
        "artifacts",
        ".git",
        ".idea",
        ".vs",
        ".types-cache",
        ".rules-cache"
    };
    internal static IEnumerable<string> SourceFiles(string root)
    {
        foreach (string path in Visit(root))
        {
            yield return path;
        }

        IEnumerable<string> Visit(string directory)
        {
            foreach (string entry in Maintenance.Entries(directory))
            {
                string relative = Path.GetRelativePath(root, entry).Replace('\\', '/');
                if (!Include(relative))
                {
                    continue;
                }

                Maintenance.Require(!Maintenance.IsLink(entry), "源码分发不支持链接：" + relative);
                if (Directory.Exists(entry))
                {
                    foreach (string child in Visit(entry))
                    {
                        yield return child;
                    }
                }
                else
                {
                    yield return relative;
                }
            }
        }
    }

    private static bool Include(string path)
    {
        string[] parts = path.Split('/');
        if (!s_roots.Contains(parts[0]) && !s_rootFiles.Contains(path))
        {
            return false;
        }

        if (parts[0] == ".github" && path is not ".github" and not ".github/workflows" and not ".github/workflows/ci.yml")
        {
            return false;
        }

        if(parts[0]=="tests" && path is not ("tests" or "tests/README.md" or "tests/ZrConfig.Server.Tests" or "tests/ZrConfig.Server.Tests/Program.cs" or "tests/ZrConfig.Server.Tests/ServerRuntimeBenchmarks.cs" or "tests/ZrConfig.Server.Tests/ServerRuntimeScenarios.cs" or "tests/ZrConfig.Server.Tests/ServerRuntimeProfiling.cs" or "tests/ZrConfig.Server.Tests/ServerRuntimeControlBenchmarks.cs" or "tests/ZrConfig.Server.Tests/SampleConsumers.cs" or "tests/ZrConfig.Server.Tests/ZrConfig.Server.Tests.csproj" or "tests/ZrConfig.Tests" or "tests/ZrConfig.Tests/PortableExportTests.cs" or "tests/ZrConfig.Tests/AdditionalLanguageTests.cs" or "tests/ZrConfig.Tests/ServerRuntimeTests.cs" or "tests/ZrConfig.Tests/GeneratedApiContractTests.cs"))return false;

        if (parts[0] == "docs" && parts.Length > 1 && !s_publicDocs.Contains(parts[1]) && path is not "docs/assets" and not "docs/assets/user-guide-example.zip" and not "docs/assets/zrconfig-app.png")
        {
            return false;
        }

        if (parts[0] == "scripts" && parts.Length > 1 && !s_publicScripts.Contains(parts[1]))
        {
            return false;
        }

        if (parts[0] == "samples" && parts.Length > 1 && parts[1] is not "Data" and not "README.md" and not "project.zrconfig.json")
        {
            return false;
        }

        if (parts.Any(s_caches.Contains))
        {
            return false;
        }

        string name = parts[^1];
        if (name is ".DS_Store" or ".generation.lock" or ".support.lock" || name.StartsWith("._") || name.StartsWith("~$"))
        {
            return false;
        }

        if (name.EndsWith(".user") || name.EndsWith(".suo") || name.EndsWith(".tmp"))
        {
            return false;
        }

        const string Sample = "unity/ZrConfig.Sample/";
        if (path.StartsWith(Sample, StringComparison.Ordinal))
        {
            if (parts.Length == 3 && (name.EndsWith(".csproj") || name.Contains(".sln")))
            {
                return false;
            }

            if (path.StartsWith(Sample + "Assets/StreamingAssets/ZrConfig", StringComparison.Ordinal))
            {
                return false;
            }

            if (path.StartsWith(Sample + "Assets/Plugins/", StringComparison.Ordinal) && new[]
            {
                ".dll",
                ".dll.meta",
                ".pdb",
                ".pdb.meta"
            }.Any(s => name.EndsWith(s, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        return true;
    }

    internal static void CheckPortable(string root)
    {
        string[] files = SourceFiles(root).ToArray();
        var included = files.ToHashSet(StringComparer.Ordinal);
        // Build the pattern in parts so that this checker does not match its own source.
        var localPath = new Regex("/" + "(?:Users|Volumes|home)/[^\\s<>\"']+|[A-Za-z]:" + @"\\+Users\\+", RegexOptions.CultureInvariant);
        foreach (string relative in files)
        {
            string path = Path.Combine(root, relative);
            if (Path.GetExtension(path) is ".zip" or ".xlsx")
            {
                using ZipArchive archive = ZipFile.OpenRead(path);
                CheckArchive(archive, relative);
            }
            else if (Path.GetExtension(path) is not ".xls" and not ".png")
            {
                string text = File.ReadAllText(path);
                CheckText(text, relative);
                if (Path.GetExtension(path) == ".md") CheckDocumentationLinks(text, path);
            }
        }

        foreach (string required in new[]
        {
            "LICENSE",
            "global.json",
            "samples/project.zrconfig.json",
            "docs/project-layout.md",
            "tests/README.md",
            "tests/ZrConfig.Server.Tests/Program.cs",
            "tests/ZrConfig.Server.Tests/ServerRuntimeBenchmarks.cs",
            "tests/ZrConfig.Server.Tests/ServerRuntimeScenarios.cs",
            "tests/ZrConfig.Server.Tests/ServerRuntimeProfiling.cs",
            "tests/ZrConfig.Server.Tests/ServerRuntimeControlBenchmarks.cs",
            "tests/ZrConfig.Server.Tests/SampleConsumers.cs",
            "tests/ZrConfig.Server.Tests/ZrConfig.Server.Tests.csproj",
            "tests/ZrConfig.Tests/PortableExportTests.cs",
            "tests/ZrConfig.Tests/AdditionalLanguageTests.cs",
            "tests/ZrConfig.Tests/ServerRuntimeTests.cs",
            "tests/ZrConfig.Tests/GeneratedApiContractTests.cs",
            "unity/ZrConfig.Sample/Assets/ZrConfigSample.unity",
            "unity/ZrConfig.Sample/Packages/manifest.json",
            "unity/ZrConfig.Sample/ProjectSettings/ProjectVersion.txt"
        }

        )
        {
            Maintenance.Require(files.Contains(required), "源码分发缺少文件：" + required);
        }

        using var project = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "samples/project.zrconfig.json")));
        foreach (JsonProperty item in project.RootElement.EnumerateObject().Where(item => item.Name is "DataFolder" or "CodeFolder" or "OutputFolder" or "RulesFolder"))
        {
            Maintenance.Require(item.Value.ValueKind == JsonValueKind.String && !Path.IsPathRooted(item.Value.GetString()!) && !Regex.IsMatch(item.Value.GetString()!,
                @"^[A-Za-z]:[\\/]"),
                "示例项目配置须使用相对路径：" + item.Name);
        }

        using ZipArchive tutorial = ZipFile.OpenRead(Path.Combine(root, "docs/assets/user-guide-example.zip"));
        Maintenance.Require(tutorial.GetEntry("ZrConfig-Beginner/LICENSE") != null, "教程附件缺少 LICENSE。");
        CheckUnityCredentials(File.ReadAllText(Path.Combine(root, "unity/ZrConfig.Sample/ProjectSettings/ProjectSettings.asset")));
        Console.WriteLine($"通过：{files.Length} 个源码分发文件无本机目录依赖，包含 Unity 示例与许可。");
        void CheckDocumentationLinks(string text, string document)
        {
            text = Regex.Replace(text, @"```.*?```", "", RegexOptions.Singleline);
            foreach (Match match in Regex.Matches(text, @"!?\[[^\]]*\]\(([^)\n]+)\)"))
            {
                string target = match.Groups[1].Value.Trim().Trim('<', '>');
                if (Regex.IsMatch(target, @"^[a-zA-Z][a-zA-Z0-9+.-]*:") || target.StartsWith("//", StringComparison.Ordinal)) continue;
                string relative = Uri.UnescapeDataString(target.Split('#', '?')[0]);
                if (relative.Length == 0) continue;
                string resolved = Path.GetFullPath(relative, Path.GetDirectoryName(document)!);
                string entry = Path.GetRelativePath(root, resolved).Replace('\\', '/');
                Maintenance.Require(included.Contains(entry) || files.Any(file => file.StartsWith(entry.TrimEnd('/') + "/", StringComparison.Ordinal)),
                    $"公开文档链接未包含在源码包：{Path.GetRelativePath(root, document)} → {target}");
            }
        }
        void CheckText(string text, string label) => Maintenance.Require(!localPath.IsMatch(text), "发现本机路径：" + label);
        void CheckArchive(ZipArchive archive, string label)
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                using Stream stream = entry.Open();
                if (entry.Name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    using var buffer = new MemoryStream();
                    stream.CopyTo(buffer);
                    buffer.Position = 0;
                    using var book = new ZipArchive(buffer, ZipArchiveMode.Read);
                    CheckArchive(book, label + "!" + entry.FullName);
                }
                else if (entry.Name.Length != 0)
                {
                    using var reader = new StreamReader(stream);
                    CheckText(reader.ReadToEnd(), label + "!" + entry.FullName);
                }
            }
        }
    }

    internal static void CheckUnityCredentials(string settings)
    {
        foreach (Match match in Regex.Matches(settings,
            @"(?m)^[ \t]*(ps4Passcode|ps4NPTitleSecret|metroCertificatePassword|AndroidKeystorePass|AndroidKeyaliasPass)[ \t]*:[ \t]*([^\r\n]*)\r?$"))
        {
            Maintenance.Require(match.Groups[2].Value.Trim()
                .Trim('\'',
                '"')
                .Length == 0,
                "Unity 示例包含非空平台凭据，请在本地备份后清空：" + match.Groups[1].Value);
        }
    }

    public static void PackageSource(string[] args)
    {
        Maintenance.Require(args.Length <= 1, "package-source [新ZIP文件]");
        string version = XDocument.Load(Maintenance.At("src/ZrConfig.App/ZrConfig.App.csproj")).Descendants("Version").Single().Value;
        string output = args.Length == 1 ? Path.GetFullPath(args[0]) : Maintenance.At($"artifacts/source/ZrConfig-{version}-source.zip");
        CheckPortable(Maintenance.Root);
        CreateSourceArchive(Maintenance.Root, output);
        Console.WriteLine("源码包（含 Unity 示例）：" + output);
    }

    internal static void CreateSourceArchive(string root, string output)
    {
        Maintenance.Require(!File.Exists(output) && !Directory.Exists(output), "输出已存在，请指定新文件以保留旧发布。");
        Maintenance.Require(!Maintenance.Within(output,
            root) || Maintenance.Within(output,
            Path.Combine(root,
            "artifacts")),
            "仓库内发布包只能写入 artifacts。");
        string[] files = SourceFiles(root).ToArray();
        Maintenance.Require(!files.Any(f => Path.GetFullPath(Path.Combine(root,
            f))
            .Equals(output,
            StringComparison.OrdinalIgnoreCase)),
            "输出不能覆盖源码。");
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        string temporary = output + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (ZipArchive archive = ZipFile.Open(temporary, ZipArchiveMode.Create))
            {
                foreach (string file in files)
                {
                    archive.CreateEntryFromFile(Path.Combine(root, file), "ZrConfig/" + file, CompressionLevel.Optimal);
                }
            }

            File.Move(temporary, output);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }

    public static void InstallSource(string[] args)
    {
        Maintenance.Require(args.Length == 2, "install-source <ZIP文件> <新目录>；目标目录必须尚不存在。");
        string archive = Path.GetFullPath(args[0]);
        string destination = Path.GetFullPath(args[1]);
        ExtractSource(archive, destination);
        Console.WriteLine("源码已展开：" + destination + "。请在该目录按 README 构建；运行 unity-sample 后再用 Unity 打开示例。");
    }

    internal static void ExtractSource(string archive, string destination)
    {
        Maintenance.Require(!Directory.Exists(destination) && !File.Exists(destination), "目标目录已存在，拒绝覆盖。");
        string parent = Path.GetDirectoryName(destination)!;
        Directory.CreateDirectory(parent);
        string stage = Path.Combine(parent, ".zrconfig-extract-" + Guid.NewGuid().ToString("N"));
        try
        {
            using (ZipArchive zip = ZipFile.OpenRead(archive))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    Maintenance.Require((entry.ExternalAttributes >> 16 & 0xF000) != 0xA000 && !entry.FullName.Contains('\\') && !entry.FullName.Contains(':') && !entry.FullName.Split('/')
                        .Contains(".."),
                        "源码压缩包包含不安全路径或链接。");
                }
            }

            ZipFile.ExtractToDirectory(archive, stage);
            string source = File.Exists(Path.Combine(stage,
                "ZrConfig.slnx")) ? stage : Directory.GetDirectories(stage)
                .SingleOrDefault(d => File.Exists(Path.Combine(d,
                "ZrConfig.slnx"))) ?? "";
            Maintenance.Require(source.Length != 0 && File.Exists(Path.Combine(source, "LICENSE")), "不是有效的 ZrConfig 源码包。");
            Directory.Move(source, destination);
        }
        finally
        {
            if (Directory.Exists(stage))
            {
                Directory.Delete(stage, true);
            }
        }
    }

    public static void CheckPackageLicenses(string directory)
    {
        foreach (string file in new[]
        {
            "LICENSE",
            "Licenses/ThirdPartyNotices.txt",
            "Licenses/ThirdPartyPackages.json"
        }

        )
        {
            Maintenance.Require(File.Exists(Path.Combine(directory, file)), "发布包缺少许可文件：" + file);
        }

        HashSet<string> covered = JsonSerializer.Deserialize<string[]>(File.ReadAllText(Path.Combine(directory,
            "Licenses/ThirdPartyPackages.json")))!.ToHashSet(StringComparer.OrdinalIgnoreCase);
        string[] deps = Directory.GetFiles(directory, "ZrConfig.*.deps.json");
        Maintenance.Require(deps.Length != 0, "发布目录缺少依赖清单。");
        foreach (string file in deps)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file));
            foreach (JsonProperty library in document.RootElement.GetProperty("libraries").EnumerateObject())
            {
                if (library.Value.GetProperty("type").GetString() is "package" or "runtimepack")
                {
                    Maintenance.Require(covered.Contains(library.Name), "新增依赖尚未补齐许可声明：" + library.Name);
                }
            }
        }

        Console.WriteLine("通过：实际发行依赖与第三方许可覆盖版本一致。");
    }
}
