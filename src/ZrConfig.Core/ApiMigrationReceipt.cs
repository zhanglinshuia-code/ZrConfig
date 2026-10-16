using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ZrConfig.Core;

public sealed record ApiMigrationRequest(int Format, string ChangeId, string PreviousDescriptorDigest,
    string CandidateDescriptorDigest, string InputDigest, string ToolFingerprint, string Target,
    string Family, string ContractVersion, string[] Languages, string[] Changes);

public sealed record ApiMigrationRevision(GeneratedApiContract Previous, GeneratedApiContract Candidate, ApiMigrationReceipt Receipt);

public sealed record ApiConsumerCheck(string Language, string Profile, string Executable, string[] Arguments,
    string Directory, string[] Inputs, int TimeoutSeconds = 300);

public sealed record ApiConsumerEvidence(string Language, string Profile, string CheckDigest,
    string InputDigest, string ExecutableDigest, string OutputDigest);

public sealed record ApiMigrationReceipt(ApiMigrationRequest Request, ApiConsumerEvidence[] Consumers, string Digest)
{
    internal static string ContentDigest(ApiMigrationRequest request, ApiConsumerEvidence[] consumers) =>
        GeneratedApiContracts.Hash(JsonSerializer.Serialize(new { Request = request, Consumers = consumers }));

    internal bool Matches(ApiMigrationRequest request) =>
        Request != null && Request.Format==1 && Request.Languages!=null && Request.Changes!=null && Consumers != null &&
        new[]{Request.ChangeId,Request.PreviousDescriptorDigest,Request.CandidateDescriptorDigest,Request.InputDigest,Request.ToolFingerprint}.All(IsDigest) &&
        Request.Languages.Length>0 && Request.Languages.Distinct(StringComparer.Ordinal).Count()==Request.Languages.Length &&
        JsonSerializer.Serialize(Request) == JsonSerializer.Serialize(request) &&
        Digest == ContentDigest(Request, Consumers) &&
        Consumers.Length<=128 && Consumers.All(c => c != null && request.Languages.Contains(c.Language,StringComparer.Ordinal) && !string.IsNullOrWhiteSpace(c.Profile) &&
            new[] { c.CheckDigest, c.InputDigest, c.ExecutableDigest, c.OutputDigest }.All(IsDigest)) &&
        Consumers.Select(c=>(c.Language,c.Profile)).Distinct().Count()==Consumers.Length &&
        request.Languages.All(language => Consumers.Any(c => c.Language == language));

    private static bool IsDigest(string? value) => value?.Length == 64 && value.All(c => c is >= '0' and <= '9' or >= 'A' and <= 'F');
}

