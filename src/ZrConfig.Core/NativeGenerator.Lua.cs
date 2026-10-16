using System.Text;

namespace ZrConfig.Core;

internal sealed partial class NativeGenerator
{
    // 长括号保留说明、Unicode 和反斜线，不把 JSON 的转义规则误当成 Lua 字符串规则。
    private string LuaString(string value)
    {
        string equals = "=";
        while (value.Contains("]" + equals + "]", StringComparison.Ordinal)) equals += "=";
        return "([" + equals + "[" + value + "]" + equals + "])";
    }
    private void LuaComment(StringBuilder text, string description, string indent = "")
    {
        if (string.IsNullOrWhiteSpace(description)) return;
        foreach (string line in description.ReplaceLineEndings("\n").Split('\n')) text.AppendLine(indent + "-- " + line);
    }
    private void Lua(PortableTable[] tables, Dictionary<string, string> files)
    {
        var types = Types(tables);
        string LType(string id)
        {
            var t = types[id];
            return t.Kind switch
            {
                "primitive" or "tablereference" => t.Primitive switch { "string" or "uint64" or "decimal" => "string", "bool" => "boolean", "float32" or "float64" => "number", _ => "integer" },
                "rawjson" => "string", "nullable" => LType(t.Element!) + "|ZrConfigNull",
                "array" => t.Rank == 1 ? "(" + LType(t.Element!) + ")[]" : "ZrConfigMatrix",
                "dictionary" => "ZrConfigDictionary",
                _ => Name(id) + (t.NullableObject ? "|ZrConfigNull" : "")
            };
        }
        const string Header = "-- 此文件由工具生成，请勿手工修改。\nlocal prefix = (...):match('^(.*%.)') or ''\nlocal R = require(prefix .. 'runtime')\n";
        files.Add("Lua/runtime.lua", Runtime("lua"));
        var definitions = new StringBuilder(Header + "---@class ZrConfigNull\n---@class ZrConfigMatrix\n---@field dimensions integer[]\n---@field values table\n---@class ZrConfigDictionary\n---@field size integer\n---@field entries table\n---@field get fun(key:any):any\nlocal types = {}\n");
        foreach (var t in types.Values.OrderBy(t => t.Id, StringComparer.Ordinal))
        {
            if (t.Kind == "enum")
            {
                definitions.AppendLine($"---@alias {Name(t.Id)} {(t.Primitive == "uint64" ? "string" : "integer")}\ntypes.{Name(t.Id)} = {{");
                foreach (var member in t.EnumMembers) definitions.AppendLine($"    {Name(member.Key)} = {(t.Primitive == "uint64" ? LuaString(member.Value) : member.Value)},");
                definitions.AppendLine("}");
            }
            else if (t.Kind is "object" or "converter" or "kv" or "vector2" or "vector3")
            {
                definitions.AppendLine($"---@class {Name(t.Id)}");
                foreach (var f in t.Fields) { LuaComment(definitions, f.Description); definitions.AppendLine($"---@field {f.Name} {LType(f.Type)}"); }
            }
        }
        definitions.AppendLine("return R.readonly(types)");
        files.Add("Lua/types.lua", definitions.ToString());
        var snapshot = new StringBuilder(Header + "local factories = {}\n");
        foreach (var t in tables) snapshot.AppendLine($"factories[{LuaString(t.Name)}] = require(prefix .. {LuaString(t.Name + "Config")})");
        snapshot.AppendLine("local M = {}\n-- 加载不调用协程让出；同一 Lua 状态内完整候选构建成功后一次替换引用。\nfunction M.load(root, prefer_json)\n    local bundle = R.bundle(root)\n    local snapshot = {version=bundle.version}\n");
        snapshot.AppendLine($"    bundle.validate_tables({{{string.Join(", ", tables.Select(t => LuaString(t.Name)))}}})");
        foreach (var t in tables) snapshot.AppendLine($"    snapshot[{LuaString(t.Name)}] = factories[{LuaString(t.Name)}](bundle, prefer_json)");
        foreach (var t in tables) snapshot.AppendLine($"    snapshot[{LuaString(t.Name)}].bind(snapshot)");
        snapshot.AppendLine("    local seen = {}");
        foreach (var t in tables) snapshot.AppendLine($"    snapshot[{LuaString(t.Name)}].seal(seen)");
        snapshot.AppendLine("    return R.readonly(snapshot, seen)\nend\nfunction M.manager()\n    local current\n    return R.readonly({\n        current=function() return current or R.invalid('配置尚未加载') end,\n        reload=function(root, prefer_json)\n            local next_snapshot=M.load(root, prefer_json)\n            current=next_snapshot\n            return next_snapshot\n        end\n    })\nend\nM.NULL=R.NULL\nM.decimal=R.decimal\nreturn R.readonly(M)");
        files.Add("Lua/snapshot.lua", snapshot.ToString());
        var server = new StringBuilder(Header + "local legacy=require(prefix..'snapshot')\nlocal M={}\nfunction M.load(root,prefer_json) local snapshot=legacy.load(root,prefer_json);R.retain_owner(snapshot,snapshot);return snapshot end\nM.manager=R.server_manager\nM.cancellation=R.cancellation\n");
        server.AppendLine($"M.catalog=R.catalog({LuaString(RuntimeCatalog.Create(tables,_names.Target).Serialize())})\nlocal factories={{}}");
        foreach (var t in tables) server.AppendLine($"factories[{LuaString(t.Name)}]=require(prefix..{LuaString(t.Name + "Config")})");
        server.AppendLine("function M.select(root,requested,prefer_json)\n    local plan=M.catalog.select(requested);local bundle=R.bundle(root);bundle.validate_catalog(M.catalog)\n    local storage={version=bundle.version};local result={version=bundle.version,selection=plan}\n    for _,name in ipairs(plan.tables)do storage[name]=factories[name](bundle,prefer_json)end\n    for _,name in ipairs(plan.tables)do storage[name].bind(storage)end\n    local seen={}\n    for _,name in ipairs(plan.tables)do storage[name].seal(seen)end\n    storage=R.readonly(storage,seen)");
        foreach (var t in tables) server.AppendLine($"    function result.get_{t.Name}()return storage[{LuaString(t.Name)}] or R.server_error('TableNotSelected',{LuaString(t.Name)})end");
        server.AppendLine("    result=R.readonly(result);R.retain_owner(storage,result);return result\nend\nreturn R.readonly(M)");
        if (_names.ServerApi) files.Add("Lua/server.lua", server.ToString());
        if (_names.ServerApi) LuaData(tables,files);
        foreach (var table in tables)
        {
            var text = new StringBuilder(Header + $"---@class {table.Name}Config\n");
            foreach (var f in table.Fields)
            {
                LuaComment(text, f.Description);
                if (types[f.Type].Kind == "tablereference") text.AppendLine($"---@field {f.Name} {ReferenceTable(f.Type)}Config|ZrConfigNull\n---@field {f.Name}Id integer");
                else text.AppendLine($"---@field {f.Name} {LType(f.Type)}");
            }
            text.AppendLine($"local schema = {LuaString(table.Serialize())}\nreturn function(bundle, prefer_json)\n    local rows = bundle.load(schema, prefer_json)\n    local result = {{rows=rows, count=#rows}}\n");
            var indexes = new List<(string Name, string[] Fields, string? Group)> { ("Key", table.Keys, null) };
            indexes.AddRange(table.Groups.Select(g => ("Group_" + g.Name, g.Fields, (string?)g.Name)));
            foreach (var ix in indexes) text.AppendLine($"    local index_{ix.Name} = R.index(rows, {{{string.Join(", ", ix.Fields.Select(LuaString))}}}, {(ix.Group == null ? "true" : "false")})");
            bool referenced = tables.Any(t => t.Fields.Any(f => types[f.Type].Kind == "tablereference" && ReferenceTable(f.Type) == table.Name));
            if (referenced) text.AppendLine("    local ids = {}\n    for _,row in ipairs(rows) do if ids[row.Id] then R.invalid('引用 Id 不唯一') end; ids[row.Id]=row end\n    function result.byId(id) return ids[id] end");
            foreach (var f in table.Fields.Where(f => types[f.Type].Kind == "tablereference")) text.AppendLine($"    for _,row in ipairs(rows) do row[{LuaString(f.Name + "Id")}] = row[{LuaString(f.Name)}]; row[{LuaString(f.Name)}]=R.NULL end");
            text.AppendLine("    function result.bind(snapshot)");
            foreach (var f in table.Fields.Where(f => types[f.Type].Kind == "tablereference")) text.AppendLine($"        for _,row in ipairs(rows) do if row[{LuaString(f.Name + "Id")}] ~= 0 then row[{LuaString(f.Name)}] = snapshot[{LuaString(ReferenceTable(f.Type))}].byId(row[{LuaString(f.Name + "Id")}]) or R.invalid({LuaString("引用目标不存在：" + table.Name + "." + f.Name)}) end end");
            text.AppendLine("    end\n    function result.seal(seen)\n        rows=R.readonly(rows, seen); result.rows=rows");
            foreach (var ix in indexes) text.AppendLine($"        index_{ix.Name}=R.readonly(index_{ix.Name}, seen)");
            if (referenced) text.AppendLine("        ids=R.readonly(ids, seen)");
            text.AppendLine("        result.bind=nil; result.seal=nil\n    end");
            foreach (var ix in indexes)
            {
                string args = string.Join(", ", ix.Fields.Select((_, i) => "k" + i));
                string method = ix.Group == null ? "find" : "get" + ix.Group;
                text.AppendLine($"    function result.{method}({args})\n        local node = index_{ix.Name}");
                for (int i = 0; i < ix.Fields.Length; i++) text.AppendLine($"        node=node[k{i}]; if node==nil then return {(ix.Group == null ? "nil" : "R.empty")} end");
                text.AppendLine("        return node\n    end");
                if (ix.Group == null) text.AppendLine($"    function result.get({args}) return result.find({args}) or R.invalid('配置主键不存在') end");
            }
            text.AppendLine("    return result\nend");
            files.Add(_names.TablePath(table.Name, "lua"), text.ToString());
        }
    }

