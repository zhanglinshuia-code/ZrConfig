#nullable enable
using System;
using System.IO;
using System.Threading;
using ZrConfig.Runtime;

namespace ZrConfig.Sample
{
    /// <summary>
    /// 示例的 AB 字节读取边界。目前用真实文件读写替代尚未选定的 AB SDK，
    /// 不创建 AssetBundle，也不伪造配置数据。正式接入时替换两个 LoadBytes 方法中的读取调用。
    /// </summary>
    public sealed class SampleAbResources
    {
        private readonly FileConfigSource _localFiles;
        private int _syncReads;
        private int _asyncReads;
        public string RootFolder
        {
            get;
        }
        public ISyncConfigSource Source
        {
            get;
        }
        public int SyncReadCount => Volatile.Read(ref _syncReads);
        public int AsyncReadCount => Volatile.Read(ref _asyncReads);

        public event Action<string>? Trace;
        public SampleAbResources(string rootFolder)
        {
            RootFolder = Path.GetFullPath(rootFolder);
            _localFiles = new FileConfigSource(RootFolder);
            Source = new CoalescingSyncConfigSource(new DelegateSyncConfigSource(LoadBytes, LoadBytesAsync));
        }

        public byte[] LoadBytes(string relativePath, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _syncReads);
            string address = "ZrConfig/" + relativePath;
            Trace?.Invoke("同步请求 → " + address);
            try
            {
                // AB 替换点：用 address 调用正式框架的同步资源接口并取得 byte[]。
                // 临时实现将 ZrConfig/ 地址前缀映射到 RootFolder，保留清单提供的相对文件路径。
                byte[] bytes = _localFiles.Read(relativePath, token);
                Trace?.Invoke("同步完成 ← " + address + " · " + bytes.Length + " B");
                return bytes;
            }
            catch (OperationCanceledException)
            {
                Trace?.Invoke("同步取消 · " + address);
                throw;
            }
            catch (Exception error)
            {
                Trace?.Invoke("同步失败 · " + address + " · " + error.Message);
                throw;
            }
        }

        public async ConfigOperation<byte[]> LoadBytesAsync(string relativePath, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _asyncReads);
            string address = "ZrConfig/" + relativePath;
            Trace?.Invoke("异步请求 → " + address);
            try
            {
                // AB 替换点：调用正式框架的异步接口；回调式 API 可桥接 ConfigCompletionSource<byte[]>。
                // 这里真正异步读取文件，不使用同步读取 + FromResult，也不阻塞等待。
                byte[] bytes = await _localFiles.ReadAsync(relativePath, token);
                Trace?.Invoke("异步完成 ← " + address + " · " + bytes.Length + " B");
                // 文件源每次分配独立托管数组，无资源句柄需要释放。运行时持有这些不可变字节。
                return bytes;
            }
            catch (OperationCanceledException)
            {
                Trace?.Invoke("异步取消 · " + address);
                throw;
            }
            catch (Exception error)
            {
                Trace?.Invoke("异步失败 · " + address + " · " + error.Message);
                throw;
            }
        }
    }
}
