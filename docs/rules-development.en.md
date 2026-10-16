# Rule development and post-validation

[中文](rules-development.md) | **English**

**Programmable post-validation is a core ZrConfig feature: catch configuration business errors during export instead of gameplay.** Developers express constraints in C#, after Excel parsing, list grouping, and reference linking. Error diagnostics block publication. Rules work on typed records and can inspect nested members/concrete action subclasses, traverse tables, and query related tables.

| Need | Capability | Official example |
| --- | --- | --- |
| Single-field names/numbers | Built-in attributes and reusable `IFieldRule<T>` | Shared item/quest name check |
| Relationships within a record | Row `Validate` | Interval ordering, action delay, buff stacks |
| Conditions across records | Table `ValidateTable` | Duplicate item/price offer combinations |
| Related-table business constraints | `DependsOn` plus post-validation, object access, or generated indexes | Referenced item enabled state; rewards versus stack limits |

Paths such as `Groups[1].Actions[0].Ratio` retain original pre-sort Excel locations. Info, Success, Warning, and Error are supported; only Error blocks publication. Rule-only edits need only another export, not an App rebuild. App/CLI use the same rules.

**Rules are debuggable.** Launch CLI export in Visual Studio / Rider with `--rules-debug`, break on executable statements in `Validate`, `ValidateTable`, or field checks, and inspect imported/nested/related data and call stacks. Dynamic compilation disables optimization and loads Portable PDB. See [Rider](debugging.en.md#3-rider-launch-debugging-from-the-ide) and [Visual Studio](debugging.en.md#4-visual-studio-launch-debugging-from-the-ide).

This guide covers placement, execution timing, Excel locations, and verification. See [tutorial chapters 3–12](project-development.en.md#3-what-are-validation-rules) for full examples, [debugging](debugging.en.md) for IDE steps, and [tool development](development.en.md) for tool changes.

Official Hero/Skill examples here use sample object-reference fields. Tutorial chapter 12 uses separate SkillId/Level exercise fields. Equal table names do not imply equal models. Check Config before copying rules and choose only one source set per project.

## 1. Projects and source ownership

All samples share `unity/ZrConfig.Sample/Assets/HotUpdate/Config` and `unity/ZrConfig.Sample/Assets/Editor/ConfigRules`. Unity compiles models/Editor rules through asmdefs; App/CLI compile the same source on export. No copies or extra Models/Validators projects are needed.

Assets/HotUpdate/Config compiles as `ZrConfig.Models`; Assets/Editor/ConfigRules uses `includePlatforms: ["Editor"]` to compile as `ZrConfig.Sample.Rules.Editor`, excluded from Players. Browse/edit/regenerate directly in Assets. See [sample source and Editor checks](../samples/README.en.md#configuration-source-and-editor-post-validation) for paths and hot-update boundaries.

Your Unity project can use:

```text
Assets/
  HotUpdate/Config/
    CustomTypes/                      Handwritten enums, classes, and structs
    Serialization/              Tool-provided support; optional project converters
    Generated/                  Models, codecs, and the single TableCodecs
  Editor/ConfigRules/            Pure C# business checks
```

Select Config root in the App's third path and ConfigRules in the fourth. CustomTypes is an organizational convention; handwritten types/converters may live at the code root or any subdirectory. Keep rules outside that root so rules depending on generated models do not enter pre-generation type compilation. Rules may live outside Assets, compiled by the tool and possibly absent from Unity's solution. For IDE feedback, use an Editor assembly or standalone rules SDK project, not a second rule copy.

This is a developer layout. The tool initializes shared serialization support; ordinary authors use provided paths/rules without implementing support or completing this programming workflow. Write special converters only when needed.

Config's assembly references Runtime; the rules Editor assembly references models, Runtime, and Validation. Match actual asmdef references and exclude Validation/business rules from Players. Do not import an old Models DLL alongside generated source with the same names.

The App ships no project Models/Validators DLL. Each export compiles `.cs` under Config and the specified rules directory, then discovers nonabstract, closed `ITableRules` implementations. Usually use `public sealed class XxxRules : TableRules<XxxConfig>` with a public parameterless constructor.

- Filenames do not bind rules; T in `TableRules<T>` selects the table.
- A table may have multiple rule classes. `IFieldRule<T>` executes only when bound by `[Check]`.
- Scanning is recursive, skipping bin, obj, dot-prefixed entries, and linked files/directories.
- A blank rules path disables business rules. A specified nonexistent/empty directory or one without usable table rules blocks export.
- Dynamic compilation ignores csproj, NuGet references, and asmdef. It references only .NET base libraries, Runtime, and Validation; no direct UnityEngine, UnityEditor, or game-assembly dependencies.
- The App does not define `UNITY_EDITOR`. Do not wrap whole rule classes in it; isolate them using Unity Editor directories/asmdefs.
- Write explicit `using System`, `System.Collections.Generic`, `System.Linq`, etc., so both Unity and the tool compile.

Rule-only changes need another export. Schema, key, CustomTypes, or converter-contract changes require generation before export. Generation/export are separate operations.

## 2. “Post” means after parsing, before publication

```text
Compile models and rules
  → Check schema, parse Excel, merge multi-row records
  → Link direct table references and determine table completeness
  → Discover rules and bind attributes, including empty tables
  → Run all field attributes
  → For each rule class, run per-row Validate, then ValidateTable
  → Only without Error: write binary, read back, calculate hashes
  → Finally replace current.json
```

`IsComplete` means parsing/linking completed, not that business checks passed. Post-methods may still run after field errors; `[Range]` does not filter bad rows out for them. Rules execute sequentially, but do not depend on relative order between classes or mutate configuration for other rules.

| Condition | Behavior |
| --- | --- |
| A row fails type parsing | No field checks for that row; table marked incomplete |
| Current table incomplete | Applicable field checks may inspect successfully parsed rows; table post-methods skipped |
| Reference target incomplete | `REF_CHECK_SKIPPED`; partial target data is not treated as a complete foreign-key set |
| DependsOn table absent/incomplete | Both Validate and ValidateTable skipped for that class; `BUSINESS_CHECK_SKIPPED` |
| Direct reference linking fails | `TABLE_REFERENCE`; incompleteness propagates through references |
| Attribute/another rule reports Error | Collect other independently decidable problems, then block publication |
| Row method throws | `VALIDATOR_EXCEPTION` with row location; continue other rows |
| Table method throws | `VALIDATOR_EXCEPTION` located at the table, not automatically a row |
| Only Info / Success / Warning | Publication allowed; read warnings. Success means only a particular check passed |

`BUSINESS_CHECK_SKIPPED` is Error for a completely absent declared dependency. For incomplete parsing it is usually Warning because the original parse Error already blocks publication. Skipped does not mean passed.

## 3. Choose a check

| Requirement | Implementation |
| --- | --- |
| Required/unique/range on top-level fields | Built-in attributes |
| Reusable single-field condition | `IFieldRule<T>` + `[Check]` |
| Multiple fields, objects, or arrays in one row | `Validate(row, context)` |
| Table-wide combination uniqueness, totals, counts, order | `ValidateTable(rows, context)` |
| Business checks reading another table | `[DependsOn(typeof(TargetConfig))]` + post-methods |
| Game query indexes/composite keys | Literal `主键` on Excel row 3; see [keys](table-indexes.en.md) |

### Built-in attribute reference

Put attributes on rule classes, not Generated. Field names must be top-level logical fields; inspect nested members in post-methods.

| Attribute | Semantics and boundaries |
| --- | --- |
| `Required(field)` | Checks import blank markers, including unquoted whitespace; explicit 0, false, and `[]` are not blank |
| `Unique(field)` | Compares nonblank values using default equality; no automatic string Trim/case folding; not array-content deduplication |
| `Positive(field)` | Filled numeric value > 0 |
| `PositiveKey(field)` | Required + Unique + Positive; creates no runtime index |
| `Range(field, min, max)` | Filled number in the inclusive interval; bounds must be finite and ordered |
| `Reference(field, typeof(Target), targetField)` | Property types must match; source value must exist among filled target values |
| `Check(field, typeof(MyRule))` | MyRule must be parameterlessly instantiable and implement matching `IFieldRule<T>` |

Positive, PositiveKey, and Range currently support only ordinary `int`, `long`, `float`, and `double`. Use matching generic field rules or post-methods for `decimal`, `uint`, `int?`, etc.

Unique, Positive, Range, and Check skip blank values; add Required separately. Reference rejects blanks by default; `AllowDefaultValue = true` permits blank/default as no reference. Reference checks existence only, without target uniqueness or object linking. Enforce target uniqueness separately.

Direct `SkillConfig` references treat 0/blank as null, so `[Required]` rejects them; explicit 0 in an ordinary int field passes Required. Business rules still validate explicit string/collection contents. `[ConfigRequired]` is a parsing constraint on handwritten members, distinct from table-field `[Required]`.

Composite keys guarantee only complete-combination uniqueness. Do not apply Unique/PositiveKey to a component allowed to repeat.

## 4. Row and table examples

The following targets HeroConfig from repository `TableReferences.xlsx` (Id, Name, Skill, OtherSkill, Mentor). Place it in the actual Rules directory for discovery. It demonstrates rules; adding these business constraints to official samples is not required.

```csharp
using System;
using System.Collections.Generic;
using ZrConfig.Models;
using ZrConfig.Validation;

[Required(nameof(HeroConfig.Name))]
public sealed class HeroNameRules : TableRules<HeroConfig>
{
    protected override void Validate(HeroConfig row, ValidationContext context)
    {
        if (context.IsEmpty(row, nameof(HeroConfig.Name))) return;
        if (row.Name.Length > 32)
            context.Error(row, nameof(HeroConfig.Name),
                "Name must not exceed 32 characters.", "HERO_NAME_LENGTH");
    }

    protected override void ValidateTable(
        IReadOnlyList<HeroConfig> rows, ValidationContext context)
    {
        var groups = new Dictionary<string, List<HeroConfig>>(
            StringComparer.OrdinalIgnoreCase);
        foreach (HeroConfig row in rows)
        {
            if (context.IsEmpty(row, nameof(HeroConfig.Name))) continue;
            string key = row.Name.Trim();
            if (!groups.TryGetValue(key, out var group))
                groups.Add(key, group = new List<HeroConfig>());
            group.Add(row);
        }
        foreach (var group in groups.Values)
        {
            if (group.Count < 2) continue;
            foreach (HeroConfig row in group)
                context.Error(row, nameof(HeroConfig.Name),
                    "Duplicate name after trimming and ignoring case.", "HERO_NAME_DUPLICATE");
        }
    }
}
```

Table methods receive merged logical records, not physical Excel rows. ValidateTable runs on empty tables too; check Count before `rows[0]`. ValidationContext currently has no row-free table Error overload. Do not invent a row to report “at least one row required”; extend diagnostics and add tests if needed.

## 5. Cross-table checks and object references

A Reference attribute alone needs no extra DependsOn. Business methods using `GetTable<T>()`, generated `GetXxxTable()`, or target members must declare dependencies.

An extra check for the bidirectional sample: a hero's selected skill must reference that hero back. Null references remain valid here.

```csharp
using ZrConfig.Models;
using ZrConfig.Validation;

[DependsOn(typeof(SkillConfig))]
public sealed class HeroOwnerRules : TableRules<HeroConfig>
{
    protected override void Validate(HeroConfig row, ValidationContext context)
    {
        var skill = row.Skill;
        if (skill == null) return;
        if (!object.ReferenceEquals(skill.Hero, row))
            context.Error(row, nameof(HeroConfig.Skill),
                "The selected skill must reference the current hero back.", "SKILL_OWNER");
    }
}
```

Bidirectional/self references are supported. Avoid unbounded recursive traversal; track visited objects or define explicit termination for long chains.

See the [integer foreign-key example](project-development.en.md#8-cross-table-post-validation). Direct references are already linked, so access `row.Skill`. To find other records by simple/composite keys, use generated table APIs below.

DependsOn neither means target business rules passed nor specifies execution order. Check target values' usable ranges yourself. Separate local and cross-table rules so local checks can still run when dependencies fail.

### Use indexed tables in post-validation

Generated `GetXxxTable()` accessors are shared by `ValidationContext` and runtime snapshots. For example, inside a method declaring `[DependsOn(typeof(SkillLevelConfig))]`:

```csharp
var levels = context.GetSkillLevelTable();
bool found = levels.TryGet(101, 1, 2, out var level);
var group = levels.GetBySkillId(101);
```

First access builds a generated table and key/group indexes from the current Excel import. All rules, row Validate, and ValidateTable in that export share one instance; later access does not rebuild. Unaccessed tables get no index table. Each export creates a fresh context, not indexes from an old published version. Caches are released with the context; never retain tables/contexts statically.

Queries return original imported rows, preserving `context.Error`, `IsEmpty`, and Excel locations. Empty tables can be queried. Missing TryGet returns false, Get throws, groups return empty. Incomplete current/dependency tables still skip post-methods. Explicitly requesting a missing/incomplete table throws `TableUnavailableException` even without a dependency declaration; partial data is never queried. Business Error does not mean incomplete parsing, so indexed values still need checking.

`context.GetTable<T>()` returns this import's `ConfigTable<T>` and shares the table/indexes with typed accessors. Generated methods pass codecs through Runtime's `IConfigTableProvider`; models do not reference Validation, and Players need no validation assembly. Manually constructed ValidationContext requires a factory for this run's tables.

Do not `new XxxConfigTable(...)`, CreateIndex, or ToDictionary per row; they rebuild. If keys are undeclared or comparison differs, build one custom dictionary in ValidateTable and handle duplicates. ValidateTable runs after all row methods of that class, so it cannot prepare indexes for earlier row calls. Every table declares keys on Excel row 3; `[PositiveKey]` is only business validation.

## 6. Diagnostic paths, blanks, and exceptions

```csharp
context.Error(row, nameof(AllTypesConfig.Id), "The identifier violates business requirements.", "MY_ID");
context.Warning(row, "Settings.MaxTargets", "High target count; please confirm.", "TARGET_NOTICE");
context.Error(row, $"Actions[{i}].Delay", "Delay must not be negative.", "ACTION_DELAY");
bool omitted = context.IsEmpty(row, "Settings.MaxTargets");
```

These are method fragments: row must be a real imported record of the corresponding table, and i a valid array index. See [AllTypesRules.cs](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/AllTypesRules.cs) for complete nested checks.

| Location | Second argument |
| --- | --- |
| Top-level field | `nameof(AllTypesConfig.Id)` |
| Object member | `Settings.MaxTargets` |
| Array element member | `Actions[1].Delay` |
| Three-level multi-row list | `Phases[1].Groups[0].Actions[0].Delay` |

The first argument is always the complete row, not Settings, an action, or a new copy. Use zero-based indexes after sorting; the tool maps to original physical cells. JSON members locate the containing cell. Paths must exist. Do not use input paths like `Actions[].Delay` or traverse references with `Skill.Name`; report the source row's Skill or the target row's Name instead.

`VALIDATOR_LOCATION` means the row is not from this import; `VALIDATOR_FIELD` means the path is missing. `IsEmpty` does not validate spelling and may return false for unknown paths; do not use it to prove field existence.

The fourth argument of each reporting method is a stable code, default CUSTOM; field rules default FIELD_RULE. Use Error for expected business failures, not throw. Exceptions expose programming defects and retain full stacks/inner exceptions.

### Diagnostic levels

Since App 1.3.5, row/field rules provide four levels, using the same Assets/ZrConfig/Validation source as Unity. `Info` and `Success` are real levels; no manual message prefix is needed.

| Method | Purpose | Publication |
| --- | --- | --- |
| `context.Info(row, field, message, code)` | Status, count summaries, and notices requiring no action | Allowed |
| `context.Success(row, field, message, code)` | Explicitly record a passed check | Allowed |
| `context.Warning(row, field, message, code)` | Configuration worth checking but export may continue | Allowed; counted as warning |
| `context.Error(row, field, message, code)` | Must-fix problem | Blocked; counted as error |

`FieldRuleContext` has the same method names with `(message, code)`, automatically carrying row/field. Table summaries may attach to a real record's field; do not access rows[0] for empty tables. Every level validates locations and enters logs/structured reports. Successes remain even with other errors; one success is not whole-export success.

See [item/shop/quest examples](../samples/README.en.md#post-validation-and-four-diagnostic-levels) for field/row/table/cross-table checks and triggers. `DiagnosticSeverity` retains Info=0, Warning=1, Error=2 and adds Success=3. Interpret JSON by level meaning, not numeric ordering.

For bilingual messages use `ConfigText.Get("中文", "English")` with `ZrConfig.Runtime`. Keep codes, fields, and table names stable.

## 7. Verify your rules

Copy exercise data to system temp or outside the repository, avoiding intentional damage to `samples/Data`. At minimum:

1. Export valid data and record current.json.
2. Break one condition; verify expected Code, Table, Sheet, Row, Column, Field, and unchanged current.json.
3. Fix and export successfully; check new versionId and game loading.
4. Test blank versus explicit 0/false, boundaries, duplicates, empty arrays, and sorted indexes.
5. For cross-table rules test absent targets, target parse failures, and duplicate target keys. Independent local checks should still follow their contract.
6. Warnings must keep success true; exceptions must preserve call stacks and reject publication.

Do not merely check that text appeared in a log: warnings also produce text and old releases may remain on disk. Check exit code, success, locations, and manifest switching together. See [CLI usage](project-development.en.md#13-command-line-usage).

## 8. Compilation, caches, and debugging

```bash
# Run from the repository root; generate Config first
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- export samples/Data samples/Output unity/ZrConfig.Sample/Assets/HotUpdate/Config unity/ZrConfig.Sample/Assets/Editor/ConfigRules --rules-debug
```

`--rules-debug` disables dynamic optimization, defines DEBUG, and emits/loads Portable PDB. Tool-project Debug cannot replace it. CLI does not wait for attachment; launch short tasks directly with IDE Debug. See [Rider / Visual Studio](debugging.en.md).

Full-project compilation cache lives in Output/.rules-cache. Generation-preview type discovery uses a bounded in-process cache, no longer Config/.types-cache. Fingerprints include content, absolute paths, mode, language, and tool/shared-assembly versions. Cache entries hold only compiled bytes/diagnostics; each run creates a new load context/rule instances and requests unload afterward. Compile failures never fall back to old rules. Do not retain dynamic types/instances through static events, background threads, or external objects.

The optional rules SDK suits projects without Unity solutions. Build App with `bash scripts/dotnet.sh build src/ZrConfig.App -c Release -p:NuGetAudit=false -p:UsedAvaloniaProducts=`, then run `bash scripts/package-rules-sdk.sh`, optionally specifying the actual tool DLL directory and a new output directory. Default artifacts/rules-sdk includes Runtime/Validation, Config/Rules, and compilation projects; generate template HeroConfig first or use your real model. Existing output directories are rejected; never overwrite handwritten rules. Normal Unity/Rider/Visual Studio workflows need no repeated SDK DLL imports.

## Validation-only mode and safe cancellation

Rules should only read current data and report diagnostics, not write workbooks, project source, or releases. Validation-only's no-write contract covers the tool, not arbitrary rule-code side effects. Cancellation is checked between rows, attributes, and table rules; executing custom rules must return before the next safe point. Never wait indefinitely or activate versions inside rules.
