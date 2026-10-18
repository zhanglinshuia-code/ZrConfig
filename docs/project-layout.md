# 项目维护

**中文** | [English](project-layout.en.md)

公开文档保留现有 `.md` 文件作为中文版，并在同一目录维护对应的 `.en.md` 英文版。每页顶部提供语言切换；目录、章节链接和源码分发清单须同时覆盖两版。教程附件使用 `README.txt` 与 `README.en.txt`，共用一套源码和工作簿。

本文面向源码与示例维护者，描述实际文件布局。日常配表只需按 [使用文档](user-guide.md) 选择根目录、填写和导出，无需学习内部源码目录或手写读写代码。

## 目录职责

| 目录 | 内容 |
|---|---|
| src | Core、CLI、App，以及链接 Assets 源码的 Runtime/Validation .NET 工程；第三方声明集中在 App/Licenses，App/CLI 链接共用 |
| .github/workflows | Windows/macOS 公开源码构建、生成、校验、导出及打包自动验证，以及 Windows/Linux/macOS 服务器运行时最低与当前工具链验收 |
| samples/Data | **正式 Excel 唯一根目录**，可在其下分类；工作簿清单见 [示例入口](../samples/README.md#打开哪个工作簿) |
| unity/ZrConfig.Sample/Assets/HotUpdate/Config | 项目业务类型、工具提供的读写支持、可选转换器与全部表的生成源码；程序员/工具维护 |
| unity/ZrConfig.Sample/Assets/Editor/ConfigRules | 共用业务检查规则 |
| samples/Projects | 链接 Config/Rules 的 Models、Validators 编译工程 |
| samples/Output | 统一导出和日志；按语言组织 Common 和 Tables/json、Tables/bytes，表文件按需覆盖 |
| scripts/samples | C# Excel 示例重建代码和 AllTypesDefinition.json 定义 |
| scripts | .NET 维护工程与 Bash/PowerShell 入口；构建、打包、安装和清理 |
| tests | 集成/UI/性能验证；ZrConfig.Server.Tests 是公开的七语言契约与消费方验收入口，链接 ZrConfig.Tests 中的四份共享夹具；测试 Excel 只生成到临时目录或 obj |
| docs | 使用与维护说明，以 README.md 为导航入口 |
| docs/assets | 教程下载附件，不作为正式导表输入 |
| unity/ZrConfig.Sample/Assets/ZrConfig | Runtime、Validation、Unity/Runtime 和 Unity/Editor 的唯一源码，按 asmdef 划分程序集 |
| unity/ZrConfig.Sample | 可运行 Unity 示例场景、展示脚本和工程设置；直接编译 Assets 中的配置/规则源码，复用统一导出数据 |
| unity/ZrConfig.Sample/Assets/ZrConfig/Native | Java、Go、C++、Lua、TypeScript、Rust 公共运行库的唯一维护源码，使用 .txt 模板避免 Unity 将其识别为原生插件；Core 链接嵌入，按目标生成到共用 Config/Generated 的语言子目录 |
| artifacts | 发布包与性能报告，可重建 |

Unity 示例的展示脚本与场景集中在 unity/ZrConfig.Sample/Assets。全部示例共用的配置源码位于 unity/ZrConfig.Sample/Assets/HotUpdate/Config，后置检查位于 unity/ZrConfig.Sample/Assets/Editor/ConfigRules，均为可直接编辑的 Assets 文件；生成器每次直接更新 Config/Generated。配置编译为独立 ZrConfig.Models，规则编译为仅编辑器可用的 ZrConfig.Sample.Rules.Editor。目录和文件名使用英文。

samples/Projects 继续链接配置与规则进行 .NET 编译检查；src/ZrConfig.Runtime、src/ZrConfig.Validation 只保留 .NET 工程，链接 Assets/ZrConfig 下的对应源码。Unity 直接编译全部 ZrConfig 源码，不导入 ZrConfig DLL，不使用 com.zrconfig.unity 包。Validation 和 Unity/Editor 的 asmdef 限定 Editor。`unity-sample` 生成配置、导出数据并将统一导出数据部署到 Assets/StreamingAssets/ZrConfig；`package-unity` 仅向新的 artifacts 目录导出可放入其他工程的 Assets/ZrConfig 源码。旧 samples/Config、samples/Rules 已统一迁移，不保留副本或链接；项目设置使用当前保存的路径，不自动重定向历史目录。

Unity 的 .meta 与对应源码一同保存，Generated 的 .meta 不参与生成源码内容比对；Library、Temp、Logs、UserSettings 和构建缓存不纳入版本控制。每张表统一生成一个 XxxConfig.g.cs，包含记录、查询表、扩展入口和 Codec，全部表共用唯一的 TableCodecs.g.cs。旧拆分文件及其配套 .meta 经过生成预览和确认后事务删除，保留合并主文件的 GUID；不保留两套生成布局，详见 [生成文件组织](development.md#生成文件组织)。示例操作统一维护在 samples/README.md。

本仓库按职责将手写业务类型放在 CustomTypes，这是项目整理约定，不是工具的扫描限制。App/CLI 从用户选择的代码根目录递归读取 C#，根目录文件与任意命名的子目录都可使用，不再自动创建类型目录。生成阶段排除根目录下由工具管理的 Generated；生成和导出都跳过 bin、obj、点号开头的文件/目录和链接文件/目录。导出阶段包含 Generated。工具管理的支持源码仍位于 Serialization，业务规则由独立规则路径指定，放在代码根目录之外。

## 新增表

1. `.xlsx` 或 `.xls` 工作簿放到 samples/Data 或其子目录，工具递归读取；每个数据 Sheet 的名称就是表名，须全局唯一，不因目录或工作簿不同而允许重名。
2. 需要项目扩展时，业务类型放到 unity/ZrConfig.Sample/Assets/HotUpdate/Config/CustomTypes，特殊转换器放到 unity/ZrConfig.Sample/Assets/HotUpdate/Config/Serialization，规则放到 unity/ZrConfig.Sample/Assets/Editor/ConfigRules；通用支持代码由工具提供，不要求配表人员编写。
3. 对整个 samples/Data 生成到 unity/ZrConfig.Sample/Assets/HotUpdate/Config，再统一导出到 samples/Output。
4. 在 samples/README.md 补上工作簿用途，更新 docs/README.md 导航，运行 `bash scripts/maintain.sh check-layout` 与 `bash tests/smoke.sh`；Windows 使用相应 PowerShell 入口。

不再按功能另建 Data/Config/Output。Data 内可以按用途分类，但根目录之外不另建输入树。Config/.schema.json 保存生成结构摘要，与生成事务共同提交；Config/.generation.json 保存相对输入根目录，Config/Serialization/.support.json 保存支持源码内容版本，均随项目保存；运行锁和编译缓存由工具维护。项目目录配置保存为项目根目录下的 `*.zrconfig.json`（正式示例使用 samples 根目录），App 与 CLI 的 `--project` 共用 Core 中的读取逻辑。Generated 只由工具维护；所有表共用唯一的 TableCodecs，不复制序列化支持源码。不要把临时测试 Excel、工作簿备份或练习包解压结果放入正式 Data 的任何层级。

新建表统一使用五行表头：字段名、类型、主键、说明、所属端，数据从第 6 行开始。主键在 Excel 第 3 行标记，至少标记一列；完整组合唯一，每个字段自动提供分组查询。主键以 Excel 声明为准，同一表不能在两处配置。Tables.json 只接受可选的 headerRows: 5 及自定义 groups；主键统一在 Excel 第 3 行声明。

仓库内 App/CLI 默认读取统一示例目录。App 按保存的路径加载项目，旧路径需要在项目设置中更新。独立工具继续支持选择外部项目。

所有表固定保留第 5 行 C/S/CS 所属端，第 6 行起数据；第 5 行不能省略，每个字段必须明确声明 C、S 或 CS，缺失标记报错。无需在 Tables.json 中启用布局，不另外建立功能专属 Data/Config/Rules/Output。生成和导出使用同一个选定目标端，规则见 [字段所属端](config-types.md#客户端与服务端字段)。

旧四行示例的显式迁移入口为 `bash scripts/maintain.sh samples migrate-headers <已保存并关闭的工作簿目录>`。该本地维护命令逐格保留数据，在系统临时目录备份原文件，遇到无法确认的布局停止；也支持迁移独立教程 ZIP。导出入口不会自动改写工作簿。

## 清理与验证

本节的 `check-layout`、`smoke` 与工作簿重建依赖完整维护工作区中的本地扩展，不随公开仓库或源码 ZIP 分发。公开使用者运行 `check-portability`、`verify-source` 和 [公开服务器验收](../tests/README.md#公开服务器验收)；本目录约定用于说明完整项目的文件归属，不表示所有维护文件均公开。

```bash
bash scripts/clean.sh           # 清理 bin/obj、编译缓存和 Finder 元数据
bash scripts/clean.sh --outputs # 额外清理 samples/Output 与 artifacts
bash scripts/maintain.sh check-layout # Excel 位置和文档本地链接
bash tests/smoke.sh             # 生成比对、构建、UI、集成与统一示例导出
```

Windows 对应 `powershell -File scripts/maintain.ps1 clean`、`powershell -File scripts/maintain.ps1 check-layout`、`powershell -File tests/smoke.ps1`。这些入口共用 C# 实现，只需 .NET SDK 10；macOS 不要求安装 PowerShell。源码打包、解压安装、路径元数据清理与发布许可校验共用 scripts/ReleaseFiles.cs；Windows 打包逻辑共用 scripts/Packages.cs。清理脚本跳过目录链接，保留维护进程正在使用的 scripts/bin、scripts/obj；如需删除，先退出维护进程。

源码、正式 Excel 和文档不会被清理脚本删除。不要把唯一的源码或测量记录放入产物目录。固定日志表示最近一次，独立历史日志保存在 Output/Logs/&lt;运行编号&gt;；新导出使用固定表文件路径，已有旧版目录保留。性能报告固定写到 artifacts/performance/latest。

## 文档职责

日常配表、安装、按钮操作与数据验收维护在 [user-guide.md](user-guide.md)，Excel 填写语法维护在 [config-types.md](config-types.md)。业务类型、转换器、完整编程练习、命令行与 Unity 接入统一在 [project-development.md](project-development.md)，不作为日常配表的必修步骤。[规则开发](rules-development.md) 负责标签与后置检查契约，[工具开发指南](development.md) 负责工具架构、支持源码升级、示例重建与维护，[调试指南](debugging.md) 统一维护 Rider、Visual Studio 与 Unity 操作，[server-generation.md](server-generation.md) 负责多语言与服务器接入、API 迁移和测量口径，[server-runtime-design.md](server-runtime-design.md) 保留设计约束和原始基线，[tests/README.md](../tests/README.md) 负责验证命令。

[samples/README.md](../samples/README.md) 是正式工作簿的统一使用入口。完整维护工作区还通过 `docs/project-status.md` 记录当前版本、实际发布包和验证结果，通过 `docs/performance-and-style.md` 保留带日期与环境的历史测量依据；这两份内部记录与 `artifacts` 不随公开源码 ZIP 分发。公开接入与验收命令见 [开发指南](development.md) 和 [服务器验证说明](../tests/README.md)，避免将本机历史报告当作当前发行结论。

主键文档的填写部分面向配表人员，查询 API 部分明确面向程序员。源码仍按现有约定归位；调整文档受众不代表通用序列化支持已经迁入 Runtime。新增专题只在 docs 中建立，入口同步到 docs/README.md，不复制多套教程或示例源码。

文档中的临时练习、故意改错与编译验证不写入正式 Data；保留原工作簿和有效发布版本。独立练习包与正式示例的同名模型可能不同，不能合并输入。
