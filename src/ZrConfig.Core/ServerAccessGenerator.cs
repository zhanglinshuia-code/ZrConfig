using System.Text;

namespace ZrConfig.Core;

// A separate namespace keeps all existing C# accessors and implicit reference semantics intact.
internal static class ServerAccessGenerator
{
    internal static void Append(StringBuilder sb, TableSchema schema, Func<string, string> legacyMember)
    {
        string model = "global::ZrConfig.Models." + schema.Name + "Config";
        string codec = model + "Table.GetCodec()";
        string row = schema.Name + "Row", table = schema.Name + "Table", group = schema.Name + "Group";
        const string runtime = "global::ZrConfig.Runtime.Server.";
        string keyType = TableAccessGenerator.KeyType(schema.KeyFields);
        string parameters = TableAccessGenerator.Parameters(schema.KeyFields);
        string key = TableAccessGenerator.Key(schema.KeyFields, "@");
        string token = TableAccessGenerator.QueryVariable(schema.KeyFields, "cancellationToken");
        string result = TableAccessGenerator.QueryVariable(schema.KeyFields, "value");
        sb.AppendLine("namespace ZrConfig.Models.Server {")
            .AppendLine($"public readonly struct {row} {{")
            .AppendLine($"private readonly {runtime}RowHandle<{model}> _handle;")
            .AppendLine($"internal {row}({runtime}RowHandle<{model}> handle) {{ _handle = handle; }}")
            .AppendLine($"public {runtime}Snapshot Snapshot => _handle.Snapshot;")
            .AppendLine($"public {model} Borrow() => _handle.Borrow();");
        foreach (FieldSchema field in schema.Fields)
        {
            if (field.Type.ReferenceTable is string reference)
            {
                string target = "global::ZrConfig.Models." + reference + "Config";
                string id = legacyMember("ZrReferenceId" + field.Name);
                sb.AppendLine($"public int @{field.Name}Id => _handle.Borrow().{id};");
                foreach (string operation in new[] { "Get", "Load" })
                    sb.AppendLine($"public {reference}Row? {operation}{field.Name}() {{ var value = _handle.{operation}Reference<{target}>({target}Table.GetCodec(), {field.Name}Id, \"{field.Name}\"); return value == null ? ({reference}Row?)null : new {reference}Row(value); }}");
            }
            else sb.AppendLine($"public {ServerValueGenerator.Type(field.Type)} @{field.Name} => {ServerValueGenerator.Project(field.Type, "_handle.Borrow().@" + field.Name, "_handle.Snapshot")};");
        }
        sb.AppendLine("}")
            .AppendLine($"public readonly struct {group} {{")
            .AppendLine($"private readonly {runtime}GroupHandle<{model}> _handle;")
            .AppendLine($"internal {group}({runtime}GroupHandle<{model}> handle) {{ _handle = handle; }}")
            .AppendLine("public int Count => _handle.Count;")
            .AppendLine($"public {runtime}Snapshot Snapshot => _handle.Snapshot;")
            .AppendLine($"public {row} this[int index] => new {row}(_handle[index]);")
            .AppendLine("}")
            .AppendLine($"public readonly struct {table} {{")
            .AppendLine($"private readonly {runtime}Snapshot _snapshot;")
            .AppendLine($"internal {table}({runtime}Snapshot snapshot) {{ _snapshot = snapshot; }}")
            .AppendLine($"public int Count => _snapshot.GetRowCount({codec});")
            .AppendLine($"public {row} Get({parameters}) => new {row}(_snapshot.GetRow<{model}, {keyType}>({codec}, {key}));")
            .AppendLine($"public {row}? TryGet({parameters}) {{ var {result} = _snapshot.TryGetRow<{model}, {keyType}>({codec}, {key}); return {result} == null ? ({row}?)null : new {row}({result}); }}")
            .AppendLine($"public {row} Load({parameters}, global::System.Threading.CancellationToken {token} = default) => new {row}(_snapshot.LoadRow<{model}, {keyType}>({codec}, {key}, {token}));")
            .AppendLine($"public {row}? TryLoad({parameters}, global::System.Threading.CancellationToken {token} = default) {{ var {result} = _snapshot.TryLoadRow<{model}, {keyType}>({codec}, {key}, {token}); return {result} == null ? ({row}?)null : new {row}({result}); }}")
            .AppendLine($"public static {runtime}WarmupRequest WarmupRow({parameters}) => {runtime}WarmupRequest.Row<{model}, {keyType}>({codec}, {key});")
            .AppendLine($"public async ConfigOperation<{row}> LoadAsync({parameters}, global::System.Threading.CancellationToken {token} = default) => new {row}(await _snapshot.LoadRowAsync<{model}, {keyType}>({codec}, {key}, {token}));")
            .AppendLine($"public {group} Rows => new {group}(_snapshot.GetRows<{model}>({codec}));")
            .AppendLine($"public {group} LoadAll(global::System.Threading.CancellationToken cancellationToken = default) {{ _snapshot.LoadTable({codec}, cancellationToken); return Rows; }}");
        for (int i = 0; i < schema.Groups.Count; i++)
        {
            TableGroupSchema g = schema.Groups[i];
            string type = TableAccessGenerator.KeyType(g.Fields), args = TableAccessGenerator.Parameters(g.Fields);
            string value = TableAccessGenerator.Key(g.Fields, "@"), cancel = TableAccessGenerator.QueryVariable(g.Fields, "cancellationToken");
            string variableBytes = "0L" + string.Concat(g.Fields.Where(f => f.Type.ClrType == typeof(string)).Select(f => $" + value.@{f.Name}.Length * 4L"));
            sb.AppendLine($"private static readonly {runtime}GroupDescriptor<{model}, {type}> group{i} = new {runtime}GroupDescriptor<{model}, {type}>(\"{g.Name}\", {model}Table.GetCodec, value => {TableAccessGenerator.Key(g.Fields, "value.@")}{TableAccessGenerator.ComparerArgument(g.Fields, "Group" + i)}).WithIndexEstimate({g.Fields.Count}, value => {variableBytes});")
                .AppendLine($"public {group} Find{g.Name}({args}) => new {group}(_snapshot.FindGroup(group{i}, {value}));")
                .AppendLine($"public static {runtime}WarmupRequest Warmup{g.Name}({args}) => {runtime}WarmupRequest.Group(group{i}, {value});")
                .AppendLine($"public {group} Load{g.Name}({args}, global::System.Threading.CancellationToken {cancel} = default) => new {group}(_snapshot.LoadGroup(group{i}, {value}, {cancel}));");
            TableAccessGenerator.AppendComparer(sb, g.Fields, "Group" + i);
        }
        sb.AppendLine($"internal static readonly {runtime}GroupDescriptor[] GroupDescriptors = new {runtime}GroupDescriptor[] {{ {string.Join(", ", Enumerable.Range(0,schema.Groups.Count).Select(i => "group" + i))} }};")
            .AppendLine("}")
            .AppendLine($"public static class {schema.Name}Access {{")
            .AppendLine($"public static {table} Get{schema.Name}Table(this {runtime}Snapshot snapshot) {{ _ = snapshot.GetRowCount({codec}); return new {table}(snapshot); }}")
            .AppendLine("}").AppendLine("}");
    }
}
