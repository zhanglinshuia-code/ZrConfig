using System.Text;

namespace ZrConfig.Core;

internal sealed partial class NativeGenerator
{
    private void TypeScript(PortableTable[] tables, Dictionary<string, string> files)
    {
        var types = Types(tables);
        bool partitioned=tables.Length>256;
        string TypeName(string id)
        {
            var t = types[id];
            return t.Kind switch
            {
                "primitive" or "tablereference" => t.Primitive switch { "string" => "string", "bool" => "boolean", "int64" or "uint64" => "bigint", "decimal" => "R.Decimal", _ => "number" },
                "rawjson" => "string",
                "nullable" => $"({TypeName(t.Element!)} | null)",
                "array" => t.Rank == 1 ? $"ReadonlyArray<{TypeName(t.Element!)}>" : $"R.Matrix<{TypeName(t.Element!)}>",
                "dictionary" => $"R.Dictionary<{TypeName(t.Key!)}, {TypeName(t.Element!)}>",
                _ => "T." + Name(id) + (t.NullableObject ? " | null" : "")
            };
        }
        string DefinitionType(string id) => TypeName(id).Replace("T.", "");
        string Literal(string primitive, string value) => value + (primitive is "int64" or "uint64" ? "n" : "");
        var definitions = new StringBuilder("// 此文件由工具生成；所有字段只读，自定义类型不包含业务行为。\nimport * as R from './runtime';\n");
        foreach (var t in types.Values.OrderBy(t => t.Id, StringComparer.Ordinal))
        {
            string name = Name(t.Id);
            if (t.Kind == "enum")
            {
                definitions.AppendLine($"export const {name} = Object.freeze({{");
                foreach (var m in t.EnumMembers) definitions.AppendLine($"    {Name(m.Key)}: {Literal(t.Primitive, m.Value)},");
                definitions.AppendLine($"}} as const);\nexport type {name} = typeof {name}[keyof typeof {name}];");
            }
            else if (t.Kind is "object" or "converter" or "kv" or "vector2" or "vector3")
            {
                bool polymorphic = t.Variants.Length > 0;
                if (!t.Abstract)
                {
                    definitions.AppendLine($"export interface {name}{(polymorphic ? "Value" : "")} {{");
                    if (t.Kind == "object") definitions.AppendLine($"    readonly $type?: {Quote(t.Id)};");
                    foreach (var f in t.Fields) { Comment(definitions, f.Description); definitions.AppendLine($"    readonly {Quote(f.Name)}: {DefinitionType(f.Type)};"); }
                    definitions.AppendLine("}");
                }
                if (polymorphic) definitions.AppendLine($"export type {name} = {string.Join(" | ", t.Variants.Select(v => Name(v) + (types[v].Variants.Length > 0 ? "Value" : "")))};");
            }
        }
        files.Add("TypeScript/types.ts", definitions.ToString());
        files.Add("TypeScript/runtime.ts", Runtime("ts"));
        files.Add("TypeScript/tsconfig.json", "{\"compilerOptions\":{\"target\":\"ES2022\",\"module\":\"CommonJS\",\"strict\":true,\"outDir\":\"dist\",\"declaration\":true},\"include\":[\"*.ts\"]}\n");
        var snapshot = new StringBuilder("// 完整加载、引用绑定成功后发布快照；持有旧快照的请求继续使用旧数据。\nimport * as R from './runtime';\n");
        foreach (var table in tables) snapshot.AppendLine($"import {{ {table.Name}Table }} from './{table.Name}Config';");
        snapshot.AppendLine("export class ConfigSnapshot {\n    readonly version: string;");
        foreach (var table in tables) snapshot.AppendLine($"    readonly {table.Name}: {table.Name}Table;");
        snapshot.AppendLine("    private constructor(bundle: R.Bundle, preferJSON: boolean) {\n        this.version = bundle.version;");
        snapshot.AppendLine($"        bundle.validateTables([{string.Join(", ", tables.Select(t => Quote(t.Name)))}]);");
        foreach (var table in tables) snapshot.AppendLine($"        this.{table.Name} = new {table.Name}Table(bundle, preferJSON);");
        foreach (var table in tables) snapshot.AppendLine($"        this.{table.Name}.bind(this);");
        snapshot.AppendLine("        Object.freeze(this);\n    }\n    static load(root: string, preferJSON = false): ConfigSnapshot { return new ConfigSnapshot(new R.Bundle(root), preferJSON); }\n}\nexport class ConfigManager {\n    #current: ConfigSnapshot | undefined;\n    current(): ConfigSnapshot { return this.#current ?? R.invalid('配置尚未加载'); }\n    reload(root: string, preferJSON = false): ConfigSnapshot {\n        const next = ConfigSnapshot.load(root, preferJSON);\n        this.#current = next;\n        return next;\n    }\n}\n");
        files.Add("TypeScript/snapshot.ts", snapshot.ToString());
        var server = new StringBuilder("// Server entry: ephemeron ownership keeps the complete original snapshot alive.\nimport * as R from './runtime';\nimport { ConfigSnapshot } from './snapshot';\nexport { ServerManager, Cancellation, ConfigError } from './runtime';\nexport function load(root: string, preferJSON = false): ConfigSnapshot {\n    const snapshot = ConfigSnapshot.load(root, preferJSON);\n");
        if(partitioned)for(int part=0;part<(tables.Length+127)/128;part++)server.AppendLine($"    $zrRetainPart{part}(snapshot);");
        else foreach (var table in tables) server.AppendLine($"    R.retainOwner(snapshot.{table.Name}, snapshot);\n    for (const row of snapshot.{table.Name}.rows) R.retainOwner(row, snapshot);");
        server.AppendLine("    return snapshot;\n}\nexport function manager(): R.ServerManager<ConfigSnapshot> { return new R.ServerManager<ConfigSnapshot>(); }");
        if(partitioned)for(int part=0;part<(tables.Length+127)/128;part++)
        {
            server.AppendLine($"function $zrRetainPart{part}(snapshot:ConfigSnapshot):void{{");
            foreach(var table in tables.Skip(part*128).Take(128))server.AppendLine($"    R.retainOwner(snapshot.{table.Name},snapshot);for(const row of snapshot.{table.Name}.rows)R.retainOwner(row,snapshot);");
            server.AppendLine("}");
        }
        server.AppendLine($"export const catalog = new R.Catalog({Quote(RuntimeCatalog.Create(tables,_names.Target).Serialize())});");
        foreach (var table in tables) server.AppendLine($"import {{ {table.Name}Table }} from './{table.Name}Config';");
        server.AppendLine("export class SelectedSnapshot {\n    readonly version: string;\n    readonly selection: R.SelectionPlan;\n    #storage: Record<string, any> = Object.create(null);\n    private constructor(bundle: R.Bundle, selection: R.SelectionPlan, preferJSON: boolean) {\n        bundle.validateCatalog(catalog);\n        this.version = bundle.version; this.selection = selection;\n        const selected = new Set(this.selection.tables);\n");
        var selectedHelpers=new StringBuilder();
        if(partitioned)
        {
            foreach(string phase in new[]{"open","bind","retain"})for(int part=0;part<(tables.Length+127)/128;part++)
            {
                server.AppendLine($"        this.#$zr{phase}Part{part}(bundle,selected,preferJSON);");
                selectedHelpers.AppendLine($"    #$zr{phase}Part{part}(bundle:R.Bundle,selected:ReadonlySet<string>,preferJSON:boolean):void{{");
                foreach(var table in tables.Skip(part*128).Take(128))selectedHelpers.AppendLine(phase switch
                {
                    "open"=>$"        if(selected.has({Quote(table.Name)}))this.#storage.{table.Name}=new {table.Name}Table(bundle,preferJSON);",
                    "bind"=>$"        if(selected.has({Quote(table.Name)}))this.#storage.{table.Name}.bind(this.#storage as ConfigSnapshot);",
                    _=>$"        if(selected.has({Quote(table.Name)})){{R.retainOwner(this.#storage.{table.Name},this);for(const row of this.#storage.{table.Name}.rows)R.retainOwner(row,this);}}"
                });
                selectedHelpers.AppendLine("    }");
            }
        }
        else
        {
            foreach (var table in tables) server.AppendLine($"        if (selected.has({Quote(table.Name)})) this.#storage.{table.Name} = new {table.Name}Table(bundle, preferJSON);");
            foreach (var table in tables) server.AppendLine($"        if (selected.has({Quote(table.Name)})) this.#storage.{table.Name}.bind(this.#storage as ConfigSnapshot);");
            foreach (var table in tables) server.AppendLine($"        if (selected.has({Quote(table.Name)})) {{ R.retainOwner(this.#storage.{table.Name}, this); for (const row of this.#storage.{table.Name}.rows) R.retainOwner(row, this); }}");
        }
        server.AppendLine("        Object.freeze(this);\n    }\n    static load(root: string, requested: readonly string[], preferJSON = false): SelectedSnapshot { const selection = catalog.select(requested); return new SelectedSnapshot(new R.Bundle(root), selection, preferJSON); }");
        foreach (var table in tables) server.AppendLine($"    get_{table.Name}(): {table.Name}Table {{ const value = this.#storage.{table.Name}; if (!value) throw new R.ConfigError('TableNotSelected', {Quote(table.Name + ": table not selected")}); return value; }}");
        server.Append(selectedHelpers);
        server.AppendLine("}");
        if (_names.ServerApi) files.Add("TypeScript/server.ts", server.ToString());
        if (_names.ServerApi) TypeScriptData(tables, files, TypeName);
        foreach (var table in tables)
        {
            string row = table.Name + "Config";
            var text = new StringBuilder("// 此文件由工具生成，请勿手工修改。\nimport * as R from './runtime';\nimport type * as T from './types';\nimport type { ConfigSnapshot } from './snapshot';\n");
            foreach (var reference in table.Fields.Where(f => types[f.Type].Kind == "tablereference").Select(f => ReferenceTable(f.Type)).Distinct())
                if (reference != table.Name) text.AppendLine($"import type {{ {reference}Config }} from './{reference}Config';");
            text.AppendLine($"export interface {row} {{");
            foreach (var f in table.Fields)
            {
                Comment(text, f.Description);
                if (types[f.Type].Kind == "tablereference") text.AppendLine($"    readonly {Quote(f.Name + "Id")}: number;\n    readonly {Quote(f.Name)}: {ReferenceTable(f.Type)}Config | null;");
                else text.AppendLine($"    readonly {Quote(f.Name)}: {TypeName(f.Type)};");
            }
            text.AppendLine($"}}\nconst schema = {Quote(table.Serialize())};\nexport class {table.Name}Table {{\n    readonly rows: readonly {row}[];");
            var indexes = new List<(string Name, string[] Fields, string? Group)> { ("Key", table.Keys, null) };
            indexes.AddRange(table.Groups.Select(g => ("Group_" + g.Name, g.Fields, (string?)g.Name)));
            foreach (var ix in indexes) text.AppendLine($"    readonly #index_{ix.Name}: Map<any, any>;");
            bool referenced = tables.Any(t => t.Fields.Any(f => types[f.Type].Kind == "tablereference" && ReferenceTable(f.Type) == table.Name));
            if (referenced) text.AppendLine($"    readonly #ids = new Map<number, {row}>();");
            text.AppendLine("    constructor(bundle: R.Bundle, preferJSON: boolean) {\n        this.rows = Object.freeze(bundle.load(schema, preferJSON));");
            foreach (var ix in indexes) text.AppendLine($"        this.#index_{ix.Name} = R.makeIndex(this.rows, [{string.Join(", ", ix.Fields.Select(Quote))}], {(ix.Group == null ? "true" : "false")});");
            if (referenced) text.AppendLine("        for (const row of this.rows) { if (this.#ids.has(row.Id)) R.invalid('引用 Id 不唯一'); this.#ids.set(row.Id, row); }");
            foreach (var f in table.Fields.Where(f => types[f.Type].Kind == "tablereference")) text.AppendLine($"        for (const row of this.rows as any[]) {{ row[{Quote(f.Name + "Id")}] = row[{Quote(f.Name)}]; row[{Quote(f.Name)}] = null; }}");
            text.AppendLine("    }\n    bind(snapshot: ConfigSnapshot): void {");
            foreach (var f in table.Fields.Where(f => types[f.Type].Kind == "tablereference")) text.AppendLine($"        for (const row of this.rows as any[]) if (row[{Quote(f.Name + "Id")}] !== 0) row[{Quote(f.Name)}] = snapshot.{ReferenceTable(f.Type)}.byId(row[{Quote(f.Name + "Id")}]) ?? R.invalid({Quote("引用目标不存在：" + table.Name + "." + f.Name)});");
            text.AppendLine("        for (const row of this.rows) Object.freeze(row);\n        Object.freeze(this);\n    }\n    get count(): number { return this.rows.length; }");
            if (referenced) text.AppendLine($"    byId(id: number): {row} | undefined {{ return this.#ids.get(id); }}");
            foreach (var ix in indexes)
            {
                string args = string.Join(", ", ix.Fields.Select((f, i) => $"k{i}: {TypeName(table.Fields.Single(x => x.Name == f).Type)}"));
                string call = string.Join(", ", ix.Fields.Select((_, i) => "k" + i));
                text.AppendLine($"    {(ix.Group == null ? "find" : "get" + ix.Group)}({args}): {(ix.Group == null ? row + " | undefined" : "readonly " + row + "[]")} {{\n        let node: any = this.#index_{ix.Name};");
                for (int i = 0; i < ix.Fields.Length; i++) text.AppendLine($"        node = node.get(R.key(k{i})); if (node === undefined) return {(ix.Group == null ? "undefined" : "R.empty")};");
                text.AppendLine("        return node;\n    }");
                if (ix.Group == null) text.AppendLine($"    get({args}): {row} {{ return this.find({call}) ?? R.invalid('配置主键不存在'); }}");
            }
            text.AppendLine("}");
            files.Add(_names.TablePath(table.Name, "typescript"), text.ToString());
        }
    }

