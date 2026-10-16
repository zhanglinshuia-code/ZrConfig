// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
use std::sync::{Arc, Weak, OnceLock};
use super::runtime::{self as R, Result, Json};
use super::types as T;
use super::snapshot::ConfigSnapshot;
use super::table_ItemCatalog::ItemCatalogConfig;
pub struct ShopOffersConfig {
    pub(super) v_Id: i32,
    pub(super) v_Item: i32,
    ref_Item: OnceLock<Weak<ItemCatalogConfig>>,
    pub(super) v_Price: i32,
    pub(super) v_DailyLimit: i32,
    pub(super) v_Enabled: bool,
}
impl ShopOffersConfig {
    // 商品编号
    pub fn get_Id(&self) -> i32 { self.v_Id }
    // 引用 ItemCatalog 的 Id
    pub fn get_ItemId(&self) -> i32 { self.v_Item }
    pub fn get_Item(&self) -> Option<Arc<ItemCatalogConfig>> { self.ref_Item.get().and_then(Weak::upgrade) }
    // 价格：非负整数；0 为免费
    pub fn get_Price(&self) -> i32 { self.v_Price }
    // 每日限购：0 为不限购
    pub fn get_DailyLimit(&self) -> i32 { self.v_DailyLimit }
    // 是否上架
    pub fn get_Enabled(&self) -> bool { self.v_Enabled }
    fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Id: R::scalar::<i32>(v.field("Id")?)?,
            v_Item: R::scalar::<i32>(v.field("Item")?)?,
            ref_Item: OnceLock::new(),
            v_Price: R::scalar::<i32>(v.field("Price")?)?,
            v_DailyLimit: R::scalar::<i32>(v.field("DailyLimit")?)?,
            v_Enabled: R::scalar::<bool>(v.field("Enabled")?)?,
        })
    }
}
pub struct ShopOffersTable {
    pub(super) rows: Vec<Arc<ShopOffersConfig>>,
    pub(super) index_Key: R::Index,
    pub(super) index_Group_ById: R::Index,
}
impl ShopOffersTable {
    pub fn rows(&self) -> &[Arc<ShopOffersConfig>] { &self.rows }
    pub fn count(&self) -> usize { self.rows.len() }
    pub(super) fn __zr_load(bundle: &R::Bundle, prefer_json: bool) -> Result<Self> {
        let source = bundle.load("{\"name\":\"ShopOffers\",\"hash\":\"0376659B4746D22E616351BEAEFC78651E69BFCEFF2D7ECAB8BD17539864E8E1\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"商品编号\"},{\"name\":\"Item\",\"type\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"description\":\"引用 ItemCatalog 的 Id\"},{\"name\":\"Price\",\"type\":\"System.Int32\",\"description\":\"价格：非负整数；0 为免费\"},{\"name\":\"DailyLimit\",\"type\":\"System.Int32\",\"description\":\"每日限购：0 为不限购\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否上架\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"ItemCatalog\"}],\"tags\":[],\"dependencies\":[\"ItemCatalog\"]}", prefer_json)?;
        let rows = source.iter().map(|v| ShopOffersConfig::__zr_decode(v).map(Arc::new)).collect::<Result<Vec<_>>>()?;
        let index_Key = R::Index::build(rows.len(), true, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        let index_Group_ById = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        Ok(Self { rows, index_Key, index_Group_ById })
    }
    pub(super) fn __zr_bind(&self, _snapshot: &ConfigSnapshot) -> Result<()> {
        for row in &self.rows { if row.v_Item != 0 { let target = _snapshot.get_ItemCatalog().ids.get(&row.v_Item).ok_or("引用目标不存在：ShopOffers.Item")?; row.ref_Item.set(Arc::downgrade(target)).map_err(|_| "引用重复绑定".to_owned())?; } }
        Ok(())
    }
    pub fn find(&self, k0: i32) -> Option<&ShopOffersConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Key.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        slot.map(|s| self.rows[self.index_Key.row(s)].as_ref())
    }
    pub fn get(&self, k0: i32) -> Result<&ShopOffersConfig> { self.find(k0).ok_or_else(|| "配置主键不存在".to_owned()) }
    pub fn get_ById(&self, k0: i32) -> R::Group<'_, ShopOffersConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_ById.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_ById.group(slot) }
    }
}
