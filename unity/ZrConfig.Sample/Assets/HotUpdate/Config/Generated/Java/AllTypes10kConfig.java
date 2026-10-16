// zrconfig-api-contract:1
package zrconfig;
import java.util.*;
// 此文件由工具自动生成，请勿手工修改。
public final class AllTypes10kConfig {
    private ConfigSnapshot serverOwner;
    private final int v_Id;
    // 唯一主键：1～10000
    public int getId() { return v_Id; }
    private final short v_ByteValue;
    // Byte；按 Id 轮换常规、边界、默认值
    public short getByteValue() { return v_ByteValue; }
    private final short v_ShortValue;
    // Short；按 Id 轮换常规、边界、默认值
    public short getShortValue() { return v_ShortValue; }
    private final int v_IntValue;
    // Int；按 Id 轮换常规、边界、默认值
    public int getIntValue() { return v_IntValue; }
    private final long v_LongValue;
    // Long；按 Id 轮换常规、边界、默认值
    public long getLongValue() { return v_LongValue; }
    private final float v_FloatValue;
    // Float；按 Id 轮换常规、边界、默认值
    public float getFloatValue() { return v_FloatValue; }
    private final double v_DoubleValue;
    // Double；按 Id 轮换常规、边界、默认值
    public double getDoubleValue() { return v_DoubleValue; }
    private final boolean v_BoolValue;
    // Bool；按 Id 轮换常规、边界、默认值
    public boolean getBoolValue() { return v_BoolValue; }
    private final String v_StringValue;
    // String；按 Id 轮换常规、边界、默认值
    public String getStringValue() { return v_StringValue; }
    private final byte v_SByteValue;
    // SByte；按 Id 轮换常规、边界、默认值
    public byte getSByteValue() { return v_SByteValue; }
    private final int v_UShortValue;
    // UShort；按 Id 轮换常规、边界、默认值
    public int getUShortValue() { return v_UShortValue; }
    private final long v_UIntValue;
    // UInt；按 Id 轮换常规、边界、默认值
    public long getUIntValue() { return v_UIntValue; }
    private final java.math.BigInteger v_ULongValue;
    // ULong；按 Id 轮换常规、边界、默认值
    public java.math.BigInteger getULongValue() { return v_ULongValue; }
    private final ConfigRuntime.Decimal v_DecimalValue;
    // Decimal；按 Id 轮换常规、边界、默认值
    public ConfigRuntime.Decimal getDecimalValue() { return v_DecimalValue; }
    private final ConfigTypes.T_Game_Config_Quality v_QualityValue;
    // Quality；按 Id 轮换常规、边界、默认值
    public ConfigTypes.T_Game_Config_Quality getQualityValue() { return v_QualityValue; }
    private final ConfigTypes.T_ZrConfig_Types_Vector2 v_Vector2Value;
    // Vector2；按 Id 轮换常规、边界、默认值
    public ConfigTypes.T_ZrConfig_Types_Vector2 getVector2Value() { return v_Vector2Value; }
    private final ConfigTypes.T_ZrConfig_Types_Vector3 v_Vector3Value;
    // Vector3；按 Id 轮换常规、边界、默认值
    public ConfigTypes.T_ZrConfig_Types_Vector3 getVector3Value() { return v_Vector3Value; }
    private final ConfigTypes.T_ZrConfig_Types_Kv v_KvValue;
    // Kv；按 Id 轮换常规、边界、默认值
    public ConfigTypes.T_ZrConfig_Types_Kv getKvValue() { return v_KvValue; }
    private final ConfigTypes.T_Game_Config_Money v_MoneyValue;
    // Money；按 Id 轮换常规、边界、默认值
    public ConfigTypes.T_Game_Config_Money getMoneyValue() { return v_MoneyValue; }
    private final List<Short> v_ByteArray;
    // Byte；按 Id 轮换常规、边界、默认值
    public List<Short> getByteArray() { return v_ByteArray; }
    private final List<Short> v_ShortArray;
    // Short；按 Id 轮换常规、边界、默认值
    public List<Short> getShortArray() { return v_ShortArray; }
    private final List<Integer> v_IntArray;
    // Int；按 Id 轮换常规、边界、默认值
    public List<Integer> getIntArray() { return v_IntArray; }
    private final List<Long> v_LongArray;
    // Long；按 Id 轮换常规、边界、默认值
    public List<Long> getLongArray() { return v_LongArray; }
    private final List<Float> v_FloatArray;
    // Float；按 Id 轮换常规、边界、默认值
    public List<Float> getFloatArray() { return v_FloatArray; }
    private final List<Double> v_DoubleArray;
    // Double；按 Id 轮换常规、边界、默认值
    public List<Double> getDoubleArray() { return v_DoubleArray; }
    private final List<Boolean> v_BoolArray;
    // Bool；按 Id 轮换常规、边界、默认值
    public List<Boolean> getBoolArray() { return v_BoolArray; }
    private final List<String> v_StringArray;
    // String；按 Id 轮换常规、边界、默认值
    public List<String> getStringArray() { return v_StringArray; }
    private final List<Byte> v_SByteArray;
    // SByte；按 Id 轮换常规、边界、默认值
    public List<Byte> getSByteArray() { return v_SByteArray; }
    private final List<Integer> v_UShortArray;
    // UShort；按 Id 轮换常规、边界、默认值
    public List<Integer> getUShortArray() { return v_UShortArray; }
    private final List<Long> v_UIntArray;
    // UInt；按 Id 轮换常规、边界、默认值
    public List<Long> getUIntArray() { return v_UIntArray; }
    private final List<java.math.BigInteger> v_ULongArray;
    // ULong；按 Id 轮换常规、边界、默认值
    public List<java.math.BigInteger> getULongArray() { return v_ULongArray; }
    private final List<ConfigRuntime.Decimal> v_DecimalArray;
    // Decimal；按 Id 轮换常规、边界、默认值
    public List<ConfigRuntime.Decimal> getDecimalArray() { return v_DecimalArray; }
    private final List<ConfigTypes.T_Game_Config_Quality> v_QualityArray;
    // Quality；按 Id 轮换常规、边界、默认值
    public List<ConfigTypes.T_Game_Config_Quality> getQualityArray() { return v_QualityArray; }
    private final List<ConfigTypes.T_ZrConfig_Types_Vector2> v_Vector2Array;
    // Vector2；按 Id 轮换常规、边界、默认值
    public List<ConfigTypes.T_ZrConfig_Types_Vector2> getVector2Array() { return v_Vector2Array; }
    private final List<ConfigTypes.T_ZrConfig_Types_Vector3> v_Vector3Array;
    // Vector3；按 Id 轮换常规、边界、默认值
    public List<ConfigTypes.T_ZrConfig_Types_Vector3> getVector3Array() { return v_Vector3Array; }
    private final List<ConfigTypes.T_ZrConfig_Types_Kv> v_KvArray;
    // Kv；按 Id 轮换常规、边界、默认值
    public List<ConfigTypes.T_ZrConfig_Types_Kv> getKvArray() { return v_KvArray; }
    private final List<ConfigTypes.T_Game_Config_Money> v_MoneyArray;
    // Money；按 Id 轮换常规、边界、默认值
    public List<ConfigTypes.T_Game_Config_Money> getMoneyArray() { return v_MoneyArray; }
    private final List<List<Short>> v_ByteArray2D;
    // Byte；按 Id 轮换常规、边界、默认值
    public List<List<Short>> getByteArray2D() { return v_ByteArray2D; }
    private final List<List<Short>> v_ShortArray2D;
    // Short；按 Id 轮换常规、边界、默认值
    public List<List<Short>> getShortArray2D() { return v_ShortArray2D; }
    private final List<List<Integer>> v_IntArray2D;
    // Int；按 Id 轮换常规、边界、默认值
    public List<List<Integer>> getIntArray2D() { return v_IntArray2D; }
    private final List<List<Long>> v_LongArray2D;
    // Long；按 Id 轮换常规、边界、默认值
    public List<List<Long>> getLongArray2D() { return v_LongArray2D; }
    private final List<List<Float>> v_FloatArray2D;
    // Float；按 Id 轮换常规、边界、默认值
    public List<List<Float>> getFloatArray2D() { return v_FloatArray2D; }
    private final List<List<Double>> v_DoubleArray2D;
    // Double；按 Id 轮换常规、边界、默认值
    public List<List<Double>> getDoubleArray2D() { return v_DoubleArray2D; }
    private final List<List<Boolean>> v_BoolArray2D;
    // Bool；按 Id 轮换常规、边界、默认值
    public List<List<Boolean>> getBoolArray2D() { return v_BoolArray2D; }
    private final List<List<String>> v_StringArray2D;
    // String；按 Id 轮换常规、边界、默认值
    public List<List<String>> getStringArray2D() { return v_StringArray2D; }
    private final List<List<Byte>> v_SByteArray2D;
    // SByte；按 Id 轮换常规、边界、默认值
    public List<List<Byte>> getSByteArray2D() { return v_SByteArray2D; }
    private final List<List<Integer>> v_UShortArray2D;
    // UShort；按 Id 轮换常规、边界、默认值
    public List<List<Integer>> getUShortArray2D() { return v_UShortArray2D; }
    private final List<List<Long>> v_UIntArray2D;
    // UInt；按 Id 轮换常规、边界、默认值
    public List<List<Long>> getUIntArray2D() { return v_UIntArray2D; }
    private final List<List<java.math.BigInteger>> v_ULongArray2D;
    // ULong；按 Id 轮换常规、边界、默认值
    public List<List<java.math.BigInteger>> getULongArray2D() { return v_ULongArray2D; }
    private final List<List<ConfigRuntime.Decimal>> v_DecimalArray2D;
    // Decimal；按 Id 轮换常规、边界、默认值
    public List<List<ConfigRuntime.Decimal>> getDecimalArray2D() { return v_DecimalArray2D; }
    private final List<List<ConfigTypes.T_Game_Config_Quality>> v_QualityArray2D;
    // Quality；按 Id 轮换常规、边界、默认值
    public List<List<ConfigTypes.T_Game_Config_Quality>> getQualityArray2D() { return v_QualityArray2D; }
    private final List<List<ConfigTypes.T_ZrConfig_Types_Vector2>> v_Vector2Array2D;
    // Vector2；按 Id 轮换常规、边界、默认值
    public List<List<ConfigTypes.T_ZrConfig_Types_Vector2>> getVector2Array2D() { return v_Vector2Array2D; }
    private final List<List<ConfigTypes.T_ZrConfig_Types_Vector3>> v_Vector3Array2D;
    // Vector3；按 Id 轮换常规、边界、默认值
    public List<List<ConfigTypes.T_ZrConfig_Types_Vector3>> getVector3Array2D() { return v_Vector3Array2D; }
    private final List<List<ConfigTypes.T_ZrConfig_Types_Kv>> v_KvArray2D;
    // Kv；按 Id 轮换常规、边界、默认值
    public List<List<ConfigTypes.T_ZrConfig_Types_Kv>> getKvArray2D() { return v_KvArray2D; }
    private final List<List<ConfigTypes.T_Game_Config_Money>> v_MoneyArray2D;
    // Money；按 Id 轮换常规、边界、默认值
    public List<List<ConfigTypes.T_Game_Config_Money>> getMoneyArray2D() { return v_MoneyArray2D; }
    private final List<List<List<Short>>> v_ByteArray3D;
    // Byte；按 Id 轮换常规、边界、默认值
    public List<List<List<Short>>> getByteArray3D() { return v_ByteArray3D; }
    private final List<List<List<Short>>> v_ShortArray3D;
    // Short；按 Id 轮换常规、边界、默认值
    public List<List<List<Short>>> getShortArray3D() { return v_ShortArray3D; }
    private final List<List<List<Integer>>> v_IntArray3D;
    // Int；按 Id 轮换常规、边界、默认值
    public List<List<List<Integer>>> getIntArray3D() { return v_IntArray3D; }
    private final List<List<List<Long>>> v_LongArray3D;
    // Long；按 Id 轮换常规、边界、默认值
    public List<List<List<Long>>> getLongArray3D() { return v_LongArray3D; }
    private final List<List<List<Float>>> v_FloatArray3D;
    // Float；按 Id 轮换常规、边界、默认值
    public List<List<List<Float>>> getFloatArray3D() { return v_FloatArray3D; }
    private final List<List<List<Double>>> v_DoubleArray3D;
    // Double；按 Id 轮换常规、边界、默认值
    public List<List<List<Double>>> getDoubleArray3D() { return v_DoubleArray3D; }
    private final List<List<List<Boolean>>> v_BoolArray3D;
    // Bool；按 Id 轮换常规、边界、默认值
    public List<List<List<Boolean>>> getBoolArray3D() { return v_BoolArray3D; }
    private final List<List<List<String>>> v_StringArray3D;
    // String；按 Id 轮换常规、边界、默认值
    public List<List<List<String>>> getStringArray3D() { return v_StringArray3D; }
    private final List<List<List<Byte>>> v_SByteArray3D;
    // SByte；按 Id 轮换常规、边界、默认值
    public List<List<List<Byte>>> getSByteArray3D() { return v_SByteArray3D; }
    private final List<List<List<Integer>>> v_UShortArray3D;
    // UShort；按 Id 轮换常规、边界、默认值
    public List<List<List<Integer>>> getUShortArray3D() { return v_UShortArray3D; }
    private final List<List<List<Long>>> v_UIntArray3D;
    // UInt；按 Id 轮换常规、边界、默认值
    public List<List<List<Long>>> getUIntArray3D() { return v_UIntArray3D; }
    private final List<List<List<java.math.BigInteger>>> v_ULongArray3D;
    // ULong；按 Id 轮换常规、边界、默认值
    public List<List<List<java.math.BigInteger>>> getULongArray3D() { return v_ULongArray3D; }
    private final List<List<List<ConfigRuntime.Decimal>>> v_DecimalArray3D;
    // Decimal；按 Id 轮换常规、边界、默认值
    public List<List<List<ConfigRuntime.Decimal>>> getDecimalArray3D() { return v_DecimalArray3D; }
    private final List<List<List<ConfigTypes.T_Game_Config_Quality>>> v_QualityArray3D;
    // Quality；按 Id 轮换常规、边界、默认值
    public List<List<List<ConfigTypes.T_Game_Config_Quality>>> getQualityArray3D() { return v_QualityArray3D; }
    private final List<List<List<ConfigTypes.T_ZrConfig_Types_Vector2>>> v_Vector2Array3D;
    // Vector2；按 Id 轮换常规、边界、默认值
    public List<List<List<ConfigTypes.T_ZrConfig_Types_Vector2>>> getVector2Array3D() { return v_Vector2Array3D; }
    private final List<List<List<ConfigTypes.T_ZrConfig_Types_Vector3>>> v_Vector3Array3D;
    // Vector3；按 Id 轮换常规、边界、默认值
    public List<List<List<ConfigTypes.T_ZrConfig_Types_Vector3>>> getVector3Array3D() { return v_Vector3Array3D; }
    private final List<List<List<ConfigTypes.T_ZrConfig_Types_Kv>>> v_KvArray3D;
    // Kv；按 Id 轮换常规、边界、默认值
    public List<List<List<ConfigTypes.T_ZrConfig_Types_Kv>>> getKvArray3D() { return v_KvArray3D; }
    private final List<List<List<ConfigTypes.T_Game_Config_Money>>> v_MoneyArray3D;
    // Money；按 Id 轮换常规、边界、默认值
    public List<List<List<ConfigTypes.T_Game_Config_Money>>> getMoneyArray3D() { return v_MoneyArray3D; }
    private final ConfigTypes.T_Game_Config_BattleSettings v_Settings;
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    public ConfigTypes.T_Game_Config_BattleSettings getSettings() { return v_Settings; }
    private final ConfigTypes.T_Game_Config_ConfigRange v_Range;
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    public ConfigTypes.T_Game_Config_ConfigRange getRange() { return v_Range; }
    private final ConfigTypes.T_Game_Config_SampleOptions v_Options;
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    public ConfigTypes.T_Game_Config_SampleOptions getOptions() { return v_Options; }
    private final ConfigTypes.T_Game_Config_BattleAction v_Action;
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    public ConfigTypes.T_Game_Config_BattleAction getAction() { return v_Action; }
    private final List<ConfigTypes.T_Game_Config_BattleAction> v_Actions;
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    public List<ConfigTypes.T_Game_Config_BattleAction> getActions() { return v_Actions; }
    private final List<ConfigTypes.T_Game_Config_ConfigRange> v_Ranges;
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    public List<ConfigTypes.T_Game_Config_ConfigRange> getRanges() { return v_Ranges; }
    private final List<ConfigTypes.T_Game_Config_SampleOptions> v_OptionsList;
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    public List<ConfigTypes.T_Game_Config_SampleOptions> getOptionsList() { return v_OptionsList; }
    private final List<ConfigTypes.T_Game_Config_ActionGroup> v_Groups;
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    public List<ConfigTypes.T_Game_Config_ActionGroup> getGroups() { return v_Groups; }
    private final List<ConfigTypes.T_Game_Config_BattlePhase> v_Phases;
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    public List<ConfigTypes.T_Game_Config_BattlePhase> getPhases() { return v_Phases; }
    private final ConfigTypes.T_Game_Config_ConfigRange v_OptionalRange;
    // 完整对象或列表；单格 JSON 输入，导出为声明的强类型
    public ConfigTypes.T_Game_Config_ConfigRange getOptionalRange() { return v_OptionalRange; }
    private final Integer v_OptionalInt;
    // NullableInt；按 Id 轮换常规、边界、默认值
    public Integer getOptionalInt() { return v_OptionalInt; }
    private final Long v_OptionalUInt;
    // NullableUInt；按 Id 轮换常规、边界、默认值
    public Long getOptionalUInt() { return v_OptionalUInt; }
    private final java.math.BigInteger v_OptionalULong;
    // NullableULong；按 Id 轮换常规、边界、默认值
    public java.math.BigInteger getOptionalULong() { return v_OptionalULong; }
    private final ConfigRuntime.Decimal v_OptionalDecimal;
    // NullableDecimal；按 Id 轮换常规、边界、默认值
    public ConfigRuntime.Decimal getOptionalDecimal() { return v_OptionalDecimal; }
    private final Boolean v_OptionalBool;
    // NullableBool；按 Id 轮换常规、边界、默认值
    public Boolean getOptionalBool() { return v_OptionalBool; }
    private final ConfigTypes.T_Game_Config_Quality v_OptionalQuality;
    // NullableQuality；按 Id 轮换常规、边界、默认值
    public ConfigTypes.T_Game_Config_Quality getOptionalQuality() { return v_OptionalQuality; }
    private final ConfigTypes.T_Game_Config_Money v_OptionalMoney;
    // NullableMoney；按 Id 轮换常规、边界、默认值
    public ConfigTypes.T_Game_Config_Money getOptionalMoney() { return v_OptionalMoney; }
    private final List<Integer> v_OptionalValues;
    // NullableIntArray；按 Id 轮换常规、边界、默认值
    public List<Integer> getOptionalValues() { return v_OptionalValues; }
    private final Map<String,Integer> v_StatMap;
    // MapInt；按 Id 轮换常规、边界、默认值
    public Map<String,Integer> getStatMap() { return v_StatMap; }
    private final Map<Integer,ConfigRuntime.Decimal> v_PriceMap;
    // MapNullableDecimal；按 Id 轮换常规、边界、默认值
    public Map<Integer,ConfigRuntime.Decimal> getPriceMap() { return v_PriceMap; }
    private final Map<ConfigTypes.T_Game_Config_Quality,Long> v_QualityMap;
    // MapUInt；按 Id 轮换常规、边界、默认值
    public Map<ConfigTypes.T_Game_Config_Quality,Long> getQualityMap() { return v_QualityMap; }
    private final Map<String,List<Integer>> v_ListMap;
    // MapIntArray；按 Id 轮换常规、边界、默认值
    public Map<String,List<Integer>> getListMap() { return v_ListMap; }
    private final ConfigRuntime.Matrix<Integer> v_IntMatrix;
    // MatrixInt；按 Id 轮换常规、边界、默认值
    public ConfigRuntime.Matrix<Integer> getIntMatrix() { return v_IntMatrix; }
    private final ConfigRuntime.Matrix<ConfigRuntime.Decimal> v_DecimalCube;
    // MatrixDecimal；按 Id 轮换常规、边界、默认值
    public ConfigRuntime.Matrix<ConfigRuntime.Decimal> getDecimalCube() { return v_DecimalCube; }
    private final ConfigRuntime.Matrix<Long> v_OptionalMatrix;
    // MatrixNullableUInt；按 Id 轮换常规、边界、默认值
    public ConfigRuntime.Matrix<Long> getOptionalMatrix() { return v_OptionalMatrix; }
    private final String v_RawJsonValue;
    // RawJson；按 Id 轮换常规、边界、默认值
    public String getRawJsonValue() { return v_RawJsonValue; }
    private final List<String> v_RawJsonArray;
    // RawJson；按 Id 轮换常规、边界、默认值
    public List<String> getRawJsonArray() { return v_RawJsonArray; }
    private final List<List<String>> v_RawJsonArray2D;
    // RawJson；按 Id 轮换常规、边界、默认值
    public List<List<String>> getRawJsonArray2D() { return v_RawJsonArray2D; }
    private final List<List<List<String>>> v_RawJsonArray3D;
    // RawJson；按 Id 轮换常规、边界、默认值
    public List<List<List<String>>> getRawJsonArray3D() { return v_RawJsonArray3D; }
    private final List<Integer> v_JsonIntArray;
    // Int；按 Id 轮换常规、边界、默认值
    public List<Integer> getJsonIntArray() { return v_JsonIntArray; }
    private final int v_Hero;
    // 常规=1，边界=2，默认=0；引用 Hero 表
    public int getHeroId() { return v_Hero; }
    private HeroConfig ref_Hero;
    public HeroConfig getHero() { return ref_Hero; }
    private final int v_Skill;
    // 常规=10，边界=11，默认=0；引用 Skill 表
    public int getSkillId() { return v_Skill; }
    private SkillConfig ref_Skill;
    public SkillConfig getSkill() { return ref_Skill; }
    private final int v_Previous;
    // Id=1 自引用，其余引用 Id-1；用于按需加载引用链
    public int getPreviousId() { return v_Previous; }
    private AllTypes10kConfig ref_Previous;
    public AllTypes10kConfig getPrevious() { return ref_Previous; }
    private AllTypes10kConfig(Object value) {
        v_Id = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Id"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_ByteValue = (short)ConfigRuntime.integer(ConfigRuntime.field(value, "ByteValue"),0,255);
        v_ShortValue = (short)ConfigRuntime.integer(ConfigRuntime.field(value, "ShortValue"),-32768,32767);
        v_IntValue = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "IntValue"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_LongValue = ConfigRuntime.int64(ConfigRuntime.field(value, "LongValue"));
        v_FloatValue = ConfigRuntime.float32(ConfigRuntime.field(value, "FloatValue"));
        v_DoubleValue = ConfigRuntime.float64(ConfigRuntime.field(value, "DoubleValue"));
        v_BoolValue = ConfigRuntime.bool(ConfigRuntime.field(value, "BoolValue"));
        v_StringValue = ConfigRuntime.string(ConfigRuntime.field(value, "StringValue"));
        v_SByteValue = (byte)ConfigRuntime.integer(ConfigRuntime.field(value, "SByteValue"),-128,127);
        v_UShortValue = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "UShortValue"),0,65535);
        v_UIntValue = ConfigRuntime.integer(ConfigRuntime.field(value, "UIntValue"),0,0xffffffffL);
        v_ULongValue = ConfigRuntime.uint64(ConfigRuntime.field(value, "ULongValue"));
        v_DecimalValue = new ConfigRuntime.Decimal(ConfigRuntime.string(ConfigRuntime.field(value, "DecimalValue")));
        v_QualityValue = ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(ConfigRuntime.field(value, "QualityValue"),Integer.MIN_VALUE,Integer.MAX_VALUE));
        v_Vector2Value = ConfigTypes.read_T_ZrConfig_Types_Vector2(ConfigRuntime.field(value, "Vector2Value"));
        v_Vector3Value = ConfigTypes.read_T_ZrConfig_Types_Vector3(ConfigRuntime.field(value, "Vector3Value"));
        v_KvValue = ConfigTypes.read_T_ZrConfig_Types_Kv(ConfigRuntime.field(value, "KvValue"));
        v_MoneyValue = ConfigTypes.read_T_Game_Config_Money(ConfigRuntime.field(value, "MoneyValue"));
        v_ByteArray = ConfigRuntime.list(ConfigRuntime.field(value, "ByteArray"), item0 -> (short)ConfigRuntime.integer(item0,0,255));
        v_ShortArray = ConfigRuntime.list(ConfigRuntime.field(value, "ShortArray"), item0 -> (short)ConfigRuntime.integer(item0,-32768,32767));
        v_IntArray = ConfigRuntime.list(ConfigRuntime.field(value, "IntArray"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE));
        v_LongArray = ConfigRuntime.list(ConfigRuntime.field(value, "LongArray"), item0 -> ConfigRuntime.int64(item0));
        v_FloatArray = ConfigRuntime.list(ConfigRuntime.field(value, "FloatArray"), item0 -> ConfigRuntime.float32(item0));
        v_DoubleArray = ConfigRuntime.list(ConfigRuntime.field(value, "DoubleArray"), item0 -> ConfigRuntime.float64(item0));
        v_BoolArray = ConfigRuntime.list(ConfigRuntime.field(value, "BoolArray"), item0 -> ConfigRuntime.bool(item0));
        v_StringArray = ConfigRuntime.list(ConfigRuntime.field(value, "StringArray"), item0 -> ConfigRuntime.string(item0));
        v_SByteArray = ConfigRuntime.list(ConfigRuntime.field(value, "SByteArray"), item0 -> (byte)ConfigRuntime.integer(item0,-128,127));
        v_UShortArray = ConfigRuntime.list(ConfigRuntime.field(value, "UShortArray"), item0 -> (int)ConfigRuntime.integer(item0,0,65535));
        v_UIntArray = ConfigRuntime.list(ConfigRuntime.field(value, "UIntArray"), item0 -> ConfigRuntime.integer(item0,0,0xffffffffL));
        v_ULongArray = ConfigRuntime.list(ConfigRuntime.field(value, "ULongArray"), item0 -> ConfigRuntime.uint64(item0));
        v_DecimalArray = ConfigRuntime.list(ConfigRuntime.field(value, "DecimalArray"), item0 -> new ConfigRuntime.Decimal(ConfigRuntime.string(item0)));
        v_QualityArray = ConfigRuntime.list(ConfigRuntime.field(value, "QualityArray"), item0 -> ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE)));
        v_Vector2Array = ConfigRuntime.list(ConfigRuntime.field(value, "Vector2Array"), item0 -> ConfigTypes.read_T_ZrConfig_Types_Vector2(item0));
        v_Vector3Array = ConfigRuntime.list(ConfigRuntime.field(value, "Vector3Array"), item0 -> ConfigTypes.read_T_ZrConfig_Types_Vector3(item0));
        v_KvArray = ConfigRuntime.list(ConfigRuntime.field(value, "KvArray"), item0 -> ConfigTypes.read_T_ZrConfig_Types_Kv(item0));
        v_MoneyArray = ConfigRuntime.list(ConfigRuntime.field(value, "MoneyArray"), item0 -> ConfigTypes.read_T_Game_Config_Money(item0));
        v_ByteArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "ByteArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (short)ConfigRuntime.integer(item1,0,255)));
        v_ShortArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "ShortArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (short)ConfigRuntime.integer(item1,-32768,32767)));
        v_IntArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "IntArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (int)ConfigRuntime.integer(item1,Integer.MIN_VALUE,Integer.MAX_VALUE)));
        v_LongArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "LongArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.int64(item1)));
        v_FloatArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "FloatArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.float32(item1)));
        v_DoubleArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "DoubleArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.float64(item1)));
        v_BoolArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "BoolArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.bool(item1)));
        v_StringArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "StringArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.string(item1)));
        v_SByteArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "SByteArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (byte)ConfigRuntime.integer(item1,-128,127)));
        v_UShortArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "UShortArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> (int)ConfigRuntime.integer(item1,0,65535)));
        v_UIntArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "UIntArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.integer(item1,0,0xffffffffL)));
        v_ULongArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "ULongArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.uint64(item1)));
        v_DecimalArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "DecimalArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> new ConfigRuntime.Decimal(ConfigRuntime.string(item1))));
        v_QualityArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "QualityArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(item1,Integer.MIN_VALUE,Integer.MAX_VALUE))));
        v_Vector2Array2D = ConfigRuntime.list(ConfigRuntime.field(value, "Vector2Array2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.read_T_ZrConfig_Types_Vector2(item1)));
        v_Vector3Array2D = ConfigRuntime.list(ConfigRuntime.field(value, "Vector3Array2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.read_T_ZrConfig_Types_Vector3(item1)));
        v_KvArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "KvArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.read_T_ZrConfig_Types_Kv(item1)));
        v_MoneyArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "MoneyArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigTypes.read_T_Game_Config_Money(item1)));
        v_ByteArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "ByteArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (short)ConfigRuntime.integer(item2,0,255))));
        v_ShortArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "ShortArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (short)ConfigRuntime.integer(item2,-32768,32767))));
        v_IntArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "IntArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (int)ConfigRuntime.integer(item2,Integer.MIN_VALUE,Integer.MAX_VALUE))));
        v_LongArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "LongArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.int64(item2))));
        v_FloatArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "FloatArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.float32(item2))));
        v_DoubleArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "DoubleArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.float64(item2))));
        v_BoolArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "BoolArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.bool(item2))));
        v_StringArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "StringArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.string(item2))));
        v_SByteArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "SByteArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (byte)ConfigRuntime.integer(item2,-128,127))));
        v_UShortArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "UShortArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> (int)ConfigRuntime.integer(item2,0,65535))));
        v_UIntArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "UIntArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.integer(item2,0,0xffffffffL))));
        v_ULongArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "ULongArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.uint64(item2))));
        v_DecimalArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "DecimalArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> new ConfigRuntime.Decimal(ConfigRuntime.string(item2)))));
        v_QualityArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "QualityArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(item2,Integer.MIN_VALUE,Integer.MAX_VALUE)))));
        v_Vector2Array3D = ConfigRuntime.list(ConfigRuntime.field(value, "Vector2Array3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.read_T_ZrConfig_Types_Vector2(item2))));
        v_Vector3Array3D = ConfigRuntime.list(ConfigRuntime.field(value, "Vector3Array3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.read_T_ZrConfig_Types_Vector3(item2))));
        v_KvArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "KvArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.read_T_ZrConfig_Types_Kv(item2))));
        v_MoneyArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "MoneyArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigTypes.read_T_Game_Config_Money(item2))));
        v_Settings = ConfigTypes.read_T_Game_Config_BattleSettings(ConfigRuntime.field(value, "Settings"));
        v_Range = ConfigTypes.read_T_Game_Config_ConfigRange(ConfigRuntime.field(value, "Range"));
        v_Options = ConfigTypes.read_T_Game_Config_SampleOptions(ConfigRuntime.field(value, "Options"));
        v_Action = ConfigTypes.read_T_Game_Config_BattleAction(ConfigRuntime.field(value, "Action"));
        v_Actions = ConfigRuntime.list(ConfigRuntime.field(value, "Actions"), item0 -> ConfigTypes.read_T_Game_Config_BattleAction(item0));
        v_Ranges = ConfigRuntime.list(ConfigRuntime.field(value, "Ranges"), item0 -> ConfigTypes.read_T_Game_Config_ConfigRange(item0));
        v_OptionsList = ConfigRuntime.list(ConfigRuntime.field(value, "OptionsList"), item0 -> ConfigTypes.read_T_Game_Config_SampleOptions(item0));
        v_Groups = ConfigRuntime.list(ConfigRuntime.field(value, "Groups"), item0 -> ConfigTypes.read_T_Game_Config_ActionGroup(item0));
        v_Phases = ConfigRuntime.list(ConfigRuntime.field(value, "Phases"), item0 -> ConfigTypes.read_T_Game_Config_BattlePhase(item0));
        v_OptionalRange = (ConfigRuntime.field(value, "OptionalRange") == null ? null : ConfigTypes.read_T_Game_Config_ConfigRange(ConfigRuntime.field(value, "OptionalRange")));
        v_OptionalInt = (ConfigRuntime.field(value, "OptionalInt") == null ? null : (int)ConfigRuntime.integer(ConfigRuntime.field(value, "OptionalInt"),Integer.MIN_VALUE,Integer.MAX_VALUE));
        v_OptionalUInt = (ConfigRuntime.field(value, "OptionalUInt") == null ? null : ConfigRuntime.integer(ConfigRuntime.field(value, "OptionalUInt"),0,0xffffffffL));
        v_OptionalULong = (ConfigRuntime.field(value, "OptionalULong") == null ? null : ConfigRuntime.uint64(ConfigRuntime.field(value, "OptionalULong")));
        v_OptionalDecimal = (ConfigRuntime.field(value, "OptionalDecimal") == null ? null : new ConfigRuntime.Decimal(ConfigRuntime.string(ConfigRuntime.field(value, "OptionalDecimal"))));
        v_OptionalBool = (ConfigRuntime.field(value, "OptionalBool") == null ? null : ConfigRuntime.bool(ConfigRuntime.field(value, "OptionalBool")));
        v_OptionalQuality = (ConfigRuntime.field(value, "OptionalQuality") == null ? null : ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(ConfigRuntime.field(value, "OptionalQuality"),Integer.MIN_VALUE,Integer.MAX_VALUE)));
        v_OptionalMoney = (ConfigRuntime.field(value, "OptionalMoney") == null ? null : ConfigTypes.read_T_Game_Config_Money(ConfigRuntime.field(value, "OptionalMoney")));
        v_OptionalValues = ConfigRuntime.list(ConfigRuntime.field(value, "OptionalValues"), item0 -> (item0 == null ? null : (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE)));
        v_StatMap = ConfigRuntime.dictionary(ConfigRuntime.field(value, "StatMap"), item0 -> ConfigRuntime.string(item0), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE));
        v_PriceMap = ConfigRuntime.dictionary(ConfigRuntime.field(value, "PriceMap"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE), item0 -> (item0 == null ? null : new ConfigRuntime.Decimal(ConfigRuntime.string(item0))));
        v_QualityMap = ConfigRuntime.dictionary(ConfigRuntime.field(value, "QualityMap"), item0 -> ConfigTypes.T_Game_Config_Quality.from((int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE)), item0 -> ConfigRuntime.integer(item0,0,0xffffffffL));
        v_ListMap = ConfigRuntime.dictionary(ConfigRuntime.field(value, "ListMap"), item0 -> ConfigRuntime.string(item0), item0 -> ConfigRuntime.list(item0, item1 -> (int)ConfigRuntime.integer(item1,Integer.MIN_VALUE,Integer.MAX_VALUE)));
        v_IntMatrix = ConfigRuntime.matrix(ConfigRuntime.field(value, "IntMatrix"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE));
        v_DecimalCube = ConfigRuntime.matrix(ConfigRuntime.field(value, "DecimalCube"), item0 -> new ConfigRuntime.Decimal(ConfigRuntime.string(item0)));
        v_OptionalMatrix = ConfigRuntime.matrix(ConfigRuntime.field(value, "OptionalMatrix"), item0 -> (item0 == null ? null : ConfigRuntime.integer(item0,0,0xffffffffL)));
        v_RawJsonValue = ConfigRuntime.string(ConfigRuntime.field(value, "RawJsonValue"));
        v_RawJsonArray = ConfigRuntime.list(ConfigRuntime.field(value, "RawJsonArray"), item0 -> ConfigRuntime.string(item0));
        v_RawJsonArray2D = ConfigRuntime.list(ConfigRuntime.field(value, "RawJsonArray2D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.string(item1)));
        v_RawJsonArray3D = ConfigRuntime.list(ConfigRuntime.field(value, "RawJsonArray3D"), item0 -> ConfigRuntime.list(item0, item1 -> ConfigRuntime.list(item1, item2 -> ConfigRuntime.string(item2))));
        v_JsonIntArray = ConfigRuntime.list(ConfigRuntime.field(value, "JsonIntArray"), item0 -> (int)ConfigRuntime.integer(item0,Integer.MIN_VALUE,Integer.MAX_VALUE));
        v_Hero = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Hero"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Skill = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Skill"),Integer.MIN_VALUE,Integer.MAX_VALUE);
        v_Previous = (int)ConfigRuntime.integer(ConfigRuntime.field(value, "Previous"),Integer.MIN_VALUE,Integer.MAX_VALUE);
    }
    static String schema() { return String.join("",
        "{\"name\":\"AllTypes10k\",\"hash\":\"34B94C4229D85CC8B37832A203504B7E6EF4447307ADB831AB5BD01628EA7432\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"唯一主键：1～10000\"},{\"name\":\"ByteValue\",\"type\":\"System.Byte\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortValue\",\"type\":\"System.Int16\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntValue\",\"type\":\"System.Int32\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongValue\",\"type\":\"System.Int64\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatValue\",\"type\":\"System.Single\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleValue\",\"type\":\"System.Double\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolValue\",\"type\":\"System.Boolean\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringValue\",\"type\":\"System.String\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteValue\",\"type\":\"System.SByte\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortValue\",\"type\":\"System.UInt16\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntValue\",\"type\":\"System.UInt32\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongValue\",\"type\":\"System.UInt64\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalValue\",\"type\":\"System.Decimal\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityValue\",\"type\":\"Game.Config.Quality\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Value\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Value\",\"type\":\"ZrConfig.Types.Vector3\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvValue\",\"type\":\"ZrConfig.Types.Kv\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyValue\",\"type\":\"Game.Config.Money\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ByteArray\",\"type\":\"System.Byte[]\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortArray\",\"type\":\"System.Int16[]\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntArray\",\"type\":\"System.Int32[]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongArray\",\"type\":\"System.Int64[]\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatArray\",\"type\":\"System.Single[]\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleArray\",\"type\":\"System.Double[]\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolArray\",\"type\":\"System.Boolean[]\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringArray\",\"type\":\"System.String[]\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteArray\",\"type\":\"System.SByte[]\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortArray\",\"type\":\"System.UInt16[]\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntArray\",\"type\":\"System.UInt32[]\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongArray\",\"type\":\"System.UInt64[]\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalArray\",\"type\":\"System.Decimal[]\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityArray\",\"type\":\"Game.Config.Quality[]\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Array\",\"type\":\"ZrConfig.Types.Vector2[]\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Array\",\"type\":\"ZrConfig.Types.Vector3[]\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvArray\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyArray\",\"type\":\"Game.Config.Money[]\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ByteArray2D\",\"type\":\"System.Byte[][]\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortArray2D\",\"type\":\"System.Int16[][]\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntArray2D\",\"type\":\"System.Int32[][]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongArray2D\",\"type\":\"System.Int64[][]\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatArray2D\",\"type\":\"System.Single[][]\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleArray2D\",\"type\":\"System.Double[][]\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolArray2D\",\"type\":\"System.Boolean[][]\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringArray2D\",\"type\":\"System.String[][]\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteArray2D\",\"type\":\"System.SByte[][]\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortArray2D\",\"type\":\"System.UInt16[][]\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntArray2D\",\"type\":\"System.UInt32[][]\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongArray2D\",\"type\":\"System.UInt64[][]\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalArray2D\",\"type\":\"System.Decimal[][]\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityArray2D\",\"type\":\"Game.Config.Quality[][]\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Array2D\",\"type\":\"ZrConfig.Types.Vector2[][]\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Array2D\",\"type\":\"ZrConfig.Types.Vector3[][]\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvArray2D\",\"type\":\"ZrConfig.Types.Kv[][]\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyArray2D\",\"type\":\"Game.Config.Money[][]\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ByteArray3D\",\"type\":\"System.Byte[][][]\",\"description\":\"Byte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ShortArray3D\",\"type\":\"System.Int16[][][]\",\"description\":\"Short；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntArray3D\",\"type\":\"System.Int32[][][]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"LongArray3D\",\"type\":\"System.Int64[][][]\",\"description\":\"Long；按 Id 轮换常规、边界、默认值\"},{\"name\":\"FloatArray3D\",\"type\":\"System.Single[][][]\",\"description\":\"Float；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DoubleArray3D\",\"type\":\"System.Double[][][]\",\"description\":\"Double；按 Id 轮换常规、边界、默认值\"},{\"name\":\"BoolArray3D\",\"type\":\"System.Boolean[][][]\",\"description\":\"Bool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StringArray3D\",\"type\":\"System.String[][][]\",\"description\":\"String；按 Id 轮换常规、边界、默认值\"},{\"name\":\"SByteArray3D\",\"type\":\"System.SByte[][][]\",\"description\":\"SByte；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UShortArray3D\",\"type\":\"System.UInt16[][][]\",\"description\":\"UShort；按 Id 轮换常规、边界、默认值\"},{\"name\":\"UIntArray3D\",\"type\":\"System.UInt32[][][]\",\"description\":\"UInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ULongArray3D\",\"type\":\"System.U",
        "Int64[][][]\",\"description\":\"ULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalArray3D\",\"type\":\"System.Decimal[][][]\",\"description\":\"Decimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityArray3D\",\"type\":\"Game.Config.Quality[][][]\",\"description\":\"Quality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector2Array3D\",\"type\":\"ZrConfig.Types.Vector2[][][]\",\"description\":\"Vector2；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Vector3Array3D\",\"type\":\"ZrConfig.Types.Vector3[][][]\",\"description\":\"Vector3；按 Id 轮换常规、边界、默认值\"},{\"name\":\"KvArray3D\",\"type\":\"ZrConfig.Types.Kv[][][]\",\"description\":\"Kv；按 Id 轮换常规、边界、默认值\"},{\"name\":\"MoneyArray3D\",\"type\":\"Game.Config.Money[][][]\",\"description\":\"Money；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Settings\",\"type\":\"Game.Config.BattleSettings\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Options\",\"type\":\"Game.Config.SampleOptions\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Action\",\"type\":\"Game.Config.BattleAction\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Ranges\",\"type\":\"Game.Config.ConfigRange[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"OptionsList\",\"type\":\"Game.Config.SampleOptions[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"Phases\",\"type\":\"Game.Config.BattlePhase[]\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"OptionalRange\",\"type\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"description\":\"完整对象或列表；单格 JSON 输入，导出为声明的强类型\"},{\"name\":\"OptionalInt\",\"type\":\"System.Nullable`1<System.Int32>\",\"description\":\"NullableInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalUInt\",\"type\":\"System.Nullable`1<System.UInt32>\",\"description\":\"NullableUInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalULong\",\"type\":\"System.Nullable`1<System.UInt64>\",\"description\":\"NullableULong；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalDecimal\",\"type\":\"System.Nullable`1<System.Decimal>\",\"description\":\"NullableDecimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalBool\",\"type\":\"System.Nullable`1<System.Boolean>\",\"description\":\"NullableBool；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalQuality\",\"type\":\"System.Nullable`1<Game.Config.Quality>\",\"description\":\"NullableQuality；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalMoney\",\"type\":\"System.Nullable`1<Game.Config.Money>\",\"description\":\"NullableMoney；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalValues\",\"type\":\"System.Nullable`1<System.Int32>[]\",\"description\":\"NullableIntArray；按 Id 轮换常规、边界、默认值\"},{\"name\":\"StatMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"description\":\"MapInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"PriceMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"description\":\"MapNullableDecimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"QualityMap\",\"type\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"description\":\"MapUInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"ListMap\",\"type\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"description\":\"MapIntArray；按 Id 轮换常规、边界、默认值\"},{\"name\":\"IntMatrix\",\"type\":\"System.Int32[,]\",\"description\":\"MatrixInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"DecimalCube\",\"type\":\"System.Decimal[,,]\",\"description\":\"MatrixDecimal；按 Id 轮换常规、边界、默认值\"},{\"name\":\"OptionalMatrix\",\"type\":\"System.Nullable`1<System.UInt32>[,]\",\"description\":\"MatrixNullableUInt；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonValue\",\"type\":\"ZrConfig.Types.RawJson\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonArray\",\"type\":\"ZrConfig.Types.RawJson[]\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonArray2D\",\"type\":\"ZrConfig.Types.RawJson[][]\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"RawJsonArray3D\",\"type\":\"ZrConfig.Types.RawJson[][][]\",\"description\":\"RawJson；按 Id 轮换常规、边界、默认值\"},{\"name\":\"JsonIntArray\",\"type\":\"System.Int32[]\",\"description\":\"Int；按 Id 轮换常规、边界、默认值\"},{\"name\":\"Hero\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"常规=1，边界=2，默认=0；引用 Hero 表\"},{\"name\":\"Skill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"常规=10，边界=11，默认=0；引用 Skill 表\"},{\"name\":\"Previous\",\"type\":\"ref:ZrConfig.Models.AllTypes10kConfig:Id:int:zero-null/1\",\"description\":\"Id=1 自引用，其余引用 Id-1；用于按需加载引用链\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"Game.Config.ActionGroup\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Actions\",\"type\":\"Game.Config.BattleAction[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ActionGroup[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ActionGroup\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.AddBuff\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"BuffId\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Stacks\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"enumValues\":[],\"abstract\":true,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleAction[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattleAction\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"b",
        "aseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Groups\",\"type\":\"Game.Config.ActionGroup[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattlePhase[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.BattlePhase\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.BattleSettings\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"MaxTargets\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Interruptible\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Min\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Max\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.ConfigRange[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Damage\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Delay\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Ratio\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"CanCrit\",\"type\":\"System.Boolean\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":\"Game.Config.BattleAction\",\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money\",\"kind\":\"converter\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Cents\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Money[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Money[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality\",\"kind\":\"enum\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[\"0\",\"1\",\"2\"],\"abstract\":false,\"baseType\":null,\"enumMembers\":{\"None\":\"0\",\"Rare\":\"1\",\"Legendary\":\"2\"},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.Quality[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.Quality[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions\",\"kind\":\"object\",\"primitive\":\"\",\"nullableObject\":true,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Range\",\"type\":\"Game.Config.ConfigRange\",\"description\":\"\"},{\"name\":\"Rewards\",\"type\":\"ZrConfig.Types.Kv[]\",\"description\":\"\"},{\"name\":\"Offset\",\"type\":\"ZrConfig.Types.Vector2\",\"description\":\"\"},{\"name\":\"Label\",\"type\":\"System.String\",\"description\":\"\"},{\"name\":\"Quality\",\"type\":\"Game.Config.Quality\",\"description\":\"\"},{\"name\":\"Levels\",\"type\":\"System.Int32[]\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Game.Config.SampleOptions[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"Game.Config.SampleOptions\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Boolean[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Boolean[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"Syst",
        "em.Byte\",\"kind\":\"primitive\",\"primitive\":\"uint8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Byte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Byte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<Game.Config.Quality,System.UInt32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":\"Game.Config.Quality\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.Int32,System.Nullable`1<System.Decimal>>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Nullable`1<System.Decimal>\",\"key\":\"System.Int32\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Collections.Generic.Dictionary`2<System.String,System.Int32[]>\",\"kind\":\"dictionary\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32[]\",\"key\":\"System.String\",\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal\",\"kind\":\"primitive\",\"primitive\":\"decimal\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[,,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":3,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Decimal[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Decimal[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double\",\"kind\":\"primitive\",\"primitive\":\"float64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Double[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Double[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16\",\"kind\":\"primitive\",\"primitive\":\"int16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":",
        "null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64\",\"kind\":\"primitive\",\"primitive\":\"int64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Int64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.ConfigRange>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.ConfigRange\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Money>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Money\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<Game.Config.Quality>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"Game.Config.Quality\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Boolean>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Boolean\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Decimal>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Decimal\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.Int32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.Int32>[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Nullable`1<System.Int32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt32>[,]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":2,\"element\":\"System.Nullable`1<System.UInt32>\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Nullable`1<System.UInt64>\",\"kind\":\"nullable\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte\",\"kind\":\"primitive\",\"primitive\":\"int8\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.SByte[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.SByte[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single\",\"kind\":\"primitive\",\"primitive\":\"float32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Single[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.Single[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"ba",
        "seType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.String[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16\",\"kind\":\"primitive\",\"primitive\":\"uint16\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt16[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt16[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32\",\"kind\":\"primitive\",\"primitive\":\"uint32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt32[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt32[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64\",\"kind\":\"primitive\",\"primitive\":\"uint64\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.UInt64[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"System.UInt64[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv\",\"kind\":\"kv\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"Key\",\"type\":\"System.Int32\",\"description\":\"\"},{\"name\":\"Value\",\"type\":\"System.Int32\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Kv[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Kv[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson\",\"kind\":\"rawjson\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.RawJson[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.RawJson[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"",
        "id\":\"ZrConfig.Types.Vector2\",\"kind\":\"vector2\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector2[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector2[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3\",\"kind\":\"vector3\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[{\"name\":\"X\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Y\",\"type\":\"System.Single\",\"description\":\"\"},{\"name\":\"Z\",\"type\":\"System.Single\",\"description\":\"\"}],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ZrConfig.Types.Vector3[][][]\",\"kind\":\"array\",\"primitive\":\"\",\"nullableObject\":false,\"rank\":1,\"element\":\"ZrConfig.Types.Vector3[][]\",\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.AllTypes10kConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"AllTypes10k\"},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"},{\"id\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Skill\"}],\"tags\":[\"Game.Config.AddBuff\",\"Game.Config.Damage\"],\"dependencies\":[\"AllTypes10k\",\"Hero\",\"Skill\"]}"); }
    private static final class Index_Key extends ConfigRuntime.Index<AllTypes10kConfig> {
        Index_Key(List<AllTypes10kConfig> rows) { super(rows, true); }
        @Override protected int hash(AllTypes10kConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(AllTypes10kConfig a, AllTypes10kConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { AllTypes10kConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        AllTypes10kConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<AllTypes10kConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    private static final class Index_Group_ById extends ConfigRuntime.Index<AllTypes10kConfig> {
        Index_Group_ById(List<AllTypes10kConfig> rows) { super(rows, false); }
        @Override protected int hash(AllTypes10kConfig row) { return (1 * 31 + (int)row.v_Id); }
        @Override protected boolean same(AllTypes10kConfig a, AllTypes10kConfig b) { return a.v_Id == b.v_Id; }
        int find(int k0) {
            int slot = mix((1 * 31 + (int)k0)) & mask;
            while (slots[slot] >= 0) { AllTypes10kConfig row = rows.get(slots[slot]); if (row.v_Id == k0) return slot; slot = (slot + 1) & mask; }
            return -1;
        }
        AllTypes10kConfig row(int slot) { return slot < 0 ? null : rows.get(slots[slot]); }
        List<AllTypes10kConfig> group(int slot) { return slot < 0 ? Collections.emptyList() : groups[slot]; }
    }
    public static final class Table {
        private final List<AllTypes10kConfig> rows;
        private final Index_Key index_Key;
        private final Index_Group_ById index_Group_ById;
        private final Map<Integer,AllTypes10kConfig> ids = new HashMap<>();
        Table(List<Object> source) {
            var values = new ArrayList<AllTypes10kConfig>(source.size());
            for (Object value : source) values.add(new AllTypes10kConfig(value));
            rows = Collections.unmodifiableList(values);
            index_Key = new Index_Key(rows);
            index_Group_ById = new Index_Group_ById(rows);
            for (var row : rows) { if (ids.put(row.v_Id, row) != null) throw ConfigRuntime.invalid("引用 Id 不唯一"); }
        }
        void bind(ConfigSnapshot snapshot) {
            for (var row : rows) if (row.v_Hero != 0) { row.ref_Hero = snapshot.Hero.byId(row.v_Hero); if (row.ref_Hero == null) throw ConfigRuntime.invalid("引用目标不存在：AllTypes10k.Hero"); }
            for (var row : rows) if (row.v_Skill != 0) { row.ref_Skill = snapshot.Skill.byId(row.v_Skill); if (row.ref_Skill == null) throw ConfigRuntime.invalid("引用目标不存在：AllTypes10k.Skill"); }
            for (var row : rows) if (row.v_Previous != 0) { row.ref_Previous = snapshot.AllTypes10k.byId(row.v_Previous); if (row.ref_Previous == null) throw ConfigRuntime.invalid("引用目标不存在：AllTypes10k.Previous"); }
        }
        void retain(ConfigSnapshot snapshot) { for (var row : rows) row.serverOwner = snapshot; }
        AllTypes10kConfig byId(int id) { return ids.get(id); }
        public List<AllTypes10kConfig> rows() { return rows; }
        public int count() { return rows.size(); }
        public AllTypes10kConfig find(int k0) { return index_Key.row(index_Key.find(k0)); }
        public AllTypes10kConfig get(int k0) { AllTypes10kConfig value = find(k0); if (value == null) throw new NoSuchElementException("配置主键不存在"); return value; }
        public List<AllTypes10kConfig> getById(int k0) { return index_Group_ById.group(index_Group_ById.find(k0)); }
    }
}
