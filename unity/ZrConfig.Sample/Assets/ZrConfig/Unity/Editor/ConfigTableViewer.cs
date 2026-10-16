using System;
using System.Linq;
using System.Reflection;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor;
using UnityEngine;
using ZrConfig.Runtime;

namespace ZrConfig.Unity.Editor
{
    // 窗口和检视面板共用当前运行时快照，不单独导入 Excel 副本。
    public sealed class ConfigTableWindow : EditorWindow
    {
        private ConfigTableDrawer _drawer;
        [MenuItem("Tools/ZrConfig/运行时配置表")]
        private static void Open() => GetWindow<ConfigTableWindow>("ZrConfig");
        private ConfigSnapshot _observedSnapshot;
        private int _observedTableCount;
        private void OnEnable() => EditorApplication.update += RefreshIfChanged;
        private void RefreshIfChanged()
        {
            ConfigSnapshot current = UnityConfigRuntime.Current;
            int count = current?.Tables.Count ?? 0;
            if (ReferenceEquals(current, _observedSnapshot) && count == _observedTableCount)
            {
                return;
            }

            _observedTableCount = count;
            _observedSnapshot = current;
            Repaint();
        }

        private void OnDisable()
        {
            EditorApplication.update -= RefreshIfChanged;
            _drawer?.Dispose();
            _drawer = null;
        }

        private void OnGUI()
        {
            if (_drawer == null)
            {
                _drawer = new ConfigTableDrawer();
            }

            _drawer.Draw(UnityConfigRuntime.Current, "");
        }
    }

    [CustomEditor(typeof(ConfigTableInspector))]
    public sealed class ConfigTableInspectorEditor : UnityEditor.Editor
    {
        private ConfigTableDrawer _drawer;
        private ConfigSnapshot _observedSnapshot;
        private int _observedTableCount;
        private readonly GUIContent _tableLabel = new GUIContent();
        private void OnEnable() => EditorApplication.update += RefreshIfChanged;
        private void RefreshIfChanged()
        {
            ConfigSnapshot current = UnityConfigRuntime.Current;
            int count = current?.Tables.Count ?? 0;
            if (ReferenceEquals(current, _observedSnapshot) && count == _observedTableCount)
            {
                return;
            }

            _observedTableCount = count;
            _observedSnapshot = current;
            Repaint();
        }