    private void LuaData(PortableTable[] tables,Dictionary<string,string> files)
    {
        var types=Types(tables);
        bool partitioned=tables.Length>256;
        var text=new StringBuilder("-- Generated server views; dot-call queries and explicit loads.\nlocal prefix=(...):match('^(.*%.)')or''\nlocal R=require(prefix..'runtime')\nlocal catalog=require(prefix..'server').catalog\nlocal M={}\nM.manager=R.server_manager\nM.cancellation=R.cancellation\nfunction M.load(root,plan,cancellation)return M.load_source(R.file_source(root),plan,cancellation)end\nfunction M.load_source(source,plan,cancellation)\n    plan=plan or{};cancellation=cancellation or R.cancellation()\n    local allowed={mode=true,tables=true,prefer_json=true,warmup_tables=true,limits=true,source_namespace=true,require_verified_paths=true};for name in pairs(plan)do if not allowed[name]then R.server_error('UnsupportedCapability')end end\n    if plan.prefer_json~=nil and type(plan.prefer_json)~='boolean'then R.server_error('InvalidPlan')end\n    local mode=plan.mode or'ServerDefault'\n    if mode~='ServerDefault'and mode~='SelectedTables'and mode~='LazyRowsStrict'and mode~='LazyRowsOnAccess'then R.server_error('InvalidPlan')end\n    local selection=catalog.select(plan.tables);local selected={}\n    for _,name in ipairs(selection.tables)do selected[name]=true end\n    local warmup={};for _,name in ipairs(plan.warmup_tables or{})do if not selected[name]then R.server_error('InvalidPlan')end;warmup[#warmup+1]=name end\n    if plan.require_verified_paths~=nil and type(plan.require_verified_paths)~='boolean'then R.server_error('InvalidPlan')end;if plan.source_namespace~=nil and type(plan.source_namespace)~='string'then R.server_error('InvalidPlan')end\n    cancellation.check();local bundle=R.source_bundle(source,plan,cancellation);bundle.validate_catalog(catalog)\n    local indexed,storage,tables={},{},{};local result={catalog_fingerprint=selection.catalog_fingerprint,load_plan_fingerprint=R.load_plan_fingerprint(mode,plan.prefer_json==true,selection.tables),version=bundle.version,manifest_digest=bundle.manifest_digest,source_namespace=plan.source_namespace or source.source_namespace or'',diagnostics=bundle.budget.diagnostics,selection=selection,mode=mode}\n");
        foreach(var table in tables)text.AppendLine($"    if selected[{LuaString(table.Name)}]then cancellation.check();indexed[{LuaString(table.Name)}]=bundle.open_rows({LuaString(table.Serialize())},plan.prefer_json==true)end");
        text.AppendLine("    for _,data in pairs(indexed)do data.bind_references(function(name)return indexed[name]end)end\n    if mode~='LazyRowsOnAccess'then\n        for _,name in ipairs(selection.tables)do local data=indexed[name];for i=1,data.count do cancellation.check();data.validate(data.read_transient(i),function(target)return indexed[target]end)end end\n    end");
        var loader=text;
        if(partitioned){text.Replace("local M={}\n","local M={}\nlocal builders={}\n");text=new StringBuilder();}
        for(int tableIndex=0;tableIndex<tables.Length;tableIndex++)
        {
            var table=tables[tableIndex];
            if(partitioned&&tableIndex%128==0)text.AppendLine($"builders[{tableIndex/128+1}]=function(indexed,storage,tables,result,selected,owner)");
            string key=LuaString(table.Name);
            text.AppendLine($"    if selected[{key}]then\n        local data=indexed[{key}]\n        local store=R.row_storage(data,function(value)");
            foreach(var field in table.Fields.Where(f=>types[f.Type].Kind=="tablereference"))
            {
                string target=LuaString(types[field.Type].ReferenceTable!);
                text.AppendLine($"            local id_{field.Name}=value[{LuaString(field.Name)}];value[{LuaString(field.Name+"Id")}]=id_{field.Name};value[{LuaString(field.Name)}]=nil\n            value.get_{field.Name}=function()return storage[{target}].reference(id_{field.Name},false)end\n            value.load_{field.Name}=function(token)return storage[{target}].reference(id_{field.Name},true,token)end");
            }
            text.AppendLine($"            return value\n        end,function()return result end)\n        storage[{key}]=store\n        local view={{count=data.count,loaded_count=store.loaded_count,rows=store.all,load_all=store.load_all}}\n");
            string args=string.Join(",",table.Keys.Select((_,i)=>"k"+i));
            text.AppendLine($"        local function ordinal({args})local node=store.keys");
            for(int i=0;i<table.Keys.Length;i++)text.AppendLine($"            node=node[k{i}];if node==nil then return nil end");
            text.AppendLine("            return node\n        end");
            text.AppendLine($"        function view.find({args})return store.get(ordinal({args}))end\n        function view.get({args})return view.find({args})or R.server_error('KeyNotFound')end\n        function view.try_load({args},token)return store.load(ordinal({args}),token)end\n        function view.load({args},token)return view.try_load({args},token)or R.server_error('KeyNotFound')end");
            foreach(var group in table.Groups)
            {
                string gargs=string.Join(",",group.Fields.Select((_,i)=>"k"+i));
                text.AppendLine($"        local function group_{group.Name}({gargs},load,token)local node=store.group_index({LuaString(group.Name)},load,token)");
                for(int i=0;i<group.Fields.Length;i++)text.AppendLine($"            node=node[k{i}];if node==nil then return R.empty end");
                text.AppendLine("            return store.group(node,load,token)\n        end");
                text.AppendLine($"        function view.find_group_{group.Name}({gargs})return group_{group.Name}({gargs},false)end\n        function view.load_group_{group.Name}({gargs},token)return group_{group.Name}({gargs},true,token)end");
            }
            text.AppendLine($"        tables[{key}]=R.readonly(view)\n    end\n    function result.get_{table.Name}()return tables[{key}]or R.server_error('TableNotSelected')end");
            if(partitioned&&(tableIndex%128==127||tableIndex==tables.Length-1))text.AppendLine("end");
        }
        string? builders=null;
        if(partitioned)
        {
            builders=text.ToString().Replace("end,function()return result end)","end,owner)",StringComparison.Ordinal);text=loader;
            text.AppendLine($"    local function owner()return result end\n    for i=1,{(tables.Length+127)/128} do builders[i](indexed,storage,tables,result,selected,owner)end");
        }
        text.AppendLine("    function result.load_all(token)for _,name in ipairs(selection.tables)do storage[name].load_all(token)end end\n    function result.completed_table_count()local total=0;for _,name in ipairs(selection.tables)do if storage[name].is_complete()then total=total+1 end end;return total end\n    function result.loaded_row_count()local total=0;for _,name in ipairs(selection.tables)do total=total+storage[name].loaded_count()end;return total end\n    if mode=='ServerDefault'or mode=='SelectedTables'then result.load_all(cancellation)end\n    for _,name in ipairs(warmup)do storage[name].load_all(cancellation)end\n    cancellation.check();result=R.readonly(result);R.retain_owner(tables,result);return result\nend\nreturn R.readonly(M)");
        text.Replace("return R.readonly(M)", "");
        if(builders!=null)text.Append(builders);
        text.AppendLine("local warmup_states=setmetatable({},{__mode='k'})\nlocal function warmup(table_name,action)local value=R.readonly({table=table_name});warmup_states[value]={table=table_name,action=action};return value end\nfunction M.load_source_with_warmup(source,plan,requests,cancellation)\n    plan=plan or{};cancellation=cancellation or R.cancellation();if type(requests)~='table'then R.server_error('InvalidPlan')end\n    local selection=catalog.select(plan.tables);local selected={};for _,name in ipairs(selection.tables)do selected[name]=true end;local steps={}\n    for i,request in ipairs(requests)do local step=warmup_states[request];if not step or not selected[step.table]then R.server_error('InvalidPlan','Warmup outside selection')end;steps[i]=step end\n    local snapshot=M.load_source(source,plan,cancellation);for _,step in ipairs(steps)do cancellation.check();step.action(snapshot,cancellation)end;cancellation.check();return snapshot\nend\nfunction M.load_with_warmup(root,plan,requests,cancellation)return M.load_source_with_warmup(R.file_source(root),plan,requests,cancellation)end");
        foreach(var table in tables)
        {
            text.AppendLine($"function M.warmup_table_{table.Name}()return warmup({LuaString(table.Name)},function(snapshot,token)snapshot.get_{table.Name}().load_all(token)end)end");
            foreach(var item in new[]{(Name:"row_"+table.Name,Fields:table.Keys,Method:"load")}.Concat(table.Groups.Select(g=>(Name:"group_"+table.Name+"_"+g.Name,Fields:g.Fields,Method:"load_group_"+g.Name))))
            {
                string args=string.Join(",",item.Fields.Select((_,i)=>"k"+i));
                text.AppendLine($"function M.warmup_{item.Name}({args})return warmup({LuaString(table.Name)},function(snapshot,token)snapshot.get_{table.Name}().{item.Method}({args},token)end)end");
            }
        }
        text.AppendLine("return R.readonly(M)");
        files.Add("Lua/data.lua",text.ToString());
    }
}
