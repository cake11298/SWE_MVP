using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BarSimulator.Systems;

namespace BarSimulator.UI
{
    /// <summary>
    /// 主選單管理系統 - 處理主選單UI和場景切換
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        #region 序列化欄位

        [Header("Menu Panels")]
        [Tooltip("主選單面板")]
        [SerializeField] private GameObject mainMenuPanel;

        [Tooltip("新遊戲確認面板")]
        [SerializeField] private GameObject newGameConfirmPanel;

        [Header("Buttons")]
        [Tooltip("新遊戲按鈕")]
        [SerializeField] private Button newGameButton;

        [Tooltip("設定按鈕")]
        [SerializeField] private Button settingsButton;

        [Tooltip("離開遊戲按鈕")]
        [SerializeField] private Button quitButton;

        [Header("New Game Confirm Buttons")]
        [Tooltip("確認新遊戲按鈕")]
        [SerializeField] private Button confirmNewGameButton;

        [Tooltip("取消新遊戲按鈕")]
        [SerializeField] private Button cancelNewGameButton;

        [Header("UI References")]
        [Tooltip("存檔資訊文字")]
        [SerializeField] private TextMeshProUGUI saveInfoText;

        [Tooltip("版本文字")]
        [SerializeField] private TextMeshProUGUI versionText;

        [Header("Settings")]
        [Tooltip("遊戲版本號")]
        [SerializeField] private string gameVersion = "v0.1.0";

        [Tooltip("遊戲場景名稱")]
        [SerializeField] private string gameSceneName = "GameScene";

        #endregion

        #region 私有欄位

        private SaveLoadSystem saveLoadSystem;
        private SceneLoader sceneLoader;
        private SettingsManager settingsManager;

        #endregion

        #region Unity 生命週期

        private void Start()
        {
            // Get system references
            saveLoadSystem = SaveLoadSystem.Instance;
            sceneLoader = SceneLoader.Instance;
            settingsManager = SettingsManager.Instance;

            // Show cursor in main menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Resume time (in case we came from paused game)
            Time.timeScale = 1f;

            // Ensure UI is set up if references are assigned
            if (mainMenuPanel != null)
            {
                SetupUI();
                UpdateSaveInfo();
            }
        }

        /// <summary>
        /// 初始化 UI 引用（從外部注入）
        /// </summary>
        public void InitializeReferences(MainMenuUIReferences refs)
        {
            mainMenuPanel = refs.mainMenuPanel;
            newGameConfirmPanel = refs.newGameConfirmPanel;
            newGameButton = refs.newGameButton;
            settingsButton = refs.settingsButton;
            quitButton = refs.quitButton;
            confirmNewGameButton = refs.confirmNewGameButton;
            cancelNewGameButton = refs.cancelNewGameButton;
            saveInfoText = refs.saveInfoText;
            versionText = refs.versionText;

            SetupUI();
            UpdateSaveInfo();
            Debug.Log("MainMenuManager: UI references initialized");
        }

        #endregion

        #region 初始化

        private void SetupUI()
        {
            // Show main menu panel
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }

            // Hide confirm panel
            if (newGameConfirmPanel != null)
            {
                newGameConfirmPanel.SetActive(false);
            }

            // Setup main menu buttons
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(OnNewGameClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            // Setup confirm buttons
            if (confirmNewGameButton != null)
            {
                confirmNewGameButton.onClick.AddListener(OnConfirmNewGame);
            }

            if (cancelNewGameButton != null)
            {
                cancelNewGameButton.onClick.AddListener(OnCancelNewGame);
            }

            // Set version text
            if (versionText != null)
            {
                versionText.text = gameVersion;
            }
        }

        /// <summary>
        /// 更新存檔資訊顯示
        /// </summary>
        private void UpdateSaveInfo()
        {
            if (saveInfoText == null) return;
            if (saveLoadSystem == null) return;

            if (saveLoadSystem.HasSaveFile())
            {
                var saveInfo = saveLoadSystem.GetSaveFileInfo();
                if (saveInfo != null)
                {
                    saveInfoText.text = $"Last Save: {saveInfo.lastModified:yyyy-MM-dd HH:mm}";
                }
                else
                {
                    saveInfoText.text = "Save file exists";
                }
            }
            else
            {
                saveInfoText.text = "No save file";
            }
        }

        #endregion

        #region 按鈕處理

