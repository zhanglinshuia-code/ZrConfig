# 调试指南：Rider、Visual Studio 与 Unity

**Debugging: Rider, Visual Studio, and Unity**

ZrConfig 的可编程后置检查用于在导出阶段发现配表业务错误，检查规则本身也支持 **Visual Studio / Rider 断点调试**。可以在逐行、整表、跨表或字段检查中暂停，观察本次 Excel 导入的值、嵌套对象与关联记录，再通过单步和调用栈确认判断逻辑；验证这些数据约束时无需先进入游戏玩法。

Programmable post-validation catches business errors during export and supports **Visual Studio / Rider breakpoints**. Pause in row, table, cross-table, or field checks to inspect imported values, nested objects, and related records, then step through logic and call stacks. Gameplay need not run to validate these constraints.

先能重复生成、导出，再调试同一份输入。业务规则的 API 与执行边界见 [规则开发与后置检查](rules-development.md)，源码构建见 [源码构建与升级](development.md)，Excel 常见错误见 [使用教程](user-guide.md#8-按错误找到修改位置)。本文 Visual Studio 指 Windows 桌面版；macOS 使用 Rider。Visual Studio Code 的 launch.json 不属于本文步骤。

Establish repeatable generation/export before debugging the same input. See [rule contracts](rules-development.md), [building](development.md), and [common Excel errors](user-guide.md#8-按错误找到修改位置). Visual Studio here means the Windows desktop IDE; use Rider on macOS. VS Code launch.json is outside these steps.

**CLI（Command-Line Interface，命令行界面）** 在本文指 `ZrConfig.Cli` 命令行工具：通过命令参数启动生成、只校验或导出，不打开配表窗口；**App** 指带按钮的桌面工具。规则调试通常从 CLI 启动，因为它提供 `--rules-debug` 参数。命令组成和工具入口说明见 [命令行使用](project-development.md#13-命令行使用)。

**CLI (Command-Line Interface)** means `ZrConfig.Cli`, which generates, validates, or exports through arguments without opening the desktop window. **App** means the desktop UI. Debug rules through CLI because it provides `--rules-debug`. See [command-line usage](project-development.md#13-命令行使用) for command structure and entry points.

## 1. 先选择正确进程

**1. Choose the right process**

| 调试目标<br>Target | 启动/附加目标<br>Launch/attach | 适合的断点<br>Useful breakpoints |
| --- | --- | --- |
| Excel 结构、类型与生成代码<br>Excel schema, types, generated code | ZrConfig.Cli，使用 `codegen` 命令<br>ZrConfig.Cli with `codegen` | ProjectWorkflow、ModelGenerator.Prepare、ModelGenerationPlan.Apply、SchemaLoader.Load<br>ProjectWorkflow, ModelGenerator.Prepare, ModelGenerationPlan.Apply, SchemaLoader.Load |
| 字段规则、逐行/整表/跨表后置检查<br>Field, row, table, cross-table checks | ZrConfig.Cli，参数带 `--rules-debug`<br>ZrConfig.Cli with `--rules-debug` | 规则 Validate / ValidateTable / IFieldRule.Validate<br>Validate / ValidateTable / IFieldRule.Validate |
| 导出、日志、发布切换<br>Export, logging, publication switch | ZrConfig.Cli | ExportPipeline.RunWithSources / Publish |
| 桌面按钮、目录、语言、日志显示<br>Desktop buttons, paths, language, logs | ZrConfig.App | MainWindow.RunOperationAsync / FlushEvents / RefreshProjectState |
| 游戏读取、对象引用、热更新<br>Game loading, references, hot updates | Unity Editor 或目标 Player<br>Unity Editor or target Player | 游戏调用 LoadAsync / ReloadAsync 的位置、资源适配器<br>Calls to LoadAsync / ReloadAsync and resource adapters |

规则即使写在 Unity 项目里，也由导表工具进程执行。附加 Unity Editor 不会命中 CLI 正在执行的规则。工具内成功导出也不表示 Unity 已读取新数据。

Even rules stored in Unity execute in the export-tool process. Attaching to Unity will not hit rules executing in CLI. A successful tool export also does not mean Unity has read the new data.

两层 Debug 必须区分：

Distinguish two Debug settings:

- 工程的 Debug 配置控制 CLI/Core 等工具源码的优化与符号。<br>
  Project Debug configuration controls optimization/symbols for CLI/Core tool source.
- `--rules-debug` 控制运行时动态编译的 Config + Rules：关闭优化、定义 DEBUG、加载 Portable PDB；默认动态规则为 Release。<br>
  `--rules-debug` controls dynamically compiled Config + Rules: disables optimization, defines DEBUG, and loads Portable PDB. Dynamic rules default to Release.

App 没有规则调试开关；给 App 命令行追加这个参数不会启用该模式。调试业务规则使用 CLI。生成阶段代码根目录中手写源码与支持源码的编译使用独立流程，不受 CLI validate/export 的该参数控制。

The App has no rule-debug switch; appending this argument does not enable it. Use CLI for business-rule debugging. Handwritten/support type compilation during generation follows a separate process, unaffected by validate/export's switch.

## 2. 共同准备

**2. Shared preparation**

1. 保存 Excel 和源码，记录实际 Data、Output、Config、Rules 的绝对路径。<br>
   Save Excel/source and record the actual absolute Data, Output, Config, and Rules paths.
2. 结构变化时先生成。确认 Config/Generated 包含当前模型，规则代码能编译。<br>
   Generate after schema changes. Confirm Config/Generated contains current models and rules compile.
3. 只调试业务规则时优先使用 `validate --project <项目文件> --rules-debug`，不发布数据；排查导出与切换时使用临时 Output。错误试验的数据副本放系统临时目录或仓库外，不在正式 Data 混入副本。<br>
   Prefer `validate --project <project-file> --rules-debug` for rules alone, without publishing. Use temporary Output for export/switch debugging. Keep intentionally faulty copies in system temp or outside the repository, never inside official Data.
4. 断点放在可执行语句，例如 if、context.Error 调用；不要只放在标签、方法声明或空大括号上。<br>
   Break on executable statements such as if or context.Error, not only attributes, declarations, or empty braces.
5. 先选一条明确能进入规则的有效记录。解析不完整会让后置方法整体跳过。<br>
   Start with a valid record known to enter the rule. Incomplete parsing can skip all post-validation methods.

macOS 从仓库根目录运行：

On macOS, run from the repository root:

```bash
bash scripts/dotnet.sh build src/ZrConfig.Cli -c Debug -p:NuGetAudit=false
# 仅在结构变化或首次初始化时生成
bash scripts/dotnet.sh src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll codegen samples/Data unity/ZrConfig.Sample/Assets/HotUpdate/Config
```

**示例英文说明 / English notes:**

- Generate only after schema changes or on first initialization


Windows PowerShell 对应命令（先安装 .NET SDK 10）：

Equivalent Windows PowerShell commands (install .NET SDK 10 first):

```powershell
dotnet build src/ZrConfig.Cli -c Debug -p:NuGetAudit=false
dotnet src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll codegen samples/Data unity/ZrConfig.Sample/Assets/HotUpdate/Config
```

`NuGetAudit=false` 只关闭本次构建的漏洞元数据查询，不替代包还原，也不改变配表校验。网络或包还原失败时先处理构建错误。

`NuGetAudit=false` only disables vulnerability-metadata queries for this build; it neither replaces restore nor changes data validation. Resolve network/restore build failures first.

### IDE 诊断与真实编译

**IDE diagnostics versus actual compilation**

先区分三个来源：Rider / Visual Studio 编辑器里的红线、.NET 构建输出、Unity Console。仓库根目录的 `ZrConfig.slnx` 编译导表工具；Unity 生成的解决方案使用 Unity 的引用与编译符号。打开其中一个不代表已经验证另一个。

Distinguish IDE red underlines, .NET build output, and Unity Console. Root `ZrConfig.slnx` builds the export tool; Unity's generated solution uses Unity references/symbols. Opening one does not validate the other.

1. 先执行 [源码构建](development.md#环境与构建)，再看 Unity Console 的第一条编译错误。记录错误代号、文件和行号，先修首个根因，后续“找不到类型”可能只是连带报错。<br>
   Run the [source build](development.md#环境与构建), then inspect Unity Console's first compile error. Record code/file/line and fix the root cause; later missing-type errors may be secondary.
2. 如果两者均成功，只有 IDE 报红，重新加载解决方案；Unity 工程在 External Tools 中重新生成项目文件，并从 Unity 打开脚本。确认 IDE 使用正确 SDK 和工程，仍异常时再重启 IDE 让索引重新加载。<br>
   If both compile and only the IDE complains, reload the solution. Regenerate Unity project files under External Tools and open scripts from Unity. Check SDK/project selection; if needed, restart the IDE to rebuild indexes.
3. Runtime / Validation 已统一迁入 `Assets/ZrConfig`。旧目录或旧 DLL 出现在 IDE 引用里时检查工程是否过期，保留唯一源码，不复制旧文件回来消除红线。<br>
   Runtime/Validation now live only under `Assets/ZrConfig`. Old paths/DLLs in IDE references suggest stale projects. Preserve the single source set; do not copy old files back to remove red underlines.
4. 清理后出现 NETSDK1004 时重新执行带还原的 build，不使用 `--no-restore`。导表时报 `CONFIG_*` 或规则编译错误时检查工具实际选择的 Config/Rules 路径；Unity 中能编译不等于动态规则可依赖 Unity API。<br>
   After cleanup, fix NETSDK1004 with a restoring build, without `--no-restore`. For `CONFIG_*` or rule compilation errors, check selected Config/Rules paths. Unity compilation success does not permit dynamic rules to depend on Unity APIs.

重启 IDE 可以恢复陈旧索引，但不能代替编译验证。不要为了清除红线删除手写代码、正式 Excel 或有效导出版本。

Restarting can fix stale indexes but cannot replace compilation checks. Never delete handwritten code, official workbooks, or valid exports just to remove IDE errors.

## 3. Rider：从 IDE 启动调试

**3. Rider: launch debugging from the IDE**

短命令优先这样操作，避免进程在附加前已结束。

Prefer this for short commands that might exit before attachment.

1. 打开仓库 `ZrConfig.slnx`，确认 IDE 能使用 .NET SDK 10。单独改规则时也可打开项目自己的 Unity solution，并使用下面的外部可执行文件方式。<br>
   Open `ZrConfig.slnx` with .NET SDK 10 available. For rule-only edits, you may use your Unity solution and the external-executable approach below.
2. 在 **Run → Edit Configurations** 创建/选择 **.NET Project**，项目选择 `ZrConfig.Cli`，使用 Debug 构建。<br>
   In **Run → Edit Configurations**, create/select **.NET Project**, choose `ZrConfig.Cli`, and build Debug.
3. Working directory 设置为仓库根目录，Program arguments 填以下内容。参数从 export 开始，不要把 dotnet、run 或 DLL 路径填到项目参数里。<br>
   Set Working directory to the repository root and Program arguments as below. Arguments start at export; do not include dotnet, run, or the DLL path.

```text
export "samples/Data" "samples/Output" "unity/ZrConfig.Sample/Assets/HotUpdate/Config" "unity/ZrConfig.Sample/Assets/Editor/ConfigRules" --rules-debug
```

4. 需要隔离输出时，把第二个路径替换为已确定的临时输出路径。打开 `unity/ZrConfig.Sample/Assets/Editor/ConfigRules/AllTypesRules.cs`，在 Validate 的 `if (row.Settings.MaxTargets <= 0)` 行设断点。<br>
   For isolated output, replace the second path with your chosen temporary output path. Open `unity/ZrConfig.Sample/Assets/Editor/ConfigRules/AllTypesRules.cs` and break on `if (row.Settings.MaxTargets <= 0)` in Validate.
5. 点击 **Debug**。初次动态程序集尚未加载时断点可能暂未绑定，加载之后再观察是否命中。<br>
   Click **Debug**. A breakpoint may remain unbound until the dynamic assembly loads.
6. 命中后查看 row.Id、row.Settings、row.Actions，以及 `context.IsEmpty(row, "Settings.MaxTargets")`。条件断点可用 `row.Id == 1` 缩小范围。<br>
   Inspect row.Id, row.Settings, row.Actions, and `context.IsEmpty(row, "Settings.MaxTargets")`. Use `row.Id == 1` as a conditional breakpoint to narrow the case.
7. Step Over 看业务判断，Step Into 查看 context.Error 如何映射位置；Continue 完成导出，核对最终诊断。<br>
   Step Over through business logic, Step Into context.Error to inspect location mapping, then Continue and check final diagnostics.

没有工具 solution 时，可在 .NET Executable 配置中选实际 dotnet 可执行文件，把完整 CLI DLL 路径放在参数第一项，后面再接 export 与四个绝对目录；或选择构建生成的 CLI apphost。两种方式都要使用实际构建产物，不要指向规则库 DLL。配置字段说明见 [Rider 官方 .NET 项目文档](https://www.jetbrains.com/help/rider/Run_Debug_Configuration_dotNet_Project.html)。

Without the tool solution, use .NET Executable with the actual dotnet executable, the full CLI DLL path as its first argument, followed by export and four absolute directories; alternatively select the built CLI apphost. Use real build output, not a rules-library DLL. See [Rider .NET project configuration](https://www.jetbrains.com/help/rider/Run_Debug_Configuration_dotNet_Project.html).

## 4. Visual Studio：从 IDE 启动调试

**4. Visual Studio: launch debugging from the IDE**

使用支持本仓库 .NET 10 和 `.slnx` 的 Visual Studio 版本；旧 IDE 不能识别时先升级，或打开 CLI 的 csproj 排查 SDK 支持，不要为此在仓库再维护一套 solution。

Use a Visual Studio version supporting this repository's .NET 10 and `.slnx`. Upgrade older IDEs or open the CLI csproj to diagnose SDK support; do not maintain a second solution for this.

1. 打开 `ZrConfig.slnx`，在解决方案资源管理器中将 **ZrConfig.Cli** 设置为启动项目。<br>
   Open `ZrConfig.slnx` and set **ZrConfig.Cli** as startup project.
2. 选择 **Debug** 配置。打开 CLI 项目属性的 **Debug / 调试 → Open debug launch profiles UI / 打开调试启动配置文件 UI**。<br>
   Select **Debug**, then CLI project properties → **Debug → Open debug launch profiles UI**.
3. Command-line arguments 填下面内容；Working directory 设为本机仓库根目录，例如 `D:\Work\ZrConfig`。不添加 `dotnet run --`。<br>
   Enter the following Command-line arguments and set Working directory to your local root, e.g. `D:\Work\ZrConfig`. Do not add `dotnet run --`.

```text
export "samples/Data" "samples/Output" "unity/ZrConfig.Sample/Assets/HotUpdate/Config" "unity/ZrConfig.Sample/Assets/Editor/ConfigRules" --rules-debug
```

4. 直接打开 `unity/ZrConfig.Sample/Assets/Editor/ConfigRules/AllTypesRules.cs`，给 Validate 第一条 if 设断点。<br>
   Open `unity/ZrConfig.Sample/Assets/Editor/ConfigRules/AllTypesRules.cs` directly and break on Validate's first if.
5. 按 **F5 / 开始调试**，而不是“不调试启动”。命中后使用 Locals、Watch、Call Stack 查看数据、原始空白标记和调用链。<br>
   Press **F5 / Start Debugging**, not Start Without Debugging. Inspect Locals, Watch, and Call Stack for values, original blank markers, and calls.
6. 需要只看某条记录，在断点设置中增加条件 `row.Id == 1`。F10 单步跳过，F11 单步进入，F5 继续；快捷键可能受个人键位设置影响。<br>
   Add breakpoint condition `row.Id == 1` for one record. F10 steps over, F11 steps in, F5 continues; personal key mappings may differ.
7. 结束后查看控制台和 Output/Logs；每次修改规则源码后重新执行导出，让工具重新编译。不要依赖 IDE Edit and Continue 更新已加载的动态程序集。<br>
   Inspect console and Output/Logs afterward. Rerun export after every rule edit so the tool recompiles it; do not rely on Edit and Continue for loaded dynamic assemblies.

IDE 会管理项目的调试启动配置；本次文档不为每位开发者提交绝对机器路径。若需要共享配置，应先按项目目录约定统一安排，并确保路径可移植。启动配置字段参考 [Microsoft 官方 .NET 调试配置](https://learn.microsoft.com/en-us/visualstudio/debugger/project-settings-for-csharp-debug-configurations-dotnetcore)。

The IDE manages launch profiles; these instructions do not commit developers' absolute machine paths. Any shared configuration must follow layout conventions and remain portable. See [Microsoft .NET debugging settings](https://learn.microsoft.com/en-us/visualstudio/debugger/project-settings-for-csharp-debug-configurations-dotnetcore).

### Visual Studio 附加现有进程

**Attach Visual Studio to an existing process**

终端先启动 CLI，例如：

Start CLI in a terminal, for example:

```powershell
dotnet src/ZrConfig.Cli/bin/Debug/net10.0/ZrConfig.Cli.dll export samples/Data samples/Output unity/ZrConfig.Sample/Assets/HotUpdate/Config unity/ZrConfig.Sample/Assets/Editor/ConfigRules --rules-debug
```

在 **Debug → Attach to Process / 调试 → 附加到进程** 中按进程名、PID、命令行确认正确的 `ZrConfig.Cli.exe` 或承载该 DLL 的 `dotnet.exe`，使用适用于 .NET/CoreCLR 的托管调试器。机器上有多个 dotnet 时，不能只凭名字选择。附加操作不会补传 `--rules-debug`，必须在启动命令中提供。

In **Debug → Attach to Process**, identify the correct `ZrConfig.Cli.exe` or hosting `dotnet.exe` by name, PID, and command line; use the .NET/CoreCLR managed debugger. Do not choose among multiple dotnet processes by name alone. Attachment cannot add `--rules-debug`; supply it at launch.

CLI 不会等待附加。若导出太快，回到本节 F5 启动方式；只有外部发布工具可用时，配置 IDE 启动该可执行文件及参数，再调试。UI 入口随 IDE 版本可能变化，参考 [Microsoft 官方附加进程文档](https://learn.microsoft.com/en-us/visualstudio/debugger/attach-to-running-processes-with-the-visual-studio-debugger)。

CLI does not wait for attachment. If it exits too quickly, use F5 as above. If only a published executable is available, configure the IDE to launch it with arguments. UI placement varies by IDE version; see [Microsoft process attachment](https://learn.microsoft.com/en-us/visualstudio/debugger/attach-to-running-processes-with-the-visual-studio-debugger).

Rider 的 **Run → Attach to Process** 同样选择实际 CLI/apphost 或 dotnet 承载进程；参见 [Rider 官方附加文档](https://www.jetbrains.com/help/rider/attach-to-process.html)。不建议为了抢附加时机把永久等待循环写进业务规则。

Rider's **Run → Attach to Process** likewise targets the actual CLI/apphost or dotnet host. See [Rider attachment](https://www.jetbrains.com/help/rider/attach-to-process.html). Do not add permanent wait loops to business rules just to catch attachment timing.

## 5. 断点不命中的排查顺序

**5. Troubleshoot missed breakpoints**

先区分安装版和源码构建版：macOS 安装版通常运行自 `/Applications/ZrConfig.app`，IDE 则启动工程的 bin 输出。窗口版本相同也可能来自不同构建，附加时同时核对完整进程路径、PID 和已加载模块的符号；修改源码后不会自动更新已安装的 App。

Distinguish installed and source-built tools. macOS installations usually run from `/Applications/ZrConfig.app`, while the IDE starts bin output. Equal displayed versions can be different builds. Verify full process path, PID, loaded modules, and symbols. Source edits do not automatically update the installed App.

| 现象<br>Symptom | 检查与处理<br>Check/action |
| --- | --- |
| 程序很快结束<br>Process exits quickly | 从 IDE Debug/F5 启动，先在 CLI 入口设断点核对 args<br>Launch with IDE Debug/F5; break at CLI entry and inspect args |
| CLI 入口也不命中<br>CLI entry breakpoint also misses | 启动项目、构建配置、实际二进制路径是否正确，是否误点 Run<br>Check startup project, build configuration, actual binary path, and whether Run was used |
| 规则断点空心/未绑定<br>Rule breakpoint hollow/unbound | 动态程序集是否已加载、是否带 --rules-debug、源码路径是否与编译时一致<br>Check dynamic assembly loading, --rules-debug, and source-path consistency |
| 代码相同仍提示源码不匹配<br>Source mismatch despite identical code | 是否打开了副本/旧 SDK 中的文件；保存后结束本轮，再从原路径重新导出<br>Check for copies/old SDK files; save, end the run, and export again from the original path |
| 只改工具为 Debug 仍难单步<br>Tool Debug still hard to step through | 工具 Debug 不等于动态规则 Debug，核对参数和 export.log 的编译模式<br>Tool Debug differs from dynamic-rule Debug; check arguments and export.log mode |
| 规则完全没执行<br>Rules never execute | Rules 是否留空，目录是否正确，类是否可实例化并绑定当前表<br>Check nonempty/correct Rules path, instantiable classes, and target-table binding |
| 字段自定义规则不执行<br>Custom field rule does not execute | IFieldRule 是否被 Check 绑定；空白字段默认跳过 Check<br>Check Check binding of IFieldRule; blank fields skip Check by default |
| ValidateTable 执行但 Validate 不执行<br>ValidateTable runs but Validate does not | 表可能是零行；逐行方法无记录可处理<br>The table may contain zero rows |
| 后置方法全部跳过<br>All post-validation skipped | 查 BUSINESS_CHECK_SKIPPED、CELL_PARSE、SCHEMA_MISMATCH、TABLE_REFERENCE 和依赖表<br>Inspect BUSINESS_CHECK_SKIPPED, CELL_PARSE, SCHEMA_MISMATCH, TABLE_REFERENCE, and dependencies |
| Unity 中能编译但 App 找不到类<br>Unity compiles but App cannot find the class | 是否用 UNITY_EDITOR 包住整个规则；App 不定义该符号<br>Remove whole-class UNITY_EDITOR wrapping; App does not define it |
| 规则运行但断点所在分支不进<br>Rule runs but branch does not | 看实际值、IsEmpty、数组长度与排序；把断点移到方法第一条语句<br>Inspect actual values, IsEmpty, array length/order; break on the first statement |
| 命中了另一份程序集<br>A different assembly is hit | 按 PID 和完整路径确认实际 CLI；不要附加构建服务器或 Unity Editor<br>Verify CLI by PID/full path; do not attach to build servers or Unity Editor |

源码调试时，可先在 `ValidationEngine.Validate` 的 `plan.Rules.ValidateRow(row, context)` 处停住，检查 Rules 的实际类型、Table.IsComplete、Dependencies，然后单步进入。这样能区分“未发现规则”“被跳过”和“已运行但条件不成立”。

When debugging tool source, break at `plan.Rules.ValidateRow(row, context)` inside `ValidationEngine.Validate`, inspect actual Rules type, Table.IsComplete, and Dependencies, then step in. This distinguishes undiscovered rules, skipped rules, and false conditions.

动态程序集名称形如 `ZrConfig.DynamicRules_<指纹>`，从内存加载 DLL 和 PDB，可能没有普通磁盘模块路径。Visual Studio 的 Modules / 模块窗口可看加载与符号状态；需要单步动态/外部代码时，检查 **Enable Just My Code / 启用仅我的代码** 是否限制了进入。Rider 同样检查外部代码单步过滤。不要通过关闭源码匹配检查掩盖打开了错误源码的问题。

Assemblies named `ZrConfig.DynamicRules_<fingerprint>` load DLL/PDB from memory and may have no normal disk module path. Visual Studio's Modules window shows load/symbol status. Check **Enable Just My Code** when stepping into dynamic/external code; check Rider's external-code filters too. Do not disable source matching to conceal opening the wrong source.

缓存命中仍加载符号。指纹包含源码绝对路径及内容，移动或编辑文件会改变缓存键。先修路径、编译与模式；只有怀疑损坏时，停止导出后清理该 Output 下的 `.rules-cache`，下一次自动重编。不要删除有效版本数据来解决断点问题。

Cache hits still load symbols. Fingerprints include absolute source paths/content, so moving/editing files changes the key. Fix paths, compilation, and mode first. Only if corruption is suspected, stop export and clear that Output's `.rules-cache`; the next run recompiles. Never delete valid releases to fix breakpoints.

## 6. 捕获异常与定位 Excel

**6. Catch exceptions and locate Excel cells**

后置检查会捕获异常并生成诊断，因此“仅遇到未处理异常时暂停”可能不会停在原始 throw。Rider 使用异常断点，Visual Studio 使用 **Exception Settings / 异常设置**，临时选择目标异常类型在抛出时中断。先关注 NullReferenceException、ArgumentException 或日志里的具体类型，定位后恢复设置，避免所有内部异常都中断。

Post-validation catches exceptions and reports diagnostics, so breaking only on unhandled exceptions may miss the original throw. Use Rider exception breakpoints or Visual Studio **Exception Settings** to break when the relevant type is thrown. Start with NullReferenceException, ArgumentException, or the logged type, and restore settings afterward to avoid stopping on every internal exception.

每次失败保留两份文件：

Preserve two files after failures:

- `Output/Logs/export.log`：过程、编译模式、规则目录、缓存信息、完整异常。<br>
  `Output/Logs/export.log`: Process steps, compile mode, rules path, cache details, and complete exceptions.
- `Output/Logs/diagnostics.json`：本次 runId、success、versionId、language 与 diagnostics。<br>
  `Output/Logs/diagnostics.json`: This run's runId, success, versionId, language, and diagnostics.

可以用下面命令读取完整 JSON：

Read the complete JSON with:

```bash
bash scripts/maintain.sh format-json samples/Output/Logs/diagnostics.json
```

Windows 可用 `powershell -File scripts/maintain.ps1 format-json samples/Output/Logs/diagnostics.json`。也可直接在 Visual Studio、Rider 或文本编辑器中打开 JSON；查看日志不需要安装额外脚本语言。

On Windows use `powershell -File scripts/maintain.ps1 format-json samples/Output/Logs/diagnostics.json`. You can also open JSON in Visual Studio, Rider, or a text editor; no extra scripting language is needed.

| 字段<br>Field | 解释<br>Meaning |
| --- | --- |
| Code | 稳定代号，例如 CELL_PARSE、RULE_BINDING、VALIDATOR_EXCEPTION<br>Stable identifier, e.g. CELL_PARSE, RULE_BINDING, VALIDATOR_EXCEPTION |
| File / Sheet / Table | 原工作簿路径、工作表、逻辑表名；编译错误 File 是 .cs<br>Original workbook path, sheet, logical table; compilation errors use a .cs File |
| Row / Column | 从 1 开始的物理行列；0 表示未定位到某一格<br>One-based physical coordinates; 0 means no individual cell located |
| Field | 逻辑字段或具体元素路径，例如 Actions[1].Delay<br>Logical field or element path, e.g. Actions[1].Delay |
| Severity | 数值枚举：0 Info、1 Warning、2 Error、3 Success；只有 Error 阻止发布<br>Numeric enum: 0 Info, 1 Warning, 2 Error, 3 Success; only Error blocks publication |
| SeverityText / Message | 当前日志语言的显示文字<br>Display text in the current logging language |

报错行不是数据列表下标。按本文档的五行表头填写时，数据从 Excel 第 6 行开始；定位错误时始终以本次诊断报告的实际行列为准。数组按 $order 排序以后，下标仍会映射回原始物理位置。

Error rows are not data-array indexes. With five headers, data starts at Excel row 6. Always use actual coordinates from this run's report. After $order sorting, indexes still map back to original physical cells.

生成代码的错误主要看 App 本次生成日志或 CLI codegen 控制台；生成不会更新导出的 diagnostics.json。输出目录建立失败、export.lock 被占用等早期错误也可能发生在报告写入前，不能把旧报告当成本次结果。先看控制台异常和文件时间/runId。

For generation errors, use the current App generation log or CLI codegen console; generation does not update export diagnostics.json. Early errors such as output creation failure or occupied export.lock can occur before report writing. Do not use an old report as current evidence; inspect console errors, timestamps, and runId.

上表描述的是导出写入的 diagnostics.json。App“导出诊断报告”及 CLI `--report` 使用独立报告格式：包含 `success`、`cancelled`、`needsGeneration`、`codeCommitted`、`published` 等结果字段，诊断 `severity` 为字符串并替换已知项目根路径。只校验时 `published` 为 false；`needsGeneration` 为 true 表示完整校验尚未完成。复制报告前核对操作类型与运行编号。

The table above describes export diagnostics.json. App “导出诊断报告” (Export diagnostic report) and CLI `--report` use a separate format with `success`, `cancelled`, `needsGeneration`, `codeCommitted`, and `published`; diagnostic `severity` is a string and known project-root paths are replaced. Validation-only sets `published` false; `needsGeneration` true means full validation is incomplete. Check operation/run identifiers before copying reports.

## 7. 工具源码的断点路线

**7. Breakpoint route through tool source**

输入递归覆盖根目录中的 `.xlsx` 和 `.xls`。先核对日志中的完整文件路径，再找 Sheet 和行列：跨目录重名报告 TABLE_DUPLICATE，子目录扫描失败报告 INPUT_SCAN，损坏或格式不符报告 EXCEL_OPEN。发现、格式选择与文本转换断点设在 ExcelWorkbookInput；解析断点仍在 SchemaLoader/ExcelDataLoader。测试副本应放在独立输入树，不能留在已选根目录的子文件夹中。

Input discovery recursively includes `.xlsx`/`.xls`. Check full paths before sheets/coordinates. Duplicate names across directories yield TABLE_DUPLICATE, scan failures INPUT_SCAN, and damaged/mismatched files EXCEL_OPEN. Break in ExcelWorkbookInput for discovery, format choice, and text conversion; SchemaLoader/ExcelDataLoader for parsing. Test copies belong in separate input trees, not subfolders of the selected root.

| 阶段<br>Stage | 文件/方法<br>File/method | 观察值<br>Inspect |
| --- | --- | --- |
| 命令行参数<br>CLI arguments | ToolCommandRunner.Run / ToolCommandLine.Parse | args、项目配置、最终目录、RulesDebug<br>args, project settings, final paths, RulesDebug |
| 类型发现/代码生成<br>Type discovery/generation | ModelGenerator.Prepare、ModelGenerationPlan.Apply、RuleCompiler.PrepareTypes<br>ModelGenerator.Prepare, ModelGenerationPlan.Apply, RuleCompiler.PrepareTypes | Config 根目录、类型编译诊断、schemas；确认后的写入在 Apply<br>Config root, compile diagnostics, schemas; confirmed writes happen in Apply |
| 表头/主键<br>Headers/keys | SchemaLoader、TableDefinitions、ExcelColumnLayout<br>SchemaLoader, TableDefinitions, ExcelColumnLayout | 表名、DataStartRow、Fields、主键、SchemaHash<br>Table name, DataStartRow, Fields, keys, SchemaHash |
| 单元格/多行<br>Cells/multi-row input | ExcelDataLoader、ExcelStructuredDataLoader、ConfigValueParser<br>ExcelDataLoader, ExcelStructuredDataLoader, ConfigValueParser | 原始文本、IsComplete、RowSource、排序后的路径<br>Raw text, IsComplete, RowSource, sorted paths |
| 动态编译<br>Dynamic compilation | RuleCompiler.PrepareCore | sources 的绝对路径、debug、fingerprint、emitted.Diagnostics<br>Absolute sources, debug, fingerprint, emitted.Diagnostics |
| 标签/后置<br>Attributes/post-validation | ValidationEngine.Validate / Bind | plans、Checks、Dependencies、complete、当前 row<br>plans, Checks, Dependencies, complete, current row |
| 发布<br>Publication | ExportPipeline.Publish | staging、versionId、manifestTmp、切换前的 current.json<br>staging, versionId, manifestTmp, pre-switch current.json |
| 运行时<br>Runtime | ConfigManager.ReloadAsync | 清单、资源相对路径、SchemaHash、candidate、Current<br>Manifest, relative resource paths, SchemaHash, candidate, Current |

调试 App 时从 IDE 启动 ZrConfig.App，给按钮回调设断点，然后在窗口点击按钮。生成/导出在 Task.Run 中执行；应 await 任务并观察原始异常，不要只看 UI 的最后一句状态。

Launch ZrConfig.App from the IDE, break in button callbacks, and click them. Generation/export runs in Task.Run; await it and inspect original exceptions, not only the last UI status.

### 7.1 目录恢复与语言偏好

**7.1 Restored paths and language preferences**

`Environment.SpecialFolder.LocalApplicationData` 下的 `ZrConfig/workspace.json` 保存四个目录，`ZrConfig/settings.json` 保存语言；实际系统根路径由运行环境决定。WorkspaceSettings 保存时把非空路径转换为绝对路径，避免从不同工作目录启动后指向别处。

Under `Environment.SpecialFolder.LocalApplicationData`, `ZrConfig/workspace.json` stores four paths and `ZrConfig/settings.json` stores language; the OS determines the actual root. WorkspaceSettings saves nonempty paths as absolute to avoid working-directory-dependent changes.

排查选择了旧项目时先看界面的四个路径。需要检查偏好文件时先关闭 App 并备份，再处理对应文件；设置不存在或损坏会回退默认值。ToolPaths 优先从可执行文件位置、再从工作目录向上查找仓库：识别到仓库使用 samples；独立 macOS .app 未识别到仓库时使用用户数据目录下的 ZrConfig/Workspace，其他独立进程使用工作目录。有效的已保存设置优先。

If an old project is selected, inspect the four UI paths first. Close/back up the App before editing preference files; missing/corrupt settings fall back to defaults. ToolPaths searches upward from the executable, then working directory, for a repository. If found, it uses samples. Standalone macOS .app uses user-data ZrConfig/Workspace when no repository is found; other standalone processes use the working directory. Valid saved settings take priority.

项目设置直接使用保存的路径；搬动项目后检查目录配置，不自动重定向历史目录。相关实现位于 WorkspaceSettings 和 ToolPaths。

Project settings use saved paths directly. Check them after moving projects; historical paths are not automatically redirected. See WorkspaceSettings and ToolPaths.

定位发布错误时保留 current.json 内容，确认在最后 File.Move 之前失败仍保留旧清单；版本目录存在不等于它已经激活。不要手工改清单里的哈希来让错误数据通过检查。

Preserve current.json when debugging publication. Failure before final File.Move must retain the old manifest; an existing version directory is not necessarily active. Never edit hashes to force invalid data through checks.

## 8. Unity 游戏端调试

**8. Debug Unity game code**

Rider 附加正在使用的 Unity Editor/Player；Visual Studio 使用安装好的 **Visual Studio Tools for Unity → Attach to Unity / 附加到 Unity**。检查代码与 Editor 工程一致，再进入 Play 并触发游戏自己的加载入口。Visual Studio 的步骤参考 [官方 Unity 调试文档](https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/get-started/using-visual-studio-tools-for-unity)。

Attach Rider to the active Unity Editor/Player, or use installed **Visual Studio Tools for Unity → Attach to Unity**. Confirm source matches the Editor project, enter Play, and trigger the game's loader. See [Microsoft Unity debugging](https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/get-started/using-visual-studio-tools-for-unity).

示例源码直接位于 `Assets/ZrConfig/Runtime`、`Assets/ZrConfig/Unity`；生成表与 `TableCodecs` 位于 `Assets/HotUpdate/Config/Generated`。从 Unity 双击脚本打开实际源码，IDE 工程未更新时在 Unity 的 External Tools 中重新生成项目文件。App 不再显示虚拟 Source 分组。

Sample source is directly in `Assets/ZrConfig/Runtime` and `Assets/ZrConfig/Unity`; generated tables/TableCodecs are in `Assets/HotUpdate/Config/Generated`. Open actual scripts from Unity and regenerate IDE files in External Tools when stale. The App no longer displays virtual Source groups.

首先在游戏调用处停住：

First break at the game's call site:

```csharp
// 放在项目已有的异步初始化方法中；configOutputFolder 是实际数据根目录。
var source = new ZrConfig.Runtime.FileConfigSource(configOutputFolder);
var snapshot = await ZrConfig.Unity.UnityConfigRuntime.LoadAsync(
    ZrConfig.Models.TableCodecs.Registry, source);
UnityEngine.Debug.Log(snapshot.VersionId);
```

**示例英文说明 / English notes:**

- Place inside the project's existing async initialization method; configOutputFolder is the actual data root.


第一次成功后用 Reload / ReloadAsync。已有同步源时也可首次调用 `Load(TableCodecs.Registry, source)`；已经调用 Initialize 的项目使用 `Reload(source)` / `ReloadAsync(source)`，不要再首次 Load。异步返回自带的 ConfigOperation，无需 Task；同步重载在已有加载期间会明确报错。Unity API 从主线程调用；观察 await 的异常，不要用未观察的后台任务吞掉失败。

After first success use Reload / ReloadAsync. A synchronous source may initially call `Load(TableCodecs.Registry, source)`. Projects already calling Initialize use `Reload(source)` / `ReloadAsync(source)`, not first Load again. Async returns built-in ConfigOperation without requiring Task. Synchronous reload explicitly fails during another load. Call Unity APIs on the main thread and observe await exceptions; do not lose failures in unobserved background work.

按以下顺序检查热更新：

Check updates in this order:

1. `IConfigSource.ReadAsync` 是否读取正确资源根目录，第一次路径应为 `current.json`，随后为 `versions/<versionId>/<file>`。<br>
   Verify `IConfigSource.ReadAsync` uses the correct root: first `current.json`, then `versions/<versionId>/<file>`.
2. 核对所需表是否在清单中、schemaHash 是否与当前生成 Codec 一致。默认懒加载在访问时检查目标表；要求整个目录一致时传入 TableCodecs.Registry 并执行 ReloadAll / ReloadAllAsync。<br>
   Check required tables and schemaHash against generated codecs. Default lazy loading validates tables on access. For whole-catalog consistency, pass TableCodecs.Registry and call ReloadAll / ReloadAllAsync.
3. sha256 不符先检查缺文件、截断、上传/缓存混用；Schema 不符先同步生成源码与数据。两者处理方式不同。<br>
   For sha256 mismatch, check missing/truncated files and mixed uploads/caches. For schema mismatch, synchronize generated source/data. They require different fixes.
4. 断点放在 ReloadAsync 的 await 前后，比较 `oldSnapshot` 和 `UnityConfigRuntime.Current`。清单失败/取消应保持同一旧快照；首次 LoadAsync 失败后可重试。默认初始化只读取清单，数据错误在 GetRowAsync/GetTableAsync 等实际访问时检查，本批失败不发布行缓存。<br>
   Break before/after ReloadAsync await and compare `oldSnapshot` with `UnityConfigRuntime.Current`. Manifest failure/cancellation retains the old snapshot; failed initial LoadAsync is retryable. Default initialization reads only the manifest; GetRowAsync/GetTableAsync checks data on access, and failed batches publish no row cache.
5. 成功后重新从 Current 取表；旧 row 的普通字段不会原地变成新数据；旧快照查询和旧行引用访问（包括缓存命中）应报配置已失效。<br>
   After success reacquire tables from Current. Old row values do not change in place; old snapshot queries/reference access, including cache hits, must report invalidated configuration.
6. 在 Changed 回调中检查 UI 刷新。监听器异常会记入 Unity Console，但不会回滚已经成功的快照；不要因为 UI 报错再次调用首次初始化。<br>
   Inspect UI refresh in Changed. Listener exceptions log to Unity Console without rolling back an already committed snapshot; do not reinitialize because the UI failed.
7. `Tools → ZrConfig → 运行时配置表` 查看的是游戏当前已加载快照，不直接读取 Excel；只按行加载时也可能无完整表，这是正常状态；查看 LoadedRowCount，或显式加载对应整表。<br>
   `Tools → ZrConfig → 运行时配置表` (Runtime tables) displays the currently loaded snapshot, not Excel. Row-only loading may leave no full table; inspect LoadedRowCount or explicitly load that table.

Android/iOS/WebGL 的示例使用内置 `StreamingAssetsConfigSource` 和 `CoalescingConfigSource`；在 Unity 主线程检查 UnityWebRequest、相对路径与取消，不将 APK 内路径或 URL 当作桌面文件。具体接入见 [StreamingAssets 适配器](project-development.md#145-androidioswebgl-与-streamingassets)。具体 Player 调试、IL2CPP 符号与代码热更新框架按项目平台验证，CLI 的 PDB 不覆盖这些环境。

Android/iOS/WebGL samples use `StreamingAssetsConfigSource` and `CoalescingConfigSource`. On Unity's main thread inspect UnityWebRequest, relative paths, and cancellation; APK paths/URLs are not desktop files. See [StreamingAssets](project-development.md#145-androidioswebgl-与-streamingassets). Verify Player debugging, IL2CPP symbols, and code hot-update frameworks per platform; CLI PDBs do not cover them.

## 9. 一次可复查的调试验收

**9. A reproducible debugging acceptance run**

- 合法数据先导出成功，记录版本和日志。<br>
  Export valid data first and record version/logs.
- 在临时数据副本制造一个规则错误，命中对应方法并确认值与空白标记。<br>
  Introduce one rule error in a temporary copy, hit its method, and inspect values/blank markers.
- 退出码为 1、success 为 false，诊断位置正确，旧 current.json 未切换。<br>
  Expect exit code 1, success false, correct diagnostic location, and unchanged old current.json.
- 修复后导出成功，Unity 加载新 versionId；旧引用仍保持旧值，新查询得到新值。<br>
  Fix and export successfully; Unity loads the new versionId. Held old values stay old, while new queries return new values.
- 如排查运行时损坏，在临时 Output 破坏 bytes：默认重载后在访问该表时应报错；使用 ReloadAll 或包含该表的 ReloadPrepared 时应在切换前失败并保留旧快照。不要损坏正式发布数据。<br>
  For corruption checks, damage bytes only in temporary Output. Default reload should fail when that table is accessed; ReloadAll or ReloadPrepared including it should fail before switching and preserve the old snapshot. Never damage real releases.
- 分享问题时附工具版本、平台/IDE、实际命令与目录、两份日志、期望/实际结果、最小复现源码和脱敏数据；固定日志会被下一次导出更新，独立记录保留在 `Output/Logs/<运行编号>/`；也可主动导出诊断报告。<br>
  Share tool version, platform/IDE, exact command/paths, both logs, expected/actual behavior, minimal source, and sanitized data. Fixed logs are overwritten by later exports; historical logs remain in `Output/Logs/<run-id>/`. Diagnostic reports may also be exported explicitly.

文档依据当前源码、仓库自动化测试与上述官方 IDE 文档整理。本仓库的自动化检查可验证动态 Debug/PDB、源码调用栈与失败保护；不能代替本机 Rider/Visual Studio 的交互断点验收，目标 Player/IL2CPP 的已有记录和未测范围见 [平台验收](release-and-acceptance.md#验收与团队试用记录)。

This guide reflects current source, repository tests, and the linked official IDE documentation. Automation verifies dynamic Debug/PDB, source call stacks, and failure protection; it does not replace interactive Rider/Visual Studio breakpoint acceptance. See [platform records](release-and-acceptance.md#验收与团队试用记录) for Player/IL2CPP coverage and untested areas.
