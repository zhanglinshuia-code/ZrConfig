using System.Text;

namespace ZrConfig.Core;

internal sealed partial class NativeGenerator
{
    // Debug builds retain constructor temporaries. Bound each frame as well as
    // the snapshot itself so loading works with a 1 MiB thread stack.
    internal const int RustPartSize = 32;
    internal const int RustPartitionThreshold = RustPartSize;
    private Dictionary<string,string> _rustTablePaths = new(StringComparer.Ordinal);
    private string RustMember(string table) => _rustTablePaths[table];
    private void Rust(PortableTable[] tables, Dictionary<string, string> files)
    {
        bool partitioned=tables.Length>RustPartitionThreshold;
        // Keep large-catalog cold construction bounded without changing hot getters.
        string cold=partitioned?"    #[inline(never)]\n":"";
        var parts=tables.Chunk(RustPartSize).ToArray();
        _rustTablePaths=tables.Select((table,index)=>(table.Name,Path:(partitioned?"part_"+(index/RustPartSize)+"().":"")+"table_"+table.Name)).ToDictionary(item=>item.Name,item=>item.Path,StringComparer.Ordinal);
        var types = Types(tables);
        string Primitive(string name) => name switch { "string" => "String", "bool" => "bool", "decimal" => "R::Decimal", "float32" => "f32", "float64" => "f64", _ => name.Replace("uint", "u").Replace("int", "i") };
        string TypeName(string id)
        {
            var t = types[id];
            return t.Kind switch
            {
                "primitive" or "tablereference" => Primitive(t.Primitive), "rawjson" => "String",
                "nullable" => $"Option<{TypeName(t.Element!)}>",
                "array" => t.Rank == 1 ? $"Vec<{TypeName(t.Element!)}>" : $"R::Matrix<{TypeName(t.Element!)}>",
                "dictionary" => $"R::Dictionary<{TypeName(t.Key!)}, {TypeName(t.Element!)}>",
                _ => t.NullableObject ? $"Option<Box<T::{Name(id)}>>" : "T::" + Name(id)
            };
        }
        string Read(string id, string value, int depth = 0)
        {
            var t = types[id]; string item = "item" + depth;
            string Function(string child) => $"|{item}| {Read(child, item, depth + 1)}";
            return t.Kind switch
            {
                "primitive" or "tablereference" => $"R::scalar::<{Primitive(t.Primitive)}>({value})",
                "rawjson" => $"R::scalar::<String>({value})",
                "nullable" => $"R::decode_optional({value}, {Function(t.Element!)})",
                "array" => $"R::decode_{(t.Rank == 1 ? "list" : "matrix")}({value}, {Function(t.Element!)})",
                "dictionary" => $"R::decode_dictionary({value}, {Function(t.Key!)}, {Function(t.Element!)})",
                _ => t.NullableObject ? $"R::decode_optional({value}, |v| T::{Name(id)}::__zr_decode(v).map(Box::new))" : $"T::{Name(id)}::__zr_decode({value})"
            };
        }
        string Local(string code) => code.Replace("T::", "");
        bool Copy(PortableType t) => t.Kind == "enum" || t.Kind is "primitive" or "tablereference" && t.Primitive != "string";
        string ReturnType(PortableField f) => Copy(types[f.Type]) ? TypeName(f.Type) : TypeName(f.Type) == "String" ? "&str" : types[f.Type].Kind == "array" && types[f.Type].Rank == 1 ? $"&[{TypeName(types[f.Type].Element!)}]" : "&" + TypeName(f.Type);
        void Getter(StringBuilder code, PortableField f, bool local)
        {
            Comment(code, f.Description);
            string signature = $"    pub fn get_{f.Name}(&self) -> {ReturnType(f)} {{ {(Copy(types[f.Type]) ? "" : "&")}self.v_{f.Name} }}";
            code.AppendLine(local ? Local(signature) : signature);
        }
        files.Add("Rust/runtime.rs", Runtime("rs"));
        files.Add("Rust/Cargo.toml", "[package]\nname = \"zrconfig-generated\"\nversion = \"0.1.0\"\nedition = \"2021\"\nrust-version = \"1.70\"\n[lib]\npath = \"lib.rs\"\n");
        var lib = new StringBuilder("// 此文件由工具生成；所有权由 Arc 快照管理，查询不复制记录。\n#![allow(non_snake_case, non_camel_case_types, dead_code, unused_imports)]\npub mod runtime;\npub mod types;\npub mod snapshot;\npub use snapshot::{ConfigManager, ConfigSnapshot};\n");
        foreach (var table in tables) lib.AppendLine($"#[path = {Quote(table.Name + "Config.rs")}]\npub mod table_{table.Name};");
        if (_names.ServerApi) lib.AppendLine("pub mod server;\npub mod data;");
        files.Add("Rust/lib.rs", lib.ToString());
        if (_names.ServerApi) RustData(tables,files,TypeName,(id,value)=>Read(id,value),ReturnType,id=>Copy(types[id]));
        var definitions = new StringBuilder("// 此文件由工具生成；数据字段私有，只提供只读访问。\nuse super::runtime::{self as R, Json, Result};\n");
        foreach (var t in types.Values.OrderBy(t => t.Id, StringComparer.Ordinal))
        {
            string name = Name(t.Id);
            if (t.Kind == "enum")
            {
                string primitive = Primitive(t.Primitive);
                definitions.AppendLine($"#[derive(Clone, Copy, Debug, PartialEq, Eq, Hash)]\npub struct {name}({primitive});\nimpl {name} {{");
                foreach (var m in t.EnumMembers) definitions.AppendLine($"    #[allow(non_upper_case_globals)]\n    pub const {Name(m.Key)}: Self = Self({m.Value}{primitive});");
                definitions.AppendLine($"    pub fn value(self) -> {primitive} {{ self.0 }}\n    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {{\n        let value = R::scalar::<{primitive}>(v)?;\n        match value {{ {string.Join(" | ", t.EnumValues.Select(v => v + primitive))} => Ok(Self(value)), _ => R::invalid(\"未知枚举值\") }}\n    }}\n}}\nimpl R::Key for {name} {{ fn hash_key(&self, state: &mut std::collections::hash_map::DefaultHasher) {{ R::hash_part(state, &self.0); }} }}");
                continue;
            }
            if (t.Kind is not ("object" or "converter" or "kv" or "vector2" or "vector3")) continue;
            bool polymorphic = t.Variants.Length > 0;
            string body = name + (polymorphic ? "Value" : "");
            if (!t.Abstract)
            {
                definitions.AppendLine($"pub struct {body} {{");
                foreach (var f in t.Fields) definitions.AppendLine($"    v_{f.Name}: {Local(TypeName(f.Type))},");
                definitions.AppendLine($"}}\nimpl {body} {{");
                foreach (var f in t.Fields) Getter(definitions, f, true);
                definitions.AppendLine("    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {\n        Ok(Self {");
                foreach (var f in t.Fields) definitions.AppendLine($"            v_{f.Name}: {Local(Read(f.Type, "v.field(" + Quote(f.Name) + ")?"))}?,");
                definitions.AppendLine("        })\n    }\n}");
            }
            if (polymorphic)
            {
                definitions.AppendLine($"pub enum {name} {{");
                foreach (string variant in t.Variants) definitions.AppendLine($"    {Name(variant)}(Box<{Name(variant)}{(types[variant].Variants.Length > 0 ? "Value" : "")}>),");
                definitions.AppendLine($"}}\nimpl {name} {{\n    pub(super) fn __zr_decode(v: &Json) -> Result<Self> {{\n        match v.field(\"$type\")?.string()? {{");
                foreach (string variant in t.Variants) definitions.AppendLine($"            {Quote(variant)} => Ok(Self::{Name(variant)}(Box::new({Name(variant)}{(types[variant].Variants.Length > 0 ? "Value" : "")}::__zr_decode(v)?))),");
                definitions.AppendLine("            _ => R::invalid(\"未知多态类型\"),\n        }\n    }");
                foreach (var f in t.Fields)
                {
                    Comment(definitions, f.Description);
                    definitions.AppendLine($"    pub fn get_{f.Name}(&self) -> {Local(ReturnType(f))} {{ match self {{");
                    foreach (string variant in t.Variants) definitions.AppendLine($"        Self::{Name(variant)}(v) => v.get_{f.Name}(),");
                    definitions.AppendLine("    } }");
                }
                definitions.AppendLine("}");
            }
        }
        files.Add("Rust/types.rs", definitions.ToString());
        var snapshot = new StringBuilder("// 完整候选加载和引用校验成功后才发布。请求持有 Arc 快照，旧版本自然释放。\nuse std::path::Path;\nuse std::sync::{Arc, Mutex, RwLock};\nuse super::runtime::{self as R, Result};\n");
        foreach (var table in tables) snapshot.AppendLine($"use super::table_{table.Name}::{table.Name}Table;");
        if(partitioned)
        for(int index=0;index<parts.Length;index++)
        {
            snapshot.AppendLine($"pub(super) trait ConfigSnapshotPart{index}Access:Send+Sync{{fn storage(&self)->&ConfigSnapshotPart{index};}}\nimpl ConfigSnapshotPart{index}Access for ConfigSnapshotPart{index}{{fn storage(&self)->&ConfigSnapshotPart{index}{{self}}}}");
            snapshot.AppendLine($"pub(super) struct ConfigSnapshotPart{index} {{");
            foreach(var table in parts[index])snapshot.AppendLine($"    pub(super) table_{table.Name}:Option<{table.Name}Table>,");
            snapshot.AppendLine($"}}\nimpl ConfigSnapshotPart{index} {{\n    #[inline(never)]\n    fn load(bundle:&R::Bundle,prefer_json:bool,selected:Option<&std::collections::HashSet<&str>>)->Result<Box<dyn ConfigSnapshotPart{index}Access>>{{Ok(Box::new(Self{{");
            foreach(var table in parts[index])snapshot.AppendLine($"        table_{table.Name}:if selected.map_or(true,|names|names.contains({Quote(table.Name)})){{Some({table.Name}Table::__zr_load(bundle,prefer_json)?)}}else{{None}},");
            snapshot.AppendLine("    }))}\n    fn bind(&self,snapshot:&Arc<ConfigSnapshot>)->Result<()>{");
            foreach(var table in parts[index])snapshot.AppendLine($"        if let Some(table)=&self.table_{table.Name}{{table.__zr_bind(snapshot)?;}}");
            snapshot.AppendLine("        Ok(())\n    }\n}");
        }
        snapshot.AppendLine("pub struct ConfigSnapshot {\n    version: String,");
        if(partitioned)for(int index=0;index<parts.Length;index++)snapshot.AppendLine($"    pub(super) part_{index}:Box<dyn ConfigSnapshotPart{index}Access>,");
        else foreach (var table in tables) snapshot.AppendLine($"    pub(super) table_{table.Name}: Option<{table.Name}Table>,");
        snapshot.AppendLine("}\nimpl ConfigSnapshot {\n    pub fn version(&self) -> &str { &self.version }");
        // Erase only private storage ownership to bound drop-check dependencies.
        // A typed private trait returns its own storage without RTTI, downcasts or unsafe code.
        if(partitioned)for(int index=0;index<parts.Length;index++)snapshot.AppendLine($"    pub(super) fn part_{index}(&self)->&ConfigSnapshotPart{index}{{self.part_{index}.storage()}}");
        foreach (var table in tables) snapshot.AppendLine($"    pub fn get_{table.Name}(&self) -> &{table.Name}Table {{ self.{RustMember(table.Name)}.as_ref().expect(\"Legacy view requires the complete catalog\") }}");
        snapshot.AppendLine("    pub fn load(root: &Path, prefer_json: bool) -> Result<Arc<Self>> {\n        let bundle = R::Bundle::open(root)?;");
        snapshot.AppendLine($"        bundle.validate_tables(&[{string.Join(", ", tables.Select(t => Quote(t.Name)))}])?;");
        snapshot.AppendLine("        Self::__zr_load_selected(&bundle, prefer_json, None)\n    }\n    pub(super) fn __zr_load_selected(bundle: &R::Bundle, _prefer_json: bool, _selected: Option<&std::collections::HashSet<&str>>) -> Result<Arc<Self>> {");
        snapshot.AppendLine("        let next = Arc::new(Self {\n            version: bundle.version.clone(),");
        if(partitioned)for(int index=0;index<parts.Length;index++)snapshot.AppendLine($"            part_{index}:ConfigSnapshotPart{index}::load(bundle,_prefer_json,_selected)?,");
        else foreach (var table in tables) snapshot.AppendLine($"            table_{table.Name}: if _selected.map_or(true, |s| s.contains({Quote(table.Name)})) {{ Some({table.Name}Table::__zr_load(bundle, _prefer_json)?) }} else {{ None }},");
        snapshot.AppendLine("        });");
        if(partitioned)for(int index=0;index<parts.Length;index++)snapshot.AppendLine($"        next.part_{index}().bind(&next)?;");
        else foreach (var table in tables) snapshot.AppendLine($"        if let Some(table) = &next.{RustMember(table.Name)} {{ table.__zr_bind(&next)?; }}");
        snapshot.AppendLine("        Ok(next)\n    }\n}\n#[derive(Default)]\npub struct ConfigManager { gate: Mutex<()>, current: RwLock<Option<Arc<ConfigSnapshot>>> }\nimpl ConfigManager {\n    pub fn new() -> Self { Self::default() }\n    pub fn current(&self) -> Result<Arc<ConfigSnapshot>> { self.current.read().map_err(|e| e.to_string())?.clone().ok_or_else(|| \"配置尚未加载\".to_owned()) }\n    pub fn reload(&self, root: &Path, prefer_json: bool) -> Result<Arc<ConfigSnapshot>> {\n        let _guard = self.gate.lock().map_err(|e| e.to_string())?;\n        let next = ConfigSnapshot::load(root, prefer_json)?;\n        *self.current.write().map_err(|e| e.to_string())? = Some(next.clone());\n        Ok(next)\n    }\n}\n");
        files.Add("Rust/snapshot.rs", snapshot.ToString());
        var server = new StringBuilder("// Server API family 1: owning handles retain the complete original snapshot.\nuse std::path::Path;\nuse std::sync::Arc;\nuse super::runtime::{self as R, ServerResult, ConfigError, ErrorCode};\nuse super::snapshot::ConfigSnapshot;\nuse super::types as T;\npub struct Snapshot { storage: Arc<ConfigSnapshot> }\npub type Manager = R::ServerManager<Snapshot>;\nimpl Snapshot {\n    pub fn load(root: &Path, prefer_json: bool) -> ServerResult<Arc<Self>> {\n        ConfigSnapshot::load(root, prefer_json).map(|storage| Arc::new(Self { storage }))\n            .map_err(|message| ConfigError::new(ErrorCode::InvalidValue, message))\n    }\n    pub fn borrow(&self) -> &ConfigSnapshot { &self.storage }\n    pub fn version(&self) -> &str { self.storage.version() }\n");
        server.Replace("pub struct Snapshot { storage: Arc<ConfigSnapshot> }", "pub struct Snapshot { storage: Arc<ConfigSnapshot>, selection: R::SelectionPlan, complete: bool }");
        server.Replace("ConfigSnapshot::load(root, prefer_json).map(|storage| Arc::new(Self { storage }))\n            .map_err(|message| ConfigError::new(ErrorCode::InvalidValue, message))", "let names = Self::catalog()?.tables().iter().map(|t|t.name()).collect::<Vec<_>>();\n        Self::load_selected(root, &names, prefer_json)");
        server.Replace("pub fn borrow(&self) -> &ConfigSnapshot { &self.storage }", "pub fn borrow(&self) -> ServerResult<&ConfigSnapshot> { if !self.complete { return Err(ConfigError::new(ErrorCode::UnsupportedCapability, \"Partial snapshot has no complete legacy view\")); } Ok(&self.storage) }");
        server.AppendLine($"    pub fn catalog() -> ServerResult<&'static R::Catalog> {{ static CATALOG: std::sync::OnceLock<ServerResult<R::Catalog>> = std::sync::OnceLock::new(); CATALOG.get_or_init(||R::Catalog::parse({Quote(RuntimeCatalog.Create(tables,_names.Target).Serialize())})).as_ref().map_err(Clone::clone) }}");
        server.AppendLine("    pub fn selection(&self) -> &R::SelectionPlan { &self.selection }\n    pub fn load_selected(root: &Path, requested: &[&str], prefer_json: bool) -> ServerResult<Arc<Self>> {\n        let catalog = Self::catalog()?; let selection = catalog.select(requested)?;\n        let bundle = R::Bundle::open(root).map_err(|message|ConfigError::new(ErrorCode::InvalidManifest,message))?; bundle.validate_catalog(catalog)?;\n        let names = selection.tables().iter().map(|s|s.as_str()).collect::<std::collections::HashSet<_>>();\n        let storage = ConfigSnapshot::__zr_load_selected(&bundle, prefer_json, Some(&names)).map_err(|message|ConfigError::new(ErrorCode::InvalidValue,message))?;\n        let complete = names.len() == catalog.tables().len();\n        Ok(Arc::new(Self { storage, selection, complete }))\n    }");
        var handles = new StringBuilder();
        foreach (var table in tables)
        {
            string row = table.Name + "Config";
            var code = new StringBuilder("// 此文件由工具生成，请勿手工修改。\nuse std::sync::{Arc, Weak, OnceLock};\nuse super::runtime::{self as R, Result, Json};\nuse super::types as T;\nuse super::snapshot::ConfigSnapshot;\n");
            foreach (var reference in table.Fields.Where(f => types[f.Type].Kind == "tablereference").Select(f => ReferenceTable(f.Type)).Distinct())
                if (reference != table.Name) code.AppendLine($"use super::table_{reference}::{reference}Config;");
            code.AppendLine($"pub struct {row} {{");
            foreach (var f in table.Fields)
            {
                code.AppendLine($"    v_{f.Name}: {TypeName(f.Type)},");
                if (types[f.Type].Kind == "tablereference") code.AppendLine($"    ref_{f.Name}: OnceLock<Weak<{ReferenceTable(f.Type)}Config>>,");
            }
            code.AppendLine($"}}\nimpl {row} {{");
            foreach (var f in table.Fields)
            {
                if (types[f.Type].Kind == "tablereference")
                {
                    Comment(code, f.Description);
                    code.AppendLine($"    pub fn get_{f.Name}Id(&self) -> i32 {{ self.v_{f.Name} }}\n    pub fn get_{f.Name}(&self) -> Option<Arc<{ReferenceTable(f.Type)}Config>> {{ self.ref_{f.Name}.get().and_then(Weak::upgrade) }}");
                }
                else Getter(code, f, false);
            }
            code.Append(cold);code.AppendLine("    fn __zr_decode(v: &Json) -> Result<Self> {\n        Ok(Self {");
            foreach (var f in table.Fields)
            {
                code.AppendLine($"            v_{f.Name}: {Read(f.Type, "v.field(" + Quote(f.Name) + ")?")}?,");
                if (types[f.Type].Kind == "tablereference") code.AppendLine($"            ref_{f.Name}: OnceLock::new(),");
            }
            code.AppendLine("        })\n    }\n}");
            var indexes = new List<(string Name, string[] Fields, string? Group)> { ("Key", table.Keys, null) };
            indexes.AddRange(table.Groups.Select(g => ("Group_" + g.Name, g.Fields, (string?)g.Name)));
            bool referenced = tables.Any(t => t.Fields.Any(f => types[f.Type].Kind == "tablereference" && ReferenceTable(f.Type) == table.Name));
            code.AppendLine($"pub struct {table.Name}Table {{\n    pub(super) rows: Vec<Arc<{row}>>,");
            foreach (var ix in indexes) code.AppendLine($"    pub(super) index_{ix.Name}: R::Index,");
            if (referenced) code.AppendLine($"    ids: std::collections::HashMap<i32, Arc<{row}>>,");
            code.AppendLine($"}}\nimpl {table.Name}Table {{\n    pub fn rows(&self) -> &[Arc<{row}>] {{ &self.rows }}\n    pub fn count(&self) -> usize {{ self.rows.len() }}\n{cold}    pub(super) fn __zr_load(bundle: &R::Bundle, prefer_json: bool) -> Result<Self> {{\n        let source = bundle.load({Quote(table.Serialize())}, prefer_json)?;\n        let rows = source.iter().map(|v| {row}::__zr_decode(v).map(Arc::new)).collect::<Result<Vec<_>>>()?;");
            foreach (var ix in indexes)
            {
                string hash = string.Join(" ", ix.Fields.Select(f => $"R::hash_part(&mut h, &rows[i].v_{f});"));
                string same = string.Join(" && ", ix.Fields.Select(f => $"rows[a].v_{f} == rows[b].v_{f}"));
                code.AppendLine($"        let index_{ix.Name} = R::Index::build(rows.len(), {(ix.Group == null ? "true" : "false")}, |i| {{ let mut h=R::new_hash(); {hash} R::finish_hash(h) }}, |a,b| {same})?;");
            }
            if (referenced) code.AppendLine("        let mut ids = std::collections::HashMap::with_capacity(rows.len());\n        for row in &rows { if ids.insert(row.v_Id, row.clone()).is_some() { return R::invalid(\"引用 Id 不唯一\"); } }");
            code.AppendLine($"        Ok(Self {{ rows, {string.Join(", ", indexes.Select(ix => "index_" + ix.Name))}{(referenced ? ", ids" : "")} }})\n    }}\n{cold}    pub(super) fn __zr_bind(&self, _snapshot: &ConfigSnapshot) -> Result<()> {{");
            foreach (var f in table.Fields.Where(f => types[f.Type].Kind == "tablereference")) code.AppendLine($"        for row in &self.rows {{ if row.v_{f.Name} != 0 {{ let target = _snapshot.get_{ReferenceTable(f.Type)}().ids.get(&row.v_{f.Name}).ok_or({Quote("引用目标不存在：" + table.Name + "." + f.Name)})?; row.ref_{f.Name}.set(Arc::downgrade(target)).map_err(|_| \"引用重复绑定\".to_owned())?; }} }}");
            code.AppendLine("        Ok(())\n    }");
            foreach (var ix in indexes)
            {
                var fields = ix.Fields.Select(f => table.Fields.Single(x => x.Name == f)).ToArray();
                string KeyType(PortableField f) => TypeName(f.Type) == "String" ? "&str" : Copy(types[f.Type]) ? TypeName(f.Type) : "&" + TypeName(f.Type);
                string args = string.Join(", ", fields.Select((f, i) => $"k{i}: {KeyType(f)}"));
                string values = string.Join(", ", fields.Select((_, i) => "k" + i));
                string hash = string.Join(" ", fields.Select((f, i) => $"R::hash_part(&mut h, {(KeyType(f).StartsWith('&') ? "" : "&")}k{i});"));
                string same = string.Join(" && ", fields.Select((f, i) => $"self.rows[i].v_{f.Name} == {(KeyType(f).StartsWith('&') && TypeName(f.Type) != "String" ? "*" : "")}k{i}"));
                code.AppendLine($"    pub fn {(ix.Group == null ? "find" : "get_" + ix.Group)}(&self, {args}) -> {(ix.Group == null ? "Option<&" + row + ">" : "R::Group<'_, " + row + ">")} {{\n        let mut h=R::new_hash(); {hash}\n        let slot=self.index_{ix.Name}.find(R::finish_hash(h), |i| {same});");
                code.AppendLine(ix.Group == null ? $"        slot.map(|s| self.rows[self.index_{ix.Name}.row(s)].as_ref())\n    }}" : $"        R::Group {{ rows: &self.rows, positions: self.index_{ix.Name}.group(slot) }}\n    }}");
                if (ix.Group == null) code.AppendLine($"    pub fn get(&self, {args}) -> Result<&{row}> {{ self.find({values}).ok_or_else(|| \"配置主键不存在\".to_owned()) }}");
                string qualifiedRow = $"super::table_{table.Name}::{row}";
                string handle = ix.Group == null ? $"Option<R::RowHandle<Snapshot, {qualifiedRow}>>" : $"R::GroupHandle<Snapshot, {qualifiedRow}>";
                string method = ix.Group == null ? "find_" + table.Name : "group_" + table.Name + "_" + ix.Group;
                server.AppendLine($"    pub fn {method}(self: &Arc<Self>, {args}) -> ServerResult<{handle}> {{\n        let table = self.storage.{RustMember(table.Name)}.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, {Quote(table.Name + ": table not selected")}))?;\n        let mut h=R::new_hash(); {hash}\n        let slot=table.index_{ix.Name}.find(R::finish_hash(h), |i| {same.Replace("self.rows[i].v_", "table.rows[i].v_")});");
                if (ix.Group == null)
                {
                    server.AppendLine($"        Ok(slot.map(|s| R::RowHandle {{ snapshot: self.clone(), row: table.rows[table.index_Key.row(s)].clone() }}))\n    }}");
                    server.AppendLine($"    pub fn get_{table.Name}(self: &Arc<Self>, {args}) -> ServerResult<R::RowHandle<Snapshot, {qualifiedRow}>> {{ self.find_{table.Name}({values})?.ok_or_else(|| ConfigError::new(ErrorCode::KeyNotFound, {Quote(table.Name + ": key not found")})) }}");
                }
                else
                    server.AppendLine($"        Ok(R::GroupHandle {{ snapshot: self.clone(), rows: |s| &s.storage.get_{table.Name}().rows, positions: |s, slot| s.storage.get_{table.Name}().index_{ix.Name}.group(slot), slot }})\n    }}");
            }
            code.AppendLine("}");
            foreach (var f in table.Fields.Where(f => types[f.Type].Kind == "tablereference"))
            {
                string target = ReferenceTable(f.Type);
                handles.AppendLine($"impl R::RowHandle<Snapshot, super::table_{table.Name}::{row}> {{\n    pub fn get_{f.Name}(&self) -> ServerResult<Option<R::RowHandle<Snapshot, super::table_{target}::{target}Config>>> {{\n        let id = self.row.get_{f.Name}Id();\n        if id == 0 {{ return Ok(None); }}\n        let row = self.snapshot.storage.{RustMember(target)}.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected, \"Reference outside selection\"))?.ids.get(&id).ok_or_else(|| ConfigError::new(ErrorCode::MissingReference, {Quote(table.Name + "." + f.Name + ": missing reference")}))?;\n        Ok(Some(R::RowHandle {{ snapshot: self.snapshot.clone(), row: row.clone() }}))\n    }}\n}}");
            }
            // 引用绑定只在生成模块内部访问 Id 索引，不向业务暴露可变容器。
            files.Add(_names.TablePath(table.Name, "rust"), code.ToString().Replace("    ids: std::collections", "    pub(super) ids: std::collections").Replace("\n    v_", "\n    pub(super) v_"));
        }
        server.AppendLine("}").Append(handles);
        if (_names.ServerApi) files.Add("Rust/server.rs", server.ToString());
    }

    private void RustData(PortableTable[] tables,Dictionary<string,string> files,Func<string,string> typeName,Func<string,string,string> read,Func<PortableField,string> returnType,Func<string,bool> copy)
    {
        bool partitioned=tables.Length>RustPartitionThreshold;
        // Keep large-catalog cold construction bounded without changing hot getters.
        string cold=partitioned?"    #[inline(never)]\n":"";
        var parts=tables.Chunk(RustPartSize).ToArray();
        var types=Types(tables);
        var text=new StringBuilder("// Generated server data: shared slots, owning handles, explicit references.\nuse std::path::Path;\nuse std::sync::{Arc,OnceLock,atomic::{AtomicBool,Ordering}};\nuse super::runtime::{self as R,Json,Result,ServerResult,ConfigError,ErrorCode,Cancellation,LoadPlan,LoadMode};\nuse super::types as T;\n");
        string KeyType(PortableField f)=>typeName(f.Type)=="String"?"&str":copy(f.Type)?typeName(f.Type):"&"+typeName(f.Type);
        foreach(var table in tables)
        {
            string data=table.Name+"Data";
            text.AppendLine($"pub struct {data}{{");
            foreach(var field in table.Fields)text.AppendLine($"    v_{field.Name}:{typeName(field.Type)},");
            text.AppendLine($"}}\nimpl {data}{{\n{cold}    fn decode(value:&Json)->Result<Self>{{Ok(Self{{");
            foreach(var field in table.Fields)text.AppendLine($"        v_{field.Name}:{read(field.Type,$"value.field({Quote(field.Name)})?")}?,");
            text.AppendLine("    })}");
            foreach(var field in table.Fields)text.AppendLine($"    pub fn get_{field.Name}{(types[field.Type].Kind=="tablereference"?"Id":"")}(&self)->{returnType(field)}{{{(copy(field.Type)?"":"&")}self.v_{field.Name}}}");
            text.AppendLine($"}}\npub type {table.Name}Row=R::RowHandle<Snapshot,{data}>;\nimpl R::RowHandle<Snapshot,{data}>{{");
            foreach(var field in table.Fields)
            {
                if(types[field.Type].Kind=="tablereference")
                {
                    string target=types[field.Type].ReferenceTable!;
                    text.AppendLine($"    pub fn get_{field.Name}Id(&self)->i32{{self.row.v_{field.Name}}}\n    pub fn get_{field.Name}(&self)->ServerResult<Option<{target}Row>>{{self.snapshot.reference_{target}(self.row.v_{field.Name},None)}}\n    pub fn load_{field.Name}(&self,cancellation:&Cancellation)->ServerResult<Option<{target}Row>>{{self.snapshot.reference_{target}(self.row.v_{field.Name},Some(cancellation))}}");
                }
                else text.AppendLine($"    pub fn get_{field.Name}(&self)->{returnType(field)}{{{(copy(field.Type)?"":"&")}self.row.v_{field.Name}}}");
            }
            text.AppendLine("}");
            var indexes=new List<(string Name,string[] Fields,string? Group)>{("Key",table.Keys,null)};
            indexes.AddRange(table.Groups.Select(g=>("Group_"+g.Name,g.Fields,(string?)g.Name)));
            foreach(var index in indexes)
            {
                string name=table.Name+"Index_"+index.Name,key=table.Name+"Key_"+index.Name;
                var fields=index.Fields.Select(n=>table.Fields.Single(f=>f.Name==n)).ToArray();
                text.AppendLine($"struct {key}{{");
                for(int i=0;i<fields.Length;i++)text.AppendLine($"    k{i}:{typeName(fields[i].Type)},");
                text.AppendLine($"}}\nimpl {key}{{fn decode(value:&Json)->Result<Self>{{Ok(Self{{");
                for(int i=0;i<fields.Length;i++)text.AppendLine($"    k{i}:{read(fields[i].Type,$"value.field({Quote(fields[i].Name)})?")}?,");
                text.AppendLine("})}} ");
                text.AppendLine($"struct {name}{{keys:Vec<{key}>,index:R::Index,");
                if(index.Group!=null)text.AppendLine("    positions:Vec<Arc<[usize]>>,ready:Vec<AtomicBool>,empty:Arc<[usize]>,");
                text.AppendLine($"}}\nimpl {name}{{\n{cold}    fn build(keys:Vec<{key}>)->ServerResult<Self>{{");
                string hash=string.Join(" ",fields.Select((_,i)=>$"R::hash_part(&mut h,&keys[i].k{i});"));
                // Use different identifiers for key component and row ordinal.
                hash=string.Join(" ",fields.Select((_,i)=>$"R::hash_part(&mut h,&keys[row].k{i});"));
                string same=string.Join("&&",fields.Select((_,i)=>$"keys[a].k{i}==keys[b].k{i}"));
                text.AppendLine($"        let {(index.Group==null?"":"mut ")}index=R::Index::build(keys.len(),{(index.Group==null?"true":"false")},|row|{{let mut h=R::new_hash();{hash}R::finish_hash(h)}},|a,b|{same}).map_err(R::data_error)?;");
                if(index.Group!=null)text.AppendLine("        let positions=index.take_shared_groups();let ready=(0..positions.len()).map(|_|AtomicBool::new(false)).collect();let empty=Arc::from(Vec::<usize>::new().into_boxed_slice());Ok(Self{keys,index,positions,ready,empty})");
                else text.AppendLine("        Ok(Self{keys,index})");
                string args=string.Join(",",fields.Select((f,i)=>$"k{i}:{KeyType(f)}"));
                string queryHash=string.Join(" ",fields.Select((f,i)=>$"R::hash_part(&mut h,{(KeyType(f).StartsWith('&')?"":"&")}k{i});"));
                string match=string.Join("&&",fields.Select((f,i)=>$"self.keys[row].k{i}=={(KeyType(f).StartsWith('&')&&typeName(f.Type)!="String"?"*":"")}k{i}"));
                text.AppendLine($"    }}\n    fn find(&self,{args})->Option<usize>{{let mut h=R::new_hash();{queryHash}self.index.find(R::finish_hash(h),|row|{match})}}\n}}");
            }
            text.AppendLine($"struct {table.Name}Storage{{rows:R::RowStorage<{data}>,key:{table.Name}Index_Key,all:OnceLock<Arc<[usize]>>,");
            foreach(var group in table.Groups)text.AppendLine($"    group_{group.Name}:R::DerivedValue<{table.Name}Index_Group_{group.Name}>,");
            text.AppendLine($"}}\nimpl {table.Name}Storage{{\n{cold}    fn new(indexed:R::IndexedRows)->ServerResult<Self>{{let mut keys=Vec::with_capacity(indexed.count());for i in 0..indexed.count(){{keys.push({table.Name}Key_Key::decode(indexed.key(i)).map_err(R::data_error)?);}}Ok(Self{{rows:R::RowStorage::new(indexed,{data}::decode),key:{table.Name}Index_Key::build(keys)?,all:OnceLock::new(),");
            foreach(var group in table.Groups)text.AppendLine($"        group_{group.Name}:R::DerivedValue::new(),");
            text.AppendLine("    })}");
            foreach(var group in table.Groups)
            {
                string g=group.Name,index=table.Name+"Index_Group_"+g;
                string cachedKey=table.Name+"Key_Group_"+g+"{"+string.Join(",",group.Fields.Select((n,i)=>$"k{i}:cached.v_{n}"+(copy(table.Fields.Single(f=>f.Name==n).Type)?"":".clone()")))+"}";
                string retainedStrings=string.Concat(group.Fields.Select((n,i)=>typeName(table.Fields.Single(f=>f.Name==n).Type)=="String"?$"retained+=4*key.k{i}.len() as u64;":""));
                text.AppendLine($"    fn ensure_{g}(&self,cancellation:Option<&Cancellation>)->ServerResult<&{index}>{{\n        let cancellation=match cancellation{{Some(token)=>token,None=>return self.group_{g}.get()}};self.group_{g}.load(cancellation,&self.rows.indexed.budget,self.rows.indexed.estimated_index_bytes({group.Fields.Length}),||{{\n        let mut keys=Vec::with_capacity(self.rows.count());let {(retainedStrings.Length == 0 ? "" : "mut ")}retained=256+512*self.rows.count() as u64*{group.Fields.Length};for i in 0..self.rows.count(){{let key=if let Some(cached)=self.rows.cached(i){{{cachedKey}}}else{{{table.Name}Key_Group_{g}::decode(&self.rows.indexed.read_transient(i)?).map_err(R::data_error)?}};{retainedStrings}keys.push(key);}}Ok(({index}::build(keys)?,retained))\n        }})\n    }}");
                var fields=group.Fields.Select(n=>table.Fields.Single(f=>f.Name==n)).ToArray();string args=string.Join(",",fields.Select((f,i)=>$"k{i}:{KeyType(f)}")),calls=string.Join(",",fields.Select((_,i)=>"k"+i));
                text.AppendLine($"    fn group_{g}(&self,{args},cancellation:Option<&Cancellation>)->ServerResult<Arc<[usize]>>{{let index=self.ensure_{g}(cancellation)?;let slot=match index.find({calls}){{Some(slot)=>slot,None=>return Ok(index.empty.clone())}};\n        if self.all.get().is_none()&&!index.ready[slot].load(Ordering::Acquire){{for ordinal in index.positions[slot].iter(){{if let Some(token)=cancellation{{self.rows.load(*ordinal,token)?;}}else{{self.rows.get(*ordinal)?;}}}}index.ready[slot].store(true,Ordering::Release);}}Ok(index.positions[slot].clone())\n    }}");
            }
            text.Append(cold);text.AppendLine("    fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{cancellation.check()?;if self.all.get().is_some(){return Ok(())}for i in 0..self.rows.count(){self.rows.load(i,cancellation)?;}");
            foreach(var group in table.Groups)text.AppendLine($"        self.ensure_{group.Name}(Some(cancellation))?;");
            text.AppendLine("        cancellation.check()?;let _=self.all.set(Arc::from((0..self.rows.count()).collect::<Vec<_>>().into_boxed_slice()));Ok(())\n    }\n}");
        }
        text.AppendLine("pub struct Warmup{table:&'static str,action:Box<dyn Fn(&Arc<Snapshot>,&Cancellation)->ServerResult<()>+Send+Sync>}\nimpl Warmup{pub fn table(&self)->&'static str{self.table}}");
        // Bound constructor/type-checking work and stack temporaries. Each owned
        // part is built and boxed in its own frame, then dropped normally on
        // any failure. Returning the box avoids catalog-sized caller temporaries.
        if(partitioned)
        for(int index=0;index<parts.Length;index++)
        {
            text.AppendLine($"trait SnapshotPart{index}Access:Send+Sync{{fn storage(&self)->&SnapshotPart{index};}}\nimpl SnapshotPart{index}Access for SnapshotPart{index}{{fn storage(&self)->&SnapshotPart{index}{{self}}}}");
            text.AppendLine($"struct SnapshotPart{index}{{");
            foreach(var table in parts[index])text.AppendLine($"    table_{table.Name}:Option<{table.Name}Storage>,");
            text.AppendLine($"}}\nimpl SnapshotPart{index}{{\n    #[inline(never)]\n    fn open(bundle:&R::SourceBundle<'_>,plan:&LoadPlan,selected:&std::collections::HashSet<&str>,cancellation:&Cancellation)->ServerResult<Box<dyn SnapshotPart{index}Access>>{{");
            foreach(var table in parts[index])text.AppendLine($"        cancellation.check()?;let table_{table.Name}=if selected.contains({Quote(table.Name)}){{Some({table.Name}Storage::new(R::IndexedRows::open(bundle,{Quote(table.Serialize())},plan.prefer_json)?)?)}}else{{None}};");
            text.AppendLine("        Ok(Box::new(Self{"+string.Join(",",parts[index].Select(table=>"table_"+table.Name))+"}))\n    }");
            const string directory="std::collections::HashMap<String,Arc<std::collections::HashMap<i32,usize>>>";
            text.AppendLine($"    #[inline(never)]\n    fn fill_directory(&self,directory:&mut {directory}){{");
            foreach(var table in parts[index])text.AppendLine($"        if let Some(table)=&self.table_{table.Name}{{directory.insert({Quote(table.Name)}.to_owned(),table.rows.indexed.reference_keys());}}");
            text.AppendLine($"    }}\n    #[inline(never)]\n    fn bind_directory(&self,directory:&Arc<{directory}>){{");
            foreach(var table in parts[index])text.AppendLine($"        if let Some(table)=&self.table_{table.Name}{{table.rows.indexed.bind_references(directory.clone());}}");
            text.AppendLine("    }\n    #[inline(never)]\n    fn validate(&self,snapshot:&Snapshot,cancellation:&Cancellation)->ServerResult<()>{");
            foreach(var table in parts[index])text.AppendLine($"        if let Some(table)=&self.table_{table.Name}{{for i in 0..table.rows.count(){{cancellation.check()?;let value=table.rows.indexed.read_transient(i)?;let _={table.Name}Data::decode(&value).map_err(R::data_error)?;table.rows.indexed.validate_references(&value,|name,id|snapshot.validate_reference(name,id))?;}}}}");
            text.AppendLine("        Ok(())\n    }\n    #[inline(never)]\n    fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{");
            foreach(var table in parts[index])text.AppendLine($"        if let Some(table)=&self.table_{table.Name}{{table.load_all(cancellation)?;}}");
            text.AppendLine("        Ok(())\n    }\n    #[inline(never)]\n    fn warmup(&self,name:&str,cancellation:&Cancellation)->ServerResult<()>{match name{");
            foreach(var table in parts[index])text.AppendLine($"        {Quote(table.Name)}=>self.table_{table.Name}.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,\"Warmup outside selection\"))?.load_all(cancellation),");
            text.AppendLine("        _=>Err(ConfigError::new(ErrorCode::InvalidPlan,\"Unknown warmup table\")),\n    }}\n    #[inline(never)]\n    fn reference(&self,name:&str,id:i32)->ServerResult<()>{match name{");
            foreach(var table in parts[index])text.AppendLine($"        {Quote(table.Name)}=>self.table_{table.Name}.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,\"Reference outside selection\"))?.rows.indexed.reference(id).map(|_|()),");
            text.AppendLine("        _=>Err(ConfigError::new(ErrorCode::MissingReference,\"Unknown reference table\")),\n    }}");
            text.AppendLine("    #[inline(never)]\n    fn loaded_row_count(&self)->usize{"+SumTerms(parts[index].Select(table=>$"self.table_{table.Name}.as_ref().map_or(0,|t|t.rows.loaded_count())"),"+")+"}");
            text.AppendLine("    #[inline(never)]\n    fn completed_table_count(&self)->usize{"+SumTerms(parts[index].Select(table=>$"self.table_{table.Name}.as_ref().map_or(0,|t|usize::from(t.all.get().is_some()))"),"+")+"}\n}");
        }
        text.AppendLine("pub struct Snapshot{version:String,manifest_digest:String,source_namespace:String,load_plan_fingerprint:String,budget:Arc<R::LoadBudget>,selection:R::SelectionPlan,mode:LoadMode,");
        if(partitioned)for(int index=0;index<parts.Length;index++)text.AppendLine($"    part_{index}:Box<dyn SnapshotPart{index}Access>,");
        else foreach(var table in tables)text.AppendLine($"    table_{table.Name}:Option<{table.Name}Storage>,");
        text.AppendLine("}\npub type Manager=R::ServerManager<Snapshot>;\nimpl Snapshot{\n    pub fn version(&self)->&str{&self.version}\n    pub fn selection(&self)->&R::SelectionPlan{&self.selection}\n    pub fn mode(&self)->LoadMode{self.mode}\n    pub fn manager()->Manager{Manager::with_policy(R::ManagerPolicy::new().with_identity(|snapshot:&Self|R::SnapshotIdentity::new(snapshot.source_namespace(),snapshot.version(),snapshot.manifest_digest())))}\n    pub fn manifest_digest(&self)->&str{&self.manifest_digest}\n    pub fn source_namespace(&self)->&str{&self.source_namespace}\n    pub fn catalog_fingerprint(&self)->&str{self.selection.catalog_fingerprint()}\n    pub fn load_plan_fingerprint(&self)->&str{&self.load_plan_fingerprint}\n    pub fn diagnostics(&self)->ServerResult<R::SnapshotDiagnostics>{self.budget.diagnostics()}\n    pub fn load(root:&Path,plan:&LoadPlan,cancellation:&Cancellation)->ServerResult<Arc<Self>>{\n        let source=R::FileConfigSource::new(root);let settings=R::LoadSettings::new(plan.clone()).with_source_namespace(source.source_namespace());Self::load_source(&source,&settings,cancellation)\n    }\n    pub fn load_source(source:&dyn R::ConfigSource,settings:&R::LoadSettings,cancellation:&Cancellation)->ServerResult<Arc<Self>>{\n        let plan=settings.plan();let catalog=super::server::Snapshot::catalog()?;let requested=match &plan.tables{Some(tables)=>tables.iter().map(String::as_str).collect::<Vec<_>>(),None=>catalog.tables().iter().map(R::TableDescriptor::name).collect()};\n        let selection=catalog.select(&requested)?;for name in &plan.warmup_tables{if !selection.tables().contains(name){return Err(ConfigError::new(ErrorCode::InvalidPlan,\"Warmup outside selection\"))}}\n        cancellation.check()?;let bundle=R::SourceBundle::open(source,settings,cancellation)?;bundle.validate_catalog(catalog)?;let _selected=selection.tables().iter().map(String::as_str).collect::<std::collections::HashSet<_>>();");
        if(partitioned)for(int index=0;index<parts.Length;index++)text.AppendLine($"        let part_{index}=SnapshotPart{index}::open(&bundle,plan,&_selected,cancellation)?;");
        else foreach(var table in tables)text.AppendLine($"        cancellation.check()?;let table_{table.Name}=if _selected.contains({Quote(table.Name)}){{Some({table.Name}Storage::new(R::IndexedRows::open(&bundle,{Quote(table.Serialize())},plan.prefer_json)?)?)}}else{{None}};");
        text.AppendLine("        let next=Self{load_plan_fingerprint:R::load_plan_fingerprint(plan.mode,plan.prefer_json,selection.tables()),version:bundle.version().to_owned(),manifest_digest:bundle.digest.clone(),source_namespace:settings.source_namespace().to_owned(),budget:bundle.budget.clone(),selection,mode:plan.mode,"+string.Join(",",partitioned?Enumerable.Range(0,parts.Length).Select(index=>"part_"+index):tables.Select(t=>"table_"+t.Name))+"};");
        if (tables.Length != 0) text.AppendLine("        let mut directory=std::collections::HashMap::new();");
        if(partitioned)for(int index=0;index<parts.Length;index++)text.AppendLine($"        next.part_{index}().fill_directory(&mut directory);");
        else foreach(var table in tables)text.AppendLine($"        if let Some(table)=&next.{RustMember(table.Name)}{{directory.insert({Quote(table.Name)}.to_owned(),table.rows.indexed.reference_keys());}}");
        if (tables.Length != 0) text.AppendLine("        let directory=Arc::new(directory);");
        if(partitioned)for(int index=0;index<parts.Length;index++)text.AppendLine($"        next.part_{index}().bind_directory(&directory);");
        else foreach(var table in tables)text.AppendLine($"        if let Some(table)=&next.{RustMember(table.Name)}{{table.rows.indexed.bind_references(directory.clone());}}");
        text.AppendLine("        if plan.mode!=LoadMode::LazyRowsOnAccess{");
        if(partitioned)for(int index=0;index<parts.Length;index++)text.AppendLine($"        next.part_{index}().validate(&next,cancellation)?;");
        else foreach(var table in tables)text.AppendLine($"            if let Some(table)=&next.{RustMember(table.Name)}{{for i in 0..table.rows.count(){{cancellation.check()?;let value=table.rows.indexed.read_transient(i)?;let _={table.Name}Data::decode(&value).map_err(R::data_error)?;table.rows.indexed.validate_references(&value,|name,id|next.validate_reference(name,id))?;}}}}");
        text.AppendLine("        }\n        if matches!(plan.mode,LoadMode::ServerDefault|LoadMode::SelectedTables){next.load_all(cancellation)?;}\n        for name in &plan.warmup_tables{match "+(partitioned?"Self::table_part(name)":"name.as_str()")+"{");
        if(partitioned)for(int index=0;index<parts.Length;index++)text.AppendLine($"        Some({index})=>next.part_{index}().warmup(name,cancellation)?,");
        else foreach(var table in tables)text.AppendLine($"            {Quote(table.Name)}=>next.{RustMember(table.Name)}.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,\"Warmup outside selection\"))?.load_all(cancellation)?,");
        text.AppendLine("            _=>return Err(ConfigError::new(ErrorCode::InvalidPlan,\"Unknown warmup table\")),\n        }}cancellation.check()?;Ok(Arc::new(next))\n    }\n    fn validate_reference(&self,name:&str,_id:i32)->ServerResult<()>{match "+(partitioned?"Self::table_part(name)":"name")+"{");
        if(partitioned)for(int index=0;index<parts.Length;index++)text.AppendLine($"        Some({index})=>self.part_{index}().reference(name,_id),");
        else foreach(var table in tables)text.AppendLine($"        {Quote(table.Name)}=>{{self.{RustMember(table.Name)}.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,\"Reference outside selection\"))?.rows.indexed.reference(_id)?;Ok(())}},");
        text.AppendLine("        _=>Err(ConfigError::new(ErrorCode::MissingReference,\"Unknown reference table\")),\n    }}\n    pub fn load_all(&self,cancellation:&Cancellation)->ServerResult<()>{\n        cancellation.check()?;");
        if(partitioned)for(int index=0;index<parts.Length;index++)text.AppendLine($"        self.part_{index}().load_all(cancellation)?;");
        else foreach(var table in tables)text.AppendLine($"        if let Some(table)=&self.{RustMember(table.Name)}{{table.load_all(cancellation)?;}}");
        text.AppendLine("        Ok(())\n    }\n    pub fn loaded_row_count(&self)->usize{"+SumTerms(partitioned?Enumerable.Range(0,parts.Length).Select(index=>$"self.part_{index}().loaded_row_count()"):tables.Select(t=>$"self.{RustMember(t.Name)}.as_ref().map_or(0,|t|t.rows.loaded_count())"),"+")+"}");
        text.AppendLine("    pub fn completed_table_count(&self)->usize{"+SumTerms(partitioned?Enumerable.Range(0,parts.Length).Select(index=>$"self.part_{index}().completed_table_count()"):tables.Select(t=>$"self.{RustMember(t.Name)}.as_ref().map_or(0,|t|usize::from(t.all.get().is_some()))"),"+")+"}");
        if(partitioned)for(int index=0;index<parts.Length;index++)text.AppendLine($"    fn part_{index}(&self)->&SnapshotPart{index}{{self.part_{index}.storage()}}");
        if(partitioned)
        {
            text.AppendLine("    fn table_part(name:&str)->Option<usize>{\n        const NAMES:&[(&str,usize)]=&[");
            foreach(var item in tables.Select((table,index)=>(table.Name,Part:index/RustPartSize)).OrderBy(item=>item.Name,StringComparer.Ordinal))
                text.AppendLine($"            ({Quote(item.Name)},{item.Part}),");
            // Match .NET's ordinal UTF-16 ordering without allocating query keys.
            text.AppendLine("        ];NAMES.binary_search_by(|entry|entry.0.encode_utf16().cmp(name.encode_utf16())).ok().map(|index|NAMES[index].1)\n    }");
        }
        text.AppendLine("    pub fn load_source_with_warmup(source:&dyn R::ConfigSource,settings:&R::LoadSettings,requests:&[Warmup],cancellation:&Cancellation)->ServerResult<Arc<Self>>{\n        let catalog=super::server::Snapshot::catalog()?;let names=match &settings.plan().tables{Some(names)=>names.iter().map(String::as_str).collect::<Vec<_>>(),None=>catalog.tables().iter().map(R::TableDescriptor::name).collect()};let selection=catalog.select(&names)?;\n        for request in requests{if !selection.tables().iter().any(|name|name==request.table){return Err(ConfigError::new(ErrorCode::InvalidPlan,\"Warmup outside selection\"));}}\n        let next=Self::load_source(source,settings,cancellation)?;for request in requests{cancellation.check()?;(request.action)(&next,cancellation)?;}cancellation.check()?;Ok(next)\n    }\n    pub fn load_with_warmup(root:&Path,plan:&LoadPlan,requests:&[Warmup],cancellation:&Cancellation)->ServerResult<Arc<Self>>{let source=R::FileConfigSource::new(root);let settings=R::LoadSettings::new(plan.clone()).with_source_namespace(source.source_namespace());Self::load_source_with_warmup(&source,&settings,requests,cancellation)}");
        foreach(var table in tables)
        {
            text.AppendLine($"    pub fn warmup_table_{table.Name}()->Warmup{{Warmup{{table:{Quote(table.Name)},action:Box::new(|snapshot,token|snapshot.load_table_{table.Name}(token).map(|_|()))}}}}");
            foreach(var item in new[]{(Name:"row_"+table.Name,Fields:table.Keys,Method:"load_"+table.Name)}.Concat(table.Groups.Select(g=>(Name:"group_"+table.Name+"_"+g.Name,Fields:g.Fields,Method:"load_group_"+table.Name+"_"+g.Name))))
            {
                var fields=item.Fields.Select(n=>table.Fields.Single(f=>f.Name==n)).ToArray();string args=string.Join(",",fields.Select((f,i)=>"k"+i+":"+typeName(f.Type))),calls=string.Join(",",fields.Select((f,i)=>(KeyType(f).StartsWith('&')?"&":"")+"k"+i));
                text.AppendLine($"    pub fn warmup_{item.Name}({args})->Warmup{{Warmup{{table:{Quote(table.Name)},action:Box::new(move|snapshot,token|snapshot.{item.Method}({calls},token).map(|_|()))}}}}");
            }
        }
        foreach(var table in tables)
        {
            string row=table.Name+"Row",data=table.Name+"Data";
            var fields=table.Keys.Select(n=>table.Fields.Single(f=>f.Name==n)).ToArray();string args=string.Join(",",fields.Select((f,i)=>$"k{i}:{KeyType(f)}")),calls=string.Join(",",fields.Select((_,i)=>"k"+i));
            string get=$"let table=self.{RustMember(table.Name)}.as_ref().ok_or_else(||ConfigError::new(ErrorCode::TableNotSelected,{Quote(table.Name)}))?;";
            text.AppendLine($"    pub fn find_{table.Name}(self:&Arc<Self>,{args})->ServerResult<Option<{row}>>{{{get}let index=match table.key.find({calls}){{Some(slot)=>table.key.index.row(slot),None=>return Ok(None)}};Ok(Some(R::RowHandle{{snapshot:self.clone(),row:table.rows.get(index)?}}))}}\n    pub fn get_{table.Name}(self:&Arc<Self>,{args})->ServerResult<{row}>{{self.find_{table.Name}({calls})?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,\"Row key not found\"))}}\n    pub fn try_load_{table.Name}(self:&Arc<Self>,{args},cancellation:&Cancellation)->ServerResult<Option<{row}>>{{cancellation.check()?;{get}let index=match table.key.find({calls}){{Some(slot)=>table.key.index.row(slot),None=>return Ok(None)}};Ok(Some(R::RowHandle{{snapshot:self.clone(),row:table.rows.load(index,cancellation)?}}))}}\n    pub fn load_{table.Name}(self:&Arc<Self>,{args},cancellation:&Cancellation)->ServerResult<{row}>{{self.try_load_{table.Name}({calls},cancellation)?.ok_or_else(||ConfigError::new(ErrorCode::KeyNotFound,\"Row key not found\"))}}\n    fn reference_{table.Name}(self:&Arc<Self>,id:i32,cancellation:Option<&Cancellation>)->ServerResult<Option<{row}>>{{if id==0{{return Ok(None)}}{get}let index=table.rows.indexed.reference(id)?;let row=if let Some(token)=cancellation{{table.rows.load(index,token)?}}else{{table.rows.get(index)?}};Ok(Some(R::RowHandle{{snapshot:self.clone(),row}}))}}\n    fn ordinal_{table.Name}(&self,index:usize)->ServerResult<Arc<{data}>>{{{get}table.rows.get(index)}}\n    pub fn rows_{table.Name}(self:&Arc<Self>)->ServerResult<R::DataGroup<Self,{data}>>{{{get}let positions=table.all.get().ok_or_else(||ConfigError::new(ErrorCode::TableNotLoaded,\"Load the table explicitly\"))?.clone();Ok(R::DataGroup{{snapshot:self.clone(),positions,query:Self::ordinal_{table.Name}}})}}\n    pub fn load_table_{table.Name}(self:&Arc<Self>,cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,{data}>>{{{get}table.load_all(cancellation)?;self.rows_{table.Name}()}}");
            foreach(var group in table.Groups)
            {
                var fs=group.Fields.Select(n=>table.Fields.Single(f=>f.Name==n)).ToArray();string ga=string.Join(",",fs.Select((f,i)=>$"k{i}:{KeyType(f)}")),gc=string.Join(",",fs.Select((_,i)=>"k"+i));
                text.AppendLine($"    pub fn find_group_{table.Name}_{group.Name}(self:&Arc<Self>,{ga})->ServerResult<R::DataGroup<Self,{data}>>{{{get}let positions=table.group_{group.Name}({gc},None)?;Ok(R::DataGroup{{snapshot:self.clone(),positions,query:Self::ordinal_{table.Name}}})}}\n    pub fn load_group_{table.Name}_{group.Name}(self:&Arc<Self>,{ga},cancellation:&Cancellation)->ServerResult<R::DataGroup<Self,{data}>>{{cancellation.check()?;{get}let positions=table.group_{group.Name}({gc},Some(cancellation))?;Ok(R::DataGroup{{snapshot:self.clone(),positions,query:Self::ordinal_{table.Name}}})}}");
            }
        }
        text.AppendLine("}");
        files.Add("Rust/data.rs",text.ToString());
    }
}
