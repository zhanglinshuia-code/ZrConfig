using System.Text;

namespace ZrConfig.Core;

internal sealed partial class NativeGenerator
{
    internal const int GoPartSize = 128;
    internal const int GoPartitionThreshold = 256;
    private Dictionary<string,string> _goTablePaths = new(StringComparer.Ordinal);
    private string GoMember(string table) => _goTablePaths[table];
    private static void GoStorageParts(StringBuilder code,PortableTable[] tables,string type,string tableSuffix)
    {
        int index=0;
        foreach(var part in tables.Chunk(GoPartSize))
        {
            code.AppendLine($"type {type}{index++} struct{{");
            foreach(var table in part)code.AppendLine($"table_{table.Name} *{table.Name}{tableSuffix}");
            code.AppendLine("}");
        }
    }

    // Keep generated cold phases bounded: Go's debug-location SSA can otherwise
    // retain quadratic state for a function containing thousands of table blocks.
    private static void GoPhase(StringBuilder code,StringBuilder helpers,PortableTable[] tables,string signature,string invocation,Func<PortableTable,string> statement,string end="",string begin="")
    {
        if(tables.Length<=GoPartitionThreshold){foreach(var table in tables)code.AppendLine(statement(table));return;}
        int part=0;
        foreach(var chunk in tables.Chunk(GoPartSize))
        {
            string number=(part++).ToString(System.Globalization.CultureInfo.InvariantCulture);
            code.AppendLine(invocation.Replace("$part",number,StringComparison.Ordinal));
            helpers.AppendLine("//go:noinline\nfunc "+signature.Replace("$part",number,StringComparison.Ordinal)+"{");
            if(begin.Length!=0)helpers.AppendLine(begin);
            foreach(var table in chunk)helpers.AppendLine(statement(table));
            helpers.AppendLine(end+"\n}");
        }
    }

