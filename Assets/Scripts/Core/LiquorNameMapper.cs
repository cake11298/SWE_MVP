using System.Collections.Generic;

namespace BarSimulator.Core
{
    /// <summary>
    /// 統一酒類名稱映射系統
    /// 將所有變體名稱映射到標準名稱
    /// Standardizes all liquor name variations to canonical names
    /// </summary>
    public static class LiquorNameMapper
    {
        private static Dictionary<string, string> nameMap;

        static LiquorNameMapper()
        {
            InitializeMapping();
        }

        private static void InitializeMapping()
        {
            nameMap = new Dictionary<string, string>
            {
                // === VERMOUTH 統一為 vermouth ===
                { "vermouth_sweet", "vermouth" },
                { "sweet_vermouth", "vermouth" },
                { "Sweet Vermouth", "vermouth" },
                { "vermouth", "vermouth" },
                
                // === SYRUP 統一為 syrup ===
                { "amaretto_syrup", "syrup" },
                { "simple_syrup", "syrup" },
                { "Amaretto Syrup", "syrup" },
                { "Simple Syrup", "syrup" },
                { "syrup", "syrup" },
                
                // === JUICE 統一為 juice ===
                { "lemon_juice", "juice" },
                { "lime_juice", "juice" },
                { "Lemon Juice", "juice" },
                { "Lime Juice", "juice" },
                { "juice", "juice" },
                
                // === BASE SPIRITS (保持原樣) ===
                { "vodka", "vodka" },
                { "gin", "gin" },
                { "rum", "rum" },
                { "whiskey", "whiskey" },
                { "tequila", "tequila" },
                { "brandy", "brandy" },
                { "cognac", "cognac" },
                
                // === LIQUEURS ===
                { "cointreau", "cointreau" },
                { "triple_sec", "cointreau" }, // Triple Sec 統一為 Cointreau
                { "campari", "campari" },
                
                // === OTHER ===
                { "vermouth_dry", "vermouth_dry" },
                { "soda_water", "soda" },
                { "tonic_water", "tonic" },
                { "cola", "cola" },
                { "orange_juice", "orange_juice" },
                { "cranberry_juice", "cranberry_juice" },
                { "pineapple_juice", "pineapple_juice" },
                { "grenadine", "grenadine" },
                { "angostura_bitters", "bitters" }
            };
        }

        /// <summary>
        /// 將任何酒類名稱轉換為標準名稱
        /// Convert any liquor name to its canonical form
        /// </summary>
        public static string GetCanonicalName(string inputName)
        {
            if (string.IsNullOrEmpty(inputName))
                return inputName;

            // 嘗試直接映射
            if (nameMap.TryGetValue(inputName, out string canonical))
                return canonical;

            // 嘗試小寫映射
            string lowerInput = inputName.ToLower();
            if (nameMap.TryGetValue(lowerInput, out canonical))
                return canonical;

            // 嘗試移除空格和下劃線
            string normalized = lowerInput.Replace(" ", "_").Replace("-", "_");
            if (nameMap.TryGetValue(normalized, out canonical))
                return canonical;

            // 如果找不到映射，返回原始名稱（小寫）
            return lowerInput;
        }

        /// <summary>
        /// 取得顯示名稱（中文）
        /// Get display name in Chinese
        /// </summary>
        public static string GetDisplayNameZH(string canonicalName)
        {
            switch (canonicalName)
            {
                case "vodka": return "伏特加";
                case "gin": return "琴酒";
                case "rum": return "蘭姆酒";
                case "whiskey": return "威士忌";
                case "tequila": return "龍舌蘭";
                case "brandy": return "白蘭地";
                case "cognac": return "干邑白蘭地";
                case "vermouth": return "甜香艾酒";
                case "vermouth_dry": return "不甜香艾酒";
                case "syrup": return "糖漿";
                case "juice": return "果汁";
                case "cointreau": return "君度橙酒";
                case "campari": return "金巴利";
                case "soda": return "蘇打水";
                case "tonic": return "通寧水";
                case "cola": return "可樂";
                case "orange_juice": return "柳橙汁";
                case "cranberry_juice": return "蔓越莓汁";
                case "pineapple_juice": return "鳳梨汁";
                case "grenadine": return "紅石榴糖漿";
                case "bitters": return "苦精";
                default: return canonicalName;
            }
        }

        /// <summary>
        /// 取得顯示名稱（英文）
        /// Get display name in English
        /// </summary>
        public static string GetDisplayNameEN(string canonicalName)
        {
            switch (canonicalName)
            {
                case "vodka": return "Vodka";
                case "gin": return "Gin";
                case "rum": return "Rum";
                case "whiskey": return "Whiskey";
                case "tequila": return "Tequila";
                case "brandy": return "Brandy";
                case "cognac": return "Cognac";
                case "vermouth": return "Vermouth";
                case "vermouth_dry": return "Dry Vermouth";
                case "syrup": return "Syrup";
                case "juice": return "Juice";
                case "cointreau": return "Cointreau";
                case "campari": return "Campari";
                case "soda": return "Soda";
                case "tonic": return "Tonic";
                case "cola": return "Cola";
                case "orange_juice": return "Orange Juice";
                case "cranberry_juice": return "Cranberry Juice";
                case "pineapple_juice": return "Pineapple Juice";
                case "grenadine": return "Grenadine";
                case "bitters": return "Bitters";
                default: return canonicalName;
            }
        }

        /// <summary>
        /// 批量轉換成分字典的鍵名
        /// Convert all keys in an ingredient dictionary to canonical names
        /// </summary>
        public static Dictionary<string, float> NormalizeIngredients(Dictionary<string, float> ingredients)
        {
            if (ingredients == null)
                return new Dictionary<string, float>();

            var normalized = new Dictionary<string, float>();
            foreach (var kvp in ingredients)
            {
                string canonical = GetCanonicalName(kvp.Key);
                if (normalized.ContainsKey(canonical))
                {
                    // 如果已存在，累加數量
                    normalized[canonical] += kvp.Value;
                }
                else
                {
                    normalized[canonical] = kvp.Value;
                }
            }
            return normalized;
        }
    }
}
