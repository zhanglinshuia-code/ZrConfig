using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZrConfig.Core;

[JsonConverter(typeof(GeneratedNameBindingJsonConverter))]
public sealed record GeneratedNameBinding(string SymbolId, string Language, string Scope, string SourceBinding, string Kind, string Name, bool Retired);

// Canonical/default writing preserves the established descriptor digest byte for
// byte. Large schema files may use a compact persistence representation; reading
// expands it before policy validation, comparison or digest verification.
internal sealed class GeneratedNameBindingJsonConverter : JsonConverter<GeneratedNameBinding>
{
    private readonly bool _compact;
    public GeneratedNameBindingJsonConverter() { }
    internal GeneratedNameBindingJsonConverter(bool compact) => _compact=compact;
    private sealed record Expanded(string SymbolId,string Language,string Scope,string SourceBinding,string Kind,string Name,bool Retired);
    public override GeneratedNameBinding? Read(ref Utf8JsonReader reader,Type type,JsonSerializerOptions options)
    {
        if(reader.TokenType!=JsonTokenType.StartArray)
        {
            var value=JsonSerializer.Deserialize<Expanded>(ref reader,options);
            return value==null?null:new(value.SymbolId,value.Language,value.Scope,value.SourceBinding,value.Kind,value.Name,value.Retired);
        }
        string? Text(ref Utf8JsonReader input){if(!input.Read()||input.TokenType is not(JsonTokenType.String or JsonTokenType.Null))throw new JsonException("Invalid compact name binding.");return input.GetString();}
        string? id=Text(ref reader),language=Text(ref reader),scope=Text(ref reader),source=Text(ref reader),kind=Text(ref reader),name=Text(ref reader);
        if(id==null||language==null||scope==null||kind==null||name==null||source==null&&kind!="reserved"||!reader.Read()||reader.TokenType is not(JsonTokenType.True or JsonTokenType.False))throw new JsonException("Invalid compact name binding.");
        bool retired=reader.GetBoolean();if(!reader.Read()||reader.TokenType!=JsonTokenType.EndArray)throw new JsonException("Invalid compact name binding length.");
        return new(id,language,scope,source??"$generated/"+scope+"/"+name,kind,name,retired);
    }
    public override void Write(Utf8JsonWriter writer,GeneratedNameBinding value,JsonSerializerOptions options)
    {
        if(!_compact)
        {
            JsonSerializer.Serialize(writer,new Expanded(value.SymbolId,value.Language,value.Scope,value.SourceBinding,value.Kind,value.Name,value.Retired),options);return;
        }
        writer.WriteStartArray();writer.WriteStringValue(value.SymbolId);writer.WriteStringValue(value.Language);writer.WriteStringValue(value.Scope);
        if(value.Kind=="reserved"&&value.SourceBinding=="$generated/"+value.Scope+"/"+value.Name)writer.WriteNullValue();else writer.WriteStringValue(value.SourceBinding);
        writer.WriteStringValue(value.Kind);writer.WriteStringValue(value.Name);writer.WriteBooleanValue(value.Retired);writer.WriteEndArray();
    }
}

