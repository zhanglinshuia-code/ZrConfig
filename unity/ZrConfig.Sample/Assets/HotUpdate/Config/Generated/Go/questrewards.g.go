// zrconfig-api-contract:1
// 此文件由工具自动生成，请勿手工修改。
package zrconfig

type QuestRewardsConfig struct {
    serverOwner *ConfigSnapshot
    v_Id int32
    v_Name string
    v_Item int32
    ref_Item *ItemCatalogConfig
    v_Count int32
    v_RequiredLevel int32
    v_Enabled bool
}
// 任务编号
func(v *QuestRewardsConfig) Id() int32 { return v.v_Id }
// 任务名称
func(v *QuestRewardsConfig) Name() string { return v.v_Name }
// 引用 ItemCatalog 的 Id
func(v *QuestRewardsConfig) ItemId() int32 { return v.v_Item }
func(v *QuestRewardsConfig) Item() *ItemCatalogConfig { return v.ref_Item }
// 奖励数量：大于零
func(v *QuestRewardsConfig) Count() int32 { return v.v_Count }
// 开放等级：大于零
func(v *QuestRewardsConfig) RequiredLevel() int32 { return v.v_RequiredLevel }
// 是否启用
func(v *QuestRewardsConfig) Enabled() bool { return v.v_Enabled }
const schemaQuestRewards = "{\"name\":\"QuestRewards\",\"hash\":\"C839B7EDFA92304709824DABBA84B5534E34810224D2814C221E6F4A3B536E35\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"任务编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"任务名称\"},{\"name\":\"Item\",\"type\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"description\":\"引用 ItemCatalog 的 Id\"},{\"name\":\"Count\",\"type\":\"System.Int32\",\"description\":\"奖励数量：大于零\"},{\"name\":\"RequiredLevel\",\"type\":\"System.Int32\",\"description\":\"开放等级：大于零\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否启用\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"ItemCatalog\"}],\"tags\":[],\"dependencies\":[\"ItemCatalog\"]}"
type QuestRewardsKey_Key struct {
    v_Id int32
}
type QuestRewardsKey_Group_ById struct {
    v_Id int32
}
type QuestRewardsTable struct {
    rows List[*QuestRewardsConfig]
    index_Key map[QuestRewardsKey_Key]*QuestRewardsConfig
    index_Group_ById map[QuestRewardsKey_Group_ById]List[*QuestRewardsConfig]
}
func loadQuestRewards(source []any)*QuestRewardsTable {
    result:=&QuestRewardsTable{rows:List[*QuestRewardsConfig]{values:make([]*QuestRewardsConfig,len(source))}}
    result.index_Key=make(map[QuestRewardsKey_Key]*QuestRewardsConfig,len(source))
    result.index_Group_ById=make(map[QuestRewardsKey_Group_ById]List[*QuestRewardsConfig],len(source))
    for i,value:=range source {
        row:=&QuestRewardsConfig{
            v_Id: int32(signed(field(value,"Id"),32)),
            v_Name: text(field(value,"Name")),
            v_Item: int32(signed(field(value,"Item"),32)),
            v_Count: int32(signed(field(value,"Count"),32)),
            v_RequiredLevel: int32(signed(field(value,"RequiredLevel"),32)),
            v_Enabled: boolean(field(value,"Enabled")),
        }
        result.rows.values[i]=row
        key_Key:=QuestRewardsKey_Key{row.v_Id}
        if _,ok:=result.index_Key[key_Key];ok{invalid("主键重复")}
        result.index_Key[key_Key]=row
        key_Group_ById:=QuestRewardsKey_Group_ById{row.v_Id}
        group_Group_ById:=result.index_Group_ById[key_Group_ById]
        group_Group_ById.values=append(group_Group_ById.values,row)
        result.index_Group_ById[key_Group_ById]=group_Group_ById
    }
    return result
}
func(t *QuestRewardsTable)bind(snapshot *ConfigSnapshot){
    for _,row:=range t.rows.values{if row.v_Item!=0{row.ref_Item=snapshot.table_ItemCatalog.ids[row.v_Item];if row.ref_Item==nil{invalid("引用目标不存在：QuestRewards.Item")}}}
}
func(t *QuestRewardsTable) Rows()List[*QuestRewardsConfig]{return t.rows}
func(t *QuestRewardsTable) Count()int{return t.rows.Len()}
func(t *QuestRewardsTable) Find(k0 int32)(*QuestRewardsConfig,bool){value,ok:=t.index_Key[QuestRewardsKey_Key{k0}];return value,ok}
func(t *QuestRewardsTable) Get(k0 int32)*QuestRewardsConfig{value:=t.index_Key[QuestRewardsKey_Key{k0}];if value==nil{invalid("配置主键不存在")};return value}
func(t *QuestRewardsTable) GetById(k0 int32)List[*QuestRewardsConfig]{return t.index_Group_ById[QuestRewardsKey_Group_ById{k0}]}
