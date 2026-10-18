using System.Collections.Concurrent;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ZrConfig.Core;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.App;

public sealed partial class MainWindow
{
    private readonly Button _validateButton = new() { MinWidth = 120 };
    private readonly Button _cancelButton = new() { IsEnabled = false };
    private readonly Button _historyButton = new() { IsVisible = false };
    private readonly Button _reportButton = new() { IsEnabled = false };
    private readonly Button _setupButton = new();
    private readonly Button _recentOpenButton = new();
    private readonly Button _recentRemoveButton = new();
    private readonly ComboBox _recentPicker = new() { MinWidth = 180, MaxWidth = 450 };
    private readonly TextBox _projectName = new() { MinWidth = 180 };
    private readonly TextBlock _projectState = new() { TextWrapping = TextWrapping.Wrap };
    private readonly Expander _directorySettings = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
    private readonly ProgressBar _operationProgress = new() { IsVisible = false, Height = 5 };
    private readonly TextBlock _progressText = new() { TextWrapping = TextWrapping.Wrap };
    private readonly DiagnosticView _diagnosticView = new();
    private readonly TabControl _resultTabs = new();
    private readonly TabItem _diagnosticTab = new();
    private readonly TabItem _logTab = new();
    private readonly ConcurrentQueue<OperationProgress> _events = new();
    private readonly List<Diagnostic> _liveDiagnostics = [];
    private readonly List<FileSystemWatcher> _inputWatchers = [];
    private readonly DispatcherTimer _eventTimer = new() { Interval = TimeSpan.FromMilliseconds(100) };
    private readonly DispatcherTimer _projectTimer = new() { Interval = TimeSpan.FromMilliseconds(350) };
    private ProjectPreferences _projectPreferences = null!;
    private CancellationTokenSource? _cancellation;
    private WorkflowOperation? _operation;
    private ProjectWorkflowResult? _lastWorkflow;
    private WorkspaceSettings? _lastSettings;
    private WorkspaceSettings? _savedProject;
    private string _lastMode = "";
    private string? _projectPath;
    private bool _openingProject;
    private bool _inputsChanged;
    private bool _closeAfterRun;
    private bool _busy;
    private bool _watchersInitializing;
    private bool _watchersClosed;
    private bool _watchersResetPending;
    private int _watcherGeneration;
    private int _activeVersionRequest;
    private string? _activeVersionFolder;
    private string? _activeVersionText;
    private string _stageText = "";
    private int _liveLogCharacters;

    private Control CreateWorkspaceHeader()
    {
        var panel = new StackPanel { Spacing = 6 };
        var projectRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 8 };
        projectRow.Children.Add(_projectName);
        StackPanel projectButtons = CreateProjectButtons();
        Grid.SetColumn(projectButtons, 1);
        projectRow.Children.Add(projectButtons);
        panel.Children.Add(projectRow);
        panel.Children.Add(_projectState);
        var actions = new WrapPanel();
        foreach (Control control in new Control[] { _recentPicker, _recentOpenButton, _recentRemoveButton, _setupButton, _historyButton, _reportButton })
        {
            control.Margin = new Thickness(0, 0, 8, 4);
            actions.Children.Add(control);
        }

