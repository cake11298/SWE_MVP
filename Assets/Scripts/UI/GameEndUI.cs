using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BarSimulator.Core;
using BarSimulator.Data;
using System.Collections.Generic;

namespace BarSimulator.UI
{
    /// <summary>
    /// Game end screen UI - shows final statistics, liquor upgrades, and decoration purchases
    /// Layout: Left (Upgrades) | Center (Stats) | Right (Decorations)
    /// </summary>
    public class GameEndUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject gameEndPanel;

        [Header("Center - Stats")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text totalCoinsText;
        [SerializeField] private Text drinksServedText;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button nextGameButton;

        [Header("Left - Liquor Upgrades")]
        [SerializeField] private Transform upgradesContainer;
        [SerializeField] private GameObject upgradeItemPrefab; // Will be created dynamically if null

        [Header("Right - Decorations")]
        [SerializeField] private Transform decorationsContainer;
        [SerializeField] private GameObject decorationItemPrefab; // Will be created dynamically if null

        private bool isShowing = false;
        private Dictionary<BaseLiquorType, UpgradeUIItem> upgradeItems = new Dictionary<BaseLiquorType, UpgradeUIItem>();
        private Dictionary<DecorationType, DecorationUIItem> decorationItems = new Dictionary<DecorationType, DecorationUIItem>();

        #region Unity Lifecycle

        private void Start()
        {
            // Hide panel initially
            if (gameEndPanel != null)
            {
                gameEndPanel.SetActive(false);
            }

            // Setup button listeners
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            if (nextGameButton != null)
            {
                nextGameButton.onClick.AddListener(OnNextGameClicked);
            }

            // Subscribe to GameManager events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }

            // Subscribe to PersistentGameData events
            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.OnCoinsChanged += OnCoinsChanged;
                PersistentGameData.Instance.OnLiquorUpgraded += OnLiquorUpgraded;
                PersistentGameData.Instance.OnDecorationPurchased += OnDecorationPurchased;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }

            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.OnCoinsChanged -= OnCoinsChanged;
                PersistentGameData.Instance.OnLiquorUpgraded -= OnLiquorUpgraded;
                PersistentGameData.Instance.OnDecorationPurchased -= OnDecorationPurchased;
            }

