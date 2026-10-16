// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
use std::sync::{Arc, Weak, OnceLock};
use super::runtime::{self as R, Result, Json};
use super::types as T;
use super::snapshot::ConfigSnapshot;
pub struct DropConfig {
    pub(super) v_GroupId: i32,
    pub(super) v_Id: i32,
    pub(super) v_Count: i32,
}
impl DropConfig {
    // 掉落组编号
    pub fn get_GroupId(&self) -> i32 { self.v_GroupId }
    // 物品编号
    pub fn get_Id(&self) -> i32 { self.v_Id }
    // 数量
    pub fn get_Count(&self) -> i32 { self.v_Count }
    fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_GroupId: R::scalar::<i32>(v.field("GroupId")?)?,
            v_Id: R::scalar::<i32>(v.field("Id")?)?,
            v_Count: R::scalar::<i32>(v.field("Count")?)?,
        })
    }
}
pub struct DropTable {
    pub(super) rows: Vec<Arc<DropConfig>>,
    pub(super) index_Key: R::Index,
    pub(super) index_Group_ByGroupId: R::Index,
    pub(super) index_Group_ById: R::Index,
}
impl DropTable {
    pub fn rows(&self) -> &[Arc<DropConfig>] { &self.rows }
    pub fn count(&self) -> usize { self.rows.len() }
    pub(super) fn __zr_load(bundle: &R::Bundle, prefer_json: bool) -> Result<Self> {
        let source = bundle.load("{\"name\":\"Drop\",\"hash\":\"9DAF981CB794247A5F399C8DFA97320D99D670CC605BE1E071D33E596021A242\",\"hasId\":true,\"fields\":[{\"name\":\"GroupId\",\"type\":\"System.Int32\",\"description\":\"掉落组编号\"},{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"物品编号\"},{\"name\":\"Count\",\"type\":\"System.Int32\",\"description\":\"数量\"}],\"keys\":[\"GroupId\",\"Id\"],\"groups\":[{\"name\":\"ByGroupId\",\"fields\":[\"GroupId\"]},{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}", prefer_json)?;
        let rows = source.iter().map(|v| DropConfig::__zr_decode(v).map(Arc::new)).collect::<Result<Vec<_>>>()?;
        let index_Key = R::Index::build(rows.len(), true, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_GroupId); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_GroupId == rows[b].v_GroupId && rows[a].v_Id == rows[b].v_Id)?;
        let index_Group_ByGroupId = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_GroupId); R::finish_hash(h) }, |a,b| rows[a].v_GroupId == rows[b].v_GroupId)?;
        let index_Group_ById = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        Ok(Self { rows, index_Key, index_Group_ByGroupId, index_Group_ById })
    }
    pub(super) fn __zr_bind(&self, _snapshot: &ConfigSnapshot) -> Result<()> {
        Ok(())
    }
    pub fn find(&self, k0: i32, k1: i32) -> Option<&DropConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0); R::hash_part(&mut h, &k1);
        let slot=self.index_Key.find(R::finish_hash(h), |i| self.rows[i].v_GroupId == k0 && self.rows[i].v_Id == k1);
        slot.map(|s| self.rows[self.index_Key.row(s)].as_ref())
    }
    pub fn get(&self, k0: i32, k1: i32) -> Result<&DropConfig> { self.find(k0, k1).ok_or_else(|| "配置主键不存在".to_owned()) }
    pub fn get_ByGroupId(&self, k0: i32) -> R::Group<'_, DropConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_ByGroupId.find(R::finish_hash(h), |i| self.rows[i].v_GroupId == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_ByGroupId.group(slot) }
    }
    pub fn get_ById(&self, k0: i32) -> R::Group<'_, DropConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_ById.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_ById.group(slot) }
    }
}
