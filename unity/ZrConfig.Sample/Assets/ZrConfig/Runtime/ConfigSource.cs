#nullable enable
using System;
using System.IO;
using System.Threading;

namespace ZrConfig.Runtime
{
    /// <summary>资源读取边界，不绑定具体 AB 框架；相对路径统一使用正斜杠。请求合并、取消隔离和句柄释放由适配层负责；返回不可变且持续有效的托管字节。</summary>
    public interface IConfigSource
    {
        ConfigOperation<byte[]> ReadAsync(string relativePath, CancellationToken cancellationToken);
    }

    /// <summary>将项目资源系统的异步字节读取函数接入配置，无需另建适配器类。</summary>
    public sealed class DelegateConfigSource : IConfigSource
    {
        private readonly Func<string, CancellationToken, ConfigOperation<byte[]>> _read;
        public DelegateConfigSource(Func<string, CancellationToken, ConfigOperation<byte[]>> read) => _read = read ?? throw new ArgumentNullException(nameof(read));
        public ConfigOperation<byte[]> ReadAsync(string relativePath,
            CancellationToken cancellationToken) => _read(relativePath,
            cancellationToken);
    }

    /// <summary>提供真正的同步读取；仅支持网络异步读取的资源源无需实现此接口。</summary>
    public interface ISyncConfigSource : IConfigSource
    {
        byte[] Read(string relativePath, CancellationToken cancellationToken = default);
    }

    /// <summary>第三方 AB 的通用双入口：分别绑定真实同步、异步读取函数。框架确定后只需实现这两个函数，配置查询与缓存用法不变。</summary>
    public sealed class DelegateSyncConfigSource : ISyncConfigSource
    {
        private readonly Func<string, CancellationToken, byte[]> _read;
        private readonly Func<string, CancellationToken, ConfigOperation<byte[]>> _readAsync;
        public DelegateSyncConfigSource(Func<string, CancellationToken, byte[]> read,
            Func<string, CancellationToken, ConfigOperation<byte[]>> readAsync)
        {
            _read = read ?? throw new ArgumentNullException(nameof(read));
            _readAsync = readAsync ?? throw new ArgumentNullException(nameof(readAsync));
        }

        public byte[] Read(string relativePath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _read(relativePath, cancellationToken);
        }

        public ConfigOperation<byte[]> ReadAsync(string relativePath,
            CancellationToken cancellationToken) => _readAsync(relativePath,
            cancellationToken);
    }

