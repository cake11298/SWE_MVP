using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using BarSimulator.Data;
using BarSimulator.Systems;
using System.Collections.Generic;

namespace BarSimulator.UI
{
    /// <summary>
    /// 商店管理系統 - 顯示和管理酒類升級、配方解鎖商店
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        #region Singleton

        private static ShopManager instance;
        public static ShopManager Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("UI References")]
        [Tooltip("商店面板")]
        [SerializeField] private GameObject shopPanel;

        [Tooltip("金錢顯示文字")]
        [SerializeField] private TextMeshProUGUI moneyText;

        [Header("Liquor Shop")]
        [Tooltip("酒類商店容器")]
        [SerializeField] private Transform liquorListContainer;

        [Tooltip("酒類項目預製物")]
        [SerializeField] private GameObject liquorItemPrefab;

        [Header("Recipe Shop")]
        [Tooltip("配方商店容器")]
        [SerializeField] private Transform recipeListContainer;

        [Tooltip("配方項目預製物")]
        [SerializeField] private GameObject recipeItemPrefab;

        [Header("Tabs")]
        [Tooltip("酒類Tab按鈕")]
        [SerializeField] private Button liquorTabButton;

        [Tooltip("配方Tab按鈕")]
        [SerializeField] private Button recipeTabButton;

        [Tooltip("酒類Tab面板")]
        [SerializeField] private GameObject liquorTabPanel;

        [Tooltip("配方Tab面板")]
        [SerializeField] private GameObject recipeTabPanel;

        [Header("Category Filters (Liquor)")]
        [Tooltip("全部按鈕")]
        [SerializeField] private Button allCategoryButton;

        [Tooltip("基酒按鈕")]
        [SerializeField] private Button baseSpiritButton;

        [Tooltip("調味料按鈕")]
        [SerializeField] private Button mixerButton;

        [Tooltip("果汁按鈕")]
        [SerializeField] private Button juiceButton;

        [Tooltip("利口酒按鈕")]
        [SerializeField] private Button liqueurButton;

        [Header("Input")]
        [Tooltip("Input Actions Asset")]
        [SerializeField] private InputActionAsset inputActions;

        #endregion

        #region 私有欄位

        private bool isOpen = false;
        private ShopTab currentTab = ShopTab.Liquor;
        private LiquorCategory currentLiquorFilter = LiquorCategory.BaseSpirit;
        private bool showAllCategories = true;

        private InputAction toggleAction;
        private bool useFallbackInput = false;

        private List<GameObject> liquorItems = new List<GameObject>();
        private List<GameObject> recipeItems = new List<GameObject>();

        private UpgradeSystem upgradeSystem;

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

            SetupInputActions();
        }

        /// <summary>
        /// 初始化 UI 引用（從外部注入）
        /// </summary>
        public void InitializeReferences(ShopUIReferences refs)
        {
            shopPanel = refs.shopPanel;
            moneyText = refs.moneyText;
            liquorListContainer = refs.liquorListContainer;
            recipeListContainer = refs.recipeListContainer;
            liquorItemPrefab = refs.liquorItemPrefab;
            recipeItemPrefab = refs.recipeItemPrefab;
            liquorTabButton = refs.liquorTabButton;
            recipeTabButton = refs.recipeTabButton;
            liquorTabPanel = refs.liquorTabPanel;
            recipeTabPanel = refs.recipeTabPanel;
            allCategoryButton = refs.allCategoryButton;
            baseSpiritButton = refs.baseSpiritButton;
            mixerButton = refs.mixerButton;
            juiceButton = refs.juiceButton;
            liqueurButton = refs.liqueurButton;

            SetupUI();
            Debug.Log("ShopManager: UI references initialized");
        }

        private void Start()
        {
            // Get UpgradeSystem reference
            upgradeSystem = UpgradeSystem.Instance;
            if (upgradeSystem == null)
            {
                Debug.LogError("ShopManager: UpgradeSystem not found!");
            }
            else
            {
                // Subscribe to money changes
                upgradeSystem.OnMoneyChanged += UpdateMoneyDisplay;
                upgradeSystem.OnLiquorUpgraded += OnLiquorUpgraded;
                upgradeSystem.OnLiquorUnlocked += OnLiquorUnlocked;
                upgradeSystem.OnRecipeUnlocked += OnRecipeUnlocked;

                // Initial money display
                UpdateMoneyDisplay(upgradeSystem.CurrentMoney);
            }
        }

        private void OnEnable()
        {
            toggleAction?.Enable();
        }

        private void OnDisable()
        {
            toggleAction?.Disable();
        }

        private void OnDestroy()
        {
            if (upgradeSystem != null)
            {
                upgradeSystem.OnMoneyChanged -= UpdateMoneyDisplay;
                upgradeSystem.OnLiquorUpgraded -= OnLiquorUpgraded;
                upgradeSystem.OnLiquorUnlocked -= OnLiquorUnlocked;
                upgradeSystem.OnRecipeUnlocked -= OnRecipeUnlocked;
            }
        }

        private void Update()
        {
            // B鍵切換商店（可以改成其他按鍵）
            bool togglePressed = false;
            if (useFallbackInput)
            {
                togglePressed = Input.GetKeyDown(KeyCode.B);
            }
            else
            {
                togglePressed = toggleAction != null && toggleAction.WasPressedThisFrame();
            }

            if (togglePressed)
            {
                ToggleShop();
            }

            // ESC鍵關閉
            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop();
            }
        }

        #endregion

        #region 初始化

        private void SetupInputActions()
        {
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("PlayerInputActions");
            }

            if (inputActions == null)
            {
                Debug.LogWarning("ShopManager: PlayerInputActions not found, using fallback input");
                useFallbackInput = true;
                return;
            }

            var uiMap = inputActions.FindActionMap("UI");
            if (uiMap != null)
            {
                toggleAction = uiMap.FindAction("ToggleShop");
                if (toggleAction == null)
                {
                    Debug.LogWarning("ShopManager: ToggleShop action not found, using fallback input (B key)");
                    useFallbackInput = true;
                }
            }
            else
            {
                Debug.LogWarning("ShopManager: UI action map not found, using fallback input");
                useFallbackInput = true;
            }
        }

        private void SetupUI()
        {
            // 初始化隱藏
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            // Setup tab buttons
            if (liquorTabButton != null)
            {
                liquorTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Liquor));
            }

            if (recipeTabButton != null)
            {
                recipeTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Recipe));
            }

            // Setup category filter buttons
            if (allCategoryButton != null)
            {
                allCategoryButton.onClick.AddListener(() => SetLiquorFilter(LiquorCategory.BaseSpirit, true));
            }

            if (baseSpiritButton != null)
            {
                baseSpiritButton.onClick.AddListener(() => SetLiquorFilter(LiquorCategory.BaseSpirit, false));
            }

            if (mixerButton != null)
            {
                mixerButton.onClick.AddListener(() => SetLiquorFilter(LiquorCategory.Mixer, false));
            }

            if (juiceButton != null)
            {
                juiceButton.onClick.AddListener(() => SetLiquorFilter(LiquorCategory.Juice, false));
            }

            if (liqueurButton != null)
            {
                liqueurButton.onClick.AddListener(() => SetLiquorFilter(LiquorCategory.Liqueur, false));
            }

            // Show liquor tab by default
            SwitchTab(ShopTab.Liquor);
        }

        #endregion

        #region 開關控制

        /// <summary>
        /// 切換商店開關
        /// </summary>
        public void ToggleShop()
        {
            if (isOpen)
            {
                CloseShop();
            }
            else
            {
                OpenShop();
            }
        }

        /// <summary>
        /// 開啟商店
        /// </summary>
        public void OpenShop()
        {
            if (shopPanel == null) return;

            isOpen = true;
            shopPanel.SetActive(true);

            // Refresh lists
            RefreshLiquorList();
            RefreshRecipeList();

            // Pause game
            Time.timeScale = 0f;

            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("ShopManager: Opened");
        }

        /// <summary>
        /// 關閉商店
        /// </summary>
        public void CloseShop()
        {
            if (shopPanel == null) return;

            isOpen = false;
            shopPanel.SetActive(false);

            // Resume game
            Time.timeScale = 1f;

            // Hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("ShopManager: Closed");
        }

        #endregion

        #region Tab 切換

        /// <summary>
        /// 切換商店Tab
        /// </summary>
        private void SwitchTab(ShopTab tab)
        {
            currentTab = tab;

            if (liquorTabPanel != null)
            {
                liquorTabPanel.SetActive(tab == ShopTab.Liquor);
            }

            if (recipeTabPanel != null)
            {
                recipeTabPanel.SetActive(tab == ShopTab.Recipe);
            }

            Debug.Log($"ShopManager: Switched to {tab} tab");
        }

        #endregion

        #region 酒類列表

        /// <summary>
        /// 設置酒類類別篩選
        /// </summary>
        private void SetLiquorFilter(LiquorCategory category, bool showAll)
        {
            currentLiquorFilter = category;
            showAllCategories = showAll;
            RefreshLiquorList();
        }

        /// <summary>
        /// 重新整理酒類列表
        /// </summary>
        private void RefreshLiquorList()
        {
            // Clear existing items
            foreach (var item in liquorItems)
            {
                Destroy(item);
            }
            liquorItems.Clear();

            if (upgradeSystem == null || upgradeSystem.LiquorDatabase == null) return;
            if (liquorListContainer == null || liquorItemPrefab == null) return;

            var database = upgradeSystem.LiquorDatabase;
            var liquors = database.liquors;

            if (liquors == null) return;

            // Create liquor items
            foreach (var liquor in liquors)
            {
                // Apply category filter
                if (!showAllCategories && liquor.category != currentLiquorFilter)
                {
                    continue;
                }

                GameObject item = Instantiate(liquorItemPrefab, liquorListContainer);
                liquorItems.Add(item);

                SetupLiquorItem(item, liquor);
            }
        }

        /// <summary>
        /// 設置酒類項目UI
        /// </summary>
        private void SetupLiquorItem(GameObject item, LiquorData liquor)
        {
            // Name
            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = liquor.isLocked ? $"{liquor.displayName} (鎖定)" : liquor.displayName;
            }

            // Level
            var levelText = item.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            if (levelText != null)
            {
                if (liquor.isLocked)
                {
                    levelText.text = "需要解鎖";
                }
                else
                {
                    levelText.text = $"Level {liquor.level}/3";
                }
            }

            // Price
            var priceText = item.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            var button = item.transform.Find("PurchaseButton")?.GetComponent<Button>();

            if (liquor.isLocked)
            {
                // Unlock button
                if (priceText != null)
                {
                    priceText.text = $"${liquor.unlockPrice}";
                }

                if (button != null)
                {
                    var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "解鎖";
                    }

                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => PurchaseUnlockLiquor(liquor.id));

                    // Check affordability
                    button.interactable = upgradeSystem.CanAfford(liquor.unlockPrice);
                }
            }
            else if (liquor.level < 3)
            {
                // Upgrade button
                int upgradePrice = upgradeSystem.GetLiquorUpgradePrice(liquor.id);

                if (priceText != null)
                {
                    priceText.text = upgradePrice >= 0 ? $"${upgradePrice}" : "N/A";
                }

                if (button != null)
                {
                    var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "升級";
                    }

                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => PurchaseUpgradeLiquor(liquor.id));

                    // Check affordability
                    button.interactable = upgradePrice >= 0 && upgradeSystem.CanAfford(upgradePrice);
                }
            }
            else
            {
                // Max level
                if (priceText != null)
                {
                    priceText.text = "最高等級";
                }

                if (button != null)
                {
                    button.interactable = false;
                    var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "已滿級";
                    }
                }
            }

            // Description
            var descText = item.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
            {
                if (liquor.isLocked)
                {
                    descText.text = "解鎖此酒類以在調酒中使用";
                }
                else if (liquor.level < 3)
                {
                    int levelIndex = Mathf.Clamp(liquor.level, 0, liquor.levelDescriptions.Length - 1);
                    descText.text = liquor.levelDescriptions[levelIndex];
                }
                else
                {
                    descText.text = liquor.levelDescriptions[2];
                }
            }
        }

        #endregion

        #region 配方列表

        /// <summary>
        /// 重新整理配方列表
        /// </summary>
        private void RefreshRecipeList()
        {
            // Clear existing items
            foreach (var item in recipeItems)
            {
                Destroy(item);
            }
            recipeItems.Clear();

            if (upgradeSystem == null || upgradeSystem.RecipeDatabase == null) return;
            if (recipeListContainer == null || recipeItemPrefab == null) return;

            var database = upgradeSystem.RecipeDatabase;
            var recipes = database.recipes;

            if (recipes == null) return;

            // Create recipe items (only show locked recipes that can be purchased)
            foreach (var recipe in recipes)
            {
                if (!recipe.isLocked || recipe.unlockPrice <= 0) continue;

                GameObject item = Instantiate(recipeItemPrefab, recipeListContainer);
                recipeItems.Add(item);

                SetupRecipeItem(item, recipe);
            }
        }

        /// <summary>
        /// 設置配方項目UI
        /// </summary>
        private void SetupRecipeItem(GameObject item, RecipeData recipe)
        {
            // Name
            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = "??? (鎖定配方)";
            }

            // Difficulty
            var difficultyText = item.transform.Find("DifficultyText")?.GetComponent<TextMeshProUGUI>();
            if (difficultyText != null)
            {
                string stars = "";
                for (int i = 0; i < recipe.difficulty; i++)
                {
                    stars += "★";
                }
                difficultyText.text = stars;
            }

            // Price
            var priceText = item.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            if (priceText != null)
            {
                priceText.text = $"${recipe.unlockPrice}";
            }

            // Button
            var button = item.transform.Find("PurchaseButton")?.GetComponent<Button>();
            if (button != null)
            {
                var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "解鎖";
                }

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => PurchaseUnlockRecipe(recipe.name));

                // Check affordability
                button.interactable = upgradeSystem.CanAfford(recipe.unlockPrice);
            }

            // Description
            var descText = item.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
            {
                descText.text = "解鎖以查看完整配方";
            }
        }

        #endregion

        #region 購買處理

        /// <summary>
        /// 購買酒類升級
        /// </summary>
        private void PurchaseUpgradeLiquor(string liquorId)
        {
            if (upgradeSystem == null) return;

            bool success = upgradeSystem.UpgradeLiquor(liquorId);
            if (success)
            {
                // Refresh to update UI
                RefreshLiquorList();
                Debug.Log($"ShopManager: Successfully upgraded {liquorId}");
            }
            else
            {
                Debug.LogWarning($"ShopManager: Failed to upgrade {liquorId}");
            }
        }

        /// <summary>
        /// 購買酒類解鎖
        /// </summary>
        private void PurchaseUnlockLiquor(string liquorId)
        {
            if (upgradeSystem == null) return;

            bool success = upgradeSystem.UnlockLiquor(liquorId);
            if (success)
            {
                RefreshLiquorList();
                Debug.Log($"ShopManager: Successfully unlocked {liquorId}");
            }
            else
            {
                Debug.LogWarning($"ShopManager: Failed to unlock {liquorId}");
            }
        }

        /// <summary>
        /// 購買配方解鎖
        /// </summary>
        private void PurchaseUnlockRecipe(string recipeName)
        {
            if (upgradeSystem == null) return;

            bool success = upgradeSystem.UnlockRecipe(recipeName);
            if (success)
            {
                RefreshRecipeList();
                Debug.Log($"ShopManager: Successfully unlocked recipe {recipeName}");
            }
            else
            {
                Debug.LogWarning($"ShopManager: Failed to unlock recipe {recipeName}");
            }
        }

        #endregion

        #region 事件處理

        /// <summary>
        /// 更新金錢顯示
        /// </summary>
        private void UpdateMoneyDisplay(int money)
        {
            if (moneyText != null)
            {
                moneyText.text = $"${money}";
            }
        }

        /// <summary>
        /// 酒類升級事件處理
        /// </summary>
        private void OnLiquorUpgraded(string liquorId, int newLevel)
        {
            RefreshLiquorList();
        }

        /// <summary>
        /// 酒類解鎖事件處理
        /// </summary>
        private void OnLiquorUnlocked(string liquorId)
        {
            RefreshLiquorList();
        }

        /// <summary>
        /// 配方解鎖事件處理
        /// </summary>
        private void OnRecipeUnlocked(string recipeName)
        {
            RefreshRecipeList();
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 商店是否開啟
        /// </summary>
        public bool IsOpen => isOpen;

        #endregion
    }

    /// <summary>
    /// 商店Tab類型
    /// </summary>
    public enum ShopTab
    {
        Liquor,
        Recipe
    }
}
