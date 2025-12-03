using UnityEngine;
using BarSimulator.Systems;
using BarSimulator.Managers;
using BarSimulator.Environment;

namespace BarSimulator.Core
{
    /// <summary>
    /// GameScene 場景初始化器
    /// 使用混合架構：靜態結構在 Scene 中，動態物件由 DynamicObjectSpawner 生成
    /// 取代原本的 BarSceneBuilder 全動態生成模式
    /// </summary>
    public class GameSceneInitializer : MonoBehaviour
    {
        #region 序列化欄位

        [Header("場景組件引用")]
        [Tooltip("動態物件生成器")]
        [SerializeField] private DynamicObjectSpawner objectSpawner;

        [Tooltip("玩家生成位置")]
        [SerializeField] private Transform playerSpawnPoint;

        [Tooltip("主攝影機")]
        [SerializeField] private Camera mainCamera;

        [Header("系統設定")]
        [Tooltip("自動創建系統管理器")]
        [SerializeField] private bool autoCreateSystems = true;

        [Tooltip("自動生成動態物件")]
        [SerializeField] private bool autoSpawnDynamicObjects = true;

        [Tooltip("自動生成玩家")]
        [SerializeField] private bool autoSpawnPlayer = true;

        [Header("玩家設定")]
        [Tooltip("玩家預製物（可選）")]
        [SerializeField] private GameObject playerPrefab;

        [Header("環境設定")]
        [Tooltip("環境音樂")]
        [SerializeField] private AudioClip ambientMusic;

        [Tooltip("環境音效")]
        [SerializeField] private AudioClip[] ambientSounds;

        #endregion

        #region 私有欄位

        private GameObject spawnedPlayer;
        private bool isInitialized = false;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            Debug.Log("GameSceneInitializer: Awake - 準備初始化遊戲場景");
        }

        private void Start()
        {
            Initialize();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化遊戲場景
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("GameSceneInitializer: 場景已經初始化過了");
                return;
            }

            Debug.Log("GameSceneInitializer: 開始初始化遊戲場景...");

            // 1. 創建核心系統
            if (autoCreateSystems)
            {
                CreateCoreSystems();
            }

            // 2. 驗證靜態場景結構
            ValidateStaticStructure();

            // 3. 生成動態物件
            if (autoSpawnDynamicObjects)
            {
                SpawnDynamicObjects();
            }

            // 4. 生成玩家
            if (autoSpawnPlayer)
            {
                SpawnPlayer();
            }

            // 5. 設置攝影機
            SetupCamera();

            // 6. 設置環境
            SetupEnvironment();

            // 7. 修復場景材質（解決紫色材質問題）
            FixSceneMaterials();

            // 8. 設置游標（鎖定在遊戲中）
            SetupCursor();

            // 9. 初始化遊戲狀態
            InitializeGameState();

