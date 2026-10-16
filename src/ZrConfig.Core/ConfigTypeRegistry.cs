using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using ZrConfig.Runtime;

namespace ZrConfig.Core;

internal enum ConfigTypeKind
{
    Primitive,
    Enum,
    Array,
    Kv,
    Vector2,
    Vector3,
    RawJson,
    Object,
    Converter,
    Nullable,
    Dictionary,
    TableReference
}

internal sealed class ConfigType(Type clrType, ConfigTypeKind kind)
{
    public Type ClrType { get; } = clrType;
    public ConfigTypeKind Kind { get; } = kind;
    public string CsName { get; } = ConfigTypeRegistry.CsName(clrType);
    public ConfigType? Element
    {
        get; set;
    }
    public ConfigType? Key
    {
        get; set;
    }
    public ConfigMember[] Members { get; set; } = [];
    public IReadOnlyDictionary<string, ConfigMember> MemberMap { get; set; } = new Dictionary<string, ConfigMember>(StringComparer.Ordinal);
    public ConfigType[] Variants { get; set; } = [];
    public IReadOnlyDictionary<string, object> EnumMembers { get; set; } = new Dictionary<string, object>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, ConfigType?> PolymorphicNames { get; set; } = new Dictionary<string, ConfigType?>(StringComparer.Ordinal);
    public Type? Converter
    {
        get; set;
    }
    public string ConverterVersion { get; set; } = "";
    public string? ReferenceTable
    {
        get; init;
    }
    public bool IsPolymorphic => Kind == ConfigTypeKind.Object && Variants.Length != 0;
    public bool IsReference => !ClrType.IsValueType;
}

internal sealed record ConfigMember(string Name, ConfigType Type, MemberInfo Member, bool Required)
{
    public void Set(object target, object? value)
    {
        if (Member is FieldInfo field)
        {
            field.SetValue(target, value);
        }
        else
        {
            ((PropertyInfo)Member).SetValue(target, value);
        }
    }

    public object? Get(object target) => Member is FieldInfo field ? field.GetValue(target) : ((PropertyInfo)Member).GetValue(target);
}

/// <summary>仅在一次生成/导出期间持有类型，避免静态缓存阻止外部程序集卸载。</summary>
internal sealed class ConfigTypeRegistry
{
    private static readonly Dictionary<string, Type> s_primitives = new(StringComparer.OrdinalIgnoreCase)
    {
        ["byte"] = typeof(byte),
        ["short"] = typeof(short),
        ["int"] = typeof(int),
        ["long"] = typeof(long),
        ["float"] = typeof(float),
        ["double"] = typeof(double),
        ["bool"] = typeof(bool),
        ["string"] = typeof(string),
        ["sbyte"] = typeof(sbyte),
        ["ushort"] = typeof(ushort),
        ["uint"] = typeof(uint),
        ["ulong"] = typeof(ulong),
        ["decimal"] = typeof(decimal)
    };
    private static readonly Dictionary<string, ConfigTypeKind> s_builtins = new(StringComparer.Ordinal)
    {
        ["ZrConfig.Types.Kv"] = ConfigTypeKind.Kv,
        ["ZrConfig.Types.Vector2"] = ConfigTypeKind.Vector2,
        ["ZrConfig.Types.Vector3"] = ConfigTypeKind.Vector3,
        ["ZrConfig.Types.RawJson"] = ConfigTypeKind.RawJson
    };
    private readonly Type[] _externalTypes;
    private readonly Dictionary<Type, ConfigType> _resolved = new();
    private readonly Dictionary<string, ConfigType> _references = new(StringComparer.Ordinal);
    public int BinaryVersion
    {
        get;
    }
    internal GenerationSourceContract SourceContract { get; private set; } = GenerationSourceContract.Empty;
    internal Dictionary<string, TableDefinition> TableDefinitions
    {
        get;
    }

    public ConfigTypeRegistry(Assembly? assembly, IEnumerable<Type>? rowTypes = null)
    {
        _externalTypes = assembly?.GetTypes() ?? [];
        TableDefinitions = TableDefinition.FromAssembly(assembly, rowTypes);
        SourceContract = GenerationSourceContract.FromAssembly(assembly);
        BinaryVersion = (int?)assembly?.GetType("ZrConfig.Serialization.BinaryFormat")?.GetField("Version")?.GetRawConstantValue() ?? 4;
    }

