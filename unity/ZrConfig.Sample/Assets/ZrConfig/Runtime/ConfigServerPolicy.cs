#nullable enable
using System;
using System.Threading;
using System.IO;

namespace ZrConfig.Runtime.Server
{
    /// <summary>Sources that enforce a byte cap before allocating the returned buffer.
    /// Implementations must return stable bytes; the server still takes its own copy.</summary>
    public interface IBoundedAsyncConfigSource : IConfigSource
    {
        ConfigOperation<byte[]> ReadBoundedAsync(string relativePath, long maxBytes, CancellationToken cancellationToken = default);
    }
    public interface IBoundedConfigSource : ISyncConfigSource, IBoundedAsyncConfigSource
    {
        byte[] ReadBounded(string relativePath, long maxBytes, CancellationToken cancellationToken = default);
        new ConfigOperation<byte[]> ReadBoundedAsync(string relativePath, long maxBytes, CancellationToken cancellationToken = default);
    }
    /// <summary>Deployment limits can tighten, but cannot relax, wire-format safety limits.
    /// Estimated byte limits are admission estimates, never process heap limits.</summary>
    public sealed class ResourceLimits
    {
        public int MaxTables { get; }
        public int MaxRows { get; }
        public int MaxDecodedRows { get; }
        public int MaxContainerElements { get; }
        public int MaxDepth { get; }
        public int MaxStringBytes { get; }
        public int MaxConcurrentDecodes { get; }
        public int MaxPendingRequests { get; }
        public long MaxEstimatedIndexBytes { get; private set; } = 2L << 30;
        public long MaxEstimatedDecodedBytes { get; }
        public long MaxEstimatedLiveBytes { get; }
        public ResourceLimits(int maxTables = 100_000, int maxRows = ConfigLimits.MaxRows,
            int maxDecodedRows = int.MaxValue, int maxContainerElements = ConfigLimits.MaxArrayLength,
            int maxDepth = ConfigLimits.MaxDepth, int maxStringBytes = 16 * 1024 * 1024,
            int maxConcurrentDecodes = 1, int maxPendingRequests = 4096,
            long maxEstimatedDecodedBytes = long.MaxValue, long maxEstimatedLiveBytes = long.MaxValue)
        {
            if (maxTables < 1 || maxRows < 1 || maxRows > ConfigLimits.MaxRows || maxDecodedRows < 1 ||
                maxContainerElements < 1 || maxContainerElements > ConfigLimits.MaxArrayLength ||
                maxDepth < 1 || maxDepth > ConfigLimits.MaxDepth || maxStringBytes < 1 || maxStringBytes > 16 * 1024 * 1024 ||
                maxConcurrentDecodes < 1 || maxPendingRequests < 1 || maxEstimatedDecodedBytes < 1 || maxEstimatedLiveBytes < 1)
                throw new ConfigException(ErrorCode.InvalidPlan, "Resource limits must be positive and within the protocol limits.");
            MaxTables = maxTables; MaxRows = maxRows; MaxDecodedRows = maxDecodedRows;
            MaxContainerElements = maxContainerElements; MaxDepth = maxDepth; MaxStringBytes = maxStringBytes;
            MaxConcurrentDecodes = maxConcurrentDecodes; MaxPendingRequests = maxPendingRequests;
            MaxEstimatedDecodedBytes = maxEstimatedDecodedBytes; MaxEstimatedLiveBytes = maxEstimatedLiveBytes;
        }
        /// <summary>Returns a copy with a separate derived-index admission budget. Estimates include build workspace.</summary>
        public ResourceLimits WithIndexBudget(long maxEstimatedIndexBytes)
        {
            if (maxEstimatedIndexBytes < 1) throw new ConfigException(ErrorCode.InvalidPlan, "Index budget must be positive.");
            var copy = (ResourceLimits)MemberwiseClone(); copy.MaxEstimatedIndexBytes = maxEstimatedIndexBytes; return copy;
        }
    }

