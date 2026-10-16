# 规则开发与后置检查

**Rule development and post-validation**

**可编程后置检查是 ZrConfig 的核心能力：把原本要到游戏运行时才发现的配表业务错误，提前到导出阶段检查。** 程序员将项目约束写成 C#，在 Excel 已解析、列表已归组、对象引用已关联之后执行，并在发现 Error 时阻止发布。规则面对的是强类型记录，可以访问嵌套成员、判断具体动作子类、遍历整表或查询关联表。

**Programmable post-validation is a core ZrConfig feature: catch configuration business errors during export instead of gameplay.** Developers express constraints in C#, after Excel parsing, list grouping, and reference linking. Error diagnostics block publication. Rules work on typed records and can inspect nested members/concrete action subclasses, traverse tables, and query related tables.

| 检查需求<br>Need | 对应能力<br>Capability | 正式示例<br>Official example |
| --- | --- | --- |
| 名称、数值等单字段约束<br>Single-field names/numbers | 内置标签与可复用 `IFieldRule<T>`<br>Built-in attributes and reusable `IFieldRule<T>` | 道具与任务共用名称检查<br>Shared item/quest name check |
| 同一记录中多个字段的关系<br>Relationships within a record | 逐行 `Validate`<br>Row `Validate` | 区间顺序、动作延时、Buff 层数<br>Interval ordering, action delay, buff stacks |
| 多条记录之间的条件<br>Conditions across records | 整表 `ValidateTable`<br>Table `ValidateTable` | 重复上架的道具与价格组合<br>Duplicate item/price offer combinations |
| 关联表的业务条件<br>Related-table business constraints | `DependsOn` 与后置方法，访问对象或查询生成索引表<br>`DependsOn` plus post-validation, object access, or generated indexes | 商品引用道具的启用状态、奖励数量与道具堆叠上限<br>Referenced item enabled state; rewards versus stack limits |

针对嵌套字段报告 `Groups[1].Actions[0].Ratio` 等路径时，诊断会保留排序前的 Excel 单元格位置。规则支持 Info、Success、Warning、Error 四级诊断，只有 Error 阻止发布。只修改规则后直接重新导出，无需重新构建 App；同一份规则源码可用于 App 和 CLI。

Paths such as `Groups[1].Actions[0].Ratio` retain original pre-sort Excel locations. Info, Success, Warning, and Error are supported; only Error blocks publication. Rule-only edits need only another export, not an App rebuild. App/CLI use the same rules.

