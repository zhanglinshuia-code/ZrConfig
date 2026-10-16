# Excel primary keys, full tables, and group queries

[中文](table-indexes.md) | **English**

Configuration authors can read “How to fill the workbook” and follow the [user guide](user-guide.en.md) to generate/export. Query APIs, allocation behavior, and loading validation below are for developers.

New tables have five header rows: field, type, primary key, description, and target side; data starts on row 6. On **row 3 of the same business sheet**, mark key fields with the literal **主键**. Mark at least one column and leave other cells blank. Marked fields jointly form one unique key; querying a single marked field returns a group of records. AllTypes uses the same headers.

No extra business column, group name, or second primary-key configuration is needed.

Primary keys enforce complete-key uniqueness and generate query APIs. Positivity, ranges, and cross-table constraints are business checks; see [rule development and post-validation](rules-development.en.md). Regenerate/export after key changes. See [debugging](debugging.en.md) for queries and hot updates.

## How to fill the workbook

Open the [two-/three-component key workbook](../samples/Data/CompositeKeys.xlsx), containing `Drop` and `SkillLevel`.

`SkillLevel` is filled as follows. The leftmost row-number column is explanatory, not an Excel data column:

| Row | A | B | C | D | E |
|---|---|---|---|---|---|
| 1: Field | SkillId | Name | Level | Quality | Damage |
| 2: Type | int | string | int | int | int |
| 3: Marker | `主键` (key marker) | | `主键` (key marker) | `主键` (key marker) | |
| 4: Description | Skill identifier | Skill name | Skill level | Quality | Damage |
| 5: Target side | CS | CS | CS | CS | CS |
| 6: Data | 101 | Fireball | 1 | 1 | 100 |
| 7: Data | 101 | Fireball | 2 | 1 | 120 |
| 8: Data | 101 | Fireball | 1 | 2 | 150 |
| 9: Data | 102 | Ice arrow | 1 | 1 | 80 |

The key is `(SkillId, Level, Quality)`, with parameters ordered left to right by marked columns. Fields need not be adjacent; no additional `Id` or `GroupId` is required. The complete triple is unique; each component may repeat independently.

A two-component key marks two columns, e.g. `Drop.GroupId` and `Id`: `Get(groupId, id)` returns one record, while `GetByGroupId(groupId)` and `GetById(id)` return groups. More components follow the same rule. With one marked field, that field is unique and each group has at most one row.

Keys support top-level non-nullable primitive types and enums. Collections, objects, nullable values, JSON, table references, and expanded member paths are unsupported. Main-record key cells must be filled; whitespace is not a value, but explicit 0 is valid. Declare positivity and range requirements through C# rules.

## Generation and usage

Generate before exporting on first setup or after fields/key markers change. “Validate and export” performs both after confirming the preview. Data-only edits can export directly. See [App buttons](user-guide.en.md#31-when-to-use-each-button) and the [CLI tutorial](project-development.en.md#13-command-line-usage). Developers integrate the following APIs.

Generated `SkillLevelConfig` represents a row; `SkillLevelConfigTable` contains all rows and indexes. `ConfigSnapshot` pins a version and caches rows/tables on demand. Use generated `GetSkillLevelAsync(id, level, quality)` / `GetSkillLevel(id, level, quality)` for individual rows. Full group indexes are created only for full-table queries; see [row-level lazy loading](project-development.en.md#lazy-row-loading-and-cross-table-references).

```csharp
using ZrConfig.Models;
using ZrConfig.Unity;

var snapshot = UnityConfigRuntime.Current;
var table = snapshot.GetSkillLevelTable();

var row = table.Get(101, 1, 2);                    // One unique row, Damage = 150
bool found = table.TryGet(101, 2, 1, out var next); // One unique row, Damage = 120
var skillRows = table.GetBySkillId(101);           // Three rows
var levelRows = table.GetByLevel(1);               // Three rows
var qualityRows = table.GetByQuality(1);           // Three rows

foreach (var item in skillRows) { /* Iterates existing row objects. */ }
foreach (var item in table) { /* Iterates the whole table. */ }
int count = table.Count;
```

If the complete key is missing, `TryGet` returns false and `Get` throws `KeyNotFoundException`. Single-field queries return `ConfigRowGroup<T>`; no match yields a shared empty group. Matching rows retain source-record order.

Post-validation can use the same generated accessors: `context.GetSkillLevelTable().TryGet(101, 1, 2, out var row)`. Indexes are created on first query and reused across all rules in that export, querying the current Excel import. Cross-table access requires `DependsOn`. See [validation index tables](rules-development.en.md#use-indexed-tables-in-post-validation) for timing, completeness, and API boundaries.

## Allocations and hot updates

Snapshot initialization reads only the manifest by default. Explicit full-table loading creates the table and query indexes. Row access decodes on demand using the file's row index, without decoding every row ahead of group queries. Once built, queries reuse tables, rows, and read-only groups without temporary tables, new indexes, row copies, or `ToList()`. A single-field key shares its index with that field's groups; composite keys use value types and generated per-field comparison/hashing to avoid boxing long tuples.

Generated accessors such as `snapshot.GetHeroTable()` also reuse tables; generic access explicitly takes `GetTable<T>(codec)`. `CreateIndex` / `CreateGroupIndex` remain explicit factories and rebuild on every call. Use generated `Get` / `TryGet` / `GetByFieldName` for business lookups; do not call factories for each query or Update.

Direct foreach over a table or `ConfigRowGroup<T>` uses value-type enumerators. Enumeration through IEnumerable/IReadOnlyList, or added LINQ/ToList operations, is outside this allocation-free guarantee. Loading, reloading, exceptions, and caller code may still allocate.

After a successful hot update, new Current owns new tables/indexes. Reacquire rows, tables, and groups from it. Old snapshot queries and old row-reference access become invalid; ordinary object values already held are not updated in place.

## Validation and constraints

- In ordinary tables, duplicate complete keys or empty key components block export and report actual Excel coordinates.
- Multi-row array tables merge logical records by complete key. Repeating a key continues that record; all key cells blank may continue the preceding record. Partial keys are errors.
- Key changes affect Schema Hash. Stale source/assemblies cannot export using old indexes. Duplicate unique keys in runtime data fail table loading without publishing a partial cache. Use full or warmup reload to detect these before switching; failures retain the old snapshot.
- At least one row-3 cell must contain `主键`; leave all others blank, without descriptions or other validation tags. Row-4 descriptions do not define business rules.
- Mark keys directly in Excel; no separate file is required. If duplicate key configuration is reported for one table, a developer must remove duplicate declarations and regenerate.
- Direct `XxxConfig` references still resolve the target's unique int Id. For composite-key tables without Id, store the key values and call generated Get/TryGet.
- `[Unique(Id)]` and `[PositiveKey(Id)]` require Id to be individually unique. Do not apply them to a composite-key component that may repeat; use `[Positive(Id)]` when only positivity is required.
