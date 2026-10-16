// zrconfig-api-contract:1
// 持有快照期间，记录和引用地址保持稳定；查询本身不分配内存。
#pragma once
#include "types.hpp"
#include "AllTypesConfig.hpp"
#include "AllTypes10kConfig.hpp"
#include "DropConfig.hpp"
#include "HeroConfig.hpp"
#include "ItemCatalogConfig.hpp"
#include "QuestRewardsConfig.hpp"
#include "ShopOffersConfig.hpp"
#include "SkillConfig.hpp"
#include "SkillLevelConfig.hpp"
namespace zrconfig {
class ConfigSnapshot {
    friend class server::Snapshot;
    std::string version_;
    std::unique_ptr<AllTypesTable> table_AllTypes;
    std::unique_ptr<AllTypes10kTable> table_AllTypes10k;
    std::unique_ptr<DropTable> table_Drop;
    std::unique_ptr<HeroTable> table_Hero;
    std::unique_ptr<ItemCatalogTable> table_ItemCatalog;
    std::unique_ptr<QuestRewardsTable> table_QuestRewards;
    std::unique_ptr<ShopOffersTable> table_ShopOffers;
    std::unique_ptr<SkillTable> table_Skill;
    std::unique_ptr<SkillLevelTable> table_SkillLevel;
    ConfigSnapshot(Bundle& bundle, [[maybe_unused]] bool prefer_json, [[maybe_unused]] const std::unordered_set<std::string>* selected = nullptr) : version_(bundle.version), table_AllTypes(!selected || selected->count("AllTypes") ? std::make_unique<AllTypesTable>(bundle.load(AllTypesTable::schema(), prefer_json)) : nullptr), table_AllTypes10k(!selected || selected->count("AllTypes10k") ? std::make_unique<AllTypes10kTable>(bundle.load(AllTypes10kTable::schema(), prefer_json)) : nullptr), table_Drop(!selected || selected->count("Drop") ? std::make_unique<DropTable>(bundle.load(DropTable::schema(), prefer_json)) : nullptr), table_Hero(!selected || selected->count("Hero") ? std::make_unique<HeroTable>(bundle.load(HeroTable::schema(), prefer_json)) : nullptr), table_ItemCatalog(!selected || selected->count("ItemCatalog") ? std::make_unique<ItemCatalogTable>(bundle.load(ItemCatalogTable::schema(), prefer_json)) : nullptr), table_QuestRewards(!selected || selected->count("QuestRewards") ? std::make_unique<QuestRewardsTable>(bundle.load(QuestRewardsTable::schema(), prefer_json)) : nullptr), table_ShopOffers(!selected || selected->count("ShopOffers") ? std::make_unique<ShopOffersTable>(bundle.load(ShopOffersTable::schema(), prefer_json)) : nullptr), table_Skill(!selected || selected->count("Skill") ? std::make_unique<SkillTable>(bundle.load(SkillTable::schema(), prefer_json)) : nullptr), table_SkillLevel(!selected || selected->count("SkillLevel") ? std::make_unique<SkillLevelTable>(bundle.load(SkillLevelTable::schema(), prefer_json)) : nullptr) {
        if (table_AllTypes) table_AllTypes->bind(*this);
        if (table_AllTypes10k) table_AllTypes10k->bind(*this);
        if (table_Drop) table_Drop->bind(*this);
        if (table_Hero) table_Hero->bind(*this);
        if (table_ItemCatalog) table_ItemCatalog->bind(*this);
        if (table_QuestRewards) table_QuestRewards->bind(*this);
        if (table_ShopOffers) table_ShopOffers->bind(*this);
        if (table_Skill) table_Skill->bind(*this);
        if (table_SkillLevel) table_SkillLevel->bind(*this);
    }
public:
    ConfigSnapshot(const ConfigSnapshot&) = delete;
    ConfigSnapshot& operator=(const ConfigSnapshot&) = delete;
    const std::string& version() const noexcept { return version_; }
    static std::shared_ptr<const ConfigSnapshot> load(const std::filesystem::path& root, bool prefer_json = false) { Bundle bundle(root); bundle.validate_tables({"AllTypes", "AllTypes10k", "Drop", "Hero", "ItemCatalog", "QuestRewards", "ShopOffers", "Skill", "SkillLevel"}); return std::shared_ptr<const ConfigSnapshot>(new ConfigSnapshot(bundle,prefer_json)); }
    const AllTypesTable& AllTypes() const noexcept { return *table_AllTypes; }
    const AllTypes10kTable& AllTypes10k() const noexcept { return *table_AllTypes10k; }
    const DropTable& Drop() const noexcept { return *table_Drop; }
    const HeroTable& Hero() const noexcept { return *table_Hero; }
    const ItemCatalogTable& ItemCatalog() const noexcept { return *table_ItemCatalog; }
    const QuestRewardsTable& QuestRewards() const noexcept { return *table_QuestRewards; }
    const ShopOffersTable& ShopOffers() const noexcept { return *table_ShopOffers; }
    const SkillTable& Skill() const noexcept { return *table_Skill; }
    const SkillLevelTable& SkillLevel() const noexcept { return *table_SkillLevel; }
};
class ConfigManager {
    mutable std::mutex gate_;
    std::shared_ptr<const ConfigSnapshot> current_;
public:
    std::shared_ptr<const ConfigSnapshot> current() const { return std::atomic_load_explicit(&current_,std::memory_order_acquire); }
    std::shared_ptr<const ConfigSnapshot> reload(const std::filesystem::path& root,bool prefer_json=false) { std::lock_guard<std::mutex> lock(gate_);auto next=ConfigSnapshot::load(root,prefer_json);std::atomic_store_explicit(&current_,next,std::memory_order_release);return next; }
};
inline void AllTypesTable::bind([[maybe_unused]] const ConfigSnapshot& snapshot) {
}
inline void AllTypes10kTable::bind([[maybe_unused]] const ConfigSnapshot& snapshot) {
    for(auto& row:rows_)if(row.v_Hero!=0){row.ref_Hero=snapshot.Hero().by_id(row.v_Hero);if(!row.ref_Hero)invalid("引用目标不存在：AllTypes10k.Hero");}
    for(auto& row:rows_)if(row.v_Skill!=0){row.ref_Skill=snapshot.Skill().by_id(row.v_Skill);if(!row.ref_Skill)invalid("引用目标不存在：AllTypes10k.Skill");}
    for(auto& row:rows_)if(row.v_Previous!=0){row.ref_Previous=snapshot.AllTypes10k().by_id(row.v_Previous);if(!row.ref_Previous)invalid("引用目标不存在：AllTypes10k.Previous");}
}
inline void DropTable::bind([[maybe_unused]] const ConfigSnapshot& snapshot) {
}
inline void HeroTable::bind([[maybe_unused]] const ConfigSnapshot& snapshot) {
    for(auto& row:rows_)if(row.v_Skill!=0){row.ref_Skill=snapshot.Skill().by_id(row.v_Skill);if(!row.ref_Skill)invalid("引用目标不存在：Hero.Skill");}
    for(auto& row:rows_)if(row.v_OtherSkill!=0){row.ref_OtherSkill=snapshot.Skill().by_id(row.v_OtherSkill);if(!row.ref_OtherSkill)invalid("引用目标不存在：Hero.OtherSkill");}
    for(auto& row:rows_)if(row.v_Mentor!=0){row.ref_Mentor=snapshot.Hero().by_id(row.v_Mentor);if(!row.ref_Mentor)invalid("引用目标不存在：Hero.Mentor");}
}
inline void ItemCatalogTable::bind([[maybe_unused]] const ConfigSnapshot& snapshot) {
}
inline void QuestRewardsTable::bind([[maybe_unused]] const ConfigSnapshot& snapshot) {
    for(auto& row:rows_)if(row.v_Item!=0){row.ref_Item=snapshot.ItemCatalog().by_id(row.v_Item);if(!row.ref_Item)invalid("引用目标不存在：QuestRewards.Item");}
}
inline void ShopOffersTable::bind([[maybe_unused]] const ConfigSnapshot& snapshot) {
    for(auto& row:rows_)if(row.v_Item!=0){row.ref_Item=snapshot.ItemCatalog().by_id(row.v_Item);if(!row.ref_Item)invalid("引用目标不存在：ShopOffers.Item");}
}
inline void SkillTable::bind([[maybe_unused]] const ConfigSnapshot& snapshot) {
    for(auto& row:rows_)if(row.v_Hero!=0){row.ref_Hero=snapshot.Hero().by_id(row.v_Hero);if(!row.ref_Hero)invalid("引用目标不存在：Skill.Hero");}
}
inline void SkillLevelTable::bind([[maybe_unused]] const ConfigSnapshot& snapshot) {
}
} // namespace zrconfig
