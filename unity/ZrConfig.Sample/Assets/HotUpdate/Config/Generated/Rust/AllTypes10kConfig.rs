// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
use std::sync::{Arc, Weak, OnceLock};
use super::runtime::{self as R, Result, Json};
use super::types as T;
use super::snapshot::ConfigSnapshot;
use super::table_Hero::HeroConfig;
use super::table_Skill::SkillConfig;
pub struct AllTypes10kConfig {
    pub(super) v_Id: i32,
    pub(super) v_ByteValue: u8,
    pub(super) v_ShortValue: i16,
    pub(super) v_IntValue: i32,
    pub(super) v_LongValue: i64,
    pub(super) v_FloatValue: f32,
    pub(super) v_DoubleValue: f64,
    pub(super) v_BoolValue: bool,
    pub(super) v_StringValue: String,
    pub(super) v_SByteValue: i8,
    pub(super) v_UShortValue: u16,
    pub(super) v_UIntValue: u32,
    pub(super) v_ULongValue: u64,
    pub(super) v_DecimalValue: R::Decimal,
    pub(super) v_QualityValue: T::T_Game_Config_Quality,
    pub(super) v_Vector2Value: T::T_ZrConfig_Types_Vector2,
    pub(super) v_Vector3Value: T::T_ZrConfig_Types_Vector3,
    pub(super) v_KvValue: T::T_ZrConfig_Types_Kv,
    pub(super) v_MoneyValue: T::T_Game_Config_Money,
    pub(super) v_ByteArray: Vec<u8>,
    pub(super) v_ShortArray: Vec<i16>,
    pub(super) v_IntArray: Vec<i32>,
    pub(super) v_LongArray: Vec<i64>,
    pub(super) v_FloatArray: Vec<f32>,
    pub(super) v_DoubleArray: Vec<f64>,
    pub(super) v_BoolArray: Vec<bool>,
    pub(super) v_StringArray: Vec<String>,
    pub(super) v_SByteArray: Vec<i8>,
    pub(super) v_UShortArray: Vec<u16>,
    pub(super) v_UIntArray: Vec<u32>,
    pub(super) v_ULongArray: Vec<u64>,
    pub(super) v_DecimalArray: Vec<R::Decimal>,
    pub(super) v_QualityArray: Vec<T::T_Game_Config_Quality>,
    pub(super) v_Vector2Array: Vec<T::T_ZrConfig_Types_Vector2>,
    pub(super) v_Vector3Array: Vec<T::T_ZrConfig_Types_Vector3>,
    pub(super) v_KvArray: Vec<T::T_ZrConfig_Types_Kv>,
    pub(super) v_MoneyArray: Vec<T::T_Game_Config_Money>,
    pub(super) v_ByteArray2D: Vec<Vec<u8>>,
    pub(super) v_ShortArray2D: Vec<Vec<i16>>,
    pub(super) v_IntArray2D: Vec<Vec<i32>>,
    pub(super) v_LongArray2D: Vec<Vec<i64>>,
    pub(super) v_FloatArray2D: Vec<Vec<f32>>,
    pub(super) v_DoubleArray2D: Vec<Vec<f64>>,
    pub(super) v_BoolArray2D: Vec<Vec<bool>>,
    pub(super) v_StringArray2D: Vec<Vec<String>>,
    pub(super) v_SByteArray2D: Vec<Vec<i8>>,
    pub(super) v_UShortArray2D: Vec<Vec<u16>>,
    pub(super) v_UIntArray2D: Vec<Vec<u32>>,
    pub(super) v_ULongArray2D: Vec<Vec<u64>>,
    pub(super) v_DecimalArray2D: Vec<Vec<R::Decimal>>,
    pub(super) v_QualityArray2D: Vec<Vec<T::T_Game_Config_Quality>>,
    pub(super) v_Vector2Array2D: Vec<Vec<T::T_ZrConfig_Types_Vector2>>,
    pub(super) v_Vector3Array2D: Vec<Vec<T::T_ZrConfig_Types_Vector3>>,
    pub(super) v_KvArray2D: Vec<Vec<T::T_ZrConfig_Types_Kv>>,
    pub(super) v_MoneyArray2D: Vec<Vec<T::T_Game_Config_Money>>,
    pub(super) v_ByteArray3D: Vec<Vec<Vec<u8>>>,
    pub(super) v_ShortArray3D: Vec<Vec<Vec<i16>>>,
    pub(super) v_IntArray3D: Vec<Vec<Vec<i32>>>,
    pub(super) v_LongArray3D: Vec<Vec<Vec<i64>>>,
    pub(super) v_FloatArray3D: Vec<Vec<Vec<f32>>>,
    pub(super) v_DoubleArray3D: Vec<Vec<Vec<f64>>>,
    pub(super) v_BoolArray3D: Vec<Vec<Vec<bool>>>,
    pub(super) v_StringArray3D: Vec<Vec<Vec<String>>>,
    pub(super) v_SByteArray3D: Vec<Vec<Vec<i8>>>,
    pub(super) v_UShortArray3D: Vec<Vec<Vec<u16>>>,
    pub(super) v_UIntArray3D: Vec<Vec<Vec<u32>>>,
    pub(super) v_ULongArray3D: Vec<Vec<Vec<u64>>>,
    pub(super) v_DecimalArray3D: Vec<Vec<Vec<R::Decimal>>>,
    pub(super) v_QualityArray3D: Vec<Vec<Vec<T::T_Game_Config_Quality>>>,
    pub(super) v_Vector2Array3D: Vec<Vec<Vec<T::T_ZrConfig_Types_Vector2>>>,
    pub(super) v_Vector3Array3D: Vec<Vec<Vec<T::T_ZrConfig_Types_Vector3>>>,
    pub(super) v_KvArray3D: Vec<Vec<Vec<T::T_ZrConfig_Types_Kv>>>,
    pub(super) v_MoneyArray3D: Vec<Vec<Vec<T::T_Game_Config_Money>>>,
    pub(super) v_Settings: Option<Box<T::T_Game_Config_BattleSettings>>,
    pub(super) v_Range: T::T_Game_Config_ConfigRange,
    pub(super) v_Options: Option<Box<T::T_Game_Config_SampleOptions>>,
    pub(super) v_Action: Option<Box<T::T_Game_Config_BattleAction>>,
    pub(super) v_Actions: Vec<Option<Box<T::T_Game_Config_BattleAction>>>,
    pub(super) v_Ranges: Vec<T::T_Game_Config_ConfigRange>,
    pub(super) v_OptionsList: Vec<Option<Box<T::T_Game_Config_SampleOptions>>>,
    pub(super) v_Groups: Vec<Option<Box<T::T_Game_Config_ActionGroup>>>,
    pub(super) v_Phases: Vec<Option<Box<T::T_Game_Config_BattlePhase>>>,
    pub(super) v_OptionalRange: Option<T::T_Game_Config_ConfigRange>,
    pub(super) v_OptionalInt: Option<i32>,
    pub(super) v_OptionalUInt: Option<u32>,
    pub(super) v_OptionalULong: Option<u64>,
    pub(super) v_OptionalDecimal: Option<R::Decimal>,
    pub(super) v_OptionalBool: Option<bool>,
    pub(super) v_OptionalQuality: Option<T::T_Game_Config_Quality>,
    pub(super) v_OptionalMoney: Option<T::T_Game_Config_Money>,
    pub(super) v_OptionalValues: Vec<Option<i32>>,
    pub(super) v_StatMap: R::Dictionary<String, i32>,
    pub(super) v_PriceMap: R::Dictionary<i32, Option<R::Decimal>>,
    pub(super) v_QualityMap: R::Dictionary<T::T_Game_Config_Quality, u32>,
    pub(super) v_ListMap: R::Dictionary<String, Vec<i32>>,
    pub(super) v_IntMatrix: R::Matrix<i32>,
    pub(super) v_DecimalCube: R::Matrix<R::Decimal>,
    pub(super) v_OptionalMatrix: R::Matrix<Option<u32>>,
    pub(super) v_RawJsonValue: String,
    pub(super) v_RawJsonArray: Vec<String>,
    pub(super) v_RawJsonArray2D: Vec<Vec<String>>,
    pub(super) v_RawJsonArray3D: Vec<Vec<Vec<String>>>,
    pub(super) v_JsonIntArray: Vec<i32>,
    pub(super) v_Hero: i32,
    ref_Hero: OnceLock<Weak<HeroConfig>>,
    pub(super) v_Skill: i32,
    ref_Skill: OnceLock<Weak<SkillConfig>>,
    pub(super) v_Previous: i32,
    ref_Previous: OnceLock<Weak<AllTypes10kConfig>>,
}
impl AllTypes10kConfig {
    // 唯一主键：1～10000
    pub fn get_Id(&self) -> i32 { self.v_Id }
    // Byte；按 Id 轮换常规、边界、默认值
    pub fn get_ByteValue(&self) -> u8 { self.v_ByteValue }
    // Short；按 Id 轮换常规、边界、默认值
    pub fn get_ShortValue(&self) -> i16 { self.v_ShortValue }
    // Int；按 Id 轮换常规、边界、默认值
    pub fn get_IntValue(&self) -> i32 { self.v_IntValue }
    // Long；按 Id 轮换常规、边界、默认值
    pub fn get_LongValue(&self) -> i64 { self.v_LongValue }
    // Float；按 Id 轮换常规、边界、默认值
    pub fn get_FloatValue(&self) -> f32 { self.v_FloatValue }
    // Double；按 Id 轮换常规、边界、默认值
    pub fn get_DoubleValue(&self) -> f64 { self.v_DoubleValue }
    // Bool；按 Id 轮换常规、边界、默认值
    pub fn get_BoolValue(&self) -> bool { self.v_BoolValue }
    // String；按 Id 轮换常规、边界、默认值
    pub fn get_StringValue(&self) -> &str { &self.v_StringValue }
    // SByte；按 Id 轮换常规、边界、默认值
    pub fn get_SByteValue(&self) -> i8 { self.v_SByteValue }
    // UShort；按 Id 轮换常规、边界、默认值
    pub fn get_UShortValue(&self) -> u16 { self.v_UShortValue }
    // UInt；按 Id 轮换常规、边界、默认值
    pub fn get_UIntValue(&self) -> u32 { self.v_UIntValue }
    // ULong；按 Id 轮换常规、边界、默认值
    pub fn get_ULongValue(&self) -> u64 { self.v_ULongValue }
    // Decimal；按 Id 轮换常规、边界、默认值
    pub fn get_DecimalValue(&self) -> R::Decimal { self.v_DecimalValue }
    // Quality；按 Id 轮换常规、边界、默认值
    pub fn get_QualityValue(&self) -> T::T_Game_Config_Quality { self.v_QualityValue }
    // Vector2；按 Id 轮换常规、边界、默认值
    pub fn get_Vector2Value(&self) -> &T::T_ZrConfig_Types_Vector2 { &self.v_Vector2Value }
    // Vector3；按 Id 轮换常规、边界、默认值
    pub fn get_Vector3Value(&self) -> &T::T_ZrConfig_Types_Vector3 { &self.v_Vector3Value }
    // Kv；按 Id 轮换常规、边界、默认值
    pub fn get_KvValue(&self) -> &T::T_ZrConfig_Types_Kv { &self.v_KvValue }
    // Money；按 Id 轮换常规、边界、默认值
    pub fn get_MoneyValue(&self) -> &T::T_Game_Config_Money { &self.v_MoneyValue }
    // Byte；按 Id 轮换常规、边界、默认值
    pub fn get_ByteArray(&self) -> &[u8] { &self.v_ByteArray }
    // Short；按 Id 轮换常规、边界、默认值
    pub fn get_ShortArray(&self) -> &[i16] { &self.v_ShortArray }
    // Int；按 Id 轮换常规、边界、默认值
    pub fn get_IntArray(&self) -> &[i32] { &self.v_IntArray }
    // Long；按 Id 轮换常规、边界、默认值
    pub fn get_LongArray(&self) -> &[i64] { &self.v_LongArray }
    // Float；按 Id 轮换常规、边界、默认值
    pub fn get_FloatArray(&self) -> &[f32] { &self.v_FloatArray }
    // Double；按 Id 轮换常规、边界、默认值
    pub fn get_DoubleArray(&self) -> &[f64] { &self.v_DoubleArray }
    // Bool；按 Id 轮换常规、边界、默认值
    pub fn get_BoolArray(&self) -> &[bool] { &self.v_BoolArray }
    // String；按 Id 轮换常规、边界、默认值
    pub fn get_StringArray(&self) -> &[String] { &self.v_StringArray }
    // SByte；按 Id 轮换常规、边界、默认值
    pub fn get_SByteArray(&self) -> &[i8] { &self.v_SByteArray }
    // UShort；按 Id 轮换常规、边界、默认值
    pub fn get_UShortArray(&self) -> &[u16] { &self.v_UShortArray }
    // UInt；按 Id 轮换常规、边界、默认值
    pub fn get_UIntArray(&self) -> &[u32] { &self.v_UIntArray }
    // ULong；按 Id 轮换常规、边界、默认值
    pub fn get_ULongArray(&self) -> &[u64] { &self.v_ULongArray }
    // Decimal；按 Id 轮换常规、边界、默认值
    pub fn get_DecimalArray(&self) -> &[R::Decimal] { &self.v_DecimalArray }
    // Quality；按 Id 轮换常规、边界、默认值
    pub fn get_QualityArray(&self) -> &[T::T_Game_Config_Quality] { &self.v_QualityArray }
    // Vector2；按 Id 轮换常规、边界、默认值
    pub fn get_Vector2Array(&self) -> &[T::T_ZrConfig_Types_Vector2] { &self.v_Vector2Array }
    // Vector3；按 Id 轮换常规、边界、默认值
    pub fn get_Vector3Array(&self) -> &[T::T_ZrConfig_Types_Vector3] { &self.v_Vector3Array }
    // Kv；按 Id 轮换常规、边界、默认值
    pub fn get_KvArray(&self) -> &[T::T_ZrConfig_Types_Kv] { &self.v_KvArray }
    // Money；按 Id 轮换常规、边界、默认值
    pub fn get_MoneyArray(&self) -> &[T::T_Game_Config_Money] { &self.v_MoneyArray }
    // Byte；按 Id 轮换常规、边界、默认值
    pub fn get_ByteArray2D(&self) -> &[Vec<u8>] { &self.v_ByteArray2D }
    // Short；按 Id 轮换常规、边界、默认值
    pub fn get_ShortArray2D(&self) -> &[Vec<i16>] { &self.v_ShortArray2D }
    // Int；按 Id 轮换常规、边界、默认值
    pub fn get_IntArray2D(&self) -> &[Vec<i32>] { &self.v_IntArray2D }
    // Long；按 Id 轮换常规、边界、默认值
    pub fn get_LongArray2D(&self) -> &[Vec<i64>] { &self.v_LongArray2D }
    // Float；按 Id 轮换常规、边界、默认值
    pub fn get_FloatArray2D(&self) -> &[Vec<f32>] { &self.v_FloatArray2D }
    // Double；按 Id 轮换常规、边界、默认值
    pub fn get_DoubleArray2D(&self) -> &[Vec<f64>] { &self.v_DoubleArray2D }
    // Bool；按 Id 轮换常规、边界、默认值
    pub fn get_BoolArray2D(&self) -> &[Vec<bool>] { &self.v_BoolArray2D }
    // String；按 Id 轮换常规、边界、默认值
    pub fn get_StringArray2D(&self) -> &[Vec<String>] { &self.v_StringArray2D }
    // SByte；按 Id 轮换常规、边界、默认值
    pub fn get_SByteArray2D(&self) -> &[Vec<i8>] { &self.v_SByteArray2D }
    // UShort；按 Id 轮换常规、边界、默认值
    pub fn get_UShortArray2D(&self) -> &[Vec<u16>] { &self.v_UShortArray2D }
    // UInt；按 Id 轮换常规、边界、默认值
    pub fn get_UIntArray2D(&self) -> &[Vec<u32>] { &self.v_UIntArray2D }
    // ULong；按 Id 轮换常规、边界、默认值
    pub fn get_ULongArray2D(&self) -> &[Vec<u64>] { &self.v_ULongArray2D }
    // Decimal；按 Id 轮换常规、边界、默认值
    pub fn get_DecimalArray2D(&self) -> &[Vec<R::Decimal>] { &self.v_DecimalArray2D }
    // Quality；按 Id 轮换常规、边界、默认值
    pub fn get_QualityArray2D(&self) -> &[Vec<T::T_Game_Config_Quality>] { &self.v_QualityArray2D }
    // Vector2；按 Id 轮换常规、边界、默认值
    pub fn get_Vector2Array2D(&self) -> &[Vec<T::T_ZrConfig_Types_Vector2>] { &self.v_Vector2Array2D }
    // Vector3；按 Id 轮换常规、边界、默认值
    pub fn get_Vector3Array2D(&self) -> &[Vec<T::T_ZrConfig_Types_Vector3>] { &self.v_Vector3Array2D }
    // Kv；按 Id 轮换常规、边界、默认值
    pub fn get_KvArray2D(&self) -> &[Vec<T::T_ZrConfig_Types_Kv>] { &self.v_KvArray2D }
    // Money；按 Id 轮换常规、边界、默认值
    pub fn get_MoneyArray2D(&self) -> &[Vec<T::T_Game_Config_Money>] { &self.v_MoneyArray2D }
    // Byte；按 Id 轮换常规、边界、默认值
    pub fn get_ByteArray3D(&self) -> &[Vec<Vec<u8>>] { &self.v_ByteArray3D }
    // Short；按 Id 轮换常规、边界、默认值
    pub fn get_ShortArray3D(&self) -> &[Vec<Vec<i16>>] { &self.v_ShortArray3D }
    // Int；按 Id 轮换常规、边界、默认值
    pub fn get_IntArray3D(&self) -> &[Vec<Vec<i32>>] { &self.v_IntArray3D }
    // Long；按 Id 轮换常规、边界、默认值
    pub fn get_LongArray3D(&self) -> &[Vec<Vec<i64>>] { &self.v_LongArray3D }
    // Float；按 Id 轮换常规、边界、默认值
    pub fn get_FloatArray3D(&self) -> &[Vec<Vec<f32>>] { &self.v_FloatArray3D }
    // Double；按 Id 轮换常规、边界、默认值
    pub fn get_DoubleArray3D(&self) -> &[Vec<Vec<f64>>] { &self.v_DoubleArray3D }
    // Bool；按 Id 轮换常规、边界、默认值
    pub fn get_BoolArray3D(&self) -> &[Vec<Vec<bool>>] { &self.v_BoolArray3D }
    // String；按 Id 轮换常规、边界、默认值
    pub fn get_StringArray3D(&self) -> &[Vec<Vec<String>>] { &self.v_StringArray3D }
    // SByte；按 Id 轮换常规、边界、默认值
    pub fn get_SByteArray3D(&self) -> &[Vec<Vec<i8>>] { &self.v_SByteArray3D }
    // UShort；按 Id 轮换常规、边界、默认值
    pub fn get_UShortArray3D(&self) -> &[Vec<Vec<u16>>] { &self.v_UShortArray3D }
    // UInt；按 Id 轮换常规、边界、默认值
    pub fn get_UIntArray3D(&self) -> &[Vec<Vec<u32>>] { &self.v_UIntArray3D }
    // ULong；按 Id 轮换常规、边界、默认值
    pub fn get_ULongArray3D(&self) -> &[Vec<Vec<u64>>] { &self.v_ULongArray3D }
    // Decimal；按 Id 轮换常规、边界、默认值
    pub fn get_DecimalArray3D(&self) -> &[Vec<Vec<R::Decimal>>] { &self.v_DecimalArray3D }
    // Quality；按 Id 轮换常规、边界、默认值
    pub fn get_QualityArray3D(&self) -> &[Vec<Vec<T::T_Game_Config_Quality>>] { &self.v_QualityArray3D }
    // Vector2；按 Id 轮换常规、边界、默认值
    pub fn get_Vector2Array3D(&self) -> &[Vec<Vec<T::T_ZrConfig_Types_Vector2>>] { &self.v_Vector2Array3D }
    // Vector3；按 Id 轮换常规、边界、默认值
    pub fn get_Vector3Array3D(&self) -> &[Vec<Vec<T::T_ZrConfig_Types_Vector3>>] { &self.v_Vector3Array3D }
    // Kv；按 Id 轮换常规、边界、默认值
    pub fn get_KvArray3D(&self) -> &[Vec<Vec<T::T_ZrConfig_Types_Kv>>] { &self.v_KvArray3D }
    // Money；按 Id 轮换常规、边界、默认值
    pub fn get_MoneyArray3D(&self) -> &[Vec<Vec<T::T_Game_Config_Money>>] { &self.v_MoneyArray3D }
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    pub fn get_Settings(&self) -> &Option<Box<T::T_Game_Config_BattleSettings>> { &self.v_Settings }
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    pub fn get_Range(&self) -> &T::T_Game_Config_ConfigRange { &self.v_Range }
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    pub fn get_Options(&self) -> &Option<Box<T::T_Game_Config_SampleOptions>> { &self.v_Options }
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    pub fn get_Action(&self) -> &Option<Box<T::T_Game_Config_BattleAction>> { &self.v_Action }
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    pub fn get_Actions(&self) -> &[Option<Box<T::T_Game_Config_BattleAction>>] { &self.v_Actions }
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    pub fn get_Ranges(&self) -> &[T::T_Game_Config_ConfigRange] { &self.v_Ranges }
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    pub fn get_OptionsList(&self) -> &[Option<Box<T::T_Game_Config_SampleOptions>>] { &self.v_OptionsList }
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    pub fn get_Groups(&self) -> &[Option<Box<T::T_Game_Config_ActionGroup>>] { &self.v_Groups }
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    pub fn get_Phases(&self) -> &[Option<Box<T::T_Game_Config_BattlePhase>>] { &self.v_Phases }
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    pub fn get_OptionalRange(&self) -> &Option<T::T_Game_Config_ConfigRange> { &self.v_OptionalRange }
    // NullableInt；按 Id 轮换常规、边界、默认值
    pub fn get_OptionalInt(&self) -> &Option<i32> { &self.v_OptionalInt }
    // NullableUInt；按 Id 轮换常规、边界、默认值
    pub fn get_OptionalUInt(&self) -> &Option<u32> { &self.v_OptionalUInt }
    // NullableULong；按 Id 轮换常规、边界、默认值
    pub fn get_OptionalULong(&self) -> &Option<u64> { &self.v_OptionalULong }
    // NullableDecimal；按 Id 轮换常规、边界、默认值
    pub fn get_OptionalDecimal(&self) -> &Option<R::Decimal> { &self.v_OptionalDecimal }
    // NullableBool；按 Id 轮换常规、边界、默认值
    pub fn get_OptionalBool(&self) -> &Option<bool> { &self.v_OptionalBool }
    // NullableQuality；按 Id 轮换常规、边界、默认值
    pub fn get_OptionalQuality(&self) -> &Option<T::T_Game_Config_Quality> { &self.v_OptionalQuality }
    // NullableMoney；按 Id 轮换常规、边界、默认值
    pub fn get_OptionalMoney(&self) -> &Option<T::T_Game_Config_Money> { &self.v_OptionalMoney }
    // NullableIntArray；按 Id 轮换常规、边界、默认值
    pub fn get_OptionalValues(&self) -> &[Option<i32>] { &self.v_OptionalValues }
    // MapInt；按 Id 轮换常规、边界、默认值
    pub fn get_StatMap(&self) -> &R::Dictionary<String, i32> { &self.v_StatMap }
    // MapNullableDecimal；按 Id 轮换常规、边界、默认值
    pub fn get_PriceMap(&self) -> &R::Dictionary<i32, Option<R::Decimal>> { &self.v_PriceMap }
    // MapUInt；按 Id 轮换常规、边界、默认值
    pub fn get_QualityMap(&self) -> &R::Dictionary<T::T_Game_Config_Quality, u32> { &self.v_QualityMap }
    // MapIntArray；按 Id 轮换常规、边界、默认值
    pub fn get_ListMap(&self) -> &R::Dictionary<String, Vec<i32>> { &self.v_ListMap }
    // MatrixInt；按 Id 轮换常规、边界、默认值
    pub fn get_IntMatrix(&self) -> &R::Matrix<i32> { &self.v_IntMatrix }
    // MatrixDecimal；按 Id 轮换常规、边界、默认值
    pub fn get_DecimalCube(&self) -> &R::Matrix<R::Decimal> { &self.v_DecimalCube }
    // MatrixNullableUInt；按 Id 轮换常规、边界、默认值
    pub fn get_OptionalMatrix(&self) -> &R::Matrix<Option<u32>> { &self.v_OptionalMatrix }
    // RawJson；按 Id 轮换常规、边界、默认值
    pub fn get_RawJsonValue(&self) -> &str { &self.v_RawJsonValue }
    // RawJson；按 Id 轮换常规、边界、默认值
    pub fn get_RawJsonArray(&self) -> &[String] { &self.v_RawJsonArray }
    // RawJson；按 Id 轮换常规、边界、默认值
    pub fn get_RawJsonArray2D(&self) -> &[Vec<String>] { &self.v_RawJsonArray2D }
    // RawJson；按 Id 轮换常规、边界、默认值
    pub fn get_RawJsonArray3D(&self) -> &[Vec<Vec<String>>] { &self.v_RawJsonArray3D }
    // Int；按 Id 轮换常规、边界、默认值
    pub fn get_JsonIntArray(&self) -> &[i32] { &self.v_JsonIntArray }
    // 常规=1，边界=2，默认=0；引用 Hero 表
    pub fn get_HeroId(&self) -> i32 { self.v_Hero }
    pub fn get_Hero(&self) -> Option<Arc<HeroConfig>> { self.ref_Hero.get().and_then(Weak::upgrade) }
    // 常规=10，边界=11，默认=0；引用 Skill 表
    pub fn get_SkillId(&self) -> i32 { self.v_Skill }
    pub fn get_Skill(&self) -> Option<Arc<SkillConfig>> { self.ref_Skill.get().and_then(Weak::upgrade) }
    // Id=1 自引用，其余引用 Id-1；用于按需加载引用链
    pub fn get_PreviousId(&self) -> i32 { self.v_Previous }
    pub fn get_Previous(&self) -> Option<Arc<AllTypes10kConfig>> { self.ref_Previous.get().and_then(Weak::upgrade) }
    fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Id: R::scalar::<i32>(v.field("Id")?)?,
            v_ByteValue: R::scalar::<u8>(v.field("ByteValue")?)?,
            v_ShortValue: R::scalar::<i16>(v.field("ShortValue")?)?,
            v_IntValue: R::scalar::<i32>(v.field("IntValue")?)?,
            v_LongValue: R::scalar::<i64>(v.field("LongValue")?)?,
            v_FloatValue: R::scalar::<f32>(v.field("FloatValue")?)?,
            v_DoubleValue: R::scalar::<f64>(v.field("DoubleValue")?)?,
            v_BoolValue: R::scalar::<bool>(v.field("BoolValue")?)?,
            v_StringValue: R::scalar::<String>(v.field("StringValue")?)?,
            v_SByteValue: R::scalar::<i8>(v.field("SByteValue")?)?,
            v_UShortValue: R::scalar::<u16>(v.field("UShortValue")?)?,
            v_UIntValue: R::scalar::<u32>(v.field("UIntValue")?)?,
            v_ULongValue: R::scalar::<u64>(v.field("ULongValue")?)?,
            v_DecimalValue: R::scalar::<R::Decimal>(v.field("DecimalValue")?)?,
            v_QualityValue: T::T_Game_Config_Quality::__zr_decode(v.field("QualityValue")?)?,
            v_Vector2Value: T::T_ZrConfig_Types_Vector2::__zr_decode(v.field("Vector2Value")?)?,
            v_Vector3Value: T::T_ZrConfig_Types_Vector3::__zr_decode(v.field("Vector3Value")?)?,
            v_KvValue: T::T_ZrConfig_Types_Kv::__zr_decode(v.field("KvValue")?)?,
            v_MoneyValue: T::T_Game_Config_Money::__zr_decode(v.field("MoneyValue")?)?,
            v_ByteArray: R::decode_list(v.field("ByteArray")?, |item0| R::scalar::<u8>(item0))?,
            v_ShortArray: R::decode_list(v.field("ShortArray")?, |item0| R::scalar::<i16>(item0))?,
            v_IntArray: R::decode_list(v.field("IntArray")?, |item0| R::scalar::<i32>(item0))?,
            v_LongArray: R::decode_list(v.field("LongArray")?, |item0| R::scalar::<i64>(item0))?,
            v_FloatArray: R::decode_list(v.field("FloatArray")?, |item0| R::scalar::<f32>(item0))?,
            v_DoubleArray: R::decode_list(v.field("DoubleArray")?, |item0| R::scalar::<f64>(item0))?,
            v_BoolArray: R::decode_list(v.field("BoolArray")?, |item0| R::scalar::<bool>(item0))?,
            v_StringArray: R::decode_list(v.field("StringArray")?, |item0| R::scalar::<String>(item0))?,
            v_SByteArray: R::decode_list(v.field("SByteArray")?, |item0| R::scalar::<i8>(item0))?,
            v_UShortArray: R::decode_list(v.field("UShortArray")?, |item0| R::scalar::<u16>(item0))?,
            v_UIntArray: R::decode_list(v.field("UIntArray")?, |item0| R::scalar::<u32>(item0))?,
            v_ULongArray: R::decode_list(v.field("ULongArray")?, |item0| R::scalar::<u64>(item0))?,
            v_DecimalArray: R::decode_list(v.field("DecimalArray")?, |item0| R::scalar::<R::Decimal>(item0))?,
            v_QualityArray: R::decode_list(v.field("QualityArray")?, |item0| T::T_Game_Config_Quality::__zr_decode(item0))?,
            v_Vector2Array: R::decode_list(v.field("Vector2Array")?, |item0| T::T_ZrConfig_Types_Vector2::__zr_decode(item0))?,
            v_Vector3Array: R::decode_list(v.field("Vector3Array")?, |item0| T::T_ZrConfig_Types_Vector3::__zr_decode(item0))?,
            v_KvArray: R::decode_list(v.field("KvArray")?, |item0| T::T_ZrConfig_Types_Kv::__zr_decode(item0))?,
            v_MoneyArray: R::decode_list(v.field("MoneyArray")?, |item0| T::T_Game_Config_Money::__zr_decode(item0))?,
            v_ByteArray2D: R::decode_list(v.field("ByteArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<u8>(item1)))?,
            v_ShortArray2D: R::decode_list(v.field("ShortArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<i16>(item1)))?,
            v_IntArray2D: R::decode_list(v.field("IntArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<i32>(item1)))?,
            v_LongArray2D: R::decode_list(v.field("LongArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<i64>(item1)))?,
            v_FloatArray2D: R::decode_list(v.field("FloatArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<f32>(item1)))?,
            v_DoubleArray2D: R::decode_list(v.field("DoubleArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<f64>(item1)))?,
            v_BoolArray2D: R::decode_list(v.field("BoolArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<bool>(item1)))?,
            v_StringArray2D: R::decode_list(v.field("StringArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<String>(item1)))?,
            v_SByteArray2D: R::decode_list(v.field("SByteArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<i8>(item1)))?,
            v_UShortArray2D: R::decode_list(v.field("UShortArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<u16>(item1)))?,
            v_UIntArray2D: R::decode_list(v.field("UIntArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<u32>(item1)))?,
            v_ULongArray2D: R::decode_list(v.field("ULongArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<u64>(item1)))?,
            v_DecimalArray2D: R::decode_list(v.field("DecimalArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<R::Decimal>(item1)))?,
            v_QualityArray2D: R::decode_list(v.field("QualityArray2D")?, |item0| R::decode_list(item0, |item1| T::T_Game_Config_Quality::__zr_decode(item1)))?,
            v_Vector2Array2D: R::decode_list(v.field("Vector2Array2D")?, |item0| R::decode_list(item0, |item1| T::T_ZrConfig_Types_Vector2::__zr_decode(item1)))?,
            v_Vector3Array2D: R::decode_list(v.field("Vector3Array2D")?, |item0| R::decode_list(item0, |item1| T::T_ZrConfig_Types_Vector3::__zr_decode(item1)))?,
            v_KvArray2D: R::decode_list(v.field("KvArray2D")?, |item0| R::decode_list(item0, |item1| T::T_ZrConfig_Types_Kv::__zr_decode(item1)))?,
            v_MoneyArray2D: R::decode_list(v.field("MoneyArray2D")?, |item0| R::decode_list(item0, |item1| T::T_Game_Config_Money::__zr_decode(item1)))?,
            v_ByteArray3D: R::decode_list(v.field("ByteArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<u8>(item2))))?,
            v_ShortArray3D: R::decode_list(v.field("ShortArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<i16>(item2))))?,
            v_IntArray3D: R::decode_list(v.field("IntArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<i32>(item2))))?,
            v_LongArray3D: R::decode_list(v.field("LongArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<i64>(item2))))?,
            v_FloatArray3D: R::decode_list(v.field("FloatArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<f32>(item2))))?,
            v_DoubleArray3D: R::decode_list(v.field("DoubleArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<f64>(item2))))?,
            v_BoolArray3D: R::decode_list(v.field("BoolArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<bool>(item2))))?,
            v_StringArray3D: R::decode_list(v.field("StringArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<String>(item2))))?,
            v_SByteArray3D: R::decode_list(v.field("SByteArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<i8>(item2))))?,
            v_UShortArray3D: R::decode_list(v.field("UShortArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<u16>(item2))))?,
            v_UIntArray3D: R::decode_list(v.field("UIntArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<u32>(item2))))?,
            v_ULongArray3D: R::decode_list(v.field("ULongArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<u64>(item2))))?,
            v_DecimalArray3D: R::decode_list(v.field("DecimalArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<R::Decimal>(item2))))?,
            v_QualityArray3D: R::decode_list(v.field("QualityArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_Game_Config_Quality::__zr_decode(item2))))?,
            v_Vector2Array3D: R::decode_list(v.field("Vector2Array3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_ZrConfig_Types_Vector2::__zr_decode(item2))))?,
            v_Vector3Array3D: R::decode_list(v.field("Vector3Array3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_ZrConfig_Types_Vector3::__zr_decode(item2))))?,
            v_KvArray3D: R::decode_list(v.field("KvArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_ZrConfig_Types_Kv::__zr_decode(item2))))?,
            v_MoneyArray3D: R::decode_list(v.field("MoneyArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_Game_Config_Money::__zr_decode(item2))))?,
            v_Settings: R::decode_optional(v.field("Settings")?, |v| T::T_Game_Config_BattleSettings::__zr_decode(v).map(Box::new))?,
            v_Range: T::T_Game_Config_ConfigRange::__zr_decode(v.field("Range")?)?,
            v_Options: R::decode_optional(v.field("Options")?, |v| T::T_Game_Config_SampleOptions::__zr_decode(v).map(Box::new))?,
            v_Action: R::decode_optional(v.field("Action")?, |v| T::T_Game_Config_BattleAction::__zr_decode(v).map(Box::new))?,
            v_Actions: R::decode_list(v.field("Actions")?, |item0| R::decode_optional(item0, |v| T::T_Game_Config_BattleAction::__zr_decode(v).map(Box::new)))?,
            v_Ranges: R::decode_list(v.field("Ranges")?, |item0| T::T_Game_Config_ConfigRange::__zr_decode(item0))?,
            v_OptionsList: R::decode_list(v.field("OptionsList")?, |item0| R::decode_optional(item0, |v| T::T_Game_Config_SampleOptions::__zr_decode(v).map(Box::new)))?,
            v_Groups: R::decode_list(v.field("Groups")?, |item0| R::decode_optional(item0, |v| T::T_Game_Config_ActionGroup::__zr_decode(v).map(Box::new)))?,
            v_Phases: R::decode_list(v.field("Phases")?, |item0| R::decode_optional(item0, |v| T::T_Game_Config_BattlePhase::__zr_decode(v).map(Box::new)))?,
            v_OptionalRange: R::decode_optional(v.field("OptionalRange")?, |item0| T::T_Game_Config_ConfigRange::__zr_decode(item0))?,
            v_OptionalInt: R::decode_optional(v.field("OptionalInt")?, |item0| R::scalar::<i32>(item0))?,
            v_OptionalUInt: R::decode_optional(v.field("OptionalUInt")?, |item0| R::scalar::<u32>(item0))?,
            v_OptionalULong: R::decode_optional(v.field("OptionalULong")?, |item0| R::scalar::<u64>(item0))?,
            v_OptionalDecimal: R::decode_optional(v.field("OptionalDecimal")?, |item0| R::scalar::<R::Decimal>(item0))?,
            v_OptionalBool: R::decode_optional(v.field("OptionalBool")?, |item0| R::scalar::<bool>(item0))?,
            v_OptionalQuality: R::decode_optional(v.field("OptionalQuality")?, |item0| T::T_Game_Config_Quality::__zr_decode(item0))?,
            v_OptionalMoney: R::decode_optional(v.field("OptionalMoney")?, |item0| T::T_Game_Config_Money::__zr_decode(item0))?,
            v_OptionalValues: R::decode_list(v.field("OptionalValues")?, |item0| R::decode_optional(item0, |item1| R::scalar::<i32>(item1)))?,
            v_StatMap: R::decode_dictionary(v.field("StatMap")?, |item0| R::scalar::<String>(item0), |item0| R::scalar::<i32>(item0))?,
            v_PriceMap: R::decode_dictionary(v.field("PriceMap")?, |item0| R::scalar::<i32>(item0), |item0| R::decode_optional(item0, |item1| R::scalar::<R::Decimal>(item1)))?,
            v_QualityMap: R::decode_dictionary(v.field("QualityMap")?, |item0| T::T_Game_Config_Quality::__zr_decode(item0), |item0| R::scalar::<u32>(item0))?,
            v_ListMap: R::decode_dictionary(v.field("ListMap")?, |item0| R::scalar::<String>(item0), |item0| R::decode_list(item0, |item1| R::scalar::<i32>(item1)))?,
            v_IntMatrix: R::decode_matrix(v.field("IntMatrix")?, |item0| R::scalar::<i32>(item0))?,
            v_DecimalCube: R::decode_matrix(v.field("DecimalCube")?, |item0| R::scalar::<R::Decimal>(item0))?,
            v_OptionalMatrix: R::decode_matrix(v.field("OptionalMatrix")?, |item0| R::decode_optional(item0, |item1| R::scalar::<u32>(item1)))?,
            v_RawJsonValue: R::scalar::<String>(v.field("RawJsonValue")?)?,
            v_RawJsonArray: R::decode_list(v.field("RawJsonArray")?, |item0| R::scalar::<String>(item0))?,
            v_RawJsonArray2D: R::decode_list(v.field("RawJsonArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<String>(item1)))?,
            v_RawJsonArray3D: R::decode_list(v.field("RawJsonArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<String>(item2))))?,
            v_JsonIntArray: R::decode_list(v.field("JsonIntArray")?, |item0| R::scalar::<i32>(item0))?,
            v_Hero: R::scalar::<i32>(v.field("Hero")?)?,
            ref_Hero: OnceLock::new(),
            v_Skill: R::scalar::<i32>(v.field("Skill")?)?,
            ref_Skill: OnceLock::new(),
            v_Previous: R::scalar::<i32>(v.field("Previous")?)?,
            ref_Previous: OnceLock::new(),
        })
    }
}
pub struct AllTypes10kTable {
    pub(super) rows: Vec<Arc<AllTypes10kConfig>>,
    pub(super) index_Key: R::Index,
    pub(super) index_Group_ById: R::Index,
    pub(super) ids: std::collections::HashMap<i32, Arc<AllTypes10kConfig>>,
}
impl AllTypes10kTable {
    pub fn rows(&self) -> &[Arc<AllTypes10kConfig>] { &self.rows }
    pub fn count(&self) -> usize { self.rows.len() }
    pub(super) fn __zr_load(bundle: &R::Bundle, prefer_json: bool) -> Result<Self> {
        let source = bundle.load("{\"name\":\"AllTypes10k\",\"hash\":\"34B94C4229D85CC8B37832A203504B7E6EF4447307ADB831AB5BD01628EA7432\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"唯一主键：1～10000\"},{\"name\":\"ByteValue\",\"type\":\"System.Byte\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortValue\",\"type\":\"System.Int16\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntValue\",\"type\":\"System.Int32\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongValue\",\"type\":\"System.Int64\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatValue\",\"type\":\"System.Single\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleValue\",\"type\":\"System.Double\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolValue\",\"type\":\"System.Boolean\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringValue\",\"type\":\"System.String\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteValue\",\"type\":\"System.SByte\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortValue\",\"type\":\"System.UInt16\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntValue\",\"type\":\"System.UInt32\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongValue\",\"type\":\"System.UInt64\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalValue\",\"type\":\"System.Decimal\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityValue\",\"type\":\"Game.Config.Quality\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Value\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Value\",\"type\":\"ZrConfig.Types.Vector3\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvValue\",\"type\":\"ZrConfig.Types.Kv\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyValue\",\"type\":\"Game.Config.Money\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ByteArray\",\"type\":\"System.Byte[]\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortArray\",\"type\":\"System.Int16[]\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntArray\",\"type\":\"System.Int32[]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongArray\",\"type\":\"System.Int64[]\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatArray\",\"type\":\"System.Single[]\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleArray\",\"type\":\"System.Double[]\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolArray\",\"type\":\"System.Boolean[]\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringArray\",\"type\":\"System.String[]\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteArray\",\"type\":\"System.SByte[]\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortArray\",\"type\":\"System.UInt16[]\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntArray\",\"type\":\"System.UInt32[]\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongArray\",\"type\":\"System.UInt64[]\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalArray\",\"type\":\"System.Decimal[]\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityArray\",\"type\":\"Game.Config.Quality[]\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Array\",\"type\":\"ZrConfig.Types.Vector2[]\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Array\",\"type\":\"ZrConfig.Types.Vector3[]\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvArray\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyArray\",\"type\":\"Game.Config.Money[]\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ByteArray2D\",\"type\":\"System.Byte[][]\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortArray2D\",\"type\":\"System.Int16[][]\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntArray2D\",\"type\":\"System.Int32[][]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongArray2D\",\"type\":\"System.Int64[][]\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatArray2D\",\"type\":\"System.Single[][]\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleArray2D\",\"type\":\"System.Double[][]\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolArray2D\",\"type\":\"System.Boolean[][]\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringArray2D\",\"type\":\"System.String[][]\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteArray2D\",\"type\":\"System.SByte[][]\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortArray2D\",\"type\":\"System.UInt16[][]\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntArray2D\",\"type\":\"System.UInt32[][]\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongArray2D\",\"type\":\"System.UInt64[][]\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalArray2D\",\"type\":\"System.Decimal[][]\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityArray2D\",\"type\":\"Game.Config.Quality[][]\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Array2D\",\"type\":\"ZrConfig.Types.Vector2[][]\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Array2D\",\"type\":\"ZrConfig.Types.Vector3[][]\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvArray2D\",\"type\":\"ZrConfig.Types.Kv[][]\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyArray2D\",\"type\":\"Game.Config.Money[][]\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ByteArray3D\",\"type\":\"System.Byte[][][]\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortArray3D\",\"type\":\"System.Int16[][][]\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntArray3D\",\"type\":\"System.Int32[][][]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongArray3D\",\"type\":\"System.Int64[][][]\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatArray3D\",\"type\":\"System.Single[][][]\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleArray3D\",\"type\":\"System.Double[][][]\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolArray3D\",\"type\":\"System.Boolean[][][]\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringArray3D\",\"type\":\"System.String[][][]\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteArray3D\",\"type\":\"System.SByte[][][]\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortArray3D\",\"type\":\"System.UInt16[][][]\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntArray3D\",\"type\":\"System.UInt32[][][]\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongArray3D\",\"type\":\"System.UInt64[][][]\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalArray3D\",\"type\":\"System.Decimal[][][]\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityArray3D\",\"type\":\"Game.Config.Quality[][][]\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Array3D\",\"type\":\"ZrConfig.Types.Vector2[][][]\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Array3D\",\"type\":\"ZrConfig.Types.Vector3[][][]\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvArray3D\",\"type\":\"ZrConfig.Types.Kv[][][]\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyArray3D\",\"type\":\"Game.Config.Money[][][]\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Settings\",\"type\":\"Game.Config.BattleSettings\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Options\",\"type\":\"Game.Config.SampleOptions\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Action\",\"type\":\"Game.Config.BattleAction\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Ranges\",\"type\":\"Game.Config.ConfigRange[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"OptionsList\",\"type\":\"Game.Config.SampleOptions[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Phases\",\"type\":\"Game.Config.BattlePhase[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"OptionalRange\",\"type\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"OptionalInt\",\"type\":\"System.Nullable`1<System.Int32>\",\"description\":\"NullableInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalUInt\",\"type\":\"System.Nullable`1<System.UInt32>\",\"description\":\"NullableUInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalULong\",\"type\":\"System.Nullable`1<System.UInt64>\",\"description\":\"NullableULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalDecimal\",\"type\":\"System.Nullable`1<System.Decimal>\",\"description\":\"NullableDecimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalBool\",\"type\":\"System.Nullable`1<System.Boolean>\",\"description\":\"NullableBool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalQuality\",\"type\":\"System.Nullable`1<Game.Config.Quality>\",\"description\":\"NullableQuality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalMoney\",\"type\":\"System.Nullable`1<Game.Config.Money>\",\"description\":\"NullableMoney；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalValues\",\"type\":\"System.Nullable`1<System.Int32>[]\",\"description\":\"NullableIntArray；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StatMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"description\":\"MapInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"PriceMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"description\":\"MapNullableDecimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityMap\",\"type\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"description\":\"MapUInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ListMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"description\":\"MapIntArray；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntMatrix\",\"type\":\"System.Int32[,]\",\"description\":\"MatrixInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalCube\",\"type\":\"System.Decimal[,,]\",\"description\":\"MatrixDecimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalMatrix\",\"type\":\"System.Nullable`1<System.UInt32>[,]\",\"description\":\"MatrixNullableUInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonValue\",\"type\":\"ZrConfig.Types.RawJson\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonArray\",\"type\":\"ZrConfig.Types.RawJson[]\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonArray2D\",\"type\":\"ZrConfig.Types.RawJson[][]\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonArray3D\",\"type\":\"ZrConfig.Types.RawJson[][][]\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"JsonIntArray\",\"type\":\"System.Int32[]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Hero\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"常规=1，边界=2，默认=0；引用 Hero 表\"},{\"name\":\"Skill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"常规=10，边界=11，默认=0；引用 Skill 表\"},{\"name\":\"Previous\",\"type\":\"ref:ZrConfig.Models.AllTypes10kConfig:Id:int:zero-null/1\",\"description\":\"Id=1 自引用，其余引用 Id-1；用于按需加载引用链\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"Game.Config.ActionGroup\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ActionGroup[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ActionGroup\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.AddBuff\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"BuffId\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Stacks\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"enumValues\":[],\"abstract\":true,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattleAction\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattlePhase\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleSettings\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"MaxTargets\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Interruptible\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Min\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Max\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Damage\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Ratio\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"CanCrit\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money\",\"kind\":\"converter\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Cents\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality\",\"kind\":\"enum\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[\"0\",\"1\",\"2\"],\"abstract\":false,\"baseType\":null,\"enumMembers\":{\"None\":\"0\",\"Rare\":\"1\",\"Legendary\":\"2\"},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"\"},{\"name\":\"Rewards\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"\"},{\"name\":\"Offset\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"\"},{\"name\":\"Label\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Quality\",\"type\":\"Game.Config.Quality\",\"description\":\"\"},{\"name\":\"Levels\",\"type\":\"System.Int32[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.SampleOptions\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte\",\"kind\":\"primitive\",\"primitive\":\"uint8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":\"Game.Config.Quality\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Nullable`1<System.Decimal>\",\"key\":\"System.Int32\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32[]\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal\",\"kind\":\"primitive\",\"primitive\":\"decimal\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[,,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":3,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double\",\"kind\":\"primitive\",\"primitive\":\"float64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16\",\"kind\":\"primitive\",\"primitive\":\"int16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64\",\"kind\":\"primitive\",\"primitive\":\"int64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Money>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Quality>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Boolean>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Decimal>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Nullable`1<System.Int32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Nullable`1<System.UInt32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt64>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte\",\"kind\":\"primitive\",\"primitive\":\"int8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single\",\"kind\":\"primitive\",\"primitive\":\"float32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16\",\"kind\":\"primitive\",\"primitive\":\"uint16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32\",\"kind\":\"primitive\",\"primitive\":\"uint32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64\",\"kind\":\"primitive\",\"primitive\":\"uint64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv\",\"kind\":\"kv\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Key\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Value\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson\",\"kind\":\"rawjson\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2\",\"kind\":\"vector2\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3\",\"kind\":\"vector3\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Z\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.AllTypes10kConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"AllTypes10k\"},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"},{\"id\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Skill\"}],\"tags\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"dependencies\":[\"AllTypes10k\",\"Hero\",\"Skill\"]}", prefer_json)?;
        let rows = source.iter().map(|v| AllTypes10kConfig::__zr_decode(v).map(Arc::new)).collect::<Result<Vec<_>>>()?;
        let index_Key = R::Index::build(rows.len(), true, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        let index_Group_ById = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        let mut ids = std::collections::HashMap::with_capacity(rows.len());
        for row in &rows { if ids.insert(row.v_Id, row.clone()).is_some() { return R::invalid("引用 Id 不唯一"); } }
        Ok(Self { rows, index_Key, index_Group_ById, ids })
    }
    pub(super) fn __zr_bind(&self, _snapshot: &ConfigSnapshot) -> Result<()> {
        for row in &self.rows { if row.v_Hero != 0 { let target = _snapshot.get_Hero().ids.get(&row.v_Hero).ok_or("引用目标不存在：AllTypes10k.Hero")?; row.ref_Hero.set(Arc::downgrade(target)).map_err(|_| "引用重复绑定".to_owned())?; } }
        for row in &self.rows { if row.v_Skill != 0 { let target = _snapshot.get_Skill().ids.get(&row.v_Skill).ok_or("引用目标不存在：AllTypes10k.Skill")?; row.ref_Skill.set(Arc::downgrade(target)).map_err(|_| "引用重复绑定".to_owned())?; } }
        for row in &self.rows { if row.v_Previous != 0 { let target = _snapshot.get_AllTypes10k().ids.get(&row.v_Previous).ok_or("引用目标不存在：AllTypes10k.Previous")?; row.ref_Previous.set(Arc::downgrade(target)).map_err(|_| "引用重复绑定".to_owned())?; } }
        Ok(())
    }
    pub fn find(&self, k0: i32) -> Option<&AllTypes10kConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Key.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        slot.map(|s| self.rows[self.index_Key.row(s)].as_ref())
    }
    pub fn get(&self, k0: i32) -> Result<&AllTypes10kConfig> { self.find(k0).ok_or_else(|| "配置主键不存在".to_owned()) }
    pub fn get_ById(&self, k0: i32) -> R::Group<'_, AllTypes10kConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_ById.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_ById.group(slot) }
    }
}
