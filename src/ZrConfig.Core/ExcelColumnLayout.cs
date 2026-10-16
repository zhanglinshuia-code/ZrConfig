using System.Globalization;
using ClosedXML.Excel;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

internal sealed record ExcelColumn(string Name, string Path, int Column, string TypeName, string Description, bool Many);
internal sealed class ExcelLayoutException(string field,
    int column,
    int row,
    string message,
    string code = "CELL_PARSE") : FormatException($"{field}: {message}")
{
    public string Field { get; } = field;
    public int Column { get; } = column;
    public int Row { get; } = row;
    public string Code { get; } = code;
}

/// <summary>导表侧的列树；每层多行数组独立分组和排序，运行时只保留强类型数据。</summary>
internal sealed class ExcelColumnLayout
{
    private sealed class Node
    {
        public ExcelColumn? Leaf;
        public bool Many;
        public Dictionary<string, Node> Children { get; } = new(StringComparer.Ordinal);
        public IEnumerable<ExcelColumn> Columns => Leaf != null ? [Leaf] : Children.Values.SelectMany(n => n.Columns);
        public IEnumerable<ExcelColumn> ScalarColumns => Leaf != null ? [Leaf] : Children.Values.Where(n => !n.Many)
            .SelectMany(n => n.ScalarColumns);
        public ExcelColumn First => Columns.First();
        public bool HasNestedLists => Children.Values.Any(n => n.Many || n.HasNestedLists);

        public bool HasValue(IXLRow row) => Columns.Any(c => !string.IsNullOrWhiteSpace(row.Cell(c.Column).GetInputText()));
    }

    private readonly Node _root = new();
    private readonly ConfigType _type;
    public IReadOnlyList<ExcelColumn> Columns
    {
        get;
    }
    public bool HasLists => _root.Many || _root.HasNestedLists;
    public IEnumerable<int> ParentColumns => _root.Many ? [] : _root.ScalarColumns.Select(c => c.Column);

    public ExcelColumnLayout(ConfigType type, IReadOnlyList<ExcelColumn> columns)
    {
        _type = type;
        Columns = columns;
        _root.Many = columns[0].Many;
        foreach (ExcelColumn column in columns)
        {
            string[] parts = column.Path.Split('.');
            if (parts.Length > ConfigLimits.MaxDepth)
            {
                throw SchemaError(column, ConfigText.Get("成员路径超过 64 层。", "Member paths must not exceed 64 levels."));
            }

            Node node = _root;
            foreach (string part in parts)
            {
                bool many = part.EndsWith("[]", StringComparison.Ordinal);
                string name = many ? part[..^2] : part;
                if (node.Leaf != null)
                {
                    throw SchemaError(column,
                        ConfigText.Get("同一成员不能同时按单格和子列填写。",
                        "A member cannot use both a single cell and expanded child columns."));
                }

                if (!node.Children.TryGetValue(name, out Node? next))
                {
                    node.Children.Add(name, next = new()
                    {
                        Many = many
                    });
                }
                else if (next.Many != many)
                {
                    throw SchemaError(column,
                        ConfigText.Get("同一成员不能混用普通分列和多行数组。",
                        "A member cannot mix ordinary columns and multi-row columns."));
                }

                node = next;
            }

            if (node.Children.Count != 0 || node.Leaf != null)
            {
                throw SchemaError(column, ConfigText.Get("成员列重复或与子列重叠。", "The member column is duplicated or overlaps child columns."));
            }

            node.Leaf = column;
        }

        Validate(_root, [type]);
    }

