using UnityEngine;
using UnityEditor;
using BarSimulator.UI;

public class TestUIPrompt : MonoBehaviour
{
    [MenuItem("Tools/Test UI Prompt")]
    public static void Execute()
    {
        // Make sure PromptText is visible for screenshot
        GameObject canvas = GameObject.Find("UI_Canvas");
        if (canvas != null)
        {
            Transform promptText = canvas.transform.Find("PromptText");
            if (promptText != null)
            {
                promptText.gameObject.SetActive(true);
                
                // Set test message
                UnityEngine.UI.Text textComponent = promptText.GetComponent<UnityEngine.UI.Text>();
                if (textComponent != null)
                {
                    textComponent.text = "Picked up Gin";
                    Debug.Log("Test prompt displayed: 'Picked up Gin'");
                }
            }
        }
    }
}
