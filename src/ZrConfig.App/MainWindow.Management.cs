using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using ZrConfig.Core;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.App;

public sealed partial class MainWindow
{
    private async Task ShowSetupAsync()
    {
        using IDisposable language = ConfigText.UseLanguage(_language);
        var dialog = new Window { Title = ConfigText.Get("首次接入项目", "Set Up Project"), Width = 820, Height = 680, MinWidth = 600, MinHeight = 450, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        var content = new StackPanel { Spacing = 8, Margin = new Thickness(18) };
        content.Children.Add(new TextBlock { Text = ConfigText.Get("1. 选择现有项目目录 → 2. 检查完整表范围 → 3. 保存项目配置\n向导不改写工作簿或生成代码；完成后在主界面校验或查看生成预览。", "1. Select existing project directories → 2. Review tables → 3. Save project settings\nSetup does not modify workbooks or generate code. Validate or review generation from the main window afterwards."), TextWrapping = TextWrapping.Wrap });
        WorkspaceSettings current = ReadWorkspace();
        var name = new TextBox { Text = current.Name ?? "", PlaceholderText = ConfigText.Get("项目名称", "Project name") };
        content.Children.Add(name);
        var inputs = new List<TextBox>();
        foreach ((string label, string value) in new[]
        {
            (ConfigText.Get("Excel 输入目录", "Excel input"), current.DataFolder),
            (ConfigText.Get("Config 代码根目录", "Config code root"), current.CodeFolder),
            (ConfigText.Get("Output 输出目录", "Output directory"), current.OutputFolder),
            (ConfigText.Get("Rules 规则目录（可选）", "Rules directory (optional)"), current.RulesFolder)
        })
        {
            content.Children.Add(new TextBlock { Text = label });
            var box = new TextBox { Text = value };
            inputs.Add(box);
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            row.Children.Add(box);
            var browse = new Button { Content = ConfigText.Get("浏览…", "Browse..."), Margin = new Thickness(6, 0, 0, 0) };
            Grid.SetColumn(browse, 1);
            row.Children.Add(browse);
            content.Children.Add(row);
            browse.Click += async (_, _) =>
            {
                IReadOnlyList<IStorageFolder> selected = await dialog.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = label });
                if (selected.FirstOrDefault()?.TryGetLocalPath() is { } path)
                {
                    box.Text = path;
                }
            };
        }

