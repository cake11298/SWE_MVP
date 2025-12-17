using UnityEngine;
using UnityEngine.UI;

namespace BarSimulator.UI
{
    /// <summary>
    /// Settings UI - Quality settings control
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Dropdown qualityDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Dropdown resolutionDropdown;
        [SerializeField] private Button closeButton;

        private Resolution[] resolutions;

        private void Start()
        {
            // Hide panel initially
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // Setup quality dropdown
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
                qualityDropdown.value = QualitySettings.GetQualityLevel();
                qualityDropdown.onValueChanged.AddListener(SetQuality);
            }

            // Setup fullscreen toggle
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }

            // Setup resolution dropdown
            if (resolutionDropdown != null)
            {
                resolutions = Screen.resolutions;
                resolutionDropdown.ClearOptions();

                var options = new System.Collections.Generic.List<string>();
                int currentResolutionIndex = 0;

                for (int i = 0; i < resolutions.Length; i++)
                {
                    string option = resolutions[i].width + " x " + resolutions[i].height;
                    options.Add(option);

                    if (resolutions[i].width == Screen.currentResolution.width &&
                        resolutions[i].height == Screen.currentResolution.height)
                    {
                        currentResolutionIndex = i;
                    }
                }

                resolutionDropdown.AddOptions(options);
                resolutionDropdown.value = currentResolutionIndex;
                resolutionDropdown.RefreshShownValue();
                resolutionDropdown.onValueChanged.AddListener(SetResolution);
            }

            // Setup close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseSettings);
            }
        }

        private void OnDestroy()
        {
            if (qualityDropdown != null)
                qualityDropdown.onValueChanged.RemoveListener(SetQuality);
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
            if (resolutionDropdown != null)
                resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
            if (closeButton != null)
                closeButton.onClick.RemoveListener(CloseSettings);
        }

        public void ShowSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        private void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            Debug.Log($"SettingsUI: Quality set to {QualitySettings.names[qualityIndex]}");
        }

        private void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            Debug.Log($"SettingsUI: Fullscreen set to {isFullscreen}");
        }

        private void SetResolution(int resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            Debug.Log($"SettingsUI: Resolution set to {resolution.width}x{resolution.height}");
        }
    }
}
