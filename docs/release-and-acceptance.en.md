# Release management and target-platform acceptance

[中文](release-and-acceptance.md) | **English**

This page defines release operations, compatibility, verification entry points, and trial records for 1.4.0 source. Implementation and acceptance results are recorded separately: a command's existence does not prove platform acceptance. See the [user guide](user-guide.en.md) for buttons and [project development](project-development.en.md) for loading policies.

## Operation boundaries

| Operation | Code | Output | Results/records |
| --- | --- | --- | --- |
| Validate only | No writes or automatic generation | Validation neither reads nor writes selected Output | Session diagnostics; optional report export; temporary compilation/binary read-back in system temp |
| Generate code | Transactionally update source, schema summary, metadata | No data publication | Preview by table, field, type, key, side; file list available as detail |
| Validate and export | Confirm generation first when needed | Atomically switch current.json after success | Archive each run entering export; successful candidate versions retain independent manifests |
| Roll back data | No game-code changes | Switch to a verified complete existing version | Deployment compatibility manifest and reason required; preview by default |
| Clean history | No source changes | Delete confirmed expired versions | Protect current, pinned, unknown, and still-readable-window versions |

Validation-only reports “generation required; full validation incomplete” for missing code, outdated support, or stale schemas. It cannot ignore new fields using old models and claim success. Blank Rules explicitly means basic checks only. Rules are executable C# and must follow project read-only conventions; arbitrary rules are not OS-sandboxed.

The App's project status bar separately reads the current version name. This display is outside validation-only and never creates/switches versions.

Passing validation does not guarantee later disk writes. Export rereads/verifies inputs and rechecks Excel, code, rules, and Tables.json fingerprints before publication, refusing switches on change. File watchers only indicate potentially stale results; correctness does not depend on watcher events.

Code generation and data publication have separate commit points. Cancellation/export failure after generation retains new code but leaves current data unchanged. A generation commit must complete or roll back. After the activation critical section, report success despite late cancellation. Uninterruptible third-party parsing/custom rules must return first. Closing an active App requests cancellation and waits for a safe finish.

## Version contents and logs

