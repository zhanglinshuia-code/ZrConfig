#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ZrConfig.Runtime;

namespace ZrConfig.Validation
{
    public enum DiagnosticSeverity
    {
        Info,
        Warning,
        Error,
        Success
    }

    public sealed class Diagnostic
    {
        public DiagnosticSeverity Severity
        {
            get;
        }
        public string Code
        {
            get;
        }
        public string Message
        {
            get;
        }
        public string File
        {
            get;
        }
        public string Table
        {
            get;
        }
        public string Sheet
        {
            get;
        }
        public int Row
        {
            get;
        }
        public string Field
        {
            get;
        }
        public int Column
        {
            get;
        }
        public string SeverityText => Severity switch
        {
            DiagnosticSeverity.Info => ConfigText.Get("信息", "Info"),
            DiagnosticSeverity.Warning => ConfigText.Get("警告", "Warning"),
            DiagnosticSeverity.Success => ConfigText.Get("成功", "Success"),
            _ => ConfigText.Get("错误", "Error")
        };

        public Diagnostic(DiagnosticSeverity severity,
            string code,
            string message,
            string file = "",
            string table = "",
            string sheet = "",
            int row = 0,
            string field = "",
            int column = 0)
        {
            Severity = severity;
            Code = code;
            Message = message;
            File = file;
            Table = table;
            Sheet = sheet;
            Row = row;
            Field = field;
            Column = column;
        }

        public override string ToString()
        {
            string location = File;
            if (!string.IsNullOrEmpty(Sheet))
            {
                string cell = Row > 0 ? (Column > 0 ? ColumnName(Column) : "R") + Row : "";
                location += $" → {Sheet}" + (cell.Length == 0 ? "" : "!" + cell);
            }
            else if (Row > 0)
            {
                location += Column > 0 ? $"({Row},{Column})" : $"({Row})";
            }
            else if (!string.IsNullOrEmpty(Table))
            {
                location += $" → {Table}";
            }

            if (!string.IsNullOrEmpty(Field))
            {
                location += $" → {Field}";
            }

            return $"[{SeverityText}] {Code} {location} {Message}";
        }

        private static string ColumnName(int column)
        {
            string name = "";
            while (column > 0)
            {
                column--;
                name = (char)('A' + column % 26) + name;
                column /= 26;
            }

            return name;
        }
    }

    public sealed class DiagnosticCollector
    {
        private readonly ConcurrentQueue<Diagnostic> _items = new ConcurrentQueue<Diagnostic>();
        public event Action<Diagnostic>? Added;
        public void Add(Diagnostic diagnostic)
        {
            _items.Enqueue(diagnostic);
            Added?.Invoke(diagnostic);
        }
        public Diagnostic[] Snapshot() => _items.ToArray();
    }

