using UnityEngine;
using BarSimulator.Data;

namespace BarSimulator.Systems
{
    /// <summary>
    /// 升級系統 - 處理酒類升級、配方解鎖和金錢管理
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        #region Singleton

        private static UpgradeSystem instance;
        public static UpgradeSystem Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("Database References")]
        [Tooltip("酒類資料庫")]
        [SerializeField] private LiquorDatabase liquorDatabase;

        // NOTE: recipeDatabase removed - use static RecipeDatabase class instead

        [Header("Starting Values")]
        [Tooltip("初始金錢數量")]
        [SerializeField] private int startingMoney = 1000;

        #endregion

        #region 私有欄位

        private int currentMoney;

        // 事件
        public System.Action<int> OnMoneyChanged;
        public System.Action<string, int> OnLiquorUpgraded; // liquorId, newLevel
        public System.Action<string> OnLiquorUnlocked; // liquorId
        public System.Action<string> OnRecipeUnlocked; // recipeName

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

            // Load databases if not assigned
            if (liquorDatabase == null)
            {
                liquorDatabase = Resources.Load<LiquorDatabase>("LiquorDatabase");
            }

            // NOTE: RecipeDatabase is now static - no initialization needed

            // Initialize money
            currentMoney = startingMoney;
        }

        #endregion

        #region 金錢管理

        /// <summary>
        /// 增加金錢
        /// </summary>
        public void AddMoney(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"UpgradeSystem: Cannot add negative money: {amount}");
                return;
            }

            currentMoney += amount;
            OnMoneyChanged?.Invoke(currentMoney);
            Debug.Log($"UpgradeSystem: Added ${amount}, Total: ${currentMoney}");
        }

        /// <summary>
        /// 扣除金錢
        /// </summary>
        private bool DeductMoney(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"UpgradeSystem: Cannot deduct negative money: {amount}");
                return false;
            }

            if (currentMoney < amount)
            {
                Debug.LogWarning($"UpgradeSystem: Insufficient funds. Need ${amount}, Have ${currentMoney}");
                return false;
            }

            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            Debug.Log($"UpgradeSystem: Deducted ${amount}, Remaining: ${currentMoney}");
            return true;
        }

        /// <summary>
        /// 設置金錢（用於存檔載入）
        /// </summary>
        public void SetMoney(int amount)
        {
            currentMoney = Mathf.Max(0, amount);
            OnMoneyChanged?.Invoke(currentMoney);
        }

        /// <summary>
        /// 檢查是否有足夠金錢
        /// </summary>
        public bool CanAfford(int amount)
        {
            return currentMoney >= amount;
        }

        #endregion

        #region 酒類升級

        /// <summary>
        /// 升級酒類等級
        /// </summary>
        /// <param name="liquorId">酒類ID</param>
        /// <returns>升級是否成功</returns>
        public bool UpgradeLiquor(string liquorId)
        {
            if (liquorDatabase == null)
            {
                Debug.LogError("UpgradeSystem: LiquorDatabase not found!");
                return false;
            }

            var liquor = liquorDatabase.GetLiquor(liquorId);
            if (liquor == null)
            {
                Debug.LogError($"UpgradeSystem: Liquor '{liquorId}' not found!");
                return false;
            }

            // Check if locked
            if (liquor.isLocked)
            {
                Debug.LogWarning($"UpgradeSystem: Cannot upgrade locked liquor '{liquor.displayName}'");
                return false;
            }

            // Check if already max level
            if (liquor.level >= 3)
            {
                Debug.LogWarning($"UpgradeSystem: '{liquor.displayName}' is already max level");
                return false;
            }

            // Get upgrade price
            int price = liquorDatabase.GetUpgradePrice(liquorId);
            if (price < 0)
            {
                Debug.LogError($"UpgradeSystem: Failed to get upgrade price for '{liquorId}'");
                return false;
            }

            // Check affordability
            if (!CanAfford(price))
            {
                Debug.LogWarning($"UpgradeSystem: Cannot afford upgrade. Need ${price}, Have ${currentMoney}");
                return false;
            }

            // Perform upgrade
            if (liquorDatabase.UpgradeLiquor(liquorId))
            {
                DeductMoney(price);
                OnLiquorUpgraded?.Invoke(liquorId, liquor.level);
                Debug.Log($"UpgradeSystem: Successfully upgraded '{liquor.displayName}' to Level {liquor.level}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 解鎖酒類
        /// </summary>
        /// <param name="liquorId">酒類ID</param>
        /// <returns>解鎖是否成功</returns>
        public bool UnlockLiquor(string liquorId)
        {
            if (liquorDatabase == null)
            {
                Debug.LogError("UpgradeSystem: LiquorDatabase not found!");
                return false;
            }

            var liquor = liquorDatabase.GetLiquor(liquorId);
            if (liquor == null)
            {
                Debug.LogError($"UpgradeSystem: Liquor '{liquorId}' not found!");
                return false;
            }

            // Check if already unlocked
            if (!liquor.isLocked)
            {
                Debug.LogWarning($"UpgradeSystem: '{liquor.displayName}' is already unlocked");
                return false;
            }

            // Check affordability
            if (!CanAfford(liquor.unlockPrice))
            {
                Debug.LogWarning($"UpgradeSystem: Cannot afford unlock. Need ${liquor.unlockPrice}, Have ${currentMoney}");
                return false;
            }

            // Perform unlock
            if (liquorDatabase.UnlockLiquor(liquorId))
            {
                DeductMoney(liquor.unlockPrice);
                OnLiquorUnlocked?.Invoke(liquorId);
                Debug.Log($"UpgradeSystem: Successfully unlocked '{liquor.displayName}'");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 取得酒類升級價格
        /// </summary>
        public int GetLiquorUpgradePrice(string liquorId)
        {
            if (liquorDatabase == null) return -1;
            return liquorDatabase.GetUpgradePrice(liquorId);
        }

        #endregion

        #region 配方解鎖

        /// <summary>
        /// 解鎖配方
        /// </summary>
        /// <param name="recipeName">配方名稱</param>
        /// <returns>解鎖是否成功</returns>
        public bool UnlockRecipe(string recipeName)
        {
            var recipe = RecipeDatabase.GetRecipe(recipeName);
            if (recipe == null)
            {
                Debug.LogError($"UpgradeSystem: Recipe '{recipeName}' not found!");
                return false;
            }

            // Check if already unlocked
            if (!recipe.isLocked)
            {
                Debug.LogWarning($"UpgradeSystem: '{recipe.name}' is already unlocked");
                return false;
            }

            // Check affordability
            if (!CanAfford(recipe.unlockPrice))
            {
                Debug.LogWarning($"UpgradeSystem: Cannot afford recipe unlock. Need ${recipe.unlockPrice}, Have ${currentMoney}");
                return false;
            }

            // Perform unlock
            recipe.isLocked = false;
            DeductMoney(recipe.unlockPrice);
            OnRecipeUnlocked?.Invoke(recipeName);
            Debug.Log($"UpgradeSystem: Successfully unlocked recipe '{recipe.name}'");
            return true;
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 當前金錢數量
        /// </summary>
        public int CurrentMoney => currentMoney;

        /// <summary>
        /// 酒類資料庫
        /// </summary>
        public LiquorDatabase LiquorDatabase => liquorDatabase;

        // NOTE: RecipeDatabase property removed - use static BarSimulator.Data.RecipeDatabase instead

        #endregion
    }
}
