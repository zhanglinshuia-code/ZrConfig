using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;

namespace ZrConfig.App;

internal static class DirectoryDrop
{
    public static void Enable(TextBox input)
    {
        DragDrop.SetAllowDrop(input, true);
        input.AddHandler(DragDrop.DragEnterEvent, OnDragOver, handledEventsToo: true);
        input.AddHandler(DragDrop.DragOverEvent, OnDragOver, handledEventsToo: true);
        input.AddHandler(DragDrop.DropEvent, OnDrop, handledEventsToo: true);
    }

    private static bool CanAccept(TextBox input, DragEventArgs args)
    {
        return input.IsEffectivelyEnabled && !input.IsReadOnly && (args.DragEffects & DragDropEffects.Copy) != 0 && args.DataTransfer.Contains(DataFormat.File);
    }

    private static void OnDragOver(object? sender, DragEventArgs args)
    {
        // 高频移动只检查格式，避免反复读取拖拽内容或访问文件系统。
        args.DragEffects = sender is TextBox input && CanAccept(input, args) ? DragDropEffects.Copy : DragDropEffects.None;
        args.Handled = true;
    }

    private static void OnDrop(object? sender, DragEventArgs args)
    {
        bool accepted = false;
        if (sender is TextBox input && CanAccept(input, args))
        {
            try
            {
                if (TryGetDirectory(args.DataTransfer, out string? directory))
                {
                    input.Text = directory;
                    accepted = true;
                }
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException)
            {
                // 非本地或无效路径不改变原输入，也不让拖拽事件异常中断界面。
            }
        }

        // 只读取路径。绝不返回 Move，避免拖拽来源删除或移动实际文件。
        args.DragEffects = accepted ? DragDropEffects.Copy : DragDropEffects.None;
        args.Handled = true;
    }

    private static bool TryGetDirectory(IDataTransfer data, out string? directory)
    {
        directory = null;
        IStorageItem[]? items = data.TryGetFiles();
        if (items == null || items.Length == 0)
        {
            return false;
        }

        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        foreach (IStorageItem item in items)
        {
            string? path = item.TryGetLocalPath();
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            path = Path.GetFullPath(path);
            string? candidate = item is IStorageFolder ? path : item is IStorageFile ? Path.GetDirectoryName(path) : null;
            if (string.IsNullOrEmpty(candidate))
            {
                return false;
            }

            candidate = Path.TrimEndingDirectorySeparator(candidate);
            // 多个文件仅在目录一致时接受，避免悄悄选择错误的目录。
            if (directory != null && !string.Equals(directory, candidate, comparison))
            {
                return false;
            }

            directory = candidate;
        }

        return true;
    }
}
