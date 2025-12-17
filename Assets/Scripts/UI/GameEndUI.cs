using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BarSimulator.Core;
using BarSimulator.Data;
using System.Collections.Generic;

namespace BarSimulator.UI
{
    /// <summary>
    /// Game end screen UI controller for the standalone GameEnd scene.
    /// Handles statistics display, shop functionality, and navigation.
    /// </summary>
    public class GameEndUI : MonoBehaviour
    {
        [Header("Center - Stats")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text totalCoinsText;
        [SerializeField] private Text drinksServedText;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button nextGameButton;

        [Header("Left - Liquor Upgrades")]
        [SerializeField] private Transform upgradesContainer;
        
        [Header("Right - Decorations")]
        [SerializeField] private Transform decorationsContainer;

        private Dictionary<BaseLiquorType, UpgradeUIItem> upgradeItems = new Dictionary<BaseLiquorType, UpgradeUIItem>();
        private Dictionary<DecorationType, DecorationUIItem> decorationItems = new Dictionary<DecorationType, DecorationUIItem>();

        #region Unity Lifecycle

        private void Start()
        {
            // Ensure cursor is visible and unlocked
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;

            // Setup button listeners
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (nextGameButton != null)
                nextGameButton.onClick.AddListener(OnNextGameClicked);

            // Subscribe to PersistentGameData events
            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.OnCoinsChanged += OnCoinsChanged;
                PersistentGameData.Instance.OnLiquorUpgraded += OnLiquorUpgraded;
                PersistentGameData.Instance.OnDecorationPurchased += OnDecorationPurchased;
            }

            // Initialize UI
            UpdateStatistics();
            CreateUpgradeItems();
            CreateDecorationItems();
        }

        private void OnDestroy()
        {
            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.OnCoinsChanged -= OnCoinsChanged;
                PersistentGameData.Instance.OnLiquorUpgraded -= OnLiquorUpgraded;
                PersistentGameData.Instance.OnDecorationPurchased -= OnDecorationPurchased;
            }

            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);

