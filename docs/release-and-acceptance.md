# 发布管理与目标平台验收

**中文** | [English](release-and-acceptance.en.md)

本页定义 1.4.0 源码的发布操作、兼容条件、验证入口和试用记录。功能实现与验收结果分别记录：命令存在不代表某个平台已经验收通过。日常按钮操作见 [使用文档](user-guide.md)，接入加载策略见 [项目开发教程](project-development.md)。

## 操作边界

| 操作 | 代码 | Output | 结果与记录 |
| --- | --- | --- | --- |
| 只校验 | 不写入、不自动生成 | 校验流程不读取或写入所选 Output | 诊断留在本次会话；可主动导出报告；临时编译及二进制回读使用系统临时目录 |
| 生成代码 | 通过同一生成事务更新源码、结构摘要和元数据 | 不发布数据 | 预览按表、字段、类型、主键及端别展示变化，文件列表作为详情 |
| 校验并导出 | 必要时先确认生成 | 成功后原子切换 current.json | 每次进入导出流程的运行单独归档，成功候选版本保存独立清单 |
| 数据回滚 | 不修改游戏代码 | 核验后切换已有完整版本 | 必须提供部署端兼容清单及原因，默认先预览 |
| 历史清理 | 不修改源码 | 删除确认的过期版本 | 保护当前、保留、未知和仍处于读取窗口的版本 |

只校验发现生成代码缺失、支持源码待更新或结构过期时，返回“需要生成代码，完整校验未完成”。它不能使用旧模型忽略新字段后显示通过。Rules 留空时明确表示只执行基础校验；业务规则是可执行 C#，项目应遵守规则只读约定，工具没有对任意规则建立操作系统级隔离。

App 的项目状态栏独立读取当前版本名称；这项只读展示不属于只校验流程，也不会创建或切换版本。

数据校验通过不保证后续磁盘写入成功；导出重新读取并核验输入。Excel、配置源码、规则和 Tables.json 的内容指纹在发布前复核，发现变化拒绝切换。App 文件监听只提供结果可能过期的提示，正确性不依赖监听事件。

生成代码和发布数据有两个提交点。生成完成后再取消或导出失败，已生成代码保留，当前数据版本不变。生成事务提交期间先完成或回滚；版本激活临界区完成后显示发布成功，不因迟到取消改报失败。不能中断的单次第三方解析或自定义规则会等待返回；关闭运行中的 App 会请求取消并等待安全结束。

## 版本内容与日志

