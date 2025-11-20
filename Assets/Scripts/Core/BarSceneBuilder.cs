using UnityEngine;
using System.Collections.Generic;
using BarSimulator.Data;
using BarSimulator.Objects;
using BarSimulator.Interaction;

namespace BarSimulator.Core
{
    /// <summary>
    /// 吧台場景建造器 - 程式化生成完整酒吧場景
    /// 參考: Three.js BarStructure.js, BarBottles.js, BarEnvironment.js
    /// </summary>
    public class BarSceneBuilder : MonoBehaviour
    {
        #region 單例

        private static BarSceneBuilder instance;
        public static BarSceneBuilder Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("資料庫")]
        [SerializeField] private LiquorDatabase liquorDatabase;

        [Header("場景尺寸")]
        [Tooltip("房間尺寸")]
        [SerializeField] private Vector3 roomSize = new Vector3(20f, 10f, 25f);

        [Header("材質顏色")]
        [SerializeField] private Color floorColor = new Color(0.17f, 0.09f, 0.06f); // 0x2c1810
        [SerializeField] private Color wallColor = new Color(0.36f, 0.31f, 0.22f);  // 0x5d4e37
        [SerializeField] private Color woodColor = new Color(0.40f, 0.26f, 0.13f);  // 0x654321
        [SerializeField] private Color marbleColor = new Color(0.91f, 0.91f, 0.91f); // 0xe8e8e8
        [SerializeField] private Color shelfBackColor = new Color(0.29f, 0.24f, 0.16f); // 0x4a3c28

        [Header("生成設定")]
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private int glassCount = 6;
        [SerializeField] private int bottlesPerShelf = 8;

        #endregion

        #region 私有欄位

        // 生成的物件集合
        private List<GameObject> structureObjects = new List<GameObject>();
        private List<Bottle> bottles = new List<Bottle>();
        private List<Glass> glasses = new List<Glass>();
        private Shaker shaker;
        private List<GameObject> npcs = new List<GameObject>();

        // 架子高度
        private float[] shelfHeights = { 1.5f, 2.7f, 3.9f };

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

        private void Start()
        {
            // 載入資料庫
            if (liquorDatabase == null)
            {
                liquorDatabase = Resources.Load<LiquorDatabase>("LiquorDatabase");
            }

            if (buildOnStart)
            {
                BuildScene();
            }
        }

        #endregion

        #region 主要建造方法

        /// <summary>
        /// 建造完整場景
        /// </summary>
        public void BuildScene()
        {
            Debug.Log("BarSceneBuilder: Building bar scene...");

            // 1. 建造基礎結構
            BuildFloor();
            BuildWalls();
            BuildBarCounter();
            BuildLiquorShelf();

            // 2. 建造物件
            BuildBottles();
            BuildGlasses();
            BuildShaker();

            // 3. 建造 NPC
            BuildNPCs();

            // 4. 設置燈光
            SetupLighting();

            Debug.Log($"BarSceneBuilder: Scene built - {bottles.Count} bottles, {glasses.Count} glasses, {npcs.Count} NPCs");
        }

        #endregion

        #region 基礎結構

        /// <summary>
        /// 建造地板
        /// 參考: BarStructure.js createFloor()
        /// </summary>
        private void BuildFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(roomSize.x / 10f, 1f, roomSize.z / 10f);

            var renderer = floor.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Standard"));
            material.color = floorColor;
            material.SetFloat("_Glossiness", 0.6f);
            renderer.material = material;

