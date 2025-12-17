using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BarSimulator.Data;
using BarSimulator.Objects;
using BarSimulator.Systems;

namespace BarSimulator.NPC
{
    /// <summary>
    /// 雞尾酒配方資料
    /// </summary>
    public class RecipeData
    {
        public string name;
        public Dictionary<string, float> ingredients; // 成分類型 -> 比例 (標準化為1)

        public RecipeData(string name, Dictionary<string, float> ingredients)
        {
            this.name = name;
            this.ingredients = ingredients;
        }
    }

    /// <summary>
    /// 飲料評估結果
    /// </summary>
    [System.Serializable]
    public class DrinkEvaluation
    {
        /// <summary>評分 (0-100)</summary>
        public int score;

        /// <summary>雞尾酒名稱</summary>
        public string cocktailName;

        /// <summary>評價訊息</summary>
        public string message;

        /// <summary>是否為有效的雞尾酒</summary>
        public bool isValidCocktail;

        /// <summary>酒精濃度</summary>
        public float alcoholContent;

        /// <summary>容量</summary>
        public float volume;
    }

    /// <summary>
    /// 飲料評分系統 - 評估調酒品質
    /// 參考: NPCManager.js 飲料評分邏輯
    /// </summary>
    public static class DrinkEvaluator
    {
        #region 評分標準

        // 基礎分數
        private const int BaseScore = 50;

        // 有效雞尾酒加分
        private const int ValidCocktailBonus = 30;

        // 容量評分範圍
        private const float OptimalVolumeMin = 100f;
        private const float OptimalVolumeMax = 250f;
        private const int VolumeBonus = 10;

        // 酒精濃度評分
        private const float OptimalAlcoholMin = 15f;
        private const float OptimalAlcoholMax = 35f;
        private const int AlcoholBonus = 10;

        // 幾何距離評分
        private const float PerfectDistanceThreshold = 0.1f;  // 10% 誤差為滿分
        private const float GoodDistanceThreshold = 0.3f;     // 30% 誤差為良好
        private const int GeometricDistanceBonus = 20;

        // === 升級系統新增參數 ===

        /// <summary>等級分數上限加成 (Level 2: +10, Level 3: +20)</summary>
        private static readonly int[] LevelScoreBonus = { 0, 10, 20 };

        /// <summary>等級容錯率乘數 (Level 1: 1.0, Level 2: 1.3, Level 3: 1.6)</summary>
        private static readonly float[] LevelToleranceMultiplier = { 1.0f, 1.3f, 1.6f };

        #endregion

        #region 配方資料庫

        /// <summary>
        /// 標準雞尾酒配方（比例標準化）
        /// </summary>
        private static readonly Dictionary<string, RecipeData> RecipeDatabase = new Dictionary<string, RecipeData>
        {
            // Martini: 2份琴酒 + 1份乾苦艾酒
            { "Martini", new RecipeData("Martini", new Dictionary<string, float> {
                { "gin", 0.67f },
                { "vermouth", 0.33f }
            })},

            // Vodka Martini: 2份伏特加 + 1份乾苦艾酒
            { "Vodka Martini", new RecipeData("Vodka Martini", new Dictionary<string, float> {
                { "vodka", 0.67f },
                { "vermouth", 0.33f }
            })},

            // Negroni: 1:1:1 比例
            { "Negroni", new RecipeData("Negroni", new Dictionary<string, float> {
                { "gin", 0.33f },
                { "campari", 0.33f },
                { "vermouth", 0.33f }
            })},

            // Margarita: 2份龍舌蘭 + 1份橙酒 + 1份青檸汁
            { "Margarita", new RecipeData("Margarita", new Dictionary<string, float> {
                { "tequila", 0.5f },
                { "cointreau", 0.25f },
                { "juice", 0.25f }
            })},

            // Daiquiri: 2份朗姆酒 + 1份青檸汁 + 0.5份糖漿
            { "Daiquiri", new RecipeData("Daiquiri", new Dictionary<string, float> {
                { "rum", 0.57f },
                { "juice", 0.29f },
                { "syrup", 0.14f }
            })},

            // Cosmopolitan: 1.5份伏特加 + 1份橙酒 + 0.5份青檸汁 + 1份蔓越莓汁
            { "Cosmopolitan", new RecipeData("Cosmopolitan", new Dictionary<string, float> {
                { "vodka", 0.375f },
                { "cointreau", 0.25f },
                { "juice", 0.125f },
                { "cranberry_juice", 0.25f }
            })},

            // Whiskey Sour: 2份威士忌 + 1份檸檬汁 + 0.5份糖漿
            { "Whiskey Sour", new RecipeData("Whiskey Sour", new Dictionary<string, float> {
                { "whiskey", 0.57f },
                { "juice", 0.29f },
                { "syrup", 0.14f }
            })},

            // Mojito: 2份朗姆酒 + 1份青檸汁 + 0.5份糖漿
            { "Mojito", new RecipeData("Mojito", new Dictionary<string, float> {
                { "rum", 0.57f },
                { "juice", 0.29f },
                { "syrup", 0.14f }
            })},

            // Pina Colada: 2份朗姆酒 + 2份鳳梨汁 + 1份椰漿
            { "Pina Colada", new RecipeData("Pina Colada", new Dictionary<string, float> {
                { "rum", 0.4f },
                { "pineapple_juice", 0.4f },
                { "coconut_cream", 0.2f }
            })}
        };

