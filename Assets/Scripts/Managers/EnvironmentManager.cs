using System.Collections.Generic;
using UnityEngine;
using BarSimulator.Data;
using BarSimulator.Objects;

namespace BarSimulator.Managers
{
    /// <summary>
    /// 環境管理器 - 管理吧台環境物件
    /// 參考: index.js 場景物件設置
    /// </summary>
    public class EnvironmentManager : MonoBehaviour
    {
        #region 單例

        private static EnvironmentManager instance;
        public static EnvironmentManager Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("資料庫")]
        [Tooltip("酒類資料庫")]
        [SerializeField] private LiquorDatabase liquorDatabase;

        [Header("預製物")]
        [Tooltip("酒瓶預製物")]
        [SerializeField] private GameObject bottlePrefab;

        [Tooltip("酒杯預製物")]
        [SerializeField] private GameObject glassPrefab;

        [Tooltip("搖酒器預製物")]
        [SerializeField] private GameObject shakerPrefab;

        [Header("生成位置")]
        [Tooltip("酒瓶架位置")]
        [SerializeField] private Transform bottleShelfTransform;

        [Tooltip("酒杯架位置")]
        [SerializeField] private Transform glassShelfTransform;

        [Tooltip("搖酒器位置")]
        [SerializeField] private Transform shakerSpawnPoint;

        [Header("生成設定")]
        [Tooltip("酒瓶間距")]
        [SerializeField] private float bottleSpacing = 0.15f;

        [Tooltip("每排酒瓶數量")]
        [SerializeField] private int bottlesPerRow = 8;

        [Tooltip("排間距")]
        [SerializeField] private float rowSpacing = 0.2f;

        [Tooltip("酒杯間距")]
        [SerializeField] private float glassSpacing = 0.12f;

        [Tooltip("生成酒杯數量")]
        [SerializeField] private int glassCount = 6;

        #endregion

        #region 私有欄位

        // 生成的物件
        private List<Bottle> spawnedBottles = new List<Bottle>();
        private List<Glass> spawnedGlasses = new List<Glass>();
        private Shaker spawnedShaker;

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
            // 載入資料庫 - 優先從 CocktailSystem 取得
            if (liquorDatabase == null)
            {
                var cocktailSystem = BarSimulator.Systems.CocktailSystem.Instance;
                if (cocktailSystem != null && cocktailSystem.LiquorDatabase != null)
                {
                    liquorDatabase = cocktailSystem.LiquorDatabase;
                }
                else
                {
                    // 建立執行期資料庫
                    liquorDatabase = ScriptableObject.CreateInstance<LiquorDatabase>();
                    liquorDatabase.InitializeDefaults();
                    Debug.Log("EnvironmentManager: Created runtime LiquorDatabase");
                }
            }

