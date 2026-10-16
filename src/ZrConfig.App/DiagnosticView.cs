using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.App;

internal sealed class DiagnosticView : UserControl
{
    private readonly ComboBox _severity = new() { MinWidth = 120, SelectedIndex = 0 };
    private readonly ComboBox _table = new() { MinWidth = 150, SelectedIndex = 0 };
    private readonly TextBox _search = new() { MinWidth = 180 };
    private readonly ColoredLogView _text = new(showToolbar: false);
    private readonly TextBlock _count = new();
    private readonly SelectableTextBlock _details = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _actionStatus = new() { TextWrapping = TextWrapping.Wrap };
    private readonly Button _open = new();
    private readonly Button _reveal = new();
    private readonly Button _copyLocation = new();
    private readonly Button _copySelected = new();
    private readonly Button _copyAll = new();
    private readonly DispatcherTimer _searchDelay = new() { Interval = TimeSpan.FromMilliseconds(180) };
    private readonly List<Entry> _entries = [];
    private readonly List<Entry> _ordered = [];
    private readonly List<Entry> _visible = [];
    private readonly List<Entry> _pending = [];
    private readonly HashSet<Entry> _selected = [];
    private readonly HashSet<string> _groups = new(StringComparer.Ordinal);
    private StringBuilder _buffer = new();
    private string[] _groupNames = [];
    private string _language = ConfigText.Chinese;
    private string _dataLanguage = ConfigText.Chinese;
    private string _filterText = "";
    private string? _filterGroup;
    private int _filterSeverity;
    private int _errors;
    private int _warnings;
    private bool _refreshing;
    private bool _editingText;
    private Entry? _active;
    internal int TotalCount => _entries.Count;
    internal int DisplayedCount => _visible.Count;
    internal TextEditor Editor => _text.Editor;
    internal Diagnostic? SelectedDiagnostic => _active?.Diagnostic;

