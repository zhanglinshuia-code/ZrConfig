using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

// Frozen legacy business calls for the shared nine-table sample. Build output is isolated
// from the candidate so the same command can provide a genuine API migration receipt.
internal static class SampleConsumers
{
    internal static async Task Run(string language,string configuration,string output)
    {
        configuration=Path.GetFullPath(configuration);output=Path.GetFullPath(output);
        if(language=="all")
        {foreach(string item in new[]{"csharp","java","go","cpp","rust","typescript","lua"})await Run(item,configuration,output);return;}
        string root=Path.Combine(Path.GetTempPath(),"zrconfig-sample-consumer-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        try
        {
            string code=Path.Combine(root,"Config");
            foreach(string file in Directory.GetFiles(configuration,"*",SearchOption.AllDirectories))
            {
                string relative=Path.GetRelativePath(configuration,file);
                if(relative.Split(Path.DirectorySeparatorChar).Any(p=>p is "obj" or "bin"))continue;
                string target=Path.Combine(code,relative);Directory.CreateDirectory(Path.GetDirectoryName(target)!);File.Copy(file,target);
            }
            string generated=Path.Combine(code,"Generated");
            string Tool(string name,string fallback)=>Environment.GetEnvironmentVariable(name)??fallback;
            async Task Execute(string tool,string[] arguments,string? work=null)
            {var result=await ServerRuntimeBenchmarks.Execute(tool,arguments,work??root);if(result.Output.Length!=0)Console.Write(result.Output);}
            if(language=="csharp")
            {
                File.WriteAllText(Path.Combine(code,"SampleConsumer.cs"),CSharp);
                var trees=Directory.GetFiles(code,"*.cs",SearchOption.AllDirectories).Select(f=>CSharpSyntaxTree.ParseText(File.ReadAllText(f),path:f));
                var references=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Where(p=>Path.GetDirectoryName(p)==Path.GetDirectoryName(typeof(object).Assembly.Location)).Append(typeof(ZrConfig.Runtime.ITableCodec).Assembly.Location).Distinct().Select(p=>MetadataReference.CreateFromFile(p));
                using var binary=new MemoryStream();var compiled=CSharpCompilation.Create("SampleConsumer"+Guid.NewGuid().ToString("N"),trees,references,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,generalDiagnosticOption:ReportDiagnostic.Error)).Emit(binary);
                if(!compiled.Success)throw new Exception(string.Join("\n",compiled.Diagnostics));
                Assembly.Load(binary.ToArray()).GetType("SampleConsumer")!.GetMethod("Run")!.Invoke(null,[output]);
            }
            else if(language=="java")
            {
                string compiler=Tool("ZRCONFIG_JAVAC","javac"),classes=Path.Combine(root,"classes"),source=Path.Combine(root,"SampleConsumer.java");Directory.CreateDirectory(classes);File.WriteAllText(source,Java);
                await Execute(compiler,["--release","17","-encoding","UTF-8","-Xlint:unchecked,deprecation","-Werror","-d",classes,..Directory.GetFiles(Path.Combine(generated,"Java"),"*.java"),source]);
                await Execute(Path.Combine(Path.GetDirectoryName(compiler)??"",OperatingSystem.IsWindows()?"java.exe":"java"),["-cp",classes,"SampleConsumer",output]);
            }
            else if(language=="go")
            {
                string source=Path.Combine(generated,"Go");File.WriteAllText(Path.Combine(source,"sample_consumer_test.go"),Go);
                await Execute(Tool("ZRCONFIG_GO","go"),["test","-run","TestSampleConsumer","-v",".","-args",output],source);
            }
            else if(language=="cpp")
            {
                string source=Path.Combine(root,"sample.cpp"),binary=Path.Combine(root,OperatingSystem.IsWindows()?"sample.exe":"sample");File.WriteAllText(source,Cpp);string compiler=Tool("ZRCONFIG_CXX","clang++");
                await Execute(compiler,PortableExportTests.CppArguments(compiler,["-std=c++17","-O2","-Wall","-Wextra","-Werror","-pedantic","-pthread","-I",Path.Combine(generated,"Cpp"),source,"-o",binary]));await Execute(binary,[output]);
            }
            else if(language=="rust")
            {
                string compiler=Tool("ZRCONFIG_RUSTC","rustc"),library=Path.Combine(root,"libzrconfig.rlib"),source=Path.Combine(root,"sample.rs"),binary=Path.Combine(root,OperatingSystem.IsWindows()?"sample.exe":"sample");File.WriteAllText(source,Rust);
                await Execute(compiler,["--edition","2021","-Dwarnings","--crate-name","zrconfig","--crate-type","rlib",Path.Combine(generated,"Rust","lib.rs"),"-o",library]);
                await Execute(compiler,["--edition","2021","-Dwarnings","--extern","zrconfig="+library,source,"-o",binary]);await Execute(binary,[output]);
            }
            else if(language=="typescript")
            {
                string source=Path.Combine(generated,"TypeScript"),compiler=Path.Combine(root,"compile.cjs");File.WriteAllText(compiler,ServerRuntimeBenchmarks.TypeScriptCompiler);File.WriteAllText(Path.Combine(source,"sample_consumer.ts"),TypeScript);
                string node=Tool("ZRCONFIG_NODE","node");
                if(Environment.GetEnvironmentVariable("ZRCONFIG_TSC") is { Length: > 0 } tsc&&string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ZRCONFIG_TYPESCRIPT")))await Execute(tsc,["-p",source]);
                else await Execute(node,[compiler,Tool("ZRCONFIG_TYPESCRIPT","typescript"),source]);
                await Execute(node,[Path.Combine(source,"dist","sample_consumer.js"),output]);
            }
            else if(language=="lua")
            {string source=Path.Combine(root,"sample.lua");File.WriteAllText(source,Lua);await Execute(Tool("ZRCONFIG_LUA","lua"),[source,Path.Combine(generated,"Lua"),output]);}
            else throw new ArgumentException("Unknown sample consumer language.",nameof(language));
            Console.WriteLine("Frozen sample consumer passed: "+language);
        }
        finally{Directory.Delete(root,true);}
    }
    private const string CSharp="""
using System;using System.Text.Json;using ZrConfig.Models;using ZrConfig.Runtime;
public static class SampleConsumer {
 public static void Run(string root){foreach(bool json in new[]{false,true}){using var manager=new ConfigManager(TableCodecs.Registry,s=>JsonSerializer.Deserialize<ConfigManifest>(s,new JsonSerializerOptions{IncludeFields=true})!,new ConfigLoadOptions(preferJson:json,retainPreviousSnapshots:true));ConfigSnapshot snapshot=manager.ReloadAll(new FileConfigSource(root));AllTypes10kConfigTable table=snapshot.GetAllTypes10kTable();Func<int,AllTypes10kConfig> get=table.Get;AllTypes10kConfig row=get(1);if(table.Count!=10000||row.Id!=1||table.GetById(1).Count!=1||table.TryGet(-1,out _))throw new Exception("legacy sample lookup");if(row.Previous!=null&&row.Previous.Id<1)throw new Exception("legacy reference");if(snapshot.GetHeroTable().Count==0||snapshot.GetSkillTable().Count==0)throw new Exception("legacy dependency");}}
}
""";
    private const string Java="""
import zrconfig.*;import java.nio.file.*;import java.util.function.*;
public final class SampleConsumer { public static void main(String[] args)throws Exception{for(boolean json:new boolean[]{false,true}){ConfigSnapshot s=ConfigSnapshot.load(Path.of(args[0]),json);AllTypes10kConfig.Table table=s.AllTypes10k;IntFunction<AllTypes10kConfig> get=table::get;AllTypes10kConfig row=get.apply(1);if(table.count()!=10000||row.getId()!=1||table.getById(1).size()!=1||table.find(-1)!=null)throw new AssertionError("legacy lookup");if(row.getPrevious()!=null&&row.getPrevious().getId()<1)throw new AssertionError("legacy reference");if(s.Hero.count()==0||s.Skill.count()==0)throw new AssertionError("legacy dependency");}}}
""";
    private const string Go="""
package zrconfig
import("os";"testing")
type sampleLookup interface{Get(int32)*AllTypes10kConfig;Count()int}
var _ sampleLookup=(*AllTypes10kTable)(nil)
func TestSampleConsumer(t *testing.T){root:=os.Args[len(os.Args)-1];for _,json:=range []bool{false,true}{s,e:=LoadConfig(root,json);if e!=nil{t.Fatal(e)};var table sampleLookup=s.AllTypes10k();var get func(int32)*AllTypes10kConfig=table.Get;row:=get(1);if table.Count()!=10000||row.Id()!=1||s.AllTypes10k().GetById(1).Len()!=1{t.Fatal("legacy lookup")};if _,ok:=s.AllTypes10k().Find(-1);ok{t.Fatal("legacy missing")};if row.Previous()!=nil&&row.Previous().Id()<1{t.Fatal("legacy reference")};if s.Hero().Count()==0||s.Skill().Count()==0{t.Fatal("legacy dependency")}}}
""";
    private const string Cpp="""
#include "snapshot.hpp"
#include <cassert>
#include <type_traits>
using namespace zrconfig;
template<class T>const AllTypes10kConfig& sample_get(const T& table){return table.get(1);}
int main(int,char** args){for(bool json:{false,true}){std::shared_ptr<const ConfigSnapshot> s=ConfigSnapshot::load(args[1],json);const AllTypes10kTable& table=s->AllTypes10k();const AllTypes10kConfig& (AllTypes10kTable::*get)(const std::int32_t&)const=&AllTypes10kTable::get;const auto& row=(table.*get)(1);assert(table.count()==10000&&row.Id()==1&&table.getById(1).size()==1&&table.find(-1)==nullptr);assert(&sample_get(table)==&row);if(row.Previous())assert(row.Previous()->Id()>0);assert(s->Hero().count()>0&&s->Skill().count()>0);}}
""";
    private const string Rust="""
extern crate zrconfig;
use zrconfig::snapshot::ConfigSnapshot;use zrconfig::table_AllTypes10k::{AllTypes10kConfig,AllTypes10kTable};
trait SampleLookup{fn business_get(&self,id:i32)->&AllTypes10kConfig;}impl SampleLookup for AllTypes10kTable{fn business_get(&self,id:i32)->&AllTypes10kConfig{self.get(id).unwrap()}}
fn main(){let a:Vec<String>=std::env::args().collect();for json in [false,true]{let s=ConfigSnapshot::load(std::path::Path::new(&a[1]),json).unwrap();let table:&AllTypes10kTable=s.get_AllTypes10k();let get:fn(&AllTypes10kTable,i32)->zrconfig::runtime::Result<&AllTypes10kConfig>=AllTypes10kTable::get;let row=get(table,1).unwrap();assert_eq!(table.count(),10000);assert_eq!(row.get_Id(),1);assert_eq!(table.get_ById(1).len(),1);assert!(table.find(-1).is_none());assert!(std::ptr::eq(table.business_get(1),row));if let Some(previous)=row.get_Previous(){assert!(previous.get_Id()>0)}assert!(s.get_Hero().count()>0&&s.get_Skill().count()>0)}}
""";
    private const string TypeScript="""
import {ConfigSnapshot} from './snapshot';import {AllTypes10kConfig,AllTypes10kTable} from './AllTypes10kConfig';
declare const process:{argv:string[]};
for(const json of [false,true]){const s:ConfigSnapshot=ConfigSnapshot.load(process.argv[2],json),table:AllTypes10kTable=s.AllTypes10k;const get:(id:number)=>AllTypes10kConfig=table.get.bind(table);const row:AllTypes10kConfig=get(1);if(table.count!==10000||row.Id!==1||table.getById(1).length!==1||table.find(-1)!==undefined)throw Error('legacy lookup');if(row.Previous&&row.Previous.Id<1)throw Error('legacy reference');if(!s.Hero.count||!s.Skill.count)throw Error('legacy dependency');}
""";
    private const string Lua="""
package.path=arg[1]..'/?.lua;'..package.path;local D=require('snapshot');for _,json in ipairs{false,true}do local s=D.load(arg[2],json);local t=s.AllTypes10k;local get=t['get'];local row=get(1);assert(#t.rows==10000 and row.Id==1 and #t.getById(1)==1 and t.find(-1)==nil);if row.Previous then assert(row.Previous.Id>0)end;assert(#s.Hero.rows>0 and #s.Skill.rows>0)end
""";
}
