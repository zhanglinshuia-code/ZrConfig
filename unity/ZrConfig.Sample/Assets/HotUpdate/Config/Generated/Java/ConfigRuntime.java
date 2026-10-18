// zrconfig-api-contract:1
package zrconfig;

import java.io.*;
import java.math.*;
import java.nio.*;
import java.nio.charset.*;
import java.nio.file.*;
import java.security.*;
import java.util.*;

/** 公共读取支持仅在加载期间解析数据；生成查询直接访问强类型记录和已建立的索引。 */
public final class ConfigRuntime {
    private ConfigRuntime() {}
    public enum ErrorCode {
        NotInitialized, ManagerClosed, ReloadBusy, StaleCandidate, AlreadyCommitted, ForeignCandidate,
        Cancelled, DeadlineExceeded, InvalidPlan, UnsupportedCapability, SyncNotSupported,
        SourceUnavailable, ReadFailed, InvalidManifest, UnsupportedFormat, TableSetMismatch, IntegrityMismatch, SchemaMismatch,
        InvalidEncoding, InvalidValue, DuplicateKey, MissingReference, KeyNotFound, TableNotSelected,
        TableNotLoaded, RowNotLoaded, LimitExceeded, BudgetExceeded, VersionIdentityConflict, CatalogMismatch, InternalError
    }
    public record ErrorContext(String versionId,String table,String rowKey,String fieldPath,String relativePath) {
        public ErrorContext { Objects.requireNonNull(versionId);Objects.requireNonNull(table);Objects.requireNonNull(rowKey);Objects.requireNonNull(fieldPath);Objects.requireNonNull(relativePath); }
        public static ErrorContext empty(){return new ErrorContext("","","","","");}
    }
    public static final class ConfigException extends RuntimeException {
        public final ErrorCode code;
        public final String stage;
        public final long operationId;
        public final ErrorContext context;
        public ConfigException(ErrorCode code,String message){this(code,message,"",0,null);}
        public ConfigException(ErrorCode code,String message,String stage,long operationId,Throwable cause){this(code,message,stage,operationId,cause,ErrorContext.empty());}
        public ConfigException(ErrorCode code,String message,String stage,long operationId,Throwable cause,ErrorContext context){super(message,cause);this.code=code;this.stage=stage;this.operationId=operationId;this.context=Objects.requireNonNull(context);}
        ConfigException locate(String phase,String version,String table,String key,String field,String path){
            return new ConfigException(code,getMessage(),stage.isEmpty()?phase:stage,operationId,this,new ErrorContext(context.versionId().isEmpty()?version:context.versionId(),context.table().isEmpty()?table:context.table(),context.rowKey().isEmpty()?key:context.rowKey(),context.fieldPath().isEmpty()?field:context.fieldPath(),context.relativePath().isEmpty()?path:context.relativePath()));
        }
    }
    public static final class Cancellation {
        private final Cancellation parent;
        private final long deadline;
        public Cancellation(){this(null,0);}
        private Cancellation(Cancellation parent,long deadline){this.parent=parent;this.deadline=deadline;}
        private final java.util.concurrent.atomic.AtomicBoolean cancelled = new java.util.concurrent.atomic.AtomicBoolean();
        private final Set<Runnable> waiters = new HashSet<>();
        public void cancel() {
            if (!cancelled.compareAndSet(false,true)) return;
            List<Runnable> callbacks; synchronized(waiters) { callbacks = List.copyOf(waiters); waiters.clear(); }
            for (Runnable callback : callbacks) callback.run();
        }
        public boolean isCancelled() { return cancelled.get()||(parent!=null&&parent.isCancelled()); }
        public void check() { if (isCancelled()) throw new ConfigException(ErrorCode.Cancelled, "Operation cancelled");if(parent!=null)parent.check();if(deadline!=0&&System.nanoTime()-deadline>=0)throw new ConfigException(ErrorCode.DeadlineExceeded,"Preparation deadline exceeded"); }
        private Runnable subscribe(Runnable callback) {
            Runnable parentRemove=parent==null?()->{}:parent.subscribe(callback);
            synchronized(waiters) { if (!isCancelled()) { waiters.add(callback); return () -> { parentRemove.run();synchronized(waiters) { waiters.remove(callback); } }; } }
            parentRemove.run();callback.run(); return () -> {};
        }
    }
    /** Immutable admission limits. Estimates describe runtime-owned values, not process RSS. */
    public static final class ResourceLimits {
        public final long maxManifestBytes;
        public final long maxTableBytes;
        public final long maxBundleBytes;
        public final int maxTables;
        public final int maxRows;
        public final int maxDecodedRows;
        public final int maxContainerElements;
        public final int maxDepth;
        public final int maxStringBytes;
        public final int maxConcurrentReads;
        public final int maxConcurrentDecodes;
        public final int maxPendingRequests;
        public final long maxEstimatedIndexBytes;
        public final long maxEstimatedDecodedBytes;
        private ResourceLimits(Builder builder) {
            maxManifestBytes=builder.maxManifestBytes; if(maxManifestBytes<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxManifestBytes");
            maxTableBytes=builder.maxTableBytes; if(maxTableBytes<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxTableBytes");
            maxBundleBytes=builder.maxBundleBytes; if(maxBundleBytes<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxBundleBytes");
            maxTables=builder.maxTables; if(maxTables<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxTables");
            maxRows=builder.maxRows; if(maxRows<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxRows");
            maxDecodedRows=builder.maxDecodedRows; if(maxDecodedRows<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxDecodedRows");
            maxContainerElements=builder.maxContainerElements; if(maxContainerElements<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxContainerElements");
            maxDepth=builder.maxDepth; if(maxDepth<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxDepth");
            maxStringBytes=builder.maxStringBytes; if(maxStringBytes<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxStringBytes");
            maxConcurrentReads=builder.maxConcurrentReads; if(maxConcurrentReads<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxConcurrentReads");
            maxConcurrentDecodes=builder.maxConcurrentDecodes; if(maxConcurrentDecodes<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxConcurrentDecodes");
            maxPendingRequests=builder.maxPendingRequests; if(maxPendingRequests<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxPendingRequests");
            maxEstimatedIndexBytes=builder.maxEstimatedIndexBytes; if(maxEstimatedIndexBytes<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxEstimatedIndexBytes");
            maxEstimatedDecodedBytes=builder.maxEstimatedDecodedBytes; if(maxEstimatedDecodedBytes<=0) throw new ConfigException(ErrorCode.InvalidPlan,"Invalid maxEstimatedDecodedBytes");
            if(maxRows>MAX_ROWS||maxDecodedRows>MAX_ROWS||maxContainerElements>MAX_ELEMENTS||maxDepth>MAX_DEPTH||maxStringBytes>16<<20)
                throw new ConfigException(ErrorCode.UnsupportedCapability,"Limits exceed wire capabilities");
        }
        public static ResourceLimits defaults(){return builder().build();}
        public static Builder builder(){return new Builder();}
        public static final class Builder {
            private long maxManifestBytes=16L<<20;
            public Builder maxManifestBytes(long value){maxManifestBytes=value;return this;}
            private long maxTableBytes=512L<<20;
            public Builder maxTableBytes(long value){maxTableBytes=value;return this;}
            private long maxBundleBytes=1L<<30;
            public Builder maxBundleBytes(long value){maxBundleBytes=value;return this;}
            private int maxTables=100_000;
            public Builder maxTables(int value){maxTables=value;return this;}
            private int maxRows=MAX_ROWS;
            public Builder maxRows(int value){maxRows=value;return this;}
            private int maxDecodedRows=MAX_ROWS;
            public Builder maxDecodedRows(int value){maxDecodedRows=value;return this;}
            private int maxContainerElements=MAX_ELEMENTS;
            public Builder maxContainerElements(int value){maxContainerElements=value;return this;}
            private int maxDepth=MAX_DEPTH;
            public Builder maxDepth(int value){maxDepth=value;return this;}
            private int maxStringBytes=16<<20;
            public Builder maxStringBytes(int value){maxStringBytes=value;return this;}
            private int maxConcurrentReads=1;
            public Builder maxConcurrentReads(int value){maxConcurrentReads=value;return this;}
            private int maxConcurrentDecodes=1;
            public Builder maxConcurrentDecodes(int value){maxConcurrentDecodes=value;return this;}
            private int maxPendingRequests=4096;
            public Builder maxPendingRequests(int value){maxPendingRequests=value;return this;}
            private long maxEstimatedIndexBytes=2L<<30;
            public Builder maxEstimatedIndexBytes(long value){maxEstimatedIndexBytes=value;return this;}
            private long maxEstimatedDecodedBytes=2L<<30;
            public Builder maxEstimatedDecodedBytes(long value){maxEstimatedDecodedBytes=value;return this;}
            private Builder(){}
            public ResourceLimits build(){return new ResourceLimits(this);}
        }
    }
    @FunctionalInterface public interface ConfigSource {
        /** Caller-owned source. Return bytes valid until this call returns; runtime captures a copy.
         * No automatic source disposal/retry. Blocking sources run only on the caller or supplied executor. */
        byte[] read(String relativePath,long maximumBytes,Cancellation cancellation) throws IOException;
    }
    public static final class MemoryConfigSource implements ConfigSource {
        private final Map<String,byte[]> files;
        public MemoryConfigSource(Map<String,byte[]> files){
            var copy=new HashMap<String,byte[]>();for(var entry:files.entrySet()){relativePath(entry.getKey());copy.put(entry.getKey(),entry.getValue().clone());}this.files=Map.copyOf(copy);
        }
        public byte[] read(String name,long maximum,Cancellation cancellation){
            cancellation.check();relativePath(name);if(maximum<=0)throw new ConfigException(ErrorCode.InvalidPlan,"Invalid byte limit");
            byte[] data=files.get(name);if(data==null)throw new ConfigException(ErrorCode.ReadFailed,"Missing source file: "+name,"read",0,new java.io.FileNotFoundException(name));
            if(data.length>maximum)throw new ConfigException(ErrorCode.LimitExceeded,"Source file exceeds limit");return data.clone();
        }
    }
    public static final class FileConfigSource implements ConfigSource {
        private final Path root;
        public FileConfigSource(Path root){this.root=Objects.requireNonNull(root).toAbsolutePath().normalize();}
        public String sourceNamespace(){return root.toString();}
        public byte[] read(String name,long maximum,Cancellation cancellation)throws IOException{
            cancellation.check();relativePath(name);if(maximum<=0)throw new ConfigException(ErrorCode.InvalidPlan,"Invalid byte limit");
            var rootInfo=Files.readAttributes(root,java.nio.file.attribute.BasicFileAttributes.class,LinkOption.NOFOLLOW_LINKS);
            if(!rootInfo.isDirectory()||rootInfo.isSymbolicLink())throw new ConfigException(ErrorCode.ReadFailed,"Source root is not a real directory");
            Path canonical=root.toRealPath(),path=canonical;for(String part:name.split("/")){path=path.resolve(part);if(Files.isSymbolicLink(path))throw new ConfigException(ErrorCode.ReadFailed,"Symbolic links are not permitted");}
            var before=Files.readAttributes(path,java.nio.file.attribute.BasicFileAttributes.class,LinkOption.NOFOLLOW_LINKS);
            if(!before.isRegularFile())throw new ConfigException(ErrorCode.ReadFailed,"Source is not a regular file");
            if(before.size()<0||before.size()>maximum||before.size()>Integer.MAX_VALUE)throw new ConfigException(ErrorCode.LimitExceeded,"Source file exceeds limit");
            byte[] bytes;
            try(var channel=java.nio.channels.FileChannel.open(path,StandardOpenOption.READ,LinkOption.NOFOLLOW_LINKS)){
                if(channel.size()!=before.size())throw new ConfigException(ErrorCode.ReadFailed,"Source changed before reading");
                bytes=new byte[(int)before.size()];var target=ByteBuffer.wrap(bytes);
                while(target.hasRemaining()){cancellation.check();int end=target.limit();target.limit(target.position()+Math.min(end-target.position(),1<<20));int count=channel.read(target);target.limit(end);if(count<=0)throw new ConfigException(ErrorCode.ReadFailed,"Short source read");}
                if(channel.size()!=bytes.length)throw new ConfigException(ErrorCode.ReadFailed,"Source changed during reading");
            }
            cancellation.check();var after=Files.readAttributes(path,java.nio.file.attribute.BasicFileAttributes.class,LinkOption.NOFOLLOW_LINKS);
            if(!after.isRegularFile()||!Objects.equals(before.fileKey(),after.fileKey())||before.size()!=after.size()||!before.lastModifiedTime().equals(after.lastModifiedTime()))throw new ConfigException(ErrorCode.ReadFailed,"Source identity changed");
            Path check=canonical;for(String part:name.split("/")){check=check.resolve(part);if(Files.isSymbolicLink(check))throw new ConfigException(ErrorCode.ReadFailed,"Source path changed");}
            if(Files.isSymbolicLink(root)||!canonical.equals(root.toRealPath()))throw new ConfigException(ErrorCode.ReadFailed,"Source root changed");return bytes;
        }
    }
    private static void relativePath(String name){
        if(name==null||name.isEmpty()||name.indexOf('\\')>=0||name.indexOf(':')>=0||name.indexOf('\0')>=0)throw new ConfigException(ErrorCode.InvalidPlan,"Invalid source path");
        for(String part:name.split("/",-1))if(part.isEmpty()||part.startsWith("."))throw new ConfigException(ErrorCode.InvalidPlan,"Invalid source path");
    }
    public interface SnapshotIdentityProvider {String version();String manifestDigest();String sourceNamespace();}
    static RuntimeException serverFailure(RuntimeException error){return rowFailure(error);}
    public record SnapshotDiagnostics(long readBytes,long retainedRawBytes,long estimatedDecodedBytes,int declaredRows,int decodedRows,int activeDecodes,int pendingJobs,int pendingWaiters,long mergedRequests,long faultedRows,String health,long faultedIndexes,long estimatedIndexBytes){public SnapshotDiagnostics(long readBytes,long retainedRawBytes,long estimatedDecodedBytes,int declaredRows,int decodedRows,int activeDecodes,int pendingJobs,int pendingWaiters,long mergedRequests,long faultedRows,String health,long faultedIndexes){this(readBytes,retainedRawBytes,estimatedDecodedBytes,declaredRows,decodedRows,activeDecodes,pendingJobs,pendingWaiters,mergedRequests,faultedRows,health,faultedIndexes,0);}public long estimatedLiveBytes(){long base=retainedRawBytes>Long.MAX_VALUE-(long)declaredRows*64?Long.MAX_VALUE:retainedRawBytes+(long)declaredRows*64;base=estimatedIndexBytes>Long.MAX_VALUE-base?Long.MAX_VALUE:base+estimatedIndexBytes;return estimatedDecodedBytes>Long.MAX_VALUE-base?Long.MAX_VALUE:base+estimatedDecodedBytes;}public SnapshotDiagnostics(long readBytes,long retainedRawBytes,long estimatedDecodedBytes,int declaredRows,int decodedRows,int activeDecodes,int pendingJobs,int pendingWaiters,long mergedRequests,long faultedRows,String health){this(readBytes,retainedRawBytes,estimatedDecodedBytes,declaredRows,decodedRows,activeDecodes,pendingJobs,pendingWaiters,mergedRequests,faultedRows,health,0);}}
    static final class LoadBudget {
        final ResourceLimits limits;long readBytes,rawBytes,estimated,indexEstimated,merged,faults,indexFaults;int rows,decoded,decodes,jobs,waiters;
        LoadBudget(ResourceLimits limits){this.limits=limits;}
        synchronized void read(int count){if(count>limits.maxBundleBytes-readBytes)throw limit();readBytes+=count;}
        synchronized void rows(int count){if(count>limits.maxRows-rows)throw limit();rows+=count;}
        synchronized long readLimit(){long result=Math.min(limits.maxTableBytes,limits.maxBundleBytes-readBytes);if(result<=0)throw limit();return result;}
        synchronized void reserve(long estimate){if(decodes>=limits.maxConcurrentDecodes||decoded>=limits.maxDecodedRows||estimate>limits.maxEstimatedDecodedBytes-estimated)throw limit();decodes++;decoded++;estimated+=estimate;}
        synchronized void reserveTransient(long estimate){if(decodes>=limits.maxConcurrentDecodes||estimate>limits.maxEstimatedDecodedBytes-estimated)throw limit();decodes++;estimated+=estimate;}
        synchronized void finishTransient(long estimate){decodes--;estimated-=estimate;}
        synchronized void finish(long estimate,boolean success,boolean terminal){decodes--;if(!success){decoded--;estimated-=estimate;}if(terminal)faults++;}
        synchronized void admit(boolean job){if(job){if(jobs>=limits.maxPendingRequests)throw limit();jobs++;}else{if(waiters>=limits.maxPendingRequests)throw limit();waiters++;merged++;}}
        synchronized void release(boolean job){if(job)jobs--;else waiters--;}
        synchronized void reserveIndex(long bytes){if(bytes<0||bytes>limits.maxEstimatedIndexBytes-indexEstimated)throw new ConfigException(ErrorCode.BudgetExceeded,"Derived index budget exhausted","group",0,null);indexEstimated+=bytes;}
        synchronized void releaseIndex(long bytes){indexEstimated-=bytes;}
        synchronized void groupFault(){indexFaults++;}
        synchronized SnapshotDiagnostics diagnostics(){return new SnapshotDiagnostics(readBytes,rawBytes,estimated,rows,decoded,decodes,jobs,waiters,merged,faults,faults==0&&indexFaults==0?"Healthy":"Degraded",indexFaults,indexEstimated);}
        private static ConfigException limit(){return new ConfigException(ErrorCode.LimitExceeded,"Load budget exhausted");}
    }
    public record PreparationOptions(long timeoutMillis, long candidateLifetimeMillis) {
        public PreparationOptions {
            if (timeoutMillis <= 0 || candidateLifetimeMillis <= 0 || timeoutMillis > Long.MAX_VALUE / 1_000_000 || candidateLifetimeMillis > Long.MAX_VALUE / 1_000_000)
                throw new ConfigException(ErrorCode.InvalidPlan, "Invalid preparation deadlines");
        }
        public static PreparationOptions defaults() { return new PreparationOptions(300_000, 300_000); }
    }
    public record TableDescriptor(String name, String schemaHash, List<String> dependencies, int component) {
        public TableDescriptor { dependencies = List.copyOf(dependencies); }
    }
    public record SelectionPlan(List<String> requested, List<String> tables, String catalogFingerprint) {
        public SelectionPlan { requested = List.copyOf(requested); tables = List.copyOf(tables); }
    }
    public static final class Catalog {
        public final String fingerprint;
        private final String target;
        public final List<TableDescriptor> tables;
        private final Map<String, TableDescriptor> names = new HashMap<>();
        private final Map<Integer, List<TableDescriptor>> components = new HashMap<>();
        public Catalog(String source) {
            Object definition = parse(source);
            long format=integer(field(definition,"format"),1,2);
            if(format==2){var supported=java.util.Set.of("binary-v4", "json-v1", "server-default", "selected-closure", "lazy-strict", "lazy-on-access", "fixed-source-bytes", "snapshot-owned-handles", "explicit-reference-load", "row-cache", "group-cache", "typed-warmup", "bounded-source", "structured-errors", "manager-lifecycle");if(!"server-v1".equals(field(definition,"runtimeContract")))throw new ConfigException(ErrorCode.UnsupportedCapability,"Unsupported runtime contract");for(Object value:array(field(definition,"requiredCapabilities")))if(!supported.contains(string(value)))throw new ConfigException(ErrorCode.UnsupportedCapability,"Unsupported runtime capability: "+value);}
            target=object(definition).get("target") instanceof String value?value:"";
            fingerprint = string(field(definition, "fingerprint"));
            var result = new ArrayList<TableDescriptor>();
            for (Object value : array(field(definition, "tables"))) {
                var dependencies = new ArrayList<String>(); for (Object name : array(field(value, "dependencies"))) dependencies.add(string(name));
                var table = new TableDescriptor(string(field(value,"name")),string(field(value,"schemaHash")),dependencies,(int)integer(field(value,"component"),0,Integer.MAX_VALUE));
                if (names.put(table.name(), table) != null) throw new ConfigException(ErrorCode.CatalogMismatch,"Duplicate catalog table");
                components.computeIfAbsent(table.component(), key -> new ArrayList<>()).add(table); result.add(table);
            }
            for (var table : result) for (String dependency : table.dependencies()) if (!names.containsKey(dependency)) throw new ConfigException(ErrorCode.CatalogMismatch,"Unknown dependency");
            tables = List.copyOf(result);
        }
        public SelectionPlan select(Collection<String> requested) {
            var input = new ArrayList<>(requested); var selected = new HashSet<String>(); var componentsSeen = new HashSet<Integer>();
            var queue = new ArrayDeque<String>(input);
            while (!queue.isEmpty()) {
                String name = queue.removeFirst(); var table = names.get(name);
                if (table == null) throw new ConfigException(ErrorCode.InvalidPlan, "Unknown selected table: " + name);
                if (!componentsSeen.add(table.component())) continue;
                for (var member : components.get(table.component())) { selected.add(member.name()); queue.addAll(member.dependencies()); }
            }
            var result = new ArrayList<String>(); for (var table : tables) if (selected.contains(table.name())) result.add(table.name());
            input.sort(String::compareTo); return new SelectionPlan(input, result, fingerprint);
        }
    }
    public enum ManagerPhase { Empty, Ready, Closed }
    public record ManagerStatus(ManagerPhase phase,long generation,long preparingOperation){}
    public record CommitResult<S>(S snapshot,long generation,long operationId,boolean committed){}
    public record SnapshotIdentity(String sourceNamespace,String version,String manifestDigest){
        public SnapshotIdentity{Objects.requireNonNull(sourceNamespace);Objects.requireNonNull(version);Objects.requireNonNull(manifestDigest);if(version.isEmpty()||!manifestDigest.matches("[0-9a-fA-F]{64}"))throw new ConfigException(ErrorCode.InvalidPlan,"Invalid snapshot identity");manifestDigest=manifestDigest.toUpperCase(Locale.ROOT);}
    }
    public record ManagerEvent(String kind,long generation,long operationId,String version,ErrorCode code){}
    public record EventDiagnostics(long dropped,long listenerFailures,int queued,int subscribers){}
    @FunctionalInterface public interface SnapshotFactory<S>{S prepare()throws Exception;}
    @FunctionalInterface public interface CancellableSnapshotFactory<S>{S prepare(Cancellation cancellation)throws Exception;}
    @FunctionalInterface public interface SnapshotValidator<S>{void validate(S snapshot,Cancellation cancellation)throws Exception;}
    @FunctionalInterface public interface SnapshotIdentityResolver<S>{SnapshotIdentity identity(S snapshot);}
    public record ManagerPolicy<S>(SnapshotIdentityResolver<S> identity,List<SnapshotValidator<S>> validators,int eventCapacity,int maxVersionIdentities,int maxPendingPreparations,long maxEstimatedLiveBytes,java.util.function.ToLongFunction<S> liveEstimator,int maxTrackedSnapshots){
        public ManagerPolicy{validators=List.copyOf(validators);if(eventCapacity<1||maxVersionIdentities<1||maxPendingPreparations<1||maxEstimatedLiveBytes<1||maxTrackedSnapshots<1||(maxEstimatedLiveBytes!=Long.MAX_VALUE&&liveEstimator==null))throw new ConfigException(ErrorCode.InvalidPlan,"Invalid manager policy");}
        public ManagerPolicy(SnapshotIdentityResolver<S> identity,List<SnapshotValidator<S>> validators,int eventCapacity,int maxVersionIdentities,int maxPendingPreparations){this(identity,validators,eventCapacity,maxVersionIdentities,maxPendingPreparations,Long.MAX_VALUE,null,1024);}
        public static <S> ManagerPolicy<S> defaults(){return new ManagerPolicy<>(null,List.of(),64,32,64);}
        public ManagerPolicy<S> withEstimatedLiveBudget(long maximum,java.util.function.ToLongFunction<S> estimate){return new ManagerPolicy<>(identity,validators,eventCapacity,maxVersionIdentities,maxPendingPreparations,maximum,Objects.requireNonNull(estimate),maxTrackedSnapshots);}
        public ManagerPolicy<S> withLiveTrackingCapacity(int maximum){return new ManagerPolicy<>(identity,validators,eventCapacity,maxVersionIdentities,maxPendingPreparations,maxEstimatedLiveBytes,liveEstimator,maximum);}
    }
    private static final class ManagerEvents{
        final int capacity;final ArrayDeque<ManagerEvent> queue=new ArrayDeque<>();final Map<Long,java.util.function.Consumer<ManagerEvent>> listeners=new LinkedHashMap<>();
        long serial,dropped,failures;boolean draining;
        ManagerEvents(int capacity){this.capacity=capacity;}
        synchronized AutoCloseable subscribe(java.util.function.Consumer<ManagerEvent> listener){Objects.requireNonNull(listener);if(serial==Long.MAX_VALUE)throw new ConfigException(ErrorCode.LimitExceeded,"Subscription identifier exhausted");long id=++serial;listeners.put(id,listener);return ()->{synchronized(this){listeners.remove(id);}};}
        synchronized void enqueue(ManagerEvent event){if(queue.size()==capacity){queue.removeFirst();dropped++;}queue.addLast(event);}
        void drain(){synchronized(this){if(draining)return;draining=true;}
            try{while(true){ManagerEvent event;List<java.util.function.Consumer<ManagerEvent>> callbacks;synchronized(this){event=queue.pollFirst();if(event==null){draining=false;return;}callbacks=List.copyOf(listeners.values());}
                for(var callback:callbacks)try{callback.accept(event);}catch(Throwable failure){synchronized(this){failures++;}}
            }}catch(Throwable failure){synchronized(this){draining=false;}throw failure;}
        }
        synchronized EventDiagnostics diagnostics(){return new EventDiagnostics(dropped,failures,queue.size(),listeners.size());}
    }
    public static final class PreparedSnapshot<S> implements AutoCloseable{
        private final java.lang.ref.WeakReference<ServerManager<S>> owner;private volatile S snapshot;private final SnapshotIdentity identity;
        private final long generation,operation,expires;private boolean committed;
        private PreparedSnapshot(ServerManager<S> owner,S snapshot,SnapshotIdentity identity,long generation,long operation,long expires){this.owner=new java.lang.ref.WeakReference<>(owner);this.snapshot=snapshot;this.identity=identity;this.generation=generation;this.operation=operation;this.expires=expires;}
        public S snapshot(){S value=snapshot;if(value==null)throw new ConfigException(ErrorCode.StaleCandidate,"Candidate discarded");return value;}
        @Override public void close(){var manager=owner.get();if(manager!=null)manager.discard(this);else snapshot=null;}
    }
    /** Monotonic elapsed time including waits, excluding event callbacks; operationId zero means no sample. */
    public record StageTiming(String stage,long operationId,double elapsedMilliseconds,boolean succeeded) {}
    public static final class ServerManager<S> implements AutoCloseable{
        private final Object gate=new Object();private final java.util.concurrent.atomic.AtomicReference<S> current=new java.util.concurrent.atomic.AtomicReference<>();
        private final ManagerPolicy<S> policy;private final ManagerEvents events;
        private final LinkedHashMap<IdentityKey,String> identities=new LinkedHashMap<>();
        private final ArrayList<java.lang.ref.WeakReference<S>> live=new ArrayList<>();private final Map<Long,Operation> operations=new HashMap<>();
        private record IdentityKey(String namespace,String version){}
        private volatile boolean closed;private boolean building;private long generation,serial,pending,expires;private int activePreparations;
        private static final class Operation{
            final long generation,id,deadline;final long started=System.nanoTime();final Cancellation cancellation;final java.util.concurrent.atomic.AtomicBoolean released=new java.util.concurrent.atomic.AtomicBoolean();
            Operation(long generation,long id,long deadline,Cancellation parent){this.generation=generation;this.id=id;this.deadline=deadline;cancellation=new Cancellation(parent,deadline);}
        }
        public ServerManager(){this(ManagerPolicy.defaults());}
        public ServerManager(ManagerPolicy<S> policy){this.policy=Objects.requireNonNull(policy);events=new ManagerEvents(policy.eventCapacity());}
        public AutoCloseable subscribe(java.util.function.Consumer<ManagerEvent> listener){return events.subscribe(listener);}
        public EventDiagnostics eventDiagnostics(){return events.diagnostics();}
        private StageTiming prepareTiming=new StageTiming("Prepare",0,0,false),commitTiming=new StageTiming("Commit",0,0,false);
        public List<StageTiming> stageTimings(){synchronized(gate){return List.of(prepareTiming,commitTiming);}}
        private void recordPreparation(Operation operation,boolean success){synchronized(gate){if(operation.id>=prepareTiming.operationId())prepareTiming=new StageTiming("Prepare",operation.id,(System.nanoTime()-operation.started)/1_000_000.0,success);}}
        public int activePreparations(){synchronized(gate){return activePreparations;}}
        // Weak tracking is conservative until GC; estimates do not impose a hard process heap limit.
        private java.util.OptionalLong liveUsage(S candidate,boolean enforce){
            if(policy.liveEstimator()==null)return java.util.OptionalLong.empty();var owners=new ArrayList<S>();
            synchronized(gate){live.removeIf(reference->reference.get()==null);for(var reference:live){S owner=reference.get();if(owner!=null)owners.add(owner);}}
            if(candidate!=null&&owners.stream().noneMatch(owner->owner==candidate))owners.add(candidate);
            long total=0;for(S owner:owners){long estimate=policy.liveEstimator().applyAsLong(owner);if(estimate<0)throw error(ErrorCode.InvalidValue);total=estimate>Long.MAX_VALUE-total?Long.MAX_VALUE:total+estimate;if(enforce&&total>policy.maxEstimatedLiveBytes())throw error(ErrorCode.BudgetExceeded);}return java.util.OptionalLong.of(total);
        }
        private void trackLive(S candidate){if(policy.liveEstimator()==null)return;live.removeIf(reference->reference.get()==null);for(var reference:live)if(reference.get()==candidate)return;if(live.size()>=policy.maxTrackedSnapshots())throw error(ErrorCode.BudgetExceeded);live.add(new java.lang.ref.WeakReference<>(candidate));}
        public java.util.OptionalLong estimatedLiveBytes(){return liveUsage(null,false);}
        public S tryAcquireSnapshot(){S value=current.get();if(closed)throw error(ErrorCode.ManagerClosed);return value;}
        public S acquireSnapshot(){S value=tryAcquireSnapshot();if(value==null)throw error(ErrorCode.NotInitialized);return value;}
        public ManagerStatus status(){synchronized(gate){return new ManagerStatus(closed?ManagerPhase.Closed:current.get()==null?ManagerPhase.Empty:ManagerPhase.Ready,generation,pending);}}
        private Operation begin(Cancellation cancellation,PreparationOptions options){
            Objects.requireNonNull(cancellation);Objects.requireNonNull(options);cancellation.check();long deadline=System.nanoTime()+options.timeoutMillis()*1_000_000;
            synchronized(gate){if(closed)throw error(ErrorCode.ManagerClosed);if(pending!=0&&(building||System.nanoTime()-expires<0))throw error(ErrorCode.ReloadBusy);
                if(serial==Long.MAX_VALUE||activePreparations>=policy.maxPendingPreparations())throw error(ErrorCode.LimitExceeded);
                long operation=++serial;var result=new Operation(generation,operation,deadline,cancellation);pending=operation;building=true;expires=deadline;activePreparations++;operations.put(operation,result);return result;
            }
        }
        private void end(Operation operation){synchronized(gate){if(pending==operation.id)pending=0;}}
        private void release(Operation operation){if(operation.released.compareAndSet(false,true))synchronized(gate){activePreparations--;operations.remove(operation.id);}}
        private SnapshotIdentity describe(S value){if(policy.identity()!=null)return Objects.requireNonNull(policy.identity().identity(value));if(value instanceof SnapshotIdentityProvider identity)return new SnapshotIdentity(identity.sourceNamespace(),identity.version(),identity.manifestDigest());return null;}
        private void checkIdentity(SnapshotIdentity identity){if(identity!=null){String old=identities.get(new IdentityKey(identity.sourceNamespace(),identity.version()));if(old!=null&&!old.equals(identity.manifestDigest()))throw error(ErrorCode.VersionIdentityConflict);}}
        private PreparedSnapshot<S> build(Operation operation,CancellableSnapshotFactory<S> factory,PreparationOptions options){
            try{operation.cancellation.check();liveUsage(null,true);S next=factory.prepare(operation.cancellation);synchronized(gate){if(closed)throw error(ErrorCode.ManagerClosed);if(generation!=operation.generation||pending!=operation.id)throw error(ErrorCode.StaleCandidate);}operation.cancellation.check();if(next==null)throw error(ErrorCode.InvalidValue);
                for(var validator:policy.validators()){validator.validate(next,operation.cancellation);operation.cancellation.check();}
                SnapshotIdentity identity=describe(next);operation.cancellation.check();liveUsage(next,true);long lifetime=System.nanoTime()+options.candidateLifetimeMillis()*1_000_000;
                synchronized(gate){if(closed)throw error(ErrorCode.ManagerClosed);if(generation!=operation.generation||pending!=operation.id)throw error(ErrorCode.StaleCandidate);checkIdentity(identity);trackLive(next);expires=lifetime;building=false;}
                var prepared=new PreparedSnapshot<>(this,next,identity,operation.generation,operation.id,lifetime);recordPreparation(operation,true);return prepared;
            }catch(Throwable failure){recordPreparation(operation,false);end(operation);release(operation);ErrorCode code=failure instanceof ConfigException structured?structured.code:failure instanceof IOException?ErrorCode.ReadFailed:failure instanceof InterruptedException?ErrorCode.Cancelled:failure instanceof DataFailure||failure instanceof BufferUnderflowException?ErrorCode.InvalidValue:ErrorCode.InternalError;
                events.enqueue(new ManagerEvent("PrepareFailed",operation.generation,operation.id,"",code));events.drain();
                if(failure instanceof Error fatal)throw fatal;if(failure instanceof RuntimeException runtime)throw serverFailure(runtime);
                if(failure instanceof InterruptedException){Thread.currentThread().interrupt();throw new ConfigException(ErrorCode.Cancelled,failure.getMessage(),"prepare",operation.id,failure);}
                throw new ConfigException(failure instanceof IOException?ErrorCode.ReadFailed:ErrorCode.InvalidValue,failure.getMessage(),"prepare",operation.id,failure);
            }finally{release(operation);}
        }
        private void abortScheduled(Operation operation,ErrorCode code){recordPreparation(operation,false);end(operation);release(operation);events.enqueue(new ManagerEvent("PrepareFailed",operation.generation,operation.id,"",code));events.drain();}
        public PreparedSnapshot<S> prepare(SnapshotFactory<S> factory,Cancellation cancellation,PreparationOptions options){Objects.requireNonNull(factory);return prepare(token->factory.prepare(),cancellation,options);}
        public PreparedSnapshot<S> prepare(CancellableSnapshotFactory<S> factory,Cancellation cancellation,PreparationOptions options){Objects.requireNonNull(factory);return build(begin(cancellation,options),factory,options);}
        public java.util.concurrent.CompletableFuture<PreparedSnapshot<S>> prepareAsync(SnapshotFactory<S> factory,Cancellation cancellation,PreparationOptions options,java.util.concurrent.Executor executor){Objects.requireNonNull(factory);return prepareAsync(token->factory.prepare(),cancellation,options,executor);}
        public java.util.concurrent.CompletableFuture<PreparedSnapshot<S>> prepareAsync(CancellableSnapshotFactory<S> factory,Cancellation cancellation,PreparationOptions options,java.util.concurrent.Executor executor){
            Objects.requireNonNull(factory);Objects.requireNonNull(executor);var result=new java.util.concurrent.CompletableFuture<PreparedSnapshot<S>>();Operation operation;
            try{operation=begin(cancellation,options);}catch(RuntimeException failure){return java.util.concurrent.CompletableFuture.failedFuture(failure);}
            result.whenComplete((value,error)->{if(result.isCancelled()){operation.cancellation.cancel();end(operation);}});
            try{executor.execute(()->{if(result.isCancelled()){abortScheduled(operation,ErrorCode.Cancelled);return;}try{var value=build(operation,factory,options);if(!result.complete(value))value.close();}catch(Throwable error){result.completeExceptionally(error);}});}
            catch(RuntimeException|Error error){abortScheduled(operation,error instanceof java.util.concurrent.RejectedExecutionException?ErrorCode.ReloadBusy:ErrorCode.InternalError);result.completeExceptionally(error);}return result;
        }
        public CommitResult<S> commit(PreparedSnapshot<S> prepared,Cancellation cancellation){
            long started=System.nanoTime();
            Objects.requireNonNull(prepared);Objects.requireNonNull(cancellation);if(prepared.owner.get()!=this)throw error(ErrorCode.ForeignCandidate);CommitResult<S> result;
            synchronized(gate){if(closed)throw error(ErrorCode.ManagerClosed);if(prepared.committed)throw error(ErrorCode.AlreadyCommitted);if(prepared.snapshot==null||generation!=prepared.generation||pending!=prepared.operation)throw error(ErrorCode.StaleCandidate);}cancellation.check();liveUsage(prepared.snapshot,true);
            synchronized(gate){if(closed)throw error(ErrorCode.ManagerClosed);if(prepared.committed)throw error(ErrorCode.AlreadyCommitted);
                if(prepared.snapshot==null||generation!=prepared.generation||pending!=prepared.operation)throw error(ErrorCode.StaleCandidate);
                cancellation.check();if(System.nanoTime()-prepared.expires>=0)throw error(ErrorCode.DeadlineExceeded);if(generation==Long.MAX_VALUE)throw error(ErrorCode.LimitExceeded);checkIdentity(prepared.identity);
                if(prepared.identity!=null){var key=new IdentityKey(prepared.identity.sourceNamespace(),prepared.identity.version());if(!identities.containsKey(key)&&identities.size()==policy.maxVersionIdentities())identities.remove(identities.keySet().iterator().next());identities.put(key,prepared.identity.manifestDigest());}
                generation++;current.set(prepared.snapshot);pending=0;prepared.committed=true;result=new CommitResult<>(prepared.snapshot,generation,prepared.operation,true);
                commitTiming=new StageTiming("Commit",prepared.operation,(System.nanoTime()-started)/1_000_000.0,true);
                events.enqueue(new ManagerEvent("Committed",generation,prepared.operation,prepared.identity==null?"":prepared.identity.version(),null));
            }events.drain();return result;
        }
        public CommitResult<S> reload(SnapshotFactory<S> factory,Cancellation cancellation,PreparationOptions options){try(var prepared=prepare(factory,cancellation,options)){return commit(prepared,cancellation);}}
        public CommitResult<S> reload(CancellableSnapshotFactory<S> factory,Cancellation cancellation,PreparationOptions options){try(var prepared=prepare(factory,cancellation,options)){return commit(prepared,cancellation);}}
        public void clear(){detach(false);}@Override public void close(){detach(true);}
        private void discard(PreparedSnapshot<S> prepared){synchronized(gate){if(!prepared.committed&&pending==prepared.operation)pending=0;prepared.snapshot=null;}}
        private void detach(boolean close){synchronized(gate){if(closed){if(close)return;throw error(ErrorCode.ManagerClosed);}if(generation==Long.MAX_VALUE)throw error(ErrorCode.LimitExceeded);
            generation++;if(close)closed=true;current.set(null);pending=0;for(var operation:operations.values())operation.cancellation.cancel();events.enqueue(new ManagerEvent(close?"Closed":"Cleared",generation,0,"",null));}events.drain();}
        private static ConfigException error(ErrorCode code){return new ConfigException(code,code.name());}
    }
    public static final int MAX_ROWS = 5_000_000, MAX_ELEMENTS = 1_000_000, MAX_DEPTH = 64;
    private static final class DataFailure extends IllegalArgumentException {
        DataFailure(String message) { super(message); }
    }
    public static IllegalArgumentException invalid(String message) { return new DataFailure(message); }
    private static RuntimeException rowFailure(RuntimeException error) {
        if (error instanceof ConfigException) return error;
        if (error instanceof DataFailure || error instanceof BufferUnderflowException)
            return new ConfigException(ErrorCode.InvalidValue,error.getMessage(),"row",0,error);
        return error;
    }
    private static boolean terminalRowFailure(RuntimeException error) {
        return error instanceof ConfigException data && switch (data.code) {
            case InvalidEncoding, InvalidValue, DuplicateKey, MissingReference, IntegrityMismatch, SchemaMismatch -> true;
            default -> false;
        };
    }
    public record NumberText(String text) {}
    @SuppressWarnings("unchecked")
    public static Map<String, Object> object(Object value) {
        if (!(value instanceof Map<?,?>))
            throw invalid("需要对象");
        return (Map<String, Object>)value;
    }
    @SuppressWarnings("unchecked")
    public static List<Object> array(Object value) {
        if (!(value instanceof List<?>))
            throw invalid("需要数组");
        return (List<Object>)value;
    }
    public static Object field(Object value, String name) {
        var map = object(value);
        if (!map.containsKey(name))
            throw invalid("缺少字段：" + name);
        return map.get(name);
    }
    public static String string(Object value) {
        if (!(value instanceof String))
            throw invalid("需要字符串");
        return (String)value;
    }
    public static String number(Object value) {
        if (!(value instanceof NumberText n))
            throw invalid("需要数值");
        return n.text;
    }
    public static boolean bool(Object value) {
        if (!(value instanceof Boolean))
            throw invalid("需要布尔值");
        return (Boolean)value;
    }
    public static long integer(Object value, long min, long max) {
        long v = Long.parseLong(number(value));
        if (v < min || v > max)
            throw invalid("整数超出范围");
        return v;
    }
    public static long int64(Object value) { return Long.parseLong(string(value)); }
    public static BigInteger uint64(Object value) {
        var v = new BigInteger(string(value));
        if (v.signum() < 0 || v.bitLength() > 64)
            throw invalid("uint64 超出范围");
        return v;
    }
    public static float float32(Object value) {
        float v = Float.parseFloat(number(value));
        if (!Float.isFinite(v))
            throw invalid("浮点数必须有限");
        return v;
    }
    public static double float64(Object value) {
        double v = Double.parseDouble(number(value));
        if (!Double.isFinite(v))
            throw invalid("浮点数必须有限");
        return v;
    }
    /** 构造时规范化，作为查询键时 equals/hashCode 不产生临时 BigDecimal。 */
    public static final class Decimal {
        private final BigDecimal value, canonical;
        private final int hash;
        public Decimal(String text) {
            if (!text.matches("[+-]?[0-9]+(\\.[0-9]*)?"))
                throw invalid("decimal 格式无效");
            value = new BigDecimal(text);
            canonical = value.stripTrailingZeros();
            BigDecimal checked = canonical.scale() < 0 ? canonical.setScale(0) : canonical;
            if (checked.scale() > 28 || checked.unscaledValue().abs().bitLength() > 96)
                throw invalid("decimal 超出范围");
            hash = canonical.hashCode();
        }
        public BigDecimal value() { return value; }
        @Override
        public int hashCode() {
            return hash;
        }
        @Override
        public boolean equals(Object other) {
            return other instanceof Decimal d && canonical.equals(d.canonical);
        }
        @Override
        public String toString() {
            return value.toPlainString();
        }
    }
    public static final class Matrix<T> {
        private final int[] dimensions;
        private final List<T> values;
        public Matrix(int[] dimensions, List<T> values) {
            this.dimensions = dimensions.clone();
            this.values = Collections.unmodifiableList(new ArrayList<>(values));
            long n = 1;
            for (int d : dimensions) {
                if (d < 0 || d > MAX_ELEMENTS)
                    throw invalid("矩阵维度无效");
                n = Math.multiplyExact(n, d);
            }
            if (n != values.size() || n > MAX_ELEMENTS)
                throw invalid("矩阵元素数无效");
        }
        public int rank() { return dimensions.length; }
        public int dimension(int axis) { return dimensions[axis]; }
        public List<T> values() { return values; }
        public T at(int index) { return values.get(index); }
    }
    public static <T> List<T> list(Object value, java.util.function.Function<Object, T> decode) {
        var source = array(value);
        if (source.size() > MAX_ELEMENTS)
            throw invalid("数组过长");
        var values = new ArrayList<T>(source.size());
        for (var item : source)
            values.add(decode.apply(item));
        return Collections.unmodifiableList(values);
    }
    public static <K, V> Map<K, V> dictionary(Object value, java.util.function.Function<Object, K> key,
                                              java.util.function.Function<Object, V> element) {
        var values = new LinkedHashMap<K, V>();
        for (var item : array(value)) {
            var pair = array(item);
            if (pair.size() != 2)
                throw invalid("字典条目无效");
            K k = key.apply(pair.get(0));
            if (values.containsKey(k))
                throw invalid("字典键重复");
            values.put(k, element.apply(pair.get(1)));
        }
        return Collections.unmodifiableMap(values);
    }
    public static <T> Matrix<T> matrix(Object value, java.util.function.Function<Object, T> decode) {
        var ds = array(field(value, "dimensions"));
        int[] d = new int[ds.size()];
        for (int i = 0; i < d.length; i++)
            d[i] = (int)integer(ds.get(i), 0, MAX_ELEMENTS);
        return new Matrix<>(d, list(field(value, "values"), decode));
    }
    public static int hash(float value) { return value == 0 ? 0 : Float.hashCode(value); }
    public static int hash(double value) { return value == 0 ? 0 : Double.hashCode(value); }

    /** 开放寻址索引保存行位置；生成的 find 方法直接比较键分量，避免组合键对象和数值装箱。 */
    public abstract static class Index<R> {
        protected final List<R> rows;
        protected final int[] slots;
        protected final List<R>[] groups;
        protected final int mask;
        protected abstract int hash(R row);
        protected abstract boolean same(R left, R right);
        @SuppressWarnings("unchecked")
        protected Index(List<R> rows, boolean unique) {
            this.rows = rows;
            int size = 2;
            while (size < (long)rows.size() * 2)
                size <<= 1;
            slots = new int[size];
            Arrays.fill(slots, -1);
            mask = size - 1;
            groups = (List<R>[]) new List<?>[unique ? 0 : size];
            for (int i = 0; i < rows.size(); i++) {
                R row = rows.get(i);
                int slot = mix(hash(row)) & mask;
                while (slots[slot] >= 0 && !same(rows.get(slots[slot]), row))
                    slot = (slot + 1) & mask;
                if (slots[slot] >= 0 && unique)
                    throw invalid("主键重复");
                if (slots[slot] < 0) {
                    slots[slot] = i;
                    if (!unique)
                        groups[slot] = new ArrayList<>();
                }
                if (!unique)
                    groups[slot].add(row);
            }
            for (int i = 0; i < groups.length; i++)
                if (groups[i] != null)
                    groups[i] = Collections.unmodifiableList(groups[i]);
        }
        protected static int mix(int h) {
            h ^= h >>> 16;
            h *= 0x7feb352d;
            h ^= h >>> 15;
            return h;
        }
    }

    public static Object parse(byte[] bytes) {return parse(bytes,null);}
    private static Object parse(byte[] bytes,JsonBounds bounds) {
        try {
            return new Parser(StandardCharsets.UTF_8.newDecoder()
                                  .onMalformedInput(CodingErrorAction.REPORT)
                                  .onUnmappableCharacter(CodingErrorAction.REPORT)
                                  .decode(ByteBuffer.wrap(bytes))
                                  .toString(),bounds)
                .parse();
        } catch (CharacterCodingException e) {
            throw new ConfigException(ErrorCode.InvalidEncoding,"Invalid UTF-8","decode",0,e);
        }
    }
    public static Object parse(String text) { return new Parser(text).parse(); }
    private static final class JsonBounds {
        final int rows,elements,members,stringBytes,depth;
        final String rootArray;
        JsonBounds(ResourceLimits limits){rows=limits.maxTables;elements=64;members=32;stringBytes=4096;depth=8;rootArray="tables";}
        JsonBounds(Object schema,ResourceLimits limits){
            rootArray="rows";rows=limits.maxRows;elements=Math.max(64,limits.maxContainerElements);depth=Math.min(72,limits.maxDepth*2+8);
            int memberMaximum=Math.max(4,array(field(schema,"fields")).size()+1),stringMaximum=Math.max(256,limits.maxStringBytes);
            stringMaximum=Math.max(stringMaximum,string(field(schema,"name")).getBytes(StandardCharsets.UTF_8).length);
            for(Object value:array(field(schema,"fields")))stringMaximum=Math.max(stringMaximum,string(field(value,"name")).getBytes(StandardCharsets.UTF_8).length);
            for(Object type:array(field(schema,"types"))){stringMaximum=Math.max(stringMaximum,string(field(type,"id")).getBytes(StandardCharsets.UTF_8).length);var fields=array(field(type,"fields"));memberMaximum=Math.max(memberMaximum,fields.size()+1);for(Object value:fields)stringMaximum=Math.max(stringMaximum,string(field(value,"name")).getBytes(StandardCharsets.UTF_8).length);}
            members=memberMaximum;stringBytes=stringMaximum;
        }
        static ConfigException exceeded(){return new ConfigException(ErrorCode.LimitExceeded,"JSON allocation budget exceeded","parse",0,null);}
    }
    private static final class Parser {
        final String text;
        int pos, remaining = 32_000_000;
        final JsonBounds bounds;
        Parser(String text) {this(text,null);}
        Parser(String text,JsonBounds bounds) { this.text = text;this.bounds=bounds; }
        void space() {
            while (pos < text.length() && " \r\n\t".indexOf(text.charAt(pos)) >= 0)
                pos++;
        }
        char take() {
            if (pos == text.length())
                throw invalid("JSON 意外结束");
            return text.charAt(pos++);
        }
        void expect(char c) {
            if (take() != c)
                throw invalid("JSON 格式错误");
        }
        boolean consume(char c) {
            space();
            if (pos < text.length() && text.charAt(pos) == c) {
                pos++;
                return true;
            }
            return false;
        }
        Object parse() {
            Object v = read(0);
            space();
            if (pos != text.length())
                throw invalid("JSON 尾随数据");
            return v;
        }
        Object read(int depth) {return read(depth,false);}
        Object read(int depth,boolean rows) {
            if(bounds!=null&&depth>bounds.depth)throw JsonBounds.exceeded();
            if (depth > MAX_DEPTH + 8 || --remaining < 0)
                throw invalid("JSON 解析预算耗尽");
            space();
            if (pos == text.length())
                throw invalid("JSON 缺少值");
            char c = text.charAt(pos);
            if (c == '"')
                return quoted();
            if (c == '{' || c == '[') {
                pos++;
                boolean obj = c == '{';
                char end = obj ? '}' : ']';
                Map<String, Object> map = new LinkedHashMap<>();
                List<Object> list = new ArrayList<>();
                if (consume(end))
                    return obj ? map : list;
                do {
                    if (obj) {
                        if(bounds!=null&&map.size()>=bounds.members)throw JsonBounds.exceeded();
                        space();
                        String name = quoted();
                        space();
                        expect(':');
                        if (map.containsKey(name))
                            throw invalid("JSON 重复成员");
                        map.put(name, read(depth + 1,depth==0&&name.equals(bounds==null?"rows":bounds.rootArray)));
                    } else {
                        if(bounds!=null&&list.size()>=(rows?bounds.rows:bounds.elements))throw JsonBounds.exceeded();
                        list.add(read(depth + 1));
                    }
                } while (consume(','));
                space();
                expect(end);
                return obj ? map : list;
            }
            if (c == 't' || c == 'f' || c == 'n') {
                String token = c == 't' ? "true" : c == 'f' ? "false" : "null";
                for (char ch : token.toCharArray())
                    expect(ch);
                return c == 'n' ? null : c == 't';
            }
            int start = pos;
            if (c == '-')
                pos++;
            if (pos == text.length())
                throw invalid("JSON 数字无效");
            if (text.charAt(pos) == '0')
                pos++;
            else {
                if (text.charAt(pos) < '1' || text.charAt(pos) > '9')
                    throw invalid("JSON 数字无效");
                digits();
            }
            if (pos < text.length() && text.charAt(pos) == '.') {
                pos++;
                requireDigits();
            }
            if (pos < text.length() && (text.charAt(pos) == 'e' || text.charAt(pos) == 'E')) {
                pos++;
                if (pos < text.length() && (text.charAt(pos) == '+' || text.charAt(pos) == '-'))
                    pos++;
                requireDigits();
            }
            if (pos - start > 1024)
                throw invalid("JSON 数字过长");
            return new NumberText(text.substring(start, pos));
        }
        void digits() {
            while (pos < text.length() && text.charAt(pos) >= '0' && text.charAt(pos) <= '9')
                pos++;
        }
        void requireDigits() {
            int start = pos;
            digits();
            if (start == pos)
                throw invalid("JSON 数字缺失");
        }
        String quoted() {
            expect('"');
            StringBuilder b = new StringBuilder();
            while (true) {
                char c = take();
                if (c == '"')
                    break;
                if (c < 32)
                    throw invalid("JSON 控制字符无效");
                if (c == '\\') {
                    c = take();
                    switch (c) {
                    case '"', '\\', '/' -> b.append(c);
                    case 'b' -> b.append('\b');
                    case 'f' -> b.append('\f');
                    case 'n' -> b.append('\n');
                    case 'r' -> b.append('\r');
                    case 't' -> b.append('\t');
                    case 'u' -> {
                        int n = 0;
                        for (int i = 0; i < 4; i++) {
                            int h = Character.digit(take(), 16);
                            if (h < 0)
                                throw invalid("Unicode 转义错误");
                            n = n * 16 + h;
                        }
                        b.append((char)n);
                    }
                    default -> throw invalid("JSON 转义无效");
                    }
                } else
                    b.append(c);
                if(bounds!=null&&b.length()>bounds.stringBytes)throw JsonBounds.exceeded();
                if (b.length() > 16 * 1024 * 1024)
                    throw invalid("字符串过长");
            }
            String s = b.toString();
            int utf8Length = 0;
            for (int i = 0; i < s.length(); i++) {
                char c = s.charAt(i);
                if (Character.isHighSurrogate(c)) {
                    if (++i == s.length() || !Character.isLowSurrogate(s.charAt(i)))
                        throw invalid("Unicode 代理项无效");
                    utf8Length += 4;
                } else if (Character.isLowSurrogate(c))
                    throw invalid("Unicode 代理项无效");
                else
                    utf8Length += c < 0x80 ? 1 : c < 0x800 ? 2 : 3;
            }
            if(bounds!=null&&utf8Length>bounds.stringBytes)throw JsonBounds.exceeded();
            if (utf8Length > 16 * 1024 * 1024) throw invalid("字符串过长");
            return s;
        }
    }

    public static final class Bundle {
        public final String version;
        private final Path folder;
        private final String prefix;
        private final String target;
        private final ConfigSource source;
        private final Cancellation cancellation;
        final LoadBudget budget;
        final String manifestDigest;
        LoadBudget budget(){return budget;}
        private final Map<String, Object> entries = new HashMap<>();
        public Bundle(Path root) throws IOException {this(root,null,null,null,Files.readAllBytes(root.resolve(Files.exists(root.resolve("Common/current.json"))?"Common/current.json":"current.json")));}
        Bundle(ConfigSource source,ResourceLimits limits,Cancellation cancellation)throws IOException{this(null,source,limits,cancellation,manifestBytes(source,limits.maxManifestBytes,cancellation));}
        private static byte[] manifestBytes(ConfigSource source,long limit,Cancellation cancellation)throws IOException{
            try{return capture(source,"Common/current.json",limit,cancellation);}
            catch(ConfigException error){if(!missingManifest(error))throw error;return capture(source,"current.json",limit,cancellation);}
        }
        private static boolean missingManifest(Throwable error){for(Throwable cause=error;cause!=null;cause=cause.getCause())if(cause instanceof java.nio.file.NoSuchFileException||cause instanceof java.io.FileNotFoundException||cause instanceof ConfigException own&&own.code==ErrorCode.SourceUnavailable)return true;return false;}
        private String tablePath(String file){return prefix+(prefix.equals("Tables/")?(file.endsWith(".json")?"json/":"bytes/"):"")+file;}
        private static byte[] capture(ConfigSource source,String name,long limit,Cancellation cancellation)throws IOException{
            try{cancellation.check();byte[] data=source.read(name,limit,cancellation);if(data==null)throw new ConfigException(ErrorCode.ReadFailed,"Source returned null");cancellation.check();if(data.length>limit)throw new ConfigException(ErrorCode.LimitExceeded,"Source exceeded byte limit");return data.clone();}
            catch(IOException failure){throw locateRead(new ConfigException(ErrorCode.ReadFailed,failure.getMessage(),"read",0,failure),name);}
            catch(ConfigException failure){throw locateRead(failure,name);}
        }
        private static ConfigException locateRead(ConfigException failure,String name){String[] parts=name.split("/");String file=parts[parts.length-1];int dot=file.lastIndexOf('.');return failure.locate("read",parts.length==3&&parts[0].equals("versions")?parts[1]:"",parts.length==3?(dot<0?file:file.substring(0,dot)):"","","",name);}
        private Bundle(Path root,ConfigSource source,ResourceLimits limits,Cancellation cancellation,byte[] bytes){
            this.source=source;this.cancellation=cancellation;budget=limits==null?null:new LoadBudget(limits);if(budget!=null)budget.read(bytes.length);
            try{manifestDigest=HexFormat.of().formatHex(MessageDigest.getInstance("SHA-256").digest(bytes));}catch(NoSuchAlgorithmException error){throw new IllegalStateException(error);}
            try{
            Object manifest=parse(bytes,limits==null?null:new JsonBounds(limits));
            target=object(manifest).get("target") instanceof String value?value:"";
            if(limits!=null&&array(field(manifest,"tables")).size()>limits.maxTables)throw new ConfigException(ErrorCode.LimitExceeded,"Too many tables");
            String format = manifestText(field(manifest, "format"));
            if (!Set.of("ZrConfig.Binary/1", "ZrConfig.Json/1", "ZrConfig.Mixed/1").contains(format))
                throw new ConfigException(ErrorCode.UnsupportedFormat,"Unsupported manifest format");
            version = segment(manifestText(field(manifest, "versionId")));
            Object layout=object(manifest).get("layout");
            if(layout!=null&&!layout.equals("")&&!layout.equals("flat")&&!layout.equals("grouped"))throw invalid("Unsupported data layout");
            prefix="grouped".equals(layout)?"Tables/":"flat".equals(layout)?"":"versions/"+version+"/";
            folder=root;
            for (Object entry : array(field(manifest, "tables"))) {
                String name = segment(manifestText(field(entry, "name")));
                String suffix = format.equals("ZrConfig.Json/1") ? ".json" : ".bytes";
                if (!Objects.equals(field(entry, "file"),name + suffix) || integer(field(entry, "rows"), 0, MAX_ROWS) < 0)
                    throw invalid("清单文件名或行数错误");
                if (format.equals("ZrConfig.Mixed/1")) {
                    if (!Objects.equals(field(entry, "jsonFile"),name + ".json") ||
                        !manifestText(field(entry, "jsonSha256")).matches("[0-9a-fA-F]{64}"))
                        throw invalid("JSON 清单无效");
                } else if (object(entry).get("jsonFile") != null || object(entry).get("jsonSha256") != null)
                    throw invalid("格式与 JSON 清单不一致");
                if (!manifestText(field(entry, "sha256")).matches("[0-9a-fA-F]{64}")||!manifestText(field(entry,"schemaHash")).matches("[0-9a-fA-F]{64}"))
                    throw invalid("清单哈希无效");
                if (entries.put(name, entry) != null)
                    throw invalid("清单表名重复");
            }
            }catch(DataFailure error){if(budget==null)throw error;throw new ConfigException(ErrorCode.InvalidManifest,error.getMessage(),"manifest",0,error);}
        }
        private static String manifestText(Object value){if(!(value instanceof String result))throw invalid("Manifest string required");return result;}
        void validateTables(String[] expected) {
            if (entries.size() != expected.length)
                throw invalid("清单表集合不匹配");
            for (String name : expected)
                if (!entries.containsKey(name)) throw invalid("清单表集合不匹配");
        }
        void validateCatalog(Catalog catalog) {
            if(!catalog.target.isEmpty()&&!catalog.target.equals(target))throw new ConfigException(ErrorCode.CatalogMismatch,"Manifest target differs from catalog","manifest",0,null);
            if (entries.size() != catalog.tables.size()) throw new ConfigException(ErrorCode.TableSetMismatch,"Manifest table set differs from catalog");
            for (var table : catalog.tables) {
                Object entry = entries.get(table.name());
                if (entry == null) throw new ConfigException(ErrorCode.TableSetMismatch,"Missing table: " + table.name());
                if (!table.schemaHash().equals(string(field(entry,"schemaHash")))) throw new ConfigException(ErrorCode.SchemaMismatch,"Schema mismatch: " + table.name());
            }
        }
        public List<Object> load(String schema, boolean preferJson) throws IOException {
            Object definition = parse(schema);
            String name = string(field(definition, "name"));
            Object entry = entries.get(name);
            if (entry == null)
                throw invalid("缺少表：" + name);
            if (!field(definition, "hash").equals(field(entry, "schemaHash")))
                throw invalid("结构不匹配：" + name);
            Map<String, Object> e = object(entry);
            boolean alternate = preferJson && e.get("jsonFile") instanceof String;
            String file = segment(string(field(entry, alternate ? "jsonFile" : "file")));
            byte[] bytes = Files.readAllBytes(folder.resolve(tablePath(file)));
            String expected = string(field(entry, alternate ? "jsonSha256" : "sha256"));
            try {
                if (!HexFormat.of()
                         .formatHex(MessageDigest.getInstance("SHA-256").digest(bytes))
                         .equalsIgnoreCase(expected))
                    throw invalid("文件哈希不匹配：" + name);
            } catch (NoSuchAlgorithmException ex) {
                throw new IllegalStateException(ex);
            }
            List<Object> rows = new Wire(definition).read(bytes, file.endsWith(".json"));
            if (rows.size() != integer(field(entry, "rows"), 0, MAX_ROWS))
                throw invalid("清单行数不匹配");
            return rows;
        }
        IndexedRows openRows(String schema, boolean preferJson) throws IOException {
            Object definition = parse(schema); String name = string(field(definition,"name")); Object entry = entries.get(name);
            if (entry == null || !field(definition,"hash").equals(field(entry,"schemaHash"))) throw new ConfigException(ErrorCode.SchemaMismatch,"Schema mismatch: " + name);
            boolean alternate = preferJson && object(entry).get("jsonFile") instanceof String;
            String file = segment(string(field(entry,alternate ? "jsonFile" : "file")));
            if(budget!=null)budget.rows((int)integer(field(entry,"rows"),0,MAX_ROWS));
            byte[] bytes = source==null?Files.readAllBytes(folder.resolve(tablePath(file))):capture(source,tablePath(file),budget.readLimit(),cancellation);if(budget!=null)budget.read(bytes.length);
            try {
                if (!HexFormat.of().formatHex(MessageDigest.getInstance("SHA-256").digest(bytes)).equalsIgnoreCase(string(field(entry,alternate ? "jsonSha256" : "sha256"))))
                    throw new ConfigException(ErrorCode.IntegrityMismatch,"Digest mismatch: " + name);
            } catch (NoSuchAlgorithmException error) { throw new IllegalStateException(error); }
            IndexedRows rows = new IndexedRows(definition,bytes,file.endsWith(".json"),budget);rows.version=version;rows.relativePath=tablePath(file);
            if (rows.count != integer(field(entry,"rows"),0,MAX_ROWS)) throw new ConfigException(ErrorCode.InvalidValue,"Row count mismatch");
            return rows;
        }
    }
    private static String segment(String name) {
        if (name.isEmpty() || name.startsWith(".") || name.indexOf('/') >= 0 || name.indexOf('\\') >= 0 ||
            name.indexOf(':') >= 0)
            throw invalid("清单路径无效");
        return name;
    }
    static String loadPlanFingerprint(LoadMode mode,boolean json,List<String> tables){
        var names=new ArrayList<String>();for(String name:tables)names.add(HexFormat.of().formatHex(name.getBytes(StandardCharsets.UTF_8)));Collections.sort(names);
        String value="zrconfig-load-plan/1\n"+mode.name()+"\n"+(json?"json-preferred":"binary")+"\n"+String.join("\n",names);
        try{return HexFormat.of().formatHex(MessageDigest.getInstance("SHA-256").digest(value.getBytes(StandardCharsets.UTF_8)));}catch(NoSuchAlgorithmException error){throw new IllegalStateException(error);}
    }
    public enum LoadMode { ServerDefault, SelectedTables, LazyRowsStrict, LazyRowsOnAccess }
    public record LoadPlan(LoadMode mode, List<String> tables, boolean preferJson, List<String> warmupTables,ResourceLimits limits,String sourceNamespace) {
        public LoadPlan(LoadMode mode,List<String> tables,boolean preferJson,List<String> warmupTables){this(mode,tables,preferJson,warmupTables,ResourceLimits.defaults(),"");}
        public LoadPlan { Objects.requireNonNull(mode); Objects.requireNonNull(limits); Objects.requireNonNull(sourceNamespace); tables = tables == null ? null : List.copyOf(tables); warmupTables = List.copyOf(warmupTables); }
        public static LoadPlan defaults() { return new LoadPlan(LoadMode.ServerDefault,null,false,List.of()); }
    }
    // Runtime-only index descriptors do not escape generated views. JSON retains independent
    // row fragments, never a full DOM alongside decoded business objects.
    private static final class IntOrdinalIndex {
        private final int[] keys,values;private final int mask;
        IntOrdinalIndex(int count){int capacity=1;while(capacity<count*2)capacity<<=1;keys=new int[capacity];values=new int[capacity];Arrays.fill(values,-2);mask=capacity-1;}
        private int slot(int key){int h=key;h^=h>>>16;h*=0x7feb352d;h^=h>>>15;h*=0x846ca68b;h^=h>>>16;int slot=h&mask;while(values[slot]!=-2&&keys[slot]!=key)slot=(slot+1)&mask;return slot;}
        void add(int key,int ordinal){int slot=slot(key);if(values[slot]!=-2)values[slot]=-1;else{keys[slot]=key;values[slot]=ordinal;}}
        int get(int key){return values[slot(key)];}
    }
    static final class IndexedRows {
        private long indexRawBytes;
        final int count;
        final Object schema;
        String version="",relativePath="";
        private final Wire wire;
        final LoadBudget budget;
        private final byte[] bytes;
        private final String[] fragments;
        private final JsonBounds jsonBounds;
        private final int[] offsets, lengths, ids;
        private final List<Object> keys;
        private final IntOrdinalIndex references;
        private java.util.function.Function<String,IndexedRows> resolve;
        void bindReferences(java.util.function.Function<String,IndexedRows> resolver) { resolve = resolver; }
        IndexedRows(Object schema, byte[] input, boolean json,LoadBudget budget) {
            this.budget=budget;this.schema = schema;jsonBounds=budget==null?null:new JsonBounds(schema,budget.limits); wire = new Wire(schema);wire.limits=budget==null?null:budget.limits; var names = array(field(schema,"keys")); var fields = array(field(schema,"fields"));
            keys = new ArrayList<>();
            if (json) {
                Object document = parse(input,jsonBounds);
                if (object(document).size() != 4 || !"ZrConfig.TableJson/1".equals(field(document,"format")) || !field(schema,"name").equals(field(document,"table")) || !field(schema,"hash").equals(field(document,"schemaHash"))) throw new ConfigException(ErrorCode.SchemaMismatch,"JSON header mismatch");
                var rows = array(field(document,"rows")); count = rows.size(); if (count > MAX_ROWS||(budget!=null&&count>budget.limits.maxRows)) throw new ConfigException(ErrorCode.LimitExceeded,"Too many rows");
                bytes = null; fragments = new String[count]; offsets = lengths = new int[0]; ids = new int[count];
                for (int i = 0; i < count; i++) {
                    Object row = rows.get(i); var key = new LinkedHashMap<String,Object>(); wire.reset();
                    for (Object name : names) { String n = string(name); Object value = field(row,n); wire.validate(wire.fieldType(fields,n),value,0); key.put(n,value); }
                    keys.add(Collections.unmodifiableMap(key)); ids[i] = bool(field(schema,"hasId")) ? (int)integer(field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE) : 0;
                    var fragment = new StringBuilder(); appendJson(fragment,row); fragments[i] = fragment.toString();
                }
            } else {
                bytes = input; fragments = null; var cursor = new Cursor(input);if(budget!=null)cursor.maximumString=budget.limits.maxStringBytes;
                if (cursor.data.getInt() != 0x4643525a || cursor.data.getInt() != 4 || !field(schema,"name").equals(cursor.text()) || !field(schema,"hash").equals(cursor.text())) throw new ConfigException(ErrorCode.SchemaMismatch,"Binary header mismatch");
                count = cursor.count(MAX_ROWS); if(budget!=null&&count>budget.limits.maxRows)throw LoadBudget.limit(); if ((long)count * 12 > cursor.data.remaining()) throw new ConfigException(ErrorCode.InvalidValue,"Truncated index");
                var allowed = new HashSet<>(array(field(schema,"tags"))); int nt = cursor.count(allowed.size()); wire.tags = new String[nt]; var seen = new HashSet<String>();
                for (int i = 0; i < nt; i++) { String tag = cursor.text(); if (!allowed.contains(tag) || !seen.add(tag)) throw new ConfigException(ErrorCode.InvalidValue,"Invalid type directory"); wire.tags[i] = tag; }
                offsets = new int[count]; lengths = new int[count]; ids = new int[count]; long end = 0;
                for (int i = 0; i < count; i++) {
                    wire.reset(); var key = new LinkedHashMap<String,Object>();
                    for (Object name : names) { String n = string(name); key.put(n,wire.value(wire.fieldType(fields,n),cursor,0)); }
                    keys.add(Collections.unmodifiableMap(key)); ids[i] = cursor.data.getInt(); offsets[i] = cursor.data.getInt(); lengths[i] = cursor.data.getInt();
                    if (offsets[i] != end || lengths[i] < 0) throw new ConfigException(ErrorCode.InvalidValue,"Invalid row bounds"); end += lengths[i];
                }
                int start = cursor.data.position(); if (start + end != input.length) throw new ConfigException(ErrorCode.InvalidValue,"Invalid payload length");
                for (int i = 0; i < count; i++) offsets[i] += start;
            }
            {long raw=bytes==null?0:bytes.length;if(fragments!=null)for(String fragment:fragments)raw+=fragment.length()*2L;indexRawBytes=raw;if(budget!=null)synchronized(budget){budget.rawBytes+=raw;}}
            references=new IntOrdinalIndex(bool(field(schema,"hasId"))?count:0);if (bool(field(schema,"hasId"))) for (int i = 0; i < count; i++) references.add(ids[i],i);
        }
        Object key(int index) { return keys.get(index); }
        int reference(int id) { int ordinal = references.get(id); if (ordinal < 0) throw new ConfigException(ErrorCode.MissingReference,"Missing or ambiguous reference: " + id); return ordinal; }
        long estimate(int index){return 256L+4L*(fragments==null?lengths[index]:fragments[index].length()*2L);}
        long estimatedIndexBytes(int fields){return 256L+512L*count*Math.max(1,fields)+8L*indexRawBytes;}
        Object readTransient(int index){long estimate=estimate(index);if(budget!=null)budget.reserveTransient(estimate);try{return read(index);}finally{if(budget!=null)budget.finishTransient(estimate);}}
        Object read(int index) {
            try{return readValue(index);}catch(RuntimeException failure){RuntimeException normalized=rowFailure(failure);if(normalized instanceof ConfigException error)throw error.locate("row",version,string(field(schema,"name")),String.valueOf(keys.get(index)),"",relativePath);throw normalized;}
        }
        private Object readValue(int index) {
            Wire wire=new Wire(this.wire);wire.reset(); Object row;
            if (fragments != null) row = new Parser(fragments[index],jsonBounds).parse();
            else { var cursor = new Cursor(bytes);if(budget!=null)cursor.maximumString=budget.limits.maxStringBytes; cursor.data.position(offsets[index]); cursor.data.limit(offsets[index]+lengths[index]); row = wire.fields(array(field(schema,"fields")),cursor,0); if (cursor.data.hasRemaining()) throw new ConfigException(ErrorCode.InvalidValue,"Row boundary mismatch"); }
            wire.reset(); wire.validateFields(array(field(schema,"fields")),row,0,false);
            for (var key : object(keys.get(index)).entrySet()) if (!Objects.equals(key.getValue(),field(row,key.getKey()))) throw new ConfigException(ErrorCode.InvalidValue,"Row key differs from index");
            if (ids[index] != (bool(field(schema,"hasId")) ? integer(field(row,"Id"),Integer.MIN_VALUE,Integer.MAX_VALUE) : 0)) throw new ConfigException(ErrorCode.InvalidValue,"Row Id differs from index");
            if (resolve != null) validateReferences(row,resolve);
            return row;
        }
        void validateReferences(Object row, java.util.function.Function<String,IndexedRows> resolve) {
            for (Object f : array(field(schema,"fields"))) {
                Object type = wire.types.get(string(field(f,"type")));
                if ("tablereference".equals(field(type,"kind"))) { int id = (int)integer(field(row,string(field(f,"name"))),Integer.MIN_VALUE,Integer.MAX_VALUE); if (id != 0) try{resolve.apply(string(field(type,"referenceTable"))).reference(id);}catch(ConfigException error){throw error.locate("reference",version,string(field(schema,"name")),"",string(field(f,"name")),relativePath);} }
            }
        }
    }
    private static void appendJson(StringBuilder text,Object value) {
        if (value == null) { text.append("null"); return; }
        if (value instanceof Boolean || value instanceof NumberText) { text.append(value instanceof NumberText n ? n.text() : value.toString()); return; }
        if (value instanceof String s) {
            text.append('"'); for (int i = 0; i < s.length(); i++) { char c = s.charAt(i); switch(c) {
                case '"', '\\' -> text.append('\\').append(c); case '\n' -> text.append("\\n"); case '\r' -> text.append("\\r"); case '\t' -> text.append("\\t");
                default -> { if (c < 32) { text.append("\\u00").append(Character.forDigit(c >>> 4,16)).append(Character.forDigit(c & 15,16)); } else text.append(c); }
            }} text.append('"'); return;
        }
        if (value instanceof List<?> list) { text.append('['); for (int i = 0; i < list.size(); i++) { if (i != 0) text.append(','); appendJson(text,list.get(i)); } text.append(']'); return; }
        text.append('{'); boolean comma = false; for (var item : object(value).entrySet()) { if (comma) text.append(','); comma = true; appendJson(text,item.getKey()); text.append(':'); appendJson(text,item.getValue()); } text.append('}');
    }
    // One gate per derived index. Factories and all parser work run outside it.
    record DerivedResult<T>(T value,long retained) {}
    static final class DerivedValue<T> {
        private volatile T ready;
        private RuntimeException failure;
        private boolean running;
        T get(){T value=ready;if(value!=null)return value;synchronized(this){if(failure!=null)throw failure;}throw new ConfigException(ErrorCode.TableNotLoaded,"Load the group index explicitly");}
        T load(Cancellation token,LoadBudget budget,long estimate,java.util.function.Supplier<DerivedResult<T>> factory){
            token.check();T value=ready;if(value!=null)return value;boolean owner;
            synchronized(this){value=ready;if(value!=null)return value;if(failure!=null)throw failure;owner=!running;budget.admit(owner);if(owner)running=true;}
            try {
                if(owner){boolean reserved=false;try{budget.reserveIndex(estimate);reserved=true;var built=factory.get();if(built.retained()>estimate)budget.reserveIndex(built.retained()-estimate);else budget.releaseIndex(estimate-built.retained());estimate=built.retained();value=built.value();ready=value;reserved=false;}catch(RuntimeException error){RuntimeException normalized=rowFailure(error);synchronized(this){if(terminalRowFailure(normalized)){failure=normalized;budget.groupFault();}}throw normalized;}finally{if(reserved)budget.releaseIndex(estimate);synchronized(this){running=false;notifyAll();}}}
                else {synchronized(this){while(running){token.check();try{wait(10);}catch(InterruptedException error){Thread.currentThread().interrupt();throw new ConfigException(ErrorCode.Cancelled,"Group wait interrupted","group",0,error);}}if(failure!=null)throw failure;value=ready;}if(value==null)return load(token,budget,estimate,factory);}
                token.check();return value;
            } finally {budget.release(owner);}
        }
    }
    public abstract static class RowStorage<R> {
        private final IndexedRows indexed;
        private final java.util.function.Function<Object,R> decode;
        private final java.util.concurrent.atomic.AtomicReferenceArray<Object> slots;
        private final java.util.concurrent.atomic.AtomicInteger loaded = new java.util.concurrent.atomic.AtomicInteger();
        private volatile List<R> all;
        private final Object tableGate = new Object();
        private static final class RowWork {
            final java.util.concurrent.CompletableFuture<Object> future=new java.util.concurrent.CompletableFuture<>();
            final Set<java.util.concurrent.CompletableFuture<Object>> waiters=Collections.newSetFromMap(new IdentityHashMap<>());
            final java.util.concurrent.atomic.AtomicBoolean released=new java.util.concurrent.atomic.AtomicBoolean();
            void finish(Object row,Throwable error){
                List<java.util.concurrent.CompletableFuture<Object>> subscribers;
                synchronized(waiters){if(error==null)future.complete(row);else future.completeExceptionally(error);subscribers=List.copyOf(waiters);waiters.clear();}
                for(var subscriber:subscribers)if(error==null)subscriber.complete(row);else subscriber.completeExceptionally(error);
            }
            void release(LoadBudget budget){if(budget!=null&&released.compareAndSet(false,true))budget.release(true);}
        }
        private record RowFailure(RuntimeException error) {}
        RowStorage(IndexedRows indexed,java.util.function.Function<Object,R> decode) { this.indexed = indexed; this.decode = decode; slots = new java.util.concurrent.atomic.AtomicReferenceArray<>(indexed.count); }
        public final int count() { return indexed.count; }
        public final int loadedCount() { return loaded.get(); }
        public final boolean isComplete() { return all != null; }
        @SuppressWarnings("unchecked")
        protected final R queryOrdinal(int ordinal) {
            if (ordinal < 0) return null; Object value = slots.get(ordinal);
            if (value instanceof RowFailure failure) throw failure.error();
            if (value == null || value instanceof RowWork) throw new ConfigException(ErrorCode.RowNotLoaded,"Load the row explicitly");
            return (R)value;
        }
        @SuppressWarnings("unchecked")
        protected final R cachedOrdinal(int ordinal){Object value=slots.get(ordinal);return value==null||value instanceof RowFailure||value instanceof RowWork?null:(R)value;}
        private RowWork claim(int ordinal){
            var budget=indexed.budget;if(budget!=null)budget.admit(true);boolean owned=false;
            try{var work=new RowWork();owned=slots.compareAndSet(ordinal,null,work);return owned?work:null;}finally{if(!owned&&budget!=null)budget.release(true);}
        }
        private R complete(int ordinal,RowWork work) {
            var budget=indexed.budget;long estimate=indexed.estimate(ordinal);boolean reserved=false,success=false,terminal=false;
            try {
                if(budget!=null){budget.reserve(estimate);reserved=true;}
                R row=decode.apply(indexed.read(ordinal));slots.set(ordinal,row);loaded.incrementAndGet();success=true;work.finish(row,null);return row;
            } catch(Throwable failure) {
                Throwable error=failure;
                try{if(failure instanceof RuntimeException runtime)error=rowFailure(runtime);terminal=error instanceof RuntimeException runtime&&terminalRowFailure(runtime);slots.set(ordinal,terminal?new RowFailure((RuntimeException)error):null);}
                catch(Throwable fatal){error=fatal;terminal=false;slots.compareAndSet(ordinal,work,null);}
                work.finish(null,error);if(error instanceof RuntimeException runtime)throw runtime;if(error instanceof Error fatal)throw fatal;throw new IllegalStateException(error);
            } finally{if(budget!=null&&reserved)budget.finish(estimate,success,terminal);work.release(budget);}
        }
        @SuppressWarnings("unchecked")
        protected final R loadOrdinal(int ordinal,Cancellation cancellation) {
            cancellation.check();if(ordinal<0)return null;Object value;
            while((value=slots.get(ordinal))==null){var work=claim(ordinal);if(work!=null){R row=complete(ordinal,work);cancellation.check();return row;}cancellation.check();}
            if(value instanceof RowFailure failure)throw failure.error();if(!(value instanceof RowWork work))return (R)value;
            var budget=indexed.budget;if(budget!=null)budget.admit(false);
            try{while(true){cancellation.check();try{R row=(R)work.future.get(10,java.util.concurrent.TimeUnit.MILLISECONDS);cancellation.check();return row;}
                catch(java.util.concurrent.TimeoutException ignored){}
                catch(InterruptedException error){Thread.currentThread().interrupt();throw new ConfigException(ErrorCode.Cancelled,"Wait interrupted","row",0,error);}
                catch(java.util.concurrent.ExecutionException error){if(error.getCause() instanceof RuntimeException cause)throw cause;if(error.getCause() instanceof Error cause)throw cause;throw new IllegalStateException(error.getCause());}
            }}finally{if(budget!=null)budget.release(false);}
        }
        protected final java.util.concurrent.CompletableFuture<R> loadOrdinalAsync(int ordinal,Cancellation cancellation,java.util.concurrent.Executor executor) {
            Objects.requireNonNull(executor);cancellation.check();if(ordinal<0)return java.util.concurrent.CompletableFuture.completedFuture(null);Object value;
            while((value=slots.get(ordinal))==null){var work=claim(ordinal);if(work!=null){
                try{executor.execute(()->{try{complete(ordinal,work);}catch(RuntimeException|Error ignored){/* shared result completed above */}});}
                catch(RuntimeException|Error error){slots.compareAndSet(ordinal,work,null);work.finish(null,error);work.release(indexed.budget);}
                value=work;break;
            }cancellation.check();}
            if(value instanceof RowFailure failure)return java.util.concurrent.CompletableFuture.failedFuture(failure.error());
            if(value instanceof RowWork work){
                var budget=indexed.budget;if(budget!=null)budget.admit(false);
                var waiting=new java.util.concurrent.CompletableFuture<Object>();
                // Register in a removable collection. Cancelled callers cannot accumulate
                // dependent callbacks on a stalled shared CompletableFuture.
                var released=new java.util.concurrent.atomic.AtomicBoolean();
                Runnable remove=cancellation.subscribe(()->{synchronized(work.waiters){work.waiters.remove(waiting);}waiting.completeExceptionally(new ConfigException(ErrorCode.Cancelled,"Row wait cancelled"));});
                waiting.whenComplete((row,error)->{remove.run();synchronized(work.waiters){work.waiters.remove(waiting);}if(budget!=null&&released.compareAndSet(false,true))budget.release(false);});
                boolean ready;synchronized(work.waiters){ready=work.future.isDone();if(!ready&&!waiting.isDone())work.waiters.add(waiting);}
                if(ready){try{waiting.complete(work.future.join());}catch(java.util.concurrent.CompletionException error){waiting.completeExceptionally(error.getCause());}}
                @SuppressWarnings("unchecked") var typed=(java.util.concurrent.CompletableFuture<R>)(java.util.concurrent.CompletableFuture<?>)waiting;return typed;
            }
            @SuppressWarnings("unchecked") R row=(R)value;return java.util.concurrent.CompletableFuture.completedFuture(row);
        }
        protected final java.util.concurrent.CompletableFuture<R> requireRow(java.util.concurrent.CompletableFuture<R> waiting){
            var result=new java.util.concurrent.CompletableFuture<R>();waiting.whenComplete((row,error)->{if(error!=null)result.completeExceptionally(error);else if(row==null)result.completeExceptionally(new ConfigException(ErrorCode.KeyNotFound,"Row key not found"));else result.complete(row);});
            result.whenComplete((row,error)->{if(result.isCancelled())waiting.cancel(false);});return result;
        }
        public final R reference(int id,boolean load) { if (id == 0) return null; int ordinal = indexed.reference(id); return load ? loadOrdinal(ordinal,new Cancellation()) : queryOrdinal(ordinal); }
        public final List<R> rows() { List<R> value = all; if (value == null) throw new ConfigException(ErrorCode.TableNotLoaded,"Load the table explicitly"); return value; }
        public final List<R> loadAll(Cancellation cancellation) {
            cancellation.check(); List<R> value = all; if (value != null) return value;
            var rows = new ArrayList<R>(count()); for (int i = 0; i < count(); i++) rows.add(loadOrdinal(i,cancellation));
            prepareGroups(cancellation); cancellation.check();
            synchronized(tableGate) { if (all == null) all = List.copyOf(rows); return all; }
        }
        protected abstract void prepareGroups(Cancellation cancellation);
        final void validate(java.util.function.Function<String,IndexedRows> resolve,Cancellation cancellation) {
            for (int i = 0; i < count(); i++) { cancellation.check(); Object row = indexed.readTransient(i); decode.apply(row); indexed.validateReferences(row,resolve); }
        }
    }

    private static final class Cursor {
        final ByteBuffer data;
        int maximumString=16<<20;
        Cursor(byte[] bytes) { data = ByteBuffer.wrap(bytes).order(ByteOrder.LITTLE_ENDIAN); }
        int count(int max) {
            int n = data.getInt();
            if (n < 0 || n > max)
                throw invalid("长度无效");
            return n;
        }
        boolean bool() {
            byte n = data.get();
            if (n != 0 && n != 1)
                throw invalid("bool 编码无效");
            return n != 0;
        }
        String text() {
            int n = data.getInt();
            if (n == -1)
                return null;
            if (n < 0 || n > 16 * 1024 * 1024 || n > data.remaining())
                throw invalid("字符串长度无效");
            if(n>maximumString)throw LoadBudget.limit();
            var slice = data.slice();
            slice.limit(n);
            data.position(data.position() + n);
            try {
                return StandardCharsets.UTF_8.newDecoder()
                    .onMalformedInput(CodingErrorAction.REPORT)
                    .decode(slice)
                    .toString();
            } catch (CharacterCodingException e) {
                throw new ConfigException(ErrorCode.InvalidEncoding,"Invalid UTF-8","decode",0,e);
            }
        }
    }
    private static final class Wire {
        final Object schema;
        final Map<String, Object> types;
        ResourceLimits limits;
        String[] tags;
        int remaining;
        Wire(Object schema) {
            this.schema = schema;types=new HashMap<>();
            for (Object type : array(field(schema, "types")))
                types.put(string(field(type, "id")), type);
        }
        Wire(Wire shared){schema=shared.schema;types=shared.types;tags=shared.tags;limits=shared.limits;}
        void reset(){remaining=limits==null?8_000_000:limits.maxContainerElements;}
        List<Object> read(byte[] bytes, boolean json) {
            if (json) {
                Object doc = parse(bytes);
                if (object(doc).size() != 4 || !"ZrConfig.TableJson/1".equals(field(doc, "format")) ||
                    !field(schema, "name").equals(field(doc, "table")) ||
                    !field(schema, "hash").equals(field(doc, "schemaHash")))
                    throw invalid("JSON 表头不匹配");
                var rows = array(field(doc, "rows"));
                if (rows.size() > MAX_ROWS)
                    throw invalid("行数过大");
                for (var row : rows) {
                    remaining = 8_000_000;
                    validateFields(array(field(schema, "fields")), row, 0, false);
                }
                return rows;
            }
            Cursor r = new Cursor(bytes);
            if (r.data.getInt() != 0x4643525a || r.data.getInt() != 4 || !field(schema, "name").equals(r.text()) ||
                !field(schema, "hash").equals(r.text()))
                throw invalid("二进制表头不匹配");
            int count = r.count(MAX_ROWS);
            if ((long)count * 12 > r.data.remaining())
                throw invalid("行索引长度不足");
            Set<String> allowed = new HashSet<>();
            for (Object v : array(field(schema, "tags")))
                allowed.add(string(v));
            int nt = r.count(allowed.size());
            tags = new String[nt];
            Set<String> seen = new HashSet<>();
            for (int i = 0; i < nt; i++) {
                tags[i] = r.text();
                if (!allowed.contains(tags[i]) || !seen.add(tags[i]))
                    throw invalid("类型映射无效");
            }
            var fields = array(field(schema, "fields"));
            var keys = array(field(schema, "keys"));
            int[] offsets = new int[count], lengths = new int[count], ids = new int[count];
            List<List<Object>> indexKeys = new ArrayList<>(count);
            for (int i = 0; i < count; i++) {
                remaining = 8_000_000;
                var key = new ArrayList<Object>();
                for (Object k : keys)
                    key.add(value(fieldType(fields, string(k)), r, 0));
                indexKeys.add(key);
                ids[i] = r.data.getInt();
                offsets[i] = r.data.getInt();
                lengths[i] = r.data.getInt();
                if (offsets[i] < 0 || lengths[i] < 0 ||
                    (i == 0 ? offsets[i] != 0 : (long)offsets[i - 1] + lengths[i - 1] != offsets[i]))
                    throw invalid("行索引无效");
            }
            int start = r.data.position();
            if ((long)start + (count == 0 ? 0 : (long)offsets[count - 1] + lengths[count - 1]) != bytes.length)
                throw invalid("文件边界不匹配");
            var rows = new ArrayList<Object>(count);
            for (int i = 0; i < count; i++) {
                remaining = 8_000_000;
                Object row = fields(fields, r, 0);
                if ((long)r.data.position() != (long)start + offsets[i] + lengths[i])
                    throw invalid("行边界不匹配");
                for (int k = 0; k < keys.size(); k++)
                    if (!Objects.equals(indexKeys.get(i).get(k), field(row, string(keys.get(k)))))
                        throw invalid("行主键与索引不同");
                if (ids[i] != (bool(field(schema, "hasId"))
                                   ? integer(field(row, "Id"), Integer.MIN_VALUE, Integer.MAX_VALUE)
                                   : 0))
                    throw invalid("行 Id 不匹配");
                rows.add(row);
            }
            return rows;
        }
        String fieldType(List<Object> fields, String name) {
            for (Object f : fields)
                if (name.equals(field(f, "name")))
                    return string(field(f, "type"));
            throw invalid("未知字段");
        }
        void claim(long count) {
            if(limits!=null&&(count<0||count>remaining))throw LoadBudget.limit();
            if (count < 0 || count > MAX_ELEMENTS || count > remaining)
                throw invalid("元素预算超限");
            remaining -= (int)count;
        }
        private RuntimeException fieldFailure(RuntimeException failure,String prefix){if(limits==null)return failure;RuntimeException normalized=rowFailure(failure);if(!(normalized instanceof ConfigException error))return normalized;String child=error.context.fieldPath();String path=prefix+(child.isEmpty()?"":child.startsWith("[")?"":".")+child;return new ConfigException(error.code,error.getMessage(),error.stage,error.operationId,error,new ErrorContext("","","",path,""));}
        Map<String, Object> fields(List<Object> fields, Cursor r, int depth) {
            var value = new LinkedHashMap<String, Object>();
            for (Object f : fields) {String name=string(field(f,"name"));try{value.put(name,value(string(field(f,"type")),r,depth+1));}catch(RuntimeException error){throw fieldFailure(error,name);}}
            return value;
        }
        private Object nestedRead(String id,Cursor r,int depth,String path){try{return value(id,r,depth);}catch(RuntimeException error){throw fieldFailure(error,path);}}
        private void nestedValidate(String id,Object v,int depth,String path){try{validate(id,v,depth);}catch(RuntimeException error){throw fieldFailure(error,path);}}
        Object value(String id, Cursor r, int depth) {
            if(limits!=null&&depth>limits.maxDepth)throw LoadBudget.limit();
            if (depth > MAX_DEPTH)
                throw invalid("嵌套过深");
            Object t = types.get(id);
            String kind = string(field(t, "kind"));
            String primitive = string(field(t, "primitive"));
            if (kind.equals("primitive") || kind.equals("enum") || kind.equals("tablereference")) {
                Object v = primitive(primitive, r);
                if (kind.equals("enum") &&
                    !array(field(t, "enumValues")).contains(v instanceof NumberText n ? n.text : v))
                    throw invalid("未知枚举值");
                return v;
            }
            if (kind.equals("rawjson"))
                return Objects.requireNonNullElse(r.text(), "null");
            if (kind.equals("nullable"))
                return r.bool() ? value(string(field(t, "element")), r, depth + 1) : null;
            if (kind.equals("array")) {
                int rank = (int)integer(field(t, "rank"), 1, 3);
                long count = 1;
                var dims = new ArrayList<Object>();
                for (int i = 0; i < rank; i++) {
                    int n = r.count(MAX_ELEMENTS);
                    count = Math.multiplyExact(count, n);
                    dims.add(new NumberText(Integer.toString(n)));
                }
                claim(count);
                var values = new ArrayList<Object>((int)count);
                for (int i=0;i<count;i++)try{values.add(value(string(field(t,"element")),r,depth+1));}catch(RuntimeException error){throw fieldFailure(error,"["+i+"]");}
                return rank == 1 ? values : Map.of("dimensions", dims, "values", values);
            }
            if (kind.equals("dictionary")) {
                int count = r.count(MAX_ELEMENTS);
                claim(count);
                var values = new ArrayList<Object>(count);
                for (int i = 0; i < count; i++)
                    values.add(Arrays.asList(nestedRead(string(field(t,"key")),r,depth+1,"["+i+"].key"),nestedRead(string(field(t,"element")),r,depth+1,"["+i+"].value")));
                return values;
            }
            var variants = array(field(t, "variants"));
            if (kind.equals("object") && !variants.isEmpty()) {
                int tag = r.data.getInt();
                if (tag == -1)
                    return null;
                if (tag < 0 || tag >= tags.length || !variants.contains(tags[tag]))
                    throw invalid("多态类型不匹配");
                var v = fields(array(field(types.get(tags[tag]), "fields")), r, depth);
                v.put("$type", tags[tag]);
                return v;
            }
            if (bool(field(t, "nullableObject")) && !r.bool())
                return null;
            return fields(array(field(t, "fields")), r, depth);
        }
        Object primitive(String kind, Cursor r) {
            String text;
            switch (kind) {
            case "string":
                return Objects.requireNonNullElse(r.text(), "");
            case "bool":
                return r.bool();
            case "int8":
                text = Byte.toString(r.data.get());
                break;
            case "uint8":
                text = Integer.toString(Byte.toUnsignedInt(r.data.get()));
                break;
            case "int16":
                text = Short.toString(r.data.getShort());
                break;
            case "uint16":
                text = Integer.toString(Short.toUnsignedInt(r.data.getShort()));
                break;
            case "int32":
                text = Integer.toString(r.data.getInt());
                break;
            case "uint32":
                text = Long.toString(Integer.toUnsignedLong(r.data.getInt()));
                break;
            case "int64":
                return Long.toString(r.data.getLong());
            case "uint64":
                return Long.toUnsignedString(r.data.getLong());
            case "decimal": {
                long lo = Integer.toUnsignedLong(r.data.getInt()), mid = Integer.toUnsignedLong(r.data.getInt()),
                     hi = Integer.toUnsignedLong(r.data.getInt());
                int flags = r.data.getInt(), scale = (flags >>> 16) & 255;
                if ((flags & 0x7f00ffff) != 0 || scale > 28)
                    throw invalid("decimal 编码无效");
                BigInteger coefficient = BigInteger.valueOf(hi)
                                             .shiftLeft(64)
                                             .or(BigInteger.valueOf(mid).shiftLeft(32))
                                             .or(BigInteger.valueOf(lo));
                if (flags < 0)
                    coefficient = coefficient.negate();
                return new BigDecimal(coefficient, scale).toPlainString();
            }
            case "float32":
                float f = r.data.getFloat();
                if (!Float.isFinite(f))
                    throw invalid("浮点数无效");
                text = Float.toString(f);
                break;
            case "float64":
                double d = r.data.getDouble();
                if (!Double.isFinite(d))
                    throw invalid("浮点数无效");
                text = Double.toString(d);
                break;
            default:
                throw invalid("未知标量");
            }
            return new NumberText(text);
        }
        void validateFields(List<Object> fields, Object value, int depth, boolean tagged) {
            if (object(value).size() != fields.size() + (tagged ? 1 : 0))
                throw invalid("未知或缺失成员");
            for (Object f : fields) {String name=string(field(f,"name"));try{validate(string(field(f,"type")),field(value,name),depth+1);}catch(RuntimeException error){throw fieldFailure(error,name);}}
        }
        void validate(String id, Object v, int depth) {
            if(limits!=null&&depth>limits.maxDepth)throw LoadBudget.limit();
            if (depth > MAX_DEPTH)
                throw invalid("嵌套过深");
            Object t = types.get(id);
            String kind = string(field(t, "kind")), p = string(field(t, "primitive"));
            if (kind.equals("primitive") || kind.equals("enum") || kind.equals("tablereference")) {
                switch (p) {
                case "string" -> {String value=string(v);if(limits!=null&&value!=null&&value.getBytes(StandardCharsets.UTF_8).length>limits.maxStringBytes)throw LoadBudget.limit();}
                case "bool" -> bool(v);
                case "int8" -> integer(v, -128, 127);
                case "uint8" -> integer(v, 0, 255);
                case "int16" -> integer(v, -32768, 32767);
                case "uint16" -> integer(v, 0, 65535);
                case "int32" -> integer(v, Integer.MIN_VALUE, Integer.MAX_VALUE);
                case "uint32" -> integer(v, 0, 0xffffffffL);
                case "int64" -> int64(v);
                case "uint64" -> uint64(v);
                case "decimal" -> new Decimal(string(v));
                case "float32" -> float32(v);
                case "float64" -> float64(v);
                default -> throw invalid("未知标量");
                }
                if (kind.equals("enum") &&
                    !array(field(t, "enumValues")).contains(v instanceof NumberText n ? n.text : v))
                    throw invalid("未知枚举值");
                return;
            }
            if (kind.equals("rawjson")) {
                if(limits!=null&&string(v).getBytes(StandardCharsets.UTF_8).length>limits.maxStringBytes)throw LoadBudget.limit();
                parse(string(v));
                return;
            }
            if (kind.equals("nullable")) {
                if (v != null)
                    validate(string(field(t, "element")), v, depth + 1);
                return;
            }
            if (kind.equals("array")) {
                int rank = (int)integer(field(t, "rank"), 1, 3);
                var values = array(rank == 1 ? v : field(v, "values"));
                claim(values.size());
                if (rank > 1) {
                    var dims = array(field(v, "dimensions"));
                    if (dims.size() != rank || object(v).size() != 2)
                        throw invalid("矩阵维度无效");
                    long n = 1;
                    for (var d : dims)
                        n = Math.multiplyExact(n, integer(d, 0, MAX_ELEMENTS));
                    if (n != values.size())
                        throw invalid("矩阵长度不匹配");
                }
                for (int i=0;i<values.size();i++)nestedValidate(string(field(t,"element")),values.get(i),depth+1,"["+i+"]");
                return;
            }
            if (kind.equals("dictionary")) {
                var values = array(v);
                claim(values.size());
                for (int i=0;i<values.size();i++) {
                    var pair = array(values.get(i));
                    if (pair.size() != 2)
                        throw invalid("字典条目无效");
                    nestedValidate(string(field(t,"key")),pair.get(0),depth+1,"["+i+"].key");
                    nestedValidate(string(field(t,"element")),pair.get(1),depth+1,"["+i+"].value");
                }
                return;
            }
            if (v == null && bool(field(t, "nullableObject")))
                return;
            var variants = array(field(t, "variants"));
            if (!variants.isEmpty()) {
                String tag = string(field(v, "$type"));
                if (!variants.contains(tag))
                    throw invalid("多态标记无效");
                validateFields(array(field(types.get(tag), "fields")), v, depth, true);
            } else
                validateFields(array(field(t, "fields")), v, depth, false);
        }
    }
}
