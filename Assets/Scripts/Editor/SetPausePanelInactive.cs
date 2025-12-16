using UnityEngine;
using UnityEditor;

public class SetPausePanelInactive
{
    public static void Execute()
    {
        GameObject canvas = GameObject.Find("UI_Canvas");
        if (canvas != null)
        {
            Transform pausePanel = canvas.transform.Find("PausePanel");
            if (pausePanel != null)
            {
                pausePanel.gameObject.SetActive(false);
                EditorUtility.SetDirty(pausePanel.gameObject);
                Debug.Log("PausePanel set to inactive");
            }
            else
            {
                Debug.LogError("PausePanel not found under UI_Canvas");
            }
        }
        else
        {
            Debug.LogError("UI_Canvas not found");
        }
    }
}
