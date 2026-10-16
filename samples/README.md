# 示例总入口

**中文** | [English](README.en.md)

所有正式 Excel 都在 [Data](Data) 目录树内，可以在 Data 下用子目录分类；工具递归读取 `.xlsx`、`.xls`，全部示例共用 Unity Assets 中的配置源码、Editor 检查和本目录 Output。示例已经配好业务类型与规则，日常使用无需编写 C# 或读写支持，也无需先重建工作簿。

## 打开哪个工作簿

| 工作簿 | 看什么 | 导出的逻辑表 |
| --- | --- | --- |
| [AllTypes.xlsx](Data/AllTypes.xlsx) | 基础值、枚举、数组、对象、多态、字典和多行列表 | AllTypes |
| [AllTypes10k.xlsx](Data/AllTypes10k.xlsx) | 一万条全类型记录、跨表引用和同表引用链；加载测试基准输入 | AllTypes10k |
| [CompositeKeys.xlsx](Data/CompositeKeys.xlsx) | 同表标记双主键与三主键 | Drop、SkillLevel |
| [TableReferences.xlsx](Data/TableReferences.xlsx) | 双向/自引用、共享目标与空引用 | Hero、Skill |
| [ItemCatalog.xlsx](Data/ItemCatalog.xlsx) | 道具名称、堆叠、品质和启用状态；可复用字段检查 | ItemCatalog |
| [ShopOffers.xlsx](Data/ShopOffers.xlsx) | 商品价格、限购、上架重复与道具状态；整表和跨表检查 | ShopOffers |
| [QuestRewards.xlsx](Data/QuestRewards.xlsx) | 奖励数量、开放等级、道具堆叠与停用任务 | QuestRewards |

`#说明`、`#类型索引` 等辅助 Sheet 不参与导出；Excel 自动产生的 `~$` 锁文件、macOS 的 `._` 附属文件和链接文件/目录会跳过。隐藏业务 Sheet 和实际隐藏目录仍参与读取，备份不能放在 Data 的任何层级。子目录不构成表名命名空间，跨目录、跨格式的表名也不能重复。

## 在 App 中怎样使用

点击“打开项目配置”，选择现成的 [project.zrconfig.json](project.zrconfig.json) 即可一次恢复全部路径。也可使用“浏览…”逐项设置：

共用项目同时生成七种语言和独立的 `server-v1` 接口，并导出 Binary + JSON。目标端仍为 C，服务器 API 的加载与生命周期能力不改变 Excel 的 C/S 分端规则；切换为 S 时，应使用同端生成物和数据。旧 C#、Java 等查询入口保留，服务器新入口见[多语言运行时](../docs/server-generation.md#服务器契约家族与显式加载)。

| 输入框 | 路径 |
| --- | --- |
| Excel 输入目录 | samples/Data |
| 输出目录 | samples/Output |
| 配表代码根目录 | unity/ZrConfig.Sample/Assets/HotUpdate/Config |
| 可选规则目录 | unity/ZrConfig.Sample/Assets/Editor/ConfigRules |

