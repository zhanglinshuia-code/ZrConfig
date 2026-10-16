using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ZrConfig.Models;
using ZrConfig.Runtime;
using ZrConfig.Unity;
using Debug = UnityEngine.Debug;

namespace ZrConfig.Sample
{
    /// <summary>Opt-in Player probe, enabled by the acceptance build or -zrAcceptanceReport.</summary>
    public static class PlayerAcceptance
    {
        public static bool IsRequested
        {
            get; private set;
        }
        public static string Status { get; private set; } = "Running ZrConfig acceptance…";
        // JsonUtility report fields intentionally use the published camelCase JSON contract.
#pragma warning disable IDE1006
        [Serializable]
        private sealed class Report
        {
            public bool success;
            public string unity;
            public string platform;
            public string backend;
            public string device;
            public string source;
            public string version;
            public string error;
            public string[] checks;
            public double firstRowMilliseconds;
            public double warmQueriesMilliseconds;
            public long firstRowAllocatedBytes;
            public long warmQueryAllocatedBytes;
            public bool allocationCounterSupported;
            public long retainedManagedBytes;
            public double allTablesMilliseconds;
        }

#pragma warning restore IDE1006

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void Start()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            string Option(string name)
            {
                int index = Array.IndexOf(arguments, name);
                return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : null;
            }