            structureObjects.Add(floor);
        }

        /// <summary>
        /// 建造牆壁
        /// 參考: BarStructure.js createWalls()
        /// </summary>
        private void BuildWalls()
        {
            var wallMaterial = new Material(Shader.Find("Standard"));
            wallMaterial.color = wallColor;
            wallMaterial.SetFloat("_Glossiness", 0.1f);

            // 牆壁配置: 位置, 旋轉, 尺寸
            var wallConfigs = new[]
            {
                // 後牆 (酒架背後)
                new { pos = new Vector3(0f, 5f, -roomSize.z / 2f), rot = 0f, size = new Vector3(roomSize.x, roomSize.y, 0.2f) },
                // 左牆
                new { pos = new Vector3(-roomSize.x / 2f, 5f, 0f), rot = 90f, size = new Vector3(roomSize.z, roomSize.y, 0.2f) },
                // 右牆
                new { pos = new Vector3(roomSize.x / 2f, 5f, 0f), rot = -90f, size = new Vector3(roomSize.z, roomSize.y, 0.2f) },
                // 前牆
                new { pos = new Vector3(0f, 5f, roomSize.z / 2f), rot = 180f, size = new Vector3(roomSize.x, roomSize.y, 0.2f) }
            };

            foreach (var config in wallConfigs)
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "Wall";
                wall.transform.position = config.pos;
                wall.transform.rotation = Quaternion.Euler(0f, config.rot, 0f);
                wall.transform.localScale = config.size;

                var renderer = wall.GetComponent<Renderer>();
                renderer.material = wallMaterial;

                structureObjects.Add(wall);
            }
        }

        /// <summary>
        /// 建造吧台
        /// 參考: BarStructure.js createBarCounter()
        /// </summary>
        private void BuildBarCounter()
        {
            // 吧台主體 (木材)
            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "BarCounter";
            counter.transform.position = new Vector3(0f, 0.5f, -3f);
            counter.transform.localScale = new Vector3(12f, 1f, 2f);

            var woodMaterial = new Material(Shader.Find("Standard"));
            woodMaterial.color = woodColor;
            woodMaterial.SetFloat("_Glossiness", 0.4f);
            counter.GetComponent<Renderer>().material = woodMaterial;

            // 大理石檯面
            var counterTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counterTop.name = "CounterTop";
            counterTop.transform.position = new Vector3(0f, 1.05f, -3f);
            counterTop.transform.localScale = new Vector3(12.2f, 0.1f, 2.2f);

            var marbleMaterial = new Material(Shader.Find("Standard"));
            marbleMaterial.color = marbleColor;
            marbleMaterial.SetFloat("_Glossiness", 0.9f);
            marbleMaterial.SetFloat("_Metallic", 0.1f);
            counterTop.GetComponent<Renderer>().material = marbleMaterial;

            structureObjects.Add(counter);
            structureObjects.Add(counterTop);
        }

        /// <summary>
        /// 建造酒架
        /// 參考: BarStructure.js createLiquorShelf()
        /// </summary>
        private void BuildLiquorShelf()
        {
            // 酒架背板
            var shelfBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelfBack.name = "ShelfBack";
            shelfBack.transform.position = new Vector3(0f, 3f, -8.5f);
            shelfBack.transform.localScale = new Vector3(10f, 5f, 0.3f);

            var backMaterial = new Material(Shader.Find("Standard"));
            backMaterial.color = shelfBackColor;
            backMaterial.SetFloat("_Glossiness", 0.2f);
            shelfBack.GetComponent<Renderer>().material = backMaterial;

            structureObjects.Add(shelfBack);

            // 建立三層架子
            var shelfMaterial = new Material(Shader.Find("Standard"));
            shelfMaterial.color = woodColor;
            shelfMaterial.SetFloat("_Glossiness", 0.5f);

            for (int i = 0; i < shelfHeights.Length; i++)
            {
                // 架子
                var shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.name = $"Shelf_{i}";
                shelf.transform.position = new Vector3(0f, shelfHeights[i], -8f);
                shelf.transform.localScale = new Vector3(10f, 0.2f, 1f);
                shelf.GetComponent<Renderer>().material = shelfMaterial;

                structureObjects.Add(shelf);

                // LED 燈帶效果
                var led = GameObject.CreatePrimitive(PrimitiveType.Cube);
                led.name = $"LED_{i}";
                led.transform.position = new Vector3(0f, shelfHeights[i] - 0.1f, -7.5f);
                led.transform.localScale = new Vector3(10f, 0.02f, 0.05f);

                var ledMaterial = new Material(Shader.Find("Standard"));
                ledMaterial.color = Color.white;
                ledMaterial.SetFloat("_Glossiness", 1f);
                ledMaterial.EnableKeyword("_EMISSION");
                ledMaterial.SetColor("_EmissionColor", Color.white * 0.5f);
                led.GetComponent<Renderer>().material = ledMaterial;

                // 移除 LED 碰撞體
                Destroy(led.GetComponent<Collider>());

                structureObjects.Add(led);
            }
        }

        #endregion

        #region 物件生成

        /// <summary>
        /// 建造酒瓶
        /// 參考: BarBottles.js createShelfBottles()
        /// </summary>
        private void BuildBottles()
        {
            if (liquorDatabase == null || liquorDatabase.liquors == null)
            {
                Debug.LogWarning("BarSceneBuilder: No liquor database available");
                return;
            }

            int liquorIndex = 0;

            // 每層架子放置酒瓶
            for (int shelfIdx = 0; shelfIdx < shelfHeights.Length; shelfIdx++)
            {
                float shelfY = shelfHeights[shelfIdx] + 0.25f; // 架子上方
                float startX = -4f;
                float spacing = 1f;

                for (int i = 0; i < bottlesPerShelf && liquorIndex < liquorDatabase.liquors.Length; i++)
                {
                    var liquorData = liquorDatabase.liquors[liquorIndex];

                    // 建立酒瓶
                    var bottleObj = CreateBottle(liquorData);
                    bottleObj.transform.position = new Vector3(startX + i * spacing, shelfY, -8f);

                    var bottle = bottleObj.GetComponent<Bottle>();
                    bottles.Add(bottle);

                    liquorIndex++;
                }
            }
        }

        /// <summary>
        /// 建立單一酒瓶
        /// </summary>
        private GameObject CreateBottle(LiquorData liquorData)
        {
            var bottleObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bottleObj.name = $"Bottle_{liquorData.id}";
            bottleObj.transform.localScale = new Vector3(0.1f, 0.25f, 0.1f);

            // 設定顏色
            var renderer = bottleObj.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Standard"));
            material.color = liquorData.color;
            material.SetFloat("_Glossiness", 0.8f);
            renderer.material = material;

            // 添加 Bottle 組件
            var bottle = bottleObj.AddComponent<Bottle>();
            bottle.Initialize(liquorData);

            // 添加 Rigidbody
            var rb = bottleObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // 設定 Layer
            bottleObj.layer = LayerMask.NameToLayer("Interactable");

            return bottleObj;
        }

        /// <summary>
        /// 建造酒杯
        /// 參考: BarBottles.js createDrinkingGlasses()
        /// </summary>
        private void BuildGlasses()
        {
            float startX = -2f;
            float spacing = 0.8f;
            float y = 1.15f; // 吧台檯面上
            float z = -2.5f;

            for (int i = 0; i < glassCount; i++)
            {
                var glassObj = CreateGlass();
                glassObj.transform.position = new Vector3(startX + i * spacing, y, z);

                var glass = glassObj.GetComponent<Glass>();
                glasses.Add(glass);
            }
        }

        /// <summary>
        /// 建立單一酒杯
        /// </summary>
        private GameObject CreateGlass()
        {
            var glassObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glassObj.name = "Glass";
            glassObj.transform.localScale = new Vector3(0.12f, 0.15f, 0.12f);

            // 透明玻璃材質
            var renderer = glassObj.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Standard"));
            material.SetFloat("_Mode", 3); // Transparent
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            material.color = new Color(0.9f, 0.9f, 0.9f, 0.3f);
            material.SetFloat("_Glossiness", 0.95f);
            renderer.material = material;

            // 添加 Glass 組件
            var glass = glassObj.AddComponent<Glass>();

            // 添加 Rigidbody
            var rb = glassObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // 設定 Layer
            glassObj.layer = LayerMask.NameToLayer("Interactable");

            return glassObj;
        }

        /// <summary>
        /// 建造搖酒器
        /// </summary>
        private void BuildShaker()
        {
            var shakerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            shakerObj.name = "Shaker";
            shakerObj.transform.position = new Vector3(3f, 1.2f, -2.5f);
            shakerObj.transform.localScale = new Vector3(0.15f, 0.2f, 0.15f);

            // 金屬材質
            var renderer = shakerObj.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.8f, 0.8f, 0.85f);
            material.SetFloat("_Metallic", 0.8f);
            material.SetFloat("_Glossiness", 0.7f);
            renderer.material = material;

            // 添加 Shaker 組件
            shaker = shakerObj.AddComponent<Shaker>();

            // 添加 Rigidbody
            var rb = shakerObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // 設定 Layer
            shakerObj.layer = LayerMask.NameToLayer("Interactable");
        }

        #endregion

        #region NPC 生成

        /// <summary>
        /// 建造 NPC
        /// </summary>
        private void BuildNPCs()
        {
            // NPC 配置
            var npcConfigs = new[]
            {
                new { name = "Tom", pos = new Vector3(-4f, 0f, 0f), color = new Color(0.8f, 0.2f, 0.2f) },
                new { name = "Lisa", pos = new Vector3(0f, 0f, 2f), color = new Color(0.2f, 0.8f, 0.2f) },
                new { name = "Mike", pos = new Vector3(4f, 0f, 0f), color = new Color(0.2f, 0.2f, 0.8f) }
            };

            foreach (var config in npcConfigs)
            {
                var npc = CreateNPC(config.name, config.pos, config.color);
                npcs.Add(npc);
            }
        }

        /// <summary>
        /// 建立單一 NPC
        /// </summary>
        private GameObject CreateNPC(string npcName, Vector3 position, Color shirtColor)
        {
            var npcRoot = new GameObject($"NPC_{npcName}");
            npcRoot.transform.position = position;

            // 身體 (膠囊體)
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(npcRoot.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            var bodyMaterial = new Material(Shader.Find("Standard"));
            bodyMaterial.color = shirtColor;
            body.GetComponent<Renderer>().material = bodyMaterial;

            // 頭部 (球體)
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(npcRoot.transform);
            head.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            head.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            var headMaterial = new Material(Shader.Find("Standard"));
            headMaterial.color = new Color(0.96f, 0.87f, 0.7f); // 膚色
            head.GetComponent<Renderer>().material = headMaterial;

            // 添加 NPCController
            var controller = npcRoot.AddComponent<NPC.NPCController>();

            // 添加碰撞體
            var collider = npcRoot.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.3f;
            collider.center = new Vector3(0f, 1f, 0f);

            return npcRoot;
        }

        #endregion

        #region 燈光設置

        /// <summary>
        /// 設置場景燈光
        /// </summary>
        private void SetupLighting()
        {
            // 主方向光
            var mainLight = new GameObject("MainLight");
            var light = mainLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.8f);
            light.intensity = 1f;
            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 環境光設置
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.1f, 0.08f, 0.06f);

            // 霧效
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.1f, 0f, 0.2f);
            RenderSettings.fogStartDistance = 10f;
            RenderSettings.fogEndDistance = 30f;

            // 吧台聚光燈
            CreateSpotlight(new Vector3(-3f, 4f, -3f), new Color(1f, 0.8f, 0.6f), 15f);
            CreateSpotlight(new Vector3(0f, 4f, -3f), new Color(0.8f, 0.8f, 1f), 15f);
            CreateSpotlight(new Vector3(3f, 4f, -3f), new Color(1f, 0.8f, 0.6f), 15f);
        }

        /// <summary>
        /// 建立聚光燈
        /// </summary>
        private void CreateSpotlight(Vector3 position, Color color, float intensity)
        {
            var spotlightObj = new GameObject("Spotlight");
            spotlightObj.transform.position = position;
            spotlightObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            var light = spotlightObj.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = 10f;
            light.spotAngle = 60f;

            structureObjects.Add(spotlightObj);
        }

        #endregion

        #region 公開屬性

        public IReadOnlyList<Bottle> Bottles => bottles.AsReadOnly();
        public IReadOnlyList<Glass> Glasses => glasses.AsReadOnly();
        public Shaker Shaker => shaker;
        public IReadOnlyList<GameObject> NPCs => npcs.AsReadOnly();

        #endregion
    }
}