            // Remove button listeners
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            }

            if (nextGameButton != null)
            {
                nextGameButton.onClick.RemoveListener(OnNextGameClicked);
            }
        }

        #endregion

        #region Event Handlers

        private void OnGameStateChanged(GameState newState)
        {
            Debug.Log($"GameEndUI: Game state changed to {newState}");
            
            if (newState == GameState.GameOver && !isShowing)
            {
                Debug.Log("GameEndUI: Showing game end screen");
                ShowGameEndScreen();
            }
        }

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
        }

        private void OnDecorationPurchased(DecorationType type)
        {
            if (decorationItems.ContainsKey(type))
            {
                decorationItems[type].UpdateDisplay();
            }
        }

        #endregion

        #region Show/Hide

        public void ShowGameEndScreen()
        {
            if (isShowing)
            {
                Debug.Log("GameEndUI: Already showing");
                return;
            }

            Debug.Log("GameEndUI: ShowGameEndScreen called");
            isShowing = true;

            // Show panel
            if (gameEndPanel != null)
            {
                gameEndPanel.SetActive(true);
                Debug.Log("GameEndUI: Panel activated");
            }
            else
            {
                Debug.LogError("GameEndUI: gameEndPanel is null!");
            }

            // COMPLETELY PAUSE GAME - freeze all time-based operations
            Time.timeScale = 0f;

            // Unlock cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Disable player input
            if (GameManager.Instance != null && GameManager.Instance.PlayerController != null)
            {
                GameManager.Instance.PlayerController.DisableInput();
            }

            // Update displays
            UpdateStatistics();
            CreateUpgradeItems();
            CreateDecorationItems();
        }

        public void HideGameEndScreen()
        {
            if (!isShowing) return;

            isShowing = false;

            if (gameEndPanel != null)
            {
                gameEndPanel.SetActive(false);
            }

            // Resume time
            Time.timeScale = 1f;
        }

        #endregion

        #region Statistics Display

        private void UpdateStatistics()
        {
            if (titleText != null)
            {
                titleText.text = "Bar Closed - Time's Up!";
            }

            UpdateCoinsDisplay();

            if (drinksServedText != null && GameManager.Instance != null)
            {
                var score = GameManager.Instance.GetScore();
                drinksServedText.text = $"Drinks Served: {score.totalDrinks}";
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
            if (upgradesContainer == null) return;

            // Clear existing items
            foreach (Transform child in upgradesContainer)
            {
                Destroy(child.gameObject);
            }
            upgradeItems.Clear();

            // Create upgrade items for each base liquor
            var upgrades = PersistentGameData.Instance.GetAllLiquorUpgrades();
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
            // Create a simple UI item
            GameObject itemObj = new GameObject($"Upgrade_{upgrade.liquorType}");
            
            // Add RectTransform
            var rectTransform = itemObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 60);

            // Add UpgradeUIItem component
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
            if (decorationsContainer == null) return;

            // Clear existing items
            foreach (Transform child in decorationsContainer)
            {
                Destroy(child.gameObject);
            }
            decorationItems.Clear();

            // Create decoration items
            var decorations = PersistentGameData.Instance.GetAllDecorations();
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
            // Create a simple UI item
            GameObject itemObj = new GameObject($"Decoration_{decoration.decorationType}");
            
            // Add RectTransform
            var rectTransform = itemObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 80);

            // Add DecorationUIItem component
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
            Debug.Log("GameEndUI: Returning to main menu");
            
            // Resume time before scene transition
            Time.timeScale = 1f;
            
            // Re-enable player input
            if (GameManager.Instance != null && GameManager.Instance.PlayerController != null)
            {
                GameManager.Instance.PlayerController.EnableInput();
            }
            
            // Load main menu scene
            SceneManager.LoadScene("MainMenu");
        }

        private void OnNextGameClicked()
        {
            Debug.Log("GameEndUI: Starting next game - inheriting coins and upgrades");
            
            // Resume time before scene transition
            Time.timeScale = 1f;

            // Re-enable player input
            if (GameManager.Instance != null && GameManager.Instance.PlayerController != null)
            {
                GameManager.Instance.PlayerController.EnableInput();
            }

            // Hide this panel
            HideGameEndScreen();
            
            // Reload the current scene (TheBar) - coins and upgrades persist via PersistentGameData
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// UI item for liquor upgrade
        /// </summary>
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
                // Create background
                var bg = gameObject.AddComponent<Image>();
                bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

                // Create name text
                GameObject nameObj = new GameObject("NameText");
                nameObj.transform.SetParent(transform, false);
                nameText = nameObj.AddComponent<Text>();
                nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                nameText.fontSize = 16;
                nameText.alignment = TextAnchor.MiddleLeft;
                nameText.color = Color.white;
                var nameRect = nameObj.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0, 0.5f);
                nameRect.anchorMax = new Vector2(0.5f, 1);
                nameRect.offsetMin = new Vector2(10, 0);
                nameRect.offsetMax = new Vector2(-5, -5);

                // Create level text
                GameObject levelObj = new GameObject("LevelText");
                levelObj.transform.SetParent(transform, false);
                levelText = levelObj.AddComponent<Text>();
                levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                levelText.fontSize = 14;
                levelText.alignment = TextAnchor.MiddleLeft;
                levelText.color = Color.yellow;
                var levelRect = levelObj.GetComponent<RectTransform>();
                levelRect.anchorMin = new Vector2(0, 0);
                levelRect.anchorMax = new Vector2(0.5f, 0.5f);
                levelRect.offsetMin = new Vector2(10, 5);
                levelRect.offsetMax = new Vector2(-5, 0);

                // Create upgrade button
                GameObject buttonObj = new GameObject("UpgradeButton");
                buttonObj.transform.SetParent(transform, false);
                upgradeButton = buttonObj.AddComponent<Button>();
                var buttonImage = buttonObj.AddComponent<Image>();
                buttonImage.color = new Color(0.3f, 0.6f, 0.3f, 1f);
                var buttonRect = buttonObj.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.5f, 0);
                buttonRect.anchorMax = new Vector2(1, 1);
                buttonRect.offsetMin = new Vector2(5, 5);
                buttonRect.offsetMax = new Vector2(-10, -5);

                // Create button text
                GameObject buttonTextObj = new GameObject("Text");
                buttonTextObj.transform.SetParent(buttonObj.transform, false);
                buttonText = buttonTextObj.AddComponent<Text>();
                buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                buttonText.fontSize = 12;
                buttonText.alignment = TextAnchor.MiddleCenter;
                buttonText.color = Color.white;
                var buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
                buttonTextRect.anchorMin = Vector2.zero;
                buttonTextRect.anchorMax = Vector2.one;
                buttonTextRect.offsetMin = Vector2.zero;
                buttonTextRect.offsetMax = Vector2.zero;

                // Add button listener
                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }

            public void UpdateDisplay()
            {
                if (upgradeData == null) return;

                // Update name
                if (nameText != null)
                {
                    nameText.text = upgradeData.liquorType.ToString();
                }

                // Update level
                if (levelText != null)
                {
                    levelText.text = $"Lv.{upgradeData.level}/{LiquorUpgradeData.MaxLevel}";
                }

                // Update button
                if (upgradeButton != null && buttonText != null)
                {
                    if (upgradeData.CanUpgrade())
                    {
                        upgradeButton.interactable = PersistentGameData.Instance.GetTotalCoins() >= upgradeData.GetUpgradeCost();
                        buttonText.text = $"Upgrade\n${upgradeData.GetUpgradeCost()}";
                    }
                    else
                    {
                        upgradeButton.interactable = false;
                        buttonText.text = "MAX";
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
                // Create background
                var bg = gameObject.AddComponent<Image>();
                bg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

                // Create name text
                GameObject nameObj = new GameObject("NameText");
                nameObj.transform.SetParent(transform, false);
                nameText = nameObj.AddComponent<Text>();
                nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                nameText.fontSize = 16;
                nameText.alignment = TextAnchor.MiddleCenter;
                nameText.color = Color.white;
                var nameRect = nameObj.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0, 0.6f);
                nameRect.anchorMax = new Vector2(1, 1);
                nameRect.offsetMin = new Vector2(10, 0);
                nameRect.offsetMax = new Vector2(-10, -5);

                // Create status text
                GameObject statusObj = new GameObject("StatusText");
                statusObj.transform.SetParent(transform, false);
                statusText = statusObj.AddComponent<Text>();
                statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                statusText.fontSize = 12;
                statusText.alignment = TextAnchor.MiddleCenter;
                statusText.color = Color.gray;
                var statusRect = statusObj.GetComponent<RectTransform>();
                statusRect.anchorMin = new Vector2(0, 0.4f);
                statusRect.anchorMax = new Vector2(1, 0.6f);
                statusRect.offsetMin = new Vector2(10, 0);
                statusRect.offsetMax = new Vector2(-10, 0);

                // Create purchase button
                GameObject buttonObj = new GameObject("PurchaseButton");
                buttonObj.transform.SetParent(transform, false);
                purchaseButton = buttonObj.AddComponent<Button>();
                var buttonImage = buttonObj.AddComponent<Image>();
                buttonImage.color = new Color(0.6f, 0.4f, 0.2f, 1f);
                var buttonRect = buttonObj.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.1f, 0.05f);
                buttonRect.anchorMax = new Vector2(0.9f, 0.35f);
                buttonRect.offsetMin = Vector2.zero;
                buttonRect.offsetMax = Vector2.zero;

                // Create button text
                GameObject buttonTextObj = new GameObject("Text");
                buttonTextObj.transform.SetParent(buttonObj.transform, false);
                buttonText = buttonTextObj.AddComponent<Text>();
                buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                buttonText.fontSize = 12;
                buttonText.alignment = TextAnchor.MiddleCenter;
                buttonText.color = Color.white;
                var buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
                buttonTextRect.anchorMin = Vector2.zero;
                buttonTextRect.anchorMax = Vector2.one;
                buttonTextRect.offsetMin = Vector2.zero;
                buttonTextRect.offsetMax = Vector2.zero;

                // Add button listener
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }

            public void UpdateDisplay()
            {
                if (decorationData == null) return;

                // Update name with Chinese translation
                if (nameText != null)
                {
                    string displayName = decorationData.decorationType switch
                    {
                        DecorationType.Speaker => "音箱",
                        DecorationType.Plant => "盆栽",
                        DecorationType.Painting => "畫",
                        _ => decorationData.decorationType.ToString()
                    };
                    nameText.text = displayName;
                }

                // Update status
                if (statusText != null)
                {
                    statusText.text = decorationData.isPurchased ? "已購買" : "未購買";
                    statusText.color = decorationData.isPurchased ? Color.green : Color.gray;
                }

                // Update button
                if (purchaseButton != null && buttonText != null)
                {
                    if (decorationData.CanPurchase())
                    {
                        purchaseButton.interactable = PersistentGameData.Instance.GetTotalCoins() >= decorationData.GetPurchaseCost();
                        buttonText.text = $"Purchase\n${decorationData.GetPurchaseCost()}";
                    }
                    else
                    {
                        purchaseButton.interactable = false;
                        buttonText.text = "Purchased";
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
