// zrconfig-api-contract:1
// 此文件由工具自动生成，请勿手工修改。
package zrconfig

type AllTypesConfig struct {
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
}
// 1=常规值；2=边界值；3=默认值。重复 Id 的多行组成同一条配置。
func(v *AllTypesConfig) Id() int32 { return v.v_Id }
// 基础整数；标量
func(v *AllTypesConfig) ByteValue() uint8 { return v.v_ByteValue }
// 基础整数；标量
func(v *AllTypesConfig) ShortValue() int16 { return v.v_ShortValue }
// 基础整数；标量
func(v *AllTypesConfig) IntValue() int32 { return v.v_IntValue }
// 基础整数；标量；文本存储，保留完整精度
func(v *AllTypesConfig) LongValue() int64 { return v.v_LongValue }
// 基础浮点；标量
func(v *AllTypesConfig) FloatValue() float32 { return v.v_FloatValue }
// 基础浮点；标量
func(v *AllTypesConfig) DoubleValue() float64 { return v.v_DoubleValue }
// 基础布尔；标量
func(v *AllTypesConfig) BoolValue() bool { return v.v_BoolValue }
// 基础字符串；标量
func(v *AllTypesConfig) StringValue() string { return v.v_StringValue }
// 基础整数；标量
func(v *AllTypesConfig) SByteValue() int8 { return v.v_SByteValue }
// 基础整数；标量
func(v *AllTypesConfig) UShortValue() uint16 { return v.v_UShortValue }
// 基础整数；标量
func(v *AllTypesConfig) UIntValue() uint32 { return v.v_UIntValue }
// 基础整数；标量；文本存储，保留完整精度
func(v *AllTypesConfig) ULongValue() uint64 { return v.v_ULongValue }
// 精确十进制；标量；文本存储，保留完整精度
func(v *AllTypesConfig) DecimalValue() Decimal { return v.v_DecimalValue }
// 枚举；标量
func(v *AllTypesConfig) QualityValue() T_Game_Config_Quality { return v.v_QualityValue }
// 二维向量；标量
func(v *AllTypesConfig) Vector2Value() T_ZrConfig_Types_Vector2 { return v.v_Vector2Value }
// 三维向量；标量
func(v *AllTypesConfig) Vector3Value() T_ZrConfig_Types_Vector3 { return v.v_Vector3Value }
// 整数键值对；标量
func(v *AllTypesConfig) KvValue() T_ZrConfig_Types_Kv { return v.v_KvValue }
// 外部特殊标量转换器；标量
func(v *AllTypesConfig) MoneyValue() T_Game_Config_Money { return v.v_MoneyValue }
// 基础整数；一维数组；元素用分号分隔
func(v *AllTypesConfig) ByteArray() List[uint8] { return v.v_ByteArray }
// 基础整数；一维数组；元素用分号分隔
func(v *AllTypesConfig) ShortArray() List[int16] { return v.v_ShortArray }
// 基础整数；一维数组；元素用分号分隔
func(v *AllTypesConfig) IntArray() List[int32] { return v.v_IntArray }
// 基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度
func(v *AllTypesConfig) LongArray() List[int64] { return v.v_LongArray }
// 基础浮点；一维数组；元素用分号分隔
func(v *AllTypesConfig) FloatArray() List[float32] { return v.v_FloatArray }
// 基础浮点；一维数组；元素用分号分隔
func(v *AllTypesConfig) DoubleArray() List[float64] { return v.v_DoubleArray }
// 基础布尔；一维数组；元素用分号分隔
func(v *AllTypesConfig) BoolArray() List[bool] { return v.v_BoolArray }
// 基础字符串；一维数组；元素用分号分隔
func(v *AllTypesConfig) StringArray() List[string] { return v.v_StringArray }
// 基础整数；一维数组；元素用分号分隔
func(v *AllTypesConfig) SByteArray() List[int8] { return v.v_SByteArray }
// 基础整数；一维数组；元素用分号分隔
func(v *AllTypesConfig) UShortArray() List[uint16] { return v.v_UShortArray }
// 基础整数；一维数组；元素用分号分隔
func(v *AllTypesConfig) UIntArray() List[uint32] { return v.v_UIntArray }
// 基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度
func(v *AllTypesConfig) ULongArray() List[uint64] { return v.v_ULongArray }
// 精确十进制；一维数组；元素用分号分隔；文本存储，保留完整精度
func(v *AllTypesConfig) DecimalArray() List[Decimal] { return v.v_DecimalArray }
// 枚举；一维数组；元素用分号分隔
func(v *AllTypesConfig) QualityArray() List[T_Game_Config_Quality] { return v.v_QualityArray }
// 二维向量；一维数组；元素用分号分隔
func(v *AllTypesConfig) Vector2Array() List[T_ZrConfig_Types_Vector2] { return v.v_Vector2Array }
// 三维向量；一维数组；元素用分号分隔
func(v *AllTypesConfig) Vector3Array() List[T_ZrConfig_Types_Vector3] { return v.v_Vector3Array }
// 整数键值对；一维数组；元素用分号分隔
func(v *AllTypesConfig) KvArray() List[T_ZrConfig_Types_Kv] { return v.v_KvArray }
// 外部特殊标量转换器；一维数组；元素用分号分隔
func(v *AllTypesConfig) MoneyArray() List[T_Game_Config_Money] { return v.v_MoneyArray }
// 基础整数；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) ByteArray2D() List[List[uint8]] { return v.v_ByteArray2D }
// 基础整数；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) ShortArray2D() List[List[int16]] { return v.v_ShortArray2D }
// 基础整数；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) IntArray2D() List[List[int32]] { return v.v_IntArray2D }
// 基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度
func(v *AllTypesConfig) LongArray2D() List[List[int64]] { return v.v_LongArray2D }
// 基础浮点；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) FloatArray2D() List[List[float32]] { return v.v_FloatArray2D }
// 基础浮点；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) DoubleArray2D() List[List[float64]] { return v.v_DoubleArray2D }
// 基础布尔；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) BoolArray2D() List[List[bool]] { return v.v_BoolArray2D }
// 基础字符串；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) StringArray2D() List[List[string]] { return v.v_StringArray2D }
// 基础整数；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) SByteArray2D() List[List[int8]] { return v.v_SByteArray2D }
// 基础整数；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) UShortArray2D() List[List[uint16]] { return v.v_UShortArray2D }
// 基础整数；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) UIntArray2D() List[List[uint32]] { return v.v_UIntArray2D }
// 基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度
func(v *AllTypesConfig) ULongArray2D() List[List[uint64]] { return v.v_ULongArray2D }
// 精确十进制；二维交错数组；元素用分号分隔；文本存储，保留完整精度
func(v *AllTypesConfig) DecimalArray2D() List[List[Decimal]] { return v.v_DecimalArray2D }
// 枚举；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) QualityArray2D() List[List[T_Game_Config_Quality]] { return v.v_QualityArray2D }
// 二维向量；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) Vector2Array2D() List[List[T_ZrConfig_Types_Vector2]] { return v.v_Vector2Array2D }
// 三维向量；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) Vector3Array2D() List[List[T_ZrConfig_Types_Vector3]] { return v.v_Vector3Array2D }
// 整数键值对；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) KvArray2D() List[List[T_ZrConfig_Types_Kv]] { return v.v_KvArray2D }
// 外部特殊标量转换器；二维交错数组；元素用分号分隔
func(v *AllTypesConfig) MoneyArray2D() List[List[T_Game_Config_Money]] { return v.v_MoneyArray2D }
// 基础整数；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) ByteArray3D() List[List[List[uint8]]] { return v.v_ByteArray3D }
// 基础整数；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) ShortArray3D() List[List[List[int16]]] { return v.v_ShortArray3D }
// 基础整数；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) IntArray3D() List[List[List[int32]]] { return v.v_IntArray3D }
// 基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度
func(v *AllTypesConfig) LongArray3D() List[List[List[int64]]] { return v.v_LongArray3D }
// 基础浮点；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) FloatArray3D() List[List[List[float32]]] { return v.v_FloatArray3D }
// 基础浮点；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) DoubleArray3D() List[List[List[float64]]] { return v.v_DoubleArray3D }
// 基础布尔；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) BoolArray3D() List[List[List[bool]]] { return v.v_BoolArray3D }
// 基础字符串；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) StringArray3D() List[List[List[string]]] { return v.v_StringArray3D }
// 基础整数；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) SByteArray3D() List[List[List[int8]]] { return v.v_SByteArray3D }
// 基础整数；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) UShortArray3D() List[List[List[uint16]]] { return v.v_UShortArray3D }
// 基础整数；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) UIntArray3D() List[List[List[uint32]]] { return v.v_UIntArray3D }
// 基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度
func(v *AllTypesConfig) ULongArray3D() List[List[List[uint64]]] { return v.v_ULongArray3D }
// 精确十进制；三维交错数组；元素用分号分隔；文本存储，保留完整精度
func(v *AllTypesConfig) DecimalArray3D() List[List[List[Decimal]]] { return v.v_DecimalArray3D }
// 枚举；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) QualityArray3D() List[List[List[T_Game_Config_Quality]]] { return v.v_QualityArray3D }
// 二维向量；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) Vector2Array3D() List[List[List[T_ZrConfig_Types_Vector2]]] { return v.v_Vector2Array3D }
// 三维向量；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) Vector3Array3D() List[List[List[T_ZrConfig_Types_Vector3]]] { return v.v_Vector3Array3D }
// 整数键值对；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) KvArray3D() List[List[List[T_ZrConfig_Types_Kv]]] { return v.v_KvArray3D }
// 外部特殊标量转换器；三维交错数组；元素用分号分隔
func(v *AllTypesConfig) MoneyArray3D() List[List[List[T_Game_Config_Money]]] { return v.v_MoneyArray3D }
// 最多目标数
func(v *AllTypesConfig) Settings() *T_Game_Config_BattleSettings { return v.v_Settings }
// 结构体最小值
func(v *AllTypesConfig) Range() T_Game_Config_ConfigRange { return v.v_Range }
// 类的可写属性
func(v *AllTypesConfig) Options() *T_Game_Config_SampleOptions { return v.v_Options }
// 单个多态对象：选择 Damage / AddBuff
func(v *AllTypesConfig) Action() T_Game_Config_BattleAction { return v.v_Action }
// 动作顺序：同一 Id 从 1 开始；一行一个动作
func(v *AllTypesConfig) Actions() List[T_Game_Config_BattleAction] { return v.v_Actions }
// 普通结构体数组也能多行；独立排序
func(v *AllTypesConfig) Ranges() List[T_Game_Config_ConfigRange] { return v.v_Ranges }
// 普通 class 数组，每行一个对象
func(v *AllTypesConfig) OptionsList() List[*T_Game_Config_SampleOptions] { return v.v_OptionsList }
// 父动作组编号；同一个编号的续行归入同一组
func(v *AllTypesConfig) Groups() List[*T_Game_Config_ActionGroup] { return v.v_Groups }
// 阶段编号；续行重复填写
func(v *AllTypesConfig) Phases() List[*T_Game_Config_BattlePhase] { return v.v_Phases }
// 可空结构体；整组空白表示 null
func(v *AllTypesConfig) OptionalRange() Optional[T_Game_Config_ConfigRange] { return v.v_OptionalRange }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) OptionalInt() Optional[int32] { return v.v_OptionalInt }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) OptionalUInt() Optional[uint32] { return v.v_OptionalUInt }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) OptionalULong() Optional[uint64] { return v.v_OptionalULong }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) OptionalDecimal() Optional[Decimal] { return v.v_OptionalDecimal }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) OptionalBool() Optional[bool] { return v.v_OptionalBool }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) OptionalQuality() Optional[T_Game_Config_Quality] { return v.v_OptionalQuality }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) OptionalMoney() Optional[T_Game_Config_Money] { return v.v_OptionalMoney }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) OptionalValues() List[Optional[int32]] { return v.v_OptionalValues }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) StatMap() Dictionary[string,int32] { return v.v_StatMap }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) PriceMap() Dictionary[int32,Optional[Decimal]] { return v.v_PriceMap }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) QualityMap() Dictionary[T_Game_Config_Quality,uint32] { return v.v_QualityMap }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) ListMap() Dictionary[string,List[int32]] { return v.v_ListMap }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) IntMatrix() Matrix[int32] { return v.v_IntMatrix }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) DecimalCube() Matrix[Decimal] { return v.v_DecimalCube }
// 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
func(v *AllTypesConfig) OptionalMatrix() Matrix[Optional[uint32]] { return v.v_OptionalMatrix }
// 原始 JSON；标量；这是 JSON 类型本身的样例
func(v *AllTypesConfig) RawJsonValue() string { return v.v_RawJsonValue }
// 原始 JSON；一维数组；这是 JSON 类型本身的样例
func(v *AllTypesConfig) RawJsonArray() List[string] { return v.v_RawJsonArray }
// 原始 JSON；二维交错数组；这是 JSON 类型本身的样例
func(v *AllTypesConfig) RawJsonArray2D() List[List[string]] { return v.v_RawJsonArray2D }
// 原始 JSON；三维交错数组；这是 JSON 类型本身的样例
func(v *AllTypesConfig) RawJsonArray3D() List[List[List[string]]] { return v.v_RawJsonArray3D }
// 基础整数；一维数组；json<T> 输入格式示例，用逗号分隔
func(v *AllTypesConfig) JsonIntArray() List[int32] { return v.v_JsonIntArray }
const schemaAllTypes = "{\"name\":\"AllTypes\",\"hash\":\"E8017397D24CBD0FDA2008AFD4F2BA2BA68619A29F4D4D79F7973F06BC8360B1\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"1=常规值；2=边界值；3=默认值。重复 Id 的多行组成同一条配置。\"},{\"name\":\"ByteValue\",\"type\":\"System.Byte\",\"description\":\"基础整数；标量\"},{\"name\":\"ShortValue\",\"type\":\"System.Int16\",\"description\":\"基础整数；标量\"},{\"name\":\"IntValue\",\"type\":\"System.Int32\",\"description\":\"基础整数；标量\"},{\"name\":\"LongValue\",\"type\":\"System.Int64\",\"description\":\"基础整数；标量；文本存储，保留完整精度\"},{\"name\":\"FloatValue\",\"type\":\"System.Single\",\"description\":\"基础浮点；标量\"},{\"name\":\"DoubleValue\",\"type\":\"System.Double\",\"description\":\"基础浮点；标量\"},{\"name\":\"BoolValue\",\"type\":\"System.Boolean\",\"description\":\"基础布尔；标量\"},{\"name\":\"StringValue\",\"type\":\"System.String\",\"description\":\"基础字符串；标量\"},{\"name\":\"SByteValue\",\"type\":\"System.SByte\",\"description\":\"基础整数；标量\"},{\"name\":\"UShortValue\",\"type\":\"System.UInt16\",\"description\":\"基础整数；标量\"},{\"name\":\"UIntValue\",\"type\":\"System.UInt32\",\"description\":\"基础整数；标量\"},{\"name\":\"ULongValue\",\"type\":\"System.UInt64\",\"description\":\"基础整数；标量；文本存储，保留完整精度\"},{\"name\":\"DecimalValue\",\"type\":\"System.Decimal\",\"description\":\"精确十进制；标量；文本存储，保留完整精度\"},{\"name\":\"QualityValue\",\"type\":\"Game.Config.Quality\",\"description\":\"枚举；标量\"},{\"name\":\"Vector2Value\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"二维向量；标量\"},{\"name\":\"Vector3Value\",\"type\":\"ZrConfig.Types.Vector3\",\"description\":\"三维向量；标量\"},{\"name\":\"KvValue\",\"type\":\"ZrConfig.Types.Kv\",\"description\":\"整数键值对；标量\"},{\"name\":\"MoneyValue\",\"type\":\"Game.Config.Money\",\"description\":\"外部特殊标量转换器；标量\"},{\"name\":\"ByteArray\",\"type\":\"System.Byte[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"ShortArray\",\"type\":\"System.Int16[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"IntArray\",\"type\":\"System.Int32[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"LongArray\",\"type\":\"System.Int64[]\",\"description\":\"基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"FloatArray\",\"type\":\"System.Single[]\",\"description\":\"基础浮点；一维数组；元素用分号分隔\"},{\"name\":\"DoubleArray\",\"type\":\"System.Double[]\",\"description\":\"基础浮点；一维数组；元素用分号分隔\"},{\"name\":\"BoolArray\",\"type\":\"System.Boolean[]\",\"description\":\"基础布尔；一维数组；元素用分号分隔\"},{\"name\":\"StringArray\",\"type\":\"System.String[]\",\"description\":\"基础字符串；一维数组；元素用分号分隔\"},{\"name\":\"SByteArray\",\"type\":\"System.SByte[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"UShortArray\",\"type\":\"System.UInt16[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"UIntArray\",\"type\":\"System.UInt32[]\",\"description\":\"基础整数；一维数组；元素用分号分隔\"},{\"name\":\"ULongArray\",\"type\":\"System.UInt64[]\",\"description\":\"基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"DecimalArray\",\"type\":\"System.Decimal[]\",\"description\":\"精确十进制；一维数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"QualityArray\",\"type\":\"Game.Config.Quality[]\",\"description\":\"枚举；一维数组；元素用分号分隔\"},{\"name\":\"Vector2Array\",\"type\":\"ZrConfig.Types.Vector2[]\",\"description\":\"二维向量；一维数组；元素用分号分隔\"},{\"name\":\"Vector3Array\",\"type\":\"ZrConfig.Types.Vector3[]\",\"description\":\"三维向量；一维数组；元素用分号分隔\"},{\"name\":\"KvArray\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"整数键值对；一维数组；元素用分号分隔\"},{\"name\":\"MoneyArray\",\"type\":\"Game.Config.Money[]\",\"description\":\"外部特殊标量转换器；一维数组；元素用分号分隔\"},{\"name\":\"ByteArray2D\",\"type\":\"System.Byte[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"ShortArray2D\",\"type\":\"System.Int16[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"IntArray2D\",\"type\":\"System.Int32[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"LongArray2D\",\"type\":\"System.Int64[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"FloatArray2D\",\"type\":\"System.Single[][]\",\"description\":\"基础浮点；二维交错数组；元素用分号分隔\"},{\"name\":\"DoubleArray2D\",\"type\":\"System.Double[][]\",\"description\":\"基础浮点；二维交错数组；元素用分号分隔\"},{\"name\":\"BoolArray2D\",\"type\":\"System.Boolean[][]\",\"description\":\"基础布尔；二维交错数组；元素用分号分隔\"},{\"name\":\"StringArray2D\",\"type\":\"System.String[][]\",\"description\":\"基础字符串；二维交错数组；元素用分号分隔\"},{\"name\":\"SByteArray2D\",\"type\":\"System.SByte[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"UShortArray2D\",\"type\":\"System.UInt16[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"UIntArray2D\",\"type\":\"System.UInt32[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔\"},{\"name\":\"ULongArray2D\",\"type\":\"System.UInt64[][]\",\"description\":\"基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"DecimalArray2D\",\"type\":\"System.Decimal[][]\",\"description\":\"精确十进制；二维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"QualityArray2D\",\"type\":\"Game.Config.Quality[][]\",\"description\":\"枚举；二维交错数组；元素用分号分隔\"},{\"name\":\"Vector2Array2D\",\"type\":\"ZrConfig.Types.Vector2[][]\",\"description\":\"二维向量；二维交错数组；元素用分号分隔\"},{\"name\":\"Vector3Array2D\",\"type\":\"ZrConfig.Types.Vector3[][]\",\"description\":\"三维向量；二维交错数组；元素用分号分隔\"},{\"name\":\"KvArray2D\",\"type\":\"ZrConfig.Types.Kv[][]\",\"description\":\"整数键值对；二维交错数组；元素用分号分隔\"},{\"name\":\"MoneyArray2D\",\"type\":\"Game.Config.Money[][]\",\"description\":\"外部特殊标量转换器；二维交错数组；元素用分号分隔\"},{\"name\":\"ByteArray3D\",\"type\":\"System.Byte[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"ShortArray3D\",\"type\":\"System.Int16[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"IntArray3D\",\"type\":\"System.Int32[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"LongArray3D\",\"type\":\"System.Int64[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"FloatArray3D\",\"type\":\"System.Single[][][]\",\"description\":\"基础浮点；三维交错数组；元素用分号分隔\"},{\"name\":\"DoubleArray3D\",\"type\":\"System.Double[][][]\",\"description\":\"基础浮点；三维交错数组；元素用分号分隔\"},{\"name\":\"BoolArray3D\",\"type\":\"System.Boolean[][][]\",\"description\":\"基础布尔；三维交错数组；元素用分号分隔\"},{\"name\":\"StringArray3D\",\"type\":\"System.String[][][]\",\"description\":\"基础字符串；三维交错数组；元素用分号分隔\"},{\"name\":\"SByteArray3D\",\"type\":\"System.SByte[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"UShortArray3D\",\"type\":\"System.UInt16[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"UIntArray3D\",\"type\":\"System.UInt32[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔\"},{\"name\":\"ULongArray3D\",\"type\":\"System.UInt64[][][]\",\"description\":\"基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"DecimalArray3D\",\"type\":\"System.Decimal[][][]\",\"description\":\"精确十进制；三维交错数组；元素用分号分隔；文本存储，保留完整精度\"},{\"name\":\"QualityArray3D\",\"type\":\"Game.Config.Quality[][][]\",\"description\":\"枚举；三维交错数组；元素用分号分隔\"},{\"name\":\"Vector2Array3D\",\"type\":\"ZrConfig.Types.Vector2[][][]\",\"description\":\"二维向量；三维交错数组；元素用分号分隔\"},{\"name\":\"Vector3Array3D\",\"type\":\"ZrConfig.Types.Vector3[][][]\",\"description\":\"三维向量；三维交错数组；元素用分号分隔\"},{\"name\":\"KvArray3D\",\"type\":\"ZrConfig.Types.Kv[][][]\",\"description\":\"整数键值对；三维交错数组；元素用分号分隔\"},{\"name\":\"MoneyArray3D\",\"type\":\"Game.Config.Money[][][]\",\"description\":\"外部特殊标量转换器；三维交错数组；元素用分号分隔\"},{\"name\":\"Settings\",\"type\":\"Game.Config.BattleSettings\",\"description\":\"最多目标数\"},{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"结构体最小值\"},{\"name\":\"Options\",\"type\":\"Game.Config.SampleOptions\",\"description\":\"类的可写属性\"},{\"name\":\"Action\",\"type\":\"Game.Config.BattleAction\",\"description\":\"单个多态对象：选择 Damage / AddBuff\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"动作顺序：同一 Id 从 1 开始；一行一个动作\"},{\"name\":\"Ranges\",\"type\":\"Game.Config.ConfigRange[]\",\"description\":\"普通结构体数组也能多行；独立排序\"},{\"name\":\"OptionsList\",\"type\":\"Game.Config.SampleOptions[]\",\"description\":\"普通 class 数组，每行一个对象\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"父动作组编号；同一个编号的续行归入同一组\"},{\"name\":\"Phases\",\"type\":\"Game.Config.BattlePhase[]\",\"description\":\"阶段编号；续行重复填写\"},{\"name\":\"OptionalRange\",\"type\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"description\":\"可空结构体；整组空白表示 null\"},{\"name\":\"OptionalInt\",\"type\":\"System.Nullable`1<System.Int32>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalUInt\",\"type\":\"System.Nullable`1<System.UInt32>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalULong\",\"type\":\"System.Nullable`1<System.UInt64>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalDecimal\",\"type\":\"System.Nullable`1<System.Decimal>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalBool\",\"type\":\"System.Nullable`1<System.Boolean>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalQuality\",\"type\":\"System.Nullable`1<Game.Config.Quality>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalMoney\",\"type\":\"System.Nullable`1<Game.Config.Money>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalValues\",\"type\":\"System.Nullable`1<System.Int32>[]\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"StatMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"PriceMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"QualityMap\",\"type\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"ListMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"IntMatrix\",\"type\":\"System.Int32[,]\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"DecimalCube\",\"type\":\"System.Decimal[,,]\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"OptionalMatrix\",\"type\":\"System.Nullable`1<System.UInt32>[,]\",\"description\":\"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明\"},{\"name\":\"RawJsonValue\",\"type\":\"ZrConfig.Types.RawJson\",\"description\":\"原始 JSON；标量；这是 JSON 类型本身的样例\"},{\"name\":\"RawJsonArray\",\"type\":\"ZrConfig.Types.RawJson[]\",\"description\":\"原始 JSON；一维数组；这是 JSON 类型本身的样例\"},{\"name\":\"RawJsonArray2D\",\"type\":\"ZrConfig.Types.RawJson[][]\",\"description\":\"原始 JSON；二维交错数组；这是 JSON 类型本身的样例\"},{\"name\":\"RawJsonArray3D\",\"type\":\"ZrConfig.Types.RawJson[][][]\",\"description\":\"原始 JSON；三维交错数组；这是 JSON 类型本身的样例\"},{\"name\":\"JsonIntArray\",\"type\":\"System.Int32[]\",\"description\":\"基础整数；一维数组；json<T> 输入格式示例，用逗号分隔\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]},{\"name\":\"ByQuality\",\"fields\":[\"QualityValue\"]}],\"types\":[{\"id\":\"Game.Config.ActionGroup\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ActionGroup[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ActionGroup\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.AddBuff\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"BuffId\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Stacks\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"enumValues\":[],\"abstract\":true,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattleAction\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattlePhase\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleSettings\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"MaxTargets\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Interruptible\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Min\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Max\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Damage\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Ratio\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"CanCrit\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money\",\"kind\":\"converter\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Cents\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality\",\"kind\":\"enum\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[\"0\",\"1\",\"2\"],\"abstract\":false,\"baseType\":null,\"enumMembers\":{\"None\":\"0\",\"Rare\":\"1\",\"Legendary\":\"2\"},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"\"},{\"name\":\"Rewards\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"\"},{\"name\":\"Offset\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"\"},{\"name\":\"Label\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Quality\",\"type\":\"Game.Config.Quality\",\"description\":\"\"},{\"name\":\"Levels\",\"type\":\"System.Int32[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.SampleOptions\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte\",\"kind\":\"primitive\",\"primitive\":\"uint8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":\"Game.Config.Quality\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Nullable`1<System.Decimal>\",\"key\":\"System.Int32\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32[]\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal\",\"kind\":\"primitive\",\"primitive\":\"decimal\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[,,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":3,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double\",\"kind\":\"primitive\",\"primitive\":\"float64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16\",\"kind\":\"primitive\",\"primitive\":\"int16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64\",\"kind\":\"primitive\",\"primitive\":\"int64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Money>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Quality>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Boolean>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Decimal>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Nullable`1<System.Int32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Nullable`1<System.UInt32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt64>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte\",\"kind\":\"primitive\",\"primitive\":\"int8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single\",\"kind\":\"primitive\",\"primitive\":\"float32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16\",\"kind\":\"primitive\",\"primitive\":\"uint16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32\",\"kind\":\"primitive\",\"primitive\":\"uint32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64\",\"kind\":\"primitive\",\"primitive\":\"uint64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv\",\"kind\":\"kv\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Key\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Value\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson\",\"kind\":\"rawjson\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2\",\"kind\":\"vector2\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3\",\"kind\":\"vector3\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Z\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"dependencies\":[]}"
type AllTypesKey_Key struct {
    v_Id int32
}
type AllTypesKey_Group_ById struct {
    v_Id int32
}
type AllTypesKey_Group_ByQuality struct {
    v_QualityValue T_Game_Config_Quality
}
type AllTypesTable struct {
    rows List[*AllTypesConfig]
    index_Key map[AllTypesKey_Key]*AllTypesConfig
    index_Group_ById map[AllTypesKey_Group_ById]List[*AllTypesConfig]
    index_Group_ByQuality map[AllTypesKey_Group_ByQuality]List[*AllTypesConfig]
}
func loadAllTypes(source []any)*AllTypesTable {
    result:=&AllTypesTable{rows:List[*AllTypesConfig]{values:make([]*AllTypesConfig,len(source))}}
    result.index_Key=make(map[AllTypesKey_Key]*AllTypesConfig,len(source))
    result.index_Group_ById=make(map[AllTypesKey_Group_ById]List[*AllTypesConfig],len(source))
    result.index_Group_ByQuality=make(map[AllTypesKey_Group_ByQuality]List[*AllTypesConfig],len(source))
    for i,value:=range source {
        row:=&AllTypesConfig{
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
        }
        result.rows.values[i]=row
        key_Key:=AllTypesKey_Key{row.v_Id}
        if _,ok:=result.index_Key[key_Key];ok{invalid("主键重复")}
        result.index_Key[key_Key]=row
        key_Group_ById:=AllTypesKey_Group_ById{row.v_Id}
        group_Group_ById:=result.index_Group_ById[key_Group_ById]
        group_Group_ById.values=append(group_Group_ById.values,row)
        result.index_Group_ById[key_Group_ById]=group_Group_ById
        key_Group_ByQuality:=AllTypesKey_Group_ByQuality{row.v_QualityValue}
        group_Group_ByQuality:=result.index_Group_ByQuality[key_Group_ByQuality]
        group_Group_ByQuality.values=append(group_Group_ByQuality.values,row)
        result.index_Group_ByQuality[key_Group_ByQuality]=group_Group_ByQuality
    }
    return result
}
func(t *AllTypesTable)bind(snapshot *ConfigSnapshot){
}
func(t *AllTypesTable) Rows()List[*AllTypesConfig]{return t.rows}
func(t *AllTypesTable) Count()int{return t.rows.Len()}
func(t *AllTypesTable) Find(k0 int32)(*AllTypesConfig,bool){value,ok:=t.index_Key[AllTypesKey_Key{k0}];return value,ok}
func(t *AllTypesTable) Get(k0 int32)*AllTypesConfig{value:=t.index_Key[AllTypesKey_Key{k0}];if value==nil{invalid("配置主键不存在")};return value}
func(t *AllTypesTable) GetById(k0 int32)List[*AllTypesConfig]{return t.index_Group_ById[AllTypesKey_Group_ById{k0}]}
func(t *AllTypesTable) GetByQuality(k0 T_Game_Config_Quality)List[*AllTypesConfig]{return t.index_Group_ByQuality[AllTypesKey_Group_ByQuality{k0}]}
