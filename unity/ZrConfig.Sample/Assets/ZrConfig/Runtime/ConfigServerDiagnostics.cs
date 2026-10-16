#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace ZrConfig.Runtime.Server
{
    /// <summary>Runs before preparation completes, without a manager lock. Must not publish side effects.</summary>
    public interface ICandidateValidator
    {
        void Validate(Snapshot candidate, CancellationToken cancellationToken);
    }
    public sealed class DelegateCandidateValidator : ICandidateValidator
    {
        private readonly Action<Snapshot, CancellationToken> _validate;
        public DelegateCandidateValidator(Action<Snapshot, CancellationToken> validate) => _validate = validate ?? throw new ArgumentNullException(nameof(validate));
        public void Validate(Snapshot candidate, CancellationToken cancellationToken) => _validate(candidate, cancellationToken);
    }
    /// <summary>Latest completed operation for a lifecycle stage. OperationId zero means no sample.
    /// Milliseconds use a monotonic elapsed clock, include waiting, and exclude notification callbacks.</summary>
    public readonly struct StageTiming
    {
        public string Stage { get; }
        public long OperationId { get; }
        public double ElapsedMilliseconds { get; }
        public bool Succeeded { get; }
        internal StageTiming(string stage, long operation, double elapsed, bool succeeded)
        { Stage = stage; OperationId = operation; ElapsedMilliseconds = elapsed; Succeeded = succeeded; }
    }
    public enum ManagerEventKind { Committed, PreparationFailed, Cleared, Closed }
    /// <summary>Contains bounded metadata, never a strong snapshot payload. Generation is the event's generation.</summary>
    public readonly struct ManagerEvent
    {
        public ManagerEventKind Kind { get; }
        public long Generation { get; }
        public long OperationId { get; }
        public string VersionId { get; }
        public ErrorCode? Error { get; }
        internal ManagerEvent(ManagerEventKind kind, long generation, long operation, string version, ErrorCode? error = null)
        { Kind = kind; Generation = generation; OperationId = operation; VersionId = version; Error = error; }
    }

    internal sealed class EventQueue
    {
        private readonly object _gate = new object();
        private readonly int _capacity;
        private readonly Queue<ManagerEvent> _events = new Queue<ManagerEvent>();
        private readonly List<Subscription> _listeners = new List<Subscription>();
        private bool _draining;
        private long _dropped, _listenerFailures;
        internal long Dropped => Interlocked.Read(ref _dropped);
        internal long ListenerFailures => Interlocked.Read(ref _listenerFailures);
        internal EventQueue(int capacity)
        {
            if (capacity < 1) throw new ConfigException(ErrorCode.InvalidPlan, "Event capacity must be positive.");
            _capacity = capacity;
        }
        internal IDisposable Subscribe(Action<ManagerEvent> listener)
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            var subscription = new Subscription(this, listener);
            lock (_gate) _listeners.Add(subscription);
            return subscription;
        }
        internal void Enqueue(ManagerEvent value)
        {
            lock (_gate)
            {
                if (_listeners.Count == 0) return;
                if (_events.Count == _capacity) { _events.Dequeue(); Interlocked.Increment(ref _dropped); }
                _events.Enqueue(value);
            }
        }
        internal void Drain()
        {
            lock (_gate) { if (_draining) return; _draining = true; }
            bool completed = false;
            try
            {
                while (true)
                {
                    ManagerEvent value; Subscription[] listeners;
                    lock (_gate)
                    {
                        if (_events.Count == 0) { _draining = false; completed = true; return; }
                        value = _events.Dequeue(); listeners = _listeners.ToArray();
                    }
                    foreach (Subscription subscription in listeners)
                    {
                        Action<ManagerEvent>? listener = Volatile.Read(ref subscription.Listener);
                        if (listener == null) continue;
                        try { listener(value); }
                        catch (Exception) { Interlocked.Increment(ref _listenerFailures); }
                    }
                }
            }
            finally { if (!completed) lock (_gate) _draining = false; }
        }
        private sealed class Subscription : IDisposable
        {
            private EventQueue? _owner;
            internal Action<ManagerEvent>? Listener;
            internal Subscription(EventQueue owner, Action<ManagerEvent> listener) { _owner = owner; Listener = listener; }
            public void Dispose()
            {
                EventQueue? owner = Interlocked.Exchange(ref _owner, null);
                Interlocked.Exchange(ref Listener, null);
                if (owner != null) lock (owner._gate) owner._listeners.Remove(this);
            }
        }
    }
}