    /// <summary>适用于真实文件目录；同步和异步分别读取，打包后的 StreamingAssets 应通过资源适配器读取。</summary>
    public sealed class FileConfigSource : ISyncConfigSource, Server.IBoundedConfigSource
    {
        private readonly string _root;
        public FileConfigSource(string folder) => _root = Path.GetFullPath(folder);
        public ConfigOperation<byte[]> ReadAsync(string relativePath, CancellationToken cancellationToken = default) => ReadCoreAsync(relativePath, int.MaxValue, false, cancellationToken);
        public ConfigOperation<byte[]> ReadBoundedAsync(string relativePath, long maxBytes, CancellationToken cancellationToken = default) => ReadCoreAsync(relativePath, maxBytes, true, cancellationToken);
        private async ConfigOperation<byte[]> ReadCoreAsync(string relativePath, long maxBytes, bool rejectLinks, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidatePath(relativePath);
            if (rejectLinks) RejectLinks(relativePath);
            using (var stream = new FileStream(Path.Combine(_root,
                relativePath),
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                1,
                FileOptions.Asynchronous))
            {
                long length = stream.Length;
                if (length > maxBytes) throw new Server.ConfigException(Server.ErrorCode.LimitExceeded, "Source byte limit exceeded before allocation.", "read", relativePath: relativePath);
                if (length > int.MaxValue)
                {
                    throw new IOException(ConfigText.Get("配置文件过大。", "Configuration file is too large."));
                }

                byte[] bytes = new byte[(int)length];
                int offset = 0;
                while (offset < bytes.Length)
                {
                    // 文件内部逐块读取不往返 Unity 主线程；调用方 await ConfigOperation 时仍恢复原上下文。
                    int read = await stream.ReadAsync(bytes,
                        offset,
                        Math.Min(64 * 1024,
                        bytes.Length - offset),
                        cancellationToken)
                        .ConfigureAwait(false);
                    if (read == 0)
                    {
                        throw new EndOfStreamException(ConfigText.Get("配置文件数据不完整。", "Incomplete configuration file."));
                    }

                    offset += read;
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (stream.Length != length)
                {
                    throw new IOException(ConfigText.Get("读取过程中配置文件发生变化。", "Configuration file changed while reading."));
                }

                if (rejectLinks) RejectLinks(relativePath);
                return bytes;
            }
        }

        public byte[] Read(string relativePath, CancellationToken cancellationToken = default) => ReadCore(relativePath, int.MaxValue, false, cancellationToken);
        public byte[] ReadBounded(string relativePath, long maxBytes, CancellationToken cancellationToken = default) => ReadCore(relativePath, maxBytes, true, cancellationToken);
        private byte[] ReadCore(string relativePath, long maxBytes, bool rejectLinks, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidatePath(relativePath);
            if (rejectLinks) RejectLinks(relativePath);
            using (var stream = new FileStream(Path.Combine(_root, relativePath), FileMode.Open, // 已直接读入最终 byte[]，无需额外的 FileStream 缓冲区。
 FileAccess.Read, FileShare.Read, 1))
            {
                long length = stream.Length;
                if (length > maxBytes) throw new Server.ConfigException(Server.ErrorCode.LimitExceeded, "Source byte limit exceeded before allocation.", "read", relativePath: relativePath);
                if (length > int.MaxValue)
                {
                    throw new IOException(ConfigText.Get("配置文件过大。", "Configuration file is too large."));
                }

                byte[] bytes = new byte[(int)length];
                int offset = 0;
                while (offset < bytes.Length)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int read = stream.Read(bytes, offset, Math.Min(64 * 1024, bytes.Length - offset));
                    if (read == 0)
                    {
                        throw new EndOfStreamException(ConfigText.Get("配置文件数据不完整。", "Incomplete configuration file."));
                    }

                    offset += read;
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (stream.Length != length)
                {
                    throw new IOException(ConfigText.Get("读取过程中配置文件发生变化。", "Configuration file changed while reading."));
                }

                if (rejectLinks) RejectLinks(relativePath);
                return bytes;
            }
        }

        private void RejectLinks(string relativePath)
        {
            string current = _root;
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new Server.ConfigException(Server.ErrorCode.ReadFailed, "The source root must not be a link.", "read");
            foreach (string part in relativePath.Split('/'))
            {
                current = Path.Combine(current, part);
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new Server.ConfigException(Server.ErrorCode.ReadFailed, "Links inside the source root are not supported.", "read", relativePath: relativePath);
            }
        }
        private static void ValidatePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath) || relativePath.Contains("\\") || Path.IsPathRooted(relativePath))
            {
                throw new InvalidDataException(ConfigText.Get("配置文件路径无效。", "Invalid configuration path."));
            }

            int segmentStart = 0;
            for (int i = 0; i <= relativePath.Length; i++)
            {
                if (i != relativePath.Length && relativePath[i] != '/')
                {
                    continue;
                }

                int length = i - segmentStart;
                if (length == 0 || (relativePath[segmentStart] == '.' && (length == 1 || (length == 2 && relativePath[segmentStart + 1] == '.'))))
                {
                    throw new InvalidDataException(ConfigText.Get("配置文件路径中包含无效目录名。", "Invalid configuration path segment."));
                }

                segmentStart = i + 1;
            }
        }
    }

    // 采用公开字段，使传输对象可直接交给 Unity JsonUtility，无需额外 JSON 包。
    [Serializable]
#pragma warning disable IDE1006 // These fields are the persisted JSON contract used by Unity JsonUtility.

    public sealed class ConfigManifest
    {
        public string format = "";
        public string target = "";
        public string versionId = "";
        public string layout = "";
        public ConfigManifestTable[] tables = Array.Empty<ConfigManifestTable>();
    }

    [Serializable]
    public sealed class ConfigManifestTable
    {
        internal string relativePath = "";
        public string name = "";
        public string file = "";
        public int rows;
        public string schemaHash = "";
        public string sha256 = "";
        public string jsonFile = "";
        public string jsonSha256 = "";
    }
#pragma warning restore IDE1006
}