    public ConfigType Parse(string text, out bool json)
    {
        text = text.Trim();
        json = text.StartsWith("json<", StringComparison.Ordinal);
        if (json)
        {
            if (!text.EndsWith('>'))
            {
                throw new FormatException("json<T> 缺少结束符 >。");
            }

            text = text[5..^1].Trim();
        }

        string referenceName = text.StartsWith("global::", StringComparison.Ordinal) ? text[8..] : text;
        if (referenceName.StartsWith("ZrConfig.Models.", StringComparison.Ordinal))
        {
            referenceName = referenceName[16..];
        }

        if (_references.TryGetValue(referenceName, out ConfigType? reference))
        {
            if (json)
            {
                throw new NotSupportedException("表引用填写目标行的 Id，不使用 json<T>。");
            }

            return reference;
        }

        return Resolve(ParseClr(text, 0));
    }

    public void RegisterTables(IEnumerable<string> names)
    {
        foreach (string name in names)
        {
            _references.TryAdd(name + "Config", new ConfigType(typeof(int), ConfigTypeKind.TableReference) { ReferenceTable = name });
        }
    }

    private Type ParseClr(string text, int depth)
    {
        if (depth > 64)
        {
            throw new FormatException("类型嵌套超过 64 层。");
        }

        text = text.Trim();
        int end = 0;
        while (end < text.Length && text[end] is not ('<' or '[' or '?'))
        {
            end++;
        }

        string nameText = text[..end].Trim();
        Type clr;
        if (end < text.Length && text[end] == '<')
        {
            int level = 1;
            int start = ++end;
            var arguments = new List<string>();
            for (; end < text.Length && level != 0; end++)
            {
                char c = text[end];
                if (c is '<' or '[')
                {
                    level++;
                }

                if (c is '>' or ']')
                {
                    level--;
                }

                if ((c == ',' && level == 1) || level == 0)
                {
                    arguments.Add(text[start..end]);
                    start = end + 1;
                }
            }

            if (level != 0)
            {
                throw new FormatException("泛型类型缺少结束符 >。");
            }

            if (nameText is not ("dictionary" or "Dictionary" or "System.Collections.Generic.Dictionary") || arguments.Count != 2)
            {
                throw new NotSupportedException("泛型类型请使用 Dictionary<TKey,TValue>。");
            }

            clr = typeof(Dictionary<,>).MakeGenericType(arguments.Select(a => ParseClr(a, depth + 1)).ToArray());
        }
        else if (!s_primitives.TryGetValue(nameText, out clr!))
        {
            string name = nameText switch
            {
                "kv" => "ZrConfig.Types.Kv",
                "vector2" => "ZrConfig.Types.Vector2",
                "vector3" => "ZrConfig.Types.Vector3",
                "json" => "ZrConfig.Types.RawJson",
                _ => nameText
            };
            Type[] matches = _externalTypes.Where(t => t.FullName == name || t.Name == name).ToArray();
            if (matches.Length != 1)
            {
                throw new NotSupportedException(matches.Length == 0 ? $"未知类型 {text}，请在所选代码根目录或其子目录的 C# 源码中定义。" : $"类型名 {text} 有歧义，请填写完整命名空间。");
            }

            clr = matches[0];
        }

        string suffix = text[end..].Trim();
        if (suffix.StartsWith('?'))
        {
            if (!clr.IsValueType || Nullable.GetUnderlyingType(clr) != null)
            {
                throw new NotSupportedException("? 仅用于非可空值类型，如 int?、Quality?、ConfigRange?。");
            }

            clr = typeof(Nullable<>).MakeGenericType(clr);
            suffix = suffix[1..].Trim();
        }

        var ranks = new List<int>();
        while (suffix.Length != 0)
        {
            int close = suffix.IndexOf(']');
            if (!suffix.StartsWith('[') || close < 1 || suffix[1..close].Any(c => c != ','))
            {
                throw new FormatException($"无效的类型后缀 {suffix}。");
            }

            int rank = close;
            if (rank > 3 || ranks.Count >= 3)
            {
                throw new NotSupportedException("数组支持最多三层嵌套，每层秩为 1、2 或 3。");
            }

            ranks.Add(rank);
            suffix = suffix[(close + 1)..].Trim();
        }

        // C# 数组后缀从外到内排列，例如 int[][,] 是矩阵的数组。
        for (int i = ranks.Count - 1; i >= 0; i--)
        {
            clr = ranks[i] == 1 ? clr.MakeArrayType() : clr.MakeArrayType(ranks[i]);
        }

        return clr;
    }

