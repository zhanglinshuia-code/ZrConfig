# Tests and change acceptance

[中文](README.md) | **English**

Run all commands from the repository root. Test projects are .NET console programs, not dotnet test adapters; actually execute DLLs or full smoke. See [development](../docs/development.en.md) for setup and [debugging](../docs/debugging.en.md) for breakpoints.

Public source ZIP includes this guide, ZrConfig.Server.Tests, and four shared server fixtures, supporting verify-source and the seven-language contract/consumer/performance commands below. Full smoke, UI, workbook rebuilds, and internal maintenance require the full workspace; those private sources are omitted from public ZIPs.

## Public server acceptance

These commands work after GitHub cloning/source extraction from the root. verify-source needs only .NET SDK 10; full seven-language acceptance additionally needs the toolchains below.

```bash
bash scripts/maintain.sh check-portability
bash scripts/maintain.sh verify-source
bash scripts/dotnet.sh run --project tests/ZrConfig.Server.Tests -c Release -p:NuGetAudit=false
```

Windows:

```powershell
powershell -File scripts/maintain.ps1 check-portability
powershell -File scripts/maintain.ps1 verify-source
dotnet run --project tests/ZrConfig.Server.Tests -c Release -p:NuGetAudit=false
```

| Toolchain | Minimum target | Optional path variable |
| --- | --- | --- |
| Java | JDK 17 | `ZRCONFIG_JAVAC` |
| Go | 1.19 | `ZRCONFIG_GO` |
| C++ | C++17 compiler | `ZRCONFIG_CXX` |
| Lua | 5.4, 64-bit integers and double precision | `ZRCONFIG_LUA` |
| TypeScript / Node | 5.8 / 22 | `ZRCONFIG_TSC` or `ZRCONFIG_TYPESCRIPT` (typescript.js), plus `ZRCONFIG_NODE` |
| Rust | 1.70 | `ZRCONFIG_RUSTC` |

Default entry requires all six native toolchains and runs C# plus six-language contracts/consumers. Append `-- --managed-only` to dotnet run for managed-only checks; it explicitly skips native compilation and is not seven-language acceptance. `-- --api-only` runs API checks alone. Existing builds can execute tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll directly.

Toolchain preflight reports each resolved path and identifies missing tools by environment variable and configured value. An invalid explicit path does not fall back to another version on PATH; setting `ZRCONFIG_TYPESCRIPT` also disables fallback to a global `tsc`. TypeScript 7 uses its native `tsc` executable (`ZRCONFIG_TSC`); 5.x/6.x continue to support `typescript.js` (`ZRCONFIG_TYPESCRIPT`). CI selects the entry point provided by the package and preserves `server-toolchains.log` with the selected paths and actual versions to diagnose minimum/current profile installation failures.

Git clones may use `-- --trusted-ref <trusted-commit-SHA>` to compare API baselines/migration chains against an immutable target. Full history for that commit is required; source ZIPs lack history and omit this check. Choose a real baseline, never the candidate itself.

[Public CI](../.github/workflows/ci.yml) defines minimum/current Windows/Linux/macOS matrices and retains logs. Matrix configuration, one local pass, and actual cross-platform passes are distinct. Current results are the commit's Actions checks. See [server performance acceptance](#seven-language-server-performance-acceptance).

On Windows, the C++ large-catalog checks enable `-Wa,-mbig-obj` for MinGW and `/bigobj` for MSVC so the 257-table generated consumer can exceed the ordinary COFF section limit. The ordinary executable verifies allocation-free queries. ASan/UBSan and TSan executables let the sanitizer own allocation interception while retaining the same query, lifecycle, and concurrency assertions; they neither redefine global `new/delete` nor report that run as an allocation measurement.

Full verification, individual/internal checks, and Unity maintenance below target the full workspace; public downloads omit those projects/smoke scripts.

## Full verification

```bash
bash scripts/maintain.sh check-layout
bash scripts/maintain.sh check-portability
bash scripts/maintain.sh check-style # Build first and generate the Unity solution
bash tests/smoke.sh
```

