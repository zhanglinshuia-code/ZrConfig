# 调试指南：Rider、Visual Studio 与 Unity

**中文** | [English](debugging.en.md)

ZrConfig 的可编程后置检查用于在导出阶段发现配表业务错误，检查规则本身也支持 **Visual Studio / Rider 断点调试**。可以在逐行、整表、跨表或字段检查中暂停，观察本次 Excel 导入的值、嵌套对象与关联记录，再通过单步和调用栈确认判断逻辑；验证这些数据约束时无需先进入游戏玩法。

先能重复生成、导出，再调试同一份输入。业务规则的 API 与执行边界见 [规则开发与后置检查](rules-development.md)，源码构建见 [源码构建与升级](development.md)，Excel 常见错误见 [使用教程](user-guide.md#8-按错误找到修改位置)。本文 Visual Studio 指 Windows 桌面版；macOS 使用 Rider。Visual Studio Code 的 launch.json 不属于本文步骤。

**CLI（Command-Line Interface，命令行界面）** 在本文指 `ZrConfig.Cli` 命令行工具：通过命令参数启动生成、只校验或导出，不打开配表窗口；**App** 指带按钮的桌面工具。规则调试通常从 CLI 启动，因为它提供 `--rules-debug` 参数。命令组成和工具入口说明见 [命令行使用](project-development.md#13-命令行使用)。

## 1. 先选择正确进程

| 调试目标 | 启动/附加目标 | 适合的断点 |
| --- | --- | --- |
| Excel 结构、类型与生成代码 | ZrConfig.Cli，使用 `codegen` 命令 | ProjectWorkflow、ModelGenerator.Prepare、ModelGenerationPlan.Apply、SchemaLoader.Load |
| 字段规则、逐行/整表/跨表后置检查 | ZrConfig.Cli，参数带 `--rules-debug` | 规则 Validate / ValidateTable / IFieldRule.Validate |
| 导出、日志、发布切换 | ZrConfig.Cli | ExportPipeline.RunWithSources / Publish |
| 桌面按钮、目录、语言、日志显示 | ZrConfig.App | MainWindow.RunOperationAsync / FlushEvents / RefreshProjectState |
| 游戏读取、对象引用、热更新 | Unity Editor 或目标 Player | 游戏调用 LoadAsync / ReloadAsync 的位置、资源适配器 |

规则即使写在 Unity 项目里，也由导表工具进程执行。附加 Unity Editor 不会命中 CLI 正在执行的规则。工具内成功导出也不表示 Unity 已读取新数据。

两层 Debug 必须区分：

- 工程的 Debug 配置控制 CLI/Core 等工具源码的优化与符号。
- `--rules-debug` 控制运行时动态编译的 Config + Rules：关闭优化、定义 DEBUG、加载 Portable PDB；默认动态规则为 Release。

App 没有规则调试开关；给 App 命令行追加这个参数不会启用该模式。调试业务规则使用 CLI。生成阶段代码根目录中手写源码与支持源码的编译使用独立流程，不受 CLI validate/export 的该参数控制。

## 2. 共同准备

1. 保存 Excel 和源码，记录实际 Data、Output、Config、Rules 的绝对路径。
2. 结构变化时先生成。确认 Config/Generated 包含当前模型，规则代码能编译。
3. 只调试业务规则时优先使用 `validate --project <项目文件> --rules-debug`，不发布数据；排查导出与切换时使用临时 Output。错误试验的数据副本放系统临时目录或仓库外，不在正式 Data 混入副本。
4. 断点放在可执行语句，例如 if、context.Error 调用；不要只放在标签、方法声明或空大括号上。
5. 先选一条明确能进入规则的有效记录。解析不完整会让后置方法整体跳过。

macOS 从仓库根目录运行：

```bash
bash scripts/dotnet.sh build src/ZrConfig.Cli -c Debug -p:NuGetAudit=false
# 仅在结构变化或首次初始化时生成
bash scripts/dotnet.sh src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll codegen samples/Data unity/ZrConfig.Sample/Assets/HotUpdate/Config
```

Windows PowerShell 对应命令（先安装 .NET SDK 10）：

```powershell
dotnet build src/ZrConfig.Cli -c Debug -p:NuGetAudit=false
dotnet src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll codegen samples/Data unity/ZrConfig.Sample/Assets/HotUpdate/Config
```

`NuGetAudit=false` 只关闭本次构建的漏洞元数据查询，不替代包还原，也不改变配表校验。网络或包还原失败时先处理构建错误。

### IDE 诊断与真实编译

先区分三个来源：Rider / Visual Studio 编辑器里的红线、.NET 构建输出、Unity Console。仓库根目录的 `ZrConfig.slnx` 编译导表工具；Unity 生成的解决方案使用 Unity 的引用与编译符号。打开其中一个不代表已经验证另一个。

1. 先执行 [源码构建](development.md#环境与构建)，再看 Unity Console 的第一条编译错误。记录错误代号、文件和行号，先修首个根因，后续“找不到类型”可能只是连带报错。
2. 如果两者均成功，只有 IDE 报红，重新加载解决方案；Unity 工程在 External Tools 中重新生成项目文件，并从 Unity 打开脚本。确认 IDE 使用正确 SDK 和工程，仍异常时再重启 IDE 让索引重新加载。
3. Runtime / Validation 已统一迁入 `Assets/ZrConfig`。旧目录或旧 DLL 出现在 IDE 引用里时检查工程是否过期，保留唯一源码，不复制旧文件回来消除红线。
4. 清理后出现 NETSDK1004 时重新执行带还原的 build，不使用 `--no-restore`。导表时报 `CONFIG_*` 或规则编译错误时检查工具实际选择的 Config/Rules 路径；Unity 中能编译不等于动态规则可依赖 Unity API。

重启 IDE 可以恢复陈旧索引，但不能代替编译验证。不要为了清除红线删除手写代码、正式 Excel 或有效导出版本。

## 3. Rider：从 IDE 启动调试

短命令优先这样操作，避免进程在附加前已结束。

1. 打开仓库 `ZrConfig.slnx`，确认 IDE 能使用 .NET SDK 10。单独改规则时也可打开项目自己的 Unity solution，并使用下面的外部可执行文件方式。
2. 在 **Run → Edit Configurations** 创建/选择 **.NET Project**，项目选择 `ZrConfig.Cli`，使用 Debug 构建。
3. Working directory 设置为仓库根目录，Program arguments 填以下内容。参数从 export 开始，不要把 dotnet、run 或 DLL 路径填到项目参数里。

```text
export "samples/Data" "samples/Output" "unity/ZrConfig.Sample/Assets/HotUpdate/Config" "unity/ZrConfig.Sample/Assets/Editor/ConfigRules" --rules-debug
```

4. 需要隔离输出时，把第二个路径替换为已确定的临时输出路径。打开 `unity/ZrConfig.Sample/Assets/Editor/ConfigRules/AllTypesRules.cs`，在 Validate 的 `if (row.Settings.MaxTargets <= 0)` 行设断点。
5. 点击 **Debug**。初次动态程序集尚未加载时断点可能暂未绑定，加载之后再观察是否命中。
6. 命中后查看 row.Id、row.Settings、row.Actions，以及 `context.IsEmpty(row, "Settings.MaxTargets")`。条件断点可用 `row.Id == 1` 缩小范围。
7. Step Over 看业务判断，Step Into 查看 context.Error 如何映射位置；Continue 完成导出，核对最终诊断。

没有工具 solution 时，可在 .NET Executable 配置中选实际 dotnet 可执行文件，把完整 CLI DLL 路径放在参数第一项，后面再接 export 与四个绝对目录；或选择构建生成的 CLI apphost。两种方式都要使用实际构建产物，不要指向规则库 DLL。配置字段说明见 [Rider 官方 .NET 项目文档](https://www.jetbrains.com/help/rider/Run_Debug_Configuration_dotNet_Project.html)。

## 4. Visual Studio：从 IDE 启动调试

使用支持本仓库 .NET 10 和 `.slnx` 的 Visual Studio 版本；旧 IDE 不能识别时先升级，或打开 CLI 的 csproj 排查 SDK 支持，不要为此在仓库再维护一套 solution。

1. 打开 `ZrConfig.slnx`，在解决方案资源管理器中将 **ZrConfig.Cli** 设置为启动项目。
2. 选择 **Debug** 配置。打开 CLI 项目属性的 **Debug / 调试 → Open debug launch profiles UI / 打开调试启动配置文件 UI**。
3. Command-line arguments 填下面内容；Working directory 设为本机仓库根目录，例如 `D:\Work\ZrConfig`。不添加 `dotnet run --`。

```text
export "samples/Data" "samples/Output" "unity/ZrConfig.Sample/Assets/HotUpdate/Config" "unity/ZrConfig.Sample/Assets/Editor/ConfigRules" --rules-debug
```

4. 直接打开 `unity/ZrConfig.Sample/Assets/Editor/ConfigRules/AllTypesRules.cs`，给 Validate 第一条 if 设断点。
5. 按 **F5 / 开始调试**，而不是“不调试启动”。命中后使用 Locals、Watch、Call Stack 查看数据、原始空白标记和调用链。
6. 需要只看某条记录，在断点设置中增加条件 `row.Id == 1`。F10 单步跳过，F11 单步进入，F5 继续；快捷键可能受个人键位设置影响。
7. 结束后查看控制台和 Output/Logs；每次修改规则源码后重新执行导出，让工具重新编译。不要依赖 IDE Edit and Continue 更新已加载的动态程序集。

IDE 会管理项目的调试启动配置；本次文档不为每位开发者提交绝对机器路径。若需要共享配置，应先按项目目录约定统一安排，并确保路径可移植。启动配置字段参考 [Microsoft 官方 .NET 调试配置](https://learn.microsoft.com/en-us/visualstudio/debugger/project-settings-for-csharp-debug-configurations-dotnetcore)。

### Visual Studio 附加现有进程

终端先启动 CLI，例如：

```powershell
dotnet src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll export samples/Data samples/Output unity/ZrConfig.Sample/Assets/HotUpdate/Config unity/ZrConfig.Sample/Assets/Editor/ConfigRules --rules-debug
```

在 **Debug → Attach to Process / 调试 → 附加到进程** 中按进程名、PID、命令行确认正确的 `ZrConfig.Cli.exe` 或承载该 DLL 的 `dotnet.exe`，使用适用于 .NET/CoreCLR 的托管调试器。机器上有多个 dotnet 时，不能只凭名字选择。附加操作不会补传 `--rules-debug`，必须在启动命令中提供。

CLI 不会等待附加。若导出太快，回到本节 F5 启动方式；只有外部发布工具可用时，配置 IDE 启动该可执行文件及参数，再调试。UI 入口随 IDE 版本可能变化，参考 [Microsoft 官方附加进程文档](https://learn.microsoft.com/en-us/visualstudio/debugger/attach-to-running-processes-with-the-visual-studio-debugger)。

Rider 的 **Run → Attach to Process** 同样选择实际 CLI/apphost 或 dotnet 承载进程；参见 [Rider 官方附加文档](https://www.jetbrains.com/help/rider/attach-to-process.html)。不建议为了抢附加时机把永久等待循环写进业务规则。

## 5. 断点不命中的排查顺序

先区分安装版和源码构建版：macOS 安装版通常运行自 `/Applications/ZrConfig.app`，IDE 则启动工程的 bin 输出。窗口版本相同也可能来自不同构建，附加时同时核对完整进程路径、PID 和已加载模块的符号；修改源码后不会自动更新已安装的 App。

| 现象 | 检查与处理 |
| --- | --- |
| 程序很快结束 | 从 IDE Debug/F5 启动，先在 CLI 入口设断点核对 args |
| CLI 入口也不命中 | 启动项目、构建配置、实际二进制路径是否正确，是否误点 Run |
| 规则断点空心/未绑定 | 动态程序集是否已加载、是否带 --rules-debug、源码路径是否与编译时一致 |
| 代码相同仍提示源码不匹配 | 是否打开了副本/旧 SDK 中的文件；保存后结束本轮，再从原路径重新导出 |
| 只改工具为 Debug 仍难单步 | 工具 Debug 不等于动态规则 Debug，核对参数和 export.log 的编译模式 |
| 规则完全没执行 | Rules 是否留空，目录是否正确，类是否可实例化并绑定当前表 |
| 字段自定义规则不执行 | IFieldRule 是否被 Check 绑定；空白字段默认跳过 Check |
| ValidateTable 执行但 Validate 不执行 | 表可能是零行；逐行方法无记录可处理 |
| 后置方法全部跳过 | 查 BUSINESS_CHECK_SKIPPED、CELL_PARSE、SCHEMA_MISMATCH、TABLE_REFERENCE 和依赖表 |
| Unity 中能编译但 App 找不到类 | 是否用 UNITY_EDITOR 包住整个规则；App 不定义该符号 |
| 规则运行但断点所在分支不进 | 看实际值、IsEmpty、数组长度与排序；把断点移到方法第一条语句 |
| 命中了另一份程序集 | 按 PID 和完整路径确认实际 CLI；不要附加构建服务器或 Unity Editor |

源码调试时，可先在 `ValidationEngine.Validate` 的 `plan.Rules.ValidateRow(row, context)` 处停住，检查 Rules 的实际类型、Table.IsComplete、Dependencies，然后单步进入。这样能区分“未发现规则”“被跳过”和“已运行但条件不成立”。

动态程序集名称形如 `ZrConfig.DynamicRules_<指纹>`，从内存加载 DLL 和 PDB，可能没有普通磁盘模块路径。Visual Studio 的 Modules / 模块窗口可看加载与符号状态；需要单步动态/外部代码时，检查 **Enable Just My Code / 启用仅我的代码** 是否限制了进入。Rider 同样检查外部代码单步过滤。不要通过关闭源码匹配检查掩盖打开了错误源码的问题。

缓存命中仍加载符号。指纹包含源码绝对路径及内容，移动或编辑文件会改变缓存键。先修路径、编译与模式；只有怀疑损坏时，停止导出后清理该 Output 下的 `.rules-cache`，下一次自动重编。不要删除有效版本数据来解决断点问题。

## 6. 捕获异常与定位 Excel

后置检查会捕获异常并生成诊断，因此“仅遇到未处理异常时暂停”可能不会停在原始 throw。Rider 使用异常断点，Visual Studio 使用 **Exception Settings / 异常设置**，临时选择目标异常类型在抛出时中断。先关注 NullReferenceException、ArgumentException 或日志里的具体类型，定位后恢复设置，避免所有内部异常都中断。

每次失败保留两份文件：

- `Output/Logs/export.log`：过程、编译模式、规则目录、缓存信息、完整异常。
- `Output/Logs/diagnostics.json`：本次 runId、success、versionId、language 与 diagnostics。

可以用下面命令读取完整 JSON：

```bash
bash scripts/maintain.sh format-json samples/Output/Logs/diagnostics.json
```

Windows 可用 `powershell -File scripts/maintain.ps1 format-json samples/Output/Logs/diagnostics.json`。也可直接在 Visual Studio、Rider 或文本编辑器中打开 JSON；查看日志不需要安装额外脚本语言。

| 字段 | 解释 |
| --- | --- |
| Code | 稳定代号，例如 CELL_PARSE、RULE_BINDING、VALIDATOR_EXCEPTION |
| File / Sheet / Table | 原工作簿路径、工作表、逻辑表名；编译错误 File 是 .cs |
| Row / Column | 从 1 开始的物理行列；0 表示未定位到某一格 |
| Field | 逻辑字段或具体元素路径，例如 Actions[1].Delay |
| Severity | 数值枚举：0 Info、1 Warning、2 Error、3 Success；只有 Error 阻止发布 |
| SeverityText / Message | 当前日志语言的显示文字 |

报错行不是数据列表下标。按本文档的五行表头填写时，数据从 Excel 第 6 行开始；定位错误时始终以本次诊断报告的实际行列为准。数组按 $order 排序以后，下标仍会映射回原始物理位置。

生成代码的错误主要看 App 本次生成日志或 CLI codegen 控制台；生成不会更新导出的 diagnostics.json。输出目录建立失败、export.lock 被占用等早期错误也可能发生在报告写入前，不能把旧报告当成本次结果。先看控制台异常和文件时间/runId。

上表描述的是导出写入的 diagnostics.json。App“导出诊断报告”及 CLI `--report` 使用独立报告格式：包含 `success`、`cancelled`、`needsGeneration`、`codeCommitted`、`published` 等结果字段，诊断 `severity` 为字符串并替换已知项目根路径。只校验时 `published` 为 false；`needsGeneration` 为 true 表示完整校验尚未完成。复制报告前核对操作类型与运行编号。

## 7. 工具源码的断点路线

输入递归覆盖根目录中的 `.xlsx` 和 `.xls`。先核对日志中的完整文件路径，再找 Sheet 和行列：跨目录重名报告 TABLE_DUPLICATE，子目录扫描失败报告 INPUT_SCAN，损坏或格式不符报告 EXCEL_OPEN。发现、格式选择与文本转换断点设在 ExcelWorkbookInput；解析断点仍在 SchemaLoader/ExcelDataLoader。测试副本应放在独立输入树，不能留在已选根目录的子文件夹中。

| 阶段 | 文件/方法 | 观察值 |
| --- | --- | --- |
| 命令行参数 | ToolCommandRunner.Run / ToolCommandLine.Parse | args、项目配置、最终目录、RulesDebug |
| 类型发现/代码生成 | ModelGenerator.Prepare、ModelGenerationPlan.Apply、RuleCompiler.PrepareTypes | Config 根目录、类型编译诊断、schemas；确认后的写入在 Apply |
| 表头/主键 | SchemaLoader、TableDefinitions、ExcelColumnLayout | 表名、DataStartRow、Fields、主键、SchemaHash |
| 单元格/多行 | ExcelDataLoader、ExcelStructuredDataLoader、ConfigValueParser | 原始文本、IsComplete、RowSource、排序后的路径 |
| 动态编译 | RuleCompiler.PrepareCore | sources 的绝对路径、debug、fingerprint、emitted.Diagnostics |
| 标签/后置 | ValidationEngine.Validate / Bind | plans、Checks、Dependencies、complete、当前 row |
| 发布 | ExportPipeline.Publish | staging、versionId、manifestTmp、切换前的 current.json |
| 运行时 | ConfigManager.ReloadAsync | 清单、资源相对路径、SchemaHash、candidate、Current |

调试 App 时从 IDE 启动 ZrConfig.App，给按钮回调设断点，然后在窗口点击按钮。生成/导出在 Task.Run 中执行；应 await 任务并观察原始异常，不要只看 UI 的最后一句状态。

### 7.1 目录恢复与语言偏好

`Environment.SpecialFolder.LocalApplicationData` 下的 `ZrConfig/workspace.json` 保存四个目录，`ZrConfig/settings.json` 保存语言；实际系统根路径由运行环境决定。WorkspaceSettings 保存时把非空路径转换为绝对路径，避免从不同工作目录启动后指向别处。

排查选择了旧项目时先看界面的四个路径。需要检查偏好文件时先关闭 App 并备份，再处理对应文件；设置不存在或损坏会回退默认值。ToolPaths 优先从可执行文件位置、再从工作目录向上查找仓库：识别到仓库使用 samples；独立 macOS .app 未识别到仓库时使用用户数据目录下的 ZrConfig/Workspace，其他独立进程使用工作目录。有效的已保存设置优先。

项目设置直接使用保存的路径；搬动项目后检查目录配置，不自动重定向历史目录。相关实现位于 WorkspaceSettings 和 ToolPaths。

定位发布错误时保留 current.json 内容，确认在最后 File.Move 之前失败仍保留旧清单；版本目录存在不等于它已经激活。不要手工改清单里的哈希来让错误数据通过检查。

## 8. Unity 游戏端调试

Rider 附加正在使用的 Unity Editor/Player；Visual Studio 使用安装好的 **Visual Studio Tools for Unity → Attach to Unity / 附加到 Unity**。检查代码与 Editor 工程一致，再进入 Play 并触发游戏自己的加载入口。Visual Studio 的步骤参考 [官方 Unity 调试文档](https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/get-started/using-visual-studio-tools-for-unity)。

示例源码直接位于 `Assets/ZrConfig/Runtime`、`Assets/ZrConfig/Unity`；生成表与 `TableCodecs` 位于 `Assets/HotUpdate/Config/Generated`。从 Unity 双击脚本打开实际源码，IDE 工程未更新时在 Unity 的 External Tools 中重新生成项目文件。App 不再显示虚拟 Source 分组。

首先在游戏调用处停住：

```csharp
// 放在项目已有的异步初始化方法中；configOutputFolder 是实际数据根目录。
var source = new ZrConfig.Runtime.FileConfigSource(configOutputFolder);
var snapshot = await ZrConfig.Unity.UnityConfigRuntime.LoadAsync(
    ZrConfig.Models.TableCodecs.Registry, source);
UnityEngine.Debug.Log(snapshot.VersionId);
```

第一次成功后用 Reload / ReloadAsync。已有同步源时也可首次调用 `Load(TableCodecs.Registry, source)`；已经调用 Initialize 的项目使用 `Reload(source)` / `ReloadAsync(source)`，不要再首次 Load。异步返回自带的 ConfigOperation，无需 Task；同步重载在已有加载期间会明确报错。Unity API 从主线程调用；观察 await 的异常，不要用未观察的后台任务吞掉失败。

按以下顺序检查热更新：

1. `IConfigSource.ReadAsync` 是否读取正确资源根目录，第一次路径应为 `current.json`，随后为 `versions/<versionId>/<file>`。
2. 核对所需表是否在清单中、schemaHash 是否与当前生成 Codec 一致。默认懒加载在访问时检查目标表；要求整个目录一致时传入 TableCodecs.Registry 并执行 ReloadAll / ReloadAllAsync。
3. sha256 不符先检查缺文件、截断、上传/缓存混用；Schema 不符先同步生成源码与数据。两者处理方式不同。
4. 断点放在 ReloadAsync 的 await 前后，比较 `oldSnapshot` 和 `UnityConfigRuntime.Current`。清单失败/取消应保持同一旧快照；首次 LoadAsync 失败后可重试。默认初始化只读取清单，数据错误在 GetRowAsync/GetTableAsync 等实际访问时检查，本批失败不发布行缓存。
5. 成功后重新从 Current 取表；旧 row 的普通字段不会原地变成新数据；旧快照查询和旧行引用访问（包括缓存命中）应报配置已失效。
6. 在 Changed 回调中检查 UI 刷新。监听器异常会记入 Unity Console，但不会回滚已经成功的快照；不要因为 UI 报错再次调用首次初始化。
7. `Tools → ZrConfig → 运行时配置表` 查看的是游戏当前已加载快照，不直接读取 Excel；只按行加载时也可能无完整表，这是正常状态；查看 LoadedRowCount，或显式加载对应整表。

Android/iOS/WebGL 的示例使用内置 `StreamingAssetsConfigSource` 和 `CoalescingConfigSource`；在 Unity 主线程检查 UnityWebRequest、相对路径与取消，不将 APK 内路径或 URL 当作桌面文件。具体接入见 [StreamingAssets 适配器](project-development.md#145-androidioswebgl-与-streamingassets)。具体 Player 调试、IL2CPP 符号与代码热更新框架按项目平台验证，CLI 的 PDB 不覆盖这些环境。

## 9. 一次可复查的调试验收

- 合法数据先导出成功，记录版本和日志。
- 在临时数据副本制造一个规则错误，命中对应方法并确认值与空白标记。
- 退出码为 1、success 为 false，诊断位置正确，旧 current.json 未切换。
- 修复后导出成功，Unity 加载新 versionId；旧引用仍保持旧值，新查询得到新值。
- 如排查运行时损坏，在临时 Output 破坏 bytes：默认重载后在访问该表时应报错；使用 ReloadAll 或包含该表的 ReloadPrepared 时应在切换前失败并保留旧快照。不要损坏正式发布数据。
- 分享问题时附工具版本、平台/IDE、实际命令与目录、两份日志、期望/实际结果、最小复现源码和脱敏数据；固定日志会被下一次导出更新，独立记录保留在 `Output/Logs/<运行编号>/`；也可主动导出诊断报告。

文档依据当前源码、仓库自动化测试与上述官方 IDE 文档整理。本仓库的自动化检查可验证动态 Debug/PDB、源码调用栈与失败保护；不能代替本机 Rider/Visual Studio 的交互断点验收，目标 Player/IL2CPP 的已有记录和未测范围见 [平台验收](release-and-acceptance.md#验收与团队试用记录)。
