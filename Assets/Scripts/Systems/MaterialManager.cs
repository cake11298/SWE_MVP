using UnityEngine;
using System.Collections.Generic;

namespace BarSimulator.Systems
{
    /// <summary>
    /// Material manager for creating and managing PBR materials
    /// 自動偵測 Render Pipeline 並使用程序化紋理生成器
    /// </summary>
    public class MaterialManager : MonoBehaviour
    {
        #region Singleton

        private static MaterialManager instance;
        public static MaterialManager Instance => instance;

        #endregion

        #region Material Types

        public enum MaterialType
        {
            Wood,
            Metal,
            Glass,
            Marble,
            Leather,
            Fabric,
            Plastic
        }

        public enum RenderPipelineType
        {
            BuiltIn,
            URP,
            HDRP
        }

        #endregion

        #region Serialized Fields

        [Header("Default Shaders")]
        [SerializeField] private Shader standardShader;
        [SerializeField] private Shader glassShader;

        [Header("Procedural Texture Settings")]
        [SerializeField] private int textureResolution = 512;
        [SerializeField] private bool generateNormalMaps = true;

        #endregion

        #region Private Fields

        private Dictionary<string, Material> materialCache = new Dictionary<string, Material>();
        private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
        private RenderPipelineType currentPipeline;

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

            // 自動偵測 Render Pipeline
            DetectRenderPipeline();

            // 根據 Pipeline 獲取對應的 Shader
            LoadShaders();

            Debug.Log($"MaterialManager initialized with {currentPipeline} pipeline");
        }

        #endregion

        #region Render Pipeline Detection

        /// <summary>
        /// 自動偵測當前使用的 Render Pipeline
        /// </summary>
        private void DetectRenderPipeline()
        {
            // 嘗試偵測 URP
            var urpAsset = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            if (urpAsset != null)
            {
                string pipelineName = urpAsset.GetType().Name;
                if (pipelineName.Contains("Universal") || pipelineName.Contains("URP"))
                {
                    currentPipeline = RenderPipelineType.URP;
                    Debug.Log("Detected URP (Universal Render Pipeline)");
                    return;
                }
                else if (pipelineName.Contains("HD") || pipelineName.Contains("HighDefinition"))
                {
                    currentPipeline = RenderPipelineType.HDRP;
                    Debug.Log("Detected HDRP (High Definition Render Pipeline)");
                    return;
                }
            }

            // 預設使用 Built-in Pipeline
            currentPipeline = RenderPipelineType.BuiltIn;
            Debug.Log("Using Built-in Render Pipeline");
        }

        /// <summary>
        /// 根據偵測到的 Pipeline 載入對應的 Shader
        /// </summary>
        private void LoadShaders()
        {
            if (standardShader == null)
            {
                switch (currentPipeline)
                {
                    case RenderPipelineType.URP:
                        standardShader = Shader.Find("Universal Render Pipeline/Lit");
                        if (standardShader == null)
                        {
                            Debug.LogWarning("URP Lit shader not found, falling back to Standard");
                            standardShader = Shader.Find("Standard");
                        }
                        break;

                    case RenderPipelineType.HDRP:
                        standardShader = Shader.Find("HDRP/Lit");
                        if (standardShader == null)
                        {
                            Debug.LogWarning("HDRP Lit shader not found, falling back to Standard");
                            standardShader = Shader.Find("Standard");
                        }
                        break;

                    case RenderPipelineType.BuiltIn:
                    default:
                        standardShader = Shader.Find("Standard");
                        break;
                }
            }

            // 玻璃 Shader（嘗試找自訂 Shader，否則使用標準 Shader）
            if (glassShader == null)
            {
                glassShader = Shader.Find("BarSimulator/GlassAdvanced");
                if (glassShader == null)
                {
                    glassShader = Shader.Find("BarSimulator/Glass");
                }
                // 如果自訂 Shader 都找不到，使用標準透明 Shader
                if (glassShader == null)
                {
                    glassShader = standardShader;
                    Debug.LogWarning("Custom glass shader not found, using standard shader for glass");
                }
            }

            // 最終檢查：確保至少有一個可用的 Shader
            if (standardShader == null)
            {
                Debug.LogError("CRITICAL: No shader found! All materials will be pink/magenta!");
            }
        }

