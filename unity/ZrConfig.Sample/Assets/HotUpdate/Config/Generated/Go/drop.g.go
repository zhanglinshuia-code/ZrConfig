// zrconfig-api-contract:1
// 此文件由工具自动生成，请勿手工修改。
package zrconfig

type DropConfig struct {
    serverOwner *ConfigSnapshot
    v_GroupId int32
    v_Id int32
    v_Count int32
}
// 掉落组编号
func(v *DropConfig) GroupId() int32 { return v.v_GroupId }
// 物品编号
func(v *DropConfig) Id() int32 { return v.v_Id }
// 数量
func(v *DropConfig) Count() int32 { return v.v_Count }
const schemaDrop = "{\"name\":\"Drop\",\"hash\":\"9DAF981CB794247A5F399C8DFA97320D99D670CC605BE1E071D33E596021A242\",\"hasId\":true,\"fields\":[{\"name\":\"GroupId\",\"type\":\"System.Int32\",\"description\":\"掉落组编号\"},{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"物品编号\"},{\"name\":\"Count\",\"type\":\"System.Int32\",\"description\":\"数量\"}],\"keys\":[\"GroupId\",\"Id\"],\"groups\":[{\"name\":\"ByGroupId\",\"fields\":[\"GroupId\"]},{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}"
type DropKey_Key struct {
    v_GroupId int32
    v_Id int32
}
type DropKey_Group_ByGroupId struct {
    v_GroupId int32
}
type DropKey_Group_ById struct {
    v_Id int32
}
type DropTable struct {
    rows List[*DropConfig]
    index_Key map[DropKey_Key]*DropConfig
    index_Group_ByGroupId map[DropKey_Group_ByGroupId]List[*DropConfig]
    index_Group_ById map[DropKey_Group_ById]List[*DropConfig]
}
func loadDrop(source []any)*DropTable {
    result:=&DropTable{rows:List[*DropConfig]{values:make([]*DropConfig,len(source))}}
    result.index_Key=make(map[DropKey_Key]*DropConfig,len(source))
    result.index_Group_ByGroupId=make(map[DropKey_Group_ByGroupId]List[*DropConfig],len(source))
    result.index_Group_ById=make(map[DropKey_Group_ById]List[*DropConfig],len(source))
    for i,value:=range source {
        row:=&DropConfig{
            v_GroupId: int32(signed(field(value,"GroupId"),32)),
            v_Id: int32(signed(field(value,"Id"),32)),
            v_Count: int32(signed(field(value,"Count"),32)),
        }
        result.rows.values[i]=row
        key_Key:=DropKey_Key{row.v_GroupId,row.v_Id}
        if _,ok:=result.index_Key[key_Key];ok{invalid("主键重复")}
        result.index_Key[key_Key]=row
        key_Group_ByGroupId:=DropKey_Group_ByGroupId{row.v_GroupId}
        group_Group_ByGroupId:=result.index_Group_ByGroupId[key_Group_ByGroupId]
        group_Group_ByGroupId.values=append(group_Group_ByGroupId.values,row)
        result.index_Group_ByGroupId[key_Group_ByGroupId]=group_Group_ByGroupId
        key_Group_ById:=DropKey_Group_ById{row.v_Id}
        group_Group_ById:=result.index_Group_ById[key_Group_ById]
        group_Group_ById.values=append(group_Group_ById.values,row)
        result.index_Group_ById[key_Group_ById]=group_Group_ById
    }
    return result
}
func(t *DropTable)bind(snapshot *ConfigSnapshot){
}
func(t *DropTable) Rows()List[*DropConfig]{return t.rows}
func(t *DropTable) Count()int{return t.rows.Len()}
func(t *DropTable) Find(k0 int32,k1 int32)(*DropConfig,bool){value,ok:=t.index_Key[DropKey_Key{k0,k1}];return value,ok}
func(t *DropTable) Get(k0 int32,k1 int32)*DropConfig{value:=t.index_Key[DropKey_Key{k0,k1}];if value==nil{invalid("配置主键不存在")};return value}
func(t *DropTable) GetByGroupId(k0 int32)List[*DropConfig]{return t.index_Group_ByGroupId[DropKey_Group_ByGroupId{k0}]}
func(t *DropTable) GetById(k0 int32)List[*DropConfig]{return t.index_Group_ById[DropKey_Group_ById{k0}]}
