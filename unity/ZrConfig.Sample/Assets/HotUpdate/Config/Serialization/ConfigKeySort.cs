#nullable enable
using System;

namespace ZrConfig.Serialization
{
    /// <summary>枚举键排序使用生成的强类型比较函数，避免装箱及每次排序创建比较委托。</summary>
    public static class ConfigKeySort
    {
        public static void Sort<T>(T[] keys, int count, Comparison<T> compare)
        {
            if ((uint)count > (uint)keys.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            // 常见小字典使用插入排序，大字典使用 O(n log n)、O(1) 空间的堆排序。
            if (count <= 16)
            {
                for (int i = 1; i < count; i++)
                {
                    T value = keys[i];
                    int j = i - 1;
                    while (j >= 0 && compare(keys[j], value) > 0)
                    {
                        keys[j + 1] = keys[j];
                        j--;
                    }

                    keys[j + 1] = value;
                }

                return;
            }

            for (int parent = count / 2 - 1; parent >= 0; parent--)
            {
                Sift(keys, parent, count, compare);
            }

            for (int end = count - 1; end > 0; end--)
            {
                T value = keys[end];
                keys[end] = keys[0];
                keys[0] = value;
                Sift(keys, 0, end, compare);
            }
        }

        private static void Sift<T>(T[] keys, int root, int count, Comparison<T> compare)
        {
            T value = keys[root];
            while (root < count / 2)
            {
                int child = root * 2 + 1;
                if (child + 1 < count && compare(keys[child], keys[child + 1]) < 0)
                {
                    child++;
                }

                if (compare(value, keys[child]) >= 0)
                {
                    break;
                }

                keys[root] = keys[child];
                root = child;
            }

            keys[root] = value;
        }
    }
}
