using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using ClosedXML.Excel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ZrConfig.Core;
using ZrConfig.Runtime;
using ZrConfig.Validation;

internal static class GeneratedApiContractTests
{
    internal static void Run(string parent)
    {
        CheckDeclarations();
        CheckNativeContracts();
        CheckMigrationHistory(parent);
        string root = Path.Combine(parent, "api-contracts"), data = Path.Combine(root, "Data"), code = Path.Combine(root, "Config");
        Directory.CreateDirectory(data);
        string workbook = Path.Combine(data, "A.xlsx");
        Write(workbook, "A", ["Id", "Name"], ["int", "string"]);
        Apply(Plan());
        string baseline = Path.Combine(code, ".schema.json");
        var first = Capture(code);
        GeneratedApiPolicy originalPolicy=JsonSerializer.Deserialize<SchemaSummary>(File.ReadAllText(baseline))!.ApiContract!.Policy!;
        var originalContract=JsonSerializer.Deserialize<SchemaSummary>(File.ReadAllText(baseline))!.ApiContract!;
        Check(originalContract.Format==3 && originalContract.CompiledSymbols?.Length>0 && originalContract.DescriptorDigest?.Length==64 && originalContract.AcceptedParentDigest==null,"Compiler contract lineage missing.");
        // This is the exact binding read used by the released pre-contract generator.
        bool oldToolRejected=false;
        try { JsonSerializer.Deserialize<string>(File.ReadAllText(Path.Combine(code,".generation.json"))); }
        catch(JsonException) { oldToolRejected=true; }
        Check(oldToolRejected,"Old generators can silently ignore the protected API baseline.");
        Check(originalPolicy.Names.Length>0 && originalPolicy.Names.All(n=>n.SymbolId.Length==64),"Stable symbol bindings missing.");
        Check(!Plan().HasChanges, "Identical generation must be a no-op.");
        DateTime timestamp = File.GetLastWriteTimeUtc(baseline);
        Apply(Plan());
        Same(first, Capture(code));
        Check(File.GetLastWriteTimeUtc(baseline) == timestamp, "No-op rewrote the baseline.");
        Consumer(code);
        Edit(s => { s.Cell(4, 2).Value = "changed description"; s.Cell(6, 2).Value = "new data"; });
        Check(!Plan().HasApiChanges, "Data/description changes altered API.");
        Apply(Plan());
        Consumer(code);
        Write(Path.Combine(data, "B.xlsx"), "B", ["Id"], ["int"]);
        Check(!Plan().HasApiChanges, "Unrelated C# table changed old symbols.");
        Apply(Plan());
        Consumer(code); // The frozen old consumer is compiled and run without edits.
        GeneratedApiPolicy extendedPolicy=JsonSerializer.Deserialize<SchemaSummary>(File.ReadAllText(baseline))!.ApiContract!.Policy!;
        Check(JsonSerializer.Deserialize<SchemaSummary>(File.ReadAllText(baseline))!.ApiContract!.AcceptedParentDigest==originalContract.DescriptorDigest,"Compatible addition did not retain its accepted parent.");
        Check(originalPolicy.ProjectIdentity==extendedPolicy.ProjectIdentity && originalPolicy.Names.All(n=>extendedPolicy.Names.Contains(n)),"Unrelated table changed stable identities/names.");
        string moved=Path.Combine(data,"Moved.xlsx"); File.Move(workbook,moved);
        Check(!Plan().HasChanges,"Moving a workbook changed names or identities."); File.Move(moved,workbook);

        byte[] originalWorkbook = File.ReadAllBytes(workbook);
        foreach (Action<IXLWorksheet> mutation in new Action<IXLWorksheet>[]
        {
            s => s.Cell(1, 2).Value = "Title",
            s => s.Cell(2, 2).Value = "int",
            s => s.Column(2).Delete(),
            s => { s.Cell(1, 3).Value = "Extra"; s.Cell(2, 3).Value = "int"; s.Cell(5, 3).Value = "CS"; },
            s => { s.Cell(3, 1).Clear(); s.Cell(3, 2).Value = "主键"; }
        })
        {
            Edit(mutation);
            ModelGenerationPlan plan = Plan();
            var before = Capture(code);
            var diagnostics = new DiagnosticCollector();
            Check(plan.HasApiChanges && plan.ApiChangeId?.Length == 64 && !plan.Apply(diagnostics, true), "Breaking schema bypassed ordinary approval.");
            Check(diagnostics.Snapshot().Any(d => d.Code == "API_CHANGE_REQUIRED"), "Missing API diagnostic.");
            Check(!GeneratedApiContractTests.AcceptTestMigration(plan, new string('0', 64)), "Unrelated approval accepted.");
            Same(before, Capture(code));
            File.WriteAllBytes(workbook, originalWorkbook);
        }
        Edit(s => s.Cell(1, 2).Value = "Title");
        ModelGenerationPlan stale = Plan();
        Check(GeneratedApiContractTests.AcceptTestMigration(stale, stale.ApiChangeId!), "Exact migration rejected.");
        Edit(s => s.Cell(1, 2).Value = "Label");
        var staleDiagnostics = new DiagnosticCollector();
        Check(!stale.Apply(staleDiagnostics, true) && staleDiagnostics.Snapshot().Any(d => d.Code == "GENERATION_CHANGED"), "Approved stale plan wrote files.");
        File.WriteAllBytes(workbook, originalWorkbook);

        string goodBaseline = File.ReadAllText(baseline);
        foreach (string? invalid in new string?[] { null, "{broken", "null", Alter(n => n["ApiContract"]!["Format"] = 99),
            Alter(n => n.AsObject().Remove("ApiContract")), Alter(n => n["ApiContract"]!["Target"] = "S"),
            Alter(n => n["ApiContract"]!["Policy"]!["Format"] = 99), Alter(n => n["ApiContract"]!["Policy"]!["Profiles"]![0]!["Toolchain"] = "unknown"),
            Alter(n => n["ApiContract"]!["Policy"]!.AsObject().Remove("Names")),
            Alter(n => n["ApiContract"]!.AsObject().Remove("CompiledSymbols")),
            Alter(n => n["ApiContract"]!["DescriptorDigest"] = new string('0',64)),
            Alter(n => n["ApiContract"]!["CompiledSymbols"]![0]!["Name"] = "Changed") })
        {
            if (invalid == null) File.Delete(baseline); else File.WriteAllText(baseline, invalid);
            var before = Capture(code);
            var diagnostics = new DiagnosticCollector();
            ModelGenerationPlan bad = Plan();
            Check(!bad.Apply(diagnostics, true) && diagnostics.Snapshot().Any(d => d.Code == "API_BASELINE_INVALID"), "Invalid/missing baseline was reset.");
            Check(!GeneratedApiContractTests.AcceptTestMigration(bad, new string('0', 64)), "Invalid baseline accepted migration.");
            Same(before, Capture(code));
            File.WriteAllText(baseline, goodBaseline);
        }
        string generated = Path.Combine(code, "Generated", "AConfig.g.cs"), goodSource = File.ReadAllText(generated);
        File.WriteAllText(generated, goodSource.Replace("public string @Name", "public int @Name", StringComparison.Ordinal));
        var modifiedDiagnostics = new DiagnosticCollector();
        Check(!Plan().Apply(modifiedDiagnostics, true) && modifiedDiagnostics.Snapshot().Any(d => d.Code == "API_BASELINE_INVALID"), "Modified public source bypassed baseline.");
        File.WriteAllText(generated, goodSource);

        // Legacy import uses the actual old output as its baseline, not the new candidate.
        File.WriteAllText(baseline, Alter(n => n.AsObject().Remove("ApiContract")));
        foreach (string file in Directory.GetFiles(Path.Combine(code, "Generated"), "*.cs"))
            File.WriteAllText(file, File.ReadAllText(file).Replace("// " + GeneratedApiContracts.Marker + "\n", ""));
        Edit(s => s.Cell(1, 2).Value = "Title");
        Check(Plan().HasApiChanges, "Legacy import silently adopted new declarations.");
        File.WriteAllBytes(workbook, originalWorkbook);
        Check(!Plan().HasApiChanges, "Unchanged legacy contract rejected.");
        Apply(Plan());

        Edit(s => s.Cell(1, 2).Value = "Title");
        var console = new StringWriter();
        var beforeCli = Capture(code);
        Check(ToolCommandRunner.Run(["codegen", data, code, "--accept-changes"], console, console) == 1 && console.ToString().Contains("API_CHANGE_REQUIRED"), "CLI generic approval bypassed API gate.");
        Same(beforeCli, Capture(code));
        Check(ToolCommandRunner.Run(["codegen", data, code, "--accept-api-change", new string('0', 64)], console, console) == 1, "CLI accepted wrong digest.");
        Same(beforeCli, Capture(code));
        var migrationPlan = Plan();
        string digest = migrationPlan.ApiChangeId!;
        Check(!migrationPlan.AcceptApiChanges(digest),"A digest without consumer evidence accepted a migration.");
        Check(ToolCommandRunner.Run(["codegen", data, code, "--accept-api-change", digest], console, console) == 1,"CLI accepted migration without evidence.");
        string receiptFile = Path.Combine(root,"migration-receipt.json");
        CheckVerificationIntegrity(migrationPlan,root);
        var receipt = VerifyMigration(migrationPlan,root);
        Check(!migrationPlan.AcceptApiMigration(digest,receipt with { Digest=new string('0',64) }),"Modified receipt accepted.");
        var externalRequest=migrationPlan.MigrationRequest!;externalRequest.Languages[0]="unverified";
        Check(migrationPlan.MigrationRequest!.Languages[0]=="csharp","Caller changed the required verification languages.");
        byte[] reviewedWorkbook=File.ReadAllBytes(workbook);
        Edit(sheet=>sheet.Cell(6,2).Value="changed after verification");
        Check(Plan().ApiChangeId!=digest,"Data input changes reused a migration receipt.");
        Check(migrationPlan.AcceptApiMigration(digest,receipt),"Exact receipt rejected before stale-input check.");
        var changedInputDiagnostics=new DiagnosticCollector();
        Check(!migrationPlan.Apply(changedInputDiagnostics,true)&&changedInputDiagnostics.Snapshot().Any(d=>d.Code=="GENERATION_CHANGED"),"Data changed after verification but migration committed.");
        File.WriteAllBytes(workbook,reviewedWorkbook);
        File.WriteAllText(receiptFile,JsonSerializer.Serialize(receipt));
        Check(ToolCommandRunner.Run(["codegen", data, code, "--accept-api-change", digest,"--migration-receipt",receiptFile], console, console) == 0, console.ToString());
        Check(!Plan().HasChanges, "Approved migration did not persist new baseline.");
        var accepted=JsonSerializer.Deserialize<SchemaSummary>(File.ReadAllText(baseline))!;
        Check(accepted.LastMigration?.Digest==receipt.Digest&&accepted.ApiContract!.Policy!.ContractVersion=="2.0","Migration receipt or major version lost.");
        ApiReleaseVerifier.Verify(System.Text.Encoding.UTF8.GetString(beforeCli[baseline]),GeneratedSources(beforeCli),File.ReadAllText(baseline),GeneratedSources(Capture(code)));
        bool rewrittenRejected=false;
        try{ApiReleaseVerifier.Verify(System.Text.Encoding.UTF8.GetString(beforeCli[baseline]),GeneratedSources(beforeCli),JsonSerializer.Serialize(accepted with {LastMigration=null}),GeneratedSources(Capture(code)));}
        catch(InvalidDataException){rewrittenRejected=true;}
        Check(rewrittenRejected,"Rewritten candidate baseline bypassed the trusted release gate.");
        var migrated = Capture(code);
        Check(ToolCommandRunner.Run(["codegen", data, code, "--accept-api-change", digest], console, console) == 1, "Stale digest was reusable.");
        Same(migrated, Capture(code));
        foreach (string[] args in new[] { new[] { "codegen", "--accept-api-change" }, ["codegen", "--accept-api-change", "all"],
            ["export", "--generate", "--accept-api-change", digest], ["codegen", "--accept-api-change", digest, "--accept-api-change", digest] })
        {
            bool rejected = false;
            try { ToolCommandLine.Parse(args); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "Invalid migration arguments accepted.");
        }
        CheckNames(root);
        CheckCachedHelperNames(root);
        CheckFamilies(root);
        Console.WriteLine("通过：API 契约、真实旧消费方编译运行、精确迁移摘要、基线恢复保护、命名冲突与无写入拒绝。");

        ModelGenerationPlan Plan()
        {
            var diagnostics = new DiagnosticCollector();
            return ModelGenerator.Prepare(data, code, diagnostics) ?? throw new Exception(string.Join("\n", diagnostics.Snapshot()));
        }
        void Edit(Action<IXLWorksheet> edit) { using var book = new XLWorkbook(workbook); edit(book.Worksheet(1)); book.Save(); }
        string Alter(Action<JsonNode> alter) { var node = JsonNode.Parse(goodBaseline)!; alter(node); return node.ToJsonString(); }
    }

