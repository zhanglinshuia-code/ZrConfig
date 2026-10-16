// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
use std::sync::{Arc, Weak, OnceLock};
use super::runtime::{self as R, Result, Json};
use super::types as T;
use super::snapshot::ConfigSnapshot;
pub struct SkillLevelConfig {
    pub(super) v_SkillId: i32,
    pub(super) v_Name: String,
    pub(super) v_Level: i32,
    pub(super) v_Quality: i32,
    pub(super) v_Damage: i32,
}
impl SkillLevelConfig {
    // 技能编号
    pub fn get_SkillId(&self) -> i32 { self.v_SkillId }
    // 技能名称
    pub fn get_Name(&self) -> &str { &self.v_Name }
    // 技能等级
    pub fn get_Level(&self) -> i32 { self.v_Level }
    // 品质
    pub fn get_Quality(&self) -> i32 { self.v_Quality }
    // 伤害
    pub fn get_Damage(&self) -> i32 { self.v_Damage }
    fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_SkillId: R::scalar::<i32>(v.field("SkillId")?)?,
            v_Name: R::scalar::<String>(v.field("Name")?)?,
            v_Level: R::scalar::<i32>(v.field("Level")?)?,
            v_Quality: R::scalar::<i32>(v.field("Quality")?)?,
            v_Damage: R::scalar::<i32>(v.field("Damage")?)?,
        })
    }
}
pub struct SkillLevelTable {
    pub(super) rows: Vec<Arc<SkillLevelConfig>>,
    pub(super) index_Key: R::Index,
    pub(super) index_Group_ByLevel: R::Index,
    pub(super) index_Group_ByQuality: R::Index,
    pub(super) index_Group_BySkillId: R::Index,
}
impl SkillLevelTable {
    pub fn rows(&self) -> &[Arc<SkillLevelConfig>] { &self.rows }
    pub fn count(&self) -> usize { self.rows.len() }
    pub(super) fn __zr_load(bundle: &R::Bundle, prefer_json: bool) -> Result<Self> {
        let source = bundle.load("{\"name\":\"SkillLevel\",\"hash\":\"9DDEDF4A13E49BE883FA11B62A966463D9B27C99620857DE5CD5E721CECC92B2\",\"hasId\":false,\"fields\":[{\"name\":\"SkillId\",\"type\":\"System.Int32\",\"description\":\"技能编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"技能名称\"},{\"name\":\"Level\",\"type\":\"System.Int32\",\"description\":\"技能等级\"},{\"name\":\"Quality\",\"type\":\"System.Int32\",\"description\":\"品质\"},{\"name\":\"Damage\",\"type\":\"System.Int32\",\"description\":\"伤害\"}],\"keys\":[\"SkillId\",\"Level\",\"Quality\"],\"groups\":[{\"name\":\"ByLevel\",\"fields\":[\"Level\"]},{\"name\":\"ByQuality\",\"fields\":[\"Quality\"]},{\"name\":\"BySkillId\",\"fields\":[\"SkillId\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}", prefer_json)?;
        let rows = source.iter().map(|v| SkillLevelConfig::__zr_decode(v).map(Arc::new)).collect::<Result<Vec<_>>>()?;
        let index_Key = R::Index::build(rows.len(), true, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_SkillId); R::hash_part(&mut h, &rows[i].v_Level); R::hash_part(&mut h, &rows[i].v_Quality); R::finish_hash(h) }, |a,b| rows[a].v_SkillId == rows[b].v_SkillId && rows[a].v_Level == rows[b].v_Level && rows[a].v_Quality == rows[b].v_Quality)?;
        let index_Group_ByLevel = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Level); R::finish_hash(h) }, |a,b| rows[a].v_Level == rows[b].v_Level)?;
        let index_Group_ByQuality = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Quality); R::finish_hash(h) }, |a,b| rows[a].v_Quality == rows[b].v_Quality)?;
        let index_Group_BySkillId = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_SkillId); R::finish_hash(h) }, |a,b| rows[a].v_SkillId == rows[b].v_SkillId)?;
        Ok(Self { rows, index_Key, index_Group_ByLevel, index_Group_ByQuality, index_Group_BySkillId })
    }
    pub(super) fn __zr_bind(&self, _snapshot: &ConfigSnapshot) -> Result<()> {
        Ok(())
    }
    pub fn find(&self, k0: i32, k1: i32, k2: i32) -> Option<&SkillLevelConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0); R::hash_part(&mut h, &k1); R::hash_part(&mut h, &k2);
        let slot=self.index_Key.find(R::finish_hash(h), |i| self.rows[i].v_SkillId == k0 && self.rows[i].v_Level == k1 && self.rows[i].v_Quality == k2);
        slot.map(|s| self.rows[self.index_Key.row(s)].as_ref())
    }
    pub fn get(&self, k0: i32, k1: i32, k2: i32) -> Result<&SkillLevelConfig> { self.find(k0, k1, k2).ok_or_else(|| "配置主键不存在".to_owned()) }
    pub fn get_ByLevel(&self, k0: i32) -> R::Group<'_, SkillLevelConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_ByLevel.find(R::finish_hash(h), |i| self.rows[i].v_Level == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_ByLevel.group(slot) }
    }
    pub fn get_ByQuality(&self, k0: i32) -> R::Group<'_, SkillLevelConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_ByQuality.find(R::finish_hash(h), |i| self.rows[i].v_Quality == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_ByQuality.group(slot) }
    }
    pub fn get_BySkillId(&self, k0: i32) -> R::Group<'_, SkillLevelConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_BySkillId.find(R::finish_hash(h), |i| self.rows[i].v_SkillId == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_BySkillId.group(slot) }
    }
}
