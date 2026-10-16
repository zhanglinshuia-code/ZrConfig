# ZrConfig 使用文档：配表、导出与验收

**ZrConfig user guide: authoring, export, and acceptance**

ZrConfig 是面向 Unity / C# 项目的 Excel 配置工具，核心是**通过可编程后置检查，让配表业务错误在导出时被发现**。商品引用停用道具、技能动作参数不合理等问题，往往要运行到对应玩法才能暴露；程序员将这些条件写成 C# 规则后，工具每次导出自动检查，并把问题定位回 Excel 的具体单元格。

ZrConfig is an Excel configuration tool for Unity / C# projects, centered on **programmable post-validation that finds business errors during export**. Offers referencing disabled items or invalid skill parameters might otherwise surface only in gameplay. Once developers encode constraints in C#, every export checks them and locates problems at specific Excel cells.

工具支持较全面的数据结构，适用于道具、商店、任务、技能与战斗动作等配置，同时自动生成强类型代码与二进制或 JSON 数据。程序员可通过 Visual Studio / Rider 的断点和单步调试验证检查逻辑；配表人员继续使用桌面工具填写、导出和处理报错。

Rich structures suit items, shops, quests, skills, and battle actions, with automatic typed code and Binary/JSON output. Developers verify checks using Visual Studio/Rider breakpoints and stepping; authors keep using the desktop tool to edit, export, and resolve errors.

本文面向日常填写 Excel、处理数据错误和交付配置的人。使用顺序是：**填写并保存 Excel → 只校验并修正问题 → 需要时生成代码 → 校验并导出 → 确认结果并交付游戏**。也可直接“校验并导出”，工具仍会执行完整检查。

This guide is for everyday workbook editing, data fixes, and delivery. Follow **edit/save Excel → validate and fix → generate when needed → validate/export → verify and deliver**. You may also directly Validate and export; full checks still run.

工具会生成需要的表代码和读写支持。日常使用不要求编写 C# 类、序列化代码或转换器。项目需要额外数据类型、业务规则或 Unity 接入时，由程序员按 [项目开发教程](project-development.md) 准备。现有项目提供的代码目录和规则目录，应按项目约定选用。

The tool generates table code and serialization support. Everyday use needs no C# classes, serializers, or converters. Developers prepare extra types/rules/Unity integration using [project development](project-development.md). Use existing code/rules directories according to project conventions.

## 配表时能用到的能力

**Authoring capabilities**

| 需求<br>Need | 工具支持<br>Support |
| --- | --- |
| 提前发现业务错误<br>Catch business errors early | 执行项目用 C# 编写的逐行、整表、跨表后置检查，结合字段校验，在发布前报告问题<br>Project C# row/table/cross-table post-checks plus field validation before publication |
| 快速找到需要修改的数据<br>Find data to fix | 报告工作簿、Sheet、行列和字段路径，嵌套成员也能定位回原始格子<br>Workbook, sheet, coordinates, field paths; nested members map to original cells |
| 配基础属性和集合<br>Primitive attributes/collections | 数值、文字、布尔、枚举、向量、数组、字典、可空值、矩形数组和 JSON<br>Numbers, text, bool, enums, vectors, arrays, dictionaries, nullable values, rectangular arrays, JSON |
| 配复杂技能和行为<br>Complex skills/behavior | 对象按成员分列；多态动作按种类填写；多行合并成列表，并支持嵌套和各层排序<br>Split object members, typed polymorphic actions, multi-row/nested lists sorted per level |
| 配等级、品质、掉落组<br>Levels, quality, drop groups | 一个或多个字段组成主键，生成完整键与分组查询<br>Single/composite primary keys with full-key/group queries |
| 关联其他表的数据<br>Link other tables | 填目标 Id，工具关联为对象；支持双向、自引用和共享目标<br>Target Ids become object references, including bidirectional/self/shared references |
| 保留有效交付数据<br>Preserve valid deliveries | 检查成功后才更新当前版本；导出失败保留上一份有效版本<br>Current version updates only after checks succeed; failed exports retain the last valid release |

