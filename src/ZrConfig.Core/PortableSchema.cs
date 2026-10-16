using System.Globalization;
using System.Reflection;
using System.Text.Json;
using ZrConfig.Runtime;

namespace ZrConfig.Core;

// 生成器边界仅传递数据描述，不能将 CLR 类型、程序集或反射成员带入语言后端。
internal sealed record PortableField(string Name, string Type, string Description = "");
internal sealed record PortableGroup(string Name, string[] Fields);
internal sealed record PortableType(string Id, string Kind, string Primitive, bool NullableObject,
    int Rank, string? Element, string? Key, PortableField[] Fields, string[] Variants,
    string[] EnumValues, bool Abstract, string? BaseType, Dictionary<string, string> EnumMembers)
{
    public string? ReferenceTable { get; init; }
}
internal sealed record PortableTable(string Name, string Hash, bool HasId, PortableField[] Fields,
    string[] Keys, PortableGroup[] Groups, PortableType[] Types, string[] Tags)
{
    public string[] Dependencies => Types.Where(t => t.Kind == "tablereference").Select(t => t.ReferenceTable ??
        throw new InvalidDataException("表引用缺少目标身份：" + t.Id)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    internal string Serialize() => JsonSerializer.Serialize(this, PortableSchema.JsonOptions);
}

internal static class PortableSchema
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    internal static PortableTable Create(TableSchema schema)
    {
        var types = new Dictionary<string, PortableType>(StringComparer.Ordinal);
        var registry = new ConfigTypeRegistry(null);
        string Visit(ConfigType type)
        {
            string id = ConfigTypeRegistry.TypeIdentity(type);
            if (types.ContainsKey(id))
                return id;
            types.Add(id, null!); // 递归结构先占位，成员展开不会无限递归。
            string primitive = type.Kind == ConfigTypeKind.Enum ? Primitive(Enum.GetUnderlyingType(type.ClrType)) :
                type.Kind == ConfigTypeKind.TableReference ? "int32" : Primitive(type.ClrType);
            PortableField[] fields = type.Members.Select(m => new PortableField(m.Name, Visit(m.Type))).ToArray();
            if (type.Kind == ConfigTypeKind.Converter)
            {
                var layout = type.ClrType.GetCustomAttribute<ConfigWireLayoutAttribute>() ??
                    type.Converter?.GetCustomAttribute<ConfigWireLayoutAttribute>();
                if (layout == null || layout.Members.Length == 0 || layout.Members.Distinct(StringComparer.Ordinal).Count() != layout.Members.Length)
                    throw new NotSupportedException($"{id}: JSON/跨语言生成需要 ConfigWireLayout 声明转换器的标准编码成员。任意自定义二进制不能自动翻译。");
                fields = layout.Members.Select(name =>
                {
                    MemberInfo? member = type.ClrType.GetField(name, BindingFlags.Public | BindingFlags.Instance) ??
                        (MemberInfo?)type.ClrType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                    Type memberType = member switch
                    {
                        FieldInfo f => f.FieldType,
                        PropertyInfo p when p.GetMethod?.IsPublic == true && p.GetIndexParameters().Length == 0 => p.PropertyType,
                        _ => throw new NotSupportedException($"{id}.{name}: 编码成员必须是公开可读字段或属性。")
                    };
                    return new PortableField(name, Visit(new ConfigTypeRegistry(type.ClrType.Assembly).Resolve(memberType)));
                }).ToArray();
            }

            if (type.Kind is ConfigTypeKind.Kv or ConfigTypeKind.Vector2 or ConfigTypeKind.Vector3)
            {
                string component = Visit(registry.Resolve(type.Kind == ConfigTypeKind.Kv ? typeof(int) : typeof(float)));
                string[] names = type.Kind == ConfigTypeKind.Kv ? ["Key", "Value"] : type.Kind == ConfigTypeKind.Vector2 ? ["X", "Y"] : ["X", "Y", "Z"];
                fields = names.Select(n => new PortableField(n, component)).ToArray();
            }

            string[] variants = type.IsPolymorphic ? type.Variants.Prepend(type).Where(t => !t.ClrType.IsAbstract).Select(Visit).Distinct().Order(StringComparer.Ordinal).ToArray() : [];
            types[id] = new(id, type.Kind.ToString().ToLowerInvariant(), primitive,
                type.Kind == ConfigTypeKind.Object && type.IsReference, type.Kind == ConfigTypeKind.Array ? type.ClrType.GetArrayRank() : 0,
                type.Element == null ? null : Visit(type.Element), type.Key == null ? null : Visit(type.Key), fields, variants,
                type.Kind == ConfigTypeKind.Enum ? type.EnumMembers.Values.Select(v => Convert.ToString(Convert.ChangeType(v, Enum.GetUnderlyingType(type.ClrType), CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)!).Distinct().ToArray() : [],
                type.ClrType.IsAbstract,
                type.Kind == ConfigTypeKind.Object && type.ClrType.BaseType is { } parent && parent != typeof(object) && parent != typeof(ValueType) ? Visit(new ConfigTypeRegistry(type.ClrType.Assembly).Resolve(parent)) : null,
                type.Kind == ConfigTypeKind.Enum ? type.EnumMembers.ToDictionary(p => p.Key, p => Convert.ToString(Convert.ChangeType(p.Value, Enum.GetUnderlyingType(type.ClrType), CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)!, StringComparer.Ordinal) : new())
                { ReferenceTable = type.Kind == ConfigTypeKind.TableReference ? type.ReferenceTable : null };
            return id;
        }

        PortableField[] fields = schema.Fields.Select(f => new PortableField(f.Name, Visit(f.Type), f.Description)).ToArray();
        return new(schema.Name, schema.SchemaHash, schema.HasIntegerId, fields, schema.KeyFields.Select(f => f.Name).ToArray(),
            schema.Groups.Select(g => new PortableGroup(g.Name, g.Fields.Select(f => f.Name).ToArray())).ToArray(),
            types.Values.OrderBy(t => t.Id, StringComparer.Ordinal).ToArray(),
            ConfigTypeRegistry.WireTags(schema.Fields.Select(f => f.Type)).Select(t => t.ClrType.FullName!).ToArray());
    }

    private static string Primitive(Type type) => Type.GetTypeCode(type) switch
    {
        TypeCode.SByte => "int8",
        TypeCode.Byte => "uint8",
        TypeCode.Int16 => "int16",
        TypeCode.UInt16 => "uint16",
        TypeCode.Int32 => "int32",
        TypeCode.UInt32 => "uint32",
        TypeCode.Int64 => "int64",
        TypeCode.UInt64 => "uint64",
        TypeCode.Single => "float32",
        TypeCode.Double => "float64",
        TypeCode.Decimal => "decimal",
        TypeCode.Boolean => "bool",
        TypeCode.String => "string",
        _ => ""
    };
}
