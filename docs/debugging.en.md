# Debugging: Rider, Visual Studio, and Unity

[中文](debugging.md) | **English**

Programmable post-validation catches business errors during export and supports **Visual Studio / Rider breakpoints**. Pause in row, table, cross-table, or field checks to inspect imported values, nested objects, and related records, then step through logic and call stacks. Gameplay need not run to validate these constraints.

Establish repeatable generation/export before debugging the same input. See [rule contracts](rules-development.en.md), [building](development.en.md), and [common Excel errors](user-guide.en.md#8-locate-fixes-from-errors). Visual Studio here means the Windows desktop IDE; use Rider on macOS. VS Code launch.json is outside these steps.

**CLI (Command-Line Interface)** means `ZrConfig.Cli`, which generates, validates, or exports through arguments without opening the desktop window. **App** means the desktop UI. Debug rules through CLI because it provides `--rules-debug`. See [command-line usage](project-development.en.md#13-command-line-usage) for command structure and entry points.

## 1. Choose the right process

| Target | Launch/attach | Useful breakpoints |
| --- | --- | --- |
| Excel schema, types, generated code | ZrConfig.Cli with `codegen` | ProjectWorkflow, ModelGenerator.Prepare, ModelGenerationPlan.Apply, SchemaLoader.Load |
| Field, row, table, cross-table checks | ZrConfig.Cli with `--rules-debug` | Validate / ValidateTable / IFieldRule.Validate |
| Export, logging, publication switch | ZrConfig.Cli | ExportPipeline.RunWithSources / Publish |
| Desktop buttons, paths, language, logs | ZrConfig.App | MainWindow.RunOperationAsync / FlushEvents / RefreshProjectState |
| Game loading, references, hot updates | Unity Editor or target Player | Calls to LoadAsync / ReloadAsync and resource adapters |

Even rules stored in Unity execute in the export-tool process. Attaching to Unity will not hit rules executing in CLI. A successful tool export also does not mean Unity has read the new data.

Distinguish two Debug settings:

- Project Debug configuration controls optimization/symbols for CLI/Core tool source.
- `--rules-debug` controls dynamically compiled Config + Rules: disables optimization, defines DEBUG, and loads Portable PDB. Dynamic rules default to Release.

The App has no rule-debug switch; appending this argument does not enable it. Use CLI for business-rule debugging. Handwritten/support type compilation during generation follows a separate process, unaffected by validate/export's switch.

## 2. Shared preparation

1. Save Excel/source and record the actual absolute Data, Output, Config, and Rules paths.
2. Generate after schema changes. Confirm Config/Generated contains current models and rules compile.
3. Prefer `validate --project <project-file> --rules-debug` for rules alone, without publishing. Use temporary Output for export/switch debugging. Keep intentionally faulty copies in system temp or outside the repository, never inside official Data.
4. Break on executable statements such as if or context.Error, not only attributes, declarations, or empty braces.
5. Start with a valid record known to enter the rule. Incomplete parsing can skip all post-validation methods.

On macOS, run from the repository root:

```bash
bash scripts/dotnet.sh build src/ZrConfig.Cli -c Debug -p:NuGetAudit=false
# Generate only after schema changes or on first initialization
bash scripts/dotnet.sh src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll codegen samples/Data unity/ZrConfig.Sample/Assets/HotUpdate/Config
```

Equivalent Windows PowerShell commands (install .NET SDK 10 first):

```powershell
dotnet build src/ZrConfig.Cli -c Debug -p:NuGetAudit=false
dotnet src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll codegen samples/Data unity/ZrConfig.Sample/Assets/HotUpdate/Config
```

`NuGetAudit=false` only disables vulnerability-metadata queries for this build; it neither replaces restore nor changes data validation. Resolve network/restore build failures first.

### IDE diagnostics versus actual compilation

Distinguish IDE red underlines, .NET build output, and Unity Console. Root `ZrConfig.slnx` builds the export tool; Unity's generated solution uses Unity references/symbols. Opening one does not validate the other.

1. Run the [source build](development.en.md#environment-and-building), then inspect Unity Console's first compile error. Record code/file/line and fix the root cause; later missing-type errors may be secondary.
2. If both compile and only the IDE complains, reload the solution. Regenerate Unity project files under External Tools and open scripts from Unity. Check SDK/project selection; if needed, restart the IDE to rebuild indexes.
3. Runtime/Validation now live only under `Assets/ZrConfig`. Old paths/DLLs in IDE references suggest stale projects. Preserve the single source set; do not copy old files back to remove red underlines.
4. After cleanup, fix NETSDK1004 with a restoring build, without `--no-restore`. For `CONFIG_*` or rule compilation errors, check selected Config/Rules paths. Unity compilation success does not permit dynamic rules to depend on Unity APIs.

Restarting can fix stale indexes but cannot replace compilation checks. Never delete handwritten code, official workbooks, or valid exports just to remove IDE errors.

## 3. Rider: launch debugging from the IDE

Prefer this for short commands that might exit before attachment.

1. Open `ZrConfig.slnx` with .NET SDK 10 available. For rule-only edits, you may use your Unity solution and the external-executable approach below.
2. In **Run → Edit Configurations**, create/select **.NET Project**, choose `ZrConfig.Cli`, and build Debug.
3. Set Working directory to the repository root and Program arguments as below. Arguments start at export; do not include dotnet, run, or the DLL path.

```text
export "samples/Data" "samples/Output" "unity/ZrConfig.Sample/Assets/HotUpdate/Config" "unity/ZrConfig.Sample/Assets/Editor/ConfigRules" --rules-debug
```

4. For isolated output, replace the second path with your chosen temporary output path. Open `unity/ZrConfig.Sample/Assets/Editor/ConfigRules/AllTypesRules.cs` and break on `if (row.Settings.MaxTargets <= 0)` in Validate.
5. Click **Debug**. A breakpoint may remain unbound until the dynamic assembly loads.
6. Inspect row.Id, row.Settings, row.Actions, and `context.IsEmpty(row, "Settings.MaxTargets")`. Use `row.Id == 1` as a conditional breakpoint to narrow the case.
7. Step Over through business logic, Step Into context.Error to inspect location mapping, then Continue and check final diagnostics.

Without the tool solution, use .NET Executable with the actual dotnet executable, the full CLI DLL path as its first argument, followed by export and four absolute directories; alternatively select the built CLI apphost. Use real build output, not a rules-library DLL. See [Rider .NET project configuration](https://www.jetbrains.com/help/rider/Run_Debug_Configuration_dotNet_Project.html).

## 4. Visual Studio: launch debugging from the IDE

Use a Visual Studio version supporting this repository's .NET 10 and `.slnx`. Upgrade older IDEs or open the CLI csproj to diagnose SDK support; do not maintain a second solution for this.

1. Open `ZrConfig.slnx` and set **ZrConfig.Cli** as startup project.
2. Select **Debug**, then CLI project properties → **Debug → Open debug launch profiles UI**.
3. Enter the following Command-line arguments and set Working directory to your local root, e.g. `D:\Work\ZrConfig`. Do not add `dotnet run --`.

```text
export "samples/Data" "samples/Output" "unity/ZrConfig.Sample/Assets/HotUpdate/Config" "unity/ZrConfig.Sample/Assets/Editor/ConfigRules" --rules-debug
```

4. Open `unity/ZrConfig.Sample/Assets/Editor/ConfigRules/AllTypesRules.cs` directly and break on Validate's first if.
5. Press **F5 / Start Debugging**, not Start Without Debugging. Inspect Locals, Watch, and Call Stack for values, original blank markers, and calls.
6. Add breakpoint condition `row.Id == 1` for one record. F10 steps over, F11 steps in, F5 continues; personal key mappings may differ.
7. Inspect console and Output/Logs afterward. Rerun export after every rule edit so the tool recompiles it; do not rely on Edit and Continue for loaded dynamic assemblies.

The IDE manages launch profiles; these instructions do not commit developers' absolute machine paths. Any shared configuration must follow layout conventions and remain portable. See [Microsoft .NET debugging settings](https://learn.microsoft.com/en-us/visualstudio/debugger/project-settings-for-csharp-debug-configurations-dotnetcore).

### Attach Visual Studio to an existing process

Start CLI in a terminal, for example:

```powershell
dotnet src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll export samples/Data samples/Output unity/ZrConfig.Sample/Assets/HotUpdate/Config unity/ZrConfig.Sample/Assets/Editor/ConfigRules --rules-debug
```

In **Debug → Attach to Process**, identify the correct `ZrConfig.Cli.exe` or hosting `dotnet.exe` by name, PID, and command line; use the .NET/CoreCLR managed debugger. Do not choose among multiple dotnet processes by name alone. Attachment cannot add `--rules-debug`; supply it at launch.

CLI does not wait for attachment. If it exits too quickly, use F5 as above. If only a published executable is available, configure the IDE to launch it with arguments. UI placement varies by IDE version; see [Microsoft process attachment](https://learn.microsoft.com/en-us/visualstudio/debugger/attach-to-running-processes-with-the-visual-studio-debugger).

Rider's **Run → Attach to Process** likewise targets the actual CLI/apphost or dotnet host. See [Rider attachment](https://www.jetbrains.com/help/rider/attach-to-process.html). Do not add permanent wait loops to business rules just to catch attachment timing.

## 5. Troubleshoot missed breakpoints

Distinguish installed and source-built tools. macOS installations usually run from `/Applications/ZrConfig.app`, while the IDE starts bin output. Equal displayed versions can be different builds. Verify full process path, PID, loaded modules, and symbols. Source edits do not automatically update the installed App.

| Symptom | Check/action |
| --- | --- |
| Process exits quickly | Launch with IDE Debug/F5; break at CLI entry and inspect args |
| CLI entry breakpoint also misses | Check startup project, build configuration, actual binary path, and whether Run was used |
| Rule breakpoint hollow/unbound | Check dynamic assembly loading, --rules-debug, and source-path consistency |
| Source mismatch despite identical code | Check for copies/old SDK files; save, end the run, and export again from the original path |
| Tool Debug still hard to step through | Tool Debug differs from dynamic-rule Debug; check arguments and export.log mode |
| Rules never execute | Check nonempty/correct Rules path, instantiable classes, and target-table binding |
| Custom field rule does not execute | Check Check binding of IFieldRule; blank fields skip Check by default |
| ValidateTable runs but Validate does not | The table may contain zero rows |
| All post-validation skipped | Inspect BUSINESS_CHECK_SKIPPED, CELL_PARSE, SCHEMA_MISMATCH, TABLE_REFERENCE, and dependencies |
| Unity compiles but App cannot find the class | Remove whole-class UNITY_EDITOR wrapping; App does not define it |
| Rule runs but branch does not | Inspect actual values, IsEmpty, array length/order; break on the first statement |
| A different assembly is hit | Verify CLI by PID/full path; do not attach to build servers or Unity Editor |

When debugging tool source, break at `plan.Rules.ValidateRow(row, context)` inside `ValidationEngine.Validate`, inspect actual Rules type, Table.IsComplete, and Dependencies, then step in. This distinguishes undiscovered rules, skipped rules, and false conditions.

Assemblies named `ZrConfig.DynamicRules_<fingerprint>` load DLL/PDB from memory and may have no normal disk module path. Visual Studio's Modules window shows load/symbol status. Check **Enable Just My Code** when stepping into dynamic/external code; check Rider's external-code filters too. Do not disable source matching to conceal opening the wrong source.

Cache hits still load symbols. Fingerprints include absolute source paths/content, so moving/editing files changes the key. Fix paths, compilation, and mode first. Only if corruption is suspected, stop export and clear that Output's `.rules-cache`; the next run recompiles. Never delete valid releases to fix breakpoints.

## 6. Catch exceptions and locate Excel cells

Post-validation catches exceptions and reports diagnostics, so breaking only on unhandled exceptions may miss the original throw. Use Rider exception breakpoints or Visual Studio **Exception Settings** to break when the relevant type is thrown. Start with NullReferenceException, ArgumentException, or the logged type, and restore settings afterward to avoid stopping on every internal exception.

Preserve two files after failures:

- `Output/Logs/export.log`: Process steps, compile mode, rules path, cache details, and complete exceptions.
- `Output/Logs/diagnostics.json`: This run's runId, success, versionId, language, and diagnostics.

Read the complete JSON with:

```bash
bash scripts/maintain.sh format-json samples/Output/Logs/diagnostics.json
```

On Windows use `powershell -File scripts/maintain.ps1 format-json samples/Output/Logs/diagnostics.json`. You can also open JSON in Visual Studio, Rider, or a text editor; no extra scripting language is needed.

| Field | Meaning |
| --- | --- |
| Code | Stable identifier, e.g. CELL_PARSE, RULE_BINDING, VALIDATOR_EXCEPTION |
| File / Sheet / Table | Original workbook path, sheet, logical table; compilation errors use a .cs File |
| Row / Column | One-based physical coordinates; 0 means no individual cell located |
| Field | Logical field or element path, e.g. Actions[1].Delay |
| Severity | Numeric enum: 0 Info, 1 Warning, 2 Error, 3 Success; only Error blocks publication |
| SeverityText / Message | Display text in the current logging language |

Error rows are not data-array indexes. With five headers, data starts at Excel row 6. Always use actual coordinates from this run's report. After $order sorting, indexes still map back to original physical cells.

For generation errors, use the current App generation log or CLI codegen console; generation does not update export diagnostics.json. Early errors such as output creation failure or occupied export.lock can occur before report writing. Do not use an old report as current evidence; inspect console errors, timestamps, and runId.

The table above describes export diagnostics.json. App “Export diagnostic report” and CLI `--report` use a separate format with `success`, `cancelled`, `needsGeneration`, `codeCommitted`, and `published`; diagnostic `severity` is a string and known project-root paths are replaced. Validation-only sets `published` false; `needsGeneration` true means full validation is incomplete. Check operation/run identifiers before copying reports.

## 7. Breakpoint route through tool source

Input discovery recursively includes `.xlsx`/`.xls`. Check full paths before sheets/coordinates. Duplicate names across directories yield TABLE_DUPLICATE, scan failures INPUT_SCAN, and damaged/mismatched files EXCEL_OPEN. Break in ExcelWorkbookInput for discovery, format choice, and text conversion; SchemaLoader/ExcelDataLoader for parsing. Test copies belong in separate input trees, not subfolders of the selected root.

| Stage | File/method | Inspect |
| --- | --- | --- |
| CLI arguments | ToolCommandRunner.Run / ToolCommandLine.Parse | args, project settings, final paths, RulesDebug |
| Type discovery/generation | ModelGenerator.Prepare, ModelGenerationPlan.Apply, RuleCompiler.PrepareTypes | Config root, compile diagnostics, schemas; confirmed writes happen in Apply |
| Headers/keys | SchemaLoader, TableDefinitions, ExcelColumnLayout | Table name, DataStartRow, Fields, keys, SchemaHash |
| Cells/multi-row input | ExcelDataLoader, ExcelStructuredDataLoader, ConfigValueParser | Raw text, IsComplete, RowSource, sorted paths |
| Dynamic compilation | RuleCompiler.PrepareCore | Absolute sources, debug, fingerprint, emitted.Diagnostics |
| Attributes/post-validation | ValidationEngine.Validate / Bind | plans, Checks, Dependencies, complete, current row |
| Publication | ExportPipeline.Publish | staging, versionId, manifestTmp, pre-switch current.json |
| Runtime | ConfigManager.ReloadAsync | Manifest, relative resource paths, SchemaHash, candidate, Current |

Launch ZrConfig.App from the IDE, break in button callbacks, and click them. Generation/export runs in Task.Run; await it and inspect original exceptions, not only the last UI status.

### 7.1 Restored paths and language preferences

Under `Environment.SpecialFolder.LocalApplicationData`, `ZrConfig/workspace.json` stores four paths and `ZrConfig/settings.json` stores language; the OS determines the actual root. WorkspaceSettings saves nonempty paths as absolute to avoid working-directory-dependent changes.

If an old project is selected, inspect the four UI paths first. Close/back up the App before editing preference files; missing/corrupt settings fall back to defaults. ToolPaths searches upward from the executable, then working directory, for a repository. If found, it uses samples. Standalone macOS .app uses user-data ZrConfig/Workspace when no repository is found; other standalone processes use the working directory. Valid saved settings take priority.

Project settings use saved paths directly. Check them after moving projects; historical paths are not automatically redirected. See WorkspaceSettings and ToolPaths.

Preserve current.json when debugging publication. Failure before final File.Move must retain the old manifest; an existing version directory is not necessarily active. Never edit hashes to force invalid data through checks.

## 8. Debug Unity game code

Attach Rider to the active Unity Editor/Player, or use installed **Visual Studio Tools for Unity → Attach to Unity**. Confirm source matches the Editor project, enter Play, and trigger the game's loader. See [Microsoft Unity debugging](https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/get-started/using-visual-studio-tools-for-unity).

Sample source is directly in `Assets/ZrConfig/Runtime` and `Assets/ZrConfig/Unity`; generated tables/TableCodecs are in `Assets/HotUpdate/Config/Generated`. Open actual scripts from Unity and regenerate IDE files in External Tools when stale. The App no longer displays virtual Source groups.

First break at the game's call site:

```csharp
// Place inside the project's existing async initialization method; configOutputFolder is the actual data root.
var source = new ZrConfig.Runtime.FileConfigSource(configOutputFolder);
var snapshot = await ZrConfig.Unity.UnityConfigRuntime.LoadAsync(
    ZrConfig.Models.TableCodecs.Registry, source);
UnityEngine.Debug.Log(snapshot.VersionId);
```

After first success use Reload / ReloadAsync. A synchronous source may initially call `Load(TableCodecs.Registry, source)`. Projects already calling Initialize use `Reload(source)` / `ReloadAsync(source)`, not first Load again. Async returns built-in ConfigOperation without requiring Task. Synchronous reload explicitly fails during another load. Call Unity APIs on the main thread and observe await exceptions; do not lose failures in unobserved background work.

Check updates in this order:

1. Verify `IConfigSource.ReadAsync` uses the correct root: first `current.json`, then `versions/<versionId>/<file>`.
2. Check required tables and schemaHash against generated codecs. Default lazy loading validates tables on access. For whole-catalog consistency, pass TableCodecs.Registry and call ReloadAll / ReloadAllAsync.
3. For sha256 mismatch, check missing/truncated files and mixed uploads/caches. For schema mismatch, synchronize generated source/data. They require different fixes.
4. Break before/after ReloadAsync await and compare `oldSnapshot` with `UnityConfigRuntime.Current`. Manifest failure/cancellation retains the old snapshot; failed initial LoadAsync is retryable. Default initialization reads only the manifest; GetRowAsync/GetTableAsync checks data on access, and failed batches publish no row cache.
5. After success reacquire tables from Current. Old row values do not change in place; old snapshot queries/reference access, including cache hits, must report invalidated configuration.
6. Inspect UI refresh in Changed. Listener exceptions log to Unity Console without rolling back an already committed snapshot; do not reinitialize because the UI failed.
7. `Tools → ZrConfig → 运行时配置表` (Runtime tables) displays the currently loaded snapshot, not Excel. Row-only loading may leave no full table; inspect LoadedRowCount or explicitly load that table.

Android/iOS/WebGL samples use `StreamingAssetsConfigSource` and `CoalescingConfigSource`. On Unity's main thread inspect UnityWebRequest, relative paths, and cancellation; APK paths/URLs are not desktop files. See [StreamingAssets](project-development.en.md#145-android-ios-webgl-and-streamingassets). Verify Player debugging, IL2CPP symbols, and code hot-update frameworks per platform; CLI PDBs do not cover them.

## 9. A reproducible debugging acceptance run

- Export valid data first and record version/logs.
- Introduce one rule error in a temporary copy, hit its method, and inspect values/blank markers.
- Expect exit code 1, success false, correct diagnostic location, and unchanged old current.json.
- Fix and export successfully; Unity loads the new versionId. Held old values stay old, while new queries return new values.
- For corruption checks, damage bytes only in temporary Output. Default reload should fail when that table is accessed; ReloadAll or ReloadPrepared including it should fail before switching and preserve the old snapshot. Never damage real releases.
- Share tool version, platform/IDE, exact command/paths, both logs, expected/actual behavior, minimal source, and sanitized data. Fixed logs are overwritten by later exports; historical logs remain in `Output/Logs/<run-id>/`. Diagnostic reports may also be exported explicitly.

This guide reflects current source, repository tests, and the linked official IDE documentation. Automation verifies dynamic Debug/PDB, source call stacks, and failure protection; it does not replace interactive Rider/Visual Studio breakpoint acceptance. See [platform records](release-and-acceptance.en.md#acceptance-and-team-trial-records) for Player/IL2CPP coverage and untested areas.
