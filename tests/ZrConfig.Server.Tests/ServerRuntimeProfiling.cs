using System.Text;

// Test-only instrumentation of copied production sources. No generated file in the
// repository is edited. It measures inclusive calls, not disjoint CPU accounting.
internal static class ServerRuntimeProfiling
{
    private static string Wrap(string source,string marker,string enter,string leave="")
    {
        int at=source.IndexOf(marker,StringComparison.Ordinal);
        if(at<0||source.IndexOf(marker,at+marker.Length,StringComparison.Ordinal)>=0)
            throw new InvalidOperationException("Profiling marker must be unique: "+marker);
        int open=marker.EndsWith('{')?at+marker.Length-1:source.IndexOf('{',at+marker.Length);
        int close=CloseBrace(source,open);
        return source[..(open+1)]+"\n"+enter+source[(open+1)..close]+leave+"\n"+source[close..];
    }
    private static int CloseBrace(string text,int open)
    {
        int depth=0;
        for(int i=open;i<text.Length;i++)
        {
            char c=text[i];
            if(c=='/'&&i+1<text.Length&&text[i+1]=='/')
            {while(i<text.Length&&text[i]!='\n')i++;continue;}
            if(c=='/'&&i+1<text.Length&&text[i+1]=='*')
            {int end=text.IndexOf("*/",i+2,StringComparison.Ordinal);if(end<0)throw new Exception("Unterminated comment");i=end+1;continue;}
            bool quoted=c is '"' or '`'||c=='\''&&i+2<text.Length&&(text[i+2]=='\''||text[i+1]=='\\');
            if(quoted)
            {
                char quote=c;bool verbatim=c=='"'&&i>0&&text[i-1]=='@';
                for(i++;i<text.Length;i++)
                {
                    if(!verbatim&&text[i]=='\\'){i++;continue;}
                    if(text[i]!=quote)continue;
                    if(verbatim&&i+1<text.Length&&text[i+1]=='"'){i++;continue;}
                    break;
                }
                continue;
            }
            if(c=='{')depth++;
            else if(c=='}'&&--depth==0)return i;
        }
        throw new Exception("Unterminated profiling body");
    }
    private static string Replace(string source,string before,string after)
    {
        if(!source.Contains(before,StringComparison.Ordinal))throw new Exception("Missing profiling expression: "+before);
        return source.Replace(before,after,StringComparison.Ordinal);
    }
    private static string Cpp(string source)
    {
        source=Replace(source,"namespace zrconfig {","namespace zrconfig {\n"+CppMeter);
        source="#include <iostream>\n#include <chrono>\n#include <array>\n"+source;
        foreach(var item in new[]{
            ("inline std::string sha256(const std::vector<std::uint8_t> &bytes) {",1),
            ("SourceBundle(ConfigSource& source,const LoadSettings& settings,const Cancellation& cancellation):source_(source),cancellation_(cancellation),budget_(std::make_shared<LoadBudget>(settings.limits())){",2),
            ("SelectionPlan select(const std::vector<std::string>& requested) const {",8),
            ("IndexedRows(const std::string& schema,std::vector<std::uint8_t> bytes,bool json,std::shared_ptr<LoadBudget> budget):wire_(schema),json_bounds_(wire_.schema_,budget->limits()),json_(json),budget_(std::move(budget)) {",3),
            ("Json read_value(std::size_t index){",4),
            ("template<class H,class E>OrdinalIndex(std::vector<Key> keys,bool unique,H hash,E equal):keys_(std::move(keys)){",5),
            ("template<class Resolve>void validate_references(const Json& row,Resolve resolve)const{",6),
            ("CommitResult commit(Prepared& prepared,const Cancellation& cancellation){",7)})
            source=Wrap(source,item.Item1,"ZrProfileScope zr_profile_scope("+item.Item2+");");
        int split=source.IndexOf("class FileConfigSource final:",StringComparison.Ordinal);
        if(split<0)throw new Exception("Missing file source");
        source=source[..split]+Wrap(source[split..],"std::vector<std::uint8_t> read(const std::string& name,std::uint64_t maximum,const Cancellation& cancellation)override{","ZrProfileScope zr_profile_scope(0);");
        return source;
    }
    private const string CppMeter="""
inline std::array<std::atomic<std::uint64_t>,9> zr_profile_ns{};
inline std::array<std::atomic<std::uint64_t>,9> zr_profile_calls{};
struct ZrProfileScope {
    std::size_t phase;std::chrono::steady_clock::time_point start;
    explicit ZrProfileScope(std::size_t value):phase(value),start(std::chrono::steady_clock::now()){}
    ~ZrProfileScope(){auto elapsed=std::chrono::duration_cast<std::chrono::nanoseconds>(std::chrono::steady_clock::now()-start).count();zr_profile_ns[phase].fetch_add(static_cast<std::uint64_t>(elapsed),std::memory_order_relaxed);zr_profile_calls[phase].fetch_add(1,std::memory_order_relaxed);}
};
inline void zr_profile_report(){const char* names[]={"read","hash","manifest","scan","decode","index","reference","commit","plan"};std::cout<<"{\"scenario\":\"stages\",\"clock\":\"elapsed\",\"stages\":[";for(std::size_t i=0;i<9;++i){if(i)std::cout<<",";std::cout<<"{\"stage\":\""<<names[i]<<"\",\"calls\":"<<zr_profile_calls[i].load()<<",\"inclusive_ns\":"<<zr_profile_ns[i].load()<<"}";}std::cout<<"]}\n";}
""";
    private static string Rust(string source)
    {
        source=Replace(source,"#![allow(dead_code)]","#![allow(dead_code)]\n"+RustMeter);
        foreach(var item in new[]{
            ("impl ConfigSource for FileConfigSource{fn read(&self,name:&str,maximum:u64,cancellation:&Cancellation)->ServerResult<Vec<u8>>{",0),
            ("pub fn sha256(bytes: &[u8]) -> String {",1),
            ("pub(super) fn open(source:&'a dyn ConfigSource,settings:&LoadSettings,cancellation:&'a Cancellation)->ServerResult<Self>{",2),
            ("pub fn select(&self, requested: &[&str]) -> ServerResult<SelectionPlan> {",8),
            ("fn new(mut wire:Wire,mut bytes:Vec<u8>,json:bool,budget:Arc<LoadBudget>)->Result<Self>{",3),
            ("pub(super) fn read(&self,index:usize)->ServerResult<Json>{",4),
            ("pub fn build(\n        n: usize,",5),
            ("pub(super) fn validate_references(&self,row:&Json,resolve:impl Fn(&str,i32)->ServerResult<()>)->ServerResult<()>{",6),
            ("pub fn commit(&self,prepared:&mut PreparedSnapshot<S>,cancellation:&Cancellation)->ServerResult<CommitResult<S>>{",7)})
            source=Wrap(source,item.Item1,"let _zr_profile_scope=ZrProfileScope::new("+item.Item2+");");
        return source;
    }
    private const string RustMeter="""
static ZR_PROFILE_NS:[std::sync::atomic::AtomicU64;9]=[std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0)];
static ZR_PROFILE_CALLS:[std::sync::atomic::AtomicU64;9]=[std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0),std::sync::atomic::AtomicU64::new(0)];
struct ZrProfileScope{phase:usize,start:std::time::Instant}
impl ZrProfileScope{fn new(phase:usize)->Self{Self{phase,start:std::time::Instant::now()}}}
impl Drop for ZrProfileScope{fn drop(&mut self){ZR_PROFILE_NS[self.phase].fetch_add(self.start.elapsed().as_nanos()as u64,std::sync::atomic::Ordering::Relaxed);ZR_PROFILE_CALLS[self.phase].fetch_add(1,std::sync::atomic::Ordering::Relaxed);}}
pub fn zr_profile_report(){let names=["read","hash","manifest","scan","decode","index","reference","commit","plan"];print!("{{\"scenario\":\"stages\",\"clock\":\"elapsed\",\"stages\":[");for i in 0..9{if i!=0{print!(",");}print!("{{\"stage\":\"{}\",\"calls\":{},\"inclusive_ns\":{}}}",names[i],ZR_PROFILE_CALLS[i].load(std::sync::atomic::Ordering::Relaxed),ZR_PROFILE_NS[i].load(std::sync::atomic::Ordering::Relaxed));}println!("]}}");}
""";
    private static string Java(string source)
    {
        source=Replace(source,"MessageDigest.getInstance(\"SHA-256\").digest(","ZrProfile.hash(");
        source=Replace(source,"public final class ConfigRuntime {","public final class ConfigRuntime {\n"+JavaMeter);
        foreach(var item in new[]{
            ("public byte[] read(String name,long maximum,Cancellation cancellation)throws IOException{",0),
            ("private Bundle(Path root,ConfigSource source,ResourceLimits limits,Cancellation cancellation,byte[] bytes){",2),
            ("IndexedRows(Object schema, byte[] input, boolean json,LoadBudget budget) {",3),
            ("Object read(int index) {",4),
            ("protected Index(List<R> rows, boolean unique) {",5),
            ("void validateReferences(Object row, java.util.function.Function<String,IndexedRows> resolve) {",6),
            ("public CommitResult<S> commit(PreparedSnapshot<S> prepared,Cancellation cancellation){",7),
            ("public SelectionPlan select(Collection<String> requested) {",8)})
            source=Wrap(source,item.Item1,"long zrProfileStart=System.nanoTime();try{","}finally{ZrProfile.done("+item.Item2+",zrProfileStart);}");
        return source;
    }
    private const string JavaMeter="""
public static final class ZrProfile {
 private static final java.util.concurrent.atomic.AtomicLongArray nanos=new java.util.concurrent.atomic.AtomicLongArray(9),calls=new java.util.concurrent.atomic.AtomicLongArray(9);
 static void done(int phase,long start){nanos.addAndGet(phase,System.nanoTime()-start);calls.incrementAndGet(phase);}
 static byte[] hash(byte[] input)throws NoSuchAlgorithmException{long start=System.nanoTime();try{return MessageDigest.getInstance("SHA-256").digest(input);}finally{done(1,start);}}
 public static void report(){String[] names={"read","hash","manifest","scan","decode","index","reference","commit","plan"};StringBuilder result=new StringBuilder("{\"scenario\":\"stages\",\"clock\":\"elapsed\",\"stages\":[");for(int i=0;i<9;i++){if(i!=0)result.append(',');result.append("{\"stage\":\"").append(names[i]).append("\",\"calls\":").append(calls.get(i)).append(",\"inclusive_ns\":").append(nanos.get(i)).append('}');}System.out.println(result.append("]}"));}
}
""";
    private static string Go(string source)
    {
        source=Replace(source,"sha256.Sum256(","zrProfileHash(")+"\n"+GoMeter;
        foreach(var item in new[]{
            ("func(s *FileConfigSource)Read(ctx context.Context,name string,maximum int64)([]byte,error){",0),
            ("func parseBundle(data []byte,root string,budget *loadBudget)(result *bundle){",2),
            ("func newIndexedRows(w *wireReader,data []byte,isJSON bool)*indexedRows{",3),
            ("func(r *indexedRows)read(index int)any{",4),
            ("func(r *indexedRows)validateReferences(row any,all map[string]*indexedRows)error{",6),
            ("func(m *ServerManager[S])Commit(ctx context.Context,p *PreparedSnapshot[S])(result CommitResult[S],err error){",7),
            ("func(c *Catalog)Select(requested []string)(SelectionPlan,error){",8)})
            source=Wrap(source,item.Item1,"defer zrProfileDone("+item.Item2+",time.Now());");
        return source;
    }
    private static string GoIndexes(string source)
    {
        var matches=System.Text.RegularExpressions.Regex.Matches(source,@"(?m)^func (?:new\w+Rows\(indexed|\(t \*\w+Rows\)buildIndex_)[^{]+\{");
        if(matches.Count==0)throw new Exception("Missing generated Go indexes");
        foreach(var match in matches.Cast<System.Text.RegularExpressions.Match>().Reverse())source=Wrap(source,match.Value,"defer zrProfileDone(5,time.Now());");
        return source.Contains("\"time\"",StringComparison.Ordinal)?source:Replace(source,"package zrconfig","package zrconfig\nimport \"time\"");
    }
    private const string GoMeter="""
var zrProfileNanos [9]atomic.Int64
var zrProfileCalls [9]atomic.Int64
func zrProfileDone(phase int,start time.Time){zrProfileNanos[phase].Add(time.Since(start).Nanoseconds());zrProfileCalls[phase].Add(1)}
func zrProfileHash(input []byte)[32]byte{defer zrProfileDone(1,time.Now());return sha256.Sum256(input)}
func zrProfileReport(){names:=[]string{"read","hash","manifest","scan","decode","index","reference","commit","plan"};stages:=make([]map[string]interface{},9);for i,name:=range names{stages[i]=map[string]interface{}{"stage":name,"calls":zrProfileCalls[i].Load(),"inclusive_ns":zrProfileNanos[i].Load()}};output,e:=json.Marshal(map[string]interface{}{"scenario":"stages","clock":"elapsed","stages":stages});if e!=nil{panic(e)};fmt.Println(string(output))}
""";
    private static string TypeScript(string source)
    {
        source=Replace(source,"crypto.createHash('sha256')","zrProfileHashStart()")+"\n"+TypeScriptMeter;
        foreach(var item in new[]{
            ("    read(relative: string, maxBytes: number, cancellation: Cancellation): Uint8Array {",0),
            ("constructor(source: string | ConfigSource, limits?: ResourceLimits, cancellation=new Cancellation()) {",2),
            ("constructor(wire: Wire, bytes: Uint8Array, json: boolean, readonly budget?: LoadBudget) {",3),
            ("read(index: number): any {",4),
            ("constructor(entries: Array<[K, V]>) {",5),
            ("protected groupIndex(name: string, load: boolean, cancellation?: Cancellation): Map<any, any> {",5),
            ("validateReferences(row: any, resolve: (table: string) => IndexedRows): void {",6),
            ("commit(prepared: PreparedSnapshot<S>, cancellation: Cancellation): CommitResult<S> {",7),
            ("select(requested?: readonly string[]): SelectionPlan {",8)})
            source=Wrap(source,item.Item1,"const zrProfileStarted=performance.now();try{","}finally{zrProfileDone("+item.Item2+",zrProfileStarted);}");
        return source;
    }
    private const string TypeScriptMeter="""
const zrProfileNanos=new Float64Array(9),zrProfileCalls=new Float64Array(9);
function zrProfileDone(phase:number,start:number):void{zrProfileNanos[phase]+=(performance.now()-start)*1e6;zrProfileCalls[phase]++;}
function zrProfileHashStart():any{const start=performance.now(),hash=crypto.createHash('sha256'),digest=hash.digest.bind(hash);hash.digest=((...args:any[])=>{try{return digest(...args);}finally{zrProfileDone(1,start);}})as any;return hash;}
export function zrProfileReport():void{const names=['read','hash','manifest','scan','decode','index','reference','commit','plan'];console.log(JSON.stringify({scenario:'stages',clock:'elapsed',stages:names.map((stage,i)=>({stage,calls:zrProfileCalls[i],inclusive_ns:Math.round(zrProfileNanos[i])}))}));}
""";
    internal static string CopyManagedRuntime(string repository,string destination)
    {
        string original=Path.Combine(repository,"unity","ZrConfig.Sample","Assets","ZrConfig","Runtime");
        Directory.CreateDirectory(destination);
        foreach(string path in Directory.GetFiles(original,"*.cs"))
        {
            string text=File.ReadAllText(path),name=Path.GetFileName(path);
            switch(name)
            {
                case "ConfigSource.cs":text=WrapCs(text,"private byte[] ReadCore(string relativePath, long maxBytes, bool rejectLinks, CancellationToken cancellationToken)",0);break;
                case "ConfigManager.cs":text=WrapCs(text,"internal static bool MatchesHash(SHA256 hash, byte[] bytes, string expected)",1);break;
                case "ConfigServer.cs":
                    text=WrapCs(text,"private ConfigManifest Manifest(byte[] bytes,ResourceLimits limits)",2);
                    text=WrapCs(text,"public CommitResult Commit(PreparedSnapshot prepared, CancellationToken cancellationToken = default)",7);
                    text=Replace(text,"hash.ComputeHash(","global::ZrBenchmarkProfile.Hash(hash,");break;
                case "ConfigWire.cs":text=WrapCs(text,"public byte[] JsonToBinary(byte[] json)",3);break;
                case "ConfigRowReader.cs":
                    text=WrapCs(text,"public ConfigRowReader(byte[] bytes,",3);
                    text=WrapCs(text,"public IConfigRow Read(int index)",4);break;
                case "ConfigServerGroups.cs":text=WrapCs(text,"public void PrepareGroup<T, TKey>(GroupDescriptor<T, TKey> descriptor, CancellationToken cancellationToken = default)",5);break;
                case "ConfigLazyLoader.cs":text=WrapCs(text,"internal void ValidateReference(ConfigRowReference reference, CancellationToken token)",6);break;
                case "ConfigCatalog.cs":text=WrapCs(text,"public SelectionPlan Select(IEnumerable<string>? requested = null)",8);break;
            }
            File.WriteAllText(Path.Combine(destination,name),text);
        }
        File.WriteAllText(Path.Combine(destination,"BenchmarkProfile.cs"),CSharpMeter);
        return destination;
    }
    private static string WrapCs(string source,string marker,int phase)=>Wrap(source,marker,"using var zrProfileScope = new global::ZrBenchmarkProfile.Scope("+phase+");");
    private const string CSharpMeter="""
internal static class ZrBenchmarkProfile
{
 private static readonly long[] Nanos=new long[9],Calls=new long[9];
 internal readonly struct Scope:System.IDisposable
 {
  private readonly int _phase;private readonly long _started;
  internal Scope(int phase){_phase=phase;_started=System.Diagnostics.Stopwatch.GetTimestamp();}
  public void Dispose(){long elapsed=(long)((System.Diagnostics.Stopwatch.GetTimestamp()-_started)*(1e9/System.Diagnostics.Stopwatch.Frequency));System.Threading.Interlocked.Add(ref Nanos[_phase],elapsed);System.Threading.Interlocked.Increment(ref Calls[_phase]);}
 }
 internal static byte[] Hash(System.Security.Cryptography.HashAlgorithm algorithm,byte[] input){using var scope=new Scope(1);return algorithm.ComputeHash(input);}
 internal static void Report(){string[] names={"read","hash","manifest","scan","decode","index","reference","commit","plan"};var stages=new object[9];for(int i=0;i<9;i++)stages[i]=new{stage=names[i],calls=Calls[i],inclusive_ns=Nanos[i]};System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{scenario="stages",clock="elapsed",stages}));}
}
""";
    private static string Lua(string source)
    {
        source=Replace(source,"local R = {}","local R = {}\n"+LuaMeter);
        source=Replace(source,"    return R.readonly(manager)","    manager.commit=zr_profile_wrap(8,manager.commit)\n    return R.readonly(manager)");
        source=Replace(source,"    return R.readonly(catalog)","    catalog.select=zr_profile_wrap(9,catalog.select)\n    return R.readonly(catalog)");
        if(!source.EndsWith("return R\n",StringComparison.Ordinal))throw new Exception("Missing Lua module end");
        return source[..^9]+LuaInstall+"\nreturn R\n";
    }
    private const string LuaMeter="""
local zr_profile_ns,zr_profile_calls={},{}
for i=1,9 do zr_profile_ns[i]=0;zr_profile_calls[i]=0 end
local function zr_profile_wrap(phase,action)
 return function(...)
  local started=os.clock();local values=table.pack(pcall(action,...));zr_profile_ns[phase]=zr_profile_ns[phase]+(os.clock()-started)*1e9;zr_profile_calls[phase]=zr_profile_calls[phase]+1
  if not values[1]then error(values[2],0)end;return table.unpack(values,2,values.n)
 end
end
function R.zr_profile_report()
 local names={'read','hash','manifest','scan','decode','index','reference','commit','plan'};local parts={}
 for i=1,9 do parts[i]=string.format('{"stage":"%s","calls":%d,"inclusive_ns":%.0f}',names[i],zr_profile_calls[i],zr_profile_ns[i])end
 print('{"scenario":"stages","clock":"cpu","stages":['..table.concat(parts,',')..']}')
end
""";
    private const string LuaInstall="""
R.sha256=zr_profile_wrap(2,R.sha256)
R.source_bundle=zr_profile_wrap(3,R.source_bundle)
R.index=zr_profile_wrap(6,R.index)
local zr_original_file_source=R.file_source
R.file_source=function(...)
 local source=zr_original_file_source(...)
 return R.readonly({source_namespace=source.source_namespace,capabilities=source.capabilities,read=zr_profile_wrap(1,source.read)})
end
local zr_original_indexed=zr_profile_wrap(4,indexed_rows)
indexed_rows=function(...)
 local rows=zr_original_indexed(...);rows.read=zr_profile_wrap(5,rows.read);rows.validate=zr_profile_wrap(7,rows.validate);return rows
end
local zr_original_storage=R.row_storage
R.row_storage=function(indexed,make,owner)return zr_original_storage(indexed,zr_profile_wrap(5,make),owner)end
""";
    internal static string Prepare(string generated,string repository,string destination,List<object> builds)
    {
        foreach(var target in new (string Folder,string File,Func<string,string> Instrument)[]{("Java","ConfigRuntime.java",Java),("Go","runtime.go",Go),("Cpp","runtime.hpp",Cpp),("Rust","runtime.rs",Rust),("TypeScript","runtime.ts",TypeScript),("Lua","runtime.lua",Lua)})
        {
            string path=Path.Combine(generated,target.Folder,target.File),original=File.ReadAllText(path),instrumented=target.Instrument(original);
            File.WriteAllText(path,instrumented);
            builds.Add(new{language=target.Folder.ToLowerInvariant(),profiling=true,production_source_sha256=Digest(original),instrumented_source_sha256=Digest(instrumented)});
        }
        string goData=Path.Combine(generated,"Go","data.go"),goBefore=File.ReadAllText(goData),goAfter=GoIndexes(goBefore);File.WriteAllText(goData,goAfter);
        builds.Add(new{language="go-data",profiling=true,production_source_sha256=Digest(goBefore),instrumented_source_sha256=Digest(goAfter)});
        CopyManagedRuntime(repository,destination);
        foreach(string path in Directory.GetFiles(destination,"*.cs").Order(StringComparer.Ordinal))
        {
            string original=Path.Combine(repository,"unity","ZrConfig.Sample","Assets","ZrConfig","Runtime",Path.GetFileName(path));
            builds.Add(new{language="csharp",file=Path.GetFileName(path),profiling=true,production_source_sha256=File.Exists(original)?Digest(File.ReadAllText(original)):null,instrumented_source_sha256=Digest(File.ReadAllText(path))});
        }
        return destination;
    }
    private static string Digest(string text)=>Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    internal static string ExtendDriver(string source,string language)
    {
        var insertion=language switch{
            "csharp"=>("for(int i=0;i<20000;i++)", "global::ZrBenchmarkProfile.Report();"),
            "java"=>("for(int i=0;i<20000;i++)", "try(var profileOwner=DataSnapshot.manager()){profileOwner.reload(()->s,token,PreparationOptions.defaults());}ConfigRuntime.ZrProfile.report();"),
            "go"=>("for i:=0;i<20000;i++", "profileOwner:=&ServerManager[DataSnapshot]{};if _,profileError:=profileOwner.Reload(ctx,func(context.Context)(*DataSnapshot,error){return s,nil},DefaultPreparationOptions());profileError!=nil{t.Fatal(profileError)};profileOwner.Close();zrProfileReport();"),
            "cpp"=>("for(int i=0;i<20000;i++)", "{ServerManager<data::Snapshot> profile_owner;profile_owner.reload([&]{return s;},token);}zr_profile_report();"),
            "rust"=>("for i in 0..20000", "{let profile_owner=zrconfig::data::Snapshot::manager();profile_owner.reload(||Ok(s.clone()),&token,PreparationOptions::default()).unwrap();}zrconfig::runtime::zr_profile_report();"),
            "typescript"=>("for(let i=0;i<20000;i++)", "{const P=require(path.join(process.argv[2],'dist/runtime.js')),profileOwner=new P.ServerManager();profileOwner.reload(()=>s,new P.Cancellation());profileOwner.close();P.zrProfileReport();}"),
            "lua"=>("for i=0,19999 do", "do local P=require('runtime');local profile_owner=P.server_manager(os.clock);profile_owner.reload(function()return s end,P.cancellation());profile_owner.close();P.zr_profile_report()end;"),
            _=>throw new ArgumentException(language)};
        return Replace(source,insertion.Item1,insertion.Item2+insertion.Item1);
    }
    internal static string PrepareLocks(string generated,string repository,string destination,List<object> builds)
    {
        string Instrument(string source,string language)
        {
            switch(language)
            {
                case "java":
                    for(int search=0,serial=0;;)
                    {
                        int at=source.IndexOf("synchronized(gate){",search,StringComparison.Ordinal);if(at<0)break;
                        int open=at+"synchronized(gate)".Length,close=CloseBrace(source,open);string variable="zrLockStart"+serial++;
                        string replacement="{long "+variable+"=System.nanoTime();synchronized(gate){ZrLockMeter.done("+variable+");"+source[(open+1)..(close+1)]+"}";
                        source=source[..at]+replacement+source[(close+1)..];search=at+replacement.Length;
                    }
                    return Replace(source,"public final class ConfigRuntime {","public final class ConfigRuntime {\n"+JavaLockMeter);
                case "go":return Replace(source,"m.gate.Lock()","zrLock(&m.gate)")+"\n"+GoLockMeter;
                case "cpp":
                    source=System.Text.RegularExpressions.Regex.Replace(source,@"std::lock_guard<std::mutex> lock\((state_?)->gate\)","ZrLockGuard lock($1->gate)");
                    return "#include <iostream>\n"+Replace(source,"namespace zrconfig {","namespace zrconfig {\n"+CppLockMeter);
                case "rust":
                    int begin=source.IndexOf("impl<S> ServerManager<S>{",StringComparison.Ordinal),end=CloseBrace(source,begin+"impl<S> ServerManager<S>".Length);
                    source=source[..begin]+Replace(source[begin..(end+1)],"self.state.lock()","zr_lock(&self.state)")+source[(end+1)..];
                    return Replace(source,"#![allow(dead_code)]","#![allow(dead_code)]\n"+RustLockMeter);
                default:return source;
            }
        }
        foreach(var item in new[]{("Java","ConfigRuntime.java"),("Go","runtime.go"),("Cpp","runtime.hpp"),("Rust","runtime.rs")})
        {
            string path=Path.Combine(generated,item.Item1,item.Item2),before=File.ReadAllText(path),after=Instrument(before,item.Item1.ToLowerInvariant());
            if(before==after)throw new Exception("Lock instrumentation missed: "+item.Item1);File.WriteAllText(path,after);
            builds.Add(new{language=item.Item1.ToLowerInvariant(),lock_profiling=true,production_source_sha256=Digest(before),instrumented_source_sha256=Digest(after)});
        }
        Directory.CreateDirectory(destination);
        foreach(string original in Directory.GetFiles(Path.Combine(repository,"unity","ZrConfig.Sample","Assets","ZrConfig","Runtime"),"*.cs"))
        {
            string before=File.ReadAllText(original),after=Path.GetFileName(original)=="ConfigServer.cs"?Replace(before,"lock (_gate)","using (global::ZrLockMeter.Enter(_gate))"):before;
            File.WriteAllText(Path.Combine(destination,Path.GetFileName(original)),after);
            builds.Add(new{language="csharp",file=Path.GetFileName(original),lock_profiling=true,production_source_sha256=Digest(before),instrumented_source_sha256=Digest(after)});
        }
        File.WriteAllText(Path.Combine(destination,"ZrLockMeter.cs"),CSharpLockMeter);return destination;
    }
    internal static string ExtendLockDriver(string source,string language)
    {
        var spec=language switch{
            "csharp"=>("Controls(a,snapshot);","global::ZrLockMeter.Reset();","global::ZrLockMeter.Report();"),
            "java"=>("controls(a,s);","ZrLockMeter.reset();","ZrLockMeter.report();"),
            "go"=>("controls(t,args,s);","zrLockReset();","zrLockReport();"),
            "cpp"=>("controls(a,s);","zr_lock_reset();","zr_lock_report();"),
            "rust"=>("controls(&a,&s);","zr_lock_reset();","zr_lock_report();"),
            "typescript"=>("controls(a,s);","","console.log(JSON.stringify({scenario:'locks',applicable:false,reason:'single-state'}));"),
            "lua"=>("controls(arg,s);","","print('{\"scenario\":\"locks\",\"applicable\":false,\"reason\":\"single-state\"}');"),_=>throw new ArgumentException(language)};
        return Replace(source,spec.Item1,spec.Item2+spec.Item1+spec.Item3);
    }
    private const string CSharpLockMeter="""
using System;using System.Diagnostics;using System.Threading;
public static class ZrLockMeter{
 static long nanos,calls,maximum;public static void Reset(){nanos=calls=maximum=0;}
 public readonly struct Scope:IDisposable{readonly object gate;public Scope(object value){gate=value;long start=Stopwatch.GetTimestamp();Monitor.Enter(gate);long elapsed=(long)((Stopwatch.GetTimestamp()-start)*1e9/Stopwatch.Frequency);Interlocked.Add(ref nanos,elapsed);Interlocked.Increment(ref calls);long before;do{before=Volatile.Read(ref maximum);if(before>=elapsed)break;}while(Interlocked.CompareExchange(ref maximum,elapsed,before)!=before);}public void Dispose()=>Monitor.Exit(gate);}
 public static Scope Enter(object gate)=>new Scope(gate);
 public static void Report()=>Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{scenario="locks",applicable=true,acquisitions=calls,acquisition_ns=nanos,max_acquisition_ns=maximum}));
}
""";
    private const string JavaLockMeter="""
public static final class ZrLockMeter{
 static final java.util.concurrent.atomic.AtomicLong nanos=new java.util.concurrent.atomic.AtomicLong(),calls=new java.util.concurrent.atomic.AtomicLong(),maximum=new java.util.concurrent.atomic.AtomicLong();
 public static void reset(){nanos.set(0);calls.set(0);maximum.set(0);}static void done(long start){long elapsed=System.nanoTime()-start;nanos.addAndGet(elapsed);calls.incrementAndGet();maximum.accumulateAndGet(elapsed,Math::max);}
 public static void report(){System.out.println("{\"scenario\":\"locks\",\"applicable\":true,\"acquisitions\":"+calls.get()+",\"acquisition_ns\":"+nanos.get()+",\"max_acquisition_ns\":"+maximum.get()+"}");}
}
""";
    private const string GoLockMeter="""
var zrLockNanos,zrLockCalls,zrLockMaximum atomic.Int64
func zrLockReset(){zrLockNanos.Store(0);zrLockCalls.Store(0);zrLockMaximum.Store(0)}
func zrLock(gate *sync.Mutex){start:=time.Now();gate.Lock();elapsed:=time.Since(start).Nanoseconds();zrLockNanos.Add(elapsed);zrLockCalls.Add(1);for{before:=zrLockMaximum.Load();if before>=elapsed||zrLockMaximum.CompareAndSwap(before,elapsed){break}}}
func zrLockReport(){encoded,e:=json.Marshal(map[string]interface{}{"scenario":"locks","applicable":true,"acquisitions":zrLockCalls.Load(),"acquisition_ns":zrLockNanos.Load(),"max_acquisition_ns":zrLockMaximum.Load()});if e!=nil{panic(e)};fmt.Println(string(encoded))}
""";
    private const string CppLockMeter="""
inline std::atomic<std::uint64_t> zr_lock_nanos{},zr_lock_calls{},zr_lock_maximum{};
inline void zr_lock_reset(){zr_lock_nanos=0;zr_lock_calls=0;zr_lock_maximum=0;}
struct ZrLockGuard{std::mutex& gate;explicit ZrLockGuard(std::mutex& value):gate(value){auto start=std::chrono::steady_clock::now();gate.lock();auto elapsed=static_cast<std::uint64_t>(std::chrono::duration_cast<std::chrono::nanoseconds>(std::chrono::steady_clock::now()-start).count());zr_lock_nanos.fetch_add(elapsed);zr_lock_calls.fetch_add(1);auto before=zr_lock_maximum.load();while(before<elapsed&&!zr_lock_maximum.compare_exchange_weak(before,elapsed)){} }~ZrLockGuard(){gate.unlock();}ZrLockGuard(const ZrLockGuard&)=delete;};
inline void zr_lock_report(){std::cout<<"{\"scenario\":\"locks\",\"applicable\":true,\"acquisitions\":"<<zr_lock_calls.load()<<",\"acquisition_ns\":"<<zr_lock_nanos.load()<<",\"max_acquisition_ns\":"<<zr_lock_maximum.load()<<"}\n";}
""";
    private const string RustLockMeter="""
static ZR_LOCK_NANOS:std::sync::atomic::AtomicU64=std::sync::atomic::AtomicU64::new(0);static ZR_LOCK_CALLS:std::sync::atomic::AtomicU64=std::sync::atomic::AtomicU64::new(0);static ZR_LOCK_MAXIMUM:std::sync::atomic::AtomicU64=std::sync::atomic::AtomicU64::new(0);
pub fn zr_lock_reset(){for counter in [&ZR_LOCK_NANOS,&ZR_LOCK_CALLS,&ZR_LOCK_MAXIMUM]{counter.store(0,std::sync::atomic::Ordering::Relaxed);}}
fn zr_lock<T>(gate:&std::sync::Mutex<T>)->std::sync::LockResult<std::sync::MutexGuard<'_,T>>{let start=std::time::Instant::now();let result=gate.lock();let elapsed=start.elapsed().as_nanos()as u64;ZR_LOCK_NANOS.fetch_add(elapsed,std::sync::atomic::Ordering::Relaxed);ZR_LOCK_CALLS.fetch_add(1,std::sync::atomic::Ordering::Relaxed);ZR_LOCK_MAXIMUM.fetch_max(elapsed,std::sync::atomic::Ordering::Relaxed);result}
pub fn zr_lock_report(){println!("{{\"scenario\":\"locks\",\"applicable\":true,\"acquisitions\":{},\"acquisition_ns\":{},\"max_acquisition_ns\":{}}}",ZR_LOCK_CALLS.load(std::sync::atomic::Ordering::Relaxed),ZR_LOCK_NANOS.load(std::sync::atomic::Ordering::Relaxed),ZR_LOCK_MAXIMUM.load(std::sync::atomic::Ordering::Relaxed));}
""";

}
