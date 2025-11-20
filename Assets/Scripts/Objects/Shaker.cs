using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Interaction;

namespace BarSimulator.Objects
{
    /// <summary>
    /// 搖酒器組件 - 可以搖酒和倒酒的容器
    /// </summary>
    public class Shaker : Container
    {
        #region 序列化欄位

        [Header("搖酒設定")]
        [Tooltip("搖晃強度")]
        [SerializeField] private float shakeIntensity = 0.05f;

        [Tooltip("搖晃頻率")]
        [SerializeField] private float shakeFrequency = 20f;

        [Tooltip("需要搖晃的最短時間（秒）才能完成混合")]
        [SerializeField] private float minShakeTime = 2f;

        [Header("倒酒設定")]
        [Tooltip("倒酒時的傾斜角度")]
        [SerializeField] private float pourTiltAngle = 60f;

        [Tooltip("傾斜速度")]
        [SerializeField] private float tiltSpeed = 3f;

        #endregion

        #region 私有欄位

        private bool isShaking;
        private float shakeTime;
        private bool isPouring;
        private float currentTilt;
        private Quaternion baseRotation;

        // 事件
        public System.Action OnShakeCompleted;

        #endregion

        #region Unity 生命週期

        protected override void Awake()
        {
            base.Awake();
            interactableType = InteractableType.Shaker;
            maxVolume = Constants.ShakerMaxVolume;
        }

        private void Update()
        {
            if (isShaking)
            {
                UpdateShakeAnimation();
            }
            else if (isPouring)
            {
                UpdatePourAnimation();
            }
        }

        #endregion

        #region 搖酒

        /// <summary>
        /// 開始搖酒
        /// 參考: CocktailSystem.js shake() Line 686-707
        /// </summary>
        public void StartShaking()
        {
            if (contents.IsEmpty) return;

            isShaking = true;
            baseRotation = transform.localRotation;
        }

        /// <summary>
        /// 停止搖酒
        /// 參考: CocktailSystem.js stopShaking() Line 713-720
        /// </summary>
        public void StopShaking()
        {
            if (!isShaking) return;

            // 檢查是否搖夠久
            if (shakeTime >= minShakeTime)
            {
                // 增強混合
                contents.UpdateMixedColor();
                UpdateLiquidVisual();
                OnShakeCompleted?.Invoke();
            }

            // 重置
            isShaking = false;
            shakeTime = 0f;
            transform.localRotation = baseRotation;
        }

        /// <summary>
        /// 更新搖晃動畫
        /// </summary>
        private void UpdateShakeAnimation()
        {
            shakeTime += Time.deltaTime;

            // 正弦波搖晃
            float shakeZ = Mathf.Sin(shakeTime * shakeFrequency) * shakeIntensity;
            float shakeX = Mathf.Sin(shakeTime * shakeFrequency * 0.75f) * shakeIntensity * 0.6f;

            transform.localRotation = baseRotation * Quaternion.Euler(
                shakeX * Mathf.Rad2Deg,
                0f,
                shakeZ * Mathf.Rad2Deg
            );
        }

        #endregion

        #region 倒酒

        /// <summary>
        /// 開始倒酒
        /// </summary>
        public void StartPouring()
        {
            if (contents.IsEmpty) return;
            isPouring = true;
        }

        /// <summary>
        /// 停止倒酒
        /// </summary>
        public void StopPouring()
        {
            isPouring = false;
        }

        /// <summary>
        /// 更新傾斜動畫
        /// </summary>
        private void UpdatePourAnimation()
        {
            float targetTilt = pourTiltAngle;
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
            transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, currentTilt);
        }

        #endregion

        #region IInteractable 覆寫

        public override void OnPickup()
        {
            base.OnPickup();
            baseRotation = Quaternion.identity;
        }

        public override void OnDrop(bool returnToOriginal)
        {
            base.OnDrop(returnToOriginal);
            isShaking = false;
            isPouring = false;
            shakeTime = 0f;
            currentTilt = 0f;
        }

        #endregion

        #region 公開方法

        /// <summary>
        /// 清空搖酒器
        /// </summary>
        public void Empty()
        {
            Clear();
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 是否正在搖酒
        /// </summary>
        public bool IsShaking => isShaking;

        /// <summary>
        /// 是否正在倒酒
        /// </summary>
        public bool IsPouring => isPouring;

        /// <summary>
        /// 搖酒時間
        /// </summary>
        public float ShakeTime => shakeTime;

        /// <summary>
        /// 是否完成搖酒
        /// </summary>
        public bool IsShakeComplete => shakeTime >= minShakeTime;

        #endregion
    }
}