    internal DiagnosticView()
    {
        var layout = new Grid { RowDefinitions = new RowDefinitions("Auto,Auto,*,Auto,Auto,Auto"), RowSpacing = 6 };
        var filters = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (Control control in new Control[] { _severity, _table, _search })
        {
            control.Margin = new Thickness(0, 0, 8, 4);
            filters.Children.Add(control);
        }

        layout.Children.Add(filters);
        Add(_count, 1);
        Add(_text, 2);
        Add(new ScrollViewer { Content = _details, MaxHeight = 105 }, 3);
        var actions = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (Button button in new[] { _open, _reveal, _copyLocation, _copySelected, _copyAll })
        {
            button.Margin = new Thickness(0, 0, 8, 4);
            actions.Children.Add(button);
        }

        Add(actions, 4);
        Add(_actionStatus, 5);
        Content = layout;
        _text.ToneAtOffset = offset => EntryAt(offset)?.Diagnostic.Severity switch
        {
            DiagnosticSeverity.Error => LogTone.Error,
            DiagnosticSeverity.Warning => LogTone.Warning,
            DiagnosticSeverity.Success => LogTone.Success,
            _ => LogTone.Information
        };
        _text.IsSelectedAtOffset = offset => _selected.Count != 0 && EntryAt(offset) is { } entry && _selected.Contains(entry);
        _severity.SelectionChanged += (_, _) => ApplyFilters();
        _table.SelectionChanged += (_, _) => ApplyFilters();
        _search.TextChanged += (_, _) => { _searchDelay.Stop(); _searchDelay.Start(); };
        _searchDelay.Tick += (_, _) => { _searchDelay.Stop(); ApplyFilters(); };
        Editor.TextArea.SelectionChanged += (_, _) => SelectionChanged();
        Editor.TextArea.Caret.PositionChanged += (_, _) => SelectionChanged();
        Editor.AddHandler(InputElement.PointerPressedEvent, (_, e) =>
        {
            if (e.GetCurrentPoint(Editor).Properties.IsLeftButtonPressed
                && (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0
                && Editor.GetPositionFromPoint(e.GetPosition(Editor)) is { } position
                && EntryAt(Editor.Document.GetOffset(position.Line, position.Column)) is { } entry)
            {
                SelectEntry(entry, toggle: true);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);
        Editor.AddHandler(InputElement.PointerPressedEvent, (_, e) =>
        {
            if (e.GetCurrentPoint(Editor).Properties.IsLeftButtonPressed
                && (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) == 0)
            {
                SelectionChanged();
            }
        }, RoutingStrategies.Bubble, handledEventsToo: true);
        _open.Click += (_, _) => OpenSelected(reveal: false);
        _reveal.Click += (_, _) => OpenSelected(reveal: true);
        _copyLocation.Click += async (_, _) => await CopyAsync(SelectedDiagnostic is { } d ? Location(d) : "");
        _copySelected.Click += async (_, _) => await CopyAsync(CopyText(selectedOnly: true));
        _copyAll.Click += async (_, _) => await CopyAsync(CopyText(selectedOnly: false));
        DetachedFromVisualTree += (_, _) => _searchDelay.Stop();
        RefreshLanguage(ConfigText.Language);
        void Add(Control control, int row)
        {
            Grid.SetRow(control, row);
            layout.Children.Add(control);
        }
    }

    internal void SetDiagnostics(IReadOnlyList<Diagnostic> diagnostics)
    {
        int shared = 0;
        while (shared < diagnostics.Count && shared < _entries.Count && ReferenceEquals(diagnostics[shared], _entries[shared].Diagnostic))
        {
            shared++;
        }

        if (shared == diagnostics.Count && shared == _entries.Count)
        {
            return;
        }

        if (shared == 0)
        {
            _dataLanguage = ConfigText.Language;
        }

        _pending.Clear();
        _entries.RemoveRange(shared, _entries.Count - shared);
        using (ConfigText.UseLanguage(_dataLanguage))
        {
            for (int i = shared; i < diagnostics.Count; i++)
            {
                _entries.Add(new Entry(diagnostics[i], i));
            }
        }

        _errors = _warnings = 0;
        _groups.Clear();
        _ordered.Clear();
        foreach (Entry entry in _entries)
        {
            Count(entry.Diagnostic);
            _groups.Add(GroupName(entry.Diagnostic));
            _ordered.Add(entry);
        }

        _ordered.Sort(EntryComparer.s_instance);
        RefreshGroups();
        RebuildText();
    }

    /// <summary>Format and sort only the new batch, then merge it into the existing text index.</summary>
    internal void AppendDiagnostics(IReadOnlyList<Diagnostic> diagnostics, int startIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex, diagnostics.Count);
        if (startIndex == diagnostics.Count)
        {
            return;
        }

        if (_entries.Count == 0)
        {
            _dataLanguage = ConfigText.Language;
        }

        _pending.Clear();
        bool groupsChanged = false;
        using (ConfigText.UseLanguage(_dataLanguage))
        {
            for (int i = startIndex; i < diagnostics.Count; i++)
            {
                var entry = new Entry(diagnostics[i], _entries.Count);
                _entries.Add(entry);
                _pending.Add(entry);
                Count(entry.Diagnostic);
                groupsChanged |= _groups.Add(GroupName(entry.Diagnostic));
            }
        }

        _pending.Sort(EntryComparer.s_instance);
        MergeInto(_ordered);

        if (groupsChanged)
        {
            RefreshGroups();
        }

        int matched = 0;
        for (int i = 0; i < _pending.Count; i++)
        {
            Entry entry = _pending[i];
            if (Matches(entry))
            {
                _pending[matched++] = entry;
            }
        }

        _pending.RemoveRange(matched, _pending.Count - matched);
        InsertPending();
        _pending.Clear();
        ResetBuffer();
        RefreshCount();
    }

    internal void RefreshLanguage(string language)
    {
        _language = language;
        using IDisposable scope = ConfigText.UseLanguage(language);
        _refreshing = true;
        int severity = Math.Max(0, _severity.SelectedIndex);
        _severity.ItemsSource = new[] { ConfigText.Get("全部级别", "All levels"), ConfigText.Get("错误", "Errors"), ConfigText.Get("警告", "Warnings"), ConfigText.Get("信息", "Information") };
        _severity.SelectedIndex = severity;
        _search.PlaceholderText = ConfigText.Get("搜索问题、字段或位置", "Search issue, field or location");
        _open.Content = ConfigText.Get("打开文件", "Open File");
        _reveal.Content = ConfigText.Get("显示所在目录", "Show Folder");
        _copyLocation.Content = ConfigText.Get("复制位置", "Copy Location");
        _copySelected.Content = ConfigText.Get("复制所选问题", "Copy Selected");
        _copyAll.Content = ConfigText.Get("复制全部问题", "Copy All");
        _refreshing = false;
        RefreshGroups();
        _text.RefreshLanguage();
        RefreshCount();
        ShowSelection();
    }

    internal void Filter(int severity, string search)
    {
        _refreshing = true;
        _severity.SelectedIndex = severity;
        _search.Text = search;
        _refreshing = false;
        _searchDelay.Stop();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (_refreshing)
        {
            return;
        }

        string search = _search.Text?.Trim() ?? "";
        string? group = _table.SelectedIndex > 0 ? _groupNames[_table.SelectedIndex - 1] : null;
        if (_filterSeverity == _severity.SelectedIndex && _filterText == search && _filterGroup == group)
        {
            return;
        }

        _filterSeverity = _severity.SelectedIndex;
        _filterText = search;
        _filterGroup = group;
        RebuildText();
    }

    private void ShowSelection()
    {
        using IDisposable language = ConfigText.UseLanguage(_language);
        Diagnostic? diagnostic = SelectedDiagnostic;
        _details.Text = diagnostic == null ? ConfigText.Get("选择问题查看位置和处理建议。", "Select an issue for its location and suggested action.") :
            _active!.Text + Environment.NewLine + Suggestion(diagnostic.Code);
        _open.IsEnabled = _reveal.IsEnabled = diagnostic != null && File.Exists(diagnostic.File);
        _copyLocation.IsEnabled = diagnostic != null && (diagnostic.Row > 0 || diagnostic.File.Length != 0);
        _copySelected.IsEnabled = diagnostic != null || _selected.Count != 0 || Editor.SelectionLength != 0;
    }

    private void RefreshGroups()
    {
        using IDisposable language = ConfigText.UseLanguage(_language);
        _refreshing = true;
        _groupNames = _groups.Order(StringComparer.Ordinal).ToArray();
        string[] choices = new string[_groupNames.Length + 1];
        choices[0] = ConfigText.Get("全部表 / 项目", "All tables / project");
        for (int i = 0; i < _groupNames.Length; i++)
        {
            choices[i + 1] = _groupNames[i].Length == 0 ? ConfigText.Get("项目 / 编译 / 发布", "Project / compilation / publication") : _groupNames[i];
        }

        _table.ItemsSource = choices;
        _table.SelectedIndex = _filterGroup == null ? 0 : Array.IndexOf(_groupNames, _filterGroup) + 1;
        if (_table.SelectedIndex == 0)
        {
            _filterGroup = null;
        }

        _refreshing = false;
    }

    private bool Matches(Entry entry) => (_filterSeverity switch
    {
        1 => entry.Diagnostic.Severity == DiagnosticSeverity.Error,
        2 => entry.Diagnostic.Severity == DiagnosticSeverity.Warning,
        3 => entry.Diagnostic.Severity is DiagnosticSeverity.Info or DiagnosticSeverity.Success,
        _ => true
    }) && (_filterGroup == null || GroupName(entry.Diagnostic) == _filterGroup)
        && (_filterText.Length == 0 || entry.Text.Contains(_filterText, StringComparison.OrdinalIgnoreCase));

    private void RebuildText()
    {
        _editingText = true;
        try
        {
            foreach (Entry entry in _visible)
            {
                entry.Offset = -1;
            }

            _visible.Clear();
            _selected.Clear();
            _active = null;
            ResetBuffer();
            foreach (Entry entry in _ordered)
            {
                if (Matches(entry))
                {
                    entry.Offset = _buffer.Length;
                    _visible.Add(entry);
                    _buffer.AppendLine(entry.Text);
                }
            }

            Editor.Text = _buffer.ToString();
            ResetBuffer();
            Editor.Select(0, 0);
            Editor.ScrollToHome();
        }
        finally
        {
            _editingText = false;
        }

        RefreshCount();
        ShowSelection();
    }

    private void InsertPending()
    {
        if (_pending.Count == 0)
        {
            return;
        }

        TextDocument document = Editor.Document;
        TextAnchor start = Anchor(Editor.SelectionStart, Editor.SelectionLength == 0 && Editor.SelectionStart == document.TextLength
            ? AnchorMovementType.BeforeInsertion : AnchorMovementType.AfterInsertion);
        TextAnchor end = Anchor(Editor.SelectionStart + Editor.SelectionLength, AnchorMovementType.BeforeInsertion);
        TextAnchor caret = Anchor(Editor.CaretOffset, Editor.CaretOffset == document.TextLength
            ? AnchorMovementType.BeforeInsertion : AnchorMovementType.AfterInsertion);
        IScrollable scroll = Editor.TextArea.TextView;
        Vector offset = scroll.Offset;
        _editingText = true;
        document.BeginUpdate();
        try
        {
            MergeInto(_visible);
            int position = 0;
            for (int index = 0; index < _visible.Count;)
            {
                Entry current = _visible[index];
                if (current.Offset >= 0)
                {
                    current.Offset = position;
                    position += current.Text.Length + Environment.NewLine.Length;
                    index++;
                    continue;
                }

                int insertionOffset = position;
                _buffer.Clear();
                do
                {
                    Entry entry = _visible[index++];
                    entry.Offset = position;
                    _buffer.AppendLine(entry.Text);
                    position += entry.Text.Length + Environment.NewLine.Length;
                }
                while (index < _visible.Count && _visible[index].Offset < 0);

                document.Insert(insertionOffset, _buffer.ToString());
            }
        }
        finally
        {
            document.EndUpdate();
            Editor.Select(start.Offset, Math.Max(0, end.Offset - start.Offset));
            Editor.CaretOffset = caret.Offset;
            scroll.Offset = offset;
            _editingText = false;
        }
    }

    // Merge backwards in the existing array: one pass, without per-record shifts or allocations.
    private void MergeInto(List<Entry> entries)
    {
        int previous = entries.Count - 1;
        int added = _pending.Count - 1;
        entries.AddRange(_pending);
        int destination = entries.Count - 1;
        while (added >= 0)
        {
            entries[destination--] = previous >= 0 && EntryComparer.s_instance.Compare(entries[previous], _pending[added]) > 0
                ? entries[previous--] : _pending[added--];
        }
    }

    private TextAnchor Anchor(int offset, AnchorMovementType movement)
    {
        TextAnchor anchor = Editor.Document.CreateAnchor(offset);
        anchor.MovementType = movement;
        return anchor;
    }

    private void ResetBuffer()
    {
        if (_buffer.Capacity > 65536)
        {
            _buffer = new StringBuilder();
        }
        else
        {
            _buffer.Clear();
        }
    }

    private Entry? EntryAt(int offset)
    {
        int low = 0;
        int high = _visible.Count - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) / 2);
            if (_visible[middle].Offset <= offset)
            {
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }

        return high >= 0 ? _visible[high] : null;
    }

