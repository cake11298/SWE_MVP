using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BarSimulator.Core;
using BarSimulator.Managers;

namespace BarSimulator.UI
{
    /// <summary>
    /// 遊戲 UI 控制器 - 管理所有遊戲選單和 HUD
    /// 參考: src/index.js 和 src/index.html
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        #region 單例模式

        private static GameUIController instance;
        public static GameUIController Instance => instance;

        #endregion

        #region 序列化欄位 - 面板引用

        [Header("主選單")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;

        [Header("教學面板")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private Button tutorialBackButton;
        [SerializeField] private Button tutorialStartButton;

        [Header("設定面板")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;
        [SerializeField] private TextMeshProUGUI musicVolumeText;
        [SerializeField] private TextMeshProUGUI sensitivityText;
        [SerializeField] private Button settingsBackButton;

        [Header("製作人員面板")]
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private Button creditsBackButton;

        [Header("暫停選單")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button pauseMenuButton;

        [Header("遊戲結束面板")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverTitle;
        [SerializeField] private TextMeshProUGUI finalSatisfiedText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI finalDrinksText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button backToMenuButton;

        [Header("遊戲中 HUD")]
        [SerializeField] private GameObject gameHUDPanel;
        [SerializeField] private TextMeshProUGUI satisfiedCountText;
        [SerializeField] private TextMeshProUGUI totalScoreText;

        [Header("評分彈窗")]
        [SerializeField] private GameObject ratingPopupPanel;
        [SerializeField] private TextMeshProUGUI ratingStarsText;
        [SerializeField] private TextMeshProUGUI ratingMessageText;

        [Header("控制提示")]
        [SerializeField] private GameObject controlsHint;

        #endregion

        #region 私有欄位

        private float ratingPopupTimer;
        private const float RATING_POPUP_DURATION = 3f;

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

            SetupButtonListeners();
            SetupSliderListeners();
        }

        private void Start()
        {
            // 訂閱遊戲事件
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
                GameManager.Instance.OnScoreUpdated += HandleScoreUpdated;
                GameManager.Instance.OnDrinkRated += HandleDrinkRated;
            }

            // 初始顯示主選單
            ShowMainMenu();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
                GameManager.Instance.OnScoreUpdated -= HandleScoreUpdated;
                GameManager.Instance.OnDrinkRated -= HandleDrinkRated;
            }
        }

        private void Update()
        {
            // 處理評分彈窗計時
            if (ratingPopupPanel != null && ratingPopupPanel.activeSelf)
            {
                ratingPopupTimer -= Time.deltaTime;
                if (ratingPopupTimer <= 0)
                {
                    ratingPopupPanel.SetActive(false);
                }
            }
        }

        #endregion

        #region 設定監聽器

        private void SetupButtonListeners()
        {
            // 主選單按鈕
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);
            if (tutorialButton != null)
                tutorialButton.onClick.AddListener(OnTutorialClicked);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
            if (creditsButton != null)
                creditsButton.onClick.AddListener(OnCreditsClicked);

            // 教學面板按鈕
            if (tutorialBackButton != null)
                tutorialBackButton.onClick.AddListener(OnTutorialBackClicked);
            if (tutorialStartButton != null)
                tutorialStartButton.onClick.AddListener(OnTutorialStartClicked);

            // 設定面板按鈕
            if (settingsBackButton != null)
                settingsBackButton.onClick.AddListener(OnSettingsBackClicked);

            // 製作人員面板按鈕
            if (creditsBackButton != null)
                creditsBackButton.onClick.AddListener(OnCreditsBackClicked);

            // 暫停選單按鈕
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
            if (pauseRestartButton != null)
                pauseRestartButton.onClick.AddListener(OnPauseRestartClicked);
            if (pauseMenuButton != null)
                pauseMenuButton.onClick.AddListener(OnPauseMenuClicked);

            // 遊戲結束按鈕
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);
            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        }

        private void SetupSliderListeners()
        {
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
                sfxVolumeSlider.value = 0.7f;
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
                musicVolumeSlider.value = 0.5f;
            }

