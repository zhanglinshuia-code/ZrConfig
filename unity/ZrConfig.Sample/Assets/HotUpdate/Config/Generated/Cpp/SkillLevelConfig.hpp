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
class SkillLevelConfig {
    friend class SkillLevelTable;
    std::int32_t v_SkillId;
    std::string v_Name;
    std::int32_t v_Level;
    std::int32_t v_Quality;
    std::int32_t v_Damage;
public:
    explicit SkillLevelConfig(const Json& value) : v_SkillId(integer<std::int32_t>(value.field("SkillId"))),v_Name(value.field("Name").string()),v_Level(integer<std::int32_t>(value.field("Level"))),v_Quality(integer<std::int32_t>(value.field("Quality"))),v_Damage(integer<std::int32_t>(value.field("Damage"))) {}
    // 技能编号
    const std::int32_t& SkillId() const noexcept { return v_SkillId; }
    // 技能名称
    const std::string& Name() const noexcept { return v_Name; }
    // 技能等级
    const std::int32_t& Level() const noexcept { return v_Level; }
    // 品质
    const std::int32_t& Quality() const noexcept { return v_Quality; }
    // 伤害
    const std::int32_t& Damage() const noexcept { return v_Damage; }
};
class SkillLevelTable {
    friend class ConfigSnapshot;
    std::vector<SkillLevelConfig> rows_;
    struct Index_Key : Index<SkillLevelConfig> {
        using Index<SkillLevelConfig>::Index;
        static std::size_t hash(const std::int32_t& k0,const std::int32_t& k1,const std::int32_t& k2) { return (((1 * 31u + Hash<std::int32_t>{}(k0)) * 31u + Hash<std::int32_t>{}(k1)) * 31u + Hash<std::int32_t>{}(k2)); }
        std::size_t find(const std::int32_t& k0,const std::int32_t& k1,const std::int32_t& k2) const {
            auto slot=mix(hash(k0,k1,k2))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.SkillId() == k0 && row.Level() == k1 && row.Quality() == k2) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Key;
    struct Index_Group_ByLevel : Index<SkillLevelConfig> {
        using Index<SkillLevelConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Level() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Group_ByLevel;
    struct Index_Group_ByQuality : Index<SkillLevelConfig> {
        using Index<SkillLevelConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Quality() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Group_ByQuality;
    struct Index_Group_BySkillId : Index<SkillLevelConfig> {
        using Index<SkillLevelConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.SkillId() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Group_BySkillId;
    static std::vector<SkillLevelConfig> decode(const std::vector<Json>& source) { std::vector<SkillLevelConfig> rows;rows.reserve(source.size());for(const auto& value:source)rows.emplace_back(value);return rows; }
    void bind(const ConfigSnapshot& snapshot);
public:
    explicit SkillLevelTable(const std::vector<Json>& source) : rows_(decode(source)), index_Key(rows_,true,[](const SkillLevelConfig& row){return Index_Key::hash(row.SkillId(),row.Level(),row.Quality());},[](const SkillLevelConfig& a,const SkillLevelConfig& b){return a.SkillId() == b.SkillId() && a.Level() == b.Level() && a.Quality() == b.Quality();}), index_Group_ByLevel(rows_,false,[](const SkillLevelConfig& row){return Index_Group_ByLevel::hash(row.Level());},[](const SkillLevelConfig& a,const SkillLevelConfig& b){return a.Level() == b.Level();}), index_Group_ByQuality(rows_,false,[](const SkillLevelConfig& row){return Index_Group_ByQuality::hash(row.Quality());},[](const SkillLevelConfig& a,const SkillLevelConfig& b){return a.Quality() == b.Quality();}), index_Group_BySkillId(rows_,false,[](const SkillLevelConfig& row){return Index_Group_BySkillId::hash(row.SkillId());},[](const SkillLevelConfig& a,const SkillLevelConfig& b){return a.SkillId() == b.SkillId();}) {
    }
    SkillLevelTable(const SkillLevelTable&) = delete;
    SkillLevelTable& operator=(const SkillLevelTable&) = delete;
    static std::string schema() { return "{\"name\":\"SkillLevel\",\"hash\":\"9DDEDF4A13E49BE883FA11B62A966463D9B27C99620857DE5CD5E721CECC92B2\",\"hasId\":false,\"fields\":[{\"name\":\"SkillId\",\"type\":\"System.Int32\",\"description\":\"技能编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"技能名称\"},{\"name\":\"Level\",\"type\":\"System.Int32\",\"description\":\"技能等级\"},{\"name\":\"Quality\",\"type\":\"System.Int32\",\"description\":\"品质\"},{\"name\":\"Damage\",\"type\":\"System.Int32\",\"description\":\"伤害\"}],\"keys\":[\"SkillId\",\"Level\",\"Quality\"],\"groups\":[{\"name\":\"ByLevel\",\"fields\":[\"Level\"]},{\"name\":\"ByQuality\",\"fields\":[\"Quality\"]},{\"name\":\"BySkillId\",\"fields\":[\"SkillId\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}"; }
    const std::vector<SkillLevelConfig>& rows() const noexcept { return rows_; }
    std::size_t count() const noexcept { return rows_.size(); }
    const SkillLevelConfig* find(const std::int32_t& k0,const std::int32_t& k1,const std::int32_t& k2) const {return index_Key.row(index_Key.find(k0,k1,k2));}
    const SkillLevelConfig& get(const std::int32_t& k0,const std::int32_t& k1,const std::int32_t& k2) const {auto p=find(k0,k1,k2);if(!p)invalid("配置主键不存在");return *p;}
    const std::vector<const SkillLevelConfig*>& getByLevel(const std::int32_t& k0) const {return index_Group_ByLevel.group(index_Group_ByLevel.find(k0));}
    const std::vector<const SkillLevelConfig*>& getByQuality(const std::int32_t& k0) const {return index_Group_ByQuality.group(index_Group_ByQuality.find(k0));}
    const std::vector<const SkillLevelConfig*>& getBySkillId(const std::int32_t& k0) const {return index_Group_BySkillId.group(index_Group_BySkillId.find(k0));}
};
} // namespace zrconfig
