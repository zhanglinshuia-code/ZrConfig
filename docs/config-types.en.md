# Excel types and input syntax

[中文](config-types.md) | **English**

See [multi-language generation and JSON](server-generation.en.md) for server generation and Binary / JSON / dual-format export.
This document explains how to fill existing fields. Start with the [user guide](user-guide.en.md) for everyday operations and the [all-types workbook](type-examples.en.md) for actual examples.

Developers provide templates and definitions for custom enums, classes, action kinds, and currency formats. Authors use those type names, members, and examples without writing types or converters first. See [project development](project-development.en.md) for new data shapes, special formats, and shared serialization support.

## Choose a table structure

A table can combine primitive fields, single-cell collections, objects split across columns, and multi-row lists. Choose a layout that matches the data, then look up the detailed syntax below.

| Data to represent | Excel layout | Imported structure |
| --- | --- | --- |
| An item or level configuration | One record per row, one column per field | A strongly typed record |
| Values, a dictionary, or a matrix | An array, dictionary, or `json<T>` in a cell | The corresponding array, dictionary, or object |
| Settings that form one object | Dotted member columns such as `Settings.MaxTargets` | One Settings object |
| A field with alternative action kinds | `Action.$type` and subclass member columns | An instance of the selected subclass |
| Multiple actions in one record | `Actions[].Member` over several rows, merged by the complete primary key | An object array |
| Phases containing groups containing actions | `Phases[].Groups[].Actions[].Member`; parent `$order` identifies ownership | Nested object arrays, sorted independently at each level |
| Several fields jointly identify a record | Mark multiple fields with the literal `主键` on row 3 | A composite primary key and a group index per key field |
| A record in another table | A top-level field of target type `XxxConfig`, containing its Id | A directly referenced configuration object |

