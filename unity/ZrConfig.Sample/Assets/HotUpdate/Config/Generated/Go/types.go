// zrconfig-api-contract:1
// 此文件由工具生成，自定义类型共用统一结构。
package zrconfig
type T_Game_Config_ActionGroup struct {
    v_Name string
    v_Actions List[T_Game_Config_BattleAction]
}
func (v *T_Game_Config_ActionGroup) Name() string { return v.v_Name }
func (v *T_Game_Config_ActionGroup) Actions() List[T_Game_Config_BattleAction] { return v.v_Actions }
func body_T_Game_Config_ActionGroup(value any) *T_Game_Config_ActionGroup {
    return &T_Game_Config_ActionGroup{
        v_Name: text(field(value,"Name")),
        v_Actions: decodeList(field(value,"Actions"),func(item0 any) T_Game_Config_BattleAction { return read_T_Game_Config_BattleAction(item0) }),
    }
}
func read_T_Game_Config_ActionGroup(value any) *T_Game_Config_ActionGroup {
    if value == nil { return nil }
    return body_T_Game_Config_ActionGroup(value)
}
type T_Game_Config_AddBuff struct {
    v_Delay float32
    v_BuffId int32
    v_Stacks int32
}
func (v *T_Game_Config_AddBuff) Delay() float32 { return v.v_Delay }
func (v *T_Game_Config_AddBuff) BuffId() int32 { return v.v_BuffId }
func (v *T_Game_Config_AddBuff) Stacks() int32 { return v.v_Stacks }
func (v *T_Game_Config_AddBuff) is_T_Game_Config_BattleAction() {}
func body_T_Game_Config_AddBuff(value any) *T_Game_Config_AddBuff {
    return &T_Game_Config_AddBuff{
        v_Delay: float32(floating(field(value,"Delay"),32)),
        v_BuffId: int32(signed(field(value,"BuffId"),32)),
        v_Stacks: int32(signed(field(value,"Stacks"),32)),
    }
}
func read_T_Game_Config_AddBuff(value any) *T_Game_Config_AddBuff {
    if value == nil { return nil }
    return body_T_Game_Config_AddBuff(value)
}
type T_Game_Config_BattleAction interface {
    is_T_Game_Config_BattleAction()
    Delay() float32
}
func read_T_Game_Config_BattleAction(value any) T_Game_Config_BattleAction {
    if value == nil { return nil }
    switch text(field(value,"$type")) {
    case "Game.Config.AddBuff": return body_T_Game_Config_AddBuff(value)
    case "Game.Config.Damage": return body_T_Game_Config_Damage(value)
    default: invalid("未知多态类型")
    }
    return nil
}
type T_Game_Config_BattlePhase struct {
    v_Name string
    v_Groups List[*T_Game_Config_ActionGroup]
}
func (v *T_Game_Config_BattlePhase) Name() string { return v.v_Name }
func (v *T_Game_Config_BattlePhase) Groups() List[*T_Game_Config_ActionGroup] { return v.v_Groups }
func body_T_Game_Config_BattlePhase(value any) *T_Game_Config_BattlePhase {
    return &T_Game_Config_BattlePhase{
        v_Name: text(field(value,"Name")),
        v_Groups: decodeList(field(value,"Groups"),func(item0 any) *T_Game_Config_ActionGroup { return read_T_Game_Config_ActionGroup(item0) }),
    }
}
func read_T_Game_Config_BattlePhase(value any) *T_Game_Config_BattlePhase {
    if value == nil { return nil }
    return body_T_Game_Config_BattlePhase(value)
}
type T_Game_Config_BattleSettings struct {
    v_MaxTargets int32
    v_Interruptible bool
}
func (v *T_Game_Config_BattleSettings) MaxTargets() int32 { return v.v_MaxTargets }
func (v *T_Game_Config_BattleSettings) Interruptible() bool { return v.v_Interruptible }
func body_T_Game_Config_BattleSettings(value any) *T_Game_Config_BattleSettings {
    return &T_Game_Config_BattleSettings{
        v_MaxTargets: int32(signed(field(value,"MaxTargets"),32)),
        v_Interruptible: boolean(field(value,"Interruptible")),
    }
}
func read_T_Game_Config_BattleSettings(value any) *T_Game_Config_BattleSettings {
    if value == nil { return nil }
    return body_T_Game_Config_BattleSettings(value)
}
type T_Game_Config_ConfigRange struct {
    v_Min int32
    v_Max int32
}
func (v T_Game_Config_ConfigRange) Min() int32 { return v.v_Min }
func (v T_Game_Config_ConfigRange) Max() int32 { return v.v_Max }
func body_T_Game_Config_ConfigRange(value any) T_Game_Config_ConfigRange {
    return T_Game_Config_ConfigRange{
        v_Min: int32(signed(field(value,"Min"),32)),
        v_Max: int32(signed(field(value,"Max"),32)),
    }
}
func read_T_Game_Config_ConfigRange(value any) T_Game_Config_ConfigRange {
    return body_T_Game_Config_ConfigRange(value)
}
type T_Game_Config_Damage struct {
    v_Delay float32
    v_Ratio float32
    v_CanCrit bool
}
func (v *T_Game_Config_Damage) Delay() float32 { return v.v_Delay }
func (v *T_Game_Config_Damage) Ratio() float32 { return v.v_Ratio }
func (v *T_Game_Config_Damage) CanCrit() bool { return v.v_CanCrit }
func (v *T_Game_Config_Damage) is_T_Game_Config_BattleAction() {}
func body_T_Game_Config_Damage(value any) *T_Game_Config_Damage {
    return &T_Game_Config_Damage{
        v_Delay: float32(floating(field(value,"Delay"),32)),
        v_Ratio: float32(floating(field(value,"Ratio"),32)),
        v_CanCrit: boolean(field(value,"CanCrit")),
    }
}
func read_T_Game_Config_Damage(value any) *T_Game_Config_Damage {
    if value == nil { return nil }
    return body_T_Game_Config_Damage(value)
}
type T_Game_Config_Money struct {
    v_Cents int32
}
func (v T_Game_Config_Money) Cents() int32 { return v.v_Cents }
func body_T_Game_Config_Money(value any) T_Game_Config_Money {
    return T_Game_Config_Money{
        v_Cents: int32(signed(field(value,"Cents"),32)),
    }
}
func read_T_Game_Config_Money(value any) T_Game_Config_Money {
    return body_T_Game_Config_Money(value)
}
type T_Game_Config_Quality int32
const T_Game_Config_Quality_T_None T_Game_Config_Quality = 0
const T_Game_Config_Quality_T_Rare T_Game_Config_Quality = 1
const T_Game_Config_Quality_T_Legendary T_Game_Config_Quality = 2
type T_Game_Config_SampleOptions struct {
    v_Range T_Game_Config_ConfigRange
    v_Rewards List[T_ZrConfig_Types_Kv]
    v_Offset T_ZrConfig_Types_Vector2
    v_Label string
    v_Quality T_Game_Config_Quality
    v_Levels List[int32]
}
func (v *T_Game_Config_SampleOptions) Range() T_Game_Config_ConfigRange { return v.v_Range }
func (v *T_Game_Config_SampleOptions) Rewards() List[T_ZrConfig_Types_Kv] { return v.v_Rewards }
func (v *T_Game_Config_SampleOptions) Offset() T_ZrConfig_Types_Vector2 { return v.v_Offset }
func (v *T_Game_Config_SampleOptions) Label() string { return v.v_Label }
func (v *T_Game_Config_SampleOptions) Quality() T_Game_Config_Quality { return v.v_Quality }
func (v *T_Game_Config_SampleOptions) Levels() List[int32] { return v.v_Levels }
func body_T_Game_Config_SampleOptions(value any) *T_Game_Config_SampleOptions {
    return &T_Game_Config_SampleOptions{
        v_Range: read_T_Game_Config_ConfigRange(field(value,"Range")),
        v_Rewards: decodeList(field(value,"Rewards"),func(item0 any) T_ZrConfig_Types_Kv { return read_T_ZrConfig_Types_Kv(item0) }),
        v_Offset: read_T_ZrConfig_Types_Vector2(field(value,"Offset")),
        v_Label: text(field(value,"Label")),
        v_Quality: T_Game_Config_Quality(int32(signed(field(value,"Quality"),32))),
        v_Levels: decodeList(field(value,"Levels"),func(item0 any) int32 { return int32(signed(item0,32)) }),
    }
}
func read_T_Game_Config_SampleOptions(value any) *T_Game_Config_SampleOptions {
    if value == nil { return nil }
    return body_T_Game_Config_SampleOptions(value)
}
type T_ZrConfig_Types_Kv struct {
    v_Key int32
    v_Value int32
}
func (v T_ZrConfig_Types_Kv) Key() int32 { return v.v_Key }
func (v T_ZrConfig_Types_Kv) Value() int32 { return v.v_Value }
func body_T_ZrConfig_Types_Kv(value any) T_ZrConfig_Types_Kv {
    return T_ZrConfig_Types_Kv{
        v_Key: int32(signed(field(value,"Key"),32)),
        v_Value: int32(signed(field(value,"Value"),32)),
    }
}
func read_T_ZrConfig_Types_Kv(value any) T_ZrConfig_Types_Kv {
    return body_T_ZrConfig_Types_Kv(value)
}
type T_ZrConfig_Types_Vector2 struct {
    v_X float32
    v_Y float32
}
func (v T_ZrConfig_Types_Vector2) X() float32 { return v.v_X }
func (v T_ZrConfig_Types_Vector2) Y() float32 { return v.v_Y }
func body_T_ZrConfig_Types_Vector2(value any) T_ZrConfig_Types_Vector2 {
    return T_ZrConfig_Types_Vector2{
        v_X: float32(floating(field(value,"X"),32)),
        v_Y: float32(floating(field(value,"Y"),32)),
    }
}
func read_T_ZrConfig_Types_Vector2(value any) T_ZrConfig_Types_Vector2 {
    return body_T_ZrConfig_Types_Vector2(value)
}
type T_ZrConfig_Types_Vector3 struct {
    v_X float32
    v_Y float32
    v_Z float32
}
func (v T_ZrConfig_Types_Vector3) X() float32 { return v.v_X }
func (v T_ZrConfig_Types_Vector3) Y() float32 { return v.v_Y }
func (v T_ZrConfig_Types_Vector3) Z() float32 { return v.v_Z }
func body_T_ZrConfig_Types_Vector3(value any) T_ZrConfig_Types_Vector3 {
    return T_ZrConfig_Types_Vector3{
        v_X: float32(floating(field(value,"X"),32)),
        v_Y: float32(floating(field(value,"Y"),32)),
        v_Z: float32(floating(field(value,"Z"),32)),
    }
}
func read_T_ZrConfig_Types_Vector3(value any) T_ZrConfig_Types_Vector3 {
    return body_T_ZrConfig_Types_Vector3(value)
}
