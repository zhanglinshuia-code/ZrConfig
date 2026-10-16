using System.Text;

namespace ZrConfig.Core;

internal sealed partial class NativeGenerator
{
    internal const int JavaCatalogPartSize = 128;
    internal const int JavaPartitionThreshold = 256;
    private void Java(PortableTable[] tables, Dictionary<string, string> files)
    {
        Dictionary<string, PortableType> types = Types(tables);
        string JType(string id, bool boxed = false)
        {
            PortableType t = types[id];
            if (t.Kind == "nullable")
                return JType(t.Element!, true);
            if (t.Kind == "array")
                return (t.Rank == 1 ? "List<" : "ConfigRuntime.Matrix<") + JType(t.Element!, true) + ">";
            if (t.Kind == "dictionary")
                return $"Map<{JType(t.Key!, true)},{JType(t.Element!, true)}>";
            if (t.Kind == "rawjson")
                return "String";
            if (t.Kind is "primitive" or "tablereference")
                return t.Primitive switch
                {
                    "int8" => boxed ? "Byte" : "byte",
                    "uint8" or "int16" => boxed ? "Short" : "short",
                    "uint16" or "int32" => boxed ? "Integer" : "int",
                    "uint32" or "int64" => boxed ? "Long" : "long",
                    "uint64" => "java.math.BigInteger",
                    "float32" => boxed ? "Float" : "float",
                    "float64" => boxed ? "Double" : "double",
                    "bool" => boxed ? "Boolean" : "boolean",
                    "string" => "String",
                    "decimal" => "ConfigRuntime.Decimal",
                    _ => throw new NotSupportedException(id)
                };
            return "ConfigTypes." + Name(id);
        }
        string Scalar(string primitive, string value) => primitive switch
        {
            "int8" => $"(byte)ConfigRuntime.integer({value},-128,127)",
            "uint8" => $"(short)ConfigRuntime.integer({value},0,255)",
            "int16" => $"(short)ConfigRuntime.integer({value},-32768,32767)",
            "uint16" => $"(int)ConfigRuntime.integer({value},0,65535)",
            "int32" => $"(int)ConfigRuntime.integer({value},Integer.MIN_VALUE,Integer.MAX_VALUE)",
            "uint32" => $"ConfigRuntime.integer({value},0,0xffffffffL)",
            "int64" => $"ConfigRuntime.int64({value})",
            "uint64" => $"ConfigRuntime.uint64({value})",
            "bool" => $"ConfigRuntime.bool({value})",
            "string" => $"ConfigRuntime.string({value})",
            "decimal" => $"new ConfigRuntime.Decimal(ConfigRuntime.string({value}))",
            "float32" => $"ConfigRuntime.float32({value})",
            "float64" => $"ConfigRuntime.float64({value})",
            _ => throw new NotSupportedException(primitive)
        };
        string Read(string id, string value, int depth = 0)
        {
            PortableType t = types[id];
            string item = "item" + depth;
            return t.Kind switch
            {
                "primitive" or "tablereference" => Scalar(t.Primitive, value),
                "rawjson" => $"ConfigRuntime.string({value})",
                "enum" => $"ConfigTypes.{Name(id)}.from({Scalar(t.Primitive, value)})",
                "nullable" => $"({value} == null ? null : {Read(t.Element!, value, depth + 1)})",
                "array" => $"ConfigRuntime.{(t.Rank == 1 ? "list" : "matrix")}({value}, {item} -> {Read(t.Element!, item, depth + 1)})",
                "dictionary" => $"ConfigRuntime.dictionary({value}, {item} -> {Read(t.Key!, item, depth + 1)}, {item} -> {Read(t.Element!, item, depth + 1)})",
                _ => $"ConfigTypes.read_{Name(id)}({value})"
            };
        }
        string PrimitiveType(PortableType t) => t.Primitive switch
        {
            "uint64" => "java.math.BigInteger",
            "uint32" or "int64" => "long",
            _ => "int"
        };
        string Literal(PortableType t, string value) => t.Primitive switch
        {
            "uint64" => $"new java.math.BigInteger({Quote(value)})",
            "uint32" or "int64" => value + "L",
            _ => value
        };
        string Hash(string id, string value)
        {
            PortableType t = types[id];
            if (t.Kind == "enum")
                return value + ".hashCode()";
            return t.Primitive switch
            {
                "bool" => $"({value} ? 1231 : 1237)",
                "float32" or "float64" => $"ConfigRuntime.hash({value})",
                "int64" or "uint32" => $"Long.hashCode({value})",
                "uint64" or "decimal" or "string" => value + ".hashCode()",
                _ => $"(int){value}"
            };
        }
        string Equal(string id, string a, string b) => types[id].Kind == "enum" || types[id].Primitive is "decimal" or "uint64" or "string" ? $"{a}.equals({b})" : $"{a} == {b}";
        files.Add("Java/ConfigRuntime.java", Runtime("java"));
        var model = new StringBuilder("package zrconfig;\nimport java.util.*;\n// 此文件由工具生成；自定义数据结构共用一份定义，不包含游戏业务行为。\npublic final class ConfigTypes {\n    private ConfigTypes() {}\n");
        foreach (PortableType t in types.Values.OrderBy(t => t.Id, StringComparer.Ordinal))
        {
            if (t.Kind == "enum")
            {
                string n = Name(t.Id), pt = PrimitiveType(t);
                model.AppendLine($"    public static final class {n} {{")
                    .AppendLine($"        private final {pt} value;")
                    .AppendLine($"        private {n}({pt} value) {{ this.value = value; }}")
                    .AppendLine($"        public {pt} value() {{ return value; }}");
                foreach (var member in t.EnumMembers)
                    model.AppendLine($"        public static final {n} {Name(member.Key)} = new {n}({Literal(t, member.Value)});");
                model.AppendLine($"        static {n} from({pt} value) {{");
                foreach (var member in t.EnumMembers.DistinctBy(p => p.Value))
                    model.AppendLine($"            if ({(pt == "java.math.BigInteger" ? "value.equals(" + Name(member.Key) + ".value)" : "value == " + Name(member.Key) + ".value")}) return {Name(member.Key)};");
                model.AppendLine("            throw ConfigRuntime.invalid(\"未知枚举值\");\n        }")
                    .AppendLine($"        @Override public int hashCode() {{ return {(pt == "int" ? "value" : pt == "long" ? "Long.hashCode(value)" : "value.hashCode()")}; }}")
                    .AppendLine($"        @Override public boolean equals(Object other) {{ return other instanceof {n} v && {(pt == "java.math.BigInteger" ? "value.equals(v.value)" : "value == v.value")}; }}")
                    .AppendLine("    }");
                continue;
            }
            if (t.Kind is not ("object" or "converter" or "kv" or "vector2" or "vector3"))
                continue;
            string name = Name(t.Id);
            PortableType? parent = t.BaseType == null ? null : types[t.BaseType];
            PortableField[] own = t.Fields.Where(f => parent == null || parent.Fields.All(p => p.Name != f.Name)).ToArray();
            model.AppendLine($"    public static {(t.Abstract ? "abstract " : "")}class {name}{(parent == null ? "" : " extends " + Name(parent.Id))} {{");
            foreach (PortableField f in own)
            {
                model.AppendLine($"        private final {JType(f.Type)} v_{f.Name};");
                Comment(model, f.Description, "        ");
                model.AppendLine($"        public final {JType(f.Type)} get{Accessor(f.Name, "java")}() {{ return v_{f.Name}; }}");
            }
            model.AppendLine($"        {name}({string.Join(", ", t.Fields.Select(f => JType(f.Type) + " p_" + f.Name))}) {{");
            if (parent != null)
                model.AppendLine($"            super({string.Join(", ", parent.Fields.Select(f => "p_" + f.Name))});");
            foreach (PortableField f in own)
                model.AppendLine($"            this.v_{f.Name} = p_{f.Name};");
            model.AppendLine("        }\n    }");
            model.AppendLine($"    static {name} read_{name}(Object value) {{");
            if (t.NullableObject)
                model.AppendLine("        if (value == null) return null;");
            if (t.Variants.Length != 0)
            {
                model.AppendLine("        switch (ConfigRuntime.string(ConfigRuntime.field(value, \"$type\"))) {");
                foreach (string variant in t.Variants)
                    model.AppendLine($"            case {Quote(variant)}: return body_{Name(variant)}(value);");
                model.AppendLine("            default: throw ConfigRuntime.invalid(\"未知多态类型\");\n        }");
            }
            else if (t.Abstract)
                model.AppendLine("        throw ConfigRuntime.invalid(\"抽象类型不能实例化\");");
            else
                model.AppendLine($"        return body_{name}(value);");
            model.AppendLine("    }");
            if (!t.Abstract)
            {
                model.AppendLine($"    private static {name} body_{name}(Object value) {{")
                    .AppendLine($"        return new {name}({string.Join(", ", t.Fields.Select(f => Read(f.Type, "ConfigRuntime.field(value, " + Quote(f.Name) + ")")))});")
                    .AppendLine("    }");
            }
        }
        model.AppendLine("}");
        files.Add("Java/ConfigTypes.java", model.ToString());
        var catalog = new StringBuilder("package zrconfig;\nimport java.io.*;\nimport java.nio.file.*;\nimport java.util.*;\nimport java.util.concurrent.atomic.AtomicReference;\n// 快照在发布前完成加载和引用绑定，旧快照由已有请求继续持有。\npublic final class ConfigSnapshot {\n    public final String version;\n");
        foreach (var table in tables)
            catalog.AppendLine($"    public final {table.Name}Config.Table {table.Name};");
        catalog.AppendLine("    private ConfigSnapshot(ConfigRuntime.Bundle bundle, boolean preferJson) throws IOException { this(bundle, preferJson, null); }\n    private ConfigSnapshot(ConfigRuntime.Bundle bundle, boolean preferJson, Set<String> selected) throws IOException {\n        version = bundle.version;");
        catalog.AppendLine($"        bundle.validateTables(new String[] {{{string.Join(", ", tables.Select(t => Quote(t.Name)))}}});");
        foreach (var table in tables)
            catalog.AppendLine($"        this.{table.Name} = selected == null || selected.contains({Quote(table.Name)}) ? new {table.Name}Config.Table(bundle.load({table.Name}Config.schema(), preferJson)) : null;");
        foreach (var table in tables)
            catalog.AppendLine($"        if (this.{table.Name} != null) this.{table.Name}.bind(this);");
        catalog.AppendLine("    }\n    public static ConfigSnapshot load(Path root) throws IOException { return new ConfigSnapshot(new ConfigRuntime.Bundle(root), false); }\n    public static ConfigSnapshot load(Path root, boolean preferJson) throws IOException { return new ConfigSnapshot(new ConfigRuntime.Bundle(root), preferJson); }\n    public static final class Manager {\n        private final AtomicReference<ConfigSnapshot> current = new AtomicReference<>();\n        public ConfigSnapshot current() { ConfigSnapshot value = current.get(); if (value == null) throw new IllegalStateException(\"配置尚未初始化\"); return value; }\n        public synchronized ConfigSnapshot reload(Path root, boolean preferJson) throws IOException { ConfigSnapshot next = load(root, preferJson); current.set(next); return next; }\n    }\n}");
        catalog.Insert(catalog.ToString().LastIndexOf('}'), "    static ConfigSnapshot fromSelection(ConfigRuntime.Bundle bundle, boolean preferJson, Set<String> selected) throws IOException { return new ConfigSnapshot(bundle, preferJson, selected); }\n");
        files.Add("Java/ConfigSnapshot.java", catalog.ToString());
        var server = new StringBuilder("package zrconfig;\nimport java.io.IOException;\nimport java.nio.file.Path;\n/** Server entry: escaped rows retain their complete original snapshot. */\npublic final class ServerConfig {\n    private ServerConfig() {}\n    public static ConfigSnapshot load(Path root, boolean preferJson) throws IOException {\n        ConfigSnapshot snapshot = ConfigSnapshot.load(root, preferJson);\n");
        foreach (var table in tables) server.AppendLine($"        snapshot.{table.Name}.retain(snapshot);");
        server.AppendLine("        return snapshot;\n    }\n    public static ConfigRuntime.ServerManager<ConfigSnapshot> manager() { return new ConfigRuntime.ServerManager<>(); }");
        string catalogJson = RuntimeCatalog.Create(tables,_names.Target).Serialize();
        server.AppendLine("    private static final class CatalogHolder { static final ConfigRuntime.Catalog VALUE = new ConfigRuntime.Catalog(String.join(\"\",");
        var chunks = catalogJson.Chunk(4000).Select(c => Quote(new string(c))).ToArray();
        server.AppendLine(string.Join(",\n", chunks)).AppendLine("    )); }\n    public static ConfigRuntime.Catalog catalog() { return CatalogHolder.VALUE; }\n}");
        if (_names.ServerApi) files.Add("Java/ServerConfig.java", server.ToString());
        var selected = new StringBuilder("package zrconfig;\nimport java.io.IOException;\nimport java.nio.file.Path;\nimport java.util.*;\npublic final class SelectedSnapshot {\n    private final ConfigSnapshot storage;\n    public final ConfigRuntime.SelectionPlan selection;\n    public final String version;\n    private SelectedSnapshot(ConfigRuntime.Bundle bundle, ConfigRuntime.SelectionPlan plan, boolean preferJson) throws IOException {\n        bundle.validateCatalog(ServerConfig.catalog());\n        selection = plan;\n        storage = ConfigSnapshot.fromSelection(bundle, preferJson, new HashSet<>(selection.tables()));\n        version = storage.version;\n");
        foreach (var table in tables) selected.AppendLine($"        if (storage.{table.Name} != null) storage.{table.Name}.retain(storage);");
        selected.AppendLine("    }\n    public static SelectedSnapshot load(Path root, Collection<String> requested, boolean preferJson) throws IOException { var plan = ServerConfig.catalog().select(requested); return new SelectedSnapshot(new ConfigRuntime.Bundle(root), plan, preferJson); }");
        foreach (var table in tables) selected.AppendLine($"    public {table.Name}Config.Table get_{table.Name}() {{ if (storage.{table.Name} == null) throw new ConfigRuntime.ConfigException(ConfigRuntime.ErrorCode.TableNotSelected, {Quote(table.Name + ": table not selected")}); return storage.{table.Name}; }}");
        selected.AppendLine("}");
        if (_names.ServerApi) files.Add("Java/SelectedSnapshot.java", selected.ToString());
        if (_names.ServerApi) JavaData(tables, files, id => JType(id), (id,value) => Read(id,value), Hash, Equal);
        foreach (PortableTable table in tables)
        {
            string rowType = table.Name + "Config";
            var text = new StringBuilder($"package zrconfig;\nimport java.util.*;\n// 此文件由工具自动生成，请勿手工修改。\npublic final class {rowType} {{\n");
            text.AppendLine("    private ConfigSnapshot serverOwner;");
            foreach (PortableField f in table.Fields)
            {
                PortableType t = types[f.Type];
                text.AppendLine($"    private final {JType(f.Type)} v_{f.Name};");
                Comment(text, f.Description);
                text.AppendLine($"    public {JType(f.Type)} get{Accessor(f.Name, "java")}{(t.Kind == "tablereference" ? "Id" : "")}() {{ return v_{f.Name}; }}");
                if (t.Kind == "tablereference")
                {
                    string target = f.Type["ref:ZrConfig.Models.".Length..].Split("Config:Id:")[0];
                    text.AppendLine($"    private {target}Config ref_{f.Name};")
                        .AppendLine($"    public {target}Config get{Accessor(f.Name, "java")}() {{ return ref_{f.Name}; }}");
                }
            }
            text.AppendLine($"    private {rowType}(Object value) {{");
            foreach (var f in table.Fields)
                text.AppendLine($"        v_{f.Name} = {Read(f.Type, "ConfigRuntime.field(value, " + Quote(f.Name) + ")")};");
            text.AppendLine("    }");
            // 分片防止大型结构字符串超过 JVM 常量池单条 UTF-8 上限。
            string schema = table.Serialize();
            text.AppendLine("    static String schema() { return String.join(\"\",");
            for (int offset = 0; offset < schema.Length; offset += 6000)
                text.AppendLine("        " + Quote(schema.Substring(offset, Math.Min(6000, schema.Length - offset))) + (offset + 6000 < schema.Length ? "," : "); }"));
            var indexes = new List<(string Name, string[] Fields, string? Group)> { ("Key", table.Keys, null) };
            indexes.AddRange(table.Groups.Select(g => ("Group_" + g.Name, g.Fields, (string?)g.Name)));
            foreach (var index in indexes)
            {
                var fields = index.Fields.Select(name => table.Fields.Single(f => f.Name == name)).ToArray();
                string args = string.Join(", ", fields.Select((f, i) => JType(f.Type) + " k" + i));
                string rowHash = "1";
                string keyHash = "1";
                for (int i = 0; i < fields.Length; i++)
                {
                    rowHash = $"({rowHash} * 31 + {Hash(fields[i].Type, "row.v_" + fields[i].Name)})";
                    keyHash = $"({keyHash} * 31 + {Hash(fields[i].Type, "k" + i)})";
                }
                text.AppendLine($"    private static final class Index_{index.Name} extends ConfigRuntime.Index<{rowType}> {{")
                    .AppendLine($"        Index_{index.Name}(List<{rowType}> rows) {{ super(rows, {(index.Name == "Key" ? "true" : "false")}); }}")
                    .AppendLine($"        @Override protected int hash({rowType} row) {{ return {rowHash}; }}")
                    .AppendLine($"        @Override protected boolean same({rowType} a, {rowType} b) {{ return {string.Join(" && ", fields.Select(f => Equal(f.Type, "a.v_" + f.Name, "b.v_" + f.Name)))}; }}")
                    .AppendLine($"        int find({args}) {{")
                    .AppendLine($"            int slot = mix({keyHash}) & mask;")
                    .AppendLine($"            while (slots[slot] >= 0) {{ {rowType} row = rows.get(slots[slot]); if ({string.Join(" && ", fields.Select((f, i) => Equal(f.Type, "row.v_" + f.Name, "k" + i)))}) return slot; slot = (slot + 1) & mask; }}")
                    .AppendLine("            return -1;\n        }")
                    .AppendLine($"        {rowType} row(int slot) {{ return slot < 0 ? null : rows.get(slots[slot]); }}")
                    .AppendLine($"        List<{rowType}> group(int slot) {{ return slot < 0 ? Collections.emptyList() : groups[slot]; }}")
                    .AppendLine("    }");
            }
            text.AppendLine($"    public static final class Table {{\n        private final List<{rowType}> rows;");
            foreach (var index in indexes)
                text.AppendLine($"        private final Index_{index.Name} index_{index.Name};");
            if (tables.Any(t => t.Fields.Any(f => types[f.Type].Kind == "tablereference" && ReferenceTable(f.Type) == table.Name)))
                text.AppendLine($"        private final Map<Integer,{rowType}> ids = new HashMap<>();");
            text.AppendLine($"        Table(List<Object> source) {{\n            var values = new ArrayList<{rowType}>(source.size());\n            for (Object value : source) values.add(new {rowType}(value));\n            rows = Collections.unmodifiableList(values);");
            foreach (var index in indexes)
                text.AppendLine($"            index_{index.Name} = new Index_{index.Name}(rows);");
            if (tables.Any(t => t.Fields.Any(f => types[f.Type].Kind == "tablereference" && ReferenceTable(f.Type) == table.Name)))
                text.AppendLine("            for (var row : rows) { if (ids.put(row.v_Id, row) != null) throw ConfigRuntime.invalid(\"引用 Id 不唯一\"); }");
            text.AppendLine("        }\n        void bind(ConfigSnapshot snapshot) {");
            foreach (var f in table.Fields.Where(f => types[f.Type].Kind == "tablereference"))
            {
                string target = f.Type["ref:ZrConfig.Models.".Length..].Split("Config:Id:")[0];
                text.AppendLine($"            for (var row : rows) if (row.v_{f.Name} != 0) {{ row.ref_{f.Name} = snapshot.{target}.byId(row.v_{f.Name}); if (row.ref_{f.Name} == null) throw ConfigRuntime.invalid(\"引用目标不存在：{table.Name}.{f.Name}\"); }}");
            }
            text.AppendLine("        }");
            text.AppendLine("        void retain(ConfigSnapshot snapshot) { for (var row : rows) row.serverOwner = snapshot; }");
            if (tables.Any(t => t.Fields.Any(f => types[f.Type].Kind == "tablereference" && ReferenceTable(f.Type) == table.Name)))
                text.AppendLine($"        {rowType} byId(int id) {{ return ids.get(id); }}");
            text.AppendLine($"        public List<{rowType}> rows() {{ return rows; }}\n        public int count() {{ return rows.size(); }}");
            foreach (var index in indexes)
            {
                var fields = index.Fields.Select(name => table.Fields.Single(f => f.Name == name)).ToArray();
                string args = string.Join(", ", fields.Select((f, i) => JType(f.Type) + " k" + i));
                string values = string.Join(", ", fields.Select((f, i) => "k" + i));
                if (index.Name == "Key")
                {
                    text.AppendLine($"        public {rowType} find({args}) {{ return index_Key.row(index_Key.find({values})); }}")
                        .AppendLine($"        public {rowType} get({args}) {{ {rowType} value = find({values}); if (value == null) throw new NoSuchElementException(\"配置主键不存在\"); return value; }}");
                }
                else
                    text.AppendLine($"        public List<{rowType}> get{index.Group}({args}) {{ return index_{index.Name}.group(index_{index.Name}.find({values})); }}");
            }
            text.AppendLine("    }\n}");
            files.Add(_names.TablePath(table.Name, "java"), text.ToString());
        }
        if (tables.Length > JavaPartitionThreshold) JavaLargeSnapshots(tables, files);
    }

