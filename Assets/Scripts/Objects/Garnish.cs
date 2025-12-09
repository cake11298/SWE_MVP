using UnityEngine;
using BarSimulator.Interaction;

namespace BarSimulator.Objects
{
    /// <summary>
    /// 裝飾物類型
    /// </summary>
    public enum GarnishType
    {
        LemonSlice,      // 檸檬片
        LimeSlice,       // 萊姆片
        OrangeSlice,     // 柳橙片
        Cherry,          // 櫻桃
        Olive,           // 橄欖
        Mint,            // 薄荷葉
        LemonPeel,       // 檸檬皮捲
        OrangePeel,      // 柳橙皮捲
        SaltRim,         // 杯緣鹽
        SugarRim,        // 杯緣糖
        Umbrella,        // 小傘
        Straw            // 吸管
    }

    /// <summary>
    /// 裝飾物基礎類別
    /// </summary>
    public class Garnish : InteractableBase
    {
        #region 序列化欄位

        [Header("Garnish Info")]
        [Tooltip("裝飾物類型")]
        [SerializeField] private GarnishType garnishType;

        [Tooltip("裝飾物名稱")]
        [SerializeField] private string garnishName;

        [Tooltip("裝飾物描述")]
        [TextArea(2, 3)]
        [SerializeField] private string description;

        [Header("Attachment")]
        [Tooltip("可附著的容器類型")]
        [SerializeField] private bool canAttachToGlass = true;

        [Tooltip("附著位置偏移")]
        [SerializeField] private Vector3 attachmentOffset = new Vector3(0f, 0.08f, 0.05f);

        [Tooltip("附著旋转")]
        [SerializeField] private Vector3 attachmentRotation = Vector3.zero;

        #endregion

        #region 私有欄位

        private bool isPickedUp;
        private bool isAttached;
        private Container attachedContainer;
        private Transform originalParent;
        private Rigidbody rb;
        private Collider col;

        // Events
        public System.Action<Container> OnAttached;
        public System.Action OnDetached;

        #endregion

        #region Unity 生命週期

        protected override void Awake()
        {
            base.Awake();

            interactableType = InteractableType.Ingredient;
            displayName = garnishName;
            canPickup = true;

            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.mass = 0.01f; // Very light
            }

            col = GetComponent<Collider>();
            if (col == null)
            {
                col = gameObject.AddComponent<SphereCollider>();
                ((SphereCollider)col).radius = 0.02f;
            }

            // Store original parent
            originalParent = transform.parent;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Auto-attach to container when picked up and brought near
            if (!isPickedUp || isAttached) return;
            if (!canAttachToGlass) return;

            var container = other.GetComponent<Container>();
            if (container != null)
            {
                AttachToContainer(container);
            }
        }

        #endregion

        #region 附著系統

        /// <summary>
        /// 附著到容器
        /// </summary>
        public void AttachToContainer(Container container)
        {
            if (isAttached || container == null) return;

            isAttached = true;
            attachedContainer = container;

            // Parent to container
            transform.SetParent(container.transform);

            // Set local position and rotation
            transform.localPosition = attachmentOffset;
            transform.localRotation = Quaternion.Euler(attachmentRotation);

            // Disable physics
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            OnAttached?.Invoke(container);
            Debug.Log($"Garnish: {garnishName} attached to {container.name}");
        }

        /// <summary>
        /// 從容器分離
        /// </summary>
        public void DetachFromContainer()
        {
            if (!isAttached) return;

            isAttached = false;
            attachedContainer = null;

            // Unparent
            transform.SetParent(originalParent);

            // Re-enable physics
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            OnDetached?.Invoke();
            Debug.Log($"Garnish: {garnishName} detached");
        }

        #endregion

        #region IInteractable 覆寫

        public override void OnInteract()
        {
            // Toggle attachment if near container
            if (isAttached)
            {
                DetachFromContainer();
            }
        }

        public override void OnPickup()
        {
            base.OnPickup();

            isPickedUp = true;

            // Detach if currently attached
            if (isAttached)
            {
                DetachFromContainer();
            }

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Debug.Log($"Garnish: {garnishName} picked up");
        }

        public override void OnDrop(bool returnToOriginal)
        {
            base.OnDrop(returnToOriginal);

            isPickedUp = false;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            Debug.Log($"Garnish: {garnishName} dropped");
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 裝飾物類型
        /// </summary>
        public GarnishType Type => garnishType;

        /// <summary>
        /// 裝飾物名稱
        /// </summary>
        public string GarnishName => garnishName;

        /// <summary>
        /// 描述
        /// </summary>
        public string Description => description;

        /// <summary>
        /// 是否已附著
        /// </summary>
        public bool IsAttached => isAttached;

        /// <summary>
        /// 附著的容器
        /// </summary>
        public Container AttachedContainer => attachedContainer;

        #endregion
    }
}
