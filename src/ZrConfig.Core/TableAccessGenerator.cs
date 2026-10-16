using System.Text;

namespace ZrConfig.Core;

internal static class TableAccessGenerator
{
    public static void Append(StringBuilder sb, TableSchema schema)
    {
        string row = schema.Name + "Config";
        string table = row + "Table";
        bool SharedGroup(TableGroupSchema group) => schema.KeyFields.Count == 1 && group.Fields.Count == 1 &&
            group.Fields[0].Name == schema.KeyFields[0].Name;
        bool sharedKey = schema.Groups.Any(SharedGroup);
        sb.AppendLine($"    public sealed class {table} : ConfigTable<{row}>").AppendLine("    {");
        sb.AppendLine("        private static readonly ITableCodec s_codec;")
            .AppendLine("        // 首次使用本表时创建一次；只保存编解码逻辑，不保存快照或配置数据。")
            .AppendLine($"        static {table}()")
            .AppendLine("        {")
            .AppendLine($"            s_codec = new {row}Codec();")
            .AppendLine("        }")
            .AppendLine("        public static ITableCodec GetCodec()")
            .AppendLine("        {")
            .AppendLine("            return s_codec;")
            .AppendLine("        }");
        sb.AppendLine(sharedKey
            ? $"        private readonly IReadOnlyDictionary<{KeyType(schema.KeyFields)}, ConfigRowGroup<{row}>> _keyGroups;"
            : $"        private readonly IReadOnlyDictionary<{KeyType(schema.KeyFields)}, {row}> _key;");

        for (int i = 0; i < schema.Groups.Count; i++)
        {
            if (!SharedGroup(schema.Groups[i]))
            {
                sb.AppendLine($"        private readonly IReadOnlyDictionary<{KeyType(schema.Groups[i].Fields)}, ConfigRowGroup<{row}>> _group{i};");
            }
        }

        sb.AppendLine($"        public {table}(IEnumerable<{row}> rows) : base(\"{schema.Name}\", rows)").AppendLine("        {");
        sb.AppendLine(sharedKey
            ? "            _keyGroups = CreateUniqueGroupIndex(SelectKey);"
            : $"            _key = CreateIndex(SelectKey{ComparerArgument(schema.KeyFields, "Key")});");

        for (int i = 0; i < schema.Groups.Count; i++)
        {
            if (!SharedGroup(schema.Groups[i]))
            {
                sb.AppendLine($"            _group{i} = CreateGroupIndex(SelectGroup{i}{ComparerArgument(schema.Groups[i].Fields, "Group" + i)});");
            }
        }

        sb.AppendLine("        }");
        AppendSelector(sb, row, schema.KeyFields, "Key");

        for (int i = 0; i < schema.Groups.Count; i++)
        {
            if (!SharedGroup(schema.Groups[i]))
            {
                AppendSelector(sb, row, schema.Groups[i].Fields, "Group" + i);
            }
        }

        sb.AppendLine($"        public {row} Get({Parameters(schema.KeyFields)})")
            .AppendLine("        {")
            .AppendLine(sharedKey
                ? $"            return _keyGroups[{Key(schema.KeyFields, "@")}][0];"
                : $"            return _key[{Key(schema.KeyFields, "@")}];")
            .AppendLine("        }");
        // 输出参数使用 camelCase；业务字段重名时添加数字后缀。
        string outRow = QueryVariable(schema.KeyFields, "row");
        sb.AppendLine($"        public bool TryGet({Parameters(schema.KeyFields)}, out {row}? {outRow})")
            .AppendLine("        {");
        if (sharedKey)
        {
            string group = QueryVariable(schema.KeyFields, "group");
            sb.AppendLine($"            if (_keyGroups.TryGetValue({Key(schema.KeyFields, "@")}, out ConfigRowGroup<{row}>? {group}))")
                .AppendLine("            {")
                .AppendLine($"                {outRow} = {group}[0];")
                .AppendLine("                return true;")
                .AppendLine("            }")
                .AppendLine($"            {outRow} = null;")
                .AppendLine("            return false;");
        }
        else
        {
            sb.AppendLine($"            return _key.TryGetValue({Key(schema.KeyFields, "@")}, out {outRow});");
        }

        sb.AppendLine("        }");

        for (int i = 0; i < schema.Groups.Count; i++)
        {
            TableGroupSchema group = schema.Groups[i];
            string index = SharedGroup(group) ? "_keyGroups" : "_group" + i;
            sb.AppendLine($"        public ConfigRowGroup<{row}> Get{group.Name}({Parameters(group.Fields)})")
                .AppendLine("        {")
                .AppendLine($"            if ({index}.TryGetValue({Key(group.Fields, "@")}, out ConfigRowGroup<{row}>? {QueryVariable(group.Fields, "group")}))")
                .AppendLine("            {")
                .AppendLine($"                return {QueryVariable(group.Fields, "group")}!;")
                .AppendLine("            }")
                .AppendLine($"            return ConfigRowGroup<{row}>.Empty;")
                .AppendLine("        }");
        }

        AppendComparer(sb, schema.KeyFields, "Key");
        for (int i = 0; i < schema.Groups.Count; i++)
        {
            AppendComparer(sb, schema.Groups[i].Fields, "Group" + i);
        }

        sb.AppendLine("    }")
            .AppendLine($"    public static class {table}Access")
            .AppendLine("    {")
            .AppendLine($"        public static {table} Get{schema.Name}Table(this ConfigSnapshot snapshot)")
            .AppendLine("        {")
            .AppendLine($"            return ({table})snapshot.GetTable<{row}>({table}.GetCodec());")
            .AppendLine("        }")
            .AppendLine($"        public static {table} Get{schema.Name}Table(this ConfigSnapshot snapshot, System.Threading.CancellationToken cancellationToken)")
            .AppendLine("        {")
            .AppendLine($"            return ({table})snapshot.GetTable<{row}>({table}.GetCodec(), cancellationToken);")
            .AppendLine("        }")
            .AppendLine($"        public static ConfigOperation<{table}> Get{schema.Name}TableAsync(this ConfigSnapshot snapshot, System.Threading.CancellationToken cancellationToken = default)")
            .AppendLine("        {")
            .AppendLine($"            return snapshot.GetTableAsync<{row}, {table}>({table}.GetCodec(), cancellationToken);")
            .AppendLine("        }")
            .AppendLine($"        public static {table} Get{schema.Name}Table(this IConfigTableProvider tables)")
            .AppendLine("        {")
            .AppendLine($"            return ({table})tables.GetTable<{row}>({table}.GetCodec());")
            .AppendLine("        }");
        string receiver = QueryVariable(schema.KeyFields, "snapshot");
        string token = QueryVariable(schema.KeyFields, "cancellationToken");
        sb.AppendLine($"        public static {row} Get{schema.Name}(this ConfigSnapshot {receiver}, {Parameters(schema.KeyFields)}, System.Threading.CancellationToken {token} = default)")
            .AppendLine("        {")
            .AppendLine($"            return {receiver}.GetRow<{row}, {KeyType(schema.KeyFields)}>({table}.GetCodec(), {Key(schema.KeyFields, "@")}, {token});")
            .AppendLine("        }")
            .AppendLine($"        public static ConfigOperation<{row}> Get{schema.Name}Async(this ConfigSnapshot {receiver}, {Parameters(schema.KeyFields)}, System.Threading.CancellationToken {token} = default)")
            .AppendLine("        {")
            .AppendLine($"            return {receiver}.GetRowAsync<{row}, {KeyType(schema.KeyFields)}>({table}.GetCodec(), {Key(schema.KeyFields, "@")}, {token});")
            .AppendLine("        }");

        sb.AppendLine("    }");
    }