**规则也可以调试。** 用 Visual Studio / Rider 启动 CLI 的导出流程并添加 `--rules-debug`，即可在 `Validate`、`ValidateTable` 或自定义字段检查的可执行语句上设置断点，单步观察导入值、嵌套对象、关联记录与调用栈。工具动态编译时关闭优化并加载 Portable PDB，便于把检查逻辑本身也验证清楚。配置步骤见 [Rider](debugging.md#3-rider从-ide-启动调试) 与 [Visual Studio](debugging.md#4-visual-studio从-ide-启动调试)。

**Rules are debuggable.** Launch CLI export in Visual Studio / Rider with `--rules-debug`, break on executable statements in `Validate`, `ValidateTable`, or field checks, and inspect imported/nested/related data and call stacks. Dynamic compilation disables optimization and loads Portable PDB. See [Rider](debugging.md#3-rider从-ide-启动调试) and [Visual Studio](debugging.md#4-visual-studio从-ide-启动调试).

本文说明规则放在哪里、什么时候执行、怎样报告 Excel 位置，以及怎样验证规则生效。程序员完整例子见 [项目开发教程第 3～12 章](project-development.md#3-检查规则到底是什么)；Rider、Visual Studio 断点步骤见 [调试指南](debugging.md)；修改工具自身见 [开发指南](development.md)。

This guide covers placement, execution timing, Excel locations, and verification. See [tutorial chapters 3–12](project-development.md#3-检查规则到底是什么) for full examples, [debugging](debugging.md) for IDE steps, and [tool development](development.md) for tool changes.

本文的正式 Hero/Skill 示例使用 samples 的对象引用字段；开发教程第 12 章使用独立练习的 SkillId、Level 等字段。同名表不代表模型相同，复制规则前先确认对应的 Config，完整项目只选用一套来源。

Official Hero/Skill examples here use sample object-reference fields. Tutorial chapter 12 uses separate SkillId/Level exercise fields. Equal table names do not imply equal models. Check Config before copying rules and choose only one source set per project.

## 1. 工程与源码归属

**1. Projects and source ownership**

仓库全部示例共用 `unity/ZrConfig.Sample/Assets/HotUpdate/Config`、`unity/ZrConfig.Sample/Assets/Editor/ConfigRules`。Unity 通过各自的 asmdef 编译模型与 Editor 规则，App/CLI 在导出时编译同一份源码，无需维护副本或另建 Models/Validators 工程。

All samples share `unity/ZrConfig.Sample/Assets/HotUpdate/Config` and `unity/ZrConfig.Sample/Assets/Editor/ConfigRules`. Unity compiles models/Editor rules through asmdefs; App/CLI compile the same source on export. No copies or extra Models/Validators projects are needed.

仓库 Unity 示例直接使用 Assets/HotUpdate/Config 与 Assets/Editor/ConfigRules：前者编译为 `ZrConfig.Models`，后者通过 `includePlatforms: ["Editor"]` 编译为 `ZrConfig.Sample.Rules.Editor`，不进入 Player。源码可在 Unity 的 Assets 中直接打开、修改和重新生成。具体路径与热更接入边界见 [示例源码与 Editor 检查](../samples/README.md#配置源码与-editor-后置检查)。

Assets/HotUpdate/Config compiles as `ZrConfig.Models`; Assets/Editor/ConfigRules uses `includePlatforms: ["Editor"]` to compile as `ZrConfig.Sample.Rules.Editor`, excluded from Players. Browse/edit/regenerate directly in Assets. See [sample source and Editor checks](../samples/README.md#配置源码与-editor-后置检查) for paths and hot-update boundaries.

自己的 Unity 工程可采用：

Your Unity project can use:

```text
Assets/
  HotUpdate/Config/
    CustomTypes/                      手写枚举、类、结构体 / Handwritten enums, classes, and structs
    Serialization/              工具提供的支持源码；可选项目转换器 / Tool-provided support; optional project converters
    Generated/                  模型、Codec、唯一的 TableCodecs / Models, codecs, and the single TableCodecs
  Editor/ConfigRules/            纯 C# 业务检查 / Pure C# business checks
```

App 第三个目录选择 Config 根目录；第四个目录选择 ConfigRules。CustomTypes 是示例整理约定，手写类型和转换器可直接放在所选代码根目录或任意子目录中。规则目录放在代码根目录之外，避免依赖生成模型的规则参与生成前的类型编译。规则也可放在 Assets 外，此时由导表工具编译，Unity solution 不一定收录它们。需要 IDE 编译提示时，使用编辑器程序集或独立规则 SDK 工程，不要复制第二份规则。

Select Config root in the App's third path and ConfigRules in the fourth. CustomTypes is an organizational convention; handwritten types/converters may live at the code root or any subdirectory. Keep rules outside that root so rules depending on generated models do not enter pre-generation type compilation. Rules may live outside Assets, compiled by the tool and possibly absent from Unity's solution. For IDE feedback, use an Editor assembly or standalone rules SDK project, not a second rule copy.

上述是程序员的源码布局。通用读写支持由工具初始化，普通配表人员只使用项目提供的目录与规则；无需编写序列化支持或完成本篇编程步骤。特殊转换器只有项目确需扩展时才编写。

This is a developer layout. The tool initializes shared serialization support; ordinary authors use provided paths/rules without implementing support or completing this programming workflow. Write special converters only when needed.

Config 所属程序集引用 Runtime；规则所属编辑器程序集引用模型程序集、Runtime、Validation。按项目 asmdef 实际配置引用，不要把 Validation 或业务规则带进 Player。不要同时导入旧 Models DLL 和同名生成源码。

Config's assembly references Runtime; the rules Editor assembly references models, Runtime, and Validation. Match actual asmdef references and exclude Validation/business rules from Players. Do not import an old Models DLL alongside generated source with the same names.

导表 App 不携带项目 Models/Validators DLL。每次导出把 Config 下的 `.cs` 与指定规则目录下的 `.cs` 一起编译，扫描非抽象、非开放泛型的 `ITableRules` 实现。通常写 `public sealed class XxxRules : TableRules<XxxConfig>`，保留 public 无参构造函数即可。

The App ships no project Models/Validators DLL. Each export compiles `.cs` under Config and the specified rules directory, then discovers nonabstract, closed `ITableRules` implementations. Usually use `public sealed class XxxRules : TableRules<XxxConfig>` with a public parameterless constructor.

- 文件名不负责绑定，`TableRules<T>` 中的 T 决定目标表。<br>
  Filenames do not bind rules; T in `TableRules<T>` selects the table.
- 同一张表可以有多个规则类；`IFieldRule<T>` 只有被 `[Check]` 绑定才执行。<br>
  A table may have multiple rule classes. `IFieldRule<T>` executes only when bound by `[Check]`.
- 递归扫描子目录，但跳过 bin、obj、点号开头的文件/目录和链接文件/目录。<br>
  Scanning is recursive, skipping bin, obj, dot-prefixed entries, and linked files/directories.
- 规则目录留空表示关闭业务规则；填写不存在、空目录或没有可用表规则的目录会阻止导出。<br>
  A blank rules path disables business rules. A specified nonexistent/empty directory or one without usable table rules blocks export.
- 动态编译不读取项目的 csproj、NuGet 引用或 asmdef，只引用 .NET 基础库、Runtime、Validation。不能直接依赖 UnityEngine、UnityEditor 或游戏程序集。<br>
  Dynamic compilation ignores csproj, NuGet references, and asmdef. It references only .NET base libraries, Runtime, and Validation; no direct UnityEngine, UnityEditor, or game-assembly dependencies.
- App 编译不定义 `UNITY_EDITOR`。不要把整个规则类包在这个条件内；Unity 中通过 Editor 目录/asmdef 隔离它。<br>
  The App does not define `UNITY_EDITOR`. Do not wrap whole rule classes in it; isolate them using Unity Editor directories/asmdefs.
- 显式写 `using System`、`System.Collections.Generic`、`System.Linq` 等，保证 Unity 与工具两边都能编译。<br>
  Write explicit `using System`, `System.Collections.Generic`, `System.Linq`, etc., so both Unity and the tool compile.

只改规则后直接导出。改表结构、主键、CustomTypes 或转换器约定后先生成，再导出。生成与导出是独立操作。

Rule-only changes need another export. Schema, key, CustomTypes, or converter-contract changes require generation before export. Generation/export are separate operations.

## 2. “后置”指解析后、发布前

**2. “Post” means after parsing, before publication**

```text
编译模型与规则 / Compile models and rules
  → 校对 Schema，解析 Excel，合并多行记录 / → Check schema, parse Excel, merge multi-row records
  → 关联直接表对象引用，确定哪些表完整可用 / → Link direct table references and determine table completeness
  → 发现规则、绑定标签（空表也绑定） / → Discover rules and bind attributes, including empty tables
  → 执行所有字段标签 / → Run all field attributes
  → 每个规则类逐行 Validate，再整表 ValidateTable / → For each rule class, run per-row Validate, then ValidateTable
  → 无 Error 才写二进制、回读、计算哈希 / → Only without Error: write binary, read back, calculate hashes
  → 最后替换 current.json / → Finally replace current.json
```

`IsComplete` 表示解析和关联完整，不表示业务规则全部通过。字段标签报错以后，后置方法仍可能执行；不能假设 `[Range]` 已经替方法过滤掉越界行。规则按顺序执行，不依赖多个规则类之间的相对顺序，也不修改配置给其他规则使用。

`IsComplete` means parsing/linking completed, not that business checks passed. Post-methods may still run after field errors; `[Range]` does not filter bad rows out for them. Rules execute sequentially, but do not depend on relative order between classes or mutate configuration for other rules.

| 情况<br>Condition | 实际行为<br>Behavior |
| --- | --- |
| 某行类型解析失败<br>A row fails type parsing | 该行不交给字段检查；当前表标为不完整<br>No field checks for that row; table marked incomplete |
| 当前表不完整<br>Current table incomplete | 已成功解析行仍可接受适用的字段检查；该表后置方法跳过<br>Applicable field checks may inspect successfully parsed rows; table post-methods skipped |
| Reference 的目标表不完整<br>Reference target incomplete | `REF_CHECK_SKIPPED`；不把残缺目标表当完整外键集合<br>`REF_CHECK_SKIPPED`; partial target data is not treated as a complete foreign-key set |
| DependsOn 表缺失或不完整<br>DependsOn table absent/incomplete | 该规则类的 Validate 和 ValidateTable 都跳过，记录 `BUSINESS_CHECK_SKIPPED`<br>Both Validate and ValidateTable skipped for that class; `BUSINESS_CHECK_SKIPPED` |
| 直接对象引用关联失败<br>Direct reference linking fails | `TABLE_REFERENCE`；引用链传播不完整状态<br>`TABLE_REFERENCE`; incompleteness propagates through references |
| 字段标签或其他规则报 Error<br>Attribute/another rule reports Error | 收集其他可独立判断的问题，最后阻止发布<br>Collect other independently decidable problems, then block publication |
| 逐行方法抛异常<br>Row method throws | `VALIDATOR_EXCEPTION`，保留该行位置，继续其他行<br>`VALIDATOR_EXCEPTION` with row location; continue other rows |
| 整表方法抛异常<br>Table method throws | `VALIDATOR_EXCEPTION`，定位表，不自动归属某行<br>`VALIDATOR_EXCEPTION` located at the table, not automatically a row |
| 只有 Info / Success / Warning<br>Only Info / Success / Warning | 允许发布，仍应阅读警告；Success 仅代表某项检查通过<br>Publication allowed; read warnings. Success means only a particular check passed |

`BUSINESS_CHECK_SKIPPED` 在声明的依赖表根本缺失时是 Error；解析不完整时通常是 Warning，原来的解析 Error 已经阻止发布。不要把“跳过”视作校验成功。

`BUSINESS_CHECK_SKIPPED` is Error for a completely absent declared dependency. For incomplete parsing it is usually Warning because the original parse Error already blocks publication. Skipped does not mean passed.

## 3. 选哪一种检查

**3. Choose a check**

| 需求<br>Requirement | 实现<br>Implementation |
| --- | --- |
| 顶层字段必填、唯一、范围<br>Required/unique/range on top-level fields | 内置标签<br>Built-in attributes |
| 可复用单字段条件<br>Reusable single-field condition | `IFieldRule<T>` + `[Check]` |
| 一行多字段、对象、数组内容<br>Multiple fields, objects, or arrays in one row | `Validate(row, context)` |
| 整表组合唯一、总和、数量、顺序<br>Table-wide combination uniqueness, totals, counts, order | `ValidateTable(rows, context)` |
| 读取另一张表进行业务判断<br>Business checks reading another table | `[DependsOn(typeof(TargetConfig))]` + 后置方法<br>`[DependsOn(typeof(TargetConfig))]` + post-methods |
| 游戏查询索引、联合键<br>Game query indexes/composite keys | Excel 第 3 行“主键”，见 [主键文档](table-indexes.md)<br>Literal `主键` on Excel row 3; see [keys](table-indexes.md) |

### 内置标签速查

**Built-in attribute reference**

标签写在规则类上，不写进 Generated。字段名称限顶层逻辑字段，嵌套成员放到后置方法检查。

Put attributes on rule classes, not Generated. Field names must be top-level logical fields; inspect nested members in post-methods.

| 标签<br>Attribute | 语义与边界<br>Semantics and boundaries |
| --- | --- |
| `Required(field)` | 检查导入时的空白标记，包括未加引号的纯空格；明确填写 0、false、`[]` 不算空白<br>Checks import blank markers, including unquoted whitespace; explicit 0, false, and `[]` are not blank |
| `Unique(field)` | 非空值按默认相等语义比较，字符串不自动 Trim/忽略大小写；不是数组内容去重<br>Compares nonblank values using default equality; no automatic string Trim/case folding; not array-content deduplication |
| `Positive(field)` | 已填写数值 > 0<br>Filled numeric value > 0 |
| `PositiveKey(field)` | Required + Unique + Positive；不建立运行时索引<br>Required + Unique + Positive; creates no runtime index |
| `Range(field, min, max)` | 已填写数值在闭区间内，边界必须有限且有序<br>Filled number in the inclusive interval; bounds must be finite and ordered |
| `Reference(field, typeof(Target), targetField)` | 两端属性类型必须一致；目标已填写值中必须存在来源值<br>Property types must match; source value must exist among filled target values |
| `Check(field, typeof(MyRule))` | MyRule 必须能无参实例化，并实现与字段类型一致的 `IFieldRule<T>`<br>MyRule must be parameterlessly instantiable and implement matching `IFieldRule<T>` |

Positive、PositiveKey、Range 当前只支持普通 `int`、`long`、`float`、`double`。`decimal`、`uint`、`int?` 等使用对应泛型的字段规则或后置判断。

Positive, PositiveKey, and Range currently support only ordinary `int`, `long`, `float`, and `double`. Use matching generic field rules or post-methods for `decimal`, `uint`, `int?`, etc.

Unique、Positive、Range、Check 跳过空白；要必填另加 Required。Reference 默认拒绝空白；`AllowDefaultValue = true` 允许空白或该类型默认值表示无引用。Reference 只验证存在，不保证目标唯一，不生成对象关联。目标唯一性另行约束。

Unique, Positive, Range, and Check skip blank values; add Required separately. Reference rejects blanks by default; `AllowDefaultValue = true` permits blank/default as no reference. Reference checks existence only, without target uniqueness or object linking. Enforce target uniqueness separately.

直接表对象引用 `SkillConfig` 的 0/空白会标记为空引用，所以 `[Required]` 会拒绝它；普通 int 列明确填 0 则能通过 Required。显式填写的字符串字面量、集合内容是否有效仍由业务规则检查。`[ConfigRequired]` 属于手写类型成员的解析约束，与表字段 `[Required]` 不同。

Direct `SkillConfig` references treat 0/blank as null, so `[Required]` rejects them; explicit 0 in an ordinary int field passes Required. Business rules still validate explicit string/collection contents. `[ConfigRequired]` is a parsing constraint on handwritten members, distinct from table-field `[Required]`.

联合键只保证完整组合唯一。不要给允许重复的单个键分量添加 Unique/PositiveKey。

Composite keys guarantee only complete-combination uniqueness. Do not apply Unique/PositiveKey to a component allowed to repeat.

## 4. 逐行与整表例子

**4. Row and table examples**

以下示例适用于仓库 `TableReferences.xlsx` 生成的 HeroConfig（Id、Name、Skill、OtherSkill、Mentor）。放进项目实际 Rules 目录即可自动发现。这里是演示规则，不要求给正式示例附加新的业务条件。

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
                "名称不能超过 32 个字符。", "HERO_NAME_LENGTH");
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
                    "忽略首尾空格与大小写后名称重复。", "HERO_NAME_DUPLICATE");
        }
    }
}
```

**示例英文说明 / English notes:**

- Name must not exceed 32 characters.
- Duplicate name after trimming and ignoring case.


整表方法拿到的是合并多行之后的逻辑记录，行数不等于 Excel 物理行数。空表也会调用 ValidateTable；先判断 Count 再使用 `rows[0]`。当前 ValidationContext 没有无行的整表 Error 重载，不能伪造行来报告“必须至少有一行”；需要这种约束时应扩展诊断接口并补测试。

Table methods receive merged logical records, not physical Excel rows. ValidateTable runs on empty tables too; check Count before `rows[0]`. ValidationContext currently has no row-free table Error overload. Do not invent a row to report “at least one row required”; extend diagnostics and add tests if needed.

## 5. 跨表与对象引用

**5. Cross-table checks and object references**

只写 Reference 标签不用另加 DependsOn；业务方法调用 `GetTable<T>()`、生成的 `GetXxxTable()` 或读取目标对象成员时，声明对应依赖。

A Reference attribute alone needs no extra DependsOn. Business methods using `GetTable<T>()`, generated `GetXxxTable()`, or target members must declare dependencies.

仓库双向引用例子的附加检查：英雄选的技能必须反向指向这个英雄。空引用在该示例中合法。

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
                "所选技能必须反向引用当前英雄。", "SKILL_OWNER");
    }
}
```

**示例英文说明 / English notes:**

- The selected skill must reference the current hero back.


对象引用支持双向与自引用，所以不要无界递归遍历对象图；遍历长链时保存 visited 集合或设置明确终止条件。

Bidirectional/self references are supported. Avoid unbounded recursive traversal; track visited objects or define explicit termination for long chains.

整数外键版本见 [教程跨表完整例子](project-development.md#8-跨表后置检查)。直接对象引用已经完成关联，访问 `row.Skill` 即可；按主键或组合键查找其他记录时，使用下面的生成表接口。

See the [integer foreign-key example](project-development.md#8-跨表后置检查). Direct references are already linked, so access `row.Skill`. To find other records by simple/composite keys, use generated table APIs below.

DependsOn 不表示“目标业务规则已经通过”，也不是执行顺序声明。目标行的值仍需检查可用范围。本表规则与跨表规则可拆成不同类，让依赖失效时本表条件继续执行。

DependsOn neither means target business rules passed nor specifies execution order. Check target values' usable ranges yourself. Separate local and cross-table rules so local checks can still run when dependencies fail.

### 在后置检查中使用索引表

**Use indexed tables in post-validation**

生成代码后，`ValidationContext` 与运行时快照共用 `GetXxxTable()` 查询入口。例如，在声明了 `[DependsOn(typeof(SkillLevelConfig))]` 的规则方法中：

Generated `GetXxxTable()` accessors are shared by `ValidationContext` and runtime snapshots. For example, inside a method declaring `[DependsOn(typeof(SkillLevelConfig))]`:

```csharp
var levels = context.GetSkillLevelTable();
bool found = levels.TryGet(101, 1, 2, out var level);
var group = levels.GetBySkillId(101);
```

首次访问某张表时，工具用本次 Excel 导入的行创建生成表，建立主键与分组索引。同一次导出的所有规则、逐行 `Validate` 和整表 `ValidateTable` 共用这一个实例；后续取表不会重建索引。未访问的表不创建索引表，下次导出创建新上下文，不读取旧发布版本的索引。缓存随本轮上下文结束释放，不应把表或上下文存入静态字段。

First access builds a generated table and key/group indexes from the current Excel import. All rules, row Validate, and ValidateTable in that export share one instance; later access does not rebuild. Unaccessed tables get no index table. Each export creates a fresh context, not indexes from an old published version. Caches are released with the context; never retain tables/contexts statically.

索引查询返回原始导入行，可继续调用 `context.Error`、`IsEmpty`，保持原 Excel 单元格定位。空表可以查询；`TryGet` 未命中返回 false，`Get` 未命中抛异常，分组未命中返回空组。当前表或依赖表解析不完整时仍按原规则跳过后置方法；即使未声明依赖，主动取一个缺失或不完整的表也会抛 `TableUnavailableException`，不会查询残缺数据。字段业务 Error 不代表表不完整，索引中的业务值仍需检查。

Queries return original imported rows, preserving `context.Error`, `IsEmpty`, and Excel locations. Empty tables can be queried. Missing TryGet returns false, Get throws, groups return empty. Incomplete current/dependency tables still skip post-methods. Explicitly requesting a missing/incomplete table throws `TableUnavailableException` even without a dependency declaration; partial data is never queried. Business Error does not mean incomplete parsing, so indexed values still need checking.

`context.GetTable<T>()` 返回本次导入的 `ConfigTable<T>`，与生成的强类型入口复用同一个表和索引。生成入口通过 Runtime 的 `IConfigTableProvider` 显式传入 Codec，模型不引用 Validation，Unity Player 无需包含检查程序集。手动构造 `ValidationContext` 时必须提供本次数据的表工厂。

`context.GetTable<T>()` returns this import's `ConfigTable<T>` and shares the table/indexes with typed accessors. Generated methods pass codecs through Runtime's `IConfigTableProvider`; models do not reference Validation, and Players need no validation assembly. Manually constructed ValidationContext requires a factory for this run's tables.

不要在逐行方法中 `new XxxConfigTable(...)`、`CreateIndex` 或 `ToDictionary`，这些操作仍会重复建表。如果检查的键不是已声明主键、需要特殊比较规则，可以在 `ValidateTable` 内建立一次自定义字典，并处理重复键。`ValidateTable` 在本规则类所有 `Validate` 之后执行，不能在其中准备供前面的逐行方法使用的索引。每张表须在 Excel 第 3 行声明主键；`[PositiveKey]` 仅是业务校验，不代替主键声明。

Do not `new XxxConfigTable(...)`, CreateIndex, or ToDictionary per row; they rebuild. If keys are undeclared or comparison differs, build one custom dictionary in ValidateTable and handle duplicates. ValidateTable runs after all row methods of that class, so it cannot prepare indexes for earlier row calls. Every table declares keys on Excel row 3; `[PositiveKey]` is only business validation.

## 6. 报错路径、空白与异常

**6. Diagnostic paths, blanks, and exceptions**

```csharp
context.Error(row, nameof(AllTypesConfig.Id), "编号不符合业务要求。", "MY_ID");
context.Warning(row, "Settings.MaxTargets", "目标数量较高，请确认。", "TARGET_NOTICE");
context.Error(row, $"Actions[{i}].Delay", "延时不能为负数。", "ACTION_DELAY");
bool omitted = context.IsEmpty(row, "Settings.MaxTargets");
```

**示例英文说明 / English notes:**

- The identifier violates business requirements.
- High target count; please confirm.
- Delay must not be negative.


上述为方法内片段：row 必须是对应表的真实导入记录，i 是有效数组下标。完整嵌套检查见 [AllTypesRules.cs](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/AllTypesRules.cs)。

These are method fragments: row must be a real imported record of the corresponding table, and i a valid array index. See [AllTypesRules.cs](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/AllTypesRules.cs) for complete nested checks.

| 报告对象<br>Location | 第二参数<br>Second argument |
| --- | --- |
| 顶层字段<br>Top-level field | `nameof(AllTypesConfig.Id)` |
| 对象成员<br>Object member | `Settings.MaxTargets` |
| 数组元素成员<br>Array element member | `Actions[1].Delay` |
| 三层多行列表<br>Three-level multi-row list | `Phases[1].Groups[0].Actions[0].Delay` |

第一参数始终是完整 row，不是 Settings、action 或手动 new 的副本。数组下标使用最终排序后的 0 起始下标；工具映射回原始物理行列。JSON 单格成员定位到承载 JSON 的格子。诊断路径必须确实存在；不要使用输入表头的 `Actions[].Delay` 或沿对象引用写 `Skill.Name`，后者应报告当前行的 Skill，或者用目标行报告其 Name。

The first argument is always the complete row, not Settings, an action, or a new copy. Use zero-based indexes after sorting; the tool maps to original physical cells. JSON members locate the containing cell. Paths must exist. Do not use input paths like `Actions[].Delay` or traverse references with `Skill.Name`; report the source row's Skill or the target row's Name instead.

`VALIDATOR_LOCATION` 表示 row 不属于本次导入；`VALIDATOR_FIELD` 表示诊断路径找不到。`IsEmpty` 不检查路径拼写，未知路径可能返回 false，所以不能用它来验证字段存在。

`VALIDATOR_LOCATION` means the row is not from this import; `VALIDATOR_FIELD` means the path is missing. `IsEmpty` does not validate spelling and may return false for unknown paths; do not use it to prove field existence.

四种报告方法的第四参数都是稳定诊断代号，省略为 CUSTOM；字段规则中省略代号为 FIELD_RULE。正常业务不通过使用 Error，不通过 throw 表达。异常用于发现程序缺陷，日志保留完整调用栈和内层异常。

The fourth argument of each reporting method is a stable code, default CUSTOM; field rules default FIELD_RULE. Use Error for expected business failures, not throw. Exceptions expose programming defects and retain full stacks/inner exceptions.

### 日志级别

**Diagnostic levels**

App 1.3.5 起，行规则和字段规则均提供四种诊断；Unity 示例使用同一份 Assets/ZrConfig/Validation 源码。`Info` 与 `Success` 是实际诊断级别，无需在消息中手写前缀。

Since App 1.3.5, row/field rules provide four levels, using the same Assets/ZrConfig/Validation source as Unity. `Info` and `Success` are real levels; no manual message prefix is needed.

| 方法<br>Method | 用途<br>Purpose | 发布行为<br>Publication |
| --- | --- | --- |
| `context.Info(row, field, message, code)` | 状态说明、数量汇总、无需处理的提示<br>Status, count summaries, and notices requiring no action | 允许发布<br>Allowed |
| `context.Success(row, field, message, code)` | 明确记录某个检查已通过<br>Explicitly record a passed check | 允许发布<br>Allowed |
| `context.Warning(row, field, message, code)` | 可继续导出，但值得确认的配置<br>Configuration worth checking but export may continue | 允许发布，计入警告数<br>Allowed; counted as warning |
| `context.Error(row, field, message, code)` | 必须修正的问题<br>Must-fix problem | 阻止发布，计入错误数<br>Blocked; counted as error |

字段规则 `FieldRuleContext` 使用相同方法名，参数为 `(message, code)`，自动带入当前行和字段。整表汇总可以关联一条真实记录的字段；空表不要访问 `rows[0]`。所有级别都检查位置合法性，并写入运行日志和结构化报告。成功项即使存在其他错误也会保留，不能把单条成功诊断当作整个导出成功。

`FieldRuleContext` has the same method names with `(message, code)`, automatically carrying row/field. Table summaries may attach to a real record's field; do not access rows[0] for empty tables. Every level validates locations and enters logs/structured reports. Successes remain even with other errors; one success is not whole-export success.

实际代码见 [道具、商店、任务示例](../samples/README.md#后置检查与四种日志示例)，包括字段规则、逐行、整表、跨表检查与错误触发方法。`DiagnosticSeverity` 保留已有 Info=0、Warning=1、Error=2，并追加 Success=3；读取 JSON 报告时应按等级含义判断，不能用枚举数值大小判断是否为错误。

See [item/shop/quest examples](../samples/README.md#后置检查与四种日志示例) for field/row/table/cross-table checks and triggers. `DiagnosticSeverity` retains Info=0, Warning=1, Error=2 and adds Success=3. Interpret JSON by level meaning, not numeric ordering.

需要双语消息时用 `ConfigText.Get("中文", "English")`，引入 `ZrConfig.Runtime`。错误代号、字段名、表名保持稳定。

For bilingual messages use `ConfigText.Get("中文", "English")` with `ZrConfig.Runtime`. Keep codes, fields, and table names stable.

## 7. 验证自己写的规则

**7. Verify your rules**

在系统临时目录或仓库之外复制练习数据，避免故意改坏 `samples/Data`。至少执行：

Copy exercise data to system temp or outside the repository, avoiding intentional damage to `samples/Data`. At minimum:

1. 合法数据导出成功，记录 current.json 的内容。<br>
   Export valid data and record current.json.
2. 只改一个条件使其失败，确认期望 Code、Table、Sheet、Row、Column、Field，且 current.json 未改变。<br>
   Break one condition; verify expected Code, Table, Sheet, Row, Column, Field, and unchanged current.json.
3. 修复后再次成功，确认新 versionId 和游戏加载结果。<br>
   Fix and export successfully; check new versionId and game loading.
4. 验证空白与显式 0/false、临界值、重复值、空数组、排序后的数组下标。<br>
   Test blank versus explicit 0/false, boundaries, duplicates, empty arrays, and sorted indexes.
5. 跨表规则增加目标缺失、目标解析失败、重复目标键的用例；本表独立规则仍应按约定运行。<br>
   For cross-table rules test absent targets, target parse failures, and duplicate target keys. Independent local checks should still follow their contract.
6. 警告场景确认 success 为 true；异常场景确认有调用栈且拒绝发布。<br>
   Warnings must keep success true; exceptions must preserve call stacks and reject publication.

不要只检查日志中出现了文字；Warning 也会有文字，已有旧版本也可能仍在磁盘。应同时检查退出码、success、诊断位置与清单是否切换。命令调用方式见 [命令行使用](project-development.md#13-命令行使用)。

Do not merely check that text appeared in a log: warnings also produce text and old releases may remain on disk. Check exit code, success, locations, and manifest switching together. See [CLI usage](project-development.md#13-命令行使用).

## 8. 编译、缓存与调试

**8. Compilation, caches, and debugging**

```bash
# 从仓库根目录执行；需要先生成 Config
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- export samples/Data samples/Output unity/ZrConfig.Sample/Assets/HotUpdate/Config unity/ZrConfig.Sample/Assets/Editor/ConfigRules --rules-debug
```

**示例英文说明 / English notes:**

- Run from the repository root; generate Config first


`--rules-debug` 使动态源码关闭优化、定义 DEBUG，并生成/加载 Portable PDB；工具工程的 Debug 配置不能替代这个参数。CLI 不会自动等调试器，短任务优先由 IDE 直接 Debug 启动。具体操作见 [Rider / Visual Studio 调试指南](debugging.md)。

`--rules-debug` disables dynamic optimization, defines DEBUG, and emits/loads Portable PDB. Tool-project Debug cannot replace it. CLI does not wait for attachment; launch short tasks directly with IDE Debug. See [Rider / Visual Studio](debugging.md).

完整项目编译缓存位于 Output/.rules-cache；生成预览的类型发现使用有容量上限的进程内缓存，不再读取或写入 Config/.types-cache。指纹包含源码内容和绝对路径、模式、语言及工具/共享程序集版本。缓存只保留编译字节与诊断，每次创建新加载上下文与新规则实例，结束后请求卸载；编译失败不回退旧规则。不要在静态事件、后台线程或外部对象中保留动态类型/实例。

Full-project compilation cache lives in Output/.rules-cache. Generation-preview type discovery uses a bounded in-process cache, no longer Config/.types-cache. Fingerprints include content, absolute paths, mode, language, and tool/shared-assembly versions. Cache entries hold only compiled bytes/diagnostics; each run creates a new load context/rule instances and requests unload afterward. Compile failures never fall back to old rules. Do not retain dynamic types/instances through static events, background threads, or external objects.

规则 SDK 是可选开发方式，适合不使用 Unity solution 的项目：先用 `bash scripts/dotnet.sh build src/ZrConfig.App -c Release -p:NuGetAudit=false -p:UsedAvaloniaProducts=` 构建 App，再执行 `bash scripts/package-rules-sdk.sh`；也可显式指定实际工具 DLL 目录与新的输出目录。SDK 默认在 artifacts/rules-sdk，含 Runtime/Validation、Config/Rules 与编译工程，模板 HeroConfig 需先生成或改为实际模型。已有输出目录会被拒绝，不能拿它覆盖手写规则。日常 Unity/Rider/Visual Studio 工作流无需重复导入 SDK DLL。

The optional rules SDK suits projects without Unity solutions. Build App with `bash scripts/dotnet.sh build src/ZrConfig.App -c Release -p:NuGetAudit=false -p:UsedAvaloniaProducts=`, then run `bash scripts/package-rules-sdk.sh`, optionally specifying the actual tool DLL directory and a new output directory. Default artifacts/rules-sdk includes Runtime/Validation, Config/Rules, and compilation projects; generate template HeroConfig first or use your real model. Existing output directories are rejected; never overwrite handwritten rules. Normal Unity/Rider/Visual Studio workflows need no repeated SDK DLL imports.

## 只校验与安全取消

**Validation-only mode and safe cancellation**

业务规则应只读取本轮数据并报告诊断，不写工作簿、项目源码或发布目录。“只校验”的无写入约定针对工具自身，不隔离任意规则代码的系统副作用。取消在逐行、标签及整表规则之间检查；正在执行的自定义规则须返回后才能到达下一安全点。不要在规则中无限等待或自行激活数据版本。

Rules should only read current data and report diagnostics, not write workbooks, project source, or releases. Validation-only's no-write contract covers the tool, not arbitrary rule-code side effects. Cancellation is checked between rows, attributes, and table rules; executing custom rules must return before the next safe point. Never wait indefinitely or activate versions inside rules.
