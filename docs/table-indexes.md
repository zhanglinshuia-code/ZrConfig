# Excel 主键、整表与分组查询

**中文** | [English](table-indexes.en.md)

配表人员阅读“怎样填写”并按 [使用文档](user-guide.md) 生成、导出即可；后面的查询 API、分配与加载校验说明面向项目程序员。

本篇介绍新建表的五行表头：字段名、类型、主键、说明、所属端，第 6 行起填写数据。直接在**同一张业务 Sheet 的第 3 行**，将参与主键的字段标为 **主键**，至少标记一列，其余单元格留空。所有标记字段共同组成一套唯一键，每个标记字段单独查询时返回一组记录。AllTypes 也使用相同的五行表头。

无需增加业务列，不需要给组命名，也不配置第二套主键。

主键保证完整键唯一并生成查询接口；正数、范围和跨表条件属于业务检查，见 [规则开发与后置检查](rules-development.md)。新增/调整主键后重新生成代码并导出，调试查询和热更新见 [调试指南](debugging.md)。

## 怎样填写

可以打开 [双键和三键 Excel 示例](../samples/Data/CompositeKeys.xlsx)，其中包含 `Drop` 和 `SkillLevel` 两张业务表。

`SkillLevel` 的填写方式如下，最左边“行号”仅用于说明，不是 Excel 数据列：

| 行号 | A | B | C | D | E |
|---|---|---|---|---|---|
| 1：字段 | SkillId | Name | Level | Quality | Damage |
| 2：类型 | int | string | int | int | int |
| 3：标记 | 主键 | | 主键 | 主键 | |
| 4：说明 | 技能编号 | 技能名称 | 技能等级 | 品质 | 伤害 |
| 5：所属端 | CS | CS | CS | CS | CS |
| 6：数据 | 101 | 火球 | 1 | 1 | 100 |
| 7：数据 | 101 | 火球 | 2 | 1 | 120 |
| 8：数据 | 101 | 火球 | 1 | 2 | 150 |
| 9：数据 | 102 | 冰箭 | 1 | 1 | 80 |

主键是 `(SkillId, Level, Quality)`，参数顺序按标记字段从左到右。字段可以不相邻，也不要求额外存在 `Id` 或 `GroupId`。完整三元组唯一；SkillId、Level、Quality 单独均允许重复。

双主键同样只标记两列，例如 `Drop` 的 `GroupId` 和 `Id`：`Get(groupId, id)` 取一条，`GetByGroupId(groupId)` 和 `GetById(id)` 各取一组。更多主键字段使用相同方式。只标一个字段时，该字段本身唯一，对应分组最多一条。

主键支持顶层非可空基础类型和枚举；不支持集合、对象、可空值、JSON、表对象引用或展开的成员路径。主记录的键字段必须填写，纯空白不算值；明确填写 0 是有效键值。正数、范围等业务要求仍通过 C# 规则声明。

## 生成与调用

第一次配置、修改字段或主键标记后，需要生成再导出；直接点击“校验并导出”可在确认生成清单后完成两步。只改数据直接导出。按钮操作见 [使用文档](user-guide.md#31-三个按钮什么时候用)，命令行见 [开发教程](project-development.md#13-命令行使用)。以下 API 由项目程序员接入。

生成的 `SkillLevelConfig` 是行对象，`SkillLevelConfigTable` 是包含全部行与索引的表，`ConfigSnapshot` 固定版本并按需缓存行和整表。单行加载使用生成的 `GetSkillLevelAsync(id, level, quality)` / `GetSkillLevel(id, level, quality)`，整表查询才创建完整分组索引，见 [按行懒加载](project-development.md#按行懒加载与跨表引用)。

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

完整键不存在时，`TryGet` 返回 false，`Get` 抛出 `KeyNotFoundException`。单字段查询返回 `ConfigRowGroup<T>`；没有匹配返回共享空组，组内按原表记录顺序排列。

后置规则也能使用相同的生成表入口：`context.GetSkillLevelTable().TryGet(101, 1, 2, out var row)`。索引首次查询时创建，并在本次导出的所有规则中复用；它查询的是本次导入的 Excel 数据。跨表使用需声明 `DependsOn`，调用时机、完整性与接口边界见 [校验索引表](rules-development.md#在后置检查中使用索引表)。

## 分配与热更新

初始化快照默认只读取清单；显式加载整表时才创建完整表及查询索引。单行入口使用表文件中的行索引按需解码，不会为分组查询提前解码全部行。整表建好后，查询复用已有表、行和只读分组，不临时 `new` 表、创建索引、复制行或 `ToList()`。单字段主键与同字段分组共用索引；复合键使用值类型和生成的逐字段比较/哈希，避免长元组装箱。

运行时生成的 `snapshot.GetHeroTable()` 等入口同样复用已有表，通用入口显式传入 `GetTable<T>(codec)`。`CreateIndex` / `CreateGroupIndex` 保留为显式建索引工厂，每次调用确实都会构建；业务查找应使用生成的 `Get` / `TryGet` / `GetBy字段名`，不要在每次查询或 Update 中调用工厂。

直接 foreach 表和 `ConfigRowGroup<T>` 使用值类型枚举器。转换成 IEnumerable/IReadOnlyList 接口后枚举，或另加 LINQ/ToList，不属于这个无分配保证。加载、重载、异常以及调用方自身逻辑仍可能分配。

热更新成功后，新 Current 持有新表和新索引。业务统一从新 Current 重新取行、表与分组；旧快照查询和旧行引用访问失效，已持有的普通对象值不会原地更新。

## 校验与约束

- 普通表的完整主键重复或任一键为空，会定位到 Excel 的实际行列并阻止导出。
- 多行数组表按完整主键归并逻辑记录；同一完整键再次出现表示该记录的续行，全部键留空也可续接前一条记录。只填写部分键会报错。
- 主键变化影响 Schema Hash。未重新生成的源码或程序集不能沿用旧索引导出。运行时数据若有重复唯一键，该表加载失败，不发布半成品缓存；需要切换前发现这类错误，使用全量或预热重载，失败时保留旧快照。
- 第 3 行至少一格为“主键”，其余格子留空，不能填说明或其他校验标签。第 4 行的说明不建立业务规则。
- 主键直接在 Excel 标记，不需要另建配置文件；遇到同一表存在重复主键配置的诊断，由程序员清理重复声明后重新生成。
- 直接 `XxxConfig` 表对象引用仍按目标唯一 int Id 解析；没有 Id 的联合键表，通过保存各键值后调用生成的 Get/TryGet 查询。
- `[Unique(Id)]` 或 `[PositiveKey(Id)]` 会要求 Id 单独唯一。联合键允许单个分量重复时，不应给该分量添加这些规则；需要正数时使用 `[Positive(Id)]`。
