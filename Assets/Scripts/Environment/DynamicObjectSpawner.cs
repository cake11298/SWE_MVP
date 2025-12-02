using UnityEngine;
using System.Collections.Generic;
using BarSimulator.Data;
using BarSimulator.Objects;
using BarSimulator.NPC;

namespace BarSimulator.Environment
{
    /// <summary>
    /// 動態物件生成器
    /// 負責在運行時生成和管理動態物件（酒瓶、玻璃杯、調酒器等）
    /// 與靜態場景結構分離，符合混合架構設計
    /// </summary>
    public class DynamicObjectSpawner : MonoBehaviour
    {
        #region 單例

        private static DynamicObjectSpawner instance;
        public static DynamicObjectSpawner Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("資料庫")]
        [Tooltip("酒類資料庫")]
        [SerializeField] private LiquorDatabase liquorDatabase;

        [Header("生成位置")]
        [Tooltip("酒瓶架子變換（用於定位生成位置）")]
        [SerializeField] private Transform bottleShelfTransform;

        [Tooltip("玻璃杯站台變換")]
        [SerializeField] private Transform glassStationTransform;

        [Tooltip("調酒器放置位置")]
        [SerializeField] private Transform shakerSpawnPoint;

        [Tooltip("NPC 生成位置列表")]
        [SerializeField] private Transform[] npcSpawnPoints;

        [Header("生成設定")]
        [Tooltip("每層架子的酒瓶數量")]
        [SerializeField] private int bottlesPerShelf = 8;

        [Tooltip("架子層數")]
        [SerializeField] private int shelfLevels = 3;

        [Tooltip("玻璃杯數量")]
        [SerializeField] private int glassCount = 6;

        [Tooltip("自動在 Start 時生成")]
        [SerializeField] private bool spawnOnStart = true;

        [Header("預製物（可選）")]
        [Tooltip("酒瓶預製物（留空則動態創建）")]
        [SerializeField] private GameObject bottlePrefab;

        [Tooltip("玻璃杯預製物（留空則動態創建）")]
        [SerializeField] private GameObject glassPrefab;

        [Tooltip("調酒器預製物（留空則動態創建）")]
        [SerializeField] private GameObject shakerPrefab;

        [Tooltip("NPC 預製物（留空則動態創建）")]
        [SerializeField] private GameObject npcPrefab;

        #endregion

        #region 私有欄位

        private List<Bottle> spawnedBottles = new List<Bottle>();
        private List<Glass> spawnedGlasses = new List<Glass>();
        private Shaker spawnedShaker;
        private List<GameObject> spawnedNPCs = new List<GameObject>();

        // 架子高度偏移
        private float[] shelfHeightOffsets = { 0f, 1.2f, 2.4f };

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
            // 確保資料庫存在
            if (liquorDatabase == null)
            {
                liquorDatabase = ScriptableObject.CreateInstance<LiquorDatabase>();
                liquorDatabase.InitializeDefaults();
                Debug.Log("DynamicObjectSpawner: 創建運行時 LiquorDatabase");
            }

            // 自動尋找架子位置（如果未設置）
            if (bottleShelfTransform == null)
            {
                bottleShelfTransform = FindShelfTransform();
            }

            if (glassStationTransform == null)
            {
                glassStationTransform = FindGlassStationTransform();
            }

            if (spawnOnStart)
            {
                SpawnAllObjects();
            }
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成所有動態物件
        /// </summary>
        public void SpawnAllObjects()
        {
            Debug.Log("DynamicObjectSpawner: 開始生成動態物件...");

            SpawnBottles();
            SpawnGlasses();
            SpawnShaker();
            SpawnNPCs();

            Debug.Log($"DynamicObjectSpawner: 完成 - {spawnedBottles.Count} 酒瓶, {spawnedGlasses.Count} 玻璃杯, {spawnedNPCs.Count} NPCs");
        }

        /// <summary>
        /// 生成酒瓶
        /// </summary>
        public void SpawnBottles()
        {
            if (bottleShelfTransform == null)
            {
                Debug.LogWarning("DynamicObjectSpawner: 找不到酒瓶架子位置！");
                return;
            }

            Vector3 shelfBasePos = bottleShelfTransform.position;
            int bottleIndex = 0;

            // 為每層架子生成酒瓶
            for (int level = 0; level < shelfLevels; level++)
            {
                float yOffset = shelfHeightOffsets[Mathf.Min(level, shelfHeightOffsets.Length - 1)];

                for (int i = 0; i < bottlesPerShelf; i++)
                {
                    if (bottleIndex >= liquorDatabase.liquors.Count)
                    {
                        Debug.LogWarning($"DynamicObjectSpawner: 酒類數量不足，需要 {bottlesPerShelf * shelfLevels}，只有 {liquorDatabase.liquors.Count}");
                        break;
                    }

                    // 計算位置（橫向排列）
                    float xOffset = (i - bottlesPerShelf / 2f + 0.5f) * 0.5f;
                    Vector3 spawnPos = shelfBasePos + new Vector3(xOffset, yOffset, 0f);

                    // 生成酒瓶
                    LiquorData liquor = liquorDatabase.liquors[bottleIndex];
                    GameObject bottleObj = CreateBottle(spawnPos, liquor);
                    bottleObj.name = $"Bottle_{liquor.name}";

                    Bottle bottle = bottleObj.GetComponent<Bottle>();
                    if (bottle != null)
                    {
                        spawnedBottles.Add(bottle);
                    }

                    bottleIndex++;
                }

                if (bottleIndex >= liquorDatabase.liquors.Count) break;
            }

            Debug.Log($"DynamicObjectSpawner: 生成了 {spawnedBottles.Count} 個酒瓶");
        }

