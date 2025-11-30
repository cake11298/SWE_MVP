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

        [Tooltip("酒瓶預製物")]
        [SerializeField] private GameObject bottlePrefab;

        [Tooltip("酒杯預製物")]
        [SerializeField] private GameObject glassPrefab;

        [Tooltip("搖酒器預製物")]
        [SerializeField] private GameObject shakerPrefab;

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

            // 步驟 1: 建立核心系統管理器（GameManager, AudioManager 等）
            if (autoCreateSystems)
            {
                CreateCoreSystemManagers();
            }

            // 步驟 2: 建立物理環境（使用 BarSceneBuilder）
            if (autoCreateScene)
            {
                BuildPhysicsEnvironment();
            }

            // 步驟 3: 生成玩家
            SpawnPlayer();

            // 步驟 4: 設置攝影機
            SetupCamera();

            // 步驟 5: 初始化 UI 系統（UIBuilder 和 UIFactory）
            // InitializeUISystem();

            // 步驟 6: 生成可互動物件（如果 BarSceneBuilder 未啟用）
            if (!autoCreateScene || BarSceneBuilder.Instance == null)
            {
                SpawnBarProps();
            }

            isInitialized = true;

            Debug.Log("SceneSetup: Scene initialization complete");
        }

        /// <summary>
        /// 建立物理環境（統一使用 BarSceneBuilder）
        /// </summary>
        private void BuildPhysicsEnvironment()
        {
            // 優先使用 BarSceneBuilder 建立完整場景
            if (BarSceneBuilder.Instance == null)
            {
                Debug.Log("SceneSetup: Creating BarSceneBuilder for physics environment...");
                var builderObj = new GameObject("BarSceneBuilder");
                var builder = builderObj.AddComponent<BarSceneBuilder>();
                // BarSceneBuilder 會在 Start 時自動建立場景
            }
            else
            {
                Debug.Log("SceneSetup: BarSceneBuilder already exists, skipping creation");
            }

            // 如果 BarSceneBuilder 沒有啟用，則建立基本場景作為後備
            if (floor == null && BarSceneBuilder.Instance == null)
            {
                Debug.LogWarning("SceneSetup: BarSceneBuilder not found, creating fallback scene");
                CreateFallbackScene();
            }
        }

        /// <summary>
        /// 建立後備場景（當 BarSceneBuilder 不可用時）
        /// </summary>
        private void CreateFallbackScene()
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

            // 建立吧台
            barCounter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barCounter.name = "BarCounter";
            barCounter.transform.position = new Vector3(0f, 0.5f, 2f);
            barCounter.transform.localScale = new Vector3(4f, 1f, 0.8f);

            renderer = barCounter.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.4f, 0.25f, 0.15f); // 木頭色
            }

            // 建立後牆
            var backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.name = "BackWall";
            backWall.transform.position = new Vector3(0f, 1.5f, 3f);
            backWall.transform.localScale = new Vector3(6f, 3f, 0.1f);

            renderer = backWall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.3f, 0.25f, 0.2f);
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
        /// 建立核心系統管理器（不包含 UI 相關）
        /// </summary>
        private void CreateCoreSystemManagers()
        {
            Debug.Log("SceneSetup: Creating core system managers...");

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

            // EnvironmentManager - 禁用自動生成（由 BarSceneBuilder 處理）
            if (EnvironmentManager.Instance == null)
            {
                var envObj = new GameObject("EnvironmentManager");
                var envMgr = envObj.AddComponent<EnvironmentManager>();
                // 註：不在此生成物件，由 BarSceneBuilder 處理
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

            Debug.Log("SceneSetup: Core system managers created");
        }

        /// <summary>
        /// 初始化 UI 系統（UIBuilder 和 UIFactory）
        /// </summary>
        private void InitializeUISystem()
        {
            Debug.Log("SceneSetup: Initializing UI system...");

            // 創建 UIBuilder（基礎 UI：準心、提示等）
            if (UIBuilder.Instance == null)
            {
                var uiObj = new GameObject("UIBuilder");
                uiObj.AddComponent<UIBuilder>();
            }

            // 創建 UIFactory（複雜 UI 面板）
            UIFactory uiFactory = null;
            if (UIFactory.Instance == null)
            {
                var factoryObj = new GameObject("UIFactory");
                uiFactory = factoryObj.AddComponent<UIFactory>();
            }
            else
            {
                uiFactory = UIFactory.Instance;
            }

            // 建構所有 UI
            uiFactory.BuildAllUI();

            // 創建 ShopManager 並注入 UI
            BarSimulator.UI.ShopManager shopManager = null;
            if (BarSimulator.UI.ShopManager.Instance == null)
            {
                var shopObj = new GameObject("ShopManager");
                shopManager = shopObj.AddComponent<BarSimulator.UI.ShopManager>();
            }
            else
            {
                shopManager = BarSimulator.UI.ShopManager.Instance;
            }

            // 建構並注入 Shop UI
            var shopUI = uiFactory.BuildShopUI();
            shopManager.InitializeReferences(shopUI);

            // 創建 SettingsManager 並注入 UI
            BarSimulator.UI.SettingsManager settingsManager = null;
            if (BarSimulator.UI.SettingsManager.Instance == null)
            {
                var settingsObj = new GameObject("SettingsManager");
                settingsManager = settingsObj.AddComponent<BarSimulator.UI.SettingsManager>();
            }
            else
            {
                settingsManager = BarSimulator.UI.SettingsManager.Instance;
            }

            // 建構並注入 Settings UI
            var settingsUI = uiFactory.BuildSettingsUI();
            settingsManager.InitializeReferences(settingsUI);

            Debug.Log("SceneSetup: UI system initialized successfully");
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
            // 銷毀場景中預設的 Main Camera，避免與玩家攝影機衝突
            var mainCameras = GameObject.FindGameObjectsWithTag("MainCamera");
            foreach (var cam in mainCameras)
            {
                if (cam.transform.parent != playerObject.transform)
                {
                    Destroy(cam);
                }
            }

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
            GameObject bottleObj;
            if (bottlePrefab != null)
            {
                bottleObj = Instantiate(bottlePrefab, position, Quaternion.identity);
            }
            else
            {
                bottleObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bottleObj.transform.position = position;
                bottleObj.transform.localScale = new Vector3(0.08f, 0.15f, 0.08f);
                bottleObj.AddComponent<Rigidbody>();
                bottleObj.AddComponent<BarSimulator.Objects.Bottle>();
            }
            
            bottleObj.name = $"Bottle_{liquorId}";

            // 設置顏色
            var renderer = bottleObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            var bottle = bottleObj.GetComponent<BarSimulator.Objects.Bottle>();
            if (bottle != null)
            {
                bottle.SetOriginalPosition(position);
                // 等待 CocktailSystem 初始化後設置酒類
                StartCoroutine(InitializeBottleAfterCocktailSystem(bottle, liquorId));
            }
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
            GameObject glassObj;
            if (glassPrefab != null)
            {
                glassObj = Instantiate(glassPrefab, position, Quaternion.identity);
            }
            else
            {
                glassObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                glassObj.transform.position = position;
                glassObj.transform.localScale = new Vector3(0.06f, 0.1f, 0.06f);
                glassObj.AddComponent<Rigidbody>();
                glassObj.AddComponent<BarSimulator.Objects.Glass>();
            }
            
            glassObj.name = "Glass";
        }

        /// <summary>
        /// 生成搖酒器
        /// </summary>
        private void SpawnShaker(Vector3 position)
        {
            GameObject shakerObj;
            if (shakerPrefab != null)
            {
                shakerObj = Instantiate(shakerPrefab, position, Quaternion.identity);
            }
            else
            {
                shakerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                shakerObj.transform.position = position;
                shakerObj.transform.localScale = new Vector3(0.1f, 0.15f, 0.1f);
                shakerObj.AddComponent<Rigidbody>();
                shakerObj.AddComponent<BarSimulator.Objects.Shaker>();
            }
            
            shakerObj.name = "Shaker";
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
