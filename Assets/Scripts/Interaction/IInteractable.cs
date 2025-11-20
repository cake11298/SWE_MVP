using UnityEngine;

namespace BarSimulator.Interaction
{
    /// <summary>
    /// 可互動物件類型
    /// </summary>
    public enum InteractableType
    {
        /// <summary>酒瓶</summary>
        Bottle,
        /// <summary>杯子</summary>
        Glass,
        /// <summary>搖酒器</summary>
        Shaker,
        /// <summary>量酒器</summary>
        Jigger,
        /// <summary>吉他</summary>
        Guitar,
        /// <summary>其他</summary>
        Other
    }

    /// <summary>
    /// 可互動物件介面
    /// 參考: InteractionSystem.js registerInteractable()
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// 物件類型
        /// </summary>
        InteractableType InteractableType { get; }

        /// <summary>
        /// 顯示名稱
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// 原始位置（用於放回原位）
        /// </summary>
        Vector3 OriginalPosition { get; }

        /// <summary>
        /// 是否可以被拾取
        /// </summary>
        bool CanPickup { get; }

        /// <summary>
        /// 當被瞄準時呼叫
        /// </summary>
        void OnTargeted();

        /// <summary>
        /// 當不再被瞄準時呼叫
        /// </summary>
        void OnUntargeted();

        /// <summary>
        /// 當被拾取時呼叫
        /// </summary>
        void OnPickup();

        /// <summary>
        /// 當被放下時呼叫
        /// </summary>
        /// <param name="returnToOriginal">是否放回原位</param>
        void OnDrop(bool returnToOriginal);

        /// <summary>
        /// 當被互動時呼叫（如：吉他彈奏）
        /// </summary>
        void OnInteract();
    }

    /// <summary>
    /// 可互動物件基礎類別
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("互動設定")]
        [Tooltip("物件類型")]
        [SerializeField] protected InteractableType interactableType;

        [Tooltip("顯示名稱")]
        [SerializeField] protected string displayName;

        [Tooltip("是否可以被拾取")]
        [SerializeField] protected bool canPickup = true;

        protected Vector3 originalPosition;
        protected Quaternion originalRotation;

        #region IInteractable 實作

        public virtual InteractableType InteractableType => interactableType;

        public virtual string DisplayName => displayName;

        public virtual Vector3 OriginalPosition => originalPosition;

        public virtual bool CanPickup => canPickup;

        public virtual void OnTargeted()
        {
            // 可覆寫：高亮顯示等
        }

        public virtual void OnUntargeted()
        {
            // 可覆寫：取消高亮等
        }

        public virtual void OnPickup()
        {
            // 可覆寫：禁用物理等
        }

        public virtual void OnDrop(bool returnToOriginal)
        {
            if (returnToOriginal)
            {
                transform.position = originalPosition;
                transform.rotation = originalRotation;
            }
        }

        public virtual void OnInteract()
        {
            // 可覆寫：特殊互動
        }

        #endregion

        #region Unity 生命週期

        protected virtual void Awake()
        {
            // 儲存原始位置與旋轉
            originalPosition = transform.position;
            originalRotation = transform.rotation;
        }

        #endregion

        #region 輔助方法

        /// <summary>
        /// 重設原始位置
        /// </summary>
        public void SetOriginalPosition(Vector3 position)
        {
            originalPosition = position;
        }

        /// <summary>
        /// 重設原始旋轉
        /// </summary>
        public void SetOriginalRotation(Quaternion rotation)
        {
            originalRotation = rotation;
        }

        /// <summary>
        /// 取得物件類型名稱（中文）
        /// 參考: InteractionSystem.js getTypeName() Line 419-428
        /// </summary>
        public static string GetTypeName(InteractableType type)
        {
            return type switch
            {
                InteractableType.Bottle => "酒瓶",
                InteractableType.Glass => "杯子",
                InteractableType.Shaker => "搖酒器",
                InteractableType.Jigger => "量酒器",
                InteractableType.Guitar => "吉他",
                _ => "物品"
            };
        }

        #endregion
    }
}
