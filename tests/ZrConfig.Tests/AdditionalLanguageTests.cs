using System.Diagnostics;
using System.Text.Json;

// 运行真实生成物；不能用动态字典替代强类型源码编译验收。
internal static class AdditionalLanguageTests
{
    internal static (string? Node, string? Lua, string? Rust, string? TypeScript, string? Tsc) ResolveTools(bool required)
    {
        string? node=PortableExportTests.FindTool("ZRCONFIG_NODE",OperatingSystem.IsWindows()?"node.exe":"node");
        string? lua=PortableExportTests.FindTool("ZRCONFIG_LUA",OperatingSystem.IsWindows()?"lua.exe":"lua");
        string? rust=PortableExportTests.FindTool("ZRCONFIG_RUSTC",OperatingSystem.IsWindows()?"rustc.exe":"rustc");
        string? ts=Environment.GetEnvironmentVariable("ZRCONFIG_TYPESCRIPT");
        // A configured compiler API is authoritative, including when it is missing.
        string? tsc=string.IsNullOrEmpty(ts)?PortableExportTests.FindTool("ZRCONFIG_TSC",OperatingSystem.IsWindows()?"tsc.cmd":"tsc"):null;
        Console.WriteLine($"工具链 ZRCONFIG_TYPESCRIPT: {ts ?? "<未设置>"}; 文件存在={File.Exists(ts)}");
        var missing=new List<string>();
        if(node==null)missing.Add(PortableExportTests.MissingTool("ZRCONFIG_NODE","node"));
        if(lua==null)missing.Add(PortableExportTests.MissingTool("ZRCONFIG_LUA","lua"));
        if(rust==null)missing.Add(PortableExportTests.MissingTool("ZRCONFIG_RUSTC","rustc"));
        if(!File.Exists(ts)&&tsc==null)missing.Add(PortableExportTests.MissingTool(string.IsNullOrEmpty(ts)?"ZRCONFIG_TSC":"ZRCONFIG_TYPESCRIPT","TypeScript"));
        if(required&&missing.Count!=0)throw new InvalidOperationException("七语言验收缺少工具链："+string.Join("; ",missing));
        return(node,lua,rust,ts,tsc);
    }

