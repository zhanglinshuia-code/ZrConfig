using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace ZrConfig.Core;

public sealed record GeneratedLanguageProfile(string Language, string NamingPolicy, string ModuleRoot, string Toolchain)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? CapabilitySet { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? RuntimeCompatibilityRange { get; init; }
}
public sealed record GeneratedApiDependency(string Identity, GeneratedApiSymbol[] Symbols);
public sealed record GeneratedApiPolicy(int Format, string ProjectIdentity, string Family, string ContractVersion,
    GeneratedLanguageProfile[] Profiles, GeneratedNameBinding[] Names, GeneratedApiDependency[] Dependencies, string[] BehaviorContracts)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public SourceNameMapping[]? SourceMappings { get; init; }
}

internal static class GeneratedApiPolicies
{
    internal static GeneratedApiPolicy Create(GeneratedSymbolPlan names, string[] languages, Assembly models, GeneratedApiPolicy? previous, string? importSeed)
    {
        if (previous != null) Validate(previous);
        // Legacy imports derive their first identity from the accepted baseline, never the
        // edited candidate. Repeated preview/CLI approval therefore addresses the same IDs.
        string project = previous?.ProjectIdentity ?? (importSeed == null ? Guid.NewGuid() :
            new Guid(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("ZrConfig.LegacyImport/1\n" + importSeed)).AsSpan(0,16))).ToString("N");
        var bindings = new List<GeneratedNameBinding>();
        var old = (previous?.Names ?? []).ToDictionary(Key, StringComparer.Ordinal);
        foreach (GeneratedNameBinding current in names.Bindings)
        {
            string key = Key(current);
            string id = old.TryGetValue(key, out GeneratedNameBinding? existing) ? existing.SymbolId : GeneratedApiContracts.Hash(project + "\n" + key);
            bindings.Add(current with { SymbolId = id }); old.Remove(key);
        }
        bindings.AddRange(old.Values.Select(name => name with { Retired = true }));
        var occupied = new Dictionary<(string,string,string),GeneratedNameBinding>();
        foreach (GeneratedNameBinding binding in bindings)
        {
            var slot = (binding.Language,binding.Scope,binding.Name);
            if (occupied.TryGetValue(slot,out GeneratedNameBinding? prior) && prior.SourceBinding != binding.SourceBinding)
                throw new InvalidDataException($"已保留或退休的符号不能分配给另一来源：{binding.Language}:{binding.Scope}.{binding.Name}");
            occupied[slot] = binding;
        }
        var profiles = languages.Append("csharp").Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).Select(language=>Profile(language) with {
            ModuleRoot=names.Namespaces.ProfileRoot(language),CapabilitySet=Capabilities(language,names.ServerApi),
            RuntimeCompatibilityRange=language=="csharp"?"runtime-contract>=1.0 <2.0; source-compile":"embedded-runtime=1; source-compile" }).ToArray();
        return new GeneratedApiPolicy(1, project, names.ServerApi ? "legacy-and-server-v1" : "legacy-v1", previous?.ContractVersion ?? "1.0",
            profiles, bindings.OrderBy(n => n.Language,StringComparer.Ordinal).ThenBy(n=>n.Scope,StringComparer.Ordinal).ThenBy(n=>n.SourceBinding,StringComparer.Ordinal).ToArray(),
            [new GeneratedApiDependency("ZrConfig.Runtime", Surface(typeof(ZrConfig.Runtime.ConfigManager).Assembly.GetExportedTypes())),
             new GeneratedApiDependency("CSharp.SupportAndBusinessTypes", Surface(DependencyClosure(models,names.PublicDependencies)))],
            names.ServerApi ? ["legacy-v1/preserve-query-and-lifetime", "server-v1/snapshot-owned", "server-v1/query-no-io-no-decode", "server-v1/load-explicit-monotonic-cache", "server-v1/no-implicit-eviction", "server-v1/structured-errors", "server-v1/hot-query-no-per-call-allocation"] : ["legacy-v1/preserve-query-and-lifetime"]) { SourceMappings=names.SourceContract.Mappings.Length==0?null:names.SourceContract.Mappings };
    }
    internal static string Key(GeneratedNameBinding binding) => binding.Language + "\n" + binding.Scope + "\n" + binding.SourceBinding + "\n" + binding.Kind;
    internal static string Capabilities(string language,bool server)
    {
        string value="binary-v4;json-v1;legacy-query;legacy-references";
        if(!server)return value;
        value+=";server-default;selected-closure;lazy-strict;lazy-on-access;fixed-source-bytes;snapshot-owned-handles;explicit-reference-load;row-cache;group-cache;typed-warmup;file-source;memory-source;bounded-source;structured-errors;bounded-events;candidate-validation;manager-lifecycle";
        if(language is "csharp" or "java" or "typescript")value+=";scheduled-async-load";
        if(language is not "lua" and not "typescript")value+=";concurrent-readers";
        return value;
    }
    internal static GeneratedLanguageProfile Profile(string language) => language switch
    {
        "csharp" => new(language,"preserve-v1","ZrConfig.Models;ZrConfig.Runtime","netstandard2.1/csharp9"),
        "java" => new(language,"preserve-v1","zrconfig","java17"),
        "go" => new(language,"preserve-v1","zrconfig","go1.19"),
        "cpp" => new(language,"preserve-v1","zrconfig","c++17"),
        "rust" => new(language,"preserve-v1","zrconfig","rust1.70/edition2021"),
        "typescript" => new(language,"preserve-v1","./snapshot;./data","typescript5.8/node22/es2022/commonjs"),
        "lua" => new(language,"preserve-v1","ConfigSnapshot;data","lua5.4/int64/float64"),
        _ => throw new InvalidDataException("Unknown language profile: " + language)
    };
    internal static void Validate(GeneratedApiPolicy policy)
    {
        if(policy.SourceMappings!=null)GenerationSourceContract.ValidateMappings(policy.SourceMappings);
        if(policy.Profiles!=null)foreach(var profile in policy.Profiles)if(profile!=null)GenerationNamespaces.ValidateRoot(profile.Language,profile.ModuleRoot);
        if (policy.Format != 1 || !Guid.TryParseExact(policy.ProjectIdentity,"N",out _) || policy.Family is not ("legacy-v1" or "legacy-and-server-v1") || !Version.TryParse(policy.ContractVersion,out Version? version) || version.Major < 1 || version.Minor < 0 || version.Build != -1 ||
            policy.Profiles == null || policy.Names == null || policy.Dependencies == null || policy.BehaviorContracts == null ||
            policy.Profiles.Any(p=>p == null || (p with {ModuleRoot=Profile(p.Language).ModuleRoot,CapabilitySet=null,RuntimeCompatibilityRange=null,
                    Toolchain=p.Language=="csharp"&&p.Toolchain=="netstandard2.1/csharp8"?Profile(p.Language).Toolchain:p.Toolchain}) != Profile(p.Language) ||
                p.CapabilitySet!=null && p.CapabilitySet!=Capabilities(p.Language,policy.Family=="legacy-and-server-v1") ||
                p.RuntimeCompatibilityRange!=null && p.RuntimeCompatibilityRange!=(p.Language=="csharp"?"runtime-contract>=1.0 <2.0; source-compile":"embedded-runtime=1; source-compile")) || policy.Profiles.Select(p=>p.Language).Distinct().Count()!=policy.Profiles.Length ||
            policy.Names.Any(n=>n == null || n.SymbolId?.Length!=64 || string.IsNullOrEmpty(n.SourceBinding) || string.IsNullOrEmpty(n.Name)) ||
            policy.Names.Select(Key).Distinct(StringComparer.Ordinal).Count()!=policy.Names.Length || policy.Names.Select(n=>n.SymbolId).Distinct(StringComparer.Ordinal).Count()!=policy.Names.Length ||
            policy.Dependencies.Any(d=>d == null || string.IsNullOrEmpty(d.Identity) || d.Symbols == null))
            throw new InvalidDataException("API policy/name bindings are invalid or use an unsupported profile. Restore the accepted baseline.");
    }
    internal static IEnumerable<string> Compare(GeneratedApiPolicy? before, GeneratedApiPolicy? after)
    {
        if (before == null) yield break; // Import the first policy alongside the existing declaration baseline.
        if (after == null) { yield return "删除已固定的命名、工具链或运行时契约。"; yield break; }
        Validate(before); Validate(after);
        if(!(before.SourceMappings??[]).SequenceEqual(after.SourceMappings??[]))yield return "显式 Excel 源名称映射变化；逻辑身份、公开名称与协议名称保持原绑定。";
        if (before.ProjectIdentity!=after.ProjectIdentity || before.Family!=after.Family || before.ContractVersion!=after.ContractVersion)
            yield return "项目身份或 API 家族/主版本发生变化。";
        foreach (GeneratedLanguageProfile profile in before.Profiles)
        {
            GeneratedLanguageProfile? next=after.Profiles.SingleOrDefault(p=>p.Language==profile.Language);
            if(next==null || (next with {CapabilitySet=profile.CapabilitySet==null?null:next.CapabilitySet,RuntimeCompatibilityRange=profile.RuntimeCompatibilityRange==null?null:next.RuntimeCompatibilityRange})!=profile)
                yield return "语言工具链/模块/命名策略/能力契约变化：" + profile.Language;
        }
        var names=after.Names.ToDictionary(Key,StringComparer.Ordinal);
        foreach (GeneratedNameBinding name in before.Names)
            if (!names.TryGetValue(Key(name),out GeneratedNameBinding? next) || next.SymbolId != name.SymbolId || next.Name != name.Name || name.Retired != next.Retired)
                yield return "固定符号绑定/退休状态变化：" + name.Language + ":" + name.Scope + "." + name.Name;
        foreach (string behavior in before.BehaviorContracts)
            if (!after.BehaviorContracts.Contains(behavior,StringComparer.Ordinal)) yield return "行为契约变化：" + behavior;
        foreach (GeneratedApiDependency dependency in before.Dependencies)
        {
            GeneratedApiDependency? next=after.Dependencies.SingleOrDefault(d=>d.Identity==dependency.Identity);
            if(next==null) { yield return "移除公开依赖："+dependency.Identity; continue; }
            var candidate = new GeneratedApiContract(1,"C",next.Symbols,[]);
            foreach(string change in GeneratedApiContracts.Compare(new GeneratedApiContract(1,"C",dependency.Symbols,[]),candidate))
                yield return dependency.Identity + ": " + change;
        }
    }

    // These are compiler-emitted CLR declarations, not a hash of hand-written source text.
    // Transitive public signatures (including support/Runtime types) are part of the baseline.
    private static IEnumerable<Type> DependencyClosure(Assembly models,IEnumerable<Type> roots)
    {
        var found=new HashSet<Type>(); var pending=new Queue<Type>();
        void Include(Type? type)
        {
            if(type==null || type.IsGenericParameter) return;
            if(type.HasElementType) { Include(type.GetElementType()); return; }
            if(type.IsGenericType) foreach(Type argument in type.GetGenericArguments()) Include(argument);
            if(type.Assembly==models && found.Add(type)) pending.Enqueue(type);
        }
        foreach(Type root in roots) Include(root);
        foreach(Type support in models.GetExportedTypes().Where(t=>t.Namespace=="ZrConfig.Serialization")) Include(support);
        while(pending.TryDequeue(out Type? type))
        {
            Include(type.BaseType); foreach(Type contract in type.GetInterfaces()) Include(contract);
            foreach(FieldInfo field in type.GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static)) Include(field.FieldType);
            foreach(MethodBase method in type.GetConstructors().Cast<MethodBase>().Concat(type.GetMethods()))
            {
                if(method is MethodInfo function) Include(function.ReturnType);
                foreach(ParameterInfo parameter in method.GetParameters()) Include(parameter.ParameterType);
            }
        }
        return found;
    }
    internal static GeneratedApiSymbol[] Surface(IEnumerable<Type> types)
    {
        const BindingFlags flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly;
        var symbols=new List<GeneratedApiSymbol>();
        foreach(Type type in types.OrderBy(t=>t.FullName,StringComparer.Ordinal))
        {
            string scope=(type.FullName ?? type.Name).Replace('+','.'), parent=type.DeclaringType==null ? type.Namespace ?? "" : (type.DeclaringType.FullName ?? type.DeclaringType.Name).Replace('+','.');
            symbols.Add(new(parent,type.Name,$"{(type.IsInterface ? "interface" : "type")} {type.Attributes} {scope} : {Identity(type.BaseType)} [{string.Join(",",type.GetInterfaces().Select(Identity).Order(StringComparer.Ordinal))}] {Attributes(type.CustomAttributes)} {Constraints(type.GetGenericArguments())}"));
            foreach(FieldInfo field in type.GetFields(flags).Where(f=>f.IsPublic||f.IsFamily||f.IsFamilyOrAssembly))
                symbols.Add(new(scope,field.Name,$"{field.Attributes} {Identity(field.FieldType)} {field.Name} {(field.IsLiteral ? JsonSerializer.Serialize(field.GetRawConstantValue()) : "")} {Attributes(field.CustomAttributes)}"));
            foreach(MethodBase method in type.GetConstructors(flags).Cast<MethodBase>().Concat(type.GetMethods(flags)).Where(m=>m.IsPublic||m.IsFamily||m.IsFamilyOrAssembly))
            {
                MethodInfo? function=method as MethodInfo;
                string result=function==null ? "" : Identity(function.ReturnType)+" "+Attributes(function.ReturnParameter.CustomAttributes);
                string parameters=string.Join(",",method.GetParameters().Select(p=>$"{p.Attributes} {Identity(p.ParameterType)} {p.Name} {(p.HasDefaultValue ? Constant(p.DefaultValue) : "")} {Attributes(p.CustomAttributes)}"));
                symbols.Add(new(scope,method.Name,$"{method.Attributes} {result} {method.Name}({parameters}) {Constraints(method.IsGenericMethod ? method.GetGenericArguments() : [])} {Attributes(method.CustomAttributes)}"));
            }
        }
        return symbols.Distinct().OrderBy(s=>s.Scope,StringComparer.Ordinal).ThenBy(s=>s.Name,StringComparer.Ordinal).ThenBy(s=>s.Signature,StringComparer.Ordinal).ToArray();
    }
    private static string Identity(Type? type) => type == null ? "" : type.IsGenericParameter ? (type.DeclaringMethod==null ? "!" : "!!")+type.GenericParameterPosition.ToString(CultureInfo.InvariantCulture) :
        type.IsByRef ? Identity(type.GetElementType())+"&" : type.IsPointer ? Identity(type.GetElementType())+"*" :
        type.IsArray ? Identity(type.GetElementType())+"["+new string(',',type.GetArrayRank()-1)+"]" :
        type.IsGenericType ? (type.GetGenericTypeDefinition().FullName ?? type.Name)+"<"+string.Join(",",type.GetGenericArguments().Select(Identity))+">" : type.FullName ?? type.Name;
    private static string Constant(object? value) => value is DBNull || value is Missing ? "missing" : JsonSerializer.Serialize(value);
    private static string Constraints(Type[] parameters) => string.Join(";",parameters.Where(t=>t.IsGenericParameter).Select(t=>Identity(t)+":"+t.GenericParameterAttributes+":"+string.Join(",",t.GetGenericParameterConstraints().Select(Identity))));
    private static string Attributes(IEnumerable<CustomAttributeData> attributes) => string.Join(";", attributes.Where(a=>a.AttributeType.FullName is not ("System.Runtime.CompilerServices.CompilerGeneratedAttribute" or "System.Diagnostics.DebuggerStepThroughAttribute" or "System.Runtime.CompilerServices.AsyncStateMachineAttribute" or "System.Runtime.CompilerServices.IteratorStateMachineAttribute"))
        .Select(a=>a.ToString()).Order(StringComparer.Ordinal));
}
