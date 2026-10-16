#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace ZrConfig.Runtime
{
    /// <summary>不依赖 Task 的可等待结果。每次调用只 await 一次；未完成时 GetResult 不会阻塞线程。</summary>
    [AsyncMethodBuilder(typeof(ConfigOperationMethodBuilder<>))]
    public readonly struct ConfigOperation<T>
    {
        private readonly ConfigCompletionSource<T>? _source;
        private readonly T _result;
        internal ConfigOperation(ConfigCompletionSource<T> source)
        {
            _source = source;
            _result = default!;
        }

        private ConfigOperation(T result)
        {
            _source = null;
            _result = result;
        }

        public static ConfigOperation<T> FromResult(T result) => new ConfigOperation<T>(result);
        public static ConfigOperation<T> FromException(Exception error)
        {
            var completion = new ConfigCompletionSource<T>();
            completion.TrySetException(error);
            return completion.Operation;
        }

        public static ConfigOperation<T> FromCanceled(CancellationToken token = default) => FromException(new OperationCanceledException(token));
        public bool IsCompleted => _source == null || _source.IsCompleted;

        public Awaiter GetAwaiter() => new Awaiter(this);
        public readonly struct Awaiter : INotifyCompletion
        {
            private readonly ConfigOperation<T> _operation;
            internal Awaiter(ConfigOperation<T> operation) => _operation = operation;
            public bool IsCompleted => _operation.IsCompleted;

            public T GetResult() => _operation._source == null ? _operation._result : _operation._source.GetResult();
            public void OnCompleted(Action continuation)
            {
                var callback = new ConfigContinuation(continuation,
                    SynchronizationContext.Current,
                    _operation._source?.DeferContinuation ?? false);
                if (_operation._source == null)
                {
                    callback.Invoke();
                }
                else
                {
                    _operation._source.OnCompleted(callback.Invoke);
                }
            }
        }
    }

    /// <summary>将资源加载器的完成回调接入 ConfigOperation；成功、失败、取消只能完成一次。</summary>
    public sealed class ConfigCompletionSource<T>
    {
        private readonly object _gate = new object();
        private bool _completed;
        private bool _awaiting;
        private T _result = default!;
        private ExceptionDispatchInfo? _error;
        private Action? _continuation;
        internal IAsyncStateMachine? _stateMachine;
        internal bool DeferContinuation
        {
            get;
        }

        public ConfigCompletionSource()
        {
        }

        internal ConfigCompletionSource(bool deferContinuation) => DeferContinuation = deferContinuation;
        public ConfigOperation<T> Operation => new ConfigOperation<T>(this);

        public bool IsCompleted
        {
            get
            {
                lock (_gate)
                {
                    return _completed;
                }
            }
        }

        public bool TrySetResult(T result) => Complete(result, null);
        public bool TrySetException(Exception error) => Complete(default!,
            ExceptionDispatchInfo.Capture(error ?? throw new ArgumentNullException(nameof(error))));
        public bool TrySetCanceled(CancellationToken token = default) => TrySetException(new OperationCanceledException(token));
        private bool Complete(T result, ExceptionDispatchInfo? error)
        {
            Action? continuation;
            lock (_gate)
            {
                if (_completed)
                {
                    return false;
                }

                _result = result;
                _error = error;
                _completed = true;
                continuation = _continuation;
                _continuation = null;
                _stateMachine = null;
            }

            continuation?.Invoke();
            return true;
        }

        internal T GetResult()
        {
            lock (_gate)
            {
                if (!_completed)
                {
                    throw new InvalidOperationException("Await the configuration operation before reading its result.");
                }

                _error?.Throw();
                return _result;
            }
        }

        internal void OnCompleted(Action continuation)
        {
            lock (_gate)
            {
                if (_awaiting)
                {
                    throw new InvalidOperationException("Await each configuration operation only once.");
                }

                _awaiting = true;
                if (!_completed)
                {
                    _continuation = continuation;
                    return;
                }
            }

            // 完成可能发生在 IsCompleted 与注册之间，仍须唤醒调用方。
            continuation();
        }
    }

    /// <summary>供编译器生成 async ConfigOperation&lt;T&gt; 方法；不包含线程池调度或 Task 包装。</summary>
    public struct ConfigOperationMethodBuilder<T>
    {
        private ConfigCompletionSource<T> _source;
        public static ConfigOperationMethodBuilder<T> Create() => new ConfigOperationMethodBuilder<T>
        {
            _source = new ConfigCompletionSource<T>()
        };
        public ConfigOperation<T> Task => _source.Operation; // Task 是 C# async builder 协议要求的属性名。

        public void SetResult(T result) => _source.TrySetResult(result);
        public void SetException(Exception error) => _source.TrySetException(error);
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        } // Start 已保存唯一的状态机实例。

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            _source._stateMachine = stateMachine;
            // 隔离方法内对 AsyncLocal 的修改，并在 await 后保留语言等执行上下文。
            new ConfigContinuation(_source._stateMachine.MoveNext, null).Invoke();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine => awaiter.OnCompleted(new ConfigContinuation(_source._stateMachine!.MoveNext,
                null).Invoke);
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine => awaiter.UnsafeOnCompleted(new ConfigContinuation(_source._stateMachine!.MoveNext,
                null).Invoke);
    }

    internal sealed class ConfigContinuation
    {
        private readonly Action _action;
        private readonly SynchronizationContext? _context;
        private readonly ExecutionContext? _executionContext;
        private readonly bool _defer;
        private readonly int _threadId;
        public ConfigContinuation(Action action, SynchronizationContext? context, bool defer = false)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _context = context;
            _executionContext = ExecutionContext.Capture();
            _defer = defer;
            _threadId = Environment.CurrentManagedThreadId;
        }

        public void Invoke()
        {
            if (_context != null && (_defer || _context != SynchronizationContext.Current || _threadId != Environment.CurrentManagedThreadId))
            {
                _context.Post(state => ((ConfigContinuation)state!).Run(), this);
            }
            else if (_defer)
            {
                ThreadPool.QueueUserWorkItem(state => ((ConfigContinuation)state!).Run(), this);
            }
            else
            {
                Run();
            }
        }

        private void Run()
        {
            if (_executionContext == null)
            {
                _action();
            }
            else
            {
                ExecutionContext.Run(_executionContext, state => ((Action)state!).Invoke(), _action);
            }
        }
    }
}
