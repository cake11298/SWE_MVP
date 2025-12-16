using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UI;

public class RebuildMainMenu
{
    public static void Execute()
    {
        // Find or create canvas
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Canvas not found");
            return;
        }

        // Delete old button container if exists
        Transform oldContainer = canvas.transform.Find("ButtonContainer");
        if (oldContainer != null)
        {
            Object.DestroyImmediate(oldContainer.gameObject);
        }

        // Create new button container
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(canvas.transform, false);
        
        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0, -50);
        containerRect.sizeDelta = new Vector2(300, 200);

        VerticalLayoutGroup layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        // Get Unity's built-in sprite
        Sprite buttonSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // Create Start Button
        GameObject startButton = CreateButton("StartButton", "Start Game", buttonContainer.transform, buttonSprite);
        
        // Create Quit Button
        GameObject quitButton = CreateButton("QuitButton", "Quit", buttonContainer.transform, buttonSprite);

        // Add SimpleMainMenu script if not exists
        SimpleMainMenu menuScript = canvas.GetComponent<SimpleMainMenu>();
        if (menuScript == null)
        {
            menuScript = canvas.AddComponent<SimpleMainMenu>();
        }

        // Wire up button events
        Button startBtn = startButton.GetComponent<Button>();
        startBtn.onClick.RemoveAllListeners();
        startBtn.onClick.AddListener(() => menuScript.StartGame());

        Button quitBtn = quitButton.GetComponent<Button>();
        quitBtn.onClick.RemoveAllListeners();
        quitBtn.onClick.AddListener(() => menuScript.QuitGame());

        EditorUtility.SetDirty(canvas);
        Debug.Log("Main menu rebuilt successfully");
    }

    private static GameObject CreateButton(string name, string text, Transform parent, Sprite sprite)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent, false);

        RectTransform rect = button.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, 60);

        Image image = button.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.784f, 0.784f, 0.784f, 1f);

        Button btn = button.AddComponent<Button>();
        btn.targetGraphic = image;
        
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.784f, 0.784f, 0.784f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.588f, 0.588f, 0.588f, 1f);
        btn.colors = colors;

        LayoutElement layoutElement = button.AddComponent<LayoutElement>();
        layoutElement.minHeight = 60;
        layoutElement.preferredHeight = 60;

        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = 24;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.color = Color.black;

        return button;
    }
}
