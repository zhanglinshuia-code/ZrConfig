using System.Text.Json;

namespace ZrConfig.Core;

internal static class GenerationFamily
{
    internal static string Validate(string value) => value is "auto" or "legacy-v1" or "server-v1" ? value :
        throw new ArgumentException("运行时家族必须是 auto、legacy-v1 或 server-v1。");

    internal static bool Server(string root, string requested)
    {
        Validate(requested);
        if (requested != "auto") return requested == "server-v1";
        string path = Path.Combine(root,".schema.json");
        if (!File.Exists(path)) return true;
        try
        {
            var saved = JsonSerializer.Deserialize<SchemaSummary>(File.ReadAllText(path));
            // Enabling a new family is explicit for an established legacy project. Its
            // legal old fields must not collide with newly introduced view helpers.
            if (saved?.ApiContract?.Policy is { } policy) return policy.Family == "legacy-and-server-v1";
            string registry = Path.Combine(root,"Generated","TableCodecs.g.cs");
            return File.Exists(registry) && GeneratedApiContracts.Capture("C",new Dictionary<string,string> { ["TableCodecs.g.cs"] = File.ReadAllText(registry) })
                .Symbols.Any(symbol=>symbol.Scope=="ZrConfig.Models" && symbol.Name=="ServerCatalog");
        }
        catch (JsonException) { return false; } // The baseline verifier reports corruption; never reset it here.
    }
}
