using UnityEngine;

namespace BarSimulator.Data
{
    /// <summary>
    /// 酒類類別枚舉
    /// </summary>
    public enum LiquorCategory
    {
        /// <summary>六大基酒</summary>
        BaseSpirit,
        /// <summary>調味料</summary>
        Mixer,
        /// <summary>果汁</summary>
        Juice,
        /// <summary>利口酒</summary>
        Liqueur,
        /// <summary>加烈酒</summary>
        FortifiedWine
    }

    /// <summary>
    /// 酒類資料結構
    /// 參考: CocktailSystem.js initLiquorDatabase() Line 55-264
    /// </summary>
    [System.Serializable]
    public class LiquorData
    {
        /// <summary>酒類識別碼 (如: vodka, gin)</summary>
        public string id;

        /// <summary>中文名稱</summary>
        public string nameZH;

        /// <summary>英文顯示名稱</summary>
        public string displayName;

        /// <summary>酒液顏色</summary>
        public Color color;

        /// <summary>酒精含量 (%)</summary>
        public float alcoholContent;

        /// <summary>酒類類別</summary>
        public LiquorCategory category;

        // === 商店升級系統新增欄位 ===

        /// <summary>當前等級 (1-3)，Level 1 = 普通，Level 2 = 中級，Level 3 = 高級</summary>
        [Range(1, 3)]
        public int level = 1;

        /// <summary>是否已解鎖（用於新酒類或稀有酒類）</summary>
        public bool isLocked = false;

        /// <summary>解鎖價格（金幣）</summary>
        public int unlockPrice = 0;

        /// <summary>升級價格陣列 [Lv1→Lv2的價格, Lv2→Lv3的價格]</summary>
        public int[] upgradePrices = new int[] { 500, 1000 };

        /// <summary>等級描述</summary>
        public string[] levelDescriptions = new string[]
        {
            "普通品質", // Level 1
            "中級品質 - 提升評分上限", // Level 2
            "高級品質 - 最高評分上限和精準度加成" // Level 3
        };

        /// <summary>
        /// 預設建構子
        /// </summary>
        public LiquorData() { }

        /// <summary>
        /// 完整建構子
        /// </summary>
        public LiquorData(string id, string nameZH, string displayName, Color color, float alcoholContent, LiquorCategory category)
        {
            this.id = id;
            this.nameZH = nameZH;
            this.displayName = displayName;
            this.color = color;
            this.alcoholContent = alcoholContent;
            this.category = category;
        }

        /// <summary>
        /// 從 Hex 顏色值建立 LiquorData
        /// </summary>
        public static LiquorData Create(string id, string nameZH, string displayName, int hexColor, float alcoholContent, LiquorCategory category)
        {
            return new LiquorData(
                id,
                nameZH,
                displayName,
                HexToColor(hexColor),
                alcoholContent,
                category
            );
        }

        /// <summary>
        /// 將 Hex 整數轉換為 Unity Color
        /// </summary>
        private static Color HexToColor(int hex)
        {
            float r = ((hex >> 16) & 0xFF) / 255f;
            float g = ((hex >> 8) & 0xFF) / 255f;
            float b = (hex & 0xFF) / 255f;
            return new Color(r, g, b);
        }
    }

    /// <summary>
    /// 酒類資料庫 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "LiquorDatabase", menuName = "Bar/Liquor Database")]
    public class LiquorDatabase : ScriptableObject
    {
        [Tooltip("所有酒類資料")]
        public LiquorData[] liquors;

        /// <summary>
        /// 根據 ID 取得酒類資料
        /// </summary>
        public LiquorData GetLiquor(string id)
        {
            if (liquors == null) return null;

            foreach (var liquor in liquors)
            {
                if (liquor.id == id)
                    return liquor;
            }
            return null;
        }

        /// <summary>
        /// 取得所有已解鎖的酒類
        /// </summary>
        public LiquorData[] GetUnlockedLiquors()
        {
            if (liquors == null) return new LiquorData[0];

            var unlocked = new System.Collections.Generic.List<LiquorData>();
            foreach (var liquor in liquors)
            {
                if (!liquor.isLocked)
                {
                    unlocked.Add(liquor);
                }
            }
            return unlocked.ToArray();
        }

        /// <summary>
        /// 取得指定類別的酒類
        /// </summary>
        public LiquorData[] GetLiquorsByCategory(LiquorCategory category)
        {
            if (liquors == null) return new LiquorData[0];

            var filtered = new System.Collections.Generic.List<LiquorData>();
            foreach (var liquor in liquors)
            {
                if (liquor.category == category)
                {
                    filtered.Add(liquor);
                }
            }
            return filtered.ToArray();
        }

        /// <summary>
        /// 升級酒類等級
        /// </summary>
        /// <returns>升級是否成功</returns>
        public bool UpgradeLiquor(string id)
        {
            var liquor = GetLiquor(id);
            if (liquor == null) return false;

            if (liquor.level >= 3)
            {
                Debug.LogWarning($"LiquorDatabase: {liquor.displayName} 已達最高等級");
                return false;
            }

            liquor.level++;
            Debug.Log($"LiquorDatabase: {liquor.displayName} 升級至 Level {liquor.level}");
            return true;
        }

