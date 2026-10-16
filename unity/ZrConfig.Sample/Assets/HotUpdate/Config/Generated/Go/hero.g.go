// zrconfig-api-contract:1
// 此文件由工具自动生成，请勿手工修改。
package zrconfig

type HeroConfig struct {
    serverOwner *ConfigSnapshot
    v_Id int32
    v_Name string
    v_Skill int32
    ref_Skill *SkillConfig
    v_OtherSkill int32
    ref_OtherSkill *SkillConfig
    v_Mentor int32
    ref_Mentor *HeroConfig
}
// 英雄编号
func(v *HeroConfig) Id() int32 { return v.v_Id }
// 英雄名称
func(v *HeroConfig) Name() string { return v.v_Name }
// 技能 Id；引用 Skill 表
func(v *HeroConfig) SkillId() int32 { return v.v_Skill }
func(v *HeroConfig) Skill() *SkillConfig { return v.ref_Skill }
// 另一个技能 Id；可复用同一对象
func(v *HeroConfig) OtherSkillId() int32 { return v.v_OtherSkill }
func(v *HeroConfig) OtherSkill() *SkillConfig { return v.ref_OtherSkill }
// 导师 Id；引用 Hero 表
func(v *HeroConfig) MentorId() int32 { return v.v_Mentor }
func(v *HeroConfig) Mentor() *HeroConfig { return v.ref_Mentor }
const schemaHero = "{\"name\":\"Hero\",\"hash\":\"7E1E75AC24BAFD42C18F1B43A19B0027A3D241CC665DB3727BC98A4908927ECE\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"英雄编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"英雄名称\"},{\"name\":\"Skill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"技能 Id；引用 Skill 表\"},{\"name\":\"OtherSkill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"另一个技能 Id；可复用同一对象\"},{\"name\":\"Mentor\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"导师 Id；引用 Hero 表\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"},{\"id\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Skill\"}],\"tags\":[],\"dependencies\":[\"Hero\",\"Skill\"]}"
type HeroKey_Key struct {
    v_Id int32
}
type HeroKey_Group_ById struct {
    v_Id int32
}
type HeroTable struct {
    rows List[*HeroConfig]
    index_Key map[HeroKey_Key]*HeroConfig
    index_Group_ById map[HeroKey_Group_ById]List[*HeroConfig]
    ids map[int32]*HeroConfig
}
func loadHero(source []any)*HeroTable {
    result:=&HeroTable{rows:List[*HeroConfig]{values:make([]*HeroConfig,len(source))}}
    result.index_Key=make(map[HeroKey_Key]*HeroConfig,len(source))
    result.index_Group_ById=make(map[HeroKey_Group_ById]List[*HeroConfig],len(source))
    result.ids=make(map[int32]*HeroConfig,len(source))
    for i,value:=range source {
        row:=&HeroConfig{
            v_Id: int32(signed(field(value,"Id"),32)),
            v_Name: text(field(value,"Name")),
            v_Skill: int32(signed(field(value,"Skill"),32)),
            v_OtherSkill: int32(signed(field(value,"OtherSkill"),32)),
            v_Mentor: int32(signed(field(value,"Mentor"),32)),
        }
        result.rows.values[i]=row
        key_Key:=HeroKey_Key{row.v_Id}
        if _,ok:=result.index_Key[key_Key];ok{invalid("主键重复")}
        result.index_Key[key_Key]=row
        key_Group_ById:=HeroKey_Group_ById{row.v_Id}
        group_Group_ById:=result.index_Group_ById[key_Group_ById]
        group_Group_ById.values=append(group_Group_ById.values,row)
        result.index_Group_ById[key_Group_ById]=group_Group_ById
        if _,ok:=result.ids[row.v_Id];ok{invalid("引用 Id 不唯一")}
        result.ids[row.v_Id]=row
    }
    return result
}
func(t *HeroTable)bind(snapshot *ConfigSnapshot){
    for _,row:=range t.rows.values{if row.v_Skill!=0{row.ref_Skill=snapshot.table_Skill.ids[row.v_Skill];if row.ref_Skill==nil{invalid("引用目标不存在：Hero.Skill")}}}
    for _,row:=range t.rows.values{if row.v_OtherSkill!=0{row.ref_OtherSkill=snapshot.table_Skill.ids[row.v_OtherSkill];if row.ref_OtherSkill==nil{invalid("引用目标不存在：Hero.OtherSkill")}}}
    for _,row:=range t.rows.values{if row.v_Mentor!=0{row.ref_Mentor=snapshot.table_Hero.ids[row.v_Mentor];if row.ref_Mentor==nil{invalid("引用目标不存在：Hero.Mentor")}}}
}
func(t *HeroTable) Rows()List[*HeroConfig]{return t.rows}
func(t *HeroTable) Count()int{return t.rows.Len()}
func(t *HeroTable) Find(k0 int32)(*HeroConfig,bool){value,ok:=t.index_Key[HeroKey_Key{k0}];return value,ok}
func(t *HeroTable) Get(k0 int32)*HeroConfig{value:=t.index_Key[HeroKey_Key{k0}];if value==nil{invalid("配置主键不存在")};return value}
func(t *HeroTable) GetById(k0 int32)List[*HeroConfig]{return t.index_Group_ById[HeroKey_Group_ById{k0}]}
