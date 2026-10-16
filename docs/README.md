# 文档导航

工具介绍与界面截图见 [项目首页](../README.md)，本文是公开源码和源码 ZIP 共用的文档入口。所有相对链接均指向公开文件；安装包和导出数据按文档命令在本机生成。

## 第一次阅读

- **配表人员**：[使用文档](user-guide.md) → [示例说明](../samples/README.md) → [Excel 填写语法](config-types.md)。
- **服务器程序员**：[多语言生成与 JSON](server-generation.md) → [服务器加载与生命周期](server-generation.md#服务器契约家族与显式加载) → [公开服务器验收](../tests/README.md#公开服务器验收)。
- **Unity 程序员**：[运行 Unity 示例](../samples/README.md#unity-可运行示例) → [项目开发教程](project-development.md) → [规则开发](rules-development.md)。
- **工具维护者**：[环境与构建](development.md#环境与构建) → [架构与数据流程](development.md#架构与数据流程) → [提交与发布检查](development.md#提交与发布检查)。

## 了解工具与亮点

| 想了解什么 | 入口 |
| --- | --- |
| 工具是什么、有什么亮点 | [项目首页](../README.md#核心亮点) |
| 编写业务检查，提前发现配表错误 | [可编程后置检查](rules-development.md) |
| 用 Visual Studio / Rider 调试规则 | [调试指南](debugging.md) |
| 支持哪些数据结构 | [结构与类型](config-types.md#先选配表结构) |
| 看现成效果 | [Excel 与 Unity 示例](../samples/README.md) |

## 日常配表与导出

| 内容 | 文档 |
| --- | --- |
| 安装、项目设置、只校验、生成、导出、报错处理 | [使用文档](user-guide.md) |
| 只校验、版本比较、受控回滚与平台验收 | [发布管理与验收](release-and-acceptance.md) |
| 日志复制与清理 | [查看与清理日志](user-guide.md#22-查看与清理日志) |
| 类型、数组、对象、字典和多行填写 | [Excel 填写语法](config-types.md) |
| 固定五行表头与 C / S / CS 标记（第 5 行必需，字段须明确声明所属端） | [客户端与服务端字段](config-types.md#客户端与服务端字段) |
| 正式 Excel 与 Unity 工程运行步骤 | [示例说明](../samples/README.md) |
| 全类型工作簿的字段与例子 | [全类型示例](type-examples.md) |
| 一万行、106 字段的加载测试输入 | [全类型万行用例](../samples/README.md#一万行全类型加载用例) |
| 主键、组合键与分组查询 | [表查询](table-indexes.md) |

## 程序开发与调试

| 内容 | 文档 |
| --- | --- |
| 一表一文件、类型职责与旧布局迁移 | [生成文件组织](development.md#生成文件组织) |
| C# / C++ / Go / Java / Lua / TypeScript / Rust 生成、JSON、快照及性能边界 | [多语言生成与 JSON](server-generation.md) |
| 服务器运行时的设计约束、原始基线与实施记录（维护者阅读） | [服务器运行时设计](server-runtime-design.md) |
| 七语言公共契约、冻结消费方及最低工具链验证命令 | [公开服务器验收](../tests/README.md#公开服务器验收) |
| API 变化预览、消费方验收与显式迁移 | [公开接口保护](server-generation.md#公开接口保护与显式迁移) |
| 业务类型、转换器、CLI 和 Unity 接入 | [项目开发教程](project-development.md) |
| validate、报告、取消和退出码 | [命令行使用](project-development.md#13-命令行使用) |
| 各表直接提供 Codec、无注册表同步/异步加载与全量目录 | [运行时懒加载](project-development.md#按行懒加载与跨表引用) |
| 第三方 AB 框架的通用同步/异步接入 | [AB 接入](project-development.md#第三方-ab-框架同步与异步) |
| 可运行的 AB 临时桥接、双模式查询与读取日志 | [Unity AB 示例](../samples/README.md#ab-同步与异步示例) |
| 字段、逐行、整表、跨表检查 | [规则开发](rules-development.md) |
| Unity 框架源码位置与接入 | [Assets 源码接入](project-development.md#142-接入-unity-源码) |
| Android / iOS / WebGL 异步资源读取 | [内置 StreamingAssets 适配器](project-development.md#145-androidioswebgl-与-streamingassets) |
| Player 构建、设备结果与实际覆盖范围 | [平台验收](release-and-acceptance.md#真实-player-入口) |
| Rider / Visual Studio / Unity 调试 | [调试指南](debugging.md) |
| 源码启动、打包、支持源码升级、当前表头约定 | [源码构建与升级](development.md) |
| C# 命名、格式、生成模板与源码维护 | [代码规范](development.md#c-代码规范) |
| 生成预览复核、失败回滚与恢复备份 | [生成一致性](development.md#生成一致性与失败恢复) |
| 公开文件范围、目录和生成文件归属 | [公开源码与目录](development.md#公开源码与目录) |
| 维护目录约定与公开验收入口 | [目录约定](project-layout.md) · [验证说明](../tests/README.md) |
| 可重复执行的构建、导出与 Unity 验证 | [验证公开源码](development.md#验证公开源码) |
| 加载策略、预热切换与诊断 | [预热与加载诊断](project-development.md#预热与加载诊断) |
| 查询分配、索引与更新后的对象使用 | [分配与热更新](table-indexes.md#分配与热更新) |
| 单表封装、提交点与锁顺序 | [索引与单表加载职责](development.md#索引与单表加载职责) |
| GitHub 提交、发行内容和反馈要求 | [提交与发布检查](development.md#提交与发布检查) |

## 示例与练习附件

正式工作簿集中在 [samples/Data](../samples/Data)，九张表共用一套 Assets 配置源码、规则和 Output，包含一万行加载用例。所有正式示例已经配好，无需先完成开发练习。

[独立练习附件](assets/user-guide-example.zip) 配套项目开发教程第 12 章，请解压到仓库之外。它使用另一套 Hero/Skill 教学字段，不能与正式工作簿或同名模型混用。
