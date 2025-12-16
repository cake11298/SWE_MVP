using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class CreateLiquidUISlider : MonoBehaviour
{
    [MenuItem("Tools/Create Liquid UI Slider")]
    public static void CreateSlider()
    {
        // Find the parent
        GameObject parent = GameObject.Find("UI_Canvas/LiquidInfoPanel");
        if (parent == null)
        {
            Debug.LogError("Cannot find UI_Canvas/LiquidInfoPanel");
            return;
        }

        // Find existing slider or create new
        Transform existingSlider = parent.transform.Find("CapacitySlider");
        GameObject sliderObj;
        
        if (existingSlider != null)
        {
            sliderObj = existingSlider.gameObject;
            // Clear children
            while (sliderObj.transform.childCount > 0)
            {
                DestroyImmediate(sliderObj.transform.GetChild(0).gameObject);
            }
        }
        else
        {
            sliderObj = new GameObject("CapacitySlider");
            sliderObj.transform.SetParent(parent.transform, false);
        }

        // Setup RectTransform
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        if (sliderRect == null)
            sliderRect = sliderObj.AddComponent<RectTransform>();
        
        sliderRect.anchorMin = new Vector2(0, 1);
        sliderRect.anchorMax = new Vector2(1, 1);
        sliderRect.pivot = new Vector2(0.5f, 1);
        sliderRect.anchoredPosition = new Vector2(0, -60);
        sliderRect.sizeDelta = new Vector2(-40, 20);

        // Add Slider component
        Slider slider = sliderObj.GetComponent<Slider>();
        if (slider == null)
            slider = sliderObj.AddComponent<Slider>();
        
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0;
        slider.interactable = false;

        // Create Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        bgImage.type = Image.Type.Sliced;

        // Create Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, -10);

        // Create Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 0.3f, 1f); // Green color
        fillImage.type = Image.Type.Sliced;

        // Assign references to Slider
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;

        // Update LiquidInfoUI reference
        GameObject canvas = GameObject.Find("UI_Canvas");
        if (canvas != null)
        {
            var liquidUI = canvas.GetComponent<BarSimulator.UI.LiquidInfoUI>();
            if (liquidUI != null)
            {
                liquidUI.capacitySlider = slider;
                EditorUtility.SetDirty(liquidUI);
            }
        }

        EditorUtility.SetDirty(sliderObj);
        Debug.Log("Liquid UI Slider created successfully!");
    }
}
