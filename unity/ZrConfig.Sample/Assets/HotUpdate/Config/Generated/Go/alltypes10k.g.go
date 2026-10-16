// zrconfig-api-contract:1
// 此文件由工具自动生成，请勿手工修改。
package zrconfig

type AllTypes10kConfig struct {
    serverOwner *ConfigSnapshot
    v_Id int32
    v_ByteValue uint8
    v_ShortValue int16
    v_IntValue int32
    v_LongValue int64
    v_FloatValue float32
    v_DoubleValue float64
    v_BoolValue bool
    v_StringValue string
    v_SByteValue int8
    v_UShortValue uint16
    v_UIntValue uint32
    v_ULongValue uint64
    v_DecimalValue Decimal
    v_QualityValue T_Game_Config_Quality
    v_Vector2Value T_ZrConfig_Types_Vector2
    v_Vector3Value T_ZrConfig_Types_Vector3
    v_KvValue T_ZrConfig_Types_Kv
    v_MoneyValue T_Game_Config_Money
    v_ByteArray List[uint8]
    v_ShortArray List[int16]
    v_IntArray List[int32]
    v_LongArray List[int64]
    v_FloatArray List[float32]
    v_DoubleArray List[float64]
    v_BoolArray List[bool]
    v_StringArray List[string]
    v_SByteArray List[int8]
    v_UShortArray List[uint16]
    v_UIntArray List[uint32]
    v_ULongArray List[uint64]
    v_DecimalArray List[Decimal]
    v_QualityArray List[T_Game_Config_Quality]
    v_Vector2Array List[T_ZrConfig_Types_Vector2]
    v_Vector3Array List[T_ZrConfig_Types_Vector3]
    v_KvArray List[T_ZrConfig_Types_Kv]
    v_MoneyArray List[T_Game_Config_Money]
    v_ByteArray2D List[List[uint8]]
    v_ShortArray2D List[List[int16]]
    v_IntArray2D List[List[int32]]
    v_LongArray2D List[List[int64]]
    v_FloatArray2D List[List[float32]]
    v_DoubleArray2D List[List[float64]]
    v_BoolArray2D List[List[bool]]
    v_StringArray2D List[List[string]]
    v_SByteArray2D List[List[int8]]
    v_UShortArray2D List[List[uint16]]
    v_UIntArray2D List[List[uint32]]
    v_ULongArray2D List[List[uint64]]
    v_DecimalArray2D List[List[Decimal]]
    v_QualityArray2D List[List[T_Game_Config_Quality]]
    v_Vector2Array2D List[List[T_ZrConfig_Types_Vector2]]
    v_Vector3Array2D List[List[T_ZrConfig_Types_Vector3]]
    v_KvArray2D List[List[T_ZrConfig_Types_Kv]]
    v_MoneyArray2D List[List[T_Game_Config_Money]]
    v_ByteArray3D List[List[List[uint8]]]
    v_ShortArray3D List[List[List[int16]]]
    v_IntArray3D List[List[List[int32]]]
    v_LongArray3D List[List[List[int64]]]
    v_FloatArray3D List[List[List[float32]]]
    v_DoubleArray3D List[List[List[float64]]]
    v_BoolArray3D List[List[List[bool]]]
    v_StringArray3D List[List[List[string]]]
    v_SByteArray3D List[List[List[int8]]]
    v_UShortArray3D List[List[List[uint16]]]
    v_UIntArray3D List[List[List[uint32]]]
    v_ULongArray3D List[List[List[uint64]]]
    v_DecimalArray3D List[List[List[Decimal]]]
    v_QualityArray3D List[List[List[T_Game_Config_Quality]]]
    v_Vector2Array3D List[List[List[T_ZrConfig_Types_Vector2]]]
    v_Vector3Array3D List[List[List[T_ZrConfig_Types_Vector3]]]
    v_KvArray3D List[List[List[T_ZrConfig_Types_Kv]]]
    v_MoneyArray3D List[List[List[T_Game_Config_Money]]]
    v_Settings *T_Game_Config_BattleSettings
    v_Range T_Game_Config_ConfigRange
    v_Options *T_Game_Config_SampleOptions
    v_Action T_Game_Config_BattleAction
    v_Actions List[T_Game_Config_BattleAction]
    v_Ranges List[T_Game_Config_ConfigRange]
    v_OptionsList List[*T_Game_Config_SampleOptions]
    v_Groups List[*T_Game_Config_ActionGroup]
    v_Phases List[*T_Game_Config_BattlePhase]
    v_OptionalRange Optional[T_Game_Config_ConfigRange]
    v_OptionalInt Optional[int32]
    v_OptionalUInt Optional[uint32]
    v_OptionalULong Optional[uint64]
    v_OptionalDecimal Optional[Decimal]
    v_OptionalBool Optional[bool]
    v_OptionalQuality Optional[T_Game_Config_Quality]
    v_OptionalMoney Optional[T_Game_Config_Money]
    v_OptionalValues List[Optional[int32]]
    v_StatMap Dictionary[string,int32]
    v_PriceMap Dictionary[int32,Optional[Decimal]]
    v_QualityMap Dictionary[T_Game_Config_Quality,uint32]
    v_ListMap Dictionary[string,List[int32]]
    v_IntMatrix Matrix[int32]
    v_DecimalCube Matrix[Decimal]
    v_OptionalMatrix Matrix[Optional[uint32]]
    v_RawJsonValue string
    v_RawJsonArray List[string]
    v_RawJsonArray2D List[List[string]]
    v_RawJsonArray3D List[List[List[string]]]
    v_JsonIntArray List[int32]
    v_Hero int32
    ref_Hero *HeroConfig
    v_Skill int32
    ref_Skill *SkillConfig
    v_Previous int32
    ref_Previous *AllTypes10kConfig
}
// 唯一主键：1～10000
func(v *AllTypes10kConfig) Id() int32 { return v.v_Id }
// Byte；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ByteValue() uint8 { return v.v_ByteValue }
// Short；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ShortValue() int16 { return v.v_ShortValue }
// Int；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) IntValue() int32 { return v.v_IntValue }
// Long；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) LongValue() int64 { return v.v_LongValue }
// Float；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) FloatValue() float32 { return v.v_FloatValue }
// Double；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) DoubleValue() float64 { return v.v_DoubleValue }
// Bool；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) BoolValue() bool { return v.v_BoolValue }
// String；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) StringValue() string { return v.v_StringValue }
// SByte；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) SByteValue() int8 { return v.v_SByteValue }
// UShort；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) UShortValue() uint16 { return v.v_UShortValue }
// UInt；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) UIntValue() uint32 { return v.v_UIntValue }
// ULong；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ULongValue() uint64 { return v.v_ULongValue }
// Decimal；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) DecimalValue() Decimal { return v.v_DecimalValue }
// Quality；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) QualityValue() T_Game_Config_Quality { return v.v_QualityValue }
// Vector2；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) Vector2Value() T_ZrConfig_Types_Vector2 { return v.v_Vector2Value }
// Vector3；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) Vector3Value() T_ZrConfig_Types_Vector3 { return v.v_Vector3Value }
// Kv；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) KvValue() T_ZrConfig_Types_Kv { return v.v_KvValue }
// Money；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) MoneyValue() T_Game_Config_Money { return v.v_MoneyValue }
// Byte；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ByteArray() List[uint8] { return v.v_ByteArray }
// Short；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ShortArray() List[int16] { return v.v_ShortArray }
// Int；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) IntArray() List[int32] { return v.v_IntArray }
// Long；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) LongArray() List[int64] { return v.v_LongArray }
// Float；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) FloatArray() List[float32] { return v.v_FloatArray }
// Double；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) DoubleArray() List[float64] { return v.v_DoubleArray }
// Bool；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) BoolArray() List[bool] { return v.v_BoolArray }
// String；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) StringArray() List[string] { return v.v_StringArray }
// SByte；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) SByteArray() List[int8] { return v.v_SByteArray }
// UShort；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) UShortArray() List[uint16] { return v.v_UShortArray }
// UInt；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) UIntArray() List[uint32] { return v.v_UIntArray }
// ULong；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ULongArray() List[uint64] { return v.v_ULongArray }
// Decimal；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) DecimalArray() List[Decimal] { return v.v_DecimalArray }
// Quality；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) QualityArray() List[T_Game_Config_Quality] { return v.v_QualityArray }
// Vector2；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) Vector2Array() List[T_ZrConfig_Types_Vector2] { return v.v_Vector2Array }
// Vector3；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) Vector3Array() List[T_ZrConfig_Types_Vector3] { return v.v_Vector3Array }
// Kv；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) KvArray() List[T_ZrConfig_Types_Kv] { return v.v_KvArray }
// Money；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) MoneyArray() List[T_Game_Config_Money] { return v.v_MoneyArray }
// Byte；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ByteArray2D() List[List[uint8]] { return v.v_ByteArray2D }
// Short；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ShortArray2D() List[List[int16]] { return v.v_ShortArray2D }
// Int；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) IntArray2D() List[List[int32]] { return v.v_IntArray2D }
// Long；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) LongArray2D() List[List[int64]] { return v.v_LongArray2D }
// Float；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) FloatArray2D() List[List[float32]] { return v.v_FloatArray2D }
// Double；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) DoubleArray2D() List[List[float64]] { return v.v_DoubleArray2D }
// Bool；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) BoolArray2D() List[List[bool]] { return v.v_BoolArray2D }
// String；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) StringArray2D() List[List[string]] { return v.v_StringArray2D }
// SByte；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) SByteArray2D() List[List[int8]] { return v.v_SByteArray2D }
// UShort；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) UShortArray2D() List[List[uint16]] { return v.v_UShortArray2D }
// UInt；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) UIntArray2D() List[List[uint32]] { return v.v_UIntArray2D }
// ULong；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ULongArray2D() List[List[uint64]] { return v.v_ULongArray2D }
// Decimal；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) DecimalArray2D() List[List[Decimal]] { return v.v_DecimalArray2D }
// Quality；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) QualityArray2D() List[List[T_Game_Config_Quality]] { return v.v_QualityArray2D }
// Vector2；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) Vector2Array2D() List[List[T_ZrConfig_Types_Vector2]] { return v.v_Vector2Array2D }
// Vector3；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) Vector3Array2D() List[List[T_ZrConfig_Types_Vector3]] { return v.v_Vector3Array2D }
// Kv；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) KvArray2D() List[List[T_ZrConfig_Types_Kv]] { return v.v_KvArray2D }
// Money；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) MoneyArray2D() List[List[T_Game_Config_Money]] { return v.v_MoneyArray2D }
// Byte；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ByteArray3D() List[List[List[uint8]]] { return v.v_ByteArray3D }
// Short；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ShortArray3D() List[List[List[int16]]] { return v.v_ShortArray3D }
// Int；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) IntArray3D() List[List[List[int32]]] { return v.v_IntArray3D }
// Long；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) LongArray3D() List[List[List[int64]]] { return v.v_LongArray3D }
// Float；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) FloatArray3D() List[List[List[float32]]] { return v.v_FloatArray3D }
// Double；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) DoubleArray3D() List[List[List[float64]]] { return v.v_DoubleArray3D }
// Bool；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) BoolArray3D() List[List[List[bool]]] { return v.v_BoolArray3D }
// String；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) StringArray3D() List[List[List[string]]] { return v.v_StringArray3D }
// SByte；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) SByteArray3D() List[List[List[int8]]] { return v.v_SByteArray3D }
// UShort；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) UShortArray3D() List[List[List[uint16]]] { return v.v_UShortArray3D }
// UInt；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) UIntArray3D() List[List[List[uint32]]] { return v.v_UIntArray3D }
// ULong；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ULongArray3D() List[List[List[uint64]]] { return v.v_ULongArray3D }
// Decimal；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) DecimalArray3D() List[List[List[Decimal]]] { return v.v_DecimalArray3D }
// Quality；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) QualityArray3D() List[List[List[T_Game_Config_Quality]]] { return v.v_QualityArray3D }
// Vector2；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) Vector2Array3D() List[List[List[T_ZrConfig_Types_Vector2]]] { return v.v_Vector2Array3D }
// Vector3；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) Vector3Array3D() List[List[List[T_ZrConfig_Types_Vector3]]] { return v.v_Vector3Array3D }
// Kv；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) KvArray3D() List[List[List[T_ZrConfig_Types_Kv]]] { return v.v_KvArray3D }
// Money；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) MoneyArray3D() List[List[List[T_Game_Config_Money]]] { return v.v_MoneyArray3D }
// 完整对象或列表；单格 JSON 输入，导出为声明的强类型
func(v *AllTypes10kConfig) Settings() *T_Game_Config_BattleSettings { return v.v_Settings }
// 完整对象或列表；单格 JSON 输入，导出为声明的强类型
func(v *AllTypes10kConfig) Range() T_Game_Config_ConfigRange { return v.v_Range }
// 完整对象或列表；单格 JSON 输入，导出为声明的强类型
func(v *AllTypes10kConfig) Options() *T_Game_Config_SampleOptions { return v.v_Options }
// 完整对象或列表；单格 JSON 输入，导出为声明的强类型
func(v *AllTypes10kConfig) Action() T_Game_Config_BattleAction { return v.v_Action }
// 完整对象或列表；单格 JSON 输入，导出为声明的强类型
func(v *AllTypes10kConfig) Actions() List[T_Game_Config_BattleAction] { return v.v_Actions }
// 完整对象或列表；单格 JSON 输入，导出为声明的强类型
func(v *AllTypes10kConfig) Ranges() List[T_Game_Config_ConfigRange] { return v.v_Ranges }
// 完整对象或列表；单格 JSON 输入，导出为声明的强类型
func(v *AllTypes10kConfig) OptionsList() List[*T_Game_Config_SampleOptions] { return v.v_OptionsList }
// 完整对象或列表；单格 JSON 输入，导出为声明的强类型
func(v *AllTypes10kConfig) Groups() List[*T_Game_Config_ActionGroup] { return v.v_Groups }
// 完整对象或列表；单格 JSON 输入，导出为声明的强类型
func(v *AllTypes10kConfig) Phases() List[*T_Game_Config_BattlePhase] { return v.v_Phases }
// 完整对象或列表；单格 JSON 输入，导出为声明的强类型
func(v *AllTypes10kConfig) OptionalRange() Optional[T_Game_Config_ConfigRange] { return v.v_OptionalRange }
// NullableInt；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) OptionalInt() Optional[int32] { return v.v_OptionalInt }
// NullableUInt；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) OptionalUInt() Optional[uint32] { return v.v_OptionalUInt }
// NullableULong；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) OptionalULong() Optional[uint64] { return v.v_OptionalULong }
// NullableDecimal；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) OptionalDecimal() Optional[Decimal] { return v.v_OptionalDecimal }
// NullableBool；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) OptionalBool() Optional[bool] { return v.v_OptionalBool }
// NullableQuality；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) OptionalQuality() Optional[T_Game_Config_Quality] { return v.v_OptionalQuality }
// NullableMoney；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) OptionalMoney() Optional[T_Game_Config_Money] { return v.v_OptionalMoney }
// NullableIntArray；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) OptionalValues() List[Optional[int32]] { return v.v_OptionalValues }
// MapInt；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) StatMap() Dictionary[string,int32] { return v.v_StatMap }
// MapNullableDecimal；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) PriceMap() Dictionary[int32,Optional[Decimal]] { return v.v_PriceMap }
// MapUInt；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) QualityMap() Dictionary[T_Game_Config_Quality,uint32] { return v.v_QualityMap }
// MapIntArray；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) ListMap() Dictionary[string,List[int32]] { return v.v_ListMap }
// MatrixInt；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) IntMatrix() Matrix[int32] { return v.v_IntMatrix }
// MatrixDecimal；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) DecimalCube() Matrix[Decimal] { return v.v_DecimalCube }
// MatrixNullableUInt；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) OptionalMatrix() Matrix[Optional[uint32]] { return v.v_OptionalMatrix }
// RawJson；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) RawJsonValue() string { return v.v_RawJsonValue }
// RawJson；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) RawJsonArray() List[string] { return v.v_RawJsonArray }
// RawJson；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) RawJsonArray2D() List[List[string]] { return v.v_RawJsonArray2D }
// RawJson；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) RawJsonArray3D() List[List[List[string]]] { return v.v_RawJsonArray3D }
// Int；按 Id 轮换常规、边界、默认值
func(v *AllTypes10kConfig) JsonIntArray() List[int32] { return v.v_JsonIntArray }
// 常规=1，边界=2，默认=0；引用 Hero 表
func(v *AllTypes10kConfig) HeroId() int32 { return v.v_Hero }
func(v *AllTypes10kConfig) Hero() *HeroConfig { return v.ref_Hero }
// 常规=10，边界=11，默认=0；引用 Skill 表
func(v *AllTypes10kConfig) SkillId() int32 { return v.v_Skill }
func(v *AllTypes10kConfig) Skill() *SkillConfig { return v.ref_Skill }
// Id=1 自引用，其余引用 Id-1；用于按需加载引用链
func(v *AllTypes10kConfig) PreviousId() int32 { return v.v_Previous }
func(v *AllTypes10kConfig) Previous() *AllTypes10kConfig { return v.ref_Previous }
const schemaAllTypes10k = "{\"name\":\"AllTypes10k\",\"hash\":\"34B94C4229D85CC8B37832A203504B7E6EF4447307ADB831AB5BD01628EA7432\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"唯一主键：1～10000\"},{\"name\":\"ByteValue\",\"type\":\"System.Byte\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortValue\",\"type\":\"System.Int16\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntValue\",\"type\":\"System.Int32\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongValue\",\"type\":\"System.Int64\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatValue\",\"type\":\"System.Single\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleValue\",\"type\":\"System.Double\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolValue\",\"type\":\"System.Boolean\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringValue\",\"type\":\"System.String\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteValue\",\"type\":\"System.SByte\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortValue\",\"type\":\"System.UInt16\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntValue\",\"type\":\"System.UInt32\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongValue\",\"type\":\"System.UInt64\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalValue\",\"type\":\"System.Decimal\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityValue\",\"type\":\"Game.Config.Quality\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Value\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Value\",\"type\":\"ZrConfig.Types.Vector3\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvValue\",\"type\":\"ZrConfig.Types.Kv\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyValue\",\"type\":\"Game.Config.Money\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ByteArray\",\"type\":\"System.Byte[]\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortArray\",\"type\":\"System.Int16[]\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntArray\",\"type\":\"System.Int32[]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongArray\",\"type\":\"System.Int64[]\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatArray\",\"type\":\"System.Single[]\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleArray\",\"type\":\"System.Double[]\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolArray\",\"type\":\"System.Boolean[]\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringArray\",\"type\":\"System.String[]\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteArray\",\"type\":\"System.SByte[]\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortArray\",\"type\":\"System.UInt16[]\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntArray\",\"type\":\"System.UInt32[]\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongArray\",\"type\":\"System.UInt64[]\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalArray\",\"type\":\"System.Decimal[]\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityArray\",\"type\":\"Game.Config.Quality[]\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Array\",\"type\":\"ZrConfig.Types.Vector2[]\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Array\",\"type\":\"ZrConfig.Types.Vector3[]\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvArray\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyArray\",\"type\":\"Game.Config.Money[]\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ByteArray2D\",\"type\":\"System.Byte[][]\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortArray2D\",\"type\":\"System.Int16[][]\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntArray2D\",\"type\":\"System.Int32[][]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongArray2D\",\"type\":\"System.Int64[][]\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatArray2D\",\"type\":\"System.Single[][]\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleArray2D\",\"type\":\"System.Double[][]\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolArray2D\",\"type\":\"System.Boolean[][]\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringArray2D\",\"type\":\"System.String[][]\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteArray2D\",\"type\":\"System.SByte[][]\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortArray2D\",\"type\":\"System.UInt16[][]\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntArray2D\",\"type\":\"System.UInt32[][]\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongArray2D\",\"type\":\"System.UInt64[][]\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalArray2D\",\"type\":\"System.Decimal[][]\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityArray2D\",\"type\":\"Game.Config.Quality[][]\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Array2D\",\"type\":\"ZrConfig.Types.Vector2[][]\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Array2D\",\"type\":\"ZrConfig.Types.Vector3[][]\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvArray2D\",\"type\":\"ZrConfig.Types.Kv[][]\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyArray2D\",\"type\":\"Game.Config.Money[][]\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ByteArray3D\",\"type\":\"System.Byte[][][]\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortArray3D\",\"type\":\"System.Int16[][][]\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntArray3D\",\"type\":\"System.Int32[][][]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongArray3D\",\"type\":\"System.Int64[][][]\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatArray3D\",\"type\":\"System.Single[][][]\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleArray3D\",\"type\":\"System.Double[][][]\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolArray3D\",\"type\":\"System.Boolean[][][]\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringArray3D\",\"type\":\"System.String[][][]\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteArray3D\",\"type\":\"System.SByte[][][]\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortArray3D\",\"type\":\"System.UInt16[][][]\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntArray3D\",\"type\":\"System.UInt32[][][]\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongArray3D\",\"type\":\"System.UInt64[][][]\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalArray3D\",\"type\":\"System.Decimal[][][]\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityArray3D\",\"type\":\"Game.Config.Quality[][][]\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Array3D\",\"type\":\"ZrConfig.Types.Vector2[][][]\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Array3D\",\"type\":\"ZrConfig.Types.Vector3[][][]\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvArray3D\",\"type\":\"ZrConfig.Types.Kv[][][]\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyArray3D\",\"type\":\"Game.Config.Money[][][]\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Settings\",\"type\":\"Game.Config.BattleSettings\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Options\",\"type\":\"Game.Config.SampleOptions\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Action\",\"type\":\"Game.Config.BattleAction\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Ranges\",\"type\":\"Game.Config.ConfigRange[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"OptionsList\",\"type\":\"Game.Config.SampleOptions[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Phases\",\"type\":\"Game.Config.BattlePhase[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"OptionalRange\",\"type\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"OptionalInt\",\"type\":\"System.Nullable`1<System.Int32>\",\"description\":\"NullableInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalUInt\",\"type\":\"System.Nullable`1<System.UInt32>\",\"description\":\"NullableUInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalULong\",\"type\":\"System.Nullable`1<System.UInt64>\",\"description\":\"NullableULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalDecimal\",\"type\":\"System.Nullable`1<System.Decimal>\",\"description\":\"NullableDecimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalBool\",\"type\":\"System.Nullable`1<System.Boolean>\",\"description\":\"NullableBool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalQuality\",\"type\":\"System.Nullable`1<Game.Config.Quality>\",\"description\":\"NullableQuality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalMoney\",\"type\":\"System.Nullable`1<Game.Config.Money>\",\"description\":\"NullableMoney；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalValues\",\"type\":\"System.Nullable`1<System.Int32>[]\",\"description\":\"NullableIntArray；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StatMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"description\":\"MapInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"PriceMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"description\":\"MapNullableDecimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityMap\",\"type\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"description\":\"MapUInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ListMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"description\":\"MapIntArray；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntMatrix\",\"type\":\"System.Int32[,]\",\"description\":\"MatrixInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalCube\",\"type\":\"System.Decimal[,,]\",\"description\":\"MatrixDecimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalMatrix\",\"type\":\"System.Nullable`1<System.UInt32>[,]\",\"description\":\"MatrixNullableUInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonValue\",\"type\":\"ZrConfig.Types.RawJson\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonArray\",\"type\":\"ZrConfig.Types.RawJson[]\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonArray2D\",\"type\":\"ZrConfig.Types.RawJson[][]\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonArray3D\",\"type\":\"ZrConfig.Types.RawJson[][][]\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"JsonIntArray\",\"type\":\"System.Int32[]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Hero\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"常规=1，边界=2，默认=0；引用 Hero 表\"},{\"name\":\"Skill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"常规=10，边界=11，默认=0；引用 Skill 表\"},{\"name\":\"Previous\",\"type\":\"ref:ZrConfig.Models.AllTypes10kConfig:Id:int:zero-null/1\",\"description\":\"Id=1 自引用，其余引用 Id-1；用于按需加载引用链\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"Game.Config.ActionGroup\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ActionGroup[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ActionGroup\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.AddBuff\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"BuffId\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Stacks\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"enumValues\":[],\"abstract\":true,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattleAction\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattlePhase\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleSettings\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"MaxTargets\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Interruptible\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Min\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Max\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Damage\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Ratio\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"CanCrit\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money\",\"kind\":\"converter\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Cents\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality\",\"kind\":\"enum\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[\"0\",\"1\",\"2\"],\"abstract\":false,\"baseType\":null,\"enumMembers\":{\"None\":\"0\",\"Rare\":\"1\",\"Legendary\":\"2\"},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"\"},{\"name\":\"Rewards\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"\"},{\"name\":\"Offset\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"\"},{\"name\":\"Label\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Quality\",\"type\":\"Game.Config.Quality\",\"description\":\"\"},{\"name\":\"Levels\",\"type\":\"System.Int32[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.SampleOptions\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte\",\"kind\":\"primitive\",\"primitive\":\"uint8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":\"Game.Config.Quality\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Nullable`1<System.Decimal>\",\"key\":\"System.Int32\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32[]\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal\",\"kind\":\"primitive\",\"primitive\":\"decimal\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[,,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":3,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double\",\"kind\":\"primitive\",\"primitive\":\"float64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16\",\"kind\":\"primitive\",\"primitive\":\"int16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64\",\"kind\":\"primitive\",\"primitive\":\"int64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Money>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Quality>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Boolean>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Decimal>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Nullable`1<System.Int32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Nullable`1<System.UInt32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt64>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte\",\"kind\":\"primitive\",\"primitive\":\"int8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single\",\"kind\":\"primitive\",\"primitive\":\"float32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16\",\"kind\":\"primitive\",\"primitive\":\"uint16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32\",\"kind\":\"primitive\",\"primitive\":\"uint32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64\",\"kind\":\"primitive\",\"primitive\":\"uint64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv\",\"kind\":\"kv\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Key\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Value\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson\",\"kind\":\"rawjson\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2\",\"kind\":\"vector2\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3\",\"kind\":\"vector3\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Z\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.AllTypes10kConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"AllTypes10k\"},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"},{\"id\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Skill\"}],\"tags\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"dependencies\":[\"AllTypes10k\",\"Hero\",\"Skill\"]}"
type AllTypes10kKey_Key struct {
    v_Id int32
}
type AllTypes10kKey_Group_ById struct {
    v_Id int32
}
type AllTypes10kTable struct {
    rows List[*AllTypes10kConfig]
    index_Key map[AllTypes10kKey_Key]*AllTypes10kConfig
    index_Group_ById map[AllTypes10kKey_Group_ById]List[*AllTypes10kConfig]
    ids map[int32]*AllTypes10kConfig
}
func loadAllTypes10k(source []any)*AllTypes10kTable {
    result:=&AllTypes10kTable{rows:List[*AllTypes10kConfig]{values:make([]*AllTypes10kConfig,len(source))}}
    result.index_Key=make(map[AllTypes10kKey_Key]*AllTypes10kConfig,len(source))
    result.index_Group_ById=make(map[AllTypes10kKey_Group_ById]List[*AllTypes10kConfig],len(source))
    result.ids=make(map[int32]*AllTypes10kConfig,len(source))
    for i,value:=range source {
        row:=&AllTypes10kConfig{
            v_Id: int32(signed(field(value,"Id"),32)),
            v_ByteValue: uint8(unsigned(field(value,"ByteValue"),8)),
            v_ShortValue: int16(signed(field(value,"ShortValue"),16)),
            v_IntValue: int32(signed(field(value,"IntValue"),32)),
            v_LongValue: int64(signed(field(value,"LongValue"),64)),
            v_FloatValue: float32(floating(field(value,"FloatValue"),32)),
            v_DoubleValue: float64(floating(field(value,"DoubleValue"),64)),
            v_BoolValue: boolean(field(value,"BoolValue")),
            v_StringValue: text(field(value,"StringValue")),
            v_SByteValue: int8(signed(field(value,"SByteValue"),8)),
            v_UShortValue: uint16(unsigned(field(value,"UShortValue"),16)),
            v_UIntValue: uint32(unsigned(field(value,"UIntValue"),32)),
            v_ULongValue: uint64(unsigned(field(value,"ULongValue"),64)),
            v_DecimalValue: decimalValue(text(field(value,"DecimalValue"))),
            v_QualityValue: T_Game_Config_Quality(int32(signed(field(value,"QualityValue"),32))),
            v_Vector2Value: read_T_ZrConfig_Types_Vector2(field(value,"Vector2Value")),
            v_Vector3Value: read_T_ZrConfig_Types_Vector3(field(value,"Vector3Value")),
            v_KvValue: read_T_ZrConfig_Types_Kv(field(value,"KvValue")),
            v_MoneyValue: read_T_Game_Config_Money(field(value,"MoneyValue")),
            v_ByteArray: decodeList(field(value,"ByteArray"),func(item0 any) uint8 { return uint8(unsigned(item0,8)) }),
            v_ShortArray: decodeList(field(value,"ShortArray"),func(item0 any) int16 { return int16(signed(item0,16)) }),
            v_IntArray: decodeList(field(value,"IntArray"),func(item0 any) int32 { return int32(signed(item0,32)) }),
            v_LongArray: decodeList(field(value,"LongArray"),func(item0 any) int64 { return int64(signed(item0,64)) }),
            v_FloatArray: decodeList(field(value,"FloatArray"),func(item0 any) float32 { return float32(floating(item0,32)) }),
            v_DoubleArray: decodeList(field(value,"DoubleArray"),func(item0 any) float64 { return float64(floating(item0,64)) }),
            v_BoolArray: decodeList(field(value,"BoolArray"),func(item0 any) bool { return boolean(item0) }),
            v_StringArray: decodeList(field(value,"StringArray"),func(item0 any) string { return text(item0) }),
            v_SByteArray: decodeList(field(value,"SByteArray"),func(item0 any) int8 { return int8(signed(item0,8)) }),
            v_UShortArray: decodeList(field(value,"UShortArray"),func(item0 any) uint16 { return uint16(unsigned(item0,16)) }),
            v_UIntArray: decodeList(field(value,"UIntArray"),func(item0 any) uint32 { return uint32(unsigned(item0,32)) }),
            v_ULongArray: decodeList(field(value,"ULongArray"),func(item0 any) uint64 { return uint64(unsigned(item0,64)) }),
            v_DecimalArray: decodeList(field(value,"DecimalArray"),func(item0 any) Decimal { return decimalValue(text(item0)) }),
            v_QualityArray: decodeList(field(value,"QualityArray"),func(item0 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item0,32))) }),
            v_Vector2Array: decodeList(field(value,"Vector2Array"),func(item0 any) T_ZrConfig_Types_Vector2 { return read_T_ZrConfig_Types_Vector2(item0) }),
            v_Vector3Array: decodeList(field(value,"Vector3Array"),func(item0 any) T_ZrConfig_Types_Vector3 { return read_T_ZrConfig_Types_Vector3(item0) }),
            v_KvArray: decodeList(field(value,"KvArray"),func(item0 any) T_ZrConfig_Types_Kv { return read_T_ZrConfig_Types_Kv(item0) }),
            v_MoneyArray: decodeList(field(value,"MoneyArray"),func(item0 any) T_Game_Config_Money { return read_T_Game_Config_Money(item0) }),
            v_ByteArray2D: decodeList(field(value,"ByteArray2D"),func(item0 any) List[uint8] { return decodeList(item0,func(item1 any) uint8 { return uint8(unsigned(item1,8)) }) }),
            v_ShortArray2D: decodeList(field(value,"ShortArray2D"),func(item0 any) List[int16] { return decodeList(item0,func(item1 any) int16 { return int16(signed(item1,16)) }) }),
            v_IntArray2D: decodeList(field(value,"IntArray2D"),func(item0 any) List[int32] { return decodeList(item0,func(item1 any) int32 { return int32(signed(item1,32)) }) }),
            v_LongArray2D: decodeList(field(value,"LongArray2D"),func(item0 any) List[int64] { return decodeList(item0,func(item1 any) int64 { return int64(signed(item1,64)) }) }),
            v_FloatArray2D: decodeList(field(value,"FloatArray2D"),func(item0 any) List[float32] { return decodeList(item0,func(item1 any) float32 { return float32(floating(item1,32)) }) }),
            v_DoubleArray2D: decodeList(field(value,"DoubleArray2D"),func(item0 any) List[float64] { return decodeList(item0,func(item1 any) float64 { return float64(floating(item1,64)) }) }),
            v_BoolArray2D: decodeList(field(value,"BoolArray2D"),func(item0 any) List[bool] { return decodeList(item0,func(item1 any) bool { return boolean(item1) }) }),
            v_StringArray2D: decodeList(field(value,"StringArray2D"),func(item0 any) List[string] { return decodeList(item0,func(item1 any) string { return text(item1) }) }),
            v_SByteArray2D: decodeList(field(value,"SByteArray2D"),func(item0 any) List[int8] { return decodeList(item0,func(item1 any) int8 { return int8(signed(item1,8)) }) }),
            v_UShortArray2D: decodeList(field(value,"UShortArray2D"),func(item0 any) List[uint16] { return decodeList(item0,func(item1 any) uint16 { return uint16(unsigned(item1,16)) }) }),
            v_UIntArray2D: decodeList(field(value,"UIntArray2D"),func(item0 any) List[uint32] { return decodeList(item0,func(item1 any) uint32 { return uint32(unsigned(item1,32)) }) }),
            v_ULongArray2D: decodeList(field(value,"ULongArray2D"),func(item0 any) List[uint64] { return decodeList(item0,func(item1 any) uint64 { return uint64(unsigned(item1,64)) }) }),
            v_DecimalArray2D: decodeList(field(value,"DecimalArray2D"),func(item0 any) List[Decimal] { return decodeList(item0,func(item1 any) Decimal { return decimalValue(text(item1)) }) }),
            v_QualityArray2D: decodeList(field(value,"QualityArray2D"),func(item0 any) List[T_Game_Config_Quality] { return decodeList(item0,func(item1 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item1,32))) }) }),
            v_Vector2Array2D: decodeList(field(value,"Vector2Array2D"),func(item0 any) List[T_ZrConfig_Types_Vector2] { return decodeList(item0,func(item1 any) T_ZrConfig_Types_Vector2 { return read_T_ZrConfig_Types_Vector2(item1) }) }),
            v_Vector3Array2D: decodeList(field(value,"Vector3Array2D"),func(item0 any) List[T_ZrConfig_Types_Vector3] { return decodeList(item0,func(item1 any) T_ZrConfig_Types_Vector3 { return read_T_ZrConfig_Types_Vector3(item1) }) }),
            v_KvArray2D: decodeList(field(value,"KvArray2D"),func(item0 any) List[T_ZrConfig_Types_Kv] { return decodeList(item0,func(item1 any) T_ZrConfig_Types_Kv { return read_T_ZrConfig_Types_Kv(item1) }) }),
            v_MoneyArray2D: decodeList(field(value,"MoneyArray2D"),func(item0 any) List[T_Game_Config_Money] { return decodeList(item0,func(item1 any) T_Game_Config_Money { return read_T_Game_Config_Money(item1) }) }),
            v_ByteArray3D: decodeList(field(value,"ByteArray3D"),func(item0 any) List[List[uint8]] { return decodeList(item0,func(item1 any) List[uint8] { return decodeList(item1,func(item2 any) uint8 { return uint8(unsigned(item2,8)) }) }) }),
            v_ShortArray3D: decodeList(field(value,"ShortArray3D"),func(item0 any) List[List[int16]] { return decodeList(item0,func(item1 any) List[int16] { return decodeList(item1,func(item2 any) int16 { return int16(signed(item2,16)) }) }) }),
            v_IntArray3D: decodeList(field(value,"IntArray3D"),func(item0 any) List[List[int32]] { return decodeList(item0,func(item1 any) List[int32] { return decodeList(item1,func(item2 any) int32 { return int32(signed(item2,32)) }) }) }),
            v_LongArray3D: decodeList(field(value,"LongArray3D"),func(item0 any) List[List[int64]] { return decodeList(item0,func(item1 any) List[int64] { return decodeList(item1,func(item2 any) int64 { return int64(signed(item2,64)) }) }) }),
            v_FloatArray3D: decodeList(field(value,"FloatArray3D"),func(item0 any) List[List[float32]] { return decodeList(item0,func(item1 any) List[float32] { return decodeList(item1,func(item2 any) float32 { return float32(floating(item2,32)) }) }) }),
            v_DoubleArray3D: decodeList(field(value,"DoubleArray3D"),func(item0 any) List[List[float64]] { return decodeList(item0,func(item1 any) List[float64] { return decodeList(item1,func(item2 any) float64 { return float64(floating(item2,64)) }) }) }),
            v_BoolArray3D: decodeList(field(value,"BoolArray3D"),func(item0 any) List[List[bool]] { return decodeList(item0,func(item1 any) List[bool] { return decodeList(item1,func(item2 any) bool { return boolean(item2) }) }) }),
            v_StringArray3D: decodeList(field(value,"StringArray3D"),func(item0 any) List[List[string]] { return decodeList(item0,func(item1 any) List[string] { return decodeList(item1,func(item2 any) string { return text(item2) }) }) }),
            v_SByteArray3D: decodeList(field(value,"SByteArray3D"),func(item0 any) List[List[int8]] { return decodeList(item0,func(item1 any) List[int8] { return decodeList(item1,func(item2 any) int8 { return int8(signed(item2,8)) }) }) }),
            v_UShortArray3D: decodeList(field(value,"UShortArray3D"),func(item0 any) List[List[uint16]] { return decodeList(item0,func(item1 any) List[uint16] { return decodeList(item1,func(item2 any) uint16 { return uint16(unsigned(item2,16)) }) }) }),
            v_UIntArray3D: decodeList(field(value,"UIntArray3D"),func(item0 any) List[List[uint32]] { return decodeList(item0,func(item1 any) List[uint32] { return decodeList(item1,func(item2 any) uint32 { return uint32(unsigned(item2,32)) }) }) }),
            v_ULongArray3D: decodeList(field(value,"ULongArray3D"),func(item0 any) List[List[uint64]] { return decodeList(item0,func(item1 any) List[uint64] { return decodeList(item1,func(item2 any) uint64 { return uint64(unsigned(item2,64)) }) }) }),
            v_DecimalArray3D: decodeList(field(value,"DecimalArray3D"),func(item0 any) List[List[Decimal]] { return decodeList(item0,func(item1 any) List[Decimal] { return decodeList(item1,func(item2 any) Decimal { return decimalValue(text(item2)) }) }) }),
            v_QualityArray3D: decodeList(field(value,"QualityArray3D"),func(item0 any) List[List[T_Game_Config_Quality]] { return decodeList(item0,func(item1 any) List[T_Game_Config_Quality] { return decodeList(item1,func(item2 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item2,32))) }) }) }),
            v_Vector2Array3D: decodeList(field(value,"Vector2Array3D"),func(item0 any) List[List[T_ZrConfig_Types_Vector2]] { return decodeList(item0,func(item1 any) List[T_ZrConfig_Types_Vector2] { return decodeList(item1,func(item2 any) T_ZrConfig_Types_Vector2 { return read_T_ZrConfig_Types_Vector2(item2) }) }) }),
            v_Vector3Array3D: decodeList(field(value,"Vector3Array3D"),func(item0 any) List[List[T_ZrConfig_Types_Vector3]] { return decodeList(item0,func(item1 any) List[T_ZrConfig_Types_Vector3] { return decodeList(item1,func(item2 any) T_ZrConfig_Types_Vector3 { return read_T_ZrConfig_Types_Vector3(item2) }) }) }),
            v_KvArray3D: decodeList(field(value,"KvArray3D"),func(item0 any) List[List[T_ZrConfig_Types_Kv]] { return decodeList(item0,func(item1 any) List[T_ZrConfig_Types_Kv] { return decodeList(item1,func(item2 any) T_ZrConfig_Types_Kv { return read_T_ZrConfig_Types_Kv(item2) }) }) }),
            v_MoneyArray3D: decodeList(field(value,"MoneyArray3D"),func(item0 any) List[List[T_Game_Config_Money]] { return decodeList(item0,func(item1 any) List[T_Game_Config_Money] { return decodeList(item1,func(item2 any) T_Game_Config_Money { return read_T_Game_Config_Money(item2) }) }) }),
            v_Settings: read_T_Game_Config_BattleSettings(field(value,"Settings")),
            v_Range: read_T_Game_Config_ConfigRange(field(value,"Range")),
            v_Options: read_T_Game_Config_SampleOptions(field(value,"Options")),
            v_Action: read_T_Game_Config_BattleAction(field(value,"Action")),
            v_Actions: decodeList(field(value,"Actions"),func(item0 any) T_Game_Config_BattleAction { return read_T_Game_Config_BattleAction(item0) }),
            v_Ranges: decodeList(field(value,"Ranges"),func(item0 any) T_Game_Config_ConfigRange { return read_T_Game_Config_ConfigRange(item0) }),
            v_OptionsList: decodeList(field(value,"OptionsList"),func(item0 any) *T_Game_Config_SampleOptions { return read_T_Game_Config_SampleOptions(item0) }),
            v_Groups: decodeList(field(value,"Groups"),func(item0 any) *T_Game_Config_ActionGroup { return read_T_Game_Config_ActionGroup(item0) }),
            v_Phases: decodeList(field(value,"Phases"),func(item0 any) *T_Game_Config_BattlePhase { return read_T_Game_Config_BattlePhase(item0) }),
            v_OptionalRange: decodeOptional(field(value,"OptionalRange"),func(item0 any) T_Game_Config_ConfigRange { return read_T_Game_Config_ConfigRange(item0) }),
            v_OptionalInt: decodeOptional(field(value,"OptionalInt"),func(item0 any) int32 { return int32(signed(item0,32)) }),
            v_OptionalUInt: decodeOptional(field(value,"OptionalUInt"),func(item0 any) uint32 { return uint32(unsigned(item0,32)) }),
            v_OptionalULong: decodeOptional(field(value,"OptionalULong"),func(item0 any) uint64 { return uint64(unsigned(item0,64)) }),
            v_OptionalDecimal: decodeOptional(field(value,"OptionalDecimal"),func(item0 any) Decimal { return decimalValue(text(item0)) }),
            v_OptionalBool: decodeOptional(field(value,"OptionalBool"),func(item0 any) bool { return boolean(item0) }),
            v_OptionalQuality: decodeOptional(field(value,"OptionalQuality"),func(item0 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item0,32))) }),
            v_OptionalMoney: decodeOptional(field(value,"OptionalMoney"),func(item0 any) T_Game_Config_Money { return read_T_Game_Config_Money(item0) }),
            v_OptionalValues: decodeList(field(value,"OptionalValues"),func(item0 any) Optional[int32] { return decodeOptional(item0,func(item1 any) int32 { return int32(signed(item1,32)) }) }),
            v_StatMap: decodeDictionary(field(value,"StatMap"),func(item0 any) string { return text(item0) },func(item0 any) int32 { return int32(signed(item0,32)) }),
            v_PriceMap: decodeDictionary(field(value,"PriceMap"),func(item0 any) int32 { return int32(signed(item0,32)) },func(item0 any) Optional[Decimal] { return decodeOptional(item0,func(item1 any) Decimal { return decimalValue(text(item1)) }) }),
            v_QualityMap: decodeDictionary(field(value,"QualityMap"),func(item0 any) T_Game_Config_Quality { return T_Game_Config_Quality(int32(signed(item0,32))) },func(item0 any) uint32 { return uint32(unsigned(item0,32)) }),
            v_ListMap: decodeDictionary(field(value,"ListMap"),func(item0 any) string { return text(item0) },func(item0 any) List[int32] { return decodeList(item0,func(item1 any) int32 { return int32(signed(item1,32)) }) }),
            v_IntMatrix: decodeMatrix(field(value,"IntMatrix"),func(item0 any) int32 { return int32(signed(item0,32)) }),
            v_DecimalCube: decodeMatrix(field(value,"DecimalCube"),func(item0 any) Decimal { return decimalValue(text(item0)) }),
            v_OptionalMatrix: decodeMatrix(field(value,"OptionalMatrix"),func(item0 any) Optional[uint32] { return decodeOptional(item0,func(item1 any) uint32 { return uint32(unsigned(item1,32)) }) }),
            v_RawJsonValue: text(field(value,"RawJsonValue")),
            v_RawJsonArray: decodeList(field(value,"RawJsonArray"),func(item0 any) string { return text(item0) }),
            v_RawJsonArray2D: decodeList(field(value,"RawJsonArray2D"),func(item0 any) List[string] { return decodeList(item0,func(item1 any) string { return text(item1) }) }),
            v_RawJsonArray3D: decodeList(field(value,"RawJsonArray3D"),func(item0 any) List[List[string]] { return decodeList(item0,func(item1 any) List[string] { return decodeList(item1,func(item2 any) string { return text(item2) }) }) }),
            v_JsonIntArray: decodeList(field(value,"JsonIntArray"),func(item0 any) int32 { return int32(signed(item0,32)) }),
            v_Hero: int32(signed(field(value,"Hero"),32)),
            v_Skill: int32(signed(field(value,"Skill"),32)),
            v_Previous: int32(signed(field(value,"Previous"),32)),
        }
        result.rows.values[i]=row
        key_Key:=AllTypes10kKey_Key{row.v_Id}
        if _,ok:=result.index_Key[key_Key];ok{invalid("主键重复")}
        result.index_Key[key_Key]=row
        key_Group_ById:=AllTypes10kKey_Group_ById{row.v_Id}
        group_Group_ById:=result.index_Group_ById[key_Group_ById]
        group_Group_ById.values=append(group_Group_ById.values,row)
        result.index_Group_ById[key_Group_ById]=group_Group_ById
        if _,ok:=result.ids[row.v_Id];ok{invalid("引用 Id 不唯一")}
        result.ids[row.v_Id]=row
    }
    return result
}
func(t *AllTypes10kTable)bind(snapshot *ConfigSnapshot){
    for _,row:=range t.rows.values{if row.v_Hero!=0{row.ref_Hero=snapshot.table_Hero.ids[row.v_Hero];if row.ref_Hero==nil{invalid("引用目标不存在：AllTypes10k.Hero")}}}
    for _,row:=range t.rows.values{if row.v_Skill!=0{row.ref_Skill=snapshot.table_Skill.ids[row.v_Skill];if row.ref_Skill==nil{invalid("引用目标不存在：AllTypes10k.Skill")}}}
    for _,row:=range t.rows.values{if row.v_Previous!=0{row.ref_Previous=snapshot.table_AllTypes10k.ids[row.v_Previous];if row.ref_Previous==nil{invalid("引用目标不存在：AllTypes10k.Previous")}}}
}
func(t *AllTypes10kTable) Rows()List[*AllTypes10kConfig]{return t.rows}
func(t *AllTypes10kTable) Count()int{return t.rows.Len()}
func(t *AllTypes10kTable) Find(k0 int32)(*AllTypes10kConfig,bool){value,ok:=t.index_Key[AllTypes10kKey_Key{k0}];return value,ok}
func(t *AllTypes10kTable) Get(k0 int32)*AllTypes10kConfig{value:=t.index_Key[AllTypes10kKey_Key{k0}];if value==nil{invalid("配置主键不存在")};return value}
func(t *AllTypes10kTable) GetById(k0 int32)List[*AllTypes10kConfig]{return t.index_Group_ById[AllTypes10kKey_Group_ById{k0}]}
