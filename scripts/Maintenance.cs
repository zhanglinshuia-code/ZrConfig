using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ZrConfig.Maintenance;

internal static partial class Maintenance
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        IndentSize = 4,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    public static readonly string[] BuildOptions = ["--disable-build-servers", "-m:1", "-p:UseSharedCompilation=false", "-p:NuGetAudit=false", "-p:UsedAvaloniaProducts="];
    public static readonly string Root = FindRoot();
    public static readonly string DotNet = FindDotNet();
    public static string At(string path) => Path.GetFullPath(path, Root);
    public static int Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        try
        {
            if (args.Length == 0 || args[0] is "help" or "--help")
            {
                Console.WriteLine(Help);
                return 0;
            }

            bool handled = false;
            RunLocalCommand(args, ref handled);
            if (handled)
            {
                return 0;
            }

            string[] options = args[1..];
            switch (args[0])
            {
                case "server-lua":
                    ServerRuntimeChecks.InstallLua(options);
                    break;
                case "verify-source":
                    NoOptions(options);
                    DeliveryChecks.VerifySource();
                    break;
                case "verify-player":
                    DeliveryChecks.VerifyPlayer(options);
                    break;
                case "release-record":
                    Require(options.Length == 1, "release-record <发布目录>");
                    DeliveryChecks.RecordRelease(options[0]);
                    break;
                case "release-check":
                    Require(options.Length == 1, "release-check <发布目录>");
                    DeliveryChecks.CheckRelease(options[0]);
                    break;
                case "check-portability":
                    NoOptions(options);
                    ReleaseFiles.CheckPortable(Root);
                    break;
                case "package-source":
                    ReleaseFiles.PackageSource(options);
                    break;
                case "install-source":
                    ReleaseFiles.InstallSource(options);
                    break;
                case "check-package-licenses":
                    Require(options.Length == 1, "check-package-licenses <发布目录>");
                    ReleaseFiles.CheckPackageLicenses(Path.GetFullPath(options[0]));
                    break;
                case "clean":
                    FlagOnly(options, "--outputs");
                    Clean(Root, options.Length > 0);
                    break;
                case "package-unity":
                    Packages.Unity(options);
                    break;
                case "package-windows":
                    Packages.Windows(options);
                    break;
                case "unity-sample":
                    NoOptions(options);
                    UnitySample.Prepare();
                    break;
                case "package-rules-sdk":
                    Packages.RulesSdk(options);
                    break;
                case "format-json":
                    Require(options.Length == 1, "format-json 需要一个 JSON 文件路径。");
                    using (var json = JsonDocument.Parse(File.ReadAllText(Path.GetFullPath(options[0]))))
                    {
                        Console.WriteLine(JsonSerializer.Serialize(json.RootElement, JsonOptions));
                    }

                    break;
                default:
                    throw new ArgumentException("未知维护命令：" + args[0] + "\n" + Help);
            }

            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
    }

    static partial void RunLocalCommand(string[] args, ref bool handled);
    private const string Help = """
        ZrConfig 维护工具（.NET SDK 10；Windows / macOS）
        bash scripts/maintain.sh <命令> [参数]
        powershell -File scripts/maintain.ps1 <命令> [参数]
          verify-source                      公开源码独立回归（不依赖内部 tests）
          server-lua <5.4.0|5.4.9> <新临时目录>  下载校验并编译独立 Lua 验收工具链
          verify-player <mono|il2cpp|android|ios|webgl>  隔离构建验收包；桌面自动运行，移动/WebGL 待设备验收；需 UNITY_EDITOR
          release-record <发布目录>          保存源码/产物哈希与工具版本，不覆盖旧记录
          release-check <发布目录>           检查产物与发行记录一致
          check-portability                  检查源码与附件中的本机路径及发布文件
          package-source [新ZIP文件]          打包源码、文档、Excel 与 Unity 示例
          install-source <ZIP文件> <新目录>   解压源码包，不覆盖已有目录
          check-package-licenses <发布目录>   校验实际发布依赖的许可覆盖
          clean [--outputs]                  清理构建缓存；--outputs 还会删除导出和发布产物
          package-unity [新输出目录]         导出 Unity Assets 源码（无 DLL/Package）
          package-windows [win-x64|win-arm64] 自包含 App/CLI，保留旧包；可交叉打包
          unity-sample                      准备 Unity 示例：Assets 配置源码、通用运行时和桌面数据
          package-rules-sdk [工具DLL目录] [新输出目录]
          format-json <JSON文件>             格式化显示诊断/报告
        """;
    public static void NoOptions(string[] args) => Require(args.Length == 0, "此命令不接受参数。");
    public static void FlagOnly(string[] args, string flag) => Require(args.Length == 0 || args.SequenceEqual([flag]), "仅支持可选参数 " + flag);
    public static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void WriteJson(string file, object data) => WriteText(file, JsonSerializer.Serialize(data, JsonOptions) + "\n");
    public static void WriteText(string file, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(file))!);
        File.WriteAllText(file, text.ReplaceLineEndings("\n"), new UTF8Encoding(false));
    }

    public static void Build(string project,
        string configuration = "Debug") => RunDotNet(["build", project, "-c", configuration, .. BuildOptions]);
    public static string RunDotNet(IEnumerable<string> arguments,
        string? cwd = null,
        int expected = 0,
        bool capture = false) => Run(DotNet,
        arguments,
        cwd ?? Root,
        expected,
        capture);
    public static string Run(string executable, IEnumerable<string> arguments, string cwd, int expected = 0, bool capture = false)
    {
        var start = new ProcessStartInfo(executable)
        {
            WorkingDirectory = cwd,
            UseShellExecute = false,
            RedirectStandardOutput = capture,
            RedirectStandardError = capture
        };
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(start) ?? throw new IOException("无法启动 " + executable);
        Task<string>? output = capture ? process.StandardOutput.ReadToEndAsync() : null;
        Task<string>? error = capture ? process.StandardError.ReadToEndAsync() : null;
        if (!process.WaitForExit(300_000))
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
            throw new TimeoutException($"进程超过 300 秒：{executable} {string.Join(' ', start.ArgumentList)}");
        }

        string text = capture ? output!.GetAwaiter().GetResult() + error!.GetAwaiter().GetResult() : "";
        Require(process.ExitCode == expected,
            $"进程退出码 {process.ExitCode}，预期 {expected}：{executable} {string.Join(' ', start.ArgumentList)}\n{text}");
        return text;
    }

    public static IEnumerable<string> Entries(string directory) => Directory.EnumerateFileSystemEntries(directory)
        .Order(StringComparer.Ordinal);
    public static bool IsLink(string path) => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
    public static void CopyTree(string source, string target, Func<string, bool>? include = null)
    {
        Require(!IsLink(source), "不复制链接目录：" + source);
        Directory.CreateDirectory(target);
        Require(!IsLink(target), "输出目录不能是链接：" + target);
        foreach (string entry in Entries(source))
        {
            string name = Path.GetFileName(entry);
            if (include != null && !include(name))
            {
                continue;
            }

            Require(!IsLink(entry), "不复制链接：" + entry);
            string destination = Path.Combine(target, name);
            if (File.Exists(destination) || Directory.Exists(destination))
            {
                Require(!IsLink(destination), "输出不能包含链接：" + destination);
            }

            if (Directory.Exists(entry))
            {
                CopyTree(entry, destination, include);
            }
            else
            {
                File.Copy(entry, destination, overwrite: true);
            }
        }
    }

    // Resolve every existing ancestor, including a link above a not-yet-created destination.
    public static string PhysicalPath(string path)
    {
        string full = Path.GetFullPath(path);
        string current = Path.GetPathRoot(full)!;
        foreach (string part in full[current.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, part);
            if (Directory.Exists(current) || File.Exists(current))
            {
                FileSystemInfo info = Directory.Exists(current) ? (FileSystemInfo)new DirectoryInfo(current) : new FileInfo(current);
                if (info.LinkTarget != null)
                {
                    current = info.ResolveLinkTarget(true)?.FullName ?? throw new IOException("无法解析链接：" + current);
                }
            }
        }

        return Path.TrimEndingDirectorySeparator(current);
    }

    public static bool Within(string path, string parent)
    {
        path = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        parent = Path.TrimEndingDirectorySeparator(Path.GetFullPath(parent));
        // Conservatively reject case aliases on macOS/Windows volumes that ignore case.
        string prefix = Path.EndsInDirectorySeparator(parent) ? parent : parent + Path.DirectorySeparatorChar;
        return path.Equals(parent, StringComparison.OrdinalIgnoreCase) || path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    public static void NoOverlap(string source, string destination)
    {
        source = PhysicalPath(source);
        destination = PhysicalPath(destination);
        Require(!Within(source, destination) && !Within(destination, source), "输出目录不能与来源目录重叠。");
    }

    public static string Temporary(string name)
    {
        string path = Path.Combine(Path.GetTempPath(), "zrconfig-" + name + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    public static void Clean(string root, bool outputs)
    {
        int count = 0;
        foreach (string name in new[]
        {
            "src",
            "tests",
            "samples",
            "unity",
            "artifacts"
        }

        )
        {
            string path = Path.Combine(root, name);
            if (Directory.Exists(path) && !IsLink(path))
            {
                Visit(path);
            }
        }

        RemoveFinder(root);
        // The maintenance assembly stays loaded on Windows, so its own scripts/bin and obj are kept.
        if (outputs)
        {
            foreach (string path in new[]
            {
                Path.Combine(root, "samples", "Output"),
                Path.Combine(root, "artifacts")
            }

            )
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, !IsLink(path));
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        Console.WriteLine($"已清理 {count} 个构建/编译缓存目录；保留维护工具自身的 scripts/bin 与 scripts/obj。");
        void Visit(string directory)
        {
            foreach (string entry in Entries(directory))
            {
                if (!Directory.Exists(entry) || IsLink(entry))
                {
                    continue;
                }

                if (Path.GetFileName(entry) is "bin" or "obj" or ".types-cache" or ".rules-cache")
                {
                    Directory.Delete(entry, true);
                    count++;
                }
                else
                {
                    Visit(entry);
                }
            }
        }

        void RemoveFinder(string directory)
        {
            foreach (string entry in Entries(directory))
            {
                if (IsLink(entry))
                {
                    continue;
                }

                if (Path.GetFileName(entry) == ".DS_Store" && File.Exists(entry))
                {
                    File.Delete(entry);
                }
                else if (Directory.Exists(entry) && Path.GetFileName(entry) != ".git")
                {
                    RemoveFinder(entry);
                }
            }
        }
    }

    private static string FindRoot()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ZrConfig.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("请从 ZrConfig 源码仓库运行维护工具。");
    }

    private static string FindDotNet()
    {
        foreach (string name in new[]
        {
            "DOTNET_BIN",
            "DOTNET_HOST_PATH"
        }

        )
        {
            if (Environment.GetEnvironmentVariable(name) is { Length: > 0 } value)
            {
                return value;
            }
        }

        string host = Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(),
            "../../..",
            OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet"));
        return File.Exists(host) ? host : "dotnet";
    }
}
