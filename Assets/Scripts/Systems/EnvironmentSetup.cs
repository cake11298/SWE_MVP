using UnityEngine;
using UnityEngine.Rendering;

namespace BarSimulator.Systems
{
    /// <summary>
    /// Environment setup for lighting, reflection probes, and ambient settings
    /// </summary>
    public class EnvironmentSetup : MonoBehaviour
    {
        #region Singleton

        private static EnvironmentSetup instance;
        public static EnvironmentSetup Instance => instance;

        #endregion

        #region Serialized Fields

        [Header("Setup Options")]
        [SerializeField] private bool setupOnStart = true;

        [Header("Main Light")]
        [SerializeField] private Color mainLightColor = new Color(1f, 0.95f, 0.9f);
        [SerializeField] private float mainLightIntensity = 1.2f;
        [SerializeField] private Vector3 mainLightRotation = new Vector3(50f, -30f, 0f);
        [SerializeField] private bool enableShadows = true;
        [SerializeField] private float shadowStrength = 0.7f;

        [Header("Fill Light")]
        [SerializeField] private bool createFillLight = true;
        [SerializeField] private Color fillLightColor = new Color(0.7f, 0.8f, 1f);
        [SerializeField] private float fillLightIntensity = 0.4f;

        [Header("Ambient")]
        [SerializeField] private AmbientMode ambientMode = AmbientMode.Trilight;
        [SerializeField] private Color skyColor = new Color(0.4f, 0.35f, 0.5f);
        [SerializeField] private Color equatorColor = new Color(0.3f, 0.25f, 0.35f);
        [SerializeField] private Color groundColor = new Color(0.15f, 0.1f, 0.1f);
        [SerializeField] private float ambientIntensity = 1f;

        [Header("Reflection Probes")]
        [SerializeField] private bool createReflectionProbes = true;
        [SerializeField] private int reflectionResolution = 256;

        [Header("Fog")]
        [SerializeField] private bool enableFog = true;
        [SerializeField] private Color fogColor = new Color(0.2f, 0.15f, 0.1f);
        [SerializeField] private float fogDensity = 0.02f;

        [Header("Bar Specific Lights")]
        [SerializeField] private bool createBarLights = true;
        [SerializeField] private Color barLightColor = new Color(1f, 0.8f, 0.6f);
        [SerializeField] private float barLightIntensity = 0.8f;

        #endregion

        #region Private Fields

        private Light mainLight;
        private Light fillLight;
        private ReflectionProbe mainProbe;
        private GameObject barLightsContainer;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void Start()
        {
            if (setupOnStart)
            {
                SetupEnvironment();
            }
        }

        #endregion

        #region Setup Methods

        /// <summary>
        /// Setup complete environment
        /// </summary>
        public void SetupEnvironment()
        {
            SetupMainLight();
            if (createFillLight) SetupFillLight();
            SetupAmbient();
            if (createReflectionProbes) SetupReflectionProbes();
            if (enableFog) SetupFog();
            if (createBarLights) SetupBarLights();

            Debug.Log("EnvironmentSetup: Environment configured successfully");
        }

        /// <summary>
        /// Setup main directional light
        /// </summary>
        private void SetupMainLight()
        {
            // Find or create main light
            mainLight = FindObjectOfType<Light>();
            if (mainLight == null || mainLight.type != LightType.Directional)
            {
                GameObject lightObj = new GameObject("Main Directional Light");
                mainLight = lightObj.AddComponent<Light>();
                mainLight.type = LightType.Directional;
            }

            // Configure
            mainLight.color = mainLightColor;
            mainLight.intensity = mainLightIntensity;
            mainLight.transform.rotation = Quaternion.Euler(mainLightRotation);

            // Shadows
            if (enableShadows)
            {
                mainLight.shadows = LightShadows.Soft;
                mainLight.shadowStrength = shadowStrength;
                mainLight.shadowBias = 0.05f;
                mainLight.shadowNormalBias = 0.4f;
            }
            else
            {
                mainLight.shadows = LightShadows.None;
            }
        }

        /// <summary>
        /// Setup fill light
        /// </summary>
        private void SetupFillLight()
        {
            GameObject fillObj = new GameObject("Fill Light");
            fillObj.transform.SetParent(transform);
            fillLight = fillObj.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = fillLightColor;
            fillLight.intensity = fillLightIntensity;
            fillLight.shadows = LightShadows.None;

            // Opposite direction to main light
            fillLight.transform.rotation = Quaternion.Euler(mainLightRotation.x, mainLightRotation.y + 180f, 0f);
        }

        /// <summary>
        /// Setup ambient lighting
        /// </summary>
        private void SetupAmbient()
        {
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientIntensity = ambientIntensity;

            switch (ambientMode)
            {
                case AmbientMode.Trilight:
                    RenderSettings.ambientSkyColor = skyColor;
                    RenderSettings.ambientEquatorColor = equatorColor;
                    RenderSettings.ambientGroundColor = groundColor;
                    break;

                case AmbientMode.Flat:
                    RenderSettings.ambientLight = equatorColor;
                    break;

                case AmbientMode.Skybox:
                    // Use skybox for ambient
                    break;
            }
        }

