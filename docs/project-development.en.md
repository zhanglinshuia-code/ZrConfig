# Project development: business extensions and Unity integration

[中文](project-development.md) | **English**

See [multi-language generation and JSON](server-generation.en.md) for server generation and Binary/JSON/dual export.
This developer tutorial retains complete examples for types, converters, post-validation, CLI, and Unity. Everyday authors need only the [user guide](user-guide.en.md); this is not a prerequisite. See [building/upgrading](development.en.md) for source startup/installers.

## Responsibilities and existing implementation

| Content | Owner | When needed |
| --- | --- | --- |
| Row models, codecs, registry | Tool generates; developers integrate/build | Initial integration/schema changes |
| Business enums/classes/structs | Developers | Built-ins cannot express project shapes |
| Generic support source | Tool supplies; tool/project maintainers upgrade | Initial generation fills files; recorded unchanged versions auto-upgrade |
| Special converters | Developers as needed | Actual special text/binary conventions |
| Business rules | Developers maintain; tool executes | Extra business constraints |
| Excel, fixes, export acceptance | Authors | Everyday work |

Config uses CustomTypes for handwritten definitions, Serialization for managed support/optional converters, Generated for generator output. Handwritten source can be at the selected root or any subdirectory; CustomTypes is not required. Generation excludes its Generated output; bin/obj/dot-prefixed entries/links are not compiled. Being project-local does not mean authors must write serialization.