    private static void CheckCachedHelperNames(string parent)
    {
        string root=Path.Combine(parent,"cached-helper-names"),data=Path.Combine(root,"Data"),code=Path.Combine(root,"Config");
        Directory.CreateDirectory(data);
        Write(Path.Combine(data,"Skill.xlsx"),"Skill",["Id"],["int"]);
        Write(Path.Combine(data,"Collision.xlsx"),"Collision",["Id","Skill","CachedSkill","TryGetCachedSkill"],["int","SkillConfig","SkillConfig","int"]);
        var diagnostics=new DiagnosticCollector();
        var plan=ModelGenerator.Prepare(data,code,diagnostics);
        Check(plan!=null,string.Join("\n",diagnostics.Snapshot()));Apply(plan!);
        string generated=File.ReadAllText(Path.Combine(code,"Generated","CollisionConfig.g.cs"));
        Check(generated.Contains("bool TryGetCachedSkill1(",StringComparison.Ordinal)&&generated.Contains("bool TryGetCachedCachedSkill(",StringComparison.Ordinal),"Real cached-reference helpers collided with an ungenerated legacy alias.");
        using var compiled=RuleCompiler.PrepareSources(code,"",Path.Combine(root,"cache"),diagnostics);
        Check(compiled!=null,string.Join("\n",diagnostics.Snapshot()));
        Check(!ModelGenerator.Prepare(data,code,new DiagnosticCollector())!.HasChanges,"Cached helper names changed on repeat generation.");
    }

