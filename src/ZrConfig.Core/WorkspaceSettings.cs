using System.Text.Json;
using System.Text.Json.Serialization;
using ZrConfig.Runtime;

namespace ZrConfig.Core;

public sealed record WorkspaceSettings(string DataFolder, string OutputFolder, string CodeFolder, string RulesFolder)
{
    [JsonConverter(typeof(ConfigTargetJsonConverter))]
    public ConfigTarget Target { get; init; } = ConfigTarget.Client;
    /// <summary>数组按内容比较，界面重新读取选项不会造成虚假的未保存状态。</summary>
    public bool SameConfiguration(WorkspaceSettings other) =>
        this == other;

    public string DataFormat { get; init; } = "binary";
    public string RuntimeFamily { get; init; } = "auto";
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]
    public GenerationNamespaces? Namespaces {get;init;}
    private readonly string _codeLanguages = "csharp";
    public string[] CodeLanguages
    {
        get => _codeLanguages.Split(',');
        init => _codeLanguages = string.Join(",", ExportOptions.Languages(value));
    }
    public string? Name
    {
        get; init;
    }
    public int FormatVersion { get; init; } = 1;
    public static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ZrConfig",
        "workspace.json");

    public static WorkspaceSettings? Load(string path)
    {
        try
        {
            WorkspaceSettings? settings = JsonSerializer.Deserialize<WorkspaceSettings>(File.ReadAllText(path));
            return settings is { DataFolder: not null, OutputFolder: not null, CodeFolder: not null, RulesFolder: not null } ? settings : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
        {
            return null;
        }
    }

    // Project files use paths relative to the JSON file, independently of the launch directory.
    public static WorkspaceSettings LoadProject(string path)
    {
        WorkspaceSettings? settings = JsonSerializer.Deserialize<WorkspaceSettings>(File.ReadAllText(path));
        if (settings is not { DataFolder: not null, OutputFolder: not null, CodeFolder: not null, RulesFolder: not null })
        {
            throw new InvalidDataException(ConfigText.Get("项目配置必须包含 DataFolder、OutputFolder、CodeFolder、RulesFolder 四个路径。",
                "Project settings must contain DataFolder, OutputFolder, CodeFolder and RulesFolder."));
        }

        if (settings.FormatVersion != 1)
        {
            throw new InvalidDataException("Unsupported project format version: " + settings.FormatVersion);
        }
        return settings.Normalize(Path.GetDirectoryName(Path.GetFullPath(path))!);
    }

    public WorkspaceSettings Normalize(string? baseDirectory = null) => Normalize(baseDirectory, generation: false);
    private WorkspaceSettings Normalize(string? baseDirectory, bool generation)
    {
        _ = ConfigTargets.Marker(Target);
        _ = ExportOptions.Format(DataFormat);
        _ = GenerationFamily.Validate(RuntimeFamily);
        _ = ExportOptions.Languages(CodeLanguages);
        baseDirectory ??= Environment.CurrentDirectory;
        string Resolve(string value, string label, bool optional = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (optional)
                {
                    return "";
                }

                throw new ArgumentException(ConfigText.Get($"请填写或浏览选择{label}。", $"Select a directory for {label}."));
            }

            return Path.GetFullPath(value.Trim(), baseDirectory);
        }

        // 单独生成只使用 Data/Config；保留未参与操作的路径，不解析或校验它们。
        return new(Resolve(DataFolder,
            ConfigText.Get("Excel 输入目录",
            "Excel input")),
            generation ? OutputFolder : Resolve(OutputFolder,
            ConfigText.Get("输出目录",
            "output"), optional: true),
            Resolve(CodeFolder,
            ConfigText.Get("Config 代码根目录",
            "Config code root")),
            generation ? RulesFolder : Resolve(RulesFolder,
            ConfigText.Get("规则目录",
            "rules"),
            optional: true))
        {
            Target = Target,
            DataFormat = ExportOptions.Format(DataFormat),
            RuntimeFamily = GenerationFamily.Validate(RuntimeFamily),
            Namespaces=Namespaces,
            CodeLanguages = ExportOptions.Languages(CodeLanguages),
            Name = Name,
            FormatVersion = FormatVersion
        };
    }

    public WorkspaceSettings Validate(bool generation, bool allowMissingCode = false)
    {
        WorkspaceSettings settings = Normalize(baseDirectory: null, generation);
        if (!generation && string.IsNullOrWhiteSpace(settings.OutputFolder))
        {
            throw new ArgumentException("Select an output directory.");
        }

        if (!Directory.Exists(settings.DataFolder))
        {
            throw new DirectoryNotFoundException(ConfigText.Get("Excel 输入目录不存在：",
                "Excel input directory does not exist: ") + settings.DataFolder);
        }

        if (File.Exists(settings.CodeFolder) || (!generation && File.Exists(settings.OutputFolder)))
        {
            throw new ArgumentException(ConfigText.Get("输出和代码路径必须是文件夹，不能选择文件。", "Output and code paths must be directories, not files."));
        }

        if (!generation && !allowMissingCode && !Directory.Exists(settings.CodeFolder))
        {
            throw new DirectoryNotFoundException(ConfigText.Get("Config 目录不存在，请先生成配表代码：",
                "Config does not exist. Generate table code first: ") + settings.CodeFolder);
        }

        if (!generation && settings.RulesFolder.Length != 0 && !Directory.Exists(settings.RulesFolder))
        {
            throw new DirectoryNotFoundException(ConfigText.Get("规则目录不存在；请重新选择项目规则目录：",
                "Rules directory does not exist. Select the project's rules directory: ") + settings.RulesFolder);
        }

        RequireSeparateDirectories(settings, includeOutput: !generation);
        return settings;
    }

    internal static void RequireSeparateDirectories(WorkspaceSettings settings, bool includeOutput)
    {
        bool Overlap(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }
            left = Path.TrimEndingDirectorySeparator(Path.GetFullPath(left));
            right = Path.TrimEndingDirectorySeparator(Path.GetFullPath(right));
            StringComparison comparison = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return left.Equals(right, comparison) || left.StartsWith(right + Path.DirectorySeparatorChar, comparison) || right.StartsWith(left + Path.DirectorySeparatorChar, comparison);
        }

        if (Overlap(settings.DataFolder, settings.CodeFolder) || (includeOutput &&
            new[] { settings.DataFolder, settings.CodeFolder, settings.RulesFolder }.Any(folder => Overlap(folder, settings.OutputFolder))))
        {
            throw new ArgumentException(ConfigText.Get("Data 与 Config 不能重叠；Output 不能与输入、代码或规则目录重叠。", "Data and Config must not overlap; Output must not overlap inputs, code or rules."));
        }
    }

    public WorkspaceSettings ValidateOnly()
    {
        WorkspaceSettings settings = Normalize(baseDirectory: null, generation: true);
        if (!Directory.Exists(settings.DataFolder))
        {
            throw new DirectoryNotFoundException("Excel input directory does not exist: " + settings.DataFolder);
        }

        string rules = string.IsNullOrWhiteSpace(RulesFolder) ? "" : Path.GetFullPath(RulesFolder.Trim());
        if (rules.Length != 0 && !Directory.Exists(rules))
        {
            throw new DirectoryNotFoundException("Rules directory does not exist: " + rules);
        }

        settings = settings with
        {
            RulesFolder = rules
        };
        RequireSeparateDirectories(settings, includeOutput: false);
        return settings;
    }

    public void SaveProject(string path)
    {
        WorkspaceSettings absolute = Normalize();
        string root = Path.GetDirectoryName(Path.GetFullPath(path))!;
        string Relative(string value) => value.Length == 0 ? "" : Path.GetRelativePath(root, value);
        Write(path,
            new WorkspaceSettings(Relative(absolute.DataFolder),
            Relative(absolute.OutputFolder),
            Relative(absolute.CodeFolder),
            Relative(absolute.RulesFolder))
            {
                Target = Target,
                DataFormat = ExportOptions.Format(DataFormat),
            RuntimeFamily = GenerationFamily.Validate(RuntimeFamily),
            Namespaces=Namespaces,
                CodeLanguages = ExportOptions.Languages(CodeLanguages),
                Name = Name,
                FormatVersion = FormatVersion
            });
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        // Relative paths must still resolve to the same project after a different launch directory.
        string FullPath(string value) => string.IsNullOrWhiteSpace(value) ? "" : Path.GetFullPath(value.Trim());
        var absolute = new WorkspaceSettings(FullPath(DataFolder), FullPath(OutputFolder), FullPath(CodeFolder), FullPath(RulesFolder))
        {
            Target = Target,
            DataFormat = ExportOptions.Format(DataFormat),
            RuntimeFamily = GenerationFamily.Validate(RuntimeFamily),
            Namespaces=Namespaces,
            CodeLanguages = ExportOptions.Languages(CodeLanguages),
            Name = Name,
            FormatVersion = FormatVersion
        };
        Write(path, absolute);
    }

    private static void Write(string path, WorkspaceSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true, IndentSize = 4 }) + "\n");
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }
}
