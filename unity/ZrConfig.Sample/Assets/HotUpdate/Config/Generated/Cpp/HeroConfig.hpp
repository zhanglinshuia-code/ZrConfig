// zrconfig-api-contract:1
// 此文件由工具自动生成，请勿手工修改。
#pragma once
#include "types.hpp"
namespace zrconfig {
class ConfigSnapshot;
class AllTypesConfig;
class AllTypes10kConfig;
class DropConfig;
class HeroConfig;
class ItemCatalogConfig;
class QuestRewardsConfig;
class ShopOffersConfig;
class SkillConfig;
class SkillLevelConfig;
class HeroConfig {
    friend class HeroTable;
    std::int32_t v_Id;
    std::string v_Name;
    std::int32_t v_Skill;
    const SkillConfig* ref_Skill = nullptr;
    std::int32_t v_OtherSkill;
    const SkillConfig* ref_OtherSkill = nullptr;
    std::int32_t v_Mentor;
    const HeroConfig* ref_Mentor = nullptr;
public:
    explicit HeroConfig(const Json& value) : v_Id(integer<std::int32_t>(value.field("Id"))),v_Name(value.field("Name").string()),v_Skill(integer<std::int32_t>(value.field("Skill"))),v_OtherSkill(integer<std::int32_t>(value.field("OtherSkill"))),v_Mentor(integer<std::int32_t>(value.field("Mentor"))) {}
    // 英雄编号
    const std::int32_t& Id() const noexcept { return v_Id; }
    // 英雄名称
    const std::string& Name() const noexcept { return v_Name; }
    // 技能 Id；引用 Skill 表
    const std::int32_t& SkillId() const noexcept { return v_Skill; }
    const SkillConfig* Skill() const noexcept { return ref_Skill; }
    // 另一个技能 Id；可复用同一对象
    const std::int32_t& OtherSkillId() const noexcept { return v_OtherSkill; }
    const SkillConfig* OtherSkill() const noexcept { return ref_OtherSkill; }
    // 导师 Id；引用 Hero 表
    const std::int32_t& MentorId() const noexcept { return v_Mentor; }
    const HeroConfig* Mentor() const noexcept { return ref_Mentor; }
};
class HeroTable {
    friend class ConfigSnapshot;
    std::vector<HeroConfig> rows_;
    struct Index_Key : Index<HeroConfig> {
        using Index<HeroConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Id() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Key;
    struct Index_Group_ById : Index<HeroConfig> {
        using Index<HeroConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Id() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Group_ById;
    std::unordered_map<std::int32_t,const HeroConfig*> ids_;
    static std::vector<HeroConfig> decode(const std::vector<Json>& source) { std::vector<HeroConfig> rows;rows.reserve(source.size());for(const auto& value:source)rows.emplace_back(value);return rows; }
    void bind(const ConfigSnapshot& snapshot);
public:
    explicit HeroTable(const std::vector<Json>& source) : rows_(decode(source)), index_Key(rows_,true,[](const HeroConfig& row){return Index_Key::hash(row.Id());},[](const HeroConfig& a,const HeroConfig& b){return a.Id() == b.Id();}), index_Group_ById(rows_,false,[](const HeroConfig& row){return Index_Group_ById::hash(row.Id());},[](const HeroConfig& a,const HeroConfig& b){return a.Id() == b.Id();}) {
        ids_.reserve(rows_.size());for(const auto& row:rows_)if(!ids_.emplace(row.Id(),&row).second)invalid("引用 Id 不唯一");
    }
    HeroTable(const HeroTable&) = delete;
    HeroTable& operator=(const HeroTable&) = delete;
    static std::string schema() { return "{\"name\":\"Hero\",\"hash\":\"7E1E75AC24BAFD42C18F1B43A19B0027A3D241CC665DB3727BC98A4908927ECE\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"英雄编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"英雄名称\"},{\"name\":\"Skill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"技能 Id；引用 Skill 表\"},{\"name\":\"OtherSkill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"另一个技能 Id；可复用同一对象\"},{\"name\":\"Mentor\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"导师 Id；引用 Hero 表\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"},{\"id\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Skill\"}],\"tags\":[],\"dependencies\":[\"Hero\",\"Skill\"]}"; }
    const std::vector<HeroConfig>& rows() const noexcept { return rows_; }
    std::size_t count() const noexcept { return rows_.size(); }
    const HeroConfig* by_id(std::int32_t id) const { auto it=ids_.find(id);return it==ids_.end()?nullptr:it->second; }
    const HeroConfig* find(const std::int32_t& k0) const {return index_Key.row(index_Key.find(k0));}
    const HeroConfig& get(const std::int32_t& k0) const {auto p=find(k0);if(!p)invalid("配置主键不存在");return *p;}
    const std::vector<const HeroConfig*>& getById(const std::int32_t& k0) const {return index_Group_ById.group(index_Group_ById.find(k0));}
};
} // namespace zrconfig