        private void OnDisable()
        {
            EditorApplication.update -= RefreshIfChanged;
            _drawer?.Dispose();
            _drawer = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _tableLabel.text = ConfigText.Get("表名", "Table name");
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ConfigTableInspector.TableName)), _tableLabel);
            serializedObject.ApplyModifiedProperties();
            if (_drawer == null)
            {
                _drawer = new ConfigTableDrawer();
            }

            _drawer.Draw(UnityConfigRuntime.Current, ((ConfigTableInspector)target).TableName);
        }
    }

    internal sealed class ConfigTableDrawer : IDisposable
    {
        private const int PageSize = 50;
        private static readonly GUILayoutOption[] s_columnWidth =
        {
            GUILayout.Width(150)
        };
        private ConfigSnapshot _availableSnapshot;
        private int _loadedTableCount;
        private string _filter;
        private IConfigTable[] _available = Array.Empty<IConfigTable>();
        private string[] _tableNames = Array.Empty<string>();
        private int _cachedPage = -1;
        private string[] _pageCells = Array.Empty<string>();
        private string _rowCountText = "";
        private ConfigSnapshot _snapshot;
        private IConfigTable _table;
        private string _selectedName = "";
        private PropertyInfo[] _properties = Array.Empty<PropertyInfo>();
        private Vector2 _scroll;
        private int _page;
#if ODIN_INSPECTOR
        private PropertyTree _tree;
#endif
        public void Draw(ConfigSnapshot snapshot, string tableFilter)
        {
            if (snapshot == null || snapshot.Tables.Count == 0)
            {
                Dispose();
                _table = null;
                _snapshot = null;
                _availableSnapshot = null;
                _available = Array.Empty<IConfigTable>();
                _tableNames = Array.Empty<string>();
                _pageCells = Array.Empty<string>();
                EditorGUILayout.HelpBox(ConfigText.Get("此处显示已完整加载的表。请先在游戏中加载所需整表；按行读取不会自动加载整表。",
                    "Only fully loaded tables appear here. Load a table in the game to inspect it; row loading does not load the whole table."),
                    MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField(ConfigText.Get("版本", "Version"), snapshot.VersionId);
            if (!ReferenceEquals(_availableSnapshot, snapshot) || _filter != tableFilter || _loadedTableCount != snapshot.Tables.Count)
            {
                _availableSnapshot = snapshot;
                _loadedTableCount = snapshot.Tables.Count;
                _filter = tableFilter;
                _available = snapshot.Tables.Where(t => string.IsNullOrEmpty(tableFilter) || t.Name == tableFilter).ToArray();
                _tableNames = _available.Select(t => t.Name).ToArray();
            }

            IConfigTable[] available = _available;
            if (available.Length == 0)
            {
                Dispose();
                _table = null;
                EditorGUILayout.HelpBox(ConfigText.Get("所选配置表尚未加载：",
                    "The selected table is not loaded: ") + tableFilter,
                    MessageType.Warning);
                return;
            }

            int selected = Array.IndexOf(_tableNames, _selectedName);
            selected = EditorGUILayout.Popup(ConfigText.Get("配置表", "Table"), Math.Max(0, selected), _tableNames);
            _selectedName = available[selected].Name;
            if (!ReferenceEquals(_snapshot, snapshot) || !ReferenceEquals(_table, available[selected]))
            {
                Dispose();
                _snapshot = snapshot;
                _table = available[selected];
                _page = 0;
                _cachedPage = -1;
                _rowCountText = _table.Rows.Count.ToString();
                _properties = _table.RowType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.GetIndexParameters()
                    .Length == 0)
                    .ToArray();
#if ODIN_INSPECTOR
                _tree = PropertyTree.Create(OdinTableAdapter.Create(_table));
#endif
            }

            EditorGUILayout.LabelField(ConfigText.Get("数据行数", "Rows"), _rowCountText);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
#if ODIN_INSPECTOR
            _tree.Draw(false);
#else
            int lastPage = Math.Max(0, (_table.Rows.Count - 1) / PageSize);
            _page = EditorGUILayout.IntSlider(ConfigText.Get("页码", "Page"), _page, 0, lastPage);
            if (_cachedPage != _page)
            {
                int start = _page * PageSize;
                int count = Math.Min(PageSize, _table.Rows.Count - start);
                int length = count * _properties.Length;
                if (_pageCells.Length != length)
                {
                    _pageCells = new string[length];
                }

                for (int row = 0; row < count; row++)
                {
                    for (int column = 0; column < _properties.Length; column++)
                    {
                        // 展示表格不访问有加载副作用的引用 getter。
                        _pageCells[row * _properties.Length + column] = typeof(IConfigRow).IsAssignableFrom(_properties[column].PropertyType) ? _properties[column].PropertyType.Name + ConfigText.Get("（按需引用）", " (lazy reference)") : _properties[column].GetValue(_table.Rows[start + row])?.ToString() ?? "";
                    }
                }

                _cachedPage = _page;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.BeginHorizontal();
                foreach (PropertyInfo p in _properties)
                {
                    EditorGUILayout.LabelField(p.Name, s_columnWidth);
                }

                EditorGUILayout.EndHorizontal();
                for (int i = _page * PageSize; i < Math.Min(_table.Rows.Count, (_page + 1) * PageSize); i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (int column = 0; column < _properties.Length; column++)
                    {
                        EditorGUILayout.TextField(_pageCells[(i - _page * PageSize) * _properties.Length + column], s_columnWidth);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

#endif
            EditorGUILayout.EndScrollView();
        }

        public void Dispose()
        {
#if ODIN_INSPECTOR
            _tree?.Dispose();
            _tree = null;
#endif
        }
    }
}
