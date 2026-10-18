# Sample entry point

[中文](README.md) | **English**

All official Excel files live under [Data](Data), optionally grouped into subdirectories. The tool recursively reads `.xlsx`/`.xls`. Every sample shares configuration source under Unity Assets, Editor checks, and this directory's Output. Business types/rules are ready to use; everyday operation requires neither C# nor serialization code nor workbook rebuilding.

## Which workbook to open

| Workbook | Demonstrates | Exported logical tables |
| --- | --- | --- |
| [AllTypes.xlsx](Data/AllTypes.xlsx) | Primitive values, enums, arrays, objects, polymorphism, dictionaries, and multi-row lists | AllTypes |
| [AllTypes10k.xlsx](Data/AllTypes10k.xlsx) | 10,000 all-types records, cross-table references, and same-table chains; loading benchmark input | AllTypes10k |
| [CompositeKeys.xlsx](Data/CompositeKeys.xlsx) | Two-/three-component keys marked on the same sheet | Drop、SkillLevel |
| [TableReferences.xlsx](Data/TableReferences.xlsx) | Bidirectional/self references, shared targets, and null references | Hero、Skill |
| [ItemCatalog.xlsx](Data/ItemCatalog.xlsx) | Item names, stacks, quality, enabled state, and reusable field checks | ItemCatalog |
| [ShopOffers.xlsx](Data/ShopOffers.xlsx) | Price, purchase limits, duplicate offers, and item state; table/cross-table checks | ShopOffers |
| [QuestRewards.xlsx](Data/QuestRewards.xlsx) | Reward quantity, unlock level, item stacks, and disabled quests | QuestRewards |

Helper sheets such as `#说明` (Instructions) and `#类型索引` (Type index) are not exported. Excel `~$` lock files, macOS `._` files, and linked files/directories are skipped. Hidden business sheets and real hidden directories are still read; keep backups outside Data at every level. Subdirectories do not namespace table names, which must remain unique across directories and formats.

## Use in the App

Click “Open project configuration” and select [project.zrconfig.json](project.zrconfig.json) to restore all paths, or set them individually with “Browse”:

The shared project generates all seven languages and the separate `server-v1` API, exporting Binary + JSON. Its target is still C. Server API loading/lifecycle features do not change Excel C/S filtering. When switching to S, use matching generated code and data. Existing C#, Java, and other query APIs remain; see [new server runtime entry points](../docs/server-generation.en.md#server-contract-families-and-explicit-loading).

| Input | Path |
| --- | --- |
| Excel input directory | samples/Data |
| Output directory | samples/Output |
| Configuration code root | unity/ZrConfig.Sample/Assets/HotUpdate/Config |
| Optional rules directory | unity/ZrConfig.Sample/Assets/Editor/ConfigRules |