        var target = new ComboBox { ItemsSource = new[] { "C · Client", "S · Server" }, SelectedIndex = current.Target == ConfigTarget.Server ? 1 : 0 };
        content.Children.Add(target);
        var preview = new SelectableTextBlock { TextWrapping = TextWrapping.Wrap };
        var inspect = new Button { Content = ConfigText.Get("检查目录与表结构", "Review Directories and Schema") };
        var save = new Button { Content = ConfigText.Get("保存项目并开始使用", "Save Project and Continue"), IsEnabled = false };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        buttons.Children.Add(inspect);
        buttons.Children.Add(save);
        content.Children.Add(buttons);
        content.Children.Add(preview);
        WorkspaceSettings? reviewed = null;
        WorkspaceSettings Read() => new(inputs[0].Text ?? "", inputs[2].Text ?? "", inputs[1].Text ?? "", inputs[3].Text ?? "")
        {
            Name = name.Text,
            DataFormat = current.DataFormat,
            RuntimeFamily = current.RuntimeFamily,
            Namespaces=current.Namespaces,
            CodeLanguages = current.CodeLanguages,
            Target = target.SelectedIndex == 1 ? ConfigTarget.Server : ConfigTarget.Client
        };
        inspect.Click += async (_, _) =>
        {
            inspect.IsEnabled = false;
            save.IsEnabled = false;
            try
            {
                WorkspaceSettings requested = Read();
                reviewed = requested.Validate(generation: false, allowMissingCode: true);
                var diagnostics = new DiagnosticCollector();
                ModelGenerationPlan? plan = await Task.Run(() => ModelGenerator.PrepareWithFamily(reviewed.RuntimeFamily, reviewed.DataFolder, reviewed.CodeFolder, diagnostics, target: reviewed.Target, languages: reviewed.CodeLanguages, dataFormat: reviewed.DataFormat));
                preview.Text = string.Join(Environment.NewLine, diagnostics.Snapshot().Select(d => d.ToString())) + Environment.NewLine + plan?.DescribeChanges();
                save.IsEnabled = plan != null && requested == Read();
            }
            catch (Exception ex) { preview.Text = ex.Message; }
            finally { inspect.IsEnabled = true; }
        };
        foreach (TextBox box in inputs.Append(name))
        {
            box.TextChanged += (_, _) => save.IsEnabled = false;
        }
        target.SelectionChanged += (_, _) => save.IsEnabled = false;
        save.Click += async (_, _) =>
        {
            if (reviewed == null)
            {
                return;
            }
            try
            {
                IStorageFile? selected = await dialog.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = ConfigText.Get("保存项目配置", "Save Project Settings"),
                    SuggestedFileName = "project.zrconfig.json",
                    FileTypeChoices = [s_projectFileType],
                    ShowOverwritePrompt = true
                });
                if (selected?.TryGetLocalPath() is not { } path)
                {
                    return;
                }
                reviewed.SaveProject(path);
                OpenProject(path);
                dialog.Close();
            }
            catch (Exception ex) { preview.Text = ex.Message; }
        };
        dialog.Content = new ScrollViewer { Content = content };
        await dialog.ShowDialog(this);
    }

    private async Task ShowHistoryAsync()
    {
        using IDisposable language = ConfigText.UseLanguage(_language);
        WorkspaceSettings settings = ReadWorkspace();
        var dialog = new Window { Title = ConfigText.Get("版本历史与发布管理", "Version History"), Width = 1050, Height = 750, MinWidth = 720, MinHeight = 500, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        var layout = new Grid { RowDefinitions = new RowDefinitions("Auto,Auto,2*,Auto,3*,Auto"), Margin = new Thickness(16), RowSpacing = 8 };
        var notice = new TextBlock { Text = ConfigText.Get("选择版本查看；回滚需要接入方提供的部署清单。历史清理默认保留最近 10 个版本及 30 天读取窗口，当前、保留和未知版本不会删除。", "Select a version. Rollback requires the integration owner's deployed manifest. Cleanup keeps the newest 10 versions and a 30-day reader window; active, pinned and unknown versions are protected."), TextWrapping = TextWrapping.Wrap };
        layout.Children.Add(notice);
        var versions = new ListBox();
        Grid.SetRow(versions, 2);
        layout.Children.Add(versions);
        var baseline = new ComboBox { MinWidth = 180, MaxWidth = 700 };
        Grid.SetRow(baseline, 1);
        layout.Children.Add(baseline);
        var detail = new SelectableTextBlock { TextWrapping = TextWrapping.Wrap };
        var details = new ScrollViewer { Content = detail };
        Grid.SetRow(details, 4);
        layout.Children.Add(details);
        var status = new TextBlock { TextWrapping = TextWrapping.Wrap };
        Grid.SetRow(status, 5);
        layout.Children.Add(status);
        var actions = new WrapPanel();
        Grid.SetRow(actions, 3);
        layout.Children.Add(actions);
        Button Button(string chinese, string english)
        {
            var button = new Button { Content = ConfigText.Get(chinese, english), Margin = new Thickness(0, 0, 8, 4) };
            actions.Children.Add(button);
            return button;
        }

        Button refresh = Button("刷新", "Refresh");
        Button compare = Button("与基准比较", "Compare to Baseline");
        Button rowDiff = Button("逐字段比较", "Compare Fields");
        Button pin = Button("保留 / 取消保留", "Pin / Unpin");
        Button rollback = Button("回滚预览", "Preview Rollback");
        Button cleanup = Button("历史清理预览", "Preview Cleanup");
        var table = new TextBox { PlaceholderText = ConfigText.Get("逐字段比较的表名", "Table for field comparison"), Width = 170, Margin = new Thickness(0, 0, 8, 4) };
        var page = new TextBox { Text = "0", PlaceholderText = "Offset", Width = 70 };
        actions.Children.Add(table);
        actions.Children.Add(page);
        ReleaseInfo[] items = [];
        bool busy = false;
        async Task Execute(Func<Task> action)
        {
            if (busy)
            {
                return;
            }
            busy = true;
            actions.IsEnabled = false;
            status.Text = ConfigText.Get("处理中……", "Working...");
            try
            {
                await action();
                status.Text = ConfigText.Get("完成。", "Done.");
            }
            catch (Exception ex) { status.Text = ex.Message; }
            finally { busy = false; actions.IsEnabled = true; }
        }

        async Task Refresh()
        {
            items = (await Task.Run(() => ReleaseHistory.List(settings.OutputFolder))).ToArray();
            string[] labels = items.Select(v => $"{(v.Current ? "● " : "")}{(v.Pinned ? "★ " : "")}{v.VersionId} | {v.Target} | {v.Tables} tables | {v.Bytes / 1048576.0:F2} MiB" + (v.Problem == null ? "" : " | " + v.Problem)).ToArray();
            versions.ItemsSource = labels;
            baseline.ItemsSource = labels;
            versions.SelectedIndex = items.Length == 0 ? -1 : 0;
            baseline.SelectedIndex = items.Length > 1 ? 1 : items.Length - 1;
        }

        ReleaseInfo Selected() => versions.SelectedIndex >= 0 ? items[versions.SelectedIndex] : throw new InvalidOperationException(ConfigText.Get("请选择版本。", "Select a version."));
        ReleaseInfo Before() => baseline.SelectedIndex >= 0 ? items[baseline.SelectedIndex] : throw new InvalidOperationException(ConfigText.Get("请选择比较基准。", "Select a baseline."));
        refresh.Click += async (_, _) => await Execute(Refresh);
        versions.SelectionChanged += (_, _) => { if (versions.SelectedIndex >= 0) { detail.Text = JsonSerializer.Serialize(Selected(), new JsonSerializerOptions { WriteIndented = true }); } };
        compare.Click += async (_, _) => await Execute(async () =>
        {
            string from = Before().VersionId, to = Selected().VersionId;
            IReadOnlyList<TableDifference> changes = await Task.Run(() => ReleaseHistory.Compare(settings.OutputFolder, from, to));
            detail.Text = string.Join(Environment.NewLine, changes.Select(c => $"{c.Table}: {c.Change} | rows {c.BeforeRows} → {c.AfterRows} | bytes {c.BeforeBytes} → {c.AfterBytes}" + (c.SchemaChanged ? " | Schema changed" : "")));
        });
        rowDiff.Click += async (_, _) => await Execute(async () =>
        {
            string from = Before().VersionId, to = Selected().VersionId, name = table.Text?.Trim() ?? "";
            if (!int.TryParse(page.Text, out int offset))
            {
                throw new ArgumentException("Invalid offset.");
            }
            RowDifferencePage changes = await Task.Run(() => ReleaseHistory.CompareRows(settings.OutputFolder, from, to, name, offset));
            detail.Text = JsonSerializer.Serialize(changes, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });
        });
        pin.Click += async (_, _) => await Execute(async () => { ReleaseInfo selected = Selected(); await Task.Run(() => ReleaseHistory.SetPinned(settings.OutputFolder, selected.VersionId, !selected.Pinned)); await Refresh(); });
        rollback.Click += async (_, _) => await Execute(async () =>
        {
            ReleaseInfo selected = Selected();
            IReadOnlyList<IStorageFile> files = await dialog.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { Title = ConfigText.Get("选择当前部署端的兼容清单", "Select the deployed client's compatibility manifest"), AllowMultiple = false });
            if (files.FirstOrDefault()?.TryGetLocalPath() is not { } contract)
            {
                return;
            }
            RollbackPlan plan = await Task.Run(() => ReleaseHistory.PrepareRollback(settings.OutputFolder, selected.VersionId, contract, settings.Target));
            string? reason = await ConfirmManagementAsync(dialog, ConfigText.Get("确认回滚", "Confirm Rollback"),
                ConfigText.Get($"将当前数据切换为 {selected.VersionId}。文件完整性及 Schema 已核验；请确认所选清单确实对应部署端代码。\n代码不会随数据回滚。请输入回滚原因。", $"Activate {selected.VersionId}. Files and schemas were verified; confirm the supplied manifest represents the deployed client.\nCode is not rolled back. Enter a reason."), requireReason: true);
            if (reason == null)
            {
                return;
            }
            await Task.Run(() => ReleaseHistory.Activate(plan, reason));
            await Refresh();
            RefreshProjectState();
        });
        cleanup.Click += async (_, _) => await Execute(async () =>
        {
            CleanupPlan plan = await Task.Run(() => ReleaseHistory.PreviewCleanup(settings.OutputFolder));
            if (plan.Candidates.Length == 0)
            {
                detail.Text = ConfigText.Get("没有符合清理条件的版本。", "No versions are eligible for cleanup.");
                return;
            }
            string text = ConfigText.Get("确认没有客户端仍在读取以下版本。删除后无法通过工具恢复：", "Confirm no clients still read these versions. Deleted versions cannot be restored by the tool:") + Environment.NewLine +
                string.Join(Environment.NewLine, plan.Candidates.Select(c => $"{c.VersionId} · {c.Bytes / 1048576.0:F2} MiB"));
            if (await ConfirmManagementAsync(dialog, ConfigText.Get("确认清理", "Confirm Cleanup"), text, false) == null)
            {
                return;
            }
            await Task.Run(() => ReleaseHistory.Clean(plan));
            await Refresh();
        });
        dialog.Content = layout;
        dialog.Opened += async (_, _) => await Execute(Refresh);
        dialog.Closing += (_, e) => { if (busy) { e.Cancel = true; status.Text = ConfigText.Get("请等待当前版本操作结束。", "Wait for the current version operation to finish."); } };
        SetBusy(true);
        try
        {
            await dialog.ShowDialog(this);
        }
        finally { SetBusy(false); RefreshProjectState(); RefreshActiveVersionAsync(); }
    }

    private static Task<string?> ConfirmManagementAsync(Window owner, string title, string message, bool requireReason)
    {
        var dialog = new Window { Title = title, Width = 640, Height = 400, MinWidth = 420, MinHeight = 300, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        var layout = new Grid { RowDefinitions = new RowDefinitions("*,Auto,Auto"), Margin = new Thickness(18), RowSpacing = 10 };
        layout.Children.Add(new ScrollViewer { Content = new SelectableTextBlock { Text = message, TextWrapping = TextWrapping.Wrap } });
        var reason = new TextBox { PlaceholderText = ConfigText.Get("操作原因", "Reason"), IsVisible = requireReason };
        Grid.SetRow(reason, 1);
        layout.Children.Add(reason);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 10 };
        var cancel = new Button { Content = ConfigText.Get("取消", "Cancel"), IsCancel = true };
        var accept = new Button { Content = ConfigText.Get("确认执行", "Confirm"), IsEnabled = !requireReason };
        reason.TextChanged += (_, _) => accept.IsEnabled = !string.IsNullOrWhiteSpace(reason.Text);
        cancel.Click += (_, _) => dialog.Close(null);
        accept.Click += (_, _) => dialog.Close(requireReason ? reason.Text!.Trim() : "confirmed");
        buttons.Children.Add(cancel);
        buttons.Children.Add(accept);
        Grid.SetRow(buttons, 2);
        layout.Children.Add(buttons);
        dialog.Content = layout;
        return dialog.ShowDialog<string?>(owner);
    }
}
