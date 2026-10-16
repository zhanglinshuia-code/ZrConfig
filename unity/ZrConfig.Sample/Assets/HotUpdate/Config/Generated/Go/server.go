// zrconfig-api-contract:1
// Server entry: rows retain their complete original snapshot.
package zrconfig

type ServerConfigManager = ServerManager[ConfigSnapshot]
func LoadServerConfig(root string, preferJSON bool)(*ConfigSnapshot,error){
    snapshot,err:=LoadConfig(root,preferJSON);if err!=nil{return nil,err}
    for _,row:=range snapshot.table_AllTypes.rows.values{row.serverOwner=snapshot}
    for _,row:=range snapshot.table_AllTypes10k.rows.values{row.serverOwner=snapshot}
    for _,row:=range snapshot.table_Drop.rows.values{row.serverOwner=snapshot}
    for _,row:=range snapshot.table_Hero.rows.values{row.serverOwner=snapshot}
    for _,row:=range snapshot.table_ItemCatalog.rows.values{row.serverOwner=snapshot}
    for _,row:=range snapshot.table_QuestRewards.rows.values{row.serverOwner=snapshot}
    for _,row:=range snapshot.table_ShopOffers.rows.values{row.serverOwner=snapshot}
    for _,row:=range snapshot.table_Skill.rows.values{row.serverOwner=snapshot}
    for _,row:=range snapshot.table_SkillLevel.rows.values{row.serverOwner=snapshot}
    return snapshot,nil
}
var serverCatalog = newCatalog("{\"format\":2,\"fingerprint\":\"ADE42D7364E51BC5A62B63F35E32A11E50424B1A690E12E56AFD23760D81DDC6\",\"tables\":[{\"name\":\"AllTypes\",\"schemaHash\":\"E8017397D24CBD0FDA2008AFD4F2BA2BA68619A29F4D4D79F7973F06BC8360B1\",\"dependencies\":[],\"component\":0},{\"name\":\"AllTypes10k\",\"schemaHash\":\"34B94C4229D85CC8B37832A203504B7E6EF4447307ADB831AB5BD01628EA7432\",\"dependencies\":[\"AllTypes10k\",\"Hero\",\"Skill\"],\"component\":1},{\"name\":\"Drop\",\"schemaHash\":\"9DAF981CB794247A5F399C8DFA97320D99D670CC605BE1E071D33E596021A242\",\"dependencies\":[],\"component\":2},{\"name\":\"Hero\",\"schemaHash\":\"7E1E75AC24BAFD42C18F1B43A19B0027A3D241CC665DB3727BC98A4908927ECE\",\"dependencies\":[\"Hero\",\"Skill\"],\"component\":3},{\"name\":\"ItemCatalog\",\"schemaHash\":\"C62E86119B370A102C131E82385650005AABD6EC76A0074028B958400781BEE0\",\"dependencies\":[],\"component\":4},{\"name\":\"QuestRewards\",\"schemaHash\":\"C839B7EDFA92304709824DABBA84B5534E34810224D2814C221E6F4A3B536E35\",\"dependencies\":[\"ItemCatalog\"],\"component\":5},{\"name\":\"ShopOffers\",\"schemaHash\":\"0376659B4746D22E616351BEAEFC78651E69BFCEFF2D7ECAB8BD17539864E8E1\",\"dependencies\":[\"ItemCatalog\"],\"component\":6},{\"name\":\"Skill\",\"schemaHash\":\"51DA7601418A98574B1C5ADC656CB7E7B479CC1F3E5237DFC5BA7532F702B9DB\",\"dependencies\":[\"Hero\"],\"component\":3},{\"name\":\"SkillLevel\",\"schemaHash\":\"9DDEDF4A13E49BE883FA11B62A966463D9B27C99620857DE5CD5E721CECC92B2\",\"dependencies\":[],\"component\":7}],\"components\":[[\"AllTypes\"],[\"AllTypes10k\"],[\"Drop\"],[\"Hero\",\"Skill\"],[\"ItemCatalog\"],[\"QuestRewards\"],[\"ShopOffers\"],[\"SkillLevel\"]],\"target\":\"C\",\"runtimeContract\":\"server-v1\",\"requiredCapabilities\":[\"binary-v4\",\"json-v1\",\"server-default\",\"selected-closure\",\"lazy-strict\",\"lazy-on-access\",\"fixed-source-bytes\",\"snapshot-owned-handles\",\"explicit-reference-load\",\"row-cache\",\"group-cache\",\"typed-warmup\",\"bounded-source\",\"structured-errors\",\"manager-lifecycle\"]}")
func ConfigurationCatalog()*Catalog{return serverCatalog}
type SelectedSnapshot struct{storage *ConfigSnapshot;selection SelectionPlan}
func(s *SelectedSnapshot)Version()string{return s.storage.version}
func(s *SelectedSnapshot)Selection()SelectionPlan{return s.selection}
func LoadSelectedConfig(root string,requested []string,preferJSON bool)(snapshot *SelectedSnapshot,err error){
    defer recoverError(&err)
    plan,err:=serverCatalog.Select(requested);if err!=nil{return nil,err}
    b:=openBundle(root);if err=b.validateCatalog(serverCatalog);err!=nil{return nil,err}
    selected:=map[string]bool{};for _,name:=range plan.tables.values{selected[name]=true}
    storage:=&ConfigSnapshot{version:b.version}
    if selected["AllTypes"]{storage.table_AllTypes=loadAllTypes(b.load(schemaAllTypes,preferJSON))}
    if selected["AllTypes10k"]{storage.table_AllTypes10k=loadAllTypes10k(b.load(schemaAllTypes10k,preferJSON))}
    if selected["Drop"]{storage.table_Drop=loadDrop(b.load(schemaDrop,preferJSON))}
    if selected["Hero"]{storage.table_Hero=loadHero(b.load(schemaHero,preferJSON))}
    if selected["ItemCatalog"]{storage.table_ItemCatalog=loadItemCatalog(b.load(schemaItemCatalog,preferJSON))}
    if selected["QuestRewards"]{storage.table_QuestRewards=loadQuestRewards(b.load(schemaQuestRewards,preferJSON))}
    if selected["ShopOffers"]{storage.table_ShopOffers=loadShopOffers(b.load(schemaShopOffers,preferJSON))}
    if selected["Skill"]{storage.table_Skill=loadSkill(b.load(schemaSkill,preferJSON))}
    if selected["SkillLevel"]{storage.table_SkillLevel=loadSkillLevel(b.load(schemaSkillLevel,preferJSON))}
    if selected["AllTypes"]{storage.table_AllTypes.bind(storage);for _,row:=range storage.table_AllTypes.rows.values{row.serverOwner=storage}}
    if selected["AllTypes10k"]{storage.table_AllTypes10k.bind(storage);for _,row:=range storage.table_AllTypes10k.rows.values{row.serverOwner=storage}}
    if selected["Drop"]{storage.table_Drop.bind(storage);for _,row:=range storage.table_Drop.rows.values{row.serverOwner=storage}}
    if selected["Hero"]{storage.table_Hero.bind(storage);for _,row:=range storage.table_Hero.rows.values{row.serverOwner=storage}}
    if selected["ItemCatalog"]{storage.table_ItemCatalog.bind(storage);for _,row:=range storage.table_ItemCatalog.rows.values{row.serverOwner=storage}}
    if selected["QuestRewards"]{storage.table_QuestRewards.bind(storage);for _,row:=range storage.table_QuestRewards.rows.values{row.serverOwner=storage}}
    if selected["ShopOffers"]{storage.table_ShopOffers.bind(storage);for _,row:=range storage.table_ShopOffers.rows.values{row.serverOwner=storage}}
    if selected["Skill"]{storage.table_Skill.bind(storage);for _,row:=range storage.table_Skill.rows.values{row.serverOwner=storage}}
    if selected["SkillLevel"]{storage.table_SkillLevel.bind(storage);for _,row:=range storage.table_SkillLevel.rows.values{row.serverOwner=storage}}
    return &SelectedSnapshot{storage:storage,selection:plan},nil
}
func(s *SelectedSnapshot)Get_AllTypes()(*AllTypesTable,error){if s.storage.table_AllTypes==nil{return nil,serverError(TableNotSelected)};return s.storage.table_AllTypes,nil}
func(s *SelectedSnapshot)Get_AllTypes10k()(*AllTypes10kTable,error){if s.storage.table_AllTypes10k==nil{return nil,serverError(TableNotSelected)};return s.storage.table_AllTypes10k,nil}
func(s *SelectedSnapshot)Get_Drop()(*DropTable,error){if s.storage.table_Drop==nil{return nil,serverError(TableNotSelected)};return s.storage.table_Drop,nil}
func(s *SelectedSnapshot)Get_Hero()(*HeroTable,error){if s.storage.table_Hero==nil{return nil,serverError(TableNotSelected)};return s.storage.table_Hero,nil}
func(s *SelectedSnapshot)Get_ItemCatalog()(*ItemCatalogTable,error){if s.storage.table_ItemCatalog==nil{return nil,serverError(TableNotSelected)};return s.storage.table_ItemCatalog,nil}
func(s *SelectedSnapshot)Get_QuestRewards()(*QuestRewardsTable,error){if s.storage.table_QuestRewards==nil{return nil,serverError(TableNotSelected)};return s.storage.table_QuestRewards,nil}
func(s *SelectedSnapshot)Get_ShopOffers()(*ShopOffersTable,error){if s.storage.table_ShopOffers==nil{return nil,serverError(TableNotSelected)};return s.storage.table_ShopOffers,nil}
func(s *SelectedSnapshot)Get_Skill()(*SkillTable,error){if s.storage.table_Skill==nil{return nil,serverError(TableNotSelected)};return s.storage.table_Skill,nil}
func(s *SelectedSnapshot)Get_SkillLevel()(*SkillLevelTable,error){if s.storage.table_SkillLevel==nil{return nil,serverError(TableNotSelected)};return s.storage.table_SkillLevel,nil}