Save Excel and click “Validate and export”. If generation is requested, review paths/changes and confirm. Select the complete Data and Config roots. See the [user guide](../docs/user-guide.en.md#31-when-to-use-each-button) for buttons, cancellation, and errors, and the [CLI tutorial](../docs/project-development.en.md#13-command-line-usage) for automation.

New tables follow the [five-row header](../docs/user-guide.en.md#41-always-use-five-header-rows): field, type, primary key, description, target side, then data from row 6. Single and composite keys use the same layout.

`AllTypes.xlsx` uses the same five rows as every official table, with data from row 6. Id is marked `主键` on row 3. Shared [Tables.json](../unity/ZrConfig.Sample/Assets/HotUpdate/Config/Tables.json) retains only the custom ByQuality group over QualityValue.

Official samples retain row 5 for target sides, explicitly using CS for shared fields. Missing C/S/CS is an error; the App defaults to C. See [header and side markers](../docs/config-types.en.md#client-and-server-fields). Save and close old workbooks before inserting row 5 and explicitly filling sides. Move data to row 6, check frozen headers/dropdowns/navigation, then regenerate/export. Do not overwrite original data with rebuild commands.

## What to check after success

Unmodified official samples should succeed with 0 errors, 4 expected warnings, 6 informational diagnostics, and 12 success diagnostics, excluding tool process logs. current.json contains these logical record counts:

| Table | Records |
| --- | ---: |
| AllTypes | 3 |
| AllTypes10k | 10,000 |
| Drop | 3 |
| SkillLevel | 4 |
| Hero | 3 |
| Skill | 2 |
| ItemCatalog | 3 |
| ShopOffers | 3 |
| QuestRewards | 3 |
| Total | 10,024 |

AllTypes multi-row actions merge into logical records, so physical Excel rows differ from record counts. Deliver current.json and all referenced `.bytes`/`.json` table data with relative version paths preserved; the shared dual-format manifest references both. Management records, `.rows.json` snapshots, and diagnostics stay with the tool. Judge success by the current result/manifest, not merely by files already present in Output.

## 10,000-row all-types loading case

`AllTypes10k.xlsx` uses five header rows, with data on rows 6–10005 and Ids 1–10000. It reuses AllTypes' 103 logical fields: primitives, enums, vectors, kv, Money converter, objects, polymorphism, jagged arrays, nullable values, dictionaries, rectangular arrays, and raw JSON. Objects/nested lists use single-cell `json<T>` but retain their declared strong output types. Each record occupies one row; the original AllTypes split-column/multi-row teaching layout is preserved.

`(Id-1)%3` selects normal/boundary/default samples for 0/1/2, totaling 3334/3333/3333 records. Added `Hero`, `Skill`, and `Previous` references bring the total to 106 fields. Default samples have null cross-table references; Previous points to the preceding row, with the first row self-referencing. Deterministic generation enables repeatable mixed-type measurement, not a claim about every business table's distribution.

This workbook is included publicly. Run `unity-sample` below to generate, export, and deploy without rebuilding Excel. All workbooks together export nine tables and 10,024 records. Reading only the manifest or querying other tables does not decode this table.

Observe read logs and LoadedRowCount in Unity, or run the AssetBundle verification entry under [public source verification](../docs/development.en.md#verify-public-source). On your target platform, measure first-row loading, full-table loading, cache hits, and reload separately, distinguishing resource I/O from decoding. See [warmup and diagnostics](../docs/project-development.en.md#prewarming-and-diagnostics).

## Primary-key and object-reference examples

CompositeKeys.xlsx marks `主键` on row 3: SkillLevel uses the complete SkillId/Level/Quality combination, and Drop uses GroupId/Id. Descriptions are on row 4, data from row 6. Individual components may repeat; complete keys must not. See [key input](../docs/table-indexes.en.md#how-to-fill-the-workbook).

In TableReferences.xlsx, Hero.Skill has type SkillConfig and stores a skill Id; Skill.Hero has type HeroConfig and stores a hero Id. Hero 1 references skill 10, which references hero 1 back. Mentor shows self-reference; OtherSkill shares a target. Hero 3 uses 0/blank for no reference. Target Ids must be nonzero and unique; a missing nonzero Id blocks export.

The tool links objects. Developers can query complete keys, groups, and linked objects using the [query API](../docs/table-indexes.en.md#generation-and-usage) and [Unity integration](../docs/project-development.en.md#14-unity-runtime-integration).

Post-validation can query keys/groups through generated accessors such as `context.GetSkillLevelTable()`. The first access builds indexes shared by all rules in that export. Existing references still use `row.Item` or `row.Skill` directly, without another lookup. See [validation index tables](../docs/rules-development.en.md#use-indexed-tables-in-post-validation) for calls and dependencies.

AllTypesRules checks target counts, interval ordering, action delay, ratios, and buff stacks. Keys/direct references also receive basic integrity checks. See the [all-types guide](../docs/type-examples.en.md) for input formats.

## Post-validation and four diagnostic levels

The three new workbooks are used together with [ConfigRules](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules), using the current App's `Info` / `Success` diagnostics. Assets/ZrConfig/Validation supplies the interfaces. Each workbook's `#说明` sheet lists cells to change, expected codes, and restoration steps.

| Level | Default example | Blocks export? |
| --- | --- | --- |
| Info | Disabled items, free offers, disabled quests, and counts for three tables | No |
| Success | A name-format, item-parameter, offer-parameter, or quest-reward check passed | No |
| Warning | ItemCatalog!C6 large stack; ShopOffers!D7 high purchase limit; QuestRewards!B6 surrounding spaces and D6 split-stack requirement | No |
| Error | Negative price, invalid quantity, duplicate active offer, or enabled offer/quest referencing a disabled item | Yes; retains the last valid version |

- [SampleNameRule.cs](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/SampleNameRule.cs): Reusable field check bound by `[Check]` for items/quests; `[Required]` checks empty cells.
- [ItemCatalogRules.cs](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/ItemCatalogRules.cs): Row checks for stacks/quality and a table-level enabled-item count.
- [ShopOfferRules.cs](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/ShopOfferRules.cs): Row price/limit checks, cross-table item-state checks, and table-level duplicate offer checks.
- [QuestRewardRules.cs](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/QuestRewardRules.cs): Compares rewards with item stack limits across tables; disabled quests still receive basic parameter checks.

A Success diagnostic only means its specific check passed; the export result determines publication. All four levels retain file, sheet, coordinates, field, and code, and are written to run logs and `Logs/diagnostics.json`. For error exercises, copy the complete Data set to a system temporary directory first. See [diagnostic levels](../docs/rules-development.en.md#diagnostic-levels).

## Runnable Unity sample

[unity/ZrConfig.Sample](../unity/ZrConfig.Sample) uses Unity **6000.3.26f1** (Unity 6), with Editor, desktop Player, and Android/iOS/WebGL read entry points. Actual platform coverage is in [acceptance records](../docs/release-and-acceptance.en.md#acceptance-and-team-trial-records). It reads this directory's Data/Output; shared configuration and post-validation source live directly in its Assets.

### First run

Clone or extract the complete repository, including Unity, into any writable directory. Spaces, Chinese characters, and different drive letters are supported; preserve internal layout. Install .NET SDK 10 and run from the repository root:

```bash
bash scripts/maintain.sh unity-sample
```

Windows:

```powershell
powershell -File scripts/maintain.ps1 unity-sample
```

A fresh checkout normally has no Library, artifacts, or Output. Framework source is already in Assets; prepare data before first use. Preparation accesses NuGet; Unity's initial dependency import accesses Unity package sources.

The command reads existing workbooks, generates as needed, validates/exports to samples/Output, then deploys the current manifest's data for Players. All ZrConfig source compiles directly under Assets, with no generic package build/install. Framework and configuration share Assets source. Excel files are neither rebuilt nor overwritten, and historical releases are retained.

1. Add `unity/ZrConfig.Sample` in Unity Hub, preserving its repository-relative location.
2. Wait for import/compilation, then open `Assets/ZrConfigSample.unity`, or use `Tools → ZrConfig → 打开示例场景` (Open sample scene).
3. Press Play. The Game window shows the AssetBundle demo, mode switch, version, queries, and resource reads. Startup reads only the nine-table manifest; loaded rows start at 0.

Unmodified data shows hero 1 as “火焰法师” (Flame mage), skill “火球” (Fireball), and self as mentor. Hero 3 has null references; drop group 10 has two rows; skill key `(101, 1, 2)` has Damage 150. Try nonexistent keys for empty results.

Select `ZrConfig Sample` to inspect loaded tables, or open `Tools → ZrConfig → 运行时配置表` (Runtime tables). “验证示例数据” (Validate sample data) checks shared/desktop loading, key/group queries, references, reload, and separation of models from Editor rules. “验证 AB 同步与异步” (Validate synchronous/asynchronous AB) checks real reads, references, caches, and reload. “验证示例程序集” (Validate sample assemblies) checks assembly separation independently.

### Synchronous and asynchronous AssetBundle sample

Editor/desktop calls follow `ZrConfigSample → UnityConfigRuntime / generated query API → DelegateSyncConfigSource → SampleAbResources → real files`. No third-party SDK or native AssetBundle is used yet. Temporary resource code replaces only the underlying AB read; manifest validation, binary decoding, references, and caches are real.

1. Enter in default asynchronous mode. Reads should contain only `ZrConfig/Common/current.json`; 0 rows loaded.
2. Click “Query hero” to see asynchronous Hero.bytes/Skill.bytes requests, names, mentor, and reverse reference. Query the same hero again: 0 resource reads for that action.
3. Switch to “Synchronous mode”, click “Reload synchronously”, then query. Requests now use synchronous reads. Switching modes alone retains caches, so already loaded objects cause no new request.
4. Use “Null-reference example”, “Query group”, and “Query skill” for null references, full drop-table grouping, and three-component keys. Both modes support them; missing keys show no configuration found.
5. To demonstrate synchronous initialization, exit Play, select `ZrConfig Sample`, disable `Use Async`, and Play again. Synchronous reads occupy the current thread; decoding after asynchronous reads is not automatically split across frames.

[SampleAbResources.cs](../unity/ZrConfig.Sample/Assets/Scripts/SampleAbResources.cs) is the sole temporary AB replacement point. `LoadBytes` calls `FileConfigSource.Read`; `LoadBytesAsync` performs real asynchronous file I/O, without fake async or hardcoded sample data. `ZrConfig/` maps to the current data root; `Common/current.json` and the `Tables/bytes` or `Tables/json` relative paths remain intact. The Game window retains 14 recent entries; Console also logs requests, completion, failure, and byte counts.

For production AB, replace reads in these two methods and return `byte[]`; business entry points stay the same. Handle address mapping, download readiness, request coalescing, independent caller cancellation, and resource-handle release according to the framework. The temporary implementation adds no resource cache and allocates an independent managed array per read; snapshots retain loaded bytes/objects. Production bridge bytes must likewise remain valid and immutable throughout snapshot use.

Android/iOS/WebGL Players automatically use `CoalescingConfigSource → StreamingAssetsConfigSource → UnityWebRequest`, always asynchronously. Synchronous switching is only for Editor/desktop demos. Mobile/browser still perform real decoding, references, and queries without a third-party SDK.

### Reload after export

The Editor reads `samples/Output/CSharp` directly. During Play, save existing Excel files and “Validate and export” in the App; on success, click “Reload” in Game. Alternatively run from the repository root:

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export --project samples/project.zrconfig.json
```

Scene entry calls `UnityConfigRuntime.Load(source)` / `LoadAsync(source)` to load only the manifest, without creating table codecs/cache containers. Generated queries use each table's `GetCodec()` and load rows/references in the current mode; drop-group queries explicitly load the full drop table. UI shows loaded rows and actual reads. Manifest failure preserves the old snapshot; row failure caches no partial result and can be retried. Successful reload invalidates old snapshots/row references; the scene adopts UnityConfigRuntime.Current and clears displayed results, even if Changed is delayed. Display data is built during Layout; button actions run afterward. Scene exit cancels loading and unsubscribes configuration/resource notifications. See [loading APIs](../docs/project-development.en.md#synchronous-loading-and-asynchronous-loading-without-task). Production AB updates must prepare all resources and ensure the next manifest read is fresh before reload. Manifest success does not validate every table; tables are checked on query. For schema/type changes, exit Play, regenerate/export, wait for Unity import/compilation, then Play. Business-source-only edits need no Models DLL copying/deployment.

### Configuration source and Editor post-validation

Source is physically under Unity Assets for direct browsing, editing, and debugging. Paths use English. App/CLI and .NET check projects share this single source set:

| Content | Sole repository location | Unity location | Assembly |
| --- | --- | --- | --- |
| Runtime loading, snapshots, and codec interfaces | [ZrConfig/Runtime](../unity/ZrConfig.Sample/Assets/ZrConfig/Runtime) | Assets/ZrConfig/Runtime | ZrConfig.Runtime |
| Validation framework | [ZrConfig/Validation](../unity/ZrConfig.Sample/Assets/ZrConfig/Validation) | Assets/ZrConfig/Validation | ZrConfig.Validation (Editor only) |
| Unity loading and viewer | [ZrConfig/Unity](../unity/ZrConfig.Sample/Assets/ZrConfig/Unity) | Assets/ZrConfig/Unity/Runtime、Editor | ZrConfig.Unity、ZrConfig.Unity.Editor |
| Generated models, codecs, query tables, and TableCodecs | [Config/Generated](../unity/ZrConfig.Sample/Assets/HotUpdate/Config/Generated) | Assets/HotUpdate/Config/Generated | ZrConfig.Models |
| Handwritten types and serialization/converters | [Config/CustomTypes](../unity/ZrConfig.Sample/Assets/HotUpdate/Config/CustomTypes)、[Config/Serialization](../unity/ZrConfig.Sample/Assets/HotUpdate/Config/Serialization) | Assets/HotUpdate/Config/CustomTypes、Serialization | ZrConfig.Models |
| Post-validation | [ConfigRules](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules) | Assets/Editor/ConfigRules | ZrConfig.Sample.Rules.Editor |

The [configuration asmdef](../unity/ZrConfig.Sample/Assets/HotUpdate/Config/ZrConfig.Models.asmdef) compiles all configuration source into an independent assembly referencing only Runtime. The [rules asmdef](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/ZrConfig.Sample.Rules.Editor.asmdef) references models, Runtime, and Validation, is Editor-only, and never enters Players. The generator root is Assets/HotUpdate/Config; output goes directly into Generated without copies/package updates.

Put handwritten extensions in CustomTypes/Serialization. Edit ConfigRules directly at any time. Generated is overwritten on generation; never store handwritten logic there. After schema edits, exit Play, regenerate/export, and wait for Unity compilation.

These directories are repository conventions. App/CLI recursively discover handwritten C# from the selected code root, without filtering by names such as CustomTypes. Root files and arbitrary subdirectories work. Generation excludes root Generated; export compiles it. Both skip bin, obj, dot-prefixed entries, and links.

No specific hot-update framework is bound yet. Once chosen, include **ZrConfig.Models** in its hot-update assembly settings and follow its DLL build/load order, AOT metadata, and business-assembly reference requirements. The sample provides independent source assemblies and data reload, not runtime C# assembly replacement.

Rules can compile, be viewed, and edited in Unity; export still executes the same pure C# source through App/CLI. See [rule development](../docs/rules-development.en.md) for sequencing/debugging processes. Do not wrap shared rule classes in `#if UNITY_EDITOR`; directories/asmdef define their Editor scope. Put UnityEditor button code in Assets/Editor, separate from pure C# rules.

Regenerate Unity IDE projects to see separate Runtime, Validation, Unity, Models, and rules projects. Double-click scripts to edit original files. The App project does not add virtual Source links.

### Desktop builds and file ownership

Build Profiles / Build Settings includes the scene. Choose a desktop target and build under `artifacts`. Desktop Players read prepared Assets/StreamingAssets/ZrConfig data through the temporary AB bridge. Exit Play and rerun `unity-sample` before building to refresh bundled data; Players do not read repository Output. Android/iOS/WebGL Players automatically use the built-in asynchronous StreamingAssets adapter. For third-party resource SDKs, implement the corresponding bridge following [resource-source integration](../docs/project-development.en.md#14-unity-runtime-integration).

- [ZrConfigSample.cs](../unity/ZrConfig.Sample/Assets/Scripts/ZrConfigSample.cs): Loading, lifecycle, queries, and runtime UI.
- [SampleAbResources.cs](../unity/ZrConfig.Sample/Assets/Scripts/SampleAbResources.cs): Temporary synchronous/asynchronous byte reads/logs; replacement point for the production SDK.
- [SampleProject.cs](../unity/ZrConfig.Sample/Assets/Editor/SampleProject.cs): Editor menus for opening the scene and validating data.
- [UnitySample.cs](../scripts/UnitySample.cs): Shared .NET preparation implementation used by Bash/PowerShell.
- Configuration/rules live directly in `unity/ZrConfig.Sample/Assets/HotUpdate/Config` and `unity/ZrConfig.Sample/Assets/Editor/ConfigRules`; Unity output is in Library/ScriptAssemblies. Framework source is solely in `Assets/ZrConfig`, without old artifacts packages or precompiled ZrConfig DLLs. StreamingAssets is Player deployment, not another export tree; preparation can rebuild its data.

## Use in your own project

Use `UnityConfigRuntime.Load / LoadAsync` initially, snapshot queries, and `Reload / ReloadAsync` for updates. Bind real synchronous/asynchronous functions through `DelegateSyncConfigSource` for third-party AB. Until a framework is chosen, do not add a specific SDK/native AssetBundle dependency. See [AB integration](../docs/project-development.en.md#third-party-ab-frameworks-synchronous-and-asynchronous-loading). Cache checks use explicit names such as `TryGetCachedSkill`.

Official sample types, generated code, and rules are in the Assets directories above. Follow [project development](../docs/project-development.en.md) and [rule development](../docs/rules-development.en.md) to integrate your project. Regenerate/export after schema changes.

The standalone exercise uses different Hero/Skill teaching fields and must stay outside official Data. See [chapter 12](../docs/project-development.en.md#12-complete-exercise-project).

See [row loading and AB adaptation](../docs/project-development.en.md#lazy-row-loading-and-cross-table-references). Current source uses the v4 row-index format; regenerate/export with this tool version and update Assets Runtime source before running samples.

## Validation-only mode and release history

Open shared `samples/project.zrconfig.json` and run “Validate only” without modifying shared Config/Output. Review generation previews after schema changes. Export updates only changed table files in a fixed directory, without creating data versions. See [release management and acceptance](../docs/release-and-acceptance.en.md) for CLI, compatible rollback, real Player builds, and reports. All workbooks keep sharing Config, Rules, and Output.

Standard Android/iOS/WebGL samples read StreamingAssets asynchronously. See [target-platform acceptance](../docs/release-and-acceptance.en.md#real-player-entry-points) for acceptance builds/device reports. Third-party AB SDKs still require integration for the project's chosen version.
