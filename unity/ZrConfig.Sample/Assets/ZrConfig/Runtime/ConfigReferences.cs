#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ZrConfig.Runtime
{
    /// <summary>生成行的主键契约，仅供加载期间关联表引用。</summary>
    public interface IConfigKeyRow : IConfigRow
    {
        int ConfigId
        {
            get;
        }
    }

    /// <summary>导出阶段的完整引用关联；运行时使用 BindReferences 延迟解析。</summary>
    public interface IConfigReferenceCodec
    {
        void ResolveReferences(IConfigRow row, ConfigReferenceResolver resolver);
    }

    /// <summary>为运行时行绑定延迟解析器；收集引用仅用于显式全量校验。</summary>
    public interface IConfigRowReferenceCodec : IConfigReferenceCodec
    {
        void BindReferences(IConfigRow row, ConfigReferenceResolver resolver);
        void CollectReferences(IConfigRow row, ICollection<ConfigRowReference> references);
    }

    public readonly struct ConfigRowReference
    {
        public Type RowType
        {
            get;
        }
        public int Id
        {
            get;
        }
        public IConfigRow Owner
        {
            get;
        }
        public string Field
        {
            get;
        }

        public ConfigRowReference(Type rowType, int id, IConfigRow owner, string field)
        {
            RowType = rowType;
            Id = id;
            Owner = owner;
            Field = field;
        }
    }

    public sealed class ConfigReferenceException : IOException
    {
        public IConfigRow Row
        {
            get;
        }
        public string Field
        {
            get;
        }

        public ConfigReferenceException(IConfigRow row, string field, string message) : base(message)
        {
            Row = row;
            Field = field;
        }
    }

    /// <summary>导出时关联已读取的行；运行时绑定单个快照，访问引用时才读取目标行。</summary>
    public sealed class ConfigReferenceResolver
    {
        private readonly Dictionary<Type, IList> _tables = new Dictionary<Type, IList>();
        private readonly Dictionary<Type, object> _indexes = new Dictionary<Type, object>();
        private readonly CancellationToken _cancellationToken;
        private readonly ConfigLazyLoader? _loader;
        internal ConfigReferenceResolver(ConfigLazyLoader loader) => _loader = loader;
        public ConfigReferenceResolver(IEnumerable<IConfigTable> tables, CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            foreach (IConfigTable table in tables)
            {
                _tables.Add(table.RowType, table.Rows);
            }
        }

        /// <summary>导表侧直接使用已解析的行列表，避免为关联复制整张表。</summary>
        public ConfigReferenceResolver(IEnumerable<KeyValuePair<Type, IList>> tables, CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            foreach (KeyValuePair<Type, IList> table in tables)
            {
                _tables.Add(table.Key, table.Value);
            }
        }

        public void ThrowIfInvalid() => _loader?.ThrowIfInvalid();
        public T? Resolve<T>(int id, IConfigRow owner, string field)
            where T : class, IConfigKeyRow
        {
            ThrowIfInvalid();
            if (_loader != null)
            {
                throw new InvalidOperationException("Runtime references require the target table codec.");
            }

            Dictionary<int, T> index = GetIndex<T>(owner, field);
            if (id == 0)
            {
                return null;
            }

            if (index.TryGetValue(id, out T? row))
            {
                return row;
            }

            throw new ConfigReferenceException(owner,
                field,
                ConfigText.Get("配置引用不存在：",
                "Configuration reference not found: ") + typeof(T).Name + ".Id=" + id);
        }

        /// <summary>由生成的引用属性直接指定目标表，首次访问时才加载目标行。</summary>
        public T? Resolve<T>(ITableCodec codec, int id, IConfigRow owner, string field)
            where T : class, IConfigKeyRow
        {
            if (codec == null)
            {
                throw new ArgumentNullException(nameof(codec));
            }

            ThrowIfInvalid();
            if (id == 0)
            {
                return null;
            }

            if (_loader == null)
            {
                return Resolve<T>(id, owner, field);
            }

            return _loader.GetReference<T>(new ConfigRowReference(typeof(T), id, owner, field), codec);
        }

        public ConfigOperation<T?> ResolveAsync<T>(ITableCodec codec,
            int id,
            IConfigRow owner,
            string field,
            CancellationToken cancellationToken = default)
            where T : class, IConfigKeyRow
        {
            try
            {
                if (codec == null)
                {
                    throw new ArgumentNullException(nameof(codec));
                }

                cancellationToken.ThrowIfCancellationRequested();
                ThrowIfInvalid();
                if (id == 0)
                {
                    return ConfigOperation<T?>.FromResult(null);
                }

                if (_loader == null)
                {
                    return ConfigOperation<T?>.FromResult(Resolve<T>(id, owner, field));
                }

                return _loader.GetReferenceAsync<T>(new ConfigRowReference(typeof(T), id, owner, field), cancellationToken, codec)!;
            }
            catch (Exception error)
            {
                return ConfigOperation<T?>.FromException(error);
            }
        }

        public bool TryResolveCached<T>(int id, IConfigRow owner, string field, out T? row)
            where T : class, IConfigKeyRow
        {
            ThrowIfInvalid();
            row = null;
            if (id == 0)
            {
                return false;
            }

            if (_loader == null)
            {
                row = Resolve<T>(id, owner, field);
                return row != null;
            }

            return _loader.TryGetReference(new ConfigRowReference(typeof(T), id, owner, field), out row);
        }

        private Dictionary<int, T> GetIndex<T>(IConfigRow owner, string field)
            where T : class, IConfigKeyRow
        {
            if (_indexes.TryGetValue(typeof(T), out object? cached))
            {
                if (cached is ConfigReferenceException failure)
                {
                    throw failure;
                }

                return (Dictionary<int, T>)cached;
            }

            if (!_tables.TryGetValue(typeof(T), out IList? rows))
            {
                throw new ConfigReferenceException(owner,
                    field,
                    ConfigText.Get("引用目标表尚未加载：",
                    "Reference target table not loaded: ") + typeof(T).Name);
            }

            var index = new Dictionary<int, T>(rows.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var row = (T)rows[i]!;
                if (row.ConfigId == 0 || !index.TryAdd(row.ConfigId, row))
                {
                    var failure = new ConfigReferenceException(row,
                        "Id",
                        ConfigText.Get("引用目标表的 Id 必须非零且唯一：",
                        "Reference target Id must be nonzero and unique: ") + typeof(T).Name + ".Id=" + row.ConfigId);
                    _indexes.Add(typeof(T), failure);
                    throw failure;
                }
            }

            _indexes.Add(typeof(T), index);
            return index;
        }
    }

    /// <summary>生成属性的成功结果缓存；不缓存未加载、读取失败或取消。</summary>
    public static class ConfigReferenceCache
    {
        public static bool TryGet<T>(ref T? cached, ConfigReferenceResolver? resolver, int id, IConfigRow owner, string field, out T? value)
            where T : class, IConfigKeyRow
        {
            resolver?.ThrowIfInvalid();
            value = Volatile.Read(ref cached);
            if (value != null)
            {
                return true;
            }

            if (id == 0 || resolver == null || !resolver.TryResolveCached(id, owner, field, out value))
            {
                return false;
            }

            value = Interlocked.CompareExchange(ref cached, value, null) ?? value;
            return true;
        }

        public static ConfigReferenceResolver RequireResolver(ConfigReferenceResolver? resolver,
            IConfigRow owner,
            string field) => resolver ?? throw new InvalidOperationException("Configuration reference is not bound to a snapshot: " + owner.GetType()
            .Name + "." + field);
    }
}
