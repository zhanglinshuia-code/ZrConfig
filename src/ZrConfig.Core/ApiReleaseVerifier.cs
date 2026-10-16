using System.Text.Json;

namespace ZrConfig.Core;

/// <summary>Compare a candidate with a separately obtained trusted release/target-branch baseline.</summary>
public static class ApiReleaseVerifier
{
    public static void Verify(string? trustedSummary, IReadOnlyDictionary<string,string> trustedSources,
        string candidateSummary, IReadOnlyDictionary<string,string> candidateSources)
    {
        var next = JsonSerializer.Deserialize<SchemaSummary>(candidateSummary)
            ?? throw new InvalidDataException("Candidate API baseline is missing.");
        if(!next.SupportedFormat)throw new InvalidDataException("Unsupported candidate baseline encoding.");
        var candidate = next.ApiContract ?? throw new InvalidDataException("Candidate removed the API baseline.");
        GeneratedApiContracts.Validate(candidate);
        RequireSource(candidate,candidateSources);
        ValidateHistory(next);
        var saved = trustedSummary == null ? null : JsonSerializer.Deserialize<SchemaSummary>(trustedSummary)
            ?? throw new InvalidDataException("Trusted baseline is invalid.");
        if(saved!=null&&!saved.SupportedFormat)throw new InvalidDataException("Unsupported trusted baseline encoding.");
        GeneratedApiContract before;
        if(saved?.ApiContract is {} contract)
        {
            GeneratedApiContracts.Validate(contract);
            RequireSource(contract,trustedSources);
            before=contract;
        }
        else
        {
            if(trustedSources.Count==0) return; // A genuinely new output has no older public surface.
            if(trustedSources.Values.Any(s=>s.Contains(GeneratedApiContracts.Marker,StringComparison.Ordinal)))
                throw new InvalidDataException("Trusted output lost its protected baseline.");
            before=GeneratedApiContracts.Capture(saved?.Target ?? candidate.Target,trustedSources);
        }
        string[] changes=GeneratedApiContracts.Compare(before,candidate);
        if(changes.Length==0) return;
        if(next.ApiMigrationHistory is {Length:>0} history)
        {
            // A branch may contain several reviewed migrations and compatible additions
            // between them. Each bridge must preserve every promise of the trusted surface.
            GeneratedApiContract current=before;
            bool followed=false;
            foreach(var revision in history)
            {
                if(GeneratedApiContracts.Compare(current,revision.Previous).Length!=0) continue;
                current=revision.Candidate;followed=true;
            }
            if(followed&&GeneratedApiContracts.Compare(current,candidate).Length==0) return;
            throw new InvalidDataException("Migration history does not connect the trusted baseline to the candidate.");
        }
        var receipt=next.LastMigration ?? throw new InvalidDataException("API changed against the trusted baseline without a migration receipt: "+string.Join("\n",changes));
        ValidateRevision(new(before,candidate,receipt));
    }

    internal static void ValidateHistory(SchemaSummary summary)
    {
        if(summary.ApiMigrationHistory is not {} history) return;
        if(history.Length==0||summary.ApiContract==null||history.Any(step=>step==null))
            throw new InvalidDataException("Migration history is empty or incomplete.");
        GeneratedApiContract? previous=null;
        foreach(var revision in history)
        {
            ValidateRevision(revision);
            if(previous!=null&&GeneratedApiContracts.Compare(previous,revision.Previous).Length!=0)
                throw new InvalidDataException("Migration history has an unauthorized gap.");
            previous=revision.Candidate;
        }
        if(summary.LastMigration?.Digest!=history[^1].Receipt.Digest||GeneratedApiContracts.Compare(previous!,summary.ApiContract).Length!=0)
            throw new InvalidDataException("Migration history does not match the accepted current contract.");
    }

    private static void ValidateRevision(ApiMigrationRevision revision)
    {
        var before=revision.Previous;var candidate=revision.Candidate;var receipt=revision.Receipt;
        if(before==null||candidate==null||receipt==null)throw new InvalidDataException("Incomplete migration revision.");
        GeneratedApiContracts.Validate(before);GeneratedApiContracts.Validate(candidate);
        string[] changes=GeneratedApiContracts.Compare(before,candidate);
        if(changes.Length==0||candidate.Policy==null||!receipt.Matches(receipt.Request) || receipt.Request.PreviousDescriptorDigest!=(before.DescriptorDigest??GeneratedApiContracts.Digest(before)) ||
            receipt.Request.CandidateDescriptorDigest!=candidate.DescriptorDigest || receipt.Request.Target!=candidate.Target ||
            receipt.Request.Family!=candidate.Policy.Family || receipt.Request.ContractVersion!=candidate.Policy.ContractVersion ||
            !receipt.Request.Changes.SequenceEqual(changes,StringComparer.Ordinal) ||
            !receipt.Request.Languages.SequenceEqual(candidate.Policy.Profiles.Select(p=>p.Language).Order(StringComparer.Ordinal),StringComparer.Ordinal))
            throw new InvalidDataException("Migration receipt does not authorize this exact contract revision.");
        int previousMajor=Version.Parse(before.Policy?.ContractVersion??"1.0").Major;
        if(Version.Parse(candidate.Policy.ContractVersion).Major<=previousMajor)
            throw new InvalidDataException("Breaking API change did not advance the contract major version.");
    }

    private static void RequireSource(GeneratedApiContract contract,IReadOnlyDictionary<string,string> files)
    {
        var actualSurface=GeneratedApiContracts.Capture(contract.Target,files);
        var actual=actualSurface with { NativeSymbols=contract.NativeSymbols==null?null:actualSurface.NativeSymbols,Format=contract.Format,Policy=contract.Policy,
            CompiledSymbols=contract.CompiledSymbols,DescriptorDigest=contract.DescriptorDigest,AcceptedParentDigest=contract.AcceptedParentDigest };
        if(GeneratedApiContracts.Compare(contract,actual).Length!=0 || GeneratedApiContracts.Compare(actual,contract).Length!=0)
            throw new InvalidDataException("Actual generated sources disagree with their recorded API baseline.");
    }
}
