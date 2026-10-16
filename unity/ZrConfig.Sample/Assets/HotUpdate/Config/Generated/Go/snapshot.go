// zrconfig-api-contract:1
// 完整候选快照构建成功后才提交，已有请求继续持有旧快照。
package zrconfig
import ("sync";"sync/atomic")
type ConfigSnapshot struct {
    version string
    table_AllTypes *AllTypesTable
    table_AllTypes10k *AllTypes10kTable
    table_Drop *DropTable
    table_Hero *HeroTable
    table_ItemCatalog *ItemCatalogTable
    table_QuestRewards *QuestRewardsTable
    table_ShopOffers *ShopOffersTable
    table_Skill *SkillTable
    table_SkillLevel *SkillLevelTable
}
func(s *ConfigSnapshot) Version() string { return s.version }
func(s *ConfigSnapshot) AllTypes() *AllTypesTable { return s.table_AllTypes }
func(s *ConfigSnapshot) AllTypes10k() *AllTypes10kTable { return s.table_AllTypes10k }
func(s *ConfigSnapshot) Drop() *DropTable { return s.table_Drop }
func(s *ConfigSnapshot) Hero() *HeroTable { return s.table_Hero }
func(s *ConfigSnapshot) ItemCatalog() *ItemCatalogTable { return s.table_ItemCatalog }
func(s *ConfigSnapshot) QuestRewards() *QuestRewardsTable { return s.table_QuestRewards }
func(s *ConfigSnapshot) ShopOffers() *ShopOffersTable { return s.table_ShopOffers }
func(s *ConfigSnapshot) Skill() *SkillTable { return s.table_Skill }
func(s *ConfigSnapshot) SkillLevel() *SkillLevelTable { return s.table_SkillLevel }
func LoadConfig(root string, preferJSON bool)(snapshot *ConfigSnapshot,err error) {
    defer recoverError(&err)
    b:=openBundle(root)
    next:=&ConfigSnapshot{version:b.version}
    b.validateTables([]string{"AllTypes", "AllTypes10k", "Drop", "Hero", "ItemCatalog", "QuestRewards", "ShopOffers", "Skill", "SkillLevel"})
    next.table_AllTypes=loadAllTypes(b.load(schemaAllTypes,preferJSON))
    next.table_AllTypes10k=loadAllTypes10k(b.load(schemaAllTypes10k,preferJSON))
    next.table_Drop=loadDrop(b.load(schemaDrop,preferJSON))
    next.table_Hero=loadHero(b.load(schemaHero,preferJSON))
    next.table_ItemCatalog=loadItemCatalog(b.load(schemaItemCatalog,preferJSON))
    next.table_QuestRewards=loadQuestRewards(b.load(schemaQuestRewards,preferJSON))
    next.table_ShopOffers=loadShopOffers(b.load(schemaShopOffers,preferJSON))
    next.table_Skill=loadSkill(b.load(schemaSkill,preferJSON))
    next.table_SkillLevel=loadSkillLevel(b.load(schemaSkillLevel,preferJSON))
    next.table_AllTypes.bind(next)
    next.table_AllTypes10k.bind(next)
    next.table_Drop.bind(next)
    next.table_Hero.bind(next)
    next.table_ItemCatalog.bind(next)
    next.table_QuestRewards.bind(next)
    next.table_ShopOffers.bind(next)
    next.table_Skill.bind(next)
    next.table_SkillLevel.bind(next)
    return next,nil
}
type ConfigManager struct { gate sync.Mutex; current atomic.Pointer[ConfigSnapshot] }
func(m *ConfigManager) Current()*ConfigSnapshot { return m.current.Load() }
func(m *ConfigManager) Reload(root string,preferJSON bool)(*ConfigSnapshot,error){m.gate.Lock();defer m.gate.Unlock();next,err:=LoadConfig(root,preferJSON);if err!=nil{return nil,err};m.current.Store(next);return next,nil}

