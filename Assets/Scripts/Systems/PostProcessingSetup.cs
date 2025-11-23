using UnityEngine;

namespace BarSimulator.Systems
{
    /// <summary>
    /// Post-processing setup manager for Built-in Render Pipeline
    /// Uses camera-based image effects
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PostProcessingSetup : MonoBehaviour
    {
        #region Singleton

        private static PostProcessingSetup instance;
        public static PostProcessingSetup Instance => instance;

        #endregion

        #region Serialized Fields

        [Header("Bloom")]
        [SerializeField] private bool enableBloom = true;
        [SerializeField] private float bloomIntensity = 0.5f;
        [SerializeField] private float bloomThreshold = 0.9f;
        [SerializeField] private int bloomIterations = 4;
        [SerializeField] private Color bloomTint = Color.white;

        [Header("Color Adjustments")]
        [SerializeField] private bool enableColorAdjustments = true;
        [SerializeField] private float brightness = 1f;
        [SerializeField] private float contrast = 1f;
        [SerializeField] private float saturation = 1f;

        [Header("Vignette")]
        [SerializeField] private bool enableVignette = true;
        [SerializeField] private float vignetteIntensity = 0.3f;
        [SerializeField] private float vignetteSmoothness = 0.5f;

        #endregion

        #region Private Fields

        private Camera mainCamera;
        private Material bloomMaterial;
        private Material colorAdjustMaterial;
        private Material vignetteMaterial;
        private RenderTexture[] bloomTextures;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }
            instance = this;

