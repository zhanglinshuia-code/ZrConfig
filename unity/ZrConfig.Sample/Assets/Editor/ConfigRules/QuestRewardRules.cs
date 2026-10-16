using System.Collections.Generic;
using System.Linq;
using ZrConfig.Models;
using ZrConfig.Validation;

[PositiveKey(nameof(QuestRewardsConfig.Id))]
[Required(nameof(QuestRewardsConfig.Name))]
[Check(nameof(QuestRewardsConfig.Name), typeof(SampleNameRule))]
[DependsOn(typeof(ItemCatalogConfig))]
public sealed class QuestRewardRules : TableRules<QuestRewardsConfig>
{
    protected override void Validate(QuestRewardsConfig row, ValidationContext context)
    {
        bool valid = true;
        if (row.Count <= 0)
        {
            context.Error(row, nameof(row.Count), "奖励数量必须大于零。", "QUEST_COUNT");
            valid = false;
        }

        if (row.RequiredLevel <= 0)
        {
            context.Error(row, nameof(row.RequiredLevel), "开放等级必须大于零。", "QUEST_LEVEL");
            valid = false;
        }

        if (!row.Enabled)
        {
            context.Info(row, nameof(row.Enabled), "任务未启用，仅检查基础参数。", "QUEST_DISABLED");
            return;
        }

        if (row.Item == null)
        {
            context.Error(row, nameof(row.Item), "已启用任务必须指定奖励道具。", "QUEST_ITEM_REQUIRED");
            valid = false;
        }
        else if (!row.Item.Enabled)
        {
            context.Error(row, nameof(row.Item), $"已启用任务引用了停用道具 {row.Item.Id}。", "QUEST_ITEM_DISABLED");
            valid = false;
        }
        else if (row.Count > row.Item.MaxStack)
        {
            context.Warning(row, nameof(row.Count), $"奖励数量超过道具 {row.Item.Id} 的堆叠上限 {row.Item.MaxStack}，发放时需要拆分堆叠。", "REWARD_SPLIT_STACK");
        }

        if (row.RequiredLevel > 60)
        {
            context.Warning(row, nameof(row.RequiredLevel), "开放等级超过 60，请确认任务是否可达。", "QUEST_HIGH_LEVEL");
        }

        if (valid)
        {
            context.Success(row, nameof(row.Item), "任务数量、等级和奖励道具状态检查通过。", "QUEST_REWARD_OK");
        }
    }

    protected override void ValidateTable(IReadOnlyList<QuestRewardsConfig> rows, ValidationContext context)
    {
        if (rows.Count > 0)
        {
            context.Info(rows[0], nameof(QuestRewardsConfig.Id), $"任务共 {rows.Count} 条，启用 {rows.Count(r => r.Enabled)} 条。", "QUEST_SUMMARY");
        }
    }
}
