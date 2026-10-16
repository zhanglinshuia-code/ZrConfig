# Project maintenance

[中文](project-layout.md) | **English**

Public documentation keeps the existing `.md` files as the Chinese edition and places matching `.en.md` English editions in the same directories. Each page includes a language switch. Navigation, section links, and source distribution must cover both editions. Tutorial archives use `README.txt` and `README.en.txt` with one shared set of source files and workbooks.

This document describes the actual layout for source/sample maintainers. Everyday authors only need the [user guide](user-guide.en.md) to select roots, enter data, and export; internal source directories and handwritten serialization are not prerequisites.

## Directory responsibilities

| Directory | Contents |
|---|---|
| src | Core, CLI, App, and Runtime/Validation .NET projects linking Assets source; third-party notices live in App/Licenses and are shared by App/CLI through links |
| .github/workflows | Automated Windows/macOS public-source build, generation, validation, export, and packaging; Windows/Linux/macOS server runtime acceptance on minimum/current toolchains |
| samples/Data | **Sole root for official Excel workbooks**, with optional subcategories; see the [workbook list](../samples/README.en.md#which-workbook-to-open) |
| unity/ZrConfig.Sample/Assets/HotUpdate/Config | Business types, tool-provided serialization support, optional converters, and generated source for every table; maintained by developers/the tool |
| unity/ZrConfig.Sample/Assets/Editor/ConfigRules | Shared business-validation rules |
| samples/Projects | Models and Validators compilation projects linking Config/Rules |
| samples/Output | Shared exports/logs; versions preserves historical releases |
| scripts/samples | C# workbook rebuild code and AllTypesDefinition.json |
| scripts | .NET maintenance project with Bash/PowerShell entry points for building, packaging, installation, and cleanup |
| tests | Integration/UI/performance checks; public seven-language contract/consumer acceptance in ZrConfig.Server.Tests links four fixtures from ZrConfig.Tests; test Excel files go only to temporary directories or obj |
| docs | User/maintenance documentation, indexed by README.md |
| docs/assets | Downloadable tutorial attachments, never official export inputs |
| unity/ZrConfig.Sample/Assets/ZrConfig | Sole source of Runtime, Validation, Unity/Runtime, and Unity/Editor, split into assemblies by asmdef |
| unity/ZrConfig.Sample | Runnable scenes, demo scripts, and project settings; compiles Assets configuration/rules directly and reuses shared exported data |
| unity/ZrConfig.Sample/Assets/ZrConfig/Native | Sole maintained Java, Go, C++, Lua, TypeScript, and Rust runtime sources; .txt templates prevent Unity treating them as native plugins; Core embeds them through linked files and generates language subdirectories under shared Config/Generated |
| artifacts | Rebuildable release packages and performance reports |

Unity demo scripts/scenes are under unity/ZrConfig.Sample/Assets. All samples share editable Assets configuration source in unity/ZrConfig.Sample/Assets/HotUpdate/Config and post-validation in unity/ZrConfig.Sample/Assets/Editor/ConfigRules. Each generation updates Config/Generated directly. Configuration compiles as ZrConfig.Models; rules compile as Editor-only ZrConfig.Sample.Rules.Editor. Use English directory/file names.

samples/Projects links configuration/rules for .NET compilation checks. src/ZrConfig.Runtime and src/ZrConfig.Validation retain only .NET projects linking corresponding Assets/ZrConfig source. Unity compiles all ZrConfig source directly, without ZrConfig DLLs or com.zrconfig.unity. Validation and Unity/Editor asmdefs are Editor-only. `unity-sample` generates/exports and deploys the shared version to Assets/StreamingAssets/ZrConfig. `package-unity` exports reusable Assets/ZrConfig source only into a new artifacts directory. Old samples/Config and samples/Rules have been migrated without copies or links. Project settings use saved paths and do not redirect old directories automatically.

Save Unity .meta files with their source. Generated .meta files are excluded from generated-source content comparison. Library, Temp, Logs, UserSettings, and build caches are unversioned. Each table generates one XxxConfig.g.cs containing its record, query table, extensions, and codec; all tables share one TableCodecs.g.cs. After generation preview/confirmation, old split files and their .meta files are removed transactionally while retaining the merged main file's GUID. Keep only one generated layout; see [generated file organization](development.en.md#generated-file-organization). Sample usage belongs in samples/README.md.

Handwritten business types live in CustomTypes by repository convention, not a scanner restriction. App/CLI recursively read C# from the selected code root, including root files and arbitrarily named subdirectories, and no longer create a type directory automatically. Generation excludes the managed root Generated directory. Generation and export skip bin, obj, dot-prefixed files/directories, and linked files/directories; export includes Generated. Managed support source remains in Serialization. Business rules use a separate path outside the code root.

## Add a table

1. Put `.xlsx` / `.xls` under samples/Data or its subdirectories for recursive discovery. Each data sheet's name is its globally unique table name; different directories/workbooks do not permit duplicates.
2. For extensions, put business types in unity/ZrConfig.Sample/Assets/HotUpdate/Config/CustomTypes, special converters in unity/ZrConfig.Sample/Assets/HotUpdate/Config/Serialization, and rules in unity/ZrConfig.Sample/Assets/Editor/ConfigRules. The tool supplies generic support code; authors need not write it.
3. Generate the whole samples/Data tree into unity/ZrConfig.Sample/Assets/HotUpdate/Config, then export everything into samples/Output.
4. Document the workbook in samples/README.md, update docs/README.md, and run `bash scripts/maintain.sh check-layout` and `bash tests/smoke.sh`, or their PowerShell equivalents on Windows.

Do not create feature-specific Data/Config/Output trees. Data may contain categories, but no parallel input root. Config/.schema.json records the schema summary in the generation transaction; Config/.generation.json records the relative input root; Config/Serialization/.support.json records support-source content versions. Save these with the project; the tool manages locks and compilation caches. Directory settings are `*.zrconfig.json` at the project root (samples for official examples). App and CLI `--project` share Core's reader. Only the tool maintains Generated; all tables share one TableCodecs and one serialization-support source set. Keep temporary test workbooks, backups, and extracted exercises out of every level of official Data.

New tables use field/type/key/description/side on rows 1–5 and data from row 6. Mark at least one key column on row 3. The complete key must be unique; each component gets group queries automatically. Excel is authoritative; do not configure a table's key twice. Tables.json accepts optional headerRows: 5 and custom groups only; keys belong on Excel row 3.

Repository App/CLI defaults use shared sample directories. The App loads saved paths; update obsolete paths in project settings. Standalone tools can still select external projects.

Every table must retain C/S/CS on row 5 and data from row 6. Missing side declarations are errors. No Tables.json layout switch or feature-specific Data/Config/Rules/Output is needed. Generation/export use the same target; see [field target sides](config-types.en.md#client-and-server-fields).

Explicitly migrate old four-row samples with `bash scripts/maintain.sh samples migrate-headers <saved-and-closed-workbook-directory>`. This local maintenance command preserves cells, backs up originals in the system temporary directory, and stops on uncertain layouts. It also migrates the standalone tutorial ZIP. Export never rewrites workbooks automatically.

## Cleanup and verification

`check-layout`, `smoke`, and workbook rebuilds here require local extensions in the full maintenance workspace, omitted from the public repository/source ZIP. Public users run `check-portability`, `verify-source`, and [public server acceptance](../tests/README.en.md#public-server-acceptance). This layout describes ownership in the full project, not a promise that every maintenance file is public.

```bash
bash scripts/clean.sh           # Remove bin/obj, compilation caches, and Finder metadata
bash scripts/clean.sh --outputs # Also remove samples/Output and artifacts
bash scripts/maintain.sh check-layout # Check Excel placement and local documentation links
bash tests/smoke.sh             # Compare generated files; build; run UI/integration checks and shared sample export
```

Windows equivalents are `powershell -File scripts/maintain.ps1 clean`, `powershell -File scripts/maintain.ps1 check-layout`, and `powershell -File tests/smoke.ps1`. They share C# implementations and require only .NET SDK 10; macOS does not require PowerShell. Source packaging, extraction/installation, path-metadata cleanup, and release-license checks share scripts/ReleaseFiles.cs; Windows packaging shares scripts/Packages.cs. Cleanup skips directory links and preserves scripts/bin and scripts/obj in use by maintenance; exit the process before deleting those.

Cleanup never deletes source, official Excel workbooks, or documentation. Do not keep unique source or measurement records in output directories. Fixed logs describe the latest run; historical logs are in Output/Logs/&lt;run-id&gt;. Data versions enable atomic switching and must not be casually removed while clients still read them. Performance reports go to artifacts/performance/latest.

## Documentation responsibilities

[user-guide.en.md](user-guide.en.md) covers authoring, installation, buttons, and data acceptance; [config-types.en.md](config-types.en.md) defines Excel syntax. [project-development.en.md](project-development.en.md) owns business types, converters, full programming exercises, CLI, and Unity integration, which are not prerequisites for everyday authoring. [Rule development](rules-development.en.md) owns tags/post-validation contracts; [tool development](development.en.md) owns architecture, support upgrades, sample rebuilds, and maintenance; [debugging](debugging.en.md) owns Rider, Visual Studio, and Unity workflows. [server-generation.en.md](server-generation.en.md) covers multi-language/server integration, API migration, and measurement methodology; [server-runtime-design.en.md](server-runtime-design.en.md) preserves constraints and original baselines; [tests/README.en.md](../tests/README.en.md) owns verification commands.

[samples/README.en.md](../samples/README.en.md) is the single usage entry for official workbooks. In the full maintenance workspace, `docs/project-status.md` records versions, actual packages, and validation results; `docs/performance-and-style.md` preserves dated measurements with environments. These internal records and `artifacts` are excluded from public source ZIPs. See the [development guide](development.en.md) and [server verification](../tests/README.en.md) for public commands; do not treat local historical reports as current release conclusions.

The input section of primary-key documentation targets authors; query APIs target developers. Source ownership follows existing conventions; changing the documentation audience does not mean generic serialization has moved into Runtime. Add topics only under docs and update docs/README.md; do not duplicate tutorials or sample source sets.

Temporary exercises, intentional errors, and compilation checks must stay outside official Data. Preserve original workbooks and valid releases. Standalone exercises may define different models with the same names as official samples; never combine their inputs.
