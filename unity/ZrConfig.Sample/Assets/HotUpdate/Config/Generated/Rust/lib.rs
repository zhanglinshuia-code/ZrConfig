// zrconfig-api-contract:1
// 此文件由工具生成；所有权由 Arc 快照管理，查询不复制记录。
#![allow(non_snake_case, non_camel_case_types, dead_code, unused_imports)]
pub mod runtime;
pub mod types;
pub mod snapshot;
pub use snapshot::{ConfigManager, ConfigSnapshot};
#[path = "AllTypesConfig.rs"]
pub mod table_AllTypes;
#[path = "AllTypes10kConfig.rs"]
pub mod table_AllTypes10k;
#[path = "DropConfig.rs"]
pub mod table_Drop;
#[path = "HeroConfig.rs"]
pub mod table_Hero;
#[path = "ItemCatalogConfig.rs"]
pub mod table_ItemCatalog;
#[path = "QuestRewardsConfig.rs"]
pub mod table_QuestRewards;
#[path = "ShopOffersConfig.rs"]
pub mod table_ShopOffers;
#[path = "SkillConfig.rs"]
pub mod table_Skill;
#[path = "SkillLevelConfig.rs"]
pub mod table_SkillLevel;
pub mod server;
pub mod data;
