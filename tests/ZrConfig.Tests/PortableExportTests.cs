using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using ClosedXML.Excel;
using ZrConfig.Core;
using ZrConfig.Runtime;
using ZrConfig.Validation;

// 验收实际生成的源码和正式数据，不使用手工替代的客户端模型。
internal static class PortableExportTests
{
    internal static async Task RunAsync(string parent, bool requireNative = false, bool singleLanguagesOnly = false, bool consumersOnly = false)
    {
        CheckToolResolution(parent);
        if (requireNative)
        {
            ResolveNativeTools(true);
            AdditionalLanguageTests.ResolveTools(true);
        }
        if (!consumersOnly) await CheckStableExports(parent);
        string root = Path.Combine(parent, "portable");
        string data = Path.Combine(root, "Data"), code = Path.Combine(root, "Config"), output = Path.Combine(root, "Output");
        Directory.CreateDirectory(data); Directory.CreateDirectory(code);
        File.WriteAllText(Path.Combine(code, "Types.cs"), "namespace Demo { public enum Quality : ulong { None = 0, Max = ulong.MaxValue } public abstract class Action { public int Delay; } public sealed class Damage : Action { public decimal Amount; } }");
        File.WriteAllText(Path.Combine(code, "Tables.json"), "{\"Entry\":{\"headerRows\":5,\"groups\":{\"Key\":[\"Name\"]}}}");
        string[] fields = ["Zone", "Id", "Name", "Signed", "Unsigned", "Amount", "Optional", "Values", "Mapping", "Shape", "Action", "Quality", "Lookup", "Secret", "Client", "class"];
        string[] types = ["string", "int", "string", "long", "ulong", "decimal", "int?", "json<int?[]>", "json<Dictionary<int,string>>", "json<int[,]>", "json<Demo.Action>", "Demo.Quality", "LookupConfig", "string", "string", "string"];
        string[] first = ["east", "1", "中文😀", long.MinValue.ToString(CultureInfo.InvariantCulture), ulong.MaxValue.ToString(CultureInfo.InvariantCulture), "79228162514264337593543950335", "null", "[null,1,-2]", "{\"7\":\"值\"}", "[[1,2],[3,4]]", "{\"$type\":\"Damage\",\"Delay\":2,\"Amount\":1.25}", "Max", "10", "server-only", "client-only", "keyword"];
        Write("Entry", fields, types, [first, first.Select((v,i) => i == 0 ? "west" : i == 5 ? "-0.00" : v).ToArray()], true);
        Write("Lookup", ["Id", "Name", "Peer"], ["int", "string", "LookupConfig"], [["10", "目标", "20"], ["20", "同伴", "10"]], false);
        // 此表没有多态根字段，父类型仅用于生成继承声明，不得添加二进制标签。
        Write("Concrete", ["Id", "Payload"], ["int", "json<Demo.Damage>"], [["1", "{\"Delay\":7,\"Amount\":2.5}"]], false);
        var settings = new WorkspaceSettings(data, output, code, "") { Target = ConfigTarget.Server, CodeLanguages = ["all"], DataFormat = "both" };
        string project = Path.Combine(root, "portable.zrconfig.json"); settings.SaveProject(project);
        Check(settings == WorkspaceSettings.LoadProject(project), "语言/格式项目配置往返失败。");
        var generated = await ProjectWorkflow.ExportAsync(settings, _ => Task.FromResult(true));
        Check(generated.Success, Describe(generated));
        Check(!ModelGenerator.Prepare(data, code, new DiagnosticCollector(), target: settings.Target, languages: settings.CodeLanguages, dataFormat: settings.DataFormat)!.HasChanges, "同一输入反复生成有差异。");
        if (!consumersOnly)
        {
        await CheckSingleLanguageExports(root, settings);
        string clientRoot = Path.Combine(root, "client"), clientCode = Path.Combine(clientRoot, "Config");
        Directory.CreateDirectory(clientCode);
        foreach (string file in new[] { "Types.cs", "Tables.json" })
            File.Copy(Path.Combine(code, file), Path.Combine(clientCode, file));
        var clientSettings = settings with { Target = ConfigTarget.Client, CodeFolder = clientCode, OutputFolder = Path.Combine(clientRoot, "Output") };
        var clientGenerated = await ProjectWorkflow.ExportAsync(clientSettings, _ => Task.FromResult(true));
        Check(clientGenerated.Success, "客户端单语言测试初始化失败：" + Describe(clientGenerated));
        await CheckSingleLanguageExports(clientRoot, clientSettings);
        }
        if (singleLanguagesOnly) return;
        output = ExportTestPaths.FlatFixture(output, Path.Combine(root, "legacy-flat"));
        foreach (string file in new[] { "EntryConfig.g.cs", "Java/EntryConfig.java", "Go/entry.g.go", "Cpp/EntryConfig.hpp", "Lua/EntryConfig.lua", "TypeScript/EntryConfig.ts", "Rust/EntryConfig.rs" })
        {
            string source = File.ReadAllText(Path.Combine(code, "Generated", file));
            Check(source.Contains("字段说明") && !source.Contains("client-only"), "字段说明或分端生成丢失：" + file);
        }
        Check(File.ReadAllText(Path.Combine(code, "Generated", "EntryConfig.g.cs")).Contains("&lt;中文&gt; &amp;"), "C# XML 注释未转义。");
        using (var book = new XLWorkbook(Path.Combine(data, "Entry.xlsx")))
        {
            book.Worksheet(1).Cell(4, 1).Value = "修改说明不会改变已有接口 // -- \"说明\"";
            book.Save();
        }
        var commentDiagnostics = new DiagnosticCollector();
        ModelGenerationPlan commentPlan = ModelGenerator.Prepare(data, code, commentDiagnostics, target: settings.Target, languages: settings.CodeLanguages, dataFormat: settings.DataFormat)!;
        Check(!commentPlan.HasApiChanges && commentPlan.Apply(commentDiagnostics), "只改说明被误判为七语言 API 变化：" + commentPlan.DescribeChanges());
        var diagnostics = new DiagnosticCollector();
        File.WriteAllText(Path.Combine(code, "ServerConsumer.cs"), ServerRuntimeTests.CSharp);
        using LoadedRules loaded = RuleCompiler.PrepareSources(code, "", Path.Combine(root, "cache"), diagnostics) ?? throw new Exception(string.Join("\n", diagnostics.Snapshot()));
        ITableCodecRegistry registry = (ITableCodecRegistry)loaded.Assembly.GetType("ZrConfig.Models.TableCodecs")!.GetProperty("Registry")!.GetValue(null)!;
        ConfigManifest Decode(string text) => JsonSerializer.Deserialize<ConfigManifest>(text, new JsonSerializerOptions { IncludeFields = true })!;
        var serverCatalog = (ZrConfig.Runtime.Server.Catalog)loaded.Assembly.GetType("ZrConfig.Models.ServerCatalog")!.GetProperty("Instance")!.GetValue(null)!;
        foreach (string language in settings.CodeLanguages)
        {
            string languageRoot = Path.Combine(settings.OutputFolder, ExportOptions.LanguageDirectory(language));
            foreach (bool json in new[] { false, true })
            {
                using var grouped = new ConfigManager(registry, Decode, new ConfigLoadOptions(preferJson: json));
                Check(grouped.ReloadAll(new FileConfigSource(languageRoot)).Tables.Count == 3, "Grouped C# load failed.");
                using var groupedServer = new ZrConfig.Runtime.Server.Manager(serverCatalog, Decode);
                groupedServer.Reload(new FileConfigSource(languageRoot), new ZrConfig.Runtime.Server.PreparationOptions(preferJson: json));
            }
        }
        await ServerRuntimeTests.CheckCSharp(registry, serverCatalog, Decode, output);
        loaded.Assembly.GetType("ServerConsumer")!.GetMethod("Run")!.Invoke(null, [output]);
        File.Delete(Path.Combine(code, "ServerConsumer.cs"));
        using var manager = new ConfigManager(registry, Decode, new ConfigLoadOptions(retainPreviousSnapshots: true, preferJson: true));
        ConfigSnapshot old = manager.ReloadAll(new FileConfigSource(ExportTestPaths.Root(output)));
        IConfigTable table = old.Tables.Single(t => t.Name == "Entry");
        var method = table.GetType().GetMethod("Get", [typeof(string), typeof(int)])!;
        var a = Expression.Parameter(typeof(string)); var b = Expression.Parameter(typeof(int));
        var get = Expression.Lambda<Func<string,int,object>>(Expression.Convert(Expression.Call(Expression.Constant(table), method, a,b), typeof(object)), a,b).Compile();
        object row = get("east",1);
        Check(row.GetType().GetProperty("Client") == null, "服务端模型泄漏客户端字段。");
        object? Field(string name) => row.GetType().GetProperty(name)!.GetValue(row);
        Check((long)Field("Signed")! == long.MinValue && (ulong)Field("Unsigned")! == ulong.MaxValue && (decimal)Field("Amount")! == decimal.MaxValue, "JSON 数值边界丢失。");
        Check(Field("Optional") == null && ((int?[])Field("Values")!)[0] == null && ((int[,])Field("Shape")!)[1,1] == 4, "JSON 集合或空值不一致。");
        var concrete = old.Tables.Single(t => t.Name == "Concrete");
        object concreteRow = concrete.GetType().GetMethod("Get", [typeof(int)])!.Invoke(concrete, [1])!;
        object payload = concreteRow.GetType().GetProperty("Payload")!.GetValue(concreteRow)!;
        Check((int)payload.GetType().GetField("Delay")!.GetValue(payload)! == 7, "具体子类继承字段读取失败。");
        for (int i=0;i<100000;i++) _=get("east",1);
        long start=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<100000;i++) Check(ReferenceEquals(row,get("east",1)), "查询对象未缓存。");
        long allocated=GC.GetAllocatedBytesForCurrentThread()-start;Check(allocated==0,"C# 热查询发生分配："+allocated);
        Console.WriteLine("通过：C# JSON 强类型读取与 100000 次组合键查询，分配 0 B。");
        var manifest = JsonNode.Parse(File.ReadAllText(ExportTestPaths.FilePath(output, "current.json")))!;
        string version = manifest["versionId"]!.GetValue<string>();
        string jsonFile=ExportTestPaths.FilePath(output, "Entry.json");
        byte[] goodJson=File.ReadAllBytes(jsonFile);
        var codec=(IConfigJsonCodec)Activator.CreateInstance(loaded.Assembly.GetTypes().Single(t => typeof(ITableCodec).IsAssignableFrom(t)&&t.Name=="EntryConfigCodec"))!;
        void Bad(Action<JsonNode> corrupt) { var json=JsonNode.Parse(goodJson)!;corrupt(json);Reject(()=>codec.JsonToBinary(System.Text.Encoding.UTF8.GetBytes(json.ToJsonString()))); }
        Bad(n=>n["rows"]![0]!["Amount"]="0.12345678901234567890123456789");
        Bad(n=>n["rows"]![0]!["Signed"]=9223372036854775807L);
        Bad(n=>n["rows"]![0]!["Unknown"]=1);
        Bad(n=>n["rows"]![0]!["Action"]!["$type"]="Unknown");
        Bad(n=>n["rows"]![0]!["Shape"]!["dimensions"]![0]=3);
        Reject(()=>ConfigJson.Parse("{\"x\":1,\"x\":2}")); Reject(()=>ConfigJson.Parse("\"\\uD800\""));
        File.WriteAllText(jsonFile,"broken"); Reject(()=>manager.ReloadAll(new FileConfigSource(ExportTestPaths.Root(output))));
        Check(ReferenceEquals(old,manager.Current),"损坏重载替换了旧快照。"); File.WriteAllBytes(jsonFile,goodJson);
        ConfigSnapshot next=manager.ReloadAll(new FileConfigSource(ExportTestPaths.Root(output))); Check(!ReferenceEquals(next,old)&&old.Tables.Count==3&&ReferenceEquals(row,get("east",1)),"旧快照不能继续查询。");
        manager.Dispose();Check(old.Tables.Count==3,"退休快照受管理器释放影响。");
        foreach(string format in new[]{"json","binary"})
        {
            var exported=ExportPipeline.RunWithSources(data,Path.Combine(root,format),code,"",target:ConfigTarget.Server,dataFormat:format);Check(exported.Success,string.Join("\n",exported.Diagnostics));
            using var verify=new ConfigManager(registry,Decode);Check(verify.ReloadAll(new FileConfigSource(ExportTestPaths.Root(Path.Combine(root,format)))).Tables.Count==3,"单格式加载失败。");
        }
        string native = Path.Combine(code,"Generated");
        string mappedNative = CheckSourceMappings(root,data,code,output);
        await NativeAsync(mappedNative,output,root,requireNative);
        var rejected=new DiagnosticCollector();
        File.WriteAllText(Path.Combine(native,"Go","handwritten.txt"),"保留");
        var removal = ModelGenerator.Prepare(data,code,rejected,target:ConfigTarget.Server,languages:["csharp"])!;
        Check(!removal.Apply(rejected,true) && rejected.Snapshot().Any(d=>d.Code=="API_CHANGE_REQUIRED"), "Deleting a language bypassed API protection.");
        rejected = new DiagnosticCollector();
        Check(GeneratedApiContractTests.AcceptTestMigration(removal, removal.ApiChangeId!) && removal.Apply(rejected,true),string.Join("\n",rejected.Snapshot()));
        Check(File.Exists(Path.Combine(native,"Go","handwritten.txt"))&&!File.Exists(Path.Combine(native,"Go","runtime.go")),"语言变更未清理登记文件或删除了手写文件。");
        File.AppendAllText(Path.Combine(code, "Types.cs"), """
            namespace Demo {
                [ZrConfig.Runtime.ConfigConverter(typeof(MoneyConverter), "1")]
                public struct Money { public int Cents; }
                public sealed class MoneyConverter : ZrConfig.Runtime.IConfigConverter<Money> {
                    public Money Parse(string text) => new Money { Cents = int.Parse(text) };
                    public void Write(System.IO.BinaryWriter writer, Money value) { writer.Write(123); writer.Write(value.Cents); }
                    public Money Read(System.IO.BinaryReader reader) { reader.ReadInt32(); return new Money { Cents = reader.ReadInt32() }; }
                }
            }
            """);
        Write("Converted", ["Id", "Value"], ["int", "Demo.Money"], [["1", "7"]], false);
        var unsupported = new DiagnosticCollector();
        Check(ModelGenerator.Prepare(data,code,unsupported,target:ConfigTarget.Server,languages:["go"])==null && unsupported.Snapshot().Any(d=>d.Code=="PORTABLE_SCHEMA"),"未声明转换器布局却生成了原生代码。");
        Check(ModelGenerator.Generate(data,code,new DiagnosticCollector(),target:ConfigTarget.Server),"旧 C# 二进制转换器兼容失败。");
        string legacy = Path.Combine(root,"legacy");
        Check(ExportPipeline.RunWithSources(data,legacy,code,"",target:ConfigTarget.Server).Success,"旧自定义转换器不能再导出二进制。");
        byte[] before=File.ReadAllBytes(ExportTestPaths.FilePath(legacy, "current.json"));
        string converterSource=File.ReadAllText(Path.Combine(code,"Types.cs")).Replace("public struct Money", "[ZrConfig.Runtime.ConfigWireLayout(nameof(Cents))] public struct Money");
        File.WriteAllText(Path.Combine(code,"Types.cs"),converterSource);
        var converterDiagnostics = new DiagnosticCollector();
        var convertedPlan = ModelGenerator.Prepare(data,code,converterDiagnostics,target:ConfigTarget.Server,languages:["all"])!;
        if (convertedPlan.HasApiChanges) Check(GeneratedApiContractTests.AcceptTestMigration(convertedPlan, convertedPlan.ApiChangeId!), "Converter migration approval failed.");
        Check(convertedPlan.Apply(converterDiagnostics, true),string.Join("\n",converterDiagnostics.Snapshot()));
        Check(!ExportPipeline.RunWithSources(data,legacy,code,"",target:ConfigTarget.Server).Success && before.SequenceEqual(File.ReadAllBytes(ExportTestPaths.FilePath(legacy, "current.json"))),"纯二进制原生发布未拒绝错误转换器布局或覆盖了旧版本。");
        Console.WriteLine("通过：多语言生成、两种数据格式、精度/损坏检查、原子重载、旧快照及生成事务。");

