using UnityEngine;
using BarSimulator.Objects;
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
        
        private Shaker currentShaker;
        private Vector3 originalHandPosition;
        private Quaternion originalHandRotation;
        private bool isHoldingShaker;
        private float shakeAnimationTime;
        
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
            if (!isHoldingShaker || currentShaker == null) return;

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
            if (currentShaker.IsShaking)
            {
                UpdateShakeAnimation();
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// 設置當前持有的Shaker
        /// </summary>
        public void SetShaker(Shaker shaker)
        {
            currentShaker = shaker;
            isHoldingShaker = shaker != null;

            if (isHoldingShaker && handPosition != null)
            {
                // 重置手持位置
                handPosition.localPosition = originalHandPosition;
                handPosition.localRotation = originalHandRotation;
            }
        }

        /// <summary>
        /// 清除當前Shaker
        /// </summary>
        public void ClearShaker()
        {
            if (currentShaker != null && currentShaker.IsShaking)
            {
                currentShaker.StopShaking();
            }

            currentShaker = null;
            isHoldingShaker = false;

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
            if (currentShaker == null) return;

            if (currentShaker.IsEmpty)
            {
                UIPromptManager.Show("Shaker is empty!");
                return;
            }

            if (currentShaker.IsShaken)
            {
                UIPromptManager.Show("Already shaken! Add new ingredients to shake again.");
                return;
            }

            currentShaker.StartShaking();
            shakeAnimationTime = 0f;
        }

        /// <summary>
        /// 停止搖晃
        /// </summary>
        private void StopShaking()
        {
            if (currentShaker == null) return;

            currentShaker.StopShaking();

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
        public Shaker CurrentShaker => currentShaker;
        
        #endregion
    }
}
