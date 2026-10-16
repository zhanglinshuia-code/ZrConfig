# ZrConfig

[中文](README.md) | **English**

A strongly typed Excel configuration tool for Unity and game servers. It reads workbooks, generates table code for C#, C++, Go, Java, Lua, TypeScript, and Rust, validates data, and exports versioned Binary or JSON data. The runtime supports snapshots, data updates, primary-key and group queries, and cross-table object references.

Licensed under [MIT](LICENSE). This repository includes tool source, build and packaging entry points, documentation, Excel samples, and a runnable Unity project.

The current project version is **1.4.0**. In addition to desktop authoring, release management, and asynchronous Unity resource loading, the source now includes seven-language generation, JSON export, and a separate `server-v1` server API. See [generation and runtimes](docs/server-generation.en.md) for integration and [acceptance records](docs/release-and-acceptance.en.md#acceptance-and-team-trial-records) for the coverage of candidate packages and successive source validations.

**Upgrading older versions:** This source update does not guarantee compatibility with the generated APIs of the previous GitHub version. All workbooks use five header rows; check workbooks, generated code, runtimes, and data together when upgrading. Known API baseline results are in the [current acceptance record](docs/release-and-acceptance.en.md#documentationpublic-source-recheck-2026-10-16).

![ZrConfig 1.4.0: language selection, validation-only mode, and diagnostics](docs/assets/zrconfig-app.png)

The screenshot shows the Chinese macOS UI in 1.4.0, including target side, data format, API family, and seven output languages. The sample's last validation succeeded with 0 errors and 4 expected business warnings; no data has been published. The status indicates that inputs or settings have since changed, so that result is stale. Diagnostics support filtering, search, navigation, and copying. Local paths in the screenshot are illustrative.

## Key features

- **Programmable post-validation:** Write C# row, table, and cross-table business rules to catch configuration errors during export, before runtime.
- **Editing and release management:** Validation-only mode, actionable diagnostics, live progress and cancellation, schema previews, version differences, and compatible rollback. See [usage and acceptance](docs/release-and-acceptance.en.md).
- **Breakpoint debugging:** Set breakpoints and step through post-validation rules in Visual Studio or Rider.
- **Rich data structures:** Arrays, dictionaries, custom objects, polymorphism, nested multi-row lists, composite primary keys, and cross-table references.
- **Multiple languages and formats:** Seven languages share parsing, validation, and publishing. Export Binary, JSON, or both; C/S field filtering is independent of the generated language.
- **Server snapshots and API protection:** Full, selected dependency-closure, and row loading retain old snapshots. Public API changes require explicit migration acceptance.
- **One workflow, two entry points:** The desktop App and CLI share project settings, generation, validation, and export logic.
- **On-demand loading and version switching:** Synchronous/asynchronous reads, per-row loading, and reference caching, with critical-data warmup and switching after full validation.

## Requirements

| Purpose | Requirements |
| --- | --- |
| Run packaged App / CLI | A self-contained Windows or macOS package for your architecture; no separate .NET installation |
| Build from source | .NET SDK 10, selected by [global.json](global.json); initial restore requires internet access |
| Run the Unity sample | Unity **6000.3.26f1**; [ProjectVersion.txt](unity/ZrConfig.Sample/ProjectSettings/ProjectVersion.txt) is authoritative |
| Integrate C# / Unity runtime source | .NET Standard 2.1 and C# 9; Unity compiles the source under Assets directly |
| Integrate other runtimes | Java 17, Go 1.19, C++17, Lua 5.4, TypeScript 5.8 / Node 22, or Rust 1.70 and later; see [outputs and dependencies](docs/server-generation.en.md#outputs-and-dependencies) |

Reading `.xlsx` and `.xls` does not require Microsoft Excel; editing workbooks still requires spreadsheet software. Source downloads omit installers, exported data, and Unity Library. Prepare them using the steps below. Maintainers build release packages; see [packaging and installation](docs/development.en.md#packaging-and-installation) for commands and output locations.

## Run the Unity sample

Clone or extract the complete repository, install .NET SDK 10, and run this from the repository root:

```bash
bash scripts/maintain.sh unity-sample
```

Windows:

```powershell
powershell -File scripts/maintain.ps1 unity-sample
```

Then open `unity/ZrConfig.Sample` in Unity Hub using the recorded Unity version, open `Assets/ZrConfigSample.unity`, and press Play. Preparation generates code and exports data; Unity compiles framework and business source directly from Assets. The first run needs internet access to restore dependencies. See the [Unity sample](samples/README.en.md#runnable-unity-sample) for full instructions.

A successful run produces nine tables, 10,024 records, 0 errors, and 4 expected warnings. The Editor reads `samples/Output`; desktop Players read `Assets/StreamingAssets/ZrConfig`, deployed by the preparation command. To try authoring and export alone, launch the App below without opening Unity first.

## Launch the authoring tool

macOS:

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.App -p:NuGetAudit=false -p:UsedAvaloniaProducts=
```

Windows:

```powershell
dotnet run --project src/ZrConfig.App -p:NuGetAudit=false -p:UsedAvaloniaProducts=
```

Open `samples/project.zrconfig.json` in the App. Save Excel, click “Validate only” to inspect problems, then “Validate and export” when ready to publish. Confirm the generation preview on first use or after schema changes. The shared sample selects all seven languages, Binary + JSON, and client target C. After fixing errors, deliver `current.json` and every `.bytes` / `.json` file it references, preserving relative version paths. Keep diagnostics and difference snapshots on the tool side. See [building and upgrading](docs/development.en.md) for installers.

## Generate and export from the command line

From the repository root, run generation, business validation, and export in one command:

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export --generate --project samples/project.zrconfig.json
```

Windows:

```powershell
dotnet run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export --generate --project samples/project.zrconfig.json
```

Project settings store relative paths and can move with the complete project. Exit codes: 0 success; 1 generation/export checks failed; 2 invalid usage or execution exception; 3 validation requires code generation first; 130 cancellation. Plain `export` uses existing code; `codegen` only generates code. See the [CLI tutorial](docs/project-development.en.md#13-command-line-usage) for options, target sides, and rule debugging.

To check data only, replace `export --generate` above with `validate`; it neither generates code nor publishes data. Run `release --help` for history, differences, rollback, and cleanup. After cloning, run `bash scripts/maintain.sh verify-source`, or `powershell -File scripts/maintain.ps1 verify-source` on Windows, to recheck building, generation, validation, export, and failure protection in a temporary directory.

## Documentation

| Task | Entry point |
| --- | --- |
| Install, author, generate, export, and troubleshoot | [User guide](docs/user-guide.en.md) |
| Compare versions, roll back compatible releases, clean up, and verify platforms | [Release management and acceptance](docs/release-and-acceptance.en.md) |
| Look up Excel types and cell formats | [Excel syntax](docs/config-types.en.md) |
| Run samples and explore workbooks | [Samples](samples/README.en.md) |
| Choose output languages, JSON data, and server loading modes | [Generation and runtimes](docs/server-generation.en.md) |
| Recheck seven-language contracts, frozen consumers, and performance | [Testing and acceptance](tests/README.en.md#public-server-acceptance) |
| Define business types and converters, use the CLI, and integrate Unity | [Project development tutorial](docs/project-development.en.md) |
| Write field, table, and cross-table checks | [Rule development](docs/rules-development.en.md) |
| Debug rules and Unity integration | [Debugging](docs/debugging.en.md) |
| Build, package, and upgrade support source | [Building and upgrading](docs/development.en.md) |
| Modify source, validate, and prepare GitHub submissions | [Contribution and release checks](docs/development.en.md#contribution-and-release-checks) |

See the [documentation index](docs/README.en.md) for all topics.

## Sample content

All workbooks live in [samples/Data](samples/Data). Configuration and rules are directly under Unity Assets, and every sample shares `samples/Output`.

| Workbook | Content |
| --- | --- |
| AllTypes.xlsx | Primitive and complex types, arrays, dictionaries, polymorphism, and nested multi-row lists |
| AllTypes10k.xlsx | 10,000 records covering all types, cross-table references, and same-table reference chains |
| CompositeKeys.xlsx | Two- and three-component primary keys and groups |
| TableReferences.xlsx | Bidirectional/self references, shared objects, and null references |
| ItemCatalog.xlsx、ShopOffers.xlsx、QuestRewards.xlsx | Items, shops, quest rewards, and business checks |

Unmodified samples export nine tables and 10,024 records, including the 10,000-row loading case, with 4 expected warnings. New tables use five header rows: field, type, primary key, description, and target side; data begins on row 6. All official workbooks follow this convention, including AllTypes index links, instructions, and data rows.

## Repository layout

| Location | Responsibility |
| --- | --- |
| `src` | Core, CLI, desktop App, and Runtime / Validation projects that link Assets source |
| `samples/Data` | Seven official workbooks under one shared input root |
| `unity/ZrConfig.Sample/Assets/ZrConfig` | Sole source of C# Runtime, Validation, Unity adapters, and six native runtime templates |
| `unity/ZrConfig.Sample/Assets/HotUpdate/Config` | Handwritten types, serialization support, and shared generated code |
| `unity/ZrConfig.Sample/Assets/Editor/ConfigRules` | Row, table, and cross-table business checks |
| `scripts` | Cross-platform maintenance, sample preparation, and packaging entry points |
| `tests` | Public server acceptance project and four shared contract fixtures |
| `.github/workflows/ci.yml` | Windows/macOS source verification and Windows/Linux/macOS matrices for minimum/current toolchains in seven languages |
| `docs` | User and developer documentation; screenshots and tutorial attachments in `docs/assets` |
| `samples/Output`、`artifacts` | Created by export and packaging; excluded from Git |

See [public source and layout](docs/development.en.md#public-source-and-layout) for directory responsibilities, generated-file ownership, and verification entry points.

## Data and runtimes

The App and CLI run on Windows/macOS and read `.xlsx` and `.xls` without Excel. After generation, projects load Binary or JSON configuration through the standalone runtime for their language. The tool UI and Excel parsing libraries are not part of the game runtime.

Export writes a complete data version before switching the manifest. The existing Unity C# API reads only the manifest by default. Generated queries provide each table's codec, validate its schema on first access, and load rows synchronously/asynchronously by primary key. Reference targets load when their properties are accessed; failures never publish partial results. For full validation before switching, provide `TableCodecs.Registry` and explicitly use ReloadAll / ReloadAllAsync. Remote deployments should also upload complete version data before updating the manifest.

Synchronous entry points call the source's `Read`; asynchronous ones call `ReadAsync`. The AssetBundle adapter manages resource request coalescing, scheduling, and handles; the configuration layer handles row decoding and object caches. Under Unity's default policy, successful reload invalidates old snapshot queries and old row references, so business code must acquire new objects. The separate `server-v1` API retains held snapshots and safe handles and supports explicit Prepare / Commit. See [server integration](docs/server-generation.en.md#server-contract-families-and-explicit-loading) for both lifecycles.

Model changes require matching generated code, Runtime, and data. In the Unity sample, Runtime, Validation, adapters, viewers, and business models all compile as source under Assets. See the [Unity tutorial](docs/project-development.en.md#14-unity-runtime-integration).

Configuration data updates are supported. A C# hot-update framework, automatic migration of old schemas, incremental export, and Excel write-back are not included. Android / iOS / WebGL can use the built-in `StreamingAssetsConfigSource`. Verify actual devices, IL2CPP, AssetBundle frameworks, and hot-update frameworks in the target project; see [scope](docs/development.en.md#scope).

## Report issues

Include the tool version, OS, Unity version if relevant, exact steps or CLI command, first error, and a minimal reproduction. Latest logs are `Output/Logs/export.log` and `diagnostics.json`; historical runs are in `Output/Logs/<run-id>/`. You can also export a diagnostic report from the App. Remove business data and paths unsuitable for publication. Before contributing, read [contribution and release checks](docs/development.en.md#contribution-and-release-checks).

## License

Source, documentation, samples, and generation templates use MIT, permitting commercial use, modification, and redistribution with the license notice retained. Users choose the licenses for their own workbooks, business code, and game data.

Third-party components retain their own licenses; see [third-party notices](src/ZrConfig.App/Licenses/ThirdPartyNotices.txt).
