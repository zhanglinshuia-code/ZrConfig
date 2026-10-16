using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ZrConfig.Sample.Editor
{
    /// <summary>Build in an isolated project copy; accepts explicit output and backend.</summary>
    public static class PlayerBuild
    {
        public static void BuildAcceptance()
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                string Required(string name)
                {
                    int index = Array.IndexOf(args, name);
                    if (index < 0 || index + 1 >= args.Length)
                    {
                        throw new ArgumentException("Missing " + name);
                    }
                    return args[index + 1];
                }

                string output = Path.GetFullPath(Required("-zrPlayerOutput"));
                string backend = Required("-zrBackend");
                if (backend != "mono" && backend != "il2cpp")
                {
                    throw new ArgumentException("Backend must be mono or il2cpp.");
                }
                BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
                if (target == BuildTarget.Android)
                {
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                }
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
                var named = NamedBuildTarget.FromBuildTargetGroup(group);
                PlayerSettings.SetScriptingBackend(named, backend == "il2cpp" ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
                PlayerSettings.SetManagedStrippingLevel(named, backend == "il2cpp" ? ManagedStrippingLevel.High : ManagedStrippingLevel.Low);
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { SampleProject.ScenePath },
                    locationPathName = output,
                    target = target,
                    options = BuildOptions.None,
                    extraScriptingDefines = new[] { "ZRCONFIG_ACCEPTANCE" }
                });
                File.WriteAllText(output + ".build.json", JsonUtility.ToJson(new BuildReceipt
                {
                    success = report.summary.result == BuildResult.Succeeded,
                    backend = backend,
                    target = target.ToString(),
                    unity = Application.unityVersion,
                    seconds = report.summary.totalTime.TotalSeconds,
                    errors = report.summary.totalErrors
                }, true));
                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException("Player build failed: " + report.summary.result);
                }
                EditorApplication.Exit(0);
            }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }

        // JsonUtility report fields intentionally use the published camelCase JSON contract.
#pragma warning disable IDE1006
        [Serializable]
        private sealed class BuildReceipt
        {
            public bool success;
            public string backend;
            public string target;
            public string unity;
            public double seconds;
            public int errors;
        }
#pragma warning restore IDE1006
    }
}
