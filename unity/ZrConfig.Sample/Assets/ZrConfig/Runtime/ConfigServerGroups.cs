#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace ZrConfig.Runtime.Server
{
    /// <summary>Generated group metadata. The selector only projects immutable key fields.</summary>
    public abstract class GroupDescriptor
    {
        public string Name { get; }
        private protected GroupDescriptor(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
        internal abstract void Prepare(Snapshot snapshot, CancellationToken token);
        public void PrepareFor(Snapshot snapshot, CancellationToken cancellationToken = default) => Prepare(snapshot, cancellationToken);
    }

    public sealed class GroupDescriptor<T, TKey> : GroupDescriptor where T : class, IConfigRow where TKey : notnull
    {
        private readonly Func<ITableCodec> _codec;
        internal readonly Func<T, TKey> Selector;
        internal readonly IEqualityComparer<TKey> Comparer;
        internal int KeyComponents { get; private set; } = 1;
        internal Func<T, long>? VariableKeyBytes { get; private set; }
        public GroupDescriptor(string name, Func<ITableCodec> codec, Func<T, TKey> selector, IEqualityComparer<TKey>? comparer = null) : base(name)
        {
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            Selector = selector ?? throw new ArgumentNullException(nameof(selector));
            Comparer = comparer ?? EqualityComparer<TKey>.Default;
        }
        /// <summary>Returns immutable metadata for generated key storage estimates, excluding fixed per-row overhead.</summary>
        public GroupDescriptor<T, TKey> WithIndexEstimate(int keyComponents, Func<T, long> variableKeyBytes)
        {
            if (keyComponents <= 0) throw new ArgumentOutOfRangeException(nameof(keyComponents));
            if (variableKeyBytes == null) throw new ArgumentNullException(nameof(variableKeyBytes));
            return new GroupDescriptor<T, TKey>(Name, _codec, Selector, Comparer)
                { KeyComponents = keyComponents, VariableKeyBytes = variableKeyBytes };
        }
        internal ITableCodec Codec => _codec();
        internal override void Prepare(Snapshot snapshot, CancellationToken token) => snapshot.PrepareGroup(this, token);
    }

    /// <summary>An immutable logical-order group. A retained group owns its selected snapshot.</summary>
    public sealed class GroupHandle<T> : IReadOnlyList<RowHandle<T>> where T : class, IConfigRow
    {
        private readonly Snapshot _snapshot;
        private readonly ITableCodec _codec;
        private readonly int[] _ordinals;
        private int _ready;
        public int Count => _ordinals.Length;
        public Snapshot Snapshot => _snapshot;
        internal GroupHandle(Snapshot snapshot, ITableCodec codec, int[] ordinals)
        { _snapshot = snapshot; _codec = codec; _ordinals = ordinals; }
        public RowHandle<T> this[int index] => _snapshot.Ordinal<T>(_codec, _ordinals[index], false, default);
        internal GroupHandle<T> Ensure(bool load, CancellationToken token)
        {
            if (Volatile.Read(ref _ready) != 0) return this;
            foreach (int ordinal in _ordinals)
            {
                token.ThrowIfCancellationRequested();
                _snapshot.Ordinal<T>(_codec, ordinal, load, token);
            }
            Volatile.Write(ref _ready, 1);
            return this;
        }
        // Enumeration allocates an iterator; indexed access and repeated group lookup do not.
        public IEnumerator<RowHandle<T>> GetEnumerator() { for (int i = 0; i < Count; i++) yield return this[i]; }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed partial class Snapshot
    {
        private readonly Dictionary<object, object> _groupIndexes = new Dictionary<object, object>();
        private readonly Dictionary<ITableCodec, object> _allRows = new Dictionary<ITableCodec, object>();
        private sealed class GroupWork
        {
            internal readonly TaskCompletionSource<object> Completion = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        private sealed class GroupIndex<T, TKey> where T : class, IConfigRow where TKey : notnull
        {
            internal readonly Dictionary<TKey, GroupHandle<T>> Groups;
            internal readonly GroupHandle<T> Empty;
            internal readonly long RetainedEstimate;
            internal GroupIndex(Snapshot owner, GroupDescriptor<T, TKey> descriptor, CancellationToken token)
            {
                ITableCodec codec = descriptor.Codec;
                int count = owner._storage.RowCount(codec);
                var positions = new Dictionary<TKey, List<int>>(descriptor.Comparer);
                long retained = checked(256L + count * 512L * descriptor.KeyComponents);
                for (int i = 0; i < count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    // Transient values are discarded; scanning a group never fills the row cache.
                    var row = (T)owner._storage.ReadTransient(codec, i, token);
                    TKey key = descriptor.Selector(row);
                    long variable = descriptor.VariableKeyBytes != null ? descriptor.VariableKeyBytes(row) : key is string text ? text.Length * 4L : 0;
                    if (variable < 0) throw new ConfigException(ErrorCode.InvalidPlan, "Negative key storage estimate.");
                    retained = checked(retained + variable);
                    if (!positions.TryGetValue(key, out List<int>? group)) positions.Add(key, group = new List<int>());
                    group.Add(i);
                }
                Groups = new Dictionary<TKey, GroupHandle<T>>(positions.Count, descriptor.Comparer);
                foreach (KeyValuePair<TKey, List<int>> group in positions)
                    Groups.Add(group.Key, new GroupHandle<T>(owner, codec, group.Value.ToArray()));
                Empty = new GroupHandle<T>(owner, codec, Array.Empty<int>());
                RetainedEstimate = retained;
                if (owner._storage.GetCachedTable(codec) != null)
                {
                    foreach (GroupHandle<T> group in Groups.Values) group.Ensure(false, token);
                    owner.CacheRows<T>(codec);
                }
            }
        }
        internal RowHandle<T> Ordinal<T>(ITableCodec codec, int ordinal, bool load, CancellationToken token) where T : class, IConfigRow =>
            Handle((T)_storage.ReadOrdinal(codec, ordinal, load, token));

        public void PrepareGroup<T, TKey>(GroupDescriptor<T, TKey> descriptor, CancellationToken cancellationToken = default)
            where T : class, IConfigRow where TKey : notnull => Index(descriptor, true, cancellationToken);

        private GroupIndex<T, TKey> Index<T, TKey>(GroupDescriptor<T, TKey> descriptor, bool load, CancellationToken token)
            where T : class, IConfigRow where TKey : notnull
        {
            if(descriptor==null)throw new ArgumentNullException(nameof(descriptor));
            try{return IndexCore(descriptor,load,token);}
            catch(Exception error){throw ConfigException.Normalize(error,"group",version:VersionId,table:descriptor.Codec.TableName,fieldPath:descriptor.Name);}
        }
        private GroupIndex<T, TKey> IndexCore<T, TKey>(GroupDescriptor<T, TKey> descriptor, bool load, CancellationToken token)
            where T : class, IConfigRow where TKey : notnull
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            CheckSelected(descriptor.Codec);
            GroupWork work;
            bool owner = false;
            lock (_groupIndexes)
            {
                if (_groupIndexes.TryGetValue(descriptor, out object? cached))
                {
                    if (cached is GroupIndex<T, TKey> index) return index;
                    if (cached is ExceptionDispatchInfo failure) { failure.Throw(); throw new InvalidOperationException(); }
                    work = (GroupWork)cached;
                }
                else
                {
                    if (!load) throw new ConfigException(ErrorCode.TableNotLoaded, "Load the group index explicitly.", table: descriptor.Codec.TableName);
                    token.ThrowIfCancellationRequested();
                    _storage.ServerBudget?.WorkRequest();
                    work = new GroupWork(); _groupIndexes.Add(descriptor, work); owner = true;
                }
            }
            if (!load) throw new ConfigException(ErrorCode.TableNotLoaded, "The group index is being prepared.", table: descriptor.Codec.TableName);
            if (owner)
            {
                long estimate = 0;
                bool reserved = false;
                try
                {
                    estimate = checked(_storage.IndexEstimate(descriptor.Codec) + _storage.RowCount(descriptor.Codec) * 512L * (descriptor.KeyComponents - 1));
                    // Shared work has its own lifetime. Cancelling one waiter must not discard
                    // another caller's result. No cache/manager gate is held during decoding.
                    _storage.ServerBudget?.ReserveIndex(estimate); reserved = true;
                    var index = new GroupIndex<T, TKey>(this, descriptor, CancellationToken.None);
                    if (index.RetainedEstimate > estimate) _storage.ServerBudget?.ReserveIndex(index.RetainedEstimate - estimate);
                    else _storage.ServerBudget?.ReleaseIndex(estimate - index.RetainedEstimate);
                    estimate = index.RetainedEstimate;
                    lock (_groupIndexes) _groupIndexes[descriptor] = index;
                    work.Completion.TrySetResult(index);
                }
                catch (Exception error)
                {
                    if (reserved) _storage.ServerBudget?.ReleaseIndex(estimate);
                    lock (_groupIndexes)
                    {
                        if (ConfigException.IsDataFailure(error))
                        { _groupIndexes[descriptor] = ExceptionDispatchInfo.Capture(error); _storage.ServerBudget?.IndexFault(); }
                        else _groupIndexes.Remove(descriptor);
                    }
                    work.Completion.TrySetException(error); _ = work.Completion.Task.Exception;
                    throw;
                }
                finally { _storage.ServerBudget?.CompleteWork(); }
            }
            if(!owner)_storage.ServerBudget?.Request();
            try
            {
                try { work.Completion.Task.Wait(token); } catch (AggregateException) { return (GroupIndex<T, TKey>)work.Completion.Task.GetAwaiter().GetResult(); }
                token.ThrowIfCancellationRequested();
                return (GroupIndex<T, TKey>)work.Completion.Task.GetAwaiter().GetResult();
            }
            finally{if(!owner)_storage.ServerBudget?.CompleteRequest();}
        }
        public GroupHandle<T> FindGroup<T, TKey>(GroupDescriptor<T, TKey> descriptor, TKey key)
            where T : class, IConfigRow where TKey : notnull
        {
            GroupIndex<T, TKey> index = Index(descriptor, false, default);
            return (index.Groups.TryGetValue(key, out GroupHandle<T>? group) ? group : index.Empty).Ensure(false, default);
        }
        public GroupHandle<T> LoadGroup<T, TKey>(GroupDescriptor<T, TKey> descriptor, TKey key, CancellationToken cancellationToken = default)
            where T : class, IConfigRow where TKey : notnull
        {
            if(descriptor==null)throw new ArgumentNullException(nameof(descriptor));
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                GroupIndex<T, TKey> index = Index(descriptor, true, cancellationToken);
                return (index.Groups.TryGetValue(key, out GroupHandle<T>? group) ? group : index.Empty).Ensure(true, cancellationToken);
            }
            catch(Exception error){throw ConfigException.Normalize(error,"group",version:VersionId,table:descriptor.Codec.TableName,fieldPath:descriptor.Name);}
        }
        public GroupHandle<T> GetRows<T>(ITableCodec codec) where T : class, IConfigRow
        {
            GetTable(codec);
            return CacheRows<T>(codec);
        }
        private GroupHandle<T> CacheRows<T>(ITableCodec codec) where T : class, IConfigRow
        {
            lock (_allRows)
            {
                if (_allRows.TryGetValue(codec, out object? existing)) return (GroupHandle<T>)existing;
                int count = _storage.RowCount(codec);
                long estimate = 128L + count * 4L;
                _storage.ServerBudget?.ReserveIndex(estimate);
                try
                {
                    var ordinals = new int[count];
                    for (int i = 0; i < count; i++) ordinals[i] = i;
                    var rows = new GroupHandle<T>(this, codec, ordinals);
                    _allRows.Add(codec, rows); return rows;
                }
                catch { _storage.ServerBudget?.ReleaseIndex(estimate); throw; }
            }
        }
        public int GetRowCount(ITableCodec codec) { CheckSelected(codec); return _storage.RowCount(codec); }
    }
}
