using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ZrConfig.Core;

/// <summary>Maps a current Excel sheet or root field name to an accepted logical identity.
/// Table and Field are the original logical names; Field=null maps the sheet name.</summary>
public sealed record SourceNameMapping(string Table, string? Field, string SourceName);

internal sealed record SourceTableOrder(string Table, string[] Fields, string[] Keys);
internal sealed record GenerationSourceContract(int Format, SourceNameMapping[] Mappings, SourceTableOrder[] Tables)
{
    internal const string MetadataKey = "ZrConfig.SourceContract/1";
    internal static readonly GenerationSourceContract Empty = new(1, [], []);
    internal static GenerationSourceContract FromAssembly(Assembly? assembly)
    {
        string[] values = assembly?.GetCustomAttributes<AssemblyMetadataAttribute>().Where(a => a.Key == MetadataKey).Select(a => a.Value ?? "").ToArray() ?? [];
        if (values.Length == 0) return Empty;
        if (values.Length != 1) throw new InvalidDataException("Duplicate generated source contract.");
        var value = JsonSerializer.Deserialize<GenerationSourceContract>(values[0]) ?? throw new InvalidDataException("Missing generated source contract.");
        value.Validate(); return value;
    }
    internal static GenerationSourceContract Resolve(string code, IReadOnlyList<SourceNameMapping>? requested)
    {
        string file = Path.Combine(code, ".schema.json");
        SchemaSummary? previous = File.Exists(file) ? JsonSerializer.Deserialize<SchemaSummary>(File.ReadAllText(file)) : null;
        if (previous != null && (!previous.SupportedFormat || previous.Tables == null)) throw new InvalidDataException("Unsupported schema baseline.");
        if (previous?.ApiContract != null) GeneratedApiContracts.Validate(previous.ApiContract);
        SourceNameMapping[] inherited = previous?.ApiContract?.Policy?.SourceMappings ?? [];
        ValidateMappings(inherited);
        var entries = inherited.ToDictionary(Key, StringComparer.Ordinal);
        if (requested != null)
        {
            ValidateMappings(requested);
            foreach (SourceNameMapping mapping in requested)
            {
                TableSummary? table = previous?.Tables.SingleOrDefault(t => t.Name == mapping.Table);
                if (table == null || mapping.Field != null && !table.Fields.Any(f => f.Name == mapping.Field))
                    throw new InvalidDataException("源名映射必须绑定已接受的逻辑表/字段：" + Key(mapping));
                if(mapping.Field==null && previous!.Tables.Any(t=>t.Name!=mapping.Table&&string.Equals(t.Name,mapping.SourceName,StringComparison.OrdinalIgnoreCase)) ||
                    mapping.Field!=null && table.Fields.Any(f=>f.Name!=mapping.Field&&string.Equals(f.Name,mapping.SourceName,StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidDataException("源名称不能占用另一已接受逻辑身份："+mapping.SourceName);
                if(mapping.SourceName==(mapping.Field??mapping.Table))entries.Remove(Key(mapping));else entries[Key(mapping)] = mapping;
            }
        }
        var contract = new GenerationSourceContract(1, entries.Values.OrderBy(Key, StringComparer.Ordinal).ToArray(),
            previous?.Tables.Select(t => new SourceTableOrder(t.Name, t.Fields.Select(f => f.Name).ToArray(), t.Keys)).ToArray() ?? []);
        contract.Validate(); return contract;
    }
    internal static void ValidateMappings(IEnumerable<SourceNameMapping> mappings)
    {
        var identities = new HashSet<string>(StringComparer.Ordinal);
        var sources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (SourceNameMapping item in mappings)
        {
            if (item == null || !Identifier(item.Table) || item.Field != null && !Identifier(item.Field) || !Identifier(item.SourceName) ||
                !identities.Add(Key(item)) || !sources.Add((item.Field == null ? "$tables" : item.Table) + "/" + item.SourceName))
                throw new InvalidDataException("源名映射无效或重复；映射按逻辑身份直接绑定，不允许链式或模糊匹配。");
        }
    }
    internal void Validate()
    {
        if (Format != 1 || Mappings == null || Tables == null) throw new InvalidDataException("Unsupported generated source contract.");
        ValidateMappings(Mappings);
        if (Tables.Any(t => t == null || !Identifier(t.Table) || t.Fields == null || t.Keys == null || t.Fields.Any(f => !Identifier(f)) ||
            t.Fields.Distinct(StringComparer.Ordinal).Count() != t.Fields.Length || t.Keys.Distinct(StringComparer.Ordinal).Count() != t.Keys.Length || t.Keys.Any(k => !t.Fields.Contains(k, StringComparer.Ordinal))) ||
            Tables.Select(t => t.Table).Distinct(StringComparer.Ordinal).Count() != Tables.Length)
            throw new InvalidDataException("Invalid stable field order.");
    }
    private static bool Identifier(string? value) => value != null && Regex.IsMatch(value, "^[A-Za-z_][A-Za-z_0-9]*$", RegexOptions.CultureInvariant);
    private static string Key(SourceNameMapping value) => value.Table + "/" + (value.Field ?? "$table");
    internal string Table(string source)
    {
        SourceNameMapping? mapped = Mappings.SingleOrDefault(m => m.Field == null && m.SourceName == source);
        if (mapped != null) return mapped.Table;
        if (Mappings.Any(m => m.Field == null && m.Table == source && m.SourceName != source)) throw new InvalidDataException("源表仍使用已映射的旧名称：" + source);
        return source;
    }
    internal string Header(string table, string header)
    {
        Match match = Regex.Match(header, "^[A-Za-z_][A-Za-z_0-9]*", RegexOptions.CultureInvariant);
        if (!match.Success) return header;
        string source = match.Value;
        SourceNameMapping? mapped = Mappings.SingleOrDefault(m => m.Table == table && m.Field != null && m.SourceName == source);
        if (mapped != null) return mapped.Field + header[source.Length..];
        if (Mappings.Any(m => m.Table == table && m.Field == source && m.SourceName != source)) throw new InvalidDataException("源字段仍使用已映射的旧名称：" + table + "." + source);
        return header;
    }
    internal string TypeName(string value) => Regex.Replace(value, "(?<![A-Za-z_0-9.])[A-Za-z_][A-Za-z_0-9]*(?![A-Za-z_0-9.])", m => Mappings.FirstOrDefault(x => x.Field == null && x.SourceName + "Config" == m.Value) is {} table ? table.Table + "Config" : m.Value, RegexOptions.CultureInvariant);
    internal TableSchema Order(TableSchema table)
    {
        SourceTableOrder? previous = Tables.SingleOrDefault(t => t.Table == table.Name);
        if (previous == null) return table;
        var positions = previous.Fields.Select((name, index) => (name, index)).ToDictionary(x => x.name, x => x.index, StringComparer.Ordinal);
        FieldSchema[] fields = table.Fields.OrderBy(f => positions.GetValueOrDefault(f.Name, int.MaxValue)).ToArray();
        // A column move does not redefine key semantics. Changing the actual key set remains a migration.
        FieldSchema[] keys = table.KeyFields.ToArray();
        if (keys.Select(k => k.Name).ToHashSet(StringComparer.Ordinal).SetEquals(previous.Keys))
            keys = previous.Keys.Select(k => keys.Single(f => f.Name == k)).ToArray();
        return table with { Fields = fields, KeyFields = keys };
    }
    internal void CheckUsed(IReadOnlyList<TableSchema> tables)
    {
        foreach (SourceNameMapping mapping in Mappings)
        {
            TableSchema? table = tables.SingleOrDefault(t => t.Name == mapping.Table);
            if (table == null || mapping.Field != null && !table.InputFields.Any(f => f.Name == mapping.Field))
                throw new InvalidDataException("源名映射没有匹配输入：" + Key(mapping));
        }
    }
    internal GenerationSourceContract ForOutput(IReadOnlyList<TableSchema> tables) => new(1, Mappings,
        tables.Select(t => new SourceTableOrder(t.Name, t.Fields.Select(f => f.Name).ToArray(), t.KeyFields.Select(f => f.Name).ToArray())).ToArray());
}
