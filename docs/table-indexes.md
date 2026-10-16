# Excel 主键、整表与分组查询

**Excel primary keys, full tables, and group queries**

配表人员阅读“怎样填写”并按 [使用文档](user-guide.md) 生成、导出即可；后面的查询 API、分配与加载校验说明面向项目程序员。

Configuration authors can read “How to fill the workbook” and follow the [user guide](user-guide.md) to generate/export. Query APIs, allocation behavior, and loading validation below are for developers.

本篇介绍新建表的五行表头：字段名、类型、主键、说明、所属端，第 6 行起填写数据。直接在**同一张业务 Sheet 的第 3 行**，将参与主键的字段标为 **主键**，至少标记一列，其余单元格留空。所有标记字段共同组成一套唯一键，每个标记字段单独查询时返回一组记录。AllTypes 也使用相同的五行表头。

New tables have five header rows: field, type, primary key, description, and target side; data starts on row 6. On **row 3 of the same business sheet**, mark key fields with the literal **主键**. Mark at least one column and leave other cells blank. Marked fields jointly form one unique key; querying a single marked field returns a group of records. AllTypes uses the same headers.

无需增加业务列，不需要给组命名，也不配置第二套主键。

No extra business column, group name, or second primary-key configuration is needed.

主键保证完整键唯一并生成查询接口；正数、范围和跨表条件属于业务检查，见 [规则开发与后置检查](rules-development.md)。新增/调整主键后重新生成代码并导出，调试查询和热更新见 [调试指南](debugging.md)。

Primary keys enforce complete-key uniqueness and generate query APIs. Positivity, ranges, and cross-table constraints are business checks; see [rule development and post-validation](rules-development.md). Regenerate/export after key changes. See [debugging](debugging.md) for queries and hot updates.

## 怎样填写

**How to fill the workbook**

可以打开 [双键和三键 Excel 示例](../samples/Data/CompositeKeys.xlsx)，其中包含 `Drop` 和 `SkillLevel` 两张业务表。

Open the [two-/three-component key workbook](../samples/Data/CompositeKeys.xlsx), containing `Drop` and `SkillLevel`.

`SkillLevel` 的填写方式如下，最左边“行号”仅用于说明，不是 Excel 数据列：

`SkillLevel` is filled as follows. The leftmost row-number column is explanatory, not an Excel data column:

| 行号<br>Row | A | B | C | D | E |
|---|---|---|---|---|---|
| 1：字段<br>1: Field | SkillId | Name | Level | Quality | Damage |
| 2：类型<br>2: Type | int | string | int | int | int |
| 3：标记<br>3: Marker | 主键<br>`主键` (key marker) |  | 主键<br>`主键` (key marker) | 主键<br>`主键` (key marker) |  |
| 4：说明<br>4: Description | 技能编号<br>Skill identifier | 技能名称<br>Skill name | 技能等级<br>Skill level | 品质<br>Quality | 伤害<br>Damage |
| 5：所属端<br>5: Target side | CS | CS | CS | CS | CS |
| 6：数据<br>6: Data | 101 | 火球<br>Fireball | 1 | 1 | 100 |
| 7：数据<br>7: Data | 101 | 火球<br>Fireball | 2 | 1 | 120 |
| 8：数据<br>8: Data | 101 | 火球<br>Fireball | 1 | 2 | 150 |
| 9：数据<br>9: Data | 102 | 冰箭<br>Ice arrow | 1 | 1 | 80 |

主键是 `(SkillId, Level, Quality)`，参数顺序按标记字段从左到右。字段可以不相邻，也不要求额外存在 `Id` 或 `GroupId`。完整三元组唯一；SkillId、Level、Quality 单独均允许重复。

The key is `(SkillId, Level, Quality)`, with parameters ordered left to right by marked columns. Fields need not be adjacent; no additional `Id` or `GroupId` is required. The complete triple is unique; each component may repeat independently.

双主键同样只标记两列，例如 `Drop` 的 `GroupId` 和 `Id`：`Get(groupId, id)` 取一条，`GetByGroupId(groupId)` 和 `GetById(id)` 各取一组。更多主键字段使用相同方式。只标一个字段时，该字段本身唯一，对应分组最多一条。

A two-component key marks two columns, e.g. `Drop.GroupId` and `Id`: `Get(groupId, id)` returns one record, while `GetByGroupId(groupId)` and `GetById(id)` return groups. More components follow the same rule. With one marked field, that field is unique and each group has at most one row.

