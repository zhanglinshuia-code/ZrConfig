using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using ZrConfig.Runtime;

namespace ZrConfig.Core;
/// <summary>反射只发生在导表侧；Unity 读取完全由生成代码直接访问成员。</summary>
internal sealed class ConfigValueParser
{
    private readonly Dictionary<ConfigType, Func<string, object?>> _converters = new();
    private readonly Dictionary<ConfigType, Array> _emptyArrays = new();
    private readonly PathPart[] _pathParts = new PathPart[ConfigLimits.MaxDepth + 4];
    public object? Parse(string text, ConfigType type, bool json, string field)
    {
        ConfigPath path = new(field, _pathParts);
        if (text.Trim().Length == 0)
        {
            return Default(type, path, 0);
        }

        return ParseText(text, type, json, path, 0);
    }

    private object? ParseText(string text, ConfigType type, bool json, ConfigPath path, int depth)
    {
        CheckDepth(depth, path);
        try
        {
            if (type.Kind == ConfigTypeKind.Nullable)
            {
                return text.Trim() == "null" ? null : ParseText(text, type.Element!, json, path, depth + 1);
            }

            if (json || type.Kind is ConfigTypeKind.Object or ConfigTypeKind.RawJson)
            {
                using var document = JsonDocument.Parse(text, new JsonDocumentOptions { MaxDepth = ConfigLimits.MaxDepth });
                return ParseJson(document.RootElement, type, path, depth);
            }

            if (type.Kind == ConfigTypeKind.Primitive && type.ClrType == typeof(string))
            {
                return text.TrimStart().StartsWith('"') ? JsonSerializer.Deserialize<string>(text) ?? "" : text;
            }

            string trimmed = text.Trim();
            if (type.Kind == ConfigTypeKind.Dictionary)
            {
                if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                {
                    trimmed = trimmed[1..^1].Trim();
                }

                var dictionary = (IDictionary)Activator.CreateInstance(type.ClrType)!;
                if (trimmed.Length == 0)
                {
                    return dictionary;
                }

                List<Range> entries = Split(trimmed, ';', path);
                if (entries.Count > ConfigLimits.MaxArrayLength)
                {
                    throw Error(path, "字典过长。");
                }

                for (int i = 0; i < entries.Count; i++)
                {
                    string entry = trimmed[entries[i]];
                    List<Range> pair = Split(entry, '=', path.Index(i));
                    if (pair.Count != 2 || pair.Any(p => string.IsNullOrWhiteSpace(entry[p])))
                    {
                        throw Error(path.Index(i), "字典项使用 键=值；空字符串请填写 \"\"。");
                    }

                    object key = ParseText(entry[pair[0]].Trim(), type.Key!, false, path.Index(i).Member("Key"), depth + 1)!;
                    if (dictionary.Contains(key))
                    {
                        throw Error(path.Index(i).Member("Key"), $"字典键 {key} 重复。");
                    }

                    dictionary.Add(key, ParseText(entry[pair[1]].Trim(), type.Element!, false, path.Index(i).Member("Value"), depth + 1));
                }

                return dictionary;
            }

            if (type.Kind == ConfigTypeKind.Array)
            {
                if (type.ClrType.GetArrayRank() > 1)
                {
                    return ParseMatrixText(trimmed, type, path, depth);
                }

                if (trimmed.StartsWith('['))
                {
                    if (!trimmed.EndsWith(']'))
                    {
                        throw Error(path, "数组缺少结束符 ]。");
                    }

                    trimmed = trimmed[1..^1];
                }
                else if (type.Element!.Kind == ConfigTypeKind.Array)
                {
                    throw Error(path, "多维数组需要用 [] 包围每一层。");
                }

                if (trimmed.Trim().Length == 0)
                {
                    return Empty(type);
                }

                List<Range> parts = Split(trimmed, ';', path);
                if (parts.Count > ConfigLimits.MaxArrayLength)
                {
                    throw Error(path, "数组过长。");
                }

                var values = Array.CreateInstance(type.Element!.ClrType, parts.Count);
                for (int i = 0; i < parts.Count; i++)
                {
                    string part = trimmed[parts[i]].Trim();
                    if (part.Length == 0)
                    {
                        throw Error(path.Index(i), "数组元素不能为空，请用 [] 表示空数组、\"\" 表示空字符串。");
                    }

                    if (type.Element.Kind == ConfigTypeKind.Array && !part.StartsWith('['))
                    {
                        throw Error(path.Index(i), "嵌套数组必须使用 []。");
                    }

                    values.SetValue(ParseText(part, type.Element, false, path.Index(i), depth + 1), i);
                }

                return values;
            }

            if (type.Kind is ConfigTypeKind.Kv or ConfigTypeKind.Vector2 or ConfigTypeKind.Vector3)
            {
                List<Range> parts = Split(trimmed, ',', path);
                int count = type.Kind == ConfigTypeKind.Vector3 ? 3 : 2;
                if (parts.Count != count)
                {
                    throw Error(path, $"需要 {count} 个逗号分隔的数值。");
                }

                object[] values = new object[count];
                for (int i = 0; i < count; i++)
                {
                    values[i] = type.Kind == ConfigTypeKind.Kv ? (object)int.Parse(trimmed[parts[i]],
                        CultureInfo.InvariantCulture) : Finite(float.Parse(trimmed[parts[i]],
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture));
                }

                return Activator.CreateInstance(type.ClrType, values)!;
            }

            if (type.Kind == ConfigTypeKind.Converter)
            {
                if (!_converters.TryGetValue(type, out Func<string, object?>? parse))
                {
                    // 泛型桥接只构建一次；每个单元格直接调用委托，不产生反射参数数组。
                    MethodInfo factory = typeof(ConfigValueParser).GetMethod(nameof(BindConverter),
                        BindingFlags.Static | BindingFlags.NonPublic)!;
                    parse = (Func<string, object?>)factory.MakeGenericMethod(type.ClrType)
                        .Invoke(null,
                        [Activator.CreateInstance(type.Converter!)])!;
                    _converters.Add(type, parse);
                }

                return parse(text);
            }

            if (type.Kind == ConfigTypeKind.Enum)
            {
                if (!type.EnumMembers.TryGetValue(trimmed, out object? value))
                {
                    throw Error(path, $"{trimmed} 不是 {type.ClrType.Name} 的枚举成员名。");
                }

                return value;
            }

            Type clr = type.ClrType;
            if (clr == typeof(bool))
            {
                return trimmed switch
                {
                    "1" => true,
                    "0" => false,
                    _ => bool.Parse(trimmed)
                };
            }

            if (clr == typeof(float))
            {
                return Finite(float.Parse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture));
            }

