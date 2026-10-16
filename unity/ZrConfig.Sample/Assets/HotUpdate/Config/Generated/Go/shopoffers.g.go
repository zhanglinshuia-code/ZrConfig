// zrconfig-api-contract:1
// 此文件由工具自动生成，请勿手工修改。
package zrconfig

type ShopOffersConfig struct {
    serverOwner *ConfigSnapshot
    v_Id int32
    v_Item int32
    ref_Item *ItemCatalogConfig
    v_Price int32
    v_DailyLimit int32
    v_Enabled bool
}
// 商品编号
func(v *ShopOffersConfig) Id() int32 { return v.v_Id }
// 引用 ItemCatalog 的 Id
func(v *ShopOffersConfig) ItemId() int32 { return v.v_Item }
func(v *ShopOffersConfig) Item() *ItemCatalogConfig { return v.ref_Item }
// 价格：非负整数；0 为免费
func(v *ShopOffersConfig) Price() int32 { return v.v_Price }
// 每日限购：0 为不限购
func(v *ShopOffersConfig) DailyLimit() int32 { return v.v_DailyLimit }
// 是否上架
func(v *ShopOffersConfig) Enabled() bool { return v.v_Enabled }
const schemaShopOffers = "{\"name\":\"ShopOffers\",\"hash\":\"0376659B4746D22E616351BEAEFC78651E69BFCEFF2D7ECAB8BD17539864E8E1\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"商品编号\"},{\"name\":\"Item\",\"type\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"description\":\"引用 ItemCatalog 的 Id\"},{\"name\":\"Price\",\"type\":\"System.Int32\",\"description\":\"价格：非负整数；0 为免费\"},{\"name\":\"DailyLimit\",\"type\":\"System.Int32\",\"description\":\"每日限购：0 为不限购\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否上架\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"ItemCatalog\"}],\"tags\":[],\"dependencies\":[\"ItemCatalog\"]}"
type ShopOffersKey_Key struct {
    v_Id int32
}
type ShopOffersKey_Group_ById struct {
    v_Id int32
}
type ShopOffersTable struct {
    rows List[*ShopOffersConfig]
    index_Key map[ShopOffersKey_Key]*ShopOffersConfig
    index_Group_ById map[ShopOffersKey_Group_ById]List[*ShopOffersConfig]
}
func loadShopOffers(source []any)*ShopOffersTable {
    result:=&ShopOffersTable{rows:List[*ShopOffersConfig]{values:make([]*ShopOffersConfig,len(source))}}
    result.index_Key=make(map[ShopOffersKey_Key]*ShopOffersConfig,len(source))
    result.index_Group_ById=make(map[ShopOffersKey_Group_ById]List[*ShopOffersConfig],len(source))
    for i,value:=range source {
        row:=&ShopOffersConfig{
            v_Id: int32(signed(field(value,"Id"),32)),
            v_Item: int32(signed(field(value,"Item"),32)),
            v_Price: int32(signed(field(value,"Price"),32)),
            v_DailyLimit: int32(signed(field(value,"DailyLimit"),32)),
            v_Enabled: boolean(field(value,"Enabled")),
        }
        result.rows.values[i]=row
        key_Key:=ShopOffersKey_Key{row.v_Id}
        if _,ok:=result.index_Key[key_Key];ok{invalid("主键重复")}
        result.index_Key[key_Key]=row
        key_Group_ById:=ShopOffersKey_Group_ById{row.v_Id}
        group_Group_ById:=result.index_Group_ById[key_Group_ById]
        group_Group_ById.values=append(group_Group_ById.values,row)
        result.index_Group_ById[key_Group_ById]=group_Group_ById
    }
    return result
}
func(t *ShopOffersTable)bind(snapshot *ConfigSnapshot){
    for _,row:=range t.rows.values{if row.v_Item!=0{row.ref_Item=snapshot.table_ItemCatalog.ids[row.v_Item];if row.ref_Item==nil{invalid("引用目标不存在：ShopOffers.Item")}}}
}
func(t *ShopOffersTable) Rows()List[*ShopOffersConfig]{return t.rows}
func(t *ShopOffersTable) Count()int{return t.rows.Len()}
func(t *ShopOffersTable) Find(k0 int32)(*ShopOffersConfig,bool){value,ok:=t.index_Key[ShopOffersKey_Key{k0}];return value,ok}
func(t *ShopOffersTable) Get(k0 int32)*ShopOffersConfig{value:=t.index_Key[ShopOffersKey_Key{k0}];if value==nil{invalid("配置主键不存在")};return value}
func(t *ShopOffersTable) GetById(k0 int32)List[*ShopOffersConfig]{return t.index_Group_ById[ShopOffersKey_Group_ById{k0}]}
