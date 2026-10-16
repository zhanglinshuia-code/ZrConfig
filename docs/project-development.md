# 项目开发教程：业务扩展与 Unity 接入

多语言服务端生成与 Binary / JSON / 双格式导出见[多语言生成与 JSON](server-generation.md)。
本文面向项目程序员，完整保留业务类型、转换器、后置检查、命令行与 Unity 接入的实现示例。日常配表人员只需阅读 [使用文档](user-guide.md)；本教程不是配表前置步骤。源码启动与安装包构建见 [源码构建与升级](development.md)。

## 开发职责与现有实现

| 内容 | 谁负责 | 何时需要 |
| --- | --- | --- |
| 表的行模型、Codec、注册表 | 工具生成，程序员负责接入编译 | 首次接入或表结构变化 |
| 业务枚举、类、结构体 | 项目程序员 | 内置类型不能表达项目数据形状时 |
| 通用序列化支持源码 | 工具提供，版本升级由工具/项目维护者处理 | 首次生成补齐；已记录且未修改的版本自动升级 |
| 特殊转换器 | 项目程序员按需编写 | 项目确有特殊文本或二进制约定时 |
| 业务规则 | 项目程序员维护，工具自动执行 | 需要额外业务约束时 |
| Excel 数据、错误修正、导出验收 | 配表人员 | 日常操作 |

示例代码根目录按用途分为 CustomTypes、Serialization、Generated：CustomTypes 保存手写的项目业务定义；Serialization 同时容纳工具提供的支持文件和可选项目转换器；Generated 由生成器更新。手写源码可放在用户选择的代码根目录或任意命名的子目录，工具递归发现，不要求 CustomTypes 这个名称。生成时排除工具的 Generated 输出；bin、obj、点号开头的文件/目录和链接文件/目录不参与编译。文件位于项目内，不代表配表人员需要编写读写代码。

