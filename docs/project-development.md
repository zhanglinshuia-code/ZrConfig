# 项目开发教程：业务扩展与 Unity 接入

**Project development: business extensions and Unity integration**

多语言服务端生成与 Binary / JSON / 双格式导出见[多语言生成与 JSON](server-generation.md)。

See [multi-language generation and JSON](server-generation.md) for server generation and Binary/JSON/dual export.
本文面向项目程序员，完整保留业务类型、转换器、后置检查、命令行与 Unity 接入的实现示例。日常配表人员只需阅读 [使用文档](user-guide.md)；本教程不是配表前置步骤。源码启动与安装包构建见 [源码构建与升级](development.md)。

This developer tutorial retains complete examples for types, converters, post-validation, CLI, and Unity. Everyday authors need only the [user guide](user-guide.md); this is not a prerequisite. See [building/upgrading](development.md) for source startup/installers.

## 开发职责与现有实现

**Responsibilities and existing implementation**

| 内容<br>Content | 谁负责<br>Owner | 何时需要<br>When needed |
| --- | --- | --- |
| 表的行模型、Codec、注册表<br>Row models, codecs, registry | 工具生成，程序员负责接入编译<br>Tool generates; developers integrate/build | 首次接入或表结构变化<br>Initial integration/schema changes |
| 业务枚举、类、结构体<br>Business enums/classes/structs | 项目程序员<br>Developers | 内置类型不能表达项目数据形状时<br>Built-ins cannot express project shapes |
| 通用序列化支持源码<br>Generic support source | 工具提供，版本升级由工具/项目维护者处理<br>Tool supplies; tool/project maintainers upgrade | 首次生成补齐；已记录且未修改的版本自动升级<br>Initial generation fills files; recorded unchanged versions auto-upgrade |
| 特殊转换器<br>Special converters | 项目程序员按需编写<br>Developers as needed | 项目确有特殊文本或二进制约定时<br>Actual special text/binary conventions |
| 业务规则<br>Business rules | 项目程序员维护，工具自动执行<br>Developers maintain; tool executes | 需要额外业务约束时<br>Extra business constraints |
| Excel 数据、错误修正、导出验收<br>Excel, fixes, export acceptance | 配表人员<br>Authors | 日常操作<br>Everyday work |

示例代码根目录按用途分为 CustomTypes、Serialization、Generated：CustomTypes 保存手写的项目业务定义；Serialization 同时容纳工具提供的支持文件和可选项目转换器；Generated 由生成器更新。手写源码可放在用户选择的代码根目录或任意命名的子目录，工具递归发现，不要求 CustomTypes 这个名称。生成时排除工具的 Generated 输出；bin、obj、点号开头的文件/目录和链接文件/目录不参与编译。文件位于项目内，不代表配表人员需要编写读写代码。

Config uses CustomTypes for handwritten definitions, Serialization for managed support/optional converters, Generated for generator output. Handwritten source can be at the selected root or any subdirectory; CustomTypes is not required. Generation excludes its Generated output; bin/obj/dot-prefixed entries/links are not compiled. Being project-local does not mean authors must write serialization.