See [structure syntax](#split-objects-polymorphic-type-columns-and-multi-row-action-lists) for split columns, polymorphism, and multi-row input, and [primary keys](table-indexes.en.md) for key/reference restrictions. After grouping, sorting, and linking objects, projects can run [post-validation](rules-development.en.md#2-post-means-after-parsing-before-publication) on these structures.

## Client and server fields

Every table uses five header rows: field names, types, primary keys, descriptions, and **target side on row 5; data starts on row 6**. **Row 5 is mandatory and every field must explicitly declare `C`, `S`, or `CS`**. Missing markers block generation and export. No layout switch in `Config/Tables.json` is needed.

| Excel row | Id | Icon | DropWeight | Name |
| --- | --- | --- | --- | --- |
| 1: Field | Id | Icon | DropWeight | Name |
| 2: Type | int | string | int | string |
| 3: Primary key | `主键` (literal marker) | | | |
| 4: Description | Identifier | Icon | Server drop weight | Name |
| 5: Target side | CS | C | S | CS |
| 6: Data | 1001 | item_1001 | 50 | Small potion |

- `C`: Included only in client generated code and binary data.
- `S`: Included only in server generated code and binary data.
- `CS`: Included on both sides. Markers ignore case and surrounding whitespace; other values such as `SC` are errors.

Select C or S in the App's “Generation / export target”, then generate, validate, and export. Project settings retain this choice; older settings default to C. After changing markers or target side, regenerate matching code. See [generation and export by side](project-development.en.md#generateexport-by-side).

Primary keys must explicitly use `CS`. Blank ordinary fields are errors; there is no default side. Split objects, polymorphic objects, and multi-row arrays are selected **as a whole top-level field**: `Settings.X` and `Settings.Y` must agree. If only one column in a group declares C, blank columns inherit C; an entirely blank group is invalid. The whole group belongs to C; handwritten types are not split by member. Records still group by the original primary key even if a side excludes all multi-row fields. Values excluded from a side are not parsed, reference-checked, or written for that side, but headers, types, and markers must remain valid. Business rules use the current side's model and cannot access excluded fields.

An existing `headerRows: 5` declaration may remain or be removed; `headerRows: 4` is rejected. For old four-row workbooks, insert a target-side row at row 5, move all data down, and explicitly fill `C`, `S`, or `CS` before regenerating and exporting. Generation/export neither inserts rows nor infers layouts. Do not leave the first data record in the target-side row.

## Types and cell values

Type names are case-sensitive, except built-in numeric keywords. Custom types accept short or fully qualified names; name collisions require the latter. Use field/type/key/description/side on rows 1–5 and data from row 6. Business fields may form the key; no extra `int Id` is required. See [Excel keys and groups](table-indexes.en.md). Field names starting with `__zr` are reserved.

AllTypes uses the same five-row header, with data from row 6 and the same cell syntax. See the [all-types workbook](type-examples.en.md) for its layout.

| Type on Excel row 2 | Cell example | Generated type |
|---|---|---|
| byte / short / int / long | 255 / -100 / 3001 / 9223372036854775807 | Corresponding C# type |
| sbyte / ushort / uint / ulong | -128 / 65535 / 4294967295 / 18446744073709551615 | Corresponding C# type |
| decimal | `1.2345678901234567890123456789` | Exact decimal, stored as text |
| float / double | 1.25 | Corresponding C# type; finite values only |
| bool | true / false / 1 / 0 | bool |
| string | Plain text, or quoted text such as `"with;delimiter"` | string |
| SkillConfig | `2001` (Id in Skill) | SkillConfig?, directly referencing the target row object |
| Quality | Rare | Custom enum Quality |
| vector2 | 1.5,2 | ZrConfig.Types.Vector2（X、Y） |
| vector3 | 1,2,3 | ZrConfig.Types.Vector3（X、Y、Z） |
| kv | 3001,10 | ZrConfig.Types.Kv（int Key、int Value） |
| int[] | `[1;2;3]` or `1;2;3` | int[] |
| kv[] | `[3001,10;3001,20]` | Kv[], preserving order and duplicate keys |
| Quality[] | `[Rare;Legendary]` | Quality[] |
| vector2[] | `[1,2;3,4]` | Vector2[] |
| int[][] | `[[1;2];[];[3]]` | Jagged array; inner lengths may differ |
| int[][][] | `[[[1;2];[3]];[[4]]]` | Three-level jagged array |
| int? / Quality? | `123` / `Rare` / `null` / blank | Nullable value type |
| int?[] | `[1;null;0]` | Array of nullable elements |
| `Dictionary<string,int>` | `["hp"=100;"attack"=25]` | Strongly typed dictionary |
| `Dictionary<int,decimal?>` | `[1001=1.25;1002=null]` | Dictionary with nullable values |
| int[,] | `[[1;2];[3;4]]` | Two-dimensional rectangular array |
| decimal[,,] | `[[[1.25;2.5]];[[3.75;4.5]]]` | Three-dimensional rectangular array |
| Settings | `{"Limit":7}` | Handwritten Settings type |
| json<Settings> | `{"Limit":7}` | Settings with JSON input |
| json<int[][]> | `[[1,2],[],[3]]` | int[][] with standard JSON separators |
| json<BattleAction[]> | `[{"$type":"Damage","Ratio":1.5}]` | BattleAction[] |
| json | `{"custom":[1,true]}` | RawJson; Text preserves the original JSON text |

Supported value/object types can take `[]`, `[][]`, or `[][][]`, except explicitly restricted types such as direct table references. Direct table references currently work only as top-level scalar fields, not array elements, dictionary values, or nested members. `json<T>` is a cell input format, with no runtime wrapper. Custom objects without split member paths still parse from single-cell JSON; split and multi-row objects are built from individual cells. Vectors/kv use numeric arrays in JSON, e.g. `[[1,2],[3,4]]` for `json<vector2[]>`.

Write `[]` for an empty array; a blank array cell also yields an empty array. Bracket every nested level; `[1;;2]` is invalid. Use JSON string escaping for `;`, `,`, brackets, or quotes: `["a;b";"x,[]";"quote\""]`. An empty string element in a one-dimensional string array is `""`. Plain unquoted string cells retain spaces; entirely blank cells become empty strings.

Enums require declared member names, not arbitrary numbers or unknown names. A blank enum cell is valid only if a member with value 0 exists. Enter long, ulong, and high-precision decimal values as text so Excel does not round significant digits before export. Floating-point parsing uses invariant culture and `.` as decimal separator.

Vectors are pure C# structures without UnityEngine dependencies; convert as needed with `new UnityEngine.Vector2(value.X, value.Y)`. A kv is two integers, not a dictionary. Business rules determine whether they mean item and quantity.

## Split objects, polymorphic type columns, and multi-row action lists

Split objects and multi-row lists use the same five-row header. Lists group by the complete primary key. Put the literal `主键` only in key columns on row 3, marking at least one; do not mark member paths, action-order columns, or collections. Inspect actual values, objects, and collections on the first sheet of [AllTypes.xlsx](../samples/Data/AllTypes.xlsx), using the type index to navigate to Actions or Range. Later sheets provide only indexes and instructions.

**Split objects** use dotted paths such as `Settings.MaxTargets`, `Settings.Interruptible`, and nested `Settings.Window.Min`. Row 2 declares the group's **root type**, e.g. `Game.Config.BattleSettings`. Fill one type cell in the group and leave the rest blank, or repeat the same root type in every column. Member types come from C# definitions; do not mix `int` and `bool` in neighboring member columns. Row-2 type cells may be merged within a group; row-1 field paths must not be merged.

| Row | Id | Settings.MaxTargets | Settings.Interruptible |
|---|---|---|---|
| Field (1) | Id | Settings.MaxTargets | Settings.Interruptible |
| Type (2) | int | Game.Config.BattleSettings | |
| Primary key (3) | `主键` (literal marker) | | |
| Description (4) | Identifier | Maximum targets | Interruptible |
| 5: Target side | CS | CS | CS |
| Data (6) | 1001 | 3 | false |

**A single polymorphic object** uses columns such as `Action.$type`, `Action.Delay`, `Action.Ratio`, and `Action.BuffId`, with root type `Game.Config.BattleAction` on row 2. `$type` contains the concrete subclass's short or full name. Fill only that subclass's fields; leave inapplicable columns blank. A business field named `Type` is distinct from reserved `$type`.

**Multi-row arrays** add `[]` after the root field, e.g. `Actions[].$type` and `Actions[].Delay`, with `Game.Config.BattleAction[]` in the type row. Rows sharing a complete key merge into one record; each nonempty action group creates one array element. Ordinary non-polymorphic object arrays are also supported; only polymorphic objects need `$type`.

The following shows only field names and data, using Id as the sole primary key. In a real workbook, retain types on row 2, `主键` only under Id on row 3, descriptions on row 4, target sides on row 5, and start these values on row 6:

| Id | Name | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
|---|---|---|---|---|---|---|---|---|
| 1001 | Slash | 1 | Damage | 0.2 | 1.5 | true | | |
| | | 2 | AddBuff | 0.5 | | | 2001 | 3 |
| 1002 | Fireball | 1 | Damage | 0 | 2.0 | true | | |

- Only sheets containing expanded `[]` columns merge rows by complete key. Ordinary tables and tables with only split objects still have one record per row.
- The first occurrence of a record must fill every key component. Continuation rows may repeat the complete key, even non-contiguously. All key cells blank continues the preceding valid main record; partially filled keys are invalid. Blank keys after an invalid key do not attach to an earlier record. This example has only Id.
- Main fields such as Name and Settings come from the first row for that complete key. Later rows must leave them blank or repeat the first value exactly. Conflicts block export.
- `$order` is an input helper, not a generated member. Without child lists it is optional and physical row order applies; when declared, each nonempty element needs a unique positive integer. Parents with child lists must declare `$order`; repeat the parent number on continuations. Each level sorts independently within its current parent; numbers need not be consecutive.
- Leaving all expanded array columns blank adds no element; a record with none gets an empty array. Filling only order or parameters while omitting a required polymorphic type is an error.
- Blank object members retain project defaults. `ConfigRequired` members must be explicit; `0` and `false` count. Uninitialized strings, arrays, and dictionaries become empty values. Explicit `null` for a nullable member also triggers required-value validation.
- Nested multi-row object arrays are supported, e.g. `Groups[].Actions[].Ratio` and `Phases[].Groups[].Actions[].BuffId`, as are lists inside ordinary objects such as `Plan.Groups[].Name`. Ordinary leaf arrays can still use single-cell input.
- Do not declare both a single-cell `Actions` and expanded `Actions[].X`, or both a single-cell `Settings` and its member columns. Do not wrap split root types in `json<T>`.

Splitting columns and rows is only an input layout; dotted paths do not become new ordinary fields. Errors retain actual sheet coordinates. For example, Actions[1].BuffId means BuffId of the second action after sorting; edit the Excel row/column given in the report.

Before changing an existing project's layout or upgrading, have a developer check code/data compatibility. See [type implementation and binary constraints](project-development.en.md#18-type-implementation-and-compatibility-constraints).

## Nested multi-row lists

In `AllTypes`, `Groups` has root type `Game.Config.ActionGroup[]`. Only selected fields and data are shown below. For a similar table, put types on row 2, `主键` under Id on row 3, descriptions on row 4, sides on row 5, and data from row 6:

| Id | Groups[].$order | Groups[].Name | Groups[].Actions[].$order | Groups[].Actions[].$type | Groups[].Actions[].Ratio | Groups[].Actions[].BuffId |
|---|---|---|---|---|---|---|
| 2 | 2 | Follow-up | 1 | Damage | 0 | |
| 2 | 1 | Opening | 1 | Damage | 0.8 | |
| 2 | 2 | | 2 | AddBuff | | 0 |

The result has two groups: `Groups[0]` is “先手” (Opening), `Groups[1]` is “后手” (Follow-up), with two actions in the latter. Repeating parent order continues its child list. Fill parent name, type, and other parameters only on that group's first row; later rows may be blank or identical, otherwise an error is raised. Child order need only be unique within its parent. Every parent number must be explicit; blank cells never infer nested ownership. An entirely blank child list becomes an empty array.

`Phases[].Groups[].Actions[]` demonstrates three levels. Rules apply recursively, up to an overall depth of 64. Expanded multi-row nodes must be one-dimensional object arrays; value arrays such as `int[,]` use single-cell input. Post-validation may address `Groups[1].Actions[0].Ratio` and still locate the original unsorted Excel cell.

## Nullable values, dictionaries, and rectangular arrays

- **Nullable `T?`:** Supports numbers, bool, enums, vectors, kv, custom structs, and provided special value types. Blank or `null` means absent; `0` and `false` are values. `ConfigRange?` can be split across columns; an entirely blank group yields null. Unfilled object members retain defaults. Reference-type annotations such as `string?` or array `?` are not part of this input syntax.
- **Dictionaries:** Use `Dictionary<TKey,TValue>` (also accepts `dictionary`). A cell contains `[key=value;key=value]`; blank or `[]` is empty. Quote string keys containing semicolons, equals signs, brackets, etc.; `""` is an empty string key. Nested example: `Dictionary<int,Dictionary<string,int[]>>` with `[1=["a"=[1;2]];2=[]]`. Values may use supported types; object values still follow object cell syntax. Prefer split columns and multi-row input for battle objects.
- **Dictionary keys:** Integers, bool, string, decimal, and enums are supported; nullable, float/double, object, and collection keys are rejected. Duplicate checks use parsed keys, so integer `1` conflicts with `01`. Dictionaries do not preserve authoring order; use arrays or list layouts when order matters.
- **Rectangular arrays:** Each dimension of `T[,]` and `T[,,]` uses brackets and semicolons; sibling lengths must match. `[[1;2];[3]]` requires jagged `T[][]`. Blank or `[]` gives zero length in every dimension; `[[];[]]` is 2×0. Binary data stores dimension lengths, caps element counts, and rejects irregular shapes, negative or excessive dimensions, and truncation.
- **Combinations:** Supports `int?[]`, `uint?[,]`, dictionary arrays, and up to three nested array levels with rank at most 3 each. Mixed suffixes follow C#: `int[][,]` is an array of matrices; `int[,][]` is a matrix of arrays. `json<Dictionary<int,int?>>` and `json<uint?[,]>` also accept standard JSON.

When first adding rectangular arrays to an older project, have a developer confirm support code compatibility. Missing read/write methods or similar compile errors require the [upgrade procedure](project-development.en.md#18-type-implementation-and-compatibility-constraints).

## Project-provided complex types

- Ordinary objects use the template's members. Enter JSON for single-cell objects or values in the appropriate split columns; do not declare the same field in both layouts.
- Enums use project-declared names. A developer must add new names and regenerate first.
- Polymorphic `$type` uses project-provided concrete kinds; use a full name if short names collide. Leave columns belonging only to other subclasses blank.
- Blank optional members may retain defaults. Required members must be filled explicitly even when a default exists. 0 and false are values.
- Nullable values allow null. Other members follow the template's emptiness rules; adding a question mark to arbitrary objects or collections does not define a new type.
- Unknown members, duplicate JSON members, wrong kinds, and invalid numbers block export. Discuss new members with a developer; do not edit generated table code.

Follow project examples for special formats, such as `1.25` in a Money cell. Rules check business constraints such as allowed ranges. Converter parsing and serialization are developer details; see [converter development](project-development.en.md#11-extend-special-formats-with-converters).

## Valid syntax still needs business validation

Parsable syntax does not guarantee valid game rules. Projects supply required, range, combination, and cross-table constraints, automatically run by “Validate and export”. See [error handling](user-guide.en.md#6-validation-and-post-validation), [rule development](rules-development.en.md), and [breakpoint debugging](debugging.en.md).