    public readonly struct CellSource
    {
        public int Row
        {
            get;
        }
        public int Column
        {
            get;
        }

        public CellSource(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }

    public sealed class RowSource
    {
        public string File
        {
            get;
        }
        public string Table
        {
            get;
        }
        public string Sheet
        {
            get;
        }
        public int ExcelRow
        {
            get;
        }
        public IReadOnlyDictionary<string, int> Columns
        {
            get;
        }
        public IReadOnlyCollection<string> EmptyFields
        {
            get;
        }
        public IReadOnlyDictionary<string, CellSource>? Locations
        {
            get;
        }

        public RowSource(string file,
            string table,
            string sheet,
            int excelRow,
            IReadOnlyDictionary<string, int> columns,
            IReadOnlyCollection<string>? emptyFields = null) : this(file,
            table,
            sheet,
            excelRow,
            columns,
            emptyFields,
            null)
        {
        }

        public RowSource(string file,
            string table,
            string sheet,
            int excelRow,
            IReadOnlyDictionary<string, int> columns,
            IReadOnlyCollection<string>? emptyFields,
            IReadOnlyDictionary<string, CellSource>? locations)
        {
            File = file;
            Table = table;
            Sheet = sheet;
            ExcelRow = excelRow;
            Columns = columns;
            EmptyFields = emptyFields ?? Array.Empty<string>();
            Locations = locations;
        }
    }

    /// <summary>可复用字段规则，通过手写表规则类上的 CheckAttribute 绑定。</summary>
    public interface IFieldRule<T>
    {
        void Validate(T value, FieldRuleContext context);
    }

    public readonly struct FieldRuleContext
    {
        private readonly ValidationContext _context;
        private readonly object _row;
        private readonly string _field;
        public FieldRuleContext(ValidationContext context, object row, string field)
        {
            _context = context;
            _row = row;
            _field = field;
        }

        public void Error(string message, string code = "FIELD_RULE") => _context.Error(_row, _field, message, code);
        public void Warning(string message, string code = "FIELD_RULE") => _context.Warning(_row, _field, message, code);
        public void Info(string message, string code = "FIELD_RULE") => _context.Info(_row, _field, message, code);
        public void Success(string message, string code = "FIELD_RULE") => _context.Success(_row, _field, message, code);
    }

    public sealed class TableUnavailableException : Exception
    {
        public TableUnavailableException(Type type) : base(ConfigText.Get($"{type.Name} 的表数据不可用，输入缺失或无效。",
            $"Table data unavailable for {type.Name}; input was missing or invalid."))
        {
        }
    }

    public sealed class ValidationContext : IConfigTableProvider
    {
        private readonly IReadOnlyDictionary<Type, object> _tables;
        private readonly DiagnosticCollector _collector;
        private readonly ConditionalWeakTable<object, RowSource> _sources;
        private readonly Func<Type, IConfigTable> _tableFactory;
        private readonly Dictionary<Type, Lazy<IConfigTable>> _indexedTables = new Dictionary<Type, Lazy<IConfigTable>>();
        public ValidationContext(IReadOnlyDictionary<Type, object> tables,
            DiagnosticCollector collector,
            ConditionalWeakTable<object, RowSource> sources,
            Func<Type, IConfigTable> tableFactory)
        {
            _tables = tables;
            _collector = collector;
            _sources = sources;
            _tableFactory = tableFactory ?? throw new ArgumentNullException(nameof(tableFactory));
        }

        /// <summary>首次访问时建立生成表及索引；本次校验的所有规则复用同一实例。</summary>
        public ConfigTable<T> GetTable<T>()
            where T : class, IConfigRow
        {
            Type rowType = typeof(T);
            if (!_indexedTables.TryGetValue(rowType, out Lazy<IConfigTable>? table))
            {
                // 只允许查询本次导入的完整表，不能为残缺行列表建立索引。
                if (!_tables.ContainsKey(rowType))
                {
                    throw new TableUnavailableException(rowType);
                }

                table = CreateIndexedTable(rowType);
                _indexedTables.Add(rowType, table);
            }

            return (ConfigTable<T>)table.Value;
        }

        ConfigTable<T> IConfigTableProvider.GetTable<T>(ITableCodec codec)
        {
            if (codec == null)
            {
                throw new ArgumentNullException(nameof(codec));
            }
            if (codec.RowType != typeof(T))
            {
                throw new ArgumentException("Codec row type mismatch.", nameof(codec));
            }
            return GetTable<T>();
        }
        // 闭包仅在首次访问时创建；失败也由 Lazy 缓存，避免每行重复建表。
        private Lazy<IConfigTable> CreateIndexedTable(Type rowType) => new Lazy<IConfigTable>(() => _tableFactory(rowType));
        public bool IsEmpty(object row, string field)
        {
            if (!_sources.TryGetValue(row, out RowSource? source))
            {
                throw new ArgumentException(ConfigText.Get("该行不属于本次导入的数据。", "Row is not part of the current import."), nameof(row));
            }

            foreach (string empty in source.EmptyFields)
            {
                if (empty == field)
                {
                    return true;
                }
            }

            return false;
        }

        public void Error<T>(T row, string field, string message, string code = "CUSTOM")
            where T : class => Report(DiagnosticSeverity.Error, row, field, message, code);
        public void Warning<T>(T row, string field, string message, string code = "CUSTOM")
            where T : class => Report(DiagnosticSeverity.Warning, row, field, message, code);
        public void Info<T>(T row, string field, string message, string code = "CUSTOM")
            where T : class => Report(DiagnosticSeverity.Info, row, field, message, code);
        /// <summary>报告某项检查通过；不代表整个导出已经发布成功。</summary>
        public void Success<T>(T row, string field, string message, string code = "CUSTOM")
            where T : class => Report(DiagnosticSeverity.Success, row, field, message, code);
        private void Report<T>(DiagnosticSeverity severity, T row, string field, string message, string code)
            where T : class
        {
            if (!_sources.TryGetValue(row, out RowSource? source))
            {
                _collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                    "VALIDATOR_LOCATION",
                    ConfigText.Get("该行对象不是由本次 Excel 导入创建的。",
                    "Row object does not originate from the current Excel import."),
                    table: typeof(T).Name,
                    field: field));
                return;
            }

            int excelRow = source.ExcelRow;
            source.Columns.TryGetValue(field, out int column);
            if (source.Locations != null && source.Locations.TryGetValue(field, out CellSource location))
            {
                column = location.Column;
                excelRow = location.Row;
            }

            if (column == 0)
            {
                _collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                    "VALIDATOR_FIELD",
                    ConfigText.Get($"表 {source.Table} 中不存在字段“{field}”。",
                    $"Unknown field '{field}' in {source.Table}."),
                    source.File,
                    source.Table,
                    source.Sheet,
                    source.ExcelRow,
                    field));
                return;
            }

            _collector.Add(new Diagnostic(severity, code, message, source.File, source.Table, source.Sheet, excelRow, field, column));
        }
    }
}