            if (clr == typeof(double))
            {
                return Finite(double.Parse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture));
            }

            return Convert.ChangeType(trimmed, clr, CultureInfo.InvariantCulture);
        }
        catch (TargetInvocationException ex)
        {
            throw Error(path, ex.InnerException?.Message ?? ex.Message);
        }
        catch (Exception ex) when (ex is not ConfigValueException)
        {
            throw Error(path, ex.Message);
        }
    }

    private object? ParseJson(JsonElement value, ConfigType type, ConfigPath path, int depth)
    {
        CheckDepth(depth, path);
        if (type.Kind == ConfigTypeKind.RawJson)
        {
            return Activator.CreateInstance(type.ClrType, value.GetRawText());
        }

        if (type.Kind == ConfigTypeKind.Nullable)
        {
            return value.ValueKind == JsonValueKind.Null ? null : ParseJson(value, type.Element!, path, depth + 1);
        }

        if (value.ValueKind == JsonValueKind.Null)
        {
            throw Error(path, "不支持显式 null；空数组请填写 []。");
        }

        if (type.Kind == ConfigTypeKind.Dictionary)
        {
            if (value.ValueKind != JsonValueKind.Object)
            {
                throw Error(path, "字典需要 JSON 对象。");
            }

            var dictionary = (IDictionary)Activator.CreateInstance(type.ClrType)!;
            foreach (JsonProperty item in value.EnumerateObject())
            {
                if (dictionary.Count >= ConfigLimits.MaxArrayLength)
                {
                    throw Error(path, "字典过长。");
                }

                object key = type.Key!.ClrType == typeof(string) ? item.Name : ParseText(item.Name,
                    type.Key,
                    false,
                    path.Member(item.Name),
                    depth + 1)!;
                if (dictionary.Contains(key))
                {
                    throw Error(path.Member(item.Name), "字典键重复。");
                }

                dictionary.Add(key, ParseJson(item.Value, type.Element!, path.Member(item.Name), depth + 1));
            }

            return dictionary;
        }

        if (type.Kind == ConfigTypeKind.Array)
        {
            if (type.ClrType.GetArrayRank() > 1)
            {
                return ParseMatrixJson(value, type, path, depth);
            }

            if (value.ValueKind != JsonValueKind.Array)
            {
                throw Error(path, "需要 JSON 数组。");
            }

            int count = value.GetArrayLength();
            if (count > ConfigLimits.MaxArrayLength)
            {
                throw Error(path, "数组过长。");
            }

            if (count == 0)
            {
                return Empty(type);
            }

            var array = Array.CreateInstance(type.Element!.ClrType, count);
            int index = 0;
            foreach (JsonElement item in value.EnumerateArray())
            {
                array.SetValue(ParseJson(item, type.Element, path.Index(index), depth + 1), index);
                index++;
            }

            return array;
        }

        if (type.Kind == ConfigTypeKind.Object)
        {
            if (value.ValueKind != JsonValueKind.Object)
            {
                throw Error(path, "需要 JSON 对象。");
            }

            ConfigType concrete = type;
            if (type.IsPolymorphic)
            {
                if (!value.TryGetProperty("$type", out JsonElement tag) || tag.ValueKind != JsonValueKind.String)
                {
                    throw Error(path.Member("$type"), "多态对象必须填写具体类型名。");
                }

                string name = tag.GetString()!;
                if (!type.PolymorphicNames.TryGetValue(name, out ConfigType? variant) || variant == null)
                {
                    throw Error(path.Member("$type"), $"类型 {name} 未知或有歧义，请填写合法子类的完整名称。");
                }

                concrete = variant;
            }

            object instance = Activator.CreateInstance(concrete.ClrType)!;
            HashSet<string> seen = new(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                string name = property.Name;
                if (!seen.Add(name))
                {
                    throw Error(path.Member(name), "字段重复。");
                }

                if (name == "$type" && type.IsPolymorphic)
                {
                    continue;
                }

                if (!concrete.MemberMap.TryGetValue(name, out ConfigMember? member))
                {
                    throw Error(path.Member(name), $"{concrete.ClrType.Name} 不包含此字段。");
                }

                object? parsed = ParseJson(property.Value, member.Type, path.Member(member.Name), depth + 1);
                if (parsed == null && member.Required)
                {
                    throw Error(path.Member(member.Name), "必填字段不能为 null。");
                }

                member.Set(instance, parsed);
            }

            foreach (ConfigMember member in concrete.Members)
            {
                if (seen.Contains(member.Name))
                {
                    continue;
                }

                if (member.Required)
                {
                    throw Error(path.Member(member.Name), "必填字段缺失。");
                }

                object? initial = member.Get(instance);
                if (initial == null && (member.Type.Kind is ConfigTypeKind.Array or ConfigTypeKind.Dictionary || member.Type.ClrType == typeof(string)))
                {
                    member.Set(instance, Default(member.Type, path.Member(member.Name), depth + 1));
                }
            }

            return instance;
        }

        if (type.Kind is ConfigTypeKind.Kv or ConfigTypeKind.Vector2 or ConfigTypeKind.Vector3)
        {
            if (value.ValueKind != JsonValueKind.Array)
            {
                throw Error(path, "kv/vector 的 JSON 值使用数值数组，如 [1,2]。");
            }

            return ParseText(string.Join(",", value.EnumerateArray().Select(v => v.GetRawText())), type, false, path, depth);
        }

        if (type.ClrType == typeof(string) && value.ValueKind != JsonValueKind.String)
        {
            throw Error(path, "需要 JSON 字符串。");
        }

        if (type.ClrType == typeof(bool) && value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw Error(path, "需要 JSON 布尔值 true/false。");
        }

        if (type.Kind == ConfigTypeKind.Enum && value.ValueKind != JsonValueKind.String)
        {
            throw Error(path, "枚举使用成员名字符串。");
        }

        if (type.Kind == ConfigTypeKind.Primitive && type.ClrType != typeof(string) && value.ValueKind == JsonValueKind.String)
        {
            throw Error(path, "数值/布尔字段不能填写为 JSON 字符串。");
        }

        if (type.ClrType == typeof(string))
        {
            return value.GetString()!;
        }

        return ParseText(value.ValueKind == JsonValueKind.String ? value.GetString()! : value.GetRawText(), type, false, path, depth);
    }

    private object? Default(ConfigType type, ConfigPath path, int depth)
    {
        CheckDepth(depth, path);
        if (type.Kind == ConfigTypeKind.Nullable)
        {
            return null;
        }

        if (type.Kind == ConfigTypeKind.Dictionary)
        {
            return Activator.CreateInstance(type.ClrType);
        }

        if (type.Kind == ConfigTypeKind.Array)
        {
            return Empty(type);
        }

        if (type.ClrType == typeof(string))
        {
            return string.Empty;
        }

        if (type.Kind == ConfigTypeKind.RawJson)
        {
            return Activator.CreateInstance(type.ClrType, "null");
        }

        if (type.Kind == ConfigTypeKind.Object)
        {
            if (type.ClrType.IsAbstract || type.IsPolymorphic)
            {
                throw Error(path, "多态对象不能为空，请填写包含 $type 的 JSON。");
            }

            using var empty = JsonDocument.Parse("{}");
            return ParseJson(empty.RootElement, type, path, depth);
        }

        if (type.Kind == ConfigTypeKind.Converter)
        {
            return ParseText("", type, false, path, depth);
        }

        object value = Activator.CreateInstance(type.ClrType)!;
        if (type.Kind == ConfigTypeKind.Enum && !Enum.IsDefined(type.ClrType, value))
        {
            throw Error(path, "枚举没有值为 0 的成员，请显式填写成员名。");
        }

        return value;
    }

    private Array Empty(ConfigType type)
    {
        if (!_emptyArrays.TryGetValue(type, out Array? array))
        {
            _emptyArrays.Add(type, array = Array.CreateInstance(type.Element!.ClrType, new int[type.ClrType.GetArrayRank()]));
        }

        return array;
    }

    private object ParseMatrixText(string text,
        ConfigType type,
        ConfigPath path,
        int depth) => ParseMatrix(text,
        type,
        path,
        depth,
        (input,
        p) =>
    {
        input = input.Trim();
        if (!input.StartsWith('[') || !input.EndsWith(']'))
        {
            throw Error(p, "矩形数组的每一维都必须使用 []。");
        }

        input = input[1..^1];
        return input.Trim().Length == 0 ? [] : Split(input, ';', p).Select(r => input[r].Trim()).ToArray();
    }, (input, p, d) => input.Length == 0 ? throw Error(p, "矩阵元素不能为空。") : ParseText(input, type.Element!, false, p, d));
    private object ParseMatrixJson(JsonElement value,
        ConfigType type,
        ConfigPath path,
        int depth) => ParseMatrix(value,
        type,
        path,
        depth,
        (input,
        p) =>
    {
        if (input.ValueKind != JsonValueKind.Array)
        {
            throw Error(p, "矩形数组的每一维都需要 JSON 数组。");
        }

        return input.EnumerateArray().ToArray();
    }, (input, p, d) => ParseJson(input, type.Element!, p, d));
    private static Array ParseMatrix<T>(T input,
        ConfigType type,
        ConfigPath path,
        int depth,
        Func<T, ConfigPath, T[]> split,
        Func<T, ConfigPath, int, object?> parse)
    {
        int rank = type.ClrType.GetArrayRank();
        int[] lengths = Enumerable.Repeat(-1, rank).ToArray();
        var values = new List<object?>();
        Visit(input, 0, path);
        for (int i = 0; i < rank; i++)
        {
            if (lengths[i] < 0)
            {
                lengths[i] = 0;
            }
        }

        var result = Array.CreateInstance(type.Element!.ClrType, lengths);
        int[] indices = new int[rank];
        for (int i = 0; i < values.Count; i++)
        {
            result.SetValue(values[i], indices);
            for (int dimension = rank - 1; dimension >= 0; dimension--)
            {
                if (++indices[dimension] < lengths[dimension])
                {
                    break;
                }
                else
                {
                    indices[dimension] = 0;
                }
            }
        }

        return result;
        void Visit(T part, int dimension, ConfigPath current)
        {
            CheckDepth(depth + dimension, current);
            T[] parts = split(part, current);
            if (parts.Length > ConfigLimits.MaxArrayLength)
            {
                throw Error(current, "矩阵维度过长。");
            }

            if (lengths[dimension] >= 0 && lengths[dimension] != parts.Length)
            {
                throw Error(current, "矩形数组各行/层长度必须一致；不等长数据请使用交错数组 T[][]。");
            }

            lengths[dimension] = parts.Length;
            for (int i = 0; i < parts.Length; i++)
            {
                if (dimension < rank - 1)
                {
                    Visit(parts[i], dimension + 1, current.Index(i));
                }
                else
                {
                    if (values.Count >= ConfigLimits.MaxArrayLength)
                    {
                        throw Error(current, "矩阵总元素数超过上限。");
                    }

                    values.Add(parse(parts[i], current.Index(i), depth + rank));
                }
            }
        }
    }

    private static Func<string, object?> BindConverter<T>(IConfigConverter<T> converter) => text => converter.Parse(text);
    private static List<Range> Split(string text, char separator, ConfigPath path)
    {
        List<Range> result = new();
        var stack = new Stack<char>();
        bool quoted = false;
        bool escape = false;
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (quoted)
            {
                if (escape)
                {
                    escape = false;
                }
                else if (c == '\\')
                {
                    escape = true;
                }
                else if (c == '"')
                {
                    quoted = false;
                }

                continue;
            }

            if (c == '"')
            {
                quoted = true;
            }
            else if (c is '[' or '{')
            {
                stack.Push(c);
                if (stack.Count > ConfigLimits.MaxDepth)
                {
                    throw Error(path, "嵌套过深。");
                }
            }
            else if (c is ']' or '}')
            {
                if (!stack.TryPop(out char open) || (c == ']' ? open != '[' : open != '{'))
                {
                    throw Error(path, "括号不匹配。");
                }
            }
            else if (c == separator && stack.Count == 0)
            {
                result.Add(start..i);
                start = i + 1;
            }
        }

        if (quoted || stack.Count != 0)
        {
            throw Error(path, "引号或括号未闭合。");
        }

        result.Add(start..text.Length);
        return result;
    }

    // 路径段复用固定缓冲，只在失败时拼接 Actions[1].BuffId 等文本。
    private readonly record struct PathPart(string? Name, int Index);
    private readonly struct ConfigPath(string root, PathPart[] parts, int count = 0)
    {
        public ConfigPath Index(int index)
        {
            if (count >= parts.Length)
            {
                throw Error(this, "嵌套过深。");
            }

            parts[count] = new(null, index);
            return new(root, parts, count + 1);
        }

        public ConfigPath Member(string name)
        {
            if (count >= parts.Length)
            {
                throw Error(this, "嵌套过深。");
            }

            parts[count] = new(name, 0);
            return new(root, parts, count + 1);
        }

        public override string ToString()
        {
            var text = new StringBuilder(root);
            for (int i = 0; i < count; i++)
            {
                if (parts[i].Name != null)
                {
                    text.Append('.').Append(parts[i].Name);
                }
                else
                {
                    text.Append('[').Append(parts[i].Index).Append(']');
                }
            }

            return text.ToString();
        }
    }

    private static float Finite(float value) => float.IsFinite(value) ? value : throw new FormatException("浮点数必须有限。");
    private static double Finite(double value) => double.IsFinite(value) ? value : throw new FormatException("浮点数必须有限。");
    private static void CheckDepth(int depth, ConfigPath path)
    {
        if (depth > ConfigLimits.MaxDepth)
        {
            throw Error(path, "嵌套超过 64 层。");
        }
    }

    private static ConfigValueException Error(ConfigPath path, string message) => new($"{path}: {message}");
    private sealed class ConfigValueException(string message) : FormatException(message);
}
