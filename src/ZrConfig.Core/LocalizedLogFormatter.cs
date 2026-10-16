using System.Globalization;
using Serilog.Events;
using Serilog.Formatting;
using ZrConfig.Runtime;

namespace ZrConfig.Core;
/// <summary>异步日志写入线程使用本次运行的固定语言，避免受到界面切换或其他任务影响。</summary>
internal sealed class LocalizedLogFormatter(string language) : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        using IDisposable scope = ConfigText.UseLanguage(language);
        string level = logEvent.Level switch
        {
            LogEventLevel.Verbose => ConfigText.Get("详细", "Verbose"),
            LogEventLevel.Debug => ConfigText.Get("调试", "Debug"),
            LogEventLevel.Information => ConfigText.Get("信息", "Info"),
            LogEventLevel.Warning => ConfigText.Get("警告", "Warning"),
            LogEventLevel.Error => ConfigText.Get("错误", "Error"),
            _ => ConfigText.Get("严重错误", "Fatal")
        };
        if (logEvent.Level == LogEventLevel.Information && logEvent.Properties.TryGetValue("DiagnosticSeverity",
            out LogEventPropertyValue? severity) && severity is ScalarValue { Value: "Success" })
        {
            level = ConfigText.Get("成功", "Success");
        }

        output.Write(logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
        output.Write(" [" + level + "] ");
        output.WriteLine(logEvent.RenderMessage(CultureInfo.InvariantCulture));
        if (logEvent.Exception != null)
        {
            output.WriteLine(logEvent.Exception);
        }
    }
}
