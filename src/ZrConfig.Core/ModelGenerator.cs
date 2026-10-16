using System.Text;
using System.Text.Json;
using ZrConfig.Runtime;
using ZrConfig.Validation;

namespace ZrConfig.Core;

public static class ModelGenerator
{
    public static bool Generate(string dataFolder,
        string generatedFolder,
        DiagnosticCollector diagnostics,
        bool acceptChanges = false,
        ConfigTarget target = ConfigTarget.Client,
        IReadOnlyList<string>? languages = null, string dataFormat = "binary") => Prepare(dataFolder,
        generatedFolder,
        diagnostics,
        target: target, languages: languages, dataFormat: dataFormat)?.Apply(diagnostics,
        acceptChanges) == true;
    /// <summary>Calculate the complete output before allowing removal or a change of input root.</summary>
    public static ModelGenerationPlan? Prepare(string dataFolder,
        string generatedFolder,
        DiagnosticCollector diagnostics,
        Action<string>? progress = null,
        ConfigTarget target = ConfigTarget.Client,
        WorkflowOperation? operation = null,
        IReadOnlyList<string>? languages = null,
        string dataFormat = "binary") => PrepareWithFamily("auto", dataFolder, generatedFolder, diagnostics, progress, target, operation, languages, dataFormat);

    /// <summary>Select an API family explicitly, without changing the established Prepare signature.</summary>
    public static ModelGenerationPlan? PrepareWithFamily(string runtimeFamily,
        string dataFolder, string generatedFolder, DiagnosticCollector diagnostics,
        Action<string>? progress = null, ConfigTarget target = ConfigTarget.Client,
        WorkflowOperation? operation = null, IReadOnlyList<string>? languages = null, string dataFormat = "binary") => PrepareWithNamespaces(null,runtimeFamily,dataFolder,generatedFolder,diagnostics,progress,target,operation,languages,dataFormat);

    public static ModelGenerationPlan? PrepareWithNamespaces(GenerationNamespaces? namespaces,string runtimeFamily,
        string dataFolder,string generatedFolder,DiagnosticCollector diagnostics,Action<string>? progress=null,
        ConfigTarget target=ConfigTarget.Client,WorkflowOperation? operation=null,IReadOnlyList<string>? languages=null,string dataFormat="binary") => PrepareWithSourceMappings(operation?.SourceMappings,namespaces,runtimeFamily,dataFolder,generatedFolder,diagnostics,progress,target,operation,languages,dataFormat);

