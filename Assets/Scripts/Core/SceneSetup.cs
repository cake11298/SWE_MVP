using UnityEngine;
using BarSimulator.Managers;
using BarSimulator.Systems;
using BarSimulator.Player;
using BarSimulator.UI;
using BarSimulator.Objects;
using BarSimulator.Interaction;

namespace BarSimulator.Core
{
    /// <summary>
    /// 場景設置 - 初始化遊戲場景和系統
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        #region 序列化欄位

        [Header("玩家設定")]
        [Tooltip("玩家生成位置")]
        [SerializeField] private Vector3 playerSpawnPosition = new Vector3(0f, 1f, -3f);

        [Tooltip("玩家初始旋轉")]
        [SerializeField] private Vector3 playerSpawnRotation = Vector3.zero;

        [Header("預製物")]
        [Tooltip("玩家預製物")]
        [SerializeField] private GameObject playerPrefab;

        [Tooltip("GameManager 預製物")]
        [SerializeField] private GameObject gameManagerPrefab;

        [Header("場景物件")]
        [Tooltip("吧台物件")]
        [SerializeField] private GameObject barCounter;

        [Tooltip("地板物件")]
        [SerializeField] private GameObject floor;

        [Tooltip("牆壁物件")]
        [SerializeField] private GameObject[] walls;

        [Header("自動建立設定")]
        [Tooltip("自動建立缺失的系統")]
        [SerializeField] private bool autoCreateSystems = true;

        [Tooltip("自動建立基本場景")]
        [SerializeField] private bool autoCreateScene = true;

        #endregion

        #region 私有欄位

