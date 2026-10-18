# ZrConfig user guide: authoring, export, and acceptance

[中文](user-guide.md) | **English**

ZrConfig is an Excel configuration tool for Unity / C# projects, centered on **programmable post-validation that finds business errors during export**. Offers referencing disabled items or invalid skill parameters might otherwise surface only in gameplay. Once developers encode constraints in C#, every export checks them and locates problems at specific Excel cells.

Rich structures suit items, shops, quests, skills, and battle actions, with automatic typed code and Binary/JSON output. Developers verify checks using Visual Studio/Rider breakpoints and stepping; authors keep using the desktop tool to edit, export, and resolve errors.

This guide is for everyday workbook editing, data fixes, and delivery. Follow **edit/save Excel → validate and fix → generate when needed → validate/export → verify and deliver**. You may also directly Validate and export; full checks still run.

The tool generates table code and serialization support. Everyday use needs no C# classes, serializers, or converters. Developers prepare extra types/rules/Unity integration using [project development](project-development.en.md). Use existing code/rules directories according to project conventions.

## Authoring capabilities

| Need | Support |
| --- | --- |
| Catch business errors early | Project C# row/table/cross-table post-checks plus field validation before publication |
| Find data to fix | Workbook, sheet, coordinates, field paths; nested members map to original cells |
| Primitive attributes/collections | Numbers, text, bool, enums, vectors, arrays, dictionaries, nullable values, rectangular arrays, JSON |
| Complex skills/behavior | Split object members, typed polymorphic actions, multi-row/nested lists sorted per level |
| Levels, quality, drop groups | Single/composite primary keys with full-key/group queries |
| Link other tables | Target Ids become object references, including bidirectional/self/shared references |
| Preserve valid deliveries | Current version updates only after checks succeed; failed exports retain the last valid release |

