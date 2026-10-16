#nullable enable
using System;
using System.Buffers;
using System.IO;
using System.Text;
using ZrConfig.Runtime;

namespace ZrConfig.Serialization
{
    /// <summary>独立的小端序 v4 表格式：表头、独立行索引、行数据；更改格式时必须升级 Version。</summary>
    public static class BinaryFormat
    {
        private static readonly UTF8Encoding s_strictUtf8 = new UTF8Encoding(false, true);
        private const int StackBufferSize = 512;
        public const uint Magic = 0x4643525A; // 小端序下对应 'ZRCF'
        public const int Version = 4;
        public const int MaxRows = 5_000_000;
        public const int MaxStringBytes = 16 * 1024 * 1024;
        public static void WriteHeader(BinaryWriter writer, string tableName, string schemaHash, int rowCount)
        {
            if (rowCount < 0 || rowCount > MaxRows)
            {
                throw new InvalidDataException(ConfigText.Get("数据行数无效。", "Invalid row count."));
            }

            writer.Write(Magic);
            writer.Write(Version);
            WriteString(writer, tableName);
            WriteString(writer, schemaHash);
            writer.Write(rowCount);
        }

        public static int ReadHeader(BinaryReader reader, string tableName, string schemaHash)
        {
            if (reader.ReadUInt32() != Magic || reader.ReadInt32() != Version)
            {
                throw new InvalidDataException(ConfigText.Get("ZrConfig 二进制文件头或版本无效。", "Invalid ZrConfig binary header or version."));
            }

            string actualTable = ReadString(reader) ?? string.Empty;
            string actualHash = ReadString(reader) ?? string.Empty;
            if (actualTable != tableName || !string.Equals(actualHash, schemaHash, StringComparison.Ordinal))
            {
                throw new InvalidDataException(ConfigText.Get("表名或结构哈希不匹配，请同步重新生成代码和数据。",
                    "Table name / SchemaHash mismatch. Regenerate code and data together."));
            }

            int rowCount = reader.ReadInt32();
            if (rowCount < 0 || rowCount > MaxRows)
            {
                throw new InvalidDataException(ConfigText.Get("数据行数无效。", "Invalid row count."));
            }

            return rowCount;
        }

        public static void WriteString(BinaryWriter writer, string? value)
        {
            if (value == null)
            {
                writer.Write(-1);
                return;
            }

            // UTF-8 每个 UTF-16 code unit 最多占 3 字节；短字符串只编码一次。
            if (value.Length <= StackBufferSize / 3)
            {
                Span<byte> small = stackalloc byte[value.Length * 3];
                int written = Encoding.UTF8.GetBytes(value.AsSpan(), small);
                writer.Write(written);
                writer.Write(small.Slice(0, written));
                return;
            }

            int length = Encoding.UTF8.GetByteCount(value);
            if (length > MaxStringBytes)
            {
                throw new InvalidDataException(ConfigText.Get("字符串超过允许的长度。", "String is too large."));
            }

            writer.Write(length);
            if (length == 0)
            {
                return;
            }

            byte[]? rented = null;
            Span<byte> buffer = length <= StackBufferSize ? stackalloc byte[length] : (rented = ArrayPool<byte>.Shared.Rent(length)).AsSpan(0,
                length);
            try
            {
                Encoding.UTF8.GetBytes(value.AsSpan(), buffer);
                writer.Write(buffer);
            }
            finally
            {
                if (rented != null)
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
        }

        public static string? ReadString(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            ZrConfig.Runtime.Server.DecodeGuard.StringBytes(length);
            if (length == -1)
            {
                return null;
            }

            if (length < 0 || length > MaxStringBytes)
            {
                throw new InvalidDataException(ConfigText.Get("字符串长度无效。", "Invalid string length."));
            }

            if (length == 0)
            {
                return string.Empty;
            }

            if (reader.BaseStream.CanSeek && length > reader.BaseStream.Length - reader.BaseStream.Position)
            {
                throw new EndOfStreamException(ConfigText.Get("字符串数据不完整。", "Incomplete string data."));
            }

            byte[]? rented = null;
            Span<byte> buffer = length <= StackBufferSize ? stackalloc byte[length] : (rented = ArrayPool<byte>.Shared.Rent(length)).AsSpan(0,
                length);
            try
            {
                int offset = 0;
                while (offset < length)
                {
                    int read = reader.Read(buffer.Slice(offset));
                    if (read == 0)
                    {
                        throw new EndOfStreamException(ConfigText.Get("字符串数据不完整。", "Incomplete string data."));
                    }

                    offset += read;
                }

                return s_strictUtf8.GetString(buffer);
            }
            finally
            {
                if (rented != null)
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
        }
    }
}
