using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using BarSimulator.UI;
using BarSimulator.Data;
using System;

public class SetupGameEndShop
{
    public static void Execute()
    {
        // Open Scene
        string scenePath = "Assets/SceneS/GameEnd.unity";
        EditorSceneManager.OpenScene(scenePath);

        // Find or Create Canvas
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("UI_Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Clear existing children
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);
        }

        // Create Shop Root
        GameObject shopRoot = new GameObject("ShopUI");
        shopRoot.transform.SetParent(canvas.transform, false);
        RectTransform shopRect = shopRoot.AddComponent<RectTransform>();
        shopRect.anchorMin = Vector2.zero;
        shopRect.anchorMax = Vector2.one;
        shopRect.offsetMin = Vector2.zero;
        shopRect.offsetMax = Vector2.zero;

        // Add Background
        Image bg = shopRoot.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        // Create Controller
        ShopUIController controller = shopRoot.AddComponent<ShopUIController>();

        // Title
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(shopRoot.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Bar Closed - Daily Summary";
        titleText.fontSize = 48;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyles.Bold;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.9f);
        titleRect.anchorMax = new Vector2(1, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // Coins Text
        GameObject coinsObj = new GameObject("CoinsText");
        coinsObj.transform.SetParent(shopRoot.transform, false);
        TextMeshProUGUI coinsText = coinsObj.AddComponent<TextMeshProUGUI>();
        coinsText.text = "Coins: 0";
        coinsText.fontSize = 36;
        coinsText.alignment = TextAlignmentOptions.Center;
        coinsText.color = Color.yellow;
        RectTransform coinsRect = coinsObj.GetComponent<RectTransform>();
        coinsRect.anchorMin = new Vector2(0.4f, 0.85f);
        coinsRect.anchorMax = new Vector2(0.6f, 0.9f);
        coinsRect.offsetMin = Vector2.zero;
        coinsRect.offsetMax = Vector2.zero;

        // === Left Panel (Liquor Upgrades) ===
        GameObject leftPanel = new GameObject("LiquorPanel");
        leftPanel.transform.SetParent(shopRoot.transform, false);
        RectTransform leftRect = leftPanel.AddComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0.02f, 0.2f); // Widened
        leftRect.anchorMax = new Vector2(0.48f, 0.8f); // Widened
        leftRect.offsetMin = Vector2.zero;
        leftRect.offsetMax = Vector2.zero;
        
        // Header
        GameObject leftHeader = new GameObject("Header");
        leftHeader.transform.SetParent(leftPanel.transform, false);
        TextMeshProUGUI leftHeaderText = leftHeader.AddComponent<TextMeshProUGUI>();
        leftHeaderText.text = "Liquor Upgrades (Lv1-5)";
        leftHeaderText.fontSize = 30;
        leftHeaderText.alignment = TextAlignmentOptions.Center;
        RectTransform leftHeaderRect = leftHeader.GetComponent<RectTransform>();
        leftHeaderRect.anchorMin = new Vector2(0, 0.9f);
        leftHeaderRect.anchorMax = new Vector2(1, 1);
        leftHeaderRect.offsetMin = Vector2.zero;
        leftHeaderRect.offsetMax = Vector2.zero;

        // Content
        GameObject leftContent = new GameObject("Content");
        leftContent.transform.SetParent(leftPanel.transform, false);
        RectTransform leftContentRect = leftContent.AddComponent<RectTransform>();
        leftContentRect.anchorMin = new Vector2(0, 0);
        leftContentRect.anchorMax = new Vector2(1, 0.9f);
        leftContentRect.offsetMin = Vector2.zero;
        leftContentRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup leftLayout = leftContent.AddComponent<VerticalLayoutGroup>();
        leftLayout.childControlHeight = false;
        leftLayout.childForceExpandHeight = false;
        leftLayout.childControlWidth = true; // Ensure items stretch to fill width
        leftLayout.childForceExpandWidth = true;
        leftLayout.spacing = 15;
        leftLayout.padding = new RectOffset(10, 10, 10, 10);
        leftLayout.childAlignment = TextAnchor.UpperCenter;

