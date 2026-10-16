// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
use std::sync::{Arc, Weak, OnceLock};
use super::runtime::{self as R, Result, Json};
use super::types as T;
use super::snapshot::ConfigSnapshot;
pub struct AllTypesConfig {
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
}
impl AllTypesConfig {
    // 1=常规值；2=边界值；3=默认值。重复 Id 的多行组成同一条配置。
    pub fn get_Id(&self) -> i32 { self.v_Id }
    // 基础整数；标量
    pub fn get_ByteValue(&self) -> u8 { self.v_ByteValue }
    // 基础整数；标量
    pub fn get_ShortValue(&self) -> i16 { self.v_ShortValue }
    // 基础整数；标量
    pub fn get_IntValue(&self) -> i32 { self.v_IntValue }
    // 基础整数；标量；文本存储，保留完整精度
    pub fn get_LongValue(&self) -> i64 { self.v_LongValue }
    // 基础浮点；标量
    pub fn get_FloatValue(&self) -> f32 { self.v_FloatValue }
    // 基础浮点；标量
    pub fn get_DoubleValue(&self) -> f64 { self.v_DoubleValue }
    // 基础布尔；标量
    pub fn get_BoolValue(&self) -> bool { self.v_BoolValue }
    // 基础字符串；标量
    pub fn get_StringValue(&self) -> &str { &self.v_StringValue }
    // 基础整数；标量
    pub fn get_SByteValue(&self) -> i8 { self.v_SByteValue }
    // 基础整数；标量
    pub fn get_UShortValue(&self) -> u16 { self.v_UShortValue }
    // 基础整数；标量
    pub fn get_UIntValue(&self) -> u32 { self.v_UIntValue }
    // 基础整数；标量；文本存储，保留完整精度
    pub fn get_ULongValue(&self) -> u64 { self.v_ULongValue }
    // 精确十进制；标量；文本存储，保留完整精度
    pub fn get_DecimalValue(&self) -> R::Decimal { self.v_DecimalValue }
    // 枚举；标量
    pub fn get_QualityValue(&self) -> T::T_Game_Config_Quality { self.v_QualityValue }
    // 二维向量；标量
    pub fn get_Vector2Value(&self) -> &T::T_ZrConfig_Types_Vector2 { &self.v_Vector2Value }
    // 三维向量；标量
    pub fn get_Vector3Value(&self) -> &T::T_ZrConfig_Types_Vector3 { &self.v_Vector3Value }
    // 整数键值对；标量
    pub fn get_KvValue(&self) -> &T::T_ZrConfig_Types_Kv { &self.v_KvValue }
    // 外部特殊标量转换器；标量
    pub fn get_MoneyValue(&self) -> &T::T_Game_Config_Money { &self.v_MoneyValue }
    // 基础整数；一维数组；元素用分号分隔
    pub fn get_ByteArray(&self) -> &[u8] { &self.v_ByteArray }
    // 基础整数；一维数组；元素用分号分隔
    pub fn get_ShortArray(&self) -> &[i16] { &self.v_ShortArray }
    // 基础整数；一维数组；元素用分号分隔
    pub fn get_IntArray(&self) -> &[i32] { &self.v_IntArray }
    // 基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度
    pub fn get_LongArray(&self) -> &[i64] { &self.v_LongArray }
    // 基础浮点；一维数组；元素用分号分隔
    pub fn get_FloatArray(&self) -> &[f32] { &self.v_FloatArray }
    // 基础浮点；一维数组；元素用分号分隔
    pub fn get_DoubleArray(&self) -> &[f64] { &self.v_DoubleArray }
    // 基础布尔；一维数组；元素用分号分隔
    pub fn get_BoolArray(&self) -> &[bool] { &self.v_BoolArray }
    // 基础字符串；一维数组；元素用分号分隔
    pub fn get_StringArray(&self) -> &[String] { &self.v_StringArray }
    // 基础整数；一维数组；元素用分号分隔
    pub fn get_SByteArray(&self) -> &[i8] { &self.v_SByteArray }
    // 基础整数；一维数组；元素用分号分隔
    pub fn get_UShortArray(&self) -> &[u16] { &self.v_UShortArray }
    // 基础整数；一维数组；元素用分号分隔
    pub fn get_UIntArray(&self) -> &[u32] { &self.v_UIntArray }
    // 基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度
    pub fn get_ULongArray(&self) -> &[u64] { &self.v_ULongArray }
    // 精确十进制；一维数组；元素用分号分隔；文本存储，保留完整精度
    pub fn get_DecimalArray(&self) -> &[R::Decimal] { &self.v_DecimalArray }
    // 枚举；一维数组；元素用分号分隔
    pub fn get_QualityArray(&self) -> &[T::T_Game_Config_Quality] { &self.v_QualityArray }
    // 二维向量；一维数组；元素用分号分隔
    pub fn get_Vector2Array(&self) -> &[T::T_ZrConfig_Types_Vector2] { &self.v_Vector2Array }
    // 三维向量；一维数组；元素用分号分隔
    pub fn get_Vector3Array(&self) -> &[T::T_ZrConfig_Types_Vector3] { &self.v_Vector3Array }
    // 整数键值对；一维数组；元素用分号分隔
    pub fn get_KvArray(&self) -> &[T::T_ZrConfig_Types_Kv] { &self.v_KvArray }
    // 外部特殊标量转换器；一维数组；元素用分号分隔
    pub fn get_MoneyArray(&self) -> &[T::T_Game_Config_Money] { &self.v_MoneyArray }
    // 基础整数；二维交错数组；元素用分号分隔
    pub fn get_ByteArray2D(&self) -> &[Vec<u8>] { &self.v_ByteArray2D }
    // 基础整数；二维交错数组；元素用分号分隔
    pub fn get_ShortArray2D(&self) -> &[Vec<i16>] { &self.v_ShortArray2D }
    // 基础整数；二维交错数组；元素用分号分隔
    pub fn get_IntArray2D(&self) -> &[Vec<i32>] { &self.v_IntArray2D }
    // 基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度
    pub fn get_LongArray2D(&self) -> &[Vec<i64>] { &self.v_LongArray2D }
    // 基础浮点；二维交错数组；元素用分号分隔
    pub fn get_FloatArray2D(&self) -> &[Vec<f32>] { &self.v_FloatArray2D }
    // 基础浮点；二维交错数组；元素用分号分隔
    pub fn get_DoubleArray2D(&self) -> &[Vec<f64>] { &self.v_DoubleArray2D }
    // 基础布尔；二维交错数组；元素用分号分隔
    pub fn get_BoolArray2D(&self) -> &[Vec<bool>] { &self.v_BoolArray2D }
    // 基础字符串；二维交错数组；元素用分号分隔
    pub fn get_StringArray2D(&self) -> &[Vec<String>] { &self.v_StringArray2D }
    // 基础整数；二维交错数组；元素用分号分隔
    pub fn get_SByteArray2D(&self) -> &[Vec<i8>] { &self.v_SByteArray2D }
    // 基础整数；二维交错数组；元素用分号分隔
    pub fn get_UShortArray2D(&self) -> &[Vec<u16>] { &self.v_UShortArray2D }
    // 基础整数；二维交错数组；元素用分号分隔
    pub fn get_UIntArray2D(&self) -> &[Vec<u32>] { &self.v_UIntArray2D }
    // 基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度
    pub fn get_ULongArray2D(&self) -> &[Vec<u64>] { &self.v_ULongArray2D }
    // 精确十进制；二维交错数组；元素用分号分隔；文本存储，保留完整精度
    pub fn get_DecimalArray2D(&self) -> &[Vec<R::Decimal>] { &self.v_DecimalArray2D }
    // 枚举；二维交错数组；元素用分号分隔
    pub fn get_QualityArray2D(&self) -> &[Vec<T::T_Game_Config_Quality>] { &self.v_QualityArray2D }
    // 二维向量；二维交错数组；元素用分号分隔
    pub fn get_Vector2Array2D(&self) -> &[Vec<T::T_ZrConfig_Types_Vector2>] { &self.v_Vector2Array2D }
    // 三维向量；二维交错数组；元素用分号分隔
    pub fn get_Vector3Array2D(&self) -> &[Vec<T::T_ZrConfig_Types_Vector3>] { &self.v_Vector3Array2D }
    // 整数键值对；二维交错数组；元素用分号分隔
    pub fn get_KvArray2D(&self) -> &[Vec<T::T_ZrConfig_Types_Kv>] { &self.v_KvArray2D }
    // 外部特殊标量转换器；二维交错数组；元素用分号分隔
    pub fn get_MoneyArray2D(&self) -> &[Vec<T::T_Game_Config_Money>] { &self.v_MoneyArray2D }
    // 基础整数；三维交错数组；元素用分号分隔
    pub fn get_ByteArray3D(&self) -> &[Vec<Vec<u8>>] { &self.v_ByteArray3D }
    // 基础整数；三维交错数组；元素用分号分隔
    pub fn get_ShortArray3D(&self) -> &[Vec<Vec<i16>>] { &self.v_ShortArray3D }
    // 基础整数；三维交错数组；元素用分号分隔
    pub fn get_IntArray3D(&self) -> &[Vec<Vec<i32>>] { &self.v_IntArray3D }
    // 基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度
    pub fn get_LongArray3D(&self) -> &[Vec<Vec<i64>>] { &self.v_LongArray3D }
    // 基础浮点；三维交错数组；元素用分号分隔
    pub fn get_FloatArray3D(&self) -> &[Vec<Vec<f32>>] { &self.v_FloatArray3D }
    // 基础浮点；三维交错数组；元素用分号分隔
    pub fn get_DoubleArray3D(&self) -> &[Vec<Vec<f64>>] { &self.v_DoubleArray3D }
    // 基础布尔；三维交错数组；元素用分号分隔
    pub fn get_BoolArray3D(&self) -> &[Vec<Vec<bool>>] { &self.v_BoolArray3D }
    // 基础字符串；三维交错数组；元素用分号分隔
    pub fn get_StringArray3D(&self) -> &[Vec<Vec<String>>] { &self.v_StringArray3D }
    // 基础整数；三维交错数组；元素用分号分隔
    pub fn get_SByteArray3D(&self) -> &[Vec<Vec<i8>>] { &self.v_SByteArray3D }
    // 基础整数；三维交错数组；元素用分号分隔
    pub fn get_UShortArray3D(&self) -> &[Vec<Vec<u16>>] { &self.v_UShortArray3D }
    // 基础整数；三维交错数组；元素用分号分隔
    pub fn get_UIntArray3D(&self) -> &[Vec<Vec<u32>>] { &self.v_UIntArray3D }
    // 基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度
    pub fn get_ULongArray3D(&self) -> &[Vec<Vec<u64>>] { &self.v_ULongArray3D }
    // 精确十进制；三维交错数组；元素用分号分隔；文本存储，保留完整精度
    pub fn get_DecimalArray3D(&self) -> &[Vec<Vec<R::Decimal>>] { &self.v_DecimalArray3D }
    // 枚举；三维交错数组；元素用分号分隔
    pub fn get_QualityArray3D(&self) -> &[Vec<Vec<T::T_Game_Config_Quality>>] { &self.v_QualityArray3D }
    // 二维向量；三维交错数组；元素用分号分隔
    pub fn get_Vector2Array3D(&self) -> &[Vec<Vec<T::T_ZrConfig_Types_Vector2>>] { &self.v_Vector2Array3D }
    // 三维向量；三维交错数组；元素用分号分隔
    pub fn get_Vector3Array3D(&self) -> &[Vec<Vec<T::T_ZrConfig_Types_Vector3>>] { &self.v_Vector3Array3D }
    // 整数键值对；三维交错数组；元素用分号分隔
    pub fn get_KvArray3D(&self) -> &[Vec<Vec<T::T_ZrConfig_Types_Kv>>] { &self.v_KvArray3D }
    // 外部特殊标量转换器；三维交错数组；元素用分号分隔
    pub fn get_MoneyArray3D(&self) -> &[Vec<Vec<T::T_Game_Config_Money>>] { &self.v_MoneyArray3D }
    // 最多目标数
    pub fn get_Settings(&self) -> &Option<Box<T::T_Game_Config_BattleSettings>> { &self.v_Settings }
    // 结构体最小值
    pub fn get_Range(&self) -> &T::T_Game_Config_ConfigRange { &self.v_Range }
    // 类的可写属性
    pub fn get_Options(&self) -> &Option<Box<T::T_Game_Config_SampleOptions>> { &self.v_Options }
    // 单个多态对象：选择 Damage / AddBuff
    pub fn get_Action(&self) -> &Option<Box<T::T_Game_Config_BattleAction>> { &self.v_Action }
    // 动作顺序：同一 Id 从 1 开始；一行一个动作
    pub fn get_Actions(&self) -> &[Option<Box<T::T_Game_Config_BattleAction>>] { &self.v_Actions }
    // 普通结构体数组也能多行；独立排序
    pub fn get_Ranges(&self) -> &[T::T_Game_Config_ConfigRange] { &self.v_Ranges }
    // 普通 class 数组，每行一个对象
    pub fn get_OptionsList(&self) -> &[Option<Box<T::T_Game_Config_SampleOptions>>] { &self.v_OptionsList }
    // 父动作组编号；同一个编号的续行归入同一组
    pub fn get_Groups(&self) -> &[Option<Box<T::T_Game_Config_ActionGroup>>] { &self.v_Groups }
    // 阶段编号；续行重复填写
    pub fn get_Phases(&self) -> &[Option<Box<T::T_Game_Config_BattlePhase>>] { &self.v_Phases }
    // 可空结构体；整组空白表示 null
    pub fn get_OptionalRange(&self) -> &Option<T::T_Game_Config_ConfigRange> { &self.v_OptionalRange }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_OptionalInt(&self) -> &Option<i32> { &self.v_OptionalInt }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_OptionalUInt(&self) -> &Option<u32> { &self.v_OptionalUInt }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_OptionalULong(&self) -> &Option<u64> { &self.v_OptionalULong }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_OptionalDecimal(&self) -> &Option<R::Decimal> { &self.v_OptionalDecimal }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_OptionalBool(&self) -> &Option<bool> { &self.v_OptionalBool }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_OptionalQuality(&self) -> &Option<T::T_Game_Config_Quality> { &self.v_OptionalQuality }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_OptionalMoney(&self) -> &Option<T::T_Game_Config_Money> { &self.v_OptionalMoney }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_OptionalValues(&self) -> &[Option<i32>] { &self.v_OptionalValues }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_StatMap(&self) -> &R::Dictionary<String, i32> { &self.v_StatMap }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_PriceMap(&self) -> &R::Dictionary<i32, Option<R::Decimal>> { &self.v_PriceMap }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_QualityMap(&self) -> &R::Dictionary<T::T_Game_Config_Quality, u32> { &self.v_QualityMap }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_ListMap(&self) -> &R::Dictionary<String, Vec<i32>> { &self.v_ListMap }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_IntMatrix(&self) -> &R::Matrix<i32> { &self.v_IntMatrix }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_DecimalCube(&self) -> &R::Matrix<R::Decimal> { &self.v_DecimalCube }
    // 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
    pub fn get_OptionalMatrix(&self) -> &R::Matrix<Option<u32>> { &self.v_OptionalMatrix }
    // 原始 JSON；标量；这是 JSON 类型本身的样例
    pub fn get_RawJsonValue(&self) -> &str { &self.v_RawJsonValue }
    // 原始 JSON；一维数组；这是 JSON 类型本身的样例
    pub fn get_RawJsonArray(&self) -> &[String] { &self.v_RawJsonArray }
    // 原始 JSON；二维交错数组；这是 JSON 类型本身的样例
    pub fn get_RawJsonArray2D(&self) -> &[Vec<String>] { &self.v_RawJsonArray2D }
    // 原始 JSON；三维交错数组；这是 JSON 类型本身的样例
    pub fn get_RawJsonArray3D(&self) -> &[Vec<Vec<String>>] { &self.v_RawJsonArray3D }
    // 基础整数；一维数组；json<T> 输入格式示例，用逗号分隔
    pub fn get_JsonIntArray(&self) -> &[i32] { &self.v_JsonIntArray }
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
        })
    }
}
pub struct AllTypesTable {
    pub(super) rows: Vec<Arc<AllTypesConfig>>,
    pub(super) index_Key: R::Index,
    pub(super) index_Group_ById: R::Index,
    pub(super) index_Group_ByQuality: R::Index,
}
impl AllTypesTable {
    pub fn rows(&self) -> &[Arc<AllTypesConfig>] { &self.rows }
    pub fn count(&self) -> usize { self.rows.len() }
    pub(super) fn __zr_load(bundle: &R::Bundle, prefer_json: bool) -> Result<Self> {
        let source = bundle.load("{\"name\":\"AllTypes\",\"hash\":\"E8017397D24CBD0FDA2008AFD4F2BA2BA68619A29F4D4D79F7973F06BC8360B1\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"1=常规值；2=边界值；3=默认值。重复 Id 的多行组成同一条配置。\"},{\"name\":\"ByteValue\",\"type\":\"System.Byte\",\"description\":\"基础整数；标量\"},{\"name\":\"ShortValue\",\"type\":\"System.Int16\",\"description\":\"基础整数；标量\"},{\"name\":\"IntValue\",\"type\":\"System.Int32\",\"description\":\"基础整数；标量\"},{\"name\":\"LongValue\",\"type\":\"System.Int64\",\"description\":\"基础整数；标量；文本存储，保留完整精度\"},{\"name\":\"FloatValue\",\"type\":\"System.Single\",\"description\":\"基础浮点；标量\"},{\"name\":\"DoubleValue\",\"type\":\"System.Double\",\"description\":\"基础浮点；标量\"},{\"name\":\"BoolValue\",\"type\":\"System.Boolean\",\"description\":\"基础布尔；标量\"},{\"name\":\"StringValue\",\"type\":\"System.String\",\"description\":\"基础字符串；标量\"},{\"name\":\"SByteValue\",\"type\":\"System.SByte\",\"description\":\"基础整数；标量\"},{\"name\":\"UShortValue\",\"type\":\"System.UInt16\",\"description\":\"基础整数；标量\"},{\"name\":\"UIntValue\",\"type\":\"System.UInt32\",\"description\":\"基础整数；标量\"},{\"name\":\"ULongValue\",\"type\":\"System.UInt64\",\"description\":\"基础整数；标量；文本存储，保留完整精度\"},{\"name\":\"DecimalValue\",\"type\":\"System.Decimal\",\"description\":\"精确十进制；标量；文本存储，保留完整精度\"},{\"name\":\"QualityValue\",\"type\":\"Game.Config.Quality\",\"description\":\"枚举；标量\"},{\"name\":\"Vector2Value\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"二维向量；标量\"},{\"name\":\"Vector3Value\",\"type\":\"ZrConfig.Types.Vector3\",\"description\":\"三维向量；标量\"},{\"name\":\"KvValue\",\"type\":\"ZrConfig.Types.Kv\",\"description\":\"整数键值对；标量\"},{\"name\":\"MoneyValue\",\"type\":\"Game.Config.Money\",\"description\":\"外部特殊标量转换器；标量\"},{\"name\":\"ByteArray\",\"type\":\"System.Byte[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"ShortArray\",\"type\":\"System.Int16[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"IntArray\",\"type\":\"System.Int32[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"LongArray\",\"type\":\"System.Int64[]\",\"description\":\"基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"FloatArray\",\"type\":\"System.Single[]\",\"description\":\"基础浮点；一维数组；元素用分号分隔\"},{\"name\":\"DoubleArray\",\"type\":\"System.Double[]\",\"description\":\"基础浮点；一维数组；元素用分号分隔\"},{\"name\":\"BoolArray\",\"type\":\"System.Boolean[]\",\"description\":\"基础布尔；一维数组；元素用分号分隔\"},{\"name\":\"StringArray\",\"type\":\"System.String[]\",\"description\":\"基础字符串；一维数组；元素用分号分隔\"},{\"name\":\"SByteArray\",\"type\":\"System.SByte[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"UShortArray\",\"type\":\"System.UInt16[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"UIntArray\",\"type\":\"System.UInt32[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"ULongArray\",\"type\":\"System.UInt64[]\",\"description\":\"基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"DecimalArray\",\"type\":\"System.Decimal[]\",\"description\":\"精确十进制；一维数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"QualityArray\",\"type\":\"Game.Config.Quality[]\",\"description\":\"枚举；一维数组；元素用分号分隔\"},{\"name\":\"Vector2Array\",\"type\":\"ZrConfig.Types.Vector2[]\",\"description\":\"二维向量；一维数组；元素用分号分隔\"},{\"name\":\"Vector3Array\",\"type\":\"ZrConfig.Types.Vector3[]\",\"description\":\"三维向量；一维数组；元素用分号分隔\"},{\"name\":\"KvArray\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"整数键值对；一维数组；元素用分号分隔\"},{\"name\":\"MoneyArray\",\"type\":\"Game.Config.Money[]\",\"description\":\"外部特殊标量转换器；一维数组；元素用分号分隔\"},{\"name\":\"ByteArray2D\",\"type\":\"System.Byte[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"ShortArray2D\",\"type\":\"System.Int16[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"IntArray2D\",\"type\":\"System.Int32[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"LongArray2D\",\"type\":\"System.Int64[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"FloatArray2D\",\"type\":\"System.Single[][]\",\"description\":\"基础浮点；二维交错数组；元素用分号分隔\"},{\"name\":\"DoubleArray2D\",\"type\":\"System.Double[][]\",\"description\":\"基础浮点；二维交错数组；元素用分号分隔\"},{\"name\":\"BoolArray2D\",\"type\":\"System.Boolean[][]\",\"description\":\"基础布尔；二维交错数组；元素用分号分隔\"},{\"name\":\"StringArray2D\",\"type\":\"System.String[][]\",\"description\":\"基础字符串；二维交错数组；元素用分号分隔\"},{\"name\":\"SByteArray2D\",\"type\":\"System.SByte[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"UShortArray2D\",\"type\":\"System.UInt16[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"UIntArray2D\",\"type\":\"System.UInt32[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"ULongArray2D\",\"type\":\"System.UInt64[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"DecimalArray2D\",\"type\":\"System.Decimal[][]\",\"description\":\"精确十进制；二维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"QualityArray2D\",\"type\":\"Game.Config.Quality[][]\",\"description\":\"枚举；二维交错数组；元素用分号分隔\"},{\"name\":\"Vector2Array2D\",\"type\":\"ZrConfig.Types.Vector2[][]\",\"description\":\"二维向量；二维交错数组；元素用分号分隔\"},{\"name\":\"Vector3Array2D\",\"type\":\"ZrConfig.Types.Vector3[][]\",\"description\":\"三维向量；二维交错数组；元素用分号分隔\"},{\"name\":\"KvArray2D\",\"type\":\"ZrConfig.Types.Kv[][]\",\"description\":\"整数键值对；二维交错数组；元素用分号分隔\"},{\"name\":\"MoneyArray2D\",\"type\":\"Game.Config.Money[][]\",\"description\":\"外部特殊标量转换器；二维交错数组；元素用分号分隔\"},{\"name\":\"ByteArray3D\",\"type\":\"System.Byte[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"ShortArray3D\",\"type\":\"System.Int16[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"IntArray3D\",\"type\":\"System.Int32[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"LongArray3D\",\"type\":\"System.Int64[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"FloatArray3D\",\"type\":\"System.Single[][][]\",\"description\":\"基础浮点；三维交错数组；元素用分号分隔\"},{\"name\":\"DoubleArray3D\",\"type\":\"System.Double[][][]\",\"description\":\"基础浮点；三维交错数组；元素用分号分隔\"},{\"name\":\"BoolArray3D\",\"type\":\"System.Boolean[][][]\",\"description\":\"基础布尔；三维交错数组；元素用分号分隔\"},{\"name\":\"StringArray3D\",\"type\":\"System.String[][][]\",\"description\":\"基础字符串；三维交错数组；元素用分号分隔\"},{\"name\":\"SByteArray3D\",\"type\":\"System.SByte[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"UShortArray3D\",\"type\":\"System.UInt16[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"UIntArray3D\",\"type\":\"System.UInt32[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"ULongArray3D\",\"type\":\"System.UInt64[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"DecimalArray3D\",\"type\":\"System.Decimal[][][]\",\"description\":\"精确十进制；三维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"QualityArray3D\",\"type\":\"Game.Config.Quality[][][]\",\"description\":\"枚举；三维交错数组；元素用分号分隔\"},{\"name\":\"Vector2Array3D\",\"type\":\"ZrConfig.Types.Vector2[][][]\",\"description\":\"二维向量；三维交错数组；元素用分号分隔\"},{\"name\":\"Vector3Array3D\",\"type\":\"ZrConfig.Types.Vector3[][][]\",\"description\":\"三维向量；三维交错数组；元素用分号分隔\"},{\"name\":\"KvArray3D\",\"type\":\"ZrConfig.Types.Kv[][][]\",\"description\":\"整数键值对；三维交错数组；元素用分号分隔\"},{\"name\":\"MoneyArray3D\",\"type\":\"Game.Config.Money[][][]\",\"description\":\"外部特殊标量转换器；三维交错数组；元素用分号分隔\"},{\"name\":\"Settings\",\"type\":\"Game.Config.BattleSettings\",\"description\":\"最多目标数\"},{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"结构体最小值\"},{\"name\":\"Options\",\"type\":\"Game.Config.SampleOptions\",\"description\":\"类的可写属性\"},{\"name\":\"Action\",\"type\":\"Game.Config.BattleAction\",\"description\":\"单个多态对象：选择 Damage / AddBuff\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"动作顺序：同一 Id 从 1 开始；一行一个动作\"},{\"name\":\"Ranges\",\"type\":\"Game.Config.ConfigRange[]\",\"description\":\"普通结构体数组也能多行；独立排序\"},{\"name\":\"OptionsList\",\"type\":\"Game.Config.SampleOptions[]\",\"description\":\"普通 class 数组，每行一个对象\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"父动作组编号；同一个编号的续行归入同一组\"},{\"name\":\"Phases\",\"type\":\"Game.Config.BattlePhase[]\",\"description\":\"阶段编号；续行重复填写\"},{\"name\":\"OptionalRange\",\"type\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"description\":\"可空结构体；整组空白表示 null\"},{\"name\":\"OptionalInt\",\"type\":\"System.Nullable`1<System.Int32>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalUInt\",\"type\":\"System.Nullable`1<System.UInt32>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalULong\",\"type\":\"System.Nullable`1<System.UInt64>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalDecimal\",\"type\":\"System.Nullable`1<System.Decimal>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalBool\",\"type\":\"System.Nullable`1<System.Boolean>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalQuality\",\"type\":\"System.Nullable`1<Game.Config.Quality>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalMoney\",\"type\":\"System.Nullable`1<Game.Config.Money>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalValues\",\"type\":\"System.Nullable`1<System.Int32>[]\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"StatMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"PriceMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"QualityMap\",\"type\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"ListMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"IntMatrix\",\"type\":\"System.Int32[,]\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"DecimalCube\",\"type\":\"System.Decimal[,,]\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalMatrix\",\"type\":\"System.Nullable`1<System.UInt32>[,]\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"RawJsonValue\",\"type\":\"ZrConfig.Types.RawJson\",\"description\":\"原始 JSON；标量；这是 JSON 类型本身的样例\"},{\"name\":\"RawJsonArray\",\"type\":\"ZrConfig.Types.RawJson[]\",\"description\":\"原始 JSON；一维数组；这是 JSON 类型本身的样例\"},{\"name\":\"RawJsonArray2D\",\"type\":\"ZrConfig.Types.RawJson[][]\",\"description\":\"原始 JSON；二维交错数组；这是 JSON 类型本身的样例\"},{\"name\":\"RawJsonArray3D\",\"type\":\"ZrConfig.Types.RawJson[][][]\",\"description\":\"原始 JSON；三维交错数组；这是 JSON 类型本身的样例\"},{\"name\":\"JsonIntArray\",\"type\":\"System.Int32[]\",\"description\":\"基础整数；一维数组；json<T> 输入格式示例，用逗号分隔\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]},{\"name\":\"ByQuality\",\"fields\":[\"QualityValue\"]}],\"types\":[{\"id\":\"Game.Config.ActionGroup\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ActionGroup[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ActionGroup\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.AddBuff\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"BuffId\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Stacks\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"enumValues\":[],\"abstract\":true,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattleAction\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattlePhase\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleSettings\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"MaxTargets\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Interruptible\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Min\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Max\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Damage\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Ratio\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"CanCrit\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money\",\"kind\":\"converter\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Cents\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality\",\"kind\":\"enum\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[\"0\",\"1\",\"2\"],\"abstract\":false,\"baseType\":null,\"enumMembers\":{\"None\":\"0\",\"Rare\":\"1\",\"Legendary\":\"2\"},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"\"},{\"name\":\"Rewards\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"\"},{\"name\":\"Offset\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"\"},{\"name\":\"Label\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Quality\",\"type\":\"Game.Config.Quality\",\"description\":\"\"},{\"name\":\"Levels\",\"type\":\"System.Int32[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.SampleOptions\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte\",\"kind\":\"primitive\",\"primitive\":\"uint8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":\"Game.Config.Quality\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Nullable`1<System.Decimal>\",\"key\":\"System.Int32\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32[]\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal\",\"kind\":\"primitive\",\"primitive\":\"decimal\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[,,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":3,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double\",\"kind\":\"primitive\",\"primitive\":\"float64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16\",\"kind\":\"primitive\",\"primitive\":\"int16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64\",\"kind\":\"primitive\",\"primitive\":\"int64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Money>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Quality>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Boolean>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Decimal>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Nullable`1<System.Int32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Nullable`1<System.UInt32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt64>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte\",\"kind\":\"primitive\",\"primitive\":\"int8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single\",\"kind\":\"primitive\",\"primitive\":\"float32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16\",\"kind\":\"primitive\",\"primitive\":\"uint16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32\",\"kind\":\"primitive\",\"primitive\":\"uint32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64\",\"kind\":\"primitive\",\"primitive\":\"uint64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv\",\"kind\":\"kv\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Key\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Value\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson\",\"kind\":\"rawjson\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2\",\"kind\":\"vector2\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3\",\"kind\":\"vector3\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Z\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"dependencies\":[]}", prefer_json)?;
        let rows = source.iter().map(|v| AllTypesConfig::__zr_decode(v).map(Arc::new)).collect::<Result<Vec<_>>>()?;
        let index_Key = R::Index::build(rows.len(), true, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        let index_Group_ById = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        let index_Group_ByQuality = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_QualityValue); R::finish_hash(h) }, |a,b| rows[a].v_QualityValue == rows[b].v_QualityValue)?;
        Ok(Self { rows, index_Key, index_Group_ById, index_Group_ByQuality })
    }
    pub(super) fn __zr_bind(&self, _snapshot: &ConfigSnapshot) -> Result<()> {
        Ok(())
    }
    pub fn find(&self, k0: i32) -> Option<&AllTypesConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Key.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        slot.map(|s| self.rows[self.index_Key.row(s)].as_ref())
    }
    pub fn get(&self, k0: i32) -> Result<&AllTypesConfig> { self.find(k0).ok_or_else(|| "配置主键不存在".to_owned()) }
    pub fn get_ById(&self, k0: i32) -> R::Group<'_, AllTypesConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_ById.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_ById.group(slot) }
    }
    pub fn get_ByQuality(&self, k0: T::T_Game_Config_Quality) -> R::Group<'_, AllTypesConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_ByQuality.find(R::finish_hash(h), |i| self.rows[i].v_QualityValue == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_ByQuality.group(slot) }
    }
}
