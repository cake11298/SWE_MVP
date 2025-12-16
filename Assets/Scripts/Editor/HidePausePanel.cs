using UnityEngine;
using UnityEditor;

public class HidePausePanel : MonoBehaviour
{
    [MenuItem("Tools/Hide Pause Panel")]
    static void Execute()
    {
        GameObject pausePanel = GameObject.Find("UI_Canvas/PausePanel");
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Debug.Log("PausePanel hidden successfully");
        }
        else
        {
            Debug.LogError("PausePanel not found");
        }
    }
}