    private static void CheckDeclarations()
    {
        const string original = "#nullable enable\nnamespace N { public class A { public A(int id) {} public string Get(int id, string label = \"a\") => label; public int X { get; private set; } public const int Limit = 2; } }";
        GeneratedApiContract Read(string source) => GeneratedApiContracts.Capture("C", new Dictionary<string, string> { ["A.g.cs"] = source });
        var baseline = Read(original);
        Check(GeneratedApiContracts.Digest(baseline)==GeneratedApiContracts.Hash(JsonSerializer.Serialize(baseline with{DescriptorDigest=null,AcceptedParentDigest=null})),"Streaming API digest changed persisted contract identity.");
        var summary=new SchemaSummary(1,"C",[]) { ApiContract=baseline with { Symbols=[new("large","value",new string('中',20000)+"😀")]} };
        Check(summary.Serialize()==JsonSerializer.Serialize(summary,new JsonSerializerOptions{WriteIndented=true})+"\n","Incremental API ledger serialization changed existing bytes across buffer boundaries.");
        var binding=new GeneratedNameBinding(new string('a',64),"java","zrconfig.Example","$generated/zrconfig.Example/find","reserved","find",false);
        string originalBinding=JsonSerializer.Serialize(new{binding.SymbolId,binding.Language,binding.Scope,binding.SourceBinding,binding.Kind,binding.Name,binding.Retired});
        Check(JsonSerializer.Serialize(binding)==originalBinding,"Default name binding encoding changed existing descriptor digests.");
        var compactOptions=new JsonSerializerOptions();compactOptions.Converters.Add(new GeneratedNameBindingJsonConverter(true));
        foreach(var name in new[]{binding,binding with{Scope="custom.Example"},binding with{Kind="member",SourceBinding="table/A/field/Id",Retired=true}})
            Check(JsonSerializer.Deserialize<GeneratedNameBinding>(JsonSerializer.Serialize(name,compactOptions))==name,"Compact binding failed to preserve identity or retirement.");
        var policy=new GeneratedApiPolicy(1,Guid.NewGuid().ToString("N"),"legacy-v1","1.0",[GeneratedApiPolicies.Profile("java")],[binding],[],["legacy-v1/preserve-query-and-lifetime"]);
        var largeContract=GeneratedApiContracts.Seal(baseline with{Policy=policy,CompiledSymbols=[]},null);
        var largeSummary=new SchemaSummary(1,"C",Enumerable.Range(0,257).Select(i=>new TableSummary("T"+i,"hash",[],[],[])).ToArray()){ApiContract=largeContract};
        var restored=JsonSerializer.Deserialize<SchemaSummary>(largeSummary.Serialize())!;
        Check(restored.SupportedFormat&&restored.NameEncoding=="compact-bindings-v1"&&restored.ApiContract!.Policy!.Names.SequenceEqual(policy.Names),"Compact ledger did not restore the full name policy.");
        GeneratedApiContracts.Validate(restored.ApiContract!);
        Check(restored.Serialize()==largeSummary.Serialize(),"Compact baseline persistence is not deterministic.");
        Check(!(restored with{NameEncoding="future-format"}).SupportedFormat,"Unknown name encoding was accepted.");
        foreach(string malformed in new[]{"[\"id\",\"java\",\"scope\",null,\"member\",\"name\",false]","[\"id\",\"java\",\"scope\",null,\"reserved\",\"name\",false,0]"})
        {
            bool rejected=false;try{JsonSerializer.Deserialize<GeneratedNameBinding>(malformed);}catch(JsonException){rejected=true;}
            Check(rejected,"Malformed compact binding was accepted.");
        }
        foreach (string changed in new[] { original.Replace("int id", "long id"), original.Replace("int id", "int key"),
            original.Replace("string Get", "object Get"), original.Replace("label = \"a\"", "label = \"b\""), original.Replace("get; private set;", "get; set;"),
            original.Replace("Limit = 2", "Limit = 3"), original.Replace("#nullable enable", "#nullable disable"),
            original.Replace("public class A", "public sealed class A"), original.Replace("public A(int id)", "internal A(int id)"),
            original.Replace("public int X", "public int Get(long id) => 0; public int X") })
            Check(GeneratedApiContracts.Compare(baseline, Read(changed)).Length != 0, "Changed public contract missed: " + changed);
        Check(GeneratedApiContracts.Compare(baseline, Read(original.Replace("=> label", "=> label + id"))).Length == 0, "Method implementation entered C# API.");
        Check(GeneratedApiContracts.Compare(baseline, Read(original.Replace("public int X", "// docs\npublic void Added() {} public int X"))).Length == 0, "Independent method rejected.");
        Check(GeneratedApiContracts.Compare(baseline, Read(original.Replace("int id", "int /* documentation */ id"))).Length == 0, "Comment entered API.");
    }

