using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using ClosedXML.Excel;
using ExcelDataReader;

namespace ZrConfig.Core;
/// <summary>Shared workbook discovery and input for generation and both export entry points.</summary>
internal static class ExcelWorkbookInput
{
    static ExcelWorkbookInput() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    // Numeric cells must use the parser's culture on every OS; text and whitespace stay unchanged.
    public static string GetInputText(this IXLCell cell) => cell.Value.ToString(CultureInfo.InvariantCulture);
    public static string[] FindFiles(string folder)
    {
        // Do not follow symlinks/junctions into another tree or back into a parent.
        // Hidden directories are included; unreadable directories must fail instead of publishing a partial set.
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = false,
            AttributesToSkip = FileAttributes.ReparsePoint
        };
        return Directory.EnumerateFiles(folder,
            "*",
            options)
            .Where(path => Path.GetExtension(path)
            .Equals(".xlsx",
            StringComparison.OrdinalIgnoreCase) || Path.GetExtension(path)
            .Equals(".xls",
            StringComparison.OrdinalIgnoreCase))
            .Where(path => !Path.GetFileName(path)
            .StartsWith("~$",
            StringComparison.Ordinal) && !Path.GetFileName(path)
            .StartsWith("._",
            StringComparison.Ordinal))
            .OrderBy(path => SortKey(folder,
            path),
            StringComparer.OrdinalIgnoreCase)
            .ThenBy(path => SortKey(folder,
            path),
            StringComparer.Ordinal)
            .ToArray();
    }

    private static string SortKey(string folder,
        string path) => Path.GetRelativePath(folder,
        path)
        .Replace(Path.DirectorySeparatorChar,
        '/');
    public static string[] GetSheetNames(string path)
    {
        if (IsBinary(path))
        {
            using FileStream stream = File.OpenRead(path);
            using IExcelDataReader reader = ExcelReaderFactory.CreateBinaryReader(stream, new()
            {
                SinglePassMode = true
            });
            var names = new List<string>();
            do
            {
                if (reader.Name is { } name && !name.StartsWith('#'))
                {
                    names.Add(name);
                }
            }
            while (reader.NextResult());
            return names.ToArray();
        }

        // XLSX discovery reads only metadata so cross-workbook references can be registered first.
        using ZipArchive archive = ZipFile.OpenRead(path);
        using Stream metadata = (archive.GetEntry("xl/workbook.xml") ?? throw new InvalidDataException("Missing xl/workbook.xml.")).Open();
        var document = XDocument.Load(metadata);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        return document.Root!.Element(ns + "sheets")!.Elements(ns + "sheet")
            .Select(sheet => (string)sheet.Attribute("name")!)
            .Where(name => !name.StartsWith('#'))
            .ToArray();
    }

    public static XLWorkbook Open(string path)
    {
        if (!IsBinary(path))
        {
            return new XLWorkbook(path);
        }

        // Read real BIFF/OLE XLS into the existing cell model in memory. No Excel/COM,
        // intermediate XLSX file or dependency on an OS-specific spreadsheet application.
        using FileStream stream = File.OpenRead(path);
        using IExcelDataReader reader = ExcelReaderFactory.CreateBinaryReader(stream, new()
        {
            SinglePassMode = true
        });
        var workbook = new XLWorkbook();
        try
        {
            do
            {
                if (reader.Name == null || reader.Name.StartsWith('#'))
                {
                    continue;
                }

                IXLWorksheet sheet = workbook.AddWorksheet(reader.Name);
                while (reader.Read())
                {
                    for (int column = 0; column < reader.FieldCount; column++)
                    {
                        // Formula errors must not become empty cells/default values.
                        if (reader.GetCellError(column) is { } error)
                        {
                            sheet.Cell(reader.Depth + 1, column + 1).Value = ErrorText(error);
                        }
                        else if (reader.GetValue(column) is { } value && value != DBNull.Value)
                        {
                            sheet.Cell(reader.Depth + 1, column + 1).Value = XLCellValue.FromObject(value, CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
            while (reader.NextResult());
            return workbook;
        }
        catch
        {
            workbook.Dispose();
            throw;
        }
    }

    private static bool IsBinary(string path) => Path.GetExtension(path).Equals(".xls", StringComparison.OrdinalIgnoreCase);
    private static string ErrorText(CellError error) => error switch
    {
        CellError.NULL => "#NULL!",
        CellError.DIV0 => "#DIV/0!",
        CellError.VALUE => "#VALUE!",
        CellError.REF => "#REF!",
        CellError.NAME => "#NAME?",
        CellError.NUM => "#NUM!",
        CellError.NA => "#N/A",
        CellError.GETTING_DATA => "#GETTING_DATA",
        _ => throw new InvalidDataException($"Unknown Excel cell error: {error}.")
    };
}