    // Generated codecs call this small bridge. The scope is synchronous and thread-local,
    // restored on every exit, and never flows into unrelated asynchronous business work.
    public static class DecodeGuard
    {
        [ThreadStatic] private static ResourceLimits? s_limits;
        internal readonly struct Scope : IDisposable
        {
            private readonly ResourceLimits? _previous;
            private readonly bool _entered;
            internal Scope(ResourceLimits limits) { _previous = s_limits; _entered = true; s_limits = limits; }
            public void Dispose() { if (_entered) s_limits = _previous; }
        }
        internal static Scope Enter(ResourceLimits limits) => new Scope(limits);
        internal static ResourceLimits? CurrentLimits => s_limits;
        public static void Depth(int depth)
        { if (s_limits != null && depth > s_limits.MaxDepth) throw Limit("Nesting depth limit exceeded."); }
        public static void Elements(long count)
        { if (s_limits != null && count > s_limits.MaxContainerElements) throw Limit("Container element limit exceeded."); }
        public static void RowElements(long count, long alreadyClaimed)
        {
            if (s_limits != null && (alreadyClaimed < 0 || alreadyClaimed > s_limits.MaxContainerElements || count > s_limits.MaxContainerElements - alreadyClaimed))
                throw Limit("Row container element budget exceeded.");
            Elements(count);
        }
        public static void StringBytes(long count)
        { if (s_limits != null && count > s_limits.MaxStringBytes) throw Limit("String byte limit exceeded."); }
        private static ConfigException Limit(string message) => new ConfigException(ErrorCode.LimitExceeded, message, "decode");
        public static Exception Invalid(ErrorCode code, string message) => s_limits == null ? (Exception)new InvalidDataException(message) : new ConfigException(code, message, "decode");
        public static Exception AtField(Exception error,string field)
        {
            if(s_limits==null)return error;
            if(!(ConfigException.Normalize(error,"decode") is ConfigException fault))return error;
            string path=field+(fault.FieldPath.Length==0||fault.FieldPath[0]=='['?"":".")+fault.FieldPath;
            return new ConfigException(fault.Code,fault.Message,fault.Stage,fault.OperationId,fault.VersionId,fault.Table,fault.RelativePath,fault,fault.RowKey,path);
        }
        public static bool ReadBoolean(BinaryReader reader)
        {
            byte value = reader.ReadByte();
            if (s_limits != null && value > 1) throw new ConfigException(ErrorCode.InvalidValue, "Invalid boolean encoding.", "decode");
            return value != 0;
        }
        public static float Finite(float value)
        { if (s_limits != null && (float.IsNaN(value) || float.IsInfinity(value))) throw new ConfigException(ErrorCode.InvalidValue, "Non-finite floating-point value.", "decode"); return value; }
        public static double Finite(double value)
        { if (s_limits != null && (double.IsNaN(value) || double.IsInfinity(value))) throw new ConfigException(ErrorCode.InvalidValue, "Non-finite floating-point value.", "decode"); return value; }
    }

