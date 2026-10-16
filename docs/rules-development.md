# 规则开发与后置检查

**中文** | [English](rules-development.en.md)

**可编程后置检查是 ZrConfig 的核心能力：把原本要到游戏运行时才发现的配表业务错误，提前到导出阶段检查。** 程序员将项目约束写成 C#，在 Excel 已解析、列表已归组、对象引用已关联之后执行，并在发现 Error 时阻止发布。规则面对的是强类型记录，可以访问嵌套成员、判断具体动作子类、遍历整表或查询关联表。

| 检查需求 | 对应能力 | 正式示例 |
| --- | --- | --- |
| 名称、数值等单字段约束 | 内置标签与可复用 `IFieldRule<T>` | 道具与任务共用名称检查 |
| 同一记录中多个字段的关系 | 逐行 `Validate` | 区间顺序、动作延时、Buff 层数 |
| 多条记录之间的条件 | 整表 `ValidateTable` | 重复上架的道具与价格组合 |
| 关联表的业务条件 | `DependsOn` 与后置方法，访问对象或查询生成索引表 | 商品引用道具的启用状态、奖励数量与道具堆叠上限 |

针对嵌套字段报告 `Groups[1].Actions[0].Ratio` 等路径时，诊断会保留排序前的 Excel 单元格位置。规则支持 Info、Success、Warning、Error 四级诊断，只有 Error 阻止发布。只修改规则后直接重新导出，无需重新构建 App；同一份规则源码可用于 App 和 CLI。

