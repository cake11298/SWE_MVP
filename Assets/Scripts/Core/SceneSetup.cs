using UnityEngine;
using BarSimulator.Managers;
using BarSimulator.Systems;
using BarSimulator.Player;

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
            if (AudioManager.Instance == null)
            {
                var audioObj = new GameObject("AudioManager");
                audioObj.AddComponent<AudioManager>();
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

            // 添加 FirstPersonController
            player.AddComponent<FirstPersonController>();

            // 建立攝影機
            var cameraObj = new GameObject("PlayerCamera");
            cameraObj.transform.SetParent(player.transform);
            cameraObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            var camera = cameraObj.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;

            // 添加 AudioListener
            cameraObj.AddComponent<AudioListener>();

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
