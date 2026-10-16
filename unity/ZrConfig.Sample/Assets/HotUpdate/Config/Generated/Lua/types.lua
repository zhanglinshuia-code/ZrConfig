-- zrconfig-api-contract:1
-- 此文件由工具生成，请勿手工修改。
local prefix = (...):match('^(.*%.)') or ''
local R = require(prefix .. 'runtime')
---@class ZrConfigNull
---@class ZrConfigMatrix
---@field dimensions integer[]
---@field values table
---@class ZrConfigDictionary
---@field size integer
---@field entries table
---@field get fun(key:any):any
local types = {}
---@class T_Game_Config_ActionGroup
---@field Name string
---@field Actions (T_Game_Config_BattleAction|ZrConfigNull)[]
---@class T_Game_Config_AddBuff
---@field Delay number
---@field BuffId integer
---@field Stacks integer
---@class T_Game_Config_BattleAction
---@field Delay number
---@class T_Game_Config_BattlePhase
---@field Name string
---@field Groups (T_Game_Config_ActionGroup|ZrConfigNull)[]
---@class T_Game_Config_BattleSettings
---@field MaxTargets integer
---@field Interruptible boolean
---@class T_Game_Config_ConfigRange
---@field Min integer
---@field Max integer
---@class T_Game_Config_Damage
---@field Delay number
---@field Ratio number
---@field CanCrit boolean
---@class T_Game_Config_Money
---@field Cents integer
---@alias T_Game_Config_Quality integer
types.T_Game_Config_Quality = {
    T_None = 0,
    T_Rare = 1,
    T_Legendary = 2,
}
---@class T_Game_Config_SampleOptions
---@field Range T_Game_Config_ConfigRange
---@field Rewards (T_ZrConfig_Types_Kv)[]
---@field Offset T_ZrConfig_Types_Vector2
---@field Label string
---@field Quality T_Game_Config_Quality
---@field Levels (integer)[]
---@class T_ZrConfig_Types_Kv
---@field Key integer
---@field Value integer
---@class T_ZrConfig_Types_Vector2
---@field X number
---@field Y number
---@class T_ZrConfig_Types_Vector3
---@field X number
---@field Y number
---@field Z number
return R.readonly(types)
