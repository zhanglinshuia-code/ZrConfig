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
class DropConfig {
    friend class DropTable;
    std::int32_t v_GroupId;
    std::int32_t v_Id;
    std::int32_t v_Count;
public:
    explicit DropConfig(const Json& value) : v_GroupId(integer<std::int32_t>(value.field("GroupId"))),v_Id(integer<std::int32_t>(value.field("Id"))),v_Count(integer<std::int32_t>(value.field("Count"))) {}
    // 掉落组编号
    const std::int32_t& GroupId() const noexcept { return v_GroupId; }
    // 物品编号
    const std::int32_t& Id() const noexcept { return v_Id; }
    // 数量
    const std::int32_t& Count() const noexcept { return v_Count; }
};
class DropTable {
    friend class ConfigSnapshot;
    std::vector<DropConfig> rows_;
    struct Index_Key : Index<DropConfig> {
        using Index<DropConfig>::Index;
        static std::size_t hash(const std::int32_t& k0,const std::int32_t& k1) { return ((1 * 31u + Hash<std::int32_t>{}(k0)) * 31u + Hash<std::int32_t>{}(k1)); }
        std::size_t find(const std::int32_t& k0,const std::int32_t& k1) const {
            auto slot=mix(hash(k0,k1))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.GroupId() == k0 && row.Id() == k1) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Key;
    struct Index_Group_ByGroupId : Index<DropConfig> {
        using Index<DropConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.GroupId() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Group_ByGroupId;
    struct Index_Group_ById : Index<DropConfig> {
        using Index<DropConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Id() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Group_ById;
    static std::vector<DropConfig> decode(const std::vector<Json>& source) { std::vector<DropConfig> rows;rows.reserve(source.size());for(const auto& value:source)rows.emplace_back(value);return rows; }
    void bind(const ConfigSnapshot& snapshot);
public:
    explicit DropTable(const std::vector<Json>& source) : rows_(decode(source)), index_Key(rows_,true,[](const DropConfig& row){return Index_Key::hash(row.GroupId(),row.Id());},[](const DropConfig& a,const DropConfig& b){return a.GroupId() == b.GroupId() && a.Id() == b.Id();}), index_Group_ByGroupId(rows_,false,[](const DropConfig& row){return Index_Group_ByGroupId::hash(row.GroupId());},[](const DropConfig& a,const DropConfig& b){return a.GroupId() == b.GroupId();}), index_Group_ById(rows_,false,[](const DropConfig& row){return Index_Group_ById::hash(row.Id());},[](const DropConfig& a,const DropConfig& b){return a.Id() == b.Id();}) {
    }
    DropTable(const DropTable&) = delete;
    DropTable& operator=(const DropTable&) = delete;
    static std::string schema() { return "{\"name\":\"Drop\",\"hash\":\"9DAF981CB794247A5F399C8DFA97320D99D670CC605BE1E071D33E596021A242\",\"hasId\":true,\"fields\":[{\"name\":\"GroupId\",\"type\":\"System.Int32\",\"description\":\"掉落组编号\"},{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"物品编号\"},{\"name\":\"Count\",\"type\":\"System.Int32\",\"description\":\"数量\"}],\"keys\":[\"GroupId\",\"Id\"],\"groups\":[{\"name\":\"ByGroupId\",\"fields\":[\"GroupId\"]},{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}"; }
    const std::vector<DropConfig>& rows() const noexcept { return rows_; }
    std::size_t count() const noexcept { return rows_.size(); }
    const DropConfig* find(const std::int32_t& k0,const std::int32_t& k1) const {return index_Key.row(index_Key.find(k0,k1));}
    const DropConfig& get(const std::int32_t& k0,const std::int32_t& k1) const {auto p=find(k0,k1);if(!p)invalid("配置主键不存在");return *p;}
    const std::vector<const DropConfig*>& getByGroupId(const std::int32_t& k0) const {return index_Group_ByGroupId.group(index_Group_ByGroupId.find(k0));}
    const std::vector<const DropConfig*>& getById(const std::int32_t& k0) const {return index_Group_ById.group(index_Group_ById.find(k0));}
};
} // namespace zrconfig