        panel.Children.Add(actions);
        return panel;
    }

    private void InitializeWorkspace(WorkspaceSettings? workspace)
    {
        _projectPreferences = new ProjectPreferences(_workspaceSettingsPath);
        _projectName.Text = workspace?.Name ?? "";
        _directorySettings.IsExpanded = workspace == null;
        RefreshRecentProjects();
        _validateButton.Click += async (_, _) => await RunOperationAsync("validate");
        _cancelButton.Click += (_, _) => CancelOperation();
        _historyButton.Click += async (_, _) => await ShowHistoryAsync();
        _reportButton.Click += async (_, _) => await SaveReportAsync();
        _setupButton.Click += async (_, _) => await ShowSetupAsync();
        _recentOpenButton.Click += (_, _) =>
        {
            if (_recentPicker.SelectedItem is RecentProject project)
            {
                try
                {
                    OpenProject(project.Path);
                }
                catch (Exception ex) { ShowProjectError(ex); }
            }
        };
        _recentRemoveButton.Click += (_, _) =>
        {
            if (_recentPicker.SelectedItem is RecentProject project)
            {
                try
                {
                    _projectPreferences.Remove(project.Path);
                    RefreshRecentProjects();
                }
                catch (Exception ex) { ShowProjectError(ex); }
            }
        };
        foreach (TextBox box in new[] { _projectName, _dataFolder, _generatedFolder, _outputFolder, _rulesFolder })
        {
            box.TextChanged += (_, _) => ProjectChanged();
        }

        _targetPicker.SelectionChanged += (_, _) => ProjectChanged();
        _formatPicker.SelectionChanged += (_, _) => { ProjectChanged(); SaveWorkspace(); };
        _runtimeFamilyPicker.SelectionChanged += (_, _) => { ProjectChanged(); SaveWorkspace(); };
        foreach (var check in _codeLanguageChecks) check.IsCheckedChanged += (_, _) =>
        {
            if (_openingProject) return;
            if (_codeLanguageChecks.All(item => item.IsChecked != true)) { check.IsChecked = true; return; }
            ProjectChanged(); SaveWorkspace();
        };
        _eventTimer.Tick += (_, _) => FlushEvents();
        _projectTimer.Tick += (_, _) => { _projectTimer.Stop(); RefreshProjectState(); RefreshActiveVersionAsync(); ResetInputWatchers(); };
        Closing += (_, e) =>
        {
            if (_cancellation != null)
            {
                e.Cancel = true;
                _closeAfterRun = true;
                CancelOperation();
            }
        };
        Closed += (_, _) =>
        {
            _eventTimer.Stop();
            _projectTimer.Stop();
            _watchersClosed = true;
            _watcherGeneration++;
            FileSystemWatcher[] retired = _inputWatchers.ToArray();
            _inputWatchers.Clear();
            _ = Task.Run(() => DisposeWatchers(retired));
        };
        Opened += (_, _) => { RefreshActiveVersionAsync(); ResetInputWatchers(); };
    }

    private void RefreshWorkspaceLanguage()
    {
        _validateButton.Content = ConfigText.Get("只校验", "Validate Only");
        _cancelButton.Content = ConfigText.Get("取消任务", "Cancel Task");
        _historyButton.Content = ConfigText.Get("版本历史", "Version History");
        _reportButton.Content = ConfigText.Get("导出诊断报告", "Save Report");
        _setupButton.Content = ConfigText.Get("首次接入", "Project Setup");
        _recentOpenButton.Content = ConfigText.Get("打开最近项目", "Open Recent");
        _recentRemoveButton.Content = ConfigText.Get("移除记录", "Forget");
        _projectName.PlaceholderText = ConfigText.Get("项目名称", "Project name");
        _directorySettings.Header = ConfigText.Get("项目目录设置", "Project Directories");
        _diagnosticTab.Header = ConfigText.Get("问题列表", "Issues");
        _logTab.Header = ConfigText.Get("运行日志", "Log");
        _diagnosticView.RefreshLanguage(_language);
        RefreshProjectState();
    }

    private void SetOperationBusy(bool busy)
    {
        _busy = busy;
        _validateButton.IsEnabled = _historyButton.IsEnabled = _setupButton.IsEnabled = _recentOpenButton.IsEnabled =
            _recentRemoveButton.IsEnabled = _recentPicker.IsEnabled = _projectName.IsEnabled = !busy;
        _reportButton.IsEnabled = !busy && _lastWorkflow != null;
        _cancelButton.IsEnabled = busy && _cancellation != null;
        _operationProgress.IsVisible = busy && _cancellation != null;
    }

    private async Task RunOperationAsync(string mode)
    {
        if (_busy)
        {
            return;
        }
        using IDisposable language = ConfigText.UseLanguage(_language);
        string runLanguage = _language;
        SaveWorkspace();
        _cancellation = new CancellationTokenSource();
        _operation = new WorkflowOperation(_cancellation.Token, new CallbackProgress<OperationProgress>(progress =>
        {
            // Limit pending UI work; the collector and persisted report retain every diagnostic.
            if (_events.Count < 10000)
            {
                _events.Enqueue(progress);
            }
        }));
        _lastMode = mode;
        _lastWorkflow = null;
        _lastSettings = ReadWorkspace();
        _inputsChanged = false;
        _liveDiagnostics.Clear();
        _liveLogCharacters = 0;
        _diagnosticView.SetDiagnostics([]);
        _resultTabs.SelectedIndex = 0;
        SetLog("", runLanguage);
        _stageText = ConfigText.Get("准备中", "Preparing");
        _operationProgress.IsIndeterminate = true;
        _statusTone = LogTone.Information;
        _statusText = () => mode == "validate" ? ConfigText.Get("正在只校验；不会发布数据。", "Validating; no data will be published.") :
            mode == "generate" ? ConfigText.Get("正在检查并生成代码。", "Reviewing and generating code.") : ConfigText.Get("正在校验并准备更新表文件。", "Validating and preparing table updates.");
        SetBusy(true);
        RefreshStatus();
        _eventTimer.Start();
        try
        {
            ProjectWorkflowResult result = mode switch
            {
                "validate" => await ProjectWorkflow.ValidateAsync(_lastSettings, operation: _operation),
                "generate" => await ProjectWorkflow.GenerateAsync(_lastSettings, plan => ConfirmGenerationAsync(plan), operation: _operation),
                _ => await ProjectWorkflow.ExportAsync(_lastSettings, plan => ConfirmGenerationAsync(plan, exportAfter: true), operation: _operation)
            };
            FlushEvents();
            _lastWorkflow = result;
            // Code writes from this operation are expected; the final input fingerprint represents the checked snapshot.
            _inputsChanged = false;
            if (result.Export?.InputFingerprint is { } fingerprint)
            {
                try
                {
                    _inputsChanged = fingerprint != (await Task.Run(() => ProjectInputSnapshot.Capture(_lastSettings))).Fingerprint;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { _inputsChanged = true; }
            }

            Diagnostic[] diagnostics = result.PreparationDiagnostics.Concat(result.Export?.Diagnostics ?? []).ToArray();
            _diagnosticView.SetDiagnostics(diagnostics);
            int warnings = diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);
            int errors = diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);
            _statusTone = result.Cancelled ? LogTone.Information : result.Success ? warnings > 0 ? LogTone.Warning : LogTone.Success : LogTone.Error;
            _statusText = () => result.Cancelled
                ? result.CodeCommitted ? ConfigText.Get("已取消；代码已更新，数据未发布，原数据保持不变。", "Cancelled; code was updated, data was not published, and existing data is unchanged.")
                    : ConfigText.Get("已取消；代码和原数据保持不变。", "Cancelled; code and the active version are unchanged.")
                : result.NeedsGeneration ? ConfigText.Get("需要生成代码；完整校验尚未完成，未发布数据。", "Code generation required; validation is incomplete and no data was published.")
                : result.Success ? mode == "generate" ? ConfigText.Get($"模型已生成 · {warnings} 条警告；Unity 端需编译对应脚本。", $"Models generated · {warnings} warnings. Compile matching Unity scripts.")
                    : mode == "validate" ? ConfigText.Get($"校验通过 · {warnings} 条警告 · 未发布数据", $"Validation passed · {warnings} warnings · No data published")
                    : result.Export?.ReusedVersion == true
                        ? ConfigText.Get($"数据未变化 · 文件保持不变 · {warnings} 条警告", $"Data unchanged · Files preserved · {warnings} warnings")
                        : ConfigText.Get($"导出完成 · 更新 {result.Export?.UpdatedTables} 张表 · {warnings} 条警告", $"Export complete · Updated {result.Export?.UpdatedTables} tables · {warnings} warnings")
                : ConfigText.Get($"操作未通过 · {errors} 条错误 · {warnings} 条警告", $"Operation failed · {errors} errors · {warnings} warnings") +
                    (result.CodeCommitted ? ConfigText.Get("；代码已更新，数据未发布。", "; code was updated; data was not published.") : "");
            var log = new StringBuilder();
            _diagnosticView.AppendAllTo(log);
            if (result.Plan != null && (result.NeedsGeneration || mode == "generate" || result.Cancelled))
            {
                log.AppendLine(result.Plan.DescribeChanges());
            }
            if (result.Export is { } export)
            {
                log.AppendLine($"Run: {export.RunId}");
                if (export.RunDirectory != null)
                {
                    log.AppendLine($"Reports: {export.RunDirectory}");
                }
                log.AppendLine(export.LogText);
            }

            SetLog(log.ToString(), runLanguage, _statusTone);
        }
        catch (Exception ex)
        {
            _statusTone = LogTone.Error;
            _statusText = () => ConfigText.Get("操作失败，请查看诊断。", "Operation failed; review diagnostics.");
            var diagnostic = new Diagnostic(DiagnosticSeverity.Error, "PROJECT_OPERATION_FAILED", ex.Message);
            _diagnosticView.SetDiagnostics([diagnostic]);
            _lastWorkflow = new(false, false, null, [diagnostic]);
            SetLog(ex.ToString(), runLanguage, LogTone.Error);
        }
        finally
        {
            _eventTimer.Stop();
            _progressText.Text = ConfigText.Get($"本次耗时：{_operation.Elapsed.TotalSeconds:F1} 秒", $"Elapsed: {_operation.Elapsed.TotalSeconds:F1} s");
            _operation = null;
            _cancellation.Dispose();
            _cancellation = null;
            while (_events.TryDequeue(out _))
            {
            }
            SetBusy(false);
            RefreshStatus();
            RefreshProjectState();
            RefreshActiveVersionAsync();
            ResetInputWatchers();
            if (_closeAfterRun)
            {
                Close();
            }
        }
    }

    private void FlushEvents()
    {
        if (_operation == null)
        {
            return;
        }
        using IDisposable language = ConfigText.UseLanguage(_logLanguage ?? _language);
        var log = new StringBuilder();
        int previousDiagnostics = _liveDiagnostics.Count;
        for (int count = 0; count < 300 && _events.TryDequeue(out OperationProgress? progress); count++)
        {
            if (progress.RunId != _operation.RunId)
            {
                continue;
            }
            if (progress.Diagnostic is { } diagnostic)
            {
                if (_liveDiagnostics.Count < 10000)
                {
                    _liveDiagnostics.Add(diagnostic);
                }
                log.AppendLine(diagnostic.ToString());
            }
            else
            {
                _stageText = progress.Message;
                _operationProgress.IsIndeterminate = progress.Total is not > 0;
                if (progress.Total is > 0)
                {
                    _operationProgress.Maximum = progress.Total.Value;
                    _operationProgress.Value = progress.Completed;
                }
                _cancelButton.IsEnabled = !progress.Committing && !_cancellation!.IsCancellationRequested;
                log.AppendLine($"[{progress.Stage}] {progress.Message}" + (progress.Total.HasValue ? $" ({progress.Completed}/{progress.Total})" : ""));
            }
        }

        if (log.Length != 0 && _liveLogCharacters < 1000000)
        {
            _logBox.AppendText(log.ToString());
            _liveLogCharacters += log.Length;
        }
        if (_liveDiagnostics.Count != previousDiagnostics)
        {
            _diagnosticView.AppendDiagnostics(_liveDiagnostics, previousDiagnostics);
        }

        using IDisposable uiLanguage = ConfigText.UseLanguage(_language);
        _progressText.Text = (_cancellation!.IsCancellationRequested ? ConfigText.Get("正在取消，等待当前安全步骤结束", "Cancelling; waiting for the current safe step") : _stageText) +
            ConfigText.Get($" · {_operation.Elapsed.TotalSeconds:F1} 秒", $" · {_operation.Elapsed.TotalSeconds:F1} s");
    }

    private void CancelOperation()
    {
        _cancellation?.Cancel();
        _cancelButton.IsEnabled = false;
        FlushEvents();
    }

    private void ProjectChanged()
    {
        if (_openingProject || _projectPreferences == null)
        {
            return;
        }
        _inputsChanged = true;
        _projectTimer.Stop();
        _projectTimer.Start();
    }

    private void RefreshProjectState()
    {
        using IDisposable language = ConfigText.UseLanguage(_language);
        WorkspaceSettings settings = ReadWorkspace();
        string source = _projectPath ?? ConfigText.Get("尚未保存项目配置", "Project settings not saved");
        bool unsaved = _savedProject != null && !_savedProject.SameConfiguration(settings);
        string state = !Directory.Exists(settings.DataFolder) ? ConfigText.Get("请设置有效的 Excel 输入目录。", "Select an existing Excel input directory.") :
            !Directory.Exists(settings.CodeFolder) ? ConfigText.Get("代码尚未生成；请查看生成预览。", "Code has not been generated; review generation.") :
            _inputsChanged && _lastWorkflow != null ? ConfigText.Get("输入或设置已变化，上次结果已过期。", "Inputs or settings changed; the previous result is stale.") :
            _lastWorkflow?.NeedsGeneration == true ? ConfigText.Get("结构变化，需要生成代码。", "Schema changed; code generation required.") :
            _lastWorkflow?.Success == true ? ConfigText.Get("最近操作成功；数据保存在输出目录。", "Last operation succeeded; data is in the output directory.") :
            ConfigText.Get("等待校验；读取磁盘上已保存的 Excel。", "Awaiting validation; reads Excel saved on disk.");
        string active = settings.OutputFolder == _activeVersionFolder ? _activeVersionText ?? ConfigText.Get("尚未导出", "Not exported") : ConfigText.Get("尚未读取", "Not read yet");
        _projectState.Text = source + (unsaved ? ConfigText.Get(" · 设置未保存", " · Unsaved settings") : "") + Environment.NewLine + state + Environment.NewLine +
            ConfigText.Get($"导出数据：{active} · ", $"Exported data: {active} · ") + (string.IsNullOrWhiteSpace(settings.RulesFolder) ? ConfigText.Get("仅基础校验", "Basic validation only") :
                Directory.Exists(settings.RulesFolder) ? ConfigText.Get("已配置业务规则", "Business rules configured") : ConfigText.Get("规则目录不存在", "Rules directory missing"));
    }

    private async void RefreshActiveVersionAsync()
    {
        WorkspaceSettings settings = ReadWorkspace();
        string folder = settings.OutputFolder;
        int request = ++_activeVersionRequest;
        string? version;
        try
        {
            var manifests = await Task.Run(() => settings.CodeLanguages.Select(language =>
                ReleaseHistory.TryReadCurrent(Path.Combine(folder, ExportOptions.LanguageDirectory(language)))).OfType<ReleaseManifest>().ToArray());
            if (manifests.Length != 0)
                version = ConfigText.Get($"{manifests.Length} 种语言，{manifests.Max(m => m.Tables.Length)} 张表，固定目录", $"{manifests.Length} languages, {manifests.Max(m => m.Tables.Length)} tables, fixed directories");
            else
                version = await Task.Run(() => ReleaseHistory.TryReadCurrent(folder)) == null ? null :
                    ConfigText.Get("旧版目录，导出后按语言归类", "Legacy layout; export to group by language");
        }
        catch (Exception error) { version = ConfigText.Get("无法读取：", "Cannot read: ") + error.Message; }
        if (!_watchersClosed && request == _activeVersionRequest && ReadWorkspace().OutputFolder == folder)
        {
            _activeVersionFolder = folder;
            _activeVersionText = version;
            RefreshProjectState();
        }
    }

    private static void DisposeWatchers(IEnumerable<FileSystemWatcher> watchers)
    {
        foreach (FileSystemWatcher watcher in watchers)
        {
            try
            {
                watcher.Dispose();
            }
            catch (Exception error) { System.Diagnostics.Trace.TraceWarning("Watcher disposal failed: {0}", error.Message); }
        }
    }

    private async void ResetInputWatchers()
    {
        if (_workspaceSettingsPath == null || _watchersClosed)
        {
            return;
        }
        int generation = ++_watcherGeneration;
        if (_watchersInitializing)
        {
            _watchersResetPending = true;
            return;
        }

        _watchersInitializing = true;
        _watchersResetPending = false;
        string[] folders = new[] { _dataFolder.Text, _generatedFolder.Text, _rulesFolder.Text }.OfType<string>()
            .Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToArray();
        FileSystemWatcher[] retired = _inputWatchers.ToArray();
        _inputWatchers.Clear();
        // Native FSEvents setup/disposal can wait on a volume or an OS permission. Never block the UI.
        var created = new List<FileSystemWatcher>();
        try
        {
            await Task.Run(() =>
            {
                DisposeWatchers(retired);
                foreach (string folder in folders)
                {
                    FileSystemWatcher? watcher = null;
                    try
                    {
                        if (!Directory.Exists(folder))
                        {
                            continue;
                        }
                        watcher = new FileSystemWatcher(folder) { IncludeSubdirectories = true, NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size };
                        void Stale() => Dispatcher.UIThread.Post(() =>
                        {
                            if (!_watchersClosed && generation == _watcherGeneration)
                            {
                                _inputsChanged = true;
                                RefreshProjectState();
                            }
                        });
                        void Changed(object sender, FileSystemEventArgs e)
                        {
                            string relative = Path.GetRelativePath(folder, e.FullPath);
                            if (relative.Split(Path.DirectorySeparatorChar).Any(part => part.StartsWith('.') || part is "bin" or "obj") || e.FullPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                            {
                                return;
                            }
                            Stale();
                        }
                        watcher.Changed += Changed;
                        watcher.Created += Changed;
                        watcher.Deleted += Changed;
                        watcher.Renamed += (sender, args) => Changed(sender, args);
                        watcher.Error += (_, _) => Stale();
                        watcher.EnableRaisingEvents = true;
                        created.Add(watcher);
                    }
                    catch (Exception error)
                    {
                        if (watcher != null)
                        {
                            DisposeWatchers([watcher]);
                        }
                        System.Diagnostics.Trace.TraceWarning("Input watcher unavailable: {0}", error.Message);
                    }
                }
            });
            if (_watchersClosed || generation != _watcherGeneration)
            {
                _ = Task.Run(() => DisposeWatchers(created));
            }
            else
            {
                _inputWatchers.AddRange(created);
            }
        }
        finally
        {
            _watchersInitializing = false;
            if (_watchersResetPending && !_watchersClosed)
            {
                ResetInputWatchers();
            }
        }
    }

    private void RememberProject(string path)
    {
        try
        {
            _projectPreferences.Remember(path, _projectName.Text);
            RefreshRecentProjects();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { _settingsNotice.IsVisible = true; }
    }

    private void RefreshRecentProjects()
    {
        _recentPicker.ItemsSource = _projectPreferences.Recent.ToArray();
        _recentPicker.SelectedIndex = _projectPreferences.Recent.Count == 0 ? -1 : 0;
    }

    private async Task SaveReportAsync()
    {
        if (_lastWorkflow == null || _lastSettings == null)
        {
            return;
        }
        try
        {
            IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions { Title = ConfigText.Get("保存诊断报告", "Save diagnostic report"), SuggestedFileName = "zrconfig-report.json", ShowOverwritePrompt = true });
            if (file?.TryGetLocalPath() is { } path)
            {
                OperationReport.Save(path, _lastWorkflow, _lastSettings, _lastMode);
            }
        }
        catch (Exception ex) { ShowProjectError(ex); }
    }
}
