// zrconfig-api-contract:1
// 公共运行库：解析、转换和索引分配集中在加载阶段，查询只读取已建立的数据。
declare function require(name: string): any;
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const utf8 = new TextDecoder('utf-8', { fatal: true });
const encoder = new TextEncoder();
class DataFailure extends Error {}
export function invalid(message: string): never { throw new DataFailure('配置数据无效：' + message); }
export type ErrorCode = 'NotInitialized' | 'ManagerClosed' | 'ReloadBusy' | 'StaleCandidate' | 'AlreadyCommitted' |
    'ForeignCandidate' | 'Cancelled' | 'DeadlineExceeded' | 'InvalidPlan' | 'UnsupportedCapability' |
    'SourceUnavailable' | 'ReadFailed' | 'InvalidManifest' | 'UnsupportedFormat' | 'TableSetMismatch' | 'IntegrityMismatch' |
    'SchemaMismatch' | 'InvalidEncoding' | 'InvalidValue' | 'DuplicateKey' | 'MissingReference' | 'KeyNotFound' | 'TableNotSelected' | 'TableNotLoaded' | 'RowNotLoaded' | 'LimitExceeded' | 'BudgetExceeded' | 'VersionIdentityConflict' | 'CatalogMismatch' | 'InternalError';
export interface ErrorContext { readonly versionId?:string;readonly table?:string;readonly rowKey?:string;readonly fieldPath?:string;readonly relativePath?:string; }
export class ConfigError extends Error {
    readonly context:Readonly<ErrorContext>;
    constructor(readonly code: ErrorCode, message: string, readonly stage = '', readonly operationId = 0n, options?: ErrorOptions, context:ErrorContext={}) {
        super(message, options); this.name = 'ConfigError';this.context=Object.freeze({...context});Object.freeze(this);
    }
}
function locateError(error:unknown,stage:string,context:ErrorContext):unknown{const value=rowFailure(error);if(!(value instanceof ConfigError))return value;return new ConfigError(value.code,value.message,value.stage||stage,value.operationId,{cause:value},{...context,...Object.fromEntries(Object.entries(value.context).filter(([,field])=>field!==undefined&&field!==''))});}
function rowFailure(error: unknown): unknown {
    return error instanceof DataFailure ? new ConfigError('InvalidValue', error.message, 'row', 0n, { cause: error }) : error;
}
function fieldError(failure:unknown,prefix:string):unknown{const error=rowFailure(failure);if(!(error instanceof ConfigError))return error;const child=error.context.fieldPath??'';return new ConfigError(error.code,error.message,error.stage,error.operationId,{cause:error},{...error.context,fieldPath:prefix+(child&&!child.startsWith('[')?'.':'')+child});}
function terminalRowFailure(error: unknown): boolean {
    return error instanceof ConfigError && ['InvalidEncoding','InvalidValue','DuplicateKey','MissingReference','IntegrityMismatch','SchemaMismatch','LimitExceeded'].includes(error.code);
}
function decodeUTF8(bytes: Uint8Array): string {
    try { return utf8.decode(bytes); }
    catch (cause) { throw new ConfigError('InvalidEncoding','Invalid UTF-8','decode',0n,{ cause }); }
}
export class Cancellation {
    #cancelled=false;#listeners=new Set<()=>void>();#parent?:Cancellation;#deadline=Infinity;
    cancel():void{if(this.#cancelled)return;this.#cancelled=true;const listeners=[...this.#listeners];this.#listeners.clear();for(const listener of listeners)listener();}
    get isCancelled():boolean{let current:Cancellation|undefined=this;while(current){if(current.#cancelled)return true;current=current.#parent;}return false;}
    check():void{if(this.isCancelled)throw new ConfigError('Cancelled','Operation cancelled');let current:Cancellation|undefined=this;const now=performance.now();while(current){if(now>=current.#deadline)throw new ConfigError('DeadlineExceeded','Operation deadline exceeded');current=current.#parent;}}
    child(deadline:number):Cancellation{if(!Number.isFinite(deadline))throw new ConfigError('InvalidPlan','Invalid operation deadline');const child=new Cancellation();child.#parent=this;child.#deadline=deadline;return child;}
    subscribe(listener:()=>void):()=>void{
        try{this.check();}catch{listener();return()=>{};}
        let finished=false,timer:ReturnType<typeof setTimeout>|undefined;let unlink=()=>{};
        const cleanup=()=>{if(finished)return;finished=true;this.#listeners.delete(complete);unlink();if(timer!==undefined)clearTimeout(timer);};
        const complete=()=>{if(finished)return;cleanup();listener();};this.#listeners.add(complete);
        if(this.#parent)unlink=this.#parent.subscribe(complete);
        if(Number.isFinite(this.#deadline)){const schedule=()=>{if(finished)return;const remaining=this.#deadline-performance.now();if(remaining<=0){complete();return;}timer=setTimeout(schedule,Math.min(remaining,2147483647));};schedule();}
        return cleanup;
    }
}
export interface ResourceLimitOptions {
    readonly maxManifestBytes?: number; readonly maxTableBytes?: number; readonly maxSnapshotBytes?: number;
    readonly maxTables?: number; readonly maxRows?: number; readonly maxDecodedRows?: number;
    readonly maxContainerElements?: number; readonly maxDepth?: number; readonly maxStringBytes?: number;
    readonly maxEstimatedIndexBytes?: number; readonly maxPendingRequests?: number; readonly maxEstimatedDecodedBytes?: number;
}
/** Immutable, validated before any source is opened. Counts refer to the entire snapshot unless named Table. */
export class ResourceLimits {
    readonly maxManifestBytes: number; readonly maxTableBytes: number; readonly maxSnapshotBytes: number;
    readonly maxTables: number; readonly maxRows: number; readonly maxDecodedRows: number;
    readonly maxContainerElements: number; readonly maxDepth: number; readonly maxStringBytes: number;
    readonly maxEstimatedIndexBytes: number; readonly maxPendingRequests: number; readonly maxEstimatedDecodedBytes: number;
    constructor(options: ResourceLimitOptions = {}) {
        const defaults = { maxManifestBytes: 16777216, maxTableBytes: 536870912, maxSnapshotBytes: 1073741824,
            maxTables: 100000, maxRows: 5000000, maxDecodedRows: 5000000, maxContainerElements: 1000000,
            maxDepth: 64, maxStringBytes: 16777216, maxPendingRequests: 4096, maxEstimatedDecodedBytes: 2147483648, maxEstimatedIndexBytes: 2147483648 };
        for (const name of Object.keys(options)) if (!Object.hasOwn(defaults,name)) throw new ConfigError('InvalidPlan','Unknown resource limit: '+name);
        const value = { ...defaults, ...options };
        for (const [name, count] of Object.entries(value)) if (!Number.isSafeInteger(count) || count <= 0) throw new ConfigError('InvalidPlan','Invalid resource limit: '+name);
        if (value.maxRows > 5000000 || value.maxContainerElements > 1000000 || value.maxDepth > 64 || value.maxStringBytes > 16777216)
            throw new ConfigError('UnsupportedCapability','Requested limit exceeds the supported wire format');
        this.maxManifestBytes=value.maxManifestBytes; this.maxTableBytes=value.maxTableBytes; this.maxSnapshotBytes=value.maxSnapshotBytes;
        this.maxTables=value.maxTables; this.maxRows=value.maxRows; this.maxDecodedRows=value.maxDecodedRows;
        this.maxContainerElements=value.maxContainerElements; this.maxDepth=value.maxDepth; this.maxStringBytes=value.maxStringBytes;
        this.maxEstimatedIndexBytes=value.maxEstimatedIndexBytes; this.maxPendingRequests=value.maxPendingRequests; this.maxEstimatedDecodedBytes=value.maxEstimatedDecodedBytes;
        Object.freeze(this);
    }
}
export interface ConfigSource { read(relativePath: string, maxBytes: number, cancellation: Cancellation): Uint8Array; }
export interface AsyncConfigSource { readAsync(relativePath: string, maxBytes: number, cancellation: Cancellation): Promise<Uint8Array>; }
function validSourcePath(relative: string): void {
    if (!relative || relative.split('/').some(part => !part || part==='.' || part==='..' || /[\\:\x00-\x1f]/.test(part))) throw new ConfigError('InvalidPlan','Invalid relative source path');
}
function sourcePath(root: string, relative: string): string {
    validSourcePath(relative); let current=root;
    if (fs.lstatSync(current).isSymbolicLink()) throw new ConfigError('SourceUnavailable','Source root is a symbolic link');
    for (const part of relative.split('/')) { current=path.join(current,part); if(fs.lstatSync(current).isSymbolicLink()) throw new ConfigError('SourceUnavailable','Source path contains a symbolic link'); }
    return current;
}
function readFailure(error: unknown): never {
    if (error instanceof ConfigError) throw error;
    if (error && typeof error === 'object' && 'code' in error && typeof error.code === 'string' && /^E[A-Z]+$/.test(error.code))
        throw new ConfigError(error.code==='ENOENT' ? 'SourceUnavailable' : 'ReadFailed','Cannot read configuration source','read',0n,{cause:error});
    throw error;
}
function locatedReadFailure(error:unknown,relativePath:string):never {
    const parts=relativePath.split('/');
    try{readFailure(error);}catch(failure){throw locateError(failure,'read',{relativePath,versionId:parts.length===3?parts[1]:undefined,table:parts.length===3?parts[2].replace(/\.[^.]+$/,''):undefined});}
}
/** Immutable owned memory source; returned buffers are independent copies. */
export class MemoryConfigSource implements ConfigSource, AsyncConfigSource {
    readonly #files=new Map<string,Uint8Array>();
    constructor(files: ReadonlyMap<string,Uint8Array>) { for(const [name,bytes] of files) { validSourcePath(name); if(!(bytes instanceof Uint8Array)) throw new ConfigError('InvalidPlan','Source bytes required'); this.#files.set(name,new Uint8Array(bytes)); } }
    read(name: string,maximum: number,cancellation: Cancellation): Uint8Array {
        cancellation.check(); validSourcePath(name); checkReadLimit(maximum); const bytes=this.#files.get(name);
        if(!bytes) throw new ConfigError('SourceUnavailable','Source file is missing: '+name);
        if(bytes.byteLength>maximum) throw new ConfigError('LimitExceeded','Source exceeds byte limit'); return new Uint8Array(bytes);
    }
    async readAsync(name: string,maximum: number,cancellation: Cancellation): Promise<Uint8Array> { return this.read(name,maximum,cancellation); }
}
function checkReadLimit(maximum: number): void { if(!Number.isSafeInteger(maximum)||maximum<=0) throw new ConfigError('InvalidPlan','Invalid source byte limit'); }
/** The application owns an immutable root while reading. Links are rejected; file descriptors are always closed. */
export class FileConfigSource implements ConfigSource, AsyncConfigSource {
    readonly #root: string;
    constructor(root: string) { this.#root=path.resolve(root); }
    get sourceNamespace():string{return this.#root;}
    read(relative: string, maxBytes: number, cancellation: Cancellation): Uint8Array {
        cancellation.check(); checkReadLimit(maxBytes); let fd: number | undefined;
        try {
            const file=sourcePath(this.#root,relative); fd=fs.openSync(file,fs.constants.O_RDONLY | (fs.constants.O_NOFOLLOW ?? 0));
            const stat=fs.fstatSync(fd); if (!stat.isFile()) throw new ConfigError('SourceUnavailable','Source is not a regular file');
            if (!Number.isSafeInteger(stat.size) || stat.size>maxBytes) throw new ConfigError('LimitExceeded','Source exceeds byte limit');
            const bytes=new Uint8Array(stat.size); let offset=0;
            while(offset<bytes.length) { cancellation.check(); const read=fs.readSync(fd,bytes,offset,Math.min(1048576,bytes.length-offset),offset); if(!read) throw new ConfigError('ReadFailed','Source changed during read'); offset+=read; }
            const after=fs.fstatSync(fd),published=fs.statSync(sourcePath(this.#root,relative));
            if(after.size!==stat.size||after.mtimeMs!==stat.mtimeMs||after.dev!==published.dev||after.ino!==published.ino) throw new ConfigError('ReadFailed','Source changed during read');
            cancellation.check(); return bytes;
        } catch(error) { return readFailure(error); } finally { if(fd!==undefined) fs.closeSync(fd); }
    }
    async readAsync(relative: string, maxBytes: number, cancellation: Cancellation): Promise<Uint8Array> {
        cancellation.check(); checkReadLimit(maxBytes); let handle: any;
        try {
            // Metadata checks are bounded by the fixed relative path depth. Payload reads use asynchronous file operations.
            const file=sourcePath(this.#root,relative); handle=await fs.promises.open(file,fs.constants.O_RDONLY | (fs.constants.O_NOFOLLOW ?? 0));
            const stat=await handle.stat(); if (!stat.isFile()) throw new ConfigError('SourceUnavailable','Source is not a regular file');
            if (!Number.isSafeInteger(stat.size) || stat.size>maxBytes) throw new ConfigError('LimitExceeded','Source exceeds byte limit');
            const bytes=new Uint8Array(stat.size); let offset=0;
            while(offset<bytes.length) { cancellation.check(); const {bytesRead}=await handle.read(bytes,offset,Math.min(1048576,bytes.length-offset),offset); if(!bytesRead) throw new ConfigError('ReadFailed','Source changed during read'); offset+=bytesRead; }
            const after=await handle.stat(),published=await fs.promises.stat(sourcePath(this.#root,relative));
            if(after.size!==stat.size||after.mtimeMs!==stat.mtimeMs||after.dev!==published.dev||after.ino!==published.ino) throw new ConfigError('ReadFailed','Source changed during read'); cancellation.check(); return bytes;
        } catch(error) { return readFailure(error); } finally { if(handle) await handle.close(); }
    }
}
export interface LoadScheduler {
    /** Schedule exactly one invocation. A rejected submission must not invoke work later. */
    execute<T>(work: () => T): Promise<T>;
    yield(cancellation: Cancellation): Promise<void>;
}
export class NodeLoadScheduler implements LoadScheduler {
    execute<T>(work: () => T): Promise<T> { return new Promise((resolve,reject) => { require('node:timers').setImmediate(() => { try { resolve(work()); } catch(error) { reject(error); } }); }); }
    yield(cancellation: Cancellation): Promise<void> { return this.execute(() => cancellation.check()); }
}
/** @internal Shared by all tables. No counters or objects are created on a hot query. */
export class LoadBudget {
    #raw=0; #readBytes=0; #declared=0; #decoded=0; #estimated=0; #indexEstimated=0; #jobs=0; #waiters=0; #faults=0; #indexFaults=0; #merges=0;
    constructor(readonly limits: ResourceLimits) {}
    get pendingJobs(): number { return this.#jobs; }
    get pendingWaiters(): number { return this.#waiters; }
    get faultedIndexes(): number { return this.#indexFaults; }
    get faultedRows(): number { return this.#faults; }
    get decodedRows(): number { return this.#decoded; }
    get retainedRawBytes(): number { return this.#raw; }
    get readBytes(): number { return this.#readBytes; }
    get declaredRows(): number { return this.#declared; }
    get estimatedIndexBytes(): number { return this.#indexEstimated; }
    reserveIndex(bytes:number):void{if(!Number.isSafeInteger(bytes)||bytes<0||bytes>this.limits.maxEstimatedIndexBytes-this.#indexEstimated)throw new ConfigError("BudgetExceeded","Derived index budget exhausted","group");this.#indexEstimated+=bytes;}
    releaseIndex(bytes:number):void{this.#indexEstimated-=bytes;}
    get estimatedDecodedBytes(): number { return this.#estimated; }
    get health(): 'Healthy' | 'Degraded' { return this.#faults || this.#indexFaults ? 'Degraded' : 'Healthy'; }
    get mergedRequests(): number { return this.#merges; }
    raw(bytes: number): void { if(bytes>this.limits.maxSnapshotBytes-this.#readBytes) throw new ConfigError('BudgetExceeded','Snapshot byte budget exceeded'); this.#readBytes+=bytes; }
    retain(bytes: number): void { this.#raw+=bytes; }
    rows(count: number): void { if(count>this.limits.maxRows-this.#declared) throw new ConfigError('LimitExceeded','Snapshot row limit exceeded'); this.#declared+=count; }
    reserveTransient(bytes:number):void{if(bytes>this.limits.maxEstimatedDecodedBytes-this.#estimated)throw new ConfigError("BudgetExceeded","Transient decode budget exceeded");this.#estimated+=bytes;}
    releaseTransient(bytes:number):void{this.#estimated-=bytes;}
    reserve(bytes: number): void { if(this.#decoded>=this.limits.maxDecodedRows || bytes>this.limits.maxEstimatedDecodedBytes-this.#estimated) throw new ConfigError('BudgetExceeded','Decoded row budget exceeded'); ++this.#decoded; this.#estimated+=bytes; }
    release(bytes: number): void { --this.#decoded; this.#estimated-=bytes; }
    indexFault(): void { ++this.#indexFaults; }
    fault(): void { ++this.#faults; }
    merge(): void { ++this.#merges; }
    request(job: boolean): () => void {
        if((job?this.#jobs:this.#waiters)>=this.limits.maxPendingRequests) throw new ConfigError('BudgetExceeded','Pending request budget exceeded');
        if(job) ++this.#jobs; else ++this.#waiters; let active=true;
        return () => { if(active) { active=false; if(job) --this.#jobs; else --this.#waiters; } };
    }
}
export interface TableDescriptor { readonly name: string; readonly schemaHash: string; readonly dependencies: readonly string[]; readonly component: number; }
export interface SelectionPlan { readonly requested: readonly string[]; readonly tables: readonly string[]; readonly catalogFingerprint: string; }
export class Catalog {
    readonly target: string;
    readonly fingerprint: string;
    readonly tables: readonly TableDescriptor[];
    #names = new Map<string, TableDescriptor>();
    #components = new Map<number, readonly TableDescriptor[]>();
    constructor(source: string) {
        const definition = parseJSON(source);
        if(definition.format!==1&&definition.format!==2)throw new ConfigError('InvalidPlan','Unknown catalog format');
        if(definition.format===2){const supported=new Set(["binary-v4", "json-v1", "server-default", "selected-closure", "lazy-strict", "lazy-on-access", "fixed-source-bytes", "snapshot-owned-handles", "explicit-reference-load", "row-cache", "group-cache", "typed-warmup", "bounded-source", "structured-errors", "manager-lifecycle"]);if(definition.runtimeContract!=='server-v1')throw new ConfigError('UnsupportedCapability','Unsupported runtime contract');for(const value of array(definition.requiredCapabilities))if(!supported.has(text(value)))throw new ConfigError('UnsupportedCapability','Unsupported runtime capability: '+value);}
        this.target=typeof definition.target==='string'?definition.target:'';
        this.fingerprint = text(definition.fingerprint);
        this.tables = Object.freeze(array(definition.tables).map(t => Object.freeze({ name: text(t.name), schemaHash: text(t.schemaHash),
            dependencies: Object.freeze(array(t.dependencies).map(text)), component: integer(t.component, 0, 2147483647) })));
        const components = new Map<number, TableDescriptor[]>();
        for (const table of this.tables) {
            if (this.#names.has(table.name)) throw new ConfigError('InvalidPlan', 'Duplicate catalog table');
            this.#names.set(table.name, table);
            let group = components.get(table.component); if (!group) { group = []; components.set(table.component, group); } group.push(table);
        }
        for (const table of this.tables) for (const dependency of table.dependencies)
            if (!this.#names.has(dependency)) throw new ConfigError('InvalidPlan', 'Unknown dependency: ' + dependency);
        for (const [id, group] of components) this.#components.set(id, Object.freeze(group));
        Object.freeze(this);
    }
    select(requested?: readonly string[]): SelectionPlan {
        const input = requested === undefined ? this.tables.map(t => t.name) : [...requested];
        const selected = new Set<string>(), components = new Set<number>(), pending: TableDescriptor[] = [];
        const include = (name: string) => {
            const table = this.#names.get(name); if (!table) throw new ConfigError('InvalidPlan', 'Unknown selected table: ' + name);
            if (components.has(table.component)) return; components.add(table.component);
            for (const member of this.#components.get(table.component)!) { selected.add(member.name); pending.push(member); }
        };
        for (const name of input) include(name);
        for (let i = 0; i < pending.length; i++) for (const dependency of pending[i].dependencies) include(dependency);
        return Object.freeze({ requested: Object.freeze(input.sort()), tables: Object.freeze(this.tables.filter(t => selected.has(t.name)).map(t => t.name)), catalogFingerprint: this.fingerprint });
    }
}
export interface PreparationOptions { readonly timeoutMillis: number; readonly candidateLifetimeMillis: number; }
export type ManagerPhase = 'Empty' | 'Ready' | 'Closed';
export interface ManagerStatus { readonly phase: ManagerPhase; readonly generation: bigint; readonly preparingOperation: bigint; }
export interface CommitResult<S> { readonly snapshot: S; readonly generation: bigint; readonly operationId: bigint; readonly committed: true; }
export interface SnapshotIdentity { readonly sourceNamespace: string; readonly versionId: string; readonly manifestDigest: string; }
export interface ManagerEvent { readonly kind: 'Committed' | 'PreparationFailed' | 'Cleared' | 'Closed'; readonly generation: bigint; readonly operationId: bigint; readonly versionId: string; readonly code?: ErrorCode; }
export interface ManagerPolicy<S> {
    readonly identity?: (snapshot: S) => SnapshotIdentity;
    readonly validators?: readonly ((snapshot: S, cancellation: Cancellation) => void)[];
    readonly eventCapacity?: number;
    readonly maxVersionIdentities?: number;
    readonly maxPendingPreparations?: number;
    readonly maxEstimatedLiveBytes?: number;
    readonly liveEstimator?: (snapshot: S) => number;
    readonly maxTrackedSnapshots?: number;
}
interface PreparedState<S> { owner: object; snapshot?: S; generation: bigint; operation: bigint; expires: number; committed: boolean; ticket: {discarded:boolean}; identity?: SnapshotIdentity; }
const preparedStates = new WeakMap<object, PreparedState<unknown>>();
const retainedOwners = new WeakMap<object, object>();
// Ephemeron ownership: rows keep their own snapshot alive; the registry does not keep an unreachable graph alive.
export function retainOwner(value: object, owner: object): void { retainedOwners.set(value, owner); }
let createPrepared: <S>(state: PreparedState<S>) => PreparedSnapshot<S>;
export class PreparedSnapshot<S> {
    private constructor() {}
    get snapshot(): S { const value = preparedStates.get(this); if (!value || value.snapshot === undefined) throw new ConfigError('StaleCandidate', 'Candidate discarded'); return value.snapshot as S; }
    discard(): void {
        const state = preparedStates.get(this); if (!state) return;
        if(!state.committed)state.ticket.discarded=true;state.snapshot=undefined;
    }
    static { createPrepared = <T>(state: PreparedState<T>): PreparedSnapshot<T> => { const candidate = new PreparedSnapshot<T>(); preparedStates.set(candidate,state); return Object.freeze(candidate); }; }
}
/** Monotonic elapsed time including waits, excluding callbacks. OperationId zero means no sample. */
export interface StageTiming{readonly stage:"Prepare"|"Commit";readonly operationId:bigint;readonly elapsedMilliseconds:number;readonly succeeded:boolean;}
export class ServerManager<S> {
    #prepareTiming:StageTiming=Object.freeze({stage:"Prepare",operationId:0n,elapsedMilliseconds:0,succeeded:false});
    #commitTiming:StageTiming=Object.freeze({stage:"Commit",operationId:0n,elapsedMilliseconds:0,succeeded:false});
    stageTimings():readonly StageTiming[]{return Object.freeze([this.#prepareTiming,this.#commitTiming]);}
    #identity = Object.freeze({}); #current: S | undefined; #closed = false;
    #generation = 0n; #serial = 0n; #pending = 0n; #building = false; #expires = 0;
    #policy: Readonly<ManagerPolicy<S>>;
    #ticket?:{discarded:boolean};#operationToken?:Cancellation;#activePreparations=0;
    #identities = new Map<string,Map<string,string>>();
    #identityOrder: { namespace: string; version: string }[]=[];
    #live: WeakRef<object>[]=[];
    #listeners = new Map<object,(event: ManagerEvent) => void>();
    #events: ManagerEvent[]=[]; #draining=false; #dropped=0; #listenerFailures=0;
    constructor(policy: ManagerPolicy<S> = {}) {
        for(const name of Object.keys(policy)) if(!['identity','validators','eventCapacity','maxVersionIdentities','maxPendingPreparations','maxEstimatedLiveBytes','liveEstimator','maxTrackedSnapshots'].includes(name)) throw new ConfigError('InvalidPlan','Unknown manager policy: '+name);
        const eventCapacity=policy.eventCapacity ?? 64,maxVersionIdentities=policy.maxVersionIdentities ?? 32,maxPendingPreparations=policy.maxPendingPreparations ?? 64;
        if(!Number.isSafeInteger(eventCapacity)||eventCapacity<=0||!Number.isSafeInteger(maxVersionIdentities)||maxVersionIdentities<=0||!Number.isSafeInteger(maxPendingPreparations)||maxPendingPreparations<=0) throw new ConfigError('InvalidPlan','Invalid manager capacity');
        if(policy.identity!==undefined && typeof policy.identity!=='function' || policy.validators?.some(value=>typeof value!=='function')) throw new ConfigError('InvalidPlan','Invalid manager callback');
        const maxTrackedSnapshots=policy.maxTrackedSnapshots ?? 1024;
        if(!Number.isSafeInteger(maxTrackedSnapshots)||maxTrackedSnapshots<1) this.#fail('InvalidPlan');
        if(policy.maxEstimatedLiveBytes!==undefined&&(!Number.isSafeInteger(policy.maxEstimatedLiveBytes)||policy.maxEstimatedLiveBytes<1||typeof policy.liveEstimator!=='function')) this.#fail('InvalidPlan');
        if(policy.liveEstimator!==undefined&&typeof policy.liveEstimator!=='function') this.#fail('InvalidPlan');
        if(policy.maxEstimatedLiveBytes!==undefined&&typeof WeakRef!=='function') this.#fail('UnsupportedCapability');
        this.#policy=Object.freeze({...policy,eventCapacity,maxVersionIdentities,maxPendingPreparations,maxTrackedSnapshots,validators:Object.freeze([...(policy.validators ?? [])])});
    }
    #liveUsage(candidate:S|undefined,enforce:boolean):number|undefined{
        if(this.#policy.maxEstimatedLiveBytes===undefined)return undefined;
        const owners=this.#live.map(reference=>reference.deref()).filter((value):value is object=>value!==undefined);this.#live=owners.map(owner=>new WeakRef(owner));
        if(candidate!==undefined){if((typeof candidate!=='object'&&typeof candidate!=='function')||candidate===null)this.#fail('UnsupportedCapability');if(!owners.includes(candidate as object))owners.push(candidate as object);}
        let total=0;for(const owner of owners){const estimate=this.#policy.liveEstimator!(owner as S);if(!Number.isSafeInteger(estimate)||estimate<0)this.#fail('InvalidValue');total=Math.min(Number.MAX_SAFE_INTEGER,total+estimate);if(enforce&&total>this.#policy.maxEstimatedLiveBytes)this.#fail('BudgetExceeded');}return total;
    }
    #trackLive(candidate:S):void{if(this.#policy.maxEstimatedLiveBytes===undefined)return;this.#live=this.#live.filter(reference=>reference.deref()!==undefined);if(this.#live.some(reference=>reference.deref()===candidate))return;if(this.#live.length>=this.#policy.maxTrackedSnapshots!)this.#fail('BudgetExceeded');this.#live.push(new WeakRef(candidate as object));}
    /** Conservative until GC; undefined when no admission budget is configured. */
    estimatedLiveBytes():number|undefined{return this.#liveUsage(undefined,false);}
    get droppedEventCount(): number { return this.#dropped; }
    get listenerFailureCount(): number { return this.#listenerFailures; }
    subscribe(listener: (event: ManagerEvent) => void): { dispose(): void } {
        const id={}; this.#listeners.set(id,listener); return Object.freeze({dispose:()=>{this.#listeners.delete(id);}});
    }
    #event(kind: ManagerEvent['kind'], operationId=0n, versionId='', code?: ErrorCode, generation=this.#generation): void {
        if(this.#events.length===this.#policy.eventCapacity) { this.#events.shift(); ++this.#dropped; }
        this.#events.push(Object.freeze({kind,generation,operationId,versionId,...(code ? {code} : {})}));
        if(this.#draining) return;
        this.#draining=true;
        try { while(this.#events.length) { const event=this.#events.shift()!; for(const listener of [...this.#listeners.values()]) { try { listener(event); } catch { ++this.#listenerFailures; } } } }
        finally { this.#draining=false; }
    }
    tryAcquireSnapshot(): S | undefined { if (this.#closed) this.#fail('ManagerClosed'); return this.#current; }
    acquireSnapshot(): S { const value = this.tryAcquireSnapshot(); if (value === undefined) this.#fail('NotInitialized'); return value; }
    status(): ManagerStatus { this.#prune();return Object.freeze({ phase: this.#closed ? 'Closed' : this.#current === undefined ? 'Empty' : 'Ready', generation: this.#generation, preparingOperation: this.#pending }); }
    diagnostics(){return Object.freeze({activePreparations:this.#activePreparations,pendingEvents:this.#events.length,droppedEvents:this.#dropped,callbackFailures:this.#listenerFailures,retainedIdentities:this.#identityOrder.length,subscribers:this.#listeners.size});}
    prepare(factory:()=>S,cancellation:Cancellation,options:PreparationOptions={timeoutMillis:300000,candidateLifetimeMillis:300000}):PreparedSnapshot<S>{return this.prepareWithCancellation(()=>factory(),cancellation,options);}
    prepareWithCancellation(factory:(operation:Cancellation)=>S,cancellation:Cancellation,options:PreparationOptions={timeoutMillis:300000,candidateLifetimeMillis:300000}):PreparedSnapshot<S>{
        const started=performance.now();const operation=this.#begin(cancellation,options);let failed=false;let failure:unknown;try{this.#liveUsage(undefined,true);if(this.#closed)this.#fail('ManagerClosed');if(operation.generation!==this.#generation||operation.id!==this.#pending)this.#fail('StaleCandidate');return this.#finish(factory(operation.token),operation);}catch(error){this.#end(operation.id);failed=true;failure=error;throw error;}finally{--this.#activePreparations;if(operation.id>=this.#prepareTiming.operationId)this.#prepareTiming=Object.freeze({stage:"Prepare",operationId:operation.id,elapsedMilliseconds:performance.now()-started,succeeded:!failed});if(failed)this.#event('PreparationFailed',operation.id,'',failure instanceof ConfigError?failure.code:'InternalError',operation.generation);}
    }
    prepareAsync(factory:()=>Promise<S>,cancellation:Cancellation,options:PreparationOptions={timeoutMillis:300000,candidateLifetimeMillis:300000}):Promise<PreparedSnapshot<S>>{return this.prepareAsyncWithCancellation(()=>factory(),cancellation,options);}
    async prepareAsyncWithCancellation(factory:(operation:Cancellation)=>Promise<S>,cancellation:Cancellation,options:PreparationOptions={timeoutMillis:300000,candidateLifetimeMillis:300000}):Promise<PreparedSnapshot<S>>{
        const started=performance.now();const operation=this.#begin(cancellation,options);let failed=false;let failure:unknown;try{this.#liveUsage(undefined,true);if(this.#closed)this.#fail('ManagerClosed');if(operation.generation!==this.#generation||operation.id!==this.#pending)this.#fail('StaleCandidate');return this.#finish(await factory(operation.token),operation);}catch(error){this.#end(operation.id);failed=true;failure=error;throw error;}finally{--this.#activePreparations;if(operation.id>=this.#prepareTiming.operationId)this.#prepareTiming=Object.freeze({stage:"Prepare",operationId:operation.id,elapsedMilliseconds:performance.now()-started,succeeded:!failed});if(failed)this.#event('PreparationFailed',operation.id,'',failure instanceof ConfigError?failure.code:'InternalError',operation.generation);}
    }
    commit(prepared: PreparedSnapshot<S>, cancellation: Cancellation): CommitResult<S> {
        const started=performance.now();
        this.#prune();const state = preparedStates.get(prepared) as PreparedState<S> | undefined;
        if (!state || state.owner !== this.#identity) this.#fail('ForeignCandidate');
        if (this.#closed) this.#fail('ManagerClosed');
        if (state.committed) this.#fail('AlreadyCommitted');
        if (state.snapshot === undefined || state.generation !== this.#generation || state.operation !== this.#pending) this.#fail('StaleCandidate');
        cancellation.check(); if (performance.now() >= state.expires) this.#fail('DeadlineExceeded');
        this.#liveUsage(state.snapshot,true);if(this.#closed)this.#fail('ManagerClosed');if(state.snapshot===undefined||state.generation!==this.#generation||state.operation!==this.#pending)this.#fail('StaleCandidate');cancellation.check();if(performance.now()>=state.expires)this.#fail('DeadlineExceeded');
        const identity=state.identity;
        if(identity) {
            if(typeof identity.sourceNamespace!=='string'||typeof identity.versionId!=='string'||!identity.versionId||typeof identity.manifestDigest!=='string'||!/^[a-fA-F0-9]{64}$/.test(identity.manifestDigest)) this.#fail('InvalidValue');
            let versions=this.#identities.get(identity.sourceNamespace);
            const known=versions?.get(identity.versionId),digest=identity.manifestDigest.toLowerCase();
            if(known!==undefined && known!==digest) this.#fail('VersionIdentityConflict');
            if(known===undefined) {
                if(this.#identityOrder.length===this.#policy.maxVersionIdentities) { const old=this.#identityOrder.shift()!; const history=this.#identities.get(old.namespace)!; history.delete(old.version); if(!history.size) this.#identities.delete(old.namespace); }
                versions=this.#identities.get(identity.sourceNamespace); if(!versions) { versions=new Map(); this.#identities.set(identity.sourceNamespace,versions); }
                versions.set(identity.versionId,digest); this.#identityOrder.push({namespace:identity.sourceNamespace,version:identity.versionId});
            }
        }
        this.#current = state.snapshot; ++this.#generation; this.#pending = 0n; state.committed = true; this.#ticket=undefined;this.#operationToken=undefined;
        const result=Object.freeze({ snapshot: state.snapshot, generation: this.#generation, operationId: state.operation, committed: true as const });
        this.#commitTiming=Object.freeze({stage:"Commit",operationId:state.operation,elapsedMilliseconds:performance.now()-started,succeeded:true});
        this.#event('Committed',state.operation,identity?.versionId); return result;
    }
    reload(factory: () => S, cancellation: Cancellation, options?: PreparationOptions): CommitResult<S> {
        const prepared = this.prepare(factory, cancellation, options); try { return this.commit(prepared, cancellation); } finally { prepared.discard(); }
    }
    async reloadAsync(factory: () => Promise<S>, cancellation: Cancellation, options?: PreparationOptions): Promise<CommitResult<S>> {
        const prepared = await this.prepareAsync(factory, cancellation, options); try { return this.commit(prepared, cancellation); } finally { prepared.discard(); }
    }
    clear(): void { this.#detach(false); }
    close(): void { this.#detach(true); }
    #detach(close: boolean): void {
        if (this.#closed) { if (close) return; this.#fail('ManagerClosed'); }
        this.#operationToken?.cancel();this.#operationToken=undefined;this.#ticket=undefined;
        ++this.#generation; if (close) this.#closed = true; this.#current = undefined; this.#pending = 0n;
        this.#event(close?'Closed':'Cleared');
    }
    #begin(cancellation: Cancellation, options: PreparationOptions) {
        this.#prune();cancellation.check(); if (this.#closed) this.#fail('ManagerClosed');
        const timeout = options.timeoutMillis, lifetime = options.candidateLifetimeMillis;
        if (!Number.isFinite(timeout) || !Number.isFinite(lifetime) || timeout <= 0 || lifetime <= 0) this.#fail('InvalidPlan');
        if (this.#pending !== 0n && (this.#building || performance.now() < this.#expires)) this.#fail('ReloadBusy');
        if(this.#activePreparations>=this.#policy.maxPendingPreparations!)this.#fail('LimitExceeded');
        const id=++this.#serial,generation=this.#generation,deadline=performance.now()+timeout;if(!Number.isFinite(deadline)||!Number.isFinite(deadline+lifetime))this.#fail('InvalidPlan');
        const token=cancellation.child(deadline),ticket={discarded:false};this.#ticket=ticket;this.#operationToken=token;++this.#activePreparations;
        this.#pending = id; this.#expires = deadline; this.#building = true;
        return { id, generation, deadline, lifetime,token,ticket };
    }
    #finish(snapshot: S, operation: { id: bigint; generation: bigint; deadline: number; lifetime: number;token:Cancellation;ticket:{discarded:boolean} }): PreparedSnapshot<S> {
        const cancellation=operation.token;if (this.#closed) this.#fail('ManagerClosed');
        if (operation.generation !== this.#generation || this.#pending !== operation.id) this.#fail('StaleCandidate');
        cancellation.check();if (performance.now() >= operation.deadline) this.#fail('DeadlineExceeded');
        if (snapshot === undefined || snapshot === null) this.#fail('InvalidValue');
        for(const validate of this.#policy.validators!) { cancellation.check(); const result=validate(snapshot,cancellation); if(result!==undefined) this.#fail('UnsupportedCapability'); }
        cancellation.check(); if(this.#closed) this.#fail('ManagerClosed');
        if(operation.generation!==this.#generation||this.#pending!==operation.id) this.#fail('StaleCandidate');
        if(performance.now()>=operation.deadline) this.#fail('DeadlineExceeded');
        const proposed=this.#policy.identity?.(snapshot);let identity:SnapshotIdentity|undefined;
        if(proposed){if(typeof proposed.sourceNamespace!=='string'||typeof proposed.versionId!=='string'||!proposed.versionId||typeof proposed.manifestDigest!=='string'||!/^[a-fA-F0-9]{64}$/.test(proposed.manifestDigest))this.#fail('InvalidValue');identity=Object.freeze({sourceNamespace:proposed.sourceNamespace,versionId:proposed.versionId,manifestDigest:proposed.manifestDigest.toLowerCase()});const known=this.#identities.get(identity.sourceNamespace)?.get(identity.versionId);if(known!==undefined&&known!==identity.manifestDigest)this.#fail('VersionIdentityConflict');}
        this.#liveUsage(snapshot,true);
        if(this.#closed)this.#fail('ManagerClosed');if(operation.generation!==this.#generation||this.#pending!==operation.id)this.#fail('StaleCandidate');cancellation.check();this.#trackLive(snapshot);
        this.#building = false; this.#expires = performance.now() + operation.lifetime;this.#operationToken=undefined;
        return createPrepared({ owner: this.#identity, snapshot, generation: operation.generation,
            operation: operation.id, expires: this.#expires, committed: false,ticket:operation.ticket,identity });
    }
    #prune():void{if(this.#ticket?.discarded){this.#operationToken?.cancel();this.#pending=0n;this.#ticket=undefined;this.#operationToken=undefined;}}
    #end(id:bigint):void{if(this.#pending===id){this.#pending=0n;this.#ticket=undefined;this.#operationToken=undefined;}}
    #fail(code: ErrorCode): never { throw new ConfigError(code, code); }
}
export function object(value: any): Record<string, any> {
    if (value === null || typeof value !== 'object' || Array.isArray(value)) invalid('需要对象');
    return value;
}
export function array(value: any): any[] { if (!Array.isArray(value)) invalid('需要数组'); return value; }
export function text(value: any): string { if (typeof value !== 'string') invalid('需要字符串'); return value; }
function integer(value: any, min: number, max: number): number {
    if (typeof value !== 'number' || !Number.isInteger(value) || value < min || value > max) invalid('整数超出范围');
    return value;
}
function bigInteger(value: any, unsigned: boolean): bigint {
    if (typeof value !== 'string' || !/^-?(0|[1-9][0-9]*)$/.test(value)) invalid('64 位整数需要十进制字符串');
    const n = BigInt(value);
    if (n < (unsigned ? 0n : -9223372036854775808n) || n > (unsigned ? 18446744073709551615n : 9223372036854775807n)) invalid('64 位整数越界');
    return n;
}
// 规范化只发生在构造阶段；查询使用已缓存的字符串键，不创建临时 Decimal。
export class Decimal {
    readonly key: string;
    readonly value: string;
    constructor(value: string) {
        const m = /^(-?)([0-9]+)(?:\.([0-9]+))?$/.exec(value);
        if (!m || (m[3]?.length ?? 0) > 28 || BigInt(m[2] + (m[3] ?? '')) > 79228162514264337593543950335n) invalid('decimal 精度或范围无效');
        const whole = m[2].replace(/^0+(?=.)/, '');
        const fraction = (m[3] ?? '').replace(/0+$/, '');
        const magnitude = whole + (fraction ? '.' + fraction : '');
        this.key = (m[1] && magnitude !== '0' ? '-' : '') + magnitude;
        this.value = value;
        Object.freeze(this);
    }
    toString(): string { return this.value; }
}
export function key(value: any): any { return value instanceof Decimal ? value.key : value; }
export class Dictionary<K, V> {
    readonly #values: Map<any, V>;
    readonly #entries: ReadonlyArray<readonly [K, V]>;
    constructor(entries: Array<[K, V]>) {
        this.#values = new Map();
        for (const pair of entries) {
            const k = key(pair[0]);
            if (this.#values.has(k)) invalid('字典键重复');
            this.#values.set(k, pair[1]); Object.freeze(pair);
        }
        this.#entries = Object.freeze(entries); Object.freeze(this);
    }
    get(k: K): V | undefined { return this.#values.get(key(k)); }
    has(k: K): boolean { return this.#values.has(key(k)); }
    get size(): number { return this.#values.size; }
    get entries(): ReadonlyArray<readonly [K, V]> { return this.#entries; }
}
export interface Matrix<T> { readonly dimensions: readonly number[]; readonly values: readonly T[]; }
export const empty: readonly never[] = Object.freeze([]);

// 原生 JSON.parse 不拒绝重复成员；这里执行严格词法解析，且不将 64 位整数转成浮点数。
interface JsonAllocationBounds {rows:number;elements:number;members:number;stringBytes:number;depth:number;rootArray?:string;}
function jsonBoundsFor(schema:Schema,limits?:ResourceLimits):JsonAllocationBounds|undefined{
    if(!limits)return undefined;const bounds={rows:limits.maxRows,elements:Math.max(64,limits.maxContainerElements),members:Math.max(4,schema.fields.length+1),stringBytes:Math.max(256,limits.maxStringBytes),depth:Math.min(72,limits.maxDepth*2+8)};
    const name=(value:string)=>{bounds.stringBytes=Math.max(bounds.stringBytes,encoder.encode(value).length);};name(schema.name);for(const field of schema.fields)name(field.name);for(const type of schema.types){name(type.id);bounds.members=Math.max(bounds.members,type.fields.length+1);for(const field of type.fields)name(field.name);}return bounds;
}
function jsonAllocationFailure():never{throw new ConfigError('LimitExceeded','JSON allocation budget exceeded','parse');}
export function parseJSON(source: string): any {return parseJSONWithBounds(source);}
function parseJSONWithBounds(source:string,bounds?:JsonAllocationBounds):any{
    let pos = 0, nodes = 0;
    function ws(): void { while (pos < source.length && /[\x20\t\r\n]/.test(source[pos])) pos++; }
    function string(): string {
        const start = pos++;
        while (pos < source.length) {
            if(bounds&&pos-start>bounds.stringBytes*6+2)jsonAllocationFailure();
            const c = source.charCodeAt(pos++);
            if (c === 34) {
                const result = JSON.parse(source.slice(start, pos));
                let bytes=0;
                for (let i = 0; i < result.length; i++) {
                    const u = result.charCodeAt(i);
                    if (u >= 0xd800 && u <= 0xdbff) { const low = result.charCodeAt(++i); if (!(low >= 0xdc00 && low <= 0xdfff)) invalid('孤立代理项');bytes+=4; }
                    else if (u >= 0xdc00 && u <= 0xdfff) invalid('孤立代理项');
                    else bytes+=u<0x80?1:u<0x800?2:3;
                }
                if(bounds&&bytes>bounds.stringBytes)jsonAllocationFailure();
                if (bytes > 16777216) invalid('字符串过长');
                return result;
            }
            if (c < 32) invalid('字符串含控制字符');
            if (c === 92) pos++;
        }
        return invalid('字符串未结束');
    }
    function value(depth: number,rows=false): any {
        if(bounds&&depth>bounds.depth)jsonAllocationFailure();
        ws(); if (++nodes > 32000000 || depth > 72) invalid('JSON 超限');
        const c = source[pos];
        if (c === '"') return string();
        if (c === '{') {
            pos++; const result = Object.create(null);let members=0; ws();
            if (source[pos] === '}') { pos++; return result; }
            while (true) {
                if(bounds&&members>=bounds.members)jsonAllocationFailure();++members;
                ws(); if (source[pos] !== '"') invalid('成员名无效'); const name = string();
                if (Object.hasOwn(result, name)) invalid('JSON 成员重复');
                ws(); if (source[pos++] !== ':') invalid('缺少冒号'); result[name] = value(depth + 1,depth===0&&name===(bounds?.rootArray??'rows')); ws();
                const sep = source[pos++]; if (sep === '}') return result; if (sep !== ',') invalid('对象分隔符无效');
            }
        }
        if (c === '[') {
            pos++; const result: any[] = []; ws(); if (source[pos] === ']') { pos++; return result; }
            while (true) { if(bounds&&result.length>=(rows?bounds.rows:bounds.elements))jsonAllocationFailure();result.push(value(depth + 1)); ws(); const sep = source[pos++]; if (sep === ']') return result; if (sep !== ',') invalid('数组分隔符无效'); }
        }
        for (const [token, result] of [['true', true], ['false', false], ['null', null]] as const)
            if (source.startsWith(token, pos)) { pos += token.length; return result; }
        const start = pos;
        while (pos < source.length && !/[\x20\t\r\n,\]}]/.test(source[pos])) { if (pos - start >= 1024) invalid('数字过长'); pos++; }
        const token = source.slice(start, pos);
        if (!/^-?(0|[1-9][0-9]*)(\.[0-9]+)?([eE][+-]?[0-9]+)?$/.test(token)) invalid('JSON 数字无效');
        const n = Number(token); if (!Number.isFinite(n)) invalid('非有限数字'); return n;
    }
    const result = value(0); ws(); if (pos !== source.length) invalid('JSON 尾部数据'); return result;
}
interface Field { name: string; type: string; }
interface Type { id: string; kind: string; primitive: string; nullableObject: boolean; rank: number; element: string; key: string; fields: Field[]; variants: string[]; enumValues: string[]; referenceTable?: string; }
interface Schema { name: string; hash: string; hasId: boolean; fields: Field[]; keys: string[]; types: Type[]; tags: string[]; groups: { name: string; fields: string[] }[]; }
class Cursor {
    pos = 0;
    readonly view: DataView;
    constructor(readonly data: Uint8Array, readonly limits?: ResourceLimits) { this.view = new DataView(data.buffer, data.byteOffset, data.byteLength); }
    take(n: number): number { const p = this.pos; if (n < 0 || n > this.data.length - p) invalid('二进制数据截断'); this.pos += n; return p; }
    i32(): number { return this.view.getInt32(this.take(4), true); }
    count(max = 1000000): number { return integer(this.i32(), 0, max); }
    bool(): boolean { const n = this.view.getUint8(this.take(1)); if (n > 1) invalid('布尔值编码无效'); return n !== 0; }
    string(): string { const n = this.i32(); if (n === -1) return ''; if (n < 0 || n > 16777216) invalid('字符串长度无效'); if(this.limits && n>this.limits.maxStringBytes) throw new ConfigError('LimitExceeded','String byte limit exceeded'); return decodeUTF8(this.data.subarray(this.take(n), this.pos)); }
    scalar(kind: string): any {
        const v = this.view;
        switch (kind) {
            case 'string': return this.string(); case 'bool': return this.bool();
            case 'int8': return v.getInt8(this.take(1)); case 'uint8': return v.getUint8(this.take(1));
            case 'int16': return v.getInt16(this.take(2), true); case 'uint16': return v.getUint16(this.take(2), true);
            case 'int32': return this.i32(); case 'uint32': return v.getUint32(this.take(4), true);
            case 'int64': return v.getBigInt64(this.take(8), true).toString(); case 'uint64': return v.getBigUint64(this.take(8), true).toString();
            case 'float32': return v.getFloat32(this.take(4), true); case 'float64': return v.getFloat64(this.take(8), true);
            case 'decimal': {
                const lo = BigInt(v.getUint32(this.take(4), true)), mid = BigInt(v.getUint32(this.take(4), true)), hi = BigInt(v.getUint32(this.take(4), true));
                const flags = v.getUint32(this.take(4), true), scale = (flags >>> 16) & 255;
                if ((flags & 0x7f00ffff) !== 0 || scale > 28) invalid('decimal 编码无效');
                let s = (lo | (mid << 32n) | (hi << 64n)).toString().padStart(scale + 1, '0');
                if (scale) s = s.slice(0, -scale) + '.' + s.slice(-scale);
                return (flags >>> 31 ? '-' : '') + s;
            }
            default: return invalid('未知标量');
        }
    }
}
class Wire {
    readonly schema: Schema;
    readonly types = new Map<string, Type>();
    tags: string[] = [];
    remaining = 8000000;
    constructor(schema: string, readonly limits?: ResourceLimits) { this.schema = parseJSON(schema); for (const t of this.schema.types) this.types.set(t.id, t); }
    claim(n: number): void { if(this.limits && n>this.limits.maxContainerElements) throw new ConfigError('LimitExceeded','Container element limit exceeded'); integer(n, 0, Math.min(1000000, this.remaining)); this.remaining -= n; }
    readFields(fields: Field[], r: Cursor, depth: number): any {
        const result = Object.create(null); for (const f of fields) {try{result[f.name]=this.readValue(f.type,r,depth+1);}catch(error){throw this.limits?fieldError(error,f.name):error;}} return result;
    }
    private nestedRead(id:string,r:Cursor,depth:number,path:string):any{try{return this.readValue(id,r,depth);}catch(error){throw this.limits?fieldError(error,path):error;}}
    private nestedConvert(id:string,v:any,depth:number,path:string):any{try{return this.convert(id,v,depth);}catch(error){throw this.limits?fieldError(error,path):error;}}
    readValue(id: string, r: Cursor, depth: number): any {
        if (depth > 64) invalid('嵌套过深'); if(this.limits && depth>this.limits.maxDepth) throw new ConfigError('LimitExceeded','Nesting limit exceeded'); const t = this.types.get(id)!;
        if (['primitive', 'enum', 'tablereference'].includes(t.kind)) return r.scalar(t.primitive);
        if (t.kind === 'rawjson') return r.string();
        if (t.kind === 'nullable') return r.bool() ? this.readValue(t.element, r, depth + 1) : null;
        if (t.kind === 'array') {
            const dimensions: number[] = []; let n = 1;
            for (let i = 0; i < t.rank; i++) { const d = r.count(); dimensions.push(d); n *= d; if (n > 1000000) invalid('矩阵过大'); }
            this.claim(n); const values: any[] = []; for(let i=0;i<n;i++)try{values.push(this.readValue(t.element,r,depth+1));}catch(error){throw this.limits?fieldError(error,`[${i}]`):error;}
            return t.rank === 1 ? values : { dimensions, values };
        }
        if (t.kind === 'dictionary') { const n = r.count(); this.claim(n); const values: any[] = []; for (let i = 0; i < n; i++) values.push([this.nestedRead(t.key,r,depth+1,`[${i}].key`),this.nestedRead(t.element,r,depth+1,`[${i}].value`)]); return values; }
        if (t.variants.length) {
            const tag = r.i32(); if (tag === -1) return null;
            if (tag < 0 || tag >= this.tags.length || !t.variants.includes(this.tags[tag])) invalid('多态标签无效');
            const v = this.readFields(this.types.get(this.tags[tag])!.fields, r, depth); v.$type = this.tags[tag]; return v;
        }
        if (t.nullableObject && !r.bool()) return null;
        return this.readFields(t.fields, r, depth);
    }
    scalar(kind: string, v: any): any {
        if (kind === 'string') { const value=text(v); if(this.limits && encoder.encode(value).length>this.limits.maxStringBytes) throw new ConfigError('LimitExceeded','String byte limit exceeded'); return value; }
        if (kind === 'bool') { if (typeof v !== 'boolean') invalid('需要布尔值'); return v; }
        if (kind === 'int64' || kind === 'uint64') return bigInteger(v, kind === 'uint64');
        if (kind === 'decimal') return new Decimal(text(v));
        if (kind === 'float32' || kind === 'float64') { if (typeof v !== 'number' || !Number.isFinite(v)) invalid('浮点数无效'); const n = kind === 'float32' ? Math.fround(v) : v; if (!Number.isFinite(n)) invalid('浮点数越界'); return n; }
        const unsigned = kind.startsWith('uint'), bits = Number(kind.slice(unsigned ? 4 : 3));
        return integer(v, unsigned ? 0 : -(2 ** (bits - 1)), unsigned ? 2 ** bits - 1 : 2 ** (bits - 1) - 1);
    }
    fields(fields: Field[], value: any, depth: number, tagged = false): any {
        const input = object(value); if (Object.keys(input).length !== fields.length + (tagged ? 1 : 0)) invalid('未知或缺失成员');
        const result = Object.create(null); for (const f of fields) { if (!Object.hasOwn(input, f.name)) invalid('缺失成员'); try{result[f.name]=this.convert(f.type,input[f.name],depth+1);}catch(error){throw this.limits?fieldError(error,f.name):error;} }
        if (tagged) result.$type = input.$type; return result;
    }
    convert(id: string, value: any, depth: number): any {
        if (depth > 64) invalid('嵌套过深'); if(this.limits && depth>this.limits.maxDepth) throw new ConfigError('LimitExceeded','Nesting limit exceeded'); const t = this.types.get(id)!;
        if (['primitive', 'enum', 'tablereference'].includes(t.kind)) {
            const v = this.scalar(t.primitive, value); if (t.kind === 'enum' && !t.enumValues.includes(String(v))) invalid('未知枚举值'); return v;
        }
        if (t.kind === 'rawjson') { parseJSON(text(value)); return value; }
        if (t.kind === 'nullable') return value === null ? null : this.convert(t.element, value, depth + 1);
        if (t.kind === 'array') {
            let values: any[], dimensions: number[] = [];
            if (t.rank === 1) values = array(value);
            else {
                object(value); if (Object.keys(value).length !== 2) invalid('矩阵字段无效'); dimensions = array(value.dimensions); values = array(value.values);
                if (dimensions.length !== t.rank) invalid('矩阵维数错误'); let n = 1;
                for (const d of dimensions) { n *= integer(d, 0, 1000000); if (n > 1000000) invalid('矩阵过大'); }
                if (n !== values.length) invalid('矩阵长度错误');
            }
            this.claim(values.length); const result = Object.freeze(values.map((v,i)=>{try{return this.convert(t.element,v,depth+1);}catch(error){throw this.limits?fieldError(error,`[${i}]`):error;}}));
            return t.rank === 1 ? result : Object.freeze({ dimensions: Object.freeze(dimensions), values: result });
        }
        if (t.kind === 'dictionary') {
            const pairs = array(value); this.claim(pairs.length);
            return new Dictionary(pairs.map((p,i) => { if (array(p).length !== 2) invalid('字典项长度无效'); return [this.nestedConvert(t.key,p[0],depth+1,`[${i}].key`),this.nestedConvert(t.element,p[1],depth+1,`[${i}].value`)]; }));
        }
        if (value === null && t.nullableObject) return null;
        if (t.variants.length) { const tag = text(object(value).$type); if (!t.variants.includes(tag)) invalid('未知多态类型'); return Object.freeze(this.fields(this.types.get(tag)!.fields, value, depth, true)); }
        return Object.freeze(this.fields(t.fields, value, depth));
    }
    read(bytes: Uint8Array, json: boolean): any[] {
        let rows: any[];
        if (json) {
            const d = object(parseJSON(decodeUTF8(bytes)));
            if (Object.keys(d).length !== 4 || d.format !== 'ZrConfig.TableJson/1' || d.table !== this.schema.name || d.schemaHash !== this.schema.hash) invalid('JSON 表头不匹配');
            rows = array(d.rows); if (rows.length > 5000000) invalid('行数过大');
        } else {
            const r = new Cursor(bytes);
            if (r.i32() !== 0x4643525a || r.i32() !== 4 || r.string() !== this.schema.name || r.string() !== this.schema.hash) invalid('二进制表头不匹配');
            const count = r.count(5000000); if (count * 12 > bytes.length - r.pos) invalid('行索引过短');
            const nt = r.count(this.schema.tags.length); const seen = new Set<string>();
            for (let i = 0; i < nt; i++) { const tag = r.string(); if (!this.schema.tags.includes(tag) || seen.has(tag)) invalid('类型目录无效'); seen.add(tag); this.tags.push(tag); }
            const indexes: any[] = []; let end = 0;
            for (let i = 0; i < count; i++) {
                this.remaining = 8000000; const keys = this.schema.keys.map(k => this.readValue(this.schema.fields.find(f => f.name === k)!.type, r, 0));
                const id = r.i32(), offset = r.i32(), length = r.i32(); if (offset !== end || length < 0) invalid('行索引不连续'); end += length;
                indexes.push({ keys, id, offset, length });
            }
            const start = r.pos; if (start + end !== bytes.length) invalid('文件长度不匹配'); rows = [];
            for (const index of indexes) {
                this.remaining = 8000000; const row = this.readFields(this.schema.fields, r, 0);
                if (r.pos !== start + index.offset + index.length || index.id !== (this.schema.hasId ? row.Id : 0)) invalid('行边界或 Id 不匹配');
                for (let k = 0; k < this.schema.keys.length; k++) if (row[this.schema.keys[k]] !== index.keys[k]) invalid('主键与索引不匹配');
                rows.push(row);
            }
        }
        return rows.map(row => { this.remaining = 8000000; return this.fields(this.schema.fields, row, 0); });
    }
}
function segment(value: any): string { const s = text(value); if (!s || s === '.' || s === '..' || /[\\/:\x00-\x1f]/.test(s)) invalid('路径无效'); return s; }
function hash(value: any): string { const s = text(value); if (!/^[0-9a-f]{64}$/i.test(s)) invalid('SHA-256 无效'); return s.toLowerCase(); }
export class Bundle {
    readonly #target:string;
    readonly version: string;
    readonly #entries = new Map<string, any>();
    readonly #format: string;
    readonly root: string;
    readonly manifestDigest: string;
    readonly budget?: LoadBudget;
    #source?: ConfigSource;
    #asyncSource?: AsyncConfigSource;
    constructor(root: string);
    constructor(source: ConfigSource, limits: ResourceLimits, cancellation?: Cancellation);
    constructor(source: string | ConfigSource, limits?: ResourceLimits, cancellation=new Cancellation()) {
        this.root=typeof source==='string'?source:'';
        this.#source=typeof source==='string'?undefined:source;
        this.budget=limits?new LoadBudget(limits):undefined;
        const bytes=this.#read('current.json',limits?.maxManifestBytes ?? Number.MAX_SAFE_INTEGER,cancellation);
        this.manifestDigest=crypto.createHash('sha256').update(bytes).digest('hex');
        try {
        const manifest = object(parseJSONWithBounds(decodeUTF8(bytes),limits?{rows:limits.maxTables,elements:64,members:32,stringBytes:4096,depth:8,rootArray:'tables'}:undefined));
        this.#target=typeof manifest.target==='string'?manifest.target:'';
        if(limits && array(manifest.tables).length>limits.maxTables) throw new ConfigError('LimitExceeded','Table count limit exceeded');
        if (!['ZrConfig.Binary/1', 'ZrConfig.Json/1', 'ZrConfig.Mixed/1'].includes(text(manifest.format))) { if(limits) throw new ConfigError('UnsupportedFormat','Unsupported manifest format'); invalid('清单格式无效'); }
        this.#format = manifest.format; this.version = segment(manifest.versionId);
        for (const item of array(manifest.tables)) {
            const entry=object(item);
            const name = segment(entry.name); if (this.#entries.has(name)) invalid('表重复');
            if (entry.file !== name + (this.#format === 'ZrConfig.Json/1' ? '.json' : '.bytes')) invalid('表文件名无效');
            hash(entry.sha256); hash(entry.schemaHash); integer(entry.rows, 0, 5000000);
            if (this.#format === 'ZrConfig.Mixed/1') { if (entry.jsonFile !== name + '.json') invalid('JSON 文件名无效'); hash(entry.jsonSha256); }
            else if (entry.jsonFile || entry.jsonSha256) invalid('格式与备用文件不一致');
            this.#entries.set(name, entry);
        }
        } catch(error) { if(limits && error instanceof DataFailure) throw new ConfigError('InvalidManifest',error.message,'manifest',0n,{cause:error}); throw error; }
    }
    static async open(source: AsyncConfigSource, limits: ResourceLimits, cancellation: Cancellation): Promise<Bundle> {
        cancellation.check(); let bytes:Uint8Array;try{bytes=await source.readAsync('current.json',limits.maxManifestBytes,cancellation);}catch(error){locatedReadFailure(error,'current.json');} cancellation.check();
        const bundle=new Bundle({read: relative => { if(relative!=='current.json') throw new ConfigError('UnsupportedCapability','Async-only source'); return bytes; }},limits,cancellation);
        bundle.#source=undefined; bundle.#asyncSource=source; return bundle;
    }
    #read(relative: string, maxBytes: number, cancellation: Cancellation): Uint8Array {
        try{
        cancellation.check(); if(this.#asyncSource) throw new ConfigError('UnsupportedCapability','Async-only source');
        const value=this.#source?this.#source.read(relative,maxBytes,cancellation):fs.readFileSync(path.join(this.root,relative));
        cancellation.check(); if(!(value instanceof Uint8Array) || value.byteLength>maxBytes) throw new ConfigError('LimitExceeded','Source returned invalid or oversized bytes');
        this.budget?.raw(value.byteLength); return this.#source?new Uint8Array(value):value;
        }catch(error){if(this.budget)locatedReadFailure(error,relative);throw error;}
    }
    #tableLimit(): number { const limit=this.budget ? Math.min(this.budget.limits.maxTableBytes,this.budget.limits.maxSnapshotBytes-this.budget.readBytes) : Number.MAX_SAFE_INTEGER; if(limit<=0) throw new ConfigError('BudgetExceeded','Snapshot byte budget exceeded'); return limit; }
    /** @internal Validate the complete generated catalog before reading table payloads. */
    validateTables(expected: readonly string[]): void {
        if (this.#entries.size !== expected.length || expected.some(name => !this.#entries.has(name))) invalid('清单表集合不匹配');
    }
    validateCatalog(catalog: Catalog): void {
        if(catalog.target&&catalog.target!==this.#target)throw new ConfigError('CatalogMismatch','Manifest target differs from catalog','manifest');
        if (this.#entries.size !== catalog.tables.length || catalog.tables.some(table => !this.#entries.has(table.name)))
            throw new ConfigError('TableSetMismatch', 'Manifest table set differs from catalog', 'manifest');
        for (const table of catalog.tables) if (this.#entries.get(table.name).schemaHash !== table.schemaHash)
            throw new ConfigError('SchemaMismatch', 'Schema mismatch: ' + table.name, 'manifest');
    }
    load(schema: string, preferJSON: boolean): any[] {
        const wire = new Wire(schema,this.budget?.limits), entry = this.#entries.get(wire.schema.name);
        if (!entry || entry.schemaHash !== wire.schema.hash) invalid('表结构不匹配');
        const alternate = this.#format === 'ZrConfig.Mixed/1' && preferJSON;
        const file = alternate ? entry.jsonFile : entry.file, expected = alternate ? entry.jsonSha256 : entry.sha256;
        const bytes = this.#read('versions/'+this.version+'/'+file,this.#tableLimit(),new Cancellation());
        if (crypto.createHash('sha256').update(bytes).digest('hex') !== hash(expected)) invalid('文件完整性校验失败');
        const rows = wire.read(bytes, this.#format === 'ZrConfig.Json/1' || alternate);
        if (rows.length !== entry.rows) invalid('行数不匹配'); return rows;
    }
    async openRowsAsync(schema: string, preferJSON: boolean, cancellation: Cancellation): Promise<IndexedRows> {
        if(!this.#asyncSource) throw new ConfigError('UnsupportedCapability','Source has no asynchronous read capability');
        const wire=new Wire(schema,this.budget?.limits),entry=this.#entries.get(wire.schema.name);
        if(!entry || entry.schemaHash!==wire.schema.hash) throw new ConfigError('SchemaMismatch','Schema mismatch: '+wire.schema.name);
        const alternate=this.#format==='ZrConfig.Mixed/1' && preferJSON;
        const file=alternate?entry.jsonFile:entry.file, expected=alternate?entry.jsonSha256:entry.sha256;
        const limit=this.#tableLimit(); cancellation.check();
        const relative='versions/'+this.version+'/'+file;let value:Uint8Array;try{value=await this.#asyncSource.readAsync(relative,limit,cancellation);}catch(error){locatedReadFailure(error,relative);} cancellation.check();
        if(!(value instanceof Uint8Array) || value.byteLength>limit) throw new ConfigError('LimitExceeded','Source returned invalid or oversized bytes');
        this.budget?.raw(value.byteLength); const bytes=new Uint8Array(value);
        if(crypto.createHash('sha256').update(bytes).digest('hex')!==hash(expected)) throw new ConfigError('IntegrityMismatch','Table digest mismatch: '+wire.schema.name);
        const result=new IndexedRows(wire,bytes,this.#format==='ZrConfig.Json/1'||alternate,this.budget);result.version=this.version;result.relativePath="versions/"+this.version+"/"+file;
        if(result.count!==entry.rows) throw new ConfigError('InvalidValue','Row count mismatch'); return result;
    }
    openRows(schema: string, preferJSON: boolean, cancellation = new Cancellation()): IndexedRows {
        const wire = new Wire(schema,this.budget?.limits), entry = this.#entries.get(wire.schema.name);
        if (!entry || entry.schemaHash !== wire.schema.hash) throw new ConfigError('SchemaMismatch', 'Schema mismatch: ' + wire.schema.name);
        const alternate = this.#format === 'ZrConfig.Mixed/1' && preferJSON;
        const file = alternate ? entry.jsonFile : entry.file, expected = alternate ? entry.jsonSha256 : entry.sha256;
        const bytes = this.#read('versions/'+this.version+'/'+file,this.#tableLimit(),cancellation);
        if (crypto.createHash('sha256').update(bytes).digest('hex') !== hash(expected)) throw new ConfigError('IntegrityMismatch', 'Table digest mismatch: ' + wire.schema.name);
        const result = new IndexedRows(wire, bytes, this.#format === 'ZrConfig.Json/1' || alternate,this.budget);result.version=this.version;result.relativePath="versions/"+this.version+"/"+file;
        if (result.count !== entry.rows) throw new ConfigError('InvalidValue', 'Row count mismatch: ' + wire.schema.name);
        return result;
    }
}

/** Fixed bytes (binary) or independent JSON row fragments; no decoded rows are retained here. */
export class IndexedRows {
    version="";relativePath="";
    readonly count: number;
    readonly schema: Schema;
    readonly keys = new Map<any, any>();
    readonly ids = new Map<number, number>();
    #wire: Wire;
    #jsonBounds?:JsonAllocationBounds;
    #bytes?: Uint8Array;
    #indexRawBytes=0;
    #json?: readonly string[];
    #offsets: Uint32Array;
    #lengths: Uint32Array;
    #rowKeys: readonly any[][];
    #rowIds: Int32Array;
    #resolve?: (name: string) => IndexedRows;
    bindReferences(resolve: (name: string) => IndexedRows): void { this.#resolve = resolve; }
    constructor(wire: Wire, bytes: Uint8Array, json: boolean, readonly budget?: LoadBudget) {
        this.#wire = wire; this.schema = wire.schema;this.#jsonBounds=jsonBoundsFor(wire.schema,budget?.limits);
        const keys: any[][] = [];
        if (json) {
            const document = object(parseJSONWithBounds(decodeUTF8(bytes),this.#jsonBounds));
            if (Object.keys(document).length !== 4 || document.format !== 'ZrConfig.TableJson/1' || document.table !== this.schema.name || document.schemaHash !== this.schema.hash)
                throw new ConfigError('SchemaMismatch', 'JSON header mismatch');
            const rows = array(document.rows); this.count = integer(rows.length, 0, 5000000); budget?.rows(this.count);
            this.#offsets = new Uint32Array(0); this.#lengths = new Uint32Array(0); this.#rowIds = new Int32Array(this.count);
            const fragments: string[] = [];
            for (let i = 0; i < rows.length; i++) {
                const row = object(rows[i]);
                keys.push(this.schema.keys.map(name => wire.convert(this.schema.fields.find(f => f.name === name)!.type, row[name], 0)));
                this.#rowIds[i] = this.schema.hasId ? integer(row.Id, -2147483648, 2147483647) : 0;
                fragments.push(JSON.stringify(row));
            }
            this.#json = Object.freeze(fragments);
        } else {
            this.#bytes = bytes;
            const cursor = new Cursor(bytes,wire.limits);
            if (cursor.i32() !== 0x4643525a || cursor.i32() !== 4 || cursor.string() !== this.schema.name || cursor.string() !== this.schema.hash)
                throw new ConfigError('SchemaMismatch', 'Binary header mismatch');
            this.count = cursor.count(5000000); budget?.rows(this.count);
            if (this.count * 12 > bytes.length - cursor.pos) throw new ConfigError('InvalidValue', 'Truncated row index');
            const tags = cursor.count(this.schema.tags.length), seen = new Set<string>();
            for (let i = 0; i < tags; i++) { const tag = cursor.string(); if (!this.schema.tags.includes(tag) || seen.has(tag)) throw new ConfigError('InvalidValue', 'Invalid type directory'); seen.add(tag); wire.tags.push(tag); }
            this.#offsets = new Uint32Array(this.count); this.#lengths = new Uint32Array(this.count); this.#rowIds = new Int32Array(this.count);
            let end = 0;
            for (let i = 0; i < this.count; i++) {
                wire.remaining = 8000000;
                keys.push(this.schema.keys.map(name => wire.convert(this.schema.fields.find(f => f.name === name)!.type, wire.readValue(this.schema.fields.find(f => f.name === name)!.type, cursor, 0), 0)));
                this.#rowIds[i] = cursor.i32();
                const offset = cursor.i32(), length = cursor.i32();
                if (offset !== end || length < 0 || length > bytes.length - end) throw new ConfigError('InvalidValue', 'Invalid row bounds');
                this.#offsets[i] = offset; this.#lengths[i] = length; end += length;
            }
            if (cursor.pos + end !== bytes.length) throw new ConfigError('InvalidValue', 'Invalid payload length');
            for (let i = 0; i < this.count; i++) this.#offsets[i] += cursor.pos;
        }
        this.#rowKeys = keys;
        this.#indexRawBytes=this.#bytes?.byteLength ?? this.#json!.reduce((sum,fragment)=>sum+fragment.length*2,0);budget?.retain(this.#indexRawBytes);
        for (let i = 0; i < this.count; i++) {
            let node = this.keys;
            for (let k = 0; k < keys[i].length; k++) {
                const value = key(keys[i][k]);
                if (k + 1 === keys[i].length) { if (node.has(value)) throw new ConfigError('InvalidValue', 'Duplicate row key'); node.set(value, i); }
                else { let next = node.get(value); if (!next) { next = new Map(); node.set(value, next); } node = next; }
            }
            if (this.schema.hasId) {
                const id = this.#rowIds[i];
                // Composite-key tables may have duplicate Id values; only reference access
                // requires uniqueness. Preserve that distinction from the existing format.
                this.ids.set(id, this.ids.has(id) ? -1 : i);
            }
        }
    }
    estimatedIndexBytes(fields:number):number{return 256+this.count*512*Math.max(1,fields)+8*this.#indexRawBytes;}
    readTransient(index:number):any{const estimate=this.estimate(index);this.budget?.reserveTransient(estimate);try{return this.read(index);}finally{this.budget?.releaseTransient(estimate);}}
    estimate(index: number): number { return 256+4*(this.#json ? this.#json[index].length*2 : this.#lengths[index]); }
    read(index: number): any {
        try{return this.readValue(index);}catch(error){throw locateError(error,'row',{versionId:this.version,relativePath:this.relativePath,table:this.schema.name,rowKey:this.#rowKeys[index].map(String).join(',')});}
    }
    private readValue(index: number): any {
        const wire = this.#wire; wire.remaining = 8000000;
        let raw: any;
        if (this.#json) raw = parseJSONWithBounds(this.#json[index],this.#jsonBounds);
        else {
            const start = this.#offsets[index], cursor = new Cursor(this.#bytes!.subarray(start, start + this.#lengths[index]),wire.limits);
            raw = wire.readFields(this.schema.fields, cursor, 0);
            if (cursor.pos !== cursor.data.length) throw new ConfigError('InvalidValue', 'Row boundary mismatch');
        }
        wire.remaining = 8000000;
        const row = wire.fields(this.schema.fields, raw, 0);
        if ((this.schema.hasId ? row.Id : 0) !== this.#rowIds[index]) throw new ConfigError('InvalidValue', 'Row Id differs from index');
        for (let i = 0; i < this.schema.keys.length; i++) if (key(row[this.schema.keys[i]]) !== key(this.#rowKeys[index][i])) throw new ConfigError('InvalidValue', 'Row key differs from index');
        if (this.#resolve) this.validateReferences(row,this.#resolve);
        return row;
    }
    referenceOrdinal(id: number): number {
        const index = this.ids.get(id);
        if (index === undefined || index < 0) throw new ConfigError('MissingReference', 'Missing or ambiguous reference: ' + this.schema.name + ':' + id);
        return index;
    }
    validateReferences(row: any, resolve: (table: string) => IndexedRows): void {
        for (const field of this.schema.fields) {
            const type = this.#wire.types.get(field.type)!;
            if (type.kind === 'tablereference' && row[field.name] !== 0) {try{resolve(type.referenceTable!).referenceOrdinal(row[field.name]);}catch(error){throw locateError(error,'reference',{table:this.schema.name,fieldPath:field.name});}}
        }
    }
    value(row: any): any {
        for (const field of this.schema.fields) if (this.#wire.types.get(field.type)!.kind === 'tablereference') { row[field.name + 'Id'] = row[field.name]; delete row[field.name]; }
        return Object.freeze(row);
    }
}

export function loadPlanFingerprint(mode:LoadMode,json:boolean,tables:readonly string[]):string{const names=tables.map(name=>Array.from(encoder.encode(name),byte=>byte.toString(16).padStart(2,'0')).join('')).sort();return crypto.createHash('sha256').update('zrconfig-load-plan/1\n'+mode+'\n'+(json?'json-preferred':'binary')+'\n'+names.join('\n')).digest('hex');}
export type LoadMode = 'ServerDefault' | 'SelectedTables' | 'LazyRowsStrict' | 'LazyRowsOnAccess';
export interface LoadPlan { readonly mode?: LoadMode; readonly tables?: readonly string[]; readonly preferJSON?: boolean; readonly warmupTables?: readonly string[]; readonly limits?: ResourceLimits; readonly sourceNamespace?: string; }
export function prepareLoadPlan(plan: LoadPlan, catalog: Catalog): { readonly plan: LoadPlan & { readonly mode: LoadMode; readonly limits: ResourceLimits }; readonly selection: SelectionPlan } {
    for(const name of Object.keys(plan)) if(!['mode','tables','preferJSON','warmupTables','limits','sourceNamespace'].includes(name)) throw new ConfigError('UnsupportedCapability','Unknown load option: '+name);
    const mode=plan.mode ?? 'ServerDefault';
    if(!['ServerDefault','SelectedTables','LazyRowsStrict','LazyRowsOnAccess'].includes(mode)) throw new ConfigError('InvalidPlan','Unknown load mode');
    if(plan.sourceNamespace!==undefined && typeof plan.sourceNamespace!=='string') throw new ConfigError('InvalidPlan','Invalid source namespace');
    if(plan.preferJSON!==undefined && typeof plan.preferJSON!=='boolean') throw new ConfigError('InvalidPlan','Invalid format preference');
    if(plan.limits!==undefined && !(plan.limits instanceof ResourceLimits)) throw new ConfigError('InvalidPlan','Resource limits must be validated');
    const selection=catalog.select(plan.tables),warmup=Object.freeze([...(plan.warmupTables ?? [])]);
    for(const name of warmup) if(!selection.tables.includes(name)) throw new ConfigError('InvalidPlan','Warmup outside selection');
    return Object.freeze({plan:Object.freeze({mode,tables:Object.freeze([...selection.requested]),limits:plan.limits ?? new ResourceLimits(),preferJSON:plan.preferJSON,warmupTables:warmup,sourceNamespace:plan.sourceNamespace ?? ''}),selection});
}
class SharedWork<T> {
    #result?: {ok: boolean,value: unknown};
    #waiters=new Set<(ok: boolean,value: unknown)=>void>();
    constructor(work: Promise<T>,done: ()=>void) { work.then(value=>this.#finish(true,value,done),error=>this.#finish(false,error,done)); }
    #finish(ok: boolean,value: unknown,done: ()=>void): void {
        this.#result={ok,value}; done();
        for(const waiter of this.#waiters) waiter(ok,value); this.#waiters.clear();
    }
    wait(cancellation: Cancellation,budget?: LoadBudget): Promise<T> {
        cancellation.check(); if(this.#result) return this.#result.ok?Promise.resolve(this.#result.value as T):Promise.reject(this.#result.value);
        const release=budget?.request(false);
        return new Promise<T>((resolve,reject)=>{
            let finished=false,unsubscribe=()=>{};
            const complete=(ok: boolean,value: unknown)=>{if(finished)return;finished=true;this.#waiters.delete(complete);unsubscribe();release?.();if(ok)resolve(value as T);else reject(value);};
            this.#waiters.add(complete);unsubscribe=cancellation.subscribe(()=>{try{cancellation.check();complete(false,new ConfigError('Cancelled','Wait cancelled'));}catch(error){complete(false,error);}});
        });
    }
}
/** One cache shared by all server modes. References are identifiers; decoding never follows them. */
interface GroupBuild {root:Map<any,any>;fields:readonly string[];next:number;estimate:number;retained:number;done:boolean;failed:boolean;error?:unknown}
export abstract class RowStorage<R extends object> {
    readonly count: number;
    protected readonly keyIndex: Map<any, any>;
    #indexed: IndexedRows;
    #rows: (R | undefined)[];
    #errors = new Map<number, unknown>();
    #groups = new Map<string, Map<any, any>>();
    #groupBuilds = new Map<string,GroupBuild>();
    #groupViews = new Map<readonly number[], readonly R[]>();
    #groupPending = new Map<string,SharedWork<Map<any,any>>>();
    #groupViewPending = new WeakMap<readonly number[],SharedWork<readonly R[]>>();
    #groupErrors = new Map<string,unknown>();
    #make: (value: any) => R;
    #loaded = 0;
    #all?: readonly R[];
    #pending = new Map<number,SharedWork<R | undefined>>();
    protected constructor(indexed: IndexedRows, make: (value: any) => R) { this.#indexed = indexed; this.count = indexed.count; this.keyIndex = indexed.keys; this.#rows = new Array(this.count); this.#make = make; }
    get loadedCount(): number { return this.#loaded; }
    get isComplete():boolean{return this.#all!==undefined;}
    get phase(): 'Indexed' | 'Ready' | 'Degraded' { return this.#errors.size ? 'Degraded' : this.#all ? 'Ready' : 'Indexed'; }
    protected queryOrdinal(index: number | undefined): R | undefined {
        if (index === undefined) return undefined;
        if (this.#errors.has(index)) throw this.#errors.get(index);
        const row = this.#rows[index]; if (!row) throw new ConfigError('RowNotLoaded', 'Load the row explicitly'); return row;
    }
    protected loadOrdinal(index: number | undefined, cancellation?: Cancellation): R | undefined {
        cancellation?.check();
        if (index === undefined) return undefined;
        if (this.#errors.has(index)) throw this.#errors.get(index);
        if (this.#rows[index]) return this.#rows[index];
        const budget=this.#indexed.budget, estimate=this.#indexed.estimate(index); budget?.reserve(estimate);
        try { const value = this.#make(this.#indexed.value(this.#indexed.read(index))); this.#rows[index] = value; ++this.#loaded; return value; }
        catch (failure) { budget?.release(estimate); const error = rowFailure(failure); if (terminalRowFailure(error)) { this.#errors.set(index, error); budget?.fault(); } throw error; }
    }
    protected loadOrdinalAsync(index: number | undefined, cancellation: Cancellation, scheduler: LoadScheduler): Promise<R | undefined> {
        cancellation.check();
        if(index===undefined) return Promise.resolve(undefined);
        if(this.#errors.has(index)) return Promise.reject(this.#errors.get(index));
        if(this.#rows[index]) return Promise.resolve(this.#rows[index]);
        const budget=this.#indexed.budget;
        let work=this.#pending.get(index);
        if(work) budget?.merge();
        else {
            const release=budget?.request(true);
            let promise: Promise<R | undefined>;
            try { promise=scheduler.execute(()=>this.loadOrdinal(index)); if(!promise || typeof promise.then!=='function') throw new ConfigError('InvalidPlan','Scheduler must return a promise'); }
            catch(error) { release?.(); throw error; }
            // Promise.resolve assimilates host thenables before registering callbacks.
            work=new SharedWork(Promise.resolve(promise),()=>{if(this.#pending.get(index)===work)this.#pending.delete(index);release?.();});
            this.#pending.set(index,work);
        }
        return work.wait(cancellation,budget);
    }
    reference(id: number, load: boolean): R | null {
        if (id === 0) return null;
        const index = this.#indexed.referenceOrdinal(id); return (load ? this.loadOrdinal(index) : this.queryOrdinal(index))!;
    }
    loadAll(cancellation?: Cancellation): readonly R[] {
        cancellation?.check(); if (this.#all) return this.#all;
        const rows: R[] = [];
        for (let i = 0; i < this.count; i++) rows.push(this.loadOrdinal(i, cancellation)!);
        for (const group of this.#indexed.schema.groups) this.groupIndex(group.name,true,cancellation);
        this.#all = Object.freeze(rows); return this.#all;
    }
    async loadAllAsync(cancellation: Cancellation, scheduler: LoadScheduler): Promise<readonly R[]> {
        cancellation.check(); if(this.#all) return this.#all;
        const rows: R[]=[];
        for(let i=0;i<this.count;i++) { if(i%64===0) await scheduler.yield(cancellation); cancellation.check(); rows.push(this.loadOrdinal(i)!); }
        for(const descriptor of this.#indexed.schema.groups) await this.groupIndexAsync(descriptor.name,cancellation,scheduler);
        cancellation.check(); if(!this.#all) this.#all=Object.freeze(rows); return this.#all;
    }
    all(): readonly R[] { if (!this.#all) throw new ConfigError('TableNotLoaded', 'Load the table explicitly'); return this.#all; }
    #beginGroup(name:string) {
        const existing=this.#groupBuilds.get(name);if(existing)return existing;
        const descriptor=this.#indexed.schema.groups.find(g=>g.name===name);if(!descriptor)throw new ConfigError('InvalidPlan','Unknown group');
        const estimate=this.#indexed.estimatedIndexBytes(descriptor.fields.length);this.#indexed.budget?.reserveIndex(estimate);
        try {const state={root:new Map<any,any>(),fields:descriptor.fields as readonly string[],next:0,estimate,retained:256+this.count*512*Math.max(1,descriptor.fields.length),done:false,failed:false,error:undefined as unknown};this.#groupBuilds.set(name,state);return state;}
        catch(error){this.#indexed.budget?.releaseIndex(estimate);throw error;}
    }
    #advanceGroup(state:GroupBuild,count:number):void {
        if(state.failed)throw state.error;if(state.done)return;
        const end=Math.min(this.count,state.next+count);
        for(;state.next<end;state.next++){
            const row:any=this.#rows[state.next]??this.#indexed.readTransient(state.next);let node=state.root;
            for(let k=0;k<state.fields.length;k++){
                const raw=row[state.fields[k]];const value=key(raw);if(typeof raw==="string")state.retained+=raw.length*4;
                if(k+1===state.fields.length){let ids=node.get(value);if(!ids){ids=[];node.set(value,ids);}ids.push(state.next);}
                else{let child=node.get(value);if(!child){child=new Map();node.set(value,child);}node=child;}
            }
        }
    }
    #finishGroup(name:string,state:GroupBuild):Map<any,any>{
        if(state.failed)throw state.error;if(!state.done){if(state.retained>state.estimate)this.#indexed.budget?.reserveIndex(state.retained-state.estimate);else this.#indexed.budget?.releaseIndex(state.estimate-state.retained);state.estimate=state.retained;this.#groups.set(name,state.root);state.done=true;this.#groupBuilds.delete(name);}return state.root;
    }
    #failGroup(name:string,state:GroupBuild,failure:unknown):unknown{
        if(state.failed)return state.error;const error=rowFailure(failure);state.error=error;state.failed=true;
        if(!state.done)this.#indexed.budget?.releaseIndex(state.estimate);
        if(this.#groupBuilds.get(name)===state)this.#groupBuilds.delete(name);
        if(terminalRowFailure(error)){this.#groupErrors.set(name,error);this.#indexed.budget?.indexFault();}return error;
    }
    protected groupIndex(name: string, load: boolean, cancellation?: Cancellation): Map<any, any> {
        const existing=this.#groups.get(name);if(existing)return existing;
        if(this.#groupErrors.has(name))throw this.#groupErrors.get(name);
        if(!load)throw new ConfigError('TableNotLoaded','Load the group index explicitly');cancellation?.check();
        const state=this.#beginGroup(name);
        try{this.#advanceGroup(state,this.count);return this.#finishGroup(name,state);}
        catch(error){throw this.#failGroup(name,state,error);}
    }
    protected groupIndexAsync(name: string, cancellation: Cancellation, scheduler: LoadScheduler): Promise<Map<any,any>> {
        cancellation.check();const existing=this.#groups.get(name);if(existing)return Promise.resolve(existing);
        if(this.#groupErrors.has(name))return Promise.reject(this.#groupErrors.get(name));
        const budget=this.#indexed.budget;let work=this.#groupPending.get(name);
        if(work)budget?.merge();else{
            const release=budget?.request(true);const operation=new Cancellation();
            const build=async()=>{
                const state=this.#beginGroup(name);
                try{
                    while(state.next<this.count&&!state.done){await scheduler.yield(operation);this.#advanceGroup(state,64);}
                    return this.#finishGroup(name,state);
                }catch(error){if(state.done)return state.root;throw this.#failGroup(name,state,error);}
            };
            work=new SharedWork(build(),()=>{if(this.#groupPending.get(name)===work)this.#groupPending.delete(name);release?.();});this.#groupPending.set(name,work);
        }
        return work.wait(cancellation,budget);
    }
    protected groupAsync(ids: readonly number[] | undefined,cancellation: Cancellation,scheduler: LoadScheduler): Promise<readonly R[]> {
        cancellation.check();if(!ids)return Promise.resolve(empty);const ready=this.#groupViews.get(ids);if(ready)return Promise.resolve(ready);
        const budget=this.#indexed.budget;let work=this.#groupViewPending.get(ids);
        if(work)budget?.merge();else{
            const operation=new Cancellation();const load=async()=>{const rows:R[]=[];for(const id of ids)rows.push((await this.loadOrdinalAsync(id,operation,scheduler))!);const ready=this.#groupViews.get(ids);if(ready)return ready;const result=Object.freeze(rows);this.#groupViews.set(ids,result);return result;};
            work=new SharedWork(load(),()=>this.#groupViewPending.delete(ids));this.#groupViewPending.set(ids,work);
        }
        return work.wait(cancellation,budget);
    }
    protected group(ids: readonly number[] | undefined, load: boolean, cancellation?: Cancellation): readonly R[] {
        if (!ids) return empty;
        const existing = this.#groupViews.get(ids); if (existing) return existing;
        const rows: R[] = []; for (const id of ids) rows.push((load ? this.loadOrdinal(id,cancellation) : this.queryOrdinal(id))!);
        const result = Object.freeze(rows); this.#groupViews.set(ids,result); return result;
    }
}
// 多级 Map 按分量查找，查询路径不分配数组、对象或拼接字符串。
export function makeIndex<T>(rows: readonly T[], fields: readonly string[], unique: boolean): Map<any, any> {
    const root = new Map<any, any>();
    for (const row of rows) {
        let node = root;
        for (let i = 0; i < fields.length; i++) {
            const k = key((row as any)[fields[i]]);
            if (i + 1 === fields.length) {
                if (unique) { if (node.has(k)) invalid('主键重复'); node.set(k, row); }
                else { let group = node.get(k); if (!group) { group = []; node.set(k, group); } group.push(row); }
            } else { let child = node.get(k); if (!child) { child = new Map(); node.set(k, child); } node = child; }
        }
    }
    function freeze(node: Map<any, any>, depth: number): void { for (const v of node.values()) { if (depth + 1 === fields.length) { if (!unique) Object.freeze(v); } else freeze(v, depth + 1); } }
    freeze(root, 0); return root;
}