    public ConfigType Resolve(Type clr)
    {
        if (_resolved.TryGetValue(clr, out ConfigType? found))
        {
            return found;
        }

        ConfigTypeKind kind;
        if (s_primitives.ContainsValue(clr))
        {
            kind = ConfigTypeKind.Primitive;
        }
        else if (clr.IsEnum)
        {
            kind = ConfigTypeKind.Enum;
        }
        else if (clr.IsArray)
        {
            kind = ConfigTypeKind.Array;
        }
        else if (Nullable.GetUnderlyingType(clr) != null)
        {
            kind = ConfigTypeKind.Nullable;
        }
        else if (clr.IsGenericType && clr.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            kind = ConfigTypeKind.Dictionary;
        }
        else if (s_builtins.TryGetValue(clr.FullName ?? "", out kind))
        {
        }
        else if (clr.GetCustomAttribute<ConfigConverterAttribute>() != null)
        {
            kind = ConfigTypeKind.Converter;
        }
        else
        {
            kind = ConfigTypeKind.Object;
        }

        var type = new ConfigType(clr, kind);
        _resolved.Add(clr, type); // 先占位，支持递归数据结构。
        if (kind == ConfigTypeKind.Enum)
        {
            if (!clr.IsVisible)
            {
                throw new NotSupportedException($"{clr}: 配置枚举必须是 public。");
            }

            type.EnumMembers = Enum.GetNames(clr).ToDictionary(name => name, name => Enum.Parse(clr, name), StringComparer.Ordinal);
        }
        else if (kind == ConfigTypeKind.Array)
        {
            int depth = 0;
            for (Type? level = clr; level?.IsArray == true; level = level.GetElementType())
            {
                if (level.GetArrayRank() > 3 || ++depth > 3)
                {
                    throw new NotSupportedException($"{clr}: 数组支持最多三层嵌套，每层秩为 1、2 或 3。");
                }
            }

            type.Element = Resolve(clr.GetElementType()!);
        }
        else if (kind == ConfigTypeKind.Nullable)
        {
            type.Element = Resolve(Nullable.GetUnderlyingType(clr)!);
        }
        else if (kind == ConfigTypeKind.Dictionary)
        {
            Type[] args = clr.GetGenericArguments();
            type.Key = Resolve(args[0]);
            if (type.Key.Kind != ConfigTypeKind.Enum && (type.Key.Kind != ConfigTypeKind.Primitive || args[0] == typeof(float) || args[0] == typeof(double)))
            {
                throw new NotSupportedException("字典键支持整数、bool、string、decimal 和枚举，不支持可空、浮点、对象或集合键。");
            }

            type.Element = Resolve(args[1]);
        }
        else if (kind == ConfigTypeKind.Converter)
        {
            ConfigConverterAttribute attr = clr.GetCustomAttribute<ConfigConverterAttribute>()!;
            if (string.IsNullOrWhiteSpace(attr.Version) || !attr.ConverterType.IsVisible || attr.ConverterType.IsAbstract || attr.ConverterType.GetConstructor(Type.EmptyTypes) == null || !typeof(IConfigConverter<>).MakeGenericType(clr)
                .IsAssignableFrom(attr.ConverterType))
            {
                throw new NotSupportedException($"{clr}: 转换器必须公开、可实例化、实现 IConfigConverter<T> 并声明版本。");
            }

            type.Converter = attr.ConverterType;
            type.ConverterVersion = attr.Version;
        }
        else if (kind == ConfigTypeKind.Object)
        {
            if (typeof(IConfigRow).IsAssignableFrom(clr))
            {
                throw new NotSupportedException("表引用仅支持直接字段，填写目标表的 XxxConfig 类型；目标 Excel 必须存在，不支持内嵌对象或集合中的表引用。");
            }

            if (!clr.IsVisible || clr.IsGenericType || clr.IsInterface || clr.Assembly != _externalTypes.FirstOrDefault()?.Assembly || typeof(Delegate).IsAssignableFrom(clr) || (!clr.IsValueType && !clr.IsAbstract && clr.GetConstructor(Type.EmptyTypes) == null))
            {
                throw new NotSupportedException($"{clr}: 自定义数据类型必须是公开的非泛型类/结构，类需要 public 无参构造函数；特殊类型可提供转换器。");
            }

            var members = new List<ConfigMember>();
            var hierarchy = new Stack<Type>();
            for (Type? current = clr; current != null && current != typeof(object) && current != typeof(ValueType); current = current.BaseType)
            {
                hierarchy.Push(current);
            }

            while (hierarchy.TryPop(out Type? current))
            {
                foreach (MemberInfo member in current.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .OrderBy(m => m.MetadataToken))
                {
                    Type memberType;
                    if (member is FieldInfo field)
                    {
                        if (field.IsInitOnly)
                        {
                            throw new NotSupportedException($"{clr}.{field.Name}: readonly 字段请改为可写数据字段，或为类型提供转换器。");
                        }

                        memberType = field.FieldType;
                    }
                    else if (member is PropertyInfo prop)
                    {
                        if (prop.GetIndexParameters().Length != 0 || prop.GetMethod?.IsPublic != true || prop.SetMethod?.IsPublic != true)
                        {
                            throw new NotSupportedException($"{clr}.{prop.Name}: 配置属性必须有 public get/set，且不能是索引器。");
                        }

                        if (prop.SetMethod.ReturnParameter.GetRequiredCustomModifiers()
                            .Any(t => t.FullName == "System.Runtime.CompilerServices.IsExternalInit"))
                        {
                            throw new NotSupportedException($"{clr}.{prop.Name}: init 属性暂不支持，请使用 set。");
                        }

                        memberType = prop.PropertyType;
                    }
                    else
                    {
                        continue;
                    }

                    if (members.Any(m => m.Name == member.Name))
                    {
                        throw new NotSupportedException($"{clr}.{member.Name}: 继承成员重名。");
                    }

                    members.Add(new ConfigMember(member.Name,
                        Resolve(memberType),
                        member,
                        member.IsDefined(typeof(ConfigRequiredAttribute))));
                }
            }

            type.Members = members.ToArray();
            type.MemberMap = members.ToDictionary(m => m.Name, StringComparer.Ordinal);
            type.Variants = _externalTypes.Where(t => t != clr && !t.IsAbstract && !t.IsGenericType && clr.IsAssignableFrom(t))
                .OrderBy(t => t.FullName,
                StringComparer.Ordinal)
                .Select(Resolve)
                .ToArray();
            if (clr.IsAbstract && type.Variants.Length == 0)
            {
                throw new NotSupportedException($"{clr}: 抽象类型没有可用的具体子类。");
            }

            var names = new Dictionary<string, ConfigType?>(StringComparer.Ordinal);
            foreach (ConfigType variant in type.Variants.Prepend(type).Where(t => !t.ClrType.IsAbstract))
            {
                names[variant.ClrType.FullName!] = variant;
                string shortName = variant.ClrType.Name;
                if (shortName != variant.ClrType.FullName && !names.TryAdd(shortName, variant))
                {
                    names[shortName] = null;
                }
            }

            type.PolymorphicNames = names;
        }

        return type;
    }