Native Windows PowerShell:

```powershell
powershell -File tests/smoke.ps1
```

PowerShell 7 uses `pwsh -File tests/smoke.ps1` on Windows/macOS. Only .NET SDK 10 is needed, without Python/Bash/WSL/diff. Both wrappers call tests/SmokeChecks.cs compiled by scripts/ZrConfig.Maintenance.csproj, keeping platform coverage aligned.

Smoke includes layout checks; the standalone command is a quick documentation/layout check. Full flow:

check-style discovers all projects and checks source/generated/template coverage, solution .editorconfig copies, warning-level style diagnostics, whitespace, and configuration text. --fix applies automatic fixes, followed by another check. Shared Assets use Unity context; standalone tests/fixtures are included. It requires a local Unity solution, so stays outside .NET-only smoke. See [conventions](../docs/development.en.md#c-conventions).

1. Create a three-table fixture in system temp with five headers/Excel keys; build CLI separately and run codegen to prove generation does not depend on project models/rules.
2. Compare four shared serialization-support files with template output.
3. Copy handwritten root source/Tables.json into temporary Config, excluding Generated/caches; regenerate from shared samples/Data and compare every official generated file.
4. Build ZrConfig.slnx, run Headless UI, then core integration without arguments.
5. Verify explicit Config with omitted Rules performs only basic checks, and external rules dynamically compile.
6. Copy standalone CLI outside the repository without project Models/Validators DLLs or Rules. Check rule changes, compile locations, unchanged current.json on failure, and recovery after removing bad rules.
7. Export nine official tables into temp: AllTypes 3, AllTypes10k 10000, Drop 3, SkillLevel 4, Hero 3, Skill 2, ItemCatalog 3, ShopOffers 3, QuestRewards 3; total 10024.
8. Rebuild all examples with C# in temp. Check five headers, existing-file protection, explicit overwrite; compare 103 fields/631 scalar-member values/9 post-error locations for rebuilt and official AllTypes. Check AllTypes10k's 10,000 rows, 106 fields, members, binary round-trip, row loading/references, plus cleanup, links, and packaging protection.

Smoke does not overwrite official Excel, Generated, or samples/Output. Builds update bin/obj; generation discovery uses only in-process compilation caches. Scripts remove their own temp workbooks/exports. Forced termination may leave temp directories; remove only after confirming no process uses them.

## Individual build and focused checks

```bash
bash scripts/dotnet.sh build tests/ZrConfig.Tests --disable-build-servers -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false -p:UsedAvaloniaProducts=
bash scripts/dotnet.sh tests/ZrConfig.Tests/bin/Debug/net10.0/ZrConfig.Tests.dll --rules-only
```

Replace the last argument with one option below. Select only one per run; matching branches return when complete. Multiple flags do not execute every suite.

| Argument | Main coverage |
| --- | --- |
| `--api-only` | Real public declarations, naming collisions, old consumers, data/description/regeneration stability, breaking-change no-writes, corrupt/missing/imported baselines, exact migration digests, stale protection; included in default |
| `--delivery-only` | Validation-only no-writes, stale schema, input rechecks, cancellation phases, two commits, version/log retention, field differences, compatible rollback, cleanup protection, CLI |
| No arguments | Core export, diagnostics/language, post-rules, publication protection, runtime, and several focused suites; UI separately |
| `--ui-only` | Headless UI, light/dark readability/theme changes, language refresh, project settings, generation confirmation/cancellation, invalid key markers, path drag/drop/restore |
| `--usability-only` | Plain/combined CLI export, shared cancellation/confirmation/stale previews, current Excel rereads, generation scope, support upgrades/handwritten protection, compile-source classification, official models/minimal documented Unity script; included in default |
| `--rules-only` | Lazy validation indexes, cross-rule reuse/run isolation/dependencies/locations/allocations; arbitrary recursive source discovery, renames and exclusions; preview cache content/language/concurrency/LRU/unload/read-only behavior; external rules, disk caches, Debug/PDB/source stacks |
| `--types-only` | Custom types, converters, polymorphism, codecs, allocations |
| `--extended-only` | Nullable values, dictionaries, rectangular arrays, determinism/boundaries |
| `--columns-only` | Split members, multi-row lists, nested sorting, locations |
| `--sheets-only` | Multiple sheets, names, skipped helper sheets |
| `--excel-input-only` | Recursive mixed XLS/XLSX, case, Chinese/spaced paths, hidden folders/sheets, locks, real binary/formula caches, multi-row/cross-format references, invariant culture, duplicates/corruption/failure protection |
| `--indexes-only` | Excel keys, custom groups, generated queries, including key-specific checks |
| `--excel-keys-only` | Same-sheet/nonadjacent/no-Id/nine-field keys, groups, missing/misspelled/mixed markers on first/repeated generation, old three-row rejection, workbook/source/release protection, conflicts, query allocations |
| `--targets-only` | C/S/CS generation/binary export, five headers, explicit CS, blank rejection, expanded-group inheritance, missing side/old layout rejection, keys/expanded markers, grouping after excluded lists, references, C# 9, project/CLI overrides, target-switch cancellation, locations/protection; included in default |
| `--references-only` | 10,000-table manifest without codecs/states, registry-free queries/references, segmented C# 9 catalog/lazy enumeration, per-table lazy singleton, on-demand schemas/rows/indexes/corruption rejection, unrelated rows, nonseekable output, empty catalog, three-table/10,000-row cycles, nulls, caches, concurrency/cancellation/retry, invalid targets/Required/dual routing/update invalidation |
| `--runtime-only` | Load/write allocations, built-in awaitable, sync/async consistency/routing, concurrency/cancellation/snapshots, reader reuse, coalescing/independent cancellation/timeouts, warmup/failure protection, background scheduling/commit races, diagnostics/Unity lifecycle |

Full smoke does not execute every --...-only branch. Run --types-only when custom types change. Final counts cover only some checks; suites have separate assertions. Judge successful exit and complete logs, not a historical count.

All-types field expectations have a separate entry: second argument is repository root, third is generated expectations JSON, not Config. In the local workspace use `samples all --output <temporary-Data> --expectations <temporary-JSON>`; both outputs belong in system temp, without overwriting official files:

```bash
bash scripts/dotnet.sh tests/ZrConfig.Tests/bin/Debug/net10.0/ZrConfig.Tests.dll --check-all-types "$PWD" "/system-temp/expectations.json"
```

Windows replaces the Bash wrapper with dotnet, uses `powershell -File scripts/maintain.ps1 check-layout`, and smoke.ps1 for full checks. Run maintenance alone via `bash scripts/maintain.sh check-maintenance` or PowerShell equivalent.

Usability covers complete CLI arguments, shared settings/project moves, generation checking only Data/Config versus export checking Output/Rules, A1 locations, read-only previews, support confirmation. Transactions cover one file/table, old three-file/.meta confirmation/staleness/per-operation rollback, main GUID/handwritten partial preservation, repeat-generation no-changes, identical binary before/after migration. UI covers generation with blank/file Output, first-export prompts, cancellation no-writes, data-only no-generation, and new-table confirmation/export.

## Test file ownership

| Project/script | Responsibility |
| --- | --- |
| ZrConfig.Server.Tests | Public seven-language contracts, frozen consumers, performance; links four fixtures |
| ZrConfig.Tests | Full-workspace core/runtime/UI/rule/specialized/performance probes; only four server fixtures public |
| ZrConfig.TestFixtures | Creates test-only three-table Excel in an explicitly temporary directory |
| ZrConfig.TestModels | Generates/compiles fixture models under obj |
| ZrConfig.TestRules | Test rules depending only on test models |
| samples/Projects | Compile-checks official Config/Rules via linked source |
| CheckLayout.cs | Official Excel only under samples/Data; local Markdown targets exist |
| SmokeChecks.cs / MaintenanceChecks.cs | Shared smoke, temporary rebuild/field comparison, overwrite protection, cleanup, packaging |
| Performance.cs | Separate Release process measurements/JSON aggregation |

ExcelInputTests uses test-only NPOI to create real BIFF/OLE .xls in temp, not renamed .xlsx or official Data. Default integration invokes it. Unix also tests cyclic directory links; Windows runs other assertions without link-creation privileges.

check-layout does not validate heading anchors, remote URLs, compilation, or command behavior. check-portability separately checks package-local links against distributed files. Documentation work must inspect anchors, fences, and tables. For tutorial source changes, compare chapter 12's seven complete files with the attachment and generate/export in temp. Usability extracts/compiles section 14.4's Unity script. Record actual date/results/untested scope in local docs/project-status.md; layout success alone does not prove all documentation correct.

Runtime checks cover awaitable completion/registration races, multiple suspensions, exceptions/cancellation, execution-context isolation, sync/async routing, independent cancellation, and reentrant reload rejection. Unity lifecycle links real adapters but uses test Unity API stubs and a pumpable context to check read/await/cancel/Changed threads. Reference checks simulate coalescing/cancellation/retry. They launch no Unity Editor/Player and do not certify third-party AB, Odin, or IL2CPP integration.

Lifecycle regressions hold context restoration after background completion, checking row/table rejection after reload/cancel; pause old codec metadata validation to prove new Current remains usable during cleanup waits; reload/reset inside Changed to prevent reversed notifications; dispose/reset during manifest/prewarm waits to block late publication.

Loading-GC regressions: --runtime-only covers missing/duplicate/zero Id semantics, 10,000-row reference-index budgets, async Unity argument errors/pre-cancellation, and lazy current-codec catalogs. --indexes-only covers sync full-table token overloads, original parameterless method groups, cold/warm cancellation, and allocation-free typed queries. See local docs/performance-and-style.md for measurements.

## Unity sample verification

Prepare using [unity-sample](../samples/README.en.md#runnable-unity-sample). Full smoke also checks desktop deployment hashes, repeated preparation, failed-manifest protection, path/link rejection, historical retention, source packaging, and unrelated user-file protection. Generated comparisons ignore Unity .meta.

For real Editor verification, set UNITY_EDITOR to the matching Hub Editor's absolute executable path; installation paths vary:

```bash
"$UNITY_EDITOR" -batchmode -nographics -quit -projectPath "$PWD/unity/ZrConfig.Sample" -executeMethod ZrConfig.Sample.Editor.SampleProject.VerifyData -logFile /tmp/zrconfig-unity-verify.log
```

Use this asynchronous AB dual-entry check without -quit; it exits when done:

```bash
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD/unity/ZrConfig.Sample" -executeMethod ZrConfig.Sample.Editor.SampleProject.VerifyAbLoading -logFile /tmp/zrconfig-unity-ab-verify.log
```

It checks real synchronous/asynchronous reads of shared Output/StreamingAssets, zero-row manifests, on-demand references, mode-shared caches, groups/composite keys, all-table decoding, and old-reference invalidation. Non-Play UI regression deliberately delays Changed after publication, checking late notifications, eight async reloads, stable display, and old-data retention on failure. Runtime tests link the same SampleAbResources.cs for temp-export read retries/cancellation/failed reload, without writing official Output.

Maintenance verifies source packages include Assets/.meta/Packages/ProjectSettings/Excel but exclude caches; extracts to Chinese/spaced paths; checks overwrite/path-escape protection, lossless metadata cleanup, and dependency-license upgrades.

Close the interactive Editor before batch checks. Manually Play: initially 0 rows; hero/skill queries load rows/references, null references avoid target loads, drop groups load full tables. Switch to sync, reload, and confirm sync logs; repeat queries show 0 reads. Re-export/reload updates the version; exiting/reentering Play reloads correctly. VerifyData checks compilation graphs: Runtime/Validation/adapters/configuration from Assets; Validation, Unity.Editor, and rules excluded from Players; old ZrConfig DLLs rejected. Batch success is not actual Player, IL2CPP, or code-hot-update acceptance.

## Acceptance checklist after changes

- Documentation/layout: update navigation, verify links/anchors, place files by purpose.
- Tables/generator: deterministic full generation, compilable models, nine-table export, one registry/source set.
- Rules: valid data passes; boundaries/blanks/duplicates fail as intended at physical cells; warnings do not block incorrectly.
- Cross-table rules: incomplete targets skip dependent checks while independent local checks run; duplicate keys cause no unrelated exception.
- Publication: failed current.json bytes remain identical; successful manifests reference complete files with matching counts/hashes.
- Loading: manifest-only default; row requests expand actual references; failure/cancellation publishes no partial result and remains retryable. ReloadAll preserves full validation/atomic switching. Successful updates invalidate old queries/references; reacquire objects.
- Debugging: actually test entry/dynamic-rule breakpoints, exceptions, and locations in the target IDE; explicitly record if untested.
- Unity: verify initial load/reload on the actual version, platform, resource adapter, assembly/hot-update setup.

## Performance checks

`--loading-probe <repository-root> <JSON-report>` reads shared Output and measures first row, sequential decoding, full table, concurrency/coalescing, background processing, row warmup, reload, and warm queries. Commands/.NET/Unity methodology are in local docs/performance-and-style.md. Source is LoadingPerformanceTests.cs; no temporary official Data files.

```bash
bash scripts/maintain.sh performance
```

Windows uses `powershell -File scripts/maintain.ps1 performance`. --native uses real windows; default Headless. Maintenance serially runs separate Release processes, up to 300 seconds each: 1,000/10,000/50,000 log rows, 10,000/100,000 exported records, then real diagnostic-log playback. artifacts/performance/latest is replaced each run; summary.json records machine/mode/median/UI queue maximum delay/memory. Archive baselines first. Documentation-only changes need no performance rerun; performance changes/new claims do.

Distinguish total/current-thread allocations, live memory, and elapsed time, recording warmup/scale/environment. Warm typed queries at 0 B do not imply allocation-free import/loading/index construction. .NET 10 does not represent Unity/IL2CPP. See local performance documentation.

`--diagnostic-probe <count> <new-report.json>` measures actual MainWindow text, result fill, language/search/tab changes, and production FlushEvents. Counts 1–50,000; live batches at most 10,000. Build Release first. Log documents are generated on demand and their initial cost belongs in SwitchToLog; faster initial display does not prove every operation faster. Old --perf-log measures only ColoredLogView, not the whole area. Historical b8f4607/current-text comparisons are local.

--ui-only checks no per-diagnostic controls, visible-line layout, 10,000 unordered/multiline appends, sorting/filtering/search, real Ctrl/Cmd mouse multiselection, document-start clicks, cross-line copying/navigation, language refresh without body rebuild, hidden-log restore/cleanup, selection retention, and disabled undo. Unchanged repeated refresh asserts 0 B; large append is capped at 256 KiB to catch whole-string copies. This says nothing about new logs/later layout allocations.

--allocation-probe also covers three 100,000-row group distributions, 1,000,000 indexed reference resolutions, and 100,000-row numeric validation. Prepare input outside timing, warm up 3 times, sample 7 times, use Release and matching JIT settings. --rules-only retains null/float/long/duplicate/reference/unload checks; --runtime-only checks reference-cache zero allocation, missing targets, failed index caches, and read-only groups.

Usability/full smoke test generation transactions across header/key/side/type/Tables.json/workbook-list changes, data-only exceptions, concurrent locks, and per-operation partial-write/delete failures, requiring support/generated/metadata rollback together. Also test first-generation cleanup, .meta protection, and locatable rollback-failure backups. File-operation injection is transaction-local.

Rule compilation tests occupied/unreadable/unwritable/corrupt caches, save/cleanup faults, warnings with continued valid execution, invalid source blocking publication, and cache recovery/hits. Fault injection is per preparation, not global compiler state.

--indexes-only covers shared single-key indexes for ints/strings/enums/comparers, duplicates, empty tables, read-only dictionaries, object/group reuse, concurrency, explicit `List<T>`.Enumerator, and 100,000 allocation-free query rounds. --index-sharing-probe compares old dual/new shared indexes over 10,000 prebuilt rows. Use Release and unique report IDs; local performance records define methodology/results.

--runtime-only pauses after real reader creation/row decoding, then cancels/reloads/disposes to test late-result rejection, clean caches/statistics, and retry. Full-table faults preserve existing rows, publish no partial table, and reuse objects on retry. Earlier commit-window/cleanup/new-Current/context/notification tests remain; --references-only covers binding/lifecycle.

Batches 4/5 use real Unity SampleProject.VerifyAbLoading plus VerifyData on Output/StreamingAssets for data, sync/async file AB, references, groups, caches, reload. Async batch omits -quit and self-exits. Record Editor and Player/IL2CPP separately in local project status; Editor success is not target acceptance.

## Multi-language and JSON acceptance

P0 tests all six native runtimes reject extra Ghost/missing/replaced equal-count tables before content reads and retain Current, for Binary/JSON. _Item actually compiles in Go. Description-only changes need no API migration; removing enabled languages requires separate approval. Corrupt-source/missing-baseline tests require restoration, never ordinary regeneration overwriting protected APIs.

--portable-only checks generation/settings/Binary/JSON/numeric boundaries/corruption/reload/C# allocations. --portable-native requires Java/Go/C++/Lua/TypeScript/Rust compilation/execution and snapshot/query behavior. Set ZRCONFIG_JAVAC/GO/CXX/NODE/LUA/RUSTC, and ZRCONFIG_TSC or ZRCONFIG_TYPESCRIPT for TypeScript. Minimums: Java 17, Go 1.19, C++17, Lua 5.4, Node 22 / TypeScript 5.8, Rust 1.70. TS checks strict compilation/hot-query heap growth; Lua pauses GC; Rust counts allocator calls. All fixtures/outputs are temporary; official Excel stays unchanged.

A 257-table boundary fixture generates old/server-v1 APIs and runs cross-segment bidirectional references, four modes, table/typed-row/group warmup, and string composite keys. Java checks inherited fields/nested types/static imports/method references; Go empty-snapshot zero values; C++ single-header cross-table visibility/owned warmup keys; Rust root size/later-segment failure retention/handle release; TypeScript sync/async; TS/Lua row-handle retention/final reclamation. C# combines timer cancellation with future monotonic checkpoints to distinguish timeout/caller/manager reasons without timing-window races.

Rust runs five private scheduling-fault tests in temporary runtime copies: independent row/group waiter cancellation, decode-panic wake/retry, permanent-error caching/transient retry, index-budget rollback, and single publication. Channels/waiter counts control interleavings; actual generated files remain unchanged. Minimum/current toolchains compile with -D warnings.

## Seven-language server performance acceptance

Public tests/ZrConfig.Server.Tests also measures performance in separate processes. Set Java/Go/C++/Lua/Rust/TypeScript/Node ZRCONFIG_* paths as above, then run:

```bash
bash scripts/dotnet.sh build tests/ZrConfig.Server.Tests -c Release -p:NuGetAudit=false
bash scripts/dotnet.sh tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll --benchmark artifacts/performance/latest 10000 3
```

Last arguments are primary-table row count and independent repetitions; supported counts: 100/1000/10000/100000. Inputs/source/builds use system temp; report is server-runtime.json in the destination. Fixed data includes narrow tables, 24-field wide tables, long strings, nested collections, high/low-cardinality groups, and a 64-row reference cycle. Each run compares Binary/JSON, full/selected closure, strict/on-demand row modes, and 1%/10%/100% access. Samples start independently, warm queries warm separately, and host verifies checksums.

macOS/Linux may set ZRCONFIG_BENCH_TIME=1 for /usr/bin/time child peak memory, requiring local process-stat access. Unavailable values are null, not zero. ZRCONFIG_BENCH_CPU/MEMORY_BYTES record verified hardware. Peaks include entire process/startup; C# workers include dynamic consumer compilation, not configuration-only memory. Query quantiles distribute means of 100-operation batches, not individual tail latency. Lua uses CPU time. Node event-loop delay measures the whole synchronous batch blockage, not an async scheduler promise. Reports define each allocation/heap metric.

First runs establish baselines. Apply the proposed 10% regression threshold only after repeated same-hardware/toolchain/scale/configuration comparisons. This does not replace corruption, concurrency, lifecycle, compatibility, or cross-platform acceptance.

Public contracts include C# randomized checks with fixed xorshift32 seeds 1729, 104729, 305419896, 2654435769: 96 barrier-controlled prepare/cancel/Clear/Close/commit/old-reader interleavings and 256 random Binary/JSON bit corruptions. Failures record seed/round/action/byte. Timeouts detect hangs, not race outcomes. This supplements other-language barriers/corruption, not exhaustive proof.

Additional entries: --benchmark-samples <report-dir> <repository-root> 3 copies nine tables into temp; --benchmark-retained <report-dir> 3 measures 0/1/4 retained snapshots; --benchmark-scenarios <report-dir> 3 measures controlled source delay, second-read failure, and 1/4 warm readers where supported; --benchmark-queries <report-dir> 3 measures key hit/miss, composite/group/reference/acquisition latency/allocations. See [methodology](../docs/server-generation.en.md#repeatable-performance-measurements). Frozen official consumers use --sample-consumer <language\|all> <Config-dir> <Output-dir> as a real migration-receipt command.

Stage/manager breakdown commands (use separate report directories to preserve baselines):

```bash
bash scripts/dotnet.sh tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll --benchmark-stages artifacts/performance/stages "$PWD" 3
bash scripts/dotnet.sh tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll --benchmark-control artifacts/performance/control 3
bash scripts/dotnet.sh tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll --benchmark-locks artifacts/performance/locks "$PWD" 3
```

Large-catalog business generation/loading uses a separate entry; graph benchmarks are not a substitute:

Large TypeScript compilation may set ZRCONFIG_TYPESCRIPT_HEAP_MB=16384, range 64–65536 MiB. Only compiler processes change; measurement processes retain Node settings. Unset uses Node's default heap. Actual compiler options/peaks are recorded separately.

```bash
ZRCONFIG_BENCH_TIME=1 ZRCONFIG_BENCH_TIMEOUT_SECONDS=3600 bash scripts/dotnet.sh tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll --benchmark-table-scale artifacts/performance/table-scale 10000 3
```

Table counts 100/1000/10000; final argument is independent process repetitions per language/format/loading setup. Inputs contain 100-row primary/wide tables, a 64-row cycle, and single-row small tables. Generate/export both formats/validate, regenerate and verify unchanged file lists/content/timestamps, then compile/run seven-language consumers. Full mode checks total loaded rows across all tables. Reports record source/API-baseline/build sizes. Large-catalog compilation time/memory can greatly exceed runtime loading; budget accordingly.

Go benchmark consumers use temporary module zrconfig, recording its declaration. This avoids temporary-path-dependent package names and repeated host paths in generic symbols. Production generation follows the project's module convention. Compiler objects, executables, and loaded memory are separate metrics.

ZRCONFIG_BENCH_TIMEOUT_SECONDS adjusts each compile/run subprocess from default 300 seconds to 1–3600 and is recorded. Timeout remains failure. ZRCONFIG_BENCH_KEEP_SCALE=1 preserves the temporary failed workspace. ZRCONFIG_BENCH_TABLE_WORKBOOK can reuse this entry's same-scale TinyTables.xlsx; input digests/full-row assertions still run. Success/failure files are server-runtime.json/failure.json. Use different directories per attempt to preserve evidence.

Instrument only temporary copies, never official Runtime/workbooks/generated code. Nine-stage timings include child calls. Lock sampling measures manager-state lock acquisition including uncontended/timer overhead. First initialization records catalog access/module imports per language. Graph benchmarks measure only Catalog/SCC/closure. Cancellation triggers deterministically on the second source read, not worst-case response at arbitrary CPU hotspots. See [measurement boundaries](../docs/server-generation.en.md#repeatable-performance-measurements).
