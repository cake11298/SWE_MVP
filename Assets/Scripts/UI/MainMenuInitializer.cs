using UnityEngine;
using BarSimulator.Systems;
using BarSimulator.Managers;

namespace BarSimulator.UI
{
    /// <summary>
    /// MainMenu 場景初始化器
    /// 負責在場景載入時設置所有必要的系統和管理器
    /// 使用標準 Unity UI 流程（Scene 中的 Canvas），不使用程式化生成
    /// </summary>
    public class MainMenuInitializer : MonoBehaviour
    {
        #region 序列化欄位

        [Header("系統引用")]
        [Tooltip("主選單管理器")]
        [SerializeField] private MainMenuManager mainMenuManager;

        [Tooltip("設定管理器")]
        [SerializeField] private SettingsManager settingsManager;

        [Header("場景設定")]
        [Tooltip("背景音樂（可選）")]
        [SerializeField] private AudioClip menuMusic;

        [Tooltip("自動初始化")]
        [SerializeField] private bool autoInitialize = true;

        #endregion

        #region Unity 生命週期

        private void Start()
        {
            if (autoInitialize)
            {
                Initialize();
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化主選單場景
        /// </summary>
        public void Initialize()
        {
            Debug.Log("MainMenuInitializer: 初始化主選單場景...");

            // 1. 確保必要的系統存在
            EnsureCoreSystems();

            // 2. 設置游標
            SetupCursor();

            // 3. 設置音樂
            SetupMusic();

            // 4. 驗證管理器引用
            ValidateManagers();

            Debug.Log("MainMenuInitializer: 主選單場景初始化完成");
        }

        /// <summary>
        /// 確保核心系統存在
        /// </summary>
        private void EnsureCoreSystems()
        {
            // SceneLoader（必須持久化）
            if (SceneLoader.Instance == null)
            {
                var loaderObj = new GameObject("SceneLoader");
                loaderObj.AddComponent<SceneLoader>();
                DontDestroyOnLoad(loaderObj);
                Debug.Log("MainMenuInitializer: 創建 SceneLoader");
            }

            // SaveLoadSystem（必須持久化）
            if (SaveLoadSystem.Instance == null)
            {
                var saveSystemObj = new GameObject("SaveLoadSystem");
                saveSystemObj.AddComponent<SaveLoadSystem>();
                DontDestroyOnLoad(saveSystemObj);
                Debug.Log("MainMenuInitializer: 創建 SaveLoadSystem");
            }

            // SettingsManager（必須持久化）
            if (SettingsManager.Instance == null && settingsManager == null)
            {
                var settingsObj = new GameObject("SettingsManager");
                settingsManager = settingsObj.AddComponent<SettingsManager>();
                DontDestroyOnLoad(settingsObj);
                Debug.Log("MainMenuInitializer: 創建 SettingsManager");
            }

            // UpgradeSystem（必須持久化，用於存檔資訊）
            if (UpgradeSystem.Instance == null)
            {
                var upgradeObj = new GameObject("UpgradeSystem");
                upgradeObj.AddComponent<UpgradeSystem>();
                DontDestroyOnLoad(upgradeObj);
                Debug.Log("MainMenuInitializer: 創建 UpgradeSystem");
            }
        }

        /// <summary>
        /// 設置游標
        /// </summary>
        private void SetupCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 設置背景音樂
        /// </summary>
        private void SetupMusic()
        {
            if (menuMusic != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(menuMusic);
            }
        }

        /// <summary>
        /// 驗證管理器引用
        /// </summary>
        private void ValidateManagers()
        {
            if (mainMenuManager == null)
            {
                mainMenuManager = FindAnyObjectByType<MainMenuManager>();
                if (mainMenuManager == null)
                {
                    Debug.LogError("MainMenuInitializer: 找不到 MainMenuManager！請確保場景中有此組件。");
                }
            }

            if (settingsManager == null)
            {
                settingsManager = SettingsManager.Instance;
                if (settingsManager == null)
                {
                    Debug.LogWarning("MainMenuInitializer: 找不到 SettingsManager");
                }
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
            Initialize();
        }

        #endregion
    }
}
