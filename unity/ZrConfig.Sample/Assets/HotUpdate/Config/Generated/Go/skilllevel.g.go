// zrconfig-api-contract:1
// 此文件由工具自动生成，请勿手工修改。
package zrconfig

type SkillLevelConfig struct {
    serverOwner *ConfigSnapshot
    v_SkillId int32
    v_Name string
    v_Level int32
    v_Quality int32
    v_Damage int32
}
// 技能编号
func(v *SkillLevelConfig) SkillId() int32 { return v.v_SkillId }
// 技能名称
func(v *SkillLevelConfig) Name() string { return v.v_Name }
// 技能等级
func(v *SkillLevelConfig) Level() int32 { return v.v_Level }
// 品质
func(v *SkillLevelConfig) Quality() int32 { return v.v_Quality }
// 伤害
func(v *SkillLevelConfig) Damage() int32 { return v.v_Damage }
const schemaSkillLevel = "{\"name\":\"SkillLevel\",\"hash\":\"9DDEDF4A13E49BE883FA11B62A966463D9B27C99620857DE5CD5E721CECC92B2\",\"hasId\":false,\"fields\":[{\"name\":\"SkillId\",\"type\":\"System.Int32\",\"description\":\"技能编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"技能名称\"},{\"name\":\"Level\",\"type\":\"System.Int32\",\"description\":\"技能等级\"},{\"name\":\"Quality\",\"type\":\"System.Int32\",\"description\":\"品质\"},{\"name\":\"Damage\",\"type\":\"System.Int32\",\"description\":\"伤害\"}],\"keys\":[\"SkillId\",\"Level\",\"Quality\"],\"groups\":[{\"name\":\"ByLevel\",\"fields\":[\"Level\"]},{\"name\":\"ByQuality\",\"fields\":[\"Quality\"]},{\"name\":\"BySkillId\",\"fields\":[\"SkillId\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}"
type SkillLevelKey_Key struct {
    v_SkillId int32
    v_Level int32
    v_Quality int32
}
type SkillLevelKey_Group_ByLevel struct {
    v_Level int32
}
type SkillLevelKey_Group_ByQuality struct {
    v_Quality int32
}
type SkillLevelKey_Group_BySkillId struct {
    v_SkillId int32
}
type SkillLevelTable struct {
    rows List[*SkillLevelConfig]
    index_Key map[SkillLevelKey_Key]*SkillLevelConfig
    index_Group_ByLevel map[SkillLevelKey_Group_ByLevel]List[*SkillLevelConfig]
    index_Group_ByQuality map[SkillLevelKey_Group_ByQuality]List[*SkillLevelConfig]
    index_Group_BySkillId map[SkillLevelKey_Group_BySkillId]List[*SkillLevelConfig]
}
func loadSkillLevel(source []any)*SkillLevelTable {
    result:=&SkillLevelTable{rows:List[*SkillLevelConfig]{values:make([]*SkillLevelConfig,len(source))}}
    result.index_Key=make(map[SkillLevelKey_Key]*SkillLevelConfig,len(source))
    result.index_Group_ByLevel=make(map[SkillLevelKey_Group_ByLevel]List[*SkillLevelConfig],len(source))
    result.index_Group_ByQuality=make(map[SkillLevelKey_Group_ByQuality]List[*SkillLevelConfig],len(source))
    result.index_Group_BySkillId=make(map[SkillLevelKey_Group_BySkillId]List[*SkillLevelConfig],len(source))
    for i,value:=range source {
        row:=&SkillLevelConfig{
            v_SkillId: int32(signed(field(value,"SkillId"),32)),
            v_Name: text(field(value,"Name")),
            v_Level: int32(signed(field(value,"Level"),32)),
            v_Quality: int32(signed(field(value,"Quality"),32)),
            v_Damage: int32(signed(field(value,"Damage"),32)),
        }
        result.rows.values[i]=row
        key_Key:=SkillLevelKey_Key{row.v_SkillId,row.v_Level,row.v_Quality}
        if _,ok:=result.index_Key[key_Key];ok{invalid("主键重复")}
        result.index_Key[key_Key]=row
        key_Group_ByLevel:=SkillLevelKey_Group_ByLevel{row.v_Level}
        group_Group_ByLevel:=result.index_Group_ByLevel[key_Group_ByLevel]
        group_Group_ByLevel.values=append(group_Group_ByLevel.values,row)
        result.index_Group_ByLevel[key_Group_ByLevel]=group_Group_ByLevel
        key_Group_ByQuality:=SkillLevelKey_Group_ByQuality{row.v_Quality}
        group_Group_ByQuality:=result.index_Group_ByQuality[key_Group_ByQuality]
        group_Group_ByQuality.values=append(group_Group_ByQuality.values,row)
        result.index_Group_ByQuality[key_Group_ByQuality]=group_Group_ByQuality
        key_Group_BySkillId:=SkillLevelKey_Group_BySkillId{row.v_SkillId}
        group_Group_BySkillId:=result.index_Group_BySkillId[key_Group_BySkillId]
        group_Group_BySkillId.values=append(group_Group_BySkillId.values,row)
        result.index_Group_BySkillId[key_Group_BySkillId]=group_Group_BySkillId
    }
    return result
}
func(t *SkillLevelTable)bind(snapshot *ConfigSnapshot){
}
func(t *SkillLevelTable) Rows()List[*SkillLevelConfig]{return t.rows}
func(t *SkillLevelTable) Count()int{return t.rows.Len()}
func(t *SkillLevelTable) Find(k0 int32,k1 int32,k2 int32)(*SkillLevelConfig,bool){value,ok:=t.index_Key[SkillLevelKey_Key{k0,k1,k2}];return value,ok}
func(t *SkillLevelTable) Get(k0 int32,k1 int32,k2 int32)*SkillLevelConfig{value:=t.index_Key[SkillLevelKey_Key{k0,k1,k2}];if value==nil{invalid("配置主键不存在")};return value}
func(t *SkillLevelTable) GetByLevel(k0 int32)List[*SkillLevelConfig]{return t.index_Group_ByLevel[SkillLevelKey_Group_ByLevel{k0}]}
func(t *SkillLevelTable) GetByQuality(k0 int32)List[*SkillLevelConfig]{return t.index_Group_ByQuality[SkillLevelKey_Group_ByQuality{k0}]}
func(t *SkillLevelTable) GetBySkillId(k0 int32)List[*SkillLevelConfig]{return t.index_Group_BySkillId[SkillLevelKey_Group_BySkillId{k0}]}
