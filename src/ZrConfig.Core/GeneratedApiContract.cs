using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ZrConfig.Core;

public sealed record GeneratedApiSymbol(string Scope, string Name, string Signature);
public sealed record GeneratedApiFile(string Path, string Digest);
public sealed record GeneratedApiContract(int Format, string Target, GeneratedApiSymbol[] Symbols, GeneratedApiFile[] NativeFiles)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public GeneratedApiPolicy? Policy { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public GeneratedApiSymbol[]? CompiledSymbols { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? DescriptorDigest { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public GeneratedApiSymbol[]? NativeSymbols { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? AcceptedParentDigest { get; init; }
}

/// <summary>
/// C# uses actual public declarations and compiled symbols. Native declarations are read from
/// emitted source; opaque digests also guard inferred types, binding and implementation changes.
/// Neither a file-deletion confirmation nor a schema hash authorizes an API migration.
/// </summary>
internal static class GeneratedApiContracts
{
    internal const string Marker = "zrconfig-api-contract:1";

    internal static GeneratedApiContract Capture(string target, IReadOnlyDictionary<string, string> files)
    {
        var symbols = new List<GeneratedApiSymbol>();
        var native = new List<GeneratedApiFile>();
        var nativeSymbols = new List<GeneratedApiSymbol>();
        foreach ((string path, string source) in files.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            if (path.EndsWith(".cs", StringComparison.Ordinal))
            {
                CompilationUnitSyntax unit = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();
                if (unit.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
                    throw new InvalidDataException("无法提取生成源码 API：" + path);
                string context = string.Join("\n", unit.DescendantTrivia(descendIntoTrivia: true)
                    .Where(t => t.IsDirective).Select(t => t.ToString().Trim()));
                Visit(unit.Members, "", symbols, context);
                // Aliases and imports participate in type/name resolution, even if declarations look equal.
                foreach (string import in unit.DescendantNodes().OfType<UsingDirectiveSyntax>().Select(Normalize).Distinct(StringComparer.Ordinal))
                    symbols.Add(new GeneratedApiSymbol("$imports", import, import));
            }
            else if (path.Contains('/') && !path.EndsWith(".meta", StringComparison.Ordinal))
            {
                nativeSymbols.AddRange(NativeApiSurface.Read(path, source));
                native.Add(new GeneratedApiFile(path, Hash(NativeSource(path.EndsWith(".java", StringComparison.Ordinal) ? JavaSchemas(source) : source,
                    path.EndsWith(".lua", StringComparison.Ordinal), path.EndsWith(".rs", StringComparison.Ordinal)))));
            }
        }
        return new GeneratedApiContract(1, target,
            symbols.Distinct().OrderBy(s => s.Scope, StringComparer.Ordinal).ThenBy(s => s.Name, StringComparer.Ordinal)
                .ThenBy(s => s.Signature, StringComparer.Ordinal).ToArray(), native.ToArray()) { NativeSymbols=nativeSymbols.Distinct().OrderBy(s=>s.Scope,StringComparer.Ordinal).ThenBy(s=>s.Name,StringComparer.Ordinal).ThenBy(s=>s.Signature,StringComparer.Ordinal).ToArray() };
    }

    internal static string[] Compare(GeneratedApiContract before, GeneratedApiContract after)
    {
        Validate(before);
        Validate(after);
        var changes = new List<string>();
        changes.AddRange(GeneratedApiPolicies.Compare(before.Policy, after.Policy));
        if (before.CompiledSymbols != null)
        {
            if (after.CompiledSymbols == null) changes.Add("删除编译器确认的 C# 公开契约。");
            else foreach (string change in Compare(new GeneratedApiContract(1,"C",before.CompiledSymbols,[]),new GeneratedApiContract(1,"C",after.CompiledSymbols,[])))
                changes.Add("编译后 C# API：" + change);
        }
        if(before.NativeSymbols!=null)
        {
            if(after.NativeSymbols==null)changes.Add("删除原生声明语法契约。");
            else foreach(string change in Compare(new GeneratedApiContract(1,"C",before.NativeSymbols,[]),new GeneratedApiContract(1,"C",after.NativeSymbols,[])))
                changes.Add("原生公开声明："+change);
        }
        if (before.Target != after.Target)
            changes.Add($"目标端 API：{before.Target} → {after.Target}");
        var current = after.Symbols.ToHashSet();
        foreach (GeneratedApiSymbol symbol in before.Symbols.Where(s => !current.Contains(s)))
            changes.Add($"删除或改变公开声明 {symbol.Scope}.{symbol.Name}：{symbol.Signature}");
        var old = before.Symbols.ToHashSet();
        var names = before.Symbols.Select(s => (s.Scope, s.Name)).ToHashSet();
        var interfaces = before.Symbols.Where(s => Regex.IsMatch(s.Signature, @"\binterface\s")).Select(s => Scope(s.Scope, s.Name)).ToHashSet();
        var extensions = before.Symbols.Where(s => s.Signature.Contains("(this ", StringComparison.Ordinal)).Select(s => s.Name).ToHashSet();
        foreach (GeneratedApiSymbol symbol in after.Symbols.Where(s => !old.Contains(s) &&
            (names.Contains((s.Scope, s.Name)) || interfaces.Contains(s.Scope) || Regex.IsMatch(s.Signature, @"\babstract\s") ||
             s.Signature.Contains("(this ", StringComparison.Ordinal) && extensions.Contains(s.Name))))
            changes.Add($"新增或改变同名入口，需要检查调用绑定 {symbol.Scope}.{symbol.Name}：{symbol.Signature}");
        var files = after.NativeFiles.ToDictionary(f => f.Path, StringComparer.Ordinal);
        foreach (GeneratedApiFile file in before.NativeFiles)
        {
            if (!files.TryGetValue(file.Path, out GeneratedApiFile? next))
                changes.Add("删除原生公开文件：" + file.Path);
            else if (file.Digest != next.Digest)
                changes.Add("原生源码发生变化，尚不能自动证明 API/行为兼容：" + file.Path);
        }
        return changes.ToArray();
    }

    internal static void Validate(GeneratedApiContract contract)
    {
        if (contract.Format is not (1 or 2 or 3) || contract.Format >= 2 && contract.Policy == null || contract.Target is not ("C" or "S") || contract.Symbols == null || contract.NativeFiles == null ||
            contract.Symbols.Any(s => s == null || s.Scope == null || string.IsNullOrEmpty(s.Name) || string.IsNullOrEmpty(s.Signature)) ||
            contract.Symbols.Distinct().Count() != contract.Symbols.Length ||
            contract.NativeFiles.Any(f => f == null || string.IsNullOrEmpty(f.Path) || !Regex.IsMatch(f.Digest ?? "", "^[A-F0-9]{64}$")) ||
            contract.NativeFiles.Select(f => f.Path).Distinct(StringComparer.Ordinal).Count() != contract.NativeFiles.Length)
            throw new InvalidDataException("API 基线无效或版本不受支持，请从版本控制恢复 .schema.json。");
        if (contract.Policy != null)
        {
            GeneratedApiPolicies.Validate(contract.Policy);
            var declared=contract.Symbols.Select(s=>(s.Scope,s.Name)).ToHashSet();
            foreach(GeneratedNameBinding binding in contract.Policy.Names.Where(n=>n.Language=="csharp" && !n.Retired && n.Kind is "type" or "property" or "member"))
                if(!declared.Contains((binding.Scope,binding.Name))) throw new InvalidDataException("固定公开名称与已保存的声明不一致："+binding.Scope+"."+binding.Name);
        }
        if(contract.NativeSymbols!=null)Validate(new GeneratedApiContract(1,"C",contract.NativeSymbols,[]));
        if (contract.CompiledSymbols != null)
            Validate(new GeneratedApiContract(1,"C",contract.CompiledSymbols,[]));
        if (contract.Format == 3 && (contract.CompiledSymbols == null || contract.DescriptorDigest != Digest(contract) ||
            contract.AcceptedParentDigest != null && !Regex.IsMatch(contract.AcceptedParentDigest,"^[A-F0-9]{64}$")))
            throw new InvalidDataException("API 描述摘要或父修订记录损坏，请恢复已接受的 .schema.json。");
    }

    internal static string Digest(GeneratedApiContract contract)
    {
        // Hash the same UTF-8 JSON bytes without retaining an additional UTF-16
        // serialization and UTF-8 copy of a large catalog's declaration surface.
        using var hash = SHA256.Create();
        using (var stream = new CryptoStream(Stream.Null, hash, CryptoStreamMode.Write, leaveOpen: true))
            JsonSerializer.Serialize(stream, contract with { DescriptorDigest = null, AcceptedParentDigest = null });
        return Convert.ToHexString(hash.Hash!);
    }
    internal static GeneratedApiContract Seal(GeneratedApiContract candidate, GeneratedApiContract? previous)
    {
        candidate = candidate with { Format = 3 };
        string digest = Digest(candidate);
        return candidate with { DescriptorDigest = digest,
            AcceptedParentDigest = previous == null ? null : previous.DescriptorDigest == digest ? previous.AcceptedParentDigest : previous.DescriptorDigest ?? Digest(previous) };
    }

    internal static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    private static string Normalize(SyntaxNode node) => node.ReplaceTrivia(node.DescendantTrivia(descendIntoTrivia: true), (_, _) => default)
        .WithoutTrivia().NormalizeWhitespace().ToFullString();
    private static string Scope(string parent, string name) => parent.Length == 0 ? name : parent + "." + name;
    private static bool Visible(SyntaxTokenList modifiers) => modifiers.Any(SyntaxKind.PublicKeyword) || modifiers.Any(SyntaxKind.ProtectedKeyword);

    private static void Visit(SyntaxList<MemberDeclarationSyntax> members, string scope, List<GeneratedApiSymbol> result, string context, bool interfaceMembers = false)
    {
        foreach (MemberDeclarationSyntax member in members)
        {
            if (member is BaseNamespaceDeclarationSyntax space)
            {
                Visit(space.Members, Scope(scope, space.Name.ToString()), result, context);
                continue;
            }
            if (member is TypeDeclarationSyntax type && Visible(type.Modifiers))
            {
                string typeName = type.Identifier.ValueText + (type.TypeParameterList == null ? "" : "`" + type.TypeParameterList.Parameters.Count);
                result.Add(new GeneratedApiSymbol(scope, typeName, context + "\n" + Normalize(type.WithMembers(default))));
                Visit(type.Members, Scope(scope, typeName), result, context, type is InterfaceDeclarationSyntax);
                continue;
            }
            if (member is EnumDeclarationSyntax enumeration && Visible(enumeration.Modifiers))
            {
                result.Add(new GeneratedApiSymbol(scope, enumeration.Identifier.ValueText, context + "\n" + Normalize(enumeration)));
                continue;
            }
            SyntaxTokenList modifiers = member switch
            {
                BaseMethodDeclarationSyntax method => method.Modifiers,
                BasePropertyDeclarationSyntax property => property.Modifiers,
                BaseFieldDeclarationSyntax field => field.Modifiers,
                DelegateDeclarationSyntax declaration => declaration.Modifiers,
                _ => default
            };
            if (!interfaceMembers && !Visible(modifiers))
                continue;
            string name;
            MemberDeclarationSyntax declarationWithoutBody;
            switch (member)
            {
                case MethodDeclarationSyntax method:
                    name = method.Identifier.ValueText;
                    declarationWithoutBody = method.WithBody(null).WithExpressionBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                    break;
                case ConstructorDeclarationSyntax constructor:
                    name = ".ctor";
                    declarationWithoutBody = constructor.WithBody(null).WithExpressionBody(null).WithInitializer(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                    break;
                case PropertyDeclarationSyntax property:
                    name = property.Identifier.ValueText;
                    declarationWithoutBody = property.WithAccessorList(Accessors(property.AccessorList, property.ExpressionBody != null))
                        .WithExpressionBody(null).WithInitializer(null).WithSemicolonToken(default);
                    break;
                case IndexerDeclarationSyntax indexer:
                    name = "this";
                    declarationWithoutBody = indexer.WithAccessorList(Accessors(indexer.AccessorList, indexer.ExpressionBody != null))
                        .WithExpressionBody(null).WithSemicolonToken(default);
                    break;
                case FieldDeclarationSyntax field:
                    foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                    {
                        var value = field.Modifiers.Any(SyntaxKind.ConstKeyword) ? variable : variable.WithInitializer(null);
                        result.Add(new GeneratedApiSymbol(scope, variable.Identifier.ValueText,
                            Normalize(field.WithDeclaration(field.Declaration.WithVariables(SyntaxFactory.SingletonSeparatedList(value))))));
                    }
                    continue;
                case EventFieldDeclarationSyntax field:
                    name = string.Join(",", field.Declaration.Variables.Select(v => v.Identifier.ValueText));
                    declarationWithoutBody = field.WithDeclaration(field.Declaration.WithVariables(SyntaxFactory.SeparatedList(field.Declaration.Variables.Select(v => v.WithInitializer(null)))));
                    break;
                case OperatorDeclarationSyntax op:
                    name = "operator " + op.OperatorToken.Text;
                    declarationWithoutBody = op.WithBody(null).WithExpressionBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                    break;
                case ConversionOperatorDeclarationSyntax op:
                    name = "conversion";
                    declarationWithoutBody = op.WithBody(null).WithExpressionBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                    break;
                case DelegateDeclarationSyntax declaration:
                    name = declaration.Identifier.ValueText;
                    declarationWithoutBody = declaration;
                    break;
                default:
                    throw new InvalidDataException("尚不支持此公开 API 声明，不能自动证明兼容：" + member.Kind());
            }
            result.Add(new GeneratedApiSymbol(scope, name, Normalize(declarationWithoutBody)));
        }
    }

    private static AccessorListSyntax Accessors(AccessorListSyntax? accessors, bool expression)
    {
        if (expression)
            return SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))));
        return (accessors ?? throw new InvalidDataException("公开属性缺少访问器。"))
            .WithAccessors(SyntaxFactory.List(accessors.Accessors.Select(a => a.WithBody(null).WithExpressionBody(null)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))));
    }