通用支持代码仍由工具管理在 Config/Serialization。生成时补齐缺失文件，并自动升级与已记录版本一致、未经手写修改的文件；手写修改和无法识别的旧版保留并提示合并。导出会检查版本，处理步骤见 [支持源码升级](development.md#支持源码升级)。

Generic support remains managed in Config/Serialization. Generation supplies missing files/upgrades recorded unchanged ones; handmade/unrecognized old versions are preserved with merge notices. Export checks versions; see [upgrades](development.md#支持源码升级).

## 阅读路线

**Reading route**

| 工作<br>Work | 章节<br>Chapters |
| --- | --- |
| 添加业务数据类型<br>Business types | [1. 手写类](#1-手写类是什么)、[2. 枚举与多态](#2-枚举结构体和多态是什么)<br>[1. Classes](#1-手写类是什么), [2. Enums/polymorphism](#2-枚举结构体和多态是什么) |
| 编写后置检查<br>Post-validation | [3. 检查机制](#3-检查规则到底是什么) 至 [9. 对象与数组](#9-检查对象成员和数组元素)<br>[3. Checks](#3-检查规则到底是什么) through [9. Objects/arrays](#9-检查对象成员和数组元素) |
| 配置对象引用<br>Object references | [10. 直接表引用](#10-直接表对象引用少写一次手动查找)<br>[10. Direct references](#10-直接表对象引用少写一次手动查找) |
| 自定义输入/编码格式<br>Custom input/encoding | [11. 转换器](#11-特殊格式怎么扩展转换器)<br>[11. Converters](#11-特殊格式怎么扩展转换器) |
| 从零完成一套代码<br>Complete implementation | [12. 完整开发练习](#12-完整练习项目)<br>[12. Full exercise](#12-完整练习项目) |
| 自动化导出<br>Automated export | [13. 命令行](#13-命令行使用)<br>[13. CLI](#13-命令行使用) |
| Unity 加载和热更新<br>Unity loading/updates | [14. Unity 接入](#14-unity-运行时如何使用)<br>[14. Unity](#14-unity-运行时如何使用) |
| Rider / Visual Studio 调试<br>Rider/Visual Studio | [调试指南](debugging.md)<br>[Debugging](debugging.md) |

第 12 章的 Hero/Skill 练习使用 SkillId、Level 等教学字段，与正式 TableReferences.xlsx 的 Skill、Mentor 对象引用字段不同。练习包解压到仓库之外，不能与 samples/Data 混用。仓库正式示例说明统一在 [samples/README.md](../samples/README.md)。

Chapter 12 Hero/Skill uses teaching fields SkillId/Level, unlike official TableReferences Skill/Mentor object references. Extract exercises outside the repository; never mix with samples/Data. Official instructions live in [samples/README.md](../samples/README.md).

本教程的 Excel 示例统一使用五行表头：字段名、类型、主键、说明、所属端，第 6 行起填写数据。第 3 行至少标记一列“主键”，其余格子留空；完整写法见 [使用文档](user-guide.md#41-统一使用五行表头)。

All examples use five headers: field/type/key/description/side, data from row 6. Mark at least one row-3 column with literal 主键, leaving others blank; see [headers](user-guide.md#41-统一使用五行表头).

## 1. 手写类是什么

**1. What is a handwritten class?**

### 1.1 类就是“描述一组数据的模板”

**1.1 A class describes a group of data**

一个英雄不只有一个整数，还可能有一组战斗设置：最多攻击几个目标、是否允许被打断。

A hero may have battle settings beyond one integer: maximum targets and whether interruption is allowed.

你把这两个值放在一起，它们就组成一个“战斗设置对象”。

Together those values form a battle-settings object.

**类**：规定这组对象里有哪些成员。可以理解成一张空白登记卡。

**Class:** Defines its members, like a blank registration card.

**对象**：按这张登记卡填出来的某一份实际内容。

**Object:** One filled instance of that card.

**成员**：卡片里的每一项，例如 MaxTargets、Interruptible。

**Member:** An entry such as MaxTargets or Interruptible.

**手写类**：这张模板由你写 C# 来定义。工具不会猜你的游戏需要哪些战斗成员。

**Handwritten class:** You define the template in C#; the tool cannot infer your game's members.

**生成类**：Excel 表的整行模型，例如 HeroConfig，由工具生成。

**Generated class:** The tool creates a whole-row model such as HeroConfig from Excel.

| 名字<br>Name | 谁写<br>Author | 放哪里<br>Location | 表示什么<br>Meaning |
| --- | --- | --- | --- |
| BattleSettings | 你写<br>You | Config/CustomTypes | 英雄里面的一组战斗设置<br>Battle settings inside a hero |
| HeroConfig | 工具生成<br>Tool | Config/Generated | Excel 中一条完整英雄配置<br>One complete Excel hero record |
| HeroRules | 你写<br>You | Rules | 如何检查英雄配置<br>Hero validation logic |
| HeroConfigCodec | 工具生成<br>Tool | Config/Generated | 如何读写 Hero 二进制<br>Hero binary reading/writing |

同一张表的记录、查询表、扩展访问入口和 Codec 都位于一个 `Generated/HeroConfig.g.cs` 文件中，仍是职责独立的类型；所有表共用一份 `TableCodecs.g.cs`。文件布局及升级说明见 [生成文件组织](development.md#生成文件组织)。

Record/query/extensions/codec share Generated/HeroConfig.g.cs but remain separate types. All tables share TableCodecs.g.cs. See [organization/upgrades](development.md#生成文件组织).

不要在 CustomTypes 里再手写一个 HeroConfig，会和工具生成的类重名。

Do not handwrite another HeroConfig in CustomTypes; it would collide.

### 1.2 第一个手写类

**1.2 Your first class**

创建文件：`Config/CustomTypes/BattleSettings.cs`。

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

逐句解释：

Line-by-line explanation:

| 代码<br>Code | 意思<br>Meaning |
| --- | --- |
| namespace Tutorial.Config | 给这些类型放进一个“名字分类盒子”，避免重名<br>Groups names to avoid collisions |
| public | 外面的代码也能看见这个类型或成员<br>Visible to other code |
| sealed class BattleSettings | 定义一个叫 BattleSettings 的类；这里不需要再被继承<br>Defines BattleSettings without further inheritance |
| public int MaxTargets = 1 | 一个整数成员，默认值为 1<br>Integer member defaulting to 1 |
| public bool Interruptible = true | 一个开关成员，默认允许打断<br>Boolean switch defaulting to interruptible |
| `{` 和 `}`<br>`{` and `}` | 这一段内容的开始和结束<br>Start/end of a block |
| `;` | 一条声明结束<br>Ends a declaration |

文件名以 `.cs` 结尾。用 Rider、VS Code 等代码编辑器保存为文本文件，不能把它保存成 `BattleSettings.cs.txt`。

Save as text ending .cs in Rider/VS Code/etc., not BattleSettings.cs.txt.

`public int MaxTargets` 是字段。也可以使用公开可读写属性：

public int MaxTargets is a field. A publicly readable/writable property also works:

```csharp
public int MaxTargets { get; set; } = 1;
```

两种形式选一种就可以，不要在同一个类中同时声明两个同名成员。

Choose one form; do not declare duplicate member names in one class.

### 1.3 Excel 怎么使用这个类：成员分列

**1.3 Use the class in Excel: split members**

在 Hero 表新增两列：

Add two Hero columns:

| Excel 行号<br>Excel row | Id | Settings.MaxTargets | Settings.Interruptible |
| --- | --- | --- | --- |
| 1 | Id | Settings.MaxTargets | Settings.Interruptible |
| 2 | int | Tutorial.Config.BattleSettings | 留空<br>Blank |
| 3 | 主键<br>`主键` (literal marker) |  |  |
| 4 | 英雄编号<br>Hero identifier | 最大目标数<br>Maximum targets | 能否打断<br>Interruptible |
| 5：所属端<br>5: Side | CS | CS | CS |
| 6 | 1001 | 3 | false |

表格里的“留空”是说明，实际格子必须为空，不要输入这两个字。

“Blank” is explanatory: leave the actual cell empty, do not type that word.

`Settings.MaxTargets` 表示“Settings 对象里面的 MaxTargets”。生成模型后，代码用 `row.Settings.MaxTargets` 访问。

Settings.MaxTargets means MaxTargets inside Settings; generated models expose row.Settings.MaxTargets.

**关键：第二行填写这一组的根类型 BattleSettings，不是在两列分别填 int、bool。** 成员类型已经由 C# 写清楚了。组内一列写根类型，其余类型格留空；也可以每列重复相同根类型。

**Row 2 declares the group's root BattleSettings, not int/bool separately.** C# already defines member types. Fill one group type cell and leave others blank, or repeat the same root type.

新增或修改类后：保存 `.cs` → 保存 Excel → 点击“生成配表代码” → 点击“校验并导出”。

After type edits: save .cs → save Excel → Generate configuration code → Validate and export.

### 1.4 另一种填法：一个格子放整个对象

**1.4 Alternative: whole object in one cell**

如果不想分列，可以把整个对象放在 `Settings` 一列，Id 仍作为本例的主键：

Put the whole object in Settings if preferred, keeping Id as this example's key:

| 行<br>Row | Id | Settings |
| --- | --- | --- |
| 1 | Id | Settings |
| 2 | int | Tutorial.Config.BattleSettings |
| 3 | 主键<br>`主键` (literal marker) |  |
| 4 | 英雄编号<br>Hero identifier | 战斗设置<br>Battle settings |
| 5：所属端<br>5: Side | CS | CS |
| 6 | 1001 | {"MaxTargets":3,"Interruptible":false} |

这是 JSON 写法。可以显式写类型 `json<Tutorial.Config.BattleSettings>`。

This is JSON; explicit `json<Tutorial.Config.BattleSettings>` also works.

同一个字段不能既有 `Settings` 整格列，又有 `Settings.MaxTargets` 展开列。选一种填写方式。

Do not combine whole-cell Settings and expanded Settings.MaxTargets for the same field. Choose one layout.

初学者编辑复杂战斗数据，建议使用分列，能直接看到每个值。

Split columns make each value visible for beginners editing complex battle data.

### 1.5 什么是默认值

**1.5 Defaults**

`MaxTargets = 1` 表示默认最多 1 个目标。

MaxTargets = 1 defaults to at most one target.

JSON 写 `{"Interruptible":false}` 时，没有提供 MaxTargets，它会保留默认值 1。分列时某个可选成员格子留空，也会保留其构造默认值。

JSON {"Interruptible":false} omits MaxTargets, retaining 1. Blank optional split members likewise retain construction defaults.

已经明确填写的 `0` 或 `false` 会覆盖默认值，不会被当成空白。

Explicit 0/false overrides defaults and is not blank.

默认值只是“没有填时用什么”，不是业务检查。即使有默认值，如果业务要求这个成员必须显式填写，也要加必填声明。

Defaults specify missing-value behavior, not business validity. Add required declarations when explicit input is mandatory, even with defaults.

### 1.6 对象成员必须显式填写：ConfigRequired

**1.6 Explicit object members: ConfigRequired**

把类改成：

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

`[ConfigRequired]` 要放在**手写类型的字段或属性上**。即使默认值是 1，Excel 或 JSON 没有明确提供 MaxTargets，也会被拒绝。

Put ConfigRequired on **handwritten fields/properties**. Even with default 1, omitted MaxTargets in Excel/JSON is rejected.

它检查“有没有明确填写”，不检查“值是否合理”。填 0 算填写了；要让 MaxTargets 大于 0，还需要第 9 章的业务检查。

It checks explicit input, not validity. 0 is filled; positivity needs chapter 9's business checks.

这是示范替换写法。第 12 章完整练习中的 BattleSettings 使用可选默认值版本，不要把两份类同时放进 CustomTypes。

This replaces the earlier example. Chapter 12 uses optional defaults; never include both class versions together.

### 1.7 手写类要遵守的要求

**1.7 Class requirements**

- 类和需要导表的成员要 public。<br>
  Types/exported members must be public.
- 类要能用公开的无参数构造函数创建；没有自己写构造函数时，普通 public 类通常已有默认构造函数。<br>
  Classes need public parameterless construction; ordinary public classes usually have it unless constructors are declared.
- 字段要可以写入；属性要有 public get 和 public set。<br>
  Fields must be writable; properties need public get/set.
- 不使用只读属性、只写属性、索引器、init 属性或泛型数据类。<br>
  No read-only/write-only properties, indexers, init properties, or generic data classes.
- 不在构造函数或 setter 中播放音效、生成角色、访问游戏场景。<br>
  Constructors/setters must not play sounds, spawn characters, or access scenes.
- 这些类型是纯数据定义，不引用 UnityEngine、UnityEditor 或游戏业务程序集。<br>
  Pure data definitions reference neither UnityEngine/UnityEditor nor game assemblies.
- 继承成员可以参与导表，但不要用同名新成员隐藏父类成员。<br>
  Inherited members can export, but do not hide base members with same-name declarations.

业务行为由游戏代码处理。导表的“伤害动作”保存 Delay、Ratio 等参数，不在导表时真正对敌人扣血。

Game code implements behavior. An exported damage action stores Delay/Ratio; export does not damage enemies.

## 2. 枚举、结构体和多态是什么

**2. Enums, structs, and polymorphism**

### 2.1 枚举：只能从几个名字中选一个

**2.1 Enums: choose a declared name**

创建 `Config/CustomTypes/Quality.cs`：

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

它相当于固定选项：无、稀有、传说。

These are fixed options: None, Rare, Legendary.

Excel 类型填 `Tutorial.Config.Quality`，数据填 `Rare`，不是任意中文，也不是随便填数字 1。

Excel type is Tutorial.Config.Quality; enter Rare, not arbitrary Chinese text or numeric 1.

大小写按声明填写。`rare` 不能当作教程里的 `Rare` 使用。没有声明的名字会报错。

Match declared case: rare is not Rare. Unknown names fail.

若允许空白枚举，需要存在取值为 0 的成员，例如 None。枚举数组类型为 `Tutorial.Config.Quality[]`，填写 `[Rare;Legendary]`。

Blank enums require a zero-valued member such as None. Arrays use Tutorial.Config.Quality[] with [Rare;Legendary].

增加、删除或修改枚举成员后，重新生成代码并导出。

Regenerate/export after adding/removing/changing enum members.

### 2.2 结构体：也可以把几个值放一起

**2.2 Structs also group values**

创建 `Config/CustomTypes/NumberRange.cs`：

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

它表示一段数字范围，例如最小 1、最大 10。

It represents a range, e.g. minimum 1, maximum 10.

Excel 分列写 `Range.Min`、`Range.Max`，第二行组类型写 `Tutorial.Config.NumberRange`。

Split Range.Min/Range.Max, with group type Tutorial.Config.NumberRange on row 2.

class 与 struct 的 C# 内存行为不同，但配表时你先把它们都理解为“有几个成员的一组数据”。普通复杂对象用 class，简单小值可以用 struct。

C# class/struct memory behavior differs; for authoring both group members. Use classes for ordinary complex objects, structs for small values.

工具不会自动知道 Min 不能大于 Max，需要后置检查：

The tool cannot infer Min ≤ Max; add post-validation:

```csharp
if (row.Range.Min > row.Range.Max)
    context.Error(row, "Range.Max", "最大值不能小于最小值。", "RANGE_ORDER");
```

**示例英文说明 / English notes:**

- Maximum must not be less than minimum.


这个片段放在对应表规则的 Validate 方法里，并且那张表确实要有 Range 字段。

Put this in the corresponding table's Validate; that model must actually have Range.

### 2.3 多态：同一动作栏，允许不同种类的动作

**2.3 Polymorphism: different action kinds in one field**

“打伤害”和“加 Buff”都属于动作，但它们需要的参数不同。

Damage and AddBuff are actions with different parameters.

创建 `Config/CustomTypes/BattleAction.cs`：

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

- BattleAction 是公共的动作模板，Delay 是所有动作都有的延时。<br>
  BattleAction is the shared template; all actions have Delay.
- Damage 是伤害动作，有 Ratio、CanCrit。<br>
  Damage has Ratio and CanCrit.
- AddBuff 是加 Buff 动作，有 BuffId、Stacks。<br>
  AddBuff has BuffId and Stacks.
- `: BattleAction` 表示“这是 BattleAction 的一种”。<br>
  `: BattleAction` means a subtype of BattleAction.
- abstract 表示基础模板本身不直接拿来创建实际动作，要选具体种类。<br>
  abstract prevents instantiating the base directly; choose a concrete kind.

单格 JSON 数组可以这样填：

A single-cell JSON array can be:

```json
[{"$type":"Damage","Delay":0.2,"Ratio":1.5,"CanCrit":true},{"$type":"AddBuff","Delay":0.5,"BuffId":2001,"Stacks":2}]
```

该列类型为 `json<Tutorial.Config.BattleAction[]>`。

Column type: `json<Tutorial.Config.BattleAction[]>`.

`$type` 是告诉工具“这次是哪一种动作”。必须写实际子类名 Damage 或 AddBuff，不写抽象父类 BattleAction。类名冲突时写完整命名空间。

$type identifies the concrete kind Damage/AddBuff, not abstract BattleAction. Use fully qualified names on collisions.

没有填写 AddBuff 的 BuffId 会报错，因为它带 ConfigRequired。明确填 0 算填写了；0 是否允许由业务检查决定。

Missing AddBuff.BuffId fails ConfigRequired. Explicit 0 counts as input; business rules decide validity.

新增 `Heal : BattleAction` 时，先写类，再生成代码，然后就可以填写 Heal，不需要给 App 增加一个手工注册表。

To add Heal : BattleAction, write the class and regenerate. Heal then works without a manual App registry.

### 2.4 把动作数组展开成多行

**2.4 Expand an action array into rows**

下面先列出五行表头，第 3 行在 Id 列标记“主键”。实际工作簿中从 A1 开始连续填写，不要把说明行号作为数据列。

Below are five headers, with 主键 under Id on row 3. Fill continuously from A1; explanatory row numbers are not data columns.

| 行<br>Row | Id | Name | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Id | Name | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
| 2 | int | string | Tutorial.Config.BattleAction[] | | | | | | |
| 3 | 主键<br>`主键` (literal marker) |  |  |  |  |  |  |  |  |
| 4 | 英雄编号<br>Hero identifier | 名称<br>Name | 动作顺序<br>Action order | 动作种类<br>Action kind | 延时<br>Delay | 倍率<br>Ratio | 能否暴击<br>Critical hits? | Buff 编号<br>Buff identifier | 层数<br>Stacks |
| 5：所属端<br>5: Side | CS | CS | CS | CS | CS | CS | CS | CS | CS |

接着第 6 行起填：

Enter data from row 6:

| Id | Name | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1001 | 小剑士<br>Young swordsman | 1 | Damage | 0.2 | 1.5 | true |  |  |
| 1001 | | 2 | AddBuff | 0.5 | | | 2001 | 2 |
| 1002 | 小法师<br>Young mage | 1 | Damage | 0 | 2 | false |  |  |

这三行数据最终得到 **两条英雄配置**。第一条有两个动作，第二条有一个动作。

These three physical rows yield **two hero records**: first with two actions, second with one.

规则：

Rules:

1. 只有出现 `[]` 展开列时，同一完整主键的多行才会合并；本例以 Id 为主键。普通表的一行仍是一条记录。<br>
   Only expanded [] columns merge equal complete keys; this example uses Id. Ordinary tables remain one record/row.
2. 首行必须填写有效完整键。续行可以重复完整键，也可以将所有键分量留空表示接着上一条有效记录；联合键不能只填一部分。初学者建议明确重复完整键。<br>
   First row requires a valid complete key. Continuations repeat all components or leave all blank for the preceding valid record; no partial composites. Beginners should repeat complete keys explicitly.
3. Name、Level 等主字段放在该 Id 的首行。续行留空，或者完全重复首行内容，不能填成另一个值。<br>
   Put Name/Level/main fields on the first row for Id. Continue blank or identical, never conflicting.
4. `$order` 是排序用的辅助列，不会变成模型成员。上例里的顺序 1、2 表示第一个、第二个动作。<br>
   $order sorts input and is not generated. 1/2 mean first/second action.
5. 声明了 `$order`，每个有效元素就要填写唯一的正整数。对于不含子列表的简单数组，也可以不声明 order，按实际行顺序排列。<br>
   If declared, every valid element needs a unique positive order. Simple arrays without children may omit it and use physical order.
6. Damage 只填 Damage 的参数；它的 BuffId、Stacks 列要留空。AddBuff 同理。<br>
   Damage fills only its parameters, leaving BuffId/Stacks blank; AddBuff follows the same rule.
7. 所有动作展开列都留空，表示这一行没有动作元素。一个英雄没有任何动作元素，最终是空数组。<br>
   All action columns blank means no element on that row. A hero with no elements gets an empty array.
8. 不要合并数据格子，不要只排序局部几列，否则 Id 与参数可能错位。<br>
   Never merge data cells or sort only some columns, which can misalign Id/parameters.

动作下标从 0 开始：第一个动作是 Actions[0]，第二个是 Actions[1]。错误定位代码会用这个下标。

Indexes are zero-based: Actions[0], Actions[1]; diagnostics use these indexes.

### 2.5 多层列表：动作组里再放动作

**2.5 Nested lists: groups containing actions**

例如：英雄有多个动作组，每个动作组又有多个动作。

A hero may contain multiple groups, each with multiple actions.

手写类型：

Handwritten type:

```csharp
using System;

namespace Tutorial.Config
{
    public sealed class ActionGroup
    {
        public string Name = "默认动作组";
        public BattleAction[] Actions = Array.Empty<BattleAction>();
    }
}
```

**示例英文说明 / English notes:**

- The default Name literal means “Default action group.”


第一行列名可以是：

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

第 2 行的 Id 类型填 int，Groups 组内一列填 `Tutorial.Config.ActionGroup[]`，其余组类型格留空。第 3 行只在 Id 列标记“主键”，第 4 行填各列说明，第 5 行保留所属端。以下数据从第 6 行开始：

Row 2: int for Id, Tutorial.Config.ActionGroup[] in one Groups type cell, others blank. Row 3 marks only Id; row 4 descriptions, row 5 sides. Data below starts at row 6:

| Id | Groups[].$order | Groups[].Name | Groups[].Actions[].$order | Groups[].Actions[].$type | Groups[].Actions[].Ratio | Groups[].Actions[].BuffId |
| --- | --- | --- | --- | --- | --- | --- |
| 1001 | 1 | 第一组<br>First group | 1 | Damage | 1.5 |  |
| 1001 | 1 | | 2 | AddBuff | | 2001 |
| 1001 | 2 | 第二组<br>Second group | 1 | Damage | 2 |  |

- 父 order 都是 1 的前两行，属于同一个动作组。<br>
  First two rows have parent order 1 and share a group.
- 第三条数据的父 order 是 2，属于第二个动作组。<br>
  Third row's parent order 2 selects the second group.
- 子 order 在每个父组内重新计算，第二组也可以有子 order 1。<br>
  Child orders restart per parent; the second group may also have child 1.
- 含子列表的父层必须声明并填写 order，工具用它判断哪些行属于同一个父对象。<br>
  Parents containing child lists must declare/fill order to identify shared parent objects.

三层同理，例如 `Phases[].Groups[].Actions[]`。先掌握一层，再使用嵌套。仓库 AllTypes 中的 Groups、Phases 有实际例子。

Three levels follow the same rule, e.g. Phases[].Groups[].Actions[]. Learn one level first; AllTypes Groups/Phases show real examples.

## 3. 检查规则到底是什么

**3. What are validation rules?**

检查规则就是“允许导出的条件”。例如：

Rules define conditions for export, for example:

- 英雄名称必须填。<br>
  Hero name required.
- 编号不能重复。<br>
  Identifiers unique.
- 等级只能是 1～100。<br>
  Level from 1 to 100.
- 高等级英雄必须有技能。<br>
  High-level heroes need skills.
- 所用技能的伤害必须够高。<br>
  Selected skills need sufficient damage.
- 动作延时不能小于 0。<br>
  Action delay nonnegative.

工具自带的基础解析会检查“这一格能不能变成声明的类型”。比如 int 列填“苹果”会失败。但“英雄等级不能超过 100”是你的游戏约定，需要规则表达。

Basic parsing checks conversion to declared types, so “apple” in int fails. A level cap of 100 is game-specific and needs a rule.

### 3.1 三种检查方法

**3.1 Three checking methods**

| 方法<br>Method | 检查什么<br>Checks | 写在哪里<br>Placement |
| --- | --- | --- |
| 内置字段标签<br>Built-in field attributes | 必填、唯一、范围、外键存在等<br>Required, unique, range, foreign-key existence | 手写表规则类上方<br>Above handwritten table rule class |
| 自定义字段规则 `IFieldRule<T>`<br>Custom `IFieldRule<T>` | 一个字段的可复用检查<br>Reusable single-field condition | 单独规则类，再用 Check 绑定到表规则<br>Separate rule class, bound via Check |
| 后置检查 Validate / ValidateTable<br>Validate / ValidateTable | 多字段、整表、跨表、自定义对象与数组业务<br>Multiple fields, tables, references, object/array business logic | 手写表规则类的方法中<br>Table-rule methods |

“后置”表示把 Excel 解析成配置对象以后，再做业务检查。它仍发生在发布新版本之前，不是数据发布后才检查。

“Post” means after Excel becomes objects, still before publishing, never after release.

执行顺序是：

Execution order:

```text
读取表结构和数据 / Read schema/data
  → 构造配置对象、关联直接表对象引用 / → Construct objects and link direct references
  → 所有规则类的字段标签检查 / → Field attributes across rule classes
  → 每个规则类的 Validate：逐条记录检查 / → Each class's Validate for every record
  → 该规则类的 ValidateTable：整张表检查 / → That class's ValidateTable
  → 没有错误才写入、回读并激活新版本 / → Only without errors: write/read-back/activate
```

不要依赖多个规则类之间的执行顺序，也不要让某个规则修改数据给另一个规则使用。规则的职责是检查并报告。

Do not depend on inter-class ordering or mutate data for another rule. Check/report only.

### 3.2 表规则的空白模板

**3.2 Empty table-rule template**

假设已经生成了 HeroConfig，创建 `Rules/HeroRules.cs`：

With HeroConfig generated, create Rules/HeroRules.cs:

```csharp
using ZrConfig.Models;
using ZrConfig.Validation;

public sealed class HeroRules : TableRules<HeroConfig>
{
}
```

`TableRules<HeroConfig>` 意味着“这个检查专门检查 Hero 表”。尖括号中的类型决定绑定哪张表，不是文件名决定。

`TableRules<HeroConfig>` binds to Hero by generic type, not filename.

类必须可被实例化，最容易的写法是 public sealed class，不写带参数构造函数。

The class must be instantiable; public sealed class without parameterized constructors is simplest.

工具自动发现有效表规则，不需要手写一个中央注册列表。

Rules are auto-discovered; no central handwritten list.

一张表可以有多个规则类。例如 HeroRules 和 HeroTutorialRules 都继承 `TableRules<HeroConfig>`，它们都会执行。所有被扫描到的规则类都会生效，不要复制旧规则文件后忘记删除或重命名类。

Multiple classes may target HeroConfig and all execute. Do not copy old files and accidentally keep duplicate active classes.

### 3.3 标签要放在规则类上方

**3.3 Attributes go above rule classes**

正确：

Correct:

```csharp
using ZrConfig.Models;
using ZrConfig.Validation;

[Required(nameof(HeroConfig.Name))]
public sealed class HeroRules : TableRules<HeroConfig>
{
}
```

不要把 Required 写到 HeroConfig.g.cs 里。生成代码是工具维护的，重生成会改掉它。

Never put Required in HeroConfig.g.cs; regeneration replaces managed code.

`nameof(HeroConfig.Name)` 的结果就是字符串 `"Name"`。这种写法让拼错成员名时编译器提前提醒你。

nameof(HeroConfig.Name) evaluates to "Name" and catches misspellings at compile time.

`typeof(SkillConfig)` 表示把“SkillConfig 这个类型”传给规则。

typeof(SkillConfig) passes the SkillConfig type to a rule.

## 4. 内置字段规则

**4. Built-in field rules**

本章片段是“加到现有规则类上方的标签”，不是让你每看一个片段就新建一份同名 HeroRules。

These are attributes added above existing classes, not instructions to create repeated HeroRules classes.

### 4.1 Required：必须填写

**4.1 Required: explicit input**

```csharp
[Required(nameof(HeroConfig.Name))]
[Required(nameof(HeroConfig.Level))]
```

| 情况<br>Input | Required 的结论<br>Required result |
| --- | --- |
| Name 格子空白<br>Blank Name | 错误<br>Error |
| Level 格子空白<br>Blank Level | 错误<br>Error |
| Level 明确填 0<br>Explicit Level 0 | 已填写，Required 本身通过<br>Filled; Required passes |
| bool 明确填 false<br>Explicit bool false | 已填写，Required 本身通过<br>Filled; Required passes |
| 数组明确写 []<br>Explicit [] | 已填写；数组至少一个元素要另外检查<br>Filled; nonempty-array requirements need another check |

Required 按导入时的空白标记检查，未加引号的纯空格也算空白。它不等于正数或非空数组；显式字符串字面量、字符串长度和首尾空格等内容约束仍应另写规则。可空值的显式 null 与直接表对象引用的 0 都表示无值，会被 Required 拒绝。

Required uses original blank markers, including unquoted whitespace. It means neither positive nor nonempty array. Check explicit string literals/length/surrounding spaces separately. Nullable null and direct reference 0 mean absent and fail.

### 4.2 Unique：不允许重复

**4.2 Unique: no duplicates**

```csharp
[Unique(nameof(HeroConfig.Name))]
```

同一张表出现两个相同值会报错。单独 Unique 不要求填值，空白会跳过。需要必填且唯一，两个标签一起加。

Equal values across rows fail. Unique alone skips blanks; combine Required for mandatory unique input.

普通字符串 Unique 按原值比较，不能把它理解为自动忽略大小写、去首尾空格。自定义的“忽略大小写”或“两个字段组合唯一”，使用 ValidateTable。

Strings compare original values without trimming/case folding. Use ValidateTable for case-insensitive or composite uniqueness.

多行数组表会先合并同一 Id 的物理行，再检查最终配置记录；因此属于一个英雄的两行动作，不会被当成两个英雄编号重复。

Multi-row records merge first, so two actions of one hero are not duplicate hero IDs.

Unique 检查的是不同记录的字段值，不是检查一个数组内部的元素是否重复。数组、对象的内容唯一等复杂条件，需要自己的后置规则。

Unique compares fields across records, not elements inside one array. Complex object/array-content uniqueness needs custom post-checks.

### 4.3 Positive：必须大于 0

**4.3 Positive: greater than zero**

```csharp
[Positive(nameof(HeroConfig.Level))]
```

填写 1 通过，填写 0 或 -1 报错。单独 Positive 会跳过空白；需要必填就再加 Required。

1 passes, 0/-1 fail. Positive skips blanks; add Required when needed.

不要用 Positive 表达“允许 0 但不能小于 0”，那要用 Range 或自定义检查。

For “zero allowed, negative forbidden,” use Range/custom checks, not Positive.

### 4.4 PositiveKey：必填、唯一、大于 0 一起检查

**4.4 PositiveKey: required, unique, positive**

```csharp
[PositiveKey(nameof(HeroConfig.Id))]
```

相当于：

Equivalent to:

```csharp
[Required(nameof(HeroConfig.Id))]
[Unique(nameof(HeroConfig.Id))]
[Positive(nameof(HeroConfig.Id))]
```

适合常见的正整数配置编号。该校验标签不自动创建运行时查询索引；在 Excel 的第 3 行标记“主键”后，由生成的表在加载时建立索引。联合主键的单个字段允许重复，不能再给该字段添加 Unique 或 PositiveKey；只检查正数可用 Positive。

Useful for positive integer IDs. It creates no runtime index; Excel row-3 主键 causes generated tables to index on load. Repeating composite components must not get Unique/PositiveKey; use Positive alone for positivity.

### 4.5 Range：限制上下界，包含两头

**4.5 Range: inclusive lower/upper bounds**

```csharp
[Required(nameof(HeroConfig.Level))]
[Range(nameof(HeroConfig.Level), 1, 100)]
```

填写 1、50、100 通过；0、101 报错；空白由 Required 报错。

1/50/100 pass; 0/101 fail; Required handles blanks.

Range 的最小值不能比最大值大。

Minimum cannot exceed maximum.

**当前内置 Positive、PositiveKey、Range 只绑定普通 int、long、float、double 属性。** 其他数据类型虽然可以导表，但 byte、uint、decimal、int?、枚举等不能直接照抄这些数值标签。请用匹配类型的 `IFieldRule<T>` 或 Validate 写判断。

**Built-in Positive/PositiveKey/Range bind only ordinary int/long/float/double properties.** Exportable byte/uint/decimal/int?/enums cannot reuse these directly. Use matching `IFieldRule<T>`/Validate.

例如 decimal 的金额非负：

For nonnegative decimal prices:

```csharp
if (row.Price < 0m)
    context.Error(row, nameof(MyConfig.Price), "价格不能小于零。", "PRICE_NEGATIVE");
```

**示例英文说明 / English notes:**

- Price must not be negative.


这个片段中的 MyConfig 是说明性占位名字，要改成你实际的表模型，且 Price 类型要为 decimal。

Replace illustrative MyConfig with your actual model; Price must be decimal.

### 4.6 Reference：编号必须在另一张表里存在

**4.6 Reference: an ID must exist in another table**

假设 Hero 有 `int SkillId`，Skill 有 `int Id`。

Assume Hero has int SkillId and Skill has int Id.

```csharp
[Reference(nameof(HeroConfig.SkillId), typeof(SkillConfig), nameof(SkillConfig.Id))]
```

意思是“英雄填的 SkillId，必须能在 Skill 表的 Id 列找到”。例如填 2001，技能表中就必须有 2001。

Hero.SkillId must exist in Skill.Id; 2001 requires target 2001.

源字段和目标字段的 C# 类型必须完全一致。不能源 int、目标 long。

C# types must match exactly, not int versus long.

默认不允许空白。若游戏约定“0 或空白代表没有技能”，改成：

Blanks fail by default. To allow 0/blank as no skill:

```csharp
[Reference(nameof(HeroConfig.SkillId), typeof(SkillConfig), nameof(SkillConfig.Id),
    AllowDefaultValue = true)]
```

AllowDefaultValue 表示允许默认值。对于 int，默认值是 0。其他非零值仍必须存在。

AllowDefaultValue permits the type's default, 0 for int. Other nonzero values must still exist.

Reference 只检查值是否存在，不自动保证目标 Id 唯一、不检查技能伤害，也不会把 SkillId 自动变成技能对象。目标表通常仍要加 PositiveKey。

Reference checks existence only, not unique target IDs/damage, and does not turn SkillId into an object. Targets commonly also use PositiveKey.

如果要求“人人必须选技能而且不能选 0”，常用组合是 Required + Positive + Reference，并且不启用 AllowDefaultValue。

For mandatory nonzero skills, combine Required + Positive + Reference without AllowDefaultValue.

### 4.7 Check：绑定自己写的字段规则

**4.7 Check: bind your field rule**

```csharp
[Check(nameof(HeroConfig.Name), typeof(DisplayNameRule))]
```

名字检查如何实现见下一章。只写 DisplayNameRule 类但没有 Check 标签，不会自动对 Hero.Name 执行。

The next chapter implements name checks. DisplayNameRule alone does not run on Hero.Name without Check binding.

### 4.8 DependsOn：声明后置检查要读取另一张表

**4.8 DependsOn: declare other tables read by post-checks**

```csharp
[DependsOn(typeof(SkillConfig))]
```

它不是“检查一个字段”的标签，它告诉工具：这个规则类的 Validate 或 ValidateTable 会使用 Skill 表。

This is not a field check. It declares that the class's Validate/ValidateTable reads Skill.

如果需要读取两张其他表，可以写两个 DependsOn。

Add two DependsOn attributes when reading two tables.

如果依赖表缺失、解析不完整，工具会跳过这个规则类的后置检查并报告原因。这个类里只检查本表的后置条件也会被一起跳过。希望本表规则独立执行时，把本表条件拆到另一个不带 DependsOn 的规则类。

Missing/incomplete dependencies skip all this class's post-checks with a diagnostic, including local-only conditions. Split independent local rules into a separate class without DependsOn.

DependsOn 不保证另一张表没有字段规则错误，只保证它有完整可用的导入数据。你的业务代码仍要防御重复键、空值和越界值。

DependsOn guarantees complete imported data, not absence of field-rule errors. Defend against duplicates/nulls/out-of-range values yourself.

### 4.9 Required 和 ConfigRequired 的区别

**4.9 Required versus ConfigRequired**

| 名字<br>Name | 放在哪里<br>Placement | 检查对象<br>Checks | 例子<br>Example |
| --- | --- | --- | --- |
| Required | `TableRules<T>` 规则类上方<br>Above `TableRules<T>` class | 表的顶层字段有没有填<br>Top-level field filled | Hero.Name |
| ConfigRequired | 手写 class/struct 的成员上<br>Handwritten class/struct member | 对象成员有没有明确提供<br>Member explicitly provided | AddBuff.BuffId |

不能把 `[Required("Settings.MaxTargets")]` 当作当前内置标签的嵌套路径语法。当前标签绑定的是模型顶层属性；嵌套对象用 ConfigRequired 或 Validate 中的成员路径检查。

Required("Settings.MaxTargets") is not supported nested-attribute syntax. Attributes bind top-level properties; use ConfigRequired or member-path checks in Validate.

## 5. 自定义可复用字段规则

**5. Reusable custom field rules**

如果很多表都要检查名称长度，就不要在每张表里重复一大段判断。把“检查一个字符串”的方法写一次，再绑定到多张表。

Write a string-length check once and bind it to many tables rather than repeat it.

创建 `Rules/DisplayNameRule.cs`：

Create Rules/DisplayNameRule.cs:

```csharp
using System;
using ZrConfig.Validation;

public sealed class DisplayNameRule : IFieldRule<string>
{
    public void Validate(string value, FieldRuleContext context)
    {
        if (value.Length > 16)
            context.Error("名称不能超过 16 个字符。", "NAME_TOO_LONG");

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            context.Error("名称首尾不能有空格。", "NAME_SPACE");

        foreach (char character in value)
        {
            if (!char.IsControl(character)) continue;
            context.Error("名称不能包含换行或制表符。", "NAME_CONTROL");
            break;
        }
    }
}
```

**示例英文说明 / English notes:**

- Name must not exceed 16 characters.
- Name must not have leading/trailing spaces.
- Name must not contain newline/tab characters.


解释：

Explanation:

- `IFieldRule<string>` 表示这条规则检查字符串字段。<br>
  `IFieldRule<string>` checks string fields.
- value 是这一格已经解析后的字符串。<br>
  value is the parsed cell string.
- context.Error 负责报告该格的错误。<br>
  context.Error reports that cell's error.
- 不需要自己找 Excel 行号，工具会把位置带进来。<br>
  The tool supplies Excel coordinates automatically.

然后在 HeroRules 和 SkillRules 上分别添加：

Add these to HeroRules and SkillRules:

```csharp
[Required(nameof(HeroConfig.Name))]
[Check(nameof(HeroConfig.Name), typeof(DisplayNameRule))]
```

```csharp
[Required(nameof(SkillConfig.Name))]
[Check(nameof(SkillConfig.Name), typeof(DisplayNameRule))]
```

两张表会用同一套名称检查。

Both tables share the same name check.

### 5.1 概率规则例子

**5.1 Probability example**

我们约定：概率按万分比写，0 表示 0%，10000 表示 100%，2500 表示 25%。

Use basis points: 0=0%, 10000=100%, 2500=25%.

创建 `Rules/ProbabilityRule.cs`：

Create Rules/ProbabilityRule.cs:

```csharp
using ZrConfig.Validation;

public sealed class ProbabilityRule : IFieldRule<int>
{
    public void Validate(int value, FieldRuleContext context)
    {
        if (value < 0 || value > 10000)
            context.Error("概率必须在 0～10000 之间。", "PROBABILITY_RANGE");
    }
}
```

**示例英文说明 / English notes:**

- Probability must be between 0 and 10000.


绑定到 Hero 的 Rate 字段：

Bind to Hero.Rate:

```csharp
[Required(nameof(HeroConfig.Rate))]
[Check(nameof(HeroConfig.Rate), typeof(ProbabilityRule))]
```

`Rate` 必须确实存在，并且类型是 int。

Rate must exist and be int.

规则类型必须匹配字段类型：int 对 `IFieldRule<int>`，decimal 对 `IFieldRule<decimal>`，int? 对 `IFieldRule<int?>`。不匹配会产生 RULE_BINDING，不是正常的数值错误。

Match `IFieldRule<int>`/decimal/int? to the exact field type. Mismatches produce RULE_BINDING, not ordinary value errors.

### 5.2 为什么还要 Required

**5.2 Why Required is still needed**

Check 会跳过原始空白字段。所以“概率要填而且范围正确”要同时写 Required 和 Check。

Check skips original blanks. Mandatory valid probability needs both Required and Check.

自定义字段规则也可以用 context.Warning 报告警告。它的调用形式是 `context.Warning("提示", "代码")`，不传 row 和字段名，因为这里已经绑定好字段。

Field rules may call context.Warning("message", "code") without row/field because they are already bound.

不要让字段规则访问游戏场景或静态保存本次数据。可以保存本次检查需要的简单状态，但不要把上一次导出数据留在全局变量中。

Do not access scenes or retain run data statically. Simple current-check state is allowed, but never retain previous-export data globally.

## 6. 逐行后置检查

**6. Row post-validation**

标签能表达“这个字段满足一个条件”。如果条件同时用到几列，写 Validate 更清楚。

Attributes express one-field constraints. Validate is clearer for conditions spanning columns.

### 6.1 完整的小例子

**6.1 Complete small example**

Hero 有 Level 和 SkillId 两列。约定：90 级以上必须选择技能。

Hero has Level/SkillId. Heroes at level 90 or above require a skill.

创建或修改 `Rules/HeroRules.cs`：

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
                "90 级及以上英雄必须配置技能。", "HIGH_LEVEL_SKILL");
        }
    }
}
```

**示例英文说明 / English notes:**

- Heroes at level 90 or above must have a skill.


`row` 是当前这条英雄配置，`row.Level` 是它的等级。

row is the current hero; row.Level is its level.

`if` 表示“如果条件成立，就执行下面的内容”。

if executes the block when the condition holds.

`&&` 表示两个条件都要成立。

&& requires both conditions.

`==` 是判断是否相等。`=` 是给变量赋值，不是判断。

== compares equality; = assigns.

`return` 表示结束本次方法；这里结束当前这条记录的检查，不是结束整个导出。

return ends this method/current-record check, not the entire export.

这个类是一份替换示例。不要同时把另一个同名 HeroRules 也保存到 Rules。

This replaces another HeroRules version; do not keep duplicate classes.

### 6.2 Error 的四个参数是什么

**6.2 Error's four arguments**

```csharp
context.Error(row, nameof(HeroConfig.SkillId),
    "90 级及以上英雄必须配置技能。", "HIGH_LEVEL_SKILL");
```

**示例英文说明 / English notes:**

- Heroes at level 90 or above must have a skill.


| 参数<br>Argument | 用途<br>Purpose |
| --- | --- |
| row | 哪一条导入记录出错<br>Which imported record failed |
| nameof(HeroConfig.SkillId) | 错误要指向哪一列<br>Which column to locate |
| 中文提示文字<br>Message text | 告诉配表的人怎么修改<br>How the author should fix it |
| HIGH_LEVEL_SKILL | 稳定的错误代号，便于搜索，不是字段名<br>Stable searchable code, not a field name |

前三个参数必须给，错误代号可以省略，省略时默认 CUSTOM。建议给清楚的代号。

First three are required. Code defaults to CUSTOM; explicit descriptive codes are recommended.

不要自己 new 一个 HeroConfig，再传给 context.Error。位置记录绑定的是本次导入的原始 row；替换成另一个对象，工具无法定位。

Do not create a new HeroConfig for context.Error. Locations belong to this import's original row; copies cannot be located.

### 6.3 条件必填：开关打开才要求描述

**6.3 Conditional required: description when enabled**

假设 Hero 有 Enabled（bool）和 Description（string）：

Assume Enabled bool and Description string:

```csharp
if (row.Enabled && context.IsEmpty(row, nameof(HeroConfig.Description)))
{
    context.Error(row, nameof(HeroConfig.Description),
        "启用英雄时必须填写描述。", "DESCRIPTION_REQUIRED");
}
```

**示例英文说明 / English notes:**

- Enabled heroes require a description.


这段放进现有 Validate 方法。不要在同一个类中再创建第二个同签名 Validate。

Add inside the existing Validate; do not create another identical method.

如果不管是否启用都要填 Description，直接加 Required。条件必填才需要 if。

Unconditionally required Description uses Required. Only conditional requirements need if.

### 6.4 警告：提醒，但允许继续导出

**6.4 Warnings: notify without blocking**

```csharp
if (!context.IsEmpty(row, nameof(HeroConfig.Level)) && row.Level >= 95)
{
    context.Warning(row, nameof(HeroConfig.Level),
        "等级接近上限，请确认不是误填。", "LEVEL_HIGH_NOTICE");
}
```

**示例英文说明 / English notes:**

- Level is near the limit; confirm it is intentional.


Error 会阻止发布。Warning 本身不阻止发布，如果本次还有其他 Error，仍然失败。

Error blocks publication. Warning alone does not; other Errors still fail.

异常不会变成“只提醒一下”：规则执行时抛异常会产生错误。因此要报告正常业务不满足，使用 context.Error，不要随手 throw。

Exceptions become errors, not harmless notices. Use context.Error for expected business failure rather than throw.

### 6.5 已经有字段错误，为什么还会跑后置检查

**6.5 Why post-checks run after field errors**

工具希望一次尽量收集所有能独立判断的错误，不是第一个错误就马上退出。

The tool collects independently decidable errors rather than stopping at the first.

字段标签不通过，不代表 Validate 一定不执行。表解析完整时，后置检查仍可能运行。

Failed attributes do not necessarily prevent Validate when parsing is complete.

写检查时先排除明显缺失、越界的数据，避免重复或衍生报错。例如等级空白时不要再把默认 0 当作“低等级英雄”。

Guard against missing/out-of-range data to avoid duplicate/derived errors. A blank level's default 0 should not imply a low-level hero.

如果当前表在解析阶段就不完整，或依赖表不可用，该规则类的后置检查会被跳过，并有 BUSINESS_CHECK_SKIPPED 诊断。

Incomplete current/dependency tables skip class post-checks with BUSINESS_CHECK_SKIPPED.

## 7. 整表后置检查

**7. Table post-validation**

Validate 一次看一条记录。ValidateTable 一次拿到整张表的最终记录，适合检查组合重复、总数量、比例总和等。

Validate sees one row; ValidateTable receives all final records for composite duplicates, counts, totals, etc.

### 7.1 名称与等级的组合不能重复

**7.1 Unique name/level combinations**

“名称相同”未必错误，“等级相同”也未必错误，但同名且同等级可能不允许。

Repeating a name or level alone may be valid; repeating both may not.

下面方法加进 HeroRules 类内；文件顶部增加 `using System.Collections.Generic;` 和 `using System.Linq;`。

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
                "名称和等级的组合重复。", "NAME_LEVEL_DUPLICATE");
        }
    }
}
```

**示例英文说明 / English notes:**

- Duplicate name and level combination.


Where：先挑出可以检查的数据。

Where selects checkable data.

GroupBy：按“名称 + 等级”分组。

GroupBy groups by name + level.

Count：组里有几条记录。

Count returns group size.

foreach：逐个处理。

foreach processes each item.

ToUpperInvariant：用一致的大小写形式比较，因而这里的英文名忽略大小写。

ToUpperInvariant normalizes case, making English names case-insensitive here.

这个检查会给每条重复记录报错，方便分别找到对应 Excel 行。

Report every duplicate so each Excel row is easy to find.

### 7.2 表里必须有至少一个启用英雄

**7.2 At least one enabled hero**

如果表不是空表，可以这样定位到第一条记录：

For nonempty tables, report against the first record:

```csharp
if (rows.Count > 0 && !rows.Any(row => row.Enabled))
{
    context.Error(rows[0], nameof(HeroConfig.Enabled),
        "至少需要一个启用的英雄。", "NO_ENABLED_HERO");
}
```

**示例英文说明 / English notes:**

- At least one enabled hero is required.


没有数据时，不能写 rows[0]，会越界。这也是先判断 Count 的原因。

rows[0] is invalid when empty; check Count first.

当前 ValidationContext 的常用报告接口需要一条真实导入记录。对于零行表的“必须有数据”规则，需要另行设计整表诊断或输入约束；不要伪造 row 或声称该片段已经覆盖零行情况。

Current common reporting APIs require a real row. Zero-row “data required” constraints need separate table diagnostics/input constraints. Do not invent rows or claim this fragment covers emptiness.

### 7.3 规则不能只靠文件名执行

**7.3 Filenames do not bind rules**

文件叫 `HeroRules.cs` 不代表工具知道检查 Hero。类必须继承 `TableRules<HeroConfig>`，而且方法签名要正确。

HeroRules.cs alone identifies nothing; inherit `TableRules<HeroConfig>` with correct signatures.

如果有多个规则类，后置检查不会只选其中一份，所有可发现且绑定有效的类都会执行。

All discovered valid rule classes run, not just one.

## 8. 跨表后置检查

**8. Cross-table post-validation**

Reference 只问“技能编号存在吗”。跨表业务检查可以问“这个技能伤害够不够配这个英雄”。

Reference asks whether an ID exists. Business checks can ask whether its damage suits the hero.

### 8.1 完整规则例子

**8.1 Complete example**

规则：技能伤害至少等于英雄等级 × 5。

Rule: skill damage ≥ hero level × 5.

创建 `Rules/HeroSkillRules.cs`。这是一个新类，能够与 HeroRules 同时存在。

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

        // 首次访问建立索引，同一次导出的所有行和规则复用。
        var skills = context.GetSkillTable();
        if (!skills.TryGet(hero.SkillId, out var skill) || skill == null || skill.Id <= 0)
            return;

        if (context.IsEmpty(skill, nameof(SkillConfig.Damage)) || skill.Damage < 0)
            return;

        int requiredDamage = hero.Level * 5;
        if (skill.Damage < requiredDamage)
            context.Error(hero, nameof(HeroConfig.SkillId),
                $"技能 {skill.Id} 伤害为 {skill.Damage}，" +
                $"该英雄至少需要 {requiredDamage}。", "SKILL_DAMAGE_LOW");
    }
}
```

**示例英文说明 / English notes:**

- First access builds indexes reused across every row/rule in this export.
- The message reports the selected skill ID and actual damage.
- It also reports the minimum damage required by the hero.


教程假设 SkillRules 会检查技能编号和伤害，HeroRules 会检查 SkillId 的引用存在。

Assume SkillRules checks IDs/damage and HeroRules checks SkillId existence.

这里跳过找不到的技能，不是允许不存在，而是把“存在性”交给 Reference，避免再报一个误导人的“伤害不足”。

Missing skills are skipped here because Reference owns existence checks, avoiding misleading insufficient-damage errors.

### 8.2 为什么逐行调用不会重复建字典

**8.2 Why row calls do not rebuild dictionaries**

`context.GetSkillTable()` 首次访问时用本次导入的 Skill 行构造生成表，并缓存主键和分组索引；其后每条英雄记录、其他规则类以及 ValidateTable 都使用同一个实例。`TryGet` 直接查询索引。规则无需 new 字典，也无需在 ValidateTable 中提前准备数据。

context.GetSkillTable() creates/caches the generated table and key/group indexes from current Skill rows on first access. Every hero/rule/ValidateTable shares it. TryGet queries directly; no handwritten dictionary or ValidateTable preparation is needed.

本例的 Skill.Id 在 Excel 第 3 行声明为主键，缺失或重复会使表不完整，带 DependsOn 的跨表方法随之跳过。PositiveKey 仍用于正数等业务约束，业务 Error 不会自动过滤索引中的记录，所以仍检查 `skill.Id > 0` 和伤害范围。

Skill.Id is marked as Excel row-3 key. Missing/duplicate keys make the table incomplete and skip dependent methods. PositiveKey still checks positivity; business Errors do not filter indexed rows, so validate skill.Id > 0 and damage ranges.

重新导出会创建新的上下文与索引，不读取旧发布版本。`context.GetTable<SkillConfig>()` 返回同一份已缓存的 `ConfigTable<SkillConfig>`。每张表统一在 Excel 中声明主键并生成查询入口。接口边界见 [校验索引表](rules-development.md#在后置检查中使用索引表)。

Re-export creates a new context/index, never old published data. `context.GetTable<SkillConfig>`() returns the same cached ConfigTable. Each table declares Excel keys and generates queries; see [boundaries](rules-development.md#在后置检查中使用索引表).

### 8.3 哪些检查要声明 DependsOn

**8.3 Which checks need DependsOn?**

你在自定义 Validate 或 ValidateTable 中用 `context.GetSkillTable()` 或 `context.GetTable<SkillConfig>()` 时，需要声明 DependsOn。

Custom Validate/ValidateTable using GetSkillTable/`GetTable<SkillConfig>` need DependsOn.

Reference 本身包含目标表信息，不需要为了这个标签再加 DependsOn。但自己的业务代码读取另一张表，仍要加。

Reference already names its target and needs no extra dependency attribute; custom reads do.

如果一个规则只判断 Hero.Level 和 Hero.SkillId，不读取技能表，它属于本表逻辑，可以不加 DependsOn。

Checks only comparing Hero.Level/SkillId without reading Skill are local and need none.

## 9. 检查对象成员和数组元素

**9. Object members and array elements**

内置标签主要检查顶层字段。Settings.MaxTargets、Actions[1].Delay 等复杂路径，放在 Validate 里检查。

Built-ins mainly check top-level fields. Check Settings.MaxTargets/Actions[1].Delay in Validate.

假设表里已经有 Settings 和 Actions，文件顶部添加：

With Settings/Actions present, add this import:

```csharp
using Tutorial.Config;
```

Validate 里写：

In Validate:

```csharp
if (row.Settings.MaxTargets <= 0)
{
    context.Error(row, "Settings.MaxTargets",
        "最多目标数必须大于零。", "TARGET_COUNT");
}

for (int i = 0; i < row.Actions.Length; i++)
{
    BattleAction action = row.Actions[i];
    string path = $"Actions[{i}]";

    if (action.Delay < 0)
        context.Error(row, path + ".Delay",
            "动作延时不能小于零。", "ACTION_DELAY");

    if (action is Damage damage && damage.Ratio < 0)
        context.Error(row, path + ".Ratio",
            "伤害倍率不能小于零。", "DAMAGE_RATIO");

    if (action is AddBuff buff && buff.Stacks <= 0)
        context.Error(row, path + ".Stacks",
            "Buff 层数必须大于零。", "BUFF_STACKS");
}
```

**示例英文说明 / English notes:**

- Maximum targets must be positive.
- Action delay must not be negative.
- Damage ratio must not be negative.
- Buff stacks must be positive.


for 是按下标逐个处理数组。

for visits array elements by index.

`action is Damage damage` 表示“如果这个动作实际是 Damage，就把它当作 Damage 读取专有成员”。否则不能直接假定每个动作都有 Ratio。

action is Damage damage tests the concrete kind and enables Damage-only members; not every action has Ratio.

### 9.1 报错路径怎么写

**9.1 Diagnostic paths**

| 检查对象<br>Target | 路径<br>Path |
| --- | --- |
| 顶层字段<br>Top-level field | `nameof(HeroConfig.Level)` |
| Settings 的目标数量<br>Settings target count | `"Settings.MaxTargets"` |
| 第一个动作的延时<br>First action delay | `"Actions[0].Delay"` |
| 第二个动作的 Buff 编号<br>Second action buff ID | `"Actions[1].BuffId"` |
| 第一组第三个动作<br>Third action in first group | `"Groups[0].Actions[2].Delay"` |
| 第二阶段第一组第一个动作<br>First action/group in second phase | `"Phases[1].Groups[0].Actions[0].Delay"` |

**这里传完整的表记录 row，不是 action、Settings 或 buff。** 第一参数用 row，第二参数用路径，工具才知道它原来来自哪张 Excel 的哪条记录。

**Pass the complete row, not action/Settings/buff.** Row plus path identifies the original workbook record.

数组下标是排序后数组的位置，从 0 开始。工具记录了输入位置映射，仍会定位到对应的原始物理行列，而不是简单把下标当作 Excel 行号。

Indexes are zero-based after sorting; input maps recover original physical coordinates, not index-as-row-number.

JSON 单格里某成员出错，会定位到承载该 JSON 的单元格；分列、多行格式可以定位到更具体的成员格子。

JSON-member errors locate the containing cell; split/multi-row formats locate individual member cells.

路径拼错会出现 VALIDATOR_FIELD。检查一下大小写、层级和下标，不要用 Actions[].Delay 作为某个具体元素的报告路径，要用 Actions[实际下标].Delay。

Misspelled paths yield VALIDATOR_FIELD. Check case/hierarchy/indexes. Use Actions[actual-index].Delay, not Actions[].Delay.

### 9.2 数组至少一个元素

**9.2 At least one array element**

```csharp
if (row.Actions.Length == 0)
    context.Error(row, nameof(HeroConfig.Actions),
        "至少需要一个动作。", "ACTION_REQUIRED");
```

**示例英文说明 / English notes:**

- At least one action is required.


这是“非空数组”检查。Required 只关心填写情况，写了 `[]` 仍然是空数组。

This checks nonempty content. Required only checks input; explicit [] remains empty.

### 9.3 成员条件必填

**9.3 Conditionally required members**

```csharp
if (row.Settings.Interruptible &&
    context.IsEmpty(row, "Settings.MaxTargets"))
{
    context.Error(row, "Settings.MaxTargets",
        "允许打断时必须显式填写目标数量。", "TARGET_COUNT_REQUIRED");
}
```

**示例英文说明 / English notes:**

- Explicit target count is required when interruption is allowed.


使用成员路径检查原始填写情况。是否有默认值和是否显式填写，是两个不同问题。

Member paths inspect original filled status. Defaults and explicit input are separate facts.

## 10. 直接表对象引用：少写一次手动查找

**10. Direct table references: avoid manual lookups**

前面的 Hero.SkillId 是整数。游戏想使用技能数据，需要拿这个编号去技能表查找。

Integer Hero.SkillId requires a lookup to access skill data.

另一种支持的写法是：Excel 字段叫 `Skill`，类型写 `SkillConfig`，数据仍填技能编号。

Alternatively use field Skill, type SkillConfig, still entering the skill ID.

| 行<br>Row | Id | Name | Skill |
| --- | --- | --- | --- |
| 1 | Id | Name | Skill |
| 2 | int | string | SkillConfig |
| 3 | 主键<br>`主键` (literal marker) |  |  |
| 4 | 编号<br>Identifier | 名称<br>Name | 技能编号<br>Skill identifier |
| 5：所属端<br>5: Side | CS | CS | CS |
| 6 | 1001 | 小剑士<br>Young swordsman | 2001 |

生成代码后，HeroConfig.Skill 是技能对象引用。程序可以读取：

Generated HeroConfig.Skill is an object reference:

```csharp
if (hero.Skill != null)
{
    int damage = hero.Skill.Damage;
}
```

工具读取时先创建全部记录，再建立对象关联，所以可以支持双向引用和自引用。

Import creates records before linking, supporting bidirectional/self references.

- 目标表必须有 int Id。<br>
  Target table needs int Id.
- 被引用目标表的 Id 要非零且唯一。<br>
  Referenced IDs must be nonzero/unique.
- 非零编号找不到目标，会阻止导出。<br>
  Missing nonzero targets block export.
- 空白或 0 表示没有引用，生成的引用可为 null。<br>
  Blank/0 means null; generated references are nullable.
- 必须选技能时，在新版本规则上写 `[Required(nameof(HeroConfig.Skill))]`。<br>
  For mandatory skills add Required(nameof(HeroConfig.Skill)).
- 不需要给对象引用再绑整数 Reference 标签；那是不同的检查方式。<br>
  Do not add integer Reference attributes to object references; they are different mechanisms.

在依赖目标数据的后置检查中仍声明 `[DependsOn(typeof(SkillConfig))]`，并先判断引用不为空，再访问成员。

Post-checks reading target data still declare DependsOn(typeof(SkillConfig)) and null-check before access.

例如：

For example:

```csharp
if (hero.Skill != null && hero.Skill.Damage < hero.Level * 5)
    context.Error(hero, nameof(HeroConfig.Skill),
        "所选技能伤害不足。", "SKILL_DAMAGE_LOW");
```

**示例英文说明 / English notes:**

- Selected skill has insufficient damage.


此片段属于使用 Skill 对象字段的替代版本。第 12 章完整练习使用 SkillId 整数，不要直接复制这个片段到没有 Skill 属性的模型里。

This is the alternative Skill-object model. Chapter 12 uses integer SkillId; do not copy into a model without Skill.

把 SkillId 改成 Skill 属于表结构变化：修改 Excel、生成代码、修改规则、重新导出，并同步游戏代码与数据。

SkillId→Skill is a schema change: edit Excel, regenerate, update rules, export, synchronize game code/data.

当前直接表对象引用支持表的直接单值字段，不用于数组元素或自定义对象内部成员。

Direct references currently support scalar table fields, not array elements/custom-object members.

## 11. 特殊格式怎么扩展：转换器

**11. Extend special formats with converters**

大多数 class、struct 不需要转换器。它们直接按公开成员生成读写代码即可。

Most classes/structs need no converter; public members generate serialization directly.

只有你需要“Excel 看起来是一种格式，但程序要保存成另一种形式”时，才使用转换器。

Use converters only when Excel representation differs from program storage.

例子：Excel 填 `1.25` 元，程序内部保存 `125` 分。

Example: enter currency 1.25, store 125 cents.

### 11.1 定义 Money

**11.1 Define Money**

创建 `Config/CustomTypes/Money.cs`：

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

这个 readonly struct 通过转换器接入，工具不需要直接给只读字段赋值。

This readonly struct uses a converter, so the tool never directly assigns readonly fields.

### 11.2 定义 MoneyConverter

**11.2 Define MoneyConverter**

创建 `Config/Serialization/MoneyConverter.cs`：

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
                throw new FormatException("金额最多支持两位小数。");

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

**示例英文说明 / English notes:**

- Currency supports at most two decimal places.


| 方法<br>Method | 什么时候用<br>When | 例子<br>Example |
| --- | --- | --- |
| Parse | 导表时读 Excel 文本<br>Read Excel text during export | "1.25" → Money(125) |
| Write | 写入二进制<br>Write binary | 保存整数 125<br>Store integer 125 |
| Read | 回读或游戏加载<br>Read-back/game load | 读取整数 125 → Money(125)<br>Integer 125 → Money(125) |

Excel 类型填 `Tutorial.Config.Money`，数据填 `1.25`。数组类型填 `Tutorial.Config.Money[]`，数据填 `[1.25;2.50]`。

Excel type Tutorial.Config.Money uses 1.25; Money[] uses [1.25;2.50].

Write 和 Read 必须一一对应：写 Int32 就读 Int32，写入的字段顺序和读取顺序一致。工具会回读检查，但你仍需正确设计协议。

Write/Read must match exactly: Int32 with Int32 and identical member order. Read-back checks help, but protocol design remains yours.

特定业务如“价格不能负数”，用字段规则或后置检查表达；这个转换器只负责金额格式和读写，不擅自禁止负数。

Negative-price policies belong in field/post-rules. This converter handles formatting/serialization without imposing positivity.

`"1"` 是转换器版本。改变二进制表示时必须升级它，例如改成 `"2"`，再重新生成、导出，并同步游戏代码。不要只更新读的一边。

"1" is converter version. Change it, e.g. to "2", when binary representation changes, then regenerate/export/synchronize game code. Never update only the reader.

转换器实例可能被复用，不能记住“上一个格子的金额”影响下一次结果，也不应保存可变共享状态。

Converter instances may be reused; no previous-cell state or mutable shared state affecting results.

## 12. 完整练习项目

**12. Complete exercise project**

前面是分开解释。本章把需要的 Excel、手写类型和规则拼成一套完整练习。

This chapter combines Excel, handwritten types, and rules into one complete exercise.

你可以直接下载 [完整练习包](assets/user-guide-example.zip)，解压后选择其中的 Data、Config、Rules，输出选新建的 Output。练习包只有两张 Excel 和手写源码，不含生成代码或旧导出记录。先生成代码，再导出即可。

Download the [exercise ZIP](assets/user-guide-example.zip), select its Data/Config/Rules, and choose new Output. It includes two workbooks/handwritten source, no generated code/old exports. Generate, then export.

手写类型统一放在 CustomTypes，与教程附件一致。

Handwritten types use CustomTypes, matching the attachment.

也可以按本章手动创建。**使用一个新的练习目录，不要把教学 Hero/Skill 表混进仓库 samples/Data。** 否则旧规则、其他同名表或额外生成模型可能影响练习。

You may create it manually. **Use a new exercise directory, never mix teaching Hero/Skill into samples/Data.** Old rules/duplicate tables/extra models could interfere.

### 12.1 最终目录

**12.1 Final layout**

```text
ZrConfig-Beginner/
  Data/
    Hero.xlsx
    Skill.xlsx
  Config/
    CustomTypes/
      BattleSettings.cs
      BattleAction.cs
    Serialization/                   首次生成时由工具初始化 / Initialized by the tool on first generation
    Generated/                       首次生成时由工具创建 / Created by the tool on first generation
  Rules/
    DisplayNameRule.cs
    ProbabilityRule.cs
    HeroRules.cs
    HeroSkillRules.cs
    SkillRules.cs
  Output/                            首次导出时创建 / Created on first export
```

下面的代码都是最终版本。之前章节中同名的示范类不要另外再保存一份。每个类在这个项目里只出现一次。

Code below is final. Do not retain earlier same-name examples separately; each class appears once.

### 12.2 两张 Excel

**12.2 Two workbooks**

#### Hero.xlsx

使用五行表头：字段名、类型、主键、说明、所属端。列比较多，下面分两块显示。**两块是同一张 Hero 工作表的左右两部分，不是两张 Sheet。** 第 3 行只有 A 列填写“主键”，其余格子留空。

Use field/type/key/description/side headers. Wide Hero is shown in two blocks, **left/right parts of one sheet, not two sheets**. Only A contains 主键 on row 3.

A～I 列：

Columns A–I:

| Excel 行号<br>Excel row | A | B | C | D | E | F | G | H | I |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Id | Name | Level | SkillId | Rate | Enabled | Description | Settings.MaxTargets | Settings.Interruptible |
| 2 | int | string | int | int | int | bool | string | Tutorial.Config.BattleSettings | |
| 3 | 主键<br>`主键` (literal marker) |  |  |  |  |  |  |  |  |
| 4 | 编号<br>Identifier | 名称<br>Name | 等级<br>Level | 技能编号<br>Skill ID | 万分比概率<br>Probability in basis points | 是否启用<br>Enabled | 描述<br>Description | 最大目标数<br>Maximum targets | 能否打断<br>Interruptible |
| 5：所属端<br>5: Side | CS | CS | CS | CS | CS | CS | CS | CS | CS |

J～P 列：

Columns J–P:

| Excel 行号<br>Excel row | J | K | L | M | N | O | P |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
| 2 | Tutorial.Config.BattleAction[] | | | | | | |
| 3 | | | | | | | |
| 4 | 动作顺序<br>Action order | 动作种类<br>Kind | 延时<br>Delay | 伤害倍率<br>Damage ratio | 能否暴击<br>Critical hits? | Buff 编号<br>Buff ID | Buff 层数<br>Stacks |
| 5：所属端<br>5: Side | CS | CS | CS | CS | CS | CS | CS |

第 6～8 行的 A～I 列：

Rows 6–8, columns A–I:

| Excel 行号<br>Excel row | A | B | C | D | E | F | G | H | I |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 6 | 1001 | 小剑士<br>Young swordsman | 10 | 2001 | 2500 | true | 新手英雄<br>Beginner hero | 1 | true |
| 7 | 1001 | | | | | | | | |
| 8 | 1002 | 小法师<br>Young mage | 95 | 2002 | 10000 | true | 高级英雄<br>Advanced hero | 3 | false |

第 6～8 行的 J～P 列：

Rows 6–8, columns J–P:

| Excel 行号<br>Excel row | J | K | L | M | N | O | P |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 6 | 1 | Damage | 0.2 | 1.5 | true | | |
| 7 | 2 | AddBuff | 0.5 | | | 3001 | 2 |
| 8 | 1 | Damage | 0 | 2 | false | | |

第 7 行是 1001 的第二个动作，主键 Id 重复填写，其他主字段留空。最终应该导出 2 条英雄记录，不是 3 条。

Row 7 is hero 1001's second action, repeating Id and leaving other main fields blank. Export 2 heroes, not 3.

#### Skill.xlsx

| Excel 行号<br>Excel row | A | B | C |
| --- | --- | --- | --- |
| 1 | Id | Name | Damage |
| 2 | int | string | int |
| 3 | 主键<br>`主键` (literal marker) |  |  |
| 4 | 技能编号<br>Skill ID | 技能名称<br>Skill name | 伤害<br>Damage |
| 5：所属端<br>5: Side | CS | CS | CS |
| 6 | 2001 | 斩击<br>Slash | 80 |
| 7 | 2002 | 火球<br>Fireball | 600 |

这里没有 Buff 表。练习只演示 Buff 参数格式、必填和层数规则，并不假装检查了 Buff 3001 是否存在。正式项目若需要这个约束，应增加 Buff 表和对应跨表业务规则。

No Buff table exists here. The exercise tests format/required input/stacks, not existence of Buff 3001. Add Buff and cross-table rules in production if required.

### 12.3 文件：Config/CustomTypes/BattleSettings.cs

**12.3 File: Config/CustomTypes/BattleSettings.cs**

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

### 12.4 文件：Config/CustomTypes/BattleAction.cs

**12.4 File: Config/CustomTypes/BattleAction.cs**

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

### 12.5 文件：Rules/DisplayNameRule.cs

**12.5 File: Rules/DisplayNameRule.cs**

```csharp
using System;
using ZrConfig.Validation;

public sealed class DisplayNameRule : IFieldRule<string>
{
    public void Validate(string value, FieldRuleContext context)
    {
        if (value.Length > 16)
            context.Error("名称不能超过 16 个字符。", "NAME_TOO_LONG");

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            context.Error("名称首尾不能有空格。", "NAME_SPACE");

        foreach (char character in value)
        {
            if (!char.IsControl(character)) continue;
            context.Error("名称不能包含换行或制表符。", "NAME_CONTROL");
            break;
        }
    }
}
```

**示例英文说明 / English notes:**

- Name must not exceed 16 characters.
- Name must not have leading/trailing spaces.
- Name must not contain newlines/tabs.


### 12.6 文件：Rules/ProbabilityRule.cs

**12.6 File: Rules/ProbabilityRule.cs**

```csharp
using ZrConfig.Validation;

public sealed class ProbabilityRule : IFieldRule<int>
{
    public void Validate(int value, FieldRuleContext context)
    {
        if (value < 0 || value > 10000)
            context.Error("概率必须在 0～10000 之间。", "PROBABILITY_RANGE");
    }
}
```

**示例英文说明 / English notes:**

- Probability must be between 0 and 10000.


### 12.7 文件：Rules/HeroRules.cs

**12.7 File: Rules/HeroRules.cs**

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
                    "90 级及以上英雄必须配置技能。", "HIGH_LEVEL_SKILL");

            if (row.Level >= 95 && row.Level <= 100)
                context.Warning(row, nameof(HeroConfig.Level),
                    "等级接近上限，请确认不是误填。", "LEVEL_HIGH_NOTICE");
        }

        if (row.Enabled && context.IsEmpty(row, nameof(HeroConfig.Description)))
            context.Error(row, nameof(HeroConfig.Description),
                "启用英雄时必须填写描述。", "DESCRIPTION_REQUIRED");

        if (row.Settings.MaxTargets <= 0)
            context.Error(row, "Settings.MaxTargets",
                "最多目标数必须大于零。", "TARGET_COUNT");

        for (int i = 0; i < row.Actions.Length; i++)
        {
            BattleAction action = row.Actions[i];
            string path = $"Actions[{i}]";

            if (action.Delay < 0)
                context.Error(row, path + ".Delay",
                    "动作延时不能小于零。", "ACTION_DELAY");

            if (action is Damage damage && damage.Ratio < 0)
                context.Error(row, path + ".Ratio",
                    "伤害倍率不能小于零。", "DAMAGE_RATIO");

            if (action is AddBuff buff && buff.Stacks <= 0)
                context.Error(row, path + ".Stacks",
                    "Buff 层数必须大于零。", "BUFF_STACKS");
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
                    "名称和等级的组合重复。", "NAME_LEVEL_DUPLICATE");
        }
    }
}
```

**示例英文说明 / English notes:**

- Heroes at level 90 or above require a skill.
- Level is near the limit; confirm it is intentional.
- Enabled heroes require a description.
- Maximum targets must be positive.
- Action delay must not be negative.
- Damage ratio must not be negative.
- Buff stacks must be positive.
- Duplicate name/level combination.


### 12.8 文件：Rules/HeroSkillRules.cs

**12.8 File: Rules/HeroSkillRules.cs**

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

        // 首次访问建立索引，同一次导出的所有行和规则复用。
        var skills = context.GetSkillTable();
        if (!skills.TryGet(hero.SkillId, out var skill) || skill == null || skill.Id <= 0)
            return;

        if (context.IsEmpty(skill, nameof(SkillConfig.Damage)) || skill.Damage < 0)
            return;

        int requiredDamage = hero.Level * 5;
        if (skill.Damage < requiredDamage)
            context.Error(hero, nameof(HeroConfig.SkillId),
                $"技能 {skill.Id} 伤害为 {skill.Damage}，" +
                $"该英雄至少需要 {requiredDamage}。", "SKILL_DAMAGE_LOW");
    }
}
```

**示例英文说明 / English notes:**

- First access builds indexes reused by all rows/rules in this export.
- The message gives the skill ID and actual damage.
- It also gives the minimum damage required by the hero.


### 12.9 文件：Rules/SkillRules.cs

**12.9 File: Rules/SkillRules.cs**

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
                "技能伤害为零，请确认是否为辅助技能。", "ZERO_DAMAGE_NOTICE");
    }
}
```

**示例英文说明 / English notes:**

- Skill damage is zero; confirm this is a support skill.


### 12.10 选好四个目录，生成和导出

**12.10 Select paths, generate, export**

| 输入框<br>Input | 选择<br>Selection |
| --- | --- |
| Excel 输入目录<br>Excel input | ZrConfig-Beginner/Data |
| 输出目录<br>Output | ZrConfig-Beginner/Output |
| 配表代码根目录<br>Code root | ZrConfig-Beginner/Config |
| C# 规则目录<br>C# rules | ZrConfig-Beginner/Rules |

点击“生成配表代码”，应得到 HeroConfig、SkillConfig 和对应 Codec。

Generate configuration code to obtain HeroConfig/SkillConfig and codecs.

再点击“校验并导出”。预期：

Then Validate and export. Expect:

- 导出成功。<br>
  Success.
- 错误 0。<br>
  0 errors.
- 警告 1：小法师等级 95，出现 LEVEL_HIGH_NOTICE。<br>
  1 warning: level-95 young mage triggers LEVEL_HIGH_NOTICE.
- Hero 表 2 条，Skill 表 2 条。<br>
  Hero 2 rows, Skill 2 rows.
- 小剑士有 2 个动作；小法师有 1 个动作。<br>
  Swordsman 2 actions, mage 1.

警告是练习中故意保留的提示，不必为了“必须 0 警告”把正确数据乱改。如果你不需要这个提醒，可以删除 LEVEL_HIGH_NOTICE 的判断。

This warning is intentional; do not corrupt valid data just to reach zero warnings. Remove the LEVEL_HIGH_NOTICE condition if unwanted.

### 12.11 故意改错，看工具怎样帮你找

**12.11 Deliberate errors and locations**

每次只做一项。保存 Excel → 直接导出 → 查看提示 → 改回原值。这里只改数据，不需要重新生成代码。

One change at a time: save → export → inspect → restore. Data-only changes need no generation.

| 改哪一格<br>Cell | 改成什么<br>Change | 预期提示<br>Expected result |
| --- | --- | --- |
| Hero C6，Level<br>Hero C6, Level | 101 | RANGE，等级必须在 1～100<br>RANGE: level must be 1–100 |
| Hero B6，Name<br>Hero B6, Name | 删除内容<br>Clear | REQUIRED，名称必填<br>REQUIRED |
| Hero B6，Name<br>Hero B6, Name | 名称前加一个空格<br>Add leading space | NAME_SPACE |
| Hero E6，Rate | 10001 | PROBABILITY_RANGE |
| Hero G6，Description<br>Hero G6, Description | 删除内容<br>Clear | DESCRIPTION_REQUIRED |
| Hero D6，SkillId | 9999 | REFERENCE_MISSING |
| Skill C6，Damage<br>Skill C6, Damage | 20 | SKILL_DAMAGE_LOW，10 级英雄至少需要 50 伤害；规则定位到引用该技能的 Hero D6<br>SKILL_DAMAGE_LOW: level 10 needs ≥50, located at referencing Hero D6 |
| Hero H6，Settings.MaxTargets | 0 | TARGET_COUNT |
| Hero L7，Actions[].Delay<br>Hero L7, Actions[].Delay | -1 | ACTION_DELAY，定位到第二个动作对应的原行<br>ACTION_DELAY at the second action's original row |
| Hero O7，Actions[].BuffId<br>Hero O7, Actions[].BuffId | 删除内容<br>Clear | 对象成员缺少必填输入，解析失败<br>Missing required member; parse failure |
| Hero P7，Actions[].Stacks | 0 | BUFF_STACKS |
| Hero D8，SkillId | 0 | HIGH_LEVEL_SKILL |
| Skill A7，Id<br>Skill A7, Id | 2001 | KEY_DUPLICATE，技能编号重复<br>KEY_DUPLICATE |
| Hero B8 和 C8<br>Hero B8 and C8 | 小剑士、10<br>Young swordsman, 10 | NAME_LEVEL_DUPLICATE，两个英雄的名称与等级组合相同<br>NAME_LEVEL_DUPLICATE for both heroes |

“为什么没有导出新版本”看错误数量即可。只要存在 Error，当前有效版本不变。修好后再导出，日志文件会被本次内容覆盖。

Any Error keeps Current unchanged. Fix/export again; latest log files update.

练习跨表重复时，可能同时出现另一英雄引用不到旧编号的错误，这并不冲突：工具尽量把独立错误一起列出来。

Duplicate-target exercises may also cause another hero's old-ID reference to fail. Independent errors are reported together.

## 13. 命令行使用

**13. Command-line usage**

**CLI** 是 **Command-Line Interface（命令行界面）** 的缩写。本文说的 CLI 通常指项目中的 **ZrConfig.Cli 命令行工具**：输入一行命令告诉程序做什么、读取哪个目录、把结果写到哪里，程序执行后输出文字结果、日志位置和退出码。退出码是交给脚本判断成功或失败的数字，具体取值见第 13.4 节。

**CLI** means **Command-Line Interface**, usually ZrConfig.Cli here. A command specifies operation, inputs, and output; execution returns text/log paths/exit code. Scripts use the numeric code for success/failure; see 13.4.

**App** 是带窗口和按钮的桌面工具。配表人员可以完全使用 App；程序员在需要脚本自动导出、构建时检查配置，或调试业务规则时使用 CLI。App 与 CLI 共用核心生成、检查和导出逻辑；相同输入、代码和规则执行相同的业务检查。`export --generate` 包含 App 的生成预检查和必要的生成步骤；普通 `export` 只导出，成功后两者都会实际写入版本数据。

**App** is the desktop UI, sufficient for authors. Developers use CLI for automation, build checks, and rule debugging. Both share generation/validation/export and identical business checks. export --generate includes App-style prechecks/needed generation; plain export only exports. Successful export writes real version data.

### 先区分几个入口

**Distinguish entry points**

| 名称<br>Name | 用途<br>Purpose | 怎样操作<br>Use |
| --- | --- | --- |
| ZrConfig.App | 桌面配表工具<br>Desktop authoring | 打开窗口，选择目录并点击按钮<br>Choose paths/click buttons |
| ZrConfig.Cli | 命令行生成、校验、导出与版本管理<br>Generation, validation, export, release management | 使用 `codegen`、`validate`、`export`、`support` 或 `release`<br>codegen, validate, export, support, release |
| ZrConfig.Maintenance | 源码仓库的维护工具<br>Repository maintenance | 通过 `scripts/maintain.sh` 或 `scripts/maintain.ps1` 准备 Unity 示例、检查可移植性、打包和清理<br>maintain.sh/maintain.ps1 for Unity preparation, portability, packaging, cleanup |

PowerShell、Bash 是接收和运行命令的环境，终端是输入命令的窗口；ZrConfig.Cli 是由它们启动的程序。`dotnet` 用于构建、启动 .NET 程序，`scripts/dotnet.sh` 是仓库寻找 dotnet 的包装脚本。`scripts/maintain.*` 的准备和打包命令属于维护工具，其参数不能直接传给 ZrConfig.Cli。公开命令及验证流程见 [源码构建与升级](development.md)。

PowerShell/Bash execute commands; terminals provide input windows. They launch CLI. dotnet builds/runs .NET; scripts/dotnet.sh locates it. maintain.* commands belong to maintenance and cannot be passed directly to CLI. See [public commands](development.md).

下面源码示例中，`run --project src/ZrConfig.Cli` 表示构建并运行命令行工程，后面的 `--` 用于分隔 dotnet 参数和工具参数；`export` 才是交给 ZrConfig.Cli 的操作名，再后面依次是目录。使用 Windows 自包含发布包时，直接运行 `ZrConfig.Cli.exe export ...`，无需 `dotnet run --project`，见第 13.6 节。

In source examples, run --project src/ZrConfig.Cli builds/runs CLI; -- separates dotnet/tool arguments. export is the tool operation, followed by paths. Self-contained Windows uses ZrConfig.Cli.exe export directly without dotnet run; see 13.6.

下面 Bash 命令从仓库根目录执行；Windows PowerShell 见第 13.6 节。`/你的路径/` 是需要替换的示意路径，不能原样使用。路径包含空格时，用英文双引号包围。Data 参数是输入根目录，工具自动递归读取其下的 `.xlsx` 和 `.xls`，两种格式可以混用。

Bash examples run from root; Windows is in 13.6. Replace /你的路径/ placeholders; quote spaced paths with ASCII double quotes. Data is the recursively scanned root and may mix .xlsx/.xls.

### 共用 App 项目配置

**Share App project settings**

推荐直接使用 App 保存的 `project.zrconfig.json`，各命令读取其中需要的目录：

Prefer the App-saved project.zrconfig.json; commands read required directories:

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- codegen --project samples/project.zrconfig.json
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- validate --project samples/project.zrconfig.json
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- export --project samples/project.zrconfig.json
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- support --project samples/project.zrconfig.json
```

第一个 `--project` 属于 dotnet，用来选择工具工程；`--` 后面的 `--project` 属于 ZrConfig，用来选择项目配置文件。配置中的相对目录以 JSON 文件所在目录为基准，整个项目移动后仍有效；RulesFolder 为空时只做基础校验。`--project` 不能与位置目录参数混用，重复或缺少文件参数会在执行前报错。`--accept-changes` 可用于 codegen 或 export --generate；`--rules-debug` 用于 validate/export，export 时可与 --generate 组合。Windows 自包含工具可直接执行 `ZrConfig.Cli.exe export --project "D:\Game Data\project.zrconfig.json"`。

First --project belongs to dotnet; after -- it belongs to ZrConfig. Relative paths resolve beside JSON and survive moves. Empty RulesFolder means basic checks. Do not mix --project with positional paths; duplicate/missing file arguments fail early. --accept-changes supports codegen/export --generate; --rules-debug supports validate/export, including combined export. Windows may run `ZrConfig.Cli.exe export --project "D:\Game Data\project.zrconfig.json"`.

项目文件继续使用 `DataFolder`、`OutputFolder`、`CodeFolder`、`RulesFolder` 和可选 `Target`；1.4.0 保存可选的 `Name` 与 `FormatVersion: 1`，旧文件未写这两个字段仍可读取。最近项目和界面偏好保存在本机，不随项目 JSON 共享。

Existing DataFolder/OutputFolder/CodeFolder/RulesFolder/optional Target remain. 1.4.0 adds optional Name and FormatVersion: 1; old files still read. Recent-project/UI preferences stay local, not shared in project JSON.

### 只校验与诊断报告

**Validation-only and reports**

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- validate --project samples/project.zrconfig.json
# 诊断报告放在 Data、Config、Rules、Output 之外
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- validate --project samples/project.zrconfig.json --report /tmp/zrconfig-validation.json
```

**示例英文说明 / English notes:**

- Keep reports outside Data, Config, Rules, and Output


位置参数形式为 `validate <Data> <ConfigRoot> [Rules]`，没有 Output 参数。它使用当前代码执行数据解析、基础检查、业务规则及二进制回读验证；不写 Config 或所选 Output、不自动生成。临时编译和回读文件写入系统临时目录。代码缺失、支持源码待更新或结构变化时返回 3，表示完整校验未完成；先核对并执行生成，再重新校验。通过只校验不表示磁盘发布成功，交付仍需 export。

Positional form: validate &lt;Data&gt; &lt;ConfigRoot&gt; [Rules], no Output. Current code parses/checks/runs rules/binary read-back without writing Config/selected Output or generating. Temporary files use system temp. Missing code/outdated support/schema returns 3, meaning incomplete full validation: review/generate/revalidate. Validation success is not publication; delivery needs export.

`validate` 支持 `--target C|S`、`--rules-debug` 和 `--report`；`export` 也支持 `--report`。报告为 JSON，保存操作状态、诊断位置和已有的运行/版本编号，不附带工作簿或源码全文；已知项目根路径替换为占位符，业务消息仍需在分享前检查。export 在生成预检查阶段就停止时不会进入导出报告保存步骤，保留终端输出或使用 App 的“导出诊断报告”。

validate supports --target C\|S, --rules-debug, --report; export also supports --report. JSON reports contain state, locations, existing run/version IDs, not full source/workbooks. Known roots are placeholders; review business messages before sharing. Export stopped during generation prechecks never reaches export-report writing; retain terminal output or App reports.

Ctrl+C 请求安全取消。第三方解析或自定义规则返回后才会到达下一个取消检查点；已提交的生成代码不会因随后取消而撤销，已完成的数据激活仍按成功处理。完整边界见 [操作边界](release-and-acceptance.md#操作边界)。

Ctrl+C requests safe cancellation. Third-party parsing/custom rules return before the next checkpoint. Committed code stays; completed activation reports success. See [boundaries](release-and-acceptance.md#操作边界).

### 一步生成并导出

**Generate and export in one step**

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- export --generate --project samples/project.zrconfig.json
```

工具先只读检查结构、类型、支持源码和已接受的公开 API，列出必要的生成变更；首次生成或未改变受保护接口的更新可直接完成，然后读取当前 Excel、完整校验并导出。代码没有变化时跳过生成写入。改变输入根目录或删除生成文件时仍会停止，退出码为 1；核对清单后再添加 `--accept-changes`。涉及 API 删除或签名变化还需要单独迁移，普通确认不放行。生成失败不会进入导出；导出校验失败保留 current.json，但此前已成功应用的代码生成不会自动回滚。

The tool read-only checks structure/types/support/accepted APIs and lists changes. Initial or compatible generation may proceed, then current Excel is fully validated/exported. Unchanged code skips writes. Root changes/deletions stop with exit 1; review then add --accept-changes. API removals/signatures need separate migration. Generation failure prevents export; export failure preserves current.json but not rollback of already committed code.

字段增加也可能改变业务调用的公开构造函数。遇到 `API_CHANGE_REQUIRED` 时，由程序员按 [公开接口保护](server-generation.md#公开接口保护与显式迁移) 验证旧消费方并取得迁移凭据，再使用相同生成选项执行 `codegen ... --accept-api-change <预览摘要> --migration-receipt <凭据文件>`，之后重新导出。摘要与凭据只适用于本次变更，不存入项目配置。`Config/.schema.json` 与 Generated 一起保存，不能通过删除基线来绕过检查。App 对这类变化只展示清单并允许取消。

Added fields may change public constructors. For `API_CHANGE_REQUIRED`, developers follow [public API protection](server-generation.md#公开接口保护与显式迁移) to verify old consumers and obtain a migration receipt, then run `codegen ... --accept-api-change <preview-digest> --migration-receipt <receipt-file>` with the same generation options before exporting again. The digest and receipt apply only to this change and are not saved in project settings. Version `Config/.schema.json` with Generated; deleting the baseline cannot bypass checks. The App displays changes and allows cancellation only.

App 与这个组合入口共用 `ProjectWorkflow`，App 通过确认窗口决定是否继续，CLI 通过上述明确选项执行。普通 `export` 不会写生成源码，仍适用于只允许检查已提交代码的自动化。`export --accept-changes` 未同时指定 `--generate` 会报参数错误。

App/combined CLI share ProjectWorkflow; App uses confirmation, CLI explicit options. Plain export never writes generated source, useful for validating committed code only. export --accept-changes without --generate is invalid.

### 按端生成和导出

**Generate/export by side**

字段标记和表头声明见 [C / S / CS 填写规则](config-types.md#客户端与服务端字段)。`codegen`、`validate` 和 `export` 都接受 `--target C` 或 `--target S`；省略时使用项目文件的 `"Target": "C"` / `"Target": "S"`，未指定目标端时默认 C。命令行显式指定的端优先，不改写已保存项目配置。`CS` 是字段标记，不能作为一次生成/导出的目标端。

See [C/S/CS](config-types.md#客户端与服务端字段). codegen/validate/export accept --target C/S, otherwise project Target, otherwise C. Explicit CLI overrides do not rewrite settings. CS is a field marker, not an operation target.

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- codegen --project samples/project.zrconfig.json --target C
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- export --project samples/project.zrconfig.json --target C --generate
```

服务端将 `--target C` 改成 `--target S`。Windows 的 `ZrConfig.Cli.exe` 使用相同参数。一次操作只生成和发布所选端，`Generated` 与 `Output/current.json` 表示当前这次操作的结果；需要同时交付两端时，各端使用实际项目的代码和输出路径，不能把同一份生成代码当作两套模型共用。正式示例继续共用现有 Data、Config、Rules、Output，未增加平行示例目录。

Replace C with S for servers; Windows arguments are identical. One operation generates/publishes one side; Generated/current.json represent that result. Delivering both sides requires actual per-side project code/output paths; one generated model set cannot serve both. Official samples retain one shared directory set.

生成的字段、构造函数、编解码器与查询接口均按端一致；所有表的结构哈希包含目标端，使用另一端代码直接导出会报 `SCHEMA_MISMATCH` 并保留当前发布版本。`current.json` 增加 `target`（C/S），便于交付核对。旧生成代码需重新生成后使用。

Fields/constructors/codecs/queries consistently follow target. Schema hashes include side; wrong-side export yields SCHEMA_MISMATCH and preserves Current. current.json target C/S supports delivery checks. Regenerate old code.

使用 Core API 时，`ModelGenerator.Generate/Prepare`、`ExportPipeline.RunWithSources/RunWithRules` 的可选 `target` 参数接受 `ConfigTarget.Client` 或 `ConfigTarget.Server`；仅程序集导出使用 `ExportPipeline.Run(data, output, assembly, target, validators)`。`ProjectWorkflow` 使用 `WorkspaceSettings.Target`。业务规则按当前端模型编译和校验，访问另一端独有字段会导致编译失败；需要不同规则时使用对应端的规则路径。

Core Generate/Prepare and RunWithSources/RunWithRules optional target accept ConfigTarget.Client/Server. Assembly-only uses ExportPipeline.Run(data, output, assembly, target, validators); ProjectWorkflow uses WorkspaceSettings.Target. Rules compile against current-side models; excluded-field access fails. Use corresponding rule paths when different sides need different rules.

### 13.1 生成代码

**13.1 Generate code**

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- codegen "/你的路径/ZrConfig-Beginner/Data" "/你的路径/ZrConfig-Beginner/Config"
```

**示例英文说明 / English notes:**

- Replace /你的路径/ with your actual exercise-project path.


参数顺序：操作名 codegen → Excel 目录 → 代码根目录。

Order: codegen → Excel directory → code root.

生成器记住这套 Config 的完整输入根目录。输入根目录变化或将删除旧生成文件时，默认停止并列出新旧路径与待删文件；请先排除误选 Data 子目录。没有 API 变化时，可核对后添加 `--accept-changes`，或在 App 选择“确认生成”；删除公开表等 API 变化必须先完成上文的单独迁移。预览不会升级支持源码、创建 Config 或写入磁盘缓存；只在进程内复用未变化的类型编译产物，确认执行后才写项目。App 导出前也执行这些检查，仅在无需 API 迁移时提供“生成并导出”。旧项目仍检查待删文件与基线；整项目一起移动时相对路径绑定有效。

Config remembers its complete input root. Changed roots/deletions stop with old/new paths and deletion list; rule out mistaken subfolders first. Without API changes, review and accept; public-table removal needs migration. Preview neither upgrades support, creates Config, nor writes disk caches; unchanged type compilations reuse only process memory. App pre-export follows this and offers combined generation only without migration. Old projects still check files/baselines; relative bindings survive whole-project moves.

### 13.2 带规则导出

**13.2 Export with rules**

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export "/你的路径/ZrConfig-Beginner/Data" "/你的路径/ZrConfig-Beginner/Output" "/你的路径/ZrConfig-Beginner/Config" "/你的路径/ZrConfig-Beginner/Rules"
```

**示例英文说明 / English notes:**

- Replace /你的路径/ with your actual exercise-project path.


参数顺序：export → Excel 目录 → 输出目录 → 代码根目录 → 规则目录。

Order: export → Excel → output → code root → rules.

CLI 会拒绝未知、重复、不适用的选项和多余路径，退出码为 2，并且不会开始生成或导出。`--help` 查看用法，`export --rules-debug` 开启规则调试；目前不支持 `--dry-run`，传入它会报错。路径以 `-` 开头时，在路径前加工具自身的 `--`，其后的参数全部按路径处理。

Unknown/duplicate/inapplicable options and excess paths fail with 2 before work. --help shows usage; --rules-debug enables debugging. --dry-run is unsupported. For paths beginning -, add the tool's own --; all subsequent arguments are paths.

### 13.3 不使用业务规则

**13.3 No business rules**

明确指定 Config，省略 Rules：

Explicit Config, omitted Rules:

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export "/你的路径/ZrConfig-Beginner/Data" "/你的路径/ZrConfig-Beginner/Output" "/你的路径/ZrConfig-Beginner/Config"
```

**示例英文说明 / English notes:**

- Replace /你的路径/ with your actual exercise-project path.


这样不回退到仓库示例规则。基础类型、结构、直接表对象关联和编解码检查仍然执行，但不会替你执行 HeroRules 等业务条件。

No fallback to sample rules. Basic types/schema/references/codecs still run, but HeroRules/business constraints do not.

### 13.4 已经构建好的 CLI

**13.4 Already-built CLI**

先构建：

Build first:

```bash
bash scripts/dotnet.sh build src/ZrConfig.Cli -p:NuGetAudit=false
```

再调用 DLL：

Then call the DLL:

```bash
bash scripts/dotnet.sh src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll codegen "Data路径" "Config路径"
bash scripts/dotnet.sh src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll export "Data路径" "Output路径" "Config路径" "Rules路径"
```

**示例英文说明 / English notes:**

- Replace Data路径 and Config路径 with actual Data/Config paths.
- Replace the four path placeholders with actual Data/Output/Config/Rules paths.


| 退出码<br>Exit code | 含义<br>Meaning | 自动化处理<br>Automation |
| --- | --- | --- |
| 0 | 本次命令成功<br>Command succeeded | 只有成功的 export 才表示本轮发布完成<br>Only successful export means new publication |
| 1 | 生成、校验或导出检查未通过，或生成确认未接受<br>Generation/validation/export checks failed or confirmation absent | 查看诊断，停止后续发布<br>Read diagnostics; stop publication |
| 2 | 参数错误或执行异常<br>Invalid arguments/execution exception | 修正命令、路径或运行环境<br>Fix command/paths/environment |
| 3 | 只校验需要先生成代码，完整校验未完成<br>Validation requires generation; incomplete | 核对结构变更，生成后重新校验<br>Review schema, generate, revalidate |
| 130 | 取消请求被处理<br>Cancellation handled | 查看是否已提交代码，不能当作数据发布成功<br>Check committed code; not publication success |

帮助、只校验、版本预览命令返回 0 都不代表执行了新数据导出。版本管理使用 `release list/diff/rows/rollback/pin/unpin/cleanup`；回滚与清理默认预览，核对后加 `--apply`。命令与兼容条件见 [发布管理](release-and-acceptance.md#比较回滚与清理)。当前没有 watch 或通用 `--dry-run` 选项，不支持从单个工作簿增量导出替代全表版本。

Help/validation/version previews returning 0 do not export data. release list/diff/rows/rollback/pin/unpin/cleanup manage versions; rollback/cleanup preview until --apply. See [compatibility](release-and-acceptance.md#比较回滚与清理). No watch/general dry-run or incremental single-workbook replacement of full versions.

位置参数必须完整提供：`codegen <Data> <ConfigRoot>`、`validate <Data> <ConfigRoot> [Rules]`、`export <Data> <Output> <ConfigRoot> [Rules]`；不能只给 Config 或省略中间的目录。在仓库内快速运行时，codegen/validate/export 完全不传目录仍使用 samples 默认值；独立工具需要显式目录或 `--project`。显式给出 Config 并省略 Rules 时不启用业务规则。release 必须使用 `--project`，不接受位置目录参数。自动化优先使用项目配置，或明确传入整套目录。

Positional paths must be complete: codegen Data ConfigRoot; validate Data ConfigRoot [Rules]; export Data Output ConfigRoot [Rules]. Do not omit intermediate paths. Inside repositories, no paths at all uses sample defaults; standalone tools need explicit paths/project. Explicit Config without Rules disables business rules. release requires --project and no positional directories. Prefer project settings or complete explicit paths.

### 13.5 规则断点调试

**13.5 Rule breakpoints**

validate 或 export 命令最后增加 `--rules-debug`，控制动态 Config/Rules 的 Debug 编译与 Portable PDB。它与工具项目的 Debug 构建配置是两回事：

Append --rules-debug to validate/export for dynamic Config/Rules Debug and Portable PDB, distinct from tool-project Debug:

```bash
bash scripts/dotnet.sh src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll export "Data路径" "Output路径" "Config路径" "Rules路径" --rules-debug
```

**示例英文说明 / English notes:**

- Replace all four path placeholders with actual project directories.


Rider 使用 .NET Project 调试配置选择 ZrConfig.Cli；Visual Studio 将 ZrConfig.Cli 设为启动项目，在调试启动配置中填写 export 与四个路径，以 Debug/F5 启动。在规则 Validate/ValidateTable 的可执行语句设断点。也可 Attach to Process 附加实际 CLI 的 .NET 进程。Debug/Release 动态编译缓存分开，日志保留完整异常和源码位置。

Rider selects CLI in .NET Project debugging. Visual Studio sets CLI startup/arguments and starts Debug/F5. Break on executable Validate/ValidateTable statements or attach to the actual CLI .NET process. Dynamic Debug/Release caches are separate; logs retain full exceptions/source locations.

逐步操作、Windows PowerShell 命令、断点未绑定、异常中断与 Unity 进程选择见 [完整调试指南](debugging.md)。

See [debugging](debugging.md) for detailed IDE/PowerShell/unbound-breakpoint/exception/Unity-process steps.

CLI 不会为了等待你附加调试器而自动暂停。进程很快结束时，要安排好附加时机；是否命中断点还取决于实际 IDE 调试环境。普通配表不需要这个选项，App 界面也不提供它。

CLI never pauses automatically for attachment. Arrange timing or launch through IDE; actual breakpoint behavior depends on the environment. Everyday authoring needs no flag; App does not expose it.

### 13.6 Windows PowerShell

使用自包含 Windows 包时，保留发布目录中的所有文件，把下例的工具目录和四个项目路径改为本机路径：

Keep all files in self-contained Windows packages. Replace tool/four project paths below:

```powershell
$zrTool = "C:\Tools\ZrConfig"
& "$zrTool\ZrConfig.Cli.exe" codegen "D:\Game Data\Data" "D:\Game Data\Config"
if ($LASTEXITCODE -ne 0) { throw "生成失败，先处理诊断。" }
& "$zrTool\ZrConfig.Cli.exe" export "D:\Game Data\Data" "D:\Game Data\Output" "D:\Game Data\Config" "D:\Game Data\Rules"
if ($LASTEXITCODE -ne 0) { throw "导出失败，不发布本次数据。" }
```

**示例英文说明 / English notes:**

- Generation failed; resolve diagnostics first.
- Export failed; do not publish this run's data.


使用源码工程时，在仓库根目录调用 SDK，无需 Bash：

From source, run SDK at root without Bash:

```powershell
dotnet run --project src/ZrConfig.Cli -p:NuGetAudit=false -- codegen "D:\Game Data\Data" "D:\Game Data\Config"
dotnet run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export "D:\Game Data\Data" "D:\Game Data\Output" "D:\Game Data\Config" "D:\Game Data\Rules" --rules-debug
```

`--rules-debug` 只在需要规则断点/PDB 时添加，普通导出可省略；正式自动化同样应检查每次命令的退出码。安装 SDK 与 Windows 打包入口见 [源码构建与升级](development.md)。Windows 路径不能原样粘到 macOS，换用该电脑实际的项目路径。

Add --rules-debug only for breakpoints/PDB. Automation checks every exit code. See [SDK/Windows packaging](development.md). Windows paths cannot be pasted unchanged on macOS; use local paths.

## 14. Unity 运行时如何使用

**14. Unity runtime integration**

本章由项目程序员完成 Unity 首次接入；配表人员按 [日常使用文档](user-guide.md) 导出并交付数据即可。

Developers perform initial integration; authors [export/deliver](user-guide.md).

### 14.1 导表完成不等于游戏自动更新

**14.1 Export does not automatically update games**

生成的 C# 代码要在游戏工程中编译。导出的数据要放到游戏能访问的位置。游戏需要执行加载或重新加载。

Compile generated C# into the game, deploy readable data, then load/reload explicitly.

你修改 Excel 并在仓库 Output 导出成功，已经运行的游戏不会凭空知道那个文件在哪里，也不会自己去读它。

A running game cannot infer or read a new repository Output file automatically.

### 14.2 接入 Unity 源码

**14.2 Integrate Unity source**

在仓库根目录运行：

Run from root:

```bash
bash scripts/package-unity.sh
```

将输出目录中的 `Assets/ZrConfig` 及同级 `ZrConfig.meta` 放入目标工程的 Assets，保留随附 LICENSE。默认输出是 `artifacts/unity/ZrConfig-source`，重复导出时请给脚本传入新的输出目录，避免覆盖已有源码。

Copy output Assets/ZrConfig and sibling ZrConfig.meta into target Assets, retaining LICENSE. Default artifacts/unity/ZrConfig-source; choose new output on repeats to avoid overwriting source.

Runtime、Validation、Unity 适配器和 Editor 查看器全部为源码，由各自 asmdef 编译；Validation 和 Unity/Editor 仅在编辑器中使用。业务配置程序集的 asmdef 引用 `ZrConfig.Runtime`，规则程序集引用 `ZrConfig.Runtime`、`ZrConfig.Validation` 和配置程序集，限定 Editor。不添加 ZrConfig 的 Package 或预编译 DLL；从旧方式升级时先移除旧 `com.zrconfig.unity` 依赖和同名 DLL，避免重复程序集。业务 HeroConfig/SkillConfig 仍由项目生成，不携带 Excel 读取库。

Runtime/Validation/adapters/viewer compile from source via asmdefs; Validation/Unity.Editor are Editor-only. Models reference Runtime; rules reference Runtime/Validation/models and target Editor. Add no ZrConfig Package/precompiled DLL. Remove old com.zrconfig.unity/same-name DLLs on upgrade to avoid duplicate assemblies. Generate business Hero/Skill in your project; no Excel parser ships to runtime.

仓库提供可直接运行的 [Unity 示例工程](../samples/README.md#unity-可运行示例)，共用正式 samples 的配置和导出数据。先体验示例可运行 `bash scripts/maintain.sh unity-sample`；将 ZrConfig 接入自己的工程时，继续按下面步骤操作。

A [runnable Unity sample](../samples/README.md#unity-可运行示例) shares official configuration/exports. Try unity-sample first; follow below for your own project.

### 14.3 把代码和规则放到正确的位置

**14.3 Place code/rules correctly**

一种易懂的布局：

One clear layout:

```text
YourUnityProject/
  Data/                                  Excel，在 Assets 外也可以 / Excel; may live outside Assets
  Rules/                                 导表业务规则，在 Assets 外 / Export business rules outside Assets
  Output/                                工具发布记录，不直接打入 Player / Tool release records, not directly bundled into Players
  Assets/
    Config/
      CustomTypes/
      Serialization/
      Generated/
    StreamingAssets/
      ZrConfig/                          当前配置清单与版本数据 / Current manifest and versioned data
    Scripts/
      HeroConfigReader.cs                游戏读取示例 / Game-side reading example
```

- Config 根目录选择 `YourUnityProject/Assets/Config`。<br>
  Set the Config root to `YourUnityProject/Assets/Config`.
- Rules 目录选择 `YourUnityProject/Rules`。<br>
  Set the Rules directory to `YourUnityProject/Rules`.
- 输出目录选择 `YourUnityProject/Output`。部署时只将 current.json 及其引用的 bytes 放入 `Assets/StreamingAssets/ZrConfig`，保留版本相对路径。<br>
  Set the output directory to `YourUnityProject/Output`. Deploy only current.json and its referenced bytes to `Assets/StreamingAssets/ZrConfig`, preserving the relative version paths.
- 首次生成后等 Unity 编译结束，再进入 Play。<br>
  After the first generation, wait for Unity to finish compiling before entering Play mode.

规则放在 Assets 外，能避免它们被游戏 Player 编译，也避免游戏程序集不需要的规则依赖。规则仍由导表 App 动态编译执行。

Keeping rules outside Assets excludes them and their dependencies from Player compilation. The export App still compiles and runs them dynamically.

如果规则需要在 Rider 的 Unity solution 中直接编译，可以放在编辑器目录，例如 `Assets/Editor/ConfigRules`，并按项目 asmdef 引用 Runtime/Validation。具体目录方案见 [规则开发](rules-development.md)。

For direct compilation in Rider's Unity solution, put rules in an Editor directory such as `Assets/Editor/ConfigRules`, with appropriate Runtime/Validation asmdef references. See [Rule development](rules-development.md).

使用 asmdef 或热更新程序集时，Config 要归入你实际的配置/热更新程序集并引用 Runtime；规则如参与 Editor 编译，要引用 Validation。避免同时导入旧项目 Models DLL 和新的同名模型源码。

With asmdefs or hot-update assemblies, put Config in your actual configuration/hot-update assembly and reference Runtime. Rules compiled in the Editor must reference Validation. Avoid importing an old Models DLL alongside new source defining the same models.

### 14.4 桌面版 Unity 的最小读取例子

**14.4 Minimal reading example for desktop Unity**

日常只需三步：首次 `UnityConfigRuntime.Load / LoadAsync`，通过返回的快照查询，更新后 `Reload / ReloadAsync`。同步和异步都受支持，按资源系统的真实能力选择。普通生成查询不需要传入 `TableCodecs.Registry`；`IsInitialized` 可判断是否已初始化。`Initialize` 和 `Manager` 供高级工具使用，普通业务不需要直接操作管理器或懒加载器。

Everyday use has three steps: initialize with `UnityConfigRuntime.Load / LoadAsync`, query the returned snapshot, then call `Reload / ReloadAsync` after updates. Choose synchronous or asynchronous calls according to actual resource capabilities. Ordinary generated queries do not require `TableCodecs.Registry`; `IsInitialized` reports initialization state. `Initialize` and `Manager` serve advanced tools; ordinary business code need not manage loaders directly.

本例直接使用仓库正式的 samples，无需先完成第 12 章开发练习：

This example uses the repository's official samples; chapter 12 is not a prerequisite:

1. 按 [示例说明](../samples/README.md) 导出整套 samples/Data（九张表，包含万行用例），确认 0 错误。<br>
   Export all samples/Data following the [sample instructions](../samples/README.md): nine tables, including the 10,000-row case. Confirm zero errors.
2. 按上一节接入 Assets/ZrConfig 源码，把 unity/ZrConfig.Sample/Assets/HotUpdate/Config 下的 CustomTypes、Serialization、Generated 三个源码目录放进 Unity 的 Assets/Config；保持一套源码和唯一的 TableCodecs。<br>
   Integrate Assets/ZrConfig source as described above. Copy CustomTypes, Serialization, and Generated from unity/ZrConfig.Sample/Assets/HotUpdate/Config into Unity's Assets/Config, retaining one source set and one TableCodecs.
3. 按 samples/Output/current.json 的清单，将全部引用的 `.bytes` / `.json` 表数据复制到 Assets/StreamingAssets/ZrConfig 下对应版本路径，最后复制 current.json；不复制日志、编译缓存、发布记录和 `.rows.json`。仓库示例可直接用 `unity-sample` 完成部署。<br>
   Follow samples/Output/current.json to copy every referenced `.bytes` / `.json` table file into its version path under Assets/StreamingAssets/ZrConfig, then copy current.json last. Exclude logs, compilation caches, release records, and `.rows.json`. The repository sample can deploy with `unity-sample`.
4. 等 Unity 编译完成，再添加下面的读取脚本。<br>
   Wait for Unity compilation, then add the reading script below.

项目已有配置时，使用自己项目的一套 Data/Config/Rules；不要把正式示例或第 12 章的同名表混入。

If your project already has configuration, use its own Data/Config/Rules set. Do not mix in tables with the same names from the official samples or chapter 12.

正式 TableReferences.xlsx 的 Hero/Skill 表已经在第 3 行将 Id 标为“主键”。完成生成和导出后，生成的表在加载时自动建立 Id 索引，可以直接使用下面的 Get/TryGet 查询。

Hero/Skill in the official TableReferences.xlsx mark Id as “主键” (primary key) in row 3. After generation and export, loading the generated tables automatically builds Id indexes for the Get/TryGet queries below.

需要 GroupId + Id 或更多字段联合键、取整组时，参见 [联合键与分组查询](table-indexes.md)。

For GroupId + Id, larger composite keys, and group retrieval, see [Composite keys and group queries](table-indexes.md).

创建 `Assets/Scripts/HeroConfigReader.cs`：

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
            // 离开场景时取消读取，不再操作被销毁的对象。
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
            Debug.LogWarning("没有找到英雄：" + id);
            return;
        }

        Debug.Log(hero.Name + "，技能 " + (hero.Skill?.Name ?? "无") +
                  "，导师 " + (hero.Mentor?.Name ?? "无"));
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

**示例英文说明 / English notes:**

- Cancel reads when leaving the scene; do not access destroyed objects.
- The warning means “Hero not found:”, followed by the ID.
- The log prints the hero name and skill; “无” means “none”.
- It also prints the mentor, or “none”.


在场景中创建空 GameObject，把这个脚本挂上去，然后 Play。应显示“火焰法师，技能 火球，导师 火焰法师”。调用 ShowHero(3) 可以查看空引用：技能和导师均为“无”。

Create an empty GameObject, attach this script, and enter Play mode. Expect “Flame Mage, skill Fireball, mentor Flame Mage” in Chinese. ShowHero(3) demonstrates null references: both skill and mentor are “无” (none).

这个脚本是应用入口示范，同一次播放期间只让一个入口调用首次 LoadAsync。不要给十个物体都挂一个会调用 LoadAsync 的脚本，因为静态运行时已经初始化后会拒绝重复初始化。

This script demonstrates an application entry point. Only one entry point should call the initial LoadAsync during a Play session. Attaching initialization scripts to ten objects would attempt duplicate initialization, which the static runtime rejects.

ReloadConfiguration 可以绑定到 UI Button。本例读取的是 Unity 工程中的 StreamingAssets 副本，每次只修改数据时按以下顺序更新：

Bind ReloadConfiguration to a UI Button if needed. This example reads the Unity project's StreamingAssets copy. For each data-only update:

1. 保存 Excel，重新导出到 samples/Output，确认成功。<br>
   Save Excel, export again to samples/Output, and confirm success.
2. 先将本次 current.json 引用的全部 `.bytes` / `.json` 表数据复制到 `Assets/StreamingAssets/ZrConfig` 下对应的 `versions/<版本号>/` 路径，完成后再替换 `Assets/StreamingAssets/ZrConfig/current.json`；管理记录和差异快照留在 Output。<br>
   Copy every `.bytes` / `.json` table referenced by the new current.json to `Assets/StreamingAssets/ZrConfig/versions/<version>/`, then replace `Assets/StreamingAssets/ZrConfig/current.json` last. Keep management records and diff snapshots in Output.
3. 点击 ReloadConfiguration 按钮，重新读取该清单和数据。只更新 samples/Output 而未同步上述文件时，按钮仍会加载旧版本。<br>
   Click ReloadConfiguration to reread the manifest and data. Updating only samples/Output without copying these files still loads the old version.

仓库 Editor 示例直接读取共享的 samples/Output；Player 使用筛选部署后的 StreamingAssets 数据。若改了表结构或类型，还需同步 Config 中对应源码、等待 Unity 编译完成，再重新进入 Play 验证；数据重载不会更新已经编译的 C# 类型。

The repository's Editor sample reads shared samples/Output directly; Players use filtered deployment in StreamingAssets. Schema or type changes also require synchronizing Config source, waiting for Unity compilation, and restarting Play mode. Data reload does not update compiled C# types.

初学解释：

For beginners:

| 代码<br>Code | 意思<br>Meaning |
| --- | --- |
| XxxConfigTable.GetCodec() | 取得本表的共享 Codec；生成查询自动传入<br>Returns this table's shared codec; generated queries pass it automatically |
| TableCodecs.Registry / EnumerateAll() | 可选的全表目录，供全量校验、导出和动态工具查询<br>Optional full-table catalog for complete validation, export, and dynamic tooling |
| FileConfigSource(folder) | 告诉工具去哪个真实文件目录读取<br>Specifies the actual directory to read |
| Load / LoadAsync | 第一次初始化并读取版本清单；同步 / 异步<br>Initializes and reads the version manifest synchronously/asynchronously |
| await | 等这一项异步操作完成，再处理结果<br>Waits for this asynchronous operation before using its result |
| ConfigSnapshot | 固定版本、资源源和按需加载的行/表缓存<br>A fixed version, resource source, and lazily loaded row/table caches |
| GetHeroTable() / GetHeroTableAsync() | 首次加载英雄整表；引用在属性访问时加载，后续复用<br>Loads the whole Hero table once; references load on property access and are reused |
| TryGet(1, out hero) | 尝试查正式示例的英雄 1<br>Attempts to find hero 1 in the official sample |
| Reload / ReloadAsync | 同步 / 异步重读 current.json，切换到新的懒加载快照<br>Rereads current.json synchronously/asynchronously and switches to a new lazy snapshot |
| CancellationToken | 离开场景时通知尚未完成的读取停止<br>Requests unfinished reads to stop when leaving the scene |

生成的英雄表在每次加载时建立一次索引，业务只需获取已有表；热更新后更换表引用。CreateIndex 是显式构建工厂，每次调用都会分配，不能放在逐帧查询中。

Each generated Hero table builds its index once when loaded. Business code retrieves the existing table and replaces its reference after hot updates. CreateIndex explicitly creates a new index and allocates on every call; do not use it in per-frame queries.

#### 同步加载与不依赖 Task 的异步加载

**Synchronous loading and asynchronous loading without Task**

异步接口统一返回项目自带的 `ConfigOperation<T>`，可直接 `await`，不需要 Task、UniTask 或第三方包；也可以声明 `async ConfigOperation<T>` 方法。它只实现配置加载需要的等待、结果、异常和取消，不提供完整的任务调度框架。每次调用得到的操作只 await 一次；未完成时调用 `GetAwaiter().GetResult()` 会报错，不会阻塞线程。立即就绪的资源可以返回 `ConfigOperation<byte[]>.FromResult(bytes)`，回调式加载器可用 `ConfigCompletionSource<byte[]>` 的 `Operation` 和 `TrySetResult` / `TrySetException` / `TrySetCanceled` 接入。

Asynchronous APIs return the built-in `ConfigOperation<T>`, directly awaitable without Task, UniTask, or third-party packages. You may declare `async ConfigOperation<T>` methods. It supports configuration-specific waiting, results, exceptions, and cancellation, not a full task scheduler. Await each returned operation only once. Calling `GetAwaiter().GetResult()` before completion throws instead of blocking. Immediately available resources can return `ConfigOperation<byte[]>.FromResult(bytes)`. Callback loaders can use `ConfigCompletionSource<byte[]>`, its `Operation`, and `TrySetResult` / `TrySetException` / `TrySetCanceled`.

首次加载可以选择同步调用（与上面的异步首次加载二选一）：

Alternatively, initialize synchronously (choose this or the asynchronous initialization above):

```csharp
var source = new FileConfigSource(folder);
ConfigSnapshot snapshot = UnityConfigRuntime.Load(source);
// 后续重载复用上次成功的资源源。
snapshot = UnityConfigRuntime.Reload();
```

**示例英文说明 / English notes:**

- Subsequent reloads reuse the last successful resource source.


异步调用形式保持一致：

The asynchronous form is equivalent:

```csharp
ConfigSnapshot snapshot = await UnityConfigRuntime.LoadAsync(source, token);
snapshot = await UnityConfigRuntime.ReloadAsync(token);
```

普通 .NET 项目使用 `manager.Reload(source)` / `await manager.ReloadAsync(source, token)`。`FileConfigSource.Read` 使用同步文件读取，`ReadAsync` 使用文件流的异步读取。同步入口只调用 `Read`，异步入口只调用 `ReadAsync`；不阻塞等待异步操作，也不把同步 I/O 包装成异步结果。`DelegateSyncConfigSource` 必须分别提供两个委托，例如 `new DelegateSyncConfigSource(files.Read, files.ReadAsync)`。纯内存资源可显式提供 `(path, token) => cachedFiles[path]` 与 `(path, token) => ConfigOperation<byte[]>.FromResult(cachedFiles[path])`。

Ordinary .NET projects use `manager.Reload(source)` / `await manager.ReloadAsync(source, token)`. `FileConfigSource.Read` reads files synchronously; `ReadAsync` uses asynchronous file streams. Synchronous entry points call only `Read`, and asynchronous ones only `ReadAsync`. Neither blocks on async operations nor disguises synchronous I/O as an async result. `DelegateSyncConfigSource` requires separate delegates, such as `new DelegateSyncConfigSource(files.Read, files.ReadAsync)`. For memory resources, explicitly provide `(path, token) => cachedFiles[path]` and `(path, token) => ConfigOperation<byte[]>.FromResult(cachedFiles[path])`.

同步接口要求资源源实现 `ISyncConfigSource.Read`。仅支持网络的源只实现 `IConfigSource.ReadAsync` 即可；上次成功加载的源不支持同步读取时，无参数 `Reload()` 会报错，可显式传入支持同步的源。同步调用会占用当前线程，适合启动阶段的本地文件或已缓存资源。同步和异步重载共用重入保护：已有重载未完成时，再次重载直接报错，不排队，也不读取资源。完成后可以再次调用。

Synchronous APIs require `ISyncConfigSource.Read`. Network-only sources may implement only `IConfigSource.ReadAsync`. Parameterless `Reload()` fails if the last successful source cannot read synchronously; you can explicitly pass a synchronous source. Synchronous calls occupy the calling thread and suit startup files or cached resources. Sync and async reloads share a reentrancy guard: a second reload fails immediately while one is pending, without queueing or reading resources. Retry after completion.

Unity 入口从主线程调用，资源读取的后续调用、await 后续代码与 Changed 通知保留 Unity 同步上下文。清单加载失败或取消时保留原快照与上次成功源。清单成功后即切换版本并触发 Changed，此时不保证所有表文件可用；文件哈希、行结构和引用在实际访问时检查。单行或整表解码失败不会发布本批半成品行，已有行缓存仍可用，可在同一有效快照重试；已经通过哈希与索引校验的表字节独立保留。

Call Unity entry points on the main thread. Subsequent resource calls, await continuations, and Changed notifications retain Unity's synchronization context. Manifest failure or cancellation preserves the previous snapshot and successful source. A valid manifest immediately switches versions and raises Changed; this does not guarantee every table file is usable. File hashes, row structures, and references are checked on access. Failed row/table decoding never publishes partial rows from that batch; existing cached rows remain usable and the same valid snapshot may retry. Table bytes that passed hash/index validation are retained independently.

资源源的 `ReadAsync` 返回 `ConfigOperation<byte[]>`，即时结果使用 `ConfigOperation<byte[]>.FromResult(bytes)`，业务通过 await 等待。此实现不承诺加载零分配：异步状态、文件字节和新表快照仍会分配内存。

`ReadAsync` returns `ConfigOperation<byte[]>`; immediate results use `ConfigOperation<byte[]>.FromResult(bytes)`, and callers await completion. Loading is not allocation-free: asynchronous state, file bytes, and new table snapshots allocate memory.

#### 按行懒加载与跨表引用

**Lazy row loading and cross-table references**

默认 `Load / LoadAsync / Reload / ReloadAsync` 只读取 `current.json` 并检查格式、路径、重复表名/文件名、行数范围和哈希格式，不枚举注册表、不创建各表 Codec 或表缓存容器，也不读取任何 `.bytes`。

Default `Load / LoadAsync / Reload / ReloadAsync` reads only `current.json`, checking format, paths, duplicate table/file names, row-count bounds, and hash format. It does not enumerate the registry, create table codecs/cache containers, or read any `.bytes` files.

每张 `XxxConfigTable` 使用静态字段、显式静态构造函数和普通 `GetCodec()` 方法提供自身 Codec。生成的 `GetHero / GetHeroAsync / GetHeroTable / GetHeroTableAsync` 直接传入该 Codec，不经过中央类型查找，也不需要注册创建工厂或反射。跨表引用只在非空且未缓存时调用目标表的 `GetCodec()`。同一程序集的多个快照复用无状态 Codec；表字节、索引、行对象和引用缓存仍分别属于各自快照。每张表都在 Excel 中显式声明主键并生成索引。生成源码的相关注释使用中文，Codec 读行上下文由显式类保存，不使用捕获局部变量的匿名函数。

Each `XxxConfigTable` exposes its codec through a static field, explicit static constructor, and ordinary `GetCodec()` method. Generated `GetHero / GetHeroAsync / GetHeroTable / GetHeroTableAsync` calls pass that codec directly without central type lookup, factory registration, or reflection. Cross-table references call the target's `GetCodec()` only when non-null and uncached. Snapshots in the same assembly share stateless codecs; bytes, indexes, row objects, and reference caches belong to individual snapshots. Every table explicitly declares its primary key in Excel. Related generated comments are Chinese; an explicit class stores the codec's row-reading context instead of closures capturing local variables.

唯一的 `TableCodecs` 由工具自动生成；新增表后重新生成即可，无需手工修改注册表。`EnumerateAll()` 每段最多枚举 128 张表，用于导出与完整验收。`Registry` 是可选全表目录，取得它不会创建 Codec；该目录只提供 `EnumerateCodecs()`，在显式全量校验时枚举；运行时查询不会自动查目录。业务使用生成入口，通用强类型代码使用 `snapshot.GetRow<HeroConfig, int>(HeroConfigTable.GetCodec(), id)`。无目录初始化支持生成查询；全量校验须传入目录。

The tool generates the sole `TableCodecs`; regenerate after adding tables without manually editing the registry. `EnumerateAll()` enumerates at most 128 tables per segment for export and complete acceptance. `Registry` is an optional full-table catalog whose retrieval creates no codecs. Its `EnumerateCodecs()` is used only for explicit full validation; ordinary runtime queries never search the catalog. Use generated entry points in business code, or `snapshot.GetRow<HeroConfig, int>(HeroConfigTable.GetCodec(), id)` in generic typed code. Generated queries work without a catalog; full validation requires one.

采用新生成代码时须同步更新 Runtime 和 Unity 适配器；本次接口调整不改变 v4 二进制格式。

Update Runtime and Unity adapters together with the new generated code. This API adjustment does not change the v4 binary format.

首次查询某张表时才检查该表是否存在，以及清单结构标识是否匹配当前代码；读取文件后继续检查文件哈希、文件头结构和行索引。未访问表的结构不兼容或缺失不会阻止普通初始化，会在首次查询该表时明确报错。普通重载发布新快照后，不会因为后续某次表查询失败而自动回滚；需要切换前完整验收时使用 `ReloadAll / ReloadAllAsync`。

A table's first query checks manifest presence and schema compatibility; reading its resource also checks the file hash, header, and row index. Missing or incompatible unvisited tables do not block ordinary initialization; they fail explicitly on first access. A later table-query failure does not automatically roll back an already published ordinary reload. Use `ReloadAll / ReloadAllAsync` for complete validation before switching.

每张表仍导出一个资源，二进制包含三个连续区域：

Each table remains one resource, containing three contiguous binary areas:

```text
表头与类型映射 / Header and type map
行索引：主键、引用 Id、数据区内偏移、字节长度 / Row index: primary key, reference Id, data-area offset, byte length
行数据：各行独立编码的内容，跨表引用仍保存 Id / Row data: independently encoded rows; cross-table references still store Id
```

生成器通过 `WriteRow / ReadRow` 统一单行编解码，导出时写入索引和数据。打开字节资源时只解析表头和索引，不解码无关行；索引必须连续、无重叠、无重复主键，并与文件长度匹配。读取单行后还会校验其主键、Id 和数据边界。普通可寻址输出直接写入，不额外缓存整表数据；不可寻址输出先在内存组装再写出。

The generator uses `WriteRow / ReadRow` for row serialization and writes indexes plus data during export. Opening a resource parses only its header/index, without decoding unrelated rows. Index entries must be contiguous, non-overlapping, unique by primary key, and consistent with file length. Decoded rows are checked against their primary key, Id, and data bounds. Seekable output is written directly without an extra full-table buffer; non-seekable output is assembled in memory first.

生成的单行接口按 Excel 声明的主键定位，支持同步和异步。业务优先使用 `GetHero/GetHeroAsync` 等生成入口；通用代码使用显式 Codec 和 `GetRow<T, TKey>`，键始终以实际类型传递。按 `Type` 查询的动态工具入口同样使用 object，不适合每帧查询：

Generated row APIs locate Excel-declared primary keys synchronously or asynchronously. Prefer generated methods such as `GetHero/GetHeroAsync`; generic code uses explicit codecs and `GetRow<T, TKey>` with actual typed keys. Dynamic tooling queries by `Type` use object and are unsuitable for per-frame access:

```csharp
ConfigSnapshot configs = await UnityConfigRuntime.LoadAsync(source, token);
HeroConfig hero = await configs.GetHeroAsync(1, token);
// 此时只创建英雄这一行，不加载技能、导师或后续引用。
SkillConfig skill = await hero.GetSkillAsync(token);
Debug.Log(skill?.Name);

// 引用属性命中缓存直接返回；未命中则同步加载目标行并缓存。
// 未读取的表资源要求 source 支持 ISyncConfigSource。
Debug.Log(hero.Skill?.Name);
// TryGet 只检查缓存，不触发资源读取或反序列化。
bool ready = hero.TryGetCachedMentor(out HeroConfig mentor);

// 已缓存行可直接同步获取。
HeroConfig sameHero = configs.GetHero(1);
SkillLevelConfig skillLevel = await configs.GetSkillLevelAsync(101, 1, 2, token);
// 通用接口；组合键使用 ValueTuple，生成接口提供强类型参数。
HeroConfig sameRow = await configs.GetRowAsync<HeroConfig, int>(HeroConfigTable.GetCodec(), 1, token);
```

**示例英文说明 / English notes:**

- Only this Hero row is created; skill, mentor, and further references remain unloaded.
- A cached reference returns immediately; a miss loads and caches its target row synchronously.
- Reading an unopened table resource requires ISyncConfigSource.
- TryGet checks caches only, without resource reads or deserialization.
- Cached rows can be retrieved synchronously.
- Generic API: composite keys use ValueTuple; generated APIs provide typed parameters.


资源由项目自己的 AB/Addressables/其他适配器通过 `IConfigSource` 返回字节，配置运行时不内置 AB 框架。AB 适配层负责定位、下载、请求合并、调度、调用方取消与资源句柄释放；配表只协调行索引、解码和对象缓存。源返回的托管字节在有效快照使用期间须保持不变且持续有效；索引直接保留这些字节以支持后续按行解码，释放 AB 句柄不能让这些字节失效。若资源源返回整表二进制，首次访问仍读取并校验整张表的字节，但只反序列化当前请求行；这不代表 AB 按行读取，也不会节省这一整份字节的驻留内存。懒加载减少行对象、复杂成员与查询索引的创建。

Project-owned AB/Addressables/other adapters return bytes through `IConfigSource`; the configuration runtime embeds no AB framework. Adapters own addressing, downloading, request coalescing, scheduling, caller cancellation, and handle release. Configuration owns row indexes, decoding, and object caches. Returned managed bytes must remain immutable and valid while an active snapshot uses them: indexes retain them for future row decoding, so releasing an AB handle must not invalidate them. With whole-table binary resources, the first access still reads and hashes all bytes but deserializes only the requested row. This is not AB-level row reading and does not reduce retained raw-byte memory; it reduces row objects, complex members, and query indexes.

引用字段保留目标行的实际类型，例如 `SkillConfig? Skill`。getter 先返回属性内的对象缓存；未命中时向本快照查询，必要时读取目标表资源、解码目标行，成功后写入属性缓存。加载目标行只绑定它自己的延迟解析器，不自动访问它的引用。因此 `Hero → Skill → Effect → Item` 中，读取 Hero 不请求 Skill；访问 `hero.Skill` 才加载 Skill，也不会自动加载 Effect 或 Item。链式访问会依次加载业务实际访问到的每一层，重复访问复用同一对象且不产生查询分配。

Reference fields retain actual target types, such as `SkillConfig? Skill`. The getter returns its property cache first; otherwise it queries this snapshot, reads target bytes and decodes the row if necessary, then caches success. Target rows bind their own lazy resolvers without accessing further references. Reading Hero in `Hero → Skill → Effect → Item` does not request Skill; accessing `hero.Skill` loads Skill without loading Effect or Item. Chained access loads only each actually visited level. Repeated access reuses the same object without query allocations.

每个引用同时生成 `GetSkillAsync(token)` 和 `TryGetCachedSkill(out skill)`。异步方法通过资源源异步读取，成功后与同步属性共用缓存。TryGetCached 对空引用、尚未加载的目标返回 false，不触发读取或解码；若索引已加载并确认目标 ID 无效，则报告引用错误。同步属性 `hero.Skill` 在缺少缓存时会同步加载；只想探查缓存时用 TryGetCached。业务字段与生成方法重名时，生成器为辅助成员添加数字后缀。

Each reference also generates `GetSkillAsync(token)` and `TryGetCachedSkill(out skill)`. Async access reads asynchronously and shares successful results with the synchronous property. TryGetCached returns false for null or unloaded targets without I/O or decoding; if an opened index proves the target ID invalid, it reports a reference error. The synchronous `hero.Skill` getter loads on a miss; use TryGetCached to inspect caches only. Naming collisions add numeric suffixes to generated helpers.

边界约定：0/空白引用返回 null，不访问资源；非零 ID 不存在时抛出含来源行、字段和目标 ID 的 ConfigReferenceException；文件缺失、哈希错误、解码失败及取消不缓存为 null，下一次调用可重试。缺失引用的行可先加载自身数据，错误在访问该引用时暴露；导出仍检查全部引用，显式全量验收也会检查未访问的引用。每个属性只缓存成功的对象；循环引用回到已缓存的行，不递归展开整条链。

Zero/blank references return null without resource access. Missing nonzero IDs throw ConfigReferenceException containing the source row, field, and target ID. Missing files, hash errors, decoding failures, and cancellation are not cached as null, so later calls can retry. A row can load before its missing reference is discovered on access. Export and explicit full acceptance still check all references, including unvisited ones. Properties cache only successful objects; cycles return cached rows instead of recursively expanding the chain.

资源源只支持异步且目标表字节尚未加载时，同步属性不能完成该读取，会提示调用 `GetSkillAsync`；不阻塞等待异步操作。若该表字节已缓存，同步属性可以直接解码另一行，无需资源源提供同步 I/O。不同表可以独立请求资源；配表不维护跨表资源等待队列。同表并发查询通过表级解码协调复用同一行对象，但可能多次调用资源接口，同路径资源请求合并由 AB 适配层完成，也可包装内置的 `CoalescingConfigSource` / `CoalescingSyncConfigSource`。适配层应让每个调用方独立取消等待，不能因为一个调用方取消而中断其他使用者。同步查询仍只使用同步源，不等待另一个异步查询；已有表字节则直接解码。

With an async-only source and unopened target bytes, synchronous reference access fails with guidance to use `GetSkillAsync`; it never blocks on async operations. Cached table bytes allow synchronous decoding of another row without synchronous I/O. Tables request resources independently, without a cross-table resource queue. Per-table decode coordination shares row identity across concurrent queries, although resource APIs may be called multiple times. Adapters own same-path coalescing, optionally using `CoalescingConfigSource` / `CoalescingSyncConfigSource`. Each caller must be able to cancel its own wait without aborting other callers. Synchronous queries use only synchronous sources and never wait for another async query; existing bytes allow direct decoding.

`GetHeroTable / GetHeroTableAsync` 显式加载该表全部行，构建完整主键/分组索引，并复用先前加载的单行对象；整表加载也不展开跨表引用。同步入口支持 `configs.GetHeroTable(token)`，与异步入口一样传递取消；原 `GetHeroTable()` 和无参方法组继续可用。分组查询和枚举使用整表接口；单行接口不会返回不完整的分组。每张表均声明主键并提供单行查询。

`GetHeroTable / GetHeroTableAsync` explicitly loads all rows, builds complete primary/group indexes, and reuses previously loaded row objects without expanding references. Synchronous `configs.GetHeroTable(token)` supports cancellation like the async entry point; the original parameterless overload remains for method-group compatibility. Group queries require the complete table.

`snapshot.LoadedRowCount` 是已成功解码并绑定解析器的行数；`snapshot.Tables` 只列出已显式加载完整的表，读取它不会触发加载。重载成功后旧快照失效并清空内部行、表和字节缓存；旧快照查询及旧行引用访问（含缓存命中）会抛出 `InvalidOperationException`，在途旧读取也不能发布结果。业务收到更新通知后统一获取新对象。已取得的普通字段仍是旧值；业务自行保留的对象不会被强制回收，不提供独立行/表卸载。编辑器查看器不执行引用 getter，避免单纯展开表格就触发加载。

`LoadedRowCount` counts decoded, bound rows. `Tables` lists completed tables without loading more. Successful reload invalidates the old snapshot and clears bytes, row caches, and tables. Old queries and reference access throw even for cached references; in-flight work cannot publish to the old snapshot. Reacquire objects from the new snapshot. Ordinary fields on old objects retain their values; held objects are not forcibly collected, and individual row unloading is unsupported. The viewer does not invoke reference getters.

版本发布与旧快照失效共用一次身份切换；旧缓存清理等待期间，新 `Current` 仍可查询。后台解码完成后恢复调用线程时会再次检查取消与快照有效性。若 `Changed` 监听器再次重载，后续监听器只接收仍有效的当前版本通知，不会在新版本后再收到旧通知；监听器结束后需要查询时仍应重新获取 `Current`。

Publishing the new Current and invalidating the old snapshot form one version transition. Old cleanup waits until the new Current is usable. Continuations recheck validity after returning to their context. Reentrant Changed handling does not deliver stale notifications; retrieve Current again after callbacks.

自行创建的 `ConfigManager` 可通过 `Dispose()` 永久结束使用，使已发布快照和预加载候选失效，并拒绝之后的重载。Unity 播放模式重置会自动释放当前及初始化中的管理器；重置前的在途加载迟到后会失败，不能重新发布或成功返回旧会话快照。资源源的句柄与底层请求生命周期仍由资源适配层管理。

Disposing a manager permanently invalidates current/candidate snapshots and rejects further use. Resetting Unity runtime disposes active/initializing managers; late initialization fails. Resource adapters own their handles.

工具需要全量验收时，先用 `new ConfigManager(TableCodecs.Registry, 清单解析方法)` 构造管理器，再调用 `manager.ReloadAll / ReloadAllAsync`，枚举生成目录、核对完整表集合并检查全部结构、数据和引用，全部验证成功后才替换快照；失败保留旧快照。`snapshot.LoadAll / LoadAllAsync` 执行同样的全量校验，在现有快照逐表预热。业务默认使用按行接口。

For full acceptance, construct ConfigManager with the Registry and manifest decoder, then call ReloadAll. It validates the complete table set, schemas, data, and references before atomically switching; failure retains the old version. Snapshot LoadAll prewarms the current snapshot. Ordinary defaults remain row-lazy.

可选的 `ConfigPreloadPlan` 与 `ReloadPrepared/ReloadPreparedAsync` 在关键行/表准备成功后再切换版本；失败或取消保留旧快照。具体使用见下方 [预热与加载诊断](#预热与加载诊断)。

Optionally use a preload plan with ReloadPrepared: publish only after critical loads succeed, preserving the old version on failure. See the following section.

行索引使用表二进制 **v4**，旧 v2 数据及此前的 v3 试验格式不能直接加载。升级时一起重新生成代码、导出数据并更新 Runtime、共享 Serialization 与 Unity 适配器；保留历史发布目录，勿用新读取器混读旧数据。

v4 does not support old v2/v3 files. Regenerate and export, update Runtime/Serialization/Unity code together, preserve old releases, and do not mix formats.

#### 预热与加载诊断

**Prewarming and diagnostics**

先决定何时允许切换版本，再选择入口：

Choose when to switch versions according to your requirements:

| 需求<br>Requirement | 入口与检查范围<br>Entry point |
| --- | --- |
| 尽快打开新版本，业务自行处理后续加载错误<br>Switch as soon as possible | 默认 Load / Reload：先读取清单，表与行在访问时检查<br>Default manifest-only reload; table errors appear on later access |
| 关键配置可用后再切换<br>Prepare critical data before switching | ReloadPrepared：指定的行/表成功加载后切换，不自动展开引用<br>ReloadPrepared with explicit rows/tables; references are not expanded |
| 所有表及引用通过后再切换<br>Validate everything before switching | 独立 ConfigManager + TableCodecs.Registry + ReloadAll：完整枚举、校验后切换<br>Manager ReloadAll with Registry |
| 提前加载当前版本的某张表<br>Prewarm the current version | 当前 snapshot 的 GetXxxTableAsync；不会重新发布版本<br>GetTableAsync without republishing |

下面放在 Unity 主线程发起的异步初始化方法中，使用 `ZrConfig.Runtime`、`ZrConfig.Models` 和 `ZrConfig.Unity` 命名空间。`source` 为实际资源源，`token` 为当前加载操作的取消令牌；这段替代首次 `LoadAsync`，不要重复初始化：

Initialize asynchronously on the main thread. Import the required namespaces and replace source/token with actual values. This replaces the first Load; do not initialize twice.

```csharp
var options = new ConfigLoadOptions(enableDiagnostics: true);
UnityConfigRuntime.Initialize(options);

var plan = new ConfigPreloadPlan()
    .WithRow<HeroConfig, int>(HeroConfigTable.GetCodec(), 1)
    .WithRow<SkillConfig, int>(SkillConfigTable.GetCodec(), 10)
    .WithTable<DropConfig>(DropConfigTable.GetCodec());

var snapshot = await UnityConfigRuntime.ReloadPreparedAsync(source, plan, token);
var hero = snapshot.GetHero(1); // 已预热，复用行对象。
ConfigLoadDiagnostics diagnostics = snapshot.Diagnostics;
UnityEngine.Debug.Log($"已加载 {snapshot.LoadedRowCount} 行，表字节 {diagnostics.RetainedBytes}");
```

**示例英文说明 / English notes:**

- The prewarmed Hero row is reused.
- The log reports loaded rows and retained table bytes.


后续更新只调用 `ReloadPreparedAsync`，不再 Initialize。`ConfigPreloadPlan` 不可变，每次 `WithRow` / `WithTable` 返回新计划；忽略返回值就不会加入该步骤。计划只包含显式列出的行/表，不因 Hero 引用 Skill 就自动预热 Skill，也不等同于全量引用校验。若需要该目标就明确加入计划。首次失败时尚无有效快照；已有快照时，准备失败或取消保留原版本，修复后可重试。

Use `ReloadPreparedAsync` for subsequent updates without reinitializing. `ConfigPreloadPlan` is immutable: each `WithRow` / `WithTable` returns a new plan; ignoring it discards that step. Plans include only explicitly listed rows/tables. Hero referencing Skill does not prewarm Skill automatically, and preparation is not full reference validation. Add targets explicitly when required. Failed first initialization leaves no valid snapshot; failed/canceled preparation with an existing snapshot preserves it and can be retried after repair.

`ConfigLoadOptions` 在创建管理器时固定。默认在调用线程执行解码；异步资源读取完成后也不会自动分帧。如果目标环境支持线程，且编解码器与转换器线程安全、不调用 Unity 主线程 API，可以显式传入 `scheduler: BackgroundConfigLoadScheduler.Instance`。该调度器只处理异步冷加载的校验、解码与索引构建，不把同步 API 变成异步，不负责 AB 下载调度；WebGL 不支持此后台策略。

`ConfigLoadOptions` is fixed when constructing the manager. Decoding runs on the calling thread by default; async I/O does not automatically spread it across frames. If the platform supports threads and codecs/converters are thread-safe and avoid Unity main-thread APIs, explicitly pass `scheduler: BackgroundConfigLoadScheduler.Instance`. This schedules only async cold-load validation, decoding, and index construction. It neither converts sync APIs into async nor schedules AB downloads. WebGL does not support this background policy.

诊断通过 `snapshot.Diagnostics` 读取，未启用时 `Enabled` 为 false。`OpenedTableCount`、`RetainedBytes`、`LoadedRowCount`、`CompletedTableCount` 描述当前缓存；读取、哈希、索引、解码和整表构建时间是累计值，包括失败尝试。读取时间包含资源源等待，RetainedBytes 只统计保留的原始表字节，不包含行对象、索引、AB 框架或 Unity 的全部内存。缓存命中和首行冷加载应分别测量，不能把一次 .NET 测量等同于 Unity / IL2CPP 的性能。

Read diagnostics through `snapshot.Diagnostics`; `Enabled` is false when disabled. `OpenedTableCount`, `RetainedBytes`, `LoadedRowCount`, and `CompletedTableCount` describe current caches. Read/hash/index/decode/table-build times accumulate, including failed attempts. Read time includes source waits. RetainedBytes counts retained raw table bytes only, excluding row objects, indexes, AB frameworks, and total Unity memory. Measure warm cache hits and cold first-row loads separately; .NET measurements do not establish Unity/IL2CPP performance.

### 14.5 Android、iOS、WebGL 与 StreamingAssets

**14.5 Android, iOS, WebGL, and StreamingAssets**

上面的 `FileConfigSource` 适用于桌面真实文件路径。Android APK 中的 `jar:` 路径和 WebGL 的 URL 需要异步请求；1.4.0 已在 [Unity 适配层](../unity/ZrConfig.Sample/Assets/ZrConfig/Unity/Runtime/StreamingAssetsConfigSource.cs) 提供 `ZrConfig.Unity.StreamingAssetsConfigSource`，随 `package-unity` 源码包一起分发，直接引用即可。

`FileConfigSource` suits real desktop paths. Android APK `jar:` paths and WebGL URLs require async requests. Version 1.4.0 includes `ZrConfig.Unity.StreamingAssetsConfigSource` in the [Unity adapter](../unity/ZrConfig.Sample/Assets/ZrConfig/Unity/Runtime/StreamingAssetsConfigSource.cs), distributed with `package-unity` source packages.

在 Unity 主线程的首次加载入口中使用：

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

默认根路径为 `Application.streamingAssetsPath + "/ZrConfig"`，也可向构造函数传入实际根路径。资源接口接收相对路径，例如 `current.json`、`versions/v某版本/Hero.bytes`；适配器检查路径并用 UnityWebRequest 异步读取，支持取消和请求失败报告。`ReadCount` 与 `Trace` 可观察实际请求，`CoalescingConfigSource` 合并同一路径的并发读取。

The default root is `Application.streamingAssetsPath + "/ZrConfig"`; the constructor also accepts your actual root. Sources receive relative paths such as `current.json` and `versions/<version>/Hero.bytes`. The adapter validates paths and reads asynchronously through UnityWebRequest, with cancellation and request errors. `ReadCount` and `Trace` expose actual requests; `CoalescingConfigSource` merges concurrent same-path reads.

适配器须在 Unity 主线程构造和调用，不配置后台加载调度器，也不阻塞等待异步操作。它只实现 `IConfigSource`：首次查询和未缓存的跨表引用使用 `GetHeroAsync`、`GetSkillAsync` 等异步入口；已缓存数据可同步查询。不能直接照搬桌面例子中的 `hero.Skill` 来读取尚未加载的资源。重新加载仍使用 `ReloadAsync`，全量检查后切换使用带 `TableCodecs.Registry` 的全量入口。

Construct and call this adapter on Unity's main thread, without background scheduling or blocking on async operations. It implements only `IConfigSource`: use `GetHeroAsync`, `GetSkillAsync`, and other async APIs for first queries and uncached references. Cached data supports synchronous queries. Do not use desktop-style `hero.Skill` to fetch an unopened resource. Reload with `ReloadAsync`; full validation before switching requires full-load entry points with `TableCodecs.Registry`.

普通示例在 Android / iOS / WebGL Player 自动采用这条异步读取路径；Editor 与桌面仍提供文件桥接的同步/异步演示。构建与实际设备/浏览器运行分开验收，入口、报告收集及已测范围见 [真实 Player 验收](release-and-acceptance.md#真实-player-入口)。使用具体 AB、Addressables 或热更新框架时，继续按下面的资源接口接入项目 SDK。

The ordinary sample automatically uses this async path in Android/iOS/WebGL Players. Editor and desktop still demonstrate synchronous/asynchronous file bridges. Build acceptance and actual device/browser execution are separate; see [Real Player entry points](release-and-acceptance.md#真实-player-入口) for commands, reports, and tested scope. For a specific AB, Addressables, or hot-update framework, connect its SDK through the following resource interface.

#### 第三方 AB 框架：同步与异步

**Third-party AB frameworks: synchronous and asynchronous loading**

仓库提供可直接 Play 的 [Unity AB 示例](../samples/README.md#ab-同步与异步示例)：同一场景切换同步/异步加载、查询和重载，并显示资源读取记录。尚未选定 AB SDK 的调用点集中在 `SampleAbResources`，临时使用真实文件读取，其余配置加载流程完整执行。

The repository includes a playable [Unity AB example](../samples/README.md#ab-同步与异步示例), switching sync/async loading, queries, and reloads in one scene with resource traces. Unselected SDK call sites are isolated in `SampleAbResources`, temporarily using actual file reads; the rest of configuration loading executes fully.

资源框架尚未选定时，使用通用 `DelegateSyncConfigSource` 作为接入边界。框架确定后只需实现下面两个函数，配置加载、行查询、缓存与重载接口都不变；ZrConfig 不安装具体 AB SDK，也不要求传入 Unity 原生 AssetBundle。

Until a framework is selected, use `DelegateSyncConfigSource` as the integration boundary. Implement the two functions below once selected; configuration loading, row queries, caching, and reload APIs remain unchanged. ZrConfig neither installs an AB SDK nor requires native Unity AssetBundle objects.

| 项目提供的函数<br>Project-provided function | 返回类型<br>Return type | 职责<br>Responsibility |
| --- | --- | --- |
| `LoadBytes(string path, CancellationToken token)` | `byte[]` | 调用框架真实的同步读取入口<br>Call the framework's actual synchronous read API |
| `LoadBytesAsync(string path, CancellationToken token)` | `ConfigOperation<byte[]>` | 调用框架真实的异步入口，完成后返回字节<br>Call its actual async API and return bytes on completion |

这两个函数是接入项目需要实现的桥接函数，不是 ZrConfig 内置方法。完成桥接后直接绑定，无需额外工厂或管理器：

Your integration implements these bridge functions; they are not built-in ZrConfig methods. Bind them directly without another factory or manager:

```csharp
var source = new DelegateSyncConfigSource(LoadBytes, LoadBytesAsync);
```

如果框架只提供异步资源读取，可以单独使用 `DelegateConfigSource`，但不能据此承诺未加载资源的同步查询。选择框架时应确认它是否支持所需平台上的真实同步读取；通常需要先完成远端资源的下载和准备，不能阻塞等待异步下载来伪装同步能力。

Async-only frameworks can use `DelegateConfigSource`, but cannot promise synchronous reads of unopened resources. Verify actual sync support on required platforms when choosing a framework. Remote resources generally must be downloaded/prepared beforehand; blocking on async downloads is not genuine synchronous support.

以下两组业务代码二选一用于首次初始化，之后可交替使用查询接口，共享同一份行缓存：

Choose either initial setup below. Afterward, query APIs may be mixed and share the same row cache:

```csharp
// 同步链：Load -> source.Read；GetHero / Skill 缺少字节时仍只调用 Read。
ConfigSnapshot configs = UnityConfigRuntime.Load(source, token);
HeroConfig hero = configs.GetHero(1, token);
Debug.Log(hero.Skill?.Name);
bool cached = hero.TryGetCachedSkill(out var sameSkill);
```

**示例英文说明 / English notes:**

- Sync chain: Load -> source.Read; GetHero / Skill also use only Read when bytes are missing.


```csharp
// 异步链：LoadAsync -> source.ReadAsync；后续资源读取同样走 ReadAsync。
ConfigSnapshot configs = await UnityConfigRuntime.LoadAsync(source, token);
HeroConfig hero = await configs.GetHeroAsync(1, token);
SkillConfig skill = await hero.GetSkillAsync(token);
Debug.Log(skill?.Name);
```

**示例英文说明 / English notes:**

- Async chain: LoadAsync -> source.ReadAsync; subsequent resource reads also use ReadAsync.


`source` 为绑定了两个读取函数的适配器，`token` 为调用方取消令牌。清单与表使用 `current.json`、`versions/<版本号>/<清单中的表文件名>` 相对路径，由桥接函数映射到资源地址。返回实际字节，不返回 URL 或文本。回调式 AB API 可用 `ConfigCompletionSource<byte[]>` 桥接：成功、失败、取消分别完成结果；不要用同步读取伪装异步，也不要轮询或阻塞等待 AB 操作。

`source` binds the two read functions; `token` is caller cancellation. Manifest/table paths are relative: `current.json`, `versions/<version>/<manifest table filename>`. Bridges map these to resource addresses and return bytes, not URLs or text. Callback AB APIs can use `ConfigCompletionSource<byte[]>` to complete success, failure, or cancellation separately. Do not disguise sync reads as async, poll AB operations, or block waiting for them.

通用委托适配器只转交读取调用。可在外层使用 `CoalescingConfigSource`（异步）或 `CoalescingSyncConfigSource`（双入口）合并同路径在途请求并隔离调用方取消；完成即移除，不缓存清单或失败结果。示例 `SampleAbResources.Source` 已包装双入口合并器。路径映射、下载、资源句柄释放和稳定字节取得仍由接入项目按框架能力处理。返回的托管字节在配置快照使用期间必须保持有效且不可修改；释放框架句柄不能回收或覆盖这份数据。

Delegate adapters only forward reads. Wrap them with `CoalescingConfigSource` (async) or `CoalescingSyncConfigSource` (both APIs) to merge in-flight same-path requests with independent caller cancellation. Entries disappear on completion; manifests and failures are not cached. `SampleAbResources.Source` already wraps the dual-API coalescer. Your integration still owns path mapping, downloads, handle release, and stable byte ownership. Managed bytes must remain valid and immutable throughout snapshot use; releasing framework handles must not reclaim or overwrite them.

更新时先完成框架的资源更新，并确保下一次读取 `current.json` 能获得新清单，再调用 `UnityConfigRuntime.Reload(token)` 或 `await UnityConfigRuntime.ReloadAsync(token)`。如果桥接函数固定绑定某个资源版本，则创建新源并传入重载。切换前失败保留旧快照和旧源；成功后从新 `Current` 重新获取配置，在旧请求结束后由资源层释放不再使用的资源。默认重载只检查清单基本格式，表是否存在、结构是否兼容和实际表文件都在查询时校验。

First complete the framework's resource update and ensure the next `current.json` read returns the new manifest. Then call `UnityConfigRuntime.Reload(token)` or `await UnityConfigRuntime.ReloadAsync(token)`. If delegates bind a fixed version, create and pass a new source. Failure before switching preserves the old snapshot/source; success requires reacquiring objects from new Current. The resource layer releases unused resources after old requests finish. Default reload checks basic manifest format only; table presence, schema, and actual files are validated on query.

配表保留 v4 行索引和已加载对象，不重复实现 AB 的资源缓存或下载调度。拿到字节后的哈希校验、解码和建索引在调用上下文执行；异步资源读取不代表解码会自动分帧。

Configuration retains v4 row indexes and loaded objects without duplicating AB caches or download scheduling. Hashing, decoding, and index construction run in the calling context after bytes arrive. Async I/O does not automatically distribute decoding over frames.

### 14.6 重新加载后，旧变量会自己变成新数据吗

**14.6 Do old variables automatically become new data after reload?**

不会。旧 hero 的普通字段仍是旧值；更新后必须重新获取配置对象。

No. Ordinary fields on an old hero retain their old values. Retrieve configuration objects again after updating.

运行时校验清单成功后替换 Current。你需要从新快照按需取得行或表；清单失败时保留 Current，行加载失败时不缓存本批半成品。成功更新会使旧快照查询与旧行引用访问失效，包括已经缓存的引用。旧普通字段不会被原地修改，业务不应继续使用旧对象。

A valid manifest replaces Current; rows/tables are retrieved lazily from the new snapshot. Manifest failure preserves Current; row failure publishes no partial batch. Successful updates invalidate old snapshot queries and old-row reference access, including cached references. Ordinary fields are not mutated in place; business code should stop using old objects.

```csharp
ConfigSnapshot snapshot = UnityConfigRuntime.Current;
HeroConfig hero = snapshot.GetHeroTable()[0];
```

游戏一次需要多张表时使用同一个 snapshot。若期间成功重载，旧查询会报配置已失效；在 Changed 通知中取消或丢弃旧业务查询，再从新 Current 获取所需对象。

Use one snapshot when a game operation needs multiple tables. A successful reload during the operation invalidates old queries. In Changed handlers, cancel or discard old queries and retrieve required objects from new Current.

### 14.7 想在更新时自动刷新 UI

**14.7 Automatically refresh UI on updates**

订阅 `UnityConfigRuntime.Changed`：

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
    // 新版本清单已就绪，从这个 snapshot 异步按需加载行或表，再刷新 UI。
}
```

**示例英文说明 / English notes:**

- The new manifest is ready. Load rows/tables asynchronously from this snapshot, then update UI.


该片段放在引用了 ZrConfig.Runtime、ZrConfig.Unity 的 MonoBehaviour 中。退出监听时取消订阅。事件只通知成功变更；监听器自己抛异常会记录日志并隔离其他监听器。

Put this snippet in a MonoBehaviour referencing ZrConfig.Runtime and ZrConfig.Unity. Unsubscribe when no longer listening. Events report successful changes only; exceptions from individual listeners are logged and isolated from other listeners.

配置按只读使用。生成行属性不可随意赋值，但数组和自定义对象的成员不做深拷贝，不要在游戏里修改它们作为角色当前状态。角色“当前血量”应存到运行时角色对象，“最大血量配置”才放在配表里。

Treat configuration as read-only. Generated row properties are not freely assignable, but arrays and custom objects are not deep-copied. Do not mutate them as character state. Store current health in runtime character objects; maximum-health configuration belongs in tables.

### 14.8 热更新项目要同步哪些内容

**14.8 What must a hot-update project synchronize?**

只改数据：发布新的完整数据版本和对应 current.json。

Data-only changes: publish a complete new data version and its current.json.

改字段、手写成员、枚举、子类集合或转换器：重新生成并编译对应 Config/Codec，导出新数据，按你的热更新流程同时发布代码与数据。

Field, handwritten member, enum, subtype-set, or converter changes: regenerate/compile Config and codecs, export new data, then publish code and data together through your hot-update workflow.

Schema Hash 是结构标识。新代码与旧数据结构不匹配会被拒绝，这是保护机制，不能靠删除 current.json 解决。

Schema Hash identifies structure. Rejecting mismatched code/data protects compatibility; deleting current.json does not solve the mismatch.

Unity 查看器入口是 `Tools > ZrConfig > 运行时配置表`。需要配置已经成功加载。它用于看当前快照，不代替 Excel 编辑和导出。

Open the Unity viewer at `Tools > ZrConfig > 运行时配置表` (Runtime configuration tables) after loading configuration. It displays the current snapshot and does not replace Excel editing/export.

### 14.9 通用查看器与 Odin

**14.9 Generic viewer and Odin**

按 [接入 Unity 源码](#142-接入-unity-源码) 放入 Assets/ZrConfig 后，通用查看器随源码一起编译，无需额外安装。

After [integrating Unity source](#142-接入-unity-源码) into Assets/ZrConfig, the generic viewer compiles with it without extra installation.

- `Tools → ZrConfig → 运行时配置表`：选择已加载的表。<br>
  `Tools → ZrConfig → 运行时配置表`: select a loaded table.
- 给 GameObject 添加 `ZrConfig/配置表查看器`：指定表名，在 Inspector 查看。<br>
  Add `ZrConfig/配置表查看器` (Configuration table viewer) to a GameObject, specify a table name, and inspect it.
- 无 Odin 时提供分页只读查看；安装 Odin 并启用 `ODIN_INSPECTOR` 后使用 Odin 表格。<br>
  Without Odin, use the paged read-only viewer. Installing Odin and enabling `ODIN_INSPECTOR` enables Odin tables.
- 通用编辑器属性处理器显示生成类的只读属性，不向 Models 注入 Odin 标签。<br>
  A generic editor property processor displays generated read-only properties without adding Odin attributes to Models.
- 查看器读取当前游戏的实际快照，热更新后刷新；普通表无需独立 View 类。<br>
  The viewer reads the game's actual snapshot and refreshes after hot updates; ordinary tables need no separate View class.

Odin 适配依据官方 [PropertyTree](https://odininspector.com/documentation/sirenix.odininspector.editor.propertytree) 和 [AttributeProcessor](https://odininspector.com/documentation/sirenix.odininspector.editor.odinattributeprocessor) 接口。未在真实 Odin 安装及 IL2CPP Player 中执行集成测试，接入具体项目后仍需验证这些环境。

Odin integration follows the official [PropertyTree](https://odininspector.com/documentation/sirenix.odininspector.editor.propertytree) and [AttributeProcessor](https://odininspector.com/documentation/sirenix.odininspector.editor.odinattributeprocessor) APIs. Integration has not been tested with a real Odin installation or IL2CPP Player; validate those environments in your project.

### 14.10 普通 .NET 接入

**14.10 Ordinary .NET integration**

不使用 Unity 时可直接使用 ConfigManager，并注入清单解析函数。下面放进已有异步初始化方法，configOutputFolder 为实际输出根目录；普通查询不需要注册表：

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

在类方法文件中，using 放文件顶部，创建和加载语句放异步方法内部。IncludeFields 用于清单字段；后续更新复用 manager 并重新从 Current 取快照。资源获取也可实现 IConfigSource 或使用 DelegateConfigSource。

In class files, put using directives at the top and initialization/loading statements inside an async method. IncludeFields handles manifest fields. Reuse the manager for later updates and retrieve a new snapshot from Current. Resource access can implement IConfigSource or use DelegateConfigSource.

## 15. 规则设计时的几个边界

**15. Boundaries when designing rules**

### 15.1 先判断“能否检查”，再判断“对不对”

**15.1 Check whether validation is possible before checking correctness**

解析失败的行不应继续被当作完整对象使用。工具会隔离不完整表，业务代码也应跳过空白、越界的关键值，避免误导性错误。

Do not treat rows with parse failures as complete objects. The tool isolates incomplete tables; business validation should also skip blank or out-of-range prerequisite values to avoid misleading errors.

例子：Level 空白 → Required 报告“没填”。不要同时给它报“0 级英雄必须选新手技能”，因为那不是实际填写的值。

Example: blank Level causes Required to report “missing”. Do not also report that a level-0 hero must select a beginner skill, because 0 was not the actual input.

### 15.2 正常业务错误用 Error，疑似问题用 Warning

**15.2 Use Error for invalid business data and Warning for suspicious data**

Error：一定不能发布，例如缺少目标技能。

Error prevents publication, for example a missing target skill.

Warning：数据可以使用，但需要确认，例如技能伤害为 0 可能是辅助技能。

Warning permits use but requests review; zero skill damage may be valid for a support skill.

不要把必须阻止的错误写成 Warning，也不要通过捕获异常后什么都不做来掩盖检查失败。

Do not downgrade blocking errors to Warning or hide failed checks by swallowing exceptions.

### 15.3 类型支持，不代表每个内置标签都支持

**15.3 Supported types do not imply support for every built-in attribute**

类型表中列出 decimal、uint、字典等，表示工具能解析、生成和读写。内置 Range 的类型限制仍然按第 4.5 节执行。

Listing decimal, uint, dictionaries, and other types means they can be parsed, generated, and serialized. Built-in Range still follows section 4.5's type restrictions.

有特殊条件就用匹配类型的 `IFieldRule<T>` 或 Validate。不要把规则系统理解成可以给任何字段随意贴任何标签。

Use a matching `IFieldRule<T>` or Validate for special conditions. Rule attributes cannot be applied arbitrarily to every field.

### 15.4 分列路径和 JSON 只改变输入方式

**15.4 Split-column paths and JSON affect input only**

同一逻辑字段可以用 JSON 单格、成员分列等方式输入。生成模型仍然是对象，不会生成名字含点号的属性。

The same logical field can use one JSON cell or split member columns. Generated models remain objects; property names do not contain dots.

结构标识根据逻辑字段与类型计算。保持相同逻辑结构时，只切换输入布局不会凭空增加一个新的运行时成员。改变了字段顺序、类型或复杂结构，仍需同步代码与数据。

Schema identifiers use logical fields and types. Switching input layouts without changing logical structure does not create runtime members. Field order, type, or complex-structure changes still require synchronized code/data.

### 15.5 读写代码版本也要一致

**15.5 Reader and writer versions must match**

当前表二进制使用 v4 行索引格式，清单容器标识仍为 ZrConfig.Binary/1。不能从名字中的“1”推断表二进制是旧版 v1。

Current tables use the v4 row-index binary format, while the manifest container identifier remains ZrConfig.Binary/1. The “1” does not mean the table format is old v1.

旧 v1 数据不能直接用新 Codec 读取。升级 Runtime、生成源码或序列化协议时，重新生成和导出，同步交付。

New codecs cannot directly read old v1 data. When upgrading Runtime, generated source, or serialization protocols, regenerate, export, and deliver them together.

Serialization 支持文件按内容记录版本：未修改的已知版本可自动升级；手写修改或无法识别的旧文件会保留并提示合并。普通 CLI `export` 与底层 ExportPipeline 不改源码，发现已知旧版本时需先生成或执行 `support`；CLI `export --generate` 会按组合流程更新代码，App 会在导出前提示确认生成升级。详见 [支持源码升级](development.md#支持源码升级)。

Serialization support files use content-based versions. Recognized unmodified files can upgrade automatically; handwritten changes and unknown old files are preserved with merge guidance. Ordinary CLI `export` and ExportPipeline do not edit source; recognized older support requires generation or `support` first. CLI `export --generate` updates code through the combined flow; the App asks before generation upgrades during export. See [Support-source upgrades](development.md#支持源码升级).

### 15.6 不是任意 C# 类型都能自动配表

**15.6 Arbitrary C# types cannot automatically become Excel configuration**

填写类型见 [Excel 类型语法](config-types.md)，实现示例见本文第 1～2、10～11 章。任意 `List<T>`、游戏对象、UnityEngine.Object、泛型数据类等不能随意当作普通 Excel 类型使用。

See [Excel type syntax](config-types.md) for cell types and chapters 1–2 and 10–11 for implementations. Arbitrary `List<T>`, game objects, UnityEngine.Object, and generic data classes are not ordinary Excel field types.

集合一般使用支持的数组或 Dictionary。特殊标量可以通过转换器接入。更复杂的游戏行为留给游戏代码处理。

Use supported arrays or Dictionary for collections. Converters support special scalar types. Keep complex game behavior in game code.

## 16. 常用代码速查

**16. Common code reference**

| 想做什么<br>Task | 代码<br>Code |
| --- | --- |
| 顶层字段必填<br>Require a top-level field | `[Required(nameof(HeroConfig.Name))]` |
| 顶层字段唯一<br>Make a top-level field unique | `[Unique(nameof(HeroConfig.Name))]` |
| 正整数唯一编号<br>Positive unique integer ID | `[PositiveKey(nameof(HeroConfig.Id))]` |
| int 等级 1～100<br>int level from 1 to 100 | `[Range(nameof(HeroConfig.Level), 1, 100)]`，需要必填再加 Required<br>`[Range(nameof(HeroConfig.Level), 1, 100)]`; also add Required if mandatory |
| 技能编号必须存在<br>Require an existing skill ID | `[Reference(nameof(HeroConfig.SkillId), typeof(SkillConfig), nameof(SkillConfig.Id))]` |
| 0 或空白允许无技能<br>Allow no skill for 0 or blank | Reference 上加 `AllowDefaultValue = true`<br>Add `AllowDefaultValue = true` to Reference |
| 绑定名称字段检查<br>Bind a display-name field rule | `[Check(nameof(HeroConfig.Name), typeof(DisplayNameRule))]` |
| 自定义后置读取技能表<br>Read Skill in a custom post-validation rule | `[DependsOn(typeof(SkillConfig))]` |
| 手写对象成员必填<br>Require a handwritten object member | 在成员上加 `[ConfigRequired]`<br>Put `[ConfigRequired]` on the member |
| 逐条记录<br>Validate each row | `protected override void Validate(HeroConfig row, ValidationContext context)` |
| 整张表<br>Validate the whole table | `protected override void ValidateTable(IReadOnlyList<HeroConfig> rows, ValidationContext context)` |
| 判断原格子没填<br>Check whether the original cell was blank | `context.IsEmpty(row, nameof(HeroConfig.Level))` |
| 判断嵌套成员没填<br>Check whether a nested member was blank | `context.IsEmpty(row, "Settings.MaxTargets")` |
| 获取另一张表<br>Retrieve another table | `context.GetSkillTable()` 复用主键/分组索引；`context.GetTable<SkillConfig>()` 返回同一份索引表<br>`context.GetSkillTable()` reuses primary/group indexes; `context.GetTable<SkillConfig>()` returns the same indexed table |
| 行规则报错<br>Report a row-rule error | `context.Error(row, nameof(HeroConfig.Level), "提示", "代号")`<br>`context.Error(row, nameof(HeroConfig.Level), "提示", "代号")` (message, code) |
| 行规则警告<br>Report a row-rule warning | `context.Warning(row, nameof(HeroConfig.Level), "提示", "代号")`<br>`context.Warning(row, nameof(HeroConfig.Level), "提示", "代号")` (message, code) |
| 字段规则报错<br>Report a field-rule error | `context.Error("提示", "代号")`<br>`context.Error("提示", "代号")` (message, code) |
| 指向数组成员<br>Target an array member | `context.Error(row, $"Actions[{i}].Delay", "提示", "代号")`<br>`context.Error(row, $"Actions[{i}].Delay", "提示", "代号")` (message, code) |

记住代码所处的位置：标签在类上方；if 判断在方法内部；using 在文件开头。不能把所有片段不分位置地贴到一个文件结尾。

Placement matters: attributes go above classes, if statements inside methods, and using directives at file start. Do not paste every snippet indiscriminately at the end of one file.

## 17. 教程验证与复验范围

**17. Tutorial validation and repeatable scope**

完成教程后，可按以下步骤检查自己的配置与规则。

After completing the tutorial, check your configuration and rules as follows.

1. 将 [练习附件](assets/user-guide-example.zip) 解压到系统临时目录；第 12.3～12.9 节的七份完整源码应与附件同路径文件一致，不混入正式 samples。<br>
   Extract the [exercise attachment](assets/user-guide-example.zip) into a system temporary directory. The seven complete source files in sections 12.3–12.9 must match the corresponding attachment files; do not mix them into official samples.
2. 对练习目录执行第 13 章的 `codegen` 与 `export` 命令，核对 Hero 2 条、Skill 2 条、0 错误和 1 条预期警告。<br>
   Run chapter 13's `codegen` and `export` against the exercise directory. Expect 2 Hero rows, 2 Skill rows, zero errors, and one expected warning.
3. 修改数据或规则示例后，再按第 12.11 节逐项制造错误，确认阻止发布且 current.json 字节不变；恢复数据后确认可导出。第二个动作负延时应定位到 Hero 第 7 行、第 12 列，路径 Actions[1].Delay。<br>
   After modifying examples, deliberately introduce each error from section 12.11. Confirm publication is blocked and current.json bytes remain unchanged; restore the data and confirm export succeeds. A negative delay on the second action should locate Hero row 7, column 12, path Actions[1].Delay.
4. 检查日志仍只有 export.log 与 diagnostics.json 两个固定文件，新增临时文件不进入正式 Data、Config 或 Output。<br>
   Confirm logs remain the two fixed files export.log and diagnostics.json. Keep temporary files out of official Data, Config, and Output.


这是示例生成、规则行为、文件位置和编译验证；不代表已经完成你自己游戏工程、移动端、WebGL、IL2CPP 或具体热更新框架的实际部署测试。

This validates sample generation, rule behavior, file placement, and compilation. It does not establish deployment in your own game, mobile, WebGL, IL2CPP, or a particular hot-update framework.

## 18. 类型实现与兼容约束

**18. Type implementation and compatibility constraints**

本节由项目程序员与工具维护者处理，配表人员不需要编写或升级通用读写源码。

Project programmers and tool maintainers handle this section. Table authors need not implement or upgrade generic serialization source.

业务类型放在 Config/CustomTypes；特殊转换器按需放在 Config/Serialization。通用支持文件由 ConfigSourceLayout 初始化并按内容版本升级，手写修改保留并提示合并。它们仍属于当前项目源码的一部分，不能只删除旧文件而不验证项目改动和 Runtime/Codec 兼容性。

Put business types in Config/CustomTypes and optional converters in Config/Serialization. ConfigSourceLayout initializes common support files and upgrades them by content version, preserving handwritten changes with merge guidance. They remain project source: do not simply delete old files without verifying project modifications and Runtime/codec compatibility.

类必须 public 且有 public 无参构造函数；可序列化字段应可写，属性使用 public get/set。继承成员参与生成，不支持隐藏同名成员、索引器、只读/只写属性、init 属性或泛型数据类。readonly 特殊结构可通过转换器接入。构造函数和 setter 不写业务副作用。

Classes must be public with a public parameterless constructor. Serializable fields must be writable; properties require public get/set. Inherited members participate. Hidden same-name members, indexers, read-only/write-only/init properties, and generic data classes are unsupported. Converters can support special readonly structs. Constructors and setters must avoid business side effects.

转换器 Parse/Write/Read 使用同一约定和固定文化格式，实例会复用，不保存上个格子的可变状态；改变二进制表示需升级 ConfigConverter 的版本。普通业务类型与转换器应保持纯 C#，不依赖 UnityEngine/UnityEditor，并兼容游戏支持的 C# / .NET API。

Converter Parse/Write/Read must share conventions and invariant formatting. Instances are reused, so do not retain mutable state from previous cells. Binary-representation changes require incrementing the ConfigConverter version. Business types and converters should remain pure C#, independent of UnityEngine/UnityEditor, and compatible with the game's C#/.NET APIs.

矩形数组的长度校验由 Serialization/ConfigReadContext.cs 的 ReadDimension、ClaimElements 和 ReadLength 处理。生成模板和仓库唯一的支持源码同步维护。

Rectangular-array bounds are handled by ReadDimension, ClaimElements, and ReadLength in Serialization/ConfigReadContext.cs. Keep generation templates synchronized with the repository's sole support-source copy.

SchemaHash 依据逻辑字段、顺序和类型结构计算，JSON/分列/多行等输入布局不参与。保持相同逻辑结构时只切换输入布局不改变运行时字段；代码与导出数据必须使用一致的结构标识。

SchemaHash uses logical fields, order, and type structure; JSON/split-column/multi-row input layouts are excluded. Layout-only changes do not alter runtime fields. Code and exported data must share a schema identifier.

生成 Codec 直接调用成员和读写方法，运行时不通过反射访问字段。数组写长度后写元素，空数组读取用 Array.Empty；字符串使用栈缓冲/ArrayPool，避免临时 UTF-8 byte[]。实际字符串、非空数组、对象、表行仍需要分配，不承诺零 GC。表的属性只读；数组元素和自定义对象不会深拷贝，业务代码应将加载后的配置视为只读。导表侧的类型发现/对象构造使用缓存元数据和反射，导出结束后不静态持有项目程序集。

Generated codecs access members and serialization methods directly without runtime field reflection. Arrays store lengths then elements; empty reads use Array.Empty. Strings use stack buffers/ArrayPool to avoid temporary UTF-8 byte arrays. Actual strings, nonempty arrays, objects, and rows still allocate; zero GC is not promised. Table properties are read-only, but array elements/custom objects are not deep-copied; treat loaded configuration as immutable. Export-side discovery/construction uses cached metadata and reflection without statically retaining project assemblies after export.

普通字段不保存每个值的类型标签。多态位置写文件局部索引，完整类名集中放在表文件头中；读取时只映射一次，再通过生成的 switch 调度。索引不是稳定业务编号。结构哈希包含二进制版本、class/struct 形态、传递成员、必填声明、枚举定义、子类集合及转换器版本；重命名类或改变结构需要重新生成代码及数据。

Ordinary values carry no per-value type tags. Polymorphic positions store file-local indexes, with full type names centralized in headers. Reading maps types once and dispatches through generated switches. These indexes are not stable business IDs. Schema hashes include binary version, class/struct shape, transitive members, required declarations, enums, subtype sets, and converter versions. Renaming classes or changing structures requires regenerated code/data.

二进制版本及旧数据兼容要求见 [15.5 读写代码版本](#155-读写代码版本也要一致)。工具维护者修改底层格式时必须同时升级 BinaryFormat.Version；项目程序员重新生成匹配的代码和数据，再通过实际交付方案同步更新 Runtime、Codec 与数据。工具不自动迁移旧格式数据。

See [15.5 Reader/writer versions](#155-读写代码版本也要一致). Maintainers changing the underlying format must also increment BinaryFormat.Version. Project programmers regenerate compatible code/data and deliver Runtime, codecs, and data together. The tool does not automatically migrate old binary data.

读取限制：单数组最多 1,000,000 元素，每张表数组总元素最多 8,000,000，递归深度最多 64，字符串 UTF-8 最多 16 MiB。超长、截断、非法索引和类型表被拒绝，发布失败保留上次 current.json。

Read limits: 1,000,000 elements per array, 8,000,000 total array elements per table, recursion depth 64, and 16 MiB of UTF-8 per string. Oversized/truncated data, invalid indexes, and invalid type tables are rejected. Failed publication preserves the previous current.json.

仍不支持 Dictionary 以外的任意泛型容器、秩大于 3 或连续嵌套超过三层的数组，以及游戏行为对象/UnityEngine 对象直接序列化。特殊类型可通过转换器扩展。

Unsupported cases remain arbitrary generic containers except Dictionary, arrays of rank above 3 or nesting deeper than three levels, and direct serialization of game-behavior/UnityEngine objects. Converters can extend special types.
