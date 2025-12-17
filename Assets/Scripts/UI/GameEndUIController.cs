using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BarSimulator.Core;
using BarSimulator.Data;
using System.Collections.Generic;

namespace BarSimulator.UI
{
    /// <summary>
    /// Game End Scene UI Controller
    /// Handles shop, upgrades, decorations, and scene transitions
    /// </summary>
    public class GameEndUIController : MonoBehaviour
    {
        [Header("Stats Display")]
        [SerializeField] private Text coinsText;
        [SerializeField] private Text drinksServedText;

        [Header("Scroll View Contents")]
        [SerializeField] private Transform upgradesContent;
        [SerializeField] private Transform decorationsContent;

        [Header("Buttons")]
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button nextGameButton;

        private Dictionary<BaseLiquorType, UpgradeItemUI> upgradeItems = new Dictionary<BaseLiquorType, UpgradeItemUI>();
        private Dictionary<DecorationType, DecorationItemUI> decorationItems = new Dictionary<DecorationType, DecorationItemUI>();

        private void Start()
        {
            // Auto-find references if not set
            if (coinsText == null)
                coinsText = GameObject.Find("CoinsText")?.GetComponent<Text>();
            if (drinksServedText == null)
                drinksServedText = GameObject.Find("DrinksServedText")?.GetComponent<Text>();
            if (upgradesContent == null)
                upgradesContent = GameObject.Find("UpgradesScrollView/Viewport/Content")?.transform;
            if (decorationsContent == null)
                decorationsContent = GameObject.Find("DecorationsScrollView/Viewport/Content")?.transform;
            if (mainMenuButton == null)
                mainMenuButton = GameObject.Find("MainMenuButton")?.GetComponent<Button>();
            if (nextGameButton == null)
                nextGameButton = GameObject.Find("NextGameButton")?.GetComponent<Button>();

            // Setup buttons
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            if (nextGameButton != null)
                nextGameButton.onClick.AddListener(OnNextGameClicked);

            // Subscribe to events
            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.OnCoinsChanged += OnCoinsChanged;
                PersistentGameData.Instance.OnLiquorUpgraded += OnLiquorUpgraded;
                PersistentGameData.Instance.OnDecorationPurchased += OnDecorationPurchased;
            }

            // Initialize UI
            UpdateStats();
            CreateUpgradeItems();
            CreateDecorationItems();

            // Unlock cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.OnCoinsChanged -= OnCoinsChanged;
                PersistentGameData.Instance.OnLiquorUpgraded -= OnLiquorUpgraded;
                PersistentGameData.Instance.OnDecorationPurchased -= OnDecorationPurchased;
            }

