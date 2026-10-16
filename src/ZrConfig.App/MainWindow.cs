using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ZrConfig.Core;
using ZrConfig.Runtime;

namespace ZrConfig.App;

public sealed partial class MainWindow : Window
{
    private readonly string? _workspaceSettingsPath;
    private readonly TextBox _dataFolder;
    private readonly TextBox _outputFolder;
    private readonly TextBox _rulesFolder;
    private readonly TextBox _generatedFolder;
    private readonly TextBlock _generatedLabel = new();
    private readonly Button _generateButton = new()
    {
        MinWidth = 160
    };
    private readonly ColoredLogView _logBox;
    private readonly TextBlock _status = new()
    {
        TextWrapping = TextWrapping.Wrap
    };
    private readonly TextBlock _description = new()
    {
        TextWrapping = TextWrapping.Wrap
    };
    private readonly TextBlock _dataLabel = new();
    private readonly TextBlock _outputLabel = new();
    private readonly TextBlock _rulesLabel = new();
    private readonly TextBlock _languageLabel = new()
    {
        VerticalAlignment = VerticalAlignment.Center
    };
    private readonly TextBlock _settingsNotice = new()
    {
        TextWrapping = TextWrapping.Wrap,
        IsVisible = false
    };
    private readonly ComboBox _languagePicker;
    private readonly TextBlock _targetLabel = new()
    {
        VerticalAlignment = VerticalAlignment.Center
    };
    private readonly ComboBox _targetPicker;
    private readonly TextBlock _formatLabel = new() { VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock _codeLanguagesLabel = new() { VerticalAlignment = VerticalAlignment.Center };
    private readonly ComboBox _formatPicker;
    private readonly ComboBox _runtimeFamilyPicker;
    private readonly TextBlock _runtimeFamilyLabel = new() { VerticalAlignment = VerticalAlignment.Center };
    private readonly CheckBox[] _codeLanguageChecks;
    private static readonly string[] CodeLanguageNames = { "csharp", "java", "go", "cpp", "lua", "typescript", "rust" };
    private readonly Button _exportButton = new()
    {
        MinWidth = 180
    };
    private string _language = LanguageSettings.Load(LanguageSettings.DefaultPath);
    private string? _logLanguage;
    private bool _logCleared;
    private LogTone _statusTone;
    private Func<string> _statusText = static () => ConfigText.Get("就绪。点击校验并导出；需要生成代码时会先列出变更供确认。",
        "Ready. Validate & Export; any required generation will be shown for confirmation first.");
    public MainWindow() : this(WorkspaceSettings.DefaultPath)
    {
    }

    internal MainWindow(string? workspaceSettingsPath)
    {
        _workspaceSettingsPath = workspaceSettingsPath;
        WorkspaceSettings? workspace = workspaceSettingsPath == null ? null : WorkspaceSettings.Load(workspaceSettingsPath);
        _generationNamespaces=workspace?.Namespaces;
        const double WindowPadding = 20;
        const double SectionSpacing = 8;
        const double MinimumLogHeight = 160;
        const double ReservedLogSpace = WindowPadding * 2 + SectionSpacing + MinimumLogHeight + 52;
        Width = 1000;
        Height = 860;
        MinWidth = 720;
        MinHeight = 500;
        StackPanel panel = new()
        {
            Spacing = 8
        };
        panel.Children.Add(new TextBlock { Text = "ZrConfig", FontSize = 28, FontWeight = FontWeight.Bold });
        StackPanel languagePanel = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };
        languagePanel.Children.Add(_languageLabel);
        _languagePicker = new ComboBox
        {
            ItemsSource = new[]
            {
                "中文",
                "English"
            },
            SelectedIndex = _language == ConfigText.English ? 1 : 0,
            MinWidth = 140
        };
        languagePanel.Children.Add(_languagePicker);
        languagePanel.Children.Add(_targetLabel);
        _targetPicker = new ComboBox
        {
            MinWidth = 180,
            ItemsSource = new[]
            {
                "C · 客户端",
                "S · 服务端"
            },
            SelectedIndex = workspace?.Target == ConfigTarget.Server ? 1 : 0
        };
        languagePanel.Children.Add(_targetPicker);
        panel.Children.Add(languagePanel);
        var exportOptions = new WrapPanel { Orientation = Orientation.Horizontal };
        exportOptions.Children.Add(_formatLabel);
        _formatPicker = new ComboBox { ItemsSource = new[] { "Binary", "JSON", "Binary + JSON" }, SelectedIndex = workspace?.DataFormat == "json" ? 1 : workspace?.DataFormat == "both" ? 2 : 0, MinWidth = 150, Margin = new Avalonia.Thickness(10, 0, 20, 0) };
        exportOptions.Children.Add(_formatPicker);
        exportOptions.Children.Add(_runtimeFamilyLabel);
        _runtimeFamilyPicker = new ComboBox { ItemsSource = new[] { "Auto", "Legacy v1", "Server v1" }, SelectedIndex = workspace?.RuntimeFamily == "legacy-v1" ? 1 : workspace?.RuntimeFamily == "server-v1" ? 2 : 0, MinWidth = 130, Margin = new Avalonia.Thickness(10, 0, 20, 0) };
        exportOptions.Children.Add(_runtimeFamilyPicker);
        exportOptions.Children.Add(_codeLanguagesLabel);
        string[] languageLabels = { "C#", "Java", "Go", "C++", "Lua", "TypeScript", "Rust" };
        _codeLanguageChecks = CodeLanguageNames.Select((code, index) => new CheckBox { Content = languageLabels[index], IsChecked = (workspace?.CodeLanguages ?? new[] { "csharp" }).Contains(code), Margin = new Avalonia.Thickness(10, 0, 0, 0) }).ToArray();
        foreach (var check in _codeLanguageChecks) exportOptions.Children.Add(check);
        panel.Children.Add(exportOptions);
        panel.Children.Add(_settingsNotice);
        _description.IsVisible = workspace == null;
        panel.Children.Add(_description);
        var directories = new StackPanel { Spacing = 6 };
        directories.Children.Add(_dataLabel);
        string baseDirectory = ToolPaths.DefaultBaseDirectory;
        _dataFolder = new TextBox
        {
            Text = workspace?.DataFolder ?? Path.Combine(baseDirectory, "Data")
        };
        directories.Children.Add(CreateDirectoryInput(_dataFolder));
        directories.Children.Add(_outputLabel);
        _outputFolder = new TextBox
        {
            Text = workspace?.OutputFolder ?? Path.Combine(baseDirectory, "Output")
        };
        directories.Children.Add(CreateDirectoryInput(_outputFolder));
        directories.Children.Add(_generatedLabel);
        _generatedFolder = new TextBox
        {
            Text = workspace?.CodeFolder ?? ToolPaths.DefaultGeneratedDirectory
        };
        directories.Children.Add(CreateDirectoryInput(_generatedFolder));
        directories.Children.Add(_rulesLabel);
        _rulesFolder = new TextBox
        {
            Text = workspace?.RulesFolder ?? ToolPaths.DefaultRulesDirectory
        };
        directories.Children.Add(CreateDirectoryInput(_rulesFolder));
        _directorySettings.Content = directories;
        panel.Children.Add(CreateWorkspaceHeader());
        panel.Children.Add(_directorySettings);
        DirectoryDrop.Enable(_dataFolder);
        DirectoryDrop.Enable(_outputFolder);
        DirectoryDrop.Enable(_generatedFolder);
        DirectoryDrop.Enable(_rulesFolder);
        StackPanel buttons = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };
        _exportButton.Click += async (_, _) => await ExportAsync();
        _generateButton.Click += async (_, _) => await GenerateModelsAsync();
        buttons.Children.Add(_validateButton);
        buttons.Children.Add(_exportButton);
        buttons.Children.Add(_generateButton);
        buttons.Children.Add(_cancelButton);
        panel.Children.Add(buttons);
        panel.Children.Add(_status);
        panel.Children.Add(_operationProgress);
        panel.Children.Add(_progressText);
        _logBox = new ColoredLogView
        {
            DeferWhileDetached = true,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _logBox.Cleared += (_, _) =>
        {
            _logLanguage = null;
            _logCleared = true;
        };
        var settings = new ScrollViewer
        {
            Content = panel,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Margin = new Avalonia.Thickness(0, 0, 0, SectionSpacing),
            MaxHeight = Height - ReservedLogSpace
        };
        var layout = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            Margin = new Avalonia.Thickness(WindowPadding)
        };
        layout.Children.Add(settings);
        _diagnosticTab.Content = _diagnosticView;
        _logTab.Content = _logBox;
        _resultTabs.ItemsSource = new[] { _diagnosticTab, _logTab };
        Grid.SetRow(_resultTabs, 1);
        layout.Children.Add(_resultTabs);
        Content = layout;
        // 小窗口仅滚动设置区；日志始终取得有限的剩余高度，保持可见行排版。
        SizeChanged += (_, _) => settings.MaxHeight = Math.Max(0, ClientSize.Height - ReservedLogSpace);
        _languagePicker.SelectionChanged += (_, _) => ChangeLanguage();
        _targetPicker.SelectionChanged += (_, _) => { if (!_openingProject) { SaveWorkspace(); } };
        Closed += (_, _) => SaveWorkspace();
        ActualThemeVariantChanged += (_, _) => RefreshStatusColor();
        InitializeWorkspace(workspace);
        RefreshLanguage();
    }

    private void ChangeLanguage()
    {
        _language = _languagePicker.SelectedIndex == 1 ? ConfigText.English : ConfigText.Chinese;
        _settingsNotice.IsVisible = false;
        try
        {
            LanguageSettings.Save(LanguageSettings.DefaultPath, _language);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 偏好保存失败不撤销本次语言切换。
            _settingsNotice.IsVisible = true;
        }

        RefreshLanguage();
    }

    private void RefreshLanguage()
    {
        using IDisposable language = ConfigText.UseLanguage(_language);
        string version = typeof(MainWindow).Assembly.GetName().Version!.ToString(3);
        Title = ConfigText.Get($"ZrConfig v{version} · 配置导表工具", $"ZrConfig v{version} · Configuration Builder");
        _languageLabel.Text = ConfigText.Get("界面语言", "Language");
        _formatLabel.Text = ConfigText.Get("数据格式", "Data format");
        _codeLanguagesLabel.Text = ConfigText.Get("生成语言", "Code languages");
        _runtimeFamilyLabel.Text = ConfigText.Get("接口家族", "API family");
        _targetLabel.Text = ConfigText.Get("生成 / 导出端", "Target");
        int selectedTarget = _targetPicker.SelectedIndex;
        bool wasOpeningProject = _openingProject;
        _openingProject = true;
        _targetPicker.ItemsSource = new[]
        {
            ConfigText.Get("C · 客户端", "C · Client"),
            ConfigText.Get("S · 服务端", "S · Server")
        };
        _targetPicker.SelectedIndex = selectedTarget;
        _openingProject = wasOpeningProject;
        _description.Text = ConfigText.Get("打开项目配置或选择下方目录，保存 Excel 后校验导出。需要生成代码时会提示确认。",
            "Open project settings or choose the directories below, save Excel, then validate and export. Required code generation will prompt for confirmation.");
        _dataLabel.Text = ConfigText.Get("Excel 输入目录", "Excel input directory");
        _outputLabel.Text = ConfigText.Get("输出目录", "Output directory");
        _generatedLabel.Text = ConfigText.Get("配表代码根目录（选择 Config）", "Config code root (select Config)");
        _generateButton.Content = ConfigText.Get("生成配表代码", "Generate Table Code");
        _rulesLabel.Text = ConfigText.Get("C# 规则目录（可选，留空仅做基础校验）", "C# rules directory (optional; leave blank for basic validation)");
        string directoryHint = ConfigText.Get("拖入文件夹，或拖入文件使用所在目录（多选须在同一目录）",
            "Drop a folder, or files from the same directory to use their parent directory");
        SetDirectoryHint(_dataFolder,
            directoryHint + ConfigText.Get("；递归读取子目录中的 .xlsx 和 .xls",
            "; includes .xlsx and .xls files in all subdirectories"));
        SetDirectoryHint(_outputFolder, directoryHint);
        SetDirectoryHint(_generatedFolder, directoryHint);
        SetDirectoryHint(_rulesFolder, directoryHint);
        _exportButton.Content = ConfigText.Get("校验并导出", "Validate & Export");
        RefreshProjectLanguage();
        RefreshWorkspaceLanguage();
        _settingsNotice.Text = ConfigText.Get("无法保存设置；下次启动可能需要重新设置。",
            "Settings could not be saved; you may need to set them again on the next launch.");
        _logBox.RefreshLanguage();
        RefreshStatus();
        RefreshLogHeader();
        if (_logLanguage == null && !_logCleared)
        {
            _logBox.SetText(ConfigText.Get("诊断信息和日志路径将显示在这里。", "Diagnostics and log paths will appear here."));
        }
    }

    private static void SetDirectoryHint(TextBox input, string hint)
    {
        input.PlaceholderText = hint;
        ToolTip.SetTip(input, hint);
    }

    private void RefreshStatus()
    {
        using IDisposable language = ConfigText.UseLanguage(_language);
        _status.Text = _statusText();
        RefreshStatusColor();
    }

    private void RefreshStatusColor()
    {
        if (ColoredLogView.ColorFor(_statusTone, ActualThemeVariant) is { } color)
        {
            _status.Foreground = color;
        }
        else
        {
            _status.ClearValue(TextBlock.ForegroundProperty);
        }
    }

    private void RefreshLogHeader()
    {
        using IDisposable language = ConfigText.UseLanguage(_language);
        string? header = null;
        if (_logLanguage != null)
        {
            string name = _logLanguage == ConfigText.English ? ConfigText.Get("英文", "English") : ConfigText.Get("中文", "Chinese");
            header = ConfigText.Get($"以下记录使用{name}；后续操作使用当前所选语言。",
                $"The following record is in {name}; future operations use the selected language.");
        }

        _logBox.SetHeader(header);
    }

    private void SetLog(string text, string logLanguage, LogTone tone = LogTone.Information)
    {
        _logCleared = false;
        _logLanguage = logLanguage;
        _logBox.SetText(text, tone);
        RefreshLogHeader();
    }

    private void SetBusy(bool busy)
    {
        if (busy)
        {
            _statusTone = LogTone.Information;
        }

        SetOperationBusy(busy);
        _exportButton.IsEnabled = !busy;
        _generateButton.IsEnabled = !busy;
        _logBox.IsClearEnabled = !busy;
        _generatedFolder.IsEnabled = !busy;
        _dataFolder.IsEnabled = !busy;
        _outputFolder.IsEnabled = !busy;
        _rulesFolder.IsEnabled = !busy;
        _targetPicker.IsEnabled = !busy;
        _formatPicker.IsEnabled = !busy;
        _runtimeFamilyPicker.IsEnabled = !busy;
        foreach (var check in _codeLanguageChecks) check.IsEnabled = !busy;
        foreach (Button button in _directoryButtons)
        {
            button.IsEnabled = !busy;
        }

        _openProjectButton.IsEnabled = !busy;
        _saveProjectButton.IsEnabled = !busy;
    }

    private void SaveWorkspace()
    {
        if (_workspaceSettingsPath == null)
        {
            return;
        }

        try
        {
            ReadWorkspace().Save(_workspaceSettingsPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            _settingsNotice.IsVisible = true;
        }
    }

    private Task GenerateModelsAsync() => RunOperationAsync("generate");
    private Task ExportAsync() => RunOperationAsync("export");
}
