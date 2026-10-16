using ZrConfig.Runtime;

namespace Game.Config
{
    public enum Quality
    {
        None,
        Rare,
        Legendary
    }

    public abstract class BattleAction
    {
        public float Delay;
    }

    public sealed class Damage : BattleAction
    {
        public float Ratio = 1;
        public bool CanCrit;
    }

    public sealed class AddBuff : BattleAction
    {
        [ConfigRequired]
        public int BuffId;
        public int Stacks = 1;
    }

    public sealed class BattleSettings
    {
        public int MaxTargets = 1;
        public bool Interruptible = true;
    }
}
