using UnityEngine;
using UnityEngine.InputSystem;
using BarSimulator.Player;
using System;

namespace BarSimulator.Core
{
    /// <summary>
    /// 遊戲狀態枚舉
    /// </summary>
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// 遊戲分數資料
    /// </summary>
    [Serializable]
    public class GameScore
    {
        public int satisfiedDrinks;
        public int totalScore;
        public int totalDrinks;
        public int targetSatisfied = 5;
        public int satisfactionThreshold = 70; // 70/100 分算滿意

        public void Reset()
        {
            satisfiedDrinks = 0;
            totalScore = 0;
            totalDrinks = 0;
        }
    }

    /// <summary>
    /// 遊戲管理器 - 管理遊戲主迴圈、系統初始化、暫停/恢復、分數追蹤
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
                }
                return instance;
            }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 遊戲狀態改變事件
        /// </summary>
        public event Action<GameState> OnGameStateChanged;

        /// <summary>
        /// 分數更新事件
        /// </summary>
        public event Action<GameScore> OnScoreUpdated;

        /// <summary>
        /// 遊戲勝利事件
        /// </summary>
        public event Action OnGameWon;

        /// <summary>
        /// 飲品評分事件
        /// </summary>
        public event Action<int, string> OnDrinkRated;

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

        [Header("遊戲設定")]
        [Tooltip("目標滿意飲品數")]
        [SerializeField] private int targetSatisfiedDrinks = 5;

        [Tooltip("滿意分數門檻 (0-100)")]
        [SerializeField] private int satisfactionThreshold = 70;

        #endregion

        #region 私有欄位

        private GameState currentGameState = GameState.Menu;
        private GameScore score = new GameScore();
        private bool isInitialized;

        // Input Actions
        private InputAction pauseAction;
        private InputAction showRecipeAction;

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

            // 初始化分數設定
            score.targetSatisfied = targetSatisfiedDrinks;
            score.satisfactionThreshold = satisfactionThreshold;
        }

        private void Start()
        {
            // 開始時顯示主選單狀態
            SetGameState(GameState.Menu);

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
            // 只有在遊戲中才處理暫停
            if (currentGameState == GameState.Playing || currentGameState == GameState.Paused)
            {
                if (pauseAction != null && pauseAction.WasPressedThisFrame())
                {
                    TogglePause();
                }

                if (showRecipeAction != null && showRecipeAction.WasPressedThisFrame())
                {
                    ToggleRecipePanel();
                }
            }

            // 如果不是遊玩狀態，不更新遊戲邏輯
            if (currentGameState != GameState.Playing) return;
        }

        #endregion

        #region 遊戲狀態管理

        /// <summary>
        /// 設定遊戲狀態
        /// </summary>
        public void SetGameState(GameState newState)
        {
            if (currentGameState == newState) return;

            GameState previousState = currentGameState;
            currentGameState = newState;

            Debug.Log($"GameManager: 遊戲狀態從 {previousState} 變為 {newState}");

            switch (newState)
            {
                case GameState.Menu:
                    Time.timeScale = 1f;
                    UnlockCursor();
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    LockCursor();
                    if (playerController != null)
                    {
                        playerController.EnableInput();
                    }
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    UnlockCursor();
                    break;

                case GameState.GameOver:
                    Time.timeScale = 1f;
                    UnlockCursor();
                    break;
            }

            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// 開始遊戲
        /// </summary>
        public void StartGame()
        {
            Debug.Log("GameManager: 開始遊戲");

            // 重置分數
            score.Reset();
            OnScoreUpdated?.Invoke(score);

            // 重置玩家位置
            if (playerController != null)
            {
                playerController.ResetPosition();
            }

            SetGameState(GameState.Playing);
        }

        /// <summary>
        /// 返回主選單
        /// </summary>
        public void ReturnToMenu()
        {
            Debug.Log("GameManager: 返回主選單");
            SetGameState(GameState.Menu);
        }

        /// <summary>
        /// 遊戲結束
        /// </summary>
        public void EndGame(bool won)
        {
            Debug.Log($"GameManager: 遊戲結束 - {(won ? "勝利" : "結束")}");

            if (won)
            {
                OnGameWon?.Invoke();
            }

            SetGameState(GameState.GameOver);
        }

        /// <summary>
        /// 重新開始遊戲
        /// </summary>
        public void RestartGame()
        {
            Debug.Log("GameManager: 重新開始遊戲");
            StartGame();
        }

        #endregion

        #region 分數系統

        /// <summary>
        /// 添加飲品評分
        /// </summary>
        /// <param name="rating">評分 (0-100)</param>
        /// <param name="npcName">NPC 名稱</param>
        public void AddDrinkScore(int rating, string npcName)
        {
            score.totalDrinks++;
            score.totalScore += rating;

            // 檢查是否滿意 (>= 門檻分數)
            if (rating >= score.satisfactionThreshold)
            {
                score.satisfiedDrinks++;
                Debug.Log($"GameManager: {npcName} 對飲品滿意！({score.satisfiedDrinks}/{score.targetSatisfied})");
            }
            else
            {
                Debug.Log($"GameManager: {npcName} 對飲品不太滿意 ({rating}分)");
            }

            // 觸發事件
            OnScoreUpdated?.Invoke(score);
            OnDrinkRated?.Invoke(rating, npcName);

            // 檢查勝利條件
            if (score.satisfiedDrinks >= score.targetSatisfied)
            {
                // 延遲結束，讓玩家看到最後的評分
                Invoke(nameof(TriggerWin), 2f);
            }
        }

        private void TriggerWin()
        {
            EndGame(true);
        }

        /// <summary>
        /// 取得當前分數
        /// </summary>
        public GameScore GetScore()
        {
            return score;
        }

        /// <summary>
        /// 將 0-100 分轉換為 0-10 星級
        /// </summary>
        public int ConvertToStarRating(int score100)
        {
            return Mathf.RoundToInt(score100 / 10f);
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化遊戲系統
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("GameManager: 系統已經初始化");
                return;
            }

            Debug.Log("GameManager: 開始初始化遊戲系統...");

            ValidateReferences();
            InitializePlayer();

            isInitialized = true;
            Debug.Log("GameManager: 遊戲系統初始化完成");
        }

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

        private void InitializePlayer()
        {
            if (playerController != null)
            {
                // 不自動啟用輸入，等待遊戲開始
                Debug.Log("GameManager: 玩家控制器已初始化");
            }
        }

        #endregion

        #region Input Actions 設定

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
        /// </summary>
        public void TogglePause()
        {
            if (currentGameState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
            }
            else if (currentGameState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
            }
        }

        /// <summary>
        /// 暫停遊戲
        /// </summary>
        public void Pause()
        {
            if (currentGameState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
            }
        }

        /// <summary>
        /// 恢復遊戲
        /// </summary>
        public void Resume()
        {
            if (currentGameState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
            }
        }

        /// <summary>
        /// 切換食譜面板
        /// </summary>
        public void ToggleRecipePanel()
        {
            TogglePause();
            Debug.Log("GameManager: 切換食譜面板");
        }

        #endregion

        #region 游標控制

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        #endregion

        #region 公開屬性

        public bool IsPaused => currentGameState == GameState.Paused;
        public bool IsPlaying => currentGameState == GameState.Playing;
        public bool IsInitialized => isInitialized;
        public GameState CurrentGameState => currentGameState;
        public FirstPersonController PlayerController => playerController;
        public Camera MainCamera => mainCamera;

        #endregion

        #region 公開方法

        public Vector3 GetPlayerPosition()
        {
            return playerController != null ? playerController.Position : Vector3.zero;
        }

        public Vector3 GetCameraForward()
        {
            return mainCamera != null ? mainCamera.transform.forward : Vector3.forward;
        }

        #endregion
    }
}
