using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using BarSimulator.UI;

/// <summary>
/// Creates the Game End UI panel
/// </summary>
public class CreateGameEndUI
{
    public static void Execute()
    {
        Debug.Log("=== Creating Game End UI ===");

        // Find UI_Canvas
        GameObject canvas = GameObject.Find("UI_Canvas");
        if (canvas == null)
        {
            Debug.LogError("UI_Canvas not found in scene!");
            return;
        }

        // Check if GameEndPanel already exists
        Transform existingPanel = canvas.transform.Find("GameEndPanel");
        if (existingPanel != null)
        {
            Debug.Log("GameEndPanel already exists, updating it...");
            GameObject.DestroyImmediate(existingPanel.gameObject);
        }

        // Create Game End Panel
        GameObject gameEndPanel = new GameObject("GameEndPanel");
        gameEndPanel.transform.SetParent(canvas.transform, false);

        // Add RectTransform
        RectTransform panelRect = gameEndPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // Add Image (background)
        Image panelImage = gameEndPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.9f); // Dark semi-transparent

        // Create Content Panel
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(gameEndPanel.transform, false);

        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(600f, 500f);
        contentRect.anchoredPosition = Vector2.zero;

        Image contentImage = contentPanel.AddComponent<Image>();
        contentImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        // Create Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(contentPanel.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(500f, 80f);
        titleRect.anchoredPosition = new Vector2(0f, -60f);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Bar Closed - Time's Up!";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 36;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        // Create Stats Container
        GameObject statsContainer = new GameObject("StatsContainer");
        statsContainer.transform.SetParent(contentPanel.transform, false);

        RectTransform statsRect = statsContainer.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.5f, 0.5f);
        statsRect.anchorMax = new Vector2(0.5f, 0.5f);
        statsRect.sizeDelta = new Vector2(500f, 250f);
        statsRect.anchoredPosition = new Vector2(0f, 20f);

        // Create stat texts
        CreateStatText(statsContainer, "TotalMoneyText", "Total Money: $0", new Vector2(0f, 80f));
        CreateStatText(statsContainer, "DrinksServedText", "Drinks Served: 0", new Vector2(0f, 40f));
        CreateStatText(statsContainer, "SatisfiedCustomersText", "Satisfied Customers: 0", new Vector2(0f, 0f));
        CreateStatText(statsContainer, "AverageRatingText", "Average Rating: 0%", new Vector2(0f, -40f));

        // Create Buttons Container
        GameObject buttonsContainer = new GameObject("ButtonsContainer");
        buttonsContainer.transform.SetParent(contentPanel.transform, false);

        RectTransform buttonsRect = buttonsContainer.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.5f, 0f);
        buttonsRect.anchorMax = new Vector2(0.5f, 0f);
        buttonsRect.sizeDelta = new Vector2(500f, 80f);
        buttonsRect.anchoredPosition = new Vector2(0f, 60f);

        // Create Main Menu Button
        GameObject mainMenuBtn = CreateButton(buttonsContainer, "MainMenuButton", "Main Menu", new Vector2(-130f, 0f));

        // Create Quit Button
        GameObject quitBtn = CreateButton(buttonsContainer, "QuitButton", "Quit", new Vector2(130f, 0f));

        // Add GameEndUI component
        GameEndUI gameEndUI = gameEndPanel.AddComponent<GameEndUI>();

        // Use reflection to set private fields
        var gameEndUIType = typeof(GameEndUI);
        
        var gameEndPanelField = gameEndUIType.GetField("gameEndPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (gameEndPanelField != null) gameEndPanelField.SetValue(gameEndUI, gameEndPanel);

        var titleTextField = gameEndUIType.GetField("titleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (titleTextField != null) titleTextField.SetValue(gameEndUI, titleText);

        var totalMoneyTextField = gameEndUIType.GetField("totalMoneyText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (totalMoneyTextField != null) totalMoneyTextField.SetValue(gameEndUI, statsContainer.transform.Find("TotalMoneyText").GetComponent<Text>());

        var drinksServedTextField = gameEndUIType.GetField("drinksServedText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (drinksServedTextField != null) drinksServedTextField.SetValue(gameEndUI, statsContainer.transform.Find("DrinksServedText").GetComponent<Text>());

        var satisfiedCustomersTextField = gameEndUIType.GetField("satisfiedCustomersText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (satisfiedCustomersTextField != null) satisfiedCustomersTextField.SetValue(gameEndUI, statsContainer.transform.Find("SatisfiedCustomersText").GetComponent<Text>());

        var averageRatingTextField = gameEndUIType.GetField("averageRatingText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (averageRatingTextField != null) averageRatingTextField.SetValue(gameEndUI, statsContainer.transform.Find("AverageRatingText").GetComponent<Text>());

        var mainMenuButtonField = gameEndUIType.GetField("mainMenuButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (mainMenuButtonField != null) mainMenuButtonField.SetValue(gameEndUI, mainMenuBtn.GetComponent<Button>());

        var quitButtonField = gameEndUIType.GetField("quitButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (quitButtonField != null) quitButtonField.SetValue(gameEndUI, quitBtn.GetComponent<Button>());

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("Game End UI created successfully!");
        EditorUtility.DisplayDialog("Success", "Game End UI has been created!", "OK");
    }

    private static void CreateStatText(GameObject parent, string name, string text, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(450f, 30f);
        rect.anchoredPosition = position;

        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = 24;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.color = Color.white;
    }

    private static GameObject CreateButton(GameObject parent, string name, string text, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent.transform, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(200f, 50f);
        rect.anchoredPosition = position;

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 0.8f, 1f);

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;

        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = 20;
        textComponent.fontStyle = FontStyle.Bold;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.color = Color.white;

        return buttonObj;
    }
}
