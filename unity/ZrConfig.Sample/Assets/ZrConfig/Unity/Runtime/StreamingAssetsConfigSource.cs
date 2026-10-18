#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using ZrConfig.Runtime;

namespace ZrConfig.Unity
{
    /// <summary>
    /// Asynchronous StreamingAssets reader for desktop, Android APK, iOS and WebGL.
    /// Construct and call on Unity's main thread. Wrap in CoalescingConfigSource to share reads.
    /// </summary>
    public sealed class StreamingAssetsConfigSource : IConfigSource
    {
        private readonly string _root;
        private readonly int _threadId;
        public int ReadCount
        {
            get; private set;
        }
        public event Action<string>? Trace;

        public StreamingAssetsConfigSource(string? root = null)
        {
            root = (root ?? Application.streamingAssetsPath.TrimEnd('/') + "/ZrConfig").TrimEnd('/', '\\');
            _root = root.Contains("://") ? root : new Uri(Path.GetFullPath(root) + Path.DirectorySeparatorChar).AbsoluteUri.TrimEnd('/');
            _threadId = Thread.CurrentThread.ManagedThreadId;
        }

        public async ConfigOperation<byte[]> ReadAsync(string path, CancellationToken cancellationToken = default)
        {
            if (Thread.CurrentThread.ManagedThreadId != _threadId)
            {
                throw new InvalidOperationException("StreamingAssetsConfigSource must be called on Unity's main thread. Do not use a background execution scheduler.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(path) || path.Contains('\\') || path.Contains(':') ||
                path.Split('/').Any(segment => segment.Length == 0 || segment == "." || segment == ".."))
            {
                throw new ArgumentException("Expected a relative configuration resource path.", nameof(path));
            }

            string address = _root + "/" + string.Join("/", path.Split('/').Select(Uri.EscapeDataString));
            ReadCount++;
            Trace?.Invoke("StreamingAssets → " + path);
            using (var request = UnityWebRequest.Get(address))
            {
                request.timeout = 120;
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                // Yield to Unity's player loop; no threads, blocking wait, or Task.Run (WebGL).
                while (!operation.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        request.Abort();
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    await Task.Yield();
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    if (request.responseCode == 404 || (_root.StartsWith("file:", StringComparison.OrdinalIgnoreCase) && !File.Exists(new Uri(address).LocalPath)))
                        throw new FileNotFoundException("Configuration resource not found: " + path, path);
                    throw new IOException("Could not read configuration resource " + path + ": " + request.error);
                }

                byte[] bytes = request.downloadHandler.data;
                if (bytes == null)
                {
                    throw new IOException("Empty response for " + path);
                }
                Trace?.Invoke("StreamingAssets ← " + path + " · " + bytes.Length + " B");
                return bytes;
            }
        }
    }
}