    public static string CsName(Type type)
    {
        if (type.IsArray)
        {
            string suffix = "";
            while (type.IsArray)
            {
                suffix += "[" + new string(',', type.GetArrayRank() - 1) + "]";
                type = type.GetElementType()!;
            }

            return CsName(type) + suffix;
        }

        if (Nullable.GetUnderlyingType(type) is { } underlying)
        {
            return CsName(underlying) + "?";
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            return "global::System.Collections.Generic.Dictionary<" + string.Join(", ", type.GetGenericArguments().Select(CsName)) + ">";
        }

        foreach ((string name, Type primitive) in s_primitives)
        {
            if (type == primitive)
            {
                return name;
            }
        }

        return "global::" + string.Join(".",
            (type.FullName ?? type.Name).Replace('+',
            '.')
            .Split('.')
            .Select(name => SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None ? "@" + name : name));
    }

    public static IReadOnlyList<ConfigType> Graph(IEnumerable<ConfigType> roots)
    {
        var found = new HashSet<ConfigType>();
        foreach (ConfigType root in roots)
        {
            Visit(root);
        }

        return found.OrderBy(TypeIdentity, StringComparer.Ordinal).ToArray();
        void Visit(ConfigType type)
        {
            if (!found.Add(type))
            {
                return;
            }

            if (type.Element != null)
            {
                Visit(type.Element);
            }

            if (type.Key != null)
            {
                Visit(type.Key);
            }

            foreach (ConfigMember member in type.Members)
            {
                Visit(member.Type);
            }

            foreach (ConfigType variant in type.Variants)
            {
                Visit(variant);
            }
        }
    }

