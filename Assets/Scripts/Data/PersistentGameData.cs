using UnityEngine;
using System;
using System.Collections.Generic;

namespace BarSimulator.Data
{
    /// <summary>
    /// Base liquor types that can be upgraded (matching the 5 base spirits in LiquorData)
    /// </summary>
    public enum BaseLiquorType
    {
        Vodka,      // 伏特加
        Gin,        // 琴酒
        Rum,        // 蘭姆酒
        Whiskey,    // 威士忌
        Tequila     // 龍舌蘭
    }

    /// <summary>
    /// Decoration types that can be purchased
    /// </summary>
    public enum DecorationType
    {
        Speaker,    // 音箱
        Plant,      // 盆栽
        Painting    // 畫
    }

    /// <summary>
    /// Liquor upgrade data
    /// </summary>
    [Serializable]
    public class LiquorUpgradeData
    {
        public BaseLiquorType liquorType;
        public int level = 1; // 1-5
        public const int MaxLevel = 5;
        public const int UpgradeCost = 1000;

        public LiquorUpgradeData(BaseLiquorType type)
        {
            liquorType = type;
            level = 1;
        }

        public bool CanUpgrade()
        {
            return level < MaxLevel;
        }

        public void Upgrade()
        {
            if (CanUpgrade())
            {
                level++;
            }
        }

        public int GetUpgradeCost()
        {
            return CanUpgrade() ? UpgradeCost : 0;
        }
    }

    /// <summary>
    /// Decoration purchase data
    /// </summary>
    [Serializable]
    public class DecorationData
    {
        public DecorationType decorationType;
        public bool isPurchased = false;
        
        // Different costs for different decorations
        public int GetPurchaseCost()
        {
            switch (decorationType)
            {
                case DecorationType.Speaker:
                    return 250;
                case DecorationType.Plant:
                    return 200;
                case DecorationType.Painting:
                    return 150;
                default:
                    return 100;
            }
        }

        public DecorationData(DecorationType type)
        {
            decorationType = type;
            isPurchased = false;
        }

        public bool CanPurchase()
        {
            return !isPurchased;
        }

        public void Purchase()
        {
            isPurchased = true;
        }
    }

    /// <summary>
    /// Persistent game data that carries over between game sessions
    /// Stores: total coins, liquor levels, decoration purchases
    /// </summary>
    public class PersistentGameData : MonoBehaviour
    {
        #region Singleton

        private static PersistentGameData instance;

