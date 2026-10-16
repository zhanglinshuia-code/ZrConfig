using UnityEngine;

namespace ZrConfig.Unity
{
    [AddComponentMenu("ZrConfig/配置表查看器")]
    public sealed class ConfigTableInspector : MonoBehaviour
    {
        [Tooltip("Excel 文件名（不含扩展名）；留空可选择任意已加载的表。")]
        public string TableName = "";
    }
}
