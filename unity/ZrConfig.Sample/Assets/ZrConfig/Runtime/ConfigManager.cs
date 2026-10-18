#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ZrConfig.Runtime
{
    /// <summary>读取并校验版本清单后发布快照；服务端可选择保留请求持有的旧快照。</summary>
    public sealed class ConfigManager : IDisposable
    {
        private static readonly UTF8Encoding s_strictUtf8 = new UTF8Encoding(false, true);
        private readonly ITableCodecRegistry? _registry;
        private readonly ConfigLoadOptions? _options;
        private readonly Func<string, ConfigManifest> _decodeManifest;
        private readonly object _reloadGate = new object();
        private bool _reloading;
        private int _disposed;
        private ConfigSnapshot? _candidate;
        private ConfigSnapshot _current = new ConfigSnapshot("");
        public ConfigSnapshot Current => Volatile.Read(ref _current);
        internal object PublicationGate => _reloadGate;

        // 先查候选再查当前，与发布时先切换 Current、再清候选的顺序配对。
        internal bool IsActive(ConfigLazyLoader loader) => Volatile.Read(ref _disposed) == 0 &&
            (_options?.RetainPreviousSnapshots == true || Volatile.Read(ref _candidate)?.HasLoader(loader) == true || Volatile.Read(ref _current).HasLoader(loader));

        public ConfigManager(Func<string, ConfigManifest> decodeManifest) : this(decodeManifest, null)
        {
        }

        public ConfigManager(ITableCodecRegistry registry,
            Func<string, ConfigManifest> decodeManifest) : this(registry,
            decodeManifest,
            null)
        {
        }

        /// <summary>普通业务只需清单解析函数；生成的查询入口会提供各表自己的编解码器。</summary>
        public ConfigManager(Func<string, ConfigManifest> decodeManifest, ConfigLoadOptions? options)
        {
            _decodeManifest = decodeManifest ?? throw new ArgumentNullException(nameof(decodeManifest));
            _options = options;
        }

        public ConfigManager(ITableCodecRegistry registry, Func<string, ConfigManifest> decodeManifest, ConfigLoadOptions? options)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _decodeManifest = decodeManifest ?? throw new ArgumentNullException(nameof(decodeManifest));
            _options = options;
        }

        /// <summary>直接同步读取与解析；已有重载时拒绝重入。</summary>
        public ConfigSnapshot Reload(ISyncConfigSource source,
            CancellationToken cancellationToken = default) => ReloadCore(source,
            false,
            cancellationToken);
        /// <summary>显式预加载全部表并校验成功后才切换版本，适用于工具验收。</summary>
        public ConfigSnapshot ReloadAll(ISyncConfigSource source,
            CancellationToken cancellationToken = default) => ReloadCore(source,
            true,
            cancellationToken);
        /// <summary>先预加载必需配置，全部成功后才发布新快照；失败时保留旧版本。</summary>
        public ConfigSnapshot ReloadPrepared(ISyncConfigSource source,
            ConfigPreloadPlan plan,
            CancellationToken cancellationToken = default) => ReloadCore(source,
            false,
            cancellationToken,
            plan ?? throw new ArgumentNullException(nameof(plan)));
        public ConfigOperation<ConfigSnapshot> ReloadPreparedAsync(IConfigSource source,
            ConfigPreloadPlan plan,
            CancellationToken cancellationToken = default) => ReloadCoreAsync(source,
            false,
            cancellationToken,
            plan ?? throw new ArgumentNullException(nameof(plan)));
        private ConfigSnapshot ReloadCore(ISyncConfigSource source,
            bool all,
            CancellationToken cancellationToken,
            ConfigPreloadPlan? preload = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            cancellationToken.ThrowIfCancellationRequested();
            EnterReload(cancellationToken);
            ConfigSnapshot? candidate = null;
            bool published = false;
            try
            {
                ConfigManifest manifest = _decodeManifest(s_strictUtf8.GetString(ReadManifest(source, cancellationToken)));
                List<ConfigManifestTable> plan = ValidateAndCopy(manifest);
                candidate = new ConfigSnapshot(manifest.versionId, source, plan, _registry, this, _options);
                RegisterCandidate(candidate);
                if (all)
                {
                    candidate.LoadAll(cancellationToken);
                }

                preload?.Run(candidate, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                Publish(candidate);
                published = true;
                return candidate;
            }
            finally
            {
                if (!published)
                {
                    candidate?.Invalidate();
                }

                ExitReload();
            }
        }

        public ConfigOperation<ConfigSnapshot> ReloadAsync(IConfigSource source,
            CancellationToken cancellationToken = default) => ReloadCoreAsync(source,
            false,
            cancellationToken);
        public ConfigOperation<ConfigSnapshot> ReloadAllAsync(IConfigSource source,
            CancellationToken cancellationToken = default) => ReloadCoreAsync(source,
            true,
            cancellationToken);
        private async ConfigOperation<ConfigSnapshot> ReloadCoreAsync(IConfigSource source,
            bool all,
            CancellationToken cancellationToken,
            ConfigPreloadPlan? preload = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            EnterReload(cancellationToken);
            ConfigSnapshot? candidate = null;
            bool published = false;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                byte[] manifestBytes = await ReadManifestAsync(source, cancellationToken);
                ConfigManifest manifest = _decodeManifest(s_strictUtf8.GetString(manifestBytes));
                List<ConfigManifestTable> plan = ValidateAndCopy(manifest);
                candidate = new ConfigSnapshot(manifest.versionId, source, plan, _registry, this, _options);
                RegisterCandidate(candidate);
                if (all)
                {
                    await candidate.LoadAllAsync(cancellationToken);
                }

                if (preload != null)
                {
                    await preload.RunAsync(candidate, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                Publish(candidate);
                published = true;
                return candidate;
            }
            finally
            {
                if (!published)
                {
                    candidate?.Invalidate();
                }

                ExitReload();
            }
        }

        private void EnterReload(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            lock (_reloadGate)
            {
                ThrowIfDisposed();
                if (_reloading)
                {
                    throw new InvalidOperationException("Configuration reload is already in progress.");
                }

                _reloading = true;
            }
        }

        private void ExitReload()
        {
            lock (_reloadGate)
            {
                Volatile.Write(ref _candidate, null);
                _reloading = false;
            }
        }

        private void RegisterCandidate(ConfigSnapshot candidate)
        {
            lock (_reloadGate)
            {
                ThrowIfDisposed();
                Volatile.Write(ref _candidate, candidate);
            }
        }

        private void Publish(ConfigSnapshot candidate)
        {
            ConfigSnapshot previous;
            lock (_reloadGate)
            {
                ThrowIfDisposed();
                // 身份切换同时使旧快照失效；与行/表的最后提交共用短锁。
                previous = Interlocked.Exchange(ref _current, candidate);
                Volatile.Write(ref _candidate, null);
                if (_options?.RetainPreviousSnapshots == true)
                    previous.Detach();
            }

            // 旧缓存清理可能等待编解码器；此时 Current 已指向可用的新版本。
            if (_options?.RetainPreviousSnapshots != true)
                previous.Invalidate();
            ThrowIfDisposed();
        }

        /// <summary>永久停止管理器，使当前快照和在途预加载失效；资源源仍由调用方管理。</summary>
        public void Dispose()
        {
            ConfigSnapshot current;
            ConfigSnapshot? candidate;
            lock (_reloadGate)
            {
                if (_disposed != 0)
                {
                    return;
                }

                Volatile.Write(ref _disposed, 1);
                current = _current;
                candidate = _candidate;
                Volatile.Write(ref _candidate, null);
            }

            current.Invalidate();
            candidate?.Invalidate();
        }

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                throw new ObjectDisposedException(nameof(ConfigManager));
            }
        }

        internal static bool MissingManifest(Exception error) => error is FileNotFoundException || error is DirectoryNotFoundException || error is System.Collections.Generic.KeyNotFoundException ||
            error is Server.ConfigException failure && failure.Code == Server.ErrorCode.SourceUnavailable;

        private static byte[] ReadManifest(ISyncConfigSource source, CancellationToken token)
        {
            try { return source.Read("Common/current.json", token); }
            catch (Exception error) when (MissingManifest(error)) { token.ThrowIfCancellationRequested(); return source.Read("current.json", token); }
        }
        private static async ConfigOperation<byte[]> ReadManifestAsync(IConfigSource source, CancellationToken token)
        {
            try { return await source.ReadAsync("Common/current.json", token); }
            catch (Exception error) when (MissingManifest(error)) { token.ThrowIfCancellationRequested(); return await source.ReadAsync("current.json", token); }
        }

        private List<ConfigManifestTable> ValidateAndCopy(ConfigManifest manifest) => ValidateAndCopy(manifest, _options?.PreferJson == true);

        internal static List<ConfigManifestTable> ValidateAndCopy(ConfigManifest manifest, bool preferJson)
        {
            if (manifest == null || (manifest.format != "ZrConfig.Binary/1" && manifest.format != "ZrConfig.Json/1" && manifest.format != "ZrConfig.Mixed/1") || !SafeSegment(manifest.versionId) || manifest.tables == null || (!string.IsNullOrEmpty(manifest.layout) && manifest.layout != "flat" && manifest.layout != "grouped"))
            {
                throw new InvalidDataException(ConfigText.Get("版本清单无效。", "Invalid manifest."));
            }

            var names = new HashSet<string>(StringComparer.Ordinal);
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<ConfigManifestTable>(manifest.tables.Length);
            foreach (ConfigManifestTable entry in manifest.tables)
            {
                if (entry == null || string.IsNullOrEmpty(entry.name) || !names.Add(entry.name) || !SafeSegment(entry.file) || !files.Add(entry.file) || entry.rows < 0 || entry.rows > ConfigLimits.MaxRows || !IsHash(entry.schemaHash) || !IsHash(entry.sha256))
                {
                    throw new InvalidDataException(ConfigText.Get("版本清单中的表信息无效或不兼容。", "Invalid or incompatible manifest table."));
                }

                bool mixed = manifest.format == "ZrConfig.Mixed/1";
                if (entry.file != entry.name + (manifest.format == "ZrConfig.Json/1" ? ".json" : ".bytes") ||
                    (mixed ? entry.jsonFile != entry.name + ".json" || !IsHash(entry.jsonSha256) : !string.IsNullOrEmpty(entry.jsonFile) || !string.IsNullOrEmpty(entry.jsonSha256)))
                    throw new InvalidDataException("版本清单格式与文件信息不一致。");
                bool json = mixed && preferJson;
                string file = json ? entry.jsonFile : entry.file;
                result.Add(new ConfigManifestTable { name = entry.name, file = file, rows = entry.rows, schemaHash = entry.schemaHash, sha256 = json ? entry.jsonSha256 : entry.sha256,
                    relativePath = manifest.layout == "grouped" ? "Tables/" + (file.EndsWith(".json", StringComparison.Ordinal) ? "json/" : "bytes/") + file : manifest.layout == "flat" ? file : "versions/" + manifest.versionId + "/" + file });
            }

            return result;
        }

        internal static bool MatchesHash(SHA256 hash, byte[] bytes, string expected)
        {
            Span<byte> actual = stackalloc byte[32];
            if (!hash.TryComputeHash(bytes, actual, out int written) || written != actual.Length)
            {
                return false;
            }

            for (int i = 0; i < actual.Length; i++)
            {
                if (actual[i] != (HexValue(expected[i * 2]) << 4 | HexValue(expected[i * 2 + 1])))
                {
                    return false;
                }
            }

            return true;
        }

        private static int HexValue(char value) => value <= '9' ? value - '0' : (value | 0x20) - 'a' + 10;
        private static bool SafeSegment(string value)
        {
            if (string.IsNullOrEmpty(value) || value == "." || value == "..")
            {
                return false;
            }

            foreach (char c in value)
            {
                if (!(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z') && !(c >= '0' && c <= '9') && c != '_' && c != '-' && c != '.')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsHash(string value)
        {
            if (value == null || value.Length != 64)
            {
                return false;
            }

            foreach (char c in value)
            {
                if (!(c >= '0' && c <= '9') && !(c >= 'a' && c <= 'f') && !(c >= 'A' && c <= 'F'))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
