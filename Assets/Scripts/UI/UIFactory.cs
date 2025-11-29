using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace BarSimulator.UI
{
    /// <summary>
    /// UI 工廠 - 程式化生成複雜 UI 面板並連接到管理器
    /// 解決從 Three.js 移植時缺少 Prefab 的問題
    /// </summary>
    public class UIFactory : MonoBehaviour
    {
        #region 單例

        private static UIFactory instance;
        public static UIFactory Instance => instance;

        #endregion

        #region 私有欄位

        private Canvas mainCanvas;
        private GameObject uiRoot;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        #endregion

        #region 主要建構方法

        /// <summary>
        /// 建構所有 UI 並連接到管理器
        /// </summary>
        public void BuildAllUI()
        {
            Debug.Log("UIFactory: Building all UI panels...");

            // 確保有主 Canvas
            EnsureMainCanvas();

            // 創建 UI 根容器
            CreateUIRoot();

            Debug.Log("UIFactory: All UI panels built successfully");
        }

        /// <summary>
        /// 確保主 Canvas 存在
        /// </summary>
        private void EnsureMainCanvas()
        {
            mainCanvas = FindAnyObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                var canvasObj = new GameObject("MainCanvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mainCanvas.sortingOrder = 100;

                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }

        /// <summary>
        /// 創建 UI 根容器
        /// </summary>
        private void CreateUIRoot()
        {
            uiRoot = new GameObject("UIRoot");
            uiRoot.transform.SetParent(mainCanvas.transform, false);

            var rectTransform = uiRoot.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        #endregion

        #region ShopPanel 建構

        /// <summary>
        /// 建構商店面板 UI
        /// </summary>
        public ShopUIReferences BuildShopUI()
        {
            var refs = new ShopUIReferences();

            // 創建商店面板
            refs.shopPanel = CreatePanel("ShopPanel", new Color(0.1f, 0.1f, 0.15f, 0.95f));
            refs.shopPanel.SetActive(false); // 預設隱藏

            // 標題
            var title = CreateText(refs.shopPanel.transform, "Title", "商店", 32, TextAlignmentOptions.Center);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.anchoredPosition = new Vector2(0, -30);
            titleRect.sizeDelta = new Vector2(-40, 50);

            // 金錢顯示
            refs.moneyText = CreateText(refs.shopPanel.transform, "MoneyText", "$1000", 28, TextAlignmentOptions.Right);
            var moneyRect = refs.moneyText.GetComponent<RectTransform>();
            moneyRect.anchorMin = new Vector2(1, 1);
            moneyRect.anchorMax = new Vector2(1, 1);
            moneyRect.anchoredPosition = new Vector2(-20, -30);
            moneyRect.sizeDelta = new Vector2(200, 50);
            refs.moneyText.color = Color.yellow;

            // Tab 按鈕容器
            var tabContainer = new GameObject("TabContainer");
            tabContainer.transform.SetParent(refs.shopPanel.transform, false);
            var tabContainerRect = tabContainer.AddComponent<RectTransform>();
            tabContainerRect.anchorMin = new Vector2(0, 1);
            tabContainerRect.anchorMax = new Vector2(1, 1);
            tabContainerRect.anchoredPosition = new Vector2(0, -90);
            tabContainerRect.sizeDelta = new Vector2(-40, 50);

            var tabLayoutGroup = tabContainer.AddComponent<HorizontalLayoutGroup>();
            tabLayoutGroup.spacing = 10;
            tabLayoutGroup.childForceExpandWidth = true;
            tabLayoutGroup.childControlWidth = true;

            // Liquor Tab 按鈕
            refs.liquorTabButton = CreateButton(tabContainer.transform, "LiquorTab", "酒類", 18);

            // Recipe Tab 按鈕
            refs.recipeTabButton = CreateButton(tabContainer.transform, "RecipeTab", "配方", 18);

            // Liquor Tab Panel
            refs.liquorTabPanel = CreateSubPanel(refs.shopPanel.transform, "LiquorTabPanel");
            var liquorRect = refs.liquorTabPanel.GetComponent<RectTransform>();
            liquorRect.anchorMin = new Vector2(0, 0);
            liquorRect.anchorMax = new Vector2(1, 1);
            liquorRect.offsetMin = new Vector2(20, 70);
            liquorRect.offsetMax = new Vector2(-20, -150);

            // 類別過濾按鈕
            var filterContainer = new GameObject("FilterContainer");
            filterContainer.transform.SetParent(refs.liquorTabPanel.transform, false);
            var filterRect = filterContainer.AddComponent<RectTransform>();
            filterRect.anchorMin = new Vector2(0, 1);
            filterRect.anchorMax = new Vector2(1, 1);
            filterRect.anchoredPosition = new Vector2(0, -25);
            filterRect.sizeDelta = new Vector2(0, 40);

            var filterLayout = filterContainer.AddComponent<HorizontalLayoutGroup>();
            filterLayout.spacing = 5;
            filterLayout.childForceExpandWidth = true;

            refs.allCategoryButton = CreateButton(filterContainer.transform, "AllBtn", "全部", 14);
            refs.baseSpiritButton = CreateButton(filterContainer.transform, "BaseBtn", "基酒", 14);
            refs.mixerButton = CreateButton(filterContainer.transform, "MixerBtn", "調味", 14);
            refs.juiceButton = CreateButton(filterContainer.transform, "JuiceBtn", "果汁", 14);
            refs.liqueurButton = CreateButton(filterContainer.transform, "LiqueurBtn", "利口酒", 14);

            // Liquor List (ScrollView)
            var liquorScrollView = CreateScrollView(refs.liquorTabPanel.transform, "LiquorScrollView");
            var liquorScrollRect = liquorScrollView.GetComponent<RectTransform>();
            liquorScrollRect.anchorMin = new Vector2(0, 0);
            liquorScrollRect.anchorMax = new Vector2(1, 1);
            liquorScrollRect.offsetMin = new Vector2(0, 0);
            liquorScrollRect.offsetMax = new Vector2(0, -60);

            refs.liquorListContainer = liquorScrollView.transform.Find("Viewport/Content");

            // Liquor Item Prefab (動態創建模板)
            refs.liquorItemPrefab = CreateLiquorItemPrefab();

            // Recipe Tab Panel
            refs.recipeTabPanel = CreateSubPanel(refs.shopPanel.transform, "RecipeTabPanel");
            var recipeRect = refs.recipeTabPanel.GetComponent<RectTransform>();
            recipeRect.anchorMin = new Vector2(0, 0);
            recipeRect.anchorMax = new Vector2(1, 1);
            recipeRect.offsetMin = new Vector2(20, 70);
            recipeRect.offsetMax = new Vector2(-20, -150);
            refs.recipeTabPanel.SetActive(false); // 預設隱藏

            // Recipe List (ScrollView)
            var recipeScrollView = CreateScrollView(refs.recipeTabPanel.transform, "RecipeScrollView");
            var recipeScrollRect = recipeScrollView.GetComponent<RectTransform>();
            recipeScrollRect.anchorMin = Vector2.zero;
            recipeScrollRect.anchorMax = Vector2.one;
            recipeScrollRect.offsetMin = Vector2.zero;
            recipeScrollRect.offsetMax = Vector2.zero;

            refs.recipeListContainer = recipeScrollView.transform.Find("Viewport/Content");

            // Recipe Item Prefab
            refs.recipeItemPrefab = CreateRecipeItemPrefab();

            // 關閉按鈕
            var closeBtn = CreateButton(refs.shopPanel.transform, "CloseBtn", "關閉 (ESC)", 16);
            var closeBtnRect = closeBtn.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.5f, 0);
            closeBtnRect.anchorMax = new Vector2(0.5f, 0);
            closeBtnRect.anchoredPosition = new Vector2(0, 30);
            closeBtnRect.sizeDelta = new Vector2(200, 50);

            return refs;
        }

        /// <summary>
        /// 創建酒類項目預製物模板
        /// </summary>
        private GameObject CreateLiquorItemPrefab()
        {
            var itemObj = new GameObject("LiquorItemPrefab");
            itemObj.SetActive(false); // 預製物不激活

            var rectTransform = itemObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 120);

            // 背景
            var bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);

            // 佈局
            var layout = itemObj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;

            // 名稱文字
            var nameText = CreateText(itemObj.transform, "NameText", "Vodka", 18, TextAlignmentOptions.Left);
            var nameLayout = nameText.AddComponent<LayoutElement>();
            nameLayout.preferredHeight = 25;

            // 等級文字
            var levelText = CreateText(itemObj.transform, "LevelText", "Level 1/3", 14, TextAlignmentOptions.Left);
            var levelLayout = levelText.AddComponent<LayoutElement>();
            levelLayout.preferredHeight = 20;

            // 描述文字
            var descText = CreateText(itemObj.transform, "DescriptionText", "Description here", 12, TextAlignmentOptions.Left);
            descText.color = new Color(0.8f, 0.8f, 0.8f);
            var descLayout = descText.AddComponent<LayoutElement>();
            descLayout.preferredHeight = 30;

            // 底部容器 (價格 + 按鈕)
            var bottomContainer = new GameObject("BottomContainer");
            bottomContainer.transform.SetParent(itemObj.transform, false);
            var bottomRect = bottomContainer.AddComponent<RectTransform>();
            var bottomLayout = bottomContainer.AddComponent<HorizontalLayoutGroup>();
            bottomLayout.spacing = 10;
            var bottomLayoutElem = bottomContainer.AddComponent<LayoutElement>();
            bottomLayoutElem.preferredHeight = 35;

            // 價格文字
            var priceText = CreateText(bottomContainer.transform, "PriceText", "$100", 16, TextAlignmentOptions.Left);
            priceText.color = Color.yellow;
            var priceLayout = priceText.AddComponent<LayoutElement>();
            priceLayout.flexibleWidth = 1;

            // 購買按鈕
            var purchaseBtn = CreateButton(bottomContainer.transform, "PurchaseButton", "購買", 14);
            var purchaseBtnLayout = purchaseBtn.AddComponent<LayoutElement>();
            purchaseBtnLayout.preferredWidth = 100;

            return itemObj;
        }

        /// <summary>
        /// 創建配方項目預製物模板
        /// </summary>
        private GameObject CreateRecipeItemPrefab()
        {
            var itemObj = new GameObject("RecipeItemPrefab");
            itemObj.SetActive(false);

            var rectTransform = itemObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 100);

            // 背景
            var bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);

            // 佈局
            var layout = itemObj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;

            // 名稱文字
            var nameText = CreateText(itemObj.transform, "NameText", "??? (鎖定)", 16, TextAlignmentOptions.Left);
            var nameLayout = nameText.AddComponent<LayoutElement>();
            nameLayout.preferredHeight = 22;

            // 難度文字
            var diffText = CreateText(itemObj.transform, "DifficultyText", "★★★", 14, TextAlignmentOptions.Left);
            diffText.color = Color.yellow;
            var diffLayout = diffText.AddComponent<LayoutElement>();
            diffLayout.preferredHeight = 20;

            // 描述文字
            var descText = CreateText(itemObj.transform, "DescriptionText", "解鎖以查看配方", 12, TextAlignmentOptions.Left);
            var descLayout = descText.AddComponent<LayoutElement>();
            descLayout.flexibleHeight = 1;

            // 底部
            var bottomContainer = new GameObject("BottomContainer");
            bottomContainer.transform.SetParent(itemObj.transform, false);
            var bottomLayout = bottomContainer.AddComponent<HorizontalLayoutGroup>();
            bottomLayout.spacing = 10;
            var bottomLayoutElem = bottomContainer.AddComponent<LayoutElement>();
            bottomLayoutElem.preferredHeight = 30;

            var priceText = CreateText(bottomContainer.transform, "PriceText", "$500", 14, TextAlignmentOptions.Left);
            priceText.color = Color.yellow;
            var priceLayout = priceText.AddComponent<LayoutElement>();
            priceLayout.flexibleWidth = 1;

            var purchaseBtn = CreateButton(bottomContainer.transform, "PurchaseButton", "解鎖", 14);
            var purchaseBtnLayout = purchaseBtn.AddComponent<LayoutElement>();
            purchaseBtnLayout.preferredWidth = 100;

            return itemObj;
        }

        #endregion

        #region SettingsPanel 建構

        /// <summary>
        /// 建構設定面板 UI
        /// </summary>
        public SettingsUIReferences BuildSettingsUI()
        {
            var refs = new SettingsUIReferences();

            // 創建設定面板
            refs.settingsPanel = CreatePanel("SettingsPanel", new Color(0.15f, 0.15f, 0.2f, 0.95f));
            refs.settingsPanel.SetActive(false);

            // 標題
            var title = CreateText(refs.settingsPanel.transform, "Title", "設定", 32, TextAlignmentOptions.Center);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.anchoredPosition = new Vector2(0, -30);
            titleRect.sizeDelta = new Vector2(-40, 50);

            // 捲軸視圖
            var scrollView = CreateScrollView(refs.settingsPanel.transform, "SettingsScrollView");
            var scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(20, 80);
            scrollRect.offsetMax = new Vector2(-20, -90);

            var contentTransform = scrollView.transform.Find("Viewport/Content");

            // 音訊設定區塊
            CreateSectionTitle(contentTransform, "音訊設定");

            refs.masterVolumeSlider = CreateSlider(contentTransform, "MasterVolumeSlider", "主音量", 0f, 1f, 1f);
            refs.musicVolumeSlider = CreateSlider(contentTransform, "MusicVolumeSlider", "音樂音量", 0f, 1f, 0.7f);
            refs.sfxVolumeSlider = CreateSlider(contentTransform, "SFXVolumeSlider", "音效音量", 0f, 1f, 1f);

            // 畫質設定區塊
            CreateSectionTitle(contentTransform, "畫質設定");

            refs.qualityDropdown = CreateDropdown(contentTransform, "QualityDropdown", "畫質等級");
            refs.fullscreenToggle = CreateToggle(contentTransform, "FullscreenToggle", "全螢幕", true);
            refs.vsyncToggle = CreateToggle(contentTransform, "VSyncToggle", "垂直同步", true);

            // 控制設定區塊
            CreateSectionTitle(contentTransform, "控制設定");

            refs.mouseSensitivitySlider = CreateSliderWithValue(contentTransform, "MouseSensitivitySlider",
                "滑鼠靈敏度", 0.1f, 3f, 1f, out refs.sensitivityValueText);

            // 按鈕容器
            var btnContainer = new GameObject("ButtonContainer");
            btnContainer.transform.SetParent(refs.settingsPanel.transform, false);
            var btnContainerRect = btnContainer.AddComponent<RectTransform>();
            btnContainerRect.anchorMin = new Vector2(0, 0);
            btnContainerRect.anchorMax = new Vector2(1, 0);
            btnContainerRect.anchoredPosition = new Vector2(0, 40);
            btnContainerRect.sizeDelta = new Vector2(-40, 60);

            var btnLayout = btnContainer.AddComponent<HorizontalLayoutGroup>();
            btnLayout.spacing = 10;
            btnLayout.childForceExpandWidth = true;

            refs.applyButton = CreateButton(btnContainer.transform, "ApplyButton", "套用", 16);
            refs.resetButton = CreateButton(btnContainer.transform, "ResetButton", "重設", 16);
            refs.closeButton = CreateButton(btnContainer.transform, "CloseButton", "關閉", 16);

            return refs;
        }

        #endregion

        #region MainMenuPanel 建構

        /// <summary>
        /// 建構主選單面板 UI (用於主選單場景)
        /// </summary>
        public MainMenuUIReferences BuildMainMenuUI()
        {
            var refs = new MainMenuUIReferences();

            // 主選單面板
            refs.mainMenuPanel = CreatePanel("MainMenuPanel", new Color(0.1f, 0.1f, 0.12f, 0.9f));

            // 遊戲標題
            var gameTitle = CreateText(refs.mainMenuPanel.transform, "GameTitle", "分子調酒模擬器", 48, TextAlignmentOptions.Center);
            var titleRect = gameTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -100);
            titleRect.sizeDelta = new Vector2(800, 80);
            gameTitle.fontStyle = FontStyles.Bold;

            // 按鈕容器
            var btnContainer = new GameObject("ButtonContainer");
            btnContainer.transform.SetParent(refs.mainMenuPanel.transform, false);
            var btnRect = btnContainer.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = Vector2.zero;
            btnRect.sizeDelta = new Vector2(400, 300);

            var btnLayout = btnContainer.AddComponent<VerticalLayoutGroup>();
            btnLayout.spacing = 15;
            btnLayout.childForceExpandWidth = true;
            btnLayout.childControlHeight = false;

            refs.newGameButton = CreateButton(btnContainer.transform, "NewGameButton", "新遊戲", 20);
            SetButtonHeight(refs.newGameButton, 60);

            refs.continueButton = CreateButton(btnContainer.transform, "ContinueButton", "繼續遊戲", 20);
            SetButtonHeight(refs.continueButton, 60);

            refs.settingsButton = CreateButton(btnContainer.transform, "SettingsButton", "設定", 20);
            SetButtonHeight(refs.settingsButton, 60);

            refs.quitButton = CreateButton(btnContainer.transform, "QuitButton", "離開遊戲", 20);
            SetButtonHeight(refs.quitButton, 60);

            // 存檔資訊
            refs.saveInfoText = CreateText(refs.mainMenuPanel.transform, "SaveInfoText", "無存檔", 14, TextAlignmentOptions.Left);
            var saveInfoRect = refs.saveInfoText.GetComponent<RectTransform>();
            saveInfoRect.anchorMin = new Vector2(0, 0);
            saveInfoRect.anchorMax = new Vector2(0, 0);
            saveInfoRect.anchoredPosition = new Vector2(20, 20);
            saveInfoRect.sizeDelta = new Vector2(300, 30);

            // 版本資訊
            refs.versionText = CreateText(refs.mainMenuPanel.transform, "VersionText", "v0.1.0", 12, TextAlignmentOptions.Right);
            var versionRect = refs.versionText.GetComponent<RectTransform>();
            versionRect.anchorMin = new Vector2(1, 0);
            versionRect.anchorMax = new Vector2(1, 0);
            versionRect.anchoredPosition = new Vector2(-20, 20);
            versionRect.sizeDelta = new Vector2(150, 30);

            // 新遊戲確認面板
            refs.newGameConfirmPanel = CreatePanel("NewGameConfirmPanel", new Color(0.15f, 0.15f, 0.18f, 0.95f), 600, 300);
            refs.newGameConfirmPanel.transform.SetParent(refs.mainMenuPanel.transform, false);
            refs.newGameConfirmPanel.SetActive(false);

            var confirmTitle = CreateText(refs.newGameConfirmPanel.transform, "ConfirmTitle", "確認新遊戲", 28, TextAlignmentOptions.Center);
            var confirmTitleRect = confirmTitle.GetComponent<RectTransform>();
            confirmTitleRect.anchorMin = new Vector2(0, 1);
            confirmTitleRect.anchorMax = new Vector2(1, 1);
            confirmTitleRect.anchoredPosition = new Vector2(0, -40);
            confirmTitleRect.sizeDelta = new Vector2(-40, 50);

            var confirmMsg = CreateText(refs.newGameConfirmPanel.transform, "ConfirmMessage",
                "開始新遊戲將會刪除現有存檔\n確定要繼續嗎？", 16, TextAlignmentOptions.Center);
            var confirmMsgRect = confirmMsg.GetComponent<RectTransform>();
            confirmMsgRect.anchorMin = new Vector2(0, 0.5f);
            confirmMsgRect.anchorMax = new Vector2(1, 0.5f);
            confirmMsgRect.anchoredPosition = Vector2.zero;
            confirmMsgRect.sizeDelta = new Vector2(-40, 80);

            var confirmBtnContainer = new GameObject("ConfirmButtonContainer");
            confirmBtnContainer.transform.SetParent(refs.newGameConfirmPanel.transform, false);
            var confirmBtnRect = confirmBtnContainer.AddComponent<RectTransform>();
            confirmBtnRect.anchorMin = new Vector2(0, 0);
            confirmBtnRect.anchorMax = new Vector2(1, 0);
            confirmBtnRect.anchoredPosition = new Vector2(0, 40);
            confirmBtnRect.sizeDelta = new Vector2(-40, 60);

            var confirmBtnLayout = confirmBtnContainer.AddComponent<HorizontalLayoutGroup>();
            confirmBtnLayout.spacing = 20;
            confirmBtnLayout.childForceExpandWidth = true;

            refs.confirmNewGameButton = CreateButton(confirmBtnContainer.transform, "ConfirmButton", "確認", 18);
            refs.cancelNewGameButton = CreateButton(confirmBtnContainer.transform, "CancelButton", "取消", 18);

            return refs;
        }

        #endregion

        #region 輔助方法 - UI 元素創建

        /// <summary>
        /// 創建面板
        /// </summary>
        private GameObject CreatePanel(string name, Color bgColor, float width = 0, float height = 0)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(uiRoot != null ? uiRoot.transform : mainCanvas.transform, false);

            var rectTransform = panel.AddComponent<RectTransform>();

            if (width > 0 && height > 0)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(width, height);
            }
            else
            {
                rectTransform.anchorMin = new Vector2(0.2f, 0.1f);
                rectTransform.anchorMax = new Vector2(0.8f, 0.9f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }

            var image = panel.AddComponent<Image>();
            image.color = bgColor;

            return panel;
        }

        /// <summary>
        /// 創建子面板
        /// </summary>
        private GameObject CreateSubPanel(Transform parent, string name)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            var rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            return panel;
        }

        /// <summary>
        /// 創建文字
        /// </summary>
        private TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            var rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 30);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;

            return tmp;
        }

        /// <summary>
        /// 創建按鈕
        /// </summary>
        private Button CreateButton(Transform parent, string name, string label, int fontSize)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var rectTransform = btnObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(160, 40);

            var image = btnObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.5f, 0.7f);

            var button = btnObj.AddComponent<Button>();
            button.targetGraphic = image;

            var colors = button.colors;
            colors.normalColor = new Color(0.3f, 0.5f, 0.7f);
            colors.highlightedColor = new Color(0.4f, 0.6f, 0.8f);
            colors.pressedColor = new Color(0.2f, 0.4f, 0.6f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            button.colors = colors;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return button;
        }

        /// <summary>
        /// 創建捲軸視圖
        /// </summary>
        private GameObject CreateScrollView(Transform parent, string name)
        {
            var scrollObj = new GameObject(name);
            scrollObj.transform.SetParent(parent, false);

            var scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;

            var scrollImage = scrollObj.AddComponent<Image>();
            scrollImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            var scrollViewComp = scrollObj.AddComponent<ScrollRect>();

            // Viewport
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);

            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            var mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;

            // Content
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 600);

            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 10;
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.childForceExpandWidth = true;
            contentLayout.childControlHeight = false;

            var contentSizeFitter = content.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollViewComp.content = contentRect;
            scrollViewComp.viewport = viewportRect;
            scrollViewComp.horizontal = false;
            scrollViewComp.vertical = true;

            return scrollObj;
        }

        /// <summary>
        /// 創建滑桿
        /// </summary>
        private Slider CreateSlider(Transform parent, string name, string label, float min, float max, float value)
        {
            var container = new GameObject(name + "_Container");
            container.transform.SetParent(parent, false);

            var containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, 60);

            var layoutElem = container.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 60;

            // 標籤
            var labelText = CreateText(container.transform, "Label", label, 16, TextAlignmentOptions.Left);
            var labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.anchoredPosition = new Vector2(0, -10);
            labelRect.sizeDelta = new Vector2(0, 25);

            // 滑桿
            var sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(container.transform, false);

            var sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0, 0);
            sliderRect.anchorMax = new Vector2(1, 0);
            sliderRect.anchoredPosition = new Vector2(0, 15);
            sliderRect.sizeDelta = new Vector2(0, 20);

            var slider = sliderObj.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);

            // Fill Area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.6f, 0.9f);

            slider.fillRect = fillRect;

            // Handle
            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            var handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = Vector2.zero;
            handleAreaRect.offsetMax = Vector2.zero;

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;

            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;

            return slider;
        }

        /// <summary>
        /// 創建帶數值顯示的滑桿
        /// </summary>
        private Slider CreateSliderWithValue(Transform parent, string name, string label,
            float min, float max, float value, out TextMeshProUGUI valueText)
        {
            var slider = CreateSlider(parent, name, label, min, max, value);
            var container = slider.transform.parent.gameObject;

            // 數值文字
            valueText = CreateText(container.transform, "ValueText", value.ToString("F2"), 14, TextAlignmentOptions.Right);
            var valueRect = valueText.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(1, 1);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.anchoredPosition = new Vector2(-10, -10);
            valueRect.sizeDelta = new Vector2(60, 25);

            return slider;
        }

        /// <summary>
        /// 創建下拉選單
        /// </summary>
        private TMP_Dropdown CreateDropdown(Transform parent, string name, string label)
        {
            var container = new GameObject(name + "_Container");
            container.transform.SetParent(parent, false);

            var containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, 60);

            var layoutElem = container.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 60;

            // 標籤
            var labelText = CreateText(container.transform, "Label", label, 16, TextAlignmentOptions.Left);
            var labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.anchoredPosition = new Vector2(0, -10);
            labelRect.sizeDelta = new Vector2(0, 25);

            // 下拉選單
            var dropdownObj = new GameObject(name);
            dropdownObj.transform.SetParent(container.transform, false);

            var dropdownRect = dropdownObj.AddComponent<RectTransform>();
            dropdownRect.anchorMin = new Vector2(0, 0);
            dropdownRect.anchorMax = new Vector2(1, 0);
            dropdownRect.anchoredPosition = new Vector2(0, 15);
            dropdownRect.sizeDelta = new Vector2(0, 30);

            var dropdownImage = dropdownObj.AddComponent<Image>();
            dropdownImage.color = new Color(0.2f, 0.2f, 0.25f);

            var dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

            // 簡化版下拉選單 (完整版需要更多設置)
            var labelObj = CreateText(dropdownObj.transform, "Label", "選項", 14, TextAlignmentOptions.Left);
            var dropdownLabelRect = labelObj.GetComponent<RectTransform>();
            dropdownLabelRect.anchorMin = new Vector2(0, 0);
            dropdownLabelRect.anchorMax = new Vector2(1, 1);
            dropdownLabelRect.offsetMin = new Vector2(10, 0);
            dropdownLabelRect.offsetMax = new Vector2(-25, 0);
            dropdown.captionText = labelObj;

            return dropdown;
        }

        /// <summary>
        /// 創建切換開關
        /// </summary>
        private Toggle CreateToggle(Transform parent, string name, string label, bool isOn)
        {
            var container = new GameObject(name + "_Container");
            container.transform.SetParent(parent, false);

            var containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, 40);

            var layoutElem = container.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 40;

            var toggleObj = new GameObject(name);
            toggleObj.transform.SetParent(container.transform, false);

            var toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0, 0.5f);
            toggleRect.anchorMax = new Vector2(0, 0.5f);
            toggleRect.anchoredPosition = new Vector2(20, 0);
            toggleRect.sizeDelta = new Vector2(30, 30);

            var toggleBg = toggleObj.AddComponent<Image>();
            toggleBg.color = new Color(0.2f, 0.2f, 0.25f);

            var toggle = toggleObj.AddComponent<Toggle>();
            toggle.isOn = isOn;

            // Checkmark
            var checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(toggleObj.transform, false);
            var checkRect = checkmark.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            var checkImage = checkmark.AddComponent<Image>();
            checkImage.color = new Color(0.3f, 0.7f, 0.3f);

            toggle.graphic = checkImage;
            toggle.targetGraphic = toggleBg;

            // 標籤
            var labelText = CreateText(container.transform, "Label", label, 16, TextAlignmentOptions.Left);
            var labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.5f);
            labelRect.anchorMax = new Vector2(1, 0.5f);
            labelRect.anchoredPosition = new Vector2(70, 0);
            labelRect.sizeDelta = new Vector2(-80, 30);

            return toggle;
        }

        /// <summary>
        /// 創建區塊標題
        /// </summary>
        private void CreateSectionTitle(Transform parent, string title)
        {
            var titleObj = new GameObject("SectionTitle_" + title);
            titleObj.transform.SetParent(parent, false);

            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 40);

            var layoutElem = titleObj.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 40;

            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = new Color(0.8f, 0.9f, 1f);

            // 分隔線
            var line = new GameObject("Line");
            line.transform.SetParent(titleObj.transform, false);
            var lineRect = line.AddComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0, 0);
            lineRect.anchorMax = new Vector2(1, 0);
            lineRect.sizeDelta = new Vector2(0, 2);
            var lineImage = line.AddComponent<Image>();
            lineImage.color = new Color(0.3f, 0.4f, 0.5f);
        }

        /// <summary>
        /// 設置按鈕高度
        /// </summary>
        private void SetButtonHeight(Button button, float height)
        {
            var layoutElem = button.gameObject.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = height;
        }

        #endregion
    }

    #region UI 引用結構

    /// <summary>
    /// 商店 UI 引用
    /// </summary>
    public class ShopUIReferences
    {
        public GameObject shopPanel;
        public TextMeshProUGUI moneyText;
        public Transform liquorListContainer;
        public Transform recipeListContainer;
        public GameObject liquorItemPrefab;
        public GameObject recipeItemPrefab;
        public Button liquorTabButton;
        public Button recipeTabButton;
        public GameObject liquorTabPanel;
        public GameObject recipeTabPanel;
        public Button allCategoryButton;
        public Button baseSpiritButton;
        public Button mixerButton;
        public Button juiceButton;
        public Button liqueurButton;
    }

    /// <summary>
    /// 設定 UI 引用
    /// </summary>
    public class SettingsUIReferences
    {
        public GameObject settingsPanel;
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public TMP_Dropdown qualityDropdown;
        public Toggle fullscreenToggle;
        public Toggle vsyncToggle;
        public Slider mouseSensitivitySlider;
        public TextMeshProUGUI sensitivityValueText;
        public Button applyButton;
        public Button resetButton;
        public Button closeButton;
    }

    /// <summary>
    /// 主選單 UI 引用
    /// </summary>
    public class MainMenuUIReferences
    {
        public GameObject mainMenuPanel;
        public GameObject newGameConfirmPanel;
        public Button newGameButton;
        public Button continueButton;
        public Button settingsButton;
        public Button quitButton;
        public Button confirmNewGameButton;
        public Button cancelNewGameButton;
        public TextMeshProUGUI saveInfoText;
        public TextMeshProUGUI versionText;
    }

    #endregion
}