    /// <summary>Explicitly bind renamed Excel sources to their previously accepted logical identities.</summary>
    public static ModelGenerationPlan? PrepareWithSourceMappings(IReadOnlyList<SourceNameMapping>? sourceMappings,GenerationNamespaces? namespaces,string runtimeFamily,
        string dataFolder,string generatedFolder,DiagnosticCollector diagnostics,Action<string>? progress=null,
        ConfigTarget target=ConfigTarget.Client,WorkflowOperation? operation=null,IReadOnlyList<string>? languages=null,string dataFormat="binary")
    {
        operation?.Checkpoint();
        dataFolder = Path.TrimEndingDirectorySeparator(Path.GetFullPath(dataFolder));
        generatedFolder = Path.TrimEndingDirectorySeparator(Path.GetFullPath(generatedFolder));
        if (!Directory.Exists(dataFolder))
        {
            diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error,
                "INPUT_MISSING",
                ConfigText.Get("Excel 输入目录不存在，请重新选择完整的 Data 根目录。",
                "Excel input directory does not exist. Select the complete Data root."),
                dataFolder));
            return null;
        }

        if (ConfigSourceLayout.IsGeneratedDirectory(generatedFolder))
        {
            diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error,
                "CODE_ROOT_GENERATED",
                ConfigText.Get("请选择代码根目录 Config，而不是 Generated 子目录。",
                "Select the Config root, not its Generated subdirectory."),
                generatedFolder));
            return null;
        }

        // 递归加载代码根目录的手写源码与共享序列化，排除本工具的 Generated 输出。
        ConfigSourceLayout.SupportPlan support = ConfigSourceLayout.Prepare(generatedFolder, diagnostics);
        var inputs = new GenerationInputSnapshot(dataFolder, generatedFolder, target, support.Replacements);
        using LoadedRules? types = RuleCompiler.PrepareTypes(generatedFolder, diagnostics, support.Replacements, progress, inputs);
        if (types == null)
        {
            return null;
        }

        var registry = new ConfigTypeRegistry(types.Assembly);
        if (registry.BinaryVersion != 4)
        {
            diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error,
                "SUPPORT_UPDATE_REQUIRED",
                ConfigText.Get("按行读取需要 v4 序列化支持源码，请合并升级 Serialization/ConfigBinary.cs 后重新生成。",
                "Row loading requires v4 serialization support. Update Serialization/ConfigBinary.cs before generating."),
                generatedFolder));
            return null;
        }

        GenerationSourceContract sourceContract;
        try { sourceContract = GenerationSourceContract.Resolve(generatedFolder,sourceMappings); }
        catch(Exception error) when(error is InvalidDataException or JsonException or ArgumentException)
        { if(sourceMappings!=null){diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error,"SOURCE_BINDING",error.Message));return null;}sourceContract=GenerationSourceContract.Empty; }
        IReadOnlyList<TableSchema> schemas = SchemaLoader.Load(dataFolder,
            diagnostics,
            registry,
            tableDefinitionsPath: Path.Combine(generatedFolder,
            "Tables.json"),
            target: target,
            inputs: inputs,
            operation: operation,sourceContract:sourceContract).OrderBy(schema => schema.Name, StringComparer.Ordinal).ToArray();
        if (diagnostics.Snapshot().Any(x => x.Severity == DiagnosticSeverity.Error))
        {
            return null;
        }

        string[] selected = ExportOptions.Languages(languages ?? ["csharp"]);
        PortableTable[]? portable = null;
        GeneratedSymbolPlan names;
        try
        {
            if (selected.Any(l => l != "csharp") || ExportOptions.Format(dataFormat) != "binary")
                portable = schemas.Select(PortableSchema.Create).ToArray();
        }
        catch (Exception error) when (error is NotSupportedException or ArgumentException)
        {
            diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "PORTABLE_SCHEMA", error.Message)); return null;
        }
        try
        {
            names = GeneratedSymbolPlan.Create(schemas, portable, selected, GenerationFamily.Server(generatedFolder,runtimeFamily));
            names.Target=ConfigTargets.Marker(target);
            names.SourceContract=sourceContract.ForOutput(schemas);
            names.Namespaces=GenerationNamespaces.Resolve(generatedFolder,namespaces);
        }
        catch (Exception error) when (error is NotSupportedException or ArgumentException or InvalidDataException or JsonException)
        {
            diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "SYMBOL_PLAN", error.Message)); return null;
        }
        Dictionary<string, string> files = new(StringComparer.Ordinal);
        foreach (TableSchema schema in schemas)
        {
            operation?.Report("generating", schema.Name);
            files.Add(names.TablePath(schema.Name, "csharp"), GenerateTable(schema, names));
        }

        files.Add("TableCodecs.g.cs", GenerateRegistry(schemas, names.ServerApi, names.Target, names.SourceContract));
        if (portable != null)
        {
            try
            {
                // 导表规则仍复用内部 C# 模型；目标语言源码只消费分端后的纯数据结构。
                NativeGenerator.Generate(portable, selected, files, names);
            }
            catch (Exception error) when (error is NotSupportedException or ArgumentException)
            {
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "PORTABLE_SCHEMA", error.Message));
                return null;
            }
        }
        names.Namespaces.Apply(files);
        if (files.Keys.Any(n => n.Contains('/')))
            files.Add(".native-files.json", JsonSerializer.Serialize(files.Keys.Where(n => n.Contains('/')).Order(StringComparer.Ordinal).ToArray()) + "\n");
        foreach (string path in files.Keys.Where(p => p.EndsWith(".cs", StringComparison.Ordinal) || p.EndsWith(".java", StringComparison.Ordinal) ||
            p.EndsWith(".go", StringComparison.Ordinal) || p.EndsWith(".hpp", StringComparison.Ordinal) || p.EndsWith(".rs", StringComparison.Ordinal) ||
            p.EndsWith(".ts", StringComparison.Ordinal) || p.EndsWith(".lua", StringComparison.Ordinal)).ToArray())
            files[path] = (path.EndsWith(".lua", StringComparison.Ordinal) ? "-- " : "// ") + GeneratedApiContracts.Marker + "\n" + files[path];
        using LoadedRules? candidate = RuleCompiler.PrepareGeneration(generatedFolder,files,support.Replacements,diagnostics,progress);
        if (candidate == null) return null;
        return new ModelGenerationPlan(dataFolder, generatedFolder, files, support, target, inputs, SchemaSummary.Create(target, schemas), names, selected, types.Assembly, candidate.Assembly);
    }

    private static string GenerateTable(TableSchema schema, GeneratedSymbolPlan names)
    {
        // 各职责直接追加到同一缓冲区；每张表只构建和格式化一个编译单元。
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />")
            .AppendLine("#nullable enable")
            .AppendLine("// 此文件由工具自动生成，请勿手工修改。")
            .AppendLine("using System;")
            .AppendLine("using System.Buffers;")
            .AppendLine("using System.Collections;")
            .AppendLine("using System.Collections.Generic;")
            .AppendLine("using System.IO;")
            .AppendLine("using System.Text;")
            .AppendLine("using ZrConfig.Runtime;")
            .AppendLine("using ZrConfig.Serialization;")
            .AppendLine("namespace ZrConfig.Models")
            .AppendLine("{");
        string Helper(string role) => names.CSharpHelper(schema.Name, role);
        AppendModel(sb, schema, Helper);
        TableAccessGenerator.Append(sb, schema);
        AppendCodec(sb, schema, Helper);
        sb.AppendLine("}");
        if (names.ServerApi) ServerAccessGenerator.Append(sb, schema, Helper);
        return GeneratedCodeStyle.Format(sb.ToString());
    }

    private static void AppendModel(StringBuilder sb, TableSchema schema, Func<string,string> helper)
    {
        string type = schema.Name + "Config";
        sb.AppendLine("    [ZrConfig.Runtime.ConfigTableLayout(5)]");

        foreach (TableGroupSchema group in schema.Groups)
        {
            sb.AppendLine($"    [ZrConfig.Runtime.ConfigTableGroup(\"{group.Name}\", {string.Join(", ", group.Fields.Select(f => "\"" + f.Name + "\""))})]");
        }

        sb.AppendLine($"    public sealed partial class {type} : ZrConfig.Runtime.{(schema.HasIntegerId ? "IConfigKeyRow" : "IConfigRow")}")
            .AppendLine("    {");
        foreach (FieldSchema field in schema.Fields)
        {
            if (field.Type.ReferenceTable is { } target)
            {
                string rowType = $"global::ZrConfig.Models.{target}Config";
                string id = helper( "ZrReferenceId" + field.Name);
                string cached = helper( "_zrCached" + field.Name);
                string resolver = helper( "_referenceResolver");
                sb.AppendLine($"        private {rowType}? {cached};")
                    .AppendLine($"        internal int {id} {{ get; }}");
                AppendFieldDocumentation(sb, field.Description);
                sb.AppendLine($"        public {rowType}? @{field.Name}")
                    .AppendLine("        {")
                    .AppendLine("            get")
                    .AppendLine("            {")
                    .AppendLine($"                {resolver}?.ThrowIfInvalid();")
                    .AppendLine($"                if ({id} == 0) return null;")
                    .AppendLine($"                {rowType}? cached = global::System.Threading.Volatile.Read(ref {cached});")
                    .AppendLine("                if (cached != null) return cached;")
                    .AppendLine("                // 空引用和缓存命中不访问目标表的编解码器。")
                    .AppendLine($"                ZrConfig.Runtime.ConfigReferenceResolver resolver = ZrConfig.Runtime.ConfigReferenceCache.RequireResolver({resolver}, this, nameof(@{field.Name}));")
                    .AppendLine($"                {rowType}? value = resolver.Resolve<{rowType}>({rowType}Table.GetCodec(), {id}, this, nameof(@{field.Name}));")
                    .AppendLine("                resolver.ThrowIfInvalid();")
                    .AppendLine($"                return global::System.Threading.Interlocked.CompareExchange(ref {cached}, value, null) ?? value;")
                    .AppendLine("            }")
                    .AppendLine("        }")
                    .AppendLine("        /// <summary>只查询已加载的引用缓存，不读取资源或解码；未缓存或空引用返回 false。</summary>")
                    .AppendLine($"        public bool {helper("TryGetCached" + field.Name)}(out {rowType}? value)")
                    .AppendLine("        {")
                    .AppendLine($"            return ZrConfig.Runtime.ConfigReferenceCache.TryGet(ref {cached}, {resolver}, {id}, this, nameof(@{field.Name}), out value);")
                    .AppendLine("        }")
                    .AppendLine($"        public ZrConfig.Runtime.ConfigOperation<{rowType}?> {helper( "Get" + field.Name + "Async")}(global::System.Threading.CancellationToken cancellationToken = default)")
                    .AppendLine("        {")
                    .AppendLine("            try")
                    .AppendLine("            {")
                    .AppendLine("                cancellationToken.ThrowIfCancellationRequested();")
                    .AppendLine($"                {resolver}?.ThrowIfInvalid();")
                    .AppendLine($"                if ({id} == 0) return ZrConfig.Runtime.ConfigOperation<{rowType}?>.FromResult(null);")
                    .AppendLine($"                {rowType}? cached = global::System.Threading.Volatile.Read(ref {cached});")
                    .AppendLine($"                if (cached != null) return ZrConfig.Runtime.ConfigOperation<{rowType}?>.FromResult(cached);")
                    .AppendLine($"                return {helper( "ZrLoad" + field.Name + "Async")}(cancellationToken);")
                    .AppendLine("            }")
                    .AppendLine($"            catch (global::System.Exception error) {{ return ZrConfig.Runtime.ConfigOperation<{rowType}?>.FromException(error); }}")
                    .AppendLine("        }")
                    .AppendLine($"        private async ZrConfig.Runtime.ConfigOperation<{rowType}?> {helper( "ZrLoad" + field.Name + "Async")}(global::System.Threading.CancellationToken cancellationToken)")
                    .AppendLine("        {")
                    .AppendLine($"            ZrConfig.Runtime.ConfigReferenceResolver resolver = ZrConfig.Runtime.ConfigReferenceCache.RequireResolver({resolver}, this, nameof(@{field.Name}));")
                    .AppendLine($"            {rowType}? value = await resolver.ResolveAsync<{rowType}>({rowType}Table.GetCodec(), {id}, this, nameof(@{field.Name}), cancellationToken);")
                    .AppendLine("            cancellationToken.ThrowIfCancellationRequested();")
                    .AppendLine("            resolver.ThrowIfInvalid();")
                    .AppendLine($"            return global::System.Threading.Interlocked.CompareExchange(ref {cached}, value, null) ?? value;")
                    .AppendLine("        }");
            }
            else
            {
                AppendFieldDocumentation(sb, field.Description);
                sb.AppendLine($"        public {field.Type.CsName} @{field.Name} {{ get; }}");
            }
        }

        if (schema.HasIntegerId)
        {
            sb.AppendLine("        int ZrConfig.Runtime.IConfigKeyRow.ConfigId => @Id;");
        }

        sb.AppendLine()
            .AppendLine($"        public {type}({string.Join(", ", schema.Fields.Select(field => field.Type.CsName + " @" + TableAccessGenerator.ParameterName(schema.Fields, field)))})")
            .AppendLine("        {");
        foreach (FieldSchema field in schema.Fields)
        {
            string parameter = TableAccessGenerator.ParameterName(schema.Fields, field);
            string member = field.Type.ReferenceTable != null ? helper(
                "ZrReferenceId" + field.Name) : "@" + field.Name;
            string receiver = field.Type.ReferenceTable == null && field.Name == parameter ? "this." : "";
            sb.AppendLine($"            {receiver}{member} = @{parameter}{(field.Type.ClrType == typeof(string) ? " ?? string.Empty" : "")};");
        }

        sb.AppendLine("        }");
        FieldSchema[] references = schema.Fields.Where(f => f.Type.ReferenceTable != null).ToArray();
        if (references.Length != 0)
        {
            string resolvedField = helper( "_referencesResolved");
            string resolveMethod = helper( "ResolveReferences");
            sb.AppendLine($"        private bool {resolvedField};")
                .AppendLine($"        private ZrConfig.Runtime.ConfigReferenceResolver? {helper( "_referenceResolver")};")
                .AppendLine($"        internal void {helper( "BindReferences")}(ZrConfig.Runtime.ConfigReferenceResolver resolver)")
                .AppendLine("        {")
                .AppendLine($"            if ({resolvedField}) throw new global::System.InvalidOperationException(\"References are already bound.\");")
                .AppendLine($"            {helper( "_referenceResolver")} = resolver;")
                .AppendLine($"            {resolvedField} = true;")
                .AppendLine("        }")
                .AppendLine($"        internal void {resolveMethod}(ZrConfig.Runtime.ConfigReferenceResolver resolver)")
                .AppendLine("        {")
                .AppendLine($"            if ({resolvedField}) throw new global::System.InvalidOperationException(\"References are already resolved.\");");
            for (int i = 0; i < references.Length; i++)
            {
                FieldSchema field = references[i];
                sb.AppendLine($"            global::ZrConfig.Models.{field.Type.ReferenceTable}Config? reference{i} = resolver.Resolve<global::ZrConfig.Models.{field.Type.ReferenceTable}Config>({helper( "ZrReferenceId" + field.Name)}, this, nameof(@{field.Name}));");
            }

            for (int i = 0; i < references.Length; i++)
            {
                sb.AppendLine($"            {helper( "_zrCached" + references[i].Name)} = reference{i};");
            }

            sb.AppendLine($"            {resolvedField} = true;").AppendLine("        }");
        }

        sb.AppendLine("    }");
    }

    // 保留 Excel 的字段说明；转义 XML，避免说明中的符号破坏文档注释。
    internal static void AppendFieldDocumentation(StringBuilder text, string description, string indent = "        ")
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return;
        }

        text.AppendLine(indent + "/// <summary>");
        foreach (string line in description.ReplaceLineEndings("\n").Split('\n'))
        {
            text.AppendLine(indent + "/// " + System.Security.SecurityElement.Escape(line));
        }

        text.AppendLine(indent + "/// </summary>");
    }

    private static void AppendCodec(StringBuilder sb, TableSchema schema, Func<string,string> helper)
    {
        string type = schema.Name + "Config";
        string? jsonSchema = null;
        try
        {
            jsonSchema = PortableSchema.Create(schema).Serialize();
        }
        catch (NotSupportedException) { /* 旧项目的任意转换器继续支持二进制；JSON 导出会给出具体诊断。 */ }
        ValueCodecGenerator values = new(schema.Fields.Select(f => f.Type));
        string Join(IEnumerable<string> parts)
        {
            string[] items = parts.ToArray();
            return items.Length == 1 ? items[0] : "(" + string.Join(", ", items) + ")";
        }

        string keyType = Join(schema.KeyFields.Select(f => f.Type.CsName));
        string key = "s_selectKey";
        string readKey = Join(schema.KeyFields.Select(f => values.Read(f.Type)));
        string create = $"new {type}({string.Join(", ", schema.Fields.Select((f,i) => "field"+i))})";
        string readFields=string.Join("\n",schema.Fields.Select((f,i)=>$"            {f.Type.CsName} field{i};try{{field{i}={values.Read(f.Type)};}}catch(Exception error){{throw global::ZrConfig.Runtime.Server.DecodeGuard.AtField(error,\"{f.Name}\");}}"));
        sb.AppendLine($"    public sealed class {type}Codec : IIndexedTableCodec{(jsonSchema == null ? "" : ", IConfigJsonCodec")}{(schema.Fields.Any(f => f.Type.ReferenceTable != null) ? ", IConfigRowReferenceCodec" : "")}")
            .AppendLine("    {")
            .AppendLine($"        public string TableName {{ get {{ return \"{schema.Name}\"; }} }}")
            .AppendLine($"        public string SchemaHash {{ get {{ return \"{schema.SchemaHash}\"; }} }}")
            .AppendLine($"        public Type RowType {{ get {{ return typeof({type}); }} }}")
            .AppendLine($"        public IList CreateRows() {{ return new List<{type}>(); }}")
            .AppendLine($"        public object CreateRow(object[] values) {{ return new {type}({string.Join(", ", schema.Fields.Select((f, i) => $"({f.Type.CsName})values[{i}]"))}); }}")
            .AppendLine($"        public IConfigTable CreateTable(IList rows) {{ return new {type}Table((IEnumerable<{type}>)rows); }}")
            .AppendLine("        public void Write(Stream output, IList rows)")
            .AppendLine("        {")
            .AppendLine("            if (!output.CanSeek)")
            .AppendLine("            {")
            .AppendLine("                using var buffer = new MemoryStream();")
            .AppendLine("                Write(buffer, rows); buffer.Position = 0; buffer.CopyTo(output);")
            .AppendLine("                return;")
            .AppendLine("            }")
            .AppendLine("            using var writer = new BinaryWriter(output, Encoding.UTF8, true);")
            .AppendLine("            BinaryFormat.WriteHeader(writer, TableName, SchemaHash, rows.Count);")
            .AppendLine("            WriteTypes(writer);")
            .AppendLine("            long[] locations = ArrayPool<long>.Shared.Rent(rows.Count);")
            .AppendLine("            try")
            .AppendLine("            {")
            .AppendLine("                for (int i = 0; i < rows.Count; i++)")
            .AppendLine("                {")
            .AppendLine($"                    var row = ({type})rows[i]!;");
        foreach (FieldSchema field in schema.KeyFields)
        {
            sb.AppendLine("                    " + values.Write(field.Type, "row.@" + field.Name));
        }

        sb.AppendLine($"                    writer.Write({(schema.HasIntegerId ? "row.@Id" : "0")});")
            .AppendLine("                    locations[i] = output.Position;")
            .AppendLine("                    writer.Write(0); writer.Write(0); // 相对行数据区的偏移、长度")
            .AppendLine("                }")
            .AppendLine("                long dataStart = output.Position;")
            .AppendLine("                for (int i = 0; i < rows.Count; i++)")
            .AppendLine("                {")
            .AppendLine("                    long start = output.Position;")
            .AppendLine($"                    WriteRow(writer, ({type})rows[i]!);")
            .AppendLine("                    long end = output.Position;")
            .AppendLine("                    output.Position = locations[i];")
            .AppendLine("                    writer.Write(checked((int)(start - dataStart)));")
            .AppendLine("                    writer.Write(checked((int)(end - start)));")
            .AppendLine("                    output.Position = end;")
            .AppendLine("                }")
            .AppendLine("            }")
            .AppendLine("            finally { ArrayPool<long>.Shared.Return(locations); }")
            .AppendLine("        }")
            .AppendLine($"        private static void WriteRow(BinaryWriter writer, {type} row)")
            .AppendLine("        {");
        foreach (FieldSchema field in schema.Fields)
        {
            sb.AppendLine("            " + values.Write(field.Type,
                "row." + (field.Type.ReferenceTable != null ? helper(
                "ZrReferenceId" + field.Name) : "@" + field.Name)));
        }

        sb.AppendLine("        }")
            .AppendLine("        public IConfigRowReader Open(byte[] bytes)")
            .AppendLine("        {")
            .AppendLine("            using var input = new MemoryStream(bytes, false);")
            .AppendLine("            using var header = new BinaryReader(input, Encoding.UTF8, true);")
            .AppendLine("            int count = BinaryFormat.ReadHeader(header, TableName, SchemaHash);")
            .AppendLine("            ConfigReadContext context = ReadTypes(header);")
            .AppendLine("            var decoder = new RowDecoder(context);")
            .AppendLine($"            return new ConfigRowReader<{type}, {keyType}>(bytes, header, count, decoder.ReadKey,")
            .AppendLine($"                decoder.Read, {key}, {(schema.HasIntegerId ? "s_selectId" : "null")}{TableAccessGenerator.ComparerArgument(schema.KeyFields, "Key")});")
            .AppendLine("        }")
            .AppendLine("        // 显式保存本次读取的上下文，避免匿名函数捕获局部变量。")
            .AppendLine("        private sealed class RowDecoder")
            .AppendLine("        {")
            .AppendLine("            private readonly ConfigReadContext _context;")
            .AppendLine("            public RowDecoder(ConfigReadContext context) { _context = context; }")
            .AppendLine($"            public {keyType} ReadKey(BinaryReader reader)")
            .AppendLine("            {")
            .AppendLine("                ConfigReadContext context = _context;")
            .AppendLine($"                return {readKey};")
            .AppendLine("            }")
            .AppendLine($"            public {type} Read(BinaryReader reader)")
            .AppendLine("            {")
            .AppendLine("                return ReadRow(reader, _context.ForRow());")
            .AppendLine("            }")
            .AppendLine("        }")
            .AppendLine($"        private static {type} ReadRow(BinaryReader reader, ConfigReadContext context)")
            .AppendLine("        {")
            .AppendLine(readFields)
            .AppendLine($"            return {create};")
            .AppendLine("        }")
            .AppendLine("        public IList Read(Stream input)")
            .AppendLine("        {")
            .AppendLine("            using var buffer = new MemoryStream();")
            .AppendLine("            input.CopyTo(buffer);")
            .AppendLine("            IConfigRowReader indexed = Open(buffer.ToArray());")
            .AppendLine($"            var rows = new List<{type}>(indexed.Count);")
            .AppendLine($"            for (int i = 0; i < indexed.Count; i++) rows.Add(({type})indexed.Read(i));")
            .AppendLine("            return rows;")
            .AppendLine("        }");
        if (jsonSchema != null)
        {
            sb.AppendLine("        // JSON 解析器按首次使用初始化，二进制查询不承担其分配与解析成本。")
                .AppendLine("        private static class JsonSupport")
                .AppendLine("        {")
                .AppendLine($"            internal static readonly ConfigWire Wire = new ConfigWire({JsonSerializer.Serialize(jsonSchema)});")
                .AppendLine("        }")
                .AppendLine("        public byte[] JsonToBinary(byte[] json) => JsonSupport.Wire.JsonToBinary(json);");
        }
        values.Append(sb);
        sb.AppendLine($"        private static readonly Func<{type}, {keyType}> s_selectKey = GetKey;")
            .AppendLine($"        private static {keyType} GetKey({type} row)")
            .AppendLine("        {")
            .AppendLine($"            return {Join(schema.KeyFields.Select(f => "row.@" + f.Name))};")
            .AppendLine("        }");

        if (schema.HasIntegerId)
        {
            sb.AppendLine($"        private static readonly Func<{type}, int> s_selectId = GetId;")
                .AppendLine($"        private static int GetId({type} row) {{ return row.@Id; }}");
        }

        TableAccessGenerator.AppendComparer(sb, schema.KeyFields, "Key");
        FieldSchema[] references = schema.Fields.Where(f => f.Type.ReferenceTable != null).ToArray();
        if (references.Length != 0)
        {
            sb.AppendLine($"        public void ResolveReferences(IConfigRow row, ConfigReferenceResolver resolver) {{ (({type})row).{helper( "ResolveReferences")}(resolver); }}")
                .AppendLine($"        public void BindReferences(IConfigRow row, ConfigReferenceResolver resolver) {{ (({type})row).{helper( "BindReferences")}(resolver); }}")
                .AppendLine("        public void CollectReferences(IConfigRow value, ICollection<ConfigRowReference> references)")
                .AppendLine("        {")
                .AppendLine($"            var row = ({type})value;");
            foreach (FieldSchema field in references)
            {
                string id = "row." + helper( "ZrReferenceId" + field.Name);
                sb.AppendLine($"            if ({id} != 0) references.Add(new ConfigRowReference(typeof(global::ZrConfig.Models.{field.Type.ReferenceTable}Config), {id}, row, nameof(row.@{field.Name})));");
            }

            sb.AppendLine("        }");
        }

        sb.AppendLine("    }");
    }

    private static string GenerateRegistry(IReadOnlyList<TableSchema> schemas, bool serverApi, string target, GenerationSourceContract sourceContract)
    {
        const int BatchSize = 128;
        int batches = (schemas.Count + BatchSize - 1) / BatchSize;
        StringBuilder sb = new();
        sb.AppendLine("// <auto-generated />")
            .AppendLine("#nullable enable")
            .AppendLine("// 此文件由工具自动生成，请勿手工修改。")
            .AppendLine("using System.Collections.Generic;")
            .AppendLine("using ZrConfig.Runtime;")
            .AppendLine("[assembly: global::System.Reflection.AssemblyMetadata("+JsonSerializer.Serialize(GenerationSourceContract.MetadataKey)+", "+JsonSerializer.Serialize(JsonSerializer.Serialize(sourceContract))+")]" )
            .AppendLine("namespace ZrConfig.Models")
            .AppendLine("{")
            .AppendLine("    public static class TableCodecs")
            .AppendLine("    {")
            .AppendLine("        private static readonly ITableCodecRegistry s_catalog = new GeneratedCatalog();")
            .AppendLine("        // 可选的全表目录；取得此属性不会创建任何表的编解码器。")
            .AppendLine("        public static ITableCodecRegistry Registry")
            .AppendLine("        {")
            .AppendLine("            get { return s_catalog; }")
            .AppendLine("        }")
            .AppendLine("        // 仅在工具导出、全量校验时枚举全部表。")
            .AppendLine("        public static IEnumerable<ITableCodec> EnumerateAll()")
            .AppendLine("        {")
            .AppendLine("            return s_catalog.EnumerateCodecs();")
            .AppendLine("        }")
            .AppendLine("        private sealed class GeneratedCatalog : ITableCodecRegistry")
            .AppendLine("        {")
            .AppendLine("            public IEnumerable<ITableCodec> EnumerateCodecs()")
            .AppendLine("            {");
        for (int i = 0; i < batches; i++)
        {
            sb.AppendLine($"                foreach (ITableCodec codec in EnumeratePart{i:D4}()) yield return codec;");
        }

        sb.AppendLine("                yield break;").AppendLine("            }");
        for (int i = 0; i < batches; i++)
        {
            sb.AppendLine("            // 每段最多枚举 128 张表，避免生成过大的方法体。")
                .AppendLine($"            private static IEnumerable<ITableCodec> EnumeratePart{i:D4}()")
                .AppendLine("            {");
            for (int j = i * BatchSize; j < Math.Min((i + 1) * BatchSize, schemas.Count); j++)
            {
                sb.AppendLine($"                yield return {schemas[j].Name}ConfigTable.GetCodec();");
            }

            sb.AppendLine("            }");
        }

        sb.AppendLine("        }").AppendLine("    }");
        if (!serverApi)
        {
            sb.AppendLine("}");
            return GeneratedCodeStyle.Format(sb.ToString());
        }
        RuntimeCatalogDescriptor catalog = RuntimeCatalog.Create(schemas.Select(s => (s.Name, s.SchemaHash,
            s.Fields.Where(f => f.Type.ReferenceTable != null).Select(f => f.Type.ReferenceTable!).Distinct(StringComparer.Ordinal).ToArray())));
        sb.AppendLine("    public static class ServerCatalog")
            .AppendLine("    {")
            .AppendLine($"        private static readonly System.Lazy<ZrConfig.Runtime.Server.Catalog> s_catalog = new System.Lazy<ZrConfig.Runtime.Server.Catalog>(() => new ZrConfig.Runtime.Server.Catalog(\"{catalog.Fingerprint}\", Enumerate(), \"server-v1\", new string[] {{ {string.Join(", ", catalog.RequiredCapabilities.Select(c => System.Text.Json.JsonSerializer.Serialize(c)))} }}, \"{target}\"), true);")
            .AppendLine("        public static ZrConfig.Runtime.Server.Catalog Instance => s_catalog.Value;")
            .AppendLine("        private static IEnumerable<ZrConfig.Runtime.Server.TableDescriptor> Enumerate()")
            .AppendLine("        {");
        for (int i = 0; i < batches; i++) sb.AppendLine($"            foreach (var table in Part{i:D4}()) yield return table;");
        sb.AppendLine("            yield break;").AppendLine("        }");
        for (int i = 0; i < batches; i++)
        {
            sb.AppendLine($"        private static IEnumerable<ZrConfig.Runtime.Server.TableDescriptor> Part{i:D4}() {{");
            foreach (RuntimeTableDescriptor table in catalog.Tables.Skip(i * BatchSize).Take(BatchSize))
                sb.AppendLine($"            yield return new ZrConfig.Runtime.Server.TableDescriptor(\"{table.Name}\", \"{table.SchemaHash}\", typeof({table.Name}Config), {table.Name}ConfigTable.GetCodec, new string[] {{ {string.Join(", ", table.Dependencies.Select(n => JsonSerializer.Serialize(n)))} }}, {table.Component}, Server.{table.Name}Table.GroupDescriptors);");
            sb.AppendLine("        }");
        }
        sb.AppendLine("    }").AppendLine("}");
        ServerValueGenerator.Append(sb, schemas);
        return GeneratedCodeStyle.Format(sb.ToString());
    }
}