通用支持代码仍由工具管理在 Config/Serialization。生成时补齐缺失文件，并自动升级与已记录版本一致、未经手写修改的文件；手写修改和无法识别的旧版保留并提示合并。导出会检查版本，处理步骤见 [支持源码升级](development.md#支持源码升级)。

## 阅读路线

| 工作 | 章节 |
| --- | --- |
| 添加业务数据类型 | [1. 手写类](#1-手写类是什么)、[2. 枚举与多态](#2-枚举结构体和多态是什么) |
| 编写后置检查 | [3. 检查机制](#3-检查规则到底是什么) 至 [9. 对象与数组](#9-检查对象成员和数组元素) |
| 配置对象引用 | [10. 直接表引用](#10-直接表对象引用少写一次手动查找) |
| 自定义输入/编码格式 | [11. 转换器](#11-特殊格式怎么扩展转换器) |
| 从零完成一套代码 | [12. 完整开发练习](#12-完整练习项目) |
| 自动化导出 | [13. 命令行](#13-命令行使用) |
| Unity 加载和热更新 | [14. Unity 接入](#14-unity-运行时如何使用) |
| Rider / Visual Studio 调试 | [调试指南](debugging.md) |

第 12 章的 Hero/Skill 练习使用 SkillId、Level 等教学字段，与正式 TableReferences.xlsx 的 Skill、Mentor 对象引用字段不同。练习包解压到仓库之外，不能与 samples/Data 混用。仓库正式示例说明统一在 [samples/README.md](../samples/README.md)。

本教程的 Excel 示例统一使用五行表头：字段名、类型、主键、说明、所属端，第 6 行起填写数据。第 3 行至少标记一列“主键”，其余格子留空；完整写法见 [使用文档](user-guide.md#41-统一使用五行表头)。

## 1. 手写类是什么

### 1.1 类就是“描述一组数据的模板”

一个英雄不只有一个整数，还可能有一组战斗设置：最多攻击几个目标、是否允许被打断。

你把这两个值放在一起，它们就组成一个“战斗设置对象”。

**类**：规定这组对象里有哪些成员。可以理解成一张空白登记卡。

**对象**：按这张登记卡填出来的某一份实际内容。

**成员**：卡片里的每一项，例如 MaxTargets、Interruptible。

**手写类**：这张模板由你写 C# 来定义。工具不会猜你的游戏需要哪些战斗成员。

**生成类**：Excel 表的整行模型，例如 HeroConfig，由工具生成。

| 名字 | 谁写 | 放哪里 | 表示什么 |
| --- | --- | --- | --- |
| BattleSettings | 你写 | Config/CustomTypes | 英雄里面的一组战斗设置 |
| HeroConfig | 工具生成 | Config/Generated | Excel 中一条完整英雄配置 |
| HeroRules | 你写 | Rules | 如何检查英雄配置 |
| HeroConfigCodec | 工具生成 | Config/Generated | 如何读写 Hero 二进制 |

同一张表的记录、查询表、扩展访问入口和 Codec 都位于一个 `Generated/HeroConfig.g.cs` 文件中，仍是职责独立的类型；所有表共用一份 `TableCodecs.g.cs`。文件布局及升级说明见 [生成文件组织](development.md#生成文件组织)。

不要在 CustomTypes 里再手写一个 HeroConfig，会和工具生成的类重名。

### 1.2 第一个手写类

创建文件：`Config/CustomTypes/BattleSettings.cs`。

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

| 代码 | 意思 |
| --- | --- |
| namespace Tutorial.Config | 给这些类型放进一个“名字分类盒子”，避免重名 |
| public | 外面的代码也能看见这个类型或成员 |
| sealed class BattleSettings | 定义一个叫 BattleSettings 的类；这里不需要再被继承 |
| public int MaxTargets = 1 | 一个整数成员，默认值为 1 |
| public bool Interruptible = true | 一个开关成员，默认允许打断 |
| `{` 和 `}` | 这一段内容的开始和结束 |
| `;` | 一条声明结束 |

文件名以 `.cs` 结尾。用 Rider、VS Code 等代码编辑器保存为文本文件，不能把它保存成 `BattleSettings.cs.txt`。

`public int MaxTargets` 是字段。也可以使用公开可读写属性：

```csharp
public int MaxTargets { get; set; } = 1;
```

两种形式选一种就可以，不要在同一个类中同时声明两个同名成员。

### 1.3 Excel 怎么使用这个类：成员分列

在 Hero 表新增两列：

| Excel 行号 | Id | Settings.MaxTargets | Settings.Interruptible |
| --- | --- | --- | --- |
| 1 | Id | Settings.MaxTargets | Settings.Interruptible |
| 2 | int | Tutorial.Config.BattleSettings | 留空 |
| 3 | 主键 | | |
| 4 | 英雄编号 | 最大目标数 | 能否打断 |
| 5：所属端 | CS | CS | CS |
| 6 | 1001 | 3 | false |

表格里的“留空”是说明，实际格子必须为空，不要输入这两个字。

`Settings.MaxTargets` 表示“Settings 对象里面的 MaxTargets”。生成模型后，代码用 `row.Settings.MaxTargets` 访问。

**关键：第二行填写这一组的根类型 BattleSettings，不是在两列分别填 int、bool。** 成员类型已经由 C# 写清楚了。组内一列写根类型，其余类型格留空；也可以每列重复相同根类型。

新增或修改类后：保存 `.cs` → 保存 Excel → 点击“生成配表代码” → 点击“校验并导出”。

### 1.4 另一种填法：一个格子放整个对象

如果不想分列，可以把整个对象放在 `Settings` 一列，Id 仍作为本例的主键：

| 行 | Id | Settings |
| --- | --- | --- |
| 1 | Id | Settings |
| 2 | int | Tutorial.Config.BattleSettings |
| 3 | 主键 | |
| 4 | 英雄编号 | 战斗设置 |
| 5：所属端 | CS | CS |
| 6 | 1001 | {"MaxTargets":3,"Interruptible":false} |

这是 JSON 写法。可以显式写类型 `json<Tutorial.Config.BattleSettings>`。

同一个字段不能既有 `Settings` 整格列，又有 `Settings.MaxTargets` 展开列。选一种填写方式。

初学者编辑复杂战斗数据，建议使用分列，能直接看到每个值。

### 1.5 什么是默认值

`MaxTargets = 1` 表示默认最多 1 个目标。

JSON 写 `{"Interruptible":false}` 时，没有提供 MaxTargets，它会保留默认值 1。分列时某个可选成员格子留空，也会保留其构造默认值。

已经明确填写的 `0` 或 `false` 会覆盖默认值，不会被当成空白。

默认值只是“没有填时用什么”，不是业务检查。即使有默认值，如果业务要求这个成员必须显式填写，也要加必填声明。

### 1.6 对象成员必须显式填写：ConfigRequired

把类改成：

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

它检查“有没有明确填写”，不检查“值是否合理”。填 0 算填写了；要让 MaxTargets 大于 0，还需要第 9 章的业务检查。

这是示范替换写法。第 12 章完整练习中的 BattleSettings 使用可选默认值版本，不要把两份类同时放进 CustomTypes。

### 1.7 手写类要遵守的要求

- 类和需要导表的成员要 public。
- 类要能用公开的无参数构造函数创建；没有自己写构造函数时，普通 public 类通常已有默认构造函数。
- 字段要可以写入；属性要有 public get 和 public set。
- 不使用只读属性、只写属性、索引器、init 属性或泛型数据类。
- 不在构造函数或 setter 中播放音效、生成角色、访问游戏场景。
- 这些类型是纯数据定义，不引用 UnityEngine、UnityEditor 或游戏业务程序集。
- 继承成员可以参与导表，但不要用同名新成员隐藏父类成员。

业务行为由游戏代码处理。导表的“伤害动作”保存 Delay、Ratio 等参数，不在导表时真正对敌人扣血。

## 2. 枚举、结构体和多态是什么

### 2.1 枚举：只能从几个名字中选一个

创建 `Config/CustomTypes/Quality.cs`：

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

Excel 类型填 `Tutorial.Config.Quality`，数据填 `Rare`，不是任意中文，也不是随便填数字 1。

大小写按声明填写。`rare` 不能当作教程里的 `Rare` 使用。没有声明的名字会报错。

若允许空白枚举，需要存在取值为 0 的成员，例如 None。枚举数组类型为 `Tutorial.Config.Quality[]`，填写 `[Rare;Legendary]`。

增加、删除或修改枚举成员后，重新生成代码并导出。

### 2.2 结构体：也可以把几个值放一起

创建 `Config/CustomTypes/NumberRange.cs`：

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

Excel 分列写 `Range.Min`、`Range.Max`，第二行组类型写 `Tutorial.Config.NumberRange`。

class 与 struct 的 C# 内存行为不同，但配表时你先把它们都理解为“有几个成员的一组数据”。普通复杂对象用 class，简单小值可以用 struct。

工具不会自动知道 Min 不能大于 Max，需要后置检查：

```csharp
if (row.Range.Min > row.Range.Max)
    context.Error(row, "Range.Max", "最大值不能小于最小值。", "RANGE_ORDER");
```

这个片段放在对应表规则的 Validate 方法里，并且那张表确实要有 Range 字段。

### 2.3 多态：同一动作栏，允许不同种类的动作

“打伤害”和“加 Buff”都属于动作，但它们需要的参数不同。

创建 `Config/CustomTypes/BattleAction.cs`：

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

- BattleAction 是公共的动作模板，Delay 是所有动作都有的延时。
- Damage 是伤害动作，有 Ratio、CanCrit。
- AddBuff 是加 Buff 动作，有 BuffId、Stacks。
- `: BattleAction` 表示“这是 BattleAction 的一种”。
- abstract 表示基础模板本身不直接拿来创建实际动作，要选具体种类。

单格 JSON 数组可以这样填：

```json
[{"$type":"Damage","Delay":0.2,"Ratio":1.5,"CanCrit":true},{"$type":"AddBuff","Delay":0.5,"BuffId":2001,"Stacks":2}]
```

该列类型为 `json<Tutorial.Config.BattleAction[]>`。

`$type` 是告诉工具“这次是哪一种动作”。必须写实际子类名 Damage 或 AddBuff，不写抽象父类 BattleAction。类名冲突时写完整命名空间。

没有填写 AddBuff 的 BuffId 会报错，因为它带 ConfigRequired。明确填 0 算填写了；0 是否允许由业务检查决定。

新增 `Heal : BattleAction` 时，先写类，再生成代码，然后就可以填写 Heal，不需要给 App 增加一个手工注册表。

### 2.4 把动作数组展开成多行

下面先列出五行表头，第 3 行在 Id 列标记“主键”。实际工作簿中从 A1 开始连续填写，不要把说明行号作为数据列。

| 行 | Id | Name | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Id | Name | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
| 2 | int | string | Tutorial.Config.BattleAction[] | | | | | | |
| 3 | 主键 | | | | | | | | |
| 4 | 英雄编号 | 名称 | 动作顺序 | 动作种类 | 延时 | 倍率 | 能否暴击 | Buff 编号 | 层数 |
| 5：所属端 | CS | CS | CS | CS | CS | CS | CS | CS | CS |

接着第 6 行起填：

| Id | Name | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1001 | 小剑士 | 1 | Damage | 0.2 | 1.5 | true | | |
| 1001 | | 2 | AddBuff | 0.5 | | | 2001 | 2 |
| 1002 | 小法师 | 1 | Damage | 0 | 2 | false | | |

这三行数据最终得到 **两条英雄配置**。第一条有两个动作，第二条有一个动作。

规则：

1. 只有出现 `[]` 展开列时，同一完整主键的多行才会合并；本例以 Id 为主键。普通表的一行仍是一条记录。
2. 首行必须填写有效完整键。续行可以重复完整键，也可以将所有键分量留空表示接着上一条有效记录；联合键不能只填一部分。初学者建议明确重复完整键。
3. Name、Level 等主字段放在该 Id 的首行。续行留空，或者完全重复首行内容，不能填成另一个值。
4. `$order` 是排序用的辅助列，不会变成模型成员。上例里的顺序 1、2 表示第一个、第二个动作。
5. 声明了 `$order`，每个有效元素就要填写唯一的正整数。对于不含子列表的简单数组，也可以不声明 order，按实际行顺序排列。
6. Damage 只填 Damage 的参数；它的 BuffId、Stacks 列要留空。AddBuff 同理。
7. 所有动作展开列都留空，表示这一行没有动作元素。一个英雄没有任何动作元素，最终是空数组。
8. 不要合并数据格子，不要只排序局部几列，否则 Id 与参数可能错位。

动作下标从 0 开始：第一个动作是 Actions[0]，第二个是 Actions[1]。错误定位代码会用这个下标。

### 2.5 多层列表：动作组里再放动作

例如：英雄有多个动作组，每个动作组又有多个动作。

手写类型：

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

第一行列名可以是：

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

| Id | Groups[].$order | Groups[].Name | Groups[].Actions[].$order | Groups[].Actions[].$type | Groups[].Actions[].Ratio | Groups[].Actions[].BuffId |
| --- | --- | --- | --- | --- | --- | --- |
| 1001 | 1 | 第一组 | 1 | Damage | 1.5 | |
| 1001 | 1 | | 2 | AddBuff | | 2001 |
| 1001 | 2 | 第二组 | 1 | Damage | 2 | |

- 父 order 都是 1 的前两行，属于同一个动作组。
- 第三条数据的父 order 是 2，属于第二个动作组。
- 子 order 在每个父组内重新计算，第二组也可以有子 order 1。
- 含子列表的父层必须声明并填写 order，工具用它判断哪些行属于同一个父对象。

三层同理，例如 `Phases[].Groups[].Actions[]`。先掌握一层，再使用嵌套。仓库 AllTypes 中的 Groups、Phases 有实际例子。

## 3. 检查规则到底是什么

检查规则就是“允许导出的条件”。例如：

- 英雄名称必须填。
- 编号不能重复。
- 等级只能是 1～100。
- 高等级英雄必须有技能。
- 所用技能的伤害必须够高。
- 动作延时不能小于 0。

工具自带的基础解析会检查“这一格能不能变成声明的类型”。比如 int 列填“苹果”会失败。但“英雄等级不能超过 100”是你的游戏约定，需要规则表达。

### 3.1 三种检查方法

| 方法 | 检查什么 | 写在哪里 |
| --- | --- | --- |
| 内置字段标签 | 必填、唯一、范围、外键存在等 | 手写表规则类上方 |
| 自定义字段规则 IFieldRule<T> | 一个字段的可复用检查 | 单独规则类，再用 Check 绑定到表规则 |
| 后置检查 Validate / ValidateTable | 多字段、整表、跨表、自定义对象与数组业务 | 手写表规则类的方法中 |

“后置”表示把 Excel 解析成配置对象以后，再做业务检查。它仍发生在发布新版本之前，不是数据发布后才检查。

执行顺序是：

```text
读取表结构和数据
  → 构造配置对象、关联直接表对象引用
  → 所有规则类的字段标签检查
  → 每个规则类的 Validate：逐条记录检查
  → 该规则类的 ValidateTable：整张表检查
  → 没有错误才写入、回读并激活新版本
```

不要依赖多个规则类之间的执行顺序，也不要让某个规则修改数据给另一个规则使用。规则的职责是检查并报告。

### 3.2 表规则的空白模板

假设已经生成了 HeroConfig，创建 `Rules/HeroRules.cs`：

```csharp
using ZrConfig.Models;
using ZrConfig.Validation;

public sealed class HeroRules : TableRules<HeroConfig>
{
}
```

`TableRules<HeroConfig>` 意味着“这个检查专门检查 Hero 表”。尖括号中的类型决定绑定哪张表，不是文件名决定。

类必须可被实例化，最容易的写法是 public sealed class，不写带参数构造函数。

工具自动发现有效表规则，不需要手写一个中央注册列表。

一张表可以有多个规则类。例如 HeroRules 和 HeroTutorialRules 都继承 TableRules<HeroConfig>，它们都会执行。所有被扫描到的规则类都会生效，不要复制旧规则文件后忘记删除或重命名类。

### 3.3 标签要放在规则类上方

正确：

```csharp
using ZrConfig.Models;
using ZrConfig.Validation;

[Required(nameof(HeroConfig.Name))]
public sealed class HeroRules : TableRules<HeroConfig>
{
}
```

不要把 Required 写到 HeroConfig.g.cs 里。生成代码是工具维护的，重生成会改掉它。

`nameof(HeroConfig.Name)` 的结果就是字符串 `"Name"`。这种写法让拼错成员名时编译器提前提醒你。

`typeof(SkillConfig)` 表示把“SkillConfig 这个类型”传给规则。

## 4. 内置字段规则

本章片段是“加到现有规则类上方的标签”，不是让你每看一个片段就新建一份同名 HeroRules。

### 4.1 Required：必须填写

```csharp
[Required(nameof(HeroConfig.Name))]
[Required(nameof(HeroConfig.Level))]
```

| 情况 | Required 的结论 |
| --- | --- |
| Name 格子空白 | 错误 |
| Level 格子空白 | 错误 |
| Level 明确填 0 | 已填写，Required 本身通过 |
| bool 明确填 false | 已填写，Required 本身通过 |
| 数组明确写 [] | 已填写；数组至少一个元素要另外检查 |

Required 按导入时的空白标记检查，未加引号的纯空格也算空白。它不等于正数或非空数组；显式字符串字面量、字符串长度和首尾空格等内容约束仍应另写规则。可空值的显式 null 与直接表对象引用的 0 都表示无值，会被 Required 拒绝。

### 4.2 Unique：不允许重复

```csharp
[Unique(nameof(HeroConfig.Name))]
```

同一张表出现两个相同值会报错。单独 Unique 不要求填值，空白会跳过。需要必填且唯一，两个标签一起加。

普通字符串 Unique 按原值比较，不能把它理解为自动忽略大小写、去首尾空格。自定义的“忽略大小写”或“两个字段组合唯一”，使用 ValidateTable。

多行数组表会先合并同一 Id 的物理行，再检查最终配置记录；因此属于一个英雄的两行动作，不会被当成两个英雄编号重复。

Unique 检查的是不同记录的字段值，不是检查一个数组内部的元素是否重复。数组、对象的内容唯一等复杂条件，需要自己的后置规则。

### 4.3 Positive：必须大于 0

```csharp
[Positive(nameof(HeroConfig.Level))]
```

填写 1 通过，填写 0 或 -1 报错。单独 Positive 会跳过空白；需要必填就再加 Required。

不要用 Positive 表达“允许 0 但不能小于 0”，那要用 Range 或自定义检查。

### 4.4 PositiveKey：必填、唯一、大于 0 一起检查

```csharp
[PositiveKey(nameof(HeroConfig.Id))]
```

相当于：

```csharp
[Required(nameof(HeroConfig.Id))]
[Unique(nameof(HeroConfig.Id))]
[Positive(nameof(HeroConfig.Id))]
```

适合常见的正整数配置编号。该校验标签不自动创建运行时查询索引；在 Excel 的第 3 行标记“主键”后，由生成的表在加载时建立索引。联合主键的单个字段允许重复，不能再给该字段添加 Unique 或 PositiveKey；只检查正数可用 Positive。

### 4.5 Range：限制上下界，包含两头

```csharp
[Required(nameof(HeroConfig.Level))]
[Range(nameof(HeroConfig.Level), 1, 100)]
```

填写 1、50、100 通过；0、101 报错；空白由 Required 报错。

Range 的最小值不能比最大值大。

**当前内置 Positive、PositiveKey、Range 只绑定普通 int、long、float、double 属性。** 其他数据类型虽然可以导表，但 byte、uint、decimal、int?、枚举等不能直接照抄这些数值标签。请用匹配类型的 IFieldRule<T> 或 Validate 写判断。

例如 decimal 的金额非负：

```csharp
if (row.Price < 0m)
    context.Error(row, nameof(MyConfig.Price), "价格不能小于零。", "PRICE_NEGATIVE");
```

这个片段中的 MyConfig 是说明性占位名字，要改成你实际的表模型，且 Price 类型要为 decimal。

### 4.6 Reference：编号必须在另一张表里存在

假设 Hero 有 `int SkillId`，Skill 有 `int Id`。

```csharp
[Reference(nameof(HeroConfig.SkillId), typeof(SkillConfig), nameof(SkillConfig.Id))]
```

意思是“英雄填的 SkillId，必须能在 Skill 表的 Id 列找到”。例如填 2001，技能表中就必须有 2001。

源字段和目标字段的 C# 类型必须完全一致。不能源 int、目标 long。

默认不允许空白。若游戏约定“0 或空白代表没有技能”，改成：

```csharp
[Reference(nameof(HeroConfig.SkillId), typeof(SkillConfig), nameof(SkillConfig.Id),
    AllowDefaultValue = true)]
```

AllowDefaultValue 表示允许默认值。对于 int，默认值是 0。其他非零值仍必须存在。

Reference 只检查值是否存在，不自动保证目标 Id 唯一、不检查技能伤害，也不会把 SkillId 自动变成技能对象。目标表通常仍要加 PositiveKey。

如果要求“人人必须选技能而且不能选 0”，常用组合是 Required + Positive + Reference，并且不启用 AllowDefaultValue。

### 4.7 Check：绑定自己写的字段规则

```csharp
[Check(nameof(HeroConfig.Name), typeof(DisplayNameRule))]
```

名字检查如何实现见下一章。只写 DisplayNameRule 类但没有 Check 标签，不会自动对 Hero.Name 执行。

### 4.8 DependsOn：声明后置检查要读取另一张表

```csharp
[DependsOn(typeof(SkillConfig))]
```

它不是“检查一个字段”的标签，它告诉工具：这个规则类的 Validate 或 ValidateTable 会使用 Skill 表。

如果需要读取两张其他表，可以写两个 DependsOn。

如果依赖表缺失、解析不完整，工具会跳过这个规则类的后置检查并报告原因。这个类里只检查本表的后置条件也会被一起跳过。希望本表规则独立执行时，把本表条件拆到另一个不带 DependsOn 的规则类。

DependsOn 不保证另一张表没有字段规则错误，只保证它有完整可用的导入数据。你的业务代码仍要防御重复键、空值和越界值。

### 4.9 Required 和 ConfigRequired 的区别

| 名字 | 放在哪里 | 检查对象 | 例子 |
| --- | --- | --- | --- |
| Required | TableRules<T> 规则类上方 | 表的顶层字段有没有填 | Hero.Name |
| ConfigRequired | 手写 class/struct 的成员上 | 对象成员有没有明确提供 | AddBuff.BuffId |

不能把 `[Required("Settings.MaxTargets")]` 当作当前内置标签的嵌套路径语法。当前标签绑定的是模型顶层属性；嵌套对象用 ConfigRequired 或 Validate 中的成员路径检查。

## 5. 自定义可复用字段规则

如果很多表都要检查名称长度，就不要在每张表里重复一大段判断。把“检查一个字符串”的方法写一次，再绑定到多张表。

创建 `Rules/DisplayNameRule.cs`：

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

解释：

- `IFieldRule<string>` 表示这条规则检查字符串字段。
- value 是这一格已经解析后的字符串。
- context.Error 负责报告该格的错误。
- 不需要自己找 Excel 行号，工具会把位置带进来。

然后在 HeroRules 和 SkillRules 上分别添加：

```csharp
[Required(nameof(HeroConfig.Name))]
[Check(nameof(HeroConfig.Name), typeof(DisplayNameRule))]
```

```csharp
[Required(nameof(SkillConfig.Name))]
[Check(nameof(SkillConfig.Name), typeof(DisplayNameRule))]
```

两张表会用同一套名称检查。

### 5.1 概率规则例子

我们约定：概率按万分比写，0 表示 0%，10000 表示 100%，2500 表示 25%。

创建 `Rules/ProbabilityRule.cs`：

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

绑定到 Hero 的 Rate 字段：

```csharp
[Required(nameof(HeroConfig.Rate))]
[Check(nameof(HeroConfig.Rate), typeof(ProbabilityRule))]
```

`Rate` 必须确实存在，并且类型是 int。

规则类型必须匹配字段类型：int 对 IFieldRule<int>，decimal 对 IFieldRule<decimal>，int? 对 IFieldRule<int?>。不匹配会产生 RULE_BINDING，不是正常的数值错误。

### 5.2 为什么还要 Required

Check 会跳过原始空白字段。所以“概率要填而且范围正确”要同时写 Required 和 Check。

自定义字段规则也可以用 context.Warning 报告警告。它的调用形式是 `context.Warning("提示", "代码")`，不传 row 和字段名，因为这里已经绑定好字段。

不要让字段规则访问游戏场景或静态保存本次数据。可以保存本次检查需要的简单状态，但不要把上一次导出数据留在全局变量中。

## 6. 逐行后置检查

标签能表达“这个字段满足一个条件”。如果条件同时用到几列，写 Validate 更清楚。

### 6.1 完整的小例子

Hero 有 Level 和 SkillId 两列。约定：90 级以上必须选择技能。

创建或修改 `Rules/HeroRules.cs`：

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

`row` 是当前这条英雄配置，`row.Level` 是它的等级。

`if` 表示“如果条件成立，就执行下面的内容”。

`&&` 表示两个条件都要成立。

`==` 是判断是否相等。`=` 是给变量赋值，不是判断。

`return` 表示结束本次方法；这里结束当前这条记录的检查，不是结束整个导出。

这个类是一份替换示例。不要同时把另一个同名 HeroRules 也保存到 Rules。

### 6.2 Error 的四个参数是什么

```csharp
context.Error(row, nameof(HeroConfig.SkillId),
    "90 级及以上英雄必须配置技能。", "HIGH_LEVEL_SKILL");
```

| 参数 | 用途 |
| --- | --- |
| row | 哪一条导入记录出错 |
| nameof(HeroConfig.SkillId) | 错误要指向哪一列 |
| 中文提示文字 | 告诉配表的人怎么修改 |
| HIGH_LEVEL_SKILL | 稳定的错误代号，便于搜索，不是字段名 |

前三个参数必须给，错误代号可以省略，省略时默认 CUSTOM。建议给清楚的代号。

不要自己 new 一个 HeroConfig，再传给 context.Error。位置记录绑定的是本次导入的原始 row；替换成另一个对象，工具无法定位。

### 6.3 条件必填：开关打开才要求描述

假设 Hero 有 Enabled（bool）和 Description（string）：

```csharp
if (row.Enabled && context.IsEmpty(row, nameof(HeroConfig.Description)))
{
    context.Error(row, nameof(HeroConfig.Description),
        "启用英雄时必须填写描述。", "DESCRIPTION_REQUIRED");
}
```

这段放进现有 Validate 方法。不要在同一个类中再创建第二个同签名 Validate。

如果不管是否启用都要填 Description，直接加 Required。条件必填才需要 if。

### 6.4 警告：提醒，但允许继续导出

```csharp
if (!context.IsEmpty(row, nameof(HeroConfig.Level)) && row.Level >= 95)
{
    context.Warning(row, nameof(HeroConfig.Level),
        "等级接近上限，请确认不是误填。", "LEVEL_HIGH_NOTICE");
}
```

Error 会阻止发布。Warning 本身不阻止发布，如果本次还有其他 Error，仍然失败。

异常不会变成“只提醒一下”：规则执行时抛异常会产生错误。因此要报告正常业务不满足，使用 context.Error，不要随手 throw。

### 6.5 已经有字段错误，为什么还会跑后置检查

工具希望一次尽量收集所有能独立判断的错误，不是第一个错误就马上退出。

字段标签不通过，不代表 Validate 一定不执行。表解析完整时，后置检查仍可能运行。

写检查时先排除明显缺失、越界的数据，避免重复或衍生报错。例如等级空白时不要再把默认 0 当作“低等级英雄”。

如果当前表在解析阶段就不完整，或依赖表不可用，该规则类的后置检查会被跳过，并有 BUSINESS_CHECK_SKIPPED 诊断。

## 7. 整表后置检查

Validate 一次看一条记录。ValidateTable 一次拿到整张表的最终记录，适合检查组合重复、总数量、比例总和等。

### 7.1 名称与等级的组合不能重复

“名称相同”未必错误，“等级相同”也未必错误，但同名且同等级可能不允许。

下面方法加进 HeroRules 类内；文件顶部增加 `using System.Collections.Generic;` 和 `using System.Linq;`。

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

Where：先挑出可以检查的数据。

GroupBy：按“名称 + 等级”分组。

Count：组里有几条记录。

foreach：逐个处理。

ToUpperInvariant：用一致的大小写形式比较，因而这里的英文名忽略大小写。

这个检查会给每条重复记录报错，方便分别找到对应 Excel 行。

### 7.2 表里必须有至少一个启用英雄

如果表不是空表，可以这样定位到第一条记录：

```csharp
if (rows.Count > 0 && !rows.Any(row => row.Enabled))
{
    context.Error(rows[0], nameof(HeroConfig.Enabled),
        "至少需要一个启用的英雄。", "NO_ENABLED_HERO");
}
```

没有数据时，不能写 rows[0]，会越界。这也是先判断 Count 的原因。

当前 ValidationContext 的常用报告接口需要一条真实导入记录。对于零行表的“必须有数据”规则，需要另行设计整表诊断或输入约束；不要伪造 row 或声称该片段已经覆盖零行情况。

### 7.3 规则不能只靠文件名执行

文件叫 `HeroRules.cs` 不代表工具知道检查 Hero。类必须继承 `TableRules<HeroConfig>`，而且方法签名要正确。

如果有多个规则类，后置检查不会只选其中一份，所有可发现且绑定有效的类都会执行。

## 8. 跨表后置检查

Reference 只问“技能编号存在吗”。跨表业务检查可以问“这个技能伤害够不够配这个英雄”。

### 8.1 完整规则例子

规则：技能伤害至少等于英雄等级 × 5。

创建 `Rules/HeroSkillRules.cs`。这是一个新类，能够与 HeroRules 同时存在。

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

教程假设 SkillRules 会检查技能编号和伤害，HeroRules 会检查 SkillId 的引用存在。

这里跳过找不到的技能，不是允许不存在，而是把“存在性”交给 Reference，避免再报一个误导人的“伤害不足”。

### 8.2 为什么逐行调用不会重复建字典

`context.GetSkillTable()` 首次访问时用本次导入的 Skill 行构造生成表，并缓存主键和分组索引；其后每条英雄记录、其他规则类以及 ValidateTable 都使用同一个实例。`TryGet` 直接查询索引。规则无需 new 字典，也无需在 ValidateTable 中提前准备数据。

本例的 Skill.Id 在 Excel 第 3 行声明为主键，缺失或重复会使表不完整，带 DependsOn 的跨表方法随之跳过。PositiveKey 仍用于正数等业务约束，业务 Error 不会自动过滤索引中的记录，所以仍检查 `skill.Id > 0` 和伤害范围。

重新导出会创建新的上下文与索引，不读取旧发布版本。`context.GetTable<SkillConfig>()` 返回同一份已缓存的 `ConfigTable<SkillConfig>`。每张表统一在 Excel 中声明主键并生成查询入口。接口边界见 [校验索引表](rules-development.md#在后置检查中使用索引表)。

### 8.3 哪些检查要声明 DependsOn

你在自定义 Validate 或 ValidateTable 中用 `context.GetSkillTable()` 或 `context.GetTable<SkillConfig>()` 时，需要声明 DependsOn。

Reference 本身包含目标表信息，不需要为了这个标签再加 DependsOn。但自己的业务代码读取另一张表，仍要加。

如果一个规则只判断 Hero.Level 和 Hero.SkillId，不读取技能表，它属于本表逻辑，可以不加 DependsOn。

## 9. 检查对象成员和数组元素

内置标签主要检查顶层字段。Settings.MaxTargets、Actions[1].Delay 等复杂路径，放在 Validate 里检查。

假设表里已经有 Settings 和 Actions，文件顶部添加：

```csharp
using Tutorial.Config;
```

Validate 里写：

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

for 是按下标逐个处理数组。

`action is Damage damage` 表示“如果这个动作实际是 Damage，就把它当作 Damage 读取专有成员”。否则不能直接假定每个动作都有 Ratio。

### 9.1 报错路径怎么写

| 检查对象 | 路径 |
| --- | --- |
| 顶层字段 | `nameof(HeroConfig.Level)` |
| Settings 的目标数量 | `"Settings.MaxTargets"` |
| 第一个动作的延时 | `"Actions[0].Delay"` |
| 第二个动作的 Buff 编号 | `"Actions[1].BuffId"` |
| 第一组第三个动作 | `"Groups[0].Actions[2].Delay"` |
| 第二阶段第一组第一个动作 | `"Phases[1].Groups[0].Actions[0].Delay"` |

**这里传完整的表记录 row，不是 action、Settings 或 buff。** 第一参数用 row，第二参数用路径，工具才知道它原来来自哪张 Excel 的哪条记录。

数组下标是排序后数组的位置，从 0 开始。工具记录了输入位置映射，仍会定位到对应的原始物理行列，而不是简单把下标当作 Excel 行号。

JSON 单格里某成员出错，会定位到承载该 JSON 的单元格；分列、多行格式可以定位到更具体的成员格子。

路径拼错会出现 VALIDATOR_FIELD。检查一下大小写、层级和下标，不要用 Actions[].Delay 作为某个具体元素的报告路径，要用 Actions[实际下标].Delay。

### 9.2 数组至少一个元素

```csharp
if (row.Actions.Length == 0)
    context.Error(row, nameof(HeroConfig.Actions),
        "至少需要一个动作。", "ACTION_REQUIRED");
```

这是“非空数组”检查。Required 只关心填写情况，写了 `[]` 仍然是空数组。

### 9.3 成员条件必填

```csharp
if (row.Settings.Interruptible &&
    context.IsEmpty(row, "Settings.MaxTargets"))
{
    context.Error(row, "Settings.MaxTargets",
        "允许打断时必须显式填写目标数量。", "TARGET_COUNT_REQUIRED");
}
```

使用成员路径检查原始填写情况。是否有默认值和是否显式填写，是两个不同问题。

## 10. 直接表对象引用：少写一次手动查找

前面的 Hero.SkillId 是整数。游戏想使用技能数据，需要拿这个编号去技能表查找。

另一种支持的写法是：Excel 字段叫 `Skill`，类型写 `SkillConfig`，数据仍填技能编号。

| 行 | Id | Name | Skill |
| --- | --- | --- | --- |
| 1 | Id | Name | Skill |
| 2 | int | string | SkillConfig |
| 3 | 主键 | | |
| 4 | 编号 | 名称 | 技能编号 |
| 5：所属端 | CS | CS | CS |
| 6 | 1001 | 小剑士 | 2001 |

生成代码后，HeroConfig.Skill 是技能对象引用。程序可以读取：

```csharp
if (hero.Skill != null)
{
    int damage = hero.Skill.Damage;
}
```

工具读取时先创建全部记录，再建立对象关联，所以可以支持双向引用和自引用。

- 目标表必须有 int Id。
- 被引用目标表的 Id 要非零且唯一。
- 非零编号找不到目标，会阻止导出。
- 空白或 0 表示没有引用，生成的引用可为 null。
- 必须选技能时，在新版本规则上写 `[Required(nameof(HeroConfig.Skill))]`。
- 不需要给对象引用再绑整数 Reference 标签；那是不同的检查方式。

在依赖目标数据的后置检查中仍声明 `[DependsOn(typeof(SkillConfig))]`，并先判断引用不为空，再访问成员。

例如：

```csharp
if (hero.Skill != null && hero.Skill.Damage < hero.Level * 5)
    context.Error(hero, nameof(HeroConfig.Skill),
        "所选技能伤害不足。", "SKILL_DAMAGE_LOW");
```

此片段属于使用 Skill 对象字段的替代版本。第 12 章完整练习使用 SkillId 整数，不要直接复制这个片段到没有 Skill 属性的模型里。

把 SkillId 改成 Skill 属于表结构变化：修改 Excel、生成代码、修改规则、重新导出，并同步游戏代码与数据。

当前直接表对象引用支持表的直接单值字段，不用于数组元素或自定义对象内部成员。

## 11. 特殊格式怎么扩展：转换器

大多数 class、struct 不需要转换器。它们直接按公开成员生成读写代码即可。

只有你需要“Excel 看起来是一种格式，但程序要保存成另一种形式”时，才使用转换器。

例子：Excel 填 `1.25` 元，程序内部保存 `125` 分。

### 11.1 定义 Money

创建 `Config/CustomTypes/Money.cs`：

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

### 11.2 定义 MoneyConverter

创建 `Config/Serialization/MoneyConverter.cs`：

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

| 方法 | 什么时候用 | 例子 |
| --- | --- | --- |
| Parse | 导表时读 Excel 文本 | "1.25" → Money(125) |
| Write | 写入二进制 | 保存整数 125 |
| Read | 回读或游戏加载 | 读取整数 125 → Money(125) |

Excel 类型填 `Tutorial.Config.Money`，数据填 `1.25`。数组类型填 `Tutorial.Config.Money[]`，数据填 `[1.25;2.50]`。

Write 和 Read 必须一一对应：写 Int32 就读 Int32，写入的字段顺序和读取顺序一致。工具会回读检查，但你仍需正确设计协议。

特定业务如“价格不能负数”，用字段规则或后置检查表达；这个转换器只负责金额格式和读写，不擅自禁止负数。

`"1"` 是转换器版本。改变二进制表示时必须升级它，例如改成 `"2"`，再重新生成、导出，并同步游戏代码。不要只更新读的一边。

转换器实例可能被复用，不能记住“上一个格子的金额”影响下一次结果，也不应保存可变共享状态。

## 12. 完整练习项目

前面是分开解释。本章把需要的 Excel、手写类型和规则拼成一套完整练习。

你可以直接下载 [完整练习包](assets/user-guide-example.zip)，解压后选择其中的 Data、Config、Rules，输出选新建的 Output。练习包只有两张 Excel 和手写源码，不含生成代码或旧导出记录。先生成代码，再导出即可。

手写类型统一放在 CustomTypes，与教程附件一致。

也可以按本章手动创建。**使用一个新的练习目录，不要把教学 Hero/Skill 表混进仓库 samples/Data。** 否则旧规则、其他同名表或额外生成模型可能影响练习。

### 12.1 最终目录

```text
ZrConfig-Beginner/
  Data/
    Hero.xlsx
    Skill.xlsx
  Config/
    CustomTypes/
      BattleSettings.cs
      BattleAction.cs
    Serialization/                   首次生成时由工具初始化
    Generated/                       首次生成时由工具创建
  Rules/
    DisplayNameRule.cs
    ProbabilityRule.cs
    HeroRules.cs
    HeroSkillRules.cs
    SkillRules.cs
  Output/                            首次导出时创建
```

下面的代码都是最终版本。之前章节中同名的示范类不要另外再保存一份。每个类在这个项目里只出现一次。

### 12.2 两张 Excel

#### Hero.xlsx

使用五行表头：字段名、类型、主键、说明、所属端。列比较多，下面分两块显示。**两块是同一张 Hero 工作表的左右两部分，不是两张 Sheet。** 第 3 行只有 A 列填写“主键”，其余格子留空。

A～I 列：

| Excel 行号 | A | B | C | D | E | F | G | H | I |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Id | Name | Level | SkillId | Rate | Enabled | Description | Settings.MaxTargets | Settings.Interruptible |
| 2 | int | string | int | int | int | bool | string | Tutorial.Config.BattleSettings | |
| 3 | 主键 | | | | | | | | |
| 4 | 编号 | 名称 | 等级 | 技能编号 | 万分比概率 | 是否启用 | 描述 | 最大目标数 | 能否打断 |
| 5：所属端 | CS | CS | CS | CS | CS | CS | CS | CS | CS |

J～P 列：

| Excel 行号 | J | K | L | M | N | O | P |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Actions[].$order | Actions[].$type | Actions[].Delay | Actions[].Ratio | Actions[].CanCrit | Actions[].BuffId | Actions[].Stacks |
| 2 | Tutorial.Config.BattleAction[] | | | | | | |
| 3 | | | | | | | |
| 4 | 动作顺序 | 动作种类 | 延时 | 伤害倍率 | 能否暴击 | Buff 编号 | Buff 层数 |
| 5：所属端 | CS | CS | CS | CS | CS | CS | CS |

第 6～8 行的 A～I 列：

| Excel 行号 | A | B | C | D | E | F | G | H | I |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 6 | 1001 | 小剑士 | 10 | 2001 | 2500 | true | 新手英雄 | 1 | true |
| 7 | 1001 | | | | | | | | |
| 8 | 1002 | 小法师 | 95 | 2002 | 10000 | true | 高级英雄 | 3 | false |

第 6～8 行的 J～P 列：

| Excel 行号 | J | K | L | M | N | O | P |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 6 | 1 | Damage | 0.2 | 1.5 | true | | |
| 7 | 2 | AddBuff | 0.5 | | | 3001 | 2 |
| 8 | 1 | Damage | 0 | 2 | false | | |

第 7 行是 1001 的第二个动作，主键 Id 重复填写，其他主字段留空。最终应该导出 2 条英雄记录，不是 3 条。

#### Skill.xlsx

| Excel 行号 | A | B | C |
| --- | --- | --- | --- |
| 1 | Id | Name | Damage |
| 2 | int | string | int |
| 3 | 主键 | | |
| 4 | 技能编号 | 技能名称 | 伤害 |
| 5：所属端 | CS | CS | CS |
| 6 | 2001 | 斩击 | 80 |
| 7 | 2002 | 火球 | 600 |

这里没有 Buff 表。练习只演示 Buff 参数格式、必填和层数规则，并不假装检查了 Buff 3001 是否存在。正式项目若需要这个约束，应增加 Buff 表和对应跨表业务规则。

### 12.3 文件：Config/CustomTypes/BattleSettings.cs

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

### 12.6 文件：Rules/ProbabilityRule.cs

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

### 12.7 文件：Rules/HeroRules.cs

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

### 12.8 文件：Rules/HeroSkillRules.cs

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

### 12.9 文件：Rules/SkillRules.cs

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

### 12.10 选好四个目录，生成和导出

| 输入框 | 选择 |
| --- | --- |
| Excel 输入目录 | ZrConfig-Beginner/Data |
| 输出目录 | ZrConfig-Beginner/Output |
| 配表代码根目录 | ZrConfig-Beginner/Config |
| C# 规则目录 | ZrConfig-Beginner/Rules |

点击“生成配表代码”，应得到 HeroConfig、SkillConfig 和对应 Codec。

再点击“校验并导出”。预期：

- 导出成功。
- 错误 0。
- 警告 1：小法师等级 95，出现 LEVEL_HIGH_NOTICE。
- Hero 表 2 条，Skill 表 2 条。
- 小剑士有 2 个动作；小法师有 1 个动作。

警告是练习中故意保留的提示，不必为了“必须 0 警告”把正确数据乱改。如果你不需要这个提醒，可以删除 LEVEL_HIGH_NOTICE 的判断。

### 12.11 故意改错，看工具怎样帮你找

每次只做一项。保存 Excel → 直接导出 → 查看提示 → 改回原值。这里只改数据，不需要重新生成代码。

| 改哪一格 | 改成什么 | 预期提示 |
| --- | --- | --- |
| Hero C6，Level | 101 | RANGE，等级必须在 1～100 |
| Hero B6，Name | 删除内容 | REQUIRED，名称必填 |
| Hero B6，Name | 名称前加一个空格 | NAME_SPACE |
| Hero E6，Rate | 10001 | PROBABILITY_RANGE |
| Hero G6，Description | 删除内容 | DESCRIPTION_REQUIRED |
| Hero D6，SkillId | 9999 | REFERENCE_MISSING |
| Skill C6，Damage | 20 | SKILL_DAMAGE_LOW，10 级英雄至少需要 50 伤害；规则定位到引用该技能的 Hero D6 |
| Hero H6，Settings.MaxTargets | 0 | TARGET_COUNT |
| Hero L7，Actions[].Delay | -1 | ACTION_DELAY，定位到第二个动作对应的原行 |
| Hero O7，Actions[].BuffId | 删除内容 | 对象成员缺少必填输入，解析失败 |
| Hero P7，Actions[].Stacks | 0 | BUFF_STACKS |
| Hero D8，SkillId | 0 | HIGH_LEVEL_SKILL |
| Skill A7，Id | 2001 | KEY_DUPLICATE，技能编号重复 |
| Hero B8 和 C8 | 小剑士、10 | NAME_LEVEL_DUPLICATE，两个英雄的名称与等级组合相同 |

“为什么没有导出新版本”看错误数量即可。只要存在 Error，当前有效版本不变。修好后再导出，日志文件会被本次内容覆盖。

练习跨表重复时，可能同时出现另一英雄引用不到旧编号的错误，这并不冲突：工具尽量把独立错误一起列出来。

## 13. 命令行使用

**CLI** 是 **Command-Line Interface（命令行界面）** 的缩写。本文说的 CLI 通常指项目中的 **ZrConfig.Cli 命令行工具**：输入一行命令告诉程序做什么、读取哪个目录、把结果写到哪里，程序执行后输出文字结果、日志位置和退出码。退出码是交给脚本判断成功或失败的数字，具体取值见第 13.4 节。

**App** 是带窗口和按钮的桌面工具。配表人员可以完全使用 App；程序员在需要脚本自动导出、构建时检查配置，或调试业务规则时使用 CLI。App 与 CLI 共用核心生成、检查和导出逻辑；相同输入、代码和规则执行相同的业务检查。`export --generate` 包含 App 的生成预检查和必要的生成步骤；普通 `export` 只导出，成功后两者都会实际写入版本数据。

### 先区分几个入口

| 名称 | 用途 | 怎样操作 |
| --- | --- | --- |
| ZrConfig.App | 桌面配表工具 | 打开窗口，选择目录并点击按钮 |
| ZrConfig.Cli | 命令行生成、校验、导出与版本管理 | 使用 `codegen`、`validate`、`export`、`support` 或 `release` |
| ZrConfig.Maintenance | 源码仓库的维护工具 | 通过 `scripts/maintain.sh` 或 `scripts/maintain.ps1` 准备 Unity 示例、检查可移植性、打包和清理 |

PowerShell、Bash 是接收和运行命令的环境，终端是输入命令的窗口；ZrConfig.Cli 是由它们启动的程序。`dotnet` 用于构建、启动 .NET 程序，`scripts/dotnet.sh` 是仓库寻找 dotnet 的包装脚本。`scripts/maintain.*` 的准备和打包命令属于维护工具，其参数不能直接传给 ZrConfig.Cli。公开命令及验证流程见 [源码构建与升级](development.md)。

下面源码示例中，`run --project src/ZrConfig.Cli` 表示构建并运行命令行工程，后面的 `--` 用于分隔 dotnet 参数和工具参数；`export` 才是交给 ZrConfig.Cli 的操作名，再后面依次是目录。使用 Windows 自包含发布包时，直接运行 `ZrConfig.Cli.exe export ...`，无需 `dotnet run --project`，见第 13.6 节。

下面 Bash 命令从仓库根目录执行；Windows PowerShell 见第 13.6 节。`/你的路径/` 是需要替换的示意路径，不能原样使用。路径包含空格时，用英文双引号包围。Data 参数是输入根目录，工具自动递归读取其下的 `.xlsx` 和 `.xls`，两种格式可以混用。

### 共用 App 项目配置

推荐直接使用 App 保存的 `project.zrconfig.json`，各命令读取其中需要的目录：

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- codegen --project samples/project.zrconfig.json
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- validate --project samples/project.zrconfig.json
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- export --project samples/project.zrconfig.json
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- support --project samples/project.zrconfig.json
```

第一个 `--project` 属于 dotnet，用来选择工具工程；`--` 后面的 `--project` 属于 ZrConfig，用来选择项目配置文件。配置中的相对目录以 JSON 文件所在目录为基准，整个项目移动后仍有效；RulesFolder 为空时只做基础校验。`--project` 不能与位置目录参数混用，重复或缺少文件参数会在执行前报错。`--accept-changes` 可用于 codegen 或 export --generate；`--rules-debug` 用于 validate/export，export 时可与 --generate 组合。Windows 自包含工具可直接执行 `ZrConfig.Cli.exe export --project "D:\Game Data\project.zrconfig.json"`。

项目文件继续使用 `DataFolder`、`OutputFolder`、`CodeFolder`、`RulesFolder` 和可选 `Target`；1.4.0 保存可选的 `Name` 与 `FormatVersion: 1`，旧文件未写这两个字段仍可读取。最近项目和界面偏好保存在本机，不随项目 JSON 共享。

### 只校验与诊断报告

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- validate --project samples/project.zrconfig.json
# 诊断报告放在 Data、Config、Rules、Output 之外
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- validate --project samples/project.zrconfig.json --report /tmp/zrconfig-validation.json
```

位置参数形式为 `validate <Data> <ConfigRoot> [Rules]`，没有 Output 参数。它使用当前代码执行数据解析、基础检查、业务规则及二进制回读验证；不写 Config 或所选 Output、不自动生成。临时编译和回读文件写入系统临时目录。代码缺失、支持源码待更新或结构变化时返回 3，表示完整校验未完成；先核对并执行生成，再重新校验。通过只校验不表示磁盘发布成功，交付仍需 export。

`validate` 支持 `--target C|S`、`--rules-debug` 和 `--report`；`export` 也支持 `--report`。报告为 JSON，保存操作状态、诊断位置和已有的运行/版本编号，不附带工作簿或源码全文；已知项目根路径替换为占位符，业务消息仍需在分享前检查。export 在生成预检查阶段就停止时不会进入导出报告保存步骤，保留终端输出或使用 App 的“导出诊断报告”。

Ctrl+C 请求安全取消。第三方解析或自定义规则返回后才会到达下一个取消检查点；已提交的生成代码不会因随后取消而撤销，已完成的数据激活仍按成功处理。完整边界见 [操作边界](release-and-acceptance.md#操作边界)。

### 一步生成并导出

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- export --generate --project samples/project.zrconfig.json
```

工具先只读检查结构、类型、支持源码和已接受的公开 API，列出必要的生成变更；首次生成或未改变受保护接口的更新可直接完成，然后读取当前 Excel、完整校验并导出。代码没有变化时跳过生成写入。改变输入根目录或删除生成文件时仍会停止，退出码为 1；核对清单后再添加 `--accept-changes`。涉及 API 删除或签名变化还需要单独迁移，普通确认不放行。生成失败不会进入导出；导出校验失败保留 current.json，但此前已成功应用的代码生成不会自动回滚。

字段增加也可能改变业务调用的公开构造函数。遇到 `API_CHANGE_REQUIRED` 时，由程序员检查旧消费方，再使用相同生成选项执行 `codegen ... --accept-api-change <预览摘要>`，之后重新导出。摘要只适用于本次变更，不存入项目配置。`Config/.schema.json` 与 Generated 一起保存，不能通过删除基线来绕过检查。App 对这类变化只展示清单并允许取消；完整规则见 [公开接口保护](server-generation.md#公开接口保护与显式迁移)。

App 与这个组合入口共用 `ProjectWorkflow`，App 通过确认窗口决定是否继续，CLI 通过上述明确选项执行。普通 `export` 不会写生成源码，仍适用于只允许检查已提交代码的自动化。`export --accept-changes` 未同时指定 `--generate` 会报参数错误。

### 按端生成和导出

字段标记和表头声明见 [C / S / CS 填写规则](config-types.md#客户端与服务端字段)。`codegen`、`validate` 和 `export` 都接受 `--target C` 或 `--target S`；省略时使用项目文件的 `"Target": "C"` / `"Target": "S"`，未指定目标端时默认 C。命令行显式指定的端优先，不改写已保存项目配置。`CS` 是字段标记，不能作为一次生成/导出的目标端。

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- codegen --project samples/project.zrconfig.json --target C
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- export --project samples/project.zrconfig.json --target C --generate
```

服务端将 `--target C` 改成 `--target S`。Windows 的 `ZrConfig.Cli.exe` 使用相同参数。一次操作只生成和发布所选端，`Generated` 与 `Output/current.json` 表示当前这次操作的结果；需要同时交付两端时，各端使用实际项目的代码和输出路径，不能把同一份生成代码当作两套模型共用。正式示例继续共用现有 Data、Config、Rules、Output，未增加平行示例目录。

生成的字段、构造函数、编解码器与查询接口均按端一致；所有表的结构哈希包含目标端，使用另一端代码直接导出会报 `SCHEMA_MISMATCH` 并保留当前发布版本。`current.json` 增加 `target`（C/S），便于交付核对。旧生成代码需重新生成后使用。

使用 Core API 时，`ModelGenerator.Generate/Prepare`、`ExportPipeline.RunWithSources/RunWithRules` 的可选 `target` 参数接受 `ConfigTarget.Client` 或 `ConfigTarget.Server`；仅程序集导出使用 `ExportPipeline.Run(data, output, assembly, target, validators)`。`ProjectWorkflow` 使用 `WorkspaceSettings.Target`。业务规则按当前端模型编译和校验，访问另一端独有字段会导致编译失败；需要不同规则时使用对应端的规则路径。

### 13.1 生成代码

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- codegen "/你的路径/ZrConfig-Beginner/Data" "/你的路径/ZrConfig-Beginner/Config"
```

参数顺序：操作名 codegen → Excel 目录 → 代码根目录。

生成器记住这套 Config 的完整输入根目录。输入根目录变化或将删除旧生成文件时，默认停止并列出新旧路径与待删文件；请先排除误选 Data 子目录。没有 API 变化时，可核对后添加 `--accept-changes`，或在 App 选择“确认生成”；删除公开表等 API 变化必须先完成上文的单独迁移。预览不会升级支持源码、创建 Config 或写入磁盘缓存；只在进程内复用未变化的类型编译产物，确认执行后才写项目。App 导出前也执行这些检查，仅在无需 API 迁移时提供“生成并导出”。旧项目仍检查待删文件与基线；整项目一起移动时相对路径绑定有效。

### 13.2 带规则导出

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export "/你的路径/ZrConfig-Beginner/Data" "/你的路径/ZrConfig-Beginner/Output" "/你的路径/ZrConfig-Beginner/Config" "/你的路径/ZrConfig-Beginner/Rules"
```

参数顺序：export → Excel 目录 → 输出目录 → 代码根目录 → 规则目录。

CLI 会拒绝未知、重复、不适用的选项和多余路径，退出码为 2，并且不会开始生成或导出。`--help` 查看用法，`export --rules-debug` 开启规则调试；目前不支持 `--dry-run`，传入它会报错。路径以 `-` 开头时，在路径前加工具自身的 `--`，其后的参数全部按路径处理。

### 13.3 不使用业务规则

明确指定 Config，省略 Rules：

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export "/你的路径/ZrConfig-Beginner/Data" "/你的路径/ZrConfig-Beginner/Output" "/你的路径/ZrConfig-Beginner/Config"
```

这样不回退到仓库示例规则。基础类型、结构、直接表对象关联和编解码检查仍然执行，但不会替你执行 HeroRules 等业务条件。

### 13.4 已经构建好的 CLI

先构建：

```bash
bash scripts/dotnet.sh build src/ZrConfig.Cli -p:NuGetAudit=false
```

再调用 DLL：

```bash
bash scripts/dotnet.sh src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll codegen "Data路径" "Config路径"
bash scripts/dotnet.sh src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll export "Data路径" "Output路径" "Config路径" "Rules路径"
```

| 退出码 | 含义 | 自动化处理 |
| --- | --- | --- |
| 0 | 本次命令成功 | 只有成功的 export 才表示本轮发布完成 |
| 1 | 生成、校验或导出检查未通过，或生成确认未接受 | 查看诊断，停止后续发布 |
| 2 | 参数错误或执行异常 | 修正命令、路径或运行环境 |
| 3 | 只校验需要先生成代码，完整校验未完成 | 核对结构变更，生成后重新校验 |
| 130 | 取消请求被处理 | 查看是否已提交代码，不能当作数据发布成功 |

帮助、只校验、版本预览命令返回 0 都不代表执行了新数据导出。版本管理使用 `release list/diff/rows/rollback/pin/unpin/cleanup`；回滚与清理默认预览，核对后加 `--apply`。命令与兼容条件见 [发布管理](release-and-acceptance.md#比较回滚与清理)。当前没有 watch 或通用 `--dry-run` 选项，不支持从单个工作簿增量导出替代全表版本。

位置参数必须完整提供：`codegen <Data> <ConfigRoot>`、`validate <Data> <ConfigRoot> [Rules]`、`export <Data> <Output> <ConfigRoot> [Rules]`；不能只给 Config 或省略中间的目录。在仓库内快速运行时，codegen/validate/export 完全不传目录仍使用 samples 默认值；独立工具需要显式目录或 `--project`。显式给出 Config 并省略 Rules 时不启用业务规则。release 必须使用 `--project`，不接受位置目录参数。自动化优先使用项目配置，或明确传入整套目录。

### 13.5 规则断点调试

validate 或 export 命令最后增加 `--rules-debug`，控制动态 Config/Rules 的 Debug 编译与 Portable PDB。它与工具项目的 Debug 构建配置是两回事：

```bash
bash scripts/dotnet.sh src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll export "Data路径" "Output路径" "Config路径" "Rules路径" --rules-debug
```

Rider 使用 .NET Project 调试配置选择 ZrConfig.Cli；Visual Studio 将 ZrConfig.Cli 设为启动项目，在调试启动配置中填写 export 与四个路径，以 Debug/F5 启动。在规则 Validate/ValidateTable 的可执行语句设断点。也可 Attach to Process 附加实际 CLI 的 .NET 进程。Debug/Release 动态编译缓存分开，日志保留完整异常和源码位置。

逐步操作、Windows PowerShell 命令、断点未绑定、异常中断与 Unity 进程选择见 [完整调试指南](debugging.md)。

CLI 不会为了等待你附加调试器而自动暂停。进程很快结束时，要安排好附加时机；是否命中断点还取决于实际 IDE 调试环境。普通配表不需要这个选项，App 界面也不提供它。

### 13.6 Windows PowerShell

使用自包含 Windows 包时，保留发布目录中的所有文件，把下例的工具目录和四个项目路径改为本机路径：

```powershell
$zrTool = "C:\Tools\ZrConfig"
& "$zrTool\ZrConfig.Cli.exe" codegen "D:\Game Data\Data" "D:\Game Data\Config"
if ($LASTEXITCODE -ne 0) { throw "生成失败，先处理诊断。" }
& "$zrTool\ZrConfig.Cli.exe" export "D:\Game Data\Data" "D:\Game Data\Output" "D:\Game Data\Config" "D:\Game Data\Rules"
if ($LASTEXITCODE -ne 0) { throw "导出失败，不发布本次数据。" }
```

使用源码工程时，在仓库根目录调用 SDK，无需 Bash：

```powershell
dotnet run --project src/ZrConfig.Cli -p:NuGetAudit=false -- codegen "D:\Game Data\Data" "D:\Game Data\Config"
dotnet run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export "D:\Game Data\Data" "D:\Game Data\Output" "D:\Game Data\Config" "D:\Game Data\Rules" --rules-debug
```

`--rules-debug` 只在需要规则断点/PDB 时添加，普通导出可省略；正式自动化同样应检查每次命令的退出码。安装 SDK 与 Windows 打包入口见 [源码构建与升级](development.md)。Windows 路径不能原样粘到 macOS，换用该电脑实际的项目路径。

## 14. Unity 运行时如何使用

本章由项目程序员完成 Unity 首次接入；配表人员按 [日常使用文档](user-guide.md) 导出并交付数据即可。

### 14.1 导表完成不等于游戏自动更新

生成的 C# 代码要在游戏工程中编译。导出的数据要放到游戏能访问的位置。游戏需要执行加载或重新加载。

你修改 Excel 并在仓库 Output 导出成功，已经运行的游戏不会凭空知道那个文件在哪里，也不会自己去读它。

### 14.2 接入 Unity 源码

在仓库根目录运行：

```bash
bash scripts/package-unity.sh
```

将输出目录中的 `Assets/ZrConfig` 及同级 `ZrConfig.meta` 放入目标工程的 Assets，保留随附 LICENSE。默认输出是 `artifacts/unity/ZrConfig-source`，重复导出时请给脚本传入新的输出目录，避免覆盖已有源码。

Runtime、Validation、Unity 适配器和 Editor 查看器全部为源码，由各自 asmdef 编译；Validation 和 Unity/Editor 仅在编辑器中使用。业务配置程序集的 asmdef 引用 `ZrConfig.Runtime`，规则程序集引用 `ZrConfig.Runtime`、`ZrConfig.Validation` 和配置程序集，限定 Editor。不添加 ZrConfig 的 Package 或预编译 DLL；从旧方式升级时先移除旧 `com.zrconfig.unity` 依赖和同名 DLL，避免重复程序集。业务 HeroConfig/SkillConfig 仍由项目生成，不携带 Excel 读取库。

仓库提供可直接运行的 [Unity 示例工程](../samples/README.md#unity-可运行示例)，共用正式 samples 的配置和导出数据。先体验示例可运行 `bash scripts/maintain.sh unity-sample`；将 ZrConfig 接入自己的工程时，继续按下面步骤操作。

### 14.3 把代码和规则放到正确的位置

一种易懂的布局：

```text
YourUnityProject/
  Data/                                  Excel，在 Assets 外也可以
  Rules/                                 导表业务规则，在 Assets 外
  Output/                                工具发布记录，不直接打入 Player
  Assets/
    Config/
      CustomTypes/
      Serialization/
      Generated/
    StreamingAssets/
      ZrConfig/                          当前配置清单与版本数据
    Scripts/
      HeroConfigReader.cs                游戏读取示例
```

- Config 根目录选择 `YourUnityProject/Assets/Config`。
- Rules 目录选择 `YourUnityProject/Rules`。
- 输出目录选择 `YourUnityProject/Output`。部署时只将 current.json 及其引用的 bytes 放入 `Assets/StreamingAssets/ZrConfig`，保留版本相对路径。
- 首次生成后等 Unity 编译结束，再进入 Play。

规则放在 Assets 外，能避免它们被游戏 Player 编译，也避免游戏程序集不需要的规则依赖。规则仍由导表 App 动态编译执行。

如果规则需要在 Rider 的 Unity solution 中直接编译，可以放在编辑器目录，例如 `Assets/Editor/ConfigRules`，并按项目 asmdef 引用 Runtime/Validation。具体目录方案见 [规则开发](rules-development.md)。

使用 asmdef 或热更新程序集时，Config 要归入你实际的配置/热更新程序集并引用 Runtime；规则如参与 Editor 编译，要引用 Validation。避免同时导入旧项目 Models DLL 和新的同名模型源码。

### 14.4 桌面版 Unity 的最小读取例子

日常只需三步：首次 `UnityConfigRuntime.Load / LoadAsync`，通过返回的快照查询，更新后 `Reload / ReloadAsync`。同步和异步都受支持，按资源系统的真实能力选择。普通生成查询不需要传入 `TableCodecs.Registry`；`IsInitialized` 可判断是否已初始化。`Initialize` 和 `Manager` 供高级工具使用，普通业务不需要直接操作管理器或懒加载器。

本例直接使用仓库正式的 samples，无需先完成第 12 章开发练习：

1. 按 [示例说明](../samples/README.md) 导出整套 samples/Data（九张表，包含万行用例），确认 0 错误。
2. 按上一节接入 Assets/ZrConfig 源码，把 unity/ZrConfig.Sample/Assets/HotUpdate/Config 下的 CustomTypes、Serialization、Generated 三个源码目录放进 Unity 的 Assets/Config；保持一套源码和唯一的 TableCodecs。
3. 按 samples/Output/current.json 的清单，将全部引用的 `.bytes` / `.json` 表数据复制到 Assets/StreamingAssets/ZrConfig 下对应版本路径，最后复制 current.json；不复制日志、编译缓存、发布记录和 `.rows.json`。仓库示例可直接用 `unity-sample` 完成部署。
4. 等 Unity 编译完成，再添加下面的读取脚本。

项目已有配置时，使用自己项目的一套 Data/Config/Rules；不要把正式示例或第 12 章的同名表混入。

正式 TableReferences.xlsx 的 Hero/Skill 表已经在第 3 行将 Id 标为“主键”。完成生成和导出后，生成的表在加载时自动建立 Id 索引，可以直接使用下面的 Get/TryGet 查询。

需要 GroupId + Id 或更多字段联合键、取整组时，参见 [联合键与分组查询](table-indexes.md)。

创建 `Assets/Scripts/HeroConfigReader.cs`：

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

在场景中创建空 GameObject，把这个脚本挂上去，然后 Play。应显示“火焰法师，技能 火球，导师 火焰法师”。调用 ShowHero(3) 可以查看空引用：技能和导师均为“无”。

这个脚本是应用入口示范，同一次播放期间只让一个入口调用首次 LoadAsync。不要给十个物体都挂一个会调用 LoadAsync 的脚本，因为静态运行时已经初始化后会拒绝重复初始化。

ReloadConfiguration 可以绑定到 UI Button。本例读取的是 Unity 工程中的 StreamingAssets 副本，每次只修改数据时按以下顺序更新：

1. 保存 Excel，重新导出到 samples/Output，确认成功。
2. 先将本次 current.json 引用的全部 `.bytes` / `.json` 表数据复制到 `Assets/StreamingAssets/ZrConfig` 下对应的 `versions/<版本号>/` 路径，完成后再替换 `Assets/StreamingAssets/ZrConfig/current.json`；管理记录和差异快照留在 Output。
3. 点击 ReloadConfiguration 按钮，重新读取该清单和数据。只更新 samples/Output 而未同步上述文件时，按钮仍会加载旧版本。

仓库 Editor 示例直接读取共享的 samples/Output；Player 使用筛选部署后的 StreamingAssets 数据。若改了表结构或类型，还需同步 Config 中对应源码、等待 Unity 编译完成，再重新进入 Play 验证；数据重载不会更新已经编译的 C# 类型。

初学解释：

| 代码 | 意思 |
| --- | --- |
| XxxConfigTable.GetCodec() | 取得本表的共享 Codec；生成查询自动传入 |
| TableCodecs.Registry / EnumerateAll() | 可选的全表目录，供全量校验、导出和动态工具查询 |
| FileConfigSource(folder) | 告诉工具去哪个真实文件目录读取 |
| Load / LoadAsync | 第一次初始化并读取版本清单；同步 / 异步 |
| await | 等这一项异步操作完成，再处理结果 |
| ConfigSnapshot | 固定版本、资源源和按需加载的行/表缓存 |
| GetHeroTable() / GetHeroTableAsync() | 首次加载英雄整表；引用在属性访问时加载，后续复用 |
| TryGet(1, out hero) | 尝试查正式示例的英雄 1 |
| Reload / ReloadAsync | 同步 / 异步重读 current.json，切换到新的懒加载快照 |
| CancellationToken | 离开场景时通知尚未完成的读取停止 |

生成的英雄表在每次加载时建立一次索引，业务只需获取已有表；热更新后更换表引用。CreateIndex 是显式构建工厂，每次调用都会分配，不能放在逐帧查询中。

#### 同步加载与不依赖 Task 的异步加载

异步接口统一返回项目自带的 `ConfigOperation<T>`，可直接 `await`，不需要 Task、UniTask 或第三方包；也可以声明 `async ConfigOperation<T>` 方法。它只实现配置加载需要的等待、结果、异常和取消，不提供完整的任务调度框架。每次调用得到的操作只 await 一次；未完成时调用 `GetAwaiter().GetResult()` 会报错，不会阻塞线程。立即就绪的资源可以返回 `ConfigOperation<byte[]>.FromResult(bytes)`，回调式加载器可用 `ConfigCompletionSource<byte[]>` 的 `Operation` 和 `TrySetResult` / `TrySetException` / `TrySetCanceled` 接入。

首次加载可以选择同步调用（与上面的异步首次加载二选一）：

```csharp
var source = new FileConfigSource(folder);
ConfigSnapshot snapshot = UnityConfigRuntime.Load(source);
// 后续重载复用上次成功的资源源。
snapshot = UnityConfigRuntime.Reload();
```

异步调用形式保持一致：

```csharp
ConfigSnapshot snapshot = await UnityConfigRuntime.LoadAsync(source, token);
snapshot = await UnityConfigRuntime.ReloadAsync(token);
```

普通 .NET 项目使用 `manager.Reload(source)` / `await manager.ReloadAsync(source, token)`。`FileConfigSource.Read` 使用同步文件读取，`ReadAsync` 使用文件流的异步读取。同步入口只调用 `Read`，异步入口只调用 `ReadAsync`；不阻塞等待异步操作，也不把同步 I/O 包装成异步结果。`DelegateSyncConfigSource` 必须分别提供两个委托，例如 `new DelegateSyncConfigSource(files.Read, files.ReadAsync)`。纯内存资源可显式提供 `(path, token) => cachedFiles[path]` 与 `(path, token) => ConfigOperation<byte[]>.FromResult(cachedFiles[path])`。

同步接口要求资源源实现 `ISyncConfigSource.Read`。仅支持网络的源只实现 `IConfigSource.ReadAsync` 即可；上次成功加载的源不支持同步读取时，无参数 `Reload()` 会报错，可显式传入支持同步的源。同步调用会占用当前线程，适合启动阶段的本地文件或已缓存资源。同步和异步重载共用重入保护：已有重载未完成时，再次重载直接报错，不排队，也不读取资源。完成后可以再次调用。

Unity 入口从主线程调用，资源读取的后续调用、await 后续代码与 Changed 通知保留 Unity 同步上下文。清单加载失败或取消时保留原快照与上次成功源。清单成功后即切换版本并触发 Changed，此时不保证所有表文件可用；文件哈希、行结构和引用在实际访问时检查。单行或整表解码失败不会发布本批半成品行，已有行缓存仍可用，可在同一有效快照重试；已经通过哈希与索引校验的表字节独立保留。

资源源的 `ReadAsync` 返回 `ConfigOperation<byte[]>`，即时结果使用 `ConfigOperation<byte[]>.FromResult(bytes)`，业务通过 await 等待。此实现不承诺加载零分配：异步状态、文件字节和新表快照仍会分配内存。

#### 按行懒加载与跨表引用

默认 `Load / LoadAsync / Reload / ReloadAsync` 只读取 `current.json` 并检查格式、路径、重复表名/文件名、行数范围和哈希格式，不枚举注册表、不创建各表 Codec 或表缓存容器，也不读取任何 `.bytes`。

每张 `XxxConfigTable` 使用静态字段、显式静态构造函数和普通 `GetCodec()` 方法提供自身 Codec。生成的 `GetHero / GetHeroAsync / GetHeroTable / GetHeroTableAsync` 直接传入该 Codec，不经过中央类型查找，也不需要注册创建工厂或反射。跨表引用只在非空且未缓存时调用目标表的 `GetCodec()`。同一程序集的多个快照复用无状态 Codec；表字节、索引、行对象和引用缓存仍分别属于各自快照。每张表都在 Excel 中显式声明主键并生成索引。生成源码的相关注释使用中文，Codec 读行上下文由显式类保存，不使用捕获局部变量的匿名函数。

唯一的 `TableCodecs` 由工具自动生成；新增表后重新生成即可，无需手工修改注册表。`EnumerateAll()` 每段最多枚举 128 张表，用于导出与完整验收。`Registry` 是可选全表目录，取得它不会创建 Codec；该目录只提供 `EnumerateCodecs()`，在显式全量校验时枚举；运行时查询不会自动查目录。业务使用生成入口，通用强类型代码使用 `snapshot.GetRow<HeroConfig, int>(HeroConfigTable.GetCodec(), id)`。无目录初始化支持生成查询；全量校验须传入目录。

采用新生成代码时须同步更新 Runtime 和 Unity 适配器；本次接口调整不改变 v4 二进制格式。

首次查询某张表时才检查该表是否存在，以及清单结构标识是否匹配当前代码；读取文件后继续检查文件哈希、文件头结构和行索引。未访问表的结构不兼容或缺失不会阻止普通初始化，会在首次查询该表时明确报错。普通重载发布新快照后，不会因为后续某次表查询失败而自动回滚；需要切换前完整验收时使用 `ReloadAll / ReloadAllAsync`。

每张表仍导出一个资源，二进制包含三个连续区域：

```text
表头与类型映射
行索引：主键、引用 Id、数据区内偏移、字节长度
行数据：各行独立编码的内容，跨表引用仍保存 Id
```

生成器通过 `WriteRow / ReadRow` 统一单行编解码，导出时写入索引和数据。打开字节资源时只解析表头和索引，不解码无关行；索引必须连续、无重叠、无重复主键，并与文件长度匹配。读取单行后还会校验其主键、Id 和数据边界。普通可寻址输出直接写入，不额外缓存整表数据；不可寻址输出先在内存组装再写出。

生成的单行接口按 Excel 声明的主键定位，支持同步和异步。业务优先使用 `GetHero/GetHeroAsync` 等生成入口；通用代码使用显式 Codec 和 `GetRow<T, TKey>`，键始终以实际类型传递。按 `Type` 查询的动态工具入口同样使用 object，不适合每帧查询：

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

资源由项目自己的 AB/Addressables/其他适配器通过 `IConfigSource` 返回字节，配置运行时不内置 AB 框架。AB 适配层负责定位、下载、请求合并、调度、调用方取消与资源句柄释放；配表只协调行索引、解码和对象缓存。源返回的托管字节在有效快照使用期间须保持不变且持续有效；索引直接保留这些字节以支持后续按行解码，释放 AB 句柄不能让这些字节失效。若资源源返回整表二进制，首次访问仍读取并校验整张表的字节，但只反序列化当前请求行；这不代表 AB 按行读取，也不会节省这一整份字节的驻留内存。懒加载减少行对象、复杂成员与查询索引的创建。

引用字段保留目标行的实际类型，例如 `SkillConfig? Skill`。getter 先返回属性内的对象缓存；未命中时向本快照查询，必要时读取目标表资源、解码目标行，成功后写入属性缓存。加载目标行只绑定它自己的延迟解析器，不自动访问它的引用。因此 `Hero → Skill → Effect → Item` 中，读取 Hero 不请求 Skill；访问 `hero.Skill` 才加载 Skill，也不会自动加载 Effect 或 Item。链式访问会依次加载业务实际访问到的每一层，重复访问复用同一对象且不产生查询分配。

每个引用同时生成 `GetSkillAsync(token)` 和 `TryGetCachedSkill(out skill)`。异步方法通过资源源异步读取，成功后与同步属性共用缓存。TryGetCached 对空引用、尚未加载的目标返回 false，不触发读取或解码；若索引已加载并确认目标 ID 无效，则报告引用错误。同步属性 `hero.Skill` 在缺少缓存时会同步加载；只想探查缓存时用 TryGetCached。业务字段与生成方法重名时，生成器为辅助成员添加数字后缀。

边界约定：0/空白引用返回 null，不访问资源；非零 ID 不存在时抛出含来源行、字段和目标 ID 的 ConfigReferenceException；文件缺失、哈希错误、解码失败及取消不缓存为 null，下一次调用可重试。缺失引用的行可先加载自身数据，错误在访问该引用时暴露；导出仍检查全部引用，显式全量验收也会检查未访问的引用。每个属性只缓存成功的对象；循环引用回到已缓存的行，不递归展开整条链。

资源源只支持异步且目标表字节尚未加载时，同步属性不能完成该读取，会提示调用 `GetSkillAsync`；不阻塞等待异步操作。若该表字节已缓存，同步属性可以直接解码另一行，无需资源源提供同步 I/O。不同表可以独立请求资源；配表不维护跨表资源等待队列。同表并发查询通过表级解码协调复用同一行对象，但可能多次调用资源接口，同路径资源请求合并由 AB 适配层完成，也可包装内置的 `CoalescingConfigSource` / `CoalescingSyncConfigSource`。适配层应让每个调用方独立取消等待，不能因为一个调用方取消而中断其他使用者。同步查询仍只使用同步源，不等待另一个异步查询；已有表字节则直接解码。

`GetHeroTable / GetHeroTableAsync` 显式加载该表全部行，构建完整主键/分组索引，并复用先前加载的单行对象；整表加载也不展开跨表引用。同步入口支持 `configs.GetHeroTable(token)`，与异步入口一样传递取消；原 `GetHeroTable()` 和无参方法组继续可用。分组查询和枚举使用整表接口；单行接口不会返回不完整的分组。每张表均声明主键并提供单行查询。

`snapshot.LoadedRowCount` 是已成功解码并绑定解析器的行数；`snapshot.Tables` 只列出已显式加载完整的表，读取它不会触发加载。重载成功后旧快照失效并清空内部行、表和字节缓存；旧快照查询及旧行引用访问（含缓存命中）会抛出 `InvalidOperationException`，在途旧读取也不能发布结果。业务收到更新通知后统一获取新对象。已取得的普通字段仍是旧值；业务自行保留的对象不会被强制回收，不提供独立行/表卸载。编辑器查看器不执行引用 getter，避免单纯展开表格就触发加载。

版本发布与旧快照失效共用一次身份切换；旧缓存清理等待期间，新 `Current` 仍可查询。后台解码完成后恢复调用线程时会再次检查取消与快照有效性。若 `Changed` 监听器再次重载，后续监听器只接收仍有效的当前版本通知，不会在新版本后再收到旧通知；监听器结束后需要查询时仍应重新获取 `Current`。

自行创建的 `ConfigManager` 可通过 `Dispose()` 永久结束使用，使已发布快照和预加载候选失效，并拒绝之后的重载。Unity 播放模式重置会自动释放当前及初始化中的管理器；重置前的在途加载迟到后会失败，不能重新发布或成功返回旧会话快照。资源源的句柄与底层请求生命周期仍由资源适配层管理。

工具需要全量验收时，先用 `new ConfigManager(TableCodecs.Registry, 清单解析方法)` 构造管理器，再调用 `manager.ReloadAll / ReloadAllAsync`，枚举生成目录、核对完整表集合并检查全部结构、数据和引用，全部验证成功后才替换快照；失败保留旧快照。`snapshot.LoadAll / LoadAllAsync` 执行同样的全量校验，在现有快照逐表预热。业务默认使用按行接口。

可选的 `ConfigPreloadPlan` 与 `ReloadPrepared/ReloadPreparedAsync` 在关键行/表准备成功后再切换版本；失败或取消保留旧快照。具体使用见下方 [预热与加载诊断](#预热与加载诊断)。

行索引使用表二进制 **v4**，旧 v2 数据及此前的 v3 试验格式不能直接加载。升级时一起重新生成代码、导出数据并更新 Runtime、共享 Serialization 与 Unity 适配器；保留历史发布目录，勿用新读取器混读旧数据。

#### 预热与加载诊断

先决定何时允许切换版本，再选择入口：

| 需求 | 入口与检查范围 |
| --- | --- |
| 尽快打开新版本，业务自行处理后续加载错误 | 默认 Load / Reload：先读取清单，表与行在访问时检查 |
| 关键配置可用后再切换 | ReloadPrepared：指定的行/表成功加载后切换，不自动展开引用 |
| 所有表及引用通过后再切换 | 独立 ConfigManager + TableCodecs.Registry + ReloadAll：完整枚举、校验后切换 |
| 提前加载当前版本的某张表 | 当前 snapshot 的 GetXxxTableAsync；不会重新发布版本 |

下面放在 Unity 主线程发起的异步初始化方法中，使用 `ZrConfig.Runtime`、`ZrConfig.Models` 和 `ZrConfig.Unity` 命名空间。`source` 为实际资源源，`token` 为当前加载操作的取消令牌；这段替代首次 `LoadAsync`，不要重复初始化：

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

后续更新只调用 `ReloadPreparedAsync`，不再 Initialize。`ConfigPreloadPlan` 不可变，每次 `WithRow` / `WithTable` 返回新计划；忽略返回值就不会加入该步骤。计划只包含显式列出的行/表，不因 Hero 引用 Skill 就自动预热 Skill，也不等同于全量引用校验。若需要该目标就明确加入计划。首次失败时尚无有效快照；已有快照时，准备失败或取消保留原版本，修复后可重试。

`ConfigLoadOptions` 在创建管理器时固定。默认在调用线程执行解码；异步资源读取完成后也不会自动分帧。如果目标环境支持线程，且编解码器与转换器线程安全、不调用 Unity 主线程 API，可以显式传入 `scheduler: BackgroundConfigLoadScheduler.Instance`。该调度器只处理异步冷加载的校验、解码与索引构建，不把同步 API 变成异步，不负责 AB 下载调度；WebGL 不支持此后台策略。

诊断通过 `snapshot.Diagnostics` 读取，未启用时 `Enabled` 为 false。`OpenedTableCount`、`RetainedBytes`、`LoadedRowCount`、`CompletedTableCount` 描述当前缓存；读取、哈希、索引、解码和整表构建时间是累计值，包括失败尝试。读取时间包含资源源等待，RetainedBytes 只统计保留的原始表字节，不包含行对象、索引、AB 框架或 Unity 的全部内存。缓存命中和首行冷加载应分别测量，不能把一次 .NET 测量等同于 Unity / IL2CPP 的性能。

### 14.5 Android、iOS、WebGL 与 StreamingAssets

上面的 `FileConfigSource` 适用于桌面真实文件路径。Android APK 中的 `jar:` 路径和 WebGL 的 URL 需要异步请求；1.4.0 已在 [Unity 适配层](../unity/ZrConfig.Sample/Assets/ZrConfig/Unity/Runtime/StreamingAssetsConfigSource.cs) 提供 `ZrConfig.Unity.StreamingAssetsConfigSource`，随 `package-unity` 源码包一起分发，直接引用即可。

在 Unity 主线程的首次加载入口中使用：

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

适配器须在 Unity 主线程构造和调用，不配置后台加载调度器，也不阻塞等待异步操作。它只实现 `IConfigSource`：首次查询和未缓存的跨表引用使用 `GetHeroAsync`、`GetSkillAsync` 等异步入口；已缓存数据可同步查询。不能直接照搬桌面例子中的 `hero.Skill` 来读取尚未加载的资源。重新加载仍使用 `ReloadAsync`，全量检查后切换使用带 `TableCodecs.Registry` 的全量入口。

普通示例在 Android / iOS / WebGL Player 自动采用这条异步读取路径；Editor 与桌面仍提供文件桥接的同步/异步演示。构建与实际设备/浏览器运行分开验收，入口、报告收集及已测范围见 [真实 Player 验收](release-and-acceptance.md#真实-player-入口)。使用具体 AB、Addressables 或热更新框架时，继续按下面的资源接口接入项目 SDK。

#### 第三方 AB 框架：同步与异步

仓库提供可直接 Play 的 [Unity AB 示例](../samples/README.md#ab-同步与异步示例)：同一场景切换同步/异步加载、查询和重载，并显示资源读取记录。尚未选定 AB SDK 的调用点集中在 `SampleAbResources`，临时使用真实文件读取，其余配置加载流程完整执行。

资源框架尚未选定时，使用通用 `DelegateSyncConfigSource` 作为接入边界。框架确定后只需实现下面两个函数，配置加载、行查询、缓存与重载接口都不变；ZrConfig 不安装具体 AB SDK，也不要求传入 Unity 原生 AssetBundle。

| 项目提供的函数 | 返回类型 | 职责 |
| --- | --- | --- |
| `LoadBytes(string path, CancellationToken token)` | `byte[]` | 调用框架真实的同步读取入口 |
| `LoadBytesAsync(string path, CancellationToken token)` | `ConfigOperation<byte[]>` | 调用框架真实的异步入口，完成后返回字节 |

这两个函数是接入项目需要实现的桥接函数，不是 ZrConfig 内置方法。完成桥接后直接绑定，无需额外工厂或管理器：

```csharp
var source = new DelegateSyncConfigSource(LoadBytes, LoadBytesAsync);
```

如果框架只提供异步资源读取，可以单独使用 `DelegateConfigSource`，但不能据此承诺未加载资源的同步查询。选择框架时应确认它是否支持所需平台上的真实同步读取；通常需要先完成远端资源的下载和准备，不能阻塞等待异步下载来伪装同步能力。

以下两组业务代码二选一用于首次初始化，之后可交替使用查询接口，共享同一份行缓存：

```csharp
// 同步链：Load -> source.Read；GetHero / Skill 缺少字节时仍只调用 Read。
ConfigSnapshot configs = UnityConfigRuntime.Load(source, token);
HeroConfig hero = configs.GetHero(1, token);
Debug.Log(hero.Skill?.Name);
bool cached = hero.TryGetCachedSkill(out var sameSkill);
```

```csharp
// 异步链：LoadAsync -> source.ReadAsync；后续资源读取同样走 ReadAsync。
ConfigSnapshot configs = await UnityConfigRuntime.LoadAsync(source, token);
HeroConfig hero = await configs.GetHeroAsync(1, token);
SkillConfig skill = await hero.GetSkillAsync(token);
Debug.Log(skill?.Name);
```

`source` 为绑定了两个读取函数的适配器，`token` 为调用方取消令牌。清单与表使用 `current.json`、`versions/<版本号>/<清单中的表文件名>` 相对路径，由桥接函数映射到资源地址。返回实际字节，不返回 URL 或文本。回调式 AB API 可用 `ConfigCompletionSource<byte[]>` 桥接：成功、失败、取消分别完成结果；不要用同步读取伪装异步，也不要轮询或阻塞等待 AB 操作。

通用委托适配器只转交读取调用。可在外层使用 `CoalescingConfigSource`（异步）或 `CoalescingSyncConfigSource`（双入口）合并同路径在途请求并隔离调用方取消；完成即移除，不缓存清单或失败结果。示例 `SampleAbResources.Source` 已包装双入口合并器。路径映射、下载、资源句柄释放和稳定字节取得仍由接入项目按框架能力处理。返回的托管字节在配置快照使用期间必须保持有效且不可修改；释放框架句柄不能回收或覆盖这份数据。

更新时先完成框架的资源更新，并确保下一次读取 `current.json` 能获得新清单，再调用 `UnityConfigRuntime.Reload(token)` 或 `await UnityConfigRuntime.ReloadAsync(token)`。如果桥接函数固定绑定某个资源版本，则创建新源并传入重载。切换前失败保留旧快照和旧源；成功后从新 `Current` 重新获取配置，在旧请求结束后由资源层释放不再使用的资源。默认重载只检查清单基本格式，表是否存在、结构是否兼容和实际表文件都在查询时校验。

配表保留 v4 行索引和已加载对象，不重复实现 AB 的资源缓存或下载调度。拿到字节后的哈希校验、解码和建索引在调用上下文执行；异步资源读取不代表解码会自动分帧。

### 14.6 重新加载后，旧变量会自己变成新数据吗

不会。旧 hero 的普通字段仍是旧值；更新后必须重新获取配置对象。

运行时校验清单成功后替换 Current。你需要从新快照按需取得行或表；清单失败时保留 Current，行加载失败时不缓存本批半成品。成功更新会使旧快照查询与旧行引用访问失效，包括已经缓存的引用。旧普通字段不会被原地修改，业务不应继续使用旧对象。

```csharp
ConfigSnapshot snapshot = UnityConfigRuntime.Current;
HeroConfig hero = snapshot.GetHeroTable()[0];
```

游戏一次需要多张表时使用同一个 snapshot。若期间成功重载，旧查询会报配置已失效；在 Changed 通知中取消或丢弃旧业务查询，再从新 Current 获取所需对象。

### 14.7 想在更新时自动刷新 UI

订阅 `UnityConfigRuntime.Changed`：

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

该片段放在引用了 ZrConfig.Runtime、ZrConfig.Unity 的 MonoBehaviour 中。退出监听时取消订阅。事件只通知成功变更；监听器自己抛异常会记录日志并隔离其他监听器。

配置按只读使用。生成行属性不可随意赋值，但数组和自定义对象的成员不做深拷贝，不要在游戏里修改它们作为角色当前状态。角色“当前血量”应存到运行时角色对象，“最大血量配置”才放在配表里。

### 14.8 热更新项目要同步哪些内容

只改数据：发布新的完整数据版本和对应 current.json。

改字段、手写成员、枚举、子类集合或转换器：重新生成并编译对应 Config/Codec，导出新数据，按你的热更新流程同时发布代码与数据。

Schema Hash 是结构标识。新代码与旧数据结构不匹配会被拒绝，这是保护机制，不能靠删除 current.json 解决。

Unity 查看器入口是 `Tools > ZrConfig > 运行时配置表`。需要配置已经成功加载。它用于看当前快照，不代替 Excel 编辑和导出。

### 14.9 通用查看器与 Odin

按 [接入 Unity 源码](#142-接入-unity-源码) 放入 Assets/ZrConfig 后，通用查看器随源码一起编译，无需额外安装。

- `Tools → ZrConfig → 运行时配置表`：选择已加载的表。
- 给 GameObject 添加 `ZrConfig/配置表查看器`：指定表名，在 Inspector 查看。
- 无 Odin 时提供分页只读查看；安装 Odin 并启用 `ODIN_INSPECTOR` 后使用 Odin 表格。
- 通用编辑器属性处理器显示生成类的只读属性，不向 Models 注入 Odin 标签。
- 查看器读取当前游戏的实际快照，热更新后刷新；普通表无需独立 View 类。

Odin 适配依据官方 [PropertyTree](https://odininspector.com/documentation/sirenix.odininspector.editor.propertytree) 和 [AttributeProcessor](https://odininspector.com/documentation/sirenix.odininspector.editor.odinattributeprocessor) 接口。未在真实 Odin 安装及 IL2CPP Player 中执行集成测试，接入具体项目后仍需验证这些环境。

### 14.10 普通 .NET 接入

不使用 Unity 时可直接使用 ConfigManager，并注入清单解析函数。下面放进已有异步初始化方法，configOutputFolder 为实际输出根目录；普通查询不需要注册表：

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

## 15. 规则设计时的几个边界

### 15.1 先判断“能否检查”，再判断“对不对”

解析失败的行不应继续被当作完整对象使用。工具会隔离不完整表，业务代码也应跳过空白、越界的关键值，避免误导性错误。

例子：Level 空白 → Required 报告“没填”。不要同时给它报“0 级英雄必须选新手技能”，因为那不是实际填写的值。

### 15.2 正常业务错误用 Error，疑似问题用 Warning

Error：一定不能发布，例如缺少目标技能。

Warning：数据可以使用，但需要确认，例如技能伤害为 0 可能是辅助技能。

不要把必须阻止的错误写成 Warning，也不要通过捕获异常后什么都不做来掩盖检查失败。

### 15.3 类型支持，不代表每个内置标签都支持

类型表中列出 decimal、uint、字典等，表示工具能解析、生成和读写。内置 Range 的类型限制仍然按第 4.5 节执行。

有特殊条件就用匹配类型的 IFieldRule<T> 或 Validate。不要把规则系统理解成可以给任何字段随意贴任何标签。

### 15.4 分列路径和 JSON 只改变输入方式

同一逻辑字段可以用 JSON 单格、成员分列等方式输入。生成模型仍然是对象，不会生成名字含点号的属性。

结构标识根据逻辑字段与类型计算。保持相同逻辑结构时，只切换输入布局不会凭空增加一个新的运行时成员。改变了字段顺序、类型或复杂结构，仍需同步代码与数据。

### 15.5 读写代码版本也要一致

当前表二进制使用 v4 行索引格式，清单容器标识仍为 ZrConfig.Binary/1。不能从名字中的“1”推断表二进制是旧版 v1。

旧 v1 数据不能直接用新 Codec 读取。升级 Runtime、生成源码或序列化协议时，重新生成和导出，同步交付。

Serialization 支持文件按内容记录版本：未修改的已知版本可自动升级；手写修改或无法识别的旧文件会保留并提示合并。普通 CLI `export` 与底层 ExportPipeline 不改源码，发现已知旧版本时需先生成或执行 `support`；CLI `export --generate` 会按组合流程更新代码，App 会在导出前提示确认生成升级。详见 [支持源码升级](development.md#支持源码升级)。

### 15.6 不是任意 C# 类型都能自动配表

填写类型见 [Excel 类型语法](config-types.md)，实现示例见本文第 1～2、10～11 章。任意 List<T>、游戏对象、UnityEngine.Object、泛型数据类等不能随意当作普通 Excel 类型使用。

集合一般使用支持的数组或 Dictionary。特殊标量可以通过转换器接入。更复杂的游戏行为留给游戏代码处理。

## 16. 常用代码速查

| 想做什么 | 代码 |
| --- | --- |
| 顶层字段必填 | `[Required(nameof(HeroConfig.Name))]` |
| 顶层字段唯一 | `[Unique(nameof(HeroConfig.Name))]` |
| 正整数唯一编号 | `[PositiveKey(nameof(HeroConfig.Id))]` |
| int 等级 1～100 | `[Range(nameof(HeroConfig.Level), 1, 100)]`，需要必填再加 Required |
| 技能编号必须存在 | `[Reference(nameof(HeroConfig.SkillId), typeof(SkillConfig), nameof(SkillConfig.Id))]` |
| 0 或空白允许无技能 | Reference 上加 `AllowDefaultValue = true` |
| 绑定名称字段检查 | `[Check(nameof(HeroConfig.Name), typeof(DisplayNameRule))]` |
| 自定义后置读取技能表 | `[DependsOn(typeof(SkillConfig))]` |
| 手写对象成员必填 | 在成员上加 `[ConfigRequired]` |
| 逐条记录 | `protected override void Validate(HeroConfig row, ValidationContext context)` |
| 整张表 | `protected override void ValidateTable(IReadOnlyList<HeroConfig> rows, ValidationContext context)` |
| 判断原格子没填 | `context.IsEmpty(row, nameof(HeroConfig.Level))` |
| 判断嵌套成员没填 | `context.IsEmpty(row, "Settings.MaxTargets")` |
| 获取另一张表 | `context.GetSkillTable()` 复用主键/分组索引；`context.GetTable<SkillConfig>()` 返回同一份索引表 |
| 行规则报错 | `context.Error(row, nameof(HeroConfig.Level), "提示", "代号")` |
| 行规则警告 | `context.Warning(row, nameof(HeroConfig.Level), "提示", "代号")` |
| 字段规则报错 | `context.Error("提示", "代号")` |
| 指向数组成员 | `context.Error(row, $"Actions[{i}].Delay", "提示", "代号")` |

记住代码所处的位置：标签在类上方；if 判断在方法内部；using 在文件开头。不能把所有片段不分位置地贴到一个文件结尾。

## 17. 教程验证与复验范围

完成教程后，可按以下步骤检查自己的配置与规则。

1. 将 [练习附件](assets/user-guide-example.zip) 解压到系统临时目录；第 12.3～12.9 节的七份完整源码应与附件同路径文件一致，不混入正式 samples。
2. 对练习目录执行第 13 章的 `codegen` 与 `export` 命令，核对 Hero 2 条、Skill 2 条、0 错误和 1 条预期警告。
3. 修改数据或规则示例后，再按第 12.11 节逐项制造错误，确认阻止发布且 current.json 字节不变；恢复数据后确认可导出。第二个动作负延时应定位到 Hero 第 7 行、第 12 列，路径 Actions[1].Delay。
4. 检查日志仍只有 export.log 与 diagnostics.json 两个固定文件，新增临时文件不进入正式 Data、Config 或 Output。


这是示例生成、规则行为、文件位置和编译验证；不代表已经完成你自己游戏工程、移动端、WebGL、IL2CPP 或具体热更新框架的实际部署测试。

## 18. 类型实现与兼容约束

本节由项目程序员与工具维护者处理，配表人员不需要编写或升级通用读写源码。

业务类型放在 Config/CustomTypes；特殊转换器按需放在 Config/Serialization。通用支持文件由 ConfigSourceLayout 初始化并按内容版本升级，手写修改保留并提示合并。它们仍属于当前项目源码的一部分，不能只删除旧文件而不验证项目改动和 Runtime/Codec 兼容性。

类必须 public 且有 public 无参构造函数；可序列化字段应可写，属性使用 public get/set。继承成员参与生成，不支持隐藏同名成员、索引器、只读/只写属性、init 属性或泛型数据类。readonly 特殊结构可通过转换器接入。构造函数和 setter 不写业务副作用。

转换器 Parse/Write/Read 使用同一约定和固定文化格式，实例会复用，不保存上个格子的可变状态；改变二进制表示需升级 ConfigConverter 的版本。普通业务类型与转换器应保持纯 C#，不依赖 UnityEngine/UnityEditor，并兼容游戏支持的 C# / .NET API。

矩形数组的长度校验由 Serialization/ConfigReadContext.cs 的 ReadDimension、ClaimElements 和 ReadLength 处理。生成模板和仓库唯一的支持源码同步维护。

SchemaHash 依据逻辑字段、顺序和类型结构计算，JSON/分列/多行等输入布局不参与。保持相同逻辑结构时只切换输入布局不改变运行时字段；代码与导出数据必须使用一致的结构标识。

生成 Codec 直接调用成员和读写方法，运行时不通过反射访问字段。数组写长度后写元素，空数组读取用 Array.Empty；字符串使用栈缓冲/ArrayPool，避免临时 UTF-8 byte[]。实际字符串、非空数组、对象、表行仍需要分配，不承诺零 GC。表的属性只读；数组元素和自定义对象不会深拷贝，业务代码应将加载后的配置视为只读。导表侧的类型发现/对象构造使用缓存元数据和反射，导出结束后不静态持有项目程序集。

普通字段不保存每个值的类型标签。多态位置写文件局部索引，完整类名集中放在表文件头中；读取时只映射一次，再通过生成的 switch 调度。索引不是稳定业务编号。结构哈希包含二进制版本、class/struct 形态、传递成员、必填声明、枚举定义、子类集合及转换器版本；重命名类或改变结构需要重新生成代码及数据。

二进制版本及旧数据兼容要求见 [15.5 读写代码版本](#155-读写代码版本也要一致)。工具维护者修改底层格式时必须同时升级 BinaryFormat.Version；项目程序员重新生成匹配的代码和数据，再通过实际交付方案同步更新 Runtime、Codec 与数据。工具不自动迁移旧格式数据。

读取限制：单数组最多 1,000,000 元素，每张表数组总元素最多 8,000,000，递归深度最多 64，字符串 UTF-8 最多 16 MiB。超长、截断、非法索引和类型表被拒绝，发布失败保留上次 current.json。

仍不支持 Dictionary 以外的任意泛型容器、秩大于 3 或连续嵌套超过三层的数组，以及游戏行为对象/UnityEngine 对象直接序列化。特殊类型可通过转换器扩展。
