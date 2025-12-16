using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using BarSimulator.UI;

namespace BarSimulator.Editor
{
    /// <summary>
    /// Editor script to rebuild the GameEndPanel UI with new layout:
    /// Left (Liquor Upgrades) | Center (Stats) | Right (Decorations)
    /// </summary>
    public class RebuildGameEndPanel : MonoBehaviour
    {
        [MenuItem("Bar/UI/Rebuild GameEndPanel")]
        public static void Execute()
        {
            Debug.Log("RebuildGameEndPanel: Starting...");

            // Find UI_Canvas
            GameObject canvas = GameObject.Find("UI_Canvas");
            if (canvas == null)
            {
                Debug.LogError("RebuildGameEndPanel: UI_Canvas not found!");
                return;
            }

            // Find or create GameEndPanel
            Transform gameEndPanelTransform = canvas.transform.Find("GameEndPanel");
            GameObject gameEndPanel;
            
            if (gameEndPanelTransform != null)
            {
                gameEndPanel = gameEndPanelTransform.gameObject;
                Debug.Log("RebuildGameEndPanel: Found existing GameEndPanel, rebuilding...");
                
                // Clear all children except the GameEndUI component
                while (gameEndPanel.transform.childCount > 0)
                {
                    DestroyImmediate(gameEndPanel.transform.GetChild(0).gameObject);
                }
            }
            else
            {
                Debug.LogError("RebuildGameEndPanel: GameEndPanel not found!");
                return;
            }

            // Ensure GameEndPanel has proper components
            var panelRect = gameEndPanel.GetComponent<RectTransform>();
            if (panelRect == null)
            {
                panelRect = gameEndPanel.AddComponent<RectTransform>();
            }
            
            // Full screen
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Background image
            var panelImage = gameEndPanel.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = gameEndPanel.AddComponent<Image>();
            }
            panelImage.color = new Color(0, 0, 0, 0.9f);

            // Get or add GameEndUI component
            var gameEndUI = gameEndPanel.GetComponent<GameEndUI>();
            if (gameEndUI == null)
            {
                gameEndUI = gameEndPanel.AddComponent<GameEndUI>();
            }

            // Create main content panel
            GameObject contentPanel = CreateContentPanel(gameEndPanel);

            // Create three sections
            GameObject leftSection = CreateLeftSection(contentPanel); // Upgrades
            GameObject centerSection = CreateCenterSection(contentPanel); // Stats
            GameObject rightSection = CreateRightSection(contentPanel); // Decorations

            // Assign references to GameEndUI
            SerializedObject so = new SerializedObject(gameEndUI);
            
            so.FindProperty("gameEndPanel").objectReferenceValue = gameEndPanel;
            
            // Center section references
            so.FindProperty("titleText").objectReferenceValue = centerSection.transform.Find("TitleText").GetComponent<Text>();
            so.FindProperty("totalCoinsText").objectReferenceValue = centerSection.transform.Find("StatsContainer/TotalCoinsText").GetComponent<Text>();
            so.FindProperty("drinksServedText").objectReferenceValue = centerSection.transform.Find("StatsContainer/DrinksServedText").GetComponent<Text>();
            so.FindProperty("mainMenuButton").objectReferenceValue = centerSection.transform.Find("ButtonsContainer/MainMenuButton").GetComponent<Button>();
            so.FindProperty("nextGameButton").objectReferenceValue = centerSection.transform.Find("ButtonsContainer/NextGameButton").GetComponent<Button>();
            
            // Left section references
            so.FindProperty("upgradesContainer").objectReferenceValue = leftSection.transform.Find("Scroll View/Viewport/Content");
            
            // Right section references
            so.FindProperty("decorationsContainer").objectReferenceValue = rightSection.transform.Find("Scroll View/Viewport/Content");
            
            so.ApplyModifiedProperties();

            Debug.Log("RebuildGameEndPanel: Complete!");
            EditorUtility.SetDirty(gameEndPanel);
        }

        private static GameObject CreateContentPanel(GameObject parent)
        {
            GameObject contentPanel = new GameObject("ContentPanel");
            contentPanel.transform.SetParent(parent.transform, false);

            var rect = contentPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.1f);
            rect.anchorMax = new Vector2(0.95f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = contentPanel.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // Add horizontal layout
            var layout = contentPanel.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 10, 10);

