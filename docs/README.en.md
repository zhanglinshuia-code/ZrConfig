# Documentation index

[中文](README.md) | **English**

Documentation is published in separate Chinese and English editions. Use the language link at the top of any page to switch editions; navigation stays within the selected language.

See the [project homepage](../README.en.md) for an introduction and screenshots. This index serves both the public repository and source ZIP. All relative links point to public files; build installers and export data locally using the documented commands.

## Start here

- **Configuration authors:** [User guide](user-guide.en.md) → [Samples](../samples/README.en.md) → [Excel syntax](config-types.en.md).
- **Server developers:** [Multi-language generation and JSON](server-generation.en.md) → [Server loading and lifecycle](server-generation.en.md#server-contract-families-and-explicit-loading) → [Public server acceptance](../tests/README.en.md#public-server-acceptance).
- **Unity developers:** [Run the Unity sample](../samples/README.en.md#runnable-unity-sample) → [Project development](project-development.en.md) → [Rule development](rules-development.en.md).
- **Tool maintainers:** [Environment and building](development.en.md#environment-and-building) → [Architecture and data flow](development.en.md#architecture-and-data-flow) → [Contribution and release checks](development.en.md#contribution-and-release-checks).

## Introduction and features

| Topic | Entry point |
| --- | --- |
| What the tool does and its features | [Homepage](../README.en.md#key-features) |
| Write business checks to catch configuration errors early | [Programmable post-validation](rules-development.en.md) |
| Debug rules in Visual Studio / Rider | [Debugging](debugging.en.md) |
| Supported data structures | [Structures and types](config-types.en.md#choose-a-table-structure) |
| Explore working examples | [Excel and Unity samples](../samples/README.en.md) |

## Authoring and export

| Topic | Document |
| --- | --- |
| Installation, project settings, validation-only mode, generation, export, and troubleshooting | [User guide](user-guide.en.md) |
| Validation-only mode, version comparison, controlled rollback, and platform acceptance | [Release management and acceptance](release-and-acceptance.en.md) |
| Copy and clear logs | [Viewing and clearing logs](user-guide.en.md#22-view-and-clear-logs) |
| Types, arrays, objects, dictionaries, and multi-row input | [Excel syntax](config-types.en.md) |
| Five fixed header rows and C / S / CS markers (row 5 is mandatory and fields must declare their target side) | [Client and server fields](config-types.en.md#client-and-server-fields) |
| Official Excel workbooks and running the Unity project | [Samples](../samples/README.en.md) |
| Fields and examples in the all-types workbook | [All-types sample](type-examples.en.md) |
| Loading-test input with 10,000 rows and 106 fields | [10,000-row all-types case](../samples/README.en.md#10000-row-all-types-loading-case) |
| Primary keys, composite keys, and group queries | [Table queries](table-indexes.en.md) |

## Development and debugging

| Topic | Document |
| --- | --- |
| One file per table, type responsibilities, and old-layout migration | [Generated file organization](development.en.md#generated-file-organization) |
| C# / C++ / Go / Java / Lua / TypeScript / Rust generation, JSON, snapshots, and performance limits | [Multi-language generation and JSON](server-generation.en.md) |
| Server runtime design constraints, original baselines, and implementation records for maintainers | [Server runtime design](server-runtime-design.en.md) |
| Seven-language public contracts, frozen consumers, and minimum-toolchain verification | [Public server acceptance](../tests/README.en.md#public-server-acceptance) |
| API change previews, consumer acceptance, and explicit migration | [Public API protection](server-generation.en.md#public-api-protection-and-explicit-migration) |
| Business types, converters, CLI, and Unity integration | [Project development](project-development.en.md) |
| validate, reports, cancellation, and exit codes | [CLI usage](project-development.en.md#13-command-line-usage) |
| Direct table codecs, registry-free synchronous/asynchronous loading, and the full catalog | [Lazy runtime loading](project-development.en.md#lazy-row-loading-and-cross-table-references) |
| Generic synchronous/asynchronous integration with third-party AssetBundle frameworks | [AssetBundle integration](project-development.en.md#third-party-ab-frameworks-synchronous-and-asynchronous-loading) |
| Runnable temporary AssetBundle bridge, dual-mode queries, and read logs | [Unity AssetBundle sample](../samples/README.en.md#synchronous-and-asynchronous-assetbundle-sample) |
| Field, row, table, and cross-table checks | [Rule development](rules-development.en.md) |
| Unity framework source location and integration | [Assets source integration](project-development.en.md#142-integrate-unity-source) |
| Asynchronous resource reads on Android / iOS / WebGL | [Built-in StreamingAssets adapter](project-development.en.md#145-android-ios-webgl-and-streamingassets) |
| Player builds, device results, and actual coverage | [Platform acceptance](release-and-acceptance.en.md#real-player-entry-points) |
| Rider / Visual Studio / Unity debugging | [Debugging](debugging.en.md) |
| Run from source, package, upgrade support source, and use current headers | [Building and upgrading](development.en.md) |
| C# naming, formatting, generation templates, and source maintenance | [Code conventions](development.en.md#c-conventions) |
| Review generation previews, rollback failures, and recovery backups | [Generation consistency](development.en.md#generation-consistency-and-failure-recovery) |
| Public files, directories, and generated-file ownership | [Public source and layout](development.en.md#public-source-and-layout) |
| Maintenance layout conventions and public verification entry points | [Layout](project-layout.en.md) · [Verification](../tests/README.en.md) |
| Repeatable build, export, and Unity verification | [Verify public source](development.en.md#verify-public-source) |
| Loading policies, warmup switching, and diagnostics | [Warmup and loading diagnostics](project-development.en.md#prewarming-and-diagnostics) |
| Query allocations, indexes, and object use after updates | [Allocations and hot updates](table-indexes.en.md#allocations-and-hot-updates) |
| Table encapsulation, commit points, and lock ordering | [Indexes and table-loading responsibilities](development.en.md#index-and-single-table-loading-responsibilities) |
| GitHub submissions, release contents, and issue-report requirements | [Contribution and release checks](development.en.md#contribution-and-release-checks) |

## Samples and tutorial attachments

Official workbooks live in [samples/Data](../samples/Data). Nine tables, including the 10,000-row loading case, share Assets configuration source, rules, and Output. All official samples are ready to use; completing the development exercises first is unnecessary.

The [standalone exercise attachment](assets/user-guide-example.zip) accompanies chapter 12 of the development tutorial. Extract it outside the repository. It uses a different Hero/Skill teaching schema and must not be mixed with official workbooks or models with the same names.
