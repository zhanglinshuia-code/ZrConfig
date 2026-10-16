#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace ZrConfig.Validation
{
    public interface ITableRules
    {
        Type RowType
        {
            get;
        }

        void ValidateRow(object row, ValidationContext context);
        void ValidateAll(IList rows, ValidationContext context);
    }

    public abstract class TableRules<T> : ITableRules where T : class
    {
        public Type RowType => typeof(T);

        protected virtual void Validate(T row, ValidationContext context)
        {
        }

        protected virtual void ValidateTable(IReadOnlyList<T> rows, ValidationContext context)
        {
        }

        void ITableRules.ValidateRow(object row, ValidationContext context) => Validate((T)row, context);
        void ITableRules.ValidateAll(IList rows, ValidationContext context) => ValidateTable((IReadOnlyList<T>)rows, context);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public abstract class FieldCheckAttribute : Attribute
    {
        public string Field
        {
            get;
        }

        protected FieldCheckAttribute(string field) => Field = field;
    }

    public sealed class RequiredAttribute : FieldCheckAttribute
    {
        public RequiredAttribute(string field) : base(field)
        {
        }
    }

    public sealed class UniqueAttribute : FieldCheckAttribute
    {
        public UniqueAttribute(string field) : base(field)
        {
        }
    }

    public sealed class PositiveAttribute : FieldCheckAttribute
    {
        public PositiveAttribute(string field) : base(field)
        {
        }
    }

    /// <summary>组合检查：必填、唯一且大于零；不隐式建立运行时索引。</summary>
    public sealed class PositiveKeyAttribute : FieldCheckAttribute
    {
        public PositiveKeyAttribute(string field) : base(field)
        {
        }
    }

    public sealed class RangeAttribute : FieldCheckAttribute
    {
        public double Min
        {
            get;
        }
        public double Max
        {
            get;
        }

        public RangeAttribute(string field, double min, double max) : base(field)
        {
            Min = min;
            Max = max;
        }
    }

    public sealed class ReferenceAttribute : FieldCheckAttribute
    {
        public Type TargetTable
        {
            get;
        }
        public string TargetField
        {
            get;
        }
        public bool AllowDefaultValue
        {
            get; set;
        }

        public ReferenceAttribute(string field, Type targetTable, string targetField) : base(field)
        {
            TargetTable = targetTable;
            TargetField = targetField;
        }
    }

    public sealed class CheckAttribute : FieldCheckAttribute
    {
        public Type RuleType
        {
            get;
        }

        public CheckAttribute(string field, Type ruleType) : base(field) => RuleType = ruleType;
    }

    /// <summary>声明自定义 Validate/ValidateTable 的依赖表；标签规则自行携带依赖信息。</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class DependsOnAttribute : Attribute
    {
        public Type TableType
        {
            get;
        }

        public DependsOnAttribute(Type tableType) => TableType = tableType;
    }
}
