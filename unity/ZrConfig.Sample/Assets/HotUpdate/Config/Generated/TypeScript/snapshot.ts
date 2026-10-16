// zrconfig-api-contract:1
// 完整加载、引用绑定成功后发布快照；持有旧快照的请求继续使用旧数据。
import * as R from './runtime';
import { AllTypesTable } from './AllTypesConfig';
import { AllTypes10kTable } from './AllTypes10kConfig';
import { DropTable } from './DropConfig';
import { HeroTable } from './HeroConfig';
import { ItemCatalogTable } from './ItemCatalogConfig';
import { QuestRewardsTable } from './QuestRewardsConfig';
import { ShopOffersTable } from './ShopOffersConfig';
import { SkillTable } from './SkillConfig';
import { SkillLevelTable } from './SkillLevelConfig';
export class ConfigSnapshot {
    readonly version: string;
    readonly AllTypes: AllTypesTable;
    readonly AllTypes10k: AllTypes10kTable;
    readonly Drop: DropTable;
    readonly Hero: HeroTable;
    readonly ItemCatalog: ItemCatalogTable;
    readonly QuestRewards: QuestRewardsTable;
    readonly ShopOffers: ShopOffersTable;
    readonly Skill: SkillTable;
    readonly SkillLevel: SkillLevelTable;
    private constructor(bundle: R.Bundle, preferJSON: boolean) {
        this.version = bundle.version;
        bundle.validateTables(["AllTypes", "AllTypes10k", "Drop", "Hero", "ItemCatalog", "QuestRewards", "ShopOffers", "Skill", "SkillLevel"]);
        this.AllTypes = new AllTypesTable(bundle, preferJSON);
        this.AllTypes10k = new AllTypes10kTable(bundle, preferJSON);
        this.Drop = new DropTable(bundle, preferJSON);
        this.Hero = new HeroTable(bundle, preferJSON);
        this.ItemCatalog = new ItemCatalogTable(bundle, preferJSON);
        this.QuestRewards = new QuestRewardsTable(bundle, preferJSON);
        this.ShopOffers = new ShopOffersTable(bundle, preferJSON);
        this.Skill = new SkillTable(bundle, preferJSON);
        this.SkillLevel = new SkillLevelTable(bundle, preferJSON);
        this.AllTypes.bind(this);
        this.AllTypes10k.bind(this);
        this.Drop.bind(this);
        this.Hero.bind(this);
        this.ItemCatalog.bind(this);
        this.QuestRewards.bind(this);
        this.ShopOffers.bind(this);
        this.Skill.bind(this);
        this.SkillLevel.bind(this);
        Object.freeze(this);
    }
    static load(root: string, preferJSON = false): ConfigSnapshot { return new ConfigSnapshot(new R.Bundle(root), preferJSON); }
}
export class ConfigManager {
    #current: ConfigSnapshot | undefined;
    current(): ConfigSnapshot { return this.#current ?? R.invalid('配置尚未加载'); }
    reload(root: string, preferJSON = false): ConfigSnapshot {
        const next = ConfigSnapshot.load(root, preferJSON);
        this.#current = next;
        return next;
    }
}

