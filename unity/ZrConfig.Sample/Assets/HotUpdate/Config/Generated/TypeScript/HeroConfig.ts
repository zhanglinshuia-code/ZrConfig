// zrconfig-api-contract:1
// 此文件由工具生成，请勿手工修改。
import * as R from './runtime';
import type * as T from './types';
import type { ConfigSnapshot } from './snapshot';
import type { SkillConfig } from './SkillConfig';
export interface HeroConfig {
    // 英雄编号
    readonly "Id": number;
    // 英雄名称
    readonly "Name": string;
    // 技能 Id；引用 Skill 表
    readonly "SkillId": number;
    readonly "Skill": SkillConfig | null;
    // 另一个技能 Id；可复用同一对象
    readonly "OtherSkillId": number;
    readonly "OtherSkill": SkillConfig | null;
    // 导师 Id；引用 Hero 表
    readonly "MentorId": number;
    readonly "Mentor": HeroConfig | null;
}
const schema = "{\"name\":\"Hero\",\"hash\":\"7E1E75AC24BAFD42C18F1B43A19B0027A3D241CC665DB3727BC98A4908927ECE\",\"hasId\":true,\"fields\":[{\"name\":\"Id\",\"type\":\"System.Int32\",\"description\":\"英雄编号\"},{\"name\":\"Name\",\"type\":\"System.String\",\"description\":\"英雄名称\"},{\"name\":\"Skill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"技能 Id；引用 Skill 表\"},{\"name\":\"OtherSkill\",\"type\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"description\":\"另一个技能 Id；可复用同一对象\"},{\"name\":\"Mentor\",\"type\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"description\":\"导师 Id；引用 Hero 表\"}],\"keys\":[\"Id\"],\"groups\":[{\"name\":\"ById\",\"fields\":[\"Id\"]}],\"types\":[{\"id\":\"System.Int32\",\"kind\":\"primitive\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"System.String\",\"kind\":\"primitive\",\"primitive\":\"string\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":null},{\"id\":\"ref:ZrConfig.Models.HeroConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Hero\"},{\"id\":\"ref:ZrConfig.Models.SkillConfig:Id:int:zero-null/1\",\"kind\":\"tablereference\",\"primitive\":\"int32\",\"nullableObject\":false,\"rank\":0,\"element\":null,\"key\":null,\"fields\":[],\"variants\":[],\"enumValues\":[],\"abstract\":false,\"baseType\":null,\"enumMembers\":{},\"referenceTable\":\"Skill\"}],\"tags\":[],\"dependencies\":[\"Hero\",\"Skill\"]}";
export class HeroTable {
    readonly rows: readonly HeroConfig[];
    readonly #index_Key: Map<any, any>;
    readonly #index_Group_ById: Map<any, any>;
    readonly #ids = new Map<number, HeroConfig>();
    constructor(bundle: R.Bundle, preferJSON: boolean) {
        this.rows = Object.freeze(bundle.load(schema, preferJSON));
        this.#index_Key = R.makeIndex(this.rows, ["Id"], true);
        this.#index_Group_ById = R.makeIndex(this.rows, ["Id"], false);
        for (const row of this.rows) { if (this.#ids.has(row.Id)) R.invalid('引用 Id 不唯一'); this.#ids.set(row.Id, row); }
        for (const row of this.rows as any[]) { row["SkillId"] = row["Skill"]; row["Skill"] = null; }
        for (const row of this.rows as any[]) { row["OtherSkillId"] = row["OtherSkill"]; row["OtherSkill"] = null; }
        for (const row of this.rows as any[]) { row["MentorId"] = row["Mentor"]; row["Mentor"] = null; }
    }
    bind(snapshot: ConfigSnapshot): void {
        for (const row of this.rows as any[]) if (row["SkillId"] !== 0) row["Skill"] = snapshot.Skill.byId(row["SkillId"]) ?? R.invalid("引用目标不存在：Hero.Skill");
        for (const row of this.rows as any[]) if (row["OtherSkillId"] !== 0) row["OtherSkill"] = snapshot.Skill.byId(row["OtherSkillId"]) ?? R.invalid("引用目标不存在：Hero.OtherSkill");
        for (const row of this.rows as any[]) if (row["MentorId"] !== 0) row["Mentor"] = snapshot.Hero.byId(row["MentorId"]) ?? R.invalid("引用目标不存在：Hero.Mentor");
        for (const row of this.rows) Object.freeze(row);
        Object.freeze(this);
    }
    get count(): number { return this.rows.length; }
    byId(id: number): HeroConfig | undefined { return this.#ids.get(id); }
    find(k0: number): HeroConfig | undefined {
        let node: any = this.#index_Key;
        node = node.get(R.key(k0)); if (node === undefined) return undefined;
        return node;
    }
    get(k0: number): HeroConfig { return this.find(k0) ?? R.invalid('配置主键不存在'); }
    getById(k0: number): readonly HeroConfig[] {
        let node: any = this.#index_Group_ById;
        node = node.get(R.key(k0)); if (node === undefined) return R.empty;
        return node;
    }
}
