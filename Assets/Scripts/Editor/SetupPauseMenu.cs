using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UI;

public class SetupPauseMenu
{
    public static void Execute()
    {
        GameObject canvas = GameObject.Find("UI_Canvas");
        if (canvas == null)
        {
            Debug.LogError("UI_Canvas not found");
            return;
        }

        // Delete old pause panel if exists
        Transform oldPanel = canvas.transform.Find("PausePanel");
        if (oldPanel != null)
        {
            Object.DestroyImmediate(oldPanel.gameObject);
        }

        // Create pause panel
        GameObject pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = pausePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = pausePanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.784f);

        // Create title
        GameObject titleObj = new GameObject("PauseTitle");
        titleObj.transform.SetParent(pausePanel.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -100);
        titleRect.sizeDelta = new Vector2(400, 80);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "PAUSED";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 48;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        // Create button container
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(pausePanel.transform, false);
        
        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(300, 250);

        VerticalLayoutGroup layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        Sprite buttonSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // Create buttons
        GameObject resumeButton = CreateButton("ResumeButton", "Resume", buttonContainer.transform, buttonSprite);
        GameObject menuButton = CreateButton("MenuButton", "Back to Menu", buttonContainer.transform, buttonSprite);
        GameObject quitButton = CreateButton("QuitButton", "Quit", buttonContainer.transform, buttonSprite);

        // Add SimplePauseMenu script
        SimplePauseMenu pauseScript = canvas.GetComponent<SimplePauseMenu>();
        if (pauseScript == null)
        {
            pauseScript = canvas.AddComponent<SimplePauseMenu>();
        }

        // Set pause panel reference
        SerializedObject so = new SerializedObject(pauseScript);
        SerializedProperty pausePanelProp = so.FindProperty("pausePanel");
        pausePanelProp.objectReferenceValue = pausePanel;
        so.ApplyModifiedProperties();

        // Wire up buttons
        Button resumeBtn = resumeButton.GetComponent<Button>();
        resumeBtn.onClick.RemoveAllListeners();
        resumeBtn.onClick.AddListener(() => pauseScript.Resume());

        Button menuBtn = menuButton.GetComponent<Button>();
        menuBtn.onClick.RemoveAllListeners();
        menuBtn.onClick.AddListener(() => pauseScript.LoadMenu());

        Button quitBtn = quitButton.GetComponent<Button>();
        quitBtn.onClick.RemoveAllListeners();
        quitBtn.onClick.AddListener(() => pauseScript.QuitGame());

        // Hide pause panel by default
        pausePanel.SetActive(false);

        EditorUtility.SetDirty(canvas);
        Debug.Log("Pause menu setup complete");
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
