using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using ZrConfig.Models;
using ZrConfig.Runtime;
using ZrConfig.Unity;

namespace ZrConfig.Sample
{
    [DisallowMultipleComponent]
    public sealed class ZrConfigSample : MonoBehaviour
    {
        [SerializeField]
        private bool _useAsync = true;
        private CancellationTokenSource _lifetime;
        private ConfigSnapshot _snapshot;
        private SampleAbResources _resources;
        // Assigned by OnEnable in mobile/WebGL player compilation, outside the Editor branch.
#pragma warning disable IDE0044
        private StreamingAssetsConfigSource _streaming;
#pragma warning restore IDE0044
        private IConfigSource _source;
        private int ResourceReads => _resources != null ? _resources.SyncReadCount + _resources.AsyncReadCount : _streaming?.ReadCount ?? 0;
        private readonly Queue<string> _trace = new Queue<string>();
        private bool _busy;
        private string _status = "等待加载";
        private string _heroId = "1";
        private string _groupId = "10";
        private string _skillId = "101";
        private string _level = "1";
        private string _quality = "2";
        private string _heroResult = "";
        private string _dropResult = "";
        private string _skillResult = "";
        private Vector2 _scroll;
        private GUIStyle _heading;
        private GUIStyle _body;
        private ViewState _view;
        private static readonly string[] s_modes =
        {
            "同步模式",
            "异步模式"
        };
        public ConfigSnapshot Snapshot => RefreshSnapshot();
        public bool IsLoading => _busy;
        public string Status => _status;
        public bool UseAsync
        {
            get => _useAsync; set => _useAsync = _streaming != null || value;
        }
        private string Mode => _useAsync ? "异步" : "同步";

        public static string DataFolder
        {
            get
            {
#if UNITY_EDITOR
                // Assets -> 示例工程 -> unity -> 仓库；所有示例使用同一份正式导出。
                return Path.GetFullPath(Path.Combine(Application.dataPath, "../../../samples/Output"));
#else
                return Path.Combine(Application.streamingAssetsPath, "ZrConfig");
#endif
            }
        }

        private void OnEnable()
        {
            if (PlayerAcceptance.IsRequested)
            {
                return;
            }
            _lifetime = new CancellationTokenSource();
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL)
            _useAsync = true;
            _streaming = new StreamingAssetsConfigSource(DataFolder);
            _streaming.Trace += OnResourceTrace;
            _source = new CoalescingConfigSource(_streaming);
#else
            _resources = new SampleAbResources(DataFolder);
            _resources.Trace += OnResourceTrace;
            _source = _resources.Source;
#endif
            _trace.Clear();
            UnityConfigRuntime.Changed += OnChanged;
            ReloadConfiguration();
        }