        private GameObject playerObject;
        private bool isInitialized;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            // 優先初始化
            InitializeScene();
        }

        private void Start()
        {
            // 延遲初始化（等待其他系統）
            if (!isInitialized)
            {
                InitializeScene();
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化場景
        /// </summary>
        public void InitializeScene()
        {
            if (isInitialized) return;

            Debug.Log("SceneSetup: Initializing scene...");

            // 建立基本場景物件
            if (autoCreateScene)
            {
                CreateBasicScene();
            }

            // 建立系統管理器
            if (autoCreateSystems)
            {
                CreateSystemManagers();
            }

            // 生成玩家
            SpawnPlayer();

            // 設置攝影機
            SetupCamera();

            // 生成場景道具
            SpawnBarProps();

            isInitialized = true;

            Debug.Log("SceneSetup: Scene initialization complete");
        }

        /// <summary>
        /// 建立基本場景物件
        /// </summary>
        private void CreateBasicScene()
        {
            // 使用 BarSceneBuilder 建立完整場景
            if (BarSceneBuilder.Instance == null)
            {
                var builderObj = new GameObject("BarSceneBuilder");
                var builder = builderObj.AddComponent<BarSceneBuilder>();
                // BarSceneBuilder 會在 Start 時自動建立場景
            }

            // 如果 BarSceneBuilder 沒有啟用，則建立基本場景作為後備
            if (floor == null && BarSceneBuilder.Instance == null)
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "Floor";
                floor.transform.position = Vector3.zero;
                floor.transform.localScale = new Vector3(2f, 1f, 2f);

                var renderer = floor.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.2f, 0.15f, 0.1f); // 深木色
                }
            }

            // 建立吧台
            if (barCounter == null && BarSceneBuilder.Instance == null)
            {
                barCounter = GameObject.CreatePrimitive(PrimitiveType.Cube);
                barCounter.name = "BarCounter";
                barCounter.transform.position = new Vector3(0f, 0.5f, 2f);
                barCounter.transform.localScale = new Vector3(4f, 1f, 0.8f);

                var renderer = barCounter.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.4f, 0.25f, 0.15f); // 木頭色
                }
            }

            // 建立後牆
            if ((walls == null || walls.Length == 0) && BarSceneBuilder.Instance == null)
            {
                var backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                backWall.name = "BackWall";
                backWall.transform.position = new Vector3(0f, 1.5f, 3f);
                backWall.transform.localScale = new Vector3(6f, 3f, 0.1f);

                var renderer = backWall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.3f, 0.25f, 0.2f);
                }
            }

            // 建立燈光
            CreateLighting();
        }

        /// <summary>
        /// 建立場景燈光
        /// </summary>
        private void CreateLighting()
        {
            // 檢查是否已有方向光
            if (FindAnyObjectByType<Light>() == null)
            {
                // 主方向光
                var mainLightObj = new GameObject("Main Light");
                var mainLight = mainLightObj.AddComponent<Light>();
                mainLight.type = LightType.Directional;
                mainLight.color = new Color(1f, 0.95f, 0.8f);
                mainLight.intensity = 1f;
                mainLightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

                // 環境光設置
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.15f, 0.12f, 0.1f);
            }
        }

        /// <summary>
        /// 建立系統管理器
        /// </summary>
        private void CreateSystemManagers()
        {
            // GameManager
            if (GameManager.Instance == null)
            {
                if (gameManagerPrefab != null)
                {
                    Instantiate(gameManagerPrefab);
                }
                else
                {
                    var gmObj = new GameObject("GameManager");
                    gmObj.AddComponent<GameManager>();
                }
            }

            // AudioManager
            if (BarSimulator.Systems.AudioManager.Instance == null)
            {
                var audioObj = new GameObject("AudioManager");
                audioObj.AddComponent<BarSimulator.Systems.AudioManager>();
            }

            // LightingManager
            if (LightingManager.Instance == null)
            {
                var lightObj = new GameObject("LightingManager");
                lightObj.AddComponent<LightingManager>();
            }

            // EnvironmentManager
            if (EnvironmentManager.Instance == null)
            {
                var envObj = new GameObject("EnvironmentManager");
                envObj.AddComponent<EnvironmentManager>();
            }

            // CocktailSystem
            if (CocktailSystem.Instance == null)
            {
                var cocktailObj = new GameObject("CocktailSystem");
                cocktailObj.AddComponent<CocktailSystem>();
            }

            // NPCManager
            if (NPCManager.Instance == null)
            {
                var npcObj = new GameObject("NPCManager");
                npcObj.AddComponent<NPCManager>();
            }

            // InteractionSystem
            if (InteractionSystem.Instance == null)
            {
                var interactionObj = new GameObject("InteractionSystem");
                interactionObj.AddComponent<InteractionSystem>();
            }

            // UIBuilder
            if (UIBuilder.Instance == null)
            {
                var uiObj = new GameObject("UIBuilder");
                uiObj.AddComponent<UIBuilder>();
            }

            // ShopManager
            if (BarSimulator.UI.ShopManager.Instance == null)
            {
                var shopObj = new GameObject("ShopManager");
                shopObj.AddComponent<BarSimulator.UI.ShopManager>();
            }

            // UpgradeSystem
            if (UpgradeSystem.Instance == null)
            {
                var upgradeObj = new GameObject("UpgradeSystem");
                upgradeObj.AddComponent<UpgradeSystem>();
            }

            // TutorialSystem
            if (TutorialSystem.Instance == null)
            {
                var tutorialObj = new GameObject("TutorialSystem");
                tutorialObj.AddComponent<TutorialSystem>();
            }

            // SaveLoadSystem
            if (SaveLoadSystem.Instance == null)
            {
                var saveObj = new GameObject("SaveLoadSystem");
                saveObj.AddComponent<SaveLoadSystem>();
            }

            // SettingsManager
            if (BarSimulator.UI.SettingsManager.Instance == null)
            {
                var settingsObj = new GameObject("SettingsManager");
                settingsObj.AddComponent<BarSimulator.UI.SettingsManager>();
            }

            // SceneLoader
            if (SceneLoader.Instance == null)
            {
                var loaderObj = new GameObject("SceneLoader");
                loaderObj.AddComponent<SceneLoader>();
            }

            // PlacementPreviewSystem
            if (PlacementPreviewSystem.Instance == null)
            {
                var previewObj = new GameObject("PlacementPreviewSystem");
                previewObj.AddComponent<PlacementPreviewSystem>();
            }
        }

        /// <summary>
        /// 生成玩家
        /// </summary>
        private void SpawnPlayer()
        {
            // 檢查是否已有玩家
            var existingPlayer = FindAnyObjectByType<FirstPersonController>();
            if (existingPlayer != null)
            {
                playerObject = existingPlayer.gameObject;
                return;
            }

            if (playerPrefab != null)
            {
                playerObject = Instantiate(
                    playerPrefab,
                    playerSpawnPosition,
                    Quaternion.Euler(playerSpawnRotation)
                );
            }
            else
            {
                // 建立簡單玩家
                playerObject = CreateDefaultPlayer();
            }

            playerObject.name = "Player";
        }

        /// <summary>
        /// 建立預設玩家
        /// </summary>
        private GameObject CreateDefaultPlayer()
        {
            var player = new GameObject("Player");
            player.transform.position = playerSpawnPosition;
            player.transform.rotation = Quaternion.Euler(playerSpawnRotation);

            // 添加 CharacterController
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            // 先建立攝影機 (在 FirstPersonController 之前)
            var cameraObj = new GameObject("PlayerCamera");
            cameraObj.transform.SetParent(player.transform);
            cameraObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            cameraObj.tag = "MainCamera"; // 設定為主攝影機標籤

            var camera = cameraObj.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;

            // 添加 AudioListener
            cameraObj.AddComponent<AudioListener>();

            // 添加 FirstPersonController (在攝影機之後，這樣 Awake 能找到)
            player.AddComponent<FirstPersonController>();

            Debug.Log("SceneSetup: 已建立玩家和攝影機");

            return player;
        }

        /// <summary>
        /// 設置攝影機
        /// </summary>
        private void SetupCamera()
        {
            // 確保只有一個 AudioListener
            var listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            for (int i = 1; i < listeners.Length; i++)
            {
                Destroy(listeners[i]);
            }
        }

        /// <summary>
        /// 生成吧台道具
        /// </summary>
        private void SpawnBarProps()
        {
            // 確保吧台已經建立
            if (barCounter == null)
            {
                Debug.LogWarning("SceneSetup: Bar counter not found, cannot spawn props");
                return;
            }

            // 計算吧台上的位置
            Vector3 barTopPosition = barCounter.transform.position + Vector3.up * 0.6f;
            float barWidth = barCounter.transform.localScale.x;
            float spacing = barWidth / 8f; // 將吧台分成 8 段

            // 生成 3 個酒瓶 (Vodka, Gin, Rum)
            SpawnBottle("vodka", "伏特加", new Color(0.9f, 0.9f, 0.95f),
                barTopPosition + Vector3.left * spacing * 2.5f);
            SpawnBottle("gin", "琴酒", new Color(0.85f, 0.95f, 0.9f),
                barTopPosition + Vector3.left * spacing * 1.5f);
            SpawnBottle("rum", "蘭姆酒", new Color(0.7f, 0.5f, 0.3f),
                barTopPosition + Vector3.left * spacing * 0.5f);

            // 生成 2 個杯子
            SpawnGlass(barTopPosition + Vector3.right * spacing * 0.5f);
            SpawnGlass(barTopPosition + Vector3.right * spacing * 1.5f);

            // 生成搖酒器
            SpawnShaker(barTopPosition + Vector3.right * spacing * 2.5f);

            // 生成攪拌棒
            SpawnStirrer(barTopPosition + Vector3.left * spacing * 3.5f);

            // 生成裝飾工作站 (在吧台一側)
            SpawnGarnishStation(barTopPosition + Vector3.right * spacing * 3.5f + Vector3.forward * 0.3f);

            // 生成冰桶 (在吧台另一側)
            SpawnIceBucket(barTopPosition + Vector3.left * spacing * 3.5f + Vector3.forward * 0.3f);

            Debug.Log("SceneSetup: Bar props spawned successfully");
        }

        /// <summary>
        /// 生成酒瓶
        /// </summary>
        private void SpawnBottle(string liquorId, string displayName, Color color, Vector3 position)
        {
            var bottleObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bottleObj.name = $"Bottle_{liquorId}";
            bottleObj.transform.position = position;
            bottleObj.transform.localScale = new Vector3(0.08f, 0.15f, 0.08f);

            // 設置顏色
            var renderer = bottleObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            // 添加 Rigidbody
            var rb = bottleObj.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.useGravity = true;

            // 添加 Bottle 組件
            var bottle = bottleObj.AddComponent<BarSimulator.Objects.Bottle>();

            // 等待 CocktailSystem 初始化後設置酒類
            StartCoroutine(InitializeBottleAfterCocktailSystem(bottle, liquorId));
        }

        /// <summary>
        /// 延遲初始化酒瓶（等待 CocktailSystem）
        /// </summary>
        private System.Collections.IEnumerator InitializeBottleAfterCocktailSystem(
            BarSimulator.Objects.Bottle bottle, string liquorId)
        {
            // 等待 CocktailSystem 初始化
            while (BarSimulator.Systems.CocktailSystem.Instance == null)
            {
                yield return null;
            }

            // 再等一幀確保資料庫已載入
            yield return null;

            // 設置酒類
            bottle.SetLiquor(liquorId);
        }

        /// <summary>
        /// 生成杯子
        /// </summary>
        private void SpawnGlass(Vector3 position)
        {
            var glassObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glassObj.name = "Glass";
            glassObj.transform.position = position;
            glassObj.transform.localScale = new Vector3(0.06f, 0.1f, 0.06f);

            // 設置透明材質
            var renderer = glassObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.color = new Color(0.8f, 0.9f, 1f, 0.3f);
                material.SetFloat("_Mode", 3); // Transparent mode
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                renderer.material = material;
            }

            // 添加 Rigidbody
            var rb = glassObj.AddComponent<Rigidbody>();
            rb.mass = 0.2f;
            rb.useGravity = true;

            // 添加 Glass 組件
            glassObj.AddComponent<BarSimulator.Objects.Glass>();
        }

        /// <summary>
        /// 生成搖酒器
        /// </summary>
        private void SpawnShaker(Vector3 position)
        {
            var shakerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            shakerObj.name = "Shaker";
            shakerObj.transform.position = position;
            shakerObj.transform.localScale = new Vector3(0.1f, 0.15f, 0.1f);

            // 設置顏色 (銀色)
            var renderer = shakerObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.75f, 0.75f, 0.8f);
                renderer.material.SetFloat("_Metallic", 0.8f);
                renderer.material.SetFloat("_Glossiness", 0.9f);
            }

            // 添加 Rigidbody
            var rb = shakerObj.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            rb.useGravity = true;

            // 添加 Shaker 組件
            shakerObj.AddComponent<BarSimulator.Objects.Shaker>();
        }

        /// <summary>
        /// 生成攪拌棒
        /// </summary>
        private void SpawnStirrer(Vector3 position)
        {
            var stirrerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            stirrerObj.name = "Stirrer";
            stirrerObj.transform.position = position;
            stirrerObj.transform.localScale = new Vector3(0.02f, 0.15f, 0.02f);
            stirrerObj.transform.rotation = Quaternion.Euler(0f, 0f, 90f);

            // 設置顏色 (銀色)
            var renderer = stirrerObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.7f, 0.7f, 0.75f);
                renderer.material.SetFloat("_Metallic", 0.7f);
            }

            // 添加 Rigidbody
            var rb = stirrerObj.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            rb.useGravity = true;

            // 添加 Stirrer 組件
            stirrerObj.AddComponent<BarSimulator.Objects.Stirrer>();
        }

        /// <summary>
        /// 生成裝飾工作站
        /// </summary>
        private void SpawnGarnishStation(Vector3 position)
        {
            var stationObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stationObj.name = "GarnishStation";
            stationObj.transform.position = position;
            stationObj.transform.localScale = new Vector3(0.3f, 0.15f, 0.3f);

            // 設置顏色 (木頭色)
            var renderer = stationObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.6f, 0.4f, 0.2f);
            }

            // 不添加 Rigidbody (固定在吧台上)

            // 添加 GarnishStation 組件
            stationObj.AddComponent<BarSimulator.Objects.GarnishStation>();
        }

        /// <summary>
        /// 生成冰桶
        /// </summary>
        private void SpawnIceBucket(Vector3 position)
        {
            var bucketObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bucketObj.name = "IceBucket";
            bucketObj.transform.position = position;
            bucketObj.transform.localScale = new Vector3(0.15f, 0.12f, 0.15f);

            // 設置顏色 (銀色金屬)
            var renderer = bucketObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.8f, 0.85f, 0.9f);
                renderer.material.SetFloat("_Metallic", 0.9f);
                renderer.material.SetFloat("_Glossiness", 0.8f);
            }

            // 不添加 Rigidbody (固定在吧台上)

            // 添加 IceContainer 組件 (用於儲存冰塊)
            bucketObj.AddComponent<IceContainer>();
        }

        #endregion

        #region 公開方法

        /// <summary>
        /// 重置場景
        /// </summary>
        public void ResetScene()
        {
            // 重置玩家位置
            if (playerObject != null)
            {
                playerObject.transform.position = playerSpawnPosition;
                playerObject.transform.rotation = Quaternion.Euler(playerSpawnRotation);
            }

            // 重置環境
            var envManager = EnvironmentManager.Instance;
            if (envManager != null)
            {
                envManager.ResetAllContainers();
            }

            Debug.Log("SceneSetup: Scene reset");
        }

        /// <summary>
        /// 取得玩家物件
        /// </summary>
        public GameObject GetPlayer()
        {
            return playerObject;
        }

        /// <summary>
        /// 設置玩家生成位置
        /// </summary>
        public void SetSpawnPoint(Vector3 position, Vector3 rotation)
        {
            playerSpawnPosition = position;
            playerSpawnRotation = rotation;
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// 玩家生成位置
        /// </summary>
        public Vector3 PlayerSpawnPosition => playerSpawnPosition;

        #endregion
    }
}