    private static void CheckNativeContracts()
    {
        GeneratedApiContract Read(string path, string source) => GeneratedApiContracts.Capture("S", new Dictionary<string, string> { [path] = source });
        foreach (var (path, prefix) in new[] { ("Go/a.go", "//"), ("Lua/a.lua", "--"), ("Rust/a.rs", "//"), ("TypeScript/a.ts", "//") })
        {
            var a = Read(path, "let value = \"// literal --\";\n");
            Check(GeneratedApiContracts.Compare(a, Read(path, prefix + " comment\nlet value = \"// literal --\"; " + prefix + " changed\n")).Length == 0, "Native comments changed contract: " + path);
            Check(GeneratedApiContracts.Compare(a, Read(path, "let value = \"// different --\";\n")).Length != 0, "String contents discarded: " + path);
        }
        foreach(var item in new (string Path,string Source,string Member)[]{
            ("Java/Entry.java","public final class Entry { private int hidden; public String find(int id) { return \"old\"; } }","find"),
            ("Go/entry.go","package sample\ntype Entry struct { Id int\n hidden int }\nfunc (e *Entry) Find(id int) string { return \"old\" }\n","Find"),
            ("Cpp/entry.hpp","namespace sample { class Entry { int hidden; public: const char* find(int id) const noexcept { return \"old\"; } }; }","find"),
            ("Rust/entry.rs","pub struct Entry { pub id:i32, hidden:i32 } impl Entry { pub fn find(&self,id:i32)->&str { \"old\" } }","find"),
            ("TypeScript/entry.ts","export class Entry { private hidden=1; find(id:number):string { return \"old\"; } }","find"),
            ("Lua/entry.lua","local M={}\nfunction M.find(id) return 'old' end\nreturn M","find")})
        {
            var contract=Read(item.Path,item.Source);var symbols=contract.NativeSymbols!;
            Check(symbols.Any(s=>s.Name==item.Member),"Native method declaration missing: "+item.Path);
            Check(!symbols.Any(s=>s.Name=="hidden"),"Private field was promoted into public declarations: "+item.Path);
            Check(symbols.SequenceEqual(Read(item.Path,item.Source.Replace("old","new")).NativeSymbols!),"Method body entered native declaration surface: "+item.Path);
            Check(GeneratedApiContracts.Compare(contract,Read(item.Path,item.Source.Replace("find","changed").Replace("Find","Changed"))).Any(s=>s.Contains("原生公开声明")),"Native API diagnostic is still only an opaque file hash: "+item.Path);
        }
        string schema(string description, string hash) => JsonSerializer.Serialize(new { name = "A", hash, fields = new[] { new { name = "Id", description, type = "int" } }, types = Array.Empty<object>() });
        string java(string value) => "static String schema() { return String.join(\"\", " + JsonSerializer.Serialize(value[..20]) + ", " + JsonSerializer.Serialize(value[20..]) + "); }";
        Check(GeneratedApiContracts.Compare(Read("Java/A.java", java(schema("old", "123"))), Read("Java/A.java", java(schema("new", "456")))).Length == 0, "Java chunked schema descriptions leaked into API.");
        Check(GeneratedApiContracts.Compare(Read("Lua/a.lua", "---@field Id integer"), Read("Lua/a.lua", "---@field Id string")).Length != 0, "Lua type annotations discarded.");
        const string annotatedLua="---@class Entry\n---@field Id integer\nlocal M={}\n---@param id integer\n---@return integer\nfunction M.find(id) return id end\nreturn M\n";
        var lfFiles=new Dictionary<string,string>{{"Lua/entry.lua",annotatedLua}};
        var crlfFiles=new Dictionary<string,string>{{"Lua/entry.lua",annotatedLua.Replace("\n","\r\n",StringComparison.Ordinal)}};
        var lf=GeneratedApiContracts.Capture("S",lfFiles);var crlf=GeneratedApiContracts.Capture("S",crlfFiles);
        Check(GeneratedApiContracts.Digest(lf)==GeneratedApiContracts.Digest(crlf),"Lua annotation line endings changed the API descriptor.");
        var luaPolicy=new GeneratedApiPolicy(1,Guid.NewGuid().ToString("N"),"legacy-v1","1.0",[GeneratedApiPolicies.Profile("lua")],[],[],["legacy-v1/preserve-query-and-lifetime"]);
        var sealedLua=GeneratedApiContracts.Seal(lf with{Policy=luaPolicy,CompiledSymbols=[]},null);
        string luaSummary=new SchemaSummary(1,"S",[]){ApiContract=sealedLua}.Serialize();
        ApiReleaseVerifier.Verify(luaSummary,lfFiles,luaSummary,crlfFiles);
        ApiReleaseVerifier.Verify(luaSummary,crlfFiles,luaSummary,lfFiles);
        Check(GeneratedApiContracts.Compare(lf,Read("Lua/entry.lua",crlfFiles["Lua/entry.lua"].Replace("integer","string",StringComparison.Ordinal))).Length!=0,"CRLF annotation type changes were ignored.");
        Check(GeneratedApiContracts.NativeSource("local value=[[first\nsecond]]",true)!=GeneratedApiContracts.NativeSource("local value=[[first\r\nsecond]]",true),"Annotation normalization altered literal contents.");
        Check(GeneratedApiContracts.NativeSource("fn f<'a>(s: &'a str) { let c = 'x'; }", false, true).Contains("&'a"), "Rust lifetime lexer failed.");
        const string luaLiterals="local M={}\nlocal a=[[function M.fake() end]]\nlocal b=[==[ ]=] function M.alsoFake() end ]==]\nlocal c=a[1]\nfunction M.real(value) return b end\nreturn M";
        var luaSymbols=Read("Lua/literals.lua",luaLiterals).NativeSymbols!;
        Check(luaSymbols.Any(s=>s.Name=="real")&&!luaSymbols.Any(s=>s.Name is "fake" or "alsoFake"),"Lua long literals entered declarations or hid a later function.");
    }

