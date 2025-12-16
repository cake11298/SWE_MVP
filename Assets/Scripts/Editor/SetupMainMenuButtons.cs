using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class SetupMainMenuButtons
{
    public static void Execute()
    {
        // Find buttons
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
        
        // Setup Start Button
        Transform startButton = buttonContainer.Find("StartButton");
        if (startButton != null)
        {
            Image startImage = startButton.GetComponent<Image>();
            if (startImage != null)
            {
                startImage.sprite = buttonSprite;
                startImage.type = Image.Type.Sliced;
                startImage.color = new Color(0.784f, 0.784f, 0.784f, 1f);
                EditorUtility.SetDirty(startImage);
            }
        }

        // Setup Quit Button
        Transform quitButton = buttonContainer.Find("QuitButton");
        if (quitButton != null)
        {
            Image quitImage = quitButton.GetComponent<Image>();
            if (quitImage != null)
            {
                quitImage.sprite = buttonSprite;
                quitImage.type = Image.Type.Sliced;
                quitImage.color = new Color(0.784f, 0.784f, 0.784f, 1f);
                EditorUtility.SetDirty(quitImage);
            }
        }

        Debug.Log("Main menu buttons setup complete");
    }
}
