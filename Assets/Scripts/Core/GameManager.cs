using UnityEngine;
using UnityEngine.InputSystem;
using BarSimulator.Player;

namespace BarSimulator.Core
{
    /// <summary>
    /// 遊戲管理器 - 管理遊戲主迴圈、系統初始化、暫停/恢復
    /// 參考: src/index.js BarSimulator 類別
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region 單例模式

        private static GameManager instance;

        /// <summary>
        /// 取得 GameManager 單例
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameManager>();
                    if (instance == null)
                    {
                        Debug.LogError("GameManager: 場景中找不到 GameManager");
                    }
                }
                return instance;
            }
        }

        #endregion

        #region 序列化欄位

        [Header("場景引用")]
        [Tooltip("玩家控制器")]
        [SerializeField] private FirstPersonController playerController;

        [Tooltip("主攝影機")]
        [SerializeField] private Camera mainCamera;

        [Header("Input Actions")]
        [Tooltip("Input Actions Asset")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("系統設定")]
        [Tooltip("是否在開始時初始化所有系統")]
        [SerializeField] private bool initializeOnStart = true;

        #endregion

        #region 私有欄位

        private bool isPaused;
        private bool isInitialized;

        // Input Actions
        private InputAction pauseAction;
        private InputAction showRecipeAction;

        // 系統引用（將在階段 2 中實作）
        // private CocktailSystem cocktailSystem;
        // private InteractionSystem interactionSystem;
        // private NPCManager npcManager;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            // 單例設定
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            SetupInputActions();
        }

        private void Start()
        {
            if (initializeOnStart)
            {
                Initialize();
            }
        }

        private void OnEnable()
        {
            pauseAction?.Enable();
            showRecipeAction?.Enable();
        }

        private void OnDisable()
        {
            pauseAction?.Disable();
            showRecipeAction?.Disable();
        }

        private void Update()
        {
            // 處理暫停輸入
            if (pauseAction != null && pauseAction.WasPressedThisFrame())
            {
                TogglePause();
            }

            // 處理顯示食譜輸入
            if (showRecipeAction != null && showRecipeAction.WasPressedThisFrame())
            {
                ToggleRecipePanel();
            }

            // 如果暫停，不更新遊戲邏輯
            // 參考: index.js Line 96-99
            if (isPaused) return;

            // 遊戲主迴圈邏輯將在這裡更新
            // 各系統的 Update 由它們自己的 MonoBehaviour 處理
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化遊戲系統
        /// 參考: index.js init() Line 26-81
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("GameManager: 系統已經初始化");
                return;
            }

            Debug.Log("GameManager: 開始初始化遊戲系統...");

            // 1. 驗證必要引用
            ValidateReferences();

            // 2. 初始化玩家控制器
            InitializePlayer();

            // 3. 初始化系統（將在後續階段實作）
            // InitializePhysicsSystem();
            // InitializeInteractionSystem();
            // InitializeCocktailSystem();
            // InitializeEnvironment();
            // InitializeNPCManager();
            // InitializeLightingSystem();

            isInitialized = true;
            Debug.Log("GameManager: 遊戲系統初始化完成");
        }

        /// <summary>
        /// 驗證必要的引用
        /// </summary>
        private void ValidateReferences()
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<FirstPersonController>();
                if (playerController == null)
                {
                    Debug.LogError("GameManager: 找不到 FirstPersonController");
                }
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("GameManager: 找不到主攝影機");
                }
            }
        }

        /// <summary>
        /// 初始化玩家控制器
        /// </summary>
        private void InitializePlayer()
        {
            if (playerController != null)
            {
                playerController.EnableInput();
                playerController.LockCursor();
                Debug.Log("GameManager: 玩家控制器已初始化");
            }
        }

        #endregion

        #region Input Actions 設定

        /// <summary>
        /// 設定 Input Actions
        /// </summary>
        private void SetupInputActions()
        {
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("PlayerInputActions");
            }

            if (inputActions == null)
            {
                Debug.LogWarning("GameManager: 無法載入 InputActionAsset");
                return;
            }

            var uiMap = inputActions.FindActionMap("UI");
            if (uiMap != null)
            {
                pauseAction = uiMap.FindAction("Pause");
                showRecipeAction = uiMap.FindAction("ShowRecipe");
            }
        }

        #endregion

        #region 暫停/恢復

        /// <summary>
        /// 切換暫停狀態
        /// 參考: index.js toggleRecipePanel() Line 408-430
        /// </summary>
        public void TogglePause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        /// 暫停遊戲
        /// </summary>
        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;

            // 解鎖游標
            if (playerController != null)
            {
                playerController.UnlockCursor();
                playerController.DisableInput();
            }

            Debug.Log("GameManager: 遊戲已暫停");
        }

        /// <summary>
        /// 恢復遊戲
        /// </summary>
        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;

            // 鎖定游標
            if (playerController != null)
            {
                playerController.LockCursor();
                playerController.EnableInput();
            }

            Debug.Log("GameManager: 遊戲已恢復");
        }

        /// <summary>
        /// 切換食譜面板
        /// 參考: index.js toggleRecipePanel() Line 408-430
        /// </summary>
        public void ToggleRecipePanel()
        {
            // UI 系統將在階段 4 實作
            // 這裡先切換暫停狀態
            TogglePause();
            Debug.Log("GameManager: 切換食譜面板（暫停狀態）");
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 遊戲是否暫停
        /// </summary>
        public bool IsPaused => isPaused;

        /// <summary>
        /// 系統是否已初始化
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// 玩家控制器引用
        /// </summary>
        public FirstPersonController PlayerController => playerController;

        /// <summary>
        /// 主攝影機引用
        /// </summary>
        public Camera MainCamera => mainCamera;

        #endregion

        #region 公開方法

        /// <summary>
        /// 取得玩家位置
        /// </summary>
        public Vector3 GetPlayerPosition()
        {
            return playerController != null ? playerController.Position : Vector3.zero;
        }

        /// <summary>
        /// 取得攝影機前方方向
        /// </summary>
        public Vector3 GetCameraForward()
        {
            return mainCamera != null ? mainCamera.transform.forward : Vector3.forward;
        }

        #endregion
    }
}
