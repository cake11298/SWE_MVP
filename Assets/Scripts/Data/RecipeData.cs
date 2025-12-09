using UnityEngine;

namespace BarSimulator.Data
{
    /// <summary>
    /// 配方成分資料
    /// </summary>
    [System.Serializable]
    public class RecipeIngredient
    {
        [Tooltip("成分名稱")]
        public string name;

        [Tooltip("數量描述 (如: 60ml, 適量)")]
        public string amount;

        /// <summary>
        /// 預設建構子
        /// </summary>
        public RecipeIngredient() { }

        /// <summary>
        /// 完整建構子
        /// </summary>
        public RecipeIngredient(string amount, string name)
        {
            this.amount = amount;
            this.name = name;
        }
    }

    /// <summary>
    /// 雞尾酒配方資料
    /// 參考: CocktailSystem.js getCocktailRecipes() Line 1176-1443
    /// </summary>
    [System.Serializable]
    public class RecipeData
    {
        [Tooltip("雞尾酒名稱")]
        public string name;

        [Tooltip("配方成分")]
        public RecipeIngredient[] ingredients;

        [Tooltip("調製方法")]
        [TextArea(2, 5)]
        public string method;

        // === 商店解鎖系統新增欄位 ===

        /// <summary>是否已解鎖（用於稀有配方）</summary>
        public bool isLocked = false;

        /// <summary>解鎖價格（金幣）</summary>
        public int unlockPrice = 0;

        /// <summary>配方難度（1-5星）</summary>
        [Range(1, 5)]
        public int difficulty = 1;

        /// <summary>配方描述</summary>
        [TextArea(2, 3)]
        public string description = "";

        /// <summary>
        /// 預設建構子
        /// </summary>
        public RecipeData() { }

        /// <summary>
        /// 完整建構子
        /// </summary>
        public RecipeData(string name, RecipeIngredient[] ingredients, string method)
        {
            this.name = name;
            this.ingredients = ingredients;
            this.method = method;
        }
    }

    // NOTE: RecipeDatabaseSO (ScriptableObject) has been removed.
    // Use the new static RecipeDatabase class in RecipeDatabase.cs instead.
}
