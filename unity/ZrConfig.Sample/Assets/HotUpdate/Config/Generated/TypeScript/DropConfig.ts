// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
import * as R from './runtime';
import type * as T from './types';
import type { ConfigSnapshot } from './snapshot';
export interface DropConfig {
    // 掉落组编号
    readonly "GroupId": number;
    // 物品编号
    readonly "Id": number;
    // 数量
    readonly "Count": number;
}
const schema = "{\"name\":\"Drop\",\"hash\":\"9DAF981CB794247A5F399C8DFA97320D99D670CC605BE1E071D33E596021A242\",\"hasId\":true,\"fields\":[{\"name\":\"GroupId\",\"type\":\"System.Int32\",\"description\":\"掉落组编号\"},{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"物品编号\"},{\"name\":\"Count\",\"type\":\"System.Int32\",\"description\":\"数量\"}],\"keys\":[\"GroupId\",\"Id\"],\"groups\":[{\"name\":\"ByGroupId\",\"fields\":[\"GroupId\"]},{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}";
export class DropTable {
    readonly rows: readonly DropConfig[];
    readonly #index_Key: Map<any, any>;
    readonly #index_Group_ByGroupId: Map<any, any>;
    readonly #index_Group_ById: Map<any, any>;
    constructor(bundle: R.Bundle, preferJSON: boolean) {
        this.rows = Object.freeze(bundle.load(schema, preferJSON));
        this.#index_Key = R.makeIndex(this.rows, ["GroupId", "Id"], true);
        this.#index_Group_ByGroupId = R.makeIndex(this.rows, ["GroupId"], false);
        this.#index_Group_ById = R.makeIndex(this.rows, ["Id"], false);
    }
    bind(snapshot: ConfigSnapshot): void {
        for (const row of this.rows) Object.freeze(row);
        Object.freeze(this);
    }
    get count(): number { return this.rows.length; }
    find(k0: number, k1: number): DropConfig | undefined {
        let node: any = this.#index_Key;
        node = node.get(R.key(k0)); if (node === undefined) return undefined;
        node = node.get(R.key(k1)); if (node === undefined) return undefined;
        return node;
    }
    get(k0: number, k1: number): DropConfig { return this.find(k0, k1) ?? R.invalid('配置主键不存在'); }
    getByGroupId(k0: number): readonly DropConfig[] {
        let node: any = this.#index_Group_ByGroupId;
        node = node.get(R.key(k0)); if (node === undefined) return R.empty;
        return node;
    }
    getById(k0: number): readonly DropConfig[] {
        let node: any = this.#index_Group_ById;
        node = node.get(R.key(k0)); if (node === undefined) return R.empty;
        return node;
    }
}
