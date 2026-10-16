#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ZrConfig.Runtime
{
    /// <summary>仅在加载阶段使用的严格 JSON 值；数值保留十进制文本，禁止先经过 double 丢失整数精度。</summary>
    public sealed class ConfigJsonValue
    {
        public readonly string Kind;
        public readonly string Text;
        public readonly List<ConfigJsonValue> Items;
        public readonly Dictionary<string, ConfigJsonValue> Members;
        public static readonly ConfigJsonValue Null = new ConfigJsonValue("null");
        public ConfigJsonValue(string kind, string text = "")
        {
            Kind = kind;
            Text = text;
            Items = kind == "array" ? new List<ConfigJsonValue>() : null!;
            Members = kind == "object" ? new Dictionary<string, ConfigJsonValue>(StringComparer.Ordinal) : null!;
        }
        public ConfigJsonValue this[string name] => Kind == "object" && Members.TryGetValue(name, out var value) ? value : throw new InvalidDataException("JSON 缺少成员：" + name);
        public string String => Kind == "string" ? Text : throw new InvalidDataException("JSON 值必须是字符串。");
        public int Int => Kind == "number" && int.TryParse(Text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int value) ? value : throw new InvalidDataException("JSON 值必须是 int32。");
        public bool Boolean => Kind == "bool" ? Text == "true" : throw new InvalidDataException("JSON 值必须是 bool。");
        public static ConfigJsonValue Number(string value) => new ConfigJsonValue("number", value);
        public static ConfigJsonValue StringValue(string value) => new ConfigJsonValue("string", value);
    }

    /// <summary>不依赖第三方 JSON 库，适用于 Unity；重复键、非法数值和非法 UTF-8 均明确拒绝。</summary>
    public static class ConfigJson
    {
        private static readonly UTF8Encoding s_utf8 = new UTF8Encoding(false, true);
        public static ConfigJsonValue Parse(byte[] bytes) => Parse(s_utf8.GetString(bytes));
        public static ConfigJsonValue Parse(string text)
        {
            return ParseCore(text, null);
        }
        internal static ConfigJsonValue ParseTable(byte[] bytes, ConfigJsonValue schema)
        {
            var limits = Server.DecodeGuard.CurrentLimits;
            return ParseCore(s_utf8.GetString(bytes), limits == null ? null : new AllocationBounds(schema, limits));
        }
        internal static void ValidateManifest(string text,Server.ResourceLimits limits) => ParseCore(text,new AllocationBounds(limits));
        private static ConfigJsonValue ParseCore(string text, AllocationBounds? bounds)
        {
            var parser = new Parser(text, bounds);
            ConfigJsonValue value = parser.Read(0);
            parser.Space();
            if (!parser.End)
                throw new InvalidDataException("JSON 存在多余内容。");
            return value;
        }
        public static byte[] Write(ConfigJsonValue value)
        {
            var text = new StringBuilder();
            WriteValue(text, value, 0);
            text.Append('\n');
            return s_utf8.GetBytes(text.ToString());
        }
        private static void WriteValue(StringBuilder output, ConfigJsonValue value, int depth)
        {
            if (depth > ConfigLimits.MaxDepth + 8)
                throw new InvalidDataException("JSON 嵌套过深。");
            switch (value.Kind)
            {
                case "null":
                    output.Append("null");
                    break;
                case "bool":
                case "number":
                    output.Append(value.Text);
                    break;
                case "string":
                    Quote(output, value.Text);
                    break;
                case "array":
                    output.Append('[');
                    for (int i = 0; i < value.Items.Count; i++)
                    {
                        if (i != 0)
                            output.Append(',');
                        WriteValue(output, value.Items[i], depth + 1);
                    }
                    output.Append(']');
                    break;
                case "object":
                    output.Append('{');
                    bool first = true;
                    foreach (var member in value.Members)
                    {
                        if (!first)
                            output.Append(',');
                        first = false;
                        Quote(output, member.Key);
                        output.Append(':');
                        WriteValue(output, member.Value, depth + 1);
                    }
                    output.Append('}');
                    break;
                default:
                    throw new InvalidDataException("未知 JSON 类型。");
            }
        }
        private static void Quote(StringBuilder output, string value)
        {
            output.Append('"');
            foreach (char c in value)
            {
                if (c == '"' || c == '\\')
                    output.Append('\\').Append(c);
                else if (c < 32)
                    output.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                else
                    output.Append(c);
            }
            output.Append('"');
        }
        // Bound the encoded representation before constructing its DOM. Exact logical
        // element/string/depth checks still run in ConfigWire (dictionary pairs, matrix
        // dimensions, schema names and type tags add bounded representation overhead).
        private sealed class AllocationBounds
        {
            internal readonly int Rows, Elements, Members, StringBytes, Depth;
            internal readonly string RootArray="rows";
            internal AllocationBounds(Server.ResourceLimits limits){Rows=limits.MaxTables;Elements=64;Members=32;StringBytes=4096;Depth=8;RootArray="tables";}
            internal AllocationBounds(ConfigJsonValue schema, Server.ResourceLimits limits)
            {
                Rows = limits.MaxRows; Elements = Math.Max(64, limits.MaxContainerElements);
                Members = Math.Max(4, schema["fields"].Items.Count + 1);
                int stringBytes = Math.Max(256, limits.MaxStringBytes);
                Depth = Math.Min(ConfigLimits.MaxDepth + 8, limits.MaxDepth * 2 + 8);
                void Name(string value) => stringBytes = Math.Max(stringBytes, s_utf8.GetByteCount(value));
                Name(schema["name"].String);
                foreach(var field in schema["fields"].Items) Name(field["name"].String);
                foreach(var type in schema["types"].Items)
                {
                    Name(type["id"].String);
                    Members = Math.Max(Members, type["fields"].Items.Count + 1);
                    foreach(var field in type["fields"].Items) Name(field["name"].String);
                }
                StringBytes = stringBytes;
            }
            internal static Exception Exceeded() => new Server.ConfigException(Server.ErrorCode.LimitExceeded, "JSON allocation budget exceeded.", "parse");
        }
        private sealed class Parser
        {
            private readonly string _text;
            private int _position;
            private int _remaining = 32_000_000;
            private readonly AllocationBounds? _bounds;
            internal Parser(string text, AllocationBounds? bounds) { _text = text; _bounds = bounds; }
            internal bool End => _position == _text.Length;
            internal void Space()
            {
                while (!End && (_text[_position] == ' ' || _text[_position] == '\r' || _text[_position] == '\n' || _text[_position] == '\t'))
                    _position++;
            }
            private char Take() => !End ? _text[_position++] : throw new InvalidDataException("JSON 意外结束。");
            private void Expect(char expected)
            {
                if (Take() != expected)
                    throw new InvalidDataException("JSON 格式错误。");
            }
            private bool Consume(char value)
            {
                Space();
                if (!End && _text[_position] == value)
                {
                    _position++;
                    return true;
                }
                return false;
            }
            internal ConfigJsonValue Read(int depth, bool rows = false)
            {
                if (_bounds != null && depth > _bounds.Depth) throw AllocationBounds.Exceeded();
                if (depth > ConfigLimits.MaxDepth + 8 || --_remaining < 0)
                    throw new InvalidDataException("JSON 超过解析预算。");
                Space();
                if (End)
                    throw new InvalidDataException("JSON 值缺失。");
                char c = _text[_position];
                if (c == '"')
                    return ConfigJsonValue.StringValue(ReadString());
                if (c == '{' || c == '[')
                {
                    _position++;
                    bool obj = c == '{';
                    var value = new ConfigJsonValue(obj ? "object" : "array");
                    char close = obj ? '}' : ']';
                    if (Consume(close))
                        return value;
                    do
                    {
                        if (obj)
                        {
                            if (_bounds != null && value.Members.Count >= _bounds.Members) throw AllocationBounds.Exceeded();
                            Space();
                            string name = ReadString();
                            Space();
                            Expect(':');
                            if (value.Members.ContainsKey(name))
                                throw new InvalidDataException("JSON 重复成员：" + name);
                            value.Members.Add(name, Read(depth + 1, depth == 0 && name == (_bounds?.RootArray??"rows")));
                        }
                        else
                        {
                            if (_bounds != null && value.Items.Count >= (rows ? _bounds.Rows : _bounds.Elements)) throw AllocationBounds.Exceeded();
                            value.Items.Add(Read(depth + 1));
                        }
                    } while (Consume(','));
                    Space();
                    Expect(close);
                    return value;
                }
                if (c == 't' || c == 'f' || c == 'n')
                {
                    string token = c == 't' ? "true" : c == 'f' ? "false" : "null";
                    foreach (char item in token)
                        Expect(item);
                    return c == 'n' ? ConfigJsonValue.Null : new ConfigJsonValue("bool", token);
                }
                int start = _position;
                if (c == '-')
                    _position++;
                if (End)
                    throw new InvalidDataException("JSON 数值无效。");
                if (_text[_position] == '0')
                    _position++;
                else
                {
                    if (_text[_position] < '1' || _text[_position] > '9')
                        throw new InvalidDataException("JSON 数值无效。");
                    Digits();
                }
                if (!End && _text[_position] == '.')
                {
                    _position++;
                    RequireDigits();
                }
                if (!End && (_text[_position] == 'e' || _text[_position] == 'E'))
                {
                    _position++;
                    if (!End && (_text[_position] == '+' || _text[_position] == '-'))
                        _position++;
                    RequireDigits();
                }
                if (_position - start > 1024)
                    throw new InvalidDataException("JSON 数值过长。");
                return ConfigJsonValue.Number(_text.Substring(start, _position - start));
            }
            private void Digits()
            {
                while (!End && _text[_position] >= '0' && _text[_position] <= '9')
                    _position++;
            }
            private void RequireDigits()
            {
                int start = _position;
                Digits();
                if (start == _position)
                    throw new InvalidDataException("JSON 数值缺少数字。");
            }
            private string ReadString()
            {
                Expect('"');
                var value = new StringBuilder();
                while (true)
                {
                    char c = Take();
                    if (c == '"')
                        break;
                    if (c < 32)
                        throw new InvalidDataException("JSON 字符串含未转义控制字符。");
                    if (c == '\\')
                    {
                        c = Take();
                        switch (c)
                        {
                            case '"':
                            case '\\':
                            case '/':
                                value.Append(c);
                                break;
                            case 'b':
                                value.Append('\b');
                                break;
                            case 'f':
                                value.Append('\f');
                                break;
                            case 'n':
                                value.Append('\n');
                                break;
                            case 'r':
                                value.Append('\r');
                                break;
                            case 't':
                                value.Append('\t');
                                break;
                            case 'u':
                                int code = 0;
                                for (int i = 0; i < 4; i++)
                                {
                                    char h = Take();
                                    int n = h >= '0' && h <= '9' ? h - '0' : h >= 'a' && h <= 'f' ? h - 'a' + 10 : h >= 'A' && h <= 'F' ? h - 'A' + 10 : -1;
                                    if (n < 0)
                                        throw new InvalidDataException("JSON Unicode 转义无效。");
                                    code = code * 16 + n;
                                }
                                value.Append((char)code);
                                break;
                            default:
                                throw new InvalidDataException("JSON 转义无效。");
                        }
                    }
                    else
                        value.Append(c);
                    if (_bounds != null && value.Length > _bounds.StringBytes) throw AllocationBounds.Exceeded();
                    if (value.Length > 16 * 1024 * 1024)
                        throw new InvalidDataException("JSON 字符串过长。");
                }
                string result = value.ToString();
                if (_bounds != null && s_utf8.GetByteCount(result) > _bounds.StringBytes) throw AllocationBounds.Exceeded();
                if (s_utf8.GetByteCount(result) > 16 * 1024 * 1024)
                    throw new InvalidDataException("JSON 字符串过长。");
                return result;
            }
        }
    }
}