            string reportPath = Option("-zrAcceptanceReport");
#if ZRCONFIG_ACCEPTANCE
            if (string.IsNullOrEmpty(reportPath)) { reportPath = Path.Combine(Application.persistentDataPath, "zrconfig-acceptance.json"); }
#else
            if (string.IsNullOrEmpty(reportPath))
            {
                return;
            }
#endif
            IsRequested = true;
            var report = new Report
            {
                unity = Application.unityVersion,
                platform = Application.platform.ToString(),
                device = SystemInfo.deviceModel + " / " + SystemInfo.processorType,
                source = "StreamingAssetsConfigSource / UnityWebRequest; not a third-party SDK",
#if ENABLE_IL2CPP
                backend = "IL2CPP"
#else
                backend = "Mono"
#endif
            };
            var checks = new List<string>();
            long counterBefore = GC.GetAllocatedBytesForCurrentThread();
            byte[] counterProbe = new byte[4096];
            report.allocationCounterSupported = GC.GetAllocatedBytesForCurrentThread() > counterBefore;
            GC.KeepAlive(counterProbe);
            try
            {
                string root = Option("-zrData") ?? Path.Combine(Application.streamingAssetsPath, "ZrConfig");
                var resources = new StreamingAssetsConfigSource(root);
                IConfigSource source = new CoalescingConfigSource(resources);
                using (var manager = new ConfigManager(TableCodecs.Registry, JsonUtility.FromJson<ConfigManifest>))
                {
                    ConfigSnapshot snapshot = await manager.ReloadAsync(source);
                    Require(snapshot.LoadedRowCount == 0 && resources.ReadCount == 1, "manifest-only initialization");
                    checks.Add("manifest-only initialization");
                    var timer = new Stopwatch();
                    long allocated = GC.GetAllocatedBytesForCurrentThread();
                    timer.Start();
                    HeroConfig hero = await snapshot.GetHeroAsync(1);
                    timer.Stop();
                    report.firstRowMilliseconds = timer.Elapsed.TotalMilliseconds;
                    report.firstRowAllocatedBytes = report.allocationCounterSupported ? GC.GetAllocatedBytesForCurrentThread() - allocated : -1;
                    Require(snapshot.LoadedRowCount == 1 && !hero.TryGetCachedSkill(out _), "lazy first row");
                    SkillConfig skill = await hero.GetSkillAsync();
                    Require(ReferenceEquals(hero, await skill.GetHeroAsync()) && ReferenceEquals(hero, await hero.GetMentorAsync()), "cyclic references");
                    checks.Add("lazy first row and cyclic references");
                    snapshot.GetHero(1);
                    int reads = resources.ReadCount;
                    timer.Restart();
                    allocated = GC.GetAllocatedBytesForCurrentThread();
                    for (int i = 0; i < 100000; i++)
                    {
                        Require(ReferenceEquals(hero, snapshot.GetHero(1)), "cached row identity");
                    }
                    report.warmQueryAllocatedBytes = report.allocationCounterSupported ? GC.GetAllocatedBytesForCurrentThread() - allocated : -1;
                    timer.Stop();
                    report.warmQueriesMilliseconds = timer.Elapsed.TotalMilliseconds;
                    Require(resources.ReadCount == reads, "cache avoids resource reads");
                    checks.Add("100000 cached queries without additional resource reads");
                    snapshot = await manager.ReloadAsync(source);
                    Task<HeroConfig> a = ReadHero(snapshot);
                    Task<HeroConfig> b = ReadHero(snapshot);
                    HeroConfig[] concurrent = await Task.WhenAll(a, b);
                    Require(ReferenceEquals(concurrent[0], concurrent[1]), "concurrent async identity");
                    checks.Add("asynchronous concurrent row loading");
                    using (var cancelled = new CancellationTokenSource())
                    {
                        cancelled.Cancel();
                        bool rejected = false;
                        try
                        {
                            await manager.ReloadAsync(source, cancelled.Token);
                        }
                        catch (OperationCanceledException) { rejected = true; }
                        Require(rejected && ReferenceEquals(manager.Current, snapshot), "cancelled reload preserves current");
                    }
                    checks.Add("cancelled reload preserves current");
                    var broken = new DelegateConfigSource((path, token) => path == "current.json" ? source.ReadAsync(path, token) : ConfigOperation<byte[]>.FromException(new IOException("Injected read failure")));
                    bool failed = false;
                    try
                    {
                        await manager.ReloadAllAsync(broken);
                    }
                    catch (IOException) { failed = true; }
                    Require(failed && ReferenceEquals(manager.Current, snapshot), "failed full reload preserves current");
                    checks.Add("failed full reload preserves current");
                    ConfigPreloadPlan preload = new ConfigPreloadPlan().WithRow<HeroConfig, int>(HeroConfigTable.GetCodec(), 1);
                    snapshot = await manager.ReloadPreparedAsync(source, preload);
                    Require(snapshot.LoadedRowCount == 1, "explicit row preloading");
                    checks.Add("explicit row preloading");
                    timer.Restart();
                    snapshot = await manager.ReloadAllAsync(source);
                    timer.Stop();
                    report.allTablesMilliseconds = timer.Elapsed.TotalMilliseconds;
                    Require(snapshot.GetAllTypes10kTable().Rows.Count == 10000, "all-types 10000-row table");
                    foreach (DropConfig row in snapshot.GetDropTable().Rows)
                    {
                        Require(ReferenceEquals(row, snapshot.GetDropTable().Get(row.GroupId, row.Id)), "composite keys");
                        Require(snapshot.GetDropTable().GetByGroupId(row.GroupId).Count > 0, "groups");
                    }
                    checks.Add("full schema/codecs, 10000-row table, composite keys and groups");
                    report.version = snapshot.VersionId;
                    report.retainedManagedBytes = GC.GetTotalMemory(false);
                }
                report.success = true;
            }
            catch (Exception error)
            {
                report.error = error.ToString();
                Debug.LogException(error);
            }
            finally
            {
                report.checks = checks.ToArray();
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(reportPath)));
                    File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
                }
                catch (Exception error) { report.success = false; Debug.LogException(error); }
                Status = "ZrConfig Player acceptance: " + (report.success ? "PASS" : "FAIL") + "\n" + JsonUtility.ToJson(report, true);
                Debug.Log("ZRCONFIG_ACCEPTANCE_REPORT " + JsonUtility.ToJson(report));
                Debug.Log(Status);
#if UNITY_STANDALONE
                Application.Quit(report.success ? 0 : 1);
#endif
            }
        }

        private static async Task<HeroConfig> ReadHero(ConfigSnapshot snapshot) => await snapshot.GetHeroAsync(1);
        private static void Require(bool condition, string name)
        {
            if (!condition)
            {
                throw new InvalidDataException("Player acceptance failed: " + name);
            }
        }
    }
}
