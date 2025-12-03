using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BarSimulator.Systems;
using BarSimulator.Managers;
using BarSimulator.Interaction;
using BarSimulator.Objects;

namespace BarSimulator.Core
{
    /// <summary>
    /// 遊戲引導程序 - 唯一的場景入口點
    /// 使用協程按照嚴格順序初始化所有系統
    /// Code-Only 方式，無需手動設定 Prefab
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        #region Configuration

        [Header("World Settings")]
        [SerializeField] private Vector3 floorSize = new Vector3(20f, 0.2f, 15f);
        [SerializeField] private Vector3 barCounterSize = new Vector3(8f, 1f, 2f);
        [SerializeField] private Vector3 barCounterPosition = new Vector3(0f, 0.5f, -5f);

        [Header("Spawn Settings")]
        [SerializeField] private int bottleCount = 6;
        [SerializeField] private int glassCount = 4;
        [SerializeField] private int npcCount = 1;

        [Header("Player Settings")]
        [SerializeField] private Vector3 playerSpawnPosition = new Vector3(0f, 1f, 3f);
        [SerializeField] private float playerHeight = 1.8f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        #endregion

        #region Private Fields

        private GameObject player;
        private Camera mainCamera;
        private Canvas uiCanvas;
        private GameObject crosshair;
        private TextMeshProUGUI interactionText;

