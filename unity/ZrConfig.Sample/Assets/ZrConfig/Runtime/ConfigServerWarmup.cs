#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace ZrConfig.Runtime.Server
{
    /// <summary>Immutable requests for a candidate; never adds tables outside its dependency closure.</summary>
    public sealed class WarmupPlan
    {
        public IReadOnlyList<WarmupRequest> Requests { get; }
        public WarmupPlan(IEnumerable<WarmupRequest> requests)
        {
            var copy = new List<WarmupRequest>(requests ?? throw new ArgumentNullException(nameof(requests)));
            if (copy.Exists(request => request == null)) throw new ConfigException(ErrorCode.InvalidPlan, "Warmup requests must not be null.");
            Requests = copy.AsReadOnly();
        }
    }
    public abstract class WarmupRequest
    {
        public string TableName { get; }
        internal readonly ITableCodec Codec;
        private protected WarmupRequest(ITableCodec codec) { Codec = codec ?? throw new ArgumentNullException(nameof(codec)); TableName = codec.TableName; }
        internal abstract void Apply(Snapshot snapshot, CancellationToken token);
        public static WarmupRequest Row<T, TKey>(ITableCodec codec, TKey key) where T : class, IConfigRow => new RowRequest<T, TKey>(codec, key);
        public static WarmupRequest Group<T, TKey>(GroupDescriptor<T, TKey> descriptor, TKey key) where T : class, IConfigRow where TKey : notnull => new GroupRequest<T, TKey>(descriptor, key);
        private sealed class RowRequest<T, TKey> : WarmupRequest where T : class, IConfigRow
        {
            private readonly TKey _key;
            internal RowRequest(ITableCodec codec, TKey key) : base(codec) { _key = key; }
            internal override void Apply(Snapshot snapshot, CancellationToken token) => snapshot.LoadRow<T, TKey>(Codec, _key, token);
        }
        private sealed class GroupRequest<T, TKey> : WarmupRequest where T : class, IConfigRow where TKey : notnull
        {
            private readonly GroupDescriptor<T, TKey> _descriptor;
            private readonly TKey _key;
            internal GroupRequest(GroupDescriptor<T, TKey> descriptor, TKey key) : base(descriptor.Codec) { _descriptor = descriptor; _key = key; }
            internal override void Apply(Snapshot snapshot, CancellationToken token) => snapshot.LoadGroup(_descriptor, _key, token);
        }
    }
}