See [structure overview](config-types.en.md#choose-a-table-structure) and [Excel syntax](config-types.en.md). Developers provide project action kinds, types, and constraints; authors fill templates.

## Reading route

| Task | Read |
| --- | --- |
| First use | [1. Prepare](#1-before-starting) → [2. Paths](#2-open-the-tool-and-set-paths) → [3. First export](#3-first-export) |
| Create/edit Excel | [4. Headers](#4-excel-headers), [5. Input syntax](#5-entering-data) |
| Fix validation errors | [6. Results](#6-validation-and-post-validation), [8. Troubleshooting](#8-locate-fixes-from-errors) |
| Verify delivery | [7. Outputs](#7-what-to-deliver-after-success), [10. Acceptance](#10-acceptance-after-every-edit) |
| Daily operations/schema changes | [9. Routine edits](#9-routine-edits) |
| Types, converters, rules, game code | [Developer documentation](README.en.md#development-and-debugging) |

## 1. Before starting

Use your project's App, workbooks, and paths. Self-contained Windows/macOS packages include the runtime; everyday authoring needs no .NET SDK or Microsoft Excel for export. Editing requires spreadsheet software saving .xlsx/.xls.

Developers handle source startup, SDKs, builds, and packaging using [building/upgrading](development.en.md). Apple Silicon packages are artifacts/macos/osx-arm64/ZrConfig.app; Intel uses its matching architecture. Windows x64 runs artifacts/windows/win-x64/ZrConfig/ZrConfig.App.exe. Keep the entire Windows folder, not just the exe.

Confirm four paths: Excel input, output, code root, optional rules. Complete [samples](../samples/README.en.md) are included; no creation/rebuild is needed first.

| Who | Responsibility |
| --- | --- |
| Authors | Edit/save Excel, generate/export, fix data errors, verify delivery |
| Developers | Initial game integration, new types/rules, compile/compatibility issues |
| Tool | Parse, generate, validate, write versioned data/current logs |

### 1.1 App and CLI

**App** is the windowed ZrConfig desktop tool with path fields/buttons, used by this guide.

**CLI** means **Command-Line Interface**, usually ZrConfig.Cli here. Enter commands in Windows PowerShell/macOS Terminal, specifying Excel/output/code/rules paths and operations; results appear as text/logs.

| Interface | Use | Audience |
| --- | --- | --- |
| Desktop App | Choose project and click Validate only, Generate configuration code, or Validate and export | Everyday authoring, data fixes, manual acceptance |
| CLI | Enter commands or invoke through scripts | Developers, automation, rule breakpoint debugging |

App Validate and export corresponds to CLI export --generate: check/update necessary code, then validate/export. Plain export uses existing code. Changed input roots or generated-file deletions require CLI preview review and --accept-changes. App users need not also start CLI. See [commands and OS examples](project-development.en.md#13-command-line-usage) when needed.

### 1.2 Installation and upgrades

macOS: quit ZrConfig, place architecture-matching ZrConfig.app in /Applications, and launch there. Preserve the last valid package before replacing the App; avoid also starting an old copy. The title shows version. See [building/upgrading](development.en.md).

Windows: quit the old tool, extract the complete new release folder to the agreed tool location, and open ZrConfig.App.exe. App, CLI, DLLs, and runtime form one package; replace together without mixing versions.

Apps and project data are separate. Upgrading needs no copying/rebuilding of Data/Config/Rules/Output or settings deletion. Open the original *.zrconfig.json, confirm paths, and Validate and export. Review requested code/support upgrades under project policy. Developers synchronize game code/data for schema/binary changes; see [installation](development.en.md#packaging-and-installation) and [support upgrades](development.en.md#support-source-upgrades).

## 2. Open the tool and set paths

The main view prioritizes project name, target side, latest operation, and current version. Open settings, select a recent project, or use onboarding to check/save. Four paths are in expandable “Project directory settings”:

| Input | Select | Routine use |
| --- | --- | --- |
| Excel input directory | Data root containing workbooks | Recursively reads .xlsx/.xls in all subdirectories |
| Output directory | Project Output folder | Results, logs, delivery files |
| Unity configuration code root | Project Config folder | Select root; tool generates and developers integrate |
| Optional C# rules directory | Project Rules folder | Select provided rules, otherwise leave blank |

Select **Config root**, not Generated. Subdirectory hints support developer integration; everyday users only choose the correct root without creating/editing serialization files.

Operations use current typed/browsed/dropped paths. C# is discovered recursively without restricting handwritten subdirectory names. Generation skips its own Generated output; compilation caches, hidden files, and links are excluded.

Blank Rules means no extra business rules. A specified empty folder instead reports unavailable rules. Do not clear configured rules to bypass errors; fix data or contact maintainers.

Click “Save project configuration” as project.zrconfig.json, preferably at project root (samples here). Relative paths move with the project. CLI codegen/validate/export/support --project uses the same file. Invalid settings never partially overwrite current paths. Standalone generation checks only Data/Config; Output/Rules may be unset then. Export checks Output and specified Rules; new output directories can be created.

All four fields accept folders. Dropped files use their parent; multiple files must share a directory or the value stays unchanged. Dragging sets paths without moving files. Paths lock during generation/export.

Generation languages select one or more code outputs. Data format selects Binary, JSON, or both and is saved with the project. These differ from UI language and C/S target. Samples use all seven languages, dual-format, and C. With existing multi-language code, selecting only C# or another language allows both generation and export: generation, export, and validation preserve existing languages and check whether their code needs updating. Deselecting a language does not delete its APIs. First-time generation and added languages follow the selection. To remove an existing language, developers use CLI `codegen --prune-languages` and review the changes via [multi-language migration](server-generation.en.md). Breaking language-removal, target-side, or API changes still require a separate review; ordinary generation confirmation cannot approve them.

### 2.1 Remembered paths and language

The App remembers paths. On project switches verify all four belong together; do not change only Excel while keeping another project's code/output.

Recent projects restore saved settings; removing a recent entry deletes no project file. Onboarding asks name/side/paths, then “Check paths/schema”, then “Save/start”. Checking is read-only and does not generate/publish. Recheck after wizard path edits.

Status shows current version, business-rule enablement, and latest operation. File/settings changes may mark results stale; save Excel and revalidate. An old status version does not mean this export succeeded.

Top “Language” selects Chinese/English. Logs use the language at operation start. Mid-run switching affects UI/future operations, not old logs. It changes neither numeric syntax nor exported data.

First repository launch usually uses samples; standalone tools may need manual paths. See [debugging](debugging.en.md) for restored settings/defaults/preference files.

### 2.2 View and clear logs

Default diagnostics group by severity/table, with filters/search. Continuous read-only text supports cross-line drag selection and Ctrl (also Cmd on macOS) multiselection. “Copy selected issues” copies whole diagnostics touched by selection; context-menu Copy copies selected text. Select an issue to open its file, reveal its directory, or copy cell location; each line of a multiline message maps to one diagnostic. Nested members retain real cells. Automatic cell navigation depends on spreadsheet software; the tool opens workbooks/copies locations.

Run logs preserve technical detail, color-coded by info/success/warning/error, with selected/all-text copying. Tab switches preserve position/selection; UI language changes do not rewrite existing diagnostic bodies.

“Clear Logs” clears current display, selection, and language hints and returns to the top. Disk logs/reports/data and operation status remain. Disabled during generation/export, it returns afterward. Language switching does not restore cleared content; the next operation displays new logs normally.

## 3. First export

Confirm “Generation/export target”: C client, S server. CS fields belong to both; missing side is an error. Target is saved. Generated code/delivered data must match; update code when switching sides.


With included samples:

Fastest: open [samples/project.zrconfig.json](../samples/project.zrconfig.json), then go to step 5. Or set individually:

1. Excel input: samples/Data.
2. Output: samples/Output.
3. Code root: unity/ZrConfig.Sample/Assets/HotUpdate/Config.
4. Rules: unity/ZrConfig.Sample/Assets/Editor/ConfigRules.
5. Click Validate and export; if prompted, review root/changes and choose Generate and export.
6. Wait for completion, verify success/0 errors, read warnings, then open output.

Unmodified samples export nine tables/10024 records, including the 10,000-row case, with 0 errors/4 expected warnings. New checks also show Info/Success; see [post-validation samples](../samples/README.en.md#post-validation-and-four-diagnostic-levels):

| Workbook | Exported tables/rows |
| --- | --- |
| AllTypes.xlsx | AllTypes：3 |
| AllTypes10k.xlsx | AllTypes10k: 10000 (all-types loading case) |
| CompositeKeys.xlsx | Drop：3；SkillLevel：4 |
| TableReferences.xlsx | Hero：3；Skill：2 |
| ItemCatalog.xlsx | ItemCatalog：3 |
| ShopOffers.xlsx | ShopOffers：3 |
| QuestRewards.xlsx | QuestRewards：3 |

AllTypes is wide: start with leftmost primitives, then use its type index for arrays/objects/actions. It shares five headers: keys row 3, descriptions row 4, sides row 5, data row 6. See [all-types layout](type-examples.en.md).

### 3.1 When to use each button

| Button | When | Result on success |
| --- | --- | --- |
| Validate only | Routine type/key/reference/business checks | Current results, no publication |
| Generate configuration code | Initial integration/schema/type changes | Updated code/schema summary, no publication |
| Validate and export | Deliver this edit | Activate a new data version after full checks |

| Change | Generate first? | Validate/export afterward? |
| --- | --- | --- |
| Values, text, array contents | No | Yes |
| Add/remove data rows | No | Yes |
| Add/remove/rename fields or change types | Yes; confirm schema with developers | Yes |
| Add/remove/rename tables or keys | Yes; confirm game integration/references | Yes |
| Developer edits business rules | Usually no; developer confirms | Yes |
| Developer edits custom types/serialization | Developer synchronizes code/version | Yes |

Validate and export first checks code, previews initial/schema/support changes, and continues after Generate and export confirmation. Data-only edits export directly. Standalone Generate updates code only. “Generate first” above may be fulfilled by pre-export confirmation.

Changed input root or old-table file deletion shows old/new roots and deletion list. Ensure you did not select a Data subfolder. Confirm intended removal/rename; otherwise cancel and restore the full root. Export prompts Generate and export; standalone generation prompts Confirm generation. Preview is read-only; cancellation preserves code, support, version records, and active release.

Validate only writes neither Config nor selected Output, requires no writable Output, and never auto-generates. Missing/stale code reports incomplete full validation; inspect generation preview first. Validation success is not publication. Use Validate and export for delivery. Compilation/read-back uses system temp; session diagnostics can be explicitly exported.

Progress shows phase, table/workbook, known progress, elapsed time, and Cancel task. Code/data have independent commits: after generation, cancellation/export failure retains code but not a new active data version; completed activation reports success. Uninterruptible parsing/rules finish at safe boundaries. See [operation boundaries](release-and-acceptance.en.md#operation-boundaries).

## 4. Excel headers

### 4.1 Always use five header rows

Every table retains target side on row 5; each field explicitly declares C/S/CS or errors. **Row 5 is mandatory; data always starts on row 6.** See [side rules](config-types.en.md#client-and-server-fields).


Rows 1–5 contain field names, types, key markers, descriptions, and target sides. **Data begins on row 6.**

This standalone Item.xlsx exercise must not be mixed into official samples:

| Excel row | Column A | Column B | Column C |
| --- | --- | --- | --- |
| 1: Field | Id | Name | Count |
| 2: Type | int | string | int |
| 3: Key | `主键` (literal marker) | | |
| 4: Description | Item identifier | Item name | Quantity |
| 5: Side | CS | CS | CS |
| 6: Data | 1001 | Small potion | 10 |
| 7: Data | 1002 | Large potion | 5 |

At least one row-3 cell must contain exactly 主键. Others stay blank, without descriptions/Required/Positive text. Only Id is marked here, so each item's Id is required and unique.

Five-row validation is fixed. Missing, misspelled, or mixed row-3 markers block generation/export, including first generation. Entirely missing markers locate row 3; individual errors locate cells. The tool never invents keys, deduplicates, or rewrites data. Failure preserves generated code/current release. Old four-row format is neither detected nor used as fallback.

Row 4 is descriptive only; writing Required/Positive creates no rule. Keys identify records/enforce complete uniqueness; positive values/required names are project business rules.

### 4.2 Composite keys

Mark every participating column with 主键 on row 3; header count/data start remain unchanged:

| Excel row | Column A | Column B | Column C |
| --- | --- | --- | --- |
| 1: Field | GroupId | Id | Count |
| 2: Type | int | int | int |
| 3: Key | `主键` (literal marker) | `主键` (literal marker) | |
| 4: Description | Group identifier | Identifier within group | Quantity |
| 5: Side | CS | CS | CS |
| 6: Data | 10 | 1001 | 1 |
| 7: Data | 20 | 1001 | 2 |

Complete key is GroupId + Id; different groups permit equal Id. Complete combinations must be unique and each main-record component filled. Business fields can form keys without extra int Id.

Explicit 0 can be a key, subject to business rules. See [key types/nonadjacent columns/examples](table-indexes.en.md#how-to-fill-the-workbook). Existing keys are not casual grouping notes; changes require regeneration/game coordination.

### 4.3 Files, sheets, and table names

- Recursively reads .xlsx/.xls, case-insensitively, from selected root/subdirectories. .csv/.xlsm/.xlsb are unsupported here.
- .xls must be real binary Excel, not renamed CSV/HTML/.xlsx. Both formats share headers/syntax/validation and may coexist.
- Each exported sheet name is its table name. Workbook/subdirectory names are not added, whether one or multiple sheets.
- Sheets starting #, e.g. #说明, are skipped. Hidden business sheets still export.
- Table names must be unique case-insensitively across all folders/formats. Two workbooks cannot both contain Item. Other sheet additions/deletions do not rename existing tables.
- Table/ordinary field names use ASCII letters, digits, underscores, never a leading digit. Keep field case consistent; __zr prefix is reserved.
- Dotted, [], $type, and $order headers are predefined split/list layouts; do not rename as ordinary fields.
- Excel ~$ locks and macOS ._ files are ignored. Actual hidden-folder workbooks still read. Backups belong outside the input root, not hidden in a backup subfolder.
- Symlinked directories, Windows junctions, and linked files are skipped to avoid cycles/duplicates. Keep actual workbooks in the input tree. Inaccessible real subdirectories fail scanning; fix permissions and retry.

For example, classify like this and select outermost Data only:

```text
Data/
  Items/
    Item.xlsx
  Battle/
    Skills/
      Skill.xls
```

Both tables generate/validate/export together; cross-folder/format references use global table names. Windows/macOS support local Chinese/spaced paths. Reselect actual local paths when switching computers.

### 4.4 Saving, precision, and sorting

The tool reads disk, not unsaved Excel-window contents. Save edits before export.

.xls formulas use saved cached results; the tool does not recalculate. Recalculate/save in spreadsheet software after formula/dependency changes, or enter final values for stable delivery. Numeric parsing uses one decimal convention independent of OS region.

Set cells to text before entering long/ulong/high-precision money beyond Excel precision. Already rounded digits cannot be recovered. Use . for decimals and ASCII punctuation for arrays/JSON.

Never merge field-name/data cells or sort only selected columns of multi-row records. Save/close before moving/rebuilding. Everyday use requires no sample-overwrite rebuild scripts.

## 5. Entering data

Headers determine types. Use provided types/enums/members when filling templates; no field implementation is required.

| Type/scenario | Example | Notes |
| --- | --- | --- |
| Integer | 1001, -5 | Range depends on numeric type/business rules |
| Decimal number | 1.25 | Dot separator; no NaN/Infinity |
| bool | true, false, 1, 0 | false is explicit input |
| string | Small potion | Ordinary text retains surrounding spaces; all whitespace is empty |
| Provided enum | Rare | Declared names only, not arbitrary numbers/new names |
| vector2 / vector3 | 1,2 / 1,2,3 | Two/three components |
| kv | 1001,10 | Two integers; project defines meaning |
| int[] | `[1;2;3]` | Semicolon separator |
| int[][] | `[[1;2];[];[3]]` | Bracket every level; inner lengths may differ |
| int? | 123, null, blank | 0 is a value |
| `Dictionary<string,int>` | `["hp"=100;"attack"=25]` | Parsed keys must be unique |
| int[,] | `[[1;2];[3;4]]` | Rectangular; equal row lengths |
| json<int[]> | `[1,2,3]` | Standard JSON uses commas |
| Provided special type | E.g. money 1.25 | Follow project instructions; developers supply special formats |

See [Excel types/syntax](config-types.en.md) for full coverage, escapes, and combinations.

### 5.1 Blank versus explicit values

Blank differs from 0, false, and []. The tool tracks filled status for Required. A blank non-nullable integer may decode as 0 but remains unfilled; nullable blank/null means absent. [] is empty, not satisfaction of an “at least one element” rule.

Blank object members may retain project defaults; required members still need explicit input. Output values alone cannot reveal whether input was filled.

Quote/escape strings containing semicolons/brackets/quotes, e.g. `["a;b";"x,[]"]`. Not every format is JSON: ordinary arrays use semicolons, json<T> commas; do not mix them.

### 5.2 Split objects, polymorphism, multi-row lists

Settings.MaxTargets is a member of a settings group. Row 2 uses the project's whole-group type, not int/bool per member.

Actions[].$type selects kinds such as Damage/AddBuff. Fill applicable members only, leaving other subclasses blank. Actions[].$order defines order using unique positive integers per template.

Expanded [] columns allow several rows to form one record:

- Group by the complete row-3 key. First fill all components; continuations repeat all or leave all blank to continue the preceding valid record. Partial keys are invalid.
- Main fields come from the first occurrence; later values are blank or exact repeats. Conflicts fail.
- Entirely blank child-list groups add no element.
- Each nested level groups/sorts by its own order. Explicitly fill parent numbers; blanks never infer ownership.
- Reported array indexes are zero-based after sorting, accompanied by original Excel coordinates; edit those original cells.

See [split/multi-row rules](config-types.en.md#split-objects-polymorphic-type-columns-and-multi-row-action-lists). Developers extend kinds/members; authors use supported kinds/parameters.

### 5.3 Cross-table object references

A SkillConfig type cell expects a target Skill int Id such as 10, not its name or object JSON.

Nonzero Ids must exist; target Ids must be nonzero/unique. Blank/0 means no reference and fails required rules. Negative IDs are actual lookups, not null. Direct references currently support top-level scalar fields only.

[TableReferences.xlsx](../samples/Data/TableReferences.xlsx) shows bidirectional/self/shared targets. The tool links them; authors need no linking code.

## 6. Validation and post-validation

Post-validation checks business rules after parsing into records, **still before publication**: positive target count, nonnegative delay, conditionally required fields, or skills compatible with heroes.

Lists are already merged and direct references linked, allowing whole-skill/shop/related-item checks:

| Scope | Problems detected |
| --- | --- |
| Field | Missing names, out-of-range values, duplicates where unique, nonexistent foreign keys |
| Row | Minimum above maximum, negative delay, invalid buff stacks |
| Table | Duplicate item/price offers |
| Cross-table | Active offers referencing disabled items; rewards above stack limits |

Project rules define these conditions. Official samples treat negative prices as errors and split-stack rewards as warnings. See [item/shop/quest checks](../samples/README.en.md#post-validation-and-four-diagnostic-levels).

Developers maintain rules; Validate and export runs them automatically without a separate post-check button. Authors fix data based on results.

For unexpected logic, developers debug the same rules in Visual Studio/Rider, inspecting imported values, linked objects, and branches to distinguish data versus rule defects. See [debugging](debugging.en.md); everyday authors need no IDE.

| Result | Meaning | Action |
| --- | --- | --- |
| Info | Business notice/summary, nonblocking | Read status/counts |
| Success diagnostic | One check passed, nonblocking | Whole-run success still comes from final result |
| Error | Cannot publish | Fix workbook/sheet/cell/message and retry |
| Warning | Nonblocking itself | Confirm intended data; other Errors still fail |
| Check skipped | Incomplete current/dependency table prevents judgment | Fix original parse/missing-table/reference errors first |
| Validation-only success, 0 errors | Current checks passed, unpublished | Validate and export for delivery |
| Export success, 0 errors | New output version activated | Verify table set/counts/delivery |

Field diagnostics include workbook, sheet, coordinates, path, and code. Even sorted nested paths such as Groups[1].Actions[0].Ratio map to original cells. Follow actual logged coordinates. See [outputs/logs](#7-what-to-deliver-after-success).

Blank Rules still checks types, structure, explicit keys, direct references, codecs, and publication integrity. It cannot infer project level caps, required conditions, or probability totals.

One export may report several independent errors; post-checks can continue after attribute errors. Fix clear inputs first, then remaining diagnostics. Skipped is not passed. Give developers logs/expected constraints for questionable rules; see [rule development](rules-development.en.md).

## 7. What to deliver after success

This shows Binary + JSON. Binary alone emits .bytes tables; JSON alone emits .json tables:

```text
Output/
  CSharp/
    Tables/
      json/                    JSON table files: Hero.json, Skill.json, ...
      bytes/                   Binary table files: Hero.bytes, Skill.bytes, ...
    Common/
      current.json             Table manifest and integrity hashes
  Java/                        Same structure for each selected language
  Go/                          Also Cpp, Lua, TypeScript, Rust
  Logs/
    export.log                 Latest run log
    diagnostics.json           Latest structured checks
    <run-id>/                  Per-run logs, diagnostics, and result
```

Data uses a fixed directory. For example, JSON mode writes `Output/CSharp/Tables/json/Hero.json`; changing Hero updates this file in place. Other tables with identical content keep their bytes and modification times. Repeated exports with no changes leave all data files and `current.json` untouched. Formatting or resaving Excel alone does not rewrite data. No new version directories, archived data copies, or row-difference snapshots are created. Existing archived version directories are preserved; after migration, use the manifest under each language’s Common folder.

Every export validates the complete input and cross-table references before writing. Changed or missing/corrupt files are replaced, with the manifest written last. A write failure restores the previous files. Format switches and explicit table removals remove only obsolete files registered in the previous manifest for that language; unrelated files are preserved.

Deliver **the selected language folder, preserving its Common and Tables subdirectories**. Dual-format output requires both `.bytes` and `.json`. Logs, locks, and caches are not game data. Edit Excel, not generated files. The manifest carries a content identity for integrity and runtime caches; it is not an output version directory or history.

Only selected language directories are updated. Selecting C# alone preserves other languages’ exported data. JSON mode creates only `Tables/json`, Binary only `Tables/bytes`, and dual-format output creates both. Readers use the language folder (for example `Output/CSharp`) as their root.

Each run still records its diagnostics under Logs. Successful export shows the number of updated tables or “Data unchanged”.

### 7.1 Why the game has not changed after export

Export only updates selected Output. The game must receive data and invoke its project's load/refresh operation; running games do not automatically read arbitrary new files.

Verify actual values through project tools after delivery. Finish synchronizing all changed files and the manifest before reloading. A lazy loader holding old metadata must reload the manifest before fetching changed files. Developers handle deployment/loading/compatibility.

Value-only edits usually need data publication only. Table/field/type/key changes may require game-code updates; new bytes cannot simply be handed to old code.

## 8. Locate fixes from errors

Read **code → file/sheet → Excel coordinates → field → message**. Summaries show full workbook/A1 locations, e.g. Data/TableReferences.xlsx → Hero!A6 → Id; C# shows Rules/Broken.cs(4,20). Excel coordinates are one-based, not array indexes. Compile locations are source lines and classify business types/generated/support-converters/rules with suggested fixes for developers.

| Message/code | First check | Owner |
| --- | --- | --- |
| INPUT_EMPTY / no workbooks | Saved .xlsx/.xls under root/subdirectories; no mistaken linked root | Author |
| INPUT_SCAN | Subdirectory existence/access, disk/network share | Environment maintainer, then retry |
| EXCEL_OPEN | Corruption, encryption, renamed extension; inspect full path | Author/file provider |
| Duplicate table name | Effective sheet names across files | Author/schema maintainer |
| SCHEMA_KEY / header-key error | Row 3 only literal 主键 or blank; descriptions row 4, data row 6 | Author; confirm schema changes |
| CELL_PARSE | Numbers, brackets, delimiters, enum, JSON versus type | Author |
| REQUIRED | Blank field/member | Author |
| RANGE / POSITIVE | Range/nonpositive value | Author |
| KEY_DUPLICATE | Duplicate record/complete key | Author |
| REFERENCE_MISSING / TABLE_REFERENCE | Target Id existence, duplicate/missing target table | Author |
| BUSINESS_CHECK_SKIPPED / REF_CHECK_SKIPPED | Earlier missing-table/parse/link errors | Fix original data first |
| GENERATION_CONFIRM_REQUIRED | Full input root/deletions; cancel mistaken subfolder | Author/maintainer |
| SUPPORT_UPDATE_REQUIRED | Generate to upgrade unchanged support, then export | Author; developer if failure |
| SUPPORT_MERGE_REQUIRED | Handwritten/unknown support requires per-file merge | Developer |
| CODE_ROOT_GENERATED | Third field mistakenly selects Generated | Select Config root |
| SCHEMA_MISMATCH / CODEC_MISSING | Excel/code match and regeneration after changes | Confirm paths/regenerate; developer if failure |
| Generation required; full validation incomplete | Validation-only never repairs missing/stale code | Review preview, generate, revalidate |
| Inputs changed during operation | Excel/Config/Rules/Tables.json edits | Save/wait for stable files/retry; use final current result |
| Missing type / CS errors | Correct code path and complete compilable source | Developer |
| RULE_SOURCE_MISSING / RULE_SOURCE_EMPTY | Correct rules path/project rules available | Confirm/contact maintainer; do not casually disable |
| RULE_BINDING / VALIDATOR_FIELD / VALIDATOR_LOCATION | Rule binding/diagnostic code defect | Developer |
| RULE_EXCEPTION / VALIDATOR_EXCEPTION | Rule execution exception | Save full log for developer |
| File busy/permissions/output lock | Another export/writable output | Stop duplicate operation or contact maintainer |
| Game missing manifest / Hash/Schema mismatch | Complete delivery/matching game code | Developer/release owner |

Follow messages for custom codes. If uncertain, provide the cell and business expectation; do not change types/delete rows/disable rules just to suppress errors.

### 8.1 Preserve troubleshooting evidence

Export this run's diagnostic report or retain Output/Logs/&lt;run-id&gt;, with tool version, four actual paths, steps, expected/actual results, and necessary sanitized workbooks.

Generation, pre-export schema failures, and cancelled generation do not update export reports. Early permission/lock errors may also precede report writing. Check current UI exceptions/timestamps, not old success reports. See [debugging](debugging.en.md) for IDEs/stacks/diagnostics.

## 9. Routine edits

### 9.1 Data only

Save Excel → confirm paths → validate/export → fix Errors → read Warnings → deliver complete version → verify actual project loading.

### 9.2 Schema, type, or key changes

Confirm fields/types/references/game compatibility with developers before editing headers. Save, validate/export, confirm generation preview, and synchronize rules. Update game code/data together as required.

Adding an ordinary numeric column does not require hand-writing the whole model. Developers extend missing action kinds/special formats; see [project development](project-development.en.md).

### 9.3 Add/remove tables

Ensure globally unique names and identify references first. Official tables stay under samples/Data with shared Config/Rules/Output. Regenerate the entire input after additions/deletions; developers update references/rules/game reads for deletions.

A code root represents a complete configuration set. Wrong subfolder selection stops generation for changed binding/deletions and requires review. Never bypass that preview.

### 9.4 Experiments and cleanup

Included samples need no rebuild. For deliberate-error exercises copy a separate project outside the repository or into system temp. No backups/tutorial files in official Data.

Do not casually delete Config, Rules, current.json, or valid versions. Maintainers handle storage/history via [cleanup](development.en.md#packaging-and-installation); development cleanup scripts are not daily export steps.

## 10. Acceptance after every edit

- Excel saved; all four paths match this project.
- Data-only exports directly; schema changes regenerated and developer-confirmed code/rules synchronized.
- Current run explicitly succeeded, 0 Errors, Warnings read/confirmed.
- Expected table set/logical counts; one multi-row group is one record.
- Deliver this current.json and complete version files without old-table mixing.
- Project actually loaded this version; key values match edits.
- Failure was not mistaken for old-output success; troubleshooting logs saved.

Developers test new rules with valid/bad/fixed temporary copies and verify old-version retention on loading failures for new integrations. See [project development](project-development.en.md).

## 11. Further reading

- [Samples](../samples/README.en.md): Seven official workbooks and shared directories.
- [Excel syntax](config-types.en.md): Arrays, objects, dictionaries, polymorphism, multi-row lists.
- [Excel keys](table-indexes.en.md#how-to-fill-the-workbook): Single/composite markers.
- [Project development](project-development.en.md): Types, converters, rules, Unity integration.
- [Building/upgrading](development.en.md) and [debugging](debugging.en.md): Source builds and Rider/Visual Studio/Unity debugging.
- [Scope](development.en.md#scope): Version entry points and environments.
