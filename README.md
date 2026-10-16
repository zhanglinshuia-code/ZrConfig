# ZrConfig

**中文 / English**：全部使用与开发文档均在同一页面逐节提供中英文，入口见 [文档导航 / Documentation](docs/README.md)。

All user and developer documentation includes Chinese and English together, section by section. Start with the [documentation index](docs/README.md).

面向 Unity 与游戏服务器的强类型 Excel 配置工具：读取工作簿、生成 C#、C++、Go、Java、Lua、TypeScript、Rust 表代码，执行校验并导出二进制或 JSON 版本数据。运行时支持配置快照、数据更新、主键/分组查询和跨表对象引用。

A strongly typed Excel configuration tool for Unity and game servers. It reads workbooks, generates table code for C#, C++, Go, Java, Lua, TypeScript, and Rust, validates data, and exports versioned Binary or JSON data. The runtime supports snapshots, data updates, primary-key and group queries, and cross-table object references.

采用 [MIT 许可证](LICENSE)。仓库包含工具源码、构建与打包入口、使用文档、Excel 示例及可运行的 Unity 工程。

Licensed under [MIT](LICENSE). This repository includes tool source, build and packaging entry points, documentation, Excel samples, and a runnable Unity project.

当前工程版本为 **1.4.0**。源码在桌面配表、发布管理和 Unity 异步资源读取的基础上，已加入七语言生成、JSON 导出与独立的 `server-v1` 服务器接口。接入见 [多语言生成与运行时](docs/server-generation.md)；已有候选包与各轮源码验证的覆盖范围分别见 [验收记录](docs/release-and-acceptance.md#验收与团队试用记录)。

The current project version is **1.4.0**. In addition to desktop authoring, release management, and asynchronous Unity resource loading, the source now includes seven-language generation, JSON export, and a separate `server-v1` server API. See [generation and runtimes](docs/server-generation.md) for integration and [acceptance records](docs/release-and-acceptance.md#验收与团队试用记录) for the coverage of candidate packages and successive source validations.

**旧版升级**：本次源码更新不保证与上一 GitHub 版本的生成接口兼容；Excel 统一使用五行表头，升级时需同步核对工作簿、生成代码、运行库与数据。已知 API 基线检查结果见 [本轮验收记录](docs/release-and-acceptance.md#文档与公开源码复验2026-10-16)。

**Upgrading older versions:** This source update does not guarantee compatibility with the generated APIs of the previous GitHub version. All workbooks use five header rows; check workbooks, generated code, runtimes, and data together when upgrading. Known API baseline results are in the [current acceptance record](docs/release-and-acceptance.md#文档与公开源码复验2026-10-16).

![ZrConfig 1.4.0：多语言选项、只校验与问题列表](docs/assets/zrconfig-app.png)

ZrConfig 1.4.0: language selection, validation-only mode, and diagnostics.

图中为 1.4.0 的 macOS 中文界面，展示生成/导出端、数据格式、接口家族及七种生成语言选项。示例上次只校验通过，0 错误、4 条预期业务警告，尚未发布数据；当前状态提示输入或设置已变化，上次结果已过期。问题列表支持筛选、搜索、定位与复制，截图中的本机路径仅作演示。

The screenshot shows the Chinese macOS UI in 1.4.0, including target side, data format, API family, and seven output languages. The sample's last validation succeeded with 0 errors and 4 expected business warnings; no data has been published. The status indicates that inputs or settings have since changed, so that result is stale. Diagnostics support filtering, search, navigation, and copying. Local paths in the screenshot are illustrative.

## 核心亮点

**Key features**

- **可编程后置检查**：用 C# 编写逐行、整表和跨表业务规则，把原本运行时才发现的配表错误提前到导出阶段。<br>
  **Programmable post-validation:** Write C# row, table, and cross-table business rules to catch configuration errors during export, before runtime.
- **日常修表与发布管理**：只校验、可操作诊断、实时进度与取消、结构预览、版本差异及兼容回滚，见 [使用与验收](docs/release-and-acceptance.md)。<br>
  **Editing and release management:** Validation-only mode, actionable diagnostics, live progress and cancellation, schema previews, version differences, and compatible rollback. See [usage and acceptance](docs/release-and-acceptance.md).
- **支持断点调试**：后置检查可在 Visual Studio / Rider 中设置断点、单步调试。<br>
  **Breakpoint debugging:** Set breakpoints and step through post-validation rules in Visual Studio or Rider.
- **数据结构丰富**：支持数组、字典、自定义对象、多态、嵌套多行列表、组合主键和跨表引用。<br>
  **Rich data structures:** Arrays, dictionaries, custom objects, polymorphism, nested multi-row lists, composite primary keys, and cross-table references.
- **多语言与双格式**：七种语言共用解析、校验和发布流程，支持 Binary、JSON 和双格式；C / S 字段筛选与生成语言独立。<br>
  **Multiple languages and formats:** Seven languages share parsing, validation, and publishing. Export Binary, JSON, or both; C/S field filtering is independent of the generated language.
- **服务器快照与接口保护**：提供全量、选择依赖闭包和按行加载，保留旧快照；公开 API 变化须通过显式迁移验收。<br>
  **Server snapshots and API protection:** Full, selected dependency-closure, and row loading retain old snapshots. Public API changes require explicit migration acceptance.
- **一套流程，两种入口**：桌面 App 与 CLI 共用项目配置、生成、校验和导出逻辑。<br>
  **One workflow, two entry points:** The desktop App and CLI share project settings, generation, validation, and export logic.
- **按需加载与版本切换**：同步/异步读取、按行加载、引用缓存，支持关键数据预热和全量验证后切换。<br>
  **On-demand loading and version switching:** Synchronous/asynchronous reads, per-row loading, and reference caching, with critical-data warmup and switching after full validation.

## 环境要求

**Requirements**

| 用途<br>Purpose | 需要准备<br>Requirements |
| --- | --- |
| 使用已打包 App / CLI<br>Run packaged App / CLI | 对应架构的 Windows 或 macOS 自包含包；无需另装 .NET<br>A self-contained Windows or macOS package for your architecture; no separate .NET installation |
| 从源码构建工具<br>Build from source | .NET SDK 10，具体选择由 [global.json](global.json) 控制；首次还原需联网<br>.NET SDK 10, selected by [global.json](global.json); initial restore requires internet access |
| 运行 Unity 示例<br>Run the Unity sample | Unity **6000.3.26f1**，版本以 [ProjectVersion.txt](unity/ZrConfig.Sample/ProjectSettings/ProjectVersion.txt) 为准<br>Unity **6000.3.26f1**; [ProjectVersion.txt](unity/ZrConfig.Sample/ProjectSettings/ProjectVersion.txt) is authoritative |
| 接入 C# / Unity 运行时源码<br>Integrate C# / Unity runtime source | .NET Standard 2.1、C# 9；Unity 直接编译 Assets 下的源码<br>.NET Standard 2.1 and C# 9; Unity compiles the source under Assets directly |
| 接入其他语言运行时<br>Integrate other runtimes | Java 17、Go 1.19、C++17、Lua 5.4、TypeScript 5.8 / Node 22、Rust 1.70 起；详见 [生成物与依赖](docs/server-generation.md#生成物与依赖)<br>Java 17, Go 1.19, C++17, Lua 5.4, TypeScript 5.8 / Node 22, or Rust 1.70 and later; see [outputs and dependencies](docs/server-generation.md#生成物与依赖) |

工具读取 `.xlsx`、`.xls` 不依赖 Microsoft Excel；编辑工作簿仍需表格软件。源码下载不包含安装包、已导出数据或 Unity Library，按下面的步骤准备即可。发布包由维护者构建，命令和产物位置见 [打包与安装](docs/development.md#打包与安装)。

Reading `.xlsx` and `.xls` does not require Microsoft Excel; editing workbooks still requires spreadsheet software. Source downloads omit installers, exported data, and Unity Library. Prepare them using the steps below. Maintainers build release packages; see [packaging and installation](docs/development.md#打包与安装) for commands and output locations.

## 运行 Unity 示例

**Run the Unity sample**

克隆或解压完整仓库，安装 .NET SDK 10，在仓库根目录执行：

Clone or extract the complete repository, install .NET SDK 10, and run this from the repository root:

```bash
bash scripts/maintain.sh unity-sample
```

Windows：

```powershell
powershell -File scripts/maintain.ps1 unity-sample
```

完成后，用 Unity Hub 打开 `unity/ZrConfig.Sample`，使用工程记录的 Unity 版本，打开 `Assets/ZrConfigSample.unity` 并 Play。准备命令会生成代码并导出数据，Unity 直接编译 Assets 中的框架和业务源码；首次运行需要联网还原依赖。完整步骤见 [Unity 示例](samples/README.md#unity-可运行示例)。

Then open `unity/ZrConfig.Sample` in Unity Hub using the recorded Unity version, open `Assets/ZrConfigSample.unity`, and press Play. Preparation generates code and exports data; Unity compiles framework and business source directly from Assets. The first run needs internet access to restore dependencies. See the [Unity sample](samples/README.md#unity-可运行示例) for full instructions.

成功时得到九张表、10,024 条记录、0 错误和 4 条预期警告。Editor 读取 `samples/Output`，桌面 Player 读取准备命令部署的 `Assets/StreamingAssets/ZrConfig`。如果只想体验配表和导出，可直接启动下方 App，无需先打开 Unity。

A successful run produces nine tables, 10,024 records, 0 errors, and 4 expected warnings. The Editor reads `samples/Output`; desktop Players read `Assets/StreamingAssets/ZrConfig`, deployed by the preparation command. To try authoring and export alone, launch the App below without opening Unity first.

## 启动配表工具

**Launch the authoring tool**

macOS：

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.App -p:NuGetAudit=false -p:UsedAvaloniaProducts=
```

Windows：

```powershell
dotnet run --project src/ZrConfig.App -p:NuGetAudit=false -p:UsedAvaloniaProducts=
```

在 App 打开 `samples/project.zrconfig.json`，保存 Excel 后点击“只校验”检查问题，准备发布时点击“校验并导出”。首次或表结构变化时，根据提示确认生成清单。共用示例已选择七语言、Binary + JSON 和 C 端。修正报错后，交付 `current.json` 与它引用的全部 `.bytes` / `.json` 数据，保留版本相对路径；诊断和差异快照留在工具侧。安装包构建见 [源码构建与升级](docs/development.md)。

Open `samples/project.zrconfig.json` in the App. Save Excel, click “只校验” (Validate only) to inspect problems, then “校验并导出” (Validate and export) when ready to publish. Confirm the generation preview on first use or after schema changes. The shared sample selects all seven languages, Binary + JSON, and client target C. After fixing errors, deliver `current.json` and every `.bytes` / `.json` file it references, preserving relative version paths. Keep diagnostics and difference snapshots on the tool side. See [building and upgrading](docs/development.md) for installers.

## 命令行生成与导出

**Generate and export from the command line**

在仓库根目录运行，一步完成必要的生成、业务校验与导出：

From the repository root, run generation, business validation, and export in one command:

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export --generate --project samples/project.zrconfig.json
```

Windows：

```powershell
dotnet run --project src/ZrConfig.Cli -p:NuGetAudit=false -- export --generate --project samples/project.zrconfig.json
```

项目配置保存相对路径，可以随完整项目一起移动。退出码 0 表示成功，1 表示生成或导出检查未通过，2 表示命令用法或执行异常，3 表示只校验需要先生成代码，130 表示取消。普通 `export` 只使用已有代码，`codegen` 只生成代码；参数、分端和规则调试见 [CLI 教程](docs/project-development.md#13-命令行使用)。

Project settings store relative paths and can move with the complete project. Exit codes: 0 success; 1 generation/export checks failed; 2 invalid usage or execution exception; 3 validation requires code generation first; 130 cancellation. Plain `export` uses existing code; `codegen` only generates code. See the [CLI tutorial](docs/project-development.md#13-命令行使用) for options, target sides, and rule debugging.

仅检查数据时，将上述 `export --generate` 换成 `validate`；它不生成代码、不发布数据。`release --help` 查看历史、差异、回滚和清理命令。克隆后可执行 `bash scripts/maintain.sh verify-source`，Windows 使用 `powershell -File scripts/maintain.ps1 verify-source`，在临时目录复验构建、生成、校验、导出及失败保护。

To check data only, replace `export --generate` above with `validate`; it neither generates code nor publishes data. Run `release --help` for history, differences, rollback, and cleanup. After cloning, run `bash scripts/maintain.sh verify-source`, or `powershell -File scripts/maintain.ps1 verify-source` on Windows, to recheck building, generation, validation, export, and failure protection in a temporary directory.

## 文档

**Documentation**

| 需要做什么<br>Task | 入口<br>Entry point |
| --- | --- |
| 安装、配表、生成、导出和处理报错<br>Install, author, generate, export, and troubleshoot | [使用文档](docs/user-guide.md)<br>[User guide](docs/user-guide.md) |
| 版本比较、兼容回滚、清理和平台验收<br>Compare versions, roll back compatible releases, clean up, and verify platforms | [发布管理与验收](docs/release-and-acceptance.md)<br>[Release management and acceptance](docs/release-and-acceptance.md) |
| 查询 Excel 类型与填写格式<br>Look up Excel types and cell formats | [Excel 填写语法](docs/config-types.md)<br>[Excel syntax](docs/config-types.md) |
| 运行示例、查看现成工作簿<br>Run samples and explore workbooks | [示例说明](samples/README.md)<br>[Samples](samples/README.md) |
| 选择生成语言、JSON 数据与服务器加载模式<br>Choose output languages, JSON data, and server loading modes | [多语言生成与运行时](docs/server-generation.md)<br>[Generation and runtimes](docs/server-generation.md) |
| 复验七语言契约、冻结消费方与性能<br>Recheck seven-language contracts, frozen consumers, and performance | [测试与验收](tests/README.md#公开服务器验收)<br>[Testing and acceptance](tests/README.md#公开服务器验收) |
| 定义业务类型、转换器，使用 CLI，接入 Unity<br>Define business types and converters, use the CLI, and integrate Unity | [项目开发教程](docs/project-development.md)<br>[Project development tutorial](docs/project-development.md) |
| 编写字段、整表和跨表检查<br>Write field, table, and cross-table checks | [规则开发](docs/rules-development.md)<br>[Rule development](docs/rules-development.md) |
| 调试自己的规则与 Unity 接入<br>Debug rules and Unity integration | [调试指南](docs/debugging.md)<br>[Debugging](docs/debugging.md) |
| 构建、打包和升级支持源码<br>Build, package, and upgrade support source | [源码构建与升级](docs/development.md)<br>[Building and upgrading](docs/development.md) |
| 修改源码、复验、准备 GitHub 提交<br>Modify source, validate, and prepare GitHub submissions | [提交与发布检查](docs/development.md#提交与发布检查)<br>[Contribution and release checks](docs/development.md#提交与发布检查) |

全部专题见 [文档导航](docs/README.md)。

See the [documentation index](docs/README.md) for all topics.

## 示例内容

**Sample content**

工作簿统一位于 [samples/Data](samples/Data)，配置与规则直接放在 Unity Assets 中，全部示例共用 `samples/Output`。

All workbooks live in [samples/Data](samples/Data). Configuration and rules are directly under Unity Assets, and every sample shares `samples/Output`.

| 工作簿<br>Workbook | 内容<br>Content |
| --- | --- |
| AllTypes.xlsx | 基础值、复杂类型、数组、字典、多态与嵌套多行列表<br>Primitive and complex types, arrays, dictionaries, polymorphism, and nested multi-row lists |
| AllTypes10k.xlsx | 一万条全类型记录、跨表引用和同表引用链<br>10,000 records covering all types, cross-table references, and same-table reference chains |
| CompositeKeys.xlsx | 双主键、三主键及分组<br>Two- and three-component primary keys and groups |
| TableReferences.xlsx | 双向/自引用、共享对象和空引用<br>Bidirectional/self references, shared objects, and null references |
| ItemCatalog.xlsx、ShopOffers.xlsx、QuestRewards.xlsx | 道具、商店、任务奖励和业务检查<br>Items, shops, quest rewards, and business checks |

未修改的示例导出九张表、10024 条记录（包含一万行全类型加载用例），产生 4 条预期警告。新建表采用五行表头：字段名、类型、主键、说明、所属端；第 6 行起填写数据。全部正式工作簿均使用当前表头规范，AllTypes 的类型索引、说明页与数据行同步更新。

Unmodified samples export nine tables and 10,024 records, including the 10,000-row loading case, with 4 expected warnings. New tables use five header rows: field, type, primary key, description, and target side; data begins on row 6. All official workbooks follow this convention, including AllTypes index links, instructions, and data rows.

## 仓库结构

**Repository layout**

| 位置<br>Location | 职责<br>Responsibility |
| --- | --- |
| `src` | Core、CLI、桌面 App，以及链接 Assets 源码的 Runtime / Validation 工程<br>Core, CLI, desktop App, and Runtime / Validation projects that link Assets source |
| `samples/Data` | 七份正式工作簿，所有示例共用同一输入根目录<br>Seven official workbooks under one shared input root |
| `unity/ZrConfig.Sample/Assets/ZrConfig` | C# Runtime、Validation、Unity 适配器与六种原生运行库模板的唯一源码<br>Sole source of C# Runtime, Validation, Unity adapters, and six native runtime templates |
| `unity/ZrConfig.Sample/Assets/HotUpdate/Config` | 手写类型、序列化支持和统一生成代码<br>Handwritten types, serialization support, and shared generated code |
| `unity/ZrConfig.Sample/Assets/Editor/ConfigRules` | 逐行、整表和跨表业务检查<br>Row, table, and cross-table business checks |
| `scripts` | 跨平台维护、示例准备和打包入口<br>Cross-platform maintenance, sample preparation, and packaging entry points |
| `tests` | 公开服务器验收工程和四份共享契约夹具<br>Public server acceptance project and four shared contract fixtures |
| `.github/workflows/ci.yml` | Windows/macOS 源码验证，以及 Windows/Linux/macOS 七语言最低/当前工具链矩阵<br>Windows/macOS source verification and Windows/Linux/macOS matrices for minimum/current toolchains in seven languages |
| `docs` | 使用与开发文档，截图和练习附件在 `docs/assets`<br>User and developer documentation; screenshots and tutorial attachments in `docs/assets` |
| `samples/Output`、`artifacts` | 执行导出和打包后生成，不随 Git 提交<br>Created by export and packaging; excluded from Git |

公开源码的目录职责、生成文件归属及验证入口见 [公开源码与目录](docs/development.md#公开源码与目录)。

See [public source and layout](docs/development.md#公开源码与目录) for directory responsibilities, generated-file ownership, and verification entry points.

## 数据与运行时

**Data and runtimes**

App 与 CLI 支持 Windows/macOS，读取 `.xlsx` 和 `.xls`，无需安装 Excel。生成代码后，项目使用对应语言的独立运行库加载二进制或 JSON 配置；工具界面与 Excel 解析库不进入游戏运行时。

The App and CLI run on Windows/macOS and read `.xlsx` and `.xls` without Excel. After generation, projects load Binary or JSON configuration through the standalone runtime for their language. The tool UI and Excel parsing libraries are not part of the game runtime.

导出先生成完整数据版本，再切换清单。Unity 现有 C# 接口默认只读取清单，生成查询直接提供各表 Codec，首次访问时校验结构，并按主键同步/异步加载记录，引用属性访问时才加载目标行，失败不发布半成品。需要全量验证后切换时提供 `TableCodecs.Registry` 并显式使用 ReloadAll / ReloadAllAsync。远端部署也应先上传完整版本数据再更新清单。

Export writes a complete data version before switching the manifest. The existing Unity C# API reads only the manifest by default. Generated queries provide each table's codec, validate its schema on first access, and load rows synchronously/asynchronously by primary key. Reference targets load when their properties are accessed; failures never publish partial results. For full validation before switching, provide `TableCodecs.Registry` and explicitly use ReloadAll / ReloadAllAsync. Remote deployments should also upload complete version data before updating the manifest.

同步入口调用资源源的 `Read`，异步入口调用 `ReadAsync`；AB 适配层负责资源请求的合并、调度和句柄，配表负责按行解码和对象缓存。Unity 默认策略下，成功重载后旧快照查询与旧行引用失效，业务统一获取新对象。独立 `server-v1` 接口保留业务已持有的快照和安全句柄，支持显式 Prepare / Commit；两套接口的生命周期见 [服务器接入](docs/server-generation.md#服务器契约家族与显式加载)。

Synchronous entry points call the source's `Read`; asynchronous ones call `ReadAsync`. The AssetBundle adapter manages resource request coalescing, scheduling, and handles; the configuration layer handles row decoding and object caches. Under Unity's default policy, successful reload invalidates old snapshot queries and old row references, so business code must acquire new objects. The separate `server-v1` API retains held snapshots and safe handles and supports explicit Prepare / Commit. See [server integration](docs/server-generation.md#服务器契约家族与显式加载) for both lifecycles.

模型结构变化需要同步生成代码、Runtime 与数据。Unity 示例的 Runtime、Validation、适配器、查看器和业务模型全部位于 Assets，以源码编译；具体接入见 [Unity 教程](docs/project-development.md#14-unity-运行时如何使用)。

Model changes require matching generated code, Runtime, and data. In the Unity sample, Runtime, Validation, adapters, viewers, and business models all compile as source under Assets. See the [Unity tutorial](docs/project-development.md#14-unity-运行时如何使用).

当前提供配置数据更新，不包含 C# 热更新框架、旧结构自动迁移、增量导出或 Excel 写回。Android / iOS / WebGL 可使用内置 `StreamingAssetsConfigSource`；具体设备、IL2CPP、AB / 热更新框架仍需按目标项目验证，接入边界见 [使用范围](docs/development.md#使用范围)。

Configuration data updates are supported. A C# hot-update framework, automatic migration of old schemas, incremental export, and Excel write-back are not included. Android / iOS / WebGL can use the built-in `StreamingAssetsConfigSource`. Verify actual devices, IL2CPP, AssetBundle frameworks, and hot-update frameworks in the target project; see [scope](docs/development.md#使用范围).

## 反馈问题

**Report issues**

反馈时提供工具版本、系统、Unity 版本（如涉及）、实际操作或 CLI 命令、第一条错误以及最小复现。固定日志位于 `Output/Logs/export.log` 和 `diagnostics.json`，独立历史记录位于 `Output/Logs/<运行编号>/`；也可从 App 导出诊断报告，并移除不适合公开的业务数据和路径。贡献代码前参考 [提交与发布检查](docs/development.md#提交与发布检查)。

Include the tool version, OS, Unity version if relevant, exact steps or CLI command, first error, and a minimal reproduction. Latest logs are `Output/Logs/export.log` and `diagnostics.json`; historical runs are in `Output/Logs/<run-id>/`. You can also export a diagnostic report from the App. Remove business data and paths unsuitable for publication. Before contributing, read [contribution and release checks](docs/development.md#提交与发布检查).

## 许可

**License**

本项目源码、文档、示例及生成模板采用 MIT，允许商用、修改和再分发，分发时保留许可声明。使用者自己的工作簿、业务代码和游戏数据由使用者决定授权方式。

Source, documentation, samples, and generation templates use MIT, permitting commercial use, modification, and redistribution with the license notice retained. Users choose the licenses for their own workbooks, business code, and game data.

第三方组件遵循各自许可证，声明见 [第三方许可](src/ZrConfig.App/Licenses/ThirdPartyNotices.txt)。

Third-party components retain their own licenses; see [third-party notices](src/ZrConfig.App/Licenses/ThirdPartyNotices.txt).