主键支持顶层非可空基础类型和枚举；不支持集合、对象、可空值、JSON、表对象引用或展开的成员路径。主记录的键字段必须填写，纯空白不算值；明确填写 0 是有效键值。正数、范围等业务要求仍通过 C# 规则声明。

Keys support top-level non-nullable primitive types and enums. Collections, objects, nullable values, JSON, table references, and expanded member paths are unsupported. Main-record key cells must be filled; whitespace is not a value, but explicit 0 is valid. Declare positivity and range requirements through C# rules.

## 生成与调用

**Generation and usage**

第一次配置、修改字段或主键标记后，需要生成再导出；直接点击“校验并导出”可在确认生成清单后完成两步。只改数据直接导出。按钮操作见 [使用文档](user-guide.md#31-三个按钮什么时候用)，命令行见 [开发教程](project-development.md#13-命令行使用)。以下 API 由项目程序员接入。

Generate before exporting on first setup or after fields/key markers change. “校验并导出” (Validate and export) performs both after confirming the preview. Data-only edits can export directly. See [App buttons](user-guide.md#31-三个按钮什么时候用) and the [CLI tutorial](project-development.md#13-命令行使用). Developers integrate the following APIs.

生成的 `SkillLevelConfig` 是行对象，`SkillLevelConfigTable` 是包含全部行与索引的表，`ConfigSnapshot` 固定版本并按需缓存行和整表。单行加载使用生成的 `GetSkillLevelAsync(id, level, quality)` / `GetSkillLevel(id, level, quality)`，整表查询才创建完整分组索引，见 [按行懒加载](project-development.md#按行懒加载与跨表引用)。

Generated `SkillLevelConfig` represents a row; `SkillLevelConfigTable` contains all rows and indexes. `ConfigSnapshot` pins a version and caches rows/tables on demand. Use generated `GetSkillLevelAsync(id, level, quality)` / `GetSkillLevel(id, level, quality)` for individual rows. Full group indexes are created only for full-table queries; see [row-level lazy loading](project-development.md#按行懒加载与跨表引用).

```csharp
using ZrConfig.Models;
using ZrConfig.Unity;

var snapshot = UnityConfigRuntime.Current;
var table = snapshot.GetSkillLevelTable();

var row = table.Get(101, 1, 2);                    // 唯一一条，Damage = 150
bool found = table.TryGet(101, 2, 1, out var next); // 唯一一条，Damage = 120
var skillRows = table.GetBySkillId(101);           // 三条
var levelRows = table.GetByLevel(1);               // 三条
var qualityRows = table.GetByQuality(1);           // 三条

foreach (var item in skillRows) { /* 原有行对象 */ }
foreach (var item in table) { /* 整张表 */ }
int count = table.Count;
```

**示例英文说明 / English notes:**

- One unique row, Damage = 150
- One unique row, Damage = 120
- Three rows
- Three rows
- Three rows
- Iterates existing row objects.
- Iterates the whole table.


完整键不存在时，`TryGet` 返回 false，`Get` 抛出 `KeyNotFoundException`。单字段查询返回 `ConfigRowGroup<T>`；没有匹配返回共享空组，组内按原表记录顺序排列。

If the complete key is missing, `TryGet` returns false and `Get` throws `KeyNotFoundException`. Single-field queries return `ConfigRowGroup<T>`; no match yields a shared empty group. Matching rows retain source-record order.

后置规则也能使用相同的生成表入口：`context.GetSkillLevelTable().TryGet(101, 1, 2, out var row)`。索引首次查询时创建，并在本次导出的所有规则中复用；它查询的是本次导入的 Excel 数据。跨表使用需声明 `DependsOn`，调用时机、完整性与接口边界见 [校验索引表](rules-development.md#在后置检查中使用索引表)。

Post-validation can use the same generated accessors: `context.GetSkillLevelTable().TryGet(101, 1, 2, out var row)`. Indexes are created on first query and reused across all rules in that export, querying the current Excel import. Cross-table access requires `DependsOn`. See [validation index tables](rules-development.md#在后置检查中使用索引表) for timing, completeness, and API boundaries.

## 分配与热更新

**Allocations and hot updates**

初始化快照默认只读取清单；显式加载整表时才创建完整表及查询索引。单行入口使用表文件中的行索引按需解码，不会为分组查询提前解码全部行。整表建好后，查询复用已有表、行和只读分组，不临时 `new` 表、创建索引、复制行或 `ToList()`。单字段主键与同字段分组共用索引；复合键使用值类型和生成的逐字段比较/哈希，避免长元组装箱。

Snapshot initialization reads only the manifest by default. Explicit full-table loading creates the table and query indexes. Row access decodes on demand using the file's row index, without decoding every row ahead of group queries. Once built, queries reuse tables, rows, and read-only groups without temporary tables, new indexes, row copies, or `ToList()`. A single-field key shares its index with that field's groups; composite keys use value types and generated per-field comparison/hashing to avoid boxing long tuples.

运行时生成的 `snapshot.GetHeroTable()` 等入口同样复用已有表，通用入口显式传入 `GetTable<T>(codec)`。`CreateIndex` / `CreateGroupIndex` 保留为显式建索引工厂，每次调用确实都会构建；业务查找应使用生成的 `Get` / `TryGet` / `GetBy字段名`，不要在每次查询或 Update 中调用工厂。

Generated accessors such as `snapshot.GetHeroTable()` also reuse tables; generic access explicitly takes `GetTable<T>(codec)`. `CreateIndex` / `CreateGroupIndex` remain explicit factories and rebuild on every call. Use generated `Get` / `TryGet` / `GetByFieldName` for business lookups; do not call factories for each query or Update.

直接 foreach 表和 `ConfigRowGroup<T>` 使用值类型枚举器。转换成 IEnumerable/IReadOnlyList 接口后枚举，或另加 LINQ/ToList，不属于这个无分配保证。加载、重载、异常以及调用方自身逻辑仍可能分配。

Direct foreach over a table or `ConfigRowGroup<T>` uses value-type enumerators. Enumeration through IEnumerable/IReadOnlyList, or added LINQ/ToList operations, is outside this allocation-free guarantee. Loading, reloading, exceptions, and caller code may still allocate.

热更新成功后，新 Current 持有新表和新索引。业务统一从新 Current 重新取行、表与分组；旧快照查询和旧行引用访问失效，已持有的普通对象值不会原地更新。

After a successful hot update, new Current owns new tables/indexes. Reacquire rows, tables, and groups from it. Old snapshot queries and old row-reference access become invalid; ordinary object values already held are not updated in place.

## 校验与约束

**Validation and constraints**

- 普通表的完整主键重复或任一键为空，会定位到 Excel 的实际行列并阻止导出。<br>
  In ordinary tables, duplicate complete keys or empty key components block export and report actual Excel coordinates.
- 多行数组表按完整主键归并逻辑记录；同一完整键再次出现表示该记录的续行，全部键留空也可续接前一条记录。只填写部分键会报错。<br>
  Multi-row array tables merge logical records by complete key. Repeating a key continues that record; all key cells blank may continue the preceding record. Partial keys are errors.
- 主键变化影响 Schema Hash。未重新生成的源码或程序集不能沿用旧索引导出。运行时数据若有重复唯一键，该表加载失败，不发布半成品缓存；需要切换前发现这类错误，使用全量或预热重载，失败时保留旧快照。<br>
  Key changes affect Schema Hash. Stale source/assemblies cannot export using old indexes. Duplicate unique keys in runtime data fail table loading without publishing a partial cache. Use full or warmup reload to detect these before switching; failures retain the old snapshot.
- 第 3 行至少一格为“主键”，其余格子留空，不能填说明或其他校验标签。第 4 行的说明不建立业务规则。<br>
  At least one row-3 cell must contain `主键`; leave all others blank, without descriptions or other validation tags. Row-4 descriptions do not define business rules.
- 主键直接在 Excel 标记，不需要另建配置文件；遇到同一表存在重复主键配置的诊断，由程序员清理重复声明后重新生成。<br>
  Mark keys directly in Excel; no separate file is required. If duplicate key configuration is reported for one table, a developer must remove duplicate declarations and regenerate.
- 直接 `XxxConfig` 表对象引用仍按目标唯一 int Id 解析；没有 Id 的联合键表，通过保存各键值后调用生成的 Get/TryGet 查询。<br>
  Direct `XxxConfig` references still resolve the target's unique int Id. For composite-key tables without Id, store the key values and call generated Get/TryGet.
- `[Unique(Id)]` 或 `[PositiveKey(Id)]` 会要求 Id 单独唯一。联合键允许单个分量重复时，不应给该分量添加这些规则；需要正数时使用 `[Positive(Id)]`。<br>
  `[Unique(Id)]` and `[PositiveKey(Id)]` require Id to be individually unique. Do not apply them to a composite-key component that may repeat; use `[Positive(Id)]` when only positivity is required.
