// zrconfig-api-contract:1
// Server API family 1: owning handles retain the complete original snapshot.
use std::path::Path;
use std::sync::Arc;
use super::runtime::{self as R, ServerResult, ConfigError, ErrorCode};
use super::snapshot::ConfigSnapshot;
use super::types as T;
pub struct Snapshot { storage: Arc<ConfigSnapshot>, selection: R::SelectionPlan, complete: bool }
pub type Manager = R::ServerManager<Snapshot>;
impl Snapshot {
    pub fn load(root: &Path, prefer_json: bool) -> ServerResult<Arc<Self>> {
        let names = Self::catalog()?.tables().iter().map(|t|t.name()).collect::<Vec<_>>();
        Self::load_selected(root, &names, prefer_json)
    }
    pub fn borrow(&self) -> ServerResult<&ConfigSnapshot> { if !self.complete { return Err(ConfigError::new(ErrorCode::UnsupportedCapability, "Partial snapshot has no complete legacy view")); } Ok(&self.storage) }
    pub fn version(&self) -> &str { self.storage.version() }
    pub fn catalog() -> ServerResult<&'static R::Catalog> { static CATALOG: std::sync::OnceLock<ServerResult<R::Catalog>> = std::sync::OnceLock::new(); CATALOG.get_or_init(||R::Catalog::parse("{\"format\":2,\"fingerprint\":\"ADE42D7364E51BC5A62B63F35E32A11E50424B1A690E12E56AFD23760D81DDC6\",\"tables\":[{\"name\":\"AllTypes\",\"schemaHash\":\"E8017397D24CBD0FDA2008AFD4F2BA2BA68619A29F4D4D79F7973F06BC8360B1\",\"dependencies\":[],\"component\":0},{\"name\":\"AllTypes10k\",\"schemaHash\":\"34B94C4229D85CC8B37832A203504B7E6EF4447307ADB831AB5BD01628EA7432\",\"dependencies\":[\"AllTypes10k\",\"Hero\",\"Skill\"],\"component\":1},{\"name\":\"Drop\",\"schemaHash\":\"9DAF981CB794247A5F399C8DFA97320D99D670CC605BE1E071D33E596021A242\",\"dependencies\":[],\"component\":2},{\"name\":\"Hero\",\"schemaHash\":\"7E1E75AC24BAFD42C18F1B43A19B0027A3D241CC665DB3727BC98A4908927ECE\",\"dependencies\":[\"Hero\",\"Skill\"],\"component\":3},{\"name\":\"ItemCatalog\",\"schemaHash\":\"C62E86119B370A102C131E82385650005AABD6EC76A0074028B958400781BEE0\",\"dependencies\":[],\"component\":4},{\"name\":\"QuestRewards\",\"schemaHash\":\"C839B7EDFA92304709824DABBA84B5534E34810224D2814C221E6F4A3B536E35\",\"dependencies\":[\"ItemCatalog\"],\"component\":5},{\"name\":\"ShopOffers\",\"schemaHash\":\"0376659B4746D22E616351BEAEFC78651E69BFCEFF2D7ECAB8BD17539864E8E1\",\"dependencies\":[\"ItemCatalog\"],\"component\":6},{\"name\":\"Skill\",\"schemaHash\":\"51DA7601418A98574B1C5ADC656CB7E7B479CC1F3E5237DFC5BA7532F702B9DB\",\"dependencies\":[\"Hero\"],\"component\":3},{\"name\":\"SkillLevel\",\"schemaHash\":\"9DDEDF4A13E49BE883FA11B62A966463D9B27C99620857DE5CD5E721CECC92B2\",\"dependencies\":[],\"component\":7}],\"components\":[[\"AllTypes\"],[\"AllTypes10k\"],[\"Drop\"],[\"Hero\",\"Skill\"],[\"ItemCatalog\"],[\"QuestRewards\"],[\"ShopOffers\"],[\"SkillLevel\"]],\"target\":\"C\",\"runtimeContract\":\"server-v1\",\"requiredCapabilities\":[\"binary-v4\",\"json-v1\",\"server-default\",\"selected-closure\",\"lazy-strict\",\"lazy-on-access\",\"fixed-source-bytes\",\"snapshot-owned-handles\",\"explicit-reference-load\",\"row-cache\",\"group-cache\",\"typed-warmup\",\"bounded-source\",\"structured-errors\",\"manager-lifecycle\"]}")).as_ref().map_err(Clone::clone) }
    pub fn selection(&self) -> &R::SelectionPlan { &self.selection }
    pub fn load_selected(root: &Path, requested: &[&str], prefer_json: bool) -> ServerResult<Arc<Self>> {
        let catalog = Self::catalog()?; let selection = catalog.select(requested)?;
        let bundle = R::Bundle::open(root).map_err(|message|ConfigError::new(ErrorCode::InvalidManifest,message))?; bundle.validate_catalog(catalog)?;
        let names = selection.tables().iter().map(|s|s.as_str()).collect::<std::collections::HashSet<_>>();
        let storage = ConfigSnapshot::__zr_load_selected(&bundle, prefer_json, Some(&names)).map_err(|message|ConfigError::new(ErrorCode::InvalidValue,message))?;
        let complete = names.len() == catalog.tables().len();
        Ok(Arc::new(Self { storage, selection, complete }))
    }
    pub fn find_AllTypes(self: &Arc<Self>, k0: i32) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_AllTypes::AllTypesConfig>>> {
        let table = self.storage.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "AllTypes: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Key.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(slot.map(|s| R::RowHandle { snapshot: self.clone(), row: table.rows[table.index_Key.row(s)].clone() }))
    }
    pub fn get_AllTypes(self: &Arc<Self>, k0: i32) -> ServerResult<R::RowHandle<Snapshot, super::table_AllTypes::AllTypesConfig>> { self.find_AllTypes(k0)?.ok_or_else(|| ConfigError::new(ErrorCode::KeyNotFound, "AllTypes: key not found")) }
    pub fn group_AllTypes_ById(self: &Arc<Self>, k0: i32) -> ServerResult<R::GroupHandle<Snapshot, super::table_AllTypes::AllTypesConfig>> {
        let table = self.storage.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "AllTypes: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_ById.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_AllTypes().rows, positions: |s, slot| s.storage.get_AllTypes().index_Group_ById.group(slot), slot })
    }
    pub fn group_AllTypes_ByQuality(self: &Arc<Self>, k0: T::T_Game_Config_Quality) -> ServerResult<R::GroupHandle<Snapshot, super::table_AllTypes::AllTypesConfig>> {
        let table = self.storage.table_AllTypes.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "AllTypes: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_ByQuality.find(R::finish_hash(h), |i| table.rows[i].v_QualityValue == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_AllTypes().rows, positions: |s, slot| s.storage.get_AllTypes().index_Group_ByQuality.group(slot), slot })
    }
    pub fn find_AllTypes10k(self: &Arc<Self>, k0: i32) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_AllTypes10k::AllTypes10kConfig>>> {
        let table = self.storage.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "AllTypes10k: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Key.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(slot.map(|s| R::RowHandle { snapshot: self.clone(), row: table.rows[table.index_Key.row(s)].clone() }))
    }
    pub fn get_AllTypes10k(self: &Arc<Self>, k0: i32) -> ServerResult<R::RowHandle<Snapshot, super::table_AllTypes10k::AllTypes10kConfig>> { self.find_AllTypes10k(k0)?.ok_or_else(|| ConfigError::new(ErrorCode::KeyNotFound, "AllTypes10k: key not found")) }
    pub fn group_AllTypes10k_ById(self: &Arc<Self>, k0: i32) -> ServerResult<R::GroupHandle<Snapshot, super::table_AllTypes10k::AllTypes10kConfig>> {
        let table = self.storage.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "AllTypes10k: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_ById.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_AllTypes10k().rows, positions: |s, slot| s.storage.get_AllTypes10k().index_Group_ById.group(slot), slot })
    }
    pub fn find_Drop(self: &Arc<Self>, k0: i32, k1: i32) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_Drop::DropConfig>>> {
        let table = self.storage.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Drop: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0); R::hash_part(&mut h, &k1);
        let slot=table.index_Key.find(R::finish_hash(h), |i| table.rows[i].v_GroupId == k0 && table.rows[i].v_Id == k1);
        Ok(slot.map(|s| R::RowHandle { snapshot: self.clone(), row: table.rows[table.index_Key.row(s)].clone() }))
    }
    pub fn get_Drop(self: &Arc<Self>, k0: i32, k1: i32) -> ServerResult<R::RowHandle<Snapshot, super::table_Drop::DropConfig>> { self.find_Drop(k0, k1)?.ok_or_else(|| ConfigError::new(ErrorCode::KeyNotFound, "Drop: key not found")) }
    pub fn group_Drop_ByGroupId(self: &Arc<Self>, k0: i32) -> ServerResult<R::GroupHandle<Snapshot, super::table_Drop::DropConfig>> {
        let table = self.storage.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Drop: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_ByGroupId.find(R::finish_hash(h), |i| table.rows[i].v_GroupId == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_Drop().rows, positions: |s, slot| s.storage.get_Drop().index_Group_ByGroupId.group(slot), slot })
    }
    pub fn group_Drop_ById(self: &Arc<Self>, k0: i32) -> ServerResult<R::GroupHandle<Snapshot, super::table_Drop::DropConfig>> {
        let table = self.storage.table_Drop.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Drop: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_ById.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_Drop().rows, positions: |s, slot| s.storage.get_Drop().index_Group_ById.group(slot), slot })
    }
    pub fn find_Hero(self: &Arc<Self>, k0: i32) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_Hero::HeroConfig>>> {
        let table = self.storage.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Hero: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Key.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(slot.map(|s| R::RowHandle { snapshot: self.clone(), row: table.rows[table.index_Key.row(s)].clone() }))
    }
    pub fn get_Hero(self: &Arc<Self>, k0: i32) -> ServerResult<R::RowHandle<Snapshot, super::table_Hero::HeroConfig>> { self.find_Hero(k0)?.ok_or_else(|| ConfigError::new(ErrorCode::KeyNotFound, "Hero: key not found")) }
    pub fn group_Hero_ById(self: &Arc<Self>, k0: i32) -> ServerResult<R::GroupHandle<Snapshot, super::table_Hero::HeroConfig>> {
        let table = self.storage.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Hero: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_ById.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_Hero().rows, positions: |s, slot| s.storage.get_Hero().index_Group_ById.group(slot), slot })
    }
    pub fn find_ItemCatalog(self: &Arc<Self>, k0: i32) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_ItemCatalog::ItemCatalogConfig>>> {
        let table = self.storage.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "ItemCatalog: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Key.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(slot.map(|s| R::RowHandle { snapshot: self.clone(), row: table.rows[table.index_Key.row(s)].clone() }))
    }
    pub fn get_ItemCatalog(self: &Arc<Self>, k0: i32) -> ServerResult<R::RowHandle<Snapshot, super::table_ItemCatalog::ItemCatalogConfig>> { self.find_ItemCatalog(k0)?.ok_or_else(|| ConfigError::new(ErrorCode::KeyNotFound, "ItemCatalog: key not found")) }
    pub fn group_ItemCatalog_ById(self: &Arc<Self>, k0: i32) -> ServerResult<R::GroupHandle<Snapshot, super::table_ItemCatalog::ItemCatalogConfig>> {
        let table = self.storage.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "ItemCatalog: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_ById.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_ItemCatalog().rows, positions: |s, slot| s.storage.get_ItemCatalog().index_Group_ById.group(slot), slot })
    }
    pub fn find_QuestRewards(self: &Arc<Self>, k0: i32) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_QuestRewards::QuestRewardsConfig>>> {
        let table = self.storage.table_QuestRewards.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "QuestRewards: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Key.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(slot.map(|s| R::RowHandle { snapshot: self.clone(), row: table.rows[table.index_Key.row(s)].clone() }))
    }
    pub fn get_QuestRewards(self: &Arc<Self>, k0: i32) -> ServerResult<R::RowHandle<Snapshot, super::table_QuestRewards::QuestRewardsConfig>> { self.find_QuestRewards(k0)?.ok_or_else(|| ConfigError::new(ErrorCode::KeyNotFound, "QuestRewards: key not found")) }
    pub fn group_QuestRewards_ById(self: &Arc<Self>, k0: i32) -> ServerResult<R::GroupHandle<Snapshot, super::table_QuestRewards::QuestRewardsConfig>> {
        let table = self.storage.table_QuestRewards.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "QuestRewards: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_ById.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_QuestRewards().rows, positions: |s, slot| s.storage.get_QuestRewards().index_Group_ById.group(slot), slot })
    }
    pub fn find_ShopOffers(self: &Arc<Self>, k0: i32) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_ShopOffers::ShopOffersConfig>>> {
        let table = self.storage.table_ShopOffers.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "ShopOffers: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Key.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(slot.map(|s| R::RowHandle { snapshot: self.clone(), row: table.rows[table.index_Key.row(s)].clone() }))
    }
    pub fn get_ShopOffers(self: &Arc<Self>, k0: i32) -> ServerResult<R::RowHandle<Snapshot, super::table_ShopOffers::ShopOffersConfig>> { self.find_ShopOffers(k0)?.ok_or_else(|| ConfigError::new(ErrorCode::KeyNotFound, "ShopOffers: key not found")) }
    pub fn group_ShopOffers_ById(self: &Arc<Self>, k0: i32) -> ServerResult<R::GroupHandle<Snapshot, super::table_ShopOffers::ShopOffersConfig>> {
        let table = self.storage.table_ShopOffers.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "ShopOffers: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_ById.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_ShopOffers().rows, positions: |s, slot| s.storage.get_ShopOffers().index_Group_ById.group(slot), slot })
    }
    pub fn find_Skill(self: &Arc<Self>, k0: i32) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_Skill::SkillConfig>>> {
        let table = self.storage.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Skill: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Key.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(slot.map(|s| R::RowHandle { snapshot: self.clone(), row: table.rows[table.index_Key.row(s)].clone() }))
    }
    pub fn get_Skill(self: &Arc<Self>, k0: i32) -> ServerResult<R::RowHandle<Snapshot, super::table_Skill::SkillConfig>> { self.find_Skill(k0)?.ok_or_else(|| ConfigError::new(ErrorCode::KeyNotFound, "Skill: key not found")) }
    pub fn group_Skill_ById(self: &Arc<Self>, k0: i32) -> ServerResult<R::GroupHandle<Snapshot, super::table_Skill::SkillConfig>> {
        let table = self.storage.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Skill: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_ById.find(R::finish_hash(h), |i| table.rows[i].v_Id == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_Skill().rows, positions: |s, slot| s.storage.get_Skill().index_Group_ById.group(slot), slot })
    }
    pub fn find_SkillLevel(self: &Arc<Self>, k0: i32, k1: i32, k2: i32) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_SkillLevel::SkillLevelConfig>>> {
        let table = self.storage.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "SkillLevel: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0); R::hash_part(&mut h, &k1); R::hash_part(&mut h, &k2);
        let slot=table.index_Key.find(R::finish_hash(h), |i| table.rows[i].v_SkillId == k0 && table.rows[i].v_Level == k1 && table.rows[i].v_Quality == k2);
        Ok(slot.map(|s| R::RowHandle { snapshot: self.clone(), row: table.rows[table.index_Key.row(s)].clone() }))
    }
    pub fn get_SkillLevel(self: &Arc<Self>, k0: i32, k1: i32, k2: i32) -> ServerResult<R::RowHandle<Snapshot, super::table_SkillLevel::SkillLevelConfig>> { self.find_SkillLevel(k0, k1, k2)?.ok_or_else(|| ConfigError::new(ErrorCode::KeyNotFound, "SkillLevel: key not found")) }
    pub fn group_SkillLevel_ByLevel(self: &Arc<Self>, k0: i32) -> ServerResult<R::GroupHandle<Snapshot, super::table_SkillLevel::SkillLevelConfig>> {
        let table = self.storage.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "SkillLevel: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_ByLevel.find(R::finish_hash(h), |i| table.rows[i].v_Level == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_SkillLevel().rows, positions: |s, slot| s.storage.get_SkillLevel().index_Group_ByLevel.group(slot), slot })
    }
    pub fn group_SkillLevel_ByQuality(self: &Arc<Self>, k0: i32) -> ServerResult<R::GroupHandle<Snapshot, super::table_SkillLevel::SkillLevelConfig>> {
        let table = self.storage.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "SkillLevel: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_ByQuality.find(R::finish_hash(h), |i| table.rows[i].v_Quality == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_SkillLevel().rows, positions: |s, slot| s.storage.get_SkillLevel().index_Group_ByQuality.group(slot), slot })
    }
    pub fn group_SkillLevel_BySkillId(self: &Arc<Self>, k0: i32) -> ServerResult<R::GroupHandle<Snapshot, super::table_SkillLevel::SkillLevelConfig>> {
        let table = self.storage.table_SkillLevel.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "SkillLevel: table not selected"))?;
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=table.index_Group_BySkillId.find(R::finish_hash(h), |i| table.rows[i].v_SkillId == k0);
        Ok(R::GroupHandle { snapshot: self.clone(), rows: |s| &s.storage.get_SkillLevel().rows, positions: |s, slot| s.storage.get_SkillLevel().index_Group_BySkillId.group(slot), slot })
    }
}
impl R::RowHandle<Snapshot, super::table_AllTypes10k::AllTypes10kConfig> {
    pub fn get_Hero(&self) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_Hero::HeroConfig>>> {
        let id = self.row.get_HeroId();
        if id == 0 { return Ok(None); }
        let row = self.snapshot.storage.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Reference outside selection"))?.ids.get(&id).ok_or_else(|| ConfigError::new(ErrorCode::MissingReference, "AllTypes10k.Hero: missing reference"))?;
        Ok(Some(R::RowHandle { snapshot: self.snapshot.clone(), row: row.clone() }))
    }
}
impl R::RowHandle<Snapshot, super::table_AllTypes10k::AllTypes10kConfig> {
    pub fn get_Skill(&self) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_Skill::SkillConfig>>> {
        let id = self.row.get_SkillId();
        if id == 0 { return Ok(None); }
        let row = self.snapshot.storage.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Reference outside selection"))?.ids.get(&id).ok_or_else(|| ConfigError::new(ErrorCode::MissingReference, "AllTypes10k.Skill: missing reference"))?;
        Ok(Some(R::RowHandle { snapshot: self.snapshot.clone(), row: row.clone() }))
    }
}
impl R::RowHandle<Snapshot, super::table_AllTypes10k::AllTypes10kConfig> {
    pub fn get_Previous(&self) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_AllTypes10k::AllTypes10kConfig>>> {
        let id = self.row.get_PreviousId();
        if id == 0 { return Ok(None); }
        let row = self.snapshot.storage.table_AllTypes10k.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Reference outside selection"))?.ids.get(&id).ok_or_else(|| ConfigError::new(ErrorCode::MissingReference, "AllTypes10k.Previous: missing reference"))?;
        Ok(Some(R::RowHandle { snapshot: self.snapshot.clone(), row: row.clone() }))
    }
}
impl R::RowHandle<Snapshot, super::table_Hero::HeroConfig> {
    pub fn get_Skill(&self) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_Skill::SkillConfig>>> {
        let id = self.row.get_SkillId();
        if id == 0 { return Ok(None); }
        let row = self.snapshot.storage.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Reference outside selection"))?.ids.get(&id).ok_or_else(|| ConfigError::new(ErrorCode::MissingReference, "Hero.Skill: missing reference"))?;
        Ok(Some(R::RowHandle { snapshot: self.snapshot.clone(), row: row.clone() }))
    }
}
impl R::RowHandle<Snapshot, super::table_Hero::HeroConfig> {
    pub fn get_OtherSkill(&self) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_Skill::SkillConfig>>> {
        let id = self.row.get_OtherSkillId();
        if id == 0 { return Ok(None); }
        let row = self.snapshot.storage.table_Skill.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Reference outside selection"))?.ids.get(&id).ok_or_else(|| ConfigError::new(ErrorCode::MissingReference, "Hero.OtherSkill: missing reference"))?;
        Ok(Some(R::RowHandle { snapshot: self.snapshot.clone(), row: row.clone() }))
    }
}
impl R::RowHandle<Snapshot, super::table_Hero::HeroConfig> {
    pub fn get_Mentor(&self) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_Hero::HeroConfig>>> {
        let id = self.row.get_MentorId();
        if id == 0 { return Ok(None); }
        let row = self.snapshot.storage.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Reference outside selection"))?.ids.get(&id).ok_or_else(|| ConfigError::new(ErrorCode::MissingReference, "Hero.Mentor: missing reference"))?;
        Ok(Some(R::RowHandle { snapshot: self.snapshot.clone(), row: row.clone() }))
    }
}
impl R::RowHandle<Snapshot, super::table_QuestRewards::QuestRewardsConfig> {
    pub fn get_Item(&self) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_ItemCatalog::ItemCatalogConfig>>> {
        let id = self.row.get_ItemId();
        if id == 0 { return Ok(None); }
        let row = self.snapshot.storage.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Reference outside selection"))?.ids.get(&id).ok_or_else(|| ConfigError::new(ErrorCode::MissingReference, "QuestRewards.Item: missing reference"))?;
        Ok(Some(R::RowHandle { snapshot: self.snapshot.clone(), row: row.clone() }))
    }
}
impl R::RowHandle<Snapshot, super::table_ShopOffers::ShopOffersConfig> {
    pub fn get_Item(&self) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_ItemCatalog::ItemCatalogConfig>>> {
        let id = self.row.get_ItemId();
        if id == 0 { return Ok(None); }
        let row = self.snapshot.storage.table_ItemCatalog.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Reference outside selection"))?.ids.get(&id).ok_or_else(|| ConfigError::new(ErrorCode::MissingReference, "ShopOffers.Item: missing reference"))?;
        Ok(Some(R::RowHandle { snapshot: self.snapshot.clone(), row: row.clone() }))
    }
}
impl R::RowHandle<Snapshot, super::table_Skill::SkillConfig> {
    pub fn get_Hero(&self) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_Hero::HeroConfig>>> {
        let id = self.row.get_HeroId();
        if id == 0 { return Ok(None); }
        let row = self.snapshot.storage.table_Hero.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, "Reference outside selection"))?.ids.get(&id).ok_or_else(|| ConfigError::new(ErrorCode::MissingReference, "Skill.Hero: missing reference"))?;
        Ok(Some(R::RowHandle { snapshot: self.snapshot.clone(), row: row.clone() }))
    }
}
