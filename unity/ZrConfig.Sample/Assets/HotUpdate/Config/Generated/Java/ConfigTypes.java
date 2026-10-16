// zrconfig-api-contract:1
package zrconfig;
import java.util.*;
// 此文件由工具生成；自定义数据结构共用一份定义，不包含游戏业务行为。
public final class ConfigTypes {
    private ConfigTypes() {}
    public static class T_Game_Config_ActionGroup {
        private final String v_Name;
        public final String getName() { return v_Name; }
        private final List<ConfigTypes.T_Game_Config_BattleAction> v_Actions;
        public final List<ConfigTypes.T_Game_Config_BattleAction> getActions() { return v_Actions; }
        T_Game_Config_ActionGroup(String p_Name, List<ConfigTypes.T_Game_Config_BattleAction> p_Actions) {
            this.v_Name = p_Name;
            this.v_Actions = p_Actions;
        }
    }
    static T_Game_Config_ActionGroup read_T_Game_Config_ActionGroup(Object value) {
        if (value == null) return null;
        return body_T_Game_Config_ActionGroup(value);
    }
    private static T_Game_Config_ActionGroup body_T_Game_Config_ActionGroup(Object value) {
        return new T_Game_Config_ActionGroup(ConfigRuntime.string(ConfigRuntime.field(value, "Name")), ConfigRuntime.list(ConfigRuntime.field(value, "Actions"), item0 -> ConfigTypes.read_T_Game_Config_BattleAction(item0)));
    }
    public static class T_Game_Config_AddBuff extends T_Game_Config_BattleAction {
        private final int v_BuffId;
        public final int getBuffId() { return v_BuffId; }
        private final int v_Stacks;
        public final int getStacks() { return v_Stacks; }
        T_Game_Config_AddBuff(float p_Delay, int p_BuffId, int p_Stacks) {
            super(p_Delay);
            this.v_BuffId = p_BuffId;
            this.v_Stacks = p_Stacks;
        }
    }
    static T_Game_Config_AddBuff read_T_Game_Config_AddBuff(Object value) {
        if (value == null) return null;
        return body_T_Game_Config_AddBuff(value);
    }
    private static T_Game_Config_AddBuff body_T_Game_Config_AddBuff(Object value) {
        return new T_Game_Config_AddBuff(ConfigRuntime.float32(ConfigRuntime.field(value, "Delay")), (int)ConfigRuntime.integer(ConfigRuntime.field(value, "BuffId"),Integer.MIN_VALUE,Integer.MAX_VALUE), (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Stacks"),Integer.MIN_VALUE,Integer.MAX_VALUE));
    }
    public static abstract class T_Game_Config_BattleAction {
        private final float v_Delay;
        public final float getDelay() { return v_Delay; }
        T_Game_Config_BattleAction(float p_Delay) {
            this.v_Delay = p_Delay;
        }
    }
    static T_Game_Config_BattleAction read_T_Game_Config_BattleAction(Object value) {
        if (value == null) return null;
        switch (ConfigRuntime.string(ConfigRuntime.field(value, "$type"))) {
            case "Game.Config.AddBuff": return body_T_Game_Config_AddBuff(value);
            case "Game.Config.Damage": return body_T_Game_Config_Damage(value);
            default: throw ConfigRuntime.invalid("未知多态类型");
        }
    }
    public static class T_Game_Config_BattlePhase {
        private final String v_Name;
        public final String getName() { return v_Name; }
        private final List<ConfigTypes.T_Game_Config_ActionGroup> v_Groups;
        public final List<ConfigTypes.T_Game_Config_ActionGroup> getGroups() { return v_Groups; }
        T_Game_Config_BattlePhase(String p_Name, List<ConfigTypes.T_Game_Config_ActionGroup> p_Groups) {
            this.v_Name = p_Name;
            this.v_Groups = p_Groups;
        }
    }
    static T_Game_Config_BattlePhase read_T_Game_Config_BattlePhase(Object value) {
        if (value == null) return null;
        return body_T_Game_Config_BattlePhase(value);
    }
    private static T_Game_Config_BattlePhase body_T_Game_Config_BattlePhase(Object value) {
        return new T_Game_Config_BattlePhase(ConfigRuntime.string(ConfigRuntime.field(value, "Name")), ConfigRuntime.list(ConfigRuntime.field(value, "Groups"), item0 -> ConfigTypes.read_T_Game_Config_ActionGroup(item0)));
    }
    public static class T_Game_Config_BattleSettings {
        private final int v_MaxTargets;
        public final int getMaxTargets() { return v_MaxTargets; }
        private final boolean v_Interruptible;
        public final boolean getInterruptible() { return v_Interruptible; }
        T_Game_Config_BattleSettings(int p_MaxTargets, boolean p_Interruptible) {
            this.v_MaxTargets = p_MaxTargets;
            this.v_Interruptible = p_Interruptible;
        }
    }
    static T_Game_Config_BattleSettings read_T_Game_Config_BattleSettings(Object value) {
        if (value == null) return null;
        return body_T_Game_Config_BattleSettings(value);
    }
    private static T_Game_Config_BattleSettings body_T_Game_Config_BattleSettings(Object value) {
        return new T_Game_Config_BattleSettings((int)ConfigRuntime.integer(ConfigRuntime.field(value, "MaxTargets"),Integer.MIN_VALUE,Integer.MAX_VALUE), ConfigRuntime.bool(ConfigRuntime.field(value, "Interruptible")));
    }
    public static class T_Game_Config_ConfigRange {
        private final int v_Min;
        public final int getMin() { return v_Min; }
        private final int v_Max;
        public final int getMax() { return v_Max; }
        T_Game_Config_ConfigRange(int p_Min, int p_Max) {
            this.v_Min = p_Min;
            this.v_Max = p_Max;
        }
    }
    static T_Game_Config_ConfigRange read_T_Game_Config_ConfigRange(Object value) {
        return body_T_Game_Config_ConfigRange(value);
    }
    private static T_Game_Config_ConfigRange body_T_Game_Config_ConfigRange(Object value) {
        return new T_Game_Config_ConfigRange((int)ConfigRuntime.integer(ConfigRuntime.field(value, "Min"),Integer.MIN_VALUE,Integer.MAX_VALUE), (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Max"),Integer.MIN_VALUE,Integer.MAX_VALUE));
    }
    public static class T_Game_Config_Damage extends T_Game_Config_BattleAction {
        private final float v_Ratio;
        public final float getRatio() { return v_Ratio; }
        private final boolean v_CanCrit;
        public final boolean getCanCrit() { return v_CanCrit; }
        T_Game_Config_Damage(float p_Delay, float p_Ratio, boolean p_CanCrit) {
            super(p_Delay);
            this.v_Ratio = p_Ratio;
            this.v_CanCrit = p_CanCrit;
        }
    }
    static T_Game_Config_Damage read_T_Game_Config_Damage(Object value) {
        if (value == null) return null;
        return body_T_Game_Config_Damage(value);
    }
    private static T_Game_Config_Damage body_T_Game_Config_Damage(Object value) {
        return new T_Game_Config_Damage(ConfigRuntime.float32(ConfigRuntime.field(value, "Delay")), ConfigRuntime.float32(ConfigRuntime.field(value, "Ratio")), ConfigRuntime.bool(ConfigRuntime.field(value, "CanCrit")));
    }
    public static class T_Game_Config_Money {
        private final int v_Cents;
        public final int getCents() { return v_Cents; }
        T_Game_Config_Money(int p_Cents) {
            this.v_Cents = p_Cents;
        }
    }
    static T_Game_Config_Money read_T_Game_Config_Money(Object value) {
        return body_T_Game_Config_Money(value);
    }
    private static T_Game_Config_Money body_T_Game_Config_Money(Object value) {
        return new T_Game_Config_Money((int)ConfigRuntime.integer(ConfigRuntime.field(value, "Cents"),Integer.MIN_VALUE,Integer.MAX_VALUE));
    }
    public static final class T_Game_Config_Quality {
        private final int value;
        private T_Game_Config_Quality(int value) { this.value = value; }
        public int value() { return value; }
        public static final T_Game_Config_Quality T_None = new T_Game_Config_Quality(0);
        public static final T_Game_Config_Quality T_Rare = new T_Game_Config_Quality(1);
        public static final T_Game_Config_Quality T_Legendary = new T_Game_Config_Quality(2);
        static T_Game_Config_Quality from(int value) {
            if (value == T_None.value) return T_None;
            if (value == T_Rare.value) return T_Rare;
            if (value == T_Legendary.value) return T_Legendary;
            throw ConfigRuntime.invalid("未知枚举值");
        }
        @Override public int hashCode() { return value; }
        @Override public boolean equals(Object other) { return other instanceof T_Game_Config_Quality v && value == v.value; }
    }
    public static class T_Game_Config_SampleOptions {
        private final ConfigTypes.T_Game_Config_ConfigRange v_Range;
        public final ConfigTypes.T_Game_Config_ConfigRange getRange() { return v_Range; }
        private final List<ConfigTypes.T_ZrConfig_Types_Kv> v_Rewards;
        public final List<ConfigTypes.T_ZrConfig_Types_Kv> getRewards() { return v_Rewards; }
        private final ConfigTypes.T_ZrConfig_Types_Vector2 v_Offset;
        public final ConfigTypes.T_ZrConfig_Types_Vector2 getOffset() { return v_Offset; }
        private final String v_Label;
        public final String getLabel() { return v_Label; }
        private final ConfigTypes.T_Game_Config_Quality v_Quality;
        public final ConfigTypes.T_Game_Config_Quality getQuality() { return v_Quality; }
        private final List<Integer> v_Levels;
        public final List<Integer> getLevels() { return v_Levels; }
        T_Game_Config_SampleOptions(ConfigTypes.T_Game_Config_ConfigRange p_Range, List<ConfigTypes.T_ZrConfig_Types_Kv> p_Rewards, ConfigTypes.T_ZrConfig_Types_Vector2 p_Offset, String p_Label, ConfigTypes.T_Game_Config_Quality p_Quality, List<Integer> p_Levels) {
            this.v_Range = p_Range;
            this.v_Rewards = p_Rewards;
            this.v_Offset = p_Offset;
            this.v_Label = p_Label;
            this.v_Quality = p_Quality;
            this.v_Levels = p_Levels;
        }
    }
    static T_Game_Config_SampleOptions read_T_Game_Config_SampleOptions(Object value) {
        if (value == null) return null;
        return body_T_Game_Config_SampleOptions(value);
    }
    private static T_Game_Config_SampleOptions body_T_Game_Config_SampleOptions(Object value) {
        return new T_Game_Config_SampleOptions(ConfigTypes.read_T_Game_Config_ConfigRange(ConfigRuntime.field(value, "Range")), ConfigRuntime.list(ConfigRuntime.field(value, "Rewards"), item0 -> ConfigTypes.read_T_ZrConfig_Types_Kv(item0)), ConfigTypes.read_T_ZrConfig_Types_Vector2(ConfigRuntime.field(value, "Offset")), ConfigRuntime.string(ConfigRuntime.field(value, "Label")), ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(ConfigRuntime.field(value, "Quality"),Integer.MIN_VALUE,Integer.MAX_VALUE)), ConfigRuntime.list(ConfigRuntime.field(value, "Levels"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE)));
    }
    public static class T_ZrConfig_Types_Kv {
        private final int v_Key;
        public final int getKey() { return v_Key; }
        private final int v_Value;
        public final int getValue() { return v_Value; }
        T_ZrConfig_Types_Kv(int p_Key, int p_Value) {
            this.v_Key = p_Key;
            this.v_Value = p_Value;
        }
    }
    static T_ZrConfig_Types_Kv read_T_ZrConfig_Types_Kv(Object value) {
        return body_T_ZrConfig_Types_Kv(value);
    }
    private static T_ZrConfig_Types_Kv body_T_ZrConfig_Types_Kv(Object value) {
        return new T_ZrConfig_Types_Kv((int)ConfigRuntime.integer(ConfigRuntime.field(value, "Key"),Integer.MIN_VALUE,Integer.MAX_VALUE), (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Value"),Integer.MIN_VALUE,Integer.MAX_VALUE));
    }
    public static class T_ZrConfig_Types_Vector2 {
        private final float v_X;
        public final float getX() { return v_X; }
        private final float v_Y;
        public final float getY() { return v_Y; }
        T_ZrConfig_Types_Vector2(float p_X, float p_Y) {
            this.v_X = p_X;
            this.v_Y = p_Y;
        }
    }
    static T_ZrConfig_Types_Vector2 read_T_ZrConfig_Types_Vector2(Object value) {
        return body_T_ZrConfig_Types_Vector2(value);
    }
    private static T_ZrConfig_Types_Vector2 body_T_ZrConfig_Types_Vector2(Object value) {
        return new T_ZrConfig_Types_Vector2(ConfigRuntime.float32(ConfigRuntime.field(value, "X")), ConfigRuntime.float32(ConfigRuntime.field(value, "Y")));
    }
    public static class T_ZrConfig_Types_Vector3 {
        private final float v_X;
        public final float getX() { return v_X; }
        private final float v_Y;
        public final float getY() { return v_Y; }
        private final float v_Z;
        public final float getZ() { return v_Z; }
        T_ZrConfig_Types_Vector3(float p_X, float p_Y, float p_Z) {
            this.v_X = p_X;
            this.v_Y = p_Y;
            this.v_Z = p_Z;
        }
    }
    static T_ZrConfig_Types_Vector3 read_T_ZrConfig_Types_Vector3(Object value) {
        return body_T_ZrConfig_Types_Vector3(value);
    }
    private static T_ZrConfig_Types_Vector3 body_T_ZrConfig_Types_Vector3(Object value) {
        return new T_ZrConfig_Types_Vector3(ConfigRuntime.float32(ConfigRuntime.field(value, "X")), ConfigRuntime.float32(ConfigRuntime.field(value, "Y")), ConfigRuntime.float32(ConfigRuntime.field(value, "Z")));
    }
}
