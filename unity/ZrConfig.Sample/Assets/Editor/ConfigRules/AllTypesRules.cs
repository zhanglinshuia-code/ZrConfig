using Game.Config;
using ZrConfig.Models;
using ZrConfig.Validation;

[PositiveKey(nameof(AllTypesConfig.Id))]
public sealed class AllTypesRules : TableRules<AllTypesConfig>
{
    protected override void Validate(AllTypesConfig row, ValidationContext context)
    {
        if (row.Settings.MaxTargets <= 0)
        {
            context.Error(row, "Settings.MaxTargets", "最多目标数必须大于零。", "TARGET_COUNT");
        }

        if (row.Range.Min > row.Range.Max)
        {
            context.Error(row, "Range.Max", "最大值不能小于最小值。", "RANGE_ORDER");
        }

        for (int i = 0; i < row.Ranges.Length; i++)
        {
            if (row.Ranges[i].Min > row.Ranges[i].Max)
            {
                context.Error(row, $"Ranges[{i}].Max", "最大值不能小于最小值。", "RANGE_ORDER");
            }
        }

        CheckAction(row.Action, "Action");
        for (int i = 0; i < row.Actions.Length; i++)
        {
            CheckAction(row.Actions[i], $"Actions[{i}]");
        }

        CheckGroups(row.Groups, "Groups");
        for (int i = 0; i < row.Phases.Length; i++)
        {
            CheckGroups(row.Phases[i].Groups, $"Phases[{i}].Groups");
        }

        void CheckGroups(ActionGroup[] groups, string path)
        {
            for (int group = 0; group < groups.Length; group++)
            {
                for (int action = 0; action < groups[group].Actions.Length; action++)
                {
                    CheckAction(groups[group].Actions[action], $"{path}[{group}].Actions[{action}]");
                }
            }
        }

        void CheckAction(BattleAction action, string path)
        {
            if (action.Delay < 0)
            {
                context.Error(row, path + ".Delay", "动作延时不能为负数。", "ACTION_DELAY");
            }

            if (action is Damage damage && damage.Ratio < 0)
            {
                context.Error(row, path + ".Ratio", "伤害倍率不能为负数。", "DAMAGE_RATIO");
            }

            if (action is AddBuff buff && buff.Stacks <= 0)
            {
                context.Error(row, path + ".Stacks", "Buff 层数必须大于零。", "BUFF_STACKS");
            }
        }
    }
}
