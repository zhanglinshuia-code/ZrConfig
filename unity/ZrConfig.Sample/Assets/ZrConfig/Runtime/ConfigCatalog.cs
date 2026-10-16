#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ZrConfig.Runtime.Server
{
    public sealed class TableDescriptor
    {
        private readonly Lazy<ITableCodec> _codec;
        public string Name { get; }
        public string SchemaHash { get; }
        public Type RowType { get; }
        public IReadOnlyList<string> Dependencies { get; }
        public int Component { get; }
        public IReadOnlyList<GroupDescriptor> Groups { get; }
        public TableDescriptor(string name, string schemaHash, Type rowType, Func<ITableCodec> codec, IEnumerable<string> dependencies, int component, IEnumerable<GroupDescriptor>? groups = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name)); SchemaHash = schemaHash ?? throw new ArgumentNullException(nameof(schemaHash));
            RowType = rowType ?? throw new ArgumentNullException(nameof(rowType)); Component = component;
            if (codec == null) throw new ArgumentNullException(nameof(codec));
            _codec = new Lazy<ITableCodec>(() =>
            {
                ITableCodec value = codec();
                if (value == null || value.TableName != Name || value.SchemaHash != SchemaHash || value.RowType != RowType)
                    throw new ConfigException(ErrorCode.CatalogMismatch, "Codec does not match its descriptor.", table: Name);
                return value;
            }, true);
            Dependencies = new List<string>(dependencies ?? throw new ArgumentNullException(nameof(dependencies))).AsReadOnly();
            Groups = new List<GroupDescriptor>(groups ?? Array.Empty<GroupDescriptor>()).AsReadOnly();
        }
        public ITableCodec GetCodec() => _codec.Value;
    }

    public sealed class SelectionPlan
    {
        public IReadOnlyList<string> RequestedTables { get; }
        public IReadOnlyList<TableDescriptor> Tables { get; }
        public string CatalogFingerprint { get; }
        internal SelectionPlan(string fingerprint, List<string> requested, List<TableDescriptor> tables)
        { CatalogFingerprint = fingerprint; RequestedTables = requested.AsReadOnly(); Tables = tables.AsReadOnly(); }
    }

    /// <summary>Generated immutable schema catalog. Planning visits each selected component/edge at most once.</summary>
    public sealed class Catalog : ITableCodecRegistry
    {
        private readonly Dictionary<string, TableDescriptor> _names = new Dictionary<string, TableDescriptor>(StringComparer.Ordinal);
        private readonly Dictionary<int, List<TableDescriptor>> _components = new Dictionary<int, List<TableDescriptor>>();
        public IReadOnlyList<TableDescriptor> Tables { get; }
        public string Fingerprint { get; }
        public string Target { get; }
        public Catalog(string fingerprint, IEnumerable<TableDescriptor> tables) : this(fingerprint,tables,"server-v1",Array.Empty<string>()) {}
        public Catalog(string fingerprint,IEnumerable<TableDescriptor> tables,string runtimeContract,IEnumerable<string> requiredCapabilities) : this(fingerprint,tables,runtimeContract,requiredCapabilities,"") {}
        public Catalog(string fingerprint,IEnumerable<TableDescriptor> tables,string runtimeContract,IEnumerable<string> requiredCapabilities,string target)
        {
            if(target!=""&&target!="C"&&target!="S")throw new ConfigException(ErrorCode.InvalidPlan,"Invalid catalog target.");
            Target=target;
            if(runtimeContract!="server-v1")throw new ConfigException(ErrorCode.UnsupportedCapability,"Unsupported runtime contract: "+runtimeContract);
            var supported=new HashSet<string>(new[]{"binary-v4", "json-v1", "server-default", "selected-closure", "lazy-strict", "lazy-on-access", "fixed-source-bytes", "snapshot-owned-handles", "explicit-reference-load", "row-cache", "group-cache", "typed-warmup", "bounded-source", "structured-errors", "manager-lifecycle"},StringComparer.Ordinal);
            foreach(string capability in requiredCapabilities??throw new ArgumentNullException(nameof(requiredCapabilities)))
                if(!supported.Contains(capability))throw new ConfigException(ErrorCode.UnsupportedCapability,"Unsupported runtime capability: "+capability);
            Fingerprint = fingerprint ?? throw new ArgumentNullException(nameof(fingerprint));
            var sorted = new List<TableDescriptor>(tables ?? throw new ArgumentNullException(nameof(tables)));
            sorted.Sort((a, b) => StringComparer.Ordinal.Compare(a.Name, b.Name));
            foreach (TableDescriptor table in sorted)
            {
                if (_names.ContainsKey(table.Name)) throw new ConfigException(ErrorCode.CatalogMismatch, "Duplicate catalog table: " + table.Name);
                _names.Add(table.Name, table);
                if (!_components.TryGetValue(table.Component, out List<TableDescriptor>? members)) _components.Add(table.Component, members = new List<TableDescriptor>());
                members.Add(table);
            }
            foreach (TableDescriptor table in sorted) foreach (string dependency in table.Dependencies)
                if (!_names.ContainsKey(dependency)) throw new ConfigException(ErrorCode.CatalogMismatch, "Unknown dependency: " + dependency);
            Tables = sorted.AsReadOnly();
        }
        public SelectionPlan Select(IEnumerable<string>? requested = null)
        {
            var input = new List<string>();
            if (requested == null) foreach (TableDescriptor table in Tables) input.Add(table.Name);
            else foreach (string name in requested)
            {
                if (string.IsNullOrEmpty(name)) throw new ConfigException(ErrorCode.InvalidPlan, "Selected table names must not be empty.");
                input.Add(name);
            }
            input.Sort(StringComparer.Ordinal);
            var components = new HashSet<int>();
            var queue = new Queue<TableDescriptor>();
            var selected = new HashSet<string>(StringComparer.Ordinal);
            void Include(string name)
            {
                if (!_names.TryGetValue(name, out TableDescriptor? table)) throw new ConfigException(ErrorCode.InvalidPlan, "Unknown selected table: " + name);
                if (!components.Add(table.Component)) return;
                foreach (TableDescriptor member in _components[table.Component]) { selected.Add(member.Name); queue.Enqueue(member); }
            }
            foreach (string name in input) Include(name);
            while (queue.Count != 0) foreach (string dependency in queue.Dequeue().Dependencies) Include(dependency);
            var result = new List<TableDescriptor>(selected.Count);
            foreach (TableDescriptor table in Tables) if (selected.Contains(table.Name)) result.Add(table);
            return new SelectionPlan(Fingerprint, input, result);
        }
        public TableDescriptor GetTable(string name) => name != null && _names.TryGetValue(name, out TableDescriptor? table)
            ? table : throw new ConfigException(ErrorCode.InvalidPlan, "Unknown catalog table.", table: name ?? "");
        public IEnumerable<ITableCodec> EnumerateCodecs() { foreach (TableDescriptor table in Tables) yield return table.GetCodec(); }
    }
}