Each new version contains manifest.json, release.json, schema.json, diagnostics.json, selected `.bytes`/`.json` table data, and `.rows.json` difference snapshots. Runtime needs only root current.json plus every referenced data file; dual-format requires both file and jsonFile targets. **Do not publish row-difference snapshots or diagnostics as game resources.** Unity deployment copies referenced data; see [files, versions, loading](server-generation.en.md#files-versions-and-loading).

release.json records tool version, input/configuration fingerprints, preceding version, and table changes. Run records live in Output/Logs/&lt;run-id&gt;; fixed export.log/diagnostics.json describe the latest export. New exports never overwrite older run directories. App reports can preserve schema-check failures, validation-only, and generation results. Old successful disk reports do not describe the current run.

Completed but inactive candidates may remain for investigation; root current.json determines the active version. Rollback writes a preparation record before activation and a receipt afterward; after interruption, reconcile against the current manifest.

Save reports as .json outside project Data/Config/Rules/Output to avoid overwriting inputs/releases. Reports exclude complete workbooks/source and replace known roots with placeholders. Business messages may still contain data/other paths; inspect before sharing.

## Compare, roll back, and clean

App “Version history” supports table comparisons, per-field differences, pinning, rollback, and cleanup. Select a version/baseline and click “Compare with baseline”; enter table/offset and “Field comparison”. Rows match by complete keys, showing additions/deletions/edits with pagination. Schema differences stop field comparison to avoid misreporting incompatible layouts as data edits. Missing historical manifests/row snapshots disable corresponding operations while preserving original data.

Replace version identifiers and manifest paths below with actual values:

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release list --project samples/project.zrconfig.json
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release diff --project samples/project.zrconfig.json --from "old-version-id" --to "new-version-id"
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release rows --project samples/project.zrconfig.json --from "old-version-id" --to "new-version-id" --table Hero --offset 0 --limit 100
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release rollback --project samples/project.zrconfig.json --version "target-version-id" --compatibility "/path/to/deployed-current.json" --reason "reason for rollback"
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release cleanup --project samples/project.zrconfig.json --keep 10 --days 30
```

Commands return JSON. rows offset is zero-based; limit is 1–1000, default 100 differences. Rollback/cleanup preview by default; review and repeat with --apply. Every CLI call recomputes its plan and rechecks state on execution. App confirmation uses the previewed plan; stale plans require a new preview.

Pin or unpin a version:

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release pin --project samples/project.zrconfig.json --version "version-id" --apply
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release unpin --project samples/project.zrconfig.json --version "version-id" --apply
```

Windows source uses `dotnet run --project src/ZrConfig.Cli -- release ...`; self-contained packages use `ZrConfig.Cli.exe release ...`, with identical remaining arguments. release requires --project; rollback target side comes from project Target.

Rollback requires matching side, format, table set, schema, and configuration-source fingerprint, verifying SHA-256 per table. Changes to previewed manifests/current version invalidate the preview. The tool cannot discover running game clients; integrators must provide compatibility manifests from actual deployments. Historical versions lacking code fingerprints cannot auto-rollback; do not manually add fields to bypass compatibility.

Use current.json matching deployed code, or that deployment's manifest.json, saved/provided by integrators. Do not select the rollback target's own manifest merely to pass checks. Rollback switches local Output data only, not C# code, CDN uploads, or game reload notifications. Deploy referenced data through your normal release process afterward.

Cleanup keeps at least one latest version and a one-day reading window; defaults are 10 latest versions and 30 days. The window must cover actual old-client lifetime; confirm no readers before deletion. Current, pinned, incomplete old directories, and directories with unknown children/links are protected. Multi-version deletion is not a cross-directory transaction; after disk errors inspect remaining directories and preview again.

Publication, rollback, and cleanup acquire export then publication locks. Atomic switching relies on same-directory rename semantics on supported local filesystems. Shared drives, SMB, NFS, and synced folders require separate acceptance.

## Reverify public source

These commands depend only on public files, not internal test projects:

```bash
bash scripts/maintain.sh verify-source
```

Windows:

```powershell
powershell -File scripts/maintain.ps1 verify-source
```

This builds App/CLI, generates/validates/exports the nine official tables in system temp, checks validation-only no-writes, invalid-rule publication protection, recovery, and management commands, then removes temp files. It does not modify official Excel, Generated, or Output. Windows/macOS public CI runs it; this is not native GUI/Player acceptance.

[.github/workflows/ci.yml](../.github/workflows/ci.yml) runs portability, verify-source, and source packaging on Windows/macOS for pushes, pull requests, and manual dispatch. It also defines minimum/current Windows/Linux/macOS matrices for public ZrConfig.Server.Tests, frozen consumers, trusted API baselines, Windows MSVC, and Linux C++ sanitizers. See [server acceptance](../tests/README.en.md#public-server-acceptance) for commands/toolchains. These need no private tests; actual coverage is the commit's Actions results.

## Real Player entry points

Set UNITY_EDITOR to the actual matching executable, install host-platform build modules, IL2CPP toolchain, and license, then run:

```bash
export UNITY_EDITOR="/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/MacOS/Unity"
bash scripts/maintain.sh verify-player mono
bash scripts/maintain.sh verify-player il2cpp
```

On Windows set `$env:UNITY_EDITOR = "C:\Program Files\Unity\Hub\Editor\6000.3.26f1\Editor\Unity.exe"` to your actual installation, then run `powershell -File scripts/maintain.ps1 verify-player mono` or il2cpp. The command copies Unity into temp, exports/deploys data, builds a non-Development Player, uses High managed stripping for IL2CPP, and actually launches it. Original workspace/data/settings remain unchanged. Reports, logs, and Players are saved in artifacts/acceptance/&lt;run-id&gt;.

Android ARM64, iOS ARM64, and WebGL WASM all use IL2CPP:

```bash
bash scripts/maintain.sh verify-player android
bash scripts/maintain.sh verify-player ios
bash scripts/maintain.sh verify-player webgl
```

These check platform modules and produce APK, Xcode project, or WebGL directory. Build success is not device success. Failure writes failure.json; successful build writes device-pending.json. Install/launch Android APK; iOS needs full Xcode, signing, and device deployment; serve WebGL via HTTP(S) with correct compression headers, not file://. ZRCONFIG_ACCEPTANCE automatically starts acceptance probes; ordinary samples lack this define.

Results appear in the app, save to Application.persistentDataPath/zrconfig-acceptance.json, and log JSON with ZRCONFIG_ACCEPTANCE_REPORT. Obtain Android logs via adb logcat, iOS via Xcode device console, WebGL via browser console; browser sandbox files are supplemental. Archive complete JSON, device/browser versions, and build manifests together, explicitly recording actual execution. Without a device/browser, retain “not run.”

Standard Android/iOS/WebGL samples use StreamingAssetsConfigSource and CoalescingConfigSource. UnityWebRequest asynchronously handles Android jar: paths and WebGL URLs; synchronous waiting is prohibited. Construct/call adapters on Unity's main thread without a background scheduler. Use GetXxxAsync for unloaded references on async sources; warm-cache queries may use sync APIs. Integrate/accept the chosen resource SDK separately.

Probes cover demand reads, references, composite keys, groups, async concurrency, cancellation/failure protection, row warmup, full loading, and 10,000-row data. Reports include first-row/warm-query/full-load timings and scoped memory/allocation metrics. They use real StreamingAssets/UnityWebRequest and **do not certify any third-party resource SDK**. Allocation counters calibrate against known allocation first; unsupported platforms report allocationCounterSupported=false and -1, not a false zero-allocation claim. Thread allocation differs from whole-Player live memory and proves nothing about all loading allocations.

Repository Unity is 6000.3.26f1. Android needs matching modules/SDK/NDK/JDK; iOS needs its module/full Xcode; WebGL needs its module and a real browser. Desktop success cannot substitute for these targets.

## Release records

Use a new directory per candidate, preserving valid packages. Confirm source, App, CLI, Unity source, and rules SDK derive from the same candidate, then record each concrete release directory:

```bash
bash scripts/maintain.sh release-record "/path/to/release"
bash scripts/maintain.sh release-check "/path/to/release"
```

release-record writes release-receipt.json with tool version, build host, and SHA-256 for public source/package files, refusing overwrite. release-check detects missing/changed/added package files; it neither rebuilds nor automatically compares current workspace source with the receipt. Hashes establish recorded file correspondence, not signing/notarization/Windows GUI/device/SDK acceptance. Archive those separately.

Preserve projects, handwritten types, rules, and valid releases when upgrading. 1.4.0 reads existing project settings and saves optional project name/format version. Binary tables remain v4. New history-management files are not runtime requirements.

## Acceptance and team-trial records

### 1.4.0 candidate measurements (2026-10-15)

These are archived results for an earlier 2026-10-15 candidate on macOS ARM64 / Unity 6000.3.26f1. Source ZIP SHA-256: `98899d36569a6907632c28e0298bb4dd77c05236aeac3584492a5c2610a94a40`. The diagnostics area later changed to continuous read-only text with optimized refresh; native App acceptance below does not cover that change or certify every subsequent source commit.

| Scope | Actual result |
| --- | --- |
| Internal regression | Full smoke, 117 integration checks, workflow tests, Headless UI, and 26 type extensions passed; temporary nine-table export: 0 errors, 4 expected warnings |
| Style/layout | 20 projects, 153 source/template files, and layout checks passed |
| macOS App | ARM64 candidate launched; validation-only, warning filtering, location navigation/copy passed; no official data published/rewritten |
| macOS Player | Mono and IL2CPP High stripping built and ran; all 8 probe checks passed |
| WebGL | IL2CPP build succeeded; Chrome 155 ran all 8 checks; JSON/screens archived |
| Android ARM64 | IL2CPP APK built; no device connected, actual execution not run |
| iOS ARM64 | IL2CPP Xcode project generated; only Command Line Tools available, full Xcode build/signing/device run not performed |
| Windows x64 | App/CLI cross-packaged; native Windows GUI not run |
| Third-party resource SDK / code hot update | Product/version unspecified; not run. Probe uses UnityWebRequest |
| Production release/team trial | Stable signing, notarization, install/upgrade, device budgets, and sustained team trial remain pending |

Logs/screens are local in artifacts/acceptance. device-pending.json reflects build completion; later execution is established by browser-report.json/device reports in the same directory. Retain failed-attempt logs separately from later success. macOS/WebGL allocation counters failed calibration and report -1; no zero-allocation conclusion is made.

### Current source/documentation recheck (2026-10-15)

This pass completed 1.4.0 usage, CLI, release, and Unity documentation. README uses the user-provided original updated screenshot at the same path. Diagnostics use continuous read-only text with cross-line selection, Ctrl/Cmd multiselection, filters, search, navigation, and copying; run logs display on demand.

- Full smoke passed, including 117 integration checks, Headless UI, validation/release workflows, 10,000-row case, and packaging protection.
- Official nine-table export passed in temp; SHA-256 of 302 existing Data/Generated/Output files remained unchanged.
- Layout, public portability, and 274 local links/anchors/images across 12 public Markdown documents passed.
- Public source ZIP extracted to fresh system temp without private tests/extensions; standalone verify-source passed App/CLI build, generation, validation-only no-writes, nine-table export, rule-error protection, recovery, and release commands.

Native App, Players, and mobile devices were not rerun in this pass. Candidate records retain their original scope. Public source includes commands/CI; actual CI results belong to the relevant commit.

### Documentation/public-source recheck (2026-10-16)

This pass synchronized seven-language generation, Binary/JSON delivery, five-row headers, and server entry points, distinguishing Unity defaults, traditional native APIs, and server-v1. It restored public layout/test documentation and two performance-acceptance sources omitted by ignore rules. Source ZIP and Git candidate lists matched at 508 files.

- Full-workspace check-layout, check-portability, and smoke passed, including UI, generation consistency, nine-table export, 10,000-row data, failure protection, and packaging.
- Fresh extracted source passed independent verify-source build/generation/validation/export/recovery/release management; public server Release build had 0 warnings/errors.
- Clean-package seven-language contracts/consumers passed on macOS arm64, JDK 17.0.20.1, Go 1.19.13, Apple clang 21 (C++17), Lua 5.4.0, Node 22.23.3 / TypeScript 5.8.3, Rust 1.70.0; no native languages skipped.
- All 660 existing Data/Config/Rules/Output files retained SHA-256 during organization/verification.
- **Push gate failed:** Using GitHub commit `8627c968c396946f7060a2754751172d9695c764` as --trusted-ref reported “Migration history does not connect the trusted baseline to the candidate.” Existing migration history did not cover changes between the old remote and first local API baseline. The maintainer explicitly chose to push this update without backfilling old-version migration. The failure and checks remain recorded; this is not a pass.

Native App, Unity Players, mobile devices, Windows/Linux/MSVC, and complete performance measurements were not rerun. Historical results retain their own builds/environments. Seven-language functional success does not replace trusted-baseline acceptance.

### Requirements for subsequent records

Each record must include candidate source/package hash, platform/architecture, Unity/SDK versions, operator, date, steps, actual result, report location, and uncovered scope. Use only passed, failed, not run, or waiting on external conditions; “entry point provided” is not a pass.

| Acceptance item | Requirements/completion criteria |
| --- | --- |
| Windows GUI | Clean ordinary-user environment; paths, clipboard, file opening, DPI, cancellation, settings retained after upgrade |
| macOS GUI | Actual launch/interaction; Chinese paths, themes, permissions, release startup experience |
| Primary Player / IL2CPP | Real backend/architecture/device; demand/full switching, corruption protection, retries |
| Resource SDK | Named product/version; real handles, coalescing, cancellation, caches, downloads, update consistency |
| Code hot update | When used: AOT, generics, code/data compatibility, update order |
| Performance | Device baseline/budget; cold/warm queries, peaks, main-thread stalls, measurement definitions |
| Team trial | 1 developer and 2–3 designers, real project for at least two continuous weeks; editing time, mistakes, blockers recorded |

Trials cover project opening, validation, type/key/reference fixes, field generation, large-task cancellation, real Player publication, compatible rollback, and recovery. Data corruption, wrong activation, false success, and blocked core target-platform workflows prevent production release. Missing devices/SDKs/participants remain “not run”; do not invent passes.

Rebuilding ad-hoc-signed macOS candidates may retrigger removable-volume permission prompts. Complete OS prompts before first external-drive reads. Watchers/current-version reads start in the background so the window remains responsive while awaiting permission. Production still requires stable signing, notarization, and upgrade-install acceptance.