            // 生成環境物件
            SpawnEnvironmentObjects();
        }

        #endregion

        #region 物件生成

        /// <summary>
        /// 生成所有環境物件
        /// </summary>
        public void SpawnEnvironmentObjects()
        {
            SpawnBottles();
            SpawnGlasses();
            SpawnShaker();

            Debug.Log($"EnvironmentManager: Spawned {spawnedBottles.Count} bottles, {spawnedGlasses.Count} glasses, 1 shaker");
        }

        /// <summary>
        /// 生成酒瓶
        /// </summary>
        private void SpawnBottles()
        {
            if (liquorDatabase == null || liquorDatabase.liquors == null) return;
            if (bottlePrefab == null && bottleShelfTransform == null) return;

            Vector3 basePosition = bottleShelfTransform != null ?
                bottleShelfTransform.position : Vector3.zero;

            int index = 0;
            foreach (var liquorData in liquorDatabase.liquors)
            {
                // 計算位置
                int row = index / bottlesPerRow;
                int col = index % bottlesPerRow;

                Vector3 position = basePosition + new Vector3(
                    col * bottleSpacing,
                    row * rowSpacing,
                    0f
                );

                // 生成酒瓶
                Bottle bottle = SpawnBottle(liquorData, position);
                if (bottle != null)
                {
                    spawnedBottles.Add(bottle);
                }

                index++;
            }
        }

        /// <summary>
        /// 生成單一酒瓶
        /// </summary>
        private Bottle SpawnBottle(LiquorData liquorData, Vector3 position)
        {
            GameObject bottleObject;

            if (bottlePrefab != null)
            {
                bottleObject = Instantiate(bottlePrefab, position, Quaternion.identity);
            }
            else
            {
                // 沒有預製物時建立簡單酒瓶
                bottleObject = CreateDefaultBottle(liquorData, position);
            }

            var bottle = bottleObject.GetComponent<Bottle>();
            if (bottle == null)
            {
                bottle = bottleObject.AddComponent<Bottle>();
            }

            bottle.Initialize(liquorData);
            return bottle;
        }

        /// <summary>
        /// 建立預設酒瓶
        /// </summary>
        private GameObject CreateDefaultBottle(LiquorData liquorData, Vector3 position)
        {
            var bottle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bottle.name = $"Bottle_{liquorData.id}";
            bottle.transform.position = position;
            bottle.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);

            // 設定顏色
            var renderer = bottle.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = liquorData.color;
            }

            return bottle;
        }

        /// <summary>
        /// 生成酒杯
        /// </summary>
        private void SpawnGlasses()
        {
            if (glassPrefab == null && glassShelfTransform == null) return;

            Vector3 basePosition = glassShelfTransform != null ?
                glassShelfTransform.position : new Vector3(0f, 1f, 0f);

            for (int i = 0; i < glassCount; i++)
            {
                Vector3 position = basePosition + new Vector3(i * glassSpacing, 0f, 0f);

                Glass glass = SpawnGlass(position);
                if (glass != null)
                {
                    spawnedGlasses.Add(glass);
                }
            }
        }

        /// <summary>
        /// 生成單一酒杯
        /// </summary>
        private Glass SpawnGlass(Vector3 position)
        {
            GameObject glassObject;

            if (glassPrefab != null)
            {
                glassObject = Instantiate(glassPrefab, position, Quaternion.identity);
            }
            else
            {
                // 沒有預製物時建立簡單酒杯
                glassObject = CreateDefaultGlass(position);
            }

            var glass = glassObject.GetComponent<Glass>();
            if (glass == null)
            {
                glass = glassObject.AddComponent<Glass>();
            }

            return glass;
        }

        /// <summary>
        /// 建立預設酒杯
        /// </summary>
        private GameObject CreateDefaultGlass(Vector3 position)
        {
            var glass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glass.name = "Glass";
            glass.transform.position = position;
            glass.transform.localScale = new Vector3(0.06f, 0.08f, 0.06f);

            // 設定透明材質
            var renderer = glass.GetComponent<Renderer>();
            if (renderer != null)
            {
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
                renderer.material = material;
            }

            return glass;
        }

        /// <summary>
        /// 生成搖酒器
        /// </summary>
        private void SpawnShaker()
        {
            Vector3 position = shakerSpawnPoint != null ?
                shakerSpawnPoint.position : new Vector3(0.5f, 1f, 0f);

            GameObject shakerObject;

            if (shakerPrefab != null)
            {
                shakerObject = Instantiate(shakerPrefab, position, Quaternion.identity);
            }
            else
            {
                // 沒有預製物時建立簡單搖酒器
                shakerObject = CreateDefaultShaker(position);
            }

            spawnedShaker = shakerObject.GetComponent<Shaker>();
            if (spawnedShaker == null)
            {
                spawnedShaker = shakerObject.AddComponent<Shaker>();
            }
        }

        /// <summary>
        /// 建立預設搖酒器
        /// </summary>
        private GameObject CreateDefaultShaker(Vector3 position)
        {
            var shaker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            shaker.name = "Shaker";
            shaker.transform.position = position;
            shaker.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);

            // 設定金屬材質
            var renderer = shaker.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.SetFloat("_Metallic", 0.8f);
                material.SetFloat("_Glossiness", 0.6f);
                material.color = new Color(0.8f, 0.8f, 0.85f);
                renderer.material = material;
            }

            return shaker;
        }

        #endregion

        #region 物件管理

        /// <summary>
        /// 重置所有容器
        /// </summary>
        public void ResetAllContainers()
        {
            // 清空所有酒杯
            foreach (var glass in spawnedGlasses)
            {
                if (glass != null)
                {
                    glass.Empty();
                }
            }

            // 清空搖酒器
            if (spawnedShaker != null)
            {
                spawnedShaker.Empty();
            }

            Debug.Log("EnvironmentManager: All containers reset");
        }

        /// <summary>
        /// 取得最近的空酒杯
        /// </summary>
        public Glass GetNearestEmptyGlass(Vector3 position)
        {
            Glass nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var glass in spawnedGlasses)
            {
                if (glass == null || !glass.IsEmpty) continue;

                float distance = Vector3.Distance(position, glass.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = glass;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 取得指定酒類的酒瓶
        /// </summary>
        public Bottle GetBottle(string liquorId)
        {
            foreach (var bottle in spawnedBottles)
            {
                if (bottle != null && bottle.LiquorData?.id == liquorId)
                {
                    return bottle;
                }
            }
            return null;
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 所有酒瓶
        /// </summary>
        public IReadOnlyList<Bottle> Bottles => spawnedBottles.AsReadOnly();

        /// <summary>
        /// 所有酒杯
        /// </summary>
        public IReadOnlyList<Glass> Glasses => spawnedGlasses.AsReadOnly();

        /// <summary>
        /// 搖酒器
        /// </summary>
        public Shaker Shaker => spawnedShaker;

        #endregion
    }
}
