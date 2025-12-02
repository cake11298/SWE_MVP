using UnityEngine;
using BarSimulator.Systems;

namespace BarSimulator.Core
{
    /// <summary>
    /// 場景材質修復器 - 自動修復場景中缺失或紫色的材質
    /// 在編輯器中執行或遊戲開始時自動運行
    /// </summary>
    [ExecuteInEditMode]
    public class SceneMaterialFixer : MonoBehaviour
    {
        #region 序列化欄位

        [Header("自動修復設定")]
        [Tooltip("遊戲開始時自動修復")]
        [SerializeField] private bool autoFixOnStart = true;

        [Tooltip("在編輯器中自動修復")]
        [SerializeField] private bool autoFixInEditor = false;

        [Header("材質設定")]
        [Tooltip("預設地板材質顏色")]
        [SerializeField] private Color floorColor = new Color(0.3f, 0.25f, 0.2f);

        [Tooltip("預設牆壁材質顏色")]
        [SerializeField] private Color wallColor = new Color(0.8f, 0.75f, 0.7f);

        [Tooltip("預設吧台材質顏色")]
        [SerializeField] private Color counterColor = new Color(0.25f, 0.15f, 0.1f);

        [Tooltip("預設 NPC 材質顏色")]
        [SerializeField] private Color npcBodyColor = new Color(0.2f, 0.4f, 0.7f);

        [Tooltip("預設 NPC 頭部材質顏色")]
        [SerializeField] private Color npcHeadColor = new Color(0.9f, 0.75f, 0.6f);

        [Header("Debug")]
        [Tooltip("顯示詳細日誌")]
        [SerializeField] private bool verboseLogging = true;

        #endregion

        #region Unity 生命週期

        private void Start()
        {
            if (autoFixOnStart)
            {
                FixAllMaterials();
            }
        }

        #endregion

        #region 材質修復

        /// <summary>
        /// 修復場景中所有材質
        /// </summary>
        [ContextMenu("修復所有材質")]
        public void FixAllMaterials()
        {
            Debug.Log("SceneMaterialFixer: 開始修復場景材質...");

            int fixedCount = 0;

            // 獲取所有 Renderer
            Renderer[] allRenderers = FindObjectsOfType<Renderer>();

            foreach (Renderer renderer in allRenderers)
            {
                if (renderer == null) continue;

                // 檢查材質是否缺失或使用錯誤的 shader
                if (NeedsMaterialFix(renderer))
                {
                    FixRenderer(renderer);
                    fixedCount++;
                }
            }

            Debug.Log($"SceneMaterialFixer: 修復完成！共修復了 {fixedCount} 個物件");
        }