    private void Go(PortableTable[] tables, Dictionary<string, string> files)
    {
        bool partitioned=tables.Length>GoPartitionThreshold;
        _goTablePaths=tables.Select((table,index)=>(table.Name,Path:partitioned?$"part_{index/GoPartSize}.table_{table.Name}":$"table_{table.Name}")).ToDictionary(item=>item.Name,item=>item.Path,StringComparer.Ordinal);
        var types = Types(tables);
        string GType(string id)
        {
            var t = types[id];
            return t.Kind switch
            {
                "nullable" => $"Optional[{GType(t.Element!)}]",
                "array" => $"{(t.Rank == 1 ? "List" : "Matrix")}[{GType(t.Element!)}]",
                "dictionary" => $"Dictionary[{GType(t.Key!)},{GType(t.Element!)}]",
                "rawjson" => "string",
                "primitive" or "tablereference" => t.Primitive == "decimal" ? "Decimal" : t.Primitive,
                _ => (t.NullableObject && t.Variants.Length == 0 ? "*" : "") + Name(id)
            };
        }
        string Scalar(string primitive, string value) => primitive switch
        {
            "int8" or "int16" or "int32" or "int64" => $"{primitive}(signed({value},{primitive[3..]}))",
            "uint8" or "uint16" or "uint32" or "uint64" => $"{primitive}(unsigned({value},{primitive[4..]}))",
            "float32" or "float64" => $"{primitive}(floating({value},{primitive[5..]}))",
            "decimal" => $"decimalValue(text({value}))",
            "bool" => $"boolean({value})",
            "string" => $"text({value})",
            _ => throw new NotSupportedException(primitive)
        };
        string Read(string id, string value, int depth = 0)
        {
            var t = types[id];
            string item = "item" + depth;
            string Function(string child) => $"func({item} any) {GType(child)} {{ return {Read(child, item, depth + 1)} }}";
            return t.Kind switch
            {
                "primitive" or "tablereference" => Scalar(t.Primitive, value),
                "enum" => $"{Name(id)}({Scalar(t.Primitive, value)})",
                "rawjson" => $"text({value})",
                "nullable" => $"decodeOptional({value},{Function(t.Element!)})",
                "array" => $"decode{(t.Rank == 1 ? "List" : "Matrix")}({value},{Function(t.Element!)})",
                "dictionary" => $"decodeDictionary({value},{Function(t.Key!)},{Function(t.Element!)})",
                _ => $"read_{Name(id)}({value})"
            };
        }
        files.Add("Go/runtime.go", Runtime("go"));
        var definitions = new StringBuilder("// 此文件由工具生成，自定义类型共用统一结构。\npackage zrconfig\n");
        foreach (var t in types.Values.OrderBy(t => t.Id, StringComparer.Ordinal))
        {
            string name = Name(t.Id);
            if (t.Kind == "enum")
            {
                definitions.AppendLine($"type {name} {t.Primitive}");
                foreach (var member in t.EnumMembers)
                    definitions.AppendLine($"const {name}_{Name(member.Key)} {name} = {member.Value}");
                continue;
            }
            if (t.Kind is not ("object" or "converter" or "kv" or "vector2" or "vector3"))
                continue;
            bool polymorphic = t.Variants.Length != 0;
            if (polymorphic)
            {
                definitions.AppendLine($"type {name} interface {{\n    is_{name}()");
                foreach (var f in t.Fields)
                {
                    Comment(definitions, f.Description);
                    definitions.AppendLine($"    {Accessor(f.Name, "go")}() {GType(f.Type)}");
                }
                definitions.AppendLine("}");
            }
            string body = name + (polymorphic ? "Value" : "");
            if (!t.Abstract)
            {
                definitions.AppendLine($"type {body} struct {{");
                foreach (var f in t.Fields)
                    definitions.AppendLine($"    v_{f.Name} {GType(f.Type)}");
                definitions.AppendLine("}");
                foreach (var f in t.Fields)
                {
                    Comment(definitions, f.Description, "");
                    definitions.AppendLine($"func (v {(t.NullableObject || polymorphic ? "*" : "") + body}) {Accessor(f.Name, "go")}() {GType(f.Type)} {{ return v.v_{f.Name} }}");
                }
                foreach (var ancestor in types.Values.Where(a => a.Variants.Contains(t.Id)))
                    definitions.AppendLine($"func (v *{body}) is_{Name(ancestor.Id)}() {{}}");
                definitions.AppendLine($"func body_{name}(value any) {(t.NullableObject || polymorphic ? "*" : "") + body} {{\n    return {(t.NullableObject || polymorphic ? "&" : "") + body}{{");
                foreach (var f in t.Fields)
                    definitions.AppendLine($"        v_{f.Name}: {Read(f.Type, "field(value," + Quote(f.Name) + ")")},");
                definitions.AppendLine("    }\n}");
            }
            definitions.AppendLine($"func read_{name}(value any) {GType(t.Id)} {{");
            if (t.NullableObject)
                definitions.AppendLine("    if value == nil { return nil }");
            if (polymorphic)
            {
                definitions.AppendLine("    switch text(field(value,\"$type\")) {");
                foreach (string variant in t.Variants)
                    definitions.AppendLine($"    case {Quote(variant)}: return body_{Name(variant)}(value)");
                definitions.AppendLine("    default: invalid(\"未知多态类型\")\n    }\n    return nil");
            }
            else if (t.Abstract)
                definitions.AppendLine("    invalid(\"抽象类型不能实例化\")\n    return nil");
            else
                definitions.AppendLine($"    return body_{name}(value)");
            definitions.AppendLine("}");
        }
        files.Add("Go/types.go", definitions.ToString());
        var catalog = new StringBuilder("// 完整候选快照构建成功后才提交，已有请求继续持有旧快照。\npackage zrconfig\nimport (\"sync\";\"sync/atomic\")\ntype ConfigSnapshot struct {\n    version string\n");
        if(partitioned)for(int i=0;i<(tables.Length+GoPartSize-1)/GoPartSize;i++)catalog.AppendLine($"part_{i} zrConfigSnapshotPart{i}");
        else foreach (var table in tables)
            catalog.AppendLine($"    table_{table.Name} *{table.Name}Table");
        catalog.AppendLine("}\nfunc(s *ConfigSnapshot) Version() string { return s.version }");
        foreach (var table in tables)
            catalog.AppendLine($"func(s *ConfigSnapshot) {Accessor(table.Name, "go")}() *{table.Name}Table {{ return s.{GoMember(table.Name)} }}");
        catalog.AppendLine("func LoadConfig(root string, preferJSON bool)(snapshot *ConfigSnapshot,err error) {\n    defer recoverError(&err)\n    b:=openBundle(root)\n    next:=&ConfigSnapshot{version:b.version}");
        catalog.AppendLine($"    b.validateTables([]string{{{string.Join(", ", tables.Select(t => Quote(t.Name)))}}})");
        var catalogHelpers=new StringBuilder();
        GoPhase(catalog,catalogHelpers,tables,"zrConfigLoadPart$part(next *ConfigSnapshot,b *bundle,preferJSON bool)","zrConfigLoadPart$part(next,b,preferJSON)",table=>$"    next.{GoMember(table.Name)}=load{table.Name}(b.load(schema{table.Name},preferJSON))");
        GoPhase(catalog,catalogHelpers,tables,"zrConfigBindPart$part(next *ConfigSnapshot)","zrConfigBindPart$part(next)",table=>$"    next.{GoMember(table.Name)}.bind(next)");
        catalog.AppendLine("    return next,nil\n}\ntype ConfigManager struct { gate sync.Mutex; current atomic.Pointer[ConfigSnapshot] }\nfunc(m *ConfigManager) Current()*ConfigSnapshot { return m.current.Load() }\nfunc(m *ConfigManager) Reload(root string,preferJSON bool)(*ConfigSnapshot,error){m.gate.Lock();defer m.gate.Unlock();next,err:=LoadConfig(root,preferJSON);if err!=nil{return nil,err};m.current.Store(next);return next,nil}\n");
        if(partitioned)
        {
            GoStorageParts(catalog,tables,"zrConfigSnapshotPart","Table");
        }
        catalog.Append(catalogHelpers);files.Add("Go/snapshot.go", catalog.ToString());
        var server = new StringBuilder("// Server entry: rows retain their complete original snapshot.\npackage zrconfig\n\ntype ServerConfigManager = ServerManager[ConfigSnapshot]\nfunc LoadServerConfig(root string, preferJSON bool)(*ConfigSnapshot,error){\n    snapshot,err:=LoadConfig(root,preferJSON);if err!=nil{return nil,err}\n");
        var serverHelpers=new StringBuilder();
        GoPhase(server,serverHelpers,tables,"zrServerRetainPart$part(snapshot *ConfigSnapshot)","zrServerRetainPart$part(snapshot)",table=>$"    for _,row:=range snapshot.{GoMember(table.Name)}.rows.values{{row.serverOwner=snapshot}}");
        server.AppendLine("    return snapshot,nil\n}");
        server.AppendLine($"var serverCatalog = newCatalog({Quote(RuntimeCatalog.Create(tables,_names.Target).Serialize())})\nfunc ConfigurationCatalog()*Catalog{{return serverCatalog}}\ntype SelectedSnapshot struct{{storage *ConfigSnapshot;selection SelectionPlan}}\nfunc(s *SelectedSnapshot)Version()string{{return s.storage.version}}\nfunc(s *SelectedSnapshot)Selection()SelectionPlan{{return s.selection}}\nfunc LoadSelectedConfig(root string,requested []string,preferJSON bool)(snapshot *SelectedSnapshot,err error){{\n    defer recoverError(&err)\n    plan,err:=serverCatalog.Select(requested);if err!=nil{{return nil,err}}\n    b:=openBundle(root);if err=b.validateCatalog(serverCatalog);err!=nil{{return nil,err}}\n    selected:=map[string]bool{{}};for _,name:=range plan.tables.values{{selected[name]=true}}\n    storage:=&ConfigSnapshot{{version:b.version}}");
        GoPhase(server,serverHelpers,tables,"zrSelectedLoadPart$part(storage *ConfigSnapshot,b *bundle,selected map[string]bool,preferJSON bool)","zrSelectedLoadPart$part(storage,b,selected,preferJSON)",table=>$"    if selected[{Quote(table.Name)}]{{storage.{GoMember(table.Name)}=load{table.Name}(b.load(schema{table.Name},preferJSON))}}");
        GoPhase(server,serverHelpers,tables,"zrSelectedBindPart$part(storage *ConfigSnapshot,selected map[string]bool)","zrSelectedBindPart$part(storage,selected)",table=>$"    if selected[{Quote(table.Name)}]{{storage.{GoMember(table.Name)}.bind(storage);for _,row:=range storage.{GoMember(table.Name)}.rows.values{{row.serverOwner=storage}}}}");
        server.AppendLine("    return &SelectedSnapshot{storage:storage,selection:plan},nil\n}");
        foreach (var table in tables) server.AppendLine($"func(s *SelectedSnapshot)Get_{table.Name}()(*{table.Name}Table,error){{if s.storage.{GoMember(table.Name)}==nil{{return nil,serverError(TableNotSelected)}};return s.storage.{GoMember(table.Name)},nil}}");
        server.Append(serverHelpers);if (_names.ServerApi) files.Add("Go/server.go", server.ToString());
        if (_names.ServerApi) GoData(tables,files,GType,(id,value)=>Read(id,value));
        foreach (var table in tables)
        {
            string row = table.Name + "Config";
            var text = new StringBuilder($"// 此文件由工具自动生成，请勿手工修改。\npackage zrconfig\n\ntype {row} struct {{\n");
            text.AppendLine("    serverOwner *ConfigSnapshot");
            foreach (var f in table.Fields)
            {
                text.AppendLine($"    v_{f.Name} {GType(f.Type)}");
                if (types[f.Type].Kind == "tablereference")
                    text.AppendLine($"    ref_{f.Name} *{ReferenceTable(f.Type)}Config");
            }
            text.AppendLine("}");
            foreach (var f in table.Fields)
            {
                Comment(text, f.Description, "");
                bool reference = types[f.Type].Kind == "tablereference";
                text.AppendLine($"func(v *{row}) {Accessor(f.Name, "go")}{(reference ? "Id" : "")}() {GType(f.Type)} {{ return v.v_{f.Name} }}");
                if (reference)
                    text.AppendLine($"func(v *{row}) {Accessor(f.Name, "go")}() *{ReferenceTable(f.Type)}Config {{ return v.ref_{f.Name} }}");
            }
            text.AppendLine($"const schema{table.Name} = {Quote(table.Serialize())}");
            var indexes = new List<(string Name, string[] Fields, string? Group)> { ("Key", table.Keys, null) };
            indexes.AddRange(table.Groups.Select(g => ("Group_" + g.Name, g.Fields, (string?)g.Name)));
            string KeyName(string index) => table.Name + "Key_" + index;
            foreach (var index in indexes)
            {
                text.AppendLine($"type {KeyName(index.Name)} struct {{");
                foreach (string f in index.Fields)
                    text.AppendLine($"    v_{f} {GType(table.Fields.Single(x => x.Name == f).Type)}");
                text.AppendLine("}");
            }
            text.AppendLine($"type {table.Name}Table struct {{\n    rows List[*{row}]");
            foreach (var index in indexes)
                text.AppendLine($"    index_{index.Name} map[{KeyName(index.Name)}]{(index.Name == "Key" ? "*" + row : "List[*" + row + "]")}");
            if (tables.Any(t => t.Fields.Any(f => types[f.Type].Kind == "tablereference" && ReferenceTable(f.Type) == table.Name)))
                text.AppendLine($"    ids map[int32]*{row}");
            text.AppendLine("}");
            text.AppendLine($"func load{table.Name}(source []any)*{table.Name}Table {{\n    result:=&{table.Name}Table{{rows:List[*{row}]{{values:make([]*{row},len(source))}}}}");
            foreach (var index in indexes)
                text.AppendLine($"    result.index_{index.Name}=make(map[{KeyName(index.Name)}]{(index.Name == "Key" ? "*" + row : "List[*" + row + "]")},len(source))");
            if (tables.Any(t => t.Fields.Any(f => types[f.Type].Kind == "tablereference" && ReferenceTable(f.Type) == table.Name)))
                text.AppendLine($"    result.ids=make(map[int32]*{row},len(source))");
            text.AppendLine($"    for i,value:=range source {{\n        row:=&{row}{{");
            foreach (var f in table.Fields)
                text.AppendLine($"            v_{f.Name}: {Read(f.Type, "field(value," + Quote(f.Name) + ")")},");
            text.AppendLine("        }\n        result.rows.values[i]=row");
            foreach (var index in indexes)
            {
                string key = $"{KeyName(index.Name)}{{{string.Join(",", index.Fields.Select(f => "row.v_" + f))}}}";
                text.AppendLine($"        key_{index.Name}:={key}");
                if (index.Name == "Key")
                    text.AppendLine($"        if _,ok:=result.index_Key[key_Key];ok{{invalid(\"主键重复\")}}\n        result.index_Key[key_Key]=row");
                else
                    text.AppendLine($"        group_{index.Name}:=result.index_{index.Name}[key_{index.Name}]\n        group_{index.Name}.values=append(group_{index.Name}.values,row)\n        result.index_{index.Name}[key_{index.Name}]=group_{index.Name}");
            }
            if (tables.Any(t => t.Fields.Any(f => types[f.Type].Kind == "tablereference" && ReferenceTable(f.Type) == table.Name)))
                text.AppendLine("        if _,ok:=result.ids[row.v_Id];ok{invalid(\"引用 Id 不唯一\")}\n        result.ids[row.v_Id]=row");
            text.AppendLine("    }\n    return result\n}");
            text.AppendLine($"func(t *{table.Name}Table)bind(snapshot *ConfigSnapshot){{");
            foreach (var f in table.Fields.Where(f => types[f.Type].Kind == "tablereference"))
                text.AppendLine($"    for _,row:=range t.rows.values{{if row.v_{f.Name}!=0{{row.ref_{f.Name}=snapshot.{GoMember(ReferenceTable(f.Type))}.ids[row.v_{f.Name}];if row.ref_{f.Name}==nil{{invalid(\"引用目标不存在：{table.Name}.{f.Name}\")}}}}}}");
            text.AppendLine("}");
            text.AppendLine($"func(t *{table.Name}Table) Rows()List[*{row}]{{return t.rows}}\nfunc(t *{table.Name}Table) Count()int{{return t.rows.Len()}}");
            foreach (var index in indexes)
            {
                string args = string.Join(",", index.Fields.Select((name, i) => "k" + i + " " + GType(table.Fields.Single(f => f.Name == name).Type)));
                string key = $"{KeyName(index.Name)}{{{string.Join(",", index.Fields.Select((f, i) => "k" + i))}}}";
                if (index.Name == "Key")
                {
                    text.AppendLine($"func(t *{table.Name}Table) Find({args})(*{row},bool){{value,ok:=t.index_Key[{key}];return value,ok}}")
                        .AppendLine($"func(t *{table.Name}Table) Get({args})*{row}{{value:=t.index_Key[{key}];if value==nil{{invalid(\"配置主键不存在\")}};return value}}");
                }
                else
                    text.AppendLine($"func(t *{table.Name}Table) Get{index.Group}({args})List[*{row}]{{return t.index_{index.Name}[{key}]}}");
            }
            // Go ignores source files beginning with '_' even when their declarations are valid.
            files.Add(_names.TablePath(table.Name, "go"), text.ToString());
        }
    }

