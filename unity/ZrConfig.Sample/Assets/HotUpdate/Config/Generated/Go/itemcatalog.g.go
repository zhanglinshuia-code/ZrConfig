// zrconfig-api-contract:1
// 此文件由工具自动生成，请勿手工修改。
package zrconfig

type ItemCatalogConfig struct {
    serverOwner *ConfigSnapshot
    v_Id int32
    v_Name string
    v_MaxStack int32
    v_Rarity int32
    v_Enabled bool
}
// 道具编号
func(v *ItemCatalogConfig) Id() int32 { return v.v_Id }
// 显示名称
func(v *ItemCatalogConfig) Name() string { return v.v_Name }
// 堆叠上限：1～10000
func(v *ItemCatalogConfig) MaxStack() int32 { return v.v_MaxStack }
// 品质：1～5
func(v *ItemCatalogConfig) Rarity() int32 { return v.v_Rarity }
// 是否启用
func(v *ItemCatalogConfig) Enabled() bool { return v.v_Enabled }
const schemaItemCatalog = "{\"name\":\"ItemCatalog\",\"hash\":\"C62E86119B370A102C131E82385650005AABD6EC76A0074028B958400781BEE0\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"道具编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"显示名称\"},{\"name\":\"MaxStack\",\"type\":\"System.Int32\",\"description\":\"堆叠上限：1～10000\"},{\"name\":\"Rarity\",\"type\":\"System.Int32\",\"description\":\"品质：1～5\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否启用\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}"
type ItemCatalogKey_Key struct {
    v_Id int32
}
type ItemCatalogKey_Group_ById struct {
    v_Id int32
}
type ItemCatalogTable struct {
    rows List[*ItemCatalogConfig]
    index_Key map[ItemCatalogKey_Key]*ItemCatalogConfig
    index_Group_ById map[ItemCatalogKey_Group_ById]List[*ItemCatalogConfig]
    ids map[int32]*ItemCatalogConfig
}
func loadItemCatalog(source []any)*ItemCatalogTable {
    result:=&ItemCatalogTable{rows:List[*ItemCatalogConfig]{values:make([]*ItemCatalogConfig,len(source))}}
    result.index_Key=make(map[ItemCatalogKey_Key]*ItemCatalogConfig,len(source))
    result.index_Group_ById=make(map[ItemCatalogKey_Group_ById]List[*ItemCatalogConfig],len(source))
    result.ids=make(map[int32]*ItemCatalogConfig,len(source))
    for i,value:=range source {
        row:=&ItemCatalogConfig{
            v_Id: int32(signed(field(value,"Id"),32)),
            v_Name: text(field(value,"Name")),
            v_MaxStack: int32(signed(field(value,"MaxStack"),32)),
            v_Rarity: int32(signed(field(value,"Rarity"),32)),
            v_Enabled: boolean(field(value,"Enabled")),
        }
        result.rows.values[i]=row
        key_Key:=ItemCatalogKey_Key{row.v_Id}
        if _,ok:=result.index_Key[key_Key];ok{invalid("主键重复")}
        result.index_Key[key_Key]=row
        key_Group_ById:=ItemCatalogKey_Group_ById{row.v_Id}
        group_Group_ById:=result.index_Group_ById[key_Group_ById]
        group_Group_ById.values=append(group_Group_ById.values,row)
        result.index_Group_ById[key_Group_ById]=group_Group_ById
        if _,ok:=result.ids[row.v_Id];ok{invalid("引用 Id 不唯一")}
        result.ids[row.v_Id]=row
    }
    return result
}
func(t *ItemCatalogTable)bind(snapshot *ConfigSnapshot){
}
func(t *ItemCatalogTable) Rows()List[*ItemCatalogConfig]{return t.rows}
func(t *ItemCatalogTable) Count()int{return t.rows.Len()}
func(t *ItemCatalogTable) Find(k0 int32)(*ItemCatalogConfig,bool){value,ok:=t.index_Key[ItemCatalogKey_Key{k0}];return value,ok}
func(t *ItemCatalogTable) Get(k0 int32)*ItemCatalogConfig{value:=t.index_Key[ItemCatalogKey_Key{k0}];if value==nil{invalid("配置主键不存在")};return value}
func(t *ItemCatalogTable) GetById(k0 int32)List[*ItemCatalogConfig]{return t.index_Group_ById[ItemCatalogKey_Group_ById{k0}]}