**规则也可以调试。** 用 Visual Studio / Rider 启动 CLI 的导出流程并添加 `--rules-debug`，即可在 `Validate`、`ValidateTable` 或自定义字段检查的可执行语句上设置断点，单步观察导入值、嵌套对象、关联记录与调用栈。工具动态编译时关闭优化并加载 Portable PDB，便于把检查逻辑本身也验证清楚。配置步骤见 [Rider](debugging.md#3-rider从-ide-启动调试) 与 [Visual Studio](debugging.md#4-visual-studio从-ide-启动调试)。

本文说明规则放在哪里、什么时候执行、怎样报告 Excel 位置，以及怎样验证规则生效。程序员完整例子见 [项目开发教程第 3～12 章](project-development.md#3-检查规则到底是什么)；Rider、Visual Studio 断点步骤见 [调试指南](debugging.md)；修改工具自身见 [开发指南](development.md)。

本文的正式 Hero/Skill 示例使用 samples 的对象引用字段；开发教程第 12 章使用独立练习的 SkillId、Level 等字段。同名表不代表模型相同，复制规则前先确认对应的 Config，完整项目只选用一套来源。

## 1. 工程与源码归属

仓库全部示例共用 `unity/ZrConfig.Sample/Assets/HotUpdate/Config`、`unity/ZrConfig.Sample/Assets/Editor/ConfigRules`。Unity 通过各自的 asmdef 编译模型与 Editor 规则，App/CLI 在导出时编译同一份源码，无需维护副本或另建 Models/Validators 工程。

仓库 Unity 示例直接使用 Assets/HotUpdate/Config 与 Assets/Editor/ConfigRules：前者编译为 `ZrConfig.Models`，后者通过 `includePlatforms: ["Editor"]` 编译为 `ZrConfig.Sample.Rules.Editor`，不进入 Player。源码可在 Unity 的 Assets 中直接打开、修改和重新生成。具体路径与热更接入边界见 [示例源码与 Editor 检查](../samples/README.md#配置源码与-editor-后置检查)。

自己的 Unity 工程可采用：

```text
Assets/
  HotUpdate/Config/
    CustomTypes/                      手写枚举、类、结构体
    Serialization/              工具提供的支持源码；可选项目转换器
    Generated/                  模型、Codec、唯一的 TableCodecs
  Editor/ConfigRules/            纯 C# 业务检查
```

App 第三个目录选择 Config 根目录；第四个目录选择 ConfigRules。CustomTypes 是示例整理约定，手写类型和转换器可直接放在所选代码根目录或任意子目录中。规则目录放在代码根目录之外，避免依赖生成模型的规则参与生成前的类型编译。规则也可放在 Assets 外，此时由导表工具编译，Unity solution 不一定收录它们。需要 IDE 编译提示时，使用编辑器程序集或独立规则 SDK 工程，不要复制第二份规则。

上述是程序员的源码布局。通用读写支持由工具初始化，普通配表人员只使用项目提供的目录与规则；无需编写序列化支持或完成本篇编程步骤。特殊转换器只有项目确需扩展时才编写。

Config 所属程序集引用 Runtime；规则所属编辑器程序集引用模型程序集、Runtime、Validation。按项目 asmdef 实际配置引用，不要把 Validation 或业务规则带进 Player。不要同时导入旧 Models DLL 和同名生成源码。

导表 App 不携带项目 Models/Validators DLL。每次导出把 Config 下的 `.cs` 与指定规则目录下的 `.cs` 一起编译，扫描非抽象、非开放泛型的 `ITableRules` 实现。通常写 `public sealed class XxxRules : TableRules<XxxConfig>`，保留 public 无参构造函数即可。

- 文件名不负责绑定，`TableRules<T>` 中的 T 决定目标表。
- 同一张表可以有多个规则类；`IFieldRule<T>` 只有被 `[Check]` 绑定才执行。
- 递归扫描子目录，但跳过 bin、obj、点号开头的文件/目录和链接文件/目录。
- 规则目录留空表示关闭业务规则；填写不存在、空目录或没有可用表规则的目录会阻止导出。
- 动态编译不读取项目的 csproj、NuGet 引用或 asmdef，只引用 .NET 基础库、Runtime、Validation。不能直接依赖 UnityEngine、UnityEditor 或游戏程序集。
- App 编译不定义 `UNITY_EDITOR`。不要把整个规则类包在这个条件内；Unity 中通过 Editor 目录/asmdef 隔离它。
- 显式写 `using System`、`System.Collections.Generic`、`System.Linq` 等，保证 Unity 与工具两边都能编译。

只改规则后直接导出。改表结构、主键、CustomTypes 或转换器约定后先生成，再导出。生成与导出是独立操作。

## 2. “后置”指解析后、发布前

```text
编译模型与规则
  → 校对 Schema，解析 Excel，合并多行记录
  → 关联直接表对象引用，确定哪些表完整可用
  → 发现规则、绑定标签（空表也绑定）
  → 执行所有字段标签
  → 每个规则类逐行 Validate，再整表 ValidateTable
  → 无 Error 才写二进制、回读、计算哈希
  → 最后替换 current.json
```

`IsComplete` 表示解析和关联完整，不表示业务规则全部通过。字段标签报错以后，后置方法仍可能执行；不能假设 `[Range]` 已经替方法过滤掉越界行。规则按顺序执行，不依赖多个规则类之间的相对顺序，也不修改配置给其他规则使用。

| 情况 | 实际行为 |
| --- | --- |
| 某行类型解析失败 | 该行不交给字段检查；当前表标为不完整 |
| 当前表不完整 | 已成功解析行仍可接受适用的字段检查；该表后置方法跳过 |
| Reference 的目标表不完整 | `REF_CHECK_SKIPPED`；不把残缺目标表当完整外键集合 |
| DependsOn 表缺失或不完整 | 该规则类的 Validate 和 ValidateTable 都跳过，记录 `BUSINESS_CHECK_SKIPPED` |
| 直接对象引用关联失败 | `TABLE_REFERENCE`；引用链传播不完整状态 |
| 字段标签或其他规则报 Error | 收集其他可独立判断的问题，最后阻止发布 |
| 逐行方法抛异常 | `VALIDATOR_EXCEPTION`，保留该行位置，继续其他行 |
| 整表方法抛异常 | `VALIDATOR_EXCEPTION`，定位表，不自动归属某行 |
| 只有 Info / Success / Warning | 允许发布，仍应阅读警告；Success 仅代表某项检查通过 |

`BUSINESS_CHECK_SKIPPED` 在声明的依赖表根本缺失时是 Error；解析不完整时通常是 Warning，原来的解析 Error 已经阻止发布。不要把“跳过”视作校验成功。

## 3. 选哪一种检查

| 需求 | 实现 |
| --- | --- |
| 顶层字段必填、唯一、范围 | 内置标签 |
| 可复用单字段条件 | `IFieldRule<T>` + `[Check]` |
| 一行多字段、对象、数组内容 | `Validate(row, context)` |
| 整表组合唯一、总和、数量、顺序 | `ValidateTable(rows, context)` |
| 读取另一张表进行业务判断 | `[DependsOn(typeof(TargetConfig))]` + 后置方法 |
| 游戏查询索引、联合键 | Excel 第 3 行“主键”，见 [主键文档](table-indexes.md) |

### 内置标签速查

标签写在规则类上，不写进 Generated。字段名称限顶层逻辑字段，嵌套成员放到后置方法检查。

| 标签 | 语义与边界 |
| --- | --- |
| `Required(field)` | 检查导入时的空白标记，包括未加引号的纯空格；明确填写 0、false、`[]` 不算空白 |
| `Unique(field)` | 非空值按默认相等语义比较，字符串不自动 Trim/忽略大小写；不是数组内容去重 |
| `Positive(field)` | 已填写数值 > 0 |
| `PositiveKey(field)` | Required + Unique + Positive；不建立运行时索引 |
| `Range(field, min, max)` | 已填写数值在闭区间内，边界必须有限且有序 |
| `Reference(field, typeof(Target), targetField)` | 两端属性类型必须一致；目标已填写值中必须存在来源值 |
| `Check(field, typeof(MyRule))` | MyRule 必须能无参实例化，并实现与字段类型一致的 `IFieldRule<T>` |

Positive、PositiveKey、Range 当前只支持普通 `int`、`long`、`float`、`double`。`decimal`、`uint`、`int?` 等使用对应泛型的字段规则或后置判断。

Unique、Positive、Range、Check 跳过空白；要必填另加 Required。Reference 默认拒绝空白；`AllowDefaultValue = true` 允许空白或该类型默认值表示无引用。Reference 只验证存在，不保证目标唯一，不生成对象关联。目标唯一性另行约束。

直接表对象引用 `SkillConfig` 的 0/空白会标记为空引用，所以 `[Required]` 会拒绝它；普通 int 列明确填 0 则能通过 Required。显式填写的字符串字面量、集合内容是否有效仍由业务规则检查。`[ConfigRequired]` 属于手写类型成员的解析约束，与表字段 `[Required]` 不同。

联合键只保证完整组合唯一。不要给允许重复的单个键分量添加 Unique/PositiveKey。

## 4. 逐行与整表例子

以下示例适用于仓库 `TableReferences.xlsx` 生成的 HeroConfig（Id、Name、Skill、OtherSkill、Mentor）。放进项目实际 Rules 目录即可自动发现。这里是演示规则，不要求给正式示例附加新的业务条件。

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

整表方法拿到的是合并多行之后的逻辑记录，行数不等于 Excel 物理行数。空表也会调用 ValidateTable；先判断 Count 再使用 `rows[0]`。当前 ValidationContext 没有无行的整表 Error 重载，不能伪造行来报告“必须至少有一行”；需要这种约束时应扩展诊断接口并补测试。

## 5. 跨表与对象引用

只写 Reference 标签不用另加 DependsOn；业务方法调用 `GetTable<T>()`、生成的 `GetXxxTable()` 或读取目标对象成员时，声明对应依赖。

仓库双向引用例子的附加检查：英雄选的技能必须反向指向这个英雄。空引用在该示例中合法。

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

对象引用支持双向与自引用，所以不要无界递归遍历对象图；遍历长链时保存 visited 集合或设置明确终止条件。

整数外键版本见 [教程跨表完整例子](project-development.md#8-跨表后置检查)。直接对象引用已经完成关联，访问 `row.Skill` 即可；按主键或组合键查找其他记录时，使用下面的生成表接口。

DependsOn 不表示“目标业务规则已经通过”，也不是执行顺序声明。目标行的值仍需检查可用范围。本表规则与跨表规则可拆成不同类，让依赖失效时本表条件继续执行。

### 在后置检查中使用索引表

生成代码后，`ValidationContext` 与运行时快照共用 `GetXxxTable()` 查询入口。例如，在声明了 `[DependsOn(typeof(SkillLevelConfig))]` 的规则方法中：

```csharp
var levels = context.GetSkillLevelTable();
bool found = levels.TryGet(101, 1, 2, out var level);
var group = levels.GetBySkillId(101);
```

首次访问某张表时，工具用本次 Excel 导入的行创建生成表，建立主键与分组索引。同一次导出的所有规则、逐行 `Validate` 和整表 `ValidateTable` 共用这一个实例；后续取表不会重建索引。未访问的表不创建索引表，下次导出创建新上下文，不读取旧发布版本的索引。缓存随本轮上下文结束释放，不应把表或上下文存入静态字段。

索引查询返回原始导入行，可继续调用 `context.Error`、`IsEmpty`，保持原 Excel 单元格定位。空表可以查询；`TryGet` 未命中返回 false，`Get` 未命中抛异常，分组未命中返回空组。当前表或依赖表解析不完整时仍按原规则跳过后置方法；即使未声明依赖，主动取一个缺失或不完整的表也会抛 `TableUnavailableException`，不会查询残缺数据。字段业务 Error 不代表表不完整，索引中的业务值仍需检查。

`context.GetTable<T>()` 返回本次导入的 `ConfigTable<T>`，与生成的强类型入口复用同一个表和索引。生成入口通过 Runtime 的 `IConfigTableProvider` 显式传入 Codec，模型不引用 Validation，Unity Player 无需包含检查程序集。手动构造 `ValidationContext` 时必须提供本次数据的表工厂。

不要在逐行方法中 `new XxxConfigTable(...)`、`CreateIndex` 或 `ToDictionary`，这些操作仍会重复建表。如果检查的键不是已声明主键、需要特殊比较规则，可以在 `ValidateTable` 内建立一次自定义字典，并处理重复键。`ValidateTable` 在本规则类所有 `Validate` 之后执行，不能在其中准备供前面的逐行方法使用的索引。每张表须在 Excel 第 3 行声明主键；`[PositiveKey]` 仅是业务校验，不代替主键声明。

## 6. 报错路径、空白与异常

```csharp
context.Error(row, nameof(AllTypesConfig.Id), "编号不符合业务要求。", "MY_ID");
context.Warning(row, "Settings.MaxTargets", "目标数量较高，请确认。", "TARGET_NOTICE");
context.Error(row, $"Actions[{i}].Delay", "延时不能为负数。", "ACTION_DELAY");
bool omitted = context.IsEmpty(row, "Settings.MaxTargets");
```

上述为方法内片段：row 必须是对应表的真实导入记录，i 是有效数组下标。完整嵌套检查见 [AllTypesRules.cs](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/AllTypesRules.cs)。

| 报告对象 | 第二参数 |
| --- | --- |
| 顶层字段 | `nameof(AllTypesConfig.Id)` |
| 对象成员 | `Settings.MaxTargets` |
| 数组元素成员 | `Actions[1].Delay` |
| 三层多行列表 | `Phases[1].Groups[0].Actions[0].Delay` |

第一参数始终是完整 row，不是 Settings、action 或手动 new 的副本。数组下标使用最终排序后的 0 起始下标；工具映射回原始物理行列。JSON 单格成员定位到承载 JSON 的格子。诊断路径必须确实存在；不要使用输入表头的 `Actions[].Delay` 或沿对象引用写 `Skill.Name`，后者应报告当前行的 Skill，或者用目标行报告其 Name。

`VALIDATOR_LOCATION` 表示 row 不属于本次导入；`VALIDATOR_FIELD` 表示诊断路径找不到。`IsEmpty` 不检查路径拼写，未知路径可能返回 false，所以不能用它来验证字段存在。

四种报告方法的第四参数都是稳定诊断代号，省略为 CUSTOM；字段规则中省略代号为 FIELD_RULE。正常业务不通过使用 Error，不通过 throw 表达。异常用于发现程序缺陷，日志保留完整调用栈和内层异常。

### 日志级别

App 1.3.5 起，行规则和字段规则均提供四种诊断；Unity 示例使用同一份 Assets/ZrConfig/Validation 源码。`Info` 与 `Success` 是实际诊断级别，无需在消息中手写前缀。

| 方法 | 用途 | 发布行为 |
| --- | --- | --- |
| `context.Info(row, field, message, code)` | 状态说明、数量汇总、无需处理的提示 | 允许发布 |
| `context.Success(row, field, message, code)` | 明确记录某个检查已通过 | 允许发布 |
| `context.Warning(row, field, message, code)` | 可继续导出，但值得确认的配置 | 允许发布，计入警告数 |
| `context.Error(row, field, message, code)` | 必须修正的问题 | 阻止发布，计入错误数 |

字段规则 `FieldRuleContext` 使用相同方法名，参数为 `(message, code)`，自动带入当前行和字段。整表汇总可以关联一条真实记录的字段；空表不要访问 `rows[0]`。所有级别都检查位置合法性，并写入运行日志和结构化报告。成功项即使存在其他错误也会保留，不能把单条成功诊断当作整个导出成功。

实际代码见 [道具、商店、任务示例](../samples/README.md#后置检查与四种日志示例)，包括字段规则、逐行、整表、跨表检查与错误触发方法。`DiagnosticSeverity` 保留已有 Info=0、Warning=1、Error=2，并追加 Success=3；读取 JSON 报告时应按等级含义判断，不能用枚举数值大小判断是否为错误。

需要双语消息时用 `ConfigText.Get("中文", "English")`，引入 `ZrConfig.Runtime`。错误代号、字段名、表名保持稳定。

## 7. 验证自己写的规则

在系统临时目录或仓库之外复制练习数据，避免故意改坏 `samples/Data`。至少执行：

1. 合法数据导出成功，记录 current.json 的内容。
2. 只改一个条件使其失败，确认期望 Code、Table、Sheet、Row、Column、Field，且 current.json 未改变。
3. 修复后再次成功，确认新 versionId 和游戏加载结果。
4. 验证空白与显式 0/false、临界值、重复值、空数组、排序后的数组下标。
5. 跨表规则增加目标缺失、目标解析失败、重复目标键的用例；本表独立规则仍应按约定运行。
6. 警告场景确认 success 为 true；异常场景确认有调用栈且拒绝发布。

不要只检查日志中出现了文字；Warning 也会有文字，已有旧版本也可能仍在磁盘。应同时检查退出码、success、诊断位置与清单是否切换。命令调用方式见 [命令行使用](project-development.md#13-命令行使用)。

## 8. 编译、缓存与调试

```bash
# 从仓库根目录执行；需要先生成 Config
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- export samples/Data samples/Output unity/ZrConfig.Sample/Assets/HotUpdate/Config unity/ZrConfig.Sample/Assets/Editor/ConfigRules --rules-debug
```

`--rules-debug` 使动态源码关闭优化、定义 DEBUG，并生成/加载 Portable PDB；工具工程的 Debug 配置不能替代这个参数。CLI 不会自动等调试器，短任务优先由 IDE 直接 Debug 启动。具体操作见 [Rider / Visual Studio 调试指南](debugging.md)。

完整项目编译缓存位于 Output/.rules-cache；生成预览的类型发现使用有容量上限的进程内缓存，不再读取或写入 Config/.types-cache。指纹包含源码内容和绝对路径、模式、语言及工具/共享程序集版本。缓存只保留编译字节与诊断，每次创建新加载上下文与新规则实例，结束后请求卸载；编译失败不回退旧规则。不要在静态事件、后台线程或外部对象中保留动态类型/实例。

规则 SDK 是可选开发方式，适合不使用 Unity solution 的项目：先用 `bash scripts/dotnet.sh build src/ZrConfig.App -c Release -p:NuGetAudit=false -p:UsedAvaloniaProducts=` 构建 App，再执行 `bash scripts/package-rules-sdk.sh`；也可显式指定实际工具 DLL 目录与新的输出目录。SDK 默认在 artifacts/rules-sdk，含 Runtime/Validation、Config/Rules 与编译工程，模板 HeroConfig 需先生成或改为实际模型。已有输出目录会被拒绝，不能拿它覆盖手写规则。日常 Unity/Rider/Visual Studio 工作流无需重复导入 SDK DLL。

## 只校验与安全取消

业务规则应只读取本轮数据并报告诊断，不写工作簿、项目源码或发布目录。“只校验”的无写入约定针对工具自身，不隔离任意规则代码的系统副作用。取消在逐行、标签及整表规则之间检查；正在执行的自定义规则须返回后才能到达下一安全点。不要在规则中无限等待或自行激活数据版本。
