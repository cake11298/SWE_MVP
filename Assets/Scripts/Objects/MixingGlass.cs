using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Interaction;

namespace BarSimulator.Objects
{
    /// <summary>
    /// 調酒杯組件 - 專門用於攪拌法調製雞尾酒的容器
    /// </summary>
    public class MixingGlass : Container
    {
        #region 序列化欄位

        [Header("Mixing Glass Settings")]
        [Tooltip("是否配有濾網")]
        [SerializeField] private bool hasStrainer = true;

        [Tooltip("濾網物件")]
        [SerializeField] private GameObject strainerObject;

        [Header("Auto-Stir Detection")]
        [Tooltip("是否自動偵測攪拌器")]
        [SerializeField] private bool autoDetectStirrer = true;

        [Tooltip("攪拌器偵測半徑")]
        [SerializeField] private float stirrerDetectionRadius = 0.1f;

        #endregion

        #region 私有欄位

        private Stirrer currentStirrer;
        private bool isBeingStirred;

        // Events
        public System.Action<Stirrer> OnStirrerInserted;
        public System.Action OnStirrerRemoved;

        #endregion

        #region Unity 生命週期

        protected override void Awake()
        {
            base.Awake();
            interactableType = InteractableType.Container;
            maxVolume = Constants.MixingGlassMaxVolume; // 500ml
        }

        protected override void Update()
        {
            base.Update();

            // Auto-detect stirrer if enabled
            if (autoDetectStirrer && !isBeingStirred)
            {
                DetectStirrer();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Detect stirrer insertion
            var stirrer = other.GetComponent<Stirrer>();
            if (stirrer != null && currentStirrer == null)
            {
                AttachStirrer(stirrer);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Detect stirrer removal
            var stirrer = other.GetComponent<Stirrer>();
            if (stirrer == currentStirrer)
            {
                DetachStirrer();
            }
        }

        #endregion

        #region 攪拌器管理

        /// <summary>
        /// 偵測附近的攪拌器
        /// </summary>
        private void DetectStirrer()
        {
            if (currentStirrer != null) return;

            Collider[] colliders = Physics.OverlapSphere(transform.position, stirrerDetectionRadius);
            foreach (var col in colliders)
            {
                var stirrer = col.GetComponent<Stirrer>();
                if (stirrer != null)
                {
                    AttachStirrer(stirrer);
                    break;
                }
            }
        }

        /// <summary>
        /// 附加攪拌器
        /// </summary>
        private void AttachStirrer(Stirrer stirrer)
        {
            currentStirrer = stirrer;
            isBeingStirred = true;

            OnStirrerInserted?.Invoke(stirrer);
            Debug.Log($"MixingGlass: Stirrer attached to {name}");

            // Subscribe to stir events
            stirrer.OnStirCompleted += OnStirCompleted;
        }

        /// <summary>
        /// 移除攪拌器
        /// </summary>
        private void DetachStirrer()
        {
            if (currentStirrer != null)
            {
                // Unsubscribe from events
                currentStirrer.OnStirCompleted -= OnStirCompleted;
            }

            currentStirrer = null;
            isBeingStirred = false;

            OnStirrerRemoved?.Invoke();
            Debug.Log($"MixingGlass: Stirrer detached from {name}");
        }

        /// <summary>
        /// 攪拌完成事件處理
        /// </summary>
        private void OnStirCompleted()
        {
            Debug.Log($"MixingGlass: Stirring completed in {name}");
            // Additional visual feedback can be added here
            TriggerWobble(0.15f);
        }

        #endregion

        #region 倒酒（帶濾網）

        /// <summary>
        /// 倒酒到目標容器（透過濾網）
        /// </summary>
        public override void PourInto(Container target, float amount)
        {
            if (hasStrainer)
            {
                // Show strainer during pour
                if (strainerObject != null)
                {
                    strainerObject.SetActive(true);
                }

                // Pour with straining
                base.PourInto(target, amount);

                // Hide strainer after pour
                if (strainerObject != null)
                {
                    strainerObject.SetActive(false);
                }

                Debug.Log($"MixingGlass: Poured {amount}ml through strainer");
            }
            else
            {
                // Regular pour
                base.PourInto(target, amount);
            }
        }

        #endregion

        #region IInteractable 覆寫

        public override string GetInteractPrompt()
        {
            if (currentStirrer != null && !contents.IsEmpty)
            {
                return currentStirrer.IsStirring
                    ? $"E: Stop Stirring ({currentStirrer.StirProgress * 100:F0}%)"
                    : "E: Start Stirring";
            }

            if (!contents.IsEmpty)
            {
                return $"E: Pick Up ({contents.volume:F0}ml)";
            }

            return "E: Pick Up";
        }

        public override void OnInteract()
        {
            // If stirrer is attached, toggle stirring
            if (currentStirrer != null && !contents.IsEmpty)
            {
                currentStirrer.OnInteract();
            }
            else
            {
                base.OnInteract();
            }
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 是否正在被攪拌
        /// </summary>
        public bool IsBeingStirred => isBeingStirred && currentStirrer != null && currentStirrer.IsStirring;

        /// <summary>
        /// 當前攪拌器
        /// </summary>
        public Stirrer CurrentStirrer => currentStirrer;

        /// <summary>
        /// 是否有濾網
        /// </summary>
        public bool HasStrainer => hasStrainer;

        #endregion
    }
}