    internal static async Task RunAsync(string generated, string output, string root, bool required,string[]? namespaceRoots=null)
    {
        var (node,lua,rust,ts,tsc)=ResolveTools(required);
        if(node==null||lua==null||rust==null||(!File.Exists(ts)&&tsc==null))
        {
            Console.WriteLine("跳过 Lua/TypeScript/Rust 实际运行：工具链不齐；--portable-native 强制要求全部工具链。");return;
        }
        if(namespaceRoots!=null)
        {
            var libraries=new List<string>();
            for(int index=0;index<namespaceRoots.Length;index++)
            {
                string id=index==0?"a":"b",native=namespaceRoots[index],tsPath=Path.Combine(native,"TypeScript"),library=Path.Combine(root,"libgame"+id+".rlib");
                if(File.Exists(ts)){string compiler=Path.Combine(root,"compile-namespace-"+id+".cjs");File.WriteAllText(compiler,TypeScriptCompiler);await Run(node,[compiler,ts!,tsPath],root);}else await Run(tsc!,["-p",tsPath],root);
                await Run(rust,["--edition","2021","--crate-name","game"+id,"--crate-type","rlib","-D","warnings",Path.Combine(native,"Rust","lib.rs"),"-o",library],root);libraries.Add(library);
            }
            string rustMain=Path.Combine(root,"two-rust.rs"),rustBinary=Path.Combine(root,OperatingSystem.IsWindows()?"two-rust.exe":"two-rust");
            File.WriteAllText(rustMain,"extern crate gamea;extern crate gameb;fn main(){let root=std::env::args().nth(1).unwrap();let root=std::path::Path::new(&root);let a=gamea::data::Snapshot::load(root,&gamea::runtime::LoadPlan::default(),&gamea::runtime::Cancellation::default()).unwrap();let b=gameb::data::Snapshot::load(root,&gameb::runtime::LoadPlan::default(),&gameb::runtime::Cancellation::default()).unwrap();assert_eq!(a.get_Entry(\"east\",1).unwrap().get_Id(),b.get_Entry(\"east\",1).unwrap().get_Id());}");
            await Run(rust,["--edition","2021","-D","warnings","--extern","gamea="+libraries[0],"--extern","gameb="+libraries[1],rustMain,"-o",rustBinary],root);await Run(rustBinary,[output],root);
            string luaMain=Path.Combine(root,"two.lua");File.WriteAllText(luaMain,"package.path=arg[1]..'/Lua/?.lua;'..arg[2]..'/Lua/?.lua;'..package.path;local a=require('gamea.config.data').load(arg[3]);local b=require('gameb.config.data').load(arg[3]);assert(a.get_Entry().get('east',1).Id==b.get_Entry().get('east',1).Id)");await Run(lua,[luaMain,namespaceRoots[0],namespaceRoots[1],output],root);
            string jsMain=Path.Combine(root,"two.cjs");File.WriteAllText(jsMain,"const path=require('path'),a=require(path.join(process.argv[2],'TypeScript/dist/data.js')).Snapshot.load(process.argv[4]),b=require(path.join(process.argv[3],'TypeScript/dist/data.js')).Snapshot.load(process.argv[4]);require('node:assert/strict').equal(a.get_Entry().get('east',1).Id,b.get_Entry().get('east',1).Id);");await Run(node,[jsMain,namespaceRoots[0],namespaceRoots[1],output],root);
            Console.WriteLine("通过：Rust/TypeScript/Lua 自定义 crate/package/require 根与同进程双 Catalog。");
        }
        foreach(string kind in new[]{"empty-server","empty-legacy","large-forward-server","large-forward-legacy"})
        {
            string boundary=Path.Combine(root,kind),tsPath=Path.Combine(boundary,"TypeScript");
            if(File.Exists(ts)) { string compiler=Path.Combine(boundary,"compile.cjs"); File.WriteAllText(compiler,TypeScriptCompiler); await Run(node,[compiler,ts!,tsPath],boundary); }
            else await Run(tsc!,["-p",tsPath],boundary);
            if(kind.StartsWith("large-forward-",StringComparison.Ordinal))
            {
                string js=Path.Combine(boundary,"boundary.cjs");File.WriteAllText(js,LargeTypeScriptBoundary(kind.EndsWith("-server",StringComparison.Ordinal)));
                await Run(node,["--expose-gc",js,tsPath,Path.Combine(boundary,"Output")],boundary);
            }
            string rustSource=Path.Combine(boundary,"check.rs");
            string boundedStack=kind.StartsWith("large-forward-",StringComparison.Ordinal)?"assert!(std::mem::size_of::<zrconfig::ConfigSnapshot>()<4096);"+(kind.EndsWith("-server",StringComparison.Ordinal)?"assert!(std::mem::size_of::<zrconfig::data::Snapshot>()<4096);":""):"";
            string largeRuntime=kind.StartsWith("large-forward-",StringComparison.Ordinal)?LargeRustBoundary(kind.EndsWith("-server",StringComparison.Ordinal)):"";
            File.WriteAllText(rustSource,"#[path = "+JsonSerializer.Serialize(Path.Combine(boundary,"Rust","lib.rs"))+"]\nmod zrconfig;\nfn main() {"+boundedStack+largeRuntime+"}\n");
            await Run(rust,["--edition","2021","-D","warnings",rustSource,"-o",Path.Combine(boundary,"rust-check")],boundary);
            await Run(Path.Combine(boundary,"rust-check"),[Path.Combine(boundary,"Output")],boundary);
            string script=Path.Combine(boundary,"check.lua");
            File.WriteAllText(script,"package.path=arg[1]..'/?.lua;'..package.path;assert(require('"+(kind.EndsWith("-server",StringComparison.Ordinal)?"data":"snapshot")+"'));"+(kind.StartsWith("large-forward-",StringComparison.Ordinal)?LargeLuaBoundary(kind.EndsWith("-server",StringComparison.Ordinal)):""));
            await Run(lua,[script,Path.Combine(boundary,"Lua"),Path.Combine(boundary,"Output")],boundary);
        }
        // 保留真实清单与哈希，将内容损坏，验证失败来自完整性校验而不是路径缺失。
        string broken = output + "-broken";
        using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(output,"current.json"))))
        {
            string version = manifest.RootElement.GetProperty("versionId").GetString()!;
            string folder = Path.Combine(broken,"versions",version);Directory.CreateDirectory(folder);
            File.Copy(Path.Combine(output,"current.json"),Path.Combine(broken,"current.json"),true);
            foreach (var entry in manifest.RootElement.GetProperty("tables").EnumerateArray())
            {
                File.WriteAllText(Path.Combine(folder,entry.GetProperty("file").GetString()!),"broken");
                if(entry.TryGetProperty("jsonFile",out var json))File.WriteAllText(Path.Combine(folder,json.GetString()!),"broken");
            }
        }
        string tsRoot=Path.Combine(generated,"TypeScript");
        if(File.Exists(ts))
        {
            string compiler=Path.Combine(root,"compile-typescript.cjs");File.WriteAllText(compiler,TypeScriptCompiler);
            await Run(node,[compiler,ts!,tsRoot],root);
        }
        else await Run(tsc!,["-p",tsRoot],root);
        string tsCheck=Path.Combine(root,"typescript-check.cjs");File.WriteAllText(tsCheck,TypeScriptCheck);
        await Run(node,["--expose-gc",tsCheck,tsRoot,output],root);
        string tsServer=Path.Combine(root,"typescript-server.cjs");File.WriteAllText(tsServer,ServerRuntimeTests.TypeScript);
        await Run(node,["--expose-gc",tsServer,tsRoot,output],root);
        string luaCheck=Path.Combine(root,"lua-check.lua");File.WriteAllText(luaCheck,LuaCheck);
        await Run(lua,[luaCheck,Path.Combine(generated,"Lua"),output],root);
        string luaServer=Path.Combine(root,"lua-server.lua");File.WriteAllText(luaServer,ServerRuntimeTests.Lua);
        await Run(lua,[luaServer,Path.Combine(generated,"Lua"),output],root);
        string rustLibrary=Path.Combine(root,"libzrconfig.rlib");
        await Run(rust,["--edition","2021","-O","-D","warnings","--crate-name","zrconfig","--crate-type","rlib",Path.Combine(generated,"Rust","lib.rs"),"-o",rustLibrary],root);
        string rustCheck=Path.Combine(root,"rust_check.rs"),rustExe=Path.Combine(root,OperatingSystem.IsWindows()?"rust-check.exe":"rust-check");
        File.WriteAllText(rustCheck,"extern crate zrconfig;\n"+RustCheck);
        await Run(rust,["--edition","2021","-O","-D","warnings","--extern","zrconfig="+rustLibrary,rustCheck,"-o",rustExe],root);
        await Run(rustExe,[output],root);
        string serverCheck=Path.Combine(root,"rust_server.rs"),serverExe=Path.Combine(root,OperatingSystem.IsWindows()?"rust-server.exe":"rust-server");
        File.WriteAllText(serverCheck,"extern crate zrconfig;\n"+ServerRuntimeTests.Rust);
        await Run(rust,["--edition","2021","-O","-D","warnings","--extern","zrconfig="+rustLibrary,serverCheck,"-o",serverExe],root);
        await Run(serverExe,[output],root);
        // Compile a temporary copy with private scheduling fault injection; emitted files stay intact.
        string internalRuntime=Path.Combine(root,"rust-runtime-internals.rs"),internalCheck=Path.Combine(root,"rust-internal-check.rs");
        string internalBinary=Path.Combine(root,OperatingSystem.IsWindows()?"rust-internal-check.exe":"rust-internal-check");
        File.WriteAllText(internalRuntime,File.ReadAllText(Path.Combine(generated,"Rust","runtime.rs"))+"\n"+ServerRuntimeTests.RustScheduling);
        File.WriteAllText(internalCheck,"#[path = "+JsonSerializer.Serialize(internalRuntime)+"]\nmod runtime;\n");
        await Run(rust,["--edition","2021","--test","-O","-D","warnings",internalCheck,"-o",internalBinary],root);
        await Run(internalBinary,[],root);
    }
    private static string LargeTypeScriptBoundary(bool server) => """
        const assert=require('node:assert/strict'),path=require('node:path'),base=process.argv[2],root=process.argv[3];
        const legacy=require(path.join(base,'dist/snapshot.js')).ConfigSnapshot.load(root,true);
        assert.equal(legacy.Forward000.get(1).Peer.Peer.Id,1);
        """ + (server ? """
        const D=require(path.join(base,'dist/data.js')),S=require(path.join(base,'dist/server.js')),R=require(path.join(base,'dist/runtime.js'));
        (async()=>{
          const selected=S.SelectedSnapshot.load(root,['Forward000'],true);assert.equal(selected.get_Forward256().get(1).Peer.Id,1);
          const token=new R.Cancellation(),scheduler=new R.NodeLoadScheduler();
          for(const mode of ['ServerDefault','SelectedTables','LazyRowsStrict','LazyRowsOnAccess']){
            const plan={mode,preferJSON:true,...(mode==='SelectedTables'?{tables:['Forward000']}:{})};
            for(const snapshot of [D.Snapshot.load(root,plan,token),await D.Snapshot.loadAsync(root,plan,token,scheduler)]){
              const expected=mode==='ServerDefault'?257:mode==='SelectedTables'?2:0;assert.equal(snapshot.loadedRowCount,expected);assert.equal(snapshot.completedTableCount,expected);
              const table=snapshot.get_Forward000(),row=table.load(1);assert.equal(row.load_Peer().load_Peer(),row);assert.equal(table.loadGroup_ById(1).length,1);
            }
          }
          const plan={mode:'LazyRowsOnAccess',preferJSON:true,warmupTables:['Forward256']},steps=[D.Snapshot.warmupRow_Forward000(1),D.Snapshot.warmupGroup_Forward000_ById(1),D.Snapshot.warmupRow_ZrConfigSnapshotPart0(1,'owned-key'),D.Snapshot.warmupGroup_ZrConfigSnapshotPart0_ByTextAndId('owned-key',1)];
          for(const snapshot of [D.Snapshot.loadWithWarmup(root,plan,steps,token),await D.Snapshot.loadWithWarmupAsync(root,plan,steps,token,scheduler)]){assert.equal(snapshot.loadedRowCount,3);assert.equal(snapshot.completedTableCount,1);}
          function retained(){const snapshot=D.Snapshot.load(root,{preferJSON:true});return {weak:new WeakRef(snapshot),row:snapshot.get_Forward000().get(1)};}
          const held=retained();global.gc();assert.ok(held.weak.deref());assert.equal(held.row.get_Peer().get_Peer(),held.row);held.row=undefined;
          for(let i=0;i<20;i++){await new Promise(resolve=>setImmediate(resolve));global.gc();}assert.equal(held.weak.deref(),undefined);
        })().catch(error=>{console.error(error);process.exitCode=1;});
        """ : "");

    private static string LargeLuaBoundary(bool server) => """
        local legacy=require('snapshot').load(arg[2],true);assert(legacy.Forward000.get(1).Peer.Peer.Id==1)
        """ + (server ? """
        local D=require('data');local token=D.cancellation()
        for _,mode in ipairs({'ServerDefault','SelectedTables','LazyRowsStrict','LazyRowsOnAccess'})do
          local snapshot=D.load(arg[2],{mode=mode,prefer_json=true,tables=mode=='SelectedTables'and{'Forward000'}or nil},token)
          local expected=mode=='ServerDefault'and 257 or mode=='SelectedTables'and 2 or 0
          assert(snapshot.loaded_row_count()==expected and snapshot.completed_table_count()==expected)
          local row=snapshot.get_Forward000().load(1,token);assert(row.load_Peer(token).load_Peer(token)==row);assert(#snapshot.get_Forward000().load_group_ById(1,token)==1)
        end
        local snapshot=D.load_with_warmup(arg[2],{mode='LazyRowsOnAccess',prefer_json=true,warmup_tables={'Forward256'}},{D.warmup_row_Forward000(1),D.warmup_group_Forward000_ById(1),D.warmup_row_ZrConfigSnapshotPart0(1,'owned-key'),D.warmup_group_ZrConfigSnapshotPart0_ByTextAndId('owned-key',1)},token)
        assert(snapshot.loaded_row_count()==3 and snapshot.completed_table_count()==1)
        local weak=setmetatable({snapshot},{__mode='v'});local row=snapshot.get_Forward000().get(1);snapshot=nil;collectgarbage('collect');assert(weak[1]~=nil and row.get_Peer().get_Peer()==row)
        row=nil;collectgarbage('collect');collectgarbage('collect');assert(weak[1]==nil)
        """ : "");

    private static string LargeRustBoundary(bool server) => """
        let root_arg=std::env::args().nth(1).unwrap();let root=std::path::Path::new(&root_arg);
        let legacy=zrconfig::ConfigSnapshot::load(root,true).unwrap();
        assert_eq!(legacy.get_Forward000().get(1).unwrap().get_Peer().unwrap().get_Peer().unwrap().get_Id(),1);
        drop(legacy);
        """ + (server ? """
        use zrconfig::runtime::{Cancellation,LoadPlan,LoadMode,PreparationOptions};
        use zrconfig::data::Snapshot;use std::sync::Arc;
        let token=Cancellation::default();
        for mode in [LoadMode::ServerDefault,LoadMode::SelectedTables,LoadMode::LazyRowsStrict,LoadMode::LazyRowsOnAccess]{
            let plan=LoadPlan{mode,prefer_json:true,tables:if mode==LoadMode::SelectedTables{Some(vec!["Forward000".to_owned()])}else{None},warmup_tables:vec![]};
            let snapshot=Snapshot::load(root,&plan,&token).unwrap();
            let expected=if mode==LoadMode::ServerDefault{257}else if mode==LoadMode::SelectedTables{2}else{0};
            assert_eq!(snapshot.loaded_row_count(),expected);assert_eq!(snapshot.completed_table_count(),expected);
            let row=snapshot.load_Forward000(1,&token).unwrap();
            assert_eq!(row.load_Peer(&token).unwrap().unwrap().load_Peer(&token).unwrap().unwrap().get_Id(),1);
            assert_eq!(snapshot.load_group_Forward000_ById(1,&token).unwrap().len(),1);
        }
        let warm=LoadPlan{mode:LoadMode::LazyRowsOnAccess,prefer_json:true,tables:None,warmup_tables:vec!["Forward256".to_owned()]};
        let snapshot=Snapshot::load_with_warmup(root,&warm,&[Snapshot::warmup_row_Forward000(1),Snapshot::warmup_group_Forward000_ById(1)],&token).unwrap();
        assert_eq!(snapshot.loaded_row_count(),2);assert_eq!(snapshot.completed_table_count(),1);drop(snapshot);
        let plan=LoadPlan{prefer_json:true,..LoadPlan::default()};let manager=Snapshot::manager();
        manager.reload(||Snapshot::load(root,&plan,&token),&token,PreparationOptions::default()).unwrap();
        let original=manager.acquire_snapshot().unwrap();let weak=Arc::downgrade(&original);let row=original.get_Forward000(1).unwrap();
        let path=root.join("versions/boundary/Forward256.json");let bytes=std::fs::read(&path).unwrap();let mut changed=bytes.clone();changed.push(b' ');std::fs::write(&path,&changed).unwrap();
        assert!(manager.reload(||Snapshot::load(root,&plan,&token),&token,PreparationOptions::default()).is_err());
        assert!(Arc::ptr_eq(&original,&manager.acquire_snapshot().unwrap()));
        std::fs::write(path,bytes).unwrap();manager.reload(||Snapshot::load(root,&plan,&token),&token,PreparationOptions::default()).unwrap();
        manager.close().unwrap();drop(original);assert!(weak.upgrade().is_some());
        assert_eq!(row.get_Peer().unwrap().unwrap().get_Peer().unwrap().unwrap().get_Id(),1);drop(row);assert!(weak.upgrade().is_none());
        """ : "");

    private static async Task Run(string file,string[] args,string root)
    {
        var start=new ProcessStartInfo(file){WorkingDirectory=root,RedirectStandardOutput=true,RedirectStandardError=true,UseShellExecute=false};foreach(string a in args)start.ArgumentList.Add(a);
        using var process=Process.Start(start)??throw new Exception("进程启动失败："+file);
        Task<string> stdout=process.StandardOutput.ReadToEndAsync(),stderr=process.StandardError.ReadToEndAsync();
        try{await process.WaitForExitAsync().WaitAsync(TimeSpan.FromMinutes(3));}catch{process.Kill(true);throw;}
        string text=await stdout+await stderr;if(process.ExitCode!=0)throw new InvalidOperationException(file+"\n"+text);Console.WriteLine(text.Trim());
    }
    private const string TypeScriptCompiler="""
        const ts=require(process.argv[2]),path=require('path'),fs=require('fs'),root=process.argv[3];
        const files=fs.readdirSync(root).filter(f=>f.endsWith('.ts')).map(f=>path.join(root,f));
        const program=ts.createProgram(files,{target:ts.ScriptTarget.ES2022,module:ts.ModuleKind.CommonJS,strict:true,types:[],outDir:path.join(root,'dist'),declaration:true});
        const errors=ts.getPreEmitDiagnostics(program);
        if(errors.length){console.error(ts.formatDiagnosticsWithColorAndContext(errors,{getCanonicalFileName:f=>f,getCurrentDirectory:()=>root,getNewLine:()=> '\n'}));process.exit(1);}
        if(program.emit().emitSkipped)process.exit(1);console.log('通过：TypeScript '+ts.version+' 严格类型编译。');
        """;
    private const string TypeScriptCheck="""
        'use strict';
        const path=require('path'),assert=require('assert/strict');
        const root=process.argv[3],{ConfigManager}=require(path.join(process.argv[2],'dist/snapshot.js'));
        const R=require(path.join(process.argv[2],'dist/runtime.js'));
        let sink=0;
        function probe(s){const before=process.memoryUsage().heapUsed;for(let i=0;i<1000000;i++)sink=s.Entry.get('east',1).Id+s.Entry.getById(1).length;return process.memoryUsage().heapUsed-before;}
        const manager=new ConfigManager();
        for(const json of [false,true]){
            const s=manager.reload(root,json),r=s.Entry.get('east',1);
            assert.equal(s.Concrete.get(1).Payload.Delay,7);assert.equal(r.Name,'中文😀');assert.equal(r.Signed,-9223372036854775808n);assert.equal(r.Unsigned,18446744073709551615n);
            assert.equal(r.Amount.toString(),'79228162514264337593543950335');assert.equal(r.Optional,null);assert.equal(r.Values[0],null);assert.equal(r.Shape.values[3],4);
            assert.equal(r.Mapping.get(7),'值');assert.equal(r.Lookup,s.Lookup.get(10));assert.equal(r.Lookup.Peer.Peer,r.Lookup);assert.equal(r.Action.Delay,2);assert.equal(r.Quality,18446744073709551615n);
            assert.equal(r.class,'keyword');assert.equal(s.Entry.getById(1).length,2);assert.equal(s.Entry.getKey('中文😀').length,2);
            assert(Object.isFrozen(r));assert.throws(()=>{r.Name='changed'});assert.throws(()=>{r.Values[0]=1});
            for(let warm=0;warm<5;warm++)probe(s);global.gc();const growth=probe(s);assert(growth<65536,'热查询出现持续堆增长：'+growth);
            const next=manager.reload(root,!json);assert.equal(s.Entry.get('east',1),r);assert.notEqual(next,s);assert.throws(()=>manager.reload(path.join(root,'missing'),json));assert.throws(()=>manager.reload(root+'-broken',json),/完整性/);assert.equal(manager.current(),next);
            for(const bad of ['extra','missing','replaced']){assert.throws(()=>manager.reload(root+'-'+bad,json),/表集合/);assert.equal(manager.current(),next);}
            assert.throws(()=>R.parseJSON('"\\uD800"'));assert.throws(()=>R.parseJSON('{"a":1,"a":2}'));
            assert.equal(new R.Decimal('1.2500').key,new R.Decimal('1.25').key);assert.throws(()=>new R.Decimal('0.12345678901234567890123456789'));
            console.log('通过：TypeScript JSON='+json+'，1000000 次主键+分组查询堆变化 '+growth+' B；计数包含 Node 测量开销，不等同精确分配量。');
        }
        """;
    private const string LuaCheck="""
        package.path=arg[1]..'/?.lua;'..package.path
        local S=require('snapshot');local R=require('runtime');local manager=S.manager();local root=arg[2]
        local sink=0
        local function probe(s)
            local before=collectgarbage('count')
            for i=1,100000 do sink=s.Entry.get('east',1).Id+#s.Entry.getById(1) end
            return (collectgarbage('count')-before)*1024
        end
        for _,json in ipairs({false,true}) do
            local s=manager.reload(root,json);local r=s.Entry.get('east',1)
            assert(s.Concrete.get(1).Payload.Delay==7 and r.Name=='中文😀' and r.Signed==math.mininteger and r.Unsigned=='18446744073709551615')
            assert(r.Amount=='79228162514264337593543950335' and r.Optional==R.NULL and r.Values[1]==R.NULL and r.Shape.values[4]==4)
            assert(r.Mapping.get(7)=='值' and r.Lookup==s.Lookup.get(10) and r.Lookup.Peer.Peer==r.Lookup)
            assert(r.Action.Delay==2 and r.Quality=='18446744073709551615' and r.class=='keyword' and #s.Entry.getById(1)==2 and #s.Entry.getKey('中文😀')==2)
            assert(not pcall(function() r.Name='changed' end));assert(not pcall(function() r.Values[1]=1 end))
            for i=1,5 do probe(s) end;collectgarbage('collect');collectgarbage('stop')
            -- 完整 GC 会收缩 Lua 调用帧；先在停止 GC 后恢复查询所需帧，再严格测量稳定路径。
            probe(s);local allocated=probe(s);collectgarbage('restart');assert(allocated==0,'热查询分配：'..allocated)
            local next=manager.reload(root,not json);assert(s.Entry.get('east',1)==r and next~=s)
            assert(not pcall(manager.reload,root..'/missing',json) and manager.current()==next)
            local ok,err=pcall(manager.reload,root..'-broken',json);assert(not ok and err:find('完整性') and manager.current()==next)
            for _,bad in ipairs({'extra','missing','replaced'}) do local ok,err=pcall(manager.reload,root..'-'..bad,json);assert(not ok and err:find('表集合') and manager.current()==next) end
            assert(not pcall(R.parse,'"\\uD800"'));assert(not pcall(R.parse,'{"a":1,"a":2}'))
            assert(R.sha256('abc')=='ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad')
            assert(not pcall(R.decimal,'0.12345678901234567890123456789'))
            print('通过：Lua JSON='..tostring(json)..'，100000 次主键+分组查询分配 '..allocated..' B，引用和旧快照正确。')
        end
        """;
    private const string RustCheck="""
        use std::alloc::{GlobalAlloc,Layout,System};use std::sync::{Arc,atomic::{AtomicUsize,Ordering}};use std::path::Path;
        static ALLOCATIONS:AtomicUsize=AtomicUsize::new(0);
        struct Counter;
        unsafe impl GlobalAlloc for Counter {
            unsafe fn alloc(&self,l:Layout)->*mut u8{ALLOCATIONS.fetch_add(1,Ordering::Relaxed);unsafe{System.alloc(l)}}
            unsafe fn alloc_zeroed(&self,l:Layout)->*mut u8{ALLOCATIONS.fetch_add(1,Ordering::Relaxed);unsafe{System.alloc_zeroed(l)}}
            unsafe fn realloc(&self,p:*mut u8,l:Layout,n:usize)->*mut u8{ALLOCATIONS.fetch_add(1,Ordering::Relaxed);unsafe{System.realloc(p,l,n)}}
            unsafe fn dealloc(&self,p:*mut u8,l:Layout){unsafe{System.dealloc(p,l)}}
        }
        #[global_allocator]static ALLOCATOR:Counter=Counter;
        fn main(){let root=std::env::args().nth(1).unwrap();let root=Path::new(&root);let manager=zrconfig::ConfigManager::new();
            for json in [false,true]{let s=manager.reload(root,json).unwrap();let r=s.get_Entry().get("east",1).unwrap();
                assert_eq!(s.get_Concrete().get(1).unwrap().get_Payload().as_ref().unwrap().get_Delay(),7);
                assert_eq!(r.get_Name(),"中文😀");assert_eq!(r.get_Signed(),i64::MIN);assert_eq!(r.get_Unsigned(),u64::MAX);assert_eq!(r.get_Amount().to_string(),"79228162514264337593543950335");
                assert!(r.get_Optional().is_none()&&r.get_Values()[0].is_none());assert_eq!(r.get_Shape().values()[3],4);assert_eq!(r.get_Mapping().get(&7).map(String::as_str),Some("值"));
                let target=r.get_Lookup().unwrap();assert!(std::ptr::eq(target.as_ref(),s.get_Lookup().get(10).unwrap()));assert!(Arc::ptr_eq(&target,&target.get_Peer().unwrap().get_Peer().unwrap()));
                assert_eq!(r.get_Action().as_ref().unwrap().get_Delay(),2);assert_eq!(r.get_Quality().value(),u64::MAX);assert_eq!(r.get_class(),"keyword");assert_eq!(s.get_Entry().get_ById(1).len(),2);assert_eq!(s.get_Entry().get_Key("中文😀").len(),2);
                let before=ALLOCATIONS.load(Ordering::Relaxed);for _ in 0..100000{std::hint::black_box(s.get_Entry().get("east",1).unwrap().get_Id()+s.get_Entry().get_ById(1).len() as i32);std::hint::black_box(r.get_Lookup());}assert_eq!(ALLOCATIONS.load(Ordering::Relaxed)-before,0);
                let next=manager.reload(root,!json).unwrap();assert!(std::ptr::eq(r,s.get_Entry().get("east",1).unwrap()));assert!(!Arc::ptr_eq(&s,&next));assert!(manager.reload(&root.join("missing"),json).is_err());assert!(manager.reload(Path::new(&format!("{}-broken",root.display())),json).err().unwrap().contains("完整性"));assert!(Arc::ptr_eq(&manager.current().unwrap(),&next));
                for bad in ["extra","missing","replaced"] { assert!(manager.reload(Path::new(&format!("{}-{bad}",root.display())),json).err().unwrap().contains("表集合")); assert!(Arc::ptr_eq(&manager.current().unwrap(),&next)); }
                assert!(zrconfig::runtime::parse_json("\"\\uD800\"").is_err());assert!(zrconfig::runtime::parse_json("{\"a\":1,\"a\":2}").is_err());
                assert!(zrconfig::runtime::Decimal::parse("0.12345678901234567890123456789").is_err());
                println!("通过：Rust JSON={json}，100000 次主键+分组+引用查询，0 次动态分配。");
            }
            let old=manager.reload(root,false).unwrap();let weak=Arc::downgrade(&old.get_Lookup().rows()[0]);manager.reload(root,true).unwrap();drop(old);assert!(weak.upgrade().is_none(),"循环引用阻止旧快照释放");
            std::thread::scope(|scope|{for _ in 0..4{let current=manager.current().unwrap();scope.spawn(move||for _ in 0..10000{assert_eq!(current.get_Entry().get("east",1).unwrap().get_Id(),1);});}manager.reload(root,false).unwrap();});
            println!("通过：Rust 并发读、失败重载与 Weak 循环引用释放。");
        }
        """;
}