        /// <summary>
        /// Setup reflection probes
        /// </summary>
        private void SetupReflectionProbes()
        {
            // Main reflection probe
            GameObject probeObj = new GameObject("Main Reflection Probe");
            probeObj.transform.SetParent(transform);
            probeObj.transform.position = new Vector3(0f, 1.5f, 0f);

            mainProbe = probeObj.AddComponent<ReflectionProbe>();
            mainProbe.mode = ReflectionProbeMode.Realtime;
            mainProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            mainProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
            mainProbe.resolution = reflectionResolution;
            mainProbe.size = new Vector3(20f, 10f, 20f);
            mainProbe.boxProjection = true;
            mainProbe.importance = 1;

            // Initial render
            mainProbe.RenderProbe();

            // Set as default reflection
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = mainProbe.texture;
        }

        /// <summary>
        /// Setup fog
        /// </summary>
        private void SetupFog()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }

        /// <summary>
        /// Setup bar-specific lighting
        /// </summary>
        private void SetupBarLights()
        {
            barLightsContainer = new GameObject("Bar Lights");
            barLightsContainer.transform.SetParent(transform);

            // Overhead bar light
            CreatePointLight("Bar Overhead Light", new Vector3(0f, 2.5f, -2f), barLightColor, barLightIntensity, 5f);

            // Back shelf lights
            CreateSpotLight("Shelf Light Left", new Vector3(-2f, 2f, -3f), barLightColor, barLightIntensity * 0.5f, 8f, 45f);
            CreateSpotLight("Shelf Light Right", new Vector3(2f, 2f, -3f), barLightColor, barLightIntensity * 0.5f, 8f, 45f);

            // Under-counter accent lights
            Color accentColor = new Color(0.6f, 0.8f, 1f);
            CreatePointLight("Counter Accent Left", new Vector3(-1.5f, 0.8f, -1.5f), accentColor, 0.3f, 2f);
            CreatePointLight("Counter Accent Right", new Vector3(1.5f, 0.8f, -1.5f), accentColor, 0.3f, 2f);
        }

        #endregion

        #region Light Creation Helpers

        private Light CreatePointLight(string name, Vector3 position, Color color, float intensity, float range)
        {
            GameObject lightObj = new GameObject(name);
            lightObj.transform.SetParent(barLightsContainer.transform);
            lightObj.transform.position = position;

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.5f;

            return light;
        }

        private Light CreateSpotLight(string name, Vector3 position, Color color, float intensity, float range, float angle)
        {
            GameObject lightObj = new GameObject(name);
            lightObj.transform.SetParent(barLightsContainer.transform);
            lightObj.transform.position = position;
            lightObj.transform.LookAt(position + Vector3.down);

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = angle;
            light.innerSpotAngle = angle * 0.7f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.5f;

            return light;
        }

        #endregion

        #region Runtime Control

        /// <summary>
        /// Set main light intensity
        /// </summary>
        public void SetMainLightIntensity(float intensity)
        {
            if (mainLight != null)
            {
                mainLight.intensity = Mathf.Max(0, intensity);
            }
        }

        /// <summary>
        /// Set main light color
        /// </summary>
        public void SetMainLightColor(Color color)
        {
            if (mainLight != null)
            {
                mainLight.color = color;
            }
        }

        /// <summary>
        /// Toggle fog
        /// </summary>
        public void SetFogEnabled(bool enabled)
        {
            RenderSettings.fog = enabled;
        }

        /// <summary>
        /// Set fog density
        /// </summary>
        public void SetFogDensity(float density)
        {
            RenderSettings.fogDensity = Mathf.Clamp(density, 0f, 0.1f);
        }

        /// <summary>
        /// Update reflection probe
        /// </summary>
        public void UpdateReflectionProbe()
        {
            if (mainProbe != null)
            {
                mainProbe.RenderProbe();
            }
        }

        /// <summary>
        /// Set bar lights intensity
        /// </summary>
        public void SetBarLightsIntensity(float multiplier)
        {
            if (barLightsContainer == null) return;

            var lights = barLightsContainer.GetComponentsInChildren<Light>();
            foreach (var light in lights)
            {
                light.intensity = barLightIntensity * multiplier;
            }
        }

        #endregion

        #region Time of Day Presets

        /// <summary>
        /// Apply morning bar lighting
        /// </summary>
        public void ApplyMorningLighting()
        {
            SetMainLightIntensity(1.5f);
            SetMainLightColor(new Color(1f, 0.98f, 0.95f));
            SetFogDensity(0.005f);
            SetBarLightsIntensity(0.3f);

            RenderSettings.ambientSkyColor = new Color(0.6f, 0.65f, 0.7f);
            RenderSettings.ambientEquatorColor = new Color(0.5f, 0.55f, 0.6f);
        }

        /// <summary>
        /// Apply evening bar lighting
        /// </summary>
        public void ApplyEveningLighting()
        {
            SetMainLightIntensity(0.8f);
            SetMainLightColor(new Color(1f, 0.85f, 0.7f));
            SetFogDensity(0.025f);
            SetBarLightsIntensity(1.2f);

            RenderSettings.ambientSkyColor = new Color(0.3f, 0.25f, 0.4f);
            RenderSettings.ambientEquatorColor = new Color(0.25f, 0.2f, 0.3f);
        }

        /// <summary>
        /// Apply night bar lighting
        /// </summary>
        public void ApplyNightLighting()
        {
            SetMainLightIntensity(0.3f);
            SetMainLightColor(new Color(0.5f, 0.6f, 0.8f));
            SetFogDensity(0.035f);
            SetBarLightsIntensity(1.5f);

            RenderSettings.ambientSkyColor = new Color(0.1f, 0.1f, 0.2f);
            RenderSettings.ambientEquatorColor = new Color(0.1f, 0.08f, 0.15f);
        }

        #endregion
    }
}
