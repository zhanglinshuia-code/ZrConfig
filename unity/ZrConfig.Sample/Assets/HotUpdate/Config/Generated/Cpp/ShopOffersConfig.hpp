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
class ShopOffersConfig {
    friend class ShopOffersTable;
    std::int32_t v_Id;
    std::int32_t v_Item;
    const ItemCatalogConfig* ref_Item = nullptr;
    std::int32_t v_Price;
    std::int32_t v_DailyLimit;
    bool v_Enabled;
public:
    explicit ShopOffersConfig(const Json& value) : v_Id(integer<std::int32_t>(value.field("Id"))),v_Item(integer<std::int32_t>(value.field("Item"))),v_Price(integer<std::int32_t>(value.field("Price"))),v_DailyLimit(integer<std::int32_t>(value.field("DailyLimit"))),v_Enabled(value.field("Enabled").boolean()) {}
    // 商品编号
    const std::int32_t& Id() const noexcept { return v_Id; }
    // 引用 ItemCatalog 的 Id
    const std::int32_t& ItemId() const noexcept { return v_Item; }
    const ItemCatalogConfig* Item() const noexcept { return ref_Item; }
    // 价格：非负整数；0 为免费
    const std::int32_t& Price() const noexcept { return v_Price; }
    // 每日限购：0 为不限购
    const std::int32_t& DailyLimit() const noexcept { return v_DailyLimit; }
    // 是否上架
    const bool& Enabled() const noexcept { return v_Enabled; }
};
class ShopOffersTable {
    friend class ConfigSnapshot;
    std::vector<ShopOffersConfig> rows_;
    struct Index_Key : Index<ShopOffersConfig> {
        using Index<ShopOffersConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Id() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Key;
    struct Index_Group_ById : Index<ShopOffersConfig> {
        using Index<ShopOffersConfig>::Index;
        static std::size_t hash(const std::int32_t& k0) { return (1 * 31u + Hash<std::int32_t>{}(k0)); }
        std::size_t find(const std::int32_t& k0) const {
            auto slot=mix(hash(k0))&mask_;
            while(slots_[slot]!=missing) { const auto& row=rows_[slots_[slot]]; if(row.Id() == k0) return slot; slot=(slot+1)&mask_; }
            return missing;
        }
    } index_Group_ById;
    static std::vector<ShopOffersConfig> decode(const std::vector<Json>& source) { std::vector<ShopOffersConfig> rows;rows.reserve(source.size());for(const auto& value:source)rows.emplace_back(value);return rows; }
    void bind(const ConfigSnapshot& snapshot);
public:
    explicit ShopOffersTable(const std::vector<Json>& source) : rows_(decode(source)), index_Key(rows_,true,[](const ShopOffersConfig& row){return Index_Key::hash(row.Id());},[](const ShopOffersConfig& a,const ShopOffersConfig& b){return a.Id() == b.Id();}), index_Group_ById(rows_,false,[](const ShopOffersConfig& row){return Index_Group_ById::hash(row.Id());},[](const ShopOffersConfig& a,const ShopOffersConfig& b){return a.Id() == b.Id();}) {
    }
    ShopOffersTable(const ShopOffersTable&) = delete;
    ShopOffersTable& operator=(const ShopOffersTable&) = delete;
    static std::string schema() { return "{\"name\":\"ShopOffers\",\"hash\":\"0376659B4746D22E616351BEAEFC78651E69BFCEFF2D7ECAB8BD17539864E8E1\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"商品编号\"},{\"name\":\"Item\",\"type\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"description\":\"引用 ItemCatalog 的 Id\"},{\"name\":\"Price\",\"type\":\"System.Int32\",\"description\":\"价格：非负整数；0 为免费\"},{\"name\":\"DailyLimit\",\"type\":\"System.Int32\",\"description\":\"每日限购：0 为不限购\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否上架\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"ItemCatalog\"}],\"tags\":[],\"dependencies\":[\"ItemCatalog\"]}"; }
    const std::vector<ShopOffersConfig>& rows() const noexcept { return rows_; }
    std::size_t count() const noexcept { return rows_.size(); }
    const ShopOffersConfig* find(const std::int32_t& k0) const {return index_Key.row(index_Key.find(k0));}
    const ShopOffersConfig& get(const std::int32_t& k0) const {auto p=find(k0);if(!p)invalid("配置主键不存在");return *p;}
    const std::vector<const ShopOffersConfig*>& getById(const std::int32_t& k0) const {return index_Group_ById.group(index_Group_ById.find(k0));}
};
} // namespace zrconfig
