using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class FixMainMenuButtons
{
    public static void Execute()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Canvas not found");
            return;
        }

        Transform buttonContainer = canvas.transform.Find("ButtonContainer");
        if (buttonContainer == null)
        {
            Debug.LogError("ButtonContainer not found");
            return;
        }

        // Get Unity's built-in sprite
        Sprite buttonSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        
        // Fix Start Button
        Transform startButton = buttonContainer.Find("StartButton");
        if (startButton != null)
        {
            // Add LayoutElement
            LayoutElement startLayout = startButton.GetComponent<LayoutElement>();
            if (startLayout == null)
            {
                startLayout = startButton.gameObject.AddComponent<LayoutElement>();
            }
            startLayout.minWidth = 280;
            startLayout.minHeight = 60;
            startLayout.preferredHeight = 60;
            
            // Setup Image
            Image startImage = startButton.GetComponent<Image>();
            if (startImage != null)
            {
                startImage.sprite = buttonSprite;
                startImage.type = Image.Type.Sliced;
                startImage.color = new Color(0.784f, 0.784f, 0.784f, 1f);
                
                // Set as target graphic for button
                Button startBtn = startButton.GetComponent<Button>();
                if (startBtn != null)
                {
                    startBtn.targetGraphic = startImage;
                }
                
                EditorUtility.SetDirty(startImage);
            }
            
            EditorUtility.SetDirty(startLayout);
        }

        // Fix Quit Button
        Transform quitButton = buttonContainer.Find("QuitButton");
        if (quitButton != null)
        {
            // Add LayoutElement
            LayoutElement quitLayout = quitButton.GetComponent<LayoutElement>();
            if (quitLayout == null)
            {
                quitLayout = quitButton.gameObject.AddComponent<LayoutElement>();
            }
            quitLayout.minWidth = 280;
            quitLayout.minHeight = 60;
            quitLayout.preferredHeight = 60;
            
            // Setup Image
            Image quitImage = quitButton.GetComponent<Image>();
            if (quitImage != null)
            {
                quitImage.sprite = buttonSprite;
                quitImage.type = Image.Type.Sliced;
                quitImage.color = new Color(0.784f, 0.784f, 0.784f, 1f);
                
                // Set as target graphic for button
                Button quitBtn = quitButton.GetComponent<Button>();
                if (quitBtn != null)
                {
                    quitBtn.targetGraphic = quitImage;
                }
                
                EditorUtility.SetDirty(quitImage);
            }
            
            EditorUtility.SetDirty(quitLayout);
        }

        Debug.Log("Main menu buttons fixed");
    }
}