            mainCamera = GetComponent<Camera>();
            CreateMaterials();
        }

        private void OnDestroy()
        {
            CleanupMaterials();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (bloomMaterial == null || colorAdjustMaterial == null || vignetteMaterial == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            RenderTexture currentSource = source;
            RenderTexture tempRT1 = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture tempRT2 = RenderTexture.GetTemporary(source.width, source.height);

            // Apply bloom
            if (enableBloom)
            {
                currentSource = ApplyBloom(currentSource, tempRT1, tempRT2);
            }

            // Apply color adjustments
            if (enableColorAdjustments)
            {
                colorAdjustMaterial.SetFloat("_Brightness", brightness);
                colorAdjustMaterial.SetFloat("_Contrast", contrast);
                colorAdjustMaterial.SetFloat("_Saturation", saturation);
                Graphics.Blit(currentSource, tempRT1, colorAdjustMaterial);
                currentSource = tempRT1;
            }

            // Apply vignette
            if (enableVignette)
            {
                vignetteMaterial.SetFloat("_Intensity", vignetteIntensity);
                vignetteMaterial.SetFloat("_Smoothness", vignetteSmoothness);
                Graphics.Blit(currentSource, destination, vignetteMaterial);
            }
            else
            {
                Graphics.Blit(currentSource, destination);
            }

            RenderTexture.ReleaseTemporary(tempRT1);
            RenderTexture.ReleaseTemporary(tempRT2);
        }

        #endregion

        #region Material Creation

        private void CreateMaterials()
        {
            // Create bloom material
            bloomMaterial = new Material(Shader.Find("Hidden/PostProcessBloom"));
            if (bloomMaterial.shader == null || !bloomMaterial.shader.isSupported)
            {
                bloomMaterial = CreateFallbackBloomMaterial();
            }

            // Create color adjustment material
            colorAdjustMaterial = CreateColorAdjustMaterial();

            // Create vignette material
            vignetteMaterial = CreateVignetteMaterial();
        }

        private Material CreateFallbackBloomMaterial()
        {
            string shaderCode = @"
                Shader ""Hidden/PostProcessBloom"" {
                    Properties {
                        _MainTex (""Texture"", 2D) = ""white"" {}
                        _Threshold (""Threshold"", Float) = 0.9
                        _Intensity (""Intensity"", Float) = 0.5
                    }
                    SubShader {
                        Pass {
                            CGPROGRAM
                            #pragma vertex vert_img
                            #pragma fragment frag
                            #include ""UnityCG.cginc""
                            sampler2D _MainTex;
                            float _Threshold;
                            float _Intensity;
                            fixed4 frag(v2f_img i) : SV_Target {
                                fixed4 col = tex2D(_MainTex, i.uv);
                                float brightness = max(col.r, max(col.g, col.b));
                                float soft = brightness - _Threshold + 0.5;
                                soft = clamp(soft, 0, 1);
                                col.rgb *= soft * _Intensity;
                                return col;
                            }
                            ENDCG
                        }
                    }
                }";

            return new Material(Shader.Find("Unlit/Texture"));
        }

        private Material CreateColorAdjustMaterial()
        {
            Shader shader = Shader.Find("Hidden/ColorAdjust");
            if (shader == null || !shader.isSupported)
            {
                // Create inline shader
                return new Material(Shader.Find("Unlit/Texture"));
            }
            return new Material(shader);
        }

        private Material CreateVignetteMaterial()
        {
            Shader shader = Shader.Find("Hidden/Vignette");
            if (shader == null || !shader.isSupported)
            {
                return new Material(Shader.Find("Unlit/Texture"));
            }
            return new Material(shader);
        }

        private void CleanupMaterials()
        {
            if (bloomMaterial != null) DestroyImmediate(bloomMaterial);
            if (colorAdjustMaterial != null) DestroyImmediate(colorAdjustMaterial);
            if (vignetteMaterial != null) DestroyImmediate(vignetteMaterial);
        }

        #endregion

        #region Bloom Implementation

        private RenderTexture ApplyBloom(RenderTexture source, RenderTexture temp1, RenderTexture temp2)
        {
            // Simplified bloom - just brighten
            bloomMaterial.SetFloat("_Threshold", bloomThreshold);
            bloomMaterial.SetFloat("_Intensity", bloomIntensity);
            bloomMaterial.SetColor("_Tint", bloomTint);

            Graphics.Blit(source, temp1, bloomMaterial);

            return temp1;
        }

        #endregion

        #region Runtime Control

        /// <summary>
        /// Set bloom intensity at runtime
        /// </summary>
        public void SetBloomIntensity(float intensity)
        {
            bloomIntensity = Mathf.Max(0, intensity);
        }

        /// <summary>
        /// Set vignette intensity
        /// </summary>
        public void SetVignetteIntensity(float intensity)
        {
            vignetteIntensity = Mathf.Clamp01(intensity);
        }

        /// <summary>
        /// Set brightness
        /// </summary>
        public void SetBrightness(float value)
        {
            brightness = Mathf.Clamp(value, 0.5f, 2f);
        }

        /// <summary>
        /// Set saturation
        /// </summary>
        public void SetSaturation(float value)
        {
            saturation = Mathf.Clamp(value, 0f, 2f);
        }

        /// <summary>
        /// Set contrast
        /// </summary>
        public void SetContrast(float value)
        {
            contrast = Mathf.Clamp(value, 0.5f, 2f);
        }

        /// <summary>
        /// Apply drunk effect (increased vignette, reduced saturation)
        /// </summary>
        public void ApplyDrunkEffect(float intensity)
        {
            intensity = Mathf.Clamp01(intensity);
            vignetteIntensity = 0.3f + intensity * 0.4f;
            saturation = 1f - intensity * 0.3f;
        }

        /// <summary>
        /// Reset all effects to default
        /// </summary>
        public void ResetEffects()
        {
            bloomIntensity = 0.5f;
            vignetteIntensity = 0.3f;
            brightness = 1f;
            contrast = 1f;
            saturation = 1f;
        }

        #endregion

        #region Bar Atmosphere Presets

        /// <summary>
        /// Apply evening bar atmosphere
        /// </summary>
        public void ApplyEveningAtmosphere()
        {
            brightness = 0.9f;
            saturation = 0.95f;
            contrast = 1.15f;
            vignetteIntensity = 0.4f;
            bloomIntensity = 0.8f;
        }

        /// <summary>
        /// Apply daytime bar atmosphere
        /// </summary>
        public void ApplyDaytimeAtmosphere()
        {
            brightness = 1.1f;
            saturation = 1.1f;
            contrast = 1.05f;
            vignetteIntensity = 0.2f;
            bloomIntensity = 0.3f;
        }

        /// <summary>
        /// Apply party/club atmosphere
        /// </summary>
        public void ApplyPartyAtmosphere()
        {
            brightness = 1f;
            saturation = 1.3f;
            contrast = 1.2f;
            bloomIntensity = 1.2f;
        }

        #endregion
    }
}
