using System.Collections.Generic;
using System.Linq;
using ZrConfig.Models;
using ZrConfig.Validation;

[PositiveKey(nameof(ItemCatalogConfig.Id))]
[Required(nameof(ItemCatalogConfig.Name))]
[Check(nameof(ItemCatalogConfig.Name), typeof(SampleNameRule))]
public sealed class ItemCatalogRules : TableRules<ItemCatalogConfig>
{
    protected override void Validate(ItemCatalogConfig row, ValidationContext context)
    {
        bool valid = true;
        if (row.MaxStack < 1 || row.MaxStack > 10000)
        {
            context.Error(row, nameof(row.MaxStack), "堆叠上限必须在 1～10000 之间。", "ITEM_STACK_RANGE");
            valid = false;
        }
        else if (row.MaxStack > 999)
        {
            context.Warning(row, nameof(row.MaxStack), "堆叠上限较大，请确认背包与奖励展示能够正确处理。", "ITEM_STACK_LARGE");
        }

        if (row.Rarity < 1 || row.Rarity > 5)
        {
            context.Error(row, nameof(row.Rarity), "品质必须在 1～5 之间。", "ITEM_RARITY_RANGE");
            valid = false;
        }

        if (!row.Enabled)
        {
            context.Info(row, nameof(row.Enabled), "道具已停用，不应被已启用的商品或任务引用。", "ITEM_DISABLED");
        }
        else if (valid)
        {
            context.Success(row, nameof(row.MaxStack), "道具堆叠与品质参数检查通过。", "ITEM_PARAMETERS_OK");
        }
    }

    protected override void ValidateTable(IReadOnlyList<ItemCatalogConfig> rows, ValidationContext context)
    {
        if (rows.Count > 0)
        {
            context.Info(rows[0], nameof(ItemCatalogConfig.Id), $"道具目录共 {rows.Count} 条，启用 {rows.Count(r => r.Enabled)} 条。", "ITEM_SUMMARY");
        }
    }
}
