using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    /// <summary>
    /// Pause Menu logic - handles pause/resume and menu navigation
    /// </summary>
    public class SimplePauseMenu : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The pause menu panel to show/hide")]
        [SerializeField] private GameObject pausePanel;

        private bool isPaused = false;

        private void Start()
        {
            // Ensure the pause panel is hidden at start
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
            
            // Ensure game is running
            Time.timeScale = 1f;
        }

        private void Update()
        {
            // Toggle pause when Escape is pressed
            if (Input.GetKeyDown(KeyCode.Escape))
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
        }

        /// <summary>
        /// Pauses the game and shows the pause menu
        /// </summary>
        private void Pause()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
            
            Time.timeScale = 0f;
            isPaused = true;
        }

        /// <summary>
        /// Resumes the game and hides the pause menu
        /// </summary>
        public void Resume()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
            
            Time.timeScale = 1f;
            isPaused = false;
        }

        /// <summary>
        /// Returns to the main menu - IMPORTANT: Resets time scale first
        /// </summary>
        public void LoadMenu()
        {
            // Critical: Reset time scale before loading new scene
            Time.timeScale = 1f;
            isPaused = false;
            
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Quits the application
        /// </summary>
        public void QuitGame()
        {
            // Reset time scale before quitting
            Time.timeScale = 1f;
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