    private void SelectionChanged()
    {
        if (_editingText)
        {
            return;
        }

        if (_selected.Count != 0)
        {
            _selected.Clear();
            Editor.TextArea.TextView.Redraw();
        }

        Entry? entry = EntryAt(Editor.CaretOffset);
        if (!ReferenceEquals(_active, entry))
        {
            _active = entry;
            ShowSelection();
        }
    }

    internal void SelectDiagnostic(int index, bool toggle = false) => SelectEntry(_visible[index], toggle);

    private void SelectEntry(Entry entry, bool toggle)
    {
        _editingText = true;
        try
        {
            if (!toggle)
            {
                _selected.Clear();
            }
            else
            {
                if (_selected.Count == 0 && _active != null)
                {
                    _selected.Add(_active);
                }

                if (!_selected.Add(entry))
                {
                    _selected.Remove(entry);
                }
            }

            _active = !toggle || _selected.Contains(entry) ? entry : _selected.FirstOrDefault();
            Editor.Select(entry.Offset, 0);
        }
        finally
        {
            _editingText = false;
        }

        Editor.TextArea.TextView.Redraw();
        ShowSelection();
    }

    internal string CopyText(bool selectedOnly)
    {
        var text = new StringBuilder();
        if (!selectedOnly)
        {
            AppendAllTo(text);
        }
        else
        {
            Entry? first = EntryAt(Editor.SelectionStart);
            Entry? last = EntryAt(Editor.SelectionStart + Math.Max(0, Editor.SelectionLength - 1));
            bool inside = false;
            foreach (Entry entry in _visible)
            {
                inside |= ReferenceEquals(entry, first);
                if (_selected.Count != 0 ? _selected.Contains(entry) : Editor.SelectionLength != 0 ? inside : ReferenceEquals(entry, _active))
                {
                    text.AppendLine(entry.Text);
                }

                if (ReferenceEquals(entry, last))
                {
                    inside = false;
                }
            }
        }

        if (text.Length > 0)
        {
            text.Length -= Environment.NewLine.Length;
        }

        return text.ToString();
    }

