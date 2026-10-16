using System.Security.Cryptography;

namespace ZrConfig.Maintenance;

internal static class Packages
{
    public static void Windows(string[] args)
    {
        Maintenance.Require(args.Length <= 1 && (args.Length == 0 || args[0] is "win-x64" or "win-arm64"),
            "package-windows [win-x64|win-arm64]");
        string rid = args.Length == 0 ? "win-x64" : args[0];
        string output = Maintenance.At("artifacts/windows/" + rid);
        Directory.CreateDirectory(output);
        string stage = Path.Combine(output, ".package-" + Guid.NewGuid().ToString("N"));
        string bundle = Path.Combine(output, "ZrConfig");
        string previous = Path.Combine(output,
            "ZrConfig.previous-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid()
            .ToString("N"));
        try
        {
            foreach (string project in new[]
            {
                "ZrConfig.App",
                "ZrConfig.Cli"
            }

            )
            {
                Maintenance.RunDotNet(["publish", $"src/{project}/{project}.csproj", "-c", "Release", "-r", rid, "--self-contained", "true", "-p:UseAppHost=true", "-p:PublishSingleFile=false", "-p:PublishTrimmed=false", .. Maintenance.BuildOptions, "-o", stage]);
            }

            foreach (string name in new[]
            {
                "ZrConfig.App.exe",
                "ZrConfig.Cli.exe",
                "ExcelDataReader.dll",
                "System.Text.Encoding.CodePages.dll"
            }

            )
            {
                Maintenance.Require(File.Exists(Path.Combine(stage, name)), "缺少 Windows 发布文件：" + name);
            }

            ReleaseFiles.CheckPackageLicenses(stage);
            if (Directory.Exists(bundle))
            {
                Directory.Move(bundle, previous);
            }

            Directory.Move(stage, bundle);
            Console.WriteLine("Windows 包：" + bundle + "；请在 Windows 运行 App/CLI 验证。");
            if (Directory.Exists(previous))
            {
                Console.WriteLine("旧包保留：" + previous);
            }
        }
        finally
        {
            if (!Directory.Exists(bundle) && Directory.Exists(previous))
            {
                Directory.Move(previous, bundle);
            }

            if (Directory.Exists(stage))
            {
                Directory.Delete(stage, true);
            }
        }
    }

    public static void Unity(string[] args)
    {
        Maintenance.Require(args.Length <= 1, "用法：package-unity [输出目录]");
        string source = Maintenance.At("unity/ZrConfig.Sample/Assets/ZrConfig");
        string output = args.Length == 1 ? Path.GetFullPath(args[0]) : Maintenance.At("artifacts/unity/ZrConfig-source");
        CopyUnity(source, output);
        Console.WriteLine("Unity 源码：" + Path.Combine(output, "Assets/ZrConfig") + "；将 Assets 内容放入目标 Unity 工程。");
    }

    internal static void CopyUnity(string source, string output)
    {
        Maintenance.NoOverlap(source, output);
        output = Maintenance.PhysicalPath(output);
        Maintenance.Require(!Directory.Exists(output) && !File.Exists(output), "输出目录已存在，请指定新目录，保留原有源码及发布内容。");
        string parent = Path.GetDirectoryName(output)!;
        Directory.CreateDirectory(parent);
        string stage = Path.Combine(parent, ".unity-source-" + Guid.NewGuid().ToString("N"));
        try
        {
            Maintenance.CopyTree(source,
                Path.Combine(stage,
                "Assets/ZrConfig"),
                name => !name.StartsWith('.') && name is not "bin" and not "obj" && !name.EndsWith(".dll",
                StringComparison.OrdinalIgnoreCase) && !name.EndsWith(".dll.meta",
                StringComparison.OrdinalIgnoreCase) && !name.EndsWith(".pdb",
                StringComparison.OrdinalIgnoreCase) && !name.EndsWith(".pdb.meta",
                StringComparison.OrdinalIgnoreCase));
            Maintenance.Require(!Maintenance.IsLink(source + ".meta"), "不复制链接元数据。");
            File.Copy(source + ".meta", Path.Combine(stage, "Assets/ZrConfig.meta"));
            File.Copy(Maintenance.At("LICENSE"), Path.Combine(stage, "LICENSE"));
            Directory.Move(stage, output);
        }
        finally
        {
            if (Directory.Exists(stage))
            {
                Directory.Delete(stage, true);
            }
        }
    }

