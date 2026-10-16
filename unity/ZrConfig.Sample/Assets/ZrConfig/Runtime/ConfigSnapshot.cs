#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace ZrConfig.Runtime
{
    // 由生成的不可变配置行实现，编辑器适配层也使用此接口。
    public interface IConfigRow
    {
    }

    public interface IConfigTable
    {
        string Name
        {
            get;
        }

        Type RowType
        {
            get;
        }

        IList Rows
        {
            get;
        }
    }

    /// <summary>提供当前数据集中的表；生成的查询入口同时用于运行时快照与导出校验。</summary>
    public interface IConfigTableProvider
    {
        ConfigTable<T> GetTable<T>(ITableCodec codec)
            where T : class, IConfigRow;
    }

    public class ConfigTable<T> : IConfigTable, IReadOnlyList<T> where T : class, IConfigRow
    {
        private readonly List<T> _rows;
        private readonly ReadOnlyCollection<T> _readOnlyRows;
        public string Name
        {
            get;
        }
        public Type RowType => typeof(T);
        public IList Rows => _readOnlyRows;
        public int Count => _rows.Count;

        public T this[int index] => _rows[index];
        public ConfigTable(string name, IEnumerable<T> rows)
        {
            Name = name;
            _rows = new List<T>(rows);
            _readOnlyRows = _rows.AsReadOnly();
        }

        // 显式工厂，每次调用都会构建；生成的表在构造时调用一次并保留结果。
        // 键由调用方明确指定，不自动把名为 Id 的属性视作主键。
        public IReadOnlyDictionary<TKey, T> CreateIndex<TKey>(Func<T, TKey> keySelector)
            where TKey : notnull => CreateIndex(keySelector, null);
        public IReadOnlyDictionary<TKey, T> CreateIndex<TKey>(Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer)
            where TKey : notnull
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            var index = new Dictionary<TKey, T>(_rows.Count, comparer);
            foreach (T row in _rows)
            {
                index.Add(keySelector(row), row);
            }

            return new ReadOnlyDictionary<TKey, T>(index);
        }

        /// <summary>显式构建分组索引；生成的表在加载时构建一次，查询返回已有只读分组。</summary>
        public IReadOnlyDictionary<TKey, ConfigRowGroup<T>> CreateGroupIndex<TKey>(Func<T, TKey> keySelector)
            where TKey : notnull => CreateGroupIndex(keySelector, null);
        public IReadOnlyDictionary<TKey, ConfigRowGroup<T>> CreateGroupIndex<TKey>(Func<T, TKey> keySelector,
            IEqualityComparer<TKey>? comparer)
            where TKey : notnull
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            _ = ConfigRowGroup<T>.Empty; // 在加载期初始化，首次查询空组也不临时构造。
            var groups = new Dictionary<TKey, List<T>>(comparer);
            foreach (T row in _rows)
            {
                TKey key = keySelector(row);
                if (!groups.TryGetValue(key, out List<T>? group))
                {
                    groups.Add(key, group = new List<T>());
                }

                group.Add(row);
            }

            var result = new Dictionary<TKey, ConfigRowGroup<T>>(groups.Count, comparer);
            foreach (KeyValuePair<TKey, List<T>> pair in groups)
            {
                result.Add(pair.Key, new ConfigRowGroup<T>(pair.Value));
            }

            return new ReadOnlyDictionary<TKey, ConfigRowGroup<T>>(result);
        }

        /// <summary>唯一键及同字段分组共用一份索引；保留现有分组和枚举器类型。</summary>
        protected IReadOnlyDictionary<TKey, ConfigRowGroup<T>> CreateUniqueGroupIndex<TKey>(Func<T, TKey> keySelector,
            IEqualityComparer<TKey>? comparer = null) where TKey : notnull
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            _ = ConfigRowGroup<T>.Empty;
            var result = new Dictionary<TKey, ConfigRowGroup<T>>(_rows.Count, comparer);
            foreach (T row in _rows)
            {
                TKey key = keySelector(row);
                var members = new List<T>(1) { row };
                result.Add(key, new ConfigRowGroup<T>(members));
            }

            return new ReadOnlyDictionary<TKey, ConfigRowGroup<T>>(result);
        }

        public List<T>.Enumerator GetEnumerator() => _rows.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class ConfigSnapshot : IConfigTableProvider
    {
        public string VersionId
        {
            get;
        }

        private readonly ConfigLazyLoader? _loader;
        /// <summary>已完整加载的表，不触发资源读取；仅按行加载的表不在此集合中。</summary>
        public IReadOnlyList<IConfigTable> Tables => _loader?.Tables ?? Array.Empty<IConfigTable>();
        public int LoadedRowCount => _loader?.LoadedRowCount ?? 0;
        public ConfigLoadDiagnostics Diagnostics => _loader?.Diagnostics ?? default;

        internal ConfigSnapshot(string versionId) => VersionId = versionId;

        internal ConfigSnapshot(string versionId,
            IConfigSource source,
            IEnumerable<ConfigManifestTable> entries,
            ITableCodecRegistry? registry,
            ConfigManager? manager,
            ConfigLoadOptions? options = null) : this(versionId)
        {
            _loader = new ConfigLazyLoader(versionId, source, entries, registry, manager, options);
        }

        internal bool HasLoader(ConfigLazyLoader loader) => ReferenceEquals(_loader, loader);

        /// <summary>直接使用目标表的编解码器，不访问全表注册表。</summary>
        public T GetRow<T, TKey>(ITableCodec codec, TKey key, CancellationToken cancellationToken = default)
            where T : class, IConfigRow
        {
            if (codec == null)
            {
                throw new ArgumentNullException(nameof(codec));
            }

            return Loader.GetRow<T, TKey>(typeof(T), key, cancellationToken, codec);
        }

        public ConfigOperation<T> GetRowAsync<T, TKey>(ITableCodec codec, TKey key, CancellationToken cancellationToken = default)
            where T : class, IConfigRow
        {
            try
            {
                if (codec == null)
                {
                    throw new ArgumentNullException(nameof(codec));
                }

                return Loader.GetRowAsync<T, TKey>(typeof(T), key, cancellationToken, codec);
            }
            catch (Exception error)
            {
                return ConfigOperation<T>.FromException(error);
            }
        }

        public ConfigTable<T> GetTable<T>(ITableCodec codec, CancellationToken cancellationToken = default)
            where T : class, IConfigRow
        {
            if (codec == null)
            {
                throw new ArgumentNullException(nameof(codec));
            }

            return Loader.GetTable<ConfigTable<T>>(typeof(T), cancellationToken, codec);
        }

        public ConfigOperation<TTable> GetTableAsync<T, TTable>(ITableCodec codec, CancellationToken cancellationToken = default)
            where T : class, IConfigRow where TTable : class, IConfigTable
        {
            try
            {
                if (codec == null)
                {
                    throw new ArgumentNullException(nameof(codec));
                }

                return Loader.GetTableAsync<TTable>(typeof(T), cancellationToken, codec);
            }
            catch (Exception error)
            {
                return ConfigOperation<TTable>.FromException(error);
            }
        }

        public IConfigRow GetRow(ITableCodec codec,
            object key,
            CancellationToken cancellationToken = default) => Loader.GetRow<IConfigRow, object>((codec ?? throw new ArgumentNullException(nameof(codec))).RowType,
            key ?? throw new ArgumentNullException(nameof(key)),
            cancellationToken, codec);
        public ConfigOperation<IConfigRow> GetRowAsync(ITableCodec codec, object key, CancellationToken cancellationToken = default)
        {
            try
            {
                return Loader.GetRowAsync<IConfigRow, object>((codec ?? throw new ArgumentNullException(nameof(codec))).RowType,
                    key ?? throw new ArgumentNullException(nameof(key)),
                    cancellationToken, codec);
            }
            catch (Exception error)
            {
                return ConfigOperation<IConfigRow>.FromException(error);
            }
        }

        public IConfigTable GetTable(ITableCodec codec,
            CancellationToken cancellationToken = default) => Loader.GetTable<IConfigTable>((codec ?? throw new ArgumentNullException(nameof(codec))).RowType,
            cancellationToken, codec);
        public ConfigOperation<IConfigTable> GetTableAsync(ITableCodec codec, CancellationToken cancellationToken = default)
        {
            try
            {
                return Loader.GetTableAsync<IConfigTable>((codec ?? throw new ArgumentNullException(nameof(codec))).RowType, cancellationToken, codec);
            }
            catch (Exception error)
            {
                return ConfigOperation<IConfigTable>.FromException(error);
            }
        }

        internal void Detach() => _loader?.Detach();
        internal void Invalidate() => _loader?.Invalidate();
        internal void PrepareRows(bool strict, CancellationToken token) => Loader.PrepareRows(strict, token);
        internal void SetServerBudget(Server.LoadBudget budget) => Loader.SetServerBudget(budget);
        internal Server.LoadBudget? ServerBudget => Loader.ServerBudget;
        internal T? GetCachedRow<T, TKey>(ITableCodec codec, TKey key) where T : class, IConfigRow => Loader.GetCachedRow<T, TKey>(codec, key);
        internal bool TryServerRow<T, TKey>(ITableCodec codec, TKey key, bool load, CancellationToken token, out T? row) where T : class, IConfigRow => Loader.TryServerRow(codec, key, load, token, out row);
        internal IConfigTable? GetCachedTable(ITableCodec codec) => Loader.GetCachedTable(codec);
        internal int RowCount(ITableCodec codec) => Loader.RowCount(codec);
        internal IConfigRow ReadOrdinal(ITableCodec codec, int ordinal, bool load, CancellationToken token) => Loader.ReadOrdinal(codec, ordinal, load, token);
        internal long IndexEstimate(ITableCodec codec) => Loader.IndexEstimate(codec);
        internal IConfigRow ReadTransient(ITableCodec codec, int ordinal, CancellationToken token) => Loader.ReadTransient(codec, ordinal, token);
        internal T? GetCachedReference<T>(ConfigRowReference reference) where T : class, IConfigKeyRow => Loader.TryGetReference<T>(reference, out T? row) ? row : null;
        internal T LoadReference<T>(ITableCodec codec, ConfigRowReference reference) where T : class, IConfigKeyRow => Loader.GetReference<T>(reference, codec);
        public void LoadAll(CancellationToken cancellationToken = default)
        {
            foreach (ITableCodec codec in Loader.ValidateTableSet(cancellationToken))
            {
                GetTable(codec, cancellationToken);
            }

            Loader.ValidateReferences(cancellationToken);
        }

        public async ConfigOperation<ConfigSnapshot> LoadAllAsync(CancellationToken cancellationToken = default)
        {
            foreach (ITableCodec codec in Loader.ValidateTableSet(cancellationToken))
            {
                await GetTableAsync(codec, cancellationToken);
            }

            Loader.ValidateReferences(cancellationToken);
            return this;
        }

        private ConfigLazyLoader Loader => _loader ?? throw new InvalidOperationException("Load a configuration manifest first.");

        ConfigTable<T> IConfigTableProvider.GetTable<T>(ITableCodec codec) => GetTable<T>(codec);
    }
}