    internal void AppendAllTo(StringBuilder text)
    {
        foreach (Entry entry in _entries)
        {
            text.AppendLine(entry.Text);
        }
    }

    private void Count(Diagnostic diagnostic)
    {
        _errors += diagnostic.Severity == DiagnosticSeverity.Error ? 1 : 0;
        _warnings += diagnostic.Severity == DiagnosticSeverity.Warning ? 1 : 0;
    }

    private void RefreshCount()
    {
        using IDisposable language = ConfigText.UseLanguage(_language);
        _count.Text = ConfigText.Get($"显示 {DisplayedCount} / {TotalCount} 条 · 错误 {_errors} · 警告 {_warnings}",
            $"Showing {DisplayedCount} / {TotalCount} · Errors {_errors} · Warnings {_warnings}");
        _copyAll.IsEnabled = TotalCount != 0;
    }

    private sealed class Entry
    {
        internal Entry(Diagnostic diagnostic, int sequence)
        {
            Diagnostic = diagnostic;
            Text = diagnostic.ToString();
            Sequence = sequence;
        }

        internal Diagnostic Diagnostic
        {
            get;
        }
        internal string Text
        {
            get;
        }
        internal int Sequence
        {
            get;
        }
        internal int Offset
        {
            get; set;
        } = -1;
    }

