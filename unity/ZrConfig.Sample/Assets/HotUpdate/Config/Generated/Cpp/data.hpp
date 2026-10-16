// zrconfig-api-contract:1
// Generated server data family: owning handles, explicit row and group loads.
#pragma once
#include "server.hpp"
namespace zrconfig { namespace data {
class Snapshot;
class AllTypesRow;
class AllTypes10kRow;
class DropRow;
class HeroRow;
class ItemCatalogRow;
class QuestRewardsRow;
class ShopOffersRow;
class SkillRow;
class SkillLevelRow;
class AllTypesData {
    std::int32_t v_Id;
    std::uint8_t v_ByteValue;
    std::int16_t v_ShortValue;
    std::int32_t v_IntValue;
    std::int64_t v_LongValue;
    float v_FloatValue;
    double v_DoubleValue;
    bool v_BoolValue;
    std::string v_StringValue;
    std::int8_t v_SByteValue;
    std::uint16_t v_UShortValue;
    std::uint32_t v_UIntValue;
    std::uint64_t v_ULongValue;
    Decimal v_DecimalValue;
    T_Game_Config_Quality v_QualityValue;
    std::shared_ptr<const T_ZrConfig_Types_Vector2> v_Vector2Value;
    std::shared_ptr<const T_ZrConfig_Types_Vector3> v_Vector3Value;
    std::shared_ptr<const T_ZrConfig_Types_Kv> v_KvValue;
    std::shared_ptr<const T_Game_Config_Money> v_MoneyValue;
    std::vector<std::uint8_t> v_ByteArray;
    std::vector<std::int16_t> v_ShortArray;
    std::vector<std::int32_t> v_IntArray;
    std::vector<std::int64_t> v_LongArray;
    std::vector<float> v_FloatArray;
    std::vector<double> v_DoubleArray;
    std::vector<bool> v_BoolArray;
    std::vector<std::string> v_StringArray;
    std::vector<std::int8_t> v_SByteArray;
    std::vector<std::uint16_t> v_UShortArray;
    std::vector<std::uint32_t> v_UIntArray;
    std::vector<std::uint64_t> v_ULongArray;
    std::vector<Decimal> v_DecimalArray;
    std::vector<T_Game_Config_Quality> v_QualityArray;
    std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>> v_Vector2Array;
    std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>> v_Vector3Array;
    std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>> v_KvArray;
    std::vector<std::shared_ptr<const T_Game_Config_Money>> v_MoneyArray;
    std::vector<std::vector<std::uint8_t>> v_ByteArray2D;
    std::vector<std::vector<std::int16_t>> v_ShortArray2D;
    std::vector<std::vector<std::int32_t>> v_IntArray2D;
    std::vector<std::vector<std::int64_t>> v_LongArray2D;
    std::vector<std::vector<float>> v_FloatArray2D;
    std::vector<std::vector<double>> v_DoubleArray2D;
    std::vector<std::vector<bool>> v_BoolArray2D;
    std::vector<std::vector<std::string>> v_StringArray2D;
    std::vector<std::vector<std::int8_t>> v_SByteArray2D;
    std::vector<std::vector<std::uint16_t>> v_UShortArray2D;
    std::vector<std::vector<std::uint32_t>> v_UIntArray2D;
    std::vector<std::vector<std::uint64_t>> v_ULongArray2D;
    std::vector<std::vector<Decimal>> v_DecimalArray2D;
    std::vector<std::vector<T_Game_Config_Quality>> v_QualityArray2D;
    std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>> v_Vector2Array2D;
    std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>> v_Vector3Array2D;
    std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>> v_KvArray2D;
    std::vector<std::vector<std::shared_ptr<const T_Game_Config_Money>>> v_MoneyArray2D;
    std::vector<std::vector<std::vector<std::uint8_t>>> v_ByteArray3D;
    std::vector<std::vector<std::vector<std::int16_t>>> v_ShortArray3D;
    std::vector<std::vector<std::vector<std::int32_t>>> v_IntArray3D;
    std::vector<std::vector<std::vector<std::int64_t>>> v_LongArray3D;
    std::vector<std::vector<std::vector<float>>> v_FloatArray3D;
    std::vector<std::vector<std::vector<double>>> v_DoubleArray3D;
    std::vector<std::vector<std::vector<bool>>> v_BoolArray3D;
    std::vector<std::vector<std::vector<std::string>>> v_StringArray3D;
    std::vector<std::vector<std::vector<std::int8_t>>> v_SByteArray3D;
    std::vector<std::vector<std::vector<std::uint16_t>>> v_UShortArray3D;
    std::vector<std::vector<std::vector<std::uint32_t>>> v_UIntArray3D;
    std::vector<std::vector<std::vector<std::uint64_t>>> v_ULongArray3D;
    std::vector<std::vector<std::vector<Decimal>>> v_DecimalArray3D;
    std::vector<std::vector<std::vector<T_Game_Config_Quality>>> v_QualityArray3D;
    std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>>> v_Vector2Array3D;
    std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>>> v_Vector3Array3D;
    std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>>> v_KvArray3D;
    std::vector<std::vector<std::vector<std::shared_ptr<const T_Game_Config_Money>>>> v_MoneyArray3D;
    std::shared_ptr<const T_Game_Config_BattleSettings> v_Settings;
    std::shared_ptr<const T_Game_Config_ConfigRange> v_Range;
    std::shared_ptr<const T_Game_Config_SampleOptions> v_Options;
    std::shared_ptr<const T_Game_Config_BattleAction> v_Action;
    std::vector<std::shared_ptr<const T_Game_Config_BattleAction>> v_Actions;
    std::vector<std::shared_ptr<const T_Game_Config_ConfigRange>> v_Ranges;
    std::vector<std::shared_ptr<const T_Game_Config_SampleOptions>> v_OptionsList;
    std::vector<std::shared_ptr<const T_Game_Config_ActionGroup>> v_Groups;
    std::vector<std::shared_ptr<const T_Game_Config_BattlePhase>> v_Phases;
    std::optional<std::shared_ptr<const T_Game_Config_ConfigRange>> v_OptionalRange;
    std::optional<std::int32_t> v_OptionalInt;
    std::optional<std::uint32_t> v_OptionalUInt;
    std::optional<std::uint64_t> v_OptionalULong;
    std::optional<Decimal> v_OptionalDecimal;
    std::optional<bool> v_OptionalBool;
    std::optional<T_Game_Config_Quality> v_OptionalQuality;
    std::optional<std::shared_ptr<const T_Game_Config_Money>> v_OptionalMoney;
    std::vector<std::optional<std::int32_t>> v_OptionalValues;
    std::unordered_map<std::string,std::int32_t,Hash<std::string>> v_StatMap;
    std::unordered_map<std::int32_t,std::optional<Decimal>,Hash<std::int32_t>> v_PriceMap;
    std::unordered_map<T_Game_Config_Quality,std::uint32_t,Hash<T_Game_Config_Quality>> v_QualityMap;
    std::unordered_map<std::string,std::vector<std::int32_t>,Hash<std::string>> v_ListMap;
    Matrix<std::int32_t> v_IntMatrix;
    Matrix<Decimal> v_DecimalCube;
    Matrix<std::optional<std::uint32_t>> v_OptionalMatrix;
    std::string v_RawJsonValue;
    std::vector<std::string> v_RawJsonArray;
    std::vector<std::vector<std::string>> v_RawJsonArray2D;
    std::vector<std::vector<std::vector<std::string>>> v_RawJsonArray3D;
    std::vector<std::int32_t> v_JsonIntArray;
public:
    explicit AllTypesData(const Json& value):v_Id(integer<std::int32_t>(value.field("Id"))),v_ByteValue(integer<std::uint8_t>(value.field("ByteValue"))),v_ShortValue(integer<std::int16_t>(value.field("ShortValue"))),v_IntValue(integer<std::int32_t>(value.field("IntValue"))),v_LongValue(integer<std::int64_t>(value.field("LongValue"))),v_FloatValue(floating<float>(value.field("FloatValue"))),v_DoubleValue(floating<double>(value.field("DoubleValue"))),v_BoolValue(value.field("BoolValue").boolean()),v_StringValue(value.field("StringValue").string()),v_SByteValue(integer<std::int8_t>(value.field("SByteValue"))),v_UShortValue(integer<std::uint16_t>(value.field("UShortValue"))),v_UIntValue(integer<std::uint32_t>(value.field("UIntValue"))),v_ULongValue(integer<std::uint64_t>(value.field("ULongValue"))),v_DecimalValue(Decimal(value.field("DecimalValue").string())),v_QualityValue(static_cast<T_Game_Config_Quality>(integer<std::int32_t>(value.field("QualityValue")))),v_Vector2Value(read_T_ZrConfig_Types_Vector2(value.field("Vector2Value"))),v_Vector3Value(read_T_ZrConfig_Types_Vector3(value.field("Vector3Value"))),v_KvValue(read_T_ZrConfig_Types_Kv(value.field("KvValue"))),v_MoneyValue(read_T_Game_Config_Money(value.field("MoneyValue"))),v_ByteArray(decode_list(value.field("ByteArray"),[](const Json& item0) { return integer<std::uint8_t>(item0); })),v_ShortArray(decode_list(value.field("ShortArray"),[](const Json& item0) { return integer<std::int16_t>(item0); })),v_IntArray(decode_list(value.field("IntArray"),[](const Json& item0) { return integer<std::int32_t>(item0); })),v_LongArray(decode_list(value.field("LongArray"),[](const Json& item0) { return integer<std::int64_t>(item0); })),v_FloatArray(decode_list(value.field("FloatArray"),[](const Json& item0) { return floating<float>(item0); })),v_DoubleArray(decode_list(value.field("DoubleArray"),[](const Json& item0) { return floating<double>(item0); })),v_BoolArray(decode_list(value.field("BoolArray"),[](const Json& item0) { return item0.boolean(); })),v_StringArray(decode_list(value.field("StringArray"),[](const Json& item0) { return item0.string(); })),v_SByteArray(decode_list(value.field("SByteArray"),[](const Json& item0) { return integer<std::int8_t>(item0); })),v_UShortArray(decode_list(value.field("UShortArray"),[](const Json& item0) { return integer<std::uint16_t>(item0); })),v_UIntArray(decode_list(value.field("UIntArray"),[](const Json& item0) { return integer<std::uint32_t>(item0); })),v_ULongArray(decode_list(value.field("ULongArray"),[](const Json& item0) { return integer<std::uint64_t>(item0); })),v_DecimalArray(decode_list(value.field("DecimalArray"),[](const Json& item0) { return Decimal(item0.string()); })),v_QualityArray(decode_list(value.field("QualityArray"),[](const Json& item0) { return static_cast<T_Game_Config_Quality>(integer<std::int32_t>(item0)); })),v_Vector2Array(decode_list(value.field("Vector2Array"),[](const Json& item0) { return read_T_ZrConfig_Types_Vector2(item0); })),v_Vector3Array(decode_list(value.field("Vector3Array"),[](const Json& item0) { return read_T_ZrConfig_Types_Vector3(item0); })),v_KvArray(decode_list(value.field("KvArray"),[](const Json& item0) { return read_T_ZrConfig_Types_Kv(item0); })),v_MoneyArray(decode_list(value.field("MoneyArray"),[](const Json& item0) { return read_T_Game_Config_Money(item0); })),v_ByteArray2D(decode_list(value.field("ByteArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::uint8_t>(item1); }); })),v_ShortArray2D(decode_list(value.field("ShortArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::int16_t>(item1); }); })),v_IntArray2D(decode_list(value.field("IntArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::int32_t>(item1); }); })),v_LongArray2D(decode_list(value.field("LongArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::int64_t>(item1); }); })),v_FloatArray2D(decode_list(value.field("FloatArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return floating<float>(item1); }); })),v_DoubleArray2D(decode_list(value.field("DoubleArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return floating<double>(item1); }); })),v_BoolArray2D(decode_list(value.field("BoolArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return item1.boolean(); }); })),v_StringArray2D(decode_list(value.field("StringArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return item1.string(); }); })),v_SByteArray2D(decode_list(value.field("SByteArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::int8_t>(item1); }); })),v_UShortArray2D(decode_list(value.field("UShortArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::uint16_t>(item1); }); })),v_UIntArray2D(decode_list(value.field("UIntArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::uint32_t>(item1); }); })),v_ULongArray2D(decode_list(value.field("ULongArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::uint64_t>(item1); }); })),v_DecimalArray2D(decode_list(value.field("DecimalArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return Decimal(item1.string()); }); })),v_QualityArray2D(decode_list(value.field("QualityArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return static_cast<T_Game_Config_Quality>(integer<std::int32_t>(item1)); }); })),v_Vector2Array2D(decode_list(value.field("Vector2Array2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return read_T_ZrConfig_Types_Vector2(item1); }); })),v_Vector3Array2D(decode_list(value.field("Vector3Array2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return read_T_ZrConfig_Types_Vector3(item1); }); })),v_KvArray2D(decode_list(value.field("KvArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return read_T_ZrConfig_Types_Kv(item1); }); })),v_MoneyArray2D(decode_list(value.field("MoneyArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return read_T_Game_Config_Money(item1); }); })),v_ByteArray3D(decode_list(value.field("ByteArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::uint8_t>(item2); }); }); })),v_ShortArray3D(decode_list(value.field("ShortArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::int16_t>(item2); }); }); })),v_IntArray3D(decode_list(value.field("IntArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::int32_t>(item2); }); }); })),v_LongArray3D(decode_list(value.field("LongArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::int64_t>(item2); }); }); })),v_FloatArray3D(decode_list(value.field("FloatArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return floating<float>(item2); }); }); })),v_DoubleArray3D(decode_list(value.field("DoubleArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return floating<double>(item2); }); }); })),v_BoolArray3D(decode_list(value.field("BoolArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return item2.boolean(); }); }); })),v_StringArray3D(decode_list(value.field("StringArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return item2.string(); }); }); })),v_SByteArray3D(decode_list(value.field("SByteArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::int8_t>(item2); }); }); })),v_UShortArray3D(decode_list(value.field("UShortArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::uint16_t>(item2); }); }); })),v_UIntArray3D(decode_list(value.field("UIntArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::uint32_t>(item2); }); }); })),v_ULongArray3D(decode_list(value.field("ULongArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::uint64_t>(item2); }); }); })),v_DecimalArray3D(decode_list(value.field("DecimalArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return Decimal(item2.string()); }); }); })),v_QualityArray3D(decode_list(value.field("QualityArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return static_cast<T_Game_Config_Quality>(integer<std::int32_t>(item2)); }); }); })),v_Vector2Array3D(decode_list(value.field("Vector2Array3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return read_T_ZrConfig_Types_Vector2(item2); }); }); })),v_Vector3Array3D(decode_list(value.field("Vector3Array3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return read_T_ZrConfig_Types_Vector3(item2); }); }); })),v_KvArray3D(decode_list(value.field("KvArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return read_T_ZrConfig_Types_Kv(item2); }); }); })),v_MoneyArray3D(decode_list(value.field("MoneyArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return read_T_Game_Config_Money(item2); }); }); })),v_Settings(read_T_Game_Config_BattleSettings(value.field("Settings"))),v_Range(read_T_Game_Config_ConfigRange(value.field("Range"))),v_Options(read_T_Game_Config_SampleOptions(value.field("Options"))),v_Action(read_T_Game_Config_BattleAction(value.field("Action"))),v_Actions(decode_list(value.field("Actions"),[](const Json& item0) { return read_T_Game_Config_BattleAction(item0); })),v_Ranges(decode_list(value.field("Ranges"),[](const Json& item0) { return read_T_Game_Config_ConfigRange(item0); })),v_OptionsList(decode_list(value.field("OptionsList"),[](const Json& item0) { return read_T_Game_Config_SampleOptions(item0); })),v_Groups(decode_list(value.field("Groups"),[](const Json& item0) { return read_T_Game_Config_ActionGroup(item0); })),v_Phases(decode_list(value.field("Phases"),[](const Json& item0) { return read_T_Game_Config_BattlePhase(item0); })),v_OptionalRange(decode_optional(value.field("OptionalRange"),[](const Json& item0) { return read_T_Game_Config_ConfigRange(item0); })),v_OptionalInt(decode_optional(value.field("OptionalInt"),[](const Json& item0) { return integer<std::int32_t>(item0); })),v_OptionalUInt(decode_optional(value.field("OptionalUInt"),[](const Json& item0) { return integer<std::uint32_t>(item0); })),v_OptionalULong(decode_optional(value.field("OptionalULong"),[](const Json& item0) { return integer<std::uint64_t>(item0); })),v_OptionalDecimal(decode_optional(value.field("OptionalDecimal"),[](const Json& item0) { return Decimal(item0.string()); })),v_OptionalBool(decode_optional(value.field("OptionalBool"),[](const Json& item0) { return item0.boolean(); })),v_OptionalQuality(decode_optional(value.field("OptionalQuality"),[](const Json& item0) { return static_cast<T_Game_Config_Quality>(integer<std::int32_t>(item0)); })),v_OptionalMoney(decode_optional(value.field("OptionalMoney"),[](const Json& item0) { return read_T_Game_Config_Money(item0); })),v_OptionalValues(decode_list(value.field("OptionalValues"),[](const Json& item0) { return decode_optional(item0,[](const Json& item1) { return integer<std::int32_t>(item1); }); })),v_StatMap(decode_dictionary(value.field("StatMap"),[](const Json& item0) { return item0.string(); },[](const Json& item0) { return integer<std::int32_t>(item0); })),v_PriceMap(decode_dictionary(value.field("PriceMap"),[](const Json& item0) { return integer<std::int32_t>(item0); },[](const Json& item0) { return decode_optional(item0,[](const Json& item1) { return Decimal(item1.string()); }); })),v_QualityMap(decode_dictionary(value.field("QualityMap"),[](const Json& item0) { return static_cast<T_Game_Config_Quality>(integer<std::int32_t>(item0)); },[](const Json& item0) { return integer<std::uint32_t>(item0); })),v_ListMap(decode_dictionary(value.field("ListMap"),[](const Json& item0) { return item0.string(); },[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::int32_t>(item1); }); })),v_IntMatrix(decode_matrix(value.field("IntMatrix"),[](const Json& item0) { return integer<std::int32_t>(item0); })),v_DecimalCube(decode_matrix(value.field("DecimalCube"),[](const Json& item0) { return Decimal(item0.string()); })),v_OptionalMatrix(decode_matrix(value.field("OptionalMatrix"),[](const Json& item0) { return decode_optional(item0,[](const Json& item1) { return integer<std::uint32_t>(item1); }); })),v_RawJsonValue(value.field("RawJsonValue").string()),v_RawJsonArray(decode_list(value.field("RawJsonArray"),[](const Json& item0) { return item0.string(); })),v_RawJsonArray2D(decode_list(value.field("RawJsonArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return item1.string(); }); })),v_RawJsonArray3D(decode_list(value.field("RawJsonArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return item2.string(); }); }); })),v_JsonIntArray(decode_list(value.field("JsonIntArray"),[](const Json& item0) { return integer<std::int32_t>(item0); })){}
    const std::int32_t& Id()const noexcept{return v_Id;}
    const std::uint8_t& ByteValue()const noexcept{return v_ByteValue;}
    const std::int16_t& ShortValue()const noexcept{return v_ShortValue;}
    const std::int32_t& IntValue()const noexcept{return v_IntValue;}
    const std::int64_t& LongValue()const noexcept{return v_LongValue;}
    const float& FloatValue()const noexcept{return v_FloatValue;}
    const double& DoubleValue()const noexcept{return v_DoubleValue;}
    const bool& BoolValue()const noexcept{return v_BoolValue;}
    const std::string& StringValue()const noexcept{return v_StringValue;}
    const std::int8_t& SByteValue()const noexcept{return v_SByteValue;}
    const std::uint16_t& UShortValue()const noexcept{return v_UShortValue;}
    const std::uint32_t& UIntValue()const noexcept{return v_UIntValue;}
    const std::uint64_t& ULongValue()const noexcept{return v_ULongValue;}
    const Decimal& DecimalValue()const noexcept{return v_DecimalValue;}
    const T_Game_Config_Quality& QualityValue()const noexcept{return v_QualityValue;}
    const std::shared_ptr<const T_ZrConfig_Types_Vector2>& Vector2Value()const noexcept{return v_Vector2Value;}
    const std::shared_ptr<const T_ZrConfig_Types_Vector3>& Vector3Value()const noexcept{return v_Vector3Value;}
    const std::shared_ptr<const T_ZrConfig_Types_Kv>& KvValue()const noexcept{return v_KvValue;}
    const std::shared_ptr<const T_Game_Config_Money>& MoneyValue()const noexcept{return v_MoneyValue;}
    const std::vector<std::uint8_t>& ByteArray()const noexcept{return v_ByteArray;}
    const std::vector<std::int16_t>& ShortArray()const noexcept{return v_ShortArray;}
    const std::vector<std::int32_t>& IntArray()const noexcept{return v_IntArray;}
    const std::vector<std::int64_t>& LongArray()const noexcept{return v_LongArray;}
    const std::vector<float>& FloatArray()const noexcept{return v_FloatArray;}
    const std::vector<double>& DoubleArray()const noexcept{return v_DoubleArray;}
    const std::vector<bool>& BoolArray()const noexcept{return v_BoolArray;}
    const std::vector<std::string>& StringArray()const noexcept{return v_StringArray;}
    const std::vector<std::int8_t>& SByteArray()const noexcept{return v_SByteArray;}
    const std::vector<std::uint16_t>& UShortArray()const noexcept{return v_UShortArray;}
    const std::vector<std::uint32_t>& UIntArray()const noexcept{return v_UIntArray;}
    const std::vector<std::uint64_t>& ULongArray()const noexcept{return v_ULongArray;}
    const std::vector<Decimal>& DecimalArray()const noexcept{return v_DecimalArray;}
    const std::vector<T_Game_Config_Quality>& QualityArray()const noexcept{return v_QualityArray;}
    const std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>& Vector2Array()const noexcept{return v_Vector2Array;}
    const std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>& Vector3Array()const noexcept{return v_Vector3Array;}
    const std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>& KvArray()const noexcept{return v_KvArray;}
    const std::vector<std::shared_ptr<const T_Game_Config_Money>>& MoneyArray()const noexcept{return v_MoneyArray;}
    const std::vector<std::vector<std::uint8_t>>& ByteArray2D()const noexcept{return v_ByteArray2D;}
    const std::vector<std::vector<std::int16_t>>& ShortArray2D()const noexcept{return v_ShortArray2D;}
    const std::vector<std::vector<std::int32_t>>& IntArray2D()const noexcept{return v_IntArray2D;}
    const std::vector<std::vector<std::int64_t>>& LongArray2D()const noexcept{return v_LongArray2D;}
    const std::vector<std::vector<float>>& FloatArray2D()const noexcept{return v_FloatArray2D;}
    const std::vector<std::vector<double>>& DoubleArray2D()const noexcept{return v_DoubleArray2D;}
    const std::vector<std::vector<bool>>& BoolArray2D()const noexcept{return v_BoolArray2D;}
    const std::vector<std::vector<std::string>>& StringArray2D()const noexcept{return v_StringArray2D;}
    const std::vector<std::vector<std::int8_t>>& SByteArray2D()const noexcept{return v_SByteArray2D;}
    const std::vector<std::vector<std::uint16_t>>& UShortArray2D()const noexcept{return v_UShortArray2D;}
    const std::vector<std::vector<std::uint32_t>>& UIntArray2D()const noexcept{return v_UIntArray2D;}
    const std::vector<std::vector<std::uint64_t>>& ULongArray2D()const noexcept{return v_ULongArray2D;}
    const std::vector<std::vector<Decimal>>& DecimalArray2D()const noexcept{return v_DecimalArray2D;}
    const std::vector<std::vector<T_Game_Config_Quality>>& QualityArray2D()const noexcept{return v_QualityArray2D;}
    const std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>>& Vector2Array2D()const noexcept{return v_Vector2Array2D;}
    const std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>>& Vector3Array2D()const noexcept{return v_Vector3Array2D;}
    const std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>>& KvArray2D()const noexcept{return v_KvArray2D;}
    const std::vector<std::vector<std::shared_ptr<const T_Game_Config_Money>>>& MoneyArray2D()const noexcept{return v_MoneyArray2D;}
    const std::vector<std::vector<std::vector<std::uint8_t>>>& ByteArray3D()const noexcept{return v_ByteArray3D;}
    const std::vector<std::vector<std::vector<std::int16_t>>>& ShortArray3D()const noexcept{return v_ShortArray3D;}
    const std::vector<std::vector<std::vector<std::int32_t>>>& IntArray3D()const noexcept{return v_IntArray3D;}
    const std::vector<std::vector<std::vector<std::int64_t>>>& LongArray3D()const noexcept{return v_LongArray3D;}
    const std::vector<std::vector<std::vector<float>>>& FloatArray3D()const noexcept{return v_FloatArray3D;}
    const std::vector<std::vector<std::vector<double>>>& DoubleArray3D()const noexcept{return v_DoubleArray3D;}
    const std::vector<std::vector<std::vector<bool>>>& BoolArray3D()const noexcept{return v_BoolArray3D;}
    const std::vector<std::vector<std::vector<std::string>>>& StringArray3D()const noexcept{return v_StringArray3D;}
    const std::vector<std::vector<std::vector<std::int8_t>>>& SByteArray3D()const noexcept{return v_SByteArray3D;}
    const std::vector<std::vector<std::vector<std::uint16_t>>>& UShortArray3D()const noexcept{return v_UShortArray3D;}
    const std::vector<std::vector<std::vector<std::uint32_t>>>& UIntArray3D()const noexcept{return v_UIntArray3D;}
    const std::vector<std::vector<std::vector<std::uint64_t>>>& ULongArray3D()const noexcept{return v_ULongArray3D;}
    const std::vector<std::vector<std::vector<Decimal>>>& DecimalArray3D()const noexcept{return v_DecimalArray3D;}
    const std::vector<std::vector<std::vector<T_Game_Config_Quality>>>& QualityArray3D()const noexcept{return v_QualityArray3D;}
    const std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>>>& Vector2Array3D()const noexcept{return v_Vector2Array3D;}
    const std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>>>& Vector3Array3D()const noexcept{return v_Vector3Array3D;}
    const std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>>>& KvArray3D()const noexcept{return v_KvArray3D;}
    const std::vector<std::vector<std::vector<std::shared_ptr<const T_Game_Config_Money>>>>& MoneyArray3D()const noexcept{return v_MoneyArray3D;}
    const std::shared_ptr<const T_Game_Config_BattleSettings>& Settings()const noexcept{return v_Settings;}
    const std::shared_ptr<const T_Game_Config_ConfigRange>& Range()const noexcept{return v_Range;}
    const std::shared_ptr<const T_Game_Config_SampleOptions>& Options()const noexcept{return v_Options;}
    const std::shared_ptr<const T_Game_Config_BattleAction>& Action()const noexcept{return v_Action;}
    const std::vector<std::shared_ptr<const T_Game_Config_BattleAction>>& Actions()const noexcept{return v_Actions;}
    const std::vector<std::shared_ptr<const T_Game_Config_ConfigRange>>& Ranges()const noexcept{return v_Ranges;}
    const std::vector<std::shared_ptr<const T_Game_Config_SampleOptions>>& OptionsList()const noexcept{return v_OptionsList;}
    const std::vector<std::shared_ptr<const T_Game_Config_ActionGroup>>& Groups()const noexcept{return v_Groups;}
    const std::vector<std::shared_ptr<const T_Game_Config_BattlePhase>>& Phases()const noexcept{return v_Phases;}
    const std::optional<std::shared_ptr<const T_Game_Config_ConfigRange>>& OptionalRange()const noexcept{return v_OptionalRange;}
    const std::optional<std::int32_t>& OptionalInt()const noexcept{return v_OptionalInt;}
    const std::optional<std::uint32_t>& OptionalUInt()const noexcept{return v_OptionalUInt;}
    const std::optional<std::uint64_t>& OptionalULong()const noexcept{return v_OptionalULong;}
    const std::optional<Decimal>& OptionalDecimal()const noexcept{return v_OptionalDecimal;}
    const std::optional<bool>& OptionalBool()const noexcept{return v_OptionalBool;}
    const std::optional<T_Game_Config_Quality>& OptionalQuality()const noexcept{return v_OptionalQuality;}
    const std::optional<std::shared_ptr<const T_Game_Config_Money>>& OptionalMoney()const noexcept{return v_OptionalMoney;}
    const std::vector<std::optional<std::int32_t>>& OptionalValues()const noexcept{return v_OptionalValues;}
    const std::unordered_map<std::string,std::int32_t,Hash<std::string>>& StatMap()const noexcept{return v_StatMap;}
    const std::unordered_map<std::int32_t,std::optional<Decimal>,Hash<std::int32_t>>& PriceMap()const noexcept{return v_PriceMap;}
    const std::unordered_map<T_Game_Config_Quality,std::uint32_t,Hash<T_Game_Config_Quality>>& QualityMap()const noexcept{return v_QualityMap;}
    const std::unordered_map<std::string,std::vector<std::int32_t>,Hash<std::string>>& ListMap()const noexcept{return v_ListMap;}
    const Matrix<std::int32_t>& IntMatrix()const noexcept{return v_IntMatrix;}
    const Matrix<Decimal>& DecimalCube()const noexcept{return v_DecimalCube;}
    const Matrix<std::optional<std::uint32_t>>& OptionalMatrix()const noexcept{return v_OptionalMatrix;}
    const std::string& RawJsonValue()const noexcept{return v_RawJsonValue;}
    const std::vector<std::string>& RawJsonArray()const noexcept{return v_RawJsonArray;}
    const std::vector<std::vector<std::string>>& RawJsonArray2D()const noexcept{return v_RawJsonArray2D;}
    const std::vector<std::vector<std::vector<std::string>>>& RawJsonArray3D()const noexcept{return v_RawJsonArray3D;}
    const std::vector<std::int32_t>& JsonIntArray()const noexcept{return v_JsonIntArray;}
};
class AllTypesRow:public DataRowHandle<Snapshot,AllTypesData>{
    friend class Snapshot;
    AllTypesRow(std::shared_ptr<const Snapshot> owner,const AllTypesData* value):DataRowHandle(std::move(owner),value){}
public:
    std::int32_t Id()const noexcept{return value_->Id();}
    std::uint8_t ByteValue()const noexcept{return value_->ByteValue();}
    std::int16_t ShortValue()const noexcept{return value_->ShortValue();}
    std::int32_t IntValue()const noexcept{return value_->IntValue();}
    std::int64_t LongValue()const noexcept{return value_->LongValue();}
    float FloatValue()const noexcept{return value_->FloatValue();}
    double DoubleValue()const noexcept{return value_->DoubleValue();}
    bool BoolValue()const noexcept{return value_->BoolValue();}
    OwnedValue<std::string> StringValue()const{return select(&AllTypesData::StringValue);}
    std::int8_t SByteValue()const noexcept{return value_->SByteValue();}
    std::uint16_t UShortValue()const noexcept{return value_->UShortValue();}
    std::uint32_t UIntValue()const noexcept{return value_->UIntValue();}
    std::uint64_t ULongValue()const noexcept{return value_->ULongValue();}
    OwnedValue<Decimal> DecimalValue()const{return select(&AllTypesData::DecimalValue);}
    T_Game_Config_Quality QualityValue()const noexcept{return value_->QualityValue();}
    OwnedValue<std::shared_ptr<const T_ZrConfig_Types_Vector2>> Vector2Value()const{return select(&AllTypesData::Vector2Value);}
    OwnedValue<std::shared_ptr<const T_ZrConfig_Types_Vector3>> Vector3Value()const{return select(&AllTypesData::Vector3Value);}
    OwnedValue<std::shared_ptr<const T_ZrConfig_Types_Kv>> KvValue()const{return select(&AllTypesData::KvValue);}
    OwnedValue<std::shared_ptr<const T_Game_Config_Money>> MoneyValue()const{return select(&AllTypesData::MoneyValue);}
    OwnedValue<std::vector<std::uint8_t>> ByteArray()const{return select(&AllTypesData::ByteArray);}
    OwnedValue<std::vector<std::int16_t>> ShortArray()const{return select(&AllTypesData::ShortArray);}
    OwnedValue<std::vector<std::int32_t>> IntArray()const{return select(&AllTypesData::IntArray);}
    OwnedValue<std::vector<std::int64_t>> LongArray()const{return select(&AllTypesData::LongArray);}
    OwnedValue<std::vector<float>> FloatArray()const{return select(&AllTypesData::FloatArray);}
    OwnedValue<std::vector<double>> DoubleArray()const{return select(&AllTypesData::DoubleArray);}
    OwnedValue<std::vector<bool>> BoolArray()const{return select(&AllTypesData::BoolArray);}
    OwnedValue<std::vector<std::string>> StringArray()const{return select(&AllTypesData::StringArray);}
    OwnedValue<std::vector<std::int8_t>> SByteArray()const{return select(&AllTypesData::SByteArray);}
    OwnedValue<std::vector<std::uint16_t>> UShortArray()const{return select(&AllTypesData::UShortArray);}
    OwnedValue<std::vector<std::uint32_t>> UIntArray()const{return select(&AllTypesData::UIntArray);}
    OwnedValue<std::vector<std::uint64_t>> ULongArray()const{return select(&AllTypesData::ULongArray);}
    OwnedValue<std::vector<Decimal>> DecimalArray()const{return select(&AllTypesData::DecimalArray);}
    OwnedValue<std::vector<T_Game_Config_Quality>> QualityArray()const{return select(&AllTypesData::QualityArray);}
    OwnedValue<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>> Vector2Array()const{return select(&AllTypesData::Vector2Array);}
    OwnedValue<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>> Vector3Array()const{return select(&AllTypesData::Vector3Array);}
    OwnedValue<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>> KvArray()const{return select(&AllTypesData::KvArray);}
    OwnedValue<std::vector<std::shared_ptr<const T_Game_Config_Money>>> MoneyArray()const{return select(&AllTypesData::MoneyArray);}
    OwnedValue<std::vector<std::vector<std::uint8_t>>> ByteArray2D()const{return select(&AllTypesData::ByteArray2D);}
    OwnedValue<std::vector<std::vector<std::int16_t>>> ShortArray2D()const{return select(&AllTypesData::ShortArray2D);}
    OwnedValue<std::vector<std::vector<std::int32_t>>> IntArray2D()const{return select(&AllTypesData::IntArray2D);}
    OwnedValue<std::vector<std::vector<std::int64_t>>> LongArray2D()const{return select(&AllTypesData::LongArray2D);}
    OwnedValue<std::vector<std::vector<float>>> FloatArray2D()const{return select(&AllTypesData::FloatArray2D);}
    OwnedValue<std::vector<std::vector<double>>> DoubleArray2D()const{return select(&AllTypesData::DoubleArray2D);}
    OwnedValue<std::vector<std::vector<bool>>> BoolArray2D()const{return select(&AllTypesData::BoolArray2D);}
    OwnedValue<std::vector<std::vector<std::string>>> StringArray2D()const{return select(&AllTypesData::StringArray2D);}
    OwnedValue<std::vector<std::vector<std::int8_t>>> SByteArray2D()const{return select(&AllTypesData::SByteArray2D);}
    OwnedValue<std::vector<std::vector<std::uint16_t>>> UShortArray2D()const{return select(&AllTypesData::UShortArray2D);}
    OwnedValue<std::vector<std::vector<std::uint32_t>>> UIntArray2D()const{return select(&AllTypesData::UIntArray2D);}
    OwnedValue<std::vector<std::vector<std::uint64_t>>> ULongArray2D()const{return select(&AllTypesData::ULongArray2D);}
    OwnedValue<std::vector<std::vector<Decimal>>> DecimalArray2D()const{return select(&AllTypesData::DecimalArray2D);}
    OwnedValue<std::vector<std::vector<T_Game_Config_Quality>>> QualityArray2D()const{return select(&AllTypesData::QualityArray2D);}
    OwnedValue<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>>> Vector2Array2D()const{return select(&AllTypesData::Vector2Array2D);}
    OwnedValue<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>>> Vector3Array2D()const{return select(&AllTypesData::Vector3Array2D);}
    OwnedValue<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>>> KvArray2D()const{return select(&AllTypesData::KvArray2D);}
    OwnedValue<std::vector<std::vector<std::shared_ptr<const T_Game_Config_Money>>>> MoneyArray2D()const{return select(&AllTypesData::MoneyArray2D);}
    OwnedValue<std::vector<std::vector<std::vector<std::uint8_t>>>> ByteArray3D()const{return select(&AllTypesData::ByteArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::int16_t>>>> ShortArray3D()const{return select(&AllTypesData::ShortArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::int32_t>>>> IntArray3D()const{return select(&AllTypesData::IntArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::int64_t>>>> LongArray3D()const{return select(&AllTypesData::LongArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<float>>>> FloatArray3D()const{return select(&AllTypesData::FloatArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<double>>>> DoubleArray3D()const{return select(&AllTypesData::DoubleArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<bool>>>> BoolArray3D()const{return select(&AllTypesData::BoolArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::string>>>> StringArray3D()const{return select(&AllTypesData::StringArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::int8_t>>>> SByteArray3D()const{return select(&AllTypesData::SByteArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::uint16_t>>>> UShortArray3D()const{return select(&AllTypesData::UShortArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::uint32_t>>>> UIntArray3D()const{return select(&AllTypesData::UIntArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::uint64_t>>>> ULongArray3D()const{return select(&AllTypesData::ULongArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<Decimal>>>> DecimalArray3D()const{return select(&AllTypesData::DecimalArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<T_Game_Config_Quality>>>> QualityArray3D()const{return select(&AllTypesData::QualityArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>>>> Vector2Array3D()const{return select(&AllTypesData::Vector2Array3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>>>> Vector3Array3D()const{return select(&AllTypesData::Vector3Array3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>>>> KvArray3D()const{return select(&AllTypesData::KvArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::shared_ptr<const T_Game_Config_Money>>>>> MoneyArray3D()const{return select(&AllTypesData::MoneyArray3D);}
    OwnedValue<std::shared_ptr<const T_Game_Config_BattleSettings>> Settings()const{return select(&AllTypesData::Settings);}
    OwnedValue<std::shared_ptr<const T_Game_Config_ConfigRange>> Range()const{return select(&AllTypesData::Range);}
    OwnedValue<std::shared_ptr<const T_Game_Config_SampleOptions>> Options()const{return select(&AllTypesData::Options);}
    OwnedValue<std::shared_ptr<const T_Game_Config_BattleAction>> Action()const{return select(&AllTypesData::Action);}
    OwnedValue<std::vector<std::shared_ptr<const T_Game_Config_BattleAction>>> Actions()const{return select(&AllTypesData::Actions);}
    OwnedValue<std::vector<std::shared_ptr<const T_Game_Config_ConfigRange>>> Ranges()const{return select(&AllTypesData::Ranges);}
    OwnedValue<std::vector<std::shared_ptr<const T_Game_Config_SampleOptions>>> OptionsList()const{return select(&AllTypesData::OptionsList);}
    OwnedValue<std::vector<std::shared_ptr<const T_Game_Config_ActionGroup>>> Groups()const{return select(&AllTypesData::Groups);}
    OwnedValue<std::vector<std::shared_ptr<const T_Game_Config_BattlePhase>>> Phases()const{return select(&AllTypesData::Phases);}
    OwnedValue<std::optional<std::shared_ptr<const T_Game_Config_ConfigRange>>> OptionalRange()const{return select(&AllTypesData::OptionalRange);}
    OwnedValue<std::optional<std::int32_t>> OptionalInt()const{return select(&AllTypesData::OptionalInt);}
    OwnedValue<std::optional<std::uint32_t>> OptionalUInt()const{return select(&AllTypesData::OptionalUInt);}
    OwnedValue<std::optional<std::uint64_t>> OptionalULong()const{return select(&AllTypesData::OptionalULong);}
    OwnedValue<std::optional<Decimal>> OptionalDecimal()const{return select(&AllTypesData::OptionalDecimal);}
    OwnedValue<std::optional<bool>> OptionalBool()const{return select(&AllTypesData::OptionalBool);}
    OwnedValue<std::optional<T_Game_Config_Quality>> OptionalQuality()const{return select(&AllTypesData::OptionalQuality);}
    OwnedValue<std::optional<std::shared_ptr<const T_Game_Config_Money>>> OptionalMoney()const{return select(&AllTypesData::OptionalMoney);}
    OwnedValue<std::vector<std::optional<std::int32_t>>> OptionalValues()const{return select(&AllTypesData::OptionalValues);}
    OwnedValue<std::unordered_map<std::string,std::int32_t,Hash<std::string>>> StatMap()const{return select(&AllTypesData::StatMap);}
    OwnedValue<std::unordered_map<std::int32_t,std::optional<Decimal>,Hash<std::int32_t>>> PriceMap()const{return select(&AllTypesData::PriceMap);}
    OwnedValue<std::unordered_map<T_Game_Config_Quality,std::uint32_t,Hash<T_Game_Config_Quality>>> QualityMap()const{return select(&AllTypesData::QualityMap);}
    OwnedValue<std::unordered_map<std::string,std::vector<std::int32_t>,Hash<std::string>>> ListMap()const{return select(&AllTypesData::ListMap);}
    OwnedValue<Matrix<std::int32_t>> IntMatrix()const{return select(&AllTypesData::IntMatrix);}
    OwnedValue<Matrix<Decimal>> DecimalCube()const{return select(&AllTypesData::DecimalCube);}
    OwnedValue<Matrix<std::optional<std::uint32_t>>> OptionalMatrix()const{return select(&AllTypesData::OptionalMatrix);}
    OwnedValue<std::string> RawJsonValue()const{return select(&AllTypesData::RawJsonValue);}
    OwnedValue<std::vector<std::string>> RawJsonArray()const{return select(&AllTypesData::RawJsonArray);}
    OwnedValue<std::vector<std::vector<std::string>>> RawJsonArray2D()const{return select(&AllTypesData::RawJsonArray2D);}
    OwnedValue<std::vector<std::vector<std::vector<std::string>>>> RawJsonArray3D()const{return select(&AllTypesData::RawJsonArray3D);}
    OwnedValue<std::vector<std::int32_t>> JsonIntArray()const{return select(&AllTypesData::JsonIntArray);}
};
class AllTypesStorage{
public:
    detail::RowStorage<AllTypesData> rows;
private:
    std::mutex groups_gate_;
    std::atomic<bool> full_{false};
    std::vector<std::size_t> all_;
    struct Key_Key{
        std::int32_t k0;
        explicit Key_Key(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    struct Index_Key:detail::OrdinalIndex<Key_Key>{
        using Base=detail::OrdinalIndex<Key_Key>;
        explicit Index_Key(std::vector<Key_Key> keys):Base(std::move(keys),true,[](const Key_Key& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Key& a,const Key_Key& b){return a.k0==b.k0;}){
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    static std::vector<Key_Key> keys(detail::IndexedRows& indexed){std::vector<Key_Key> result;result.reserve(indexed.count());for(std::size_t i=0;i<indexed.count();++i)result.emplace_back(indexed.key(i));return result;}
    Index_Key key_;
public:
    std::size_t ordinal(const std::int32_t& k0)const{return key_.ordinal(key_.find(k0));}
private:
    struct Key_Group_ById{
        std::int32_t k0;
        Key_Group_ById(const std::int32_t& k0):k0(k0){}
        explicit Key_Group_ById(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    static Key_Group_ById cached_key_Group_ById(const AllTypesData& row){return Key_Group_ById{ row.Id() };}
    struct Index_Group_ById:detail::OrdinalIndex<Key_Group_ById>{
        using Base=detail::OrdinalIndex<Key_Group_ById>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_ById(std::vector<Key_Group_ById> keys):Base(std::move(keys),false,[](const Key_Group_ById& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Group_ById& a,const Key_Group_ById& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_ById> index_ById;
    Index_Group_ById& ensure_ById(bool load,const Cancellation* cancellation){
        if(!load)return index_ById.get();return index_ById.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_ById> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_ById(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_ById>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_ById(const std::int32_t& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_ById(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
    struct Key_Group_ByQuality{
        T_Game_Config_Quality k0;
        Key_Group_ByQuality(const T_Game_Config_Quality& k0):k0(k0){}
        explicit Key_Group_ByQuality(const Json& row):k0(static_cast<T_Game_Config_Quality>(integer<std::int32_t>(row.field("QualityValue")))){}
    };
    static Key_Group_ByQuality cached_key_Group_ByQuality(const AllTypesData& row){return Key_Group_ByQuality{ row.QualityValue() };}
    struct Index_Group_ByQuality:detail::OrdinalIndex<Key_Group_ByQuality>{
        using Base=detail::OrdinalIndex<Key_Group_ByQuality>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_ByQuality(std::vector<Key_Group_ByQuality> keys):Base(std::move(keys),false,[](const Key_Group_ByQuality& a){return (1*31u+Hash<T_Game_Config_Quality>{}(a.k0));},[](const Key_Group_ByQuality& a,const Key_Group_ByQuality& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const T_Game_Config_Quality& k0)const{auto slot=mix((1*31u+Hash<T_Game_Config_Quality>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_ByQuality> index_ByQuality;
    Index_Group_ByQuality& ensure_ByQuality(bool load,const Cancellation* cancellation){
        if(!load)return index_ByQuality.get();return index_ByQuality.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_ByQuality> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_ByQuality(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_ByQuality>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_ByQuality(const T_Game_Config_Quality& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_ByQuality(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
public:
    explicit AllTypesStorage(std::unique_ptr<detail::IndexedRows> indexed):rows(std::move(indexed)),key_(keys(rows.indexed())){}
    void load_all(const Cancellation& cancellation){
        cancellation.check();if(full_.load(std::memory_order_acquire))return;for(std::size_t i=0;i<rows.count();++i)rows.load(i,cancellation);
        ensure_ById(true,&cancellation);
        ensure_ByQuality(true,&cancellation);
        std::lock_guard<std::mutex> lock(groups_gate_);cancellation.check();if(!full_.load(std::memory_order_acquire)){all_.reserve(rows.count());for(std::size_t i=0;i<rows.count();++i)all_.push_back(i);full_.store(true,std::memory_order_release);}
    }
    bool is_complete()const noexcept{return full_.load(std::memory_order_acquire); }
    const std::vector<std::size_t>& all()const{if(!full_.load(std::memory_order_acquire))throw ConfigError(ErrorCode::TableNotLoaded,"Load the table explicitly");return all_;}
};
class AllTypes10kData {
    std::int32_t v_Id;
    std::uint8_t v_ByteValue;
    std::int16_t v_ShortValue;
    std::int32_t v_IntValue;
    std::int64_t v_LongValue;
    float v_FloatValue;
    double v_DoubleValue;
    bool v_BoolValue;
    std::string v_StringValue;
    std::int8_t v_SByteValue;
    std::uint16_t v_UShortValue;
    std::uint32_t v_UIntValue;
    std::uint64_t v_ULongValue;
    Decimal v_DecimalValue;
    T_Game_Config_Quality v_QualityValue;
    std::shared_ptr<const T_ZrConfig_Types_Vector2> v_Vector2Value;
    std::shared_ptr<const T_ZrConfig_Types_Vector3> v_Vector3Value;
    std::shared_ptr<const T_ZrConfig_Types_Kv> v_KvValue;
    std::shared_ptr<const T_Game_Config_Money> v_MoneyValue;
    std::vector<std::uint8_t> v_ByteArray;
    std::vector<std::int16_t> v_ShortArray;
    std::vector<std::int32_t> v_IntArray;
    std::vector<std::int64_t> v_LongArray;
    std::vector<float> v_FloatArray;
    std::vector<double> v_DoubleArray;
    std::vector<bool> v_BoolArray;
    std::vector<std::string> v_StringArray;
    std::vector<std::int8_t> v_SByteArray;
    std::vector<std::uint16_t> v_UShortArray;
    std::vector<std::uint32_t> v_UIntArray;
    std::vector<std::uint64_t> v_ULongArray;
    std::vector<Decimal> v_DecimalArray;
    std::vector<T_Game_Config_Quality> v_QualityArray;
    std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>> v_Vector2Array;
    std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>> v_Vector3Array;
    std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>> v_KvArray;
    std::vector<std::shared_ptr<const T_Game_Config_Money>> v_MoneyArray;
    std::vector<std::vector<std::uint8_t>> v_ByteArray2D;
    std::vector<std::vector<std::int16_t>> v_ShortArray2D;
    std::vector<std::vector<std::int32_t>> v_IntArray2D;
    std::vector<std::vector<std::int64_t>> v_LongArray2D;
    std::vector<std::vector<float>> v_FloatArray2D;
    std::vector<std::vector<double>> v_DoubleArray2D;
    std::vector<std::vector<bool>> v_BoolArray2D;
    std::vector<std::vector<std::string>> v_StringArray2D;
    std::vector<std::vector<std::int8_t>> v_SByteArray2D;
    std::vector<std::vector<std::uint16_t>> v_UShortArray2D;
    std::vector<std::vector<std::uint32_t>> v_UIntArray2D;
    std::vector<std::vector<std::uint64_t>> v_ULongArray2D;
    std::vector<std::vector<Decimal>> v_DecimalArray2D;
    std::vector<std::vector<T_Game_Config_Quality>> v_QualityArray2D;
    std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>> v_Vector2Array2D;
    std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>> v_Vector3Array2D;
    std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>> v_KvArray2D;
    std::vector<std::vector<std::shared_ptr<const T_Game_Config_Money>>> v_MoneyArray2D;
    std::vector<std::vector<std::vector<std::uint8_t>>> v_ByteArray3D;
    std::vector<std::vector<std::vector<std::int16_t>>> v_ShortArray3D;
    std::vector<std::vector<std::vector<std::int32_t>>> v_IntArray3D;
    std::vector<std::vector<std::vector<std::int64_t>>> v_LongArray3D;
    std::vector<std::vector<std::vector<float>>> v_FloatArray3D;
    std::vector<std::vector<std::vector<double>>> v_DoubleArray3D;
    std::vector<std::vector<std::vector<bool>>> v_BoolArray3D;
    std::vector<std::vector<std::vector<std::string>>> v_StringArray3D;
    std::vector<std::vector<std::vector<std::int8_t>>> v_SByteArray3D;
    std::vector<std::vector<std::vector<std::uint16_t>>> v_UShortArray3D;
    std::vector<std::vector<std::vector<std::uint32_t>>> v_UIntArray3D;
    std::vector<std::vector<std::vector<std::uint64_t>>> v_ULongArray3D;
    std::vector<std::vector<std::vector<Decimal>>> v_DecimalArray3D;
    std::vector<std::vector<std::vector<T_Game_Config_Quality>>> v_QualityArray3D;
    std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>>> v_Vector2Array3D;
    std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>>> v_Vector3Array3D;
    std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>>> v_KvArray3D;
    std::vector<std::vector<std::vector<std::shared_ptr<const T_Game_Config_Money>>>> v_MoneyArray3D;
    std::shared_ptr<const T_Game_Config_BattleSettings> v_Settings;
    std::shared_ptr<const T_Game_Config_ConfigRange> v_Range;
    std::shared_ptr<const T_Game_Config_SampleOptions> v_Options;
    std::shared_ptr<const T_Game_Config_BattleAction> v_Action;
    std::vector<std::shared_ptr<const T_Game_Config_BattleAction>> v_Actions;
    std::vector<std::shared_ptr<const T_Game_Config_ConfigRange>> v_Ranges;
    std::vector<std::shared_ptr<const T_Game_Config_SampleOptions>> v_OptionsList;
    std::vector<std::shared_ptr<const T_Game_Config_ActionGroup>> v_Groups;
    std::vector<std::shared_ptr<const T_Game_Config_BattlePhase>> v_Phases;
    std::optional<std::shared_ptr<const T_Game_Config_ConfigRange>> v_OptionalRange;
    std::optional<std::int32_t> v_OptionalInt;
    std::optional<std::uint32_t> v_OptionalUInt;
    std::optional<std::uint64_t> v_OptionalULong;
    std::optional<Decimal> v_OptionalDecimal;
    std::optional<bool> v_OptionalBool;
    std::optional<T_Game_Config_Quality> v_OptionalQuality;
    std::optional<std::shared_ptr<const T_Game_Config_Money>> v_OptionalMoney;
    std::vector<std::optional<std::int32_t>> v_OptionalValues;
    std::unordered_map<std::string,std::int32_t,Hash<std::string>> v_StatMap;
    std::unordered_map<std::int32_t,std::optional<Decimal>,Hash<std::int32_t>> v_PriceMap;
    std::unordered_map<T_Game_Config_Quality,std::uint32_t,Hash<T_Game_Config_Quality>> v_QualityMap;
    std::unordered_map<std::string,std::vector<std::int32_t>,Hash<std::string>> v_ListMap;
    Matrix<std::int32_t> v_IntMatrix;
    Matrix<Decimal> v_DecimalCube;
    Matrix<std::optional<std::uint32_t>> v_OptionalMatrix;
    std::string v_RawJsonValue;
    std::vector<std::string> v_RawJsonArray;
    std::vector<std::vector<std::string>> v_RawJsonArray2D;
    std::vector<std::vector<std::vector<std::string>>> v_RawJsonArray3D;
    std::vector<std::int32_t> v_JsonIntArray;
    std::int32_t v_Hero;
    std::int32_t v_Skill;
    std::int32_t v_Previous;
public:
    explicit AllTypes10kData(const Json& value):v_Id(integer<std::int32_t>(value.field("Id"))),v_ByteValue(integer<std::uint8_t>(value.field("ByteValue"))),v_ShortValue(integer<std::int16_t>(value.field("ShortValue"))),v_IntValue(integer<std::int32_t>(value.field("IntValue"))),v_LongValue(integer<std::int64_t>(value.field("LongValue"))),v_FloatValue(floating<float>(value.field("FloatValue"))),v_DoubleValue(floating<double>(value.field("DoubleValue"))),v_BoolValue(value.field("BoolValue").boolean()),v_StringValue(value.field("StringValue").string()),v_SByteValue(integer<std::int8_t>(value.field("SByteValue"))),v_UShortValue(integer<std::uint16_t>(value.field("UShortValue"))),v_UIntValue(integer<std::uint32_t>(value.field("UIntValue"))),v_ULongValue(integer<std::uint64_t>(value.field("ULongValue"))),v_DecimalValue(Decimal(value.field("DecimalValue").string())),v_QualityValue(static_cast<T_Game_Config_Quality>(integer<std::int32_t>(value.field("QualityValue")))),v_Vector2Value(read_T_ZrConfig_Types_Vector2(value.field("Vector2Value"))),v_Vector3Value(read_T_ZrConfig_Types_Vector3(value.field("Vector3Value"))),v_KvValue(read_T_ZrConfig_Types_Kv(value.field("KvValue"))),v_MoneyValue(read_T_Game_Config_Money(value.field("MoneyValue"))),v_ByteArray(decode_list(value.field("ByteArray"),[](const Json& item0) { return integer<std::uint8_t>(item0); })),v_ShortArray(decode_list(value.field("ShortArray"),[](const Json& item0) { return integer<std::int16_t>(item0); })),v_IntArray(decode_list(value.field("IntArray"),[](const Json& item0) { return integer<std::int32_t>(item0); })),v_LongArray(decode_list(value.field("LongArray"),[](const Json& item0) { return integer<std::int64_t>(item0); })),v_FloatArray(decode_list(value.field("FloatArray"),[](const Json& item0) { return floating<float>(item0); })),v_DoubleArray(decode_list(value.field("DoubleArray"),[](const Json& item0) { return floating<double>(item0); })),v_BoolArray(decode_list(value.field("BoolArray"),[](const Json& item0) { return item0.boolean(); })),v_StringArray(decode_list(value.field("StringArray"),[](const Json& item0) { return item0.string(); })),v_SByteArray(decode_list(value.field("SByteArray"),[](const Json& item0) { return integer<std::int8_t>(item0); })),v_UShortArray(decode_list(value.field("UShortArray"),[](const Json& item0) { return integer<std::uint16_t>(item0); })),v_UIntArray(decode_list(value.field("UIntArray"),[](const Json& item0) { return integer<std::uint32_t>(item0); })),v_ULongArray(decode_list(value.field("ULongArray"),[](const Json& item0) { return integer<std::uint64_t>(item0); })),v_DecimalArray(decode_list(value.field("DecimalArray"),[](const Json& item0) { return Decimal(item0.string()); })),v_QualityArray(decode_list(value.field("QualityArray"),[](const Json& item0) { return static_cast<T_Game_Config_Quality>(integer<std::int32_t>(item0)); })),v_Vector2Array(decode_list(value.field("Vector2Array"),[](const Json& item0) { return read_T_ZrConfig_Types_Vector2(item0); })),v_Vector3Array(decode_list(value.field("Vector3Array"),[](const Json& item0) { return read_T_ZrConfig_Types_Vector3(item0); })),v_KvArray(decode_list(value.field("KvArray"),[](const Json& item0) { return read_T_ZrConfig_Types_Kv(item0); })),v_MoneyArray(decode_list(value.field("MoneyArray"),[](const Json& item0) { return read_T_Game_Config_Money(item0); })),v_ByteArray2D(decode_list(value.field("ByteArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::uint8_t>(item1); }); })),v_ShortArray2D(decode_list(value.field("ShortArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::int16_t>(item1); }); })),v_IntArray2D(decode_list(value.field("IntArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::int32_t>(item1); }); })),v_LongArray2D(decode_list(value.field("LongArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::int64_t>(item1); }); })),v_FloatArray2D(decode_list(value.field("FloatArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return floating<float>(item1); }); })),v_DoubleArray2D(decode_list(value.field("DoubleArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return floating<double>(item1); }); })),v_BoolArray2D(decode_list(value.field("BoolArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return item1.boolean(); }); })),v_StringArray2D(decode_list(value.field("StringArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return item1.string(); }); })),v_SByteArray2D(decode_list(value.field("SByteArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::int8_t>(item1); }); })),v_UShortArray2D(decode_list(value.field("UShortArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::uint16_t>(item1); }); })),v_UIntArray2D(decode_list(value.field("UIntArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::uint32_t>(item1); }); })),v_ULongArray2D(decode_list(value.field("ULongArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::uint64_t>(item1); }); })),v_DecimalArray2D(decode_list(value.field("DecimalArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return Decimal(item1.string()); }); })),v_QualityArray2D(decode_list(value.field("QualityArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return static_cast<T_Game_Config_Quality>(integer<std::int32_t>(item1)); }); })),v_Vector2Array2D(decode_list(value.field("Vector2Array2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return read_T_ZrConfig_Types_Vector2(item1); }); })),v_Vector3Array2D(decode_list(value.field("Vector3Array2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return read_T_ZrConfig_Types_Vector3(item1); }); })),v_KvArray2D(decode_list(value.field("KvArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return read_T_ZrConfig_Types_Kv(item1); }); })),v_MoneyArray2D(decode_list(value.field("MoneyArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return read_T_Game_Config_Money(item1); }); })),v_ByteArray3D(decode_list(value.field("ByteArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::uint8_t>(item2); }); }); })),v_ShortArray3D(decode_list(value.field("ShortArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::int16_t>(item2); }); }); })),v_IntArray3D(decode_list(value.field("IntArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::int32_t>(item2); }); }); })),v_LongArray3D(decode_list(value.field("LongArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::int64_t>(item2); }); }); })),v_FloatArray3D(decode_list(value.field("FloatArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return floating<float>(item2); }); }); })),v_DoubleArray3D(decode_list(value.field("DoubleArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return floating<double>(item2); }); }); })),v_BoolArray3D(decode_list(value.field("BoolArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return item2.boolean(); }); }); })),v_StringArray3D(decode_list(value.field("StringArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return item2.string(); }); }); })),v_SByteArray3D(decode_list(value.field("SByteArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::int8_t>(item2); }); }); })),v_UShortArray3D(decode_list(value.field("UShortArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::uint16_t>(item2); }); }); })),v_UIntArray3D(decode_list(value.field("UIntArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::uint32_t>(item2); }); }); })),v_ULongArray3D(decode_list(value.field("ULongArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return integer<std::uint64_t>(item2); }); }); })),v_DecimalArray3D(decode_list(value.field("DecimalArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return Decimal(item2.string()); }); }); })),v_QualityArray3D(decode_list(value.field("QualityArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return static_cast<T_Game_Config_Quality>(integer<std::int32_t>(item2)); }); }); })),v_Vector2Array3D(decode_list(value.field("Vector2Array3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return read_T_ZrConfig_Types_Vector2(item2); }); }); })),v_Vector3Array3D(decode_list(value.field("Vector3Array3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return read_T_ZrConfig_Types_Vector3(item2); }); }); })),v_KvArray3D(decode_list(value.field("KvArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return read_T_ZrConfig_Types_Kv(item2); }); }); })),v_MoneyArray3D(decode_list(value.field("MoneyArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return read_T_Game_Config_Money(item2); }); }); })),v_Settings(read_T_Game_Config_BattleSettings(value.field("Settings"))),v_Range(read_T_Game_Config_ConfigRange(value.field("Range"))),v_Options(read_T_Game_Config_SampleOptions(value.field("Options"))),v_Action(read_T_Game_Config_BattleAction(value.field("Action"))),v_Actions(decode_list(value.field("Actions"),[](const Json& item0) { return read_T_Game_Config_BattleAction(item0); })),v_Ranges(decode_list(value.field("Ranges"),[](const Json& item0) { return read_T_Game_Config_ConfigRange(item0); })),v_OptionsList(decode_list(value.field("OptionsList"),[](const Json& item0) { return read_T_Game_Config_SampleOptions(item0); })),v_Groups(decode_list(value.field("Groups"),[](const Json& item0) { return read_T_Game_Config_ActionGroup(item0); })),v_Phases(decode_list(value.field("Phases"),[](const Json& item0) { return read_T_Game_Config_BattlePhase(item0); })),v_OptionalRange(decode_optional(value.field("OptionalRange"),[](const Json& item0) { return read_T_Game_Config_ConfigRange(item0); })),v_OptionalInt(decode_optional(value.field("OptionalInt"),[](const Json& item0) { return integer<std::int32_t>(item0); })),v_OptionalUInt(decode_optional(value.field("OptionalUInt"),[](const Json& item0) { return integer<std::uint32_t>(item0); })),v_OptionalULong(decode_optional(value.field("OptionalULong"),[](const Json& item0) { return integer<std::uint64_t>(item0); })),v_OptionalDecimal(decode_optional(value.field("OptionalDecimal"),[](const Json& item0) { return Decimal(item0.string()); })),v_OptionalBool(decode_optional(value.field("OptionalBool"),[](const Json& item0) { return item0.boolean(); })),v_OptionalQuality(decode_optional(value.field("OptionalQuality"),[](const Json& item0) { return static_cast<T_Game_Config_Quality>(integer<std::int32_t>(item0)); })),v_OptionalMoney(decode_optional(value.field("OptionalMoney"),[](const Json& item0) { return read_T_Game_Config_Money(item0); })),v_OptionalValues(decode_list(value.field("OptionalValues"),[](const Json& item0) { return decode_optional(item0,[](const Json& item1) { return integer<std::int32_t>(item1); }); })),v_StatMap(decode_dictionary(value.field("StatMap"),[](const Json& item0) { return item0.string(); },[](const Json& item0) { return integer<std::int32_t>(item0); })),v_PriceMap(decode_dictionary(value.field("PriceMap"),[](const Json& item0) { return integer<std::int32_t>(item0); },[](const Json& item0) { return decode_optional(item0,[](const Json& item1) { return Decimal(item1.string()); }); })),v_QualityMap(decode_dictionary(value.field("QualityMap"),[](const Json& item0) { return static_cast<T_Game_Config_Quality>(integer<std::int32_t>(item0)); },[](const Json& item0) { return integer<std::uint32_t>(item0); })),v_ListMap(decode_dictionary(value.field("ListMap"),[](const Json& item0) { return item0.string(); },[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return integer<std::int32_t>(item1); }); })),v_IntMatrix(decode_matrix(value.field("IntMatrix"),[](const Json& item0) { return integer<std::int32_t>(item0); })),v_DecimalCube(decode_matrix(value.field("DecimalCube"),[](const Json& item0) { return Decimal(item0.string()); })),v_OptionalMatrix(decode_matrix(value.field("OptionalMatrix"),[](const Json& item0) { return decode_optional(item0,[](const Json& item1) { return integer<std::uint32_t>(item1); }); })),v_RawJsonValue(value.field("RawJsonValue").string()),v_RawJsonArray(decode_list(value.field("RawJsonArray"),[](const Json& item0) { return item0.string(); })),v_RawJsonArray2D(decode_list(value.field("RawJsonArray2D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return item1.string(); }); })),v_RawJsonArray3D(decode_list(value.field("RawJsonArray3D"),[](const Json& item0) { return decode_list(item0,[](const Json& item1) { return decode_list(item1,[](const Json& item2) { return item2.string(); }); }); })),v_JsonIntArray(decode_list(value.field("JsonIntArray"),[](const Json& item0) { return integer<std::int32_t>(item0); })),v_Hero(integer<std::int32_t>(value.field("Hero"))),v_Skill(integer<std::int32_t>(value.field("Skill"))),v_Previous(integer<std::int32_t>(value.field("Previous"))){}
    const std::int32_t& Id()const noexcept{return v_Id;}
    const std::uint8_t& ByteValue()const noexcept{return v_ByteValue;}
    const std::int16_t& ShortValue()const noexcept{return v_ShortValue;}
    const std::int32_t& IntValue()const noexcept{return v_IntValue;}
    const std::int64_t& LongValue()const noexcept{return v_LongValue;}
    const float& FloatValue()const noexcept{return v_FloatValue;}
    const double& DoubleValue()const noexcept{return v_DoubleValue;}
    const bool& BoolValue()const noexcept{return v_BoolValue;}
    const std::string& StringValue()const noexcept{return v_StringValue;}
    const std::int8_t& SByteValue()const noexcept{return v_SByteValue;}
    const std::uint16_t& UShortValue()const noexcept{return v_UShortValue;}
    const std::uint32_t& UIntValue()const noexcept{return v_UIntValue;}
    const std::uint64_t& ULongValue()const noexcept{return v_ULongValue;}
    const Decimal& DecimalValue()const noexcept{return v_DecimalValue;}
    const T_Game_Config_Quality& QualityValue()const noexcept{return v_QualityValue;}
    const std::shared_ptr<const T_ZrConfig_Types_Vector2>& Vector2Value()const noexcept{return v_Vector2Value;}
    const std::shared_ptr<const T_ZrConfig_Types_Vector3>& Vector3Value()const noexcept{return v_Vector3Value;}
    const std::shared_ptr<const T_ZrConfig_Types_Kv>& KvValue()const noexcept{return v_KvValue;}
    const std::shared_ptr<const T_Game_Config_Money>& MoneyValue()const noexcept{return v_MoneyValue;}
    const std::vector<std::uint8_t>& ByteArray()const noexcept{return v_ByteArray;}
    const std::vector<std::int16_t>& ShortArray()const noexcept{return v_ShortArray;}
    const std::vector<std::int32_t>& IntArray()const noexcept{return v_IntArray;}
    const std::vector<std::int64_t>& LongArray()const noexcept{return v_LongArray;}
    const std::vector<float>& FloatArray()const noexcept{return v_FloatArray;}
    const std::vector<double>& DoubleArray()const noexcept{return v_DoubleArray;}
    const std::vector<bool>& BoolArray()const noexcept{return v_BoolArray;}
    const std::vector<std::string>& StringArray()const noexcept{return v_StringArray;}
    const std::vector<std::int8_t>& SByteArray()const noexcept{return v_SByteArray;}
    const std::vector<std::uint16_t>& UShortArray()const noexcept{return v_UShortArray;}
    const std::vector<std::uint32_t>& UIntArray()const noexcept{return v_UIntArray;}
    const std::vector<std::uint64_t>& ULongArray()const noexcept{return v_ULongArray;}
    const std::vector<Decimal>& DecimalArray()const noexcept{return v_DecimalArray;}
    const std::vector<T_Game_Config_Quality>& QualityArray()const noexcept{return v_QualityArray;}
    const std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>& Vector2Array()const noexcept{return v_Vector2Array;}
    const std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>& Vector3Array()const noexcept{return v_Vector3Array;}
    const std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>& KvArray()const noexcept{return v_KvArray;}
    const std::vector<std::shared_ptr<const T_Game_Config_Money>>& MoneyArray()const noexcept{return v_MoneyArray;}
    const std::vector<std::vector<std::uint8_t>>& ByteArray2D()const noexcept{return v_ByteArray2D;}
    const std::vector<std::vector<std::int16_t>>& ShortArray2D()const noexcept{return v_ShortArray2D;}
    const std::vector<std::vector<std::int32_t>>& IntArray2D()const noexcept{return v_IntArray2D;}
    const std::vector<std::vector<std::int64_t>>& LongArray2D()const noexcept{return v_LongArray2D;}
    const std::vector<std::vector<float>>& FloatArray2D()const noexcept{return v_FloatArray2D;}
    const std::vector<std::vector<double>>& DoubleArray2D()const noexcept{return v_DoubleArray2D;}
    const std::vector<std::vector<bool>>& BoolArray2D()const noexcept{return v_BoolArray2D;}
    const std::vector<std::vector<std::string>>& StringArray2D()const noexcept{return v_StringArray2D;}
    const std::vector<std::vector<std::int8_t>>& SByteArray2D()const noexcept{return v_SByteArray2D;}
    const std::vector<std::vector<std::uint16_t>>& UShortArray2D()const noexcept{return v_UShortArray2D;}
    const std::vector<std::vector<std::uint32_t>>& UIntArray2D()const noexcept{return v_UIntArray2D;}
    const std::vector<std::vector<std::uint64_t>>& ULongArray2D()const noexcept{return v_ULongArray2D;}
    const std::vector<std::vector<Decimal>>& DecimalArray2D()const noexcept{return v_DecimalArray2D;}
    const std::vector<std::vector<T_Game_Config_Quality>>& QualityArray2D()const noexcept{return v_QualityArray2D;}
    const std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>>& Vector2Array2D()const noexcept{return v_Vector2Array2D;}
    const std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>>& Vector3Array2D()const noexcept{return v_Vector3Array2D;}
    const std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>>& KvArray2D()const noexcept{return v_KvArray2D;}
    const std::vector<std::vector<std::shared_ptr<const T_Game_Config_Money>>>& MoneyArray2D()const noexcept{return v_MoneyArray2D;}
    const std::vector<std::vector<std::vector<std::uint8_t>>>& ByteArray3D()const noexcept{return v_ByteArray3D;}
    const std::vector<std::vector<std::vector<std::int16_t>>>& ShortArray3D()const noexcept{return v_ShortArray3D;}
    const std::vector<std::vector<std::vector<std::int32_t>>>& IntArray3D()const noexcept{return v_IntArray3D;}
    const std::vector<std::vector<std::vector<std::int64_t>>>& LongArray3D()const noexcept{return v_LongArray3D;}
    const std::vector<std::vector<std::vector<float>>>& FloatArray3D()const noexcept{return v_FloatArray3D;}
    const std::vector<std::vector<std::vector<double>>>& DoubleArray3D()const noexcept{return v_DoubleArray3D;}
    const std::vector<std::vector<std::vector<bool>>>& BoolArray3D()const noexcept{return v_BoolArray3D;}
    const std::vector<std::vector<std::vector<std::string>>>& StringArray3D()const noexcept{return v_StringArray3D;}
    const std::vector<std::vector<std::vector<std::int8_t>>>& SByteArray3D()const noexcept{return v_SByteArray3D;}
    const std::vector<std::vector<std::vector<std::uint16_t>>>& UShortArray3D()const noexcept{return v_UShortArray3D;}
    const std::vector<std::vector<std::vector<std::uint32_t>>>& UIntArray3D()const noexcept{return v_UIntArray3D;}
    const std::vector<std::vector<std::vector<std::uint64_t>>>& ULongArray3D()const noexcept{return v_ULongArray3D;}
    const std::vector<std::vector<std::vector<Decimal>>>& DecimalArray3D()const noexcept{return v_DecimalArray3D;}
    const std::vector<std::vector<std::vector<T_Game_Config_Quality>>>& QualityArray3D()const noexcept{return v_QualityArray3D;}
    const std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>>>& Vector2Array3D()const noexcept{return v_Vector2Array3D;}
    const std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>>>& Vector3Array3D()const noexcept{return v_Vector3Array3D;}
    const std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>>>& KvArray3D()const noexcept{return v_KvArray3D;}
    const std::vector<std::vector<std::vector<std::shared_ptr<const T_Game_Config_Money>>>>& MoneyArray3D()const noexcept{return v_MoneyArray3D;}
    const std::shared_ptr<const T_Game_Config_BattleSettings>& Settings()const noexcept{return v_Settings;}
    const std::shared_ptr<const T_Game_Config_ConfigRange>& Range()const noexcept{return v_Range;}
    const std::shared_ptr<const T_Game_Config_SampleOptions>& Options()const noexcept{return v_Options;}
    const std::shared_ptr<const T_Game_Config_BattleAction>& Action()const noexcept{return v_Action;}
    const std::vector<std::shared_ptr<const T_Game_Config_BattleAction>>& Actions()const noexcept{return v_Actions;}
    const std::vector<std::shared_ptr<const T_Game_Config_ConfigRange>>& Ranges()const noexcept{return v_Ranges;}
    const std::vector<std::shared_ptr<const T_Game_Config_SampleOptions>>& OptionsList()const noexcept{return v_OptionsList;}
    const std::vector<std::shared_ptr<const T_Game_Config_ActionGroup>>& Groups()const noexcept{return v_Groups;}
    const std::vector<std::shared_ptr<const T_Game_Config_BattlePhase>>& Phases()const noexcept{return v_Phases;}
    const std::optional<std::shared_ptr<const T_Game_Config_ConfigRange>>& OptionalRange()const noexcept{return v_OptionalRange;}
    const std::optional<std::int32_t>& OptionalInt()const noexcept{return v_OptionalInt;}
    const std::optional<std::uint32_t>& OptionalUInt()const noexcept{return v_OptionalUInt;}
    const std::optional<std::uint64_t>& OptionalULong()const noexcept{return v_OptionalULong;}
    const std::optional<Decimal>& OptionalDecimal()const noexcept{return v_OptionalDecimal;}
    const std::optional<bool>& OptionalBool()const noexcept{return v_OptionalBool;}
    const std::optional<T_Game_Config_Quality>& OptionalQuality()const noexcept{return v_OptionalQuality;}
    const std::optional<std::shared_ptr<const T_Game_Config_Money>>& OptionalMoney()const noexcept{return v_OptionalMoney;}
    const std::vector<std::optional<std::int32_t>>& OptionalValues()const noexcept{return v_OptionalValues;}
    const std::unordered_map<std::string,std::int32_t,Hash<std::string>>& StatMap()const noexcept{return v_StatMap;}
    const std::unordered_map<std::int32_t,std::optional<Decimal>,Hash<std::int32_t>>& PriceMap()const noexcept{return v_PriceMap;}
    const std::unordered_map<T_Game_Config_Quality,std::uint32_t,Hash<T_Game_Config_Quality>>& QualityMap()const noexcept{return v_QualityMap;}
    const std::unordered_map<std::string,std::vector<std::int32_t>,Hash<std::string>>& ListMap()const noexcept{return v_ListMap;}
    const Matrix<std::int32_t>& IntMatrix()const noexcept{return v_IntMatrix;}
    const Matrix<Decimal>& DecimalCube()const noexcept{return v_DecimalCube;}
    const Matrix<std::optional<std::uint32_t>>& OptionalMatrix()const noexcept{return v_OptionalMatrix;}
    const std::string& RawJsonValue()const noexcept{return v_RawJsonValue;}
    const std::vector<std::string>& RawJsonArray()const noexcept{return v_RawJsonArray;}
    const std::vector<std::vector<std::string>>& RawJsonArray2D()const noexcept{return v_RawJsonArray2D;}
    const std::vector<std::vector<std::vector<std::string>>>& RawJsonArray3D()const noexcept{return v_RawJsonArray3D;}
    const std::vector<std::int32_t>& JsonIntArray()const noexcept{return v_JsonIntArray;}
    const std::int32_t& HeroId()const noexcept{return v_Hero;}
    const std::int32_t& SkillId()const noexcept{return v_Skill;}
    const std::int32_t& PreviousId()const noexcept{return v_Previous;}
};
class AllTypes10kRow:public DataRowHandle<Snapshot,AllTypes10kData>{
    friend class Snapshot;
    AllTypes10kRow(std::shared_ptr<const Snapshot> owner,const AllTypes10kData* value):DataRowHandle(std::move(owner),value){}
public:
    std::int32_t Id()const noexcept{return value_->Id();}
    std::uint8_t ByteValue()const noexcept{return value_->ByteValue();}
    std::int16_t ShortValue()const noexcept{return value_->ShortValue();}
    std::int32_t IntValue()const noexcept{return value_->IntValue();}
    std::int64_t LongValue()const noexcept{return value_->LongValue();}
    float FloatValue()const noexcept{return value_->FloatValue();}
    double DoubleValue()const noexcept{return value_->DoubleValue();}
    bool BoolValue()const noexcept{return value_->BoolValue();}
    OwnedValue<std::string> StringValue()const{return select(&AllTypes10kData::StringValue);}
    std::int8_t SByteValue()const noexcept{return value_->SByteValue();}
    std::uint16_t UShortValue()const noexcept{return value_->UShortValue();}
    std::uint32_t UIntValue()const noexcept{return value_->UIntValue();}
    std::uint64_t ULongValue()const noexcept{return value_->ULongValue();}
    OwnedValue<Decimal> DecimalValue()const{return select(&AllTypes10kData::DecimalValue);}
    T_Game_Config_Quality QualityValue()const noexcept{return value_->QualityValue();}
    OwnedValue<std::shared_ptr<const T_ZrConfig_Types_Vector2>> Vector2Value()const{return select(&AllTypes10kData::Vector2Value);}
    OwnedValue<std::shared_ptr<const T_ZrConfig_Types_Vector3>> Vector3Value()const{return select(&AllTypes10kData::Vector3Value);}
    OwnedValue<std::shared_ptr<const T_ZrConfig_Types_Kv>> KvValue()const{return select(&AllTypes10kData::KvValue);}
    OwnedValue<std::shared_ptr<const T_Game_Config_Money>> MoneyValue()const{return select(&AllTypes10kData::MoneyValue);}
    OwnedValue<std::vector<std::uint8_t>> ByteArray()const{return select(&AllTypes10kData::ByteArray);}
    OwnedValue<std::vector<std::int16_t>> ShortArray()const{return select(&AllTypes10kData::ShortArray);}
    OwnedValue<std::vector<std::int32_t>> IntArray()const{return select(&AllTypes10kData::IntArray);}
    OwnedValue<std::vector<std::int64_t>> LongArray()const{return select(&AllTypes10kData::LongArray);}
    OwnedValue<std::vector<float>> FloatArray()const{return select(&AllTypes10kData::FloatArray);}
    OwnedValue<std::vector<double>> DoubleArray()const{return select(&AllTypes10kData::DoubleArray);}
    OwnedValue<std::vector<bool>> BoolArray()const{return select(&AllTypes10kData::BoolArray);}
    OwnedValue<std::vector<std::string>> StringArray()const{return select(&AllTypes10kData::StringArray);}
    OwnedValue<std::vector<std::int8_t>> SByteArray()const{return select(&AllTypes10kData::SByteArray);}
    OwnedValue<std::vector<std::uint16_t>> UShortArray()const{return select(&AllTypes10kData::UShortArray);}
    OwnedValue<std::vector<std::uint32_t>> UIntArray()const{return select(&AllTypes10kData::UIntArray);}
    OwnedValue<std::vector<std::uint64_t>> ULongArray()const{return select(&AllTypes10kData::ULongArray);}
    OwnedValue<std::vector<Decimal>> DecimalArray()const{return select(&AllTypes10kData::DecimalArray);}
    OwnedValue<std::vector<T_Game_Config_Quality>> QualityArray()const{return select(&AllTypes10kData::QualityArray);}
    OwnedValue<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>> Vector2Array()const{return select(&AllTypes10kData::Vector2Array);}
    OwnedValue<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>> Vector3Array()const{return select(&AllTypes10kData::Vector3Array);}
    OwnedValue<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>> KvArray()const{return select(&AllTypes10kData::KvArray);}
    OwnedValue<std::vector<std::shared_ptr<const T_Game_Config_Money>>> MoneyArray()const{return select(&AllTypes10kData::MoneyArray);}
    OwnedValue<std::vector<std::vector<std::uint8_t>>> ByteArray2D()const{return select(&AllTypes10kData::ByteArray2D);}
    OwnedValue<std::vector<std::vector<std::int16_t>>> ShortArray2D()const{return select(&AllTypes10kData::ShortArray2D);}
    OwnedValue<std::vector<std::vector<std::int32_t>>> IntArray2D()const{return select(&AllTypes10kData::IntArray2D);}
    OwnedValue<std::vector<std::vector<std::int64_t>>> LongArray2D()const{return select(&AllTypes10kData::LongArray2D);}
    OwnedValue<std::vector<std::vector<float>>> FloatArray2D()const{return select(&AllTypes10kData::FloatArray2D);}
    OwnedValue<std::vector<std::vector<double>>> DoubleArray2D()const{return select(&AllTypes10kData::DoubleArray2D);}
    OwnedValue<std::vector<std::vector<bool>>> BoolArray2D()const{return select(&AllTypes10kData::BoolArray2D);}
    OwnedValue<std::vector<std::vector<std::string>>> StringArray2D()const{return select(&AllTypes10kData::StringArray2D);}
    OwnedValue<std::vector<std::vector<std::int8_t>>> SByteArray2D()const{return select(&AllTypes10kData::SByteArray2D);}
    OwnedValue<std::vector<std::vector<std::uint16_t>>> UShortArray2D()const{return select(&AllTypes10kData::UShortArray2D);}
    OwnedValue<std::vector<std::vector<std::uint32_t>>> UIntArray2D()const{return select(&AllTypes10kData::UIntArray2D);}
    OwnedValue<std::vector<std::vector<std::uint64_t>>> ULongArray2D()const{return select(&AllTypes10kData::ULongArray2D);}
    OwnedValue<std::vector<std::vector<Decimal>>> DecimalArray2D()const{return select(&AllTypes10kData::DecimalArray2D);}
    OwnedValue<std::vector<std::vector<T_Game_Config_Quality>>> QualityArray2D()const{return select(&AllTypes10kData::QualityArray2D);}
    OwnedValue<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>>> Vector2Array2D()const{return select(&AllTypes10kData::Vector2Array2D);}
    OwnedValue<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>>> Vector3Array2D()const{return select(&AllTypes10kData::Vector3Array2D);}
    OwnedValue<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>>> KvArray2D()const{return select(&AllTypes10kData::KvArray2D);}
    OwnedValue<std::vector<std::vector<std::shared_ptr<const T_Game_Config_Money>>>> MoneyArray2D()const{return select(&AllTypes10kData::MoneyArray2D);}
    OwnedValue<std::vector<std::vector<std::vector<std::uint8_t>>>> ByteArray3D()const{return select(&AllTypes10kData::ByteArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::int16_t>>>> ShortArray3D()const{return select(&AllTypes10kData::ShortArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::int32_t>>>> IntArray3D()const{return select(&AllTypes10kData::IntArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::int64_t>>>> LongArray3D()const{return select(&AllTypes10kData::LongArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<float>>>> FloatArray3D()const{return select(&AllTypes10kData::FloatArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<double>>>> DoubleArray3D()const{return select(&AllTypes10kData::DoubleArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<bool>>>> BoolArray3D()const{return select(&AllTypes10kData::BoolArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::string>>>> StringArray3D()const{return select(&AllTypes10kData::StringArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::int8_t>>>> SByteArray3D()const{return select(&AllTypes10kData::SByteArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::uint16_t>>>> UShortArray3D()const{return select(&AllTypes10kData::UShortArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::uint32_t>>>> UIntArray3D()const{return select(&AllTypes10kData::UIntArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::uint64_t>>>> ULongArray3D()const{return select(&AllTypes10kData::ULongArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<Decimal>>>> DecimalArray3D()const{return select(&AllTypes10kData::DecimalArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<T_Game_Config_Quality>>>> QualityArray3D()const{return select(&AllTypes10kData::QualityArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector2>>>>> Vector2Array3D()const{return select(&AllTypes10kData::Vector2Array3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Vector3>>>>> Vector3Array3D()const{return select(&AllTypes10kData::Vector3Array3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>>>> KvArray3D()const{return select(&AllTypes10kData::KvArray3D);}
    OwnedValue<std::vector<std::vector<std::vector<std::shared_ptr<const T_Game_Config_Money>>>>> MoneyArray3D()const{return select(&AllTypes10kData::MoneyArray3D);}
    OwnedValue<std::shared_ptr<const T_Game_Config_BattleSettings>> Settings()const{return select(&AllTypes10kData::Settings);}
    OwnedValue<std::shared_ptr<const T_Game_Config_ConfigRange>> Range()const{return select(&AllTypes10kData::Range);}
    OwnedValue<std::shared_ptr<const T_Game_Config_SampleOptions>> Options()const{return select(&AllTypes10kData::Options);}
    OwnedValue<std::shared_ptr<const T_Game_Config_BattleAction>> Action()const{return select(&AllTypes10kData::Action);}
    OwnedValue<std::vector<std::shared_ptr<const T_Game_Config_BattleAction>>> Actions()const{return select(&AllTypes10kData::Actions);}
    OwnedValue<std::vector<std::shared_ptr<const T_Game_Config_ConfigRange>>> Ranges()const{return select(&AllTypes10kData::Ranges);}
    OwnedValue<std::vector<std::shared_ptr<const T_Game_Config_SampleOptions>>> OptionsList()const{return select(&AllTypes10kData::OptionsList);}
    OwnedValue<std::vector<std::shared_ptr<const T_Game_Config_ActionGroup>>> Groups()const{return select(&AllTypes10kData::Groups);}
    OwnedValue<std::vector<std::shared_ptr<const T_Game_Config_BattlePhase>>> Phases()const{return select(&AllTypes10kData::Phases);}
    OwnedValue<std::optional<std::shared_ptr<const T_Game_Config_ConfigRange>>> OptionalRange()const{return select(&AllTypes10kData::OptionalRange);}
    OwnedValue<std::optional<std::int32_t>> OptionalInt()const{return select(&AllTypes10kData::OptionalInt);}
    OwnedValue<std::optional<std::uint32_t>> OptionalUInt()const{return select(&AllTypes10kData::OptionalUInt);}
    OwnedValue<std::optional<std::uint64_t>> OptionalULong()const{return select(&AllTypes10kData::OptionalULong);}
    OwnedValue<std::optional<Decimal>> OptionalDecimal()const{return select(&AllTypes10kData::OptionalDecimal);}
    OwnedValue<std::optional<bool>> OptionalBool()const{return select(&AllTypes10kData::OptionalBool);}
    OwnedValue<std::optional<T_Game_Config_Quality>> OptionalQuality()const{return select(&AllTypes10kData::OptionalQuality);}
    OwnedValue<std::optional<std::shared_ptr<const T_Game_Config_Money>>> OptionalMoney()const{return select(&AllTypes10kData::OptionalMoney);}
    OwnedValue<std::vector<std::optional<std::int32_t>>> OptionalValues()const{return select(&AllTypes10kData::OptionalValues);}
    OwnedValue<std::unordered_map<std::string,std::int32_t,Hash<std::string>>> StatMap()const{return select(&AllTypes10kData::StatMap);}
    OwnedValue<std::unordered_map<std::int32_t,std::optional<Decimal>,Hash<std::int32_t>>> PriceMap()const{return select(&AllTypes10kData::PriceMap);}
    OwnedValue<std::unordered_map<T_Game_Config_Quality,std::uint32_t,Hash<T_Game_Config_Quality>>> QualityMap()const{return select(&AllTypes10kData::QualityMap);}
    OwnedValue<std::unordered_map<std::string,std::vector<std::int32_t>,Hash<std::string>>> ListMap()const{return select(&AllTypes10kData::ListMap);}
    OwnedValue<Matrix<std::int32_t>> IntMatrix()const{return select(&AllTypes10kData::IntMatrix);}
    OwnedValue<Matrix<Decimal>> DecimalCube()const{return select(&AllTypes10kData::DecimalCube);}
    OwnedValue<Matrix<std::optional<std::uint32_t>>> OptionalMatrix()const{return select(&AllTypes10kData::OptionalMatrix);}
    OwnedValue<std::string> RawJsonValue()const{return select(&AllTypes10kData::RawJsonValue);}
    OwnedValue<std::vector<std::string>> RawJsonArray()const{return select(&AllTypes10kData::RawJsonArray);}
    OwnedValue<std::vector<std::vector<std::string>>> RawJsonArray2D()const{return select(&AllTypes10kData::RawJsonArray2D);}
    OwnedValue<std::vector<std::vector<std::vector<std::string>>>> RawJsonArray3D()const{return select(&AllTypes10kData::RawJsonArray3D);}
    OwnedValue<std::vector<std::int32_t>> JsonIntArray()const{return select(&AllTypes10kData::JsonIntArray);}
    std::int32_t HeroId()const noexcept{return value_->HeroId();}
    std::optional<HeroRow> get_Hero()const;
    std::optional<HeroRow> load_Hero(const Cancellation& cancellation)const;
    std::int32_t SkillId()const noexcept{return value_->SkillId();}
    std::optional<SkillRow> get_Skill()const;
    std::optional<SkillRow> load_Skill(const Cancellation& cancellation)const;
    std::int32_t PreviousId()const noexcept{return value_->PreviousId();}
    std::optional<AllTypes10kRow> get_Previous()const;
    std::optional<AllTypes10kRow> load_Previous(const Cancellation& cancellation)const;
};
class AllTypes10kStorage{
public:
    detail::RowStorage<AllTypes10kData> rows;
private:
    std::mutex groups_gate_;
    std::atomic<bool> full_{false};
    std::vector<std::size_t> all_;
    struct Key_Key{
        std::int32_t k0;
        explicit Key_Key(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    struct Index_Key:detail::OrdinalIndex<Key_Key>{
        using Base=detail::OrdinalIndex<Key_Key>;
        explicit Index_Key(std::vector<Key_Key> keys):Base(std::move(keys),true,[](const Key_Key& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Key& a,const Key_Key& b){return a.k0==b.k0;}){
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    static std::vector<Key_Key> keys(detail::IndexedRows& indexed){std::vector<Key_Key> result;result.reserve(indexed.count());for(std::size_t i=0;i<indexed.count();++i)result.emplace_back(indexed.key(i));return result;}
    Index_Key key_;
public:
    std::size_t ordinal(const std::int32_t& k0)const{return key_.ordinal(key_.find(k0));}
private:
    struct Key_Group_ById{
        std::int32_t k0;
        Key_Group_ById(const std::int32_t& k0):k0(k0){}
        explicit Key_Group_ById(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    static Key_Group_ById cached_key_Group_ById(const AllTypes10kData& row){return Key_Group_ById{ row.Id() };}
    struct Index_Group_ById:detail::OrdinalIndex<Key_Group_ById>{
        using Base=detail::OrdinalIndex<Key_Group_ById>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_ById(std::vector<Key_Group_ById> keys):Base(std::move(keys),false,[](const Key_Group_ById& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Group_ById& a,const Key_Group_ById& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_ById> index_ById;
    Index_Group_ById& ensure_ById(bool load,const Cancellation* cancellation){
        if(!load)return index_ById.get();return index_ById.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_ById> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_ById(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_ById>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_ById(const std::int32_t& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_ById(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
public:
    explicit AllTypes10kStorage(std::unique_ptr<detail::IndexedRows> indexed):rows(std::move(indexed)),key_(keys(rows.indexed())){}
    void load_all(const Cancellation& cancellation){
        cancellation.check();if(full_.load(std::memory_order_acquire))return;for(std::size_t i=0;i<rows.count();++i)rows.load(i,cancellation);
        ensure_ById(true,&cancellation);
        std::lock_guard<std::mutex> lock(groups_gate_);cancellation.check();if(!full_.load(std::memory_order_acquire)){all_.reserve(rows.count());for(std::size_t i=0;i<rows.count();++i)all_.push_back(i);full_.store(true,std::memory_order_release);}
    }
    bool is_complete()const noexcept{return full_.load(std::memory_order_acquire); }
    const std::vector<std::size_t>& all()const{if(!full_.load(std::memory_order_acquire))throw ConfigError(ErrorCode::TableNotLoaded,"Load the table explicitly");return all_;}
};
class DropData {
    std::int32_t v_GroupId;
    std::int32_t v_Id;
    std::int32_t v_Count;
public:
    explicit DropData(const Json& value):v_GroupId(integer<std::int32_t>(value.field("GroupId"))),v_Id(integer<std::int32_t>(value.field("Id"))),v_Count(integer<std::int32_t>(value.field("Count"))){}
    const std::int32_t& GroupId()const noexcept{return v_GroupId;}
    const std::int32_t& Id()const noexcept{return v_Id;}
    const std::int32_t& Count()const noexcept{return v_Count;}
};
class DropRow:public DataRowHandle<Snapshot,DropData>{
    friend class Snapshot;
    DropRow(std::shared_ptr<const Snapshot> owner,const DropData* value):DataRowHandle(std::move(owner),value){}
public:
    std::int32_t GroupId()const noexcept{return value_->GroupId();}
    std::int32_t Id()const noexcept{return value_->Id();}
    std::int32_t Count()const noexcept{return value_->Count();}
};
class DropStorage{
public:
    detail::RowStorage<DropData> rows;
private:
    std::mutex groups_gate_;
    std::atomic<bool> full_{false};
    std::vector<std::size_t> all_;
    struct Key_Key{
        std::int32_t k0;
        std::int32_t k1;
        explicit Key_Key(const Json& row):k0(integer<std::int32_t>(row.field("GroupId"))),k1(integer<std::int32_t>(row.field("Id"))){}
    };
    struct Index_Key:detail::OrdinalIndex<Key_Key>{
        using Base=detail::OrdinalIndex<Key_Key>;
        explicit Index_Key(std::vector<Key_Key> keys):Base(std::move(keys),true,[](const Key_Key& a){return ((1*31u+Hash<std::int32_t>{}(a.k0))*31u+Hash<std::int32_t>{}(a.k1));},[](const Key_Key& a,const Key_Key& b){return a.k0==b.k0&&a.k1==b.k1;}){
        }
        std::size_t find(const std::int32_t& k0,const std::int32_t& k1)const{auto slot=mix(((1*31u+Hash<std::int32_t>{}(k0))*31u+Hash<std::int32_t>{}(k1)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0&&row.k1==k1)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    static std::vector<Key_Key> keys(detail::IndexedRows& indexed){std::vector<Key_Key> result;result.reserve(indexed.count());for(std::size_t i=0;i<indexed.count();++i)result.emplace_back(indexed.key(i));return result;}
    Index_Key key_;
public:
    std::size_t ordinal(const std::int32_t& k0,const std::int32_t& k1)const{return key_.ordinal(key_.find(k0,k1));}
private:
    struct Key_Group_ByGroupId{
        std::int32_t k0;
        Key_Group_ByGroupId(const std::int32_t& k0):k0(k0){}
        explicit Key_Group_ByGroupId(const Json& row):k0(integer<std::int32_t>(row.field("GroupId"))){}
    };
    static Key_Group_ByGroupId cached_key_Group_ByGroupId(const DropData& row){return Key_Group_ByGroupId{ row.GroupId() };}
    struct Index_Group_ByGroupId:detail::OrdinalIndex<Key_Group_ByGroupId>{
        using Base=detail::OrdinalIndex<Key_Group_ByGroupId>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_ByGroupId(std::vector<Key_Group_ByGroupId> keys):Base(std::move(keys),false,[](const Key_Group_ByGroupId& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Group_ByGroupId& a,const Key_Group_ByGroupId& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_ByGroupId> index_ByGroupId;
    Index_Group_ByGroupId& ensure_ByGroupId(bool load,const Cancellation* cancellation){
        if(!load)return index_ByGroupId.get();return index_ByGroupId.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_ByGroupId> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_ByGroupId(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_ByGroupId>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_ByGroupId(const std::int32_t& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_ByGroupId(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
    struct Key_Group_ById{
        std::int32_t k0;
        Key_Group_ById(const std::int32_t& k0):k0(k0){}
        explicit Key_Group_ById(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    static Key_Group_ById cached_key_Group_ById(const DropData& row){return Key_Group_ById{ row.Id() };}
    struct Index_Group_ById:detail::OrdinalIndex<Key_Group_ById>{
        using Base=detail::OrdinalIndex<Key_Group_ById>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_ById(std::vector<Key_Group_ById> keys):Base(std::move(keys),false,[](const Key_Group_ById& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Group_ById& a,const Key_Group_ById& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_ById> index_ById;
    Index_Group_ById& ensure_ById(bool load,const Cancellation* cancellation){
        if(!load)return index_ById.get();return index_ById.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_ById> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_ById(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_ById>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_ById(const std::int32_t& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_ById(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
public:
    explicit DropStorage(std::unique_ptr<detail::IndexedRows> indexed):rows(std::move(indexed)),key_(keys(rows.indexed())){}
    void load_all(const Cancellation& cancellation){
        cancellation.check();if(full_.load(std::memory_order_acquire))return;for(std::size_t i=0;i<rows.count();++i)rows.load(i,cancellation);
        ensure_ByGroupId(true,&cancellation);
        ensure_ById(true,&cancellation);
        std::lock_guard<std::mutex> lock(groups_gate_);cancellation.check();if(!full_.load(std::memory_order_acquire)){all_.reserve(rows.count());for(std::size_t i=0;i<rows.count();++i)all_.push_back(i);full_.store(true,std::memory_order_release);}
    }
    bool is_complete()const noexcept{return full_.load(std::memory_order_acquire); }
    const std::vector<std::size_t>& all()const{if(!full_.load(std::memory_order_acquire))throw ConfigError(ErrorCode::TableNotLoaded,"Load the table explicitly");return all_;}
};
class HeroData {
    std::int32_t v_Id;
    std::string v_Name;
    std::int32_t v_Skill;
    std::int32_t v_OtherSkill;
    std::int32_t v_Mentor;
public:
    explicit HeroData(const Json& value):v_Id(integer<std::int32_t>(value.field("Id"))),v_Name(value.field("Name").string()),v_Skill(integer<std::int32_t>(value.field("Skill"))),v_OtherSkill(integer<std::int32_t>(value.field("OtherSkill"))),v_Mentor(integer<std::int32_t>(value.field("Mentor"))){}
    const std::int32_t& Id()const noexcept{return v_Id;}
    const std::string& Name()const noexcept{return v_Name;}
    const std::int32_t& SkillId()const noexcept{return v_Skill;}
    const std::int32_t& OtherSkillId()const noexcept{return v_OtherSkill;}
    const std::int32_t& MentorId()const noexcept{return v_Mentor;}
};
class HeroRow:public DataRowHandle<Snapshot,HeroData>{
    friend class Snapshot;
    HeroRow(std::shared_ptr<const Snapshot> owner,const HeroData* value):DataRowHandle(std::move(owner),value){}
public:
    std::int32_t Id()const noexcept{return value_->Id();}
    OwnedValue<std::string> Name()const{return select(&HeroData::Name);}
    std::int32_t SkillId()const noexcept{return value_->SkillId();}
    std::optional<SkillRow> get_Skill()const;
    std::optional<SkillRow> load_Skill(const Cancellation& cancellation)const;
    std::int32_t OtherSkillId()const noexcept{return value_->OtherSkillId();}
    std::optional<SkillRow> get_OtherSkill()const;
    std::optional<SkillRow> load_OtherSkill(const Cancellation& cancellation)const;
    std::int32_t MentorId()const noexcept{return value_->MentorId();}
    std::optional<HeroRow> get_Mentor()const;
    std::optional<HeroRow> load_Mentor(const Cancellation& cancellation)const;
};
class HeroStorage{
public:
    detail::RowStorage<HeroData> rows;
private:
    std::mutex groups_gate_;
    std::atomic<bool> full_{false};
    std::vector<std::size_t> all_;
    struct Key_Key{
        std::int32_t k0;
        explicit Key_Key(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    struct Index_Key:detail::OrdinalIndex<Key_Key>{
        using Base=detail::OrdinalIndex<Key_Key>;
        explicit Index_Key(std::vector<Key_Key> keys):Base(std::move(keys),true,[](const Key_Key& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Key& a,const Key_Key& b){return a.k0==b.k0;}){
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    static std::vector<Key_Key> keys(detail::IndexedRows& indexed){std::vector<Key_Key> result;result.reserve(indexed.count());for(std::size_t i=0;i<indexed.count();++i)result.emplace_back(indexed.key(i));return result;}
    Index_Key key_;
public:
    std::size_t ordinal(const std::int32_t& k0)const{return key_.ordinal(key_.find(k0));}
private:
    struct Key_Group_ById{
        std::int32_t k0;
        Key_Group_ById(const std::int32_t& k0):k0(k0){}
        explicit Key_Group_ById(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    static Key_Group_ById cached_key_Group_ById(const HeroData& row){return Key_Group_ById{ row.Id() };}
    struct Index_Group_ById:detail::OrdinalIndex<Key_Group_ById>{
        using Base=detail::OrdinalIndex<Key_Group_ById>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_ById(std::vector<Key_Group_ById> keys):Base(std::move(keys),false,[](const Key_Group_ById& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Group_ById& a,const Key_Group_ById& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_ById> index_ById;
    Index_Group_ById& ensure_ById(bool load,const Cancellation* cancellation){
        if(!load)return index_ById.get();return index_ById.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_ById> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_ById(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_ById>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_ById(const std::int32_t& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_ById(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
public:
    explicit HeroStorage(std::unique_ptr<detail::IndexedRows> indexed):rows(std::move(indexed)),key_(keys(rows.indexed())){}
    void load_all(const Cancellation& cancellation){
        cancellation.check();if(full_.load(std::memory_order_acquire))return;for(std::size_t i=0;i<rows.count();++i)rows.load(i,cancellation);
        ensure_ById(true,&cancellation);
        std::lock_guard<std::mutex> lock(groups_gate_);cancellation.check();if(!full_.load(std::memory_order_acquire)){all_.reserve(rows.count());for(std::size_t i=0;i<rows.count();++i)all_.push_back(i);full_.store(true,std::memory_order_release);}
    }
    bool is_complete()const noexcept{return full_.load(std::memory_order_acquire); }
    const std::vector<std::size_t>& all()const{if(!full_.load(std::memory_order_acquire))throw ConfigError(ErrorCode::TableNotLoaded,"Load the table explicitly");return all_;}
};
class ItemCatalogData {
    std::int32_t v_Id;
    std::string v_Name;
    std::int32_t v_MaxStack;
    std::int32_t v_Rarity;
    bool v_Enabled;
public:
    explicit ItemCatalogData(const Json& value):v_Id(integer<std::int32_t>(value.field("Id"))),v_Name(value.field("Name").string()),v_MaxStack(integer<std::int32_t>(value.field("MaxStack"))),v_Rarity(integer<std::int32_t>(value.field("Rarity"))),v_Enabled(value.field("Enabled").boolean()){}
    const std::int32_t& Id()const noexcept{return v_Id;}
    const std::string& Name()const noexcept{return v_Name;}
    const std::int32_t& MaxStack()const noexcept{return v_MaxStack;}
    const std::int32_t& Rarity()const noexcept{return v_Rarity;}
    const bool& Enabled()const noexcept{return v_Enabled;}
};
class ItemCatalogRow:public DataRowHandle<Snapshot,ItemCatalogData>{
    friend class Snapshot;
    ItemCatalogRow(std::shared_ptr<const Snapshot> owner,const ItemCatalogData* value):DataRowHandle(std::move(owner),value){}
public:
    std::int32_t Id()const noexcept{return value_->Id();}
    OwnedValue<std::string> Name()const{return select(&ItemCatalogData::Name);}
    std::int32_t MaxStack()const noexcept{return value_->MaxStack();}
    std::int32_t Rarity()const noexcept{return value_->Rarity();}
    bool Enabled()const noexcept{return value_->Enabled();}
};
class ItemCatalogStorage{
public:
    detail::RowStorage<ItemCatalogData> rows;
private:
    std::mutex groups_gate_;
    std::atomic<bool> full_{false};
    std::vector<std::size_t> all_;
    struct Key_Key{
        std::int32_t k0;
        explicit Key_Key(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    struct Index_Key:detail::OrdinalIndex<Key_Key>{
        using Base=detail::OrdinalIndex<Key_Key>;
        explicit Index_Key(std::vector<Key_Key> keys):Base(std::move(keys),true,[](const Key_Key& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Key& a,const Key_Key& b){return a.k0==b.k0;}){
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    static std::vector<Key_Key> keys(detail::IndexedRows& indexed){std::vector<Key_Key> result;result.reserve(indexed.count());for(std::size_t i=0;i<indexed.count();++i)result.emplace_back(indexed.key(i));return result;}
    Index_Key key_;
public:
    std::size_t ordinal(const std::int32_t& k0)const{return key_.ordinal(key_.find(k0));}
private:
    struct Key_Group_ById{
        std::int32_t k0;
        Key_Group_ById(const std::int32_t& k0):k0(k0){}
        explicit Key_Group_ById(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    static Key_Group_ById cached_key_Group_ById(const ItemCatalogData& row){return Key_Group_ById{ row.Id() };}
    struct Index_Group_ById:detail::OrdinalIndex<Key_Group_ById>{
        using Base=detail::OrdinalIndex<Key_Group_ById>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_ById(std::vector<Key_Group_ById> keys):Base(std::move(keys),false,[](const Key_Group_ById& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Group_ById& a,const Key_Group_ById& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_ById> index_ById;
    Index_Group_ById& ensure_ById(bool load,const Cancellation* cancellation){
        if(!load)return index_ById.get();return index_ById.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_ById> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_ById(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_ById>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_ById(const std::int32_t& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_ById(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
public:
    explicit ItemCatalogStorage(std::unique_ptr<detail::IndexedRows> indexed):rows(std::move(indexed)),key_(keys(rows.indexed())){}
    void load_all(const Cancellation& cancellation){
        cancellation.check();if(full_.load(std::memory_order_acquire))return;for(std::size_t i=0;i<rows.count();++i)rows.load(i,cancellation);
        ensure_ById(true,&cancellation);
        std::lock_guard<std::mutex> lock(groups_gate_);cancellation.check();if(!full_.load(std::memory_order_acquire)){all_.reserve(rows.count());for(std::size_t i=0;i<rows.count();++i)all_.push_back(i);full_.store(true,std::memory_order_release);}
    }
    bool is_complete()const noexcept{return full_.load(std::memory_order_acquire); }
    const std::vector<std::size_t>& all()const{if(!full_.load(std::memory_order_acquire))throw ConfigError(ErrorCode::TableNotLoaded,"Load the table explicitly");return all_;}
};
class QuestRewardsData {
    std::int32_t v_Id;
    std::string v_Name;
    std::int32_t v_Item;
    std::int32_t v_Count;
    std::int32_t v_RequiredLevel;
    bool v_Enabled;
public:
    explicit QuestRewardsData(const Json& value):v_Id(integer<std::int32_t>(value.field("Id"))),v_Name(value.field("Name").string()),v_Item(integer<std::int32_t>(value.field("Item"))),v_Count(integer<std::int32_t>(value.field("Count"))),v_RequiredLevel(integer<std::int32_t>(value.field("RequiredLevel"))),v_Enabled(value.field("Enabled").boolean()){}
    const std::int32_t& Id()const noexcept{return v_Id;}
    const std::string& Name()const noexcept{return v_Name;}
    const std::int32_t& ItemId()const noexcept{return v_Item;}
    const std::int32_t& Count()const noexcept{return v_Count;}
    const std::int32_t& RequiredLevel()const noexcept{return v_RequiredLevel;}
    const bool& Enabled()const noexcept{return v_Enabled;}
};
class QuestRewardsRow:public DataRowHandle<Snapshot,QuestRewardsData>{
    friend class Snapshot;
    QuestRewardsRow(std::shared_ptr<const Snapshot> owner,const QuestRewardsData* value):DataRowHandle(std::move(owner),value){}
public:
    std::int32_t Id()const noexcept{return value_->Id();}
    OwnedValue<std::string> Name()const{return select(&QuestRewardsData::Name);}
    std::int32_t ItemId()const noexcept{return value_->ItemId();}
    std::optional<ItemCatalogRow> get_Item()const;
    std::optional<ItemCatalogRow> load_Item(const Cancellation& cancellation)const;
    std::int32_t Count()const noexcept{return value_->Count();}
    std::int32_t RequiredLevel()const noexcept{return value_->RequiredLevel();}
    bool Enabled()const noexcept{return value_->Enabled();}
};
class QuestRewardsStorage{
public:
    detail::RowStorage<QuestRewardsData> rows;
private:
    std::mutex groups_gate_;
    std::atomic<bool> full_{false};
    std::vector<std::size_t> all_;
    struct Key_Key{
        std::int32_t k0;
        explicit Key_Key(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    struct Index_Key:detail::OrdinalIndex<Key_Key>{
        using Base=detail::OrdinalIndex<Key_Key>;
        explicit Index_Key(std::vector<Key_Key> keys):Base(std::move(keys),true,[](const Key_Key& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Key& a,const Key_Key& b){return a.k0==b.k0;}){
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    static std::vector<Key_Key> keys(detail::IndexedRows& indexed){std::vector<Key_Key> result;result.reserve(indexed.count());for(std::size_t i=0;i<indexed.count();++i)result.emplace_back(indexed.key(i));return result;}
    Index_Key key_;
public:
    std::size_t ordinal(const std::int32_t& k0)const{return key_.ordinal(key_.find(k0));}
private:
    struct Key_Group_ById{
        std::int32_t k0;
        Key_Group_ById(const std::int32_t& k0):k0(k0){}
        explicit Key_Group_ById(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    static Key_Group_ById cached_key_Group_ById(const QuestRewardsData& row){return Key_Group_ById{ row.Id() };}
    struct Index_Group_ById:detail::OrdinalIndex<Key_Group_ById>{
        using Base=detail::OrdinalIndex<Key_Group_ById>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_ById(std::vector<Key_Group_ById> keys):Base(std::move(keys),false,[](const Key_Group_ById& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Group_ById& a,const Key_Group_ById& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_ById> index_ById;
    Index_Group_ById& ensure_ById(bool load,const Cancellation* cancellation){
        if(!load)return index_ById.get();return index_ById.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_ById> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_ById(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_ById>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_ById(const std::int32_t& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_ById(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
public:
    explicit QuestRewardsStorage(std::unique_ptr<detail::IndexedRows> indexed):rows(std::move(indexed)),key_(keys(rows.indexed())){}
    void load_all(const Cancellation& cancellation){
        cancellation.check();if(full_.load(std::memory_order_acquire))return;for(std::size_t i=0;i<rows.count();++i)rows.load(i,cancellation);
        ensure_ById(true,&cancellation);
        std::lock_guard<std::mutex> lock(groups_gate_);cancellation.check();if(!full_.load(std::memory_order_acquire)){all_.reserve(rows.count());for(std::size_t i=0;i<rows.count();++i)all_.push_back(i);full_.store(true,std::memory_order_release);}
    }
    bool is_complete()const noexcept{return full_.load(std::memory_order_acquire); }
    const std::vector<std::size_t>& all()const{if(!full_.load(std::memory_order_acquire))throw ConfigError(ErrorCode::TableNotLoaded,"Load the table explicitly");return all_;}
};
class ShopOffersData {
    std::int32_t v_Id;
    std::int32_t v_Item;
    std::int32_t v_Price;
    std::int32_t v_DailyLimit;
    bool v_Enabled;
public:
    explicit ShopOffersData(const Json& value):v_Id(integer<std::int32_t>(value.field("Id"))),v_Item(integer<std::int32_t>(value.field("Item"))),v_Price(integer<std::int32_t>(value.field("Price"))),v_DailyLimit(integer<std::int32_t>(value.field("DailyLimit"))),v_Enabled(value.field("Enabled").boolean()){}
    const std::int32_t& Id()const noexcept{return v_Id;}
    const std::int32_t& ItemId()const noexcept{return v_Item;}
    const std::int32_t& Price()const noexcept{return v_Price;}
    const std::int32_t& DailyLimit()const noexcept{return v_DailyLimit;}
    const bool& Enabled()const noexcept{return v_Enabled;}
};
class ShopOffersRow:public DataRowHandle<Snapshot,ShopOffersData>{
    friend class Snapshot;
    ShopOffersRow(std::shared_ptr<const Snapshot> owner,const ShopOffersData* value):DataRowHandle(std::move(owner),value){}
public:
    std::int32_t Id()const noexcept{return value_->Id();}
    std::int32_t ItemId()const noexcept{return value_->ItemId();}
    std::optional<ItemCatalogRow> get_Item()const;
    std::optional<ItemCatalogRow> load_Item(const Cancellation& cancellation)const;
    std::int32_t Price()const noexcept{return value_->Price();}
    std::int32_t DailyLimit()const noexcept{return value_->DailyLimit();}
    bool Enabled()const noexcept{return value_->Enabled();}
};
class ShopOffersStorage{
public:
    detail::RowStorage<ShopOffersData> rows;
private:
    std::mutex groups_gate_;
    std::atomic<bool> full_{false};
    std::vector<std::size_t> all_;
    struct Key_Key{
        std::int32_t k0;
        explicit Key_Key(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    struct Index_Key:detail::OrdinalIndex<Key_Key>{
        using Base=detail::OrdinalIndex<Key_Key>;
        explicit Index_Key(std::vector<Key_Key> keys):Base(std::move(keys),true,[](const Key_Key& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Key& a,const Key_Key& b){return a.k0==b.k0;}){
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    static std::vector<Key_Key> keys(detail::IndexedRows& indexed){std::vector<Key_Key> result;result.reserve(indexed.count());for(std::size_t i=0;i<indexed.count();++i)result.emplace_back(indexed.key(i));return result;}
    Index_Key key_;
public:
    std::size_t ordinal(const std::int32_t& k0)const{return key_.ordinal(key_.find(k0));}
private:
    struct Key_Group_ById{
        std::int32_t k0;
        Key_Group_ById(const std::int32_t& k0):k0(k0){}
        explicit Key_Group_ById(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    static Key_Group_ById cached_key_Group_ById(const ShopOffersData& row){return Key_Group_ById{ row.Id() };}
    struct Index_Group_ById:detail::OrdinalIndex<Key_Group_ById>{
        using Base=detail::OrdinalIndex<Key_Group_ById>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_ById(std::vector<Key_Group_ById> keys):Base(std::move(keys),false,[](const Key_Group_ById& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Group_ById& a,const Key_Group_ById& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_ById> index_ById;
    Index_Group_ById& ensure_ById(bool load,const Cancellation* cancellation){
        if(!load)return index_ById.get();return index_ById.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_ById> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_ById(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_ById>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_ById(const std::int32_t& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_ById(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
public:
    explicit ShopOffersStorage(std::unique_ptr<detail::IndexedRows> indexed):rows(std::move(indexed)),key_(keys(rows.indexed())){}
    void load_all(const Cancellation& cancellation){
        cancellation.check();if(full_.load(std::memory_order_acquire))return;for(std::size_t i=0;i<rows.count();++i)rows.load(i,cancellation);
        ensure_ById(true,&cancellation);
        std::lock_guard<std::mutex> lock(groups_gate_);cancellation.check();if(!full_.load(std::memory_order_acquire)){all_.reserve(rows.count());for(std::size_t i=0;i<rows.count();++i)all_.push_back(i);full_.store(true,std::memory_order_release);}
    }
    bool is_complete()const noexcept{return full_.load(std::memory_order_acquire); }
    const std::vector<std::size_t>& all()const{if(!full_.load(std::memory_order_acquire))throw ConfigError(ErrorCode::TableNotLoaded,"Load the table explicitly");return all_;}
};
class SkillData {
    std::int32_t v_Id;
    std::string v_Name;
    std::int32_t v_Hero;
public:
    explicit SkillData(const Json& value):v_Id(integer<std::int32_t>(value.field("Id"))),v_Name(value.field("Name").string()),v_Hero(integer<std::int32_t>(value.field("Hero"))){}
    const std::int32_t& Id()const noexcept{return v_Id;}
    const std::string& Name()const noexcept{return v_Name;}
    const std::int32_t& HeroId()const noexcept{return v_Hero;}
};
class SkillRow:public DataRowHandle<Snapshot,SkillData>{
    friend class Snapshot;
    SkillRow(std::shared_ptr<const Snapshot> owner,const SkillData* value):DataRowHandle(std::move(owner),value){}
public:
    std::int32_t Id()const noexcept{return value_->Id();}
    OwnedValue<std::string> Name()const{return select(&SkillData::Name);}
    std::int32_t HeroId()const noexcept{return value_->HeroId();}
    std::optional<HeroRow> get_Hero()const;
    std::optional<HeroRow> load_Hero(const Cancellation& cancellation)const;
};
class SkillStorage{
public:
    detail::RowStorage<SkillData> rows;
private:
    std::mutex groups_gate_;
    std::atomic<bool> full_{false};
    std::vector<std::size_t> all_;
    struct Key_Key{
        std::int32_t k0;
        explicit Key_Key(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    struct Index_Key:detail::OrdinalIndex<Key_Key>{
        using Base=detail::OrdinalIndex<Key_Key>;
        explicit Index_Key(std::vector<Key_Key> keys):Base(std::move(keys),true,[](const Key_Key& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Key& a,const Key_Key& b){return a.k0==b.k0;}){
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    static std::vector<Key_Key> keys(detail::IndexedRows& indexed){std::vector<Key_Key> result;result.reserve(indexed.count());for(std::size_t i=0;i<indexed.count();++i)result.emplace_back(indexed.key(i));return result;}
    Index_Key key_;
public:
    std::size_t ordinal(const std::int32_t& k0)const{return key_.ordinal(key_.find(k0));}
private:
    struct Key_Group_ById{
        std::int32_t k0;
        Key_Group_ById(const std::int32_t& k0):k0(k0){}
        explicit Key_Group_ById(const Json& row):k0(integer<std::int32_t>(row.field("Id"))){}
    };
    static Key_Group_ById cached_key_Group_ById(const SkillData& row){return Key_Group_ById{ row.Id() };}
    struct Index_Group_ById:detail::OrdinalIndex<Key_Group_ById>{
        using Base=detail::OrdinalIndex<Key_Group_ById>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_ById(std::vector<Key_Group_ById> keys):Base(std::move(keys),false,[](const Key_Group_ById& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Group_ById& a,const Key_Group_ById& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_ById> index_ById;
    Index_Group_ById& ensure_ById(bool load,const Cancellation* cancellation){
        if(!load)return index_ById.get();return index_ById.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_ById> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_ById(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_ById>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_ById(const std::int32_t& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_ById(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
public:
    explicit SkillStorage(std::unique_ptr<detail::IndexedRows> indexed):rows(std::move(indexed)),key_(keys(rows.indexed())){}
    void load_all(const Cancellation& cancellation){
        cancellation.check();if(full_.load(std::memory_order_acquire))return;for(std::size_t i=0;i<rows.count();++i)rows.load(i,cancellation);
        ensure_ById(true,&cancellation);
        std::lock_guard<std::mutex> lock(groups_gate_);cancellation.check();if(!full_.load(std::memory_order_acquire)){all_.reserve(rows.count());for(std::size_t i=0;i<rows.count();++i)all_.push_back(i);full_.store(true,std::memory_order_release);}
    }
    bool is_complete()const noexcept{return full_.load(std::memory_order_acquire); }
    const std::vector<std::size_t>& all()const{if(!full_.load(std::memory_order_acquire))throw ConfigError(ErrorCode::TableNotLoaded,"Load the table explicitly");return all_;}
};
class SkillLevelData {
    std::int32_t v_SkillId;
    std::string v_Name;
    std::int32_t v_Level;
    std::int32_t v_Quality;
    std::int32_t v_Damage;
public:
    explicit SkillLevelData(const Json& value):v_SkillId(integer<std::int32_t>(value.field("SkillId"))),v_Name(value.field("Name").string()),v_Level(integer<std::int32_t>(value.field("Level"))),v_Quality(integer<std::int32_t>(value.field("Quality"))),v_Damage(integer<std::int32_t>(value.field("Damage"))){}
    const std::int32_t& SkillId()const noexcept{return v_SkillId;}
    const std::string& Name()const noexcept{return v_Name;}
    const std::int32_t& Level()const noexcept{return v_Level;}
    const std::int32_t& Quality()const noexcept{return v_Quality;}
    const std::int32_t& Damage()const noexcept{return v_Damage;}
};
class SkillLevelRow:public DataRowHandle<Snapshot,SkillLevelData>{
    friend class Snapshot;
    SkillLevelRow(std::shared_ptr<const Snapshot> owner,const SkillLevelData* value):DataRowHandle(std::move(owner),value){}
public:
    std::int32_t SkillId()const noexcept{return value_->SkillId();}
    OwnedValue<std::string> Name()const{return select(&SkillLevelData::Name);}
    std::int32_t Level()const noexcept{return value_->Level();}
    std::int32_t Quality()const noexcept{return value_->Quality();}
    std::int32_t Damage()const noexcept{return value_->Damage();}
};
class SkillLevelStorage{
public:
    detail::RowStorage<SkillLevelData> rows;
private:
    std::mutex groups_gate_;
    std::atomic<bool> full_{false};
    std::vector<std::size_t> all_;
    struct Key_Key{
        std::int32_t k0;
        std::int32_t k1;
        std::int32_t k2;
        explicit Key_Key(const Json& row):k0(integer<std::int32_t>(row.field("SkillId"))),k1(integer<std::int32_t>(row.field("Level"))),k2(integer<std::int32_t>(row.field("Quality"))){}
    };
    struct Index_Key:detail::OrdinalIndex<Key_Key>{
        using Base=detail::OrdinalIndex<Key_Key>;
        explicit Index_Key(std::vector<Key_Key> keys):Base(std::move(keys),true,[](const Key_Key& a){return (((1*31u+Hash<std::int32_t>{}(a.k0))*31u+Hash<std::int32_t>{}(a.k1))*31u+Hash<std::int32_t>{}(a.k2));},[](const Key_Key& a,const Key_Key& b){return a.k0==b.k0&&a.k1==b.k1&&a.k2==b.k2;}){
        }
        std::size_t find(const std::int32_t& k0,const std::int32_t& k1,const std::int32_t& k2)const{auto slot=mix((((1*31u+Hash<std::int32_t>{}(k0))*31u+Hash<std::int32_t>{}(k1))*31u+Hash<std::int32_t>{}(k2)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0&&row.k1==k1&&row.k2==k2)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    static std::vector<Key_Key> keys(detail::IndexedRows& indexed){std::vector<Key_Key> result;result.reserve(indexed.count());for(std::size_t i=0;i<indexed.count();++i)result.emplace_back(indexed.key(i));return result;}
    Index_Key key_;
public:
    std::size_t ordinal(const std::int32_t& k0,const std::int32_t& k1,const std::int32_t& k2)const{return key_.ordinal(key_.find(k0,k1,k2));}
private:
    struct Key_Group_ByLevel{
        std::int32_t k0;
        Key_Group_ByLevel(const std::int32_t& k0):k0(k0){}
        explicit Key_Group_ByLevel(const Json& row):k0(integer<std::int32_t>(row.field("Level"))){}
    };
    static Key_Group_ByLevel cached_key_Group_ByLevel(const SkillLevelData& row){return Key_Group_ByLevel{ row.Level() };}
    struct Index_Group_ByLevel:detail::OrdinalIndex<Key_Group_ByLevel>{
        using Base=detail::OrdinalIndex<Key_Group_ByLevel>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_ByLevel(std::vector<Key_Group_ByLevel> keys):Base(std::move(keys),false,[](const Key_Group_ByLevel& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Group_ByLevel& a,const Key_Group_ByLevel& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_ByLevel> index_ByLevel;
    Index_Group_ByLevel& ensure_ByLevel(bool load,const Cancellation* cancellation){
        if(!load)return index_ByLevel.get();return index_ByLevel.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_ByLevel> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_ByLevel(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_ByLevel>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_ByLevel(const std::int32_t& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_ByLevel(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
    struct Key_Group_ByQuality{
        std::int32_t k0;
        Key_Group_ByQuality(const std::int32_t& k0):k0(k0){}
        explicit Key_Group_ByQuality(const Json& row):k0(integer<std::int32_t>(row.field("Quality"))){}
    };
    static Key_Group_ByQuality cached_key_Group_ByQuality(const SkillLevelData& row){return Key_Group_ByQuality{ row.Quality() };}
    struct Index_Group_ByQuality:detail::OrdinalIndex<Key_Group_ByQuality>{
        using Base=detail::OrdinalIndex<Key_Group_ByQuality>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_ByQuality(std::vector<Key_Group_ByQuality> keys):Base(std::move(keys),false,[](const Key_Group_ByQuality& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Group_ByQuality& a,const Key_Group_ByQuality& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_ByQuality> index_ByQuality;
    Index_Group_ByQuality& ensure_ByQuality(bool load,const Cancellation* cancellation){
        if(!load)return index_ByQuality.get();return index_ByQuality.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_ByQuality> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_ByQuality(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_ByQuality>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_ByQuality(const std::int32_t& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_ByQuality(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
    struct Key_Group_BySkillId{
        std::int32_t k0;
        Key_Group_BySkillId(const std::int32_t& k0):k0(k0){}
        explicit Key_Group_BySkillId(const Json& row):k0(integer<std::int32_t>(row.field("SkillId"))){}
    };
    static Key_Group_BySkillId cached_key_Group_BySkillId(const SkillLevelData& row){return Key_Group_BySkillId{ row.SkillId() };}
    struct Index_Group_BySkillId:detail::OrdinalIndex<Key_Group_BySkillId>{
        using Base=detail::OrdinalIndex<Key_Group_BySkillId>;
        std::unique_ptr<std::atomic<bool>[]> ready;
        explicit Index_Group_BySkillId(std::vector<Key_Group_BySkillId> keys):Base(std::move(keys),false,[](const Key_Group_BySkillId& a){return (1*31u+Hash<std::int32_t>{}(a.k0));},[](const Key_Group_BySkillId& a,const Key_Group_BySkillId& b){return a.k0==b.k0;}){
            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);
        }
        std::size_t find(const std::int32_t& k0)const{auto slot=mix((1*31u+Hash<std::int32_t>{}(k0)))&mask_;while(slots_[slot]!=missing){const auto& row=keys_[slots_[slot]];if(row.k0==k0)return slot;slot=(slot+1)&mask_;}return missing;}
    };
    detail::DerivedValue<Index_Group_BySkillId> index_BySkillId;
    Index_Group_BySkillId& ensure_BySkillId(bool load,const Cancellation* cancellation){
        if(!load)return index_BySkillId.get();return index_BySkillId.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes(1),[&](){std::vector<Key_Group_BySkillId> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*1;for(std::size_t i=0;i<rows.count();++i){if(auto cached=rows.cached(i))keys.push_back(cached_key_Group_BySkillId(*cached));else keys.emplace_back(rows.indexed().read_transient(i));}return std::make_pair(std::make_unique<Index_Group_BySkillId>(std::move(keys)),retained);});
    }
public:
    const std::vector<std::size_t>& group_BySkillId(const std::int32_t& k0,bool load,const Cancellation* cancellation){
        auto& index=ensure_BySkillId(load,cancellation);auto slot=index.find(k0);const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;
        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){for(auto ordinal:ordinals){if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}index.ready[slot].store(true,std::memory_order_release);}return ordinals;
    }
private:
public:
    explicit SkillLevelStorage(std::unique_ptr<detail::IndexedRows> indexed):rows(std::move(indexed)),key_(keys(rows.indexed())){}
    void load_all(const Cancellation& cancellation){
        cancellation.check();if(full_.load(std::memory_order_acquire))return;for(std::size_t i=0;i<rows.count();++i)rows.load(i,cancellation);
        ensure_ByLevel(true,&cancellation);
        ensure_ByQuality(true,&cancellation);
        ensure_BySkillId(true,&cancellation);
        std::lock_guard<std::mutex> lock(groups_gate_);cancellation.check();if(!full_.load(std::memory_order_acquire)){all_.reserve(rows.count());for(std::size_t i=0;i<rows.count();++i)all_.push_back(i);full_.store(true,std::memory_order_release);}
    }
    bool is_complete()const noexcept{return full_.load(std::memory_order_acquire); }
    const std::vector<std::size_t>& all()const{if(!full_.load(std::memory_order_acquire))throw ConfigError(ErrorCode::TableNotLoaded,"Load the table explicitly");return all_;}
};
class Snapshot:public std::enable_shared_from_this<Snapshot>{
    std::string version_,manifest_digest_,source_namespace_,load_plan_fingerprint_;
    std::shared_ptr<detail::LoadBudget> budget_;
    SelectionPlan selection_;
    LoadMode mode_;

    friend class AllTypesRow;
    friend class AllTypes10kRow;
    friend class DropRow;
    friend class HeroRow;
    friend class ItemCatalogRow;
    friend class QuestRewardsRow;
    friend class ShopOffersRow;
    friend class SkillRow;
    friend class SkillLevelRow;
    std::unique_ptr<AllTypesStorage> table_AllTypes;
    std::unique_ptr<AllTypes10kStorage> table_AllTypes10k;
    std::unique_ptr<DropStorage> table_Drop;
    std::unique_ptr<HeroStorage> table_Hero;
    std::unique_ptr<ItemCatalogStorage> table_ItemCatalog;
    std::unique_ptr<QuestRewardsStorage> table_QuestRewards;
    std::unique_ptr<ShopOffersStorage> table_ShopOffers;
    std::unique_ptr<SkillStorage> table_Skill;
    std::unique_ptr<SkillLevelStorage> table_SkillLevel;
    Snapshot(detail::SourceBundle& bundle,SelectionPlan selection,const LoadSettings& settings,const Cancellation& cancellation):version_(bundle.version()),manifest_digest_(bundle.manifest_digest()),source_namespace_(settings.source_namespace()),budget_(bundle.budget()),selection_(std::move(selection)),mode_(settings.plan().mode){
        const auto& plan=settings.plan();load_plan_fingerprint_=detail::load_plan_fingerprint(plan.mode,plan.prefer_json,selection_.tables);std::unordered_set<std::string> selected(selection_.tables.begin(),selection_.tables.end());
        std::unordered_map<std::string,detail::IndexedRows*> indexed;
        if(selected.count("AllTypes")){cancellation.check();table_AllTypes=std::make_unique<AllTypesStorage>(detail::IndexedRows::open(bundle,AllTypesTable::schema(),plan.prefer_json));indexed.emplace("AllTypes",&table_AllTypes->rows.indexed());}
        if(selected.count("AllTypes10k")){cancellation.check();table_AllTypes10k=std::make_unique<AllTypes10kStorage>(detail::IndexedRows::open(bundle,AllTypes10kTable::schema(),plan.prefer_json));indexed.emplace("AllTypes10k",&table_AllTypes10k->rows.indexed());}
        if(selected.count("Drop")){cancellation.check();table_Drop=std::make_unique<DropStorage>(detail::IndexedRows::open(bundle,DropTable::schema(),plan.prefer_json));indexed.emplace("Drop",&table_Drop->rows.indexed());}
        if(selected.count("Hero")){cancellation.check();table_Hero=std::make_unique<HeroStorage>(detail::IndexedRows::open(bundle,HeroTable::schema(),plan.prefer_json));indexed.emplace("Hero",&table_Hero->rows.indexed());}
        if(selected.count("ItemCatalog")){cancellation.check();table_ItemCatalog=std::make_unique<ItemCatalogStorage>(detail::IndexedRows::open(bundle,ItemCatalogTable::schema(),plan.prefer_json));indexed.emplace("ItemCatalog",&table_ItemCatalog->rows.indexed());}
        if(selected.count("QuestRewards")){cancellation.check();table_QuestRewards=std::make_unique<QuestRewardsStorage>(detail::IndexedRows::open(bundle,QuestRewardsTable::schema(),plan.prefer_json));indexed.emplace("QuestRewards",&table_QuestRewards->rows.indexed());}
        if(selected.count("ShopOffers")){cancellation.check();table_ShopOffers=std::make_unique<ShopOffersStorage>(detail::IndexedRows::open(bundle,ShopOffersTable::schema(),plan.prefer_json));indexed.emplace("ShopOffers",&table_ShopOffers->rows.indexed());}
        if(selected.count("Skill")){cancellation.check();table_Skill=std::make_unique<SkillStorage>(detail::IndexedRows::open(bundle,SkillTable::schema(),plan.prefer_json));indexed.emplace("Skill",&table_Skill->rows.indexed());}
        if(selected.count("SkillLevel")){cancellation.check();table_SkillLevel=std::make_unique<SkillLevelStorage>(detail::IndexedRows::open(bundle,SkillLevelTable::schema(),plan.prefer_json));indexed.emplace("SkillLevel",&table_SkillLevel->rows.indexed());}
        auto directory=std::make_shared<const std::unordered_map<std::string,detail::IndexedRows*>>(indexed);for(const auto& entry:indexed)entry.second->bind_references(directory);
        if(mode_!=LoadMode::LazyRowsOnAccess){
            if(table_AllTypes){auto& input=table_AllTypes->rows.indexed();for(std::size_t i=0;i<input.count();++i){cancellation.check();auto raw=input.read_transient(i);[[maybe_unused]] AllTypesData validated(raw);input.validate_references(raw,[&](const std::string& name)->detail::IndexedRows&{return *indexed.at(name);});}}
            if(table_AllTypes10k){auto& input=table_AllTypes10k->rows.indexed();for(std::size_t i=0;i<input.count();++i){cancellation.check();auto raw=input.read_transient(i);[[maybe_unused]] AllTypes10kData validated(raw);input.validate_references(raw,[&](const std::string& name)->detail::IndexedRows&{return *indexed.at(name);});}}
            if(table_Drop){auto& input=table_Drop->rows.indexed();for(std::size_t i=0;i<input.count();++i){cancellation.check();auto raw=input.read_transient(i);[[maybe_unused]] DropData validated(raw);input.validate_references(raw,[&](const std::string& name)->detail::IndexedRows&{return *indexed.at(name);});}}
            if(table_Hero){auto& input=table_Hero->rows.indexed();for(std::size_t i=0;i<input.count();++i){cancellation.check();auto raw=input.read_transient(i);[[maybe_unused]] HeroData validated(raw);input.validate_references(raw,[&](const std::string& name)->detail::IndexedRows&{return *indexed.at(name);});}}
            if(table_ItemCatalog){auto& input=table_ItemCatalog->rows.indexed();for(std::size_t i=0;i<input.count();++i){cancellation.check();auto raw=input.read_transient(i);[[maybe_unused]] ItemCatalogData validated(raw);input.validate_references(raw,[&](const std::string& name)->detail::IndexedRows&{return *indexed.at(name);});}}
            if(table_QuestRewards){auto& input=table_QuestRewards->rows.indexed();for(std::size_t i=0;i<input.count();++i){cancellation.check();auto raw=input.read_transient(i);[[maybe_unused]] QuestRewardsData validated(raw);input.validate_references(raw,[&](const std::string& name)->detail::IndexedRows&{return *indexed.at(name);});}}
            if(table_ShopOffers){auto& input=table_ShopOffers->rows.indexed();for(std::size_t i=0;i<input.count();++i){cancellation.check();auto raw=input.read_transient(i);[[maybe_unused]] ShopOffersData validated(raw);input.validate_references(raw,[&](const std::string& name)->detail::IndexedRows&{return *indexed.at(name);});}}
            if(table_Skill){auto& input=table_Skill->rows.indexed();for(std::size_t i=0;i<input.count();++i){cancellation.check();auto raw=input.read_transient(i);[[maybe_unused]] SkillData validated(raw);input.validate_references(raw,[&](const std::string& name)->detail::IndexedRows&{return *indexed.at(name);});}}
            if(table_SkillLevel){auto& input=table_SkillLevel->rows.indexed();for(std::size_t i=0;i<input.count();++i){cancellation.check();auto raw=input.read_transient(i);[[maybe_unused]] SkillLevelData validated(raw);input.validate_references(raw,[&](const std::string& name)->detail::IndexedRows&{return *indexed.at(name);});}}
        }
        if(mode_==LoadMode::ServerDefault||mode_==LoadMode::SelectedTables)load_all(cancellation);
        for(const auto& name:plan.warmup_tables){(void)name;
            if(name=="AllTypes")table_AllTypes->load_all(cancellation);
            if(name=="AllTypes10k")table_AllTypes10k->load_all(cancellation);
            if(name=="Drop")table_Drop->load_all(cancellation);
            if(name=="Hero")table_Hero->load_all(cancellation);
            if(name=="ItemCatalog")table_ItemCatalog->load_all(cancellation);
            if(name=="QuestRewards")table_QuestRewards->load_all(cancellation);
            if(name=="ShopOffers")table_ShopOffers->load_all(cancellation);
            if(name=="Skill")table_Skill->load_all(cancellation);
            if(name=="SkillLevel")table_SkillLevel->load_all(cancellation);
        }
        cancellation.check();
    }
public:
    const std::string& version()const noexcept{return version_;}
    const SelectionPlan& selection()const noexcept{return selection_;}
    LoadMode mode()const noexcept{return mode_;}
    const std::string& manifest_digest()const noexcept{return manifest_digest_;}
    const std::string& source_namespace()const noexcept{return source_namespace_;}
    const std::string& catalog_fingerprint()const noexcept{return selection_.catalog_fingerprint;}
    const std::string& load_plan_fingerprint()const noexcept{return load_plan_fingerprint_;}
    SnapshotDiagnostics diagnostics()const{return budget_->diagnostics();}
    static std::shared_ptr<const Snapshot> load(const std::filesystem::path& root,const LoadPlan& plan,const Cancellation& cancellation){FileConfigSource source(root);LoadSettings settings(plan);settings.with_source_namespace(source.source_namespace());return load(source,settings,cancellation);}
    static std::shared_ptr<const Snapshot> load(ConfigSource& source,const LoadSettings& settings,const Cancellation& cancellation){
        const auto& plan=settings.plan();
        switch(plan.mode){case LoadMode::ServerDefault:case LoadMode::SelectedTables:case LoadMode::LazyRowsStrict:case LoadMode::LazyRowsOnAccess:break;default:throw ConfigError(ErrorCode::InvalidPlan,"Unknown load mode");}
        std::vector<std::string> names;if(plan.tables)names=*plan.tables;else for(const auto& table:server::Snapshot::catalog().tables())names.push_back(table.name);auto selection=server::Snapshot::catalog().select(names);
        for(const auto& name:plan.warmup_tables)if(std::find(selection.tables.begin(),selection.tables.end(),name)==selection.tables.end())throw ConfigError(ErrorCode::InvalidPlan,"Warmup outside selection");
        cancellation.check();try{detail::SourceBundle bundle(source,settings,cancellation);server::Snapshot::catalog().validate(bundle.metadata());return std::shared_ptr<const Snapshot>(new Snapshot(bundle,std::move(selection),settings,cancellation));}catch(const detail::InvalidData& error){throw ConfigError(ErrorCode::InvalidValue,error.what(),"prepare");}
    }
    void load_all(const Cancellation& cancellation)const{
        cancellation.check();
        if(table_AllTypes)table_AllTypes->load_all(cancellation);
        if(table_AllTypes10k)table_AllTypes10k->load_all(cancellation);
        if(table_Drop)table_Drop->load_all(cancellation);
        if(table_Hero)table_Hero->load_all(cancellation);
        if(table_ItemCatalog)table_ItemCatalog->load_all(cancellation);
        if(table_QuestRewards)table_QuestRewards->load_all(cancellation);
        if(table_ShopOffers)table_ShopOffers->load_all(cancellation);
        if(table_Skill)table_Skill->load_all(cancellation);
        if(table_SkillLevel)table_SkillLevel->load_all(cancellation);
    }
    std::size_t loaded_row_count()const noexcept{return (table_AllTypes?table_AllTypes->rows.loaded_count():0)+(table_AllTypes10k?table_AllTypes10k->rows.loaded_count():0)+(table_Drop?table_Drop->rows.loaded_count():0)+(table_Hero?table_Hero->rows.loaded_count():0)+(table_ItemCatalog?table_ItemCatalog->rows.loaded_count():0)+(table_QuestRewards?table_QuestRewards->rows.loaded_count():0)+(table_ShopOffers?table_ShopOffers->rows.loaded_count():0)+(table_Skill?table_Skill->rows.loaded_count():0)+(table_SkillLevel?table_SkillLevel->rows.loaded_count():0);}
    std::size_t completed_table_count()const noexcept{return (table_AllTypes && table_AllTypes->is_complete()?1:0)+(table_AllTypes10k && table_AllTypes10k->is_complete()?1:0)+(table_Drop && table_Drop->is_complete()?1:0)+(table_Hero && table_Hero->is_complete()?1:0)+(table_ItemCatalog && table_ItemCatalog->is_complete()?1:0)+(table_QuestRewards && table_QuestRewards->is_complete()?1:0)+(table_ShopOffers && table_ShopOffers->is_complete()?1:0)+(table_Skill && table_Skill->is_complete()?1:0)+(table_SkillLevel && table_SkillLevel->is_complete()?1:0);}
    class Warmup {
        friend class Snapshot;std::string table_;std::function<void(const std::shared_ptr<const Snapshot>&,const Cancellation&)> action_;
        Warmup(std::string table,std::function<void(const std::shared_ptr<const Snapshot>&,const Cancellation&)> action):table_(std::move(table)),action_(std::move(action)){}
    public:const std::string& table()const noexcept{return table_;}
    };
    static std::shared_ptr<const Snapshot> load_with_warmup(ConfigSource& source,const LoadSettings& settings,const std::vector<Warmup>& requests,const Cancellation& cancellation){
        auto steps=requests;std::vector<std::string> names;if(settings.plan().tables)names=*settings.plan().tables;else for(const auto& table:server::Snapshot::catalog().tables())names.push_back(table.name);auto selection=server::Snapshot::catalog().select(names);
        for(const auto& step:steps)if(std::find(selection.tables.begin(),selection.tables.end(),step.table_)==selection.tables.end())throw ConfigError(ErrorCode::InvalidPlan,"Warmup outside selection");
        auto next=load(source,settings,cancellation);for(const auto& step:steps){cancellation.check();step.action_(next,cancellation);}cancellation.check();return next;
    }
    static std::shared_ptr<const Snapshot> load_with_warmup(const std::filesystem::path& root,const LoadPlan& plan,const std::vector<Warmup>& requests,const Cancellation& cancellation){FileConfigSource source(root);LoadSettings settings(plan);settings.with_source_namespace(source.source_namespace());return load_with_warmup(source,settings,requests,cancellation);}
    static Warmup warmup_table_AllTypes(){return Warmup("AllTypes",[](const auto& snapshot,const auto& token){snapshot->load_table_AllTypes(token);});}
    static Warmup warmup_row_AllTypes(std::int32_t k0){return Warmup("AllTypes",[k0](const auto& snapshot,const auto& token){snapshot->load_AllTypes(k0,token);});}
    static Warmup warmup_group_AllTypes_ById(std::int32_t k0){return Warmup("AllTypes",[k0](const auto& snapshot,const auto& token){snapshot->load_group_AllTypes_ById(k0,token);});}
    static Warmup warmup_group_AllTypes_ByQuality(T_Game_Config_Quality k0){return Warmup("AllTypes",[k0](const auto& snapshot,const auto& token){snapshot->load_group_AllTypes_ByQuality(k0,token);});}
    static Warmup warmup_table_AllTypes10k(){return Warmup("AllTypes10k",[](const auto& snapshot,const auto& token){snapshot->load_table_AllTypes10k(token);});}
    static Warmup warmup_row_AllTypes10k(std::int32_t k0){return Warmup("AllTypes10k",[k0](const auto& snapshot,const auto& token){snapshot->load_AllTypes10k(k0,token);});}
    static Warmup warmup_group_AllTypes10k_ById(std::int32_t k0){return Warmup("AllTypes10k",[k0](const auto& snapshot,const auto& token){snapshot->load_group_AllTypes10k_ById(k0,token);});}
    static Warmup warmup_table_Drop(){return Warmup("Drop",[](const auto& snapshot,const auto& token){snapshot->load_table_Drop(token);});}
    static Warmup warmup_row_Drop(std::int32_t k0,std::int32_t k1){return Warmup("Drop",[k0,k1](const auto& snapshot,const auto& token){snapshot->load_Drop(k0,k1,token);});}
    static Warmup warmup_group_Drop_ByGroupId(std::int32_t k0){return Warmup("Drop",[k0](const auto& snapshot,const auto& token){snapshot->load_group_Drop_ByGroupId(k0,token);});}
    static Warmup warmup_group_Drop_ById(std::int32_t k0){return Warmup("Drop",[k0](const auto& snapshot,const auto& token){snapshot->load_group_Drop_ById(k0,token);});}
    static Warmup warmup_table_Hero(){return Warmup("Hero",[](const auto& snapshot,const auto& token){snapshot->load_table_Hero(token);});}
    static Warmup warmup_row_Hero(std::int32_t k0){return Warmup("Hero",[k0](const auto& snapshot,const auto& token){snapshot->load_Hero(k0,token);});}
    static Warmup warmup_group_Hero_ById(std::int32_t k0){return Warmup("Hero",[k0](const auto& snapshot,const auto& token){snapshot->load_group_Hero_ById(k0,token);});}
    static Warmup warmup_table_ItemCatalog(){return Warmup("ItemCatalog",[](const auto& snapshot,const auto& token){snapshot->load_table_ItemCatalog(token);});}
    static Warmup warmup_row_ItemCatalog(std::int32_t k0){return Warmup("ItemCatalog",[k0](const auto& snapshot,const auto& token){snapshot->load_ItemCatalog(k0,token);});}
    static Warmup warmup_group_ItemCatalog_ById(std::int32_t k0){return Warmup("ItemCatalog",[k0](const auto& snapshot,const auto& token){snapshot->load_group_ItemCatalog_ById(k0,token);});}
    static Warmup warmup_table_QuestRewards(){return Warmup("QuestRewards",[](const auto& snapshot,const auto& token){snapshot->load_table_QuestRewards(token);});}
    static Warmup warmup_row_QuestRewards(std::int32_t k0){return Warmup("QuestRewards",[k0](const auto& snapshot,const auto& token){snapshot->load_QuestRewards(k0,token);});}
    static Warmup warmup_group_QuestRewards_ById(std::int32_t k0){return Warmup("QuestRewards",[k0](const auto& snapshot,const auto& token){snapshot->load_group_QuestRewards_ById(k0,token);});}
    static Warmup warmup_table_ShopOffers(){return Warmup("ShopOffers",[](const auto& snapshot,const auto& token){snapshot->load_table_ShopOffers(token);});}
    static Warmup warmup_row_ShopOffers(std::int32_t k0){return Warmup("ShopOffers",[k0](const auto& snapshot,const auto& token){snapshot->load_ShopOffers(k0,token);});}
    static Warmup warmup_group_ShopOffers_ById(std::int32_t k0){return Warmup("ShopOffers",[k0](const auto& snapshot,const auto& token){snapshot->load_group_ShopOffers_ById(k0,token);});}
    static Warmup warmup_table_Skill(){return Warmup("Skill",[](const auto& snapshot,const auto& token){snapshot->load_table_Skill(token);});}
    static Warmup warmup_row_Skill(std::int32_t k0){return Warmup("Skill",[k0](const auto& snapshot,const auto& token){snapshot->load_Skill(k0,token);});}
    static Warmup warmup_group_Skill_ById(std::int32_t k0){return Warmup("Skill",[k0](const auto& snapshot,const auto& token){snapshot->load_group_Skill_ById(k0,token);});}
    static Warmup warmup_table_SkillLevel(){return Warmup("SkillLevel",[](const auto& snapshot,const auto& token){snapshot->load_table_SkillLevel(token);});}
    static Warmup warmup_row_SkillLevel(std::int32_t k0,std::int32_t k1,std::int32_t k2){return Warmup("SkillLevel",[k0,k1,k2](const auto& snapshot,const auto& token){snapshot->load_SkillLevel(k0,k1,k2,token);});}
    static Warmup warmup_group_SkillLevel_ByLevel(std::int32_t k0){return Warmup("SkillLevel",[k0](const auto& snapshot,const auto& token){snapshot->load_group_SkillLevel_ByLevel(k0,token);});}
    static Warmup warmup_group_SkillLevel_ByQuality(std::int32_t k0){return Warmup("SkillLevel",[k0](const auto& snapshot,const auto& token){snapshot->load_group_SkillLevel_ByQuality(k0,token);});}
    static Warmup warmup_group_SkillLevel_BySkillId(std::int32_t k0){return Warmup("SkillLevel",[k0](const auto& snapshot,const auto& token){snapshot->load_group_SkillLevel_BySkillId(k0,token);});}
    std::optional<AllTypesRow> find_AllTypes(const std::int32_t& k0)const{if(!table_AllTypes)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes");auto index=table_AllTypes->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return AllTypesRow(shared_from_this(),table_AllTypes->rows.get(index));}
    AllTypesRow get_AllTypes(const std::int32_t& k0)const{auto row=find_AllTypes(k0);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
    std::optional<AllTypesRow> try_load_AllTypes(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_AllTypes)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes");cancellation.check();auto index=table_AllTypes->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return AllTypesRow(shared_from_this(),table_AllTypes->rows.load(index,cancellation));}
    AllTypesRow load_AllTypes(const std::int32_t& k0,const Cancellation& cancellation)const{auto row=try_load_AllTypes(k0,cancellation);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
private:
    std::optional<AllTypesRow> reference_AllTypes(std::int32_t id,bool load,const Cancellation* cancellation=nullptr)const{if(!id)return std::nullopt;if(!table_AllTypes)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes");auto index=table_AllTypes->rows.indexed().reference(id);return AllTypesRow(shared_from_this(),load?table_AllTypes->rows.load(index,*cancellation):table_AllTypes->rows.get(index));}
    static AllTypesRow ordinal_AllTypes(const std::shared_ptr<const Snapshot>& owner,std::size_t index){return AllTypesRow(owner,owner->table_AllTypes->rows.get(index));}
public:
    DataGroup<Snapshot,AllTypesRow> rows_AllTypes()const{if(!table_AllTypes)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes");return {shared_from_this(),table_AllTypes->all(),&ordinal_AllTypes};}
    DataGroup<Snapshot,AllTypesRow> load_table_AllTypes(const Cancellation& cancellation)const{if(!table_AllTypes)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes");table_AllTypes->load_all(cancellation);return rows_AllTypes();}
    DataGroup<Snapshot,AllTypesRow> find_group_AllTypes_ById(const std::int32_t& k0)const{if(!table_AllTypes)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes");return {shared_from_this(),table_AllTypes->group_ById(k0,false,nullptr),&ordinal_AllTypes};}
    DataGroup<Snapshot,AllTypesRow> load_group_AllTypes_ById(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_AllTypes)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes");cancellation.check();return {shared_from_this(),table_AllTypes->group_ById(k0,true,&cancellation),&ordinal_AllTypes};}
    DataGroup<Snapshot,AllTypesRow> find_group_AllTypes_ByQuality(const T_Game_Config_Quality& k0)const{if(!table_AllTypes)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes");return {shared_from_this(),table_AllTypes->group_ByQuality(k0,false,nullptr),&ordinal_AllTypes};}
    DataGroup<Snapshot,AllTypesRow> load_group_AllTypes_ByQuality(const T_Game_Config_Quality& k0,const Cancellation& cancellation)const{if(!table_AllTypes)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes");cancellation.check();return {shared_from_this(),table_AllTypes->group_ByQuality(k0,true,&cancellation),&ordinal_AllTypes};}
    std::optional<AllTypes10kRow> find_AllTypes10k(const std::int32_t& k0)const{if(!table_AllTypes10k)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes10k");auto index=table_AllTypes10k->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return AllTypes10kRow(shared_from_this(),table_AllTypes10k->rows.get(index));}
    AllTypes10kRow get_AllTypes10k(const std::int32_t& k0)const{auto row=find_AllTypes10k(k0);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
    std::optional<AllTypes10kRow> try_load_AllTypes10k(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_AllTypes10k)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes10k");cancellation.check();auto index=table_AllTypes10k->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return AllTypes10kRow(shared_from_this(),table_AllTypes10k->rows.load(index,cancellation));}
    AllTypes10kRow load_AllTypes10k(const std::int32_t& k0,const Cancellation& cancellation)const{auto row=try_load_AllTypes10k(k0,cancellation);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
private:
    std::optional<AllTypes10kRow> reference_AllTypes10k(std::int32_t id,bool load,const Cancellation* cancellation=nullptr)const{if(!id)return std::nullopt;if(!table_AllTypes10k)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes10k");auto index=table_AllTypes10k->rows.indexed().reference(id);return AllTypes10kRow(shared_from_this(),load?table_AllTypes10k->rows.load(index,*cancellation):table_AllTypes10k->rows.get(index));}
    static AllTypes10kRow ordinal_AllTypes10k(const std::shared_ptr<const Snapshot>& owner,std::size_t index){return AllTypes10kRow(owner,owner->table_AllTypes10k->rows.get(index));}
public:
    DataGroup<Snapshot,AllTypes10kRow> rows_AllTypes10k()const{if(!table_AllTypes10k)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes10k");return {shared_from_this(),table_AllTypes10k->all(),&ordinal_AllTypes10k};}
    DataGroup<Snapshot,AllTypes10kRow> load_table_AllTypes10k(const Cancellation& cancellation)const{if(!table_AllTypes10k)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes10k");table_AllTypes10k->load_all(cancellation);return rows_AllTypes10k();}
    DataGroup<Snapshot,AllTypes10kRow> find_group_AllTypes10k_ById(const std::int32_t& k0)const{if(!table_AllTypes10k)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes10k");return {shared_from_this(),table_AllTypes10k->group_ById(k0,false,nullptr),&ordinal_AllTypes10k};}
    DataGroup<Snapshot,AllTypes10kRow> load_group_AllTypes10k_ById(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_AllTypes10k)throw ConfigError(ErrorCode::TableNotSelected,"AllTypes10k");cancellation.check();return {shared_from_this(),table_AllTypes10k->group_ById(k0,true,&cancellation),&ordinal_AllTypes10k};}
    std::optional<DropRow> find_Drop(const std::int32_t& k0,const std::int32_t& k1)const{if(!table_Drop)throw ConfigError(ErrorCode::TableNotSelected,"Drop");auto index=table_Drop->ordinal(k0,k1);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return DropRow(shared_from_this(),table_Drop->rows.get(index));}
    DropRow get_Drop(const std::int32_t& k0,const std::int32_t& k1)const{auto row=find_Drop(k0,k1);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
    std::optional<DropRow> try_load_Drop(const std::int32_t& k0,const std::int32_t& k1,const Cancellation& cancellation)const{if(!table_Drop)throw ConfigError(ErrorCode::TableNotSelected,"Drop");cancellation.check();auto index=table_Drop->ordinal(k0,k1);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return DropRow(shared_from_this(),table_Drop->rows.load(index,cancellation));}
    DropRow load_Drop(const std::int32_t& k0,const std::int32_t& k1,const Cancellation& cancellation)const{auto row=try_load_Drop(k0,k1,cancellation);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
private:
    std::optional<DropRow> reference_Drop(std::int32_t id,bool load,const Cancellation* cancellation=nullptr)const{if(!id)return std::nullopt;if(!table_Drop)throw ConfigError(ErrorCode::TableNotSelected,"Drop");auto index=table_Drop->rows.indexed().reference(id);return DropRow(shared_from_this(),load?table_Drop->rows.load(index,*cancellation):table_Drop->rows.get(index));}
    static DropRow ordinal_Drop(const std::shared_ptr<const Snapshot>& owner,std::size_t index){return DropRow(owner,owner->table_Drop->rows.get(index));}
public:
    DataGroup<Snapshot,DropRow> rows_Drop()const{if(!table_Drop)throw ConfigError(ErrorCode::TableNotSelected,"Drop");return {shared_from_this(),table_Drop->all(),&ordinal_Drop};}
    DataGroup<Snapshot,DropRow> load_table_Drop(const Cancellation& cancellation)const{if(!table_Drop)throw ConfigError(ErrorCode::TableNotSelected,"Drop");table_Drop->load_all(cancellation);return rows_Drop();}
    DataGroup<Snapshot,DropRow> find_group_Drop_ByGroupId(const std::int32_t& k0)const{if(!table_Drop)throw ConfigError(ErrorCode::TableNotSelected,"Drop");return {shared_from_this(),table_Drop->group_ByGroupId(k0,false,nullptr),&ordinal_Drop};}
    DataGroup<Snapshot,DropRow> load_group_Drop_ByGroupId(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_Drop)throw ConfigError(ErrorCode::TableNotSelected,"Drop");cancellation.check();return {shared_from_this(),table_Drop->group_ByGroupId(k0,true,&cancellation),&ordinal_Drop};}
    DataGroup<Snapshot,DropRow> find_group_Drop_ById(const std::int32_t& k0)const{if(!table_Drop)throw ConfigError(ErrorCode::TableNotSelected,"Drop");return {shared_from_this(),table_Drop->group_ById(k0,false,nullptr),&ordinal_Drop};}
    DataGroup<Snapshot,DropRow> load_group_Drop_ById(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_Drop)throw ConfigError(ErrorCode::TableNotSelected,"Drop");cancellation.check();return {shared_from_this(),table_Drop->group_ById(k0,true,&cancellation),&ordinal_Drop};}
    std::optional<HeroRow> find_Hero(const std::int32_t& k0)const{if(!table_Hero)throw ConfigError(ErrorCode::TableNotSelected,"Hero");auto index=table_Hero->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return HeroRow(shared_from_this(),table_Hero->rows.get(index));}
    HeroRow get_Hero(const std::int32_t& k0)const{auto row=find_Hero(k0);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
    std::optional<HeroRow> try_load_Hero(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_Hero)throw ConfigError(ErrorCode::TableNotSelected,"Hero");cancellation.check();auto index=table_Hero->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return HeroRow(shared_from_this(),table_Hero->rows.load(index,cancellation));}
    HeroRow load_Hero(const std::int32_t& k0,const Cancellation& cancellation)const{auto row=try_load_Hero(k0,cancellation);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
private:
    std::optional<HeroRow> reference_Hero(std::int32_t id,bool load,const Cancellation* cancellation=nullptr)const{if(!id)return std::nullopt;if(!table_Hero)throw ConfigError(ErrorCode::TableNotSelected,"Hero");auto index=table_Hero->rows.indexed().reference(id);return HeroRow(shared_from_this(),load?table_Hero->rows.load(index,*cancellation):table_Hero->rows.get(index));}
    static HeroRow ordinal_Hero(const std::shared_ptr<const Snapshot>& owner,std::size_t index){return HeroRow(owner,owner->table_Hero->rows.get(index));}
public:
    DataGroup<Snapshot,HeroRow> rows_Hero()const{if(!table_Hero)throw ConfigError(ErrorCode::TableNotSelected,"Hero");return {shared_from_this(),table_Hero->all(),&ordinal_Hero};}
    DataGroup<Snapshot,HeroRow> load_table_Hero(const Cancellation& cancellation)const{if(!table_Hero)throw ConfigError(ErrorCode::TableNotSelected,"Hero");table_Hero->load_all(cancellation);return rows_Hero();}
    DataGroup<Snapshot,HeroRow> find_group_Hero_ById(const std::int32_t& k0)const{if(!table_Hero)throw ConfigError(ErrorCode::TableNotSelected,"Hero");return {shared_from_this(),table_Hero->group_ById(k0,false,nullptr),&ordinal_Hero};}
    DataGroup<Snapshot,HeroRow> load_group_Hero_ById(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_Hero)throw ConfigError(ErrorCode::TableNotSelected,"Hero");cancellation.check();return {shared_from_this(),table_Hero->group_ById(k0,true,&cancellation),&ordinal_Hero};}
    std::optional<ItemCatalogRow> find_ItemCatalog(const std::int32_t& k0)const{if(!table_ItemCatalog)throw ConfigError(ErrorCode::TableNotSelected,"ItemCatalog");auto index=table_ItemCatalog->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return ItemCatalogRow(shared_from_this(),table_ItemCatalog->rows.get(index));}
    ItemCatalogRow get_ItemCatalog(const std::int32_t& k0)const{auto row=find_ItemCatalog(k0);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
    std::optional<ItemCatalogRow> try_load_ItemCatalog(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_ItemCatalog)throw ConfigError(ErrorCode::TableNotSelected,"ItemCatalog");cancellation.check();auto index=table_ItemCatalog->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return ItemCatalogRow(shared_from_this(),table_ItemCatalog->rows.load(index,cancellation));}
    ItemCatalogRow load_ItemCatalog(const std::int32_t& k0,const Cancellation& cancellation)const{auto row=try_load_ItemCatalog(k0,cancellation);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
private:
    std::optional<ItemCatalogRow> reference_ItemCatalog(std::int32_t id,bool load,const Cancellation* cancellation=nullptr)const{if(!id)return std::nullopt;if(!table_ItemCatalog)throw ConfigError(ErrorCode::TableNotSelected,"ItemCatalog");auto index=table_ItemCatalog->rows.indexed().reference(id);return ItemCatalogRow(shared_from_this(),load?table_ItemCatalog->rows.load(index,*cancellation):table_ItemCatalog->rows.get(index));}
    static ItemCatalogRow ordinal_ItemCatalog(const std::shared_ptr<const Snapshot>& owner,std::size_t index){return ItemCatalogRow(owner,owner->table_ItemCatalog->rows.get(index));}
public:
    DataGroup<Snapshot,ItemCatalogRow> rows_ItemCatalog()const{if(!table_ItemCatalog)throw ConfigError(ErrorCode::TableNotSelected,"ItemCatalog");return {shared_from_this(),table_ItemCatalog->all(),&ordinal_ItemCatalog};}
    DataGroup<Snapshot,ItemCatalogRow> load_table_ItemCatalog(const Cancellation& cancellation)const{if(!table_ItemCatalog)throw ConfigError(ErrorCode::TableNotSelected,"ItemCatalog");table_ItemCatalog->load_all(cancellation);return rows_ItemCatalog();}
    DataGroup<Snapshot,ItemCatalogRow> find_group_ItemCatalog_ById(const std::int32_t& k0)const{if(!table_ItemCatalog)throw ConfigError(ErrorCode::TableNotSelected,"ItemCatalog");return {shared_from_this(),table_ItemCatalog->group_ById(k0,false,nullptr),&ordinal_ItemCatalog};}
    DataGroup<Snapshot,ItemCatalogRow> load_group_ItemCatalog_ById(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_ItemCatalog)throw ConfigError(ErrorCode::TableNotSelected,"ItemCatalog");cancellation.check();return {shared_from_this(),table_ItemCatalog->group_ById(k0,true,&cancellation),&ordinal_ItemCatalog};}
    std::optional<QuestRewardsRow> find_QuestRewards(const std::int32_t& k0)const{if(!table_QuestRewards)throw ConfigError(ErrorCode::TableNotSelected,"QuestRewards");auto index=table_QuestRewards->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return QuestRewardsRow(shared_from_this(),table_QuestRewards->rows.get(index));}
    QuestRewardsRow get_QuestRewards(const std::int32_t& k0)const{auto row=find_QuestRewards(k0);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
    std::optional<QuestRewardsRow> try_load_QuestRewards(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_QuestRewards)throw ConfigError(ErrorCode::TableNotSelected,"QuestRewards");cancellation.check();auto index=table_QuestRewards->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return QuestRewardsRow(shared_from_this(),table_QuestRewards->rows.load(index,cancellation));}
    QuestRewardsRow load_QuestRewards(const std::int32_t& k0,const Cancellation& cancellation)const{auto row=try_load_QuestRewards(k0,cancellation);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
private:
    std::optional<QuestRewardsRow> reference_QuestRewards(std::int32_t id,bool load,const Cancellation* cancellation=nullptr)const{if(!id)return std::nullopt;if(!table_QuestRewards)throw ConfigError(ErrorCode::TableNotSelected,"QuestRewards");auto index=table_QuestRewards->rows.indexed().reference(id);return QuestRewardsRow(shared_from_this(),load?table_QuestRewards->rows.load(index,*cancellation):table_QuestRewards->rows.get(index));}
    static QuestRewardsRow ordinal_QuestRewards(const std::shared_ptr<const Snapshot>& owner,std::size_t index){return QuestRewardsRow(owner,owner->table_QuestRewards->rows.get(index));}
public:
    DataGroup<Snapshot,QuestRewardsRow> rows_QuestRewards()const{if(!table_QuestRewards)throw ConfigError(ErrorCode::TableNotSelected,"QuestRewards");return {shared_from_this(),table_QuestRewards->all(),&ordinal_QuestRewards};}
    DataGroup<Snapshot,QuestRewardsRow> load_table_QuestRewards(const Cancellation& cancellation)const{if(!table_QuestRewards)throw ConfigError(ErrorCode::TableNotSelected,"QuestRewards");table_QuestRewards->load_all(cancellation);return rows_QuestRewards();}
    DataGroup<Snapshot,QuestRewardsRow> find_group_QuestRewards_ById(const std::int32_t& k0)const{if(!table_QuestRewards)throw ConfigError(ErrorCode::TableNotSelected,"QuestRewards");return {shared_from_this(),table_QuestRewards->group_ById(k0,false,nullptr),&ordinal_QuestRewards};}
    DataGroup<Snapshot,QuestRewardsRow> load_group_QuestRewards_ById(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_QuestRewards)throw ConfigError(ErrorCode::TableNotSelected,"QuestRewards");cancellation.check();return {shared_from_this(),table_QuestRewards->group_ById(k0,true,&cancellation),&ordinal_QuestRewards};}
    std::optional<ShopOffersRow> find_ShopOffers(const std::int32_t& k0)const{if(!table_ShopOffers)throw ConfigError(ErrorCode::TableNotSelected,"ShopOffers");auto index=table_ShopOffers->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return ShopOffersRow(shared_from_this(),table_ShopOffers->rows.get(index));}
    ShopOffersRow get_ShopOffers(const std::int32_t& k0)const{auto row=find_ShopOffers(k0);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
    std::optional<ShopOffersRow> try_load_ShopOffers(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_ShopOffers)throw ConfigError(ErrorCode::TableNotSelected,"ShopOffers");cancellation.check();auto index=table_ShopOffers->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return ShopOffersRow(shared_from_this(),table_ShopOffers->rows.load(index,cancellation));}
    ShopOffersRow load_ShopOffers(const std::int32_t& k0,const Cancellation& cancellation)const{auto row=try_load_ShopOffers(k0,cancellation);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
private:
    std::optional<ShopOffersRow> reference_ShopOffers(std::int32_t id,bool load,const Cancellation* cancellation=nullptr)const{if(!id)return std::nullopt;if(!table_ShopOffers)throw ConfigError(ErrorCode::TableNotSelected,"ShopOffers");auto index=table_ShopOffers->rows.indexed().reference(id);return ShopOffersRow(shared_from_this(),load?table_ShopOffers->rows.load(index,*cancellation):table_ShopOffers->rows.get(index));}
    static ShopOffersRow ordinal_ShopOffers(const std::shared_ptr<const Snapshot>& owner,std::size_t index){return ShopOffersRow(owner,owner->table_ShopOffers->rows.get(index));}
public:
    DataGroup<Snapshot,ShopOffersRow> rows_ShopOffers()const{if(!table_ShopOffers)throw ConfigError(ErrorCode::TableNotSelected,"ShopOffers");return {shared_from_this(),table_ShopOffers->all(),&ordinal_ShopOffers};}
    DataGroup<Snapshot,ShopOffersRow> load_table_ShopOffers(const Cancellation& cancellation)const{if(!table_ShopOffers)throw ConfigError(ErrorCode::TableNotSelected,"ShopOffers");table_ShopOffers->load_all(cancellation);return rows_ShopOffers();}
    DataGroup<Snapshot,ShopOffersRow> find_group_ShopOffers_ById(const std::int32_t& k0)const{if(!table_ShopOffers)throw ConfigError(ErrorCode::TableNotSelected,"ShopOffers");return {shared_from_this(),table_ShopOffers->group_ById(k0,false,nullptr),&ordinal_ShopOffers};}
    DataGroup<Snapshot,ShopOffersRow> load_group_ShopOffers_ById(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_ShopOffers)throw ConfigError(ErrorCode::TableNotSelected,"ShopOffers");cancellation.check();return {shared_from_this(),table_ShopOffers->group_ById(k0,true,&cancellation),&ordinal_ShopOffers};}
    std::optional<SkillRow> find_Skill(const std::int32_t& k0)const{if(!table_Skill)throw ConfigError(ErrorCode::TableNotSelected,"Skill");auto index=table_Skill->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return SkillRow(shared_from_this(),table_Skill->rows.get(index));}
    SkillRow get_Skill(const std::int32_t& k0)const{auto row=find_Skill(k0);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
    std::optional<SkillRow> try_load_Skill(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_Skill)throw ConfigError(ErrorCode::TableNotSelected,"Skill");cancellation.check();auto index=table_Skill->ordinal(k0);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return SkillRow(shared_from_this(),table_Skill->rows.load(index,cancellation));}
    SkillRow load_Skill(const std::int32_t& k0,const Cancellation& cancellation)const{auto row=try_load_Skill(k0,cancellation);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
private:
    std::optional<SkillRow> reference_Skill(std::int32_t id,bool load,const Cancellation* cancellation=nullptr)const{if(!id)return std::nullopt;if(!table_Skill)throw ConfigError(ErrorCode::TableNotSelected,"Skill");auto index=table_Skill->rows.indexed().reference(id);return SkillRow(shared_from_this(),load?table_Skill->rows.load(index,*cancellation):table_Skill->rows.get(index));}
    static SkillRow ordinal_Skill(const std::shared_ptr<const Snapshot>& owner,std::size_t index){return SkillRow(owner,owner->table_Skill->rows.get(index));}
public:
    DataGroup<Snapshot,SkillRow> rows_Skill()const{if(!table_Skill)throw ConfigError(ErrorCode::TableNotSelected,"Skill");return {shared_from_this(),table_Skill->all(),&ordinal_Skill};}
    DataGroup<Snapshot,SkillRow> load_table_Skill(const Cancellation& cancellation)const{if(!table_Skill)throw ConfigError(ErrorCode::TableNotSelected,"Skill");table_Skill->load_all(cancellation);return rows_Skill();}
    DataGroup<Snapshot,SkillRow> find_group_Skill_ById(const std::int32_t& k0)const{if(!table_Skill)throw ConfigError(ErrorCode::TableNotSelected,"Skill");return {shared_from_this(),table_Skill->group_ById(k0,false,nullptr),&ordinal_Skill};}
    DataGroup<Snapshot,SkillRow> load_group_Skill_ById(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_Skill)throw ConfigError(ErrorCode::TableNotSelected,"Skill");cancellation.check();return {shared_from_this(),table_Skill->group_ById(k0,true,&cancellation),&ordinal_Skill};}
    std::optional<SkillLevelRow> find_SkillLevel(const std::int32_t& k0,const std::int32_t& k1,const std::int32_t& k2)const{if(!table_SkillLevel)throw ConfigError(ErrorCode::TableNotSelected,"SkillLevel");auto index=table_SkillLevel->ordinal(k0,k1,k2);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return SkillLevelRow(shared_from_this(),table_SkillLevel->rows.get(index));}
    SkillLevelRow get_SkillLevel(const std::int32_t& k0,const std::int32_t& k1,const std::int32_t& k2)const{auto row=find_SkillLevel(k0,k1,k2);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
    std::optional<SkillLevelRow> try_load_SkillLevel(const std::int32_t& k0,const std::int32_t& k1,const std::int32_t& k2,const Cancellation& cancellation)const{if(!table_SkillLevel)throw ConfigError(ErrorCode::TableNotSelected,"SkillLevel");cancellation.check();auto index=table_SkillLevel->ordinal(k0,k1,k2);if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return SkillLevelRow(shared_from_this(),table_SkillLevel->rows.load(index,cancellation));}
    SkillLevelRow load_SkillLevel(const std::int32_t& k0,const std::int32_t& k1,const std::int32_t& k2,const Cancellation& cancellation)const{auto row=try_load_SkillLevel(k0,k1,k2,cancellation);if(!row)throw ConfigError(ErrorCode::KeyNotFound,"Row key not found");return *row;}
private:
    std::optional<SkillLevelRow> reference_SkillLevel(std::int32_t id,bool load,const Cancellation* cancellation=nullptr)const{if(!id)return std::nullopt;if(!table_SkillLevel)throw ConfigError(ErrorCode::TableNotSelected,"SkillLevel");auto index=table_SkillLevel->rows.indexed().reference(id);return SkillLevelRow(shared_from_this(),load?table_SkillLevel->rows.load(index,*cancellation):table_SkillLevel->rows.get(index));}
    static SkillLevelRow ordinal_SkillLevel(const std::shared_ptr<const Snapshot>& owner,std::size_t index){return SkillLevelRow(owner,owner->table_SkillLevel->rows.get(index));}
public:
    DataGroup<Snapshot,SkillLevelRow> rows_SkillLevel()const{if(!table_SkillLevel)throw ConfigError(ErrorCode::TableNotSelected,"SkillLevel");return {shared_from_this(),table_SkillLevel->all(),&ordinal_SkillLevel};}
    DataGroup<Snapshot,SkillLevelRow> load_table_SkillLevel(const Cancellation& cancellation)const{if(!table_SkillLevel)throw ConfigError(ErrorCode::TableNotSelected,"SkillLevel");table_SkillLevel->load_all(cancellation);return rows_SkillLevel();}
    DataGroup<Snapshot,SkillLevelRow> find_group_SkillLevel_ByLevel(const std::int32_t& k0)const{if(!table_SkillLevel)throw ConfigError(ErrorCode::TableNotSelected,"SkillLevel");return {shared_from_this(),table_SkillLevel->group_ByLevel(k0,false,nullptr),&ordinal_SkillLevel};}
    DataGroup<Snapshot,SkillLevelRow> load_group_SkillLevel_ByLevel(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_SkillLevel)throw ConfigError(ErrorCode::TableNotSelected,"SkillLevel");cancellation.check();return {shared_from_this(),table_SkillLevel->group_ByLevel(k0,true,&cancellation),&ordinal_SkillLevel};}
    DataGroup<Snapshot,SkillLevelRow> find_group_SkillLevel_ByQuality(const std::int32_t& k0)const{if(!table_SkillLevel)throw ConfigError(ErrorCode::TableNotSelected,"SkillLevel");return {shared_from_this(),table_SkillLevel->group_ByQuality(k0,false,nullptr),&ordinal_SkillLevel};}
    DataGroup<Snapshot,SkillLevelRow> load_group_SkillLevel_ByQuality(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_SkillLevel)throw ConfigError(ErrorCode::TableNotSelected,"SkillLevel");cancellation.check();return {shared_from_this(),table_SkillLevel->group_ByQuality(k0,true,&cancellation),&ordinal_SkillLevel};}
    DataGroup<Snapshot,SkillLevelRow> find_group_SkillLevel_BySkillId(const std::int32_t& k0)const{if(!table_SkillLevel)throw ConfigError(ErrorCode::TableNotSelected,"SkillLevel");return {shared_from_this(),table_SkillLevel->group_BySkillId(k0,false,nullptr),&ordinal_SkillLevel};}
    DataGroup<Snapshot,SkillLevelRow> load_group_SkillLevel_BySkillId(const std::int32_t& k0,const Cancellation& cancellation)const{if(!table_SkillLevel)throw ConfigError(ErrorCode::TableNotSelected,"SkillLevel");cancellation.check();return {shared_from_this(),table_SkillLevel->group_BySkillId(k0,true,&cancellation),&ordinal_SkillLevel};}
};
using Manager=ServerManager<Snapshot>;
inline std::optional<HeroRow> AllTypes10kRow::get_Hero()const{return owner_->reference_Hero(value_->HeroId(),false);}
inline std::optional<HeroRow> AllTypes10kRow::load_Hero(const Cancellation& cancellation)const{return owner_->reference_Hero(value_->HeroId(),true,&cancellation);}
inline std::optional<SkillRow> AllTypes10kRow::get_Skill()const{return owner_->reference_Skill(value_->SkillId(),false);}
inline std::optional<SkillRow> AllTypes10kRow::load_Skill(const Cancellation& cancellation)const{return owner_->reference_Skill(value_->SkillId(),true,&cancellation);}
inline std::optional<AllTypes10kRow> AllTypes10kRow::get_Previous()const{return owner_->reference_AllTypes10k(value_->PreviousId(),false);}
inline std::optional<AllTypes10kRow> AllTypes10kRow::load_Previous(const Cancellation& cancellation)const{return owner_->reference_AllTypes10k(value_->PreviousId(),true,&cancellation);}
inline std::optional<SkillRow> HeroRow::get_Skill()const{return owner_->reference_Skill(value_->SkillId(),false);}
inline std::optional<SkillRow> HeroRow::load_Skill(const Cancellation& cancellation)const{return owner_->reference_Skill(value_->SkillId(),true,&cancellation);}
inline std::optional<SkillRow> HeroRow::get_OtherSkill()const{return owner_->reference_Skill(value_->OtherSkillId(),false);}
inline std::optional<SkillRow> HeroRow::load_OtherSkill(const Cancellation& cancellation)const{return owner_->reference_Skill(value_->OtherSkillId(),true,&cancellation);}
inline std::optional<HeroRow> HeroRow::get_Mentor()const{return owner_->reference_Hero(value_->MentorId(),false);}
inline std::optional<HeroRow> HeroRow::load_Mentor(const Cancellation& cancellation)const{return owner_->reference_Hero(value_->MentorId(),true,&cancellation);}
inline std::optional<ItemCatalogRow> QuestRewardsRow::get_Item()const{return owner_->reference_ItemCatalog(value_->ItemId(),false);}
inline std::optional<ItemCatalogRow> QuestRewardsRow::load_Item(const Cancellation& cancellation)const{return owner_->reference_ItemCatalog(value_->ItemId(),true,&cancellation);}
inline std::optional<ItemCatalogRow> ShopOffersRow::get_Item()const{return owner_->reference_ItemCatalog(value_->ItemId(),false);}
inline std::optional<ItemCatalogRow> ShopOffersRow::load_Item(const Cancellation& cancellation)const{return owner_->reference_ItemCatalog(value_->ItemId(),true,&cancellation);}
inline std::optional<HeroRow> SkillRow::get_Hero()const{return owner_->reference_Hero(value_->HeroId(),false);}
inline std::optional<HeroRow> SkillRow::load_Hero(const Cancellation& cancellation)const{return owner_->reference_Hero(value_->HeroId(),true,&cancellation);}
} } // namespace zrconfig::data