        #endregion

        #region 公開方法

        /// <summary>
        /// 評估飲料品質
        /// </summary>
        /// <param name="drinkInfo">飲料資訊</param>
        /// <param name="liquorDatabase">酒類資料庫（用於查詢等級）</param>
        public static DrinkEvaluation Evaluate(DrinkInfo drinkInfo, LiquorDatabase liquorDatabase = null)
        {
            var evaluation = new DrinkEvaluation
            {
                cocktailName = drinkInfo.cocktailName,
                volume = drinkInfo.volume,
                score = BaseScore
            };

            // 檢查是否為空
            if (drinkInfo.ingredients == null || drinkInfo.ingredients.Length == 0)
            {
                evaluation.score = 0;
                evaluation.message = "The glass is empty!";
                evaluation.isValidCocktail = false;
                return evaluation;
            }

            // === 計算酒類平均等級和容錯率 ===
            float averageLevel = CalculateAverageLiquorLevel(drinkInfo.ingredients, liquorDatabase);
            int levelIndex = Mathf.Clamp(Mathf.RoundToInt(averageLevel) - 1, 0, 2); // Level 1-3 → Index 0-2
            float toleranceMultiplier = LevelToleranceMultiplier[levelIndex];
            int scoreBonus = LevelScoreBonus[levelIndex];

            // 計算酒精濃度
            evaluation.alcoholContent = CalculateAlcoholContent(drinkInfo.ingredients);

            // 檢查是否為有效雞尾酒
            evaluation.isValidCocktail = IsValidCocktail(drinkInfo.cocktailName);
            if (evaluation.isValidCocktail)
            {
                evaluation.score += ValidCocktailBonus;

                // 使用幾何距離評估配方準確度（應用容錯率）
                int geometricScore = EvaluateGeometricDistance(
                    drinkInfo.cocktailName,
                    drinkInfo.ingredients,
                    toleranceMultiplier
                );
                evaluation.score += geometricScore;
            }

            // 評估容量
            evaluation.score += EvaluateVolume(drinkInfo.volume);

            // 評估酒精濃度
            evaluation.score += EvaluateAlcoholContent(evaluation.alcoholContent);

            // 評估成分平衡
            evaluation.score += EvaluateBalance(drinkInfo.ingredients);

            // 應用等級分數加成
            evaluation.score += scoreBonus;

            // 限制分數範圍（高級酒可突破100分）
            int maxScore = 100 + scoreBonus;
            evaluation.score = Mathf.Clamp(evaluation.score, 0, maxScore);

            // 產生評價訊息
            evaluation.message = GenerateMessage(evaluation, averageLevel);

            return evaluation;
        }

