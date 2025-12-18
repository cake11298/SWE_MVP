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
        [SerializeField] private TextMeshProUGUI failMessageText;
        [SerializeField] private Transform liquorListContainer;
        [SerializeField] private Transform decorationListContainer;
        [SerializeField] private Button nextGameButton;
        [SerializeField] private Button mainMenuButton;

        private bool _initialWinConditionMet;

        private void Start()
        {
            if (PersistentGameData.Instance == null)
            {
                Debug.LogError("ShopUIController: PersistentGameData.Instance is null!");
                return;
            }

            _initialWinConditionMet = PersistentGameData.Instance.GetTotalCoins() >= 300;

            // Initialize UI
            UpdateCoinsUI(PersistentGameData.Instance.GetTotalCoins());
            
            // Subscribe to events
            PersistentGameData.Instance.OnCoinsChanged += UpdateCoinsUI;
            
            // Refresh existing items
            RefreshShopItems();

            // Bind Buttons
            if (nextGameButton != null) nextGameButton.onClick.AddListener(OnNextGameClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
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

            CheckWinCondition(coins);
        }

        private void CheckWinCondition(int coins)
        {
            bool canProceed = _initialWinConditionMet;
            if (nextGameButton != null) nextGameButton.interactable = canProceed;
            if (failMessageText != null) failMessageText.gameObject.SetActive(!canProceed);
        }

        private void RefreshShopItems()
        {
            var views = GetComponentsInChildren<ShopItemView>(true);
            foreach (var view in views)
            {
                if (view.isLiquor)
                {
                    var data = PersistentGameData.Instance.GetLiquorUpgrade(view.liquorType);
                    if (data != null)
                    {
                        UpdateLiquorItemVisuals(view, data);
                        view.actionButton.onClick.RemoveAllListeners();
                        view.actionButton.onClick.AddListener(() => {
                            if (PersistentGameData.Instance.UpgradeLiquor(view.liquorType))
                            {
                                UpdateLiquorItemVisuals(view, data);
                            }
                        });
                    }
                }
                else
                {
                    var data = PersistentGameData.Instance.GetDecoration(view.decorationType);
                    if (data != null)
                    {
                        UpdateDecorationItemVisuals(view, data);
                        view.actionButton.onClick.RemoveAllListeners();
                        view.actionButton.onClick.AddListener(() => {
                            if (PersistentGameData.Instance.PurchaseDecoration(view.decorationType))
                            {
                                UpdateDecorationItemVisuals(view, data);
                            }
                        });
                    }
                }
            }
        }

        private void UpdateLiquorItemVisuals(ShopItemView view, LiquorUpgradeData upgrade)
        {
            if (view.nameText != null) view.nameText.text = upgrade.liquorType.ToString();
            if (view.infoText != null) view.infoText.text = $"Lvl {upgrade.level} -> {(upgrade.CanUpgrade() ? (upgrade.level + 1).ToString() : "MAX")}";
            
            if (upgrade.CanUpgrade())
            {
                int cost = upgrade.GetUpgradeCost();
                view.actionButton.interactable = PersistentGameData.Instance.GetTotalCoins() >= cost;
                if (view.buttonText != null) view.buttonText.text = $"${cost}";
            }
            else
            {
                view.actionButton.interactable = false;
                if (view.buttonText != null) view.buttonText.text = "Maxed";
            }
        }

        private void UpdateDecorationItemVisuals(ShopItemView view, DecorationData deco)
        {
            if (view.nameText != null) view.nameText.text = deco.decorationType.ToString();
            if (view.infoText != null) view.infoText.text = deco.isPurchased ? "Owned" : "Not Owned";
            
            if (!deco.isPurchased)
            {
                int cost = deco.GetPurchaseCost();
                view.actionButton.interactable = PersistentGameData.Instance.GetTotalCoins() >= cost;
                if (view.buttonText != null) view.buttonText.text = $"${cost}";
            }
            else
            {
                view.actionButton.interactable = false;
                if (view.buttonText != null) view.buttonText.text = "Owned";
            }
        }

        private void OnNextGameClicked()
        {
            SceneManager.LoadScene("TheBar");
        }

        private void OnMainMenuClicked()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
