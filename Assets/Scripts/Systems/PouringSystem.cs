using UnityEngine;

namespace BarSimulator.Systems
{
    /// <summary>
    /// 倒酒視覺效果系統 - 處理倒酒粒子和視覺回饋
    /// 參考: CocktailSystem.js createPourParticles() Line 626-668
    /// </summary>
    public class PouringSystem : MonoBehaviour
    {
        #region 單例

        private static PouringSystem instance;
        public static PouringSystem Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("粒子設定")]
        [Tooltip("倒酒粒子系統預製物")]
        [SerializeField] private ParticleSystem pourParticlePrefab;

        [Tooltip("粒子顏色")]
        [SerializeField] private Color defaultParticleColor = new Color(0.67f, 0.8f, 1f, 0.8f);

        [Tooltip("粒子數量")]
        [SerializeField] private int particleCount = 200;

        [Tooltip("粒子大小")]
        [SerializeField] private float particleSize = 0.05f;

        [Header("流體設定")]
        [Tooltip("倒酒流體線寬")]
        [SerializeField] private float streamWidth = 0.02f;

        #endregion

        #region 私有欄位

        private ParticleSystem activeParticleSystem;
        private bool isPouring;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        #endregion

        #region 倒酒效果

        /// <summary>
        /// 開始倒酒效果
        /// </summary>
        public void StartPourEffect(Transform source, Transform target, Color liquidColor)
        {
            if (isPouring) return;

            isPouring = true;

            // 建立粒子系統
            if (pourParticlePrefab != null)
            {
                activeParticleSystem = Instantiate(pourParticlePrefab, source.position, Quaternion.identity);

                // 設定顏色
                var main = activeParticleSystem.main;
                main.startColor = liquidColor;

                activeParticleSystem.Play();
            }
        }

        /// <summary>
        /// 更新倒酒效果位置
        /// </summary>
        public void UpdatePourEffect(Transform source, Transform target)
        {
            if (!isPouring || activeParticleSystem == null) return;

            // 更新位置（從瓶口到杯子）
            Vector3 pourStart = source.position + source.up * -0.3f; // 瓶口位置
            activeParticleSystem.transform.position = pourStart;

            // 更新方向
            Vector3 direction = (target.position - pourStart).normalized;
            activeParticleSystem.transform.rotation = Quaternion.LookRotation(direction);
        }

        /// <summary>
        /// 停止倒酒效果
        /// </summary>
        public void StopPourEffect()
        {
            if (!isPouring) return;

            isPouring = false;

            if (activeParticleSystem != null)
            {
                activeParticleSystem.Stop();
                Destroy(activeParticleSystem.gameObject, 2f); // 延遲銷毀，讓粒子消失
                activeParticleSystem = null;
            }
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 是否正在顯示倒酒效果
        /// </summary>
        public bool IsPouring => isPouring;

        #endregion

        #region 靜態方法

        /// <summary>
        /// 建立預設粒子系統設定
        /// </summary>
        public static ParticleSystem CreateDefaultPourParticles()
        {
            var go = new GameObject("PourParticles");
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = 0.5f;
            main.startSpeed = 2f;
            main.startSize = 0.03f;
            main.maxParticles = 200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 100f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 5f;
            shape.radius = 0.01f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            // 設定材質
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

            return ps;
        }

        #endregion
    }
}
