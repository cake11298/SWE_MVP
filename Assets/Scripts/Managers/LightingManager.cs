using UnityEngine;

namespace BarSimulator.Managers
{
    /// <summary>
    /// 燈光管理器 - 控制場景燈光效果
    /// 參考: Three.js 場景燈光設置
    /// </summary>
    public class LightingManager : MonoBehaviour
    {
        #region 單例

        private static LightingManager instance;
        public static LightingManager Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("主要燈光")]
        [Tooltip("環境光")]
        [SerializeField] private Light ambientLight;

        [Tooltip("主方向光")]
        [SerializeField] private Light mainDirectionalLight;

        [Header("吧台燈光")]
        [Tooltip("吧台聚光燈")]
        [SerializeField] private Light[] barSpotlights;

        [Tooltip("霓虹燈")]
        [SerializeField] private Light[] neonLights;

        [Header("燈光設定")]
        [Tooltip("環境光強度")]
        [SerializeField] private float ambientIntensity = 0.15f;

        [Tooltip("主燈強度")]
        [SerializeField] private float mainLightIntensity = 0.5f;

        [Tooltip("聚光燈強度")]
        [SerializeField] private float spotlightIntensity = 1.5f;

        [Tooltip("霓虹燈強度")]
        [SerializeField] private float neonIntensity = 1f;

        [Header("顏色設定")]
        [Tooltip("環境光顏色")]
        [SerializeField] private Color ambientColor = new Color(0.15f, 0.12f, 0.1f);

        [Tooltip("主燈顏色")]
        [SerializeField] private Color mainLightColor = new Color(1f, 0.9f, 0.8f);

        [Tooltip("暖色聚光燈")]
        [SerializeField] private Color warmSpotColor = new Color(1f, 0.8f, 0.6f);

        [Tooltip("冷色聚光燈")]
        [SerializeField] private Color coolSpotColor = new Color(0.6f, 0.8f, 1f);

        [Header("動態效果")]
        [Tooltip("啟用霓虹燈閃爍")]
        [SerializeField] private bool enableNeonFlicker = true;

        [Tooltip("閃爍速度")]
        [SerializeField] private float flickerSpeed = 5f;

        [Tooltip("閃爍幅度")]
        [SerializeField] private float flickerAmount = 0.2f;

        #endregion

        #region 私有欄位

        // 燈光狀態
        private bool lightsOn = true;
        private float[] neonBaseIntensities;
        private float flickerTime;

        // 過渡狀態
        private bool isTransitioning;
        private float transitionProgress;
        private float transitionDuration = 1f;

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