        /// <summary>
        /// 檢查 Renderer 是否需要修復材質
        /// </summary>
        private bool NeedsMaterialFix(Renderer renderer)
        {
            if (renderer.sharedMaterial == null)
            {
                return true; // 材質缺失
            }

            // 檢查是否使用了錯誤的 shader（導致紫色）
            if (renderer.sharedMaterial.shader == null)
            {
                return true;
            }

            // 檢查 shader 名稱（某些不支援的 shader 會導致紫色）
            string shaderName = renderer.sharedMaterial.shader.name;
            if (shaderName.Contains("Hidden") || shaderName.Contains("Error"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 修復單一 Renderer 的材質
        /// </summary>
        private void FixRenderer(Renderer renderer)
        {
            GameObject obj = renderer.gameObject;
            string objName = obj.name.ToLower();

            if (verboseLogging)
            {
                Debug.Log($"SceneMaterialFixer: 修復物件 '{obj.name}' 的材質");
            }

            // 根據物件名稱判斷應該使用什麼材質
            Material newMaterial = CreateMaterialForObject(obj, objName);

            if (newMaterial != null)
            {
                renderer.sharedMaterial = newMaterial;
            }
        }

        /// <summary>
        /// 根據物件創建適合的材質
        /// </summary>
        private Material CreateMaterialForObject(GameObject obj, string objName)
        {
            MaterialManager matManager = MaterialManager.Instance;

            // 如果沒有 MaterialManager，創建基礎材質
            if (matManager == null)
            {
                return CreateBasicMaterial(obj, objName);
            }

            // NPC 相關
            if (objName.Contains("npc") || obj.GetComponent<BarSimulator.NPC.NPCController>() != null)
            {
                if (objName.Contains("head"))
                {
                    return CreateBasicMaterial(npcHeadColor, "NPC_Head");
                }
                else
                {
                    return CreateBasicMaterial(npcBodyColor, "NPC_Body");
                }
            }

            // 地板
            if (objName.Contains("floor") || objName.Contains("ground") || objName.Contains("plane"))
            {
                return matManager.CreateWoodMaterial("Floor", floorColor, 0.7f);
            }

            // 牆壁
            if (objName.Contains("wall"))
            {
                return CreateBasicMaterial(wallColor, "Wall");
            }

            // 吧台/桌子
            if (objName.Contains("counter") || objName.Contains("bar") || objName.Contains("table"))
            {
                return matManager.CreateBarCounterMaterial();
            }

            // 玻璃/杯子
            if (objName.Contains("glass") || objName.Contains("cup"))
            {
                return matManager.CreateClearGlassMaterial();
            }

            // 瓶子
            if (objName.Contains("bottle"))
            {
                Color bottleColor = new Color(0.2f, 0.6f, 0.3f);
                return matManager.CreateTintedGlassMaterial("Bottle", bottleColor);
            }

            // 金屬物件
            if (objName.Contains("metal") || objName.Contains("faucet") || objName.Contains("tap"))
            {
                return matManager.CreateChromeFaucetMaterial();
            }

            // 座椅
            if (objName.Contains("stool") || objName.Contains("chair") || objName.Contains("seat"))
            {
                return matManager.CreateBarStoolLeatherMaterial();
            }

            // 預設：使用基礎材質
            return CreateBasicMaterial(Color.white, obj.name);
        }

        /// <summary>
        /// 創建基礎材質（當 MaterialManager 不可用時）
        /// </summary>
        private Material CreateBasicMaterial(GameObject obj, string objName)
        {
            Color color = Color.white;

            // 根據名稱判斷顏色
            if (objName.Contains("floor"))
                color = floorColor;
            else if (objName.Contains("wall"))
                color = wallColor;
            else if (objName.Contains("counter") || objName.Contains("bar"))
                color = counterColor;
            else if (objName.Contains("npc"))
                color = npcBodyColor;

            return CreateBasicMaterial(color, obj.name);
        }

        /// <summary>
        /// 創建基礎材質
        /// </summary>
        private Material CreateBasicMaterial(Color color, string materialName)
        {
            // 嘗試使用 URP Lit shader
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            // 如果找不到 URP shader，使用標準 shader
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            // 如果還是找不到，使用 Diffuse
            if (shader == null)
            {
                shader = Shader.Find("Legacy Shaders/Diffuse");
            }

            Material material = new Material(shader);
            material.name = materialName;

            // 設置顏色
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            // 設置光滑度
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.5f);
            }
            else if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.5f);
            }

            return material;
        }

        /// <summary>
        /// 為特定物件設置材質（僅在編輯器中可用）
        /// </summary>
        public void FixSpecificObject(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("SceneMaterialFixer: 物件為空");
                return;
            }

            int fixedCount = 0;
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                if (NeedsMaterialFix(renderer))
                {
                    FixRenderer(renderer);
                    fixedCount++;
                }
            }

            Debug.Log($"SceneMaterialFixer: 修復了 {obj.name} 中的 {fixedCount} 個 Renderer");
        }

        #endregion

        #region 環境優化

        /// <summary>
        /// 優化場景光照設置
        /// </summary>
        [ContextMenu("優化場景光照")]
        public void OptimizeLighting()
        {
            Debug.Log("SceneMaterialFixer: 優化場景光照...");

            // 設置環境光
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.35f, 0.3f);

            // 檢查是否有方向光
            Light[] lights = FindObjectsOfType<Light>();
            Light directionalLight = null;

            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }

            if (directionalLight == null)
            {
                // 創建方向光
                GameObject lightObj = new GameObject("Directional Light");
                directionalLight = lightObj.AddComponent<Light>();
                directionalLight.type = LightType.Directional;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

                Debug.Log("SceneMaterialFixer: 創建了方向光");
            }

            // 設置方向光屬性
            directionalLight.color = new Color(1f, 0.95f, 0.85f);
            directionalLight.intensity = 1.0f;

            // 設置霧效（可選）
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.5f, 0.5f, 0.55f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 10f;
            RenderSettings.fogEndDistance = 50f;

            Debug.Log("SceneMaterialFixer: 光照優化完成");
        }

        #endregion
    }
}
