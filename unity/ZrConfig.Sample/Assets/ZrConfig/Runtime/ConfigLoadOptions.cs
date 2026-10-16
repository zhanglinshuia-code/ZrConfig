#nullable enable
using System;
using System.Diagnostics;
using System.Threading;

namespace ZrConfig.Runtime
{
    /// <summary>每个管理器固定的加载策略。默认在调用线程解码；后台调度只影响异步冷加载。</summary>
    public sealed class ConfigLoadOptions
    {
        public bool EnableDiagnostics
        {
            get;
        }
        public IConfigLoadScheduler? Scheduler
        {
            get;
        }

        /// <summary>服务端可保留请求已持有的旧快照；管理器不持有历史列表，由调用方释放引用后回收。</summary>
        public bool RetainPreviousSnapshots
        {
            get;
        }
        /// <summary>双格式版本优先读取 JSON；默认仍使用二进制。</summary>
        public bool PreferJson
        {
            get;
        }

        public ConfigLoadOptions(bool enableDiagnostics = false, IConfigLoadScheduler? scheduler = null,
            bool retainPreviousSnapshots = false, bool preferJson = false)
        {
            EnableDiagnostics = enableDiagnostics;
            Scheduler = scheduler;
            RetainPreviousSnapshots = retainPreviousSnapshots;
            PreferJson = preferJson;
        }
    }

    /// <summary>只调度校验、解码和索引构建；资源读取仍由 source 决定。必须执行一次并传递异常/取消。</summary>
    public interface IConfigLoadScheduler
    {
        ConfigOperation<T> Execute<T>(Func<T> work, CancellationToken cancellationToken);
    }

    /// <summary>显式启用前须确认编解码器/转换器线程安全且不访问 Unity 主线程 API。WebGL 不支持此策略。</summary>
    public sealed class BackgroundConfigLoadScheduler : IConfigLoadScheduler
    {
        public static BackgroundConfigLoadScheduler Instance
        {
            get;
        } = new BackgroundConfigLoadScheduler();

        private BackgroundConfigLoadScheduler()
        {
        }

        public ConfigOperation<T> Execute<T>(Func<T> work, CancellationToken cancellationToken)
        {
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return ConfigOperation<T>.FromCanceled(cancellationToken);
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            return ConfigOperation<T>.FromException(new PlatformNotSupportedException("Background configuration loading requires threads."));
#else
            var completion = new ConfigCompletionSource<T>(true);
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    T result = work();
                    cancellationToken.ThrowIfCancellationRequested();
                    completion.TrySetResult(result);
                }
                catch (OperationCanceledException error)
                {
                    completion.TrySetCanceled(error.CancellationToken);
                }
                catch (Exception error)
                {
                    completion.TrySetException(error);
                }
            });
            return completion.Operation;
#endif
        }
    }

    /// <summary>诊断读数。时间累计包含失败尝试；读取时间包含 source 等待，非独占 I/O 时间。字节数仅统计驻留的原始表数据。</summary>
    public readonly struct ConfigLoadDiagnostics
    {
        public bool Enabled
        {
            get;
        }
        public int OpenedTableCount
        {
            get;
        }
        public long RetainedBytes
        {
            get;
        }
        public int LoadedRowCount
        {
            get;
        }
        public int CompletedTableCount
        {
            get;
        }
        public TimeSpan ReadTime
        {
            get;
        }
        public TimeSpan HashTime
        {
            get;
        }
        public TimeSpan IndexTime
        {
            get;
        }
        public TimeSpan DecodeTime
        {
            get;
        }
        public TimeSpan TableBuildTime
        {
            get;
        }

        internal ConfigLoadDiagnostics(LoadMetrics metrics, int rows)
        {
            Enabled = true;
            OpenedTableCount = Volatile.Read(ref metrics._openedTables);
            RetainedBytes = Interlocked.Read(ref metrics._bytes);
            LoadedRowCount = rows;
            CompletedTableCount = Volatile.Read(ref metrics._completedTables);
            ReadTime = Elapsed(ref metrics._readTicks);
            HashTime = Elapsed(ref metrics._hashTicks);
            IndexTime = Elapsed(ref metrics._indexTicks);
            DecodeTime = Elapsed(ref metrics._decodeTicks);
            TableBuildTime = Elapsed(ref metrics._tableTicks);
        }

        private static TimeSpan Elapsed(ref long ticks) => TimeSpan.FromSeconds((double)Interlocked.Read(ref ticks) / Stopwatch.Frequency);
    }

    internal sealed class LoadMetrics
    {
        internal int _openedTables;
        internal int _completedTables;
        internal long _bytes;
        internal long _readTicks;
        internal long _hashTicks;
        internal long _indexTicks;
        internal long _decodeTicks;
        internal long _tableTicks;
        internal static long Start(LoadMetrics? metrics) => metrics == null ? 0 : Stopwatch.GetTimestamp();
        internal static void Add(ref long target, long start) => Interlocked.Add(ref target, Stopwatch.GetTimestamp() - start);
    }
}
