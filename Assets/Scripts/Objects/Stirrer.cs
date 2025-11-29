using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Interaction;

namespace BarSimulator.Objects
{
    /// <summary>
    /// 攪拌器組件 - 用於攪拌法調製雞尾酒（如Martini）
    /// 參考: CocktailSystem.js stir() Line 722-743
    /// </summary>
    public class Stirrer : MonoBehaviour, IInteractable
    {
        #region 序列化欄位

        [Header("Stirring Settings")]
        [Tooltip("攪拌強度")]
        [SerializeField] private float stirIntensity = 0.03f;

        [Tooltip("攪拌頻率")]
        [SerializeField] private float stirFrequency = 15f;

        [Tooltip("需要攪拌的最短時間（秒）才能完成混合")]
        [SerializeField] private float minStirTime = 3f;

        [Header("Visual")]
        [Tooltip("攪拌棒模型")]
        [SerializeField] private GameObject stirrerModel;

        [Tooltip("旋轉速度")]
        [SerializeField] private float rotationSpeed = 360f;

        #endregion

        #region 私有欄位

        private bool isStirring;
        private float stirTime;
        private Container targetContainer; // 正在攪拌的容器
        private bool stirCompleteNotified;

        // Visual feedback
        private float currentRotation;
        private ParticleSystem stirParticles;

        // Events
        public System.Action OnStirCompleted;
        public System.Action<float> OnStirProgress;
        public System.Action OnStirStart;

        // IInteractable
        private bool isPickedUp;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Rigidbody rb;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            // Store original position
            originalPosition = transform.position;
            originalRotation = transform.rotation;
        }

        private void Update()
        {
            if (isStirring)
            {
                UpdateStirAnimation();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // When stirrer enters a container with liquid
            if (!isPickedUp) return;

            var container = other.GetComponent<Container>();
            if (container != null && !container.IsEmpty)
            {
                // Auto-start stirring when inserted into container
                targetContainer = container;
                Debug.Log($"Stirrer: Inserted into {container.name}");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // When stirrer exits container
            var container = other.GetComponent<Container>();
            if (container == targetContainer)
            {
                StopStirring();
                targetContainer = null;
            }
        }

        #endregion

        #region 攪拌

        /// <summary>
        /// 開始攪拌
        /// </summary>
        public void StartStirring()
        {
            if (targetContainer == null || targetContainer.IsEmpty)
            {
                Debug.LogWarning("Stirrer: No container with liquid to stir");
                return;
            }

            isStirring = true;
            stirCompleteNotified = false;

            // Create particle effect
            CreateStirParticles();

            OnStirStart?.Invoke();
            Debug.Log($"Stirrer: Started stirring in {targetContainer.name} with {targetContainer.Volume:F0}ml");
        }

        /// <summary>
        /// 停止攪拌
        /// </summary>
        public void StopStirring()
        {
            if (!isStirring) return;

            // Check if stirred long enough
            if (stirTime >= minStirTime && targetContainer != null)
            {
                // Enhance mixing
                targetContainer.Contents.UpdateMixedColor();
                targetContainer.UpdateLiquidVisual();
                OnStirCompleted?.Invoke();
                Debug.Log($"Stirrer: Stir completed! Contents well mixed.");
            }
            else if (stirTime > 0)
            {
                Debug.Log($"Stirrer: Stir incomplete ({stirTime:F1}s / {minStirTime:F1}s required)");
            }

            // Stop particles
            if (stirParticles != null)
            {
                stirParticles.Stop();
            }

            // Reset
            isStirring = false;
            stirTime = 0f;
        }

        /// <summary>
        /// 更新攪拌動畫
        /// </summary>
        private void UpdateStirAnimation()
        {
            stirTime += Time.deltaTime;

            // Calculate progress
            float progress = Mathf.Clamp01(stirTime / minStirTime);
            OnStirProgress?.Invoke(progress);

            // Notify when stir is complete
            if (progress >= 1f && !stirCompleteNotified)
            {
                stirCompleteNotified = true;
                Debug.Log("Stirrer: Stir ready! You can stop stirring now.");

                // Visual cue
                if (targetContainer != null)
                {
                    targetContainer.TriggerWobble(0.1f);
                }
            }

            // Rotate stirrer model
            if (stirrerModel != null)
            {
                currentRotation += rotationSpeed * Time.deltaTime;
                stirrerModel.transform.localRotation = Quaternion.Euler(0f, currentRotation, 0f);
            }

            // Add slight wobble to container
            if (targetContainer != null && Time.frameCount % 10 == 0)
            {
                targetContainer.TriggerWobble(0.02f);
            }

            // Update particles
            if (stirParticles != null && !stirParticles.isPlaying)
            {
                stirParticles.Play();
            }
        }

        /// <summary>
        /// 創建攪拌粒子效果
        /// </summary>
        private void CreateStirParticles()
        {
            if (stirParticles != null) return;

            // Create simple particle effect
            GameObject particleObj = new GameObject("StirParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.down * 0.05f;

            stirParticles = particleObj.AddComponent<ParticleSystem>();

            // Configure particle system
            var main = stirParticles.main;
            main.startColor = new Color(1f, 1f, 1f, 0.2f);
            main.startSize = 0.01f;
            main.startSpeed = 0.3f;
            main.startLifetime = 0.5f;
            main.maxParticles = 15;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = stirParticles.emission;
            emission.rateOverTime = 20f;

            var shape = stirParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.03f;

            // Renderer settings
            var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = Color.white;
        }

        #endregion

        #region IInteractable 實作

        public InteractableType GetInteractableType()
        {
            return InteractableType.Tool;
        }

        public void OnInteract()
        {
            // Toggle stirring if inside container
            if (targetContainer != null)
            {
                if (isStirring)
                {
                    StopStirring();
                }
                else
                {
                    StartStirring();
                }
            }
        }

        public void OnPickup()
        {
            isPickedUp = true;

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Debug.Log("Stirrer: Picked up");
        }

        public void OnDrop(bool returnToOriginal)
        {
            isPickedUp = false;
            StopStirring();
            targetContainer = null;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            if (returnToOriginal)
            {
                transform.position = originalPosition;
                transform.rotation = originalRotation;
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }

            Debug.Log("Stirrer: Dropped");
        }

        public bool CanInteract()
        {
            return true;
        }

        public string GetInteractPrompt()
        {
            if (isPickedUp)
            {
                if (targetContainer != null)
                {
                    return isStirring ? "E: Stop Stirring" : "E: Start Stirring";
                }
                return "Insert into glass to stir";
            }
            return "E: Pick Up Stirrer";
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 是否正在攪拌
        /// </summary>
        public bool IsStirring => isStirring;

        /// <summary>
        /// 攪拌時間
        /// </summary>
        public float StirTime => stirTime;

        /// <summary>
        /// 是否完成攪拌
        /// </summary>
        public bool IsStirComplete => stirTime >= minStirTime;

        /// <summary>
        /// 攪拌進度 (0-1)
        /// </summary>
        public float StirProgress => Mathf.Clamp01(stirTime / minStirTime);

        /// <summary>
        /// 最短攪拌時間
        /// </summary>
        public float MinStirTime => minStirTime;

        /// <summary>
        /// 剩餘攪拌時間
        /// </summary>
        public float RemainingStirTime => Mathf.Max(0f, minStirTime - stirTime);

        /// <summary>
        /// 目標容器
        /// </summary>
        public Container TargetContainer => targetContainer;

        #endregion
    }
}