    private static void CheckMigrationHistory(string parent)
    {
        string root=Path.Combine(parent,"migration-history"),data=Path.Combine(root,"Data"),code=Path.Combine(root,"Config");
        Directory.CreateDirectory(data);string workbook=Path.Combine(data,"A.xlsx"),summaryPath=Path.Combine(code,".schema.json");
        ModelGenerationPlan Plan()=>ModelGenerator.Prepare(data,code,new DiagnosticCollector())??throw new Exception("History preview failed.");
        Write(workbook,"A",["Id","Name"],["int","string"]);Apply(Plan());
        string trusted=File.ReadAllText(summaryPath);var trustedSources=GeneratedSources(Capture(code));
        foreach(string field in new[]{"Title","Label"})
        {
            Write(workbook,"A",["Id",field],["int","string"]);var migration=Plan();
            Check(AcceptTestMigration(migration,migration.ApiChangeId!),"History migration acceptance failed.");
            Check(AcceptTestMigration(migration,migration.ApiChangeId!),"Repeated exact acceptance failed.");Apply(migration);
        }
        string current=File.ReadAllText(summaryPath);var candidateSources=GeneratedSources(Capture(code));
        var summary=JsonSerializer.Deserialize<SchemaSummary>(current)!;
        Check(summary.ApiMigrationHistory?.Length==2&&summary.ApiContract?.Policy?.ContractVersion=="3.0","Migration history duplicated or lost a revision.");
        ApiReleaseVerifier.Verify(trusted,trustedSources,current,candidateSources);
        Check(!Plan().HasChanges,"Migration history made an identical generation dirty.");
        bool Refused(SchemaSummary changed){try{ApiReleaseVerifier.Verify(trusted,trustedSources,JsonSerializer.Serialize(changed),candidateSources);return false;}catch(InvalidDataException){return true;}}
        Check(Refused(summary with{ApiMigrationHistory=[summary.ApiMigrationHistory![1]]}),"Missing first migration bypassed trusted baseline.");
        Check(Refused(summary with{ApiMigrationHistory=summary.ApiMigrationHistory!.Reverse().ToArray()}),"Reordered migration chain was accepted.");
        Check(Refused(summary with{LastMigration=summary.ApiMigrationHistory![0].Receipt}),"Stale latest receipt was accepted.");
        File.WriteAllText(summaryPath,JsonSerializer.Serialize(summary with{ApiMigrationHistory=[]}));
        var diagnostic=new DiagnosticCollector();var rejected=ModelGenerator.Prepare(data,code,diagnostic);
        Check(rejected!=null&&!rejected.Apply(diagnostic,true)&&diagnostic.Snapshot().Any(d=>d.Code=="API_BASELINE_INVALID"),"Corrupt migration history was overwritten.");
    }