    private static void Validate(Node node, IReadOnlyList<ConfigType> types)
    {
        if (node.Many)
        {
            if (node.Leaf != null || types.Any(t => t.Kind != ConfigTypeKind.Array || t.ClrType.GetArrayRank() != 1))
            {
                throw SchemaError(node.First,
                    ConfigText.Get("多行列需要一层对象数组 T[]，并且至少有一个成员列。",
                    "Multi-row columns require an object array T[] with at least one member column."),
                    2);
            }

            types = types.Select(t => t.Element!).ToArray();
            if (node.HasNestedLists && !node.Children.ContainsKey("$order"))
            {
                throw SchemaError(node.First,
                    ConfigText.Get("包含子列表的多行数组必须声明 $order，用于将续行归入同一个父元素。",
                    "A multi-row array with child lists requires $order to group continuation rows by parent element."));
            }
        }

        if (node.Leaf != null)
        {
            return;
        }

        types = types.Select(t => t.Kind == ConfigTypeKind.Nullable ? t.Element! : t).ToArray();
        if (types.Any(t => t.Kind != ConfigTypeKind.Object))
        {
            throw SchemaError(node.First,
                ConfigText.Get("分列需要自定义 class/struct；多行分列需要对象数组。",
                "Expanded columns require a custom class/struct; multi-row columns require an object array."),
                2);
        }

        if (node.Children.Keys.All(k => k == "$order"))
        {
            throw SchemaError(node.First, ConfigText.Get("多行数组必须至少有一个成员列。", "A multi-row array requires at least one member column."));
        }

        ConfigType[] concrete = types.SelectMany(t => t.Variants.Prepend(t)).Distinct().ToArray();
        if (types.Any(t => t.IsPolymorphic) && !node.Children.ContainsKey("$type"))
        {
            throw SchemaError(node.First, ConfigText.Get("多态结构需要对应的 $type 列。", "A polymorphic object requires a matching $type column."));
        }

        foreach ((string name, Node child) in node.Children)
        {
            if (name == "$order")
            {
                if (!node.Many || child.Leaf == null)
                {
                    throw SchemaError(child.First, ConfigText.Get("$order 只用于多行数组。", "$order is only valid for a multi-row array."));
                }

                continue;
            }

            if (name == "$type")
            {
                if (child.Leaf == null || !types.Any(t => t.IsPolymorphic))
                {
                    throw SchemaError(child.First,
                        ConfigText.Get("$type 只能是多态对象的类型列。",
                        "$type must be a leaf column of a polymorphic object."));
                }

                continue;
            }

            ConfigType[] members = concrete.Where(t => t.MemberMap.ContainsKey(name))
                .Select(t => t.MemberMap[name].Type)
                .Distinct()
                .ToArray();
            if (members.Length == 0)
            {
                throw SchemaError(child.First,
                    ConfigText.Get($"C# 类型及其子类中不存在成员 {name}。",
                    $"Member {name} does not exist in the C# type or its subclasses."));
            }

            Validate(child, members);
        }
    }

    public object? Read(IReadOnlyList<IXLRow> rows,
        string field,
        ConfigValueParser parser,
        Dictionary<string, CellSource> locations,
        HashSet<string> empty) => ReadNode(_root,
        _type,
        rows,
        field,
        parser,
        locations,
        empty,
        0);
    private static object? ReadNode(Node node,
        ConfigType type,
        IReadOnlyList<IXLRow> rows,
        string path,
        ConfigValueParser parser,
        Dictionary<string, CellSource> locations,
        HashSet<string> empty,
        int depth,
        bool element = false)
    {
        IXLRow row = rows[0];
        ExcelColumn first = node.First;
        locations[path] = new CellSource(row.RowNumber(), first.Column);
        if (depth > ConfigLimits.MaxDepth)
        {
            throw CellError(first, row, path, ConfigText.Get("嵌套超过 64 层。", "Nesting exceeds 64 levels."));
        }

        if (!rows.Any(node.HasValue))
        {
            empty.Add(path);
        }

        if (node.Many && !element)
        {
            return ReadList(node, type, rows, path, parser, locations, empty, depth);
        }

        // 整个可选子对象留空时，后置检查仍可定位声明过的子成员。
        BindEmptyLocations(node, row, path, locations, empty);
        if (node.Leaf is { } leaf)
        {
            try
            {
                object? value = parser.Parse(row.Cell(leaf.Column).GetInputText(), type, false, path);
                if (value == null)
                {
                    empty.Add(path);
                }

                return value;
            }
            catch (Exception ex)
            {
                throw CellError(leaf, row, path, ex.Message);
            }
        }

        if (type.Kind == ConfigTypeKind.Nullable)
        {
            if (!rows.Any(node.HasValue))
            {
                return null;
            }

            type = type.Element!;
        }

        ConfigType concrete = type;
        if (type.IsPolymorphic)
        {
            ExcelColumn tag = node.Children["$type"].Leaf!;
            string name = row.Cell(tag.Column).GetInputText().Trim();
            if (name.Length == 0 || !type.PolymorphicNames.TryGetValue(name, out ConfigType? variant) || variant == null)
            {
                throw CellError(tag,
                    row,
                    path + ".$type",
                    ConfigText.Get("请填写合法的具体子类名称；同名子类需填写完整命名空间。",
                    "Enter a valid concrete type name; use the full namespace if short names are ambiguous."));
            }

            concrete = variant;
        }
        else if (node.Children.TryGetValue("$type", out Node? unusedTag) && rows.Any(unusedTag.HasValue))
        {
            throw CellError(unusedTag.First,
                row,
                path + ".$type",
                ConfigText.Get("当前成员类型不是多态类型，请清空类型列。",
                "The selected member is not polymorphic; clear its type column."));
        }

        object instance = Activator.CreateInstance(concrete.ClrType)!;
        foreach ((string name, Node child) in node.Children)
        {
            if (name is "$type" or "$order")
            {
                continue;
            }

            IXLRow? invalidRow = !concrete.MemberMap.ContainsKey(name) ? rows.FirstOrDefault(child.HasValue) : null;
            if (invalidRow != null)
            {
                ExcelColumn cell = child.Columns.First(c => !string.IsNullOrWhiteSpace(invalidRow.Cell(c.Column).GetInputText()));
                throw CellError(cell,
                    invalidRow,
                    path + "." + name,
                    ConfigText.Get($"{concrete.ClrType.Name} 不包含此成员，请清空不适用的列。",
                    $"{concrete.ClrType.Name} does not contain this member; clear the inapplicable column."));
            }
        }

        foreach (ConfigMember member in concrete.Members)
        {
            string memberPath = path + "." + member.Name;
            node.Children.TryGetValue(member.Name, out Node? child);
            ExcelColumn cell = child?.First ?? first;
            locations[memberPath] = new CellSource(row.RowNumber(), cell.Column);
            if (child == null || !rows.Any(child.HasValue))
            {
                empty.Add(memberPath);
                if (member.Required)
                {
                    throw CellError(cell,
                        row,
                        memberPath,
                        ConfigText.Get("必填成员未填写；0 和 false 可以显式填写。",
                        "A required member is blank; explicit 0 and false are valid."));
                }

                if (member.Get(instance) == null && (member.Type.ClrType == typeof(string) || member.Type.Kind is ConfigTypeKind.Array or ConfigTypeKind.Dictionary))
                {
                    member.Set(instance, parser.Parse("", member.Type, false, memberPath));
                }

                continue;
            }

            object? value = ReadNode(child, member.Type, rows, memberPath, parser, locations, empty, depth + 1);
            if (value == null && member.Required)
            {
                throw CellError(cell, row, memberPath, ConfigText.Get("必填成员不能为 null。", "A required member cannot be null."));
            }

            member.Set(instance, value);
        }

        return instance;
    }

