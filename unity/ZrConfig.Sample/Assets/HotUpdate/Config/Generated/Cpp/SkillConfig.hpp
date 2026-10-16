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
class SkillConfig {
    friend class SkillTable;
    std::int32_t v_Id;
    std::string v_Name;
    std::int32_t v_Hero;
    const HeroConfig* ref_Hero = nullptr;
public:
    explicit SkillConfig(const Json& value) : v_Id(integer<std::int32_t>(value.field("Id"))),v_Name(value.field("Name").string()),v_Hero(integer<std::int32_t>(value.field("Hero"))) {}
    // 技能编号
    const std::int32_t& Id() const noexcept { return v_Id; }
    // 技能名称
    const std::string& Name() const noexcept { return v_Name; }
    // 英雄 Id；反向引用 Hero 表
    const std::int32_t& HeroId() const noexcept { return v_Hero; }
    const HeroConfig* Hero() const noexcept { return ref_Hero; }
};
class SkillTable {
    friend class ConfigSnapshot;
    std::vector<SkillConfig> rows_;
    struct Index_Key : Index<SkillConfig> {
        using Index<SkillConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Id() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Key;
    struct Index_Group_ById : Index<SkillConfig> {
        using Index<SkillConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Id() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Group_ById;
    std::unordered_map<std::int32_t,const SkillConfig*> ids_;
    static std::vector<SkillConfig> decode(const std::vector<Json>& source) { std::vector<SkillConfig> rows;rows.reserve(source.size());for(const auto& value:source)rows.emplace_back(value);return rows; }
    void bind(const ConfigSnapshot& snapshot);
public:
    explicit SkillTable(const std::vector<Json>& source) : rows_(decode(source)), index_Key(rows_,true,[](const SkillConfig& row){return Index_Key::hash(row.Id());},[](const SkillConfig& a,const SkillConfig& b){return a.Id() == b.Id();}), index_Group_ById(rows_,false,[](const SkillConfig& row){return Index_Group_ById::hash(row.Id());},[](const SkillConfig& a,const SkillConfig& b){return a.Id() == b.Id();}) {
        ids_.reserve(rows_.size());for(const auto& row:rows_)if(!ids_.emplace(row.Id(),&row).second)invalid("引用 Id 不唯一");
    }
    SkillTable(const SkillTable&) = delete;
    SkillTable& operator=(const SkillTable&) = delete;
    static std::string schema() { return "{\"name\":\"Skill\",\"hash\":\"51DA7601418A98574B1C5ADC656CB7E7B479CC1F3E5237DFC5BA7532F702B9DB\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"技能编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"技能名称\"},{\"name\":\"Hero\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"英雄 Id；反向引用 Hero 表\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"}],\"tags\":[],\"dependencies\":[\"Hero\"]}"; }
    const std::vector<SkillConfig>& rows() const noexcept { return rows_; }
    std::size_t count() const noexcept { return rows_.size(); }
    const SkillConfig* by_id(std::int32_t id) const { auto it=ids_.find(id);return it==ids_.end()?nullptr:it->second; }
    const SkillConfig* find(const std::int32_t& k0) const {return index_Key.row(index_Key.find(k0));}
    const SkillConfig& get(const std::int32_t& k0) const {auto p=find(k0);if(!p)invalid("配置主键不存在");return *p;}
    const std::vector<const SkillConfig*>& getById(const std::int32_t& k0) const {return index_Group_ById.group(index_Group_ById.find(k0));}
};
} // namespace zrconfig
