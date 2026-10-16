using System.Collections;
using System.Runtime.CompilerServices;
using ClosedXML.Excel;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

public sealed class LoadedTable
{
    public TableSchema Schema
    {
        get;
    }
    public ITableCodec Codec
    {
        get;
    }
    public IList Rows
    {
        get;
    }
    public bool IsComplete
    {
        get; internal set;
    }

    public LoadedTable(TableSchema schema, ITableCodec codec, IList rows, bool complete)
    {
        Schema = schema;
        Codec = codec;
        Rows = rows;
        IsComplete = complete;
    }
}

public sealed class LoadResult
{
    public IReadOnlyList<LoadedTable> Tables
    {
        get;
    }
    public ConditionalWeakTable<object, RowSource> Sources
    {
        get;
    }

    public LoadResult(IReadOnlyList<LoadedTable> tables, ConditionalWeakTable<object, RowSource> sources)
    {
        Tables = tables;
        Sources = sources;
    }
}

public static class ExcelDataLoader
{
    public static LoadResult Load(IReadOnlyList<TableSchema> schemas,
        IReadOnlyDictionary<string, ITableCodec> codecs,
        DiagnosticCollector collector)
    {
        List<LoadedTable> loaded = new(schemas.Count);
        ConditionalWeakTable<object, RowSource> sources = new();
        foreach (TableSchema schema in schemas)
        {
            ReadInto(schema, null, codecs, sources, collector, loaded);
        }

        ResolveReferences(loaded, sources, collector);
        return new LoadResult(loaded, sources);
    }

    internal static LoadResult Load(string folder,
        ConfigTypeRegistry registry,
        IReadOnlyDictionary<string, ITableCodec> codecs,
        DiagnosticCollector collector,
        out IReadOnlyList<TableSchema> schemas,
        string? tableDefinitionsPath = null,
        ConfigTarget target = ConfigTarget.Client,
        WorkflowOperation? operation = null)
    {
        List<LoadedTable> loaded = new();
        ConditionalWeakTable<object, RowSource> sources = new();
        schemas = SchemaLoader.Load(folder,
            collector,
            registry,
            (schema,
            workbook) => ReadInto(schema,
            workbook,
            codecs,
            sources,
            collector,
            loaded, operation),
            tableDefinitionsPath,
            target, operation: operation);
        ResolveReferences(loaded, sources, collector, operation);
        return new LoadResult(loaded, sources);
    }

