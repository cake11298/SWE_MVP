using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using BarSimulator.UI;

namespace BarSimulator.Editor
{
    /// <summary>
    /// Editor script to add liquor levels display to LiquidInfoUI
    /// </summary>
    public class SetupLiquorLevelsUI : MonoBehaviour
    {
        [MenuItem("Bar Simulator/Setup/Setup Liquor Levels UI")]
        public static void Setup()
        {
            Debug.Log("Setting up Liquor Levels UI...");

            // Find LiquidInfoUI
            LiquidInfoUI liquidInfoUI = FindFirstObjectByType<LiquidInfoUI>();
            if (liquidInfoUI == null)
            {
                Debug.LogError("LiquidInfoUI not found in scene!");
                return;
            }

            // Find or create the info panel
            GameObject infoPanel = liquidInfoUI.infoPanel;
            if (infoPanel == null)
            {
                Debug.LogError("Info panel not found on LiquidInfoUI!");
                return;
            }

            // Check if liquor levels text already exists
            Transform existingText = infoPanel.transform.Find("LiquorLevelsText");
            if (existingText != null)
            {
                Debug.Log("LiquorLevelsText already exists, updating reference...");
                var existingTextComponent = existingText.GetComponent<Text>();
                if (existingTextComponent != null)
                {
                    // Update reference using reflection
                    var field = typeof(LiquidInfoUI).GetField("liquorLevelsText", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(liquidInfoUI, existingTextComponent);
                    }
                }
                EditorUtility.SetDirty(liquidInfoUI);
                Debug.Log("Liquor Levels UI setup complete!");
                return;
            }

            // Create liquor levels text
            GameObject liquorLevelsTextObj = new GameObject("LiquorLevelsText");
            liquorLevelsTextObj.transform.SetParent(infoPanel.transform, false);

            // Add RectTransform
            RectTransform rectTransform = liquorLevelsTextObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0.4f);
            rectTransform.offsetMin = new Vector2(10, 10);
            rectTransform.offsetMax = new Vector2(-10, -10);

            // Add Text component
            Text textComponent = liquorLevelsTextObj.AddComponent<Text>();
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 14;
            textComponent.alignment = TextAnchor.UpperLeft;
            textComponent.color = Color.white;
            textComponent.text = "=== Base Liquors ===\nLoading...";

            // Set reference using reflection
            var liquorLevelsTextField = typeof(LiquidInfoUI).GetField("liquorLevelsText", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (liquorLevelsTextField != null)
            {
                liquorLevelsTextField.SetValue(liquidInfoUI, textComponent);
            }

            // Mark as dirty
            EditorUtility.SetDirty(liquidInfoUI);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log("Liquor Levels UI setup complete!");
        }
    }
}