    private sealed class EntryComparer : IComparer<Entry>
    {
        internal static readonly EntryComparer s_instance = new();

        public int Compare(Entry? x, Entry? y)
        {
            Diagnostic a = x!.Diagnostic;
            Diagnostic b = y!.Diagnostic;
            int result = Rank(a.Severity).CompareTo(Rank(b.Severity));
            if (result == 0)
            {
                result = StringComparer.Ordinal.Compare(GroupName(a), GroupName(b));
            }

            if (result == 0)
            {
                result = StringComparer.Ordinal.Compare(a.File, b.File);
            }

            if (result == 0)
            {
                result = StringComparer.Ordinal.Compare(a.Sheet, b.Sheet);
            }

            if (result == 0)
            {
                result = a.Row.CompareTo(b.Row);
            }

            if (result == 0)
            {
                result = a.Column.CompareTo(b.Column);
            }

            return result == 0 ? x.Sequence.CompareTo(y.Sequence) : result;
        }

        private static int Rank(DiagnosticSeverity severity) => severity == DiagnosticSeverity.Error ? 0 : severity == DiagnosticSeverity.Warning ? 1 : 2;
    }

    private void OpenSelected(bool reveal)
    {
        using IDisposable language = ConfigText.UseLanguage(_language);
        try
        {
            if (SelectedDiagnostic is not { } diagnostic || !File.Exists(diagnostic.File))
            {
                throw new FileNotFoundException(ConfigText.Get("文件已移动或不存在。", "The file was moved or no longer exists."));
            }

            string path = Path.GetFullPath(diagnostic.File);
            if (reveal && OperatingSystem.IsMacOS())
            {
                var start = new ProcessStartInfo("/usr/bin/open") { UseShellExecute = false };
                start.ArgumentList.Add("-R");
                start.ArgumentList.Add(path);
                Process.Start(start)?.Dispose();
            }
            else
            {
                Process.Start(new ProcessStartInfo(reveal ? Path.GetDirectoryName(path)! : path) { UseShellExecute = true })?.Dispose();
            }

            _actionStatus.Text = ConfigText.Get("已打开；可复制位置后在表格软件中定位。", "Opened; copy the location to navigate in your spreadsheet app.");
        }
        catch (Exception ex)
        {
            _actionStatus.Text = ex.Message;
        }
    }

