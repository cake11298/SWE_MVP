using UnityEngine;
using System.Collections.Generic;

namespace BarSimulator.Systems
{
    /// <summary>
    /// Material manager for creating and managing PBR materials
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

        #endregion

        #region Serialized Fields

        [Header("Default Shaders")]
        [SerializeField] private Shader standardShader;
        [SerializeField] private Shader glassShader;

        [Header("PBR Material Properties")]
        [SerializeField] private bool useUnityStandard = true;

        #endregion

        #region Private Fields

        private Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

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

            // Get default shaders
            if (standardShader == null)
            {
                standardShader = Shader.Find("Universal Render Pipeline/Lit");
                if (standardShader == null)
                {
                    standardShader = Shader.Find("Standard");
                }
            }

            if (glassShader == null)
            {
                glassShader = Shader.Find("BarSimulator/GlassAdvanced");
                if (glassShader == null)
                {
                    glassShader = Shader.Find("BarSimulator/Glass");
                }
            }
        }

        #endregion

        #region Material Creation

        /// <summary>
        /// Create a PBR wood material
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

            // Base properties
            SetMaterialColor(mat, baseColor);
            SetMaterialSmoothness(mat, 1f - roughness);
            SetMaterialMetallic(mat, 0f);

            // Wood-specific settings
            mat.SetFloat("_BumpScale", 0.5f);
            mat.SetFloat("_OcclusionStrength", 0.8f);

            materialCache[key] = mat;
            return mat;
        }

        /// <summary>
        /// Create a PBR metal material
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

            // Base properties
            SetMaterialColor(mat, baseColor);
            SetMaterialSmoothness(mat, 1f - roughness);
            SetMaterialMetallic(mat, isChrome ? 1f : 0.9f);

            // Metal-specific
            if (isChrome)
            {
                mat.SetFloat("_EnvironmentReflections", 1f);
            }

            materialCache[key] = mat;
            return mat;
        }

        /// <summary>
        /// Create a glass material
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

            // Glass properties
            Color glassColor = tint;
            glassColor.a = transparency;
            SetMaterialColor(mat, glassColor);

            if (shader == standardShader)
            {
                // Standard shader glass setup
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

        private void SetMaterialColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }
        }

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
            foreach (var kvp in materialCache)
            {
                if (kvp.Value != null)
                {
                    DestroyImmediate(kvp.Value);
                }
            }
            materialCache.Clear();
        }

        private void OnDestroy()
        {
            ClearCache();
        }

        #endregion
    }
}