        void Write(string name,string[] names,string[] ts,string[][] rows,bool targets)
        {
            using var book=new XLWorkbook();var sheet=book.AddWorksheet(name);
            for(int i=0;i<names.Length;i++)
            {
                sheet.Cell(1,i+1).Value=names[i];sheet.Cell(2,i+1).Value=ts[i];
                if(i==0||name=="Entry"&&i==1)sheet.Cell(3,i+1).Value="主键";
                if(i!=names.Length-1)sheet.Cell(4,i+1).Value="字段说明 <中文> & \"备注\"\n第二行\\   ";
                sheet.Cell(5,i+1).Value=targets&&names[i]=="Secret"?"S":targets&&names[i]=="Client"?"C":"CS";
                for(int r=0;r<rows.Length;r++)sheet.Cell(r+6,i+1).Value=rows[r][i];
            }
            book.SaveAs(Path.Combine(data,name+".xlsx"));
        }
    }
    internal static async Task CheckStableExports(string parent)
    {
        string root = Path.Combine(parent, "stable-export"), data = Path.Combine(root, "Data"), code = Path.Combine(root, "Config"), rules = Path.Combine(root, "Rules"), output = Path.Combine(root, "Output");
        Directory.CreateDirectory(data);
        Directory.CreateDirectory(rules);
        string bookPath = Path.Combine(data, "Stable.xlsx"), rulePath = Path.Combine(rules, "StableRules.cs");
        const string goodRule = "using ZrConfig.Models; using ZrConfig.Validation; public sealed class StableRules : TableRules<StableConfig> { }";
        File.WriteAllText(rulePath, goodRule);
        using (var book = new XLWorkbook())
        {
            var sheet = book.AddWorksheet("Stable");
            sheet.Cell(1, 1).Value = "Id"; sheet.Cell(2, 1).Value = "int"; sheet.Cell(3, 1).Value = "主键"; sheet.Cell(5, 1).Value = "CS";
            sheet.Cell(1, 2).Value = "Name"; sheet.Cell(2, 2).Value = "string"; sheet.Cell(5, 2).Value = "CS";
            sheet.Cell(6, 1).Value = 1; sheet.Cell(6, 2).Value = "original";
            var untouched = book.AddWorksheet("Untouched");
            untouched.Cell(1, 1).Value = "Id"; untouched.Cell(2, 1).Value = "int"; untouched.Cell(3, 1).Value = "主键"; untouched.Cell(5, 1).Value = "CS"; untouched.Cell(6, 1).Value = 99;
            book.SaveAs(bookPath);
        }
        void Edit(Action<IXLWorksheet> action)
        {
            using var book = new XLWorkbook(bookPath);
            action(book.Worksheet(1)); book.Save();
        }
        var settings = new WorkspaceSettings(data, output, code, rules);
        Check((await ProjectWorkflow.GenerateAsync(settings, _ => Task.FromResult(false))).Success, "稳定导出测试生成失败。");
        string current = Path.Combine(output, "CSharp", "Common", "current.json"), versions = Path.Combine(output, "versions");
        async Task<ExportResult> Export(WorkflowOperation? operation = null)
        {
            var result = await ProjectWorkflow.ExportAsync(settings, _ => Task.FromResult(false), operation: operation);
            Check(result.Success && result.Export != null, "稳定导出失败：" + Describe(result));
            return result.Export!;
        }
        Dictionary<string, (byte[] Bytes, DateTime Written)> Saved() => Directory.GetFiles(Path.Combine(output, "CSharp"), "*", SearchOption.AllDirectories).Where(p => Path.GetExtension(p) is ".bytes" or ".json").ToDictionary(p => p, p => (File.ReadAllBytes(p), File.GetLastWriteTimeUtc(p)));
        void Unchanged(Dictionary<string, (byte[] Bytes, DateTime Written)> saved)
        {
            Check(Saved().Count == saved.Count && saved.All(p => File.Exists(p.Key) && File.ReadAllBytes(p.Key).SequenceEqual(p.Value.Bytes) && File.GetLastWriteTimeUtc(p.Key) == p.Value.Written),
                "相同内容导出改写了当前清单、版本文件或时间戳。");
            Check(!Directory.Exists(versions), "固定目录导出仍创建版本目录。");
        }
        async Task Repeat(string version)
        {
            var saved = Saved();
            var repeated = await Export();
            Check(repeated.ReusedVersion && !repeated.Published && repeated.VersionId == version, "相同内容仍创建了新版本。");
            Unchanged(saved);
            using var run = JsonDocument.Parse(File.ReadAllText(Path.Combine(repeated.RunDirectory!, "run.json")));
            Check(run.RootElement.GetProperty("reusedVersion").GetBoolean() && !run.RootElement.GetProperty("published").GetBoolean() && run.RootElement.GetProperty("versionId").GetString() == version,
                "复用版本的运行记录不准确。");
            using var diagnostic = JsonDocument.Parse(File.ReadAllText(repeated.DiagnosticFile));
            Check(diagnostic.RootElement.GetProperty("versionId").GetString() == version, "复用版本丢失诊断清单版本。");
        }
        foreach (string format in new[] { "binary", "json", "both" })
        {
            settings = settings with { DataFormat = format };
            var first = await Export();
            Check(first.Published && !first.ReusedVersion, "首次导出/格式切换没有发布新版本。");
            await Repeat(first.VersionId!);
            await Repeat(first.VersionId!);
            Edit(sheet => sheet.Column(2).Width += 1);
            await Repeat(first.VersionId!);
            var originalFiles = Saved();
            string originalName;
            using (var book = new XLWorkbook(bookPath)) originalName = book.Worksheet(1).Cell(6, 2).GetString();
            Edit(sheet => sheet.Cell(6, 2).Value = format + "-changed");
            var unchangedFiles = Saved().Where(p => Path.GetFileName(p.Key).StartsWith("Untouched.", StringComparison.Ordinal)).ToDictionary();
            var changed = await Export();
            Check(changed.UpdatedTables == 1 && unchangedFiles.All(p => File.ReadAllBytes(p.Key).SequenceEqual(p.Value.Bytes) && File.GetLastWriteTimeUtc(p.Key) == p.Value.Written), "只改一张表重写了其他表。");
            Check(changed.Published && changed.VersionId != first.VersionId, "数据变化被错误复用。");
            await Repeat(changed.VersionId!);
            Edit(sheet => sheet.Cell(6, 2).Value = originalName);
            var reverted = await Export();
            Check(reverted.VersionId == first.VersionId && originalFiles.All(p => File.ReadAllBytes(p.Key).SequenceEqual(p.Value.Bytes)), "恢复原数据未得到相同清单内容，会产生缓存身份冲突。");
            Edit(sheet => sheet.Cell(6, 2).Value = format + "-changed");
            await Export();
            var manifest = ReleaseHistory.TryReadCurrent(output)!;
            var table = manifest.Tables.Single(t => t.Name == "Stable");
            string damaged = ExportTestPaths.FilePath(output, table.JsonFile ?? table.File);
            File.WriteAllText(damaged, "broken");
            var repaired = await Export();
            Check(repaired.Published && repaired.VersionId == changed.VersionId && File.ReadAllText(damaged) != "broken" && repaired.UpdatedTables == 1, "损坏数据未通过新版本修复或覆盖了旧版本。");
            await Repeat(repaired.VersionId!);
            manifest = ReleaseHistory.TryReadCurrent(output)!;
            File.Delete(ExportTestPaths.FilePath(output, manifest.Tables.Single(t => t.Name == "Stable").File));
            var restored = await Export();
            Check(restored.Published && restored.VersionId == repaired.VersionId && restored.UpdatedTables == 1, "缺失表文件被错误复用。");
            await Repeat(restored.VersionId!);
            Console.WriteLine($"通过：{format} 固定路径、未改表时间戳不变、只更新改动表、格式切换及损坏/缺失修复。");
        }
        string active = ReleaseHistory.TryReadCurrent(output)!.VersionId;
        var before = Saved();
        using (var cancellation = new CancellationTokenSource())
        {
            var operation = new WorkflowOperation(cancellation.Token, new CallbackProgress<OperationProgress>(p => { if (p.Stage == "unchanged") cancellation.Cancel(); }));
            var cancelled = await ProjectWorkflow.ExportAsync(settings, _ => Task.FromResult(false), operation: operation);
            Check(cancelled.Cancelled && !cancelled.Success && cancelled.Export is { ReusedVersion: false, Published: false }, "版本复用忽略取消。");
            Unchanged(before);
        }
        var changing = new WorkflowOperation(progress: new CallbackProgress<OperationProgress>(p => { if (p.Stage == "unchanged") Edit(sheet => sheet.Column(2).Width += 1); }));
        var interrupted = await ProjectWorkflow.ExportAsync(settings, _ => Task.FromResult(false), operation: changing);
        Check(!interrupted.Success, "版本复用忽略了运行期间的输入变化。");
        Unchanged(before);
        File.WriteAllText(rulePath, "using ZrConfig.Models; using ZrConfig.Validation; public sealed class StableRules : TableRules<StableConfig> { protected override void Validate(StableConfig row, ValidationContext context) { context.Error(row, nameof(StableConfig.Name), \"blocked\", \"STABLE_RULE_ERROR\"); } }");
        var blocked = await ProjectWorkflow.ExportAsync(settings, _ => Task.FromResult(false));
        Check(!blocked.Success && blocked.Export!.Diagnostics.Any(d => d.Code == "STABLE_RULE_ERROR"), "相同数据绕过了本轮规则检查。");
        Unchanged(before);
        File.WriteAllText(rulePath, goodRule + " // updated validation source");
        await Repeat(active);
        string project = Path.Combine(root, "project.zrconfig.json");
        settings.SaveProject(project);
        using var cli = new StringWriter();
        using (ConfigText.UseLanguage(ConfigText.Chinese))
            Check(ToolCommandRunner.Run(["export", "--project", project], cli, cli) == 0 && cli.ToString().Contains("数据未变化", StringComparison.Ordinal), "CLI 未报告稳定导出结果：" + cli);
        Unchanged(before);
        File.WriteAllText(Path.Combine(code, "Notes.cs"), "// changed model source fingerprint");
        var codeChanged = await Export();
        Check(codeChanged.Published && codeChanged.VersionId != active, "代码兼容信息变化仍复用旧版本。");
        await Repeat(codeChanged.VersionId!);
        File.WriteAllText(current, "invalid-json");
        var manifestRepaired = await Export();
        Check(manifestRepaired.Published, "损坏入口清单未恢复。");
        await Repeat(manifestRepaired.VersionId!);
        before = Saved();
        Edit(sheet => sheet.Cell(6, 2).Value = "rollback");
        using (var cancellation = new CancellationTokenSource())
        {
            var operation = new WorkflowOperation(cancellation.Token, new CallbackProgress<OperationProgress>(p => { if (p.Stage == "preparing-output") cancellation.Cancel(); }));
            var cancelled = await ProjectWorkflow.ExportAsync(settings, _ => Task.FromResult(false), operation: operation);
            Check(cancelled.Cancelled && !cancelled.Success, "写入前取消未生效。");
            Unchanged(before);
        }
        var failed = await ProjectWorkflow.ExportAsync(settings, _ => Task.FromResult(false), operation: new WorkflowOperation { ExportFileOperations = new FailingExportWrite() });
        Check(!failed.Success, "写入故障未报告失败。");
        Unchanged(before);
        await Export();
        // Migrate an existing flat export without leaving a second managed data tree.
        var groupedManifest = ReleaseHistory.TryReadCurrent(output)!;
        foreach (var entry in groupedManifest.Tables)
            foreach (string file in new[] { entry.File, entry.JsonFile }.OfType<string>())
                File.Move(ExportTestPaths.FilePath(output, file), Path.Combine(output, file));
        File.Delete(current);
        File.WriteAllText(Path.Combine(output, "current.json"), JsonSerializer.Serialize(groupedManifest with { Layout = "flat" }, ReleaseHistory.JsonOptions));
        File.WriteAllText(Path.Combine(output, "unmanaged.json"), "user data");
        await Export();
        Check(!File.Exists(Path.Combine(output, "current.json")) && !File.Exists(Path.Combine(output, "Stable.bytes")) && File.ReadAllText(Path.Combine(output, "unmanaged.json")) == "user data", "旧 flat 布局迁移残留受管文件或删除了用户文件。");
        // 模拟升级前的发布：固定目录首次导出保留旧版本及用户文件。
        var legacy = ReleaseHistory.TryReadCurrent(output)!;
        string legacyFolder = Path.Combine(versions, legacy.VersionId);
        Directory.CreateDirectory(legacyFolder);
        foreach (var file in Saved().Where(p => p.Key != current)) File.Move(file.Key, Path.Combine(legacyFolder, Path.GetFileName(file.Key)));
        File.Delete(current);
        File.WriteAllText(Path.Combine(output, "current.json"), JsonSerializer.Serialize(legacy with { Layout = null }, ReleaseHistory.JsonOptions));
        File.WriteAllText(Path.Combine(output, "user.txt"), "keep");
        var oldFiles = Directory.GetFiles(legacyFolder).ToDictionary(p => p, File.ReadAllBytes);
        var migrated = await Export();
        Check(migrated.UpdatedTables == 2 && !File.Exists(Path.Combine(output, "current.json")) && ReleaseHistory.TryReadCurrent(output)!.Layout == "grouped" && oldFiles.All(p => File.ReadAllBytes(p.Key).SequenceEqual(p.Value)) && File.ReadAllText(Path.Combine(output, "user.txt")) == "keep", "旧版本迁移未保留旧数据/用户文件。");
        Directory.Delete(versions, true); // 仅清理本测试建立的旧版夹具。
        settings = settings with { Target = ConfigTarget.Server, CodeFolder = Path.Combine(root, "ServerConfig") };
        Check((await ProjectWorkflow.GenerateAsync(settings, _ => Task.FromResult(false))).Success, "S 端生成失败。");
        var server = await Export();
        Check(server.Published && ReleaseHistory.TryReadCurrent(output)!.Target == "S", "切换目标端仍复用 C 端版本。");
        await Repeat(server.VersionId!);
        Console.WriteLine("通过：固定目录导出的 CLI、规则重检、取消、故障回滚、旧版迁移、C/S 切换及清单修复。");
    }

    private sealed class FailingExportWrite : GenerationFileOperations
    {
        private int writes;
        internal override void Write(string path, byte[] bytes, bool restoring)
        {
            base.Write(path, bytes, restoring);
            if (!restoring && ++writes == 2) throw new IOException("Injected partial export failure");
        }
    }

    private static async Task CheckSingleLanguageExports(string root, WorkspaceSettings original)
    {
        string target = ConfigTargets.Marker(original.Target);
        string[] formats = ["binary", "json", "both"];
        var saved = Directory.GetFiles(Path.Combine(original.CodeFolder, "Generated"), "*", SearchOption.AllDirectories)
            .Append(Path.Combine(original.CodeFolder, ".schema.json"))
            .Append(Path.Combine(original.CodeFolder, ".generation.json"))
            .ToDictionary(path => path, File.ReadAllBytes);
        foreach (string language in original.CodeLanguages.OrderBy(language => language != "csharp"))
        {
            string folder = Path.Combine(root, "single-languages", language);
            var settings = original with { CodeLanguages = [language], OutputFolder = Path.Combine(folder, "Output") };
            bool generationConfirmed = false;
            var generation = await ProjectWorkflow.GenerateAsync(settings, _ =>
            {
                generationConfirmed = true;
                return Task.FromResult(false);
            });
            Check(generation.Success && generation.CodeCommitted && !generationConfirmed && generation.Plan is { HasApiChanges: false, RemovedFiles.Count: 0 },
                "选择单一语言生成不应删除其他语言或要求 API 迁移：" + language);
            Check(saved.All(file => File.Exists(file.Key) && File.ReadAllBytes(file.Key).SequenceEqual(file.Value)),
                "单语言重复生成改写了原有源码或 API 基线：" + language);
            foreach (string format in formats)
            {
                bool confirmed = false;
                var exported = await ProjectWorkflow.ExportAsync(settings with { DataFormat = format }, _ =>
                {
                    confirmed = true;
                    return Task.FromResult(false);
                });
                Check(exported.Success && exported.Export?.Published == true && !exported.CodeCommitted && !confirmed,
                    $"选择 {language}/{format} 导出不应删除已有语言或要求 API 迁移：" + Describe(exported));
                string languageRoot = Path.Combine(settings.OutputFolder, ExportOptions.LanguageDirectory(language));
                var manifest = ReleaseHistory.TryReadCurrent(languageRoot)!;
                Check(manifest.Layout == "grouped" && File.Exists(Path.Combine(languageRoot, "Common", "current.json")), "清单未放入语言/Common。");
                foreach (var entry in manifest.Tables)
                    foreach (string file in new[] { entry.File, entry.JsonFile }.OfType<string>())
                        Check(File.Exists(Path.Combine(languageRoot, "Tables", file.EndsWith(".json", StringComparison.Ordinal) ? "json" : "bytes", file)), "表文件未按格式归类：" + file);
                Check(!Directory.Exists(Path.Combine(languageRoot, "versions")) && Directory.GetFiles(languageRoot).Length == 0, "语言根目录残留散落数据文件。");
                var timestamps = Directory.GetFiles(languageRoot, "*", SearchOption.AllDirectories).ToDictionary(p => p, File.GetLastWriteTimeUtc);
                var repeat = await ProjectWorkflow.ExportAsync(settings with { DataFormat = format }, _ => Task.FromResult(false));
                Check(repeat.Export is { ReusedVersion: true, UpdatedTables: 0 } && timestamps.All(p => File.GetLastWriteTimeUtc(p.Key) == p.Value), "单语言重复导出改写文件。");
                var validated = await ProjectWorkflow.ValidateAsync(settings with { DataFormat = format });
                Check(validated.Success && !validated.NeedsGeneration, "单语言导出后校验失败：" + Describe(validated));
                Check(saved.All(file => File.Exists(file.Key) && File.ReadAllBytes(file.Key).SequenceEqual(file.Value)),
                    "单语言导出或校验改写了已有生成源码/API 基线。");
            }

            string languageFolder = ExportOptions.LanguageDirectory(language);
            Check(Directory.GetDirectories(settings.OutputFolder).Where(p => Path.GetFileName(p) != "Logs" && !Path.GetFileName(p).StartsWith(".", StringComparison.Ordinal)).Select(Path.GetFileName).SequenceEqual(new[] { languageFolder }), "单选导出创建了其他语言目录。");
            var otherLanguages = original.CodeLanguages.Where(l => l != language).SelectMany(l => Directory.GetFiles(Path.Combine(original.OutputFolder, ExportOptions.LanguageDirectory(l)), "*", SearchOption.AllDirectories))
                .ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Written: File.GetLastWriteTimeUtc(p)));
            var selected = original with { CodeLanguages = [language], DataFormat = "json" };
            Check((await ProjectWorkflow.ExportAsync(selected, _ => Task.FromResult(false))).Success, "已有多语言目录切换单选失败。");
            Check(otherLanguages.All(p => File.ReadAllBytes(p.Key).SequenceEqual(p.Value.Bytes) && File.GetLastWriteTimeUtc(p.Key) == p.Value.Written), "单语言导出修改了未选语言的数据或清单。");
            Check((await ProjectWorkflow.ExportAsync(original with { CodeLanguages = [language] }, _ => Task.FromResult(false))).Success, "恢复所选语言格式失败。");

            string code = Path.Combine(folder, "Config");
            Directory.CreateDirectory(code);
            foreach (string file in new[] { "Types.cs", "Tables.json" })
                File.Copy(Path.Combine(original.CodeFolder, file), Path.Combine(code, file));
            var fresh = settings with
            {
                CodeFolder = code,
                Namespaces = new GenerationNamespaces(Go: "example.invalid/single", TypeScript: "single-language")
            };
            var firstGeneration = await ProjectWorkflow.GenerateAsync(fresh, _ => Task.FromResult(false));
            Check(firstGeneration.Success && firstGeneration.CodeCommitted, "首次单语言生成失败：" + Describe(firstGeneration));
            var freshFiles = Directory.GetFiles(Path.Combine(code, "Generated"), "*", SearchOption.AllDirectories)
                .Append(Path.Combine(code, ".schema.json")).Append(Path.Combine(code, ".generation.json"))
                .ToDictionary(path => path, File.ReadAllBytes);
            foreach (string format in formats)
            {
                var formatSettings = fresh with { DataFormat = format };
                var repeated = await ProjectWorkflow.GenerateAsync(formatSettings, _ => Task.FromResult(false));
                Check(repeated.Success && repeated.Plan is { HasChanges: false }, $"{target}/{language}/{format} 重复生成失败或生成内容不稳定：" + Describe(repeated));
                var exported = await ProjectWorkflow.ExportAsync(formatSettings, _ => Task.FromResult(false));
                Check(exported.Success && !exported.CodeCommitted && exported.Export?.Published == true, "首次单语言导出失败：" + Describe(exported));
                Check((await ProjectWorkflow.ValidateAsync(formatSettings)).Success, $"{target}/{language}/{format} 单语言生成后校验失败。");
                Check(freshFiles.All(file => File.Exists(file.Key) && File.ReadAllBytes(file.Key).SequenceEqual(file.Value)),
                    $"{target}/{language}/{format} 重复生成/导出/校验改写了已有源码或 API 基线。");
            }
            string[] nativeFolders = Directory.GetDirectories(Path.Combine(code, "Generated")).Select(Path.GetFileName).ToArray()!;
            string? expected = language switch
            {
                "csharp" => null, "cpp" => "Cpp", "go" => "Go", "java" => "Java",
                "lua" => "Lua", "rust" => "Rust", "typescript" => "TypeScript", _ => throw new Exception(language)
            };
            Check(expected == null ? nativeFolders.Length == 0 : nativeFolders.SequenceEqual(new[] { expected }),
                "单语言生成产生了未选择语言的目录：" + language + ": " + string.Join(",", nativeFolders));
            using var languageLog = new StringWriter();
            Check(ToolCommandRunner.Run(["codegen", original.DataFolder, original.CodeFolder, "--target", target, "--languages", language], languageLog, languageLog) == 0,
                $"CLI {target}/{language} 单语言生成失败：" + languageLog);
            Check(saved.All(file => File.ReadAllBytes(file.Key).SequenceEqual(file.Value)), "CLI 单语言生成改写了已有源码/API 基线。");
            Console.WriteLine($"通过：{target}/{language} 首次生成、重复生成、多选转单选、CLI 生成、三种格式导出/校验及其他语言保留。");
        }
        using var cliLog = new StringWriter();
        Check(ToolCommandRunner.Run(["codegen", original.DataFolder, original.CodeFolder, "--target", target, "--languages", "csharp", "--prune-languages", "--accept-changes"], cliLog, cliLog) == 1 &&
            cliLog.ToString().Contains("API_CHANGE_REQUIRED", StringComparison.Ordinal), "显式清理语言绕过了 API 迁移保护。");
        Check(saved.All(file => File.ReadAllBytes(file.Key).SequenceEqual(file.Value)), "未批准语言清理仍改写了生成源码/API 基线。");
        Check(ToolCommandLine.Parse(["codegen", "--project", "project.zrconfig.json", "--prune-languages"]).PruneLanguages,
            "项目模式丢失显式语言清理参数。");
        Reject(() => ToolCommandLine.Parse(["export", "--prune-languages"]));
        Reject(() => ToolCommandLine.Parse(["validate", "--prune-languages"]));
        Reject(() => ToolCommandLine.Parse(["codegen", "--prune-languages", "--prune-languages"]));
        Console.WriteLine($"通过：{target} 端七语言单选完整矩阵及显式语言清理保护。");
    }

    private static string CheckSourceMappings(string root,string originalData,string originalCode,string originalOutput)
    {
        string mapped=Path.Combine(root,"source-mapped"),data=Path.Combine(mapped,"Data"),code=Path.Combine(mapped,"Config");
        Directory.CreateDirectory(data);Directory.CreateDirectory(code);
        foreach(string file in Directory.GetFiles(originalData,"*.xlsx"))File.Copy(file,Path.Combine(data,Path.GetFileName(file)));
        foreach(string file in Directory.GetFiles(originalCode,"*",SearchOption.AllDirectories))
        {
            string relative=Path.GetRelativePath(originalCode,file);
            if(relative.Split(Path.DirectorySeparatorChar).Any(p=>p.Contains("cache",StringComparison.Ordinal)||p is "bin" or "obj")||relative.EndsWith(".lock",StringComparison.Ordinal))continue;
            string target=Path.Combine(code,relative);Directory.CreateDirectory(Path.GetDirectoryName(target)!);File.Copy(file,target);
        }
        GeneratedApiContract Baseline()=>JsonSerializer.Deserialize<SchemaSummary>(File.ReadAllText(Path.Combine(code,".schema.json")))!.ApiContract!;
        GeneratedApiContract before=Baseline();
        using(var book=new XLWorkbook(Path.Combine(data,"Entry.xlsx")))
        {
            var sheet=book.Worksheet(1);for(int row=1;row<=sheet.LastRowUsed()!.RowNumber();row++){var first=sheet.Cell(row,1).Value;sheet.Cell(row,1).Value=sheet.Cell(row,2).Value;sheet.Cell(row,2).Value=first;}book.Save();
        }
        var diagnostics=new DiagnosticCollector();
        var reordered=ModelGenerator.Prepare(data,code,diagnostics,target:ConfigTarget.Server,languages:["all"],dataFormat:"both");
        Check(reordered!=null&&!reordered.HasChanges,"Moving source columns changed accepted API/order: "+reordered?.DescribeChanges()+string.Join("\n",diagnostics.Snapshot()));
        using(var book=new XLWorkbook(Path.Combine(data,"Entry.xlsx"))){book.Worksheet(1).Cell(1,3).Value="Label";book.Worksheet(1).Cell(2,13).Value="TargetsConfig";book.Save();}
        using(var book=new XLWorkbook(Path.Combine(data,"Lookup.xlsx"))){book.Worksheet(1).Name="Targets";book.Worksheet(1).Cell(2,3).Value="TargetsConfig";book.Save();}
        void Remap(SourceNameMapping[] mappings)
        {
            var plan=ModelGenerator.PrepareWithSourceMappings(mappings,null,"auto",data,code,diagnostics,target:ConfigTarget.Server,languages:["all"],dataFormat:"both");
            Check(plan!=null&&plan.HasApiChanges,"Explicit source mapping was not reviewed: "+string.Join("\n",diagnostics.Snapshot()));
            Check(!plan!.Apply(new DiagnosticCollector(),true),"Ordinary confirmation accepted a source identity change.");
            // A unit receipt tests the transaction; unchanged real consumers run below in all seven toolchains.
            Check(GeneratedApiContractTests.AcceptTestMigration(plan,plan.ApiChangeId!)&&plan.Apply(diagnostics,true),string.Join("\n",diagnostics.Snapshot()));
        }
        Remap([new("Lookup",null,"Targets"),new("Entry","Name","Label")]);
        using(var book=new XLWorkbook(Path.Combine(data,"Entry.xlsx"))){book.Worksheet(1).Cell(1,3).Value="DisplayName";book.Save();}
        Remap([new("Entry","Name","DisplayName")]);
        GeneratedApiContract after=Baseline();
        Check(before.Symbols.SequenceEqual(after.Symbols)&&before.CompiledSymbols!.SequenceEqual(after.CompiledSymbols!)&&before.NativeFiles.SequenceEqual(after.NativeFiles)&&before.Policy!.Names.SequenceEqual(after.Policy!.Names),"Source rename changed public API, source binding or stable SymbolId.");
        Check(!ModelGenerator.Prepare(data,code,diagnostics,target:ConfigTarget.Server,languages:["all"],dataFormat:"both")!.HasChanges,"Accepted source mapping not inherited.");
        foreach(var invalid in new[]{new[]{new SourceNameMapping("Entry","Missing","X")},new[]{new SourceNameMapping("Entry","Name","Id")},new[]{new SourceNameMapping("Entry","Name","X"),new SourceNameMapping("Entry","Name","Y")}})
            Check(ModelGenerator.PrepareWithSourceMappings(invalid,null,"auto",data,code,new DiagnosticCollector(),target:ConfigTarget.Server,languages:["all"])==null,"Unknown or ambiguous source mapping was accepted.");
        string output=Path.Combine(mapped,"Output");var exported=ExportPipeline.RunWithSources(data,output,code,"",target:ConfigTarget.Server,dataFormat:"both");Check(exported.Success,string.Join("\n",exported.Diagnostics));
        var original=JsonNode.Parse(File.ReadAllText(ExportTestPaths.FilePath(originalOutput, "current.json")))!;var current=JsonNode.Parse(File.ReadAllText(ExportTestPaths.FilePath(output, "current.json")))!;
        Check(original["tables"]!.ToJsonString()==current["tables"]!.ToJsonString(),"Source rename/reorder changed table identity, bytes or hashes.");
        using var compiled=RuleCompiler.PrepareSources(code,"",Path.Combine(mapped,"cache"),diagnostics)??throw new Exception(string.Join("\n",diagnostics.Snapshot()));
        var assemblyExport=ExportPipeline.Run(data,Path.Combine(mapped,"AssemblyOutput"),compiled.Assembly,ConfigTarget.Server,"both");Check(assemblyExport.Success,string.Join("\n",assemblyExport.Diagnostics));
        Check(!ModelGenerator.PrepareWithSourceMappings([],null,"auto",data,code,new DiagnosticCollector(),target:ConfigTarget.Server,languages:["all"],dataFormat:"both")!.HasChanges,"An empty mapping request removed accepted mappings.");
        using(var book=new XLWorkbook(Path.Combine(data,"Entry.xlsx"))){book.Worksheet(1).Cell(1,3).Value="Name";book.Worksheet(1).Cell(2,13).Value="LookupConfig";book.Save();}
        using(var book=new XLWorkbook(Path.Combine(data,"Lookup.xlsx"))){book.Worksheet(1).Name="Lookup";book.Worksheet(1).Cell(2,3).Value="LookupConfig";book.Save();}
        SourceNameMapping[] reset=[new("Lookup",null,"Lookup"),new("Entry","Name","Name")];
        var resetPlan=ModelGenerator.PrepareWithSourceMappings(reset,null,"auto",data,code,new DiagnosticCollector(),target:ConfigTarget.Server,languages:["all"],dataFormat:"both")!;
        Check(resetPlan.HasApiChanges,"Mapping reset bypassed review.");
        string mappingFile=Path.Combine(mapped,"reset-mappings.json"),receiptFile=Path.Combine(mapped,"reset-unit-receipt.json");
        File.WriteAllText(mappingFile,JsonSerializer.Serialize(reset));File.WriteAllText(receiptFile,JsonSerializer.Serialize(GeneratedApiContractTests.CreateTestReceipt(resetPlan)));
        string[] command=["codegen",data,code,"--target","S","--languages","all","--format","both","--source-map",mappingFile,"--accept-api-change",resetPlan.ApiChangeId!,"--migration-receipt",receiptFile];
        Check(ToolCommandLine.Parse(command).SourceMappingsFile==mappingFile,"CLI source-map option lost.");
        using var log=new StringWriter();Check(ToolCommandRunner.Run(command,log,log)==0,log.ToString());
        Check((Baseline().Policy!.SourceMappings?.Length??0)==0&&before.Symbols.SequenceEqual(Baseline().Symbols),"Explicit mapping reset changed API or retained aliases.");
        var resetExport=ExportPipeline.RunWithSources(data,Path.Combine(mapped,"ResetOutput"),code,"",target:ConfigTarget.Server,dataFormat:"both");Check(resetExport.Success,string.Join("\n",resetExport.Diagnostics));
        var resetManifest=JsonNode.Parse(File.ReadAllText(ExportTestPaths.FilePath(Path.Combine(mapped,"ResetOutput"),"current.json")))!;
        Check(original["tables"]!.ToJsonString()==resetManifest["tables"]!.ToJsonString(),"Mapping reset changed exported identity or bytes.");
        Console.WriteLine("通过：列移动、源表/字段两次重命名保留公开 API、SymbolId、参数顺序与两种数据字节；源码和 DLL 导出均继承映射。");
        Console.WriteLine("通过：空映射保留当前绑定；CLI 显式恢复源名需迁移凭据，恢复后公开 API 与双格式字节不变。");
        return Path.Combine(code,"Generated");
    }
    private static string Describe(ProjectWorkflowResult result)=>string.Join("\n",result.PreparationDiagnostics.Concat(result.Export?.Diagnostics??[]));
    private static void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
    private static void Reject(Action action){try{action();}catch(Exception){return;}throw new InvalidOperationException("错误输入未拒绝。");}

    internal static string? FindTool(string variable, string file)
    {
        string? configured = Environment.GetEnvironmentVariable(variable);
        string? result = ResolveTool(configured, file, Environment.GetEnvironmentVariable("PATH") ?? "");
        Console.WriteLine($"工具链 {variable}: {result ?? "未找到"}; 配置={configured ?? "<未设置>"}; 默认命令={file}");
        return result;
    }

    private static string? ResolveTool(string? configured, string file, string searchPath)
    {
        // An explicit profile must not silently fall back to an unrelated compiler on PATH.
        if (!string.IsNullOrEmpty(configured)) return File.Exists(configured) ? Path.GetFullPath(configured) : null;
        foreach (string path in searchPath.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(path)) continue;
            string candidate = Path.Combine(path.Trim('"'), file);
            if (File.Exists(candidate)) return Path.GetFullPath(candidate);
        }
        return null;
    }

    internal static string MissingTool(string variable, string file) =>
        $"{variable} ({file}; 配置={Environment.GetEnvironmentVariable(variable) ?? "<未设置>"})";

    private static (string? Javac, string? Go, string? Cpp) ResolveNativeTools(bool required)
    {
        string javacName = OperatingSystem.IsWindows() ? "javac.exe" : "javac";
        string goName = OperatingSystem.IsWindows() ? "go.exe" : "go";
        string cppName = OperatingSystem.IsWindows() ? "clang++.exe" : "clang++";
        string? javac = FindTool("ZRCONFIG_JAVAC", javacName), go = FindTool("ZRCONFIG_GO", goName), cpp = FindTool("ZRCONFIG_CXX", cppName);
        var missing = new List<string>();
        if (javac == null) missing.Add(MissingTool("ZRCONFIG_JAVAC", javacName));
        else if (!File.Exists(Path.Combine(Path.GetDirectoryName(javac)!, OperatingSystem.IsWindows() ? "java.exe" : "java")))
            missing.Add("java (JDK runtime beside " + javac + ")");
        if (go == null) missing.Add(MissingTool("ZRCONFIG_GO", goName));
        if (cpp == null) missing.Add(MissingTool("ZRCONFIG_CXX", cppName));
        if (required && missing.Count != 0) throw new InvalidOperationException("跨语言验收缺少工具链：" + string.Join("; ", missing));
        return (javac, go, cpp);
    }

    private static void CheckToolResolution(string parent)
    {
        string directory = Path.Combine(parent, "tool-resolution");
        Directory.CreateDirectory(directory);
        string compiler = Path.Combine(directory, "compiler"), missing = Path.Combine(directory, "missing");
        File.WriteAllText(compiler, "fixture");
        try
        {
            Check(ResolveTool(null, "compiler", directory) == compiler, "PATH tool resolution failed.");
            Check(ResolveTool(compiler, "other", "") == compiler, "Explicit tool profile was ignored.");
            Check(ResolveTool(missing, "compiler", directory) == null, "Missing explicit profile silently fell back to PATH.");
            Check(ResolveTool(null, "missing", directory) == null, "Missing tool was accepted.");
        }
        finally { Directory.Delete(directory, true); }
    }

    internal static string[] CppArguments(string compiler,IReadOnlyList<string> arguments)
    {
        string name=Path.GetFileNameWithoutExtension(compiler);
        if(!name.Equals("cl",StringComparison.OrdinalIgnoreCase))
        {
            // Large generated catalogs exceed the ordinary PE/COFF section limit.
            // Clang selects big objects automatically; MinGW's assembler needs this flag.
            if(OperatingSystem.IsWindows()&&(name.Equals("g++",StringComparison.OrdinalIgnoreCase)||name.EndsWith("-g++",StringComparison.OrdinalIgnoreCase)))
                return ["-Wa,-mbig-obj",..arguments];
            return arguments.ToArray();
        }
        var result=new List<string>{"/nologo","/std:c++17","/EHsc","/utf-8","/permissive-","/Zc:__cplusplus","/Zc:preprocessor","/bigobj","/W4","/WX"};
        for(int i=0;i<arguments.Count;i++)
        {
            string arg=arguments[i];
            switch(arg)
            {
                case "-std=c++17":case "-Wall":case "-Wextra":case "-Werror":case "-pedantic":case "-pthread":break;
                case "-O2":result.Add("/O2");break;
                case "-O1":result.Add("/O1");break;
                case "-g":result.Add("/Zi");break;
                case "-I":result.Add("/I"+arguments[++i]);break;
                case "-o":result.Add("/Fe:"+arguments[++i]);break;
                default:
                    if(arg.StartsWith('-'))throw new NotSupportedException("MSVC verification does not support flag: "+arg);
                    result.Add(arg);break;
            }
        }
        return result.ToArray();
    }
    private static Task CompileCpp(string compiler,string[] arguments,string root)=>Run(compiler,CppArguments(compiler,arguments),root);
    private static async Task CompileJava(string compiler,string[] arguments,string root)
    {
        // Keep the source list out of CreateProcess's command-line length limit.
        // javac argument files have their own quoting rules, independent of the shell.
        string file=Path.Combine(root,"javac-"+Guid.NewGuid().ToString("N")+".args");
        string Quote(string value)=>"\""+value.Replace("\\","\\\\",StringComparison.Ordinal).Replace("\"","\\\"",StringComparison.Ordinal)
            .Replace("\r","\\r",StringComparison.Ordinal).Replace("\n","\\n",StringComparison.Ordinal).Replace("\t","\\t",StringComparison.Ordinal).Replace("\f","\\f",StringComparison.Ordinal)+"\"";
        try
        {
            File.WriteAllLines(file,arguments.Select(Quote),new System.Text.UTF8Encoding(false));
            // JDK 17 otherwise reads argument files using the platform's default charset.
            await Run(compiler,["-J-Dfile.encoding=UTF-8","@"+file],root);
        }
        finally { File.Delete(file); }
    }
    private static async Task CheckJavaArgumentFiles(string javac,string java,string root)
    {
        string probe=Path.Combine(root,"javac args 空格 # 'quoted'"),classes=Path.Combine(probe,"compiled classes");
        Directory.CreateDirectory(probe);
        string[] sources=Enumerable.Range(0,512).Select(i=>Path.Combine(probe,"Source"+i.ToString("D4",CultureInfo.InvariantCulture)+".java")).ToArray();
        foreach(string source in sources)File.WriteAllText(source,"final class "+Path.GetFileNameWithoutExtension(source)+" {}\n");
        string main=Path.Combine(probe,"ArgumentFileCheck.java");
        File.WriteAllText(main,"public class ArgumentFileCheck { public static void main(String[] args) { Object first = new Source0000(), last = new Source0511(); if (first.getClass() == last.getClass()) throw new AssertionError(); } }\n");
        string[] arguments=["--release","17","-encoding","UTF-8","-d",classes,..sources,main];
        Check(arguments.Sum(argument=>argument.Length+1)>32767,"Java 参数文件回归必须超过 Windows 命令行长度上限。");
        await CompileJava(javac,arguments,probe);
        Check(Directory.GetFiles(classes,"*.class").Length==sources.Length+1,"Java 参数文件未编译全部源码。");
        Check(Directory.GetFiles(probe,"javac-*.args").Length==0,"Java 编译后遗留参数文件。");
        await Run(java,["-cp",classes,"ArgumentFileCheck"],probe);
        Console.WriteLine("通过：Java 超长源码列表使用参数文件，保留空格、中文和引号路径，并清理临时参数文件。");
    }
    private static async Task<string[]> CheckNamespaces(string root,string output,string javac,string java,string go,string cpp)
    {
        var paths=new List<string>();
        foreach(string id in new[]{"a","b"})
        {
            string code=Path.Combine(root,"namespaces-"+id);Directory.CreateDirectory(code);
            foreach(string file in new[]{"Types.cs","Tables.json"})File.Copy(Path.Combine(root,"Config",file),Path.Combine(code,file));
            var profile=new GenerationNamespaces(CSharp:"Game"+id.ToUpperInvariant()+".Config",Java:"game"+id+".config",Go:"example.invalid/game"+id,Cpp:"game"+id+"::config",Rust:"game"+id,TypeScript:"game-"+id,Lua:"game"+id+".config");
            var diagnostics=new DiagnosticCollector();
            var plan=ModelGenerator.PrepareWithNamespaces(profile,"server-v1",Path.Combine(root,"Data"),code,diagnostics,target:ConfigTarget.Server,languages:["all"],dataFormat:"both");
            Check(plan!=null&&plan.Apply(diagnostics),string.Join("\n",diagnostics.Snapshot())+plan?.DescribeChanges());
            var repeated=ModelGenerator.Prepare(Path.Combine(root,"Data"),code,diagnostics,target:ConfigTarget.Server,languages:["all"],dataFormat:"both");
            Check(repeated!=null&&!repeated.HasChanges,"Accepted module roots were not inherited: "+repeated?.DescribeChanges());
            var settings=new WorkspaceSettings(Path.Combine(root,"Data"),output,code,""){Namespaces=profile,CodeLanguages=["all"],Target=ConfigTarget.Server};
            string project=Path.Combine(code,"profile.zrconfig.json");settings.SaveProject(project);Check(WorkspaceSettings.LoadProject(project).Namespaces==profile,"Module roots lost during project roundtrip");
            using var compiled=RuleCompiler.PrepareSources(code,"",Path.Combine(root,"namespace-cache-"+id),diagnostics)??throw new Exception(string.Join("\n",diagnostics.Snapshot()));
            var catalog=(ZrConfig.Runtime.Server.Catalog)compiled.Assembly.GetType("Game"+id.ToUpperInvariant()+".Config.ServerCatalog")!.GetProperty("Instance")!.GetValue(null)!;
            using var manager=new ZrConfig.Runtime.Server.Manager(catalog,text=>JsonSerializer.Deserialize<ConfigManifest>(text,new JsonSerializerOptions{IncludeFields=true})!);
            var snapshot=manager.Reload(new FileConfigSource(ExportTestPaths.Root(output))).Snapshot;
            Check(snapshot.AsLegacySnapshot().Tables.Single(t=>t.Name=="Entry").Rows[0]!.GetType().Namespace=="Game"+id.ToUpperInvariant()+".Config","Custom CLR model namespace missing");
            paths.Add(Path.Combine(code,"Generated"));
        }
        string main=Path.Combine(root,"TwoCatalogs.java"),classes=Path.Combine(root,"two-java");Directory.CreateDirectory(classes);
        File.WriteAllText(main,"public class TwoCatalogs{public static void main(String[] args)throws Exception{var root=java.nio.file.Path.of(args[0]);var a=gamea.config.DataSnapshot.load(root,gamea.config.ConfigRuntime.LoadPlan.defaults(),new gamea.config.ConfigRuntime.Cancellation());var b=gameb.config.DataSnapshot.load(root,gameb.config.ConfigRuntime.LoadPlan.defaults(),new gameb.config.ConfigRuntime.Cancellation());if(a.get_Entry().get(\"east\",1).getId()!=b.get_Entry().get(\"east\",1).getId())throw new AssertionError();}}");
        await CompileJava(javac,["--release","17","-Xlint:unchecked,deprecation","-Werror","-encoding","UTF-8","-d",classes,..paths.SelectMany(p=>Directory.GetFiles(Path.Combine(p,"Java"),"*.java")),main],root);await Run(java,["-cp",classes,"TwoCatalogs",output],root);
        string goRoot=Path.Combine(root,"two-go");Directory.CreateDirectory(goRoot);
        File.WriteAllText(Path.Combine(goRoot,"go.mod"),"module consumer\n\ngo 1.19\nrequire (\nexample.invalid/gamea v0.0.0\nexample.invalid/gameb v0.0.0\n)\nreplace example.invalid/gamea => "+JsonSerializer.Serialize(Path.Combine(paths[0],"Go"))+"\nreplace example.invalid/gameb => "+JsonSerializer.Serialize(Path.Combine(paths[1],"Go"))+"\n");
        File.WriteAllText(Path.Combine(goRoot,"main.go"),"package main\nimport(\"context\";\"os\";a \"example.invalid/gamea\";b \"example.invalid/gameb\")\nfunc main(){x,e:=a.LoadData(context.Background(),os.Args[1],a.LoadPlan{});if e!=nil{panic(e)};y,e:=b.LoadData(context.Background(),os.Args[1],b.LoadPlan{});if e!=nil{panic(e)};xt,_:=x.Get_Entry();yt,_:=y.Get_Entry();xr,_:=xt.Get(\"east\",1);yr,_:=yt.Get(\"east\",1);if xr.Id()!=yr.Id(){panic(\"catalog mismatch\")}}\n");
        await Run(go,["run","-race",".",output],goRoot,new(){["GO111MODULE"]="on",["GOWORK"]="off",["GOCACHE"]=Path.Combine(root,"gocache")});
        string cppMain=Path.Combine(root,"two.cpp"),binary=Path.Combine(root,OperatingSystem.IsWindows()?"two-cpp.exe":"two-cpp");
        File.WriteAllText(cppMain,"#include "+JsonSerializer.Serialize(Path.Combine(paths[0],"Cpp","data.hpp").Replace('\\','/'))+"\n#include "+JsonSerializer.Serialize(Path.Combine(paths[1],"Cpp","data.hpp").Replace('\\','/'))+"\nint main(int argc,char** argv){if(argc!=2)return 2;gamea::config::Cancellation a;gameb::config::Cancellation b;auto x=gamea::config::data::Snapshot::load(argv[1],gamea::config::LoadPlan{},a);auto y=gameb::config::data::Snapshot::load(argv[1],gameb::config::LoadPlan{},b);return x->get_Entry(\"east\",1).Id()==y->get_Entry(\"east\",1).Id()?0:1;}\n");
        await CompileCpp(cpp,["-std=c++17","-O2","-pthread",cppMain,"-o",binary],root);await Run(binary,[output],root);
        Console.WriteLine("通过：C#/Java/Go/C++ 自定义模块根、继承固定配置及同进程双 Catalog。");return paths.ToArray();
    }
    private static async Task NativeAsync(string generated,string output,string root,bool required)
    {
        // No table payloads are copied: catalog mismatches must fail before any payload I/O.
        foreach (string kind in new[] { "extra", "missing", "replaced" })
        {
            var manifest = JsonNode.Parse(File.ReadAllText(ExportTestPaths.FilePath(output, "current.json")))!;
            var tables = manifest["tables"]!.AsArray();
            JsonNode ghost = tables[0]!.DeepClone();
            ghost["name"] = "Ghost"; ghost["file"] = "Ghost.bytes"; ghost["jsonFile"] = "Ghost.json";
            if (kind != "extra") tables.RemoveAt(0);
            if (kind != "missing") tables.Add(ghost);
            Directory.CreateDirectory(output + "-" + kind);
            File.WriteAllText(Path.Combine(output + "-" + kind, "current.json"), manifest.ToJsonString());
        }
        var (javac, go, cpp) = ResolveNativeTools(required);
        if(javac==null||go==null||cpp==null)
        {
            Check(!required,"跨语言验收需要 Java 17+、Go 1.19+、C++17；可用 ZRCONFIG_JAVAC/ZRCONFIG_GO/ZRCONFIG_CXX 指定。");
            Console.WriteLine("跳过原生编译验收：未配置全部工具链；使用 --portable-native 强制验收。");return;
        }
        string java=Path.Combine(Path.GetDirectoryName(javac)!,OperatingSystem.IsWindows()?"java.exe":"java");
        await CheckJavaArgumentFiles(javac,java,root);
        string javaMain=Path.Combine(root,"NativeCheck.java");File.WriteAllText(javaMain,JavaCheck);
        string classes=Path.Combine(root,"classes");Directory.CreateDirectory(classes);
        await CompileJava(javac,["--release","17","-Xlint:unchecked,deprecation","-Werror","-encoding","UTF-8","-d",classes,..Directory.GetFiles(Path.Combine(generated,"Java"),"*.java"),javaMain],root);
        await Run(java,["-cp",classes,"NativeCheck",output],root);
        string javaServer=Path.Combine(root,"ServerCheck.java");File.WriteAllText(javaServer,ServerRuntimeTests.Java);
        await CompileJava(javac,["--release","17","-Xlint:unchecked,deprecation","-Werror","-encoding","UTF-8","-cp",classes,"-d",classes,javaServer],root);
        await Run(java,["-cp",classes,"ServerCheck",output],root);
        File.WriteAllText(Path.Combine(generated,"Go","native_test.go"),GoCheck);
        File.WriteAllText(Path.Combine(generated,"Go","server_test.go"),ServerRuntimeTests.Go);
        await Run(go,["test","-race","-v","."],Path.Combine(generated,"Go"),new(){["GO111MODULE"]="off",["ZRCONFIG_TEST_OUTPUT"]=output,["GOCACHE"]=Path.Combine(root,"gocache")});
        string cppMain=Path.Combine(root,"native.cpp"),executable=Path.Combine(root,OperatingSystem.IsWindows()?"native.exe":"native");File.WriteAllText(cppMain,CppCheck);
        await CompileCpp(cpp,["-std=c++17","-O2","-Wall","-Wextra","-pedantic","-Werror","-I",Path.Combine(generated,"Cpp"),cppMain,"-o",executable],root);
        await Run(executable,[output],root);
        string serverMain=Path.Combine(root,"server.cpp"),serverExecutable=Path.Combine(root,OperatingSystem.IsWindows()?"server.exe":"server");
        File.WriteAllText(serverMain,ServerRuntimeTests.Cpp);
        await CompileCpp(cpp,["-std=c++17","-O2","-Wall","-Wextra","-pedantic","-Werror","-pthread","-I",Path.Combine(generated,"Cpp"),serverMain,"-o",serverExecutable],root);
        await Run(serverExecutable,[output],root);
        string macroMain=Path.Combine(root,"macros.cpp");
        File.WriteAllText(macroMain,string.Join("\n",File.ReadLines(Path.Combine(generated,"Cpp","runtime.hpp")).Where(line=>line.StartsWith("#include ",StringComparison.Ordinal)))+"\n#define min(a,b) broken_min_macro(a,b)\n#define max(a,b) broken_max_macro(a,b)\n#include \"data.hpp\"\n#ifndef min\n#error min macro lost\n#endif\n#ifndef max\n#error max macro lost\n#endif\nint main(){return 0;}\n");
        await CompileCpp(cpp,["-std=c++17","-Wall","-Wextra","-pedantic","-Werror","-I",Path.Combine(generated,"Cpp"),macroMain,"-o",Path.Combine(root,"macros")],root);
        string? sanitizers=Environment.GetEnvironmentVariable("ZRCONFIG_CPP_SANITIZERS");
        if(!string.IsNullOrEmpty(sanitizers))
        {
            if(sanitizers is not ("address,undefined" or "thread"))throw new InvalidOperationException("Unknown C++ sanitizer selection.");
            string sanitized=Path.Combine(root,"server-sanitized");
            await CompileCpp(cpp,["-std=c++17","-O1","-g","-fno-omit-frame-pointer","-fsanitize="+sanitizers,"-DZRCONFIG_SANITIZED_TEST","-pthread","-I",Path.Combine(generated,"Cpp"),serverMain,"-o",sanitized],root);
            await Run(sanitized,[output],root,new(){["UBSAN_OPTIONS"]="halt_on_error=1:print_stacktrace=1",["ASAN_OPTIONS"]="halt_on_error=1",["TSAN_OPTIONS"]="halt_on_error=1"});
        }


        // A table include historically makes unrelated row pointer types visible.
        // The shared forward declarations for large catalogs must preserve that
        // source contract, including when the complete snapshot is not included.
        foreach(bool server in new[]{false,true})
        {
            string boundary=Path.Combine(root,server?"large-forward-server":"large-forward-legacy");
            var scalar=new PortableType("System.Int32","primitive","int32",false,0,null,null,[],[],[],false,null,new());
            var stringType=new PortableType("System.String","primitive","string",false,0,null,null,[],[],[],false,null,new());
            var tables=Enumerable.Range(0,257).Select(i=>{
                string name=i==3?"zrconfig":i is 1 or 2?"ZrConfigSnapshotPart"+(i-1).ToString(CultureInfo.InvariantCulture):"Forward"+i.ToString("D3",CultureInfo.InvariantCulture);
                string target=i==0?"Forward256":"Forward000";
                var reference=new PortableType("ref:ZrConfig.Models."+target+"Config:Id:System.Int32","tablereference","int32",false,0,null,null,[],[],[],false,null,new()){ReferenceTable=target};
                return new PortableTable(name,new string('A',64),true,i is 0 or 256?[new("Id",scalar.Id),new("Peer",reference.Id)]:i==1?[new("Id",scalar.Id),new("Text",stringType.Id)]:[new("Id",scalar.Id)],i==1?["Id","Text"]:["Id"],i==1?[new("ById",["Id"]),new("ByText",["Text"]),new("ByTextAndId",["Text","Id"])]:[new("ById",["Id"])],i is 0 or 256?[scalar,reference]:i==1?[scalar,stringType]:[scalar],[]);
            }).ToArray();
            var sources=new Dictionary<string,string>(StringComparer.Ordinal);
            var largeLanguages=ExportOptions.Languages(["all"]);
            var largeNames=GeneratedSymbolPlan.Create([],tables,largeLanguages,server);largeNames.Target="C";
            NativeGenerator.Generate(tables,largeLanguages,sources,largeNames);
            foreach(var file in sources){string path=Path.Combine(boundary,file.Key);Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path,file.Value);}
            string boundaryOutput=Path.Combine(boundary,"Output"),boundaryVersion=Path.Combine(boundaryOutput,"versions","boundary");Directory.CreateDirectory(boundaryVersion);
            var entries=new List<object>();
            foreach(var table in tables)
            {
                var row=table.Fields.ToDictionary(field=>field.Name,field=>field.Type==stringType.Id?(object)"owned-key":1,StringComparer.Ordinal);
                byte[] bytes=JsonSerializer.SerializeToUtf8Bytes(new{format="ZrConfig.TableJson/1",table=table.Name,schemaHash=table.Hash,rows=new[]{row}});
                string file=table.Name+".json";File.WriteAllBytes(Path.Combine(boundaryVersion,file),bytes);
                entries.Add(new{name=table.Name,file,rows=1,schemaHash=table.Hash,sha256=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes))});
            }
            File.WriteAllText(Path.Combine(boundaryOutput,"current.json"),JsonSerializer.Serialize(new{format="ZrConfig.Json/1",target="C",versionId="boundary",createdUtc="2026-10-16T00:00:00Z",codeFingerprint=new string('A',64),tables=entries}));
            string include=Path.Combine(boundary,"forward.cpp"),binary=Path.Combine(boundary,OperatingSystem.IsWindows()?"forward.exe":"forward");
            File.WriteAllText(include,"#include \"Forward000Config.hpp\"\nzrconfig::Forward256Config* other=nullptr;\n#include \""+(server?"data.hpp":"snapshot.hpp")+"\"\n#include <cassert>\nint main(int,char** argv){assert(other==nullptr);std::filesystem::path root(argv[1]);auto legacy=zrconfig::ConfigSnapshot::load(root,true);assert(legacy->Forward000().get(1).Peer()->Peer()->Id()==1);"+(server?LargeCppBoundary:"")+"}\n");
            await CompileCpp(cpp,["-std=c++17","-Wall","-Wextra","-pedantic","-Werror","-I",Path.Combine(boundary,"Cpp"),include,"-o",binary],root);
            await Run(binary,[boundaryOutput],root);
            string javaConsumer=Path.Combine(boundary,"Boundary.java"),javaClasses=Path.Combine(boundary,"classes");
            File.WriteAllText(javaConsumer,"import zrconfig.*;\nimport static zrconfig.ConfigRuntime.*;\n"+(server?"import zrconfig.DataSnapshot.Forward000Row;\nimport static zrconfig.DataSnapshot.warmupRow_Forward000;\n":"")+"public class Boundary {\nstatic void check(boolean value){if(!value)throw new AssertionError();}\n"+
                "static Forward000Config.Table legacy(ConfigSnapshot value){return value.Forward000;}\n"+
                (server?"static Forward000Row row(DataSnapshot value){DataSnapshot.Forward000Table table=value.get_Forward000();return table.get(1);}\nstatic DataSnapshot.Warmup warm(){java.util.function.IntFunction<DataSnapshot.Warmup> call=DataSnapshot::warmupRow_Forward000;return call.apply(1);}\nstatic DataSnapshot.Warmup imported(){return warmupRow_Forward000(1);}\nstatic Forward256Config.Table selected(SelectedSnapshot value){return value.get_Forward256();}\n":"")+
                "public static void main(String[] args)throws Exception{if(ConfigSnapshot.class.getField(\"Forward000\").getType()!=Forward000Config.Table.class)throw new AssertionError();"+
                (server?"if(!DataSnapshot.Forward000Row.class.getCanonicalName().equals(\"zrconfig.DataSnapshot.Forward000Row\")||DataSnapshot.class.getMethod(\"warmupRow_Forward000\",int.class).getReturnType()!=DataSnapshot.Warmup.class||DataSnapshot.Forward000Table.class.getMethod(\"get\",int.class).getReturnType()!=DataSnapshot.Forward000Row.class)throw new AssertionError();":"")+"var root=java.nio.file.Path.of(args[0]);check(ConfigSnapshot.load(root,true).Forward000.get(1).getPeer().getPeer().getId()==1);"+(server?LargeJavaBoundary:"")+"}\n}\n");
            await CompileJava(javac,["--release","17","-Xlint:unchecked,deprecation","-Werror","-encoding","UTF-8","-d",javaClasses,..Directory.GetFiles(Path.Combine(boundary,"Java"),"*.java"),javaConsumer],root);
            await Run(java,["-cp",javaClasses,"Boundary",boundaryOutput],root);
            File.WriteAllText(Path.Combine(boundary,"Go","boundary_test.go"),"package zrconfig\nimport (\"testing\""+(server?";\"context\"":"")+")\nfunc TestLargeBoundary(t *testing.T){root:="+JsonSerializer.Serialize(boundaryOutput)+";legacy,err:=LoadConfig(root,true);if err!=nil{t.Fatal(err)};if legacy.Forward000().Get(1).Peer().Peer().Id()!=1{t.Fatal(\"legacy reference cycle\")};"+(server?LargeGoBoundary:"")+"}\n");
            await Run(go,["test","."],Path.Combine(boundary,"Go"),new(){["GO111MODULE"]="off",["GOCACHE"]=Path.Combine(root,"gocache")});
        }
        Console.WriteLine("通过：257 表保留 C++ 单表头文件跨表类型可见性，以及 Java 字段、嵌套类型、静态导入和方法引用，兼容旧入口与 server-v1。");

        foreach(bool server in new[]{false,true})
        {
            string boundary=Path.Combine(root,server?"empty-server":"empty-legacy"); Directory.CreateDirectory(boundary);
            var sources=new Dictionary<string,string>(StringComparer.Ordinal);
            NativeGenerator.Generate([],ExportOptions.Languages(["all"]),sources,GeneratedSymbolPlan.Create([],[],ExportOptions.Languages(["all"]),server));
            foreach(var file in sources){string path=Path.Combine(boundary,file.Key);Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path,file.Value);}
            await CompileJava(javac,["--release","17","-Xlint:unchecked,deprecation","-Werror","-encoding","UTF-8","-d",Path.Combine(boundary,"classes"),..Directory.GetFiles(Path.Combine(boundary,"Java"),"*.java")],root);
            await Run(go,["test","."],Path.Combine(boundary,"Go"),new(){["GO111MODULE"]="off",["GOCACHE"]=Path.Combine(root,"gocache")});
            string include=Path.Combine(boundary,"check.cpp");File.WriteAllText(include,"#include \""+(server?"data.hpp":"snapshot.hpp")+"\"\nint main(){return 0;}\n");
            await CompileCpp(cpp,["-std=c++17","-Wall","-Wextra","-pedantic","-Werror","-I",Path.Combine(boundary,"Cpp"),include,"-o",Path.Combine(boundary,"check")],root);
        }
        string[] namespaceRoots=await CheckNamespaces(root,output,javac,java,go,cpp);
        await AdditionalLanguageTests.RunAsync(generated,output,root,required,namespaceRoots);
        string names = Path.Combine(root, "go-names"), nameData = Path.Combine(names, "Data"), nameCode = Path.Combine(names, "Config");
        Directory.CreateDirectory(nameData);
        using (var book = new XLWorkbook())
        {
            var sheet = book.AddWorksheet("_Item"); sheet.Cell(1, 1).Value = "Id"; sheet.Cell(2, 1).Value = "int";
            sheet.Cell(3, 1).Value = "主键"; sheet.Cell(5, 1).Value = "CS";
            sheet.CopyTo("java"); // A valid field name must not shadow a package-qualified expression in generated code.
            book.SaveAs(Path.Combine(nameData, "Input.xlsx"));
        }
        var diagnostics = new DiagnosticCollector();
        Check(ModelGenerator.Generate(nameData, nameCode, diagnostics, languages: ["go", "java"]), string.Join("\n", diagnostics.Snapshot()));
        await Run(go, ["test", "."], Path.Combine(nameCode, "Generated", "Go"), new() { ["GO111MODULE"] = "off", ["GOCACHE"] = Path.Combine(root, "gocache") });
        await CompileJava(javac, ["--release", "17", "-encoding", "UTF-8", "-d", Path.Combine(names, "classes"), .. Directory.GetFiles(Path.Combine(nameCode, "Generated", "Java"), "*.java")], root);
    }
    private static async Task Run(string file,string[] arguments,string directory,Dictionary<string,string>? environment=null)
    {
        var start=new ProcessStartInfo(file){WorkingDirectory=directory,RedirectStandardOutput=true,RedirectStandardError=true,UseShellExecute=false};
        foreach(string arg in arguments)start.ArgumentList.Add(arg);
        if(environment!=null)foreach(var p in environment)start.Environment[p.Key]=p.Value;
        using var process=Process.Start(start)??throw new Exception("进程启动失败："+file);
        Task<string> stdout=process.StandardOutput.ReadToEndAsync(),stderr=process.StandardError.ReadToEndAsync();
        try{await process.WaitForExitAsync().WaitAsync(TimeSpan.FromMinutes(3));}catch{process.Kill(true);throw;}
        string text=await stdout+await stderr;Check(process.ExitCode==0,file+"\n"+text);Console.WriteLine(text.Trim());
    }
    private const string JavaCheck = """
        import zrconfig.*;import java.nio.file.*;import java.lang.management.*;
        public class NativeCheck {
          static volatile int sink;
          static long probe(com.sun.management.ThreadMXBean bean,ConfigSnapshot s){long id=Thread.currentThread().getId();long before=bean.getThreadAllocatedBytes(id);for(int i=0;i<100000;i++){sink=s.Entry.get("east",1).getId()+s.Entry.getById(1).size();}return bean.getThreadAllocatedBytes(id)-before;}
          static void check(boolean v){if(!v)throw new AssertionError();}
          public static void main(String[] args)throws Exception {
            var root=Path.of(args[0]);var manager=new ConfigSnapshot.Manager();
            for(boolean json:new boolean[]{false,true}) {
              var s=manager.reload(root,json);var row=s.Entry.get("east",1);
              check(s.Concrete.get(1).getPayload().getDelay()==7);
              check(row.getName().equals("中文😀")&&row.getSigned()==Long.MIN_VALUE&&row.getUnsigned().toString().equals("18446744073709551615"));
              check(row.getAmount().toString().equals("79228162514264337593543950335")&&row.getOptional()==null&&row.getValues().get(0)==null&&row.getShape().at(3)==4);
              check(row.getMapping().get(7).equals("值")&&row.getLookup()==s.Lookup.get(10)&&row.getLookup().getPeer().getPeer()==row.getLookup());
              check(row.getAction() instanceof ConfigTypes.T_Demo_Damage&&row.getQuality()==ConfigTypes.T_Demo_Quality.T_Max);
              check(row.getclass().equals("keyword")&&s.Entry.get("west",1)!=row&&s.Entry.getById(1).size()==2&&s.Entry.getKey("中文😀").size()==2);
              for(int i=0;i<200000;i++){sink=s.Entry.get("east",1).getId()+s.Entry.getById(1).size();}
              var bean=(com.sun.management.ThreadMXBean)ManagementFactory.getThreadMXBean();bean.setThreadAllocatedMemoryEnabled(true);for(int warm=0;warm<5;warm++)probe(bean,s);
              long allocated=probe(bean,s);if(allocated!=0)throw new AssertionError("allocated="+allocated);
              var next=manager.reload(root,!json);check(s.Entry.get("east",1)==row&&next!=s);try{manager.reload(root.resolve("missing"),json);throw new AssertionError();}catch(java.io.IOException expected){}check(manager.current()==next);
              for(String bad:new String[]{"extra","missing","replaced"}){try{manager.reload(Path.of(args[0]+"-"+bad),json);throw new AssertionError("catalog accepted");}catch(IllegalArgumentException expected){check(expected.getMessage().contains("表集合"));}check(manager.current()==next);}
              try{ConfigRuntime.parse("\"\\uD800\"");throw new AssertionError();}catch(IllegalArgumentException expected){}
              System.out.println("通过：Java "+(json?"JSON":"Binary")+"，100000 次主键+分组查询分配 "+allocated+" B，引用及快照正确。");
            }
          }
        }
        """;
    private const string GoCheck = """
        package zrconfig
        import("testing";"os";"strings")
        var sink int32
        func TestNative(t *testing.T){
          root:=os.Getenv("ZRCONFIG_TEST_OUTPUT");manager:=&ConfigManager{}
          for _,json:=range []bool{false,true}{
            s,e:=manager.Reload(root,json);if e!=nil{t.Fatal(e)};r:=s.Entry().Get("east",1)
            if s.Concrete().Get(1).Payload().Delay()!=7{t.Fatal("concrete inheritance")}
            if r.Name()!="中文😀"||r.Signed()!=-9223372036854775808||r.Unsigned()!=18446744073709551615||r.Amount().String()!="79228162514264337593543950335"{t.Fatal("scalar")}
            _,optional:=r.Optional().Value();_,element:=r.Values().At(0).Value();mapped,ok:=r.Mapping().Find(7)
            if optional||element||!ok||mapped!="值"||r.Shape().Values().At(3)!=4||r.Lookup()!=s.Lookup().Get(10)||r.Lookup().Peer().Peer()!=r.Lookup(){t.Fatal("containers/reference")}
            if _,ok:=r.Action().(*T_Demo_Damage);!ok||r.Quality()!=T_Demo_Quality_T_Max||r.Get_class()!="keyword"||s.Entry().GetById(1).Len()!=2||s.Entry().GetKey("中文😀").Len()!=2{t.Fatal("types/query")}
            n:=testing.AllocsPerRun(100000,func(){sink=s.Entry().Get("east",1).Id()+int32(s.Entry().GetById(1).Len())});if n!=0{t.Fatalf("allocs=%v",n)}
            next,e:=manager.Reload(root,!json);if e!=nil||s.Entry().Get("east",1)!=r||next==s{t.Fatal("snapshot")};_,e=manager.Reload(root+"/missing",json);if e==nil||manager.Current()!=next{t.Fatal("rollback")}
            for _,bad:=range []string{"extra","missing","replaced"}{_,e=manager.Reload(root+"-"+bad,json);if e==nil||!strings.Contains(e.Error(),"表集合")||manager.Current()!=next{t.Fatal("catalog validation",e)}}
            func(){defer func(){if recover()==nil{t.Fatal("surrogate")}}();parseJSON([]byte(`"\uD800"`))}()
            t.Logf("通过：Go JSON=%v，主键+分组查询 0 allocs/op，引用及快照正确。",json)
          }
        }
        """;
    private const string LargeJavaBoundary = """
        check(SelectedSnapshot.load(root,java.util.List.of("Forward000"),true).get_Forward256().get(1).getPeer().getId()==1);
        var token=new Cancellation();
        for(var mode:LoadMode.values()){
            var plan=new LoadPlan(mode,mode==LoadMode.SelectedTables?java.util.List.of("Forward000"):null,true,java.util.List.of());
            var snapshot=DataSnapshot.load(root,plan,token);var expected=mode==LoadMode.ServerDefault?257:mode==LoadMode.SelectedTables?2:0;
            check(snapshot.loadedRowCount()==expected&&snapshot.completedTableCount()==expected);
            var table=snapshot.get_Forward000();var row=table.load(1,token);check(row.load_Peer().load_Peer()==row);check(table.loadGroup_ById(1,token).size()==1);
        }
        var warmed=DataSnapshot.loadWithWarmup(root,new LoadPlan(LoadMode.LazyRowsOnAccess,null,true,java.util.List.of("Forward256")),java.util.List.of(DataSnapshot.warmupRow_Forward000(1),DataSnapshot.warmupGroup_Forward000_ById(1),DataSnapshot.warmupRow_ZrConfigSnapshotPart0(1,"owned-key"),DataSnapshot.warmupGroup_ZrConfigSnapshotPart0_ByTextAndId("owned-key",1)),token);
        check(warmed.loadedRowCount()==3&&warmed.completedTableCount()==1);
        """;

    private const string LargeGoBoundary = """
        ctx:=context.Background()
        empty:=new(DataSnapshot);if empty.LoadedRowCount()!=0||empty.CompletedTableCount()!=0||empty.LoadAll(ctx)!=nil{t.Fatal("zero snapshot counts")};if _,err:=empty.Get_Forward000();err==nil{t.Fatal("zero snapshot selection")};if new(ConfigSnapshot).Forward000()!=nil{t.Fatal("zero legacy snapshot")}
        selected,err:=LoadSelectedConfig(root,[]string{"Forward000"},true);if err!=nil{t.Fatal(err)}
        target,err:=selected.Get_Forward256();if err!=nil||target.Get(1).Peer().Id()!=1{t.Fatal("selected reference",err)}
        owned,err:=LoadServerConfig(root,true);if err!=nil||owned.Forward000().Get(1).serverOwner!=owned{t.Fatal("server ownership",err)}
        for _,mode:=range []LoadMode{ServerDefault,SelectedTables,LazyRowsStrict,LazyRowsOnAccess}{
            plan:=LoadPlan{Mode:mode,PreferJSON:true};expected:=0
            if mode==ServerDefault{expected=257};if mode==SelectedTables{expected=2;plan.Tables=[]string{"Forward000"}}
            snapshot,err:=LoadData(ctx,root,plan);if err!=nil{t.Fatal(err)}
            if snapshot.LoadedRowCount()!=expected||snapshot.CompletedTableCount()!=expected{t.Fatal("counts",mode)}
            table,err:=snapshot.Get_Forward000();if err!=nil{t.Fatal(err)};row,err:=table.Load(ctx,1);if err!=nil{t.Fatal(err)}
            peer,err:=row.Load_Peer(ctx);if err!=nil{t.Fatal(err)};back,err:=peer.Load_Peer(ctx);if err!=nil||back!=row{t.Fatal("reference identity",err)}
            group,err:=table.LoadGroup_ById(ctx,1);if err!=nil||group.Len()!=1{t.Fatal("group",err)}
        }
        plan:=LoadPlan{Mode:LazyRowsOnAccess,PreferJSON:true,WarmupTables:[]string{"Forward256"}}
        snapshot,err:=LoadDataWithWarmup(ctx,root,plan,[]DataWarmup{WarmupRow_Forward000(1),WarmupGroup_Forward000_ById(1),WarmupRow_ZrConfigSnapshotPart0(1,"owned-key"),WarmupGroup_ZrConfigSnapshotPart0_ByTextAndId("owned-key",1)});if err!=nil{t.Fatal(err)}
        if snapshot.LoadedRowCount()!=3||snapshot.CompletedTableCount()!=1{t.Fatal("warmup counts")}
        """;

    private const string LargeCppBoundary = """
        using namespace zrconfig;using Snapshot=zrconfig::data::Snapshot;
        Cancellation token;
        for(auto mode:{LoadMode::ServerDefault,LoadMode::SelectedTables,LoadMode::LazyRowsStrict,LoadMode::LazyRowsOnAccess}){
            LoadPlan plan;plan.mode=mode;plan.prefer_json=true;if(mode==LoadMode::SelectedTables)plan.tables=std::vector<std::string>{"Forward000"};
            auto snapshot=Snapshot::load(root,plan,token);std::size_t expected=mode==LoadMode::ServerDefault?257:mode==LoadMode::SelectedTables?2:0;
            assert(snapshot->loaded_row_count()==expected&&snapshot->completed_table_count()==expected);
            auto row=snapshot->load_Forward000(1,token);assert(row.load_Peer(token)->load_Peer(token)->Id()==1);
            assert(snapshot->load_group_Forward000_ById(1,token).size()==1);
        }
        LoadPlan lazy;lazy.mode=LoadMode::LazyRowsOnAccess;lazy.prefer_json=true;
        std::string key="owned-key";
        auto rowStep=Snapshot::warmup_row_ZrConfigSnapshotPart0(1,key);
        auto groupStep=Snapshot::warmup_group_ZrConfigSnapshotPart0_ByText(key);
        auto compositeStep=Snapshot::warmup_group_ZrConfigSnapshotPart0_ByTextAndId(key,1);
        key.assign("changed-after-capture");
        auto snapshot=Snapshot::load_with_warmup(root,lazy,{Snapshot::warmup_table_Forward256(),Snapshot::warmup_row_Forward000(1),Snapshot::warmup_group_Forward000_ById(1),rowStep,groupStep,compositeStep},token);
        assert(snapshot->loaded_row_count()==3&&snapshot->completed_table_count()==1);
        assert(snapshot->get_ZrConfigSnapshotPart0(1,"owned-key").Id()==1);
        assert(snapshot->find_group_ZrConfigSnapshotPart0_ByText("owned-key").size()==1);
        assert(snapshot->find_group_ZrConfigSnapshotPart0_ByTextAndId("owned-key",1).size()==1);
        std::weak_ptr<const Snapshot> weak=snapshot;
        {auto row=snapshot->get_Forward000(1);snapshot.reset();assert(!weak.expired());assert(row.get_Peer()->get_Peer()->Id()==1);}
        assert(weak.expired());
        """;

    private const string CppCheck = """
        #include "snapshot.hpp"
        #include <iostream>
        #include <cstdlib>
        #include <new>
        static std::atomic<std::size_t> allocations{0};static volatile std::int32_t sink;
        #if defined(__GNUC__)
        __attribute__((noinline))
        #endif
        void* operator new(std::size_t n){allocations.fetch_add(1,std::memory_order_relaxed);if(auto p=std::malloc(n?n:1))return p;throw std::bad_alloc();}
        // Keep replacement allocation/deallocation boundaries visible to GCC.
        #if defined(__GNUC__)
        __attribute__((noinline))
        #endif
        void operator delete(void* p)noexcept{std::free(p);}void* operator new[](std::size_t n){return ::operator new(n);}void operator delete[](void* p)noexcept{::operator delete(p);}
        void operator delete(void* p,std::size_t)noexcept{::operator delete(p);}
        void operator delete[](void* p,std::size_t)noexcept{::operator delete[](p);}
        void check(bool v){if(!v)throw std::runtime_error("assertion failed");}
        int main(int,char**argv){try{zrconfig::ConfigManager manager;std::filesystem::path root(argv[1]);
          for(bool json:{false,true}){auto s=manager.reload(root,json);std::string key="east";const auto&r=s->Entry().get(key,1);
            check(s->Concrete().get(1).Payload()->Delay()==7);
            check(r.Name()=="中文😀"&&r.Signed()==std::numeric_limits<std::int64_t>::min()&&r.Unsigned()==std::numeric_limits<std::uint64_t>::max()&&r.Amount().string()=="79228162514264337593543950335");
            check(!r.Optional()&&!r.Values()[0]&&r.Shape().values()[3]==4&&r.Mapping().at(7)=="值"&&r.Lookup()==s->Lookup().find(10)&&r.Lookup()->Peer()->Peer()==r.Lookup());
            check(dynamic_cast<const zrconfig::T_Demo_Damage*>(r.Action().get())&&r.Quality()==zrconfig::T_Demo_Quality::T_Max&&r.class_()=="keyword"&&s->Entry().getById(1).size()==2&&s->Entry().getKey("中文😀").size()==2);
            auto before=allocations.load();for(int i=0;i<100000;i++)sink=s->Entry().get(key,1).Id()+static_cast<std::int32_t>(s->Entry().getById(1).size());check(allocations.load()==before);
            auto next=manager.reload(root,!json);check(&s->Entry().get(key,1)==&r&&next!=s);try{manager.reload(root/"missing",json);return 2;}catch(const std::exception&){}check(manager.current()==next);
            for(const auto*bad:{"extra","missing","replaced"}){try{manager.reload(root.string()+"-"+bad,json);return 4;}catch(const std::exception&e){check(std::string(e.what()).find("表集合")!=std::string::npos);}check(manager.current()==next);}
            try{zrconfig::parse_json("\"\\uD800\"");return 3;}catch(const std::exception&){}
            std::cout<<"通过：C++ JSON="<<json<<"，100000 次主键+分组查询分配 0 次，引用及快照正确。"<<std::endl;
          }
        }catch(const std::exception&e){std::cerr<<e.what()<<std::endl;return 1;}return 0;}
        """;
}

