using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

internal sealed record TableDefinition(string[] Key, IReadOnlyDictionary<string, string[]> Groups, int HeaderRows = 5)
{
    public static Dictionary<string, TableDefinition> Read(string path)
    {
        return Parse(File.Exists(path) ? File.ReadAllText(path) : null);
    }

    internal static Dictionary<string, TableDefinition> Parse(string? text)
    {
        var result = new Dictionary<string, TableDefinition>(StringComparer.Ordinal);
        if (text == null)
        {
            return result;
        }

        using var document = JsonDocument.Parse(text);
        foreach (JsonProperty table in document.RootElement.EnumerateObject())
        {
            int headerRows = 5;
            var groups = new Dictionary<string, string[]>(StringComparer.Ordinal);
            var properties = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in table.Value.EnumerateObject())
            {
                if (!properties.Add(property.Name))
                {
                    throw new FormatException($"{table.Name}: 重复属性 {property.Name} / Duplicate property.");
                }

                switch (property.Name)
                {
                    case "headerRows":
                        if (property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetInt32(out headerRows) || headerRows != 5)
                        {
                            throw new FormatException($"{table.Name}: 表头固定为 5 行，headerRows 可省略或填写 5；请保留第 5 行所属端，数据从第 6 行开始。 / Exactly 5 header rows are required; omit headerRows or set it to 5. Reserve row 5 for targets and start data in row 6.");
                        }

                        break;
                    case "groups":
                        foreach (JsonProperty group in property.Value.EnumerateObject())
                        {
                            groups.Add(group.Name, ReadFields(group.Value));
                        }

                        break;
                    default:
                        throw new FormatException($"{table.Name}: 未知属性 {property.Name} / Unknown property (headerRows, groups); declare keys in Excel row 3.");
                }
            }

            result.Add(table.Name, new TableDefinition([], groups, headerRows));
        }

        return result;
        static string[] ReadFields(JsonElement value)
        {
            string[] fields = value.EnumerateArray().Select(v => v.GetString() ?? "").ToArray();
            if (fields.Length == 0 || fields.Any(string.IsNullOrWhiteSpace) || fields.Distinct(StringComparer.Ordinal)
                .Count() != fields.Length)
            {
                throw new FormatException("键字段列表不能为空，不能包含空名或重复字段。 / Key fields must be nonempty and distinct.");
            }

            return fields;
        }
    }

    public static Dictionary<string, TableDefinition> FromAssembly(Assembly? assembly, IEnumerable<Type>? rowTypes = null)
    {
        var result = new Dictionary<string, TableDefinition>(StringComparer.Ordinal);
        if (assembly == null)
        {
            return result;
        }

        // 包装编解码器可引用其他程序集中的行类型；布局声明随实际行类型读取。
        foreach (Type type in assembly.GetTypes()
            .Concat(rowTypes ?? [])
            .Distinct()
            .Where(t => typeof(IConfigRow).IsAssignableFrom(t) && t.Name.EndsWith("Config",
            StringComparison.Ordinal)))
        {
            int headerRows = type.GetCustomAttribute<ConfigTableLayoutAttribute>()?.HeaderRows ?? 5;
            var groups = type.GetCustomAttributes<ConfigTableGroupAttribute>()
                .ToDictionary(g => g.Name,
                g => g.Fields,
                StringComparer.Ordinal);
            if (groups.Count != 0)
            {
                result.Add(type.Name[..^6], new TableDefinition([], groups, headerRows));
            }
        }

        return result;
    }

    public TableSchema Apply(TableSchema schema)
    {
        FieldSchema[] key = Resolve(Key);
        var groups = new List<TableGroupSchema>();
        foreach (KeyValuePair<string, string[]> pair in Groups.OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            if (!Regex.IsMatch(pair.Key, "^[A-Za-z_][A-Za-z_0-9]*$") || "Get" + pair.Key == schema.Name + "ConfigTable")
            {
                throw new FormatException($"无效或保留的分组名 / Invalid or reserved group name: {pair.Key}");
            }

            groups.Add(new TableGroupSchema(pair.Key, Resolve(pair.Value)));
        }

        if (key.Length == 0 && groups.Count == 0)
        {
            return schema;
        }

        string identity = JsonSerializer.Serialize(new
        {
            key = Key,
            groups = groups.Select(g => new { g.Name, Fields = g.Fields.Select(f => f.Name) })
        });
        string hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(schema.SchemaHash + "\nIndexes/1\n" + identity)));
        return schema with
        {
            KeyFields = key,
            Groups = groups,
            SchemaHash = hash
        };
        FieldSchema[] Resolve(string[] names) => names.Select(name =>
        {
            FieldSchema field = schema.Fields.FirstOrDefault(f => f.Name == name) ?? throw new FormatException($"{schema.Name}: 键字段不存在 / Unknown key field: {name}");
            if (!CanIndex(field))
            {
                throw new FormatException($"{schema.Name}.{name}: 键仅支持非空标量或枚举 / Keys require non-null scalar or enum fields.");
            }

            return field;
        }).ToArray();
    }

    internal static bool CanIndex(FieldSchema field) => field.Type.Kind is ConfigTypeKind.Primitive or ConfigTypeKind.Enum && field.Layout == null && !field.Json;
}

public sealed record TableGroupSchema(string Name, IReadOnlyList<FieldSchema> Fields);
// 导表侧的可变字段数键；业务运行时由生成器输出强类型 ValueTuple，无 object[] 或装箱。
internal sealed class TableKeyValues(object[] values) : IEquatable<TableKeyValues>
{
    public bool Equals(TableKeyValues? other) => other != null && values.AsSpan().SequenceEqual(other.Values);
    private object[] Values => values;

    public override bool Equals(object? obj) => obj is TableKeyValues other && Equals(other);
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (object value in values)
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }
}

internal sealed class TableKeyTracker(TableSchema schema, DiagnosticCollector collector)
{
    private readonly int[] _indices = schema.KeyFields.Select(f => schema.Fields.ToList().IndexOf(f)).ToArray();
    private readonly HashSet<TableKeyValues> _keys = new();
    public bool Validate(object[] values, int row, ICollection<string>? empty)
    {
        if (_indices.Length == 0)
        {
            return true;
        }

        FieldSchema? missing = schema.KeyFields.FirstOrDefault(f => empty?.Contains(f.Name) == true);
        if (missing != null)
        {
            return Fail("KEY_REQUIRED", missing, "联合键字段不能为空。", "Key fields must not be blank.");
        }

        var key = new TableKeyValues(_indices.Select(i => values[i]).ToArray());
        return _keys.Add(key) || Fail("KEY_DUPLICATE",
            schema.KeyFields[0],
            "联合键重复：" + string.Join(" + ",
            schema.KeyFields.Select(f => f.Name)),
            "Duplicate composite key.");
        bool Fail(string code, FieldSchema field, string chinese, string english)
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                code,
                ConfigText.Get(chinese,
                english),
                schema.FilePath,
                schema.Name,
                schema.SheetName,
                row,
                field.Name,
                field.Column));
            return false;
        }
    }
}