        /// <summary>
        /// 生成玻璃杯
        /// </summary>
        public void SpawnGlasses()
        {
            if (glassStationTransform == null)
            {
                Debug.LogWarning("DynamicObjectSpawner: 找不到玻璃杯站台位置！");
                return;
            }

            Vector3 stationPos = glassStationTransform.position;

            for (int i = 0; i < glassCount; i++)
            {
                // 橫向排列玻璃杯
                float xOffset = (i - glassCount / 2f + 0.5f) * 0.4f;
                Vector3 spawnPos = stationPos + new Vector3(xOffset, 0.15f, 0f);

                GameObject glassObj = CreateGlass(spawnPos);
                glassObj.name = $"Glass_{i + 1}";

                Glass glass = glassObj.GetComponent<Glass>();
                if (glass != null)
                {
                    spawnedGlasses.Add(glass);
                }
            }

            Debug.Log($"DynamicObjectSpawner: 生成了 {spawnedGlasses.Count} 個玻璃杯");
        }

        /// <summary>
        /// 生成調酒器
        /// </summary>
        public void SpawnShaker()
        {
            Vector3 spawnPos = shakerSpawnPoint != null
                ? shakerSpawnPoint.position
                : glassStationTransform.position + new Vector3(1.5f, 0.2f, 0f);

            GameObject shakerObj = CreateShaker(spawnPos);
            shakerObj.name = "Shaker";

            spawnedShaker = shakerObj.GetComponent<Shaker>();
            Debug.Log("DynamicObjectSpawner: 生成了調酒器");
        }

        /// <summary>
        /// 生成 NPCs
        /// </summary>
        public void SpawnNPCs()
        {
            if (npcSpawnPoints == null || npcSpawnPoints.Length == 0)
            {
                Debug.LogWarning("DynamicObjectSpawner: 沒有設置 NPC 生成位置");
                return;
            }

            // NPC 資料
            var npcData = GetDefaultNPCData();

            for (int i = 0; i < Mathf.Min(npcSpawnPoints.Length, npcData.Count); i++)
            {
                if (npcSpawnPoints[i] == null) continue;

                GameObject npcObj = CreateNPC(npcSpawnPoints[i].position, npcData[i]);
                npcObj.name = $"NPC_{npcData[i].name}";
                spawnedNPCs.Add(npcObj);
            }

            Debug.Log($"DynamicObjectSpawner: 生成了 {spawnedNPCs.Count} 個 NPCs");
        }

        #endregion

        #region 物件創建

        private GameObject CreateBottle(Vector3 position, LiquorData liquor)
        {
            GameObject bottleObj;

            if (bottlePrefab != null)
            {
                bottleObj = Instantiate(bottlePrefab, position, Quaternion.identity);
            }
            else
            {
                // 動態創建
                bottleObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bottleObj.transform.position = position;
                bottleObj.transform.localScale = new Vector3(0.1f, 0.25f, 0.1f);

                // 材質
                var renderer = bottleObj.GetComponent<Renderer>();
                var material = new Material(Shader.Find("Standard"));
                material.color = liquor.color;
                material.SetFloat("_Glossiness", 0.8f);
                material.SetFloat("_Metallic", 0.2f);
                renderer.material = material;

                // 組件
                var bottle = bottleObj.AddComponent<Bottle>();
                bottle.Initialize(liquor);

                var rb = bottleObj.AddComponent<Rigidbody>();
                rb.mass = 0.5f;
                rb.isKinematic = true;

                bottleObj.layer = LayerMask.NameToLayer("Interactable");
            }

            bottleObj.transform.SetParent(transform);
            return bottleObj;
        }

        private GameObject CreateGlass(Vector3 position)
        {
            GameObject glassObj;

            if (glassPrefab != null)
            {
                glassObj = Instantiate(glassPrefab, position, Quaternion.identity);
            }
            else
            {
                // 動態創建
                glassObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                glassObj.transform.position = position;
                glassObj.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);

                // 透明材質
                var renderer = glassObj.GetComponent<Renderer>();
                var material = new Material(Shader.Find("Standard"));
                material.color = new Color(0.8f, 0.9f, 1f, 0.3f);
                material.SetFloat("_Mode", 3); // Transparent
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                renderer.material = material;

                // 組件
                glassObj.AddComponent<Glass>();

                var rb = glassObj.AddComponent<Rigidbody>();
                rb.mass = 0.2f;
                rb.isKinematic = true;
            }

