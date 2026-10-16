using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;

namespace ZrConfig.Maintenance;

// Toolchain bootstrapping is outside the generated runtime. No package manager or
// tool installation runs during normal generation/export or inside business code.
internal static class ServerRuntimeChecks
{
    internal static void InstallLua(string[] options)
    {
        Maintenance.Require(options.Length == 2, "server-lua <5.4.0|5.4.9> <empty temporary destination>");
        string version = options[0], destination = Path.GetFullPath(options[1]);
        string digest = version switch
        {
            // Published SHA-256 values: https://www.lua.org/ftp/
            "5.4.0" => "eac0836eb7219e421a96b7ee3692b93f0629e4cdb0c788432e3d10ce9ed47e28",
            "5.4.9" => "2335b6c582a52654f94612bf10d2f4672805d05329aa6568b1d8cd9e5c6fb8e6",
            _ => throw new ArgumentException("Unsupported Lua verification profile.")
        };
        Maintenance.Require(!Directory.Exists(destination) || !Directory.EnumerateFileSystemEntries(destination).Any(), "Lua destination must be empty.");
        Directory.CreateDirectory(destination);
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        byte[] archive = client.GetByteArrayAsync($"https://www.lua.org/ftp/lua-{version}.tar.gz").GetAwaiter().GetResult();
        Maintenance.Require(Convert.ToHexString(SHA256.HashData(archive)).Equals(digest, StringComparison.OrdinalIgnoreCase), "Lua source checksum mismatch.");
        using (var input = new GZipStream(new MemoryStream(archive), CompressionMode.Decompress))
            TarFile.ExtractToDirectory(input, destination, overwriteFiles: false);
        string source = Path.Combine(destination, "lua-" + version, "src");
        string compiler = Environment.GetEnvironmentVariable("ZRCONFIG_CC") ?? (OperatingSystem.IsWindows() ? "gcc" : "cc");
        string binary = Path.Combine(destination, OperatingSystem.IsWindows() ? "lua.exe" : "lua");
        List<string> arguments = ["-O2"];
        if (!OperatingSystem.IsWindows()) arguments.Add("-DLUA_USE_POSIX");
        arguments.AddRange(Directory.GetFiles(source, "*.c").Where(p => Path.GetFileName(p) != "luac.c").Order(StringComparer.Ordinal));
        arguments.AddRange(["-lm", "-o", binary]);
        Maintenance.Run(compiler, arguments, source);
        Maintenance.Run(binary, ["-v"], destination);
        Console.WriteLine(binary);
    }
}
