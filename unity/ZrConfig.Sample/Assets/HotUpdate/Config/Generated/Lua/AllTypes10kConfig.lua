-- zrconfig-api-contract:1
-- 此文件由工具生成，请勿手工修改。
local prefix = (...):match('^(.*%.)') or ''
local R = require(prefix .. 'runtime')
---@class AllTypes10kConfig
-- 唯一主键：1～10000
---@field Id integer
-- Byte；按 Id 轮换常规、边界、默认值
---@field ByteValue integer
-- Short；按 Id 轮换常规、边界、默认值
---@field ShortValue integer
-- Int；按 Id 轮换常规、边界、默认值
---@field IntValue integer
-- Long；按 Id 轮换常规、边界、默认值
---@field LongValue integer
-- Float；按 Id 轮换常规、边界、默认值
---@field FloatValue number
-- Double；按 Id 轮换常规、边界、默认值
---@field DoubleValue number
-- Bool；按 Id 轮换常规、边界、默认值
---@field BoolValue boolean
-- String；按 Id 轮换常规、边界、默认值
---@field StringValue string
-- SByte；按 Id 轮换常规、边界、默认值
---@field SByteValue integer
-- UShort；按 Id 轮换常规、边界、默认值
---@field UShortValue integer
-- UInt；按 Id 轮换常规、边界、默认值
---@field UIntValue integer
-- ULong；按 Id 轮换常规、边界、默认值
---@field ULongValue string
-- Decimal；按 Id 轮换常规、边界、默认值
---@field DecimalValue string
-- Quality；按 Id 轮换常规、边界、默认值
---@field QualityValue T_Game_Config_Quality
-- Vector2；按 Id 轮换常规、边界、默认值
---@field Vector2Value T_ZrConfig_Types_Vector2
-- Vector3；按 Id 轮换常规、边界、默认值
---@field Vector3Value T_ZrConfig_Types_Vector3
-- Kv；按 Id 轮换常规、边界、默认值
---@field KvValue T_ZrConfig_Types_Kv
-- Money；按 Id 轮换常规、边界、默认值
---@field MoneyValue T_Game_Config_Money
-- Byte；按 Id 轮换常规、边界、默认值
---@field ByteArray (integer)[]
-- Short；按 Id 轮换常规、边界、默认值
---@field ShortArray (integer)[]
-- Int；按 Id 轮换常规、边界、默认值
---@field IntArray (integer)[]
-- Long；按 Id 轮换常规、边界、默认值
---@field LongArray (integer)[]
-- Float；按 Id 轮换常规、边界、默认值
---@field FloatArray (number)[]
-- Double；按 Id 轮换常规、边界、默认值
---@field DoubleArray (number)[]
-- Bool；按 Id 轮换常规、边界、默认值
---@field BoolArray (boolean)[]
-- String；按 Id 轮换常规、边界、默认值
---@field StringArray (string)[]
-- SByte；按 Id 轮换常规、边界、默认值
---@field SByteArray (integer)[]
-- UShort；按 Id 轮换常规、边界、默认值
---@field UShortArray (integer)[]
-- UInt；按 Id 轮换常规、边界、默认值
---@field UIntArray (integer)[]
-- ULong；按 Id 轮换常规、边界、默认值
---@field ULongArray (string)[]
-- Decimal；按 Id 轮换常规、边界、默认值
---@field DecimalArray (string)[]
-- Quality；按 Id 轮换常规、边界、默认值
---@field QualityArray (T_Game_Config_Quality)[]
-- Vector2；按 Id 轮换常规、边界、默认值
---@field Vector2Array (T_ZrConfig_Types_Vector2)[]
-- Vector3；按 Id 轮换常规、边界、默认值
---@field Vector3Array (T_ZrConfig_Types_Vector3)[]
-- Kv；按 Id 轮换常规、边界、默认值
---@field KvArray (T_ZrConfig_Types_Kv)[]
-- Money；按 Id 轮换常规、边界、默认值
---@field MoneyArray (T_Game_Config_Money)[]
-- Byte；按 Id 轮换常规、边界、默认值
---@field ByteArray2D ((integer)[])[]
-- Short；按 Id 轮换常规、边界、默认值
---@field ShortArray2D ((integer)[])[]
-- Int；按 Id 轮换常规、边界、默认值
---@field IntArray2D ((integer)[])[]
-- Long；按 Id 轮换常规、边界、默认值
---@field LongArray2D ((integer)[])[]
-- Float；按 Id 轮换常规、边界、默认值
---@field FloatArray2D ((number)[])[]
-- Double；按 Id 轮换常规、边界、默认值
---@field DoubleArray2D ((number)[])[]
-- Bool；按 Id 轮换常规、边界、默认值
---@field BoolArray2D ((boolean)[])[]
-- String；按 Id 轮换常规、边界、默认值
---@field StringArray2D ((string)[])[]
-- SByte；按 Id 轮换常规、边界、默认值
---@field SByteArray2D ((integer)[])[]
-- UShort；按 Id 轮换常规、边界、默认值
---@field UShortArray2D ((integer)[])[]
-- UInt；按 Id 轮换常规、边界、默认值
---@field UIntArray2D ((integer)[])[]
-- ULong；按 Id 轮换常规、边界、默认值
---@field ULongArray2D ((string)[])[]
-- Decimal；按 Id 轮换常规、边界、默认值
---@field DecimalArray2D ((string)[])[]
-- Quality；按 Id 轮换常规、边界、默认值
---@field QualityArray2D ((T_Game_Config_Quality)[])[]
-- Vector2；按 Id 轮换常规、边界、默认值
---@field Vector2Array2D ((T_ZrConfig_Types_Vector2)[])[]
-- Vector3；按 Id 轮换常规、边界、默认值
---@field Vector3Array2D ((T_ZrConfig_Types_Vector3)[])[]
-- Kv；按 Id 轮换常规、边界、默认值
---@field KvArray2D ((T_ZrConfig_Types_Kv)[])[]
-- Money；按 Id 轮换常规、边界、默认值
---@field MoneyArray2D ((T_Game_Config_Money)[])[]
-- Byte；按 Id 轮换常规、边界、默认值
---@field ByteArray3D (((integer)[])[])[]
-- Short；按 Id 轮换常规、边界、默认值
---@field ShortArray3D (((integer)[])[])[]
-- Int；按 Id 轮换常规、边界、默认值
---@field IntArray3D (((integer)[])[])[]
-- Long；按 Id 轮换常规、边界、默认值
---@field LongArray3D (((integer)[])[])[]
-- Float；按 Id 轮换常规、边界、默认值
---@field FloatArray3D (((number)[])[])[]
-- Double；按 Id 轮换常规、边界、默认值
---@field DoubleArray3D (((number)[])[])[]
-- Bool；按 Id 轮换常规、边界、默认值
---@field BoolArray3D (((boolean)[])[])[]
-- String；按 Id 轮换常规、边界、默认值
---@field StringArray3D (((string)[])[])[]
-- SByte；按 Id 轮换常规、边界、默认值
---@field SByteArray3D (((integer)[])[])[]
-- UShort；按 Id 轮换常规、边界、默认值
---@field UShortArray3D (((integer)[])[])[]
-- UInt；按 Id 轮换常规、边界、默认值
---@field UIntArray3D (((integer)[])[])[]
-- ULong；按 Id 轮换常规、边界、默认值
---@field ULongArray3D (((string)[])[])[]
-- Decimal；按 Id 轮换常规、边界、默认值
---@field DecimalArray3D (((string)[])[])[]
-- Quality；按 Id 轮换常规、边界、默认值
---@field QualityArray3D (((T_Game_Config_Quality)[])[])[]
-- Vector2；按 Id 轮换常规、边界、默认值
---@field Vector2Array3D (((T_ZrConfig_Types_Vector2)[])[])[]
-- Vector3；按 Id 轮换常规、边界、默认值
---@field Vector3Array3D (((T_ZrConfig_Types_Vector3)[])[])[]
-- Kv；按 Id 轮换常规、边界、默认值
---@field KvArray3D (((T_ZrConfig_Types_Kv)[])[])[]
-- Money；按 Id 轮换常规、边界、默认值
---@field MoneyArray3D (((T_Game_Config_Money)[])[])[]
-- 完整对象或列表；单格 JSON 输入，导出为声明的强类型
---@field Settings T_Game_Config_BattleSettings|ZrConfigNull
-- 完整对象或列表；单格 JSON 输入，导出为声明的强类型
---@field Range T_Game_Config_ConfigRange
-- 完整对象或列表；单格 JSON 输入，导出为声明的强类型
---@field Options T_Game_Config_SampleOptions|ZrConfigNull
-- 完整对象或列表；单格 JSON 输入，导出为声明的强类型
---@field Action T_Game_Config_BattleAction|ZrConfigNull
-- 完整对象或列表；单格 JSON 输入，导出为声明的强类型
---@field Actions (T_Game_Config_BattleAction|ZrConfigNull)[]
-- 完整对象或列表；单格 JSON 输入，导出为声明的强类型
---@field Ranges (T_Game_Config_ConfigRange)[]
-- 完整对象或列表；单格 JSON 输入，导出为声明的强类型
---@field OptionsList (T_Game_Config_SampleOptions|ZrConfigNull)[]
-- 完整对象或列表；单格 JSON 输入，导出为声明的强类型
---@field Groups (T_Game_Config_ActionGroup|ZrConfigNull)[]
-- 完整对象或列表；单格 JSON 输入，导出为声明的强类型
---@field Phases (T_Game_Config_BattlePhase|ZrConfigNull)[]
-- 完整对象或列表；单格 JSON 输入，导出为声明的强类型
---@field OptionalRange T_Game_Config_ConfigRange|ZrConfigNull
-- NullableInt；按 Id 轮换常规、边界、默认值
---@field OptionalInt integer|ZrConfigNull
-- NullableUInt；按 Id 轮换常规、边界、默认值
---@field OptionalUInt integer|ZrConfigNull
-- NullableULong；按 Id 轮换常规、边界、默认值
---@field OptionalULong string|ZrConfigNull
-- NullableDecimal；按 Id 轮换常规、边界、默认值
---@field OptionalDecimal string|ZrConfigNull
-- NullableBool；按 Id 轮换常规、边界、默认值
---@field OptionalBool boolean|ZrConfigNull
-- NullableQuality；按 Id 轮换常规、边界、默认值
---@field OptionalQuality T_Game_Config_Quality|ZrConfigNull
-- NullableMoney；按 Id 轮换常规、边界、默认值
---@field OptionalMoney T_Game_Config_Money|ZrConfigNull
-- NullableIntArray；按 Id 轮换常规、边界、默认值
---@field OptionalValues (integer|ZrConfigNull)[]
-- MapInt；按 Id 轮换常规、边界、默认值
---@field StatMap ZrConfigDictionary
-- MapNullableDecimal；按 Id 轮换常规、边界、默认值
---@field PriceMap ZrConfigDictionary
-- MapUInt；按 Id 轮换常规、边界、默认值
---@field QualityMap ZrConfigDictionary
-- MapIntArray；按 Id 轮换常规、边界、默认值
---@field ListMap ZrConfigDictionary
-- MatrixInt；按 Id 轮换常规、边界、默认值
---@field IntMatrix ZrConfigMatrix
-- MatrixDecimal；按 Id 轮换常规、边界、默认值
---@field DecimalCube ZrConfigMatrix
-- MatrixNullableUInt；按 Id 轮换常规、边界、默认值
---@field OptionalMatrix ZrConfigMatrix
-- RawJson；按 Id 轮换常规、边界、默认值
---@field RawJsonValue string
-- RawJson；按 Id 轮换常规、边界、默认值
---@field RawJsonArray (string)[]
-- RawJson；按 Id 轮换常规、边界、默认值
---@field RawJsonArray2D ((string)[])[]
-- RawJson；按 Id 轮换常规、边界、默认值
---@field RawJsonArray3D (((string)[])[])[]
-- Int；按 Id 轮换常规、边界、默认值
---@field JsonIntArray (integer)[]
-- 常规=1，边界=2，默认=0；引用 Hero 表
---@field Hero HeroConfig|ZrConfigNull
---@field HeroId integer
-- 常规=10，边界=11，默认=0；引用 Skill 表
---@field Skill SkillConfig|ZrConfigNull
---@field SkillId integer
-- Id=1 自引用，其余引用 Id-1；用于按需加载引用链
---@field Previous AllTypes10kConfig|ZrConfigNull
---@field PreviousId integer
local schema = ([=[{"name":"AllTypes10k","hash":"34B94C4229D85CC8B37832A203504B7E6EF4447307ADB831AB5BD01628EA7432","hasId":true,"fields":[{"name":"Id","type":"System.Int32","description":"唯一主键：1～10000"},{"name":"ByteValue","type":"System.Byte","description":"Byte；按 Id 轮换常规、边界、默认值"},{"name":"ShortValue","type":"System.Int16","description":"Short；按 Id 轮换常规、边界、默认值"},{"name":"IntValue","type":"System.Int32","description":"Int；按 Id 轮换常规、边界、默认值"},{"name":"LongValue","type":"System.Int64","description":"Long；按 Id 轮换常规、边界、默认值"},{"name":"FloatValue","type":"System.Single","description":"Float；按 Id 轮换常规、边界、默认值"},{"name":"DoubleValue","type":"System.Double","description":"Double；按 Id 轮换常规、边界、默认值"},{"name":"BoolValue","type":"System.Boolean","description":"Bool；按 Id 轮换常规、边界、默认值"},{"name":"StringValue","type":"System.String","description":"String；按 Id 轮换常规、边界、默认值"},{"name":"SByteValue","type":"System.SByte","description":"SByte；按 Id 轮换常规、边界、默认值"},{"name":"UShortValue","type":"System.UInt16","description":"UShort；按 Id 轮换常规、边界、默认值"},{"name":"UIntValue","type":"System.UInt32","description":"UInt；按 Id 轮换常规、边界、默认值"},{"name":"ULongValue","type":"System.UInt64","description":"ULong；按 Id 轮换常规、边界、默认值"},{"name":"DecimalValue","type":"System.Decimal","description":"Decimal；按 Id 轮换常规、边界、默认值"},{"name":"QualityValue","type":"Game.Config.Quality","description":"Quality；按 Id 轮换常规、边界、默认值"},{"name":"Vector2Value","type":"ZrConfig.Types.Vector2","description":"Vector2；按 Id 轮换常规、边界、默认值"},{"name":"Vector3Value","type":"ZrConfig.Types.Vector3","description":"Vector3；按 Id 轮换常规、边界、默认值"},{"name":"KvValue","type":"ZrConfig.Types.Kv","description":"Kv；按 Id 轮换常规、边界、默认值"},{"name":"MoneyValue","type":"Game.Config.Money","description":"Money；按 Id 轮换常规、边界、默认值"},{"name":"ByteArray","type":"System.Byte[]","description":"Byte；按 Id 轮换常规、边界、默认值"},{"name":"ShortArray","type":"System.Int16[]","description":"Short；按 Id 轮换常规、边界、默认值"},{"name":"IntArray","type":"System.Int32[]","description":"Int；按 Id 轮换常规、边界、默认值"},{"name":"LongArray","type":"System.Int64[]","description":"Long；按 Id 轮换常规、边界、默认值"},{"name":"FloatArray","type":"System.Single[]","description":"Float；按 Id 轮换常规、边界、默认值"},{"name":"DoubleArray","type":"System.Double[]","description":"Double；按 Id 轮换常规、边界、默认值"},{"name":"BoolArray","type":"System.Boolean[]","description":"Bool；按 Id 轮换常规、边界、默认值"},{"name":"StringArray","type":"System.String[]","description":"String；按 Id 轮换常规、边界、默认值"},{"name":"SByteArray","type":"System.SByte[]","description":"SByte；按 Id 轮换常规、边界、默认值"},{"name":"UShortArray","type":"System.UInt16[]","description":"UShort；按 Id 轮换常规、边界、默认值"},{"name":"UIntArray","type":"System.UInt32[]","description":"UInt；按 Id 轮换常规、边界、默认值"},{"name":"ULongArray","type":"System.UInt64[]","description":"ULong；按 Id 轮换常规、边界、默认值"},{"name":"DecimalArray","type":"System.Decimal[]","description":"Decimal；按 Id 轮换常规、边界、默认值"},{"name":"QualityArray","type":"Game.Config.Quality[]","description":"Quality；按 Id 轮换常规、边界、默认值"},{"name":"Vector2Array","type":"ZrConfig.Types.Vector2[]","description":"Vector2；按 Id 轮换常规、边界、默认值"},{"name":"Vector3Array","type":"ZrConfig.Types.Vector3[]","description":"Vector3；按 Id 轮换常规、边界、默认值"},{"name":"KvArray","type":"ZrConfig.Types.Kv[]","description":"Kv；按 Id 轮换常规、边界、默认值"},{"name":"MoneyArray","type":"Game.Config.Money[]","description":"Money；按 Id 轮换常规、边界、默认值"},{"name":"ByteArray2D","type":"System.Byte[][]","description":"Byte；按 Id 轮换常规、边界、默认值"},{"name":"ShortArray2D","type":"System.Int16[][]","description":"Short；按 Id 轮换常规、边界、默认值"},{"name":"IntArray2D","type":"System.Int32[][]","description":"Int；按 Id 轮换常规、边界、默认值"},{"name":"LongArray2D","type":"System.Int64[][]","description":"Long；按 Id 轮换常规、边界、默认值"},{"name":"FloatArray2D","type":"System.Single[][]","description":"Float；按 Id 轮换常规、边界、默认值"},{"name":"DoubleArray2D","type":"System.Double[][]","description":"Double；按 Id 轮换常规、边界、默认值"},{"name":"BoolArray2D","type":"System.Boolean[][]","description":"Bool；按 Id 轮换常规、边界、默认值"},{"name":"StringArray2D","type":"System.String[][]","description":"String；按 Id 轮换常规、边界、默认值"},{"name":"SByteArray2D","type":"System.SByte[][]","description":"SByte；按 Id 轮换常规、边界、默认值"},{"name":"UShortArray2D","type":"System.UInt16[][]","description":"UShort；按 Id 轮换常规、边界、默认值"},{"name":"UIntArray2D","type":"System.UInt32[][]","description":"UInt；按 Id 轮换常规、边界、默认值"},{"name":"ULongArray2D","type":"System.UInt64[][]","description":"ULong；按 Id 轮换常规、边界、默认值"},{"name":"DecimalArray2D","type":"System.Decimal[][]","description":"Decimal；按 Id 轮换常规、边界、默认值"},{"name":"QualityArray2D","type":"Game.Config.Quality[][]","description":"Quality；按 Id 轮换常规、边界、默认值"},{"name":"Vector2Array2D","type":"ZrConfig.Types.Vector2[][]","description":"Vector2；按 Id 轮换常规、边界、默认值"},{"name":"Vector3Array2D","type":"ZrConfig.Types.Vector3[][]","description":"Vector3；按 Id 轮换常规、边界、默认值"},{"name":"KvArray2D","type":"ZrConfig.Types.Kv[][]","description":"Kv；按 Id 轮换常规、边界、默认值"},{"name":"MoneyArray2D","type":"Game.Config.Money[][]","description":"Money；按 Id 轮换常规、边界、默认值"},{"name":"ByteArray3D","type":"System.Byte[][][]","description":"Byte；按 Id 轮换常规、边界、默认值"},{"name":"ShortArray3D","type":"System.Int16[][][]","description":"Short；按 Id 轮换常规、边界、默认值"},{"name":"IntArray3D","type":"System.Int32[][][]","description":"Int；按 Id 轮换常规、边界、默认值"},{"name":"LongArray3D","type":"System.Int64[][][]","description":"Long；按 Id 轮换常规、边界、默认值"},{"name":"FloatArray3D","type":"System.Single[][][]","description":"Float；按 Id 轮换常规、边界、默认值"},{"name":"DoubleArray3D","type":"System.Double[][][]","description":"Double；按 Id 轮换常规、边界、默认值"},{"name":"BoolArray3D","type":"System.Boolean[][][]","description":"Bool；按 Id 轮换常规、边界、默认值"},{"name":"StringArray3D","type":"System.String[][][]","description":"String；按 Id 轮换常规、边界、默认值"},{"name":"SByteArray3D","type":"System.SByte[][][]","description":"SByte；按 Id 轮换常规、边界、默认值"},{"name":"UShortArray3D","type":"System.UInt16[][][]","description":"UShort；按 Id 轮换常规、边界、默认值"},{"name":"UIntArray3D","type":"System.UInt32[][][]","description":"UInt；按 Id 轮换常规、边界、默认值"},{"name":"ULongArray3D","type":"System.UInt64[][][]","description":"ULong；按 Id 轮换常规、边界、默认值"},{"name":"DecimalArray3D","type":"System.Decimal[][][]","description":"Decimal；按 Id 轮换常规、边界、默认值"},{"name":"QualityArray3D","type":"Game.Config.Quality[][][]","description":"Quality；按 Id 轮换常规、边界、默认值"},{"name":"Vector2Array3D","type":"ZrConfig.Types.Vector2[][][]","description":"Vector2；按 Id 轮换常规、边界、默认值"},{"name":"Vector3Array3D","type":"ZrConfig.Types.Vector3[][][]","description":"Vector3；按 Id 轮换常规、边界、默认值"},{"name":"KvArray3D","type":"ZrConfig.Types.Kv[][][]","description":"Kv；按 Id 轮换常规、边界、默认值"},{"name":"MoneyArray3D","type":"Game.Config.Money[][][]","description":"Money；按 Id 轮换常规、边界、默认值"},{"name":"Settings","type":"Game.Config.BattleSettings","description":"完整对象或列表；单格 JSON 输入，导出为声明的强类型"},{"name":"Range","type":"Game.Config.ConfigRange","description":"完整对象或列表；单格 JSON 输入，导出为声明的强类型"},{"name":"Options","type":"Game.Config.SampleOptions","description":"完整对象或列表；单格 JSON 输入，导出为声明的强类型"},{"name":"Action","type":"Game.Config.BattleAction","description":"完整对象或列表；单格 JSON 输入，导出为声明的强类型"},{"name":"Actions","type":"Game.Config.BattleAction[]","description":"完整对象或列表；单格 JSON 输入，导出为声明的强类型"},{"name":"Ranges","type":"Game.Config.ConfigRange[]","description":"完整对象或列表；单格 JSON 输入，导出为声明的强类型"},{"name":"OptionsList","type":"Game.Config.SampleOptions[]","description":"完整对象或列表；单格 JSON 输入，导出为声明的强类型"},{"name":"Groups","type":"Game.Config.ActionGroup[]","description":"完整对象或列表；单格 JSON 输入，导出为声明的强类型"},{"name":"Phases","type":"Game.Config.BattlePhase[]","description":"完整对象或列表；单格 JSON 输入，导出为声明的强类型"},{"name":"OptionalRange","type":"System.Nullable`1<Game.Config.ConfigRange>","description":"完整对象或列表；单格 JSON 输入，导出为声明的强类型"},{"name":"OptionalInt","type":"System.Nullable`1<System.Int32>","description":"NullableInt；按 Id 轮换常规、边界、默认值"},{"name":"OptionalUInt","type":"System.Nullable`1<System.UInt32>","description":"NullableUInt；按 Id 轮换常规、边界、默认值"},{"name":"OptionalULong","type":"System.Nullable`1<System.UInt64>","description":"NullableULong；按 Id 轮换常规、边界、默认值"},{"name":"OptionalDecimal","type":"System.Nullable`1<System.Decimal>","description":"NullableDecimal；按 Id 轮换常规、边界、默认值"},{"name":"OptionalBool","type":"System.Nullable`1<System.Boolean>","description":"NullableBool；按 Id 轮换常规、边界、默认值"},{"name":"OptionalQuality","type":"System.Nullable`1<Game.Config.Quality>","description":"NullableQuality；按 Id 轮换常规、边界、默认值"},{"name":"OptionalMoney","type":"System.Nullable`1<Game.Config.Money>","description":"NullableMoney；按 Id 轮换常规、边界、默认值"},{"name":"OptionalValues","type":"System.Nullable`1<System.Int32>[]","description":"NullableIntArray；按 Id 轮换常规、边界、默认值"},{"name":"StatMap","type":"System.Collections.Generic.Dictionary`2<System.String,System.Int32>","description":"MapInt；按 Id 轮换常规、边界、默认值"},{"name":"PriceMap","type":"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>","description":"MapNullableDecimal；按 Id 轮换常规、边界、默认值"},{"name":"QualityMap","type":"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>","description":"MapUInt；按 Id 轮换常规、边界、默认值"},{"name":"ListMap","type":"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>","description":"MapIntArray；按 Id 轮换常规、边界、默认值"},{"name":"IntMatrix","type":"System.Int32[,]","description":"MatrixInt；按 Id 轮换常规、边界、默认值"},{"name":"DecimalCube","type":"System.Decimal[,,]","description":"MatrixDecimal；按 Id 轮换常规、边界、默认值"},{"name":"OptionalMatrix","type":"System.Nullable`1<System.UInt32>[,]","description":"MatrixNullableUInt；按 Id 轮换常规、边界、默认值"},{"name":"RawJsonValue","type":"ZrConfig.Types.RawJson","description":"RawJson；按 Id 轮换常规、边界、默认值"},{"name":"RawJsonArray","type":"ZrConfig.Types.RawJson[]","description":"RawJson；按 Id 轮换常规、边界、默认值"},{"name":"RawJsonArray2D","type":"ZrConfig.Types.RawJson[][]","description":"RawJson；按 Id 轮换常规、边界、默认值"},{"name":"RawJsonArray3D","type":"ZrConfig.Types.RawJson[][][]","description":"RawJson；按 Id 轮换常规、边界、默认值"},{"name":"JsonIntArray","type":"System.Int32[]","description":"Int；按 Id 轮换常规、边界、默认值"},{"name":"Hero","type":"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1","description":"常规=1，边界=2，默认=0；引用 Hero 表"},{"name":"Skill","type":"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1","description":"常规=10，边界=11，默认=0；引用 Skill 表"},{"name":"Previous","type":"ref:ZrConfig.Models.AllTypes10kConfig:Id:int:zero-null/1","description":"Id=1 自引用，其余引用 Id-1；用于按需加载引用链"}],"keys":["Id"],"groups":[{"name":"ById","fields":["Id"]}],"types":[{"id":"Game.Config.ActionGroup","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"Name","type":"System.String","description":""},{"name":"Actions","type":"Game.Config.BattleAction[]","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.ActionGroup[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.ActionGroup","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.AddBuff","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"Delay","type":"System.Single","description":""},{"name":"BuffId","type":"System.Int32","description":""},{"name":"Stacks","type":"System.Int32","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":"Game.Config.BattleAction","enumMembers":{},"referenceTable":null},{"id":"Game.Config.BattleAction","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"Delay","type":"System.Single","description":""}],"variants":["Game.Config.AddBuff","Game.Config.Damage"],"enumValues":[],"abstract":true,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.BattleAction[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.BattleAction","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.BattlePhase","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"Name","type":"System.String","description":""},{"name":"Groups","type":"Game.Config.ActionGroup[]","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.BattlePhase[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.BattlePhase","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.BattleSettings","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"MaxTargets","type":"System.Int32","description":""},{"name":"Interruptible","type":"System.Boolean","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.ConfigRange","kind":"object","primitive":"","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[{"name":"Min","type":"System.Int32","description":""},{"name":"Max","type":"System.Int32","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.ConfigRange[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.ConfigRange","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Damage","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"Delay","type":"System.Single","description":""},{"name":"Ratio","type":"System.Single","description":""},{"name":"CanCrit","type":"System.Boolean","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":"Game.Config.BattleAction","enumMembers":{},"referenceTable":null},{"id":"Game.Config.Money","kind":"converter","primitive":"","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[{"name":"Cents","type":"System.Int32","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Money[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.Money","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Money[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.Money[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Money[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.Money[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Quality","kind":"enum","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":["0","1","2"],"abstract":false,"baseType":null,"enumMembers":{"None":"0","Rare":"1","Legendary":"2"},"referenceTable":null},{"id":"Game.Config.Quality[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.Quality","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Quality[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.Quality[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.Quality[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.Quality[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.SampleOptions","kind":"object","primitive":"","nullableObject":true,"rank":0,"element":null,"key":null,"fields":[{"name":"Range","type":"Game.Config.ConfigRange","description":""},{"name":"Rewards","type":"ZrConfig.Types.Kv[]","description":""},{"name":"Offset","type":"ZrConfig.Types.Vector2","description":""},{"name":"Label","type":"System.String","description":""},{"name":"Quality","type":"Game.Config.Quality","description":""},{"name":"Levels","type":"System.Int32[]","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"Game.Config.SampleOptions[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"Game.Config.SampleOptions","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Boolean","kind":"primitive","primitive":"bool","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Boolean[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Boolean","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Boolean[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Boolean[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Boolean[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Boolean[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Byte","kind":"primitive","primitive":"uint8","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Byte[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Byte","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Byte[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Byte[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Byte[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Byte[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>","kind":"dictionary","primitive":"","nullableObject":false,"rank":0,"element":"System.UInt32","key":"Game.Config.Quality","fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>","kind":"dictionary","primitive":"","nullableObject":false,"rank":0,"element":"System.Nullable`1<System.Decimal>","key":"System.Int32","fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Collections.Generic.Dictionary`2<System.String,System.Int32>","kind":"dictionary","primitive":"","nullableObject":false,"rank":0,"element":"System.Int32","key":"System.String","fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>","kind":"dictionary","primitive":"","nullableObject":false,"rank":0,"element":"System.Int32[]","key":"System.String","fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Decimal","kind":"primitive","primitive":"decimal","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Decimal[,,]","kind":"array","primitive":"","nullableObject":false,"rank":3,"element":"System.Decimal","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Decimal[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Decimal","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Decimal[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Decimal[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Decimal[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Decimal[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Double","kind":"primitive","primitive":"float64","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Double[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Double","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Double[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Double[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Double[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Double[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int16","kind":"primitive","primitive":"int16","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int16[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int16","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int16[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int16[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int16[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int16[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int32","kind":"primitive","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int32[,]","kind":"array","primitive":"","nullableObject":false,"rank":2,"element":"System.Int32","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int32[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int32","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int32[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int32[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int32[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int32[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int64","kind":"primitive","primitive":"int64","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int64[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int64","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int64[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int64[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Int64[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Int64[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<Game.Config.ConfigRange>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"Game.Config.ConfigRange","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<Game.Config.Money>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"Game.Config.Money","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<Game.Config.Quality>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"Game.Config.Quality","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.Boolean>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"System.Boolean","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.Decimal>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"System.Decimal","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.Int32>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"System.Int32","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.Int32>[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Nullable`1<System.Int32>","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.UInt32>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"System.UInt32","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.UInt32>[,]","kind":"array","primitive":"","nullableObject":false,"rank":2,"element":"System.Nullable`1<System.UInt32>","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Nullable`1<System.UInt64>","kind":"nullable","primitive":"","nullableObject":false,"rank":0,"element":"System.UInt64","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.SByte","kind":"primitive","primitive":"int8","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.SByte[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.SByte","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.SByte[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.SByte[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.SByte[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.SByte[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Single","kind":"primitive","primitive":"float32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Single[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Single","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Single[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Single[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.Single[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.Single[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.String","kind":"primitive","primitive":"string","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.String[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.String","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.String[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.String[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.String[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.String[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt16","kind":"primitive","primitive":"uint16","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt16[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt16","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt16[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt16[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt16[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt16[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt32","kind":"primitive","primitive":"uint32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt32[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt32","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt32[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt32[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt32[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt32[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt64","kind":"primitive","primitive":"uint64","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt64[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt64","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt64[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt64[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"System.UInt64[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"System.UInt64[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Kv","kind":"kv","primitive":"","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[{"name":"Key","type":"System.Int32","description":""},{"name":"Value","type":"System.Int32","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Kv[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Kv","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Kv[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Kv[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Kv[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Kv[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.RawJson","kind":"rawjson","primitive":"","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.RawJson[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.RawJson","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.RawJson[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.RawJson[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.RawJson[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.RawJson[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector2","kind":"vector2","primitive":"","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[{"name":"X","type":"System.Single","description":""},{"name":"Y","type":"System.Single","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector2[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Vector2","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector2[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Vector2[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector2[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Vector2[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector3","kind":"vector3","primitive":"","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[{"name":"X","type":"System.Single","description":""},{"name":"Y","type":"System.Single","description":""},{"name":"Z","type":"System.Single","description":""}],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector3[]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Vector3","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector3[][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Vector3[]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ZrConfig.Types.Vector3[][][]","kind":"array","primitive":"","nullableObject":false,"rank":1,"element":"ZrConfig.Types.Vector3[][]","key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":null},{"id":"ref:ZrConfig.Models.AllTypes10kConfig:Id:int:zero-null/1","kind":"tablereference","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":"AllTypes10k"},{"id":"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1","kind":"tablereference","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":"Hero"},{"id":"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1","kind":"tablereference","primitive":"int32","nullableObject":false,"rank":0,"element":null,"key":null,"fields":[],"variants":[],"enumValues":[],"abstract":false,"baseType":null,"enumMembers":{},"referenceTable":"Skill"}],"tags":["Game.Config.AddBuff","Game.Config.Damage"],"dependencies":["AllTypes10k","Hero","Skill"]}]=])
return function(bundle, prefer_json)
    local rows = bundle.load(schema, prefer_json)
    local result = {rows=rows, count=#rows}

    local index_Key = R.index(rows, {([=[Id]=])}, true)
    local index_Group_ById = R.index(rows, {([=[Id]=])}, false)
    local ids = {}
    for _,row in ipairs(rows) do if ids[row.Id] then R.invalid('引用 Id 不唯一') end; ids[row.Id]=row end
    function result.byId(id) return ids[id] end
    for _,row in ipairs(rows) do row[([=[HeroId]=])] = row[([=[Hero]=])]; row[([=[Hero]=])]=R.NULL end
    for _,row in ipairs(rows) do row[([=[SkillId]=])] = row[([=[Skill]=])]; row[([=[Skill]=])]=R.NULL end
    for _,row in ipairs(rows) do row[([=[PreviousId]=])] = row[([=[Previous]=])]; row[([=[Previous]=])]=R.NULL end
    function result.bind(snapshot)
        for _,row in ipairs(rows) do if row[([=[HeroId]=])] ~= 0 then row[([=[Hero]=])] = snapshot[([=[Hero]=])].byId(row[([=[HeroId]=])]) or R.invalid(([=[引用目标不存在：AllTypes10k.Hero]=])) end end
        for _,row in ipairs(rows) do if row[([=[SkillId]=])] ~= 0 then row[([=[Skill]=])] = snapshot[([=[Skill]=])].byId(row[([=[SkillId]=])]) or R.invalid(([=[引用目标不存在：AllTypes10k.Skill]=])) end end
        for _,row in ipairs(rows) do if row[([=[PreviousId]=])] ~= 0 then row[([=[Previous]=])] = snapshot[([=[AllTypes10k]=])].byId(row[([=[PreviousId]=])]) or R.invalid(([=[引用目标不存在：AllTypes10k.Previous]=])) end end
    end
    function result.seal(seen)
        rows=R.readonly(rows, seen); result.rows=rows
        index_Key=R.readonly(index_Key, seen)
        index_Group_ById=R.readonly(index_Group_ById, seen)
        ids=R.readonly(ids, seen)
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
    return result
end
