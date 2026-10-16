// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
import * as R from './runtime';
import type * as T from './types';
import type { ConfigSnapshot } from './snapshot';
import type { HeroConfig } from './HeroConfig';
export interface SkillConfig {
    // 技能编号
    readonly "Id": number;
    // 技能名称
    readonly "Name": string;
    // 英雄 Id；反向引用 Hero 表
    readonly "HeroId": number;
    readonly "Hero": HeroConfig | null;
}
const schema = "{\"name\":\"Skill\",\"hash\":\"51DA7601418A98574B1C5ADC656CB7E7B479CC1F3E5237DFC5BA7532F702B9DB\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"技能编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"技能名称\"},{\"name\":\"Hero\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"英雄 Id；反向引用 Hero 表\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"}],\"tags\":[],\"dependencies\":[\"Hero\"]}";
export class SkillTable {
    readonly rows: readonly SkillConfig[];
    readonly #index_Key: Map<any, any>;
    readonly #index_Group_ById: Map<any, any>;
    readonly #ids = new Map<number, SkillConfig>();
    constructor(bundle: R.Bundle, preferJSON: boolean) {
        this.rows = Object.freeze(bundle.load(schema, preferJSON));
        this.#index_Key = R.makeIndex(this.rows, ["Id"], true);
        this.#index_Group_ById = R.makeIndex(this.rows, ["Id"], false);
        for (const row of this.rows) { if (this.#ids.has(row.Id)) R.invalid('引用 Id 不唯一'); this.#ids.set(row.Id, row); }
        for (const row of this.rows as any[]) { row["HeroId"] = row["Hero"]; row["Hero"] = null; }
    }
    bind(snapshot: ConfigSnapshot): void {
        for (const row of this.rows as any[]) if (row["HeroId"] !== 0) row["Hero"] = snapshot.Hero.byId(row["HeroId"]) ?? R.invalid("引用目标不存在：Skill.Hero");
        for (const row of this.rows) Object.freeze(row);
        Object.freeze(this);
    }
    get count(): number { return this.rows.length; }
    byId(id: number): SkillConfig | undefined { return this.#ids.get(id); }
    find(k0: number): SkillConfig | undefined {
        let node: any = this.#index_Key;
        node = node.get(R.key(k0)); if (node === undefined) return undefined;
        return node;
    }
    get(k0: number): SkillConfig { return this.find(k0) ?? R.invalid('配置主键不存在'); }
    getById(k0: number): readonly SkillConfig[] {
        let node: any = this.#index_Group_ById;
        node = node.get(R.key(k0)); if (node === undefined) return R.empty;
        return node;
    }
}
