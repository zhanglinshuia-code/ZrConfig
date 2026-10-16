using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using ZrConfig.Runtime;

namespace ZrConfig.Core;

// Read-only projections retain their snapshot and never copy collections on a query.
// Legacy model types remain available through an explicitly named Borrow boundary.
internal static class ServerValueGenerator
{
    private const string Snapshot = "global::ZrConfig.Runtime.Server.Snapshot";
    private static bool Wrapped(ConfigType type) => type.Kind is ConfigTypeKind.Array or ConfigTypeKind.Dictionary or
        ConfigTypeKind.Object or ConfigTypeKind.Converter;
    internal static string Name(ConfigType type)
    {
        string identity = type.CsName.Replace("global::", "", StringComparison.Ordinal);
        string readable = Regex.Replace(identity, "[^A-Za-z0-9_]", "_");
        if (readable.Length > 80) readable = readable[..80];
        return "Value_" + readable + "_" + GeneratedApiContracts.Hash(identity)[..12];
    }
    internal static string Type(ConfigType type) => type.Kind == ConfigTypeKind.Nullable ? Type(type.Element!) + "?" :
        Wrapped(type) ? FullName(type) + (NullableObject(type) ? "?" : "") : type.CsName;
    private static string FullName(ConfigType type) => "global::ZrConfig.Models.Server." + Name(type);
    private static bool NullableObject(ConfigType type) => type.IsReference && type.Kind is ConfigTypeKind.Object or ConfigTypeKind.Converter;
    internal static string Project(ConfigType type, string value, string owner) => type.Kind == ConfigTypeKind.Nullable
        ? $"({value}.HasValue ? ({Type(type.Element!)}?){Project(type.Element!, value + ".GetValueOrDefault()", owner)} : null)"
        : Wrapped(type) ? NullableObject(type) ? $"({value} == null ? ({FullName(type)}?)null : new {FullName(type)}({value}, {owner}))" : $"new {FullName(type)}({value}, {owner})" : value;

    internal static IReadOnlyList<(ConfigType Type, ConfigMember[] Members)> Shapes(IEnumerable<TableSchema> schemas)
    {
        var pending=new Queue<ConfigType>(ConfigTypeRegistry.Graph(schemas.SelectMany(s=>s.Fields).Select(f=>f.Type)));
        var found=new Dictionary<string,(ConfigType Type,ConfigMember[] Members)>(StringComparer.Ordinal);
        while(pending.TryDequeue(out ConfigType? type))
        {
            if(found.ContainsKey(type.CsName)) continue;
            ConfigMember[] members=type.Members;
            if(type.Kind==ConfigTypeKind.Converter)
            {
                ConfigWireLayoutAttribute? layout=type.ClrType.GetCustomAttribute<ConfigWireLayoutAttribute>() ?? type.Converter?.GetCustomAttribute<ConfigWireLayoutAttribute>();
                if(layout!=null)
                {
                    var registry=new ConfigTypeRegistry(type.ClrType.Assembly);
                    members=layout.Members.Select(name=> {
                        MemberInfo? member=type.ClrType.GetField(name,BindingFlags.Public|BindingFlags.Instance) ?? (MemberInfo?)type.ClrType.GetProperty(name,BindingFlags.Public|BindingFlags.Instance);
                        Type clr=member switch { FieldInfo field=>field.FieldType, PropertyInfo property when property.GetMethod?.IsPublic==true && property.GetIndexParameters().Length==0 => property.PropertyType, _=>throw new NotSupportedException("转换器只读视图缺少公开编码成员："+type.CsName+"."+name) };
                        return new ConfigMember(name,registry.Resolve(clr),member!,false);
                    }).ToArray();
                }
            }
            found.Add(type.CsName,(type,members));
            foreach(ConfigType dependency in ConfigTypeRegistry.Graph(members.Select(m=>m.Type))) pending.Enqueue(dependency);
        }
        return found.Values.OrderBy(t=>t.Type.CsName,StringComparer.Ordinal).ToArray();
    }