    private StringBuilder[] JavaDataParts(PortableTable[] tables)
    {
        var parts=new StringBuilder[(tables.Length+JavaCatalogPartSize-1)/JavaCatalogPartSize];
        for(int part=0;part<parts.Length;part++)
        {
            var text=new StringBuilder("package zrconfig;\nimport java.util.*;\nimport java.io.IOException;\nimport static zrconfig.ConfigRuntime.*;\nimport static zrconfig.DataSnapshot.*;\n");
            text.AppendLine($"public abstract class ZrDataSnapshotPart{part}{(part==0?"":" extends ZrDataSnapshotPart"+(part-1))} {{")
                .AppendLine($"    ZrDataSnapshotPart{part}() {{}}");
            if(part==0)text.AppendLine($"    final RowStorage<?>[] zrTables=new RowStorage<?>[{tables.Length}];");
            int end=Math.Min(tables.Length,(part+1)*JavaCatalogPartSize);
            text.AppendLine("    static void openRows(Bundle bundle,LoadPlan plan,Set<String> selected,Map<String,IndexedRows> indexed,Cancellation cancellation)throws IOException {");
            for(int i=part*JavaCatalogPartSize;i<end;i++)
            {
                var table=tables[i];
                text.AppendLine($"        if(selected.contains({Quote(table.Name)})){{cancellation.check();indexed.put({Quote(table.Name)},bundle.openRows({table.Name}Config.schema(),plan.preferJson()));}}");
            }
            text.AppendLine("    }\n    static void buildRows(DataSnapshot owner,Map<String,IndexedRows> indexed,Map<String,RowStorage<?>> active) {");
            for(int i=part*JavaCatalogPartSize;i<end;i++)
            {
                var table=tables[i];
                text.AppendLine($"        if(indexed.containsKey({Quote(table.Name)})){{var table=new {table.Name}Table(indexed.get({Quote(table.Name)}),owner);owner.zrTables[{i}]=table;active.put({Quote(table.Name)},table);}}");
            }
            parts[part]=text.AppendLine("    }");
        }
        return parts;
    }

