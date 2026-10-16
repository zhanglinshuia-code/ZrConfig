#nullable enable
using System;
using System.Threading;

namespace ZrConfig.Runtime
{
    /// <summary>为当前异步操作提供语言环境，避免并行导出相互影响；不改变数值解析区域设置。</summary>
    public static class ConfigText
    {
        public const string Chinese = "zh-CN";
        public const string English = "en";
        private static readonly AsyncLocal<string?> s_currentLanguage = new AsyncLocal<string?>();
        public static string Language => s_currentLanguage.Value ?? Chinese;

        public static string Get(string chinese, string english) => Language == English ? english : chinese;
        public static string Boolean(bool value) => value ? Get("是", "Yes") : Get("否", "No");
        public static IDisposable UseLanguage(string language)
        {
            if (language != Chinese && language != English)
            {
                throw new ArgumentException("不支持的语言 / Unsupported language: " + language, nameof(language));
            }

            var scope = new LanguageScope(s_currentLanguage.Value);
            s_currentLanguage.Value = language;
            return scope;
        }

        private sealed class LanguageScope : IDisposable
        {
            private readonly string? _previous;
            private bool _disposed;
            public LanguageScope(string? previous) => _previous = previous;
            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                s_currentLanguage.Value = _previous;
                _disposed = true;
            }
        }
    }
}
