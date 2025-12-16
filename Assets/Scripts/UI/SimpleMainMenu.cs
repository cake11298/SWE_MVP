using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    /// <summary>
    /// Main Menu logic - handles scene loading and application quit
    /// </summary>
    public class SimpleMainMenu : MonoBehaviour
    {
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
