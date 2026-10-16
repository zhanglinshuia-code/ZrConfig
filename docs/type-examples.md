# 一张全类型配表

本文的 Data、Config、Rules、Output 路径均相对于 `samples/` 示例目录。

基础值、复杂对象与集合示例集中在 [Data/AllTypes.xlsx](../samples/Data/AllTypes.xlsx) 的第一张工作表 `AllTypes` 中。结构、多态和多行动作都集中在这张表中；同目录的 CompositeKeys.xlsx 演示双键和三键，TableReferences.xlsx 演示直接表对象引用。“类型索引”和“填写说明”是辅助导航页。

AllTypes 使用五行表头：字段名、类型、主键、说明、所属端，Id 在第 3 行标为“主键”，数据从第 6 行开始。三条配置的首行是第 6、8、11 行，续行是第 7、9、10 行。类型索引链接、冻结行、动作下拉和填写说明均与此布局一致；[Config/Tables.json](../unity/ZrConfig.Sample/Assets/HotUpdate/Config/Tables.json) 仅声明 ByQuality 自定义分组。

## 打开先看到基础值

最左侧 A～I 列依次为：

| 字段 | Id | ByteValue | ShortValue | IntValue | LongValue | FloatValue | DoubleValue | BoolValue | StringValue |
|---|---|---|---|---|---|---|---|---|---|
| 类型 | int | byte | short | int | long | float | double | bool | string |
| 常规示例 | 1 | 12 | -12 | -123 | 9007199254740993 | 1.25 | 3.141592653589793 | 1 | 保留首尾空格 |

J～N 紧接着展示 sbyte、ushort、uint、ulong、decimal。后面依次排列其他标量、一维/二维/三维数组、结构与多态分列、多行列表，最后是 JSON 类型和输入格式的专门示例。数字按文本保存，避免 long、ulong、decimal 精度丢失。

## 类型覆盖

| 类型类别 | 本表示例 |
|---|---|
| byte、sbyte、short、ushort、int、uint、long、ulong、float、double、decimal、bool、string | 各自的 Value / Array / Array2D / Array3D |
| 枚举 | QualityValue 及其数组 |
| vector2、vector3 | Vector2Value、Vector3Value 及其数组 |
| kv | KvValue 及其数组，保留重复 Key |
| 自定义 struct | Range.Min / Max；Ranges[] 多行列表 |
| 自定义 class 与嵌套对象 | Options.Label / Range.Min 等；OptionsList[] 多行列表 |
| 多态对象 | Action.$type；Actions[].$type 多行动作 |
| 自定义转换器 | MoneyValue 及其数组，金额文本转整数分 |
| 原始 JSON 类型 | RawJsonValue / Array / Array2D / Array3D |
| 可空值 | OptionalInt / UInt / ULong / Decimal / Bool / Quality / Money / Range；OptionalValues |
| 字典 | StatMap、PriceMap、QualityMap、ListMap |
| 矩形数组 | IntMatrix、DecimalCube、OptionalMatrix |
| 嵌套多行列表 | Groups[].Actions[]、Phases[].Groups[].Actions[] |
| json<T> 输入格式 | JsonIntArray，解析后为 int[] |

这里覆盖 13 种基础值及各类扩展类型、集合、可空值和 JSON 输入格式。自定义类型可以继续扩展；`json<T>` 是输入格式，不增加运行时包装类型。结构和战斗参数按成员分列填写，JSON 文本只出现在明确的 RawJson 类型示例中。

基础值、枚举、向量、kv、Money、RawJson 展示标量和一至三层数组。class / struct / 多态展示标量分列与一维多行数组；对象的二维/三维单格组合仍在自动化测试中覆盖。

## 同一张表里的多行

本表有 103 个逻辑字段、144 个 Excel 列；3 条配置占 6 行。Id 为 1、2、3 的记录分别占 2、3、1 行；基础值等父字段只在每条记录的首行填写。重复 Id 用于明确标记归属。

通过“类型索引”点击 Actions，可以看到以下动作（这里只列出部分列）：

| Id | Actions[].$order | Actions[].$type | Actions[].Ratio | Actions[].BuffId |
|---|---|---|---|---|
| 1 | 1 | Damage | 1.5 | |
| 1 | 2 | AddBuff | | 1001 |
| 2 | 1 | Damage | 0 | |
| 2 | 2 | Damage | 0.8 | |
| 2 | 3 | AddBuff | | 0 |
| 3 | | | | |

运行时 Actions 长度分别为 2、3、0。Ranges[] 和 OptionsList[] 分别展示普通 struct/class 的多行列表，各列表独立排序与汇总。

多态类型通过下拉框选择 Damage / AddBuff；只填当前子类的参数，其他子类专属列留空。BuffId 必填，显式 0 合法。整组多行列留空表示空数组。不能合并数据单元格或只对局部列排序。

## 新增类型的填写示例

| 字段 | 类型 | 数据示例 |
|---|---|---|
| UIntValue | uint | `4294967295` |
| ULongValue | ulong | `18446744073709551615`（文本） |
| DecimalValue | decimal | `1.2345678901234567890123456789`（文本） |
| OptionalInt | int? | `123` / `null` / 留空 |
| OptionalValues | int?[] | `[1;null;0]` |
| StatMap | Dictionary<string,int> | `["hp"=100;"attack"=25]` |
| PriceMap | Dictionary<int,decimal?> | `[1001=1.25;1002=null]` |
| IntMatrix | int[,] | `[[1;2];[3;4]]` |
| DecimalCube | decimal[,,] | `[[[1.25;2.5]];[[3.75;4.5]]]` |

类型索引中的 **Groups** 跳转到二层列表，**Phases** 跳转到三层列表。Id=2 的父级顺序故意填写 2、1、2，演示归组后自动排序；重复父级 `$order` 继续填写它的子动作。各层子列表独立排序，父参数续行留空或原样重复。结构和动作成员继续分列。

## 使用与检查

App 四个目录依次选择 samples/Data、samples/Output、unity/ZrConfig.Sample/Assets/HotUpdate/Config、unity/ZrConfig.Sample/Assets/Editor/ConfigRules，所有示例共用这一套配套文件。按 [使用文档](user-guide.md#3-第一次导出) 生成并导出，无需编写类型或读写代码。

已有后置检查覆盖最大目标数、区间顺序、动作延时、伤害倍率和 Buff 层数，错误会定位到具体成员或动作行。填写规则见 [类型语法](config-types.md)，业务含义有疑问时请与项目程序员确认。

示例已包含完整工作簿，直接按 [示例说明](../samples/README.md) 使用。
