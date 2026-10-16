# Excel 类型与填写语法

**Excel types and input syntax**

多语言服务端生成与 Binary / JSON / 双格式导出见[多语言生成与 JSON](server-generation.md)。

See [multi-language generation and JSON](server-generation.md) for server generation and Binary / JSON / dual-format export.
本文说明已有字段如何填写。日常操作先看 [使用文档](user-guide.md)，实际工作簿位置见 [全类型表说明](type-examples.md)。

This document explains how to fill existing fields. Start with the [user guide](user-guide.md) for everyday operations and the [all-types workbook](type-examples.md) for actual examples.

项目使用自定义枚举、类、动作种类或金额格式时，程序员会提供相应模板与数据定义。配表人员按给定的类型名、成员和示例填写，不需要先编写类型或转换器。新增数据形状、特殊输入格式和通用读写支持维护见 [项目开发教程](project-development.md)。

Developers provide templates and definitions for custom enums, classes, action kinds, and currency formats. Authors use those type names, members, and examples without writing types or converters first. See [project development](project-development.md) for new data shapes, special formats, and shared serialization support.

## 先选配表结构

**Choose a table structure**

同一张表可以组合基础字段、单格集合、分列对象和多行列表。先按数据含义选择布局，再查下面的具体类型语法。

A table can combine primitive fields, single-cell collections, objects split across columns, and multi-row lists. Choose a layout that matches the data, then look up the detailed syntax below.

| 想表达的数据<br>Data to represent | Excel 布局<br>Excel layout | 导入后的结构<br>Imported structure |
| --- | --- | --- |
| 一条道具或等级配置<br>An item or level configuration | 一行一条，字段各占一列<br>One record per row, one column per field | 一条强类型配置记录<br>A strongly typed record |
| 一组数值、字典或矩阵<br>Values, a dictionary, or a matrix | 在单元格填写数组、字典或 `json<T>`<br>An array, dictionary, or `json<T>` in a cell | 数组、字典或对应对象<br>The corresponding array, dictionary, or object |
| 多个设置项组成一个对象<br>Settings that form one object | `Settings.MaxTargets` 等点号成员列<br>Dotted member columns such as `Settings.MaxTargets` | 一个 Settings 对象<br>One Settings object |
| 同一字段可选不同动作<br>A field with alternative action kinds | `Action.$type` 与各子类成员列<br>`Action.$type` and subclass member columns | 选定子类的对象<br>An instance of the selected subclass |
| 一条配置包含多个动作<br>Multiple actions in one record | `Actions[].成员` 跨多行填写，按完整主键合并<br>`Actions[].Member` over several rows, merged by the complete primary key | 一个对象数组<br>An object array |
| 阶段内含动作组、组内含动作<br>Phases containing groups containing actions | `Phases[].Groups[].Actions[].成员`，父级 `$order` 明确归属<br>`Phases[].Groups[].Actions[].Member`; parent `$order` identifies ownership | 嵌套对象数组，各层独立排序<br>Nested object arrays, sorted independently at each level |
| 多个业务字段共同标识一条记录<br>Several fields jointly identify a record | 第 3 行在多个字段标记“主键”<br>Mark multiple fields with the literal `主键` on row 3 | 联合主键与各主键字段的分组索引<br>A composite primary key and a group index per key field |
| 指向另一张表的一条记录<br>A record in another table | 顶层字段类型为目标 `XxxConfig`，格子填写目标 Id<br>A top-level field of target type `XxxConfig`, containing its Id | 直接关联的配置对象<br>A directly referenced configuration object |

