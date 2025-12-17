using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Interaction;
using BarSimulator.Systems;
using BarSimulator.Data;

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

        [Tooltip("倒酒速度 (ml/s)")]
        [SerializeField] private float pourRate = 20f;

        [Tooltip("倒酒檢測距離")]
        [SerializeField] private float pourCheckDistance = 1.0f;

        [Tooltip("倒酒點 (如果為空則自動尋找 PourPoint 子物件)")]
        [SerializeField] private Transform pourPoint;
        [SerializeField] private AudioSource shakerAudioSource;

        #endregion

        #region 私有欄位

        private bool isShaking;
        private float shakeTime;
        private bool isPouringAnimation;
        private float currentTilt;
        private Quaternion baseRotation;
        private bool isQTEActive;

        // Visual feedback
        private bool shakeCompleteNotified;
        private float shakeProgressVisual;
        private ParticleSystem shakeParticles;
        private float lastShakeIntensityPeak;

        // 事件
        public System.Action OnShakeCompleted;
        public System.Action<float> OnShakeProgress;
        public System.Action OnShakeStart;

        #endregion

        #region Unity 生命週期

        protected override void Awake()
        {
            base.Awake();
            interactableType = InteractableType.Shaker;
            maxVolume = Constants.ShakerMaxVolume;

            // 設置倒酒點
            if (pourPoint == null)
            {
                var child = transform.Find("PourPoint");
                if (child != null) pourPoint = child;
                else
                {
                    // Create a default pour point at the top
                    GameObject pp = new GameObject("PourPoint");
                    pp.transform.SetParent(transform);
                    pp.transform.localPosition = new Vector3(0, 0.3f, 0);
                    pourPoint = pp.transform;
                }
            }
        }

        protected override void Update()
        {
            // 呼叫基礎類別的 Update 以更新動態液體效果
            base.Update();

            if (isShaking)
            {
                UpdateShakeAnimation();
                // 搖酒時增加波動效果
                TriggerWobble(0.05f);
            }
            else if (isPouringAnimation)
            {
                UpdatePourAnimation();
            }
        }

        #endregion

        #region 搖酒

        /// <summary>
        /// 開始搖酒
        /// </summary>
        public void StartShaking(bool useTimer = true)
        {
            if (contents.IsEmpty) return;

            // 如果已經搖過，不能再搖
            if (contents.isShaken)
            {
                Debug.Log("Shaker: Already shaken! Add new ingredients to shake again.");
                return;
            }

            isShaking = true;
            shakeCompleteNotified = false;
            baseRotation = transform.localRotation;
            
            // 如果使用 QTE，則禁用計時器自動完成
            if (!useTimer)
            {
                // 設置一個非常大的時間，避免自動完成，或者在 Update 中處理
                // 這裡我們簡單地標記一下，或者依賴 Update 中的邏輯
                // 為了簡單起見，我們讓 Update 繼續運行動畫，但我們不觸發完成事件
                // 這需要修改 UpdateShakeAnimation
            }
            this.isQTEActive = !useTimer;

            // Create particle effect if not exists
            // CreateShakeParticles();

            OnShakeStart?.Invoke();
            Debug.Log($"Shaker: Started shaking with {contents.volume:F0}ml (Timer: {useTimer})");
        }

        /// <summary>
        /// 停止搖酒
        /// 參考: CocktailSystem.js stopShaking() Line 713-720
        /// </summary>
        public void StopShaking()
        {
            if (!isShaking) return;

            // Stop particles
            if (shakeParticles != null)
            {
                shakeParticles.Stop();
            }

            // 重置
            isShaking = false;
            shakeTime = 0f;
            shakeProgressVisual = 0f;
            transform.localRotation = baseRotation;
        }

        /// <summary>
        /// 更新搖晃動畫
        /// </summary>
        private void UpdateShakeAnimation()
        {
            shakeTime += Time.deltaTime;

            // Calculate progress
            float progress = Mathf.Clamp01(shakeTime / minShakeTime);
            shakeProgressVisual = progress;
            OnShakeProgress?.Invoke(progress);

            // Notify when shake is complete (Only if not in QTE mode)
            if (!isQTEActive && progress >= 1f && !shakeCompleteNotified)
            {
                shakeCompleteNotified = true;
                Debug.Log("Shaker: Shake ready! You can stop shaking now.");

                // 自動完成搖晃狀態
                contents.isShaken = true;
                contents.UpdateMixedColor();
                UpdateLiquidVisual();
                
                // 同步到ShakerContainer（如果存在）
                var shakerContainer = GetComponent<ShakerContainer>();
                if (shakerContainer != null)
                {
                    shakerContainer.isShaken = true;
                }

                OnShakeCompleted?.Invoke();

                // Visual cue - brief intensity increase
                TriggerWobble(0.15f);
            }

            // Dynamic shake intensity - increases as it gets closer to completion
            float dynamicIntensity = shakeIntensity * (0.8f + progress * 0.4f);

            // 正弦波搖晃
            float shakeZ = Mathf.Sin(shakeTime * shakeFrequency) * dynamicIntensity;
            float shakeX = Mathf.Sin(shakeTime * shakeFrequency * 0.75f) * dynamicIntensity * 0.6f;

            // Add some randomness for more natural shake
            float noise = Mathf.PerlinNoise(shakeTime * 5f, 0f) * 0.02f - 0.01f;
            shakeZ += noise;
            shakeX += noise * 0.5f;

            // 旋轉由 ImprovedInteractionSystem 控制，這裡只處理狀態和粒子
            // transform.localRotation = baseRotation * Quaternion.Euler(
            //     shakeX * Mathf.Rad2Deg,
            //     0f,
            //     shakeZ * Mathf.Rad2Deg
            // );

            // Update particles
            if (shakeParticles != null && !shakeParticles.isPlaying)
            {
                shakeParticles.Play();
            }
        }

        /// <summary>
        /// 創建搖酒粒子效果
        /// </summary>
        private void CreateShakeParticles()
        {
            if (shakeParticles != null) return;

            // Create simple particle effect
            GameObject particleObj = new GameObject("ShakeParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.up * 0.1f;

            shakeParticles = particleObj.AddComponent<ParticleSystem>();

            // Configure particle system
            var main = shakeParticles.main;
            main.startColor = new Color(1f, 1f, 1f, 0.3f);
            main.startSize = 0.02f;
            main.startSpeed = 0.5f;
            main.startLifetime = 0.3f;
            main.maxParticles = 20;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = shakeParticles.emission;
            emission.rateOverTime = 30f;

            var shape = shakeParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f;

            // Renderer settings
            var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = Color.white;
        }

        #endregion

        #region 倒酒

        /// <summary>
        /// 開始倒酒
        /// </summary>
        public void StartPouring()
        {
            if (contents.IsEmpty) return;
            isPouringAnimation = true;
        }

        /// <summary>
        /// 停止倒酒
        /// </summary>
        public void StopPouring()
        {
            isPouringAnimation = false;
        }

        /// <summary>
        /// 更新傾斜動畫並執行倒酒
        /// </summary>
        private void UpdatePourAnimation()
        {
            float targetTilt = pourTiltAngle;
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
            transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, currentTilt);

            // 執行倒酒邏輯
            PerformPouring();
        }

        /// <summary>
        /// 執行倒酒（檢測目標容器並倒入液體）
        /// </summary>
        private void PerformPouring()
        {
            if (pourPoint == null || contents.IsEmpty) return;

            // 使用 SphereCast 增加檢測範圍，避免 Raycast 太細射不中
            RaycastHit hit;
            float radius = 0.05f; // 5cm 半徑
            if (Physics.SphereCast(pourPoint.position, radius, Vector3.down, out hit, pourCheckDistance))
            {
                // 檢查是否是普通Container
                Container targetContainer = hit.collider.GetComponent<Container>();
                
                // 如果打到的是子物件，嘗試往上找 Container
                if (targetContainer == null)
                {
                    targetContainer = hit.collider.GetComponentInParent<Container>();
                }

                // 確保不是倒給自己
                if (targetContainer != null && targetContainer != this)
                {
                    // 使用 Container 的 TransferTo 方法，它會正確處理成分轉移
                    float amount = pourRate * Time.deltaTime;
                    float transferred = this.TransferTo(targetContainer, amount);
                    
                    if (transferred > 0)
                    {
                        targetContainer.SetPouringState(true);
                        // Debug.Log($"Pouring {transferred}ml to {targetContainer.name}");
                    }
                }
            }
            
            // Debug visualization
            Debug.DrawRay(pourPoint.position, Vector3.down * pourCheckDistance, Color.cyan);
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
            isPouringAnimation = false;
            shakeTime = 0f;
            currentTilt = 0f;
        }

        #endregion

        #region QTE回調

        /// <summary>
        /// QTE完成回調 (由外部 QTEManager 呼叫)
        /// </summary>
        public void OnShakeFinished(float quality)
        {
            isQTEActive = false;

            // 假設 quality > 0 即為成功，或者總是標記為 shaken 但品質不同
            // 這裡我們簡單標記為 shaken
            contents.isShaken = true;
            contents.UpdateMixedColor();
            UpdateLiquidVisual();

            // 同步到ShakerContainer（如果存在）
            var shakerContainer = GetComponent<ShakerContainer>();
            if (shakerContainer != null)
            {
                shakerContainer.isShaken = true;
            }

            OnShakeCompleted?.Invoke();
            Debug.Log($"Shaker: QTE Finished with quality {quality}. Contents shaken.");

            // 停止搖晃動畫
            StopShaking();
        }

        /// <summary>
        /// QTE完成回調 (Legacy)
        /// </summary>
        private void OnQTEComplete(bool success)
        {
            OnShakeFinished(success ? 1.0f : 0.0f);
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
        /// 是否正在倒酒動畫
        /// </summary>
        public bool IsPouringAnimation => isPouringAnimation;

        /// <summary>
        /// 搖酒時間
        /// </summary>
        public float ShakeTime => shakeTime;

        /// <summary>
        /// 是否完成搖酒
        /// </summary>
        public bool IsShakeComplete => shakeTime >= minShakeTime;

        /// <summary>
        /// 搖酒進度 (0-1)
        /// </summary>
        public float ShakeProgress => Mathf.Clamp01(shakeTime / minShakeTime);

        /// <summary>
        /// 最短搖酒時間
        /// </summary>
        public float MinShakeTime => minShakeTime;

        /// <summary>
        /// 剩餘搖酒時間
        /// </summary>
        public float RemainingShakeTime => Mathf.Max(0f, minShakeTime - shakeTime);

        /// <summary>
        /// 是否已搖晃
        /// </summary>
        public bool IsShaken => contents.isShaken;

        #endregion

        #region 清理

        #endregion
    }
}
