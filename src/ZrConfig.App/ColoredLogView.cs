using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using ZrConfig.Runtime;

namespace ZrConfig.App;

internal enum LogTone
{
    Information,
    Success,
    Warning,
    Error
}

/// <summary>单个只读文本控件保存日志；仅排版可见文字，级别由文本高亮绘制。</summary>
internal sealed class ColoredLogView : Border
{
    // 状态栏文字配色；顺序：普通信息、成功、警告、错误。
    private static readonly IBrush[] s_lightColors = [Brushes.Transparent, Brush.Parse("#007F36"), Brush.Parse("#C2410C"), Brush.Parse("#D70022")];
    private static readonly IBrush[] s_darkColors = [Brushes.Transparent, Brush.Parse("#4DFF91"), Brush.Parse("#FFBC42"), Brush.Parse("#FF6B75")];
    // 日志保留绿色成功、黄色警告、蓝色错误；亮度与背景随主题调整。
    private static readonly IBrush[] s_lightLogColors = [Brushes.Black, Brushes.Green, Brush.Parse("#806000"), Brushes.Blue];
    private static readonly IBrush[] s_darkLogColors = [Brush.Parse("#F3F4F6"), Brush.Parse("#4DFF91"), Brush.Parse("#FFDB70"), Brush.Parse("#8CBFFF")];
    private static readonly IBrush s_darkLogBackground = Brush.Parse("#202020");
    private static readonly FontFamily s_logFont = new("Menlo, Consolas, monospace");
    private static readonly Typeface s_boldFont = new(s_logFont, weight: FontWeight.Bold);
    private readonly TextEditor _editor;
    private readonly MenuItem _copy = new();
    private readonly MenuItem _copyAll = new();
    private readonly Button _clear = new()
    {
        Margin = new Thickness(12, 0, 0, 0),
        VerticalAlignment = VerticalAlignment.Center
    };
    private readonly SelectableTextBlock _header;
    private readonly Run _legendTitle = new();
    private readonly Run[] _legendItems = new Run[s_lightLogColors.Length];
    // 每行仅存一个级别值，不创建对应的 UI 控件。
    private readonly List<LogTone> _tones = new();
    private string? _headerText;
    private LogTone _fallback;
    private string? _pendingText;
    private LogTone _pendingTone;
    private StringBuilder? _pendingAppend;
    internal bool DeferWhileDetached
    {
        get; init;
    }
    public ColoredLogView(bool showToolbar = true)
    {
        BorderThickness = new Thickness(1);
        BorderBrush = Brush.Parse("#60808080");
        CornerRadius = new CornerRadius(4);
        Padding = new Thickness(10);
        Styles.Add(new StyleInclude(new Uri("avares://AvaloniaEdit/")) { Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml") });
        var menu = new ContextMenu
        {
            ItemsSource = new[]
            {
                _copy,
                _copyAll
            }
        };
        _editor = new TextEditor
        {
            IsReadOnly = true,
            ShowLineNumbers = false,
            WordWrap = false,
            FontFamily = s_logFont,
            FontSize = 13,
            Background = Brushes.Transparent,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            ContextMenu = menu
        };
        _editor.Options.EnableHyperlinks = false;
        _editor.Options.EnableEmailHyperlinks = false;
        _editor.Options.HighlightCurrentLine = false;
        _editor.Options.AllowScrollBelowDocument = false;
        _editor.Options.EnableTextDragDrop = false;
        _editor.Options.EnableRectangularSelection = false;
        _editor.Document.UndoStack.SizeLimit = 0;
        _editor.TextArea.SelectionForeground = null;
        _editor.TextArea.SelectionBrush = Brush.Parse("#60808080");
        _editor.TextArea.TemplateApplied += (_, e) =>
        {
            if (e.NameScope.Find<TextBlock>("PART_Watermark") is { } watermark)
            {
                // AvaloniaEdit 12 binds this to Document.Text, copying the entire log on every update.
                watermark.Bind(IsVisibleProperty, new Binding("Document.TextLength")
                {
                    Source = _editor.TextArea,
                    Converter = new FuncValueConverter<int, bool>(length => length == 0)
                });
            }
        };
        var highlighting = new LogHighlighting(this);
        _editor.TextArea.TextView.LineTransformers.Add(highlighting);
        var legend = new TextBlock
        {
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
        legend.Inlines!.Add(_legendTitle);
        for (int index = 0; index < _legendItems.Length; index++)
        {
            // 图例与正文共用配色，修改颜色时两处始终保持一致。
            var item = new Run
            {
                FontWeight = index == (int)LogTone.Information ? FontWeight.Normal : FontWeight.Bold
            };
            _legendItems[index] = item;
            if (index > 0)
            {
                legend.Inlines.Add(new Run("    "));
            }

            legend.Inlines.Add(item);
        }

        _header = new SelectableTextBlock
        {
            IsVisible = false,
            FontFamily = s_logFont,
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 12),
            ContextMenu = menu
        };
        menu.Opening += (_, _) => _copy.IsEnabled = _editor.SelectionLength > 0 || _header.CanCopy;
        _copy.Click += (_, _) =>
        {
            if (_editor.SelectionLength > 0)
            {
                _editor.Copy();
            }
            else
            {
                _header.Copy();
            }
        };
        _copyAll.Click += async (_, _) => await CopyAllAsync();
        _clear.Click += (_, _) =>
        {
            if (_clear.IsEnabled)
            {
                Clear();
                Cleared?.Invoke(this, EventArgs.Empty);
            }
        };
        ContextMenu = menu;
        var toolbar = new Grid
        {
            IsVisible = showToolbar,
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(0, 0, 0, 10)
        };
        toolbar.Children.Add(legend);
        Grid.SetColumn(_clear, 1);
        toolbar.Children.Add(_clear);
        // 给编辑器明确的可视高度；外层不套滚动容器，避免展开整份文档。
        var content = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*")
        };
        content.Children.Add(toolbar);
        Grid.SetRow(_header, 1);
        content.Children.Add(_header);
        Grid.SetRow(_editor, 2);
        content.Children.Add(_editor);
        Child = content;
        ActualThemeVariantChanged += (_, _) => RefreshLogColors();
        AttachedToVisualTree += (_, _) => RenderPendingText();
        RefreshLogColors();
        RefreshLanguage();
    }

    private IBrush[] LogColors => ActualThemeVariant == ThemeVariant.Dark ? s_darkLogColors : s_lightLogColors;

    // Diagnostic text supplies its own offset index; ordinary logs keep the existing line parser.
    internal TextEditor Editor
    {
        get
        {
            RenderPendingText();
            return _editor;
        }
    }
    internal Func<int, LogTone>? ToneAtOffset
    {
        get; set;
    }
    internal Func<int, bool>? IsSelectedAtOffset
    {
        get; set;
    }

    public event EventHandler? Cleared;

    public bool IsClearEnabled
    {
        get => _clear.IsEnabled;
        set => _clear.IsEnabled = value;
    }

    public void Clear()
    {
        _pendingText = null;
        _pendingAppend = null;
        _fallback = LogTone.Information;
        _tones.Clear();
        _editor.Text = string.Empty;
        _editor.Select(0, 0);
        _editor.ScrollToHome();
        SetHeader(null);
    }

    private void RefreshLogColors()
    {
        IBrush[] colors = LogColors;
        Background = ActualThemeVariant == ThemeVariant.Dark ? s_darkLogBackground : Brushes.White;
        _editor.Foreground = colors[(int)LogTone.Information];
        _header.Foreground = colors[(int)LogTone.Information];
        _legendTitle.Foreground = colors[(int)LogTone.Information];
        for (int index = 0; index < _legendItems.Length; index++)
        {
            _legendItems[index].Foreground = colors[index];
        }

        // 重新绘制现有文字，保留文档、选区与滚动位置。
        _editor.TextArea.TextView.Redraw();
    }

    private async Task CopyAllAsync()
    {
        if (TopLevel.GetTopLevel(this)?.Clipboard is not { } clipboard)
        {
            return;
        }

        RenderPendingText();
        string content = _editor.Text;
        string text = _headerText == null ? content : _headerText + Environment.NewLine + Environment.NewLine + content;
        try
        {
            await clipboard.SetTextAsync(text);
        }
        catch (Exception ex) when (ex is IOException or System.Runtime.InteropServices.ExternalException or NotSupportedException)
        {
            System.Diagnostics.Trace.TraceWarning(ConfigText.Get($"复制日志失败：{ex.Message}", $"Failed to copy log: {ex.Message}"));
        }
    }

    public void RefreshLanguage()
    {
        _copy.Header = ConfigText.Get("复制", "Copy");
        _copyAll.Header = ConfigText.Get("复制全部日志", "Copy All Logs");
        _clear.Content = ConfigText.Get("清理日志", "Clear Logs");
        ToolTip.SetTip(_clear, ConfigText.Get("清空窗口中的日志，保留磁盘日志文件。", "Clear displayed logs; keep log files on disk."));
        _legendTitle.Text = ConfigText.Get("日志颜色：", "Log colors: ");
        _legendItems[(int)LogTone.Information].Text = ConfigText.Get("正常", "Info");
        _legendItems[(int)LogTone.Success].Text = ConfigText.Get("成功（绿色）", "Success (green)");
        _legendItems[(int)LogTone.Warning].Text = ConfigText.Get("警告（黄色）", "Warning (yellow)");
        _legendItems[(int)LogTone.Error].Text = ConfigText.Get("错误（蓝色）", "Error (blue)");
    }

    public void SetHeader(string? header)
    {
        if (_headerText == header)
        {
            return;
        }

        _headerText = header;
        _header.Text = header;
        _header.IsVisible = header != null;
    }

    /// <summary>相同内容不操作；尾部增加时追加；新一轮日志才替换文档内容。</summary>
    public void SetText(string text, LogTone fallback = LogTone.Information)
    {
        // A new operation clears the old document immediately, even in an unopened tab.
        if (text.Length == 0)
        {
            _pendingText = null;
            _pendingAppend = null;
            SetTextCore(text, fallback);
            return;
        }

        if (ShouldDefer)
        {
            _pendingText = text;
            _pendingTone = fallback;
            _pendingAppend = null;
            return;
        }

        RenderPendingText();
        SetTextCore(text, fallback);
    }

    private void SetTextCore(string text, LogTone fallback)
    {
        string current = _editor.Text;
        if (fallback == _fallback && text.StartsWith(current, StringComparison.Ordinal))
        {
            AppendTextCore(text[current.Length..]);
            return;
        }

        _fallback = fallback;
        _tones.Clear();
        _editor.Text = text;
        UpdateTones(1);
        _editor.ScrollToHome();
    }

    /// <summary>仅插入新增文本并更新受影响的末行级别，保留选择和阅读位置。</summary>
    public void AppendText(string text)
    {
        if (ShouldDefer)
        {
            if (text.Length != 0)
            {
                (_pendingAppend ??= new StringBuilder()).Append(text);
            }

            return;
        }

        RenderPendingText();
        AppendTextCore(text);
    }

    private bool ShouldDefer => DeferWhileDetached && VisualRoot == null;

    // Hidden tabs retain plain text; create/update their document only when viewed or copied.
    private void RenderPendingText()
    {
        if (_pendingText is { } text)
        {
            _pendingText = null;
            SetTextCore(text, _pendingTone);
        }

        if (_pendingAppend is { } append)
        {
            _pendingAppend = null;
            AppendTextCore(append.ToString());
        }
    }

    private void AppendTextCore(string text)
    {
        if (text.Length == 0)
        {
            return;
        }

        TextDocument document = _editor.Document;
        int firstLine = document.GetLineByOffset(Math.Max(0, document.TextLength - 1)).LineNumber;
        Selection selection = _editor.TextArea.Selection;
        int caret = _editor.CaretOffset;
        IScrollable scroll = _editor.TextArea.TextView;
        Vector offset = scroll.Offset;
        _editor.AppendText(text);
        UpdateTones(firstLine);
        // 文末插入也不扩展原有选择或将光标带到新增日志末尾。
        _editor.TextArea.Selection = selection;
        _editor.CaretOffset = caret;
        scroll.Offset = offset;
    }

    public static IBrush? ColorFor(LogTone tone, ThemeVariant theme)
    {
        return tone == LogTone.Information ? null : (theme == ThemeVariant.Dark ? s_darkColors : s_lightColors)[(int)tone];
    }

    private void UpdateTones(int firstLine)
    {
        if (ToneAtOffset != null)
        {
            return;
        }

        int start = firstLine - 1;
        if (start < _tones.Count)
        {
            _tones.RemoveRange(start, _tones.Count - start);
        }

        LogTone previous = start == 0 ? _fallback : _tones[start - 1];
        TextDocument document = _editor.Document;
        for (DocumentLine? line = document.GetLineByNumber(firstLine); line != null; line = line.NextLine)
        {
            previous = ToneFor(document.GetText(line.Offset, line.Length), previous, _fallback);
            _tones.Add(previous);
        }
    }

    private LogTone ToneAt(int lineNumber)
    {
        return lineNumber > 0 && lineNumber <= _tones.Count ? _tones[lineNumber - 1] : _fallback;
    }

    private sealed class LogHighlighting : DocumentColorizingTransformer
    {
        private readonly ColoredLogView _owner;
        public LogHighlighting(ColoredLogView owner)
        {
            _owner = owner;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            LogTone tone = _owner.ToneAtOffset?.Invoke(line.Offset) ?? _owner.ToneAt(line.LineNumber);
            bool selected = _owner.IsSelectedAtOffset?.Invoke(line.Offset) == true;
            ChangeLinePart(line.Offset, line.EndOffset, element =>
            {
                element.TextRunProperties.SetForegroundBrush(_owner.LogColors[(int)tone]);
                if (selected)
                {
                    element.TextRunProperties.SetBackgroundBrush(_owner._editor.TextArea.SelectionBrush);
                }
                if (tone != LogTone.Information)
                {
                    element.TextRunProperties.SetTypeface(s_boldFont);
                }
            });
        }
    }

    private static bool HasTimestampPrefix(ReadOnlySpan<char> text)
    {
        if (text.Length < 24 || text[23] != ' ')
        {
            return false;
        }

        for (int i = 0; i < 23; i++)
        {
            char separator = i switch
            {
                4 or 7 => '-',
                10 => ' ',
                13 or 16 => ':',
                19 => '.',
                _ => '\0'
            };
            if (separator == '\0' ? !char.IsDigit(text[i]) : text[i] != separator)
            {
                return false;
            }
        }

        return true;
    }

    private static LogTone ToneFor(string line, LogTone previous, LogTone fallback)
    {
        ReadOnlySpan<char> text = line.AsSpan();
        if (text.IsWhiteSpace() || text.StartsWith("---", StringComparison.Ordinal))
        {
            return fallback;
        }

        ReadOnlySpan<char> prefix = HasTimestampPrefix(text) ? text[24..] : text;
        int closing = prefix.IndexOf(']');
        bool hasPrefix = prefix.Length > 2 && prefix[0] == '[' && closing > 1;
        ReadOnlySpan<char> message = text;
        if (hasPrefix)
        {
            switch (prefix[1..closing])
            {
                case "错误":
                case "严重错误":
                case "Error":
                case "Fatal":
                case "ERR":
                case "FTL":
                    return LogTone.Error;
                case "警告":
                case "Warning":
                case "WRN":
                    return LogTone.Warning;
                case "成功":
                case "Success":
                    return LogTone.Success;
                case "信息":
                case "Info":
                case "INF":
                case "调试":
                case "Debug":
                case "DBG":
                case "详细":
                case "Verbose":
                case "VRB":
                    message = prefix[(closing + 1)..].TrimStart();
                    break;
                default:
                    return fallback;
            }
        }

        if (message.StartsWith("运行结束。成功=否",
            StringComparison.Ordinal) || message.StartsWith("Run finished. Success=No",
            StringComparison.Ordinal))
        {
            return LogTone.Error;
        }

        if (message.StartsWith("已发布版本 ",
            StringComparison.Ordinal) || message.StartsWith("Published version ",
            StringComparison.Ordinal) || message.StartsWith("运行结束。成功=是",
            StringComparison.Ordinal) || message.StartsWith("Run finished. Success=Yes",
            StringComparison.Ordinal) || message.StartsWith("规则自动编译完成",
            StringComparison.Ordinal) || message.StartsWith("Rules compiled successfully;",
            StringComparison.Ordinal) || message.StartsWith("规则未变化，已加载缓存。",
            StringComparison.Ordinal) || message.StartsWith("Rules unchanged; loaded the cached compilation.",
            StringComparison.Ordinal) || message.StartsWith("已创建：",
            StringComparison.Ordinal) || message.StartsWith("Created: ",
            StringComparison.Ordinal))
        {
            return LogTone.Success;
        }

        // 异常和多行诊断的后续行延续级别；下一个明确的日志级别或空行结束延续。
        return !hasPrefix && previous is LogTone.Error or LogTone.Warning ? previous : fallback;
    }
}
