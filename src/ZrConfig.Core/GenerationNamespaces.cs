using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ZrConfig.Core;

/// <summary>Null members inherit the accepted project profile. Values never change wire identities.</summary>
public sealed record GenerationNamespaces(string? CSharp = null, string? Java = null, string? Go = null,
    string? Cpp = null, string? Rust = null, string? TypeScript = null, string? Lua = null)
{
    internal string? Get(string language) => language switch
    {
        "csharp" => CSharp, "java" => Java, "go" => Go, "cpp" => Cpp, "rust" => Rust,
        "typescript" => TypeScript, "lua" => Lua, _ => throw new ArgumentException("Unknown language: " + language)
    };
    internal static GenerationNamespaces Resolve(string root, GenerationNamespaces? requested)
    {
        GeneratedApiPolicy? saved = null;
        // Baseline integrity is reported by ModelGenerationPlan as API_BASELINE_INVALID.
        // Namespace resolution must not turn a damaged baseline into a naming failure.
        try
        {
            if (File.Exists(Path.Combine(root,".schema.json")))
                saved = JsonSerializer.Deserialize<SchemaSummary>(File.ReadAllText(Path.Combine(root,".schema.json")))?.ApiContract?.Policy;
            if (saved != null) GeneratedApiPolicies.Validate(saved);
        }
        catch (Exception error) when (error is JsonException or InvalidDataException or ArgumentException)
        {
            saved = null;
        }
        string ResolveOne(string language)
        {
            string value=requested?.Get(language) ?? saved?.Profiles.SingleOrDefault(p=>p.Language==language)?.ModuleRoot ?? GeneratedApiPolicies.Profile(language).ModuleRoot;
            ValidateRoot(language,value);return value;
        }
        return new(ResolveOne("csharp"),ResolveOne("java"),ResolveOne("go"),ResolveOne("cpp"),ResolveOne("rust"),ResolveOne("typescript"),ResolveOne("lua"));
    }
    internal static void ValidateRoot(string language,string value)
    {
        if(value==GeneratedApiPolicies.Profile(language).ModuleRoot)return;
        bool identifiers(string name,string separator) => name.Length<=160 && name.Split(separator).All(part=>Regex.IsMatch(part,"^[A-Za-z][A-Za-z0-9_]*$") && SyntaxFacts.GetKeywordKind(part)==SyntaxKind.None && !Reserved.Contains(part));
        bool valid=language switch
        {
            "csharp" => identifiers(value.EndsWith(";ZrConfig.Runtime",StringComparison.Ordinal)?value[..^17]:value,"."),
            "java" or "lua" => identifiers(value,"."),
            "cpp" => identifiers(value,"::"),
            "rust" => identifiers(value,"." ) && !value.Contains('.'),
            "go" => value.Length<=180 && Regex.IsMatch(value,"^[a-z0-9][a-z0-9._-]*(/[A-Za-z0-9][A-Za-z0-9._-]*)*$") && !value.Split('/').Any(p=>p is "." or ".."),
            "typescript" => value.Length<=160 && Regex.IsMatch(value,"^(?:@[a-z0-9][a-z0-9._-]*/)?[a-z0-9][a-z0-9._-]*$"),
            _ => false
        };
        if(!valid)throw new ArgumentException("Invalid module root for "+language+": "+value);
    }
    private static readonly HashSet<string> Reserved=new(("self Self super crate mod pub impl trait type fn let mut enum struct match move ref use as async await dyn const static extern unsafe where loop in true false class package import extends implements interface abstract final native synchronized transient volatile strictfp instanceof throws throw new return break continue switch case default if else for while do try catch finally public private protected void int long boolean null end local function then elseif repeat until and or not nil goto").Split(' '),StringComparer.Ordinal);
    internal string ModelRoot => CSharp!.Split(';')[0];
    internal string ProfileRoot(string language)=>language=="csharp"?ModelRoot+";ZrConfig.Runtime":Get(language)!;
    internal string PathFor(string path)=>Lua!="ConfigSnapshot;data"&&path.StartsWith("Lua/",StringComparison.Ordinal)?"Lua/"+Lua!.Replace('.','/')+"/"+path[4..]:path;
    internal string ScopeFor(string language,string scope)=>language switch
    {
        "csharp" when scope=="ZrConfig.Models"||scope.StartsWith("ZrConfig.Models.",StringComparison.Ordinal)=>ModelRoot+scope[15..],
        "java" when scope=="zrconfig"||scope.StartsWith("zrconfig.",StringComparison.Ordinal)=>Java+scope[8..],
        _=>scope
    };
    internal void Apply(Dictionary<string,string> files)
    {
        foreach(string path in files.Keys.ToArray())
        {
            string source=files[path];
            if(path.EndsWith(".cs",StringComparison.Ordinal)&&ModelRoot!="ZrConfig.Models")
            {
                var tree=CSharpSyntaxTree.ParseText(source).GetRoot();
                source=tree.ReplaceNodes(tree.DescendantNodes().Where(name=>name is NameSyntax or MemberAccessExpressionSyntax && name.ToString() is "ZrConfig.Models" or "global::ZrConfig.Models"),
                    (original,rewritten)=>(original is NameSyntax
                        ? (SyntaxNode)SyntaxFactory.ParseName((original.ToString().StartsWith("global::",StringComparison.Ordinal)?"global::":"")+ModelRoot)
                        : SyntaxFactory.ParseExpression((original.ToString().StartsWith("global::",StringComparison.Ordinal)?"global::":"")+ModelRoot)).WithTriviaFrom(original)).ToFullString();
            }
            if(path.StartsWith("Java/",StringComparison.Ordinal)&&Java!="zrconfig")source=RewriteIdentifier(source,"zrconfig",Java!);
            if(path.StartsWith("Cpp/",StringComparison.Ordinal)&&Cpp!="zrconfig")source=RewriteIdentifier(source,"zrconfig",Cpp!);
            if(path=="Rust/Cargo.toml"&&Rust!="zrconfig")source=source.Replace("name = \"zrconfig-generated\"","name = "+JsonSerializer.Serialize(Rust),StringComparison.Ordinal);
            string next=PathFor(path);if(next!=path)files.Remove(path);files[next]=source;
        }
        if(Go!="zrconfig")files.Add("Go/go.mod","module "+Go+"\n\ngo 1.19\n");
        if(TypeScript!="./snapshot;./data")
        {
            var exports = new Dictionary<string,object>();
            foreach (string module in new[] { "snapshot", "data", "server", "runtime" })
                if (files.ContainsKey("TypeScript/"+module+".ts"))
                    exports.Add(module=="snapshot"?".":"./"+module,new { types="./dist/"+module+".d.ts", @default="./dist/"+module+".js" });
            files.Add("TypeScript/package.json",JsonSerializer.Serialize(new { name=TypeScript,version="1.0.0",@private=true,type="commonjs",main="./dist/snapshot.js",types="./dist/snapshot.d.ts",exports })+"\n");
        }
    }
    // Replace code identifiers only; protocol metadata, literals, and comments remain byte-for-byte intact.
    private static string RewriteIdentifier(string source,string before,string after)
    {
        var result=new StringBuilder(source.Length);int i=0;
        while(i<source.Length)
        {
            int start=i;char c=source[i++];
            if(c is '\'' or '"') {while(i<source.Length){if(source[i++]=='\\'){if(i<source.Length)i++;}else if(source[i-1]==c)break;}}
            else if(c=='/'&&i<source.Length&&source[i]=='/') {while(i<source.Length&&source[i]!='\n')i++;}
            else if(c=='/'&&i<source.Length&&source[i]=='*') {int end=source.IndexOf("*/",i+1,StringComparison.Ordinal);if(end<0)throw new InvalidDataException("Unterminated generated comment.");i=end+2;}
            else if(char.IsLetter(c)||c=='_') {while(i<source.Length&&(char.IsLetterOrDigit(source[i])||source[i]=='_'))i++;if(source.AsSpan(start,i-start).SequenceEqual(before)){result.Append(after);continue;}}
            result.Append(source.AsSpan(start,i-start));
        }
        return result.ToString();
    }
}
