using UnityEngine;
using BarSimulator.Objects;
using BarSimulator.Systems;
using BarSimulator.UI;

namespace BarSimulator.Player
{
    /// <summary>
    /// Shaker Controller - 處理玩家與Shaker的互動
    /// 按住右鍵開始QTE搖酒
    /// </summary>
    public class ShakerController : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("References")]
        [Tooltip("玩家手持位置")]
        [SerializeField] private Transform handPosition;
        
        [Tooltip("Shaker動畫設定")]
        [SerializeField] private Vector3 shakePositionOffset = new Vector3(0f, 0f, 0.3f);
        
        [Tooltip("Shaker搖晃時的傾斜角度")]
        [SerializeField] private float shakeTiltAngle = 15f;
        
        [Tooltip("Shaker搖晃時的抖動強度")]
        [SerializeField] private float shakeVibrateIntensity = 0.02f;
        
        [Tooltip("Shaker搖晃時的抖動頻率")]
        [SerializeField] private float shakeVibrateFrequency = 30f;
        
        #endregion

        #region Private Fields
        
        private GameObject currentShakerObject;
        private ShakerContainer currentShakerContainer;
        private ShakerQTESystem currentQTESystem;
        private Vector3 originalHandPosition;
        private Quaternion originalHandRotation;
        private bool isHoldingShaker;
        private float shakeAnimationTime;
        private bool isShaking;
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            if (handPosition == null)
            {
                // 嘗試從相機找手持位置
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    Transform hand = cam.transform.Find("HandPosition");
                    if (hand != null)
                    {
                        handPosition = hand;
                    }
                }
            }

            if (handPosition != null)
            {
                originalHandPosition = handPosition.localPosition;
                originalHandRotation = handPosition.localRotation;
            }
        }

        private void Update()
        {
            if (!isHoldingShaker || currentShakerObject == null) return;

            // 檢查右鍵輸入
            if (Input.GetMouseButtonDown(1))
            {
                StartShaking();
            }

            if (Input.GetMouseButtonUp(1))
            {
                StopShaking();
            }

            // 更新搖晃動畫
            if (isShaking)
            {
                UpdateShakeAnimation();
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// 設置當前持有的Shaker（支持Shaker或ShakerContainer）
        /// </summary>
        public void SetShaker(GameObject shakerObj)
        {
            currentShakerObject = shakerObj;
            
            if (shakerObj != null)
            {
                currentShakerContainer = shakerObj.GetComponent<ShakerContainer>();
                currentQTESystem = shakerObj.GetComponent<ShakerQTESystem>();
                
                if (currentQTESystem != null)
                {
                    currentQTESystem.OnQTEComplete += OnQTEComplete;
                }
            }
            
            isHoldingShaker = shakerObj != null;

            if (isHoldingShaker && handPosition != null)
            {
                // 重置手持位置
                handPosition.localPosition = originalHandPosition;
                handPosition.localRotation = originalHandRotation;
            }
        }

        /// <summary>
        /// 設置當前持有的Shaker（舊版兼容）
        /// </summary>
        public void SetShaker(Shaker shaker)
        {
            if (shaker != null)
            {
                SetShaker(shaker.gameObject);
            }
            else
            {
                ClearShaker();
            }
        }

        /// <summary>
        /// 清除當前Shaker
        /// </summary>
        public void ClearShaker()
        {
            if (isShaking && currentQTESystem != null)
            {
                currentQTESystem.CancelQTE();
            }

            if (currentQTESystem != null)
            {
                currentQTESystem.OnQTEComplete -= OnQTEComplete;
            }

            currentShakerObject = null;
            currentShakerContainer = null;
            currentQTESystem = null;
            isHoldingShaker = false;
            isShaking = false;

            if (handPosition != null)
            {
                handPosition.localPosition = originalHandPosition;
                handPosition.localRotation = originalHandRotation;
            }
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// 開始搖晃
        /// </summary>
        private void StartShaking()
        {
            if (currentShakerContainer == null || currentQTESystem == null) return;

            if (currentShakerContainer.IsEmpty())
            {
                UIPromptManager.Show("Shaker is empty!");
                return;
            }

            if (currentShakerContainer.isShaken)
            {
                UIPromptManager.Show("Already shaken! Add new ingredients to shake again.");
                return;
            }

            // 啟動QTE
            currentQTESystem.StartQTE();
            isShaking = true;
            shakeAnimationTime = 0f;
            
            Debug.Log("ShakerController: Started shaking with QTE");
        }

        /// <summary>
        /// 停止搖晃
        /// </summary>
        private void StopShaking()
        {
            if (currentQTESystem == null) return;

            // 取消QTE
            currentQTESystem.CancelQTE();
            isShaking = false;

            // 重置手持位置
            if (handPosition != null)
            {
                handPosition.localPosition = originalHandPosition;
                handPosition.localRotation = originalHandRotation;
            }
            
            Debug.Log("ShakerController: Stopped shaking");
        }

        /// <summary>
        /// QTE完成回調
        /// </summary>
        private void OnQTEComplete(bool success)
        {
            isShaking = false;

            if (success && currentShakerContainer != null)
            {
                // 標記為已搖晃
                currentShakerContainer.isShaken = true;
                UIPromptManager.Show("Successful shaken!");
                Debug.Log("ShakerController: QTE Success! Shaker contents marked as shaken.");
            }
            else
            {
                UIPromptManager.Show("Shake Failed! Please try again.");
                Debug.Log("ShakerController: QTE Failed!");
            }

            // 重置手持位置
            if (handPosition != null)
            {
                handPosition.localPosition = originalHandPosition;
                handPosition.localRotation = originalHandRotation;
            }
        }

        /// <summary>
        /// 更新搖晃動畫
        /// </summary>
        private void UpdateShakeAnimation()
        {
            if (handPosition == null) return;

            shakeAnimationTime += Time.deltaTime;

            // 計算置中位置（從右下角移到中間）
            Vector3 targetPosition = originalHandPosition + shakePositionOffset;

            // 添加抖動效果
            float vibrateX = Mathf.Sin(shakeAnimationTime * shakeVibrateFrequency) * shakeVibrateIntensity;
            float vibrateY = Mathf.Cos(shakeAnimationTime * shakeVibrateFrequency * 1.3f) * shakeVibrateIntensity;
            float vibrateZ = Mathf.Sin(shakeAnimationTime * shakeVibrateFrequency * 0.7f) * shakeVibrateIntensity * 0.5f;

            Vector3 vibrateOffset = new Vector3(vibrateX, vibrateY, vibrateZ);

            // 平滑移動到目標位置
            handPosition.localPosition = Vector3.Lerp(
                handPosition.localPosition,
                targetPosition + vibrateOffset,
                Time.deltaTime * 10f
            );

            // 添加傾斜和搖晃旋轉
            float tiltX = Mathf.Sin(shakeAnimationTime * shakeVibrateFrequency * 0.5f) * shakeTiltAngle;
            float tiltZ = Mathf.Cos(shakeAnimationTime * shakeVibrateFrequency * 0.7f) * shakeTiltAngle * 0.5f;

            Quaternion targetRotation = originalHandRotation * Quaternion.Euler(tiltX, 0f, tiltZ);

            handPosition.localRotation = Quaternion.Lerp(
                handPosition.localRotation,
                targetRotation,
                Time.deltaTime * 8f
            );
        }
        
        #endregion

        #region Public Properties
        
        public bool IsHoldingShaker => isHoldingShaker;
        public GameObject CurrentShakerObject => currentShakerObject;
        
        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (currentQTESystem != null)
            {
                currentQTESystem.OnQTEComplete -= OnQTEComplete;
            }
        }

        #endregion
    }
}