/// <summary>One immutable naming decision per logical symbol; language backends consume this plan.</summary>
internal sealed class GeneratedSymbolPlan
{
    private readonly Dictionary<(string Language, string Scope, string Name), GeneratedNameBinding> _scopes = new();
    private readonly Dictionary<(string Language, string Name), string> _accessors = new();
    private readonly Dictionary<string, string> _types = new(StringComparer.Ordinal);
    private readonly Dictionary<(string Table, string Role), string> _csharpHelpers = new();
    private readonly Dictionary<string,string> _paths = new(StringComparer.OrdinalIgnoreCase);
    internal Type[] PublicDependencies { get; private set; } = [];
    internal bool ServerApi { get; private set; }
    internal GenerationSourceContract SourceContract { get; set; } = GenerationSourceContract.Empty;
    internal string Target { get; set; } = "";
    internal GenerationNamespaces Namespaces {get;set;} = new("ZrConfig.Models;ZrConfig.Runtime","zrconfig","zrconfig","zrconfig","zrconfig","./snapshot;./data","ConfigSnapshot;data");
    internal string CSharpHelper(string table, string role) => _csharpHelpers[(table,role)];
    internal string TablePath(string table, string language)
    {
        string path = language switch
        {
            "csharp" => table + "Config.g.cs",
            "java" => "Java/" + table + "Config.java",
            "go" => "Go/" + (table.StartsWith('_') ? "table_" : "") + table.ToLowerInvariant() + ".g.go",
            "cpp" => "Cpp/" + table + "Config.hpp",
            "rust" => "Rust/" + table + "Config.rs",
            "typescript" => "TypeScript/" + table + "Config.ts",
            "lua" => "Lua/" + table + "Config.lua",
            _ => throw new InvalidDataException("Unknown language: " + language)
        };
        PlanPath(language,path,"table/"+table+"/file"); return path;
    }
    private void PlanPath(string language,string path,string source)
    {
        if(_paths.TryGetValue(path,out string? previous) && previous!=source) throw new NotSupportedException("跨平台生成路径冲突："+path+"（"+previous+" / "+source+"）");
        if(Regex.IsMatch(Path.GetFileName(path),@"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant))
            throw new NotSupportedException("Windows 保留路径："+path);
        if(path.Length>240 || path.Split('/').Any(part=>part.Length==0 || part is "." or ".." || part.EndsWith(' ') || part.EndsWith('.') || part.Any(c=>char.IsControl(c)||c=='\\'||c==':')))
            throw new NotSupportedException("生成路径超出跨平台预算或包含非法分隔符："+path);
        _paths[path]=source; Claim(language,"$files",path,source,"path");
    }
    internal GeneratedNameBinding[] Bindings => _scopes.Values.Select(binding=>binding with { Scope=Namespaces.ScopeFor(binding.Language,binding.Scope),Name=binding.Kind=="path"?Namespaces.PathFor(binding.Name):binding.Name }).OrderBy(s => s.Language, StringComparer.Ordinal)
        .ThenBy(s => s.Scope, StringComparer.Ordinal).ThenBy(s => s.SourceBinding, StringComparer.Ordinal).ToArray();

    internal string TypeName(string source)
    {
        if (!_types.TryGetValue(source, out string? name)) _types.Add(source, name = "T_" + Regex.Replace(source, "[^A-Za-z0-9_]", "_"));
        return name;
    }
    internal string Accessor(string source, string language)
    {
        if (!_accessors.TryGetValue((language, source), out string? name))
        {
            name = language switch
            {
                "java" => source == "Class" ? "Class_" : source,
                "go" => source[0] is >= 'A' and <= 'Z' ? source : "Get_" + source,
                "cpp" => CppKeywords.Contains(source) ? source + "_" : source,
                _ => source
            };
            _accessors.Add((language, source), name);
        }
        return name;
    }
    private void Claim(string language, string scope, string name, string binding, string kind = "member")
    {
        var key = (language, scope, name);
        if (_scopes.TryGetValue(key, out GeneratedNameBinding? previous))
        {
            if (previous.SourceBinding == binding && previous.Kind == kind) return;
            throw new NotSupportedException($"符号冲突 [{language}] {scope}.{name}：{previous.SourceBinding} 与 {binding}。已有公开名不会自动改写；请调整新增项。");
        }
        _scopes.Add(key, new GeneratedNameBinding("", language, scope, binding, kind, name, false));
    }
    private void Reserve(string language, string scope, params string[] names)
    { foreach (string name in names) Claim(language, scope, name, "$generated/" + scope + "/" + name, "reserved"); }

    internal static GeneratedSymbolPlan Create(IReadOnlyList<TableSchema> schemas, PortableTable[]? portable, string[] languages, bool serverApi = true)
    {
        var plan = new GeneratedSymbolPlan { ServerApi = serverApi };
        plan.PublicDependencies = ConfigTypeRegistry.Graph(schemas.SelectMany(s=>s.Fields).Select(f=>f.Type)).Select(t=>t.ClrType).Distinct().ToArray();
        plan.CSharp(schemas);
        if (portable != null) foreach (string language in languages.Where(l => l != "csharp")) plan.Native(portable, language);
        return plan;
    }
    private void CSharp(IReadOnlyList<TableSchema> schemas)
    {
        const string language = "csharp", modelScope = "ZrConfig.Models", serverScope = "ZrConfig.Models.Server";
        Reserve(language, modelScope, "TableCodecs");
        if (ServerApi) Reserve(language, modelScope, "ServerCatalog");
        PlanPath(language,"TableCodecs.g.cs","$generated/registry");
        foreach (TableSchema table in schemas)
        {
            PlanCSharpHelpers(table);
            string source = "table/" + table.Name;
            TablePath(table.Name,language);
            foreach (string suffix in new[] { "Config", "ConfigTable", "ConfigTableAccess", "ConfigCodec" })
                Claim(language, modelScope, table.Name + suffix, source + "/" + suffix, "type");
            if (ServerApi) foreach (string suffix in new[] { "Row", "Table", "Group", "Access" })
                Claim(language, serverScope, table.Name + suffix, source + "/server/" + suffix, "type");
            string row = serverScope + "." + table.Name + "Row", view = serverScope + "." + table.Name + "Table";
            if (ServerApi) Reserve(language, row, table.Name + "Row", "Snapshot", "Borrow", "_handle");
            Reserve(language, modelScope + "." + table.Name + "Config", table.Name + "Config");
            if (ServerApi) Reserve(language, view, table.Name + "Table", "Count", "Get", "TryGet", "Load", "TryLoad", "LoadAsync", "LoadAll", "Rows", "WarmupRow", "GroupDescriptors", "_snapshot");
            foreach (FieldSchema field in table.Fields)
            {
                Claim(language, modelScope + "." + table.Name + "Config", field.Name, source + "/field/" + field.Name, "property");
                if (!ServerApi) continue;
                if (field.Type.ReferenceTable != null)
                    foreach (string member in new[] { field.Name + "Id", "Get" + field.Name, "Load" + field.Name })
                        Claim(language, row, member, source + "/field/" + field.Name + "/" + member);
                else Claim(language, row, field.Name, source + "/field/" + field.Name, "property");
            }
            if (ServerApi) foreach (TableGroupSchema group in table.Groups)
                foreach (string prefix in new[] { "Find", "Load", "Warmup" }) Claim(language, view, prefix + group.Name, source + "/group/" + group.Name + "/" + prefix);
            foreach (var helper in _csharpHelpers.Where(p=>p.Key.Table==table.Name))
                Claim(language,modelScope+"."+table.Name+"Config",helper.Value,source+"/helper/"+helper.Key.Role,"helper");
        }
        if (!ServerApi) return;
        foreach ((ConfigType type, ConfigMember[] members) in ServerValueGenerator.Shapes(schemas))
        {
            if (type.Kind is not (ConfigTypeKind.Array or ConfigTypeKind.Dictionary or ConfigTypeKind.Object or ConfigTypeKind.Converter)) continue;
            string name = ServerValueGenerator.Name(type), scope = serverScope + "." + name;
            Claim(language, serverScope, name, "value/" + type.CsName, "type");
            Reserve(language, scope, name, "Snapshot", "Borrow", "_value", "_owner");
            foreach (ConfigMember member in members) Claim(language, scope, member.Name, "value/" + type.CsName + "/field/" + member.Name, "property");
        }
    }
    private void PlanCSharpHelpers(TableSchema table)
    {
        FieldSchema[] references=table.Fields.Where(f=>f.Type.ReferenceTable!=null).ToArray();
        if(references.Length==0) return;
        // Only plan helpers which are emitted. A retired TryGet<Field> alias
        // would collide with the real TryGetCached<Field> role when another
        // reference is named Cached<Field>.
        var generated=references.SelectMany(f=>new[]{"ZrReferenceId"+f.Name,"_zrCached"+f.Name,"Get"+f.Name+"Async","ZrLoad"+f.Name+"Async"})
            .Concat(new[]{"_referencesResolved","_referenceResolver","ResolveReferences","BindReferences"}).ToHashSet(StringComparer.Ordinal);
        foreach(string role in generated)
        {
            string candidate=role;
            for(int suffix=1;candidate==table.Name+"Config" || table.Fields.Any(f=>f.Name==candidate) || candidate!=role&&generated.Contains(candidate);suffix++) candidate=role+suffix;
            _csharpHelpers.Add((table.Name,role),candidate);
        }
        var used=table.Fields.Select(f=>f.Name).Append(table.Name+"Config").ToHashSet(StringComparer.Ordinal);
        foreach(FieldSchema field in references) foreach(string role in new[]{"ZrReferenceId"+field.Name,"_zrCached"+field.Name,"Get"+field.Name+"Async","ZrLoad"+field.Name+"Async"}) used.Add(CSharpHelper(table.Name,role));
        foreach(FieldSchema field in references)
        {
            string role="TryGetCached"+field.Name,candidate=role;
            for(int suffix=1;!used.Add(candidate);suffix++) candidate=role+suffix;
            _csharpHelpers.Add((table.Name,role),candidate);
        }
    }
    private void Native(PortableTable[] tables, string language)
    {
        Dictionary<string, PortableType> types = tables.SelectMany(t => t.Types).DistinctBy(t => t.Id).ToDictionary(t => t.Id, StringComparer.Ordinal);
        string module = language is "java" or "go" ? "zrconfig" : language;
        string folder=language switch { "java"=>"Java", "go"=>"Go", "cpp"=>"Cpp", "rust"=>"Rust", "typescript"=>"TypeScript", _=>"Lua" };
        string[] files=language switch {
            "java"=>["ConfigRuntime.java","ConfigTypes.java","ConfigSnapshot.java","ServerConfig.java","SelectedSnapshot.java","DataSnapshot.java"],
            "go"=>["runtime.go","types.go","snapshot.go","server.go","data.go"],
            "cpp"=>["runtime.hpp","types.hpp","snapshot.hpp","server.hpp","data.hpp"],
            "rust"=>["runtime.rs","types.rs","snapshot.rs","server.rs","data.rs","lib.rs","Cargo.toml"],
            "typescript"=>["runtime.ts","types.ts","snapshot.ts","server.ts","data.ts","tsconfig.json"],
            _=>["runtime.lua","types.lua","snapshot.lua","server.lua","data.lua"] };
        foreach(string file in files.Where(file=>ServerApi || !(file.StartsWith("server.",StringComparison.Ordinal)||file.StartsWith("data.",StringComparison.Ordinal)||file is "ServerConfig.java" or "SelectedSnapshot.java" or "DataSnapshot.java"))) PlanPath(language,folder+"/"+file,"$generated/"+language+"/"+file);
        Reserve(language, module, "ConfigSnapshot", "ConfigManager", "ConfigRuntime");
        if (language == "go") Reserve(language, module, "DataSnapshot", "DataManager", "LoadData", "LoadPlan", "LoadMode", "Catalog", "SelectionPlan", "ConfigError", "ServerManager", "ErrorCode", "ResourceLimits", "ConfigSource", "MemoryConfigSource", "FileConfigSource", "SnapshotDiagnostics", "PreparationOptions", "ManagerPhase", "TableDescriptor", "ManagerStatus", "SnapshotIdentity", "ManagerEvent", "EventDiagnostics", "ManagerPolicy", "PreparedSnapshot", "CommitResult", "Decimal", "List", "Dictionary", "Optional", "Matrix", "DataWarmup", "LoadDataSource", "LoadDataWithWarmup", "LoadDataSourceWithWarmup", "NewServerManager", "NewFileConfigSource", "NewMemoryConfigSource", "SnapshotMemory", "NewSnapshotMemory", "StageTiming");
        if (ServerApi && language == "java") Reserve(language, module, "DataSnapshot", "ServerConfig", "SelectedSnapshot");
        if(language=="go"&&tables.Length>NativeGenerator.GoPartitionThreshold)
        {
            if(ServerApi)Reserve(language,module,"zrDataWarmup");
            for(int i=0;i<(tables.Length+NativeGenerator.GoPartSize-1)/NativeGenerator.GoPartSize;i++)
            {
                string suffix=i.ToString(System.Globalization.CultureInfo.InvariantCulture);
                Reserve(language,module,"zrConfigSnapshotPart"+suffix);Reserve(language,module+".ConfigSnapshot","part_"+suffix);
                if(ServerApi){Reserve(language,module,"zrDataSnapshotPart"+suffix);Reserve(language,module+".DataSnapshot","part_"+suffix);}
                foreach(string prefix in ServerApi?new[]{"zrConfigLoadPart","zrConfigBindPart","zrServerRetainPart","zrSelectedLoadPart","zrSelectedBindPart","zrDataOpenPart","zrDataValidatePart","zrDataLoadAllPart","zrDataLoadedCountPart","zrDataCompletedCountPart","zrDataWarmupPart"}:new[]{"zrConfigLoadPart","zrConfigBindPart"})
                    Reserve(language,module,prefix+suffix);
            }
        }
        if(language=="java"&&tables.Length>NativeGenerator.JavaPartitionThreshold)
        {
            for(int i=0;i<(tables.Length+NativeGenerator.JavaCatalogPartSize-1)/NativeGenerator.JavaCatalogPartSize;i++)
                foreach(string prefix in ServerApi?new[]{"ZrConfigSnapshotPart","ZrSelectedSnapshotPart","ZrDataSnapshotPart"}:new[]{"ZrConfigSnapshotPart"})
                {
                    string helper=prefix+i.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    Reserve(language,module,helper);PlanPath(language,folder+"/"+helper+".java","$generated/java/"+helper);
                }
            if(ServerApi)foreach(var table in tables)
            {
                string helper="ZrDataTableStorage_"+table.Name;
                Reserve(language,module,helper);PlanPath(language,folder+"/"+helper+".java","$generated/java/"+helper);
            }
        }
        foreach (PortableType type in types.Values.Where(t => t.Kind is "enum" or "object" or "converter" or "kv" or "vector2" or "vector3"))
        {
            string name = TypeName(type.Id);
            Claim(language, module, name, "type/" + type.Id, "type");
            if (language is "go" or "rust" or "typescript" && type.Variants.Length != 0 && !type.Abstract)
                Claim(language,module,name+"Value","type/"+type.Id+"/body","type");
            if(language=="go")foreach(string member in type.EnumMembers.Keys)
                Claim(language,module,name+"_"+TypeName(member),"type/"+type.Id+"/enum/"+member,"constant");
            if(language is "go" or "cpp")foreach(string helper in type.Kind=="enum"?Array.Empty<string>():new[]{"read_","body_"})
                Claim(language,module,helper+name,"type/"+type.Id+"/helper/"+helper,"private");
            string scope = module + ".types." + name;
            if (language == "java") Reserve(language, scope, "getClass", "wait", "notify", "notifyAll");
            if (language == "cpp") Reserve(language, scope, name);
            foreach (PortableField field in type.Fields)
            {
                Claim(language, scope, (language == "java" ? "get" : language == "rust" ? "get_" : "") + Accessor(field.Name, language), "type/" + type.Id + "/field/" + field.Name);
                if(language=="cpp")Claim(language,scope,"v_"+field.Name,"type/"+type.Id+"/storage/"+field.Name,"private");
            }
            foreach (string member in type.EnumMembers.Keys) Claim(language, scope + "/enum", TypeName(member), "type/" + type.Id + "/enum/" + member, "constant");
        }
        string snapshotScope=module+".ConfigSnapshot";
        Reserve(language,snapshotScope,"ConfigSnapshot","version","version_","load","Load","ConfigRuntime","ConfigTypes");
        if(language=="go")Reserve(language,snapshotScope,"Version");
        if(language=="typescript")Reserve(language,snapshotScope,"constructor","__proto__","prototype");
        string dataScope=language=="java"?module+".DataSnapshot":language=="go"?module:module+".data";
        if(ServerApi && language!="go") Reserve(language,dataScope,"Snapshot","Manager","Warmup");
        if(language=="rust"&&tables.Length>NativeGenerator.RustPartitionThreshold)
        for(int index=0;index<(tables.Length+NativeGenerator.RustPartSize-1)/NativeGenerator.RustPartSize;index++)
        {
            string suffix=index.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Reserve(language,module+".snapshot","ConfigSnapshotPart"+suffix,"ConfigSnapshotPart"+suffix+"Access");
            Reserve(language,snapshotScope,"part_"+suffix);
            if(ServerApi){Reserve(language,dataScope,"SnapshotPart"+suffix,"SnapshotPart"+suffix+"Access");Reserve(language,dataScope+".Snapshot","part_"+suffix,"table_part");}
        }
        foreach (PortableTable table in tables)
        {
            TablePath(table.Name,language);
            string source = "table/" + table.Name;
            if(language=="cpp"&&(Regex.IsMatch(table.Name,@"^_[A-Z_]")||table.Name.Contains("__",StringComparison.Ordinal)))
                throw new NotSupportedException("C++ 实现保留标识符不能用作生成表名："+table.Name);
            Claim(language, module, table.Name + "Config", source + "/legacy/row", "type");
            string dataModule = language == "java" ? module + ".DataSnapshot" : language == "go" ? module : module + ".data";
            if (ServerApi) foreach (string suffix in language == "go" ? new[] { "Row", "Rows" } : language is "cpp" or "rust" ? new[] { "Row", "Data" } : language == "lua" ? Array.Empty<string>() : new[] { "Row", "Table" })
                Claim(language, dataModule, table.Name + suffix, source + "/server/" + suffix, "type");
            // Aggregate methods and warmup factories share scopes across all tables.
            Claim(language,snapshotScope,Accessor(table.Name,language),source+"/legacy/snapshot");
            if(ServerApi)
            {
                string aggregate=language=="go"?module+".DataSnapshot":language=="java"?dataScope:dataScope+".Snapshot";
                Claim(language,aggregate,(language=="go"?"Get_":"get_")+table.Name,source+"/server/access");
                string warmup=language switch { "go"=>"Warmup", "java" or "typescript"=>"warmup", _=>"warmup_" };
                string scope=language=="go"?module:language=="lua"?dataScope:aggregate;
                foreach(string kind in new[]{"Row_","Table_"})Claim(language,scope,warmup+(language is "cpp" or "rust" or "lua"?kind.ToLowerInvariant():kind)+table.Name,source+"/warmup/"+kind);
                foreach(var group in table.Groups)Claim(language,scope,warmup+(language is "cpp" or "rust" or "lua"?"group_":"Group_")+table.Name+"_"+group.Name,source+"/warmup/group/"+group.Name);
                string tableScope=dataScope+"."+table.Name+(language=="go"?"Rows":language is "cpp" or "rust"?"Storage":"Table");
                Reserve(language,tableScope,"Count","count","LoadedCount","loadedCount","loaded_count","Get","get","Find","find","Load","load","TryLoad","tryLoad","try_load","Rows","rows","LoadAll","loadAll","load_all","storage","keyIndex","full","new");
                foreach(var group in table.Groups)
                    foreach(string prefix in language=="go"?new[]{"FindGroup_","LoadGroup_","index_","buildIndex_","group_"}:language is "cpp" or "rust"?new[]{"find_group_","load_group_","ensure_","group_"}:language=="lua"?new[]{"find_group_","load_group_","group_"}:new[]{"findGroup_","loadGroup_","index_","group_"})
                        Claim(language,tableScope,prefix+group.Name,source+"/group/"+group.Name+"/"+prefix,prefix is "index_" or "buildIndex_" or "ensure_" or "group_"?"private":"member");
                if(language=="typescript")foreach(var group in table.Groups)
                    Claim(language,tableScope,"loadGroup_"+group.Name+"Async",source+"/group/"+group.Name+"/loadGroupAsync","member");
            }
            if(language=="go")
            {
                Claim(language,module,table.Name+"Table",source+"/legacy/table","type");
                Claim(language,module,"schema"+table.Name,source+"/legacy/schema","private");
                Claim(language,module,"load"+table.Name,source+"/legacy/load","private");
                Claim(language,module,table.Name+"Key_Key",source+"/key","type");
                foreach(var group in table.Groups)Claim(language,module,table.Name+"Key_Group_"+group.Name,source+"/group-key/"+group.Name,"type");
            }
            string row = dataModule + "." + table.Name + "Row", legacy = module + "." + table.Name + "Config";
            if(language=="cpp")Reserve(language,legacy,table.Name+"Config");
            if(language=="java")Reserve(language,legacy,"getClass","wait","notify","notifyAll");
            if(language=="typescript")Reserve(language,legacy,"constructor","__proto__","prototype");
            if (ServerApi) switch (language)
            {
                case "java": Reserve(language, row, "getClass", "wait", "notify", "notifyAll", table.Name + "Row", "owner"); break;
                case "cpp": Reserve(language, row, table.Name + "Row", "borrow", "select"); break;
                case "typescript": Reserve(language, row, "constructor", "__proto__", "prototype"); break;
                case "go": Reserve(language, row, "owner"); break;
            }
            foreach (PortableField field in table.Fields)
            {
                bool reference = types[field.Type].Kind == "tablereference";
                string access = Accessor(field.Name, language);
                string getter = language == "java" ? "get" + access : language == "rust" ? "get_" + access : access;
                Claim(language, legacy, getter, source + "/field/" + field.Name);
                if (reference) Claim(language, legacy, getter + "Id", source + "/field/" + field.Name + "/id");
                if (ServerApi) Claim(language, row, getter + (reference ? "Id" : ""), source + "/field/" + field.Name + (reference ? "/id" : ""));
                if (ServerApi && reference)
                    foreach (string prefix in language == "go" ? new[] { "Get_", "Load_" } : new[] { "get_", "load_" })
                        Claim(language, row, prefix + field.Name, source + "/field/" + field.Name + "/" + prefix);
                if (language == "cpp")
                {
                    Claim(language, legacy, "v_" + field.Name, source + "/storage/" + field.Name, "private");
                    if (reference) Claim(language, legacy, "ref_" + field.Name, source + "/reference/" + field.Name, "private");
                }
            }
        }
    }
    private static readonly HashSet<string> CppKeywords = new(("alignas alignof and and_eq asm atomic_cancel atomic_commit atomic_noexcept auto bitand bitor bool break case catch char char16_t char32_t class compl concept const constexpr const_cast continue co_await co_return co_yield decltype default delete do double dynamic_cast else enum explicit export extern false float for friend goto if inline int long mutable namespace new noexcept not not_eq nullptr operator or or_eq private protected public register reinterpret_cast requires return short signed sizeof static static_assert static_cast struct switch synchronized template this thread_local throw true try typedef typeid typename union unsigned using virtual void volatile wchar_t while xor xor_eq").Split(' '), StringComparer.Ordinal);
}
