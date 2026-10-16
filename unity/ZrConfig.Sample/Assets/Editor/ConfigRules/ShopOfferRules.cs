using System.Collections.Generic;
using System.Linq;
using ZrConfig.Models;
using ZrConfig.Validation;

[PositiveKey(nameof(ShopOffersConfig.Id))]
[DependsOn(typeof(ItemCatalogConfig))]
public sealed class ShopOfferRules : TableRules<ShopOffersConfig>
{
    protected override void Validate(ShopOffersConfig row, ValidationContext context)
    {
        bool valid = true;
        if (row.Price < 0)
        {
            context.Error(row, nameof(row.Price), "商品价格不能为负数。", "SHOP_PRICE");
            valid = false;
        }

        if (row.DailyLimit < 0)
        {
            context.Error(row, nameof(row.DailyLimit), "每日限购不能为负数；0 表示不限购。", "SHOP_DAILY_LIMIT");
            valid = false;
        }

        if (!row.Enabled)
        {
            context.Info(row, nameof(row.Enabled), "商品未上架，仅检查基础参数。", "SHOP_DISABLED");
            return;
        }

        if (row.Item == null)
        {
            context.Error(row, nameof(row.Item), "已上架商品必须指定道具。", "SHOP_ITEM_REQUIRED");
            valid = false;
        }
        else if (!row.Item.Enabled)
        {
            context.Error(row, nameof(row.Item), $"已上架商品引用了停用道具 {row.Item.Id}。", "SHOP_ITEM_DISABLED");
            valid = false;
        }

        if (row.Price == 0)
        {
            context.Info(row, nameof(row.Price), "这是免费商品，请确认领取条件。", "SHOP_FREE");
        }

        if (row.DailyLimit == 0)
        {
            context.Info(row, nameof(row.DailyLimit), "该商品不限购。", "SHOP_UNLIMITED");
        }
        else if (row.DailyLimit > 100)
        {
            context.Warning(row, nameof(row.DailyLimit), "每日限购超过 100，请确认投放数量。", "SHOP_HIGH_LIMIT");
        }

        if (valid)
        {
            context.Success(row, nameof(row.Item), "商品价格、限购和道具状态检查通过。", "SHOP_PARAMETERS_OK");
        }
    }

    protected override void ValidateTable(IReadOnlyList<ShopOffersConfig> rows, ValidationContext context)
    {
        var offers = new Dictionary<(int Item, int Price), int>();
        foreach (ShopOffersConfig row in rows)
        {
            if (!row.Enabled || row.Item == null)
            {
                continue;
            }

            (int Id, int Price) key = (row.Item.Id, row.Price);
            if (offers.TryGetValue(key, out int firstId))
            {
                context.Error(row, nameof(row.Item), $"与商品 {firstId} 重复：同一道具和价格只能上架一次。", "SHOP_DUPLICATE_OFFER");
            }
            else
            {
                offers.Add(key, row.Id);
            }
        }

        if (rows.Count > 0)
        {
            context.Info(rows[0], nameof(ShopOffersConfig.Id), $"商品共 {rows.Count} 条，上架 {rows.Count(r => r.Enabled)} 条。", "SHOP_SUMMARY");
        }
    }
}