    // JVM method and constant-pool limits apply even when the data has one row.
    // Keep the old source-level field/getter names and canonical nested row/table
    // types; distribute implementation and inherited entry points into bounded
    // classes. Constructors of these implementation bases are package-private.
    private void JavaLargeSnapshots(PortableTable[] tables,Dictionary<string,string> files)
    {
        int count=(tables.Length+JavaCatalogPartSize-1)/JavaCatalogPartSize;
        for(int part=0;part<count;part++)
        {
            int end=Math.Min(tables.Length,(part+1)*JavaCatalogPartSize);
            var text=new StringBuilder("package zrconfig;\nimport java.util.*;\nimport java.io.IOException;\n");
            text.AppendLine($"public abstract class ZrConfigSnapshotPart{part}{(part==0?"":" extends ZrConfigSnapshotPart"+(part-1))} {{");
            for(int i=part*JavaCatalogPartSize;i<end;i++)text.AppendLine($"    public final {tables[i].Name}Config.Table {tables[i].Name};");
            text.AppendLine($"    ZrConfigSnapshotPart{part}(ConfigRuntime.Bundle bundle,boolean preferJson,Set<String> selected)throws IOException {{");
            if(part!=0)text.AppendLine("        super(bundle,preferJson,selected);");
            for(int i=part*JavaCatalogPartSize;i<end;i++)
            {
                var table=tables[i];
                text.AppendLine($"        this.{table.Name}=selected==null||selected.contains({Quote(table.Name)})?new {table.Name}Config.Table(bundle.load({table.Name}Config.schema(),preferJson)):null;");
            }
            text.AppendLine($"    }}\n    static void $zrFillNamesPart{part}(String[] names){{");
            for(int i=part*JavaCatalogPartSize;i<end;i++)text.AppendLine($"        names[{i}]={Quote(tables[i].Name)};");
            text.AppendLine($"    }}\n    static void $zrBindRowsPart{part}(ConfigSnapshot snapshot){{");
            for(int i=part*JavaCatalogPartSize;i<end;i++)text.AppendLine($"        if(snapshot.{tables[i].Name}!=null)snapshot.{tables[i].Name}.bind(snapshot);");
            text.AppendLine($"    }}\n    static void $zrRetainRowsPart{part}(ConfigSnapshot snapshot){{");
            for(int i=part*JavaCatalogPartSize;i<end;i++)text.AppendLine($"        if(snapshot.{tables[i].Name}!=null)snapshot.{tables[i].Name}.retain(snapshot);");
            text.AppendLine("    }");
            files[$"Java/ZrConfigSnapshotPart{part}.java"]=text.AppendLine("}").ToString();
            if(!_names.ServerApi)continue;
            var selected=new StringBuilder($"package zrconfig;\npublic abstract class ZrSelectedSnapshotPart{part}{(part==0?"":" extends ZrSelectedSnapshotPart"+(part-1))} {{\n");
            if(part==0)selected.AppendLine("    final ConfigSnapshot storage;");
            selected.AppendLine($"    ZrSelectedSnapshotPart{part}(ConfigSnapshot storage){{{(part==0?"this.storage=storage;":"super(storage);")}}}");
            for(int i=part*JavaCatalogPartSize;i<end;i++)
            {
                var table=tables[i];
                selected.AppendLine($"    public {table.Name}Config.Table get_{table.Name}(){{if(storage.{table.Name}==null)throw new ConfigRuntime.ConfigException(ConfigRuntime.ErrorCode.TableNotSelected,{Quote(table.Name+": table not selected")});return storage.{table.Name};}}");
            }
            files[$"Java/ZrSelectedSnapshotPart{part}.java"]=selected.AppendLine("}").ToString();
        }
        var snapshot=new StringBuilder($"package zrconfig;\nimport java.io.*;\nimport java.nio.file.*;\nimport java.util.*;\nimport java.util.concurrent.atomic.AtomicReference;\npublic final class ConfigSnapshot extends ZrConfigSnapshotPart{count-1} {{\n    public final String version;\n    private ConfigSnapshot(ConfigRuntime.Bundle bundle,boolean preferJson,Set<String> selected)throws IOException{{\n        super($zrCheckedCatalog(bundle),preferJson,selected);version=bundle.version;\n");
        for(int i=0;i<count;i++)snapshot.AppendLine($"        $zrBindRowsPart{i}(this);");
        snapshot.AppendLine("    }\n    void retainRows(){");
        for(int i=0;i<count;i++)snapshot.AppendLine($"        $zrRetainRowsPart{i}(this);");
        snapshot.AppendLine($"    }}\n    private static ConfigRuntime.Bundle $zrCheckedCatalog(ConfigRuntime.Bundle bundle){{String[] names=new String[{tables.Length}];");
        for(int i=0;i<count;i++)snapshot.AppendLine($"        $zrFillNamesPart{i}(names);");
        snapshot.AppendLine("        bundle.validateTables(names);return bundle;");
        snapshot.AppendLine("    }\n    public static ConfigSnapshot load(Path root)throws IOException{return load(root,false);}\n    public static ConfigSnapshot load(Path root,boolean preferJson)throws IOException{return new ConfigSnapshot(new ConfigRuntime.Bundle(root),preferJson,null);}\n    static ConfigSnapshot fromSelection(ConfigRuntime.Bundle bundle,boolean preferJson,Set<String> selected)throws IOException{return new ConfigSnapshot(bundle,preferJson,selected);}\n    public static final class Manager {\n        private final AtomicReference<ConfigSnapshot> current=new AtomicReference<>();\n        public ConfigSnapshot current(){ConfigSnapshot value=current.get();if(value==null)throw new IllegalStateException(\"配置尚未初始化\");return value;}\n        public synchronized ConfigSnapshot reload(Path root,boolean preferJson)throws IOException{ConfigSnapshot next=load(root,preferJson);current.set(next);return next;}\n    }\n}");
        files["Java/ConfigSnapshot.java"]=snapshot.ToString();
        if(!_names.ServerApi)return;
        files["Java/SelectedSnapshot.java"]=$"package zrconfig;\nimport java.io.IOException;\nimport java.nio.file.Path;\nimport java.util.*;\npublic final class SelectedSnapshot extends ZrSelectedSnapshotPart{count-1} {{\n    public final ConfigRuntime.SelectionPlan selection;\n    public final String version;\n    private SelectedSnapshot(ConfigRuntime.Bundle bundle,ConfigRuntime.SelectionPlan plan,boolean preferJson)throws IOException{{super(checked(bundle,plan,preferJson));selection=plan;version=storage.version;storage.retainRows();}}\n    private static ConfigSnapshot checked(ConfigRuntime.Bundle bundle,ConfigRuntime.SelectionPlan plan,boolean preferJson)throws IOException{{bundle.validateCatalog(ServerConfig.catalog());return ConfigSnapshot.fromSelection(bundle,preferJson,new HashSet<>(plan.tables()));}}\n    public static SelectedSnapshot load(Path root,Collection<String> requested,boolean preferJson)throws IOException{{var plan=ServerConfig.catalog().select(requested);return new SelectedSnapshot(new ConfigRuntime.Bundle(root),plan,preferJson);}}\n}}\n";
        var retain=new StringBuilder();foreach(var table in tables)retain.AppendLine($"        snapshot.{table.Name}.retain(snapshot);");
        files["Java/ServerConfig.java"]=files["Java/ServerConfig.java"].Replace(retain.ToString(),"        snapshot.retainRows();\n",StringComparison.Ordinal);
    }

