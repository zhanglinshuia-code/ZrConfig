// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
use std::sync::{Arc, Weak, OnceLock};
use super::runtime::{self as R, Result, Json};
use super::types as T;
use super::snapshot::ConfigSnapshot;
use super::table_Skill::SkillConfig;
pub struct HeroConfig {
    pub(super) v_Id: i32,
    pub(super) v_Name: String,
    pub(super) v_Skill: i32,
    ref_Skill: OnceLock<Weak<SkillConfig>>,
    pub(super) v_OtherSkill: i32,
    ref_OtherSkill: OnceLock<Weak<SkillConfig>>,
    pub(super) v_Mentor: i32,
    ref_Mentor: OnceLock<Weak<HeroConfig>>,
}
impl HeroConfig {
    // 英雄编号
    pub fn get_Id(&self) -> i32 { self.v_Id }
    // 英雄名称
    pub fn get_Name(&self) -> &str { &self.v_Name }
    // 技能 Id；引用 Skill 表
    pub fn get_SkillId(&self) -> i32 { self.v_Skill }
    pub fn get_Skill(&self) -> Option<Arc<SkillConfig>> { self.ref_Skill.get().and_then(Weak::upgrade) }
    // 另一个技能 Id；可复用同一对象
    pub fn get_OtherSkillId(&self) -> i32 { self.v_OtherSkill }
    pub fn get_OtherSkill(&self) -> Option<Arc<SkillConfig>> { self.ref_OtherSkill.get().and_then(Weak::upgrade) }
    // 导师 Id；引用 Hero 表
    pub fn get_MentorId(&self) -> i32 { self.v_Mentor }
    pub fn get_Mentor(&self) -> Option<Arc<HeroConfig>> { self.ref_Mentor.get().and_then(Weak::upgrade) }
    fn __zr_decode(v: &Json) -> Result<Self> {
        Ok(Self {
            v_Id: R::scalar::<i32>(v.field("Id")?)?,
            v_Name: R::scalar::<String>(v.field("Name")?)?,
            v_Skill: R::scalar::<i32>(v.field("Skill")?)?,
            ref_Skill: OnceLock::new(),
            v_OtherSkill: R::scalar::<i32>(v.field("OtherSkill")?)?,
            ref_OtherSkill: OnceLock::new(),
            v_Mentor: R::scalar::<i32>(v.field("Mentor")?)?,
            ref_Mentor: OnceLock::new(),
        })
    }
}
pub struct HeroTable {
    pub(super) rows: Vec<Arc<HeroConfig>>,
    pub(super) index_Key: R::Index,
    pub(super) index_Group_ById: R::Index,
    pub(super) ids: std::collections::HashMap<i32, Arc<HeroConfig>>,
}
impl HeroTable {
    pub fn rows(&self) -> &[Arc<HeroConfig>] { &self.rows }
    pub fn count(&self) -> usize { self.rows.len() }
    pub(super) fn __zr_load(bundle: &R::Bundle, prefer_json: bool) -> Result<Self> {
        let source = bundle.load("{\"name\":\"Hero\",\"hash\":\"7E1E75AC24BAFD42C18F1B43A19B0027A3D241CC665DB3727BC98A4908927ECE\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"英雄编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"英雄名称\"},{\"name\":\"Skill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"技能 Id；引用 Skill 表\"},{\"name\":\"OtherSkill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"另一个技能 Id；可复用同一对象\"},{\"name\":\"Mentor\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"导师 Id；引用 Hero 表\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"},{\"id\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Skill\"}],\"tags\":[],\"dependencies\":[\"Hero\",\"Skill\"]}", prefer_json)?;
        let rows = source.iter().map(|v| HeroConfig::__zr_decode(v).map(Arc::new)).collect::<Result<Vec<_>>>()?;
        let index_Key = R::Index::build(rows.len(), true, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        let index_Group_ById = R::Index::build(rows.len(), false, |i| { let mut h=R::new_hash(); R::hash_part(&mut h, &rows[i].v_Id); R::finish_hash(h) }, |a,b| rows[a].v_Id == rows[b].v_Id)?;
        let mut ids = std::collections::HashMap::with_capacity(rows.len());
        for row in &rows { if ids.insert(row.v_Id, row.clone()).is_some() { return R::invalid("引用 Id 不唯一"); } }
        Ok(Self { rows, index_Key, index_Group_ById, ids })
    }
    pub(super) fn __zr_bind(&self, _snapshot: &ConfigSnapshot) -> Result<()> {
        for row in &self.rows { if row.v_Skill != 0 { let target = _snapshot.get_Skill().ids.get(&row.v_Skill).ok_or("引用目标不存在：Hero.Skill")?; row.ref_Skill.set(Arc::downgrade(target)).map_err(|_| "引用重复绑定".to_owned())?; } }
        for row in &self.rows { if row.v_OtherSkill != 0 { let target = _snapshot.get_Skill().ids.get(&row.v_OtherSkill).ok_or("引用目标不存在：Hero.OtherSkill")?; row.ref_OtherSkill.set(Arc::downgrade(target)).map_err(|_| "引用重复绑定".to_owned())?; } }
        for row in &self.rows { if row.v_Mentor != 0 { let target = _snapshot.get_Hero().ids.get(&row.v_Mentor).ok_or("引用目标不存在：Hero.Mentor")?; row.ref_Mentor.set(Arc::downgrade(target)).map_err(|_| "引用重复绑定".to_owned())?; } }
        Ok(())
    }
    pub fn find(&self, k0: i32) -> Option<&HeroConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Key.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        slot.map(|s| self.rows[self.index_Key.row(s)].as_ref())
    }
    pub fn get(&self, k0: i32) -> Result<&HeroConfig> { self.find(k0).ok_or_else(|| "配置主键不存在".to_owned()) }
    pub fn get_ById(&self, k0: i32) -> R::Group<'_, HeroConfig> {
        let mut h=R::new_hash(); R::hash_part(&mut h, &k0);
        let slot=self.index_Group_ById.find(R::finish_hash(h), |i| self.rows[i].v_Id == k0);
        R::Group { rows: &self.rows, positions: self.index_Group_ById.group(slot) }
    }
}
