// zrconfig-api-contract:1
// Generated server data: shared slots, owning handles, explicit references.
use std::path::Path;
use std::sync::{Arc,OnceLock,atomic::{AtomicBool,Ordering}};
use super::runtime::{self as R,Json,Result,ServerResult,ConfigError,ErrorCode,Cancellation,LoadPlan,LoadMode};
use super::types as T;
pub struct AllTypesData{
    v_Id:i32,
    v_ByteValue:u8,
    v_ShortValue:i16,
    v_IntValue:i32,
    v_LongValue:i64,
    v_FloatValue:f32,
    v_DoubleValue:f64,
    v_BoolValue:bool,
    v_StringValue:String,
    v_SByteValue:i8,
    v_UShortValue:u16,
    v_UIntValue:u32,
    v_ULongValue:u64,
    v_DecimalValue:R::Decimal,
    v_QualityValue:T::T_Game_Config_Quality,
    v_Vector2Value:T::T_ZrConfig_Types_Vector2,
    v_Vector3Value:T::T_ZrConfig_Types_Vector3,
    v_KvValue:T::T_ZrConfig_Types_Kv,
    v_MoneyValue:T::T_Game_Config_Money,
    v_ByteArray:Vec<u8>,
    v_ShortArray:Vec<i16>,
    v_IntArray:Vec<i32>,
    v_LongArray:Vec<i64>,
    v_FloatArray:Vec<f32>,
    v_DoubleArray:Vec<f64>,
    v_BoolArray:Vec<bool>,
    v_StringArray:Vec<String>,
    v_SByteArray:Vec<i8>,
    v_UShortArray:Vec<u16>,
    v_UIntArray:Vec<u32>,
    v_ULongArray:Vec<u64>,
    v_DecimalArray:Vec<R::Decimal>,
    v_QualityArray:Vec<T::T_Game_Config_Quality>,
    v_Vector2Array:Vec<T::T_ZrConfig_Types_Vector2>,
    v_Vector3Array:Vec<T::T_ZrConfig_Types_Vector3>,
    v_KvArray:Vec<T::T_ZrConfig_Types_Kv>,
    v_MoneyArray:Vec<T::T_Game_Config_Money>,
    v_ByteArray2D:Vec<Vec<u8>>,
    v_ShortArray2D:Vec<Vec<i16>>,
    v_IntArray2D:Vec<Vec<i32>>,
    v_LongArray2D:Vec<Vec<i64>>,
    v_FloatArray2D:Vec<Vec<f32>>,
    v_DoubleArray2D:Vec<Vec<f64>>,
    v_BoolArray2D:Vec<Vec<bool>>,
    v_StringArray2D:Vec<Vec<String>>,
    v_SByteArray2D:Vec<Vec<i8>>,
    v_UShortArray2D:Vec<Vec<u16>>,
    v_UIntArray2D:Vec<Vec<u32>>,
    v_ULongArray2D:Vec<Vec<u64>>,
    v_DecimalArray2D:Vec<Vec<R::Decimal>>,
    v_QualityArray2D:Vec<Vec<T::T_Game_Config_Quality>>,
    v_Vector2Array2D:Vec<Vec<T::T_ZrConfig_Types_Vector2>>,
    v_Vector3Array2D:Vec<Vec<T::T_ZrConfig_Types_Vector3>>,
    v_KvArray2D:Vec<Vec<T::T_ZrConfig_Types_Kv>>,
    v_MoneyArray2D:Vec<Vec<T::T_Game_Config_Money>>,
    v_ByteArray3D:Vec<Vec<Vec<u8>>>,
    v_ShortArray3D:Vec<Vec<Vec<i16>>>,
    v_IntArray3D:Vec<Vec<Vec<i32>>>,
    v_LongArray3D:Vec<Vec<Vec<i64>>>,
    v_FloatArray3D:Vec<Vec<Vec<f32>>>,
    v_DoubleArray3D:Vec<Vec<Vec<f64>>>,
    v_BoolArray3D:Vec<Vec<Vec<bool>>>,
    v_StringArray3D:Vec<Vec<Vec<String>>>,
    v_SByteArray3D:Vec<Vec<Vec<i8>>>,
    v_UShortArray3D:Vec<Vec<Vec<u16>>>,
    v_UIntArray3D:Vec<Vec<Vec<u32>>>,
    v_ULongArray3D:Vec<Vec<Vec<u64>>>,
    v_DecimalArray3D:Vec<Vec<Vec<R::Decimal>>>,
    v_QualityArray3D:Vec<Vec<Vec<T::T_Game_Config_Quality>>>,
    v_Vector2Array3D:Vec<Vec<Vec<T::T_ZrConfig_Types_Vector2>>>,
    v_Vector3Array3D:Vec<Vec<Vec<T::T_ZrConfig_Types_Vector3>>>,
    v_KvArray3D:Vec<Vec<Vec<T::T_ZrConfig_Types_Kv>>>,
    v_MoneyArray3D:Vec<Vec<Vec<T::T_Game_Config_Money>>>,
    v_Settings:Option<Box<T::T_Game_Config_BattleSettings>>,
    v_Range:T::T_Game_Config_ConfigRange,
    v_Options:Option<Box<T::T_Game_Config_SampleOptions>>,
    v_Action:Option<Box<T::T_Game_Config_BattleAction>>,
    v_Actions:Vec<Option<Box<T::T_Game_Config_BattleAction>>>,
    v_Ranges:Vec<T::T_Game_Config_ConfigRange>,
    v_OptionsList:Vec<Option<Box<T::T_Game_Config_SampleOptions>>>,
    v_Groups:Vec<Option<Box<T::T_Game_Config_ActionGroup>>>,
    v_Phases:Vec<Option<Box<T::T_Game_Config_BattlePhase>>>,
    v_OptionalRange:Option<T::T_Game_Config_ConfigRange>,
    v_OptionalInt:Option<i32>,
    v_OptionalUInt:Option<u32>,
    v_OptionalULong:Option<u64>,
    v_OptionalDecimal:Option<R::Decimal>,
    v_OptionalBool:Option<bool>,
    v_OptionalQuality:Option<T::T_Game_Config_Quality>,
    v_OptionalMoney:Option<T::T_Game_Config_Money>,
    v_OptionalValues:Vec<Option<i32>>,
    v_StatMap:R::Dictionary<String, i32>,
    v_PriceMap:R::Dictionary<i32, Option<R::Decimal>>,
    v_QualityMap:R::Dictionary<T::T_Game_Config_Quality, u32>,
    v_ListMap:R::Dictionary<String, Vec<i32>>,
    v_IntMatrix:R::Matrix<i32>,
    v_DecimalCube:R::Matrix<R::Decimal>,
    v_OptionalMatrix:R::Matrix<Option<u32>>,
    v_RawJsonValue:String,
    v_RawJsonArray:Vec<String>,
    v_RawJsonArray2D:Vec<Vec<String>>,
    v_RawJsonArray3D:Vec<Vec<Vec<String>>>,
    v_JsonIntArray:Vec<i32>,
}
impl AllTypesData{
    fn decode(value:&Json)->Result<Self>{Ok(Self{
        v_Id:R::scalar::<i32>(value.field("Id")?)?,
        v_ByteValue:R::scalar::<u8>(value.field("ByteValue")?)?,
        v_ShortValue:R::scalar::<i16>(value.field("ShortValue")?)?,
        v_IntValue:R::scalar::<i32>(value.field("IntValue")?)?,
        v_LongValue:R::scalar::<i64>(value.field("LongValue")?)?,
        v_FloatValue:R::scalar::<f32>(value.field("FloatValue")?)?,
        v_DoubleValue:R::scalar::<f64>(value.field("DoubleValue")?)?,
        v_BoolValue:R::scalar::<bool>(value.field("BoolValue")?)?,
        v_StringValue:R::scalar::<String>(value.field("StringValue")?)?,
        v_SByteValue:R::scalar::<i8>(value.field("SByteValue")?)?,
        v_UShortValue:R::scalar::<u16>(value.field("UShortValue")?)?,
        v_UIntValue:R::scalar::<u32>(value.field("UIntValue")?)?,
        v_ULongValue:R::scalar::<u64>(value.field("ULongValue")?)?,
        v_DecimalValue:R::scalar::<R::Decimal>(value.field("DecimalValue")?)?,
        v_QualityValue:T::T_Game_Config_Quality::__zr_decode(value.field("QualityValue")?)?,
        v_Vector2Value:T::T_ZrConfig_Types_Vector2::__zr_decode(value.field("Vector2Value")?)?,
        v_Vector3Value:T::T_ZrConfig_Types_Vector3::__zr_decode(value.field("Vector3Value")?)?,
        v_KvValue:T::T_ZrConfig_Types_Kv::__zr_decode(value.field("KvValue")?)?,
        v_MoneyValue:T::T_Game_Config_Money::__zr_decode(value.field("MoneyValue")?)?,
        v_ByteArray:R::decode_list(value.field("ByteArray")?, |item0| R::scalar::<u8>(item0))?,
        v_ShortArray:R::decode_list(value.field("ShortArray")?, |item0| R::scalar::<i16>(item0))?,
        v_IntArray:R::decode_list(value.field("IntArray")?, |item0| R::scalar::<i32>(item0))?,
        v_LongArray:R::decode_list(value.field("LongArray")?, |item0| R::scalar::<i64>(item0))?,
        v_FloatArray:R::decode_list(value.field("FloatArray")?, |item0| R::scalar::<f32>(item0))?,
        v_DoubleArray:R::decode_list(value.field("DoubleArray")?, |item0| R::scalar::<f64>(item0))?,
        v_BoolArray:R::decode_list(value.field("BoolArray")?, |item0| R::scalar::<bool>(item0))?,
        v_StringArray:R::decode_list(value.field("StringArray")?, |item0| R::scalar::<String>(item0))?,
        v_SByteArray:R::decode_list(value.field("SByteArray")?, |item0| R::scalar::<i8>(item0))?,
        v_UShortArray:R::decode_list(value.field("UShortArray")?, |item0| R::scalar::<u16>(item0))?,
        v_UIntArray:R::decode_list(value.field("UIntArray")?, |item0| R::scalar::<u32>(item0))?,
        v_ULongArray:R::decode_list(value.field("ULongArray")?, |item0| R::scalar::<u64>(item0))?,
        v_DecimalArray:R::decode_list(value.field("DecimalArray")?, |item0| R::scalar::<R::Decimal>(item0))?,
        v_QualityArray:R::decode_list(value.field("QualityArray")?, |item0| T::T_Game_Config_Quality::__zr_decode(item0))?,
        v_Vector2Array:R::decode_list(value.field("Vector2Array")?, |item0| T::T_ZrConfig_Types_Vector2::__zr_decode(item0))?,
        v_Vector3Array:R::decode_list(value.field("Vector3Array")?, |item0| T::T_ZrConfig_Types_Vector3::__zr_decode(item0))?,
        v_KvArray:R::decode_list(value.field("KvArray")?, |item0| T::T_ZrConfig_Types_Kv::__zr_decode(item0))?,
        v_MoneyArray:R::decode_list(value.field("MoneyArray")?, |item0| T::T_Game_Config_Money::__zr_decode(item0))?,
        v_ByteArray2D:R::decode_list(value.field("ByteArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<u8>(item1)))?,
        v_ShortArray2D:R::decode_list(value.field("ShortArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<i16>(item1)))?,
        v_IntArray2D:R::decode_list(value.field("IntArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<i32>(item1)))?,
        v_LongArray2D:R::decode_list(value.field("LongArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<i64>(item1)))?,
        v_FloatArray2D:R::decode_list(value.field("FloatArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<f32>(item1)))?,
        v_DoubleArray2D:R::decode_list(value.field("DoubleArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<f64>(item1)))?,
        v_BoolArray2D:R::decode_list(value.field("BoolArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<bool>(item1)))?,
        v_StringArray2D:R::decode_list(value.field("StringArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<String>(item1)))?,
        v_SByteArray2D:R::decode_list(value.field("SByteArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<i8>(item1)))?,
        v_UShortArray2D:R::decode_list(value.field("UShortArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<u16>(item1)))?,
        v_UIntArray2D:R::decode_list(value.field("UIntArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<u32>(item1)))?,
        v_ULongArray2D:R::decode_list(value.field("ULongArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<u64>(item1)))?,
        v_DecimalArray2D:R::decode_list(value.field("DecimalArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<R::Decimal>(item1)))?,
        v_QualityArray2D:R::decode_list(value.field("QualityArray2D")?, |item0| R::decode_list(item0, |item1| T::T_Game_Config_Quality::__zr_decode(item1)))?,
        v_Vector2Array2D:R::decode_list(value.field("Vector2Array2D")?, |item0| R::decode_list(item0, |item1| T::T_ZrConfig_Types_Vector2::__zr_decode(item1)))?,
        v_Vector3Array2D:R::decode_list(value.field("Vector3Array2D")?, |item0| R::decode_list(item0, |item1| T::T_ZrConfig_Types_Vector3::__zr_decode(item1)))?,
        v_KvArray2D:R::decode_list(value.field("KvArray2D")?, |item0| R::decode_list(item0, |item1| T::T_ZrConfig_Types_Kv::__zr_decode(item1)))?,
        v_MoneyArray2D:R::decode_list(value.field("MoneyArray2D")?, |item0| R::decode_list(item0, |item1| T::T_Game_Config_Money::__zr_decode(item1)))?,
        v_ByteArray3D:R::decode_list(value.field("ByteArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<u8>(item2))))?,
        v_ShortArray3D:R::decode_list(value.field("ShortArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<i16>(item2))))?,
        v_IntArray3D:R::decode_list(value.field("IntArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<i32>(item2))))?,
        v_LongArray3D:R::decode_list(value.field("LongArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<i64>(item2))))?,
        v_FloatArray3D:R::decode_list(value.field("FloatArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<f32>(item2))))?,
        v_DoubleArray3D:R::decode_list(value.field("DoubleArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<f64>(item2))))?,
        v_BoolArray3D:R::decode_list(value.field("BoolArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<bool>(item2))))?,
        v_StringArray3D:R::decode_list(value.field("StringArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<String>(item2))))?,
        v_SByteArray3D:R::decode_list(value.field("SByteArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<i8>(item2))))?,
        v_UShortArray3D:R::decode_list(value.field("UShortArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<u16>(item2))))?,
        v_UIntArray3D:R::decode_list(value.field("UIntArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<u32>(item2))))?,
        v_ULongArray3D:R::decode_list(value.field("ULongArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<u64>(item2))))?,
        v_DecimalArray3D:R::decode_list(value.field("DecimalArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<R::Decimal>(item2))))?,
        v_QualityArray3D:R::decode_list(value.field("QualityArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_Game_Config_Quality::__zr_decode(item2))))?,
        v_Vector2Array3D:R::decode_list(value.field("Vector2Array3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_ZrConfig_Types_Vector2::__zr_decode(item2))))?,
        v_Vector3Array3D:R::decode_list(value.field("Vector3Array3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_ZrConfig_Types_Vector3::__zr_decode(item2))))?,
        v_KvArray3D:R::decode_list(value.field("KvArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_ZrConfig_Types_Kv::__zr_decode(item2))))?,
        v_MoneyArray3D:R::decode_list(value.field("MoneyArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_Game_Config_Money::__zr_decode(item2))))?,
        v_Settings:R::decode_optional(value.field("Settings")?, |v| T::T_Game_Config_BattleSettings::__zr_decode(v).map(Box::new))?,
        v_Range:T::T_Game_Config_ConfigRange::__zr_decode(value.field("Range")?)?,
        v_Options:R::decode_optional(value.field("Options")?, |v| T::T_Game_Config_SampleOptions::__zr_decode(v).map(Box::new))?,
        v_Action:R::decode_optional(value.field("Action")?, |v| T::T_Game_Config_BattleAction::__zr_decode(v).map(Box::new))?,
        v_Actions:R::decode_list(value.field("Actions")?, |item0| R::decode_optional(item0, |v| T::T_Game_Config_BattleAction::__zr_decode(v).map(Box::new)))?,
        v_Ranges:R::decode_list(value.field("Ranges")?, |item0| T::T_Game_Config_ConfigRange::__zr_decode(item0))?,
        v_OptionsList:R::decode_list(value.field("OptionsList")?, |item0| R::decode_optional(item0, |v| T::T_Game_Config_SampleOptions::__zr_decode(v).map(Box::new)))?,
        v_Groups:R::decode_list(value.field("Groups")?, |item0| R::decode_optional(item0, |v| T::T_Game_Config_ActionGroup::__zr_decode(v).map(Box::new)))?,
        v_Phases:R::decode_list(value.field("Phases")?, |item0| R::decode_optional(item0, |v| T::T_Game_Config_BattlePhase::__zr_decode(v).map(Box::new)))?,
        v_OptionalRange:R::decode_optional(value.field("OptionalRange")?, |item0| T::T_Game_Config_ConfigRange::__zr_decode(item0))?,
        v_OptionalInt:R::decode_optional(value.field("OptionalInt")?, |item0| R::scalar::<i32>(item0))?,
        v_OptionalUInt:R::decode_optional(value.field("OptionalUInt")?, |item0| R::scalar::<u32>(item0))?,
        v_OptionalULong:R::decode_optional(value.field("OptionalULong")?, |item0| R::scalar::<u64>(item0))?,
        v_OptionalDecimal:R::decode_optional(value.field("OptionalDecimal")?, |item0| R::scalar::<R::Decimal>(item0))?,
        v_OptionalBool:R::decode_optional(value.field("OptionalBool")?, |item0| R::scalar::<bool>(item0))?,
        v_OptionalQuality:R::decode_optional(value.field("OptionalQuality")?, |item0| T::T_Game_Config_Quality::__zr_decode(item0))?,
        v_OptionalMoney:R::decode_optional(value.field("OptionalMoney")?, |item0| T::T_Game_Config_Money::__zr_decode(item0))?,
        v_OptionalValues:R::decode_list(value.field("OptionalValues")?, |item0| R::decode_optional(item0, |item1| R::scalar::<i32>(item1)))?,
        v_StatMap:R::decode_dictionary(value.field("StatMap")?, |item0| R::scalar::<String>(item0), |item0| R::scalar::<i32>(item0))?,
        v_PriceMap:R::decode_dictionary(value.field("PriceMap")?, |item0| R::scalar::<i32>(item0), |item0| R::decode_optional(item0, |item1| R::scalar::<R::Decimal>(item1)))?,
        v_QualityMap:R::decode_dictionary(value.field("QualityMap")?, |item0| T::T_Game_Config_Quality::__zr_decode(item0), |item0| R::scalar::<u32>(item0))?,
        v_ListMap:R::decode_dictionary(value.field("ListMap")?, |item0| R::scalar::<String>(item0), |item0| R::decode_list(item0, |item1| R::scalar::<i32>(item1)))?,
        v_IntMatrix:R::decode_matrix(value.field("IntMatrix")?, |item0| R::scalar::<i32>(item0))?,
        v_DecimalCube:R::decode_matrix(value.field("DecimalCube")?, |item0| R::scalar::<R::Decimal>(item0))?,
        v_OptionalMatrix:R::decode_matrix(value.field("OptionalMatrix")?, |item0| R::decode_optional(item0, |item1| R::scalar::<u32>(item1)))?,
        v_RawJsonValue:R::scalar::<String>(value.field("RawJsonValue")?)?,
        v_RawJsonArray:R::decode_list(value.field("RawJsonArray")?, |item0| R::scalar::<String>(item0))?,
        v_RawJsonArray2D:R::decode_list(value.field("RawJsonArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<String>(item1)))?,
        v_RawJsonArray3D:R::decode_list(value.field("RawJsonArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<String>(item2))))?,
        v_JsonIntArray:R::decode_list(value.field("JsonIntArray")?, |item0| R::scalar::<i32>(item0))?,
    })}
    pub fn get_Id(&self)->i32{self.v_Id}
    pub fn get_ByteValue(&self)->u8{self.v_ByteValue}
    pub fn get_ShortValue(&self)->i16{self.v_ShortValue}
    pub fn get_IntValue(&self)->i32{self.v_IntValue}
    pub fn get_LongValue(&self)->i64{self.v_LongValue}
    pub fn get_FloatValue(&self)->f32{self.v_FloatValue}
    pub fn get_DoubleValue(&self)->f64{self.v_DoubleValue}
    pub fn get_BoolValue(&self)->bool{self.v_BoolValue}
    pub fn get_StringValue(&self)->&str{&self.v_StringValue}
    pub fn get_SByteValue(&self)->i8{self.v_SByteValue}
    pub fn get_UShortValue(&self)->u16{self.v_UShortValue}
    pub fn get_UIntValue(&self)->u32{self.v_UIntValue}
    pub fn get_ULongValue(&self)->u64{self.v_ULongValue}
    pub fn get_DecimalValue(&self)->R::Decimal{self.v_DecimalValue}
    pub fn get_QualityValue(&self)->T::T_Game_Config_Quality{self.v_QualityValue}
    pub fn get_Vector2Value(&self)->&T::T_ZrConfig_Types_Vector2{&self.v_Vector2Value}
    pub fn get_Vector3Value(&self)->&T::T_ZrConfig_Types_Vector3{&self.v_Vector3Value}
    pub fn get_KvValue(&self)->&T::T_ZrConfig_Types_Kv{&self.v_KvValue}
    pub fn get_MoneyValue(&self)->&T::T_Game_Config_Money{&self.v_MoneyValue}
    pub fn get_ByteArray(&self)->&[u8]{&self.v_ByteArray}
    pub fn get_ShortArray(&self)->&[i16]{&self.v_ShortArray}
    pub fn get_IntArray(&self)->&[i32]{&self.v_IntArray}
    pub fn get_LongArray(&self)->&[i64]{&self.v_LongArray}
    pub fn get_FloatArray(&self)->&[f32]{&self.v_FloatArray}
    pub fn get_DoubleArray(&self)->&[f64]{&self.v_DoubleArray}
    pub fn get_BoolArray(&self)->&[bool]{&self.v_BoolArray}
    pub fn get_StringArray(&self)->&[String]{&self.v_StringArray}
    pub fn get_SByteArray(&self)->&[i8]{&self.v_SByteArray}
    pub fn get_UShortArray(&self)->&[u16]{&self.v_UShortArray}
    pub fn get_UIntArray(&self)->&[u32]{&self.v_UIntArray}
    pub fn get_ULongArray(&self)->&[u64]{&self.v_ULongArray}
    pub fn get_DecimalArray(&self)->&[R::Decimal]{&self.v_DecimalArray}
    pub fn get_QualityArray(&self)->&[T::T_Game_Config_Quality]{&self.v_QualityArray}
    pub fn get_Vector2Array(&self)->&[T::T_ZrConfig_Types_Vector2]{&self.v_Vector2Array}
    pub fn get_Vector3Array(&self)->&[T::T_ZrConfig_Types_Vector3]{&self.v_Vector3Array}
    pub fn get_KvArray(&self)->&[T::T_ZrConfig_Types_Kv]{&self.v_KvArray}
    pub fn get_MoneyArray(&self)->&[T::T_Game_Config_Money]{&self.v_MoneyArray}
    pub fn get_ByteArray2D(&self)->&[Vec<u8>]{&self.v_ByteArray2D}
    pub fn get_ShortArray2D(&self)->&[Vec<i16>]{&self.v_ShortArray2D}
    pub fn get_IntArray2D(&self)->&[Vec<i32>]{&self.v_IntArray2D}
    pub fn get_LongArray2D(&self)->&[Vec<i64>]{&self.v_LongArray2D}
    pub fn get_FloatArray2D(&self)->&[Vec<f32>]{&self.v_FloatArray2D}
    pub fn get_DoubleArray2D(&self)->&[Vec<f64>]{&self.v_DoubleArray2D}
    pub fn get_BoolArray2D(&self)->&[Vec<bool>]{&self.v_BoolArray2D}
    pub fn get_StringArray2D(&self)->&[Vec<String>]{&self.v_StringArray2D}
    pub fn get_SByteArray2D(&self)->&[Vec<i8>]{&self.v_SByteArray2D}
    pub fn get_UShortArray2D(&self)->&[Vec<u16>]{&self.v_UShortArray2D}
    pub fn get_UIntArray2D(&self)->&[Vec<u32>]{&self.v_UIntArray2D}
    pub fn get_ULongArray2D(&self)->&[Vec<u64>]{&self.v_ULongArray2D}
    pub fn get_DecimalArray2D(&self)->&[Vec<R::Decimal>]{&self.v_DecimalArray2D}
    pub fn get_QualityArray2D(&self)->&[Vec<T::T_Game_Config_Quality>]{&self.v_QualityArray2D}
    pub fn get_Vector2Array2D(&self)->&[Vec<T::T_ZrConfig_Types_Vector2>]{&self.v_Vector2Array2D}
    pub fn get_Vector3Array2D(&self)->&[Vec<T::T_ZrConfig_Types_Vector3>]{&self.v_Vector3Array2D}
    pub fn get_KvArray2D(&self)->&[Vec<T::T_ZrConfig_Types_Kv>]{&self.v_KvArray2D}
    pub fn get_MoneyArray2D(&self)->&[Vec<T::T_Game_Config_Money>]{&self.v_MoneyArray2D}
    pub fn get_ByteArray3D(&self)->&[Vec<Vec<u8>>]{&self.v_ByteArray3D}
    pub fn get_ShortArray3D(&self)->&[Vec<Vec<i16>>]{&self.v_ShortArray3D}
    pub fn get_IntArray3D(&self)->&[Vec<Vec<i32>>]{&self.v_IntArray3D}
    pub fn get_LongArray3D(&self)->&[Vec<Vec<i64>>]{&self.v_LongArray3D}
    pub fn get_FloatArray3D(&self)->&[Vec<Vec<f32>>]{&self.v_FloatArray3D}
    pub fn get_DoubleArray3D(&self)->&[Vec<Vec<f64>>]{&self.v_DoubleArray3D}
    pub fn get_BoolArray3D(&self)->&[Vec<Vec<bool>>]{&self.v_BoolArray3D}
    pub fn get_StringArray3D(&self)->&[Vec<Vec<String>>]{&self.v_StringArray3D}
    pub fn get_SByteArray3D(&self)->&[Vec<Vec<i8>>]{&self.v_SByteArray3D}
    pub fn get_UShortArray3D(&self)->&[Vec<Vec<u16>>]{&self.v_UShortArray3D}
    pub fn get_UIntArray3D(&self)->&[Vec<Vec<u32>>]{&self.v_UIntArray3D}
    pub fn get_ULongArray3D(&self)->&[Vec<Vec<u64>>]{&self.v_ULongArray3D}
    pub fn get_DecimalArray3D(&self)->&[Vec<Vec<R::Decimal>>]{&self.v_DecimalArray3D}
    pub fn get_QualityArray3D(&self)->&[Vec<Vec<T::T_Game_Config_Quality>>]{&self.v_QualityArray3D}
    pub fn get_Vector2Array3D(&self)->&[Vec<Vec<T::T_ZrConfig_Types_Vector2>>]{&self.v_Vector2Array3D}
    pub fn get_Vector3Array3D(&self)->&[Vec<Vec<T::T_ZrConfig_Types_Vector3>>]{&self.v_Vector3Array3D}
    pub fn get_KvArray3D(&self)->&[Vec<Vec<T::T_ZrConfig_Types_Kv>>]{&self.v_KvArray3D}
    pub fn get_MoneyArray3D(&self)->&[Vec<Vec<T::T_Game_Config_Money>>]{&self.v_MoneyArray3D}
    pub fn get_Settings(&self)->&Option<Box<T::T_Game_Config_BattleSettings>>{&self.v_Settings}
    pub fn get_Range(&self)->&T::T_Game_Config_ConfigRange{&self.v_Range}
    pub fn get_Options(&self)->&Option<Box<T::T_Game_Config_SampleOptions>>{&self.v_Options}
    pub fn get_Action(&self)->&Option<Box<T::T_Game_Config_BattleAction>>{&self.v_Action}
    pub fn get_Actions(&self)->&[Option<Box<T::T_Game_Config_BattleAction>>]{&self.v_Actions}
    pub fn get_Ranges(&self)->&[T::T_Game_Config_ConfigRange]{&self.v_Ranges}
    pub fn get_OptionsList(&self)->&[Option<Box<T::T_Game_Config_SampleOptions>>]{&self.v_OptionsList}
    pub fn get_Groups(&self)->&[Option<Box<T::T_Game_Config_ActionGroup>>]{&self.v_Groups}
    pub fn get_Phases(&self)->&[Option<Box<T::T_Game_Config_BattlePhase>>]{&self.v_Phases}
    pub fn get_OptionalRange(&self)->&Option<T::T_Game_Config_ConfigRange>{&self.v_OptionalRange}
    pub fn get_OptionalInt(&self)->&Option<i32>{&self.v_OptionalInt}
    pub fn get_OptionalUInt(&self)->&Option<u32>{&self.v_OptionalUInt}
    pub fn get_OptionalULong(&self)->&Option<u64>{&self.v_OptionalULong}
    pub fn get_OptionalDecimal(&self)->&Option<R::Decimal>{&self.v_OptionalDecimal}
    pub fn get_OptionalBool(&self)->&Option<bool>{&self.v_OptionalBool}
    pub fn get_OptionalQuality(&self)->&Option<T::T_Game_Config_Quality>{&self.v_OptionalQuality}
    pub fn get_OptionalMoney(&self)->&Option<T::T_Game_Config_Money>{&self.v_OptionalMoney}
    pub fn get_OptionalValues(&self)->&[Option<i32>]{&self.v_OptionalValues}
    pub fn get_StatMap(&self)->&R::Dictionary<String, i32>{&self.v_StatMap}
    pub fn get_PriceMap(&self)->&R::Dictionary<i32, Option<R::Decimal>>{&self.v_PriceMap}
    pub fn get_QualityMap(&self)->&R::Dictionary<T::T_Game_Config_Quality, u32>{&self.v_QualityMap}
    pub fn get_ListMap(&self)->&R::Dictionary<String, Vec<i32>>{&self.v_ListMap}
    pub fn get_IntMatrix(&self)->&R::Matrix<i32>{&self.v_IntMatrix}
    pub fn get_DecimalCube(&self)->&R::Matrix<R::Decimal>{&self.v_DecimalCube}
    pub fn get_OptionalMatrix(&self)->&R::Matrix<Option<u32>>{&self.v_OptionalMatrix}
    pub fn get_RawJsonValue(&self)->&str{&self.v_RawJsonValue}
    pub fn get_RawJsonArray(&self)->&[String]{&self.v_RawJsonArray}
    pub fn get_RawJsonArray2D(&self)->&[Vec<String>]{&self.v_RawJsonArray2D}
    pub fn get_RawJsonArray3D(&self)->&[Vec<Vec<String>>]{&self.v_RawJsonArray3D}
    pub fn get_JsonIntArray(&self)->&[i32]{&self.v_JsonIntArray}
}
pub type AllTypesRow=R::RowHandle<Snapshot,AllTypesData>;
impl R::RowHandle<Snapshot,AllTypesData>{
    pub fn get_Id(&self)->i32{self.row.v_Id}
    pub fn get_ByteValue(&self)->u8{self.row.v_ByteValue}
    pub fn get_ShortValue(&self)->i16{self.row.v_ShortValue}
    pub fn get_IntValue(&self)->i32{self.row.v_IntValue}
    pub fn get_LongValue(&self)->i64{self.row.v_LongValue}
    pub fn get_FloatValue(&self)->f32{self.row.v_FloatValue}
    pub fn get_DoubleValue(&self)->f64{self.row.v_DoubleValue}
    pub fn get_BoolValue(&self)->bool{self.row.v_BoolValue}
    pub fn get_StringValue(&self)->&str{&self.row.v_StringValue}
    pub fn get_SByteValue(&self)->i8{self.row.v_SByteValue}
    pub fn get_UShortValue(&self)->u16{self.row.v_UShortValue}
    pub fn get_UIntValue(&self)->u32{self.row.v_UIntValue}
    pub fn get_ULongValue(&self)->u64{self.row.v_ULongValue}
    pub fn get_DecimalValue(&self)->R::Decimal{self.row.v_DecimalValue}
    pub fn get_QualityValue(&self)->T::T_Game_Config_Quality{self.row.v_QualityValue}
    pub fn get_Vector2Value(&self)->&T::T_ZrConfig_Types_Vector2{&self.row.v_Vector2Value}
    pub fn get_Vector3Value(&self)->&T::T_ZrConfig_Types_Vector3{&self.row.v_Vector3Value}
    pub fn get_KvValue(&self)->&T::T_ZrConfig_Types_Kv{&self.row.v_KvValue}
    pub fn get_MoneyValue(&self)->&T::T_Game_Config_Money{&self.row.v_MoneyValue}
    pub fn get_ByteArray(&self)->&[u8]{&self.row.v_ByteArray}
    pub fn get_ShortArray(&self)->&[i16]{&self.row.v_ShortArray}
    pub fn get_IntArray(&self)->&[i32]{&self.row.v_IntArray}
    pub fn get_LongArray(&self)->&[i64]{&self.row.v_LongArray}
    pub fn get_FloatArray(&self)->&[f32]{&self.row.v_FloatArray}
    pub fn get_DoubleArray(&self)->&[f64]{&self.row.v_DoubleArray}
    pub fn get_BoolArray(&self)->&[bool]{&self.row.v_BoolArray}
    pub fn get_StringArray(&self)->&[String]{&self.row.v_StringArray}
    pub fn get_SByteArray(&self)->&[i8]{&self.row.v_SByteArray}
    pub fn get_UShortArray(&self)->&[u16]{&self.row.v_UShortArray}
    pub fn get_UIntArray(&self)->&[u32]{&self.row.v_UIntArray}
    pub fn get_ULongArray(&self)->&[u64]{&self.row.v_ULongArray}
    pub fn get_DecimalArray(&self)->&[R::Decimal]{&self.row.v_DecimalArray}
    pub fn get_QualityArray(&self)->&[T::T_Game_Config_Quality]{&self.row.v_QualityArray}
    pub fn get_Vector2Array(&self)->&[T::T_ZrConfig_Types_Vector2]{&self.row.v_Vector2Array}
    pub fn get_Vector3Array(&self)->&[T::T_ZrConfig_Types_Vector3]{&self.row.v_Vector3Array}
    pub fn get_KvArray(&self)->&[T::T_ZrConfig_Types_Kv]{&self.row.v_KvArray}
    pub fn get_MoneyArray(&self)->&[T::T_Game_Config_Money]{&self.row.v_MoneyArray}
    pub fn get_ByteArray2D(&self)->&[Vec<u8>]{&self.row.v_ByteArray2D}
    pub fn get_ShortArray2D(&self)->&[Vec<i16>]{&self.row.v_ShortArray2D}
    pub fn get_IntArray2D(&self)->&[Vec<i32>]{&self.row.v_IntArray2D}
    pub fn get_LongArray2D(&self)->&[Vec<i64>]{&self.row.v_LongArray2D}
    pub fn get_FloatArray2D(&self)->&[Vec<f32>]{&self.row.v_FloatArray2D}
    pub fn get_DoubleArray2D(&self)->&[Vec<f64>]{&self.row.v_DoubleArray2D}
    pub fn get_BoolArray2D(&self)->&[Vec<bool>]{&self.row.v_BoolArray2D}
    pub fn get_StringArray2D(&self)->&[Vec<String>]{&self.row.v_StringArray2D}
    pub fn get_SByteArray2D(&self)->&[Vec<i8>]{&self.row.v_SByteArray2D}
    pub fn get_UShortArray2D(&self)->&[Vec<u16>]{&self.row.v_UShortArray2D}
    pub fn get_UIntArray2D(&self)->&[Vec<u32>]{&self.row.v_UIntArray2D}
    pub fn get_ULongArray2D(&self)->&[Vec<u64>]{&self.row.v_ULongArray2D}
    pub fn get_DecimalArray2D(&self)->&[Vec<R::Decimal>]{&self.row.v_DecimalArray2D}
    pub fn get_QualityArray2D(&self)->&[Vec<T::T_Game_Config_Quality>]{&self.row.v_QualityArray2D}
    pub fn get_Vector2Array2D(&self)->&[Vec<T::T_ZrConfig_Types_Vector2>]{&self.row.v_Vector2Array2D}
    pub fn get_Vector3Array2D(&self)->&[Vec<T::T_ZrConfig_Types_Vector3>]{&self.row.v_Vector3Array2D}
    pub fn get_KvArray2D(&self)->&[Vec<T::T_ZrConfig_Types_Kv>]{&self.row.v_KvArray2D}
    pub fn get_MoneyArray2D(&self)->&[Vec<T::T_Game_Config_Money>]{&self.row.v_MoneyArray2D}
    pub fn get_ByteArray3D(&self)->&[Vec<Vec<u8>>]{&self.row.v_ByteArray3D}
    pub fn get_ShortArray3D(&self)->&[Vec<Vec<i16>>]{&self.row.v_ShortArray3D}
    pub fn get_IntArray3D(&self)->&[Vec<Vec<i32>>]{&self.row.v_IntArray3D}
    pub fn get_LongArray3D(&self)->&[Vec<Vec<i64>>]{&self.row.v_LongArray3D}
    pub fn get_FloatArray3D(&self)->&[Vec<Vec<f32>>]{&self.row.v_FloatArray3D}
    pub fn get_DoubleArray3D(&self)->&[Vec<Vec<f64>>]{&self.row.v_DoubleArray3D}
    pub fn get_BoolArray3D(&self)->&[Vec<Vec<bool>>]{&self.row.v_BoolArray3D}
    pub fn get_StringArray3D(&self)->&[Vec<Vec<String>>]{&self.row.v_StringArray3D}
    pub fn get_SByteArray3D(&self)->&[Vec<Vec<i8>>]{&self.row.v_SByteArray3D}
    pub fn get_UShortArray3D(&self)->&[Vec<Vec<u16>>]{&self.row.v_UShortArray3D}
    pub fn get_UIntArray3D(&self)->&[Vec<Vec<u32>>]{&self.row.v_UIntArray3D}
    pub fn get_ULongArray3D(&self)->&[Vec<Vec<u64>>]{&self.row.v_ULongArray3D}
    pub fn get_DecimalArray3D(&self)->&[Vec<Vec<R::Decimal>>]{&self.row.v_DecimalArray3D}
    pub fn get_QualityArray3D(&self)->&[Vec<Vec<T::T_Game_Config_Quality>>]{&self.row.v_QualityArray3D}
    pub fn get_Vector2Array3D(&self)->&[Vec<Vec<T::T_ZrConfig_Types_Vector2>>]{&self.row.v_Vector2Array3D}
    pub fn get_Vector3Array3D(&self)->&[Vec<Vec<T::T_ZrConfig_Types_Vector3>>]{&self.row.v_Vector3Array3D}
    pub fn get_KvArray3D(&self)->&[Vec<Vec<T::T_ZrConfig_Types_Kv>>]{&self.row.v_KvArray3D}
    pub fn get_MoneyArray3D(&self)->&[Vec<Vec<T::T_Game_Config_Money>>]{&self.row.v_MoneyArray3D}
    pub fn get_Settings(&self)->&Option<Box<T::T_Game_Config_BattleSettings>>{&self.row.v_Settings}
    pub fn get_Range(&self)->&T::T_Game_Config_ConfigRange{&self.row.v_Range}
    pub fn get_Options(&self)->&Option<Box<T::T_Game_Config_SampleOptions>>{&self.row.v_Options}
    pub fn get_Action(&self)->&Option<Box<T::T_Game_Config_BattleAction>>{&self.row.v_Action}
    pub fn get_Actions(&self)->&[Option<Box<T::T_Game_Config_BattleAction>>]{&self.row.v_Actions}
    pub fn get_Ranges(&self)->&[T::T_Game_Config_ConfigRange]{&self.row.v_Ranges}
    pub fn get_OptionsList(&self)->&[Option<Box<T::T_Game_Config_SampleOptions>>]{&self.row.v_OptionsList}
    pub fn get_Groups(&self)->&[Option<Box<T::T_Game_Config_ActionGroup>>]{&self.row.v_Groups}
    pub fn get_Phases(&self)->&[Option<Box<T::T_Game_Config_BattlePhase>>]{&self.row.v_Phases}
    pub fn get_OptionalRange(&self)->&Option<T::T_Game_Config_ConfigRange>{&self.row.v_OptionalRange}
    pub fn get_OptionalInt(&self)->&Option<i32>{&self.row.v_OptionalInt}
    pub fn get_OptionalUInt(&self)->&Option<u32>{&self.row.v_OptionalUInt}
    pub fn get_OptionalULong(&self)->&Option<u64>{&self.row.v_OptionalULong}
    pub fn get_OptionalDecimal(&self)->&Option<R::Decimal>{&self.row.v_OptionalDecimal}
    pub fn get_OptionalBool(&self)->&Option<bool>{&self.row.v_OptionalBool}
    pub fn get_OptionalQuality(&self)->&Option<T::T_Game_Config_Quality>{&self.row.v_OptionalQuality}
    pub fn get_OptionalMoney(&self)->&Option<T::T_Game_Config_Money>{&self.row.v_OptionalMoney}
    pub fn get_OptionalValues(&self)->&[Option<i32>]{&self.row.v_OptionalValues}
    pub fn get_StatMap(&self)->&R::Dictionary<String, i32>{&self.row.v_StatMap}
    pub fn get_PriceMap(&self)->&R::Dictionary<i32, Option<R::Decimal>>{&self.row.v_PriceMap}
    pub fn get_QualityMap(&self)->&R::Dictionary<T::T_Game_Config_Quality, u32>{&self.row.v_QualityMap}
    pub fn get_ListMap(&self)->&R::Dictionary<String, Vec<i32>>{&self.row.v_ListMap}
    pub fn get_IntMatrix(&self)->&R::Matrix<i32>{&self.row.v_IntMatrix}
    pub fn get_DecimalCube(&self)->&R::Matrix<R::Decimal>{&self.row.v_DecimalCube}
    pub fn get_OptionalMatrix(&self)->&R::Matrix<Option<u32>>{&self.row.v_OptionalMatrix}
    pub fn get_RawJsonValue(&self)->&str{&self.row.v_RawJsonValue}
    pub fn get_RawJsonArray(&self)->&[String]{&self.row.v_RawJsonArray}
    pub fn get_RawJsonArray2D(&self)->&[Vec<String>]{&self.row.v_RawJsonArray2D}
    pub fn get_RawJsonArray3D(&self)->&[Vec<Vec<String>>]{&self.row.v_RawJsonArray3D}
    pub fn get_JsonIntArray(&self)->&[i32]{&self.row.v_JsonIntArray}
}
struct AllTypesKey_Key{
    k0:i32,
}
impl AllTypesKey_Key{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct AllTypesIndex_Key{keys:Vec<AllTypesKey_Key>,index:R::Index,
}
impl AllTypesIndex_Key{
    fn build(keys:Vec<AllTypesKey_Key>)->ServerResult<Self>{
        let index=R::Index::build(keys.len(),true,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        Ok(Self{keys,index})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct AllTypesKey_Group_ById{
    k0:i32,
}
impl AllTypesKey_Group_ById{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct AllTypesIndex_Group_ById{keys:Vec<AllTypesKey_Group_ById>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl AllTypesIndex_Group_ById{
    fn build(keys:Vec<AllTypesKey_Group_ById>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct AllTypesKey_Group_ByQuality{
    k0:T::T_Game_Config_Quality,
}
impl AllTypesKey_Group_ByQuality{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:T::T_Game_Config_Quality::__zr_decode(value.field("QualityValue")?)?,
})}} 
struct AllTypesIndex_Group_ByQuality{keys:Vec<AllTypesKey_Group_ByQuality>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl AllTypesIndex_Group_ByQuality{
    fn build(keys:Vec<AllTypesKey_Group_ByQuality>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:T::T_Game_Config_Quality)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct AllTypesStorage{rows:R::RowStorage<AllTypesData>,key:AllTypesIndex_Key,all:OnceLock<Arc<[usize]>>,
    group_ById:R::DerivedValue<AllTypesIndex_Group_ById>,
    group_ByQuality:R::DerivedValue<AllTypesIndex_Group_ByQuality>,
}
impl AllTypesStorage{
    fn new(indexed:R::IndexedRows)->ServerResult<Self>{let mut keys=Vec::with_capacity(indexed.count());for i in 0..indexed.count(){keys.push(AllTypesKey_Key::decode(indexed.key(i)).map_err(R::data_error)?);}Ok(Self{rows:R::RowStorage::new(indexed,AllTypesData::decode),key:AllTypesIndex_Key::build(keys)?,all:OnceLock::new(),
        group_ById:R::DerivedValue::new(),
        group_ByQuality:R::DerivedValue::new(),
    })}
    fn ensure_ById(&self,cancellation:Option<&Cancellation>)->ServerResult<&AllTypesIndex_Group_ById>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_ById.get()};self.group_ById.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){AllTypesKey_Group_ById{k0:cached.v_Id}}else{AllTypesKey_Group_ById::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((AllTypesIndex_Group_ById::build(keys)?,retained))
        })
    }
    fn group_ById(&self,k0:i32,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_ById(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn ensure_ByQuality(&self,cancellation:Option<&Cancellation>)->ServerResult<&AllTypesIndex_Group_ByQuality>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_ByQuality.get()};self.group_ByQuality.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){AllTypesKey_Group_ByQuality{k0:cached.v_QualityValue}}else{AllTypesKey_Group_ByQuality::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((AllTypesIndex_Group_ByQuality::build(keys)?,retained))
        })
    }
    fn group_ByQuality(&self,k0:T::T_Game_Config_Quality,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_ByQuality(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{cancellation.check()?;if self.all.get().is_some(){return Ok(())}for i in 0..self.rows.count(){self.rows.load(i,cancellation)?;}
        self.ensure_ById(Some(cancellation))?;
        self.ensure_ByQuality(Some(cancellation))?;
        cancellation.check()?;let _=self.all.set(Arc::from((0..self.rows.count()).collect::<Vec<_>>().into_boxed_slice()));Ok(())
    }
}
pub struct AllTypes10kData{
    v_Id:i32,
    v_ByteValue:u8,
    v_ShortValue:i16,
    v_IntValue:i32,
    v_LongValue:i64,
    v_FloatValue:f32,
    v_DoubleValue:f64,
    v_BoolValue:bool,
    v_StringValue:String,
    v_SByteValue:i8,
    v_UShortValue:u16,
    v_UIntValue:u32,
    v_ULongValue:u64,
    v_DecimalValue:R::Decimal,
    v_QualityValue:T::T_Game_Config_Quality,
    v_Vector2Value:T::T_ZrConfig_Types_Vector2,
    v_Vector3Value:T::T_ZrConfig_Types_Vector3,
    v_KvValue:T::T_ZrConfig_Types_Kv,
    v_MoneyValue:T::T_Game_Config_Money,
    v_ByteArray:Vec<u8>,
    v_ShortArray:Vec<i16>,
    v_IntArray:Vec<i32>,
    v_LongArray:Vec<i64>,
    v_FloatArray:Vec<f32>,
    v_DoubleArray:Vec<f64>,
    v_BoolArray:Vec<bool>,
    v_StringArray:Vec<String>,
    v_SByteArray:Vec<i8>,
    v_UShortArray:Vec<u16>,
    v_UIntArray:Vec<u32>,
    v_ULongArray:Vec<u64>,
    v_DecimalArray:Vec<R::Decimal>,
    v_QualityArray:Vec<T::T_Game_Config_Quality>,
    v_Vector2Array:Vec<T::T_ZrConfig_Types_Vector2>,
    v_Vector3Array:Vec<T::T_ZrConfig_Types_Vector3>,
    v_KvArray:Vec<T::T_ZrConfig_Types_Kv>,
    v_MoneyArray:Vec<T::T_Game_Config_Money>,
    v_ByteArray2D:Vec<Vec<u8>>,
    v_ShortArray2D:Vec<Vec<i16>>,
    v_IntArray2D:Vec<Vec<i32>>,
    v_LongArray2D:Vec<Vec<i64>>,
    v_FloatArray2D:Vec<Vec<f32>>,
    v_DoubleArray2D:Vec<Vec<f64>>,
    v_BoolArray2D:Vec<Vec<bool>>,
    v_StringArray2D:Vec<Vec<String>>,
    v_SByteArray2D:Vec<Vec<i8>>,
    v_UShortArray2D:Vec<Vec<u16>>,
    v_UIntArray2D:Vec<Vec<u32>>,
    v_ULongArray2D:Vec<Vec<u64>>,
    v_DecimalArray2D:Vec<Vec<R::Decimal>>,
    v_QualityArray2D:Vec<Vec<T::T_Game_Config_Quality>>,
    v_Vector2Array2D:Vec<Vec<T::T_ZrConfig_Types_Vector2>>,
    v_Vector3Array2D:Vec<Vec<T::T_ZrConfig_Types_Vector3>>,
    v_KvArray2D:Vec<Vec<T::T_ZrConfig_Types_Kv>>,
    v_MoneyArray2D:Vec<Vec<T::T_Game_Config_Money>>,
    v_ByteArray3D:Vec<Vec<Vec<u8>>>,
    v_ShortArray3D:Vec<Vec<Vec<i16>>>,
    v_IntArray3D:Vec<Vec<Vec<i32>>>,
    v_LongArray3D:Vec<Vec<Vec<i64>>>,
    v_FloatArray3D:Vec<Vec<Vec<f32>>>,
    v_DoubleArray3D:Vec<Vec<Vec<f64>>>,
    v_BoolArray3D:Vec<Vec<Vec<bool>>>,
    v_StringArray3D:Vec<Vec<Vec<String>>>,
    v_SByteArray3D:Vec<Vec<Vec<i8>>>,
    v_UShortArray3D:Vec<Vec<Vec<u16>>>,
    v_UIntArray3D:Vec<Vec<Vec<u32>>>,
    v_ULongArray3D:Vec<Vec<Vec<u64>>>,
    v_DecimalArray3D:Vec<Vec<Vec<R::Decimal>>>,
    v_QualityArray3D:Vec<Vec<Vec<T::T_Game_Config_Quality>>>,
    v_Vector2Array3D:Vec<Vec<Vec<T::T_ZrConfig_Types_Vector2>>>,
    v_Vector3Array3D:Vec<Vec<Vec<T::T_ZrConfig_Types_Vector3>>>,
    v_KvArray3D:Vec<Vec<Vec<T::T_ZrConfig_Types_Kv>>>,
    v_MoneyArray3D:Vec<Vec<Vec<T::T_Game_Config_Money>>>,
    v_Settings:Option<Box<T::T_Game_Config_BattleSettings>>,
    v_Range:T::T_Game_Config_ConfigRange,
    v_Options:Option<Box<T::T_Game_Config_SampleOptions>>,
    v_Action:Option<Box<T::T_Game_Config_BattleAction>>,
    v_Actions:Vec<Option<Box<T::T_Game_Config_BattleAction>>>,
    v_Ranges:Vec<T::T_Game_Config_ConfigRange>,
    v_OptionsList:Vec<Option<Box<T::T_Game_Config_SampleOptions>>>,
    v_Groups:Vec<Option<Box<T::T_Game_Config_ActionGroup>>>,
    v_Phases:Vec<Option<Box<T::T_Game_Config_BattlePhase>>>,
    v_OptionalRange:Option<T::T_Game_Config_ConfigRange>,
    v_OptionalInt:Option<i32>,
    v_OptionalUInt:Option<u32>,
    v_OptionalULong:Option<u64>,
    v_OptionalDecimal:Option<R::Decimal>,
    v_OptionalBool:Option<bool>,
    v_OptionalQuality:Option<T::T_Game_Config_Quality>,
    v_OptionalMoney:Option<T::T_Game_Config_Money>,
    v_OptionalValues:Vec<Option<i32>>,
    v_StatMap:R::Dictionary<String, i32>,
    v_PriceMap:R::Dictionary<i32, Option<R::Decimal>>,
    v_QualityMap:R::Dictionary<T::T_Game_Config_Quality, u32>,
    v_ListMap:R::Dictionary<String, Vec<i32>>,
    v_IntMatrix:R::Matrix<i32>,
    v_DecimalCube:R::Matrix<R::Decimal>,
    v_OptionalMatrix:R::Matrix<Option<u32>>,
    v_RawJsonValue:String,
    v_RawJsonArray:Vec<String>,
    v_RawJsonArray2D:Vec<Vec<String>>,
    v_RawJsonArray3D:Vec<Vec<Vec<String>>>,
    v_JsonIntArray:Vec<i32>,
    v_Hero:i32,
    v_Skill:i32,
    v_Previous:i32,
}
impl AllTypes10kData{
    fn decode(value:&Json)->Result<Self>{Ok(Self{
        v_Id:R::scalar::<i32>(value.field("Id")?)?,
        v_ByteValue:R::scalar::<u8>(value.field("ByteValue")?)?,
        v_ShortValue:R::scalar::<i16>(value.field("ShortValue")?)?,
        v_IntValue:R::scalar::<i32>(value.field("IntValue")?)?,
        v_LongValue:R::scalar::<i64>(value.field("LongValue")?)?,
        v_FloatValue:R::scalar::<f32>(value.field("FloatValue")?)?,
        v_DoubleValue:R::scalar::<f64>(value.field("DoubleValue")?)?,
        v_BoolValue:R::scalar::<bool>(value.field("BoolValue")?)?,
        v_StringValue:R::scalar::<String>(value.field("StringValue")?)?,
        v_SByteValue:R::scalar::<i8>(value.field("SByteValue")?)?,
        v_UShortValue:R::scalar::<u16>(value.field("UShortValue")?)?,
        v_UIntValue:R::scalar::<u32>(value.field("UIntValue")?)?,
        v_ULongValue:R::scalar::<u64>(value.field("ULongValue")?)?,
        v_DecimalValue:R::scalar::<R::Decimal>(value.field("DecimalValue")?)?,
        v_QualityValue:T::T_Game_Config_Quality::__zr_decode(value.field("QualityValue")?)?,
        v_Vector2Value:T::T_ZrConfig_Types_Vector2::__zr_decode(value.field("Vector2Value")?)?,
        v_Vector3Value:T::T_ZrConfig_Types_Vector3::__zr_decode(value.field("Vector3Value")?)?,
        v_KvValue:T::T_ZrConfig_Types_Kv::__zr_decode(value.field("KvValue")?)?,
        v_MoneyValue:T::T_Game_Config_Money::__zr_decode(value.field("MoneyValue")?)?,
        v_ByteArray:R::decode_list(value.field("ByteArray")?, |item0| R::scalar::<u8>(item0))?,
        v_ShortArray:R::decode_list(value.field("ShortArray")?, |item0| R::scalar::<i16>(item0))?,
        v_IntArray:R::decode_list(value.field("IntArray")?, |item0| R::scalar::<i32>(item0))?,
        v_LongArray:R::decode_list(value.field("LongArray")?, |item0| R::scalar::<i64>(item0))?,
        v_FloatArray:R::decode_list(value.field("FloatArray")?, |item0| R::scalar::<f32>(item0))?,
        v_DoubleArray:R::decode_list(value.field("DoubleArray")?, |item0| R::scalar::<f64>(item0))?,
        v_BoolArray:R::decode_list(value.field("BoolArray")?, |item0| R::scalar::<bool>(item0))?,
        v_StringArray:R::decode_list(value.field("StringArray")?, |item0| R::scalar::<String>(item0))?,
        v_SByteArray:R::decode_list(value.field("SByteArray")?, |item0| R::scalar::<i8>(item0))?,
        v_UShortArray:R::decode_list(value.field("UShortArray")?, |item0| R::scalar::<u16>(item0))?,
        v_UIntArray:R::decode_list(value.field("UIntArray")?, |item0| R::scalar::<u32>(item0))?,
        v_ULongArray:R::decode_list(value.field("ULongArray")?, |item0| R::scalar::<u64>(item0))?,
        v_DecimalArray:R::decode_list(value.field("DecimalArray")?, |item0| R::scalar::<R::Decimal>(item0))?,
        v_QualityArray:R::decode_list(value.field("QualityArray")?, |item0| T::T_Game_Config_Quality::__zr_decode(item0))?,
        v_Vector2Array:R::decode_list(value.field("Vector2Array")?, |item0| T::T_ZrConfig_Types_Vector2::__zr_decode(item0))?,
        v_Vector3Array:R::decode_list(value.field("Vector3Array")?, |item0| T::T_ZrConfig_Types_Vector3::__zr_decode(item0))?,
        v_KvArray:R::decode_list(value.field("KvArray")?, |item0| T::T_ZrConfig_Types_Kv::__zr_decode(item0))?,
        v_MoneyArray:R::decode_list(value.field("MoneyArray")?, |item0| T::T_Game_Config_Money::__zr_decode(item0))?,
        v_ByteArray2D:R::decode_list(value.field("ByteArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<u8>(item1)))?,
        v_ShortArray2D:R::decode_list(value.field("ShortArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<i16>(item1)))?,
        v_IntArray2D:R::decode_list(value.field("IntArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<i32>(item1)))?,
        v_LongArray2D:R::decode_list(value.field("LongArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<i64>(item1)))?,
        v_FloatArray2D:R::decode_list(value.field("FloatArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<f32>(item1)))?,
        v_DoubleArray2D:R::decode_list(value.field("DoubleArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<f64>(item1)))?,
        v_BoolArray2D:R::decode_list(value.field("BoolArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<bool>(item1)))?,
        v_StringArray2D:R::decode_list(value.field("StringArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<String>(item1)))?,
        v_SByteArray2D:R::decode_list(value.field("SByteArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<i8>(item1)))?,
        v_UShortArray2D:R::decode_list(value.field("UShortArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<u16>(item1)))?,
        v_UIntArray2D:R::decode_list(value.field("UIntArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<u32>(item1)))?,
        v_ULongArray2D:R::decode_list(value.field("ULongArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<u64>(item1)))?,
        v_DecimalArray2D:R::decode_list(value.field("DecimalArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<R::Decimal>(item1)))?,
        v_QualityArray2D:R::decode_list(value.field("QualityArray2D")?, |item0| R::decode_list(item0, |item1| T::T_Game_Config_Quality::__zr_decode(item1)))?,
        v_Vector2Array2D:R::decode_list(value.field("Vector2Array2D")?, |item0| R::decode_list(item0, |item1| T::T_ZrConfig_Types_Vector2::__zr_decode(item1)))?,
        v_Vector3Array2D:R::decode_list(value.field("Vector3Array2D")?, |item0| R::decode_list(item0, |item1| T::T_ZrConfig_Types_Vector3::__zr_decode(item1)))?,
        v_KvArray2D:R::decode_list(value.field("KvArray2D")?, |item0| R::decode_list(item0, |item1| T::T_ZrConfig_Types_Kv::__zr_decode(item1)))?,
        v_MoneyArray2D:R::decode_list(value.field("MoneyArray2D")?, |item0| R::decode_list(item0, |item1| T::T_Game_Config_Money::__zr_decode(item1)))?,
        v_ByteArray3D:R::decode_list(value.field("ByteArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<u8>(item2))))?,
        v_ShortArray3D:R::decode_list(value.field("ShortArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<i16>(item2))))?,
        v_IntArray3D:R::decode_list(value.field("IntArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<i32>(item2))))?,
        v_LongArray3D:R::decode_list(value.field("LongArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<i64>(item2))))?,
        v_FloatArray3D:R::decode_list(value.field("FloatArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<f32>(item2))))?,
        v_DoubleArray3D:R::decode_list(value.field("DoubleArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<f64>(item2))))?,
        v_BoolArray3D:R::decode_list(value.field("BoolArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<bool>(item2))))?,
        v_StringArray3D:R::decode_list(value.field("StringArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<String>(item2))))?,
        v_SByteArray3D:R::decode_list(value.field("SByteArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<i8>(item2))))?,
        v_UShortArray3D:R::decode_list(value.field("UShortArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<u16>(item2))))?,
        v_UIntArray3D:R::decode_list(value.field("UIntArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<u32>(item2))))?,
        v_ULongArray3D:R::decode_list(value.field("ULongArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<u64>(item2))))?,
        v_DecimalArray3D:R::decode_list(value.field("DecimalArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<R::Decimal>(item2))))?,
        v_QualityArray3D:R::decode_list(value.field("QualityArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_Game_Config_Quality::__zr_decode(item2))))?,
        v_Vector2Array3D:R::decode_list(value.field("Vector2Array3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_ZrConfig_Types_Vector2::__zr_decode(item2))))?,
        v_Vector3Array3D:R::decode_list(value.field("Vector3Array3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_ZrConfig_Types_Vector3::__zr_decode(item2))))?,
        v_KvArray3D:R::decode_list(value.field("KvArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_ZrConfig_Types_Kv::__zr_decode(item2))))?,
        v_MoneyArray3D:R::decode_list(value.field("MoneyArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| T::T_Game_Config_Money::__zr_decode(item2))))?,
        v_Settings:R::decode_optional(value.field("Settings")?, |v| T::T_Game_Config_BattleSettings::__zr_decode(v).map(Box::new))?,
        v_Range:T::T_Game_Config_ConfigRange::__zr_decode(value.field("Range")?)?,
        v_Options:R::decode_optional(value.field("Options")?, |v| T::T_Game_Config_SampleOptions::__zr_decode(v).map(Box::new))?,
        v_Action:R::decode_optional(value.field("Action")?, |v| T::T_Game_Config_BattleAction::__zr_decode(v).map(Box::new))?,
        v_Actions:R::decode_list(value.field("Actions")?, |item0| R::decode_optional(item0, |v| T::T_Game_Config_BattleAction::__zr_decode(v).map(Box::new)))?,
        v_Ranges:R::decode_list(value.field("Ranges")?, |item0| T::T_Game_Config_ConfigRange::__zr_decode(item0))?,
        v_OptionsList:R::decode_list(value.field("OptionsList")?, |item0| R::decode_optional(item0, |v| T::T_Game_Config_SampleOptions::__zr_decode(v).map(Box::new)))?,
        v_Groups:R::decode_list(value.field("Groups")?, |item0| R::decode_optional(item0, |v| T::T_Game_Config_ActionGroup::__zr_decode(v).map(Box::new)))?,
        v_Phases:R::decode_list(value.field("Phases")?, |item0| R::decode_optional(item0, |v| T::T_Game_Config_BattlePhase::__zr_decode(v).map(Box::new)))?,
        v_OptionalRange:R::decode_optional(value.field("OptionalRange")?, |item0| T::T_Game_Config_ConfigRange::__zr_decode(item0))?,
        v_OptionalInt:R::decode_optional(value.field("OptionalInt")?, |item0| R::scalar::<i32>(item0))?,
        v_OptionalUInt:R::decode_optional(value.field("OptionalUInt")?, |item0| R::scalar::<u32>(item0))?,
        v_OptionalULong:R::decode_optional(value.field("OptionalULong")?, |item0| R::scalar::<u64>(item0))?,
        v_OptionalDecimal:R::decode_optional(value.field("OptionalDecimal")?, |item0| R::scalar::<R::Decimal>(item0))?,
        v_OptionalBool:R::decode_optional(value.field("OptionalBool")?, |item0| R::scalar::<bool>(item0))?,
        v_OptionalQuality:R::decode_optional(value.field("OptionalQuality")?, |item0| T::T_Game_Config_Quality::__zr_decode(item0))?,
        v_OptionalMoney:R::decode_optional(value.field("OptionalMoney")?, |item0| T::T_Game_Config_Money::__zr_decode(item0))?,
        v_OptionalValues:R::decode_list(value.field("OptionalValues")?, |item0| R::decode_optional(item0, |item1| R::scalar::<i32>(item1)))?,
        v_StatMap:R::decode_dictionary(value.field("StatMap")?, |item0| R::scalar::<String>(item0), |item0| R::scalar::<i32>(item0))?,
        v_PriceMap:R::decode_dictionary(value.field("PriceMap")?, |item0| R::scalar::<i32>(item0), |item0| R::decode_optional(item0, |item1| R::scalar::<R::Decimal>(item1)))?,
        v_QualityMap:R::decode_dictionary(value.field("QualityMap")?, |item0| T::T_Game_Config_Quality::__zr_decode(item0), |item0| R::scalar::<u32>(item0))?,
        v_ListMap:R::decode_dictionary(value.field("ListMap")?, |item0| R::scalar::<String>(item0), |item0| R::decode_list(item0, |item1| R::scalar::<i32>(item1)))?,
        v_IntMatrix:R::decode_matrix(value.field("IntMatrix")?, |item0| R::scalar::<i32>(item0))?,
        v_DecimalCube:R::decode_matrix(value.field("DecimalCube")?, |item0| R::scalar::<R::Decimal>(item0))?,
        v_OptionalMatrix:R::decode_matrix(value.field("OptionalMatrix")?, |item0| R::decode_optional(item0, |item1| R::scalar::<u32>(item1)))?,
        v_RawJsonValue:R::scalar::<String>(value.field("RawJsonValue")?)?,
        v_RawJsonArray:R::decode_list(value.field("RawJsonArray")?, |item0| R::scalar::<String>(item0))?,
        v_RawJsonArray2D:R::decode_list(value.field("RawJsonArray2D")?, |item0| R::decode_list(item0, |item1| R::scalar::<String>(item1)))?,
        v_RawJsonArray3D:R::decode_list(value.field("RawJsonArray3D")?, |item0| R::decode_list(item0, |item1| R::decode_list(item1, |item2| R::scalar::<String>(item2))))?,
        v_JsonIntArray:R::decode_list(value.field("JsonIntArray")?, |item0| R::scalar::<i32>(item0))?,
        v_Hero:R::scalar::<i32>(value.field("Hero")?)?,
        v_Skill:R::scalar::<i32>(value.field("Skill")?)?,
        v_Previous:R::scalar::<i32>(value.field("Previous")?)?,
    })}
    pub fn get_Id(&self)->i32{self.v_Id}
    pub fn get_ByteValue(&self)->u8{self.v_ByteValue}
    pub fn get_ShortValue(&self)->i16{self.v_ShortValue}
    pub fn get_IntValue(&self)->i32{self.v_IntValue}
    pub fn get_LongValue(&self)->i64{self.v_LongValue}
    pub fn get_FloatValue(&self)->f32{self.v_FloatValue}
    pub fn get_DoubleValue(&self)->f64{self.v_DoubleValue}
    pub fn get_BoolValue(&self)->bool{self.v_BoolValue}
    pub fn get_StringValue(&self)->&str{&self.v_StringValue}
    pub fn get_SByteValue(&self)->i8{self.v_SByteValue}
    pub fn get_UShortValue(&self)->u16{self.v_UShortValue}
    pub fn get_UIntValue(&self)->u32{self.v_UIntValue}
    pub fn get_ULongValue(&self)->u64{self.v_ULongValue}
    pub fn get_DecimalValue(&self)->R::Decimal{self.v_DecimalValue}
    pub fn get_QualityValue(&self)->T::T_Game_Config_Quality{self.v_QualityValue}
    pub fn get_Vector2Value(&self)->&T::T_ZrConfig_Types_Vector2{&self.v_Vector2Value}
    pub fn get_Vector3Value(&self)->&T::T_ZrConfig_Types_Vector3{&self.v_Vector3Value}
    pub fn get_KvValue(&self)->&T::T_ZrConfig_Types_Kv{&self.v_KvValue}
    pub fn get_MoneyValue(&self)->&T::T_Game_Config_Money{&self.v_MoneyValue}
    pub fn get_ByteArray(&self)->&[u8]{&self.v_ByteArray}
    pub fn get_ShortArray(&self)->&[i16]{&self.v_ShortArray}
    pub fn get_IntArray(&self)->&[i32]{&self.v_IntArray}
    pub fn get_LongArray(&self)->&[i64]{&self.v_LongArray}
    pub fn get_FloatArray(&self)->&[f32]{&self.v_FloatArray}
    pub fn get_DoubleArray(&self)->&[f64]{&self.v_DoubleArray}
    pub fn get_BoolArray(&self)->&[bool]{&self.v_BoolArray}
    pub fn get_StringArray(&self)->&[String]{&self.v_StringArray}
    pub fn get_SByteArray(&self)->&[i8]{&self.v_SByteArray}
    pub fn get_UShortArray(&self)->&[u16]{&self.v_UShortArray}
    pub fn get_UIntArray(&self)->&[u32]{&self.v_UIntArray}
    pub fn get_ULongArray(&self)->&[u64]{&self.v_ULongArray}
    pub fn get_DecimalArray(&self)->&[R::Decimal]{&self.v_DecimalArray}
    pub fn get_QualityArray(&self)->&[T::T_Game_Config_Quality]{&self.v_QualityArray}
    pub fn get_Vector2Array(&self)->&[T::T_ZrConfig_Types_Vector2]{&self.v_Vector2Array}
    pub fn get_Vector3Array(&self)->&[T::T_ZrConfig_Types_Vector3]{&self.v_Vector3Array}
    pub fn get_KvArray(&self)->&[T::T_ZrConfig_Types_Kv]{&self.v_KvArray}
    pub fn get_MoneyArray(&self)->&[T::T_Game_Config_Money]{&self.v_MoneyArray}
    pub fn get_ByteArray2D(&self)->&[Vec<u8>]{&self.v_ByteArray2D}
    pub fn get_ShortArray2D(&self)->&[Vec<i16>]{&self.v_ShortArray2D}
    pub fn get_IntArray2D(&self)->&[Vec<i32>]{&self.v_IntArray2D}
    pub fn get_LongArray2D(&self)->&[Vec<i64>]{&self.v_LongArray2D}
    pub fn get_FloatArray2D(&self)->&[Vec<f32>]{&self.v_FloatArray2D}
    pub fn get_DoubleArray2D(&self)->&[Vec<f64>]{&self.v_DoubleArray2D}
    pub fn get_BoolArray2D(&self)->&[Vec<bool>]{&self.v_BoolArray2D}
    pub fn get_StringArray2D(&self)->&[Vec<String>]{&self.v_StringArray2D}
    pub fn get_SByteArray2D(&self)->&[Vec<i8>]{&self.v_SByteArray2D}
    pub fn get_UShortArray2D(&self)->&[Vec<u16>]{&self.v_UShortArray2D}
    pub fn get_UIntArray2D(&self)->&[Vec<u32>]{&self.v_UIntArray2D}
    pub fn get_ULongArray2D(&self)->&[Vec<u64>]{&self.v_ULongArray2D}
    pub fn get_DecimalArray2D(&self)->&[Vec<R::Decimal>]{&self.v_DecimalArray2D}
    pub fn get_QualityArray2D(&self)->&[Vec<T::T_Game_Config_Quality>]{&self.v_QualityArray2D}
    pub fn get_Vector2Array2D(&self)->&[Vec<T::T_ZrConfig_Types_Vector2>]{&self.v_Vector2Array2D}
    pub fn get_Vector3Array2D(&self)->&[Vec<T::T_ZrConfig_Types_Vector3>]{&self.v_Vector3Array2D}
    pub fn get_KvArray2D(&self)->&[Vec<T::T_ZrConfig_Types_Kv>]{&self.v_KvArray2D}
    pub fn get_MoneyArray2D(&self)->&[Vec<T::T_Game_Config_Money>]{&self.v_MoneyArray2D}
    pub fn get_ByteArray3D(&self)->&[Vec<Vec<u8>>]{&self.v_ByteArray3D}
    pub fn get_ShortArray3D(&self)->&[Vec<Vec<i16>>]{&self.v_ShortArray3D}
    pub fn get_IntArray3D(&self)->&[Vec<Vec<i32>>]{&self.v_IntArray3D}
    pub fn get_LongArray3D(&self)->&[Vec<Vec<i64>>]{&self.v_LongArray3D}
    pub fn get_FloatArray3D(&self)->&[Vec<Vec<f32>>]{&self.v_FloatArray3D}
    pub fn get_DoubleArray3D(&self)->&[Vec<Vec<f64>>]{&self.v_DoubleArray3D}
    pub fn get_BoolArray3D(&self)->&[Vec<Vec<bool>>]{&self.v_BoolArray3D}
    pub fn get_StringArray3D(&self)->&[Vec<Vec<String>>]{&self.v_StringArray3D}
    pub fn get_SByteArray3D(&self)->&[Vec<Vec<i8>>]{&self.v_SByteArray3D}
    pub fn get_UShortArray3D(&self)->&[Vec<Vec<u16>>]{&self.v_UShortArray3D}
    pub fn get_UIntArray3D(&self)->&[Vec<Vec<u32>>]{&self.v_UIntArray3D}
    pub fn get_ULongArray3D(&self)->&[Vec<Vec<u64>>]{&self.v_ULongArray3D}
    pub fn get_DecimalArray3D(&self)->&[Vec<Vec<R::Decimal>>]{&self.v_DecimalArray3D}
    pub fn get_QualityArray3D(&self)->&[Vec<Vec<T::T_Game_Config_Quality>>]{&self.v_QualityArray3D}
    pub fn get_Vector2Array3D(&self)->&[Vec<Vec<T::T_ZrConfig_Types_Vector2>>]{&self.v_Vector2Array3D}
    pub fn get_Vector3Array3D(&self)->&[Vec<Vec<T::T_ZrConfig_Types_Vector3>>]{&self.v_Vector3Array3D}
    pub fn get_KvArray3D(&self)->&[Vec<Vec<T::T_ZrConfig_Types_Kv>>]{&self.v_KvArray3D}
    pub fn get_MoneyArray3D(&self)->&[Vec<Vec<T::T_Game_Config_Money>>]{&self.v_MoneyArray3D}
    pub fn get_Settings(&self)->&Option<Box<T::T_Game_Config_BattleSettings>>{&self.v_Settings}
    pub fn get_Range(&self)->&T::T_Game_Config_ConfigRange{&self.v_Range}
    pub fn get_Options(&self)->&Option<Box<T::T_Game_Config_SampleOptions>>{&self.v_Options}
    pub fn get_Action(&self)->&Option<Box<T::T_Game_Config_BattleAction>>{&self.v_Action}
    pub fn get_Actions(&self)->&[Option<Box<T::T_Game_Config_BattleAction>>]{&self.v_Actions}
    pub fn get_Ranges(&self)->&[T::T_Game_Config_ConfigRange]{&self.v_Ranges}
    pub fn get_OptionsList(&self)->&[Option<Box<T::T_Game_Config_SampleOptions>>]{&self.v_OptionsList}
    pub fn get_Groups(&self)->&[Option<Box<T::T_Game_Config_ActionGroup>>]{&self.v_Groups}
    pub fn get_Phases(&self)->&[Option<Box<T::T_Game_Config_BattlePhase>>]{&self.v_Phases}
    pub fn get_OptionalRange(&self)->&Option<T::T_Game_Config_ConfigRange>{&self.v_OptionalRange}
    pub fn get_OptionalInt(&self)->&Option<i32>{&self.v_OptionalInt}
    pub fn get_OptionalUInt(&self)->&Option<u32>{&self.v_OptionalUInt}
    pub fn get_OptionalULong(&self)->&Option<u64>{&self.v_OptionalULong}
    pub fn get_OptionalDecimal(&self)->&Option<R::Decimal>{&self.v_OptionalDecimal}
    pub fn get_OptionalBool(&self)->&Option<bool>{&self.v_OptionalBool}
    pub fn get_OptionalQuality(&self)->&Option<T::T_Game_Config_Quality>{&self.v_OptionalQuality}
    pub fn get_OptionalMoney(&self)->&Option<T::T_Game_Config_Money>{&self.v_OptionalMoney}
    pub fn get_OptionalValues(&self)->&[Option<i32>]{&self.v_OptionalValues}
    pub fn get_StatMap(&self)->&R::Dictionary<String, i32>{&self.v_StatMap}
    pub fn get_PriceMap(&self)->&R::Dictionary<i32, Option<R::Decimal>>{&self.v_PriceMap}
    pub fn get_QualityMap(&self)->&R::Dictionary<T::T_Game_Config_Quality, u32>{&self.v_QualityMap}
    pub fn get_ListMap(&self)->&R::Dictionary<String, Vec<i32>>{&self.v_ListMap}
    pub fn get_IntMatrix(&self)->&R::Matrix<i32>{&self.v_IntMatrix}
    pub fn get_DecimalCube(&self)->&R::Matrix<R::Decimal>{&self.v_DecimalCube}
    pub fn get_OptionalMatrix(&self)->&R::Matrix<Option<u32>>{&self.v_OptionalMatrix}
    pub fn get_RawJsonValue(&self)->&str{&self.v_RawJsonValue}
    pub fn get_RawJsonArray(&self)->&[String]{&self.v_RawJsonArray}
    pub fn get_RawJsonArray2D(&self)->&[Vec<String>]{&self.v_RawJsonArray2D}
    pub fn get_RawJsonArray3D(&self)->&[Vec<Vec<String>>]{&self.v_RawJsonArray3D}
    pub fn get_JsonIntArray(&self)->&[i32]{&self.v_JsonIntArray}
    pub fn get_HeroId(&self)->i32{self.v_Hero}
    pub fn get_SkillId(&self)->i32{self.v_Skill}
    pub fn get_PreviousId(&self)->i32{self.v_Previous}
}
pub type AllTypes10kRow=R::RowHandle<Snapshot,AllTypes10kData>;
impl R::RowHandle<Snapshot,AllTypes10kData>{
    pub fn get_Id(&self)->i32{self.row.v_Id}
    pub fn get_ByteValue(&self)->u8{self.row.v_ByteValue}
    pub fn get_ShortValue(&self)->i16{self.row.v_ShortValue}
    pub fn get_IntValue(&self)->i32{self.row.v_IntValue}
    pub fn get_LongValue(&self)->i64{self.row.v_LongValue}
    pub fn get_FloatValue(&self)->f32{self.row.v_FloatValue}
    pub fn get_DoubleValue(&self)->f64{self.row.v_DoubleValue}
    pub fn get_BoolValue(&self)->bool{self.row.v_BoolValue}
    pub fn get_StringValue(&self)->&str{&self.row.v_StringValue}
    pub fn get_SByteValue(&self)->i8{self.row.v_SByteValue}
    pub fn get_UShortValue(&self)->u16{self.row.v_UShortValue}
    pub fn get_UIntValue(&self)->u32{self.row.v_UIntValue}
    pub fn get_ULongValue(&self)->u64{self.row.v_ULongValue}
    pub fn get_DecimalValue(&self)->R::Decimal{self.row.v_DecimalValue}
    pub fn get_QualityValue(&self)->T::T_Game_Config_Quality{self.row.v_QualityValue}
    pub fn get_Vector2Value(&self)->&T::T_ZrConfig_Types_Vector2{&self.row.v_Vector2Value}
    pub fn get_Vector3Value(&self)->&T::T_ZrConfig_Types_Vector3{&self.row.v_Vector3Value}
    pub fn get_KvValue(&self)->&T::T_ZrConfig_Types_Kv{&self.row.v_KvValue}
    pub fn get_MoneyValue(&self)->&T::T_Game_Config_Money{&self.row.v_MoneyValue}
    pub fn get_ByteArray(&self)->&[u8]{&self.row.v_ByteArray}
    pub fn get_ShortArray(&self)->&[i16]{&self.row.v_ShortArray}
    pub fn get_IntArray(&self)->&[i32]{&self.row.v_IntArray}
    pub fn get_LongArray(&self)->&[i64]{&self.row.v_LongArray}
    pub fn get_FloatArray(&self)->&[f32]{&self.row.v_FloatArray}
    pub fn get_DoubleArray(&self)->&[f64]{&self.row.v_DoubleArray}
    pub fn get_BoolArray(&self)->&[bool]{&self.row.v_BoolArray}
    pub fn get_StringArray(&self)->&[String]{&self.row.v_StringArray}
    pub fn get_SByteArray(&self)->&[i8]{&self.row.v_SByteArray}
    pub fn get_UShortArray(&self)->&[u16]{&self.row.v_UShortArray}
    pub fn get_UIntArray(&self)->&[u32]{&self.row.v_UIntArray}
    pub fn get_ULongArray(&self)->&[u64]{&self.row.v_ULongArray}
    pub fn get_DecimalArray(&self)->&[R::Decimal]{&self.row.v_DecimalArray}
    pub fn get_QualityArray(&self)->&[T::T_Game_Config_Quality]{&self.row.v_QualityArray}
    pub fn get_Vector2Array(&self)->&[T::T_ZrConfig_Types_Vector2]{&self.row.v_Vector2Array}
    pub fn get_Vector3Array(&self)->&[T::T_ZrConfig_Types_Vector3]{&self.row.v_Vector3Array}
    pub fn get_KvArray(&self)->&[T::T_ZrConfig_Types_Kv]{&self.row.v_KvArray}
    pub fn get_MoneyArray(&self)->&[T::T_Game_Config_Money]{&self.row.v_MoneyArray}
    pub fn get_ByteArray2D(&self)->&[Vec<u8>]{&self.row.v_ByteArray2D}
    pub fn get_ShortArray2D(&self)->&[Vec<i16>]{&self.row.v_ShortArray2D}
    pub fn get_IntArray2D(&self)->&[Vec<i32>]{&self.row.v_IntArray2D}
    pub fn get_LongArray2D(&self)->&[Vec<i64>]{&self.row.v_LongArray2D}
    pub fn get_FloatArray2D(&self)->&[Vec<f32>]{&self.row.v_FloatArray2D}
    pub fn get_DoubleArray2D(&self)->&[Vec<f64>]{&self.row.v_DoubleArray2D}
    pub fn get_BoolArray2D(&self)->&[Vec<bool>]{&self.row.v_BoolArray2D}
    pub fn get_StringArray2D(&self)->&[Vec<String>]{&self.row.v_StringArray2D}
    pub fn get_SByteArray2D(&self)->&[Vec<i8>]{&self.row.v_SByteArray2D}
    pub fn get_UShortArray2D(&self)->&[Vec<u16>]{&self.row.v_UShortArray2D}
    pub fn get_UIntArray2D(&self)->&[Vec<u32>]{&self.row.v_UIntArray2D}
    pub fn get_ULongArray2D(&self)->&[Vec<u64>]{&self.row.v_ULongArray2D}
    pub fn get_DecimalArray2D(&self)->&[Vec<R::Decimal>]{&self.row.v_DecimalArray2D}
    pub fn get_QualityArray2D(&self)->&[Vec<T::T_Game_Config_Quality>]{&self.row.v_QualityArray2D}
    pub fn get_Vector2Array2D(&self)->&[Vec<T::T_ZrConfig_Types_Vector2>]{&self.row.v_Vector2Array2D}
    pub fn get_Vector3Array2D(&self)->&[Vec<T::T_ZrConfig_Types_Vector3>]{&self.row.v_Vector3Array2D}
    pub fn get_KvArray2D(&self)->&[Vec<T::T_ZrConfig_Types_Kv>]{&self.row.v_KvArray2D}
    pub fn get_MoneyArray2D(&self)->&[Vec<T::T_Game_Config_Money>]{&self.row.v_MoneyArray2D}
    pub fn get_ByteArray3D(&self)->&[Vec<Vec<u8>>]{&self.row.v_ByteArray3D}
    pub fn get_ShortArray3D(&self)->&[Vec<Vec<i16>>]{&self.row.v_ShortArray3D}
    pub fn get_IntArray3D(&self)->&[Vec<Vec<i32>>]{&self.row.v_IntArray3D}
    pub fn get_LongArray3D(&self)->&[Vec<Vec<i64>>]{&self.row.v_LongArray3D}
    pub fn get_FloatArray3D(&self)->&[Vec<Vec<f32>>]{&self.row.v_FloatArray3D}
    pub fn get_DoubleArray3D(&self)->&[Vec<Vec<f64>>]{&self.row.v_DoubleArray3D}
    pub fn get_BoolArray3D(&self)->&[Vec<Vec<bool>>]{&self.row.v_BoolArray3D}
    pub fn get_StringArray3D(&self)->&[Vec<Vec<String>>]{&self.row.v_StringArray3D}
    pub fn get_SByteArray3D(&self)->&[Vec<Vec<i8>>]{&self.row.v_SByteArray3D}
    pub fn get_UShortArray3D(&self)->&[Vec<Vec<u16>>]{&self.row.v_UShortArray3D}
    pub fn get_UIntArray3D(&self)->&[Vec<Vec<u32>>]{&self.row.v_UIntArray3D}
    pub fn get_ULongArray3D(&self)->&[Vec<Vec<u64>>]{&self.row.v_ULongArray3D}
    pub fn get_DecimalArray3D(&self)->&[Vec<Vec<R::Decimal>>]{&self.row.v_DecimalArray3D}
    pub fn get_QualityArray3D(&self)->&[Vec<Vec<T::T_Game_Config_Quality>>]{&self.row.v_QualityArray3D}
    pub fn get_Vector2Array3D(&self)->&[Vec<Vec<T::T_ZrConfig_Types_Vector2>>]{&self.row.v_Vector2Array3D}
    pub fn get_Vector3Array3D(&self)->&[Vec<Vec<T::T_ZrConfig_Types_Vector3>>]{&self.row.v_Vector3Array3D}
    pub fn get_KvArray3D(&self)->&[Vec<Vec<T::T_ZrConfig_Types_Kv>>]{&self.row.v_KvArray3D}
    pub fn get_MoneyArray3D(&self)->&[Vec<Vec<T::T_Game_Config_Money>>]{&self.row.v_MoneyArray3D}
    pub fn get_Settings(&self)->&Option<Box<T::T_Game_Config_BattleSettings>>{&self.row.v_Settings}
    pub fn get_Range(&self)->&T::T_Game_Config_ConfigRange{&self.row.v_Range}
    pub fn get_Options(&self)->&Option<Box<T::T_Game_Config_SampleOptions>>{&self.row.v_Options}
    pub fn get_Action(&self)->&Option<Box<T::T_Game_Config_BattleAction>>{&self.row.v_Action}
    pub fn get_Actions(&self)->&[Option<Box<T::T_Game_Config_BattleAction>>]{&self.row.v_Actions}
    pub fn get_Ranges(&self)->&[T::T_Game_Config_ConfigRange]{&self.row.v_Ranges}
    pub fn get_OptionsList(&self)->&[Option<Box<T::T_Game_Config_SampleOptions>>]{&self.row.v_OptionsList}
    pub fn get_Groups(&self)->&[Option<Box<T::T_Game_Config_ActionGroup>>]{&self.row.v_Groups}
    pub fn get_Phases(&self)->&[Option<Box<T::T_Game_Config_BattlePhase>>]{&self.row.v_Phases}
    pub fn get_OptionalRange(&self)->&Option<T::T_Game_Config_ConfigRange>{&self.row.v_OptionalRange}
    pub fn get_OptionalInt(&self)->&Option<i32>{&self.row.v_OptionalInt}
    pub fn get_OptionalUInt(&self)->&Option<u32>{&self.row.v_OptionalUInt}
    pub fn get_OptionalULong(&self)->&Option<u64>{&self.row.v_OptionalULong}
    pub fn get_OptionalDecimal(&self)->&Option<R::Decimal>{&self.row.v_OptionalDecimal}
    pub fn get_OptionalBool(&self)->&Option<bool>{&self.row.v_OptionalBool}
    pub fn get_OptionalQuality(&self)->&Option<T::T_Game_Config_Quality>{&self.row.v_OptionalQuality}
    pub fn get_OptionalMoney(&self)->&Option<T::T_Game_Config_Money>{&self.row.v_OptionalMoney}
    pub fn get_OptionalValues(&self)->&[Option<i32>]{&self.row.v_OptionalValues}
    pub fn get_StatMap(&self)->&R::Dictionary<String, i32>{&self.row.v_StatMap}
    pub fn get_PriceMap(&self)->&R::Dictionary<i32, Option<R::Decimal>>{&self.row.v_PriceMap}
    pub fn get_QualityMap(&self)->&R::Dictionary<T::T_Game_Config_Quality, u32>{&self.row.v_QualityMap}
    pub fn get_ListMap(&self)->&R::Dictionary<String, Vec<i32>>{&self.row.v_ListMap}
    pub fn get_IntMatrix(&self)->&R::Matrix<i32>{&self.row.v_IntMatrix}
    pub fn get_DecimalCube(&self)->&R::Matrix<R::Decimal>{&self.row.v_DecimalCube}
    pub fn get_OptionalMatrix(&self)->&R::Matrix<Option<u32>>{&self.row.v_OptionalMatrix}
    pub fn get_RawJsonValue(&self)->&str{&self.row.v_RawJsonValue}
    pub fn get_RawJsonArray(&self)->&[String]{&self.row.v_RawJsonArray}
    pub fn get_RawJsonArray2D(&self)->&[Vec<String>]{&self.row.v_RawJsonArray2D}
    pub fn get_RawJsonArray3D(&self)->&[Vec<Vec<String>>]{&self.row.v_RawJsonArray3D}
    pub fn get_JsonIntArray(&self)->&[i32]{&self.row.v_JsonIntArray}
    pub fn get_HeroId(&self)->i32{self.row.v_Hero}
    pub fn get_Hero(&self)->ServerResult<Option<HeroRow>>{self.snapshot.reference_Hero(self.row.v_Hero,None)}
    pub fn load_Hero(&self,cancellation:&Cancellation)->ServerResult<Option<HeroRow>>{self.snapshot.reference_Hero(self.row.v_Hero,Some(cancellation))}
    pub fn get_SkillId(&self)->i32{self.row.v_Skill}
    pub fn get_Skill(&self)->ServerResult<Option<SkillRow>>{self.snapshot.reference_Skill(self.row.v_Skill,None)}
    pub fn load_Skill(&self,cancellation:&Cancellation)->ServerResult<Option<SkillRow>>{self.snapshot.reference_Skill(self.row.v_Skill,Some(cancellation))}
    pub fn get_PreviousId(&self)->i32{self.row.v_Previous}
    pub fn get_Previous(&self)->ServerResult<Option<AllTypes10kRow>>{self.snapshot.reference_AllTypes10k(self.row.v_Previous,None)}
    pub fn load_Previous(&self,cancellation:&Cancellation)->ServerResult<Option<AllTypes10kRow>>{self.snapshot.reference_AllTypes10k(self.row.v_Previous,Some(cancellation))}
}
struct AllTypes10kKey_Key{
    k0:i32,
}
impl AllTypes10kKey_Key{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct AllTypes10kIndex_Key{keys:Vec<AllTypes10kKey_Key>,index:R::Index,
}
impl AllTypes10kIndex_Key{
    fn build(keys:Vec<AllTypes10kKey_Key>)->ServerResult<Self>{
        let index=R::Index::build(keys.len(),true,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        Ok(Self{keys,index})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct AllTypes10kKey_Group_ById{
    k0:i32,
}
impl AllTypes10kKey_Group_ById{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct AllTypes10kIndex_Group_ById{keys:Vec<AllTypes10kKey_Group_ById>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl AllTypes10kIndex_Group_ById{
    fn build(keys:Vec<AllTypes10kKey_Group_ById>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct AllTypes10kStorage{rows:R::RowStorage<AllTypes10kData>,key:AllTypes10kIndex_Key,all:OnceLock<Arc<[usize]>>,
    group_ById:R::DerivedValue<AllTypes10kIndex_Group_ById>,
}
impl AllTypes10kStorage{
    fn new(indexed:R::IndexedRows)->ServerResult<Self>{let mut keys=Vec::with_capacity(indexed.count());for i in 0..indexed.count(){keys.push(AllTypes10kKey_Key::decode(indexed.key(i)).map_err(R::data_error)?);}Ok(Self{rows:R::RowStorage::new(indexed,AllTypes10kData::decode),key:AllTypes10kIndex_Key::build(keys)?,all:OnceLock::new(),
        group_ById:R::DerivedValue::new(),
    })}
    fn ensure_ById(&self,cancellation:Option<&Cancellation>)->ServerResult<&AllTypes10kIndex_Group_ById>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_ById.get()};self.group_ById.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){AllTypes10kKey_Group_ById{k0:cached.v_Id}}else{AllTypes10kKey_Group_ById::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((AllTypes10kIndex_Group_ById::build(keys)?,retained))
        })
    }
    fn group_ById(&self,k0:i32,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_ById(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{cancellation.check()?;if self.all.get().is_some(){return Ok(())}for i in 0..self.rows.count(){self.rows.load(i,cancellation)?;}
        self.ensure_ById(Some(cancellation))?;
        cancellation.check()?;let _=self.all.set(Arc::from((0..self.rows.count()).collect::<Vec<_>>().into_boxed_slice()));Ok(())
    }
}
pub struct DropData{
    v_GroupId:i32,
    v_Id:i32,
    v_Count:i32,
}
impl DropData{
    fn decode(value:&Json)->Result<Self>{Ok(Self{
        v_GroupId:R::scalar::<i32>(value.field("GroupId")?)?,
        v_Id:R::scalar::<i32>(value.field("Id")?)?,
        v_Count:R::scalar::<i32>(value.field("Count")?)?,
    })}
    pub fn get_GroupId(&self)->i32{self.v_GroupId}
    pub fn get_Id(&self)->i32{self.v_Id}
    pub fn get_Count(&self)->i32{self.v_Count}
}
pub type DropRow=R::RowHandle<Snapshot,DropData>;
impl R::RowHandle<Snapshot,DropData>{
    pub fn get_GroupId(&self)->i32{self.row.v_GroupId}
    pub fn get_Id(&self)->i32{self.row.v_Id}
    pub fn get_Count(&self)->i32{self.row.v_Count}
}
struct DropKey_Key{
    k0:i32,
    k1:i32,
}
impl DropKey_Key{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("GroupId")?)?,
    k1:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct DropIndex_Key{keys:Vec<DropKey_Key>,index:R::Index,
}
impl DropIndex_Key{
    fn build(keys:Vec<DropKey_Key>)->ServerResult<Self>{
        let index=R::Index::build(keys.len(),true,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0); R::hash_part(&mut h,&keys[row].k1);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0&&keys[a].k1==keys[b].k1).map_err(R::data_error)?;
        Ok(Self{keys,index})
    }
    fn find(&self,k0:i32,k1:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0); R::hash_part(&mut h,&k1);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0&&self.keys[row].k1==k1)}
}
struct DropKey_Group_ByGroupId{
    k0:i32,
}
impl DropKey_Group_ByGroupId{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("GroupId")?)?,
})}} 
struct DropIndex_Group_ByGroupId{keys:Vec<DropKey_Group_ByGroupId>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl DropIndex_Group_ByGroupId{
    fn build(keys:Vec<DropKey_Group_ByGroupId>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct DropKey_Group_ById{
    k0:i32,
}
impl DropKey_Group_ById{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct DropIndex_Group_ById{keys:Vec<DropKey_Group_ById>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl DropIndex_Group_ById{
    fn build(keys:Vec<DropKey_Group_ById>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct DropStorage{rows:R::RowStorage<DropData>,key:DropIndex_Key,all:OnceLock<Arc<[usize]>>,
    group_ByGroupId:R::DerivedValue<DropIndex_Group_ByGroupId>,
    group_ById:R::DerivedValue<DropIndex_Group_ById>,
}
impl DropStorage{
    fn new(indexed:R::IndexedRows)->ServerResult<Self>{let mut keys=Vec::with_capacity(indexed.count());for i in 0..indexed.count(){keys.push(DropKey_Key::decode(indexed.key(i)).map_err(R::data_error)?);}Ok(Self{rows:R::RowStorage::new(indexed,DropData::decode),key:DropIndex_Key::build(keys)?,all:OnceLock::new(),
        group_ByGroupId:R::DerivedValue::new(),
        group_ById:R::DerivedValue::new(),
    })}
    fn ensure_ByGroupId(&self,cancellation:Option<&Cancellation>)->ServerResult<&DropIndex_Group_ByGroupId>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_ByGroupId.get()};self.group_ByGroupId.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){DropKey_Group_ByGroupId{k0:cached.v_GroupId}}else{DropKey_Group_ByGroupId::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((DropIndex_Group_ByGroupId::build(keys)?,retained))
        })
    }
    fn group_ByGroupId(&self,k0:i32,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_ByGroupId(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn ensure_ById(&self,cancellation:Option<&Cancellation>)->ServerResult<&DropIndex_Group_ById>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_ById.get()};self.group_ById.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){DropKey_Group_ById{k0:cached.v_Id}}else{DropKey_Group_ById::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((DropIndex_Group_ById::build(keys)?,retained))
        })
    }
    fn group_ById(&self,k0:i32,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_ById(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{cancellation.check()?;if self.all.get().is_some(){return Ok(())}for i in 0..self.rows.count(){self.rows.load(i,cancellation)?;}
        self.ensure_ByGroupId(Some(cancellation))?;
        self.ensure_ById(Some(cancellation))?;
        cancellation.check()?;let _=self.all.set(Arc::from((0..self.rows.count()).collect::<Vec<_>>().into_boxed_slice()));Ok(())
    }
}
pub struct HeroData{
    v_Id:i32,
    v_Name:String,
    v_Skill:i32,
    v_OtherSkill:i32,
    v_Mentor:i32,
}
impl HeroData{
    fn decode(value:&Json)->Result<Self>{Ok(Self{
        v_Id:R::scalar::<i32>(value.field("Id")?)?,
        v_Name:R::scalar::<String>(value.field("Name")?)?,
        v_Skill:R::scalar::<i32>(value.field("Skill")?)?,
        v_OtherSkill:R::scalar::<i32>(value.field("OtherSkill")?)?,
        v_Mentor:R::scalar::<i32>(value.field("Mentor")?)?,
    })}
    pub fn get_Id(&self)->i32{self.v_Id}
    pub fn get_Name(&self)->&str{&self.v_Name}
    pub fn get_SkillId(&self)->i32{self.v_Skill}
    pub fn get_OtherSkillId(&self)->i32{self.v_OtherSkill}
    pub fn get_MentorId(&self)->i32{self.v_Mentor}
}
pub type HeroRow=R::RowHandle<Snapshot,HeroData>;
impl R::RowHandle<Snapshot,HeroData>{
    pub fn get_Id(&self)->i32{self.row.v_Id}
    pub fn get_Name(&self)->&str{&self.row.v_Name}
    pub fn get_SkillId(&self)->i32{self.row.v_Skill}
    pub fn get_Skill(&self)->ServerResult<Option<SkillRow>>{self.snapshot.reference_Skill(self.row.v_Skill,None)}
    pub fn load_Skill(&self,cancellation:&Cancellation)->ServerResult<Option<SkillRow>>{self.snapshot.reference_Skill(self.row.v_Skill,Some(cancellation))}
    pub fn get_OtherSkillId(&self)->i32{self.row.v_OtherSkill}
    pub fn get_OtherSkill(&self)->ServerResult<Option<SkillRow>>{self.snapshot.reference_Skill(self.row.v_OtherSkill,None)}
    pub fn load_OtherSkill(&self,cancellation:&Cancellation)->ServerResult<Option<SkillRow>>{self.snapshot.reference_Skill(self.row.v_OtherSkill,Some(cancellation))}
    pub fn get_MentorId(&self)->i32{self.row.v_Mentor}
    pub fn get_Mentor(&self)->ServerResult<Option<HeroRow>>{self.snapshot.reference_Hero(self.row.v_Mentor,None)}
    pub fn load_Mentor(&self,cancellation:&Cancellation)->ServerResult<Option<HeroRow>>{self.snapshot.reference_Hero(self.row.v_Mentor,Some(cancellation))}
}
struct HeroKey_Key{
    k0:i32,
}
impl HeroKey_Key{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct HeroIndex_Key{keys:Vec<HeroKey_Key>,index:R::Index,
}
impl HeroIndex_Key{
    fn build(keys:Vec<HeroKey_Key>)->ServerResult<Self>{
        let index=R::Index::build(keys.len(),true,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        Ok(Self{keys,index})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct HeroKey_Group_ById{
    k0:i32,
}
impl HeroKey_Group_ById{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct HeroIndex_Group_ById{keys:Vec<HeroKey_Group_ById>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl HeroIndex_Group_ById{
    fn build(keys:Vec<HeroKey_Group_ById>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct HeroStorage{rows:R::RowStorage<HeroData>,key:HeroIndex_Key,all:OnceLock<Arc<[usize]>>,
    group_ById:R::DerivedValue<HeroIndex_Group_ById>,
}
impl HeroStorage{
    fn new(indexed:R::IndexedRows)->ServerResult<Self>{let mut keys=Vec::with_capacity(indexed.count());for i in 0..indexed.count(){keys.push(HeroKey_Key::decode(indexed.key(i)).map_err(R::data_error)?);}Ok(Self{rows:R::RowStorage::new(indexed,HeroData::decode),key:HeroIndex_Key::build(keys)?,all:OnceLock::new(),
        group_ById:R::DerivedValue::new(),
    })}
    fn ensure_ById(&self,cancellation:Option<&Cancellation>)->ServerResult<&HeroIndex_Group_ById>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_ById.get()};self.group_ById.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){HeroKey_Group_ById{k0:cached.v_Id}}else{HeroKey_Group_ById::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((HeroIndex_Group_ById::build(keys)?,retained))
        })
    }
    fn group_ById(&self,k0:i32,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_ById(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{cancellation.check()?;if self.all.get().is_some(){return Ok(())}for i in 0..self.rows.count(){self.rows.load(i,cancellation)?;}
        self.ensure_ById(Some(cancellation))?;
        cancellation.check()?;let _=self.all.set(Arc::from((0..self.rows.count()).collect::<Vec<_>>().into_boxed_slice()));Ok(())
    }
}
pub struct ItemCatalogData{
    v_Id:i32,
    v_Name:String,
    v_MaxStack:i32,
    v_Rarity:i32,
    v_Enabled:bool,
}
impl ItemCatalogData{
    fn decode(value:&Json)->Result<Self>{Ok(Self{
        v_Id:R::scalar::<i32>(value.field("Id")?)?,
        v_Name:R::scalar::<String>(value.field("Name")?)?,
        v_MaxStack:R::scalar::<i32>(value.field("MaxStack")?)?,
        v_Rarity:R::scalar::<i32>(value.field("Rarity")?)?,
        v_Enabled:R::scalar::<bool>(value.field("Enabled")?)?,
    })}
    pub fn get_Id(&self)->i32{self.v_Id}
    pub fn get_Name(&self)->&str{&self.v_Name}
    pub fn get_MaxStack(&self)->i32{self.v_MaxStack}
    pub fn get_Rarity(&self)->i32{self.v_Rarity}
    pub fn get_Enabled(&self)->bool{self.v_Enabled}
}
pub type ItemCatalogRow=R::RowHandle<Snapshot,ItemCatalogData>;
impl R::RowHandle<Snapshot,ItemCatalogData>{
    pub fn get_Id(&self)->i32{self.row.v_Id}
    pub fn get_Name(&self)->&str{&self.row.v_Name}
    pub fn get_MaxStack(&self)->i32{self.row.v_MaxStack}
    pub fn get_Rarity(&self)->i32{self.row.v_Rarity}
    pub fn get_Enabled(&self)->bool{self.row.v_Enabled}
}
struct ItemCatalogKey_Key{
    k0:i32,
}
impl ItemCatalogKey_Key{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct ItemCatalogIndex_Key{keys:Vec<ItemCatalogKey_Key>,index:R::Index,
}
impl ItemCatalogIndex_Key{
    fn build(keys:Vec<ItemCatalogKey_Key>)->ServerResult<Self>{
        let index=R::Index::build(keys.len(),true,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        Ok(Self{keys,index})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct ItemCatalogKey_Group_ById{
    k0:i32,
}
impl ItemCatalogKey_Group_ById{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct ItemCatalogIndex_Group_ById{keys:Vec<ItemCatalogKey_Group_ById>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl ItemCatalogIndex_Group_ById{
    fn build(keys:Vec<ItemCatalogKey_Group_ById>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct ItemCatalogStorage{rows:R::RowStorage<ItemCatalogData>,key:ItemCatalogIndex_Key,all:OnceLock<Arc<[usize]>>,
    group_ById:R::DerivedValue<ItemCatalogIndex_Group_ById>,
}
impl ItemCatalogStorage{
    fn new(indexed:R::IndexedRows)->ServerResult<Self>{let mut keys=Vec::with_capacity(indexed.count());for i in 0..indexed.count(){keys.push(ItemCatalogKey_Key::decode(indexed.key(i)).map_err(R::data_error)?);}Ok(Self{rows:R::RowStorage::new(indexed,ItemCatalogData::decode),key:ItemCatalogIndex_Key::build(keys)?,all:OnceLock::new(),
        group_ById:R::DerivedValue::new(),
    })}
    fn ensure_ById(&self,cancellation:Option<&Cancellation>)->ServerResult<&ItemCatalogIndex_Group_ById>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_ById.get()};self.group_ById.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){ItemCatalogKey_Group_ById{k0:cached.v_Id}}else{ItemCatalogKey_Group_ById::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((ItemCatalogIndex_Group_ById::build(keys)?,retained))
        })
    }
    fn group_ById(&self,k0:i32,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_ById(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{cancellation.check()?;if self.all.get().is_some(){return Ok(())}for i in 0..self.rows.count(){self.rows.load(i,cancellation)?;}
        self.ensure_ById(Some(cancellation))?;
        cancellation.check()?;let _=self.all.set(Arc::from((0..self.rows.count()).collect::<Vec<_>>().into_boxed_slice()));Ok(())
    }
}
pub struct QuestRewardsData{
    v_Id:i32,
    v_Name:String,
    v_Item:i32,
    v_Count:i32,
    v_RequiredLevel:i32,
    v_Enabled:bool,
}
impl QuestRewardsData{
    fn decode(value:&Json)->Result<Self>{Ok(Self{
        v_Id:R::scalar::<i32>(value.field("Id")?)?,
        v_Name:R::scalar::<String>(value.field("Name")?)?,
        v_Item:R::scalar::<i32>(value.field("Item")?)?,
        v_Count:R::scalar::<i32>(value.field("Count")?)?,
        v_RequiredLevel:R::scalar::<i32>(value.field("RequiredLevel")?)?,
        v_Enabled:R::scalar::<bool>(value.field("Enabled")?)?,
    })}
    pub fn get_Id(&self)->i32{self.v_Id}
    pub fn get_Name(&self)->&str{&self.v_Name}
    pub fn get_ItemId(&self)->i32{self.v_Item}
    pub fn get_Count(&self)->i32{self.v_Count}
    pub fn get_RequiredLevel(&self)->i32{self.v_RequiredLevel}
    pub fn get_Enabled(&self)->bool{self.v_Enabled}
}
pub type QuestRewardsRow=R::RowHandle<Snapshot,QuestRewardsData>;
impl R::RowHandle<Snapshot,QuestRewardsData>{
    pub fn get_Id(&self)->i32{self.row.v_Id}
    pub fn get_Name(&self)->&str{&self.row.v_Name}
    pub fn get_ItemId(&self)->i32{self.row.v_Item}
    pub fn get_Item(&self)->ServerResult<Option<ItemCatalogRow>>{self.snapshot.reference_ItemCatalog(self.row.v_Item,None)}
    pub fn load_Item(&self,cancellation:&Cancellation)->ServerResult<Option<ItemCatalogRow>>{self.snapshot.reference_ItemCatalog(self.row.v_Item,Some(cancellation))}
    pub fn get_Count(&self)->i32{self.row.v_Count}
    pub fn get_RequiredLevel(&self)->i32{self.row.v_RequiredLevel}
    pub fn get_Enabled(&self)->bool{self.row.v_Enabled}
}
struct QuestRewardsKey_Key{
    k0:i32,
}
impl QuestRewardsKey_Key{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct QuestRewardsIndex_Key{keys:Vec<QuestRewardsKey_Key>,index:R::Index,
}
impl QuestRewardsIndex_Key{
    fn build(keys:Vec<QuestRewardsKey_Key>)->ServerResult<Self>{
        let index=R::Index::build(keys.len(),true,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        Ok(Self{keys,index})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct QuestRewardsKey_Group_ById{
    k0:i32,
}
impl QuestRewardsKey_Group_ById{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct QuestRewardsIndex_Group_ById{keys:Vec<QuestRewardsKey_Group_ById>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl QuestRewardsIndex_Group_ById{
    fn build(keys:Vec<QuestRewardsKey_Group_ById>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct QuestRewardsStorage{rows:R::RowStorage<QuestRewardsData>,key:QuestRewardsIndex_Key,all:OnceLock<Arc<[usize]>>,
    group_ById:R::DerivedValue<QuestRewardsIndex_Group_ById>,
}
impl QuestRewardsStorage{
    fn new(indexed:R::IndexedRows)->ServerResult<Self>{let mut keys=Vec::with_capacity(indexed.count());for i in 0..indexed.count(){keys.push(QuestRewardsKey_Key::decode(indexed.key(i)).map_err(R::data_error)?);}Ok(Self{rows:R::RowStorage::new(indexed,QuestRewardsData::decode),key:QuestRewardsIndex_Key::build(keys)?,all:OnceLock::new(),
        group_ById:R::DerivedValue::new(),
    })}
    fn ensure_ById(&self,cancellation:Option<&Cancellation>)->ServerResult<&QuestRewardsIndex_Group_ById>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_ById.get()};self.group_ById.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){QuestRewardsKey_Group_ById{k0:cached.v_Id}}else{QuestRewardsKey_Group_ById::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((QuestRewardsIndex_Group_ById::build(keys)?,retained))
        })
    }
    fn group_ById(&self,k0:i32,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_ById(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{cancellation.check()?;if self.all.get().is_some(){return Ok(())}for i in 0..self.rows.count(){self.rows.load(i,cancellation)?;}
        self.ensure_ById(Some(cancellation))?;
        cancellation.check()?;let _=self.all.set(Arc::from((0..self.rows.count()).collect::<Vec<_>>().into_boxed_slice()));Ok(())
    }
}
pub struct ShopOffersData{
    v_Id:i32,
    v_Item:i32,
    v_Price:i32,
    v_DailyLimit:i32,
    v_Enabled:bool,
}
impl ShopOffersData{
    fn decode(value:&Json)->Result<Self>{Ok(Self{
        v_Id:R::scalar::<i32>(value.field("Id")?)?,
        v_Item:R::scalar::<i32>(value.field("Item")?)?,
        v_Price:R::scalar::<i32>(value.field("Price")?)?,
        v_DailyLimit:R::scalar::<i32>(value.field("DailyLimit")?)?,
        v_Enabled:R::scalar::<bool>(value.field("Enabled")?)?,
    })}
    pub fn get_Id(&self)->i32{self.v_Id}
    pub fn get_ItemId(&self)->i32{self.v_Item}
    pub fn get_Price(&self)->i32{self.v_Price}
    pub fn get_DailyLimit(&self)->i32{self.v_DailyLimit}
    pub fn get_Enabled(&self)->bool{self.v_Enabled}
}
pub type ShopOffersRow=R::RowHandle<Snapshot,ShopOffersData>;
impl R::RowHandle<Snapshot,ShopOffersData>{
    pub fn get_Id(&self)->i32{self.row.v_Id}
    pub fn get_ItemId(&self)->i32{self.row.v_Item}
    pub fn get_Item(&self)->ServerResult<Option<ItemCatalogRow>>{self.snapshot.reference_ItemCatalog(self.row.v_Item,None)}
    pub fn load_Item(&self,cancellation:&Cancellation)->ServerResult<Option<ItemCatalogRow>>{self.snapshot.reference_ItemCatalog(self.row.v_Item,Some(cancellation))}
    pub fn get_Price(&self)->i32{self.row.v_Price}
    pub fn get_DailyLimit(&self)->i32{self.row.v_DailyLimit}
    pub fn get_Enabled(&self)->bool{self.row.v_Enabled}
}
struct ShopOffersKey_Key{
    k0:i32,
}
impl ShopOffersKey_Key{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct ShopOffersIndex_Key{keys:Vec<ShopOffersKey_Key>,index:R::Index,
}
impl ShopOffersIndex_Key{
    fn build(keys:Vec<ShopOffersKey_Key>)->ServerResult<Self>{
        let index=R::Index::build(keys.len(),true,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        Ok(Self{keys,index})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct ShopOffersKey_Group_ById{
    k0:i32,
}
impl ShopOffersKey_Group_ById{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct ShopOffersIndex_Group_ById{keys:Vec<ShopOffersKey_Group_ById>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl ShopOffersIndex_Group_ById{
    fn build(keys:Vec<ShopOffersKey_Group_ById>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct ShopOffersStorage{rows:R::RowStorage<ShopOffersData>,key:ShopOffersIndex_Key,all:OnceLock<Arc<[usize]>>,
    group_ById:R::DerivedValue<ShopOffersIndex_Group_ById>,
}
impl ShopOffersStorage{
    fn new(indexed:R::IndexedRows)->ServerResult<Self>{let mut keys=Vec::with_capacity(indexed.count());for i in 0..indexed.count(){keys.push(ShopOffersKey_Key::decode(indexed.key(i)).map_err(R::data_error)?);}Ok(Self{rows:R::RowStorage::new(indexed,ShopOffersData::decode),key:ShopOffersIndex_Key::build(keys)?,all:OnceLock::new(),
        group_ById:R::DerivedValue::new(),
    })}
    fn ensure_ById(&self,cancellation:Option<&Cancellation>)->ServerResult<&ShopOffersIndex_Group_ById>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_ById.get()};self.group_ById.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){ShopOffersKey_Group_ById{k0:cached.v_Id}}else{ShopOffersKey_Group_ById::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((ShopOffersIndex_Group_ById::build(keys)?,retained))
        })
    }
    fn group_ById(&self,k0:i32,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_ById(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{cancellation.check()?;if self.all.get().is_some(){return Ok(())}for i in 0..self.rows.count(){self.rows.load(i,cancellation)?;}
        self.ensure_ById(Some(cancellation))?;
        cancellation.check()?;let _=self.all.set(Arc::from((0..self.rows.count()).collect::<Vec<_>>().into_boxed_slice()));Ok(())
    }
}
pub struct SkillData{
    v_Id:i32,
    v_Name:String,
    v_Hero:i32,
}
impl SkillData{
    fn decode(value:&Json)->Result<Self>{Ok(Self{
        v_Id:R::scalar::<i32>(value.field("Id")?)?,
        v_Name:R::scalar::<String>(value.field("Name")?)?,
        v_Hero:R::scalar::<i32>(value.field("Hero")?)?,
    })}
    pub fn get_Id(&self)->i32{self.v_Id}
    pub fn get_Name(&self)->&str{&self.v_Name}
    pub fn get_HeroId(&self)->i32{self.v_Hero}
}
pub type SkillRow=R::RowHandle<Snapshot,SkillData>;
impl R::RowHandle<Snapshot,SkillData>{
    pub fn get_Id(&self)->i32{self.row.v_Id}
    pub fn get_Name(&self)->&str{&self.row.v_Name}
    pub fn get_HeroId(&self)->i32{self.row.v_Hero}
    pub fn get_Hero(&self)->ServerResult<Option<HeroRow>>{self.snapshot.reference_Hero(self.row.v_Hero,None)}
    pub fn load_Hero(&self,cancellation:&Cancellation)->ServerResult<Option<HeroRow>>{self.snapshot.reference_Hero(self.row.v_Hero,Some(cancellation))}
}
struct SkillKey_Key{
    k0:i32,
}
impl SkillKey_Key{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct SkillIndex_Key{keys:Vec<SkillKey_Key>,index:R::Index,
}
impl SkillIndex_Key{
    fn build(keys:Vec<SkillKey_Key>)->ServerResult<Self>{
        let index=R::Index::build(keys.len(),true,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        Ok(Self{keys,index})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct SkillKey_Group_ById{
    k0:i32,
}
impl SkillKey_Group_ById{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Id")?)?,
})}} 
struct SkillIndex_Group_ById{keys:Vec<SkillKey_Group_ById>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl SkillIndex_Group_ById{
    fn build(keys:Vec<SkillKey_Group_ById>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct SkillStorage{rows:R::RowStorage<SkillData>,key:SkillIndex_Key,all:OnceLock<Arc<[usize]>>,
    group_ById:R::DerivedValue<SkillIndex_Group_ById>,
}
impl SkillStorage{
    fn new(indexed:R::IndexedRows)->ServerResult<Self>{let mut keys=Vec::with_capacity(indexed.count());for i in 0..indexed.count(){keys.push(SkillKey_Key::decode(indexed.key(i)).map_err(R::data_error)?);}Ok(Self{rows:R::RowStorage::new(indexed,SkillData::decode),key:SkillIndex_Key::build(keys)?,all:OnceLock::new(),
        group_ById:R::DerivedValue::new(),
    })}
    fn ensure_ById(&self,cancellation:Option<&Cancellation>)->ServerResult<&SkillIndex_Group_ById>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_ById.get()};self.group_ById.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){SkillKey_Group_ById{k0:cached.v_Id}}else{SkillKey_Group_ById::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((SkillIndex_Group_ById::build(keys)?,retained))
        })
    }
    fn group_ById(&self,k0:i32,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_ById(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{cancellation.check()?;if self.all.get().is_some(){return Ok(())}for i in 0..self.rows.count(){self.rows.load(i,cancellation)?;}
        self.ensure_ById(Some(cancellation))?;
        cancellation.check()?;let _=self.all.set(Arc::from((0..self.rows.count()).collect::<Vec<_>>().into_boxed_slice()));Ok(())
    }
}
pub struct SkillLevelData{
    v_SkillId:i32,
    v_Name:String,
    v_Level:i32,
    v_Quality:i32,
    v_Damage:i32,
}
impl SkillLevelData{
    fn decode(value:&Json)->Result<Self>{Ok(Self{
        v_SkillId:R::scalar::<i32>(value.field("SkillId")?)?,
        v_Name:R::scalar::<String>(value.field("Name")?)?,
        v_Level:R::scalar::<i32>(value.field("Level")?)?,
        v_Quality:R::scalar::<i32>(value.field("Quality")?)?,
        v_Damage:R::scalar::<i32>(value.field("Damage")?)?,
    })}
    pub fn get_SkillId(&self)->i32{self.v_SkillId}
    pub fn get_Name(&self)->&str{&self.v_Name}
    pub fn get_Level(&self)->i32{self.v_Level}
    pub fn get_Quality(&self)->i32{self.v_Quality}
    pub fn get_Damage(&self)->i32{self.v_Damage}
}
pub type SkillLevelRow=R::RowHandle<Snapshot,SkillLevelData>;
impl R::RowHandle<Snapshot,SkillLevelData>{
    pub fn get_SkillId(&self)->i32{self.row.v_SkillId}
    pub fn get_Name(&self)->&str{&self.row.v_Name}
    pub fn get_Level(&self)->i32{self.row.v_Level}
    pub fn get_Quality(&self)->i32{self.row.v_Quality}
    pub fn get_Damage(&self)->i32{self.row.v_Damage}
}
struct SkillLevelKey_Key{
    k0:i32,
    k1:i32,
    k2:i32,
}
impl SkillLevelKey_Key{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("SkillId")?)?,
    k1:R::scalar::<i32>(value.field("Level")?)?,
    k2:R::scalar::<i32>(value.field("Quality")?)?,
})}} 
struct SkillLevelIndex_Key{keys:Vec<SkillLevelKey_Key>,index:R::Index,
}
impl SkillLevelIndex_Key{
    fn build(keys:Vec<SkillLevelKey_Key>)->ServerResult<Self>{
        let index=R::Index::build(keys.len(),true,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0); R::hash_part(&mut h,&keys[row].k1); R::hash_part(&mut h,&keys[row].k2);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0&&keys[a].k1==keys[b].k1&&keys[a].k2==keys[b].k2).map_err(R::data_error)?;
        Ok(Self{keys,index})
    }
    fn find(&self,k0:i32,k1:i32,k2:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0); R::hash_part(&mut h,&k1); R::hash_part(&mut h,&k2);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0&&self.keys[row].k1==k1&&self.keys[row].k2==k2)}
}
struct SkillLevelKey_Group_ByLevel{
    k0:i32,
}
impl SkillLevelKey_Group_ByLevel{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Level")?)?,
})}} 
struct SkillLevelIndex_Group_ByLevel{keys:Vec<SkillLevelKey_Group_ByLevel>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl SkillLevelIndex_Group_ByLevel{
    fn build(keys:Vec<SkillLevelKey_Group_ByLevel>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct SkillLevelKey_Group_ByQuality{
    k0:i32,
}
impl SkillLevelKey_Group_ByQuality{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("Quality")?)?,
})}} 
struct SkillLevelIndex_Group_ByQuality{keys:Vec<SkillLevelKey_Group_ByQuality>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl SkillLevelIndex_Group_ByQuality{
    fn build(keys:Vec<SkillLevelKey_Group_ByQuality>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct SkillLevelKey_Group_BySkillId{
    k0:i32,
}
impl SkillLevelKey_Group_BySkillId{fn decode(value:&Json)->Result<Self>{Ok(Self{
    k0:R::scalar::<i32>(value.field("SkillId")?)?,
})}} 
struct SkillLevelIndex_Group_BySkillId{keys:Vec<SkillLevelKey_Group_BySkillId>,index:R::Index,
    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,
}
impl SkillLevelIndex_Group_BySkillId{
    fn build(keys:Vec<SkillLevelKey_Group_BySkillId>)->ServerResult<Self>{
        let mut index=R::Index::build(keys.len(),false,|row|{let mut h=R::new_hash();R::hash_part(&mut h,&keys[row].k0);R::finish_hash(h)},|a,b|keys[a].k0==keys[b].k0).map_err(R::data_error)?;
        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})
    }
    fn find(&self,k0:i32)->Option<usize>{let mut h=R::new_hash();R::hash_part(&mut h,&k0);self.index.find(R::finish_hash(h),|row|self.keys[row].k0==k0)}
}
struct SkillLevelStorage{rows:R::RowStorage<SkillLevelData>,key:SkillLevelIndex_Key,all:OnceLock<Arc<[usize]>>,
    group_ByLevel:R::DerivedValue<SkillLevelIndex_Group_ByLevel>,
    group_ByQuality:R::DerivedValue<SkillLevelIndex_Group_ByQuality>,
    group_BySkillId:R::DerivedValue<SkillLevelIndex_Group_BySkillId>,
}
impl SkillLevelStorage{
    fn new(indexed:R::IndexedRows)->ServerResult<Self>{let mut keys=Vec::with_capacity(indexed.count());for i in 0..indexed.count(){keys.push(SkillLevelKey_Key::decode(indexed.key(i)).map_err(R::data_error)?);}Ok(Self{rows:R::RowStorage::new(indexed,SkillLevelData::decode),key:SkillLevelIndex_Key::build(keys)?,all:OnceLock::new(),
        group_ByLevel:R::DerivedValue::new(),
        group_ByQuality:R::DerivedValue::new(),
        group_BySkillId:R::DerivedValue::new(),
    })}
    fn ensure_ByLevel(&self,cancellation:Option<&Cancellation>)->ServerResult<&SkillLevelIndex_Group_ByLevel>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_ByLevel.get()};self.group_ByLevel.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){SkillLevelKey_Group_ByLevel{k0:cached.v_Level}}else{SkillLevelKey_Group_ByLevel::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((SkillLevelIndex_Group_ByLevel::build(keys)?,retained))
        })
    }
    fn group_ByLevel(&self,k0:i32,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_ByLevel(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn ensure_ByQuality(&self,cancellation:Option<&Cancellation>)->ServerResult<&SkillLevelIndex_Group_ByQuality>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_ByQuality.get()};self.group_ByQuality.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){SkillLevelKey_Group_ByQuality{k0:cached.v_Quality}}else{SkillLevelKey_Group_ByQuality::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((SkillLevelIndex_Group_ByQuality::build(keys)?,retained))
        })
    }
    fn group_ByQuality(&self,k0:i32,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_ByQuality(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn ensure_BySkillId(&self,cancellation:Option<&Cancellation>)->ServerResult<&SkillLevelIndex_Group_BySkillId>{
        let cancellation=match cancellation{Some(token)=>token,None=>return self.group_BySkillId.get()};self.group_BySkillId.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes(1),||{
        let mut keys=Vec::with_capacity(self.rows.count());let retained=256+512*self.rows.count() as u64*1;for i in 0..self.rows.count(){let key=if let Some(cached)=self.rows.cached(i){SkillLevelKey_Group_BySkillId{k0:cached.v_SkillId}}else{SkillLevelKey_Group_BySkillId::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?};keys.push(key);}Ok((SkillLevelIndex_Group_BySkillId::build(keys)?,retained))
        })
    }
    fn group_BySkillId(&self,k0:i32,cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{let index=self.ensure_BySkillId(cancellation)?;let slot=match index.find(k0){Some(slot)=>slot,None=>return Ok(index.empty.clone())};
        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){for ordinal in index.positions[slot].iter(){if let Some(token)=cancellation{self.rows.load(*ordinal,token)?;}else{self.rows.get(*ordinal)?;}}index.ready[slot].store(true,Ordering::Release);}Ok(index.positions[slot].clone())
    }
    fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{cancellation.check()?;if self.all.get().is_some(){return Ok(())}for i in 0..self.rows.count(){self.rows.load(i,cancellation)?;}
        self.ensure_ByLevel(Some(cancellation))?;
        self.ensure_ByQuality(Some(cancellation))?;
        self.ensure_BySkillId(Some(cancellation))?;
        cancellation.check()?;let _=self.all.set(Arc::from((0..self.rows.count()).collect::<Vec<_>>().into_boxed_slice()));Ok(())
    }
}
pub struct Warmup{table:&'static str,action:Box<dyn Fn(&Arc<Snapshot>,&Cancellation)->ServerResult<()>+Send+Sync>}
impl Warmup{pub fn table(&self)->&'static str{self.table}}
pub struct Snapshot{version:String,manifest_digest:String,source_namespace:String,load_plan_fingerprint:String,budget:Arc<R::LoadBudget>,selection:R::SelectionPlan,mode:LoadMode,
    table_AllTypes:Option<AllTypesStorage>,
    table_AllTypes10k:Option<AllTypes10kStorage>,
    table_Drop:Option<DropStorage>,
    table_Hero:Option<HeroStorage>,
    table_ItemCatalog:Option<ItemCatalogStorage>,
    table_QuestRewards:Option<QuestRewardsStorage>,
    table_ShopOffers:Option<ShopOffersStorage>,
    table_Skill:Option<SkillStorage>,
    table_SkillLevel:Option<SkillLevelStorage>,
}
pub type Manager=R::ServerManager<Snapshot>;
impl Snapshot{
    pub fn version(&self)->&str{&self.version}
    pub fn selection(&self)->&R::SelectionPlan{&self.selection}
    pub fn mode(&self)->LoadMode{self.mode}
    pub fn manager()->Manager{Manager::with_policy(R::ManagerPolicy::new().with_identity(|snapshot:&Self|R::SnapshotIdentity::new(snapshot.source_namespace(),snapshot.version(),snapshot.manifest_digest())))}
    pub fn manifest_digest(&self)->&str{&self.manifest_digest}
    pub fn source_namespace(&self)->&str{&self.source_namespace}
    pub fn catalog_fingerprint(&self)->&str{self.selection.catalog_fingerprint()}
    pub fn load_plan_fingerprint(&self)->&str{&self.load_plan_fingerprint}
    pub fn diagnostics(&self)->ServerResult<R::SnapshotDiagnostics>{self.budget.diagnostics()}
    pub fn load(root:&Path,plan:&LoadPlan,cancellation:&Cancellation)->ServerResult<Arc<Self>>{
        let source=R::FileConfigSource::new(root);let settings=R::LoadSettings::new(plan.clone()).with_source_namespace(source.source_namespace());Self::load_source(&source,&settings,cancellation)
    }
    pub fn load_source(source:&dyn R::ConfigSource,settings:&R::LoadSettings,cancellation:&Cancellation)->ServerResult<Arc<Self>>{
        let plan=settings.plan();let catalog=super::server::Snapshot::catalog()?;let requested=match &plan.tables{Some(tables)=>tables.iter().map(String::as_str).collect::<Vec<_>>(),None=>catalog.tables().iter().map(R::TableDescriptor::name).collect()};
        let selection=catalog.select(&requested)?;for name in &plan.warmup_tables{if !selection.tables().contains(name){return Err(ConfigError::new(ErrorCode::InvalidPlan,"Warmup outside selection"))}}
        cancellation.check()?;let bundle=R::SourceBundle::open(source,settings,cancellation)?;bundle.validate_catalog(catalog)?;let _selected=selection.tables().iter().map(String::as_str).collect::<std::collections::HashSet<_>>();
        cancellation.check()?;let table_AllTypes=if _selected.contains("AllTypes"){Some(AllTypesStorage::new(R::IndexedRows::open(&bundle,"{\"name\":\"AllTypes\",\"hash\":\"E8017397D24CBD0FDA2008AFD4F2BA2BA68619A29F4D4D79F7973F06BC8360B1\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"1=常规值；2=边界值；3=默认值。重复 Id 的多行组成同一条配置。\"},{\"name\":\"ByteValue\",\"type\":\"System.Byte\",\"description\":\"基础整数；标量\"},{\"name\":\"ShortValue\",\"type\":\"System.Int16\",\"description\":\"基础整数；标量\"},{\"name\":\"IntValue\",\"type\":\"System.Int32\",\"description\":\"基础整数；标量\"},{\"name\":\"LongValue\",\"type\":\"System.Int64\",\"description\":\"基础整数；标量；文本存储，保留完整精度\"},{\"name\":\"FloatValue\",\"type\":\"System.Single\",\"description\":\"基础浮点；标量\"},{\"name\":\"DoubleValue\",\"type\":\"System.Double\",\"description\":\"基础浮点；标量\"},{\"name\":\"BoolValue\",\"type\":\"System.Boolean\",\"description\":\"基础布尔；标量\"},{\"name\":\"StringValue\",\"type\":\"System.String\",\"description\":\"基础字符串；标量\"},{\"name\":\"SByteValue\",\"type\":\"System.SByte\",\"description\":\"基础整数；标量\"},{\"name\":\"UShortValue\",\"type\":\"System.UInt16\",\"description\":\"基础整数；标量\"},{\"name\":\"UIntValue\",\"type\":\"System.UInt32\",\"description\":\"基础整数；标量\"},{\"name\":\"ULongValue\",\"type\":\"System.UInt64\",\"description\":\"基础整数；标量；文本存储，保留完整精度\"},{\"name\":\"DecimalValue\",\"type\":\"System.Decimal\",\"description\":\"精确十进制；标量；文本存储，保留完整精度\"},{\"name\":\"QualityValue\",\"type\":\"Game.Config.Quality\",\"description\":\"枚举；标量\"},{\"name\":\"Vector2Value\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"二维向量；标量\"},{\"name\":\"Vector3Value\",\"type\":\"ZrConfig.Types.Vector3\",\"description\":\"三维向量；标量\"},{\"name\":\"KvValue\",\"type\":\"ZrConfig.Types.Kv\",\"description\":\"整数键值对；标量\"},{\"name\":\"MoneyValue\",\"type\":\"Game.Config.Money\",\"description\":\"外部特殊标量转换器；标量\"},{\"name\":\"ByteArray\",\"type\":\"System.Byte[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"ShortArray\",\"type\":\"System.Int16[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"IntArray\",\"type\":\"System.Int32[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"LongArray\",\"type\":\"System.Int64[]\",\"description\":\"基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"FloatArray\",\"type\":\"System.Single[]\",\"description\":\"基础浮点；一维数组；元素用分号分隔\"},{\"name\":\"DoubleArray\",\"type\":\"System.Double[]\",\"description\":\"基础浮点；一维数组；元素用分号分隔\"},{\"name\":\"BoolArray\",\"type\":\"System.Boolean[]\",\"description\":\"基础布尔；一维数组；元素用分号分隔\"},{\"name\":\"StringArray\",\"type\":\"System.String[]\",\"description\":\"基础字符串；一维数组；元素用分号分隔\"},{\"name\":\"SByteArray\",\"type\":\"System.SByte[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"UShortArray\",\"type\":\"System.UInt16[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"UIntArray\",\"type\":\"System.UInt32[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"ULongArray\",\"type\":\"System.UInt64[]\",\"description\":\"基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"DecimalArray\",\"type\":\"System.Decimal[]\",\"description\":\"精确十进制；一维数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"QualityArray\",\"type\":\"Game.Config.Quality[]\",\"description\":\"枚举；一维数组；元素用分号分隔\"},{\"name\":\"Vector2Array\",\"type\":\"ZrConfig.Types.Vector2[]\",\"description\":\"二维向量；一维数组；元素用分号分隔\"},{\"name\":\"Vector3Array\",\"type\":\"ZrConfig.Types.Vector3[]\",\"description\":\"三维向量；一维数组；元素用分号分隔\"},{\"name\":\"KvArray\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"整数键值对；一维数组；元素用分号分隔\"},{\"name\":\"MoneyArray\",\"type\":\"Game.Config.Money[]\",\"description\":\"外部特殊标量转换器；一维数组；元素用分号分隔\"},{\"name\":\"ByteArray2D\",\"type\":\"System.Byte[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"ShortArray2D\",\"type\":\"System.Int16[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"IntArray2D\",\"type\":\"System.Int32[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"LongArray2D\",\"type\":\"System.Int64[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"FloatArray2D\",\"type\":\"System.Single[][]\",\"description\":\"基础浮点；二维交错数组；元素用分号分隔\"},{\"name\":\"DoubleArray2D\",\"type\":\"System.Double[][]\",\"description\":\"基础浮点；二维交错数组；元素用分号分隔\"},{\"name\":\"BoolArray2D\",\"type\":\"System.Boolean[][]\",\"description\":\"基础布尔；二维交错数组；元素用分号分隔\"},{\"name\":\"StringArray2D\",\"type\":\"System.String[][]\",\"description\":\"基础字符串；二维交错数组；元素用分号分隔\"},{\"name\":\"SByteArray2D\",\"type\":\"System.SByte[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"UShortArray2D\",\"type\":\"System.UInt16[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"UIntArray2D\",\"type\":\"System.UInt32[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"ULongArray2D\",\"type\":\"System.UInt64[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"DecimalArray2D\",\"type\":\"System.Decimal[][]\",\"description\":\"精确十进制；二维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"QualityArray2D\",\"type\":\"Game.Config.Quality[][]\",\"description\":\"枚举；二维交错数组；元素用分号分隔\"},{\"name\":\"Vector2Array2D\",\"type\":\"ZrConfig.Types.Vector2[][]\",\"description\":\"二维向量；二维交错数组；元素用分号分隔\"},{\"name\":\"Vector3Array2D\",\"type\":\"ZrConfig.Types.Vector3[][]\",\"description\":\"三维向量；二维交错数组；元素用分号分隔\"},{\"name\":\"KvArray2D\",\"type\":\"ZrConfig.Types.Kv[][]\",\"description\":\"整数键值对；二维交错数组；元素用分号分隔\"},{\"name\":\"MoneyArray2D\",\"type\":\"Game.Config.Money[][]\",\"description\":\"外部特殊标量转换器；二维交错数组；元素用分号分隔\"},{\"name\":\"ByteArray3D\",\"type\":\"System.Byte[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"ShortArray3D\",\"type\":\"System.Int16[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"IntArray3D\",\"type\":\"System.Int32[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"LongArray3D\",\"type\":\"System.Int64[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"FloatArray3D\",\"type\":\"System.Single[][][]\",\"description\":\"基础浮点；三维交错数组；元素用分号分隔\"},{\"name\":\"DoubleArray3D\",\"type\":\"System.Double[][][]\",\"description\":\"基础浮点；三维交错数组；元素用分号分隔\"},{\"name\":\"BoolArray3D\",\"type\":\"System.Boolean[][][]\",\"description\":\"基础布尔；三维交错数组；元素用分号分隔\"},{\"name\":\"StringArray3D\",\"type\":\"System.String[][][]\",\"description\":\"基础字符串；三维交错数组；元素用分号分隔\"},{\"name\":\"SByteArray3D\",\"type\":\"System.SByte[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"UShortArray3D\",\"type\":\"System.UInt16[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"UIntArray3D\",\"type\":\"System.UInt32[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"ULongArray3D\",\"type\":\"System.UInt64[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"DecimalArray3D\",\"type\":\"System.Decimal[][][]\",\"description\":\"精确十进制；三维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"QualityArray3D\",\"type\":\"Game.Config.Quality[][][]\",\"description\":\"枚举；三维交错数组；元素用分号分隔\"},{\"name\":\"Vector2Array3D\",\"type\":\"ZrConfig.Types.Vector2[][][]\",\"description\":\"二维向量；三维交错数组；元素用分号分隔\"},{\"name\":\"Vector3Array3D\",\"type\":\"ZrConfig.Types.Vector3[][][]\",\"description\":\"三维向量；三维交错数组；元素用分号分隔\"},{\"name\":\"KvArray3D\",\"type\":\"ZrConfig.Types.Kv[][][]\",\"description\":\"整数键值对；三维交错数组；元素用分号分隔\"},{\"name\":\"MoneyArray3D\",\"type\":\"Game.Config.Money[][][]\",\"description\":\"外部特殊标量转换器；三维交错数组；元素用分号分隔\"},{\"name\":\"Settings\",\"type\":\"Game.Config.BattleSettings\",\"description\":\"最多目标数\"},{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"结构体最小值\"},{\"name\":\"Options\",\"type\":\"Game.Config.SampleOptions\",\"description\":\"类的可写属性\"},{\"name\":\"Action\",\"type\":\"Game.Config.BattleAction\",\"description\":\"单个多态对象：选择 Damage / AddBuff\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"动作顺序：同一 Id 从 1 开始；一行一个动作\"},{\"name\":\"Ranges\",\"type\":\"Game.Config.ConfigRange[]\",\"description\":\"普通结构体数组也能多行；独立排序\"},{\"name\":\"OptionsList\",\"type\":\"Game.Config.SampleOptions[]\",\"description\":\"普通 class 数组，每行一个对象\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"父动作组编号；同一个编号的续行归入同一组\"},{\"name\":\"Phases\",\"type\":\"Game.Config.BattlePhase[]\",\"description\":\"阶段编号；续行重复填写\"},{\"name\":\"OptionalRange\",\"type\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"description\":\"可空结构体；整组空白表示 null\"},{\"name\":\"OptionalInt\",\"type\":\"System.Nullable`1<System.Int32>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalUInt\",\"type\":\"System.Nullable`1<System.UInt32>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalULong\",\"type\":\"System.Nullable`1<System.UInt64>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalDecimal\",\"type\":\"System.Nullable`1<System.Decimal>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalBool\",\"type\":\"System.Nullable`1<System.Boolean>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalQuality\",\"type\":\"System.Nullable`1<Game.Config.Quality>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalMoney\",\"type\":\"System.Nullable`1<Game.Config.Money>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalValues\",\"type\":\"System.Nullable`1<System.Int32>[]\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"StatMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"PriceMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"QualityMap\",\"type\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"ListMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"IntMatrix\",\"type\":\"System.Int32[,]\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"DecimalCube\",\"type\":\"System.Decimal[,,]\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalMatrix\",\"type\":\"System.Nullable`1<System.UInt32>[,]\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"RawJsonValue\",\"type\":\"ZrConfig.Types.RawJson\",\"description\":\"原始 JSON；标量；这是 JSON 类型本身的样例\"},{\"name\":\"RawJsonArray\",\"type\":\"ZrConfig.Types.RawJson[]\",\"description\":\"原始 JSON；一维数组；这是 JSON 类型本身的样例\"},{\"name\":\"RawJsonArray2D\",\"type\":\"ZrConfig.Types.RawJson[][]\",\"description\":\"原始 JSON；二维交错数组；这是 JSON 类型本身的样例\"},{\"name\":\"RawJsonArray3D\",\"type\":\"ZrConfig.Types.RawJson[][][]\",\"description\":\"原始 JSON；三维交错数组；这是 JSON 类型本身的样例\"},{\"name\":\"JsonIntArray\",\"type\":\"System.Int32[]\",\"description\":\"基础整数；一维数组；json<T> 输入格式示例，用逗号分隔\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]},{\"name\":\"ByQuality\",\"fields\":[\"QualityValue\"]}],\"types\":[{\"id\":\"Game.Config.ActionGroup\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ActionGroup[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ActionGroup\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.AddBuff\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"BuffId\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Stacks\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"enumValues\":[],\"abstract\":true,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattleAction\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattlePhase\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleSettings\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"MaxTargets\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Interruptible\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Min\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Max\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Damage\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Ratio\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"CanCrit\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money\",\"kind\":\"converter\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Cents\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality\",\"kind\":\"enum\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[\"0\",\"1\",\"2\"],\"abstract\":false,\"baseType\":null,\"enumMembers\":{\"None\":\"0\",\"Rare\":\"1\",\"Legendary\":\"2\"},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"\"},{\"name\":\"Rewards\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"\"},{\"name\":\"Offset\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"\"},{\"name\":\"Label\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Quality\",\"type\":\"Game.Config.Quality\",\"description\":\"\"},{\"name\":\"Levels\",\"type\":\"System.Int32[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.SampleOptions\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte\",\"kind\":\"primitive\",\"primitive\":\"uint8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":\"Game.Config.Quality\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Nullable`1<System.Decimal>\",\"key\":\"System.Int32\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32[]\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal\",\"kind\":\"primitive\",\"primitive\":\"decimal\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[,,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":3,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double\",\"kind\":\"primitive\",\"primitive\":\"float64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16\",\"kind\":\"primitive\",\"primitive\":\"int16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64\",\"kind\":\"primitive\",\"primitive\":\"int64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Money>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Quality>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Boolean>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Decimal>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Nullable`1<System.Int32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Nullable`1<System.UInt32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt64>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte\",\"kind\":\"primitive\",\"primitive\":\"int8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single\",\"kind\":\"primitive\",\"primitive\":\"float32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16\",\"kind\":\"primitive\",\"primitive\":\"uint16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32\",\"kind\":\"primitive\",\"primitive\":\"uint32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64\",\"kind\":\"primitive\",\"primitive\":\"uint64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv\",\"kind\":\"kv\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Key\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Value\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson\",\"kind\":\"rawjson\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2\",\"kind\":\"vector2\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3\",\"kind\":\"vector3\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Z\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"dependencies\":[]}",plan.prefer_json)?)?)}else{None};
        cancellation.check()?;let table_AllTypes10k=if _selected.contains("AllTypes10k"){Some(AllTypes10kStorage::new(R::IndexedRows::open(&bundle,"{\"name\":\"AllTypes10k\",\"hash\":\"34B94C4229D85CC8B37832A203504B7E6EF4447307ADB831AB5BD01628EA7432\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"唯一主键：1～10000\"},{\"name\":\"ByteValue\",\"type\":\"System.Byte\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortValue\",\"type\":\"System.Int16\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntValue\",\"type\":\"System.Int32\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongValue\",\"type\":\"System.Int64\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatValue\",\"type\":\"System.Single\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleValue\",\"type\":\"System.Double\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolValue\",\"type\":\"System.Boolean\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringValue\",\"type\":\"System.String\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteValue\",\"type\":\"System.SByte\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortValue\",\"type\":\"System.UInt16\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntValue\",\"type\":\"System.UInt32\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongValue\",\"type\":\"System.UInt64\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalValue\",\"type\":\"System.Decimal\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityValue\",\"type\":\"Game.Config.Quality\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Value\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Value\",\"type\":\"ZrConfig.Types.Vector3\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvValue\",\"type\":\"ZrConfig.Types.Kv\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyValue\",\"type\":\"Game.Config.Money\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ByteArray\",\"type\":\"System.Byte[]\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortArray\",\"type\":\"System.Int16[]\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntArray\",\"type\":\"System.Int32[]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongArray\",\"type\":\"System.Int64[]\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatArray\",\"type\":\"System.Single[]\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleArray\",\"type\":\"System.Double[]\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolArray\",\"type\":\"System.Boolean[]\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringArray\",\"type\":\"System.String[]\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteArray\",\"type\":\"System.SByte[]\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortArray\",\"type\":\"System.UInt16[]\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntArray\",\"type\":\"System.UInt32[]\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongArray\",\"type\":\"System.UInt64[]\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalArray\",\"type\":\"System.Decimal[]\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityArray\",\"type\":\"Game.Config.Quality[]\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Array\",\"type\":\"ZrConfig.Types.Vector2[]\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Array\",\"type\":\"ZrConfig.Types.Vector3[]\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvArray\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyArray\",\"type\":\"Game.Config.Money[]\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ByteArray2D\",\"type\":\"System.Byte[][]\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortArray2D\",\"type\":\"System.Int16[][]\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntArray2D\",\"type\":\"System.Int32[][]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongArray2D\",\"type\":\"System.Int64[][]\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatArray2D\",\"type\":\"System.Single[][]\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleArray2D\",\"type\":\"System.Double[][]\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolArray2D\",\"type\":\"System.Boolean[][]\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringArray2D\",\"type\":\"System.String[][]\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteArray2D\",\"type\":\"System.SByte[][]\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortArray2D\",\"type\":\"System.UInt16[][]\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntArray2D\",\"type\":\"System.UInt32[][]\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongArray2D\",\"type\":\"System.UInt64[][]\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalArray2D\",\"type\":\"System.Decimal[][]\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityArray2D\",\"type\":\"Game.Config.Quality[][]\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Array2D\",\"type\":\"ZrConfig.Types.Vector2[][]\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Array2D\",\"type\":\"ZrConfig.Types.Vector3[][]\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvArray2D\",\"type\":\"ZrConfig.Types.Kv[][]\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyArray2D\",\"type\":\"Game.Config.Money[][]\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ByteArray3D\",\"type\":\"System.Byte[][][]\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortArray3D\",\"type\":\"System.Int16[][][]\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntArray3D\",\"type\":\"System.Int32[][][]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongArray3D\",\"type\":\"System.Int64[][][]\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatArray3D\",\"type\":\"System.Single[][][]\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleArray3D\",\"type\":\"System.Double[][][]\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolArray3D\",\"type\":\"System.Boolean[][][]\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringArray3D\",\"type\":\"System.String[][][]\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteArray3D\",\"type\":\"System.SByte[][][]\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortArray3D\",\"type\":\"System.UInt16[][][]\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntArray3D\",\"type\":\"System.UInt32[][][]\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongArray3D\",\"type\":\"System.UInt64[][][]\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalArray3D\",\"type\":\"System.Decimal[][][]\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityArray3D\",\"type\":\"Game.Config.Quality[][][]\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Array3D\",\"type\":\"ZrConfig.Types.Vector2[][][]\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Array3D\",\"type\":\"ZrConfig.Types.Vector3[][][]\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvArray3D\",\"type\":\"ZrConfig.Types.Kv[][][]\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyArray3D\",\"type\":\"Game.Config.Money[][][]\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Settings\",\"type\":\"Game.Config.BattleSettings\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Options\",\"type\":\"Game.Config.SampleOptions\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Action\",\"type\":\"Game.Config.BattleAction\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Ranges\",\"type\":\"Game.Config.ConfigRange[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"OptionsList\",\"type\":\"Game.Config.SampleOptions[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Phases\",\"type\":\"Game.Config.BattlePhase[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"OptionalRange\",\"type\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"OptionalInt\",\"type\":\"System.Nullable`1<System.Int32>\",\"description\":\"NullableInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalUInt\",\"type\":\"System.Nullable`1<System.UInt32>\",\"description\":\"NullableUInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalULong\",\"type\":\"System.Nullable`1<System.UInt64>\",\"description\":\"NullableULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalDecimal\",\"type\":\"System.Nullable`1<System.Decimal>\",\"description\":\"NullableDecimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalBool\",\"type\":\"System.Nullable`1<System.Boolean>\",\"description\":\"NullableBool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalQuality\",\"type\":\"System.Nullable`1<Game.Config.Quality>\",\"description\":\"NullableQuality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalMoney\",\"type\":\"System.Nullable`1<Game.Config.Money>\",\"description\":\"NullableMoney；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalValues\",\"type\":\"System.Nullable`1<System.Int32>[]\",\"description\":\"NullableIntArray；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StatMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"description\":\"MapInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"PriceMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"description\":\"MapNullableDecimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityMap\",\"type\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"description\":\"MapUInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ListMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"description\":\"MapIntArray；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntMatrix\",\"type\":\"System.Int32[,]\",\"description\":\"MatrixInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalCube\",\"type\":\"System.Decimal[,,]\",\"description\":\"MatrixDecimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalMatrix\",\"type\":\"System.Nullable`1<System.UInt32>[,]\",\"description\":\"MatrixNullableUInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonValue\",\"type\":\"ZrConfig.Types.RawJson\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonArray\",\"type\":\"ZrConfig.Types.RawJson[]\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonArray2D\",\"type\":\"ZrConfig.Types.RawJson[][]\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonArray3D\",\"type\":\"ZrConfig.Types.RawJson[][][]\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"JsonIntArray\",\"type\":\"System.Int32[]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Hero\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"常规=1，边界=2，默认=0；引用 Hero 表\"},{\"name\":\"Skill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"常规=10，边界=11，默认=0；引用 Skill 表\"},{\"name\":\"Previous\",\"type\":\"ref:ZrConfig.Models.AllTypes10kConfig:Id:int:zero-null/1\",\"description\":\"Id=1 自引用，其余引用 Id-1；用于按需加载引用链\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"Game.Config.ActionGroup\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ActionGroup[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ActionGroup\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.AddBuff\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"BuffId\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Stacks\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"enumValues\":[],\"abstract\":true,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattleAction\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattlePhase\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleSettings\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"MaxTargets\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Interruptible\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Min\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Max\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Damage\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Ratio\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"CanCrit\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money\",\"kind\":\"converter\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Cents\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality\",\"kind\":\"enum\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[\"0\",\"1\",\"2\"],\"abstract\":false,\"baseType\":null,\"enumMembers\":{\"None\":\"0\",\"Rare\":\"1\",\"Legendary\":\"2\"},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"\"},{\"name\":\"Rewards\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"\"},{\"name\":\"Offset\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"\"},{\"name\":\"Label\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Quality\",\"type\":\"Game.Config.Quality\",\"description\":\"\"},{\"name\":\"Levels\",\"type\":\"System.Int32[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.SampleOptions\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte\",\"kind\":\"primitive\",\"primitive\":\"uint8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":\"Game.Config.Quality\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Nullable`1<System.Decimal>\",\"key\":\"System.Int32\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32[]\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal\",\"kind\":\"primitive\",\"primitive\":\"decimal\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[,,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":3,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double\",\"kind\":\"primitive\",\"primitive\":\"float64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16\",\"kind\":\"primitive\",\"primitive\":\"int16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64\",\"kind\":\"primitive\",\"primitive\":\"int64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Money>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Quality>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Boolean>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Decimal>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Nullable`1<System.Int32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Nullable`1<System.UInt32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt64>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte\",\"kind\":\"primitive\",\"primitive\":\"int8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single\",\"kind\":\"primitive\",\"primitive\":\"float32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16\",\"kind\":\"primitive\",\"primitive\":\"uint16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32\",\"kind\":\"primitive\",\"primitive\":\"uint32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64\",\"kind\":\"primitive\",\"primitive\":\"uint64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv\",\"kind\":\"kv\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Key\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Value\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson\",\"kind\":\"rawjson\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2\",\"kind\":\"vector2\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3\",\"kind\":\"vector3\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Z\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.AllTypes10kConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"AllTypes10k\"},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"},{\"id\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Skill\"}],\"tags\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"dependencies\":[\"AllTypes10k\",\"Hero\",\"Skill\"]}",plan.prefer_json)?)?)}else{None};
        cancellation.check()?;let table_Drop=if _selected.contains("Drop"){Some(DropStorage::new(R::IndexedRows::open(&bundle,"{\"name\":\"Drop\",\"hash\":\"9DAF981CB794247A5F399C8DFA97320D99D670CC605BE1E071D33E596021A242\",\"hasId\":true,\"fields\":[{\"name\":\"GroupId\",\"type\":\"System.Int32\",\"description\":\"掉落组编号\"},{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"物品编号\"},{\"name\":\"Count\",\"type\":\"System.Int32\",\"description\":\"数量\"}],\"keys\":[\"GroupId\",\"Id\"],\"groups\":[{\"name\":\"ByGroupId\",\"fields\":[\"GroupId\"]},{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}",plan.prefer_json)?)?)}else{None};
        cancellation.check()?;let table_Hero=if _selected.contains("Hero"){Some(HeroStorage::new(R::IndexedRows::open(&bundle,"{\"name\":\"Hero\",\"hash\":\"7E1E75AC24BAFD42C18F1B43A19B0027A3D241CC665DB3727BC98A4908927ECE\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"英雄编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"英雄名称\"},{\"name\":\"Skill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"技能 Id；引用 Skill 表\"},{\"name\":\"OtherSkill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"另一个技能 Id；可复用同一对象\"},{\"name\":\"Mentor\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"导师 Id；引用 Hero 表\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"},{\"id\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Skill\"}],\"tags\":[],\"dependencies\":[\"Hero\",\"Skill\"]}",plan.prefer_json)?)?)}else{None};
        cancellation.check()?;let table_ItemCatalog=if _selected.contains("ItemCatalog"){Some(ItemCatalogStorage::new(R::IndexedRows::open(&bundle,"{\"name\":\"ItemCatalog\",\"hash\":\"C62E86119B370A102C131E82385650005AABD6EC76A0074028B958400781BEE0\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"道具编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"显示名称\"},{\"name\":\"MaxStack\",\"type\":\"System.Int32\",\"description\":\"堆叠上限：1～10000\"},{\"name\":\"Rarity\",\"type\":\"System.Int32\",\"description\":\"品质：1～5\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否启用\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}",plan.prefer_json)?)?)}else{None};
        cancellation.check()?;let table_QuestRewards=if _selected.contains("QuestRewards"){Some(QuestRewardsStorage::new(R::IndexedRows::open(&bundle,"{\"name\":\"QuestRewards\",\"hash\":\"C839B7EDFA92304709824DABBA84B5534E34810224D2814C221E6F4A3B536E35\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"任务编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"任务名称\"},{\"name\":\"Item\",\"type\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"description\":\"引用 ItemCatalog 的 Id\"},{\"name\":\"Count\",\"type\":\"System.Int32\",\"description\":\"奖励数量：大于零\"},{\"name\":\"RequiredLevel\",\"type\":\"System.Int32\",\"description\":\"开放等级：大于零\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否启用\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"ItemCatalog\"}],\"tags\":[],\"dependencies\":[\"ItemCatalog\"]}",plan.prefer_json)?)?)}else{None};
        cancellation.check()?;let table_ShopOffers=if _selected.contains("ShopOffers"){Some(ShopOffersStorage::new(R::IndexedRows::open(&bundle,"{\"name\":\"ShopOffers\",\"hash\":\"0376659B4746D22E616351BEAEFC78651E69BFCEFF2D7ECAB8BD17539864E8E1\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"商品编号\"},{\"name\":\"Item\",\"type\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"description\":\"引用 ItemCatalog 的 Id\"},{\"name\":\"Price\",\"type\":\"System.Int32\",\"description\":\"价格：非负整数；0 为免费\"},{\"name\":\"DailyLimit\",\"type\":\"System.Int32\",\"description\":\"每日限购：0 为不限购\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否上架\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"ItemCatalog\"}],\"tags\":[],\"dependencies\":[\"ItemCatalog\"]}",plan.prefer_json)?)?)}else{None};
        cancellation.check()?;let table_Skill=if _selected.contains("Skill"){Some(SkillStorage::new(R::IndexedRows::open(&bundle,"{\"name\":\"Skill\",\"hash\":\"51DA7601418A98574B1C5ADC656CB7E7B479CC1F3E5237DFC5BA7532F702B9DB\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"技能编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"技能名称\"},{\"name\":\"Hero\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"英雄 Id；反向引用 Hero 表\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"}],\"tags\":[],\"dependencies\":[\"Hero\"]}",plan.prefer_json)?)?)}else{None};
        cancellation.check()?;let table_SkillLevel=if _selected.contains("SkillLevel"){Some(SkillLevelStorage::new(R::IndexedRows::open(&bundle,"{\"name\":\"SkillLevel\",\"hash\":\"9DDEDF4A13E49BE883FA11B62A966463D9B27C99620857DE5CD5E721CECC92B2\",\"hasId\":false,\"fields\":[{\"name\":\"SkillId\",\"type\":\"System.Int32\",\"description\":\"技能编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"技能名称\"},{\"name\":\"Level\",\"type\":\"System.Int32\",\"description\":\"技能等级\"},{\"name\":\"Quality\",\"type\":\"System.Int32\",\"description\":\"品质\"},{\"name\":\"Damage\",\"type\":\"System.Int32\",\"description\":\"伤害\"}],\"keys\":[\"SkillId\",\"Level\",\"Quality\"],\"groups\":[{\"name\":\"ByLevel\",\"fields\":[\"Level\"]},{\"name\":\"ByQuality\",\"fields\":[\"Quality\"]},{\"name\":\"BySkillId\",\"fields\":[\"SkillId\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}",plan.prefer_json)?)?)}else{None};
        let next=Self{load_plan_fingerprint:R::load_plan_fingerprint(plan.mode,plan.prefer_json,selection.tables()),version:bundle.version().to_owned(),manifest_digest:bundle.digest.clone(),source_namespace:settings.source_namespace().to_owned(),budget:bundle.budget.clone(),selection,mode:plan.mode,table_AllTypes,table_AllTypes10k,table_Drop,table_Hero,table_ItemCatalog,table_QuestRewards,table_ShopOffers,table_Skill,table_SkillLevel};
        let mut directory=std::collections::HashMap::new();
        if let Some(table)=&next.table_AllTypes{directory.insert("AllTypes".to_owned(),table.rows.indexed.reference_keys());}
        if let Some(table)=&next.table_AllTypes10k{directory.insert("AllTypes10k".to_owned(),table.rows.indexed.reference_keys());}
        if let Some(table)=&next.table_Drop{directory.insert("Drop".to_owned(),table.rows.indexed.reference_keys());}
        if let Some(table)=&next.table_Hero{directory.insert("Hero".to_owned(),table.rows.indexed.reference_keys());}
        if let Some(table)=&next.table_ItemCatalog{directory.insert("ItemCatalog".to_owned(),table.rows.indexed.reference_keys());}
        if let Some(table)=&next.table_QuestRewards{directory.insert("QuestRewards".to_owned(),table.rows.indexed.reference_keys());}
        if let Some(table)=&next.table_ShopOffers{directory.insert("ShopOffers".to_owned(),table.rows.indexed.reference_keys());}
        if let Some(table)=&next.table_Skill{directory.insert("Skill".to_owned(),table.rows.indexed.reference_keys());}
        if let Some(table)=&next.table_SkillLevel{directory.insert("SkillLevel".to_owned(),table.rows.indexed.reference_keys());}
        let directory=Arc::new(directory);
        if let Some(table)=&next.table_AllTypes{table.rows.indexed.bind_references(directory.clone());}
        if let Some(table)=&next.table_AllTypes10k{table.rows.indexed.bind_references(directory.clone());}
        if let Some(table)=&next.table_Drop{table.rows.indexed.bind_references(directory.clone());}
        if let Some(table)=&next.table_Hero{table.rows.indexed.bind_references(directory.clone());}
        if let Some(table)=&next.table_ItemCatalog{table.rows.indexed.bind_references(directory.clone());}
        if let Some(table)=&next.table_QuestRewards{table.rows.indexed.bind_references(directory.clone());}
        if let Some(table)=&next.table_ShopOffers{table.rows.indexed.bind_references(directory.clone());}
        if let Some(table)=&next.table_Skill{table.rows.indexed.bind_references(directory.clone());}
        if let Some(table)=&next.table_SkillLevel{table.rows.indexed.bind_references(directory.clone());}
        if plan.mode!=LoadMode::LazyRowsOnAccess{
            if let Some(table)=&next.table_AllTypes{for i in 0..table.rows.count(){cancellation.check()?;let value=table.rows.indexed.read_transient(i)?;let _=AllTypesData::decode(&value).map_err(R::data_error)?;table.rows.indexed.validate_references(&value,|name,id|next.validate_reference(name,id))?;}}
            if let Some(table)=&next.table_AllTypes10k{for i in 0..table.rows.count(){cancellation.check()?;let value=table.rows.indexed.read_transient(i)?;let _=AllTypes10kData::decode(&value).map_err(R::data_error)?;table.rows.indexed.validate_references(&value,|name,id|next.validate_reference(name,id))?;}}
            if let Some(table)=&next.table_Drop{for i in 0..table.rows.count(){cancellation.check()?;let value=table.rows.indexed.read_transient(i)?;let _=DropData::decode(&value).map_err(R::data_error)?;table.rows.indexed.validate_references(&value,|name,id|next.validate_reference(name,id))?;}}
            if let Some(table)=&next.table_Hero{for i in 0..table.rows.count(){cancellation.check()?;let value=table.rows.indexed.read_transient(i)?;let _=HeroData::decode(&value).map_err(R::data_error)?;table.rows.indexed.validate_references(&value,|name,id|next.validate_reference(name,id))?;}}
            if let Some(table)=&next.table_ItemCatalog{for i in 0..table.rows.count(){cancellation.check()?;let value=table.rows.indexed.read_transient(i)?;let _=ItemCatalogData::decode(&value).map_err(R::data_error)?;table.rows.indexed.validate_references(&value,|name,id|next.validate_reference(name,id))?;}}
            if let Some(table)=&next.table_QuestRewards{for i in 0..table.rows.count(){cancellation.check()?;let value=table.rows.indexed.read_transient(i)?;let _=QuestRewardsData::decode(&value).map_err(R::data_error)?;table.rows.indexed.validate_references(&value,|name,id|next.validate_reference(name,id))?;}}
            if let Some(table)=&next.table_ShopOffers{for i in 0..table.rows.count(){cancellation.check()?;let value=table.rows.indexed.read_transient(i)?;let _=ShopOffersData::decode(&value).map_err(R::data_error)?;table.rows.indexed.validate_references(&value,|name,id|next.validate_reference(name,id))?;}}
            if let Some(table)=&next.table_Skill{for i in 0..table.rows.count(){cancellation.check()?;let value=table.rows.indexed.read_transient(i)?;let _=SkillData::decode(&value).map_err(R::data_error)?;table.rows.indexed.validate_references(&value,|name,id|next.validate_reference(name,id))?;}}
            if let Some(table)=&next.table_SkillLevel{for i in 0..table.rows.count(){cancellation.check()?;let value=table.rows.indexed.read_transient(i)?;let _=SkillLevelData::decode(&value).map_err(R::data_error)?;table.rows.indexed.validate_references(&value,|name,id|next.validate_reference(name,id))?;}}
        }
        if matches!(plan.mode,LoadMode::ServerDefault|LoadMode::SelectedTables){next.load_all(cancellation)?;}
        for name in &plan.warmup_tables{match name.as_str(){
            "AllTypes"=>next.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Warmup outside selection"))?.load_all(cancellation)?,
            "AllTypes10k"=>next.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Warmup outside selection"))?.load_all(cancellation)?,
            "Drop"=>next.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Warmup outside selection"))?.load_all(cancellation)?,
            "Hero"=>next.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Warmup outside selection"))?.load_all(cancellation)?,
            "ItemCatalog"=>next.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Warmup outside selection"))?.load_all(cancellation)?,
            "QuestRewards"=>next.table_QuestRewards.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Warmup outside selection"))?.load_all(cancellation)?,
            "ShopOffers"=>next.table_ShopOffers.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Warmup outside selection"))?.load_all(cancellation)?,
            "Skill"=>next.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Warmup outside selection"))?.load_all(cancellation)?,
            "SkillLevel"=>next.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Warmup outside selection"))?.load_all(cancellation)?,
            _=>return Err(ConfigError::new(ErrorCode::InvalidPlan,"Unknown warmup table")),
        }}cancellation.check()?;Ok(Arc::new(next))
    }
    fn validate_reference(&self,name:&str,_id:i32)->ServerResult<()>{match name{
        "AllTypes"=>{self.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Reference outside selection"))?.rows.indexed.reference(_id)?;Ok(())},
        "AllTypes10k"=>{self.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Reference outside selection"))?.rows.indexed.reference(_id)?;Ok(())},
        "Drop"=>{self.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Reference outside selection"))?.rows.indexed.reference(_id)?;Ok(())},
        "Hero"=>{self.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Reference outside selection"))?.rows.indexed.reference(_id)?;Ok(())},
        "ItemCatalog"=>{self.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Reference outside selection"))?.rows.indexed.reference(_id)?;Ok(())},
        "QuestRewards"=>{self.table_QuestRewards.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Reference outside selection"))?.rows.indexed.reference(_id)?;Ok(())},
        "ShopOffers"=>{self.table_ShopOffers.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Reference outside selection"))?.rows.indexed.reference(_id)?;Ok(())},
        "Skill"=>{self.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Reference outside selection"))?.rows.indexed.reference(_id)?;Ok(())},
        "SkillLevel"=>{self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Reference outside selection"))?.rows.indexed.reference(_id)?;Ok(())},
        _=>Err(ConfigError::new(ErrorCode::MissingReference,"Unknown reference table")),
    }}
    pub fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{
        cancellation.check()?;
        if let Some(table)=&self.table_AllTypes{table.load_all(cancellation)?;}
        if let Some(table)=&self.table_AllTypes10k{table.load_all(cancellation)?;}
        if let Some(table)=&self.table_Drop{table.load_all(cancellation)?;}
        if let Some(table)=&self.table_Hero{table.load_all(cancellation)?;}
        if let Some(table)=&self.table_ItemCatalog{table.load_all(cancellation)?;}
        if let Some(table)=&self.table_QuestRewards{table.load_all(cancellation)?;}
        if let Some(table)=&self.table_ShopOffers{table.load_all(cancellation)?;}
        if let Some(table)=&self.table_Skill{table.load_all(cancellation)?;}
        if let Some(table)=&self.table_SkillLevel{table.load_all(cancellation)?;}
        Ok(())
    }
    pub fn loaded_row_count(&self)->usize{self.table_AllTypes.as_ref().map_or(0,|t|t.rows.loaded_count())+self.table_AllTypes10k.as_ref().map_or(0,|t|t.rows.loaded_count())+self.table_Drop.as_ref().map_or(0,|t|t.rows.loaded_count())+self.table_Hero.as_ref().map_or(0,|t|t.rows.loaded_count())+self.table_ItemCatalog.as_ref().map_or(0,|t|t.rows.loaded_count())+self.table_QuestRewards.as_ref().map_or(0,|t|t.rows.loaded_count())+self.table_ShopOffers.as_ref().map_or(0,|t|t.rows.loaded_count())+self.table_Skill.as_ref().map_or(0,|t|t.rows.loaded_count())+self.table_SkillLevel.as_ref().map_or(0,|t|t.rows.loaded_count())}
    pub fn completed_table_count(&self)->usize{self.table_AllTypes.as_ref().map_or(0,|t|usize::from(t.all.get().is_some()))+self.table_AllTypes10k.as_ref().map_or(0,|t|usize::from(t.all.get().is_some()))+self.table_Drop.as_ref().map_or(0,|t|usize::from(t.all.get().is_some()))+self.table_Hero.as_ref().map_or(0,|t|usize::from(t.all.get().is_some()))+self.table_ItemCatalog.as_ref().map_or(0,|t|usize::from(t.all.get().is_some()))+self.table_QuestRewards.as_ref().map_or(0,|t|usize::from(t.all.get().is_some()))+self.table_ShopOffers.as_ref().map_or(0,|t|usize::from(t.all.get().is_some()))+self.table_Skill.as_ref().map_or(0,|t|usize::from(t.all.get().is_some()))+self.table_SkillLevel.as_ref().map_or(0,|t|usize::from(t.all.get().is_some()))}
    pub fn load_source_with_warmup(source:&dyn R::ConfigSource,settings:&R::LoadSettings,requests:&[Warmup],cancellation:&Cancellation)->ServerResult<Arc<Self>>{
        let catalog=super::server::Snapshot::catalog()?;let names=match &settings.plan().tables{Some(names)=>names.iter().map(String::as_str).collect::<Vec<_>>(),None=>catalog.tables().iter().map(R::TableDescriptor::name).collect()};let selection=catalog.select(&names)?;
        for request in requests{if !selection.tables().iter().any(|name|name==request.table){return Err(ConfigError::new(ErrorCode::InvalidPlan,"Warmup outside selection"));}}
        let next=Self::load_source(source,settings,cancellation)?;for request in requests{cancellation.check()?;(request.action)(&next,cancellation)?;}cancellation.check()?;Ok(next)
    }
    pub fn load_with_warmup(root:&Path,plan:&LoadPlan,requests:&[Warmup],cancellation:&Cancellation)->ServerResult<Arc<Self>>{let source=R::FileConfigSource::new(root);let settings=R::LoadSettings::new(plan.clone()).with_source_namespace(source.source_namespace());Self::load_source_with_warmup(&source,&settings,requests,cancellation)}
    pub fn warmup_table_AllTypes()->Warmup{Warmup{table:"AllTypes",action:Box::new(|snapshot,token|snapshot.load_table_AllTypes(token).map(|_|()))}}
    pub fn warmup_row_AllTypes(k0:i32)->Warmup{Warmup{table:"AllTypes",action:Box::new(move|snapshot,token|snapshot.load_AllTypes(k0,token).map(|_|()))}}
    pub fn warmup_group_AllTypes_ById(k0:i32)->Warmup{Warmup{table:"AllTypes",action:Box::new(move|snapshot,token|snapshot.load_group_AllTypes_ById(k0,token).map(|_|()))}}
    pub fn warmup_group_AllTypes_ByQuality(k0:T::T_Game_Config_Quality)->Warmup{Warmup{table:"AllTypes",action:Box::new(move|snapshot,token|snapshot.load_group_AllTypes_ByQuality(k0,token).map(|_|()))}}
    pub fn warmup_table_AllTypes10k()->Warmup{Warmup{table:"AllTypes10k",action:Box::new(|snapshot,token|snapshot.load_table_AllTypes10k(token).map(|_|()))}}
    pub fn warmup_row_AllTypes10k(k0:i32)->Warmup{Warmup{table:"AllTypes10k",action:Box::new(move|snapshot,token|snapshot.load_AllTypes10k(k0,token).map(|_|()))}}
    pub fn warmup_group_AllTypes10k_ById(k0:i32)->Warmup{Warmup{table:"AllTypes10k",action:Box::new(move|snapshot,token|snapshot.load_group_AllTypes10k_ById(k0,token).map(|_|()))}}
    pub fn warmup_table_Drop()->Warmup{Warmup{table:"Drop",action:Box::new(|snapshot,token|snapshot.load_table_Drop(token).map(|_|()))}}
    pub fn warmup_row_Drop(k0:i32,k1:i32)->Warmup{Warmup{table:"Drop",action:Box::new(move|snapshot,token|snapshot.load_Drop(k0,k1,token).map(|_|()))}}
    pub fn warmup_group_Drop_ByGroupId(k0:i32)->Warmup{Warmup{table:"Drop",action:Box::new(move|snapshot,token|snapshot.load_group_Drop_ByGroupId(k0,token).map(|_|()))}}
    pub fn warmup_group_Drop_ById(k0:i32)->Warmup{Warmup{table:"Drop",action:Box::new(move|snapshot,token|snapshot.load_group_Drop_ById(k0,token).map(|_|()))}}
    pub fn warmup_table_Hero()->Warmup{Warmup{table:"Hero",action:Box::new(|snapshot,token|snapshot.load_table_Hero(token).map(|_|()))}}
    pub fn warmup_row_Hero(k0:i32)->Warmup{Warmup{table:"Hero",action:Box::new(move|snapshot,token|snapshot.load_Hero(k0,token).map(|_|()))}}
    pub fn warmup_group_Hero_ById(k0:i32)->Warmup{Warmup{table:"Hero",action:Box::new(move|snapshot,token|snapshot.load_group_Hero_ById(k0,token).map(|_|()))}}
    pub fn warmup_table_ItemCatalog()->Warmup{Warmup{table:"ItemCatalog",action:Box::new(|snapshot,token|snapshot.load_table_ItemCatalog(token).map(|_|()))}}
    pub fn warmup_row_ItemCatalog(k0:i32)->Warmup{Warmup{table:"ItemCatalog",action:Box::new(move|snapshot,token|snapshot.load_ItemCatalog(k0,token).map(|_|()))}}
    pub fn warmup_group_ItemCatalog_ById(k0:i32)->Warmup{Warmup{table:"ItemCatalog",action:Box::new(move|snapshot,token|snapshot.load_group_ItemCatalog_ById(k0,token).map(|_|()))}}
    pub fn warmup_table_QuestRewards()->Warmup{Warmup{table:"QuestRewards",action:Box::new(|snapshot,token|snapshot.load_table_QuestRewards(token).map(|_|()))}}
    pub fn warmup_row_QuestRewards(k0:i32)->Warmup{Warmup{table:"QuestRewards",action:Box::new(move|snapshot,token|snapshot.load_QuestRewards(k0,token).map(|_|()))}}
    pub fn warmup_group_QuestRewards_ById(k0:i32)->Warmup{Warmup{table:"QuestRewards",action:Box::new(move|snapshot,token|snapshot.load_group_QuestRewards_ById(k0,token).map(|_|()))}}
    pub fn warmup_table_ShopOffers()->Warmup{Warmup{table:"ShopOffers",action:Box::new(|snapshot,token|snapshot.load_table_ShopOffers(token).map(|_|()))}}
    pub fn warmup_row_ShopOffers(k0:i32)->Warmup{Warmup{table:"ShopOffers",action:Box::new(move|snapshot,token|snapshot.load_ShopOffers(k0,token).map(|_|()))}}
    pub fn warmup_group_ShopOffers_ById(k0:i32)->Warmup{Warmup{table:"ShopOffers",action:Box::new(move|snapshot,token|snapshot.load_group_ShopOffers_ById(k0,token).map(|_|()))}}
    pub fn warmup_table_Skill()->Warmup{Warmup{table:"Skill",action:Box::new(|snapshot,token|snapshot.load_table_Skill(token).map(|_|()))}}
    pub fn warmup_row_Skill(k0:i32)->Warmup{Warmup{table:"Skill",action:Box::new(move|snapshot,token|snapshot.load_Skill(k0,token).map(|_|()))}}
    pub fn warmup_group_Skill_ById(k0:i32)->Warmup{Warmup{table:"Skill",action:Box::new(move|snapshot,token|snapshot.load_group_Skill_ById(k0,token).map(|_|()))}}
    pub fn warmup_table_SkillLevel()->Warmup{Warmup{table:"SkillLevel",action:Box::new(|snapshot,token|snapshot.load_table_SkillLevel(token).map(|_|()))}}
    pub fn warmup_row_SkillLevel(k0:i32,k1:i32,k2:i32)->Warmup{Warmup{table:"SkillLevel",action:Box::new(move|snapshot,token|snapshot.load_SkillLevel(k0,k1,k2,token).map(|_|()))}}
    pub fn warmup_group_SkillLevel_ByLevel(k0:i32)->Warmup{Warmup{table:"SkillLevel",action:Box::new(move|snapshot,token|snapshot.load_group_SkillLevel_ByLevel(k0,token).map(|_|()))}}
    pub fn warmup_group_SkillLevel_ByQuality(k0:i32)->Warmup{Warmup{table:"SkillLevel",action:Box::new(move|snapshot,token|snapshot.load_group_SkillLevel_ByQuality(k0,token).map(|_|()))}}
    pub fn warmup_group_SkillLevel_BySkillId(k0:i32)->Warmup{Warmup{table:"SkillLevel",action:Box::new(move|snapshot,token|snapshot.load_group_SkillLevel_BySkillId(k0,token).map(|_|()))}}
    pub fn find_AllTypes(self:&Arc<Self>,k0:i32)->ServerResult<Option<AllTypesRow>>{let table=self.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.get(index)?}))}
    pub fn get_AllTypes(self:&Arc<Self>,k0:i32)->ServerResult<AllTypesRow>{self.find_AllTypes(k0)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    pub fn try_load_AllTypes(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<Option<AllTypesRow>>{cancellation.check()?;let table=self.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.load(index,cancellation)?}))}
    pub fn load_AllTypes(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<AllTypesRow>{self.try_load_AllTypes(k0,cancellation)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    fn reference_AllTypes(self:&Arc<Self>,id:i32,cancellation:Option<&Cancellation>)->ServerResult<Option<AllTypesRow>>{if id==0{return Ok(None)}let table=self.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes"))?;let index=table.rows.indexed.reference(id)?;let row=if let Some(token)=cancellation{table.rows.load(index,token)?}else{table.rows.get(index)?};Ok(Some(R::RowHandle{snapshot:self.clone(),row}))}
    fn ordinal_AllTypes(&self,index:usize)->ServerResult<Arc<AllTypesData>>{let table=self.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes"))?;table.rows.get(index)}
    pub fn rows_AllTypes(self:&Arc<Self>)->ServerResult<R::DataGroup<Self,AllTypesData>>{let table=self.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes"))?;let positions=table.all.get().ok_or_else(||ConfigError::new(ErrorCode::TableNotLoaded,"Load the table explicitly"))?.clone();Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_AllTypes})}
    pub fn load_table_AllTypes(self:&Arc<Self>,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,AllTypesData>>{let table=self.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes"))?;table.load_all(cancellation)?;self.rows_AllTypes()}
    pub fn find_group_AllTypes_ById(self:&Arc<Self>,k0:i32)->ServerResult<R::DataGroup<Self,AllTypesData>>{let table=self.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes"))?;let positions=table.group_ById(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_AllTypes})}
    pub fn load_group_AllTypes_ById(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,AllTypesData>>{cancellation.check()?;let table=self.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes"))?;let positions=table.group_ById(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_AllTypes})}
    pub fn find_group_AllTypes_ByQuality(self:&Arc<Self>,k0:T::T_Game_Config_Quality)->ServerResult<R::DataGroup<Self,AllTypesData>>{let table=self.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes"))?;let positions=table.group_ByQuality(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_AllTypes})}
    pub fn load_group_AllTypes_ByQuality(self:&Arc<Self>,k0:T::T_Game_Config_Quality,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,AllTypesData>>{cancellation.check()?;let table=self.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes"))?;let positions=table.group_ByQuality(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_AllTypes})}
    pub fn find_AllTypes10k(self:&Arc<Self>,k0:i32)->ServerResult<Option<AllTypes10kRow>>{let table=self.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes10k"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.get(index)?}))}
    pub fn get_AllTypes10k(self:&Arc<Self>,k0:i32)->ServerResult<AllTypes10kRow>{self.find_AllTypes10k(k0)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    pub fn try_load_AllTypes10k(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<Option<AllTypes10kRow>>{cancellation.check()?;let table=self.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes10k"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.load(index,cancellation)?}))}
    pub fn load_AllTypes10k(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<AllTypes10kRow>{self.try_load_AllTypes10k(k0,cancellation)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    fn reference_AllTypes10k(self:&Arc<Self>,id:i32,cancellation:Option<&Cancellation>)->ServerResult<Option<AllTypes10kRow>>{if id==0{return Ok(None)}let table=self.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes10k"))?;let index=table.rows.indexed.reference(id)?;let row=if let Some(token)=cancellation{table.rows.load(index,token)?}else{table.rows.get(index)?};Ok(Some(R::RowHandle{snapshot:self.clone(),row}))}
    fn ordinal_AllTypes10k(&self,index:usize)->ServerResult<Arc<AllTypes10kData>>{let table=self.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes10k"))?;table.rows.get(index)}
    pub fn rows_AllTypes10k(self:&Arc<Self>)->ServerResult<R::DataGroup<Self,AllTypes10kData>>{let table=self.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes10k"))?;let positions=table.all.get().ok_or_else(||ConfigError::new(ErrorCode::TableNotLoaded,"Load the table explicitly"))?.clone();Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_AllTypes10k})}
    pub fn load_table_AllTypes10k(self:&Arc<Self>,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,AllTypes10kData>>{let table=self.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes10k"))?;table.load_all(cancellation)?;self.rows_AllTypes10k()}
    pub fn find_group_AllTypes10k_ById(self:&Arc<Self>,k0:i32)->ServerResult<R::DataGroup<Self,AllTypes10kData>>{let table=self.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes10k"))?;let positions=table.group_ById(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_AllTypes10k})}
    pub fn load_group_AllTypes10k_ById(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,AllTypes10kData>>{cancellation.check()?;let table=self.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"AllTypes10k"))?;let positions=table.group_ById(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_AllTypes10k})}
    pub fn find_Drop(self:&Arc<Self>,k0:i32,k1:i32)->ServerResult<Option<DropRow>>{let table=self.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Drop"))?;let index=match table.key.find(k0,k1){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.get(index)?}))}
    pub fn get_Drop(self:&Arc<Self>,k0:i32,k1:i32)->ServerResult<DropRow>{self.find_Drop(k0,k1)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    pub fn try_load_Drop(self:&Arc<Self>,k0:i32,k1:i32,cancellation:&Cancellation)->ServerResult<Option<DropRow>>{cancellation.check()?;let table=self.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Drop"))?;let index=match table.key.find(k0,k1){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.load(index,cancellation)?}))}
    pub fn load_Drop(self:&Arc<Self>,k0:i32,k1:i32,cancellation:&Cancellation)->ServerResult<DropRow>{self.try_load_Drop(k0,k1,cancellation)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    fn reference_Drop(self:&Arc<Self>,id:i32,cancellation:Option<&Cancellation>)->ServerResult<Option<DropRow>>{if id==0{return Ok(None)}let table=self.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Drop"))?;let index=table.rows.indexed.reference(id)?;let row=if let Some(token)=cancellation{table.rows.load(index,token)?}else{table.rows.get(index)?};Ok(Some(R::RowHandle{snapshot:self.clone(),row}))}
    fn ordinal_Drop(&self,index:usize)->ServerResult<Arc<DropData>>{let table=self.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Drop"))?;table.rows.get(index)}
    pub fn rows_Drop(self:&Arc<Self>)->ServerResult<R::DataGroup<Self,DropData>>{let table=self.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Drop"))?;let positions=table.all.get().ok_or_else(||ConfigError::new(ErrorCode::TableNotLoaded,"Load the table explicitly"))?.clone();Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_Drop})}
    pub fn load_table_Drop(self:&Arc<Self>,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,DropData>>{let table=self.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Drop"))?;table.load_all(cancellation)?;self.rows_Drop()}
    pub fn find_group_Drop_ByGroupId(self:&Arc<Self>,k0:i32)->ServerResult<R::DataGroup<Self,DropData>>{let table=self.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Drop"))?;let positions=table.group_ByGroupId(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_Drop})}
    pub fn load_group_Drop_ByGroupId(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,DropData>>{cancellation.check()?;let table=self.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Drop"))?;let positions=table.group_ByGroupId(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_Drop})}
    pub fn find_group_Drop_ById(self:&Arc<Self>,k0:i32)->ServerResult<R::DataGroup<Self,DropData>>{let table=self.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Drop"))?;let positions=table.group_ById(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_Drop})}
    pub fn load_group_Drop_ById(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,DropData>>{cancellation.check()?;let table=self.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Drop"))?;let positions=table.group_ById(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_Drop})}
    pub fn find_Hero(self:&Arc<Self>,k0:i32)->ServerResult<Option<HeroRow>>{let table=self.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Hero"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.get(index)?}))}
    pub fn get_Hero(self:&Arc<Self>,k0:i32)->ServerResult<HeroRow>{self.find_Hero(k0)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    pub fn try_load_Hero(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<Option<HeroRow>>{cancellation.check()?;let table=self.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Hero"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.load(index,cancellation)?}))}
    pub fn load_Hero(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<HeroRow>{self.try_load_Hero(k0,cancellation)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    fn reference_Hero(self:&Arc<Self>,id:i32,cancellation:Option<&Cancellation>)->ServerResult<Option<HeroRow>>{if id==0{return Ok(None)}let table=self.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Hero"))?;let index=table.rows.indexed.reference(id)?;let row=if let Some(token)=cancellation{table.rows.load(index,token)?}else{table.rows.get(index)?};Ok(Some(R::RowHandle{snapshot:self.clone(),row}))}
    fn ordinal_Hero(&self,index:usize)->ServerResult<Arc<HeroData>>{let table=self.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Hero"))?;table.rows.get(index)}
    pub fn rows_Hero(self:&Arc<Self>)->ServerResult<R::DataGroup<Self,HeroData>>{let table=self.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Hero"))?;let positions=table.all.get().ok_or_else(||ConfigError::new(ErrorCode::TableNotLoaded,"Load the table explicitly"))?.clone();Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_Hero})}
    pub fn load_table_Hero(self:&Arc<Self>,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,HeroData>>{let table=self.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Hero"))?;table.load_all(cancellation)?;self.rows_Hero()}
    pub fn find_group_Hero_ById(self:&Arc<Self>,k0:i32)->ServerResult<R::DataGroup<Self,HeroData>>{let table=self.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Hero"))?;let positions=table.group_ById(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_Hero})}
    pub fn load_group_Hero_ById(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,HeroData>>{cancellation.check()?;let table=self.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Hero"))?;let positions=table.group_ById(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_Hero})}
    pub fn find_ItemCatalog(self:&Arc<Self>,k0:i32)->ServerResult<Option<ItemCatalogRow>>{let table=self.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ItemCatalog"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.get(index)?}))}
    pub fn get_ItemCatalog(self:&Arc<Self>,k0:i32)->ServerResult<ItemCatalogRow>{self.find_ItemCatalog(k0)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    pub fn try_load_ItemCatalog(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<Option<ItemCatalogRow>>{cancellation.check()?;let table=self.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ItemCatalog"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.load(index,cancellation)?}))}
    pub fn load_ItemCatalog(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<ItemCatalogRow>{self.try_load_ItemCatalog(k0,cancellation)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    fn reference_ItemCatalog(self:&Arc<Self>,id:i32,cancellation:Option<&Cancellation>)->ServerResult<Option<ItemCatalogRow>>{if id==0{return Ok(None)}let table=self.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ItemCatalog"))?;let index=table.rows.indexed.reference(id)?;let row=if let Some(token)=cancellation{table.rows.load(index,token)?}else{table.rows.get(index)?};Ok(Some(R::RowHandle{snapshot:self.clone(),row}))}
    fn ordinal_ItemCatalog(&self,index:usize)->ServerResult<Arc<ItemCatalogData>>{let table=self.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ItemCatalog"))?;table.rows.get(index)}
    pub fn rows_ItemCatalog(self:&Arc<Self>)->ServerResult<R::DataGroup<Self,ItemCatalogData>>{let table=self.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ItemCatalog"))?;let positions=table.all.get().ok_or_else(||ConfigError::new(ErrorCode::TableNotLoaded,"Load the table explicitly"))?.clone();Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_ItemCatalog})}
    pub fn load_table_ItemCatalog(self:&Arc<Self>,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,ItemCatalogData>>{let table=self.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ItemCatalog"))?;table.load_all(cancellation)?;self.rows_ItemCatalog()}
    pub fn find_group_ItemCatalog_ById(self:&Arc<Self>,k0:i32)->ServerResult<R::DataGroup<Self,ItemCatalogData>>{let table=self.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ItemCatalog"))?;let positions=table.group_ById(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_ItemCatalog})}
    pub fn load_group_ItemCatalog_ById(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,ItemCatalogData>>{cancellation.check()?;let table=self.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ItemCatalog"))?;let positions=table.group_ById(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_ItemCatalog})}
    pub fn find_QuestRewards(self:&Arc<Self>,k0:i32)->ServerResult<Option<QuestRewardsRow>>{let table=self.table_QuestRewards.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"QuestRewards"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.get(index)?}))}
    pub fn get_QuestRewards(self:&Arc<Self>,k0:i32)->ServerResult<QuestRewardsRow>{self.find_QuestRewards(k0)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    pub fn try_load_QuestRewards(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<Option<QuestRewardsRow>>{cancellation.check()?;let table=self.table_QuestRewards.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"QuestRewards"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.load(index,cancellation)?}))}
    pub fn load_QuestRewards(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<QuestRewardsRow>{self.try_load_QuestRewards(k0,cancellation)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    fn reference_QuestRewards(self:&Arc<Self>,id:i32,cancellation:Option<&Cancellation>)->ServerResult<Option<QuestRewardsRow>>{if id==0{return Ok(None)}let table=self.table_QuestRewards.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"QuestRewards"))?;let index=table.rows.indexed.reference(id)?;let row=if let Some(token)=cancellation{table.rows.load(index,token)?}else{table.rows.get(index)?};Ok(Some(R::RowHandle{snapshot:self.clone(),row}))}
    fn ordinal_QuestRewards(&self,index:usize)->ServerResult<Arc<QuestRewardsData>>{let table=self.table_QuestRewards.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"QuestRewards"))?;table.rows.get(index)}
    pub fn rows_QuestRewards(self:&Arc<Self>)->ServerResult<R::DataGroup<Self,QuestRewardsData>>{let table=self.table_QuestRewards.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"QuestRewards"))?;let positions=table.all.get().ok_or_else(||ConfigError::new(ErrorCode::TableNotLoaded,"Load the table explicitly"))?.clone();Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_QuestRewards})}
    pub fn load_table_QuestRewards(self:&Arc<Self>,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,QuestRewardsData>>{let table=self.table_QuestRewards.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"QuestRewards"))?;table.load_all(cancellation)?;self.rows_QuestRewards()}
    pub fn find_group_QuestRewards_ById(self:&Arc<Self>,k0:i32)->ServerResult<R::DataGroup<Self,QuestRewardsData>>{let table=self.table_QuestRewards.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"QuestRewards"))?;let positions=table.group_ById(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_QuestRewards})}
    pub fn load_group_QuestRewards_ById(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,QuestRewardsData>>{cancellation.check()?;let table=self.table_QuestRewards.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"QuestRewards"))?;let positions=table.group_ById(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_QuestRewards})}
    pub fn find_ShopOffers(self:&Arc<Self>,k0:i32)->ServerResult<Option<ShopOffersRow>>{let table=self.table_ShopOffers.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ShopOffers"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.get(index)?}))}
    pub fn get_ShopOffers(self:&Arc<Self>,k0:i32)->ServerResult<ShopOffersRow>{self.find_ShopOffers(k0)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    pub fn try_load_ShopOffers(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<Option<ShopOffersRow>>{cancellation.check()?;let table=self.table_ShopOffers.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ShopOffers"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.load(index,cancellation)?}))}
    pub fn load_ShopOffers(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<ShopOffersRow>{self.try_load_ShopOffers(k0,cancellation)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    fn reference_ShopOffers(self:&Arc<Self>,id:i32,cancellation:Option<&Cancellation>)->ServerResult<Option<ShopOffersRow>>{if id==0{return Ok(None)}let table=self.table_ShopOffers.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ShopOffers"))?;let index=table.rows.indexed.reference(id)?;let row=if let Some(token)=cancellation{table.rows.load(index,token)?}else{table.rows.get(index)?};Ok(Some(R::RowHandle{snapshot:self.clone(),row}))}
    fn ordinal_ShopOffers(&self,index:usize)->ServerResult<Arc<ShopOffersData>>{let table=self.table_ShopOffers.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ShopOffers"))?;table.rows.get(index)}
    pub fn rows_ShopOffers(self:&Arc<Self>)->ServerResult<R::DataGroup<Self,ShopOffersData>>{let table=self.table_ShopOffers.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ShopOffers"))?;let positions=table.all.get().ok_or_else(||ConfigError::new(ErrorCode::TableNotLoaded,"Load the table explicitly"))?.clone();Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_ShopOffers})}
    pub fn load_table_ShopOffers(self:&Arc<Self>,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,ShopOffersData>>{let table=self.table_ShopOffers.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ShopOffers"))?;table.load_all(cancellation)?;self.rows_ShopOffers()}
    pub fn find_group_ShopOffers_ById(self:&Arc<Self>,k0:i32)->ServerResult<R::DataGroup<Self,ShopOffersData>>{let table=self.table_ShopOffers.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ShopOffers"))?;let positions=table.group_ById(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_ShopOffers})}
    pub fn load_group_ShopOffers_ById(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,ShopOffersData>>{cancellation.check()?;let table=self.table_ShopOffers.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"ShopOffers"))?;let positions=table.group_ById(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_ShopOffers})}
    pub fn find_Skill(self:&Arc<Self>,k0:i32)->ServerResult<Option<SkillRow>>{let table=self.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Skill"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.get(index)?}))}
    pub fn get_Skill(self:&Arc<Self>,k0:i32)->ServerResult<SkillRow>{self.find_Skill(k0)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    pub fn try_load_Skill(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<Option<SkillRow>>{cancellation.check()?;let table=self.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Skill"))?;let index=match table.key.find(k0){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.load(index,cancellation)?}))}
    pub fn load_Skill(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<SkillRow>{self.try_load_Skill(k0,cancellation)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    fn reference_Skill(self:&Arc<Self>,id:i32,cancellation:Option<&Cancellation>)->ServerResult<Option<SkillRow>>{if id==0{return Ok(None)}let table=self.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Skill"))?;let index=table.rows.indexed.reference(id)?;let row=if let Some(token)=cancellation{table.rows.load(index,token)?}else{table.rows.get(index)?};Ok(Some(R::RowHandle{snapshot:self.clone(),row}))}
    fn ordinal_Skill(&self,index:usize)->ServerResult<Arc<SkillData>>{let table=self.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Skill"))?;table.rows.get(index)}
    pub fn rows_Skill(self:&Arc<Self>)->ServerResult<R::DataGroup<Self,SkillData>>{let table=self.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Skill"))?;let positions=table.all.get().ok_or_else(||ConfigError::new(ErrorCode::TableNotLoaded,"Load the table explicitly"))?.clone();Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_Skill})}
    pub fn load_table_Skill(self:&Arc<Self>,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,SkillData>>{let table=self.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Skill"))?;table.load_all(cancellation)?;self.rows_Skill()}
    pub fn find_group_Skill_ById(self:&Arc<Self>,k0:i32)->ServerResult<R::DataGroup<Self,SkillData>>{let table=self.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Skill"))?;let positions=table.group_ById(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_Skill})}
    pub fn load_group_Skill_ById(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,SkillData>>{cancellation.check()?;let table=self.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"Skill"))?;let positions=table.group_ById(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_Skill})}
    pub fn find_SkillLevel(self:&Arc<Self>,k0:i32,k1:i32,k2:i32)->ServerResult<Option<SkillLevelRow>>{let table=self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"SkillLevel"))?;let index=match table.key.find(k0,k1,k2){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.get(index)?}))}
    pub fn get_SkillLevel(self:&Arc<Self>,k0:i32,k1:i32,k2:i32)->ServerResult<SkillLevelRow>{self.find_SkillLevel(k0,k1,k2)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    pub fn try_load_SkillLevel(self:&Arc<Self>,k0:i32,k1:i32,k2:i32,cancellation:&Cancellation)->ServerResult<Option<SkillLevelRow>>{cancellation.check()?;let table=self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"SkillLevel"))?;let index=match table.key.find(k0,k1,k2){Some(slot)=>table.key.index.row(slot),None=>return Ok(None)};Ok(Some(R::RowHandle{snapshot:self.clone(),row:table.rows.load(index,cancellation)?}))}
    pub fn load_SkillLevel(self:&Arc<Self>,k0:i32,k1:i32,k2:i32,cancellation:&Cancellation)->ServerResult<SkillLevelRow>{self.try_load_SkillLevel(k0,k1,k2,cancellation)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,"Row key not found"))}
    fn reference_SkillLevel(self:&Arc<Self>,id:i32,cancellation:Option<&Cancellation>)->ServerResult<Option<SkillLevelRow>>{if id==0{return Ok(None)}let table=self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"SkillLevel"))?;let index=table.rows.indexed.reference(id)?;let row=if let Some(token)=cancellation{table.rows.load(index,token)?}else{table.rows.get(index)?};Ok(Some(R::RowHandle{snapshot:self.clone(),row}))}
    fn ordinal_SkillLevel(&self,index:usize)->ServerResult<Arc<SkillLevelData>>{let table=self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"SkillLevel"))?;table.rows.get(index)}
    pub fn rows_SkillLevel(self:&Arc<Self>)->ServerResult<R::DataGroup<Self,SkillLevelData>>{let table=self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"SkillLevel"))?;let positions=table.all.get().ok_or_else(||ConfigError::new(ErrorCode::TableNotLoaded,"Load the table explicitly"))?.clone();Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_SkillLevel})}
    pub fn load_table_SkillLevel(self:&Arc<Self>,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,SkillLevelData>>{let table=self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"SkillLevel"))?;table.load_all(cancellation)?;self.rows_SkillLevel()}
    pub fn find_group_SkillLevel_ByLevel(self:&Arc<Self>,k0:i32)->ServerResult<R::DataGroup<Self,SkillLevelData>>{let table=self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"SkillLevel"))?;let positions=table.group_ByLevel(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_SkillLevel})}
    pub fn load_group_SkillLevel_ByLevel(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,SkillLevelData>>{cancellation.check()?;let table=self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"SkillLevel"))?;let positions=table.group_ByLevel(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_SkillLevel})}
    pub fn find_group_SkillLevel_ByQuality(self:&Arc<Self>,k0:i32)->ServerResult<R::DataGroup<Self,SkillLevelData>>{let table=self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"SkillLevel"))?;let positions=table.group_ByQuality(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_SkillLevel})}
    pub fn load_group_SkillLevel_ByQuality(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,SkillLevelData>>{cancellation.check()?;let table=self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"SkillLevel"))?;let positions=table.group_ByQuality(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_SkillLevel})}
    pub fn find_group_SkillLevel_BySkillId(self:&Arc<Self>,k0:i32)->ServerResult<R::DataGroup<Self,SkillLevelData>>{let table=self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"SkillLevel"))?;let positions=table.group_BySkillId(k0,None)?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_SkillLevel})}
    pub fn load_group_SkillLevel_BySkillId(self:&Arc<Self>,k0:i32,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,SkillLevelData>>{cancellation.check()?;let table=self.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,"SkillLevel"))?;let positions=table.group_BySkillId(k0,Some(cancellation))?;Ok(R::DataGroup{snapshot:self.clone(),positions,query:Self::ordinal_SkillLevel})}
}