    private void TypeScriptData(PortableTable[] tables, Dictionary<string, string> files, Func<string,string> typeName)
    {
        var types = Types(tables);
        var initializers=new StringBuilder();
        var text = new StringBuilder("// Generated server views: queries are pure, loads are explicit.\nimport * as R from './runtime';\nimport type * as T from './types';\nimport { catalog } from './server';\nexport { LoadPlan, LoadMode } from './runtime';\nconst schemas = new Map<string,string>([\n");
        foreach(var table in tables) text.AppendLine($"    [{Quote(table.Name)},{Quote(table.Serialize())}],");
        text.AppendLine(" ]);\nconst viewToken={};\nexport class Snapshot {\n    readonly version: string;\n    readonly manifestDigest: string;\n    readonly sourceNamespace: string;\n    readonly catalogFingerprint:string;\n    readonly loadPlanFingerprint:string;\n    readonly selection: R.SelectionPlan;\n    readonly mode: R.LoadMode;\n    readonly #budget: R.LoadBudget;");
        foreach (var table in tables) text.AppendLine($"    #{table.Name}: {table.Name}Table | undefined;");
        text.AppendLine("""
            private constructor(bundle: R.Bundle, plan: R.LoadPlan, selection: R.SelectionPlan, indexed: Map<string,R.IndexedRows>) {
                this.version=bundle.version; this.manifestDigest=bundle.manifestDigest; this.#budget=bundle.budget!;
                this.selection=selection; this.mode=plan.mode!; this.catalogFingerprint=selection.catalogFingerprint;this.loadPlanFingerprint=R.loadPlanFingerprint(this.mode,plan.preferJSON===true,selection.tables); this.sourceNamespace=plan.sourceNamespace ?? '';
                for(const data of indexed.values()) data.bindReferences(name=>indexed.get(name)!);
                const selected=new Set(selection.tables);
            """);
        if(tables.Length>256)for(int part=0;part<(tables.Length+127)/128;part++)
        {
            text.AppendLine($"        this.#$zrInitializePart{part}(indexed,selected);");
            initializers.AppendLine($"    #$zrInitializePart{part}(indexed:Map<string,R.IndexedRows>,selected:ReadonlySet<string>):void{{");
            foreach(var table in tables.Skip(part*128).Take(128))initializers.AppendLine($"        if(selected.has({Quote(table.Name)}))this.#{table.Name}=create_{table.Name}Table(indexed.get({Quote(table.Name)})!,this);");
            initializers.AppendLine("    }");
        }
        else foreach (var table in tables) text.AppendLine($"        if (selected.has({Quote(table.Name)})) this.#{table.Name} = create_{table.Name}Table(indexed.get({Quote(table.Name)})!, this);");
        text.AppendLine("""
                Object.freeze(this);
            }
            static load(source: string | R.ConfigSource, options: R.LoadPlan = {}, cancellation = new R.Cancellation()): Snapshot {
                const resolved=typeof source==='string'?new R.FileConfigSource(source):source;
                const effective=options.sourceNamespace===undefined&&resolved instanceof R.FileConfigSource?{...options,sourceNamespace:resolved.sourceNamespace}:options;
                const {plan,selection}=R.prepareLoadPlan(effective,catalog); cancellation.check();
                const bundle=new R.Bundle(resolved,plan.limits,cancellation);
                bundle.validateCatalog(catalog); const indexed=new Map<string,R.IndexedRows>();
                for(const name of selection.tables) { cancellation.check(); indexed.set(name,bundle.openRows(schemas.get(name)!,plan.preferJSON===true,cancellation)); }
                const next=new Snapshot(bundle,plan,selection,indexed);
                if(plan.mode!=='LazyRowsOnAccess') for(const data of indexed.values()) for(let i=0;i<data.count;i++) { cancellation.check(); data.readTransient(i); }
                if(plan.mode==='ServerDefault'||plan.mode==='SelectedTables') next.loadAll(cancellation);
                for(const name of plan.warmupTables ?? []) next.#warmup(name,cancellation);
                cancellation.check(); return next;
            }
            static async loadAsync(source: string | R.AsyncConfigSource, options: R.LoadPlan, cancellation: R.Cancellation, scheduler: R.LoadScheduler): Promise<Snapshot> {
                const resolved=typeof source==='string'?new R.FileConfigSource(source):source;
                const effective=options.sourceNamespace===undefined&&resolved instanceof R.FileConfigSource?{...options,sourceNamespace:resolved.sourceNamespace}:options;
                const {plan,selection}=R.prepareLoadPlan(effective,catalog); cancellation.check();
                if(!scheduler || typeof scheduler.yield!=='function' || typeof scheduler.execute!=='function') throw new R.ConfigError('InvalidPlan','An explicit load scheduler is required');
                const bundle=await R.Bundle.open(resolved,plan.limits,cancellation);
                bundle.validateCatalog(catalog); const indexed=new Map<string,R.IndexedRows>();
                for(const name of selection.tables) { cancellation.check(); indexed.set(name,await bundle.openRowsAsync(schemas.get(name)!,plan.preferJSON===true,cancellation)); }
                const next=new Snapshot(bundle,plan,selection,indexed);
                if(plan.mode!=='LazyRowsOnAccess') for(const data of indexed.values()) for(let i=0;i<data.count;i++) { if(i%64===0) await scheduler.yield(cancellation); cancellation.check(); data.readTransient(i); }
                if(plan.mode==='ServerDefault'||plan.mode==='SelectedTables') await next.loadAllAsync(cancellation,scheduler);
                for(const name of plan.warmupTables ?? []) await next.#warmupAsync(name,cancellation,scheduler);
                cancellation.check(); return next;
            }
            diagnostics() { return Object.freeze({readBytes:this.#budget.readBytes,retainedRawBytes:this.#budget.retainedRawBytes,estimatedDecodedBytes:this.#budget.estimatedDecodedBytes,estimatedIndexBytes:this.#budget.estimatedIndexBytes,declaredRows:this.#budget.declaredRows,decodedRows:this.#budget.decodedRows,pendingJobs:this.#budget.pendingJobs,pendingWaiters:this.#budget.pendingWaiters,mergedRequests:this.#budget.mergedRequests,faultedRows:this.#budget.faultedRows,faultedIndexes:this.#budget.faultedIndexes,health:this.#budget.health}); }
            get health(): 'Healthy' | 'Degraded' { return this.#budget.health; }
            get retainedRawBytes(): number { return this.#budget.retainedRawBytes; }
            get declaredRowCount(): number { return this.#budget.declaredRows; }
            get estimatedIndexBytes(): number { return this.#budget.estimatedIndexBytes; }
            get estimatedDecodedBytes(): number { return this.#budget.estimatedDecodedBytes; }
            get mergedRowRequests(): number { return this.#budget.mergedRequests; }
            #warmup(name: string, cancellation: R.Cancellation): void {
                switch(name) {
            """);
        foreach(var table in tables) text.AppendLine($"            case {Quote(table.Name)}: this.get_{table.Name}().loadAll(cancellation); return;");
        text.AppendLine("            default: throw new R.ConfigError('InvalidPlan','Unknown warmup table');\n        }\n    }\n    async #warmupAsync(name: string, cancellation: R.Cancellation, scheduler: R.LoadScheduler): Promise<void> {\n        switch(name) {");
        foreach(var table in tables) text.AppendLine($"            case {Quote(table.Name)}: await this.get_{table.Name}().loadAllAsync(cancellation,scheduler); return;");
        text.AppendLine("            default: throw new R.ConfigError('InvalidPlan','Unknown warmup table');\n        }\n    }\n    loadAll(cancellation?: R.Cancellation): void {");
        if(tables.Length>256)for(int part=0;part<(tables.Length+127)/128;part++)
        {
            text.AppendLine($"        this.#$zrLoadPart{part}(cancellation);");
            initializers.AppendLine($"    #$zrLoadPart{part}(cancellation?:R.Cancellation):void{{");
            foreach(var table in tables.Skip(part*128).Take(128))initializers.AppendLine($"        this.#{table.Name}?.loadAll(cancellation);");
            initializers.AppendLine("    }");
        }
        else foreach(var table in tables) text.AppendLine($"        this.#{table.Name}?.loadAll(cancellation);");
        text.AppendLine("    }\n    async loadAllAsync(cancellation: R.Cancellation, scheduler: R.LoadScheduler): Promise<void> {");
        if(tables.Length>256)for(int part=0;part<(tables.Length+127)/128;part++)
        {
            text.AppendLine($"        await this.#$zrLoadAsyncPart{part}(cancellation,scheduler);");
            initializers.AppendLine($"    async #$zrLoadAsyncPart{part}(cancellation:R.Cancellation,scheduler:R.LoadScheduler):Promise<void>{{");
            foreach(var table in tables.Skip(part*128).Take(128))initializers.AppendLine($"        await this.#{table.Name}?.loadAllAsync(cancellation,scheduler);");
            initializers.AppendLine("    }");
        }
        else foreach(var table in tables) text.AppendLine($"        await this.#{table.Name}?.loadAllAsync(cancellation,scheduler);");
        text.AppendLine("    }\n    get loadedRowCount(): number { return " + SumTerms(tables.Select(t => $"(this.#{t.Name}?.loadedCount ?? 0)")," + ") + "; }");
        text.AppendLine("    get completedTableCount():number{return "+SumTerms(tables.Select(t=>$"(this.#{t.Name}?.isComplete?1:0)"),"+")+";}");
        foreach (var table in tables) text.AppendLine($"    get_{table.Name}(): {table.Name}Table {{ if (!this.#{table.Name}) throw new R.ConfigError('TableNotSelected',{Quote(table.Name)}); return this.#{table.Name}; }}");
        text.Append(initializers);
        text.AppendLine("    static loadWithWarmup(source: string | R.ConfigSource,options: R.LoadPlan,requests: readonly Warmup[],cancellation=new R.Cancellation()): Snapshot {\n        const {selection}=R.prepareLoadPlan(options,catalog);const steps=Warmup.validate(requests,selection);const next=Snapshot.load(source,options,cancellation);\n        for(const step of steps){cancellation.check();step.apply(next,cancellation);}cancellation.check();return next;\n    }\n    static async loadWithWarmupAsync(source: string | R.AsyncConfigSource,options: R.LoadPlan,requests: readonly Warmup[],cancellation: R.Cancellation,scheduler: R.LoadScheduler): Promise<Snapshot> {\n        const {selection}=R.prepareLoadPlan(options,catalog);const steps=Warmup.validate(requests,selection);const next=await Snapshot.loadAsync(source,options,cancellation,scheduler);\n        for(const step of steps){cancellation.check();await step.applyAsync(next,cancellation,scheduler);}cancellation.check();return next;\n    }");
        foreach(var table in tables)
        {
            text.AppendLine($"    static warmupTable_{table.Name}(): Warmup {{return createWarmup({Quote(table.Name)},(snapshot,token)=>{{snapshot.get_{table.Name}().loadAll(token);}},async(snapshot,token,scheduler)=>{{await snapshot.get_{table.Name}().loadAllAsync(token,scheduler);}});}}");
            foreach(var item in new[]{(Name:"Row_"+table.Name,Fields:table.Keys,Method:"load",Async:"loadAsync")}.Concat(table.Groups.Select(g=>(Name:"Group_"+table.Name+"_"+g.Name,Fields:g.Fields,Method:"loadGroup_"+g.Name,Async:"loadGroup_"+g.Name+"Async"))))
            {
                var fields=item.Fields.Select(n=>table.Fields.Single(f=>f.Name==n)).ToArray();string args=string.Join(",",fields.Select((f,i)=>"k"+i+": "+typeName(f.Type))),calls=string.Join(",",fields.Select((_,i)=>"k"+i));
                text.AppendLine($"    static warmup{item.Name}({args}): Warmup {{return createWarmup({Quote(table.Name)},(snapshot,token)=>{{snapshot.get_{table.Name}().{item.Method}({calls},token);}},async(snapshot,token,scheduler)=>{{await snapshot.get_{table.Name}().{item.Async}({calls},token,scheduler);}});}}");
            }
        }
        text.AppendLine("}\nexport function manager(policy: Omit<R.ManagerPolicy<Snapshot>,'identity'> = {}): R.ServerManager<Snapshot> { return new R.ServerManager<Snapshot>({...policy,identity:snapshot=>({sourceNamespace:snapshot.sourceNamespace,versionId:snapshot.version,manifestDigest:snapshot.manifestDigest})}); }");
        text.AppendLine("type WarmupAction=(snapshot: Snapshot,token: R.Cancellation)=>void;\ntype WarmupAsyncAction=(snapshot: Snapshot,token: R.Cancellation,scheduler: R.LoadScheduler)=>Promise<void>;\nconst warmupToken={};const warmupInstances=new WeakSet<Warmup>();\nlet createWarmup:(table: string,action: WarmupAction,asyncAction: WarmupAsyncAction)=>Warmup;\nexport class Warmup {\n    readonly table:string;readonly #action:WarmupAction;readonly #async:WarmupAsyncAction;\n    private constructor(token: object,table:string,action:WarmupAction,asyncAction:WarmupAsyncAction){if(token!==warmupToken)throw new R.ConfigError('InvalidPlan','Use a generated warmup factory');this.table=table;this.#action=action;this.#async=asyncAction;warmupInstances.add(this);Object.freeze(this);}\n    static {createWarmup=(table,action,asyncAction)=>new Warmup(warmupToken,table,action,asyncAction);}\n    static validate(requests:readonly Warmup[],selection:R.SelectionPlan):readonly Warmup[]{if(!Array.isArray(requests)||requests.some(request=>!warmupInstances.has(request)||!selection.tables.includes(request.table)))throw new R.ConfigError('InvalidPlan','Warmup outside selection');return Object.freeze([...requests]);}\n    apply(snapshot:Snapshot,token:R.Cancellation):void{this.#action(snapshot,token);}\n    applyAsync(snapshot:Snapshot,token:R.Cancellation,scheduler:R.LoadScheduler):Promise<void>{return this.#async(snapshot,token,scheduler); }\n}");
        foreach (var table in tables)
        {
            string row = table.Name + "Row";
            text.AppendLine($"let create_{row}: (value: unknown, snapshot: Snapshot) => {row};\nexport class {row} {{\n    #value: any;\n    #snapshot: Snapshot;\n    private constructor(value: unknown, snapshot: Snapshot,token: object) {{ if(token!==viewToken)throw new R.ConfigError('InvalidPlan','Use snapshot row accessors');this.#value = value; this.#snapshot = snapshot; Object.freeze(this); }}\n    static {{ create_{row} = (value,snapshot) => new {row}(value,snapshot,viewToken); }}");
            foreach (var field in table.Fields)
            {
                if (types[field.Type].Kind == "tablereference")
                {
                    string target = types[field.Type].ReferenceTable!;
                    text.AppendLine($"    get {field.Name}Id(): number {{ return this.#value[{Quote(field.Name + "Id")}]; }}\n    get_{field.Name}(): {target}Row | null {{ return this.#snapshot.get_{target}().reference(this.{field.Name}Id,false); }}\n    load_{field.Name}(): {target}Row | null {{ return this.#snapshot.get_{target}().reference(this.{field.Name}Id,true); }}");
                }
                else text.AppendLine($"    get {field.Name}(): {typeName(field.Type)} {{ return this.#value[{Quote(field.Name)}]; }}");
            }
            text.AppendLine("}");
            text.AppendLine($"let create_{table.Name}Table: (indexed: R.IndexedRows, snapshot: Snapshot) => {table.Name}Table;\nexport class {table.Name}Table extends R.RowStorage<{row}> {{\n    private constructor(indexed: R.IndexedRows, snapshot: Snapshot,token: object) {{ if(token!==viewToken)throw new R.ConfigError('InvalidPlan','Use snapshot table accessors');super(indexed, value => create_{row}(value,snapshot)); }}\n    static {{ create_{table.Name}Table = (indexed,snapshot) => new {table.Name}Table(indexed,snapshot,viewToken); }}");
            string Args(string[] fields) => string.Join(", ", fields.Select((name,i) => $"k{i}: {typeName(table.Fields.Single(f => f.Name == name).Type)}"));
            string Call(string[] fields) => string.Join(", ", fields.Select((_,i) => "k" + i));
            text.AppendLine($"    #ordinal({Args(table.Keys)}): number | undefined {{\n        let node: any = this.keyIndex;");
            for (int i = 0; i < table.Keys.Length; i++) text.AppendLine($"        node = node.get(R.key(k{i})); if (node === undefined) return undefined;");
            text.AppendLine("        return node;\n    }");
            text.AppendLine($"    find({Args(table.Keys)}): {row} | undefined {{ return this.queryOrdinal(this.#ordinal({Call(table.Keys)})); }}\n    get({Args(table.Keys)}): {row} {{ const row = this.find({Call(table.Keys)}); if (!row) throw new R.ConfigError('KeyNotFound','Row key not found'); return row; }}\n    tryLoad({Args(table.Keys)}, cancellation?: R.Cancellation): {row} | undefined {{ return this.loadOrdinal(this.#ordinal({Call(table.Keys)}),cancellation); }}\n    load({Args(table.Keys)}, cancellation?: R.Cancellation): {row} {{ const row = this.tryLoad({Call(table.Keys)},cancellation); if (!row) throw new R.ConfigError('KeyNotFound','Row key not found'); return row; }}");
            text.AppendLine($"    loadAsync({Args(table.Keys)}, cancellation: R.Cancellation, scheduler: R.LoadScheduler): Promise<{row}> {{ return this.loadOrdinalAsync(this.#ordinal({Call(table.Keys)}),cancellation,scheduler).then(value=>{{if(!value) throw new R.ConfigError('KeyNotFound','Row key not found'); return value;}}); }}");
            foreach (var group in table.Groups)
            {
                text.AppendLine($"    #{group.Name}({Args(group.Fields)}, load: boolean, cancellation?: R.Cancellation): readonly {row}[] {{\n        let node: any = this.groupIndex({Quote(group.Name)},load,cancellation);");
                for (int i = 0; i < group.Fields.Length; i++) text.AppendLine($"        node = node.get(R.key(k{i})); if (node === undefined) return R.empty;");
                text.AppendLine("        return this.group(node,load,cancellation);\n    }");
                text.AppendLine($"    findGroup_{group.Name}({Args(group.Fields)}): readonly {row}[] {{ return this.#{group.Name}({Call(group.Fields)},false); }}\n    loadGroup_{group.Name}({Args(group.Fields)}, cancellation?: R.Cancellation): readonly {row}[] {{ return this.#{group.Name}({Call(group.Fields)},true,cancellation); }}");
                text.AppendLine($"    async loadGroup_{group.Name}Async({Args(group.Fields)},cancellation: R.Cancellation,scheduler: R.LoadScheduler): Promise<readonly {row}[]> {{\n        let node: any=await this.groupIndexAsync({Quote(group.Name)},cancellation,scheduler);");
                for (int i=0;i<group.Fields.Length;i++) text.AppendLine($"        node=node.get(R.key(k{i}));if(node===undefined)return R.empty;");
                text.AppendLine("        return this.groupAsync(node,cancellation,scheduler);\n    }");

            }
            text.AppendLine("}");
        }
        files.Add("TypeScript/data.ts",text.ToString());
    }
}