    // Preserve code and literal contents, remove comments, and normalize only generated wire metadata.
    // This intentionally errs on the side of review, rather than guessing native public declarations.
    internal static string NativeSource(string source, bool lua, bool rust = false)
    {
        var output = new StringBuilder();
        void Space() { if (output.Length != 0 && output[^1] != ' ') output.Append(' '); }
        for (int i = 0; i < source.Length;)
        {
            char c = source[i];
            if (char.IsWhiteSpace(c)) { Space(); while (i < source.Length && char.IsWhiteSpace(source[i])) i++; continue; }
            bool comment = i + 1 < source.Length && (lua ? source.AsSpan(i).StartsWith("--") : source.AsSpan(i).StartsWith("//"));
            if (comment)
            {
                int end = source.IndexOf('\n', i);
                if (end < 0) end = source.Length;
                // Type annotations are contractual, but CRLF's carriage return
                // is a line terminator rather than part of the annotation.
                if (lua && source.AsSpan(i).StartsWith("---@")) output.Append(source.AsSpan(i, end - i).TrimEnd('\r'));
                Space(); i = end; continue;
            }
            if (!lua && source.AsSpan(i).StartsWith("/*"))
            {
                int end = source.IndexOf("*/", i + 2, StringComparison.Ordinal);
                if (end < 0) throw new InvalidDataException("未结束的源码注释。");
                Space(); i = end + 2; continue;
            }
            if (lua && c == '[')
            {
                int openingEnd = i + 1;
                while (openingEnd < source.Length && source[openingEnd] == '=') openingEnd++;
                if (openingEnd < source.Length && source[openingEnd] == '[')
                {
                    string close = "]" + new string('=', openingEnd - i - 1) + "]";
                    int end = source.IndexOf(close, openingEnd + 1, StringComparison.Ordinal);
                    if (end < 0) throw new InvalidDataException("未结束的 Lua 字符串。");
                    string value = source.Substring(openingEnd + 1, end - openingEnd - 1);
                    output.Append(JsonSerializer.Serialize(NormalizeSchema(value)));
                    i = end + close.Length; continue;
                }
            }
            if (c == '"' || c == '\'' || c == '`')
            {
                // Rust lifetime annotations are identifiers, not quoted character literals.
                if (c == '\'' && rust && i + 1 < source.Length && (char.IsLetter(source[i + 1]) || source[i + 1] == '_') &&
                    (i + 2 >= source.Length || source[i + 2] != '\''))
                { output.Append(c); i++; continue; }
                int start = i++;
                bool complete = false;
                while (i < source.Length)
                {
                    if (source[i] == '\\') { i += 2; continue; }
                    if (source[i++] == c) { complete = true; break; }
                }
                if (!complete) throw new InvalidDataException("未结束的源码字符串。");
                string literal = source[start..i];
                if (c == '"')
                {
                    try { literal = JsonSerializer.Serialize(NormalizeSchema(JsonSerializer.Deserialize<string>(literal)!)); }
                    catch (JsonException) { /* Non-JSON escapes remain opaque and guarded. */ }
                }
                output.Append(literal); continue;
            }
            output.Append(c); i++;
        }
        return output.ToString().TrimEnd();
    }

