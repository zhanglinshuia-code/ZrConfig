// zrconfig-api-contract:1
// 完整候选加载和引用校验成功后才发布。请求持有 Arc 快照，旧版本自然释放。
use std::path::Path;
use std::sync::{Arc, Mutex, RwLock};
use super::runtime::{self as R, Result};
use super::table_AllTypes::AllTypesTable;
use super::table_AllTypes10k::AllTypes10kTable;
use super::table_Drop::DropTable;
use super::table_Hero::HeroTable;
use super::table_ItemCatalog::ItemCatalogTable;
use super::table_QuestRewards::QuestRewardsTable;
use super::table_ShopOffers::ShopOffersTable;
use super::table_Skill::SkillTable;
use super::table_SkillLevel::SkillLevelTable;
pub struct ConfigSnapshot {
    version: String,
    pub(super) table_AllTypes: Option<AllTypesTable>,
    pub(super) table_AllTypes10k: Option<AllTypes10kTable>,
    pub(super) table_Drop: Option<DropTable>,
    pub(super) table_Hero: Option<HeroTable>,
    pub(super) table_ItemCatalog: Option<ItemCatalogTable>,
    pub(super) table_QuestRewards: Option<QuestRewardsTable>,
    pub(super) table_ShopOffers: Option<ShopOffersTable>,
    pub(super) table_Skill: Option<SkillTable>,
    pub(super) table_SkillLevel: Option<SkillLevelTable>,
}
impl ConfigSnapshot {
    pub fn version(&self) -> &str { &self.version }
    pub fn get_AllTypes(&self) -> &AllTypesTable { self.table_AllTypes.as_ref().expect("Legacy view requires the complete catalog") }
    pub fn get_AllTypes10k(&self) -> &AllTypes10kTable { self.table_AllTypes10k.as_ref().expect("Legacy view requires the complete catalog") }
    pub fn get_Drop(&self) -> &DropTable { self.table_Drop.as_ref().expect("Legacy view requires the complete catalog") }
    pub fn get_Hero(&self) -> &HeroTable { self.table_Hero.as_ref().expect("Legacy view requires the complete catalog") }
    pub fn get_ItemCatalog(&self) -> &ItemCatalogTable { self.table_ItemCatalog.as_ref().expect("Legacy view requires the complete catalog") }
    pub fn get_QuestRewards(&self) -> &QuestRewardsTable { self.table_QuestRewards.as_ref().expect("Legacy view requires the complete catalog") }
    pub fn get_ShopOffers(&self) -> &ShopOffersTable { self.table_ShopOffers.as_ref().expect("Legacy view requires the complete catalog") }
    pub fn get_Skill(&self) -> &SkillTable { self.table_Skill.as_ref().expect("Legacy view requires the complete catalog") }
    pub fn get_SkillLevel(&self) -> &SkillLevelTable { self.table_SkillLevel.as_ref().expect("Legacy view requires the complete catalog") }
    pub fn load(root: &Path, prefer_json: bool) -> Result<Arc<Self>> {
        let bundle = R::Bundle::open(root)?;
        bundle.validate_tables(&["AllTypes", "AllTypes10k", "Drop", "Hero", "ItemCatalog", "QuestRewards", "ShopOffers", "Skill", "SkillLevel"])?;
        Self::__zr_load_selected(&bundle, prefer_json, None)
    }
    pub(super) fn __zr_load_selected(bundle: &R::Bundle, _prefer_json: bool, _selected: Option<&std::collections::HashSet<&str>>) -> Result<Arc<Self>> {
        let next = Arc::new(Self {
            version: bundle.version.clone(),
            table_AllTypes: if _selected.map_or(true, |s| s.contains("AllTypes")) { Some(AllTypesTable::__zr_load(bundle, _prefer_json)?) } else { None },
            table_AllTypes10k: if _selected.map_or(true, |s| s.contains("AllTypes10k")) { Some(AllTypes10kTable::__zr_load(bundle, _prefer_json)?) } else { None },
            table_Drop: if _selected.map_or(true, |s| s.contains("Drop")) { Some(DropTable::__zr_load(bundle, _prefer_json)?) } else { None },
            table_Hero: if _selected.map_or(true, |s| s.contains("Hero")) { Some(HeroTable::__zr_load(bundle, _prefer_json)?) } else { None },
            table_ItemCatalog: if _selected.map_or(true, |s| s.contains("ItemCatalog")) { Some(ItemCatalogTable::__zr_load(bundle, _prefer_json)?) } else { None },
            table_QuestRewards: if _selected.map_or(true, |s| s.contains("QuestRewards")) { Some(QuestRewardsTable::__zr_load(bundle, _prefer_json)?) } else { None },
            table_ShopOffers: if _selected.map_or(true, |s| s.contains("ShopOffers")) { Some(ShopOffersTable::__zr_load(bundle, _prefer_json)?) } else { None },
            table_Skill: if _selected.map_or(true, |s| s.contains("Skill")) { Some(SkillTable::__zr_load(bundle, _prefer_json)?) } else { None },
            table_SkillLevel: if _selected.map_or(true, |s| s.contains("SkillLevel")) { Some(SkillLevelTable::__zr_load(bundle, _prefer_json)?) } else { None },
        });
        if let Some(table) = &next.table_AllTypes { table.__zr_bind(&next)?; }
        if let Some(table) = &next.table_AllTypes10k { table.__zr_bind(&next)?; }
        if let Some(table) = &next.table_Drop { table.__zr_bind(&next)?; }
        if let Some(table) = &next.table_Hero { table.__zr_bind(&next)?; }
        if let Some(table) = &next.table_ItemCatalog { table.__zr_bind(&next)?; }
        if let Some(table) = &next.table_QuestRewards { table.__zr_bind(&next)?; }
        if let Some(table) = &next.table_ShopOffers { table.__zr_bind(&next)?; }
        if let Some(table) = &next.table_Skill { table.__zr_bind(&next)?; }
        if let Some(table) = &next.table_SkillLevel { table.__zr_bind(&next)?; }
        Ok(next)
    }
}
#[derive(Default)]
pub struct ConfigManager { gate: Mutex<()>, current: RwLock<Option<Arc<ConfigSnapshot>>> }
impl ConfigManager {
    pub fn new() -> Self { Self::default() }
    pub fn current(&self) -> Result<Arc<ConfigSnapshot>> { self.current.read().map_err(|e| e.to_string())?.clone().ok_or_else(|| "配置尚未加载".to_owned()) }
    pub fn reload(&self, root: &Path, prefer_json: bool) -> Result<Arc<ConfigSnapshot>> {
        let _guard = self.gate.lock().map_err(|e| e.to_string())?;
        let next = ConfigSnapshot::load(root, prefer_json)?;
        *self.current.write().map_err(|e| e.to_string())? = Some(next.clone());
        Ok(next)
    }
}

