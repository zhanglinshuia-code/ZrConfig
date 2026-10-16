// zrconfig-api-contract:1
// Server entry: ephemeron ownership keeps the complete original snapshot alive.
import * as R from './runtime';
import { ConfigSnapshot } from './snapshot';
export { ServerManager, Cancellation, ConfigError } from './runtime';
export function load(root: string, preferJSON = false): ConfigSnapshot {
    const snapshot = ConfigSnapshot.load(root, preferJSON);
    R.retainOwner(snapshot.AllTypes, snapshot);
    for (const row of snapshot.AllTypes.rows) R.retainOwner(row, snapshot);
    R.retainOwner(snapshot.AllTypes10k, snapshot);
    for (const row of snapshot.AllTypes10k.rows) R.retainOwner(row, snapshot);
    R.retainOwner(snapshot.Drop, snapshot);
    for (const row of snapshot.Drop.rows) R.retainOwner(row, snapshot);
    R.retainOwner(snapshot.Hero, snapshot);
    for (const row of snapshot.Hero.rows) R.retainOwner(row, snapshot);
    R.retainOwner(snapshot.ItemCatalog, snapshot);
    for (const row of snapshot.ItemCatalog.rows) R.retainOwner(row, snapshot);
    R.retainOwner(snapshot.QuestRewards, snapshot);
    for (const row of snapshot.QuestRewards.rows) R.retainOwner(row, snapshot);
    R.retainOwner(snapshot.ShopOffers, snapshot);
    for (const row of snapshot.ShopOffers.rows) R.retainOwner(row, snapshot);
    R.retainOwner(snapshot.Skill, snapshot);
    for (const row of snapshot.Skill.rows) R.retainOwner(row, snapshot);
    R.retainOwner(snapshot.SkillLevel, snapshot);
    for (const row of snapshot.SkillLevel.rows) R.retainOwner(row, snapshot);
    return snapshot;
}
export function manager(): R.ServerManager<ConfigSnapshot> { return new R.ServerManager<ConfigSnapshot>(); }
export const catalog = new R.Catalog("{\"format\":2,\"fingerprint\":\"ADE42D7364E51BC5A62B63F35E32A11E50424B1A690E12E56AFD23760D81DDC6\",\"tables\":[{\"name\":\"AllTypes\",\"schemaHash\":\"E8017397D24CBD0FDA2008AFD4F2BA2BA68619A29F4D4D79F7973F06BC8360B1\",\"dependencies\":[],\"component\":0},{\"name\":\"AllTypes10k\",\"schemaHash\":\"34B94C4229D85CC8B37832A203504B7E6EF4447307ADB831AB5BD01628EA7432\",\"dependencies\":[\"AllTypes10k\",\"Hero\",\"Skill\"],\"component\":1},{\"name\":\"Drop\",\"schemaHash\":\"9DAF981CB794247A5F399C8DFA97320D99D670CC605BE1E071D33E596021A242\",\"dependencies\":[],\"component\":2},{\"name\":\"Hero\",\"schemaHash\":\"7E1E75AC24BAFD42C18F1B43A19B0027A3D241CC665DB3727BC98A4908927ECE\",\"dependencies\":[\"Hero\",\"Skill\"],\"component\":3},{\"name\":\"ItemCatalog\",\"schemaHash\":\"C62E86119B370A102C131E82385650005AABD6EC76A0074028B958400781BEE0\",\"dependencies\":[],\"component\":4},{\"name\":\"QuestRewards\",\"schemaHash\":\"C839B7EDFA92304709824DABBA84B5534E34810224D2814C221E6F4A3B536E35\",\"dependencies\":[\"ItemCatalog\"],\"component\":5},{\"name\":\"ShopOffers\",\"schemaHash\":\"0376659B4746D22E616351BEAEFC78651E69BFCEFF2D7ECAB8BD17539864E8E1\",\"dependencies\":[\"ItemCatalog\"],\"component\":6},{\"name\":\"Skill\",\"schemaHash\":\"51DA7601418A98574B1C5ADC656CB7E7B479CC1F3E5237DFC5BA7532F702B9DB\",\"dependencies\":[\"Hero\"],\"component\":3},{\"name\":\"SkillLevel\",\"schemaHash\":\"9DDEDF4A13E49BE883FA11B62A966463D9B27C99620857DE5CD5E721CECC92B2\",\"dependencies\":[],\"component\":7}],\"components\":[[\"AllTypes\"],[\"AllTypes10k\"],[\"Drop\"],[\"Hero\",\"Skill\"],[\"ItemCatalog\"],[\"QuestRewards\"],[\"ShopOffers\"],[\"SkillLevel\"]],\"target\":\"C\",\"runtimeContract\":\"server-v1\",\"requiredCapabilities\":[\"binary-v4\",\"json-v1\",\"server-default\",\"selected-closure\",\"lazy-strict\",\"lazy-on-access\",\"fixed-source-bytes\",\"snapshot-owned-handles\",\"explicit-reference-load\",\"row-cache\",\"group-cache\",\"typed-warmup\",\"bounded-source\",\"structured-errors\",\"manager-lifecycle\"]}");
import { AllTypesTable } from './AllTypesConfig';
import { AllTypes10kTable } from './AllTypes10kConfig';
import { DropTable } from './DropConfig';
import { HeroTable } from './HeroConfig';
import { ItemCatalogTable } from './ItemCatalogConfig';
import { QuestRewardsTable } from './QuestRewardsConfig';
import { ShopOffersTable } from './ShopOffersConfig';
import { SkillTable } from './SkillConfig';
import { SkillLevelTable } from './SkillLevelConfig';
export class SelectedSnapshot {
    readonly version: string;
    readonly selection: R.SelectionPlan;
    #storage: Record<string, any> = Object.create(null);
    private constructor(bundle: R.Bundle, selection: R.SelectionPlan, preferJSON: boolean) {
        bundle.validateCatalog(catalog);
        this.version = bundle.version; this.selection = selection;
        const selected = new Set(this.selection.tables);

        if (selected.has("AllTypes")) this.#storage.AllTypes = new AllTypesTable(bundle, preferJSON);
        if (selected.has("AllTypes10k")) this.#storage.AllTypes10k = new AllTypes10kTable(bundle, preferJSON);
        if (selected.has("Drop")) this.#storage.Drop = new DropTable(bundle, preferJSON);
        if (selected.has("Hero")) this.#storage.Hero = new HeroTable(bundle, preferJSON);
        if (selected.has("ItemCatalog")) this.#storage.ItemCatalog = new ItemCatalogTable(bundle, preferJSON);
        if (selected.has("QuestRewards")) this.#storage.QuestRewards = new QuestRewardsTable(bundle, preferJSON);
        if (selected.has("ShopOffers")) this.#storage.ShopOffers = new ShopOffersTable(bundle, preferJSON);
        if (selected.has("Skill")) this.#storage.Skill = new SkillTable(bundle, preferJSON);
        if (selected.has("SkillLevel")) this.#storage.SkillLevel = new SkillLevelTable(bundle, preferJSON);
        if (selected.has("AllTypes")) this.#storage.AllTypes.bind(this.#storage as ConfigSnapshot);
        if (selected.has("AllTypes10k")) this.#storage.AllTypes10k.bind(this.#storage as ConfigSnapshot);
        if (selected.has("Drop")) this.#storage.Drop.bind(this.#storage as ConfigSnapshot);
        if (selected.has("Hero")) this.#storage.Hero.bind(this.#storage as ConfigSnapshot);
        if (selected.has("ItemCatalog")) this.#storage.ItemCatalog.bind(this.#storage as ConfigSnapshot);
        if (selected.has("QuestRewards")) this.#storage.QuestRewards.bind(this.#storage as ConfigSnapshot);
        if (selected.has("ShopOffers")) this.#storage.ShopOffers.bind(this.#storage as ConfigSnapshot);
        if (selected.has("Skill")) this.#storage.Skill.bind(this.#storage as ConfigSnapshot);
        if (selected.has("SkillLevel")) this.#storage.SkillLevel.bind(this.#storage as ConfigSnapshot);
        if (selected.has("AllTypes")) { R.retainOwner(this.#storage.AllTypes, this); for (const row of this.#storage.AllTypes.rows) R.retainOwner(row, this); }
        if (selected.has("AllTypes10k")) { R.retainOwner(this.#storage.AllTypes10k, this); for (const row of this.#storage.AllTypes10k.rows) R.retainOwner(row, this); }
        if (selected.has("Drop")) { R.retainOwner(this.#storage.Drop, this); for (const row of this.#storage.Drop.rows) R.retainOwner(row, this); }
        if (selected.has("Hero")) { R.retainOwner(this.#storage.Hero, this); for (const row of this.#storage.Hero.rows) R.retainOwner(row, this); }
        if (selected.has("ItemCatalog")) { R.retainOwner(this.#storage.ItemCatalog, this); for (const row of this.#storage.ItemCatalog.rows) R.retainOwner(row, this); }
        if (selected.has("QuestRewards")) { R.retainOwner(this.#storage.QuestRewards, this); for (const row of this.#storage.QuestRewards.rows) R.retainOwner(row, this); }
        if (selected.has("ShopOffers")) { R.retainOwner(this.#storage.ShopOffers, this); for (const row of this.#storage.ShopOffers.rows) R.retainOwner(row, this); }
        if (selected.has("Skill")) { R.retainOwner(this.#storage.Skill, this); for (const row of this.#storage.Skill.rows) R.retainOwner(row, this); }
        if (selected.has("SkillLevel")) { R.retainOwner(this.#storage.SkillLevel, this); for (const row of this.#storage.SkillLevel.rows) R.retainOwner(row, this); }
        Object.freeze(this);
    }
    static load(root: string, requested: readonly string[], preferJSON = false): SelectedSnapshot { const selection = catalog.select(requested); return new SelectedSnapshot(new R.Bundle(root), selection, preferJSON); }
    get_AllTypes(): AllTypesTable { const value = this.#storage.AllTypes; if (!value) throw new R.ConfigError('TableNotSelected', "AllTypes: table not selected"); return value; }
    get_AllTypes10k(): AllTypes10kTable { const value = this.#storage.AllTypes10k; if (!value) throw new R.ConfigError('TableNotSelected', "AllTypes10k: table not selected"); return value; }
    get_Drop(): DropTable { const value = this.#storage.Drop; if (!value) throw new R.ConfigError('TableNotSelected', "Drop: table not selected"); return value; }
    get_Hero(): HeroTable { const value = this.#storage.Hero; if (!value) throw new R.ConfigError('TableNotSelected', "Hero: table not selected"); return value; }
    get_ItemCatalog(): ItemCatalogTable { const value = this.#storage.ItemCatalog; if (!value) throw new R.ConfigError('TableNotSelected', "ItemCatalog: table not selected"); return value; }
    get_QuestRewards(): QuestRewardsTable { const value = this.#storage.QuestRewards; if (!value) throw new R.ConfigError('TableNotSelected', "QuestRewards: table not selected"); return value; }
    get_ShopOffers(): ShopOffersTable { const value = this.#storage.ShopOffers; if (!value) throw new R.ConfigError('TableNotSelected', "ShopOffers: table not selected"); return value; }
    get_Skill(): SkillTable { const value = this.#storage.Skill; if (!value) throw new R.ConfigError('TableNotSelected', "Skill: table not selected"); return value; }
    get_SkillLevel(): SkillLevelTable { const value = this.#storage.SkillLevel; if (!value) throw new R.ConfigError('TableNotSelected', "SkillLevel: table not selected"); return value; }
}