/// <summary>Runs explicitly supplied consumer commands against a temporary candidate, never against Generated in place.</summary>
public static class ApiMigrationVerifier
{
    public static async Task<ApiMigrationReceipt> VerifyAsync(ModelGenerationPlan plan,
        string checksFile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        string configuration = Path.GetFullPath(checksFile), baseDirectory = Path.GetDirectoryName(configuration)!;
        var checks = JsonSerializer.Deserialize<ApiConsumerCheck[]>(await File.ReadAllTextAsync(configuration, cancellationToken))
            ?? throw new InvalidDataException("Consumer checks must be a JSON array.");
        ApiMigrationRequest request = plan.MigrationRequest ?? throw new InvalidOperationException("No valid API migration is pending.");
        if (checks.Length == 0 || checks.Length > 128 || checks.Any(c=>c==null) || checks.Select(c=>(c.Language,c.Profile)).Distinct().Count()!=checks.Length || request.Languages.Any(language => !checks.Any(c => c?.Language == language)))
            throw new InvalidDataException("Every affected language requires a compiler and consumer verification command.");
        string temporary = Path.Combine(Path.GetTempPath(), "zrconfig-api-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        try
        {
            plan.WriteVerificationCandidate(temporary);
            var candidateInputs = FileInventory(temporary);
            var evidence = new List<ApiConsumerEvidence>();
            foreach (ApiConsumerCheck check in checks)
            {
                if (check == null || !request.Languages.Contains(check.Language, StringComparer.Ordinal) ||
                    string.IsNullOrWhiteSpace(check.Profile) || string.IsNullOrWhiteSpace(check.Executable) ||
                    check.Arguments == null || check.Inputs == null || check.Inputs.Length == 0 ||
                    check.TimeoutSeconds is < 1 or > 1800 || string.IsNullOrWhiteSpace(check.Directory))
                    throw new InvalidDataException("Invalid API consumer check.");
                string directory = Path.GetFullPath(check.Directory, baseDirectory);
                string executable = ResolveExecutable(check.Executable, baseDirectory);
                string inputs = InputDigest(check.Inputs, baseDirectory), executableDigest = FileDigest(executable);
                string Expand(string argument) => argument.Replace("{candidate}", temporary, StringComparison.Ordinal)
                    .Replace("{input}", plan.InputDirectory, StringComparison.Ordinal);
                var start = new ProcessStartInfo(executable) { WorkingDirectory = directory, UseShellExecute = false,
                    RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
                foreach (string argument in check.Arguments) start.ArgumentList.Add(Expand(argument));
                start.Environment["ZRCONFIG_API_CANDIDATE"] = temporary;
                start.Environment["ZRCONFIG_API_CHANGE"] = request.ChangeId;
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(check.TimeoutSeconds));
                using var process = Process.Start(start) ?? throw new IOException("Unable to start API consumer verification.");
                using var output = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                using var error = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                var outputTail=new OutputTail();var errorTail=new OutputTail();
                Task readOutput = HashStream(process.StandardOutput.BaseStream, output,outputTail, timeout.Token);
                Task readError = HashStream(process.StandardError.BaseStream, error,errorTail, timeout.Token);
                try
                {
                    await process.WaitForExitAsync(timeout.Token);
                    await Task.WhenAll(readOutput, readError);
                }
                catch
                {
                    if (!process.HasExited) process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync(CancellationToken.None);
                    try { await Task.WhenAll(readOutput, readError); } catch (OperationCanceledException) { }
                    throw;
                }
                if (process.ExitCode != 0)
                    throw new InvalidDataException($"API consumer {check.Language}/{check.Profile} failed with exit code {process.ExitCode}.\nstdout (last 8192 bytes):\n{outputTail.Text}\nstderr (last 8192 bytes):\n{errorTail.Text}");
                if (inputs != InputDigest(check.Inputs, baseDirectory) || executableDigest != FileDigest(executable))
                    throw new IOException("Consumer sources or tool changed during API verification.");
                var actualCandidate=FileInventory(temporary);
                if(actualCandidate.Count!=candidateInputs.Count||candidateInputs.Any(file=>!actualCandidate.TryGetValue(file.Key,out string? digest)||digest!=file.Value))
                    throw new IOException("Consumer verification changed the candidate it was required to verify.");
                evidence.Add(new(check.Language, check.Profile, GeneratedApiContracts.Hash(JsonSerializer.Serialize(check)),
                    inputs, executableDigest, GeneratedApiContracts.Hash(Convert.ToHexString(output.GetHashAndReset()) + Convert.ToHexString(error.GetHashAndReset()))));
            }
            plan.RequireMigrationCurrent();
            var results = evidence.OrderBy(e => e.Language, StringComparer.Ordinal).ThenBy(e => e.Profile, StringComparer.Ordinal).ToArray();
            return new(request, results, ApiMigrationReceipt.ContentDigest(request, results));
        }
        finally { Directory.Delete(temporary, recursive: true); }
    }

    private static async Task HashStream(Stream stream, IncrementalHash hash,OutputTail tail, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[8192];
        int read;
        while ((read = await stream.ReadAsync(buffer, cancellationToken)) != 0){hash.AppendData(buffer,0,read);tail.Append(buffer,read);}
    }
    private static string ResolveExecutable(string value, string directory)
    {
        if (Path.IsPathRooted(value) || value.Contains(Path.DirectorySeparatorChar) || value.Contains(Path.AltDirectorySeparatorChar))
            return Path.GetFullPath(value, directory);
        foreach (string entry in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(entry)) continue;
            string candidate = Path.Combine(entry, value);
            if (File.Exists(candidate)) return candidate;
            if (OperatingSystem.IsWindows() && File.Exists(candidate + ".exe")) return candidate + ".exe";
        }
        throw new FileNotFoundException("Consumer compiler/executable is unavailable: " + value);
    }
    private static string FileDigest(string path) { using var file = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(file)); }
    private sealed class OutputTail
    {
        private readonly byte[] _bytes=new byte[8192];private int _length;
        internal void Append(byte[] bytes,int count){int keep=Math.Min(_length,_bytes.Length-count);if(keep>0)Buffer.BlockCopy(_bytes,_length-keep,_bytes,0,keep);Buffer.BlockCopy(bytes,0,_bytes,keep,count);_length=keep+count;}
        internal string Text=>Encoding.UTF8.GetString(_bytes,0,_length);
    }
    private static Dictionary<string,string> FileInventory(string root)
    {
        var files=new Dictionary<string,string>(StringComparer.Ordinal);var pending=new Stack<string>();pending.Push(root);
        while(pending.TryPop(out string? directory))foreach(string path in Directory.EnumerateFileSystemEntries(directory))
        {
            FileAttributes attributes=File.GetAttributes(path);
            if((attributes&FileAttributes.ReparsePoint)!=0)throw new IOException("Linked verification inputs are not supported: "+path);
            if((attributes&FileAttributes.Directory)!=0)pending.Push(path);else files.Add(path,FileDigest(path));
        }
        return files;
    }
    private static string InputDigest(string[] inputs,string directory)
    {
        var records=new SortedDictionary<string,string>(StringComparer.Ordinal);
        foreach(string input in inputs.Distinct(StringComparer.Ordinal))
        {
            string path=Path.GetFullPath(input,directory);
            if((File.GetAttributes(path)&FileAttributes.ReparsePoint)!=0)throw new IOException("Linked consumer input: "+path);
            if(Directory.Exists(path))foreach(var file in FileInventory(path))records.Add(input+"/"+Path.GetRelativePath(path,file.Key).Replace('\\','/'),file.Value);
            else records.Add(input,FileDigest(path));
        }
        return GeneratedApiContracts.Hash(JsonSerializer.Serialize(records));
    }
}