// Shared fixture paths: exported data is grouped; hand-built legacy fixtures keep their original layout.
internal static class ExportTestPaths
{
    internal static string Root(string folder)
    {
        string csharp = Path.Combine(folder, "CSharp");
        return File.Exists(Path.Combine(csharp, "Common", "current.json")) ? csharp : folder;
    }
    internal static string FilePath(string folder, string file)
    {
        string root = Root(folder);
        if (file == "Common/current.json") file = "current.json";
        if (System.IO.File.Exists(Path.Combine(root, "Common", "current.json")))
            return Path.Combine(root, file == "current.json" ? "Common/current.json" : "Tables/" + (file.EndsWith(".json", StringComparison.Ordinal) ? "json/" : "bytes/") + file);
        return Path.Combine(root, file);
    }
    // Exercise old flat bundles as well as the new grouped fixtures, without mutating exported data.
    internal static string FlatFixture(string folder, string destination)
    {
        string root = Root(folder);
        var manifest = JsonNode.Parse(System.IO.File.ReadAllBytes(FilePath(folder, "current.json")))!;
        Directory.CreateDirectory(destination);
        foreach (var table in manifest["tables"]!.AsArray())
            foreach (string field in new[] { "file", "jsonFile" })
                if (table![field] is { } value)
                {
                    string file = value.GetValue<string>();
                    System.IO.File.Copy(FilePath(folder, file), Path.Combine(destination, file), true);
                }
        manifest["layout"] = "flat";
        System.IO.File.WriteAllText(Path.Combine(destination, "current.json"), manifest.ToJsonString());
        return destination;
    }
}
