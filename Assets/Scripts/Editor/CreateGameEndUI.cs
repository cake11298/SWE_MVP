using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace BarSimulator.Editor
{
    public static class CreateGameEndUI
    {
        [MenuItem("Bar/Create Game End UI")]
        public static void Execute()
        {
            // Open GameEnd scene
            EditorSceneManager.OpenScene("Assets/SceneS/GameEnd.unity");

            // Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Create Background
            GameObject bgObj = CreateUIElement("Background", canvasObj.transform);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Create Title
            GameObject titleObj = CreateUIElement("TitleText", canvasObj.transform);
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "遊戲結束 - Game Over";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 60;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.85f);
            titleRect.anchorMax = new Vector2(0.5f, 0.95f);
            titleRect.sizeDelta = new Vector2(800, 100);

            // Create Stats Panel (Center)
            GameObject statsPanel = CreateUIElement("StatsPanel", canvasObj.transform);
            Image statsPanelImg = statsPanel.AddComponent<Image>();
            statsPanelImg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            RectTransform statsRect = statsPanel.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.35f, 0.4f);
            statsRect.anchorMax = new Vector2(0.65f, 0.75f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;

            // Stats Title
            GameObject statsTitleObj = CreateUIElement("StatsTitle", statsPanel.transform);
            Text statsTitleText = statsTitleObj.AddComponent<Text>();
            statsTitleText.text = "本局統計 Statistics";
            statsTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statsTitleText.fontSize = 32;
            statsTitleText.alignment = TextAnchor.MiddleCenter;
            statsTitleText.color = Color.yellow;
            RectTransform statsTitleRect = statsTitleObj.GetComponent<RectTransform>();
            statsTitleRect.anchorMin = new Vector2(0, 0.85f);
            statsTitleRect.anchorMax = new Vector2(1, 1);
            statsTitleRect.offsetMin = new Vector2(20, -10);
            statsTitleRect.offsetMax = new Vector2(-20, -10);

            // Coins Text
            GameObject coinsObj = CreateUIElement("CoinsText", statsPanel.transform);
            Text coinsText = coinsObj.AddComponent<Text>();
            coinsText.text = "總金幣 Total Coins: $0";
            coinsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            coinsText.fontSize = 28;
            coinsText.alignment = TextAnchor.MiddleCenter;
            coinsText.color = Color.white;
            RectTransform coinsRect = coinsObj.GetComponent<RectTransform>();
            coinsRect.anchorMin = new Vector2(0, 0.65f);
            coinsRect.anchorMax = new Vector2(1, 0.8f);
            coinsRect.offsetMin = new Vector2(20, 0);
            coinsRect.offsetMax = new Vector2(-20, 0);

            // Drinks Served Text
            GameObject drinksObj = CreateUIElement("DrinksServedText", statsPanel.transform);
            Text drinksText = drinksObj.AddComponent<Text>();
            drinksText.text = "服務杯數 Drinks Served: 0";
            drinksText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            drinksText.fontSize = 24;
            drinksText.alignment = TextAnchor.MiddleCenter;
            drinksText.color = Color.white;
            RectTransform drinksRect = drinksObj.GetComponent<RectTransform>();
            drinksRect.anchorMin = new Vector2(0, 0.5f);
            drinksRect.anchorMax = new Vector2(1, 0.65f);
            drinksRect.offsetMin = new Vector2(20, 0);
            drinksRect.offsetMax = new Vector2(-20, 0);

            // Create Left Panel (Upgrades)
            GameObject upgradesPanel = CreateUIElement("UpgradesPanel", canvasObj.transform);
            Image upgradesPanelImg = upgradesPanel.AddComponent<Image>();
            upgradesPanelImg.color = new Color(0.15f, 0.25f, 0.15f, 0.9f);
            RectTransform upgradesRect = upgradesPanel.GetComponent<RectTransform>();
            upgradesRect.anchorMin = new Vector2(0.05f, 0.15f);
            upgradesRect.anchorMax = new Vector2(0.3f, 0.75f);
            upgradesRect.offsetMin = Vector2.zero;
            upgradesRect.offsetMax = Vector2.zero;

            // Upgrades Title
            GameObject upgradeTitleObj = CreateUIElement("UpgradesTitle", upgradesPanel.transform);
            Text upgradeTitleText = upgradeTitleObj.AddComponent<Text>();
            upgradeTitleText.text = "酒類升級 Liquor Upgrades";
            upgradeTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            upgradeTitleText.fontSize = 24;
            upgradeTitleText.alignment = TextAnchor.MiddleCenter;
            upgradeTitleText.color = Color.green;
            RectTransform upgradeTitleRect = upgradeTitleObj.GetComponent<RectTransform>();
            upgradeTitleRect.anchorMin = new Vector2(0, 0.9f);
            upgradeTitleRect.anchorMax = new Vector2(1, 1);
            upgradeTitleRect.offsetMin = new Vector2(10, -10);
            upgradeTitleRect.offsetMax = new Vector2(-10, -10);

            // Upgrades Scroll View
            GameObject upgradesScrollView = CreateScrollView("UpgradesScrollView", upgradesPanel.transform);
            RectTransform upgradesScrollRect = upgradesScrollView.GetComponent<RectTransform>();
            upgradesScrollRect.anchorMin = new Vector2(0, 0);
            upgradesScrollRect.anchorMax = new Vector2(1, 0.88f);
            upgradesScrollRect.offsetMin = new Vector2(10, 10);
            upgradesScrollRect.offsetMax = new Vector2(-10, -10);

            // Create Right Panel (Decorations)
            GameObject decorationsPanel = CreateUIElement("DecorationsPanel", canvasObj.transform);
            Image decorationsPanelImg = decorationsPanel.AddComponent<Image>();
            decorationsPanelImg.color = new Color(0.25f, 0.15f, 0.15f, 0.9f);
            RectTransform decorationsRect = decorationsPanel.GetComponent<RectTransform>();
            decorationsRect.anchorMin = new Vector2(0.7f, 0.15f);
            decorationsRect.anchorMax = new Vector2(0.95f, 0.75f);
            decorationsRect.offsetMin = Vector2.zero;
            decorationsRect.offsetMax = Vector2.zero;

            // Decorations Title
            GameObject decorationTitleObj = CreateUIElement("DecorationsTitle", decorationsPanel.transform);
            Text decorationTitleText = decorationTitleObj.AddComponent<Text>();
            decorationTitleText.text = "裝飾品 Decorations";
            decorationTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            decorationTitleText.fontSize = 24;
            decorationTitleText.alignment = TextAnchor.MiddleCenter;
            decorationTitleText.color = new Color(1f, 0.7f, 0.3f);
            RectTransform decorationTitleRect = decorationTitleObj.GetComponent<RectTransform>();
            decorationTitleRect.anchorMin = new Vector2(0, 0.9f);
            decorationTitleRect.anchorMax = new Vector2(1, 1);
            decorationTitleRect.offsetMin = new Vector2(10, -10);
            decorationTitleRect.offsetMax = new Vector2(-10, -10);

            // Decorations Scroll View
            GameObject decorationsScrollView = CreateScrollView("DecorationsScrollView", decorationsPanel.transform);
            RectTransform decorationsScrollRect = decorationsScrollView.GetComponent<RectTransform>();
            decorationsScrollRect.anchorMin = new Vector2(0, 0);
            decorationsScrollRect.anchorMax = new Vector2(1, 0.88f);
            decorationsScrollRect.offsetMin = new Vector2(10, 10);
            decorationsScrollRect.offsetMax = new Vector2(-10, -10);

            // Create Buttons Panel
            GameObject buttonsPanel = CreateUIElement("ButtonsPanel", canvasObj.transform);
            RectTransform buttonsPanelRect = buttonsPanel.GetComponent<RectTransform>();
            buttonsPanelRect.anchorMin = new Vector2(0.3f, 0.05f);
            buttonsPanelRect.anchorMax = new Vector2(0.7f, 0.15f);
            buttonsPanelRect.offsetMin = Vector2.zero;
            buttonsPanelRect.offsetMax = Vector2.zero;

            // Main Menu Button
            GameObject mainMenuBtn = CreateButton("MainMenuButton", "返回主選單 Main Menu", buttonsPanel.transform);
            RectTransform mainMenuRect = mainMenuBtn.GetComponent<RectTransform>();
            mainMenuRect.anchorMin = new Vector2(0, 0);
            mainMenuRect.anchorMax = new Vector2(0.45f, 1);
            mainMenuRect.offsetMin = new Vector2(10, 10);
            mainMenuRect.offsetMax = new Vector2(-10, -10);
            mainMenuBtn.GetComponent<Image>().color = new Color(0.8f, 0.3f, 0.3f);

            // Next Game Button
            GameObject nextGameBtn = CreateButton("NextGameButton", "下一局 Next Game", buttonsPanel.transform);
            RectTransform nextGameRect = nextGameBtn.GetComponent<RectTransform>();
            nextGameRect.anchorMin = new Vector2(0.55f, 0);
            nextGameRect.anchorMax = new Vector2(1, 1);
            nextGameRect.offsetMin = new Vector2(10, 10);
            nextGameRect.offsetMax = new Vector2(-10, -10);
            nextGameBtn.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f);

            // Add GameEndUIController component to Canvas
            var controller = canvasObj.AddComponent<BarSimulator.UI.GameEndUIController>();

            // Save scene
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("Game End UI created successfully!");
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            return obj;
        }

        private static GameObject CreateButton(string name, string text, Transform parent)
        {
            GameObject btnObj = CreateUIElement(name, parent);
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = Color.white;
            Button btn = btnObj.AddComponent<Button>();

            GameObject textObj = CreateUIElement("Text", btnObj.transform);
            Text btnText = textObj.AddComponent<Text>();
            btnText.text = text;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 24;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btnObj;
        }

        private static GameObject CreateScrollView(string name, Transform parent)
        {
            GameObject scrollViewObj = CreateUIElement(name, parent);
            Image scrollImage = scrollViewObj.AddComponent<Image>();
            scrollImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();

            // Create Viewport
            GameObject viewportObj = CreateUIElement("Viewport", scrollViewObj.transform);
            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0);
            Mask viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            // Create Content
            GameObject contentObj = CreateUIElement("Content", viewportObj.transform);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 600);

            // Add VerticalLayoutGroup to Content
            VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            // Add ContentSizeFitter
            ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Setup ScrollRect
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            return scrollViewObj;
        }
    }
}
