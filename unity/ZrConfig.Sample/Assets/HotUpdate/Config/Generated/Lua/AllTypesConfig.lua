-- zrconfig-api-contract:1
-- 此文件由工具生成，请勿手工修改。
local prefix = (...):match('^(.*%.)') or ''
local R = require(prefix .. 'runtime')
---@class AllTypesConfig
-- 1=常规值；2=边界值；3=默认值。重复 Id 的多行组成同一条配置。
---@field Id integer
-- 基础整数；标量
---@field ByteValue integer
-- 基础整数；标量
---@field ShortValue integer
-- 基础整数；标量
---@field IntValue integer
-- 基础整数；标量；文本存储，保留完整精度
---@field LongValue integer
-- 基础浮点；标量
---@field FloatValue number
-- 基础浮点；标量
---@field DoubleValue number
-- 基础布尔；标量
---@field BoolValue boolean
-- 基础字符串；标量
---@field StringValue string
-- 基础整数；标量
---@field SByteValue integer
-- 基础整数；标量
---@field UShortValue integer
-- 基础整数；标量
---@field UIntValue integer
-- 基础整数；标量；文本存储，保留完整精度
---@field ULongValue string
-- 精确十进制；标量；文本存储，保留完整精度
---@field DecimalValue string
-- 枚举；标量
---@field QualityValue T_Game_Config_Quality
-- 二维向量；标量
---@field Vector2Value T_ZrConfig_Types_Vector2
-- 三维向量；标量
---@field Vector3Value T_ZrConfig_Types_Vector3
-- 整数键值对；标量
---@field KvValue T_ZrConfig_Types_Kv
-- 外部特殊标量转换器；标量
---@field MoneyValue T_Game_Config_Money
-- 基础整数；一维数组；元素用分号分隔
---@field ByteArray (integer)[]
-- 基础整数；一维数组；元素用分号分隔
---@field ShortArray (integer)[]
-- 基础整数；一维数组；元素用分号分隔
---@field IntArray (integer)[]
-- 基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度
---@field LongArray (integer)[]
-- 基础浮点；一维数组；元素用分号分隔
---@field FloatArray (number)[]
-- 基础浮点；一维数组；元素用分号分隔
---@field DoubleArray (number)[]
-- 基础布尔；一维数组；元素用分号分隔
---@field BoolArray (boolean)[]
-- 基础字符串；一维数组；元素用分号分隔
---@field StringArray (string)[]
-- 基础整数；一维数组；元素用分号分隔
---@field SByteArray (integer)[]
-- 基础整数；一维数组；元素用分号分隔
---@field UShortArray (integer)[]
-- 基础整数；一维数组；元素用分号分隔
---@field UIntArray (integer)[]
-- 基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度
---@field ULongArray (string)[]
-- 精确十进制；一维数组；元素用分号分隔；文本存储，保留完整精度
---@field DecimalArray (string)[]
-- 枚举；一维数组；元素用分号分隔
---@field QualityArray (T_Game_Config_Quality)[]
-- 二维向量；一维数组；元素用分号分隔
---@field Vector2Array (T_ZrConfig_Types_Vector2)[]
-- 三维向量；一维数组；元素用分号分隔
---@field Vector3Array (T_ZrConfig_Types_Vector3)[]
-- 整数键值对；一维数组；元素用分号分隔
---@field KvArray (T_ZrConfig_Types_Kv)[]
-- 外部特殊标量转换器；一维数组；元素用分号分隔
---@field MoneyArray (T_Game_Config_Money)[]
-- 基础整数；二维交错数组；元素用分号分隔
---@field ByteArray2D ((integer)[])[]
-- 基础整数；二维交错数组；元素用分号分隔
---@field ShortArray2D ((integer)[])[]
-- 基础整数；二维交错数组；元素用分号分隔
---@field IntArray2D ((integer)[])[]
-- 基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度
---@field LongArray2D ((integer)[])[]
-- 基础浮点；二维交错数组；元素用分号分隔
---@field FloatArray2D ((number)[])[]
-- 基础浮点；二维交错数组；元素用分号分隔
---@field DoubleArray2D ((number)[])[]
-- 基础布尔；二维交错数组；元素用分号分隔
---@field BoolArray2D ((boolean)[])[]
-- 基础字符串；二维交错数组；元素用分号分隔
---@field StringArray2D ((string)[])[]
-- 基础整数；二维交错数组；元素用分号分隔
---@field SByteArray2D ((integer)[])[]
-- 基础整数；二维交错数组；元素用分号分隔
---@field UShortArray2D ((integer)[])[]
-- 基础整数；二维交错数组；元素用分号分隔
---@field UIntArray2D ((integer)[])[]
-- 基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度
---@field ULongArray2D ((string)[])[]
-- 精确十进制；二维交错数组；元素用分号分隔；文本存储，保留完整精度
---@field DecimalArray2D ((string)[])[]
-- 枚举；二维交错数组；元素用分号分隔
---@field QualityArray2D ((T_Game_Config_Quality)[])[]
-- 二维向量；二维交错数组；元素用分号分隔
---@field Vector2Array2D ((T_ZrConfig_Types_Vector2)[])[]
-- 三维向量；二维交错数组；元素用分号分隔
---@field Vector3Array2D ((T_ZrConfig_Types_Vector3)[])[]
-- 整数键值对；二维交错数组；元素用分号分隔
---@field KvArray2D ((T_ZrConfig_Types_Kv)[])[]
-- 外部特殊标量转换器；二维交错数组；元素用分号分隔
---@field MoneyArray2D ((T_Game_Config_Money)[])[]
-- 基础整数；三维交错数组；元素用分号分隔
---@field ByteArray3D (((integer)[])[])[]
-- 基础整数；三维交错数组；元素用分号分隔
---@field ShortArray3D (((integer)[])[])[]
-- 基础整数；三维交错数组；元素用分号分隔
---@field IntArray3D (((integer)[])[])[]
-- 基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度
---@field LongArray3D (((integer)[])[])[]
-- 基础浮点；三维交错数组；元素用分号分隔
---@field FloatArray3D (((number)[])[])[]
-- 基础浮点；三维交错数组；元素用分号分隔
---@field DoubleArray3D (((number)[])[])[]
-- 基础布尔；三维交错数组；元素用分号分隔
---@field BoolArray3D (((boolean)[])[])[]
-- 基础字符串；三维交错数组；元素用分号分隔
---@field StringArray3D (((string)[])[])[]
-- 基础整数；三维交错数组；元素用分号分隔
---@field SByteArray3D (((integer)[])[])[]
-- 基础整数；三维交错数组；元素用分号分隔
---@field UShortArray3D (((integer)[])[])[]
-- 基础整数；三维交错数组；元素用分号分隔
---@field UIntArray3D (((integer)[])[])[]
-- 基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度
---@field ULongArray3D (((string)[])[])[]
-- 精确十进制；三维交错数组；元素用分号分隔；文本存储，保留完整精度
---@field DecimalArray3D (((string)[])[])[]
-- 枚举；三维交错数组；元素用分号分隔
---@field QualityArray3D (((T_Game_Config_Quality)[])[])[]
-- 二维向量；三维交错数组；元素用分号分隔
---@field Vector2Array3D (((T_ZrConfig_Types_Vector2)[])[])[]
-- 三维向量；三维交错数组；元素用分号分隔
---@field Vector3Array3D (((T_ZrConfig_Types_Vector3)[])[])[]
-- 整数键值对；三维交错数组；元素用分号分隔
---@field KvArray3D (((T_ZrConfig_Types_Kv)[])[])[]
-- 外部特殊标量转换器；三维交错数组；元素用分号分隔
---@field MoneyArray3D (((T_Game_Config_Money)[])[])[]
-- 最多目标数
---@field Settings T_Game_Config_BattleSettings|ZrConfigNull
-- 结构体最小值
---@field Range T_Game_Config_ConfigRange
-- 类的可写属性
---@field Options T_Game_Config_SampleOptions|ZrConfigNull
-- 单个多态对象：选择 Damage / AddBuff
---@field Action T_Game_Config_BattleAction|ZrConfigNull
-- 动作顺序：同一 Id 从 1 开始；一行一个动作
---@field Actions (T_Game_Config_BattleAction|ZrConfigNull)[]
-- 普通结构体数组也能多行；独立排序
---@field Ranges (T_Game_Config_ConfigRange)[]
-- 普通 class 数组，每行一个对象
---@field OptionsList (T_Game_Config_SampleOptions|ZrConfigNull)[]
-- 父动作组编号；同一个编号的续行归入同一组
---@field Groups (T_Game_Config_ActionGroup|ZrConfigNull)[]
-- 阶段编号；续行重复填写
---@field Phases (T_Game_Config_BattlePhase|ZrConfigNull)[]
-- 可空结构体；整组空白表示 null
---@field OptionalRange T_Game_Config_ConfigRange|ZrConfigNull
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field OptionalInt integer|ZrConfigNull
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field OptionalUInt integer|ZrConfigNull
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field OptionalULong string|ZrConfigNull
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field OptionalDecimal string|ZrConfigNull
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field OptionalBool boolean|ZrConfigNull
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field OptionalQuality T_Game_Config_Quality|ZrConfigNull
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field OptionalMoney T_Game_Config_Money|ZrConfigNull
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field OptionalValues (integer|ZrConfigNull)[]
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field StatMap ZrConfigDictionary
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field PriceMap ZrConfigDictionary
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field QualityMap ZrConfigDictionary
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field ListMap ZrConfigDictionary
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field IntMatrix ZrConfigMatrix
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field DecimalCube ZrConfigMatrix
-- 可空、字典与矩形数组；空白 / null / [] 的区别见填写说明
---@field OptionalMatrix ZrConfigMatrix
-- 原始 JSON；标量；这是 JSON 类型本身的样例
---@field RawJsonValue string
-- 原始 JSON；一维数组；这是 JSON 类型本身的样例
---@field RawJsonArray (string)[]
-- 原始 JSON；二维交错数组；这是 JSON 类型本身的样例
---@field RawJsonArray2D ((string)[])[]
-- 原始 JSON；三维交错数组；这是 JSON 类型本身的样例
---@field RawJsonArray3D (((string)[])[])[]
-- 基础整数；一维数组；json<T> 输入格式示例，用逗号分隔
---@field JsonIntArray (integer)[]
local schema = ([=[{"name":"AllTypes","hash":"E8017397D24CBD0FDA2008AFD4F2BA2BA68619A29F4D4D79F7973F06BC8360B1","hasId":true,"fields":[{"name":"Id","type":"System.Int32","description":"1=常规值；2=边界值；3=默认值。重复 Id 的多行组成同一条配置。"},{"name":"ByteValue","type":"System.Byte","description":"基础整数；标量"},{"name":"ShortValue","type":"System.Int16","description":"基础整数；标量"},{"name":"IntValue","type":"System.Int32","description":"基础整数；标量"},{"name":"LongValue","type":"System.Int64","description":"基础整数；标量；文本存储，保留完整精度"},{"name":"FloatValue","type":"System.Single","description":"基础浮点；标量"},{"name":"DoubleValue","type":"System.Double","description":"基础浮点；标量"},{"name":"BoolValue","type":"System.Boolean","description":"基础布尔；标量"},{"name":"StringValue","type":"System.String","description":"基础字符串；标量"},{"name":"SByteValue","type":"System.SByte","description":"基础整数；标量"},{"name":"UShortValue","type":"System.UInt16","description":"基础整数；标量"},{"name":"UIntValue","type":"System.UInt32","description":"基础整数；标量"},{"name":"ULongValue","type":"System.UInt64","description":"基础整数；标量；文本存储，保留完整精度"},{"name":"DecimalValue","type":"System.Decimal","description":"精确十进制；标量；文本存储，保留完整精度"},{"name":"QualityValue","type":"Game.Config.Quality","description":"枚举；标量"},{"name":"Vector2Value","type":"ZrConfig.Types.Vector2","description":"二维向量；标量"},{"name":"Vector3Value","type":"ZrConfig.Types.Vector3","description":"三维向量；标量"},{"name":"KvValue","type":"ZrConfig.Types.Kv","description":"整数键值对；标量"},{"name":"MoneyValue","type":"Game.Config.Money","description":"外部特殊标量转换器；标量"},{"name":"ByteArray","type":"System.Byte[]","description":"基础整数；一维数组；元素用分号分隔"},{"name":"ShortArray","type":"System.Int16[]","description":"基础整数；一维数组；元素用分号分隔"},{"name":"IntArray","type":"System.Int32[]","description":"基础整数；一维数组；元素用分号分隔"},{"name":"LongArray","type":"System.Int64[]","description":"基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度"},{"name":"FloatArray","type":"System.Single[]","description":"基础浮点；一维数组；元素用分号分隔"},{"name":"DoubleArray","type":"System.Double[]","description":"基础浮点；一维数组；元素用分号分隔"},{"name":"BoolArray","type":"System.Boolean[]","description":"基础布尔；一维数组；元素用分号分隔"},{"name":"StringArray","type":"System.String[]","description":"基础字符串；一维数组；元素用分号分隔"},{"name":"SByteArray","type":"System.SByte[]","description":"基础整数；一维数组；元素用分号分隔"},{"name":"UShortArray","type":"System.UInt16[]","description":"基础整数；一维数组；元素用分号分隔"},{"name":"UIntArray","type":"System.UInt32[]","description":"基础整数；一维数组；元素用分号分隔"},{"name":"ULongArray","type":"System.UInt64[]","description":"基础整数；一维数组；元素用分号分隔；文本存储，保留完整精度"},{"name":"DecimalArray","type":"System.Decimal[]","description":"精确十进制；一维数组；元素用分号分隔；文本存储，保留完整精度"},{"name":"QualityArray","type":"Game.Config.Quality[]","description":"枚举；一维数组；元素用分号分隔"},{"name":"Vector2Array","type":"ZrConfig.Types.Vector2[]","description":"二维向量；一维数组；元素用分号分隔"},{"name":"Vector3Array","type":"ZrConfig.Types.Vector3[]","description":"三维向量；一维数组；元素用分号分隔"},{"name":"KvArray","type":"ZrConfig.Types.Kv[]","description":"整数键值对；一维数组；元素用分号分隔"},{"name":"MoneyArray","type":"Game.Config.Money[]","description":"外部特殊标量转换器；一维数组；元素用分号分隔"},{"name":"ByteArray2D","type":"System.Byte[][]","description":"基础整数；二维交错数组；元素用分号分隔"},{"name":"ShortArray2D","type":"System.Int16[][]","description":"基础整数；二维交错数组；元素用分号分隔"},{"name":"IntArray2D","type":"System.Int32[][]","description":"基础整数；二维交错数组；元素用分号分隔"},{"name":"LongArray2D","type":"System.Int64[][]","description":"基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度"},{"name":"FloatArray2D","type":"System.Single[][]","description":"基础浮点；二维交错数组；元素用分号分隔"},{"name":"DoubleArray2D","type":"System.Double[][]","description":"基础浮点；二维交错数组；元素用分号分隔"},{"name":"BoolArray2D","type":"System.Boolean[][]","description":"基础布尔；二维交错数组；元素用分号分隔"},{"name":"StringArray2D","type":"System.String[][]","description":"基础字符串；二维交错数组；元素用分号分隔"},{"name":"SByteArray2D","type":"System.SByte[][]","description":"基础整数；二维交错数组；元素用分号分隔"},{"name":"UShortArray2D","type":"System.UInt16[][]","description":"基础整数；二维交错数组；元素用分号分隔"},{"name":"UIntArray2D","type":"System.UInt32[][]","description":"基础整数；二维交错数组；元素用分号分隔"},{"name":"ULongArray2D","type":"System.UInt64[][]","description":"基础整数；二维交错数组；元素用分号分隔；文本存储，保留完整精度"},{"name":"DecimalArray2D","type":"System.Decimal[][]","description":"精确十进制；二维交错数组；元素用分号分隔；文本存储，保留完整精度"},{"name":"QualityArray2D","type":"Game.Config.Quality[][]","description":"枚举；二维交错数组；元素用分号分隔"},{"name":"Vector2Array2D","type":"ZrConfig.Types.Vector2[][]","description":"二维向量；二维交错数组；元素用分号分隔"},{"name":"Vector3Array2D","type":"ZrConfig.Types.Vector3[][]","description":"三维向量；二维交错数组；元素用分号分隔"},{"name":"KvArray2D","type":"ZrConfig.Types.Kv[][]","description":"整数键值对；二维交错数组；元素用分号分隔"},{"name":"MoneyArray2D","type":"Game.Config.Money[][]","description":"外部特殊标量转换器；二维交错数组；元素用分号分隔"},{"name":"ByteArray3D","type":"System.Byte[][][]","description":"基础整数；三维交错数组；元素用分号分隔"},{"name":"ShortArray3D","type":"System.Int16[][][]","description":"基础整数；三维交错数组；元素用分号分隔"},{"name":"IntArray3D","type":"System.Int32[][][]","description":"基础整数；三维交错数组；元素用分号分隔"},{"name":"LongArray3D","type":"System.Int64[][][]","description":"基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度"},{"name":"FloatArray3D","type":"System.Single[][][]","description":"基础浮点；三维交错数组；元素用分号分隔"},{"name":"DoubleArray3D","type":"System.Double[][][]","description":"基础浮点；三维交错数组；元素用分号分隔"},{"name":"BoolArray3D","type":"System.Boolean[][][]","description":"基础布尔；三维交错数组；元素用分号分隔"},{"name":"StringArray3D","type":"System.String[][][]","description":"基础字符串；三维交错数组；元素用分号分隔"},{"name":"SByteArray3D","type":"System.SByte[][][]","description":"基础整数；三维交错数组；元素用分号分隔"},{"name":"UShortArray3D","type":"System.UInt16[][][]","description":"基础整数；三维交错数组；元素用分号分隔"},{"name":"UIntArray3D","type":"System.UInt32[][][]","description":"基础整数；三维交错数组；元素用分号分隔"},{"name":"ULongArray3D","type":"System.UInt64[][][]","description":"基础整数；三维交错数组；元素用分号分隔；文本存储，保留完整精度"},{"name":"DecimalArray3D","type":"System.Decimal[][][]","description":"精确十进制；三维交错数组；元素用分号分隔；文本存储，保留完整精度"},{"name":"QualityArray3D","type":"Game.Config.Quality[][][]","description":"枚举；三维交错数组；元素用分号分隔"},{"name":"Vector2Array3D","type":"ZrConfig.Types.Vector2[][][]","description":"二维向量；三维交错数组；元素用分号分隔"},{"name":"Vector3Array3D","type":"ZrConfig.Types.Vector3[][][]","description":"三维向量；三维交错数组；元素用分号分隔"},{"name":"KvArray3D","type":"ZrConfig.Types.Kv[][][]","description":"整数键值对；三维交错数组；元素用分号分隔"},{"name":"MoneyArray3D","type":"Game.Config.Money[][][]","description":"外部特殊标量转换器；三维交错数组；元素用分号分隔"},{"name":"Settings","type":"Game.Config.BattleSettings","description":"最多目标数"},{"name":"Range","type":"Game.Config.ConfigRange","description":"结构体最小值"},{"name":"Options","type":"Game.Config.SampleOptions","description":"类的可写属性"},{"name":"Action","type":"Game.Config.BattleAction","description":"单个多态对象：选择 Damage / AddBuff"},{"name":"Actions","type":"Game.Config.BattleAction[]","description":"动作顺序：同一 Id 从 1 开始；一行一个动作"},{"name":"Ranges","type":"Game.Config.ConfigRange[]","description":"普通结构体数组也能多行；独立排序"},{"name":"OptionsList","type":"Game.Config.SampleOptions[]","description":"普通 class 数组，每行一个对象"},{"name":"Groups","type":"Game.Config.ActionGroup[]","description":"父动作组编号；同一个编号的续行归入同一组"},{"name":"Phases","type":"Game.Config.BattlePhase[]","description":"阶段编号；续行重复填写"},{"name":"OptionalRange","type":"System.Nullable`1<Game.Config.ConfigRange>","description":"可空结构体；整组空白表示 null"},{"name":"OptionalInt","type":"System.Nullable`1<System.Int32>","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"OptionalUInt","type":"System.Nullable`1<System.UInt32>","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"OptionalULong","type":"System.Nullable`1<System.UInt64>","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"OptionalDecimal","type":"System.Nullable`1<System.Decimal>","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"OptionalBool","type":"System.Nullable`1<System.Boolean>","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"OptionalQuality","type":"System.Nullable`1<Game.Config.Quality>","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"OptionalMoney","type":"System.Nullable`1<Game.Config.Money>","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"OptionalValues","type":"System.Nullable`1<System.Int32>[]","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"StatMap","type":"System.Collections.Generic.Dictionary`2<System.String,System.Int32>","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"PriceMap","type":"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"QualityMap","type":"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"ListMap","type":"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"IntMatrix","type":"System.Int32[,]","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"DecimalCube","type":"System.Decimal[,,]","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"OptionalMatrix","type":"System.Nullable`1<System.UInt32>[,]","description":"可空、字典与矩形数组；空白 / null / [] 的区别见填写说明"},{"name":"RawJsonValue","type":"ZrConfig.Types.RawJson","description":"原始 JSON；标量；这是 JSON 类型本身的样例"},{"name":"RawJsonArray","type":"ZrConfig.Types.RawJson[]","description":"原始 JSON；一维数组；这是 JSON 类型本身的样例"},{"name":"RawJsonArray2D","type":"ZrConfig.Types.RawJson[][]","description":"原始 JSON；二维交错数组；这是 JSON 类型本身的样例"},{"name":"RawJsonArray3D","type":"ZrConfig.Types.RawJson[][][]","description":"原始 JSON；三维交错数组；这是 JSON 类型本身的样例"},{"name":"JsonIntArray","type":"System.Int32[]","description":"基础整数；一维数组；json<T> 输入格式示例，用逗号分隔"}],"keys":["Id"],"groups":[{"name":"ById","fields":["Id"]},{"name":"ByQuality","fields":["QualityValue"]}],"types":[{"id":"Game.Config.ActionGroup","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"Name","type":"System.String","description":""},{"name":"Actions","type":"Game.Config.BattleAction[]","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.ActionGroup[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.ActionGroup","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.AddBuff","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"Delay","type":"System.Single","description":""},{"name":"BuffId","type":"System.Int32","description":""},{"name":"Stacks","type":"System.Int32","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":"Game.Config.BattleAction","enumMembers":{},"referenceTable":null},{"id":"Game.Config.BattleAction","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"Delay","type":"System.Single","description":""}],"variants":["Game.Config.AddBuff","Game.Config.Damage"],"enumValues":[],"abstract":true,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.BattleAction[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.BattleAction","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.BattlePhase","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"Name","type":"System.String","description":""},{"name":"Groups","type":"Game.Config.ActionGroup[]","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.BattlePhase[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.BattlePhase","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.BattleSettings","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"MaxTargets","type":"System.Int32","description":""},{"name":"Interruptible","type":"System.Boolean","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.ConfigRange","kind":"object","primitive":"","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[{"name":"Min","type":"System.Int32","description":""},{"name":"Max","type":"System.Int32","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.ConfigRange[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.ConfigRange","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Damage","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"Delay","type":"System.Single","description":""},{"name":"Ratio","type":"System.Single","description":""},{"name":"CanCrit","type":"System.Boolean","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":"Game.Config.BattleAction","enumMembers":{},"referenceTable":null},{"id":"Game.Config.Money","kind":"converter","primitive":"","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[{"name":"Cents","type":"System.Int32","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Money[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.Money","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Money[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.Money[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Money[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.Money[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Quality","kind":"enum","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":["0","1","2"],"abstract":false,"baseType":null,"enumMembers":{"None":"0","Rare":"1","Legendary":"2"},"referenceTable":null},{"id":"Game.Config.Quality[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.Quality","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Quality[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.Quality[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Quality[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.Quality[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.SampleOptions","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"Range","type":"Game.Config.ConfigRange","description":""},{"name":"Rewards","type":"ZrConfig.Types.Kv[]","description":""},{"name":"Offset","type":"ZrConfig.Types.Vector2","description":""},{"name":"Label","type":"System.String","description":""},{"name":"Quality","type":"Game.Config.Quality","description":""},{"name":"Levels","type":"System.Int32[]","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.SampleOptions[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.SampleOptions","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Boolean","kind":"primitive","primitive":"bool","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Boolean[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Boolean","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Boolean[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Boolean[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Boolean[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Boolean[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Byte","kind":"primitive","primitive":"uint8","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Byte[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Byte","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Byte[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Byte[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Byte[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Byte[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>","kind":"dictionary","primitive":"","nullableObject":false,"rank":0,"element":"System.UInt32","key":"Game.Config.Quality","fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>","kind":"dictionary","primitive":"","nullableObject":false,"rank":0,"element":"System.Nullable`1<System.Decimal>","key":"System.Int32","fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Collections.Generic.Dictionary`2<System.String,System.Int32>","kind":"dictionary","primitive":"","nullableObject":false,"rank":0,"element":"System.Int32","key":"System.String","fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>","kind":"dictionary","primitive":"","nullableObject":false,"rank":0,"element":"System.Int32[]","key":"System.String","fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Decimal","kind":"primitive","primitive":"decimal","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Decimal[,,]","kind":"array","primitive":"","nullableObject":false,"rank":3,"element":"System.Decimal","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Decimal[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Decimal","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Decimal[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Decimal[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Decimal[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Decimal[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Double","kind":"primitive","primitive":"float64","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Double[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Double","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Double[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Double[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Double[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Double[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int16","kind":"primitive","primitive":"int16","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int16[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int16","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int16[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int16[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int16[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int16[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int32","kind":"primitive","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int32[,]","kind":"array","primitive":"","nullableObject":false,"rank":2,"element":"System.Int32","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int32[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int32","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int32[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int32[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int32[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int32[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int64","kind":"primitive","primitive":"int64","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int64[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int64","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int64[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int64[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int64[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int64[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<Game.Config.ConfigRange>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"Game.Config.ConfigRange","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<Game.Config.Money>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"Game.Config.Money","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<Game.Config.Quality>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"Game.Config.Quality","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.Boolean>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"System.Boolean","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.Decimal>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"System.Decimal","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.Int32>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"System.Int32","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.Int32>[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Nullable`1<System.Int32>","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.UInt32>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"System.UInt32","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.UInt32>[,]","kind":"array","primitive":"","nullableObject":false,"rank":2,"element":"System.Nullable`1<System.UInt32>","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.UInt64>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"System.UInt64","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.SByte","kind":"primitive","primitive":"int8","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.SByte[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.SByte","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.SByte[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.SByte[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.SByte[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.SByte[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Single","kind":"primitive","primitive":"float32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Single[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Single","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Single[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Single[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Single[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Single[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.String","kind":"primitive","primitive":"string","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.String[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.String","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.String[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.String[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.String[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.String[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt16","kind":"primitive","primitive":"uint16","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt16[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt16","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt16[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt16[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt16[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt16[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt32","kind":"primitive","primitive":"uint32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt32[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt32","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt32[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt32[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt32[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt32[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt64","kind":"primitive","primitive":"uint64","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt64[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt64","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt64[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt64[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt64[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt64[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Kv","kind":"kv","primitive":"","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[{"name":"Key","type":"System.Int32","description":""},{"name":"Value","type":"System.Int32","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Kv[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Kv","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Kv[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Kv[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Kv[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Kv[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.RawJson","kind":"rawjson","primitive":"","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.RawJson[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.RawJson","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.RawJson[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.RawJson[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.RawJson[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.RawJson[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector2","kind":"vector2","primitive":"","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[{"name":"X","type":"System.Single","description":""},{"name":"Y","type":"System.Single","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector2[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Vector2","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector2[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Vector2[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector2[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Vector2[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector3","kind":"vector3","primitive":"","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[{"name":"X","type":"System.Single","description":""},{"name":"Y","type":"System.Single","description":""},{"name":"Z","type":"System.Single","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector3[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Vector3","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector3[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Vector3[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector3[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Vector3[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null}],"tags":["Game.Config.AddBuff","Game.Config.Damage"],"dependencies":[]}]=])
return function(bundle, prefer_json)
    local rows = bundle.load(schema, prefer_json)
    local result = {rows=rows, count=#rows}

    local index_Key = R.index(rows, {([=[Id]=])}, true)
    local index_Group_ById = R.index(rows, {([=[Id]=])}, false)
    local index_Group_ByQuality = R.index(rows, {([=[QualityValue]=])}, false)
    function result.bind(snapshot)
    end
    function result.seal(seen)
        rows=R.readonly(rows, seen); result.rows=rows
        index_Key=R.readonly(index_Key, seen)
        index_Group_ById=R.readonly(index_Group_ById, seen)
        index_Group_ByQuality=R.readonly(index_Group_ByQuality, seen)
        result.bind=nil; result.seal=nil
    end
    function result.find(k0)
        local node = index_Key
        node=node[k0]; if node==nil then return nil end
        return node
    end
    function result.get(k0) return result.find(k0) or R.invalid('配置主键不存在') end
    function result.getById(k0)
        local node = index_Group_ById
        node=node[k0]; if node==nil then return R.empty end
        return node
    end
    function result.getByQuality(k0)
        local node = index_Group_ByQuality
        node=node[k0]; if node==nil then return R.empty end
        return node
    end
    return result
end
