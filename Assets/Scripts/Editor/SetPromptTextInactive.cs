using UnityEngine;
using UnityEditor;

public class SetPromptTextInactive : MonoBehaviour
{
    [MenuItem("Tools/Set PromptText Inactive")]
    public static void Execute()
    {
        GameObject canvas = GameObject.Find("UI_Canvas");
        if (canvas != null)
        {
            Transform promptText = canvas.transform.Find("PromptText");
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
                Debug.Log("PromptText set to inactive");
            }
            else
            {
                Debug.LogWarning("PromptText not found");
            }
        }
        else
        {
            Debug.LogWarning("UI_Canvas not found");
        }
    }
}