            if (sensitivitySlider != null)
            {
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
                sensitivitySlider.value = 5f;
            }
        }

        #endregion

        #region 事件處理

        private void HandleGameStateChanged(GameState newState)
        {
            HideAllPanels();

            switch (newState)
            {
                case GameState.Menu:
                    ShowMainMenu();
                    break;

                case GameState.Playing:
                    ShowGameHUD();
                    break;

                case GameState.Paused:
                    ShowPauseMenu();
                    break;

                case GameState.GameOver:
                    ShowGameOver();
                    break;
            }
        }

        private void HandleScoreUpdated(GameScore score)
        {
            UpdateScoreUI(score);
        }

        private void HandleDrinkRated(int rating, string npcName)
        {
            ShowRatingPopup(rating, npcName);
        }

        #endregion

        #region 面板顯示控制

        private void HideAllPanels()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(tutorialPanel, false);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(creditsPanel, false);
            SetPanelActive(pauseMenuPanel, false);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(gameHUDPanel, false);
            SetPanelActive(controlsHint, false);
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

        public void ShowMainMenu()
        {
            HideAllPanels();
            SetPanelActive(mainMenuPanel, true);
        }

        public void ShowTutorial()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(tutorialPanel, true);
        }

        public void ShowSettings()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(settingsPanel, true);
        }

        public void ShowCredits()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(creditsPanel, true);
        }

        public void ShowPauseMenu()
        {
            SetPanelActive(pauseMenuPanel, true);
            SetPanelActive(gameHUDPanel, false);
        }

        public void ShowGameHUD()
        {
            HideAllPanels();
            SetPanelActive(gameHUDPanel, true);
            SetPanelActive(controlsHint, true);
        }

        public void ShowGameOver()
        {
            HideAllPanels();
            SetPanelActive(gameOverPanel, true);

            // 更新遊戲結束資訊
            if (GameManager.Instance != null)
            {
                GameScore score = GameManager.Instance.GetScore();
                bool won = score.satisfiedDrinks >= score.targetSatisfied;

                if (gameOverTitle != null)
                    gameOverTitle.text = won ? "恭喜過關！" : "遊戲結束";

                if (finalSatisfiedText != null)
                    finalSatisfiedText.text = $"{score.satisfiedDrinks} / {score.targetSatisfied}";

                if (finalScoreText != null)
                    finalScoreText.text = score.totalScore.ToString();

                if (finalDrinksText != null)
                    finalDrinksText.text = score.totalDrinks.ToString();
            }
        }

        #endregion

        #region 分數 UI 更新

        private void UpdateScoreUI(GameScore score)
        {
            if (satisfiedCountText != null)
                satisfiedCountText.text = $"{score.satisfiedDrinks} / {score.targetSatisfied}";

            if (totalScoreText != null)
                totalScoreText.text = score.totalScore.ToString();
        }

        public void ShowRatingPopup(int rating, string npcName)
        {
            if (ratingPopupPanel == null) return;

            // 生成星星顯示 (轉換 0-100 為 0-10)
            int starRating = Mathf.RoundToInt(rating / 10f);
            string stars = "";

            for (int i = 0; i < starRating / 2; i++)
            {
                stars += "★";
            }
            if (starRating % 2 == 1)
            {
                stars += "☆";
            }

            if (string.IsNullOrEmpty(stars))
            {
                stars = "☆";
            }

            // 評價文字
            string comment;
            if (rating >= 90)
                comment = "完美！";
            else if (rating >= 70)
                comment = "很棒！";
            else if (rating >= 50)
                comment = "還可以";
            else
                comment = "需要改進";

            if (ratingStarsText != null)
                ratingStarsText.text = stars;

            if (ratingMessageText != null)
                ratingMessageText.text = $"{npcName}: {rating}/100 - {comment}";

            ratingPopupPanel.SetActive(true);
            ratingPopupTimer = RATING_POPUP_DURATION;

            // 播放音效
            if (AudioManager.Instance != null)
            {
                if (rating >= 70)
                    AudioManager.Instance.PlaySFX("success");
                else
                    AudioManager.Instance.PlaySFX("fail");
            }
        }

        #endregion

        #region 按鈕回調

        private void OnStartGameClicked()
        {
            PlayButtonSound();
            GameManager.Instance?.StartGame();
        }

        private void OnTutorialClicked()
        {
            PlayButtonSound();
            ShowTutorial();
        }

        private void OnSettingsClicked()
        {
            PlayButtonSound();
            ShowSettings();
        }

        private void OnCreditsClicked()
        {
            PlayButtonSound();
            ShowCredits();
        }

        private void OnTutorialBackClicked()
        {
            PlayButtonSound();
            SetPanelActive(tutorialPanel, false);
            SetPanelActive(mainMenuPanel, true);
        }

        private void OnTutorialStartClicked()
        {
            PlayButtonSound();
            GameManager.Instance?.StartGame();
        }

        private void OnSettingsBackClicked()
        {
            PlayButtonSound();
            SetPanelActive(settingsPanel, false);
            SetPanelActive(mainMenuPanel, true);
        }

        private void OnCreditsBackClicked()
        {
            PlayButtonSound();
            SetPanelActive(creditsPanel, false);
            SetPanelActive(mainMenuPanel, true);
        }

        private void OnResumeClicked()
        {
            PlayButtonSound();
            GameManager.Instance?.Resume();
        }

        private void OnPauseRestartClicked()
        {
            PlayButtonSound();
            GameManager.Instance?.RestartGame();
        }

        private void OnPauseMenuClicked()
        {
            PlayButtonSound();
            GameManager.Instance?.ReturnToMenu();
        }

        private void OnPlayAgainClicked()
        {
            PlayButtonSound();
            GameManager.Instance?.RestartGame();
        }

        private void OnBackToMenuClicked()
        {
            PlayButtonSound();
            GameManager.Instance?.ReturnToMenu();
        }

        #endregion

        #region 滑桿回調

        private void OnSFXVolumeChanged(float value)
        {
            if (sfxVolumeText != null)
                sfxVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetSFXVolume(value);
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (musicVolumeText != null)
                musicVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMusicVolume(value);
        }

        private void OnSensitivityChanged(float value)
        {
            if (sensitivityText != null)
                sensitivityText.text = value.ToString("F1");

            if (GameManager.Instance?.PlayerController != null)
                GameManager.Instance.PlayerController.SetMouseSensitivity(value / 500f);
        }

        #endregion

        #region 輔助方法

        private void PlayButtonSound()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("ui_click");
        }

        #endregion
    }
}