            if (nextGameButton != null)
                nextGameButton.onClick.RemoveListener(OnNextGameClicked);
        }

        #endregion

        #region Event Handlers

        private void OnCoinsChanged(int newTotal)
        {
            UpdateCoinsDisplay();
            UpdateUpgradeButtons();
            UpdateDecorationButtons();
        }

        private void OnLiquorUpgraded(BaseLiquorType type, int newLevel)
        {
            if (upgradeItems.ContainsKey(type))
            {
                upgradeItems[type].UpdateDisplay();
            }
            UpdateCoinsDisplay(); // Update coins as well
        }

        private void OnDecorationPurchased(DecorationType type)
        {
            if (decorationItems.ContainsKey(type))
            {
                decorationItems[type].UpdateDisplay();
            }
            UpdateCoinsDisplay(); // Update coins as well
        }

        #endregion

        #region Statistics Display

        private void UpdateStatistics()
        {
            if (titleText != null)
            {
                titleText.text = "Shift Complete";
            }

            UpdateCoinsDisplay();

            if (drinksServedText != null)
            {
                // If we came from the game, we might want to pass the score.
                // For now, we can read from GameManager if it persists, or just show total drinks from PersistentData if we tracked it there.
                // Since GameManager is destroyed on scene load (unless DontDestroyOnLoad), we might lose the immediate session stats.
                // However, GameManager is a singleton with DontDestroyOnLoad in the provided code.
                if (GameManager.Instance != null)
                {
                    var score = GameManager.Instance.GetScore();
                    drinksServedText.text = $"Drinks Served (Session): {score.totalDrinks}\nSatisfied: {score.satisfiedDrinks}";
                }
                else
                {
                    drinksServedText.text = "Session Ended";
                }
            }
        }

        private void UpdateCoinsDisplay()
        {
            if (totalCoinsText != null && PersistentGameData.Instance != null)
            {
                int coins = PersistentGameData.Instance.GetTotalCoins();
                totalCoinsText.text = $"Total Coins: ${coins}";
            }
        }

        #endregion

        #region Upgrade Items

        private void CreateUpgradeItems()
        {
            if (upgradesContainer == null)
            {
                Debug.LogError("Upgrades Container is null!");
                return;
            }

            // Clear existing items
            foreach (Transform child in upgradesContainer)
            {
                Destroy(child.gameObject);
            }
            upgradeItems.Clear();

            if (PersistentGameData.Instance == null)
            {
                Debug.LogError("PersistentGameData is null!");
                return;
            }

            var upgrades = PersistentGameData.Instance.GetAllLiquorUpgrades();
            Debug.Log($"Found {upgrades.Count} liquor upgrades to display.");

            foreach (var upgrade in upgrades)
            {
                GameObject itemObj = CreateUpgradeItemUI(upgrade);
                if (itemObj != null)
                {
                    itemObj.transform.SetParent(upgradesContainer, false);
                    var uiItem = itemObj.GetComponent<UpgradeUIItem>();
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
            
            var rectTransform = itemObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 80); // Width controlled by layout

            var uiItem = itemObj.AddComponent<UpgradeUIItem>();
            uiItem.Initialize(upgrade);

            return itemObj;
        }

        private void UpdateUpgradeButtons()
        {
            foreach (var item in upgradeItems.Values)
            {
                item.UpdateDisplay();
            }
        }

        #endregion

        #region Decoration Items

        private void CreateDecorationItems()
        {
            if (decorationsContainer == null)
            {
                Debug.LogError("Decorations Container is null!");
                return;
            }

            foreach (Transform child in decorationsContainer)
            {
                Destroy(child.gameObject);
            }
            decorationItems.Clear();

            if (PersistentGameData.Instance == null) return;

            var decorations = PersistentGameData.Instance.GetAllDecorations();
            Debug.Log($"Found {decorations.Count} decorations to display.");

            foreach (var decoration in decorations)
            {
                GameObject itemObj = CreateDecorationItemUI(decoration);
                if (itemObj != null)
                {
                    itemObj.transform.SetParent(decorationsContainer, false);
                    var uiItem = itemObj.GetComponent<DecorationUIItem>();
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
            
            var rectTransform = itemObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 100); // Width controlled by layout

            var uiItem = itemObj.AddComponent<DecorationUIItem>();
            uiItem.Initialize(decoration);

            return itemObj;
        }

        private void UpdateDecorationButtons()
        {
            foreach (var item in decorationItems.Values)
            {
                item.UpdateDisplay();
            }
        }

        #endregion

        #region Button Handlers

        private void OnMainMenuClicked()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void OnNextGameClicked()
        {
            // Reset session score in GameManager if needed, but StartGame() usually handles it.
            SceneManager.LoadScene("TheBar");
        }

        #endregion

        #region Helper Classes

        public class UpgradeUIItem : MonoBehaviour
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
                var bg = gameObject.AddComponent<Image>();
                bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

                // Name
                GameObject nameObj = new GameObject("NameText");
                nameObj.transform.SetParent(transform, false);
                nameText = nameObj.AddComponent<Text>();
                nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                nameText.fontSize = 18;
                nameText.alignment = TextAnchor.MiddleLeft;
                nameText.color = Color.white;
                var nameRect = nameObj.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0.05f, 0.5f);
                nameRect.anchorMax = new Vector2(0.6f, 1);
                nameRect.offsetMin = Vector2.zero;
                nameRect.offsetMax = Vector2.zero;

                // Level
                GameObject levelObj = new GameObject("LevelText");
                levelObj.transform.SetParent(transform, false);
                levelText = levelObj.AddComponent<Text>();
                levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                levelText.fontSize = 14;
                levelText.alignment = TextAnchor.MiddleLeft;
                levelText.color = Color.yellow;
                var levelRect = levelObj.GetComponent<RectTransform>();
                levelRect.anchorMin = new Vector2(0.05f, 0);
                levelRect.anchorMax = new Vector2(0.6f, 0.5f);
                levelRect.offsetMin = Vector2.zero;
                levelRect.offsetMax = Vector2.zero;

                // Button
                GameObject buttonObj = new GameObject("UpgradeButton");
                buttonObj.transform.SetParent(transform, false);
                upgradeButton = buttonObj.AddComponent<Button>();
                var buttonImage = buttonObj.AddComponent<Image>();
                buttonImage.color = new Color(0.3f, 0.6f, 0.3f, 1f);
                var buttonRect = buttonObj.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.65f, 0.1f);
                buttonRect.anchorMax = new Vector2(0.95f, 0.9f);
                buttonRect.offsetMin = Vector2.zero;
                buttonRect.offsetMax = Vector2.zero;

                // Button Text
                GameObject buttonTextObj = new GameObject("Text");
                buttonTextObj.transform.SetParent(buttonObj.transform, false);
                buttonText = buttonTextObj.AddComponent<Text>();
                buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                buttonText.fontSize = 14;
                buttonText.alignment = TextAnchor.MiddleCenter;
                buttonText.color = Color.white;
                var buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
                buttonTextRect.anchorMin = Vector2.zero;
                buttonTextRect.anchorMax = Vector2.one;
                buttonTextRect.offsetMin = Vector2.zero;
                buttonTextRect.offsetMax = Vector2.zero;

                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }

            public void UpdateDisplay()
            {
                if (upgradeData == null) return;

                if (nameText != null)
                {
                    string displayName = upgradeData.liquorType switch
                    {
                        BaseLiquorType.Vodka => "伏特加 (Vodka)",
                        BaseLiquorType.Gin => "琴酒 (Gin)",
                        BaseLiquorType.Rum => "蘭姆酒 (Rum)",
                        BaseLiquorType.Whiskey => "威士忌 (Whiskey)",
                        BaseLiquorType.Tequila => "龍舌蘭 (Tequila)",
                        _ => upgradeData.liquorType.ToString()
                    };
                    nameText.text = displayName;
                }

                if (levelText != null)
                {
                    levelText.text = $"Level {upgradeData.level} / {LiquorUpgradeData.MaxLevel}";
                }

                if (upgradeButton != null && buttonText != null)
                {
                    if (upgradeData.CanUpgrade())
                    {
                        int cost = upgradeData.GetUpgradeCost();
                        bool canAfford = PersistentGameData.Instance.GetTotalCoins() >= cost;
                        upgradeButton.interactable = canAfford;
                        buttonText.text = $"Upgrade\n${cost}";
                        upgradeButton.GetComponent<Image>().color = canAfford ? new Color(0.3f, 0.6f, 0.3f, 1f) : Color.gray;
                    }
                    else
                    {
                        upgradeButton.interactable = false;
                        buttonText.text = "MAX";
                        upgradeButton.GetComponent<Image>().color = Color.gray;
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
        }

        public class DecorationUIItem : MonoBehaviour
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
                var bg = gameObject.AddComponent<Image>();
                bg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

                // Name
                GameObject nameObj = new GameObject("NameText");
                nameObj.transform.SetParent(transform, false);
                nameText = nameObj.AddComponent<Text>();
                nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                nameText.fontSize = 18;
                nameText.alignment = TextAnchor.MiddleCenter;
                nameText.color = Color.white;
                var nameRect = nameObj.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0.05f, 0.6f);
                nameRect.anchorMax = new Vector2(0.95f, 0.95f);
                nameRect.offsetMin = Vector2.zero;
                nameRect.offsetMax = Vector2.zero;

                // Status
                GameObject statusObj = new GameObject("StatusText");
                statusObj.transform.SetParent(transform, false);
                statusText = statusObj.AddComponent<Text>();
                statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                statusText.fontSize = 14;
                statusText.alignment = TextAnchor.MiddleCenter;
                statusText.color = Color.gray;
                var statusRect = statusObj.GetComponent<RectTransform>();
                statusRect.anchorMin = new Vector2(0.05f, 0.35f);
                statusRect.anchorMax = new Vector2(0.95f, 0.55f);
                statusRect.offsetMin = Vector2.zero;
                statusRect.offsetMax = Vector2.zero;

                // Button
                GameObject buttonObj = new GameObject("PurchaseButton");
                buttonObj.transform.SetParent(transform, false);
                purchaseButton = buttonObj.AddComponent<Button>();
                var buttonImage = buttonObj.AddComponent<Image>();
                buttonImage.color = new Color(0.6f, 0.4f, 0.2f, 1f);
                var buttonRect = buttonObj.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.2f, 0.05f);
                buttonRect.anchorMax = new Vector2(0.8f, 0.3f);
                buttonRect.offsetMin = Vector2.zero;
                buttonRect.offsetMax = Vector2.zero;

                // Button Text
                GameObject buttonTextObj = new GameObject("Text");
                buttonTextObj.transform.SetParent(buttonObj.transform, false);
                buttonText = buttonTextObj.AddComponent<Text>();
                buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                buttonText.fontSize = 14;
                buttonText.alignment = TextAnchor.MiddleCenter;
                buttonText.color = Color.white;
                var buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
                buttonTextRect.anchorMin = Vector2.zero;
                buttonTextRect.anchorMax = Vector2.one;
                buttonTextRect.offsetMin = Vector2.zero;
                buttonTextRect.offsetMax = Vector2.zero;

                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }

            public void UpdateDisplay()
            {
                if (decorationData == null) return;

                if (nameText != null)
                {
                    string displayName = decorationData.decorationType switch
                    {
                        DecorationType.Speaker => "音箱 (Speakers)",
                        DecorationType.Plant => "盆栽 (Plant)",
                        DecorationType.Painting => "畫 (Painting)",
                        _ => decorationData.decorationType.ToString()
                    };
                    nameText.text = displayName;
                }

                if (statusText != null)
                {
                    statusText.text = decorationData.isPurchased ? "已擁有 (Owned)" : "未擁有 (Not Owned)";
                    statusText.color = decorationData.isPurchased ? Color.green : Color.gray;
                }

                if (purchaseButton != null && buttonText != null)
                {
                    if (decorationData.CanPurchase())
                    {
                        int cost = decorationData.GetPurchaseCost();
                        bool canAfford = PersistentGameData.Instance.GetTotalCoins() >= cost;
                        purchaseButton.interactable = canAfford;
                        buttonText.text = $"Buy ${cost}";
                        purchaseButton.GetComponent<Image>().color = canAfford ? new Color(0.6f, 0.4f, 0.2f, 1f) : Color.gray;
                    }
                    else
                    {
                        purchaseButton.interactable = false;
                        buttonText.text = "Purchased";
                        purchaseButton.GetComponent<Image>().color = Color.gray;
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
        }

        #endregion
    }
}
