#nullable enable
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZrConfig.Runtime
{
    /// <summary>生成的随机行读取器；打开索引时不创建配置行对象。</summary>
    public interface IIndexedTableCodec : ITableCodec
    {
        IConfigRowReader Open(byte[] bytes);
    }

    public interface IConfigRowReader
    {
        int Count
        {
            get;
        }

        int Find(object key);
        int FindReference(int id);
        IConfigRow Read(int index);
    }

    /// <summary>自定义读取器应实现与主键匹配的泛型接口，避免值类型查询退回 Find(object) 时装箱。</summary>
    public interface IConfigRowReader<TKey> : IConfigRowReader
    {
        int Find(TKey key);
    }

    /// <summary>由生成代码提供键和值的读取函数，不使用反射。</summary>
    internal interface IConfigRowSize { int EncodedBytes(int ordinal); }
    internal interface IConfigRowLookup<TKey> { bool TryFind(TKey key, out int ordinal); }
    public sealed class ConfigRowReader<T, TKey> : IConfigRowReader<TKey>, IConfigRowSize, IConfigRowLookup<TKey> where T : class, IConfigRow where TKey : notnull
    {
        private readonly byte[] _bytes;
        private readonly int[] _offsets;
        private readonly int[] _lengths;
        private readonly int[] _ids;
        private readonly TKey[] _keys;
        private readonly Dictionary<TKey, int> _byKey;
        private readonly Dictionary<int, int>? _byId;
        private readonly bool _validReferenceIds = true;
        private readonly Func<BinaryReader, T> _read;
        private readonly Func<T, TKey> _key;
        private readonly Func<T, int>? _id;
        private readonly object _readGate = new object();
        private RowBinaryReader? _availableReader;
        public int Count => _offsets.Length;
        int IConfigRowSize.EncodedBytes(int ordinal) => _lengths[ordinal];
        bool IConfigRowLookup<TKey>.TryFind(TKey key, out int ordinal) => _byKey.TryGetValue(key, out ordinal);

        public ConfigRowReader(byte[] bytes,
            BinaryReader reader,
            int count,
            Func<BinaryReader, TKey> readKey,
            Func<BinaryReader, T> read,
            Func<T, TKey> key,
            Func<T, int>? id = null,
            IEqualityComparer<TKey>? comparer = null)
        {
            _bytes = bytes;
            _read = read;
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _id = id;
            // 每条索引至少有 id、偏移和长度；先拒绝伪造的巨大行数再分配。
            if (count < 0 || count > (reader.BaseStream.Length - reader.BaseStream.Position) / 12)
            {
                throw new EndOfStreamException("Invalid row index length.");
            }

            _offsets = new int[count];
            _lengths = new int[count];
            // 只有声明 int Id 的表需要引用索引；已知行数时一次分配，避免打开大表时逐级扩容。
            _ids = id == null ? Array.Empty<int>() : new int[count];
            _byId = id == null ? null : new Dictionary<int, int>(count);
            _keys = new TKey[count];
            _byKey = new Dictionary<TKey, int>(count, comparer);
            long payloadLength = 0;
            for (int i = 0; i < count; i++)
            {
                TKey value = readKey(reader);
                int referenceId = reader.ReadInt32();
                int offset = reader.ReadInt32();
                int length = reader.ReadInt32();
                if (length < 0 || offset != payloadLength)
                {
                    throw new InvalidDataException("Invalid, overlapping or non-contiguous row index.");
                }

                payloadLength += length;
                if (payloadLength > bytes.Length)
                {
                    throw new EndOfStreamException("Incomplete row payload.");
                }

                if (!_byKey.TryAdd(value, i))
                {
                    throw Server.DecodeGuard.Invalid(Server.ErrorCode.DuplicateKey, "Duplicate row key.");
                }

                if (_byId != null)
                {
                    if (referenceId == 0 || !_byId.TryAdd(referenceId, i))
                    {
                        _validReferenceIds = false;
                    }

                    _ids[i] = referenceId;
                }

                _keys[i] = value;
                _offsets[i] = offset;
                _lengths[i] = length;
            }

            long dataStart = reader.BaseStream.Position;
            if (payloadLength > reader.BaseStream.Length - dataStart)
            {
                throw new EndOfStreamException("Incomplete row payload.");
            }

            if (payloadLength != reader.BaseStream.Length - dataStart)
            {
                throw new InvalidDataException("Trailing bytes found in the table.");
            }

            for (int i = 0; i < count; i++)
            {
                _offsets[i] = checked((int)(dataStart + _offsets[i]));
            }
        }

        public int Find(object key) => key is TKey typed ? Find(typed) : throw new KeyNotFoundException("Configuration row not found: " + key);
        public int Find(TKey key)
        {
            return _byKey.TryGetValue(key, out int index) ? index : throw new KeyNotFoundException("Configuration row not found: " + key);
        }

        public int FindReference(int id) => _validReferenceIds && id != 0 && _byId != null && _byId.TryGetValue(id,
            out int index) ? index : throw new KeyNotFoundException("Configuration reference not found: " + typeof(T).Name + ".Id=" + id);
        public IConfigRow Read(int index)
        {
            lock (_readGate)
            {
                // 借出期间槽位为空；转换器重入时使用独立游标。
                RowBinaryReader reader = _availableReader ?? new RowBinaryReader(new RowStream(_bytes));
                _availableReader = null;
                bool success = false;
                try
                {
                    reader._input.Reset(_offsets[index], _lengths[index]);
                    T row = _read(reader);
                    if (reader._input.Position != reader._input.Length || (!_byKey.Comparer.Equals(_key(row),
                        _keys[index])) || (_id != null && _id(row) != _ids[index]))
                    {
                        throw new InvalidDataException("Row payload does not match its index.");
                    }

                    success = true;
                    return row;
                }
                finally
                {
                    if (success && reader._reusable && reader._input.CanRead && _availableReader == null)
                    {
                        _availableReader = reader;
                    }
                    else
                    {
                        reader.Dispose();
                    }
                }
            }
        }

        // 字符 API 可能持有 Decoder 状态，使用过它们的读取器不跨行复用。
        // 生成的字符串读取使用显式 UTF-8 字节边界，不经过这些方法。
        private sealed class RowBinaryReader : BinaryReader
        {
            internal readonly RowStream _input;
            internal bool _reusable = true;
            internal RowBinaryReader(RowStream input) : base(input, Encoding.UTF8, false) => _input = input;
            // 直接读取当前行的有界切片，避免 BinaryReader 对一般 Stream 的临时复制。
            public override byte ReadByte() => _input.Take(1)[0];
            public override sbyte ReadSByte() => unchecked((sbyte)ReadByte());
            public override bool ReadBoolean() => Server.DecodeGuard.ReadBoolean(this);
            public override short ReadInt16() => BinaryPrimitives.ReadInt16LittleEndian(_input.Take(2));
            public override ushort ReadUInt16() => BinaryPrimitives.ReadUInt16LittleEndian(_input.Take(2));
            public override int ReadInt32() => BinaryPrimitives.ReadInt32LittleEndian(_input.Take(4));
            public override uint ReadUInt32() => BinaryPrimitives.ReadUInt32LittleEndian(_input.Take(4));
            public override long ReadInt64() => BinaryPrimitives.ReadInt64LittleEndian(_input.Take(8));
            public override ulong ReadUInt64() => BinaryPrimitives.ReadUInt64LittleEndian(_input.Take(8));
            public override float ReadSingle() => Server.DecodeGuard.Finite(BitConverter.Int32BitsToSingle(ReadInt32()));
            public override double ReadDouble() => Server.DecodeGuard.Finite(BitConverter.Int64BitsToDouble(ReadInt64()));
            public override int Read()
            {
                _reusable = false;
                return base.Read();
            }

            public override char ReadChar()
            {
                _reusable = false;
                return base.ReadChar();
            }

            public override char[] ReadChars(int count)
            {
                _reusable = false;
                return base.ReadChars(count);
            }

            public override int Read(char[] buffer, int index, int count)
            {
                _reusable = false;
                return base.Read(buffer, index, count);
            }

            public override int Read(Span<char> buffer)
            {
                _reusable = false;
                return base.Read(buffer);
            }

            public override int PeekChar()
            {
                _reusable = false;
                return base.PeekChar();
            }

            public override string ReadString()
            {
                _reusable = false;
                return base.ReadString();
            }

            protected override void Dispose(bool disposing)
            {
                _reusable = false;
                base.Dispose(disposing);
            }
        }

        // Position/Length 均相对当前行；Seek、Read 和直接 BaseStream 访问不能越过行边界。
        private sealed class RowStream : Stream
        {
            private readonly byte[] _bytes;
            private int _start;
            private int _length;
            private int _position;
            private bool _disposed;
            internal RowStream(byte[] bytes) => _bytes = bytes;
            internal void Reset(int start, int length)
            {
                _start = start;
                _length = length;
                _position = 0;
            }

            public override bool CanRead => !_disposed;
            public override bool CanSeek => !_disposed;
            public override bool CanWrite => false;

            public override long Length
            {
                get
                {
                    CheckOpen();
                    return _length;
                }
            }

            public override long Position
            {
                get
                {
                    CheckOpen();
                    return _position;
                }

                set
                {
                    CheckOpen();
                    if (value < 0 || value > _length)
                    {
                        throw new IOException("Position is outside the configuration row.");
                    }

                    _position = (int)value;
                }
            }

            internal ReadOnlySpan<byte> Take(int count)
            {
                CheckOpen();
                if (count > _length - _position)
                {
                    _position = _length;
                    throw new EndOfStreamException();
                }

                ReadOnlySpan<byte> result = _bytes.AsSpan(_start + _position, count);
                _position += count;
                return result;
            }

            public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
            public override int Read(Span<byte> buffer)
            {
                CheckOpen();
                int count = Math.Min(buffer.Length, _length - _position);
                _bytes.AsSpan(_start + _position, count).CopyTo(buffer);
                _position += count;
                return count;
            }

            public override int ReadByte()
            {
                CheckOpen();
                return _position == _length ? -1 : _bytes[_start + _position++];
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                long start = origin == SeekOrigin.Begin ? 0 : origin == SeekOrigin.Current ? Position : origin == SeekOrigin.End ? Length : throw new ArgumentOutOfRangeException(nameof(origin));
                Position = checked(start + offset);
                return Position;
            }

            public override void Flush() => CheckOpen();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            protected override void Dispose(bool disposing)
            {
                _disposed = true;
                base.Dispose(disposing);
            }

            private void CheckOpen()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(RowStream));
                }
            }
        }
    }
}
