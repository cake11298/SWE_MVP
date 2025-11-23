using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BarSimulator.Systems
{
    /// <summary>
    /// Post-processing setup manager
    /// Adds bloom, color grading, depth of field, and vignette effects
    /// </summary>
    public class PostProcessingSetup : MonoBehaviour
    {
        #region Singleton

        private static PostProcessingSetup instance;
        public static PostProcessingSetup Instance => instance;

        #endregion

        #region Serialized Fields

        [Header("Volume Settings")]
        [Tooltip("Create global volume on start")]
        [SerializeField] private bool createVolumeOnStart = true;

        [Header("Bloom")]
        [SerializeField] private bool enableBloom = true;
        [SerializeField] private float bloomIntensity = 0.5f;
        [SerializeField] private float bloomThreshold = 0.9f;
        [SerializeField] private float bloomScatter = 0.7f;
        [SerializeField] private Color bloomTint = Color.white;

        [Header("Color Adjustments")]
        [SerializeField] private bool enableColorAdjustments = true;
        [SerializeField] private float postExposure = 0.2f;
        [SerializeField] private float contrast = 10f;
        [SerializeField] private float saturation = 10f;

        [Header("Depth of Field")]
        [SerializeField] private bool enableDepthOfField = false;
        [SerializeField] private float focusDistance = 3f;
        [SerializeField] private float aperture = 5.6f;
        [SerializeField] private float focalLength = 50f;

        [Header("Vignette")]
        [SerializeField] private bool enableVignette = true;
        [SerializeField] private float vignetteIntensity = 0.3f;
        [SerializeField] private float vignetteSmoothness = 0.5f;

        [Header("Ambient Occlusion")]
        [SerializeField] private bool enableAmbientOcclusion = true;
        [SerializeField] private float aoIntensity = 0.5f;

        [Header("Film Grain")]
        [SerializeField] private bool enableFilmGrain = false;
        [SerializeField] private float grainIntensity = 0.2f;

        [Header("Chromatic Aberration")]
        [SerializeField] private bool enableChromaticAberration = false;
        [SerializeField] private float chromaticIntensity = 0.1f;

        #endregion

        #region Private Fields

        private Volume globalVolume;
        private VolumeProfile volumeProfile;

        // Effect references
        private Bloom bloom;
        private ColorAdjustments colorAdjustments;
        private DepthOfField depthOfField;
        private Vignette vignette;
        private FilmGrain filmGrain;
        private ChromaticAberration chromaticAberration;

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
            if (createVolumeOnStart)
            {
                SetupPostProcessing();
            }
        }

        #endregion

        #region Setup Methods

        /// <summary>
        /// Set up post-processing volume and effects
        /// </summary>
        public void SetupPostProcessing()
        {
            // Create or get volume
            globalVolume = GetComponent<Volume>();
            if (globalVolume == null)
            {
                globalVolume = gameObject.AddComponent<Volume>();
            }

            globalVolume.isGlobal = true;
            globalVolume.priority = 100;

            // Create profile
            volumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            globalVolume.profile = volumeProfile;

            // Add effects
            SetupBloom();
            SetupColorAdjustments();
            SetupDepthOfField();
            SetupVignette();
            SetupFilmGrain();
            SetupChromaticAberration();

            Debug.Log("PostProcessingSetup: Post-processing effects configured");
        }

        private void SetupBloom()
        {
            if (!enableBloom) return;

            bloom = volumeProfile.Add<Bloom>(true);
            bloom.active = true;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = bloomIntensity;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = bloomThreshold;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = bloomScatter;
            bloom.tint.overrideState = true;
            bloom.tint.value = bloomTint;
        }

        private void SetupColorAdjustments()
        {
            if (!enableColorAdjustments) return;

            colorAdjustments = volumeProfile.Add<ColorAdjustments>(true);
            colorAdjustments.active = true;
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value = postExposure;
            colorAdjustments.contrast.overrideState = true;
            colorAdjustments.contrast.value = contrast;
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = saturation;
        }

        private void SetupDepthOfField()
        {
            if (!enableDepthOfField) return;

            depthOfField = volumeProfile.Add<DepthOfField>(true);
            depthOfField.active = true;
            depthOfField.mode.overrideState = true;
            depthOfField.mode.value = DepthOfFieldMode.Bokeh;
            depthOfField.focusDistance.overrideState = true;
            depthOfField.focusDistance.value = focusDistance;
            depthOfField.aperture.overrideState = true;
            depthOfField.aperture.value = aperture;
            depthOfField.focalLength.overrideState = true;
            depthOfField.focalLength.value = focalLength;
        }

        private void SetupVignette()
        {
            if (!enableVignette) return;

            vignette = volumeProfile.Add<Vignette>(true);
            vignette.active = true;
            vignette.intensity.overrideState = true;
            vignette.intensity.value = vignetteIntensity;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = vignetteSmoothness;
        }

        private void SetupFilmGrain()
        {
            if (!enableFilmGrain) return;

            filmGrain = volumeProfile.Add<FilmGrain>(true);
            filmGrain.active = true;
            filmGrain.intensity.overrideState = true;
            filmGrain.intensity.value = grainIntensity;
        }

        private void SetupChromaticAberration()
        {
            if (!enableChromaticAberration) return;

            chromaticAberration = volumeProfile.Add<ChromaticAberration>(true);
            chromaticAberration.active = true;
            chromaticAberration.intensity.overrideState = true;
            chromaticAberration.intensity.value = chromaticIntensity;
        }

        #endregion

        #region Runtime Control

        /// <summary>
        /// Set bloom intensity at runtime
        /// </summary>
        public void SetBloomIntensity(float intensity)
        {
            if (bloom != null)
            {
                bloom.intensity.value = Mathf.Max(0, intensity);
            }
        }

        /// <summary>
        /// Set depth of field focus distance
        /// </summary>
        public void SetFocusDistance(float distance)
        {
            if (depthOfField != null)
            {
                depthOfField.focusDistance.value = Mathf.Max(0.1f, distance);
            }
        }

        /// <summary>
        /// Enable/disable depth of field
        /// </summary>
        public void SetDepthOfFieldEnabled(bool enabled)
        {
            if (depthOfField != null)
            {
                depthOfField.active = enabled;
            }
        }

        /// <summary>
        /// Set vignette intensity
        /// </summary>
        public void SetVignetteIntensity(float intensity)
        {
            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Clamp01(intensity);
            }
        }

        /// <summary>
        /// Set post exposure for brightness
        /// </summary>
        public void SetExposure(float exposure)
        {
            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = exposure;
            }
        }

        /// <summary>
        /// Set saturation
        /// </summary>
        public void SetSaturation(float saturation)
        {
            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.value = Mathf.Clamp(saturation, -100f, 100f);
            }
        }

        /// <summary>
        /// Apply drunk effect (blur + vignette + chromatic)
        /// </summary>
        public void ApplyDrunkEffect(float intensity)
        {
            intensity = Mathf.Clamp01(intensity);

            // Increase vignette
            if (vignette != null)
            {
                vignette.intensity.value = vignetteIntensity + intensity * 0.4f;
            }

            // Add blur via depth of field
            if (depthOfField != null)
            {
                depthOfField.active = intensity > 0.1f;
                depthOfField.aperture.value = Mathf.Lerp(5.6f, 1.4f, intensity);
            }

            // Add chromatic aberration
            if (chromaticAberration != null || intensity > 0)
            {
                if (chromaticAberration == null && volumeProfile != null)
                {
                    chromaticAberration = volumeProfile.Add<ChromaticAberration>(true);
                    chromaticAberration.intensity.overrideState = true;
                }
                if (chromaticAberration != null)
                {
                    chromaticAberration.active = intensity > 0;
                    chromaticAberration.intensity.value = intensity * 0.3f;
                }
            }
        }

        /// <summary>
        /// Reset all effects to default
        /// </summary>
        public void ResetEffects()
        {
            if (bloom != null) bloom.intensity.value = bloomIntensity;
            if (vignette != null) vignette.intensity.value = vignetteIntensity;
            if (depthOfField != null)
            {
                depthOfField.active = enableDepthOfField;
                depthOfField.aperture.value = aperture;
            }
            if (chromaticAberration != null)
            {
                chromaticAberration.active = enableChromaticAberration;
                chromaticAberration.intensity.value = chromaticIntensity;
            }
            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = postExposure;
                colorAdjustments.saturation.value = saturation;
            }
        }

        #endregion

        #region Bar Atmosphere Presets

        /// <summary>
        /// Apply evening bar atmosphere
        /// </summary>
        public void ApplyEveningAtmosphere()
        {
            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = -0.1f;
                colorAdjustments.saturation.value = -5f;
                colorAdjustments.contrast.value = 15f;
            }

            if (vignette != null)
            {
                vignette.intensity.value = 0.4f;
            }

            if (bloom != null)
            {
                bloom.intensity.value = 0.8f;
                bloom.threshold.value = 0.8f;
            }
        }

        /// <summary>
        /// Apply daytime bar atmosphere
        /// </summary>
        public void ApplyDaytimeAtmosphere()
        {
            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = 0.3f;
                colorAdjustments.saturation.value = 10f;
                colorAdjustments.contrast.value = 5f;
            }

            if (vignette != null)
            {
                vignette.intensity.value = 0.2f;
            }

            if (bloom != null)
            {
                bloom.intensity.value = 0.3f;
                bloom.threshold.value = 1.0f;
            }
        }

        /// <summary>
        /// Apply party/club atmosphere
        /// </summary>
        public void ApplyPartyAtmosphere()
        {
            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = 0f;
                colorAdjustments.saturation.value = 30f;
                colorAdjustments.contrast.value = 20f;
            }

            if (bloom != null)
            {
                bloom.intensity.value = 1.2f;
                bloom.threshold.value = 0.7f;
                bloom.tint.value = new Color(1f, 0.9f, 0.95f);
            }

            if (chromaticAberration != null)
            {
                chromaticAberration.active = true;
                chromaticAberration.intensity.value = 0.15f;
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (volumeProfile != null)
            {
                DestroyImmediate(volumeProfile);
            }
        }

        #endregion
    }
}
