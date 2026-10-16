#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using ZrConfig.Runtime;

namespace ZrConfig.Unity.Editor
{
    internal static class OdinTableAdapter
    {
        // 反射仅用于编辑器；运行时的编解码和表对象构造使用生成的直接调用。
        public static object Create(IConfigTable table) => Activator.CreateInstance(typeof(TableView<>).MakeGenericType(table.RowType),
            table);
        private sealed class TableView<T>
            where T : class, IConfigRow
        {
            [ShowInInspector, LabelText("配置数据"), TableList(IsReadOnly = true, ShowPaging = true), ReadOnly]
            public List<T> Rows
            {
                get;
            }

            public TableView(IConfigTable table)
            {
                Rows = new List<T>();
                foreach (T row in table.Rows)
                {
                    Rows.Add(row);
                }
            }
        }
    }

    // 通过编辑器适配显示生成的只读属性，避免模型程序集依赖 Odin。
    public sealed class ConfigRowAttributeProcessor : OdinAttributeProcessor
    {
        public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member) => member is PropertyInfo property && property.GetMethod != null && property.GetIndexParameters()
            .Length == 0 && typeof(IConfigRow).IsAssignableFrom(member.DeclaringType);
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            if (member is PropertyInfo property && typeof(IConfigRow).IsAssignableFrom(property.PropertyType))
            {
                attributes.Add(new UnityEngine.HideInInspector());
                return;
            }

            attributes.Add(new ShowInInspectorAttribute());
            attributes.Add(new ReadOnlyAttribute());
        }
    }
}
#endif
