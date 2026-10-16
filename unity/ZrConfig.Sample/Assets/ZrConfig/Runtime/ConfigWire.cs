#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ZrConfig.Runtime
{
    /// <summary>生成的 Codec 提供固定结构的 JSON 入口；查询阶段继续复用已有的强类型行和索引。</summary>
    public interface IConfigJsonCodec
    {
        byte[] JsonToBinary(byte[] json);
    }

    /// <summary>统一 v4 与 JSON 的数据语义。转换仅发生在导出或加载阶段，不进入查询热路径。</summary>
    public sealed class ConfigWire
    {
        private static readonly CultureInfo s_culture = CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding s_utf8 = new UTF8Encoding(false, true);
        private readonly ConfigJsonValue _schema;
        private readonly Dictionary<string, ConfigJsonValue> _types = new Dictionary<string, ConfigJsonValue>(StringComparer.Ordinal);
        private readonly List<ConfigJsonValue> _fields;
        private readonly string[] _keys;
        private readonly string[] _tags;
        private readonly string _name;
        private readonly string _hash;
        private readonly bool _hasId;
        private const int Magic = 0x4643525A;
        public ConfigWire(string schema)
        {
            _schema = ConfigJson.Parse(schema);
            _name = _schema["name"].String;
            _hash = _schema["hash"].String;
            _hasId = _schema["hasId"].Boolean;
            _fields = _schema["fields"].Items;
            _keys = _schema["keys"].Items.Select(v => v.String).ToArray();
            foreach (var type in _schema["types"].Items)
                _types.Add(type["id"].String, type);
            _tags = _schema["tags"].Items.Select(t => t.String).ToArray();
        }

        public byte[] BinaryToJson(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes, false);
            using var reader = new BinaryReader(stream, s_utf8, true);
            if (reader.ReadInt32() != Magic || reader.ReadInt32() != 4 || ReadString(reader) != _name || ReadString(reader) != _hash)
                throw new InvalidDataException("二进制表头或结构不匹配：" + _name);
            int count = Length(reader.ReadInt32(), ConfigLimits.MaxRows);
            int tagCount = Length(reader.ReadInt32(), _tags.Length);
            var tags = new string[tagCount];
            var seenTags = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < tagCount; i++)
            {
                tags[i] = ReadString(reader) ?? "";
                if (Array.IndexOf(_tags, tags[i]) < 0 || !seenTags.Add(tags[i]))
                    throw new InvalidDataException("多态类型映射无效。");
            }
            if ((long)count * 12 > stream.Length - stream.Position)
                throw new InvalidDataException("行索引长度不足。 ");
            var offsets = new int[count];
            var lengths = new int[count];
            var ids = new int[count];
            var keys = new ConfigJsonValue[count][];
            for (int i = 0; i < count; i++)
            {
                var budget = new Budget();
                keys[i] = new ConfigJsonValue[_keys.Length];
                for (int k = 0; k < _keys.Length; k++)
                    keys[i][k] = ReadValue(FieldType(_keys[k]), reader, tags, 0, budget);
                ids[i] = reader.ReadInt32();
                offsets[i] = reader.ReadInt32();
                lengths[i] = reader.ReadInt32();
                if (offsets[i] < 0 || lengths[i] < 0 || (i == 0 ? offsets[i] != 0 : (long)offsets[i - 1] + lengths[i - 1] != offsets[i]))
                    throw new InvalidDataException("行索引不连续。");
            }
            long start = stream.Position;
            if (start + (count == 0 ? 0 : (long)offsets[count - 1] + lengths[count - 1]) != stream.Length)
                throw new InvalidDataException("行数据长度不匹配。");
            var rows = new ConfigJsonValue("array");
            for (int i = 0; i < count; i++)
            {
                var budget = new Budget();
                var row = ReadFields(_fields, reader, tags, 0, budget);
                if (stream.Position != start + offsets[i] + lengths[i])
                    throw new InvalidDataException("行数据边界不匹配。");
                for (int k = 0; k < _keys.Length; k++)
                if (!EqualKey(keys[i][k], row[_keys[k]]))
                    throw new InvalidDataException("行主键与索引不匹配。");
                if ((_hasId ? row["Id"].Int : 0) != ids[i])
                    throw new InvalidDataException("行 Id 与索引不匹配。");
                rows.Items.Add(row);
            }
            var result = new ConfigJsonValue("object");
            result.Members.Add("format", ConfigJsonValue.StringValue("ZrConfig.TableJson/1"));
            result.Members.Add("table", ConfigJsonValue.StringValue(_name));
            result.Members.Add("schemaHash", ConfigJsonValue.StringValue(_hash));
            result.Members.Add("rows", rows);
            return ConfigJson.Write(result);
        }

        public byte[] JsonToBinary(byte[] json)
        {
            var document = ConfigJson.ParseTable(json, _schema);
            if (document["format"].String != "ZrConfig.TableJson/1" || document["table"].String != _name || document["schemaHash"].String != _hash || document.Members.Count != 4 || document["rows"].Kind != "array")
                throw new InvalidDataException("JSON 表头或结构不匹配：" + _name);
            var rows = document["rows"].Items;
            Length(rows.Count, ConfigLimits.MaxRows);
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, s_utf8, true);
            writer.Write(Magic);
            writer.Write(4);
            WriteString(writer, _name);
            WriteString(writer, _hash);
            writer.Write(rows.Count);
            writer.Write(_tags.Length);
            foreach (string tag in _tags)
                WriteString(writer, tag);
            var locations = new long[rows.Count];
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var budget = new Budget();
                foreach (string key in _keys)
                    WriteValue(FieldType(key), row[key], writer, 0, budget);
                writer.Write(_hasId ? row["Id"].Int : 0);
                locations[i] = stream.Position;
                writer.Write(0);
                writer.Write(0);
            }
            long start = stream.Position;
            for (int i = 0; i < rows.Count; i++)
            {
                long rowStart = stream.Position;
                WriteFields(_fields, rows[i], writer, 0, new Budget(), false);
                long end = stream.Position;
                stream.Position = locations[i];
                writer.Write(checked((int)(rowStart - start)));
                writer.Write(checked((int)(end - rowStart)));
                stream.Position = end;
            }
            return stream.ToArray();
        }

        private string FieldType(string name) => _fields.First(f => f["name"].String == name)["type"].String;
        private static bool EqualKey(ConfigJsonValue a, ConfigJsonValue b) => a.Kind == b.Kind && a.Text == b.Text;
        private static int Length(int count, int maximum = ConfigLimits.MaxArrayLength) => count >= 0 && count <= maximum ? count : throw new InvalidDataException("数据长度超过限制。");
        private sealed class Budget
        {
            private int _remaining = Server.DecodeGuard.CurrentLimits?.MaxContainerElements ?? 8_000_000;
            public void Claim(long count)
            {
                Server.DecodeGuard.Elements(count);
                if (count > _remaining && Server.DecodeGuard.CurrentLimits != null)
                    throw new Server.ConfigException(Server.ErrorCode.LimitExceeded, "Row container element budget exceeded.", "decode");
                if (count < 0 || count > ConfigLimits.MaxArrayLength || count > _remaining)
                    throw new InvalidDataException("集合元素超过预算。");
                _remaining -= (int)count;
            }
        }
        private static void Depth(int depth)
        {
            Server.DecodeGuard.Depth(depth);
            if (depth > ConfigLimits.MaxDepth)
                throw new InvalidDataException("数据嵌套超过限制。");
        }
        private static bool ReadBool(BinaryReader reader)
        {
            byte value = reader.ReadByte();
            if (value > 1)
                throw new InvalidDataException("bool 编码无效。");
            return value != 0;
        }
        private static string? ReadString(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            Server.DecodeGuard.StringBytes(length);
            if (length == -1)
                return null;
            Length(length, 16 * 1024 * 1024);
            if (length > reader.BaseStream.Length - reader.BaseStream.Position)
                throw new EndOfStreamException();
            return s_utf8.GetString(reader.ReadBytes(length));
        }
        private static void WriteString(BinaryWriter writer, string? value)
        {
            if (value == null)
            {
                writer.Write(-1);
                return;
            }
            byte[] bytes = s_utf8.GetBytes(value);
            Length(bytes.Length, 16 * 1024 * 1024);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }
        private ConfigJsonValue ReadFields(List<ConfigJsonValue> fields, BinaryReader reader, string[] tags, int depth, Budget budget)
        {
            var value = new ConfigJsonValue("object");
            foreach (var field in fields)
                try{value.Members.Add(field["name"].String, ReadValue(field["type"].String, reader, tags, depth + 1, budget));}catch(Exception error){throw Server.DecodeGuard.AtField(error,field["name"].String);}
            return value;
        }
        private void WriteFields(List<ConfigJsonValue> fields, ConfigJsonValue value, BinaryWriter writer, int depth, Budget budget, bool tagged)
        {
            if (value.Kind != "object" || value.Members.Count != fields.Count + (tagged ? 1 : 0))
                throw new InvalidDataException("对象成员缺失或包含未知成员。");
            foreach (var field in fields)
                try{WriteValue(field["type"].String, value[field["name"].String], writer, depth + 1, budget);}catch(Exception error){throw Server.DecodeGuard.AtField(error,field["name"].String);}
        }
        private ConfigJsonValue ReadValue(string id, BinaryReader reader, string[] tags, int depth, Budget budget)
        {
            Depth(depth);
            var type = _types[id];
            string kind = type["kind"].String;
            if (kind == "primitive" || kind == "enum" || kind == "tablereference")
            {
                var result = ReadPrimitive(type["primitive"].String, reader);
                if (kind == "enum" && !type["enumValues"].Items.Any(v => v.String == result.Text))
                    throw new InvalidDataException("未知枚举值。");
                return result;
            }
            if (kind == "rawjson")
                return ConfigJsonValue.StringValue(ReadString(reader) ?? "null");
            if (kind == "nullable")
                return ReadBool(reader) ? ReadValue(type["element"].String, reader, tags, depth + 1, budget) : ConfigJsonValue.Null;
            if (kind == "array")
            {
                int rank = type["rank"].Int;
                long count = 1;
                var dims = new ConfigJsonValue("array");
                for (int i = 0; i < rank; i++)
                {
                    int n = Length(reader.ReadInt32());
                    dims.Items.Add(ConfigJsonValue.Number(n.ToString(s_culture)));
                    count = checked(count * n);
                }
                budget.Claim(count);
                var values = new ConfigJsonValue("array");
                for (long i = 0; i < count; i++)
                    try{values.Items.Add(ReadValue(type["element"].String, reader, tags, depth + 1, budget));}catch(Exception error){throw Server.DecodeGuard.AtField(error,"["+i.ToString(s_culture)+"]");}
                if (rank == 1)
                    return values;
                var matrix = new ConfigJsonValue("object");
                matrix.Members.Add("dimensions", dims);
                matrix.Members.Add("values", values);
                return matrix;
            }
            if (kind == "dictionary")
            {
                int count = Length(reader.ReadInt32());
                budget.Claim(count);
                var value = new ConfigJsonValue("array");
                for (int i = 0; i < count; i++)
                {
                    var pair = new ConfigJsonValue("array");
                    try{pair.Items.Add(ReadValue(type["key"].String, reader, tags, depth + 1, budget));}catch(Exception error){throw Server.DecodeGuard.AtField(error,"["+i.ToString(s_culture)+"].key");}
                    try{pair.Items.Add(ReadValue(type["element"].String, reader, tags, depth + 1, budget));}catch(Exception error){throw Server.DecodeGuard.AtField(error,"["+i.ToString(s_culture)+"].value");}
                    value.Items.Add(pair);
                }
                return value;
            }
            if (kind == "object" && type["variants"].Items.Count != 0)
            {
                int tag = reader.ReadInt32();
                if (tag == -1)
                    return ConfigJsonValue.Null;
                if (tag < 0 || tag >= tags.Length || !type["variants"].Items.Any(v => v.String == tags[tag]))
                    throw new InvalidDataException("多态类型不匹配。");
                var value = ReadFields(_types[tags[tag]]["fields"].Items, reader, tags, depth, budget);
                value.Members.Add("$type", ConfigJsonValue.StringValue(tags[tag]));
                return value;
            }
            if (type["nullableObject"].Boolean && !ReadBool(reader))
                return ConfigJsonValue.Null;
            return ReadFields(type["fields"].Items, reader, tags, depth, budget);
        }
        private void WriteValue(string id, ConfigJsonValue value, BinaryWriter writer, int depth, Budget budget)
        {
            Depth(depth);
            var type = _types[id];
            string kind = type["kind"].String;
            if (kind == "primitive" || kind == "enum" || kind == "tablereference")
            {
                if (kind == "enum" && !type["enumValues"].Items.Any(v => v.String == value.Text))
                    throw new InvalidDataException("未知枚举值。");
                WritePrimitive(type["primitive"].String, value, writer);
                return;
            }
            if (kind == "rawjson")
            {
                ConfigJson.Parse(value.String);
                WriteString(writer, value.String);
                return;
            }
            if (kind == "nullable")
            {
                writer.Write(value.Kind != "null");
                if (value.Kind != "null")
                    WriteValue(type["element"].String, value, writer, depth + 1, budget);
                return;
            }
            if (kind == "array")
            {
                int rank = type["rank"].Int;
                var values = rank == 1 ? value : value["values"];
                if (values.Kind != "array")
                    throw new InvalidDataException("数组格式无效。");
                long count = values.Items.Count;
                if (rank == 1)
                    writer.Write(Length(values.Items.Count));
                else
                {
                    if (value.Members.Count != 2 || value["dimensions"].Kind != "array" || value["dimensions"].Items.Count != rank)
                        throw new InvalidDataException("矩阵维度不匹配。");
                    count = 1;
                    foreach (var dimension in value["dimensions"].Items)
                    {
                        int n = Length(dimension.Int);
                        writer.Write(n);
                        count = checked(count * n);
                    }
                    if (count != values.Items.Count)
                        throw new InvalidDataException("矩阵元素数量不匹配。");
                }
                budget.Claim(count);
                for(int i=0;i<values.Items.Count;i++)
                    try{WriteValue(type["element"].String,values.Items[i],writer,depth+1,budget);}catch(Exception error){throw Server.DecodeGuard.AtField(error,"["+i.ToString(s_culture)+"]");}
                return;
            }
            if (kind == "dictionary")
            {
                if (value.Kind != "array")
                    throw new InvalidDataException("字典必须使用键值条目数组。");
                writer.Write(Length(value.Items.Count));
                budget.Claim(value.Items.Count);
                for(int i=0;i<value.Items.Count;i++)
                {
                    var pair=value.Items[i];
                    if (pair.Kind != "array" || pair.Items.Count != 2)
                        throw new InvalidDataException("字典条目无效。");
                    try{WriteValue(type["key"].String,pair.Items[0],writer,depth+1,budget);}catch(Exception error){throw Server.DecodeGuard.AtField(error,"["+i.ToString(s_culture)+"].key");}
                    try{WriteValue(type["element"].String,pair.Items[1],writer,depth+1,budget);}catch(Exception error){throw Server.DecodeGuard.AtField(error,"["+i.ToString(s_culture)+"].value");}
                }
                return;
            }
            if (kind == "object" && type["variants"].Items.Count != 0)
            {
                if (value.Kind == "null")
                {
                    writer.Write(-1);
                    return;
                }
                string tag = value["$type"].String;
                if (!type["variants"].Items.Any(v => v.String == tag))
                    throw new InvalidDataException("多态类型不匹配。");
                writer.Write(Array.IndexOf(_tags, tag));
                WriteFields(_types[tag]["fields"].Items, value, writer, depth, budget, true);
                return;
            }
            if (type["nullableObject"].Boolean)
            {
                writer.Write(value.Kind != "null");
                if (value.Kind == "null")
                    return;
            }
            WriteFields(type["fields"].Items, value, writer, depth, budget, false);
        }
        private static ConfigJsonValue ReadPrimitive(string type, BinaryReader reader)
        {
            if (type == "string")
                return ConfigJsonValue.StringValue(ReadString(reader) ?? "");
            if (type == "bool")
                return new ConfigJsonValue("bool", ReadBool(reader) ? "true" : "false");
            string text;
            switch (type)
            {
                case "int8":
                    text = reader.ReadSByte().ToString(s_culture);
                    break;
                case "uint8":
                    text = reader.ReadByte().ToString(s_culture);
                    break;
                case "int16":
                    text = reader.ReadInt16().ToString(s_culture);
                    break;
                case "uint16":
                    text = reader.ReadUInt16().ToString(s_culture);
                    break;
                case "int32":
                    text = reader.ReadInt32().ToString(s_culture);
                    break;
                case "uint32":
                    text = reader.ReadUInt32().ToString(s_culture);
                    break;
                case "int64":
                    text = reader.ReadInt64().ToString(s_culture);
                    break;
                case "uint64":
                    text = reader.ReadUInt64().ToString(s_culture);
                    break;
                case "decimal":
                    decimal amount = reader.ReadDecimal();
                    text = amount.ToString(s_culture);
                    // decimal 的负零符号不影响数值，但属于原始编码；JSON 往返需要保留。
                    if (amount == 0m && decimal.GetBits(amount)[3] < 0) text = "-" + text;
                    break;
                case "float32":
                    float f = reader.ReadSingle();
                    if (float.IsNaN(f) || float.IsInfinity(f))
                        throw new InvalidDataException("浮点数必须有限。");
                    text = f.ToString("R", s_culture);
                    break;
                case "float64":
                    double d = reader.ReadDouble();
                    if (double.IsNaN(d) || double.IsInfinity(d))
                        throw new InvalidDataException("浮点数必须有限。");
                    text = d.ToString("R", s_culture);
                    break;
                default:
                    throw new InvalidDataException("未知标量类型：" + type);
            }
            return new ConfigJsonValue(type == "int64" || type == "uint64" || type == "decimal" ? "string" : "number", text);
        }
        // decimal.Parse 会舍入过长的小数；先检查精确系数，禁止无声丢失精度。
        private static decimal ExactDecimal(string text)
        {
            string digits = text;
            if (digits.StartsWith("-", StringComparison.Ordinal) || digits.StartsWith("+", StringComparison.Ordinal))
                digits = digits.Substring(1);
            int point = digits.IndexOf('.');
            int scale = point < 0 ? 0 : digits.Length - point - 1;
            if (point == 0)
                throw new InvalidDataException("decimal 必须包含整数部分。");
            if (point >= 0)
                digits = digits.Remove(point, 1);
            if (digits.Length == 0)
                throw new InvalidDataException("decimal 格式无效。");
            foreach (char c in digits)
            if (c < '0' || c > '9')
                throw new InvalidDataException("decimal 格式无效。");
            digits = digits.TrimStart('0');
            while (scale > 0 && digits.EndsWith("0", StringComparison.Ordinal))
            {
                digits = digits.Substring(0, digits.Length - 1);
                scale--;
            }
            const string maximum = "79228162514264337593543950335";
            if (digits.Length != 0 && (scale > 28 || digits.Length > maximum.Length || (digits.Length == maximum.Length && string.CompareOrdinal(digits, maximum) > 0)))
                throw new InvalidDataException("decimal 超出精确表示范围。");
            return decimal.Parse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, s_culture);
        }

        private static void WritePrimitive(string type, ConfigJsonValue value, BinaryWriter writer)
        {
            if (type == "string")
            {
                WriteString(writer, value.String);
                return;
            }
            if (type == "bool")
            {
                writer.Write(value.Boolean);
                return;
            }
            string expected = type == "int64" || type == "uint64" || type == "decimal" ? "string" : "number";
            if (value.Kind != expected)
                throw new InvalidDataException("数值表示无效：" + type);
            string text = value.Text;
            const NumberStyles integer = NumberStyles.AllowLeadingSign;
            switch (type)
            {
                case "int8":
                    writer.Write(sbyte.Parse(text, integer, s_culture));
                    break;
                case "uint8":
                    writer.Write(byte.Parse(text, integer, s_culture));
                    break;
                case "int16":
                    writer.Write(short.Parse(text, integer, s_culture));
                    break;
                case "uint16":
                    writer.Write(ushort.Parse(text, integer, s_culture));
                    break;
                case "int32":
                    writer.Write(int.Parse(text, integer, s_culture));
                    break;
                case "uint32":
                    writer.Write(uint.Parse(text, integer, s_culture));
                    break;
                case "int64":
                    writer.Write(long.Parse(text, integer, s_culture));
                    break;
                case "uint64":
                    writer.Write(ulong.Parse(text, integer, s_culture));
                    break;
                case "decimal":
                    writer.Write(ExactDecimal(text));
                    break;
                case "float32":
                    float f = float.Parse(text, NumberStyles.Float, s_culture);
                    if (float.IsNaN(f) || float.IsInfinity(f))
                        throw new InvalidDataException("浮点数必须有限。");
                    writer.Write(f);
                    break;
                case "float64":
                    double d = double.Parse(text, NumberStyles.Float, s_culture);
                    if (double.IsNaN(d) || double.IsInfinity(d))
                        throw new InvalidDataException("浮点数必须有限。");
                    writer.Write(d);
                    break;
                default:
                    throw new InvalidDataException("未知标量类型：" + type);
            }
        }
    }
}
