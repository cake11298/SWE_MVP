using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Interaction;

namespace BarSimulator.Objects
{
    /// <summary>
    /// 冰塊組件 - 可以降低飲品溫度並稀釋飲品
    /// </summary>
    public class IceCube : MonoBehaviour, IInteractable
    {
        #region 序列化欄位

        [Header("Ice Properties")]
        [Tooltip("冰塊溫度（°C）")]
        [SerializeField] private float temperature = -5f;

        [Tooltip("冰塊大小（影響融化速度）")]
        [SerializeField] private float size = 1f;

        [Tooltip("融化速度（ml/秒）")]
        [SerializeField] private float meltRate = 0.5f;

        [Tooltip("冷卻效果（°C/秒）")]
        [SerializeField] private float coolingRate = 5f;

        [Header("Visual")]
        [Tooltip("冰塊模型")]
        [SerializeField] private GameObject iceModel;

        [Tooltip("融化粒子效果")]
        [SerializeField] private ParticleSystem meltParticles;

        #endregion

        #region 私有欄位

        private bool isPickedUp;
        private bool isInContainer;
        private Container currentContainer;
        private float currentSize;
        private bool isMelting;

        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Rigidbody rb;

        // Events
        public System.Action OnFullyMelted;
        public System.Action<Container> OnAddedToContainer;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.mass = 0.05f;
            }

            originalPosition = transform.position;
            originalRotation = transform.rotation;
            currentSize = size;
        }

        private void Update()
        {
            if (isInContainer && currentContainer != null)
            {
                UpdateMelting();
                UpdateCooling();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Ice enters a container
            if (isPickedUp && !isInContainer)
            {
                var container = other.GetComponent<Container>();
                if (container != null)
                {
                    AddToContainer(container);
                }
            }
        }

        #endregion

        #region 容器互動

        /// <summary>
        /// 加入容器
        /// </summary>
        private void AddToContainer(Container container)
        {
            if (isInContainer) return;

            isInContainer = true;
            currentContainer = container;
            isMelting = true;

            // Parent to container
            transform.SetParent(container.transform);
            transform.localPosition = Vector3.up * 0.05f; // Float at top

            // Disable physics
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Start melt particles
            if (meltParticles != null)
            {
                meltParticles.Play();
            }

            OnAddedToContainer?.Invoke(container);
            Debug.Log($"IceCube: Added to {container.name}");
        }

        /// <summary>
        /// 從容器移除
        /// </summary>
        private void RemoveFromContainer()
        {
            if (!isInContainer) return;

            isInContainer = false;
            isMelting = false;
            currentContainer = null;

            // Unparent
            transform.SetParent(null);

            // Re-enable physics
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            // Stop particles
            if (meltParticles != null)
            {
                meltParticles.Stop();
            }

            Debug.Log("IceCube: Removed from container");
        }

        #endregion

        #region 融化系統

        /// <summary>
        /// 更新融化
        /// </summary>
        private void UpdateMelting()
        {
            if (!isMelting || currentContainer == null) return;

            // Melt rate depends on size and container temperature
            float containerTemp = currentContainer.Temperature;
            float tempDiff = containerTemp - temperature;
            float adjustedMeltRate = meltRate * (tempDiff / 25f); // Faster melting at higher temp

            // Melt over time
            float meltAmount = adjustedMeltRate * Time.deltaTime;
            currentSize -= meltAmount / 10f; // Size decreases

            // Add melted water to container
            if (currentContainer.Volume < currentContainer.MaxVolume)
            {
                // Add water (dilution)
                currentContainer.AddLiquid("water", meltAmount);
            }

            // Update visual size
            if (iceModel != null)
            {
                float sizeRatio = currentSize / size;
                iceModel.transform.localScale = Vector3.one * sizeRatio;
            }

            // Check if fully melted
            if (currentSize <= 0f)
            {
                FullyMelted();
            }
        }

        /// <summary>
        /// 完全融化
        /// </summary>
        private void FullyMelted()
        {
            Debug.Log("IceCube: Fully melted");
            OnFullyMelted?.Invoke();

            // Clean up
            if (meltParticles != null)
            {
                meltParticles.Stop();
            }

            Destroy(gameObject);
        }

        #endregion

        #region 冷卻系統

        /// <summary>
        /// 更新冷卻效果
        /// </summary>
        private void UpdateCooling()
        {
            if (currentContainer == null) return;

            // Cool the container contents
            float targetTemp = Mathf.Lerp(currentContainer.Temperature, temperature, coolingRate * Time.deltaTime / 10f);
            currentContainer.SetTemperature(targetTemp);
        }

        #endregion

        #region IInteractable 實作

        public InteractableType GetInteractableType()
        {
            return InteractableType.Ingredient;
        }

        public void OnInteract()
        {
            // Remove from container if inside one
            if (isInContainer)
            {
                RemoveFromContainer();
            }
        }

        public void OnPickup()
        {
            isPickedUp = true;

            // Remove from container if inside
            if (isInContainer)
            {
                RemoveFromContainer();
            }

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Debug.Log("IceCube: Picked up");
        }

        public void OnDrop(bool returnToOriginal)
        {
            isPickedUp = false;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            if (returnToOriginal && !isInContainer)
            {
                transform.position = originalPosition;
                transform.rotation = originalRotation;
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }

            Debug.Log("IceCube: Dropped");
        }

        public bool CanInteract()
        {
            return true;
        }

        public string GetInteractPrompt()
        {
            if (isPickedUp)
            {
                return isInContainer ? "E: Remove Ice" : "Add to glass";
            }
            return "E: Pick Up Ice";
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 冰塊溫度
        /// </summary>
        public float Temperature => temperature;

        /// <summary>
        /// 當前大小
        /// </summary>
        public float CurrentSize => currentSize;

        /// <summary>
        /// 是否在容器中
        /// </summary>
        public bool IsInContainer => isInContainer;

        /// <summary>
        /// 融化進度 (0-1, 1 = 完全融化)
        /// </summary>
        public float MeltProgress => 1f - (currentSize / size);

        #endregion
    }
}
