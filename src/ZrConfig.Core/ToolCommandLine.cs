using ZrConfig.Runtime;

namespace ZrConfig.Core;
/// <summary>App-independent argument validation for the CLI.</summary>
public sealed record ToolCommandLine(string Command,
    string[] Paths,
    bool RulesDebug,
    bool AcceptChanges,
    bool Help,
    string? ProjectFile = null,
    bool Generate = false,
    ConfigTarget? Target = null,
    string? ReportFile = null,
    string? DataFormat = null,
    string[]? CodeLanguages = null)
{
    public string? ApiChangeId { get; init; }
    public string? ApiVerificationFile { get; init; }
    public string? MigrationReceiptFile { get; init; }
    public string? RuntimeFamily { get; init; }
    public string? SourceMappingsFile { get; init; }
    public bool PruneLanguages { get; init; }
    public string PathAt(int index, string fallback) => Paths.Length > index ? Paths[index] : fallback;
    public static ToolCommandLine Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return new("help", [], false, false, true);
        }

        string command = args[0].ToLowerInvariant();
        if (command is "help" or "-h" or "--help")
        {
            if (args.Length != 1)
            {
                throw Error("帮助命令不接受其他参数。", "Help does not accept extra arguments.");
            }

            return new("help", [], false, false, true);
        }

        int maximum = command switch
        {
            "codegen" => 2,
            "support" => 1,
            "export" => 4,
            "validate" => 3,
            _ => throw Error($"未知命令：{args[0]}", $"Unknown command: {args[0]}")
        };
        if (args.Length == 2 && args[1] is "--help" or "-h")
        {
            return new(command, [], false, false, true);
        }

        List<string> paths = [];
        bool debug = false;
        bool accept = false;
        bool generate = false;
        bool pruneLanguages = false;
        bool literal = false;
        string? project = null;
        string? report = null;
        ConfigTarget? target = null;
        string? dataFormat = null;
        string[]? languages = null;
        string? apiChangeId = null;
        string? apiVerification = null;
        string? migrationReceipt = null;
        string? runtimeFamily = null;
        string? sourceMappings = null;
        for (int i = 1; i < args.Length; i++)
        {
            string argument = args[i];
            if (!literal && argument == "--")
            {
                literal = true;
                continue;
            }

            if (!literal && argument == "--source-map")
            {
                if(command!="codegen"||sourceMappings!=null||i+1==args.Length||string.IsNullOrWhiteSpace(args[i+1])||args[i+1].StartsWith('-'))
                    throw Error("--source-map 仅用于 codegen，需要唯一映射文件。","--source-map requires one mapping file and is only valid for codegen.");
                sourceMappings=args[++i];continue;
            }
            if (!literal && argument == "--runtime-family")
            {
                if (command == "support" || runtimeFamily != null || i + 1 == args.Length)
                    throw Error("运行时家族参数缺失、重复或不适用。", "Missing, duplicate or unsupported runtime family option.");
                runtimeFamily = GenerationFamily.Validate(args[++i]);
                continue;
            }
            if (!literal && argument is "--format" or "--languages")
            {
                if (command == "support" || i + 1 == args.Length || (argument == "--format" ? dataFormat != null : languages != null))
                    throw Error("语言/格式参数缺失、重复或不适用。", "Missing, duplicate or unsupported language/format option.");
                if (argument == "--format") dataFormat = ExportOptions.Format(args[++i]);
                else languages = ExportOptions.Languages([args[++i]]);
                continue;
            }

            if (!literal && argument == "--report")
            {
                if (command is not ("validate" or "export") || report != null || i + 1 == args.Length || string.IsNullOrWhiteSpace(args[i + 1]) || args[i + 1].StartsWith('-'))
                {
                    throw Error("--report 仅用于校验/导出，需要报告路径，且不能重复。", "--report requires one path, is valid for validate/export, and cannot be repeated.");
                }

                report = args[++i];
                continue;
            }

            if (!literal && argument == "--accept-api-change")
            {
                if (command != "codegen" || apiChangeId != null || i + 1 == args.Length ||
                    args[i + 1].Length != 64 || !args[i + 1].All(c => c is >= '0' and <= '9' or >= 'A' and <= 'F'))
                    throw Error("--accept-api-change 仅用于 codegen，需要预览提供的完整 64 位大写摘要，且不能重复。",
                        "--accept-api-change is codegen-only and requires the full 64-character uppercase preview digest exactly once.");
                apiChangeId = args[++i];
                continue;
            }

            if (!literal && argument is "--verify-api" or "--migration-receipt")
            {
                if (command != "codegen" || i+1 == args.Length || string.IsNullOrWhiteSpace(args[i+1]) || args[i+1].StartsWith('-') ||
                    (argument == "--verify-api" ? apiVerification != null : migrationReceipt != null))
                    throw Error("API 迁移参数仅用于 codegen，必须指定唯一文件路径。", "API migration options require a single file path and are codegen-only.");
                if (argument == "--verify-api") apiVerification=args[++i]; else migrationReceipt=args[++i];
                continue;
            }

            if (!literal && argument == "--target")
            {
                if (command == "support" || target != null || i + 1 == args.Length)
                {
                    throw Error("--target 仅用于生成/导出，需要 C 或 S，且不能重复。",
                        "--target requires C or S, is valid only for codegen/export and cannot be repeated.");
                }

                target = ConfigTargets.Parse(args[++i]);
                continue;
            }

            if (!literal && argument == "--project")
            {
                if (project != null || i + 1 == args.Length || string.IsNullOrWhiteSpace(args[i + 1]) || args[i + 1].StartsWith('-'))
                {
                    throw Error("--project 需要一个项目配置文件路径，且不能重复。", "--project requires one project settings file and cannot be repeated.");
                }

                project = args[++i].Trim();
                continue;
            }

            if (!literal && argument.StartsWith('-'))
            {
                if (argument == "--rules-debug" && command is "export" or "validate" && !debug)
                {
                    debug = true;
                }
                else if (argument == "--generate" && command == "export" && !generate)
                {
                    generate = true;
                }
                else if (argument == "--prune-languages" && command == "codegen" && !pruneLanguages)
                {
                    pruneLanguages = true;
                }
                else if (argument == "--accept-changes" && command is "codegen" or "export" && !accept)
                {
                    accept = true;
                }
                else
                {
                    throw Error($"未知、不适用或重复的参数：{argument}。请使用 {command} --help 查看用法。",
                        $"Unknown, unsupported or duplicate option: {argument}. Use {command} --help.");
                }
            }
            else
            {
                paths.Add(argument.Trim());
            }
        }

        if (paths.Count > maximum)
        {
            throw Error($"{command} 最多接受 {maximum} 个路径，收到 {paths.Count} 个。",
                $"{command} accepts at most {maximum} paths; received {paths.Count}.");
        }

        if (command == "export" && accept && !generate)
        {
            throw Error("export 的 --accept-changes 必须与 --generate 一起使用。", "export --accept-changes requires --generate.");
        }

        if (project != null)
        {
            if (paths.Count != 0)
            {
                throw Error("--project 不能与位置目录参数混用；请修改项目配置或只传目录。",
                    "Do not combine --project with positional directories; edit the project or use directories alone.");
            }

            ValidateMigration();
            return new(command, [], debug, accept, false, project, generate, target, report, dataFormat, languages) { PruneLanguages=pruneLanguages, SourceMappingsFile=sourceMappings, ApiChangeId = apiChangeId, RuntimeFamily = runtimeFamily, ApiVerificationFile=apiVerification, MigrationReceiptFile=migrationReceipt };
        }

        if ((command is "codegen" or "validate" && paths.Count == 1) || (command == "export" && paths.Count is 1 or 2))
        {
            throw Error("目录参数不完整：codegen <Data> <ConfigRoot>；export <Data> <Output> <ConfigRoot> [Rules]。也可使用 --project <文件>。",
                "Incomplete directories: codegen <Data> <ConfigRoot>; export <Data> <Output> <ConfigRoot> [Rules]. Or use --project <file>.");
        }

        if (command == "support" && (paths.Count != 1 || paths[0].Length == 0))
        {
            throw Error("请指定 Config 根目录：support <ConfigRoot>。", "Specify the Config root: support <ConfigRoot>.");
        }

        // Only the optional rules path may be explicitly empty.
        if (paths.Take(command == "export" ? 3 : command == "validate" ? 2 : maximum).Any(string.IsNullOrWhiteSpace))
        {
            throw Error("目录参数不能为空；只有可选 Rules 可以留空。", "Directory arguments cannot be empty; only optional Rules may be blank.");
        }

        ValidateMigration();
        return new(command, paths.ToArray(), debug, accept, false, Generate: generate, Target: target, ReportFile: report, DataFormat: dataFormat, CodeLanguages: languages) { PruneLanguages=pruneLanguages, SourceMappingsFile=sourceMappings, ApiChangeId = apiChangeId, RuntimeFamily = runtimeFamily, ApiVerificationFile=apiVerification, MigrationReceiptFile=migrationReceipt };

        void ValidateMigration()
        {
            if (apiVerification != null && (migrationReceipt == null || apiChangeId != null) || migrationReceipt != null && apiVerification == null && apiChangeId == null)
                throw Error("--verify-api 需要 --migration-receipt，且与接受迁移分开执行；凭据只能用于验证或精确接受。", "Verification requires a receipt output and runs separately from acceptance; a receipt must accompany verification or exact acceptance.");
        }
    }

    private static ArgumentException Error(string chinese, string english) => new(ConfigText.Get(chinese, english));
}
