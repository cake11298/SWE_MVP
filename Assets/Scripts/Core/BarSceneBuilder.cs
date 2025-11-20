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

            // 4. 建造裝飾
            BuildDecorations();

            // 5. 設置燈光
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
            marbleMaterial.SetFloat("_Glossiness", 0.3f); // 降低反射
            marbleMaterial.SetFloat("_Metallic", 0f);
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
            // 如果沒有資料庫，使用預設酒瓶
            if (liquorDatabase == null || liquorDatabase.liquors == null || liquorDatabase.liquors.Length == 0)
            {
                Debug.Log("BarSceneBuilder: No liquor database, creating default bottles");
                BuildDefaultBottles();
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
        /// 建立預設酒瓶 (當沒有資料庫時)
        /// </summary>
        private void BuildDefaultBottles()
        {
            // 預設酒類資料
            var defaultLiquors = new[]
            {
                new { name = "Gin", color = new Color(0.9f, 0.95f, 1f, 0.8f), type = "gin" },
                new { name = "Vodka", color = new Color(0.95f, 0.95f, 1f, 0.9f), type = "vodka" },
                new { name = "Whiskey", color = new Color(0.8f, 0.5f, 0.2f, 0.9f), type = "whiskey" },
                new { name = "Rum", color = new Color(0.7f, 0.4f, 0.2f, 0.85f), type = "rum" },
                new { name = "Tequila", color = new Color(1f, 0.95f, 0.8f, 0.85f), type = "tequila" },
                new { name = "Triple Sec", color = new Color(1f, 0.6f, 0.2f, 0.8f), type = "triple_sec" },
                new { name = "Vermouth", color = new Color(0.8f, 0.7f, 0.5f, 0.85f), type = "vermouth" },
                new { name = "Campari", color = new Color(0.9f, 0.2f, 0.2f, 0.9f), type = "campari" }
            };

            int liquorIndex = 0;

            // 每層架子放置酒瓶
            for (int shelfIdx = 0; shelfIdx < shelfHeights.Length; shelfIdx++)
            {
                float shelfY = shelfHeights[shelfIdx] + 0.25f;
                float startX = -4f;
                float spacing = 1f;

                for (int i = 0; i < bottlesPerShelf && liquorIndex < defaultLiquors.Length * 3; i++)
                {
                    var liquor = defaultLiquors[liquorIndex % defaultLiquors.Length];

                    // 建立酒瓶
                    var bottleObj = CreateDefaultBottle(liquor.name, liquor.color, liquor.type);
                    bottleObj.transform.position = new Vector3(startX + i * spacing, shelfY, -8f);

                    var bottle = bottleObj.GetComponent<Bottle>();
                    bottles.Add(bottle);

                    liquorIndex++;
                }
            }
        }

        /// <summary>
        /// 建立預設酒瓶物件
        /// </summary>
        private GameObject CreateDefaultBottle(string name, Color color, string type)
        {
            var bottleObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bottleObj.name = $"Bottle_{name}";
            bottleObj.transform.localScale = new Vector3(0.1f, 0.25f, 0.1f);

            // 設定顏色
            var renderer = bottleObj.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Glossiness", 0.8f);
            renderer.material = material;

            // 添加 Bottle 組件
            var bottle = bottleObj.AddComponent<Bottle>();
            // 使用反射或直接設定屬性來初始化
            bottle.gameObject.name = $"Bottle_{name}";

            // 添加 Rigidbody
            var rb = bottleObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // 設定 Layer (檢查是否存在)
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer != -1)
            {
                bottleObj.layer = interactableLayer;
            }

            return bottleObj;
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

            // 設定 Layer (檢查是否存在)
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer != -1)
            {
                bottleObj.layer = interactableLayer;
            }

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
            glassObj.transform.localScale = new Vector3(0.25f, 0.3f, 0.25f); // 放大酒杯

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

            // 設定 Layer (檢查是否存在)
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer != -1)
            {
                glassObj.layer = interactableLayer;
            }

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

            // 設定 Layer (檢查是否存在)
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer != -1)
            {
                shakerObj.layer = interactableLayer;
            }
        }

        #endregion

        #region 裝飾生成

        /// <summary>
        /// 建造裝飾物
        /// 參考: RetirementLounge.js
        /// </summary>
        private void BuildDecorations()
        {
            // 盆栽植物
            CreatePottedPlant(new Vector3(-8f, 0f, 8f), 1.2f);
            CreatePottedPlant(new Vector3(8f, 0f, 8f), 1.0f);
            CreatePottedPlant(new Vector3(-8f, 0f, -8f), 0.8f);
            CreatePottedPlant(new Vector3(8f, 0f, -8f), 1.1f);

            // 吧台角落小植物
            CreateSmallPlant(new Vector3(-5f, 1.15f, -2.5f));
            CreateSmallPlant(new Vector3(5f, 1.15f, -2.5f));

            // 牆上裝飾畫
            CreateWallArt(new Vector3(-4f, 3f, -12.3f), new Color(0.8f, 0.6f, 0.4f));
            CreateWallArt(new Vector3(4f, 3f, -12.3f), new Color(0.4f, 0.6f, 0.8f));

            // 酒吧招牌
            CreateBarSign();

            // 地毯
            CreateRug(new Vector3(0f, 0.01f, 3f), new Color(0.5f, 0.1f, 0.1f));
        }

        /// <summary>
        /// 建立盆栽植物
        /// </summary>
        private void CreatePottedPlant(Vector3 position, float scale)
        {
            var plantRoot = new GameObject("PottedPlant");
            plantRoot.transform.position = position;

            // 花盆
            var pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "Pot";
            pot.transform.SetParent(plantRoot.transform);
            pot.transform.localPosition = new Vector3(0f, 0.3f * scale, 0f);
            pot.transform.localScale = new Vector3(0.4f * scale, 0.3f * scale, 0.4f * scale);

            var potMaterial = new Material(Shader.Find("Standard"));
            potMaterial.color = new Color(0.6f, 0.4f, 0.3f); // 陶土色
            potMaterial.SetFloat("_Glossiness", 0.2f);
            pot.GetComponent<Renderer>().material = potMaterial;

            // 植物葉子 (用多個球體模擬)
            var leafMaterial = new Material(Shader.Find("Standard"));
            leafMaterial.color = new Color(0.2f, 0.5f, 0.2f); // 綠色
            leafMaterial.SetFloat("_Glossiness", 0.3f);

            for (int i = 0; i < 5; i++)
            {
                var leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaf.name = $"Leaf_{i}";
                leaf.transform.SetParent(plantRoot.transform);
                float angle = i * 72f * Mathf.Deg2Rad;
                float radius = 0.25f * scale;
                leaf.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0.8f * scale,
                    Mathf.Sin(angle) * radius
                );
                leaf.transform.localScale = new Vector3(0.3f * scale, 0.4f * scale, 0.3f * scale);
                leaf.GetComponent<Renderer>().material = leafMaterial;

                // 移除碰撞體
                Destroy(leaf.GetComponent<Collider>());
            }

            // 中間的葉子
            var centerLeaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            centerLeaf.name = "CenterLeaf";
            centerLeaf.transform.SetParent(plantRoot.transform);
            centerLeaf.transform.localPosition = new Vector3(0f, 1f * scale, 0f);
            centerLeaf.transform.localScale = new Vector3(0.35f * scale, 0.5f * scale, 0.35f * scale);
            centerLeaf.GetComponent<Renderer>().material = leafMaterial;
            Destroy(centerLeaf.GetComponent<Collider>());

            structureObjects.Add(plantRoot);
        }

        /// <summary>
        /// 建立小型植物 (吧台上)
        /// </summary>
        private void CreateSmallPlant(Vector3 position)
        {
            var plantRoot = new GameObject("SmallPlant");
            plantRoot.transform.position = position;

            // 小花盆
            var pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "SmallPot";
            pot.transform.SetParent(plantRoot.transform);
            pot.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            pot.transform.localScale = new Vector3(0.1f, 0.05f, 0.1f);

            var potMaterial = new Material(Shader.Find("Standard"));
            potMaterial.color = new Color(0.9f, 0.9f, 0.9f); // 白色陶瓷
            potMaterial.SetFloat("_Glossiness", 0.5f);
            pot.GetComponent<Renderer>().material = potMaterial;

            // 多肉植物
            var plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plant.name = "Succulent";
            plant.transform.SetParent(plantRoot.transform);
            plant.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            plant.transform.localScale = new Vector3(0.08f, 0.06f, 0.08f);

            var plantMaterial = new Material(Shader.Find("Standard"));
            plantMaterial.color = new Color(0.3f, 0.6f, 0.3f);
            plantMaterial.SetFloat("_Glossiness", 0.4f);
            plant.GetComponent<Renderer>().material = plantMaterial;

            Destroy(plant.GetComponent<Collider>());
            structureObjects.Add(plantRoot);
        }

        /// <summary>
        /// 建立牆上藝術品
        /// </summary>
        private void CreateWallArt(Vector3 position, Color artColor)
        {
            var artRoot = new GameObject("WallArt");
            artRoot.transform.position = position;

            // 畫框
            var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.SetParent(artRoot.transform);
            frame.transform.localPosition = Vector3.zero;
            frame.transform.localScale = new Vector3(1.2f, 0.8f, 0.05f);

            var frameMaterial = new Material(Shader.Find("Standard"));
            frameMaterial.color = new Color(0.3f, 0.2f, 0.1f); // 深棕色
            frameMaterial.SetFloat("_Glossiness", 0.6f);
            frame.GetComponent<Renderer>().material = frameMaterial;

            // 畫布
            var canvas = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canvas.name = "Canvas";
            canvas.transform.SetParent(artRoot.transform);
            canvas.transform.localPosition = new Vector3(0f, 0f, 0.03f);
            canvas.transform.localScale = new Vector3(1f, 0.6f, 0.02f);

            var canvasMaterial = new Material(Shader.Find("Standard"));
            canvasMaterial.color = artColor;
            canvasMaterial.SetFloat("_Glossiness", 0.1f);
            canvas.GetComponent<Renderer>().material = canvasMaterial;

            Destroy(canvas.GetComponent<Collider>());
            structureObjects.Add(artRoot);
        }

        /// <summary>
        /// 建立酒吧招牌
        /// </summary>
        private void CreateBarSign()
        {
            var signRoot = new GameObject("BarSign");
            signRoot.transform.position = new Vector3(0f, 5f, -8.3f);

            // 招牌背板
            var signBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBoard.name = "SignBoard";
            signBoard.transform.SetParent(signRoot.transform);
            signBoard.transform.localPosition = Vector3.zero;
            signBoard.transform.localScale = new Vector3(3f, 0.8f, 0.1f);

            var boardMaterial = new Material(Shader.Find("Standard"));
            boardMaterial.color = new Color(0.1f, 0.1f, 0.1f);
            boardMaterial.SetFloat("_Glossiness", 0.8f);
            signBoard.GetComponent<Renderer>().material = boardMaterial;

            // 霓虹燈效果
            var neonMaterial = new Material(Shader.Find("Standard"));
            neonMaterial.color = new Color(1f, 0.4f, 0.7f); // 粉紅霓虹
            neonMaterial.EnableKeyword("_EMISSION");
            neonMaterial.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.7f) * 2f);
            neonMaterial.SetFloat("_Glossiness", 0.9f);

            // 用小方塊模擬文字 "BAR"
            float[] letterX = { -0.8f, 0f, 0.8f };
            foreach (float x in letterX)
            {
                var letter = GameObject.CreatePrimitive(PrimitiveType.Cube);
                letter.name = "NeonLetter";
                letter.transform.SetParent(signRoot.transform);
                letter.transform.localPosition = new Vector3(x, 0f, 0.06f);
                letter.transform.localScale = new Vector3(0.4f, 0.4f, 0.02f);
                letter.GetComponent<Renderer>().material = neonMaterial;
                Destroy(letter.GetComponent<Collider>());
            }

            Destroy(signBoard.GetComponent<Collider>());
            structureObjects.Add(signRoot);
        }

        /// <summary>
        /// 建立地毯
        /// </summary>
        private void CreateRug(Vector3 position, Color rugColor)
        {
            var rug = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rug.name = "Rug";
            rug.transform.position = position;
            rug.transform.localScale = new Vector3(4f, 0.02f, 3f);

            var rugMaterial = new Material(Shader.Find("Standard"));
            rugMaterial.color = rugColor;
            rugMaterial.SetFloat("_Glossiness", 0.1f);
            rug.GetComponent<Renderer>().material = rugMaterial;

            Destroy(rug.GetComponent<Collider>());
            structureObjects.Add(rug);
        }

        #endregion

        #region NPC 生成

        /// <summary>
        /// 建造 NPC
        /// 參考: NPCManager.js createNPCs()
        /// </summary>
        private void BuildNPCs()
        {
            // Gustave - 調酒社創始社長
            var gustave = CreateNPCWithDetails(
                "Gustave",
                "Founder President",
                new Vector3(2f, 0f, -5f),
                HexToColor(0x0066cc), // 藍色上衣
                HexToColor(0x1a1a1a), // 黑色褲子
                0f,
                new string[] {
                    "Hi! I'm Gustave Yang, founder of NCU Molecular Mixology Club!",
                    "Molecular mixology is the fusion of science and art",
                    "Freedom is priceless",
                    "Some things can't be solved by morning coffee, but forgotten by evening cocktails"
                }
            );
            npcs.Add(gustave);

            // Seaton - 調酒社共同創辦人
            var seaton = CreateNPCWithDetails(
                "Seaton",
                "Co-founder",
                new Vector3(-2f, 0f, -5f),
                HexToColor(0xcc0066), // 粉紅色上衣
                HexToColor(0x333333), // 深灰褲子
                0f,
                new string[] {
                    "Hello! I'm Seaton, also a co-founder of the club!",
                    "My favorite is Japanese whiskey",
                    "I love the classic Old Fashioned"
                }
            );
            npcs.Add(seaton);

            // 正安 - 公關兼副社長
            var zhengan = CreateNPCWithDetails(
                "Zheng An",
                "PR & Vice President",
                new Vector3(9f, 0f, 1f),
                HexToColor(0xffb6c1), // 淺粉紅色
                HexToColor(0x4169e1), // 藍色牛仔褲
                -90f,
                new string[] {
                    "Hi! I'm Zheng An, in charge of event planning~",
                    "Want to join our molecular mixology workshop?",
                    "I love planning themed parties!",
                    "Follow our social media!"
                },
                true // 女性
            );
            npcs.Add(zhengan);

            // 瑜柔 - 學術研究長
            var yurou = CreateNPCWithDetails(
                "Yu Rou",
                "Academic Director",
                new Vector3(9f, 0f, 3f),
                HexToColor(0x90ee90), // 淺綠色
                HexToColor(0x2f4f4f), // 深灰色
                -90f,
                new string[] {
                    "I'm Yu Rou, in charge of academic research",
                    "Molecular mixology has deep chemistry principles",
                    "Theory and practice are equally important"
                },
                true // 女性
            );
            npcs.Add(yurou);

            // 恩若 - 美宣長
            var enruo = CreateNPCWithDetails(
                "En Ruo",
                "Art Director",
                new Vector3(9f, 0f, -1f),
                HexToColor(0xffd700), // 金黃色
                HexToColor(0x8b4513), // 棕色
                -90f,
                new string[] {
                    "Hi hi! I'm En Ruo, in charge of art design",
                    "Have you followed our social media? Like and share!",
                    "I'm best at building friendly relationships"
                },
                true // 女性
            );
            npcs.Add(enruo);

            // 旻偉 - 器材長
            var minwei = CreateNPCWithDetails(
                "Min Wei",
                "Equipment Manager",
                new Vector3(9f, 0f, 5f),
                HexToColor(0x708090), // 灰藍色
                HexToColor(0x556b2f), // 軍綠色
                -90f,
                new string[] {
                    "I'm Min Wei, in charge of equipment management",
                    "All bar tools are maintained by me",
                    "Budget planning and procurement are my job"
                }
            );
            npcs.Add(minwei);
        }

        /// <summary>
        /// Hex 顏色轉 Unity Color
        /// </summary>
        private Color HexToColor(int hex)
        {
            float r = ((hex >> 16) & 0xFF) / 255f;
            float g = ((hex >> 8) & 0xFF) / 255f;
            float b = (hex & 0xFF) / 255f;
            return new Color(r, g, b);
        }

        /// <summary>
        /// 建立詳細 NPC
        /// </summary>
        private GameObject CreateNPCWithDetails(string npcName, string role, Vector3 position,
            Color shirtColor, Color pantsColor, float rotationY, string[] dialogues, bool isFemale = false)
        {
            var npcRoot = new GameObject($"NPC_{npcName}");
            npcRoot.transform.position = position;
            npcRoot.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

            // 身體比例根據性別調整
            float shoulderWidth = isFemale ? 0.35f : 0.4f;

            // 上衣
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(npcRoot.transform);
            body.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            body.transform.localScale = isFemale ?
                new Vector3(0.35f, 0.45f, 0.35f) :
                new Vector3(0.4f, 0.5f, 0.4f);

            var bodyMaterial = new Material(Shader.Find("Standard"));
            bodyMaterial.color = shirtColor;
            body.GetComponent<Renderer>().material = bodyMaterial;

            // 褲子
            var pants = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            pants.name = "Pants";
            pants.transform.SetParent(npcRoot.transform);
            pants.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            pants.transform.localScale = new Vector3(0.35f, 0.5f, 0.35f);

            var pantsMaterial = new Material(Shader.Find("Standard"));
            pantsMaterial.color = pantsColor;
            pants.GetComponent<Renderer>().material = pantsMaterial;

            // 頭部
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(npcRoot.transform);
            head.transform.localPosition = new Vector3(0f, 1.9f, 0f);
            head.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

            var headMaterial = new Material(Shader.Find("Standard"));
            headMaterial.color = new Color(0.99f, 0.74f, 0.71f); // 膚色 0xfdbcb4
            head.GetComponent<Renderer>().material = headMaterial;

            // 頭髮
            var hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hair.name = "Hair";
            hair.transform.SetParent(npcRoot.transform);
            hair.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            hair.transform.localScale = isFemale ?
                new Vector3(0.38f, 0.25f, 0.38f) : // 女性長髮
                new Vector3(0.36f, 0.2f, 0.36f);   // 男性短髮

            var hairMaterial = new Material(Shader.Find("Standard"));
            hairMaterial.color = new Color(0.2f, 0.2f, 0.2f); // 黑髮
            hair.GetComponent<Renderer>().material = hairMaterial;

            // 左手臂
            var leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leftArm.name = "LeftArm";
            leftArm.transform.SetParent(npcRoot.transform);
            leftArm.transform.localPosition = new Vector3(-shoulderWidth - 0.15f, 1.2f, 0f);
            leftArm.transform.localScale = new Vector3(0.12f, 0.4f, 0.12f);

            var armMaterial = new Material(Shader.Find("Standard"));
            armMaterial.color = new Color(0.99f, 0.74f, 0.71f); // 膚色
            leftArm.GetComponent<Renderer>().material = armMaterial;

            // 右手臂
            var rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rightArm.name = "RightArm";
            rightArm.transform.SetParent(npcRoot.transform);
            rightArm.transform.localPosition = new Vector3(shoulderWidth + 0.15f, 1.2f, 0f);
            rightArm.transform.localScale = new Vector3(0.12f, 0.4f, 0.12f);
            rightArm.GetComponent<Renderer>().material = armMaterial;

            // 添加 NPCController
            var controller = npcRoot.AddComponent<NPC.NPCController>();
            controller.InitializeNPC(npcName, role, dialogues);

            // 添加碰撞體
            var collider = npcRoot.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.4f;
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
            // 主方向光 - 降低強度
            var mainLight = new GameObject("MainLight");
            var light = mainLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.8f);
            light.intensity = 0.5f; // 降低主光源強度
            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 環境光設置 - 略微提高以平衡降低的主光源
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.15f, 0.12f, 0.1f);

            // 霧效
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.1f, 0f, 0.2f);
            RenderSettings.fogStartDistance = 10f;
            RenderSettings.fogEndDistance = 30f;

            // 吧台聚光燈 - 降低強度
            CreateSpotlight(new Vector3(-3f, 4f, -3f), new Color(1f, 0.8f, 0.6f), 5f);
            CreateSpotlight(new Vector3(0f, 4f, -3f), new Color(0.8f, 0.8f, 1f), 5f);
            CreateSpotlight(new Vector3(3f, 4f, -3f), new Color(1f, 0.8f, 0.6f), 5f);
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
