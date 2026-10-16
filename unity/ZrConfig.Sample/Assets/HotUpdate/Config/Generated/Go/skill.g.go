// zrconfig-api-contract:1
// 此文件由工具自动生成，请勿手工修改。
package zrconfig

type SkillConfig struct {
    serverOwner *ConfigSnapshot
    v_Id int32
    v_Name string
    v_Hero int32
    ref_Hero *HeroConfig
}
// 技能编号
func(v *SkillConfig) Id() int32 { return v.v_Id }
// 技能名称
func(v *SkillConfig) Name() string { return v.v_Name }
// 英雄 Id；反向引用 Hero 表
func(v *SkillConfig) HeroId() int32 { return v.v_Hero }
func(v *SkillConfig) Hero() *HeroConfig { return v.ref_Hero }
const schemaSkill = "{\"name\":\"Skill\",\"hash\":\"51DA7601418A98574B1C5ADC656CB7E7B479CC1F3E5237DFC5BA7532F702B9DB\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"技能编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"技能名称\"},{\"name\":\"Hero\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"英雄 Id；反向引用 Hero 表\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"}],\"tags\":[],\"dependencies\":[\"Hero\"]}"
type SkillKey_Key struct {
    v_Id int32
}
type SkillKey_Group_ById struct {
    v_Id int32
}
type SkillTable struct {
    rows List[*SkillConfig]
    index_Key map[SkillKey_Key]*SkillConfig
    index_Group_ById map[SkillKey_Group_ById]List[*SkillConfig]
    ids map[int32]*SkillConfig
}
func loadSkill(source []any)*SkillTable {
    result:=&SkillTable{rows:List[*SkillConfig]{values:make([]*SkillConfig,len(source))}}
    result.index_Key=make(map[SkillKey_Key]*SkillConfig,len(source))
    result.index_Group_ById=make(map[SkillKey_Group_ById]List[*SkillConfig],len(source))
    result.ids=make(map[int32]*SkillConfig,len(source))
    for i,value:=range source {
        row:=&SkillConfig{
            v_Id: int32(signed(field(value,"Id"),32)),
            v_Name: text(field(value,"Name")),
            v_Hero: int32(signed(field(value,"Hero"),32)),
        }
        result.rows.values[i]=row
        key_Key:=SkillKey_Key{row.v_Id}
        if _,ok:=result.index_Key[key_Key];ok{invalid("主键重复")}
        result.index_Key[key_Key]=row
        key_Group_ById:=SkillKey_Group_ById{row.v_Id}
        group_Group_ById:=result.index_Group_ById[key_Group_ById]
        group_Group_ById.values=append(group_Group_ById.values,row)
        result.index_Group_ById[key_Group_ById]=group_Group_ById
        if _,ok:=result.ids[row.v_Id];ok{invalid("引用 Id 不唯一")}
        result.ids[row.v_Id]=row
    }
    return result
}
func(t *SkillTable)bind(snapshot *ConfigSnapshot){
    for _,row:=range t.rows.values{if row.v_Hero!=0{row.ref_Hero=snapshot.table_Hero.ids[row.v_Hero];if row.ref_Hero==nil{invalid("引用目标不存在：Skill.Hero")}}}
}
func(t *SkillTable) Rows()List[*SkillConfig]{return t.rows}
func(t *SkillTable) Count()int{return t.rows.Len()}
func(t *SkillTable) Find(k0 int32)(*SkillConfig,bool){value,ok:=t.index_Key[SkillKey_Key{k0}];return value,ok}
func(t *SkillTable) Get(k0 int32)*SkillConfig{value:=t.index_Key[SkillKey_Key{k0}];if value==nil{invalid("配置主键不存在")};return value}
func(t *SkillTable) GetById(k0 int32)List[*SkillConfig]{return t.index_Group_ById[SkillKey_Group_ById{k0}]}