    internal sealed class LoadBudget
    {
        internal readonly ResourceLimits Limits;
        private readonly SemaphoreSlim _decodes;
        private readonly object _gate = new object();
        private int _rows, _pending, _jobs, _faults, _indexFaults;
        private long _estimatedBytes, _estimatedIndexBytes, _merged, _rawBytes;
        [ThreadStatic] private static bool s_decoding;
        internal int Faults => Volatile.Read(ref _faults);
        internal int IndexFaults => Volatile.Read(ref _indexFaults);
        internal long EstimatedDecodedBytes { get { lock (_gate) return _estimatedBytes; } }
        internal long MergedRequests => Interlocked.Read(ref _merged);
        internal long RawBytes => Interlocked.Read(ref _rawBytes);
        internal void OwnBytes(long bytes) => Interlocked.Add(ref _rawBytes, bytes);
        internal LoadBudget(ResourceLimits limits) { Limits = limits; _decodes = new SemaphoreSlim(limits.MaxConcurrentDecodes); }
        internal void Merge() => Interlocked.Increment(ref _merged);
        internal void Fault() => Interlocked.Increment(ref _faults);
        internal void IndexFault() => Interlocked.Increment(ref _indexFaults);
        internal void Request()
        {
            if (Interlocked.Increment(ref _pending) > Limits.MaxPendingRequests)
            { Interlocked.Decrement(ref _pending); throw new ConfigException(ErrorCode.BudgetExceeded, "Pending request budget exceeded.", "schedule"); }
        }
        internal void CompleteRequest() => Interlocked.Decrement(ref _pending);
        internal void WorkRequest()
        {
            if (Interlocked.Increment(ref _jobs) > Limits.MaxPendingRequests)
            { Interlocked.Decrement(ref _jobs); throw new ConfigException(ErrorCode.BudgetExceeded, "Pending shared-work budget exceeded.", "schedule"); }
        }
        internal void CompleteWork() => Interlocked.Decrement(ref _jobs);
        internal long EstimatedIndexBytes { get { lock (_gate) return _estimatedIndexBytes; } }
        internal void ReserveIndex(long bytes)
        {
            lock (_gate) { if (bytes < 0 || bytes > Limits.MaxEstimatedIndexBytes - _estimatedIndexBytes) throw new ConfigException(ErrorCode.BudgetExceeded, "Derived index admission budget exceeded.", "group"); _estimatedIndexBytes += bytes; }
        }
        internal void ReleaseIndex(long bytes) { lock (_gate) _estimatedIndexBytes -= bytes; }
        internal void ReserveTransient(long bytes)
        { lock (_gate) { if (bytes > Limits.MaxEstimatedDecodedBytes - _estimatedBytes) throw new ConfigException(ErrorCode.BudgetExceeded, "Transient decode budget exceeded.", "decode"); _estimatedBytes += bytes; } }
        internal void ReleaseTransient(long bytes) { lock (_gate) _estimatedBytes -= bytes; }
        internal void Reserve(long bytes)
        {
            lock (_gate)
            {
                if (_rows >= Limits.MaxDecodedRows || bytes > Limits.MaxEstimatedDecodedBytes - _estimatedBytes)
                    throw new ConfigException(ErrorCode.BudgetExceeded, "Decoded row admission budget exceeded.", "decode");
                _rows++; _estimatedBytes += bytes;
            }
        }
        internal void Release(long bytes) { lock (_gate) { _rows--; _estimatedBytes -= bytes; } }
        internal void EnterDecode()
        {
            if (s_decoding) throw new ConfigException(ErrorCode.InvalidPlan, "A codec must not recursively load other rows.", "decode");
            _decodes.Wait(); s_decoding = true;
        }
        internal void ExitDecode() { s_decoding = false; _decodes.Release(); }
    }

    public sealed class ManagerPolicy
    {
        public int MaxPendingPreparations {get;}
        public int EventCapacity {get;}
        public ManagerPolicy(int maxPendingPreparations=64,int eventCapacity=64){if(maxPendingPreparations<=0||eventCapacity<=0)throw new ConfigException(ErrorCode.InvalidPlan,"Manager capacities must be positive.");MaxPendingPreparations=maxPendingPreparations;EventCapacity=eventCapacity;}
    }
    internal sealed class PreparationCancellation : IDisposable
    {
        private readonly CancellationToken _caller;
        private readonly CancellationToken _manager;
        private readonly CancellationTokenSource _source;
        internal CancellationToken Token => _source.Token;
        internal PreparationCancellation(CancellationToken caller, TimeSpan timeout,CancellationToken manager=default)
        {
            _caller = caller; _manager = manager; _source = CancellationTokenSource.CreateLinkedTokenSource(caller,manager);
            // Very long deadlines continue to use monotonic checkpoint checks. The timer API
            // has a smaller platform range than TimeSpan; never truncate it into an early expiry.
            if (timeout.TotalMilliseconds <= int.MaxValue) _source.CancelAfter(timeout);
        }
        internal Exception Classify(Exception error, long deadline)
        {
            // CancelAfter uses the timer clock and may become observable before the
            // monotonic checkpoint reaches its deadline. Preserve the cancellation
            // cause instead of misreporting our own timeout as caller cancellation.
            bool timerExpired = _source.IsCancellationRequested && !_manager.IsCancellationRequested;
            if (!_caller.IsCancellationRequested && (timerExpired || System.Diagnostics.Stopwatch.GetTimestamp() >= deadline) &&
                (error is OperationCanceledException || error is ConfigException failure && failure.Code == ErrorCode.Cancelled))
                return new ConfigException(ErrorCode.DeadlineExceeded, "Preparation deadline exceeded.", "prepare", cause: error);
            return error;
        }
        public void Dispose() => _source.Dispose();
    }
}
