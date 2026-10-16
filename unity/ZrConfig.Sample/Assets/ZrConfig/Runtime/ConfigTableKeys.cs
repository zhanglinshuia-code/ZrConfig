#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace ZrConfig.Runtime
{
    /// <summary>显式的表头布局声明，仅供导表工具读取，不改变运行时数据。</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ConfigTableLayoutAttribute : Attribute
    {
        public int HeaderRows
        {
            get;
        }

        public ConfigTableLayoutAttribute(int headerRows)
        {
            if (headerRows != 5)
            {
                throw new ArgumentOutOfRangeException(nameof(headerRows));
            }

            HeaderRows = headerRows;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ConfigTableGroupAttribute : Attribute
    {
        public string Name
        {
            get;
        }
        public string[] Fields
        {
            get;
        }

        public ConfigTableGroupAttribute(string name, params string[] fields)
        {
            Name = name;
            Fields = fields;
        }
    }

    /// <summary>加载时建立的只读分组；直接 foreach 使用值类型枚举器，缺失组共用 Empty。</summary>
    public sealed class ConfigRowGroup<T> : IReadOnlyList<T> where T : class, IConfigRow
    {
        private readonly List<T> _rows;
        public static ConfigRowGroup<T> Empty { get; } = new ConfigRowGroup<T>(new List<T>(0));

        internal ConfigRowGroup(List<T> rows) => _rows = rows;
        public int Count => _rows.Count;

        public T this[int index] => _rows[index];
        public List<T>.Enumerator GetEnumerator() => _rows.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
