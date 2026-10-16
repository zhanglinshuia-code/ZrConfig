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
class QuestRewardsConfig {
    friend class QuestRewardsTable;
    std::int32_t v_Id;
    std::string v_Name;
    std::int32_t v_Item;
    const ItemCatalogConfig* ref_Item = nullptr;
    std::int32_t v_Count;
    std::int32_t v_RequiredLevel;
    bool v_Enabled;
public:
    explicit QuestRewardsConfig(const Json& value) : v_Id(integer<std::int32_t>(value.field("Id"))),v_Name(value.field("Name").string()),v_Item(integer<std::int32_t>(value.field("Item"))),v_Count(integer<std::int32_t>(value.field("Count"))),v_RequiredLevel(integer<std::int32_t>(value.field("RequiredLevel"))),v_Enabled(value.field("Enabled").boolean()) {}
    // 任务编号
    const std::int32_t& Id() const noexcept { return v_Id; }
    // 任务名称
    const std::string& Name() const noexcept { return v_Name; }
    // 引用 ItemCatalog 的 Id
    const std::int32_t& ItemId() const noexcept { return v_Item; }
    const ItemCatalogConfig* Item() const noexcept { return ref_Item; }
    // 奖励数量：大于零
    const std::int32_t& Count() const noexcept { return v_Count; }
    // 开放等级：大于零
    const std::int32_t& RequiredLevel() const noexcept { return v_RequiredLevel; }
    // 是否启用
    const bool& Enabled() const noexcept { return v_Enabled; }
};
class QuestRewardsTable {
    friend class ConfigSnapshot;
    std::vector<QuestRewardsConfig> rows_;
    struct Index_Key : Index<QuestRewardsConfig> {
        using Index<QuestRewardsConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Id() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Key;
    struct Index_Group_ById : Index<QuestRewardsConfig> {
        using Index<QuestRewardsConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Id() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Group_ById;
    static std::vector<QuestRewardsConfig> decode(const std::vector<Json>& source) { std::vector<QuestRewardsConfig> rows;rows.reserve(source.size());for(const auto& value:source)rows.emplace_back(value);return rows; }
    void bind(const ConfigSnapshot& snapshot);
public:
    explicit QuestRewardsTable(const std::vector<Json>& source) : rows_(decode(source)), index_Key(rows_,true,[](const QuestRewardsConfig& row){return Index_Key::hash(row.Id());},[](const QuestRewardsConfig& a,const QuestRewardsConfig& b){return a.Id() == b.Id();}), index_Group_ById(rows_,false,[](const QuestRewardsConfig& row){return Index_Group_ById::hash(row.Id());},[](const QuestRewardsConfig& a,const QuestRewardsConfig& b){return a.Id() == b.Id();}) {
    }
    QuestRewardsTable(const QuestRewardsTable&) = delete;
    QuestRewardsTable& operator=(const QuestRewardsTable&) = delete;
    static std::string schema() { return "{\"name\":\"QuestRewards\",\"hash\":\"C839B7EDFA92304709824DABBA84B5534E34810224D2814C221E6F4A3B536E35\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"任务编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"任务名称\"},{\"name\":\"Item\",\"type\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"description\":\"引用 ItemCatalog 的 Id\"},{\"name\":\"Count\",\"type\":\"System.Int32\",\"description\":\"奖励数量：大于零\"},{\"name\":\"RequiredLevel\",\"type\":\"System.Int32\",\"description\":\"开放等级：大于零\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否启用\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"ItemCatalog\"}],\"tags\":[],\"dependencies\":[\"ItemCatalog\"]}"; }
    const std::vector<QuestRewardsConfig>& rows() const noexcept { return rows_; }
    std::size_t count() const noexcept { return rows_.size(); }
    const QuestRewardsConfig* find(const std::int32_t& k0) const {return index_Key.row(index_Key.find(k0));}
    const QuestRewardsConfig& get(const std::int32_t& k0) const {auto p=find(k0);if(!p)invalid("配置主键不存在");return *p;}
    const std::vector<const QuestRewardsConfig*>& getById(const std::int32_t& k0) const {return index_Group_ById.group(index_Group_ById.find(k0));}
};
} // namespace zrconfig
