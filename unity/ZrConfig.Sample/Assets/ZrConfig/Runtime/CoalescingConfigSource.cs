#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace ZrConfig.Runtime
{
    /// <summary>
    /// 合并同一适配器内相同路径的在途异步请求；完成即移除，不缓存 current.json 或结果。
    /// 每个调用者独立取消，底层请求只受适配器生命周期和可选超时控制。不同资源根/版本上下文应使用不同实例。
    /// 底层 source 仍负责释放资源句柄，并须返回不会再被修改的字节数组。
    /// </summary>
    public class CoalescingConfigSource : IConfigSource
    {
        private readonly IConfigSource _source;
        private readonly CancellationToken _lifetime;
        private readonly TimeSpan _timeout;
        private readonly object _gate = new object();
        private readonly Dictionary<string, Request> _requests = new Dictionary<string, Request>(StringComparer.Ordinal);
        public CoalescingConfigSource(IConfigSource source, CancellationToken lifetime = default, TimeSpan? timeout = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _lifetime = lifetime;
            _timeout = timeout ?? Timeout.InfiniteTimeSpan;
            if (_timeout != Timeout.InfiniteTimeSpan && (_timeout <= TimeSpan.Zero || _timeout.TotalMilliseconds > int.MaxValue))
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }
        }

        public ConfigOperation<byte[]> ReadAsync(string path, CancellationToken cancellationToken = default)
        {
            if (path == null)
            {
                return ConfigOperation<byte[]>.FromException(new ArgumentNullException(nameof(path)));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return ConfigOperation<byte[]>.FromCanceled(cancellationToken);
            }

            if (_lifetime.IsCancellationRequested)
            {
                return ConfigOperation<byte[]>.FromCanceled(_lifetime);
            }

            Request request;
            Waiter waiter;
            bool start = false;
            lock (_gate)
            {
                if (!_requests.TryGetValue(path, out request!))
                {
                    request = new Request(path);
                    _requests.Add(path, request);
                    start = true;
                }

                waiter = new Waiter(request, cancellationToken);
                request._waiters.Add(waiter);
            }

            if (cancellationToken.CanBeCanceled)
            {
                CancellationTokenRegistration registration = cancellationToken.Register(() => Cancel(waiter));
                bool dispose;
                lock (_gate)
                {
                    dispose = waiter._done;
                    if (!dispose)
                    {
                        waiter._registration = registration;
                    }
                }

                if (dispose)
                {
                    registration.Dispose();
                }
            }

            if (start)
            {
                // 每次底层操作仅等待一次；不同调用者取得各自的单次等待句柄。
                ConfigOperation<bool>.Awaiter awaiter = Run(request).GetAwaiter();
                if (awaiter.IsCompleted)
                {
                    awaiter.GetResult();
                }
                else
                {
                    awaiter.OnCompleted(() => awaiter.GetResult());
                }
            }

            return waiter._completion.Operation;
        }

        private void Cancel(Waiter waiter)
        {
            CancellationTokenRegistration registration;
            lock (_gate)
            {
                if (waiter._done)
                {
                    return;
                }

                waiter._done = true;
                waiter._request._waiters.Remove(waiter);
                registration = waiter._registration;
            }

            registration.Dispose();
            waiter._completion.TrySetCanceled(waiter._token);
        }

        private async ConfigOperation<bool> Run(Request request)
        {
            using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(_lifetime);
            if (_timeout != Timeout.InfiniteTimeSpan)
            {
                lifetime.CancelAfter(_timeout);
            }

            using CancellationTokenRegistration registration = lifetime.Token.Register(() => Finish(request,
                null,
                new OperationCanceledException(lifetime.Token)));
            try
            {
                lifetime.Token.ThrowIfCancellationRequested();
                byte[] bytes = await _source.ReadAsync(request._path, lifetime.Token);
                lifetime.Token.ThrowIfCancellationRequested();
                Finish(request, bytes, null);
            }
            catch (Exception error)
            {
                Finish(request, null, error);
            }

            return true;
        }

        private void Finish(Request request, byte[]? bytes, Exception? error)
        {
            Waiter[] waiters;
            lock (_gate)
            {
                if (request._done)
                {
                    return;
                }

                request._done = true;
                _requests.Remove(request._path);
                waiters = request._waiters.ToArray();
                request._waiters.Clear();
                foreach (Waiter waiter in waiters)
                {
                    waiter._done = true;
                }
            }

            foreach (Waiter waiter in waiters)
            {
                waiter._registration.Dispose();
                if (waiter._token.IsCancellationRequested)
                {
                    waiter._completion.TrySetCanceled(waiter._token);
                }
                else if (error is OperationCanceledException canceled)
                {
                    waiter._completion.TrySetCanceled(canceled.CancellationToken);
                }
                else if (error != null)
                {
                    waiter._completion.TrySetException(error);
                }
                else
                {
                    waiter._completion.TrySetResult(bytes!);
                }
            }
        }

        private sealed class Request
        {
            internal readonly string _path;
            internal readonly List<Waiter> _waiters = new List<Waiter>();
            internal bool _done;
            internal Request(string path) => _path = path;
        }

        private sealed class Waiter
        {
            internal readonly Request _request;
            internal readonly CancellationToken _token;
            internal readonly ConfigCompletionSource<byte[]> _completion = new ConfigCompletionSource<byte[]>(true);
            internal CancellationTokenRegistration _registration;
            internal bool _done;
            internal Waiter(Request request, CancellationToken token)
            {
                _request = request;
                _token = token;
            }
        }
    }

    /// <summary>保留真正的同步路径；同步调用不等待或加入异步请求，异步调用合并在途请求。</summary>
    public sealed class CoalescingSyncConfigSource : CoalescingConfigSource, ISyncConfigSource
    {
        private readonly ISyncConfigSource _source;
        private readonly CancellationToken _lifetime;
        public CoalescingSyncConfigSource(ISyncConfigSource source,
            CancellationToken lifetime = default,
            TimeSpan? timeout = null) : base(source,
            lifetime,
            timeout)
        {
            _source = source;
            _lifetime = lifetime;
        }

        public byte[] Read(string path, CancellationToken cancellationToken = default)
        {
            _lifetime.ThrowIfCancellationRequested();
            cancellationToken.ThrowIfCancellationRequested();
            if (!_lifetime.CanBeCanceled)
            {
                return _source.Read(path, cancellationToken);
            }

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(_lifetime, cancellationToken);
            return _source.Read(path, linked.Token);
        }
    }
}