每个新版本包含 `manifest.json`、`release.json`、`schema.json`、`diagnostics.json`、所选格式的表数据（`.bytes`、`.json` 或两者）和供差异查看使用的 `.rows.json`。运行时只需要根 `current.json` 及其引用的全部表数据；双格式须保留 `file` 与 `jsonFile` 指向的两份数据。**不要把 `.rows.json` 行级差异快照和诊断当作游戏资源发布**。Unity 示例部署入口复制清单引用的数据，格式约定见 [文件、版本与加载](server-generation.md#文件版本与加载)。

`release.json` 记录工具版本、输入指纹、配置源码指纹、上一个版本及表变化。运行记录位于 `Output/Logs/<运行编号>/`，固定 `export.log` 和 `diagnostics.json` 仍表示最近一次导出，旧运行目录不被新导出覆盖。结构检查阶段失败、只校验和生成结果可通过 App“导出诊断报告”保存；不要把磁盘上旧的成功记录当作本次结果。

候选完成但未激活的目录可以保留用于排查；是否当前生效以根 `current.json` 为准。回滚记录在激活前写入准备记录，激活后追加回执，进程中断时同样以当前清单核对事实。

报告必须保存为项目 Data/Config/Rules/Output 之外的 .json 文件，避免覆盖输入和有效版本。报告导出不会附带工作簿或源码全文，已知项目根路径替换为占位符。业务错误消息可能包含业务数据或其他路径，分享前仍需检查内容。

## 比较、回滚与清理

App 的“版本历史”支持表级比较、指定表逐字段比较、保留版本、回滚和清理。先选择要查看的版本与比较基准，点击“与基准比较”；输入表名与偏移量后点击“逐字段比较”。逐字段比较按完整主键匹配，列出新增、删除和修改，支持分页；结构不同则停止逐字段比较，避免把不兼容布局误报为普通数据修改。历史版本缺少独立清单或行快照时，相应管理操作不可用，原数据继续保留。

CLI 示例中的版本编号和清单路径须替换为实际值：

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release list --project samples/project.zrconfig.json
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release diff --project samples/project.zrconfig.json --from "旧版本编号" --to "新版本编号"
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release rows --project samples/project.zrconfig.json --from "旧版本编号" --to "新版本编号" --table Hero --offset 0 --limit 100
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release rollback --project samples/project.zrconfig.json --version "目标版本编号" --compatibility "/实际路径/deployed-current.json" --reason "回滚原因"
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release cleanup --project samples/project.zrconfig.json --keep 10 --days 30
```

以上命令输出 JSON；`rows` 的 offset 从 0 开始，limit 为 1～1000，默认每页 100 条差异。回滚和清理默认只输出预览。核对后重复命令并添加 `--apply` 执行。每次 CLI 调用会重新计算计划，执行时再核验状态；App 的确认窗口使用当前预览计划，计划过期后需重新预览。

保留或取消保留指定版本：

```bash
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release pin --project samples/project.zrconfig.json --version "版本编号" --apply
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -- release unpin --project samples/project.zrconfig.json --version "版本编号" --apply
```

Windows 源码入口使用 `dotnet run --project src/ZrConfig.Cli -- release ...`；自包含包直接使用 `ZrConfig.Cli.exe release ...`，后续参数相同。release 必须指定 `--project`，回滚目标端取自项目配置的 Target。

回滚要求端别、格式、表集合、Schema 及配置源码指纹匹配，逐表核验 SHA-256；预览后的清单或当前版本变化会使预览失效。工具无法发现正在运行的游戏客户端，兼容清单必须由接入方从实际部署版本提供。缺少配置源码指纹的历史版本不自动回滚，不能用手工补字段绕过代码兼容检查。

兼容清单使用与当前部署代码配套的 `current.json` 或该部署版本的 `manifest.json`，由接入方保存并提供。不要仅为通过检查而选择待回滚版本自身的清单。回滚只切换本地 Output 的当前数据，不回滚 C# 代码、不自动上传 CDN，也不通知游戏重新加载；后续仍按项目发布流程部署清单引用的数据。

清理至少保留一个最新版本和一天读取窗口，默认保留 10 个最新版本及 30 天窗口。该窗口必须符合项目实际旧客户端存活时间；手动确认没有读者后才能删除。当前版本、保留版本、不完整旧目录及含未知子目录/链接的目录不会自动删除。清理多个历史版本不是跨目录事务，磁盘错误时检查实际剩余目录后重新预览。

发布、回滚和清理按相同顺序获取导出锁、发布锁。原子切换依赖受支持本地文件系统的同目录重命名语义；共享盘、SMB、NFS、同步盘需要单独验收。

## 公开源码复验

以下命令只依赖公开文件，不要求内部 tests 工程：

```bash
bash scripts/maintain.sh verify-source
```

Windows：

```powershell
powershell -File scripts/maintain.ps1 verify-source
```

该入口构建 App/CLI，在系统临时目录生成、校验、导出正式九表，检查只校验无写入、错误规则阻止发布、恢复和版本管理入口，结束后清理临时目录。它不修改正式 Excel、Generated 或 Output。公开 CI 在 Windows/macOS 执行该入口，不等同于原生窗口或 Player 验收。

公开工作流位于 [.github/workflows/ci.yml](../.github/workflows/ci.yml)，push、pull request 和手动触发时分别在 Windows/macOS 检查可移植性、运行 verify-source 并生成源码分发包。另外配置 Windows/Linux/macOS 的最低与当前工具链矩阵，执行公开 `ZrConfig.Server.Tests`、冻结消费方、可信 API 基线、Windows MSVC 及 Linux C++ sanitizer 检查；命令与工具链见 [公开服务器验收](../tests/README.md#公开服务器验收)。这些入口不依赖维护者本地的内部测试工程，实际通过范围以对应提交的 Actions 结果为准。

## 真实 Player 入口

将 `UNITY_EDITOR` 设置为工程对应的真实 Editor 可执行文件路径，安装匹配的主机平台构建模块、IL2CPP 工具链和许可证后执行：

```bash
export UNITY_EDITOR="/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/MacOS/Unity"
bash scripts/maintain.sh verify-player mono
bash scripts/maintain.sh verify-player il2cpp
```

Windows 先在 PowerShell 设置 `$env:UNITY_EDITOR = "C:\Program Files\Unity\Hub\Editor\6000.3.26f1\Editor\Unity.exe"`，再使用 `powershell -File scripts/maintain.ps1 verify-player mono` 或 `il2cpp`；安装位置不同则改为实际路径。入口在临时目录复制 Unity 工程、导出并部署数据，构建非 Development Player；IL2CPP 使用 High managed stripping，构建后实际启动 Player。源码工作区、正式数据和原 Unity 设置不被改写。报告、构建日志和 Player 保存在 `artifacts/acceptance/<本次编号>/`。

目标平台为 Android ARM64、iOS ARM64、WebGL WASM，三端统一用 IL2CPP，命令如下：

```bash
bash scripts/maintain.sh verify-player android
bash scripts/maintain.sh verify-player ios
bash scripts/maintain.sh verify-player webgl
```

三个入口检查相应 Editor 构建模块，分别生成 APK、Xcode 工程、WebGL 目录；它们只完成构建，不把构建成功计为设备运行通过。失败保存 failure.json；构建成功后保存 device-pending.json。Android 安装 APK 后启动；iOS 需要完整 Xcode、签名与真机部署；WebGL 通过支持生成文件压缩头的 HTTP(S) 服务打开，不能直接用 file:// 打开。验收包由 ZRCONFIG_ACCEPTANCE 编译标记自动启动测试，常规示例包没有该标记。

结果会显示在应用界面，保存到 Application.persistentDataPath/zrconfig-acceptance.json，同时以 ZRCONFIG_ACCEPTANCE_REPORT 前缀输出 JSON 日志。Android 可从 adb logcat 获取，iOS 从 Xcode 设备控制台获取，WebGL 从浏览器控制台复制；浏览器沙箱文件仅作辅助，不要求本地文件可直接访问。把完整 JSON、设备/浏览器版本和构建清单归档到对应 acceptance 目录，明确设备实际运行结果。未接设备或浏览器时保留“未执行”。

普通示例在 Android/iOS/WebGL 自动使用 StreamingAssetsConfigSource 与 CoalescingConfigSource：Android 的 jar: 路径和 WebGL 的 URL 由 UnityWebRequest 异步读取，禁止同步等待。适配器在 Unity 主线程构造和调用；不要配置后台加载调度器。异步源的未加载引用用 GetXxxAsync 访问，暖缓存查询可使用同步入口。资源 SDK 确定后仍要单独替换、验收项目 SDK。

探针覆盖按需读取、引用、组合键、分组、异步并发、取消与失败保护、指定行预热、全量加载和万行用例；报告包含首行、缓存查询、全量加载耗时及限定口径的内存/分配。它使用真实 StreamingAssets 与 UnityWebRequest，**不代表任何第三方资源 SDK 已验收**。分配计数器先用已知分配校准；不支持的平台返回 allocationCounterSupported=false、分配值 -1，不能把 API 恒为 0 误写为零分配。线程分配和整个 Player 存活内存不是同一指标，也不推导所有加载过程零分配。

本仓库 Unity 版本为 6000.3.26f1。Android 需要匹配模块及 SDK/NDK/JDK；iOS 需要 iOS 模块与完整 Xcode；WebGL 需要 WebGL 模块及实际浏览器。桌面成功不能代替这三端通过。

## 发行记录

每次候选发行使用新目录，保留已有有效包。源码、App、CLI、Unity 源码包及规则 SDK 确认来自同一份候选源码后，为具体发布目录保存记录：

```bash
bash scripts/maintain.sh release-record "/实际路径/发布目录"
bash scripts/maintain.sh release-check "/实际路径/发布目录"
```

命令在指定目录写入 `release-receipt.json`，记录工具版本、构建主机、公开源码与包内文件 SHA-256，拒绝覆盖已有记录。`release-check` 校验发布目录内的文件是否缺失、变化或增加；不会重新构建，也不会自动核验当前工作区源码与记录一致。哈希记录只证明记录时的文件对应关系，不证明签名、公证、Windows GUI、移动设备或资源 SDK 通过；各项证据需要分别归档。

升级时保留项目、手写类型、规则和有效发布版本。1.4.0 继续读取既有项目配置，并保存可选项目名称与项目格式版本；二进制表格式继续使用 v4。新增历史管理文件不是运行时必须加载的文件。

## 验收与团队试用记录

### 1.4.0 候选包实测（2026-10-15）

以下为 2026-10-15 较早一轮候选包的归档结果，构建主机为 macOS ARM64、Unity 6000.3.26f1。对应源码 ZIP 的 SHA-256 为 `98899d36569a6907632c28e0298bb4dd77c05236aeac3584492a5c2610a94a40`。该轮之后问题区改为连续只读文本并优化刷新；下面的原生 App 验收不覆盖这次界面调整，也不代表之后每个源码提交都重新完成了全部平台验收。

| 范围 | 实际结果 |
| --- | --- |
| 内部回归 | 完整 smoke、117 项集成验证、新工作流专项、Headless UI、26 项类型扩展通过；正式九表临时导出 0 错误、4 条预期警告 |
| 格式与布局 | 20 个工程、153 份源码/模板及目录布局检查通过 |
| macOS App | 该轮 ARM64 候选包实际启动，只校验、警告筛选、诊断定位和复制位置通过；没有发布或改写正式数据 |
| macOS Player | Mono、IL2CPP High stripping 构建并实际运行通过，覆盖探针全部 8 项检查 |
| WebGL | IL2CPP 构建成功，Chrome 155 浏览器实际运行全部 8 项检查通过，已归档 JSON 与画面 |
| Android ARM64 | IL2CPP APK 构建成功；未连接 Android 设备，真机运行未执行 |
| iOS ARM64 | IL2CPP Xcode 工程生成成功；本机只有 Command Line Tools，完整 Xcode 编译、签名和真机运行未执行 |
| Windows x64 | App/CLI 交叉打包成功；原生 Windows GUI 未执行 |
| 第三方资源 SDK / 代码热更新 | 未提供产品及版本，未执行；当前探针使用 UnityWebRequest |
| 正式发行与团队试用 | 稳定签名、公证、安装升级、目标设备性能预算和连续团队试用仍待完成 |

详细日志及截图保存在本地 `artifacts/acceptance`；构建入口产生的 `device-pending.json` 表示构建结束时的状态，后续实际运行以同目录的 `browser-report.json` 或设备报告为准。失败尝试的日志保留，不能把它们与后续成功记录混用。macOS 和 WebGL 本轮分配计数器均未通过校准，报告使用 -1，不作“零分配”结论。

### 当前源码与文档同步复验（2026-10-15）

本次补齐 1.4.0 使用、CLI、发布管理与 Unity 接入文档，README 使用用户提供的新版原始截图，保留同一资源路径。问题列表为连续只读文本，支持跨行选区、Ctrl/Cmd 多选、筛选、搜索、定位和复制；运行日志按需显示。

- 完整 smoke 通过，包含 117 项集成验证、Headless UI、只校验与发布管理专项、万行用例和打包保护。
- 正式九表在临时目录导出成功；正式 Data、Generated、Output 的 302 个已有文件 SHA-256 保持一致。
- 目录布局、公开源码可移植性与 12 份公开 Markdown 的 274 处本地链接、锚点和图片引用通过检查。
- 公开源码包解压到新的系统临时目录，不携带内部测试或本地扩展；`verify-source` 独立构建 App/CLI，验证生成、只校验无写入、九表导出、错误规则阻止发布、恢复及版本管理入口通过。

本次没有重新打包或运行原生 App、Player 与移动设备；上节候选包记录保留其原有范围。公开仓库包含验证入口和 CI 配置，CI 实际结果以对应 GitHub 提交的检查为准。

### 文档与公开源码复验（2026-10-16）

本轮同步七语言生成、Binary / JSON 交付、五行表头与服务器接入入口，区分 Unity 默认生命周期、传统原生入口和 `server-v1`。补齐 Git 忽略规则中遗漏的公开目录约定、测试说明及两份性能验收源码；公开源码包与 Git 候选文件清单一致，共 508 个文件。

- 完整维护工作区的 `check-layout`、`check-portability` 和 smoke 通过；包括 UI、生成一致性、正式九表导出、万行用例、失败保护与打包检查。
- 源码包解压到新的系统临时目录，`verify-source` 独立构建、生成、只校验、导出、恢复和发布管理通过；公开服务器工程 Release 构建 0 警告、0 错误。
- 干净源码包的七语言契约与消费方检查通过，使用 macOS arm64、JDK 17.0.20.1、Go 1.19.13、Apple clang 21（C++17）、Lua 5.4.0、Node 22.23.3 / TypeScript 5.8.3、Rust 1.70.0；未跳过原生语言。
- 正式 Data、Config、Rules 和 Output 的 660 个既有文件逐一核验 SHA-256，不因本轮整理与验证而改变。
- **推送门禁未通过**：以 GitHub 当前提交 `8627c968c396946f7060a2754751172d9695c764` 为可信基线执行 `--trusted-ref`，报告 `Migration history does not connect the trusted baseline to the candidate.`。远端旧版到首份本地 API 基线之间的变化未被现有迁移链覆盖。维护者已明确决定本次不补旧版兼容迁移，直接推送当前更新；保留该失败记录和现有检查，不将它记为通过。

本轮没有重新运行原生 App、Unity Player、移动设备、Windows/Linux/MSVC 或完整性能测量。历史实测按各自构建与环境保留，七语言功能检查成功不能替代可信发布基线通过。

### 后续记录要求

每条验收记录必须填写：候选源码/包哈希、平台与架构、Unity/SDK 版本、执行人、日期、步骤、实际结果、报告位置、未覆盖范围。状态限定为通过、失败、未执行、等待外部条件，不能用“入口已提供”替代通过。

| 验收项 | 必要条件与完成标准 |
| --- | --- |
| Windows GUI | 干净普通用户环境；目录、剪贴板、文件打开、DPI、取消、升级后设置保留 |
| macOS GUI | 实际启动与交互；中文路径、主题、权限与发行包启动体验 |
| 主目标 Player / IL2CPP | 实际目标后端、架构、设备运行；按需与全量切换、损坏保护和重试 |
| 资源 SDK | 明确产品和版本；真实句柄、请求合并、取消、缓存、下载与更新一致性 |
| 代码热更新 | 项目实际使用时验证 AOT、泛型、代码与数据兼容及更新顺序 |
| 性能 | 目标设备基线和预算，明确冷/暖查询、峰值、主线程停顿及统计口径 |
| 团队试用 | 1 名程序员、2～3 名策划、真实项目连续至少两周；记录修表耗时、误操作及阻断问题 |

试用任务包括打开项目、只校验、修正类型/主键/引用错误、生成字段变化、取消大任务、发布到真实 Player、兼容回滚和故障恢复。数据破坏、错误激活、错误结果标为成功，以及目标平台核心流程阻断均阻止正式发行。未获得设备、SDK 或真实试用人员时保留未执行状态，不补写通过结论。

macOS 的临时签名候选包每次重建可能重新触发系统的可移动卷访问许可；首次读取外置盘前应完成系统提示。目录监听和当前版本读取在后台启动，等待系统授权时窗口仍保持响应。正式发行仍需稳定签名、公证和升级安装验收。
