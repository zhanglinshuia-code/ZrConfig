using System.Text;

namespace ZrConfig.Core;

internal sealed partial class NativeGenerator
{
    private void Cpp(PortableTable[] tables, Dictionary<string, string> files)
    {
        var types = Types(tables);
        string Primitive(string p) => p switch
        {
            "string" => "std::string",
            "decimal" => "Decimal",
            "bool" => "bool",
            "float32" => "float",
            "float64" => "double",
            _ => "std::" + p + "_t"
        };
        string CType(string id)
        {
            var t = types[id];
            return t.Kind switch
            {
                "primitive" or "tablereference" => Primitive(t.Primitive),
                "rawjson" => "std::string",
                "enum" => Name(id),
                "nullable" => $"std::optional<{CType(t.Element!)}>",
                "array" => $"{(t.Rank == 1 ? "std::vector" : "Matrix")}<{CType(t.Element!)}>",
                "dictionary" => $"std::unordered_map<{CType(t.Key!)},{CType(t.Element!)},Hash<{CType(t.Key!)}>>",
                _ => $"std::shared_ptr<const {Name(id)}>"
            };
        }
        string Scalar(string p, string v) => p switch
        {
            "bool" => v + ".boolean()",
            "string" => v + ".string()",
            "decimal" => $"Decimal({v}.string())",
            "float32" or "float64" => $"floating<{Primitive(p)}>({v})",
            _ => $"integer<{Primitive(p)}>({v})"
        };
        string Read(string id, string value, int depth = 0)
        {
            var t = types[id];
            string item = "item" + depth;
            string Lambda(string child) => $"[](const Json& {item}) {{ return {Read(child, item, depth + 1)}; }}";
            return t.Kind switch
            {
                "primitive" or "tablereference" => Scalar(t.Primitive, value),
                "rawjson" => value + ".string()",
                "enum" => $"static_cast<{Name(id)}>({Scalar(t.Primitive, value)})",
                "nullable" => $"decode_optional({value},{Lambda(t.Element!)})",
                "array" => $"decode_{(t.Rank == 1 ? "list" : "matrix")}({value},{Lambda(t.Element!)})",
                "dictionary" => $"decode_dictionary({value},{Lambda(t.Key!)},{Lambda(t.Element!)})",
                _ => $"read_{Name(id)}({value})"
            };
        }
        files.Add("Cpp/runtime.hpp", Runtime("hpp"));
        var model = new StringBuilder("// 此文件由工具生成；对象只读，集合访问返回引用，不复制数据。\n#pragma once\n#include \"runtime.hpp\"\nnamespace zrconfig {\n");
        foreach (var t in types.Values.Where(t => t.Kind == "enum"))
        {
            model.AppendLine($"enum class {Name(t.Id)} : {Primitive(t.Primitive)} {{");
            foreach (var m in t.EnumMembers)
                model.AppendLine($"    {Name(m.Key)} = {(m.Value == long.MinValue.ToString(System.Globalization.CultureInfo.InvariantCulture) ? "(-9223372036854775807LL - 1)" : m.Value + (t.Primitive == "uint64" ? "ULL" : t.Primitive == "int64" ? "LL" : ""))},");
            model.AppendLine("};");
        }
        var objects = types.Values.Where(t => t.Kind is "object" or "converter" or "kv" or "vector2" or "vector3").ToArray();
        foreach (var t in objects)
            model.AppendLine($"class {Name(t.Id)};\ninline {CType(t.Id)} read_{Name(t.Id)}(const Json& value);");
        var emitted = new HashSet<string>(StringComparer.Ordinal);
        void Definition(PortableType t)
        {
            if (!emitted.Add(t.Id))
                return;
            var parent = t.BaseType == null ? null : types[t.BaseType];
            if (parent != null)
                Definition(parent);
            var own = t.Fields.Where(f => parent == null || parent.Fields.All(p => p.Name != f.Name)).ToArray();
            string n = Name(t.Id);
            model.AppendLine($"class {n}{(parent == null ? "" : " : public " + Name(parent.Id))} {{");
            foreach (var f in own)
                model.AppendLine($"    {CType(f.Type)} v_{f.Name};");
            model.AppendLine("public:");
            model.AppendLine($"    virtual ~{n}() = default;");
            model.Append($"    {n}({string.Join(",", t.Fields.Select(f => CType(f.Type) + " p_" + f.Name))})");
            var initializers = new List<string>();
            if (parent != null)
                initializers.Add(Name(parent.Id) + "(" + string.Join(",", parent.Fields.Select(f => "std::move(p_" + f.Name + ")")) + ")");
            initializers.AddRange(own.Select(f => "v_" + f.Name + "(std::move(p_" + f.Name + "))"));
            model.AppendLine((initializers.Count == 0 ? "" : " : " + string.Join(",", initializers)) + " {}");
            foreach (var f in own)
            {
                Comment(model, f.Description);
                model.AppendLine($"    const {CType(f.Type)}& {Accessor(f.Name, "cpp")}() const noexcept {{ return v_{f.Name}; }}");
            }
            model.AppendLine("};");
        }
        foreach (var t in objects)
            Definition(t);
        foreach (var t in objects.Where(t => !t.Abstract))
            model.AppendLine($"inline {CType(t.Id)} body_{Name(t.Id)}(const Json& value) {{ return std::make_shared<const {Name(t.Id)}>({string.Join(",", t.Fields.Select(f => Read(f.Type, "value.field(" + Quote(f.Name) + ")")))}); }}");
        foreach (var t in objects)
        {
            model.AppendLine($"inline {CType(t.Id)} read_{Name(t.Id)}(const Json& value) {{");
            if (t.NullableObject)
                model.AppendLine("    if(value.kind == Json::Kind::Null) return nullptr;");
            if (t.Variants.Length != 0)
            {
                model.AppendLine("    const auto& tag = value.field(\"$type\").string();");
                foreach (string v in t.Variants)
                    model.AppendLine($"    if(tag == {Quote(v)}) return body_{Name(v)}(value);");
                model.AppendLine("    invalid(\"未知多态类型\");");
            }
            else if (t.Abstract)
                model.AppendLine("    invalid(\"抽象类型不能实例化\");");
            else
                model.AppendLine($"    return body_{Name(t.Id)}(value);");
            model.AppendLine("}");
        }
        // Every table already includes types.hpp. For large catalogs, declare the
        // same visible row types once here instead of emitting N copies of N names.
        // Keep existing small-project files byte-identical; neither path changes
        // which row types a business consumer can name after including a table.
        bool sharedForwards = tables.Length > 256;
        if (sharedForwards)
            foreach (var table in tables) model.AppendLine($"class {table.Name}Config;");
        model.AppendLine("} // namespace zrconfig");
        files.Add("Cpp/types.hpp", model.ToString());
        var catalog = new StringBuilder("// 持有快照期间，记录和引用地址保持稳定；查询本身不分配内存。\n#pragma once\n#include \"types.hpp\"\n");
        foreach (var t in tables)
            catalog.AppendLine($"#include \"{t.Name}Config.hpp\"");
        catalog.AppendLine("namespace zrconfig {\nclass ConfigSnapshot {\n    friend class server::Snapshot;\n    std::string version_;");
        foreach (var t in tables)
            catalog.AppendLine($"    std::unique_ptr<{t.Name}Table> table_{t.Name};");
        catalog.Append("    ConfigSnapshot(Bundle& bundle, [[maybe_unused]] bool prefer_json, [[maybe_unused]] const std::unordered_set<std::string>* selected = nullptr) : version_(bundle.version)");
        foreach (var t in tables)
            catalog.Append($", table_{t.Name}(!selected || selected->count({Quote(t.Name)}) ? std::make_unique<{t.Name}Table>(bundle.load({t.Name}Table::schema(), prefer_json)) : nullptr)");
        catalog.AppendLine(" {");
        foreach (var t in tables)
            catalog.AppendLine($"        if (table_{t.Name}) table_{t.Name}->bind(*this);");
        catalog.AppendLine("    }\npublic:\n    ConfigSnapshot(const ConfigSnapshot&) = delete;\n    ConfigSnapshot& operator=(const ConfigSnapshot&) = delete;\n    const std::string& version() const noexcept { return version_; }");
        catalog.AppendLine($"    static std::shared_ptr<const ConfigSnapshot> load(const std::filesystem::path& root, bool prefer_json = false) {{ Bundle bundle(root); bundle.validate_tables({{{string.Join(", ", tables.Select(t => Quote(t.Name)))}}}); return std::shared_ptr<const ConfigSnapshot>(new ConfigSnapshot(bundle,prefer_json)); }}");
        foreach (var t in tables)
            catalog.AppendLine($"    const {t.Name}Table& {Accessor(t.Name, "cpp")}() const noexcept {{ return *table_{t.Name}; }}");
        catalog.AppendLine("};\nclass ConfigManager {\n    mutable std::mutex gate_;\n    std::shared_ptr<const ConfigSnapshot> current_;\npublic:\n    std::shared_ptr<const ConfigSnapshot> current() const { return std::atomic_load_explicit(&current_,std::memory_order_acquire); }\n    std::shared_ptr<const ConfigSnapshot> reload(const std::filesystem::path& root,bool prefer_json=false) { std::lock_guard<std::mutex> lock(gate_);auto next=ConfigSnapshot::load(root,prefer_json);std::atomic_store_explicit(&current_,next,std::memory_order_release);return next; }\n};");
        foreach (var table in tables)
        {
            string row = table.Name + "Config";
            var text = new StringBuilder($"// 此文件由工具自动生成，请勿手工修改。\n#pragma once\n#include \"types.hpp\"\nnamespace zrconfig {{\nclass ConfigSnapshot;\n");
            if (!sharedForwards)
                foreach (var t in tables)
                    text.AppendLine($"class {t.Name}Config;");
            text.AppendLine($"class {row} {{\n    friend class {table.Name}Table;");
            foreach (var f in table.Fields)
            {
                text.AppendLine($"    {CType(f.Type)} v_{f.Name};");
                if (types[f.Type].Kind == "tablereference")
                    text.AppendLine($"    const {ReferenceTable(f.Type)}Config* ref_{f.Name} = nullptr;");
            }
            text.AppendLine($"public:\n    explicit {row}(const Json& value) : {string.Join(",", table.Fields.Select(f => "v_" + f.Name + "(" + Read(f.Type, "value.field(" + Quote(f.Name) + ")") + ")"))} {{}}");
            foreach (var f in table.Fields)
            {
                bool reference = types[f.Type].Kind == "tablereference";
                Comment(text, f.Description);
                text.AppendLine($"    const {CType(f.Type)}& {Accessor(f.Name, "cpp")}{(reference ? "Id" : "")}() const noexcept {{ return v_{f.Name}; }}");
                if (reference)
                    text.AppendLine($"    const {ReferenceTable(f.Type)}Config* {Accessor(f.Name, "cpp")}() const noexcept {{ return ref_{f.Name}; }}");
            }
            text.AppendLine("};");
            var indexes = new List<(string Name, string[] Fields, string? Group)> { ("Key", table.Keys, null) };
            indexes.AddRange(table.Groups.Select(g => ("Group_" + g.Name, g.Fields, (string?)g.Name)));
            text.AppendLine($"class {table.Name}Table {{\n    friend class ConfigSnapshot;\n    std::vector<{row}> rows_;");
            foreach (var index in indexes)
            {
                var fields = index.Fields.Select(n => table.Fields.Single(f => f.Name == n)).ToArray();
                string Args() => string.Join(",", fields.Select((f, i) => "const " + CType(f.Type) + "& k" + i));
                string hash = "1";
                for (int i = 0; i < fields.Length; i++)
                    hash = $"({hash} * 31u + Hash<{CType(fields[i].Type)}>{{}}(k{i}))";
                text.AppendLine($"    struct Index_{index.Name} : Index<{row}> {{\n        using Index<{row}>::Index;")
                    .AppendLine($"        static std::size_t hash({Args()}) {{ return {hash}; }}")
                    .AppendLine($"        std::size_t find({Args()}) const {{\n            auto slot=mix(hash({string.Join(",", fields.Select((_, i) => "k" + i))}))&mask_;\n            while(slots_[slot]!=missing) {{ const auto& row=rows_[slots_[slot]]; if({string.Join(" && ", fields.Select((f, i) => "row." + Accessor(f.Name, "cpp") + "() == k" + i))}) return slot; slot=(slot+1)&mask_; }}\n            return missing;\n        }}\n    }} index_{index.Name};");
            }
            bool referenced = tables.Any(t => t.Fields.Any(f => types[f.Type].Kind == "tablereference" && ReferenceTable(f.Type) == table.Name));
            if (referenced)
                text.AppendLine($"    std::unordered_map<std::int32_t,const {row}*> ids_;");
            text.AppendLine($"    static std::vector<{row}> decode(const std::vector<Json>& source) {{ std::vector<{row}> rows;rows.reserve(source.size());for(const auto& value:source)rows.emplace_back(value);return rows; }}");
            text.AppendLine("    void bind(const ConfigSnapshot& snapshot);\npublic:");
            text.Append($"    explicit {table.Name}Table(const std::vector<Json>& source) : rows_(decode(source))");
            foreach (var index in indexes)
                text.Append($", index_{index.Name}(rows_,{(index.Name == "Key" ? "true" : "false")},[](const {row}& row){{return Index_{index.Name}::hash({string.Join(",", index.Fields.Select(f => "row." + Accessor(f, "cpp") + "()"))});}},[](const {row}& a,const {row}& b){{return {string.Join(" && ", index.Fields.Select(f => "a." + Accessor(f, "cpp") + "() == b." + Accessor(f, "cpp") + "()"))};}})");
            text.AppendLine(" {");
            if (referenced)
                text.AppendLine("        ids_.reserve(rows_.size());for(const auto& row:rows_)if(!ids_.emplace(row.Id(),&row).second)invalid(\"引用 Id 不唯一\");");
            text.AppendLine($"    }}\n    {table.Name}Table(const {table.Name}Table&) = delete;\n    {table.Name}Table& operator=(const {table.Name}Table&) = delete;");
            text.AppendLine($"    static std::string schema() {{ return {Quote(table.Serialize())}; }}\n    const std::vector<{row}>& rows() const noexcept {{ return rows_; }}\n    std::size_t count() const noexcept {{ return rows_.size(); }}");
            if (referenced)
                text.AppendLine($"    const {row}* by_id(std::int32_t id) const {{ auto it=ids_.find(id);return it==ids_.end()?nullptr:it->second; }}");
            foreach (var index in indexes)
            {
                string args = string.Join(",", index.Fields.Select((n, i) => "const " + CType(table.Fields.Single(f => f.Name == n).Type) + "& k" + i));
                string values = string.Join(",", index.Fields.Select((_, i) => "k" + i));
                if (index.Name == "Key")
                    text.AppendLine($"    const {row}* find({args}) const {{return index_Key.row(index_Key.find({values}));}}\n    const {row}& get({args}) const {{auto p=find({values});if(!p)invalid(\"配置主键不存在\");return *p;}}");
                else
                    text.AppendLine($"    const std::vector<const {row}*>& get{index.Group}({args}) const {{return index_{index.Name}.group(index_{index.Name}.find({values}));}}");
            }
            text.AppendLine("};\n} // namespace zrconfig");
            files.Add(_names.TablePath(table.Name, "cpp"), text.ToString());
            catalog.AppendLine($"inline void {table.Name}Table::bind([[maybe_unused]] const ConfigSnapshot& snapshot) {{");
            foreach (var f in table.Fields.Where(f => types[f.Type].Kind == "tablereference"))
                catalog.AppendLine($"    for(auto& row:rows_)if(row.v_{f.Name}!=0){{row.ref_{f.Name}=snapshot.{Accessor(ReferenceTable(f.Type), "cpp")}().by_id(row.v_{f.Name});if(!row.ref_{f.Name})invalid(\"引用目标不存在：{table.Name}.{f.Name}\");}}");
            catalog.AppendLine("}");
        }
        catalog.AppendLine("} // namespace zrconfig");
        files.Add("Cpp/snapshot.hpp", catalog.ToString());
        var server = new StringBuilder("// Server API family 1. Owning values retain their entire original snapshot.\n#pragma once\n#include \"snapshot.hpp\"\nnamespace zrconfig { namespace server {\nclass Snapshot {\n    std::shared_ptr<const ConfigSnapshot> storage_;\n    explicit Snapshot(std::shared_ptr<const ConfigSnapshot> storage) : storage_(std::move(storage)) {}\npublic:\n    const ConfigSnapshot& borrow() const & noexcept { return *storage_; }\n    const ConfigSnapshot& borrow() const && = delete;\n    const std::string& version() const noexcept { return storage_->version(); }\n    static std::shared_ptr<const Snapshot> load(const std::filesystem::path& root, bool prefer_json = false) { return std::shared_ptr<const Snapshot>(new Snapshot(ConfigSnapshot::load(root, prefer_json))); }\n");
        // Selected snapshots share the same table/row storage and bind code as full legacy loads.
        server.Replace("storage_;\n", "storage_;\n    bool complete_ = true;\n    SelectionPlan selection_;\n");
        server.Replace("borrow() const & noexcept { return *storage_; }", "borrow() const & { if (!complete_) throw ConfigError(ErrorCode::UnsupportedCapability, \"Partial snapshot has no complete legacy view\"); return *storage_; }");
        server.Replace("return std::shared_ptr<const Snapshot>(new Snapshot(ConfigSnapshot::load(root, prefer_json)));", "std::vector<std::string> names; for (const auto& table : catalog().tables()) names.push_back(table.name); return load_selected(root, names, prefer_json);");
        server.AppendLine($"    static const Catalog& catalog() {{ static const Catalog value({Quote(RuntimeCatalog.Create(tables,_names.Target).Serialize())}); return value; }}");
        server.AppendLine("    const SelectionPlan& selection() const noexcept { return selection_; }\n    static std::shared_ptr<const Snapshot> load_selected(const std::filesystem::path& root, const std::vector<std::string>& requested, bool prefer_json = false) {\n        auto plan = catalog().select(requested); Bundle bundle(root); catalog().validate(bundle);\n        std::unordered_set<std::string> selected(plan.tables.begin(), plan.tables.end());\n        auto storage = std::shared_ptr<const ConfigSnapshot>(new ConfigSnapshot(bundle, prefer_json, &selected));\n        auto result = std::shared_ptr<Snapshot>(new Snapshot(std::move(storage)));\n        result->complete_ = selected.size() == catalog().tables().size(); result->selection_ = std::move(plan); return result;\n    }");
        foreach (var table in tables)
        {
            string row = table.Name + "Config", access = "storage_->" + Accessor(table.Name, "cpp") + "()";
            string Args(string[] fields) => string.Join(",", fields.Select((n, i) => "const " + CType(table.Fields.Single(f => f.Name == n).Type) + "& k" + i));
            string Values(string[] fields) => string.Join(",", fields.Select((_, i) => "k" + i));
            string guard = $"if (!storage_->table_{table.Name}) throw ConfigError(ErrorCode::TableNotSelected, {Quote(table.Name + ": table not selected")});";
            server.AppendLine($"    std::optional<OwnedValue<{row}>> find_{table.Name}({Args(table.Keys)}) const {{ {guard} const auto* row = {access}.find({Values(table.Keys)}); if (!row) return std::nullopt; return OwnedValue<{row}>(storage_, *row); }}");
            server.AppendLine($"    OwnedValue<{row}> get_{table.Name}({Args(table.Keys)}) const {{ auto row = find_{table.Name}({Values(table.Keys)}); if (!row) throw ConfigError(ErrorCode::KeyNotFound, {Quote(table.Name + ": key not found")}); return *row; }}");
            foreach (var group in table.Groups)
                server.AppendLine($"    OwnedGroup<{row}> group_{table.Name}_{group.Name}({Args(group.Fields)}) const {{ {guard} return OwnedGroup<{row}>(storage_, {access}.get{group.Name}({Values(group.Fields)})); }}");
        }
        server.AppendLine("};\nusing Manager = ServerManager<Snapshot>;\n} } // namespace zrconfig::server");
        if (_names.ServerApi) files.Add("Cpp/server.hpp", server.ToString());
        if (_names.ServerApi) CppData(tables,files,CType,(id,value)=>Read(id,value));
    }

