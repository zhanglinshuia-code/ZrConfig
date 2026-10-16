#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ZrConfig.Runtime
{
    /// <summary>显式全量校验所需的表目录；普通查询直接传入表自己的编解码器。</summary>
    public interface ITableCodecRegistry
    {
        IEnumerable<ITableCodec> EnumerateCodecs();
    }

    public interface ITableCodec
    {
        string TableName
        {
            get;
        }

        string SchemaHash
        {
            get;
        }

        Type RowType
        {
            get;
        }

        IList CreateRows();
        object CreateRow(object[] values);
        IConfigTable CreateTable(IList rows);
        void Write(Stream output, IList rows);
        IList Read(Stream input);
    }
}