    internal static void Append(StringBuilder sb, IEnumerable<TableSchema> schemas)
    {
        sb.AppendLine("namespace ZrConfig.Models.Server {");
        foreach ((ConfigType type, ConfigMember[] members) in Shapes(schemas).Where(t=>Wrapped(t.Type)))
        {
            string name = Name(type);
            sb.AppendLine($"public readonly struct {name} {{")
                .AppendLine($"private readonly {type.CsName} _value;")
                .AppendLine($"private readonly {Snapshot} _owner;")
                .AppendLine($"internal {name}({type.CsName} value, {Snapshot} owner) {{ _value = value; _owner = owner; }}")
                .AppendLine($"public {Snapshot} Snapshot => _owner;")
                .AppendLine($"public {type.CsName} Borrow() => _value;");
            switch (type.Kind)
            {
                case ConfigTypeKind.Array:
                    int rank = type.ClrType.GetArrayRank();
                    string dimensions = string.Join(", ", Enumerable.Range(0, rank).Select(i => "int index" + i));
                    string indices = string.Join(", ", Enumerable.Range(0, rank).Select(i => "index" + i));
                    sb.AppendLine("public int Count => _value.Length;")
                        .AppendLine($"public int Rank => {rank};")
                        .AppendLine("public int GetLength(int dimension) => _value.GetLength(dimension);")
                        .AppendLine($"public {Type(type.Element!)} this[{dimensions}] => {Project(type.Element!, "_value[" + indices + "]", "_owner")};");
                    if (rank == 1)
                        sb.AppendLine("public Enumerator GetEnumerator() => new Enumerator(this);")
                            .AppendLine($"public struct Enumerator {{ private readonly {name} _view; private int _index; internal Enumerator({name} view) {{ _view = view; _index = -1; }} public bool MoveNext() => ++_index < _view.Count; public {Type(type.Element!)} Current => _view[_index]; }}");
                    break;
                case ConfigTypeKind.Dictionary:
                    string key = Type(type.Key!), value = Type(type.Element!);
                    sb.AppendLine("public int Count => _value.Count;")
                        .AppendLine($"public bool ContainsKey({key} key) => _value.ContainsKey(key);")
                        .AppendLine($"public {value} this[{key} key] => {Project(type.Element!, "_value[key]", "_owner")};")
                        .AppendLine($"public bool TryGetValue({key} key, out {value} value) {{ if (_value.TryGetValue(key, out var source)) {{ value = {Project(type.Element!, "source", "_owner")}; return true; }} value = default!; return false; }}")
                        .AppendLine("public Enumerator GetEnumerator() => new Enumerator(_value.GetEnumerator(), _owner);")
                        .AppendLine($"public struct Enumerator {{ private {type.CsName}.Enumerator _iterator; private readonly {Snapshot} _owner; internal Enumerator({type.CsName}.Enumerator iterator, {Snapshot} owner) {{ _iterator = iterator; _owner = owner; }} public bool MoveNext() => _iterator.MoveNext(); public global::System.Collections.Generic.KeyValuePair<{key}, {value}> Current {{ get {{ var item = _iterator.Current; return new global::System.Collections.Generic.KeyValuePair<{key}, {value}>(item.Key, {Project(type.Element!, "item.Value", "_owner")}); }} }} public void Dispose() => _iterator.Dispose(); }}");
                    break;
                default:
                    foreach (ConfigMember member in members)
                        sb.AppendLine($"public {Type(member.Type)} @{member.Name} => {Project(member.Type, "_value.@" + member.Name, "_owner")};");
                    foreach (ConfigType variant in type.Variants)
                        sb.AppendLine($"public {FullName(variant)}? As_{Name(variant)}() => _value is {variant.CsName} value ? ({FullName(variant)}?)new {FullName(variant)}(value, _owner) : null;");
                    break;
            }
            sb.AppendLine("}");
        }
        sb.AppendLine("}");
    }
}
