// zrconfig-api-contract:1
// 此文件由工具生成；数据字段私有，只提供只读访问。
use super::runtime::{self as R, Json, Result};
pub struct T_Game_Config_ActionGroup {
    v_Name: String,
    v_Actions: Vec<Option<Box<T_Game_Config_BattleAction>>>,
}
impl T_Game_Config_ActionGroup {
    pub fn get_Name(&self) -> &str { &self.v_Name }
    pub fn get_Actions(&self) -> &[Option<Box<T_Game_Config_BattleAction>>] { &self.v_Actions }
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Name: R::scalar::<String>(v.field("Name")?)?,
            v_Actions: R::decode_list(v.field("Actions")?, |item0| R::decode_optional(item0, |v| T_Game_Config_BattleAction::__zr_decode(v).map(Box::new)))?,
        })
    }
}
pub struct T_Game_Config_AddBuff {
    v_Delay: f32,
    v_BuffId: i32,
    v_Stacks: i32,
}
impl T_Game_Config_AddBuff {
    pub fn get_Delay(&self) -> f32 { self.v_Delay }
    pub fn get_BuffId(&self) -> i32 { self.v_BuffId }
    pub fn get_Stacks(&self) -> i32 { self.v_Stacks }
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Delay: R::scalar::<f32>(v.field("Delay")?)?,
            v_BuffId: R::scalar::<i32>(v.field("BuffId")?)?,
            v_Stacks: R::scalar::<i32>(v.field("Stacks")?)?,
        })
    }
}
pub enum T_Game_Config_BattleAction {
    T_Game_Config_AddBuff(Box<T_Game_Config_AddBuff>),
    T_Game_Config_Damage(Box<T_Game_Config_Damage>),
}
impl T_Game_Config_BattleAction {
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        match v.field("$type")?.string()? {
            "Game.Config.AddBuff" => Ok(Self::T_Game_Config_AddBuff(Box::new(T_Game_Config_AddBuff::__zr_decode(v)?))),
            "Game.Config.Damage" => Ok(Self::T_Game_Config_Damage(Box::new(T_Game_Config_Damage::__zr_decode(v)?))),
            _ => R::invalid("未知多态类型"),
        }
    }
    pub fn get_Delay(&self) -> f32 { match self {
        Self::T_Game_Config_AddBuff(v) => v.get_Delay(),
        Self::T_Game_Config_Damage(v) => v.get_Delay(),
    } }
}
pub struct T_Game_Config_BattlePhase {
    v_Name: String,
    v_Groups: Vec<Option<Box<T_Game_Config_ActionGroup>>>,
}
impl T_Game_Config_BattlePhase {
    pub fn get_Name(&self) -> &str { &self.v_Name }
    pub fn get_Groups(&self) -> &[Option<Box<T_Game_Config_ActionGroup>>] { &self.v_Groups }
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Name: R::scalar::<String>(v.field("Name")?)?,
            v_Groups: R::decode_list(v.field("Groups")?, |item0| R::decode_optional(item0, |v| T_Game_Config_ActionGroup::__zr_decode(v).map(Box::new)))?,
        })
    }
}
pub struct T_Game_Config_BattleSettings {
    v_MaxTargets: i32,
    v_Interruptible: bool,
}
impl T_Game_Config_BattleSettings {
    pub fn get_MaxTargets(&self) -> i32 { self.v_MaxTargets }
    pub fn get_Interruptible(&self) -> bool { self.v_Interruptible }
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_MaxTargets: R::scalar::<i32>(v.field("MaxTargets")?)?,
            v_Interruptible: R::scalar::<bool>(v.field("Interruptible")?)?,
        })
    }
}
pub struct T_Game_Config_ConfigRange {
    v_Min: i32,
    v_Max: i32,
}
impl T_Game_Config_ConfigRange {
    pub fn get_Min(&self) -> i32 { self.v_Min }
    pub fn get_Max(&self) -> i32 { self.v_Max }
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Min: R::scalar::<i32>(v.field("Min")?)?,
            v_Max: R::scalar::<i32>(v.field("Max")?)?,
        })
    }
}
pub struct T_Game_Config_Damage {
    v_Delay: f32,
    v_Ratio: f32,
    v_CanCrit: bool,
}
impl T_Game_Config_Damage {
    pub fn get_Delay(&self) -> f32 { self.v_Delay }
    pub fn get_Ratio(&self) -> f32 { self.v_Ratio }
    pub fn get_CanCrit(&self) -> bool { self.v_CanCrit }
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Delay: R::scalar::<f32>(v.field("Delay")?)?,
            v_Ratio: R::scalar::<f32>(v.field("Ratio")?)?,
            v_CanCrit: R::scalar::<bool>(v.field("CanCrit")?)?,
        })
    }
}
pub struct T_Game_Config_Money {
    v_Cents: i32,
}
impl T_Game_Config_Money {
    pub fn get_Cents(&self) -> i32 { self.v_Cents }
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Cents: R::scalar::<i32>(v.field("Cents")?)?,
        })
    }
}
#[derive(Clone, Copy, Debug, PartialEq, Eq, Hash)]
pub struct T_Game_Config_Quality(i32);
impl T_Game_Config_Quality {
    #[allow(non_upper_case_globals)]
    pub const T_None: Self = Self(0i32);
    #[allow(non_upper_case_globals)]
    pub const T_Rare: Self = Self(1i32);
    #[allow(non_upper_case_globals)]
    pub const T_Legendary: Self = Self(2i32);
    pub fn value(self) -> i32 { self.0 }
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        let value = R::scalar::<i32>(v)?;
        match value { 0i32 | 1i32 | 2i32 => Ok(Self(value)), _ => R::invalid("未知枚举值") }
    }
}
impl R::Key for T_Game_Config_Quality { fn hash_key(&self, state: &mut std::collections::hash_map::DefaultHasher) { R::hash_part(state, &self.0); } }
pub struct T_Game_Config_SampleOptions {
    v_Range: T_Game_Config_ConfigRange,
    v_Rewards: Vec<T_ZrConfig_Types_Kv>,
    v_Offset: T_ZrConfig_Types_Vector2,
    v_Label: String,
    v_Quality: T_Game_Config_Quality,
    v_Levels: Vec<i32>,
}
impl T_Game_Config_SampleOptions {
    pub fn get_Range(&self) -> &T_Game_Config_ConfigRange { &self.v_Range }
    pub fn get_Rewards(&self) -> &[T_ZrConfig_Types_Kv] { &self.v_Rewards }
    pub fn get_Offset(&self) -> &T_ZrConfig_Types_Vector2 { &self.v_Offset }
    pub fn get_Label(&self) -> &str { &self.v_Label }
    pub fn get_Quality(&self) -> T_Game_Config_Quality { self.v_Quality }
    pub fn get_Levels(&self) -> &[i32] { &self.v_Levels }
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Range: T_Game_Config_ConfigRange::__zr_decode(v.field("Range")?)?,
            v_Rewards: R::decode_list(v.field("Rewards")?, |item0| T_ZrConfig_Types_Kv::__zr_decode(item0))?,
            v_Offset: T_ZrConfig_Types_Vector2::__zr_decode(v.field("Offset")?)?,
            v_Label: R::scalar::<String>(v.field("Label")?)?,
            v_Quality: T_Game_Config_Quality::__zr_decode(v.field("Quality")?)?,
            v_Levels: R::decode_list(v.field("Levels")?, |item0| R::scalar::<i32>(item0))?,
        })
    }
}
pub struct T_ZrConfig_Types_Kv {
    v_Key: i32,
    v_Value: i32,
}
impl T_ZrConfig_Types_Kv {
    pub fn get_Key(&self) -> i32 { self.v_Key }
    pub fn get_Value(&self) -> i32 { self.v_Value }
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Key: R::scalar::<i32>(v.field("Key")?)?,
            v_Value: R::scalar::<i32>(v.field("Value")?)?,
        })
    }
}
pub struct T_ZrConfig_Types_Vector2 {
    v_X: f32,
    v_Y: f32,
}
impl T_ZrConfig_Types_Vector2 {
    pub fn get_X(&self) -> f32 { self.v_X }
    pub fn get_Y(&self) -> f32 { self.v_Y }
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_X: R::scalar::<f32>(v.field("X")?)?,
            v_Y: R::scalar::<f32>(v.field("Y")?)?,
        })
    }
}
pub struct T_ZrConfig_Types_Vector3 {
    v_X: f32,
    v_Y: f32,
    v_Z: f32,
}
impl T_ZrConfig_Types_Vector3 {
    pub fn get_X(&self) -> f32 { self.v_X }
    pub fn get_Y(&self) -> f32 { self.v_Y }
    pub fn get_Z(&self) -> f32 { self.v_Z }
    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_X: R::scalar::<f32>(v.field("X")?)?,
            v_Y: R::scalar::<f32>(v.field("Y")?)?,
            v_Z: R::scalar::<f32>(v.field("Z")?)?,
        })
    }
}
