// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
import * as R from './runtime';
import type * as T from './types';
import type { ConfigSnapshot } from './snapshot';
import type { ItemCatalogConfig } from './ItemCatalogConfig';
export interface ShopOffersConfig {
    // 商品编号
    readonly "Id": number;
    // 引用 ItemCatalog 的 Id
    readonly "ItemId": number;
    readonly "Item": ItemCatalogConfig | null;
    // 价格：非负整数；0 为免费
    readonly "Price": number;
    // 每日限购：0 为不限购
    readonly "DailyLimit": number;
    // 是否上架
    readonly "Enabled": boolean;
}
const schema = "{\"name\":\"ShopOffers\",\"hash\":\"0376659B4746D22E616351BEAEFC78651E69BFCEFF2D7ECAB8BD17539864E8E1\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"商品编号\"},{\"name\":\"Item\",\"type\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"description\":\"引用 ItemCatalog 的 Id\"},{\"name\":\"Price\",\"type\":\"System.Int32\",\"description\":\"价格：非负整数；0 为免费\"},{\"name\":\"DailyLimit\",\"type\":\"System.Int32\",\"description\":\"每日限购：0 为不限购\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否上架\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"ItemCatalog\"}],\"tags\":[],\"dependencies\":[\"ItemCatalog\"]}";
export class ShopOffersTable {
    readonly rows: readonly ShopOffersConfig[];
    readonly #index_Key: Map<any, any>;
    readonly #index_Group_ById: Map<any, any>;
    constructor(bundle: R.Bundle, preferJSON: boolean) {
        this.rows = Object.freeze(bundle.load(schema, preferJSON));
        this.#index_Key = R.makeIndex(this.rows, ["Id"], true);
        this.#index_Group_ById = R.makeIndex(this.rows, ["Id"], false);
        for (const row of this.rows as any[]) { row["ItemId"] = row["Item"]; row["Item"] = null; }
    }
    bind(snapshot: ConfigSnapshot): void {
        for (const row of this.rows as any[]) if (row["ItemId"] !== 0) row["Item"] = snapshot.ItemCatalog.byId(row["ItemId"]) ?? R.invalid("引用目标不存在：ShopOffers.Item");
        for (const row of this.rows) Object.freeze(row);
        Object.freeze(this);
    }
    get count(): number { return this.rows.length; }
    find(k0: number): ShopOffersConfig | undefined {
        let node: any = this.#index_Key;
        node = node.get(R.key(k0)); if (node === undefined) return undefined;
        return node;
    }
    get(k0: number): ShopOffersConfig { return this.find(k0) ?? R.invalid('配置主键不存在'); }
    getById(k0: number): readonly ShopOffersConfig[] {
        let node: any = this.#index_Group_ById;
        node = node.get(R.key(k0)); if (node === undefined) return R.empty;
        return node;
    }
}
