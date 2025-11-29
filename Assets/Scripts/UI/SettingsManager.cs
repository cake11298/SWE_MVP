using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using BarSimulator.Systems;

namespace BarSimulator.UI
{
    /// <summary>
    /// 設定管理系統 - 處理遊戲設定（音量、畫質、控制等）
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        #region Singleton

        private static SettingsManager instance;
        public static SettingsManager Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("UI References")]
        [Tooltip("設定面板")]
        [SerializeField] private GameObject settingsPanel;

        [Header("Audio Settings")]
        [Tooltip("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Tooltip("主音量滑桿")]
        [SerializeField] private Slider masterVolumeSlider;

        [Tooltip("音樂音量滑桿")]
        [SerializeField] private Slider musicVolumeSlider;

        [Tooltip("音效音量滑桿")]
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Graphics Settings")]
        [Tooltip("畫質下拉選單")]
        [SerializeField] private TMP_Dropdown qualityDropdown;

        [Tooltip("全螢幕切換")]
        [SerializeField] private Toggle fullscreenToggle;

        [Tooltip("VSync切換")]
        [SerializeField] private Toggle vsyncToggle;

        [Header("Control Settings")]
        [Tooltip("滑鼠靈敏度滑桿")]
        [SerializeField] private Slider mouseSensitivitySlider;

        [Tooltip("滑鼠靈敏度值顯示")]
        [SerializeField] private TextMeshProUGUI sensitivityValueText;

        [Header("Buttons")]
        [Tooltip("套用按鈕")]
        [SerializeField] private Button applyButton;

        [Tooltip("重設按鈕")]
        [SerializeField] private Button resetButton;

        [Tooltip("關閉按鈕")]
        [SerializeField] private Button closeButton;

        #endregion

        #region 私有欄位

        private bool isOpen = false;

        // Current settings
        private float masterVolume = 1.0f;
        private float musicVolume = 0.7f;
        private float sfxVolume = 1.0f;
        private float mouseSensitivity = 1.0f;
        private int qualityLevel = 2;
        private bool isFullscreen = true;
        private bool useVSync = true;

        // Events
        public System.Action OnSettingsChanged;
        public System.Action<float> OnMasterVolumeChanged;
        public System.Action<float> OnMusicVolumeChanged;
        public System.Action<float> OnSFXVolumeChanged;
        public System.Action<float> OnMouseSensitivityChanged;

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

            LoadSettings();
        }

        /// <summary>
        /// 初始化 UI 引用（從外部注入）
        /// </summary>
        public void InitializeReferences(SettingsUIReferences refs)
        {
            settingsPanel = refs.settingsPanel;
            masterVolumeSlider = refs.masterVolumeSlider;
            musicVolumeSlider = refs.musicVolumeSlider;
            sfxVolumeSlider = refs.sfxVolumeSlider;
            qualityDropdown = refs.qualityDropdown;
            fullscreenToggle = refs.fullscreenToggle;
            vsyncToggle = refs.vsyncToggle;
            mouseSensitivitySlider = refs.mouseSensitivitySlider;
            sensitivityValueText = refs.sensitivityValueText;
            applyButton = refs.applyButton;
            resetButton = refs.resetButton;
            closeButton = refs.closeButton;

            SetupUI();
            Debug.Log("SettingsManager: UI references initialized");
        }

        private void Start()
        {
            ApplyAllSettings();
        }

        #endregion

        #region 初始化

        private void SetupUI()
        {
            // Hide settings panel initially
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // Setup audio sliders
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeSliderChanged);
            }

            // Setup graphics controls
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
                qualityDropdown.onValueChanged.AddListener(OnQualityDropdownChanged);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
            }

            if (vsyncToggle != null)
            {
                vsyncToggle.onValueChanged.AddListener(OnVSyncToggleChanged);
            }

            // Setup control settings
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivitySliderChanged);
            }

            // Setup buttons
            if (applyButton != null)
            {
                applyButton.onClick.AddListener(ApplyAllSettings);
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(ResetToDefaults);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseSettings);
            }
        }

        #endregion

        #region 開關控制

        /// <summary>
        /// 開啟設定面板
        /// </summary>
        public void OpenSettings()
        {
            if (settingsPanel == null) return;

            isOpen = true;
            settingsPanel.SetActive(true);

            // Update UI to reflect current settings
            UpdateUIValues();

            // Pause game
            Time.timeScale = 0f;

            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("SettingsManager: Opened");
        }

        /// <summary>
        /// 關閉設定面板
        /// </summary>
        public void CloseSettings()
        {
            if (settingsPanel == null) return;

            isOpen = false;
            settingsPanel.SetActive(false);

            // Resume game
            Time.timeScale = 1f;

            // Hide cursor (unless in main menu)
            if (SceneLoader.Instance != null && SceneLoader.Instance.CurrentSceneName != "MainMenu")
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            Debug.Log("SettingsManager: Closed");
        }

        /// <summary>
        /// 切換設定面板
        /// </summary>
        public void ToggleSettings()
        {
            if (isOpen)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }

        #endregion

        #region 音量設定

        private void OnMasterVolumeSliderChanged(float value)
        {
            masterVolume = value;
            SetMasterVolume(value);
        }

        private void OnMusicVolumeSliderChanged(float value)
        {
            musicVolume = value;
            SetMusicVolume(value);
        }

        private void OnSFXVolumeSliderChanged(float value)
        {
            sfxVolume = value;
            SetSFXVolume(value);
        }

        /// <summary>
        /// 設定主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            if (audioMixer != null)
            {
                // Convert 0-1 to -80dB to 0dB (logarithmic scale)
                float db = masterVolume > 0 ? 20f * Mathf.Log10(masterVolume) : -80f;
                audioMixer.SetFloat("MasterVolume", db);
            }
            else
            {
                AudioListener.volume = masterVolume;
            }
            OnMasterVolumeChanged?.Invoke(masterVolume);
        }

        /// <summary>
        /// 設定音樂音量
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (audioMixer != null)
            {
                float db = musicVolume > 0 ? 20f * Mathf.Log10(musicVolume) : -80f;
                audioMixer.SetFloat("MusicVolume", db);
            }
            OnMusicVolumeChanged?.Invoke(musicVolume);
        }

        /// <summary>
        /// 設定音效音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (audioMixer != null)
            {
                float db = sfxVolume > 0 ? 20f * Mathf.Log10(sfxVolume) : -80f;
                audioMixer.SetFloat("SFXVolume", db);
            }
            OnSFXVolumeChanged?.Invoke(sfxVolume);
        }

        #endregion

        #region 畫質設定

        private void OnQualityDropdownChanged(int index)
        {
            qualityLevel = index;
        }

        private void OnFullscreenToggleChanged(bool value)
        {
            isFullscreen = value;
        }

        private void OnVSyncToggleChanged(bool value)
        {
            useVSync = value;
        }

        /// <summary>
        /// 設定畫質等級
        /// </summary>
        public void SetQualityLevel(int level)
        {
            qualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(qualityLevel);
            Debug.Log($"SettingsManager: Quality set to {QualitySettings.names[qualityLevel]}");
        }

        /// <summary>
        /// 設定全螢幕
        /// </summary>
        public void SetFullscreen(bool fullscreen)
        {
            isFullscreen = fullscreen;
            Screen.fullScreen = isFullscreen;
            Debug.Log($"SettingsManager: Fullscreen set to {isFullscreen}");
        }

        /// <summary>
        /// 設定VSync
        /// </summary>
        public void SetVSync(bool enabled)
        {
            useVSync = enabled;
            QualitySettings.vSyncCount = useVSync ? 1 : 0;
            Debug.Log($"SettingsManager: VSync set to {useVSync}");
        }

        #endregion

        #region 控制設定

        private void OnMouseSensitivitySliderChanged(float value)
        {
            mouseSensitivity = value;
            if (sensitivityValueText != null)
            {
                sensitivityValueText.text = $"{value:F2}";
            }
        }

        /// <summary>
        /// 設定滑鼠靈敏度
        /// </summary>
        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 3.0f);
            OnMouseSensitivityChanged?.Invoke(mouseSensitivity);
            Debug.Log($"SettingsManager: Mouse sensitivity set to {mouseSensitivity:F2}");
        }

        #endregion

        #region 套用和重設

        /// <summary>
        /// 套用所有設定
        /// </summary>
        public void ApplyAllSettings()
        {
            SetMasterVolume(masterVolume);
            SetMusicVolume(musicVolume);
            SetSFXVolume(sfxVolume);
            SetQualityLevel(qualityLevel);
            SetFullscreen(isFullscreen);
            SetVSync(useVSync);
            SetMouseSensitivity(mouseSensitivity);

            OnSettingsChanged?.Invoke();

            // Save settings
            SaveSettings();

            Debug.Log("SettingsManager: All settings applied");
        }

        /// <summary>
        /// 重設為預設值
        /// </summary>
        public void ResetToDefaults()
        {
            masterVolume = 1.0f;
            musicVolume = 0.7f;
            sfxVolume = 1.0f;
            mouseSensitivity = 1.0f;
            qualityLevel = 2;
            isFullscreen = true;
            useVSync = true;

            UpdateUIValues();
            ApplyAllSettings();

            Debug.Log("SettingsManager: Reset to defaults");
        }

        /// <summary>
        /// 更新UI顯示值
        /// </summary>
        private void UpdateUIValues()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(masterVolume);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(musicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
            }

            if (qualityDropdown != null)
            {
                qualityDropdown.SetValueWithoutNotify(qualityLevel);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);
            }

            if (vsyncToggle != null)
            {
                vsyncToggle.SetIsOnWithoutNotify(useVSync);
            }

            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.SetValueWithoutNotify(mouseSensitivity);
            }

            if (sensitivityValueText != null)
            {
                sensitivityValueText.text = $"{mouseSensitivity:F2}";
            }
        }

        #endregion

        #region 存檔/讀檔

        /// <summary>
        /// 儲存設定到 PlayerPrefs
        /// </summary>
        private void SaveSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
            PlayerPrefs.SetInt("QualityLevel", qualityLevel);
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
            PlayerPrefs.SetInt("VSync", useVSync ? 1 : 0);
            PlayerPrefs.Save();

            Debug.Log("SettingsManager: Settings saved");
        }

        /// <summary>
        /// 從 PlayerPrefs 載入設定
        /// </summary>
        private void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
            mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
            qualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
            isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            useVSync = PlayerPrefs.GetInt("VSync", 1) == 1;

            Debug.Log("SettingsManager: Settings loaded");
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 設定面板是否開啟
        /// </summary>
        public bool IsOpen => isOpen;

        /// <summary>
        /// 滑鼠靈敏度
        /// </summary>
        public float MouseSensitivity => mouseSensitivity;

        /// <summary>
        /// 主音量
        /// </summary>
        public float MasterVolume => masterVolume;

        /// <summary>
        /// 音樂音量
        /// </summary>
        public float MusicVolume => musicVolume;

        /// <summary>
        /// 音效音量
        /// </summary>
        public float SFXVolume => sfxVolume;

        #endregion
    }
}
