using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using BarSimulator.UI;

public class SetupGameEndScene : MonoBehaviour
{
    public static void Execute()
    {
        // 1. Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Create Background Panel
        GameObject panelObj = new GameObject("BackgroundPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 3. Title
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Bar Closed - Restock & Upgrade";
        titleText.fontSize = 48;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -50);
        titleRect.sizeDelta = new Vector2(0, 100);

        // 4. Coins Display
        GameObject coinsObj = new GameObject("CoinsText");
        coinsObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI coinsText = coinsObj.AddComponent<TextMeshProUGUI>();
        coinsText.text = "Coins: 0";
        coinsText.fontSize = 36;
        coinsText.alignment = TextAlignmentOptions.Right;
        coinsText.color = Color.yellow;
        RectTransform coinsRect = coinsObj.GetComponent<RectTransform>();
        coinsRect.anchorMin = new Vector2(1, 1);
        coinsRect.anchorMax = new Vector2(1, 1);
        coinsRect.pivot = new Vector2(1, 1);
        coinsRect.anchoredPosition = new Vector2(-50, -50);
        coinsRect.sizeDelta = new Vector2(300, 50);

        // 5. Shop Container (Horizontal Layout for Liquor vs Decor)
        GameObject shopContainer = new GameObject("ShopContainer");
        shopContainer.transform.SetParent(panelObj.transform, false);
        RectTransform shopRect = shopContainer.AddComponent<RectTransform>();
        shopRect.anchorMin = new Vector2(0.1f, 0.2f);
        shopRect.anchorMax = new Vector2(0.9f, 0.8f);
        shopRect.offsetMin = Vector2.zero;
        shopRect.offsetMax = Vector2.zero;
        HorizontalLayoutGroup shopLayout = shopContainer.AddComponent<HorizontalLayoutGroup>();
        shopLayout.spacing = 50;
        shopLayout.childControlHeight = true;
        shopLayout.childControlWidth = true;

        // 6. Liquor Section
        GameObject liquorSection = CreateSection(shopContainer.transform, "Liquor Upgrades");
        GameObject decorSection = CreateSection(shopContainer.transform, "Decorations");

        // 7. Buttons Container
        GameObject btnContainer = new GameObject("ButtonContainer");
        btnContainer.transform.SetParent(panelObj.transform, false);
        RectTransform btnRect = btnContainer.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0, 0);
        btnRect.anchorMax = new Vector2(1, 0.15f);
        HorizontalLayoutGroup btnLayout = btnContainer.AddComponent<HorizontalLayoutGroup>();
        btnLayout.childAlignment = TextAnchor.MiddleCenter;
        btnLayout.spacing = 100;

        // Next Game Button
        GameObject nextBtn = CreateButton(btnContainer.transform, "Next Game");
        // Main Menu Button
        GameObject menuBtn = CreateButton(btnContainer.transform, "Main Menu");

        // 8. Create Shop Item Prefab
        GameObject prefabObj = CreateShopItemPrefab(canvasObj.transform);
        // Save as prefab (simulated by keeping it hidden or just referencing it)
        // Ideally we save it to assets, but for now let's just keep it in scene and reference it
        prefabObj.SetActive(false);

        // 9. Setup Controller
        ShopUIController controller = canvasObj.AddComponent<ShopUIController>();
        
        // Use SerializedObject to assign private fields
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("coinsText").objectReferenceValue = coinsText;
        so.FindProperty("liquorListContainer").objectReferenceValue = liquorSection.transform.Find("Content");
        so.FindProperty("decorationListContainer").objectReferenceValue = decorSection.transform.Find("Content");
        so.FindProperty("nextGameButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        so.FindProperty("mainMenuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        so.FindProperty("shopItemPrefab").objectReferenceValue = prefabObj;
        so.ApplyModifiedProperties();

        // Add EventSystem
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }

    private static GameObject CreateSection(Transform parent, string title)
    {
        GameObject section = new GameObject(title + "Section");
        section.transform.SetParent(parent, false);
        Image bg = section.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.3f);
        VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 10;

        // Title
        GameObject txtObj = new GameObject("Title");
        txtObj.transform.SetParent(section.transform, false);
        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = title;
        txt.fontSize = 24;
        txt.alignment = TextAlignmentOptions.Center;
        txtObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40);

        // Content Container
        GameObject content = new GameObject("Content");
        content.transform.SetParent(section.transform, false);
        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 5;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandHeight = false;
        
        // Add Content Size Fitter to Content
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return section;
    }

    private static GameObject CreateButton(Transform parent, string text)
    {
        GameObject btnObj = new GameObject(text + "Button");
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = Color.white;
        Button btn = btnObj.AddComponent<Button>();
        
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = text;
        txt.fontSize = 24;
        txt.color = Color.black;
        txt.alignment = TextAlignmentOptions.Center;
        
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 60);
        
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        return btnObj;
    }

    private static GameObject CreateShopItemPrefab(Transform parent)
    {
        GameObject item = new GameObject("ShopItem");
        item.transform.SetParent(parent, false);
        Image bg = item.AddComponent<Image>();
        bg.color = new Color(1, 1, 1, 0.1f);
        
        HorizontalLayoutGroup layout = item.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 5, 5);
        layout.spacing = 10;
        layout.childControlWidth = false;
        layout.childForceExpandWidth = false;

        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(0, 60);

        // Name
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(item.transform, false);
        TextMeshProUGUI nameTxt = nameObj.AddComponent<TextMeshProUGUI>();
        nameTxt.fontSize = 18;
        nameTxt.color = Color.white;
        nameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 50);

        // Info
        GameObject infoObj = new GameObject("Info");
        infoObj.transform.SetParent(item.transform, false);
        TextMeshProUGUI infoTxt = infoObj.AddComponent<TextMeshProUGUI>();
        infoTxt.fontSize = 16;
        infoTxt.color = Color.gray;
        infoObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 50);

        // Cost
        GameObject costObj = new GameObject("Cost");
        costObj.transform.SetParent(item.transform, false);
        TextMeshProUGUI costTxt = costObj.AddComponent<TextMeshProUGUI>();
        costTxt.fontSize = 18;
        costTxt.color = Color.yellow;
        costObj.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 50);

        // Button
        GameObject btnObj = CreateButton(item.transform, "Buy");
        btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 40);
        
        return item;
    }
}