            // Remove button listeners
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            if (nextGameButton != null)
                nextGameButton.onClick.RemoveListener(OnNextGameClicked);
        }

        #region Stats Display

        private void UpdateStats()
        {
            if (coinsText != null && PersistentGameData.Instance != null)
            {
                int coins = PersistentGameData.Instance.GetTotalCoins();
                coinsText.text = $"總金幣 Total Coins: ${coins}";
            }

            if (drinksServedText != null && GameManager.Instance != null)
            {
                var score = GameManager.Instance.GetScore();
                drinksServedText.text = $"服務杯數 Drinks Served: {score.totalDrinks}";
            }
        }

        #endregion

        #region Upgrade Items

        private void CreateUpgradeItems()
        {
            if (upgradesContent == null || PersistentGameData.Instance == null) return;

            // Clear existing
            foreach (Transform child in upgradesContent)
            {
                Destroy(child.gameObject);
            }
            upgradeItems.Clear();

            // Create items for each liquor type
            var upgrades = PersistentGameData.Instance.GetAllLiquorUpgrades();
            foreach (var upgrade in upgrades)
            {
                GameObject itemObj = CreateUpgradeItemUI(upgrade);
                if (itemObj != null)
                {
                    itemObj.transform.SetParent(upgradesContent, false);
                    var uiItem = itemObj.GetComponent<UpgradeItemUI>();
                    if (uiItem != null)
                    {
                        upgradeItems[upgrade.liquorType] = uiItem;
                    }
                }
            }
        }

        private GameObject CreateUpgradeItemUI(LiquorUpgradeData upgrade)
        {
            GameObject itemObj = new GameObject($"Upgrade_{upgrade.liquorType}");
            
            // Add RectTransform
            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 80);

            // Add UpgradeItemUI component
            var uiItem = itemObj.AddComponent<UpgradeItemUI>();
            uiItem.Initialize(upgrade);

            return itemObj;
        }

        #endregion

        #region Decoration Items

        private void CreateDecorationItems()
        {
            if (decorationsContent == null || PersistentGameData.Instance == null) return;

            // Clear existing
            foreach (Transform child in decorationsContent)
            {
                Destroy(child.gameObject);
            }
            decorationItems.Clear();

            // Create items for each decoration
            var decorations = PersistentGameData.Instance.GetAllDecorations();
            foreach (var decoration in decorations)
            {
                GameObject itemObj = CreateDecorationItemUI(decoration);
                if (itemObj != null)
                {
                    itemObj.transform.SetParent(decorationsContent, false);
                    var uiItem = itemObj.GetComponent<DecorationItemUI>();
                    if (uiItem != null)
                    {
                        decorationItems[decoration.decorationType] = uiItem;
                    }
                }
            }
        }

        private GameObject CreateDecorationItemUI(DecorationData decoration)
        {
            GameObject itemObj = new GameObject($"Decoration_{decoration.decorationType}");
            
            // Add RectTransform
            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 100);

            // Add DecorationItemUI component
            var uiItem = itemObj.AddComponent<DecorationItemUI>();
            uiItem.Initialize(decoration);

            return itemObj;
        }

        #endregion

        #region Event Handlers

        private void OnCoinsChanged(int newTotal)
        {
            UpdateStats();
            
            // Update all upgrade buttons
            foreach (var item in upgradeItems.Values)
            {
                item.UpdateDisplay();
            }

            // Update all decoration buttons
            foreach (var item in decorationItems.Values)
            {
                item.UpdateDisplay();
            }
        }

        private void OnLiquorUpgraded(BaseLiquorType type, int newLevel)
        {
            if (upgradeItems.ContainsKey(type))
            {
                upgradeItems[type].UpdateDisplay();
            }
        }

        private void OnDecorationPurchased(DecorationType type)
        {
            if (decorationItems.ContainsKey(type))
            {
                decorationItems[type].UpdateDisplay();
            }
        }

        #endregion

        #region Button Handlers

        private void OnMainMenuClicked()
        {
            Debug.Log("GameEndUIController: Returning to main menu");
            SceneManager.LoadScene("MainMenu");
        }

        private void OnNextGameClicked()
        {
            Debug.Log("GameEndUIController: Starting next game");
            // Coins and upgrades persist via PersistentGameData
            SceneManager.LoadScene("TheBar");
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// UI item for liquor upgrade
        /// </summary>
        public class UpgradeItemUI : MonoBehaviour
        {
            private LiquorUpgradeData upgradeData;
            private Text nameText;
            private Text levelText;
            private Button upgradeButton;
            private Text buttonText;

            public void Initialize(LiquorUpgradeData data)
            {
                upgradeData = data;
                CreateUI();
                UpdateDisplay();
            }

            private void CreateUI()
            {
                // Background
                Image bg = gameObject.AddComponent<Image>();
                bg.color = new Color(0.2f, 0.3f, 0.2f, 0.8f);

                // Name text
                GameObject nameObj = new GameObject("NameText");
                nameObj.transform.SetParent(transform, false);
                nameText = nameObj.AddComponent<Text>();
                nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                nameText.fontSize = 18;
                nameText.alignment = TextAnchor.MiddleLeft;
                nameText.color = Color.white;
                RectTransform nameRect = nameObj.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0, 0.5f);
                nameRect.anchorMax = new Vector2(0.5f, 1);
                nameRect.offsetMin = new Vector2(10, 0);
                nameRect.offsetMax = new Vector2(-5, -5);

                // Level text
                GameObject levelObj = new GameObject("LevelText");
                levelObj.transform.SetParent(transform, false);
                levelText = levelObj.AddComponent<Text>();
                levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                levelText.fontSize = 16;
                levelText.alignment = TextAnchor.MiddleLeft;
                levelText.color = Color.yellow;
                RectTransform levelRect = levelObj.GetComponent<RectTransform>();
                levelRect.anchorMin = new Vector2(0, 0);
                levelRect.anchorMax = new Vector2(0.5f, 0.5f);
                levelRect.offsetMin = new Vector2(10, 5);
                levelRect.offsetMax = new Vector2(-5, 0);

                // Upgrade button
                GameObject buttonObj = new GameObject("UpgradeButton");
                buttonObj.transform.SetParent(transform, false);
                upgradeButton = buttonObj.AddComponent<Button>();
                Image buttonImage = buttonObj.AddComponent<Image>();
                buttonImage.color = new Color(0.3f, 0.7f, 0.3f, 1f);
                RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.5f, 0.1f);
                buttonRect.anchorMax = new Vector2(0.95f, 0.9f);
                buttonRect.offsetMin = new Vector2(5, 0);
                buttonRect.offsetMax = new Vector2(-5, 0);

                // Button text
                GameObject buttonTextObj = new GameObject("Text");
                buttonTextObj.transform.SetParent(buttonObj.transform, false);
                buttonText = buttonTextObj.AddComponent<Text>();
                buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                buttonText.fontSize = 14;
                buttonText.alignment = TextAnchor.MiddleCenter;
                buttonText.color = Color.white;
                RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
                buttonTextRect.anchorMin = Vector2.zero;
                buttonTextRect.anchorMax = Vector2.one;
                buttonTextRect.offsetMin = Vector2.zero;
                buttonTextRect.offsetMax = Vector2.zero;

                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }

            public void UpdateDisplay()
            {
                if (upgradeData == null) return;

                // Update name
                if (nameText != null)
                {
                    nameText.text = LiquorNameMapper.GetDisplayNameZH(upgradeData.liquorType.ToString().ToLower());
                }

                // Update level
                if (levelText != null)
                {
                    levelText.text = $"等級 Lv.{upgradeData.level}/{LiquorUpgradeData.MaxLevel}";
                }

                // Update button
                if (upgradeButton != null && buttonText != null)
                {
                    if (upgradeData.CanUpgrade())
                    {
                        int cost = upgradeData.GetUpgradeCost();
                        bool canAfford = PersistentGameData.Instance.GetTotalCoins() >= cost;
                        upgradeButton.interactable = canAfford;
                        buttonText.text = $"升級 Upgrade\n${cost}";
                        upgradeButton.GetComponent<Image>().color = canAfford ? 
                            new Color(0.3f, 0.7f, 0.3f, 1f) : 
                            new Color(0.5f, 0.5f, 0.5f, 1f);
                    }
                    else
                    {
                        upgradeButton.interactable = false;
                        buttonText.text = "已滿級 MAX";
                        upgradeButton.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);
                    }
                }
            }

            private void OnUpgradeClicked()
            {
                if (PersistentGameData.Instance != null)
                {
                    PersistentGameData.Instance.UpgradeLiquor(upgradeData.liquorType);
                }
            }

            private void OnDestroy()
            {
                if (upgradeButton != null)
                {
                    upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
                }
            }
        }

        /// <summary>
        /// UI item for decoration purchase
        /// </summary>
        public class DecorationItemUI : MonoBehaviour
        {
            private DecorationData decorationData;
            private Text nameText;
            private Text statusText;
            private Button purchaseButton;
            private Text buttonText;

            public void Initialize(DecorationData data)
            {
                decorationData = data;
                CreateUI();
                UpdateDisplay();
            }

            private void CreateUI()
            {
                // Background
                Image bg = gameObject.AddComponent<Image>();
                bg.color = new Color(0.3f, 0.2f, 0.2f, 0.8f);

                // Name text
                GameObject nameObj = new GameObject("NameText");
                nameObj.transform.SetParent(transform, false);
                nameText = nameObj.AddComponent<Text>();
                nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                nameText.fontSize = 18;
                nameText.alignment = TextAnchor.MiddleCenter;
                nameText.color = Color.white;
                RectTransform nameRect = nameObj.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0, 0.7f);
                nameRect.anchorMax = new Vector2(1, 1);
                nameRect.offsetMin = new Vector2(10, 0);
                nameRect.offsetMax = new Vector2(-10, -5);

                // Status text
                GameObject statusObj = new GameObject("StatusText");
                statusObj.transform.SetParent(transform, false);
                statusText = statusObj.AddComponent<Text>();
                statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                statusText.fontSize = 14;
                statusText.alignment = TextAnchor.MiddleCenter;
                statusText.color = Color.gray;
                RectTransform statusRect = statusObj.GetComponent<RectTransform>();
                statusRect.anchorMin = new Vector2(0, 0.5f);
                statusRect.anchorMax = new Vector2(1, 0.7f);
                statusRect.offsetMin = new Vector2(10, 0);
                statusRect.offsetMax = new Vector2(-10, 0);

                // Purchase button
                GameObject buttonObj = new GameObject("PurchaseButton");
                buttonObj.transform.SetParent(transform, false);
                purchaseButton = buttonObj.AddComponent<Button>();
                Image buttonImage = buttonObj.AddComponent<Image>();
                buttonImage.color = new Color(0.7f, 0.5f, 0.2f, 1f);
                RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.1f, 0.05f);
                buttonRect.anchorMax = new Vector2(0.9f, 0.45f);
                buttonRect.offsetMin = Vector2.zero;
                buttonRect.offsetMax = Vector2.zero;

                // Button text
                GameObject buttonTextObj = new GameObject("Text");
                buttonTextObj.transform.SetParent(buttonObj.transform, false);
                buttonText = buttonTextObj.AddComponent<Text>();
                buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                buttonText.fontSize = 14;
                buttonText.alignment = TextAnchor.MiddleCenter;
                buttonText.color = Color.white;
                RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
                buttonTextRect.anchorMin = Vector2.zero;
                buttonTextRect.anchorMax = Vector2.one;
                buttonTextRect.offsetMin = Vector2.zero;
                buttonTextRect.offsetMax = Vector2.zero;

                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }

            public void UpdateDisplay()
            {
                if (decorationData == null) return;

                // Update name
                if (nameText != null)
                {
                    string displayName = decorationData.decorationType switch
                    {
                        DecorationType.Speaker => "音箱 Speaker",
                        DecorationType.Plant => "盆栽 Plant",
                        DecorationType.Painting => "畫作 Painting",
                        _ => decorationData.decorationType.ToString()
                    };
                    nameText.text = displayName;
                }

                // Update status
                if (statusText != null)
                {
                    statusText.text = decorationData.isPurchased ? "已購買 Purchased" : "未購買 Not Owned";
                    statusText.color = decorationData.isPurchased ? Color.green : Color.gray;
                }

                // Update button
                if (purchaseButton != null && buttonText != null)
                {
                    if (decorationData.CanPurchase())
                    {
                        int cost = decorationData.GetPurchaseCost();
                        bool canAfford = PersistentGameData.Instance.GetTotalCoins() >= cost;
                        purchaseButton.interactable = canAfford;
                        buttonText.text = $"購買 Purchase\n${cost}";
                        purchaseButton.GetComponent<Image>().color = canAfford ? 
                            new Color(0.7f, 0.5f, 0.2f, 1f) : 
                            new Color(0.5f, 0.5f, 0.5f, 1f);
                    }
                    else
                    {
                        purchaseButton.interactable = false;
                        buttonText.text = "已擁有 Owned";
                        purchaseButton.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);
                    }
                }
            }

            private void OnPurchaseClicked()
            {
                if (PersistentGameData.Instance != null)
                {
                    PersistentGameData.Instance.PurchaseDecoration(decorationData.decorationType);
                }
            }

            private void OnDestroy()
            {
                if (purchaseButton != null)
                {
                    purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
                }
            }
        }

        #endregion
    }
}