        /// <summary>
        /// 評估特定雞尾酒的準確度
        /// </summary>
        public static int EvaluateCocktailAccuracy(string targetCocktail, DrinkInfo drinkInfo)
        {
            // 先進行基本評估
            var baseEvaluation = Evaluate(drinkInfo);

            // 檢查是否符合目標雞尾酒
            if (drinkInfo.cocktailName == targetCocktail)
            {
                return Mathf.Min(baseEvaluation.score + 20, 100);
            }

            // 不符合目標扣分
            return Mathf.Max(baseEvaluation.score - 30, 0);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 計算酒類平均等級
        /// </summary>
        private static float CalculateAverageLiquorLevel(Ingredient[] ingredients, LiquorDatabase liquorDatabase)
        {
            if (ingredients == null || ingredients.Length == 0) return 1f;
            if (liquorDatabase == null)
            {
                // 如果沒有資料庫，從CocktailSystem取得
                var cocktailSystem = CocktailSystem.Instance;
                if (cocktailSystem != null)
                {
                    liquorDatabase = cocktailSystem.LiquorDatabase;
                }
            }

            if (liquorDatabase == null) return 1f; // 預設為Level 1

            float totalLevel = 0f;
            int count = 0;

            foreach (var ingredient in ingredients)
            {
                var liquor = liquorDatabase.GetLiquor(ingredient.type);
                if (liquor != null)
                {
                    totalLevel += liquor.level;
                    count++;
                }
            }

            if (count == 0) return 1f;
            return totalLevel / count;
        }

        /// <summary>
        /// 計算酒精濃度
        /// </summary>
        private static float CalculateAlcoholContent(Ingredient[] ingredients)
        {
            if (ingredients == null || ingredients.Length == 0) return 0f;

            var cocktailSystem = CocktailSystem.Instance;
            if (cocktailSystem == null || cocktailSystem.LiquorDatabase == null) return 0f;

            float totalAlcohol = 0f;
            float totalVolume = 0f;

            foreach (var ingredient in ingredients)
            {
                var liquor = cocktailSystem.LiquorDatabase.GetLiquor(ingredient.type);
                if (liquor != null)
                {
                    totalAlcohol += ingredient.amount * (liquor.alcoholContent / 100f);
                }
                totalVolume += ingredient.amount;
            }

            if (totalVolume <= 0) return 0f;
            return (totalAlcohol / totalVolume) * 100f;
        }

        /// <summary>
        /// 檢查是否為有效雞尾酒名稱
        /// </summary>
        private static bool IsValidCocktail(string cocktailName)
        {
            // 有效雞尾酒名稱列表
            string[] validCocktails = {
                "Martini", "Vodka Martini", "Negroni", "Margarita",
                "Daiquiri", "Pina Colada", "Cosmopolitan", "Mojito",
                "Whiskey Sour"
            };

            foreach (var valid in validCocktails)
            {
                if (cocktailName == valid)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 評估容量
        /// </summary>
        private static int EvaluateVolume(float volume)
        {
            if (volume >= OptimalVolumeMin && volume <= OptimalVolumeMax)
            {
                return VolumeBonus;
            }
            else if (volume < OptimalVolumeMin)
            {
                // 太少
                return -5;
            }
            else
            {
                // 太多
                return -3;
            }
        }

        /// <summary>
        /// 評估酒精濃度
        /// </summary>
        private static int EvaluateAlcoholContent(float alcoholContent)
        {
            if (alcoholContent >= OptimalAlcoholMin && alcoholContent <= OptimalAlcoholMax)
            {
                return AlcoholBonus;
            }
            else if (alcoholContent < 5f)
            {
                // 幾乎沒酒精
                return -10;
            }
            else if (alcoholContent > 50f)
            {
                // 太烈
                return -5;
            }

            return 0;
        }

        /// <summary>
        /// 評估成分平衡
        /// </summary>
        private static int EvaluateBalance(Ingredient[] ingredients)
        {
            if (ingredients == null || ingredients.Length == 0) return 0;

            // 計算成分數量分數
            int count = ingredients.Length;

            if (count == 1)
            {
                // 單一成分（純飲）
                return -5;
            }
            else if (count >= 2 && count <= 4)
            {
                // 理想成分數量
                return 5;
            }
            else if (count > 6)
            {
                // 太多成分
                return -3;
            }

            return 0;
        }

        /// <summary>
        /// 使用幾何距離評估配方準確度
        /// 計算實際成分比例與標準配方的歐幾里得距離
        /// </summary>
        /// <param name="toleranceMultiplier">容錯率乘數（高等級酒類有更高容錯）</param>
        private static int EvaluateGeometricDistance(string cocktailName, Ingredient[] ingredients, float toleranceMultiplier = 1.0f)
        {
            // 檢查是否有對應配方
            if (!RecipeDatabase.TryGetValue(cocktailName, out RecipeData recipe))
            {
                return 0; // 無配方資料，不額外評分
            }

            if (ingredients == null || ingredients.Length == 0)
            {
                return -GeometricDistanceBonus;
            }

            // 計算實際成分的總量
            float totalAmount = ingredients.Sum(i => i.amount);
            if (totalAmount <= 0) return -GeometricDistanceBonus;

            // 轉換實際成分為比例字典
            var actualRatios = new Dictionary<string, float>();
            foreach (var ingredient in ingredients)
            {
                float ratio = ingredient.amount / totalAmount;
                if (actualRatios.ContainsKey(ingredient.type))
                {
                    actualRatios[ingredient.type] += ratio;
                }
                else
                {
                    actualRatios[ingredient.type] = ratio;
                }
            }

            // 計算幾何距離
            float distance = CalculateEuclideanDistance(recipe.ingredients, actualRatios);

            // 應用容錯率（高等級酒類容錯範圍更大）
            float adjustedPerfectThreshold = PerfectDistanceThreshold * toleranceMultiplier;
            float adjustedGoodThreshold = GoodDistanceThreshold * toleranceMultiplier;

            // 根據距離計算分數
            if (distance <= adjustedPerfectThreshold)
            {
                // 誤差在容錯範圍以內，滿分
                return GeometricDistanceBonus;
            }
            else if (distance <= adjustedGoodThreshold)
            {
                // 誤差在良好範圍以內，部分加分
                float factor = 1f - ((distance - adjustedPerfectThreshold) / (adjustedGoodThreshold - adjustedPerfectThreshold));
                return Mathf.RoundToInt(GeometricDistanceBonus * factor * 0.7f);
            }
            else
            {
                // 誤差太大，遞減給分
                float factor = Mathf.Max(0f, 1f - (distance - adjustedGoodThreshold));
                return Mathf.RoundToInt(GeometricDistanceBonus * factor * 0.3f) - 5;
            }
        }

        /// <summary>
        /// 計算兩個成分比例向量的歐幾里得距離
        /// </summary>
        private static float CalculateEuclideanDistance(Dictionary<string, float> expected, Dictionary<string, float> actual)
        {
            // 收集所有成分類型
            var allTypes = new HashSet<string>(expected.Keys);
            foreach (var type in actual.Keys)
            {
                allTypes.Add(type);
            }

            // 計算距離平方和
            float sumSquared = 0f;
            foreach (var type in allTypes)
            {
                float expectedValue = expected.TryGetValue(type, out float e) ? e : 0f;
                float actualValue = actual.TryGetValue(type, out float a) ? a : 0f;
                float diff = expectedValue - actualValue;
                sumSquared += diff * diff;
            }

            return Mathf.Sqrt(sumSquared);
        }

        /// <summary>
        /// 尋找最接近的雞尾酒配方
        /// </summary>
        public static string FindClosestRecipe(Ingredient[] ingredients)
        {
            if (ingredients == null || ingredients.Length == 0)
            {
                return "Empty Glass";
            }

            // 計算實際成分比例
            float totalAmount = ingredients.Sum(i => i.amount);
            if (totalAmount <= 0) return "Unknown Mix";

            var actualRatios = new Dictionary<string, float>();
            foreach (var ingredient in ingredients)
            {
                float ratio = ingredient.amount / totalAmount;
                if (actualRatios.ContainsKey(ingredient.type))
                {
                    actualRatios[ingredient.type] += ratio;
                }
                else
                {
                    actualRatios[ingredient.type] = ratio;
                }
            }

            // 找到距離最近的配方
            string closestRecipe = "Custom Cocktail";
            float minDistance = float.MaxValue;

            foreach (var recipe in RecipeDatabase)
            {
                float distance = CalculateEuclideanDistance(recipe.Value.ingredients, actualRatios);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestRecipe = recipe.Key;
                }
            }

            // 如果最近距離太大，返回自定義
            if (minDistance > 0.5f)
            {
                return "Custom Cocktail";
            }

            return closestRecipe;
        }

        /// <summary>
        /// 產生評價訊息
        /// </summary>
        private static string GenerateMessage(DrinkEvaluation evaluation, float averageLevel = 1f)
        {
            // 高級酒類有特殊評語
            bool isPremiumDrink = averageLevel >= 2.5f;
            bool isHighQualityDrink = averageLevel >= 1.5f && averageLevel < 2.5f;

            if (evaluation.score >= 110)
            {
                return "Masterpiece! The premium ingredients really shine through!";
            }
            else if (evaluation.score >= 100)
            {
                return isPremiumDrink
                    ? "Outstanding! These premium spirits make all the difference!"
                    : "Perfect! This is exactly what I wanted!";
            }
            else if (evaluation.score >= 90)
            {
                return isPremiumDrink
                    ? "Excellent! You've made great use of these high-quality ingredients!"
                    : "Perfect! This is exactly what I wanted!";
            }
            else if (evaluation.score >= 80)
            {
                return isHighQualityDrink
                    ? "Very good! The quality ingredients are noticeable."
                    : "Excellent work! Very well made.";
            }
            else if (evaluation.score >= 70)
            {
                return "Good! This tastes nice.";
            }
            else if (evaluation.score >= 60)
            {
                return "Not bad, but could be better.";
            }
            else if (evaluation.score >= 50)
            {
                return "It's okay, I guess...";
            }
            else if (evaluation.score >= 30)
            {
                return "This doesn't taste quite right.";
            }
            else
            {
                return "What is this? This isn't drinkable!";
            }
        }

        #endregion
    }
}
