using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ZrConfig.Core;

/// <summary>各语言只接收已分端的不可变结构；生成源码与运行库通过同一生成事务提交。</summary>
internal sealed partial class NativeGenerator
{
    private readonly GeneratedSymbolPlan _names;
    private readonly Dictionary<string, string> _referenceTables = new(StringComparer.Ordinal);
    private NativeGenerator(GeneratedSymbolPlan names) => _names = names;
    internal static void Generate(PortableTable[] tables, string[] languages, Dictionary<string, string> files, GeneratedSymbolPlan names) => new NativeGenerator(names).GenerateCore(tables, languages, files);
    private void GenerateCore(PortableTable[] tables, string[] languages, Dictionary<string, string> files)
    {
        var types = Types(tables);
        foreach (var type in types.Values.Where(t => t.Kind == "tablereference"))
            _referenceTables.Add(type.Id, type.ReferenceTable ?? throw new NotSupportedException("引用类型缺少目标表：" + type.Id));
        foreach (var collision in types.Values.Where(t => t.Kind is "enum" or "object" or "converter" or "kv" or "vector2" or "vector3").GroupBy(t => Name(t.Id)).Where(g => g.Count() > 1))
            throw new NotSupportedException("目标语言类型名称冲突：" + string.Join(", ", collision.Select(t => t.Id)));
        foreach (string language in languages)
        {
            if (language == "csharp")
                continue;
            foreach (var t in types.Values)
            {
                if (t.EnumMembers.Keys.Select(Name).Distinct(StringComparer.Ordinal).Count() != t.EnumMembers.Count ||
                    t.Fields.Select(f => Accessor(f.Name, language)).Distinct(StringComparer.Ordinal).Count() != t.Fields.Length)
                    throw new NotSupportedException($"{t.Id} 的成员在 {language} 中重名，请调整类型定义。");
                if (language == "cpp")
                    CheckCppMembers(t.Id, Name(t.Id), t.Fields.Select(f => Accessor(f.Name, language)), t.Fields.Select(f => "v_" + f.Name));
            }
            foreach (var table in tables)
            {
                if (language == "java" && (JavaKeywords.Contains(table.Name) || table.Name is "version" or "bundle" or "preferJson") || language == "go" && Accessor(table.Name, language) == "Version" ||
                    language == "cpp" && table.Name is "version" or "load" or "ConfigSnapshot" ||
                    language is "lua" or "typescript" && table.Name == "version" || language == "typescript" && table.Name == "constructor")
                    throw new NotSupportedException($"表名 {table.Name} 与 {language} 快照 API 重名，请调整表名。");
                var accessors = table.Fields.SelectMany(f => types[f.Type].Kind == "tablereference" ? new[] { Accessor(f.Name, language), Accessor(f.Name, language) + "Id" } : new[] { Accessor(f.Name, language) });
                if (accessors.Distinct(StringComparer.Ordinal).Count() != accessors.Count())
                    throw new NotSupportedException($"{table.Name} 的字段或引用 Id 入口在 {language} 中重名，请调整字段名称。");
                if (language == "cpp")
                    CheckCppMembers(table.Name, table.Name + "Config", accessors, table.Fields.SelectMany(f => types[f.Type].Kind == "tablereference" ? new[] { "v_" + f.Name, "ref_" + f.Name } : new[] { "v_" + f.Name }));
            }
            if (language == "cpp")
                CheckCppMembers("ConfigSnapshot", "ConfigSnapshot", tables.Select(t => Accessor(t.Name, language)), tables.Select(t => "table_" + t.Name).Append("version_"));
            switch (language)
            {
                case "java":
                    Java(tables, files);
                    break;
                case "go":
                    Go(tables, files);
                    break;
                case "cpp":
                    Cpp(tables, files);
                    break;
                case "typescript":
                    TypeScript(tables, files);
                    break;
                case "lua":
                    Lua(tables, files);
                    break;
                case "rust":
                    Rust(tables, files);
                    break;
            }
        }
        // Validate the complete file set before a generation transaction can write anything.
        foreach (var collision in files.Keys.GroupBy(p => p, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            throw new NotSupportedException("生成文件路径在大小写不敏感的文件系统中冲突：" + string.Join(", ", collision));
        foreach (string path in files.Keys)
            if (Regex.IsMatch(Path.GetFileName(path), @"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                throw new NotSupportedException("生成文件名与 Windows 保留设备名冲突：" + path);
    }
    private void CheckCppMembers(string owner, string constructor, IEnumerable<string> accessors, IEnumerable<string> storage)
    {
        var reserved = storage.Append(constructor).ToHashSet(StringComparer.Ordinal);
        string? collision = accessors.FirstOrDefault(reserved.Contains);
        if (collision != null)
            throw new NotSupportedException($"{owner} 的 C++ 入口 {collision} 与构造函数或内部存储重名；请显式调整新字段，不会自动改名已有 API。");
    }
    private static readonly HashSet<string> JavaKeywords = new(("abstract assert boolean break byte case catch char class const continue default do double else enum extends final finally float for goto if implements import instanceof int interface long native new package private protected public return short static strictfp super switch synchronized this throw throws transient try void volatile while true false null _").Split(' '), StringComparer.Ordinal);
    private string Runtime(string suffix)
    {
        var assembly = typeof(NativeGenerator).Assembly;
        using var stream = assembly.GetManifestResourceStream("ZrConfig.Native.ConfigRuntime." + suffix + ".txt") ?? throw new InvalidOperationException("缺少目标语言运行库：" + suffix);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().ReplaceLineEndings("\n");
    }
    private string Quote(string value) => JsonSerializer.Serialize(value, PortableSchema.JsonOptions);
    private string Name(string value) => _names.TypeName(value);
    private string Accessor(string name, string language) => _names.Accessor(name, language);

    private static string SumTerms(IEnumerable<string> terms,string separator)
    {
        string[] values=terms.ToArray();
        if(values.Length==0)return "0";
        if(values.Length<=256)return string.Join(separator,values);
        string Build(int start,int length)
        {
            if(length==1)return values[start];
            int left=length/2;
            return "("+Build(start,left)+separator+Build(start+left,length-left)+")";
        }
        string result=Build(0,values.Length);
        return result[1..^1];
    }


    private void Comment(StringBuilder text, string description, string indent = "    ")
    {
        if (string.IsNullOrWhiteSpace(description))
            return;
        foreach (string line in description.ReplaceLineEndings("\n").Split('\n'))
            text.AppendLine(indent + "// " + line.Replace("\\u", "\\ u").Replace("\u2028", " ").Replace("\u2029", " ").TrimEnd().TrimEnd('\\'));
    }
    private Dictionary<string, PortableType> Types(PortableTable[] tables) => tables.SelectMany(t => t.Types).DistinctBy(t => t.Id).ToDictionary(t => t.Id, StringComparer.Ordinal);
}
