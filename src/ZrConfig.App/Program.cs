using Avalonia;

namespace ZrConfig.App;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        _ = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<ZrConfigApplication>().UsePlatformDetect().LogToTrace();
    }
}