        #endregion

        #region Material Creation

        /// <summary>
        /// Create a PBR wood material with procedural texture
        /// </summary>
        public Material CreateWoodMaterial(string name, Color baseColor, float roughness = 0.7f)
        {
            string key = $"Wood_{name}";
            if (materialCache.TryGetValue(key, out Material cached))
            {
                return cached;
            }

            Material mat = new Material(standardShader);
            mat.name = name;

            // 生成程序化木紋紋理
            Texture2D woodTexture = GetOrCreateTexture(
                $"WoodTexture_{name}",
                () => ProceduralTextureGenerator.GenerateWoodTexture(
                    textureResolution,
                    textureResolution,
                    baseColor,
                    baseColor * 1.5f, // 較亮的木紋顏色
                    0.05f,
                    0.4f
                )
            );

            // 設定材質屬性
            SetMaterialTexture(mat, "_MainTex", woodTexture);
            SetMaterialTexture(mat, "_BaseMap", woodTexture); // URP
            SetMaterialColor(mat, Color.white); // 讓紋理顏色完全顯示
            SetMaterialSmoothness(mat, 1f - roughness);
            SetMaterialMetallic(mat, 0f);

            // Wood-specific settings
            if (mat.HasProperty("_BumpScale"))
                mat.SetFloat("_BumpScale", 0.5f);
            if (mat.HasProperty("_OcclusionStrength"))
                mat.SetFloat("_OcclusionStrength", 0.8f);

            materialCache[key] = mat;
            return mat;
        }

        /// <summary>
        /// Create a PBR metal material with procedural texture
        /// </summary>
        public Material CreateMetalMaterial(string name, Color baseColor, float roughness = 0.3f, bool isChrome = false)
        {
            string key = $"Metal_{name}";
            if (materialCache.TryGetValue(key, out Material cached))
            {
                return cached;
            }

            Material mat = new Material(standardShader);
            mat.name = name;

            // 生成程序化金屬紋理
            Texture2D metalTexture = GetOrCreateTexture(
                $"MetalTexture_{name}",
                () => ProceduralTextureGenerator.GenerateMetalTexture(
                    textureResolution,
                    textureResolution,
                    baseColor,
                    roughness,
                    true // 啟用拉絲效果
                )
            );

            // 設定材質屬性
            SetMaterialTexture(mat, "_MainTex", metalTexture);
            SetMaterialTexture(mat, "_BaseMap", metalTexture); // URP
            SetMaterialColor(mat, Color.white); // 讓紋理顏色完全顯示
            SetMaterialSmoothness(mat, 1f - roughness);
            SetMaterialMetallic(mat, isChrome ? 1f : 0.9f);

            // Metal-specific
            if (isChrome && mat.HasProperty("_EnvironmentReflections"))
            {
                mat.SetFloat("_EnvironmentReflections", 1f);
            }

            materialCache[key] = mat;
            return mat;
        }

