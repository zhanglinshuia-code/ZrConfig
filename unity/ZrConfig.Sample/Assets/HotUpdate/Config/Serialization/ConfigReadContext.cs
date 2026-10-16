#nullable enable
using System;
using System.IO;
using ZrConfig.Runtime;

namespace ZrConfig.Serialization
{
    /// <summary>共享类型映射，每次单行读取使用独立的元素预算；不使用反射。</summary>
    public sealed class ConfigReadContext
    {
        private int _remainingElements = 8_000_000;
        private readonly int[] _typeMap;
        private ConfigReadContext(int[] typeMap) => _typeMap = typeMap;
        public ConfigReadContext ForRow() => new ConfigReadContext(_typeMap);
        public ConfigReadContext(BinaryReader reader, Func<string, int> resolveType, int maxTypes)
        {
            int count = reader.ReadInt32();
            if (count < 0 || count > maxTypes)
            {
                throw new InvalidDataException("Invalid type table length.");
            }

            _typeMap = count == 0 ? Array.Empty<int>() : new int[count];
            bool[] seen = count == 0 ? Array.Empty<bool>() : new bool[maxTypes];
            for (int i = 0; i < count; i++)
            {
                int id = resolveType(BinaryFormat.ReadString(reader) ?? "");
                if (id < 0 || id >= maxTypes || seen[id])
                {
                    throw new InvalidDataException("Unknown or duplicate polymorphic type.");
                }

                seen[id] = true;
                _typeMap[i] = id;
            }
        }

        public int ReadType(BinaryReader reader)
        {
            int index = reader.ReadInt32();
            if (index == -1)
            {
                return -1;
            }

            if ((uint)index >= (uint)_typeMap.Length)
            {
                throw new InvalidDataException("Invalid polymorphic type index.");
            }

            return _typeMap[index];
        }

        public int ReadLength(BinaryReader reader)
        {
            int count = ReadDimension(reader);
            ClaimElements(count);
            return count;
        }

        public static int ReadDimension(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            ZrConfig.Runtime.Server.DecodeGuard.Elements(count);
            if (count < 0 || count > ConfigLimits.MaxArrayLength)
            {
                throw new InvalidDataException("Invalid array dimension.");
            }

            return count;
        }

        public void ClaimElements(long count)
        {
            ZrConfig.Runtime.Server.DecodeGuard.RowElements(count, 8_000_000 - _remainingElements);
            if (count < 0 || count > ConfigLimits.MaxArrayLength || count > _remainingElements)
            {
                throw new InvalidDataException("Invalid array length or element budget exceeded.");
            }

            _remainingElements -= (int)count;
        }

        public static void CheckDepth(int depth)
        {
            ZrConfig.Runtime.Server.DecodeGuard.Depth(depth);
            if (depth > ConfigLimits.MaxDepth)
            {
                throw new InvalidDataException("Config nesting exceeds 64 levels (possible cycle).");
            }
        }

        public static void CheckLength(int count)
        {
            if (count > ConfigLimits.MaxArrayLength)
            {
                throw new InvalidDataException("Config array is too large.");
            }
        }
    }
}
