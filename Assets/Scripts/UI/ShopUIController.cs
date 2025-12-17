using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BarSimulator.Data;
using BarSimulator.Core;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace BarSimulator.UI
{
    public class ShopUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private Transform liquorListContainer;
        [SerializeField] private Transform decorationListContainer;
        [SerializeField] private Button nextGameButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject shopItemPrefab; // Generic prefab for both

        private void Start()
        {
            // Initialize UI
            UpdateCoinsUI(PersistentGameData.Instance.GetTotalCoins());
            
            // Subscribe to events
            PersistentGameData.Instance.OnCoinsChanged += UpdateCoinsUI;
            
            // Generate Shop Items
            GenerateLiquorItems();
            GenerateDecorationItems();

            // Bind Buttons
            nextGameButton.onClick.AddListener(OnNextGameClicked);
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnDestroy()
        {
            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.OnCoinsChanged -= UpdateCoinsUI;
            }
        }

        private void UpdateCoinsUI(int coins)
        {
            if (coinsText != null)
                coinsText.text = $"Coins: {coins}";
        }

        private void GenerateLiquorItems()
        {
            foreach (Transform child in liquorListContainer) Destroy(child.gameObject);

            var upgrades = PersistentGameData.Instance.GetAllLiquorUpgrades();
            foreach (var upgrade in upgrades)
            {
                GameObject itemObj = Instantiate(shopItemPrefab, liquorListContainer);
                SetupLiquorItem(itemObj, upgrade);
            }
        }

        private void SetupLiquorItem(GameObject itemObj, LiquorUpgradeData upgrade)
        {
            // Assuming Prefab has: NameText, LevelText, CostText, ActionButton
            var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
            var button = itemObj.GetComponentInChildren<Button>();

            // Simple finding by order or name (adjust based on actual prefab structure)
            // Let's assume: 0: Name, 1: Info/Level, 2: Cost
            if (texts.Length > 0) texts[0].text = upgrade.liquorType.ToString();
            
            UpdateLiquorItemVisuals(itemObj, upgrade);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                if (PersistentGameData.Instance.UpgradeLiquor(upgrade.liquorType))
                {
                    UpdateLiquorItemVisuals(itemObj, upgrade);
                }
            });
        }

        private void UpdateLiquorItemVisuals(GameObject itemObj, LiquorUpgradeData upgrade)
        {
            var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
            var button = itemObj.GetComponentInChildren<Button>();

            if (texts.Length > 1) texts[1].text = $"Lvl {upgrade.level} -> {(upgrade.CanUpgrade() ? (upgrade.level + 1).ToString() : "MAX")}";
            
            if (upgrade.CanUpgrade())
            {
                int cost = upgrade.GetUpgradeCost();
                if (texts.Length > 2) texts[2].text = $"${cost}";
                button.interactable = PersistentGameData.Instance.GetTotalCoins() >= cost;
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Upgrade";
            }
            else
            {
                if (texts.Length > 2) texts[2].text = "---";
                button.interactable = false;
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Maxed";
            }
        }

        private void GenerateDecorationItems()
        {
            foreach (Transform child in decorationListContainer) Destroy(child.gameObject);

            var decorations = PersistentGameData.Instance.GetAllDecorations();
            foreach (var deco in decorations)
            {
                GameObject itemObj = Instantiate(shopItemPrefab, decorationListContainer);
                SetupDecorationItem(itemObj, deco);
            }
        }

        private void SetupDecorationItem(GameObject itemObj, DecorationData deco)
        {
            var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
            var button = itemObj.GetComponentInChildren<Button>();

            if (texts.Length > 0) texts[0].text = deco.decorationType.ToString();
            
            UpdateDecorationItemVisuals(itemObj, deco);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                if (PersistentGameData.Instance.PurchaseDecoration(deco.decorationType))
                {
                    UpdateDecorationItemVisuals(itemObj, deco);
                }
            });
        }

        private void UpdateDecorationItemVisuals(GameObject itemObj, DecorationData deco)
        {
            var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
            var button = itemObj.GetComponentInChildren<Button>();

            if (texts.Length > 1) texts[1].text = deco.isPurchased ? "Owned" : "Not Owned";
            
            if (!deco.isPurchased)
            {
                int cost = deco.GetPurchaseCost();
                if (texts.Length > 2) texts[2].text = $"${cost}";
                button.interactable = PersistentGameData.Instance.GetTotalCoins() >= cost;
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Buy";
            }
            else
            {
                if (texts.Length > 2) texts[2].text = "---";
                button.interactable = false;
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Owned";
            }
        }

        private void OnNextGameClicked()
        {
            // Reset Game State if needed (GameManager handles this on Start)
            SceneManager.LoadScene("TheBar");
        }

        private void OnMainMenuClicked()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