        /// <summary>
        /// Create a glass material with procedural texture
        /// </summary>
        public Material CreateGlassMaterial(string name, Color tint, float transparency = 0.1f)
        {
            string key = $"Glass_{name}";
            if (materialCache.TryGetValue(key, out Material cached))
            {
                return cached;
            }

            Shader shader = glassShader ?? standardShader;
            Material mat = new Material(shader);
            mat.name = name;

            // 生成程序化玻璃紋理
            Texture2D glassTexture = GetOrCreateTexture(
                $"GlassTexture_{name}",
                () => ProceduralTextureGenerator.GenerateGlassTexture(
                    textureResolution,
                    textureResolution,
                    tint,
                    0.95f // 高清晰度
                )
            );

            // 設定材質紋理
            SetMaterialTexture(mat, "_MainTex", glassTexture);
            SetMaterialTexture(mat, "_BaseMap", glassTexture); // URP

            // Glass properties
            Color glassColor = tint;
            glassColor.a = transparency;
            SetMaterialColor(mat, glassColor);

            // 根據 Pipeline 設定透明度
            if (currentPipeline == RenderPipelineType.URP)
            {
                // URP 透明設定
                if (mat.HasProperty("_Surface"))
                {
                    mat.SetFloat("_Surface", 1); // Transparent
                }
                if (mat.HasProperty("_Blend"))
                {
                    mat.SetFloat("_Blend", 0); // Alpha blend
                }
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else if (shader == standardShader || currentPipeline == RenderPipelineType.BuiltIn)
            {
                // Built-in Standard shader glass setup
                if (mat.HasProperty("_Mode"))
                    mat.SetFloat("_Mode", 3); // Transparent mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }

            SetMaterialSmoothness(mat, 0.98f);
            SetMaterialMetallic(mat, 0f);

            materialCache[key] = mat;
            return mat;
        }

        /// <summary>
        /// Create a marble material
        /// </summary>
        public Material CreateMarbleMaterial(string name, Color baseColor, Color veinColor, float roughness = 0.2f)
        {
            string key = $"Marble_{name}";
            if (materialCache.TryGetValue(key, out Material cached))
            {
                return cached;
            }

            Material mat = new Material(standardShader);
            mat.name = name;

            // Marble base
            SetMaterialColor(mat, baseColor);
            SetMaterialSmoothness(mat, 1f - roughness);
            SetMaterialMetallic(mat, 0f);

            // Secondary color would need a custom shader or texture
            mat.SetColor("_EmissionColor", veinColor * 0.05f);

            materialCache[key] = mat;
            return mat;
        }

        /// <summary>
        /// Create a leather material
        /// </summary>
        public Material CreateLeatherMaterial(string name, Color baseColor, float roughness = 0.6f)
        {
            string key = $"Leather_{name}";
            if (materialCache.TryGetValue(key, out Material cached))
            {
                return cached;
            }

            Material mat = new Material(standardShader);
            mat.name = name;

            SetMaterialColor(mat, baseColor);
            SetMaterialSmoothness(mat, 1f - roughness);
            SetMaterialMetallic(mat, 0f);

            // Leather-specific
            mat.SetFloat("_BumpScale", 0.3f);

            materialCache[key] = mat;
            return mat;
        }

        /// <summary>
        /// Create a fabric material
        /// </summary>
        public Material CreateFabricMaterial(string name, Color baseColor)
        {
            string key = $"Fabric_{name}";
            if (materialCache.TryGetValue(key, out Material cached))
            {
                return cached;
            }

            Material mat = new Material(standardShader);
            mat.name = name;

            SetMaterialColor(mat, baseColor);
            SetMaterialSmoothness(mat, 0.1f); // Very rough
            SetMaterialMetallic(mat, 0f);

            materialCache[key] = mat;
            return mat;
        }

        #endregion

        #region Material Helpers

        /// <summary>
        /// 獲取或創建紋理（使用快取）
        /// </summary>
        private Texture2D GetOrCreateTexture(string key, System.Func<Texture2D> generator)
        {
            if (textureCache.TryGetValue(key, out Texture2D cached))
            {
                return cached;
            }

            Texture2D texture = generator();
            textureCache[key] = texture;
            return texture;
        }

        /// <summary>
        /// 設定材質紋理（跨 Pipeline 相容）
        /// </summary>
        private void SetMaterialTexture(Material mat, string propertyName, Texture2D texture)
        {
            if (mat.HasProperty(propertyName))
            {
                mat.SetTexture(propertyName, texture);
            }
        }

        /// <summary>
        /// 設定材質顏色（跨 Pipeline 相容）
        /// </summary>
        private void SetMaterialColor(Material mat, Color color)
        {
            // URP 使用 _BaseColor
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            // Built-in 使用 _Color
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }
        }

        /// <summary>
        /// 設定材質光滑度（跨 Pipeline 相容）
        /// </summary>
        private void SetMaterialSmoothness(Material mat, float smoothness)
        {
            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", smoothness);
            }
            else if (mat.HasProperty("_Glossiness"))
            {
                mat.SetFloat("_Glossiness", smoothness);
            }
        }

