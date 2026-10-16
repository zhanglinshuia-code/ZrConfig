// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
import * as R from './runtime';
import type * as T from './types';
import type { ConfigSnapshot } from './snapshot';
import type { ItemCatalogConfig } from './ItemCatalogConfig';
export interface QuestRewardsConfig {
    // 任务编号
    readonly "Id": number;
    // 任务名称
    readonly "Name": string;
    // 引用 ItemCatalog 的 Id
    readonly "ItemId": number;
    readonly "Item": ItemCatalogConfig | null;
    // 奖励数量：大于零
    readonly "Count": number;
    // 开放等级：大于零
    readonly "RequiredLevel": number;
    // 是否启用
    readonly "Enabled": boolean;
}
const schema = "{\"name\":\"QuestRewards\",\"hash\":\"C839B7EDFA92304709824DABBA84B5534E34810224D2814C221E6F4A3B536E35\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"任务编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"任务名称\"},{\"name\":\"Item\",\"type\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"description\":\"引用 ItemCatalog 的 Id\"},{\"name\":\"Count\",\"type\":\"System.Int32\",\"description\":\"奖励数量：大于零\"},{\"name\":\"RequiredLevel\",\"type\":\"System.Int32\",\"description\":\"开放等级：大于零\"},{\"name\":\"Enabled\",\"type\":\"System.Boolean\",\"description\":\"是否启用\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Boolean\",\"kind\":\"primitive\",\"primitive\":\"bool\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.ItemCatalogConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"ItemCatalog\"}],\"tags\":[],\"dependencies\":[\"ItemCatalog\"]}";
export class QuestRewardsTable {
    readonly rows: readonly QuestRewardsConfig[];
    readonly #index_Key: Map<any, any>;
    readonly #index_Group_ById: Map<any, any>;
    constructor(bundle: R.Bundle, preferJSON: boolean) {
        this.rows = Object.freeze(bundle.load(schema, preferJSON));
        this.#index_Key = R.makeIndex(this.rows, ["Id"], true);
        this.#index_Group_ById = R.makeIndex(this.rows, ["Id"], false);
        for (const row of this.rows as any[]) { row["ItemId"] = row["Item"]; row["Item"] = null; }
    }
    bind(snapshot: ConfigSnapshot): void {
        for (const row of this.rows as any[]) if (row["ItemId"] !== 0) row["Item"] = snapshot.ItemCatalog.byId(row["ItemId"]) ?? R.invalid("引用目标不存在：QuestRewards.Item");
        for (const row of this.rows) Object.freeze(row);
        Object.freeze(this);
    }
    get count(): number { return this.rows.length; }
    find(k0: number): QuestRewardsConfig | undefined {
        let node: any = this.#index_Key;
        node = node.get(R.key(k0)); if (node === undefined) return undefined;
        return node;
    }
    get(k0: number): QuestRewardsConfig { return this.find(k0) ?? R.invalid('配置主键不存在'); }
    getById(k0: number): readonly QuestRewardsConfig[] {
        let node: any = this.#index_Group_ById;
        node = node.get(R.key(k0)); if (node === undefined) return R.empty;
        return node;
    }
}