        public static PersistentGameData Instance
        {
            get
            {
                if (instance == null)
                {
                    // Try to find existing instance
                    instance = FindFirstObjectByType<PersistentGameData>();

                    // Create new if not found
                    if (instance == null)
                    {
                        GameObject go = new GameObject("PersistentGameData");
                        instance = go.AddComponent<PersistentGameData>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        #endregion

        #region Events

        public event Action<int> OnCoinsChanged;
        public event Action<BaseLiquorType, int> OnLiquorUpgraded;
        public event Action<DecorationType> OnDecorationPurchased;

        #endregion

        #region Data

        [Header("Persistent Data")]
        [SerializeField] private int totalCoins = 0;

        [Header("Liquor Upgrades")]
        [SerializeField] private List<LiquorUpgradeData> liquorUpgrades = new List<LiquorUpgradeData>();

        [Header("Decorations")]
        [SerializeField] private List<DecorationData> decorations = new List<DecorationData>();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton setup
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize data if empty
            InitializeData();
        }

        #endregion

        #region Initialization

        private void InitializeData()
        {
            // Initialize liquor upgrades if empty
            if (liquorUpgrades.Count == 0)
            {
                foreach (BaseLiquorType type in Enum.GetValues(typeof(BaseLiquorType)))
                {
                    liquorUpgrades.Add(new LiquorUpgradeData(type));
                }
            }

            // Initialize decorations if empty
            if (decorations.Count == 0)
            {
                foreach (DecorationType type in Enum.GetValues(typeof(DecorationType)))
                {
                    decorations.Add(new DecorationData(type));
                }
            }
        }

        #endregion

        #region Coins Management

        /// <summary>
        /// Add coins to the total
        /// </summary>
        public void AddCoins(int amount)
        {
            totalCoins += amount;
            OnCoinsChanged?.Invoke(totalCoins);
            Debug.Log($"PersistentGameData: Added {amount} coins. Total: {totalCoins}");
        }

        /// <summary>
        /// Spend coins (returns true if successful)
        /// </summary>
        public bool SpendCoins(int amount)
        {
            if (totalCoins >= amount)
            {
                totalCoins -= amount;
                OnCoinsChanged?.Invoke(totalCoins);
                Debug.Log($"PersistentGameData: Spent {amount} coins. Remaining: {totalCoins}");
                return true;
            }
            else
            {
                Debug.LogWarning($"PersistentGameData: Not enough coins! Need {amount}, have {totalCoins}");
                return false;
            }
        }

        /// <summary>
        /// Get current total coins
        /// </summary>
        public int GetTotalCoins()
        {
            return totalCoins;
        }

        #endregion

        #region Liquor Upgrades

        /// <summary>
        /// Get liquor upgrade data
        /// </summary>
        public LiquorUpgradeData GetLiquorUpgrade(BaseLiquorType type)
        {
            return liquorUpgrades.Find(x => x.liquorType == type);
        }

        /// <summary>
        /// Attempt to upgrade a liquor (returns true if successful)
        /// </summary>
        public bool UpgradeLiquor(BaseLiquorType type)
        {
            var upgrade = GetLiquorUpgrade(type);
            if (upgrade == null)
            {
                Debug.LogError($"PersistentGameData: Liquor upgrade not found for {type}");
                return false;
            }

            if (!upgrade.CanUpgrade())
            {
                Debug.LogWarning($"PersistentGameData: {type} is already at max level");
                return false;
            }

            int cost = upgrade.GetUpgradeCost();
            if (SpendCoins(cost))
            {
                upgrade.Upgrade();
                OnLiquorUpgraded?.Invoke(type, upgrade.level);
                Debug.Log($"PersistentGameData: Upgraded {type} to level {upgrade.level}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get all liquor upgrades
        /// </summary>
        public List<LiquorUpgradeData> GetAllLiquorUpgrades()
        {
            return new List<LiquorUpgradeData>(liquorUpgrades);
        }

        #endregion

        #region Decorations

        /// <summary>
        /// Get decoration data
        /// </summary>
        public DecorationData GetDecoration(DecorationType type)
        {
            return decorations.Find(x => x.decorationType == type);
        }

        /// <summary>
        /// Attempt to purchase a decoration (returns true if successful)
        /// </summary>
        public bool PurchaseDecoration(DecorationType type)
        {
            var decoration = GetDecoration(type);
            if (decoration == null)
            {
                Debug.LogError($"PersistentGameData: Decoration not found for {type}");
                return false;
            }

            if (!decoration.CanPurchase())
            {
                Debug.LogWarning($"PersistentGameData: {type} is already purchased");
                return false;
            }

            int cost = decoration.GetPurchaseCost();
            if (SpendCoins(cost))
            {
                decoration.Purchase();
                OnDecorationPurchased?.Invoke(type);
                Debug.Log($"PersistentGameData: Purchased {type}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a decoration is purchased
        /// </summary>
        public bool IsDecorationPurchased(DecorationType type)
        {
            var decoration = GetDecoration(type);
            return decoration != null && decoration.isPurchased;
        }

        /// <summary>
        /// Get all decorations
        /// </summary>
        public List<DecorationData> GetAllDecorations()
        {
            return new List<DecorationData>(decorations);
        }

        #endregion

        #region Reset

        /// <summary>
        /// Reset all persistent data (for testing or new game)
        /// </summary>
        public void ResetAllData()
        {
            totalCoins = 0;
            
            foreach (var upgrade in liquorUpgrades)
            {
                upgrade.level = 1;
            }

            foreach (var decoration in decorations)
            {
                decoration.isPurchased = false;
            }

            OnCoinsChanged?.Invoke(totalCoins);
            Debug.Log("PersistentGameData: All data reset");
        }

        #endregion
    }
}