            // 儲存霓虹燈基礎強度
            SaveNeonBaseIntensities();
        }

        private void Start()
        {
            // 初始化燈光
            InitializeLighting();
        }

        private void Update()
        {
            if (enableNeonFlicker && lightsOn)
            {
                UpdateNeonFlicker();
            }

            if (isTransitioning)
            {
                UpdateTransition();
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 儲存霓虹燈基礎強度
        /// </summary>
        private void SaveNeonBaseIntensities()
        {
            if (neonLights == null) return;

            neonBaseIntensities = new float[neonLights.Length];
            for (int i = 0; i < neonLights.Length; i++)
            {
                if (neonLights[i] != null)
                {
                    neonBaseIntensities[i] = neonLights[i].intensity;
                }
            }
        }

        /// <summary>
        /// 初始化燈光設置
        /// </summary>
        private void InitializeLighting()
        {
            // 設置環境光
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.reflectionIntensity = 0.3f;
            RenderSettings.fog = false; // 強制禁用霧效

            // 如果沒有指定主燈，自動找場景中的 Directional Light
            if (mainDirectionalLight == null)
            {
                var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
                foreach (var light in lights)
                {
                    if (light.type == LightType.Directional)
                    {
                        mainDirectionalLight = light;
                        break;
                    }
                }
            }

            // 設置主燈
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.color = mainLightColor;
                mainDirectionalLight.intensity = mainLightIntensity;
                mainDirectionalLight.shadowStrength = 0.5f;
            }

            // 設置聚光燈
            SetupSpotlights();

            // 設置霓虹燈
            SetupNeonLights();

            Debug.Log($"LightingManager: Lighting initialized - Ambient: {ambientIntensity}, Main: {mainLightIntensity}");
        }

        /// <summary>
        /// 設置聚光燈
        /// </summary>
        private void SetupSpotlights()
        {
            if (barSpotlights == null) return;

            for (int i = 0; i < barSpotlights.Length; i++)
            {
                if (barSpotlights[i] == null) continue;

                barSpotlights[i].intensity = spotlightIntensity;
                // 交替暖冷色
                barSpotlights[i].color = (i % 2 == 0) ? warmSpotColor : coolSpotColor;
            }
        }

        /// <summary>
        /// 設置霓虹燈
        /// </summary>
        private void SetupNeonLights()
        {
            if (neonLights == null) return;

            foreach (var light in neonLights)
            {
                if (light != null)
                {
                    light.intensity = neonIntensity;
                }
            }
        }

        #endregion

        #region 動態效果

        /// <summary>
        /// 更新霓虹燈閃爍效果
        /// </summary>
        private void UpdateNeonFlicker()
        {
            if (neonLights == null || neonBaseIntensities == null) return;

            flickerTime += Time.deltaTime * flickerSpeed;

            for (int i = 0; i < neonLights.Length; i++)
            {
                if (neonLights[i] == null) continue;

                // 使用不同的相位偏移讓每個燈獨立閃爍
                float phase = flickerTime + i * 1.5f;
                float flicker = 1f + Mathf.Sin(phase) * flickerAmount * 0.5f +
                               Mathf.Sin(phase * 2.3f) * flickerAmount * 0.3f;

                neonLights[i].intensity = neonBaseIntensities[i] * flicker;
            }
        }

        #endregion

        #region 燈光控制

        /// <summary>
        /// 切換所有燈光
        /// </summary>
        public void ToggleLights()
        {
            SetLightsOn(!lightsOn);
        }

        /// <summary>
        /// 設置燈光開關
        /// </summary>
        public void SetLightsOn(bool on)
        {
            if (lightsOn == on) return;

            lightsOn = on;
            isTransitioning = true;
            transitionProgress = 0f;

            Debug.Log($"LightingManager: Lights {(on ? "ON" : "OFF")}");
        }

        /// <summary>
        /// 更新燈光過渡
        /// </summary>
        private void UpdateTransition()
        {
            transitionProgress += Time.deltaTime / transitionDuration;

            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;
            }

            float t = lightsOn ? transitionProgress : (1f - transitionProgress);
            t = Mathf.SmoothStep(0f, 1f, t);

            // 過渡主燈
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = Mathf.Lerp(0f, mainLightIntensity, t);
            }

            // 過渡聚光燈
            if (barSpotlights != null)
            {
                foreach (var light in barSpotlights)
                {
                    if (light != null)
                    {
                        light.intensity = Mathf.Lerp(0f, spotlightIntensity, t);
                    }
                }
            }

            // 過渡霓虹燈
            if (neonLights != null && neonBaseIntensities != null)
            {
                for (int i = 0; i < neonLights.Length; i++)
                {
                    if (neonLights[i] != null)
                    {
                        neonLights[i].intensity = Mathf.Lerp(0f, neonBaseIntensities[i], t);
                    }
                }
            }

            // 過渡環境光
            RenderSettings.ambientIntensity = Mathf.Lerp(0.05f, ambientIntensity, t);
        }

        /// <summary>
        /// 設置氣氛模式
        /// </summary>
        public void SetMood(LightingMood mood)
        {
            switch (mood)
            {
                case LightingMood.Normal:
                    SetNormalMood();
                    break;
                case LightingMood.Romantic:
                    SetRomanticMood();
                    break;
                case LightingMood.Party:
                    SetPartyMood();
                    break;
                case LightingMood.Chill:
                    SetChillMood();
                    break;
            }
        }

        private void SetNormalMood()
        {
            ambientIntensity = 0.3f;
            mainLightIntensity = 1f;
            spotlightIntensity = 2f;
            flickerAmount = 0.2f;
            InitializeLighting();
        }

        private void SetRomanticMood()
        {
            ambientIntensity = 0.15f;
            mainLightIntensity = 0.3f;
            spotlightIntensity = 1.5f;
            warmSpotColor = new Color(1f, 0.6f, 0.4f);
            flickerAmount = 0.3f;
            InitializeLighting();
        }

        private void SetPartyMood()
        {
            ambientIntensity = 0.2f;
            mainLightIntensity = 0.5f;
            spotlightIntensity = 3f;
            flickerAmount = 0.5f;
            flickerSpeed = 8f;
            InitializeLighting();
        }

        private void SetChillMood()
        {
            ambientIntensity = 0.25f;
            mainLightIntensity = 0.6f;
            spotlightIntensity = 1.2f;
            coolSpotColor = new Color(0.5f, 0.7f, 1f);
            flickerAmount = 0.1f;
            InitializeLighting();
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 燈光是否開啟
        /// </summary>
        public bool LightsOn => lightsOn;

        /// <summary>
        /// 是否正在過渡
        /// </summary>
        public bool IsTransitioning => isTransitioning;

        #endregion
    }

    /// <summary>
    /// 燈光氣氛模式
    /// </summary>
    public enum LightingMood
    {
        Normal,
        Romantic,
        Party,
        Chill
    }
}
