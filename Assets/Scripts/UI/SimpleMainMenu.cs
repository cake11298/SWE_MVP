using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace UI
{
    /// <summary>
    /// Main Menu logic - handles scene loading and application quit
    /// </summary>
    public class SimpleMainMenu : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private TMP_Dropdown qualityDropdown;

        private void Start()
        {
            // Initialize Quality Dropdown
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string> { "Low", "Medium", "High" });
                qualityDropdown.value = QualitySettings.GetQualityLevel();
                qualityDropdown.onValueChanged.AddListener(SetQuality);
            }

            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        public void ToggleSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(!settingsPanel.activeSelf);
        }

        public void SetQuality(int index)
        {
            QualitySettings.SetQualityLevel(index, true);
        }

        /// <summary>
        /// Loads the main game scene "TheBar"
        /// </summary>
        public void StartGame()
        {
            // Load TheBar scene - using scene name from build settings
            SceneManager.LoadScene("TheBar");
        }

        /// <summary>
        /// Quits the application
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
