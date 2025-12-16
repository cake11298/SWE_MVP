using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Editor script to setup pause menu button listeners
/// </summary>
public class SetupPauseMenuButtons : MonoBehaviour
{
    [MenuItem("Tools/Setup Pause Menu Buttons")]
    public static void Setup()
    {
        // Find the UI Canvas
        GameObject canvas = GameObject.Find("UI_Canvas");
        if (canvas == null)
        {
            Debug.LogError("UI_Canvas not found");
            return;
        }

        // Get the SimplePauseMenu component
        var pauseMenu = canvas.GetComponent<UI.SimplePauseMenu>();
        if (pauseMenu == null)
        {
            Debug.LogError("SimplePauseMenu component not found on UI_Canvas");
            return;
        }

        // Find the buttons
        GameObject resumeButton = GameObject.Find("UI_Canvas/PausePanel/ButtonContainer/ResumeButton");
        GameObject menuButton = GameObject.Find("UI_Canvas/PausePanel/ButtonContainer/MenuButton");

        if (resumeButton == null || menuButton == null)
        {
            Debug.LogError("Buttons not found");
            return;
        }

        // Get Button components
        Button resumeBtn = resumeButton.GetComponent<Button>();
        Button menuBtn = menuButton.GetComponent<Button>();

        if (resumeBtn == null || menuBtn == null)
        {
            Debug.LogError("Button components not found");
            return;
        }

        // Clear existing listeners
        resumeBtn.onClick.RemoveAllListeners();
        menuBtn.onClick.RemoveAllListeners();

        // Add new listeners
        resumeBtn.onClick.AddListener(() => pauseMenu.Resume());
        menuBtn.onClick.AddListener(() => pauseMenu.LoadMenu());

        Debug.Log("Pause menu buttons setup complete!");
        Debug.Log("Resume button: " + resumeBtn.onClick.GetPersistentEventCount() + " listeners");
        Debug.Log("Menu button: " + menuBtn.onClick.GetPersistentEventCount() + " listeners");

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }
}
