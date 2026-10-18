#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ZrConfig.Runtime.Server
{
    public enum ErrorCode
    {
        NotInitialized, ManagerClosed, ReloadBusy, StaleCandidate, AlreadyCommitted, ForeignCandidate,
        Cancelled, DeadlineExceeded, InvalidPlan, UnsupportedCapability, SyncNotSupported,
        SourceUnavailable, ReadFailed, InvalidManifest, TableSetMismatch, IntegrityMismatch, SchemaMismatch,
        InvalidEncoding, InvalidValue, DuplicateKey, MissingReference, KeyNotFound, TableNotSelected,
        TableNotLoaded, RowNotLoaded, LimitExceeded, BudgetExceeded, VersionIdentityConflict, CatalogMismatch, UnsupportedFormat, InternalError
    }

    public sealed class ConfigException : Exception
    {
        public ErrorCode Code { get; }
        public string Stage { get; }
        public long OperationId { get; }
        public string VersionId { get; }
        public string Table { get; }
        public string RelativePath { get; }
        public string RowKey { get; }
        public string FieldPath { get; }
        public ConfigException(ErrorCode code, string message, string stage = "", long operationId = 0,
            string versionId = "", string table = "", string relativePath = "", Exception? cause = null, string rowKey = "", string fieldPath = "") : base(message, cause)
        {
            Code = code; Stage = stage; OperationId = operationId; VersionId = versionId; Table = table; RelativePath = relativePath; RowKey = rowKey; FieldPath = fieldPath;
        }
        internal static Exception Normalize(Exception error, string stage, long operation = 0, string version = "", string table = "", string path = "", string rowKey = "", string fieldPath = "")
        {
            if (error is ConfigException existing)
                return existing.Stage.Length != 0 && (table.Length == 0 || existing.Table.Length != 0) && (version.Length == 0 || existing.VersionId.Length != 0) && (path.Length == 0 || existing.RelativePath.Length != 0) && (rowKey.Length == 0 || existing.RowKey.Length != 0) && (fieldPath.Length == 0 || existing.FieldPath.Length != 0) && (operation == 0 || existing.OperationId != 0) ? existing :
                    new ConfigException(existing.Code, existing.Message, existing.Stage.Length == 0 ? stage : existing.Stage,
                        existing.OperationId == 0 ? operation : existing.OperationId, existing.VersionId.Length == 0 ? version : existing.VersionId,
                        existing.Table.Length == 0 ? table : existing.Table, existing.RelativePath.Length == 0 ? path : existing.RelativePath,
                        existing, existing.RowKey.Length == 0 ? rowKey : existing.RowKey, existing.FieldPath.Length == 0 ? fieldPath : existing.FieldPath);
            ErrorCode? code = error is OperationCanceledException ? ErrorCode.Cancelled :
                error is ConfigReferenceException ? ErrorCode.MissingReference :
                error is DecoderFallbackException ? ErrorCode.InvalidEncoding :
                error is InvalidDataException || error is FormatException || error is EndOfStreamException ? ErrorCode.InvalidValue :
                error is IOException || error is UnauthorizedAccessException ? ErrorCode.ReadFailed : (ErrorCode?)null;
            return code.HasValue ? new ConfigException(code.Value, error.Message, stage, operation, version, table, path, error, rowKey, fieldPath) : error;
        }
        internal static bool IsDataFailure(Exception error)
        {
            if (error is InvalidDataException || error is EndOfStreamException || error is FormatException || error is DecoderFallbackException || error is ConfigReferenceException) return true;
            if (!(error is ConfigException failure)) return false;
            switch (failure.Code)
            {
                case ErrorCode.InvalidValue: case ErrorCode.InvalidEncoding: case ErrorCode.IntegrityMismatch:
                case ErrorCode.SchemaMismatch: case ErrorCode.DuplicateKey: case ErrorCode.MissingReference:
                case ErrorCode.LimitExceeded: case ErrorCode.UnsupportedFormat: return true;
                default: return false;
            }
        }
    }

    public enum ManagerPhase { Empty, Ready, Closed }
    public enum LoadMode { ServerDefault, SelectedTables, LazyRowsStrict, LazyRowsOnAccess }
    public enum SnapshotHealth { Healthy, Degraded }
    public sealed class PreparationOptions
    {
        public TimeSpan Timeout { get; }
        public TimeSpan CandidateLifetime { get; }
        public long MaxManifestBytes { get; }
        public long MaxTableBytes { get; }
        public long MaxBundleBytes { get; }
        public bool PreferJson { get; }
        public IReadOnlyList<string>? SelectedTables { get; }
        public string SourceNamespace { get; }
        public LoadMode Mode { get; }
        public IConfigLoadScheduler? Scheduler { get; }
        public IReadOnlyList<string> WarmupTables { get; }
        public ResourceLimits Limits { get; }
        public bool EnableDiagnostics { get; }
        public IReadOnlyList<ICandidateValidator> Validators { get; }
        public WarmupPlan? Warmup { get; }
        public PreparationOptions(TimeSpan? timeout = null, TimeSpan? candidateLifetime = null,
            long maxManifestBytes = 16 * 1024 * 1024, long maxTableBytes = int.MaxValue,
            long maxBundleBytes = long.MaxValue, bool preferJson = false, IEnumerable<string>? selectedTables = null, string sourceNamespace = "default", LoadMode mode = LoadMode.ServerDefault, IConfigLoadScheduler? scheduler = null, IEnumerable<string>? warmupTables = null, ResourceLimits? limits = null, bool enableDiagnostics = false, IEnumerable<ICandidateValidator>? validators = null, WarmupPlan? warmup = null)
        {
            Timeout = timeout ?? TimeSpan.FromMinutes(5); CandidateLifetime = candidateLifetime ?? TimeSpan.FromMinutes(5);
            if (Timeout <= TimeSpan.Zero || CandidateLifetime <= TimeSpan.Zero || maxManifestBytes <= 0 || maxTableBytes <= 0 || maxBundleBytes <= 0)
                throw new ConfigException(ErrorCode.InvalidPlan, "Deadlines and resource limits must be positive.");
            MaxManifestBytes = maxManifestBytes; MaxTableBytes = maxTableBytes; MaxBundleBytes = maxBundleBytes; PreferJson = preferJson;
            SelectedTables = selectedTables == null ? null : new List<string>(selectedTables).AsReadOnly();
            SourceNamespace = string.IsNullOrEmpty(sourceNamespace) ? throw new ConfigException(ErrorCode.InvalidPlan, "Source namespace is required.") : sourceNamespace;
            if (!Enum.IsDefined(typeof(LoadMode), mode)) throw new ConfigException(ErrorCode.InvalidPlan, "Unknown load mode.");
            Mode = mode;
            Scheduler = scheduler;
            WarmupTables = new List<string>(warmupTables ?? Array.Empty<string>()).AsReadOnly();
            Limits = limits ?? new ResourceLimits();
            EnableDiagnostics = enableDiagnostics;
            var validation = new List<ICandidateValidator>(validators ?? Array.Empty<ICandidateValidator>());
            if (validation.Exists(value => value == null)) throw new ConfigException(ErrorCode.InvalidPlan, "Candidate validator must not be null.");
            Validators = validation.AsReadOnly();
            Warmup = warmup;
        }
    }

    public readonly struct ManagerStatus
    {
        public ManagerPhase Phase { get; }
        public long Generation { get; }
        public long PreparingOperation { get; }
        internal ManagerStatus(ManagerPhase phase, long generation, long operation) { Phase = phase; Generation = generation; PreparingOperation = operation; }
    }

    /// <summary>Independent immutable snapshot. Closing its manager does not revoke rows or references.</summary>
    public sealed partial class Snapshot
    {
        private readonly ConfigSnapshot _storage;
        private readonly HashSet<string> _selected;
        private readonly bool _completeCatalog;
        private readonly long _estimatedIndexBytes;
        private readonly Dictionary<IConfigRow, object> _handles = new Dictionary<IConfigRow, object>(RowIdentity.Instance);
        private readonly HashSet<string> _readyTables = new HashSet<string>(StringComparer.Ordinal);
        public LoadMode Mode { get; }
        public string VersionId => _storage.VersionId;
        public string ManifestDigest { get; }
        public string SourceNamespace { get; }
        public string CatalogFingerprint { get; }
        public string LoadPlanFingerprint { get; }
        public IReadOnlyList<string> SelectedTables { get; }
        public ConfigLoadDiagnostics Diagnostics => _storage.Diagnostics;
        public int LoadedRowCount => _storage.LoadedRowCount;
        public int CompletedTableCount { get { lock (_readyTables) return _readyTables.Count; } }
        public int FaultedRowCount => _storage.ServerBudget?.Faults ?? 0;
        public int FaultedIndexCount => _storage.ServerBudget?.IndexFaults ?? 0;
        public SnapshotHealth Health => FaultedRowCount == 0 && FaultedIndexCount == 0 ? SnapshotHealth.Healthy : SnapshotHealth.Degraded;
        /// <summary>Admission estimate: fixed bytes + 64 bytes per indexed row + reserved decoded estimates.
        /// Excludes runtime allocator overhead, business copies and process memory.</summary>
        public long EstimatedRetainedBytes
        {
            get
            {
                return RetainedRawBytes + EstimatedIndexBytes + (_storage.ServerBudget?.EstimatedDecodedBytes ?? 0);
            }
        }
        public long MergedRowRequests => _storage.ServerBudget?.MergedRequests ?? 0;
        public long EstimatedIndexBytes => _estimatedIndexBytes + (_storage.ServerBudget?.EstimatedIndexBytes ?? 0);
        public long RetainedRawBytes => _storage.ServerBudget?.RawBytes ?? Diagnostics.RetainedBytes;
        public long DeclaredRowCount => _estimatedIndexBytes / 64;
        internal Snapshot(ConfigSnapshot storage, string manifestDigest, string sourceNamespace, IEnumerable<string> selected, bool completeCatalog, LoadMode mode, long estimatedIndexBytes, string catalogFingerprint, bool preferJson)
        {
            _storage = storage; CatalogFingerprint = catalogFingerprint; ManifestDigest = manifestDigest; SourceNamespace = sourceNamespace; _completeCatalog = completeCatalog;
            var names = new List<string>(selected); names.Sort(StringComparer.Ordinal); SelectedTables = names.AsReadOnly();
            _selected = new HashSet<string>(names, StringComparer.Ordinal);
            Mode = mode;
            var encodedNames = new List<string>(); foreach(string name in names) encodedNames.Add(BitConverter.ToString(Encoding.UTF8.GetBytes(name)).Replace("-", "").ToLowerInvariant()); encodedNames.Sort(StringComparer.Ordinal);
            using(var hash=SHA256.Create()) LoadPlanFingerprint=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes("zrconfig-load-plan/1\n"+mode+"\n"+(preferJson?"json-preferred":"binary")+"\n"+string.Join("\n",encodedNames)))).Replace("-", "").ToLowerInvariant();
            _estimatedIndexBytes = estimatedIndexBytes;
        }
        /// <summary>Compatibility view with the legacy query semantics; it shares all row identities and caches.</summary>
        public ConfigSnapshot AsLegacySnapshot() => _completeCatalog && Mode != LoadMode.LazyRowsStrict && Mode != LoadMode.LazyRowsOnAccess ? _storage : throw new ConfigException(ErrorCode.UnsupportedCapability, "A partial or lazy snapshot cannot expose the complete legacy catalog.");
        public IConfigTable GetTable(ITableCodec codec)
        {
            CheckSelected(codec);
            lock (_readyTables) if (!_readyTables.Contains(codec.TableName)) throw new ConfigException(ErrorCode.TableNotLoaded, "Load the table and its required indexes explicitly.", table: codec.TableName);
            return _storage.GetCachedTable(codec)!;
        }
        public RowHandle<T> GetRow<T, TKey>(ITableCodec codec, TKey key) where T : class, IConfigRow
        {
            CheckSelected(codec);
            return _storage.TryServerRow(codec, key, false, default, out T? row) ? Handle(row!) : throw new ConfigException(ErrorCode.KeyNotFound, "Row key not found.", table: codec.TableName);
        }
        public RowHandle<T>? TryGetRow<T, TKey>(ITableCodec codec, TKey key) where T : class, IConfigRow
        { CheckSelected(codec); return _storage.TryServerRow(codec, key, false, default, out T? row) ? Handle(row!) : null; }
        public RowHandle<T>? TryLoadRow<T, TKey>(ITableCodec codec, TKey key, CancellationToken cancellationToken = default) where T : class, IConfigRow
        { CheckSelected(codec);try{return _storage.TryServerRow(codec, key, true, cancellationToken, out T? row) ? Handle(row!) : null;}catch(Exception error){throw ConfigException.Normalize(error,"row",version:VersionId,table:codec.TableName,rowKey:Convert.ToString(key,System.Globalization.CultureInfo.InvariantCulture)??"");} }
        public RowHandle<T> LoadRow<T, TKey>(ITableCodec codec, TKey key, CancellationToken cancellationToken = default) where T : class, IConfigRow
        { return TryLoadRow<T,TKey>(codec,key,cancellationToken) ?? throw new ConfigException(ErrorCode.KeyNotFound,"Row key not found.","row",versionId:VersionId,table:codec.TableName,rowKey:Convert.ToString(key,System.Globalization.CultureInfo.InvariantCulture)??""); }
        public async ConfigOperation<RowHandle<T>> LoadRowAsync<T, TKey>(ITableCodec codec, TKey key, CancellationToken cancellationToken = default) where T : class, IConfigRow
        { CheckSelected(codec); try { return Handle(await _storage.GetRowAsync<T, TKey>(codec, key, cancellationToken)); } catch (KeyNotFoundException error) { throw new ConfigException(ErrorCode.KeyNotFound, "Row key not found.", table: codec.TableName, cause: error); }catch(Exception error){throw ConfigException.Normalize(error,"row",version:VersionId,table:codec.TableName,rowKey:Convert.ToString(key,System.Globalization.CultureInfo.InvariantCulture)??"");} }
        public IConfigTable LoadTable(ITableCodec codec, CancellationToken cancellationToken = default)
        {
            CheckSelected(codec);
            try
            {
                IConfigTable table = _storage.GetTable(codec, cancellationToken);
                PrepareGroups(codec.TableName, cancellationToken);
                MarkReady(codec.TableName);
                return table;
            }
            catch(Exception error){throw ConfigException.Normalize(error,"table",version:VersionId,table:codec.TableName);}
        }
        public void LoadAll(CancellationToken cancellationToken = default)
        {
            try
            {
                _storage.LoadAll(cancellationToken);
                foreach (string name in SelectedTables) { PrepareGroups(name, cancellationToken); MarkReady(name); }
            }
            catch(Exception error){throw ConfigException.Normalize(error,"snapshot",version:VersionId);}
        }
        private Catalog? _catalog;
        internal void BindCatalog(Catalog? catalog) => _catalog = catalog;
        internal void MarkReady(string name) { lock (_readyTables) _readyTables.Add(name); }
        internal void PrepareGroups(string table, CancellationToken token)
        {
            if (_catalog == null) return;
            foreach (GroupDescriptor descriptor in _catalog.GetTable(table).Groups) descriptor.Prepare(this, token);
        }
        public ConfigTable<T> GetTable<T>(ITableCodec codec) where T : class, IConfigRow
        { return (ConfigTable<T>)GetTable(codec); }
        internal RowHandle<T> Handle<T>(T row) where T : class, IConfigRow
        {
            lock (_handles)
            {
                if (_handles.TryGetValue(row, out object? handle)) return (RowHandle<T>)handle;
                var result = new RowHandle<T>(this, row); _handles.Add(row, result); return result;
            }
        }
        internal RowHandle<T>? Reference<T>(ITableCodec codec, ConfigRowReference reference, bool load) where T : class, IConfigKeyRow
        {
            if (reference.Id == 0) return null;
            CheckSelected(codec);
            try
            {
                T? row = load ? _storage.LoadReference<T>(codec, reference) : _storage.GetCachedReference<T>(reference);
                return Handle(row ?? throw new ConfigException(ErrorCode.RowNotLoaded, "Load the reference explicitly.", table: codec.TableName));
            }
            catch (ConfigReferenceException error) { throw new ConfigException(ErrorCode.MissingReference, error.Message, "reference", versionId: VersionId, table: codec.TableName, cause: error); }
        }
        private sealed class RowIdentity : IEqualityComparer<IConfigRow>
        {
            internal static readonly RowIdentity Instance = new RowIdentity();
            public bool Equals(IConfigRow? left, IConfigRow? right) => ReferenceEquals(left, right);
            public int GetHashCode(IConfigRow value) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
        }
        private void CheckSelected(ITableCodec codec)
        {
            if (codec == null) throw new ArgumentNullException(nameof(codec));
            if (!_selected.Contains(codec.TableName)) throw new ConfigException(ErrorCode.TableNotSelected, "Table is outside the selected dependency closure.", "query", versionId: VersionId, table: codec.TableName);
        }
    }

    /// <summary>Retains the complete selected snapshot. Borrow explicitly opts into legacy field/reference semantics.</summary>
    public sealed class RowHandle<T> where T : class, IConfigRow
    {
        private readonly T _value;
        public Snapshot Snapshot { get; }
        internal RowHandle(Snapshot snapshot, T value) { Snapshot = snapshot; _value = value; }
        public T Borrow() => _value;
        public RowHandle<TReference>? GetReference<TReference>(ITableCodec codec, int id, string field) where TReference : class, IConfigKeyRow =>
            Snapshot.Reference<TReference>(codec, new ConfigRowReference(typeof(TReference), id, _value, field), false);
        public RowHandle<TReference>? LoadReference<TReference>(ITableCodec codec, int id, string field) where TReference : class, IConfigKeyRow =>
            Snapshot.Reference<TReference>(codec, new ConfigRowReference(typeof(TReference), id, _value, field), true);
    }

    public readonly struct CommitResult
    {
        public bool Committed { get; }
        public Snapshot Snapshot { get; }
        public long Generation { get; }
        public long OperationId { get; }
        internal CommitResult(Snapshot snapshot, long generation, long operation) { Snapshot = snapshot; Generation = generation; OperationId = operation; Committed = true; }
    }

    public sealed class PreparedSnapshot : IDisposable
    {
        internal readonly WeakReference<Manager> Owner;
        internal readonly long Generation, Operation, Expires, MaxEstimatedLiveBytes;
        internal Snapshot? Value;
        internal bool Committed;
        internal PreparedSnapshot(Manager owner, Snapshot snapshot, long generation, long operation, long expires, long maxEstimatedLiveBytes)
        { Owner = new WeakReference<Manager>(owner); Value = snapshot; Generation = generation; Operation = operation; Expires = expires; MaxEstimatedLiveBytes = maxEstimatedLiveBytes; }
        public Snapshot Snapshot => Volatile.Read(ref Value) ?? throw new ConfigException(ErrorCode.StaleCandidate, "Candidate has been discarded.");
        public void Dispose()
        {
            if (Owner.TryGetTarget(out Manager? owner)) owner.Discard(this);
            else Interlocked.Exchange(ref Value, null);
        }
    }

    /// <summary>Server lifecycle; preparation performs I/O outside the publication gate. One candidate at a time.</summary>
    public sealed class Manager : IDisposable
    {
        private static readonly UTF8Encoding s_utf8 = new UTF8Encoding(false, true);
        private readonly ITableCodecRegistry _catalog;
        private readonly Func<string, ConfigManifest> _decodeManifest;
        private readonly object _gate = new object();
        private Snapshot? _current;
        private int _closed;
        private long _generation, _serial, _pending, _expires;
        private bool _building;
        private StageTiming _prepareTiming = new StageTiming("Prepare", 0, 0, false), _commitTiming = new StageTiming("Commit", 0, 0, false);
        public IReadOnlyList<StageTiming> StageTimings { get { lock (_gate) return Array.AsReadOnly(new[] { _prepareTiming, _commitTiming }); } }
        private void RecordPreparation(long operation, long started, bool success) { lock (_gate) if (operation >= _prepareTiming.OperationId) _prepareTiming = new StageTiming("Prepare", operation, (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency, success); }
        private readonly Dictionary<long,CancellationTokenSource> _activePreparations = new Dictionary<long,CancellationTokenSource>();
        private readonly ManagerPolicy _policy = new ManagerPolicy();
        private readonly Dictionary<(string Source, string Version), string> _identities = new Dictionary<(string, string), string>();
        private readonly Queue<(string Source, string Version)> _identityOrder = new Queue<(string, string)>();
        private readonly List<WeakReference<Snapshot>> _observed = new List<WeakReference<Snapshot>>();
        private readonly EventQueue _events;
        public Manager(ITableCodecRegistry catalog, Func<string, ConfigManifest> decodeManifest, int eventCapacity = 64)
        { _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog)); _decodeManifest = decodeManifest ?? throw new ArgumentNullException(nameof(decodeManifest)); _events = new EventQueue(eventCapacity); }
        public Manager(ITableCodecRegistry catalog,Func<string,ConfigManifest> decodeManifest,ManagerPolicy policy)
            :this(catalog,decodeManifest,(policy ?? throw new ArgumentNullException(nameof(policy))).EventCapacity){_policy=policy;}
        public int ActivePreparationCount { get { lock(_gate)return _activePreparations.Count; } }
        public IDisposable Subscribe(Action<ManagerEvent> listener) => _events.Subscribe(listener);
        public long DroppedEventCount => _events.Dropped;
        public long ListenerFailureCount => _events.ListenerFailures;
        public Snapshot? TryAcquireSnapshot()
        {
            Snapshot? value = Volatile.Read(ref _current);
            if (Volatile.Read(ref _closed) != 0) throw Error(ErrorCode.ManagerClosed);
            return value;
        }
        public Snapshot AcquireSnapshot() => TryAcquireSnapshot() ?? throw Error(ErrorCode.NotInitialized);
        public ManagerStatus GetStatus()
        {
            lock (_gate) return new ManagerStatus(_closed != 0 ? ManagerPhase.Closed : _current == null ? ManagerPhase.Empty : ManagerPhase.Ready, _generation, _pending);
        }
        public PreparedSnapshot Prepare(ISyncConfigSource source, PreparationOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            options ??= new PreparationOptions();
            ValidatePlan(options);
            (long generation, long operation, long deadline, CancellationTokenSource ticket) = Begin(options, cancellationToken);
            long started = Stopwatch.GetTimestamp();
            using var lifetime = new PreparationCancellation(cancellationToken, options.Timeout,ticket.Token);
            cancellationToken = lifetime.Token;
            try
            {
                long manifestLimit = ReadAllowance(options, Math.Min(options.MaxManifestBytes, options.MaxBundleBytes), 0, 0);
                byte[] bytes = Own(ReadManifest(source, manifestLimit, cancellationToken), manifestLimit, "Common/current.json");
                ConfigManifest manifest = Manifest(bytes,options.Limits);
                List<ConfigManifestTable> entries = Entries(manifest, options);
                var files = new Dictionary<string, byte[]>(StringComparer.Ordinal);
                long total = bytes.Length;
                foreach (ConfigManifestTable entry in entries)
                {
                    Check(cancellationToken, deadline);
                    string path = entry.relativePath;
                    long limit = ReadAllowance(options, Math.Min(options.MaxTableBytes, options.MaxBundleBytes - total), total, IndexEstimate(entries));
                    byte[] data = Own(Read(source, path, limit, cancellationToken, manifest.versionId), limit, path);
                    total = checked(total + data.Length); files.Add(path, data);
                }
                var prepared = Finish(manifest, entries, files, bytes, options, generation, operation, deadline, cancellationToken);
                RecordPreparation(operation, started, true); return prepared;
            }
            catch (Exception error) { RecordPreparation(operation, started, false); throw Failed(operation, generation, lifetime.Classify(error, deadline)); }
            finally { lock(_gate)_activePreparations.Remove(operation);ticket.Dispose(); }
        }
        public async ConfigOperation<PreparedSnapshot> PrepareAsync(IConfigSource source, PreparationOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            options ??= new PreparationOptions();
            ValidatePlan(options);
            (long generation, long operation, long deadline, CancellationTokenSource ticket) = Begin(options, cancellationToken);
            long started = Stopwatch.GetTimestamp();
            using var lifetime = new PreparationCancellation(cancellationToken, options.Timeout,ticket.Token);
            cancellationToken = lifetime.Token;
            try
            {
                long manifestLimit = ReadAllowance(options, Math.Min(options.MaxManifestBytes, options.MaxBundleBytes), 0, 0);
                byte[] bytes = Own(await ReadManifestAsync(source, manifestLimit, cancellationToken), manifestLimit, "Common/current.json");
                ConfigManifest manifest = Manifest(bytes,options.Limits);
                List<ConfigManifestTable> entries = Entries(manifest, options);
                var files = new Dictionary<string, byte[]>(StringComparer.Ordinal);
                long total = bytes.Length;
                foreach (ConfigManifestTable entry in entries)
                {
                    Check(cancellationToken, deadline);
                    string path = entry.relativePath;
                    long limit = ReadAllowance(options, Math.Min(options.MaxTableBytes, options.MaxBundleBytes - total), total, IndexEstimate(entries));
                    byte[] data = Own(await ReadAsync(source, path, limit, cancellationToken, manifest.versionId), limit, path);
                    total = checked(total + data.Length); files.Add(path, data);
                }
                var prepared = Finish(manifest, entries, files, bytes, options, generation, operation, deadline, cancellationToken);
                RecordPreparation(operation, started, true); return prepared;
            }
            catch (Exception error) { RecordPreparation(operation, started, false); throw Failed(operation, generation, lifetime.Classify(error, deadline)); }
            finally { lock(_gate)_activePreparations.Remove(operation);ticket.Dispose(); }
        }
        public CommitResult Reload(ISyncConfigSource source, PreparationOptions? options = null, CancellationToken cancellationToken = default)
        { using PreparedSnapshot prepared = Prepare(source, options, cancellationToken); return Commit(prepared, cancellationToken); }
        public async ConfigOperation<CommitResult> ReloadAsync(IConfigSource source, PreparationOptions? options = null, CancellationToken cancellationToken = default)
        { using PreparedSnapshot prepared = await PrepareAsync(source, options, cancellationToken); return Commit(prepared, cancellationToken); }
        public CommitResult Commit(PreparedSnapshot prepared, CancellationToken cancellationToken = default)
        {
            long started = Stopwatch.GetTimestamp();
            if (prepared == null) throw new ArgumentNullException(nameof(prepared));
            if (!prepared.Owner.TryGetTarget(out Manager? owner) || !ReferenceEquals(owner, this)) throw Error(ErrorCode.ForeignCandidate);
            CommitResult result;
            lock (_gate)
            {
                if (_closed != 0) throw Error(ErrorCode.ManagerClosed);
                if (prepared.Committed) throw Error(ErrorCode.AlreadyCommitted);
                Snapshot value = prepared.Value ?? throw Error(ErrorCode.StaleCandidate);
                if (prepared.Generation != _generation || prepared.Operation != _pending) throw Error(ErrorCode.StaleCandidate);
                Check(cancellationToken, prepared.Expires);
                // Published or retired lazy snapshots may have decoded more rows since
                // preparation. Recheck admission at the publication point as well.
                if (LiveEstimate() > prepared.MaxEstimatedLiveBytes)
                    throw new ConfigException(ErrorCode.BudgetExceeded, "Estimated live snapshot admission limit exceeded.", "commit", prepared.Operation);
                var identity = (value.SourceNamespace, value.VersionId);
                if (_identities.TryGetValue(identity, out string? known) && known != value.ManifestDigest) throw Error(ErrorCode.VersionIdentityConflict);
                long next = checked(_generation + 1);
                if (!_identities.ContainsKey(identity))
                {
                    if (_identities.Count == 32) _identities.Remove(_identityOrder.Dequeue());
                    _identities.Add(identity, value.ManifestDigest); _identityOrder.Enqueue(identity);
                }
                Volatile.Write(ref _current, value); _generation = next; _pending = 0; prepared.Committed = true;
                result = new CommitResult(value, next, prepared.Operation);
                _commitTiming = new StageTiming("Commit", prepared.Operation, (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency, true);
                _events.Enqueue(new ManagerEvent(ManagerEventKind.Committed, next, prepared.Operation, value.VersionId));
            }
            _events.Drain();
            return result;
        }
        public void Clear() => Detach(false);
        public void Close() => Detach(true);
        public void Dispose() => Close();
        internal void Discard(PreparedSnapshot prepared)
        {
            lock (_gate)
            {
                if (!prepared.Committed && _pending == prepared.Operation) _pending = 0;
                Volatile.Write(ref prepared.Value, null);
            }
        }
        private void Detach(bool close)
        {
            CancellationTokenSource[] cancelled;
            lock (_gate)
            {
                if (_closed != 0) { if (close) return; throw Error(ErrorCode.ManagerClosed); }
                _generation = checked(_generation + 1);
                if (close) Volatile.Write(ref _closed, 1);
                Volatile.Write(ref _current, null); _pending = 0;
                cancelled=new CancellationTokenSource[_activePreparations.Count];_activePreparations.Values.CopyTo(cancelled,0);
                _events.Enqueue(new ManagerEvent(close ? ManagerEventKind.Closed : ManagerEventKind.Cleared, _generation, 0, ""));
            }
            foreach(var ticket in cancelled){try{ticket.Cancel();}catch(ObjectDisposedException){}catch(AggregateException){/* Cancellation callbacks cannot undo detachment. */}}
            _events.Drain();
        }
        private (long Generation, long Operation, long Deadline,CancellationTokenSource Ticket) Begin(PreparationOptions options, CancellationToken token)
        {
            Check(token, long.MaxValue);
            if (options.SelectedTables != null)
            {
                if (!(_catalog is Catalog catalog)) throw Error(ErrorCode.UnsupportedCapability);
                catalog.Select(options.SelectedTables);
            }
            long deadline = Deadline(options.Timeout);
            lock (_gate)
            {
                if (_closed != 0) throw Error(ErrorCode.ManagerClosed);
                if (_pending != 0 && (_building || Stopwatch.GetTimestamp() < _expires)) throw Error(ErrorCode.ReloadBusy);
                if(_activePreparations.Count>=_policy.MaxPendingPreparations)throw Error(ErrorCode.BudgetExceeded);
                var ticket=new CancellationTokenSource();
                _serial = checked(_serial + 1);_activePreparations.Add(_serial,ticket); _pending = _serial; _building = true; _expires = deadline;
                return (_generation, _serial, deadline,ticket);
            }
        }
        private PreparedSnapshot Finish(ConfigManifest manifest, List<ConfigManifestTable> entries, Dictionary<string, byte[]> files,
            byte[] manifestBytes, PreparationOptions options, long generation, long operation, long deadline, CancellationToken token)
        {
            var selectedNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (ConfigManifestTable entry in entries) selectedNames.Add(entry.name);
            var frozen = new FrozenSource(files);
            var storage = new ConfigSnapshot(manifest.versionId, frozen, entries, new ScopedRegistry(_catalog, selectedNames), null,
                new ConfigLoadOptions(enableDiagnostics: options.EnableDiagnostics, scheduler: options.Scheduler, retainPreviousSnapshots: true, preferJson: options.PreferJson));
            storage.SetServerBudget(new LoadBudget(options.Limits));
            storage.PrepareRows(options.Mode == LoadMode.LazyRowsStrict, token);
            if (options.Mode == LoadMode.ServerDefault || options.Mode == LoadMode.SelectedTables) storage.LoadAll(token);
            // Every selected index now owns its fixed bytes; release the source dictionary,
            // particularly JSON input that has already been converted to indexed binary.
            frozen.Release();
            Check(token, deadline);
            string digest;
            using (var hash = SHA256.Create()) digest = BitConverter.ToString(hash.ComputeHash(manifestBytes)).Replace("-", "");
            var snapshot = new Snapshot(storage, digest, options.SourceNamespace, selectedNames, entries.Count == manifest.tables.Length, options.Mode, IndexEstimate(entries), CatalogIdentity(manifest), options.PreferJson);
            snapshot.BindCatalog(_catalog as Catalog);
            if (options.Mode == LoadMode.ServerDefault || options.Mode == LoadMode.SelectedTables)
                foreach (string name in snapshot.SelectedTables) { snapshot.PrepareGroups(name, token); snapshot.MarkReady(name); }
            foreach (string name in options.WarmupTables)
                snapshot.LoadTable(((Catalog)_catalog).GetTable(name).GetCodec(), token);
            if (options.Warmup != null) foreach (WarmupRequest request in options.Warmup.Requests)
            {
                Check(token, deadline); request.Apply(snapshot, token);
            }
            foreach (ICandidateValidator validator in options.Validators)
            {
                Check(token, deadline);
                validator.Validate(snapshot, token);
            }
            Check(token, deadline);
            if (options.Limits.MaxEstimatedLiveBytes != long.MaxValue && snapshot.EstimatedRetainedBytes > options.Limits.MaxEstimatedLiveBytes - LiveEstimate())
                throw new ConfigException(ErrorCode.BudgetExceeded, "Estimated live snapshot admission limit exceeded.", "prepare");
            long expires = Deadline(options.CandidateLifetime);
            lock (_gate)
            {
                if (_closed != 0) throw Error(ErrorCode.ManagerClosed);
                if (_generation != generation || _pending != operation) throw Error(ErrorCode.StaleCandidate);
                _building = false; _expires = expires;
            }
            lock (_observed) _observed.Add(new WeakReference<Snapshot>(snapshot));
            return new PreparedSnapshot(this, snapshot, generation, operation, expires, options.Limits.MaxEstimatedLiveBytes);
        }
        private ConfigManifest Manifest(byte[] bytes,ResourceLimits limits)
        {
            try { string text=s_utf8.GetString(bytes);ConfigJson.ValidateManifest(text,limits);return _decodeManifest(text); }
            catch (DecoderFallbackException error) { throw new ConfigException(ErrorCode.InvalidEncoding, "Invalid manifest UTF-8.", "manifest", cause: error); }
            catch (Exception error) when (error is InvalidDataException || error is FormatException || IsJsonParseError(error))
            { throw new ConfigException(ErrorCode.InvalidManifest, "Invalid manifest.", "manifest", cause: error); }
        }
        private static bool IsJsonParseError(Exception error)
        {
            for (Type? type = error.GetType(); type != null; type = type.BaseType)
                if (type.FullName == "System.Text.Json.JsonException" || type.FullName == "Newtonsoft.Json.JsonReaderException") return true;
            return false;
        }
        private List<ConfigManifestTable> Entries(ConfigManifest manifest, PreparationOptions options)
        {
            List<ConfigManifestTable> entries;
            if (manifest == null || string.IsNullOrEmpty(manifest.format))
                throw new ConfigException(ErrorCode.InvalidManifest, "Manifest and format are required.", "manifest");
            if (manifest.format != "ZrConfig.Binary/1" && manifest.format != "ZrConfig.Json/1" && manifest.format != "ZrConfig.Mixed/1")
                throw new ConfigException(ErrorCode.UnsupportedFormat, "Unsupported manifest format.", "manifest");
            if(_catalog is Catalog targetCatalog&&targetCatalog.Target.Length!=0&&manifest.target!=targetCatalog.Target)
                throw new ConfigException(ErrorCode.CatalogMismatch,"Manifest target differs from catalog.","manifest",versionId:manifest.versionId);
            if (manifest.tables != null && manifest.tables.Length > options.Limits.MaxTables)
                throw new ConfigException(ErrorCode.LimitExceeded, "Table count limit exceeded.", "manifest");
            try { entries = ConfigManager.ValidateAndCopy(manifest, options.PreferJson); }
            catch (InvalidDataException error) { throw new ConfigException(ErrorCode.InvalidManifest, "Invalid manifest.", "manifest", cause: error); }
            if (entries.Count > options.Limits.MaxTables) throw new ConfigException(ErrorCode.LimitExceeded, "Table count limit exceeded.", "manifest");
            var schemas = new Dictionary<string, string>(StringComparer.Ordinal);
            if (_catalog is Catalog catalog)
            {
                foreach (TableDescriptor table in catalog.Tables) schemas.Add(table.Name, table.SchemaHash);
            }
            else foreach (ITableCodec codec in _catalog.EnumerateCodecs())
            {
                if (codec == null || schemas.ContainsKey(codec.TableName)) throw Error(ErrorCode.CatalogMismatch);
                schemas.Add(codec.TableName, codec.SchemaHash);
            }
            if (schemas.Count != entries.Count) throw Error(ErrorCode.TableSetMismatch);
            foreach (ConfigManifestTable entry in entries)
            {
                if (!schemas.TryGetValue(entry.name, out string? hash)) throw Error(ErrorCode.TableSetMismatch);
                if (hash != entry.schemaHash) throw new ConfigException(ErrorCode.SchemaMismatch, "Schema mismatch.", "manifest", versionId: manifest.versionId, table: entry.name);
            }
            if (options.SelectedTables != null)
            {
                if (!(_catalog is Catalog selectionCatalog)) throw Error(ErrorCode.UnsupportedCapability);
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (TableDescriptor table in selectionCatalog.Select(options.SelectedTables).Tables) names.Add(table.Name);
                entries.RemoveAll(entry => !names.Contains(entry.name));
            }
            long selectedRows = 0;
            foreach (ConfigManifestTable entry in entries)
            {
                selectedRows += entry.rows;
                if (selectedRows > options.Limits.MaxRows)
                    throw new ConfigException(ErrorCode.LimitExceeded, "Selected snapshot row limit exceeded.", "manifest");
            }
            return entries;
        }
        private void ValidatePlan(PreparationOptions options)
        {
            if (!(_catalog is Catalog catalog))
            {
                if (options.SelectedTables != null || options.WarmupTables.Count != 0 || options.Warmup != null) throw Error(ErrorCode.UnsupportedCapability);
                return;
            }
            SelectionPlan selection = catalog.Select(options.SelectedTables);
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (TableDescriptor descriptor in selection.Tables) names.Add(descriptor.Name);
            foreach (string name in options.WarmupTables)
                if (name == null || !names.Contains(name)) throw new ConfigException(ErrorCode.InvalidPlan, "Warmup table is outside the selected closure.", table: name ?? "");
            if (options.Warmup != null) foreach (WarmupRequest request in options.Warmup.Requests)
            {
                if (!names.Contains(request.TableName)) throw new ConfigException(ErrorCode.InvalidPlan, "Warmup row/group is outside the selected closure.", table: request.TableName);
                TableDescriptor descriptor = catalog.GetTable(request.TableName);
                if (descriptor.SchemaHash != request.Codec.SchemaHash || descriptor.RowType != request.Codec.RowType) throw Error(ErrorCode.CatalogMismatch);
            }
        }
        private Exception Failed(long operation, long generation, Exception error)
        {
            Exception result = Map(error, operation);
            lock (_gate)
            {
                if(result is ConfigException cancelled && cancelled.Code==ErrorCode.Cancelled && generation!=_generation)result=new ConfigException(_closed!=0?ErrorCode.ManagerClosed:ErrorCode.StaleCandidate,"Preparation belongs to an earlier manager generation.","prepare",operation,cause:result);
                if (_pending == operation) { _pending = 0; _building = false; }
                _activePreparations.Remove(operation);
                _events.Enqueue(new ManagerEvent(ManagerEventKind.PreparationFailed, generation, operation, "", (result as ConfigException)?.Code ?? ErrorCode.InternalError));
            }
            _events.Drain(); return result;
        }
        private string CatalogIdentity(ConfigManifest manifest)
        {
            if (_catalog is Catalog catalog) return catalog.Fingerprint;
            var names = new List<string>(); foreach (ConfigManifestTable entry in manifest.tables) names.Add(entry.name + ":" + entry.schemaHash);
            names.Sort(StringComparer.Ordinal);
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes("zrconfig-legacy-catalog/1\n" + string.Join("\n", names)))).Replace("-", "").ToLowerInvariant();
        }
        private static long IndexEstimate(List<ConfigManifestTable> entries)
        {
            long result = 0;
            foreach (ConfigManifestTable table in entries) result = checked(result + table.rows * 64L);
            return result;
        }
        private long LiveEstimate()
        {
            long result = 0;
            lock (_observed)
                for (int i = _observed.Count - 1; i >= 0; i--)
                    if (_observed[i].TryGetTarget(out Snapshot? value))
                        result = value.EstimatedRetainedBytes > long.MaxValue - result ? long.MaxValue : result + value.EstimatedRetainedBytes;
                    else _observed.RemoveAt(i);
            return result;
        }
        private long ReadAllowance(PreparationOptions options, long fileLimit, long accumulated, long indexes)
        {
            long live = LiveEstimate();
            if (options.Limits.MaxEstimatedLiveBytes == long.MaxValue) return fileLimit;
            long remaining = options.Limits.MaxEstimatedLiveBytes - live;
            if (indexes > remaining || accumulated > (remaining - indexes) / 4)
                throw new ConfigException(ErrorCode.BudgetExceeded, "Estimated live preparation admission limit exceeded.", "read");
            // Reserve room for input ownership, parsing and conversion before asking the source.
            return Math.Min(fileLimit, (remaining - indexes) / 4 - accumulated);
        }
        private static byte[] ReadManifest(ISyncConfigSource source, long limit, CancellationToken token)
        {
            try { return Read(source, "Common/current.json", limit, token); }
            catch (Exception error) when (ConfigManager.MissingManifest(error) || error.InnerException != null && ConfigManager.MissingManifest(error.InnerException)) { token.ThrowIfCancellationRequested(); return Read(source, "current.json", limit, token); }
        }
        private static async ConfigOperation<byte[]> ReadManifestAsync(IConfigSource source, long limit, CancellationToken token)
        {
            try { return await ReadAsync(source, "Common/current.json", limit, token); }
            catch (Exception error) when (ConfigManager.MissingManifest(error) || error.InnerException != null && ConfigManager.MissingManifest(error.InnerException)) { token.ThrowIfCancellationRequested(); return await ReadAsync(source, "current.json", limit, token); }
        }
        private static byte[] Read(ISyncConfigSource source, string path, long limit, CancellationToken token, string version = "")
        {
            try { return source is IBoundedConfigSource bounded ? bounded.ReadBounded(path, limit, token) : source.Read(path, token); }
            catch(Exception error){Exception failure=ReadFailure(error,path,version);if(ReferenceEquals(failure,error))throw;throw failure;}
        }
        private static async ConfigOperation<byte[]> ReadAsync(IConfigSource source, string path, long limit, CancellationToken token, string version = "")
        {
            try { return await (source is IBoundedAsyncConfigSource bounded ? bounded.ReadBoundedAsync(path, limit, token) : source.ReadAsync(path, token)); }
            catch(Exception error){Exception failure=ReadFailure(error,path,version);if(ReferenceEquals(failure,error))throw;throw failure;}
        }
        private static Exception ReadFailure(Exception error,string path,string version)
        {
            string[] parts=path.Split('/');
            return ConfigException.Normalize(error,"read",version:version.Length>0?version:parts.Length==3&&parts[0]=="versions"?parts[1]:"",table:Path.GetFileName(path)=="current.json"?"":Path.GetFileNameWithoutExtension(path),path:path);
        }
        private static byte[] Own(byte[] data, long limit, string path)
        {
            if (data == null) throw new ConfigException(ErrorCode.ReadFailed, "Source returned null bytes.", "read", relativePath: path);
            if (data.LongLength > limit) throw new ConfigException(ErrorCode.LimitExceeded, "Source byte limit exceeded.", "read", relativePath: path);
            return (byte[])data.Clone();
        }
        private static long Deadline(TimeSpan duration)
        {
            double ticks = duration.TotalSeconds * Stopwatch.Frequency;
            long now = Stopwatch.GetTimestamp();
            if (ticks >= long.MaxValue - now) throw Error(ErrorCode.InvalidPlan);
            return checked(now + (long)ticks);
        }
        private static void Check(CancellationToken token, long deadline)
        {
            if (token.IsCancellationRequested) throw Error(ErrorCode.Cancelled);
            if (Stopwatch.GetTimestamp() >= deadline) throw Error(ErrorCode.DeadlineExceeded);
        }
        private static ConfigException Error(ErrorCode code) => new ConfigException(code, code.ToString());
        private static Exception Map(Exception error, long operation) => ConfigException.Normalize(error, "prepare", operation);
        private sealed class ScopedRegistry : ITableCodecRegistry
        {
            private readonly ITableCodecRegistry _catalog;
            private readonly HashSet<string> _names;
            internal ScopedRegistry(ITableCodecRegistry catalog, HashSet<string> names) { _catalog = catalog; _names = names; }
            public IEnumerable<ITableCodec> EnumerateCodecs()
            {
                if (_catalog is Catalog catalog)
                {
                    foreach (TableDescriptor table in catalog.Tables) if (_names.Contains(table.Name)) yield return table.GetCodec();
                }
                else foreach (ITableCodec codec in _catalog.EnumerateCodecs()) if (_names.Contains(codec.TableName)) yield return codec;
            }
        }
        // Never exposed to callers; bytes are owned copies and can safely outlive the external source.
        private sealed class FrozenSource : ISyncConfigSource
        {
            private readonly Dictionary<string, byte[]> _files;
            internal FrozenSource(Dictionary<string, byte[]> files) => _files = files;
            internal void Release() => _files.Clear();
            public byte[] Read(string path, CancellationToken cancellationToken = default)
            { cancellationToken.ThrowIfCancellationRequested(); return _files.TryGetValue(path, out byte[]? bytes) ? bytes : throw new ConfigException(ErrorCode.SourceUnavailable, "Pinned data missing.", relativePath: path); }
            public ConfigOperation<byte[]> ReadAsync(string path, CancellationToken cancellationToken) => ConfigOperation<byte[]>.FromResult(Read(path, cancellationToken));
        }
    }
}