            glassObj.transform.SetParent(transform);
            return glassObj;
        }

        private GameObject CreateShaker(Vector3 position)
        {
            GameObject shakerObj;

            if (shakerPrefab != null)
            {
                shakerObj = Instantiate(shakerPrefab, position, Quaternion.identity);
            }
            else
            {
                // 動態創建
                shakerObj = new GameObject("Shaker");
                shakerObj.transform.position = position;

                // 底部金屬部分
                var bottom = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bottom.name = "Bottom";
                bottom.transform.SetParent(shakerObj.transform);
                bottom.transform.localPosition = Vector3.zero;
                bottom.transform.localScale = new Vector3(0.15f, 0.2f, 0.15f);

                var bottomMat = new Material(Shader.Find("Standard"));
                bottomMat.color = new Color(0.7f, 0.7f, 0.8f);
                bottomMat.SetFloat("_Metallic", 0.8f);
                bottom.GetComponent<Renderer>().material = bottomMat;

                // 組件
                shakerObj.AddComponent<Shaker>();
                shakerObj.AddComponent<CapsuleCollider>();

                var rb = shakerObj.AddComponent<Rigidbody>();
                rb.mass = 0.3f;
                rb.isKinematic = true;
            }

            shakerObj.transform.SetParent(transform);
            return shakerObj;
        }

        private GameObject CreateNPC(Vector3 position, NPCData data)
        {
            GameObject npcObj;

            if (npcPrefab != null)
            {
                npcObj = Instantiate(npcPrefab, position, Quaternion.identity);
            }
            else
            {
                // 動態創建簡單 NPC
                npcObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                npcObj.transform.position = position;
                npcObj.transform.localScale = new Vector3(0.5f, 1f, 0.5f);

                // 組件
                npcObj.AddComponent<InteractableNPC>();

                // 設置 NPC 資料（需要 NPCController 組件）
                var npcController = npcObj.GetComponent<NPCController>();
                if (npcController != null)
                {
                    // 設置 NPC 資料
                }
            }

            npcObj.transform.SetParent(transform);
            return npcObj;
        }

        #endregion

        #region 輔助方法

        private Transform FindShelfTransform()
        {
            var structures = FindObjectsByType<StaticBarStructure>(FindObjectsSortMode.None);
            foreach (var structure in structures)
            {
                if (structure.IsBottleShelf)
                {
                    return structure.transform;
                }
            }

            Debug.LogWarning("DynamicObjectSpawner: 找不到標記為 BottleShelf 的靜態結構");
            return null;
        }

        private Transform FindGlassStationTransform()
        {
            var structures = FindObjectsByType<StaticBarStructure>(FindObjectsSortMode.None);
            foreach (var structure in structures)
            {
                if (structure.IsGlassStation)
                {
                    return structure.transform;
                }
            }

            Debug.LogWarning("DynamicObjectSpawner: 找不到標記為 GlassStation 的靜態結構");
            return null;
        }

        private List<NPCData> GetDefaultNPCData()
        {
            return new List<NPCData>
            {
                new NPCData { name = "Gustave", role = "調酒師", dialogue = "歡迎來到酒吧！" },
                new NPCData { name = "Seaton", role = "常客", dialogue = "今天天氣不錯。" },
                new NPCData { name = "ZhengAn", role = "商人", dialogue = "我有些好貨要賣。" },
                new NPCData { name = "YuRou", role = "學生", dialogue = "這裡的氣氛真好。" },
                new NPCData { name = "EnRuo", role = "藝術家", dialogue = "靈感來了！" },
                new NPCData { name = "MinWei", role = "工程師", dialogue = "來杯馬丁尼。" }
            };
        }

        #endregion

        #region 公開方法

        /// <summary>
        /// 清除所有生成的物件
        /// </summary>
        public void ClearAllSpawnedObjects()
        {
            foreach (var bottle in spawnedBottles)
            {
                if (bottle != null) Destroy(bottle.gameObject);
            }
            spawnedBottles.Clear();

            foreach (var glass in spawnedGlasses)
            {
                if (glass != null) Destroy(glass.gameObject);
            }
            spawnedGlasses.Clear();

            if (spawnedShaker != null)
            {
                Destroy(spawnedShaker.gameObject);
                spawnedShaker = null;
            }

            foreach (var npc in spawnedNPCs)
            {
                if (npc != null) Destroy(npc);
            }
            spawnedNPCs.Clear();

            Debug.Log("DynamicObjectSpawner: 已清除所有生成的物件");
        }

        /// <summary>
        /// 重新生成所有物件
        /// </summary>
        [ContextMenu("Respawn All Objects")]
        public void RespawnAllObjects()
        {
            ClearAllSpawnedObjects();
            SpawnAllObjects();
        }

        #endregion
    }

    /// <summary>
    /// NPC 資料結構
    /// </summary>
    [System.Serializable]
    public class NPCData
    {
        public string name;
        public string role;
        public string dialogue;
    }
}