    private void GoData(PortableTable[] tables,Dictionary<string,string> files,Func<string,string> typeName,Func<string,string,string> read)
    {
        var types=Types(tables);
        var helpers=new StringBuilder();bool partitioned=tables.Length>GoPartitionThreshold;
        var text=new StringBuilder("// Generated independent server views.\npackage zrconfig\nimport(\"context\";\"sync/atomic\")\ntype DataSnapshot struct{version string;selection SelectionPlan;mode LoadMode;manifestDigest,sourceNamespace,loadPlanFingerprint string;budget *loadBudget;memory *SnapshotMemory\n");
        if(partitioned)for(int i=0;i<(tables.Length+GoPartSize-1)/GoPartSize;i++)text.AppendLine($"part_{i} zrDataSnapshotPart{i}");
        else foreach(var table in tables)text.AppendLine($"table_{table.Name} *{table.Name}Rows");
        text.AppendLine("}\ntype DataManager = ServerManager[DataSnapshot]\nfunc(s *DataSnapshot)Version()string{return s.version}\nfunc(s *DataSnapshot)Selection()SelectionPlan{return s.selection}\nfunc(s *DataSnapshot)Mode()LoadMode{return s.mode}\nfunc LoadData(ctx context.Context,root string,plan LoadPlan)(*DataSnapshot,error){source,err:=NewFileConfigSource(root);if err!=nil{return nil,err};if plan.SourceNamespace==\"\"{plan.SourceNamespace=source.root};return LoadDataSource(ctx,source,plan)}\nfunc(s *DataSnapshot)ManifestDigest()string{return s.manifestDigest}\nfunc(s *DataSnapshot)SourceNamespace()string{return s.sourceNamespace}\nfunc(s *DataSnapshot)CatalogFingerprint()string{return s.selection.CatalogFingerprint()}\nfunc(s *DataSnapshot)LoadPlanFingerprint()string{return s.loadPlanFingerprint}\nfunc(s *DataSnapshot)Diagnostics()SnapshotDiagnostics{return s.budget.diagnostics()}\nfunc(s *DataSnapshot)SnapshotMemory()*SnapshotMemory{return s.memory}\nfunc LoadDataSource(ctx context.Context,source ConfigSource,plan LoadPlan)(snapshot *DataSnapshot,err error){\ndefer recoverServer(&err)\nlimits,err:=plan.Limits.validated();if err!=nil{return nil,err};if source==nil{return nil,serverError(InvalidPlan)}\nif err=checkContext(ctx);err!=nil{return nil,err};mode:=plan.Mode;if mode==\"\"{mode=ServerDefault};if mode!=ServerDefault&&mode!=SelectedTables&&mode!=LazyRowsStrict&&mode!=LazyRowsOnAccess{return nil,serverError(InvalidPlan)}\nrequested:=append([]string(nil),plan.Tables...);if plan.Tables==nil{for _,table:=range serverCatalog.tables.values{requested=append(requested,table.name)}};selection,err:=serverCatalog.Select(requested);if err!=nil{return nil,err};selected:=map[string]bool{};for _,name:=range selection.tables.values{selected[name]=true};warmup:=append([]string(nil),plan.WarmupTables...);for _,name:=range warmup{if !selected[name]{return nil,serverError(InvalidPlan)}}\nb:=openServerBundle(ctx,source,limits);if err=b.validateCatalog(serverCatalog);err!=nil{return nil,err};indexed:=map[string]*indexedRows{};next:=&DataSnapshot{loadPlanFingerprint:loadPlanFingerprint(mode,plan.PreferJSON,selection.tables.values),version:b.version,selection:selection,mode:mode,manifestDigest:b.digest,sourceNamespace:plan.SourceNamespace,budget:b.budget,memory:newBudgetSnapshotMemory(b.budget)}");
        GoPhase(text,helpers,tables,"zrDataOpenPart$part(ctx context.Context,next *DataSnapshot,b *bundle,plan LoadPlan,selected map[string]bool,indexed map[string]*indexedRows)(snapshot *DataSnapshot,err error)","if _,err=zrDataOpenPart$part(ctx,next,b,plan,selected,indexed);err!=nil{return nil,err}",table=>$"if selected[{Quote(table.Name)}]{{if err=checkContext(ctx);err!=nil{{return nil,err}};indexed[{Quote(table.Name)}]=b.openRows(schema{table.Name},plan.PreferJSON);next.{GoMember(table.Name)}=new{table.Name}Rows(indexed[{Quote(table.Name)}],next)}}","return next,nil");
        text.AppendLine("for _,data:=range indexed{data.resolve=indexed}\nif mode!=LazyRowsOnAccess{");
        GoPhase(text,helpers,tables,"zrDataValidatePart$part(ctx context.Context,next *DataSnapshot,indexed map[string]*indexedRows)(snapshot *DataSnapshot,err error)","if _,err=zrDataValidatePart$part(ctx,next,indexed);err!=nil{return nil,err}",table=>$"if data:=indexed[{Quote(table.Name)}];data!=nil{{for i:=range data.ids{{if err=checkContext(ctx);err!=nil{{return nil,err}};value:=data.readTransient(i);_=read{table.Name}Row(value,next);if err=data.validateReferences(value,indexed);err!=nil{{return nil,err}}}}}}","return next,nil");
        text.AppendLine("}\nif mode==ServerDefault||mode==SelectedTables{if err=next.LoadAll(ctx);err!=nil{return nil,err}}\nfor _,name:=range warmup{"+(partitioned?"err=zrDataWarmup(ctx,next,name)":"switch name{"));
        if(!partitioned)foreach(var table in tables)text.AppendLine($"case {Quote(table.Name)}:_,err=next.{GoMember(table.Name)}.LoadAll(ctx)");
        text.AppendLine((partitioned?"":"}")+";if err!=nil{return nil,err}};if err=checkContext(ctx);err!=nil{return nil,err};return next,nil\n}\nfunc(s *DataSnapshot)LoadAll(ctx context.Context)error{");
        GoPhase(text,helpers,tables,"zrDataLoadAllPart$part(ctx context.Context,s *DataSnapshot)error","if err:=zrDataLoadAllPart$part(ctx,s);err!=nil{return err}",table=>$"if s.{GoMember(table.Name)}!=nil{{if _,err:=s.{GoMember(table.Name)}.LoadAll(ctx);err!=nil{{return err}}}}","return nil");
        text.AppendLine("return nil\n}\nfunc(s *DataSnapshot)LoadedRowCount()int{count:=0");
        GoPhase(text,helpers,tables,"zrDataLoadedCountPart$part(s *DataSnapshot)(count int)","count+=zrDataLoadedCountPart$part(s)",table=>$"if s.{GoMember(table.Name)}!=nil{{count+=s.{GoMember(table.Name)}.LoadedCount()}}","return count");
        text.AppendLine("return count\n}");
        text.AppendLine("func(s *DataSnapshot)CompletedTableCount()int{count:=0");
        GoPhase(text,helpers,tables,"zrDataCompletedCountPart$part(s *DataSnapshot)(count int)","count+=zrDataCompletedCountPart$part(s)",table=>$"if s.{GoMember(table.Name)}!=nil&&s.{GoMember(table.Name)}.full.Load(){{count++}}","return count");
        text.AppendLine("return count}");
        if(partitioned)
        {
            text.AppendLine("func zrDataWarmup(ctx context.Context,next *DataSnapshot,name string)error{");
            GoPhase(text,helpers,tables,"zrDataWarmupPart$part(ctx context.Context,next *DataSnapshot,name string)(handled bool,err error)","if handled,err:=zrDataWarmupPart$part(ctx,next,name);handled{return err}",table=>$"case {Quote(table.Name)}:_,err=next.{GoMember(table.Name)}.LoadAll(ctx);return true,err","};return false,nil","switch name{");
            text.AppendLine("return serverError(InvalidPlan)\n}");
        }
        text.AppendLine("type DataWarmup struct{table string;action func(context.Context,*DataSnapshot)error}\nfunc(w DataWarmup)Table()string{return w.table}\nfunc LoadDataSourceWithWarmup(ctx context.Context,source ConfigSource,plan LoadPlan,requests []DataWarmup)(*DataSnapshot,error){\nsteps:=append([]DataWarmup(nil),requests...);names:=plan.Tables;if names==nil{for _,table:=range serverCatalog.tables.values{names=append(names,table.name)}};selection,err:=serverCatalog.Select(names);if err!=nil{return nil,err};selected:=map[string]bool{};for _,name:=range selection.tables.values{selected[name]=true};for _,step:=range steps{if step.action==nil||!selected[step.table]{return nil,serverError(InvalidPlan)}}\nnext,err:=LoadDataSource(ctx,source,plan);if err!=nil{return nil,err};for _,step:=range steps{if err=checkContext(ctx);err!=nil{return nil,err};if err=step.action(ctx,next);err!=nil{return nil,err}};if err=checkContext(ctx);err!=nil{return nil,err};return next,nil\n}\nfunc LoadDataWithWarmup(ctx context.Context,root string,plan LoadPlan,requests []DataWarmup)(*DataSnapshot,error){source,err:=NewFileConfigSource(root);if err!=nil{return nil,err};if plan.SourceNamespace==\"\"{plan.SourceNamespace=source.root};return LoadDataSourceWithWarmup(ctx,source,plan,requests)}");
        foreach(var table in tables)
        {
            text.AppendLine($"func WarmupTable_{table.Name}()DataWarmup{{return DataWarmup{{table:{Quote(table.Name)},action:func(ctx context.Context,snapshot *DataSnapshot)error{{table,err:=snapshot.Get_{table.Name}();if err!=nil{{return err}};_,err=table.LoadAll(ctx);return err}}}}}}");
            foreach(var item in new[]{(Name:"Row_"+table.Name,Fields:table.Keys,Method:"Load")}.Concat(table.Groups.Select(g=>(Name:"Group_"+table.Name+"_"+g.Name,Fields:g.Fields,Method:"LoadGroup_"+g.Name))))
            {
                var fields=item.Fields.Select(n=>table.Fields.Single(f=>f.Name==n)).ToArray();
                string args=string.Join(",",fields.Select((f,i)=>"k"+i+" "+typeName(f.Type))),calls=string.Join(",",fields.Select((_,i)=>"k"+i));
                text.AppendLine($"func Warmup{item.Name}({args})DataWarmup{{return DataWarmup{{table:{Quote(table.Name)},action:func(ctx context.Context,snapshot *DataSnapshot)error{{table,err:=snapshot.Get_{table.Name}();if err!=nil{{return err}};_,err=table.{item.Method}(ctx,{calls});return err}}}}}}");
            }
        }
        foreach(var table in tables)text.AppendLine($"func(s *DataSnapshot)Get_{table.Name}()(*{table.Name}Rows,error){{if s.{GoMember(table.Name)}==nil{{return nil,serverError(TableNotSelected)}};return s.{GoMember(table.Name)},nil}}");
        text.AppendLine("type dataGroup[T any]struct{ordinals []int;ready atomic.Pointer[List[*T]]}\nfunc(g *dataGroup[T])view(ctx context.Context,storage *rowStorage[T],load bool)(List[*T],error){if g==nil{return List[*T]{},nil};if ready:=g.ready.Load();ready!=nil{return *ready,nil};rows:=List[*T]{values:make([]*T,len(g.ordinals))};for i,index:=range g.ordinals{var row *T;var err error;if load{row,err=storage.load(ctx,index)}else{row,err=storage.query(index)};if err!=nil{return List[*T]{},err};rows.values[i]=row};g.ready.CompareAndSwap(nil,&rows);return *g.ready.Load(),nil}");
        foreach(var table in tables)
        {
            string row=table.Name+"Row",view=table.Name+"Rows";
            text.AppendLine($"type {row} struct{{owner *DataSnapshot");
            foreach(var field in table.Fields)text.AppendLine($"v_{field.Name} {typeName(field.Type)}");
            text.AppendLine($"}}\nfunc read{row}(value any,owner *DataSnapshot)*{row}{{return &{row}{{owner:owner,");
            foreach(var field in table.Fields)text.AppendLine($"v_{field.Name}:{read(field.Type,$"field(value,{Quote(field.Name)})")},");
            text.AppendLine("}}");
            foreach(var field in table.Fields)
            {
                bool reference=types[field.Type].Kind=="tablereference";
                text.AppendLine($"func(v *{row}){Accessor(field.Name,"go")}{(reference?"Id":"")}(){typeName(field.Type)}{{return v.v_{field.Name}}}");
                if(reference)
                {
                    string target=types[field.Type].ReferenceTable!;
                    text.AppendLine($"func(v *{row})Get_{field.Name}()(*{target}Row,error){{return v.owner.{GoMember(target)}.storage.reference(nil,v.v_{field.Name},false)}}\nfunc(v *{row})Load_{field.Name}(ctx context.Context)(*{target}Row,error){{return v.owner.{GoMember(target)}.storage.reference(ctx,v.v_{field.Name},true)}}");
                }
            }
            text.AppendLine($"type {view} struct{{storage *rowStorage[{row}];keyIndex map[{table.Name}Key_Key]int;full atomic.Bool");
            foreach(var group in table.Groups)text.AppendLine($"group_{group.Name} oneValue[map[{table.Name}Key_Group_{group.Name}]*dataGroup[{row}]]");
            text.AppendLine($"}}\nfunc new{view}(indexed *indexedRows,owner *DataSnapshot)*{view}{{result:=&{view}{{storage:newRowStorage(indexed,func(value any)*{row}{{return read{row}(value,owner)}}),keyIndex:make(map[{table.Name}Key_Key]int,len(indexed.ids))}};for i,value:=range indexed.keys{{key:={table.Name}Key_Key{{{string.Join(",",table.Keys.Select(n=>read(table.Fields.Single(f=>f.Name==n).Type,$"field(value,{Quote(n)})")))}}};if _,exists:=result.keyIndex[key];exists{{panic(serverError(InvalidValue))}};result.keyIndex[key]=i}};return result}}");
            string args=string.Join(",",table.Keys.Select((n,i)=>"k"+i+" "+typeName(table.Fields.Single(f=>f.Name==n).Type)));
            string calls=string.Join(",",table.Keys.Select((_,i)=>"k"+i));string key=$"{table.Name}Key_Key{{{calls}}}";
            text.AppendLine($"func(t *{view})Find({args})(*{row},bool,error){{index,ok:=t.keyIndex[{key}];if !ok{{return nil,false,nil}};row,err:=t.storage.query(index);return row,err==nil,err}}\nfunc(t *{view})Get({args})(*{row},error){{row,found,err:=t.Find({calls});if err==nil&&!found{{err=serverError(KeyNotFound)}};return row,err}}\nfunc(t *{view})TryLoad(ctx context.Context,{args})(*{row},bool,error){{if err:=checkContext(ctx);err!=nil{{return nil,false,err}};index,ok:=t.keyIndex[{key}];if !ok{{return nil,false,nil}};row,err:=t.storage.load(ctx,index);return row,err==nil,err}}\nfunc(t *{view})Load(ctx context.Context,{args})(*{row},error){{row,found,err:=t.TryLoad(ctx,{calls});if err==nil&&!found{{err=serverError(KeyNotFound)}};return row,err}}\nfunc(t *{view})Count()int{{return len(t.storage.ready)}}\nfunc(t *{view})LoadedCount()int{{return int(t.storage.loaded.Load())}}\nfunc(t *{view})Rows()(List[*{row}],error){{if !t.full.Load(){{return List[*{row}]{{}},serverError(TableNotLoaded)}};return *t.storage.all.Load(),nil}}\nfunc(t *{view})LoadAll(ctx context.Context)(List[*{row}],error){{rows,err:=t.storage.loadAll(ctx);if err!=nil{{return rows,err}}");
            foreach(var group in table.Groups)text.AppendLine($"if _,err=t.index_{group.Name}(ctx,true);err!=nil{{return List[*{row}]{{}},err}}");
            text.AppendLine("t.full.Store(true);return rows,nil}");
            foreach(var group in table.Groups)
            {
                string g=group.Name,gtype=$"{table.Name}Key_Group_{g}";
                string gargs=string.Join(",",group.Fields.Select((n,i)=>"k"+i+" "+typeName(table.Fields.Single(f=>f.Name==n).Type)));
                string retainedStrings=string.Concat(group.Fields.Select((n,i)=>typeName(table.Fields.Single(f=>f.Name==n).Type)=="string"?$"retained+=int64(len(key.v_{n}))*4;":""));
                string gkey=gtype+"{"+string.Join(",",group.Fields.Select((_,i)=>"k"+i))+"}";
                string fromCached=gtype+"{"+string.Join(",",group.Fields.Select(n=>"cached.v_"+n))+"}";
                string create=gtype+"{"+string.Join(",",group.Fields.Select(n=>read(table.Fields.Single(f=>f.Name==n).Type,$"field(value,{Quote(n)})")))+"}";
                text.AppendLine($"func(t *{view})index_{g}(ctx context.Context,load bool)(map[{gtype}]*dataGroup[{row}],error){{if cached:=t.group_{g}.ready.Load();cached!=nil{{return *cached,nil}};if !load{{return t.group_{g}.get()}};return t.buildIndex_{g}(ctx)}}\nfunc(t *{view})buildIndex_{g}(ctx context.Context)(map[{gtype}]*dataGroup[{row}],error){{return t.group_{g}.load(ctx,t.storage.indexed.budget,t.storage.indexed.estimatedIndexBytes({group.Fields.Length}),func()(map[{gtype}]*dataGroup[{row}],int64,error){{retained:=int64(256)+int64(t.Count())*512*{group.Fields.Length};result:=make(map[{gtype}]*dataGroup[{row}]);for i:=0;i<t.Count();i++{{var key {gtype};if cached:=t.storage.ready[i].Load();cached!=nil{{key={fromCached}}}else{{value:=t.storage.indexed.readTransient(i);key={create}}};{retainedStrings}group:=result[key];if group==nil{{group=&dataGroup[{row}]{{}};result[key]=group}};group.ordinals=append(group.ordinals,i)}};return result,retained,nil}})}}\nfunc(t *{view})FindGroup_{g}({gargs})(List[*{row}],error){{index,err:=t.index_{g}(nil,false);if err!=nil{{return List[*{row}]{{}},err}};return index[{gkey}].view(nil,t.storage,false)}}\nfunc(t *{view})LoadGroup_{g}(ctx context.Context,{gargs})(List[*{row}],error){{if err:=checkContext(ctx);err!=nil{{return List[*{row}]{{}},err}};index,err:=t.index_{g}(ctx,true);if err!=nil{{return List[*{row}]{{}},err}};return index[{gkey}].view(ctx,t.storage,true)}}");
            }
        }
        if (tables.Length == 0) text.Replace("import(\"context\";\"sync/atomic\")", "import(\"context\";\"sync/atomic\")");
        if(partitioned)
        {
            GoStorageParts(text,tables,"zrDataSnapshotPart","Rows");
        }
        text.Append(helpers);files.Add("Go/data.go",text.ToString());
    }

    private string ReferenceTable(string id) => _referenceTables[id];
}
