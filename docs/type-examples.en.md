# An all-types workbook

[中文](type-examples.md) | **English**

Data, Config, Rules, and Output paths in this document are relative to the `samples/` sample directory.

Primitive values, complex objects, and collections are demonstrated on the first `AllTypes` sheet of [Data/AllTypes.xlsx](../samples/Data/AllTypes.xlsx). Structures, polymorphism, and multi-row actions share this sheet. CompositeKeys.xlsx demonstrates two- and three-component keys, and TableReferences.xlsx demonstrates direct table-object references. “类型索引” (Type index) and “填写说明” (Input instructions) are navigation sheets.

AllTypes uses five header rows: field, type, key, description, and target side. Id is marked with the literal `主键` on row 3; data starts on row 6. The three records begin on rows 6, 8, and 11, with continuation rows 7, 9, and 10. Index links, frozen rows, action dropdowns, and instructions match this layout. [Config/Tables.json](../unity/ZrConfig.Sample/Assets/HotUpdate/Config/Tables.json) declares only the custom ByQuality group.

## Primitive values appear first

The leftmost columns A–I are:

| Field | Id | ByteValue | ShortValue | IntValue | LongValue | FloatValue | DoubleValue | BoolValue | StringValue |
|---|---|---|---|---|---|---|---|---|---|
| Type | int | byte | short | int | long | float | double | bool | string |
| Typical example | 1 | 12 | -12 | -123 | 9007199254740993 | 1.25 | 3.141592653589793 | 1 | Preserve leading/trailing spaces |

Columns J–N show sbyte, ushort, uint, ulong, and decimal. They are followed by other scalars, one-/two-/three-level arrays, split structures and polymorphism, multi-row lists, and dedicated JSON types/input examples. Numbers are stored as text to preserve long, ulong, and decimal precision.

## Type coverage

| Type category | Examples in this table |
|---|---|
| byte、sbyte、short、ushort、int、uint、long、ulong、float、double、decimal、bool、string | Value / Array / Array2D / Array3D for each |
| Enum | QualityValue and arrays |
| vector2、vector3 | Vector2Value, Vector3Value, and arrays |
| kv | KvValue and arrays, preserving duplicate keys |
| Custom struct | Range.Min / Max; multi-row Ranges[] |
| Custom class and nested objects | Options.Label / Range.Min, etc.; multi-row OptionsList[] |
| Polymorphic objects | Action.$type; multi-row Actions[].$type |
| Custom converter | MoneyValue and arrays; currency text converted to integer cents |
| Raw JSON type | RawJsonValue / Array / Array2D / Array3D |
| Nullable values | OptionalInt / UInt / ULong / Decimal / Bool / Quality / Money / Range; OptionalValues |
| Dictionaries | StatMap, PriceMap, QualityMap, ListMap |
| Rectangular arrays | IntMatrix, DecimalCube, OptionalMatrix |
| Nested multi-row lists | Groups[].Actions[], Phases[].Groups[].Actions[] |
| json<T> input format | JsonIntArray, parsed as int[] |

The table covers 13 primitive types plus extended types, collections, nullable values, and JSON input. Custom types remain extensible. `json<T>` is an input format, not a runtime wrapper. Structures and battle parameters use member columns; explicit RawJson examples contain JSON text.

Primitives, enums, vectors, kv, Money, and RawJson show scalars and one to three array levels. Classes, structs, and polymorphic types show split scalars and one-dimensional multi-row arrays. Automated tests also cover two-/three-level single-cell object combinations.

## Multiple rows in one table

The table has 103 logical fields and 144 Excel columns; 3 records span 6 rows. Ids 1, 2, and 3 occupy 2, 3, and 1 rows respectively. Parent fields such as primitive values appear only on each record's first row. Repeated Ids explicitly identify ownership.

Click Actions in “类型索引” (Type index) to see these actions (only selected columns are shown):

| Id | Actions[].$order | Actions[].$type | Actions[].Ratio | Actions[].BuffId |
|---|---|---|---|---|
| 1 | 1 | Damage | 1.5 | |
| 1 | 2 | AddBuff | | 1001 |
| 2 | 1 | Damage | 0 | |
| 2 | 2 | Damage | 0.8 | |
| 2 | 3 | AddBuff | | 0 |
| 3 | | | | |

Runtime Actions lengths are 2, 3, and 0. Ranges[] and OptionsList[] show ordinary struct/class multi-row lists; each list is sorted and collected independently.

Choose Damage / AddBuff from the polymorphic dropdown. Fill only the selected subclass's parameters, leaving other subclass columns blank. BuffId is required; explicit 0 is valid. A completely blank multi-row group means an empty array. Do not merge data cells or sort only some columns.

## Input examples for additional types

| Field | Type | Example |
|---|---|---|
| UIntValue | uint | `4294967295` |
| ULongValue | ulong | `18446744073709551615` (text) |
| DecimalValue | decimal | `1.2345678901234567890123456789` (text) |
| OptionalInt | int? | `123` / `null` / blank |
| OptionalValues | int?[] | `[1;null;0]` |
| StatMap | `Dictionary<string,int>` | `["hp"=100;"attack"=25]` |
| PriceMap | `Dictionary<int,decimal?>` | `[1001=1.25;1002=null]` |
| IntMatrix | int[,] | `[[1;2];[3;4]]` |
| DecimalCube | decimal[,,] | `[[[1.25;2.5]];[[3.75;4.5]]]` |

**Groups** in the type index opens a two-level list, and **Phases** a three-level list. For Id=2, parent orders 2, 1, 2 intentionally demonstrate automatic sorting after grouping; repeating a parent's `$order` continues its child actions. Child lists sort independently. Parent parameters on continuations are blank or exact repeats. Structure/action members remain split into columns.

## Use and validate

Select these four App directories in order: samples/Data, samples/Output, unity/ZrConfig.Sample/Assets/HotUpdate/Config, and unity/ZrConfig.Sample/Assets/Editor/ConfigRules. All samples share them. Follow the [user guide](user-guide.en.md#3-first-export) to generate/export; no types or serialization code need to be written.

Existing post-validation checks maximum target count, interval ordering, action delay, damage ratio, and buff stacks, locating errors at individual members/action rows. See [type syntax](config-types.en.md) for input rules and consult project developers about business meaning.

Complete workbooks are included; follow the [sample instructions](../samples/README.en.md).