分列、多态和多行填写详见 [结构语法](#结构分列多态类型列与多行动作列表)；主键及引用限制见 [主键文档](table-indexes.md)。多行归组、排序和对象关联完成后，项目还可对这些结构执行 [后置检查](rules-development.md#2-后置指解析后发布前)。

See [structure syntax](#结构分列多态类型列与多行动作列表) for split columns, polymorphism, and multi-row input, and [primary keys](table-indexes.md) for key/reference restrictions. After grouping, sorting, and linking objects, projects can run [post-validation](rules-development.md#2-后置指解析后发布前) on these structures.

## 客户端与服务端字段

**Client and server fields**

所有表统一使用五行表头：第 1 行字段名、第 2 行类型、第 3 行主键、第 4 行说明、**第 5 行所属端，第 6 行起数据**。**第 5 行不能省略，每个字段必须明确声明 `C`、`S` 或 `CS`**；缺失标记会阻止生成和导出。无需在 `Config/Tables.json` 中启用布局。

Every table uses five header rows: field names, types, primary keys, descriptions, and **target side on row 5; data starts on row 6**. **Row 5 is mandatory and every field must explicitly declare `C`, `S`, or `CS`**. Missing markers block generation and export. No layout switch in `Config/Tables.json` is needed.

| Excel 行<br>Excel row | Id | Icon | DropWeight | Name |
| --- | --- | --- | --- | --- |
| 1：字段名<br>1: Field | Id | Icon | DropWeight | Name |
| 2：类型<br>2: Type | int | string | int | string |
| 3：主键<br>3: Primary key | 主键<br>`主键` (literal marker) |  |  |  |
| 4：说明<br>4: Description | 编号<br>Identifier | 图标<br>Icon | 服务端掉落权重<br>Server drop weight | 名称<br>Name |
| 5：所属端<br>5: Target side | CS | C | S | CS |
| 6：数据<br>6: Data | 1001 | item_1001 | 50 | 小药水<br>Small potion |

- `C`：只进入客户端生成代码与二进制数据。<br>
  `C`: Included only in client generated code and binary data.
- `S`：只进入服务端生成代码与二进制数据。<br>
  `S`: Included only in server generated code and binary data.
- `CS`：两端都包含。标记忽略大小写和首尾空格；`SC` 等其他值报错。<br>
  `CS`: Included on both sides. Markers ignore case and surrounding whitespace; other values such as `SC` are errors.

在 App 的“生成 / 导出端”选择 C 或 S，再生成、校验并导出；项目配置会保存这个选择，旧配置默认 C。修改字段标记或切换目标端后，需要生成与该端匹配的代码。对应命令见 [按端生成和导出](project-development.md#按端生成和导出)。

Select C or S in the App's “生成 / 导出端” (Generation / export target), then generate, validate, and export. Project settings retain this choice; older settings default to C. After changing markers or target side, regenerate matching code. See [generation and export by side](project-development.md#按端生成和导出).

主键必须明确填写 `CS`。普通字段留空报错，不设置默认所属端。分列对象、多态对象、多行数组按**顶层字段整体**选择，例如 `Settings.X` 与 `Settings.Y` 必须使用一致标记；只在同组一列填 C、其余留空时，空白列继承已填写的 C；整组留空则报错。整组属于 C，不会按成员拆开手写类型。即使某端排除了全部多行列表字段，记录仍按原表的主键归组。另一端字段的数据值不参与本端解析、引用检查和写入；表头、类型与标记仍须合法。项目业务规则使用当前端的模型，不能访问本端已排除的字段。

Primary keys must explicitly use `CS`. Blank ordinary fields are errors; there is no default side. Split objects, polymorphic objects, and multi-row arrays are selected **as a whole top-level field**: `Settings.X` and `Settings.Y` must agree. If only one column in a group declares C, blank columns inherit C; an entirely blank group is invalid. The whole group belongs to C; handwritten types are not split by member. Records still group by the original primary key even if a side excludes all multi-row fields. Values excluded from a side are not parsed, reference-checked, or written for that side, but headers, types, and markers must remain valid. Business rules use the current side's model and cannot access excluded fields.

已有 `headerRows: 5` 声明可以保留或删除；`headerRows: 4` 会报错。旧四行表须先在第 5 行插入所属端行，将原数据整体下移，并明确填写 `C`、`S` 或 `CS`，再生成代码并导出。生成和导出不会自动插行或推断布局；不要将第一条数据留在所属端行。

An existing `headerRows: 5` declaration may remain or be removed; `headerRows: 4` is rejected. For old four-row workbooks, insert a target-side row at row 5, move all data down, and explicitly fill `C`, `S`, or `CS` before regenerating and exporting. Generation/export neither inserts rows nor infers layouts. Do not leave the first data record in the target-side row.

## 类型与填写

**Types and cell values**

类型名区分大小写（内置数值关键字兼容大小写）。自定义类型使用短名或完整命名空间；同名时必须写全名。新建表使用第 1 行字段名、第 2 行类型、第 3 行主键、第 4 行说明、第 5 行所属端，第 6 行起填数据。可以用业务字段组成主键，不要求额外增加 `int Id`。详见 [Excel 主键与分组](table-indexes.md)。`__zr` 开头的字段名保留给生成器。

Type names are case-sensitive, except built-in numeric keywords. Custom types accept short or fully qualified names; name collisions require the latter. Use field/type/key/description/side on rows 1–5 and data from row 6. Business fields may form the key; no extra `int Id` is required. See [Excel keys and groups](table-indexes.md). Field names starting with `__zr` are reserved.

AllTypes 示例使用相同的五行表头，数据从第 6 行开始；单元格类型语法与本文相同，具体布局见 [全类型表说明](type-examples.md)。

AllTypes uses the same five-row header, with data from row 6 and the same cell syntax. See the [all-types workbook](type-examples.md) for its layout.

| Excel 第二行类型<br>Type on Excel row 2 | 单元格示例<br>Cell example | 生成类型<br>Generated type |
|---|---|---|
| byte / short / int / long | 255 / -100 / 3001 / 9223372036854775807 | 对应 C# 类型<br>Corresponding C# type |
| sbyte / ushort / uint / ulong | -128 / 65535 / 4294967295 / 18446744073709551615 | 对应 C# 类型<br>Corresponding C# type |
| decimal | `1.2345678901234567890123456789` | 精确十进制，文本存储<br>Exact decimal, stored as text |
| float / double | 1.25 | 对应 C# 类型，仅有限值<br>Corresponding C# type; finite values only |
| bool | true / false / 1 / 0 | bool |
| string | 普通文本，或 `"含;分隔符"`<br>Plain text, or quoted text such as `"with;delimiter"` | string |
| SkillConfig | `2001`（Skill 表的 Id）<br>`2001` (Id in Skill) | SkillConfig?，直接引用目标行对象<br>SkillConfig?, directly referencing the target row object |
| Quality | Rare | 自定义枚举 Quality<br>Custom enum Quality |
| vector2 | 1.5,2 | ZrConfig.Types.Vector2（X、Y） |
| vector3 | 1,2,3 | ZrConfig.Types.Vector3（X、Y、Z） |
| kv | 3001,10 | ZrConfig.Types.Kv（int Key、int Value） |
| int[] | `[1;2;3]` 或 `1;2;3`<br>`[1;2;3]` or `1;2;3` | int[] |
| kv[] | `[3001,10;3001,20]` | Kv[]，保留顺序和重复 Key<br>Kv[], preserving order and duplicate keys |
| Quality[] | `[Rare;Legendary]` | Quality[] |
| vector2[] | `[1,2;3,4]` | Vector2[] |
| int[][] | `[[1;2];[];[3]]` | 交错数组，子数组可不等长<br>Jagged array; inner lengths may differ |
| int[][][] | `[[[1;2];[3]];[[4]]]` | 三层交错数组<br>Three-level jagged array |
| int? / Quality? | `123` / `Rare` / `null` / 留空<br>`123` / `Rare` / `null` / blank | Nullable 值类型<br>Nullable value type |
| int?[] | `[1;null;0]` | 可空元素数组<br>Array of nullable elements |
| `Dictionary<string,int>` | `["hp"=100;"attack"=25]` | 强类型字典<br>Strongly typed dictionary |
| `Dictionary<int,decimal?>` | `[1001=1.25;1002=null]` | 可空值字典<br>Dictionary with nullable values |
| int[,] | `[[1;2];[3;4]]` | 二维矩形数组<br>Two-dimensional rectangular array |
| decimal[,,] | `[[[1.25;2.5]];[[3.75;4.5]]]` | 三维矩形数组<br>Three-dimensional rectangular array |
| Settings | `{"Limit":7}` | 手写 Settings 类型<br>Handwritten Settings type |
| `json<Settings>` | `{"Limit":7}` | Settings，JSON 输入<br>Settings with JSON input |
| `json<int[][]>` | `[[1,2],[],[3]]` | int[][]，标准 JSON 分隔符<br>int[][] with standard JSON separators |
| `json<BattleAction[]>` | `[{"$type":"Damage","Ratio":1.5}]` | BattleAction[] |
| json | `{"custom":[1,true]}` | RawJson，Text 保存原始 JSON 文本<br>RawJson; Text preserves the original JSON text |

除直接表对象引用等有明确限制的类型外，支持的值类型和对象类型可以追加 `[]`、`[][]`、`[][][]`。直接表对象引用目前只支持顶层单值字段，不支持数组、字典值或对象内部引用。`json<T>` 是单元格输入格式，不增加运行时包装对象；未使用分列路径的自定义对象单格仍按 JSON 解析；分列与多行对象直接从各单元格构造。JSON 中的 vector/kv 使用数值数组，如 `json<vector2[]>` 填 `[[1,2],[3,4]]`。

Supported value/object types can take `[]`, `[][]`, or `[][][]`, except explicitly restricted types such as direct table references. Direct table references currently work only as top-level scalar fields, not array elements, dictionary values, or nested members. `json<T>` is a cell input format, with no runtime wrapper. Custom objects without split member paths still parse from single-cell JSON; split and multi-row objects are built from individual cells. Vectors/kv use numeric arrays in JSON, e.g. `[[1,2],[3,4]]` for `json<vector2[]>`.

数组为空写 `[]`，空白数组单元格也得到空数组。每层嵌套数组都要加括号；`[1;;2]` 不合法。字符串包含 `;`、`,`、括号或引号时使用 JSON 字符串转义：`["a;b";"x,[]";"quote\""]`。一维字符串数组中的空字符串写 `""`。普通未加引号的 string 单元格保留原空格；全空白单元格按空字符串处理。

Write `[]` for an empty array; a blank array cell also yields an empty array. Bracket every nested level; `[1;;2]` is invalid. Use JSON string escaping for `;`, `,`, brackets, or quotes: `["a;b";"x,[]";"quote\""]`. An empty string element in a one-dimensional string array is `""`. Plain unquoted string cells retain spaces; entirely blank cells become empty strings.

枚举填写成员名称，不接受任意数字或未声明的名称；空枚举单元格仅在声明了值为 0 的成员时可用。long、ulong 和高精度 decimal 必须按文本输入，避免 Excel 在导出前就将有效位舍入。浮点数采用固定文化解析，小数点为 `.`。

Enums require declared member names, not arbitrary numbers or unknown names. A blank enum cell is valid only if a member with value 0 exists. Enter long, ulong, and high-precision decimal values as text so Excel does not round significant digits before export. Floating-point parsing uses invariant culture and `.` as decimal separator.

向量是纯 C# 数据结构，不依赖 UnityEngine；游戏侧按需使用 `new UnityEngine.Vector2(value.X, value.Y)`。kv 只是两个整数，是否代表物品与数量由业务规则决定，不是字典。

Vectors are pure C# structures without UnityEngine dependencies; convert as needed with `new UnityEngine.Vector2(value.X, value.Y)`. A kv is two integers, not a dictionary. Business rules determine whether they mean item and quantity.

## 结构分列、多态类型列与多行动作列表

**Split objects, polymorphic type columns, and multi-row action lists**

结构分列和多行列表同样固定使用上述五行表头；多行列表按完整主键归组。第 3 行只在主键字段填写“主键”，至少标记一列；对象成员路径、动作顺序列和集合列不标主键。基础值、复杂对象与集合的实际内容可在 [AllTypes.xlsx](../samples/Data/AllTypes.xlsx) 第一张工作表查看，通过类型索引跳到 Actions 多行动作、Range 结构分列等位置。后续工作页只提供索引和说明。

Split objects and multi-row lists use the same five-row header. Lists group by the complete primary key. Put the literal `主键` only in key columns on row 3, marking at least one; do not mark member paths, action-order columns, or collections. Inspect actual values, objects, and collections on the first sheet of [AllTypes.xlsx](../samples/Data/AllTypes.xlsx), using the type index to navigate to Actions or Range. Later sheets provide only indexes and instructions.

**结构分列**使用点号路径，例如 `Settings.MaxTargets`、`Settings.Interruptible`；嵌套对象可以继续写 `Settings.Window.Min`。第二行声明的是该组的**根类型**（例如 `Game.Config.BattleSettings`），只需在组内一列填写，其余类型格留空；也允许每列重复相同根类型。成员类型直接从 C# 定义读取，不能在相邻成员列混填 `int`、`bool`。可合并组内第二行的类型格，第一行字段路径不能合并。

**Split objects** use dotted paths such as `Settings.MaxTargets`, `Settings.Interruptible`, and nested `Settings.Window.Min`. Row 2 declares the group's **root type**, e.g. `Game.Config.BattleSettings`. Fill one type cell in the group and leave the rest blank, or repeat the same root type in every column. Member types come from C# definitions; do not mix `int` and `bool` in neighboring member columns. Row-2 type cells may be merged within a group; row-1 field paths must not be merged.

| 行<br>Row | Id | Settings.MaxTargets | Settings.Interruptible |
|---|---|---|---|
| 字段（1）<br>Field (1) | Id | Settings.MaxTargets | Settings.Interruptible |
| 类型（2）<br>Type (2) | int | Game.Config.BattleSettings |  |
| 主键（3）<br>Primary key (3) | 主键<br>`主键` (literal marker) |  |  |
| 说明（4）<br>Description (4) | 编号<br>Identifier | 最大目标数<br>Maximum targets | 能否打断<br>Interruptible |
| 5：所属端<br>5: Target side | CS | CS | CS |
| 数据（6）<br>Data (6) | 1001 | 3 | false |

**单个多态对象**使用 `Action.$type`、`Action.Delay`、`Action.Ratio`、`Action.BuffId` 等列，第二行根类型为 `Game.Config.BattleAction`。`$type` 填写具体子类短名或完整名称；只填写该子类拥有的字段，不适用列必须留空。自定义 C# 类型的业务字段 `Type` 与保留列 `$type` 不同。

**A single polymorphic object** uses columns such as `Action.$type`, `Action.Delay`, `Action.Ratio`, and `Action.BuffId`, with root type `Game.Config.BattleAction` on row 2. `$type` contains the concrete subclass's short or full name. Fill only that subclass's fields; leave inapplicable columns blank. A business field named `Type` is distinct from reserved `$type`.

**多行数组**在根字段后写 `[]`：`Actions[].$type`、`Actions[].Delay` 等，类型行声明 `Game.Config.BattleAction[]`。同一完整主键的所有行合并成一条配置记录，每个非空动作组构造一个数组元素。也支持普通非多态对象数组；只有多态对象需要 `$type`。

**Multi-row arrays** add `[]` after the root field, e.g. `Actions[].$type` and `Actions[].Delay`, with `Game.Config.BattleAction[]` in the type row. Rows sharing a complete key merge into one record; each nonempty action group creates one array element. Ordinary non-polymorphic object arrays are also supported; only polymorphic objects need `$type`.

下面以 Id 为单列主键，仅列字段名和数据。实际工作簿中第 2 行仍填写类型，第 3 行只在 Id 列填写“主键”，第 4 行填写说明，第 5 行保留所属端，以下数据从第 6 行开始：

The following shows only field names and data, using Id as the sole primary key. In a real workbook, retain types on row 2, `主键` only under Id on row 3, descriptions on row 4, target sides on row 5, and start these values on row 6:

| Id | Name | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
|---|---|---|---|---|---|---|---|---|
| 1001 | 斩击<br>Slash | 1 | Damage | 0.2 | 1.5 | true |  |  |
| | | 2 | AddBuff | 0.5 | | | 2001 | 3 |
| 1002 | 火球<br>Fireball | 1 | Damage | 0 | 2.0 | true |  |  |

- 只有包含 `[]` 展开列的工作表才会按完整主键合并；普通表、仅有结构分列的表仍是一行一条记录。<br>
  Only sheets containing expanded `[]` columns merge rows by complete key. Ordinary tables and tables with only split objects still have one record per row.
- 首次出现的记录必须填写所有键分量；续行可以重复完整键，包括不连续的行。全部键留空表示续接上一条有效主记录，不能只填部分键；无效键后的空键不会误接到更早的记录。上例只有 Id 一个键分量。<br>
  The first occurrence of a record must fill every key component. Continuation rows may repeat the complete key, even non-contiguously. All key cells blank continues the preceding valid main record; partially filled keys are invalid. Blank keys after an invalid key do not attach to an earlier record. This example has only Id.
- 每条记录的 Name、Settings 等主字段由该完整键第一次出现的行定义；后续行留空，或逐字重复首行内容。存在冲突就阻止导出。<br>
  Main fields such as Name and Settings come from the first row for that complete key. Later rows must leave them blank or repeat the first value exactly. Conflicts block export.
- `$order` 是输入辅助列，不生成到模型。没有子列表时可省略，按物理行顺序排列；声明后每个非空元素必须填写唯一正整数。包含子列表时，父级必须声明 `$order`，续行重复父级编号以归组。每层在当前父元素内独立排序，不要求连续。<br>
  `$order` is an input helper, not a generated member. Without child lists it is optional and physical row order applies; when declared, each nonempty element needs a unique positive integer. Parents with child lists must declare `$order`; repeat the parent number on continuations. Each level sorts independently within its current parent; numbers need not be consecutive.
- 数组的全部展开列留空表示不增加元素；整条记录没有元素时得到空数组。仅填写 order 或参数却遗漏多态类型会报错。<br>
  Leaving all expanded array columns blank adds no element; a record with none gets an empty array. Filling only order or parameters while omitting a required polymorphic type is an error.
- 对象成员留空保留 项目定义的默认值。`ConfigRequired` 成员必须显式填写，`0`、`false` 有效。未初始化的 string、数组、字典分别补为空字符串、空数组、空字典；可空值的显式 `null` 也会触发必填检查。<br>
  Blank object members retain project defaults. `ConfigRequired` members must be explicit; `0` and `false` count. Uninitialized strings, arrays, and dictionaries become empty values. Explicit `null` for a nullable member also triggers required-value validation.
- 支持嵌套多行对象数组，例如 `Groups[].Actions[].Ratio`、`Phases[].Groups[].Actions[].BuffId`；也支持普通对象内的列表 `Plan.Groups[].Name`。普通数组叶子仍可使用单格格式。<br>
  Nested multi-row object arrays are supported, e.g. `Groups[].Actions[].Ratio` and `Phases[].Groups[].Actions[].BuffId`, as are lists inside ordinary objects such as `Plan.Groups[].Name`. Ordinary leaf arrays can still use single-cell input.
- 同一字段不能同时声明 `Actions` 单格和 `Actions[].X` 展开列，也不能同时声明 `Settings` 单格和其成员列。分列根类型不加 `json<T>`。<br>
  Do not declare both a single-cell `Actions` and expanded `Actions[].X`, or both a single-cell `Settings` and its member columns. Do not wrap split root types in `json<T>`.

分列和多行只是填写方式，不会把点号变成新的普通字段。错误仍定位到实际工作表行列；例如 Actions[1].BuffId 表示排序后第二个动作的 BuffId，按报告里的 Excel 行列修改。

Splitting columns and rows is only an input layout; dotted paths do not become new ordinary fields. Errors retain actual sheet coordinates. For example, Actions[1].BuffId means BuffId of the second action after sorting; edit the Excel row/column given in the report.

已有项目改变填写布局或升级工具前，先由程序员确认代码与数据兼容；具体结构与二进制约定见 [开发教程](project-development.md#18-类型实现与兼容约束)。

Before changing an existing project's layout or upgrading, have a developer check code/data compatibility. See [type implementation and binary constraints](project-development.md#18-类型实现与兼容约束).

## 嵌套多行列表

**Nested multi-row lists**

`AllTypes` 中的 `Groups` 根类型为 `Game.Config.ActionGroup[]`。下面仅列部分字段名和数据内容；创建同类表时第 2 行填写类型，第 3 行在 Id 列填写“主键”，第 4 行填写说明，第 5 行保留所属端，第 6 行起填写数据：

In `AllTypes`, `Groups` has root type `Game.Config.ActionGroup[]`. Only selected fields and data are shown below. For a similar table, put types on row 2, `主键` under Id on row 3, descriptions on row 4, sides on row 5, and data from row 6:

| Id | Groups[].$order | Groups[].Name | Groups[].Actions[].$order | Groups[].Actions[].$type | Groups[].Actions[].Ratio | Groups[].Actions[].BuffId |
|---|---|---|---|---|---|---|
| 2 | 2 | 后手<br>Follow-up | 1 | Damage | 0 |  |
| 2 | 1 | 先手<br>Opening | 1 | Damage | 0.8 |  |
| 2 | 2 | | 2 | AddBuff | | 0 |

输出包含两个组：`Groups[0]` 为“先手”，`Groups[1]` 为“后手”，后手含两个动作。重复父级 order 表示继续填它的子列表；父级名称、类型等参数只在该组首行填写，后续留空或原样重复，冲突会报错。子级 order 只需在当前父组内唯一。所有父级编号必须显式填写，不能用空白推断嵌套归属。子列表整组留空得到空数组。

The result has two groups: `Groups[0]` is “先手” (Opening), `Groups[1]` is “后手” (Follow-up), with two actions in the latter. Repeating parent order continues its child list. Fill parent name, type, and other parameters only on that group's first row; later rows may be blank or identical, otherwise an error is raised. Child order need only be unique within its parent. Every parent number must be explicit; blank cells never infer nested ownership. An entirely blank child list becomes an empty array.

`Phases[].Groups[].Actions[]` 演示三层；相同规则递归应用，整体深度上限为 64。多行展开节点必须是一维对象数组，`int[,]` 等值数组使用单格填写。后置检查可使用 `Groups[1].Actions[0].Ratio`，仍定位到排序前的 Excel 原始单元格。

`Phases[].Groups[].Actions[]` demonstrates three levels. Rules apply recursively, up to an overall depth of 64. Expanded multi-row nodes must be one-dimensional object arrays; value arrays such as `int[,]` use single-cell input. Post-validation may address `Groups[1].Actions[0].Ratio` and still locate the original unsorted Excel cell.

## 可空值、字典与矩形数组

**Nullable values, dictionaries, and rectangular arrays**

- **可空 `T?`**：支持数值、bool、枚举、向量、kv、自定义 struct 和已提供的特殊值类型。留空或 `null` 表示无值；`0`、`false` 是实际值。`ConfigRange?` 可分列，整组空白得到 null。对象内未填写成员仍保留项目默认值。`string?`、数组 `?` 等引用类型可空注解不属于此输入语法。<br>
  **Nullable `T?`:** Supports numbers, bool, enums, vectors, kv, custom structs, and provided special value types. Blank or `null` means absent; `0` and `false` are values. `ConfigRange?` can be split across columns; an entirely blank group yields null. Unfilled object members retain defaults. Reference-type annotations such as `string?` or array `?` are not part of this input syntax.
- **字典**：语法为 `Dictionary<TKey,TValue>`（也接受 `dictionary`）。单格用 `[键=值;键=值]`，空白或 `[]` 表示空字典。字符串键包含分号、等号、括号等字符时用引号包围，空字符串键写 `""`。嵌套例：`Dictionary<int,Dictionary<string,int[]>>` 填 `[1=["a"=[1;2]];2=[]]`。值可使用已支持的任意类型，对象值的单格输入仍遵循对象语法；战斗对象建议继续分列、多行填写。<br>
  **Dictionaries:** Use `Dictionary<TKey,TValue>` (also accepts `dictionary`). A cell contains `[key=value;key=value]`; blank or `[]` is empty. Quote string keys containing semicolons, equals signs, brackets, etc.; `""` is an empty string key. Nested example: `Dictionary<int,Dictionary<string,int[]>>` with `[1=["a"=[1;2]];2=[]]`. Values may use supported types; object values still follow object cell syntax. Prefer split columns and multi-row input for battle objects.
- **字典键**：支持整数、bool、string、decimal、枚举；拒绝可空、float/double、对象和集合键。按解析后的键检查重复，例如 int 键 `1` 与 `01` 冲突。字典不用于保存填写顺序；需要有顺序的一组数据时使用数组或列表布局。<br>
  **Dictionary keys:** Integers, bool, string, decimal, and enums are supported; nullable, float/double, object, and collection keys are rejected. Duplicate checks use parsed keys, so integer `1` conflicts with `01`. Dictionaries do not preserve authoring order; use arrays or list layouts when order matters.
- **矩形数组**：`T[,]`、`T[,,]` 每一维都用方括号与分号表示，所有同层长度一致；`[[1;2];[3]]` 必须改用交错数组 `T[][]`。空白或 `[]` 是各维长度为 0 的矩阵，`[[];[]]` 是 2×0。二进制保存各维长度，限制总元素数，拒绝不规则形状、负维度、超大维度和截断数据。<br>
  **Rectangular arrays:** Each dimension of `T[,]` and `T[,,]` uses brackets and semicolons; sibling lengths must match. `[[1;2];[3]]` requires jagged `T[][]`. Blank or `[]` gives zero length in every dimension; `[[];[]]` is 2×0. Binary data stores dimension lengths, caps element counts, and rejects irregular shapes, negative or excessive dimensions, and truncation.
- **组合**：支持 `int?[]`、`uint?[,]`、字典数组以及最多三层数组嵌套，每层秩不超过 3。混合后缀遵循 C#：`int[][,]` 是“矩阵的数组”，`int[,][]` 是“元素为数组的矩阵”。`json<Dictionary<int,int?>>`、`json<uint?[,]>` 也可使用标准 JSON 输入。<br>
  **Combinations:** Supports `int?[]`, `uint?[,]`, dictionary arrays, and up to three nested array levels with rank at most 3 each. Mixed suffixes follow C#: `int[][,]` is an array of matrices; `int[,][]` is a matrix of arrays. `json<Dictionary<int,int?>>` and `json<uint?[,]>` also accept standard JSON.

旧项目首次增加矩形数组时，由程序员确认配套代码已支持该类型；遇到读写方法缺失等编译错误，交由维护者按 [升级约定](project-development.md#18-类型实现与兼容约束) 处理。

When first adding rectangular arrays to an older project, have a developer confirm support code compatibility. Missing read/write methods or similar compile errors require the [upgrade procedure](project-development.md#18-类型实现与兼容约束).

## 项目提供的复杂类型

**Project-provided complex types**

- 普通对象使用模板给定的成员。单格对象按 JSON 写，分列对象按各列填写，不在两种布局中重复声明同一字段。<br>
  Ordinary objects use the template's members. Enter JSON for single-cell objects or values in the appropriate split columns; do not declare the same field in both layouts.
- 枚举填写项目声明的名称；新增名称需程序员扩展后重新生成。<br>
  Enums use project-declared names. A developer must add new names and regenerate first.
- 多态对象的 `$type` 使用项目提供的具体种类，短名有冲突时用完整名称；不同子类不适用的列留空。<br>
  Polymorphic `$type` uses project-provided concrete kinds; use a full name if short names collide. Leave columns belonging only to other subclasses blank.
- 可选成员留空可能保留项目默认值；被标记为必须填写的成员即使有默认值也需要显式填写。0、false 是实际值。<br>
  Blank optional members may retain defaults. Required members must be filled explicitly even when a default exists. 0 and false are values.
- 可空值允许 null；其他成员能否为空按模板约定，不能把任意对象或集合都加问号当作新类型。<br>
  Nullable values allow null. Other members follow the template's emptiness rules; adding a question mark to arbitrary objects or collections does not define a new type.
- 未知成员、重复 JSON 成员、错误种类或错误数值会阻止导出。新增成员先与程序员确认，不用修改自动生成的表代码。<br>
  Unknown members, duplicate JSON members, wrong kinds, and invalid numbers block export. Discuss new members with a developer; do not edit generated table code.

特殊格式按项目示例填写，例如 Money 格子中的 `1.25`。金额允许范围等业务条件由规则检查；转换器如何解析和读写是程序员的实现细节，见 [转换器开发](project-development.md#11-特殊格式怎么扩展转换器)。

Follow project examples for special formats, such as `1.25` in a Money cell. Rules check business constraints such as allowed ranges. Converter parsing and serialization are developer details; see [converter development](project-development.md#11-特殊格式怎么扩展转换器).

## 填写正确后仍需业务校验

**Valid syntax still needs business validation**

语法能解析不代表符合游戏规则。必填、范围、组合和跨表条件由项目提供，点击“校验并导出”自动执行；报错处理见 [使用文档](user-guide.md#6-怎样理解校验与后置检查)。程序员编写检查见 [规则开发](rules-development.md)，断点见 [调试指南](debugging.md)。

Parsable syntax does not guarantee valid game rules. Projects supply required, range, combination, and cross-table constraints, automatically run by “校验并导出” (Validate and export). See [error handling](user-guide.md#6-怎样理解校验与后置检查), [rule development](rules-development.md), and [breakpoint debugging](debugging.md).
