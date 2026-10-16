// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
import * as R from './runtime';
import type * as T from './types';
import type { ConfigSnapshot } from './snapshot';
export interface ItemCatalogConfig {
    // 道具编号
    readonly "Id": number;
    // 显示名称
    readonly "Name": string;
    // 堆叠上限：1～10000
    readonly "MaxStack": number;
    // 品质：1～5
    readonly "Rarity": number;
    // 是否启用
    readonly "Enabled": boolean;
}
const schema = "{\"name\":\"ItemCatalog\",\"hash\":\"C62E86119B370A102C131E82385650005AABD6EC76A0074028B958400781BEE0\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"道具编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"显示名称\"},{\"name\":\"MaxStack\",\"type\":\"System.Int32\",\"description\":\"堆叠上限：1～10000\"},{\"name\":\"Rarity\",\"type\":\"System.Int32\",\"description\":\"品质：1～5\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否启用\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}";
export class ItemCatalogTable {
    readonly rows: readonly ItemCatalogConfig[];
    readonly #index_Key: Map<any, any>;
    readonly #index_Group_ById: Map<any, any>;
    readonly #ids = new Map<number, ItemCatalogConfig>();
    constructor(bundle: R.Bundle, preferJSON: boolean) {
        this.rows = Object.freeze(bundle.load(schema, preferJSON));
        this.#index_Key = R.makeIndex(this.rows, ["Id"], true);
        this.#index_Group_ById = R.makeIndex(this.rows, ["Id"], false);
        for (const row of this.rows) { if (this.#ids.has(row.Id)) R.invalid('引用 Id 不唯一'); this.#ids.set(row.Id, row); }
    }
    bind(snapshot: ConfigSnapshot): void {
        for (const row of this.rows) Object.freeze(row);
        Object.freeze(this);
    }
    get count(): number { return this.rows.length; }
    byId(id: number): ItemCatalogConfig | undefined { return this.#ids.get(id); }
    find(k0: number): ItemCatalogConfig | undefined {
        let node: any = this.#index_Key;
        node = node.get(R.key(k0)); if (node === undefined) return undefined;
        return node;
    }
    get(k0: number): ItemCatalogConfig { return this.find(k0) ?? R.invalid('配置主键不存在'); }
    getById(k0: number): readonly ItemCatalogConfig[] {
        let node: any = this.#index_Group_ById;
        node = node.get(R.key(k0)); if (node === undefined) return R.empty;
        return node;
    }
}