    private static Array ReadList(Node node,
        ConfigType type,
        IReadOnlyList<IXLRow> rows,
        string path,
        ConfigValueParser parser,
        Dictionary<string, CellSource> locations,
        HashSet<string> empty,
        int depth)
    {
        ExcelColumn? orderColumn = node.Children.GetValueOrDefault("$order")?.Leaf;
        var groups = new SortedDictionary<int, List<IXLRow>>();
        foreach (IXLRow row in rows)
        {
            if (!node.HasValue(row))
            {
                continue;
            }

            int order = row.RowNumber();
            if (orderColumn != null && (!int.TryParse(row.Cell(orderColumn.Column)
                .GetInputText()
                .Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out order) || order <= 0))
            {
                throw CellError(orderColumn,
                    row,
                    path + ".$order",
                    ConfigText.Get("$order 必须是正整数；嵌套续行也要填写父级 $order。",
                    "$order must be a positive integer; nested continuation rows must repeat the parent order."),
                    "LIST_ORDER");
            }

            if (!groups.TryGetValue(order, out List<IXLRow>? group))
            {
                if (groups.Count >= ConfigLimits.MaxArrayLength)
                {
                    throw CellError(node.First, row, path, ConfigText.Get("数组元素数量超过上限。", "The array exceeds the element limit."));
                }

                groups.Add(order, group = []);
            }
            else
            {
                if (!node.HasNestedLists)
                {
                    throw CellError(orderColumn!,
                        row,
                        path + ".$order",
                        ConfigText.Get("$order 必须在当前父元素的列表中唯一。",
                        "$order must be unique within the list of the current parent element."),
                        "LIST_ORDER");
                }

                foreach (ExcelColumn column in node.ScalarColumns)
                {
                    string text = row.Cell(column.Column).GetInputText();
                    if (!string.IsNullOrWhiteSpace(text) && text != group[0].Cell(column.Column).GetInputText())
                    {
                        throw CellError(column,
                            row,
                            column.Name,
                            ConfigText.Get("同一 $order 的父元素字段与首行不同；续行请留空或原样重复。",
                            "The parent field conflicts with the first row for this order; leave continuation cells blank or repeat the exact value."),
                            "ROW_PARENT_CONFLICT");
                    }
                }
            }

            group.Add(row);
        }

        var result = Array.CreateInstance(type.Element!.ClrType, groups.Count);
        int index = 0;
        foreach (List<IXLRow> group in groups.Values)
        {
            result.SetValue(ReadNode(node, type.Element, group, $"{path}[{index}]", parser, locations, empty, depth + 1, true), index);
            index++;
        }

        return result;
    }

    private static void BindEmptyLocations(Node node,
        IXLRow row,
        string path,
        Dictionary<string, CellSource> locations,
        HashSet<string> empty)
    {
        locations[path] = new CellSource(row.RowNumber(), node.First.Column);
        if (!node.HasValue(row))
        {
            empty.Add(path);
        }

        foreach ((string name, Node child) in node.Children)
        {
            if (!child.Many)
            {
                BindEmptyLocations(child, row, path + "." + name, locations, empty);
            }
        }
    }

    private static ExcelLayoutException SchemaError(ExcelColumn column,
        string message,
        int row = 1) => new(column.Name,
        column.Column,
        row,
        message);
    private static ExcelLayoutException CellError(ExcelColumn column,
        IXLRow row,
        string field,
        string message,
        string code = "CELL_PARSE") => new(field,
        column.Column,
        row.RowNumber(),
        message,
        code);
}
