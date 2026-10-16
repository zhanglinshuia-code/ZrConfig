using System.Linq.Expressions;
using System.Reflection;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

public static class ValidationEngine
{
    private sealed record Plan(ITableRules Rules, LoadedTable Table, Type[] Dependencies, IReadOnlyList<Action> Checks);
    public static void Validate(LoadResult result, DiagnosticCollector collector, params Assembly[] ruleAssemblies)
        => Validate(result, collector, null, ruleAssemblies);

    public static void Validate(LoadResult result, DiagnosticCollector collector, WorkflowOperation? operation, params Assembly[] ruleAssemblies)
    {
        operation?.Checkpoint();
        var tables = result.Tables.ToDictionary(t => t.Codec.RowType);
        var complete = result.Tables.Where(t => t.IsComplete).ToDictionary(t => t.Codec.RowType, t => (object)t.Rows);
        ValidationContext context = new(complete, collector, result.Sources, rowType =>
        {
            LoadedTable table = tables[rowType];
            return table.Codec.CreateTable(table.Rows);
        });
        List<Plan> plans = new();
        // 每次导出仅扫描、绑定一次，包括空表；无效标签必须明确报错。
        foreach (Type type in ruleAssemblies.Distinct()
            .SelectMany(a => GetTypes(a,
            collector))
            .Where(t => !t.IsAbstract && !t.ContainsGenericParameters && typeof(ITableRules).IsAssignableFrom(t))
            .OrderBy(t => t.FullName,
            StringComparer.Ordinal))
        {
            try
            {
                var rules = (ITableRules)Activator.CreateInstance(type)!;
                if (!tables.TryGetValue(rules.RowType, out LoadedTable? table))
                {
                    collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                        "RULE_TABLE_MISSING",
                        ConfigText.Get($"规则 {type.Name} 对应的表不存在或无法读取：{rules.RowType.Name}。",
                        $"{type.Name} targets a missing or unreadable table: {rules.RowType.Name}.")));
                    continue;
                }

                List<Action> checks = new();
                foreach (FieldCheckAttribute tag in type.GetCustomAttributes<FieldCheckAttribute>(true))
                {
                    try
                    {
                        checks.Add(Bind(tag, table, tables, context, collector, operation));
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        Report(table, tag.Field, "RULE_BINDING", Unwrap(ex).ToString(), collector);
                    }
                }

                Type[] dependencies = type.GetCustomAttributes<DependsOnAttribute>(true).Select(a => a.TableType).Distinct().ToArray();
                plans.Add(new Plan(rules, table, dependencies, checks));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                collector.Add(new Diagnostic(DiagnosticSeverity.Error, "RULE_LOAD", $"{type.Name}: {Unwrap(ex).ToString()}"));
            }
        }

        // 先执行所有标签检查；顺序执行也允许自定义规则保存简单状态。
        foreach (Plan plan in plans)
        {
            operation?.Report("validating", plan.Table.Schema.Name);
            foreach (Action check in plan.Checks)
            {
                operation?.Checkpoint();
                try
                {
                    check();
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Report(plan.Table, "", "RULE_EXCEPTION", Unwrap(ex).ToString(), collector);
                }
            }
        }

        foreach (Plan plan in plans)
        {
            operation?.Report("validating", plan.Table.Schema.Name);
            if (!plan.Table.IsComplete || plan.Dependencies.Any(t => !complete.ContainsKey(t)))
            {
                bool missingDependency = plan.Dependencies.Any(t => !tables.ContainsKey(t));
                Report(plan.Table,
                    "",
                    "BUSINESS_CHECK_SKIPPED",
                    ConfigText.Get("已跳过业务检查：当前表或声明的依赖表不可用。",
                    "Business check skipped: source or declared dependency is unavailable."),
                    collector,
                    missingDependency ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning);
                continue;
            }

            // 逐行捕获异常，避免单个自定义检查失败影响其他独立行或表。
            foreach (object row in plan.Table.Rows)
            {
                operation?.Checkpoint();
                try
                {
                    plan.Rules.ValidateRow(row, context);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    result.Sources.TryGetValue(row, out RowSource? source);
                    collector.Add(new Diagnostic(DiagnosticSeverity.Error,
                        "VALIDATOR_EXCEPTION",
                        Unwrap(ex)
                        .ToString(),
                        plan.Table.Schema.FilePath,
                        plan.Table.Schema.Name,
                        plan.Table.Schema.SheetName,
                        row: source?.ExcelRow ?? 0));
                }
            }

            try
            {
                plan.Rules.ValidateAll(plan.Table.Rows, context);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Report(plan.Table, "", "VALIDATOR_EXCEPTION", Unwrap(ex).ToString(), collector);
            }
        }
    }

    private static Action Bind(FieldCheckAttribute tag,
        LoadedTable table,
        IReadOnlyDictionary<Type, LoadedTable> tables,
        ValidationContext context,
        DiagnosticCollector collector, WorkflowOperation? operation)
    {
        if (!table.Schema.Fields.Any(f => f.Name == tag.Field))
        {
            throw new InvalidOperationException(ConfigText.Get($"Excel 表结构中不存在字段“{tag.Field}”。",
                $"Field '{tag.Field}' is not part of the Excel schema."));
        }

        PropertyInfo property = GetProperty(table.Codec.RowType, tag.Field);
        bool numeric = property.PropertyType == typeof(int) || property.PropertyType == typeof(long) || property.PropertyType == typeof(float) || property.PropertyType == typeof(double);
        if (tag is PositiveAttribute or PositiveKeyAttribute or RangeAttribute && !numeric)
        {
            throw new InvalidOperationException(ConfigText.Get($"{tag.GetType().Name} 只能用于数值字段。",
                $"{tag.GetType().Name} requires a numeric field."));
        }

        if (tag is RangeAttribute range && (!double.IsFinite(range.Min) || !double.IsFinite(range.Max) || range.Min > range.Max))
        {
            throw new InvalidOperationException(ConfigText.Get("范围上下界必须是有限数值，且下界不能大于上界。", "Range bounds must be finite and ordered."));
        }

        if (tag is CheckAttribute custom)
        {
            Type contract = typeof(IFieldRule<>).MakeGenericType(property.PropertyType);
            if (!contract.IsAssignableFrom(custom.RuleType) || custom.RuleType.IsAbstract)
            {
                throw new InvalidOperationException(ConfigText.Get($"{custom.RuleType.Name} 必须实现 IFieldRule<{property.PropertyType.Name}>。",
                    $"{custom.RuleType.Name} must implement IFieldRule<{property.PropertyType.Name}>."));
            }

            object rule = Activator.CreateInstance(custom.RuleType)!;
            ParameterExpression rowParameter = Expression.Parameter(typeof(object));
            ParameterExpression contextParameter = Expression.Parameter(typeof(FieldRuleContext));
            Action<object, FieldRuleContext> invoke = Expression.Lambda<Action<object, FieldRuleContext>>(Expression.Call(Expression.Constant(rule,
                contract),
                contract.GetMethod("Validate")!,
                Expression.Property(Expression.Convert(rowParameter,
                property.DeclaringType!),
                property),
                contextParameter),
                rowParameter,
                contextParameter)
                .Compile();
            return () =>
            {
                foreach (object row in table.Rows)
                {
                    operation?.Checkpoint();
                    if (context.IsEmpty(row, tag.Field))
                    {
                        continue; // 必须填写的字段需要额外添加 Required。
                    }

                    try
                    {
                        invoke(row, new FieldRuleContext(context, row, tag.Field));
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        context.Error(row, tag.Field, Unwrap(ex).ToString(), "RULE_EXCEPTION");
                    }
                }
            };
        }

        if (tag is RequiredAttribute)
        {
            return () =>
            {
                foreach (object row in table.Rows)
                {
                    operation?.Checkpoint();
                    if (context.IsEmpty(row, tag.Field))
                    {
                        context.Error(row, tag.Field, ConfigText.Get("必填单元格为空。", "Required cell is empty."), "REQUIRED");
                    }
                }
            };
        }

        if (tag is not (UniqueAttribute or PositiveAttribute or PositiveKeyAttribute or RangeAttribute or ReferenceAttribute))
        {
            throw new InvalidOperationException(ConfigText.Get($"不支持检查标签 {tag.GetType().Name}。",
                $"Unsupported check attribute {tag.GetType().Name}."));
        }

        // 数值字段使用强类型取值和集合，避免每行装箱。其他类型保留 object 相等语义，
        // 不将自定义类型的 Equals(object) 悄悄改为 IEquatable<T>。
        if (property.PropertyType == typeof(int))
        {
            return BindValues<int>(tag, property, table, tables, context, collector, operation, static value => value);
        }

        if (property.PropertyType == typeof(long))
        {
            return BindValues<long>(tag, property, table, tables, context, collector, operation, static value => value);
        }

        if (property.PropertyType == typeof(float))
        {
            return BindValues<float>(tag, property, table, tables, context, collector, operation, static value => value);
        }

        if (property.PropertyType == typeof(double))
        {
            return BindValues<double>(tag, property, table, tables, context, collector, operation, static value => value);
        }

        return BindValues<object>(tag, property, table, tables, context, collector, operation);
    }

    private static Action BindValues<TValue>(FieldCheckAttribute tag,
        PropertyInfo property,
        LoadedTable table,
        IReadOnlyDictionary<Type, LoadedTable> tables,
        ValidationContext context,
        DiagnosticCollector collector,
        WorkflowOperation? operation,
        Func<TValue, double>? number = null)
    {
        Func<object, TValue> get = Getter<TValue>(property);
        if (tag is ReferenceAttribute reference)
        {
            PropertyInfo targetField = GetProperty(reference.TargetTable, reference.TargetField);
            if (targetField.PropertyType != property.PropertyType)
            {
                throw new InvalidOperationException(ConfigText.Get("引用字段与目标字段的类型必须一致。", "Reference fields must have identical types."));
            }

            if (!tables.TryGetValue(reference.TargetTable, out LoadedTable? target))
            {
                throw new InvalidOperationException(ConfigText.Get($"引用目标表 {reference.TargetTable.Name} 不可用。",
                    $"Referenced table {reference.TargetTable.Name} is unavailable."));
            }

            if (!target.Schema.Fields.Any(f => f.Name == reference.TargetField))
            {
                throw new InvalidOperationException(ConfigText.Get("引用目标字段不在 Excel 表结构中。",
                    "Reference target field is not part of the Excel schema."));
            }

            if (!target.IsComplete)
            {
                return () => Report(table,
                    tag.Field,
                    "REF_CHECK_SKIPPED",
                    ConfigText.Get("引用目标表存在解析错误，已跳过引用检查。",
                    "Referenced table has parsing errors."),
                    collector,
                    DiagnosticSeverity.Warning);
            }

            Func<object, TValue> getTarget = Getter<TValue>(targetField);
            // 引用目标字段可以重复，不按总行数预留空间。
            var keys = new HashSet<TValue>();
            foreach (object row in target.Rows)
            {
                operation?.Checkpoint();
                if (!context.IsEmpty(row, reference.TargetField))
                {
                    keys.Add(getTarget(row));
                }
            }

            TValue defaultValue = property.PropertyType.IsValueType ? (TValue)Activator.CreateInstance(property.PropertyType)! : (TValue)(object)"";
            return () =>
            {
                foreach (object row in table.Rows)
                {
                    operation?.Checkpoint();
                    TValue value = get(row);
                    bool empty = context.IsEmpty(row, tag.Field);
                    if (reference.AllowDefaultValue && (empty || EqualityComparer<TValue>.Default.Equals(value, defaultValue)))
                    {
                        continue;
                    }

                    if (empty || !keys.Contains(value))
                    {
                        context.Error(row,
                            tag.Field,
                            ConfigText.Get($"找不到引用值“{value}”。",
                            $"Reference '{value}' not found."),
                            "REFERENCE_MISSING");
                    }
                }
            };
        }

        bool uniqueValues = tag is UniqueAttribute or PositiveKeyAttribute;
        bool positive = tag is PositiveAttribute or PositiveKeyAttribute;
        bool required = tag is PositiveKeyAttribute;
        var bounds = tag as RangeAttribute;
        return () =>
        {
            HashSet<TValue>? unique = uniqueValues ? new HashSet<TValue>(typeof(TValue) == typeof(object) ? 0 : table.Rows.Count) : null;
            foreach (object row in table.Rows)
            {
                operation?.Checkpoint();
                if (context.IsEmpty(row, tag.Field))
                {
                    if (required)
                    {
                        context.Error(row, tag.Field, ConfigText.Get("必填单元格为空。", "Required cell is empty."), "REQUIRED");
                    }

                    continue;
                }

                TValue value = get(row);
                if (unique != null && !unique.Add(value))
                {
                    context.Error(row, tag.Field, ConfigText.Get($"存在重复值：{value}。", $"Duplicate value: {value}."), "KEY_DUPLICATE");
                }

                if (positive && number!(value) <= 0)
                {
                    context.Error(row, tag.Field, ConfigText.Get("数值必须大于零。", "Value must be greater than zero."), "POSITIVE");
                }

                if (bounds != null)
                {
                    double numericValue = number!(value);
                    if (numericValue < bounds.Min || numericValue > bounds.Max)
                    {
                        context.Error(row,
                            tag.Field,
                            ConfigText.Get($"数值必须在 [{bounds.Min}, {bounds.Max}] 范围内。",
                            $"Value must be within [{bounds.Min}, {bounds.Max}]."),
                            "RANGE");
                    }
                }
            }
        };
    }

    private static PropertyInfo GetProperty(Type type, string name)
    {
        PropertyInfo? property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (property?.GetMethod == null || property.GetIndexParameters().Length != 0)
        {
            throw new InvalidOperationException(ConfigText.Get($"在 {type.Name} 中找不到可读取的字段“{name}”。",
                $"Readable field '{name}' not found in {type.Name}."));
        }

        return property;
    }

    private static Func<object, TValue> Getter<TValue>(PropertyInfo property)
    {
        ParameterExpression row = Expression.Parameter(typeof(object));
        return Expression.Lambda<Func<object, TValue>>(Expression.Convert(Expression.Property(Expression.Convert(row,
            property.DeclaringType!),
            property),
            typeof(TValue)),
            row)
            .Compile();
    }

    private static void Report(LoadedTable table,
        string field,
        string code,
        string message,
        DiagnosticCollector collector,
        DiagnosticSeverity severity = DiagnosticSeverity.Error) => collector.Add(new Diagnostic(severity,
        code,
        message,
        table.Schema.FilePath,
        table.Schema.Name,
        table.Schema.SheetName,
        field: field,
        column: table.Schema.Fields.FirstOrDefault(f => f.Name == field)?.Column ?? 0));
    private static Type[] GetTypes(Assembly assembly, DiagnosticCollector collector)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            collector.Add(new Diagnostic(DiagnosticSeverity.Error, "RULE_ASSEMBLY_LOAD", ex.Message));
            return ex.Types.OfType<Type>().ToArray();
        }
    }

    private static Exception Unwrap(Exception ex) => ex is TargetInvocationException { InnerException: not null } tie ? tie.InnerException! : ex;
}