        /// <summary>
        /// 新遊戲按鈕點擊
        /// </summary>
        private void OnNewGameClicked()
        {
            Debug.Log("MainMenuManager: New Game clicked");

            // Check if save file exists
            if (saveLoadSystem != null && saveLoadSystem.HasSaveFile())
            {
                // Show confirmation dialog
                ShowNewGameConfirmation();
            }
            else
            {
                // No save file, start new game directly
                StartNewGame();
            }
        }

        /// <summary>
        /// 設定按鈕點擊
        /// </summary>
        private void OnSettingsClicked()
        {
            Debug.Log("MainMenuManager: Settings clicked");

            if (settingsManager != null)
            {
                settingsManager.OpenSettings();
            }
            else
            {
                Debug.LogWarning("MainMenuManager: SettingsManager not found");
            }
        }

        /// <summary>
        /// 離開遊戲按鈕點擊
        /// </summary>
        private void OnQuitClicked()
        {
            Debug.Log("MainMenuManager: Quit clicked");

            if (sceneLoader != null)
            {
                sceneLoader.QuitGame();
            }
            else
            {
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            }
        }

        #endregion

        #region 新遊戲確認

        /// <summary>
        /// 顯示新遊戲確認對話框
        /// </summary>
        private void ShowNewGameConfirmation()
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }

            if (newGameConfirmPanel != null)
            {
                newGameConfirmPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 確認新遊戲
        /// </summary>
        private void OnConfirmNewGame()
        {
            Debug.Log("MainMenuManager: Confirmed new game");

            // Delete old save
            if (saveLoadSystem != null)
            {
                saveLoadSystem.DeleteSaveFile();
            }

            StartNewGame();
        }

        /// <summary>
        /// 取消新遊戲
        /// </summary>
        private void OnCancelNewGame()
        {
            Debug.Log("MainMenuManager: Cancelled new game");

            if (newGameConfirmPanel != null)
            {
                newGameConfirmPanel.SetActive(false);
            }

            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
        }

        #endregion

        #region 場景切換

        /// <summary>
        /// 開始新遊戲
        /// </summary>
        private void StartNewGame()
        {
            Debug.Log("MainMenuManager: Starting new game");

            // Initialize default game state
            InitializeNewGameState();

            // Load game scene
            LoadGameScene();
        }

        /// <summary>
        /// 初始化新遊戲狀態
        /// </summary>
        private void InitializeNewGameState()
        {
            // Reset all game systems to default state
            // This will be saved when the player first saves in the game

            if (UpgradeSystem.Instance != null)
            {
                // Reset money to starting amount
                UpgradeSystem.Instance.SetMoney(1000);

                // Reset all liquors to default state (Level 1, unlocked base spirits)
                var liquorDB = UpgradeSystem.Instance.LiquorDatabase;
                if (liquorDB != null && liquorDB.liquors != null)
                {
                    foreach (var liquor in liquorDB.liquors)
                    {
                        liquor.level = 1;
                        // Base spirits are unlocked by default, others are locked
                        liquor.isLocked = liquor.category != Data.LiquorCategory.BaseSpirit;
                    }
                }

                // Lock all advanced recipes
                var recipeDB = UpgradeSystem.Instance.RecipeDatabase;
                if (recipeDB != null && recipeDB.recipes != null)
                {
                    foreach (var recipe in recipeDB.recipes)
                    {
                        // First 3 recipes are unlocked by default
                        recipe.isLocked = System.Array.IndexOf(recipeDB.recipes, recipe) >= 3;
                    }
                }
            }

            Debug.Log("MainMenuManager: New game state initialized");
        }

        /// <summary>
        /// 載入遊戲場景
        /// </summary>
        private void LoadGameScene()
        {
            if (sceneLoader != null)
            {
                sceneLoader.LoadScene(gameSceneName);
            }
            else
            {
                Debug.LogWarning("MainMenuManager: SceneLoader not found, using direct load");
                UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
            }
        }

        #endregion

        #region 公開方法

        /// <summary>
        /// 返回主選單（從遊戲內呼叫）
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("MainMenuManager: Returning to main menu");

            // Save game before returning
            if (saveLoadSystem != null)
            {
                saveLoadSystem.SaveGame();
            }

            // Load main menu scene
            if (sceneLoader != null)
            {
                sceneLoader.LoadMainMenu();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        #endregion
    }
}
