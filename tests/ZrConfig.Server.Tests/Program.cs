if(args.Length>=2&&args[0]=="--benchmark-table-scale"){await ServerRuntimeBenchmarks.Run(args[1],100,args.Length>3?int.Parse(args[3]):3,tableCount:args.Length>2?int.Parse(args[2]):10000);return;}
if(args.Length>=3&&args[0]=="--benchmark-locks"){await ServerRuntimeBenchmarks.Run(args[1],1000,args.Length>3?int.Parse(args[3]):3,controlScenarios:true,lockRepository:args[2]);return;}
if(args.Length>=2&&args[0]=="--benchmark-control"){await ServerRuntimeBenchmarks.Run(args[1],1000,args.Length>2?int.Parse(args[2]):3,controlScenarios:true);return;}
if(args.Length>=3&&args[0]=="--benchmark-stages"){await ServerRuntimeBenchmarks.Run(args[1],1000,args.Length>3?int.Parse(args[3]):3,null,false,false,false,args[2]);return;}
if(args.Length>=2&&args[0]=="--benchmark-queries"){await ServerRuntimeBenchmarks.Run(args[1],1000,args.Length>2?int.Parse(args[2]):3,null,false,false,true);return;}
if(args.Length>=2&&args[0]=="--benchmark-scenarios"){await ServerRuntimeBenchmarks.Run(args[1],1000,args.Length>2?int.Parse(args[2]):3,null,false,true);return;}
if(args.Length>=2&&args[0]=="--benchmark-retained"){await ServerRuntimeBenchmarks.Run(args[1],10000,args.Length>2?int.Parse(args[2]):3,null,true);return;}
if(args.Length==4&&args[0]=="--sample-consumer"){await SampleConsumers.Run(args[1],args[2],args[3]);return;}
if(args.Length>=3&&args[0]=="--benchmark-samples"){await ServerRuntimeBenchmarks.Run(args[1],10000,args.Length>3?int.Parse(args[3]):3,args[2]);return;}
if(args.Length>=2&&args[0]=="--benchmark"){await ServerRuntimeBenchmarks.Run(args[1],args.Length>2?int.Parse(args[2]):10000,args.Length>3?int.Parse(args[3]):3);return;}
if(args.Length>=2&&args[0]=="--benchmark-worker"){ServerRuntimeBenchmarks.ManagedWorker(args.Skip(1).ToArray());return;}
// Public, independent consumer verification. Fixtures and generated sources stay temporary.
if(args.Length==4&&args[0]=="--api-migration-mutator"){GeneratedApiContractTests.MigrationMutationProbe(args[1],args[2],args[3]);return;}
if(args.Length==2&&args[0]=="--api-migration-consumer"){GeneratedApiContractTests.MigrationConsumer(args[1]);return;}
if(args.Length==2&&args[0]=="--trusted-ref"){GeneratedApiContractTests.VerifyTrustedRef(args[1]);return;}
string root = Path.Combine(Path.GetTempPath(), "zrconfig-server-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try
{
    GeneratedApiContractTests.Run(root);
    if (!args.Contains("--api-only", StringComparer.Ordinal))
        await PortableExportTests.RunAsync(root, requireNative: !args.Contains("--managed-only", StringComparer.Ordinal));
}
finally { Directory.Delete(root, recursive: true); }