            return contentPanel;
        }

        private static GameObject CreateLeftSection(GameObject parent)
        {
            GameObject leftSection = new GameObject("LeftSection_Upgrades");
            leftSection.transform.SetParent(parent.transform, false);

            var rect = leftSection.AddComponent<RectTransform>();
            var layoutElement = leftSection.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1;

            var image = leftSection.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            // Add vertical layout
            var layout = leftSection.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 5;
            layout.padding = new RectOffset(10, 10, 10, 10);

            // Title
            GameObject title = new GameObject("Title");
            title.transform.SetParent(leftSection.transform, false);
            var titleText = title.AddComponent<Text>();
            titleText.text = "Liquor Upgrades";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 20;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            var titleRect = title.GetComponent<RectTransform>();
            var titleLayout = title.AddComponent<LayoutElement>();
            titleLayout.minHeight = 40;

            // Scroll view for upgrades
            CreateScrollView(leftSection, "Upgrades");

            return leftSection;
        }

        private static GameObject CreateCenterSection(GameObject parent)
        {
            GameObject centerSection = new GameObject("CenterSection_Stats");
            centerSection.transform.SetParent(parent.transform, false);

            var rect = centerSection.AddComponent<RectTransform>();
            var layoutElement = centerSection.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1;

            var image = centerSection.AddComponent<Image>();
            image.color = new Color(0.2f, 0.15f, 0.15f, 1f);

            // Add vertical layout
            var layout = centerSection.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 20;
            layout.padding = new RectOffset(20, 20, 20, 20);

            // Title
            GameObject title = new GameObject("TitleText");
            title.transform.SetParent(centerSection.transform, false);
            var titleText = title.AddComponent<Text>();
            titleText.text = "Bar Closed - Time's Up!";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 28;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.yellow;
            var titleLayout = title.AddComponent<LayoutElement>();
            titleLayout.minHeight = 50;

            // Stats Container
            GameObject statsContainer = new GameObject("StatsContainer");
            statsContainer.transform.SetParent(centerSection.transform, false);
            var statsLayout = statsContainer.AddComponent<VerticalLayoutGroup>();
            statsLayout.childControlWidth = true;
            statsLayout.childControlHeight = false;
            statsLayout.childForceExpandWidth = true;
            statsLayout.spacing = 15;
            var statsLayoutElement = statsContainer.AddComponent<LayoutElement>();
            statsLayoutElement.minHeight = 150;

            // Total Coins Text
            CreateStatText(statsContainer, "TotalCoinsText", "Total Coins: $0", 24);

            // Drinks Served Text
            CreateStatText(statsContainer, "DrinksServedText", "Drinks Served: 0", 20);

            // Spacer
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(centerSection.transform, false);
            var spacerLayout = spacer.AddComponent<LayoutElement>();
            spacerLayout.flexibleHeight = 1;

            // Buttons Container
            GameObject buttonsContainer = new GameObject("ButtonsContainer");
            buttonsContainer.transform.SetParent(centerSection.transform, false);
            var buttonsLayout = buttonsContainer.AddComponent<VerticalLayoutGroup>();
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childControlHeight = false;
            buttonsLayout.childForceExpandWidth = true;
            buttonsLayout.spacing = 10;
            var buttonsLayoutElement = buttonsContainer.AddComponent<LayoutElement>();
            buttonsLayoutElement.minHeight = 120;

            // Main Menu Button
            CreateButton(buttonsContainer, "MainMenuButton", "Main Menu", new Color(0.6f, 0.3f, 0.3f, 1f));

            // Next Game Button
            CreateButton(buttonsContainer, "NextGameButton", "Next Game", new Color(0.3f, 0.6f, 0.3f, 1f));

            return centerSection;
        }

        private static GameObject CreateRightSection(GameObject parent)
        {
            GameObject rightSection = new GameObject("RightSection_Decorations");
            rightSection.transform.SetParent(parent.transform, false);

            var rect = rightSection.AddComponent<RectTransform>();
            var layoutElement = rightSection.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1;

            var image = rightSection.AddComponent<Image>();
            image.color = new Color(0.15f, 0.2f, 0.15f, 1f);

            // Add vertical layout
            var layout = rightSection.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 5;
            layout.padding = new RectOffset(10, 10, 10, 10);

            // Title
            GameObject title = new GameObject("Title");
            title.transform.SetParent(rightSection.transform, false);
            var titleText = title.AddComponent<Text>();
            titleText.text = "Decorations";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 20;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            var titleLayout = title.AddComponent<LayoutElement>();
            titleLayout.minHeight = 40;

            // Scroll view for decorations
            CreateScrollView(rightSection, "Decorations");

            return rightSection;
        }

        private static void CreateScrollView(GameObject parent, string contentName)
        {
            // Scroll View
            GameObject scrollView = new GameObject("Scroll View");
            scrollView.transform.SetParent(parent.transform, false);
            
            var scrollRect = scrollView.AddComponent<ScrollRect>();
            var scrollImage = scrollView.AddComponent<Image>();
            scrollImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            
            var scrollLayout = scrollView.AddComponent<LayoutElement>();
            scrollLayout.flexibleHeight = 1;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 500);

            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.spacing = 10;
            contentLayout.padding = new RectOffset(5, 5, 5, 5);

            var contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Setup ScrollRect
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 20;
        }

        private static void CreateStatText(GameObject parent, string name, string text, int fontSize)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);
            
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = fontSize;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = Color.white;
            
            var layoutElement = textObj.AddComponent<LayoutElement>();
            layoutElement.minHeight = 30;
        }

        private static void CreateButton(GameObject parent, string name, string text, Color color)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);
            
            var button = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = color;
            
            var layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.minHeight = 50;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 18;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = Color.white;
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
}
