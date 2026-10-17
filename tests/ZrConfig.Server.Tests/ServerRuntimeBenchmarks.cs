using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using ClosedXML.Excel;
using ZrConfig.Core;
using ZrConfig.Validation;

// Independent processes keep cold loads separate from warmed query loops. All input lives
// under the system temporary directory; only measurements are saved under artifacts.
internal static class ServerRuntimeBenchmarks
{
    private sealed record Driver(string Language,string Executable,string[] Arguments,string Directory);
    private static readonly JsonSerializerOptions JsonOptions = new(){WriteIndented=true};
    internal static async Task Run(string destination,int rows=10000,int repeats=3,string? sampleRepository=null,bool retainedScenarios=false,bool schedulingScenarios=false,bool queryScenarios=false,string? profilingRepository=null,bool controlScenarios=false,string? lockRepository=null,int tableCount=3)
    {
#if DEBUG
        throw new InvalidOperationException("Benchmarks require a Release build.");
#else
        if(rows is not(100 or 1000 or 10000 or 100000)||repeats<1||repeats>20)throw new ArgumentOutOfRangeException(nameof(rows));
        if(tableCount is not(3 or 100 or 1000 or 10000))throw new ArgumentOutOfRangeException(nameof(tableCount));
        _ = TypeScriptCompilerHeap();
        destination=Path.GetFullPath(destination);Directory.CreateDirectory(destination);
        string root=Path.Combine(Path.GetTempPath(),"zrconfig-benchmark-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        var results=new List<JsonObject>();var builds=new List<object>();
        string benchmarkDigest=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(Assembly.GetExecutingAssembly().Location))),generatorDigest=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(typeof(ModelGenerator).Assembly.Location)));
        try
        {
            string data=Path.Combine(root,"Data"),code=Path.Combine(root,"Config"),output=Path.Combine(root,"Output");Directory.CreateDirectory(data);Directory.CreateDirectory(code);
            if(sampleRepository!=null)
            {
                rows=10000;
                CopyTree(Path.Combine(sampleRepository,"samples","Data"),data,f=>f.EndsWith(".xlsx",StringComparison.OrdinalIgnoreCase)&&!Path.GetFileName(f).StartsWith("~$",StringComparison.Ordinal));
                string sampleCode=Path.Combine(sampleRepository,"unity","ZrConfig.Sample","Assets","HotUpdate","Config");
                foreach(string part in new[]{"CustomTypes","Serialization"})CopyTree(Path.Combine(sampleCode,part),Path.Combine(code,part),f=>f.EndsWith(".cs",StringComparison.Ordinal));
                File.Copy(Path.Combine(sampleCode,"Tables.json"),Path.Combine(code,"Tables.json"));
            }
            else
            {
            Write(data,"Bench",["Id","Group","Text","Values","Peer"],["int","int","string","json<int[][]>","LookupConfig"],rows,i=>[i.ToString(),(i%100).ToString(),i%97==0?new string('x',4096):"row-"+i,"[[1,2,3],[4,5,6]]",(i%64+1).ToString()]);
            Write(data,"Lookup",["Id","Peer"],["int","LookupConfig"],64,i=>[i.ToString(),(i%64+1).ToString()]);
            Write(data,"Wide",["Id",..Enumerable.Range(1,24).Select(i=>"Value"+i)],["int",..Enumerable.Repeat("long",24)],rows,i=>[i.ToString(),..Enumerable.Range(1,24).Select(j=>(10000000000L+i*j).ToString(CultureInfo.InvariantCulture))]);
            File.WriteAllText(Path.Combine(code,"Tables.json"),"{\"Bench\":{\"groups\":{\"ByGroup\":[\"Group\"]}}}");
            if(tableCount>3)
            {
                string? savedWorkbook=Environment.GetEnvironmentVariable("ZRCONFIG_BENCH_TABLE_WORKBOOK");
                if(savedWorkbook!=null)File.Copy(Path.GetFullPath(savedWorkbook),Path.Combine(data,"TinyTables.xlsx"));
                else
                {
                using var book=new XLWorkbook();
                for(int i=3;i<tableCount;i++)
                {
                    var sheet=book.AddWorksheet("Tiny"+i.ToString("D5",CultureInfo.InvariantCulture));
                    sheet.Cell(1,1).Value="Id";sheet.Cell(2,1).Value="int";sheet.Cell(3,1).Value="主键";
                    sheet.Cell(4,1).Value="Scale fixture";sheet.Cell(5,1).Value="CS";sheet.Cell(6,1).Value=1;
                }
                book.SaveAs(Path.Combine(data,"TinyTables.xlsx"));
                }
                Console.WriteLine("Scale fixture written: "+tableCount+" tables.");
            }
            if(queryScenarios)
            {
                Write(data,"Combo",["Zone","Id"],["string","int"],rows,i=>["east",i.ToString(CultureInfo.InvariantCulture)]);
                using var combo=new XLWorkbook(Path.Combine(data,"Combo.xlsx"));combo.Worksheet(1).Cell(3,2).Value="主键";combo.Save();
            }
            }
            var watch=Stopwatch.StartNew();var settings=new WorkspaceSettings(data,output,code,""){Target=ConfigTarget.Server,CodeLanguages=["all"],DataFormat="both"};
            string previousStage="";double lastProgress=-30;
            var operation=tableCount>3?new WorkflowOperation(progress:new Progress<OperationProgress>(value=>{
                string stage=value.Stage is "writing" or "verifying"?"exporting":value.Stage;
                if(stage!=previousStage||value.Elapsed.TotalSeconds-lastProgress>=30){previousStage=stage;lastProgress=value.Elapsed.TotalSeconds;Console.WriteLine($"scale {value.Elapsed}: {stage}: {value.Message}");}
            })):null;
            var generated=await ProjectWorkflow.ExportAsync(settings,_=>Task.FromResult(true),operation:operation);if(!generated.Success)throw new Exception(string.Join("\n",generated.PreparationDiagnostics.Concat(generated.Export?.Diagnostics??[])));
            builds.Add(new{language="generator-and-export",milliseconds=watch.Elapsed.TotalMilliseconds,source_bytes=Directory.GetFiles(Path.Combine(code,"Generated"),"*",SearchOption.AllDirectories).Sum(f=>new FileInfo(f).Length),api_ledger_bytes=new FileInfo(Path.Combine(code,".schema.json")).Length});
            if(tableCount>3)
            {
                Dictionary<string,(string Digest,long Ticks)> CaptureGenerated()
                {
                    return Directory.GetFiles(Path.Combine(code,"Generated"),"*",SearchOption.AllDirectories)
                        .Concat(new[]{Path.Combine(code,".schema.json"),Path.Combine(code,".generation.json")})
                        .ToDictionary(path=>Path.GetRelativePath(code,path),path=>{
                            using var stream=File.OpenRead(path);
                            return(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)),File.GetLastWriteTimeUtc(path).Ticks);
                        },StringComparer.Ordinal);
                }
                var before=CaptureGenerated();watch.Restart();
                var repeated=await ProjectWorkflow.GenerateAsync(settings,_=>Task.FromResult(false),operation:operation);
                if(!repeated.Success||repeated.Plan==null||repeated.Plan.HasChanges)throw new Exception("Scale repeat generation changed the accepted contract: "+string.Join("\n",repeated.PreparationDiagnostics));
                var after=CaptureGenerated();
                if(before.Count!=after.Count||before.Any(pair=>!after.TryGetValue(pair.Key,out var value)||value!=pair.Value))throw new Exception("Scale repeat generation changed file content, timestamps or the file set.");
                builds.Add(new{language="generator-repeat",milliseconds=watch.Elapsed.TotalMilliseconds,unchanged_files=after.Count});
                Console.WriteLine($"Scale repeat generation unchanged: {after.Count} files.");
            }
            if(controlScenarios)ServerRuntimeControlBenchmarks.WriteGraphs(output,builds);
            var drivers=await Compile(root,code,builds,sampleRepository!=null,schedulingScenarios,queryScenarios,profilingRepository,controlScenarios,lockRepository);
            foreach(Driver driver in drivers)
            foreach(string format in new[]{"binary","json"})
            foreach(var profile in schedulingScenarios||queryScenarios||controlScenarios ? new[]{("lazy",1)} : retainedScenarios ? new[]{("full",100),("lazy",1)} : new[]{("full",100),("selected",100),("strict",1),("lazy",1),("lazy",10),("lazy",100)})
            foreach(int retained in retainedScenarios?new[]{0,1,4}:new[]{0})
            for(int repeat=0;repeat<repeats;repeat++)
            {
                var result=await Execute(driver.Executable,[..driver.Arguments,output,format,profile.Item1,profile.Item2.ToString(CultureInfo.InvariantCulture),rows.ToString(CultureInfo.InvariantCulture),retained.ToString(CultureInfo.InvariantCulture)],driver.Directory);
                string line=result.Output.Split('\n').Last(s=>s.StartsWith('{'));var value=JsonNode.Parse(line)!.AsObject();
                var setup=result.Output.Split('\n').Where(s=>s.StartsWith('{')).Select(s=>JsonNode.Parse(s)!).SingleOrDefault(s=>s["benchmark_setup"]!=null);
                if(setup!=null)value["setup"]=setup;
                if(profilingRepository!=null)
                {
                    var evidence=result.Output.Split('\n').Where(s=>s.StartsWith('{')).Select(s=>JsonNode.Parse(s)!).Single(s=>s["scenario"]?.GetValue<string>()=="stages");
                    var stages=evidence["stages"]!.AsArray();
                    if(stages.Count!=9||stages.Any(s=>s!["calls"]!.GetValue<long>()<=0||s["inclusive_ns"]!.GetValue<long>()<0))throw new Exception("Missing instrumented stage: "+driver.Language+" "+evidence.ToJsonString());
                    value["stage_profile"]=evidence;
                }
                if(schedulingScenarios||queryScenarios)
                {
                    var scenarios=result.Output.Split('\n').Where(line=>line.StartsWith('{')).Select(line=>JsonNode.Parse(line)!).Where(item=>item["scenario"]!=null).ToArray();
                    if(queryScenarios ? scenarios.Length!=6||scenarios.Any(item=>item["scenario"]!.GetValue<string>()!="query") : scenarios.Length!=(driver.Language is "lua" or "typescript"?4:5)||scenarios.Count(item=>item["scenario"]!.GetValue<string>()=="source")!=3)throw new Exception("Missing scenario evidence: "+driver.Language);
                    if(queryScenarios)ValidateQueries(scenarios,rows,driver.Language);
                    value["scenarios"]=new JsonArray(scenarios);
                }
                if(controlScenarios)
                {
                    var scenarios=result.Output.Split('\n').Where(s=>s.StartsWith('{')).Select(s=>JsonNode.Parse(s)!).Where(s=>s["scenario"]!=null).ToArray();
                    ServerRuntimeControlBenchmarks.Validate(scenarios,driver.Language,lockRepository!=null);
                    value["control_scenarios"]=new JsonArray(scenarios);
                }
                value["language"]=driver.Language;value["format"]=format;value["mode"]=profile.Item1;value["coverage_percent"]=profile.Item2;value["repeat"]=repeat;value["retained_other_snapshots"]=retained;value["process_ms"]=result.Milliseconds;value["process_peak_working_set_bytes"]=result.Peak;
                long count=(long)rows*profile.Item2/100,expected=count*(count+1)/2+(sampleRepository==null?rows/100:1);for(int i=0;i<120000;i++)expected+=i%count+1;
                if(value["checksum"]!.GetValue<long>()!=expected)throw new Exception("Benchmark checksum mismatch: "+driver.Language);
                if(tableCount>3&&profile.Item1=="full"&&value["sparse_loaded_rows"]!.GetValue<long>()!=2L*rows+64+tableCount-3)throw new Exception("Scale full-load row count: "+driver.Language);
                results.Add(value);await Save();Console.WriteLine($"benchmark {driver.Language} {format} {profile.Item1} {profile.Item2}% held={retained} {repeat+1}/{repeats}: {value["cold_ms"]} ms");
            }
            await Save();
            async Task Save()=>await File.WriteAllTextAsync(Path.Combine(destination,"server-runtime.json"),JsonSerializer.Serialize(new{
                format="ZrConfig.ServerBenchmark/1",created_utc=DateTimeOffset.UtcNow,fixture=sampleRepository==null?"synthetic":"samples/Data",input_sha256=Directory.GetFiles(data,"*.xlsx",SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToDictionary(f=>Path.GetRelativePath(data,f),f=>Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(f)))),seed=1729,rows,repeats,table_count=tableCount,subprocess_timeout_seconds=ProcessTimeout(),retained_scenarios=retainedScenarios,scheduling_scenarios=schedulingScenarios,query_scenarios=queryScenarios,os=RuntimeInformation.OSDescription,architecture=RuntimeInformation.ProcessArchitecture.ToString(),logical_processors=Environment.ProcessorCount,
                cpu=Environment.GetEnvironmentVariable("ZRCONFIG_BENCH_CPU"),physical_memory_bytes=Environment.GetEnvironmentVariable("ZRCONFIG_BENCH_MEMORY_BYTES"),benchmark_sha256=benchmarkDigest,generator_sha256=generatorDigest,stage_profiling=profilingRepository!=null,control_scenarios=controlScenarios,lock_profiling=lockRepository!=null,runtime=RuntimeInformation.FrameworkDescription,gc_server=System.Runtime.GCSettings.IsServerGC,gc_latency=System.Runtime.GCSettings.LatencyMode.ToString(),toolchains=drivers.Select(d=>new{d.Language,d.Executable}),builds,results,
                notes=new[]{"Query scenarios use callbacks after 20000 warmup operations, 1000 batches of 100 calls, fixed two-row hit keys, changing negative misses, alternating populated/empty groups and exact checksums. Callback, bounds checks and safe handle ownership are included. Primary, composite, group, reference and manager acquisition are measured independently; heap trends are not allocation counters.","Scheduling scenarios compare 1 and 4 warmed readers on threaded runtimes, including wake/join overhead but excluding thread creation. TypeScript/Lua measure one state only. Sources inject one millisecond per read (Lua: explicit CPU busy wait), or fail the second read; every failure verifies unchanged manager generation and current snapshot. Extra scenario work is excluded from ordinary cold/retention runs.","Retained scenarios hold 0, 1 or 4 independently loaded snapshots of the same immutable version, then load the measured snapshot; this isolates ownership retention from version-content changes. Retained snapshots use the same selected mode; cold time excludes their initial loading. Correctness of manager retirement is tested separately.","Every sample is a fresh process. No warm-cache claim is made about the OS page cache. The first C# worker compiles its consumer; subsequent workers share the temporary on-disk compilation cache. All cold_ms windows exclude this preparation, while whole-process time/RSS include it; do not compare first-process compiler cost with later runtime-only processes.","Hot percentiles are distributions of 100-operation batch means, after 20000 warmup calls; they are not individual request tail latency.","Peak working set is whole-process high water including runtime, generated code and startup. It is not configuration-only live memory; null means unavailable. ZRCONFIG_BENCH_TIME=1 uses OS child resource accounting.","Lua durations use process CPU time; other drivers use monotonic wall time.","Heap differences in JS/Lua are trends, not allocation counters. Runtime estimates are not measured heap size.","Cold access uses a deterministic permutation of the requested prefix. Coverage is for the target Bench/AllTypes10k table, not every table/dependency/group in the version; full and selected modes do more preparation. First group is measured after sparse access and may scan/decode the table for its derived index.","No prior comparable performance release exists; these measurements establish a baseline, not proof of a 10 percent regression gate."}},JsonOptions));
        }
        catch(Exception error)
        {
            await File.WriteAllTextAsync(Path.Combine(destination,"failure.json"),JsonSerializer.Serialize(new{table_count=tableCount,rows,repeats,benchmark_sha256=benchmarkDigest,generator_sha256=generatorDigest,builds,error=error.ToString()},JsonOptions));
            throw;
        }
        finally
        {
            if(tableCount>3&&Environment.GetEnvironmentVariable("ZRCONFIG_BENCH_KEEP_SCALE")=="1")Console.WriteLine("Scale workspace retained: "+root);
            else Directory.Delete(root,true);
        }
