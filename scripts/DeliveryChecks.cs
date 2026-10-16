using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;

namespace ZrConfig.Maintenance;

/// <summary>Public verification and release receipts; no private test project is required.</summary>
internal static class DeliveryChecks
{
    internal static void VerifySource()
    {
        string temporary = Maintenance.Temporary("public-verification");
        try
        {
            Maintenance.Build("src/ZrConfig.Cli", "Release");
            Maintenance.Build("src/ZrConfig.App", "Release");
            string cli = Maintenance.At("src/ZrConfig.Cli/bin/Release/net10.0/ZrConfig.Cli.dll");
            string code = Path.Combine(temporary, "Config");
            string output = Path.Combine(temporary, "Output");
            string rules = Path.Combine(temporary, "Rules");
            Maintenance.CopyTree(Maintenance.At("unity/ZrConfig.Sample/Assets/HotUpdate/Config"), code,
                name => !name.StartsWith('.') && name is not "bin" and not "obj" and not "Generated");
            Maintenance.CopyTree(Maintenance.At("unity/ZrConfig.Sample/Assets/Editor/ConfigRules"), rules, name => !name.StartsWith('.') && name is not "bin" and not "obj");
            string data = Maintenance.At("samples/Data");
            Maintenance.RunDotNet([cli, "codegen", data, code]);
            Dictionary<string, string> before = Fingerprints(code);
            Maintenance.RunDotNet([cli, "validate", data, code, rules, "--report", Path.Combine(temporary, "validation.json")]);
            Maintenance.Require(!Directory.Exists(output) && before.OrderBy(p => p.Key).SequenceEqual(Fingerprints(code).OrderBy(p => p.Key)), "只校验改写了 Config 或 Output。");
            Maintenance.RunDotNet([cli, "export", data, output, code, rules]);
            byte[] current = File.ReadAllBytes(Path.Combine(output, "current.json"));
            Maintenance.WriteText(Path.Combine(rules, "Invalid.cs"), "This is deliberately invalid C#;");
            Maintenance.RunDotNet([cli, "export", data, output, code, rules], expected: 1, capture: true);
            Maintenance.Require(current.SequenceEqual(File.ReadAllBytes(Path.Combine(output, "current.json"))), "失败导出改写当前版本。");
            File.Delete(Path.Combine(rules, "Invalid.cs"));
            Maintenance.RunDotNet([cli, "export", data, output, code, rules]);
            using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "current.json")));
            string version = manifest.RootElement.GetProperty("versionId").GetString()!;
            Maintenance.Require(File.Exists(Path.Combine(output, "versions", version, "manifest.json")) && File.Exists(Path.Combine(output, "versions", version, "release.json")), "缺少版本独立清单。");
            string project = Path.Combine(temporary, "project.zrconfig.json");
            Maintenance.WriteJson(project, new
            {
                DataFolder = data,
                OutputFolder = output,
                CodeFolder = code,
                RulesFolder = rules,
                Target = "C"
            });
            Maintenance.RunDotNet([cli, "release", "list", "--project", project]);
            Maintenance.RunDotNet([cli, "release", "cleanup", "--project", project]);
            Console.WriteLine("通过：公开源码构建、生成、只校验无写入、正式九表临时导出、失败保护和版本管理入口。");
        }
        finally
        {
            Directory.Delete(temporary, recursive: true);
        }
    }

    internal static void RecordRelease(string folder)
    {
        folder = Path.GetFullPath(folder);
        Maintenance.Require(Directory.Exists(folder), "发布目录不存在。");
        string path = Path.Combine(folder, "release-receipt.json");
        Maintenance.Require(!File.Exists(path), "发布记录已存在；不得覆盖有效发布记录。");
        string version = XDocument.Load(Maintenance.At("src/ZrConfig.App/ZrConfig.App.csproj")).Descendants("Version").Single().Value;
        var sources = ReleaseFiles.SourceFiles(Maintenance.Root).ToDictionary(p => p, p => Hash(Maintenance.At(p)), StringComparer.Ordinal);
        Dictionary<string, string> artifacts = Fingerprints(folder, allowInternalFileLinks: true);
        Maintenance.WriteJson(path, new
        {
            format = 1,
            toolVersion = version,
            createdUtc = DateTime.UtcNow,
            os = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            sources,
            artifacts,
            acceptance = "Consult the separately recorded acceptance matrix; hashes alone do not certify target platforms."
        });
        Console.WriteLine("发布记录：" + path);
    }

    internal static void CheckRelease(string folder)
    {
        folder = Path.GetFullPath(folder);
        using var receipt = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "release-receipt.json")));
        Dictionary<string, string> expected = receipt.RootElement.GetProperty("artifacts").Deserialize<Dictionary<string, string>>()!;
        Dictionary<string, string> actual = Fingerprints(folder, allowInternalFileLinks: true);
        actual.Remove("release-receipt.json");
        Maintenance.Require(expected.OrderBy(p => p.Key).SequenceEqual(actual.OrderBy(p => p.Key)), "发布产物已变化、缺失或混入额外文件。");
        Console.WriteLine($"通过：{actual.Count} 个发布文件与记录完全一致。");
    }

    internal static void VerifyPlayer(string[] options)
    {
        Maintenance.Require(options.Length == 1 && options[0] is "mono" or "il2cpp" or "android" or "ios" or "webgl", "verify-player <mono|il2cpp|android|ios|webgl>；设置 UNITY_EDITOR 指向真实 Unity Editor。");
        string editor = Environment.GetEnvironmentVariable("UNITY_EDITOR") ?? throw new ArgumentException("请设置 UNITY_EDITOR。");
        Maintenance.Require(File.Exists(editor), "UNITY_EDITOR 不存在。");
        Maintenance.Require(OperatingSystem.IsMacOS() || OperatingSystem.IsWindows(), "自动运行入口目前覆盖 macOS/Windows 主机 Player。");
        string run = Maintenance.At("artifacts/acceptance/" + DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ") + "-" + options[0] + "-" + Guid.NewGuid().ToString("N")[..6]);
        Directory.CreateDirectory(run);
        string temporary = Maintenance.PhysicalPath(Maintenance.Temporary("player-acceptance"));
        try
        {
            string requested = options[0];
            bool host = requested is "mono" or "il2cpp";
            string target = requested switch
            {
                "android" => "Android",
                "ios" => "iOS",
                "webgl" => "WebGL",
                _ => OperatingSystem.IsMacOS() ? "OSXUniversal" : "Win64"
            };
            string backend = host ? requested : "il2cpp";
            string module = requested switch
            {
                "android" => "AndroidPlayer",
                "ios" => "iOSSupport",
                "webgl" => "WebGLSupport",
                _ => OperatingSystem.IsMacOS() ? "MacStandaloneSupport" : "WindowsStandaloneSupport"
            };
            string playback = OperatingSystem.IsMacOS() ? Path.GetFullPath(Path.Combine(Path.GetDirectoryName(editor)!, "../PlaybackEngines")) : Path.Combine(Path.GetDirectoryName(editor)!, "Data/PlaybackEngines");
            string hubPlayback = OperatingSystem.IsMacOS() ? Path.GetFullPath(Path.Combine(Path.GetDirectoryName(editor)!, "../../../PlaybackEngines")) : playback;
            Maintenance.Require(Directory.Exists(Path.Combine(playback, module)) || Directory.Exists(Path.Combine(hubPlayback, module)), "缺少 Unity 构建模块：" + module + "；请通过 Unity Hub 为同一 Editor 安装模块。此状态不是平台验收通过。");
            string project = Path.Combine(temporary, "UnityProject");
            const string Prefix = "unity/ZrConfig.Sample/";
            foreach (string relative in ReleaseFiles.SourceFiles(Maintenance.Root).Where(p => p.StartsWith(Prefix, StringComparison.Ordinal)))
            {
                string destination = Path.Combine(project, relative[Prefix.Length..]);
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(Maintenance.At(relative), destination);
            }

            Maintenance.Build("src/ZrConfig.Cli", "Release");
            string cli = Maintenance.At("src/ZrConfig.Cli/bin/Release/net10.0/ZrConfig.Cli.dll");
            string output = Path.Combine(temporary, "Output");
            Maintenance.RunDotNet([cli, "export", Maintenance.At("samples/Data"), output, Path.Combine(project, "Assets/HotUpdate/Config"), Path.Combine(project, "Assets/Editor/ConfigRules"), "--languages", "all", "--format", "both", "--generate", "--accept-changes"]);
            UnitySample.DeployData(output, Path.Combine(project, "Assets/StreamingAssets/ZrConfig"));
            string player = Path.Combine(run, requested switch
            {
                "android" => "ZrConfigAcceptance.apk",
                "ios" => "XcodeProject",
                "webgl" => "WebGL",
                _ => OperatingSystem.IsMacOS() ? "ZrConfigAcceptance.app" : "ZrConfigAcceptance.exe"
            });
            RunLong(editor, ["-batchmode", "-nographics", "-projectPath", project, "-buildTarget", target,
                "-executeMethod", "ZrConfig.Sample.Editor.PlayerBuild.BuildAcceptance", "-zrPlayerOutput", player, "-zrBackend", backend, "-logFile", Path.Combine(run, "build.log")], temporary, 1800);
            if (!host)
            {
                Maintenance.WriteJson(Path.Combine(run, "device-pending.json"), new
                {
                    status = "awaiting-device-run",
                    target,
                    backend,
                    architecture = requested == "webgl" ? "wasm" : "arm64",
                    artifact = Path.GetFileName(player),
                    unityEditor = editor,
                    report = "zrconfig-acceptance.json in persistentDataPath; console marker ZRCONFIG_ACCEPTANCE_REPORT; in-app result panel"
                });
                Console.WriteLine("构建完成，设备/浏览器验收尚未执行：" + player);
                return;
            }
            string executable = OperatingSystem.IsMacOS() ? Path.Combine(player, "Contents/MacOS", "ZrConfig Sample") : player;
            if (OperatingSystem.IsMacOS() && !File.Exists(executable))
            {
                executable = Directory.GetFiles(Path.Combine(player, "Contents/MacOS")).Single();
            }

            string report = Path.Combine(run, "player.json");
            RunLong(executable, ["-batchmode", "-nographics", "-zrAcceptanceReport", report, "-logFile", Path.Combine(run, "player.log")], run, 300);
            using var result = JsonDocument.Parse(File.ReadAllText(report));
            Maintenance.Require(result.RootElement.GetProperty("success").GetBoolean(), "Player 验证失败，查看 " + report);
            Console.WriteLine("真实 Player 验证通过：" + report);
        }
        catch (Exception error)
        {
            Maintenance.WriteJson(Path.Combine(run, "failure.json"), new
            {
                success = false,
                requestedTarget = options[0],
                error = error.Message,
                recordedUtc = DateTime.UtcNow
            });
            throw;
        }
        finally
        {
            Directory.Delete(temporary, recursive: true);
        }
    }

    private static void RunLong(string executable, string[] arguments, string cwd, int timeoutSeconds)
    {
        var start = new ProcessStartInfo(executable) { WorkingDirectory = cwd, UseShellExecute = false };
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }
        using Process process = Process.Start(start) ?? throw new IOException("无法启动：" + executable);
        if (!process.WaitForExit(timeoutSeconds * 1000))
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
            throw new TimeoutException("构建/运行超时，请查看本次报告。");
        }

        Maintenance.Require(process.ExitCode == 0, "真实进程失败，退出码：" + process.ExitCode);
    }

    private static Dictionary<string, string> Fingerprints(string root, bool allowInternalFileLinks = false)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        void Visit(string folder)
        {
            foreach (string entry in Directory.EnumerateFileSystemEntries(folder))
            {
                string relative = Path.GetRelativePath(root, entry).Replace('\\', '/');
                if (Maintenance.IsLink(entry))
                {
                    var link = new FileInfo(entry);
                    FileSystemInfo? target = link.ResolveLinkTarget(returnFinalTarget: true);
                    Maintenance.Require(allowInternalFileLinks && target is FileInfo && target.Exists &&
                        target.FullName.StartsWith(Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar, StringComparison.Ordinal),
                        "仅支持发布目录内部的文件链接：" + entry);
                    result.Add(relative, "symlink:" + link.LinkTarget + ";sha256:" + Hash(target!.FullName));
                    continue;
                }
                if (Directory.Exists(entry))
                {
                    Visit(entry);
                }
                else
                {
                    result.Add(relative, Hash(entry));
                }
            }
        }

        Visit(root);
        return result;
    }

    private static string Hash(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