        /// <summary>
        /// 解鎖酒類
        /// </summary>
        public bool UnlockLiquor(string id)
        {
            var liquor = GetLiquor(id);
            if (liquor == null) return false;

            if (!liquor.isLocked)
            {
                Debug.LogWarning($"LiquorDatabase: {liquor.displayName} 已經解鎖");
                return false;
            }

            liquor.isLocked = false;
            Debug.Log($"LiquorDatabase: 解鎖 {liquor.displayName}");
            return true;
        }

        /// <summary>
        /// 取得升級所需價格
        /// </summary>
        public int GetUpgradePrice(string id)
        {
            var liquor = GetLiquor(id);
            if (liquor == null || liquor.level >= 3) return -1;

            int priceIndex = liquor.level - 1; // Level 1→2 用index 0, Level 2→3 用index 1
            if (priceIndex < 0 || priceIndex >= liquor.upgradePrices.Length) return -1;

            return liquor.upgradePrices[priceIndex];
        }

        /// <summary>
        /// 初始化預設酒類資料庫
        /// 參考: CocktailSystem.js initLiquorDatabase()
        /// </summary>
        public void InitializeDefaults()
        {
            liquors = new LiquorData[]
            {
                // === 六大基酒 ===
                LiquorData.Create("vodka", "伏特加", "Vodka", 0xf0f0f0, 40f, LiquorCategory.BaseSpirit),
                LiquorData.Create("gin", "琴酒", "Gin", 0xe8f4f8, 40f, LiquorCategory.BaseSpirit),
                LiquorData.Create("rum", "蘭姆酒", "Rum", 0xd4a574, 40f, LiquorCategory.BaseSpirit),
                LiquorData.Create("whiskey", "威士忌", "Whiskey", 0xb87333, 40f, LiquorCategory.BaseSpirit),
                LiquorData.Create("tequila", "龍舌蘭", "Tequila", 0xf5deb3, 40f, LiquorCategory.BaseSpirit),
                LiquorData.Create("brandy", "白蘭地", "Brandy", 0x8b4513, 40f, LiquorCategory.BaseSpirit),

                // === 調味料 ===
                LiquorData.Create("lemon_juice", "檸檬汁", "Lemon Juice", 0xfff44f, 0f, LiquorCategory.Mixer),
                LiquorData.Create("lime_juice", "萊姆汁", "Lime Juice", 0x32cd32, 0f, LiquorCategory.Mixer),
                LiquorData.Create("simple_syrup", "糖漿", "Simple Syrup", 0xffe4b5, 0f, LiquorCategory.Mixer),
                LiquorData.Create("grenadine", "紅石榴糖漿", "Grenadine", 0xff0000, 0f, LiquorCategory.Mixer),
                LiquorData.Create("angostura_bitters", "安格仕苦精", "Angostura Bitters", 0x8b0000, 44.7f, LiquorCategory.Mixer),
                LiquorData.Create("soda_water", "蘇打水", "Soda Water", 0xe0ffff, 0f, LiquorCategory.Mixer),
                LiquorData.Create("tonic_water", "通寧水", "Tonic Water", 0xf0ffff, 0f, LiquorCategory.Mixer),
                LiquorData.Create("cola", "可樂", "Cola", 0x3e2723, 0f, LiquorCategory.Mixer),
                LiquorData.Create("coconut_cream", "椰漿", "Coconut Cream", 0xfffaf0, 0f, LiquorCategory.Mixer),

                // === 果汁類 ===
                LiquorData.Create("orange_juice", "柳橙汁", "Orange Juice", 0xffa500, 0f, LiquorCategory.Juice),
                LiquorData.Create("pineapple_juice", "鳳梨汁", "Pineapple Juice", 0xffeb3b, 0f, LiquorCategory.Juice),
                LiquorData.Create("cranberry_juice", "蔓越莓汁", "Cranberry Juice", 0xdc143c, 0f, LiquorCategory.Juice),
                LiquorData.Create("tomato_juice", "番茄汁", "Tomato Juice", 0xff6347, 0f, LiquorCategory.Juice),
                LiquorData.Create("grapefruit_juice", "葡萄柚汁", "Grapefruit Juice", 0xff69b4, 0f, LiquorCategory.Juice),

                // === 利口酒 & 香艾酒 ===
                LiquorData.Create("vermouth_dry", "不甜香艾酒", "Dry Vermouth", 0xe8e8d0, 18f, LiquorCategory.FortifiedWine),
                LiquorData.Create("vermouth_sweet", "甜香艾酒", "Sweet Vermouth", 0x8b4513, 18f, LiquorCategory.FortifiedWine),
                LiquorData.Create("campari", "金巴利", "Campari", 0xdc143c, 25f, LiquorCategory.Liqueur),
                LiquorData.Create("triple_sec", "橙皮酒", "Triple Sec", 0xffa500, 40f, LiquorCategory.Liqueur),
                LiquorData.Create("liqueur", "利口酒", "Liqueur", 0xff6b9d, 20f, LiquorCategory.Liqueur)
            };
        }
    }
}
