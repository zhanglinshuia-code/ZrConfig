using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using ClosedXML.Excel;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

internal static class ExcelStructuredDataLoader
{
    private sealed class Group(int first)
    {
        public int First { get; } = first;
        public List<int> Rows { get; } = [first];

        public bool Valid = true;
    }

    public static LoadedTable Read(TableSchema schema,
        ITableCodec codec,
        ConditionalWeakTable<object, RowSource> sources,
        DiagnosticCollector collector,
        XLWorkbook workbook, WorkflowOperation? operation = null)
    {
        IXLWorksheet sheet = workbook.Worksheet(schema.SheetName);
        IList rows = codec.CreateRows();
        ConfigValueParser parser = new();
        bool complete = true;
        bool multiRow = schema.InputFields.Any(f => f.Layout?.HasLists == true);
        IReadOnlyList<FieldSchema> keyFields = schema.KeyFields;
        var keys = new TableKeyTracker(schema, collector);
        var groups = new List<Group>();
        bool integerKey = keyFields.Count == 1 && keyFields[0].Type.ClrType == typeof(int);
        Dictionary<int, Group>? idGroups = integerKey ? new Dictionary<int, Group>() : null;
        Dictionary<TableKeyValues, Group>? keyedGroups = integerKey ? null : new Dictionary<TableKeyValues, Group>();
        Group? previous = null;
        int[] physicalColumns = schema.InputFields.SelectMany(Columns).Distinct().ToArray();
        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? schema.DataStartRow - 1;
        for (int number = schema.DataStartRow; number <= lastRow; number++)
        {
            operation?.Checkpoint();
            if ((number - schema.DataStartRow) % 256 == 0)
            {
                operation?.Report("reading", schema.Name, number - schema.DataStartRow, lastRow - schema.DataStartRow + 1);
            }
            IXLRow row = sheet.Row(number);
            if (physicalColumns.All(c => row.Cell(c).IsEmpty()))
            {
                continue;
            }

            if (!multiRow)
            {
                groups.Add(new Group(number));
                continue;
            }

            string? integerText = integerKey ? row.Cell(keyFields[0].Column).GetInputText().Trim() : null;
            string[]? texts = integerKey ? null : keyFields.Select(f => row.Cell(f.Column).GetInputText()).ToArray();
            Group? group;
            if (integerKey ? integerText!.Length == 0 : texts!.All(string.IsNullOrWhiteSpace))
            {
                group = previous;
                if (group == null)
                {
                    Fail("ROW_PARENT_MISSING",
                        number,
                        keyFields[0].Name,
                        keyFields[0].Column,
                        ConfigText.Get("多行列表的首行必须填写完整键；键全部留空只能续接上一条有效主记录。",
                        "The first row requires a complete key; only a fully blank key can continue the previous valid record."));
                    continue;
                }
            }
            else
            {
                if (integerKey)
                {
                    if (!int.TryParse(integerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                    {
                        Fail("CELL_PARSE",
                            number,
                            keyFields[0].Name,
                            keyFields[0].Column,
                            ConfigText.Get("键必须是有效的 int。",
                            "The key must be a valid int."));
                        previous = null;
                        continue;
                    }

                    if (!idGroups!.TryGetValue(value, out group))
                    {
                        idGroups.Add(value, group = new Group(number));
                    }
                }
                else
                {
                    object[] values = new object[keyFields.Count];
                    bool validKey = true;
                    for (int i = 0; i < keyFields.Count; i++)
                    {
                        FieldSchema field = keyFields[i];
                        try
                        {
                            if (string.IsNullOrWhiteSpace(texts![i]))
                            {
                                throw new FormatException(ConfigText.Get("联合键必须全部填写或全部留空。",
                                    "Fill every key component, or leave the entire key blank for a continuation."));
                            }

                            values[i] = parser.Parse(texts[i], field.Type, false, field.Name)!;
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            validKey = false;
                            Fail("CELL_PARSE", number, field.Name, field.Column, ex.Message);
                        }
                    }

                    if (!validKey)
                    {
                        previous = null;
                        continue;
                    }

                    var key = new TableKeyValues(values);
                    if (!keyedGroups!.TryGetValue(key, out group))
                    {
                        keyedGroups.Add(key, group = new Group(number));
                    }
                }

                if (group.First == number)
                {
                    groups.Add(group);
                    previous = group;
                    continue;
                }
            }

            previous = group;
            group.Rows.Add(number);
            // 主记录只由首次出现的行定义；续行留空或原样重复，禁止静默覆盖。
            foreach (FieldSchema field in schema.Fields.Where(f => !keyFields.Contains(f)))
            {
                foreach (int column in field.Layout?.ParentColumns ?? Columns(field))
                {
                    string text = row.Cell(column).GetInputText();
                    if (string.IsNullOrWhiteSpace(text) || text == sheet.Cell(group.First, column).GetInputText())
                    {
                        continue;
                    }

                    group.Valid = false;
                    Fail("ROW_PARENT_CONFLICT",
                        number,
                        sheet.Cell(1,
                        column)
                        .GetInputText(),
                        column,
                        ConfigText.Get("同一键的主字段与首行不同；续行请留空或填写与首行完全相同的值。",
                        "The parent field conflicts with the first row of this key; leave continuation cells blank or repeat the exact first-row value."));
                }
            }
        }

        var columns = schema.Fields.ToDictionary(f => f.Name, f => f.Column);
        foreach (Group group in groups)
        {
            operation?.Checkpoint();
            bool valid = group.Valid;
            var excelRows = new IXLRow[group.Rows.Count];
            for (int i = 0; i < excelRows.Length; i++)
            {
                excelRows[i] = sheet.Row(group.Rows[i]);
            }

            object[] values = new object[schema.Fields.Count];
            var locations = new Dictionary<string, CellSource>(StringComparer.Ordinal);
            var empty = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < schema.Fields.Count; i++)
            {
                FieldSchema field = schema.Fields[i];
                locations[field.Name] = new CellSource(group.First, field.Column);
                try
                {
                    if (field.Layout is { } layout)
                    {
                        values[i] = layout.Read(excelRows, field.Name, parser, locations, empty)!;
                    }
                    else
                    {
                        string text = sheet.Cell(group.First, field.Column).GetInputText();
                        if (string.IsNullOrWhiteSpace(text))
                        {
                            empty.Add(field.Name);
                        }

                        values[i] = parser.Parse(text, field.Type, field.Json, field.Name)!;
                    }

                    if (values[i] == null || (field.Type.ReferenceTable != null && (int)values[i] == 0))
                    {
                        empty.Add(field.Name);
                    }
                }
                catch (ExcelLayoutException ex)
                {
                    valid = false;
                    Fail(ex.Code, ex.Row, ex.Field, ex.Column, ex.Message);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    valid = false;
                    Fail("CELL_PARSE", group.First, field.Name, field.Column, ex.Message);
                }
            }

            if (!valid)
            {
                continue;
            }

            if (!keys.Validate(values, group.First, empty))
            {
                complete = false;
                continue;
            }

            object instance = codec.CreateRow(values);
            rows.Add(instance);
            sources.Add(instance, new RowSource(schema.FilePath, schema.Name, schema.SheetName, group.First, columns, empty, locations));
        }

        return new LoadedTable(schema, codec, rows, complete);
        void Fail(string code, int row, string field, int column, string message)
        {
            complete = false;
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                code,
                message,
                schema.FilePath,
                schema.Name,
                schema.SheetName,
                row,
                field,
                column));
        }
    }

    private static IEnumerable<int> Columns(FieldSchema field) => field.Layout?.Columns.Select(c => c.Column) ?? [field.Column];
}
