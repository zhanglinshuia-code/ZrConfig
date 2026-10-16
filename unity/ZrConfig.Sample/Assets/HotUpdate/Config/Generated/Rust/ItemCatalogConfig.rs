// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
use std::sync::{Arc, Weak, OnceLock};
use super::runtime::{self as R, Result, Json};
use super::types as T;
use super::snapshot::ConfigSnapshot;
pub struct ItemCatalogConfig {
    pub(super) v_Id: i32,
    pub(super) v_Name: String,
    pub(super) v_MaxStack: i32,
    pub(super) v_Rarity: i32,
    pub(super) v_Enabled: bool,
}
impl ItemCatalogConfig {
    // 道具编号
    pub fn get_Id(&self) -> i32 { self.v_Id }
    // 显示名称
    pub fn get_Name(&self) -> &str { &self.v_Name }
    // 堆叠上限：1～10000
    pub fn get_MaxStack(&self) -> i32 { self.v_MaxStack }
    // 品质：1～5
    pub fn get_Rarity(&self) -> i32 { self.v_Rarity }
    // 是否启用
    pub fn get_Enabled(&self) -> bool { self.v_Enabled }
    fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Id: R::scalar::<i32>(v.field("Id")?)?,
            v_Name: R::scalar::<String>(v.field("Name")?)?,
            v_MaxStack: R::scalar::<i32>(v.field("MaxStack")?)?,
            v_Rarity: R::scalar::<i32>(v.field("Rarity")?)?,
            v_Enabled: R::scalar::<bool>(v.field("Enabled")?)?,
        })
    }
}
pub struct ItemCatalogTable {
    pub(super) rows: Vec<Arc<ItemCatalogConfig>>,
    pub(super) index_Key: R::Index,
    pub(super) index_Group_ById: R::Index,
    pub(super) ids: std::collections::HashMap<i32, Arc<ItemCatalogConfig>>,
}
impl ItemCatalogTable {
    pub fn rows(&self) -> &[Arc<ItemCatalogConfig>] { &self.rows }
    pub fn count(&self) -> usize { self.rows.len() }
    pub(super) fn __zr_load(bundle: &R::Bundle, prefer_json: bool) -> Result<Self> {
        let source = bundle.load("{\"name\":\"ItemCatalog\",\"hash\":\"C62E86119B370A102C131E82385650005AABD6EC76A0074028B958400781BEE0\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"道具编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"显示名称\"},{\"name\":\"MaxStack\",\"type\":\"System.Int32\",\"description\":\"堆叠上限：1～10000\"},{\"name\":\"Rarity\",\"type\":\"System.Int32\",\"description\":\"品质：1～5\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否启用\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}", prefer_json)?;
        let rows = source.iter().map(|v| ItemCatalogConfig::__zr_decode(v).map(Arc::new)).collect::<Result<Vec<_>>>()?;
        let index_Key = R::Index::build(rows.len(), true, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        let index_Group_ById = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        let mut ids = std::collections::HashMap::with_capacity(rows.len());
        for row in &rows { if ids.insert(row.v_Id, row.clone()).is_some() { return R::invalid("引用 Id 不唯一"); } }
        Ok(Self { rows, index_Key, index_Group_ById, ids })
    }
    pub(super) fn __zr_bind(&self, _snapshot: &ConfigSnapshot) -> Result<()> {
        Ok(())
    }
    pub fn find(&self, k0: i32) -> Option<&ItemCatalogConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Key.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        slot.map(|s| self.rows[self.index_Key.row(s)].as_ref())
    }
    pub fn get(&self, k0: i32) -> Result<&ItemCatalogConfig> { self.find(k0).ok_or_else(|| "配置主键不存在".to_owned()) }
    pub fn get_ById(&self, k0: i32) -> R::Group<'_, ItemCatalogConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_ById.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_ById.group(slot) }
    }
}
