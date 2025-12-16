using UnityEngine;
using UnityEditor;

public class ActivateGameEndPanel
{
    [MenuItem("Tools/Activate GameEndPanel")]
    public static void Execute()
    {
        GameObject panel = GameObject.Find("UI_Canvas/GameEndPanel");
        if (panel == null)
        {
            // Try alternative search
            GameObject canvas = GameObject.Find("UI_Canvas");
            if (canvas != null)
            {
                Transform panelTransform = canvas.transform.Find("GameEndPanel");
                if (panelTransform != null)
                {
                    panel = panelTransform.gameObject;
                }
            }
        }

        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log("GameEndPanel activated!");
        }
        else
        {
            Debug.LogError("GameEndPanel not found!");
        }
    }
}
