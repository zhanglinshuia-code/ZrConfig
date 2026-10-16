using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using ZrConfig.Runtime;

namespace ZrConfig.Core;

public sealed record FieldSummary(string Name, string Type, string Target);
public sealed record TableSummary(string Name, string SchemaHash, string[] Keys, string[] Groups, FieldSummary[] Fields);
public sealed record SchemaSummary(int Format, string Target, TableSummary[] Tables)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NameEncoding { get; init; }
    internal bool SupportedFormat => Format==1 && NameEncoding is null or "compact-bindings-v1";
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeneratedApiContract? ApiContract { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiMigrationReceipt? LastMigration { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiMigrationRevision[]? ApiMigrationHistory { get; init; }

    internal static SchemaSummary Create(ConfigTarget target, IReadOnlyList<TableSchema> tables) => new(1,
        ConfigTargets.Marker(target), tables.OrderBy(t => t.Name, StringComparer.Ordinal).Select(t => new TableSummary(t.Name,
            t.SchemaHash, t.KeyFields.Select(f => f.Name).ToArray(), t.Groups.Select(g => g.Name + ":" + string.Join(",", g.Fields.Select(f => f.Name))).ToArray(),
            t.Fields.Select(f => new FieldSummary(f.Name, f.TypeName, f.Targets.ToString())).ToArray())).ToArray());

    internal string Serialize()
    {
        using var text = new JsonTextStream();
        // Indentation alone becomes hundreds of MB for millions of bindings.
        // Whitespace has no role in the descriptor digest or baseline semantics.
        bool compact=Tables.Length>256;
        var options=new JsonSerializerOptions { WriteIndented = !compact };
        if(compact)options.Converters.Add(new GeneratedNameBindingJsonConverter(true));
        JsonSerializer.Serialize(text, this with { NameEncoding=compact?"compact-bindings-v1":null }, options);
        return text.Finish();
    }

    // The serializer's string overload transcodes one contiguous UTF-8 buffer.
    // Decode incrementally so large API ledgers don't require a maximum-sized
    // temporary character array in addition to their final transaction text.
    private sealed class JsonTextStream : Stream
    {
        private readonly StringBuilder _text = new();
        private readonly Decoder _decoder = Encoding.UTF8.GetDecoder();
        private readonly char[] _characters = new char[16384];
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            while (!buffer.IsEmpty)
            {
                _decoder.Convert(buffer, _characters, false, out int bytes, out int characters, out _);
                _text.Append(_characters, 0, characters);
                buffer = buffer[bytes..];
            }
        }
        internal string Finish()
        {
            _decoder.Convert(ReadOnlySpan<byte>.Empty, _characters, true, out _, out int characters, out _);
            _text.Append(_characters, 0, characters);
            _text.Append('\n');
            return _text.ToString();
        }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }

    internal string[] Compare(string? previous)
    {
        SchemaSummary? before = null;
        try
        {
            before = previous == null ? null : JsonSerializer.Deserialize<SchemaSummary>(previous);
        }
        catch (JsonException)
        {
            // An old/corrupt summary is unknown, not evidence of an unchanged schema.
        }

        var changes = new List<string>();
        if (before is not { SupportedFormat: true, Tables: not null } || before.Tables.Any(t => t == null || t.Keys == null || t.Groups == null || t.Fields == null || t.Fields.Any(f => f == null)))
        {
            changes.Add(ConfigText.Get("首次建立结构摘要；旧结构未知，请同时检查文件变化。", "Establishing the schema baseline; previous schema is unknown. Review file changes too."));
            changes.AddRange(Tables.Select(t => ConfigText.Get($"表 {t.Name}：{t.Fields.Length} 个字段，主键 {string.Join(", ", t.Keys)}", $"Table {t.Name}: {t.Fields.Length} fields; key {string.Join(", ", t.Keys)}")));
            return changes.ToArray();
        }

        if (before.Target != Target)
        {
            changes.Add(ConfigText.Get($"目标端：{before.Target} → {Target}", $"Target: {before.Target} → {Target}"));
        }

        foreach (TableSummary old in before.Tables.Where(t => Tables.All(n => n.Name != t.Name)))
        {
            changes.Add(ConfigText.Get($"删除表：{old.Name}", $"Removed table: {old.Name}"));
        }

        foreach (TableSummary table in Tables)
        {
            TableSummary? old = before.Tables.FirstOrDefault(t => t.Name == table.Name);
            if (old == null)
            {
                changes.Add(ConfigText.Get($"新增表：{table.Name}", $"Added table: {table.Name}"));
                continue;
            }

            if (!old.Keys.SequenceEqual(table.Keys))
            {
                changes.Add($"{table.Name}: " + ConfigText.Get("主键", "Primary key") + $" [{string.Join(", ", old.Keys)}] → [{string.Join(", ", table.Keys)}]");
            }

            if (!old.Groups.SequenceEqual(table.Groups))
            {
                changes.Add($"{table.Name}: " + ConfigText.Get("分组索引变化", "Group indexes changed"));
            }

            foreach (FieldSummary field in old.Fields.Where(f => table.Fields.All(n => n.Name != f.Name)))
            {
                changes.Add(ConfigText.Get($"删除字段：{table.Name}.{field.Name}", $"Removed field: {table.Name}.{field.Name}"));
            }

            foreach (FieldSummary field in table.Fields)
            {
                FieldSummary? previousField = old.Fields.FirstOrDefault(f => f.Name == field.Name);
                if (previousField == null)
                {
                    changes.Add(ConfigText.Get($"新增字段：{table.Name}.{field.Name} ({field.Type})", $"Added field: {table.Name}.{field.Name} ({field.Type})"));
                }
                else if (previousField != field)
                {
                    changes.Add($"{table.Name}.{field.Name}: {previousField.Type}/{previousField.Target} → {field.Type}/{field.Target}");
                }
            }

            if (old.SchemaHash != table.SchemaHash && old.Fields.SequenceEqual(table.Fields) && old.Keys.SequenceEqual(table.Keys))
            {
                changes.Add($"{table.Name}: " + ConfigText.Get("成员布局、业务类型或序列化结构变化", "Member layout, custom type or serialization schema changed"));
            }
        }

        return changes.ToArray();
    }
}