    private static void CheckNames(string root)
    {
        foreach (var (name, fields, language, expected) in new[] {
            ("class", new[] { "Id" }, "java", false), ("bundle", new[] { "Id" }, "java", false),
            ("A", new[] { "Id", "v_Id" }, "cpp", false), ("A", new[] { "Id", "class", "class_" }, "cpp", false),
            ("_Item", new[] { "Id" }, "go", true), ("_Item",new[]{"Id"},"cpp",false),
            ("Version",new[]{"Id"},"go",false), ("A",new[]{"Id","AConfig"},"cpp",false) })
        {
            string data = Path.Combine(root, "names", language + name + fields.Length, "Data"), code = Path.Combine(Path.GetDirectoryName(data)!, "Config");
            Directory.CreateDirectory(data);
            Write(Path.Combine(data, "Input.xlsx"), name, fields, fields.Select(_ => "int").ToArray());
            var diagnostics = new DiagnosticCollector();
            ModelGenerationPlan? plan = ModelGenerator.Prepare(data, code, diagnostics, languages: [language]);
            Check((plan != null) == expected, string.Join("\n", diagnostics.Snapshot()) + " Bad name preview result: " + name);
            if (expected)
            {
                Apply(plan!);
                Check(File.Exists(Path.Combine(code, "Generated", "Go", "table__item.g.go")), "Go still ignores the underscore source file.");
            }
            else Check(!Directory.Exists(Path.Combine(code, "Generated")), "Rejected naming plan wrote output.");
        }
    }

    private static void CheckFamilies(string root)
    {
        string data=Path.Combine(root,"families","Data"),code=Path.Combine(root,"families","Config");
        Directory.CreateDirectory(data);
        Write(Path.Combine(data,"Server.xlsx"),"Server",["Id","Snapshot","Borrow"],["int","int","int"]);
        var diagnostics=new DiagnosticCollector();
        var legacy=ModelGenerator.PrepareWithFamily("legacy-v1",data,code,diagnostics,languages:["all"]);
        Check(legacy!=null,string.Join("\n",diagnostics.Snapshot())); Apply(legacy!);
        var files=Capture(code);
        var inherited=ModelGenerator.Prepare(data,code,new DiagnosticCollector(),languages:["all"]);
        Check(inherited!=null&&!inherited.HasChanges,"Legacy family was silently changed.");
        diagnostics=new DiagnosticCollector();
        Check(ModelGenerator.PrepareWithFamily("server-v1",data,code,diagnostics,languages:["all"])==null && diagnostics.Snapshot().Any(d=>d.Code=="SYMBOL_PLAN"),"New-family collision was not diagnosed.");
        Same(files,Capture(code));
        var settings=new WorkspaceSettings(data,"",code,""){RuntimeFamily="legacy-v1"};
        Check(settings.Validate(generation:true).RuntimeFamily=="legacy-v1","Validation discarded the API family.");
        Check(ToolCommandLine.Parse(["codegen","--runtime-family","server-v1"]).RuntimeFamily=="server-v1","CLI discarded family selection.");
    }