详细结构例子见 [配表结构总览](config-types.md#先选配表结构) 与 [Excel 填写语法](config-types.md)。项目专属的动作种类、类型和业务条件由程序员准备，日常按模板填写即可。

See [structure overview](config-types.md#先选配表结构) and [Excel syntax](config-types.md). Developers provide project action kinds, types, and constraints; authors fill templates.

## 阅读路线

**Reading route**

| 要做什么<br>Task | 阅读位置<br>Read |
| --- | --- |
| 第一次使用<br>First use | [1. 准备](#1-开始前准备什么) → [2. 目录设置](#2-打开工具并设置目录) → [3. 第一次导出](#3-第一次导出)<br>[1. Prepare](#1-开始前准备什么) → [2. Paths](#2-打开工具并设置目录) → [3. First export](#3-第一次导出) |
| 创建或修改 Excel<br>Create/edit Excel | [4. 表头](#4-excel-表头怎么写)、[5. 填写语法](#5-数据怎么填写)<br>[4. Headers](#4-excel-表头怎么写), [5. Input syntax](#5-数据怎么填写) |
| 处理校验报错<br>Fix validation errors | [6. 校验结果](#6-怎样理解校验与后置检查)、[8. 排错](#8-按错误找到修改位置)<br>[6. Results](#6-怎样理解校验与后置检查), [8. Troubleshooting](#8-按错误找到修改位置) |
| 确认交付内容<br>Verify delivery | [7. 导出结果](#7-成功后交付什么)、[10. 验收](#10-每次修改后的验收清单)<br>[7. Outputs](#7-成功后交付什么), [10. Acceptance](#10-每次修改后的验收清单) |
| 日常操作与结构变化<br>Daily operations/schema changes | [9. 日常修改](#9-日常修改怎样操作)<br>[9. Routine edits](#9-日常修改怎样操作) |
| 写类型、转换器、规则或游戏代码<br>Types, converters, rules, game code | [开发文档入口](README.md#程序开发与调试)<br>[Developer documentation](README.md#程序开发与调试) |

## 1. 开始前准备什么

**1. Before starting**

使用项目提供的 ZrConfig App、Excel 文件和目录配置。Windows、macOS 的自包含发布包已携带运行环境，日常配表不需要安装 .NET SDK，也不要求安装 Microsoft Excel 才能导出；编辑工作簿需要能保存 `.xlsx` 或 `.xls` 的表格软件。

Use your project's App, workbooks, and paths. Self-contained Windows/macOS packages include the runtime; everyday authoring needs no .NET SDK or Microsoft Excel for export. Editing requires spreadsheet software saving .xlsx/.xls.

源码启动、SDK 安装、构建和打包由开发者参考 [源码构建与升级](development.md)。打包后，Apple Silicon App 位于 `artifacts/macos/osx-arm64/ZrConfig.app`，Intel Mac 使用对应架构的包；Windows x64 使用 `artifacts/windows/win-x64/ZrConfig/ZrConfig.App.exe`。Windows 发布文件夹要整体保留，不能只取出 exe。

Developers handle source startup, SDKs, builds, and packaging using [building/upgrading](development.md). Apple Silicon packages are artifacts/macos/osx-arm64/ZrConfig.app; Intel uses its matching architecture. Windows x64 runs artifacts/windows/win-x64/ZrConfig/ZrConfig.App.exe. Keep the entire Windows folder, not just the exe.

先确认本项目的四个路径：Excel 输入、输出、代码根目录、可选规则目录。仓库已经带好完整示例，可直接使用 [samples](../samples/README.md)，无需先创建或重建示例。

Confirm four paths: Excel input, output, code root, optional rules. Complete [samples](../samples/README.md) are included; no creation/rebuild is needed first.

| 谁做<br>Who | 内容<br>Responsibility |
| --- | --- |
| 配表人员<br>Authors | 填写 Excel、保存、生成/导出、处理数据报错、确认交付结果<br>Edit/save Excel, generate/export, fix data errors, verify delivery |
| 程序员<br>Developers | 首次游戏接入，新增业务类型和业务规则，处理代码编译与版本兼容问题<br>Initial game integration, new types/rules, compile/compatibility issues |
| 工具<br>Tool | 解析数据、生成表代码、执行检查、写入版本数据和本次日志<br>Parse, generate, validate, write versioned data/current logs |

### 1.1 App 和 CLI 是什么

**1.1 App and CLI**

**App** 指带窗口、目录输入框和按钮的 ZrConfig 桌面工具，本篇教程使用的就是它。

**App** is the windowed ZrConfig desktop tool with path fields/buttons, used by this guide.

**CLI** 是 **Command-Line Interface（命令行界面）** 的缩写，中文可直接理解为“命令行工具”。在 ZrConfig 文档里通常指 `ZrConfig.Cli`：在 Windows PowerShell 或 macOS 终端里输入命令，指定 Excel、输出、代码和规则目录，再执行操作。执行结果显示为文字和日志。

**CLI** means **Command-Line Interface**, usually ZrConfig.Cli here. Enter commands in Windows PowerShell/macOS Terminal, specifying Excel/output/code/rules paths and operations; results appear as text/logs.

| 操作方式<br>Interface | 怎样使用<br>Use | 适合谁<br>Audience |
| --- | --- | --- |
| App 桌面工具<br>Desktop App | 选择项目，点击“只校验”“生成配表代码”或“校验并导出”<br>Choose project and click Validate only, Generate configuration code, or Validate and export | 日常配表、修正数据和人工验收<br>Everyday authoring, data fixes, manual acceptance |
| CLI 命令行工具<br>CLI | 输入命令，或让脚本自动调用<br>Enter commands or invoke through scripts | 程序员、自动化导出、规则断点调试<br>Developers, automation, rule breakpoint debugging |

例如，App 中点击“校验并导出”，对应命令行里的 `export --generate` 流程：先检查并更新必要的代码，再校验导出。普通 `export` 只使用现有代码导出；CLI 遇到输入根目录变化或删除生成文件时，还需核对清单并添加 `--accept-changes`。日常使用 App 无需同时启动 CLI。需要命令行时再阅读 [命令、参数与 Windows/macOS 示例](project-development.md#13-命令行使用)。

App Validate and export corresponds to CLI export --generate: check/update necessary code, then validate/export. Plain export uses existing code. Changed input roots or generated-file deletions require CLI preview review and --accept-changes. App users need not also start CLI. See [commands and OS examples](project-development.md#13-命令行使用) when needed.

### 1.2 安装与升级

**1.2 Installation and upgrades**

macOS：退出正在运行的 ZrConfig，把与本机架构匹配的 `ZrConfig.app` 放入“应用程序”（`/Applications`），从这里启动。升级时保留上一份有效发布包，再替换这个应用；不要同时从旧发布目录启动另一份。窗口标题显示工具版本，源码构建与打包方式见 [源码构建与升级](development.md)。

macOS: quit ZrConfig, place architecture-matching ZrConfig.app in /Applications, and launch there. Preserve the last valid package before replacing the App; avoid also starting an old copy. The title shows version. See [building/upgrading](development.md).

Windows：退出旧工具，将新版发布文件夹整体解压到约定的工具位置，再打开其中的 `ZrConfig.App.exe`。App、CLI、DLL 和运行环境属于同一份发布包，升级时整体替换，不混合两个版本的文件。

Windows: quit the old tool, extract the complete new release folder to the agreed tool location, and open ZrConfig.App.exe. App, CLI, DLLs, and runtime form one package; replace together without mixing versions.

应用程序与项目数据分别保存。升级 App 不需要复制或重建 Data、Config、Rules、Output，也不需要删除原来的设置。启动后打开原项目的 `*.zrconfig.json`，确认四个路径，再执行“校验并导出”；如提示代码或支持源码升级，核对清单后按项目约定处理。代码结构或二进制格式发生变化时，由程序员同步游戏端代码与数据，见 [打包与安装](development.md#打包与安装) 和 [支持源码升级](development.md#支持源码升级)。

Apps and project data are separate. Upgrading needs no copying/rebuilding of Data/Config/Rules/Output or settings deletion. Open the original *.zrconfig.json, confirm paths, and Validate and export. Review requested code/support upgrades under project policy. Developers synchronize game code/data for schema/binary changes; see [installation](development.md#打包与安装) and [support upgrades](development.md#支持源码升级).

## 2. 打开工具并设置目录

**2. Open the tool and set paths**

主界面优先显示项目名称、目标端、最近操作和当前版本。可以打开项目配置、选择最近项目，或通过“首次接入”检查并保存项目；四个路径位于可展开的“项目目录设置”中：

The main view prioritizes project name, target side, latest operation, and current version. Open settings, select a recent project, or use onboarding to check/save. Four paths are in expandable “项目目录设置” (Project directory settings):

| 输入框<br>Input | 选择什么<br>Select | 平时怎么处理<br>Routine use |
| --- | --- | --- |
| Excel 输入目录<br>Excel input directory | 工作簿所在的 Data 根文件夹<br>Data root containing workbooks | 递归读取根目录和全部子目录中的 .xlsx、.xls<br>Recursively reads .xlsx/.xls in all subdirectories |
| 输出目录<br>Output directory | 项目约定的 Output 文件夹<br>Project Output folder | 查看本次结果、日志和交付文件<br>Results, logs, delivery files |
| Unity 配表代码根目录<br>Unity configuration code root | 项目提供的 Config 文件夹<br>Project Config folder | 选根目录即可，由工具生成、程序员接入<br>Select root; tool generates and developers integrate |
| C# 规则目录（可选）<br>Optional C# rules directory | 项目提供的 Rules 文件夹<br>Project Rules folder | 有规则就使用该目录；没有时留空<br>Select provided rules, otherwise leave blank |

代码目录要选 **Config 根目录**，不要选其中的 Generated 子目录。界面标签里的子目录提示用于程序接入，日常操作只需正确选择根目录，无需创建或编辑里面的读写文件。

Select **Config root**, not Generated. Subdirectory hints support developer integration; everyday users only choose the correct root without creating/editing serialization files.

点击生成或导出时使用输入框当前路径，手动输入、浏览和拖入均生效。工具递归读取所选代码根目录中的 C#，不限制手写源码的子目录名称；生成时跳过工具自己的 Generated 输出，编译缓存、隐藏文件和链接不参与读取。

Operations use current typed/browsed/dropped paths. C# is discovered recursively without restricting handwritten subdirectory names. Generation skips its own Generated output; compilation caches, hidden files, and links are excluded.

“规则目录留空”表示不用额外业务规则；“填写一个空目录”不等同于留空，会报规则不可用。如果项目已经配置规则，不要为绕过错误而清空它，应按诊断修改数据或联系规则维护者。

Blank Rules means no extra business rules. A specified empty folder instead reports unavailable rules. Do not clear configured rules to bypass errors; fix data or contact maintainers.

配置好后点击“保存项目配置”，保存为 `project.zrconfig.json`（建议放在对应项目根目录；仓库示例放 samples）。配置使用相对于该文件的路径，项目一起移动后仍可打开。CLI 的 `codegen/validate/export/support --project <项目文件>` 也使用这份配置，无需重复填写目录。打开损坏的配置不会部分覆盖当前路径。单独“生成配表代码”只检查 Excel 输入和 Config 目录，输出及规则目录可暂不填写；导出时再检查输出和已填写的规则目录，新输出目录可自动创建。

Click “保存项目配置” (Save project configuration) as project.zrconfig.json, preferably at project root (samples here). Relative paths move with the project. CLI codegen/validate/export/support --project uses the same file. Invalid settings never partially overwrite current paths. Standalone generation checks only Data/Config; Output/Rules may be unset then. Export checks Output and specified Rules; new output directories can be created.

四个框都支持拖文件夹。拖文件时使用它的父目录；多文件必须来自同一目录，跨目录多选不会更改原值。拖拽只填路径，不移动文件。生成或导出期间目录锁定。

All four fields accept folders. Dropped files use their parent; multiple files must share a directory or the value stays unchanged. Dragging sets paths without moving files. Paths lock during generation/export.

“生成语言”决定输出哪些语言的代码，可多选；“数据格式”可选 Binary、JSON、Binary + JSON，随项目配置保存。它们与顶部的界面语言、C/S 目标端分别设置。仓库示例选择七语言和双格式，目标端为 C；日常沿用项目设置。语言集、目标端或接口结构变化由程序员按 [多语言生成与迁移](server-generation.md) 核对，普通生成确认不能批准 API 破坏性变化。

Generation languages select one or more code outputs. Data format selects Binary, JSON, or both and is saved with the project. These differ from UI language and C/S target. Samples use all seven languages, dual-format, and C; retain project settings routinely. Developers review language/side/API changes via [multi-language migration](server-generation.md); ordinary generation confirmation cannot approve breaking APIs.

### 2.1 目录和语言会记住

**2.1 Remembered paths and language**

App 会记住上次的目录。切换项目时，先确认四个路径属于同一个项目，不能只换 Excel 路径却继续使用另一项目的代码或输出。

The App remembers paths. On project switches verify all four belong together; do not change only Excel while keeping another project's code/output.

“打开最近项目”恢复已保存项目；“移除记录”只从最近列表移除，不删除项目文件。第一次接入时点击“首次接入”，填写项目名称、目标端和目录，先“检查目录与表结构”，通过后“保存项目并开始使用”。目录检查只读，不替你生成或发布数据；修改向导中的目录后需重新检查。

Recent projects restore saved settings; removing a recent entry deletes no project file. Onboarding asks name/side/paths, then “检查目录与表结构” (Check paths/schema), then “保存项目并开始使用” (Save/start). Checking is read-only and does not generate/publish. Recheck after wizard path edits.

项目状态显示当前版本、是否启用业务规则和最近操作。文件或设置变化后，上次结果可能标为过期；保存 Excel 后重新校验。状态栏的旧版本号不表示本次导出成功。

Status shows current version, business-rule enablement, and latest operation. File/settings changes may mark results stale; save Excel and revalidate. An old status version does not mean this export succeeded.

顶部“界面语言 / Language”可选中文或 English。本次操作的日志使用点击按钮时的语言；中途切换只影响界面和后续操作，不会翻译旧日志。切换语言不改变数值填写语法或导出数据。

Top “界面语言 / Language” selects Chinese/English. Logs use the language at operation start. Mid-run switching affects UI/future operations, not old logs. It changes neither numeric syntax nor exported data.

首次在仓库内运行通常使用 samples；独立工具可能需要手动选择目录。恢复设置、默认路径与偏好文件的技术细节见 [调试指南](debugging.md)。

First repository launch usually uses samples; standalone tools may need manual paths. See [debugging](debugging.md) for restored settings/defaults/preference files.

### 2.2 查看与清理日志

**2.2 View and clear logs**

默认“问题列表”按严重程度和表显示诊断，支持级别筛选、表筛选和搜索。正文是连续的只读文本，可以拖动跨行选择；按住 Ctrl（macOS 也支持 Cmd）点击可选择多条不连续的问题。“复制所选问题”复制选区涉及的完整诊断，右键“复制”复制选中的原文。点击问题后可打开文件、显示目录或复制单元格位置；多行消息中的任意一行都对应同一条诊断。嵌套成员位置仍对应实际 Excel 单元格。自动跳转单元格依赖表格软件，工具提供打开工作簿与复制位置。

Default diagnostics group by severity/table, with filters/search. Continuous read-only text supports cross-line drag selection and Ctrl (also Cmd on macOS) multiselection. “复制所选问题” (Copy selected issues) copies whole diagnostics touched by selection; context-menu Copy copies selected text. Select an issue to open its file, reveal its directory, or copy cell location; each line of a multiline message maps to one diagnostic. Nested members retain real cells. Automatic cell navigation depends on spreadsheet software; the tool opens workbooks/copies locations.

“运行日志”保留完整技术详情，颜色区分普通信息、成功、警告与错误，右键可以复制选中文字或全部日志。切换标签保留阅读位置和选区；界面语言变化不改写已有诊断正文。

Run logs preserve technical detail, color-coded by info/success/warning/error, with selected/all-text copying. Tab switches preserve position/selection; UI language changes do not rewrite existing diagnostic bodies.

日志区域右上角的“清理日志 / Clear Logs”清空当前显示、选区和语言提示，并回到顶部；磁盘上的日志、诊断报告和已导出数据保留，操作结果状态也保留。生成、导出期间按钮暂时禁用，操作结束后恢复。清理后切换语言不会恢复旧内容，下次操作会正常显示新日志。

“清理日志 / Clear Logs” clears current display, selection, and language hints and returns to the top. Disk logs/reports/data and operation status remain. Disabled during generation/export, it returns afterward. Language switching does not restore cleared content; the next operation displays new logs normally.

## 3. 第一次导出

**3. First export**

先确认窗口顶部的“生成 / 导出端”：C 为客户端，S 为服务端。所属端为 CS 的字段两端都包含，缺失所属端会报错；当前目标端会随项目配置保存。生成代码与交付数据必须属于同一端，切换后按生成提示更新代码。

Confirm “生成 / 导出端” (Generation/export target): C client, S server. CS fields belong to both; missing side is an error. Target is saved. Generated code/delivered data must match; update code when switching sides.


用仓库现成示例：

With included samples:

最快方式：点击“打开项目配置”，选择 [samples/project.zrconfig.json](../samples/project.zrconfig.json)，然后执行下面第 5 步。也可以逐项设置：

Fastest: open [samples/project.zrconfig.json](../samples/project.zrconfig.json), then go to step 5. Or set individually:

1. Excel 输入选择 `samples/Data`。<br>
   Excel input: samples/Data.
2. 输出选择 `samples/Output`。<br>
   Output: samples/Output.
3. 代码根目录选择 `unity/ZrConfig.Sample/Assets/HotUpdate/Config`。<br>
   Code root: unity/ZrConfig.Sample/Assets/HotUpdate/Config.
4. 规则目录选择 `unity/ZrConfig.Sample/Assets/Editor/ConfigRules`。<br>
   Rules: unity/ZrConfig.Sample/Assets/Editor/ConfigRules.
5. 点击“校验并导出”；若出现生成提示，核对输入目录和变更清单后点击“生成并导出”。<br>
   Click Validate and export; if prompted, review root/changes and choose Generate and export.
6. 等待本次结束，确认成功、错误为 0，阅读警告，再打开输出目录。<br>
   Wait for completion, verify success/0 errors, read warnings, then open output.

未修改的正式示例应导出九张表、10024 条逻辑记录（包含一万行全类型加载用例），0 错误、4 条预期警告；新增检查还会显示信息与成功日志，详见 [后置检查示例](../samples/README.md#后置检查与四种日志示例)：

Unmodified samples export nine tables/10024 records, including the 10,000-row case, with 0 errors/4 expected warnings. New checks also show Info/Success; see [post-validation samples](../samples/README.md#后置检查与四种日志示例):

| 工作簿<br>Workbook | 导出的表与行数<br>Exported tables/rows |
| --- | --- |
| AllTypes.xlsx | AllTypes：3 |
| AllTypes10k.xlsx | AllTypes10k：10000（全类型加载用例）<br>AllTypes10k: 10000 (all-types loading case) |
| CompositeKeys.xlsx | Drop：3；SkillLevel：4 |
| TableReferences.xlsx | Hero：3；Skill：2 |
| ItemCatalog.xlsx | ItemCatalog：3 |
| ShopOffers.xlsx | ShopOffers：3 |
| QuestRewards.xlsx | QuestRewards：3 |

AllTypes 比较宽，先看最左边的基础值列，再用工作簿里的类型索引查找数组、对象和动作列。它与其他正式表统一使用五行表头：第 3 行主键、第 4 行说明、第 5 行所属端、第 6 行起数据，详细位置见 [全类型表说明](type-examples.md)。

AllTypes is wide: start with leftmost primitives, then use its type index for arrays/objects/actions. It shares five headers: keys row 3, descriptions row 4, sides row 5, data row 6. See [all-types layout](type-examples.md).

### 3.1 三个按钮什么时候用

**3.1 When to use each button**

| 按钮<br>Button | 适用情况<br>When | 成功后得到什么<br>Result on success |
| --- | --- | --- |
| 只校验<br>Validate only | 日常修表，先检查类型、主键、引用和业务规则<br>Routine type/key/reference/business checks | 本轮检查结果，不发布数据<br>Current results, no publication |
| 生成配表代码<br>Generate configuration code | 首次接入、表结构或类型变化<br>Initial integration/schema/type changes | 更新后的代码和结构摘要，不发布数据<br>Updated code/schema summary, no publication |
| 校验并导出<br>Validate and export | 准备交付本轮修改<br>Deliver this edit | 完整检查通过后激活新数据版本<br>Activate a new data version after full checks |

| 改了什么<br>Change | 先生成代码<br>Generate first? | 再校验导出<br>Validate/export afterward? |
| --- | --- | --- |
| 修改数值、文字、数组内容<br>Values, text, array contents | 不需要<br>No | 需要<br>Yes |
| 增删数据行<br>Add/remove data rows | 不需要<br>No | 需要<br>Yes |
| 新增/删除/重命名字段，改变字段类型<br>Add/remove/rename fields or change types | 需要，先与程序员确认结构变化<br>Yes; confirm schema with developers | 需要<br>Yes |
| 新增/删除/重命名表，调整主键<br>Add/remove/rename tables or keys | 需要，先确认游戏接入和关联表<br>Yes; confirm game integration/references | 需要<br>Yes |
| 程序员修改业务规则<br>Developer edits business rules | 通常不需要，由程序员确认<br>Usually no; developer confirms | 需要<br>Yes |
| 程序员修改自定义类型或读写约定<br>Developer edits custom types/serialization | 由程序员完成代码与版本同步<br>Developer synchronizes code/version | 需要<br>Yes |

日常可直接点击“校验并导出”：工具先检查代码，发现首次生成、结构变化或支持源码升级时列出变更，确认“生成并导出”后继续；只改数据时直接导出。单独的“生成配表代码”按钮仍可用于只更新代码，此时不会导出数据。上表的“先生成”可通过导出前的确认完成。

Validate and export first checks code, previews initial/schema/support changes, and continues after Generate and export confirmation. Data-only edits export directly. Standalone Generate updates code only. “Generate first” above may be fulfilled by pre-export confirmation.

如果输入根目录与上次不同，或本次会删除旧表的生成文件，工具会显示新旧输入路径和待删文件清单。先确认没有误选 Data 的子目录；确实删除/重命名了表时点击“确认生成”，否则取消并改回完整输入目录。导出前提示使用“生成并导出”，单独生成使用“确认生成”。预览只读取项目；取消时生成代码、支持源码、版本记录和当前发布版本均保持不变。

Changed input root or old-table file deletion shows old/new roots and deletion list. Ensure you did not select a Data subfolder. Confirm intended removal/rename; otherwise cancel and restore the full root. Export prompts Generate and export; standalone generation prompts Confirm generation. Preview is read-only; cancellation preserves code, support, version records, and active release.

“只校验”不修改 Config 或所选 Output，也不自动生成代码；不需要 Output 可写。发现代码缺失或结构过期时显示“需要生成代码，完整校验尚未完成”，先查看生成预览。通过只校验不表示已经发布；需要交付数据时执行“校验并导出”。编译和回读验证使用系统临时目录，诊断默认留在会话，可主动导出报告。

Validate only writes neither Config nor selected Output, requires no writable Output, and never auto-generates. Missing/stale code reports incomplete full validation; inspect generation preview first. Validation success is not publication. Use Validate and export for delivery. Compilation/read-back uses system temp; session diagnostics can be explicitly exported.

任务显示当前阶段、表/工作簿、可知的进度和耗时，支持“取消任务”。生成代码与发布数据有独立提交点：已生成后再取消或导出失败，代码保留，当前数据版本不变；激活已经完成则显示成功。单次不能中断的解析或规则等待安全结束。具体边界见 [发布管理](release-and-acceptance.md#操作边界)。

Progress shows phase, table/workbook, known progress, elapsed time, and Cancel task. Code/data have independent commits: after generation, cancellation/export failure retains code but not a new active data version; completed activation reports success. Uninterruptible parsing/rules finish at safe boundaries. See [operation boundaries](release-and-acceptance.md#操作边界).

## 4. Excel 表头怎么写

**4. Excel headers**

### 4.1 统一使用五行表头

**4.1 Always use five header rows**

所有表都保留第 5 行“所属端”，每个字段必须明确声明 `C`、`S` 或 `CS`，留空会报错。**第 5 行不能省略；数据固定从第 6 行开始。** 详见[所属端规则](config-types.md#客户端与服务端字段)。

Every table retains target side on row 5; each field explicitly declares C/S/CS or errors. **Row 5 is mandatory; data always starts on row 6.** See [side rules](config-types.md#客户端与服务端字段).


第 1 行填写字段名，第 2 行填写类型，第 3 行标记主键，第 4 行填写说明，第 5 行保留所属端，**第 6 行开始填写数据**。

Rows 1–5 contain field names, types, key markers, descriptions, and target sides. **Data begins on row 6.**

下面是独立练习用的 `Item.xlsx`，不要直接混入正式示例目录：

This standalone Item.xlsx exercise must not be mixed into official samples:

| Excel 行号<br>Excel row | A 列<br>Column A | B 列<br>Column B | C 列<br>Column C |
| --- | --- | --- | --- |
| 1：字段名<br>1: Field | Id | Name | Count |
| 2：类型<br>2: Type | int | string | int |
| 3：主键<br>3: Key | 主键<br>`主键` (literal marker) |  |  |
| 4：说明<br>4: Description | 物品编号<br>Item identifier | 物品名称<br>Item name | 数量<br>Quantity |
| 5：所属端<br>5: Side | CS | CS | CS |
| 6：数据<br>6: Data | 1001 | 小药水<br>Small potion | 10 |
| 7：数据<br>7: Data | 1002 | 大药水<br>Large potion | 5 |

第 3 行至少有一列填写完整的“主键”二字，其他格子留空，不能混填说明、“必填”或“正数”。上例只有 Id 列标记主键，因此每条物品记录的 Id 必须填写且不能重复。

At least one row-3 cell must contain exactly 主键. Others stay blank, without descriptions/Required/Positive text. Only Id is marked here, so each item's Id is required and unique.

默认固定按五行表头检查；第 3 行全部漏填、拼错或混填，都会报告表头错误并阻止生成和导出，首次生成也一样。整行漏填定位到第 3 行，拼错或混填定位到具体单元格。工具不会自动补主键、去重或改写数据；失败时原生成代码和当前发布版本会保留。仅支持五行表头，不自动识别或回退旧四行格式。

Five-row validation is fixed. Missing, misspelled, or mixed row-3 markers block generation/export, including first generation. Entirely missing markers locate row 3; individual errors locate cells. The tool never invents keys, deduplicates, or rewrites data. Failure preserves generated code/current release. Old four-row format is neither detected nor used as fallback.

第 4 行只用于说明，写“必填”“正数”不会自动建立业务规则。主键负责标识记录和检查完整键唯一；数值必须大于 0、名称必填等业务约束由项目规则检查。

Row 4 is descriptive only; writing Required/Positive creates no rule. Keys identify records/enforce complete uniqueness; positive values/required names are project business rules.

### 4.2 多个字段组成主键

**4.2 Composite keys**

需要联合主键时，在第 3 行将参与主键的每一列都标为“主键”，表头行数和数据起始行不变：

Mark every participating column with 主键 on row 3; header count/data start remain unchanged:

| Excel 行号<br>Excel row | A 列<br>Column A | B 列<br>Column B | C 列<br>Column C |
| --- | --- | --- | --- |
| 1：字段名<br>1: Field | GroupId | Id | Count |
| 2：类型<br>2: Type | int | int | int |
| 3：主键<br>3: Key | 主键<br>`主键` (literal marker) | 主键<br>`主键` (literal marker) |  |
| 4：说明<br>4: Description | 组编号<br>Group identifier | 组内编号<br>Identifier within group | 数量<br>Quantity |
| 5：所属端<br>5: Side | CS | CS | CS |
| 6：数据<br>6: Data | 10 | 1001 | 1 |
| 7：数据<br>7: Data | 20 | 1001 | 2 |

这里完整键是 GroupId + Id；两行的组不同，所以允许 Id 相同。完整组合不能重复，主记录的每个键分量必须填写。可以直接使用业务字段组成键，不要求额外增加 int Id。

Complete key is GroupId + Id; different groups permit equal Id. Complete combinations must be unique and each main-record component filled. Business fields can form keys without extra int Id.

明确填写 0 可以是键值，是否允许取决于项目业务要求。主键支持的类型、非连续列及更多例子见 [Excel 主键说明](table-indexes.md#怎样填写)。已有表的主键不是随意改的分组备注，修改后必须重新生成并协调游戏代码。

Explicit 0 can be a key, subject to business rules. See [key types/nonadjacent columns/examples](table-indexes.md#怎样填写). Existing keys are not casual grouping notes; changes require regeneration/game coordination.

### 4.3 文件、Sheet 和表名

**4.3 Files, sheets, and table names**

- 输入递归读取所选根目录及其全部子目录中的 `.xlsx` 和 `.xls`；`.XLSX`、`.Xls` 等大小写写法也可用。`.csv`、`.xlsm`、`.xlsb` 不属于本次支持的输入格式。<br>
  Recursively reads .xlsx/.xls, case-insensitively, from selected root/subdirectories. .csv/.xlsm/.xlsb are unsupported here.
- `.xls` 支持实际的 Excel 二进制工作簿，不能把 CSV、HTML 或 `.xlsx` 只改扩展名当成 `.xls`。`.xls` 与 `.xlsx` 使用相同的表头、填写和校验规则，可以混在同一输入树中。<br>
  .xls must be real binary Excel, not renamed CSV/HTML/.xlsx. Both formats share headers/syntax/validation and may coexist.
- 每张参与导出的 Sheet 名就是表名；单表、多表使用同一规则，工作簿文件名和子目录名称不加入表名。<br>
  Each exported sheet name is its table name. Workbook/subdirectory names are not added, whether one or multiple sheets.
- 名称以 `#` 开头的 Sheet 跳过，例如 `#说明`；隐藏 Sheet 仍会导出。<br>
  Sheets starting #, e.g. #说明, are skipped. Hidden business sheets still export.
- 表名在所有层级、两种格式的输入工作簿中不能重复，不区分大小写。例如两个工作簿不能都含有名为 Item 的数据 Sheet。新增或删除其他 Sheet 不改变已有表名。<br>
  Table names must be unique case-insensitively across all folders/formats. Two workbooks cannot both contain Item. Other sheet additions/deletions do not rename existing tables.
- 表名、普通字段名使用 ASCII 字母、数字、下划线，首字符不能是数字。字段大小写要一致；`__zr` 开头保留给工具。<br>
  Table/ordinary field names use ASCII letters, digits, underscores, never a leading digit. Keep field case consistent; __zr prefix is reserved.
- 带点号、`[]`、`$type`、`$order` 的表头是项目预设的分列/列表布局，不要按普通字段随意改名。<br>
  Dotted, [], $type, and $order headers are predefined split/list layouts; do not rename as ordinary fields.
- Excel 的 `~$` 锁文件和 macOS 的 `._` 附属文件会被忽略。隐藏目录中的实际工作簿仍参与读取；备份必须放到输入根目录之外，不能藏在输入目录下的“备份”文件夹。<br>
  Excel ~$ locks and macOS ._ files are ignored. Actual hidden-folder workbooks still read. Backups belong outside the input root, not hidden in a backup subfolder.
- 目录符号链接、Windows 目录联接及链接文件会跳过，避免循环或重复读取。实际工作簿请放在输入目录树内；某个实际子目录不可访问时，本次扫描失败，需要处理权限后重试。<br>
  Symlinked directories, Windows junctions, and linked files are skipped to avoid cycles/duplicates. Keep actual workbooks in the input tree. Inaccessible real subdirectories fail scanning; fix permissions and retry.

例如可以按下面方式分类，只选择最外层 Data：

For example, classify like this and select outermost Data only:

```text
Data/
  道具/ / (Items category)
    Item.xlsx
  战斗/ / (Battle category)
    技能/ / (Skills category)
      Skill.xls
```

两张表一起生成、校验和导出，跨目录、跨格式引用也按全局表名查找。Windows 与 macOS 都支持含中文、空格的本机路径；切换电脑时重新选择该电脑上的实际目录。

Both tables generate/validate/export together; cross-folder/format references use global table names. Windows/macOS support local Chinese/spaced paths. Reselect actual local paths when switching computers.

### 4.4 保存、精度与排序

**4.4 Saving, precision, and sorting**

工具读取磁盘文件，不会读取 Excel 窗口里尚未保存的内容。修改后先保存，再导出。

The tool reads disk, not unsaved Excel-window contents. Save edits before export.

`.xls` 中的公式使用文件已保存的计算结果，工具不会重新计算公式。修改公式或其依赖后，在表格软件中完成计算并保存，再导出；需要稳定交付时可填最终值。数值读取采用统一的小数点约定，不随 Windows/macOS 的地区设置改变。

.xls formulas use saved cached results; the tool does not recalculate. Recalculate/save in spreadsheet software after formula/dependency changes, or enter final values for stable delivery. Numeric parsing uses one decimal convention independent of OS region.

超过 Excel 精度的 long、ulong 或高精度金额，先将单元格设为文本再输入。已经被 Excel 舍入的数字，工具无法恢复。小数使用英文 `.`，数组与 JSON 使用英文标点。

Set cells to text before entering long/ulong/high-precision money beyond Excel precision. Already rounded digits cannot be recovered. Use . for decimals and ASCII punctuation for arrays/JSON.

不要合并字段名或数据单元格；多行记录不能只对其中几列排序。移动或重建工作簿前保存并关闭；日常使用无需执行覆盖示例的重建脚本。

Never merge field-name/data cells or sort only selected columns of multi-row records. Save/close before moving/rebuilding. Everyday use requires no sample-overwrite rebuild scripts.

## 5. 数据怎么填写

**5. Entering data**

以下类型由表头决定。填写已有项目模板时使用提供的类型、枚举值和对象成员，不需要为这些字段编写代码。

Headers determine types. Use provided types/enums/members when filling templates; no field implementation is required.

| 类型/场景<br>Type/scenario | 填写例子<br>Example | 注意<br>Notes |
| --- | --- | --- |
| 整数<br>Integer | `1001`、`-5`<br>1001, -5 | 范围由具体整数类型与业务规则决定<br>Range depends on numeric type/business rules |
| 小数<br>Decimal number | `1.25`<br>1.25 | 英文小数点；不接受 NaN/Infinity<br>Dot separator; no NaN/Infinity |
| bool | `true`、`false`、`1`、`0`<br>true, false, 1, 0 | false 是明确填写的值<br>false is explicit input |
| string | `小药水`<br>Small potion | 普通文字保留首尾空格；纯空白按空处理<br>Ordinary text retains surrounding spaces; all whitespace is empty |
| 已提供的枚举<br>Provided enum | `Rare`<br>Rare | 使用项目声明的名称，不随意填数字或新名字<br>Declared names only, not arbitrary numbers/new names |
| vector2 / vector3 | `1,2` / `1,2,3`<br>1,2 / 1,2,3 | 分别为两个/三个分量<br>Two/three components |
| kv | `1001,10`<br>1001,10 | 两个整数；具体业务含义由项目规定<br>Two integers; project defines meaning |
| int[] | `[1;2;3]` | 普通数组用分号<br>Semicolon separator |
| int[][] | `[[1;2];[];[3]]` | 每层括号，子数组可不等长<br>Bracket every level; inner lengths may differ |
| int? | `123`、`null`、留空<br>123, null, blank | 0 仍是实际值<br>0 is a value |
| `Dictionary<string,int>` | `["hp"=100;"attack"=25]` | 解析后的键不能重复<br>Parsed keys must be unique |
| int[,] | `[[1;2];[3;4]]` | 矩形，各行长度一致<br>Rectangular; equal row lengths |
| `json<int[]>` | `[1,2,3]` | 标准 JSON 使用逗号<br>Standard JSON uses commas |
| 已提供的特殊类型<br>Provided special type | 例如金额 `1.25`<br>E.g. money 1.25 | 按项目说明填写；特殊格式由程序员准备<br>Follow project instructions; developers supply special formats |

完整类型范围、转义和集合组合见 [Excel 类型与填写语法](config-types.md)。

See [Excel types/syntax](config-types.md) for full coverage, escapes, and combinations.

### 5.1 空白与显式值

**5.1 Blank versus explicit values**

空白不等于 0、false 或 `[]`。工具保留是否填写的信息，必填规则会据此检查。普通非可空整数留空可能得到默认 0，但仍算未填写；可空值留空或写 null 表示无值。空数组可写 `[]`，不代表满足“至少一个元素”的业务要求。

Blank differs from 0, false, and []. The tool tracks filled status for Required. A blank non-nullable integer may decode as 0 but remains unfilled; nullable blank/null means absent. [] is empty, not satisfaction of an “at least one element” rule.

对象成员留空可能采用项目定义的默认值；标为必填的成员仍必须显式填写。不要仅凭导出后的值推断原来是否填过。

Blank object members may retain project defaults; required members still need explicit input. Output values alone cannot reveal whether input was filled.

普通数组中的字符串含分号、括号或引号时使用引号和转义，例如 `["a;b";"x,[]"]`。不是所有地方都使用 JSON：普通数组用分号，`json<T>` 用逗号，不能混用。

Quote/escape strings containing semicolons/brackets/quotes, e.g. `["a;b";"x,[]"]`. Not every format is JSON: ordinary arrays use semicolons, `json<T>` commas; do not mix them.

### 5.2 对象分列、多态与多行列表

**5.2 Split objects, polymorphism, multi-row lists**

已有模板中的 `Settings.MaxTargets` 表示某组设置的一个成员。第二行使用项目提供的整组类型，不要把每个成员列都改成 int/bool。

Settings.MaxTargets is a member of a settings group. Row 2 uses the project's whole-group type, not int/bool per member.

`Actions[].$type` 用于选择动作种类，例如 Damage、AddBuff；只填写所选种类适用的成员，其他种类专属列留空。`Actions[].$order` 是排列顺序，按项目模板填写唯一正整数。

Actions[].$type selects kinds such as Damage/AddBuff. Fill applicable members only, leaving other subclasses blank. Actions[].$order defines order using unique positive integers per template.

包含 `[]` 展开列时，多行可以合成一条配置：

Expanded [] columns allow several rows to form one record:

- 按第 3 行标记的完整主键归组；主记录先填写所有键分量，续行重复完整键或将所有键分量留空以延续上一条有效记录，不能只填部分键。<br>
  Group by the complete row-3 key. First fill all components; continuations repeat all or leave all blank to continue the preceding valid record. Partial keys are invalid.
- 主字段在首次出现的行确定，后续留空或逐字重复，冲突会报错。<br>
  Main fields come from the first occurrence; later values are blank or exact repeats. Conflicts fail.
- 子列表整组留空表示不增加元素。<br>
  Entirely blank child-list groups add no element.
- 嵌套列表各层按自己的 order 归组、排序；父级顺序编号要明确填写，不能靠空白猜归属。<br>
  Each nested level groups/sorts by its own order. Explicitly fill parent numbers; blanks never infer ownership.
- 工具报告的数组下标从 0 开始，指排序后的元素；同时会给出原始 Excel 行列，修改时按原始位置找格子。<br>
  Reported array indexes are zero-based after sorting, accompanied by original Excel coordinates; edit those original cells.

完整规则和表格例子见 [分列与多行填写](config-types.md#结构分列多态类型列与多行动作列表)。新增动作种类或成员由程序员扩展模板；配表人员只填写已支持的种类和参数。

See [split/multi-row rules](config-types.md#结构分列多态类型列与多行动作列表). Developers extend kinds/members; authors use supported kinds/parameters.

### 5.3 跨表对象引用

**5.3 Cross-table object references**

若模板的类型行写 `SkillConfig`，格子里填目标 Skill 表的 int Id，例如 10；不是填技能名称或一段对象 JSON。

A SkillConfig type cell expects a target Skill int Id such as 10, not its name or object JSON.

非零 Id 必须存在；被引用目标的 Id 必须非零且唯一。空白或 0 表示无引用，项目要求必填时会报错。负数不是空引用，会按实际 Id 查找。当前直接表引用限顶层单值字段。

Nonzero Ids must exist; target Ids must be nonzero/unique. Blank/0 means no reference and fails required rules. Negative IDs are actual lookups, not null. Direct references currently support top-level scalar fields only.

正式 [TableReferences.xlsx](../samples/Data/TableReferences.xlsx) 展示双向、自引用和共享目标。关联由工具处理，配表人员无需编写关联代码。

[TableReferences.xlsx](../samples/Data/TableReferences.xlsx) shows bidirectional/self/shared targets. The tool links them; authors need no linking code.

## 6. 怎样理解校验与后置检查

**6. Validation and post-validation**

“后置检查”指数据解析成配置记录之后的业务检查，**仍发生在发布之前**。例如：最大目标数必须大于 0、动作延时不能为负、启用某功能时另一字段必填、所选技能必须符合当前英雄的要求。

Post-validation checks business rules after parsing into records, **still before publication**: positive target count, nonnegative delay, conditionally required fields, or skills compatible with heroes.

执行这些检查时，多行数据已经合并为列表，直接跨表引用也已关联成对象，因此可以检查整条技能、整张商店表或关联道具的状态：

Lists are already merged and direct references linked, allowing whole-skill/shop/related-item checks:

| 范围<br>Scope | 实际能发现的问题<br>Problems detected |
| --- | --- |
| 字段检查<br>Field | 名称未填、数值越界、需要唯一的字段重复、外键不存在<br>Missing names, out-of-range values, duplicates where unique, nonexistent foreign keys |
| 逐行后置检查<br>Row | 最小值大于最大值、动作延时为负、Buff 层数无效<br>Minimum above maximum, negative delay, invalid buff stacks |
| 整表后置检查<br>Table | 同一道具、同一价格重复上架<br>Duplicate item/price offers |
| 跨表后置检查<br>Cross-table | 已上架商品引用停用道具；奖励数量超过道具堆叠上限<br>Active offers referencing disabled items; rewards above stack limits |

这些业务条件由项目规则决定。例如正式示例把负价格记为错误、把需要拆分堆叠的奖励记为警告。查看 [道具、商店与任务检查示例](../samples/README.md#后置检查与四种日志示例)。

Project rules define these conditions. Official samples treat negative prices as errors and split-stack rewards as warnings. See [item/shop/quest checks](../samples/README.md#后置检查与四种日志示例).

业务规则由程序员维护，点击“校验并导出”会自动执行，不需要额外点一次“后置检查”。普通使用只需根据结果修正数据。

Developers maintain rules; Validate and export runs them automatically without a separate post-check button. Authors fix data based on results.

遇到规则判断不符合预期时，程序员可以用 Visual Studio / Rider 对同一份规则设置断点，观察本次导入值、关联对象和判断分支，确认问题来自数据还是检查逻辑。具体操作见 [规则断点调试](debugging.md)；日常配表无需启动 IDE。

For unexpected logic, developers debug the same rules in Visual Studio/Rider, inspecting imported values, linked objects, and branches to distinguish data versus rule defects. See [debugging](debugging.md); everyday authors need no IDE.

| 结果<br>Result | 意味着什么<br>Meaning | 怎样处理<br>Action |
| --- | --- | --- |
| Info / 信息<br>Info | 业务提示或检查汇总，本身不阻止发布<br>Business notice/summary, nonblocking | 阅读状态、数量等说明<br>Read status/counts |
| Success / 成功诊断<br>Success diagnostic | 某一项检查通过，本身不阻止发布<br>One check passed, nonblocking | 整次是否成功仍看最终导出结果<br>Whole-run success still comes from final result |
| Error / 错误<br>Error | 本次不能发布<br>Cannot publish | 按工作簿、Sheet、行列和消息修正后重试<br>Fix workbook/sheet/cell/message and retry |
| Warning / 警告<br>Warning | 本身不阻止发布<br>Nonblocking itself | 确认是否为合理数据；存在其他 Error 时仍失败<br>Confirm intended data; other Errors still fail |
| 检查被跳过<br>Check skipped | 当前表或依赖表不完整，相关业务条件暂时无法判断<br>Incomplete current/dependency table prevents judgment | 先修原始解析/缺表/关联错误，再重新导出<br>Fix original parse/missing-table/reference errors first |
| 只校验成功且 0 错误<br>Validation-only success, 0 errors | 本轮数据检查通过，尚未发布<br>Current checks passed, unpublished | 需要交付时执行“校验并导出”<br>Validate and export for delivery |
| 导出成功且 0 错误<br>Export success, 0 errors | 本次已激活新输出版本<br>New output version activated | 继续核对表集合、行数和项目交付结果<br>Verify table set/counts/delivery |

针对字段报告的诊断带有工作簿、Sheet、行列、字段路径和代号。即使动作按 `$order` 排序，`Groups[1].Actions[0].Ratio` 这样的嵌套路径仍能对应到原始 Excel 单元格；按日志中的实际行列修改。运行日志与结构化诊断文件见 [导出结果](#7-成功后交付什么)。

Field diagnostics include workbook, sheet, coordinates, path, and code. Even sorted nested paths such as Groups[1].Actions[0].Ratio map to original cells. Follow actual logged coordinates. See [outputs/logs](#7-成功后交付什么).

规则目录留空仍做类型、结构、明确主键、直接表对象引用、编解码及发布完整性检查；它不会凭空知道项目的等级上限、必填条件或概率总和。

Blank Rules still checks types, structure, explicit keys, direct references, codecs, and publication integrity. It cannot infer project level caps, required conditions, or probability totals.

一次导出可能报告多个独立错误；标签已经报错时，后置检查也可能继续运行。先修明确的输入错误，再看剩余信息。“跳过检查”不是“检查通过”。规则提示本身有疑问时，把日志与期望条件交给程序员，编写方法见 [规则开发](rules-development.md)。

One export may report several independent errors; post-checks can continue after attribute errors. Fix clear inputs first, then remaining diagnostics. Skipped is not passed. Give developers logs/expected constraints for questionable rules; see [rule development](rules-development.md).

## 7. 成功后交付什么

**7. What to deliver after success**

下面以 Binary + JSON 为例；Binary 只输出 `.bytes` 表数据，JSON 只输出 `.json` 表数据：

This shows Binary + JSON. Binary alone emits .bytes tables; JSON alone emits .json tables:

```text
Output/
  current.json                 当前有效版本的入口清单 / Entry manifest for the current valid version
  versions/
    v某次运行编号/ / (Directory for one version/run identifier)
      AllTypes.bytes           二进制表数据（Binary / Binary + JSON） / Binary table data (Binary / Binary + JSON)
      AllTypes.json            JSON 表数据（JSON / Binary + JSON） / JSON table data (JSON / Binary + JSON)
      AllTypes.rows.json       逐字段比较快照，仅供工具管理 / Field-difference snapshot, tool management only
      manifest.json            该版本的独立清单 / Independent version manifest
      release.json             发布记录、指纹与变更摘要 / Release record, fingerprints, change summary
      schema.json              表结构摘要 / Table schema summary
      diagnostics.json         该候选版本的诊断 / Candidate diagnostics
      ...
  Logs/
    export.log                 本次运行日志 / Current run log
    diagnostics.json           最近一次结构化检查结果 / Latest structured checks
    某次运行编号/              独立运行日志、诊断与结果 / Per-run logs, diagnostics, and result
```

其他锁文件或编译缓存由工具维护，不属于游戏数据交付内容。不要手改 `.bytes` 或表数据 `.json`，修改数据回到 Excel。

Locks/caches are tool-managed, not game deliverables. Do not hand-edit .bytes/table .json; edit Excel.

交付 **current.json 和它引用的全部表数据文件**（`.bytes` / `.json`），保留版本相对路径；双格式清单中的 `file` 和 `jsonFile` 都要交付。管理清单、`.rows.json` 差异快照和诊断不属于游戏资源。不从不同版本挑选表文件拼在一起，也不手工修改清单里的文件信息。新数据全部写入并检查完成后，工具才切换当前清单；失败时旧的有效数据继续保留。

Deliver **current.json plus every referenced table data file**, preserving relative version paths. Dual-format requires both file/jsonFile targets. Management manifests, .rows.json, and diagnostics are not game resources. Never mix table files from versions or edit manifest file details. The tool switches only after complete new data is written/checked; failure retains old valid data.

固定日志是最近一次结果，每次导出另存独立运行目录。新版本有完整清单；通过“版本历史”查看差异、保留版本、预览回滚和清理。回滚必须提供与实际部署端一致的兼容清单，并通过端别、Schema、源码指纹和文件哈希检查。缺少元数据的旧版本不自动回滚。详见 [比较、回滚与清理](release-and-acceptance.md#比较回滚与清理)。

Fixed logs describe the latest run; each export also keeps an independent directory. New versions have full manifests. Version history offers comparisons/pins/rollback/cleanup previews. Rollback needs actual-deployment compatibility manifests passing side/schema/source-fingerprint/hash checks. Old versions lacking metadata cannot auto-rollback. See [management](release-and-acceptance.md#比较回滚与清理).

### 7.1 导出成功后，游戏为何还没变化

**7.1 Why the game has not changed after export**

导出只更新所选输出目录。游戏必须能取得这份数据，并执行项目提供的加载/刷新操作；正在运行的游戏不会自动读取任意目录里的新文件。

Export only updates selected Output. The game must receive data and invoke its project's load/refresh operation; running games do not automatically read arbitrary new files.

交付后通过项目提供的查看方式确认版本和实际字段值。远端部署先上传完整版本，再更新清单；不要在客户端仍可能读取时清理旧版本。资源部署、加载实现和代码兼容由程序员处理。

Verify version/values through project tools after delivery. Remote deployment uploads complete versions before manifests. Do not clean old versions while clients may read them. Developers handle deployment/loading/compatibility.

只改值通常只需要发布数据。增删表/字段、改变类型或主键，可能同时需要更新游戏代码，不能只把新的 bytes 交给旧游戏继续加载。

Value-only edits usually need data publication only. Table/field/type/key changes may require game-code updates; new bytes cannot simply be handed to old code.

## 8. 按错误找到修改位置

**8. Locate fixes from errors**

先看：**错误代号 → 文件/Sheet → Excel 行列 → 字段 → 消息**。摘要直接显示完整工作簿路径和 A1 位置，例如 `Data/TableReferences.xlsx → Hero!A6 → Id`；C# 源码位置显示为 `Rules/Broken.cs(4,20)`。Excel 行列从 1 开始，不是数组下标。C# 编译错误指向的是源码行号，并区分业务类型、生成代码、序列化支持/转换器和业务规则，附对应处理建议，需要程序员处理。

Read **code → file/sheet → Excel coordinates → field → message**. Summaries show full workbook/A1 locations, e.g. Data/TableReferences.xlsx → Hero!A6 → Id; C# shows Rules/Broken.cs(4,20). Excel coordinates are one-based, not array indexes. Compile locations are source lines and classify business types/generated/support-converters/rules with suggested fixes for developers.

| 提示/代号<br>Message/code | 先检查什么<br>First check | 需要谁处理<br>Owner |
| --- | --- | --- |
| INPUT_EMPTY / 没有工作簿<br>INPUT_EMPTY / no workbooks | 根目录及子目录是否有 .xlsx 或 .xls，是否已保存，是否误用链接目录<br>Saved .xlsx/.xls under root/subdirectories; no mistaken linked root | 配表人员<br>Author |
| INPUT_SCAN / 输入目录扫描失败<br>INPUT_SCAN | 诊断中的子目录是否存在、是否可访问，磁盘或网络共享是否正常<br>Subdirectory existence/access, disk/network share | 环境维护者；修复后重新导出<br>Environment maintainer, then retry |
| EXCEL_OPEN / 工作簿无法打开<br>EXCEL_OPEN | 文件是否损坏、加密或只是改了扩展名；按诊断中的完整路径检查<br>Corruption, encryption, renamed extension; inspect full path | 配表人员/文件提供者<br>Author/file provider |
| 表名重复<br>Duplicate table name | 文件名与有效 Sheet 是否产生同名表<br>Effective sheet names across files | 配表人员与结构维护者确认<br>Author/schema maintainer |
| 表头/主键错误（SCHEMA_KEY）<br>SCHEMA_KEY / header-key error | 按提示检查第 3 行的单元格，只填“主键”或留空；说明放第 4 行，数据从第 6 行开始<br>Row 3 only literal 主键 or blank; descriptions row 4, data row 6 | 配表人员；改变结构先确认<br>Author; confirm schema changes |
| CELL_PARSE | 数字、括号、分隔符、枚举值、JSON 是否符合类型<br>Numbers, brackets, delimiters, enum, JSON versus type | 配表人员<br>Author |
| REQUIRED | 指定字段或成员是否空白<br>Blank field/member | 配表人员<br>Author |
| RANGE / POSITIVE | 是否超出允许范围或填了非正数<br>Range/nonpositive value | 配表人员<br>Author |
| KEY_DUPLICATE / 主键重复<br>KEY_DUPLICATE | 重复记录或完整键是否冲突<br>Duplicate record/complete key | 配表人员<br>Author |
| REFERENCE_MISSING / TABLE_REFERENCE | 目标 Id 是否存在，目标表是否重复/缺失<br>Target Id existence, duplicate/missing target table | 配表人员<br>Author |
| BUSINESS_CHECK_SKIPPED / REF_CHECK_SKIPPED | 前面是否有缺表、解析或关联错误<br>Earlier missing-table/parse/link errors | 先修原始数据问题<br>Fix original data first |
| GENERATION_CONFIRM_REQUIRED / 生成范围变化<br>GENERATION_CONFIRM_REQUIRED | 核对完整输入根目录和待删除文件；误选子目录时取消<br>Full input root/deletions; cancel mistaken subfolder | 配表人员/项目维护者<br>Author/maintainer |
| SUPPORT_UPDATE_REQUIRED | 先生成配表代码，自动升级未修改的支持文件，再导出<br>Generate to upgrade unchanged support, then export | 配表人员；失败交程序员<br>Author; developer if failure |
| SUPPORT_MERGE_REQUIRED | 保留的支持文件有手写修改或属于未知旧版，按文件提示对比合并<br>Handwritten/unknown support requires per-file merge | 程序员<br>Developer |
| CODE_ROOT_GENERATED | 第三个输入框是否误选 Generated 子目录<br>Third field mistakenly selects Generated | 改为项目 Config 根目录<br>Select Config root |
| SCHEMA_MISMATCH / CODEC_MISSING | 当前 Excel 与代码是否对应，结构变化后是否生成<br>Excel/code match and regeneration after changes | 确认目录后重新生成，失败交程序员<br>Confirm paths/regenerate; developer if failure |
| 需要生成代码，完整校验尚未完成<br>Generation required; full validation incomplete | 只校验不会自动修复过期或缺失的生成代码<br>Validation-only never repairs missing/stale code | 核对生成预览，生成后重新校验<br>Review preview, generate, revalidate |
| 输入在运行期间发生变化<br>Inputs changed during operation | Excel、Config、Rules 或 Tables.json 是否在本次运行中改动<br>Excel/Config/Rules/Tables.json edits | 保存并等待文件稳定后重试，以本轮最终结果为准<br>Save/wait for stable files/retry; use final current result |
| 类型不存在、CS 开头错误<br>Missing type / CS errors | 是否选错代码目录，项目代码是否完整可编译<br>Correct code path and complete compilable source | 程序员<br>Developer |
| RULE_SOURCE_MISSING / RULE_SOURCE_EMPTY | 规则目录是否选错或项目未提供规则<br>Correct rules path/project rules available | 确认路径，交规则维护者；不要随意关闭规则<br>Confirm/contact maintainer; do not casually disable |
| RULE_BINDING / VALIDATOR_FIELD / VALIDATOR_LOCATION | 规则绑定或诊断代码有误<br>Rule binding/diagnostic code defect | 程序员<br>Developer |
| RULE_EXCEPTION / VALIDATOR_EXCEPTION | 规则执行异常<br>Rule execution exception | 保存完整日志交程序员<br>Save full log for developer |
| 文件占用、无权限、输出锁<br>File busy/permissions/output lock | 是否有另一个导出正在运行，输出是否可写<br>Another export/writable output | 关闭重复操作或联系环境维护者<br>Stop duplicate operation or contact maintainer |
| 游戏找不到清单、Hash/Schema 不符<br>Game missing manifest / Hash/Schema mismatch | 是否交付完整版本、游戏代码是否匹配<br>Complete delivery/matching game code | 程序员/发布负责人<br>Developer/release owner |

项目自定义的错误代号按消息处理；拿不准时提供具体单元格与业务期望，不要通过改类型、删行或关规则来消除提示。

Follow messages for custom codes. If uncertain, provide the cell and business expectation; do not change types/delete rows/disable rules just to suppress errors.

### 8.1 需要保存哪些排错信息

**8.1 Preserve troubleshooting evidence**

通过“导出诊断报告”保存本次结果，或保留 `Output/Logs/<运行编号>/`，附工具版本、四个实际目录、操作步骤、期望与实际结果，以及必要的脱敏工作簿。

Export this run's diagnostic report or retain Output/Logs/&lt;run-id&gt;, with tool version, four actual paths, steps, expected/actual results, and necessary sanitized workbooks.

生成操作、导出前结构检查失败或取消生成都不会更新导出报告。输出目录权限/锁等早期失败也可能来不及写报告，先看界面当次异常与日志时间，避免把旧的成功报告当作这次结果。IDE 断点、异常栈和程序诊断见 [调试指南](debugging.md)。

Generation, pre-export schema failures, and cancelled generation do not update export reports. Early permission/lock errors may also precede report writing. Check current UI exceptions/timestamps, not old success reports. See [debugging](debugging.md) for IDEs/stacks/diagnostics.

## 9. 日常修改怎样操作

**9. Routine edits**

### 9.1 只改数据

**9.1 Data only**

保存 Excel → 确认四个目录 → 校验并导出 → 修正 Error → 阅读 Warning → 交付完整版本 → 在项目中确认加载效果。

Save Excel → confirm paths → validate/export → fix Errors → read Warnings → deliver complete version → verify actual project loading.

### 9.2 改表结构、类型或主键

**9.2 Schema, type, or key changes**

先与程序员确认字段、类型、关联关系和游戏兼容要求，再修改表头。保存后点击校验导出，按清单确认生成，确认项目配套规则已同步；按项目安排一起更新游戏代码和数据。

Confirm fields/types/references/game compatibility with developers before editing headers. Save, validate/export, confirm generation preview, and synchronize rules. Update game code/data together as required.

不需要为了新增一个普通数值列去手写整张表的模型。已有模板缺少所需动作类型或特殊格式时，由程序员准备扩展，见 [项目开发教程](project-development.md)。

Adding an ordinary numeric column does not require hand-writing the whole model. Developers extend missing action kinds/special formats; see [project development](project-development.md).

### 9.3 新增或删除表

**9.3 Add/remove tables**

先确定表名唯一以及哪些表引用它。仓库正式表只放 samples/Data，仍使用同一份 Config、Rules、Output。增删后对整套输入重新生成；删除表还需要程序员处理引用、规则和游戏读取代码。

Ensure globally unique names and identify references first. Official tables stay under samples/Data with shared Config/Rules/Output. Regenerate the entire input after additions/deletions; developers update references/rules/game reads for deletions.

代码根目录属于一套完整配置，应按整套数据操作。误选子目录时，生成器会因输入绑定变化或待删文件而停止并要求核对；不要跳过清单确认。

A code root represents a complete configuration set. Wrong subfolder selection stops generation for changed binding/deletions and requires review. Never bypass that preview.

### 9.4 试验与清理

**9.4 Experiments and cleanup**

使用仓库正式样例不需要重建工作簿。做故意改错练习时，在仓库外复制独立项目，或者使用系统临时目录；不要向正式 Data 添加备份和教学文件。

Included samples need no rebuild. For deliberate-error exercises copy a separate project outside the repository or into system temp. No backups/tutorial files in official Data.

不要随意删除 Config、Rules、current.json 或有效版本。空间清理与旧版本保留由项目维护者按 [清理说明](development.md#打包与安装) 处理；开发清理脚本不属于日常导出步骤。

Do not casually delete Config, Rules, current.json, or valid versions. Maintainers handle storage/history via [cleanup](development.md#打包与安装); development cleanup scripts are not daily export steps.

## 10. 每次修改后的验收清单

**10. Acceptance after every edit**

- Excel 已保存，四个路径匹配本项目。<br>
  Excel saved; all four paths match this project.
- 只改数据可直接导出；结构变化已生成，且程序员确认配套代码与规则同步。<br>
  Data-only exports directly; schema changes regenerated and developer-confirmed code/rules synchronized.
- 本次明确成功，Error 为 0，Warning 已阅读并确认。<br>
  Current run explicitly succeeded, 0 Errors, Warnings read/confirmed.
- 表集合和逻辑记录数符合预期；多行数组的一组不是多条配置。<br>
  Expected table set/logical counts; one multi-row group is one record.
- 交付本次 current.json 与其完整版本文件；没有混用旧表文件。<br>
  Deliver this current.json and complete version files without old-table mixing.
- 项目侧实际加载到本次版本，关键字段与修改一致。<br>
  Project actually loaded this version; key values match edits.
- 本次失败时没有把旧输出误认为新结果；排错需要的日志已保存。<br>
  Failure was not mistaken for old-output success; troubleshooting logs saved.

新业务规则还需由程序员在独立数据副本上验证合法、错误和修复场景；新增游戏接入需验证加载失败保留旧版本。具体接入见 [项目开发教程](project-development.md)。

Developers test new rules with valid/bad/fixed temporary copies and verify old-version retention on loading failures for new integrations. See [project development](project-development.md).

## 11. 继续阅读

**11. Further reading**

- [示例总入口](../samples/README.md)：七份正式工作簿的用途与统一目录。<br>
  [Samples](../samples/README.md): The seven official workbooks and their shared directories.
- [Excel 类型与填写语法](config-types.md)：数组、对象、字典、多态和多行列表的详细填法。<br>
  [Excel syntax](config-types.md): Arrays, objects, dictionaries, polymorphism, multi-row lists.
- [Excel 主键说明](table-indexes.md#怎样填写)：单键与联合键的标记。<br>
  [Excel keys](table-indexes.md#怎样填写): Single/composite markers.
- [项目开发教程](project-development.md)：程序员编写业务类型、转换器和规则，接入 Unity。<br>
  [Project development](project-development.md): Types, converters, rules, Unity integration.
- [源码构建与升级](development.md)、[调试指南](debugging.md)：源码构建与 Rider / Visual Studio / Unity 调试。<br>
  [Building/upgrading](development.md) and [debugging](debugging.md): Source builds and Rider/Visual Studio/Unity debugging.
- [使用范围](development.md#使用范围)：版本入口和运行环境。<br>
  [Scope](development.md#使用范围): Version entry points and environments.