    // 标签只来源于实际编码字段；生成继承声明补充的父类型不能扩大二进制类型目录。
    internal static ConfigType[] WireTags(IEnumerable<ConfigType> roots) => Graph(roots)
        .Where(t => t.IsPolymorphic).SelectMany(t => t.Variants.Prepend(t))
        .Where(t => !t.ClrType.IsAbstract).Distinct()
        .OrderBy(t => t.ClrType.FullName, StringComparer.Ordinal).ToArray();

    public static string Signature(IEnumerable<ConfigType> roots)
    {
        // 类型签名属于跨平台数据协议，不能使用系统换行符。
        var text = new StringBuilder("ZrConfig.Types/2\n");
        foreach (ConfigType type in Graph(roots))
        {
            text.Append(TypeIdentity(type))
                .Append(':')
                .Append(type.Kind)
                .Append(':')
                .Append(type.ClrType.IsValueType)
                .Append(':')
                .Append(type.ClrType.IsAbstract)
                .Append(':')
                .Append(type.Converter?.FullName)
                .Append(':')
                .Append(type.ConverterVersion)
                .Append('\n');
            var wireLayout = type.ClrType.GetCustomAttribute<ConfigWireLayoutAttribute>() ?? type.Converter?.GetCustomAttribute<ConfigWireLayoutAttribute>();
            if (wireLayout != null) text.Append("wire:").AppendJoin(",", wireLayout.Members).Append('\n');
            foreach (ConfigMember member in type.Members)
            {
                text.Append(member.Name).Append(':').Append(Identity(member.Type.ClrType)).Append(':').Append(member.Required).Append('\n');
            }

            foreach (ConfigType variant in type.Variants)
            {
                text.Append("$").Append(variant.ClrType.FullName).Append('\n');
            }

            if (type.Kind == ConfigTypeKind.Enum)
            {
                text.Append(Enum.GetUnderlyingType(type.ClrType).FullName).Append('\n');
                foreach (string name in Enum.GetNames(type.ClrType))
                {
                    text.Append(name)
                        .Append('=')
                        .Append(Convert.ToString(Convert.ChangeType(Enum.Parse(type.ClrType,
                        name),
                        Enum.GetUnderlyingType(type.ClrType),
                        CultureInfo.InvariantCulture),
                        CultureInfo.InvariantCulture))
                        .Append('\n');
                }
            }
        }

        return text.ToString();
    }

    // 泛型 FullName 带动态程序集指纹；schema 只记录结构，不能随规则源码变化。
    public static string TypeIdentity(ConfigType type) => type.ReferenceTable == null ? Identity(type.ClrType) : "ref:ZrConfig.Models." + type.ReferenceTable + "Config:Id:int:zero-null/1";
    public static string Identity(Type type)
    {
        if (type.IsArray)
        {
            return Identity(type.GetElementType()!) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
        }

        if (type.IsGenericType)
        {
            return type.GetGenericTypeDefinition().FullName + "<" + string.Join(",", type.GetGenericArguments().Select(Identity)) + ">";
        }

        return type.FullName!;
    }
}