        // === Right Panel (Decorations) ===
        GameObject rightPanel = new GameObject("DecorationPanel");
        rightPanel.transform.SetParent(shopRoot.transform, false);
        RectTransform rightRect = rightPanel.AddComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.52f, 0.2f); // Widened
        rightRect.anchorMax = new Vector2(0.98f, 0.8f); // Widened
        rightRect.offsetMin = Vector2.zero;
        rightRect.offsetMax = Vector2.zero;

        // Header
        GameObject rightHeader = new GameObject("Header");
        rightHeader.transform.SetParent(rightPanel.transform, false);
        TextMeshProUGUI rightHeaderText = rightHeader.AddComponent<TextMeshProUGUI>();
        rightHeaderText.text = "Decorations";
        rightHeaderText.fontSize = 30;
        rightHeaderText.alignment = TextAlignmentOptions.Center;
        RectTransform rightHeaderRect = rightHeader.GetComponent<RectTransform>();
        rightHeaderRect.anchorMin = new Vector2(0, 0.9f);
        rightHeaderRect.anchorMax = new Vector2(1, 1);
        rightHeaderRect.offsetMin = Vector2.zero;
        rightHeaderRect.offsetMax = Vector2.zero;

        // Content
        GameObject rightContent = new GameObject("Content");
        rightContent.transform.SetParent(rightPanel.transform, false);
        RectTransform rightContentRect = rightContent.AddComponent<RectTransform>();
        rightContentRect.anchorMin = new Vector2(0, 0);
        rightContentRect.anchorMax = new Vector2(1, 0.9f);
        rightContentRect.offsetMin = Vector2.zero;
        rightContentRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup rightLayout = rightContent.AddComponent<VerticalLayoutGroup>();
        rightLayout.childControlHeight = false;
        rightLayout.childForceExpandHeight = false;
        rightLayout.childControlWidth = true; // Ensure items stretch to fill width
        rightLayout.childForceExpandWidth = true;
        rightLayout.spacing = 15;
        rightLayout.padding = new RectOffset(10, 10, 10, 10);
        rightLayout.childAlignment = TextAnchor.UpperCenter;

        // === Buttons Container ===
        GameObject buttonsPanel = new GameObject("ButtonsPanel");
        buttonsPanel.transform.SetParent(shopRoot.transform, false);
        RectTransform buttonsRect = buttonsPanel.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0, 0.05f);
        buttonsRect.anchorMax = new Vector2(1, 0.15f);
        buttonsRect.offsetMin = Vector2.zero;
        buttonsRect.offsetMax = Vector2.zero;

        HorizontalLayoutGroup btnLayout = buttonsPanel.AddComponent<HorizontalLayoutGroup>();
        btnLayout.childControlWidth = false;
        btnLayout.childForceExpandWidth = false;
        btnLayout.spacing = 100;
        btnLayout.childAlignment = TextAnchor.MiddleCenter;

        // Next Game Button
        GameObject nextBtn = CreateButton("NextGameButton", "Next Game", buttonsPanel.transform, Color.green);
        
        // Main Menu Button
        GameObject menuBtn = CreateButton("MainMenuButton", "Main Menu", buttonsPanel.transform, Color.white);

        // === Generate Static Items ===
        
        // Liquors
        foreach (BaseLiquorType type in Enum.GetValues(typeof(BaseLiquorType)))
        {
            CreateShopItem(leftContent.transform, type.ToString(), true, type, DecorationType.Speaker);
        }

        // Decorations
        foreach (DecorationType type in Enum.GetValues(typeof(DecorationType)))
        {
            CreateShopItem(rightContent.transform, type.ToString(), false, BaseLiquorType.Vodka, type);
        }

        // Assign to Controller
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("coinsText").objectReferenceValue = coinsText;
        so.FindProperty("liquorListContainer").objectReferenceValue = leftContent.transform;
        so.FindProperty("decorationListContainer").objectReferenceValue = rightContent.transform;
        so.FindProperty("nextGameButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        so.FindProperty("mainMenuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        so.ApplyModifiedProperties();

        // Save Scene
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("GameEnd Shop UI Setup Complete (Static Items)!");
    }

    private static void CreateShopItem(Transform parent, string name, bool isLiquor, BaseLiquorType lType, DecorationType dType)
    {
        GameObject itemObj = new GameObject($"Item_{name}");
        itemObj.transform.SetParent(parent, false);
        
        Image itemBg = itemObj.AddComponent<Image>();
        itemBg.color = new Color(0.3f, 0.3f, 0.35f, 1f);
        
        LayoutElement le = itemObj.AddComponent<LayoutElement>();
        le.minHeight = 250; // Increased Height (approx 3x)
        le.preferredHeight = 250;
        le.preferredWidth = 1200;

        ShopItemView view = itemObj.AddComponent<ShopItemView>();
        view.isLiquor = isLiquor;
        view.liquorType = lType;
        view.decorationType = dType;

        // Name Text
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(itemObj.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = name;
        nameText.fontSize = 48; // Increased Font Size
        nameText.color = Color.white;
        nameText.fontStyle = FontStyles.Bold;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.02f, 0.55f);
        nameRect.anchorMax = new Vector2(0.48f, 0.9f); // Adjusted for wider button
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        view.nameText = nameText;

        // Info Text
        GameObject infoObj = new GameObject("InfoText");
        infoObj.transform.SetParent(itemObj.transform, false);
        TextMeshProUGUI infoText = infoObj.AddComponent<TextMeshProUGUI>();
        infoText.text = "Loading...";
        infoText.fontSize = 32; // Increased Font Size
        infoText.color = new Color(0.8f, 0.8f, 0.8f);
        RectTransform infoRect = infoObj.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.02f, 0.1f);
        infoRect.anchorMax = new Vector2(0.48f, 0.45f); // Adjusted for wider button
        infoRect.offsetMin = Vector2.zero;
        infoRect.offsetMax = Vector2.zero;
        view.infoText = infoText;

        // Button
        GameObject btnObj = CreateButton("ActionButton", "$1000", itemObj.transform, new Color(0.2f, 0.6f, 0.3f));
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.15f); // Wider button (starts at 50%)
        btnRect.anchorMax = new Vector2(0.98f, 0.85f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;
        btnRect.sizeDelta = Vector2.zero;
        
        // Increase Button Text Size
        var btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null) btnText.fontSize = 40;
        
        view.actionButton = btnObj.GetComponent<Button>();
        view.buttonText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
    }

    private static GameObject CreateButton(string name, string text, Transform parent, Color color)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = color;
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(600, 80); // Widened 3x

        return btnObj;
    }
}
