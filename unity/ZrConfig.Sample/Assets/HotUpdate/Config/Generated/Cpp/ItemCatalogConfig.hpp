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
class ItemCatalogConfig {
    friend class ItemCatalogTable;
    std::int32_t v_Id;
    std::string v_Name;
    std::int32_t v_MaxStack;
    std::int32_t v_Rarity;
    bool v_Enabled;
public:
    explicit ItemCatalogConfig(const Json& value) : v_Id(integer<std::int32_t>(value.field("Id"))),v_Name(value.field("Name").string()),v_MaxStack(integer<std::int32_t>(value.field("MaxStack"))),v_Rarity(integer<std::int32_t>(value.field("Rarity"))),v_Enabled(value.field("Enabled").boolean()) {}
    // 道具编号
    const std::int32_t& Id() const noexcept { return v_Id; }
    // 显示名称
    const std::string& Name() const noexcept { return v_Name; }
    // 堆叠上限：1～10000
    const std::int32_t& MaxStack() const noexcept { return v_MaxStack; }
    // 品质：1～5
    const std::int32_t& Rarity() const noexcept { return v_Rarity; }
    // 是否启用
    const bool& Enabled() const noexcept { return v_Enabled; }
};
class ItemCatalogTable {
    friend class ConfigSnapshot;
    std::vector<ItemCatalogConfig> rows_;
    struct Index_Key : Index<ItemCatalogConfig> {
        using Index<ItemCatalogConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Id() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Key;
    struct Index_Group_ById : Index<ItemCatalogConfig> {
        using Index<ItemCatalogConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Id() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Group_ById;
    std::unordered_map<std::int32_t,const ItemCatalogConfig*> ids_;
    static std::vector<ItemCatalogConfig> decode(const std::vector<Json>& source) { std::vector<ItemCatalogConfig> rows;rows.reserve(source.size());for(const auto& value:source)rows.emplace_back(value);return rows; }
    void bind(const ConfigSnapshot& snapshot);
public:
    explicit ItemCatalogTable(const std::vector<Json>& source) : rows_(decode(source)), index_Key(rows_,true,[](const ItemCatalogConfig& row){return Index_Key::hash(row.Id());},[](const ItemCatalogConfig& a,const ItemCatalogConfig& b){return a.Id() == b.Id();}), index_Group_ById(rows_,false,[](const ItemCatalogConfig& row){return Index_Group_ById::hash(row.Id());},[](const ItemCatalogConfig& a,const ItemCatalogConfig& b){return a.Id() == b.Id();}) {
        ids_.reserve(rows_.size());for(const auto& row:rows_)if(!ids_.emplace(row.Id(),&row).second)invalid("引用 Id 不唯一");
    }
    ItemCatalogTable(const ItemCatalogTable&) = delete;
    ItemCatalogTable& operator=(const ItemCatalogTable&) = delete;
    static std::string schema() { return "{\"name\":\"ItemCatalog\",\"hash\":\"C62E86119B370A102C131E82385650005AABD6EC76A0074028B958400781BEE0\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"道具编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"显示名称\"},{\"name\":\"MaxStack\",\"type\":\"System.Int32\",\"description\":\"堆叠上限：1～10000\"},{\"name\":\"Rarity\",\"type\":\"System.Int32\",\"description\":\"品质：1～5\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否启用\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}"; }
    const std::vector<ItemCatalogConfig>& rows() const noexcept { return rows_; }
    std::size_t count() const noexcept { return rows_.size(); }
    const ItemCatalogConfig* by_id(std::int32_t id) const { auto it=ids_.find(id);return it==ids_.end()?nullptr:it->second; }
    const ItemCatalogConfig* find(const std::int32_t& k0) const {return index_Key.row(index_Key.find(k0));}
    const ItemCatalogConfig& get(const std::int32_t& k0) const {auto p=find(k0);if(!p)invalid("配置主键不存在");return *p;}
    const std::vector<const ItemCatalogConfig*>& getById(const std::int32_t& k0) const {return index_Group_ById.group(index_Group_ById.find(k0));}
};
} // namespace zrconfig
