// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
import * as R from './runtime';
import type * as T from './types';
import type { ConfigSnapshot } from './snapshot';
export interface SkillLevelConfig {
    // 技能编号
    readonly "SkillId": number;
    // 技能名称
    readonly "Name": string;
    // 技能等级
    readonly "Level": number;
    // 品质
    readonly "Quality": number;
    // 伤害
    readonly "Damage": number;
}
const schema = "{\"name\":\"SkillLevel\",\"hash\":\"9DDEDF4A13E49BE883FA11B62A966463D9B27C99620857DE5CD5E721CECC92B2\",\"hasId\":false,\"fields\":[{\"name\":\"SkillId\",\"type\":\"System.Int32\",\"description\":\"技能编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"技能名称\"},{\"name\":\"Level\",\"type\":\"System.Int32\",\"description\":\"技能等级\"},{\"name\":\"Quality\",\"type\":\"System.Int32\",\"description\":\"品质\"},{\"name\":\"Damage\",\"type\":\"System.Int32\",\"description\":\"伤害\"}],\"keys\":[\"SkillId\",\"Level\",\"Quality\"],\"groups\":[{\"name\":\"ByLevel\",\"fields\":[\"Level\"]},{\"name\":\"ByQuality\",\"fields\":[\"Quality\"]},{\"name\":\"BySkillId\",\"fields\":[\"SkillId\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null}],\"tags\":[],\"dependencies\":[]}";
export class SkillLevelTable {
    readonly rows: readonly SkillLevelConfig[];
    readonly #index_Key: Map<any, any>;
    readonly #index_Group_ByLevel: Map<any, any>;
    readonly #index_Group_ByQuality: Map<any, any>;
    readonly #index_Group_BySkillId: Map<any, any>;
    constructor(bundle: R.Bundle, preferJSON: boolean) {
        this.rows = Object.freeze(bundle.load(schema, preferJSON));
        this.#index_Key = R.makeIndex(this.rows, ["SkillId", "Level", "Quality"], true);
        this.#index_Group_ByLevel = R.makeIndex(this.rows, ["Level"], false);
        this.#index_Group_ByQuality = R.makeIndex(this.rows, ["Quality"], false);
        this.#index_Group_BySkillId = R.makeIndex(this.rows, ["SkillId"], false);
    }
    bind(snapshot: ConfigSnapshot): void {
        for (const row of this.rows) Object.freeze(row);
        Object.freeze(this);
    }
    get count(): number { return this.rows.length; }
    find(k0: number, k1: number, k2: number): SkillLevelConfig | undefined {
        let node: any = this.#index_Key;
        node = node.get(R.key(k0)); if (node === undefined) return undefined;
        node = node.get(R.key(k1)); if (node === undefined) return undefined;
        node = node.get(R.key(k2)); if (node === undefined) return undefined;
        return node;
    }
    get(k0: number, k1: number, k2: number): SkillLevelConfig { return this.find(k0, k1, k2) ?? R.invalid('配置主键不存在'); }
    getByLevel(k0: number): readonly SkillLevelConfig[] {
        let node: any = this.#index_Group_ByLevel;
        node = node.get(R.key(k0)); if (node === undefined) return R.empty;
        return node;
    }
    getByQuality(k0: number): readonly SkillLevelConfig[] {
        let node: any = this.#index_Group_ByQuality;
        node = node.get(R.key(k0)); if (node === undefined) return R.empty;
        return node;
    }
    getBySkillId(k0: number): readonly SkillLevelConfig[] {
        let node: any = this.#index_Group_BySkillId;
        node = node.get(R.key(k0)); if (node === undefined) return R.empty;
        return node;
    }
}