    private async Task CopyAsync(string text)
    {
        try
        {
            if (TopLevel.GetTopLevel(this)?.Clipboard is not { } clipboard)
            {
                throw new InvalidOperationException(ConfigText.Get("剪贴板不可用。", "Clipboard is unavailable."));
            }

            await clipboard.SetTextAsync(text);
            _actionStatus.Text = ConfigText.Get("已复制。", "Copied.");
        }
        catch (Exception ex)
        {
            _actionStatus.Text = ex.Message;
        }
    }

    internal static string Location(Diagnostic d)
    {
        string column = "";
        for (int number = d.Column; number > 0; number = (number - 1) / 26)
        {
            column = (char)('A' + (number - 1) % 26) + column;
        }

        return d.Sheet.Length != 0 ? "'" + d.Sheet.Replace("'", "''") + "'" + (d.Row > 0 ? "!" + (column.Length != 0 ? column : "R") + d.Row : "") :
            d.File + (d.Row > 0 ? $"({d.Row},{d.Column})" : "");
    }

    private static string GroupName(Diagnostic d) => d.Table.Length != 0 ? d.Table : d.Sheet;
    private static string Suggestion(string code) => code switch
    {
        "GENERATION_REQUIRED" or "SCHEMA_MISMATCH" or "CODEC_MISSING" or "EXCEL_TABLE_MISSING" => ConfigText.Get("检查完整输入目录，预览并生成代码，然后重新校验。", "Check the complete input directory, review and generate code, then validate again."),
        "KEY_DUPLICATE" or "PRIMARY_KEY_DUPLICATE" => ConfigText.Get("检查重复记录；组合主键的完整组合必须唯一。", "Review duplicate records; the complete composite key must be unique."),
        "TABLE_REFERENCE" or "REFERENCE_MISSING" => ConfigText.Get("检查引用值、目标表主键和目标表自身的错误。", "Check the reference value, target key and errors in the target table."),
        "CELL_PARSE" => ConfigText.Get("按照字段类型和填写语法修正单元格，保存 Excel 后重新校验。", "Correct the cell using the declared type and syntax, save Excel, then validate again."),
        "EXCEL_OPEN" or "INPUT_SCAN" => ConfigText.Get("保存工作簿，检查文件格式和访问权限，然后重试。", "Save the workbook, check its format and access permissions, then retry."),
        "GENERATION_CHANGED" => ConfigText.Get("输入或生成文件已变化，请重新打开生成预览。", "Inputs or generated files changed. Prepare a fresh generation preview."),
        _ => ConfigText.Get("修正说明中的问题并保存文件；规则或编译问题可复制诊断交给项目程序员。", "Fix the reported issue and save. Copy rule or compilation diagnostics for the project programmer if needed.")
    };
}