    private static void Consumer(string code)
    {
        const string source = """
            using System;
            using ZrConfig.Models;
            public static class FrozenConsumer {
                public static bool Run() {
                    var row = new AConfig(id: 7, name: "business");
                    var table = new AConfigTable(new[] { row });
                    Func<int, AConfig> get = table.Get;
                    return ReferenceEquals(get(7), row) && table.TryGet(7, out var found) && found.Name == "business";
                }
            }
            """;
        var trees = RuleCompiler.EnumerateSources(code).Select(p => CSharpSyntaxTree.ParseText(File.ReadAllText(p))).Append(CSharpSyntaxTree.ParseText(source));
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Where(p => Path.GetDirectoryName(p) == Path.GetDirectoryName(typeof(object).Assembly.Location))
            .Append(typeof(ITableCodec).Assembly.Location).Distinct().Select(p => MetadataReference.CreateFromFile(p));
        using var bytes = new MemoryStream();
        var result = CSharpCompilation.Create("Consumer" + Guid.NewGuid().ToString("N"), trees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,generalDiagnosticOption:ReportDiagnostic.Error)).Emit(bytes);
        Check(result.Success, string.Join("\n", result.Diagnostics));
        Check((bool)Assembly.Load(bytes.ToArray()).GetType("FrozenConsumer")!.GetMethod("Run")!.Invoke(null, null)!, "Old consumer behavior changed.");
    }

    // Unit tests of rollback/removal inject a receipt as a test double. Release
    // verification uses ApiMigrationVerifier and real frozen consumer commands.
    internal static bool AcceptTestMigration(ModelGenerationPlan plan,string changeId)
    {
        return CreateTestReceipt(plan) is {} receipt && plan.AcceptApiMigration(changeId,receipt);
    }
    internal static ApiMigrationReceipt? CreateTestReceipt(ModelGenerationPlan plan)
    {
        if(plan.MigrationRequest is not {} request)return null;
        string fixture=GeneratedApiContracts.Hash("unit-test receipt fixture");
        var results=request.Languages.Select(language=>new ApiConsumerEvidence(language,"unit-test-double",fixture,fixture,fixture,fixture)).ToArray();
        return new(request,results,ApiMigrationReceipt.ContentDigest(request,results));
    }
    private static ApiMigrationReceipt VerifyMigration(ModelGenerationPlan plan,string root)
    {
        string executable=Environment.ProcessPath!;
        var arguments=new List<string>();
        if(Path.GetFileNameWithoutExtension(executable).Equals("dotnet",StringComparison.OrdinalIgnoreCase))arguments.Add(Assembly.GetEntryAssembly()!.Location);
        arguments.AddRange(["--api-migration-consumer","{candidate}"]);
        string checks=Path.Combine(root,"consumer-checks.json");
        File.WriteAllText(checks,JsonSerializer.Serialize(new[]{new ApiConsumerCheck("csharp","net10-migrated-business",executable,arguments.ToArray(),root,[Assembly.GetExecutingAssembly().Location])}));
        return ApiMigrationVerifier.VerifyAsync(plan,checks).GetAwaiter().GetResult();
    }
    private static void CheckVerificationIntegrity(ModelGenerationPlan plan,string root)
    {
        string executable=Environment.ProcessPath!,inputs=Path.Combine(root,"consumer-dependencies");Directory.CreateDirectory(inputs);string dependency=Path.Combine(inputs,"dependency.txt");
        foreach(string mode in new[]{"add","replace","delete","input","fail"})
        {
            File.WriteAllText(dependency,"frozen input");var args=new List<string>();
            if(Path.GetFileNameWithoutExtension(executable).Equals("dotnet",StringComparison.OrdinalIgnoreCase))args.Add(Assembly.GetEntryAssembly()!.Location);
            args.AddRange(["--api-migration-mutator","{candidate}",mode,dependency]);
            string checks=Path.Combine(root,"integrity-checks.json");File.WriteAllText(checks,JsonSerializer.Serialize(new[]{new ApiConsumerCheck("csharp","integrity-probe",executable,args.ToArray(),root,[Assembly.GetExecutingAssembly().Location,inputs])}));
            bool rejected=false;try{ApiMigrationVerifier.VerifyAsync(plan,checks).GetAwaiter().GetResult();}
            catch(Exception error)when(error is IOException or InvalidDataException){rejected=mode!="fail"||error.Message.Contains("bounded-failure-marker",StringComparison.Ordinal)&&error.Message.Length<18000;}
            Check(rejected,"Verification accepted changed candidate/consumer inputs or lost bounded failure output: "+mode);
        }
        Console.WriteLine("通过：迁移验收拒绝候选增删改与依赖变化，失败输出保留有界尾部。");
    }
    internal static void MigrationMutationProbe(string candidate,string mode,string dependency)
    {
        MigrationConsumer(candidate);
        switch(mode){case "add":File.WriteAllText(Path.Combine(candidate,"Extra.cs"),"class Extra {}");break;case "replace":File.AppendAllText(Path.Combine(candidate,"Generated","AConfig.g.cs"),"\n// changed");break;case "delete":File.Delete(Path.Combine(candidate,"Generated","AConfig.g.cs"));break;case "input":File.AppendAllText(dependency,"changed");break;case "fail":Console.Write(new string('x',100000));Console.Error.WriteLine("bounded-failure-marker");Environment.Exit(7);break;default:throw new ArgumentException(mode);}
    }
    internal static void MigrationConsumer(string candidate)
    {
        // This migration intentionally renames Name to Title. The reviewed new
        // consumer is compiled and run separately; the old consumer remains frozen.
        const string source="using System;using ZrConfig.Models;public static class MigrationConsumer{public static bool Run(){var row=new AConfig(id:7,title:\"business\");var table=new AConfigTable(new[]{row});Func<int,AConfig> get=table.Get;return ReferenceEquals(get(7),row)&&table.Get(7).Title==\"business\";}}";
        var trees=RuleCompiler.EnumerateSources(candidate).Select(p=>CSharpSyntaxTree.ParseText(File.ReadAllText(p))).Append(CSharpSyntaxTree.ParseText(source));
        var references=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Where(p=>Path.GetDirectoryName(p)==Path.GetDirectoryName(typeof(object).Assembly.Location)).Append(typeof(ITableCodec).Assembly.Location).Distinct().Select(p=>MetadataReference.CreateFromFile(p));
        using var bytes=new MemoryStream();
        var result=CSharpCompilation.Create("MigrationConsumer",trees,references,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,generalDiagnosticOption:ReportDiagnostic.Error)).Emit(bytes);
        Check(result.Success,string.Join("\n",result.Diagnostics));
        Check((bool)Assembly.Load(bytes.ToArray()).GetType("MigrationConsumer")!.GetMethod("Run")!.Invoke(null,null)!,"Migrated consumer behavior failed.");
    }
    internal static void VerifyTrustedRef(string reference)
    {
        if(reference.Length is < 7 or > 64 || !reference.All(Uri.IsHexDigit))throw new ArgumentException("Use an immutable commit hash for the trusted baseline.");
        string Git(params string[] arguments)
        {
            var start=new System.Diagnostics.ProcessStartInfo("git"){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};
            foreach(string argument in arguments)start.ArgumentList.Add(argument);
            using var process=System.Diagnostics.Process.Start(start)!;
            var output=process.StandardOutput.ReadToEndAsync();var error=process.StandardError.ReadToEndAsync();process.WaitForExit();
            if(process.ExitCode!=0)throw new IOException(error.GetAwaiter().GetResult());return output.GetAwaiter().GetResult();
        }
        string root=Git("rev-parse","--show-toplevel").Trim();
        string[] paths=Git("ls-tree","-rz","--name-only",reference).Split('\0',StringSplitOptions.RemoveEmptyEntries);
        var directories=paths.Where(p=>p.EndsWith("/.schema.json",StringComparison.Ordinal)).Select(p=>p[..^13])
            .Concat(paths.Where(p=>p.EndsWith("/Generated/TableCodecs.g.cs",StringComparison.Ordinal)).Select(p=>p[..^27])).Distinct(StringComparer.Ordinal).ToArray();
        foreach(string directory in directories)
        {
            string metadata=directory+"/.schema.json",prefix=directory+"/Generated/";
            string? previous=paths.Contains(metadata,StringComparer.Ordinal)?Git("show",reference+":"+metadata):null;
            var oldFiles=paths.Where(p=>p.StartsWith(prefix,StringComparison.Ordinal)&&!p.EndsWith(".meta",StringComparison.Ordinal)&&!Path.GetFileName(p).StartsWith('.'))
                .ToDictionary(p=>p[prefix.Length..],p=>Git("show",reference+":"+p));
            string candidateRoot=Path.Combine(root,directory,"Generated"),summary=Path.Combine(root,metadata);
            if(!File.Exists(summary)||!Directory.Exists(candidateRoot))throw new InvalidDataException("Trusted API output was removed: "+directory);
            var files=Directory.GetFiles(candidateRoot,"*",SearchOption.AllDirectories).Where(p=>!p.EndsWith(".meta",StringComparison.Ordinal)&&!Path.GetFileName(p).StartsWith('.')).ToDictionary(p=>Path.GetRelativePath(candidateRoot,p).Replace('\\','/'),File.ReadAllText);
            ApiReleaseVerifier.Verify(previous,oldFiles,File.ReadAllText(summary),files);
        }
        Console.WriteLine("通过：可信提交 "+reference+" 的 "+directories.Length+" 个 API 基线与实际生成源码。");
    }
    private static void Write(string path, string name, string[] fields, string[] types)
    {
        using var book = new XLWorkbook(); var sheet = book.AddWorksheet(name);
        for (int i = 0; i < fields.Length; i++) { sheet.Cell(1, i + 1).Value = fields[i]; sheet.Cell(2, i + 1).Value = types[i]; sheet.Cell(5, i + 1).Value = "CS"; }
        sheet.Cell(3, 1).Value = "主键"; book.SaveAs(path);
    }
    private static void Apply(ModelGenerationPlan plan)
    {
        var diagnostics = new DiagnosticCollector(); Check(plan.Apply(diagnostics, true), string.Join("\n", diagnostics.Snapshot()));
    }
    private static Dictionary<string, byte[]> Capture(string root) => Directory.GetFiles(root, "*", SearchOption.AllDirectories)
        .Where(p => !p.Contains(".types-cache") && !p.EndsWith(".lock")).ToDictionary(p => p, File.ReadAllBytes);
    private static Dictionary<string,string> GeneratedSources(Dictionary<string,byte[]> files) => files.Where(f=>f.Key.Contains(Path.DirectorySeparatorChar+"Generated"+Path.DirectorySeparatorChar,StringComparison.Ordinal)&&!f.Key.EndsWith(".meta",StringComparison.Ordinal)&&!Path.GetFileName(f.Key).StartsWith('.')).ToDictionary(f=>f.Key[(f.Key.IndexOf(Path.DirectorySeparatorChar+"Generated"+Path.DirectorySeparatorChar,StringComparison.Ordinal)+11)..].Replace('\\','/'),f=>System.Text.Encoding.UTF8.GetString(f.Value));
    private static void Same(Dictionary<string, byte[]> before, Dictionary<string, byte[]> after) =>
        Check(before.Count == after.Count && before.All(p => after.TryGetValue(p.Key, out var bytes) && p.Value.SequenceEqual(bytes)), "Rejected/no-op generation modified files.");
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