        /// <summary>
        /// 設定材質金屬度（跨 Pipeline 相容）
        /// </summary>
        private void SetMaterialMetallic(Material mat, float metallic)
        {
            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", metallic);
            }
        }

        #endregion

        #region Preset Materials

        /// <summary>
        /// Create bar counter wood material (dark polished wood)
        /// </summary>
        public Material CreateBarCounterMaterial()
        {
            return CreateWoodMaterial("BarCounter", new Color(0.3f, 0.15f, 0.05f), 0.4f);
        }

        /// <summary>
        /// Create chrome faucet material
        /// </summary>
        public Material CreateChromeFaucetMaterial()
        {
            return CreateMetalMaterial("ChromeFaucet", new Color(0.9f, 0.9f, 0.9f), 0.1f, true);
        }

        /// <summary>
        /// Create brass fixture material
        /// </summary>
        public Material CreateBrassFixtureMaterial()
        {
            return CreateMetalMaterial("BrassFixture", new Color(0.8f, 0.6f, 0.2f), 0.3f, false);
        }

        /// <summary>
        /// Create clear glass material
        /// </summary>
        public Material CreateClearGlassMaterial()
        {
            return CreateGlassMaterial("ClearGlass", new Color(0.95f, 0.98f, 1f), 0.08f);
        }

        /// <summary>
        /// Create tinted glass material (for bottles)
        /// </summary>
        public Material CreateTintedGlassMaterial(string name, Color tint)
        {
            return CreateGlassMaterial(name, tint, 0.15f);
        }

        /// <summary>
        /// Create bar stool leather material
        /// </summary>
        public Material CreateBarStoolLeatherMaterial()
        {
            return CreateLeatherMaterial("BarStoolLeather", new Color(0.15f, 0.08f, 0.05f), 0.5f);
        }

        /// <summary>
        /// Create white marble counter material
        /// </summary>
        public Material CreateWhiteMarbleMaterial()
        {
            return CreateMarbleMaterial("WhiteMarble", new Color(0.95f, 0.95f, 0.95f), new Color(0.7f, 0.7f, 0.8f), 0.15f);
        }

        /// <summary>
        /// Create black marble counter material
        /// </summary>
        public Material CreateBlackMarbleMaterial()
        {
            return CreateMarbleMaterial("BlackMarble", new Color(0.1f, 0.1f, 0.1f), new Color(0.2f, 0.2f, 0.25f), 0.15f);
        }

        #endregion

        #region Apply to Objects

        /// <summary>
        /// Apply material to a renderer
        /// </summary>
        public void ApplyMaterialToRenderer(Renderer renderer, Material material)
        {
            if (renderer != null && material != null)
            {
                renderer.material = material;
            }
        }

        /// <summary>
        /// Apply material to object and all children
        /// </summary>
        public void ApplyMaterialToHierarchy(GameObject root, Material material)
        {
            if (root == null || material == null) return;

            var renderers = root.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material = material;
            }
        }

        /// <summary>
        /// Create and apply appropriate material based on object name
        /// </summary>
        public void AutoApplyMaterial(GameObject obj)
        {
            string name = obj.name.ToLower();
            Material mat = null;

            if (name.Contains("counter") || name.Contains("bar") || name.Contains("table"))
            {
                mat = CreateBarCounterMaterial();
            }
            else if (name.Contains("glass") || name.Contains("cup"))
            {
                mat = CreateClearGlassMaterial();
            }
            else if (name.Contains("bottle"))
            {
                // Determine bottle color from existing material
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    Color existingColor = renderer.material.color;
                    mat = CreateTintedGlassMaterial(name, existingColor);
                }
                else
                {
                    mat = CreateClearGlassMaterial();
                }
            }
            else if (name.Contains("faucet") || name.Contains("tap"))
            {
                mat = CreateChromeFaucetMaterial();
            }
            else if (name.Contains("stool") || name.Contains("seat"))
            {
                mat = CreateBarStoolLeatherMaterial();
            }

            if (mat != null)
            {
                ApplyMaterialToHierarchy(obj, mat);
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Clear material cache
        /// </summary>
        public void ClearCache()
        {
            // 清理材質
            foreach (var kvp in materialCache)
            {
                if (kvp.Value != null)
                {
                    DestroyImmediate(kvp.Value);
                }
            }
            materialCache.Clear();

            // 清理紋理
            foreach (var kvp in textureCache)
            {
                if (kvp.Value != null)
                {
                    DestroyImmediate(kvp.Value);
                }
            }
            textureCache.Clear();
        }

        private void OnDestroy()
        {
            ClearCache();
        }

        #endregion
    }
}
