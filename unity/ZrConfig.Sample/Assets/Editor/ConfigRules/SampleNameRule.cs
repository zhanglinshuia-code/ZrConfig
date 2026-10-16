using System;
using ZrConfig.Validation;

// 可复用字段规则：由表规则上的 Check 标签绑定，自动定位到当前字段。
public sealed class SampleNameRule : IFieldRule<string>
{
    public void Validate(string value, FieldRuleContext context)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            context.Error("名称不能为空。", "NAME_EMPTY");
        }
        else if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            context.Warning("名称首尾有空格，请确认是否需要保留。", "NAME_WHITESPACE");
        }
        else
        {
            context.Success("名称格式检查通过。", "NAME_FORMAT_OK");
        }
    }
}
