namespace ZrConfig.Core;

public static class ToolPaths
{
    public const string SampleCodeRelativeDirectory = "unity/ZrConfig.Sample/Assets/HotUpdate/Config";
    public const string SampleRulesRelativeDirectory = "unity/ZrConfig.Sample/Assets/Editor/ConfigRules";
    public static string DefaultBaseDirectory => FindProjectRoot() is { } project ? Path.Combine(project,
        "samples") : (IsMacAppBundle ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ZrConfig",
        "Workspace") : Environment.CurrentDirectory);
    // 正式示例共用 Unity Assets 中可直接生成、编辑的源码；独立 App 使用项目配置。
    public static string DefaultRulesDirectory => FindProjectRoot() is { } root ? Path.Combine(root, SampleRulesRelativeDirectory) : "";
    public static string DefaultGeneratedDirectory => FindProjectRoot() is { } root ? Path.Combine(root, SampleCodeRelativeDirectory) : "";
    private static bool IsMacAppBundle => OperatingSystem.IsMacOS() && new DirectoryInfo(AppContext.BaseDirectory).Parent is { Name: "Contents", Parent: { } bundle } && bundle.Name.EndsWith(".app",
        StringComparison.OrdinalIgnoreCase);

    private static string? FindProjectRoot()
    {
        // 优先按可执行文件定位，避免工作目录恰好属于另一份工程时选错规则。
        foreach (string start in new[]
        {
            AppContext.BaseDirectory,
            Environment.CurrentDirectory
        }

        )
        {
            for (DirectoryInfo? directory = new(start); directory != null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ZrConfig.slnx")))
                {
                    return directory.FullName;
                }
            }
        }

        return null;
    }
}
