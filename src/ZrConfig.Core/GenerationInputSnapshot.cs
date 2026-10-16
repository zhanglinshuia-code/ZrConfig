using System.Security.Cryptography;
using System.Text;
using ClosedXML.Excel;

namespace ZrConfig.Core;

// 只保存编译和结构发现实际读到的值，不保留可回收程序集中的类型。
internal sealed class GenerationInputSnapshot(string dataRoot, string codeRoot, ConfigTarget target,
    IReadOnlyDictionary<string, string> replacements)
{
    private readonly Dictionary<string, string> _sources = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _workbooks = new(StringComparer.Ordinal);
    private string[] _files = [];
    private string? _definitions;

    internal void RecordSource(string path, string text) => _sources.Add(path, Hash(text));
    internal void RecordFiles(string[] files) => _files = files;
    internal string MigrationDigest()
    {
        var entries = new SortedDictionary<string,string>(StringComparer.Ordinal);
        foreach (string file in _files)
        {
            using var stream = File.OpenRead(file);
            entries.Add("Data/" + Path.GetRelativePath(dataRoot,file).Replace('\\','/'),Convert.ToHexString(SHA256.HashData(stream)));
        }
        foreach (var entry in _sources)
            entries.Add("Config/" + Path.GetRelativePath(codeRoot,entry.Key).Replace('\\','/'), entry.Value);
        entries.Add("Tables.json",Hash(_definitions ?? ""));
        return Hash(System.Text.Json.JsonSerializer.Serialize(entries));
    }
    internal Dictionary<string, TableDefinition> ReadDefinitions(string path)
    {
        _definitions = File.Exists(path) ? File.ReadAllText(path) : null;
        return TableDefinition.Parse(_definitions);
    }

    internal void RecordWorkbook(string path, XLWorkbook workbook, string[] names)
    {
        if (!names.SequenceEqual(SheetNames(workbook), StringComparer.Ordinal))
        {
            throw new IOException("Workbook sheets changed during generation preview: " + path);
        }

        _workbooks.Add(path, HeaderHash(workbook));
    }

    internal bool IsCurrent()
    {
        if (!_files.SequenceEqual(ExcelWorkbookInput.FindFiles(dataRoot), StringComparer.Ordinal))
        {
            return false;
        }

        string definitionsPath = Path.Combine(codeRoot, "Tables.json");
        if (_definitions != (File.Exists(definitionsPath) ? File.ReadAllText(definitionsPath) : null))
        {
            return false;
        }

        string[] sources = RuleCompiler.EnumerateSources(codeRoot, Path.Combine(codeRoot, "Generated"))
            .Concat(replacements.Keys).Distinct(StringComparer.Ordinal).ToArray();
        if (sources.Length != _sources.Count || sources.Any(path => !_sources.TryGetValue(path, out string? hash) ||
            hash != Hash(replacements.TryGetValue(path, out string? text) ? text : File.ReadAllText(path))))
        {
            return false;
        }

        foreach (string path in _files)
        {
            using XLWorkbook workbook = ExcelWorkbookInput.Open(path);
            if (!_workbooks.TryGetValue(path, out string? previous) ? SheetNames(workbook).Length != 0 :
                previous != HeaderHash(workbook))
            {
                return false;
            }
        }

        return true;
    }

    private static string[] SheetNames(XLWorkbook workbook) => workbook.Worksheets.Select(sheet => sheet.Name)
        .Where(name => !name.StartsWith('#')).ToArray();

    private string HeaderHash(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write((int)target);
            string[] names = SheetNames(workbook);
            writer.Write(names.Length);
            foreach (string name in names)
            {
                writer.Write(name);
                const int rows = 5;
                writer.Write(rows);
                foreach (IXLCell cell in workbook.Worksheet(name).Rows(1, rows).CellsUsed(XLCellsUsedOptions.Contents)
                    .OrderBy(cell => cell.Address.RowNumber).ThenBy(cell => cell.Address.ColumnNumber))
                {
                    writer.Write(cell.Address.RowNumber);
                    writer.Write(cell.Address.ColumnNumber);
                    writer.Write(cell.GetInputText());
                }

                writer.Write(0);
            }
        }

        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    private static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
}
