using System.Text.Json;

namespace ZrConfig.Core;

internal sealed record RuntimeTableDescriptor(string Name, string SchemaHash, string[] Dependencies, int Component);
internal sealed record RuntimeCatalogDescriptor(int Format, string Fingerprint, RuntimeTableDescriptor[] Tables, string[][] Components)
{
    public string Target { get; init; } = "";
    public string RuntimeContract { get; init; } = "server-v1";
    public string[] RequiredCapabilities { get; init; } = ["binary-v4", "json-v1", "server-default", "selected-closure", "lazy-strict", "lazy-on-access", "fixed-source-bytes", "snapshot-owned-handles", "explicit-reference-load", "row-cache", "group-cache", "typed-warmup", "bounded-source", "structured-errors", "manager-lifecycle"];
    internal string Serialize() => JsonSerializer.Serialize(this, PortableSchema.JsonOptions);
}

internal static class RuntimeCatalog
{
    internal static RuntimeCatalogDescriptor Create(IEnumerable<PortableTable> tables,string target="") => Create(tables.Select(t => (t.Name, t.Hash, t.Dependencies))) with {Target=target};

    internal static RuntimeCatalogDescriptor Create(IEnumerable<(string Name, string Hash, string[] Dependencies)> source)
    {
        var tables = source.OrderBy(t => t.Name, StringComparer.Ordinal).ToArray();
        var ordinals = tables.Select((t, i) => (t.Name, i)).ToDictionary(t => t.Name, t => t.i, StringComparer.Ordinal);
        var edges = tables.Select(t => t.Dependencies.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).Select(name =>
            ordinals.TryGetValue(name, out int index) ? index : throw new InvalidDataException($"{t.Name} 引用了目录外的表 {name}。")).ToArray()).ToArray();
        var reverse = tables.Select(_ => new List<int>()).ToArray();
        for (int node = 0; node < edges.Length; node++) foreach (int target in edges[node]) reverse[target].Add(node);

        // Iterative Kosaraju: bounded heap stack, O(tables + edges), independent of graph depth.
        var visited = new bool[tables.Length];
        var finish = new List<int>(tables.Length);
        var stack = new Stack<(int Node, int Edge)>();
        for (int root = 0; root < tables.Length; root++)
        {
            if (visited[root]) continue;
            visited[root] = true; stack.Push((root, 0));
            while (stack.TryPop(out var frame))
            {
                if (frame.Edge == edges[frame.Node].Length) { finish.Add(frame.Node); continue; }
                stack.Push((frame.Node, frame.Edge + 1));
                int next = edges[frame.Node][frame.Edge];
                if (!visited[next]) { visited[next] = true; stack.Push((next, 0)); }
            }
        }
        Array.Fill(visited, false);
        var groups = new List<string[]>();
        var pending = new Stack<int>();
        for (int i = finish.Count - 1; i >= 0; i--)
        {
            int root = finish[i]; if (visited[root]) continue;
            var names = new List<string>(); visited[root] = true; pending.Push(root);
            while (pending.TryPop(out int node))
            {
                names.Add(tables[node].Name);
                foreach (int next in reverse[node]) if (!visited[next]) { visited[next] = true; pending.Push(next); }
            }
            names.Sort(StringComparer.Ordinal); groups.Add(names.ToArray());
        }
        string[][] components = groups.OrderBy(g => g[0], StringComparer.Ordinal).ToArray();
        var membership = components.SelectMany((g, i) => g.Select(n => (n, i))).ToDictionary(x => x.n, x => x.i, StringComparer.Ordinal);
        RuntimeTableDescriptor[] descriptors = tables.Select(t => new RuntimeTableDescriptor(t.Name, t.Hash,
            t.Dependencies.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(), membership[t.Name])).ToArray();
        string fingerprint = GeneratedApiContracts.Hash(JsonSerializer.Serialize(descriptors, PortableSchema.JsonOptions));
        return new RuntimeCatalogDescriptor(2, fingerprint, descriptors, components);
    }
}
