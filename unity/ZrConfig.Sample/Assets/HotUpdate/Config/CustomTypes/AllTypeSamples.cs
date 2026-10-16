using System;
using ZrConfig.Runtime;
using ZrConfig.Types;

namespace Game.Config
{
    // 自定义 struct：成员由 C# 定义，Excel 可按成员分列，也兼容单格 JSON。
    public struct ConfigRange
    {
        public int Min;
        public int Max;
    }

    // 同时演示 public 属性、嵌套结构、枚举、向量和数组成员。
    public sealed class SampleOptions
    {
        public string Label { get; set; } = "默认配置";
        public Quality Quality { get; set; } = Quality.Rare;
        public int[] Levels { get; set; } = Array.Empty<int>();

        public ConfigRange Range;
        public Kv[] Rewards = Array.Empty<Kv>();
        public Vector2 Offset;
    }

    public sealed class ActionGroup
    {
        public string Name = "默认动作组";
        public BattleAction[] Actions = Array.Empty<BattleAction>();
    }

    public sealed class BattlePhase
    {
        public string Name = "默认阶段";
        public ActionGroup[] Groups = Array.Empty<ActionGroup>();
    }

    // 特殊标量示例：Excel 填金额文本，二进制保存整数分。
    [ConfigConverter(typeof(MoneyConverter), "1")]
    [ConfigWireLayout(nameof(Cents))]
    public readonly struct Money
    {
        public readonly int Cents;
        public Money(int cents)
        {
            Cents = cents;
        }
    }
}
