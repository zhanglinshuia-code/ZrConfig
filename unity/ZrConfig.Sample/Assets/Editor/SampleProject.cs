using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZrConfig.Models;
using ZrConfig.Runtime;
using ZrConfig.Unity;

namespace ZrConfig.Sample.Editor
{
    public static class SampleProject
    {
        public const string ScenePath = "Assets/ZrConfigSample.unity";
        [MenuItem("Tools/ZrConfig/打开示例场景")]
        public static void OpenScene()
        {
            if (EditorApplication.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.075f, 0.11f);
            camera.transform.position = new Vector3(0, 0, -10);
            var sample = new GameObject("ZrConfig Sample");
            sample.AddComponent<ZrConfigSample>();
            sample.AddComponent<ConfigTableInspector>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            PlayerSettings.companyName = "ZrConfig";
            PlayerSettings.productName = "ZrConfig Sample";
            PlayerSettings.defaultScreenWidth = 1100;
            PlayerSettings.defaultScreenHeight = 820;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/ZrConfig/验证示例数据")]
        public static void VerifyData()
        {
            VerifyAssemblies();
            VerifyFolder(ZrConfigSample.DataFolder);
            VerifyFolder(Path.Combine(Application.streamingAssetsPath, "ZrConfig"));
            Debug.Log("ZrConfig 示例验证通过：共享导出和桌面数据均可读取，主键、分组、引用、重载正常。");
        }

        // 批处理调用此入口时不要传 -quit；异步验证结束后自行退出，避免中途终止文件读取。
        [MenuItem("Tools/ZrConfig/验证 AB 同步与异步")]
        public static async void VerifyAbLoading()
        {
            try
            {
                VerifyData();
                await VerifyFolderAsync(ZrConfigSample.DataFolder);
                await VerifyFolderAsync(Path.Combine(Application.streamingAssetsPath, "ZrConfig"));
                if (!EditorApplication.isPlaying)
                {
                    await VerifySampleReloadViewAsync();
                }

                Debug.Log("ZrConfig AB 示例验证通过：真实文件同步/异步读取、按需引用、缓存共享、分组、组合键与重载正常。");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (System.Exception error)
            {
                Debug.LogException(error);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        [MenuItem("Tools/ZrConfig/验证示例程序集")]
        public static void VerifyAssemblies()
        {
            UnityEditor.Compilation.Assembly[] editor = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
            UnityEditor.Compilation.Assembly models = editor.SingleOrDefault(assembly => assembly.name == "ZrConfig.Models");
            UnityEditor.Compilation.Assembly rules = editor.SingleOrDefault(assembly => assembly.name == "ZrConfig.Sample.Rules.Editor");
            if (models == null || !models.sourceFiles.Any(file => file.Replace('\\',
                '/')
                .StartsWith("Assets/HotUpdate/Config/Generated/")) || models.sourceFiles.Any(file => !file.Replace('\\',
                '/')
                .StartsWith("Assets/HotUpdate/Config/")))
            {
                throw new InvalidDataException("配置必须直接由 Assets/HotUpdate/Config 中的源码编译。");
            }

            if (rules == null || (rules.flags & AssemblyFlags.EditorAssembly) == 0 || typeof(AllTypesRules).Assembly.GetName()
                .Name != rules.name || rules.sourceFiles.Any(file => !file.Replace('\\',
                '/')
                .StartsWith("Assets/Editor/ConfigRules/")))
            {
                throw new InvalidDataException("后置检查必须编译到独立 Editor 程序集。");
            }

            UnityEditor.Compilation.Assembly[] player = CompilationPipeline.GetAssemblies(AssembliesType.Player);
            RequireSource("ZrConfig.Runtime", "Assets/ZrConfig/Runtime/", false);
            RequireSource("ZrConfig.Validation", "Assets/ZrConfig/Validation/", true);
            RequireSource("ZrConfig.Unity", "Assets/ZrConfig/Unity/Runtime/", false);
            RequireSource("ZrConfig.Unity.Editor", "Assets/ZrConfig/Unity/Editor/", true);
            if (!player.Any(assembly => assembly.name == models.name) || player.Any(assembly => assembly.name == rules.name))
            {
                throw new InvalidDataException("Player 程序集应包含配置，并排除后置检查。");
            }

            if (typeof(HeroConfig).Assembly.GetReferencedAssemblies().Any(reference => reference.Name == "ZrConfig.Validation"))
            {
                throw new InvalidDataException("配置程序集不应依赖编辑器校验接口。");
            }

            string[] names = new[]
            {
                "ZrConfig.Models",
                "ZrConfig.Runtime",
                "ZrConfig.Validation",
                "ZrConfig.Unity",
                "ZrConfig.Unity.Editor"
            };
            if (Directory.EnumerateFiles(Application.dataPath,
                "*.dll",
                SearchOption.AllDirectories)
                .Any(file => names.Contains(Path.GetFileNameWithoutExtension(file))))
            {
                throw new InvalidDataException("请移除旧的 ZrConfig DLL，所有 ZrConfig 程序集都应由 Assets 源码编译。");
            }

            Debug.Log("ZrConfig 程序集验证通过：Runtime、Validation、Unity 适配器、配置及规则均来自 Assets 源码；Player 不包含校验框架、规则或查看器编辑器代码。");
            void RequireSource(string name, string folder, bool editorOnly)
            {
                UnityEditor.Compilation.Assembly assembly = editor.SingleOrDefault(item => item.name == name);
                if (assembly == null || assembly.sourceFiles.Length == 0 || assembly.sourceFiles.Any(file => !file.Replace('\\',
                    '/')
                    .StartsWith(folder,
                    System.StringComparison.Ordinal)) || ((assembly.flags & AssemblyFlags.EditorAssembly) != 0) != editorOnly || player.Any(item => item.name == name) == editorOnly)
                {
                    throw new InvalidDataException(name + " 必须从 " + folder + " 编译，并保持正确的 Editor / Player 边界。");
                }
            }
        }

        private static void VerifyFolder(string folder)
        {
            var manager = new ConfigManager(JsonUtility.FromJson<ConfigManifest>);
            var resources = new SampleAbResources(folder);
            ISyncConfigSource source = resources.Source;
            ConfigSnapshot snapshot = manager.Reload(source);
            if (snapshot.LoadedRowCount != 0 || snapshot.Tables.Count != 0 || resources.SyncReadCount != 1 || resources.AsyncReadCount != 0)
            {
                throw new InvalidDataException("初始化不应加载表数据。");
            }

            HeroConfig single = snapshot.GetHero(1);
            if (snapshot.LoadedRowCount != 1 || single.TryGetCachedSkill(out _))
            {
                throw new InvalidDataException("按行加载不应展开引用。");
            }

            if (!ReferenceEquals(single,
                single.Skill.Hero) || !ReferenceEquals(single,
                single.Mentor) || snapshot.LoadedRowCount != 2 || resources.SyncReadCount != 3 || resources.AsyncReadCount != 0)
            {
                throw new InvalidDataException("属性按需加载或循环引用不正确。");
            }

            foreach (HeroConfig hero in snapshot.GetHeroTable().Rows)
            {
                if (!ReferenceEquals(hero, snapshot.GetHeroTable().Get(hero.Id)))
                {
                    throw new InvalidDataException("英雄主键查询不一致。");
                }
            }

            foreach (DropConfig drop in snapshot.GetDropTable().Rows)
            {
                if (!ReferenceEquals(drop,
                    snapshot.GetDropTable()
                    .Get(drop.GroupId,
                    drop.Id)) || snapshot.GetDropTable()
                    .GetByGroupId(drop.GroupId)
                    .Count == 0)
                {
                    throw new InvalidDataException("掉落键或分组查询不一致。");
                }
            }

            foreach (SkillLevelConfig skill in snapshot.GetSkillLevelTable().Rows)
            {
                if (!ReferenceEquals(skill, snapshot.GetSkillLevelTable().Get(skill.SkillId, skill.Level, skill.Quality)))
                {
                    throw new InvalidDataException("技能组合键查询不一致。");
                }
            }

            foreach (HeroConfig hero in snapshot.GetHeroTable().Rows)
            {
                if (hero.Skill != null && !ReferenceEquals(hero.Skill, snapshot.GetSkillTable().Get(hero.Skill.Id)))
                {
                    throw new InvalidDataException("技能对象引用不一致。");
                }

                if (hero.Mentor != null && !ReferenceEquals(hero.Mentor, snapshot.GetHeroTable().Get(hero.Mentor.Id)))
                {
                    throw new InvalidDataException("导师对象引用不一致。");
                }
            }

            ConfigSnapshot current = manager.Reload(source);
            if (ReferenceEquals(snapshot, current))
            {
                throw new InvalidDataException("重载未产生新快照。");
            }

            bool staleSnapshot = false;
            bool staleReference = false;
            try
            {
                snapshot.GetHero(1);
            }
            catch (System.InvalidOperationException)
            {
                staleSnapshot = true;
            }

            try
            {
                _ = single.Skill;
            }
            catch (System.InvalidOperationException)
            {
                staleReference = true;
            }

            if (!staleSnapshot || !staleReference)
            {
                throw new InvalidDataException("更新后旧快照和缓存引用应失效。");
            }

            HeroConfig replacement = current.GetHero(1);
            ConfigOperation<HeroConfig> cached = current.GetHeroAsync(1);
            if (!cached.IsCompleted || !ReferenceEquals(replacement, cached.GetAwaiter().GetResult()))
            {
                throw new InvalidDataException("同步与异步查询未共享行缓存。");
            }

            if (resources.AsyncReadCount != 0)
            {
                throw new InvalidDataException("同步流程不应调用异步资源读取。");
            }
        }

        private static async ConfigOperation<bool> VerifySampleReloadViewAsync()
        {
            ISyncConfigSource source = new SampleAbResources(ZrConfigSample.DataFolder).Source;
            ConfigSnapshot first = UnityConfigRuntime.IsInitialized ? UnityConfigRuntime.Reload(source) : UnityConfigRuntime.Load(source);
            var host = new GameObject("ZrConfig reload view verification")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            host.SetActive(false);
            try
            {
                ZrConfigSample sample = host.AddComponent<ZrConfigSample>();
                MethodInfo changed = typeof(ZrConfigSample).GetMethod("OnChanged",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                MethodInfo capture = typeof(ZrConfigSample).GetMethod("CaptureView",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                changed.Invoke(sample, new object[] { first });
                first.GetHero(1);
                object firstView = capture.Invoke(sample, null);
                FieldInfo overview = firstView.GetType().GetField("Overview");
                string firstOverview = (string)overview.GetValue(firstView);
                // 精确重现 Publish 已使旧快照失效、Unity Changed 尚未回调的窗口。
                ConfigSnapshot next = UnityConfigRuntime.Manager.Reload(source);
                if (sample.Snapshot.LoadedRowCount != 0 || !ReferenceEquals(sample.Snapshot, next))
                {
                    throw new InvalidDataException("发布新快照后，示例仍在读取等待通知的旧快照。");
                }

                if ((string)overview.GetValue(firstView) != firstOverview || !firstOverview.Contains("已加载 1 行"))
                {
                    throw new InvalidDataException("绘制中的显示数据随配置重载发生变化。");
                }

                for (int i = 0; i < 8; i++)
                {
                    next = await UnityConfigRuntime.Manager.ReloadAsync(source);
                    // 迟到的旧通知不应把界面退回失效快照。
                    changed.Invoke(sample, new object[] { first });
                    if (sample.Snapshot.LoadedRowCount != 0 || !ReferenceEquals(sample.Snapshot, next))
                    {
                        throw new InvalidDataException("异步重载后，示例没有跟随当前快照。");
                    }

                    if (!((string)overview.GetValue(capture.Invoke(sample, null))).Contains("已加载 0 行"))
                    {
                        throw new InvalidDataException("界面仍显示旧快照中的行数。");
                    }

                    await next.GetHeroAsync(1);
                    if (sample.Snapshot.LoadedRowCount != 1)
                    {
                        throw new InvalidDataException("示例显示行数没有随当前快照更新。");
                    }
                }

                var broken = new DelegateConfigSource((_,
                    _) => ConfigOperation<byte[]>.FromException(new IOException("expected verification failure")));
                try
                {
                    await UnityConfigRuntime.ReloadAsync(broken);
                }
                catch (IOException)
                {
                }

                if (!ReferenceEquals(sample.Snapshot, next) || sample.Snapshot.LoadedRowCount != 1)
                {
                    throw new InvalidDataException("加载失败后界面没有保留当前配置。");
                }

                Debug.Log("ZrConfig 示例界面回归通过：发布/通知间隙、迟到通知、连续异步重载与失败保留。");
                return true;
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }

        private static async ConfigOperation<bool> VerifyFolderAsync(string folder)
        {
            var manager = new ConfigManager(JsonUtility.FromJson<ConfigManifest>);
            var resources = new SampleAbResources(folder);
            ConfigSnapshot snapshot = await manager.ReloadAsync(resources.Source);
            if (snapshot.LoadedRowCount != 0 || resources.AsyncReadCount != 1)
            {
                throw new InvalidDataException("异步初始化只应读取清单。");
            }

            HeroConfig hero = await snapshot.GetHeroAsync(1);
            if (snapshot.LoadedRowCount != 1 || hero.TryGetCachedSkill(out _) || resources.AsyncReadCount != 2)
            {
                throw new InvalidDataException("异步单行查询不应提前读取引用。");
            }

            SkillConfig skill = await hero.GetSkillAsync();
            HeroConfig mentor = await hero.GetMentorAsync();
            if (!ReferenceEquals(hero, await skill.GetHeroAsync()) || !ReferenceEquals(hero, mentor) || resources.AsyncReadCount != 3)
            {
                throw new InvalidDataException("异步引用与循环引用加载不正确。");
            }

            if (!ReferenceEquals(hero,
                snapshot.GetHero(1)) || !ReferenceEquals(skill,
                hero.Skill) || !ReferenceEquals(hero,
                await snapshot.GetHeroAsync(1)) || resources.AsyncReadCount != 3)
            {
                throw new InvalidDataException("重复查询或切换模式没有复用缓存。");
            }

            DropConfigTable drops = await snapshot.GetDropTableAsync();
            foreach (DropConfig row in drops.Rows)
            {
                if (!ReferenceEquals(row, drops.Get(row.GroupId, row.Id)) || drops.GetByGroupId(row.GroupId).Count == 0)
                {
                    throw new InvalidDataException("异步整表分组查询不正确。");
                }
            }

            SkillLevelConfigTable levels = await snapshot.GetSkillLevelTableAsync();
            foreach (SkillLevelConfig row in levels.Rows)
            {
                if (!ReferenceEquals(row, await snapshot.GetSkillLevelAsync(row.SkillId, row.Level, row.Quality)))
                {
                    throw new InvalidDataException("异步组合键查询不正确。");
                }
            }

            // 完整验收显式提供目录；前面的业务查询不需要注册表。
            var validator = new ConfigManager(TableCodecs.Registry, JsonUtility.FromJson<ConfigManifest>);
            await validator.ReloadAllAsync(resources.Source);
            ConfigSnapshot replacement = await manager.ReloadAsync(resources.Source);
            if (replacement.LoadedRowCount != 0 || ReferenceEquals(hero, await replacement.GetHeroAsync(1)))
            {
                throw new InvalidDataException("异步重载没有生成独立快照与对象。");
            }

            bool staleReference = false;
            try
            {
                await hero.GetSkillAsync();
            }
            catch (System.InvalidOperationException)
            {
                staleReference = true;
            }

            if (!staleReference || resources.SyncReadCount != 0)
            {
                throw new InvalidDataException("异步重载后旧引用应失效，且全程不能触发同步资源读取。");
            }

            return true;
        }
    }
}
