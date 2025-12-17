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
            
            // Handle C key - Back to Main Menu (only when paused)
            if (isPaused && Input.GetKeyDown(KeyCode.C))
            {
                LoadMenu();
            }
            
            // Handle M key - Force to Game End (only when paused)
            if (isPaused && Input.GetKeyDown(KeyCode.M))
            {
                ForceGameEnd();
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
            
            // COMPLETELY STOP THE GAME - freeze all time-based operations
            Time.timeScale = 0f;
            isPaused = true;
            
            // Unlock cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Disable player input
            var gameManager = FindObjectOfType<BarSimulator.Core.GameManager>();
            if (gameManager != null)
            {
                if (gameManager.IsPlaying)
                {
                    gameManager.Pause();
                }
                
                // Disable player controller input
                if (gameManager.PlayerController != null)
                {
                    gameManager.PlayerController.DisableInput();
                }
            }
            
            // Also disable ImprovedInteractionSystem if it exists
            var improvedInteraction = FindObjectOfType<BarSimulator.Player.ImprovedInteractionSystem>();
            if (improvedInteraction != null)
            {
                improvedInteraction.enabled = false;
            }
            
            // Disable PlayerInteraction if it exists
            var playerInteraction = FindObjectOfType<BarSimulator.Player.PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.enabled = false;
            }
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
            
            // Resume game time
            Time.timeScale = 1f;
            isPaused = false;
            
            // Lock cursor back for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Re-enable player input
            var gameManager = FindObjectOfType<BarSimulator.Core.GameManager>();
            if (gameManager != null)
            {
                if (gameManager.IsPaused)
                {
                    gameManager.Resume();
                }
                
                // Re-enable player controller input
                if (gameManager.PlayerController != null)
                {
                    gameManager.PlayerController.EnableInput();
                }
            }
            
            // Re-enable ImprovedInteractionSystem if it exists
            var improvedInteraction = FindObjectOfType<BarSimulator.Player.ImprovedInteractionSystem>();
            if (improvedInteraction != null)
            {
                improvedInteraction.enabled = true;
            }
            
            // Re-enable PlayerInteraction if it exists
            var playerInteraction = FindObjectOfType<BarSimulator.Player.PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.enabled = true;
            }
        }

        /// <summary>
        /// Returns to the main menu - Resets game state and time scale
        /// </summary>
        public void LoadMenu()
        {
            // Critical: Reset time scale before loading new scene
            Time.timeScale = 1f;
            isPaused = false;
            
            // Re-enable player input before scene transition
            var gameManager = FindObjectOfType<BarSimulator.Core.GameManager>();
            if (gameManager != null)
            {
                // Clear any game state
                Debug.Log("[SimplePauseMenu] Resetting game state before returning to main menu");
                
                // Re-enable player controller input
                if (gameManager.PlayerController != null)
                {
                    gameManager.PlayerController.EnableInput();
                }
            }
            
            // Load main menu scene
            SceneManager.LoadScene("MainMenu");
        }
        
        /// <summary>
        /// Force jump to Game End screen - Shows GameEndPanel directly
        /// </summary>
        public void ForceGameEnd()
        {
            // Keep time paused - GameEndPanel will handle it
            Time.timeScale = 0f;
            isPaused = false;
            
            // Hide pause panel
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
            
            // Disable player input
            var gameManager = FindObjectOfType<BarSimulator.Core.GameManager>();
            if (gameManager != null && gameManager.PlayerController != null)
            {
                gameManager.PlayerController.DisableInput();
            }
            
            // Find and show GameEndPanel
            var canvas = GameObject.Find("UI_Canvas");
            if (canvas != null)
            {
                var gameEndPanel = canvas.transform.Find("GameEndPanel");
                if (gameEndPanel != null)
                {
                    gameEndPanel.gameObject.SetActive(true);
                    Debug.Log("[SimplePauseMenu] Forced game end - showing GameEndPanel");
                    
                    // Unlock cursor for UI interaction
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    
                    // Trigger GameEndUI to show properly
                    var gameEndUI = gameEndPanel.GetComponent<BarSimulator.UI.GameEndUI>();
                    if (gameEndUI != null)
                    {
                        gameEndUI.ShowGameEndScreen();
                    }
                }
                else
                {
                    Debug.LogWarning("[SimplePauseMenu] GameEndPanel not found in UI_Canvas!");
                }
            }
            else
            {
                Debug.LogWarning("[SimplePauseMenu] UI_Canvas not found!");
            }
        }
    }
}