    private static void ResolveReferences(List<LoadedTable> tables,
        ConditionalWeakTable<object, RowSource> sources,
        DiagnosticCollector collector, WorkflowOperation? operation = null)
    {
        if (!tables.Any(t => t.Codec is IConfigReferenceCodec))
        {
            return;
        }

        var resolver = new ConfigReferenceResolver(tables.Select(t => new KeyValuePair<Type, IList>(t.Codec.RowType, t.Rows)));
        var byName = tables.ToDictionary(t => t.Schema.Name, StringComparer.Ordinal);
        HashSet<ConfigReferenceException>? reported = null;
        foreach (LoadedTable table in tables)
        {
            if (table.Codec is not IConfigReferenceCodec references)
            {
                continue;
            }

            foreach (IConfigRow row in table.Rows)
            {
                operation?.Checkpoint();
                try
                {
                    references.ResolveReferences(row, resolver);
                }
                catch (ConfigReferenceException ex)
                {
                    table.IsComplete = false;
                    if (!(reported ??= new()).Add(ex))
                    {
                        continue;
                    }

                    sources.TryGetValue(ex.Row, out RowSource? source);
                    if (source != null && byName.TryGetValue(source.Table, out LoadedTable? invalidTable))
                    {
                        invalidTable.IsComplete = false;
                    }

                    int excelRow = source?.ExcelRow ?? 0;
                    int column = source?.Columns.GetValueOrDefault(ex.Field) ?? 0;
                    if (source?.Locations?.TryGetValue(ex.Field, out CellSource location) == true)
                    {
                        excelRow = location.Row;
                        column = location.Column;
                    }

                    collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                        "TABLE_REFERENCE",
                        ex.Message,
                        source?.File ?? table.Schema.FilePath,
                        source?.Table ?? table.Schema.Name,
                        source?.Sheet ?? table.Schema.SheetName,
                        excelRow,
                        ex.Field,
                        column));
                }
            }
        }

        // 引用链上的任一表不完整，均不能向业务规则提供半关联对象图。
        bool changed;
        do
        {
            changed = false;
            foreach (LoadedTable table in tables.Where(t => t.IsComplete))
            {
                if (table.Schema.Fields.Any(f => f.Type.ReferenceTable is { } target && (!byName.TryGetValue(target,
                    out LoadedTable? dependency) || !dependency.IsComplete)))
                {
                    table.IsComplete = false;
                    changed = true;
                }
            }
        }
        while (changed);
    }

    private static void ReadInto(TableSchema schema,
        XLWorkbook? workbook,
        IReadOnlyDictionary<string, ITableCodec> codecs,
        ConditionalWeakTable<object, RowSource> sources,
        DiagnosticCollector collector,
        List<LoadedTable> loaded, WorkflowOperation? operation = null)
    {
        if (!codecs.TryGetValue(schema.Name, out ITableCodec? codec))
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                "CODEC_MISSING",
                ConfigText.Get("缺少生成的编解码器；请在指定模型目录重新生成配表代码。",
                "Generated codec is not available; regenerate table code in the selected model directory."),
                schema.FilePath,
                schema.Name,
                schema.SheetName));
            return;
        }

        if (!string.Equals(codec.SchemaHash, schema.SchemaHash, StringComparison.Ordinal))
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                "SCHEMA_MISMATCH",
                ConfigText.Get("Excel 表结构与模型不一致；请在指定模型目录重新生成配表代码。",
                "Excel structure differs from models. Regenerate table code in the selected model directory."),
                schema.FilePath,
                schema.Name,
                schema.SheetName));
            return;
        }

        try
        {
            if (workbook != null)
            {
                loaded.Add(ReadTable(schema, codec, sources, collector, workbook, operation));
            }
            else
            {
                using XLWorkbook opened = ExcelWorkbookInput.Open(schema.FilePath);
                loaded.Add(ReadTable(schema, codec, sources, collector, opened, operation));
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                "TABLE_READ",
                ConfigText.Get($"读取表数据失败。详情：{ex.Message}",
                $"Unable to read table data. Details: {ex.Message}"),
                schema.FilePath,
                schema.Name,
                schema.SheetName));
        }
    }

    private static LoadedTable ReadTable(TableSchema schema,
        ITableCodec codec,
        ConditionalWeakTable<object, RowSource> sources,
        DiagnosticCollector collector,
        XLWorkbook workbook, WorkflowOperation? operation = null)
    {
        if (schema.InputFields.Any(f => f.Layout != null))
        {
            return ExcelStructuredDataLoader.Read(schema, codec, sources, collector, workbook, operation);
        }

        IList rows = codec.CreateRows();
        ConfigValueParser parser = new();
        var columns = schema.Fields.ToDictionary(x => x.Name, x => x.Column);
        bool complete = true;
        var keys = new TableKeyTracker(schema, collector);
        IXLWorksheet sheet = workbook.Worksheet(schema.SheetName);
        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? schema.DataStartRow - 1;
        for (int rowNumber = schema.DataStartRow; rowNumber <= lastRow; rowNumber++)
        {
            operation?.Checkpoint();
            if ((rowNumber - schema.DataStartRow) % 256 == 0)
            {
                operation?.Report("reading", schema.Name, rowNumber - schema.DataStartRow, lastRow - schema.DataStartRow + 1);
            }
            IXLRow excelRow = sheet.Row(rowNumber);
            if (IsEmptyRow(excelRow, schema.Fields))
            {
                continue;
            }

            object[] values = new object[schema.Fields.Count];
            List<string>? emptyFields = null;
            bool valid = true;
            for (int index = 0; index < schema.Fields.Count; index++)
            {
                FieldSchema field = schema.Fields[index];
                IXLCell cell = excelRow.Cell(field.Column);
                string text = cell.GetInputText();
                string raw = text.Trim();
                if (raw.Length == 0)
                {
                    (emptyFields ??= new List<string>()).Add(field.Name);
                }

                try
                {
                    values[index] = parser.Parse(text, field.Type, field.Json, field.Name)!;
                    if (raw.Length != 0 && (values[index] == null || (field.Type.ReferenceTable != null && (int)values[index] == 0)))
                    {
                        (emptyFields ??= new List<string>()).Add(field.Name);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Fail("CELL_PARSE",
                        ConfigText.Get($"无法将“{raw}”解析为 {field.TypeName}。详情：{ex.Message}",
                        $"Unable to parse '{raw}' as {field.TypeName}: {ex.Message}"),
                        field);
                }
            }

            if (valid && !keys.Validate(values, rowNumber, emptyFields))
            {
                valid = false;
                complete = false;
            }

            if (valid)
            {
                object row = codec.CreateRow(values);
                rows.Add(row);
                sources.Add(row, new RowSource(schema.FilePath, schema.Name, schema.SheetName, rowNumber, columns, emptyFields));
            }

            void Fail(string code, string message, FieldSchema field)
            {
                valid = false;
                complete = false;
                collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                    code,
                    message,
                    schema.FilePath,
                    schema.Name,
                    schema.SheetName,
                    rowNumber,
                    field.Name,
                    field.Column));
            }
        }

        return new LoadedTable(schema, codec, rows, complete);
    }

    private static bool IsEmptyRow(IXLRow row, IReadOnlyList<FieldSchema> fields)
    {
        for (int i = 0; i < fields.Count; i++)
        {
            if (!row.Cell(fields[i].Column).IsEmpty())
            {
                return false;
            }
        }

        return true;
    }
}
