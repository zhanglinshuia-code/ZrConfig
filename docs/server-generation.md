# 多语言生成与 JSON 数据

**中文** | [English](server-generation.en.md)

一次生成可以选择 C#、C++、Go、Java、Lua、TypeScript、Rust 中任意组合。Excel 的字段所属端与代码语言独立：`C` 导出 C/CS 字段，`S` 导出 S/CS 字段；选择 Java 不会自动改变字段所属端。数据格式可选 `binary`、`json`、`both`，旧项目默认仍是 C# 和二进制。

本文是接入与操作说明。设计约束见 [服务器运行时设计](server-runtime-design.md)，可执行检查见 [公开服务器验收](../tests/README.md#公开服务器验收)，各轮实测范围见 [验收记录](release-and-acceptance.md#验收与团队试用记录)。

| 需要做什么 | 阅读位置 |
| --- | --- |
| 选择语言、目标端和输出格式 | [使用入口](#使用入口)、[生成物与依赖](#生成物与依赖) |
| 处理重新生成时的 API 变化 | [公开接口保护与显式迁移](#公开接口保护与显式迁移) |
| 核对数值、集合、转换器和数据文件 | [类型契约](#类型与数据契约)、[文件与版本](#文件版本与加载) |
| 新接入服务器、选择加载方式 | [server-v1 入口](#服务器契约家族与显式加载) |
| 维护已有同步全量消费方 | [原有加载入口](#文件版本与加载)、[生命周期](#并发性能与生命周期)、[Lua / TypeScript / Rust](#luatypescriptrust-的接入差异) |
| 验证功能与测量成本 | [验收范围](#验收范围)、[性能测量](#可重复的性能测量) |

## 使用入口

App 的“生成语言”支持多选，“数据格式”支持 Binary、JSON、Binary + JSON。选项随项目保存，生成预览、只校验、导出、命令行共用同一配置。

```bash
# 已有项目：一次生成全部语言，并将两种数据一起发布。
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- \
  export --project "/项目路径/project.zrconfig.json" --target S --languages all --format both --generate

# 仅生成选定语言；生成预览仍会列出更新和退役文件。
bash scripts/dotnet.sh run --project src/ZrConfig.Cli -p:NuGetAudit=false -- \
  codegen --project "/项目路径/project.zrconfig.json" --target S --languages java,go,cpp --format json
```

以上命令从仓库根目录执行，将项目路径替换为实际文件。Windows 使用 `dotnet run --project src/ZrConfig.Cli -p:NuGetAudit=false --` 并将参数写在同一行。共用示例已设置七语言、`both`、C 端和 `server-v1`，体验示例时直接使用 `samples/project.zrconfig.json`，无需改动其目标端或语言集。

`--accept-changes` 沿用原有含义：核对预览后，允许删除退役生成文件或调整输入根目录。`export` 不带 `--generate` 不会更改源码；更改语言列表后应先生成。`validate` 检查选定语言的生成文件是否过期，不会调用外部编译器。构建 Java/Go/C++ 应在对应业务工程或验收环境完成。

### 公开接口保护与显式迁移

Config/.schema.json 现在同时保存结构摘要和 `ApiContract`，与生成源码在同一事务中提交；请与源码一起保存在版本控制中。工具先比较已接受基线与本次候选，再写入文件。普通 `--accept-changes`、App 的生成确认，以及 `export --generate` 都不能批准公开接口变化。

大于 256 表时，基线使用明确标记的 `NameEncoding: compact-bindings-v1`：名称绑定按固定七列保存，只有能够从 Scope、Name 精确重建的内部保留名省略重复来源字符串。读取后还原完整绑定，再进行名称、退休状态、迁移链及原有描述摘要校验；没有删减冻结范围。摘要计算采用流式 UTF-8，普通规模的基线编码与摘要保持原样。未知编码、损坏或缺项停止写入；不支持这种编码的旧工具也不能把它当作新项目重建。

大目录的 Java 生成使用分段实现类，避免 JVM 方法长度及常量池限制，保留 `ConfigSnapshot.Xxx`、`DataSnapshot.XxxRow/XxxTable`、getter、静态导入和方法引用等源码调用形式。实现基类的构造受包访问限制。继承关系和成员声明归属仍属于契约变更报告：已有项目切换这种布局时，不能跳过 API 门禁，也不能把源码调用可用等同于反射声明归属或二进制 ABI 完全不变。Lua 和 TypeScript 的大型初始化过程拆为内部函数；跨表累计表达式控制语法树深度。

Rust 大目录使用私有的类型化存储分段；根快照通过私有 trait 取得具体分段，行值仍存入 `OnceLock<Arc<T>>`。行与分组调度、索引构建和引用校验共用不依赖行类型的冷加载实现，回调只借用栈上的类型化状态。热查询直接读取类型化槽，不使用反射、向下转换或 `unsafe`。超过 256 表时，私有构建与加载函数还显式限制内联，控制编译器重复展开的成本；公开类型、方法、模块入口和热查询 getter 保持原样。编译耗时、编译峰值及运行时加载成本应分别测量，不能把某个目录规模的结果推广为任意表结构的容量保证。

- C# 从实际生成源码提取 public/protected 声明，包括构造函数、参数名/顺序/默认值、返回类型、可空上下文、访问器、继承、约束、常量和重载集合。删除、改签名、增加同名重载会阻止写入。增加字段可能改变公开构造函数，不能简单视为兼容增加。
- 六种原生语言从实际输出源码建立独立的声明语法模型 `NativeSymbols`，记录作用域、名称及完整声明文本，并同时保留**保守源码摘要**，覆盖登记的模块、运行库及构建配置文件。声明模型用于显示删除、改签名和同名入口变化；推断类型、语言绑定及实现语义继续由摘要门禁与真实消费方共同保护。忽略普通注释和生成结构元数据中的说明/哈希，保留 Lua 类型注解和字符串内容。其他源码变化需要人工审阅，包括内部实现更新和新增表导致的聚合快照变化。这不是六种语言的编译器级 API/ABI 兼容证明。
- 相同输入重复生成不会重写源码或基线；仅数据变化不改变 API，修改说明允许更新注释。合法旧名称保持原样，不因风格调整而批量改名。
- 首次升级只有旧结构摘要时，从**实际旧生成源码**导入基线，再比较候选。已有源码却没有摘要、摘要损坏/不受支持、已启用保护后契约被删除，或旧源码公开声明与基线不一致，均停止写入；应先从版本控制恢复，不能删除基线当作首次生成。

确需迁移时，第一次 `codegen` 返回 `API_CHANGE_REQUIRED`，列出变化和完整摘要。程序员完成影响检查及业务消费方验证后，重新执行相同目录、目标端和语言参数，并增加：

```text
codegen ... --verify-api <consumer-checks.json> --migration-receipt <新的receipt.json>
codegen ... --accept-api-change <本次预览的64位大写摘要> --migration-receipt <receipt.json>
```

若还涉及输入范围变化或文件删除，另加 `--accept-changes`。凭据绑定旧/新描述摘要、完整变更、目标契约版本、语言和目标端、工具指纹、输入摘要及消费方验证结果。仅提供摘要不能放行。验证阶段在系统临时目录生成候选并执行显式指定的消费方命令，失败、超时或候选被改写时不生成凭据。接受迁移会推进契约主版本，并将最近的凭据纳入 `.schema.json` 的 `LastMigration`，同时在 `ApiMigrationHistory` 保存每步旧／新契约及对应凭据；不保存永久允许破坏变更的开关。候选、工作簿、基线或工具变化，以及重复使用旧摘要都会失败。提交时再次检查输入、旧文件、支持源码和基线，过期计划报 `GENERATION_CHANGED`。API 迁移只允许通过 `codegen` 显式完成，再执行导出；App 当前提供查看和取消，不提供一键绕过。`API_BASELINE_INVALID` 表示必须先恢复或核对基线；`API_CHANGE_ID_MISMATCH` 表示摘要不匹配。

同一分支可连续执行多次独立迁移。发布校验从可信目标提交连接这些迁移记录；中间普通新增必须保持兼容。缺失迁移、顺序错误、旧凭据与新契约不符都会拒绝。历史记录不能代替真实消费方命令，也不是数字签名；应随变更一起接受代码审阅，受保护分支保存已经接受的记录。

C# 同时比较 Roslyn 声明与实际编译程序集的公开表面，包含 Runtime、序列化支持及业务类型公开依赖闭包。原生声明模型与生成器的 SymbolPlan 相互独立；编译和真实调用由公共验收工程覆盖。语法模型不替代编译器的类型推断、重载解析或 ABI 检验，原生描述仍保留保守源码门禁。所有行为、业务调用绑定、构建条件和性能承诺仍须独立验收。CI 从可信目标提交读取基线和生成源码，独立比较当前候选，不能仅靠更新当前 `.schema.json` 放行。

消费方检查文件是 JSON 数组，每项包含 `Language`、`Profile`、`Executable`、`Arguments`、`Directory`、`Inputs` 和可选 `TimeoutSeconds`。受影响语言必须都有检查命令；命令应同时编译候选和运行业务消费方。`Inputs` 列出参与验收的冻结源码、工程配置及锁文件，也可列目录并递归绑定其文件集合；路径相对于检查文件。验证前后重新检查输入与候选的完整文件清单，增加、删除、修改或引入符号链接都会拒绝凭据。候选目录只供读取，编译中间文件与结果应写到独立临时目录。参数中的 `{candidate}` 是临时 Config 根目录，`{input}` 是原 Excel 根目录。工具使用独立进程直接传参，不经过 shell；消费方命令由项目维护者明确配置和审阅。凭据记录命令、输入文件、可执行文件和输出的摘要；它是可审阅的验收记录，并非数字签名，也不能替代对验收命令和未知外部消费者的审阅。

```json
[
  {
    "Language": "csharp",
    "Profile": "business-release",
    "Executable": "dotnet",
    "Arguments": ["run", "--project", "Consumer.csproj", "-c", "Release", "-p:ConfigRoot={candidate}"],
    "Directory": ".",
    "Inputs": ["Consumer.csproj", "Program.cs"],
    "TimeoutSeconds": 300
  }
]
```

上述是检查配置格式示例；`Consumer.csproj` 必须由业务工程提供，并在候选路径上编译实际调用。不同语言使用对应编译器/测试入口，不能用空命令替代验收。

项目 JSON 新增可选项：

```json
{
    "Target": "S",
    "CodeLanguages": ["csharp", "java", "go", "cpp", "lua", "typescript", "rust"],
    "DataFormat": "both",
    "RuntimeFamily": "auto",
    "Namespaces": {
        "CSharp": "Game.Config",
        "Java": "game.config",
        "Go": "example.com/game/config",
        "Cpp": "game::config",
        "Rust": "game_config",
        "TypeScript": "@game/config",
        "Lua": "game.config"
    }
}
```

`Namespaces` 整体和各项都可省略，省略时继承已接受基线，首次生成才采用默认值。C# Runtime 继续使用共享 `ZrConfig.Runtime`；Go 包名保持 `zrconfig`，模块根写入生成的 `go.mod`；TypeScript 生成对应包清单；Lua 根据点号模块根归位文件。模块根不改变线协议中的类型标识，改变已接受模块根必须经过 API 迁移。七语言的同进程双 Catalog 隔离由公共验收覆盖。App 会保留项目文件中的这些设置。

这里仅展示新增选项；原有 DataFolder、CodeFolder、OutputFolder、RulesFolder 必须保留。每次操作固定一个目标端。生成另一端时必须使用对应端的代码和数据，不能混用；数据结构不一致时加载器会拒绝加载。

## 生成物与依赖

所有语言共用 Excel 解析、分端、规则校验、结构摘要、生成事务及版本发布。语言生成器只接收不含 CLR 类型或反射对象的类型描述。原有 C# 类型定义和转换器由工具端适配到该描述；这是兼容现有工程的输入方式，服务端运行时不需要 .NET。

| 位置 | 内容 | 业务工程要求 |
| --- | --- | --- |
| Config/Generated/*.g.cs | C# 表、查询、Codec、唯一 TableCodecs | C# 9 / .NET Standard 2.1 或现有 Unity 工程 |
| Config/Generated/Java | 每表一个 XxxConfig.java、ConfigTypes、ConfigRuntime、ConfigSnapshot | Java 17+，包名 `zrconfig`，仅 JDK |
| Config/Generated/Go | 每表一个 xxx.g.go、types.go、runtime.go、snapshot.go | Go 1.19+，包名 `zrconfig`，仅标准库；由业务 go.mod 管理模块 |
| Config/Generated/Cpp | 每表一个 XxxConfig.hpp、types.hpp、runtime.hpp、snapshot.hpp | C++17，包含 snapshot.hpp，标准库和 filesystem |
| Config/Generated/Lua | 每表一个 XxxConfig.lua、types.lua、runtime.lua、snapshot.lua | Lua 5.4，64 位整数/双精度构建，仅标准库；提供 LuaLS 类型注解 |
| Config/Generated/TypeScript | 每表一个 XxxConfig.ts、types/runtime/snapshot.ts、tsconfig.json | TypeScript 5.8+、Node 22+，ES2022/CommonJS；仅 Node 标准库 |
| Config/Generated/Rust | 每表一个 XxxConfig.rs、types/runtime/snapshot/lib.rs、Cargo.toml | Rust 1.70+、edition 2021，仅标准库，无第三方 crate |

源码均为 UTF-8；Windows/MSVC 工程应启用 `/std:c++17 /utf-8 /EHsc`，Linux/macOS 使用 `-std=c++17`。

即使只选择服务端语言，工具仍生成内部 C# 模型，以复用已有规则编译和导出校验；这些文件不用拷入服务端工程。公共运行库的维护源码只保留在 Assets/ZrConfig/Native，以文本模板嵌入工具；Generated 内为可重建产物。生成事务只清理登记的原生文件，不递归删除业务文件；未登记的同名文件会阻止覆盖。

Excel 第四行说明会成为各语言公开字段或访问入口的中文注释，保留多行内容并做必要转义。空说明不补注释。字段说明不参与数据结构哈希，修改说明仍会更新生成代码。

## 类型与数据契约

| 类型 | 统一含义 / JSON 表示 | 各语言注意点 |
| --- | --- | --- |
| sbyte/byte/short/ushort/int/uint | 固定位宽有符号或无符号整数，JSON 数字 | Java 无符号值扩大到可容纳的有符号类型 |
| long/ulong | 64 位整数，JSON **十进制字符串** | Java 的 ulong 使用 BigInteger，防止溢出 |
| float/double | IEEE 754 单/双精度，JSON 数字 | 拒绝 NaN、Infinity；正负零查询相等 |
| decimal | 96 位系数、0–28 位小数，JSON **十进制字符串** | Java/Go/C++ 提供精确 Decimal；不经过浮点转换；拒绝需要舍入的值 |
| bool/string | JSON 布尔值 / UTF-8 字符串 | 严格检查 UTF-8、代理项及重复 JSON 成员；字符串保留原文 |
| enum | 显式枚举值，遵守底层整数表示 | 未定义的值拒绝；枚举别名保留 |
| T? | `null` 或 T | Java 包装类型、Go Optional、C++ optional |
| 数组/列表 | JSON 数组 | 加载后只读访问；空集合与 null 不混淆 |
| 多维数组 | `{"dimensions":[2,2],"values":[1,2,3,4]}` | 行优先展平，维度与元素数必须相符；空维度保留 |
| `Dictionary<K,V>` | `[[key,value], ...]` | 不把键强制转换为 JSON 成员名；拒绝重复键 |
| 普通对象 | 按定义列出全部成员；引用类型可为 null | 不执行业务行为，不自动翻译 C# 方法 |
| 多态对象 | 对象增加 `$type` | 使用明确的完整类型标识，仅允许已声明的具体子类型 |
| RawJson | 保存原始 JSON 文本的字符串 | 保留原文；不是外层数据格式的开关 |
| 表引用 | 目标表 int32 Id；0 为无引用 | 加载时绑定到同一快照对象；循环引用允许，缺失目标拒绝 |

表的结构仍在 Excel/类型定义中声明。JSON 数据文件不是新的结构定义入口：缺字段、多余字段、错类型、超范围、未知枚举或多态类型都会报错；不静默补默认值。Excel 空单元格的默认值继续由现有解析器决定，并完整写入数据。对象成员顺序、字典输出顺序和二进制协议由工具固定，业务不应依赖 JSON 空白或成员显示顺序。

数据保护上限为每表 5,000,000 行、单集合 1,000,000 个元素、类型嵌套 64 层、单字符串 UTF-8 编码 16 MiB；每行嵌套集合总元素预算为 8,000,000。JSON 解析额外限制 32,000,000 个值节点、72 层文本嵌套及 1,024 字符数字 token。超限会明确拒绝，不能把这些保护上限当作建议容量，部署前还需按内存预算测量实际配置。

主键和分组沿用已有约束，包括组合键、数值范围及可索引类型。仅被引用的表需要全局唯一的整数 Id；普通组合主键表中的 Id 可以重复。查询缺失时，Java `find` / C++ `find` 返回 null，Go `Find` 返回 `(nil,false)`；`get` / `Get` 对缺失键抛出异常或 panic。分组缺失返回空集合。

公开名称遵循目标语言限制：Java 的 Class 字段入口为 getClass_；Go 小写字段入口加 Get_，避免不可导出；C++ 关键字入口加下划线。协议字段名仍保留 Excel 原名。转义后冲突、类型名冲突及与快照入口重名会在生成预览中报错，不生成不可编译源码。

生成前由统一 SymbolPlan 检查公共入口、内部存储、辅助类型、模块和路径的冲突，包括 Java 关键字/构造参数、C++ 的 `v_`/`ref_` 存储、大小写路径冲突和 Windows 设备名。Go 表名 `_Item` 的源文件为 `table__item.g.go`，避免被 Go 构建忽略；已接受的正常名称保持稳定。新增生成符号必须同时登记规划规则并补充闭包测试，最终源码还要经过独立声明模型和真实编译器验收。

### 自定义转换器

任意 C# `Read/Write` 逻辑无法自动成为其他六种语言。支持标准编码成员序列的转换器，需在类型或转换器上声明布局：

```csharp
[ConfigConverter(typeof(MoneyConverter), "1")]
[ConfigWireLayout(nameof(Cents))]
public readonly struct Money
{
    public long Cents { get; }
    public Money(long cents) { Cents = cents; }
}
```

该声明表示转换器直接使用标准协议按顺序写入 Cents，不能额外添加私有字节、压缩流或自定义头。选择原生语言时（包括只导出二进制），工具会对真实导出数据执行二进制→JSON→二进制逐字节核对，并用原 Codec 回读；不一致时阻止发布。没有布局的转换器仍能使用原来的 C# 二进制流程，但选择 JSON 或其他语言会明确报错。布局变化进入结构签名；编码语义变化仍须更新转换器 Version。该属性是程序员维护的协议契约，不是对任意转换器代码的自动证明。

## 文件、版本与加载

二进制继续使用 v4 编码：小端整数、长度前缀 UTF-8、固定结构哈希、行索引、显式多态类型目录。JSON 表文件格式为 `ZrConfig.TableJson/1`：

```json
{
    "format": "ZrConfig.TableJson/1",
    "table": "Item",
    "schemaHash": "与生成代码一致的结构哈希",
    "rows": [{ "Id": 1, "Name": "药水", "Price": "10000000000000000" }]
}
```

每个版本仍发布到 Output/versions/版本号，最后原子替换 current.json。清单格式分别为 `ZrConfig.Binary/1`、`ZrConfig.Json/1`、`ZrConfig.Mixed/1`：

- binary：每表 `.bytes`，清单 file / sha256 指向二进制。
- json：每表 `.json`，file / sha256 指向 JSON。
- both：同一版本同时包含两者；file / sha256 指向二进制，jsonFile / jsonSha256 指向 JSON。

所有格式都校验表名、结构哈希、文件 SHA-256、行数、主键和引用；SHA-256 用于完整性检查，不是签名认证。格式未知、文件损坏或结构不匹配时不会静默回退到另一份数据。版本回滚会校验两份文件，要求与接入方部署清单兼容；示例部署也复制两种格式。

六种原生快照加载入口还会在读取任何表内容前比较清单与生成表的完整集合。额外表、缺表、同数量但替换了表名都会失败；重复表仍由清单解析拒绝。失败不会替换当前快照。下列传统 `ConfigSnapshot` / `ConfigManager` 入口保留同步全量加载语义；按表选择、按行加载及 Clear/Close 使用后文的服务器契约入口。

```java
var manager = new zrconfig.ConfigSnapshot.Manager();
var snapshot = manager.reload(java.nio.file.Path.of(output), false);
var item = snapshot.ItemCatalog.get(1001);
// false 优先二进制，true 在双格式版本中选择 JSON；JSON 单格式始终读 JSON。
```

```go
var manager zrconfig.ConfigManager
snapshot, err := manager.Reload(output, false)
if err != nil { return err }
item, exists := snapshot.ItemCatalog().Find(1001)
```

```cpp
#include "snapshot.hpp"
zrconfig::ConfigManager manager;
auto snapshot = manager.reload(output, false);
const auto* item = snapshot->ItemCatalog().find(1001);
// snapshot 必须覆盖 item、集合引用和跨表指针的整个使用期。
```

C# 的 ConfigLoadOptions 新增 `preferJson` 和 `retainPreviousSnapshots`，默认均为 false。使用 JSON 不需要额外 JSON 包：在加载时转换为现有按行读取结构，之后继续复用原有查询索引。示例：

```csharp
var options = new ConfigLoadOptions(retainPreviousSnapshots: true, preferJson: true);
var manager = new ConfigManager(TableCodecs.Registry, decodeManifest, options);
var snapshot = manager.ReloadAll(new FileConfigSource(output));
```

生成的注册表实际入口以当前 TableCodecs 文件为准；无注册表接入、异步资源源和原有 Unity 适配器继续可用。服务端需要完整候选校验时使用 ReloadAll / ReloadAllAsync，不应仅发布尚未读取的数据清单。

## 服务器契约家族与显式加载

生成的目录现在使用 Catalog 格式 2，携带 `server-v1` 运行时契约、目标端和必需能力列表。七种运行时在创建目录时校验能力；未知能力拒绝为 `UnsupportedCapability`。只理解 Catalog 格式 1 的旧运行时会拒绝格式 2，不会静默忽略新要求。数据文件仍沿用既有二进制 v4 / JSON v1；这次目录元信息升级不改变行编码。

新生成的服务器目录严格检查清单 `target`。目标端不符或缺失返回 `CatalogMismatch`，即使表名和 SchemaHash 一致也不会读取表文件。C# 自定义清单解码器需要保留 `ConfigManifest.target`；原来的通用注册表与旧 Catalog 构造方式继续保留其既有行为。

清单和表 JSON 都在构建解析树前检查对象成员、集合、字符串和嵌套预算。表 JSON 的解析上限包含字段名、多态标签、矩阵维度及字典键值对的编码开销，随后再次检查业务字段的精确限额；`MaxContainerElements` 累计约束单行，而非仅约束每个子集合。错误路径使用从零开始的元素下标，矩阵使用行优先展开下标，字典使用 `[序号].key` / `[序号].value`，例如 `Mapping[0].value`。这只影响诊断，Lua 业务集合仍沿用其原有下标方式。

新项目默认生成 `server-v1`：保留已有消费入口，额外生成独立的服务器入口。已有项目省略 `--runtime-family` 时继承已接受家族；迁移项目显式使用 `--runtime-family server-v1`，再按前文流程完成兼容验收。`legacy-v1` 保持旧消费接口。以下服务器行为指新增入口；本文传统 `ConfigManager` 示例仍遵循各自旧契约。

| 语言 | 服务器入口 | 显式按行加载示例形式 | 已缓存查询示例形式 |
| --- | --- | --- | --- |
| C# | `ZrConfig.Runtime.Server.Manager`、生成的 `ServerCatalog` | `snapshot.GetEntryTable().Load("east", 1)` | `snapshot.GetEntryTable().Get("east", 1)` |
| Java | `DataSnapshot`、`ConfigRuntime.ServerManager` | `snapshot.get_Entry().load("east", 1, cancellation)` | `snapshot.get_Entry().get("east", 1)` |
| Go | `DataSnapshot`、`ServerManager[DataSnapshot]` | 取得 `Get_Entry()` 表后调用 `Load(ctx, "east", 1)` | 取得表后调用 `Get("east", 1)` |
| C++ | `zrconfig::data::Snapshot`、`ServerManager` | `snapshot->load_Entry("east", 1, cancellation)` | `snapshot->get_Entry("east", 1)` |
| Rust | `data::Snapshot`、`runtime::ServerManager` | `snapshot.load_Entry("east", 1, &cancellation)` | `snapshot.get_Entry("east", 1)` |
| TypeScript | `data.Snapshot`、`runtime.ServerManager` | `snapshot.get_Entry().load("east", 1)` | `snapshot.get_Entry().get("east", 1)` |
| Lua | `data.load`、`server.manager` | `snapshot.get_Entry().load('east', 1, cancellation)` | `snapshot.get_Entry().get('east', 1)` |

表名、主键、分组名来自各自项目；表内例子对应验收夹具 `Entry` 的 `(Zone, Id)` 组合键。业务直接使用生成的具体类型，不必用字符串表名查询。

四种加载模式在七种语言中采用相同范围语义：`ServerDefault` 默认全量；`SelectedTables` 加载指定表及引用依赖闭包；`LazyRowsStrict` 固定所选数据并校验所有行，业务行按需创建；`LazyRowsOnAccess` 固定数据、建立键索引，非主键数据在访问时完整校验。懒加载保留原始二进制或 JSON 行片段，不等同于从磁盘随机读取一行。

`Get`／`Find` 只查缓存，不隐式读源或解码；存在但尚未加载的行报告 `RowNotLoaded`，范围外表报告 `TableNotSelected`。`Load`、整表加载和分组加载复用同一行缓存。首次分组加载可能扫描整表来建立分组索引，然后只创建命中组的业务行；这一步有冷加载成本。引用字段先暴露 Id，`Get` 引用只取已经加载的目标，`Load` 引用才显式加载目标。引用环不会自动递归展开。

`Prepare` 完成固定数据、选定校验和业务校验后产生候选，`Commit` 才发布。`Clear` 清除当前入口并使在途候选失效；`Close` 还禁止后续获取和更新。新的安全行／表／分组视图持有所属快照，更新、Clear、Close 后仍可继续读取原版本；不要把传统 C++ 借用引用或旧 Rust 记录的行为套用到这些安全视图上。

C#、Java、TypeScript 提供相应异步入口。Java 使用业务提供的 Executor；TypeScript 使用显式调度器并让出事件循环，异步方法名本身不保证所有解码都已移到 Worker。Go 使用 context 与 goroutine；C++、Rust 的阻塞工作由业务执行器调度；Lua 在单个状态中运行。单个等待者取消不取消其他等待者共享的行工作。低层源忽略取消时，其任务仍计入准备任务上限，直到实际退出。

### 源名称映射与列移动

生成器从已接受的 `.schema.json` 继承字段顺序和组合键参数顺序。移动 Excel 列不会交换同类型参数的业务含义，也不会改变已有表的编码顺序；改变实际主键字段集合仍属于迁移。所有字段的列定位仍取当前工作簿，数据不会按旧列号读取。

显式源名映射使用 `codegen --source-map <JSON 文件>`。例如，将原 `Lookup` 工作表改成 `Targets`，将 `Entry.Name` 的 Excel 根字段改成 `DisplayName`：

```json
[
  { "Table": "Lookup", "Field": null, "SourceName": "Targets" },
  { "Table": "Entry", "Field": "Name", "SourceName": "DisplayName" }
]
```

`Table`、`Field` 始终写已接受的逻辑名称，`SourceName` 写当前 Excel 名称。公开类型、方法、文件、协议名称和 SymbolId 保持原身份，业务继续使用 `Lookup` 和 `Name`。表引用输入可随源表写成 `TargetsConfig`，生成器将它绑定到原 `LookupConfig`。字段映射作用于根字段，也适用于该根下的展开列；不会重写自定义类型内部成员或单元格 JSON 内容。

该参数只提交映射候选，不修改 Excel。预览会阻止普通 `--accept-changes`；在同一命令中保留 `--source-map`，按现有 `--verify-api`、`--migration-receipt`、`--accept-api-change` 流程验收并接受精确候选。接受后，映射只存入唯一 API 契约元数据，后续生成自动继承，无需保留临时映射请求文件。生成程序集包含供 DLL 导出使用的只读副本，源码导出和 DLL 导出使用相同映射。

再次改名仍使用原逻辑名称直接绑定新源名，不形成别名链。将 `SourceName` 写回原逻辑名称会请求移除该映射，也需迁移验收。映射到不存在的旧身份、重复目标、占用另一已接受名称或没有匹配输入都拒绝。该机制保留公开名称；需要新的公开类型名或字段类型时，继续按 API 迁移处理。

### 快照身份与诊断

服务器快照分别暴露源身份、版本、原始清单摘要、Catalog 指纹及加载计划指纹；它们共同组成缓存域身份，行与引用缓存仍只属于单个快照实例。管理器的版本冲突历史只比较“源 + 版本 → 清单摘要”，不能将加载方式变化误判成数据版本被覆盖。旧的三字段 `SnapshotIdentity` 解析器用于这项冲突检测，不是跨快照共享行缓存的键。

`CatalogFingerprint` / `catalogFingerprint` / `catalog_fingerprint` 来自生成目录；C# 使用旧注册表接入时按完整清单表名和结构哈希生成独立 legacy 目录身份。`LoadPlanFingerprint` / `loadPlanFingerprint` / `load_plan_fingerprint` 固定为 `zrconfig-load-plan/1`：SHA-256 输入依次为版本行、模式名称、`binary` 或 `json-preferred`、选中依赖闭包的 UTF-8 表名小写十六进制（按字节排序、换行分隔，末尾无额外换行）。相同闭包不因请求顺序改变；模式或格式偏好变化产生不同指纹。预算、调度器、诊断开关与预热只影响准入或缓存就绪度，不改变这个数据解释身份；不得用此指纹代替完整准备参数的审计记录。

`CompletedTableCount` / `completedTableCount` / `completed_table_count` 统计行和该表必需分组索引全部就绪的表数，逐行取遍记录不自动等同于整表完成。计数读取不触发加载。管理器的 `StageTimings` / `stageTimings()` / `stage_timings()` 返回固定两个记录：最新操作编号的已完成 Prepare（成功或失败）和最近成功 Commit。每项包含阶段、操作编号、毫秒和成功标记；编号 0 表示尚无记录。晚完成的旧准备不会覆盖较新操作，回调重入可以读取失败计时及已归还的准备名额。

计时在冷路径采集，不为热查询增加计时或日志。C#、Java、Go、C++、Rust、TypeScript 使用单调经过时间，包含源等待、验证器和调度等待；Lua 使用 `os.clock()` 的进程 CPU 时间，不包含阻塞等待，宿主提供的期限时钟另行使用。Commit 计时截止发布点，排除旧对象销毁与通知回调；拒绝提交不覆盖最近成功 Commit。这里只是生命周期阶段时间，不将其称为独立 I/O、解码或 GC 分析器结果。查询计数、预算估算和实际内存测量的口径分别保留。

### 活快照的内存准入

解码行数、解码估算字节、派生索引估算字节、源读取字节、并发工作和等待者均有独立限额。`MaxEstimatedLiveBytes` 还用于阻止旧快照长期持有期间继续增加更新高峰；它是配置内存估算，不是 RSS 或进程堆的硬上限。准备前、候选完成后及提交前检查，业务需要将源读取和单快照预算同时收紧。

| 语言 | 配置方式 | 旧快照跟踪 |
| --- | --- | --- |
| C# | `PreparationOptions` 的 `ResourceLimits(maxEstimatedLiveBytes: ...)` | 弱引用；重新读取快照估算 |
| Java | `ManagerPolicy.withEstimatedLiveBudget(maximum, estimator)` | `WeakReference`；估算器在管理锁外执行 |
| Go | `ManagerPolicy.MaxEstimatedLiveBytes` | 独立叶子 GC 标记，只更新回收计数，不负责资源释放 |
| C++ | `ManagerPolicy.with_estimated_live_budget(maximum, estimator)` | `weak_ptr`；生成快照可自动使用诊断估算 |
| Rust | `ManagerPolicy.with_estimated_live_budget(maximum, estimator)` | `Weak`；估算器返回 `ServerResult<u64>` |
| TypeScript | `maxEstimatedLiveBytes`、`liveEstimator` | `WeakRef`，要求快照是对象 |
| Lua | `max_estimated_live_bytes`、`live_estimator` | 弱键表 |

分组索引默认有独立的 2 GiB 估算预算。C# 使用 `ResourceLimits.WithIndexBudget(bytes)` 返回新配置；Java 使用 `Builder.maxEstimatedIndexBytes`；Go 使用 `MaxEstimatedIndexBytes`；C++/Rust 使用 `max_estimated_index_bytes`；TypeScript 使用 `maxEstimatedIndexBytes`；Lua 使用 `max_estimated_index_bytes`。构建前预留，成功后由快照持有，异常与取消导致构建失败时归还；已发布缓存的重复查询不重复计费。`EstimatedIndexBytes` / `estimatedIndexBytes` / `estimated_index_bytes` 提供诊断。C# 此诊断还包含基础行索引估算，其余语言的 `declaredRows × 64` 单独进入总估算。

派生索引在构建前按表固定原始字节的 8 倍，加每行每键分量 512 字节和 256 字节固定开销预留工作区；构建时复用已经完整缓存的行，未缓存行仍临时解码且不填充行缓存；成功后归还临时额度，保留固定开销、每行每键分量额度和字符串键长度的 4 倍。UTF-16 语言使用代码单元数，其他语言使用 UTF-8 字节数；重复字符串仍保守计入。C# 生成的 GroupDescriptor 带有键分量及字符串估算，自定义描述可使用 `WithIndexEstimate` 提供相应元数据。此估算可能高于或低于实际堆占用，不能代表分配器实测值。严格验证与分组扫描的临时行解码同时占用解码字节预算和解码并发槽，完成后归还，不占用缓存行数额度。自定义对象和估算器仍需按实际业务测量；所有限制都是有边界的准入估算，不能替代进程内存限制。TypeScript 同步加载与异步索引构建交错时共用构建进度、工作区和一次预算预留；同步调用可完成剩余工作，异步等待者取得同一已发布索引。

原生管理器默认不开启此项准入预算；开启时有界跟踪快照，默认最多 1024 个记录，并在准入时移除已回收记录。Java、C++、Rust、TypeScript、Lua 的自定义估算器必须快速、无副作用，不应捕获它正在估算的快照集合。Go 生成快照自动维护 `SnapshotMemory`；自定义快照须实现同名方法，稳定返回自己持有的 GC 标记，可通过 `NewSnapshotMemory` 和 `SetEstimatedBytes` 更新估算。此标记不得复制，业务视图仍须持有所属快照。Manager 只保留计数状态，不因此保留原始数据图。

GC 语言的额度归还可能晚于业务释放引用；生产运行库不主动强制 GC。C++、Rust 在最后强引用释放后可立即观察到跟踪对象消失。预算拒绝不使已有句柄失效，也不替换当前版本。诊断中的行失败与索引失败分别计数，任一出现确定性数据故障时健康状态为 `Degraded`；失败缓存中的错误在公共边界不能被调用者改坏。

## 并发、性能与生命周期

本节及后面的 Lua / TypeScript / Rust 代码演示的是保留的传统同步全量入口。`server-v1` 的显式加载、安全句柄和 Prepare / Commit 行为以 [服务器契约家族](#服务器契约家族与显式加载) 一节为准。

Java、Go、C++ 在加载阶段完成全部表解析、建索引和引用绑定，成功后原子发布新快照。请求或战斗开始时读取一次 current 并持有快照，中途不反复读取管理器 current。未初始化时 Java current 抛出异常，Go/C++ current 返回 nil/空 shared_ptr，因此应先成功加载再接收业务请求。加载失败保留当前快照；重载串行执行，读查询不参与重载锁。旧快照不放入管理器历史列表，由业务释放引用后回收。

C++ 的记录/集合/表引用属于快照，返回指针不延长其生命周期，必须保留 shared_ptr 快照。Java/Go 返回的记录保持只读访问；不要用反射或同包内部访问修改它们。C# 选择 retainPreviousSnapshots 后，退休快照脱离管理器，Dispose 管理器也不会使已经退休且被持有的快照失效；默认行为仍是更新后旧快照失效。

正常命中的主键、组合键、分组和已绑定引用查询复用记录与索引，不拼字符串键、不临时创建组合键对象、不复制集合。Java/C++ 使用保存行号的开放寻址索引；Go 使用可比较结构体键的 map。Decimal 的规范化与哈希计算发生在构造阶段。需要大整数、Decimal、字符串键时，业务应复用键对象；在查询表达式里解析字符串或 new 键对象的分配属于调用方。

加载、JSON 解析、集合/索引构建和重载会分配内存；本实现不宣称整个加载过程零 GC。JSON 的体积和加载临时内存通常高于二进制，C# JSON 加载还包含转码成本。旧快照持有越久，重载期间保留的内存越多。调用显式 Copy、格式化、异常路径或业务 LINQ/stream 也可能分配。

## 验收范围

本地自动化覆盖七种语言源码编译或解释执行、两种数据读取、64 位/decimal 边界、Unicode、空值、数组、字典、矩阵、枚举、多态、组合主键、分组、循环表引用、失败重载、旧快照保留，以及热查询分配。完整正式示例的九张表（含一万行全类型表）也用于生成、编译和双格式读取。

公开使用者先运行 [公开服务器验收](../tests/README.md#公开服务器验收)。完整维护工作区另有开发测试命令：`tests/ZrConfig.Tests` 的 `--portable-native` 强制要求全部六种服务端工具链，可用 ZRCONFIG_JAVAC、ZRCONFIG_GO、ZRCONFIG_CXX、ZRCONFIG_NODE、ZRCONFIG_LUA、ZRCONFIG_RUSTC 指定可执行文件；TypeScript 使用 ZRCONFIG_TSC 指定编译器，或使用 ZRCONFIG_TYPESCRIPT 指定 typescript.js 模块。`--portable-only` 在工具链不全时仍运行 .NET 与生成/数据契约测试，并明确报告跳过原生编译。项目完整入口仍是 `bash tests/smoke.sh`。公开源码包包含独立的 `tests/ZrConfig.Server.Tests`，默认要求所有原生工具链；`--managed-only` 明确只完成托管部分，不能作为七语言验收。受保护分支的 API 检查另用 `--trusted-ref <不可变提交 SHA>` 比较可信基线。

当前最低目标为 Java 17、Go 1.19、C++17、Lua 5.4、Node 22 / TypeScript 5.8、Rust 1.70；实测编译器版本和分配记录应随验收报告保存。macOS 上通过编译和运行不代表已经完成 Windows/Linux 的原生运行测试、Unity Player 或实际业务服务器压力测试。

## Lua、TypeScript、Rust 的接入差异

新增语言复用同一结构与数据文件，不引入新的 Excel、Rules 或 Output 目录。`--languages all` 包含全部七种语言，仍可用逗号列表选择子集。

TypeScript 提供只读 interface、枚举常量、只读集合和查询表。运行 `tsc -p Config/Generated/TypeScript` 后，在 Node 后端导入生成的 snapshot.js。行与数组会冻结，字典仅提供读取 API；64 位整数均为 bigint，decimal 为精确 Decimal。组合键通过多级 Map 按分量查询，不分配键数组。

```ts
import { ConfigManager } from './Generated/TypeScript/snapshot';
const manager = new ConfigManager();
const snapshot = manager.reload(output, true);
const item = snapshot.ItemCatalog.get(1001);
// item.Id、item.Name 为只读字段，引用直接指向同快照记录。
```

Lua 使用 LuaLS/EmmyLua 注解描述表和自定义结构，运行时保持动态类型。使用 `require('你的模块前缀.snapshot')`，同目录模块自动复用该前缀；也可将生成目录加入 package.path 后 `require('snapshot')`。Lua 5.1/LuaJIT 不能直接执行此 Lua 5.4 运行库。加载不调用协程让出，同一 Lua 状态内成功后一次替换快照；多个 Lua 状态各自持有管理器。

```lua
local Config = require('zrconfig.snapshot')
local manager = Config.manager()
local snapshot = manager.reload(output, true)
local item = snapshot.ItemCatalog.get(1001)
-- 用点调用查询；缺失主键由 find 返回 nil，get 抛错。
```

Lua 的 long 使用 64 位整数，ulong（包括其枚举）使用十进制字符串；decimal 使用规范化的精确十进制字符串，查询时复用 `Config.decimal('1.25')` 的结果。JSON null 和空引用使用 `Config.NULL`，保留数组位置和长度，不能按 nil 判断。数组下标从 1 开始；字典使用 `.get(key)`，矩阵使用 dimensions / values。只读代理阻止普通赋值，禁止业务使用 rawset、debug 或修改模块内部状态绕过只读约定。冻结引用图使用迭代处理，长链和循环引用不会递归耗尽 Lua 调用栈。

Rust 生成私有字段和 `get_字段名()` 方法；表使用 `get` / `find`、分组使用 `get_分组名`，快照使用 `get_表名`。普通查询返回借用，分组返回借用的 Group 视图。枚举使用显式值类型及关联常量，decimal 保存精确整数系数和小数位，比较和哈希不格式化字符串。

```rust
let manager = zrconfig_generated::ConfigManager::new();
let snapshot = manager.reload(std::path::Path::new(output), true)?;
let item = snapshot.get_ItemCatalog().get(1001)?;
let name = item.get_Name();
```

Rust 管理器用 Mutex 串行重载，RwLock 保护快照引用的发布与获取；业务取得 Arc 后，查询不经过管理器锁。表引用预绑定 Weak，读取时升级为 Arc，不产生堆分配，循环引用不会阻止回收。**业务须持有整个 Arc 快照覆盖引用使用期**；只克隆单条记录不能保证其他表记录仍存活，目标释放后引用返回 None。生产运行库不使用 unsafe。

TypeScript 的 reload 为同步加载，成功后替换同一事件循环中的引用；多个 Worker 各自管理快照。大表同步加载会占用事件循环，应在初始化或业务允许的加载时机进行，不能当作无阻塞异步 I/O。TypeScript 堆差值测试包含引擎和测量开销，仅用于检查持续增长，不能等同精确分配计数。Lua 暂停 GC 测量热查询堆变化，Rust 使用全局分配器计数，两者均只测已加载配置的查询。

语言运行时依据：[Lua 5.4 手册](https://www.lua.org/manual/5.4/manual.html)、[Node TypeScript 说明](https://nodejs.org/api/typescript.html)、[Rust 同步类型](https://doc.rust-lang.org/std/sync/)。Node 的类型剥离不会执行类型检查，验收必须另外运行 TypeScript 严格编译。

### 可重复的性能测量

公开源码包包含独立入口 `tests/ZrConfig.Server.Tests/ServerRuntimeBenchmarks.cs`。配置对应的 `ZRCONFIG_*` 工具路径后，以 Release 构建运行：

```bash
bash scripts/dotnet.sh build tests/ZrConfig.Server.Tests -c Release -p:NuGetAudit=false
bash scripts/dotnet.sh tests/ZrConfig.Server.Tests/bin/Release/net10.0/ZrConfig.Server.Tests.dll --benchmark artifacts/performance/latest 10000 3
```

`--benchmark-scenarios <报告目录> 3` 使用临时 1,000 行输入，比较正常源、每次读取延迟 1 ms、第二次读取失败，以及线程运行时的 1／4 个热读者。每个子进程都核验查询校验和、失败后的快照身份与代数。线程先到达屏障再计时，耗时包含唤醒和 join；TypeScript/Lua 只测单状态，不声称 Worker 或多状态并行。Lua 没有内置睡眠，延迟源明确使用 CPU 忙等，其结果不代表阻塞 I/O。额外场景不混入普通冷加载和保留快照样本。

`--benchmark-retained <报告目录> 3` 使用 0、1、4 份独立保留快照检查加载峰值与释放成本；每份使用同一不可变数据版本，完整模式和按行模式分别采样。它隔离快照所有权的内存代价，管理器退休与失败回滚由独立回归验证。

`--benchmark-queries <报告目录> 3` 分别测主键命中、主键缺失、字符串与整数的组合键、已建立分组、已解析引用和管理器快照获取。每项先预热 20,000 次，再测 1,000 批、每批 100 次，校验实际结果的总和；命中在两个键间交替，缺失使用变化的负数键，分组交替查询已存在组和空组。记录 p50/p95/p99 批次均值及可取得的分配计数或堆趋势。计时包含测量回调、边界检查与安全句柄所有权成本，不声称是裸哈希查询或逐请求尾延迟。主键、组合键、分组、引用和获取快照分别保存记录，不能用其中一项代表全部热路径。

`--benchmark-stages <报告目录> <仓库根目录> 3` 在临时生成物及临时 C# Runtime 副本中插入计时，测量 read、hash、manifest、scan、decode、index、reference、commit、plan 九种函数路径。每个函数标记必须唯一匹配；缺失阶段、未调用阶段或负时间直接失败。原始与插桩源码摘要随报告保存。结果是包含子调用的时间，不能相加，也不能当成无插桩吞吐。scan 包含结构扫描及部分主键处理；decode 为行解析，C# 与 Lua 另包含相应构造路径；index 依语言包含基础或派生索引构建。原生临时行值转换与完整类型构造并非完全相同的边界，跨语言比较必须使用总加载时间。正式生成物不参与插桩。

`--benchmark-control <报告目录> 3` 将首次目录/模块初始化与加载分开记录，并测量 10,000 节点长链、256 节点完全有向环、10,000 张独立表的 Catalog 解析和选择闭包；每个图重复选择 20 次并校验结果。生成器额外保存三次 SCC 构建时间。该输入测目录规模，不声称生成并全量解码 10,000 张业务表。每个语言/格式进程还在第二次数据源读取处发出取消，连续 100 次记录取消到准备退出（含清理和不变量检查）的延迟，核对当前快照、代数和任务清理。支持线程的语言以屏障启动四个读者和一个更新者，执行 40,000 次获取与 100 次提交；TS/Lua 使用单状态交错。C# 更新包含真实准备 I/O，其余语言复用已准备快照来隔离管理器成本，不能直接按该耗时排列语言快慢。旧快照经过全部更新、Clear、Close 后继续读取。

`dotnet run --project tests/ZrConfig.Server.Tests -c Release -- --benchmark-table-scale <报告目录> 10000 3` 单独执行万表业务夹具的完整生成、双格式导出、重复生成稳定性检查、七语言编译和加载。它与目录图测量分别报告；保存 API 基线体积、编译产物及每种模式实际加载结果。`ZRCONFIG_BENCH_TIMEOUT_SECONDS` 设置每个子进程的超时，默认 300 秒，允许 1–3600 秒；万表编译可使用 3600 秒。`ZRCONFIG_BENCH_KEEP_SCALE=1` 保留系统临时工作区，供检查失败产物；`ZRCONFIG_BENCH_TABLE_WORKBOOK` 可指向上次临时生成的 `TinyTables.xlsx`，避免重建相同测试工作簿。最终表数量与各加载模式的结果仍须通过检查，不能用保留工作区或延长超时替代验收。

万表 TypeScript 类型检查可能超过 Node 默认堆上限。性能入口可设置 `ZRCONFIG_TYPESCRIPT_HEAP_MB=16384`，仅给 TypeScript 编译子进程传入 `--max-old-space-size=16384`，并将选项写入构建报告；加载与查询进程不继承这项编译参数。未设置时保留 Node 默认值，允许范围为 64–65536 MiB，非法值在创建夹具前拒绝。堆上限不是整个编译进程的内存硬上限，须同时查看实际峰值 RSS。

大型 Catalog 的内部生成实现按语言拆分：Java 避免单个类及初始化方法超过字节码限制；Go 将私有快照存储和冷加载函数分段，避免泛型类型描述随全部表字段膨胀；Rust 将私有存储放入固定大小的堆上分段，并隔离根快照的具体存储类型，限制析构分析、初始化、验证和名称分派的规模；TypeScript、Lua 限制单个初始化函数的大小；C++ 共用前置声明、行加载调度与所有权实现，并复用同类键的私有索引模板及预热可调用类型。当前切分阈值为 256 表，采用分段的路径通常每段 128 表。阈值、辅助类型、字段布局和分段名称属于内部实现，业务仍使用公开的表、行、分组和加载入口。跨越切分阈值也必须经过实际声明比较及冻结消费方验收，不能因优化目的绕过兼容门禁。

Go 的万表编译报告还记录消费方模块名。最低工具链的对象文件有 32 位偏移限制；编译对象会重复包含模块路径和泛型类型名称，因此路径、名称长度、索引数量都会影响可编译规模。固定模块名用于消除随机临时路径带来的测量偏差，不代表任意业务路径和任意规模均可通过；编译对象、最终程序体积和运行时内存分别报告。

`--benchmark-locks <报告目录> <仓库根目录> 3` 另用临时 Runtime 副本运行上述控制场景。C#、Java、Go、C++、Rust 记录管理器状态锁的获取次数、累计获取时间及最长获取时间；包含无竞争的锁操作和计时开销，不是纯阻塞时间，不包含行锁、索引锁或事件队列锁。采样原子计数也会改变竞争条件。TS/Lua 单状态标为不适用。C# 使用独立预编译消费方执行插桩样本；不能与基线动态编译工作进程的 RSS 直接比较。

另可使用 `--benchmark-samples <报告目录> <仓库根目录> 3` 测量正式九张表：只复制 `samples/Data` 和共用 Config 的手写源码到系统临时目录，记录工作簿摘要，不改写正式数据。`--sample-consumer <语言|all> <Config目录> <Output目录>` 单独编译并运行正式示例的冻结旧接口消费方，可用于 API 迁移凭据；候选本身不会被改写。

性能命令使用临时工作簿和七种语言的独立进程，保存原始 `server-runtime.json`。报告包含工具链、构建时间、二进制/JSON、全量/选择/按行加载、1%/10%/100% 覆盖率、首行与分组时间、热查询批次分布，以及各语言可实际取得的分配/堆/释放统计。macOS/Linux 的 `ZRCONFIG_BENCH_TIME=1` 使用系统资源统计记录整个子进程的峰值；未取得的指标为 `null`。报告内说明计时、分位数和内存口径，不把堆差值当作精确分配计数，也不把首次基线当作已经通过回归门槛。

每个样本进程独立，但不保证操作系统文件缓存为冷。C# 首个工作进程动态编译消费方，后续进程复用该临时输入的磁盘编译缓存；`cold_ms` 均排除这部分准备，整个进程的耗时和 RSS 包含它。首次编译峰值与后续运行峰值须分开解读，不能把它们当作相同条件下的配置内存。运行时诊断中的预算估算也不能代替实际堆或配置专属 RSS。
