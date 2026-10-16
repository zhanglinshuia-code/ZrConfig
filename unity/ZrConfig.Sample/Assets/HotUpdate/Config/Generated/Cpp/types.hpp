// zrconfig-api-contract:1
// 此文件由工具生成；对象只读，集合访问返回引用，不复制数据。
#pragma once
#include "runtime.hpp"
namespace zrconfig {
enum class T_Game_Config_Quality : std::int32_t {
    T_None = 0,
    T_Rare = 1,
    T_Legendary = 2,
};
class T_Game_Config_ActionGroup;
inline std::shared_ptr<const T_Game_Config_ActionGroup> read_T_Game_Config_ActionGroup(const Json& value);
class T_Game_Config_AddBuff;
inline std::shared_ptr<const T_Game_Config_AddBuff> read_T_Game_Config_AddBuff(const Json& value);
class T_Game_Config_BattleAction;
inline std::shared_ptr<const T_Game_Config_BattleAction> read_T_Game_Config_BattleAction(const Json& value);
class T_Game_Config_BattlePhase;
inline std::shared_ptr<const T_Game_Config_BattlePhase> read_T_Game_Config_BattlePhase(const Json& value);
class T_Game_Config_BattleSettings;
inline std::shared_ptr<const T_Game_Config_BattleSettings> read_T_Game_Config_BattleSettings(const Json& value);
class T_Game_Config_ConfigRange;
inline std::shared_ptr<const T_Game_Config_ConfigRange> read_T_Game_Config_ConfigRange(const Json& value);
class T_Game_Config_Damage;
inline std::shared_ptr<const T_Game_Config_Damage> read_T_Game_Config_Damage(const Json& value);
class T_Game_Config_Money;
inline std::shared_ptr<const T_Game_Config_Money> read_T_Game_Config_Money(const Json& value);
class T_Game_Config_SampleOptions;
inline std::shared_ptr<const T_Game_Config_SampleOptions> read_T_Game_Config_SampleOptions(const Json& value);
class T_ZrConfig_Types_Kv;
inline std::shared_ptr<const T_ZrConfig_Types_Kv> read_T_ZrConfig_Types_Kv(const Json& value);
class T_ZrConfig_Types_Vector2;
inline std::shared_ptr<const T_ZrConfig_Types_Vector2> read_T_ZrConfig_Types_Vector2(const Json& value);
class T_ZrConfig_Types_Vector3;
inline std::shared_ptr<const T_ZrConfig_Types_Vector3> read_T_ZrConfig_Types_Vector3(const Json& value);
class T_Game_Config_ActionGroup {
    std::string v_Name;
    std::vector<std::shared_ptr<const T_Game_Config_BattleAction>> v_Actions;
public:
    virtual ~T_Game_Config_ActionGroup() = default;
    T_Game_Config_ActionGroup(std::string p_Name,std::vector<std::shared_ptr<const T_Game_Config_BattleAction>> p_Actions) : v_Name(std::move(p_Name)),v_Actions(std::move(p_Actions)) {}
    const std::string& Name() const noexcept { return v_Name; }
    const std::vector<std::shared_ptr<const T_Game_Config_BattleAction>>& Actions() const noexcept { return v_Actions; }
};
class T_Game_Config_BattleAction {
    float v_Delay;
public:
    virtual ~T_Game_Config_BattleAction() = default;
    T_Game_Config_BattleAction(float p_Delay) : v_Delay(std::move(p_Delay)) {}
    const float& Delay() const noexcept { return v_Delay; }
};
class T_Game_Config_AddBuff : public T_Game_Config_BattleAction {
    std::int32_t v_BuffId;
    std::int32_t v_Stacks;
public:
    virtual ~T_Game_Config_AddBuff() = default;
    T_Game_Config_AddBuff(float p_Delay,std::int32_t p_BuffId,std::int32_t p_Stacks) : T_Game_Config_BattleAction(std::move(p_Delay)),v_BuffId(std::move(p_BuffId)),v_Stacks(std::move(p_Stacks)) {}
    const std::int32_t& BuffId() const noexcept { return v_BuffId; }
    const std::int32_t& Stacks() const noexcept { return v_Stacks; }
};
class T_Game_Config_BattlePhase {
    std::string v_Name;
    std::vector<std::shared_ptr<const T_Game_Config_ActionGroup>> v_Groups;
public:
    virtual ~T_Game_Config_BattlePhase() = default;
    T_Game_Config_BattlePhase(std::string p_Name,std::vector<std::shared_ptr<const T_Game_Config_ActionGroup>> p_Groups) : v_Name(std::move(p_Name)),v_Groups(std::move(p_Groups)) {}
    const std::string& Name() const noexcept { return v_Name; }
    const std::vector<std::shared_ptr<const T_Game_Config_ActionGroup>>& Groups() const noexcept { return v_Groups; }
};
class T_Game_Config_BattleSettings {
    std::int32_t v_MaxTargets;
    bool v_Interruptible;
public:
    virtual ~T_Game_Config_BattleSettings() = default;
    T_Game_Config_BattleSettings(std::int32_t p_MaxTargets,bool p_Interruptible) : v_MaxTargets(std::move(p_MaxTargets)),v_Interruptible(std::move(p_Interruptible)) {}
    const std::int32_t& MaxTargets() const noexcept { return v_MaxTargets; }
    const bool& Interruptible() const noexcept { return v_Interruptible; }
};
class T_Game_Config_ConfigRange {
    std::int32_t v_Min;
    std::int32_t v_Max;
public:
    virtual ~T_Game_Config_ConfigRange() = default;
    T_Game_Config_ConfigRange(std::int32_t p_Min,std::int32_t p_Max) : v_Min(std::move(p_Min)),v_Max(std::move(p_Max)) {}
    const std::int32_t& Min() const noexcept { return v_Min; }
    const std::int32_t& Max() const noexcept { return v_Max; }
};
class T_Game_Config_Damage : public T_Game_Config_BattleAction {
    float v_Ratio;
    bool v_CanCrit;
public:
    virtual ~T_Game_Config_Damage() = default;
    T_Game_Config_Damage(float p_Delay,float p_Ratio,bool p_CanCrit) : T_Game_Config_BattleAction(std::move(p_Delay)),v_Ratio(std::move(p_Ratio)),v_CanCrit(std::move(p_CanCrit)) {}
    const float& Ratio() const noexcept { return v_Ratio; }
    const bool& CanCrit() const noexcept { return v_CanCrit; }
};
class T_Game_Config_Money {
    std::int32_t v_Cents;
public:
    virtual ~T_Game_Config_Money() = default;
    T_Game_Config_Money(std::int32_t p_Cents) : v_Cents(std::move(p_Cents)) {}
    const std::int32_t& Cents() const noexcept { return v_Cents; }
};
class T_Game_Config_SampleOptions {
    std::shared_ptr<const T_Game_Config_ConfigRange> v_Range;
    std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>> v_Rewards;
    std::shared_ptr<const T_ZrConfig_Types_Vector2> v_Offset;
    std::string v_Label;
    T_Game_Config_Quality v_Quality;
    std::vector<std::int32_t> v_Levels;
public:
    virtual ~T_Game_Config_SampleOptions() = default;
    T_Game_Config_SampleOptions(std::shared_ptr<const T_Game_Config_ConfigRange> p_Range,std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>> p_Rewards,std::shared_ptr<const T_ZrConfig_Types_Vector2> p_Offset,std::string p_Label,T_Game_Config_Quality p_Quality,std::vector<std::int32_t> p_Levels) : v_Range(std::move(p_Range)),v_Rewards(std::move(p_Rewards)),v_Offset(std::move(p_Offset)),v_Label(std::move(p_Label)),v_Quality(std::move(p_Quality)),v_Levels(std::move(p_Levels)) {}
    const std::shared_ptr<const T_Game_Config_ConfigRange>& Range() const noexcept { return v_Range; }
    const std::vector<std::shared_ptr<const T_ZrConfig_Types_Kv>>& Rewards() const noexcept { return v_Rewards; }
    const std::shared_ptr<const T_ZrConfig_Types_Vector2>& Offset() const noexcept { return v_Offset; }
    const std::string& Label() const noexcept { return v_Label; }
    const T_Game_Config_Quality& Quality() const noexcept { return v_Quality; }
    const std::vector<std::int32_t>& Levels() const noexcept { return v_Levels; }
};
class T_ZrConfig_Types_Kv {
    std::int32_t v_Key;
    std::int32_t v_Value;
public:
    virtual ~T_ZrConfig_Types_Kv() = default;
    T_ZrConfig_Types_Kv(std::int32_t p_Key,std::int32_t p_Value) : v_Key(std::move(p_Key)),v_Value(std::move(p_Value)) {}
    const std::int32_t& Key() const noexcept { return v_Key; }
    const std::int32_t& Value() const noexcept { return v_Value; }
};
class T_ZrConfig_Types_Vector2 {
    float v_X;
    float v_Y;
public:
    virtual ~T_ZrConfig_Types_Vector2() = default;
    T_ZrConfig_Types_Vector2(float p_X,float p_Y) : v_X(std::move(p_X)),v_Y(std::move(p_Y)) {}
    const float& X() const noexcept { return v_X; }
    const float& Y() const noexcept { return v_Y; }
};
class T_ZrConfig_Types_Vector3 {
    float v_X;
    float v_Y;
    float v_Z;
public:
    virtual ~T_ZrConfig_Types_Vector3() = default;
    T_ZrConfig_Types_Vector3(float p_X,float p_Y,float p_Z) : v_X(std::move(p_X)),v_Y(std::move(p_Y)),v_Z(std::move(p_Z)) {}
    const float& X() const noexcept { return v_X; }
    const float& Y() const noexcept { return v_Y; }
    const float& Z() const noexcept { return v_Z; }
};
inline std::shared_ptr<const T_Game_Config_ActionGroup> body_T_Game_Config_ActionGroup(const Json& value) { return std::make_shared<const T_Game_Config_ActionGroup>(value.field("Name").string(),decode_list(value.field("Actions"),[](const Json& item0) { return read_T_Game_Config_BattleAction(item0); })); }
inline std::shared_ptr<const T_Game_Config_AddBuff> body_T_Game_Config_AddBuff(const Json& value) { return std::make_shared<const T_Game_Config_AddBuff>(floating<float>(value.field("Delay")),integer<std::int32_t>(value.field("BuffId")),integer<std::int32_t>(value.field("Stacks"))); }
inline std::shared_ptr<const T_Game_Config_BattlePhase> body_T_Game_Config_BattlePhase(const Json& value) { return std::make_shared<const T_Game_Config_BattlePhase>(value.field("Name").string(),decode_list(value.field("Groups"),[](const Json& item0) { return read_T_Game_Config_ActionGroup(item0); })); }
inline std::shared_ptr<const T_Game_Config_BattleSettings> body_T_Game_Config_BattleSettings(const Json& value) { return std::make_shared<const T_Game_Config_BattleSettings>(integer<std::int32_t>(value.field("MaxTargets")),value.field("Interruptible").boolean()); }
inline std::shared_ptr<const T_Game_Config_ConfigRange> body_T_Game_Config_ConfigRange(const Json& value) { return std::make_shared<const T_Game_Config_ConfigRange>(integer<std::int32_t>(value.field("Min")),integer<std::int32_t>(value.field("Max"))); }
inline std::shared_ptr<const T_Game_Config_Damage> body_T_Game_Config_Damage(const Json& value) { return std::make_shared<const T_Game_Config_Damage>(floating<float>(value.field("Delay")),floating<float>(value.field("Ratio")),value.field("CanCrit").boolean()); }
inline std::shared_ptr<const T_Game_Config_Money> body_T_Game_Config_Money(const Json& value) { return std::make_shared<const T_Game_Config_Money>(integer<std::int32_t>(value.field("Cents"))); }
inline std::shared_ptr<const T_Game_Config_SampleOptions> body_T_Game_Config_SampleOptions(const Json& value) { return std::make_shared<const T_Game_Config_SampleOptions>(read_T_Game_Config_ConfigRange(value.field("Range")),decode_list(value.field("Rewards"),[](const Json& item0) { return read_T_ZrConfig_Types_Kv(item0); }),read_T_ZrConfig_Types_Vector2(value.field("Offset")),value.field("Label").string(),static_cast<T_Game_Config_Quality>(integer<std::int32_t>(value.field("Quality"))),decode_list(value.field("Levels"),[](const Json& item0) { return integer<std::int32_t>(item0); })); }
inline std::shared_ptr<const T_ZrConfig_Types_Kv> body_T_ZrConfig_Types_Kv(const Json& value) { return std::make_shared<const T_ZrConfig_Types_Kv>(integer<std::int32_t>(value.field("Key")),integer<std::int32_t>(value.field("Value"))); }
inline std::shared_ptr<const T_ZrConfig_Types_Vector2> body_T_ZrConfig_Types_Vector2(const Json& value) { return std::make_shared<const T_ZrConfig_Types_Vector2>(floating<float>(value.field("X")),floating<float>(value.field("Y"))); }
inline std::shared_ptr<const T_ZrConfig_Types_Vector3> body_T_ZrConfig_Types_Vector3(const Json& value) { return std::make_shared<const T_ZrConfig_Types_Vector3>(floating<float>(value.field("X")),floating<float>(value.field("Y")),floating<float>(value.field("Z"))); }
inline std::shared_ptr<const T_Game_Config_ActionGroup> read_T_Game_Config_ActionGroup(const Json& value) {
    if(value.kind == Json::Kind::Null) return nullptr;
    return body_T_Game_Config_ActionGroup(value);
}
inline std::shared_ptr<const T_Game_Config_AddBuff> read_T_Game_Config_AddBuff(const Json& value) {
    if(value.kind == Json::Kind::Null) return nullptr;
    return body_T_Game_Config_AddBuff(value);
}
inline std::shared_ptr<const T_Game_Config_BattleAction> read_T_Game_Config_BattleAction(const Json& value) {
    if(value.kind == Json::Kind::Null) return nullptr;
    const auto& tag = value.field("$type").string();
    if(tag == "Game.Config.AddBuff") return body_T_Game_Config_AddBuff(value);
    if(tag == "Game.Config.Damage") return body_T_Game_Config_Damage(value);
    invalid("未知多态类型");
}
inline std::shared_ptr<const T_Game_Config_BattlePhase> read_T_Game_Config_BattlePhase(const Json& value) {
    if(value.kind == Json::Kind::Null) return nullptr;
    return body_T_Game_Config_BattlePhase(value);
}
inline std::shared_ptr<const T_Game_Config_BattleSettings> read_T_Game_Config_BattleSettings(const Json& value) {
    if(value.kind == Json::Kind::Null) return nullptr;
    return body_T_Game_Config_BattleSettings(value);
}
inline std::shared_ptr<const T_Game_Config_ConfigRange> read_T_Game_Config_ConfigRange(const Json& value) {
    return body_T_Game_Config_ConfigRange(value);
}
inline std::shared_ptr<const T_Game_Config_Damage> read_T_Game_Config_Damage(const Json& value) {
    if(value.kind == Json::Kind::Null) return nullptr;
    return body_T_Game_Config_Damage(value);
}
inline std::shared_ptr<const T_Game_Config_Money> read_T_Game_Config_Money(const Json& value) {
    return body_T_Game_Config_Money(value);
}
inline std::shared_ptr<const T_Game_Config_SampleOptions> read_T_Game_Config_SampleOptions(const Json& value) {
    if(value.kind == Json::Kind::Null) return nullptr;
    return body_T_Game_Config_SampleOptions(value);
}
inline std::shared_ptr<const T_ZrConfig_Types_Kv> read_T_ZrConfig_Types_Kv(const Json& value) {
    return body_T_ZrConfig_Types_Kv(value);
}
inline std::shared_ptr<const T_ZrConfig_Types_Vector2> read_T_ZrConfig_Types_Vector2(const Json& value) {
    return body_T_ZrConfig_Types_Vector2(value);
}
inline std::shared_ptr<const T_ZrConfig_Types_Vector3> read_T_ZrConfig_Types_Vector3(const Json& value) {
    return body_T_ZrConfig_Types_Vector3(value);
}
} // namespace zrconfig
