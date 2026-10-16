using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using ZrConfig.Core;
using ZrConfig.Runtime;

namespace ZrConfig.App;

public sealed partial class MainWindow
{
    private readonly List<Button> _directoryButtons = [];
    private readonly Button _openProjectButton = new();
    private readonly Button _saveProjectButton = new();
    private static readonly FilePickerFileType s_projectFileType = new("ZrConfig project")
    {
        Patterns = ["*.zrconfig.json"]
    };
    private StackPanel CreateProjectButtons()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };
        _openProjectButton.Click += async (_, _) => await PickProjectAsync(save: false);
        _saveProjectButton.Click += async (_, _) => await PickProjectAsync(save: true);
        panel.Children.Add(_openProjectButton);
        panel.Children.Add(_saveProjectButton);
        return panel;
    }

    private Grid CreateDirectoryInput(TextBox input)
    {
        var button = new Button
        {
            Margin = new Thickness(8, 0, 0, 0),
            MinWidth = 80
        };
        button.Click += async (_, _) => await BrowseDirectoryAsync(input);
        _directoryButtons.Add(button);
        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        row.Children.Add(input);
        Grid.SetColumn(button, 1);
        row.Children.Add(button);
        return row;
    }

    private void RefreshProjectLanguage()
    {
        _openProjectButton.Content = ConfigText.Get("打开项目配置", "Open Project Settings");
        _saveProjectButton.Content = ConfigText.Get("保存项目配置", "Save Project Settings");
        foreach (Button button in _directoryButtons)
        {
            button.Content = ConfigText.Get("浏览…", "Browse...");
        }
    }

    private GenerationNamespaces? _generationNamespaces;
    private WorkspaceSettings ReadWorkspace() => new(_dataFolder.Text ?? "",
        _outputFolder.Text ?? "",
        _generatedFolder.Text ?? "",
        _rulesFolder.Text ?? "")
    {
        Target = _targetPicker.SelectedIndex == 1 ? ConfigTarget.Server : ConfigTarget.Client,
        DataFormat = _formatPicker.SelectedIndex == 1 ? "json" : _formatPicker.SelectedIndex == 2 ? "both" : "binary",
        RuntimeFamily = _runtimeFamilyPicker.SelectedIndex == 1 ? "legacy-v1" : _runtimeFamilyPicker.SelectedIndex == 2 ? "server-v1" : "auto",
        Namespaces=_generationNamespaces,
        CodeLanguages = CodeLanguageNames.Where((_, i) => _codeLanguageChecks[i].IsChecked == true).ToArray(),
        Name = _projectName.Text
    };
    internal void OpenProject(string path)
    {
        // Read and resolve every field before replacing any of the current paths.
        var settings = WorkspaceSettings.LoadProject(path);
        _openingProject = true;
        _projectPath = Path.GetFullPath(path);
        _projectName.Text = settings.Name ?? Path.GetFileNameWithoutExtension(path);
        _dataFolder.Text = settings.DataFolder;
        _outputFolder.Text = settings.OutputFolder;
        _generatedFolder.Text = settings.CodeFolder;
        _rulesFolder.Text = settings.RulesFolder;
        _generationNamespaces=settings.Namespaces;
        _targetPicker.SelectedIndex = settings.Target == ConfigTarget.Server ? 1 : 0;
        _formatPicker.SelectedIndex = settings.DataFormat == "json" ? 1 : settings.DataFormat == "both" ? 2 : 0;
        _runtimeFamilyPicker.SelectedIndex = settings.RuntimeFamily == "legacy-v1" ? 1 : settings.RuntimeFamily == "server-v1" ? 2 : 0;
        for (int i = 0; i < CodeLanguageNames.Length; i++) _codeLanguageChecks[i].IsChecked = settings.CodeLanguages.Contains(CodeLanguageNames[i]);
        _openingProject = false;
        RememberProject(path);
        _savedProject = ReadWorkspace();
        _lastWorkflow = null;
        _directorySettings.IsExpanded = false;
        RefreshProjectState();
        RefreshActiveVersionAsync();
        ResetInputWatchers();
        SaveWorkspace();
    }

    private async Task BrowseDirectoryAsync(TextBox input)
    {
        using IDisposable language = ConfigText.UseLanguage(_language);
        SetBusy(true);
        try
        {
            if (!StorageProvider.CanPickFolder)
            {
                throw new NotSupportedException(ConfigText.Get("当前环境不支持目录选择，请输入或拖入路径。",
                    "Folder picking is unavailable; type or drop a path."));
            }

            IReadOnlyList<IStorageFolder> folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = ConfigText.Get("选择目录",
                "Select directory"),
                AllowMultiple = false
            });
            if (folders.Count != 0 && folders[0].TryGetLocalPath() is { } path)
            {
                input.Text = path;
                SaveWorkspace();
            }
        }
        catch (Exception ex)
        {
            ShowProjectError(ex);
        }
        finally
        {
            SetBusy(false);
            RefreshStatus();
        }
    }

    private async Task PickProjectAsync(bool save)
    {
        using IDisposable language = ConfigText.UseLanguage(_language);
        SetBusy(true);
        try
        {
            if (save)
            {
                WorkspaceSettings settings = ReadWorkspace().Normalize();
                if (!StorageProvider.CanSave)
                {
                    throw new NotSupportedException(ConfigText.Get("当前环境不支持保存文件对话框。", "Save dialogs are unavailable."));
                }

                IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = ConfigText.Get("保存项目配置",
                    "Save project settings"),
                    SuggestedFileName = "project.zrconfig.json",
                    DefaultExtension = "zrconfig.json",
                    FileTypeChoices = [s_projectFileType],
                    ShowOverwritePrompt = true
                });
                if (file?.TryGetLocalPath() is not { } path)
                {
                    return;
                }

                settings.SaveProject(path);
                _projectPath = path;
                _savedProject = ReadWorkspace();
                RememberProject(path);
                RefreshProjectState();
                _statusText = () => ConfigText.Get($"已保存项目配置：{path}", $"Project settings saved: {path}");
            }
            else
            {
                if (!StorageProvider.CanOpen)
                {
                    throw new NotSupportedException(ConfigText.Get("当前环境不支持打开文件对话框。", "Open dialogs are unavailable."));
                }

                IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = ConfigText.Get("打开项目配置",
                    "Open project settings"),
                    AllowMultiple = false,
                    FileTypeFilter = [s_projectFileType]
                });
                if (files.Count == 0 || files[0].TryGetLocalPath() is not { } path)
                {
                    return;
                }

                OpenProject(path);
                _statusText = () => ConfigText.Get($"已打开项目配置：{path}", $"Project settings opened: {path}");
            }

            _statusTone = LogTone.Success;
        }
        catch (Exception ex)
        {
            ShowProjectError(ex);
        }
        finally
        {
            SetBusy(false);
            RefreshStatus();
        }
    }

    private void ShowProjectError(Exception ex)
    {
        _statusTone = LogTone.Error;
        _statusText = () => ConfigText.Get("目录或项目配置操作失败。", "Directory or project settings operation failed.");
        SetLog(ex.Message, _language, LogTone.Error);
    }

    private Task<bool> ConfirmGenerationAsync(ModelGenerationPlan plan, bool exportAfter = false)
    {
        var dialog = new Window
        {
            Title = plan.HasApiChanges ? ConfigText.Get("公开 API 变更需要单独审阅", "Public API changes require a separate review") : exportAfter ? ConfigText.Get("导出前需要生成代码",
                "Generation required before export") : ConfigText.Get("确认生成范围变化",
                "Confirm generation scope changes"),
            Width = 680,
            Height = 420,
            MinWidth = 480,
            MinHeight = 300,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        var panel = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            Margin = new Thickness(20)
        };
        panel.Children.Add(new TextBlock
        {
            Text = plan.HasApiChanges ? ConfigText.Get("本次生成已阻止。请由程序员核对下方公开接口变化，并使用预览中的 codegen 迁移摘要显式处理；基线损坏时先从版本控制恢复。普通生成确认不会批准接口变化。",
                "Generation is blocked. Review the public API changes and use the preview's codegen migration digest. Restore an invalid baseline from version control. Ordinary generation confirmation does not approve API changes.") : exportAfter ? ConfigText.Get("代码尚未生成、结构已变化或支持源码需更新。请核对完整输入目录和变更清单；确认后生成并继续导出，Unity 端需同步代码。",
            "Code is missing, the schema changed, or shared support needs updating. Check the complete input root and changes; confirm to generate and export, then synchronize Unity code.") : ConfigText.Get("请确认使用的是完整输入目录。继续将按下方清单更新生成代码。",
            "Check that the complete input root is selected. Continuing will apply the changes below."),
            TextWrapping = TextWrapping.Wrap
        });
        var preview = new ScrollViewer
        {
            Content = new SelectableTextBlock
            {
                Text = plan.DescribeChanges(),
                TextWrapping = TextWrapping.Wrap
            },
            Margin = new Thickness(0, 12)
        };
        Grid.SetRow(preview, 1);
        panel.Children.Add(preview);
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 12
        };
        var cancel = new Button
        {
            Content = ConfigText.Get("取消", "Cancel"),
            IsCancel = true
        };
        var accept = new Button
        {
            Content = exportAfter ? ConfigText.Get("生成并导出", "Generate & Export") : ConfigText.Get("确认生成", "Confirm Generation")
        };
        cancel.Click += (_, _) => dialog.Close(false);
        accept.Click += (_, _) => dialog.Close(true);
        buttons.Children.Add(cancel);
        if (!plan.HasApiChanges) buttons.Children.Add(accept);
        Grid.SetRow(buttons, 2);
        panel.Children.Add(buttons);
        dialog.Content = panel;
        return dialog.ShowDialog<bool>(this);
    }
}
