#nullable disable
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Scripting;
using ZrConfig.Runtime;

namespace ZrConfig.Unity
{
    /// <summary>请在 Unity 主线程调用 Load、LoadAsync、Initialize、Reload 和 ReloadAsync，变更通知也将在该线程执行。</summary>
    public static class UnityConfigRuntime
    {
        private static ConfigManager s_initializing;
        private static IConfigSource s_source;
        private static readonly object s_observerGate = new object();
        private static Action<ConfigSnapshot> s_changed;
        private static Delegate[] s_observers = Array.Empty<Delegate>();
        public static ConfigManager Manager
        {
            get;
            private set;
        }
        /// <summary>是否已完成管理器初始化；Initialize 单独调用时仍需 Reload 读取清单。</summary>
        public static bool IsInitialized => Manager != null;
        public static ConfigSnapshot Current => Manager?.Current;

        public static event Action<ConfigSnapshot> Changed
        {
            add
            {
                lock (s_observerGate)
                {
                    s_changed += value;
                    Volatile.Write(ref s_observers, s_changed?.GetInvocationList() ?? Array.Empty<Delegate>());
                }
            }

            remove
            {
                lock (s_observerGate)
                {
                    s_changed -= value;
                    Volatile.Write(ref s_observers, s_changed?.GetInvocationList() ?? Array.Empty<Delegate>());
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            ConfigManager manager = Manager;
            ConfigManager initializing = s_initializing;
            Manager = null;
            s_initializing = null;
            s_source = null;
            lock (s_observerGate)
            {
                s_changed = null;
                Volatile.Write(ref s_observers, Array.Empty<Delegate>());
            }

            // 先结束当前会话，再使外部持有的快照与尚未完成的加载一起失效。
            manager?.Dispose();
            if (!ReferenceEquals(initializing, manager))
            {
                initializing?.Dispose();
            }
        }

        /// <summary>无需全表注册表；生成的查询入口直接提供目标表的编解码器。</summary>
        public static void Initialize() => Initialize(options: null);
        public static void Initialize(ConfigLoadOptions options)
        {
            if (Manager != null || s_initializing != null)
            {
                throw new InvalidOperationException(ConfigText.Get("ZrConfig 已经初始化。", "ZrConfig is already initialized."));
            }

            Manager = new ConfigManager(DecodeManifest, options);
        }

        public static void Initialize(ITableCodecRegistry registry) => Initialize(registry, null);
        public static void Initialize(ITableCodecRegistry registry, ConfigLoadOptions options)
        {
            if (Manager != null || s_initializing != null)
            {
                throw new InvalidOperationException(ConfigText.Get("ZrConfig 已经初始化。", "ZrConfig is already initialized."));
            }

            Manager = new ConfigManager(registry, DecodeManifest, options);
        }

        /// <summary>同步初始化并读取版本清单；配置行在首次查询时加载。</summary>
        public static ConfigSnapshot Load(ISyncConfigSource source,
            CancellationToken cancellationToken = default) => Load(source,
            options: null,
            cancellationToken: cancellationToken);
        public static ConfigSnapshot Load(ISyncConfigSource source, CancellationToken cancellationToken, ConfigLoadOptions options)
        {
            return LoadCore(BeginLoad(options), source, cancellationToken);
        }

        /// <summary>可选表目录仅供显式全量校验使用；初始化时不枚举。</summary>
        public static ConfigSnapshot Load(ITableCodecRegistry registry,
            ISyncConfigSource source,
            CancellationToken cancellationToken = default) => Load(registry,
            source,
            options: null,
            cancellationToken: cancellationToken);
        public static ConfigSnapshot Load(ITableCodecRegistry registry,
            ISyncConfigSource source,
            CancellationToken cancellationToken,
            ConfigLoadOptions options) => LoadCore(BeginLoad(registry,
            options),
            source,
            cancellationToken);
        private static ConfigSnapshot LoadCore(ConfigManager manager,
            ISyncConfigSource source,
            CancellationToken cancellationToken,
            ConfigPreloadPlan preload = null)
        {
            try
            {
                ConfigSnapshot snapshot = preload == null ? manager.Reload(source,
                    cancellationToken) : manager.ReloadPrepared(source,
                    preload,
                    cancellationToken);
                CompleteLoad(manager, source, snapshot);
                return snapshot;
            }
            finally
            {
                if (ReferenceEquals(s_initializing, manager))
                {
                    s_initializing = null;
                }
            }
        }

        /// <summary>首次接入初始化并异步读取版本清单；配置行通过快照按需加载。</summary>
        public static ConfigOperation<ConfigSnapshot> LoadAsync(IConfigSource source,
            CancellationToken cancellationToken = default) => LoadAsync(source,
            options: null,
            cancellationToken: cancellationToken);
        public static ConfigOperation<ConfigSnapshot> LoadAsync(IConfigSource source,
            CancellationToken cancellationToken,
            ConfigLoadOptions options)
        {
            try
            {
                return LoadCoreAsync(BeginLoad(options), source, cancellationToken);
            }
            catch (Exception error)
            {
                // 保留异步入口的异常契约，避免仅为转发再创建一套异步状态机。
                return ConfigOperation<ConfigSnapshot>.FromException(error);
            }
        }

        public static ConfigOperation<ConfigSnapshot> LoadAsync(ITableCodecRegistry registry,
            IConfigSource source,
            CancellationToken cancellationToken = default) => LoadAsync(registry,
            source,
            options: null,
            cancellationToken: cancellationToken);
        public static ConfigOperation<ConfigSnapshot> LoadAsync(ITableCodecRegistry registry,
            IConfigSource source,
            CancellationToken cancellationToken,
            ConfigLoadOptions options)
        {
            try
            {
                return LoadCoreAsync(BeginLoad(registry, options), source, cancellationToken);
            }
            catch (Exception error)
            {
                return ConfigOperation<ConfigSnapshot>.FromException(error);
            }
        }

        private static async ConfigOperation<ConfigSnapshot> LoadCoreAsync(ConfigManager manager,
            IConfigSource source,
            CancellationToken cancellationToken,
            ConfigPreloadPlan preload = null)
        {
            try
            {
                ConfigSnapshot snapshot = preload == null ? await manager.ReloadAsync(source,
                    cancellationToken) : await manager.ReloadPreparedAsync(source,
                    preload,
                    cancellationToken);
                CompleteLoad(manager, source, snapshot);
                return snapshot;
            }
            finally
            {
                if (ReferenceEquals(s_initializing, manager))
                {
                    s_initializing = null;
                }
            }
        }

        private static ConfigManager BeginLoad(ConfigLoadOptions options)
        {
            if (Manager != null || s_initializing != null)
            {
                throw new InvalidOperationException(ConfigText.Get("ZrConfig 已经初始化或正在加载。", "ZrConfig is initialized or loading."));
            }

            var manager = new ConfigManager(DecodeManifest, options);
            s_initializing = manager;
            return manager;
        }

        private static ConfigManager BeginLoad(ITableCodecRegistry registry, ConfigLoadOptions options)
        {
            if (Manager != null || s_initializing != null)
            {
                throw new InvalidOperationException(ConfigText.Get("ZrConfig 已经初始化或正在加载。", "ZrConfig is initialized or loading."));
            }

            var manager = new ConfigManager(registry, DecodeManifest, options);
            s_initializing = manager;
            return manager;
        }

        /// <summary>首次加载时先准备关键行/表，成功后初始化并通知；失败可重新调用。</summary>
        public static ConfigSnapshot LoadPrepared(ISyncConfigSource source,
            ConfigPreloadPlan plan,
            CancellationToken cancellationToken = default,
            ConfigLoadOptions options = null)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            return LoadCore(BeginLoad(options), source, cancellationToken, plan);
        }

        public static ConfigOperation<ConfigSnapshot> LoadPreparedAsync(IConfigSource source,
            ConfigPreloadPlan plan,
            CancellationToken cancellationToken = default,
            ConfigLoadOptions options = null)
        {
            try
            {
                if (plan == null)
                {
                    throw new ArgumentNullException(nameof(plan));
                }

                return LoadCoreAsync(BeginLoad(options), source, cancellationToken, plan);
            }
            catch (Exception error)
            {
                return ConfigOperation<ConfigSnapshot>.FromException(error);
            }
        }

        private static void CompleteLoad(ConfigManager manager, IConfigSource source, ConfigSnapshot snapshot)
        {
            // 播放模式重置使正在等待的首次加载失效。
            if (!ReferenceEquals(s_initializing, manager))
            {
                throw SessionExpired();
            }

            Manager = manager;
            s_source = source;
            Notify(snapshot);
            EnsureCurrentSession(manager);
        }

        [Preserve]
        private static ConfigManifest DecodeManifest(string json) => JsonUtility.FromJson<ConfigManifest>(json);
        /// <summary>使用上次成功加载的资源源同步重载；该源必须支持 ISyncConfigSource。</summary>
        public static ConfigSnapshot Reload(CancellationToken cancellationToken = default) => Reload(s_source as ISyncConfigSource ?? throw new InvalidOperationException(ConfigText.Get("上次成功加载的资源源不支持同步读取，请传入 ISyncConfigSource。",
            "The last successful source does not support synchronous reads; provide an ISyncConfigSource.")),
            cancellationToken);
        public static ConfigSnapshot Reload(ISyncConfigSource source, CancellationToken cancellationToken = default)
        {
            ConfigManager manager = Manager ?? throw new InvalidOperationException(ConfigText.Get("加载配置前请先初始化 ZrConfig。",
                "Initialize ZrConfig before loading."));
            ConfigSnapshot snapshot = manager.Reload(source, cancellationToken);
            CompleteReload(manager, source, snapshot);
            return snapshot;
        }

        /// <summary>使用上次成功加载的资源源；首次加载仍需传入 source。</summary>
        public static ConfigOperation<ConfigSnapshot> ReloadAsync(CancellationToken cancellationToken = default) => ReloadAsync(s_source ?? throw new InvalidOperationException(ConfigText.Get("请先成功加载配置或传入资源源。",
            "Load configuration successfully first or provide a source.")),
            cancellationToken);
        public static async ConfigOperation<ConfigSnapshot> ReloadAsync(IConfigSource source, CancellationToken cancellationToken = default)
        {
            ConfigManager manager = Manager ?? throw new InvalidOperationException(ConfigText.Get("加载配置前请先初始化 ZrConfig。",
                "Initialize ZrConfig before loading."));
            ConfigSnapshot snapshot = await manager.ReloadAsync(source, cancellationToken);
            CompleteReload(manager, source, snapshot);
            return snapshot;
        }

        public static ConfigSnapshot ReloadPrepared(ISyncConfigSource source,
            ConfigPreloadPlan plan,
            CancellationToken cancellationToken = default)
        {
            ConfigManager manager = Manager ?? throw new InvalidOperationException("Initialize ZrConfig before loading.");
            ConfigSnapshot snapshot = manager.ReloadPrepared(source, plan, cancellationToken);
            CompleteReload(manager, source, snapshot);
            return snapshot;
        }

        public static async ConfigOperation<ConfigSnapshot> ReloadPreparedAsync(IConfigSource source,
            ConfigPreloadPlan plan,
            CancellationToken cancellationToken = default)
        {
            ConfigManager manager = Manager ?? throw new InvalidOperationException("Initialize ZrConfig before loading.");
            ConfigSnapshot snapshot = await manager.ReloadPreparedAsync(source, plan, cancellationToken);
            CompleteReload(manager, source, snapshot);
            return snapshot;
        }

        private static void CompleteReload(ConfigManager manager, IConfigSource source, ConfigSnapshot snapshot)
        {
            EnsureCurrentSession(manager);
            if (ReferenceEquals(snapshot, manager.Current))
            {
                s_source = source;
                Notify(snapshot);
            }

            // 监听器也可能结束播放会话，不能在其重置后返回成功结果。
            EnsureCurrentSession(manager);
        }

        private static void EnsureCurrentSession(ConfigManager manager)
        {
            if (!ReferenceEquals(manager, Manager))
            {
                throw SessionExpired();
            }
        }

        private static InvalidOperationException SessionExpired() => new InvalidOperationException(ConfigText.Get("配置加载所属的播放会话已重置。",
            "The play session for this configuration load has been reset."));

        private static void Notify(ConfigSnapshot snapshot)
        {
            // 固定本次通知的监听器；回调中的订阅变更只影响下一次通知。
            ConfigManager manager = Manager;
            Delegate[] observers = Volatile.Read(ref s_observers);
            foreach (Action<ConfigSnapshot> observer in observers)
            {
                // 回调可以重入重载或重置；后续监听器只接收仍为当前版本的通知。
                if (manager == null || !ReferenceEquals(manager, Manager) || !ReferenceEquals(snapshot, manager.Current))
                {
                    return;
                }

                try
                {
                    observer(snapshot);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }
}