            isInitialized = true;
            Debug.Log("GameSceneInitializer: 遊戲場景初始化完成！");
        }

        /// <summary>
        /// 創建核心系統
        /// </summary>
        private void CreateCoreSystems()
        {
            Debug.Log("GameSceneInitializer: 創建核心系統...");

            // GameManager
            if (GameManager.Instance == null)
            {
                var gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
                Debug.Log("GameSceneInitializer: 創建 GameManager");
            }

            // AudioManager
            if (Managers.AudioManager.Instance == null)
            {
                var audioObj = new GameObject("AudioManager");
                audioObj.AddComponent<Managers.AudioManager>();
                Debug.Log("GameSceneInitializer: 創建 AudioManager");
            }

            // LightingManager
            if (LightingManager.Instance == null)
            {
                var lightObj = new GameObject("LightingManager");
                lightObj.AddComponent<LightingManager>();
                Debug.Log("GameSceneInitializer: 創建 LightingManager");
            }

            // EnvironmentManager
            if (EnvironmentManager.Instance == null)
            {
                var envObj = new GameObject("EnvironmentManager");
                envObj.AddComponent<EnvironmentManager>();
                Debug.Log("GameSceneInitializer: 創建 EnvironmentManager");
            }

            // CocktailSystem
            if (CocktailSystem.Instance == null)
            {
                var cocktailObj = new GameObject("CocktailSystem");
                cocktailObj.AddComponent<CocktailSystem>();
                Debug.Log("GameSceneInitializer: 創建 CocktailSystem");
            }

            // NPCManager
            if (NPCManager.Instance == null)
            {
                var npcObj = new GameObject("NPCManager");
                npcObj.AddComponent<NPCManager>();
                Debug.Log("GameSceneInitializer: 創建 NPCManager");
            }

            // InteractionSystem
            if (InteractionSystem.Instance == null)
            {
                var interactionObj = new GameObject("InteractionSystem");
                interactionObj.AddComponent<InteractionSystem>();
                Debug.Log("GameSceneInitializer: 創建 InteractionSystem");
            }

            // UpgradeSystem (應該已經存在，從主選單持久化)
            if (UpgradeSystem.Instance == null)
            {
                var upgradeObj = new GameObject("UpgradeSystem");
                upgradeObj.AddComponent<UpgradeSystem>();
                Debug.Log("GameSceneInitializer: 創建 UpgradeSystem");
            }

            // TutorialSystem
            if (TutorialSystem.Instance == null)
            {
                var tutorialObj = new GameObject("TutorialSystem");
                tutorialObj.AddComponent<TutorialSystem>();
                Debug.Log("GameSceneInitializer: 創建 TutorialSystem");
            }

            // UIManager
            if (UIManager.Instance == null)
            {
                var uiObj = new GameObject("UIManager");
                uiObj.AddComponent<UIManager>();
                Debug.Log("GameSceneInitializer: 創建 UIManager");
            }
        }

        /// <summary>
        /// 驗證靜態場景結構
        /// </summary>
        private void ValidateStaticStructure()
        {
            Debug.Log("GameSceneInitializer: 驗證靜態場景結構...");

            var staticStructures = FindObjectsByType<StaticBarStructure>(FindObjectsSortMode.None);

            if (staticStructures.Length == 0)
            {
                Debug.LogWarning("GameSceneInitializer: 警告！場景中沒有找到任何 StaticBarStructure 組件。");
                Debug.LogWarning("請在 Scene 中手動創建地板、牆壁、吧台等靜態結構，並加上 StaticBarStructure 組件。");
            }
            else
            {
                Debug.Log($"GameSceneInitializer: 找到 {staticStructures.Length} 個靜態結構組件");

                // 檢查重要結構
                bool hasBottleShelf = false;
                bool hasGlassStation = false;

                foreach (var structure in staticStructures)
                {
                    if (structure.IsBottleShelf) hasBottleShelf = true;
                    if (structure.IsGlassStation) hasGlassStation = true;
                }

                if (!hasBottleShelf)
                {
                    Debug.LogWarning("GameSceneInitializer: 沒有標記為 BottleShelf 的結構！動態物件可能無法正確生成。");
                }

                if (!hasGlassStation)
                {
                    Debug.LogWarning("GameSceneInitializer: 沒有標記為 GlassStation 的結構！");
                }
            }
        }

        /// <summary>
        /// 生成動態物件
        /// </summary>
        private void SpawnDynamicObjects()
        {
            Debug.Log("GameSceneInitializer: 生成動態物件...");

            if (objectSpawner == null)
            {
                objectSpawner = FindAnyObjectByType<DynamicObjectSpawner>();
            }

            if (objectSpawner != null)
            {
                objectSpawner.SpawnAllObjects();
            }
            else
            {
                Debug.LogWarning("GameSceneInitializer: 找不到 DynamicObjectSpawner！請在場景中添加此組件。");
            }
        }

        /// <summary>
        /// 生成玩家
        /// </summary>
        private void SpawnPlayer()
        {
            Debug.Log("GameSceneInitializer: 生成玩家...");

            Vector3 spawnPos = playerSpawnPoint != null
                ? playerSpawnPoint.position
                : new Vector3(0f, 1f, 5f);

            Quaternion spawnRot = playerSpawnPoint != null
                ? playerSpawnPoint.rotation
                : Quaternion.identity;

            if (playerPrefab != null)
            {
                spawnedPlayer = Instantiate(playerPrefab, spawnPos, spawnRot);
                spawnedPlayer.name = "Player";
            }
            else
            {
                // 創建簡單的玩家物件
                spawnedPlayer = CreateBasicPlayer(spawnPos);
            }

            Debug.Log($"GameSceneInitializer: 玩家已生成於 {spawnPos}");
        }

        /// <summary>
        /// 創建基礎玩家物件
        /// </summary>
        private GameObject CreateBasicPlayer(Vector3 position)
        {
            var playerObj = new GameObject("Player");
            playerObj.transform.position = position;

            // 添加角色控制器
            var controller = playerObj.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = new Vector3(0f, 1f, 0f);

            // 添加攝影機（如果沒有主攝影機）
            if (mainCamera == null || Camera.main == null)
            {
                var camObj = new GameObject("PlayerCamera");
                camObj.transform.SetParent(playerObj.transform);
                camObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
                mainCamera = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";

                // AudioListener will be added in SetupCamera() to avoid duplicates
            }

            // 添加玩家控制器腳本（如果存在）
            // 注意：PlayerController 組件需要根據你的實際實現來添加
            // var playerController = playerObj.GetComponent<PlayerController>();
            // if (playerController == null)
            // {
            //     playerObj.AddComponent<PlayerController>();
            // }
            Debug.Log("GameSceneInitializer: 基礎玩家物件已創建，請根據需要添加玩家控制器");

            return playerObj;
        }

        /// <summary>
        /// 設置攝影機
        /// </summary>
        private void SetupCamera()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera != null)
            {
                // 移除多餘的 AudioListener - 只保留主攝像機上的一個
                var listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
                if (listeners.Length > 1)
                {
                    AudioListener mainCameraListener = mainCamera.GetComponent<AudioListener>();

                    // 如果主攝像機沒有 AudioListener，添加一個
                    if (mainCameraListener == null)
                    {
                        mainCameraListener = mainCamera.gameObject.AddComponent<AudioListener>();
                    }

                    // 移除所有其他的 AudioListener
                    foreach (var listener in listeners)
                    {
                        if (listener != mainCameraListener)
                        {
                            Debug.Log($"GameSceneInitializer: 移除多餘的 AudioListener from {listener.gameObject.name}");
                            DestroyImmediate(listener);
                        }
                    }
                }
                else if (listeners.Length == 0)
                {
                    // 如果沒有 AudioListener，在主攝像機上添加一個
                    mainCamera.gameObject.AddComponent<AudioListener>();
                    Debug.Log("GameSceneInitializer: 添加 AudioListener 到主攝像機");
                }

                Debug.Log("GameSceneInitializer: 攝影機設置完成");
            }
        }

        /// <summary>
        /// 設置環境
        /// </summary>
        private void SetupEnvironment()
        {
            Debug.Log("GameSceneInitializer: 設置環境...");

            // 播放環境音樂
            // 注意：AudioManager.PlayMusic 使用預設的音樂軌道索引，而不是直接播放 AudioClip
            // 如果需要播放自訂音樂，請修改 AudioManager 或使用不同的方法
            if (Managers.AudioManager.Instance != null)
            {
                // 播放第一首音樂軌道（索引 0）
                Managers.AudioManager.Instance.PlayMusic(0);
            }

            // 設置環境光照
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.25f, 0.2f);

            Debug.Log("GameSceneInitializer: 環境設置完成");
        }

        /// <summary>
        /// 修復場景材質
        /// </summary>
        private void FixSceneMaterials()
        {
            Debug.Log("GameSceneInitializer: 修復場景材質...");

            // 檢查是否已經有 SceneMaterialFixer
            var materialFixer = FindAnyObjectByType<SceneMaterialFixer>();

            if (materialFixer == null)
            {
                // 創建材質修復器
                var fixerObj = new GameObject("SceneMaterialFixer");
                materialFixer = fixerObj.AddComponent<SceneMaterialFixer>();
            }

            // 執行材質修復
            materialFixer.FixAllMaterials();
            materialFixer.OptimizeLighting();

            Debug.Log("GameSceneInitializer: 場景材質修復完成");
        }

        /// <summary>
        /// 設置游標
        /// </summary>
        private void SetupCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 初始化遊戲狀態
        /// </summary>
        private void InitializeGameState()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameState(GameState.Playing);
                Debug.Log("GameSceneInitializer: 遊戲狀態設為 Playing");
            }

            // 嘗試載入存檔
            if (SaveLoadSystem.Instance != null && SaveLoadSystem.Instance.HasSaveFile())
            {
                Debug.Log("GameSceneInitializer: 發現存檔，嘗試載入...");
                SaveLoadSystem.Instance.LoadGame();
            }
        }

        #endregion

        #region 公開方法

        /// <summary>
        /// 強制重新初始化（用於調試）
        /// </summary>
        [ContextMenu("Force Reinitialize")]
        public void ForceReinitialize()
        {
            isInitialized = false;
            Initialize();
        }

        /// <summary>
        /// 獲取生成的玩家物件
        /// </summary>
        public GameObject GetPlayer()
        {
            return spawnedPlayer;
        }

        #endregion
    }
}
