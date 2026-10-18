// Independent server-family consumers. The original portable consumers remain unchanged.
internal static class ServerRuntimeTests
{
    internal static async Task CheckCSharp(ZrConfig.Runtime.ITableCodecRegistry catalog, ZrConfig.Runtime.Server.Catalog serverCatalog, Func<string, ZrConfig.Runtime.ConfigManifest> decode, string output)
    {
        await CheckPreparationCancellationCauses();
        var options = new ZrConfig.Runtime.Server.PreparationOptions();
        var source = new ZrConfig.Runtime.FileConfigSource(output);
        CreateSelectionFixtures(output);
        CreateJsonBudgetFixtures(output);
        var manager = new ZrConfig.Runtime.Server.Manager(catalog, decode);
        void Check(bool ok) { if (!ok) throw new Exception("C# server assertion failed."); }
        void Error(ZrConfig.Runtime.Server.ErrorCode code, Action work)
        { try { work(); } catch (ZrConfig.Runtime.Server.ConfigException error) { Check(error.Code == code); return; } throw new Exception("Expected server error: " + code); }
        Error(ZrConfig.Runtime.Server.ErrorCode.UnsupportedCapability,()=>new ZrConfig.Runtime.Server.Catalog("test",Array.Empty<ZrConfig.Runtime.Server.TableDescriptor>(),"server-v1",new[]{"future-storage"}));
        Error(ZrConfig.Runtime.Server.ErrorCode.NotInitialized, () => manager.AcquireSnapshot());
        using var prepared = manager.Prepare(source, options);
        Check(manager.TryAcquireSnapshot() == null);
        Error(ZrConfig.Runtime.Server.ErrorCode.ReloadBusy, () => manager.Prepare(source));
        using var other = new ZrConfig.Runtime.Server.Manager(catalog, decode);
        Error(ZrConfig.Runtime.Server.ErrorCode.ForeignCandidate, () => other.Commit(prepared));
        var result = manager.Commit(prepared);
        Check(manager.StageTimings.Count==2 && manager.StageTimings.All(t=>t.Succeeded && t.OperationId==result.OperationId && t.ElapsedMilliseconds>=0));
        Check(result.Committed && result.Generation == 1);
        Error(ZrConfig.Runtime.Server.ErrorCode.AlreadyCommitted, () => manager.Commit(prepared));
        object row = result.Snapshot.AsLegacySnapshot().Tables.Single(t => t.Name == "Entry").Rows[0]!;
        object reference = row.GetType().GetProperty("Lookup")!.GetValue(row)!;
        using var pending = manager.Prepare(source);
        manager.Clear();
        Error(ZrConfig.Runtime.Server.ErrorCode.StaleCandidate, () => manager.Commit(pending));
        Check(manager.TryAcquireSnapshot() == null);
        var next = await manager.ReloadAsync(source);
        Check(next.Generation == 3);
        manager.Close(); manager.Close();
        Error(ZrConfig.Runtime.Server.ErrorCode.ManagerClosed, () => manager.AcquireSnapshot());
        Error(ZrConfig.Runtime.Server.ErrorCode.ManagerClosed, () => manager.Prepare(source));
        Check((string)row.GetType().GetProperty("Name")!.GetValue(row)! == "中文😀");
        Check(reference.GetType().GetProperty("Peer")!.GetValue(reference) != null);
        Check(result.Snapshot.AsLegacySnapshot().Tables.Count == 3);

        using var races = new ZrConfig.Runtime.Server.Manager(catalog, decode);
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        async ZrConfig.Runtime.ConfigOperation<byte[]> Read(string path, CancellationToken token)
        { entered.TrySetResult(true); await resume.Task; return source.Read(path, token); }
        var blocked = races.PrepareAsync(new ZrConfig.Runtime.DelegateConfigSource(Read));
        await entered.Task;
        Error(ZrConfig.Runtime.Server.ErrorCode.ReloadBusy, () => races.Prepare(source));
        races.Clear();
        using var fresh = races.Prepare(source);
        long operation = races.GetStatus().PreparingOperation;
        resume.TrySetResult(true);
        try { await blocked; throw new Exception("Late prepare published."); }
        catch (ZrConfig.Runtime.Server.ConfigException error) { Check(error.Code == ZrConfig.Runtime.Server.ErrorCode.StaleCandidate); }
        Check(races.GetStatus().PreparingOperation == operation);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Error(ZrConfig.Runtime.Server.ErrorCode.Cancelled, () => races.Commit(fresh, cancellation.Token));
        races.Commit(fresh); races.Close();
        using(var admission=new ZrConfig.Runtime.Server.Manager(serverCatalog,decode,new ZrConfig.Runtime.Server.ManagerPolicy(maxPendingPreparations:1)))
        {
            var started=new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            async ZrConfig.Runtime.ConfigOperation<byte[]> SlowRead(string path,CancellationToken token){started.TrySetResult(token);await release.Task;return source.Read(path);}
            var oldPreparation=admission.PrepareAsync(new ZrConfig.Runtime.DelegateConfigSource(SlowRead));
            CancellationToken activeToken=await started.Task;
            admission.Clear();Check(activeToken.IsCancellationRequested&&admission.ActivePreparationCount==1);
            Error(ZrConfig.Runtime.Server.ErrorCode.BudgetExceeded,()=>admission.Prepare(source));
            release.TrySetResult(true);
            try{await oldPreparation;throw new Exception("Detached preparation succeeded.");}catch(ZrConfig.Runtime.Server.ConfigException error){Check(error.Code==ZrConfig.Runtime.Server.ErrorCode.StaleCandidate);}
            Check(admission.ActivePreparationCount==0);
            admission.Reload(source);
        }
        using(var boundedManager=new ZrConfig.Runtime.Server.Manager(serverCatalog,decode))
        {
            var bounded=new AsyncOnlyBoundedSource(source);
            var loaded=await boundedManager.ReloadAsync(bounded,new ZrConfig.Runtime.Server.PreparationOptions(selectedTables:["Entry"]));
            Check(bounded.Reads==4 && loaded.Snapshot.LoadedRowCount==4);
            var delayed=new AsyncOnlyBoundedSource(source,delay:true);
            try { await boundedManager.PrepareAsync(delayed,new ZrConfig.Runtime.Server.PreparationOptions(timeout:TimeSpan.FromMilliseconds(25))); throw new Exception("Missing deadline failure."); }
            catch(ZrConfig.Runtime.Server.ConfigException error) { Check(error.Code==ZrConfig.Runtime.Server.ErrorCode.DeadlineExceeded); }
            Check(ReferenceEquals(boundedManager.AcquireSnapshot(),loaded.Snapshot));
        }
        var programFailure=new InvalidOperationException("Manifest decoder programming failure");int attempts=0;
        using(var programmer=new ZrConfig.Runtime.Server.Manager(serverCatalog,text=>++attempts==1?throw programFailure:decode(text)))
        {
            try { programmer.Prepare(source);throw new Exception("Programming exception was swallowed."); }
            catch(InvalidOperationException error) { Check(ReferenceEquals(error,programFailure)); }
            programmer.Reload(source);
        }
        using(var failedRead=new ZrConfig.Runtime.Server.Manager(serverCatalog,decode))
        {
            var ioFailure=new IOException("controlled source failure");
            byte[] FailPayload(string path,CancellationToken token)=>path.EndsWith("current.json", StringComparison.Ordinal)?source.Read(path,token):throw ioFailure;
            try{failedRead.Prepare(new ZrConfig.Runtime.DelegateSyncConfigSource(FailPayload,(path,token)=>ZrConfig.Runtime.ConfigOperation<byte[]>.FromResult(FailPayload(path,token))));throw new Exception("Source read succeeded unexpectedly.");}
            catch(ZrConfig.Runtime.Server.ConfigException error){Check(error.Code==ZrConfig.Runtime.Server.ErrorCode.ReadFailed&&error.Stage=="read"&&error.OperationId>0&&error.VersionId.Length>0&&error.Table.Length>0&&!error.RelativePath.Contains("/")&&ReferenceEquals(error.GetBaseException(),ioFailure));}
        }
        foreach(bool json in new[]{false,true})
        {
            using var containerManager=new ZrConfig.Runtime.Server.Manager(serverCatalog,decode);
            Error(ZrConfig.Runtime.Server.ErrorCode.LimitExceeded,()=>containerManager.Prepare(source,new ZrConfig.Runtime.Server.PreparationOptions(preferJson:json,selectedTables:["Entry"],limits:new ZrConfig.Runtime.Server.ResourceLimits(maxContainerElements:4))));
        }
        foreach(var item in new[]{(Suffix:"string",Table:"Lookup"),(Suffix:"members",Table:"Lookup"),(Suffix:"array",Table:"Entry")})
        {
            using var parserManager=new ZrConfig.Runtime.Server.Manager(serverCatalog,decode);
            var good=parserManager.Reload(source).Snapshot;
            try{parserManager.Prepare(new ZrConfig.Runtime.FileConfigSource(output+"-json-"+item.Suffix),new ZrConfig.Runtime.Server.PreparationOptions(preferJson:true,selectedTables:[item.Table],limits:new ZrConfig.Runtime.Server.ResourceLimits(maxContainerElements:1,maxStringBytes:512)));throw new Exception("JSON allocation budget was not enforced.");}
            catch(ZrConfig.Runtime.Server.ConfigException error){Check(error.Code==ZrConfig.Runtime.Server.ErrorCode.LimitExceeded&&error.Stage=="parse"&&ReferenceEquals(parserManager.AcquireSnapshot(),good));}
        }
        using var selectedManager = new ZrConfig.Runtime.Server.Manager(serverCatalog, decode);
        var reads = new List<string>();
        byte[] Track(string path, CancellationToken token) { reads.Add(path); return source.Read(path, token); }
        var selectedSource = new ZrConfig.Runtime.DelegateSyncConfigSource(Track, (path, token) => ZrConfig.Runtime.ConfigOperation<byte[]>.FromResult(Track(path, token)));
        var selection = serverCatalog.Select(["Entry"]);
        Check(selection.Tables.Select(t => t.Name).SequenceEqual(new[] { "Entry", "Lookup" }));
        var selected = selectedManager.Reload(selectedSource, new ZrConfig.Runtime.Server.PreparationOptions(selectedTables: ["Entry"])).Snapshot;
        Check(selected.LoadedRowCount == 4 && reads.Count == 4 && reads.All(p => !p.Contains("Concrete")));
        Error(ZrConfig.Runtime.Server.ErrorCode.UnsupportedCapability, () => selected.AsLegacySnapshot());
        Error(ZrConfig.Runtime.Server.ErrorCode.TableNotSelected, () => selected.GetTable(serverCatalog.Tables.Single(t => t.Name == "Concrete").GetCodec()));
        Check(selected.GetTable(serverCatalog.Tables.Single(t => t.Name == "Entry").GetCodec()).Rows.Count == 2);
        selectedManager.Reload(new ZrConfig.Runtime.FileConfigSource(output + "-selected"), new ZrConfig.Runtime.Server.PreparationOptions(selectedTables: ["Entry"]));
        Error(ZrConfig.Runtime.Server.ErrorCode.SchemaMismatch, () => selectedManager.Reload(new ZrConfig.Runtime.FileConfigSource(output + "-selection-schema"), new ZrConfig.Runtime.Server.PreparationOptions(selectedTables: ["Entry"])));
        var chain = Enumerable.Range(0, 10000).Select(i => (Name: "N" + i.ToString("D5"), Hash: "hash", Dependencies: i == 9999 ? new[] { "N00000" } : new[] { "N" + (i + 1).ToString("D5") })).ToArray();
        var graph = ZrConfig.Core.RuntimeCatalog.Create(chain);
        Check(graph.Components.Length == 1 && graph.Components[0].Length == 10000);
        Check(graph.Serialize() == ZrConfig.Core.RuntimeCatalog.Create(chain.Reverse()).Serialize());
        using(var observed = new ZrConfig.Runtime.Server.Manager(serverCatalog,decode,eventCapacity:2))
        {
            var events = new List<ZrConfig.Runtime.Server.ManagerEvent>();
            using var listener = observed.Subscribe(value => {
                events.Add(value);
                if(value.Kind == ZrConfig.Runtime.Server.ManagerEventKind.Committed)
                    for(int i=0;i<10;i++) observed.Clear();
            });
            using var faulty = observed.Subscribe(_=>throw new InvalidOperationException("Listener failure is diagnostic only."));
            var committed = observed.Reload(source);
            Check(committed.Committed && committed.Generation == 1 && observed.TryAcquireSnapshot() == null);
            Check(events.Count == 3 && events[0].Generation == 1 && events[1].Generation == 10 && events[2].Generation == 11);
            Check(observed.DroppedEventCount == 8 && observed.ListenerFailureCount == 3);
            listener.Dispose(); faulty.Dispose();
            var current = observed.Reload(source).Snapshot;
            int calls=0;
            using var failures=observed.Subscribe(value=> { if(value.Kind == ZrConfig.Runtime.Server.ManagerEventKind.PreparationFailed) { Check(observed.ActivePreparationCount==0 && !observed.StageTimings[0].Succeeded && observed.StageTimings[0].OperationId==value.OperationId); using var retry=observed.Prepare(source); calls++; } });
            var validator = new ZrConfig.Runtime.Server.DelegateCandidateValidator((candidate,token)=> {
                Check(ReferenceEquals(observed.AcquireSnapshot(),current) && !ReferenceEquals(candidate,current));
                Error(ZrConfig.Runtime.Server.ErrorCode.ReloadBusy,()=>observed.Prepare(source));
                throw new ZrConfig.Runtime.Server.ConfigException(ZrConfig.Runtime.Server.ErrorCode.InvalidValue,"Business validation rejected the candidate.","validate");
            });
            Error(ZrConfig.Runtime.Server.ErrorCode.InvalidValue,()=>observed.Reload(source,new ZrConfig.Runtime.Server.PreparationOptions(validators:[validator])));
            Check(calls == 1 && ReferenceEquals(observed.AcquireSnapshot(),current));
            observed.Reload(source);
        }
        foreach(string kind in new[]{"wrong-target","missing-target"}){using var targetManager=new ZrConfig.Runtime.Server.Manager(serverCatalog,decode);var current=targetManager.Reload(source).Snapshot;Error(ZrConfig.Runtime.Server.ErrorCode.CatalogMismatch,()=>targetManager.Reload(new ZrConfig.Runtime.FileConfigSource(output+"-"+kind)));Check(ReferenceEquals(current,targetManager.AcquireSnapshot()));}
        using(var manifestManager=new ZrConfig.Runtime.Server.Manager(serverCatalog,decode)){try{manifestManager.Prepare(source,new ZrConfig.Runtime.Server.PreparationOptions(limits:new ZrConfig.Runtime.Server.ResourceLimits(maxTables:1)));throw new Exception("Manifest allocation budget accepted.");}catch(ZrConfig.Runtime.Server.ConfigException error){Check(error.Code==ZrConfig.Runtime.Server.ErrorCode.LimitExceeded&&error.Stage=="parse");}}
        foreach(var item in new[]{("array","Values[1]"),("matrix","Shape[2]"),("dictionary","Mapping[0].value"),("polymorphic","Action.Amount")})
        {
            using var fieldsManager=new ZrConfig.Runtime.Server.Manager(serverCatalog,decode);var current=fieldsManager.Reload(source).Snapshot;
            try{fieldsManager.Reload(new ZrConfig.Runtime.FileConfigSource(output+"-json-field-"+item.Item1),new ZrConfig.Runtime.Server.PreparationOptions(preferJson:true));throw new Exception("Malformed nested field accepted.");}
            catch(ZrConfig.Runtime.Server.ConfigException error){Check(error.Code==ZrConfig.Runtime.Server.ErrorCode.InvalidValue&&error.FieldPath==item.Item2&&ReferenceEquals(current,fieldsManager.AcquireSnapshot()));}
        }
        await (Task)typeof(ServerRuntimeTests).GetMethod(nameof(CheckLazyCSharp), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
            .MakeGenericMethod(serverCatalog.Tables.Single(t => t.Name == "Lookup").RowType).Invoke(null, [serverCatalog, decode, output])!;
        await CheckSeededCSharp(serverCatalog, decode, source);
        Console.WriteLine("通过：C# 独立服务器生命周期、旧行/引用保留、异步准备、候选归属和屏障竞态。");
        Console.WriteLine("通过：生成依赖目录、万表环非递归规划、稳定闭包、未选择表零读取与结构化范围拒绝。");
    }

    // Fixed xorshift32 seeds make action selection and corrupt byte offsets
    // reproducible across .NET versions. Barriers choose the interleaving;
    // timeouts only detect hangs and never decide the expected outcome.
    private static async Task CheckSeededCSharp(ZrConfig.Runtime.Server.Catalog catalog,
        Func<string,ZrConfig.Runtime.ConfigManifest> decode,ZrConfig.Runtime.FileConfigSource source)
    {
        foreach(uint seed in new uint[]{1729,104729,0x12345678,0x9e3779b9})
        {
            uint state=seed;
            uint Next(){state^=state<<13;state^=state>>17;state^=state<<5;return state;}
            for(int round=0;round<24;round++)
            {
                int action=(int)(Next()%8);bool json=(Next()&1)!=0;
                string context=$"seed={seed}, round={round}, action={action}, json={json}";
                void Require(bool condition){if(!condition)throw new Exception("Seeded lifecycle: "+context);}
                using var manager=new ZrConfig.Runtime.Server.Manager(catalog,decode);
                var options=new ZrConfig.Runtime.Server.PreparationOptions(preferJson:json,selectedTables:["Entry"]);
                var original=manager.Reload(source,options).Snapshot;
                var codec=catalog.Tables.Single(table=>table.Name=="Entry").GetCodec();
                object first=original.GetTable(codec).Rows[0]!;
                var entered=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var release=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                using var cancellation=new CancellationTokenSource();
                async ZrConfig.Runtime.ConfigOperation<byte[]> Read(string path,CancellationToken token)
                {entered.TrySetResult(true);await release.Task;return source.Read(path,token);}
                var pending=manager.PrepareAsync(new ZrConfig.Runtime.DelegateConfigSource(Read),options,cancellation.Token);
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
                var readers=Task.Run(()=>{for(int i=0;i<2048;i++)Require(ReferenceEquals(first,original.GetTable(codec).Rows[0]));});
                if(action==1)cancellation.Cancel();
                if(action==3)manager.Clear();
                if(action==4)manager.Close();
                release.TrySetResult(true);
                try
                {
                    using var candidate=await pending;
                    Require(action!=1&&action!=3&&action!=4);
                    if(action==2)cancellation.Cancel();
                    if(action==5)candidate.Dispose();
                    if(action==6)manager.Clear();
                    try
                    {
                        var committed=manager.Commit(candidate,cancellation.Token);
                        Require(action==0||action==7);
                        if(action==7)cancellation.Cancel();
                        Require(committed.Committed&&committed.Generation==2&&ReferenceEquals(manager.AcquireSnapshot(),committed.Snapshot));
                    }
                    catch(ZrConfig.Runtime.Server.ConfigException error)
                    {
                        Require(action==2?error.Code==ZrConfig.Runtime.Server.ErrorCode.Cancelled:
                            (action==5||action==6)&&error.Code==ZrConfig.Runtime.Server.ErrorCode.StaleCandidate);
                    }
                }
                catch(ZrConfig.Runtime.Server.ConfigException error)
                {
                    Require(error.Code==(action==1?ZrConfig.Runtime.Server.ErrorCode.Cancelled:
                        action==3?ZrConfig.Runtime.Server.ErrorCode.StaleCandidate:ZrConfig.Runtime.Server.ErrorCode.ManagerClosed));
                    Require(action==1||action==3||action==4);
                }
                await readers.WaitAsync(TimeSpan.FromSeconds(10));
                Require(manager.ActivePreparationCount==0&&ReferenceEquals(first,original.GetTable(codec).Rows[0]));
                if(action is 1 or 2 or 5)Require(ReferenceEquals(manager.AcquireSnapshot(),original)&&manager.GetStatus().Generation==1);
                if(action is 3 or 6)Require(manager.TryAcquireSnapshot()==null&&manager.GetStatus().Generation==2);
                if(action==4)Require(manager.GetStatus().Phase==ZrConfig.Runtime.Server.ManagerPhase.Closed);
                if(action!=4){using var retry=manager.Prepare(source,options);manager.Commit(retry);}
            }
            foreach(bool json in new[]{false,true})
            {
                using var manager=new ZrConfig.Runtime.Server.Manager(catalog,decode);
                var options=new ZrConfig.Runtime.Server.PreparationOptions(preferJson:json,selectedTables:["Entry"]);
                var original=manager.Reload(source,options).Snapshot;
                for(int round=0;round<32;round++)
                {
                    int read=0,offset=-1;uint position=Next();byte mask=(byte)(1<<(int)(Next()%8));
                    byte[] Read(string path,CancellationToken token)
                    {
                        byte[] bytes=source.Read(path,token);
                        if(++read==2){offset=(int)(position%(uint)bytes.Length);bytes[offset]^=mask;}
                        return bytes;
                    }
                    var corrupt=new ZrConfig.Runtime.DelegateSyncConfigSource(Read,(path,token)=>ZrConfig.Runtime.ConfigOperation<byte[]>.FromResult(Read(path,token)));
                    try{using var candidate=manager.Prepare(corrupt,options);throw new Exception($"Corruption accepted: seed={seed}, round={round}, json={json}, offset={offset}");}
                    catch(ZrConfig.Runtime.Server.ConfigException error)
                    {
                        if(error.Code!=ZrConfig.Runtime.Server.ErrorCode.IntegrityMismatch||manager.ActivePreparationCount!=0||!ReferenceEquals(manager.AcquireSnapshot(),original))
                            throw new Exception($"Corruption rollback: seed={seed}, round={round}, json={json}, offset={offset}",error);
                    }
                }
                manager.Reload(source,options);
            }
        }
        Console.WriteLine("通过：四个固定种子、96 轮屏障交错/旧快照读者及 256 次双格式随机位损坏，失败输出种子与轮次。");
    }

    private static async Task CheckLazyCSharp<T>(ZrConfig.Runtime.Server.Catalog catalog, Func<string, ZrConfig.Runtime.ConfigManifest> decode, string output) where T : class, ZrConfig.Runtime.IConfigKeyRow
    {
        using var manager = new ZrConfig.Runtime.Server.Manager(catalog, decode);
        var codec = catalog.Tables.Single(t => t.Name == "Lookup").GetCodec();
        void Check(bool value) { if (!value) throw new Exception("C# lazy server assertion failed."); }
        void Code(ZrConfig.Runtime.Server.ErrorCode expected, Action action)
        { try { action(); } catch (ZrConfig.Runtime.Server.ConfigException error) when (error.Code == expected) { return; } throw new Exception("Expected lazy server error: " + expected); }
        foreach (bool json in new[] { false, true }) foreach (var mode in new[] { ZrConfig.Runtime.Server.LoadMode.LazyRowsStrict, ZrConfig.Runtime.Server.LoadMode.LazyRowsOnAccess })
        {
            int reads = 0;
            var source = new ZrConfig.Runtime.FileConfigSource(output);
            byte[] Read(string path, CancellationToken token) { reads++; return source.Read(path, token); }
            var snapshot = manager.Reload(new ZrConfig.Runtime.DelegateSyncConfigSource(Read, (p,t) => ZrConfig.Runtime.ConfigOperation<byte[]>.FromResult(Read(p,t))),
                new ZrConfig.Runtime.Server.PreparationOptions(preferJson: json, selectedTables: ["Lookup"], mode: mode, enableDiagnostics:true)).Snapshot;
            Check(snapshot.CompletedTableCount == 0 && snapshot.LoadedRowCount == 0 && reads == 3 && snapshot.Diagnostics.OpenedTableCount == 1);
            Code(ZrConfig.Runtime.Server.ErrorCode.RowNotLoaded, () => snapshot.TryGetRow<T,int>(codec,10));
            Code(ZrConfig.Runtime.Server.ErrorCode.TableNotLoaded, () => snapshot.GetTable(codec));
            Check(snapshot.TryGetRow<T,int>(codec,999) == null && snapshot.LoadedRowCount == 0);
            var requests = Enumerable.Range(0,32).Select(_ => Task.Run(() => snapshot.LoadRow<T,int>(codec,10))).ToArray();
            var rows = await Task.WhenAll(requests);
            var row = rows[0];
            Check(rows.All(r => ReferenceEquals(r,row)) && snapshot.LoadedRowCount == 1);
            Code(ZrConfig.Runtime.Server.ErrorCode.RowNotLoaded, () => row.GetReference<T>(codec,20,"Peer"));
            Code(ZrConfig.Runtime.Server.ErrorCode.MissingReference, () => row.GetReference<T>(codec,999,"Peer"));
            Check(row.GetReference<T>(codec,0,"Peer") == null && snapshot.LoadedRowCount == 1);
            var peer = row.LoadReference<T>(codec,20,"Peer")!;
            Check(ReferenceEquals(peer.GetReference<T>(codec,10,"Peer"), row) && snapshot.LoadedRowCount == 2);
            var table = snapshot.LoadTable(codec);
            Check(table.Rows.Cast<object>().Any(r => ReferenceEquals(r,row.Borrow())) && snapshot.CompletedTableCount == 1 && snapshot.LoadedRowCount == 2 && reads == 3);
            snapshot.LoadAll(); manager.Clear();
            Check(ReferenceEquals(snapshot.GetRow<T,int>(codec,10),row) && ReferenceEquals(row.GetReference<T>(codec,20,"Peer"),peer));
        }
        var scheduler = new GateScheduler();
        var lazy = manager.Reload(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(selectedTables:["Lookup"], mode:ZrConfig.Runtime.Server.LoadMode.LazyRowsStrict,scheduler:scheduler)).Snapshot;
        using var cancel = new CancellationTokenSource();
        var first = lazy.LoadRowAsync<T,int>(codec,10,cancel.Token);
        await scheduler.Entered.Task;
        var second = lazy.LoadRowAsync<T,int>(codec,10);
        cancel.Cancel();
        try { await first; throw new Exception("Cancelled row waiter succeeded."); } catch(ZrConfig.Runtime.Server.ConfigException error) when(error.Code==ZrConfig.Runtime.Server.ErrorCode.Cancelled) { }
        Check(lazy.LoadedRowCount == 0 && scheduler.Count == 1);
        scheduler.Resume.TrySetResult(true);
        var ready = await second;
        Check(lazy.LoadedRowCount == 1 && ReferenceEquals(ready,lazy.GetRow<T,int>(codec,10)) && scheduler.Count == 1);
        Check(lazy.MergedRowRequests == 1);
        Code(ZrConfig.Runtime.Server.ErrorCode.BudgetExceeded,()=>manager.Prepare(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(selectedTables:["Lookup"],mode:ZrConfig.Runtime.Server.LoadMode.LazyRowsStrict,limits:new ZrConfig.Runtime.Server.ResourceLimits(maxEstimatedDecodedBytes:1))));
        var groupDescriptor = new ZrConfig.Runtime.Server.GroupDescriptor<T,int>("BudgetGroup",()=>codec,_=>1);
        var indexCapped = manager.Reload(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(selectedTables:["Lookup"],mode:ZrConfig.Runtime.Server.LoadMode.LazyRowsOnAccess,limits:new ZrConfig.Runtime.Server.ResourceLimits().WithIndexBudget(1))).Snapshot;
        long baseIndexes=indexCapped.EstimatedIndexBytes;
        for(int i=0;i<2;i++)Code(ZrConfig.Runtime.Server.ErrorCode.BudgetExceeded,()=>indexCapped.LoadGroup(groupDescriptor,1));
        Check(indexCapped.EstimatedIndexBytes==baseIndexes&&indexCapped.LoadedRowCount==0&&indexCapped.FaultedIndexCount==0);
        bool failGroup=true;var retryGroup=new ZrConfig.Runtime.Server.GroupDescriptor<T,int>("RetryGroup",()=>codec,_=>failGroup?throw new ZrConfig.Runtime.Server.ConfigException(ZrConfig.Runtime.Server.ErrorCode.InvalidPlan,"Injected host failure"):1);
        var indexRetry=manager.Reload(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(selectedTables:["Lookup"],mode:ZrConfig.Runtime.Server.LoadMode.LazyRowsOnAccess)).Snapshot;
        baseIndexes=indexRetry.EstimatedIndexBytes;Code(ZrConfig.Runtime.Server.ErrorCode.InvalidPlan,()=>indexRetry.LoadGroup(retryGroup,1));Check(indexRetry.EstimatedIndexBytes==baseIndexes);failGroup=false;
        Check(indexRetry.LoadGroup(retryGroup,1).Count==2&&indexRetry.EstimatedIndexBytes>baseIndexes);long retainedIndexes=indexRetry.EstimatedIndexBytes;indexRetry.LoadGroup(retryGroup,1);Check(indexRetry.EstimatedIndexBytes==retainedIndexes);
        var capped = manager.Reload(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(selectedTables:["Lookup"],mode:ZrConfig.Runtime.Server.LoadMode.LazyRowsOnAccess,limits:new ZrConfig.Runtime.Server.ResourceLimits(maxDecodedRows:1))).Snapshot;
        var retained = capped.LoadRow<T,int>(codec,10);
        Code(ZrConfig.Runtime.Server.ErrorCode.BudgetExceeded,()=>capped.LoadRow<T,int>(codec,20));
        Code(ZrConfig.Runtime.Server.ErrorCode.BudgetExceeded,()=>capped.LoadRow<T,int>(codec,20));
        Check(capped.LoadedRowCount == 1 && capped.Health == ZrConfig.Runtime.Server.SnapshotHealth.Healthy && ReferenceEquals(retained,capped.GetRow<T,int>(codec,10)));
        Code(ZrConfig.Runtime.Server.ErrorCode.BudgetExceeded,()=>manager.Reload(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(limits:new ZrConfig.Runtime.Server.ResourceLimits(maxDecodedRows:1))));
        Check(ReferenceEquals(manager.AcquireSnapshot(),capped));
        Code(ZrConfig.Runtime.Server.ErrorCode.LimitExceeded,()=>manager.Prepare(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(maxManifestBytes:1)));
        Code(ZrConfig.Runtime.Server.ErrorCode.LimitExceeded,()=>manager.Prepare(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(limits:new ZrConfig.Runtime.Server.ResourceLimits(maxTables:1))));
        Code(ZrConfig.Runtime.Server.ErrorCode.LimitExceeded,()=>manager.Prepare(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(limits:new ZrConfig.Runtime.Server.ResourceLimits(maxRows:1))));
        Code(ZrConfig.Runtime.Server.ErrorCode.LimitExceeded,()=>manager.Prepare(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(limits:new ZrConfig.Runtime.Server.ResourceLimits(maxContainerElements:1))));
        Code(ZrConfig.Runtime.Server.ErrorCode.LimitExceeded,()=>manager.Prepare(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(selectedTables:["Entry"],limits:new ZrConfig.Runtime.Server.ResourceLimits(maxRows:3))));
        using (var admission = new ZrConfig.Runtime.Server.Manager(catalog,decode))
        {
            var oldSnapshots=new List<ZrConfig.Runtime.Server.Snapshot>();
            for(int i=0;i<10;i++)oldSnapshots.Add(admission.Reload(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(selectedTables:["Entry"],mode:ZrConfig.Runtime.Server.LoadMode.LazyRowsOnAccess)).Snapshot);
            byte[] manifestBytes=File.ReadAllBytes(Path.Combine(output,"current.json"));var manifest=decode(System.Text.Encoding.UTF8.GetString(manifestBytes));
            string lookupFile=manifest.tables.Single(entry=>entry.name=="Lookup").file;
            long allowance=oldSnapshots.Sum(snapshot=>snapshot.EstimatedRetainedBytes)+4*(manifestBytes.LongLength+new FileInfo(Path.Combine(output,lookupFile)).Length)+1000;
            using var prepared=admission.Prepare(new ZrConfig.Runtime.FileConfigSource(output),new ZrConfig.Runtime.Server.PreparationOptions(selectedTables:["Lookup"],mode:ZrConfig.Runtime.Server.LoadMode.LazyRowsOnAccess,limits:new ZrConfig.Runtime.Server.ResourceLimits(maxEstimatedLiveBytes:allowance)));
            foreach(var snapshot in oldSnapshots)snapshot.LoadAll();
            Code(ZrConfig.Runtime.Server.ErrorCode.BudgetExceeded,()=>admission.Commit(prepared));
            Check(ReferenceEquals(admission.AcquireSnapshot(),oldSnapshots[^1])&&admission.GetStatus().Generation==10);
        }
        manager.Reload(new ZrConfig.Runtime.FileConfigSource(output));
        string corrupt = CreateCorruptRowFixture(output);
        using var broken = new ZrConfig.Runtime.Server.Manager(catalog,decode);
        Code(ZrConfig.Runtime.Server.ErrorCode.InvalidEncoding,()=>broken.Prepare(new ZrConfig.Runtime.FileConfigSource(corrupt),new ZrConfig.Runtime.Server.PreparationOptions(selectedTables:["Lookup"],mode:ZrConfig.Runtime.Server.LoadMode.LazyRowsStrict)));
        var degraded = broken.Reload(new ZrConfig.Runtime.FileConfigSource(corrupt),new ZrConfig.Runtime.Server.PreparationOptions(selectedTables:["Lookup"],mode:ZrConfig.Runtime.Server.LoadMode.LazyRowsOnAccess)).Snapshot;
        for(int attempt=0;attempt<2;attempt++) Code(ZrConfig.Runtime.Server.ErrorCode.InvalidEncoding,()=>degraded.LoadRow<T,int>(codec,10));
        try{degraded.LoadRow<T,int>(codec,10);throw new Exception("Missing cached error");}catch(ZrConfig.Runtime.Server.ConfigException error){Check(error.Table=="Lookup"&&error.VersionId.Length>0&&error.RowKey.Length>0&&error.FieldPath=="Name");}
        Check(degraded.Health == ZrConfig.Runtime.Server.SnapshotHealth.Degraded && degraded.LoadedRowCount == 0);
        Check(degraded.LoadRow<T,int>(codec,20).Borrow().ConfigId == 20 && degraded.LoadedRowCount == 1);
        Code(ZrConfig.Runtime.Server.ErrorCode.InvalidEncoding,()=>degraded.GetRow<T,int>(codec,10));
        Console.WriteLine("通过：C# strict/on-access 惰性加载、零驻留行准备、缓存身份、引用环、32 路合并和等待取消隔离。");
    }

    private sealed class GateScheduler : ZrConfig.Runtime.IConfigLoadScheduler
    {
        internal readonly TaskCompletionSource<bool> Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource<bool> Resume = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal int Count;
        public async ZrConfig.Runtime.ConfigOperation<T> Execute<T>(Func<T> work,CancellationToken token)
        { Interlocked.Increment(ref Count); Entered.TrySetResult(true); await Resume.Task; token.ThrowIfCancellationRequested(); return work(); }
    }

    private static async Task CheckPreparationCancellationCauses()
    {
        // Model the timer becoming observable before the monotonic checkpoint.
        // Reflection keeps the clock-boundary fixture out of the public runtime API.
        var type=typeof(ZrConfig.Runtime.Server.Manager).Assembly.GetType("ZrConfig.Runtime.Server.PreparationCancellation",throwOnError:true)!;
        const System.Reflection.BindingFlags flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        var constructor=type.GetConstructor(flags,null,[typeof(CancellationToken),typeof(TimeSpan),typeof(CancellationToken)],null)!;
        var tokenProperty=type.GetProperty("Token",flags)!;var classify=type.GetMethod("Classify",flags)!;
        foreach(string cause in new[]{"timer","caller","manager"})
        {
            using var caller=new CancellationTokenSource();using var manager=new CancellationTokenSource();
            using var lifetime=(IDisposable)constructor.Invoke([caller.Token,cause=="timer"?TimeSpan.FromMilliseconds(5):TimeSpan.FromMinutes(1),manager.Token]);
            var token=(CancellationToken)tokenProperty.GetValue(lifetime)!;
            if(cause=="caller")caller.Cancel();if(cause=="manager")manager.Cancel();
            try{await Task.Delay(Timeout.Infinite,token).WaitAsync(TimeSpan.FromSeconds(10));throw new Exception("Cancellation did not arrive.");}
            catch(OperationCanceledException){ }
            var original=new OperationCanceledException(token);var result=(Exception)classify.Invoke(lifetime,[original,long.MaxValue])!;
            if(cause=="timer")
            {
                if(result is not ZrConfig.Runtime.Server.ConfigException error||error.Code!=ZrConfig.Runtime.Server.ErrorCode.DeadlineExceeded||!ReferenceEquals(error.InnerException,original))
                    throw new Exception("Timer cancellation lost its deadline cause before the monotonic checkpoint.");
            }
            else if(!ReferenceEquals(result,original))throw new Exception("Caller/manager cancellation was classified as a timer deadline.");
        }
        Console.WriteLine("通过：计时器、调用者与管理器取消原因分别保留，计时器先于单调检查点也报告超时。");
    }

    private sealed class AsyncOnlyBoundedSource : ZrConfig.Runtime.Server.IBoundedAsyncConfigSource
    {
        private readonly ZrConfig.Runtime.FileConfigSource _source;
        private readonly bool _delay;
        internal int Reads;
        internal AsyncOnlyBoundedSource(ZrConfig.Runtime.FileConfigSource source,bool delay=false){_source=source;_delay=delay;}
        public ZrConfig.Runtime.ConfigOperation<byte[]> ReadAsync(string path,CancellationToken token=default) => throw new Exception("Unbounded source fallback must not be used.");
        public async ZrConfig.Runtime.ConfigOperation<byte[]> ReadBoundedAsync(string path,long maxBytes,CancellationToken token=default)
        {
            Reads++;
            if(_delay) await Task.Delay(Timeout.Infinite,token);
            return await _source.ReadBoundedAsync(path,maxBytes,token);
        }
    }

    private static string CreateCorruptRowFixture(string output)
    {
        string root = output + "-bad-row";
        var manifest=System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(output,"current.json")))!;
        string version=manifest["versionId"]!.GetValue<string>();
        string directory=root; Directory.CreateDirectory(directory);
        var entry=manifest["tables"]!.AsArray().Single(t=>t!["name"]!.GetValue<string>()=="Lookup")!;
        string file=entry["file"]!.GetValue<string>();
        byte[] bytes=File.ReadAllBytes(Path.Combine(output,file));
        using(var input=new MemoryStream(bytes)) using(var reader=new BinaryReader(input))
        {
            input.Position=8;
            for(int i=0;i<2;i++) { int length=reader.ReadInt32(); input.Position += length; }
            int count=reader.ReadInt32(), tags=reader.ReadInt32();
            for(int i=0;i<tags;i++) { int length=reader.ReadInt32(); input.Position += length; }
            input.Position += count*16;
            // Lookup payload: Id(int32), Name byte count(int32), UTF-8 Name, Peer(int32).
            bytes[checked((int)input.Position+8)]=0xff;
        }
        File.WriteAllBytes(Path.Combine(directory,file),bytes);
        entry["sha256"]=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
        File.WriteAllText(Path.Combine(root,"current.json"),manifest.ToJsonString());
        return root;
    }

    private static void CreateJsonBudgetFixtures(string output)
    {
        foreach(string kind in new[]{"string","members","array","field-array","field-matrix","field-dictionary","field-polymorphic"})
        {
            var manifest=System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllBytes(Path.Combine(output,"current.json")))!;
            string version=manifest["versionId"]!.GetValue<string>(),table=kind=="array"||kind.StartsWith("field-",StringComparison.Ordinal)?"Entry":"Lookup",destination=output+"-json-"+kind;
            string folder=destination;Directory.CreateDirectory(folder);
            foreach(string file in Directory.GetFiles(output).Where(p => Path.GetExtension(p) is ".bytes" or ".json"))File.Copy(file,Path.Combine(folder,Path.GetFileName(file)),true);
            var entry=manifest["tables"]!.AsArray().Single(value=>value!["name"]!.GetValue<string>()==table)!;
            string path=Path.Combine(folder,entry["jsonFile"]!.GetValue<string>());
            var document=System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllBytes(path))!;
            var row=document["rows"]![0]!;
            if(kind=="string")row["Name"]=new string('x',1_000_000);
            else if(kind=="members")for(int i=0;i<10000;i++)row["unknown"+i]=i;
            else if(kind=="field-array")row["Values"]![1]="bad";
            else if(kind=="field-matrix")row["Shape"]!["values"]![2]="bad";
            else if(kind=="field-dictionary")row["Mapping"]![0]![1]=22;
            else if(kind=="field-polymorphic")row["Action"]!["Amount"]="bad";
            else row["Values"]=new System.Text.Json.Nodes.JsonArray(Enumerable.Range(0,100000).Select(i=>(System.Text.Json.Nodes.JsonNode?)System.Text.Json.Nodes.JsonValue.Create(i)).ToArray());
            byte[] bytes=System.Text.Encoding.UTF8.GetBytes(document.ToJsonString());File.WriteAllBytes(path,bytes);
            entry["jsonSha256"]=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
            File.WriteAllText(Path.Combine(destination,"current.json"),manifest.ToJsonString());
        }
    }

    private static void CreateSelectionFixtures(string output)
    {
        var manifest = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(output, "current.json")))!;
        foreach(string kind in new[]{"wrong-target","missing-target"}){var changed=manifest.DeepClone();if(kind=="wrong-target")changed["target"]="C";else changed.AsObject().Remove("target");Directory.CreateDirectory(output+"-"+kind);File.WriteAllText(Path.Combine(output+"-"+kind,"current.json"),changed.ToJsonString());}
        string version = manifest["versionId"]!.GetValue<string>();
        string target = output + "-selected";
        Directory.CreateDirectory(target);
        File.Copy(Path.Combine(output,"current.json"),Path.Combine(output+"-selected","current.json"),true);
        foreach (var table in manifest["tables"]!.AsArray())
        {
            if (table!["name"]!.GetValue<string>() == "Concrete") continue;
            foreach (string field in new[] { "file", "jsonFile" })
            {
                string name = table[field]!.GetValue<string>();
                File.Copy(Path.Combine(output,name),Path.Combine(target,name),true);
            }
        }
        string corrupt=output+"-missing-ref";
        string corruptFolder=corrupt; Directory.CreateDirectory(corruptFolder);
        var bad=manifest.DeepClone();
        foreach(var entry in bad["tables"]!.AsArray())
        {
            if(entry!["name"]!.GetValue<string>()=="Concrete") continue;
            string file=entry["jsonFile"]!.GetValue<string>();
            byte[] bytes=File.ReadAllBytes(Path.Combine(output,file));
            if(entry["name"]!.GetValue<string>()=="Entry")
            {
                var doc=System.Text.Json.Nodes.JsonNode.Parse(bytes)!;
                doc["rows"]!.AsArray()[0]!["Lookup"]=99999;
                bytes=System.Text.Encoding.UTF8.GetBytes(doc.ToJsonString());
                entry["jsonSha256"]=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
            }
            File.WriteAllBytes(Path.Combine(corruptFolder,file),bytes);
        }
        File.WriteAllText(Path.Combine(corrupt,"current.json"),bad.ToJsonString());
        string large=output+"-large-ids",largeFolder=large;Directory.CreateDirectory(largeFolder);
        var largeManifest=manifest.DeepClone();var largeEntry=largeManifest["tables"]!.AsArray().Single(t=>t!["name"]!.GetValue<string>()=="Lookup")!;
        string largeFile=largeEntry["jsonFile"]!.GetValue<string>();var largeDocument=System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllBytes(Path.Combine(output,largeFile)))!;
        foreach(var row in largeDocument["rows"]!.AsArray()){row!["Id"]=row["Id"]!.GetValue<int>()+1000;row["Peer"]=row["Peer"]!.GetValue<int>()+1000;}
        byte[] largeBytes=System.Text.Encoding.UTF8.GetBytes(largeDocument.ToJsonString());File.WriteAllBytes(Path.Combine(largeFolder,largeFile),largeBytes);
        largeEntry["jsonSha256"]=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(largeBytes));File.WriteAllText(Path.Combine(large,"current.json"),largeManifest.ToJsonString());
        manifest["tables"]!.AsArray().Single(t => t!["name"]!.GetValue<string>() == "Concrete")!["schemaHash"] = new string('0',64);
        Directory.CreateDirectory(output+"-selection-schema");
        File.WriteAllText(Path.Combine(output+"-selection-schema","current.json"),manifest.ToJsonString());
    }

    internal const string CSharp = """
using System;
using System.Text.Json;
using ZrConfig.Models.Server;
using ZrConfig.Runtime;
using ZrConfig.Runtime.Server;
public static class ServerConsumer {
    static void Check(bool value) { if (!value) throw new Exception("C# typed server consumer assertion failed."); }
    static void Code(ErrorCode code, Action work) { try { work(); } catch(ConfigException e) when(e.Code == code) { return; } throw new Exception("Expected " + code); }
    public static void Run(string root) {
        ConfigManifest Decode(string json) => JsonSerializer.Deserialize<ConfigManifest>(json, new JsonSerializerOptions { IncludeFields = true })!;
        using var manager = new Manager(ZrConfig.Models.ServerCatalog.Instance, Decode);
        foreach (bool json in new[]{false,true}) foreach (var mode in new[]{LoadMode.ServerDefault,LoadMode.SelectedTables,LoadMode.LazyRowsStrict,LoadMode.LazyRowsOnAccess}) {
            var snapshot = manager.Reload(new FileConfigSource(root),new PreparationOptions(preferJson:json,selectedTables:new[]{"Entry"},mode:mode)).Snapshot;
            Check(snapshot.CatalogFingerprint.Length==64);if(mode==LoadMode.LazyRowsOnAccess)Check(snapshot.LoadPlanFingerprint==(json?"c55af6527720622add35286774df8dd1cf6e8eda7537d04aa1ee2eef94f31d91":"a3fe29f4e2fb7a9cc160a8a474905d5c40bb7f57179f9651f08a8967455785d1"));
            var table = snapshot.GetEntryTable(); bool lazy = mode == LoadMode.LazyRowsStrict || mode == LoadMode.LazyRowsOnAccess;
            Check(table.Count == 2 && snapshot.LoadedRowCount == (lazy ? 0 : 4));
            if(lazy) { Code(ErrorCode.RowNotLoaded,()=>table.TryGet("east",1)); Code(ErrorCode.TableNotLoaded,()=>table.FindByZone("east")); }
            Check(table.TryGet("missing",1) == null);
            var group = table.LoadByZone("east"); Check(group.Count == 1 && snapshot.LoadedRowCount == (lazy ? 1 : 4));
            var row = table.Get("east",1); Check(ReferenceEquals(row.Borrow(),group[0].Borrow()) && row.Name == "中文😀" && row.LookupId == 10);
            Check(row.Values.Count == 3 && row.Values[0] == null && row.Values[2] == -2 && row.Mapping[7] == "值" && row.Shape[1,1] == 4 && row.Action!.Value.Delay == 2);
            int entries=0; foreach(var item in row.Mapping) { Check(item.Key == 7 && item.Value == "值"); entries++; } Check(entries == 1);
            if(lazy) Code(ErrorCode.RowNotLoaded,()=>row.GetLookup());
            var lookup = row.LoadLookup()!.Value; var peer = lookup.LoadPeer()!.Value;
            Check(ReferenceEquals(peer.GetPeer()!.Value.Borrow(),lookup.Borrow()));
            Check(table.FindByZone("absent").Count == 0);
            var all = table.LoadAll(); Check(all.Count == 2 && snapshot.LoadedRowCount == 4 && table.Rows.Count == 2);
            int ReadHot() => table.Get("east",1).Id + table.FindByZone("east")[0].Id + row.GetLookup()!.Value.Id + row.Shape[1,1] + row.Values[2]!.Value + row.Action!.Value.Delay + (table.TryGet("absent",1).HasValue ? 1 : 0);
            for(int i=0;i<100000;i++) Check(ReadHot()==16);
            long before = GC.GetAllocatedBytesForCurrentThread(); long sum = 0;
            for(int i=0;i<100000;i++) sum += ReadHot();
            long allocated=GC.GetAllocatedBytesForCurrentThread()-before;
            if(sum!=1600000||allocated!=0)throw new Exception("C# server hot query: sum="+sum+", allocated="+allocated+", JSON="+json+", mode="+mode);
            manager.Clear(); Check(group[0].GetLookup()!.Value.Name == "目标");
        }
        var warm = manager.Reload(new FileConfigSource(root),new PreparationOptions(selectedTables:new[]{"Entry"},mode:LoadMode.LazyRowsStrict,warmupTables:new[]{"Entry"})).Snapshot;
        Check(warm.LoadedRowCount == 2 && warm.GetEntryTable().FindByZone("east").Count == 1);
        var sparse = manager.Reload(new FileConfigSource(root),new PreparationOptions(selectedTables:new[]{"Entry"},mode:LoadMode.LazyRowsStrict,
            warmup:new WarmupPlan(new[]{EntryTable.WarmupByZone("east"),LookupTable.WarmupRow(10)}))).Snapshot;
        Check(sparse.LoadedRowCount == 2 && sparse.GetEntryTable().Get("east",1).GetLookup()!.Value.Id == 10);
        int reads=0; byte[] Read(string path,System.Threading.CancellationToken token) { reads++; throw new Exception("Plan validation must precede I/O."); }
        var unavailable=new DelegateSyncConfigSource(Read,(p,t)=>ConfigOperation<byte[]>.FromResult(Read(p,t)));
        Code(ErrorCode.InvalidPlan,()=>manager.Prepare(unavailable,new PreparationOptions(selectedTables:new[]{"Missing"})));
        Code(ErrorCode.InvalidPlan,()=>manager.Prepare(unavailable,new PreparationOptions(selectedTables:new[]{"Lookup"},warmupTables:new[]{"Entry"})));
        Check(reads == 0);
        Console.WriteLine("通过：C# 强类型业务 API、分组按命中行加载、整表身份复用、预热、无 I/O 计划拒绝及热路径零分配。");
    }
}
""";

    internal const string Java = """
        import zrconfig.*;
        import static zrconfig.ConfigRuntime.*;
        import java.nio.file.*;
        import java.util.concurrent.*;
        public class ServerCheck {
          static void check(boolean value){if(!value)throw new AssertionError("server assertion failed");}
          interface Work { void run() throws Exception; }
          static void code(ErrorCode code,Work work)throws Exception{try{work.run();}catch(ConfigException e){check(e.code==code);return;}throw new AssertionError("expected "+code);}
          public static void main(String[] args)throws Exception {
            var root=Path.of(args[0]);var token=new Cancellation();var options=PreparationOptions.defaults();
            {
              for(boolean json:new boolean[]{false,true}){var plan=new LoadPlan(LoadMode.LazyRowsOnAccess,java.util.List.of("Entry"),json,java.util.List.of());
                var steps=java.util.List.of(DataSnapshot.warmupRow_Lookup(10),DataSnapshot.warmupGroup_Entry_ById(1),DataSnapshot.warmupRow_Lookup(10));
                var warmed=DataSnapshot.loadWithWarmup(root,plan,steps,token);check(warmed.loadedRowCount()==3);check(warmed.get_Entry().get("east",1).get_Lookup()==warmed.get_Lookup().get(10));code(ErrorCode.RowNotLoaded,()->warmed.get_Lookup().get(20));
              }
              int[] reads={0};ConfigSource source=(name,maximum,cancellation)->{reads[0]++;throw new java.io.IOException("must not read");};
              code(ErrorCode.InvalidPlan,()->DataSnapshot.loadWithWarmup(source,new LoadPlan(LoadMode.LazyRowsOnAccess,java.util.List.of("Lookup"),false,java.util.List.of()),java.util.List.of(DataSnapshot.warmupRow_Entry("east",1)),token));check(reads[0]==0);
            }
            for(boolean json:new boolean[]{false,true}) for(LoadMode mode:LoadMode.values()) {
              var snapshot=DataSnapshot.load(Path.of(args[0]+"-selected"),new LoadPlan(mode,java.util.List.of("Entry"),json,java.util.List.of()),token);
              check(snapshot.catalogFingerprint().length()==64);if(mode==LoadMode.LazyRowsOnAccess)check(snapshot.loadPlanFingerprint().equals(json?"c55af6527720622add35286774df8dd1cf6e8eda7537d04aa1ee2eef94f31d91":"a3fe29f4e2fb7a9cc160a8a474905d5c40bb7f57179f9651f08a8967455785d1"));
              var entry=snapshot.get_Entry();var lookup=snapshot.get_Lookup();code(ErrorCode.TableNotSelected,()->snapshot.get_Concrete());
              if(mode==LoadMode.LazyRowsStrict||mode==LoadMode.LazyRowsOnAccess) {
                check(snapshot.completedTableCount()==0&&snapshot.loadedRowCount()==0&&entry.find("missing",99)==null);
                code(ErrorCode.RowNotLoaded,()->entry.find("east",1));code(ErrorCode.TableNotLoaded,()->entry.rows());
                var row=entry.load("east",1,token);check(snapshot.loadedRowCount()==1&&row.getLookupId()==10);
                code(ErrorCode.RowNotLoaded,()->row.get_Lookup());var target=row.load_Lookup();check(snapshot.loadedRowCount()==2);
                code(ErrorCode.RowNotLoaded,()->target.get_Peer());var peer=target.load_Peer();check(peer.get_Peer()==target&&snapshot.loadedRowCount()==3);
                code(ErrorCode.TableNotLoaded,()->entry.findGroup_ById(1));
                var group=entry.loadGroup_ById(1,token);check(group.size()==2&&group.get(0)==row&&entry.findGroup_ById(1)==group&&snapshot.loadedRowCount()==4);
              }
              var row=entry.load("east",1,token);snapshot.loadAll(token);check(snapshot.completedTableCount()==2&&snapshot.loadedRowCount()==4&&entry.get("east",1)==row&&entry.rows().get(0)==row);
              check(row.load_Lookup().load_Peer().load_Peer()==row.get_Lookup());code(ErrorCode.KeyNotFound,()->entry.get("missing",99));
            }
            var badRoot=Path.of(args[0]+"-missing-ref");
            code(ErrorCode.MissingReference,()->DataSnapshot.load(badRoot,new LoadPlan(LoadMode.LazyRowsStrict,java.util.List.of("Entry"),true,java.util.List.of()),token));
            var bad=DataSnapshot.load(badRoot,new LoadPlan(LoadMode.LazyRowsOnAccess,java.util.List.of("Entry"),true,java.util.List.of()),token);
            for(int attempt=0;attempt<2;attempt++)code(ErrorCode.MissingReference,()->bad.get_Entry().load("east",1,token));
            try{bad.get_Entry().load("east",1,token);throw new AssertionError();}catch(ConfigException error){check(error.context.table().equals("Entry")&&error.context.fieldPath().equals("Lookup")&&!error.context.rowKey().isEmpty()&&!error.context.versionId().isEmpty()&&error.context.relativePath().endsWith("Entry.json"));}
            check(bad.loadedRowCount()==0);bad.get_Lookup().load(10,token);check(bad.loadedRowCount()==1);
            var lazy=DataSnapshot.load(root,new LoadPlan(LoadMode.LazyRowsStrict,java.util.List.of("Lookup"),false,java.util.List.of()),token);
            var scheduled=new java.util.concurrent.atomic.AtomicReference<Runnable>();var scheduledCount=new java.util.concurrent.atomic.AtomicInteger();
            Executor gate=work->{scheduledCount.incrementAndGet();scheduled.set(work);};
            var cancelledWait=new Cancellation();var first=lazy.get_Lookup().loadAsync(10,cancelledWait,gate);var second=lazy.get_Lookup().loadAsync(10,token,gate);
            cancelledWait.cancel();try{first.join();throw new AssertionError("cancelled row wait succeeded");}catch(CompletionException e){check(((ConfigException)e.getCause()).code==ErrorCode.Cancelled);}
            check(scheduledCount.get()==1&&lazy.loadedRowCount()==0);scheduled.get().run();check(second.join()==lazy.get_Lookup().get(10)&&lazy.loadedRowCount()==1);
            code(ErrorCode.InvalidPlan,()->DataSnapshot.load(Path.of("/missing"),new LoadPlan(LoadMode.LazyRowsStrict,java.util.List.of("Unknown"),false,java.util.List.of()),token));
            for(boolean json:new boolean[]{false,true}) {
              var selected=SelectedSnapshot.load(Path.of(args[0]+"-selected"),java.util.List.of("Entry"),json);
              check(selected.selection.tables().equals(java.util.List.of("Entry","Lookup")));
              check(selected.get_Entry().get("east",1).getLookup()==selected.get_Lookup().get(10));
              code(ErrorCode.TableNotSelected,()->selected.get_Concrete());
              code(ErrorCode.SchemaMismatch,()->SelectedSnapshot.load(Path.of(args[0]+"-selection-schema"),java.util.List.of("Entry"),json));
            }
            for(boolean json:new boolean[]{false,true}) {
              var manager=ServerConfig.manager();SnapshotFactory<ConfigSnapshot> make=()->ServerConfig.load(root,json);
              code(ErrorCode.NotInitialized,()->manager.acquireSnapshot());check(manager.tryAcquireSnapshot()==null);
              var prepared=manager.prepare(make,token,options);check(manager.tryAcquireSnapshot()==null);
              code(ErrorCode.ReloadBusy,()->manager.prepare(make,token,options));
              code(ErrorCode.ForeignCandidate,()->ServerConfig.manager().commit(prepared,token));
              var result=manager.commit(prepared,token);check(result.committed()&&result.generation()==1);check(manager.stageTimings().size()==2&&manager.stageTimings().stream().allMatch(t->t.succeeded()&&t.operationId()==result.operationId()&&t.elapsedMilliseconds()>=0));
              code(ErrorCode.AlreadyCommitted,()->manager.commit(prepared,token));
              var row=result.snapshot().Entry.get("east",1);var group=result.snapshot().Entry.getById(1);
              var pending=manager.prepare(make,token,options);manager.clear();
              code(ErrorCode.StaleCandidate,()->manager.commit(pending,token));
              manager.reload(make,token,options);manager.close();manager.close();
              code(ErrorCode.ManagerClosed,()->manager.acquireSnapshot());
              prepared.close();pending.close();check(row.getName().equals("中文😀")&&group.size()==2&&row.getLookup().getPeer().getPeer()==row.getLookup());
            }
            var manager=ServerConfig.manager();var entered=new CountDownLatch(1);var resume=new CountDownLatch(1);
            var executor=Executors.newSingleThreadExecutor();
            try {
              var loading=manager.prepareAsync(()->{entered.countDown();resume.await();return ServerConfig.load(root,false);},token,options,executor);
              entered.await();code(ErrorCode.ReloadBusy,()->manager.prepare(()->ServerConfig.load(root,false),token,options));
              manager.clear();var fresh=manager.prepare(()->ServerConfig.load(root,false),token,options);long operation=manager.status().preparingOperation();
              resume.countDown();try{loading.join();throw new AssertionError("late prepare succeeded");}catch(CompletionException e){check(((ConfigException)e.getCause()).code==ErrorCode.StaleCandidate);}
              check(manager.status().preparingOperation()==operation);
              var cancelled=new Cancellation();cancelled.cancel();code(ErrorCode.Cancelled,()->manager.commit(fresh,cancelled));
              var result=manager.commit(fresh,token);token.cancel();check(manager.acquireSnapshot()==result.snapshot());
              manager.close();fresh.close();
            } finally { resume.countDown();executor.shutdownNow();check(executor.awaitTermination(10,TimeUnit.SECONDS)); }
            for(String json:new String[]{"{}","{\"tables\":[null],\"format\":\"ZrConfig.Binary/1\",\"versionId\":\"v1\"}",""})
              code(ErrorCode.InvalidManifest,()->DataSnapshot.load(new MemoryConfigSource(java.util.Map.of("current.json",json.getBytes(java.nio.charset.StandardCharsets.UTF_8))),LoadPlan.defaults(),new Cancellation()));
            for(String kind:new String[]{"string","members","array"}){
              var budgetPlan=new LoadPlan(LoadMode.ServerDefault,java.util.List.of(kind.equals("array")?"Entry":"Lookup"),true,java.util.List.of(),ResourceLimits.builder().maxContainerElements(1).maxStringBytes(512).build(),"json-budget");
              try{DataSnapshot.load(Path.of(args[0]+"-json-"+kind),budgetPlan,new Cancellation());throw new AssertionError("JSON budget accepted");}catch(ConfigException error){check(error.code==ErrorCode.LimitExceeded&&error.stage.equals("parse"));}
            }
            var policyToken=new Cancellation();var fileSource=new FileConfigSource(root);var calls=new java.util.concurrent.atomic.AtomicInteger();
            for(String[] item:new String[][]{{"array","Values[1]"},{"matrix","Shape[2]"},{"dictionary","Mapping[0].value"},{"polymorphic","Action.Amount"}}){var malformed=DataSnapshot.load(Path.of(args[0]+"-json-field-"+item[0]),new LoadPlan(LoadMode.LazyRowsOnAccess,java.util.List.of("Entry"),true,java.util.List.of()),policyToken);try{malformed.get_Entry().load("east",1,policyToken);throw new AssertionError("Invalid field accepted");}catch(ConfigException error){check(error.code==ErrorCode.InvalidValue&&error.context.fieldPath().equals(item[1]));}}
            code(ErrorCode.UnsupportedCapability,()->new Catalog("{\"format\":2,\"runtimeContract\":\"server-v1\",\"requiredCapabilities\":[\"future-storage\"],\"fingerprint\":\"test\",\"tables\":[],\"components\":[]}"));
            for(String kind:new String[]{"wrong-target","missing-target"})code(ErrorCode.CatalogMismatch,()->DataSnapshot.load(Path.of(args[0]+"-"+kind),new LoadPlan(LoadMode.ServerDefault,null,false,java.util.List.of()),policyToken));
            try{DataSnapshot.load(Path.of(args[0]),new LoadPlan(LoadMode.ServerDefault,null,false,java.util.List.of(),ResourceLimits.builder().maxTables(1).build(),""),policyToken);throw new AssertionError();}catch(ConfigException error){check(error.code==ErrorCode.LimitExceeded&&error.stage.equals("parse"));}
            ConfigSource source=(name,maximum,cancellation)->{calls.incrementAndGet();return fileSource.read(name,maximum,cancellation);};
            code(ErrorCode.LimitExceeded,()->DataSnapshot.load(source,new LoadPlan(LoadMode.ServerDefault,null,false,java.util.List.of(),ResourceLimits.builder().maxManifestBytes(1).build(),"test"),policyToken));check(calls.get()==2);
            code(ErrorCode.UnsupportedCapability,()->ResourceLimits.builder().maxDepth(65).build());
            code(ErrorCode.LimitExceeded,()->DataSnapshot.load(source,new LoadPlan(LoadMode.ServerDefault,java.util.List.of("Entry"),false,java.util.List.of(),ResourceLimits.builder().maxRows(1).build(),"test"),policyToken));
            code(ErrorCode.LimitExceeded,()->DataSnapshot.load(source,new LoadPlan(LoadMode.ServerDefault,java.util.List.of("Lookup"),false,java.util.List.of(),ResourceLimits.builder().maxTableBytes(1).build(),"test"),policyToken));
            code(ErrorCode.LimitExceeded,()->DataSnapshot.load(source,new LoadPlan(LoadMode.LazyRowsStrict,java.util.List.of("Lookup"),false,java.util.List.of(),ResourceLimits.builder().maxEstimatedDecodedBytes(1).build(),"test"),policyToken));
            for(boolean json:new boolean[]{false,true}){
              var indexCapped=DataSnapshot.load(source,new LoadPlan(LoadMode.LazyRowsOnAccess,java.util.List.of("Entry"),json,java.util.List.of(),ResourceLimits.builder().maxEstimatedIndexBytes(1).build(),"test"),policyToken);
              for(int i=0;i<2;i++)code(ErrorCode.BudgetExceeded,()->indexCapped.get_Entry().loadGroup_ById(1,policyToken));check(indexCapped.diagnostics().estimatedIndexBytes()==0&&indexCapped.loadedRowCount()==0&&indexCapped.diagnostics().pendingJobs()==0);
              var indexed=DataSnapshot.load(source,new LoadPlan(LoadMode.LazyRowsOnAccess,java.util.List.of("Entry"),json,java.util.List.of()),policyToken);indexed.get_Entry().loadGroup_ById(1,policyToken);long estimate=indexed.diagnostics().estimatedIndexBytes();check(estimate>0);indexed.get_Entry().loadGroup_ById(1,policyToken);check(indexed.diagnostics().estimatedIndexBytes()==estimate);
            }
            {var brokenIndex=DataSnapshot.load(Path.of(args[0]+"-json-field-array"),new LoadPlan(LoadMode.LazyRowsOnAccess,java.util.List.of("Entry"),true,java.util.List.of()),policyToken);code(ErrorCode.InvalidValue,()->brokenIndex.get_Entry().loadGroup_ById(1,policyToken));check(brokenIndex.diagnostics().estimatedIndexBytes()==0&&brokenIndex.diagnostics().pendingJobs()==0);}
            var limited=DataSnapshot.load(source,new LoadPlan(LoadMode.LazyRowsOnAccess,java.util.List.of("Lookup"),false,java.util.List.of(),ResourceLimits.builder().maxDecodedRows(1).build(),"test"),policyToken);
            limited.get_Lookup().load(10,policyToken);for(int i=0;i<2;i++)code(ErrorCode.LimitExceeded,()->limited.get_Lookup().load(20,policyToken));
            var diagnostic=limited.diagnostics();check(diagnostic.decodedRows()==1&&diagnostic.activeDecodes()==0&&diagnostic.pendingJobs()==0&&diagnostic.pendingWaiters()==0&&diagnostic.faultedRows()==0&&diagnostic.readBytes()>diagnostic.retainedRawBytes());
            check(bad.diagnostics().faultedRows()==1&&bad.diagnostics().health().equals("Degraded"));
            var buffers=new java.util.HashMap<String,byte[]>();ConfigSource reused=(name,maximum,cancellation)->{byte[] bytes=fileSource.read(name,maximum,cancellation);buffers.put(name,bytes);return bytes;};
            var fixed=DataSnapshot.load(reused,new LoadPlan(LoadMode.LazyRowsOnAccess,java.util.List.of("Lookup"),false,java.util.List.of(),ResourceLimits.defaults(),"deployment"),policyToken);
            for(byte[] bytes:buffers.values())java.util.Arrays.fill(bytes,(byte)0);check(fixed.get_Lookup().load(10,policyToken).getName().equals("目标"));
            for(String name:buffers.keySet())buffers.put(name,fileSource.read(name,1L<<30,policyToken));var memory=new MemoryConfigSource(buffers);for(byte[] bytes:buffers.values())java.util.Arrays.fill(bytes,(byte)0);
            check(DataSnapshot.load(memory,new LoadPlan(LoadMode.ServerDefault,java.util.List.of("Lookup"),false,java.util.List.of()),policyToken).loadedRowCount()==2);
            code(ErrorCode.InvalidPlan,()->fileSource.read("../current.json",1024,policyToken));
            var history=DataSnapshot.manager();history.reload(()->fixed,policyToken,options);
            code(ErrorCode.VersionIdentityConflict,()->history.reload(()->DataSnapshot.load(badRoot,new LoadPlan(LoadMode.LazyRowsOnAccess,java.util.List.of("Entry"),true,java.util.List.of(),ResourceLimits.defaults(),"deployment"),policyToken),policyToken,options));check(history.acquireSnapshot()==fixed);
            var observed=new ServerManager<DataSnapshot>(new ManagerPolicy<>(null,java.util.List.of(),2,32,64));var generations=new java.util.ArrayList<Long>();
            var subscription=observed.subscribe(event->{generations.add(event.generation());if(event.kind().equals("Committed"))for(int i=0;i<10;i++)observed.clear();throw new IllegalStateException("listener failed");});
            var published=observed.reload(()->fixed,policyToken,options);check(published.generation()==1&&generations.equals(java.util.List.of(1L,10L,11L)));check(observed.eventDiagnostics().dropped()==8&&observed.eventDiagnostics().listenerFailures()==3);subscription.close();check(observed.eventDiagnostics().subscribers()==0);
            {var failureOwner=new ServerManager<Object>();var seenFailures=new java.util.ArrayList<ErrorCode>();var hostFailure=new AssertionError("original host failure");try(var sub=failureOwner.subscribe(event->{if(event.kind().equals("PrepareFailed")){check(!failureOwner.stageTimings().get(0).succeeded()&&failureOwner.stageTimings().get(0).operationId()==event.operationId());check(failureOwner.activePreparations()==0&&failureOwner.status().preparingOperation()==0);try(var retry=failureOwner.prepare(Object::new,policyToken,options)){check(retry.snapshot()!=null);}seenFailures.add(event.code());throw new IllegalStateException("Isolated failure listener");}})){code(ErrorCode.InvalidValue,()->failureOwner.prepare(()->{throw new ConfigException(ErrorCode.InvalidValue,"rejected");},policyToken,options));try{failureOwner.prepare(()->{throw hostFailure;},policyToken,options);throw new AssertionError();}catch(AssertionError error){check(error==hostFailure);}check(seenFailures.equals(java.util.List.of(ErrorCode.InvalidValue,ErrorCode.InternalError))&&failureOwner.eventDiagnostics().listenerFailures()==2);}}
            var rejected=new java.util.concurrent.atomic.AtomicBoolean();SnapshotValidator<DataSnapshot> validator=(snapshot,cancellation)->{if(rejected.get())throw new ConfigException(ErrorCode.InvalidValue,"validation");};
            var validated=new ServerManager<DataSnapshot>(new ManagerPolicy<>(null,java.util.List.of(validator),64,32,64));validated.reload(()->fixed,policyToken,options);rejected.set(true);code(ErrorCode.InvalidValue,()->validated.reload(()->fixed,policyToken,options));check(validated.acquireSnapshot()==fixed);
            var defect=new IllegalStateException("host programming defect");try{history.prepare(()->{throw defect;},policyToken,options);throw new AssertionError();}catch(IllegalStateException error){check(error==defect);}check(history.status().preparingOperation()==0);
            var queued=new java.util.ArrayList<Runnable>();var bounded=new ServerManager<DataSnapshot>(new ManagerPolicy<>(null,java.util.List.of(),64,32,2));
            var queued1=bounded.prepareAsync(()->fixed,policyToken,options,queued::add);check(bounded.activePreparations()==1);
            var busy=bounded.prepareAsync(()->fixed,policyToken,options,queued::add);try{busy.join();throw new AssertionError();}catch(CompletionException error){check(((ConfigException)error.getCause()).code==ErrorCode.ReloadBusy);}check(queued.size()==1);
            queued1.cancel(false);var queued2=bounded.prepareAsync(()->fixed,policyToken,options,queued::add);queued2.cancel(false);
            var full=bounded.prepareAsync(()->fixed,policyToken,options,queued::add);try{full.join();throw new AssertionError();}catch(CompletionException error){check(((ConfigException)error.getCause()).code==ErrorCode.LimitExceeded);}check(queued.size()==2);
            var scheduledFailures=new java.util.ArrayList<ErrorCode>();try(var subscribed=bounded.subscribe(event->{if(event.kind().equals("PrepareFailed")){scheduledFailures.add(event.code());check(!bounded.stageTimings().get(0).succeeded());}})){
              for(var work:queued)work.run();check(bounded.activePreparations()==0&&bounded.status().preparingOperation()==0);check(scheduledFailures.equals(java.util.List.of(ErrorCode.Cancelled,ErrorCode.Cancelled)));
              var rejection=new java.util.concurrent.RejectedExecutionException("closed executor");var rejectedFuture=bounded.prepareAsync(()->fixed,policyToken,options,work->{throw rejection;});try{rejectedFuture.join();throw new AssertionError();}catch(CompletionException error){check(error.getCause()==rejection);}check(bounded.activePreparations()==0&&scheduledFailures.equals(java.util.List.of(ErrorCode.Cancelled,ErrorCode.Cancelled,ErrorCode.ReloadBusy)));
            }
            code(ErrorCode.DeadlineExceeded,()->history.prepare(cancellation->{while(true){cancellation.check();Thread.onSpinWait();}},policyToken,new PreparationOptions(1,1000)));
            var memoryOwner=new java.util.concurrent.atomic.AtomicReference<ServerManager<Object>>();
            var livePolicy=ManagerPolicy.<Object>defaults().withEstimatedLiveBudget(100,value->{var owner=memoryOwner.get();if(owner!=null){var checkLock=new java.util.concurrent.FutureTask<>(owner::status);var threadCheck=new Thread(checkLock);threadCheck.start();try{checkLock.get(2,TimeUnit.SECONDS);}catch(Exception e){throw new AssertionError("Estimator called under manager lock",e);}}return 64;});
            var liveManager=new ServerManager<Object>(livePolicy);memoryOwner.set(liveManager);Object[] retained={liveManager.reload(Object::new,policyToken,options).snapshot()};var weakSnapshot=new java.lang.ref.WeakReference<>(retained[0]);liveManager.clear();
            check(liveManager.estimatedLiveBytes().orElseThrow()==64);code(ErrorCode.BudgetExceeded,()->liveManager.prepare(Object::new,policyToken,options));java.lang.ref.Reference.reachabilityFence(retained[0]);retained[0]=null;
            long gcDeadline=System.nanoTime()+TimeUnit.SECONDS.toNanos(5);while(weakSnapshot.get()!=null&&System.nanoTime()<gcDeadline){System.gc();Thread.sleep(10);}check(weakSnapshot.get()==null&&liveManager.estimatedLiveBytes().orElseThrow()==0);
            var estimate=new java.util.concurrent.atomic.AtomicLong(1);var growing=new ServerManager<Object>(ManagerPolicy.<Object>defaults().withEstimatedLiveBudget(1,value->estimate.get()));try(var candidate=growing.prepare(Object::new,policyToken,options)){estimate.set(2);code(ErrorCode.BudgetExceeded,()->growing.commit(candidate,policyToken));check(growing.tryAcquireSnapshot()==null&&growing.estimatedLiveBytes().orElseThrow()==2);}
            var cancellationOwner=new ServerManager<Object>(new ManagerPolicy<>(null,java.util.List.of(),64,32,1));var operationToken=new java.util.concurrent.atomic.AtomicReference<Cancellation>();var operationEntered=new CountDownLatch(1);var operationResume=new CountDownLatch(1);var operationExecutor=Executors.newSingleThreadExecutor();
            try{var operationFuture=cancellationOwner.prepareAsync(cancellation->{operationToken.set(cancellation);operationEntered.countDown();operationResume.await();return new Object();},policyToken,options,operationExecutor);operationEntered.await();cancellationOwner.clear();code(ErrorCode.Cancelled,()->operationToken.get().check());check(cancellationOwner.activePreparations()==1);code(ErrorCode.LimitExceeded,()->cancellationOwner.prepare(Object::new,policyToken,options));operationResume.countDown();try{operationFuture.join();throw new AssertionError();}catch(CompletionException e){check(((ConfigException)e.getCause()).code==ErrorCode.StaleCandidate);}check(cancellationOwner.activePreparations()==0);}finally{operationResume.countDown();operationExecutor.shutdownNow();check(operationExecutor.awaitTermination(10,TimeUnit.SECONDS));}
            var waitingSnapshot=DataSnapshot.load(root,new LoadPlan(LoadMode.LazyRowsOnAccess,java.util.List.of("Lookup"),false,java.util.List.of(),ResourceLimits.builder().maxPendingRequests(2).build(),"test"),policyToken);
            var rowJob=new java.util.concurrent.atomic.AtomicReference<Runnable>();Executor rowGate=rowJob::set;
            for(int i=0;i<100;i++){var waiter=waitingSnapshot.get_Lookup().loadAsync(10,policyToken,rowGate);waiter.cancel(false);check(waitingSnapshot.diagnostics().pendingWaiters()==0);}
            var surviving=waitingSnapshot.get_Lookup().loadAsync(10,policyToken,rowGate);check(waitingSnapshot.diagnostics().pendingJobs()==1);rowJob.get().run();check(surviving.join()==waitingSnapshot.get_Lookup().get(10));check(waitingSnapshot.diagnostics().pendingJobs()==0&&waitingSnapshot.diagnostics().pendingWaiters()==0);
            var large=DataSnapshot.load(Path.of(args[0]+"-large-ids"),new LoadPlan(LoadMode.ServerDefault,java.util.List.of("Lookup"),true,java.util.List.of()),policyToken);
            var largeRow=large.get_Lookup().get(1010);check(largeRow.get_Peer().get_Peer()==largeRow);
            var allocation=(com.sun.management.ThreadMXBean)java.lang.management.ManagementFactory.getThreadMXBean();allocation.setThreadAllocatedMemoryEnabled(true);
            for(int i=0;i<100000;i++)check(large.get_Lookup().get(1010).get_Peer().get_Peer()==largeRow);
            long thread=Thread.currentThread().getId(),before=allocation.getThreadAllocatedBytes(thread);for(int i=0;i<100000;i++)check(large.get_Lookup().get(1010).get_Peer().get_Peer()==largeRow);
            long allocated=allocation.getThreadAllocatedBytes(thread)-before;check(allocated==0);System.out.println("通过：Java 新服务器入口、大 Id 双向引用，100000 次查询分配 "+allocated+" B。");
            System.out.println("通过：Java 文件/内存源、预算、版本身份、校验器、队列准入、期限及取消等待者移除。");
            System.out.println("通过：Java 服务器生命周期、安全行/分组、异步准备、取消及屏障竞态。");
          }
        }
        """;
    internal const string Go = """
        package zrconfig
        import("context";"errors";"os";"testing";"sync";"path/filepath";"time";"runtime")
        type sourceFunc func(context.Context,string,int64)([]byte,error)
        func(f sourceFunc)Read(ctx context.Context,name string,maximum int64)([]byte,error){return f(ctx,name,maximum)}
        type memorySnapshot struct{memory *SnapshotMemory}
        func(s *memorySnapshot)SnapshotMemory()*SnapshotMemory{return s.memory}
        var serverSink int32
        func TestServer(t *testing.T){
          root:=os.Getenv("ZRCONFIG_TEST_OUTPUT");ctx:=context.Background();options:=DefaultPreparationOptions()
          code:=func(expected ErrorCode,err error){t.Helper();var e *ConfigError;if !errors.As(err,&e)||e.Code!=expected{t.Fatalf("expected %s, got %v",expected,err)}}
          {
            for _,json:=range []bool{false,true}{snapshot,err:=LoadDataWithWarmup(ctx,root,LoadPlan{Mode:LazyRowsOnAccess,Tables:[]string{"Entry"},PreferJSON:json},[]DataWarmup{WarmupRow_Lookup(10),WarmupGroup_Entry_ById(1),WarmupRow_Lookup(10)});if err!=nil{t.Fatal(err)};if snapshot.LoadedRowCount()!=3{t.Fatal("warmup decoded unrelated rows")};lookup,_:=snapshot.Get_Lookup();_,err=lookup.Get(20);code(RowNotLoaded,err);entry,_:=snapshot.Get_Entry();row,_:=entry.Get("east",1);target,err:=row.Get_Lookup();expected,_:=lookup.Get(10);if err!=nil||target!=expected{t.Fatal("warmup identity",err)}}
            reads:=0;source:=sourceFunc(func(context.Context,string,int64)([]byte,error){reads++;return nil,errors.New("must not read")});_,err:=LoadDataSourceWithWarmup(ctx,source,LoadPlan{Tables:[]string{"Lookup"}},[]DataWarmup{WarmupRow_Entry("east",1)});code(InvalidPlan,err);if reads!=0{t.Fatal("invalid warmup performed I/O")}
          }
          for _,json:=range []bool{false,true}{snapshot,err:=LoadData(ctx,root,LoadPlan{PreferJSON:json});if err!=nil{t.Fatal(err)};entry,_:=snapshot.Get_Entry();lookup,_:=snapshot.Get_Lookup();allocations:=testing.AllocsPerRun(10000,func(){row,e:=entry.Get("east",1);if e!=nil{panic(e)};group,e:=entry.FindGroup_ById(1);if e!=nil{panic(e)};target,e:=row.Get_Lookup();if e!=nil{panic(e)};peer,e:=target.Get_Peer();if e!=nil{panic(e)};back,e:=peer.Get_Peer();if e!=nil{panic(e)};expected,e:=lookup.Get(10);if e!=nil||back!=expected{panic("identity")};serverSink=row.Id()+int32(group.Len())});if allocations!=0{t.Fatalf("server hot allocation: %v",allocations)};t.Logf("新服务器热路径 JSON=%v: %v allocs/op",json,allocations)}
          for _,json:=range []bool{false,true}{for _,mode:=range []LoadMode{ServerDefault,SelectedTables,LazyRowsStrict,LazyRowsOnAccess}{
            snapshot,err:=LoadData(ctx,root+"-selected",LoadPlan{Mode:mode,Tables:[]string{"Entry"},PreferJSON:json});if err!=nil{t.Fatal(err)}
            expectedPlan:="a3fe29f4e2fb7a9cc160a8a474905d5c40bb7f57179f9651f08a8967455785d1";if json{expectedPlan="c55af6527720622add35286774df8dd1cf6e8eda7537d04aa1ee2eef94f31d91"};if len(snapshot.CatalogFingerprint())!=64||mode==LazyRowsOnAccess&&snapshot.LoadPlanFingerprint()!=expectedPlan{t.Fatal("snapshot identity")};entry,err:=snapshot.Get_Entry();if err!=nil{t.Fatal(err)};lookup,err:=snapshot.Get_Lookup();if err!=nil{t.Fatal(err)};_,err=snapshot.Get_Concrete();code(TableNotSelected,err)
            if mode==LazyRowsStrict||mode==LazyRowsOnAccess{
              if snapshot.LoadedRowCount()!=0||snapshot.CompletedTableCount()!=0{t.Fatal("eager row cache")};_,found,err:=entry.Find("missing",99);if found||err!=nil{t.Fatal(found,err)}
              _,_,err=entry.Find("east",1);code(RowNotLoaded,err);_,err=entry.Rows();code(TableNotLoaded,err)
              row,err:=entry.Load(ctx,"east",1);if err!=nil||snapshot.LoadedRowCount()!=1||row.LookupId()!=10{t.Fatal(err)}
              _,err=row.Get_Lookup();code(RowNotLoaded,err);target,err:=row.Load_Lookup(ctx);if err!=nil||snapshot.LoadedRowCount()!=2{t.Fatal(err)}
              _,err=target.Get_Peer();code(RowNotLoaded,err);peer,err:=target.Load_Peer(ctx);if err!=nil{t.Fatal(err)};back,err:=peer.Get_Peer();if err!=nil||back!=target||snapshot.LoadedRowCount()!=3{t.Fatal(err)}
              cached,err:=lookup.Get(10);if err!=nil||cached!=target{t.Fatal(err)}
              _,err=entry.FindGroup_ById(1);code(TableNotLoaded,err);group,err:=entry.LoadGroup_ById(ctx,1);if err!=nil||group.Len()!=2||group.At(0)!=row||snapshot.LoadedRowCount()!=4{t.Fatal(err)}
            }
            row,err:=entry.Load(ctx,"east",1);if err!=nil{t.Fatal(err)};if err=snapshot.LoadAll(ctx);err!=nil{t.Fatal(err)};if snapshot.CompletedTableCount()!=2{t.Fatal("complete table count")};rows,err:=entry.Rows();if err!=nil||rows.At(0)!=row||snapshot.LoadedRowCount()!=4{t.Fatal(err)}
          }}
          _,badErr:=LoadData(ctx,root+"-missing-ref",LoadPlan{Mode:LazyRowsStrict,Tables:[]string{"Entry"},PreferJSON:true});code(MissingReference,badErr)
          bad,badErr:=LoadData(ctx,root+"-missing-ref",LoadPlan{Mode:LazyRowsOnAccess,Tables:[]string{"Entry"},PreferJSON:true});if badErr!=nil{t.Fatal(badErr)};badEntry,_:=bad.Get_Entry()
          for attempt:=0;attempt<2;attempt++{_,badErr=badEntry.Load(ctx,"east",1);code(MissingReference,badErr)};if bad.LoadedRowCount()!=0{t.Fatal("partial bad row published")}
          exposedFailure:=badErr.(*ConfigError);exposedFailure.Code=KeyNotFound;exposedFailure.Field="poisoned"
          _,badErr=badEntry.Load(ctx,"east",1);code(MissingReference,badErr);if badErr.(*ConfigError).Field!="Lookup"{t.Fatal("caller modified cached error")}
          lazy,err:=LoadData(ctx,root,LoadPlan{Mode:LazyRowsStrict,Tables:[]string{"Lookup"}});if err!=nil{t.Fatal(err)};lookup,_:=lazy.Get_Lookup();var wait sync.WaitGroup;results:=make([]*LookupRow,32)
          for i:=range results{wait.Add(1);go func(i int){defer wait.Done();row,e:=lookup.Load(ctx,10);if e!=nil{t.Error(e)};results[i]=row}(i)};wait.Wait();for _,row:=range results{if row!=results[0]{t.Fatal("row identity mismatch")}};if lazy.LoadedRowCount()!=1{t.Fatal("row decoded more than once")}
          _,err=LoadData(ctx,"/missing",LoadPlan{Tables:[]string{"Unknown"}});code(InvalidPlan,err)
          for _,json:=range []bool{false,true}{
            selected,err:=LoadSelectedConfig(root+"-selected",[]string{"Entry"},json);if err!=nil{t.Fatal(err)}
            if selected.Selection().Tables().Len()!=2{t.Fatal("wrong closure")}
            entry,err:=selected.Get_Entry();if err!=nil{t.Fatal(err)};lookup,err:=selected.Get_Lookup();if err!=nil{t.Fatal(err)}
            if entry.Get("east",1).Lookup()!=lookup.Get(10){t.Fatal("mixed reference owner")}
            _,err=selected.Get_Concrete();code(TableNotSelected,err)
            _,err=LoadSelectedConfig(root+"-selection-schema",[]string{"Entry"},json);code(SchemaMismatch,err)
          }
          for _,json:=range []bool{false,true}{
            var manager ServerConfigManager;make:=func(context.Context)(*ConfigSnapshot,error){return LoadServerConfig(root,json)}
            _,err:=manager.AcquireSnapshot();code(NotInitialized,err)
            prepared,err:=manager.Prepare(ctx,make,options);if err!=nil{t.Fatal(err)}
            _,err=manager.Prepare(ctx,make,options);code(ReloadBusy,err)
            var other ServerConfigManager;_,err=other.Commit(ctx,prepared);code(ForeignCandidate,err)
            result,err:=manager.Commit(ctx,prepared);if err!=nil||!result.Committed||result.Generation!=1{t.Fatal(result,err)};for _,timing:=range manager.StageTimings(){if !timing.Succeeded||timing.OperationID!=result.OperationID||timing.ElapsedMilliseconds<0{t.Fatal(timing)}}
            _,err=manager.Commit(ctx,prepared);code(AlreadyCommitted,err)
            row:=result.Snapshot.Entry().Get("east",1);group:=result.Snapshot.Entry().GetById(1)
            pending,err:=manager.Prepare(ctx,make,options);if err!=nil{t.Fatal(err)};if err=manager.Clear();err!=nil{t.Fatal(err)}
            _,err=manager.Commit(ctx,pending);code(StaleCandidate,err)
            if _,err=manager.Reload(ctx,make,options);err!=nil{t.Fatal(err)}
            manager.Close();manager.Close();_,err=manager.AcquireSnapshot();code(ManagerClosed,err)
            prepared.Discard();pending.Discard()
            if row.Name()!="中文😀"||group.Len()!=2||row.Lookup().Peer().Peer()!=row.Lookup(){t.Fatal("old owner invalid")}
          }
          var manager ServerConfigManager;entered,resume:=make(chan struct{}),make(chan struct{});done:=make(chan error,1)
          factory:=func(context.Context)(*ConfigSnapshot,error){return LoadServerConfig(root,false)}
          go func(){_,err:=manager.Prepare(ctx,func(context.Context)(*ConfigSnapshot,error){close(entered);<-resume;return factory(ctx)},options);done<-err}()
          <-entered;_,err=manager.Prepare(ctx,factory,options);code(ReloadBusy,err)
          manager.Clear();fresh,err:=manager.Prepare(ctx,factory,options);if err!=nil{t.Fatal(err)};operation:=manager.Status().PreparingOperation
          close(resume);code(StaleCandidate,<-done);if manager.Status().PreparingOperation!=operation{t.Fatal("stale cleanup removed fresh candidate")}
          cancelled,cancel:=context.WithCancel(ctx);cancel();_,err=manager.Commit(cancelled,fresh);code(Cancelled,err)
          if _,err=manager.Commit(ctx,fresh);err!=nil{t.Fatal(err)};manager.Close();fresh.Discard()
          for _,json:=range []string{"{}",`{"tables":[null],"format":"ZrConfig.Binary/1","versionId":"v1"}`,""}{memory,_:=NewMemoryConfigSource(map[string][]byte{"current.json":[]byte(json)});_,err=LoadDataSource(ctx,memory,LoadPlan{});code(InvalidManifest,err)}
          fileSource,err:=NewFileConfigSource(root);if err!=nil{t.Fatal(err)}
          reads:=0;source:=sourceFunc(func(ctx context.Context,name string,maximum int64)([]byte,error){reads++;return fileSource.Read(ctx,name,maximum)})
          limit:=DefaultResourceLimits();limit.MaxManifestBytes=1;_,err=LoadDataSource(ctx,source,LoadPlan{Limits:limit});code(LimitExceeded,err);if reads!=2{t.Fatal("manifest limit did not stop payload reads")}
          limit=DefaultResourceLimits();limit.MaxRows=1;_,err=LoadDataSource(ctx,source,LoadPlan{Limits:limit,Tables:[]string{"Entry"}});code(LimitExceeded,err)
          limit=DefaultResourceLimits();limit.MaxTableBytes=1;_,err=LoadDataSource(ctx,source,LoadPlan{Limits:limit,Tables:[]string{"Lookup"}});code(LimitExceeded,err)
          for _,item:=range [][2]string{{"array","Values[1]"},{"matrix","Shape[2]"},{"dictionary","Mapping[0].value"},{"polymorphic","Action.Amount"}}{malformed,e:=LoadData(ctx,root+"-json-field-"+item[0],LoadPlan{Mode:LazyRowsOnAccess,Tables:[]string{"Entry"},PreferJSON:true});if e!=nil{t.Fatal(e)};table,_:=malformed.Get_Entry();_,e=table.Load(ctx,"east",1);code(InvalidValue,e);var failure *ConfigError;if !errors.As(e,&failure)||failure.Field!=item[1]{t.Fatal("Field path",item,e)}}
          func(){var failure error;func(){defer recoverServer(&failure);newCatalog("{\"format\":2,\"runtimeContract\":\"server-v1\",\"requiredCapabilities\":[\"future-storage\"],\"fingerprint\":\"test\",\"tables\":[],\"components\":[]}")}();code(UnsupportedCapability,failure)}()
          for _,kind:=range []string{"wrong-target","missing-target"}{_,failure:=LoadData(ctx,root+"-"+kind,LoadPlan{});code(CatalogMismatch,failure)}
          func(){limits:=DefaultResourceLimits();limits.MaxTables=1;_,e:=LoadData(ctx,root,LoadPlan{Limits:limits});code(LimitExceeded,e);var failure *ConfigError;if !errors.As(e,&failure)||failure.Stage!="parse"{t.Fatal("Manifest allocation guard",e)}}()
          for _,kind:=range []string{"string","members","array"}{table:="Lookup";if kind=="array"{table="Entry"};budget:=DefaultResourceLimits();budget.MaxContainerElements=1;budget.MaxStringBytes=512;_,failure:=LoadData(ctx,root+"-json-"+kind,LoadPlan{Tables:[]string{table},PreferJSON:true,Limits:budget});code(LimitExceeded,failure);var structured *ConfigError;if !errors.As(failure,&structured)||structured.Stage!="parse"{t.Fatal("JSON allocation check ran after parsing",kind,failure)}}
          limit=DefaultResourceLimits();limit.MaxDepth=65;reads=0;_,err=LoadDataSource(ctx,source,LoadPlan{Limits:limit});code(UnsupportedCapability,err);if reads!=0{t.Fatal("invalid policy did I/O")}
          {limits:=DefaultResourceLimits();limits.MaxEstimatedDecodedBytes=1;_,e:=LoadDataSource(ctx,source,LoadPlan{Mode:LazyRowsStrict,Tables:[]string{"Lookup"},Limits:limits});code(LimitExceeded,e)}
          for _,json:=range []bool{false,true}{limits:=DefaultResourceLimits();limits.MaxEstimatedIndexBytes=1;bounded,e:=LoadDataSource(ctx,source,LoadPlan{Mode:LazyRowsOnAccess,Tables:[]string{"Entry"},PreferJSON:json,Limits:limits});if e!=nil{t.Fatal(e)};table,_:=bounded.Get_Entry();for i:=0;i<2;i++{_,e=table.LoadGroup_ById(ctx,1);code(BudgetExceeded,e)};d:=bounded.Diagnostics();if d.EstimatedIndexBytes!=0||d.PendingJobs!=0||bounded.LoadedRowCount()!=0{t.Fatal("index admission accounting",d)}
            indexed,e:=LoadDataSource(ctx,source,LoadPlan{Mode:LazyRowsOnAccess,Tables:[]string{"Entry"},PreferJSON:json});if e!=nil{t.Fatal(e)};view,_:=indexed.Get_Entry();if _,e=view.LoadGroup_ById(ctx,1);e!=nil{t.Fatal(e)};estimate:=indexed.Diagnostics().EstimatedIndexBytes;view.LoadGroup_ById(ctx,1);if estimate<=0||indexed.Diagnostics().EstimatedIndexBytes!=estimate{t.Fatal("index charged twice")}}
          {broken,e:=LoadData(ctx,root+"-json-field-array",LoadPlan{Mode:LazyRowsOnAccess,Tables:[]string{"Entry"},PreferJSON:true});if e!=nil{t.Fatal(e)};table,_:=broken.Get_Entry();_,e=table.LoadGroup_ById(ctx,1);code(InvalidValue,e);if broken.Diagnostics().EstimatedIndexBytes!=0||broken.Diagnostics().PendingJobs!=0{t.Fatal("failed index retained budget")}}
          limit=DefaultResourceLimits();limit.MaxDecodedRows=1
          limited,err:=LoadDataSource(ctx,source,LoadPlan{Limits:limit,Mode:LazyRowsOnAccess,Tables:[]string{"Lookup"}});if err!=nil{t.Fatal(err)};limitedRows,_:=limited.Get_Lookup();_,err=limitedRows.Load(ctx,10);if err!=nil{t.Fatal(err)}
          for i:=0;i<2;i++{_,err=limitedRows.Load(ctx,20);code(LimitExceeded,err)};d:=limited.Diagnostics();if d.DecodedRows!=1||d.ActiveDecodes!=0||d.PendingJobs!=0||d.PendingWaiters!=0||d.FaultedRows!=0||d.Health!="Healthy"||d.ReadBytes<=d.RetainedRawBytes{t.Fatal("bad limit accounting",d)}
          if bad.Diagnostics().FaultedRows!=1||bad.Diagnostics().Health!="Degraded"{t.Fatal("terminal fault not counted once")}
          reuse:=map[string][]byte{};source=sourceFunc(func(ctx context.Context,name string,maximum int64)([]byte,error){data,err:=fileSource.Read(ctx,name,maximum);if err==nil{reuse[name]=data};return data,err})
          fixed,err:=LoadDataSource(ctx,source,LoadPlan{Mode:LazyRowsOnAccess,Tables:[]string{"Lookup"},SourceNamespace:"deployment"});if err!=nil{t.Fatal(err)};for _,data:=range reuse{for i:=range data{data[i]=0}}
          fixedRows,_:=fixed.Get_Lookup();fixedRow,err:=fixedRows.Load(ctx,10);if err!=nil||fixedRow.Name()!="目标"{t.Fatal("source buffer reuse corrupted captured version",err)}
          memoryFiles:=map[string][]byte{};for name:=range reuse{data,err:=fileSource.Read(ctx,name,1<<30);if err!=nil{t.Fatal(err)};memoryFiles[name]=data};memory,err:=NewMemoryConfigSource(memoryFiles);if err!=nil{t.Fatal(err)};for _,data:=range memoryFiles{for i:=range data{data[i]=0}}
          if _,err=LoadDataSource(ctx,memory,LoadPlan{Tables:[]string{"Lookup"}});err!=nil{t.Fatal("memory source ownership",err)}
          _,err=fileSource.Read(ctx,"../current.json",100);code(InvalidPlan,err)
          link:=filepath.Join(t.TempDir(),"release");if err=os.Symlink(root,link);err==nil{linked,_:=NewFileConfigSource(link);_,err=linked.Read(ctx,"current.json",1<<20);code(ReadFailed,err)}
          var deadlineManager DataManager;short:=options;short.Timeout=time.Millisecond
          _,err=deadlineManager.Prepare(ctx,func(ctx context.Context)(*DataSnapshot,error){return LoadDataSource(ctx,sourceFunc(func(ctx context.Context,_ string,_ int64)([]byte,error){<-ctx.Done();return nil,checkContext(ctx)}),LoadPlan{})},short);code(DeadlineExceeded,err)
          if deadlineManager.Status().PreparingOperation!=0{t.Fatal("deadline left pending candidate")}
          history,_:=NewServerManager[DataSnapshot](ManagerPolicy[DataSnapshot]{})
          factoryData:=func(context.Context)(*DataSnapshot,error){return fixed,nil};_,err=history.Reload(ctx,factoryData,options);if err!=nil{t.Fatal(err)}
          admission,_:=NewServerManager[DataSnapshot](ManagerPolicy[DataSnapshot]{MaxPendingPreparations:1})
          enteredAdmission,releaseAdmission:=make(chan context.Context,1),make(chan struct{});admissionDone:=make(chan error,1)
          go func(){_,failure:=admission.Prepare(ctx,func(active context.Context)(*DataSnapshot,error){enteredAdmission<-active;<-releaseAdmission;return fixed,nil},options);admissionDone<-failure}()
          activeAdmission:=<-enteredAdmission;if err=admission.Clear();err!=nil{t.Fatal(err)};if activeAdmission.Err()!=context.Canceled{t.Fatal("clear did not cancel active preparation")}
          _,err=admission.Prepare(ctx,factoryData,options);code(LimitExceeded,err);close(releaseAdmission);code(StaleCandidate,<-admissionDone)
          if _,err=admission.Reload(ctx,factoryData,options);err!=nil{t.Fatal("preparation admission was not released",err)}
          _,err=history.Reload(ctx,func(context.Context)(*DataSnapshot,error){return LoadData(ctx,root+"-missing-ref",LoadPlan{Mode:LazyRowsOnAccess,Tables:[]string{"Entry"},PreferJSON:true,SourceNamespace:"deployment"})},options);code(VersionIdentityConflict,err)
          current,_:=history.AcquireSnapshot();if current!=fixed{t.Fatal("identity failure published candidate")}
          var observed *ServerManager[DataSnapshot];observed,_=NewServerManager[DataSnapshot](ManagerPolicy[DataSnapshot]{EventCapacity:2});var generations []uint64
          remove,_:=observed.Subscribe(func(event ManagerEvent){generations=append(generations,event.Generation);if event.Kind=="Committed"{for i:=0;i<10;i++{if err:=observed.Clear();err!=nil{t.Fatal(err)}}};panic("listener failure")})
          committed,err:=observed.Reload(ctx,factoryData,options);if err!=nil||committed.Generation!=1||len(generations)!=3||generations[1]!=10||generations[2]!=11{t.Fatal("reentrant event order",generations,err)}
          ed:=observed.EventDiagnostics();if ed.Dropped!=8||ed.ListenerFailures!=3{t.Fatal("event accounting",ed)};remove();if observed.EventDiagnostics().Subscribers!=0{t.Fatal("unsubscribe")}
          {failureOwner,_:=NewServerManager[DataSnapshot](ManagerPolicy[DataSnapshot]{MaxPendingPreparations:1});var seen []ErrorCode;remove,_:=failureOwner.Subscribe(func(event ManagerEvent){if event.Kind=="PrepareFailed"{if failureOwner.StageTimings()[0].Succeeded||failureOwner.StageTimings()[0].OperationID!=event.OperationID{t.Fatal("failure timing")};if failureOwner.Status().PreparingOperation!=0{t.Fatal("failure before ticket cleanup")};retry,e:=failureOwner.Prepare(ctx,factoryData,options);if e!=nil{t.Fatal("failure before active admission cleanup",e)};retry.Discard();seen=append(seen,event.Code);panic("Isolated failure listener")}});_,e:=failureOwner.Prepare(ctx,func(context.Context)(*DataSnapshot,error){return nil,serverError(InvalidValue)},options);code(InvalidValue,e);hostFailure:=&struct{Value int}{42};func(){defer func(){if recover()!=hostFailure{t.Fatal("host panic was replaced")}}();failureOwner.Prepare(ctx,func(context.Context)(*DataSnapshot,error){panic(hostFailure)},options)}();if len(seen)!=2||seen[0]!=InvalidValue||seen[1]!=InternalError||failureOwner.EventDiagnostics().ListenerFailures!=2{t.Fatal("failure event count",seen)};remove()}
          reject:=false;gated,_:=NewServerManager[DataSnapshot](ManagerPolicy[DataSnapshot]{Validators:[]func(context.Context,*DataSnapshot)error{func(context.Context,*DataSnapshot)error{if reject{return serverError(InvalidValue)};return nil}}})
          if _,err=gated.Reload(ctx,factoryData,options);err!=nil{t.Fatal(err)};reject=true;_,err=gated.Reload(ctx,factoryData,options);code(InvalidValue,err);if current,_:=gated.AcquireSnapshot();current!=fixed{t.Fatal("validator changed current")}
          abandoned,err:=history.Prepare(ctx,factoryData,options);if err!=nil{t.Fatal(err)};abandoned.Discard();if history.Status().PreparingOperation!=0{t.Fatal("discard did not release ticket")}
          {
            memoryManager,e:=NewServerManager[memorySnapshot](ManagerPolicy[memorySnapshot]{MaxEstimatedLiveBytes:100});if e!=nil{t.Fatal(e)}
            factory:=func(context.Context)(*memorySnapshot,error){return &memorySnapshot{NewSnapshotMemory(64)},nil};result,e:=memoryManager.Reload(ctx,factory,options);if e!=nil{t.Fatal(e)};retained:=result.Snapshot;result=CommitResult[memorySnapshot]{};memoryManager.Clear()
            if bytes,configured:=memoryManager.EstimatedLiveBytes();!configured||bytes!=64{t.Fatal("live estimate",bytes)};_,e=memoryManager.Prepare(ctx,factory,options);code(BudgetExceeded,e);runtime.KeepAlive(retained);retained=nil
            until:=time.Now().Add(5*time.Second);for time.Now().Before(until){runtime.GC();time.Sleep(time.Millisecond*10);if bytes,_:=memoryManager.EstimatedLiveBytes();bytes==0{break}}
            if bytes,_:=memoryManager.EstimatedLiveBytes();bytes!=0{t.Fatal("manager retained retired snapshot accounting",bytes)}
            candidate,e:=memoryManager.Prepare(ctx,factory,options);if e!=nil{t.Fatal(e)};value,_:=candidate.Snapshot();value.memory.SetEstimatedBytes(101);_,e=memoryManager.Commit(ctx,candidate);code(BudgetExceeded,e);if current,_:=memoryManager.TryAcquireSnapshot();current!=nil{t.Fatal("over-budget commit published")};candidate.Discard()
            _,e=NewServerManager[int](ManagerPolicy[int]{MaxEstimatedLiveBytes:100});code(UnsupportedCapability,e)
          }
          t.Log("通过：Go 文件/内存源、限额、固定缓冲、预算归还、身份冲突、校验器及有界重入事件。")
          t.Log("通过：Go 服务器生命周期、安全行/分组、候选归属、取消及屏障竞态。")
        }
        """;
    internal const string TypeScript = """
        const assert=require('node:assert/strict'),path=require('node:path');
        const S=require(path.join(process.argv[2],'dist/server.js')),D=require(path.join(process.argv[2],'dist/data.js')),R=require(path.join(process.argv[2],'dist/runtime.js')),root=process.argv[3];
        function code(expected,work){assert.throws(work,e=>e.code===expected);}
        (async()=>{
          const token=new R.Cancellation();
          {
            for(const json of [false,true]){const options={mode:'LazyRowsOnAccess',tables:['Entry'],preferJSON:json};const steps=[D.Snapshot.warmupRow_Lookup(10),D.Snapshot.warmupGroup_Entry_ById(1),D.Snapshot.warmupRow_Lookup(10)];
              for(const snapshot of [D.Snapshot.loadWithWarmup(root,options,steps,token),await D.Snapshot.loadWithWarmupAsync(root,options,steps,token,new R.NodeLoadScheduler())]){assert.equal(snapshot.loadedRowCount,3);assert.equal(snapshot.get_Entry().get('east',1).get_Lookup(),snapshot.get_Lookup().get(10));code('RowNotLoaded',()=>snapshot.get_Lookup().get(20));}
            }
            let reads=0;const source={read(){reads++;throw Error('must not read')}};code('InvalidPlan',()=>D.Snapshot.loadWithWarmup(source,{tables:['Lookup']},[D.Snapshot.warmupRow_Entry('east',1)],token));assert.equal(reads,0);
            const snapshot=D.Snapshot.load(root,{mode:'LazyRowsOnAccess',tables:['Entry']});const jobs=[];const scheduler={yield:()=>new Promise(resolve=>jobs.push(resolve)),execute:work=>Promise.resolve().then(work)};
            const waitingToken=new R.Cancellation(),cancelled=snapshot.get_Entry().loadGroup_ByIdAsync(1,waitingToken,scheduler);const keep=snapshot.get_Entry().loadGroup_ByIdAsync(1,token,scheduler);assert.equal(jobs.length,1);waitingToken.cancel();await assert.rejects(cancelled,e=>e.code==='Cancelled');jobs.shift()();const group=await keep;assert.equal(group.length,2);assert.equal(snapshot.loadedRowCount,2);assert.equal(snapshot.get_Entry().findGroup_ById(1),group);assert.equal(snapshot.diagnostics().pendingWaiters,0);
          }
          {
            let sink;function probe(snapshot){const before=process.memoryUsage().heapUsed;const table=snapshot.get_Entry();for(let i=0;i<1000000;i++){const row=table.get('east',1),target=row.get_Lookup();assert.equal(target.get_Peer().get_Peer(),target);sink=row.Id+table.findGroup_ById(1).length;}return process.memoryUsage().heapUsed-before;}
            for(const json of [false,true]){const snapshot=D.Snapshot.load(root,{preferJSON:json});for(let i=0;i<4;i++)probe(snapshot);if(global.gc)global.gc();const growth=probe(snapshot);assert.equal(sink,3);console.log('通过：TypeScript 新服务器主键、分组及双向引用 1000000 次查询堆变化 '+growth+' B；非精确分配计数。');}
          }
          for (const json of [false,true]) for (const mode of ['ServerDefault','SelectedTables','LazyRowsStrict','LazyRowsOnAccess']) {
            const snapshot=D.Snapshot.load(root+'-selected',{mode,tables:['Entry'],preferJSON:json});assert.equal(snapshot.catalogFingerprint.length,64);if(mode==='LazyRowsOnAccess')assert.equal(snapshot.loadPlanFingerprint,json?'c55af6527720622add35286774df8dd1cf6e8eda7537d04aa1ee2eef94f31d91':'a3fe29f4e2fb7a9cc160a8a474905d5c40bb7f57179f9651f08a8967455785d1');
            const entry=snapshot.get_Entry(),lookup=snapshot.get_Lookup();
            code('TableNotSelected',()=>snapshot.get_Concrete());
            if (mode.startsWith('Lazy')) {
              assert.equal(snapshot.completedTableCount,0);assert.equal(snapshot.loadedRowCount,0);assert.equal(entry.find('missing',99),undefined);
              code('RowNotLoaded',()=>entry.find('east',1));code('TableNotLoaded',()=>entry.all());
              const row=entry.load('east',1);assert.equal(snapshot.loadedRowCount,1);
              assert.equal(row.LookupId,10);code('RowNotLoaded',()=>row.get_Lookup());
              const target=row.load_Lookup();assert.equal(snapshot.loadedRowCount,2);
              code('RowNotLoaded',()=>target.get_Peer());const peer=target.load_Peer();
              assert.equal(peer.get_Peer(),target);assert.equal(row.get_Lookup(),lookup.get(10));assert.equal(snapshot.loadedRowCount,3);
              code('TableNotLoaded',()=>entry.findGroup_ById(1));
              const group=entry.loadGroup_ById(1);assert.equal(group.length,2);assert.equal(group[0],row);
              assert.equal(entry.findGroup_ById(1),group);assert.equal(snapshot.loadedRowCount,4);
            }
            const existing=entry.load('east',1);snapshot.loadAll();
            assert.equal(snapshot.completedTableCount,2);assert.equal(snapshot.loadedRowCount,4);assert.equal(entry.get('east',1),existing);assert.equal(entry.all()[0],existing);
            assert.equal(existing.load_Lookup().load_Peer().load_Peer(),existing.load_Lookup());
            code('KeyNotFound',()=>entry.get('missing',99));
          }
          code('MissingReference',()=>D.Snapshot.load(root+'-missing-ref',{mode:'LazyRowsStrict',tables:['Entry'],preferJSON:true}));
          const bad=D.Snapshot.load(root+'-missing-ref',{mode:'LazyRowsOnAccess',tables:['Entry'],preferJSON:true});
          for(let i=0;i<2;i++)code('MissingReference',()=>bad.get_Entry().load('east',1));
          try{bad.get_Entry().load('east',1);assert.fail();}catch(error){assert.equal(error.context.table,'Entry');assert.equal(error.context.fieldPath,'Lookup');assert.ok(error.context.versionId&&error.context.rowKey&&error.context.relativePath.endsWith('Entry.json'));assert.ok(Object.isFrozen(error)&&Object.isFrozen(error.context));}
          assert.equal(bad.loadedRowCount,0);assert.equal(bad.get_Entry().phase,'Degraded');bad.get_Lookup().load(10);assert.equal(bad.loadedRowCount,1);
          for(const [kind,field] of [['array','Values[1]'],['matrix','Shape[2]'],['dictionary','Mapping[0].value'],['polymorphic','Action.Amount']]){const malformed=D.Snapshot.load(root+'-json-field-'+kind,{mode:'LazyRowsOnAccess',tables:['Entry'],preferJSON:true});try{malformed.get_Entry().load('east',1);assert.fail('Invalid field accepted');}catch(error){assert.equal(error.code,'InvalidValue');assert.equal(error.context.fieldPath,field);}}
          assert.throws(()=>new R.Catalog("{\"format\":2,\"runtimeContract\":\"server-v1\",\"requiredCapabilities\":[\"future-storage\"],\"fingerprint\":\"test\",\"tables\":[],\"components\":[]}"),error=>error.code==='UnsupportedCapability');
          for(const kind of ['wrong-target','missing-target'])assert.throws(()=>D.Snapshot.load(root+'-'+kind),error=>error.code==='CatalogMismatch');
          assert.throws(()=>D.Snapshot.load(root,{limits:new R.ResourceLimits({maxTables:1})}),error=>error.code==='LimitExceeded'&&error.stage==='parse');
          for(const kind of ['string','members','array']){try{D.Snapshot.load(root+'-json-'+kind,{tables:[kind==='array'?'Entry':'Lookup'],preferJSON:true,limits:new R.ResourceLimits({maxContainerElements:1,maxStringBytes:512})});assert.fail('JSON budget accepted');}catch(error){assert.equal(error.code,'LimitExceeded');assert.equal(error.stage,'parse');}}
          for(const json of ['{}','{"tables":[null],"format":"ZrConfig.Binary/1","versionId":"v1"}',''])code('InvalidManifest',()=>D.Snapshot.load(new R.MemoryConfigSource(new Map([['current.json',Buffer.from(json)]]))));
          const fileSource=new R.FileConfigSource(root);let reads=0;
          const tracked={read:(name,max,token)=>{reads++;return fileSource.read(name,max,token);}};
          code('InvalidPlan',()=>D.Snapshot.load(tracked,{tables:['Unknown']}));assert.equal(reads,0);
          code('UnsupportedCapability',()=>D.Snapshot.load(tracked,{unrecognized:true}));assert.equal(reads,0);
          code('LimitExceeded',()=>D.Snapshot.load(tracked,{limits:new R.ResourceLimits({maxManifestBytes:4})}));
          code('LimitExceeded',()=>D.Snapshot.load(tracked,{limits:new R.ResourceLimits({maxTables:1})}));
          code('LimitExceeded',()=>D.Snapshot.load(tracked,{tables:['Lookup'],limits:new R.ResourceLimits({maxRows:1})}));
          code('BudgetExceeded',()=>D.Snapshot.load(tracked,{mode:'LazyRowsStrict',tables:['Lookup'],limits:new R.ResourceLimits({maxEstimatedDecodedBytes:1})}));
          for(const preferJSON of [false,true]){const capped=D.Snapshot.load(tracked,{mode:'LazyRowsOnAccess',tables:['Entry'],preferJSON,limits:new R.ResourceLimits({maxEstimatedIndexBytes:1})});for(let i=0;i<2;i++)code('BudgetExceeded',()=>capped.get_Entry().loadGroup_ById(1));assert.equal(capped.estimatedIndexBytes,0);assert.equal(capped.loadedRowCount,0);assert.equal(capped.diagnostics().pendingJobs,0);
            const indexed=D.Snapshot.load(tracked,{mode:'LazyRowsOnAccess',tables:['Entry'],preferJSON});const view=indexed.get_Entry();const pending=view.loadGroup_ByIdAsync(1,new R.Cancellation(),new R.NodeLoadScheduler());const workspace=indexed.estimatedIndexBytes;view.loadGroup_ById(1);const estimate=indexed.estimatedIndexBytes;assert(estimate>0&&estimate<workspace);await pending;assert.equal(indexed.estimatedIndexBytes,estimate);assert.equal(indexed.diagnostics().pendingJobs,0);}
          {const broken=D.Snapshot.load(root+'-json-field-array',{mode:'LazyRowsOnAccess',tables:['Entry'],preferJSON:true});code('InvalidValue',()=>broken.get_Entry().loadGroup_ById(1));assert.equal(broken.estimatedIndexBytes,0);}
          {
            const budget=new R.LoadBudget(new R.ResourceLimits());let scans=0,yields=0,resume,arrived;
            const paused=new Promise(resolve=>arrived=resolve),release=new Promise(resolve=>resume=resolve);
            class GroupProbe extends R.RowStorage {
              constructor(){super({count:130,keys:new Map(),schema:{groups:[{name:'Key',fields:['Id']}]},budget,estimatedIndexBytes:()=>100000,readTransient:i=>{scans++;return {Id:i%3};}},row=>row);}
              sync(){return this.groupIndex('Key',true,new R.Cancellation());}
              async begin(){return this.groupIndexAsync('Key',new R.Cancellation(),{yield:async()=>{if(++yields===2){arrived();await release;}}});}
            }
            const probe=new GroupProbe(),pending=probe.begin();await paused;assert.equal(scans,64);
            const completed=probe.sync();assert.equal(scans,130);resume();assert.strictEqual(await pending,completed);assert.equal(scans,130);
            assert.equal(completed.get(0).length,44);assert.equal(budget.pendingJobs,0);assert.equal(budget.pendingWaiters,0);
            assert(budget.estimatedIndexBytes>0&&budget.estimatedIndexBytes<100000);
          }
          const bounded=D.Snapshot.load(tracked,{mode:'LazyRowsOnAccess',tables:['Lookup'],limits:new R.ResourceLimits({maxDecodedRows:1})});
          const one=bounded.get_Lookup().load(10);code('BudgetExceeded',()=>bounded.get_Lookup().load(20));
          assert.equal(bounded.health,'Healthy');assert.equal(bounded.loadedRowCount,1);assert.equal(bounded.get_Lookup().get(10),one);
          assert(bounded.retainedRawBytes>0);assert.equal(bounded.manifestDigest.length,64);
          const scheduler=new R.NodeLoadScheduler();
          const asynchronously=await D.Snapshot.loadAsync(root,{tables:['Entry']},new R.Cancellation(),scheduler);
          assert.equal(asynchronously.loadedRowCount,4);assert.equal(asynchronously.get_Entry().get('east',1).load_Lookup().Id,10);
          const lazyAsync=await D.Snapshot.loadAsync(fileSource,{mode:'LazyRowsStrict',tables:['Lookup']},new R.Cancellation(),scheduler);
          let run,submissions=0;
          const gated={execute:work=>{submissions++;return new Promise((resolve,reject)=>{run=()=>{try{resolve(work());}catch(error){reject(error);}};});},yield:token=>Promise.resolve(token.check())};
          const cancelledRow=new R.Cancellation(),liveRow=new R.Cancellation();
          const firstRow=lazyAsync.get_Lookup().loadAsync(10,cancelledRow,gated),secondRow=lazyAsync.get_Lookup().loadAsync(10,liveRow,gated);
          cancelledRow.cancel();await assert.rejects(firstRow,e=>e.code==='Cancelled');assert.equal(submissions,1);assert.equal(lazyAsync.loadedRowCount,0);
          run();assert.equal(await secondRow,lazyAsync.get_Lookup().get(10));assert.equal(lazyAsync.mergedRowRequests,1);
          const rejected=lazyAsync.get_Lookup().loadAsync(20,liveRow,{execute:()=>Promise.reject(new Error('scheduler unavailable')),yield:gated.yield});
          await assert.rejects(rejected,/scheduler unavailable/);assert.equal(lazyAsync.health,'Healthy');assert.equal(lazyAsync.get_Lookup().load(20).Id,20);
          const early=new R.Cancellation();let asyncReads=0;
          await assert.rejects(D.Snapshot.loadAsync({readAsync:async(name,max,token)=>{asyncReads++;early.cancel();return fileSource.read(name,max,new R.Cancellation());}},{tables:['Lookup']},early,scheduler),e=>e.code==='Cancelled');
          assert.equal(asyncReads,1);
          code('InvalidPlan',()=>fileSource.read('current.json',NaN,new R.Cancellation()));
          const captured=new Map();const stable=D.Snapshot.load({read:(name,max,token)=>{const bytes=fileSource.read(name,max,token);captured.set(name,bytes);return bytes;}},{mode:'LazyRowsOnAccess',tables:['Lookup']});
          const memory=new R.MemoryConfigSource(captured);for(const bytes of captured.values())bytes.fill(0);
          assert.equal(stable.get_Lookup().load(10).Name,'目标');assert.equal((await D.Snapshot.loadAsync(memory,{tables:['Lookup']},new R.Cancellation(),scheduler)).loadedRowCount,2);
          const cancellableSourceToken=new R.Cancellation();let captureToken;
          code('Cancelled',()=>D.Snapshot.load({read:(name,max,token)=>{if(name!=='current.json'){captureToken=token;cancellableSourceToken.cancel();}return fileSource.read(name,max,token);}},{tables:['Lookup']},cancellableSourceToken));assert.equal(captureToken,cancellableSourceToken);
          const many=D.Snapshot.load(root,{mode:'LazyRowsOnAccess',tables:['Lookup'],limits:new R.ResourceLimits({maxPendingRequests:2})});let pendingRun;
          const held={execute:work=>new Promise((resolve,reject)=>pendingRun=()=>{try{resolve(work());}catch(error){reject(error);}}),yield:gated.yield};
          for(let i=0;i<100;i++){const token=new R.Cancellation(),waiting=many.get_Lookup().loadAsync(10,token,held);token.cancel();await assert.rejects(waiting,error=>error.code==='Cancelled');assert.equal(many.diagnostics().pendingWaiters,0);}
          const survivor=many.get_Lookup().loadAsync(10,new R.Cancellation(),held);assert.equal(many.diagnostics().pendingJobs,1);pendingRun();assert.equal(await survivor,many.get_Lookup().get(10));assert.equal(many.diagnostics().pendingJobs,0);assert.equal(many.diagnostics().pendingWaiters,0);
          const history=D.manager();history.reload(()=>D.Snapshot.load(root,{tables:['Entry'],sourceNamespace:'deployment'}),new R.Cancellation());const lastGood=history.acquireSnapshot();
          code('VersionIdentityConflict',()=>history.reload(()=>D.Snapshot.load(root+'-missing-ref',{mode:'LazyRowsOnAccess',tables:['Entry'],preferJSON:true,sourceNamespace:'deployment'}),new R.Cancellation()));assert.equal(history.acquireSnapshot(),lastGood);
          {
            code('InvalidPlan',()=>new D.EntryRow({},lastGood));code('InvalidPlan',()=>new D.EntryTable({},lastGood));
            assert.equal(D.Snapshot.load(root,{tables:[]}).sourceNamespace,path.resolve(root));
            const bounded=new R.ServerManager({maxPendingPreparations:1});let release;const held=new Promise(resolve=>release=resolve);let entered;
            const started=new Promise(resolve=>entered=resolve);const pending=bounded.prepareAsyncWithCancellation(async operation=>{entered();await held;operation.check();return {};},new R.Cancellation());await started;bounded.clear();
            code('LimitExceeded',()=>bounded.prepare(()=>({}),new R.Cancellation()));release();await assert.rejects(pending,e=>e.code==='Cancelled');assert.equal(bounded.diagnostics().activePreparations,0);bounded.reload(()=>({}),new R.Cancellation());
            const slow=new R.ServerManager();const failureEvents=[];slow.subscribe(event=>{if(event.kind==='PreparationFailed'){assert.equal(slow.diagnostics().activePreparations,0);assert.equal(slow.status().preparingOperation,0n);slow.prepare(()=>({}),new R.Cancellation()).discard();failureEvents.push(event.code);throw new Error('Isolated failure listener');}});const hostFailure=new Error('original host failure');assert.throws(()=>slow.prepare(()=>{throw hostFailure;},new R.Cancellation()),error=>error===hostFailure);await assert.rejects(slow.prepareAsyncWithCancellation(async operation=>{await new Promise(resolve=>setTimeout(resolve,10));operation.check();return {};},new R.Cancellation(),{timeoutMillis:1,candidateLifetimeMillis:100}),e=>e.code==='DeadlineExceeded');assert.equal(slow.diagnostics().activePreparations,0);
            assert.deepEqual(failureEvents,['InternalError','DeadlineExceeded']);assert.equal(slow.diagnostics().callbackFailures,2);
            const waiting=D.Snapshot.load(root,{mode:'LazyRowsOnAccess',tables:['Lookup']});let run;const scheduler={execute:work=>new Promise(resolve=>run=()=>resolve(work())),yield:async token=>token.check()};
            const deadline=new R.Cancellation().child(performance.now()+10);const timed=waiting.get_Lookup().loadAsync(10,deadline,scheduler);await assert.rejects(timed,e=>e.code==='DeadlineExceeded');assert.equal(waiting.diagnostics().pendingWaiters,0);const survivor=waiting.get_Lookup().loadAsync(10,new R.Cancellation(),scheduler);run();assert.equal((await survivor).Id,10);
            function candidateOnly(){const manager=new R.ServerManager();const candidate=manager.prepare(()=>({}),new R.Cancellation());return {candidate,weak:new WeakRef(manager)};}
            const retained=candidateOnly();for(let i=0;i<20;i++){await new Promise(resolve=>setImmediate(resolve));global.gc?.();}assert.equal(retained.weak.deref(),undefined,'candidate retained manager');assert.deepEqual(retained.candidate.snapshot,{});retained.candidate.discard();
          }
          {
            const memory=new R.ServerManager({maxEstimatedLiveBytes:100,liveEstimator:()=>64});let retained=memory.reload(()=>({}),new R.Cancellation()).snapshot;const weak=new WeakRef(retained);memory.clear();assert.equal(memory.estimatedLiveBytes(),64);code('BudgetExceeded',()=>memory.prepare(()=>({}),new R.Cancellation()));assert.ok(retained);retained=undefined;
            for(let i=0;i<20;i++){await new Promise(resolve=>setImmediate(resolve));global.gc();}assert.equal(weak.deref(),undefined);assert.equal(memory.estimatedLiveBytes(),0);
            let estimate=1;const growing=new R.ServerManager({maxEstimatedLiveBytes:1,liveEstimator:()=>estimate});const candidate=growing.prepare(()=>({}),new R.Cancellation());estimate=2;code('BudgetExceeded',()=>growing.commit(candidate,new R.Cancellation()));assert.equal(growing.tryAcquireSnapshot(),undefined);assert.equal(growing.estimatedLiveBytes(),2);candidate.discard();
            let clear=false;const reentrant=new R.ServerManager({maxEstimatedLiveBytes:100,liveEstimator:()=>{if(clear){clear=false;reentrant.clear();}return 1;}});const interrupted=reentrant.prepare(()=>({}),new R.Cancellation());clear=true;code('StaleCandidate',()=>reentrant.commit(interrupted,new R.Cancellation()));assert.equal(reentrant.tryAcquireSnapshot(),undefined);interrupted.discard();
            code('InvalidPlan',()=>new R.ServerManager({maxEstimatedLiveBytes:100}));code('UnsupportedCapability',()=>new R.ServerManager({maxEstimatedLiveBytes:100,liveEstimator:()=>1}).prepare(()=>1,new R.Cancellation()));
          }
          const observed=D.manager({eventCapacity:2}),events=[];
          const subscription=observed.subscribe(event=>{events.push(event);if(event.kind==='Committed')for(let i=0;i<10;i++)observed.clear();});
          const faulty=observed.subscribe(()=>{throw new Error('listener failure');});
          const published=observed.reload(()=>lastGood,new R.Cancellation());
          assert.equal(published.generation,1n);assert.equal(observed.tryAcquireSnapshot(),undefined);assert.deepEqual(events.map(e=>e.generation),[1n,10n,11n]);assert.equal(observed.droppedEventCount,8);assert.equal(observed.listenerFailureCount,3);subscription.dispose();faulty.dispose();
          let validations=0;const gatedManager=D.manager({validators:[()=>{validations++;if(validations===2)throw new R.ConfigError('InvalidValue','business gate');}]});
          gatedManager.reload(()=>lastGood,new R.Cancellation());code('InvalidValue',()=>gatedManager.reload(()=>asynchronously,new R.Cancellation()));assert.equal(gatedManager.acquireSnapshot(),lastGood);
          code('UnsupportedCapability',()=>D.manager({validators:[()=>Promise.resolve()]}).reload(()=>lastGood,new R.Cancellation()));
          const warmed=D.Snapshot.load(root,{mode:'LazyRowsStrict',tables:['Entry'],warmupTables:['Lookup']});
          assert.equal(warmed.loadedRowCount,2);assert.equal(warmed.get_Entry().loadedCount,0);
          code('InvalidPlan',()=>D.Snapshot.load('/missing',{tables:['Unknown']}));
          code('InvalidPlan',()=>D.Snapshot.load('/missing',{tables:['Lookup'],warmupTables:['Concrete']}));
          for(const json of [false,true]){
            const selected=S.SelectedSnapshot.load(root+'-selected',['Entry'],json);
            assert.deepEqual(selected.selection.tables,['Entry','Lookup']);
            assert.equal(selected.get_Entry().get('east',1).Lookup,selected.get_Lookup().get(10));
            code('TableNotSelected',()=>selected.get_Concrete());
            code('SchemaMismatch',()=>S.SelectedSnapshot.load(root+'-selection-schema',['Entry'],json));
          }
          for(const json of [false,true]){
            const manager=S.manager(),make=()=>S.load(root,json);
            code('NotInitialized',()=>manager.acquireSnapshot());assert.equal(manager.tryAcquireSnapshot(),undefined);
            const prepared=manager.prepare(make,token);assert.equal(manager.tryAcquireSnapshot(),undefined);
            code('ReloadBusy',()=>manager.prepare(make,token));code('ForeignCandidate',()=>S.manager().commit(prepared,token));
            const result=manager.commit(prepared,token);assert.equal(result.generation,1n);assert.equal(result.committed,true);assert.equal(manager.stageTimings().length,2);for(const timing of manager.stageTimings()){assert.equal(timing.succeeded,true);assert.equal(timing.operationId,result.operationId);assert.ok(timing.elapsedMilliseconds>=0);}
            code('AlreadyCommitted',()=>manager.commit(prepared,token));
            const row=result.snapshot.Entry.get('east',1),group=result.snapshot.Entry.getById(1);
            const pending=manager.prepare(make,token);manager.clear();code('StaleCandidate',()=>manager.commit(pending,token));
            manager.reload(make,token);manager.close();manager.close();code('ManagerClosed',()=>manager.acquireSnapshot());
            prepared.discard();pending.discard();assert.equal(row.Name,'中文😀');assert.equal(group.length,2);assert.equal(row.Lookup.Peer.Peer,row.Lookup);
          }
          const manager=S.manager();let entered,resume;
          const started=new Promise(resolve=>entered=resolve),gate=new Promise(resolve=>resume=resolve);
          const loading=manager.prepareAsync(async()=>{entered();await gate;return S.load(root);},token);
          await started;code('ReloadBusy',()=>manager.prepare(()=>S.load(root),token));
          manager.clear();const fresh=manager.prepare(()=>S.load(root),token),operation=manager.status().preparingOperation;
          resume();await assert.rejects(loading,e=>e.code==='StaleCandidate');assert.equal(manager.status().preparingOperation,operation);
          const cancelled=new R.Cancellation();cancelled.cancel();code('Cancelled',()=>manager.commit(fresh,cancelled));
          const result=manager.commit(fresh,token);token.cancel();assert.equal(manager.acquireSnapshot(),result.snapshot);
          manager.close();fresh.discard();
          console.log('通过：TypeScript 服务器生命周期、安全行/分组、异步候选与取消竞态。');
        })().catch(e=>{console.error(e);process.exitCode=1;});
        """;
    internal const string Lua = """
        package.path=arg[1]..'/?.lua;'..package.path
        local S,R,D=require('server'),require('runtime'),require('data');local now=0;local function clock()return now end
        local function code(expected,work)local ok,e=pcall(work);assert(not ok and type(e)=='table' and e.code==expected,tostring(e))end
        local token=S.cancellation()
        do
          for _,json in ipairs({false,true})do local snapshot=D.load_with_warmup(arg[2],{mode='LazyRowsOnAccess',tables={'Entry'},prefer_json=json},{D.warmup_row_Lookup(10),D.warmup_group_Entry_ById(1),D.warmup_row_Lookup(10)},token)
            assert(snapshot.loaded_row_count()==3 and snapshot.get_Entry().get('east',1).get_Lookup()==snapshot.get_Lookup().get(10));code('RowNotLoaded',function()snapshot.get_Lookup().get(20)end)
          end
          local reads=0;code('InvalidPlan',function()D.load_source_with_warmup({read=function()reads=reads+1;error('must not read')end},{tables={'Lookup'}},{D.warmup_row_Entry('east',1)},token)end);assert(reads==0)
        end
        do
          for _,json in ipairs({'{}','{"tables":[null],"format":"ZrConfig.Binary/1","versionId":"v1"}',''})do code('InvalidManifest',function()D.load_source(R.memory_source({['current.json']=json}))end)end
          code('InvalidPlan',function()R.resource_limits({max_rows=0})end);code('UnsupportedCapability',function()R.resource_limits({max_depth=65})end)
          local file=R.file_source(arg[2]);code('InvalidPlan',function()file.read('../current.json',1<<30,token)end);code('LimitExceeded',function()file.read('current.json',1,token)end)
          local reads=0;local source={capabilities={verified_paths=false},read=function(...)reads=reads+1;return file.read(...)end}
          code('UnsupportedCapability',function()D.load_source(source,{require_verified_paths=true})end);assert(reads==0)
          code('LimitExceeded',function()D.load_source({read=function()return 'xx'end},{limits={max_manifest_bytes=1}})end)
          local manifest=file.read('current.json',1<<30,token);local version=R.parse(manifest).versionId;local path='Lookup.bytes'
          local files={['current.json']=manifest,[path]=file.read(path,1<<30,token)};local memory=R.memory_source(files);files[path]='corrupt'
          code('LimitExceeded',function()D.load_source(file,{mode='LazyRowsStrict',tables={'Lookup'},limits={max_estimated_decoded_bytes=1}},token)end)
          for _,json in ipairs({false,true})do local capped=D.load_source(file,{mode='LazyRowsOnAccess',tables={'Entry'},prefer_json=json,limits={max_estimated_index_bytes=1}},token);for i=1,2 do code('BudgetExceeded',function()capped.get_Entry().load_group_ById(1,token)end)end;assert(capped.diagnostics().estimated_index_bytes==0 and capped.loaded_row_count()==0)
            local indexed=D.load_source(file,{mode='LazyRowsOnAccess',tables={'Entry'},prefer_json=json},token);indexed.get_Entry().load_group_ById(1,token);local estimate=indexed.diagnostics().estimated_index_bytes;indexed.get_Entry().load_group_ById(1,token);assert(estimate>0 and indexed.diagnostics().estimated_index_bytes==estimate)end
          do local broken=D.load(arg[2]..'-json-field-array',{mode='LazyRowsOnAccess',tables={'Entry'},prefer_json=true},token);code('InvalidValue',function()broken.get_Entry().load_group_ById(1,token)end);assert(broken.diagnostics().estimated_index_bytes==0)end
          local source_weak=setmetatable({memory},{__mode='v'});local snapshot=D.load_source(memory,{mode='LazyRowsOnAccess',tables={'Lookup'},limits={max_decoded_rows=1},source_namespace='memory-test',require_verified_paths=true});memory=nil;collectgarbage('collect');assert(source_weak[1]==nil,'snapshot retained source')
          assert(snapshot.source_namespace=='memory-test'and #snapshot.manifest_digest==64);local diagnostics=snapshot.diagnostics();assert(diagnostics.declared_rows==2 and diagnostics.decoded_rows==0 and diagnostics.read_bytes>diagnostics.retained_raw_bytes)
          local row=snapshot.get_Lookup().load(10);assert(row.Id==10);for i=1,2 do code('LimitExceeded',function()snapshot.get_Lookup().load(20)end)end
          diagnostics=snapshot.diagnostics();assert(diagnostics.decoded_rows==1 and diagnostics.active_decodes==0 and diagnostics.pending_jobs==0 and diagnostics.faulted_rows==0 and diagnostics.health=='Healthy')
          code('LimitExceeded',function()D.load_source(file,{tables={'Lookup'},limits={max_rows=1}})end)
          for _,item in ipairs({{'array','Values[1]'},{'matrix','Shape[2]'},{'dictionary','Mapping[0].value'},{'polymorphic','Action.Amount'}})do local malformed=D.load(arg[2]..'-json-field-'..item[1],{mode='LazyRowsOnAccess',tables={'Entry'},prefer_json=true},token);local ok,failure=pcall(function()malformed.get_Entry().load('east',1,token)end);assert(not ok and failure.code=='InvalidValue'and failure.field_path==item[2],tostring(failure))end
          code('UnsupportedCapability',function()R.catalog("{\"format\":2,\"runtimeContract\":\"server-v1\",\"requiredCapabilities\":[\"future-storage\"],\"fingerprint\":\"test\",\"tables\":[],\"components\":[]}")end)
          for _,kind in ipairs({'wrong-target','missing-target'})do code('CatalogMismatch',function()D.load(arg[2]..'-'..kind,{},token)end)end
          do local ok,failure=pcall(function()D.load(arg[2],{limits={max_tables=1}},token)end);assert(not ok and failure.code=='LimitExceeded'and failure.stage=='parse')end
          for _,kind in ipairs({'string','members','array'})do local ok,failure=pcall(function()D.load(arg[2]..'-json-'..kind,{tables={kind=='array'and'Entry'or'Lookup'},prefer_json=true,limits={max_container_elements=1,max_string_bytes=512}},token)end);assert(not ok and failure.code=='LimitExceeded'and failure.stage=='parse','JSON allocation check ran after parsing')end
          local host_error={programming_error=true};local ok,err=pcall(function()D.load_source({read=function()error(host_error)end})end);assert(not ok and err==host_error)
          local validations=0;local policy={identity=function(value)return {source_namespace='scope',version_id='version',manifest_digest=tostring(value.n)}end,validators={function(value,operation)operation.check();validations=validations+1;if value.n<0 then R.server_error('InvalidValue')end end}}
          local manager=S.manager(clock,policy);policy.validators[1]=function()error('must use copied validators')end
          manager.reload(function()return {n=1}end,token);local current=manager.acquire_snapshot()
          code('VersionIdentityConflict',function()manager.prepare(function()return {n=2}end,token)end);code('InvalidValue',function()manager.prepare(function()return {n=-1}end,token)end);assert(validations==3 and manager.acquire_snapshot()==current)
          manager.clear();code('VersionIdentityConflict',function()manager.prepare(function()return {n=2}end,token)end);assert(manager.diagnostics().retained_identities==1)
          local events=S.manager(clock,{event_capacity=2});local generations={};local listener=events.subscribe(function(event)generations[#generations+1]=event.generation;if event.kind=='Committed'and event.generation==1 then for i=1,10 do events.clear()end end end)
          local failure=events.subscribe(function()error('Expected isolated event failure')end);local committed=events.reload(function()return {}end,token);assert(committed.committed and table.concat(generations,',')=='1,10,11')
          diagnostics=events.diagnostics();assert(diagnostics.dropped_events==8 and diagnostics.callback_failures==3 and diagnostics.pending_events==0);assert(events.unsubscribe(listener)and events.unsubscribe(failure))
          local slow=S.manager(clock);local preparation_failures={};slow.subscribe(function(event)if event.kind=='PrepareFailed'then assert(slow.diagnostics().active_preparations==0 and slow.status().preparing_operation==0 and event.operation_id>0);preparation_failures[#preparation_failures+1]=event.code;error('Isolated failure listener')end end);code('DeadlineExceeded',function()slow.prepare_with_cancellation(function(operation)now=now+2;operation.check();return {}end,token,{timeout=1})end);assert(slow.diagnostics().active_preparations==0)
          ok,err=pcall(function()slow.prepare(function()error(host_error)end,token)end);assert(not ok and err==host_error and slow.status().preparing_operation==0);assert(table.concat(preparation_failures,',')=='DeadlineExceeded,InternalError'and slow.diagnostics().callback_failures==2)
          local bounded=S.manager(clock,{preparation_capacity=1});code('Cancelled',function()bounded.prepare_with_cancellation(function(operation)bounded.clear();code('LimitExceeded',function()bounded.prepare(function()return {}end,token)end);operation.check();return {}end,token)end)
          bounded.reload(function()return {}end,token);assert(bounded.diagnostics().active_preparations==0)
          local candidate=bounded.prepare(function()return {}end,token);candidate.discard();assert(bounded.status().preparing_operation==0)
          local preserved=candidate;local manager_weak=setmetatable({bounded},{__mode='v'});bounded=nil;collectgarbage('collect');collectgarbage('collect');assert(manager_weak[1]==nil,'candidate retained manager');preserved.discard()
          local memory=S.manager(clock,{max_estimated_live_bytes=100,live_estimator=function()return 64 end});local retained=memory.reload(function()return {}end,token).snapshot;local weak=setmetatable({retained},{__mode='v'});memory.clear();assert(memory.estimated_live_bytes()==64);code('BudgetExceeded',function()memory.prepare(function()return {}end,token)end);assert(retained);retained=nil;collectgarbage('collect');collectgarbage('collect');assert(weak[1]==nil and memory.estimated_live_bytes()==0)
          local estimate=1;local growing=S.manager(clock,{max_estimated_live_bytes=1,live_estimator=function()return estimate end});local growing_candidate=growing.prepare(function()return {}end,token);estimate=2;code('BudgetExceeded',function()growing.commit(growing_candidate,token)end);assert(growing.try_acquire_snapshot()==nil and growing.estimated_live_bytes()==2);growing_candidate.discard()
          local clear=false;local reentrant;reentrant=S.manager(clock,{max_estimated_live_bytes=100,live_estimator=function()if clear then clear=false;reentrant.clear()end;return 1 end});local interrupted=reentrant.prepare(function()return {}end,token);clear=true;code('StaleCandidate',function()reentrant.commit(interrupted,token)end);assert(reentrant.try_acquire_snapshot()==nil);interrupted.discard()
          print('通过：Lua 有界数据源、路径验证能力、预算、版本身份、校验器、事件隔离及候选释放。')
        end
        do
          local sink=0;local function probe(snapshot)local before=collectgarbage('count');local entry=snapshot.get_Entry();for i=1,100000 do local row=entry.get('east',1);local target=row.get_Lookup();assert(target.get_Peer().get_Peer()==target);sink=row.Id+#entry.find_group_ById(1)end;return (collectgarbage('count')-before)*1024 end
          for _,json in ipairs({false,true})do local snapshot=D.load(arg[2],{prefer_json=json});for i=1,5 do probe(snapshot)end;collectgarbage('collect');collectgarbage('stop');probe(snapshot);local allocated=probe(snapshot);collectgarbage('restart');assert(allocated==0 and sink==3,'new server hot allocation: '..allocated);print('通过：Lua 新服务器主键、分组及双向引用 100000 次查询分配 0 B。')end
        end
        for _,json in ipairs({false,true})do for _,mode in ipairs({'ServerDefault','SelectedTables','LazyRowsStrict','LazyRowsOnAccess'})do
          local snapshot=D.load(arg[2]..'-selected',{mode=mode,tables={'Entry'},prefer_json=json},token);assert(#snapshot.catalog_fingerprint==64);if mode=='LazyRowsOnAccess'then assert(snapshot.load_plan_fingerprint==(json and'c55af6527720622add35286774df8dd1cf6e8eda7537d04aa1ee2eef94f31d91'or'a3fe29f4e2fb7a9cc160a8a474905d5c40bb7f57179f9651f08a8967455785d1'))end;
          local entry,lookup=snapshot.get_Entry(),snapshot.get_Lookup();code('TableNotSelected',snapshot.get_Concrete)
          if mode=='LazyRowsStrict'or mode=='LazyRowsOnAccess'then
            assert(snapshot.completed_table_count()==0 and snapshot.loaded_row_count()==0 and entry.find('missing',99)==nil)
            code('RowNotLoaded',function()entry.find('east',1)end);code('TableNotLoaded',entry.rows)
            local row=entry.load('east',1,token);assert(snapshot.loaded_row_count()==1 and row.LookupId==10)
            code('RowNotLoaded',row.get_Lookup);local target=row.load_Lookup(token);assert(snapshot.loaded_row_count()==2)
            code('RowNotLoaded',target.get_Peer);local peer=target.load_Peer(token);assert(peer.get_Peer()==target and snapshot.loaded_row_count()==3 and lookup.get(10)==target)
            code('TableNotLoaded',function()entry.find_group_ById(1)end)
            local group=entry.load_group_ById(1,token);assert(#group==2 and group[1]==row and entry.find_group_ById(1)==group and snapshot.loaded_row_count()==4)
          end
          local row=entry.load('east',1,token);snapshot.load_all(token);assert(snapshot.completed_table_count()==2);assert(entry.get('east',1)==row and entry.rows()[1]==row and snapshot.loaded_row_count()==4)
          assert(row.load_Lookup().load_Peer().load_Peer()==row.get_Lookup());code('KeyNotFound',function()entry.get('missing',99)end)
        end end
        code('MissingReference',function()D.load(arg[2]..'-missing-ref',{mode='LazyRowsStrict',tables={'Entry'},prefer_json=true})end)
        local bad=D.load(arg[2]..'-missing-ref',{mode='LazyRowsOnAccess',tables={'Entry'},prefer_json=true})
        for i=1,2 do code('MissingReference',function()bad.get_Entry().load('east',1)end)end;local ok,failure=pcall(function()bad.get_Entry().load('east',1)end);assert(not ok and failure.table=='Entry'and failure.field_path=='Lookup'and failure.version_id~=''and failure.row_key~=''and failure.relative_path:match('Entry.json$'));assert(not pcall(function()failure.code='Changed'end));assert(bad.loaded_row_count()==0)
        code('InvalidPlan',function()D.load('/missing',{tables={'Unknown'}})end)
        local function retained()
          local snapshot=D.load(arg[2],{mode='LazyRowsStrict',tables={'Lookup'}})
          return snapshot.get_Lookup().load(10),setmetatable({snapshot},{__mode='v'})
        end
        local held,weak=retained();collectgarbage('collect');assert(weak[1]~=nil and held.load_Peer().load_Peer()==held)
        held=nil;collectgarbage('collect');collectgarbage('collect');assert(weak[1]==nil,'server row graph leaked')
        for _,json in ipairs({false,true})do
          local selected=S.select(arg[2]..'-selected',{'Entry'},json)
          assert(#selected.selection.tables==2 and selected.get_Entry().get('east',1).Lookup==selected.get_Lookup().get(10))
          code('TableNotSelected',selected.get_Concrete)
          code('SchemaMismatch',function()S.select(arg[2]..'-selection-schema',{'Entry'},json)end)
        end
        for _,json in ipairs({false,true})do
          local manager=S.manager(clock);local function make()return S.load(arg[2],json)end
          code('NotInitialized',manager.acquire_snapshot);assert(manager.try_acquire_snapshot()==nil)
          local prepared=manager.prepare(make,token);assert(manager.try_acquire_snapshot()==nil)
          code('ReloadBusy',function()manager.prepare(make,token)end)
          code('ForeignCandidate',function()S.manager(clock).commit(prepared,token)end)
          local result=manager.commit(prepared,token);assert(result.committed and result.generation==1);for _,timing in ipairs(manager.stage_timings())do assert(timing.succeeded and timing.operation_id==result.operation_id and timing.elapsed_milliseconds>=0)end
          code('AlreadyCommitted',function()manager.commit(prepared,token)end)
          local row=result.snapshot.Entry.get('east',1);local group=result.snapshot.Entry.getById(1)
          local pending=manager.prepare(make,token);manager.clear();code('StaleCandidate',function()manager.commit(pending,token)end)
          manager.reload(make,token);manager.close();manager.close();code('ManagerClosed',manager.acquire_snapshot)
          prepared.discard();pending.discard();assert(row.Name=='中文😀' and #group==2 and row.Lookup.Peer.Peer==row.Lookup)
        end
        local manager=S.manager(clock);local fresh
        code('StaleCandidate',function()manager.prepare(function()
          manager.clear();fresh=manager.prepare(function()return S.load(arg[2],false)end,token)
          return S.load(arg[2],true)
        end,token)end)
        assert(manager.status().preparing_operation~=0)
        local cancelled=S.cancellation();cancelled.cancel();code('Cancelled',function()manager.commit(fresh,cancelled)end)
        local result=manager.commit(fresh,token);token.cancel();assert(manager.acquire_snapshot()==result.snapshot);fresh.discard();manager.close()
        local expiry=S.manager(clock);local candidate=expiry.prepare(function()return {}end,S.cancellation(),{timeout=1,candidate_lifetime=1});now=now+2
        code('DeadlineExceeded',function()expiry.commit(candidate,S.cancellation())end);candidate.discard()
        print('通过：Lua 服务器生命周期、安全行/分组、重入/迟到候选、取消与宿主单调期限。')
        """;

    internal const string Cpp = """
        #include "server.hpp"
        #include "data.hpp"
        #include <condition_variable>
        #include <future>
        #include <iostream>
        #include <thread>
        #include <cstdlib>
        #include <new>
        static volatile std::int32_t server_sink;
        // Sanitizers own new/delete interception. Allocation counts are verified
        // by the ordinary executable, which runs before each sanitized executable.
        #ifndef ZRCONFIG_SANITIZED_TEST
        static std::atomic<std::size_t> server_allocations{0};
        #if defined(__GNUC__)
        __attribute__((noinline))
        #endif
        void* operator new(std::size_t n){server_allocations.fetch_add(1,std::memory_order_relaxed);if(auto p=std::malloc(n?n:1))return p;throw std::bad_alloc();}
        // Keep replacement allocation/deallocation boundaries visible to GCC.
        #if defined(__GNUC__)
        __attribute__((noinline))
        #endif
        void operator delete(void* p)noexcept{std::free(p);}void* operator new[](std::size_t n){return ::operator new(n);}void operator delete[](void* p)noexcept{::operator delete(p);}
        void operator delete(void* p,std::size_t)noexcept{::operator delete(p);}
        void operator delete[](void* p,std::size_t)noexcept{::operator delete[](p);}
        #endif
        using namespace zrconfig;
        void check(bool value) { if (!value) throw std::runtime_error("server assertion failed"); }
        template<class F> void fails(ErrorCode code, F&& f) { try { f(); } catch (const ConfigError& e) { check(e.code == code); return; } throw std::runtime_error("expected server error"); }
        struct Tracked { static std::atomic<int> live; Tracked(){++live;} ~Tracked(){--live;} };
        std::atomic<int> Tracked::live{0};
        int main(int, char** argv) {
          try {
            Cancellation token;
            for(const auto& item:std::vector<std::pair<std::string,std::string>>{{"array","Values[1]"},{"matrix","Shape[2]"},{"dictionary","Mapping[0].value"},{"polymorphic","Action.Amount"}}){LoadPlan plan;plan.mode=LoadMode::LazyRowsOnAccess;plan.tables=std::vector<std::string>{"Entry"};plan.prefer_json=true;auto malformed=data::Snapshot::load(std::string(argv[1])+"-json-field-"+item.first,plan,token);try{malformed->load_Entry("east",1,token);check(false);}catch(const ConfigError& error){check(error.code==ErrorCode::InvalidValue&&error.context.field_path==item.second);}}
            fails(ErrorCode::UnsupportedCapability,[]{Catalog catalog("{\"format\":2,\"runtimeContract\":\"server-v1\",\"requiredCapabilities\":[\"future-storage\"],\"fingerprint\":\"test\",\"tables\":[],\"components\":[]}");});
            for(const std::string kind:{"wrong-target","missing-target"})fails(ErrorCode::CatalogMismatch,[&]{data::Snapshot::load(std::string(argv[1])+"-"+kind,LoadPlan{},token);});
            {FileConfigSource source(argv[1]);try{data::Snapshot::load(source,LoadSettings{}.with_limits(ResourceLimits::builder().max_tables(1).build()),token);check(false);}catch(const ConfigError& error){check(error.code==ErrorCode::LimitExceeded&&error.stage=="parse");}}
            for(const std::string kind:{"string","members","array"}){LoadPlan plan;plan.tables=std::vector<std::string>{kind=="array"?"Entry":"Lookup"};plan.prefer_json=true;auto limits=ResourceLimits::builder().max_container_elements(1).max_string_bytes(512).build();FileConfigSource source(std::string(argv[1])+"-json-"+kind);try{data::Snapshot::load(source,LoadSettings(plan).with_limits(limits),token);check(false);}catch(const ConfigError& error){check(error.code==ErrorCode::LimitExceeded&&error.stage=="parse");}}
            {
              LoadPlan plan;plan.tables=std::vector<std::string>{"Entry"};plan.prefer_json=true;plan.mode=LoadMode::LazyRowsStrict;
              auto badRoot=std::string(argv[1])+"-missing-ref";fails(ErrorCode::MissingReference,[&]{data::Snapshot::load(badRoot,plan,token);});
              plan.mode=LoadMode::LazyRowsOnAccess;auto bad=data::Snapshot::load(badRoot,plan,token);
              for(int i=0;i<2;++i)fails(ErrorCode::MissingReference,[&]{bad->load_Entry("east",1,token);});
              try{bad->load_Entry("east",1,token);check(false);}catch(const ConfigError& error){check(error.context.table=="Entry"&&error.context.field_path=="Lookup"&&!error.context.row_key.empty()&&!error.context.version_id.empty()&&!error.context.relative_path.empty());}check(bad->loaded_row_count()==0);
            }
            {
              for(const std::string json:{"{}","{\"tables\":[null],\"format\":\"ZrConfig.Binary/1\",\"versionId\":\"v1\"}",""}){MemoryConfigSource source({{"current.json",std::vector<std::uint8_t>(json.begin(),json.end())}});fails(ErrorCode::InvalidManifest,[&]{data::Snapshot::load(source,LoadSettings{},token);});}
              fails(ErrorCode::InvalidPlan,[]{ResourceLimits::builder().max_rows(0).build();});fails(ErrorCode::UnsupportedCapability,[]{ResourceLimits::builder().max_depth(65).build();});
              FileConfigSource file(argv[1]);fails(ErrorCode::InvalidPlan,[&]{file.read("../current.json",1ull<<30,token);});fails(ErrorCode::LimitExceeded,[&]{file.read("current.json",1,token);});
              {LoadPlan plan;plan.mode=LoadMode::LazyRowsStrict;plan.tables=std::vector<std::string>{"Lookup"};fails(ErrorCode::LimitExceeded,[&]{data::Snapshot::load(file,LoadSettings(plan).with_limits(ResourceLimits::builder().max_estimated_decoded_bytes(1).build()),token);});}
              for(bool json:{false,true}){LoadPlan plan;plan.mode=LoadMode::LazyRowsOnAccess;plan.tables=std::vector<std::string>{"Entry"};plan.prefer_json=json;auto bounded=data::Snapshot::load(file,LoadSettings(plan).with_limits(ResourceLimits::builder().max_estimated_index_bytes(1).build()),token);for(int i=0;i<2;++i)fails(ErrorCode::BudgetExceeded,[&]{bounded->load_group_Entry_ById(1,token);});check(bounded->diagnostics().estimated_index_bytes==0&&bounded->loaded_row_count()==0&&bounded->diagnostics().pending_jobs==0);
                auto indexed=data::Snapshot::load(file,LoadSettings(plan),token);indexed->load_group_Entry_ById(1,token);auto estimate=indexed->diagnostics().estimated_index_bytes;indexed->load_group_Entry_ById(1,token);check(estimate>0&&indexed->diagnostics().estimated_index_bytes==estimate);}
              {LoadPlan plan;plan.mode=LoadMode::LazyRowsOnAccess;plan.tables=std::vector<std::string>{"Entry"};plan.prefer_json=true;auto broken=data::Snapshot::load(std::string(argv[1])+"-json-field-array",plan,token);fails(ErrorCode::InvalidValue,[&]{broken->load_group_Entry_ById(1,token);});check(broken->diagnostics().estimated_index_bytes==0&&broken->diagnostics().pending_jobs==0);}
              LoadPlan plan;plan.mode=LoadMode::LazyRowsOnAccess;plan.tables=std::vector<std::string>{"Lookup"};LoadSettings settings(plan);settings.with_limits(ResourceLimits::builder().max_decoded_rows(1).build()).with_source_namespace("memory-test");
              auto bytes=file.read("current.json",1ull<<30,token);auto manifest=parse_json(std::string(bytes.begin(),bytes.end()));std::string path="Lookup.bytes";
              std::unordered_map<std::string,std::vector<std::uint8_t>> files{{"current.json",bytes},{path,file.read(path,1ull<<30,token)}};
              std::shared_ptr<const data::Snapshot> snapshot;{MemoryConfigSource memory(files);for(auto& entry:files)std::fill(entry.second.begin(),entry.second.end(),std::uint8_t{0});snapshot=data::Snapshot::load(memory,settings,token);}
              check(snapshot->source_namespace()=="memory-test"&&snapshot->manifest_digest().size()==64);auto diagnostics=snapshot->diagnostics();check(diagnostics.declared_rows==2&&diagnostics.decoded_rows==0&&diagnostics.read_bytes>diagnostics.retained_raw_bytes);
              {auto row=snapshot->load_Lookup(10,token);check(row.Id()==10);for(int i=0;i<2;++i)fails(ErrorCode::LimitExceeded,[&]{snapshot->load_Lookup(20,token);});}
              diagnostics=snapshot->diagnostics();check(diagnostics.decoded_rows==1&&diagnostics.active_decodes==0&&diagnostics.pending_jobs==0&&diagnostics.faulted_rows==0&&std::string(diagnostics.health())=="Healthy");
              std::weak_ptr<const data::Snapshot> weak=snapshot;snapshot.reset();check(weak.expired());settings.with_limits(ResourceLimits::builder().max_rows(1).build());fails(ErrorCode::LimitExceeded,[&]{data::Snapshot::load(file,settings,token);});
              struct Oversized:ConfigSource{std::vector<std::uint8_t> read(const std::string&,std::uint64_t,const Cancellation&)override{return {1,2};}} oversized;
              settings.with_limits(ResourceLimits::builder().max_manifest_bytes(1).build());fails(ErrorCode::LimitExceeded,[&]{data::Snapshot::load(oversized,settings,token);});
            }
            {
              int validations=0;ManagerPolicy<int> policy;policy.with_identity([](const int& value){return SnapshotIdentity("scope","version",std::to_string(value));}).with_validator([&](const int& value,const Cancellation& token){token.check();++validations;if(value<0)throw ConfigError(ErrorCode::InvalidValue,"Rejected by host");});
              ServerManager<int> manager(policy);manager.reload([]{return std::make_shared<const int>(1);},token);auto current=manager.acquire_snapshot();
              fails(ErrorCode::VersionIdentityConflict,[&]{manager.prepare([]{return std::make_shared<const int>(2);},token);});fails(ErrorCode::InvalidValue,[&]{manager.prepare([]{return std::make_shared<const int>(-1);},token);});check(validations==3&&manager.acquire_snapshot()==current);
              manager.clear();fails(ErrorCode::VersionIdentityConflict,[&]{manager.prepare([]{return std::make_shared<const int>(2);},token);});check(manager.diagnostics().retained_identities==1);
              ManagerPolicy<int> event_policy;event_policy.with_event_capacity(2);ServerManager<int> events(event_policy);std::vector<std::uint64_t> generations;
              auto listener=events.subscribe([&](const ManagerEvent& event){generations.push_back(event.generation);if(event.kind==ManagerEventKind::Committed&&event.generation==1)for(int i=0;i<10;++i)events.clear();});auto failure=events.subscribe([](const ManagerEvent&){throw std::logic_error("Expected listener failure");});
              auto committed=events.reload([]{return std::make_shared<const int>(7);},token);check(committed.committed&&generations==std::vector<std::uint64_t>{1,10,11});auto diagnostics=events.diagnostics();check(diagnostics.dropped_events==8&&diagnostics.callback_failures==3&&diagnostics.pending_events==0);check(events.unsubscribe(listener)&&events.unsubscribe(failure));
              ServerManager<int> slow;std::vector<ErrorCode> preparation_failures;slow.subscribe([&](const ManagerEvent& event){if(event.kind==ManagerEventKind::PreparationFailed){check(slow.diagnostics().active_preparations==0&&slow.status().preparing_operation==0&&event.operation_id>0&&event.error.has_value());preparation_failures.push_back(*event.error);throw std::logic_error("Isolated failure listener");}});PreparationOptions options;options.timeout=std::chrono::milliseconds(1);fails(ErrorCode::DeadlineExceeded,[&]{slow.prepare_with_cancellation([](const Cancellation& operation){std::this_thread::sleep_for(std::chrono::milliseconds(5));operation.check();return std::make_shared<const int>(1);},token,options);});check(slow.diagnostics().active_preparations==0);
              bool propagated=false;try{slow.prepare([]()->std::shared_ptr<const int>{throw std::logic_error("host programming error");},token);}catch(const std::logic_error&){propagated=true;}check(propagated&&slow.status().preparing_operation==0&&slow.diagnostics().active_preparations==0);check(preparation_failures==std::vector<ErrorCode>{ErrorCode::DeadlineExceeded,ErrorCode::InternalError}&&slow.diagnostics().callback_failures==2);
              ServerManager<int>* observable_memory=nullptr;ManagerPolicy<int> live_policy;live_policy.with_estimated_live_budget(100,[&](const int&){if(observable_memory)observable_memory->status();return std::uint64_t{64};});ServerManager<int> memory(live_policy);observable_memory=&memory;
              auto held=memory.reload([]{return std::make_shared<const int>(7);},token).snapshot;check(memory.estimated_live_bytes()==64);memory.clear();
              fails(ErrorCode::BudgetExceeded,[&]{memory.prepare([]{return std::make_shared<const int>(8);},token);});check(memory.estimated_live_bytes()==64);held.reset();check(memory.estimated_live_bytes()==0);
              auto admitted=memory.prepare([]{return std::make_shared<const int>(9);},token);check(memory.estimated_live_bytes()==64);admitted.discard();check(memory.estimated_live_bytes()==0);
              ManagerPolicy<int> missing_estimator;missing_estimator.with_estimated_live_budget(100);fails(ErrorCode::UnsupportedCapability,[&]{ServerManager<int> invalid(missing_estimator);});
              std::uint64_t estimate=1;ManagerPolicy<int> growing_policy;growing_policy.with_estimated_live_budget(1,[&](const int&){return estimate;});ServerManager<int> growing(growing_policy);auto growing_candidate=growing.prepare([]{return std::make_shared<const int>(1);},token);estimate=2;fails(ErrorCode::BudgetExceeded,[&]{growing.commit(growing_candidate,token);});check(!growing.try_acquire_snapshot()&&growing.estimated_live_bytes()==2);growing_candidate.discard();
              ManagerPolicy<int> bounded_policy;bounded_policy.with_preparation_capacity(1);ServerManager<int> bounded(bounded_policy);std::promise<void> entered,resume;auto resumed=resume.get_future();
              auto worker=std::async(std::launch::async,[&]{fails(ErrorCode::Cancelled,[&]{bounded.prepare_with_cancellation([&](const Cancellation& operation){entered.set_value();resumed.wait();operation.check();return std::make_shared<const int>(1);},token);});});
              entered.get_future().wait();bounded.clear();fails(ErrorCode::LimitExceeded,[&]{bounded.prepare([]{return std::make_shared<const int>(2);},token);});resume.set_value();worker.get();bounded.reload([]{return std::make_shared<const int>(2);},token);check(bounded.diagnostics().active_preparations==0);
              std::cout<<"通过：C++ 有界数据源、资源预算、版本身份、校验器、事件隔离及准备任务准入。\n";
            }
            {
              for(bool json:{false,true}){LoadPlan plan;plan.mode=LoadMode::LazyRowsOnAccess;plan.tables=std::vector<std::string>{"Entry"};plan.prefer_json=json;auto snapshot=data::Snapshot::load_with_warmup(argv[1],plan,{data::Snapshot::warmup_row_Lookup(10),data::Snapshot::warmup_group_Entry_ById(1),data::Snapshot::warmup_row_Lookup(10)},token);check(snapshot->loaded_row_count()==3);auto entry=snapshot->get_Entry("east",1);auto target=entry.get_Lookup();auto expected=snapshot->get_Lookup(10);check(&target->borrow()==&expected.borrow());fails(ErrorCode::RowNotLoaded,[&]{snapshot->get_Lookup(20);});}
              struct Counting:ConfigSource{int reads=0;std::vector<std::uint8_t> read(const std::string&,std::uint64_t,const Cancellation&)override{++reads;throw std::logic_error("must not read");}} source;
              LoadPlan plan;plan.tables=std::vector<std::string>{"Lookup"};fails(ErrorCode::InvalidPlan,[&]{data::Snapshot::load_with_warmup(source,LoadSettings(plan),{data::Snapshot::warmup_row_Entry("east",1)},token);});check(source.reads==0);
            }
            for(bool json:{false,true}){
              LoadPlan plan;plan.prefer_json=json;auto snapshot=data::Snapshot::load(argv[1],plan,token);std::string key="east";
              #ifndef ZRCONFIG_SANITIZED_TEST
              auto before=server_allocations.load();
              #endif
              for(int i=0;i<100000;++i){auto row=snapshot->get_Entry(key,1);auto group=snapshot->find_group_Entry_ById(1);auto target=row.get_Lookup();auto peer=target->get_Peer();auto back=peer->get_Peer();auto expected=snapshot->get_Lookup(10);check(&back->borrow()==&expected.borrow());server_sink=row.Id()+static_cast<std::int32_t>(group.size());}
              #ifndef ZRCONFIG_SANITIZED_TEST
              check(server_allocations.load()==before);std::cout<<"通过：C++ 新服务器主键、分组及双向引用 100000 次查询分配 0 次。"<<std::endl;
              #else
              std::cout<<"通过：C++ sanitizer 下主键、分组及双向引用 100000 次查询。"<<std::endl;
              #endif
            }
            for(bool json:{false,true})for(auto mode:{LoadMode::ServerDefault,LoadMode::SelectedTables,LoadMode::LazyRowsStrict,LoadMode::LazyRowsOnAccess}){
              LoadPlan plan;plan.mode=mode;plan.tables=std::vector<std::string>{"Entry"};plan.prefer_json=json;
              auto snapshot=data::Snapshot::load(std::string(argv[1])+"-selected",plan,token);check(snapshot->catalog_fingerprint().size()==64);if(mode==LoadMode::LazyRowsOnAccess)check(snapshot->load_plan_fingerprint()==(json?"c55af6527720622add35286774df8dd1cf6e8eda7537d04aa1ee2eef94f31d91":"a3fe29f4e2fb7a9cc160a8a474905d5c40bb7f57179f9651f08a8967455785d1"));
              fails(ErrorCode::TableNotSelected,[&]{snapshot->get_Concrete(1);});
              if(mode==LoadMode::LazyRowsStrict||mode==LoadMode::LazyRowsOnAccess){
                check(snapshot->completed_table_count()==0&&snapshot->loaded_row_count()==0&&!snapshot->find_Entry("missing",99));
                fails(ErrorCode::RowNotLoaded,[&]{snapshot->find_Entry("east",1);});fails(ErrorCode::TableNotLoaded,[&]{snapshot->rows_Entry();});
                auto row=snapshot->load_Entry("east",1,token);check(snapshot->loaded_row_count()==1&&row.LookupId()==10);
                fails(ErrorCode::RowNotLoaded,[&]{row.get_Lookup();});auto target=row.load_Lookup(token);check(snapshot->loaded_row_count()==2);
                fails(ErrorCode::RowNotLoaded,[&]{target->get_Peer();});auto peer=target->load_Peer(token);auto back=peer->get_Peer();
                check(&back->borrow()==&target->borrow()&&snapshot->loaded_row_count()==3);
                fails(ErrorCode::TableNotLoaded,[&]{snapshot->find_group_Entry_ById(1);});auto group=snapshot->load_group_Entry_ById(1,token);auto member=group.at(0);
                check(group.size()==2&&&member.borrow()==&row.borrow()&&snapshot->loaded_row_count()==4);
              }
              auto row=snapshot->load_Entry("east",1,token);auto name=row.Name();snapshot->load_all(token);auto member=snapshot->rows_Entry().at(0);
              check(snapshot->completed_table_count()==2&&snapshot->loaded_row_count()==4&&&member.borrow()==&row.borrow()&&name.borrow()=="中文😀");
              std::weak_ptr<const data::Snapshot> weak=snapshot;snapshot.reset();check(!weak.expired());check(row.load_Lookup(token)->load_Peer(token)->load_Peer(token)->Id()==10);
            }
            {
              LoadPlan plan;plan.mode=LoadMode::LazyRowsStrict;plan.tables=std::vector<std::string>{"Lookup"};
              auto snapshot=data::Snapshot::load(argv[1],plan,token);std::vector<std::future<data::LookupRow>> futures;
              for(int i=0;i<32;++i)futures.push_back(std::async(std::launch::async,[&]{return snapshot->load_Lookup(10,token);}));
              auto first=futures[0].get();for(std::size_t i=1;i<futures.size();++i){auto row=futures[i].get();check(&row.borrow()==&first.borrow());}check(snapshot->loaded_row_count()==1);
            }
            for(bool json : {false,true}) {
              auto selected=server::Snapshot::load_selected(std::string(argv[1])+"-selected",{"Entry"},json);
              check(selected->selection().tables==std::vector<std::string>({"Entry","Lookup"}));
              auto row=selected->get_Entry("east",1);auto lookup=selected->get_Lookup(10);
              check(&row.reference(&EntryConfig::Lookup)->borrow()==&lookup.borrow());
              fails(ErrorCode::TableNotSelected,[&]{selected->get_Concrete(1);});
              fails(ErrorCode::UnsupportedCapability,[&]{selected->borrow();});
              fails(ErrorCode::SchemaMismatch,[&]{server::Snapshot::load_selected(std::string(argv[1])+"-selection-schema",{"Entry"},json);});
            }
            for (bool json : {false, true}) {
              server::Manager manager;
              fails(ErrorCode::NotInitialized, [&]{manager.acquire_snapshot();});
              check(!manager.try_acquire_snapshot());
              auto make = [&]{return server::Snapshot::load(argv[1], json);};
              auto prepared = manager.prepare(make, token);
              check(!manager.try_acquire_snapshot());
              fails(ErrorCode::ReloadBusy, [&]{manager.prepare(make, token);});
              server::Manager other;
              fails(ErrorCode::ForeignCandidate, [&]{other.commit(prepared, token);});
              auto result = manager.commit(prepared, token);
              for(const auto& timing:manager.stage_timings())check(timing.succeeded&&timing.operation_id==result.operation_id&&timing.elapsed_milliseconds>=0);
              check(result.committed && result.generation == 1);
              fails(ErrorCode::AlreadyCommitted, [&]{manager.commit(prepared, token);});
              auto row = result.snapshot->get_Entry("east", 1);
              auto name = row.select(&EntryConfig::Name);
              auto group = result.snapshot->group_Entry_ById(1);
              auto peer = row.reference(&EntryConfig::Lookup);
              check(peer && peer->reference(&LookupConfig::Peer)->reference(&LookupConfig::Peer)->borrow().Id() == peer->borrow().Id());
              check(&row.borrow() == &result.snapshot->borrow().Entry().get("east",1));
              auto pending = manager.prepare(make, token);
              manager.clear();
              check(!manager.try_acquire_snapshot());
              fails(ErrorCode::StaleCandidate, [&]{manager.commit(pending,token);});
              manager.reload(make,token);
              manager.close(); manager.close();
              fails(ErrorCode::ManagerClosed, [&]{manager.acquire_snapshot();});
              fails(ErrorCode::ManagerClosed, [&]{manager.prepare(make,token);});
              result.snapshot.reset(); prepared.discard(); pending.discard();
              auto member = group.at(0);
              check(name.borrow() == "中文😀" && group.size() == 2 && member.borrow().Id() == 1);
              check(peer->reference(&LookupConfig::Peer)->reference(&LookupConfig::Peer)->borrow().Id() == 10);
            }
            {
              ServerManager<Tracked> manager;
              auto make=[] { return std::make_shared<const Tracked>(); };
              { auto p=manager.prepare(make,token); check(Tracked::live==1); }
              check(Tracked::live==0 && manager.status().preparing_operation==0);
              auto p=manager.prepare(make,token);
              Cancellation cancelled; cancelled.cancel();
              fails(ErrorCode::Cancelled,[&]{manager.commit(p,cancelled);});
              check(!manager.try_acquire_snapshot());
              auto result=manager.commit(p,token);
              token.cancel(); check(manager.acquire_snapshot()==result.snapshot);
              manager.clear(); p.discard(); result.snapshot.reset(); check(Tracked::live==0);
            }
            {
              ServerManager<Tracked> manager; Cancellation active;
              std::promise<void> entered, resume; auto wait=resume.get_future();
              auto loading=std::async(std::launch::async,[&]{
                fails(ErrorCode::StaleCandidate,[&]{manager.prepare([&]{entered.set_value();wait.get();return std::make_shared<const Tracked>();},active);});
              });
              entered.get_future().get();
              fails(ErrorCode::ReloadBusy,[&]{manager.prepare([]{return std::make_shared<const Tracked>();},active);});
              manager.clear();
              auto fresh=manager.prepare([]{return std::make_shared<const Tracked>();},active);
              auto operation=manager.status().preparing_operation;
              resume.set_value();loading.get();
              check(manager.status().preparing_operation==operation);
              manager.commit(fresh,active);manager.close();fresh.discard();check(Tracked::live==0);
            }
            std::cout << "通过：C++ 服务器安全行/字符串/分组、引用环、候选归属/取消/清理/关闭及屏障竞态。" << std::endl;
          } catch(const std::exception& e) { std::cerr << e.what() << std::endl; return 1; }
        }
        """;

    internal const string Rust = """
        use std::sync::{Arc, atomic::{AtomicUsize,Ordering},mpsc};
        use std::path::Path;
        use zrconfig::runtime::{Cancellation,ErrorCode,ServerManager,PreparationOptions,ServerResult,LoadMode,LoadPlan};
        fn code<T>(result: ServerResult<T>, expected: ErrorCode) { match result { Err(e) => assert_eq!(e.code, expected), Ok(_) => panic!("expected server error") } }
        use std::alloc::{GlobalAlloc,Layout,System};
        static SERVER_ALLOCATIONS:AtomicUsize=AtomicUsize::new(0);struct Counter;
        unsafe impl GlobalAlloc for Counter{unsafe fn alloc(&self,l:Layout)->*mut u8{SERVER_ALLOCATIONS.fetch_add(1,Ordering::Relaxed);unsafe{System.alloc(l)}}unsafe fn alloc_zeroed(&self,l:Layout)->*mut u8{SERVER_ALLOCATIONS.fetch_add(1,Ordering::Relaxed);unsafe{System.alloc_zeroed(l)}}unsafe fn realloc(&self,p:*mut u8,l:Layout,n:usize)->*mut u8{SERVER_ALLOCATIONS.fetch_add(1,Ordering::Relaxed);unsafe{System.realloc(p,l,n)}}unsafe fn dealloc(&self,p:*mut u8,l:Layout){unsafe{System.dealloc(p,l)}}}
        #[global_allocator]static SERVER_ALLOCATOR:Counter=Counter;
        struct Tracked(Arc<AtomicUsize>);
        impl Drop for Tracked { fn drop(&mut self) { self.0.fetch_sub(1,Ordering::SeqCst); } }
        fn main() {
          let root=std::env::args().nth(1).unwrap();let root=Path::new(&root);
          let token=Cancellation::new();let options=PreparationOptions::default();
          {
            let bad_root=format!("{}-missing-ref",root.display());let bad_root=Path::new(&bad_root);
            let mut plan=LoadPlan{mode:LoadMode::LazyRowsStrict,tables:Some(vec!["Entry".to_owned()]),prefer_json:true,..LoadPlan::default()};
            code(zrconfig::data::Snapshot::load(bad_root,&plan,&token),ErrorCode::MissingReference);
            plan.mode=LoadMode::LazyRowsOnAccess;let bad=zrconfig::data::Snapshot::load(bad_root,&plan,&token).unwrap();
            for _ in 0..2{code(bad.load_Entry("east",1,&token),ErrorCode::MissingReference)}let error=match bad.load_Entry("east",1,&token){Ok(_)=>panic!("Missing cached error"),Err(error)=>error};assert_eq!(error.context.table,"Entry");assert_eq!(error.context.field_path,"Lookup");assert!(!error.context.version_id.is_empty()&&!error.context.row_key.is_empty()&&error.context.relative_path.ends_with("Entry.json"));assert_eq!(bad.loaded_row_count(),0);
          }
          {
            use zrconfig::runtime::{ResourceLimits,LoadSettings,ConfigSource,MemoryConfigSource,FileConfigSource};
            for json in ["{}",r#"{"tables":[null],"format":"ZrConfig.Binary/1","versionId":"v1"}"#,""]{
              let memory=MemoryConfigSource::new(std::collections::HashMap::from([("current.json".to_owned(),json.as_bytes().to_vec())])).unwrap();
              code(zrconfig::data::Snapshot::load_source(&memory,&LoadSettings::default(),&token),ErrorCode::InvalidManifest);
            }
            code(ResourceLimits::builder().max_rows(0).build(),ErrorCode::InvalidPlan);
            code(ResourceLimits::builder().max_depth(65).build(),ErrorCode::UnsupportedCapability);
            let file=FileConfigSource::new(root);code(file.read("../current.json",1<<30,&token),ErrorCode::InvalidPlan);
            let missing=file.read("missing.json",1<<20,&token).unwrap_err();assert!(std::error::Error::source(&missing).unwrap().is::<std::io::Error>());
            code(file.read("current.json",1,&token),ErrorCode::LimitExceeded);
            let plan=LoadPlan{mode:LoadMode::LazyRowsOnAccess,tables:Some(vec!["Lookup".to_owned()]),..LoadPlan::default()};
            {let plan=LoadPlan{mode:LoadMode::LazyRowsStrict,tables:Some(vec!["Lookup".to_owned()]),..LoadPlan::default()};code(zrconfig::data::Snapshot::load_source(&file,&LoadSettings::new(plan).with_limits(ResourceLimits::builder().max_estimated_decoded_bytes(1).build().unwrap()),&token),ErrorCode::LimitExceeded);}
            for json in [false,true]{let plan=LoadPlan{mode:LoadMode::LazyRowsOnAccess,tables:Some(vec!["Entry".to_owned()]),prefer_json:json,..LoadPlan::default()};let bounded=zrconfig::data::Snapshot::load_source(&file,&LoadSettings::new(plan.clone()).with_limits(ResourceLimits::builder().max_estimated_index_bytes(1).build().unwrap()),&token).unwrap();for _ in 0..2{code(bounded.load_group_Entry_ById(1,&token),ErrorCode::BudgetExceeded);}assert_eq!(bounded.diagnostics().unwrap().estimated_index_bytes,0);assert_eq!(bounded.loaded_row_count(),0);assert_eq!(bounded.diagnostics().unwrap().pending_jobs,0);
              let indexed=zrconfig::data::Snapshot::load_source(&file,&LoadSettings::new(plan),&token).unwrap();indexed.load_group_Entry_ById(1,&token).unwrap();let estimate=indexed.diagnostics().unwrap().estimated_index_bytes;indexed.load_group_Entry_ById(1,&token).unwrap();assert!(estimate>0);assert_eq!(indexed.diagnostics().unwrap().estimated_index_bytes,estimate);}
            {let plan=LoadPlan{mode:LoadMode::LazyRowsOnAccess,tables:Some(vec!["Entry".to_owned()]),prefer_json:true,..LoadPlan::default()};let broken=zrconfig::data::Snapshot::load(std::path::Path::new(&format!("{}-json-field-array",root.display())),&plan,&token).unwrap();code(broken.load_group_Entry_ById(1,&token),ErrorCode::InvalidValue);assert_eq!(broken.diagnostics().unwrap().estimated_index_bytes,0);assert_eq!(broken.diagnostics().unwrap().pending_jobs,0);}
            let settings=LoadSettings::new(plan.clone()).with_limits(ResourceLimits::builder().max_decoded_rows(1).build().unwrap()).with_source_namespace("memory-test");
            let mut buffers=std::collections::HashMap::new();buffers.insert("current.json".to_owned(),file.read("current.json",1<<30,&token).unwrap());
            let path=String::from("Lookup.bytes");buffers.insert(path.clone(),file.read(&path,1<<30,&token).unwrap());
            let memory=MemoryConfigSource::new(buffers).unwrap();let snapshot=zrconfig::data::Snapshot::load_source(&memory,&settings,&token).unwrap();drop(memory);
            assert_eq!(snapshot.source_namespace(),"memory-test");assert_eq!(snapshot.manifest_digest().len(),64);
            let diagnostics=snapshot.diagnostics().unwrap();assert_eq!(diagnostics.declared_rows,2);assert_eq!(diagnostics.decoded_rows,0);assert!(diagnostics.read_bytes>diagnostics.retained_raw_bytes);
            let row=snapshot.load_Lookup(10,&token).unwrap();for _ in 0..2{code(snapshot.load_Lookup(20,&token),ErrorCode::LimitExceeded);}
            let diagnostics=snapshot.diagnostics().unwrap();assert_eq!(diagnostics.decoded_rows,1);assert_eq!(diagnostics.active_decodes,0);assert_eq!(diagnostics.pending_jobs,0);assert_eq!(diagnostics.faulted_rows,0);assert_eq!(diagnostics.health(),"Healthy");
            assert_eq!(row.get_Id(),10);let weak=Arc::downgrade(&snapshot);drop(row);drop(snapshot);assert!(weak.upgrade().is_none());
            let limits=ResourceLimits::builder().max_rows(1).build().unwrap();code(zrconfig::data::Snapshot::load_source(&file,&LoadSettings::new(plan).with_limits(limits),&token),ErrorCode::LimitExceeded);
          }
          for(kind,field)in [("array","Values[1]"),("matrix","Shape[2]"),("dictionary","Mapping[0].value"),("polymorphic","Action.Amount")]{let plan=LoadPlan{mode:LoadMode::LazyRowsOnAccess,tables:Some(vec!["Entry".to_owned()]),prefer_json:true,..LoadPlan::default()};let malformed=zrconfig::data::Snapshot::load(std::path::Path::new(&format!("{}-json-field-{}",root.display(),kind)),&plan,&token).unwrap();match malformed.load_Entry("east",1,&token){Err(error)=>{assert_eq!(error.code,ErrorCode::InvalidValue);assert_eq!(error.context.field_path,field);},Ok(_)=>panic!("Invalid field accepted")}}
          code(zrconfig::runtime::Catalog::parse("{\"format\":2,\"runtimeContract\":\"server-v1\",\"requiredCapabilities\":[\"future-storage\"],\"fingerprint\":\"test\",\"tables\":[],\"components\":[]}"),ErrorCode::UnsupportedCapability);
          for kind in ["wrong-target","missing-target"]{code(zrconfig::data::Snapshot::load(std::path::Path::new(&format!("{}-{}",root.display(),kind)),&LoadPlan::default(),&token),ErrorCode::CatalogMismatch);}
          {use zrconfig::runtime::{FileConfigSource,LoadSettings,ResourceLimits};let source=FileConfigSource::new(root);let settings=LoadSettings::default().with_limits(ResourceLimits::builder().max_tables(1).build().unwrap());match zrconfig::data::Snapshot::load_source(&source,&settings,&token){Err(error)=>{assert_eq!(error.code,ErrorCode::LimitExceeded);assert_eq!(error.stage,"parse");},Ok(_)=>panic!("Manifest allocation budget accepted")}}
          for kind in ["string","members","array"]{use zrconfig::runtime::{ResourceLimits,LoadSettings,FileConfigSource};let plan=LoadPlan{tables:Some(vec![if kind=="array"{"Entry".to_owned()}else{"Lookup".to_owned()}]),prefer_json:true,..LoadPlan::default()};let limits=ResourceLimits::builder().max_container_elements(1).max_string_bytes(512).build().unwrap();let source=FileConfigSource::new(format!("{}-json-{}",root.display(),kind));match zrconfig::data::Snapshot::load_source(&source,&LoadSettings::new(plan).with_limits(limits),&token){Err(error)=>{assert_eq!(error.code,ErrorCode::LimitExceeded);assert_eq!(error.stage,"parse");},Ok(_)=>panic!("JSON allocation budget accepted")}}
          {
            use zrconfig::data::Snapshot;use zrconfig::runtime::{ConfigSource,LoadSettings};
            for json in [false,true]{let plan=LoadPlan{mode:LoadMode::LazyRowsOnAccess,tables:Some(vec!["Entry".to_owned()]),prefer_json:json,..LoadPlan::default()};let requests=[Snapshot::warmup_row_Lookup(10),Snapshot::warmup_group_Entry_ById(1),Snapshot::warmup_row_Lookup(10)];let snapshot=Snapshot::load_with_warmup(root,&plan,&requests,&token).unwrap();assert_eq!(snapshot.loaded_row_count(),3);let row=snapshot.get_Entry("east",1).unwrap();assert!(std::ptr::eq(row.get_Lookup().unwrap().unwrap().borrow(),snapshot.get_Lookup(10).unwrap().borrow()));code(snapshot.get_Lookup(20),ErrorCode::RowNotLoaded);}
            struct Counting(std::cell::Cell<usize>);impl ConfigSource for Counting{fn read(&self,_:&str,_:u64,_:&Cancellation)->ServerResult<Vec<u8>>{self.0.set(self.0.get()+1);panic!("must not read")}}
            let source=Counting(std::cell::Cell::new(0));let settings=LoadSettings::new(LoadPlan{tables:Some(vec!["Lookup".to_owned()]),..LoadPlan::default()});code(Snapshot::load_source_with_warmup(&source,&settings,&[Snapshot::warmup_row_Entry("east".to_owned(),1)],&token),ErrorCode::InvalidPlan);assert_eq!(source.0.get(),0);
          }
          for json in [false,true]{let snapshot=zrconfig::data::Snapshot::load(root,&LoadPlan{prefer_json:json,..LoadPlan::default()},&token).unwrap();let before=SERVER_ALLOCATIONS.load(Ordering::Relaxed);for _ in 0..100000{let row=snapshot.get_Entry("east",1).unwrap();let group=snapshot.find_group_Entry_ById(1).unwrap();let target=row.get_Lookup().unwrap().unwrap();let back=target.get_Peer().unwrap().unwrap().get_Peer().unwrap().unwrap();assert!(std::ptr::eq(target.borrow(),back.borrow()));std::hint::black_box(row.get_Id()+group.len()as i32);}assert_eq!(SERVER_ALLOCATIONS.load(Ordering::Relaxed)-before,0);println!("通过：Rust 新服务器主键、分组及双向引用 100000 次查询分配 0 次。");}
          for json in [false,true] { for mode in [LoadMode::ServerDefault,LoadMode::SelectedTables,LoadMode::LazyRowsStrict,LoadMode::LazyRowsOnAccess] {
            let plan=LoadPlan{mode,tables:Some(vec!["Entry".to_owned()]),prefer_json:json,..LoadPlan::default()};
            let snapshot=zrconfig::data::Snapshot::load(Path::new(&format!("{}-selected",root.display())),&plan,&token).unwrap();assert_eq!(snapshot.catalog_fingerprint().len(),64);if mode==LoadMode::LazyRowsOnAccess{assert_eq!(snapshot.load_plan_fingerprint(),if json{"c55af6527720622add35286774df8dd1cf6e8eda7537d04aa1ee2eef94f31d91"}else{"a3fe29f4e2fb7a9cc160a8a474905d5c40bb7f57179f9651f08a8967455785d1"});}
            code(snapshot.get_Concrete(1),ErrorCode::TableNotSelected);
            if matches!(mode,LoadMode::LazyRowsStrict|LoadMode::LazyRowsOnAccess) {
              assert_eq!(snapshot.completed_table_count(),0);assert_eq!(snapshot.loaded_row_count(),0);assert!(snapshot.find_Entry("missing",99).unwrap().is_none());
              code(snapshot.find_Entry("east",1),ErrorCode::RowNotLoaded);code(snapshot.rows_Entry(),ErrorCode::TableNotLoaded);
              let row=snapshot.load_Entry("east",1,&token).unwrap();assert_eq!(snapshot.loaded_row_count(),1);assert_eq!(row.get_LookupId(),10);
              code(row.get_Lookup(),ErrorCode::RowNotLoaded);let target=row.load_Lookup(&token).unwrap().unwrap();assert_eq!(snapshot.loaded_row_count(),2);
              code(target.get_Peer(),ErrorCode::RowNotLoaded);let peer=target.load_Peer(&token).unwrap().unwrap();let back=peer.get_Peer().unwrap().unwrap();
              assert!(std::ptr::eq(back.borrow(),target.borrow()));assert_eq!(snapshot.loaded_row_count(),3);
              code(snapshot.find_group_Entry_ById(1),ErrorCode::TableNotLoaded);let group=snapshot.load_group_Entry_ById(1,&token).unwrap();
              assert_eq!(group.len(),2);assert!(std::ptr::eq(group.at(0).unwrap().borrow(),row.borrow()));assert_eq!(snapshot.loaded_row_count(),4);
            }
            let row=snapshot.load_Entry("east",1,&token).unwrap();snapshot.load_all(&token).unwrap();assert_eq!(snapshot.completed_table_count(),2);let group=snapshot.rows_Entry().unwrap();
            assert_eq!(snapshot.loaded_row_count(),4);assert!(std::ptr::eq(group.at(0).unwrap().borrow(),row.borrow()));assert_eq!(row.get_Name(),"中文😀");
            let weak=Arc::downgrade(&snapshot);drop(snapshot);assert!(weak.upgrade().is_some());
            assert_eq!(row.load_Lookup(&token).unwrap().unwrap().load_Peer(&token).unwrap().unwrap().load_Peer(&token).unwrap().unwrap().get_Id(),10);
            drop(row);drop(group);assert!(weak.upgrade().is_none());
          }}
          {
            let plan=LoadPlan{mode:LoadMode::LazyRowsStrict,tables:Some(vec!["Lookup".to_owned()]),..LoadPlan::default()};
            let snapshot=zrconfig::data::Snapshot::load(root,&plan,&token).unwrap();
            std::thread::scope(|scope|{let mut jobs=Vec::new();for _ in 0..32{jobs.push(scope.spawn(||snapshot.load_Lookup(10,&token).unwrap()));}let first=jobs.remove(0).join().unwrap();for job in jobs{assert!(std::ptr::eq(job.join().unwrap().borrow(),first.borrow()));}});
            assert_eq!(snapshot.loaded_row_count(),1);
          }
          for json in [false,true] {
            let selected=zrconfig::server::Snapshot::load_selected(Path::new(&format!("{}-selected",root.display())),&["Entry"],json).unwrap();
            assert_eq!(selected.selection().tables(),&["Entry","Lookup"]);
            let row=selected.get_Entry("east",1).unwrap();let lookup=selected.get_Lookup(10).unwrap();
            assert!(std::ptr::eq(row.get_Lookup().unwrap().unwrap().borrow(),lookup.borrow()));
            code(selected.get_Concrete(1),ErrorCode::TableNotSelected);code(selected.borrow(),ErrorCode::UnsupportedCapability);
            code(row.snapshot().get_Concrete(1),ErrorCode::TableNotSelected);
            code(zrconfig::server::Snapshot::load_selected(Path::new(&format!("{}-selection-schema",root.display())),&["Entry"],json),ErrorCode::SchemaMismatch);
          }
          for json in [false,true] {
            let manager=zrconfig::server::Manager::new();
            code(manager.acquire_snapshot(),ErrorCode::NotInitialized);
            let make=||zrconfig::server::Snapshot::load(root,json);
            let mut prepared=manager.prepare(make,&token,options).unwrap();
            assert!(manager.try_acquire_snapshot().unwrap().is_none());
            code(manager.prepare(make,&token,options),ErrorCode::ReloadBusy);
            let other=zrconfig::server::Manager::new();
            code(other.commit(&mut prepared,&token),ErrorCode::ForeignCandidate);
            let result=manager.commit(&mut prepared,&token).unwrap();for timing in manager.stage_timings().unwrap(){assert!(timing.succeeded&&timing.operation_id==result.operation_id&&timing.elapsed_milliseconds>=0.0);}
            assert!(result.committed&&result.generation==1);
            code(manager.commit(&mut prepared,&token),ErrorCode::AlreadyCommitted);
            let row=result.snapshot.get_Entry("east",1).unwrap();
            assert!(std::ptr::eq(row.borrow(),result.snapshot.borrow().unwrap().get_Entry().get("east",1).unwrap()));
            let weak=Arc::downgrade(row.snapshot());
            let group=result.snapshot.group_Entry_ById(1).unwrap();
            let peer=row.get_Lookup().unwrap().unwrap();
            assert_eq!(peer.get_Peer().unwrap().unwrap().get_Peer().unwrap().unwrap().borrow().get_Id(),10);
            let mut pending=manager.prepare(make,&token,options).unwrap();
            manager.clear().unwrap();
            code(manager.commit(&mut pending,&token),ErrorCode::StaleCandidate);
            manager.reload(make,&token,options).unwrap();
            manager.close().unwrap();manager.close().unwrap();
            code(manager.acquire_snapshot(),ErrorCode::ManagerClosed);
            drop(result);drop(prepared);drop(pending);drop(manager);
            assert_eq!(row.borrow().get_Name(),"中文😀");
            assert_eq!(group.len(),2);assert_eq!(group.at(0).unwrap().borrow().get_Id(),1);
            drop(row);drop(group);
            assert!(weak.upgrade().is_some());
            assert_eq!(peer.get_Peer().unwrap().unwrap().get_Peer().unwrap().unwrap().borrow().get_Id(),10);
            drop(peer);assert!(weak.upgrade().is_none(),"owning handles leaked snapshot through a cycle");
          }
          {
            use zrconfig::runtime::{ManagerPolicy,SnapshotIdentity,ManagerEventKind,ConfigError};
            let checks=Arc::new(AtomicUsize::new(0));let counter=checks.clone();
            let policy=ManagerPolicy::new().with_identity(|value:&i32|SnapshotIdentity::new("scope","version",value.to_string()))
              .with_validator(move|value,token|{token.check()?;counter.fetch_add(1,Ordering::SeqCst);if *value<0{Err(ConfigError::new(ErrorCode::InvalidValue,"Rejected by host"))}else{Ok(())}});
            let manager=ServerManager::with_policy(policy);manager.reload(||Ok(Arc::new(1)),&token,options).unwrap();
            let current=manager.acquire_snapshot().unwrap();code(manager.prepare(||Ok(Arc::new(2)),&token,options),ErrorCode::VersionIdentityConflict);
            code(manager.prepare(||Ok(Arc::new(-1)),&token,options),ErrorCode::InvalidValue);assert!(Arc::ptr_eq(&current,&manager.acquire_snapshot().unwrap()));assert_eq!(checks.load(Ordering::SeqCst),3);
            manager.clear().unwrap();code(manager.prepare(||Ok(Arc::new(2)),&token,options),ErrorCode::VersionIdentityConflict);assert_eq!(manager.diagnostics().unwrap().retained_identities,1);
            let bounded=Arc::new(ServerManager::<i32>::with_policy(ManagerPolicy::new().with_event_capacity(2).unwrap()));let weak=Arc::downgrade(&bounded);let weak_listener=weak.clone();
            let generations=Arc::new(std::sync::Mutex::new(Vec::new()));let observed=generations.clone();
            let subscription=bounded.subscribe(move|event|{observed.lock().unwrap().push(event.generation);if event.kind==ManagerEventKind::Committed&&event.generation==1{let manager=weak_listener.upgrade().unwrap();for _ in 0..10{manager.clear().unwrap();}}}).unwrap();
            let failure=bounded.subscribe(|_|panic!("Expected isolated callback failure")).unwrap();
            let committed=bounded.reload(||Ok(Arc::new(7)),&token,options).unwrap();assert!(committed.committed);assert_eq!(*generations.lock().unwrap(),vec![1,10,11]);
            let diagnostics=bounded.diagnostics().unwrap();assert_eq!(diagnostics.dropped_events,8);assert_eq!(diagnostics.callback_failures,3);assert_eq!(diagnostics.pending_events,0);
            assert!(bounded.unsubscribe(subscription).unwrap());assert!(bounded.unsubscribe(failure).unwrap());drop(bounded);assert!(weak.upgrade().is_none());
            let slow=Arc::new(ServerManager::<i32>::new());let weak_slow=Arc::downgrade(&slow);let preparation_failures=Arc::new(std::sync::Mutex::new(Vec::new()));let failures_seen=preparation_failures.clone();slow.subscribe(move|event|{if event.kind==ManagerEventKind::PreparationFailed{let owner=weak_slow.upgrade().unwrap();assert_eq!(owner.diagnostics().unwrap().active_preparations,0);assert!(owner.status().unwrap().preparing_operation.is_none());assert!(event.operation_id>0);failures_seen.lock().unwrap().push(event.error.unwrap());panic!("Isolated failure listener");}}).unwrap();let short=PreparationOptions{timeout:std::time::Duration::from_millis(1),..options};
            code(slow.prepare_with_cancellation(|operation|{std::thread::sleep(std::time::Duration::from_millis(5));operation.check()?;Ok(Arc::new(1))},&token,short),ErrorCode::DeadlineExceeded);assert_eq!(slow.diagnostics().unwrap().active_preparations,0);
            let panic=std::panic::catch_unwind(std::panic::AssertUnwindSafe(||slow.prepare(||->ServerResult<Arc<i32>>{panic!("Host programming error")},&token,options)));assert!(panic.is_err());assert!(slow.status().unwrap().preparing_operation.is_none());assert_eq!(slow.diagnostics().unwrap().active_preparations,0);assert_eq!(*preparation_failures.lock().unwrap(),vec![ErrorCode::DeadlineExceeded,ErrorCode::InternalError]);assert_eq!(slow.diagnostics().unwrap().callback_failures,2);
            let observed_manager=Arc::new(std::sync::Mutex::new(std::sync::Weak::<ServerManager<i32>>::new()));let observable=observed_manager.clone();
            let memory=Arc::new(ServerManager::with_policy(ManagerPolicy::new().with_estimated_live_budget(100,move|_:&i32|{if let Some(manager)=observable.lock().unwrap().upgrade(){manager.status()?;}Ok(64)}).unwrap()));*observed_manager.lock().unwrap()=Arc::downgrade(&memory);
            let held=memory.reload(||Ok(Arc::new(7)),&token,options).unwrap().snapshot;assert_eq!(memory.estimated_live_bytes().unwrap(),Some(64));memory.clear().unwrap();
            code(memory.prepare(||Ok(Arc::new(8)),&token,options),ErrorCode::BudgetExceeded);assert_eq!(memory.estimated_live_bytes().unwrap(),Some(64));drop(held);assert_eq!(memory.estimated_live_bytes().unwrap(),Some(0));
            let mut admitted=memory.prepare(||Ok(Arc::new(9)),&token,options).unwrap();assert_eq!(memory.estimated_live_bytes().unwrap(),Some(64));admitted.discard();assert_eq!(memory.estimated_live_bytes().unwrap(),Some(0));
            let estimate=Arc::new(AtomicUsize::new(1));let observed=estimate.clone();let growing=ServerManager::with_policy(ManagerPolicy::new().with_estimated_live_budget(1,move|_:&i32|Ok(observed.load(Ordering::SeqCst)as u64)).unwrap());
            let mut growing_candidate=growing.prepare(||Ok(Arc::new(1)),&token,options).unwrap();estimate.store(2,Ordering::SeqCst);code(growing.commit(&mut growing_candidate,&token),ErrorCode::BudgetExceeded);assert!(growing.try_acquire_snapshot().unwrap().is_none());assert_eq!(growing.estimated_live_bytes().unwrap(),Some(2));growing_candidate.discard();
            let manager=Arc::new(ServerManager::<i32>::with_policy(ManagerPolicy::new().with_preparation_capacity(1).unwrap()));
            std::thread::scope(|scope|{let(tx,rx)=mpsc::channel();let(resume_tx,resume_rx)=mpsc::channel();let owner=manager.clone();let t=token.clone();
              let worker=scope.spawn(move||code(owner.prepare_with_cancellation(|operation|{tx.send(()).unwrap();resume_rx.recv().unwrap();operation.check()?;Ok(Arc::new(1))},&t,options),ErrorCode::Cancelled));
              rx.recv().unwrap();manager.clear().unwrap();code(manager.prepare(||Ok(Arc::new(2)),&token,options),ErrorCode::LimitExceeded);resume_tx.send(()).unwrap();worker.join().unwrap();
            });manager.reload(||Ok(Arc::new(2)),&token,options).unwrap();assert_eq!(manager.diagnostics().unwrap().active_preparations,0);
            println!("通过：Rust 有界数据源、资源预算、版本身份、校验器、通知重入及准备任务准入。");
          }
          let live=Arc::new(AtomicUsize::new(0));
          let make=||{live.fetch_add(1,Ordering::SeqCst);Ok(Arc::new(Tracked(live.clone())))};
          let manager=ServerManager::new();
          { let _prepared=manager.prepare(make,&token,options).unwrap();assert_eq!(live.load(Ordering::SeqCst),1); }
          assert_eq!(live.load(Ordering::SeqCst),0);
          let mut prepared=manager.prepare(make,&token,options).unwrap();
          let cancelled=Cancellation::new();cancelled.cancel();
          code(manager.commit(&mut prepared,&cancelled),ErrorCode::Cancelled);
          let result=manager.commit(&mut prepared,&token).unwrap();for timing in manager.stage_timings().unwrap(){assert!(timing.succeeded&&timing.operation_id==result.operation_id&&timing.elapsed_milliseconds>=0.0);}token.cancel();
          assert!(Arc::ptr_eq(&manager.acquire_snapshot().unwrap(),&result.snapshot));
          manager.clear().unwrap();drop(result);drop(prepared);assert_eq!(live.load(Ordering::SeqCst),0);
          let token=Cancellation::new();
          std::thread::scope(|scope| {
            let (entered_tx,entered_rx)=mpsc::channel();let (resume_tx,resume_rx)=mpsc::channel();
            let manager_ref=&manager;let token_ref=&token;let make_ref=&make;
            let running=scope.spawn(move||code(manager_ref.prepare(||{entered_tx.send(()).unwrap();resume_rx.recv().unwrap();make_ref()},token_ref,options),ErrorCode::StaleCandidate));
            entered_rx.recv().unwrap();
            code(manager.prepare(make,&token,options),ErrorCode::ReloadBusy);
            manager.clear().unwrap();let mut fresh=manager.prepare(make,&token,options).unwrap();
            let operation=manager.status().unwrap().preparing_operation;
            resume_tx.send(()).unwrap();running.join().unwrap();
            assert_eq!(manager.status().unwrap().preparing_operation,operation);
            manager.commit(&mut fresh,&token).unwrap();manager.close().unwrap();drop(fresh);
          });
          assert_eq!(live.load(Ordering::SeqCst),0);
          println!("通过：Rust 服务器安全行/分组、引用环释放、候选归属/取消/清理/关闭及屏障竞态。");
        }
        """;
    internal const string RustScheduling = """
        #[cfg(test)]
        mod scheduler_tests {
            use super::*;
            use std::sync::{atomic::{AtomicBool,AtomicUsize,Ordering},mpsc};
            use std::time::{Duration,Instant};
            fn budget()->Arc<LoadBudget>{Arc::new(LoadBudget::new(ResourceLimits::default()))}
            fn waiters(budget:&LoadBudget,count:usize){let until=Instant::now()+Duration::from_secs(10);while budget.diagnostics().unwrap().pending_waiters!=count{assert!(Instant::now()<until,"Waiter did not register");std::thread::yield_now();}}
            fn drained(budget:&LoadBudget){let d=budget.diagnostics().unwrap();assert_eq!((d.pending_jobs,d.pending_waiters,d.active_decodes),(0,0,0));}
            #[test]
            fn cancelling_one_waiter_keeps_shared_row_work_alive(){
                let schedule=RowSchedule::new();let budget=budget();let ready=AtomicBool::new(false);let calls=AtomicUsize::new(0);let cancelled=Cancellation::new();
                std::thread::scope(|scope|{
                    let (entered_tx,entered_rx)=mpsc::channel();let (resume_tx,resume_rx)=mpsc::channel();
                    let (s,b,r,c)=(&schedule,&budget,&ready,&calls);
                    let owner=scope.spawn(move||s.load(7,&Cancellation::new(),b,&||r.load(Ordering::Acquire),&mut ||{c.fetch_add(1,Ordering::SeqCst);entered_tx.send(()).unwrap();resume_rx.recv_timeout(Duration::from_secs(10)).unwrap();r.store(true,Ordering::Release);Ok(())}));
                    entered_rx.recv_timeout(Duration::from_secs(10)).unwrap();
                    let leaving=scope.spawn(||schedule.load(7,&cancelled,&budget,&||ready.load(Ordering::Acquire),&mut ||panic!("Duplicate row work")));
                    let staying=scope.spawn(||schedule.load(7,&Cancellation::new(),&budget,&||ready.load(Ordering::Acquire),&mut ||panic!("Duplicate row work")));
                    waiters(&budget,2);cancelled.cancel();assert_eq!(leaving.join().unwrap().unwrap_err().code,ErrorCode::Cancelled);
                    resume_tx.send(()).unwrap();owner.join().unwrap().unwrap();staying.join().unwrap().unwrap();
                });assert_eq!(calls.load(Ordering::SeqCst),1);assert!(ready.load(Ordering::Acquire));drained(&budget);
            }
            #[test]
            fn decoder_panic_wakes_waiters_and_allows_retry(){
                let schedule=RowSchedule::new();let budget=budget();
                std::thread::scope(|scope|{
                    let (entered_tx,entered_rx)=mpsc::channel();let (resume_tx,resume_rx)=mpsc::channel();let(s,b)=(&schedule,&budget);
                    let owner=scope.spawn(move||s.load(9,&Cancellation::new(),b,&||false,&mut ||{entered_tx.send(()).unwrap();resume_rx.recv_timeout(Duration::from_secs(10)).unwrap();panic!("Injected decoder panic")}));
                    entered_rx.recv_timeout(Duration::from_secs(10)).unwrap();
                    let waiter=scope.spawn(||schedule.load(9,&Cancellation::new(),&budget,&||false,&mut ||panic!("Duplicate row work")));
                    waiters(&budget,1);resume_tx.send(()).unwrap();assert!(owner.join().is_err());assert_eq!(waiter.join().unwrap().unwrap_err().code,ErrorCode::InternalError);
                });drained(&budget);assert_eq!(budget.diagnostics().unwrap().faulted_rows,0);
                schedule.load(9,&Cancellation::new(),&budget,&||false,&mut ||Ok(())).unwrap();drained(&budget);
            }
            #[test]
            fn row_data_failure_is_cached_but_transient_failure_retries(){
                let schedule=RowSchedule::new();let budget=budget();let mut calls=0;
                for _ in 0..2{assert_eq!(schedule.load(1,&Cancellation::new(),&budget,&||false,&mut ||{calls+=1;Err(ConfigError::new(ErrorCode::InvalidValue,"Bad row"))}).unwrap_err().code,ErrorCode::InvalidValue);}
                assert_eq!(calls,1);assert_eq!(budget.diagnostics().unwrap().faulted_rows,1);
                assert_eq!(schedule.load(2,&Cancellation::new(),&budget,&||false,&mut ||Err(ConfigError::new(ErrorCode::ReadFailed,"Transient"))).unwrap_err().code,ErrorCode::ReadFailed);
                schedule.load(2,&Cancellation::new(),&budget,&||false,&mut ||{calls+=1;Ok(())}).unwrap();assert_eq!(calls,2);drained(&budget);
            }
            #[test]
            fn index_panic_rolls_back_budget_and_success_publishes_once(){
                let index=DerivedValue::<String>::new();let budget=budget();let token=Cancellation::new();
                let panic=std::panic::catch_unwind(std::panic::AssertUnwindSafe(||index.load(&token,&budget,128,||panic!("Injected index panic"))));assert!(panic.is_err());
                assert_eq!(budget.diagnostics().unwrap().estimated_index_bytes,0);drained(&budget);
                assert_eq!(index.load(&token,&budget,128,||Ok(("ready".to_owned(),64))).unwrap(),"ready");
                assert_eq!(index.load(&token,&budget,128,||panic!("Index factory repeated")).unwrap(),"ready");
                assert_eq!(budget.diagnostics().unwrap().estimated_index_bytes,64);drained(&budget);
                let failed=DerivedValue::<String>::new();
                assert_eq!(failed.load(&token,&budget,128,||Err(ConfigError::new(ErrorCode::InvalidValue,"Bad index"))).unwrap_err().code,ErrorCode::InvalidValue);
                assert_eq!(failed.load(&token,&budget,128,||panic!("Failed index rebuilt")).unwrap_err().code,ErrorCode::InvalidValue);
                assert_eq!(budget.diagnostics().unwrap().estimated_index_bytes,64);assert_eq!(budget.diagnostics().unwrap().faulted_indexes,1);drained(&budget);
            }
            #[test]
            fn cancelling_one_index_waiter_does_not_cancel_publication(){
                let index=DerivedValue::<String>::new();let budget=budget();let calls=AtomicUsize::new(0);let cancelled=Cancellation::new();
                std::thread::scope(|scope|{
                    let (entered_tx,entered_rx)=mpsc::channel();let (resume_tx,resume_rx)=mpsc::channel();let(i,b,c)=(&index,&budget,&calls);
                    let owner=scope.spawn(move||i.load(&Cancellation::new(),b,64,||{c.fetch_add(1,Ordering::SeqCst);entered_tx.send(()).unwrap();resume_rx.recv_timeout(Duration::from_secs(10)).unwrap();Ok(("ready".to_owned(),8))}));
                    entered_rx.recv_timeout(Duration::from_secs(10)).unwrap();
                    let leaving=scope.spawn(||index.load(&cancelled,&budget,64,||panic!("Duplicate index work")));
                    let staying=scope.spawn(||index.load(&Cancellation::new(),&budget,64,||panic!("Duplicate index work")));
                    waiters(&budget,2);cancelled.cancel();assert_eq!(leaving.join().unwrap().unwrap_err().code,ErrorCode::Cancelled);
                    resume_tx.send(()).unwrap();assert_eq!(owner.join().unwrap().unwrap(),"ready");assert_eq!(staying.join().unwrap().unwrap(),"ready");
                });assert_eq!(calls.load(Ordering::SeqCst),1);assert_eq!(budget.diagnostics().unwrap().estimated_index_bytes,8);drained(&budget);
            }

        }
        """;

}
