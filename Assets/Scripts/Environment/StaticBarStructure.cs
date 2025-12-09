using UnityEngine;

namespace BarSimulator.Environment
{
    /// <summary>
    /// 靜態酒吧結構標記組件
    /// 用於標識在 Unity Editor 中手動創建的場景結構（地板、牆壁、吧台等）
    /// 這些物件不應該在運行時被動態生成
    /// </summary>
    public class StaticBarStructure : MonoBehaviour
    {
        #region 序列化欄位

        [Header("結構資訊")]
        [Tooltip("結構類型")]
        [SerializeField] private StructureType structureType = StructureType.Other;

        [Tooltip("結構描述")]
        [SerializeField] private string description = "";

        [Header("重要位置標記")]
        [Tooltip("是否標記為酒瓶架子")]
        [SerializeField] private bool isBottleShelf = false;

        [Tooltip("是否標記為玻璃杯放置區")]
        [SerializeField] private bool isGlassStation = false;

        [Tooltip("是否標記為調酒區")]
        [SerializeField] private bool isMixingStation = false;

        #endregion

        #region 公開屬性

        public StructureType Type => structureType;
        public string Description => description;
        public bool IsBottleShelf => isBottleShelf;
        public bool IsGlassStation => isGlassStation;
        public bool IsMixingStation => isMixingStation;

        #endregion

        #region Editor 輔助

        private void OnDrawGizmos()
        {
            // 在 Scene 視圖中顯示結構類型
            Gizmos.color = GetGizmoColor();
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }

        private void OnDrawGizmosSelected()
        {
            // 選中時顯示更明顯的標記
            Gizmos.color = GetGizmoColor();
            Gizmos.DrawWireCube(transform.position, transform.localScale);

            // 特殊標記
            if (isBottleShelf)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.2f);
            }

            if (isGlassStation)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.2f);
            }

            if (isMixingStation)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.2f);
            }
        }

        private Color GetGizmoColor()
        {
            switch (structureType)
            {
                case StructureType.Floor:
                    return new Color(0.5f, 0.3f, 0.1f, 0.5f);
                case StructureType.Wall:
                    return new Color(0.6f, 0.5f, 0.4f, 0.5f);
                case StructureType.BarCounter:
                    return new Color(0.4f, 0.2f, 0.1f, 0.5f);
                case StructureType.Shelf:
                    return new Color(0.3f, 0.3f, 0.5f, 0.5f);
                case StructureType.Decoration:
                    return new Color(0.8f, 0.8f, 0.2f, 0.5f);
                default:
                    return new Color(0.5f, 0.5f, 0.5f, 0.3f);
            }
        }

        #endregion
    }

    /// <summary>
    /// 結構類型枚舉
    /// </summary>
    public enum StructureType
    {
        Floor,          // 地板
        Wall,           // 牆壁
        Ceiling,        // 天花板
        BarCounter,     // 吧台
        Shelf,          // 架子
        Decoration,     // 裝飾
        Lighting,       // 燈光
        Other           // 其他
    }
}
