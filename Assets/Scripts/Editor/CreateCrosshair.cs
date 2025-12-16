using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class CreateCrosshair : MonoBehaviour
{
    public static void Execute()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("UI_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("Created UI Canvas");
        
        // Create Crosshair
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(canvasObj.transform, false);
        
        Image crosshairImage = crosshairObj.AddComponent<Image>();
        
        // Create a simple dot texture
        Texture2D dotTexture = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        
        // Fill with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        // Draw a small white dot in the center (4x4 pixels)
        for (int y = 6; y < 10; y++)
        {
            for (int x = 6; x < 10; x++)
            {
                pixels[y * 16 + x] = Color.white;
            }
        }
        
        dotTexture.SetPixels(pixels);
        dotTexture.Apply();
        
        // Create sprite from texture
        Sprite dotSprite = Sprite.Create(
            dotTexture,
            new Rect(0, 0, 16, 16),
            new Vector2(0.5f, 0.5f),
            100f
        );
        
        crosshairImage.sprite = dotSprite;
        crosshairImage.color = Color.white;
        
        // Set RectTransform
        RectTransform rectTransform = crosshairObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(16, 16);
        
        Debug.Log("Created Crosshair at screen center");
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }
}