#endif
    }
    private static void ValidateQueries(JsonNode[] scenarios,int rows,string language)
    {
        var expected=new Dictionary<string,long>(StringComparer.Ordinal)
        {
            ["primary-hit"]=150000,["primary-miss"]=100000,["composite-hit"]=150000,
            ["group"]=(long)(rows/100)*50000,["reference"]=200000,["acquire"]=100000
        };
        foreach(var scenario in scenarios)
        {
            string kind=scenario["kind"]!.GetValue<string>();
            if(!expected.Remove(kind,out long checksum)||scenario["checksum"]!.GetValue<long>()!=checksum||scenario["queries"]!.GetValue<int>()!=100000)
                throw new Exception("Query coverage/checksum mismatch: "+language+"/"+kind);
            if(scenario["allocation_kind"]!.GetValue<string>()!="heap-trend"&&scenario["allocation_delta"]!.GetValue<long>()<0)
                throw new Exception("Unavailable allocation counter: "+language+"/"+kind);
            double previous=0;
            foreach(string field in new[]{"p50_ns","p95_ns","p99_ns"})
            {
                double value=scenario[field]!.GetValue<double>();
                if(!double.IsFinite(value)||value<previous)throw new Exception("Invalid query distribution: "+language+"/"+kind+"/"+field);
                previous=value;
            }
            foreach(string field in new[]{"mean_ns","queries_per_second"})
                if(!double.IsFinite(scenario[field]!.GetValue<double>())||scenario[field]!.GetValue<double>()<=0)
                    throw new Exception("Invalid query rate: "+language+"/"+kind+"/"+field);
        }
        if(expected.Count!=0)throw new Exception("Missing query coverage: "+language);
    }
    private static void CopyTree(string source,string destination,Func<string,bool> include)
    {
        foreach(string file in Directory.GetFiles(source,"*",SearchOption.AllDirectories).Where(include))
        {string target=Path.Combine(destination,Path.GetRelativePath(source,file));Directory.CreateDirectory(Path.GetDirectoryName(target)!);File.Copy(file,target);}
    }
    private static void Write(string directory,string name,string[] fields,string[] types,int count,Func<int,string[]> row)
    {
        using var book=new XLWorkbook();var sheet=book.AddWorksheet(name);
        for(int j=0;j<fields.Length;j++){sheet.Cell(1,j+1).Value=fields[j];sheet.Cell(2,j+1).Value=types[j];sheet.Cell(3,j+1).Value=j==0?"主键":"";sheet.Cell(4,j+1).Value="Deterministic benchmark field";sheet.Cell(5,j+1).Value="CS";}
        for(int i=1;i<=count;i++){var values=row(i);for(int j=0;j<values.Length;j++)sheet.Cell(i+5,j+1).Value=values[j];}book.SaveAs(Path.Combine(directory,name+".xlsx"));
    }
    internal static void ManagedWorker(string[] args)
    {
        string code=args[0];var diagnostic=new DiagnosticCollector();var watch=Stopwatch.StartNew();using var loaded=RuleCompiler.PrepareSources(code,"",Path.Combine(code,"obj","benchmark"),diagnostic)??throw new Exception(string.Join("\n",diagnostic.Snapshot()));
        double elapsed=watch.Elapsed.TotalMilliseconds;string assemblyPath=loaded.Assembly.Location;
        Console.WriteLine(JsonSerializer.Serialize(new{benchmark_setup="managed-consumer",prepare_ms=elapsed,consumer_assembly_bytes=File.Exists(assemblyPath)?new FileInfo(assemblyPath).Length:(long?)null}));
        loaded.Assembly.GetType("BenchmarkConsumer")!.GetMethod("Run")!.Invoke(null,[args.Skip(1).ToArray()]);
    }
    private static string Tool(string variable,string fallback)=>Environment.GetEnvironmentVariable(variable)??fallback;
    private static int? TypeScriptCompilerHeap()
    {
        string? configured=Environment.GetEnvironmentVariable("ZRCONFIG_TYPESCRIPT_HEAP_MB");
        if(string.IsNullOrEmpty(configured))return null;
        if(!int.TryParse(configured,NumberStyles.None,CultureInfo.InvariantCulture,out int value)||value is <64 or >65536)
            throw new ArgumentException("ZRCONFIG_TYPESCRIPT_HEAP_MB must be an integer between 64 and 65536.");
        return value;
    }
    private static async Task<List<Driver>> Compile(string root,string code,List<object> builds,bool samples=false,bool schedulingScenarios=false,bool queryScenarios=false,string? profilingRepository=null,bool controlScenarios=false,string? lockRepository=null)
    {
        string Consumer(string source,string language) { if(schedulingScenarios)source=ServerRuntimeScenarios.Extend(source,language);if(queryScenarios)source=ServerRuntimeScenarios.ExtendQueries(source,language);if(profilingRepository!=null)source=ServerRuntimeProfiling.ExtendDriver(source,language);if(controlScenarios)source=ServerRuntimeControlBenchmarks.Extend(source,language);if(lockRepository!=null)source=ServerRuntimeProfiling.ExtendLockDriver(source,language);return samples ? source.Replace("\"Bench\"","\"AllTypes10k\"",StringComparison.Ordinal).Replace("'Bench'","'AllTypes10k'",StringComparison.Ordinal).Replace("_Bench","_AllTypes10k",StringComparison.Ordinal).Replace("GetBenchTable","GetAllTypes10kTable",StringComparison.Ordinal).Replace("ByGroup(0","ById(1",StringComparison.Ordinal).Replace("ByGroup(ctx,0","ById(ctx,1",StringComparison.Ordinal) : source;}
        string generated=Path.Combine(code,"Generated"),java=Tool("ZRCONFIG_JAVAC","javac"),go=Tool("ZRCONFIG_GO","go"),cpp=Tool("ZRCONFIG_CXX","clang++"),rust=Tool("ZRCONFIG_RUSTC","rustc"),lua=Tool("ZRCONFIG_LUA","lua"),node=Tool("ZRCONFIG_NODE","node");
        string? profileRuntime=profilingRepository==null?null:ServerRuntimeProfiling.Prepare(generated,profilingRepository,Path.Combine(root,"profile-runtime"),builds);
        if(lockRepository!=null)profileRuntime=ServerRuntimeProfiling.PrepareLocks(generated,lockRepository,Path.Combine(root,"lock-runtime"),builds);
        if(controlScenarios)ServerRuntimeControlBenchmarks.PrepareInitialization(generated);
        var drivers=new List<Driver>();
        foreach(var tool in new[]{("java",java,"--version"),("go",go,"version"),("cpp",cpp,"--version"),("rust",rust,"--version"),("node",node,"--version"),("lua",lua,"-v")})
        {
            // cl.exe has no successful --version invocation. Read its file version;
            // the actual compiler is still executed by the normal C++ build below.
            if(tool.Item1=="cpp"&&Path.GetFileNameWithoutExtension(cpp).Equals("cl",StringComparison.OrdinalIgnoreCase))
            {
                string compilerPath=File.Exists(cpp)?cpp:(Environment.GetEnvironmentVariable("PATH")??"").Split(Path.PathSeparator).Select(p=>Path.Combine(p,"cl.exe")).First(File.Exists);
                builds.Add(new{language="cpp",toolchain=FileVersionInfo.GetVersionInfo(compilerPath).FileVersion,toolchain_source="compiler-file-version"});
            }
            else{var version=await Execute(tool.Item2,[tool.Item3],root);builds.Add(new{language=tool.Item1,toolchain=version.Output.Trim()});}
        }
        string? nativeTypeScript=string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ZRCONFIG_TYPESCRIPT"))?Environment.GetEnvironmentVariable("ZRCONFIG_TSC"):null;
        if(string.IsNullOrEmpty(nativeTypeScript))nativeTypeScript=null;
        var tsVersion=nativeTypeScript!=null?await Execute(nativeTypeScript,["--version"],root):await Execute(node,["-e","process.stdout.write(require(process.argv[1]).version)",Tool("ZRCONFIG_TYPESCRIPT","typescript")],root);builds.Add(new{language="typescript",toolchain=tsVersion.Output});
        async Task Build(string language,string exe,string[] args,string work){Console.WriteLine("benchmark build "+language+": started");var r=await Execute(exe,args,work);builds.Add(new{language,milliseconds=r.Milliseconds,peak_working_set_bytes=r.Peak});Console.WriteLine($"benchmark build {language}: {r.Milliseconds:F0} ms");}
        string javaSource=Path.Combine(root,"BenchmarkMain.java"),classes=Path.Combine(root,"classes");File.WriteAllText(javaSource,Consumer(Java,"java"));Directory.CreateDirectory(classes);
        // Large catalogs exceed the OS command-line byte limit before javac runs.
        // javac argument files preserve spaces and non-ASCII paths without a shell.
        string javaArguments=Path.Combine(root,"java-sources.args");
        string JavaArgument(string path)=>"\""+path.Replace("\\","\\\\",StringComparison.Ordinal).Replace("\"","\\\"",StringComparison.Ordinal)+"\"";
        File.WriteAllLines(javaArguments,Directory.GetFiles(Path.Combine(generated,"Java"),"*.java").Append(javaSource).Select(JavaArgument),new System.Text.UTF8Encoding(false));
        await Build("java",java,["--release","17","-encoding","UTF-8","-d",classes,"@"+javaArguments],root);
        drivers.Add(new("java",Path.Combine(Path.GetDirectoryName(java)??"",OperatingSystem.IsWindows()?"java.exe":"java"),["-cp",classes,"BenchmarkMain"],root));
        string goRoot=Path.Combine(generated,"Go");File.WriteAllText(Path.Combine(goRoot,"benchmark_test.go"),Consumer(Go,"go"));string goBinary=Path.Combine(root,"go-benchmark"+(OperatingSystem.IsWindows()?".exe":""));
        // Use a reproducible consumer module, as a business Go project does. In
        // GOPATH-less legacy mode Go embeds the random absolute temporary path
        // into every generic symbol, making compiler size depend on the host.
        if(!File.Exists(Path.Combine(goRoot,"go.mod")))File.WriteAllText(Path.Combine(goRoot,"go.mod"),"module zrconfig\n\ngo 1.19\n");
        builds.Add(new{language="go",consumer_module=File.ReadAllText(Path.Combine(goRoot,"go.mod"))});
        await Build("go",go,["test","-c","-o",goBinary,"."],goRoot);drivers.Add(new("go",goBinary,["-test.run=TestBenchmarkProcess","-test.v","--"],goRoot));
        string cppSource=Path.Combine(root,"benchmark.cpp"),cppBinary=Path.Combine(root,"cpp-benchmark"+(OperatingSystem.IsWindows()?".exe":""));File.WriteAllText(cppSource,Consumer(Cpp,"cpp"));
        await Build("cpp",cpp,PortableExportTests.CppArguments(cpp,["-std=c++17","-O2","-pthread","-I",Path.Combine(generated,"Cpp"),cppSource,"-o",cppBinary]),root);drivers.Add(new("cpp",cppBinary,[],root));
        string rustLibrary=Path.Combine(root,"libzrconfig.rlib"),rustSource=Path.Combine(root,"benchmark.rs"),rustBinary=Path.Combine(root,"rust-benchmark"+(OperatingSystem.IsWindows()?".exe":""));File.WriteAllText(rustSource,Consumer(Rust,"rust"));
        await Build("rust-library",rust,["--edition","2021","-O","--crate-name","zrconfig","--crate-type","rlib",Path.Combine(generated,"Rust","lib.rs"),"-o",rustLibrary],root);
        await Build("rust-consumer",rust,["--edition","2021","-O","--extern","zrconfig="+rustLibrary,rustSource,"-o",rustBinary],root);drivers.Add(new("rust",rustBinary,[],root));
        string tsRoot=Path.Combine(generated,"TypeScript"),compiler=Path.Combine(root,"compile.cjs");File.WriteAllText(compiler,TypeScriptCompiler);
        int? compilerHeap=TypeScriptCompilerHeap();
        if(nativeTypeScript!=null&&compilerHeap.HasValue)throw new ArgumentException("ZRCONFIG_TYPESCRIPT_HEAP_MB applies only to the JavaScript TypeScript compiler.");
        string[] compilerOptions=compilerHeap.HasValue?["--max-old-space-size="+compilerHeap.Value.ToString(CultureInfo.InvariantCulture)]:[];
        builds.Add(new{language="typescript",compiler_heap_megabytes=compilerHeap,compiler_options=compilerOptions});
        if(nativeTypeScript!=null)await Build("typescript",nativeTypeScript,["-p",tsRoot],root);
        else await Build("typescript",node,[..compilerOptions,compiler,Tool("ZRCONFIG_TYPESCRIPT","typescript"),tsRoot],root);
        string js=Path.Combine(root,"benchmark.cjs");File.WriteAllText(js,Consumer(TypeScript,"typescript"));drivers.Add(new("typescript",node,["--expose-gc",js,tsRoot],root));
        string luaSource=Path.Combine(root,"benchmark.lua");File.WriteAllText(luaSource,Consumer(Lua,"lua"));drivers.Add(new("lua",lua,[luaSource,Path.Combine(generated,"Lua")],root));
        long Size(string path)=>Directory.Exists(path)?Directory.GetFiles(path,"*",SearchOption.AllDirectories).Sum(f=>new FileInfo(f).Length):new FileInfo(path).Length;
        builds.Add(new{language="java",artifact_bytes=Size(classes)});builds.Add(new{language="go",artifact_bytes=Size(goBinary)});
        builds.Add(new{language="cpp",artifact_bytes=Size(cppBinary)});builds.Add(new{language="rust-library",artifact_bytes=Size(rustLibrary)});
        builds.Add(new{language="rust-consumer",artifact_bytes=Size(rustBinary)});builds.Add(new{language="typescript",artifact_bytes=Size(Path.Combine(tsRoot,"dist"))});
        File.WriteAllText(Path.Combine(code,"BenchmarkConsumer.cs"),Consumer(CSharp,"csharp"));
        if(profileRuntime!=null)
        {
            string project=Path.Combine(root,"profile-consumer");Directory.CreateDirectory(project);
            string Xml(string value)=>System.Security.SecurityElement.Escape(value)!;
            File.WriteAllText(Path.Combine(project,"Consumer.csproj"),"<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><Nullable>enable</Nullable></PropertyGroup><ItemGroup><Compile Include=\"Main.cs\" /><Compile Include=\""+Xml(profileRuntime)+"/*.cs\" /><Compile Include=\""+Xml(code)+"/**/*.cs\" Exclude=\""+Xml(code)+"/obj/**;"+Xml(code)+"/bin/**\" /></ItemGroup></Project>");
            File.WriteAllText(Path.Combine(project,"Main.cs"),"BenchmarkConsumer.Run(args);");
            await Build("csharp-instrumented",Environment.ProcessPath!,["build",Path.Combine(project,"Consumer.csproj"),"-c","Release","--disable-build-servers","-m:1","-p:NuGetAudit=false","-v","quiet"],root);
            drivers.Add(new("csharp",Environment.ProcessPath!,[Path.Combine(project,"bin","Release","net10.0","Consumer.dll")],root));
        }
        else
        {
            string worker=Path.Combine(root,"worker");CopyTree(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,worker,_=>true);
            drivers.Add(new("csharp",Environment.ProcessPath!,[Path.Combine(worker,Path.GetFileName(Assembly.GetExecutingAssembly().Location)),"--benchmark-worker",code],root));
        }
        return drivers;
    }
    private static int ProcessTimeout()
    {
        string? configured=Environment.GetEnvironmentVariable("ZRCONFIG_BENCH_TIMEOUT_SECONDS");
        if(configured==null)return 300;
        if(!int.TryParse(configured,NumberStyles.None,CultureInfo.InvariantCulture,out int seconds)||seconds is <1 or >3600)
            throw new ArgumentException("ZRCONFIG_BENCH_TIMEOUT_SECONDS must be between 1 and 3600.");
        return seconds;
    }
    internal static async Task<(string Output,double Milliseconds,long? Peak)> Execute(string file,string[] arguments,string directory)
    {
        int timeoutSeconds=ProcessTimeout();
        bool externalTime=Environment.GetEnvironmentVariable("ZRCONFIG_BENCH_TIME")=="1"&&File.Exists("/usr/bin/time")&&(OperatingSystem.IsMacOS()||OperatingSystem.IsLinux());
        var start=new ProcessStartInfo(externalTime?"/usr/bin/time":file){WorkingDirectory=directory,RedirectStandardOutput=true,RedirectStandardError=true,UseShellExecute=false};
        if(externalTime){start.ArgumentList.Add(OperatingSystem.IsMacOS()?"-l":"-v");start.ArgumentList.Add(file);}foreach(string arg in arguments)start.ArgumentList.Add(arg);
        start.Environment["GO111MODULE"]=File.Exists(Path.Combine(directory,"go.mod"))?"on":"off";start.Environment["GOCACHE"]=Path.Combine(Path.GetTempPath(),"zrconfig-benchmark-go-cache");
        using var process=Process.Start(start)??throw new Exception("Cannot start "+file);var watch=Stopwatch.StartNew();var stdout=process.StandardOutput.ReadToEndAsync();var stderr=process.StandardError.ReadToEndAsync();
        try{await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(timeoutSeconds));}catch{process.Kill(true);throw;}
        string output=await stdout,error=await stderr;if(process.ExitCode!=0)throw new Exception(file+" failed:\n"+output+error);
        long? peak=null;
        if(externalTime){var match=System.Text.RegularExpressions.Regex.Match(error,OperatingSystem.IsMacOS()?@"(?m)^\s*(\d+)\s+maximum resident set size":@"Maximum resident set size \(kbytes\):\s*(\d+)");if(match.Success)peak=long.Parse(match.Groups[1].Value,CultureInfo.InvariantCulture)*(OperatingSystem.IsLinux()?1024L:1L);}
        else try{long size=process.PeakWorkingSet64;if(size>0)peak=size;}catch(InvalidOperationException){}
        return(output,watch.Elapsed.TotalMilliseconds,peak);
    }
    internal const string TypeScriptCompiler="""
const ts=require(process.argv[2]),path=require('path'),root=process.argv[3];const config=ts.readConfigFile(path.join(root,'tsconfig.json'),ts.sys.readFile);const parsed=ts.parseJsonConfigFileContent(config.config,ts.sys,root);parsed.options.types=[];const p=ts.createProgram(parsed.fileNames,parsed.options);const emitted=p.emit();const errors=ts.getPreEmitDiagnostics(p).concat(emitted.diagnostics);if(errors.length){console.error(ts.formatDiagnosticsWithColorAndContext(errors,{getCurrentDirectory:()=>root,getCanonicalFileName:x=>x,getNewLine:()=> '\n'}));process.exit(1)}
""";
    private const string CSharp="""
using System;using System.Diagnostics;using System.Text.Json;using ZrConfig.Runtime;using ZrConfig.Runtime.Server;using ZrConfig.Models.Server;
public static class BenchmarkConsumer {
 public static void Run(string[] a){int n=int.Parse(a[4]),count=n*int.Parse(a[3])/100;var mode=a[2]=="full"?LoadMode.ServerDefault:a[2]=="selected"?LoadMode.SelectedTables:a[2]=="strict"?LoadMode.LazyRowsStrict:LoadMode.LazyRowsOnAccess;ConfigManifest Decode(string text)=>JsonSerializer.Deserialize<ConfigManifest>(text,new JsonSerializerOptions{IncludeFields=true})!;
 using var manager=new Manager(ZrConfig.Models.ServerCatalog.Instance,Decode);var options=new PreparationOptions(preferJson:a[1]=="json",mode:mode,selectedTables:a[2]=="full"?null:new[]{"Bench"});var retained=new System.Collections.Generic.List<Snapshot>();for(int j=0;j<int.Parse(a[5]);j++)retained.Add(manager.Reload(new FileConfigSource(a[0]),options).Snapshot);var watch=Stopwatch.StartNew();var snapshot=manager.Reload(new FileConfigSource(a[0]),options).Snapshot;double cold=watch.Elapsed.TotalMilliseconds;var table=snapshot.GetBenchTable();watch.Restart();var first=table.Load(1);double firstMs=watch.Elapsed.TotalMilliseconds;watch.Restart();long sum=0;for(int i=0;i<count;i++)sum+=table.Load((i*7919%count)+1).Id;double access=watch.Elapsed.TotalMilliseconds;long sparse=snapshot.LoadedRowCount;watch.Restart();sum+=table.LoadByGroup(0).Count;double group=watch.Elapsed.TotalMilliseconds;
 for(int i=0;i<20000;i++)sum+=table.Get(i%count+1).Id;var samples=new double[1000];long before=GC.GetAllocatedBytesForCurrentThread();for(int b=0;b<samples.Length;b++){long stamp=Stopwatch.GetTimestamp();for(int i=0;i<100;i++)sum+=table.Get((b*100+i)%count+1).Id;samples[b]=(Stopwatch.GetTimestamp()-stamp)*1e9/Stopwatch.Frequency/100;}long allocated=GC.GetAllocatedBytesForCurrentThread()-before;Array.Sort(samples);Console.WriteLine(JsonSerializer.Serialize(new{cold_ms=cold,first_row_ms=firstMs,access_ms=access,group_ms=group,sparse_loaded_rows=sparse,hot_p50_ns=samples[500],hot_p95_ns=samples[950],hot_p99_ns=samples[990],hot_alloc_bytes=allocated,heap_bytes=GC.GetTotalMemory(false),gen0=GC.CollectionCount(0),gen1=GC.CollectionCount(1),gen2=GC.CollectionCount(2),checksum=sum}));GC.KeepAlive(retained);
 }
}
""";
    private const string Java="""
import zrconfig.*;import static zrconfig.ConfigRuntime.*;import java.nio.file.*;import java.util.*;import java.lang.management.*;
public class BenchmarkMain{
 static double ms(long t){return(System.nanoTime()-t)/1e6;}static volatile long sink;
 public static void main(String[] a)throws Exception{int n=Integer.parseInt(a[4]),count=n*Integer.parseInt(a[3])/100;var mode=a[2].equals("full")?LoadMode.ServerDefault:a[2].equals("selected")?LoadMode.SelectedTables:a[2].equals("strict")?LoadMode.LazyRowsStrict:LoadMode.LazyRowsOnAccess;var token=new Cancellation();var retained=new ArrayList<DataSnapshot>();for(int j=0;j<Integer.parseInt(a[5]);j++)retained.add(DataSnapshot.load(Path.of(a[0]),new LoadPlan(mode,a[2].equals("full")?null:List.of("Bench"),a[1].equals("json"),List.of()),token));long t=System.nanoTime();var s=DataSnapshot.load(Path.of(a[0]),new LoadPlan(mode,a[2].equals("full")?null:List.of("Bench"),a[1].equals("json"),List.of()),token);double cold=ms(t);var table=s.get_Bench();t=System.nanoTime();table.load(1,token);double first=ms(t);t=System.nanoTime();long sum=0;for(int i=0;i<count;i++)sum+=table.load(i*7919%count+1,token).getId();double access=ms(t);int sparse=s.loadedRowCount();t=System.nanoTime();sum+=table.loadGroup_ByGroup(0,token).size();double group=ms(t);for(int i=0;i<20000;i++)sum+=table.get(i%count+1).getId();double[] samples=new double[1000];var bean=(com.sun.management.ThreadMXBean)ManagementFactory.getThreadMXBean();if(!bean.isThreadAllocatedMemorySupported())throw new UnsupportedOperationException("Thread allocation counter unavailable");if(!bean.isThreadAllocatedMemoryEnabled())bean.setThreadAllocatedMemoryEnabled(true);long thread=Thread.currentThread().getId(),before=bean.getThreadAllocatedBytes(thread);for(int b=0;b<1000;b++){t=System.nanoTime();for(int i=0;i<100;i++)sum+=table.get((b*100+i)%count+1).getId();samples[b]=(System.nanoTime()-t)/100.0;}long allocated=bean.getThreadAllocatedBytes(thread)-before;Arrays.sort(samples);sink=sum;System.out.printf(Locale.ROOT,"{\"cold_ms\":%.6f,\"first_row_ms\":%.6f,\"access_ms\":%.6f,\"group_ms\":%.6f,\"sparse_loaded_rows\":%d,\"hot_p50_ns\":%.6f,\"hot_p95_ns\":%.6f,\"hot_p99_ns\":%.6f,\"hot_alloc_bytes\":%d,\"heap_bytes\":%d,\"checksum\":%d}%n",cold,first,access,group,sparse,samples[500],samples[950],samples[990],allocated,Runtime.getRuntime().totalMemory()-Runtime.getRuntime().freeMemory(),sum);java.lang.ref.Reference.reachabilityFence(retained);}
}
""";
    private const string Go="""
package zrconfig
import("context";"encoding/json";"fmt";"os";"runtime";"sort";"strconv";"testing";"time")
func TestBenchmarkProcess(t *testing.T){args:=os.Args;for i,a:=range args{if a=="--"{args=args[i+1:];break}};n,_:=strconv.Atoi(args[4]);percent,_:=strconv.Atoi(args[3]);count:=n*percent/100;ctx:=context.Background();mode:=LazyRowsOnAccess;if args[2]=="full"{mode=ServerDefault}else if args[2]=="selected"{mode=SelectedTables}else if args[2]=="strict"{mode=LazyRowsStrict};var names []string;if args[2]!="full"{names=[]string{"Bench"}};held,_:=strconv.Atoi(args[5]);retained:=make([]*DataSnapshot,0,held);for j:=0;j<held;j++{snapshot,e:=LoadData(ctx,args[0],LoadPlan{Mode:mode,Tables:names,PreferJSON:args[1]=="json"});if e!=nil{t.Fatal(e)};retained=append(retained,snapshot)};start:=time.Now();s,e:=LoadData(ctx,args[0],LoadPlan{Mode:mode,Tables:names,PreferJSON:args[1]=="json"});if e!=nil{t.Fatal(e)};cold:=float64(time.Since(start).Nanoseconds())/1e6;table,e:=s.Get_Bench();if e!=nil{t.Fatal(e)};start=time.Now();_,e=table.Load(ctx,1);if e!=nil{t.Fatal(e)};first:=float64(time.Since(start).Nanoseconds())/1e6;start=time.Now();sum:=int64(0);for i:=0;i<count;i++{v,e:=table.Load(ctx,int32(i*7919%count+1));if e!=nil{t.Fatal(e)};sum+=int64(v.Id())};access:=float64(time.Since(start).Nanoseconds())/1e6;sparse:=s.LoadedRowCount();start=time.Now();g,e:=table.LoadGroup_ByGroup(ctx,0);if e!=nil{t.Fatal(e)};sum+=int64(g.Len());group:=float64(time.Since(start).Nanoseconds())/1e6;for i:=0;i<20000;i++{v,e:=table.Get(int32(i%count+1));if e!=nil{t.Fatal(e)};sum+=int64(v.Id())};samples:=make([]float64,1000);var before,after runtime.MemStats;runtime.ReadMemStats(&before);for b:=range samples{start=time.Now();for i:=0;i<100;i++{v,e:=table.Get(int32((b*100+i)%count+1));if e!=nil{t.Fatal(e)};sum+=int64(v.Id())};samples[b]=float64(time.Since(start).Nanoseconds())/100};runtime.ReadMemStats(&after);sort.Float64s(samples);bytes,_:=json.Marshal(map[string]interface{}{"cold_ms":cold,"first_row_ms":first,"access_ms":access,"group_ms":group,"sparse_loaded_rows":sparse,"hot_p50_ns":samples[500],"hot_p95_ns":samples[950],"hot_p99_ns":samples[990],"hot_alloc_bytes":after.TotalAlloc-before.TotalAlloc,"hot_allocs":after.Mallocs-before.Mallocs,"heap_bytes":after.HeapAlloc,"gc_count":after.NumGC,"gc_pause_ns":after.PauseTotalNs,"checksum":sum});fmt.Println(string(bytes));runtime.KeepAlive(s);runtime.KeepAlive(retained)}
""";
    private const string Cpp="""
#include "data.hpp"
#include <iostream>
#include <chrono>
#include <cstdlib>
#include <new>
static std::atomic<std::size_t> allocations{0};
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
using namespace zrconfig;using Clock=std::chrono::steady_clock;double ms(Clock::time_point t){return std::chrono::duration<double,std::milli>(Clock::now()-t).count();}volatile std::int64_t sink;
int main(int,char** a){int n=std::stoi(a[5]),count=n*std::stoi(a[4])/100;std::string mode=a[3];LoadPlan plan;plan.prefer_json=std::string(a[2])=="json";plan.mode=mode=="full"?LoadMode::ServerDefault:mode=="selected"?LoadMode::SelectedTables:mode=="strict"?LoadMode::LazyRowsStrict:LoadMode::LazyRowsOnAccess;if(mode!="full")plan.tables=std::vector<std::string>{"Bench"};Cancellation token;std::vector<std::shared_ptr<const data::Snapshot>> retained;for(int j=0;j<std::stoi(a[6]);++j)retained.push_back(data::Snapshot::load(a[1],plan,token));auto t=Clock::now();auto s=data::Snapshot::load(a[1],plan,token);double cold=ms(t);t=Clock::now();s->load_Bench(1,token);double first=ms(t);t=Clock::now();std::int64_t sum=0;for(int i=0;i<count;i++)sum+=s->load_Bench(i*7919%count+1,token).Id();double access=ms(t);auto sparse=s->loaded_row_count();t=Clock::now();sum+=s->load_group_Bench_ByGroup(0,token).size();double group=ms(t);for(int i=0;i<20000;i++)sum+=s->get_Bench(i%count+1).Id();std::vector<double> samples(1000);auto before=allocations.load();for(int b=0;b<1000;b++){t=Clock::now();for(int i=0;i<100;i++)sum+=s->get_Bench((b*100+i)%count+1).Id();samples[b]=ms(t)*1e6/100;}auto hot_allocs=allocations.load()-before;std::sort(samples.begin(),samples.end());auto diagnostic=s->diagnostics();t=Clock::now();s.reset();retained.clear();double release=ms(t);sink=sum;std::cout<<"{\"cold_ms\":"<<cold<<",\"first_row_ms\":"<<first<<",\"access_ms\":"<<access<<",\"group_ms\":"<<group<<",\"sparse_loaded_rows\":"<<sparse<<",\"hot_p50_ns\":"<<samples[500]<<",\"hot_p95_ns\":"<<samples[950]<<",\"hot_p99_ns\":"<<samples[990]<<",\"release_ms\":"<<release<<",\"retained_raw_bytes\":"<<diagnostic.retained_raw_bytes<<",\"hot_allocs\":"<<hot_allocs<<",\"checksum\":"<<sum<<"}\n";}
""";
    private const string Rust="""
extern crate zrconfig;
use std::sync::atomic::{AtomicUsize,Ordering};use std::alloc::{GlobalAlloc,Layout,System};
static ALLOCATIONS:AtomicUsize=AtomicUsize::new(0);struct Counter;
unsafe impl GlobalAlloc for Counter{unsafe fn alloc(&self,l:Layout)->*mut u8{ALLOCATIONS.fetch_add(1,Ordering::Relaxed);unsafe{System.alloc(l)}}unsafe fn alloc_zeroed(&self,l:Layout)->*mut u8{ALLOCATIONS.fetch_add(1,Ordering::Relaxed);unsafe{System.alloc_zeroed(l)}}unsafe fn realloc(&self,p:*mut u8,l:Layout,n:usize)->*mut u8{ALLOCATIONS.fetch_add(1,Ordering::Relaxed);unsafe{System.realloc(p,l,n)}}unsafe fn dealloc(&self,p:*mut u8,l:Layout){unsafe{System.dealloc(p,l)}}}
#[global_allocator]static ALLOCATOR:Counter=Counter;
use zrconfig::runtime::*;use std::time::Instant;fn ms(t:Instant)->f64{t.elapsed().as_secs_f64()*1000.0}
fn main(){let a:Vec<String>=std::env::args().collect();let n:usize=a[5].parse().unwrap();let count=n*a[4].parse::<usize>().unwrap()/100;let token=Cancellation::default();let plan=LoadPlan{mode:match a[3].as_str(){"full"=>LoadMode::ServerDefault,"selected"=>LoadMode::SelectedTables,"strict"=>LoadMode::LazyRowsStrict,_=>LoadMode::LazyRowsOnAccess},tables:if a[3]=="full"{None}else{Some(vec!["Bench".into()])},prefer_json:a[2]=="json",..LoadPlan::default()};let mut retained=Vec::new();for _ in 0..a[6].parse::<usize>().unwrap(){retained.push(zrconfig::data::Snapshot::load(std::path::Path::new(&a[1]),&plan,&token).unwrap());}let mut t=Instant::now();let s=zrconfig::data::Snapshot::load(std::path::Path::new(&a[1]),&plan,&token).unwrap();let cold=ms(t);t=Instant::now();s.load_Bench(1,&token).unwrap();let first=ms(t);t=Instant::now();let mut sum=0i64;for i in 0..count{sum+=s.load_Bench((i*7919%count+1)as i32,&token).unwrap().get_Id()as i64;}let access=ms(t);let sparse=s.loaded_row_count();t=Instant::now();sum+=s.load_group_Bench_ByGroup(0,&token).unwrap().len()as i64;let group=ms(t);for i in 0..20000{sum+=s.get_Bench((i%count+1)as i32).unwrap().get_Id()as i64;}let mut samples=vec![0.0f64;1000];let before=ALLOCATIONS.load(Ordering::Relaxed);for b in 0..1000{t=Instant::now();for i in 0..100{sum+=s.get_Bench(((b*100+i)%count+1)as i32).unwrap().get_Id()as i64;}samples[b]=ms(t)*1e6/100.0;}let hot_allocs=ALLOCATIONS.load(Ordering::Relaxed)-before;samples.sort_by(|a,b|a.partial_cmp(b).unwrap());let diagnostics=s.diagnostics().unwrap();t=Instant::now();drop(s);drop(retained);let release=ms(t);std::hint::black_box(sum);println!("{{\"cold_ms\":{},\"first_row_ms\":{},\"access_ms\":{},\"group_ms\":{},\"sparse_loaded_rows\":{},\"hot_p50_ns\":{},\"hot_p95_ns\":{},\"hot_p99_ns\":{},\"release_ms\":{},\"retained_raw_bytes\":{},\"hot_allocs\":{},\"checksum\":{}}}",cold,first,access,group,sparse,samples[500],samples[950],samples[990],release,diagnostics.retained_raw_bytes,hot_allocs,sum);}
""";
    private const string TypeScript="""
const path=require('path'),{performance}=require('perf_hooks'),D=require(path.join(process.argv[2],'dist/data.js'));const a=process.argv.slice(3),n=Number(a[4]),count=n*Number(a[3])/100;const retained=[];for(let j=0;j<Number(a[5]);j++)retained.push(D.Snapshot.load(a[0],{mode:a[2]==='full'?'ServerDefault':a[2]==='selected'?'SelectedTables':a[2]==='strict'?'LazyRowsStrict':'LazyRowsOnAccess',tables:a[2]==='full'?undefined:['Bench'],preferJSON:a[1]==='json'}));global.__zrBenchmarkHeld=retained;const loopStart=performance.now();let start=performance.now();const s=D.Snapshot.load(a[0],{mode:a[2]==='full'?'ServerDefault':a[2]==='selected'?'SelectedTables':a[2]==='strict'?'LazyRowsStrict':'LazyRowsOnAccess',tables:a[2]==='full'?undefined:['Bench'],preferJSON:a[1]==='json'}),cold=performance.now()-start,table=s.get_Bench();start=performance.now();table.load(1);const first=performance.now()-start;start=performance.now();let sum=0;for(let i=0;i<count;i++)sum+=table.load(i*7919%count+1).Id;const access=performance.now()-start,sparse=s.loadedRowCount;start=performance.now();sum+=table.loadGroup_ByGroup(0).length;const group=performance.now()-start;for(let i=0;i<20000;i++)sum+=table.get(i%count+1).Id;const samples=new Float64Array(1000),before=process.memoryUsage().heapUsed;for(let b=0;b<1000;b++){start=performance.now();for(let i=0;i<100;i++)sum+=table.get((b*100+i)%count+1).Id;samples[b]=(performance.now()-start)*1e6/100;}samples.sort();const after=process.memoryUsage().heapUsed;setImmediate(()=>console.log(JSON.stringify({event_loop_delay_ms:performance.now()-loopStart,cold_ms:cold,first_row_ms:first,access_ms:access,group_ms:group,sparse_loaded_rows:sparse,hot_p50_ns:samples[500],hot_p95_ns:samples[950],hot_p99_ns:samples[990],hot_heap_delta_bytes:after-before,heap_bytes:after,checksum:sum})));
""";
    private const string Lua="""
package.path=arg[1]..'/?.lua;'..package.path;local D=require('data');local root,format,mode,percent,n=arg[2],arg[3],arg[4],tonumber(arg[5]),tonumber(arg[6]);local count=n*percent//100;local held=tonumber(arg[7]);local retained={};for j=1,held do retained[j]=D.load(root,{mode=mode=='full'and'ServerDefault'or mode=='selected'and'SelectedTables'or mode=='strict'and'LazyRowsStrict'or'LazyRowsOnAccess',tables=mode~='full'and{'Bench'}or nil,prefer_json=format=='json'}) end;local start=os.clock();local s=D.load(root,{mode=mode=='full'and'ServerDefault'or mode=='selected'and'SelectedTables'or mode=='strict'and'LazyRowsStrict'or'LazyRowsOnAccess',tables=mode~='full'and{'Bench'}or nil,prefer_json=format=='json'});local cold=(os.clock()-start)*1000;local view=s.get_Bench();start=os.clock();view.load(1);local first=(os.clock()-start)*1000;start=os.clock();local sum=0;for i=0,count-1 do sum=sum+view.load(i*7919%count+1).Id end;local access=(os.clock()-start)*1000;local sparse=s.loaded_row_count();start=os.clock();sum=sum+#view.load_group_ByGroup(0);local group=(os.clock()-start)*1000;for i=0,19999 do sum=sum+view.get(i%count+1).Id end;local samples={};for i=1,1000 do samples[i]=0 end;local before=collectgarbage('count')*1024;for b=0,999 do start=os.clock();for i=0,99 do sum=sum+view.get((b*100+i)%count+1).Id end;samples[b+1]=(os.clock()-start)*1e9/100 end;local after=collectgarbage('count')*1024;table.sort(samples);print(string.format('{"cold_ms":%.6f,"first_row_ms":%.6f,"access_ms":%.6f,"group_ms":%.6f,"sparse_loaded_rows":%d,"hot_p50_ns":%.6f,"hot_p95_ns":%.6f,"hot_p99_ns":%.6f,"hot_heap_delta_bytes":%.0f,"heap_bytes":%.0f,"checksum":%d}',cold,first,access,group,sparse,samples[501],samples[951],samples[991],after-before,after,sum));assert(#retained==held)
""";
}
