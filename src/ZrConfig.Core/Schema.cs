using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

public sealed record FieldSchema(string Name, string TypeName, int Column, string Description)
{
    public ConfigTarget Targets { get; init; } = ConfigTarget.Both;
    internal ConfigType Type { get; init; } = null!;
    internal bool Json
    {
        get; init;
    }
    internal ExcelColumnLayout? Layout
    {
        get; init;
    }
}

public sealed record TableSchema(string Name, string FilePath, string SheetName, IReadOnlyList<FieldSchema> Fields, string SchemaHash)
{
    public IReadOnlyList<FieldSchema> KeyFields { get; init; } = [];
    public IReadOnlyList<TableGroupSchema> Groups { get; init; } = [];
    public int DataStartRow { get; init; } = 6;
    public ConfigTarget Target { get; init; } = ConfigTarget.Client;
    internal IReadOnlyList<FieldSchema> SourceFields { get; init; } = [];
    internal IReadOnlyList<FieldSchema> InputFields => SourceFields.Count == 0 ? Fields : SourceFields;
    internal bool HasIntegerId => Fields.Any(f => f.Name == "Id" && f.Type.Kind == ConfigTypeKind.Primitive && f.Type.ClrType == typeof(int));
}

/// <summary>
/// Excel 格式（所有名称不以 # 开头的工作表）：第 1 行字段名，第 2 行 C# 类型（展开列声明根类型），
/// 第 3 行标记主键，第 4 行说明，第 5 行声明所属端，第 6 行起为数据。
/// 主键行只填写“主键”或留空，所有标记字段共同唯一，各字段自动建立分组。
/// 每个有效工作表使用自己的 Sheet 名作为表名，与工作簿名称及工作表数量无关。
/// </summary>
public static class SchemaLoader
{
    public static IReadOnlyList<TableSchema> Load(string folder,
        DiagnosticCollector collector,
        ConfigTarget target = ConfigTarget.Client) => Load(folder,
        collector,
        new ConfigTypeRegistry(null),
        target: target);
    public static IReadOnlyList<TableSchema> Load(string folder,
        DiagnosticCollector collector,
        string tableDefinitionsPath,
        ConfigTarget target = ConfigTarget.Client) => Load(folder,
        collector,
        new ConfigTypeRegistry(null),
        tableDefinitionsPath: tableDefinitionsPath,
        target: target);
    internal static IReadOnlyList<TableSchema> Load(string folder,
        DiagnosticCollector collector,
        ConfigTypeRegistry registry,
        Action<TableSchema, XLWorkbook>? readData = null,
        string? tableDefinitionsPath = null,
        ConfigTarget target = ConfigTarget.Client,
        GenerationInputSnapshot? inputs = null,
        WorkflowOperation? operation = null, GenerationSourceContract? sourceContract = null)
    {
        sourceContract ??= registry.SourceContract;
        _ = ConfigTargets.Marker(target);
        List<TableSchema> result = new();
        Dictionary<string, TableDefinition> definitions;
        try
        {
            definitions = tableDefinitionsPath == null ? registry.TableDefinitions : inputs?.ReadDefinitions(tableDefinitionsPath) ?? TableDefinition.Read(tableDefinitionsPath);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error, "TABLE_DEFINITION", ex.Message, file: tableDefinitionsPath ?? ""));
            return result;
        }

        if (!Directory.Exists(folder))
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                "INPUT_MISSING",
                ConfigText.Get("Excel 输入目录不存在。",
                "Excel directory not found."),
                file: folder));
            return result;
        }

        string[] files;
        try
        {
            files = ExcelWorkbookInput.FindFiles(folder);
            inputs?.RecordFiles(files);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                "INPUT_SCAN",
                ConfigText.Get($"无法完整扫描 Excel 输入目录及其子目录。详情：{ex.Message}",
                $"Unable to scan the Excel directory and all subdirectories. Details: {ex.Message}"),
                file: folder));
            return result;
        }

        if (files.Length == 0)
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                "INPUT_EMPTY",
                ConfigText.Get("输入目录及其子目录中没有找到 .xlsx 或 .xls 文件。",
                "No .xlsx or .xls files found in the input directory or its subdirectories."),
                file: folder));
        }

        HashSet<string> tableNames = new(StringComparer.OrdinalIgnoreCase);
        // 先发现所有表名，使跨工作表、跨工作簿的引用不依赖读取顺序。
        Dictionary<string, string[]> sheetNames = new(StringComparer.Ordinal);
        foreach (string file in files)
        {
            operation?.Report("reading", Path.GetFileName(file), Array.IndexOf(files, file), files.Length);
            try
            {
                string[] names = ExcelWorkbookInput.GetSheetNames(file);
                sheetNames.Add(file, names);
                registry.RegisterTables(names.Select(sourceContract.Table));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                    "EXCEL_OPEN",
                    ConfigText.Get($"无法读取 Excel 文件。详情：{ex.Message}",
                    $"Unable to read the Excel file. Details: {ex.Message}"),
                    file: file));
            }
        }

        foreach (string file in files)
        {
            operation?.Report("reading", Path.GetFileName(file), Array.IndexOf(files, file), files.Length);
            if (!sheetNames.TryGetValue(file, out string[]? names) || names.Length == 0)
            {
                continue;
            }

            try
            {
                using XLWorkbook workbook = ExcelWorkbookInput.Open(file);
                inputs?.RecordWorkbook(file, workbook, names);
                foreach (string name in names)
                {
                    operation?.Report("reading", Path.GetFileName(file) + " / " + name);
                    string table = sourceContract.Table(name);
                    definitions.TryGetValue(table, out TableDefinition? definition);
                    TableSchema? schema = Read(file, table, workbook.Worksheet(name), collector, registry, definition, sourceContract);
                    if (schema == null)
                    {
                        continue;
                    }

                    if (schema.KeyFields.Any(f => f.Targets != ConfigTarget.Both))
                    {
                        foreach (FieldSchema field in schema.KeyFields.Where(f => f.Targets != ConfigTarget.Both))
                        {
                            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                                "SCHEMA_TARGET",
                                ConfigText.Get("主键必须属于两端，请明确填写 CS。",
                                "Key fields must be shared: explicitly use CS."),
                                file,
                                table,
                                name,
                                5,
                                field.Name,
                                field.Column));
                        }

                        continue;
                    }

                    schema = SelectTarget(sourceContract.Order(schema), registry, target);
                    if (!tableNames.Add(schema.Name))
                    {
                        collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                            "TABLE_DUPLICATE",
                            ConfigText.Get("表名不能重复（不区分大小写）。",
                            "Table names must be unique, ignoring case."),
                            file,
                            schema.Name,
                            schema.SheetName));
                    }
                    else
                    {
                        result.Add(schema);
                        readData?.Invoke(schema, workbook);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                    "EXCEL_OPEN",
                    ConfigText.Get($"无法读取 Excel 文件。详情：{ex.Message}",
                    $"Unable to read the Excel file. Details: {ex.Message}"),
                    file: file));
            }
        }

        try { sourceContract.CheckUsed(result); }
        catch(InvalidDataException error) { collector.Add(new Diagnostic(DiagnosticSeverity.Error,"SOURCE_BINDING",error.Message)); }
        foreach (string missing in definitions.Keys.Except(result.Select(t => t.Name), StringComparer.Ordinal))
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                "TABLE_DEFINITION",
                ConfigText.Get("键定义未匹配有效表：",
                "Key definition has no matching valid table: ") + missing,
                file: tableDefinitionsPath ?? "",
                table: missing));
        }

        foreach (TableSchema schema in result)
        {
            foreach (FieldSchema field in schema.Fields.Where(f => f.Type.ReferenceTable != null))
            {
                if (result.FirstOrDefault(t => t.Name == field.Type.ReferenceTable) is { HasIntegerId: false })
                {
                    collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                        "SCHEMA_REFERENCE",
                        ConfigText.Get("直接表引用需要目标表有唯一的 int Id；联合键表请保存各键值并调用生成的 Get/TryGet。",
                        "Direct table references require a unique int Id on the target; use key fields and generated Get/TryGet for composite keys."),
                        schema.FilePath,
                        schema.Name,
                        schema.SheetName,
                        2,
                        field.Name,
                        field.Column));
                }
            }
        }

        return result;
    }

    private static TableSchema? Read(string path,
        string table,
        IXLWorksheet sheet,
        DiagnosticCollector collector,
        ConfigTypeRegistry registry,
        TableDefinition? definition, GenerationSourceContract sourceContract)
    {
        if (!Regex.IsMatch(table, @"^[A-Za-z_][A-Za-z_0-9]*$"))
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                "SCHEMA_NAME",
                ConfigText.Get("表名必须是有效的 C# 标识符，仅使用英文字母、数字和下划线。",
                "Table name must be a valid C# identifier (ASCII letters, digits, underscore)."),
                path,
                table,
                sheet.Name));
            return null;
        }

        int lastColumn = sheet.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
        if (lastColumn == 0)
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                "SCHEMA_EMPTY",
                ConfigText.Get("字段名行为空。",
                "The field-name row is empty."),
                path,
                table,
                sheet.Name,
                1));
            return null;
        }

        // 所属端行始终占第 5 行，数据固定从第 6 行开始。
        const int headerRows = 5;
        int markerColumns = Math.Max(lastColumn, sheet.Row(3).LastCellUsed()?.Address.ColumnNumber ?? 0);
        string[] markers = Enumerable.Range(1, markerColumns).Select(c => sheet.Cell(3, c).GetInputText().Trim()).ToArray();
        bool invalidMarkers = false;
        for (int col = 1; col <= markers.Length; col++)
        {
            if (markers[col - 1] is "" or "主键")
            {
                continue;
            }

            invalidMarkers = true;
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                "SCHEMA_KEY",
                ConfigText.Get($"主键行不能填写“{markers[col - 1]}”；第 3 行只允许“主键”或留空，说明请放在第 4 行，第 5 行保留所属端，第 6 行起为数据。",
                $"Invalid key marker '{markers[col - 1]}'. Row 3 accepts only 主键 or blank cells; descriptions belong in row 4, targets in row 5, and data starts in row 6."),
                path,
                table,
                sheet.Name,
                3,
                sheet.Cell(1,
                col)
                .GetInputText()
                .Trim(),
                col));
        }

        if (invalidMarkers)
        {
            return null;
        }

        if (!markers.Contains("主键", StringComparer.Ordinal))
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                "SCHEMA_KEY",
                ConfigText.Get("第 3 行至少需要一列标记“主键”，不能留空或拼错。工具不会自动补键或改写数据。",
                "Row 3 must contain at least one 主键 marker. The tool never infers keys or rewrites data."),
                path,
                table,
                sheet.Name,
                3));
            return null;
        }

        const int DescriptionRow = 4;
        bool invalid = false;
        int targetColumns = sheet.Row(5).LastCellUsed()?.Address.ColumnNumber ?? 0;
        for (int column = lastColumn + 1; column <= targetColumns; column++)
        {
            if (string.IsNullOrWhiteSpace(sheet.Cell(5, column).GetInputText()))
            {
                continue;
            }

            invalid = true;
            collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                "SCHEMA_TARGET",
                ConfigText.Get("所属端标记没有对应的字段。",
                "The target marker has no matching field."),
                path,
                table,
                sheet.Name,
                5,
                "",
                column));
        }

        List<FieldSchema> fields = new();
        Dictionary<string, List<ExcelColumn>> groups = new(StringComparer.Ordinal);
        HashSet<string> usedNames = new(StringComparer.Ordinal);
        for (int col = 1; col <= lastColumn; col++)
        {
            string name = sourceContract.Header(table,sheet.Cell(1, col).GetInputText().Trim());
            string typeName = sourceContract.TypeName(sheet.Cell(2, col).GetInputText().Trim());
            string description = sheet.Cell(DescriptionRow, col).GetInputText();
            Match header = Regex.Match(name,
                @"^(?<root>[A-Za-z_][A-Za-z_0-9]*)(?<many>\[\])?(?:\.(?<path>(?:[A-Za-z_][A-Za-z_0-9]*(?:\[\])?|\$type|\$order)(?:\.(?:[A-Za-z_][A-Za-z_0-9]*(?:\[\])?|\$type|\$order))*))?$");
            if (!header.Success || header.Groups["root"].Value == table + "Config" || header.Groups["root"].Value.StartsWith("__zr",
                StringComparison.Ordinal) || (header.Groups["many"].Success && !header.Groups["path"].Success))
            {
                Add("SCHEMA_FIELD", ConfigText.Get($"字段名“{name}”无效。", $"Invalid field name '{name}'."), 1);
                continue;
            }

            if (!usedNames.Add(name))
            {
                Add("SCHEMA_DUPLICATE", ConfigText.Get($"字段“{name}”重复。", $"Duplicate field '{name}'."), 1);
                continue;
            }

            string root = header.Groups["root"].Value;
            if (!groups.TryGetValue(root, out List<ExcelColumn>? group))
            {
                groups.Add(root, group = []);
            }

            group.Add(new ExcelColumn(name, header.Groups["path"].Value, col, typeName, description, header.Groups["many"].Success));
            continue;
            void Add(string code, string msg, int row)
            {
                invalid = true;
                collector.Add(new Diagnostic(DiagnosticSeverity.Error, code, msg, path, table, sheet.Name, row, name, col));
            }
        }

        foreach ((string name, List<ExcelColumn> group) in groups)
        {
            ExcelColumn first = group[0];
            ExcelColumn declaration = group.FirstOrDefault(c => c.TypeName.Length != 0) ?? first;
            ConfigTarget? declaredTargets = null;
            bool invalidTargets = false;
            foreach (ExcelColumn column in group)
            {
                string marker = sheet.Cell(5, column.Column).GetInputText().Trim().ToUpperInvariant();
                if (marker.Length == 0)
                {
                    continue;
                }

                ConfigTarget? current = marker switch
                {
                    "C" => ConfigTarget.Client,
                    "S" => ConfigTarget.Server,
                    "CS" => ConfigTarget.Both,
                    _ => null
                };
                if (current == null || (declaredTargets != null && current != declaredTargets))
                {
                    invalid = true;
                    invalidTargets = true;
                    collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                        "SCHEMA_TARGET",
                        ConfigText.Get("第 5 行必须明确声明所属端 C、S 或 CS，数据从第 6 行开始；同一字段的展开列必须一致，留空列只继承同组已填写的标记。",
                        "Explicitly declare C, S or CS in row 5; data starts in row 6. Expanded columns of one field must agree; blank cells only inherit an explicit marker in their group."),
                        path,
                        table,
                        sheet.Name,
                        5,
                        column.Name,
                        column.Column));
                }
                else
                {
                    declaredTargets = current;
                }
            }

            if (invalidTargets)
            {
                continue;
            }

            if (declaredTargets is not ConfigTarget targets)
            {
                invalid = true;
                collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                    "SCHEMA_TARGET",
                    ConfigText.Get("第 5 行缺少所属端，请明确填写 C、S 或 CS；展开字段至少在同组一列声明所属端，不再将留空视为 CS。",
                    "Missing target in row 5: explicitly enter C, S or CS. Expanded fields require a marker in at least one column of their group; blank no longer means CS."),
                    path,
                    table,
                    sheet.Name,
                    5,
                    first.Name,
                    first.Column));
                continue;
            }

            try
            {
                ConfigType type = registry.Parse(declaration.TypeName, out bool json);
                bool expanded = first.Path.Length != 0;
                if (group.Any(c => (c.Path.Length != 0) != expanded || c.Many != first.Many))
                {
                    throw new ExcelLayoutException(first.Name,
                        first.Column,
                        1,
                        ConfigText.Get("同一字段不能混用单格、结构分列或多行数组填写。",
                        "A field cannot mix single-cell, expanded-object and multi-row layouts."));
                }

                foreach (ExcelColumn column in group.Where(c => c.TypeName.Length != 0))
                {
                    ConfigType declared = registry.Parse(column.TypeName, out bool declaredJson);
                    if (declared.ClrType != type.ClrType || declared.ReferenceTable != type.ReferenceTable || declaredJson != json)
                    {
                        throw new ExcelLayoutException(column.Name,
                            column.Column,
                            2,
                            ConfigText.Get("同一组的类型行必须声明相同的根类型，其余列也可以留空。",
                            "Type declarations in one group must name the same root type; other type cells may be blank."));
                    }
                }

                if (expanded && json)
                {
                    throw new ExcelLayoutException(declaration.Name,
                        declaration.Column,
                        2,
                        ConfigText.Get("分列填写请使用 C# 根类型，不要使用 json<T>。",
                        "Expanded columns require the C# root type without json<T>."));
                }

                if (expanded && type.ReferenceTable != null)
                {
                    throw new ExcelLayoutException(declaration.Name, declaration.Column, 2, "表引用使用单格填写目标 Id，不展开目标表的成员。");
                }

                ExcelColumnLayout? layout = expanded ? new ExcelColumnLayout(type, group) : null;
                fields.Add(new FieldSchema(name,
                    declaration.TypeName,
                    first.Column,
                    first.Description)
                {
                    Type = type,
                    Json = json,
                    Layout = layout,
                    Targets = targets
                });
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                invalid = true;
                var layout = ex as ExcelLayoutException;
                collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                    layout == null ? "SCHEMA_TYPE" : "SCHEMA_LAYOUT",
                    ex.Message,
                    path,
                    table,
                    sheet.Name,
                    layout?.Row ?? 2,
                    layout?.Field ?? declaration.Name,
                    layout?.Column ?? declaration.Column));
            }
        }

        if (fields.Count == 0 || invalid)
        {
            return null;
        }

        // 填写布局不改变运行时结构：JSON、分列和多行列表使用同一份模型与二进制协议。
        string signature = table + "\n" + string.Join("|",
            fields.Select(x => $"{x.Name}:{ConfigTypeRegistry.TypeIdentity(x.Type)}")) + "\nBinary/" + registry.BinaryVersion + "\n" + ConfigTypeRegistry.Signature(fields.Select(f => f.Type));
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(signature)));
        var schema = new TableSchema(table, path, sheet.Name, fields, hash)
        {
            DataStartRow = headerRows + 1
        };
        var key = new List<string>();
        for (int col = 1; col <= markers.Length; col++)
        {
            if (markers[col - 1] != "主键")
            {
                continue;
            }

            string name = sourceContract.Header(table,sheet.Cell(1, col).GetInputText().Trim());
            FieldSchema? field = fields.FirstOrDefault(f => f.Name == name && f.Column == col);
            if (field == null || !TableDefinition.CanIndex(field))
            {
                collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                    "SCHEMA_KEY",
                    ConfigText.Get("主键只能标在顶层非可空标量或枚举字段上，不能标记成员路径、集合、JSON 或表引用。",
                    "Keys require top-level non-null scalar or enum fields, without member paths, collections, JSON or references."),
                    path,
                    table,
                    sheet.Name,
                    3,
                    name,
                    col));
                invalid = true;
            }
            else
            {
                key.Add(name);
            }
        }

        if (invalid)
        {
            return null;
        }

        try
        {
            var indexGroups = key.ToDictionary(n => "By" + n, n => new[] { n }, StringComparer.Ordinal);
            if (definition != null)
            {
                foreach (KeyValuePair<string, string[]> group in definition.Groups)
                {
                    if (indexGroups.TryGetValue(group.Key, out string[]? existingFields) && !existingFields.SequenceEqual(group.Value))
                    {
                        throw new FormatException($"分组名称与自动主键分组冲突 / Group conflicts with a key group: {group.Key}");
                    }

                    indexGroups[group.Key] = group.Value;
                }
            }

            return new TableDefinition(key.ToArray(), indexGroups).Apply(schema);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error, "SCHEMA_KEY", ex.Message, path, table, sheet.Name, 3));
            return null;
        }
    }

    private static TableSchema SelectTarget(TableSchema schema, ConfigTypeRegistry registry, ConfigTarget target)
    {
        FieldSchema[] fields = schema.Fields.Where(f => (f.Targets & target) != 0).ToArray();
        string signature = schema.Name + "\n" + string.Join("|",
            fields.Select(f => $"{f.Name}:{ConfigTypeRegistry.TypeIdentity(f.Type)}")) + "\nBinary/" + registry.BinaryVersion + "\n" + ConfigTypeRegistry.Signature(fields.Select(f => f.Type)) + "\nTarget/" + ConfigTargets.Marker(target);
        TableSchema selected = schema with
        {
            Fields = fields,
            SourceFields = schema.Fields,
            Target = target,
            SchemaHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(signature)))
        };
        return new TableDefinition(schema.KeyFields.Select(f => f.Name)
            .ToArray(),
            schema.Groups.Where(g => g.Fields.All(f => (f.Targets & target) != 0))
            .ToDictionary(g => g.Name,
            g => g.Fields.Select(f => f.Name)
            .ToArray(),
            StringComparer.Ordinal)).Apply(selected);
    }
}