    private void CppData(PortableTable[] tables,Dictionary<string,string> files,Func<string,string> typeName,Func<string,string,string> read)
    {
        var types=Types(tables);
        bool sharedKeys=tables.Length>256;
        var text=new StringBuilder("// Generated server data family: owning handles, explicit row and group loads.\n#pragma once\n#include \"server.hpp\"\n"+(sharedKeys?"#include <tuple>\n":"")+"namespace zrconfig { namespace data {\nclass Snapshot;\n");
        foreach(var table in tables)text.AppendLine($"class {table.Name}Row;");
        foreach(var table in tables)
        {
            string data=table.Name+"Data",row=table.Name+"Row";
            text.AppendLine($"class {data} {{");
            foreach(var field in table.Fields)text.AppendLine($"    {typeName(field.Type)} v_{field.Name};");
            text.AppendLine($"public:\n    explicit {data}(const Json& value):{string.Join(",",table.Fields.Select(f=>"v_"+f.Name+"("+read(f.Type,$"value.field({Quote(f.Name)})")+")"))}{{}}");
            foreach(var field in table.Fields)text.AppendLine($"    const {typeName(field.Type)}& {Accessor(field.Name,"cpp")}{(types[field.Type].Kind=="tablereference"?"Id":"")}()const noexcept{{return v_{field.Name};}}");
            text.AppendLine($"}};\nclass {row}:public DataRowHandle<Snapshot,{data}>{{\n    friend class Snapshot;\n    {row}(std::shared_ptr<const Snapshot> owner,const {data}* value):DataRowHandle(std::move(owner),value){{}}\npublic:");
            foreach(var field in table.Fields)
            {
                var type=types[field.Type];string access=Accessor(field.Name,"cpp");
                if(type.Kind=="tablereference")text.AppendLine($"    std::int32_t {access}Id()const noexcept{{return value_->{access}Id();}}\n    std::optional<{type.ReferenceTable}Row> get_{field.Name}()const;\n    std::optional<{type.ReferenceTable}Row> load_{field.Name}(const Cancellation& cancellation)const;");
                else if(type.Kind=="enum"||type.Kind=="primitive"&&type.Primitive is not("string"or"decimal"))text.AppendLine($"    {typeName(field.Type)} {access}()const noexcept{{return value_->{access}();}}");
                else text.AppendLine($"    OwnedValue<{typeName(field.Type)}> {access}()const{{return select(&{data}::{access});}}");
            }
            text.AppendLine("};");
            text.AppendLine($"class {table.Name}Storage{{\npublic:\n    detail::RowStorage<{data}> rows;\nprivate:\n    std::mutex groups_gate_;\n    std::atomic<bool> full_{{false}};\n    std::vector<std::size_t> all_;");
            var indexes=new List<(string Name,string[] Fields,string? Group)>{("Key",table.Keys,null)};
            indexes.AddRange(table.Groups.Select(g=>("Group_"+g.Name,g.Fields,(string?)g.Name)));
            foreach(var index in indexes)
            {
                var fields=index.Fields.Select(n=>table.Fields.Single(f=>f.Name==n)).ToArray();
                string args=string.Join(",",fields.Select((f,i)=>"const "+typeName(f.Type)+"& k"+i));
                string calls=string.Join(",",fields.Select((_,i)=>"k"+i));
                string Member(string prefix,int i)=>sharedKeys&&prefix.Length!=0?$"std::get<{i}>({prefix.TrimEnd('.')})":prefix+"k"+i;
                string Hash(string prefix){string value="1";for(int i=0;i<fields.Length;i++)value=$"({value}*31u+Hash<{typeName(fields[i].Type)}>{{}}({Member(prefix,i)}))";return value;}
                string same=string.Join("&&",fields.Select((_,i)=>$"{Member("a.",i)}=={Member("b.",i)}"));
                string match=string.Join("&&",fields.Select((_,i)=>$"{Member("row.",i)}==k{i}"));
                if(sharedKeys)
                {
                    text.AppendLine($"    using Key_{index.Name}=std::tuple<{string.Join(",",fields.Select(f=>typeName(f.Type)))}>;\n    static Key_{index.Name} decode_key_{index.Name}(const Json& row){{return Key_{index.Name}{{{string.Join(",",fields.Select(f=>read(f.Type,$"row.field({Quote(f.Name)})")))}}};}}");
                }
                else
                {
                    text.AppendLine($"    struct Key_{index.Name}{{");
                    for(int i=0;i<fields.Length;i++)text.AppendLine($"        {typeName(fields[i].Type)} k{i};");
                    if(index.Group!=null)text.AppendLine($"        Key_{index.Name}({args}):{string.Join(",",fields.Select((f,i)=>$"k{i}(k{i})"))}{{}}");
                    text.AppendLine($"        explicit Key_{index.Name}(const Json& row):{string.Join(",",fields.Select((f,i)=>$"k{i}({read(f.Type,$"row.field({Quote(f.Name)})")})"))}{{}}\n    }};");
                }
                if(index.Group!=null)text.AppendLine($"    static Key_{index.Name} cached_key_{index.Name}(const {data}& row){{return Key_{index.Name}{{ {string.Join(",",fields.Select(f=>"row."+Accessor(f.Name,"cpp")+"()"))} }};}}");
                text.AppendLine($"    struct Index_{index.Name}:detail::OrdinalIndex<Key_{index.Name}>{{\n        using Base=detail::OrdinalIndex<Key_{index.Name}>;");
                if(index.Group!=null)text.AppendLine("        std::unique_ptr<std::atomic<bool>[]> ready;");
                string decay=sharedKeys?"+":"";
                text.AppendLine($"        explicit Index_{index.Name}(std::vector<Key_{index.Name}> keys):Base(std::move(keys),{(index.Group==null?"true":"false")},{decay}[](const Key_{index.Name}& a){{return {Hash("a.")};}},{decay}[](const Key_{index.Name}& a,const Key_{index.Name}& b){{return {same};}}){{");
                if(index.Group!=null)text.AppendLine("            ready=std::make_unique<std::atomic<bool>[]>(slots_.size());for(std::size_t i=0;i<slots_.size();++i)ready[i].store(false,std::memory_order_relaxed);");
                text.AppendLine($"        }}\n        std::size_t find({args})const{{auto slot=mix({Hash("")})&mask_;while(slots_[slot]!=missing){{const auto& row=keys_[slots_[slot]];if({match})return slot;slot=(slot+1)&mask_;}}return missing;}}\n    }};");
                if(index.Group==null)text.AppendLine($"    static std::vector<Key_Key> keys(detail::IndexedRows& indexed){{std::vector<Key_Key> result;result.reserve(indexed.count());for(std::size_t i=0;i<indexed.count();++i)result.emplace_back({(sharedKeys?"decode_key_Key(indexed.key(i))":"indexed.key(i)")});return result;}}\n    Index_Key key_;\npublic:\n    std::size_t ordinal({args})const{{return key_.ordinal(key_.find({calls}));}}\nprivate:");
                else
                {
                    string g=index.Group;
                    string retainedStrings=string.Concat(fields.Select((f,i)=>typeName(f.Type)=="std::string"?$"retained+=4*static_cast<std::uint64_t>({Member("keys.back().",i)}.size());":""));
                    string transient=sharedKeys?$"decode_key_{index.Name}(rows.indexed().read_transient(i))":"rows.indexed().read_transient(i)";
                    text.AppendLine($"    detail::DerivedValue<Index_{index.Name}> index_{g};\n    Index_{index.Name}& ensure_{g}(bool load,const Cancellation* cancellation){{\n        if(!load)return index_{g}.get();return index_{g}.load(*cancellation,rows.indexed().budget(),rows.indexed().estimated_index_bytes({index.Fields.Length}),[&](){{std::vector<Key_{index.Name}> keys;keys.reserve(rows.count());std::uint64_t retained=256+512*static_cast<std::uint64_t>(rows.count())*{fields.Length};for(std::size_t i=0;i<rows.count();++i){{if(auto cached=rows.cached(i))keys.push_back(cached_key_{index.Name}(*cached));else keys.emplace_back({transient});{retainedStrings}}}return std::make_pair(std::make_unique<Index_{index.Name}>(std::move(keys)),retained);}});\n    }}\npublic:\n    const std::vector<std::size_t>& group_{g}({args},bool load,const Cancellation* cancellation){{\n        auto& index=ensure_{g}(load,cancellation);auto slot=index.find({calls});const auto& ordinals=index.group(slot);if(ordinals.empty())return ordinals;\n        if(!full_.load(std::memory_order_acquire)&&!index.ready[slot].load(std::memory_order_acquire)){{for(auto ordinal:ordinals){{if(load)rows.load(ordinal,*cancellation);else rows.get(ordinal);}}index.ready[slot].store(true,std::memory_order_release);}}return ordinals;\n    }}\nprivate:");
                }
            }
            text.AppendLine($"public:\n    explicit {table.Name}Storage(std::unique_ptr<detail::IndexedRows> indexed):rows(std::move(indexed)),key_(keys(rows.indexed())){{}}\n    void load_all(const Cancellation& cancellation){{\n        cancellation.check();if(full_.load(std::memory_order_acquire))return;for(std::size_t i=0;i<rows.count();++i)rows.load(i,cancellation);");
            foreach(var group in table.Groups)text.AppendLine($"        ensure_{group.Name}(true,&cancellation);");
            text.AppendLine("        std::lock_guard<std::mutex> lock(groups_gate_);cancellation.check();if(!full_.load(std::memory_order_acquire)){all_.reserve(rows.count());for(std::size_t i=0;i<rows.count();++i)all_.push_back(i);full_.store(true,std::memory_order_release);}\n    }\n    bool is_complete()const noexcept{return full_.load(std::memory_order_acquire); }\n    const std::vector<std::size_t>& all()const{if(!full_.load(std::memory_order_acquire))throw ConfigError(ErrorCode::TableNotLoaded,\"Load the table explicitly\");return all_;}\n};");
        }
        text.AppendLine("class Snapshot:public std::enable_shared_from_this<Snapshot>{\n    std::string version_,manifest_digest_,source_namespace_,load_plan_fingerprint_;\n    std::shared_ptr<detail::LoadBudget> budget_;\n    SelectionPlan selection_;\n    LoadMode mode_;\n");
        foreach(var table in tables)text.AppendLine($"    friend class {table.Name}Row;");
        foreach(var table in tables)text.AppendLine($"    std::unique_ptr<{table.Name}Storage> table_{table.Name};");
        text.AppendLine("    Snapshot(detail::SourceBundle& bundle,SelectionPlan selection,const LoadSettings& settings,const Cancellation& cancellation):version_(bundle.version()),manifest_digest_(bundle.manifest_digest()),source_namespace_(settings.source_namespace()),budget_(bundle.budget()),selection_(std::move(selection)),mode_(settings.plan().mode){\n        const auto& plan=settings.plan();load_plan_fingerprint_=detail::load_plan_fingerprint(plan.mode,plan.prefer_json,selection_.tables);std::unordered_set<std::string> selected(selection_.tables.begin(),selection_.tables.end());\n        std::unordered_map<std::string,detail::IndexedRows*> indexed;");
        foreach(var table in tables)text.AppendLine($"        if(selected.count({Quote(table.Name)})){{cancellation.check();table_{table.Name}=std::make_unique<{table.Name}Storage>(detail::IndexedRows::open(bundle,{table.Name}Table::schema(),plan.prefer_json));indexed.emplace({Quote(table.Name)},&table_{table.Name}->rows.indexed());}}");
        text.AppendLine("        auto directory=std::make_shared<const std::unordered_map<std::string,detail::IndexedRows*>>(indexed);for(const auto& entry:indexed)entry.second->bind_references(directory);\n        if(mode_!=LoadMode::LazyRowsOnAccess){");
        foreach(var table in tables)text.AppendLine($"            if(table_{table.Name}){{auto& input=table_{table.Name}->rows.indexed();for(std::size_t i=0;i<input.count();++i){{cancellation.check();auto raw=input.read_transient(i);[[maybe_unused]] {table.Name}Data validated(raw);input.validate_references(raw,[&](const std::string& name)->detail::IndexedRows&{{return *indexed.at(name);}});}}}}");
        text.AppendLine("        }\n        if(mode_==LoadMode::ServerDefault||mode_==LoadMode::SelectedTables)load_all(cancellation);\n        for(const auto& name:plan.warmup_tables){(void)name;");
        foreach(var table in tables)text.AppendLine($"            if(name=={Quote(table.Name)})table_{table.Name}->load_all(cancellation);");
        text.AppendLine("        }\n        cancellation.check();\n    }\npublic:\n    const std::string& version()const noexcept{return version_;}\n    const SelectionPlan& selection()const noexcept{return selection_;}\n    LoadMode mode()const noexcept{return mode_;}\n    const std::string& manifest_digest()const noexcept{return manifest_digest_;}\n    const std::string& source_namespace()const noexcept{return source_namespace_;}\n    const std::string& catalog_fingerprint()const noexcept{return selection_.catalog_fingerprint;}\n    const std::string& load_plan_fingerprint()const noexcept{return load_plan_fingerprint_;}\n    SnapshotDiagnostics diagnostics()const{return budget_->diagnostics();}\n    static std::shared_ptr<const Snapshot> load(const std::filesystem::path& root,const LoadPlan& plan,const Cancellation& cancellation){FileConfigSource source(root);LoadSettings settings(plan);settings.with_source_namespace(source.source_namespace());return load(source,settings,cancellation);}\n    static std::shared_ptr<const Snapshot> load(ConfigSource& source,const LoadSettings& settings,const Cancellation& cancellation){\n        const auto& plan=settings.plan();\n        switch(plan.mode){case LoadMode::ServerDefault:case LoadMode::SelectedTables:case LoadMode::LazyRowsStrict:case LoadMode::LazyRowsOnAccess:break;default:throw ConfigError(ErrorCode::InvalidPlan,\"Unknown load mode\");}\n        std::vector<std::string> names;if(plan.tables)names=*plan.tables;else for(const auto& table:server::Snapshot::catalog().tables())names.push_back(table.name);auto selection=server::Snapshot::catalog().select(names);\n        for(const auto& name:plan.warmup_tables)if(std::find(selection.tables.begin(),selection.tables.end(),name)==selection.tables.end())throw ConfigError(ErrorCode::InvalidPlan,\"Warmup outside selection\");\n        cancellation.check();try{detail::SourceBundle bundle(source,settings,cancellation);server::Snapshot::catalog().validate(bundle.metadata());return std::shared_ptr<const Snapshot>(new Snapshot(bundle,std::move(selection),settings,cancellation));}catch(const detail::InvalidData& error){throw ConfigError(ErrorCode::InvalidValue,error.what(),\"prepare\");}\n    }\n    void load_all(const Cancellation& cancellation)const{\n        cancellation.check();");
        foreach(var table in tables)text.AppendLine($"        if(table_{table.Name})table_{table.Name}->load_all(cancellation);");
        text.AppendLine("    }\n    std::size_t loaded_row_count()const noexcept{return "+SumTerms(tables.Select(t=>$"(table_{t.Name}?table_{t.Name}->rows.loaded_count():0)"),"+")+";}");
        text.AppendLine("    std::size_t completed_table_count()const noexcept{return "+SumTerms(tables.Select(t=>$"(table_{t.Name} && table_{t.Name}->is_complete()?1:0)"),"+")+";}");
        text.AppendLine("    class Warmup {\n        friend class Snapshot;std::string table_;std::function<void(const std::shared_ptr<const Snapshot>&,const Cancellation&)> action_;\n        Warmup(std::string table,std::function<void(const std::shared_ptr<const Snapshot>&,const Cancellation&)> action):table_(std::move(table)),action_(std::move(action)){}\n    public:const std::string& table()const noexcept{return table_;}\n    };\n    static std::shared_ptr<const Snapshot> load_with_warmup(ConfigSource& source,const LoadSettings& settings,const std::vector<Warmup>& requests,const Cancellation& cancellation){\n        auto steps=requests;std::vector<std::string> names;if(settings.plan().tables)names=*settings.plan().tables;else for(const auto& table:server::Snapshot::catalog().tables())names.push_back(table.name);auto selection=server::Snapshot::catalog().select(names);\n        for(const auto& step:steps)if(std::find(selection.tables.begin(),selection.tables.end(),step.table_)==selection.tables.end())throw ConfigError(ErrorCode::InvalidPlan,\"Warmup outside selection\");\n        auto next=load(source,settings,cancellation);for(const auto& step:steps){cancellation.check();step.action_(next,cancellation);}cancellation.check();return next;\n    }\n    static std::shared_ptr<const Snapshot> load_with_warmup(const std::filesystem::path& root,const LoadPlan& plan,const std::vector<Warmup>& requests,const Cancellation& cancellation){FileConfigSource source(root);LoadSettings settings(plan);settings.with_source_namespace(source.source_namespace());return load_with_warmup(source,settings,requests,cancellation);}");
        foreach(var table in tables)
        {
            if(tables.Length>256)text.AppendLine($"    static Warmup warmup_table_{table.Name}(){{return Warmup({Quote(table.Name)},+[](const std::shared_ptr<const Snapshot>& snapshot,const Cancellation& token){{snapshot->load_table_{table.Name}(token);}});}}");
            else text.AppendLine($"    static Warmup warmup_table_{table.Name}(){{return Warmup({Quote(table.Name)},[](const auto& snapshot,const auto& token){{snapshot->load_table_{table.Name}(token);}});}}");
            foreach(var item in new[]{(Name:"row_"+table.Name,Fields:table.Keys,Method:"load_"+table.Name)}.Concat(table.Groups.Select(g=>(Name:"group_"+table.Name+"_"+g.Name,Fields:g.Fields,Method:"load_group_"+table.Name+"_"+g.Name))))
            {
                var fields=item.Fields.Select(n=>table.Fields.Single(f=>f.Name==n)).ToArray();string args=string.Join(",",fields.Select((f,i)=>typeName(f.Type)+" k"+i)),calls=string.Join(",",fields.Select((_,i)=>"k"+i));
                if(tables.Length>256)
                {
                    string keyArgs=string.Join(",",fields.Select((field,index)=>"const "+typeName(field.Type)+"& k"+index));
                    text.AppendLine($"    static Warmup warmup_{item.Name}({args}){{return Warmup({Quote(table.Name)},std::bind(+[](const std::shared_ptr<const Snapshot>& snapshot,const Cancellation& token,{keyArgs}){{snapshot->{item.Method}({calls},token);}},std::placeholders::_1,std::placeholders::_2,{calls}));}}");
                }
                else text.AppendLine($"    static Warmup warmup_{item.Name}({args}){{return Warmup({Quote(table.Name)},[{calls}](const auto& snapshot,const auto& token){{snapshot->{item.Method}({calls},token);}});}}");
            }
        }
        foreach(var table in tables)
        {
            string row=table.Name+"Row",keyArgs=string.Join(",",table.Keys.Select((n,i)=>"const "+typeName(table.Fields.Single(f=>f.Name==n).Type)+"& k"+i)),calls=string.Join(",",table.Keys.Select((_,i)=>"k"+i));
            string guard=$"if(!table_{table.Name})throw ConfigError(ErrorCode::TableNotSelected,{Quote(table.Name)});";
            text.AppendLine($"    std::optional<{row}> find_{table.Name}({keyArgs})const{{{guard}auto index=table_{table.Name}->ordinal({calls});if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return {row}(shared_from_this(),table_{table.Name}->rows.get(index));}}\n    {row} get_{table.Name}({keyArgs})const{{auto row=find_{table.Name}({calls});if(!row)throw ConfigError(ErrorCode::KeyNotFound,\"Row key not found\");return *row;}}\n    std::optional<{row}> try_load_{table.Name}({keyArgs},const Cancellation& cancellation)const{{{guard}cancellation.check();auto index=table_{table.Name}->ordinal({calls});if(index==(std::numeric_limits<std::size_t>::max)())return std::nullopt;return {row}(shared_from_this(),table_{table.Name}->rows.load(index,cancellation));}}\n    {row} load_{table.Name}({keyArgs},const Cancellation& cancellation)const{{auto row=try_load_{table.Name}({calls},cancellation);if(!row)throw ConfigError(ErrorCode::KeyNotFound,\"Row key not found\");return *row;}}\nprivate:\n    std::optional<{row}> reference_{table.Name}(std::int32_t id,bool load,const Cancellation* cancellation=nullptr)const{{if(!id)return std::nullopt;{guard}auto index=table_{table.Name}->rows.indexed().reference(id);return {row}(shared_from_this(),load?table_{table.Name}->rows.load(index,*cancellation):table_{table.Name}->rows.get(index));}}\n    static {row} ordinal_{table.Name}(const std::shared_ptr<const Snapshot>& owner,std::size_t index){{return {row}(owner,owner->table_{table.Name}->rows.get(index));}}\npublic:\n    DataGroup<Snapshot,{row}> rows_{table.Name}()const{{{guard}return {{shared_from_this(),table_{table.Name}->all(),&ordinal_{table.Name}}};}}\n    DataGroup<Snapshot,{row}> load_table_{table.Name}(const Cancellation& cancellation)const{{{guard}table_{table.Name}->load_all(cancellation);return rows_{table.Name}();}}");
            foreach(var group in table.Groups)
            {
                string args=string.Join(",",group.Fields.Select((n,i)=>"const "+typeName(table.Fields.Single(f=>f.Name==n).Type)+"& k"+i)),values=string.Join(",",group.Fields.Select((_,i)=>"k"+i));
                text.AppendLine($"    DataGroup<Snapshot,{row}> find_group_{table.Name}_{group.Name}({args})const{{{guard}return {{shared_from_this(),table_{table.Name}->group_{group.Name}({values},false,nullptr),&ordinal_{table.Name}}};}}\n    DataGroup<Snapshot,{row}> load_group_{table.Name}_{group.Name}({args},const Cancellation& cancellation)const{{{guard}cancellation.check();return {{shared_from_this(),table_{table.Name}->group_{group.Name}({values},true,&cancellation),&ordinal_{table.Name}}};}}");
            }
        }
        text.AppendLine("};\nusing Manager=ServerManager<Snapshot>;");
        foreach(var table in tables)foreach(var field in table.Fields.Where(f=>types[f.Type].Kind=="tablereference"))
        {
            string target=types[field.Type].ReferenceTable!,access=Accessor(field.Name,"cpp");
            text.AppendLine($"inline std::optional<{target}Row> {table.Name}Row::get_{field.Name}()const{{return owner_->reference_{target}(value_->{access}Id(),false);}}\ninline std::optional<{target}Row> {table.Name}Row::load_{field.Name}(const Cancellation& cancellation)const{{return owner_->reference_{target}(value_->{access}Id(),true,&cancellation);}}");
        }
        text.AppendLine("} } // namespace zrconfig::data");
        files.Add("Cpp/data.hpp",text.ToString());
    }
}
