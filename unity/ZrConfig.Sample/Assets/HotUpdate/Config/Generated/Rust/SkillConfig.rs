// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
use std::sync::{Arc, Weak, OnceLock};
use super::runtime::{self as R, Result, Json};
use super::types as T;
use super::snapshot::ConfigSnapshot;
use super::table_Hero::HeroConfig;
pub struct SkillConfig {
    pub(super) v_Id: i32,
    pub(super) v_Name: String,
    pub(super) v_Hero: i32,
    ref_Hero: OnceLock<Weak<HeroConfig>>,
}
impl SkillConfig {
    // 技能编号
    pub fn get_Id(&self) -> i32 { self.v_Id }
    // 技能名称
    pub fn get_Name(&self) -> &str { &self.v_Name }
    // 英雄 Id；反向引用 Hero 表
    pub fn get_HeroId(&self) -> i32 { self.v_Hero }
    pub fn get_Hero(&self) -> Option<Arc<HeroConfig>> { self.ref_Hero.get().and_then(Weak::upgrade) }
    fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Id: R::scalar::<i32>(v.field("Id")?)?,
            v_Name: R::scalar::<String>(v.field("Name")?)?,
            v_Hero: R::scalar::<i32>(v.field("Hero")?)?,
            ref_Hero: OnceLock::new(),
        })
    }
}
pub struct SkillTable {
    pub(super) rows: Vec<Arc<SkillConfig>>,
    pub(super) index_Key: R::Index,
    pub(super) index_Group_ById: R::Index,
    pub(super) ids: std::collections::HashMap<i32, Arc<SkillConfig>>,
}
impl SkillTable {
    pub fn rows(&self) -> &[Arc<SkillConfig>] { &self.rows }
    pub fn count(&self) -> usize { self.rows.len() }
    pub(super) fn __zr_load(bundle: &R::Bundle, prefer_json: bool) -> Result<Self> {
        let source = bundle.load("{\"name\":\"Skill\",\"hash\":\"51DA7601418A98574B1C5ADC656CB7E7B479CC1F3E5237DFC5BA7532F702B9DB\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"技能编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"技能名称\"},{\"name\":\"Hero\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"英雄 Id；反向引用 Hero 表\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"}],\"tags\":[],\"dependencies\":[\"Hero\"]}", prefer_json)?;
        let rows = source.iter().map(|v| SkillConfig::__zr_decode(v).map(Arc::new)).collect::<Result<Vec<_>>>()?;
        let index_Key = R::Index::build(rows.len(), true, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        let index_Group_ById = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        let mut ids = std::collections::HashMap::with_capacity(rows.len());
        for row in &rows { if ids.insert(row.v_Id, row.clone()).is_some() { return R::invalid("引用 Id 不唯一"); } }
        Ok(Self { rows, index_Key, index_Group_ById, ids })
    }
    pub(super) fn __zr_bind(&self, _snapshot: &ConfigSnapshot) -> Result<()> {
        for row in &self.rows { if row.v_Hero != 0 { let target = _snapshot.get_Hero().ids.get(&row.v_Hero).ok_or("引用目标不存在：Skill.Hero")?; row.ref_Hero.set(Arc::downgrade(target)).map_err(|_| "引用重复绑定".to_owned())?; } }
        Ok(())
    }
    pub fn find(&self, k0: i32) -> Option<&SkillConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Key.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        slot.map(|s| self.rows[self.index_Key.row(s)].as_ref())
    }
    pub fn get(&self, k0: i32) -> Result<&SkillConfig> { self.find(k0).ok_or_else(|| "配置主键不存在".to_owned()) }
    pub fn get_ById(&self, k0: i32) -> R::Group<'_, SkillConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_ById.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_ById.group(slot) }
    }
}
