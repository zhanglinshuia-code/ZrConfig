// zrconfig-api-contract:1
// 此文件由工具生成；所有字段只读，自定义类型不包含业务行为。
import * as R from './runtime';
export interface T_Game_Config_ActionGroup {
    readonly $type?: "Game.Config.ActionGroup";
    readonly "Name": string;
    readonly "Actions": ReadonlyArray<T_Game_Config_BattleAction | null>;
}
export interface T_Game_Config_AddBuff {
    readonly $type?: "Game.Config.AddBuff";
    readonly "Delay": number;
    readonly "BuffId": number;
    readonly "Stacks": number;
}
export type T_Game_Config_BattleAction = T_Game_Config_AddBuff | T_Game_Config_Damage;
export interface T_Game_Config_BattlePhase {
    readonly $type?: "Game.Config.BattlePhase";
    readonly "Name": string;
    readonly "Groups": ReadonlyArray<T_Game_Config_ActionGroup | null>;
}
export interface T_Game_Config_BattleSettings {
    readonly $type?: "Game.Config.BattleSettings";
    readonly "MaxTargets": number;
    readonly "Interruptible": boolean;
}
export interface T_Game_Config_ConfigRange {
    readonly $type?: "Game.Config.ConfigRange";
    readonly "Min": number;
    readonly "Max": number;
}
export interface T_Game_Config_Damage {
    readonly $type?: "Game.Config.Damage";
    readonly "Delay": number;
    readonly "Ratio": number;
    readonly "CanCrit": boolean;
}
export interface T_Game_Config_Money {
    readonly "Cents": number;
}
export const T_Game_Config_Quality = Object.freeze({
    T_None: 0,
    T_Rare: 1,
    T_Legendary: 2,
} as const);
export type T_Game_Config_Quality = typeof T_Game_Config_Quality[keyof typeof T_Game_Config_Quality];
export interface T_Game_Config_SampleOptions {
    readonly $type?: "Game.Config.SampleOptions";
    readonly "Range": T_Game_Config_ConfigRange;
    readonly "Rewards": ReadonlyArray<T_ZrConfig_Types_Kv>;
    readonly "Offset": T_ZrConfig_Types_Vector2;
    readonly "Label": string;
    readonly "Quality": T_Game_Config_Quality;
    readonly "Levels": ReadonlyArray<number>;
}
export interface T_ZrConfig_Types_Kv {
    readonly "Key": number;
    readonly "Value": number;
}
export interface T_ZrConfig_Types_Vector2 {
    readonly "X": number;
    readonly "Y": number;
}
export interface T_ZrConfig_Types_Vector3 {
    readonly "X": number;
    readonly "Y": number;
    readonly "Z": number;
}