    internal static string QueryVariable(IReadOnlyList<FieldSchema> fields, string name)
    {
        string candidate = name;
        for (int suffix = 1; fields.Any(field => ParameterName(fields, field) == candidate); suffix++)
        {
            candidate = name + suffix;
        }

        return candidate;
    }

    private static void AppendSelector(StringBuilder sb, string row, IReadOnlyList<FieldSchema> fields, string name)
    {
        sb.AppendLine($"        private static {KeyType(fields)} Select{name}({row} row)")
            .AppendLine("        {")
            .AppendLine($"            return {Key(fields, "row.@")};")
            .AppendLine("        }");
    }

    internal static string ComparerArgument(IReadOnlyList<FieldSchema> fields,
        string name) => fields.Count > 1 ? $", Zr{name}Comparer.Instance" : "";
    internal static void AppendComparer(StringBuilder sb, IReadOnlyList<FieldSchema> fields, string name)
    {
        if (fields.Count < 2)
        {
            return;
        }

        string type = KeyType(fields);
        string comparer = "Zr" + name + "Comparer";
        // 显式比较每个分量，避免长 ValueTuple 在比较/哈希 Rest 时装箱。
        sb.AppendLine($"        private sealed class {comparer} : IEqualityComparer<{type}>")
            .AppendLine("        {")
            .AppendLine($"            public static readonly {comparer} Instance = new {comparer}();")
            .AppendLine($"            public bool Equals({type} x, {type} y)")
            .AppendLine("            {")
            .AppendLine("                return " + string.Join(" && ",
            fields.Select((f,
            i) => $"EqualityComparer<{f.Type.CsName}>.Default.Equals(x.Item{i + 1}, y.Item{i + 1})")) + ";")
            .AppendLine("            }")
            .AppendLine($"            public int GetHashCode({type} value)")
            .AppendLine("            {")
            .AppendLine("                unchecked")
            .AppendLine("                {")
            .AppendLine("                    int hash = (int)2166136261;");
        for (int i = 0; i < fields.Count; i++)
        {
            sb.AppendLine($"                    hash = (hash ^ EqualityComparer<{fields[i].Type.CsName}>.Default.GetHashCode(value.Item{i + 1})) * 16777619;");
        }

        sb.AppendLine("                    return hash;")
            .AppendLine("                }")
            .AppendLine("            }")
            .AppendLine("        }");
    }

    internal static string KeyType(IReadOnlyList<FieldSchema> fields) => Join(fields.Select(f => f.Type.CsName));
    internal static string Key(IReadOnlyList<FieldSchema> fields,
        string prefix) => Join(fields.Select(field => prefix + (prefix == "@" ? ParameterName(fields,
        field) : field.Name)));
    private static string Join(IEnumerable<string> parts)
    {
        string[] items = parts.ToArray();
        return items.Length == 1 ? items[0] : "(" + string.Join(", ", items) + ")";
    }

    internal static string Parameters(IReadOnlyList<FieldSchema> fields) => string.Join(", ",
        fields.Select(field => field.Type.CsName + " @" + ParameterName(fields,
        field)));
    internal static string ParameterName(IReadOnlyList<FieldSchema> fields, FieldSchema field)
    {
        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (FieldSchema current in fields)
        {
            string stem = char.ToLowerInvariant(current.Name[0]) + current.Name[1..];
            string candidate = stem;
            for (int suffix = 1; !used.Add(candidate); suffix++)
            {
                candidate = stem + suffix;
            }

            if (ReferenceEquals(current, field))
            {
                return candidate;
            }
        }

        throw new ArgumentException("The field must belong to the parameter list.", nameof(field));
    }
}