    public static void RulesSdk(string[] args)
    {
        Maintenance.Require(args.Length <= 2, "用法：package-rules-sdk [工具DLL目录] [新输出目录]");
        string source = args.Length > 0 ? Path.GetFullPath(args[0]) : Maintenance.At("src/ZrConfig.App/bin/Release/net10.0");
        string output = args.Length > 1 ? Path.GetFullPath(args[1]) : Maintenance.At("artifacts/rules-sdk");
        CreateRulesSdk(source, output);
        Console.WriteLine("规则开发包：" + output);
    }

    internal static void CreateRulesSdk(string source, string output)
    {
        Maintenance.NoOverlap(source, output);
        string[] names = ["ZrConfig.Validation", "ZrConfig.Runtime"];
        Maintenance.Require(names.All(name => File.Exists(Path.Combine(source,
            name + ".dll"))),
            "请先构建 App，或指定已分发 App 的 DLL 目录（macOS：Contents/Resources）。");
        Maintenance.Require(!Directory.Exists(output) && !File.Exists(output), "输出目录已存在，请指定新目录，避免覆盖开发者规则。");
        string parent = Path.GetDirectoryName(output)!;
        Directory.CreateDirectory(parent);
        string stage = Path.Combine(parent, ".rules-sdk-" + Guid.NewGuid().ToString("N"));
        try
        {
            string libraries = Path.Combine(stage, "References");
            Directory.CreateDirectory(libraries);
            var manifest = new Dictionary<string, string>();
            foreach (string name in names)
            {
                string file = Path.Combine(source, name + ".dll");
                File.Copy(file, Path.Combine(libraries, name + ".dll"));
                manifest[name + ".dll"] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(file)));
                foreach (string suffix in new[]
                {
                    ".pdb",
                    ".xml"
                }

                )
                {
                    if (File.Exists(Path.Combine(source, name + suffix)))
                    {
                        File.Copy(Path.Combine(source, name + suffix), Path.Combine(libraries, name + suffix));
                    }
                }
            }

            Maintenance.WriteJson(Path.Combine(stage, "manifest.json"), manifest);
            File.Copy(Maintenance.At("LICENSE"), Path.Combine(stage, "LICENSE"));
            Directory.CreateDirectory(Path.Combine(stage, "Config"));
            Maintenance.WriteText(Path.Combine(stage, "Rules/ProjectRules.cs"), """
                using ZrConfig.Models;
                using ZrConfig.Validation;

                // 先生成到代码根目录 Config；替换为项目实际配置类型。
                public sealed class ProjectRules : TableRules<HeroConfig>
                {
                    protected override void Validate(HeroConfig row, ValidationContext context)
                    {
                        // context.Error(row, nameof(row.Id), "业务检查失败");
                    }
                }
                """ + "\n");
            Maintenance.WriteText(Path.Combine(stage, "Rules.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
                    <DebugType>embedded</DebugType>
                  </PropertyGroup>
                  <ItemGroup>
                    <Compile Include="Config/**/*.cs" />
                    <Compile Include="Rules/**/*.cs" />
                    <Reference Include="ZrConfig.Validation"><HintPath>References/ZrConfig.Validation.dll</HintPath></Reference>
                    <Reference Include="ZrConfig.Runtime"><HintPath>References/ZrConfig.Runtime.dll</HintPath></Reference>
                  </ItemGroup>
                </Project>
                """ + "\n");
            Maintenance.WriteText(Path.Combine(stage, "README.md"), """
                # ZrConfig 规则开发包

                本包包含通用 Runtime/Validation 引用与一个规则工程模板，采用随包 LICENSE 中的 MIT 许可。
                编译需要 .NET SDK 10；没有绑定开发者电脑的目录。

                1. 用配套版本的 ZrConfig 工具将自己的 Excel 生成到本包 Config 目录。
                2. 编辑 Rules/ProjectRules.cs，将示例 HeroConfig 替换为实际生成的表类型。
                3. 在本目录执行 `dotnet build Rules.csproj`。
                4. App/CLI 选择自己的 Data、本包 Config 和 Rules，以及自己的 Output 目录。

                Config 支持整个源码目录树；不要额外复制一套通用序列化支持源码。
                工具导出时读取并编译 Rules 中的源文件，无需把 Rules.dll 复制到工具目录。
                逐行、整表与跨表规则教程见完整源码仓库的 docs/rules-development.md；
                Excel 语法见 docs/config-types.md。需要调试动态规则时启用 CLI --rules-debug。
                manifest.json 记录 References 中两个 DLL 的 SHA-256，升级时应整体使用配套版本。
                """ + "\n");
            Directory.Move(stage, output);
        }
        finally
        {
            if (Directory.Exists(stage))
            {
                Directory.Delete(stage, true);
            }
        }
    }
}
