# 测试与修改后验收

**中文** | [English](README.en.md)

所有命令从仓库根目录执行。测试工程是 .NET 控制台程序，不是 `dotnet test` 测试适配器；必须实际运行 DLL 或完整 smoke。准备环境见 [开发指南](../docs/development.md)，断点操作见 [调试指南](../docs/debugging.md)。

公开源码 ZIP 包含本文、`ZrConfig.Server.Tests` 和四份共享服务器夹具，支持 `verify-source` 及本文的七语言契约、消费方与性能命令。完整 smoke、UI、工作簿重建和内部维护专项需要完整维护工作区；这些内部测试源码不在公开 ZIP 中。

## 公开服务器验收

GitHub 克隆和源码 ZIP 解压后均可执行以下命令；工作目录为仓库根目录。`verify-source` 只需 .NET SDK 10，七语言完整验收还需下表的外部工具链。

```bash
bash scripts/maintain.sh check-portability
bash scripts/maintain.sh verify-source
bash scripts/dotnet.sh run --project tests/ZrConfig.Server.Tests -c Release -p:NuGetAudit=false
```

Windows：

```powershell
powershell -File scripts/maintain.ps1 check-portability
powershell -File scripts/maintain.ps1 verify-source
dotnet run --project tests/ZrConfig.Server.Tests -c Release -p:NuGetAudit=false
```

| 工具链 | 最低目标 | 可选路径环境变量 |
| --- | --- | --- |
| Java | JDK 17 | `ZRCONFIG_JAVAC` |
| Go | 1.19 | `ZRCONFIG_GO` |
| C++ | C++17 编译器 | `ZRCONFIG_CXX` |
| Lua | 5.4，64 位整数与双精度 | `ZRCONFIG_LUA` |
| TypeScript / Node | 5.8 / 22 | `ZRCONFIG_TSC` 或 `ZRCONFIG_TYPESCRIPT`（typescript.js），以及 `ZRCONFIG_NODE` |
| Rust | 1.70 | `ZRCONFIG_RUSTC` |

默认入口强制要求六种原生工具链，并执行 C# 与六种语言的契约和消费方检查。仅验证托管部分时，在 `dotnet run` 的末尾加 `-- --managed-only`；它会明确跳过原生编译，不能计为七语言通过。`-- --api-only` 只执行 API 专项。已有构建可直接运行 `tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll`。

工具链预检输出每项工具的解析路径；缺失时指出对应环境变量及配置值。显式设置的路径不存在时不会回退到 PATH 中的其他版本；`ZRCONFIG_TYPESCRIPT` 设置后也不会回退到全局 `tsc`。TypeScript 7 使用原生 `tsc` 可执行文件（`ZRCONFIG_TSC`）；5.x/6.x 继续支持 `typescript.js`（`ZRCONFIG_TYPESCRIPT`）。CI 自动选择包提供的入口，另保存 `server-toolchains.log`，记录所选工具的路径和实际版本，便于定位最低／当前配置的安装问题。

Git 克隆还可运行 `-- --trusted-ref <可信提交SHA>`，从不可变目标提交比较 API 基线和迁移链；需要该提交的完整历史，源码 ZIP 没有 Git 历史，不执行此项。选取真正的目标基线，不能用当前候选自身代替。

