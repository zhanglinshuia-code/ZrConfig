#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace ZrConfig.Runtime
{
    // 资源请求的合并、调度和句柄由 source 管理；这里只协调每张表的解码与对象缓存。
    internal sealed class ConfigLazyLoader
    {
        private IConfigSource? _source;
        private ConfigManager? _manager;
        private readonly ConfigReferenceResolver _resolver;
        private readonly ITableCodecRegistry? _registry;
        private readonly IConfigLoadScheduler? _scheduler;
        private readonly LoadMetrics? _metrics;
        private readonly string _version;
        private readonly bool _independent;
        private readonly Dictionary<string, ConfigManifestTable> _manifest = new Dictionary<string, ConfigManifestTable>(StringComparer.Ordinal);
        private readonly object _entriesGate = new object();
        private readonly Dictionary<Type, TableState> _entries = new Dictionary<Type, TableState>();
        private readonly object _tablesGate;
        private IReadOnlyList<IConfigTable> _tables = Array.Empty<IConfigTable>();
        private int _active = 1;
        private int _detached;
        private int _loadedRows;
        internal Server.LoadBudget? ServerBudget { get; private set; }
        internal void SetServerBudget(Server.LoadBudget budget) => ServerBudget = budget;
        internal ConfigLazyLoader(string version,
            IConfigSource source,
            IEnumerable<ConfigManifestTable> entries,
            ITableCodecRegistry? registry,
            ConfigManager? manager,
            ConfigLoadOptions? options)
        {
            _source = source;
            _manager = manager;
            _independent = manager == null;
            _tablesGate = manager?.PublicationGate ?? new object();
            _detached = manager == null ? 1 : 0;
            _registry = registry;
            _scheduler = options?.Scheduler;
            _metrics = options?.EnableDiagnostics == true ? new LoadMetrics() : null;
            _version = version;
            _resolver = new ConfigReferenceResolver(this);
            foreach (ConfigManifestTable entry in entries)
            {
                _manifest.Add(entry.name, entry);
            }
        }

        internal void ThrowIfInvalid()
        {
            if (Volatile.Read(ref _active) == 0 || (Volatile.Read(ref _manager)?.IsActive(this) != true && Volatile.Read(ref _detached) == 0))
            {
                throw new InvalidOperationException("Configuration has been reloaded. Obtain the current snapshot and rows again.");
            }
        }

        // 保留快照脱离管理器，旧请求不会因此间接持有后来发布的新版本。
        internal void Detach()
        {
            Volatile.Write(ref _detached, 1);
            Interlocked.Exchange(ref _manager, null);
        }

        internal void Invalidate()
        {
            if (Interlocked.Exchange(ref _active, 0) == 0)
            {
                return;
            }

            Interlocked.Exchange(ref _source, null);
            // 业务可能长期保留旧行；退休后不再经管理器持有后续版本。
            Interlocked.Exchange(ref _manager, null);
            TableState[] entries;
            lock (_entriesGate)
            {
                entries = new TableState[_entries.Count];
                _entries.Values.CopyTo(entries, 0);
                _entries.Clear();
                _entries.TrimExcess();
            }

            foreach (TableState table in entries)
            {
                table.ClearWhenIdle();
            }

            lock (_tablesGate)
            {
                _tables = Array.Empty<IConfigTable>();
                Volatile.Write(ref _loadedRows, 0);
                if (_metrics != null)
                {
                    Interlocked.Exchange(ref _metrics._bytes, 0);
                    Volatile.Write(ref _metrics._openedTables, 0);
                    Volatile.Write(ref _metrics._completedTables, 0);
                }
            }
        }

        internal ConfigLoadDiagnostics Diagnostics
        {
            get
            {
                ThrowIfInvalid();
                return _metrics == null ? default : new ConfigLoadDiagnostics(_metrics, Volatile.Read(ref _loadedRows));
            }
        }

        internal int LoadedRowCount
        {
            get
            {
                ThrowIfInvalid();
                return Volatile.Read(ref _loadedRows);
            }
        }

        internal IReadOnlyList<IConfigTable> Tables
        {
            get
            {
                lock (_tablesGate)
                {
                    ThrowIfInvalid();
                    return _tables;
                }
            }
        }

        private TableState Entry(Type type, CancellationToken token, ITableCodec? suppliedCodec = null)
        {
            token.ThrowIfCancellationRequested();
            ThrowIfInvalid();
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (suppliedCodec != null && suppliedCodec.RowType != type)
            {
                throw new InvalidDataException("Codec row type mismatch: " + type);
            }

            lock (_entriesGate)
            {
                ThrowIfInvalid();
                if (_entries.TryGetValue(type, out TableState? table))
                {
                    if (suppliedCodec != null && (suppliedCodec.TableName != table.Manifest.name || suppliedCodec.SchemaHash != table.Manifest.schemaHash))
                    {
                        throw new InvalidDataException("Codec metadata mismatch: " + type);
                    }

                    return table;
                }

                // 查询必须显式提供编解码器；无 Codec 的内部调用只允许访问已验证的表。
                ITableCodec codec = suppliedCodec ?? throw new InvalidOperationException("Pass the table codec to the query.");
                if (!_manifest.TryGetValue(codec.TableName, out ConfigManifestTable? entry))
                {
                    throw new InvalidDataException("Configuration table missing from manifest: " + codec.TableName);
                }

                if (entry.schemaHash != codec.SchemaHash)
                {
                    throw new InvalidDataException("SchemaHash mismatch: " + codec.TableName + ". Regenerate code and data together.");
                }

                token.ThrowIfCancellationRequested();
                ThrowIfInvalid();
                table = new TableState(this, codec, entry, string.IsNullOrEmpty(entry.relativePath) ? "versions/" + _version + "/" + entry.file : entry.relativePath);
                _entries.Add(type, table);
                return table;
            }
        }

        private ITableCodecRegistry RequireRegistry()
        {
            return _registry ?? throw new InvalidOperationException("Supply a codec catalog for full validation.");
        }

        // 只有显式全量验收才枚举注册表并核对完整表集合。
        internal IReadOnlyList<ITableCodec> ValidateTableSet(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            ThrowIfInvalid();
            var types = new HashSet<Type>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<ITableCodec>();
            ITableCodecRegistry registry = RequireRegistry();
            foreach (ITableCodec codec in registry.EnumerateCodecs())
            {
                token.ThrowIfCancellationRequested();
                ThrowIfInvalid();
                if (codec == null || codec.RowType == null || !types.Add(codec.RowType) || !names.Add(Entry(codec.RowType,
                    token,
                    codec).Manifest.name))
                {
                    throw new InvalidDataException("Duplicate or invalid table in codec catalog.");
                }

                result.Add(codec);
            }

            if (names.Count != _manifest.Count)
            {
                throw new InvalidDataException("Manifest table set does not match the codec registry.");
            }

            return result;
        }

        private TableState[] CopyEntries()
        {
            lock (_entriesGate)
            {
                var result = new TableState[_entries.Count];
                _entries.Values.CopyTo(result, 0);
                return result;
            }
        }

        private IConfigSource Source
        {
            get
            {
                ThrowIfInvalid();
                return Volatile.Read(ref _source) ?? throw new InvalidOperationException("Configuration has been reloaded.");
            }
        }

        internal T GetRow<T, TKey>(Type type, TKey key, CancellationToken token, ITableCodec? codec = null)
            where T : class, IConfigRow
        {
            return ReadRow<T, Key<TKey>>(Entry(type, token, codec), new Key<TKey>(key), token);
        }

        // New server queries never initiate decoding or I/O. Legacy GetRow keeps its existing semantics.
        internal T? GetCachedRow<T, TKey>(ITableCodec codec, TKey key) where T : class, IConfigRow =>
            Entry(typeof(T), default, codec).ReadCached<T, Key<TKey>>(new Key<TKey>(key));
        internal bool TryServerRow<T, TKey>(ITableCodec codec, TKey key, bool load, CancellationToken token, out T? row) where T : class, IConfigRow =>
            Entry(typeof(T), token, codec).TryServerRow(key, load, token, out row);
        internal IConfigTable? GetCachedTable(ITableCodec codec) => Entry(codec.RowType, default, codec).ReadCachedTable();
        internal int RowCount(ITableCodec codec) => Entry(codec.RowType, default, codec).Manifest.rows;
        internal IConfigRow ReadOrdinal(ITableCodec codec, int ordinal, bool load, CancellationToken token) =>
            Entry(codec.RowType, token, codec).ReadOrdinal(ordinal, load, token);
        internal long IndexEstimate(ITableCodec codec) => Entry(codec.RowType, default, codec).IndexEstimate;
        internal IConfigRow ReadTransient(ITableCodec codec, int ordinal, CancellationToken token) =>
            Entry(codec.RowType, token, codec).ReadForValidation(ordinal, token);
        internal void PrepareRows(bool strict, CancellationToken token)
        {
            IReadOnlyList<ITableCodec> codecs = ValidateTableSet(token);
            foreach (ITableCodec codec in codecs) ReadBytes(Entry(codec.RowType, token, codec), token);
            if (!strict) return;
            var references = new List<ConfigRowReference>();
            foreach (ITableCodec codec in codecs)
            {
                TableState table = Entry(codec.RowType, token, codec);
                for (int index = 0; index < table.Manifest.rows; index++)
                {
                    token.ThrowIfCancellationRequested();
                    IConfigRow row = table.ReadForValidation(index, token);
                    if (codec is IConfigRowReferenceCodec referenceCodec)
                    {
                        references.Clear(); referenceCodec.CollectReferences(row, references);
                        // Never hold one table's gate while resolving a different table's index.
                        foreach (ConfigRowReference reference in references) Entry(reference.RowType, token).ValidateReference(reference, token);
                    }
                }
            }
        }

        internal ConfigOperation<T> GetRowAsync<T, TKey>(Type type, TKey key, CancellationToken token, ITableCodec? codec = null)
            where T : class, IConfigRow
        {
            return ReadRowAsync<T, Key<TKey>>(type, new Key<TKey>(key), token, codec);
        }

        internal T GetReference<T>(ConfigRowReference reference, ITableCodec? codec = null)
            where T : class, IConfigKeyRow
        {
            return ReadRow<T, ReferenceKey>(Entry(typeof(T), default, codec), new ReferenceKey(reference), default);
        }

        internal ConfigOperation<T> GetReferenceAsync<T>(ConfigRowReference reference, CancellationToken token, ITableCodec? codec = null)
            where T : class, IConfigKeyRow
        {
            return ReadRowAsync<T, ReferenceKey>(typeof(T), new ReferenceKey(reference), token, codec);
        }

        internal bool TryGetReference<T>(ConfigRowReference reference, out T? row)
            where T : class, IConfigKeyRow
        {
            ThrowIfInvalid();
            TableState? table;
            lock (_entriesGate)
            {
                ThrowIfInvalid();
                if (!_entries.TryGetValue(typeof(T), out table))
                {
                    row = null;
                    return false;
                }
            }

            return table.TryGetCachedReference(reference, out row);
        }

        private T ReadRow<T, TKey>(TableState table, TKey key, CancellationToken token)
            where T : class, IConfigRow where TKey : struct, IRowKey
        {
            if (table.TryReadRow(key, token, out T? row))
            {
                return row!;
            }

            ReadBytes(table, token);
            table.TryReadRow(key, token, out row);
            return row!;
        }

        // 无资源等待时不进入 async builder，包括已缓存字节中尚未解码的行。
        private ConfigOperation<T> ReadRowAsync<T, TKey>(Type type, TKey key, CancellationToken token, ITableCodec? codec)
            where T : class, IConfigRow where TKey : struct, IRowKey
        {
            try
            {
                TableState table = Entry(type, token, codec);
                if (_independent && table.HasReader) return table.ReadServerAsync<T, TKey>(key, token);
                T? row;
                bool cached = _scheduler == null ? table.TryReadRow(key, token, out row) : table.TryCachedRow(key, token, out row);
                return cached ? ConfigOperation<T>.FromResult(row!) : ReadRowSlowAsync<T, TKey>(table, key, token);
            }
            catch (Exception error)
            {
                return ConfigOperation<T>.FromException(error);
            }
        }

        private async ConfigOperation<T> ReadRowSlowAsync<T, TKey>(TableState table, TKey key, CancellationToken token)
            where T : class, IConfigRow where TKey : struct, IRowKey
        {
            byte[]? bytes = !table.HasReader ? await ReadSourceAsync(table.Path, token) : null;
            if (_scheduler != null)
            {
                T row = await _scheduler.Execute(() => ReadOpenedRow<T, TKey>(table, key, bytes, token), token);
                token.ThrowIfCancellationRequested();
                ThrowIfInvalid();
                return row;
            }

            return ReadOpenedRow<T, TKey>(table, key, bytes, token);
        }

        private T ReadOpenedRow<T, TKey>(TableState table, TKey key, byte[]? bytes, CancellationToken token)
            where T : class, IConfigRow where TKey : struct, IRowKey
        {
            if (bytes != null)
            {
                table.Open(bytes, token);
            }

            table.TryReadRow(key, token, out T? row);
            return row!;
        }

        internal TTable GetTable<TTable>(Type type, CancellationToken token, ITableCodec? codec = null)
            where TTable : class, IConfigTable
        {
            TableState table = Entry(type, token, codec);
            if (table.TryReadTable(token, out TTable? result))
            {
                return result!;
            }

            ReadBytes(table, token);
            table.TryReadTable(token, out result);
            return result!;
        }

        internal ConfigOperation<TTable> GetTableAsync<TTable>(Type type, CancellationToken token, ITableCodec? codec = null)
            where TTable : class, IConfigTable
        {
            try
            {
                TableState table = Entry(type, token, codec);
                TTable? result;
                bool cached = _scheduler == null ? table.TryReadTable(token, out result) : table.TryCachedTable(token, out result);
                return cached ? ConfigOperation<TTable>.FromResult(result!) : ReadTableSlowAsync<TTable>(table, token);
            }
            catch (Exception error)
            {
                return ConfigOperation<TTable>.FromException(error);
            }
        }

        private async ConfigOperation<TTable> ReadTableSlowAsync<TTable>(TableState table, CancellationToken token)
            where TTable : class, IConfigTable
        {
            byte[]? bytes = !table.HasReader ? await ReadSourceAsync(table.Path, token) : null;
            if (_scheduler != null)
            {
                TTable result = await _scheduler.Execute(() => ReadOpenedTable<TTable>(table, bytes, token), token);
                token.ThrowIfCancellationRequested();
                ThrowIfInvalid();
                return result;
            }

            return ReadOpenedTable<TTable>(table, bytes, token);
        }

        private TTable ReadOpenedTable<TTable>(TableState table, byte[]? bytes, CancellationToken token)
            where TTable : class, IConfigTable
        {
            if (bytes != null)
            {
                table.Open(bytes, token);
            }

            table.TryReadTable(token, out TTable? result);
            return result!;
        }

        private ConfigOperation<byte[]> ReadSourceAsync(string path,
            CancellationToken token) => _metrics == null ? Source.ReadAsync(path,
            token) : TimedReadAsync(path,
            token);
        private async ConfigOperation<byte[]> TimedReadAsync(string path, CancellationToken token)
        {
            long started = LoadMetrics.Start(_metrics);
            try
            {
                return await Source.ReadAsync(path, token);
            }
            finally
            {
                LoadMetrics.Add(ref _metrics!._readTicks, started);
            }
        }

        private void ReadBytes(TableState table, CancellationToken token)
        {
            if (!(Source is ISyncConfigSource sync))
            {
                throw new InvalidOperationException("This source only supports asynchronous loading. Use the asynchronous configuration API.");
            }

            byte[] bytes;
            long started = LoadMetrics.Start(_metrics);
            try
            {
                bytes = sync.Read(table.Path, token);
            }
            finally
            {
                if (_metrics != null)
                {
                    LoadMetrics.Add(ref _metrics._readTicks, started);
                }
            }

            table.Open(bytes, token);
        }

        // 全量验收才遍历引用；正常加载不展开引用链。
        internal void ValidateReferences(CancellationToken token)
        {
            ThrowIfInvalid();
            var references = new List<ConfigRowReference>();
            foreach (TableState table in CopyEntries())
            {
                table.CollectReferences(references, token);
            }

            foreach (ConfigRowReference reference in references)
            {
                Entry(reference.RowType, token).ValidateReference(reference, token);
            }
        }

        // 单表锁由此类封装；提交只按「单表锁 → 发布锁」进入。
        // 发布锁内不读取资源、不解码，也不等待任何单表锁。
        private sealed class TableState
        {
            private readonly ConfigLazyLoader _owner;
            private readonly ITableCodec _codec;
            private readonly ConfigManifestTable _manifest;
            private readonly string _path;
            private IConfigRowReader? _reader;
            private long _rawByteCount;
            internal long IndexEstimate => 256L + 512L * _manifest.rows + 8L * _rawByteCount;
            private Dictionary<int, IConfigRow> _rows = new Dictionary<int, IConfigRow>();
            private IConfigTable? _table;
            // Independent server snapshots never revoke published rows. Slots are shared by
            // single-row, reference and whole-table loading; only cold slots allocate work.
            private object?[]? _slots;
            private readonly object _fullGate = new object();
            internal ConfigManifestTable Manifest => _manifest;
            internal string Path => _path;
            internal bool HasReader => Volatile.Read(ref _reader) != null;

            internal TableState(ConfigLazyLoader owner, ITableCodec codec, ConfigManifestTable manifest, string path)
            {
                _owner = owner;
                _codec = codec;
                _manifest = manifest;
                _path = path;
            }

            internal void ClearWhenIdle()
            {
                // 旧快照已失效；后台作业持锁时延迟清理，发布新 Current 不等待旧解码。
                if (Monitor.TryEnter(this))
                {
                    try
                    {
                        Clear();
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        lock (this)
                        {
                            Clear();
                        }
                    });
                }
            }

            private void Clear()
            {
                Volatile.Write(ref _reader, null);
                _rows.Clear();
                _rows.TrimExcess();
                _table = null;
            }

            internal bool TryGetCachedReference<T>(ConfigRowReference reference, out T? row)
                where T : class, IConfigKeyRow
            {
                if (_owner._independent)
                {
                    row = ReadCached<T, ReferenceKey>(new ReferenceKey(reference));
                    return row != null;
                }
                lock (this)
                {
                    _owner.ThrowIfInvalid();
                    row = _reader != null && _rows.TryGetValue(new ReferenceKey(reference).Find(_reader),
                        out IConfigRow? cached) ? (T)cached : null;
                    return row != null;
                }
            }
            internal T? ReadCached<T, TKey>(TKey key) where T : class, IConfigRow where TKey : struct, IRowKey
            {
                if (_owner._independent)
                {
                    IConfigRowReader reader = Volatile.Read(ref _reader) ?? throw new Server.ConfigException(Server.ErrorCode.TableNotLoaded, "Table index is not loaded.", table: _manifest.name);
                    object? slot = Volatile.Read(ref _slots![key.Find(reader)]);
                    if (slot is RowFailure failure) failure.Error.Throw();
                    return slot as T;
                }
                lock (this)
                {
                    _owner.ThrowIfInvalid();
                    if (_reader == null) throw new Server.ConfigException(Server.ErrorCode.TableNotLoaded, "Table index is not loaded.", table: _manifest.name);
                    int index = key.Find(_reader);
                    return _rows.TryGetValue(index, out IConfigRow? value) ? (T)value : null;
                }
            }
            internal IConfigTable? ReadCachedTable() { _owner.ThrowIfInvalid(); return Volatile.Read(ref _table); }
            internal bool TryServerRow<T, TKey>(TKey key, bool load, CancellationToken token, out T? row) where T : class, IConfigRow
            {
                token.ThrowIfCancellationRequested();
                int ordinal;
                if (_reader is IConfigRowLookup<TKey> lookup)
                {
                    if (!lookup.TryFind(key, out ordinal)) { row = null; return false; }
                }
                else
                {
                    try { ordinal = new Key<TKey>(key).Find(_reader!); }
                    catch (KeyNotFoundException) { row = null; return false; }
                }
                row = (T)ReadOrdinal(ordinal, load, token); return true;
            }
            internal IConfigRow ReadOrdinal(int ordinal, bool load, CancellationToken token)
            {
                if ((uint)ordinal >= (uint)_manifest.rows) throw new ArgumentOutOfRangeException(nameof(ordinal));
                if (load) return ReadServer(ordinal, token);
                object? slot = Volatile.Read(ref _slots![ordinal]);
                if (slot is RowFailure failure) failure.Error.Throw();
                return slot as IConfigRow ?? throw new Server.ConfigException(Server.ErrorCode.RowNotLoaded, "Load the row explicitly.", table: _manifest.name);
            }
            internal IConfigRow ReadForValidation(int index, CancellationToken token)
            {
                if (_owner._independent)
                {
                    token.ThrowIfCancellationRequested();
                    if (_slots != null && Volatile.Read(ref _slots[index]) is IConfigRow cached) return cached;
                    using var scope = ServerBudgetScope();
                    var budget = _owner.ServerBudget;
                    long estimate = 256L + (_reader is IConfigRowSize sized ? sized.EncodedBytes(index) * 4L : 1024L);
                    try
                    {
                        budget?.EnterDecode();
                        try
                        {
                            budget?.ReserveTransient(estimate);
                            try { return _reader!.Read(index); }
                            finally { budget?.ReleaseTransient(estimate); }
                        }
                        finally { budget?.ExitDecode(); }
                    }
                    catch (Exception error) when (Server.ConfigException.IsDataFailure(error))
                    { throw Server.ConfigException.Normalize(error, "validate", version: _owner._version, table: _manifest.name, path: _path, rowKey: "ordinal:" + index.ToString(System.Globalization.CultureInfo.InvariantCulture)); }
                }
                lock (this)
                {
                    token.ThrowIfCancellationRequested(); _owner.ThrowIfInvalid();
                    // Transient validation values are never installed in the published row cache.
                    using var scope = ServerBudgetScope();
                    return _reader!.Read(index);
                }
            }
            private Server.DecodeGuard.Scope ServerBudgetScope() => _owner.ServerBudget == null ? default : Server.DecodeGuard.Enter(_owner.ServerBudget.Limits);

            internal void CollectReferences(List<ConfigRowReference> references, CancellationToken token)
            {
                if (_owner._independent)
                {
                    if (_codec is IConfigRowReferenceCodec serverCodec && _slots != null)
                        for (int i = 0; i < _slots.Length; i++)
                        {
                            token.ThrowIfCancellationRequested();
                            if (Volatile.Read(ref _slots[i]) is IConfigRow row) serverCodec.CollectReferences(row, references);
                        }
                    return;
                }
                lock (this)
                {
                    token.ThrowIfCancellationRequested();
                    _owner.ThrowIfInvalid();
                    if (_codec is IConfigRowReferenceCodec codec)
                    {
                        foreach (IConfigRow row in _rows.Values)
                        {
                            codec.CollectReferences(row, references);
                        }
                    }
                }
            }

            internal void ValidateReference(ConfigRowReference reference, CancellationToken token)
            {
                lock (this)
                {
                    token.ThrowIfCancellationRequested();
                    _owner.ThrowIfInvalid();
                    new ReferenceKey(reference).Find(_reader!);
                }
            }

            // 后台作业可能持锁；异步入口尝试缓存，避免调用线程等待解码。
            internal bool TryCachedRow<T, TKey>(TKey key, CancellationToken token, out T? row)
                where T : class, IConfigRow where TKey : struct, IRowKey
            {
                row = null;
                if (!Monitor.TryEnter(this))
                {
                    return false;
                }

                try
                {
                    token.ThrowIfCancellationRequested();
                    _owner.ThrowIfInvalid();
                    if (_reader == null || !_rows.TryGetValue(key.Find(_reader), out IConfigRow? cached))
                    {
                        return false;
                    }

                    row = (T)cached;
                    return true;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            internal bool TryReadRow<T, TKey>(TKey key, CancellationToken token, out T? row)
                where T : class, IConfigRow where TKey : struct, IRowKey
            {
                if (_owner._independent)
                {
                    token.ThrowIfCancellationRequested();
                    row = null;
                    IConfigRowReader? reader = Volatile.Read(ref _reader);
                    if (reader == null) return false;
                    row = (T)ReadServer(key.Find(reader), token);
                    return true;
                }
                lock (this)
                {
                    token.ThrowIfCancellationRequested();
                    _owner.ThrowIfInvalid();
                    row = null;
                    if (_reader == null)
                    {
                        return false;
                    }

                    int index = key.Find(_reader);
                    if (!_rows.TryGetValue(index, out IConfigRow? value))
                    {
                        value = Decode(index);
                        CommitRow(index, value, token);
                    }

                    row = (T)value;
                    return true;
                }
            }

            internal bool TryCachedTable<TTable>(CancellationToken token, out TTable? result)
                where TTable : class, IConfigTable
            {
                result = null;
                if (!Monitor.TryEnter(this))
                {
                    return false;
                }

                try
                {
                    token.ThrowIfCancellationRequested();
                    _owner.ThrowIfInvalid();
                    result = (TTable?)_table;
                    return result != null;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            internal bool TryReadTable<TTable>(CancellationToken token, out TTable? result)
                where TTable : class, IConfigTable
            {
                if (_owner._independent) return ReadServerTable(token, out result);
                lock (this)
                {
                    token.ThrowIfCancellationRequested();
                    _owner.ThrowIfInvalid();
                    result = null;
                    if (_table != null)
                    {
                        result = (TTable)_table;
                        return true;
                    }

                    if (_reader == null)
                    {
                        return false;
                    }

                    IList rows = _codec.CreateRows();
                    for (int i = 0; i < _reader.Count; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        rows.Add(_rows.TryGetValue(i, out IConfigRow? cached) ? cached : Decode(i));
                    }

                    long started = LoadMetrics.Start(_owner._metrics);
                    try
                    {
                        result = (TTable)_codec.CreateTable(rows);
                    }
                    finally
                    {
                        if (_owner._metrics != null)
                        {
                            LoadMetrics.Add(ref _owner._metrics._tableTicks, started);
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    _owner.ThrowIfInvalid();
                    int count = rows.Count;
                    var completedRows = new Dictionary<int, IConfigRow>(count);
                    for (int i = 0; i < count; i++)
                    {
                        completedRows.Add(i, (IConfigRow)rows[i]!);
                    }

                    CommitTable(result, completedRows, token);

                    return true;
                }
            }

            private sealed class RowWork
            {
                internal readonly TaskCompletionSource<IConfigRow> Completion = new TaskCompletionSource<IConfigRow>(TaskCreationOptions.RunContinuationsAsynchronously);
                private Server.LoadBudget? _budget;
                internal RowWork(Server.LoadBudget? budget) { budget?.WorkRequest(); _budget = budget; }
                internal void Release() => Interlocked.Exchange(ref _budget, null)?.CompleteWork();
            }
            private sealed class RowFailure
            {
                internal readonly ExceptionDispatchInfo Error;
                internal RowFailure(Exception error) => Error = ExceptionDispatchInfo.Capture(error);
            }
            private object StartServer(int index, out RowWork? owned)
            {
                owned = null;
                object? slot = Volatile.Read(ref _slots![index]);
                if (slot != null) return slot;
                var work = new RowWork(_owner.ServerBudget);
                slot = Interlocked.CompareExchange(ref _slots[index], work, null);
                if (slot != null) { work.Release(); return slot; }
                owned = work;
                return work;
            }
            private IConfigRow CompleteServer(int index, RowWork work)
            {
                Server.LoadBudget? budget = _owner.ServerBudget;
                long estimate = 256L + (_reader is IConfigRowSize sized ? sized.EncodedBytes(index) * 4L : 1024L);
                bool reserved = false;
                try
                {
                    budget?.Reserve(estimate); reserved = true;
                    // No manager/table/cache lock is held while user codecs run. A cancelled
                    // waiter does not cancel a shared decode or discard another waiter's row.
                    IConfigRow value;
                    budget?.EnterDecode();
                    try { using var scope = ServerBudgetScope(); value = Decode(index); }
                    finally { budget?.ExitDecode(); }
                    Volatile.Write(ref _slots![index], value);
                    Interlocked.Increment(ref _owner._loadedRows);
                    work.Completion.TrySetResult(value);
                    return value;
                }
                catch (Exception error)
                {
                    if (reserved) budget?.Release(estimate);
                    Exception normalized = Server.ConfigException.Normalize(error, "decode", version: _owner._version, table: _manifest.name,
                        path: _path, rowKey: "ordinal:" + index.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    object? terminal = Server.ConfigException.IsDataFailure(error) ? new RowFailure(normalized) : null;
                    if (terminal != null) budget?.Fault();
                    Volatile.Write(ref _slots![index], terminal);
                    work.Completion.TrySetException(normalized);
                    // Observe the task even when the decoding caller uses the synchronous path.
                    _ = work.Completion.Task.Exception;
                    ExceptionDispatchInfo.Capture(normalized).Throw(); throw new InvalidOperationException();
                }
                finally { work.Release(); }
            }
            private IConfigRow ReadServer(int index, CancellationToken token)
            {
                token.ThrowIfCancellationRequested();
                if (Volatile.Read(ref _slots![index]) is IConfigRow hot) return hot;
                _owner.ServerBudget?.Request();
                try
                {
                    object slot = StartServer(index, out RowWork? owned);
                    if (owned != null)
                    {
                        IConfigRow value = CompleteServer(index, owned);
                        token.ThrowIfCancellationRequested(); return value;
                    }
                    if (slot is IConfigRow row) return row;
                    if (slot is RowFailure failure) { failure.Error.Throw(); throw new InvalidOperationException(); }
                    _owner.ServerBudget?.Merge();
                    var task = ((RowWork)slot).Completion.Task;
                    try { task.Wait(token); } catch (AggregateException) { return task.GetAwaiter().GetResult(); }
                    token.ThrowIfCancellationRequested(); return task.GetAwaiter().GetResult();
                }
                finally { _owner.ServerBudget?.CompleteRequest(); }
            }
            internal ConfigOperation<T> ReadServerAsync<T, TKey>(TKey key, CancellationToken token)
                where T : class, IConfigRow where TKey : struct, IRowKey
            {
                token.ThrowIfCancellationRequested();
                int index = key.Find(_reader!);
                if (Volatile.Read(ref _slots![index]) is T hot) return ConfigOperation<T>.FromResult(hot);
                _owner.ServerBudget?.Request();
                bool handedOff = false;
                try
                {
                    object slot = StartServer(index, out RowWork? owned);
                    if (slot is T row) return ConfigOperation<T>.FromResult(row);
                    if (slot is RowFailure failure) return ConfigOperation<T>.FromException(failure.Error.SourceException);
                    if (owned != null)
                    {
                        if (_owner._scheduler == null) CompleteServer(index, owned);
                        else _ = ScheduleServer(index, owned);
                    }
                    else _owner.ServerBudget?.Merge();
                    handedOff = true;
                    return WaitServer<T>(((RowWork)slot).Completion.Task, token);
                }
                finally { if (!handedOff) _owner.ServerBudget?.CompleteRequest(); }
            }
            private async Task ScheduleServer(int index, RowWork work)
            {
                try { await _owner._scheduler!.Execute(() => CompleteServer(index, work), CancellationToken.None); }
                catch (Exception error)
                {
                    // A scheduler may reject before executing the work; make that slot retryable.
                    Interlocked.CompareExchange(ref _slots![index], null, work);
                    work.Completion.TrySetException(error);
                    _ = work.Completion.Task.Exception;
                    work.Release();
                }
            }
            private async ConfigOperation<T> WaitServer<T>(Task<IConfigRow> work, CancellationToken token) where T : class, IConfigRow
            {
                try
                {
                    if (!work.IsCompleted && token.CanBeCanceled)
                    {
                        var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                        using (token.Register(state => ((TaskCompletionSource<bool>)state!).TrySetResult(true), cancelled))
                        {
                            await Task.WhenAny(work, cancelled.Task);
                            token.ThrowIfCancellationRequested();
                        }
                    }
                    T row = (T)await work;
                    token.ThrowIfCancellationRequested(); return row;
                }
                finally { _owner.ServerBudget?.CompleteRequest(); }
            }
            private bool ReadServerTable<TTable>(CancellationToken token, out TTable? result) where TTable : class, IConfigTable
            {
                token.ThrowIfCancellationRequested();
                result = (TTable?)Volatile.Read(ref _table);
                if (result != null) return true;
                IConfigRowReader? reader = Volatile.Read(ref _reader);
                if (reader == null) return false;
                // Decode first, outside the table publication gate; row slots merge concurrent callers.
                IList rows = _codec.CreateRows();
                for (int i = 0; i < reader.Count; i++) rows.Add(ReadServer(i, token));
                lock (_fullGate)
                {
                    token.ThrowIfCancellationRequested();
                    result = (TTable?)_table;
                    if (result != null) return true;
                    result = (TTable)_codec.CreateTable(rows);
                    token.ThrowIfCancellationRequested();
                    lock (_owner._tablesGate)
                    {
                        var visible = new List<IConfigTable>(_owner._tables) { result };
                        _owner._tables = visible.AsReadOnly();
                        Volatile.Write(ref _table, result);
                        if (_owner._metrics != null) Interlocked.Increment(ref _owner._metrics._completedTables);
                    }
                    return true;
                }
            }

            private IConfigRow Decode(int index)
            {
                long started = LoadMetrics.Start(_owner._metrics);
                try
                {
                    IConfigRow row = _reader!.Read(index);
                    if (_codec is IConfigRowReferenceCodec references)
                    {
                        if (_owner._independent)
                        {
                            var targets = new List<ConfigRowReference>();
                            references.CollectReferences(row, targets);
                            foreach (ConfigRowReference target in targets) _owner.Entry(target.RowType, default).ValidateReference(target, default);
                        }
                        references.BindReferences(row, _owner._resolver);
                    }
                    else if (_codec is IConfigReferenceCodec)
                    {
                        throw new InvalidOperationException("Regenerate reference codecs for row loading.");
                    }

                    return row;
                }
                finally
                {
                    if (_owner._metrics != null)
                    {
                        LoadMetrics.Add(ref _owner._metrics._decodeTicks, started);
                    }
                }
            }

            internal void Open(byte[] bytes, CancellationToken token)
            {
                lock (this)
                {
                    token.ThrowIfCancellationRequested();
                    _owner.ThrowIfInvalid();
                    if (_reader != null)
                    {
                        return;
                    }
                    using var limits = ServerBudgetScope();

                    long started = LoadMetrics.Start(_owner._metrics);
                    try
                    {
                        using var hash = SHA256.Create();
                        if (!ConfigManager.MatchesHash(hash, bytes, _manifest.sha256))
                        {
                            if (_owner._independent) throw new Server.ConfigException(Server.ErrorCode.IntegrityMismatch, "File hash mismatch.", "hash", versionId: _owner._version, table: _manifest.name, relativePath: _path);
                            throw new InvalidDataException("File hash mismatch: " + _manifest.name);
                        }
                    }
                    finally
                    {
                        if (_owner._metrics != null)
                        {
                            LoadMetrics.Add(ref _owner._metrics._hashTicks, started);
                        }
                    }

                    if (!(_codec is IIndexedTableCodec codec))
                    {
                        throw new InvalidOperationException("Regenerate configuration codecs to enable row loading: " + _manifest.name);
                    }

                    IConfigRowReader reader;
                    started = LoadMetrics.Start(_owner._metrics);
                    try
                    {
                        if (_manifest.file.EndsWith(".json", StringComparison.Ordinal))
                        {
                            if (!(_codec is IConfigJsonCodec jsonCodec))
                                throw new InvalidDataException("请重新生成支持 JSON 的配置代码：" + _manifest.name);
                            bytes = jsonCodec.JsonToBinary(bytes);
                        }
                        reader = codec.Open(bytes);
                    }
                    finally
                    {
                        if (_owner._metrics != null)
                        {
                            LoadMetrics.Add(ref _owner._metrics._indexTicks, started);
                        }
                    }

                    if (reader.Count != _manifest.rows)
                    {
                        throw new InvalidDataException("Invalid row count: " + _manifest.name);
                    }

                    CommitReader(reader, bytes.LongLength, token);
                    _owner.ServerBudget?.OwnBytes(bytes.LongLength);
                }
            }

            // 下列三个提交点由持有单表锁的操作调用；最后一次有效性检查与发布不可分开。
            private void CommitReader(IConfigRowReader reader, long bytes, CancellationToken token)
            {
                lock (_owner._tablesGate)
                {
                    token.ThrowIfCancellationRequested();
                    _owner.ThrowIfInvalid();
                    if (_owner._independent) _slots = new object?[reader.Count];
                    _rawByteCount = bytes;
                    Volatile.Write(ref _reader, reader);
                    if (_owner._metrics != null)
                    {
                        Interlocked.Add(ref _owner._metrics._bytes, bytes);
                        Interlocked.Increment(ref _owner._metrics._openedTables);
                    }
                }
            }

            private void CommitRow(int index, IConfigRow row, CancellationToken token)
            {
                lock (_owner._tablesGate)
                {
                    token.ThrowIfCancellationRequested();
                    _owner.ThrowIfInvalid();
                    _rows.Add(index, row);
                    Interlocked.Increment(ref _owner._loadedRows);
                }
            }

            private void CommitTable(IConfigTable table, Dictionary<int, IConfigRow> rows, CancellationToken token)
            {
                lock (_owner._tablesGate)
                {
                    token.ThrowIfCancellationRequested();
                    _owner.ThrowIfInvalid();
                    // 先准备可见表清单，全部构建成功后再一起更新缓存与统计。
                    var visible = new List<IConfigTable>(_owner._tables) { table };
                    IReadOnlyList<IConfigTable> tables = visible.AsReadOnly();
                    int added = rows.Count - _rows.Count;
                    _rows = rows;
                    _table = table;
                    _owner._tables = tables;
                    Interlocked.Add(ref _owner._loadedRows, added);
                    if (_owner._metrics != null)
                    {
                        Interlocked.Increment(ref _owner._metrics._completedTables);
                    }
                }
            }
        }

        // 结构体查询参数通过约束调用传递，强类型键和引用 Id 都不经过 object 装箱。
        private interface IRowKey
        {
            int Find(IConfigRowReader reader);
        }

        private readonly struct Key<TKey> : IRowKey
        {
            private readonly TKey _value;
            internal Key(TKey value) => _value = value;
            public int Find(IConfigRowReader reader) => reader is IConfigRowReader<TKey> typed ? typed.Find(_value) : reader.Find(_value!);
        }

        private readonly struct ReferenceKey : IRowKey
        {
            private readonly ConfigRowReference _reference;
            internal ReferenceKey(ConfigRowReference reference) => _reference = reference;
            public int Find(IConfigRowReader reader)
            {
                try
                {
                    return reader.FindReference(_reference.Id);
                }
                catch (KeyNotFoundException error)
                {
                    throw new ConfigReferenceException(_reference.Owner,
                        _reference.Field,
                        _reference.Owner.GetType().Name + "." + _reference.Field + " -> " + error.Message);
                }
            }
        }
    }
}