        public async void ReloadConfiguration()
        {
            if (_busy || _lifetime == null)
            {
                return;
            }

            CancellationToken token = _lifetime.Token;
            _busy = true;
            _status = Mode + "读取配置清单…";
            try
            {
                IConfigSource source = _source;
                // 场景重进时复用已初始化的运行时；首次加载失败后仍可重试。
                ConfigSnapshot snapshot;
                if (_useAsync)
                {
                    snapshot = !UnityConfigRuntime.IsInitialized ? await UnityConfigRuntime.LoadAsync(source,
                        token) : await UnityConfigRuntime.ReloadAsync(source,
                        token);
                }
                else
                {
                    snapshot = !UnityConfigRuntime.IsInitialized ? UnityConfigRuntime.Load((ISyncConfigSource)source,
                        token) : UnityConfigRuntime.Reload((ISyncConfigSource)source,
                        token);
                }

                if (token.IsCancellationRequested)
                {
                    return;
                }

                // Changed 可能晚于 Current 发布；以当前快照为准，也忽略被后续重载替代的结果。
                if (!ReferenceEquals(snapshot, RefreshSnapshot()))
                {
                    return;
                }

                _status = Mode + "清单就绪 · " + DateTime.Now.ToString("HH:mm:ss");
                OnResourceTrace("发布快照 · " + snapshot.VersionId + " · 已加载 " + snapshot.LoadedRowCount + " 行");
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception error)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                _status = "加载失败" + (_snapshot == null ? "" : "（保留上次配置）") + "：" + error.Message;
                Debug.LogException(error);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    _busy = false;
                }
            }
        }

        private void OnChanged(ConfigSnapshot snapshot) => RefreshSnapshot();
        private ConfigSnapshot RefreshSnapshot()
        {
            // 异步重载先发布 Current，再恢复 Unity 包装层并通知；不能只依赖 Changed 更新界面。
            ConfigSnapshot current = UnityConfigRuntime.Current;
            if (!ReferenceEquals(_snapshot, current))
            {
                _snapshot = current;
                _heroResult = _dropResult = _skillResult = "点击查询，按需加载配置。";
            }

            return current;
        }

        private void QueryHero() => Query((snapshot, token) =>
        {
            if (!int.TryParse(_heroId, out int id))
            {
                return "请输入整数英雄 Id。";
            }

            HeroConfig hero = snapshot.GetHero(id, token);
            // 同步引用 getter 在缓存未命中时，通过 AB 同步入口取目标表字节。
            SkillConfig skill = hero.Skill;
            return DescribeHero(hero, skill, hero.OtherSkill, hero.Mentor, skill?.Hero);
        }, async (snapshot, token) =>
        {
            if (!int.TryParse(_heroId, out int id))
            {
                return "请输入整数英雄 Id。";
            }

            HeroConfig hero = await snapshot.GetHeroAsync(id, token);
            SkillConfig skill = await hero.GetSkillAsync(token);
            SkillConfig otherSkill = await hero.GetOtherSkillAsync(token);
            HeroConfig mentor = await hero.GetMentorAsync(token);
            HeroConfig skillHero = skill == null ? null : await skill.GetHeroAsync(token);
            return DescribeHero(hero, skill, otherSkill, mentor, skillHero);
        }, text => _heroResult = text);
        private static string DescribeHero(HeroConfig hero,
            SkillConfig skill,
            SkillConfig otherSkill,
            HeroConfig mentor,
            HeroConfig skillHero) => hero.Name + "  (Id = " + hero.Id + ")\n" + "技能：" + (skill?.Name ?? "无") + "    其他技能：" + (otherSkill?.Name ?? "无") + "\n导师：" + (mentor?.Name ?? "无") + "\n技能反向引用：" + (skillHero?.Name ?? "无") + "\n与导师为同一对象：" + ReferenceEquals(hero,
            mentor);
        private void QueryDrops() => Query((snapshot, token) =>
        {
            if (!int.TryParse(_groupId, out int id))
            {
                return "请输入整数掉落组 Id。";
            }

            return DescribeDrops(snapshot.GetDropTable(token), id);
        }, async (snapshot, token) =>
        {
            if (!int.TryParse(_groupId, out int id))
            {
                return "请输入整数掉落组 Id。";
            }

            DropConfigTable table = await snapshot.GetDropTableAsync(token);
            return DescribeDrops(table, id);
        }, text => _dropResult = text);
        private static string DescribeDrops(DropConfigTable table, int id)
        {
            ConfigRowGroup<DropConfig> group = table.GetByGroupId(id);
            var text = new StringBuilder("组 " + id + " · " + group.Count + " 条记录");
            foreach (DropConfig row in group)
            {
                text.Append("\n物品 ").Append(row.Id).Append(" × ").Append(row.Count);
            }

            return text.ToString();
        }

        private void QuerySkill() => Query((snapshot, token) =>
        {
            if (!int.TryParse(_skillId, out int id) || !int.TryParse(_level, out int level) || !int.TryParse(_quality, out int quality))
            {
                return "技能 Id、等级和品质都需要填写整数。";
            }

            return DescribeSkill(snapshot.GetSkillLevel(id, level, quality, token));
        }, async (snapshot, token) =>
        {
            if (!int.TryParse(_skillId, out int id) || !int.TryParse(_level, out int level) || !int.TryParse(_quality, out int quality))
            {
                return "技能 Id、等级和品质都需要填写整数。";
            }

            SkillLevelConfig row = await snapshot.GetSkillLevelAsync(id, level, quality, token);
            return DescribeSkill(row);
        }, text => _skillResult = text);
        private static string DescribeSkill(SkillLevelConfig row) => row.Name + " · 等级 " + row.Level + " · 品质 " + row.Quality + " · 伤害 " + row.Damage;
        private async void Query(Func<ConfigSnapshot, CancellationToken, string> read,
            Func<ConfigSnapshot, CancellationToken, ConfigOperation<string>> readAsync,
            Action<string> display)
        {
            if (_lifetime == null || _busy)
            {
                return;
            }

            ConfigSnapshot snapshot = RefreshSnapshot();
            if (snapshot == null)
            {
                return;
            }

            CancellationToken token = _lifetime.Token;
            _busy = true;
            int beforeReads = ResourceReads;
            try
            {
                string result = _useAsync ? await readAsync(snapshot, token) : read(snapshot, token);
                if (!token.IsCancellationRequested && ReferenceEquals(snapshot, UnityConfigRuntime.Current))
                {
                    display(result);
                    int reads = ResourceReads - beforeReads;
                    OnResourceTrace(Mode + "查询结束 · 本次资源读取 " + reads + " 次 · 已加载 " + snapshot.LoadedRowCount + " 行");
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception error)
            {
                if (!token.IsCancellationRequested && ReferenceEquals(snapshot, UnityConfigRuntime.Current))
                {
                    display(error is KeyNotFoundException ? "未找到对应配置。" : "加载失败：" + error.Message);
                }
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    _busy = false;
                }
            }
        }

        private void OnResourceTrace(string message)
        {
            // 只保留最近 14 条，避免示例运行越久占用越多内存。
            if (_trace.Count == 14)
            {
                _trace.Dequeue();
            }

            _trace.Enqueue(DateTime.Now.ToString("HH:mm:ss.fff") + "  " + message);
            Debug.Log("[ZrConfig AB 示例] " + message);
        }

        // Layout 与后续输入/Repaint 共用纯显示数据，不在 GUILayout 内查询可失效的配置。
        private sealed class ViewState
        {
            public bool Ready;
            public bool Busy;
            public string Status;
            public string Overview;
            public string Tables;
            public string Hero;
            public string Drops;
            public string Skill;
            public string Resources;
            public string Trace;
        }

        private ViewState CaptureView()
        {
            ConfigSnapshot snapshot = RefreshSnapshot();
            var tables = new StringBuilder();
            if (snapshot != null)
            {
                foreach (IConfigTable table in snapshot.Tables)
                {
                    if (tables.Length > 0)
                    {
                        tables.Append(" / ");
                    }

                    tables.Append(table.Name).Append(" · ").Append(table.Rows.Count).Append(" 条");
                }
            }

            return new ViewState
            {
                Ready = snapshot != null,
                Busy = _busy,
                Status = _status,
                Overview = snapshot == null ? "尚未加载配置。" : "版本：" + snapshot.VersionId + " · 已加载 " + snapshot.LoadedRowCount + " 行",
                Tables = tables.Length == 0 ? "尚未显式加载整表。" : tables.ToString(),
                Hero = _heroResult,
                Drops = _dropResult,
                Skill = _skillResult,
                Resources = _streaming != null ? "StreamingAssets 异步请求：" + _streaming.ReadCount + " 次" : _resources == null ? "尚未请求资源。" : "累计请求：同步 " + _resources.SyncReadCount + " 次 / 异步 " + _resources.AsyncReadCount + " 次",
                Trace = string.Join("\n", _trace.ToArray())
            };
        }

        private void OnGUI()
        {
            if (PlayerAcceptance.IsRequested)
            {
                GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, Screen.height - 40));
                _scroll = GUILayout.BeginScrollView(_scroll);
                GUILayout.TextArea(PlayerAcceptance.Status);
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                return;
            }

            if (_heading == null)
            {
                _heading = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold
                };
                _body = new GUIStyle(GUI.skin.label)
                {
                    wordWrap = true,
                    fontSize = 14
                };
            }

            // 在任何 Begin 之前准备数据；同一轮事件不增减控件或读取旧快照。
            if (Event.current.type == EventType.Layout || _view == null)
            {
                _view = CaptureView();
            }

            ViewState view = _view;
            Action clicked = null;
            float scale = Mathf.Max(0.5f, Mathf.Min(Screen.width / 1000f, Screen.height / 760f));
            Matrix4x4 previousMatrix = GUI.matrix;
            bool previousEnabled = GUI.enabled;
            try
            {
                GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));
                using (new GUILayout.AreaScope(new Rect(24, 20, Screen.width / scale - 48, Screen.height / scale - 40)))
                {
                    using (var scroll = new GUILayout.ScrollViewScope(_scroll))
                    {
                        _scroll = scroll.scrollPosition;
                        GUILayout.Label("ZrConfig · AB 加载示例", _heading);
                        GUILayout.Label(_streaming != null ? "StreamingAssets 异步读取 · Android / iOS / WebGL" : "临时 AB 桥接：真实本地文件 · 同步 / 异步 / 引用 / 分组 / 重载", _body);
                        GUILayout.Space(12);
                        GUILayout.Label(view.Status, _body);
                        GUILayout.Label("读取目录：" + DataFolder, _body);
                        using (new GUILayout.HorizontalScope())
                        {
                            GUI.enabled = previousEnabled && !view.Busy && !_busy;
                            GUI.enabled = previousEnabled && !view.Busy && !_busy && _streaming == null;
                            _useAsync = GUILayout.Toolbar(_useAsync ? 1 : 0, s_modes, GUILayout.Height(32)) == 1 || _streaming != null;
                            GUI.enabled = previousEnabled && !view.Busy && !_busy;
                            if (GUILayout.Button(view.Ready ? Mode + "重新加载配置" : Mode + "加载 / 重试", GUILayout.Height(32)))
                            {
                                clicked = ReloadConfiguration;
                            }
                        }

                        GUI.enabled = previousEnabled;
                        GUILayout.Label("重载只读取版本清单；点击查询时加载对应行，掉落分组查询加载掉落表。", _body);
                        GUILayout.Label(_useAsync ? "LoadAsync → AB.ReadAsync → 清单；GetHeroAsync → 英雄；GetSkillAsync → 引用。" : "Load → AB.Read → 清单；GetHero → 英雄；hero.Skill → 引用。",
                            _body);
                        GUILayout.Label("切换模式共用缓存；要观察完整读取链，切换后先重新加载，再点击查询。", _body);
                        GUILayout.Space(10);
                        GUILayout.Label(view.Overview, _body);
                        // 表清单与资源日志各占一个控件，加载完成也不会改变 Layout / Repaint 的控件数量。
                        GUILayout.Label(view.Tables, _body);
                        GUILayout.Space(10);
                        GUI.enabled = previousEnabled && view.Ready && !view.Busy && !_busy;
                        GUILayout.Label("01  英雄主键与对象引用", _heading);
                        using (new GUILayout.HorizontalScope())
                        {
                            Field("Hero Id", ref _heroId);
                            if (GUILayout.Button("查询英雄"))
                            {
                                clicked = QueryHero;
                            }

                            if (GUILayout.Button("查看空引用示例"))
                            {
                                clicked = () =>
                                {
                                    _heroId = "3";
                                    QueryHero();
                                };
                            }
                        }

                        GUILayout.Label(view.Hero, _body);
                        GUILayout.Space(10);
                        GUILayout.Label("02  掉落组查询", _heading);
                        using (new GUILayout.HorizontalScope())
                        {
                            Field("Group Id", ref _groupId);
                            if (GUILayout.Button("查询整组"))
                            {
                                clicked = QueryDrops;
                            }
                        }

                        GUILayout.Label(view.Drops, _body);
                        GUILayout.Space(10);
                        GUILayout.Label("03  技能三字段组合键", _heading);
                        using (new GUILayout.HorizontalScope())
                        {
                            Field("Skill Id", ref _skillId);
                            Field("Level", ref _level);
                            Field("Quality", ref _quality);
                            if (GUILayout.Button("查询技能"))
                            {
                                clicked = QuerySkill;
                            }
                        }

                        GUILayout.Label(view.Skill, _body);
                        GUI.enabled = previousEnabled;
                        GUILayout.Space(12);
                        GUILayout.Label("04  资源读取记录", _heading);
                        GUILayout.Label(view.Resources, _body);
                        GUILayout.Label(view.Trace, _body);
                    }
                }
            }
            finally
            {
                GUI.enabled = previousEnabled;
                GUI.matrix = previousMatrix;
            }

            // 同步加载也可能当场发布新快照；等所有布局作用域结束后再执行按钮操作。
            clicked?.Invoke();
        }

        private static void Field(string label, ref string value)
        {
            GUILayout.Label(label, GUILayout.Width(70));
            value = GUILayout.TextField(value, 10, GUILayout.Width(90));
        }

        private void OnDisable()
        {
            UnityConfigRuntime.Changed -= OnChanged;
            if (_resources != null)
            {
                _resources.Trace -= OnResourceTrace;
            }
            if (_streaming != null)
            {
                _streaming.Trace -= OnResourceTrace;
            }

            _lifetime?.Cancel();
            _lifetime?.Dispose();
            _lifetime = null;
            _busy = false;
            _snapshot = null;
            _view = null;
        }
    }
}
