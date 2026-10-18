# Multi-language generation and JSON data

[中文](server-generation.md) | **English**

Generate any combination of C#, C++, Go, Java, Lua, TypeScript, and Rust. Excel side filtering is independent: C exports C/CS, S exports S/CS; Java selection does not change sides. Formats are binary, json, both; old projects default to C#/Binary.

This is the integration/operation guide. See [design constraints](server-runtime-design.en.md), [public acceptance](../tests/README.en.md#public-server-acceptance), and [dated results](release-and-acceptance.en.md#acceptance-and-team-trial-records).

| Task | Section |
| --- | --- |
| Choose languages/side/format | [Usage](#usage), [outputs/dependencies](#outputs-and-dependencies) |
| Handle regenerated API changes | [API protection/migration](#public-api-protection-and-explicit-migration) |
| Verify numbers, collections, converters, files | [Type contracts](#type-and-data-contracts), [files/versions](#files-versions-and-loading) |
| New server integration/loading modes | [server-v1](#server-contract-families-and-explicit-loading) |
| Maintain synchronous full-load consumers | [Legacy loading](#files-versions-and-loading), [lifecycle](#concurrency-performance-lifecycle), [Lua/TS/Rust](#lua-typescript-and-rust-differences) |
| Verify features/measure cost | [Acceptance](#acceptance-scope), [measurements](#repeatable-performance-measurements) |

## Usage

App generation languages allow multiple choices; format offers Binary, JSON, or both. Saved settings are shared by preview, validation-only, export, and CLI.

```bash
# Existing project: generate every language and publish both formats together.
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- \
  export --project "/path/to/project/project.zrconfig.json" --target S --languages all --format both --generate

# Generate selected languages only; preview still lists updated/retired files.
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- \
  codegen --project "/path/to/project/project.zrconfig.json" --target S --languages java,go,cpp --format json
```

Run from root with actual project paths. Windows uses `dotnet run --project src/ZrConfig.Cli -p:NuGetAudit=false --` with arguments on one line. Shared samples already select seven languages, both, C, and server-v1; use samples/project.zrconfig.json without changing side/languages.

--accept-changes retains its meaning: after preview review, allow retired-file removal/input-root changes. Export without --generate never changes source. App generation/export, CLI `codegen`, `export --generate`, and `validate` check the selected languages together with all languages in the accepted baseline by default. Narrowing the selection preserves existing outputs and does not itself require an API migration. Added languages require generation. Explicitly removing languages requires `codegen --prune-languages --languages <retained-languages>` and an API migration; retain `--prune-languages` for preview, `--verify-api`, and `--accept-api-change`. Ordinary --accept-changes cannot bypass API protection. Pruning is not saved in project settings and is invalid for export or validation. Validate checks generated files for staleness without external compilers. Build Java/Go/C++ in business projects/acceptance environments.

### Public API protection and explicit migration

Config/.schema.json now stores ApiContract alongside schema summary, committed with generated source; version them together. The tool compares accepted/candidate baselines before writing. Ordinary --accept-changes, App confirmation, and export --generate cannot approve public API changes.

Above 256 tables, baseline NameEncoding: compact-bindings-v1 uses fixed seven-column name bindings. Only internal reserved names exactly reconstructible from Scope/Name omit repeated origin strings. Reading restores full bindings before name/retirement/history/descriptor checks; frozen scope is unchanged. Digests stream UTF-8; ordinary-size encoding/digests remain unchanged. Unknown/corrupt/incomplete encoding stops writes; older tools cannot treat it as a fresh project.

Large Java catalogs use segmented implementation classes to avoid JVM method/constant-pool limits while retaining ConfigSnapshot.Xxx, DataSnapshot.XxxRow/XxxTable, getters, static imports, and method references. Base constructors are package-restricted. Inheritance/declaration ownership still appears in contract reports; switching layouts cannot bypass API gates or equate source-call compatibility with unchanged reflection ownership/binary ABI. Lua/TS split large initialization into internal functions and bound aggregate expression depth.

Large Rust catalogs use private typed storage segments accessed through private traits; row slots remain OnceLock<Arc<T>>. Row/group scheduling, indexes, and references share cold loading independent of row type, with callbacks borrowing stack state. Hot getters read typed slots without reflection/downcasts/unsafe. Above 256 tables, private build/load functions also limit inlining to control compiler expansion. Public types/methods/modules/hot getters remain. Measure compilation time/peak and runtime loading separately; one catalog result is not a universal capacity guarantee.

- C# extracts actual public/protected declarations, including constructors, parameter names/order/defaults, return types, nullable context, accessors, inheritance, constraints, constants, and overload sets. Removal/signature changes/new same-name overloads block writes. Added fields may change public constructors and are not automatically compatible.
- Six native languages build independent NativeSymbols syntax models from actual output, recording scope/name/full declarations, plus **conservative source digests** covering registered modules/runtimes/build files. Models explain removals/signature/same-name changes; inferred types, binding, and semantics remain protected by digest gates and real consumers. Ordinary comments and generated description/hash metadata are ignored; Lua annotations/string contents remain. Other source changes need review, including internals and aggregate snapshots after new tables. This is not compiler-level API/ABI proof.
- Identical regeneration rewrites neither source nor baseline. Data-only edits do not change APIs; descriptions may update comments. Accepted valid names remain stable despite style changes.
- First upgrades with only old schema summaries import baselines from **actual old generated source**, then compare. Existing source with missing/corrupt/unsupported summaries, deleted protected contracts, or source/baseline disagreement stops writes. Restore from version control; never delete baselines to simulate first generation.

Required migrations initially return API_CHANGE_REQUIRED with changes/full digest. After impact review and consumer checks, rerun identical paths/side/languages with:

```text
codegen ... --verify-api <consumer-checks.json> --migration-receipt <new-receipt.json>
codegen ... --accept-api-change <current-preview-64-character-uppercase-digest> --migration-receipt <receipt.json>
```

Add --accept-changes separately for input-scope/deletion changes. Receipts bind old/new descriptor digests, full changes, target contract version, languages/side, tool/input fingerprints, and consumer results. Digest alone is insufficient. Verification generates candidates in temp and runs explicitly configured consumer commands; failure/timeout/candidate edits prevent receipts. Acceptance increments contract major version, records LastMigration and every old/new step/receipt in ApiMigrationHistory, without a permanent bypass flag. Changed candidates/workbooks/baselines/tools or reused digests fail. Commit rechecks inputs/old files/support/baseline; stale plans yield GENERATION_CHANGED. Explicit migration is codegen-only before export; App offers inspection/cancel, no bypass. API_BASELINE_INVALID requires restoration/review; API_CHANGE_ID_MISMATCH means digest mismatch.

A branch may perform several independent migrations. Release verification connects them from a trusted commit; intervening ordinary additions must remain compatible. Missing/out-of-order/mismatched receipts fail. History neither replaces consumer commands nor constitutes digital signatures; review it with code and preserve accepted records on protected branches.

C# compares Roslyn declarations and compiled public surfaces, including Runtime, support, and business-type public dependency closure. Native models are independent of generator SymbolPlan; public acceptance compiles/calls real consumers. Syntax does not replace inference/overload/ABI checks, so conservative native gates remain. Behavior, bindings, build conditions, and performance need separate acceptance. CI independently compares trusted-commit baseline/source with candidates; editing current .schema.json alone cannot pass.

Consumer checks are JSON arrays with Language, Profile, Executable, Arguments, Directory, Inputs, optional TimeoutSeconds. Every affected language needs commands compiling candidates and running business consumers. Inputs freeze source/config/lockfiles or recursive directory file sets relative to the check file. Before/after checks reject additions/deletions/edits/symlinks in inputs or candidates. Candidates are read-only; builds go to separate temp. {candidate} is temporary Config root; {input} original Excel root. Commands run as direct subprocess arguments, without shell, and must be explicitly configured/reviewed. Receipts digest commands/inputs/executable/output: reviewable evidence, not signatures or a substitute for reviewing commands/unknown consumers.

```json
[
  {
    "Language": "csharp",
    "Profile": "business-release",
    "Executable": "dotnet",
    "Arguments": ["run", "--project", "Consumer.csproj", "-c", "Release", "-p:ConfigRoot={candidate}"],
    "Directory": ".",
    "Inputs": ["Consumer.csproj", "Program.cs"],
    "TimeoutSeconds": 300
  }
]
```

This illustrates configuration format only. Business projects must provide Consumer.csproj and compile real calls against the candidate. Use actual language compilers/tests, not empty commands.

New optional project JSON settings:

```json
{
    "Target": "S",
    "CodeLanguages": ["csharp", "java", "go", "cpp", "lua", "typescript", "rust"],
    "DataFormat": "both",
    "RuntimeFamily": "auto",
    "Namespaces": {
        "CSharp": "Game.Config",
        "Java": "game.config",
        "Go": "example.com/game/config",
        "Cpp": "game::config",
        "Rust": "game_config",
        "TypeScript": "@game/config",
        "Lua": "game.config"
    }
}
```

Namespaces and individual entries may be omitted, inheriting accepted baselines; defaults apply only initially. C# shares ZrConfig.Runtime; Go remains package zrconfig with module root in generated go.mod; TS gets package metadata; Lua dot-separated roots determine paths. Roots do not change wire type IDs; changing accepted roots requires migration. Public tests cover two Catalogs in one process for seven languages. App preserves these settings.

Only new options are shown; retain DataFolder/CodeFolder/OutputFolder/RulesFolder. One operation uses one side. Other sides require matching code/data; loaders reject incompatible schemas.

## Outputs and dependencies

Languages share Excel parsing, filtering, rules, summaries, transactions, and publication. Generators receive type descriptors without CLR/reflection objects. Tool-side adapters map existing C# definitions/converters, preserving input compatibility; server runtimes need no .NET.

| Location | Contents | Business-project requirements |
| --- | --- | --- |
| Config/Generated/*.g.cs | C# tables, queries, codecs, one TableCodecs | C# 9 / .NET Standard 2.1 or existing Unity |
| Config/Generated/Java | XxxConfig.java per table, ConfigTypes, ConfigRuntime, ConfigSnapshot | Java 17+, package zrconfig, JDK only |
| Config/Generated/Go | xxx.g.go per table, types.go, runtime.go, snapshot.go | Go 1.19+, package zrconfig, standard library; business go.mod manages module |
| Config/Generated/Cpp | XxxConfig.hpp per table, types.hpp, runtime.hpp, snapshot.hpp | C++17, include snapshot.hpp, standard library/filesystem |
| Config/Generated/Lua | XxxConfig.lua per table, types.lua, runtime.lua, snapshot.lua | Lua 5.4 with 64-bit ints/doubles, standard library, LuaLS annotations |
| Config/Generated/TypeScript | XxxConfig.ts per table, types/runtime/snapshot.ts, tsconfig.json | TS 5.8+, Node 22+, ES2022/CommonJS, Node standard library |
| Config/Generated/Rust | XxxConfig.rs per table, types/runtime/snapshot/lib.rs, Cargo.toml | Rust 1.70+, edition 2021, standard library, no third-party crates |

All source is UTF-8. MSVC uses /std:c++17 /utf-8 /EHsc; Linux/macOS -std=c++17.

Server-only selections still generate internal C# models for existing rules/export checks; do not copy these into servers. Native maintenance source exists only in Assets/ZrConfig/Native, embedded as text templates. Generated files are rebuildable. Transactions remove only registered native files, never recursively business files; unregistered name collisions block overwrite.

Excel row-4 descriptions become Chinese public-field/accessor comments, preserving multiline content with escaping. Empty descriptions add no comments. Descriptions do not affect schema hashes, but edits update generated source.

## Type and data contracts

| Type | Shared meaning / JSON | Language notes |
| --- | --- | --- |
| sbyte/byte/short/ushort/int/uint | Fixed-width signed/unsigned integers, JSON numbers | Java widens unsigned values to fitting signed types |
| long/ulong | 64-bit integers, **decimal strings** in JSON | Java ulong uses BigInteger to avoid overflow |
| float/double | IEEE 754 single/double, JSON numbers | Reject NaN/Infinity; positive/negative zero query equally |
| decimal | 96-bit coefficient, 0–28 scale, **decimal strings** | Exact Java/Go/C++ Decimal, no float conversion/rounding |
| bool/string | JSON bool / UTF-8 string | Strict UTF-8/surrogate/duplicate-member checks; original text retained |
| enum | Explicit values respecting underlying integer type | Unknown values rejected; aliases retained |
| T? | null or T | Java wrappers, Go Optional, C++ optional |
| Arrays/lists | JSON arrays | Read-only after loading; empty differs from null |
| Multidimensional arrays | `{"dimensions":[2,2],"values":[1,2,3,4]}` | Row-major; dimensions/count agree; empty dimensions retained |
| `Dictionary<K,V>` | `[[key,value], ...]` | Keys not coerced into member names; duplicates rejected |
| Ordinary objects | Every declared member; reference types may be null | No business behavior or automatic C# method translation |
| Polymorphic objects | Additional $type | Explicit full type IDs, declared concrete subclasses only |
| RawJson | String preserving original JSON text | Not a switch for outer data format |
| Table references | Target int32 Id; 0 means absent | Bound within one snapshot; cycles allowed, missing targets rejected |

Excel/type definitions remain the schema source. JSON data is not a new schema entry: missing/extra fields, wrong types/ranges, unknown enums/subtypes fail without silent defaults. Existing parsers determine blank-cell defaults and write them fully. Tool fixes member/dictionary ordering and binary protocol; business code must not depend on JSON whitespace/display order.

Protection limits: 5,000,000 rows/table, 1,000,000 elements/collection, 64 type levels, 16 MiB UTF-8/string, 8,000,000 nested elements/row. JSON also caps 32,000,000 value nodes, 72 text levels, and 1,024-character number tokens. Exceeding limits fails explicitly. These are safeguards, not recommended capacities; measure actual configuration against deployment budgets.

Existing key/group constraints remain, including composite keys/ranges/indexable types. Only referenced tables need globally unique integer Id; ordinary composite tables may repeat Id. Missing find returns null in Java/C++, (nil,false) in Go; get/Get throws/panics. Missing groups are empty.

Public names obey language restrictions: Java Class accessor getClass_, Go lowercase fields Get_ prefix for export, C++ keywords trailing underscore. Wire names preserve Excel. Escaping/type/snapshot collisions fail preview rather than emit uncompilable source.

SymbolPlan checks public/internal/helper/module/path collisions, including Java keywords/constructor parameters, C++ v_/ref_ storage, case-folded paths, Windows device names. Go _Item emits table__item.g.go so builds do not ignore it. Accepted normal names stay stable. New symbols need planning rules/closure tests, plus independent declaration and compiler acceptance.

### Custom converters

Arbitrary C# Read/Write cannot automatically translate to six other languages. For standard encoded member sequences, declare layout on type/converter:

```csharp
[ConfigConverter(typeof(MoneyConverter), "1")]
[ConfigWireLayout(nameof(Cents))]
public readonly struct Money
{
    public long Cents { get; }
    public Money(long cents) { Cents = cents; }
}
```

This declares direct standard-protocol Cents encoding, without private bytes/compression/custom headers. With native languages, even Binary-only, real exports undergo byte-exact Binary→JSON→Binary comparison and original-codec read-back; mismatch blocks publication. Layout-free converters retain C# Binary support but fail JSON/other-language selection. Layout affects schema signatures; semantic encoding changes still require converter Version updates. This is a developer-maintained contract, not automatic proof of arbitrary converter logic.

## Files, versions, and loading

Binary remains v4: little-endian integers, length-prefixed UTF-8, fixed schema hashes, row indexes, explicit polymorphic catalogs. JSON format is ZrConfig.TableJson/1:

```json
{
    "format": "ZrConfig.TableJson/1",
    "table": "Item",
    "schemaHash": "schema-hash-matching-generated-code",
    "rows": [{ "Id": 1, "Name": "Potion", "Price": "10000000000000000" }]
}
```

Data is grouped by selected language: `Output/CSharp/Tables/bytes/Hero.bytes`, `Output/CSharp/Tables/json/Hero.json`, and `Output/CSharp/Common/current.json`. Java, Go, Cpp, Lua, TypeScript and Rust have the same structure. The manifest has `layout: "grouped"`; loaders use the language folder as their root. Only selected languages are updated. Legacy flat and version layouts remain readable. Every export validates all tables, then replaces only changed/missing/corrupt files and writes the manifest last. Unchanged files retain their bytes and timestamps. No version directories or data history are created; existing legacy directories remain readable. The deterministic `versionId` is an internal content/cache identity, not a directory. A fixed directory is not an immutable snapshot of on-disk files: finish export before reloading; lazy loaders with old metadata must reload before fetching changed files. Manifest formats: ZrConfig.Binary/1, ZrConfig.Json/1, ZrConfig.Mixed/1:

- binary: .bytes per table; file/sha256 refer to Binary.
- json: .json per table; file/sha256 refer to JSON.
- both: both formats in one version; file/sha256 refer to Binary, jsonFile/jsonSha256 to JSON.

Every format verifies names, schema hashes, SHA-256, counts, keys, references. SHA-256 ensures integrity, not signature authentication. Unknown/corrupt/mismatched data never silently falls back to another copy. Sample deployment verifies and copies both formats into a fixed directory.

Six native snapshot loaders compare the complete manifest/generated table set before reading table contents. Extra/missing/equal-count replacement tables fail; duplicate tables fail parsing. Failure preserves Current. Traditional ConfigSnapshot/ConfigManager below retain synchronous full-load semantics; selected/row loading and Clear/Close use later server contracts.

```java
var manager = new zrconfig.ConfigSnapshot.Manager();
var snapshot = manager.reload(java.nio.file.Path.of(output), false);
var item = snapshot.ItemCatalog.get(1001);
// false prefers Binary; true selects JSON in dual-format versions. JSON-only always reads JSON.
```

```go
var manager zrconfig.ConfigManager
snapshot, err := manager.Reload(output, false)
if err != nil { return err }
item, exists := snapshot.ItemCatalog().Find(1001)
```

```cpp
#include "snapshot.hpp"
zrconfig::ConfigManager manager;
auto snapshot = manager.reload(output, false);
const auto* item = snapshot->ItemCatalog().find(1001);
// Keep snapshot alive throughout item, collection-reference, and cross-table-pointer use.
```

C# ConfigLoadOptions adds preferJson and retainPreviousSnapshots, both false by default. JSON needs no extra package: loading converts it to existing row-reader structures, reusing query indexes. Example:

```csharp
var options = new ConfigLoadOptions(retainPreviousSnapshots: true, preferJson: true);
var manager = new ConfigManager(TableCodecs.Registry, decodeManifest, options);
var snapshot = manager.ReloadAll(new FileConfigSource(output));
```

Actual registry entry points are defined by current TableCodecs. Registry-free integration, async sources, and Unity adapters remain available. Servers requiring fully validated candidates should use ReloadAll/ReloadAllAsync rather than publish unread manifests alone.

## Server contract families and explicit loading

Generated Catalog format 2 carries server-v1 contract, side, and required capabilities. All seven runtimes check capabilities on catalog creation; unknown ones yield UnsupportedCapability. Format-1-only runtimes reject format 2 rather than ignore requirements. Data remains Binary v4/JSON v1; catalog metadata does not change row encoding.

New server catalogs strictly check manifest target. Missing/mismatched side yields CatalogMismatch before table reads even with matching names/hashes. C# custom decoders must preserve ConfigManifest.target. Generic registries/old Catalog constructors retain previous behavior.

Manifest/table JSON budgets for members/collections/strings/depth are checked before parse-tree construction. Table limits include field names, subtype tags, matrix dimensions, and dictionary-pair overhead, then exact business limits are rechecked. MaxContainerElements applies cumulatively per row. Diagnostic indexes are zero-based; matrices use row-major flat indexes, dictionaries [index].key/.value, e.g. Mapping[0].value. Lua business indexing remains unchanged.

New projects default server-v1, retaining old consumers and adding separate server APIs. Existing projects inherit accepted family when omitted. Migrate explicitly with --runtime-family server-v1 and consumer acceptance; legacy-v1 retains old APIs. Server behavior below concerns new entry points; traditional examples keep old contracts.

| Language | Server entry | Explicit row-load form | Cached-query form |
| --- | --- | --- | --- |
| C# | ZrConfig.Runtime.Server.Manager, generated ServerCatalog | snapshot.GetEntryTable().Load("east", 1) | snapshot.GetEntryTable().Get("east", 1) |
| Java | `DataSnapshot`、`ConfigRuntime.ServerManager` | `snapshot.get_Entry().load("east", 1, cancellation)` | `snapshot.get_Entry().get("east", 1)` |
| Go | DataSnapshot, ServerManager[DataSnapshot] | Get_Entry() table, then Load(ctx, "east", 1) | Table.Get("east", 1) |
| C++ | `zrconfig::data::Snapshot`、`ServerManager` | `snapshot->load_Entry("east", 1, cancellation)` | `snapshot->get_Entry("east", 1)` |
| Rust | `data::Snapshot`、`runtime::ServerManager` | `snapshot.load_Entry("east", 1, &cancellation)` | `snapshot.get_Entry("east", 1)` |
| TypeScript | `data.Snapshot`、`runtime.ServerManager` | `snapshot.get_Entry().load("east", 1)` | `snapshot.get_Entry().get("east", 1)` |
| Lua | `data.load`、`server.manager` | `snapshot.get_Entry().load('east', 1, cancellation)` | `snapshot.get_Entry().get('east', 1)` |

Names/keys/groups come from each project. These examples use fixture Entry key (Zone, Id). Use generated concrete types rather than string table names.

Four modes share scope across seven languages: ServerDefault full load; SelectedTables selected tables plus reference closure; LazyRowsStrict pins selected data and validates every row, creating business rows on demand; LazyRowsOnAccess pins data/builds key indexes and fully validates non-key data on access. Lazy loading retains Binary/JSON row slices, not single-row random disk I/O.

Get/Find query caches only, without I/O/decoding. Existing unloaded rows yield RowNotLoaded; out-of-scope tables TableNotSelected. Row/table/group loads share caches. First group load may scan a whole table for its index, then create only matching rows: a cold cost. References expose Id first; Get retrieves loaded targets, Load explicitly loads them. Cycles never recursively auto-expand.

Prepare pins data and completes selected/business validation, producing a candidate; Commit publishes. Clear removes Current and invalidates in-flight candidates; Close also forbids future acquisition/updates. Safe row/table/group views own their snapshot and remain readable after update/Clear/Close. Do not apply traditional borrowed C++/old Rust semantics to these safe views.

C#/Java/TS provide async APIs. Java uses business Executor; TS explicit scheduler/event-loop yields, not a promise all decoding moved to Workers. Go uses context/goroutines; business executors schedule blocking C++/Rust; Lua runs in one state. One waiter cancellation does not cancel shared work. Sources ignoring cancellation still occupy preparation slots until they actually exit.

### Source-name mapping and column moves

Accepted .schema.json preserves field order/composite-key parameter order. Moving Excel columns neither swaps same-type parameter meaning nor changes existing encoding order. Changing key sets is still migration. Cell locations always use current columns, never old indexes.

Use codegen --source-map <JSON-file> for explicit mapping, e.g. rename Lookup sheet to Targets and Entry.Name root field to DisplayName:

```json
[
  { "Table": "Lookup", "Field": null, "SourceName": "Targets" },
  { "Table": "Entry", "Field": "Name", "SourceName": "DisplayName" }
]
```

Table/Field always name accepted logical identities; SourceName names current Excel. Public types/methods/files/wire names/SymbolId remain Lookup/Name. References may use TargetsConfig and bind to original LookupConfig. Root-field mappings cover expanded members, not custom-type internals or cell JSON.

This submits a mapping candidate without editing Excel. Ordinary --accept-changes cannot approve it. Keep --source-map while using verify-api/receipt/accept-api-change for the exact candidate. Accepted maps live only in the API contract and are inherited; temporary request files are unnecessary afterward. Generated assemblies carry read-only copies for DLL export; source/DLL exports agree.

Further renames bind original logical names directly to new source names, without alias chains. Mapping SourceName back to logical name requests removal and still needs migration acceptance. Missing old identities, duplicate targets, occupied accepted names, and unmatched input fail. This preserves public names; new public names/types require ordinary API migration.

### Snapshot identity and diagnostics

Server snapshots expose source identity, version, raw manifest digest, Catalog fingerprint, and load-plan fingerprint as the cache-domain identity. Row/reference caches still belong to individual instances. Manager conflict history compares only source+version→manifest digest, avoiding false conflicts from mode changes. Old three-field SnapshotIdentity serves conflict detection, not cross-snapshot cache sharing.

CatalogFingerprint/camelCase/snake_case comes from generated catalogs; C# legacy registries derive separate identity from full table names/hashes. LoadPlanFingerprint uses zrconfig-load-plan/1: SHA-256 over version line, mode name, binary or json-preferred, then lowercase hex UTF-8 names of the selected closure, byte-sorted/newline-separated without trailing newline. Same closure ignores request order; mode/preference changes differ. Budgets/schedulers/diagnostics/warmup affect admission/readiness, not interpretation identity. Keep full preparation audit parameters separately.

CompletedTableCount counts tables with all rows and required group indexes ready; loading every row individually is not automatically full-table completion. Reading counts triggers no loading. StageTimings returns two fixed records: latest-operation completed Prepare, success/failure, and latest successful Commit. Each has stage/operation ID/ms/success; ID 0 means none. Older late preparations cannot overwrite newer records. Reentrant callbacks can read failed timing and returned preparation slots.

Timing is cold-path only, with no hot-query timers/logs. C#/Java/Go/C++/Rust/TS use monotonic elapsed time including source/validator/scheduler waits. Lua os.clock measures process CPU excluding blocking waits; host deadline clocks are separate. Commit ends at publication, excluding old destruction/notifications; rejection preserves last successful Commit. These are lifecycle-stage timings, not isolated I/O/decode/GC profiling. Query counts, budget estimates, and real memory remain separate.

### Live-snapshot memory admission

Decoded rows, estimated decode/index bytes, source bytes, concurrency, and waiters have separate limits. MaxEstimatedLiveBytes additionally limits update peaks while old snapshots remain held. It estimates configuration memory, not a hard RSS/process-heap ceiling. Check before preparation, after candidate completion, and before commit; also constrain source/snapshot budgets.

| Language | Configuration | Old snapshot tracking |
| --- | --- | --- |
| C# | PreparationOptions ResourceLimits(maxEstimatedLiveBytes: ...) | Weak references; reread estimates |
| Java | ManagerPolicy.withEstimatedLiveBudget(maximum, estimator) | WeakReference; estimator outside manager lock |
| Go | ManagerPolicy.MaxEstimatedLiveBytes | Independent leaf GC marker updates reclamation counts, not resource cleanup |
| C++ | ManagerPolicy.with_estimated_live_budget(maximum, estimator) | weak_ptr; generated snapshots can use diagnostic estimates |
| Rust | ManagerPolicy.with_estimated_live_budget(maximum, estimator) | Weak; estimator returns ServerResult<u64> |
| TypeScript | maxEstimatedLiveBytes, liveEstimator | WeakRef; snapshots must be objects |
| Lua | max_estimated_live_bytes, live_estimator | Weak-key table |

Group indexes default to a separate estimated 2 GiB budget. C#: ResourceLimits.WithIndexBudget(bytes); Java Builder.maxEstimatedIndexBytes; Go MaxEstimatedIndexBytes; C++/Rust/Lua max_estimated_index_bytes; TS maxEstimatedIndexBytes. Reserve before building, retain on success, return on failure/cancel; published cache hits are not charged again. EstimatedIndexBytes variants expose diagnostics. C# includes base row-index estimates; other languages count declaredRows×64 separately in totals.

Derived indexes reserve workspace of 8× pinned raw table bytes + 512 bytes per row/key component + 256 fixed bytes. Reuse fully cached rows; uncached rows decode temporarily without filling row caches. On success return temporary credit, retaining fixed/per-component costs plus 4× string-key lengths (UTF-16 units there, UTF-8 bytes elsewhere), conservatively counting duplicates. Generated C# GroupDescriptor carries component/string estimates; custom descriptors may use WithIndexEstimate. Estimates may exceed/fall below actual heaps and are not allocator measurements. Strict scans/group scans consume decode-byte and concurrency budgets temporarily, not cached-row quota. Measure custom objects/estimators; admission estimates never replace process limits. Interleaved TS sync loading/async indexing share progress/workspace/one reservation; sync may finish work and async waiters receive the same published index.

Native live-budget admission is off by default. When enabled, tracking is bounded, default 1024 records, pruning reclaimed entries on admission. Java/C++/Rust/TS/Lua estimators must be fast/pure and not capture estimated snapshots. Go generated snapshots maintain SnapshotMemory; custom snapshots implement it, stably returning an owned GC marker updated through NewSnapshotMemory/SetEstimatedBytes. Never copy markers; views still own snapshots. Manager keeps counter state, not original data graphs.

GC-language quota returns may lag released references; production never forces GC. C++/Rust can observe disappearance at last strong release. Budget rejection invalidates no handles and preserves Current. Row/index failures count separately; deterministic faults mark Degraded. Public callers cannot mutate cached errors.

## Concurrency, performance, lifecycle

This and subsequent Lua/TS/Rust examples demonstrate retained traditional synchronous full-load APIs. For server-v1 explicit loads, safe handles, Prepare/Commit see [server contracts](#server-contract-families-and-explicit-loading).

Java/Go/C++ fully parse/index/link before atomic publication. Acquire Current once per request/battle and retain it; do not reread mid-operation. Uninitialized Java Current throws, Go/C++ return nil/empty shared_ptr: load before accepting work. Failed loads preserve Current; reloads serialize, reads avoid reload locks. Managers retain no historical list; business references determine reclamation.

Traditional C++ record/collection/table pointers belong to snapshots and do not extend lifetime: retain shared_ptr. Java/Go records are read-only; never mutate through reflection/package internals. C# retainPreviousSnapshots detaches retired snapshots from managers; disposing managers does not invalidate held retired snapshots. Defaults still invalidate old snapshots after updates.

Normal key/composite/group/bound-reference hits reuse rows/indexes without string keys, temporary key objects, or collection copies. Java/C++ use row-number open-addressing indexes; Go maps comparable struct keys. Decimal normalization/hash happens at construction. Reuse bigint/Decimal/string key objects; parsing/new allocations inside query expressions belong to callers.

Loading, JSON parsing, collections/indexes, and reload allocate; no whole-load zero-GC claim. JSON usually costs more bytes/temp memory than Binary; C# also transcodes. Longer old-snapshot retention increases reload memory. Copy/formatting/exceptions/business LINQ/streams may allocate.

## Acceptance scope

Local automation compiles/interprets seven languages, reads both formats, tests 64-bit/decimal boundaries, Unicode/nulls/arrays/dictionaries/matrices/enums/polymorphism/composite keys/groups/cycles/failed reload/retained snapshots/hot allocations. Nine official tables including AllTypes10k also undergo generation/compilation/dual reads.

Public users run [server acceptance](../tests/README.en.md#public-server-acceptance). Full workspaces additionally use ZrConfig.Tests --portable-native with all six toolchains, configured by ZRCONFIG_JAVAC/GO/CXX/NODE/LUA/RUSTC and TSC or TYPESCRIPT. --portable-only still runs managed/generation/data checks without full toolchains and explicitly reports skips. Full internal entry remains bash tests/smoke.sh. Public ZrConfig.Server.Tests defaults all native tools; --managed-only is not seven-language acceptance. Protected-branch API checks use --trusted-ref <immutable-SHA>.

Minimums: Java 17, Go 1.19, C++17, Lua 5.4, Node 22 / TS 5.8, Rust 1.70. Archive actual compiler/allocation versions/results. macOS success is not Windows/Linux execution, Unity Player, or real server stress acceptance.

## Lua, TypeScript, and Rust differences

New languages reuse schemas/data without new Excel/Rules/Output directories. --languages all means seven; comma-separated subsets remain supported.

TS emits readonly interfaces, enum constants, collections, and query tables. Compile tsc -p Config/Generated/TypeScript, then import snapshot.js in Node. Rows/arrays freeze; dictionaries expose reads only. All 64-bit integers use bigint, decimal exact Decimal. Composite keys query nested Maps by component without key arrays.

```ts
import { ConfigManager } from './Generated/TypeScript/snapshot';
const manager = new ConfigManager();
const snapshot = manager.reload(output, true);
const item = snapshot.ItemCatalog.get(1001);
// item.Id/item.Name are readonly; references target records in the same snapshot.
```

LuaLS/EmmyLua describes generated/custom types, but runtime is dynamic. require('your.module.prefix.snapshot') reuses that prefix for peers, or add Generated to package.path and require('snapshot'). Lua 5.1/LuaJIT cannot directly run this 5.4 library. Loading does not yield coroutines; success swaps within one state; each state owns a manager.

```lua
local Config = require('zrconfig.snapshot')
local manager = Config.manager()
local snapshot = manager.reload(output, true)
local item = snapshot.ItemCatalog.get(1001)
-- Use dot calls; find returns nil for missing keys, get throws.
```

Lua long is 64-bit integer; ulong/enums decimal strings; decimal normalized exact strings—reuse Config.decimal('1.25'). JSON null/empty references use Config.NULL to preserve positions/length, not nil. Arrays are one-based, dictionaries .get(key), matrices dimensions/values. Readonly proxies block normal assignment; business rawset/debug/internal mutation is forbidden. Iterative freezing handles chains/cycles without recursive stack exhaustion.

Rust emits private fields/get_Field() methods; tables get/find, groups get_Group, snapshots get_Table. Ordinary queries borrow; groups return borrowed Group views. Enums use explicit value types/constants. Decimal stores exact coefficient/scale; comparison/hash never formats strings.

```rust
let manager = zrconfig_generated::ConfigManager::new();
let snapshot = manager.reload(std::path::Path::new(output), true)?;
let item = snapshot.get_ItemCatalog().get(1001)?;
let name = item.get_Name();
```

Rust Mutex serializes reload; RwLock protects snapshot publication/acquisition. After acquiring Arc, queries avoid manager locks. References prebind Weak and upgrade to Arc without heap allocation; cycles permit reclamation. **Hold the whole Arc snapshot throughout reference use**; cloning one row does not retain other tables, and released targets return None. Production uses no unsafe.

Traditional TS reload is synchronous, replacing references in one event loop; Workers manage snapshots separately. Large loads block, so schedule at initialization/allowed times, not as nonblocking I/O. TS heap differences include engine/measurement costs and detect growth, not exact allocations. Lua pauses GC; Rust counts global allocator calls; both measure already-loaded queries only.

Runtime references: [Lua 5.4](https://www.lua.org/manual/5.4/manual.html), [Node TypeScript](https://nodejs.org/api/typescript.html), [Rust synchronization](https://doc.rust-lang.org/std/sync/). Node type stripping does not type-check; acceptance must separately run strict TS compilation.

### Repeatable performance measurements

Public source includes tests/ZrConfig.Server.Tests/ServerRuntimeBenchmarks.cs. Set ZRCONFIG_* tool paths and run Release:

```bash
bash scripts/dotnet.sh build tests/ZrConfig.Server.Tests -c Release -p:NuGetAudit=false
bash scripts/dotnet.sh tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll --benchmark artifacts/performance/latest 10000 3
```

--benchmark-scenarios <report-dir> 3 uses temporary 1,000-row input to compare normal sources, 1 ms/read delay, second-read failure, and 1/4 warm readers in threaded runtimes. Processes verify checksums/failure snapshot identity/generation. Threads reach barriers before timing, including wake/join; TS/Lua single states claim no Worker/multistate concurrency. Lua delay is explicitly CPU busy-wait, not blocking I/O. Scenarios remain separate from ordinary cold/retained samples.

--benchmark-retained <report-dir> 3 measures peaks/release with 0/1/4 independent retained snapshots of one immutable version, separately full/lazy. It isolates ownership memory; manager retirement/rollback are separate regressions.

--benchmark-queries <report-dir> 3 separately measures key hits/misses, string/int composites, built groups, resolved references, and manager acquisition. Warm 20,000 times; measure 1,000 batches×100 operations with checksum verification. Alternate hit keys, varying negative misses, existing/empty groups. Record p50/p95/p99 batch means and available allocation/heap trends. Includes callbacks/bounds/safe-handle ownership, not bare hash or per-request tails. Do not generalize one path to all hot paths.

--benchmark-stages <report-dir> <root> 3 instruments temporary generated/C# Runtime copies for read/hash/manifest/scan/decode/index/reference/commit/plan. Markers must match uniquely; absent/uncalled/negative stages fail. Record original/instrumented digests. Inclusive timings cannot be summed or treated as uninstrumented throughput. Scan includes structure/some keys; decode parses rows, plus C#/Lua construction paths; indexes vary by language. Temporary native conversion/full construction boundaries differ; compare languages using total load time. Official outputs stay untouched.

--benchmark-control <report-dir> 3 separates initial catalog/module setup from loading and tests 10,000-node chains, 256-node complete directed cycles, and 10,000 independent tables, selecting each graph 20 times with assertions. Generator records three SCC timings. This tests directory scale, not generation/full decoding of 10,000 business tables. Each language/format cancels at second read 100 times, measuring cancellation-to-exit including cleanup/invariants and checking Current/generation/tasks. Threaded languages barrier-start 4 readers/1 updater for 40,000 acquisitions/100 commits; TS/Lua interleave one state. C# includes real Prepare I/O, others reuse prepared snapshots to isolate manager cost, so do not rank languages directly. Old snapshots remain readable after updates/Clear/Close.

`dotnet run --project tests/ZrConfig.Server.Tests -c Release -- --benchmark-table-scale <report-dir> 10000 3` performs actual 10,000-table generation, dual export, repeat-generation stability, seven-language compilation/loading separately from graph reports. Records baseline/build sizes and actual loads. ZRCONFIG_BENCH_TIMEOUT_SECONDS sets subprocess timeout, default 300, range 1–3600; large compilation may use 3600. KEEP_SCALE=1 preserves temp failure artifacts; TABLE_WORKBOOK may reuse prior TinyTables.xlsx. Counts/mode outcomes still must pass; retention/time extension is not acceptance.

Large TS checks may exceed Node's default heap. ZRCONFIG_TYPESCRIPT_HEAP_MB=16384 passes --max-old-space-size=16384 only to compiler subprocesses and records it. Load/query processes do not inherit it. Unset uses Node defaults; range 64–65536 MiB, invalid values fail before fixtures. Heap cap is not whole-process memory cap; inspect peak RSS too.

Large catalogs split internal implementations per language: Java bounds class/initializer bytecode; Go segments private storage/cold loaders to control generic descriptions; Rust uses heap segments and isolates concrete root storage to bound drop/initialization/validation/dispatch analysis; TS/Lua bound initializer size; C++ shares forward declarations/scheduling/ownership and private index/warmup callable templates. Threshold is 256 tables, typically 128/segment. Threshold/helpers/layout/names are internal; public row/table/group/load APIs remain. Crossing thresholds still requires declaration comparison/frozen consumers, never compatibility bypass.

Large Go reports record consumer module names. Minimum toolchains have 32-bit object-file offsets; repeated module paths/generic names mean path/name lengths/index counts affect compile scale. Fixed modules remove random-temp-path bias, not guarantee arbitrary business paths/scales. Report compiler objects, executables, and runtime memory separately.

--benchmark-locks <report-dir> <root> 3 uses temporary runtimes in control scenarios. C#/Java/Go/C++/Rust record manager-state lock acquisition count/total/max, including uncontended operations/timer overhead—not pure blocked time, row/index/queue locks. Atomic sampling alters contention. TS/Lua are not applicable. C# instrumentation uses a precompiled consumer, whose RSS is not directly comparable with dynamic baseline workers.

--benchmark-samples <report-dir> <root> 3 copies official nine-table Data and handwritten Config into temp, records workbook digests, and measures without modifying originals. --sample-consumer <language\|all> <Config-dir> <Output-dir> independently compiles/runs frozen legacy consumers for migration receipts without modifying candidates.

Commands use temporary workbooks and independent seven-language processes, saving raw server-runtime.json with toolchains/build time/formats/modes/coverage/first-row/group/hot-batch/allocation/heap/release metrics. ZRCONFIG_BENCH_TIME=1 records whole-child peaks on macOS/Linux; unavailable values are null. Reports define timing/quantile/memory scope; heap differences are not exact allocation counts and initial baselines are not regression passes.

Processes are independent, but OS file caches are not guaranteed cold. First C# workers dynamically compile consumers; later workers reuse temporary disk compile caches. cold_ms excludes preparation, while whole-process time/RSS includes it. Interpret first-compile/later-run peaks separately, not as identical configuration-memory conditions. Runtime budget estimates do not replace measured heaps or configuration-exclusive RSS.
