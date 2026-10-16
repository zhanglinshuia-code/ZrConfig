// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
use std::sync::{Arc, Weak, OnceLock};
use super::runtime::{self as R, Result, Json};
use super::types as T;
use super::snapshot::ConfigSnapshot;
use super::table_ItemCatalog::ItemCatalogConfig;
pub struct QuestRewardsConfig {
    pub(super) v_Id: i32,
    pub(super) v_Name: String,
    pub(super) v_Item: i32,
    ref_Item: OnceLock<Weak<ItemCatalogConfig>>,
    pub(super) v_Count: i32,
    pub(super) v_RequiredLevel: i32,
    pub(super) v_Enabled: bool,
}
impl QuestRewardsConfig {
    // 任务编号
    pub fn get_Id(&self) -> i32 { self.v_Id }
    // 任务名称
    pub fn get_Name(&self) -> &str { &self.v_Name }
    // 引用 ItemCatalog 的 Id
    pub fn get_ItemId(&self) -> i32 { self.v_Item }
    pub fn get_Item(&self) -> Option<Arc<ItemCatalogConfig>> { self.ref_Item.get().and_then(Weak::upgrade) }
    // 奖励数量：大于零
    pub fn get_Count(&self) -> i32 { self.v_Count }
    // 开放等级：大于零
    pub fn get_RequiredLevel(&self) -> i32 { self.v_RequiredLevel }
    // 是否启用
    pub fn get_Enabled(&self) -> bool { self.v_Enabled }
    fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Id: R::scalar::<i32>(v.field("Id")?)?,
            v_Name: R::scalar::<String>(v.field("Name")?)?,
            v_Item: R::scalar::<i32>(v.field("Item")?)?,
            ref_Item: OnceLock::new(),
            v_Count: R::scalar::<i32>(v.field("Count")?)?,
            v_RequiredLevel: R::scalar::<i32>(v.field("RequiredLevel")?)?,
            v_Enabled: R::scalar::<bool>(v.field("Enabled")?)?,
        })
    }
}
pub struct QuestRewardsTable {
    pub(super) rows: Vec<Arc<QuestRewardsConfig>>,
    pub(super) index_Key: R::Index,
    pub(super) index_Group_ById: R::Index,
}
impl QuestRewardsTable {
    pub fn rows(&self) -> &[Arc<QuestRewardsConfig>] { &self.rows }
    pub fn count(&self) -> usize { self.rows.len() }
    pub(super) fn __zr_load(bundle: &R::Bundle, prefer_json: bool) -> Result<Self> {
        let source = bundle.load("{\"name\":\"QuestRewards\",\"hash\":\"C839B7EDFA92304709824DABBA84B5534E34810224D2814C221E6F4A3B536E35\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"任务编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"任务名称\"},{\"name\":\"Item\",\"type\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"description\":\"引用 ItemCatalog 的 Id\"},{\"name\":\"Count\",\"type\":\"System.Int32\",\"description\":\"奖励数量：大于零\"},{\"name\":\"RequiredLevel\",\"type\":\"System.Int32\",\"description\":\"开放等级：大于零\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否启用\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"ItemCatalog\"}],\"tags\":[],\"dependencies\":[\"ItemCatalog\"]}", prefer_json)?;
        let rows = source.iter().map(|v| QuestRewardsConfig::__zr_decode(v).map(Arc::new)).collect::<Result<Vec<_>>>()?;
        let index_Key = R::Index::build(rows.len(), true, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        let index_Group_ById = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        Ok(Self { rows, index_Key, index_Group_ById })
    }
    pub(super) fn __zr_bind(&self, _snapshot: &ConfigSnapshot) -> Result<()> {
        for row in &self.rows { if row.v_Item != 0 { let target = _snapshot.get_ItemCatalog().ids.get(&row.v_Item).ok_or("引用目标不存在：QuestRewards.Item")?; row.ref_Item.set(Arc::downgrade(target)).map_err(|_| "引用重复绑定".to_owned())?; } }
        Ok(())
    }
    pub fn find(&self, k0: i32) -> Option<&QuestRewardsConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Key.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        slot.map(|s| self.rows[self.index_Key.row(s)].as_ref())
    }
    pub fn get(&self, k0: i32) -> Result<&QuestRewardsConfig> { self.find(k0).ok_or_else(|| "配置主键不存在".to_owned()) }
    pub fn get_ById(&self, k0: i32) -> R::Group<'_, QuestRewardsConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_ById.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_ById.group(slot) }
    }
}