Generic support remains managed in Config/Serialization. Generation supplies missing files/upgrades recorded unchanged ones; handmade/unrecognized old versions are preserved with merge notices. Export checks versions; see [upgrades](development.en.md#support-source-upgrades).

## Reading route

| Work | Chapters |
| --- | --- |
| Business types | [1. Classes](#1-what-is-a-handwritten-class), [2. Enums/polymorphism](#2-enums-structs-and-polymorphism) |
| Post-validation | [3. Checks](#3-what-are-validation-rules) through [9. Objects/arrays](#9-object-members-and-array-elements) |
| Object references | [10. Direct references](#10-direct-table-references-avoid-manual-lookups) |
| Custom input/encoding | [11. Converters](#11-extend-special-formats-with-converters) |
| Complete implementation | [12. Full exercise](#12-complete-exercise-project) |
| Automated export | [13. CLI](#13-command-line-usage) |
| Unity loading/updates | [14. Unity](#14-unity-runtime-integration) |
| Rider/Visual Studio | [Debugging](debugging.en.md) |

Chapter 12 Hero/Skill uses teaching fields SkillId/Level, unlike official TableReferences Skill/Mentor object references. Extract exercises outside the repository; never mix with samples/Data. Official instructions live in [samples/README.en.md](../samples/README.en.md).

All examples use five headers: field/type/key/description/side, data from row 6. Mark at least one row-3 column with literal 主键, leaving others blank; see [headers](user-guide.en.md#41-always-use-five-header-rows).

## 1. What is a handwritten class?

### 1.1 A class describes a group of data

A hero may have battle settings beyond one integer: maximum targets and whether interruption is allowed.

Together those values form a battle-settings object.

**Class:** Defines its members, like a blank registration card.

**Object:** One filled instance of that card.

**Member:** An entry such as MaxTargets or Interruptible.

**Handwritten class:** You define the template in C#; the tool cannot infer your game's members.

**Generated class:** The tool creates a whole-row model such as HeroConfig from Excel.

| Name | Author | Location | Meaning |
| --- | --- | --- | --- |
| BattleSettings | You | Config/CustomTypes | Battle settings inside a hero |
| HeroConfig | Tool | Config/Generated | One complete Excel hero record |
| HeroRules | You | Rules | Hero validation logic |
| HeroConfigCodec | Tool | Config/Generated | Hero binary reading/writing |

Record/query/extensions/codec share Generated/HeroConfig.g.cs but remain separate types. All tables share TableCodecs.g.cs. See [organization/upgrades](development.en.md#generated-file-organization).

Do not handwrite another HeroConfig in CustomTypes; it would collide.

### 1.2 Your first class

Create Config/CustomTypes/BattleSettings.cs.

```csharp
namespace Tutorial.Config
{
    public sealed class BattleSettings
    {
        public int MaxTargets = 1;
        public bool Interruptible = true;
    }
}
```

Line-by-line explanation:

| Code | Meaning |
| --- | --- |
| namespace Tutorial.Config | Groups names to avoid collisions |
| public | Visible to other code |
| sealed class BattleSettings | Defines BattleSettings without further inheritance |
| public int MaxTargets = 1 | Integer member defaulting to 1 |
| public bool Interruptible = true | Boolean switch defaulting to interruptible |
| `{` and `}` | Start/end of a block |
| `;` | Ends a declaration |

Save as text ending .cs in Rider/VS Code/etc., not BattleSettings.cs.txt.

public int MaxTargets is a field. A publicly readable/writable property also works:

```csharp
public int MaxTargets { get; set; } = 1;
```

Choose one form; do not declare duplicate member names in one class.

### 1.3 Use the class in Excel: split members

Add two Hero columns:

| Excel row | Id | Settings.MaxTargets | Settings.Interruptible |
| --- | --- | --- | --- |
| 1 | Id | Settings.MaxTargets | Settings.Interruptible |
| 2 | int | Tutorial.Config.BattleSettings | Blank |
| 3 | `主键` (literal marker) | | |
| 4 | Hero identifier | Maximum targets | Interruptible |
| 5: Side | CS | CS | CS |
| 6 | 1001 | 3 | false |

“Blank” is explanatory: leave the actual cell empty, do not type that word.

Settings.MaxTargets means MaxTargets inside Settings; generated models expose row.Settings.MaxTargets.

**Row 2 declares the group's root BattleSettings, not int/bool separately.** C# already defines member types. Fill one group type cell and leave others blank, or repeat the same root type.

After type edits: save .cs → save Excel → Generate configuration code → Validate and export.

### 1.4 Alternative: whole object in one cell

Put the whole object in Settings if preferred, keeping Id as this example's key:

| Row | Id | Settings |
| --- | --- | --- |
| 1 | Id | Settings |
| 2 | int | Tutorial.Config.BattleSettings |
| 3 | `主键` (literal marker) | |
| 4 | Hero identifier | Battle settings |
| 5: Side | CS | CS |
| 6 | 1001 | {"MaxTargets":3,"Interruptible":false} |

This is JSON; explicit json<Tutorial.Config.BattleSettings> also works.

Do not combine whole-cell Settings and expanded Settings.MaxTargets for the same field. Choose one layout.

Split columns make each value visible for beginners editing complex battle data.

### 1.5 Defaults

MaxTargets = 1 defaults to at most one target.

JSON {"Interruptible":false} omits MaxTargets, retaining 1. Blank optional split members likewise retain construction defaults.

Explicit 0/false overrides defaults and is not blank.

Defaults specify missing-value behavior, not business validity. Add required declarations when explicit input is mandatory, even with defaults.

### 1.6 Explicit object members: ConfigRequired

Change the class to:

```csharp
using ZrConfig.Runtime;

namespace Tutorial.Config
{
    public sealed class BattleSettings
    {
        [ConfigRequired]
        public int MaxTargets = 1;

        public bool Interruptible = true;
    }
}
```

Put ConfigRequired on **handwritten fields/properties**. Even with default 1, omitted MaxTargets in Excel/JSON is rejected.

It checks explicit input, not validity. 0 is filled; positivity needs chapter 9's business checks.

This replaces the earlier example. Chapter 12 uses optional defaults; never include both class versions together.

### 1.7 Class requirements

- Types/exported members must be public.
- Classes need public parameterless construction; ordinary public classes usually have it unless constructors are declared.
- Fields must be writable; properties need public get/set.
- No read-only/write-only properties, indexers, init properties, or generic data classes.
- Constructors/setters must not play sounds, spawn characters, or access scenes.
- Pure data definitions reference neither UnityEngine/UnityEditor nor game assemblies.
- Inherited members can export, but do not hide base members with same-name declarations.

Game code implements behavior. An exported damage action stores Delay/Ratio; export does not damage enemies.

## 2. Enums, structs, and polymorphism

### 2.1 Enums: choose a declared name

Create Config/CustomTypes/Quality.cs:

```csharp
namespace Tutorial.Config
{
    public enum Quality
    {
        None = 0,
        Rare = 1,
        Legendary = 2
    }
}
```

These are fixed options: None, Rare, Legendary.

Excel type is Tutorial.Config.Quality; enter Rare, not arbitrary Chinese text or numeric 1.

Match declared case: rare is not Rare. Unknown names fail.

Blank enums require a zero-valued member such as None. Arrays use Tutorial.Config.Quality[] with [Rare;Legendary].

Regenerate/export after adding/removing/changing enum members.

### 2.2 Structs also group values

Create Config/CustomTypes/NumberRange.cs:

```csharp
namespace Tutorial.Config
{
    public struct NumberRange
    {
        public int Min;
        public int Max;
    }
}
```

It represents a range, e.g. minimum 1, maximum 10.

Split Range.Min/Range.Max, with group type Tutorial.Config.NumberRange on row 2.

C# class/struct memory behavior differs; for authoring both group members. Use classes for ordinary complex objects, structs for small values.

The tool cannot infer Min ≤ Max; add post-validation:

```csharp
if (row.Range.Min > row.Range.Max)
    context.Error(row, "Range.Max", "Maximum must not be less than minimum.", "RANGE_ORDER");
```

Put this in the corresponding table's Validate; that model must actually have Range.

### 2.3 Polymorphism: different action kinds in one field

Damage and AddBuff are actions with different parameters.

Create Config/CustomTypes/BattleAction.cs:

```csharp
using ZrConfig.Runtime;

namespace Tutorial.Config
{
    public abstract class BattleAction
    {
        public float Delay;
    }

    public sealed class Damage : BattleAction
    {
        public float Ratio = 1;
        public bool CanCrit;
    }

    public sealed class AddBuff : BattleAction
    {
        [ConfigRequired]
        public int BuffId;

        public int Stacks = 1;
    }
}
```

- BattleAction is the shared template; all actions have Delay.
- Damage has Ratio and CanCrit.
- AddBuff has BuffId and Stacks.
- `: BattleAction` means a subtype of BattleAction.
- abstract prevents instantiating the base directly; choose a concrete kind.

A single-cell JSON array can be:

```json
[{"$type":"Damage","Delay":0.2,"Ratio":1.5,"CanCrit":true},{"$type":"AddBuff","Delay":0.5,"BuffId":2001,"Stacks":2}]
```

Column type: json<Tutorial.Config.BattleAction[]>.

$type identifies the concrete kind Damage/AddBuff, not abstract BattleAction. Use fully qualified names on collisions.

Missing AddBuff.BuffId fails ConfigRequired. Explicit 0 counts as input; business rules decide validity.

To add Heal : BattleAction, write the class and regenerate. Heal then works without a manual App registry.

### 2.4 Expand an action array into rows

Below are five headers, with 主键 under Id on row 3. Fill continuously from A1; explanatory row numbers are not data columns.

| Row | Id | Name | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Id | Name | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
| 2 | int | string | Tutorial.Config.BattleAction[] | | | | | | |
| 3 | `主键` (literal marker) | | | | | | | | |
| 4 | Hero identifier | Name | Action order | Action kind | Delay | Ratio | Critical hits? | Buff identifier | Stacks |
| 5: Side | CS | CS | CS | CS | CS | CS | CS | CS | CS |

Enter data from row 6:

| Id | Name | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1001 | Young swordsman | 1 | Damage | 0.2 | 1.5 | true | | |
| 1001 | | 2 | AddBuff | 0.5 | | | 2001 | 2 |
| 1002 | Young mage | 1 | Damage | 0 | 2 | false | | |

These three physical rows yield **two hero records**: first with two actions, second with one.

Rules:

1. Only expanded [] columns merge equal complete keys; this example uses Id. Ordinary tables remain one record/row.
2. First row requires a valid complete key. Continuations repeat all components or leave all blank for the preceding valid record; no partial composites. Beginners should repeat complete keys explicitly.
3. Put Name/Level/main fields on the first row for Id. Continue blank or identical, never conflicting.
4. $order sorts input and is not generated. 1/2 mean first/second action.
5. If declared, every valid element needs a unique positive order. Simple arrays without children may omit it and use physical order.
6. Damage fills only its parameters, leaving BuffId/Stacks blank; AddBuff follows the same rule.
7. All action columns blank means no element on that row. A hero with no elements gets an empty array.
8. Never merge data cells or sort only some columns, which can misalign Id/parameters.

Indexes are zero-based: Actions[0], Actions[1]; diagnostics use these indexes.

### 2.5 Nested lists: groups containing actions

A hero may contain multiple groups, each with multiple actions.

Handwritten type:

```csharp
using System;

namespace Tutorial.Config
{
    public sealed class ActionGroup
    {
        public string Name = "Default action group";
        public BattleAction[] Actions = Array.Empty<BattleAction>();
    }
}
```

Row-1 column names may be:

```text
Id
Groups[].$order
Groups[].Name
Groups[].Actions[].$order
Groups[].Actions[].$type
Groups[].Actions[].Ratio
Groups[].Actions[].BuffId
```

Row 2: int for Id, Tutorial.Config.ActionGroup[] in one Groups type cell, others blank. Row 3 marks only Id; row 4 descriptions, row 5 sides. Data below starts at row 6:

| Id | Groups[].$order | Groups[].Name | Groups[].Actions[].$order | Groups[].Actions[].$type | Groups[].Actions[].Ratio | Groups[].Actions[].BuffId |
| --- | --- | --- | --- | --- | --- | --- |
| 1001 | 1 | First group | 1 | Damage | 1.5 | |
| 1001 | 1 | | 2 | AddBuff | | 2001 |
| 1001 | 2 | Second group | 1 | Damage | 2 | |

- First two rows have parent order 1 and share a group.
- Third row's parent order 2 selects the second group.
- Child orders restart per parent; the second group may also have child 1.
- Parents containing child lists must declare/fill order to identify shared parent objects.

Three levels follow the same rule, e.g. Phases[].Groups[].Actions[]. Learn one level first; AllTypes Groups/Phases show real examples.

## 3. What are validation rules?

Rules define conditions for export, for example:

- Hero name required.
- Identifiers unique.
- Level from 1 to 100.
- High-level heroes need skills.
- Selected skills need sufficient damage.
- Action delay nonnegative.

Basic parsing checks conversion to declared types, so “apple” in int fails. A level cap of 100 is game-specific and needs a rule.

### 3.1 Three checking methods

| Method | Checks | Placement |
| --- | --- | --- |
| Built-in field attributes | Required, unique, range, foreign-key existence | Above handwritten table rule class |
| Custom IFieldRule<T> | Reusable single-field condition | Separate rule class, bound via Check |
| Validate / ValidateTable | Multiple fields, tables, references, object/array business logic | Table-rule methods |

“Post” means after Excel becomes objects, still before publishing, never after release.

Execution order:

```text
Read schema/data
  → Construct objects and link direct references
  → Field attributes across rule classes
  → Each class's Validate for every record
  → That class's ValidateTable
  → Only without errors: write/read-back/activate
```

Do not depend on inter-class ordering or mutate data for another rule. Check/report only.

### 3.2 Empty table-rule template

With HeroConfig generated, create Rules/HeroRules.cs:

```csharp
using ZrConfig.Models;
using ZrConfig.Validation;

public sealed class HeroRules : TableRules<HeroConfig>
{
}
```

TableRules<HeroConfig> binds to Hero by generic type, not filename.

The class must be instantiable; public sealed class without parameterized constructors is simplest.

Rules are auto-discovered; no central handwritten list.

Multiple classes may target HeroConfig and all execute. Do not copy old files and accidentally keep duplicate active classes.

### 3.3 Attributes go above rule classes

Correct:

```csharp
using ZrConfig.Models;
using ZrConfig.Validation;

[Required(nameof(HeroConfig.Name))]
public sealed class HeroRules : TableRules<HeroConfig>
{
}
```

Never put Required in HeroConfig.g.cs; regeneration replaces managed code.

nameof(HeroConfig.Name) evaluates to "Name" and catches misspellings at compile time.

typeof(SkillConfig) passes the SkillConfig type to a rule.

## 4. Built-in field rules

These are attributes added above existing classes, not instructions to create repeated HeroRules classes.

### 4.1 Required: explicit input

```csharp
[Required(nameof(HeroConfig.Name))]
[Required(nameof(HeroConfig.Level))]
```

| Input | Required result |
| --- | --- |
| Blank Name | Error |
| Blank Level | Error |
| Explicit Level 0 | Filled; Required passes |
| Explicit bool false | Filled; Required passes |
| Explicit [] | Filled; nonempty-array requirements need another check |

Required uses original blank markers, including unquoted whitespace. It means neither positive nor nonempty array. Check explicit string literals/length/surrounding spaces separately. Nullable null and direct reference 0 mean absent and fail.

### 4.2 Unique: no duplicates

```csharp
[Unique(nameof(HeroConfig.Name))]
```

Equal values across rows fail. Unique alone skips blanks; combine Required for mandatory unique input.

Strings compare original values without trimming/case folding. Use ValidateTable for case-insensitive or composite uniqueness.

Multi-row records merge first, so two actions of one hero are not duplicate hero IDs.

Unique compares fields across records, not elements inside one array. Complex object/array-content uniqueness needs custom post-checks.

### 4.3 Positive: greater than zero

```csharp
[Positive(nameof(HeroConfig.Level))]
```

1 passes, 0/-1 fail. Positive skips blanks; add Required when needed.

For “zero allowed, negative forbidden,” use Range/custom checks, not Positive.

### 4.4 PositiveKey: required, unique, positive

```csharp
[PositiveKey(nameof(HeroConfig.Id))]
```

Equivalent to:

```csharp
[Required(nameof(HeroConfig.Id))]
[Unique(nameof(HeroConfig.Id))]
[Positive(nameof(HeroConfig.Id))]
```

Useful for positive integer IDs. It creates no runtime index; Excel row-3 主键 causes generated tables to index on load. Repeating composite components must not get Unique/PositiveKey; use Positive alone for positivity.

### 4.5 Range: inclusive lower/upper bounds

```csharp
[Required(nameof(HeroConfig.Level))]
[Range(nameof(HeroConfig.Level), 1, 100)]
```

1/50/100 pass; 0/101 fail; Required handles blanks.

Minimum cannot exceed maximum.

**Built-in Positive/PositiveKey/Range bind only ordinary int/long/float/double properties.** Exportable byte/uint/decimal/int?/enums cannot reuse these directly. Use matching IFieldRule<T>/Validate.

For nonnegative decimal prices:

```csharp
if (row.Price < 0m)
    context.Error(row, nameof(MyConfig.Price), "Price must not be negative.", "PRICE_NEGATIVE");
```

Replace illustrative MyConfig with your actual model; Price must be decimal.

### 4.6 Reference: an ID must exist in another table

Assume Hero has int SkillId and Skill has int Id.

```csharp
[Reference(nameof(HeroConfig.SkillId), typeof(SkillConfig), nameof(SkillConfig.Id))]
```

Hero.SkillId must exist in Skill.Id; 2001 requires target 2001.

C# types must match exactly, not int versus long.

Blanks fail by default. To allow 0/blank as no skill:

```csharp
[Reference(nameof(HeroConfig.SkillId), typeof(SkillConfig), nameof(SkillConfig.Id),
    AllowDefaultValue = true)]
```

AllowDefaultValue permits the type's default, 0 for int. Other nonzero values must still exist.

Reference checks existence only, not unique target IDs/damage, and does not turn SkillId into an object. Targets commonly also use PositiveKey.

For mandatory nonzero skills, combine Required + Positive + Reference without AllowDefaultValue.

### 4.7 Check: bind your field rule

```csharp
[Check(nameof(HeroConfig.Name), typeof(DisplayNameRule))]
```

The next chapter implements name checks. DisplayNameRule alone does not run on Hero.Name without Check binding.

### 4.8 DependsOn: declare other tables read by post-checks

```csharp
[DependsOn(typeof(SkillConfig))]
```

This is not a field check. It declares that the class's Validate/ValidateTable reads Skill.

Add two DependsOn attributes when reading two tables.

Missing/incomplete dependencies skip all this class's post-checks with a diagnostic, including local-only conditions. Split independent local rules into a separate class without DependsOn.

DependsOn guarantees complete imported data, not absence of field-rule errors. Defend against duplicates/nulls/out-of-range values yourself.

### 4.9 Required versus ConfigRequired

| Name | Placement | Checks | Example |
| --- | --- | --- | --- |
| Required | Above TableRules<T> class | Top-level field filled | Hero.Name |
| ConfigRequired | Handwritten class/struct member | Member explicitly provided | AddBuff.BuffId |

Required("Settings.MaxTargets") is not supported nested-attribute syntax. Attributes bind top-level properties; use ConfigRequired or member-path checks in Validate.

## 5. Reusable custom field rules

Write a string-length check once and bind it to many tables rather than repeat it.

Create Rules/DisplayNameRule.cs:

```csharp
using System;
using ZrConfig.Validation;

public sealed class DisplayNameRule : IFieldRule<string>
{
    public void Validate(string value, FieldRuleContext context)
    {
        if (value.Length > 16)
            context.Error("Name must not exceed 16 characters.", "NAME_TOO_LONG");

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            context.Error("Name must not have leading/trailing spaces.", "NAME_SPACE");

        foreach (char character in value)
        {
            if (!char.IsControl(character)) continue;
            context.Error("Name must not contain newline/tab characters.", "NAME_CONTROL");
            break;
        }
    }
}
```

Explanation:

- IFieldRule<string> checks string fields.
- value is the parsed cell string.
- context.Error reports that cell's error.
- The tool supplies Excel coordinates automatically.

Add these to HeroRules and SkillRules:

```csharp
[Required(nameof(HeroConfig.Name))]
[Check(nameof(HeroConfig.Name), typeof(DisplayNameRule))]
```

```csharp
[Required(nameof(SkillConfig.Name))]
[Check(nameof(SkillConfig.Name), typeof(DisplayNameRule))]
```

Both tables share the same name check.

### 5.1 Probability example

Use basis points: 0=0%, 10000=100%, 2500=25%.

Create Rules/ProbabilityRule.cs:

```csharp
using ZrConfig.Validation;

public sealed class ProbabilityRule : IFieldRule<int>
{
    public void Validate(int value, FieldRuleContext context)
    {
        if (value < 0 || value > 10000)
            context.Error("Probability must be between 0 and 10000.", "PROBABILITY_RANGE");
    }
}
```

Bind to Hero.Rate:

```csharp
[Required(nameof(HeroConfig.Rate))]
[Check(nameof(HeroConfig.Rate), typeof(ProbabilityRule))]
```

Rate must exist and be int.

Match IFieldRule<int>/decimal/int? to the exact field type. Mismatches produce RULE_BINDING, not ordinary value errors.

### 5.2 Why Required is still needed

Check skips original blanks. Mandatory valid probability needs both Required and Check.

Field rules may call context.Warning("message", "code") without row/field because they are already bound.

Do not access scenes or retain run data statically. Simple current-check state is allowed, but never retain previous-export data globally.

## 6. Row post-validation

Attributes express one-field constraints. Validate is clearer for conditions spanning columns.

### 6.1 Complete small example

Hero has Level/SkillId. Heroes at level 90 or above require a skill.

Create/edit Rules/HeroRules.cs:

```csharp
using ZrConfig.Models;
using ZrConfig.Validation;

[PositiveKey(nameof(HeroConfig.Id))]
[Required(nameof(HeroConfig.Level))]
[Range(nameof(HeroConfig.Level), 1, 100)]
public sealed class HeroRules : TableRules<HeroConfig>
{
    protected override void Validate(HeroConfig row, ValidationContext context)
    {
        if (context.IsEmpty(row, nameof(HeroConfig.Level)))
            return;

        if (row.Level >= 90 && row.SkillId == 0)
        {
            context.Error(row, nameof(HeroConfig.SkillId),
                "Heroes at level 90 or above must have a skill.", "HIGH_LEVEL_SKILL");
        }
    }
}
```

row is the current hero; row.Level is its level.

if executes the block when the condition holds.

&& requires both conditions.

== compares equality; = assigns.

return ends this method/current-record check, not the entire export.

This replaces another HeroRules version; do not keep duplicate classes.

### 6.2 Error's four arguments

```csharp
context.Error(row, nameof(HeroConfig.SkillId),
    "Heroes at level 90 or above must have a skill.", "HIGH_LEVEL_SKILL");
```

| Argument | Purpose |
| --- | --- |
| row | Which imported record failed |
| nameof(HeroConfig.SkillId) | Which column to locate |
| Message text | How the author should fix it |
| HIGH_LEVEL_SKILL | Stable searchable code, not a field name |

First three are required. Code defaults to CUSTOM; explicit descriptive codes are recommended.

Do not create a new HeroConfig for context.Error. Locations belong to this import's original row; copies cannot be located.

### 6.3 Conditional required: description when enabled

Assume Enabled bool and Description string:

```csharp
if (row.Enabled && context.IsEmpty(row, nameof(HeroConfig.Description)))
{
    context.Error(row, nameof(HeroConfig.Description),
        "Enabled heroes require a description.", "DESCRIPTION_REQUIRED");
}
```

Add inside the existing Validate; do not create another identical method.

Unconditionally required Description uses Required. Only conditional requirements need if.

### 6.4 Warnings: notify without blocking

```csharp
if (!context.IsEmpty(row, nameof(HeroConfig.Level)) && row.Level >= 95)
{
    context.Warning(row, nameof(HeroConfig.Level),
        "Level is near the limit; confirm it is intentional.", "LEVEL_HIGH_NOTICE");
}
```

Error blocks publication. Warning alone does not; other Errors still fail.

Exceptions become errors, not harmless notices. Use context.Error for expected business failure rather than throw.

### 6.5 Why post-checks run after field errors

The tool collects independently decidable errors rather than stopping at the first.

Failed attributes do not necessarily prevent Validate when parsing is complete.

Guard against missing/out-of-range data to avoid duplicate/derived errors. A blank level's default 0 should not imply a low-level hero.

Incomplete current/dependency tables skip class post-checks with BUSINESS_CHECK_SKIPPED.

## 7. Table post-validation

Validate sees one row; ValidateTable receives all final records for composite duplicates, counts, totals, etc.

### 7.1 Unique name/level combinations

Repeating a name or level alone may be valid; repeating both may not.

Add this method inside HeroRules and System.Collections.Generic/System.Linq imports:

```csharp
protected override void ValidateTable(
    IReadOnlyList<HeroConfig> rows, ValidationContext context)
{
    var groups = rows
        .Where(row => !context.IsEmpty(row, nameof(HeroConfig.Name)) &&
                      !context.IsEmpty(row, nameof(HeroConfig.Level)) &&
                      row.Level >= 1 && row.Level <= 100)
        .GroupBy(row => new
        {
            Name = row.Name.Trim().ToUpperInvariant(),
            Level = row.Level
        });

    foreach (var group in groups)
    {
        if (group.Count() <= 1) continue;

        foreach (HeroConfig row in group)
        {
            context.Error(row, nameof(HeroConfig.Name),
                "Duplicate name and level combination.", "NAME_LEVEL_DUPLICATE");
        }
    }
}
```

Where selects checkable data.

GroupBy groups by name + level.

Count returns group size.

foreach processes each item.

ToUpperInvariant normalizes case, making English names case-insensitive here.

Report every duplicate so each Excel row is easy to find.

### 7.2 At least one enabled hero

For nonempty tables, report against the first record:

```csharp
if (rows.Count > 0 && !rows.Any(row => row.Enabled))
{
    context.Error(rows[0], nameof(HeroConfig.Enabled),
        "At least one enabled hero is required.", "NO_ENABLED_HERO");
}
```

rows[0] is invalid when empty; check Count first.

Current common reporting APIs require a real row. Zero-row “data required” constraints need separate table diagnostics/input constraints. Do not invent rows or claim this fragment covers emptiness.

### 7.3 Filenames do not bind rules

HeroRules.cs alone identifies nothing; inherit TableRules<HeroConfig> with correct signatures.

All discovered valid rule classes run, not just one.

## 8. Cross-table post-validation

Reference asks whether an ID exists. Business checks can ask whether its damage suits the hero.

### 8.1 Complete example

Rule: skill damage ≥ hero level × 5.

Create Rules/HeroSkillRules.cs, a separate class that can coexist with HeroRules.

```csharp
using ZrConfig.Models;
using ZrConfig.Validation;

[DependsOn(typeof(SkillConfig))]
public sealed class HeroSkillRules : TableRules<HeroConfig>
{
    protected override void Validate(HeroConfig hero, ValidationContext context)
    {
        if (context.IsEmpty(hero, nameof(HeroConfig.Level)) ||
            hero.Level < 1 || hero.Level > 100)
            return;

        // First access builds indexes reused across every row/rule in this export.
        var skills = context.GetSkillTable();
        if (!skills.TryGet(hero.SkillId, out var skill) || skill == null || skill.Id <= 0)
            return;

        if (context.IsEmpty(skill, nameof(SkillConfig.Damage)) || skill.Damage < 0)
            return;

        int requiredDamage = hero.Level * 5;
        if (skill.Damage < requiredDamage)
            context.Error(hero, nameof(HeroConfig.SkillId),
                $"Skill {skill.Id} has damage {skill.Damage}; " +
                $"this hero requires at least {requiredDamage}.", "SKILL_DAMAGE_LOW");
    }
}
```

Assume SkillRules checks IDs/damage and HeroRules checks SkillId existence.

Missing skills are skipped here because Reference owns existence checks, avoiding misleading insufficient-damage errors.

### 8.2 Why row calls do not rebuild dictionaries

context.GetSkillTable() creates/caches the generated table and key/group indexes from current Skill rows on first access. Every hero/rule/ValidateTable shares it. TryGet queries directly; no handwritten dictionary or ValidateTable preparation is needed.

Skill.Id is marked as Excel row-3 key. Missing/duplicate keys make the table incomplete and skip dependent methods. PositiveKey still checks positivity; business Errors do not filter indexed rows, so validate skill.Id > 0 and damage ranges.

Re-export creates a new context/index, never old published data. context.GetTable<SkillConfig>() returns the same cached ConfigTable. Each table declares Excel keys and generates queries; see [boundaries](rules-development.en.md#use-indexed-tables-in-post-validation).

### 8.3 Which checks need DependsOn?

Custom Validate/ValidateTable using GetSkillTable/GetTable<SkillConfig> need DependsOn.

Reference already names its target and needs no extra dependency attribute; custom reads do.

Checks only comparing Hero.Level/SkillId without reading Skill are local and need none.

## 9. Object members and array elements

Built-ins mainly check top-level fields. Check Settings.MaxTargets/Actions[1].Delay in Validate.

With Settings/Actions present, add this import:

```csharp
using Tutorial.Config;
```

In Validate:

```csharp
if (row.Settings.MaxTargets <= 0)
{
    context.Error(row, "Settings.MaxTargets",
        "Maximum targets must be positive.", "TARGET_COUNT");
}

for (int i = 0; i < row.Actions.Length; i++)
{
    BattleAction action = row.Actions[i];
    string path = $"Actions[{i}]";

    if (action.Delay < 0)
        context.Error(row, path + ".Delay",
            "Action delay must not be negative.", "ACTION_DELAY");

    if (action is Damage damage && damage.Ratio < 0)
        context.Error(row, path + ".Ratio",
            "Damage ratio must not be negative.", "DAMAGE_RATIO");

    if (action is AddBuff buff && buff.Stacks <= 0)
        context.Error(row, path + ".Stacks",
            "Buff stacks must be positive.", "BUFF_STACKS");
}
```

for visits array elements by index.

action is Damage damage tests the concrete kind and enables Damage-only members; not every action has Ratio.

### 9.1 Diagnostic paths

| Target | Path |
| --- | --- |
| Top-level field | `nameof(HeroConfig.Level)` |
| Settings target count | `"Settings.MaxTargets"` |
| First action delay | `"Actions[0].Delay"` |
| Second action buff ID | `"Actions[1].BuffId"` |
| Third action in first group | `"Groups[0].Actions[2].Delay"` |
| First action/group in second phase | `"Phases[1].Groups[0].Actions[0].Delay"` |

**Pass the complete row, not action/Settings/buff.** Row plus path identifies the original workbook record.

Indexes are zero-based after sorting; input maps recover original physical coordinates, not index-as-row-number.

JSON-member errors locate the containing cell; split/multi-row formats locate individual member cells.

Misspelled paths yield VALIDATOR_FIELD. Check case/hierarchy/indexes. Use Actions[actual-index].Delay, not Actions[].Delay.

### 9.2 At least one array element

```csharp
if (row.Actions.Length == 0)
    context.Error(row, nameof(HeroConfig.Actions),
        "At least one action is required.", "ACTION_REQUIRED");
```

This checks nonempty content. Required only checks input; explicit [] remains empty.

### 9.3 Conditionally required members

```csharp
if (row.Settings.Interruptible &&
    context.IsEmpty(row, "Settings.MaxTargets"))
{
    context.Error(row, "Settings.MaxTargets",
        "Explicit target count is required when interruption is allowed.", "TARGET_COUNT_REQUIRED");
}
```

Member paths inspect original filled status. Defaults and explicit input are separate facts.

## 10. Direct table references: avoid manual lookups

Integer Hero.SkillId requires a lookup to access skill data.

Alternatively use field Skill, type SkillConfig, still entering the skill ID.

| Row | Id | Name | Skill |
| --- | --- | --- | --- |
| 1 | Id | Name | Skill |
| 2 | int | string | SkillConfig |
| 3 | `主键` (literal marker) | | |
| 4 | Identifier | Name | Skill identifier |
| 5: Side | CS | CS | CS |
| 6 | 1001 | Young swordsman | 2001 |

Generated HeroConfig.Skill is an object reference:

```csharp
if (hero.Skill != null)
{
    int damage = hero.Skill.Damage;
}
```

Import creates records before linking, supporting bidirectional/self references.

- Target table needs int Id.
- Referenced IDs must be nonzero/unique.
- Missing nonzero targets block export.
- Blank/0 means null; generated references are nullable.
- For mandatory skills add Required(nameof(HeroConfig.Skill)).
- Do not add integer Reference attributes to object references; they are different mechanisms.

Post-checks reading target data still declare DependsOn(typeof(SkillConfig)) and null-check before access.

For example:

```csharp
if (hero.Skill != null && hero.Skill.Damage < hero.Level * 5)
    context.Error(hero, nameof(HeroConfig.Skill),
        "Selected skill has insufficient damage.", "SKILL_DAMAGE_LOW");
```

This is the alternative Skill-object model. Chapter 12 uses integer SkillId; do not copy into a model without Skill.

SkillId→Skill is a schema change: edit Excel, regenerate, update rules, export, synchronize game code/data.

Direct references currently support scalar table fields, not array elements/custom-object members.

## 11. Extend special formats with converters

Most classes/structs need no converter; public members generate serialization directly.

Use converters only when Excel representation differs from program storage.

Example: enter currency 1.25, store 125 cents.

### 11.1 Define Money

Create Config/CustomTypes/Money.cs:

```csharp
using ZrConfig.Runtime;

namespace Tutorial.Config
{
    [ConfigConverter(typeof(MoneyConverter), "1")]
    public readonly struct Money
    {
        public readonly int Cents;

        public Money(int cents)
        {
            Cents = cents;
        }
    }
}
```

This readonly struct uses a converter, so the tool never directly assigns readonly fields.

### 11.2 Define MoneyConverter

Create Config/Serialization/MoneyConverter.cs:

```csharp
using System;
using System.Globalization;
using System.IO;
using ZrConfig.Runtime;

namespace Tutorial.Config
{
    public sealed class MoneyConverter : IConfigConverter<Money>
    {
        public Money Parse(string text)
        {
            decimal cents = decimal.Parse(text,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture) * 100;

            if (decimal.Truncate(cents) != cents)
                throw new FormatException("Currency supports at most two decimal places.");

            return new Money(checked((int)cents));
        }

        public void Write(BinaryWriter writer, Money value)
        {
            writer.Write(value.Cents);
        }

        public Money Read(BinaryReader reader)
        {
            return new Money(reader.ReadInt32());
        }
    }
}
```

| Method | When | Example |
| --- | --- | --- |
| Parse | Read Excel text during export | "1.25" → Money(125) |
| Write | Write binary | Store integer 125 |
| Read | Read-back/game load | Integer 125 → Money(125) |

Excel type Tutorial.Config.Money uses 1.25; Money[] uses [1.25;2.50].

Write/Read must match exactly: Int32 with Int32 and identical member order. Read-back checks help, but protocol design remains yours.

Negative-price policies belong in field/post-rules. This converter handles formatting/serialization without imposing positivity.

"1" is converter version. Change it, e.g. to "2", when binary representation changes, then regenerate/export/synchronize game code. Never update only the reader.

Converter instances may be reused; no previous-cell state or mutable shared state affecting results.

## 12. Complete exercise project

This chapter combines Excel, handwritten types, and rules into one complete exercise.

Download the [exercise ZIP](assets/user-guide-example.zip), select its Data/Config/Rules, and choose new Output. It includes two workbooks/handwritten source, no generated code/old exports. Generate, then export.

Handwritten types use CustomTypes, matching the attachment.

You may create it manually. **Use a new exercise directory, never mix teaching Hero/Skill into samples/Data.** Old rules/duplicate tables/extra models could interfere.

### 12.1 Final layout

```text
ZrConfig-Beginner/
  Data/
    Hero.xlsx
    Skill.xlsx
  Config/
    CustomTypes/
      BattleSettings.cs
      BattleAction.cs
    Serialization/                   Initialized by the tool on first generation
    Generated/                       Created by the tool on first generation
  Rules/
    DisplayNameRule.cs
    ProbabilityRule.cs
    HeroRules.cs
    HeroSkillRules.cs
    SkillRules.cs
  Output/                            Created on first export
```

Code below is final. Do not retain earlier same-name examples separately; each class appears once.

### 12.2 Two workbooks

#### Hero.xlsx

Use field/type/key/description/side headers. Wide Hero is shown in two blocks, **left/right parts of one sheet, not two sheets**. Only A contains 主键 on row 3.

Columns A–I:

| Excel row | A | B | C | D | E | F | G | H | I |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Id | Name | Level | SkillId | Rate | Enabled | Description | Settings.MaxTargets | Settings.Interruptible |
| 2 | int | string | int | int | int | bool | string | Tutorial.Config.BattleSettings | |
| 3 | `主键` (literal marker) | | | | | | | | |
| 4 | Identifier | Name | Level | Skill ID | Probability in basis points | Enabled | Description | Maximum targets | Interruptible |
| 5: Side | CS | CS | CS | CS | CS | CS | CS | CS | CS |

Columns J–P:

| Excel row | J | K | L | M | N | O | P |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
| 2 | Tutorial.Config.BattleAction[] | | | | | | |
| 3 | | | | | | | |
| 4 | Action order | Kind | Delay | Damage ratio | Critical hits? | Buff ID | Stacks |
| 5: Side | CS | CS | CS | CS | CS | CS | CS |

Rows 6–8, columns A–I:

| Excel row | A | B | C | D | E | F | G | H | I |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 6 | 1001 | Young swordsman | 10 | 2001 | 2500 | true | Beginner hero | 1 | true |
| 7 | 1001 | | | | | | | | |
| 8 | 1002 | Young mage | 95 | 2002 | 10000 | true | Advanced hero | 3 | false |

Rows 6–8, columns J–P:

| Excel row | J | K | L | M | N | O | P |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 6 | 1 | Damage | 0.2 | 1.5 | true | | |
| 7 | 2 | AddBuff | 0.5 | | | 3001 | 2 |
| 8 | 1 | Damage | 0 | 2 | false | | |

Row 7 is hero 1001's second action, repeating Id and leaving other main fields blank. Export 2 heroes, not 3.

#### Skill.xlsx

| Excel row | A | B | C |
| --- | --- | --- | --- |
| 1 | Id | Name | Damage |
| 2 | int | string | int |
| 3 | `主键` (literal marker) | | |
| 4 | Skill ID | Skill name | Damage |
| 5: Side | CS | CS | CS |
| 6 | 2001 | Slash | 80 |
| 7 | 2002 | Fireball | 600 |

No Buff table exists here. The exercise tests format/required input/stacks, not existence of Buff 3001. Add Buff and cross-table rules in production if required.

### 12.3 File: Config/CustomTypes/BattleSettings.cs

```csharp
namespace Tutorial.Config
{
    public sealed class BattleSettings
    {
        public int MaxTargets = 1;
        public bool Interruptible = true;
    }
}
```

### 12.4 File: Config/CustomTypes/BattleAction.cs

```csharp
using ZrConfig.Runtime;

namespace Tutorial.Config
{
    public abstract class BattleAction
    {
        public float Delay;
    }

    public sealed class Damage : BattleAction
    {
        public float Ratio = 1;
        public bool CanCrit;
    }

    public sealed class AddBuff : BattleAction
    {
        [ConfigRequired]
        public int BuffId;

        public int Stacks = 1;
    }
}
```

### 12.5 File: Rules/DisplayNameRule.cs

```csharp
using System;
using ZrConfig.Validation;

public sealed class DisplayNameRule : IFieldRule<string>
{
    public void Validate(string value, FieldRuleContext context)
    {
        if (value.Length > 16)
            context.Error("Name must not exceed 16 characters.", "NAME_TOO_LONG");

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            context.Error("Name must not have leading/trailing spaces.", "NAME_SPACE");

        foreach (char character in value)
        {
            if (!char.IsControl(character)) continue;
            context.Error("Name must not contain newlines/tabs.", "NAME_CONTROL");
            break;
        }
    }
}
```

### 12.6 File: Rules/ProbabilityRule.cs

```csharp
using ZrConfig.Validation;

public sealed class ProbabilityRule : IFieldRule<int>
{
    public void Validate(int value, FieldRuleContext context)
    {
        if (value < 0 || value > 10000)
            context.Error("Probability must be between 0 and 10000.", "PROBABILITY_RANGE");
    }
}
```

### 12.7 File: Rules/HeroRules.cs

```csharp
using System.Collections.Generic;
using System.Linq;
using Tutorial.Config;
using ZrConfig.Models;
using ZrConfig.Validation;

[PositiveKey(nameof(HeroConfig.Id))]
[Required(nameof(HeroConfig.Name))]
[Check(nameof(HeroConfig.Name), typeof(DisplayNameRule))]
[Required(nameof(HeroConfig.Level))]
[Range(nameof(HeroConfig.Level), 1, 100)]
[Reference(nameof(HeroConfig.SkillId), typeof(SkillConfig), nameof(SkillConfig.Id),
    AllowDefaultValue = true)]
[Required(nameof(HeroConfig.Rate))]
[Check(nameof(HeroConfig.Rate), typeof(ProbabilityRule))]
[Required(nameof(HeroConfig.Enabled))]
public sealed class HeroRules : TableRules<HeroConfig>
{
    protected override void Validate(HeroConfig row, ValidationContext context)
    {
        if (!context.IsEmpty(row, nameof(HeroConfig.Level)))
        {
            if (row.Level >= 90 && row.SkillId == 0)
                context.Error(row, nameof(HeroConfig.SkillId),
                    "Heroes at level 90 or above require a skill.", "HIGH_LEVEL_SKILL");

            if (row.Level >= 95 && row.Level <= 100)
                context.Warning(row, nameof(HeroConfig.Level),
                    "Level is near the limit; confirm it is intentional.", "LEVEL_HIGH_NOTICE");
        }

        if (row.Enabled && context.IsEmpty(row, nameof(HeroConfig.Description)))
            context.Error(row, nameof(HeroConfig.Description),
                "Enabled heroes require a description.", "DESCRIPTION_REQUIRED");

        if (row.Settings.MaxTargets <= 0)
            context.Error(row, "Settings.MaxTargets",
                "Maximum targets must be positive.", "TARGET_COUNT");

        for (int i = 0; i < row.Actions.Length; i++)
        {
            BattleAction action = row.Actions[i];
            string path = $"Actions[{i}]";

            if (action.Delay < 0)
                context.Error(row, path + ".Delay",
                    "Action delay must not be negative.", "ACTION_DELAY");

            if (action is Damage damage && damage.Ratio < 0)
                context.Error(row, path + ".Ratio",
                    "Damage ratio must not be negative.", "DAMAGE_RATIO");

            if (action is AddBuff buff && buff.Stacks <= 0)
                context.Error(row, path + ".Stacks",
                    "Buff stacks must be positive.", "BUFF_STACKS");
        }
    }

    protected override void ValidateTable(
        IReadOnlyList<HeroConfig> rows, ValidationContext context)
    {
        var groups = rows
            .Where(row => !context.IsEmpty(row, nameof(HeroConfig.Name)) &&
                          !context.IsEmpty(row, nameof(HeroConfig.Level)) &&
                          row.Level >= 1 && row.Level <= 100)
            .GroupBy(row => new
            {
                Name = row.Name.Trim().ToUpperInvariant(),
                Level = row.Level
            });

        foreach (var group in groups)
        {
            if (group.Count() <= 1) continue;
            foreach (HeroConfig row in group)
                context.Error(row, nameof(HeroConfig.Name),
                    "Duplicate name/level combination.", "NAME_LEVEL_DUPLICATE");
        }
    }
}
```

### 12.8 File: Rules/HeroSkillRules.cs

```csharp
using ZrConfig.Models;
using ZrConfig.Validation;

[DependsOn(typeof(SkillConfig))]
public sealed class HeroSkillRules : TableRules<HeroConfig>
{
    protected override void Validate(HeroConfig hero, ValidationContext context)
    {
        if (context.IsEmpty(hero, nameof(HeroConfig.Level)) ||
            hero.Level < 1 || hero.Level > 100)
            return;

        // First access builds indexes reused by all rows/rules in this export.
        var skills = context.GetSkillTable();
        if (!skills.TryGet(hero.SkillId, out var skill) || skill == null || skill.Id <= 0)
            return;

        if (context.IsEmpty(skill, nameof(SkillConfig.Damage)) || skill.Damage < 0)
            return;

        int requiredDamage = hero.Level * 5;
        if (skill.Damage < requiredDamage)
            context.Error(hero, nameof(HeroConfig.SkillId),
                $"Skill {skill.Id} has damage {skill.Damage}; " +
                $"this hero requires at least {requiredDamage}.", "SKILL_DAMAGE_LOW");
    }
}
```

### 12.9 File: Rules/SkillRules.cs

```csharp
using ZrConfig.Models;
using ZrConfig.Validation;

[PositiveKey(nameof(SkillConfig.Id))]
[Required(nameof(SkillConfig.Name))]
[Check(nameof(SkillConfig.Name), typeof(DisplayNameRule))]
[Required(nameof(SkillConfig.Damage))]
[Range(nameof(SkillConfig.Damage), 0, 1000000)]
public sealed class SkillRules : TableRules<SkillConfig>
{
    protected override void Validate(SkillConfig row, ValidationContext context)
    {
        if (!context.IsEmpty(row, nameof(SkillConfig.Damage)) && row.Damage == 0)
            context.Warning(row, nameof(SkillConfig.Damage),
                "Skill damage is zero; confirm this is a support skill.", "ZERO_DAMAGE_NOTICE");
    }
}
```

### 12.10 Select paths, generate, export

| Input | Selection |
| --- | --- |
| Excel input | ZrConfig-Beginner/Data |
| Output | ZrConfig-Beginner/Output |
| Code root | ZrConfig-Beginner/Config |
| C# rules | ZrConfig-Beginner/Rules |

Generate configuration code to obtain HeroConfig/SkillConfig and codecs.

Then Validate and export. Expect:

- Success.
- 0 errors.
- 1 warning: level-95 young mage triggers LEVEL_HIGH_NOTICE.
- Hero 2 rows, Skill 2 rows.
- Swordsman 2 actions, mage 1.

This warning is intentional; do not corrupt valid data just to reach zero warnings. Remove the LEVEL_HIGH_NOTICE condition if unwanted.

### 12.11 Deliberate errors and locations

One change at a time: save → export → inspect → restore. Data-only changes need no generation.

| Cell | Change | Expected result |
| --- | --- | --- |
| Hero C6, Level | 101 | RANGE: level must be 1–100 |
| Hero B6, Name | Clear | REQUIRED |
| Hero B6, Name | Add leading space | NAME_SPACE |
| Hero E6，Rate | 10001 | PROBABILITY_RANGE |
| Hero G6, Description | Clear | DESCRIPTION_REQUIRED |
| Hero D6，SkillId | 9999 | REFERENCE_MISSING |
| Skill C6, Damage | 20 | SKILL_DAMAGE_LOW: level 10 needs ≥50, located at referencing Hero D6 |
| Hero H6，Settings.MaxTargets | 0 | TARGET_COUNT |
| Hero L7, Actions[].Delay | -1 | ACTION_DELAY at the second action's original row |
| Hero O7, Actions[].BuffId | Clear | Missing required member; parse failure |
| Hero P7，Actions[].Stacks | 0 | BUFF_STACKS |
| Hero D8，SkillId | 0 | HIGH_LEVEL_SKILL |
| Skill A7, Id | 2001 | KEY_DUPLICATE |
| Hero B8 and C8 | Young swordsman, 10 | NAME_LEVEL_DUPLICATE for both heroes |

Any Error keeps Current unchanged. Fix/export again; latest log files update.

Duplicate-target exercises may also cause another hero's old-ID reference to fail. Independent errors are reported together.

## 13. Command-line usage

**CLI** means **Command-Line Interface**, usually ZrConfig.Cli here. A command specifies operation, inputs, and output; execution returns text/log paths/exit code. Scripts use the numeric code for success/failure; see 13.4.

**App** is the desktop UI, sufficient for authors. Developers use CLI for automation, build checks, and rule debugging. Both share generation/validation/export and identical business checks. export --generate includes App-style prechecks/needed generation; plain export only exports. Successful export writes real version data.

### Distinguish entry points

| Name | Purpose | Use |
| --- | --- | --- |
| ZrConfig.App | Desktop authoring | Choose paths/click buttons |
| ZrConfig.Cli | Generation, validation, export, release management | codegen, validate, export, support, release |
| ZrConfig.Maintenance | Repository maintenance | maintain.sh/maintain.ps1 for Unity preparation, portability, packaging, cleanup |

PowerShell/Bash execute commands; terminals provide input windows. They launch CLI. dotnet builds/runs .NET; scripts/dotnet.sh locates it. maintain.* commands belong to maintenance and cannot be passed directly to CLI. See [public commands](development.en.md).

In source examples, run --project src/ZrConfig.Cli builds/runs CLI; -- separates dotnet/tool arguments. export is the tool operation, followed by paths. Self-contained Windows uses ZrConfig.Cli.exe export directly without dotnet run; see 13.6.

Bash examples run from root; Windows is in 13.6. Replace /path/to/ placeholders; quote spaced paths with ASCII double quotes. Data is the recursively scanned root and may mix .xlsx/.xls.

### Share App project settings

Prefer the App-saved project.zrconfig.json; commands read required directories:

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- codegen --project samples/project.zrconfig.json
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- validate --project samples/project.zrconfig.json
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- export --project samples/project.zrconfig.json
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- support --project samples/project.zrconfig.json
```

First --project belongs to dotnet; after -- it belongs to ZrConfig. Relative paths resolve beside JSON and survive moves. Empty RulesFolder means basic checks. Do not mix --project with positional paths; duplicate/missing file arguments fail early. --accept-changes supports codegen/export --generate; --rules-debug supports validate/export, including combined export. Windows may run `ZrConfig.Cli.exe export --project "D:\Game Data\project.zrconfig.json"`.

Existing DataFolder/OutputFolder/CodeFolder/RulesFolder/optional Target remain. 1.4.0 adds optional Name and FormatVersion: 1; old files still read. Recent-project/UI preferences stay local, not shared in project JSON.

### Validation-only and reports

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- validate --project samples/project.zrconfig.json
# Keep reports outside Data, Config, Rules, and Output
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- validate --project samples/project.zrconfig.json --report /tmp/zrconfig-validation.json
```

Positional form: validate <Data> <ConfigRoot> [Rules], no Output. Current code parses/checks/runs rules/binary read-back without writing Config/selected Output or generating. Temporary files use system temp. Missing code/outdated support/schema returns 3, meaning incomplete full validation: review/generate/revalidate. Validation success is not publication; delivery needs export.

validate supports --target C\|S, --rules-debug, --report; export also supports --report. JSON reports contain state, locations, existing run/version IDs, not full source/workbooks. Known roots are placeholders; review business messages before sharing. Export stopped during generation prechecks never reaches export-report writing; retain terminal output or App reports.

Ctrl+C requests safe cancellation. Third-party parsing/custom rules return before the next checkpoint. Committed code stays; completed activation reports success. See [boundaries](release-and-acceptance.en.md#operation-boundaries).

### Generate and export in one step

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- export --generate --project samples/project.zrconfig.json
```

The tool read-only checks structure/types/support/accepted APIs and lists changes. Initial or compatible generation may proceed, then current Excel is fully validated/exported. Unchanged code skips writes. Root changes/deletions stop with exit 1; review then add --accept-changes. API removals/signatures need separate migration. Generation failure prevents export; export failure preserves current.json but not rollback of already committed code.

Added fields may change public constructors. For `API_CHANGE_REQUIRED`, developers follow [public API protection](server-generation.en.md#public-api-protection-and-explicit-migration) to verify old consumers and obtain a migration receipt, then run `codegen ... --accept-api-change <preview-digest> --migration-receipt <receipt-file>` with the same generation options before exporting again. The digest and receipt apply only to this change and are not saved in project settings. Version `Config/.schema.json` with Generated; deleting the baseline cannot bypass checks. The App displays changes and allows cancellation only.

App/combined CLI share ProjectWorkflow; App uses confirmation, CLI explicit options. Plain export never writes generated source, useful for validating committed code only. export --accept-changes without --generate is invalid.

### Generate/export by side

See [C/S/CS](config-types.en.md#client-and-server-fields). codegen/validate/export accept --target C/S, otherwise project Target, otherwise C. Explicit CLI overrides do not rewrite settings. CS is a field marker, not an operation target.

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- codegen --project samples/project.zrconfig.json --target C
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- export --project samples/project.zrconfig.json --target C --generate
```

Replace C with S for servers; Windows arguments are identical. One operation generates/publishes one side; Generated/current.json represent that result. Delivering both sides requires actual per-side project code/output paths; one generated model set cannot serve both. Official samples retain one shared directory set.

Fields/constructors/codecs/queries consistently follow target. Schema hashes include side; wrong-side export yields SCHEMA_MISMATCH and preserves Current. current.json target C/S supports delivery checks. Regenerate old code.

Core Generate/Prepare and RunWithSources/RunWithRules optional target accept ConfigTarget.Client/Server. Assembly-only uses ExportPipeline.Run(data, output, assembly, target, validators); ProjectWorkflow uses WorkspaceSettings.Target. Rules compile against current-side models; excluded-field access fails. Use corresponding rule paths when different sides need different rules.

### 13.1 Generate code

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- codegen "/path/to/ZrConfig-Beginner/Data" "/path/to/ZrConfig-Beginner/Config"
```

Order: codegen → Excel directory → code root.

Config remembers its complete input root. Changed roots/deletions stop with old/new paths and deletion list; rule out mistaken subfolders first. Without API changes, review and accept; public-table removal needs migration. Preview neither upgrades support, creates Config, nor writes disk caches; unchanged type compilations reuse only process memory. App pre-export follows this and offers combined generation only without migration. Old projects still check files/baselines; relative bindings survive whole-project moves.

### 13.2 Export with rules

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export "/path/to/ZrConfig-Beginner/Data" "/path/to/ZrConfig-Beginner/Output" "/path/to/ZrConfig-Beginner/Config" "/path/to/ZrConfig-Beginner/Rules"
```

Order: export → Excel → output → code root → rules.

Unknown/duplicate/inapplicable options and excess paths fail with 2 before work. --help shows usage; --rules-debug enables debugging. --dry-run is unsupported. For paths beginning -, add the tool's own --; all subsequent arguments are paths.

### 13.3 No business rules

Explicit Config, omitted Rules:

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export "/path/to/ZrConfig-Beginner/Data" "/path/to/ZrConfig-Beginner/Output" "/path/to/ZrConfig-Beginner/Config"
```

No fallback to sample rules. Basic types/schema/references/codecs still run, but HeroRules/business constraints do not.

### 13.4 Already-built CLI

Build first:

```bash
bash scripts/dotnet.sh build src/ZrConfig.Cli -p:NuGetAudit=false
```

Then call the DLL:

```bash
bash scripts/dotnet.sh src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll codegen "path/to/Data" "path/to/Config"
bash scripts/dotnet.sh src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll export "path/to/Data" "path/to/Output" "path/to/Config" "path/to/Rules"
```

| Exit code | Meaning | Automation |
| --- | --- | --- |
| 0 | Command succeeded | Only successful export means new publication |
| 1 | Generation/validation/export checks failed or confirmation absent | Read diagnostics; stop publication |
| 2 | Invalid arguments/execution exception | Fix command/paths/environment |
| 3 | Validation requires generation; incomplete | Review schema, generate, revalidate |
| 130 | Cancellation handled | Check committed code; not publication success |

Help/validation/version previews returning 0 do not export data. release list/diff/rows/rollback/pin/unpin/cleanup manage versions; rollback/cleanup preview until --apply. See [compatibility](release-and-acceptance.en.md#compare-roll-back-and-clean). No watch/general dry-run or incremental single-workbook replacement of full versions.

Positional paths must be complete: codegen Data ConfigRoot; validate Data ConfigRoot [Rules]; export Data Output ConfigRoot [Rules]. Do not omit intermediate paths. Inside repositories, no paths at all uses sample defaults; standalone tools need explicit paths/project. Explicit Config without Rules disables business rules. release requires --project and no positional directories. Prefer project settings or complete explicit paths.

### 13.5 Rule breakpoints

Append --rules-debug to validate/export for dynamic Config/Rules Debug and Portable PDB, distinct from tool-project Debug:

```bash
bash scripts/dotnet.sh src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll export "path/to/Data" "path/to/Output" "path/to/Config" "path/to/Rules" --rules-debug
```

Rider selects CLI in .NET Project debugging. Visual Studio sets CLI startup/arguments and starts Debug/F5. Break on executable Validate/ValidateTable statements or attach to the actual CLI .NET process. Dynamic Debug/Release caches are separate; logs retain full exceptions/source locations.

See [debugging](debugging.en.md) for detailed IDE/PowerShell/unbound-breakpoint/exception/Unity-process steps.

CLI never pauses automatically for attachment. Arrange timing or launch through IDE; actual breakpoint behavior depends on the environment. Everyday authoring needs no flag; App does not expose it.

### 13.6 Windows PowerShell

Keep all files in self-contained Windows packages. Replace tool/four project paths below:

```powershell
$zrTool = "C:\Tools\ZrConfig"
& "$zrTool\ZrConfig.Cli.exe" codegen "D:\Game Data\Data" "D:\Game Data\Config"
if ($LASTEXITCODE -ne 0) { throw "Generation failed; resolve diagnostics first." }
& "$zrTool\ZrConfig.Cli.exe" export "D:\Game Data\Data" "D:\Game Data\Output" "D:\Game Data\Config" "D:\Game Data\Rules"
if ($LASTEXITCODE -ne 0) { throw "Export failed; do not publish this run's data." }
```

From source, run SDK at root without Bash:

```powershell
dotnet run --project src/ZrConfig.Cli -p:NuGetAudit=false -- codegen "D:\Game Data\Data" "D:\Game Data\Config"
dotnet run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export "D:\Game Data\Data" "D:\Game Data\Output" "D:\Game Data\Config" "D:\Game Data\Rules" --rules-debug
```

Add --rules-debug only for breakpoints/PDB. Automation checks every exit code. See [SDK/Windows packaging](development.en.md). Windows paths cannot be pasted unchanged on macOS; use local paths.

## 14. Unity runtime integration

Developers perform initial integration; authors [export/deliver](user-guide.en.md).

### 14.1 Export does not automatically update games

Compile generated C# into the game, deploy readable data, then load/reload explicitly.

A running game cannot infer or read a new repository Output file automatically.

### 14.2 Integrate Unity source

Run from root:

```bash
bash scripts/package-unity.sh
```

Copy output Assets/ZrConfig and sibling ZrConfig.meta into target Assets, retaining LICENSE. Default artifacts/unity/ZrConfig-source; choose new output on repeats to avoid overwriting source.

Runtime/Validation/adapters/viewer compile from source via asmdefs; Validation/Unity.Editor are Editor-only. Models reference Runtime; rules reference Runtime/Validation/models and target Editor. Add no ZrConfig Package/precompiled DLL. Remove old com.zrconfig.unity/same-name DLLs on upgrade to avoid duplicate assemblies. Generate business Hero/Skill in your project; no Excel parser ships to runtime.

A [runnable Unity sample](../samples/README.en.md#runnable-unity-sample) shares official configuration/exports. Try unity-sample first; follow below for your own project.

### 14.3 Place code/rules correctly

One clear layout:

```text
YourUnityProject/
  Excel; may live outside Assets
  Rules/                                 Export business rules outside Assets
  Output/                                Tool release records, not directly bundled into Players
  Assets/
    Config/
      CustomTypes/
      Serialization/
      Generated/
    StreamingAssets/
      ZrConfig/                          Current manifest and versioned data
    Scripts/
      HeroConfigReader.cs                Game-side reading example
```

- Set the Config root to `YourUnityProject/Assets/Config`.
- Set the Rules directory to `YourUnityProject/Rules`.
- Set the output directory to `YourUnityProject/Output`. Deploy only current.json and its referenced bytes to `Assets/StreamingAssets/ZrConfig`, preserving the relative version paths.
- After the first generation, wait for Unity to finish compiling before entering Play mode.

Keeping rules outside Assets excludes them and their dependencies from Player compilation. The export App still compiles and runs them dynamically.

For direct compilation in Rider's Unity solution, put rules in an Editor directory such as `Assets/Editor/ConfigRules`, with appropriate Runtime/Validation asmdef references. See [Rule development](rules-development.en.md).

With asmdefs or hot-update assemblies, put Config in your actual configuration/hot-update assembly and reference Runtime. Rules compiled in the Editor must reference Validation. Avoid importing an old Models DLL alongside new source defining the same models.

### 14.4 Minimal reading example for desktop Unity

Everyday use has three steps: initialize with `UnityConfigRuntime.Load / LoadAsync`, query the returned snapshot, then call `Reload / ReloadAsync` after updates. Choose synchronous or asynchronous calls according to actual resource capabilities. Ordinary generated queries do not require `TableCodecs.Registry`; `IsInitialized` reports initialization state. `Initialize` and `Manager` serve advanced tools; ordinary business code need not manage loaders directly.

This example uses the repository's official samples; chapter 12 is not a prerequisite:

1. Export all samples/Data following the [sample instructions](../samples/README.en.md): nine tables, including the 10,000-row case. Confirm zero errors.
2. Integrate Assets/ZrConfig source as described above. Copy CustomTypes, Serialization, and Generated from unity/ZrConfig.Sample/Assets/HotUpdate/Config into Unity's Assets/Config, retaining one source set and one TableCodecs.
3. Follow samples/Output/current.json to copy every referenced `.bytes` / `.json` table file into its version path under Assets/StreamingAssets/ZrConfig, then copy current.json last. Exclude logs, compilation caches, release records, and `.rows.json`. The repository sample can deploy with `unity-sample`.
4. Wait for Unity compilation, then add the reading script below.

If your project already has configuration, use its own Data/Config/Rules set. Do not mix in tables with the same names from the official samples or chapter 12.

Hero/Skill in the official TableReferences.xlsx mark Id as “主键” (primary key) in row 3. After generation and export, loading the generated tables automatically builds Id indexes for the Get/TryGet queries below.

For GroupId + Id, larger composite keys, and group retrieval, see [Composite keys and group queries](table-indexes.en.md).

Create `Assets/Scripts/HeroConfigReader.cs`:

```csharp
using System;
using System.IO;
using System.Threading;
using UnityEngine;
using ZrConfig.Models;
using ZrConfig.Runtime;
using ZrConfig.Unity;

public sealed class HeroConfigReader : MonoBehaviour
{
    private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
    private HeroConfigTable _heroes;
    private bool _ready;

    private async void Start()
    {
        CancellationToken token = _lifetime.Token;
        try
        {
            string folder = Path.Combine(Application.streamingAssetsPath, "ZrConfig");
            var source = new FileConfigSource(folder);
            ConfigSnapshot snapshot = await UnityConfigRuntime.LoadAsync(
                source, token);

            _heroes = await snapshot.GetHeroTableAsync(token);
            _ready = true;
            ShowHero(1);
        }
        catch (OperationCanceledException)
        {
            // Cancel reads when leaving the scene; do not access destroyed objects.
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public void ShowHero(int id)
    {
        if (!_ready) return;
        if (!_heroes.TryGet(id, out HeroConfig hero))
        {
            Debug.LogWarning("Hero not found: " + id);
            return;
        }

        Debug.Log(hero.Name + ", skill " + (hero.Skill?.Name ?? "none") +
                  ", mentor " + (hero.Mentor?.Name ?? "none"));
    }

    public async void ReloadConfiguration()
    {
        if (!_ready) return;
        CancellationToken token = _lifetime.Token;
        try
        {
            ConfigSnapshot snapshot = await UnityConfigRuntime.ReloadAsync(token);
            _heroes = await snapshot.GetHeroTableAsync(token);
            ShowHero(1);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void OnDestroy()
    {
        _ready = false;
        _lifetime.Cancel();
        _lifetime.Dispose();
    }
}
```

Create an empty GameObject, attach this script, and enter Play mode. Expect “Flame Mage, skill Fireball, mentor Flame Mage” in Chinese. ShowHero(3) demonstrates null references: both skill and mentor are “none”.

This script demonstrates an application entry point. Only one entry point should call the initial LoadAsync during a Play session. Attaching initialization scripts to ten objects would attempt duplicate initialization, which the static runtime rejects.

Bind ReloadConfiguration to a UI Button if needed. This example reads the Unity project's StreamingAssets copy. For each data-only update:

1. Save Excel, export again to samples/Output, and confirm success.
2. Copy every `.bytes` / `.json` table referenced by the new current.json to `Assets/StreamingAssets/ZrConfig/versions/<version>/`, then replace `Assets/StreamingAssets/ZrConfig/current.json` last. Keep management records and diff snapshots in Output.
3. Click ReloadConfiguration to reread the manifest and data. Updating only samples/Output without copying these files still loads the old version.

The repository's Editor sample reads shared samples/Output directly; Players use filtered deployment in StreamingAssets. Schema or type changes also require synchronizing Config source, waiting for Unity compilation, and restarting Play mode. Data reload does not update compiled C# types.

For beginners:

| Code | Meaning |
| --- | --- |
| XxxConfigTable.GetCodec() | Returns this table's shared codec; generated queries pass it automatically |
| TableCodecs.Registry / EnumerateAll() | Optional full-table catalog for complete validation, export, and dynamic tooling |
| FileConfigSource(folder) | Specifies the actual directory to read |
| Load / LoadAsync | Initializes and reads the version manifest synchronously/asynchronously |
| await | Waits for this asynchronous operation before using its result |
| ConfigSnapshot | A fixed version, resource source, and lazily loaded row/table caches |
| GetHeroTable() / GetHeroTableAsync() | Loads the whole Hero table once; references load on property access and are reused |
| TryGet(1, out hero) | Attempts to find hero 1 in the official sample |
| Reload / ReloadAsync | Rereads current.json synchronously/asynchronously and switches to a new lazy snapshot |
| CancellationToken | Requests unfinished reads to stop when leaving the scene |

Each generated Hero table builds its index once when loaded. Business code retrieves the existing table and replaces its reference after hot updates. CreateIndex explicitly creates a new index and allocates on every call; do not use it in per-frame queries.

#### Synchronous loading and asynchronous loading without Task

Asynchronous APIs return the built-in `ConfigOperation<T>`, directly awaitable without Task, UniTask, or third-party packages. You may declare `async ConfigOperation<T>` methods. It supports configuration-specific waiting, results, exceptions, and cancellation, not a full task scheduler. Await each returned operation only once. Calling `GetAwaiter().GetResult()` before completion throws instead of blocking. Immediately available resources can return `ConfigOperation<byte[]>.FromResult(bytes)`. Callback loaders can use `ConfigCompletionSource<byte[]>`, its `Operation`, and `TrySetResult` / `TrySetException` / `TrySetCanceled`.

Alternatively, initialize synchronously (choose this or the asynchronous initialization above):

```csharp
var source = new FileConfigSource(folder);
ConfigSnapshot snapshot = UnityConfigRuntime.Load(source);
// Subsequent reloads reuse the last successful resource source.
snapshot = UnityConfigRuntime.Reload();
```

The asynchronous form is equivalent:

```csharp
ConfigSnapshot snapshot = await UnityConfigRuntime.LoadAsync(source, token);
snapshot = await UnityConfigRuntime.ReloadAsync(token);
```

Ordinary .NET projects use `manager.Reload(source)` / `await manager.ReloadAsync(source, token)`. `FileConfigSource.Read` reads files synchronously; `ReadAsync` uses asynchronous file streams. Synchronous entry points call only `Read`, and asynchronous ones only `ReadAsync`. Neither blocks on async operations nor disguises synchronous I/O as an async result. `DelegateSyncConfigSource` requires separate delegates, such as `new DelegateSyncConfigSource(files.Read, files.ReadAsync)`. For memory resources, explicitly provide `(path, token) => cachedFiles[path]` and `(path, token) => ConfigOperation<byte[]>.FromResult(cachedFiles[path])`.

Synchronous APIs require `ISyncConfigSource.Read`. Network-only sources may implement only `IConfigSource.ReadAsync`. Parameterless `Reload()` fails if the last successful source cannot read synchronously; you can explicitly pass a synchronous source. Synchronous calls occupy the calling thread and suit startup files or cached resources. Sync and async reloads share a reentrancy guard: a second reload fails immediately while one is pending, without queueing or reading resources. Retry after completion.

Call Unity entry points on the main thread. Subsequent resource calls, await continuations, and Changed notifications retain Unity's synchronization context. Manifest failure or cancellation preserves the previous snapshot and successful source. A valid manifest immediately switches versions and raises Changed; this does not guarantee every table file is usable. File hashes, row structures, and references are checked on access. Failed row/table decoding never publishes partial rows from that batch; existing cached rows remain usable and the same valid snapshot may retry. Table bytes that passed hash/index validation are retained independently.

`ReadAsync` returns `ConfigOperation<byte[]>`; immediate results use `ConfigOperation<byte[]>.FromResult(bytes)`, and callers await completion. Loading is not allocation-free: asynchronous state, file bytes, and new table snapshots allocate memory.

#### Lazy row loading and cross-table references

Default `Load / LoadAsync / Reload / ReloadAsync` reads only `current.json`, checking format, paths, duplicate table/file names, row-count bounds, and hash format. It does not enumerate the registry, create table codecs/cache containers, or read any `.bytes` files.

Each `XxxConfigTable` exposes its codec through a static field, explicit static constructor, and ordinary `GetCodec()` method. Generated `GetHero / GetHeroAsync / GetHeroTable / GetHeroTableAsync` calls pass that codec directly without central type lookup, factory registration, or reflection. Cross-table references call the target's `GetCodec()` only when non-null and uncached. Snapshots in the same assembly share stateless codecs; bytes, indexes, row objects, and reference caches belong to individual snapshots. Every table explicitly declares its primary key in Excel. Related generated comments are Chinese; an explicit class stores the codec's row-reading context instead of closures capturing local variables.

The tool generates the sole `TableCodecs`; regenerate after adding tables without manually editing the registry. `EnumerateAll()` enumerates at most 128 tables per segment for export and complete acceptance. `Registry` is an optional full-table catalog whose retrieval creates no codecs. Its `EnumerateCodecs()` is used only for explicit full validation; ordinary runtime queries never search the catalog. Use generated entry points in business code, or `snapshot.GetRow<HeroConfig, int>(HeroConfigTable.GetCodec(), id)` in generic typed code. Generated queries work without a catalog; full validation requires one.

Update Runtime and Unity adapters together with the new generated code. This API adjustment does not change the v4 binary format.

A table's first query checks manifest presence and schema compatibility; reading its resource also checks the file hash, header, and row index. Missing or incompatible unvisited tables do not block ordinary initialization; they fail explicitly on first access. A later table-query failure does not automatically roll back an already published ordinary reload. Use `ReloadAll / ReloadAllAsync` for complete validation before switching.

Each table remains one resource, containing three contiguous binary areas:

```text
Header and type map
Row index: primary key, reference Id, data-area offset, byte length
Row data: independently encoded rows; cross-table references still store Id
```

The generator uses `WriteRow / ReadRow` for row serialization and writes indexes plus data during export. Opening a resource parses only its header/index, without decoding unrelated rows. Index entries must be contiguous, non-overlapping, unique by primary key, and consistent with file length. Decoded rows are checked against their primary key, Id, and data bounds. Seekable output is written directly without an extra full-table buffer; non-seekable output is assembled in memory first.

Generated row APIs locate Excel-declared primary keys synchronously or asynchronously. Prefer generated methods such as `GetHero/GetHeroAsync`; generic code uses explicit codecs and `GetRow<T, TKey>` with actual typed keys. Dynamic tooling queries by `Type` use object and are unsuitable for per-frame access:

```csharp
ConfigSnapshot configs = await UnityConfigRuntime.LoadAsync(source, token);
HeroConfig hero = await configs.GetHeroAsync(1, token);
// Only this Hero row is created; skill, mentor, and further references remain unloaded.
SkillConfig skill = await hero.GetSkillAsync(token);
Debug.Log(skill?.Name);

// A cached reference returns immediately; a miss loads and caches its target row synchronously.
// Reading an unopened table resource requires ISyncConfigSource.
Debug.Log(hero.Skill?.Name);
// TryGet checks caches only, without resource reads or deserialization.
bool ready = hero.TryGetCachedMentor(out HeroConfig mentor);

// Cached rows can be retrieved synchronously.
HeroConfig sameHero = configs.GetHero(1);
SkillLevelConfig skillLevel = await configs.GetSkillLevelAsync(101, 1, 2, token);
// Generic API: composite keys use ValueTuple; generated APIs provide typed parameters.
HeroConfig sameRow = await configs.GetRowAsync<HeroConfig, int>(HeroConfigTable.GetCodec(), 1, token);
```

Project-owned AB/Addressables/other adapters return bytes through `IConfigSource`; the configuration runtime embeds no AB framework. Adapters own addressing, downloading, request coalescing, scheduling, caller cancellation, and handle release. Configuration owns row indexes, decoding, and object caches. Returned managed bytes must remain immutable and valid while an active snapshot uses them: indexes retain them for future row decoding, so releasing an AB handle must not invalidate them. With whole-table binary resources, the first access still reads and hashes all bytes but deserializes only the requested row. This is not AB-level row reading and does not reduce retained raw-byte memory; it reduces row objects, complex members, and query indexes.

Reference fields retain actual target types, such as `SkillConfig? Skill`. The getter returns its property cache first; otherwise it queries this snapshot, reads target bytes and decodes the row if necessary, then caches success. Target rows bind their own lazy resolvers without accessing further references. Reading Hero in `Hero → Skill → Effect → Item` does not request Skill; accessing `hero.Skill` loads Skill without loading Effect or Item. Chained access loads only each actually visited level. Repeated access reuses the same object without query allocations.

Each reference also generates `GetSkillAsync(token)` and `TryGetCachedSkill(out skill)`. Async access reads asynchronously and shares successful results with the synchronous property. TryGetCached returns false for null or unloaded targets without I/O or decoding; if an opened index proves the target ID invalid, it reports a reference error. The synchronous `hero.Skill` getter loads on a miss; use TryGetCached to inspect caches only. Naming collisions add numeric suffixes to generated helpers.

Zero/blank references return null without resource access. Missing nonzero IDs throw ConfigReferenceException containing the source row, field, and target ID. Missing files, hash errors, decoding failures, and cancellation are not cached as null, so later calls can retry. A row can load before its missing reference is discovered on access. Export and explicit full acceptance still check all references, including unvisited ones. Properties cache only successful objects; cycles return cached rows instead of recursively expanding the chain.

With an async-only source and unopened target bytes, synchronous reference access fails with guidance to use `GetSkillAsync`; it never blocks on async operations. Cached table bytes allow synchronous decoding of another row without synchronous I/O. Tables request resources independently, without a cross-table resource queue. Per-table decode coordination shares row identity across concurrent queries, although resource APIs may be called multiple times. Adapters own same-path coalescing, optionally using `CoalescingConfigSource` / `CoalescingSyncConfigSource`. Each caller must be able to cancel its own wait without aborting other callers. Synchronous queries use only synchronous sources and never wait for another async query; existing bytes allow direct decoding.

`GetHeroTable / GetHeroTableAsync` explicitly loads all rows, builds complete primary/group indexes, and reuses previously loaded row objects without expanding references. Synchronous `configs.GetHeroTable(token)` supports cancellation like the async entry point; the original parameterless overload remains for method-group compatibility. Group queries require the complete table.

`LoadedRowCount` counts decoded, bound rows. `Tables` lists completed tables without loading more. Successful reload invalidates the old snapshot and clears bytes, row caches, and tables. Old queries and reference access throw even for cached references; in-flight work cannot publish to the old snapshot. Reacquire objects from the new snapshot. Ordinary fields on old objects retain their values; held objects are not forcibly collected, and individual row unloading is unsupported. The viewer does not invoke reference getters.

Publishing the new Current and invalidating the old snapshot form one version transition. Old cleanup waits until the new Current is usable. Continuations recheck validity after returning to their context. Reentrant Changed handling does not deliver stale notifications; retrieve Current again after callbacks.

Disposing a manager permanently invalidates current/candidate snapshots and rejects further use. Resetting Unity runtime disposes active/initializing managers; late initialization fails. Resource adapters own their handles.

For full acceptance, construct ConfigManager with the Registry and manifest decoder, then call ReloadAll. It validates the complete table set, schemas, data, and references before atomically switching; failure retains the old version. Snapshot LoadAll prewarms the current snapshot. Ordinary defaults remain row-lazy.

Optionally use `ConfigPreloadPlan` with `ReloadPrepared` / `ReloadPreparedAsync`: publish only after critical loads succeed, preserving the old version on failure or cancellation. See [Prewarming and diagnostics](#prewarming-and-diagnostics).

v4 does not support old v2/v3 files. Regenerate and export, update Runtime/Serialization/Unity code together, preserve old releases, and do not mix formats.

#### Prewarming and diagnostics

Choose when to switch versions according to your requirements:

| Requirement | Entry point |
| --- | --- |
| Switch as soon as possible | Default manifest-only reload; table errors appear on later access |
| Prepare critical data before switching | ReloadPrepared with explicit rows/tables; references are not expanded |
| Validate everything before switching | Manager ReloadAll with Registry |
| Prewarm the current version | GetTableAsync without republishing |

Initialize asynchronously on the main thread. Import the required namespaces and replace source/token with actual values. This replaces the first Load; do not initialize twice.

```csharp
var options = new ConfigLoadOptions(enableDiagnostics: true);
UnityConfigRuntime.Initialize(options);

var plan = new ConfigPreloadPlan()
    .WithRow<HeroConfig, int>(HeroConfigTable.GetCodec(), 1)
    .WithRow<SkillConfig, int>(SkillConfigTable.GetCodec(), 10)
    .WithTable<DropConfig>(DropConfigTable.GetCodec());

var snapshot = await UnityConfigRuntime.ReloadPreparedAsync(source, plan, token);
var hero = snapshot.GetHero(1); // The prewarmed Hero row is reused.
ConfigLoadDiagnostics diagnostics = snapshot.Diagnostics;
UnityEngine.Debug.Log($"Loaded {snapshot.LoadedRowCount} rows; table bytes: {diagnostics.RetainedBytes}");
```

Use `ReloadPreparedAsync` for subsequent updates without reinitializing. `ConfigPreloadPlan` is immutable: each `WithRow` / `WithTable` returns a new plan; ignoring it discards that step. Plans include only explicitly listed rows/tables. Hero referencing Skill does not prewarm Skill automatically, and preparation is not full reference validation. Add targets explicitly when required. Failed first initialization leaves no valid snapshot; failed/canceled preparation with an existing snapshot preserves it and can be retried after repair.

`ConfigLoadOptions` is fixed when constructing the manager. Decoding runs on the calling thread by default; async I/O does not automatically spread it across frames. If the platform supports threads and codecs/converters are thread-safe and avoid Unity main-thread APIs, explicitly pass `scheduler: BackgroundConfigLoadScheduler.Instance`. This schedules only async cold-load validation, decoding, and index construction. It neither converts sync APIs into async nor schedules AB downloads. WebGL does not support this background policy.

Read diagnostics through `snapshot.Diagnostics`; `Enabled` is false when disabled. `OpenedTableCount`, `RetainedBytes`, `LoadedRowCount`, and `CompletedTableCount` describe current caches. Read/hash/index/decode/table-build times accumulate, including failed attempts. Read time includes source waits. RetainedBytes counts retained raw table bytes only, excluding row objects, indexes, AB frameworks, and total Unity memory. Measure warm cache hits and cold first-row loads separately; .NET measurements do not establish Unity/IL2CPP performance.

### 14.5 Android, iOS, WebGL, and StreamingAssets

`FileConfigSource` suits real desktop paths. Android APK `jar:` paths and WebGL URLs require async requests. Version 1.4.0 includes `ZrConfig.Unity.StreamingAssetsConfigSource` in the [Unity adapter](../unity/ZrConfig.Sample/Assets/ZrConfig/Unity/Runtime/StreamingAssetsConfigSource.cs), distributed with `package-unity` source packages.

Use this in the first-load entry point on Unity's main thread:

```csharp
// using ZrConfig.Runtime;
// using ZrConfig.Unity;
// using ZrConfig.Models;
var streaming = new StreamingAssetsConfigSource();
var source = new CoalescingConfigSource(streaming);
ConfigSnapshot snapshot = await UnityConfigRuntime.LoadAsync(source, token);
HeroConfig hero = await snapshot.GetHeroAsync(1, token);
SkillConfig skill = hero == null ? null : await hero.GetSkillAsync(token);
```

The default root is `Application.streamingAssetsPath + "/ZrConfig"`; the constructor also accepts your actual root. Sources receive relative paths such as `current.json` and `versions/<version>/Hero.bytes`. The adapter validates paths and reads asynchronously through UnityWebRequest, with cancellation and request errors. `ReadCount` and `Trace` expose actual requests; `CoalescingConfigSource` merges concurrent same-path reads.

Construct and call this adapter on Unity's main thread, without background scheduling or blocking on async operations. It implements only `IConfigSource`: use `GetHeroAsync`, `GetSkillAsync`, and other async APIs for first queries and uncached references. Cached data supports synchronous queries. Do not use desktop-style `hero.Skill` to fetch an unopened resource. Reload with `ReloadAsync`; full validation before switching requires full-load entry points with `TableCodecs.Registry`.

The ordinary sample automatically uses this async path in Android/iOS/WebGL Players. Editor and desktop still demonstrate synchronous/asynchronous file bridges. Build acceptance and actual device/browser execution are separate; see [Real Player entry points](release-and-acceptance.en.md#real-player-entry-points) for commands, reports, and tested scope. For a specific AB, Addressables, or hot-update framework, connect its SDK through the following resource interface.

#### Third-party AB frameworks: synchronous and asynchronous loading

The repository includes a playable [Unity AB example](../samples/README.en.md#synchronous-and-asynchronous-assetbundle-sample), switching sync/async loading, queries, and reloads in one scene with resource traces. Unselected SDK call sites are isolated in `SampleAbResources`, temporarily using actual file reads; the rest of configuration loading executes fully.

Until a framework is selected, use `DelegateSyncConfigSource` as the integration boundary. Implement the two functions below once selected; configuration loading, row queries, caching, and reload APIs remain unchanged. ZrConfig neither installs an AB SDK nor requires native Unity AssetBundle objects.

| Project-provided function | Return type | Responsibility |
| --- | --- | --- |
| `LoadBytes(string path, CancellationToken token)` | `byte[]` | Call the framework's actual synchronous read API |
| `LoadBytesAsync(string path, CancellationToken token)` | `ConfigOperation<byte[]>` | Call its actual async API and return bytes on completion |

Your integration implements these bridge functions; they are not built-in ZrConfig methods. Bind them directly without another factory or manager:

```csharp
var source = new DelegateSyncConfigSource(LoadBytes, LoadBytesAsync);
```

Async-only frameworks can use `DelegateConfigSource`, but cannot promise synchronous reads of unopened resources. Verify actual sync support on required platforms when choosing a framework. Remote resources generally must be downloaded/prepared beforehand; blocking on async downloads is not genuine synchronous support.

Choose either initial setup below. Afterward, query APIs may be mixed and share the same row cache:

```csharp
// Sync chain: Load -> source.Read; GetHero / Skill also use only Read when bytes are missing.
ConfigSnapshot configs = UnityConfigRuntime.Load(source, token);
HeroConfig hero = configs.GetHero(1, token);
Debug.Log(hero.Skill?.Name);
bool cached = hero.TryGetCachedSkill(out var sameSkill);
```

```csharp
// Async chain: LoadAsync -> source.ReadAsync; subsequent resource reads also use ReadAsync.
ConfigSnapshot configs = await UnityConfigRuntime.LoadAsync(source, token);
HeroConfig hero = await configs.GetHeroAsync(1, token);
SkillConfig skill = await hero.GetSkillAsync(token);
Debug.Log(skill?.Name);
```

`source` binds the two read functions; `token` is caller cancellation. Manifest/table paths are relative: `current.json`, `versions/<version>/<manifest table filename>`. Bridges map these to resource addresses and return bytes, not URLs or text. Callback AB APIs can use `ConfigCompletionSource<byte[]>` to complete success, failure, or cancellation separately. Do not disguise sync reads as async, poll AB operations, or block waiting for them.

Delegate adapters only forward reads. Wrap them with `CoalescingConfigSource` (async) or `CoalescingSyncConfigSource` (both APIs) to merge in-flight same-path requests with independent caller cancellation. Entries disappear on completion; manifests and failures are not cached. `SampleAbResources.Source` already wraps the dual-API coalescer. Your integration still owns path mapping, downloads, handle release, and stable byte ownership. Managed bytes must remain valid and immutable throughout snapshot use; releasing framework handles must not reclaim or overwrite them.

First complete the framework's resource update and ensure the next `current.json` read returns the new manifest. Then call `UnityConfigRuntime.Reload(token)` or `await UnityConfigRuntime.ReloadAsync(token)`. If delegates bind a fixed version, create and pass a new source. Failure before switching preserves the old snapshot/source; success requires reacquiring objects from new Current. The resource layer releases unused resources after old requests finish. Default reload checks basic manifest format only; table presence, schema, and actual files are validated on query.

Configuration retains v4 row indexes and loaded objects without duplicating AB caches or download scheduling. Hashing, decoding, and index construction run in the calling context after bytes arrive. Async I/O does not automatically distribute decoding over frames.

### 14.6 Do old variables automatically become new data after reload?

No. Ordinary fields on an old hero retain their old values. Retrieve configuration objects again after updating.

A valid manifest replaces Current; rows/tables are retrieved lazily from the new snapshot. Manifest failure preserves Current; row failure publishes no partial batch. Successful updates invalidate old snapshot queries and old-row reference access, including cached references. Ordinary fields are not mutated in place; business code should stop using old objects.

```csharp
ConfigSnapshot snapshot = UnityConfigRuntime.Current;
HeroConfig hero = snapshot.GetHeroTable()[0];
```

Use one snapshot when a game operation needs multiple tables. A successful reload during the operation invalidates old queries. In Changed handlers, cancel or discard old queries and retrieve required objects from new Current.

### 14.7 Automatically refresh UI on updates

Subscribe to `UnityConfigRuntime.Changed`:

```csharp
private void OnEnable()
{
    UnityConfigRuntime.Changed += OnConfigChanged;
}

private void OnDisable()
{
    UnityConfigRuntime.Changed -= OnConfigChanged;
}

private void OnConfigChanged(ConfigSnapshot snapshot)
{
    // The new manifest is ready. Load rows/tables asynchronously from this snapshot, then update UI.
}
```

Put this snippet in a MonoBehaviour referencing ZrConfig.Runtime and ZrConfig.Unity. Unsubscribe when no longer listening. Events report successful changes only; exceptions from individual listeners are logged and isolated from other listeners.

Treat configuration as read-only. Generated row properties are not freely assignable, but arrays and custom objects are not deep-copied. Do not mutate them as character state. Store current health in runtime character objects; maximum-health configuration belongs in tables.

### 14.8 What must a hot-update project synchronize?

Data-only changes: publish a complete new data version and its current.json.

Field, handwritten member, enum, subtype-set, or converter changes: regenerate/compile Config and codecs, export new data, then publish code and data together through your hot-update workflow.

Schema Hash identifies structure. Rejecting mismatched code/data protects compatibility; deleting current.json does not solve the mismatch.

Open the Unity viewer at `Tools > ZrConfig > 运行时配置表` (Runtime configuration tables) after loading configuration. It displays the current snapshot and does not replace Excel editing/export.

### 14.9 Generic viewer and Odin

After [integrating Unity source](#142-integrate-unity-source) into Assets/ZrConfig, the generic viewer compiles with it without extra installation.

- `Tools → ZrConfig → 运行时配置表`: select a loaded table.
- Add `ZrConfig/配置表查看器` (Configuration table viewer) to a GameObject, specify a table name, and inspect it.
- Without Odin, use the paged read-only viewer. Installing Odin and enabling `ODIN_INSPECTOR` enables Odin tables.
- A generic editor property processor displays generated read-only properties without adding Odin attributes to Models.
- The viewer reads the game's actual snapshot and refreshes after hot updates; ordinary tables need no separate View class.

Odin integration follows the official [PropertyTree](https://odininspector.com/documentation/sirenix.odininspector.editor.propertytree) and [AttributeProcessor](https://odininspector.com/documentation/sirenix.odininspector.editor.odinattributeprocessor) APIs. Integration has not been tested with a real Odin installation or IL2CPP Player; validate those environments in your project.

### 14.10 Ordinary .NET integration

Without Unity, use ConfigManager directly and inject a manifest decoder. Place this in an existing async initialization method; configOutputFolder is the actual output root. Ordinary queries need no registry:

```csharp
using System.Text.Json;
using ZrConfig.Models;
using ZrConfig.Runtime;

var manager = new ConfigManager(
    json => JsonSerializer.Deserialize<ConfigManifest>(json,
        new JsonSerializerOptions { IncludeFields = true })!);
await manager.ReloadAsync(new FileConfigSource(configOutputFolder));
ConfigSnapshot snapshot = manager.Current;
```

In class files, put using directives at the top and initialization/loading statements inside an async method. IncludeFields handles manifest fields. Reuse the manager for later updates and retrieve a new snapshot from Current. Resource access can implement IConfigSource or use DelegateConfigSource.

## 15. Boundaries when designing rules

### 15.1 Check whether validation is possible before checking correctness

Do not treat rows with parse failures as complete objects. The tool isolates incomplete tables; business validation should also skip blank or out-of-range prerequisite values to avoid misleading errors.

Example: blank Level causes Required to report “missing”. Do not also report that a level-0 hero must select a beginner skill, because 0 was not the actual input.

### 15.2 Use Error for invalid business data and Warning for suspicious data

Error prevents publication, for example a missing target skill.

Warning permits use but requests review; zero skill damage may be valid for a support skill.

Do not downgrade blocking errors to Warning or hide failed checks by swallowing exceptions.

### 15.3 Supported types do not imply support for every built-in attribute

Listing decimal, uint, dictionaries, and other types means they can be parsed, generated, and serialized. Built-in Range still follows section 4.5's type restrictions.

Use a matching `IFieldRule<T>` or Validate for special conditions. Rule attributes cannot be applied arbitrarily to every field.

### 15.4 Split-column paths and JSON affect input only

The same logical field can use one JSON cell or split member columns. Generated models remain objects; property names do not contain dots.

Schema identifiers use logical fields and types. Switching input layouts without changing logical structure does not create runtime members. Field order, type, or complex-structure changes still require synchronized code/data.

### 15.5 Reader and writer versions must match

Current tables use the v4 row-index binary format, while the manifest container identifier remains ZrConfig.Binary/1. The “1” does not mean the table format is old v1.

New codecs cannot directly read old v1 data. When upgrading Runtime, generated source, or serialization protocols, regenerate, export, and deliver them together.

Serialization support files use content-based versions. Recognized unmodified files can upgrade automatically; handwritten changes and unknown old files are preserved with merge guidance. Ordinary CLI `export` and ExportPipeline do not edit source; recognized older support requires generation or `support` first. CLI `export --generate` updates code through the combined flow; the App asks before generation upgrades during export. See [Support-source upgrades](development.en.md#support-source-upgrades).

### 15.6 Arbitrary C# types cannot automatically become Excel configuration

See [Excel type syntax](config-types.en.md) for cell types and chapters 1–2 and 10–11 for implementations. Arbitrary `List<T>`, game objects, UnityEngine.Object, and generic data classes are not ordinary Excel field types.

Use supported arrays or Dictionary for collections. Converters support special scalar types. Keep complex game behavior in game code.

## 16. Common code reference

| Task | Code |
| --- | --- |
| Require a top-level field | `[Required(nameof(HeroConfig.Name))]` |
| Make a top-level field unique | `[Unique(nameof(HeroConfig.Name))]` |
| Positive unique integer ID | `[PositiveKey(nameof(HeroConfig.Id))]` |
| int level from 1 to 100 | `[Range(nameof(HeroConfig.Level), 1, 100)]`; also add Required if mandatory |
| Require an existing skill ID | `[Reference(nameof(HeroConfig.SkillId), typeof(SkillConfig), nameof(SkillConfig.Id))]` |
| Allow no skill for 0 or blank | Add `AllowDefaultValue = true` to Reference |
| Bind a display-name field rule | `[Check(nameof(HeroConfig.Name), typeof(DisplayNameRule))]` |
| Read Skill in a custom post-validation rule | `[DependsOn(typeof(SkillConfig))]` |
| Require a handwritten object member | Put `[ConfigRequired]` on the member |
| Validate each row | `protected override void Validate(HeroConfig row, ValidationContext context)` |
| Validate the whole table | `protected override void ValidateTable(IReadOnlyList<HeroConfig> rows, ValidationContext context)` |
| Check whether the original cell was blank | `context.IsEmpty(row, nameof(HeroConfig.Level))` |
| Check whether a nested member was blank | `context.IsEmpty(row, "Settings.MaxTargets")` |
| Retrieve another table | `context.GetSkillTable()` reuses primary/group indexes; `context.GetTable<SkillConfig>()` returns the same indexed table |
| Report a row-rule error | `context.Error(row, nameof(HeroConfig.Level), "Message", "CODE")` (message, code) |
| Report a row-rule warning | `context.Warning(row, nameof(HeroConfig.Level), "Message", "CODE")` (message, code) |
| Report a field-rule error | `context.Error("Message", "CODE")` (message, code) |
| Target an array member | `context.Error(row, $"Actions[{i}].Delay", "Message", "CODE")` (message, code) |

Placement matters: attributes go above classes, if statements inside methods, and using directives at file start. Do not paste every snippet indiscriminately at the end of one file.

## 17. Tutorial validation and repeatable scope

After completing the tutorial, check your configuration and rules as follows.

1. Extract the [exercise attachment](assets/user-guide-example.zip) into a system temporary directory. The seven complete source files in sections 12.3–12.9 use the same types, rule codes, and behavior as the attachment. The English snippets translate comments and diagnostic messages; the shared ZIP retains the original diagnostic text. Do not mix these files into official samples.
2. Run chapter 13's `codegen` and `export` against the exercise directory. Expect 2 Hero rows, 2 Skill rows, zero errors, and one expected warning.
3. After modifying examples, deliberately introduce each error from section 12.11. Confirm publication is blocked and current.json bytes remain unchanged; restore the data and confirm export succeeds. A negative delay on the second action should locate Hero row 7, column 12, path Actions[1].Delay.
4. Confirm logs remain the two fixed files export.log and diagnostics.json. Keep temporary files out of official Data, Config, and Output.


This validates sample generation, rule behavior, file placement, and compilation. It does not establish deployment in your own game, mobile, WebGL, IL2CPP, or a particular hot-update framework.

## 18. Type implementation and compatibility constraints

Project programmers and tool maintainers handle this section. Table authors need not implement or upgrade generic serialization source.

Put business types in Config/CustomTypes and optional converters in Config/Serialization. ConfigSourceLayout initializes common support files and upgrades them by content version, preserving handwritten changes with merge guidance. They remain project source: do not simply delete old files without verifying project modifications and Runtime/codec compatibility.

Classes must be public with a public parameterless constructor. Serializable fields must be writable; properties require public get/set. Inherited members participate. Hidden same-name members, indexers, read-only/write-only/init properties, and generic data classes are unsupported. Converters can support special readonly structs. Constructors and setters must avoid business side effects.

Converter Parse/Write/Read must share conventions and invariant formatting. Instances are reused, so do not retain mutable state from previous cells. Binary-representation changes require incrementing the ConfigConverter version. Business types and converters should remain pure C#, independent of UnityEngine/UnityEditor, and compatible with the game's C#/.NET APIs.

Rectangular-array bounds are handled by ReadDimension, ClaimElements, and ReadLength in Serialization/ConfigReadContext.cs. Keep generation templates synchronized with the repository's sole support-source copy.

SchemaHash uses logical fields, order, and type structure; JSON/split-column/multi-row input layouts are excluded. Layout-only changes do not alter runtime fields. Code and exported data must share a schema identifier.

Generated codecs access members and serialization methods directly without runtime field reflection. Arrays store lengths then elements; empty reads use Array.Empty. Strings use stack buffers/ArrayPool to avoid temporary UTF-8 byte arrays. Actual strings, nonempty arrays, objects, and rows still allocate; zero GC is not promised. Table properties are read-only, but array elements/custom objects are not deep-copied; treat loaded configuration as immutable. Export-side discovery/construction uses cached metadata and reflection without statically retaining project assemblies after export.

Ordinary values carry no per-value type tags. Polymorphic positions store file-local indexes, with full type names centralized in headers. Reading maps types once and dispatches through generated switches. These indexes are not stable business IDs. Schema hashes include binary version, class/struct shape, transitive members, required declarations, enums, subtype sets, and converter versions. Renaming classes or changing structures requires regenerated code/data.

See [15.5 Reader/writer versions](#155-reader-and-writer-versions-must-match). Maintainers changing the underlying format must also increment BinaryFormat.Version. Project programmers regenerate compatible code/data and deliver Runtime, codecs, and data together. The tool does not automatically migrate old binary data.

Read limits: 1,000,000 elements per array, 8,000,000 total array elements per table, recursion depth 64, and 16 MiB of UTF-8 per string. Oversized/truncated data, invalid indexes, and invalid type tables are rejected. Failed publication preserves the previous current.json.

Unsupported cases remain arbitrary generic containers except Dictionary, arrays of rank above 3 or nesting deeper than three levels, and direct serialization of game-behavior/UnityEngine objects. Converters can extend special types.