保存 Excel 后点击“校验并导出”；出现生成提示时核对路径与变更清单，再确认生成。选择完整 Data 根目录和 Config 根目录，具体按钮行为、取消与错误处理统一见 [使用文档](../docs/user-guide.md#31-三个按钮什么时候用)。命令行自动化见 [开发教程](../docs/project-development.md#13-命令行使用)。

新建表按 [五行表头](../docs/user-guide.md#41-统一使用五行表头) 填写：第 1 行字段名、第 2 行类型、第 3 行主键、第 4 行说明、第 5 行所属端，第 6 行起填数据。单列主键与联合主键使用相同的表头布局。

`AllTypes.xlsx` 与其他正式表统一使用五行表头：字段名、类型、主键、说明、所属端，第 6 行起填写数据。Id 在第 3 行标为“主键”；共用 [Tables.json](../unity/ZrConfig.Sample/Assets/HotUpdate/Config/Tables.json) 仅保留按 QualityValue 查询的 ByQuality 自定义分组。

正式示例统一保留第 5 行所属端，共享字段明确填写 CS，缺失 C/S/CS 标记报错，App 目标端默认 C。填写方式见[五行表头与 C/S/CS 标记](../docs/config-types.md#客户端与服务端字段)。旧表须先保存并关闭，再在第 5 行插入所属端行并明确填写 C/S/CS；数据整体下移到第 6 行，检查冻结表头、下拉和导航位置后重新生成并导出。不要用重建命令覆盖原数据。

## 成功后核对什么

未修改的正式示例应成功、0 错误、4 条预期警告，并显示 6 条信息和 12 条成功诊断（不包括工具流程日志）；current.json 包含以下逻辑记录数：

| 表 | 记录数 |
| --- | ---: |
| AllTypes | 3 |
| AllTypes10k | 10,000 |
| Drop | 3 |
| SkillLevel | 4 |
| Hero | 3 |
| Skill | 2 |
| ItemCatalog | 3 |
| ShopOffers | 3 |
| QuestRewards | 3 |
| 合计 | 10,024 |

AllTypes 的多行动作会先合并为逻辑记录，所以 Excel 的物理行数不等于表记录数。交付 current.json 及其引用的全部 `.bytes` 和 `.json` 表数据，保留版本相对路径；共用示例的双格式清单同时引用两者。管理记录、`.rows.json` 行快照和诊断留在工具侧。以本次成功结果和当前清单为准，不能只看 Output 中已有文件就认定成功。

## 一万行全类型加载用例

`AllTypes10k.xlsx` 使用五行表头，数据为第 6～10005 行，Id 连续为 1～10000。复用 AllTypes 的 103 个逻辑字段，覆盖基础值、枚举、向量、kv、Money 转换器、对象、多态、交错数组、可空值、字典、矩形数组与原始 JSON。对象和嵌套列表使用 `json<T>` 单格输入，输出仍为原声明的强类型，因此一条配置恰好占一行，不覆盖原 `AllTypes.xlsx` 的分列、多行教学布局。

`(Id-1)%3` 为 0/1/2 时分别使用常规/边界/默认样本，共 3334/3333/3333 条。附加 `Hero`、`Skill` 与 `Previous` 三个引用字段，共 106 字段。默认样本的跨表引用为空；Previous 指向前一条，首条指向自身。数据确定性生成，适合重复测量混合类型负载，不代表所有业务表的数据分布。

公开仓库已包含这份工作簿，直接执行下面的 `unity-sample` 即可生成代码、导出并部署数据，无需重建 Excel。全部工作簿一起导出为九张表、10024 条记录；只读取清单或查询其他表时不会解码本表。

在 Unity 场景中观察读取日志和 LoadedRowCount，或运行 [公开源码验证](../docs/development.md#验证公开源码) 中的 AB 验证入口。需要测量性能时，在自己的目标平台分别记录首行加载、完整表、缓存命中和重载，区分资源读取与解码耗时。可选的预热与诊断接口见 [预热与加载诊断](../docs/project-development.md#预热与加载诊断)。

## 主键和对象引用示例

CompositeKeys.xlsx 的第 3 行标“主键”：SkillLevel 使用 SkillId、Level、Quality 的完整组合；Drop 使用 GroupId、Id 的完整组合。说明在第 4 行，数据从第 6 行开始。每个单独分量允许重复，完整组合不可重复。详见 [主键填写](../docs/table-indexes.md#怎样填写)。

TableReferences.xlsx 的 Hero.Skill 类型为 SkillConfig，格子填技能 Id；Skill.Hero 类型为 HeroConfig，格子填英雄 Id。英雄 1 引用技能 10，技能 10 反向引用英雄 1。Mentor 展示自引用，OtherSkill 展示共享目标；英雄 3 的 0 和空白引用表示无引用。目标 Id 必须非零且唯一，不存在的非零 Id 会阻止导出。

对象关联由工具完成。程序员查询完整键、分组和关联对象的方法见 [查询接口](../docs/table-indexes.md#生成与调用) 与 [Unity 接入](../docs/project-development.md#14-unity-运行时如何使用)。

后置规则可通过 `context.GetSkillLevelTable()` 等生成入口查询主键与分组，首次访问建立索引，本次导出的所有规则共用。现有对象引用仍直接访问 `row.Item` 或 `row.Skill`，无需重复查找；调用示例和依赖声明见 [校验索引表](../docs/rules-development.md#在后置检查中使用索引表)。

AllTypesRules 检查目标数量、区间顺序、动作延时、倍率和 Buff 层数。主键与直接对象关联也有基础完整性检查。填写格式见 [全类型表说明](../docs/type-examples.md)。

## 后置检查与四种日志示例

三份新工作簿一起使用，代码在 [ConfigRules](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules)，使用本仓库当前 App 的 `Info` / `Success` 诊断；Unity 示例的 Assets/ZrConfig/Validation 源码提供相应检查接口。每份工作簿的 `#说明` 页给出具体修改格子、预期代号和恢复方法。

| 日志 | 默认示例 | 是否阻止导出 |
| --- | --- | --- |
| 信息 Info | 道具停用、免费商品、停用任务和三张表的数量汇总 | 否 |
| 成功 Success | 名称格式、道具参数、商品参数或任务奖励的某项检查通过 | 否 |
| 警告 Warning | ItemCatalog!C6 大堆叠；ShopOffers!D7 高限购；QuestRewards!B6 首尾空格、D6 需要拆分堆叠 | 否 |
| 错误 Error | 负价格、非法数量、上架组合重复、启用商品或任务引用停用道具 | 是，保留上次有效版本 |

- [SampleNameRule.cs](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/SampleNameRule.cs)：可复用字段检查，供道具和任务通过 `[Check]` 绑定；空单元格由 `[Required]` 检查。
- [ItemCatalogRules.cs](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/ItemCatalogRules.cs)：逐行检查堆叠和品质，整表汇总启用数量。
- [ShopOfferRules.cs](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/ShopOfferRules.cs)：逐行检查价格和限购，跨表检查道具状态，整表检查重复上架组合。
- [QuestRewardRules.cs](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/QuestRewardRules.cs)：跨表比较奖励数量与堆叠上限；停用任务保留基础参数检查。

成功诊断只表示对应检查通过，最终是否发布仍看导出结果。四种诊断都保留文件、Sheet、行列、字段与代号，写入运行日志和 `Logs/diagnostics.json`。错误练习请在系统临时目录复制整套 Data 后操作，避免破坏正式工作簿；详细接口见 [日志级别](../docs/rules-development.md#日志级别)。

## Unity 可运行示例

现成工程在 [unity/ZrConfig.Sample](../unity/ZrConfig.Sample)，使用 Unity **6000.3.26f1**（Unity 6），提供 Editor、桌面 Player 与 Android/iOS/WebGL 读取入口；各平台实际验收结果见 [验收记录](../docs/release-and-acceptance.md#验收与团队试用记录)。它读取本目录的 Data 和 Output；共用的配置源码与后置检查直接保存在该工程的 Assets 下。

### 首次运行

完整仓库（包括此 Unity 示例）可克隆或解压到任意可写目录，允许空格、中文或不同盘符；保持仓库内部结构。安装 .NET SDK 10，在仓库根目录执行：

```bash
bash scripts/maintain.sh unity-sample
```

Windows：

```powershell
powershell -File scripts/maintain.ps1 unity-sample
```

新检出的仓库没有 Library、artifacts 或 Output，这是正常状态。框架源码已在 Assets 中，首次运行前执行准备命令生成数据；该命令需要访问 NuGet，Unity 首次导入自身依赖时需要访问 Unity 包源。

命令读取已有工作簿，按需生成代码并校验导出到 samples/Output，最后为 Player 部署当前清单引用的数据。全部 ZrConfig 源码直接在 Assets 下编译，无需构建或安装通用包；框架和配置统一使用 Assets 中的源码。不会重建或覆盖 Excel，也不会删除历史发布版本。

1. Unity Hub 中添加仓库内的 `unity/ZrConfig.Sample` 工程；保持它在仓库中的相对位置。
2. 等待 Assets 导入与编译完成，打开 `Assets/ZrConfigSample.unity`（也可用菜单 `Tools → ZrConfig → 打开示例场景`）。
3. 点击 Play，在 Game 窗口看到 AB 加载示例、模式切换、版本号、查询面板和资源读取记录；启动只读取九张表的清单，已加载行数为 0。

未修改的正式数据应显示：英雄 1 为“火焰法师”，技能“火球”，导师为自身；英雄 3 的引用为空；掉落组 10 有两条；技能组合键 `(101, 1, 2)` 的伤害为 150。也可输入不存在的键观察空结果。

选中场景里的 `ZrConfig Sample` 对象，可在 Inspector 查看已加载表；菜单 `Tools → ZrConfig → 运行时配置表` 打开通用查看器。`验证示例数据` 菜单检查共享导出与桌面数据的加载、键查询、分组、引用和重载，也检查配置源码程序集与 Editor 规则隔离。`验证 AB 同步与异步` 进一步执行真实同步/异步读取、引用、缓存与重载验证；`验证示例程序集` 可单独检查程序集隔离。

### AB 同步与异步示例

Editor 与桌面 Player 的调用链是 `ZrConfigSample → UnityConfigRuntime / 生成的查询 API → DelegateSyncConfigSource → SampleAbResources → 真实文件`。还没有接入第三方 SDK，也没有构建原生 AssetBundle；临时资源实现代替的是 AB 底层读取，清单校验、二进制解码、对象引用和缓存均真实执行。

1. 默认异步模式进入场景。读取记录应只有 `ZrConfig/current.json` 请求，已加载 0 行。
2. 点击“查询英雄”，看到 `Hero.bytes` 和 `Skill.bytes` 的异步请求、英雄与技能名称、导师和反向引用；再次查询同一英雄，本次资源读取为 0 次。
3. 切换“同步模式”，先点击“同步重新加载配置”，再查询英雄。此时对应请求走同步入口。仅切换模式不清空缓存，因此直接查询已加载对象不会产生新的资源请求。
4. 点击“查看空引用示例”“查询整组”“查询技能”，分别观察空引用、掉落整表分组和三字段组合键查询。两种模式都支持这些操作；输入不存在的键会显示未找到配置。
5. 若要演示首次同步初始化，在退出 Play 后选中 `ZrConfig Sample`，取消脚本上的 `Use Async`，再 Play。同步读取会占用当前线程；异步读取完成后的解码不会自动分帧。

[SampleAbResources.cs](../unity/ZrConfig.Sample/Assets/Scripts/SampleAbResources.cs) 是唯一的临时 AB 替换点。`LoadBytes` 调用 `FileConfigSource.Read`，`LoadBytesAsync` 调用真正的文件异步读取；没有用同步读取伪装异步，也没有写死演示数据。`ZrConfig/` 是示例资源地址前缀，映射到当前数据根目录，后面的 `versions/<版本号>/<表文件名>` 保持不变。Game 窗口保留最近 14 条记录，Console 也会输出请求、完成、失败和字节数。

接入正式 AB 时，在这两个方法里替换读取调用并返回 `byte[]`，业务入口不需要修改。按实际框架处理地址映射、下载准备、同路径请求合并、调用方独立取消和资源句柄释放。临时文件实现不额外缓存资源，每次读取产生独立托管数组；ZrConfig 快照负责保留已加载字节和对象。正式桥接返回的字节也必须在快照使用期间持续有效且不可修改。

Android/iOS/WebGL Player 自动使用 `CoalescingConfigSource → StreamingAssetsConfigSource → UnityWebRequest`，固定异步模式；同步切换步骤仅用于 Editor 与桌面演示。移动端和浏览器仍执行真实的解码、引用与查询，不要求接入第三方 SDK。

### 导出后重载

Editor 直接读取 `samples/Output`。Play 期间，保存已有 Excel 并在 App 中“校验并导出”，成功后点击 Game 窗口的“重新加载配置”。也可以从仓库根目录运行：

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export --project samples/project.zrconfig.json
```

进入场景通过 `UnityConfigRuntime.Load(source)` 或 `LoadAsync(source)` 只加载清单，不创建各表 Codec 或表缓存容器；生成查询直接使用各表的 `GetCodec()`，按当前模式同步或异步加载单行与引用，掉落分组查询显式加载掉落整表。界面显示当前已加载行数与真实资源读取次数。清单失败保留旧快照，行加载失败不缓存半成品，可再次点击重试。重载成功后旧快照与旧行引用失效，场景以 UnityConfigRuntime.Current 为准换用新快照并清空显示结果；Changed 延迟或迟到时也不会继续读取旧快照。界面在 Layout 阶段生成显示数据，按钮操作在布局结束后执行。场景退出时取消正在进行的加载并退订配置、资源日志通知。接口说明见 [开发教程](../docs/project-development.md#同步加载与不依赖-task-的异步加载)。正式 AB 更新时先准备完整资源、确保下一次读取获得新清单，再调用重载；清单成功不代表全部表已验收，表文件仍在查询时校验。表结构或业务类型发生变化时，先退出 Play，在 App 中重新生成并导出；回到 Unity 等待 Assets 中的源码导入、编译完成，再 Play。仅修改业务源码不需要复制或重新部署 Models DLL。

### 配置源码与 Editor 后置检查

源码实际保存在 Unity 工程的 Assets 下，可在 Project 窗口直接查看、修改和调试。所有路径均使用英文，App/CLI 和 .NET 编译检查工程共用这一份源码：

| 内容 | 仓库中的唯一位置 | Unity 中的位置 | 编译程序集 |
| --- | --- | --- | --- |
| 运行时加载、快照、Codec 接口 | [ZrConfig/Runtime](../unity/ZrConfig.Sample/Assets/ZrConfig/Runtime) | Assets/ZrConfig/Runtime | ZrConfig.Runtime |
| 校验框架 | [ZrConfig/Validation](../unity/ZrConfig.Sample/Assets/ZrConfig/Validation) | Assets/ZrConfig/Validation | ZrConfig.Validation（仅 Editor） |
| Unity 加载入口与查看器 | [ZrConfig/Unity](../unity/ZrConfig.Sample/Assets/ZrConfig/Unity) | Assets/ZrConfig/Unity/Runtime、Editor | ZrConfig.Unity、ZrConfig.Unity.Editor |
| 生成模型、Codec、查询表和 TableCodecs | [Config/Generated](../unity/ZrConfig.Sample/Assets/HotUpdate/Config/Generated) | Assets/HotUpdate/Config/Generated | ZrConfig.Models |
| 手写类型与序列化/转换器 | [Config/CustomTypes](../unity/ZrConfig.Sample/Assets/HotUpdate/Config/CustomTypes)、[Config/Serialization](../unity/ZrConfig.Sample/Assets/HotUpdate/Config/Serialization) | Assets/HotUpdate/Config/CustomTypes、Serialization | ZrConfig.Models |
| 后置检查 | [ConfigRules](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules) | Assets/Editor/ConfigRules | ZrConfig.Sample.Rules.Editor |

[配置 asmdef](../unity/ZrConfig.Sample/Assets/HotUpdate/Config/ZrConfig.Models.asmdef) 把整套配置源码编译为独立程序集，只引用通用 Runtime；[规则 asmdef](../unity/ZrConfig.Sample/Assets/Editor/ConfigRules/ZrConfig.Sample.Rules.Editor.asmdef) 引用模型、Runtime 和 Validation，平台限定为 Editor，不进入 Player。生成器的代码根目录就是 Assets/HotUpdate/Config，生成结果直接写入 Generated，无需复制文件或更新 Package。

手写扩展放 CustomTypes、Serialization；后置检查可随时直接修改 ConfigRules 下的代码。Generated 会在下次生成时更新，不在里面保存需要保留的手写逻辑。编辑表结构后退出 Play，重新生成并导出，等待 Unity 编译完成即可运行。

上述目录是本仓库的整理约定。App/CLI 会从输入框选择的代码根目录递归发现手写 C#，不按 CustomTypes 等名称筛选；根目录和任意命名的子目录均可使用。生成时排除根目录下的 Generated，导出时将其一同编译；两者均跳过 bin、obj、点号开头的文件/目录和链接。

暂未绑定具体热更框架。确定方案后，把 **ZrConfig.Models** 纳入该框架的热更程序集配置；热更 DLL 的构建、加载顺序、AOT 元数据及业务程序集引用按框架接入。当前示例提供独立源码程序集和数据重载，尚未实现运行时替换 C# 程序集。

后置检查在 Unity 中可以编译、查看和编辑；导出时仍由 App/CLI 加载同一份纯 C# 源码执行，操作顺序和断点进程见 [规则开发](../docs/rules-development.md)。不要给共用规则类整体添加 `#if UNITY_EDITOR`；它的编辑器归属由目录与 asmdef 确定。需要 UnityEditor API 的按钮代码放 Assets/Editor，保持与纯 C# 规则分开。

重新生成 Unity 的 IDE 项目后，可以看到 Runtime、Validation、Unity、Models 和规则的独立源码工程；双击脚本即可查看或修改原文件。App 项目不额外挂虚拟 Source 链接。

### 桌面构建与文件归属

Build Profiles / Build Settings 已包含示例场景；选择桌面平台，输出到 `artifacts` 下的构建目录。桌面 Player 经临时 AB 桥接读取 `Assets/StreamingAssets/ZrConfig` 中准备好的版本。构建前退出 Play 并重新运行 `unity-sample`，以刷新随包数据；Player 不会读取仓库的 Output。Android/iOS/WebGL Player 自动使用内置异步 StreamingAssets 适配器；接入第三方资源 SDK 时再按 [资源源接入](../docs/project-development.md#14-unity-运行时如何使用) 实现对应桥接。

- [ZrConfigSample.cs](../unity/ZrConfig.Sample/Assets/Scripts/ZrConfigSample.cs)：实际加载、生命周期、查询与运行界面。
- [SampleAbResources.cs](../unity/ZrConfig.Sample/Assets/Scripts/SampleAbResources.cs)：临时 AB 同步/异步字节读取与日志，正式 SDK 的替换位置。
- [SampleProject.cs](../unity/ZrConfig.Sample/Assets/Editor/SampleProject.cs)：打开场景和验证数据的编辑器菜单。
- [UnitySample.cs](../scripts/UnitySample.cs)：统一 .NET 准备入口；Bash/PowerShell 使用同一实现。
- 配置与规则直接位于 `unity/ZrConfig.Sample/Assets/HotUpdate/Config`、`unity/ZrConfig.Sample/Assets/Editor/ConfigRules`；Unity 编译产物位于 Library/ScriptAssemblies。框架源码统一位于 `Assets/ZrConfig`，不引用 artifacts 中的旧包或 ZrConfig 预编译 DLL。StreamingAssets 是 Player 部署文件，不是另一套导出目录，数据产物可由准备命令重建。

## 在自己的项目中使用

运行时统一使用 `UnityConfigRuntime.Load / LoadAsync` 首次加载、快照查询、`Reload / ReloadAsync` 更新。第三方 AB 框架通过 `DelegateSyncConfigSource` 绑定真实同步、异步两个读取函数；框架尚未确定时不引入具体 SDK 或原生 AssetBundle 依赖。完整示例见 [AB 接入](../docs/project-development.md#第三方-ab-框架同步与异步)。引用缓存检查使用 `TryGetCachedSkill` 等明确命名。

正式示例的业务类型、生成代码和规则位于上面的 Unity Assets 目录；按 [项目开发教程](../docs/project-development.md) 与 [规则开发](../docs/rules-development.md) 接入自己的工程。修改表结构后重新生成并导出。

独立练习附件使用另一套 Hero/Skill 教学字段，不能混入正式 Data。练习步骤见 [开发教程第 12 章](../docs/project-development.md#12-完整练习项目)。

运行时按行加载与 AB 适配方式见 [开发教程](../docs/project-development.md#按行懒加载与跨表引用)。当前源码使用 v4 行索引格式，运行示例前需用本版工具重新生成、导出并更新 Assets 中的 Runtime 源码。

## 只校验与发布记录

打开统一 `samples/project.zrconfig.json` 后，可先执行“只校验”，它不修改共享 Config 或 Output。结构变化时先检查生成预览；发布后从“版本历史”查看表和记录差异。CLI、兼容回滚、真实 Player 构建与报告说明见 [发布管理与验收](../docs/release-and-acceptance.md)。全部工作簿继续共用现有 Config、Rules 和 Output。

Android/iOS/WebGL 的普通示例使用异步 StreamingAssets 读取，验收包构建与设备报告收集见 [目标平台验收](../docs/release-and-acceptance.md#真实-player-入口)。第三方 AB SDK 尚需按项目指定版本单独接入。