    private void JavaData(PortableTable[] tables, Dictionary<string,string> files, Func<string,string> typeName,
        Func<string,string,string> read, Func<string,string,string> hash, Func<string,string,string,string> equal)
    {
        var types = Types(tables);
        bool partitioned = tables.Length > JavaPartitionThreshold;
        var parts = partitioned ? JavaDataParts(tables) : [];
        var text = new StringBuilder("package zrconfig;\nimport java.util.*;\nimport java.nio.file.Path;\nimport java.io.IOException;\nimport static zrconfig.ConfigRuntime.*;\n/** Independent server snapshot; explicit loads share the same row slots in every mode. */\npublic final class DataSnapshot implements SnapshotIdentityProvider {\n    private final LoadBudget budget;\n    private final String namespace,manifestDigest,loadPlanFingerprint;\n    public final String version;\n    public final SelectionPlan selection;\n    public final LoadMode mode;\n");
        if (partitioned) text.Replace("DataSnapshot implements",$"DataSnapshot extends ZrDataSnapshotPart{parts.Length-1} implements");
        if (!partitioned) foreach (var table in tables) text.AppendLine($"    private final {table.Name}Table table_{table.Name};");
        text.AppendLine("    private DataSnapshot(ConfigSource source,LoadPlan plan,SelectionPlan selection,Cancellation cancellation) throws IOException {\n        this.selection = selection; mode = plan.mode(); namespace=plan.sourceNamespace();loadPlanFingerprint=ConfigRuntime.loadPlanFingerprint(mode,plan.preferJson(),selection.tables());\n        var bundle = new Bundle(source,plan.limits(),cancellation); bundle.validateCatalog(ServerConfig.catalog()); version = bundle.version;budget=bundle.budget();manifestDigest=bundle.manifestDigest;\n        var indexed = new HashMap<String,IndexedRows>(); var selected = new HashSet<>(selection.tables());");
        if (partitioned)
        {
            text.AppendLine("        var active = new HashMap<String,RowStorage<?>>();");
            for(int i=0;i<parts.Length;i++)text.AppendLine($"        ZrDataSnapshotPart{i}.openRows(bundle,plan,selected,indexed,cancellation);");
            for(int i=0;i<parts.Length;i++)text.AppendLine($"        ZrDataSnapshotPart{i}.buildRows(this,indexed,active);");
        }
        else
        {
            foreach (var table in tables) text.AppendLine($"        if (selected.contains({Quote(table.Name)})) {{ cancellation.check(); indexed.put({Quote(table.Name)},bundle.openRows({table.Name}Config.schema(),plan.preferJson())); }}");
            foreach (var table in tables) text.AppendLine($"        table_{table.Name} = selected.contains({Quote(table.Name)}) ? new {table.Name}Table(indexed.get({Quote(table.Name)}),this) : null;");
        }
        text.AppendLine("        for (var data : indexed.values()) data.bindReferences(indexed::get);\n        if (mode != LoadMode.LazyRowsOnAccess) {");
        if(partitioned)text.AppendLine("            for (var table : zrTables) if (table != null) table.validate(indexed::get,cancellation);");
        else foreach (var table in tables) text.AppendLine($"            if (table_{table.Name} != null) table_{table.Name}.validate(indexed::get,cancellation);");
        text.AppendLine("        }\n        if (mode == LoadMode.ServerDefault || mode == LoadMode.SelectedTables) loadAll(cancellation);\n        for (String name : plan.warmupTables()) switch(name) {");
        if(partitioned)text.AppendLine("            default -> { var table=active.get(name); if(table==null)throw new ConfigException(ErrorCode.InvalidPlan,\"Unknown warmup table\"); table.loadAll(cancellation); }");
        else foreach (var table in tables) text.AppendLine($"            case {Quote(table.Name)} -> get_{table.Name}().loadAll(cancellation);");
        text.AppendLine("            default -> throw new ConfigException(ErrorCode.InvalidPlan,\"Unknown warmup table\");\n        }\n        cancellation.check();\n    }\n    public static DataSnapshot load(Path root,LoadPlan plan,Cancellation cancellation) throws IOException {\n        var source=new FileConfigSource(root);return load(source,new LoadPlan(plan.mode(),plan.tables(),plan.preferJson(),plan.warmupTables(),plan.limits(),plan.sourceNamespace().isEmpty()?source.sourceNamespace():plan.sourceNamespace()),cancellation);\n    }\n    public String version(){return version;}\n    public String manifestDigest(){return manifestDigest;}\n    public String sourceNamespace(){return namespace;}\n    public String catalogFingerprint(){return selection.catalogFingerprint();}\n    public String loadPlanFingerprint(){return loadPlanFingerprint;}\n    public SnapshotDiagnostics diagnostics(){return budget.diagnostics();}\n    public static DataSnapshot load(ConfigSource source,LoadPlan plan,Cancellation cancellation) throws IOException {\n        Collection<String> requested = plan.tables() == null ? ServerConfig.catalog().tables.stream().map(TableDescriptor::name).toList() : plan.tables();\n        var selection = ServerConfig.catalog().select(requested);\n        for (String name : plan.warmupTables()) if (!selection.tables().contains(name)) throw new ConfigException(ErrorCode.InvalidPlan,\"Warmup outside selection\");\n        cancellation.check(); try{return new DataSnapshot(source,plan,selection,cancellation);}catch(IOException error){throw new ConfigException(ErrorCode.ReadFailed,error.getMessage(),\"read\",0,error);}catch(RuntimeException error){throw serverFailure(error);}\n    }\n    public static ServerManager<DataSnapshot> manager() { return new ServerManager<>(); }\n    public void loadAll(Cancellation cancellation) {");
        if(partitioned)
            text.AppendLine("        for(var table:zrTables)if(table!=null)table.loadAll(cancellation);\n    }\n    public int loadedRowCount(){int count=0;for(var table:zrTables)if(table!=null)count+=table.loadedCount();return count;}\n    public int completedTableCount(){int count=0;for(var table:zrTables)if(table!=null&&table.isComplete())count++;return count;}");
        else
        {
            foreach (var table in tables) text.AppendLine($"        if (table_{table.Name} != null) table_{table.Name}.loadAll(cancellation);");
            text.AppendLine("    }\n    public int loadedRowCount() { return " + (tables.Length == 0 ? "0" : string.Join(" + ",tables.Select(t => $"(table_{t.Name} == null ? 0 : table_{t.Name}.loadedCount())"))) + "; }");
            text.AppendLine("    public int completedTableCount(){return "+(tables.Length==0?"0":string.Join("+",tables.Select(t=>$"(table_{t.Name}!=null && table_{t.Name}.isComplete()?1:0)")))+";}");
        }
        text.AppendLine("    public static final class Warmup {\n        private final String table; private final java.util.function.BiConsumer<DataSnapshot,Cancellation> action;\n        private Warmup(String table,java.util.function.BiConsumer<DataSnapshot,Cancellation> action){this.table=table;this.action=action;}\n        public String table(){return table;}\n    }\n    public static DataSnapshot loadWithWarmup(ConfigSource source,LoadPlan plan,List<Warmup> requests,Cancellation cancellation)throws IOException {\n        if(requests==null||requests.stream().anyMatch(Objects::isNull))throw new ConfigException(ErrorCode.InvalidPlan,\"Missing warmup request\");var steps=List.copyOf(requests);\n        var selected=ServerConfig.catalog().select(plan.tables()==null?ServerConfig.catalog().tables.stream().map(TableDescriptor::name).toList():plan.tables());\n        for(var step:steps)if(!selected.tables().contains(step.table))throw new ConfigException(ErrorCode.InvalidPlan,\"Warmup outside selection\");\n        var next=load(source,plan,cancellation);for(var step:steps){cancellation.check();step.action.accept(next,cancellation);}cancellation.check();return next;\n    }\n    public static DataSnapshot loadWithWarmup(Path root,LoadPlan plan,List<Warmup> requests,Cancellation cancellation)throws IOException {\n        var source=new FileConfigSource(root);return loadWithWarmup(source,new LoadPlan(plan.mode(),plan.tables(),plan.preferJson(),plan.warmupTables(),plan.limits(),plan.sourceNamespace().isEmpty()?source.sourceNamespace():plan.sourceNamespace()),requests,cancellation);\n    }");
        for(int tableIndex=0;tableIndex<tables.Length;tableIndex++)
        {
            var table=tables[tableIndex];var api=partitioned?parts[tableIndex/JavaCatalogPartSize]:text;
            api.AppendLine($"    public static Warmup warmupTable_{table.Name}(){{return new Warmup({Quote(table.Name)},(snapshot,token)->snapshot.get_{table.Name}().loadAll(token));}}");
            foreach(var item in new[]{(Name:"Row_"+table.Name,Fields:table.Keys,Method:"load")}.Concat(table.Groups.Select(g=>(Name:"Group_"+table.Name+"_"+g.Name,Fields:g.Fields,Method:"loadGroup_"+g.Name))))
            {
                var fields=item.Fields.Select(n=>table.Fields.Single(f=>f.Name==n)).ToArray();
                string args=string.Join(",",fields.Select((f,i)=>typeName(f.Type)+" k"+i)),calls=string.Join(",",fields.Select((_,i)=>"k"+i));
                string check=string.Join("",fields.Select((f,i)=>typeName(f.Type) is "String" or "java.math.BigDecimal" or "BigDecimal"?$"if(k{i}==null)throw new ConfigException(ErrorCode.InvalidPlan,\"Null warmup key\");":""));
                api.AppendLine($"    public static Warmup warmup{item.Name}({args}){{{check}return new Warmup({Quote(table.Name)},(snapshot,token)->snapshot.get_{table.Name}().{item.Method}({calls},token));}}");
            }
        }
        for(int tableIndex=0;tableIndex<tables.Length;tableIndex++)
        {
            var table=tables[tableIndex];var api=partitioned?parts[tableIndex/JavaCatalogPartSize]:text;
            if(partitioned)api.AppendLine($"    public {table.Name}Table get_{table.Name}() {{ var table=zrTables[{tableIndex}];if(table==null)throw new ConfigException(ErrorCode.TableNotSelected,{Quote(table.Name)});return ({table.Name}Table)table; }}");
            else api.AppendLine($"    public {table.Name}Table get_{table.Name}() {{ if (table_{table.Name} == null) throw new ConfigException(ErrorCode.TableNotSelected,{Quote(table.Name)}); return table_{table.Name}; }}");
        }
        foreach (var table in tables)
        {
            string row = table.Name + "Row";
            text.AppendLine($"    public static final class {row} {{\n        private final DataSnapshot owner;");
            foreach (var field in table.Fields) text.AppendLine($"        private final {typeName(field.Type)} value_{field.Name};");
            text.AppendLine($"        private {row}(Object value,DataSnapshot owner) {{\n            this.owner = owner;");
            foreach (var field in table.Fields) text.AppendLine($"            value_{field.Name} = {read(field.Type,$"ConfigRuntime.field(value,{Quote(field.Name)})")};");
            text.AppendLine("        }");
            foreach (var field in table.Fields)
            {
                if (types[field.Type].Kind == "tablereference")
                {
                    string target = types[field.Type].ReferenceTable!;
                    text.AppendLine($"        public int get{Accessor(field.Name,"java")}Id() {{ return value_{field.Name}; }}\n        public {target}Row get_{field.Name}() {{ return owner.get_{target}().reference(value_{field.Name},false); }}\n        public {target}Row load_{field.Name}() {{ return owner.get_{target}().reference(value_{field.Name},true); }}");
                }
                else text.AppendLine($"        public {typeName(field.Type)} get{Accessor(field.Name,"java")}() {{ return value_{field.Name}; }}");
            }
            text.AppendLine("    }");
            var snapshotText=text;
            string storageType=partitioned?"ZrDataTableStorage_"+table.Name:table.Name+"Table";
            if(partitioned)
            {
                text.AppendLine($"    public static final class {table.Name}Table extends {storageType} {{\n        {table.Name}Table(IndexedRows indexed,DataSnapshot owner){{super(indexed,value->new {row}(value,owner));}}\n    }}");
                text=new StringBuilder("package zrconfig;\nimport java.util.*;\nimport static zrconfig.ConfigRuntime.*;\nimport static zrconfig.DataSnapshot.*;\n");
            }
            text.AppendLine($"    {(partitioned?"abstract class ":"public static final class ")}{storageType} extends RowStorage<{row}> {{\n        private final IndexedRows indexed;\n        private final Index_Key keyIndex;");
            foreach (var group in table.Groups) text.AppendLine($"        private final DerivedValue<Index_Group_{group.Name}> group_{group.Name} = new DerivedValue<>();");
            text.AppendLine(partitioned
                ? $"        {storageType}(IndexedRows indexed,java.util.function.Function<Object,{row}> factory) {{\n            super(indexed,factory); this.indexed = indexed;\n            var keys = new ArrayList<Key_Key>(indexed.count); for (int i = 0; i < indexed.count; i++) keys.add(new Key_Key(indexed.key(i),i)); keyIndex = new Index_Key(keys);\n        }}"
                : $"        private {table.Name}Table(IndexedRows indexed,DataSnapshot owner) {{\n            super(indexed,value -> new {row}(value,owner)); this.indexed = indexed;\n            var keys = new ArrayList<Key_Key>(indexed.count); for (int i = 0; i < indexed.count; i++) keys.add(new Key_Key(indexed.key(i),i)); keyIndex = new Index_Key(keys);\n        }}");
            var indexes = new List<(string Name,string[] Fields,string? Group)> { ("Key",table.Keys,null) };
            indexes.AddRange(table.Groups.Select(g => ("Group_" + g.Name,g.Fields,(string?)g.Name)));
            foreach (var index in indexes)
            {
                var fields = index.Fields.Select(n => table.Fields.Single(f => f.Name == n)).ToArray();
                string args = string.Join(", ",fields.Select((f,i) => typeName(f.Type)+" k"+i));
                string calls = string.Join(", ",fields.Select((_,i) => "k"+i));
                text.AppendLine($"        private static final class Key_{index.Name} {{\n            final int ordinal;");
                for (int i=0;i<fields.Length;i++) text.AppendLine($"            final {typeName(fields[i].Type)} k{i};");
                text.AppendLine($"            Key_{index.Name}(Object row,int ordinal) {{ this.ordinal = ordinal;");
                for (int i=0;i<fields.Length;i++) text.AppendLine($"                k{i} = {read(fields[i].Type,$"ConfigRuntime.field(row,{Quote(fields[i].Name)})")};");
                text.AppendLine("            }");
                if(index.Group!=null)text.AppendLine($"            Key_{index.Name}({row} row,int ordinal) {{ this.ordinal=ordinal; {string.Join(" ",fields.Select((f,i)=>$"k{i}=row.get{Accessor(f.Name,"java")}();"))} }}");
                text.AppendLine("        }");
                string HashKey(string prefix) { string h="1";for(int i=0;i<fields.Length;i++)h=$"({h} * 31 + {hash(fields[i].Type,prefix+"k"+i)})";return h; }
                string EqualKey(string left,string right) => string.Join(" && ",fields.Select((f,i)=>equal(f.Type,left+"k"+i,right+"k"+i)));
                text.AppendLine($"        private static final class Index_{index.Name} extends Index<Key_{index.Name}> {{");
                if(index.Group!=null)text.AppendLine($"            final java.util.concurrent.atomic.AtomicReferenceArray<List<{row}>> views;");
                text.AppendLine($"            Index_{index.Name}(List<Key_{index.Name}> keys) {{ super(keys,{(index.Group==null?"true":"false")}); {(index.Group==null?"":"views = new java.util.concurrent.atomic.AtomicReferenceArray<>(slots.length);")} }}\n            @Override protected int hash(Key_{index.Name} row) {{ return {HashKey("row.")}; }}\n            @Override protected boolean same(Key_{index.Name} a,Key_{index.Name} b) {{ return {EqualKey("a.","b.")}; }}\n            int find({args}) {{ int slot = mix({HashKey("")}) & mask; while(slots[slot] >= 0) {{ var row = rows.get(slots[slot]); if ({EqualKey("row.","")}) return slot; slot = (slot + 1) & mask; }} return -1; }}");
                if(index.Group==null) text.AppendLine("            int ordinal(int slot) { return slot < 0 ? -1 : rows.get(slots[slot]).ordinal; }");
                else text.AppendLine($"            List<{row}> view(int slot,{storageType} table,boolean load,Cancellation cancellation) {{\n                if (slot < 0) return List.of(); var existing = views.get(slot); if (existing != null) return existing;\n                var result = new ArrayList<{row}>(); for (var key : groups[slot]) result.add(load ? table.loadOrdinal(key.ordinal,cancellation) : table.queryOrdinal(key.ordinal));\n                List<{row}> value = List.copyOf(result); views.compareAndSet(slot,null,value); return views.get(slot);\n            }}");
                text.AppendLine("        }");
                if(index.Group==null)
                {
                    text.AppendLine($"        public {row} find({args}) {{ return queryOrdinal(keyIndex.ordinal(keyIndex.find({calls}))); }}\n        public {row} get({args}) {{ var row = find({calls}); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,\"Row key not found\"); return row; }}\n        public {row} tryLoad({args},Cancellation cancellation) {{ return loadOrdinal(keyIndex.ordinal(keyIndex.find({calls})),cancellation); }}\n        public {row} load({args},Cancellation cancellation) {{ var row = tryLoad({calls},cancellation); if (row == null) throw new ConfigException(ErrorCode.KeyNotFound,\"Row key not found\"); return row; }}\n        public java.util.concurrent.CompletableFuture<{row}> loadAsync({args},Cancellation cancellation,java.util.concurrent.Executor executor) {{ return requireRow(loadOrdinalAsync(keyIndex.ordinal(keyIndex.find({calls})),cancellation,executor)); }}");
                }
                else
                {
                    string g=index.Group;
                    string retainedStrings=string.Concat(fields.Select((f,i)=>typeName(f.Type)=="String"?$"retained += 4L * key.k{i}.length(); ":""));
                    text.AppendLine($"        private Index_{index.Name} index_{g}(boolean load,Cancellation cancellation) {{\n            if (!load) return group_{g}.get();return group_{g}.load(cancellation,indexed.budget,indexed.estimatedIndexBytes({index.Fields.Length}),()->{{ var keys = new ArrayList<Key_{index.Name}>(count()); long retained=256L+512L*count()*{fields.Length}; for (int i=0;i<count();i++) {{ var cached=cachedOrdinal(i); var key=cached==null?new Key_{index.Name}(indexed.readTransient(i),i):new Key_{index.Name}(cached,i); {retainedStrings}keys.add(key); }} return new DerivedResult<>(new Index_{index.Name}(keys),retained); }});\n        }}\n        public List<{row}> findGroup_{g}({args}) {{ var index = index_{g}(false,null); return index.view(index.find({calls}),this,false,null); }}\n        public List<{row}> loadGroup_{g}({args},Cancellation cancellation) {{ var index = index_{g}(true,cancellation); return index.view(index.find({calls}),this,true,cancellation); }}");
                }
            }
            text.AppendLine("        @Override protected void prepareGroups(Cancellation cancellation) {");
            foreach(var group in table.Groups)text.AppendLine($"            index_{group.Name}(true,cancellation);");
            text.AppendLine("        }\n    }");
            if(partitioned)
            {
                files.Add("Java/"+storageType+".java",text.ToString());
                text=snapshotText;
            }
        }
        text.AppendLine("}");
        if(partitioned)
        {
            text.Replace("private Warmup(","Warmup(");
            text.Replace("            default -> throw new ConfigException(ErrorCode.InvalidPlan,\"Unknown warmup table\");\n", "");
            for(int i=0;i<parts.Length;i++)files.Add($"Java/ZrDataSnapshotPart{i}.java",parts[i].AppendLine("}").ToString());
        }
        files.Add("Java/DataSnapshot.java",text.ToString());
    }
}