        // 系統引用
        private MaterialManager materialManager;
        private GameManager gameManager;
        private AudioManager audioManager;
        private InteractionSystem interactionSystem;
        private CocktailSystem cocktailSystem;
        private UIManager uiManager;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            StartCoroutine(BootstrapGame());
        }

        #endregion

        #region Coroutine Bootstrapper

        /// <summary>
        /// 主要啟動協程 - 按順序執行所有初始化步驟
        /// </summary>
        private IEnumerator BootstrapGame()
        {
            Log("========================================");
            Log("Game Bootstrapper Started");
            Log("========================================");

            // Step 1: 初始化核心系統
            yield return StartCoroutine(InitCore());

            // Step 2: 初始化視覺系統（Material Manager + 預生成材質）
            yield return StartCoroutine(InitVisuals());

            // Step 3: 建構世界（地板、牆壁、吧台）
            yield return StartCoroutine(BuildWorld());

            // Step 4: 生成道具（酒瓶、杯子、NPC）
            yield return StartCoroutine(SpawnProps());

            // Step 5: 初始化 UI
            yield return StartCoroutine(InitUI());

            // Step 6: 啟動遊戲
            yield return StartCoroutine(StartGame());

            Log("========================================");
            Log("Game Bootstrap Complete!");
            Log("========================================");
        }

        #endregion

        #region Step 1: Init Core

        /// <summary>
        /// 步驟 1: 初始化核心系統（GameManager, AudioManager, EventBus）
        /// </summary>
        private IEnumerator InitCore()
        {
            Log("[Step 1/6] Initializing Core Systems...");

            // EventBus 是靜態的，無需初始化
            Log("  ✓ EventBus (static) ready");

            // 創建 GameManager
            if (GameManager.Instance == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gameManager = gmObj.AddComponent<GameManager>();
                Log("  ✓ GameManager created");
            }
            else
            {
                gameManager = GameManager.Instance;
                Log("  ✓ GameManager found (existing)");
            }

            yield return null; // 等待一幀確保單例初始化

            // 創建 AudioManager
            if (AudioManager.Instance == null)
            {
                GameObject audioObj = new GameObject("AudioManager");
                audioManager = audioObj.AddComponent<AudioManager>();
                Log("  ✓ AudioManager created");
            }
            else
            {
                audioManager = AudioManager.Instance;
                Log("  ✓ AudioManager found (existing)");
            }

            yield return null;

            Log("[Step 1/6] Core Systems Initialized ✓");
        }

        #endregion

        #region Step 2: Init Visuals

        /// <summary>
        /// 步驟 2: 初始化視覺系統（MaterialManager）並預生成所有材質
        /// </summary>
        private IEnumerator InitVisuals()
        {
            Log("[Step 2/6] Initializing Visual Systems...");

            // 創建 MaterialManager
            if (MaterialManager.Instance == null)
            {
                GameObject matObj = new GameObject("MaterialManager");
                materialManager = matObj.AddComponent<MaterialManager>();
                Log("  ✓ MaterialManager created");
            }
            else
            {
                materialManager = MaterialManager.Instance;
                Log("  ✓ MaterialManager found (existing)");
            }

            yield return null; // 等待 MaterialManager.Awake() 完成

            // 預先生成常用材質（避免運行時卡頓）
            Log("  → Pre-generating materials...");

            materialManager.CreateBarCounterMaterial();
            Log("    • Bar Counter Material");

            materialManager.CreateWoodMaterial("Floor", new Color(0.6f, 0.4f, 0.2f), 0.8f);
            Log("    • Floor Wood Material");

            materialManager.CreateMetalMaterial("Shaker", new Color(0.8f, 0.8f, 0.8f), 0.3f, false);
            Log("    • Metal Shaker Material");

            materialManager.CreateClearGlassMaterial();
            Log("    • Clear Glass Material");

            materialManager.CreateTintedGlassMaterial("GreenBottle", new Color(0.2f, 0.8f, 0.3f));
            Log("    • Tinted Glass Material");

            yield return null;

            Log("[Step 2/6] Visual Systems Initialized ✓");
        }

        #endregion

        #region Step 3: Build World

        /// <summary>
        /// 步驟 3: 建構世界（地板、牆壁、吧台）
        /// </summary>
        private IEnumerator BuildWorld()
        {
            Log("[Step 3/6] Building World...");

            // 創建地板
            GameObject floor = CreateFloor();
            Log("  ✓ Floor created");

            yield return null;

            // 創建吧台
            GameObject barCounter = CreateBarCounter();
            Log("  ✓ Bar Counter created");

            yield return null;

            // 創建背景牆
            GameObject backWall = CreateBackWall();
            Log("  ✓ Back Wall created");

            yield return null;

            // 創建燈光
            CreateLighting();
            Log("  ✓ Lighting setup");

            yield return null;

            Log("[Step 3/6] World Built ✓");
        }

        /// <summary>
        /// 創建地板
        /// </summary>
        private GameObject CreateFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.position = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = floorSize;

            // 應用木紋材質
            Material floorMat = materialManager.CreateWoodMaterial(
                "Floor",
                new Color(0.6f, 0.4f, 0.2f),
                0.8f
            );
            floor.GetComponent<Renderer>().material = floorMat;

            // 設定碰撞層
            floor.layer = LayerMask.NameToLayer("Default");

            return floor;
        }

        /// <summary>
        /// 創建吧台
        /// </summary>
        private GameObject CreateBarCounter()
        {
            GameObject barCounter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barCounter.name = "BarCounter";
            barCounter.transform.position = barCounterPosition;
            barCounter.transform.localScale = barCounterSize;

            // 應用吧台材質
            Material barMat = materialManager.CreateBarCounterMaterial();
            barCounter.GetComponent<Renderer>().material = barMat;

            // 設定碰撞層
            barCounter.layer = LayerMask.NameToLayer("Default");

            return barCounter;
        }

        /// <summary>
        /// 創建背景牆
        /// </summary>
        private GameObject CreateBackWall()
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "BackWall";
            wall.transform.position = new Vector3(0f, 2.5f, -7f);
            wall.transform.localScale = new Vector3(15f, 5f, 0.2f);

            // 應用簡單材質（淺灰色）
            Material wallMat = materialManager.CreateWoodMaterial(
                "Wall",
                new Color(0.85f, 0.85f, 0.8f),
                0.9f
            );
            wall.GetComponent<Renderer>().material = wallMat;

            return wall;
        }

        /// <summary>
        /// 創建場景燈光
        /// </summary>
        private void CreateLighting()
        {
            // 主要方向光（太陽光）
            GameObject directionalLight = new GameObject("Directional Light");
            Light light = directionalLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.9f);
            light.intensity = 1f;
            directionalLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 環境光
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.45f);

            // 吧台聚光燈（可選）
            GameObject spotLight = new GameObject("Bar Spotlight");
            spotLight.transform.position = barCounterPosition + Vector3.up * 3f;
            spotLight.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Light spot = spotLight.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.color = new Color(1f, 0.9f, 0.7f);
            spot.intensity = 2f;
            spot.range = 10f;
            spot.spotAngle = 60f;
        }

        #endregion

        #region Step 4: Spawn Props

        /// <summary>
        /// 步驟 4: 生成道具（酒瓶、杯子、NPC）
        /// </summary>
        private IEnumerator SpawnProps()
        {
            Log("[Step 4/6] Spawning Props...");

            // 生成酒瓶
            for (int i = 0; i < bottleCount; i++)
            {
                CreateBottle(i);
            }
            Log($"  ✓ Spawned {bottleCount} bottles");

            yield return null;

            // 生成杯子
            for (int i = 0; i < glassCount; i++)
            {
                CreateGlass(i);
            }
            Log($"  ✓ Spawned {glassCount} glasses");

            yield return null;

            // 生成 NPC（稍後實現）
            // for (int i = 0; i < npcCount; i++)
            // {
            //     CreateNPC(i);
            // }
            Log($"  ⚠ NPC spawning skipped (implement later)");

            yield return null;

            Log("[Step 4/6] Props Spawned ✓");
        }

        /// <summary>
        /// 創建酒瓶
        /// </summary>
        private GameObject CreateBottle(int index)
        {
            GameObject bottle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bottle.name = $"Bottle_{index}";

            // 調整形狀（細長瓶子）
            bottle.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);

            // 放置在吧台後方
            float xOffset = (index - bottleCount / 2f) * 0.5f;
            bottle.transform.position = barCounterPosition + new Vector3(xOffset, 1.3f, -0.8f);

            // 應用玻璃材質（不同顏色）
            Color[] bottleColors = new Color[]
            {
                new Color(0.2f, 0.8f, 0.3f), // 綠色
                new Color(0.8f, 0.6f, 0.2f), // 琥珀色
                new Color(0.9f, 0.9f, 0.95f), // 透明
                new Color(0.3f, 0.5f, 0.8f), // 藍色
                new Color(0.8f, 0.3f, 0.3f), // 紅色
                new Color(0.6f, 0.4f, 0.8f)  // 紫色
            };

            Color tintColor = bottleColors[index % bottleColors.Length];
            Material bottleMat = materialManager.CreateTintedGlassMaterial($"Bottle{index}", tintColor);
            bottle.GetComponent<Renderer>().material = bottleMat;

            // 添加 Interactable 組件
            var interactable = bottle.AddComponent<Bottle>();
            interactable.DisplayName = $"Bottle {index + 1}";

            // 設定層級
            bottle.layer = LayerMask.NameToLayer("Interactable");

            return bottle;
        }

        /// <summary>
        /// 創建杯子
        /// </summary>
        private GameObject CreateGlass(int index)
        {
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glass.name = $"Glass_{index}";

            // 調整形狀（矮杯子）
            glass.transform.localScale = new Vector3(0.2f, 0.15f, 0.2f);

            // 放置在吧台上
            float xOffset = (index - glassCount / 2f) * 0.8f;
            glass.transform.position = barCounterPosition + new Vector3(xOffset, 0.65f, 0.5f);

            // 應用透明玻璃材質
            Material glassMat = materialManager.CreateClearGlassMaterial();
            glass.GetComponent<Renderer>().material = glassMat;

            // 添加 Interactable 組件
            var interactable = glass.AddComponent<Glass>();
            interactable.DisplayName = $"Glass {index + 1}";

            // 設定層級
            glass.layer = LayerMask.NameToLayer("Interactable");

            return glass;
        }

        #endregion

        #region Step 5: Init UI

        /// <summary>
        /// 步驟 5: 初始化 UI（Canvas、準心、互動提示）
        /// </summary>
        private IEnumerator InitUI()
        {
            Log("[Step 5/6] Initializing UI...");

            // 創建玩家（必須在 UI 之前，因為攝影機需要引用）
            CreatePlayer();
            Log("  ✓ Player created");

            yield return null;

            // 創建 UI Canvas
            CreateUICanvas();
            Log("  ✓ UI Canvas created");

            yield return null;

            // 創建準心
            CreateCrosshair();
            Log("  ✓ Crosshair created");

            yield return null;

            // 創建互動提示文字
            CreateInteractionText();
            Log("  ✓ Interaction text created");

            yield return null;

            // 創建 UIManager
            if (UIManager.Instance == null)
            {
                GameObject uiManagerObj = new GameObject("UIManager");
                uiManager = uiManagerObj.AddComponent<UIManager>();
                Log("  ✓ UIManager created");
            }
            else
            {
                uiManager = UIManager.Instance;
                Log("  ✓ UIManager found (existing)");
            }

            yield return null;

            Log("[Step 5/6] UI Initialized ✓");
        }

        /// <summary>
        /// 創建玩家
        /// </summary>
        private void CreatePlayer()
        {
            // 創建玩家主體
            player = new GameObject("Player");
            player.transform.position = playerSpawnPosition;

            // 添加 CharacterController
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = playerHeight;
            controller.radius = 0.5f;
            controller.center = new Vector3(0f, playerHeight * 0.5f, 0f);

            // 創建攝影機
            GameObject cameraObj = new GameObject("PlayerCamera");
            cameraObj.transform.SetParent(player.transform);
            cameraObj.transform.localPosition = new Vector3(0f, playerHeight - 0.2f, 0f);
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";

            // 添加 AudioListener
            if (cameraObj.GetComponent<AudioListener>() == null)
            {
                cameraObj.AddComponent<AudioListener>();
            }

            // 添加玩家控制器（如果存在）
            // var playerController = player.AddComponent<PlayerController>();

            // 鎖定游標
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// 創建 UI Canvas
        /// </summary>
        private void CreateUICanvas()
        {
            GameObject canvasObj = new GameObject("GameCanvas");
            uiCanvas = canvasObj.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 創建準心
        /// </summary>
        private void CreateCrosshair()
        {
            crosshair = new GameObject("Crosshair");
            crosshair.transform.SetParent(uiCanvas.transform, false);

            RectTransform rect = crosshair.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(20f, 20f);
            rect.anchoredPosition = Vector2.zero;

            Image image = crosshair.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.7f);

            // 創建簡單的十字準心（使用 + 字形）
            // 中心點
            GameObject center = new GameObject("Center");
            center.transform.SetParent(crosshair.transform, false);
            RectTransform centerRect = center.AddComponent<RectTransform>();
            centerRect.sizeDelta = new Vector2(2f, 2f);
            centerRect.anchoredPosition = Vector2.zero;
            Image centerImage = center.AddComponent<Image>();
            centerImage.color = Color.white;
        }

        /// <summary>
        /// 創建互動提示文字
        /// </summary>
        private void CreateInteractionText()
        {
            GameObject textObj = new GameObject("InteractionText");
            textObj.transform.SetParent(uiCanvas.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400f, 50f);
            rect.anchoredPosition = new Vector2(0f, -100f);

            interactionText = textObj.AddComponent<TextMeshProUGUI>();
            interactionText.text = "";
            interactionText.fontSize = 24f;
            interactionText.color = Color.white;
            interactionText.alignment = TextAlignmentOptions.Center;
            interactionText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            // 添加陰影
            var shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }

        #endregion

        #region Step 6: Start Game

        /// <summary>
        /// 步驟 6: 啟動遊戲（初始化其他系統並開始遊戲）
        /// </summary>
        private IEnumerator StartGame()
        {
            Log("[Step 6/6] Starting Game...");

            // 創建 InteractionSystem
            if (InteractionSystem.Instance == null)
            {
                GameObject interactionObj = new GameObject("InteractionSystem");
                interactionSystem = interactionObj.AddComponent<InteractionSystem>();
                Log("  ✓ InteractionSystem created");
            }
            else
            {
                interactionSystem = InteractionSystem.Instance;
                Log("  ✓ InteractionSystem found (existing)");
            }

            yield return null;

            // 創建 CocktailSystem
            if (CocktailSystem.Instance == null)
            {
                GameObject cocktailObj = new GameObject("CocktailSystem");
                cocktailSystem = cocktailObj.AddComponent<CocktailSystem>();
                Log("  ✓ CocktailSystem created");
            }
            else
            {
                cocktailSystem = CocktailSystem.Instance;
                Log("  ✓ CocktailSystem found (existing)");
            }

            yield return null;

            // 設定遊戲狀態為 Playing
            if (gameManager != null)
            {
                gameManager.SetGameState(GameState.Playing);
                Log("  ✓ Game state set to Playing");
            }

            yield return null;

            Log("[Step 6/6] Game Started ✓");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 輸出日誌（可控制）
        /// </summary>
        private void Log(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[Bootstrapper] {message}");
            }
        }

        #endregion
    }
}