    // Generated Java splits large metadata literals to stay below the classfile string limit.
    private static string JavaSchemas(string source)
    {
        const string literal = "\"(?:\\\\.|[^\"\\\\])*\"";
        return Regex.Replace(source, "String\\.join\\(\"\",\\s*(?<parts>" + literal + "(?:\\s*,\\s*" + literal + ")*)\\s*\\)", match =>
        {
            try
            {
                string value = string.Concat(Regex.Matches(match.Groups["parts"].Value, literal)
                    .Select(part => JsonSerializer.Deserialize<string>(part.Value)));
                string normalized = NormalizeSchema(value);
                return normalized == value ? match.Value : JsonSerializer.Serialize(normalized);
            }
            catch (JsonException) { return match.Value; }
        });
    }

    private static string NormalizeSchema(string value)
    {
        if (!value.StartsWith('{') || !value.Contains("\"schema", StringComparison.Ordinal) && !value.Contains("\"fields\"", StringComparison.Ordinal)) return value;
        try
        {
            if (JsonNode.Parse(value) is not JsonObject root || root["name"] == null || root["fields"] == null || root["types"] == null) return value;
            root.Remove("hash");
            RemoveDescriptions(root);
            return root.ToJsonString();
        }
        catch (JsonException) { return value; }
    }

    private static void RemoveDescriptions(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            obj.Remove("description");
            foreach (var item in obj) RemoveDescriptions(item.Value);
        }
        else if (node is JsonArray array)
            foreach (var item in array) RemoveDescriptions(item);
    }
}