[公开 CI](../.github/workflows/ci.yml) 配置 Windows/Linux/macOS 的最低与当前工具链矩阵，并保留日志。矩阵配置、一次本机成功和跨平台实际通过分别记录；最新状态以对应提交的 Actions 检查为准。性能命令见 [七语言服务器性能验收](#七语言服务器性能验收)。

以下“完整验证”“单独构建与专项验证”和 Unity 维护检查面向完整维护工作区；公开下载不包含这些内部工程和 smoke 脚本。

## 完整验证

```bash
bash scripts/maintain.sh check-layout
bash scripts/maintain.sh check-portability
bash scripts/maintain.sh check-style # 需先构建并在 Unity 中生成解决方案
bash tests/smoke.sh
```

Windows 原生 PowerShell：

```powershell
powershell -File tests/smoke.ps1
```

PowerShell 7 使用 `pwsh -File tests/smoke.ps1`，可在 Windows/macOS 执行。只依赖 .NET SDK 10，无需 Python、Bash、WSL 或 diff。两个 smoke 入口均调用 tests/SmokeChecks.cs，由 scripts/ZrConfig.Maintenance.csproj 编译，避免平台间漏检。

smoke 已包含目录检查，单独那条用于快速检查文档/布局。完整流程：

`check-style` 自动发现全部工程，独立检查所有源码、生成代码和支持模板的工程覆盖、解决方案 `.editorconfig` 副本、warning 级样式诊断、空白格式及配置文本格式；`check-style --fix` 应用可自动修正的规则，之后需重新运行检查。共享 Assets 使用 Unity 编译上下文，独立测试和夹具工程也显式参与检查。它需要本机 Unity 解决方案，故不放入只要求 .NET SDK 的 smoke。具体规则见 [代码规范](../docs/development.md#c-代码规范)。

1. 在系统临时目录创建三表夹具，使用五行表头与 Excel 主键；单独构建 CLI 并执行 codegen，确认生成入口不依赖项目模型与规则。
2. 比较四份共享序列化支持源码与模板生成结果。
3. 复制代码根目录的手写源码与 Tables.json 到临时 Config（排除 Generated 和缓存），读取统一 samples/Data 重新生成，与正式 Generated 逐文件比较。
4. 构建 ZrConfig.slnx，运行 Headless UI，再运行无参数的核心集成测试。
5. 验证显式 Config、省略 Rules 时仅执行基础校验；验证外部规则可以动态编译。
6. 在仓库外复制独立 CLI 运行文件，不带项目 Models/Validators DLL 或 Rules，验证规则修改生效、编译错误定位、失败保留 current.json、删除错误规则后恢复。
7. 将正式九表导出到临时 Output，确认行数：AllTypes 3、AllTypes10k 10000、Drop 3、SkillLevel 4、Hero 3、Skill 2、ItemCatalog 3、ShopOffers 3、QuestRewards 3，共 10024 条逻辑记录。
8. 在临时目录用 C# 重建全部示例，验证五行表头、现有文件保护和显式覆盖；对重建与正式 AllTypes 分别执行 103 个字段、631 次标量/成员比较与 9 处后置错误定位；对 AllTypes10k 检查一万行、106 字段、逐成员比较、二进制往返、按行加载和引用，检查清理范围、文档链接和打包保护。

smoke 不覆盖正式 Excel、正式 Generated 或 samples/Output；构建会更新 bin/obj，生成器类型发现只使用进程内编译缓存。脚本结束清理自己的临时工作簿与导出目录；强制杀进程时可留下临时目录，确认无进程使用后再清理。

## 单独构建与专项验证

```bash
bash scripts/dotnet.sh build tests/ZrConfig.Tests --disable-build-servers -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false -p:UsedAvaloniaProducts=
bash scripts/dotnet.sh tests/ZrConfig.Tests/bin/Debug/net10.0/ZrConfig.Tests.dll --rules-only
```

把最后一个参数替换为下表的一项。一次只选一个专项参数，入口会在匹配分支执行完后返回；不要同时传多个参数期待全部运行。

| 参数 | 覆盖重点 |
| --- | --- |
| `--api-only` | 实际公开声明、命名冲突、旧消费方编译运行、数据/说明与重生成稳定性、破坏性变化无写入、损坏/缺失基线、旧基线导入、精确迁移摘要和过期保护（纳入无参数入口） |
| `--delivery-only` | 只校验无写入、结构过期、输入复核、阶段取消、双提交点、版本与日志保留、逐字段差异、兼容回滚、清理保护及 CLI |
| 无参数 | 核心导出、诊断与语言、后置规则、发布保护、运行时，并调用多个专项；UI 另行运行 |
| `--ui-only` | Headless UI、浅色/深色日志可读性与主题切换、语言刷新、项目配置、生成确认/取消、主键缺失/拼错/混填阻止、目录拖拽/恢复 |
| `--usability-only` | CLI 普通/组合导出、共享流程取消/确认/过期预览、当前 Excel 重读、生成范围保护、支持源码升级/手写保护、编译来源分类、正式模型与文档 Unity 最小脚本编译（也纳入无参数入口） |
| `--rules-only` | 校验索引按需创建/跨规则复用/跨轮次隔离/无效依赖/单元格定位与查询分配；根目录/任意嵌套目录源码发现、目录改名、生成产物/缓存/链接排除，类型预览内存缓存命中/内容失效/语言/并发/LRU/卸载与只读保护，外部源码发现、规则修改、磁盘缓存、Debug/PDB、源码调用栈 |
| `--types-only` | 自定义类型、转换器、多态、编解码与相关分配 |
| `--extended-only` | 可空值、字典、矩形数组及确定性/边界 |
| `--columns-only` | 成员分列、多行列表、嵌套排序与诊断位置 |
| `--sheets-only` | 多工作表、命名、跳过说明页等 |
| `--excel-input-only` | 递归混合 XLS/XLSX、扩展名大小写、中文/空格路径、隐藏目录/Sheet、锁文件、真实二进制值/公式缓存、多行、跨格式引用、固定文化、重名/坏文件与失败保护 |
| `--indexes-only` | Excel 主键、自定义分组与生成查询接口，并包含 Excel 主键专项 |
| `--excel-keys-only` | 同表主键、非连续列、无 Id、九字段键、分组、缺失/拼错/混填在首次及再次生成的拦截、三行旧布局拒绝、工作簿/源码/发布保护、冲突与查询分配 |
| `--targets-only` | C/S/CS 按端生成与二进制导出、五行表头、显式 CS 共享字段、空白所属端拒绝、展开列同组继承、缺少所属端行和旧布局声明拒绝、主键/展开列标记、排除列表后的多行归组、引用、C# 9 编译、项目/CLI 选择与覆盖、切端取消、错误定位和失败保护（纳入无参数入口） |
| `--references-only` | 万表清单零 Codec/表状态、无注册表查询与引用、万表分段目录的 C# 9 编译及惰性枚举、表内部延迟单例、按需结构校验、按行懒加载、独立索引与损坏拒绝、无关行不解码、不可寻址输出、零表初始化、三表逐层访问/一万行循环、空引用、属性缓存、并发/取消/重试，以及无效目标拒绝、Required、双入口路由与更新失效 |
| `--runtime-only` | 加载/写入分配、自带 awaitable、同步/异步加载一致性、双入口委托路由、并发/取消/快照保护、读取器复用边界、请求合并/独立取消/超时、预热发布/失败保护、后台调度/提交竞态、诊断及 Unity 适配器生命周期 |

完整 smoke 的集成入口不会逐一调用每个 `--...-only` 分支。尤其改自定义类型时仍应按需执行 `--types-only`。最终摘要数字只统计部分检查，其他专项有自己的断言；以成功退出和全部日志为准，不把历史某个测试数量作为完成标准。

全类型逐字段期望检查另有入口；第二参数为仓库根目录，第三参数为维护脚本生成的期望值 JSON 文件，不是 Config 目录。本地维护工作区使用 `samples all --output <临时Data> --expectations <临时JSON>` 生成期望文件；两个输出路径均选系统临时目录，不覆盖正式工作簿：

```bash
bash scripts/dotnet.sh tests/ZrConfig.Tests/bin/Debug/net10.0/ZrConfig.Tests.dll --check-all-types "$PWD" "/系统临时目录/expectations.json"
```

Windows 专项命令把 Bash 包装器替换为 `dotnet`，目录检查使用 `powershell -File scripts/maintain.ps1 check-layout`；完整验证使用本目录 smoke.ps1。维护专项可单独运行 `bash scripts/maintain.sh check-maintenance` 或 `powershell -File scripts/maintain.ps1 check-maintenance`。

易用性专项 `--usability-only` 覆盖完整 CLI 参数、App/CLI 共用项目配置及整项目移动、单独生成只校验 Data/Config 而导出仍校验 Output/Rules、诊断 A1 定位、只读生成预览与支持源码升级确认。生成事务检查还覆盖一表一文件、旧三文件布局及配套 .meta 的确认/过期预览/逐操作回滚、保留主文件 GUID 与手写 partial、重复生成无变化、迁移前后二进制一致。UI 专项覆盖输出留空或误选文件时的单独生成确认/取消、首次导出的生成提示、取消无写入、数据修改无需生成及新增表确认后导出。

## 测试文件归属

| 工程/脚本 | 职责 |
| --- | --- |
| ZrConfig.Server.Tests | 公开七语言契约、冻结消费方和性能入口；链接四份共享夹具 |
| ZrConfig.Tests | 完整维护工作区的核心、运行时、UI、规则编译、专项与性能探针；仅四份共享服务器夹具公开 |
| ZrConfig.TestFixtures | 创建测试专用三表 Excel，必须指定临时目录 |
| ZrConfig.TestModels | 生成并编译夹具模型，产物位于 obj |
| ZrConfig.TestRules | 测试规则，只依赖测试模型 |
| samples/Projects | 编译检查正式 Config/Rules，通过链接引用源码 |
| CheckLayout.cs | 正式 Excel 仅在 samples/Data 及其子目录，Markdown 本地链接目标存在 |
| SmokeChecks.cs / MaintenanceChecks.cs | 共用冒烟流程；临时重建、逐字段比较、防覆盖、清理与打包验证 |
| Performance.cs | Release 独立进程测量和 JSON 报告汇总 |

ExcelInputTests 使用测试专属 NPOI 依赖在临时目录创建实际 BIFF/OLE `.xls`，不会把改扩展名的 `.xlsx` 当作 XLS 测试，也不会生成到正式 Data。无参数集成入口自动调用本专项；Unix 还创建循环目录链接验证跳过，Windows 无需链接创建权限即可运行其余断言。

check-layout 不校验 Markdown 标题锚点、远端网址、代码片段能否编译、命令行为。公开源码包的本地文档链接由 `check-portability` 按实际分发文件清单另行检查。文档整理还需核对标题锚点、代码围栏和表格；修改教程源码时比较第 12 章七份完整代码与附件，并在临时目录执行生成/导出。第 14.4 节 Unity 脚本由易用性专项提取编译。实际执行的日期、结果及未测范围统一记录在维护工作区的项目状态（`docs/project-status.md`），不能只凭目录检查通过宣称文档全部正确。

运行时专项还检查自带 awaitable 的完成/注册竞争、多次挂起、异常/取消、执行上下文隔离、同步/异步资源入口分离、调用方独立取消及重载重入拒绝。Unity 生命周期测试链接正式适配器源码，以可泵送的同步上下文检查资源读取、await 恢复、取消与 Changed 通知线程，但调用的是测试专用 Unity API 桩。引用专项通过模拟资源源检查 AB 层请求合并、独立取消和失败重试边界。它不启动 Unity Editor/Player，也不代表某个第三方 AB SDK、Odin 或 IL2CPP 的真实集成验收。

加载生命周期回归还会截留后台完成后的上下文恢复，分别验证行/整表遇到重载或取消时拒绝旧结果；暂停旧快照的 Codec 元数据检查，验证旧缓存清理等待期间新 Current 可用；在 Changed 回调中重载或重置，验证不会倒序发送旧通知；在清单读取及候选预热等待期间释放管理器或重置 Unity 会话，验证迟到请求不能重新发布。

加载 GC 回归补充：`--runtime-only` 覆盖无 Id/重复或零 Id 的索引语义、万行引用索引分配预算，以及 Unity 异步入口参数异常、预取消与当前 Codec 目录的惰性枚举。`--indexes-only` 验证新增同步整表 token 重载、原无参方法组、冷热缓存取消及强类型查询零分配。前后测量记录见维护工作区的加载 GC 与 API 检查（`docs/performance-and-style.md`）。

## Unity 示例工程验证

先按 [Unity 示例说明](../samples/README.md#unity-可运行示例) 执行 `unity-sample`。完整 smoke 还覆盖桌面数据部署的哈希、重复准备、失败清单保护、路径/链接拒绝和历史版本保留，以及源码打包与其他用户文件保护。生成源码比对忽略 Unity 自动生成的 .meta。

真实 Unity Editor 的数据验证：先把 UNITY_EDITOR 环境变量设置为 Unity Hub 中该版本 Editor 的可执行文件绝对路径（不同电脑、平台的安装目录可以不同）。

```bash
"$UNITY_EDITOR" -batchmode -nographics -quit -projectPath "$PWD/unity/ZrConfig.Sample" -executeMethod ZrConfig.Sample.Editor.SampleProject.VerifyData -logFile /tmp/zrconfig-unity-verify.log
```

验证示例的 AB 双入口时使用下面的异步入口（不加 `-quit`，由验证完成后自行退出）：

```bash
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD/unity/ZrConfig.Sample" -executeMethod ZrConfig.Sample.Editor.SampleProject.VerifyAbLoading -logFile /tmp/zrconfig-unity-ab-verify.log
```

该入口对共享 Output 与 StreamingAssets 分别检查真实文件的同步/异步读取、清单零行、引用按需加载、模式切换共享缓存、分组、组合键、全部表解码和重载后旧引用失效。在非 Play 模式下还追加示例显示数据回归：主动制造新快照已发布而 Changed 未到达的窗口，检查迟到通知、连续八次异步重载、显示数据稳定和失败后保留旧配置。运行时专项链接同一份 `SampleAbResources.cs`，在测试临时导出中验证资源读取失败重试、取消与失败重载保留旧快照；不写正式 Output。

维护专项还验证源码包包含 Unity Assets/.meta/Packages/ProjectSettings 和正式 Excel，排除缓存；在中文与空格目录解压，检查防覆盖、路径越界拒绝、无损清理工作簿元数据以及依赖升级时的许可检查。

关闭正在打开该工程的 Editor 后再执行批处理。交互检查打开示例场景并 Play，确认初始已加载 0 行，点击英雄/技能才按行加载、空引用不加载目标表，掉落组显式加载整表；切换同步模式后重载再查询，确认资源日志走同步入口；重复查询应显示 0 次资源读取。重新导出再点击重载，确认版本更新。退出/再次进入 Play 应可重新加载。`VerifyData` 同时检查 Unity 编译图：Runtime、Validation、Unity 适配器和配置均来自 Assets 源码；Validation、Unity.Editor 与后置检查从 Player 编译图排除，拒绝 Assets 下的旧 ZrConfig DLL。上述批处理验证不等于实际 Player 构建、IL2CPP 或代码热更框架验证。

## 修改后验收清单

- 文档/布局：导航更新、链接和锚点正确，新增文件按用途归位。
- 新表/生成器：全量生成一致、模型可编译、正式九表仍可导出，无多份注册表和源码副本。
- 新业务规则：合法数据通过、边界/空白/重复数据按预期拒绝，报错指向正确物理单元格；Warning 不误阻止。
- 跨表规则：目标解析失败时跳过依赖检查，独立本表检查仍能执行；不因重复键抛无关异常。
- 发布：失败的 current.json 字节保持不变；成功清单指向完整文件，行数与哈希对应。
- 加载：默认只读清单；行请求仅展开实际引用，失败/取消不发布半成品并可重试。显式 ReloadAll 保留完整版本校验与原子切换；成功更新后旧查询及旧引用失效，业务统一获取新对象。
- 调试：对目标 IDE 实测入口断点、动态规则断点、异常与位置映射；未实测则明确记录。
- Unity：在项目实际版本、平台、资源适配器、程序集/代码热更新方案中验证首次加载与重载。

## 性能检查

万行真实加载使用独立入口 `--loading-probe <仓库根目录> <JSON报告路径>`，读取共享 Output，覆盖首行、连续解码、完整表、并发读取、请求合并、后台处理、关键行预热、重载和暖查询；构建与复跑命令、.NET/Unity 测量口径见维护工作区的加载优化与实测（`docs/performance-and-style.md`）。测试源码为 `LoadingPerformanceTests.cs`，不在正式 Data 新增临时文件。

```bash
bash scripts/maintain.sh performance
```

Windows 使用 `powershell -File scripts/maintain.ps1 performance`。加 `--native` 使用真实桌面窗口，默认 Headless。维护程序串行启动独立的 Release 测试进程，每个进程最多 300 秒：1,000/10,000/50,000 行日志，10,000/100,000 条导出，再回放实际导出诊断日志。报告固定在 artifacts/performance/latest，每次替换上次报告，summary.json 汇总机器、模式、中位耗时、UI 队列最大延迟和内存。需要保留基线先归档已有报告。日常文档改动无需重新跑性能；只有性能相关修改或新声明需要测量。

报告应区分总分配、当前线程分配、存活内存和耗时，注明预热、数据规模和环境。强类型查询预热后 0 B 不等于导入/加载/建索引零分配；.NET 10 结果不代表 Unity/IL2CPP。历史口径见维护工作区的性能文档（`docs/performance-and-style.md`）。

`--diagnostic-probe <诊断数量> <新报告.json>` 测量实际 MainWindow 中的默认问题文本、最终结果填充、语言切换、搜索、标签切换与生产 FlushEvents；数量范围 1–50,000，实时批处理最多 10,000 条。先构建 Release，再单独运行该参数。运行日志按需生成文档，其首次生成成本计入 SwitchToLog，不能只看首次结果显示就宣称所有操作都更快。旧 `--perf-log` 只测 ColoredLogView，不能代表整个结果区域；历史 b8f4607 对照和当前单文本改造见维护工作区的性能文档（`docs/performance-and-style.md`）。

`--ui-only` 同时验证单文本问题区无逐条 UI 组件、仅排版可见行、万条乱序/多行诊断批量追加、排序/筛选/搜索、Ctrl/Cmd 实际鼠标多选、文档起点点击、跨行复制和定位、语言刷新不重建正文、隐藏日志恢复与清理、选区保留及撤销历史禁用。无变化的诊断重复刷新单独断言 0 B 托管分配；大文档追加使用 256 KiB 上限阻止整份字符串复制回归。该结果不代表有新日志或后续布局时零分配。

`--allocation-probe <报告.json>` 还覆盖 10 万行的三种分组分布、100 万次已建索引的引用解析和 10 万行数值标签校验；输入在计时前准备，每项预热 3 次、采样 7 次。使用 Release，并保持前后 JIT 设置一致。`--rules-only` 保留空值、浮点边界、长整数、重复/引用诊断及程序集卸载检查；`--runtime-only` 检查引用缓存零分配、目标表缺失、失败索引缓存和只读分组语义。

生成事务回归由易用性专项和完整 smoke 执行：覆盖表头／主键／目标端／类型源码／Tables.json／工作簿清单变更、数据行例外、并发写锁，并逐操作注入部分写入和删除失败，检查支持源码、生成文件、元数据一起回滚。另检查首次生成清理、`.meta` 保护与回滚失败后的可定位备份。测试文件操作注入仅作用于单个事务实例。

规则编译专项还覆盖缓存路径占用、不可读／不可写、损坏内容、保存和清理故障；验证警告与规则执行保留、错误源码不发布、缓存恢复后命中。磁盘故障注入只作用于单次源码准备，不更改全局编译状态。

单主键共享索引回归纳入 `--indexes-only`：覆盖整数、字符串、枚举、比较器、重复键、空表、只读字典、对象/分组复用、并发读取，以及显式 `List<T>.Enumerator` 与十万轮查询零分配。`--index-sharing-probe <report.json>` 对一万行预建行对象测量旧双索引和新共享索引；应使用 Release 并给报告唯一运行编号，口径及实际数据见维护工作区的性能记录（`docs/performance-and-style.md`）。

单表封装回归纳入 `--runtime-only`：在真实读取器建立和单行解码后暂停，分别触发取消、重载、Dispose，验证提交边界拒绝迟到结果、缓存/统计不受污染和取消后重试；整表构建故障验证已有行保留、半成品不发布、成功重试复用对象。原整表提交窗口、旧缓存清理、新 Current 可用、上下文恢复及 Unity 通知顺序用例继续保留，`--references-only` 覆盖引用绑定和生命周期。

第 4、5 批真实 Unity Editor 均使用现有 `ZrConfig.Sample.Editor.SampleProject.VerifyAbLoading`，同时执行 `VerifyData`，对共享 Output 与 StreamingAssets 检查数据、同步/异步文件 AB 桥接、引用、分组、缓存和重载；异步批处理不加 `-quit`，由入口结束后退出。Editor 验证与 Player/IL2CPP 验证分别记录在维护工作区的项目状态（`docs/project-status.md`），不以 Editor 成功代替目标平台验收。

## 多语言与 JSON 验收

P0 回归还覆盖六种原生运行时在读取表内容前拒绝额外 Ghost、缺表、等数量替换表，并保留原当前快照；分别测试二进制/JSON。`_Item` 实际通过 Go 编译。七语言修改说明后不触发 API 迁移，删除已启用语言必须单独批准。损坏旧源码与缺失基线的测试现在要求先恢复，不能依靠普通重新生成覆盖受保护接口。

`--portable-only` 验证生成、项目配置、二进制/JSON、数值边界、损坏数据、重载及 C# 查询分配。`--portable-native` 强制编译或执行 Java、Go、C++、Lua、TypeScript、Rust 生成物，验证查询与快照行为。工具链可通过 `ZRCONFIG_JAVAC`、`ZRCONFIG_GO`、`ZRCONFIG_CXX`、`ZRCONFIG_NODE`、`ZRCONFIG_LUA`、`ZRCONFIG_RUSTC` 指定；TypeScript 使用 `ZRCONFIG_TSC` 指定编译器，或 `ZRCONFIG_TYPESCRIPT` 指定 typescript.js 模块。最低目标为 Java 17、Go 1.19、C++17、Lua 5.4、Node 22 / TypeScript 5.8、Rust 1.70。TypeScript 检查严格编译与热查询堆增长；Lua 暂停 GC 测量堆变化，Rust 使用全局分配器计数。正式 Excel 不被修改，全部测试数据和生成物保存在系统临时目录。

257 表边界夹具同时生成旧入口与 server-v1，实际运行跨分段双向引用、四种加载模式、整表及强类型行/分组预热，并包含字符串组合键。Java 验证继承字段、嵌套类型、静态导入和方法引用；Go 验证空快照的零值行为；C++ 验证单表头文件的跨表类型可见性及预热键独立持有；Rust 验证根快照大小、后续分段失败时保留旧版本和句柄释放；TypeScript 覆盖同步/异步路径，TypeScript/Lua 另查行句柄持有与最终回收。C# 的取消原因测试将计时器取消与未来的单调检查点组合，分别检查超时、调用者取消和管理器取消，不依赖抢中某个时间窗口。

Rust 还在临时运行库副本中执行五项私有调度故障测试：行与分组等待者取消隔离、解码 panic 后唤醒和重试、永久错误缓存与临时失败重试、索引预算回滚及单次发布。测试使用通道和等待者计数控制交错，实际生成文件保持不变；最低与当前工具链都以 `-D warnings` 编译这些测试。

## 七语言服务器性能验收

公共验收入口 `tests/ZrConfig.Server.Tests` 同时提供独立进程的性能测量。先按多语言验收说明设置 Java、Go、C++、Lua、Rust、TypeScript 和 Node 的 `ZRCONFIG_*` 工具路径，再执行：

```bash
bash scripts/dotnet.sh build tests/ZrConfig.Server.Tests -c Release -p:NuGetAudit=false
bash scripts/dotnet.sh tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll --benchmark artifacts/performance/latest 10000 3
```

最后两个参数分别为主表行数和独立重复次数；行数支持 100、1000、10000、100000。输入、生成源码和编译产物均位于系统临时目录；报告为目的目录中的 `server-runtime.json`。固定数据包含窄表、24 字段宽表、长字符串、嵌套集合、高低基数分组和 64 行引用环。每次运行比较二进制/JSON、全量/选择闭包、严格按行及按需校验按行模式，以及 1%/10%/100% 访问覆盖率。每个样本独立启动，热查询有独立预热，校验和由宿主核对。

macOS/Linux 可设置 `ZRCONFIG_BENCH_TIME=1`，由 `/usr/bin/time` 记录子进程峰值内存；需允许读取本机进程资源统计。无可用统计时保存 `null`，不填零。`ZRCONFIG_BENCH_CPU` 与 `ZRCONFIG_BENCH_MEMORY_BYTES` 可记录已核实的硬件信息。峰值包含整个进程和启动成本，C# 工作进程包含动态消费方编译；它不是配置独占内存。热查询分位数是每批 100 次操作的均值分布，不是单请求尾延迟；Lua 使用 CPU 时间。Node 事件循环延迟测量整个同步工作批次的阻塞，不能当成异步调度器的延迟承诺。各语言分配计数与堆差值的具体含义保存在报告中。

首次运行建立性能基线；需要使用相同硬件、工具链、规模和配置多次比较后才能应用方案中的 10% 回归门槛。该入口不替代坏数据、并发、生命周期、旧消费方兼容或跨平台验收。

公共契约验收包含固定 xorshift32 种子 1729、104729、305419896、2654435769 的 C# 随机验证：96 轮屏障控制的准备、取消、Clear、Close、提交及旧快照读者交错，256 次二进制/JSON 随机位损坏。失败消息记录种子、轮次、动作和字节位置；超时只用于发现挂起，不用于判定竞态结果。这部分不替代其他语言的屏障和损坏夹具，也不作为穷尽证明。

补充场景入口：`--benchmark-samples <报告目录> <仓库根目录> 3` 复制正式九表到临时目录测量；`--benchmark-retained <报告目录> 3` 测量 0／1／4 份保留快照；`--benchmark-scenarios <报告目录> 3` 测量受控源延迟、第二次读取失败及可用语言的 1／4 个热读者；`--benchmark-queries <报告目录> 3` 分别测主键命中/缺失、组合键、分组、引用和快照获取的延迟与分配。具体口径见 [服务器性能测量](../docs/server-generation.md#可重复的性能测量)。冻结正式消费方使用 `--sample-consumer <语言|all> <Config目录> <Output目录>`，可作为迁移凭据的真实验收命令。

阶段和管理器细分入口（各保存到不同目录，避免覆盖基线）：

```bash
bash scripts/dotnet.sh tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll --benchmark-stages artifacts/performance/stages "$PWD" 3
bash scripts/dotnet.sh tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll --benchmark-control artifacts/performance/control 3
bash scripts/dotnet.sh tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll --benchmark-locks artifacts/performance/locks "$PWD" 3
```

万表业务生成与加载使用独立入口，不能用目录图基准代替：

大型 TypeScript 编译可设置 `ZRCONFIG_TYPESCRIPT_HEAP_MB=16384`，范围为 64–65536 MiB；该选项只调整编译进程，测量进程继续使用原有 Node 选项。未设置时使用 Node 默认堆上限；实际编译参数和峰值另记入报告。

```bash
ZRCONFIG_BENCH_TIME=1 ZRCONFIG_BENCH_TIMEOUT_SECONDS=3600 bash scripts/dotnet.sh tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll --benchmark-table-scale artifacts/performance/table-scale 10000 3
```

表数支持 100、1000、10000；最后一个参数为每种语言、格式和加载配置的独立进程重复次数。输入包含 100 行主表、100 行宽表、64 行引用环及其余单行小表。先完整生成、双格式导出和校验，再重复生成并核对文件清单、内容、时间戳均不变，随后编译和运行七语言消费方。全量模式额外核对所有表的已加载行总数；报告保存源码、API 基线与编译产物体积。大目录的编译时间和内存可能显著高于运行时加载，应为其预留资源。

Go 性能消费方使用临时 `zrconfig` 模块，报告保存模块声明。这样包名不会随系统临时目录的长度变化，也不会把主机路径重复写入所有泛型符号；正式生成物仍按业务项目的模块约定集成。编译器目标文件、最终可执行文件与加载后的内存分别统计，不能相互替代。

`ZRCONFIG_BENCH_TIMEOUT_SECONDS` 将每个编译或运行子进程的超时从默认 300 秒调整到 1–3600 秒，并写入报告；超时仍是失败，不记为通过。排查失败时可用 `ZRCONFIG_BENCH_KEEP_SCALE=1` 保留该次系统临时工作区。`ZRCONFIG_BENCH_TABLE_WORKBOOK` 可复用该入口已生成的同规模 `TinyTables.xlsx`；输入摘要和全量行数断言仍执行。成功报告与失败记录分别为 `server-runtime.json`、`failure.json`，不同尝试使用不同报告目录，避免覆盖证据。

插桩只写临时副本，正式 Runtime、工作簿和业务生成物不改写。九阶段是含子调用时间；锁采样是管理器状态锁获取成本，包含无竞争加锁和计时成本。首次初始化按语言记录目录访问或模块导入；图性能只测 Catalog/SCC/闭包。取消统计使用第二次源读取作为确定触发点，不代表任意 CPU 热点的最坏响应。口径与边界统一见 [性能测量说明](../docs/server-generation.md#可重复的性能测量)。
