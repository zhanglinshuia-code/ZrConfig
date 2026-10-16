#nullable enable
using System;
using System.Threading;

namespace ZrConfig.Runtime
{
    /// <summary>不可变预加载清单。仅加载显式指定的行/表，不自动展开引用；每次 With 调用返回新清单。</summary>
    public sealed class ConfigPreloadPlan
    {
        private readonly IStep[] _steps;
        public ConfigPreloadPlan() => _steps = Array.Empty<IStep>();
        private ConfigPreloadPlan(IStep[] steps) => _steps = steps;
        public ConfigPreloadPlan WithRow<T, TKey>(ITableCodec codec, TKey key)
            where T : class, IConfigRow => Append(new Row<T, TKey>(CheckCodec<T>(codec), key));
        public ConfigPreloadPlan WithTable<T>(ITableCodec codec)
            where T : class, IConfigRow => Append(new Table<T>(CheckCodec<T>(codec)));
        private static ITableCodec CheckCodec<T>(ITableCodec codec)
        {
            if (codec == null)
            {
                throw new ArgumentNullException(nameof(codec));
            }

            if (codec.RowType != typeof(T))
            {
                throw new ArgumentException("Codec row type mismatch.", nameof(codec));
            }

            return codec;
        }

        private ConfigPreloadPlan Append(IStep step)
        {
            var steps = new IStep[_steps.Length + 1];
            Array.Copy(_steps, steps, _steps.Length);
            steps[steps.Length - 1] = step;
            return new ConfigPreloadPlan(steps);
        }

        internal void Run(ConfigSnapshot snapshot, CancellationToken token)
        {
            foreach (IStep step in _steps)
            {
                token.ThrowIfCancellationRequested();
                step.Run(snapshot, token);
            }
        }

        internal async ConfigOperation<bool> RunAsync(ConfigSnapshot snapshot, CancellationToken token)
        {
            foreach (IStep step in _steps)
            {
                token.ThrowIfCancellationRequested();
                await step.RunAsync(snapshot, token);
            }

            return true;
        }

        private interface IStep
        {
            void Run(ConfigSnapshot snapshot, CancellationToken token);
            ConfigOperation<bool> RunAsync(ConfigSnapshot snapshot, CancellationToken token);
        }

        private sealed class Row<T, TKey> : IStep where T : class, IConfigRow
        {
            private readonly ITableCodec _codec;
            private readonly TKey _key;
            internal Row(ITableCodec codec, TKey key)
            {
                _codec = codec;
                _key = key;
            }

            public void Run(ConfigSnapshot snapshot, CancellationToken token) => snapshot.GetRow<T, TKey>(_codec, _key, token);
            public async ConfigOperation<bool> RunAsync(ConfigSnapshot snapshot, CancellationToken token)
            {
                await snapshot.GetRowAsync<T, TKey>(_codec, _key, token);
                return true;
            }
        }

        private sealed class Table<T> : IStep where T : class, IConfigRow
        {
            private readonly ITableCodec _codec;
            internal Table(ITableCodec codec) => _codec = codec;
            public void Run(ConfigSnapshot snapshot, CancellationToken token) => snapshot.GetTable<T>(_codec, token);
            public async ConfigOperation<bool> RunAsync(ConfigSnapshot snapshot, CancellationToken token)
            {
                await snapshot.GetTableAsync<T, ConfigTable<T>>(_codec, token);
                return true;
            }
        }
    }
}
