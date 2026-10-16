# 文档导航

**Documentation index**

本文及所列使用、开发、示例和验证文档均逐节提供中英文。保留既有中文标题锚点，方便旧链接继续访问；代码、Excel 标记与示例数据保持原值，代码块后的英文说明用于解释其中的中文文本。

This index and every listed user, developer, sample, and verification guide provide Chinese and English section by section. Existing Chinese heading anchors preserve old links. Code, Excel markers, and sample values retain their original literals; English notes after code blocks explain Chinese text in those examples.

工具介绍与界面截图见 [项目首页](../README.md)，本文是公开源码和源码 ZIP 共用的文档入口。所有相对链接均指向公开文件；安装包和导出数据按文档命令在本机生成。

See the [project homepage](../README.md) for an introduction and screenshots. This index serves both the public repository and source ZIP. All relative links point to public files; build installers and export data locally using the documented commands.

## 第一次阅读

**Start here**

- **配表人员**：[使用文档](user-guide.md) → [示例说明](../samples/README.md) → [Excel 填写语法](config-types.md)。<br>
  **Configuration authors:** [User guide](user-guide.md) → [Samples](../samples/README.md) → [Excel syntax](config-types.md).
- **服务器程序员**：[多语言生成与 JSON](server-generation.md) → [服务器加载与生命周期](server-generation.md#服务器契约家族与显式加载) → [公开服务器验收](../tests/README.md#公开服务器验收)。<br>
  **Server developers:** [Multi-language generation and JSON](server-generation.md) → [Server loading and lifecycle](server-generation.md#服务器契约家族与显式加载) → [Public server acceptance](../tests/README.md#公开服务器验收).
- **Unity 程序员**：[运行 Unity 示例](../samples/README.md#unity-可运行示例) → [项目开发教程](project-development.md) → [规则开发](rules-development.md)。<br>
  **Unity developers:** [Run the Unity sample](../samples/README.md#unity-可运行示例) → [Project development](project-development.md) → [Rule development](rules-development.md).
- **工具维护者**：[环境与构建](development.md#环境与构建) → [架构与数据流程](development.md#架构与数据流程) → [提交与发布检查](development.md#提交与发布检查)。<br>
  **Tool maintainers:** [Environment and building](development.md#环境与构建) → [Architecture and data flow](development.md#架构与数据流程) → [Contribution and release checks](development.md#提交与发布检查).

## 了解工具与亮点

**Introduction and features**

| 想了解什么<br>Topic | 入口<br>Entry point |
| --- | --- |
| 工具是什么、有什么亮点<br>What the tool does and its features | [项目首页](../README.md#核心亮点)<br>[Homepage](../README.md#核心亮点) |
| 编写业务检查，提前发现配表错误<br>Write business checks to catch configuration errors early | [可编程后置检查](rules-development.md)<br>[Programmable post-validation](rules-development.md) |
| 用 Visual Studio / Rider 调试规则<br>Debug rules in Visual Studio / Rider | [调试指南](debugging.md)<br>[Debugging](debugging.md) |
| 支持哪些数据结构<br>Supported data structures | [结构与类型](config-types.md#先选配表结构)<br>[Structures and types](config-types.md#先选配表结构) |
| 看现成效果<br>Explore working examples | [Excel 与 Unity 示例](../samples/README.md)<br>[Excel and Unity samples](../samples/README.md) |

## 日常配表与导出

**Authoring and export**

| 内容<br>Topic | 文档<br>Document |
| --- | --- |
| 安装、项目设置、只校验、生成、导出、报错处理<br>Installation, project settings, validation-only mode, generation, export, and troubleshooting | [使用文档](user-guide.md)<br>[User guide](user-guide.md) |
| 只校验、版本比较、受控回滚与平台验收<br>Validation-only mode, version comparison, controlled rollback, and platform acceptance | [发布管理与验收](release-and-acceptance.md)<br>[Release management and acceptance](release-and-acceptance.md) |
| 日志复制与清理<br>Copy and clear logs | [查看与清理日志](user-guide.md#22-查看与清理日志)<br>[Viewing and clearing logs](user-guide.md#22-查看与清理日志) |
| 类型、数组、对象、字典和多行填写<br>Types, arrays, objects, dictionaries, and multi-row input | [Excel 填写语法](config-types.md)<br>[Excel syntax](config-types.md) |
| 固定五行表头与 C / S / CS 标记（第 5 行必需，字段须明确声明所属端）<br>Five fixed header rows and C / S / CS markers (row 5 is mandatory and fields must declare their target side) | [客户端与服务端字段](config-types.md#客户端与服务端字段)<br>[Client and server fields](config-types.md#客户端与服务端字段) |
| 正式 Excel 与 Unity 工程运行步骤<br>Official Excel workbooks and running the Unity project | [示例说明](../samples/README.md)<br>[Samples](../samples/README.md) |
| 全类型工作簿的字段与例子<br>Fields and examples in the all-types workbook | [全类型示例](type-examples.md)<br>[All-types sample](type-examples.md) |
| 一万行、106 字段的加载测试输入<br>Loading-test input with 10,000 rows and 106 fields | [全类型万行用例](../samples/README.md#一万行全类型加载用例)<br>[10,000-row all-types case](../samples/README.md#一万行全类型加载用例) |
| 主键、组合键与分组查询<br>Primary keys, composite keys, and group queries | [表查询](table-indexes.md)<br>[Table queries](table-indexes.md) |

## 程序开发与调试

**Development and debugging**

| 内容<br>Topic | 文档<br>Document |
| --- | --- |
| 一表一文件、类型职责与旧布局迁移<br>One file per table, type responsibilities, and old-layout migration | [生成文件组织](development.md#生成文件组织)<br>[Generated file organization](development.md#生成文件组织) |
| C# / C++ / Go / Java / Lua / TypeScript / Rust 生成、JSON、快照及性能边界<br>C# / C++ / Go / Java / Lua / TypeScript / Rust generation, JSON, snapshots, and performance limits | [多语言生成与 JSON](server-generation.md)<br>[Multi-language generation and JSON](server-generation.md) |
| 服务器运行时的设计约束、原始基线与实施记录（维护者阅读）<br>Server runtime design constraints, original baselines, and implementation records for maintainers | [服务器运行时设计](server-runtime-design.md)<br>[Server runtime design](server-runtime-design.md) |
| 七语言公共契约、冻结消费方及最低工具链验证命令<br>Seven-language public contracts, frozen consumers, and minimum-toolchain verification | [公开服务器验收](../tests/README.md#公开服务器验收)<br>[Public server acceptance](../tests/README.md#公开服务器验收) |
| API 变化预览、消费方验收与显式迁移<br>API change previews, consumer acceptance, and explicit migration | [公开接口保护](server-generation.md#公开接口保护与显式迁移)<br>[Public API protection](server-generation.md#公开接口保护与显式迁移) |
| 业务类型、转换器、CLI 和 Unity 接入<br>Business types, converters, CLI, and Unity integration | [项目开发教程](project-development.md)<br>[Project development](project-development.md) |
| validate、报告、取消和退出码<br>validate, reports, cancellation, and exit codes | [命令行使用](project-development.md#13-命令行使用)<br>[CLI usage](project-development.md#13-命令行使用) |
| 各表直接提供 Codec、无注册表同步/异步加载与全量目录<br>Direct table codecs, registry-free synchronous/asynchronous loading, and the full catalog | [运行时懒加载](project-development.md#按行懒加载与跨表引用)<br>[Lazy runtime loading](project-development.md#按行懒加载与跨表引用) |
| 第三方 AB 框架的通用同步/异步接入<br>Generic synchronous/asynchronous integration with third-party AssetBundle frameworks | [AB 接入](project-development.md#第三方-ab-框架同步与异步)<br>[AssetBundle integration](project-development.md#第三方-ab-框架同步与异步) |
| 可运行的 AB 临时桥接、双模式查询与读取日志<br>Runnable temporary AssetBundle bridge, dual-mode queries, and read logs | [Unity AB 示例](../samples/README.md#ab-同步与异步示例)<br>[Unity AssetBundle sample](../samples/README.md#ab-同步与异步示例) |
| 字段、逐行、整表、跨表检查<br>Field, row, table, and cross-table checks | [规则开发](rules-development.md)<br>[Rule development](rules-development.md) |
| Unity 框架源码位置与接入<br>Unity framework source location and integration | [Assets 源码接入](project-development.md#142-接入-unity-源码)<br>[Assets source integration](project-development.md#142-接入-unity-源码) |
| Android / iOS / WebGL 异步资源读取<br>Asynchronous resource reads on Android / iOS / WebGL | [内置 StreamingAssets 适配器](project-development.md#145-androidioswebgl-与-streamingassets)<br>[Built-in StreamingAssets adapter](project-development.md#145-androidioswebgl-与-streamingassets) |
| Player 构建、设备结果与实际覆盖范围<br>Player builds, device results, and actual coverage | [平台验收](release-and-acceptance.md#真实-player-入口)<br>[Platform acceptance](release-and-acceptance.md#真实-player-入口) |
| Rider / Visual Studio / Unity 调试<br>Rider / Visual Studio / Unity debugging | [调试指南](debugging.md)<br>[Debugging](debugging.md) |
| 源码启动、打包、支持源码升级、当前表头约定<br>Run from source, package, upgrade support source, and use current headers | [源码构建与升级](development.md)<br>[Building and upgrading](development.md) |
| C# 命名、格式、生成模板与源码维护<br>C# naming, formatting, generation templates, and source maintenance | [代码规范](development.md#c-代码规范)<br>[Code conventions](development.md#c-代码规范) |
| 生成预览复核、失败回滚与恢复备份<br>Review generation previews, rollback failures, and recovery backups | [生成一致性](development.md#生成一致性与失败恢复)<br>[Generation consistency](development.md#生成一致性与失败恢复) |
| 公开文件范围、目录和生成文件归属<br>Public files, directories, and generated-file ownership | [公开源码与目录](development.md#公开源码与目录)<br>[Public source and layout](development.md#公开源码与目录) |
| 维护目录约定与公开验收入口<br>Maintenance layout conventions and public verification entry points | [目录约定](project-layout.md) · [验证说明](../tests/README.md)<br>[Layout](project-layout.md) · [Verification](../tests/README.md) |
| 可重复执行的构建、导出与 Unity 验证<br>Repeatable build, export, and Unity verification | [验证公开源码](development.md#验证公开源码)<br>[Verify public source](development.md#验证公开源码) |
| 加载策略、预热切换与诊断<br>Loading policies, warmup switching, and diagnostics | [预热与加载诊断](project-development.md#预热与加载诊断)<br>[Warmup and loading diagnostics](project-development.md#预热与加载诊断) |
| 查询分配、索引与更新后的对象使用<br>Query allocations, indexes, and object use after updates | [分配与热更新](table-indexes.md#分配与热更新)<br>[Allocations and hot updates](table-indexes.md#分配与热更新) |
| 单表封装、提交点与锁顺序<br>Table encapsulation, commit points, and lock ordering | [索引与单表加载职责](development.md#索引与单表加载职责)<br>[Indexes and table-loading responsibilities](development.md#索引与单表加载职责) |
| GitHub 提交、发行内容和反馈要求<br>GitHub submissions, release contents, and issue-report requirements | [提交与发布检查](development.md#提交与发布检查)<br>[Contribution and release checks](development.md#提交与发布检查) |

## 示例与练习附件

**Samples and tutorial attachments**

正式工作簿集中在 [samples/Data](../samples/Data)，九张表共用一套 Assets 配置源码、规则和 Output，包含一万行加载用例。所有正式示例已经配好，无需先完成开发练习。

Official workbooks live in [samples/Data](../samples/Data). Nine tables, including the 10,000-row loading case, share Assets configuration source, rules, and Output. All official samples are ready to use; completing the development exercises first is unnecessary.

[独立练习附件](assets/user-guide-example.zip) 配套项目开发教程第 12 章，请解压到仓库之外。它使用另一套 Hero/Skill 教学字段，不能与正式工作簿或同名模型混用。

The [standalone exercise attachment](assets/user-guide-example.zip) accompanies chapter 12 of the development tutorial. Extract it outside the repository. It uses a different Hero/Skill teaching schema and must not be mixed with official workbooks or models with the same names.
