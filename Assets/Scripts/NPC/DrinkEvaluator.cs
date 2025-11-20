using System.Collections.Generic;
using UnityEngine;
using BarSimulator.Data;
using BarSimulator.Objects;
using BarSimulator.Systems;

namespace BarSimulator.NPC
{
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

        #endregion

        #region 公開方法

        /// <summary>
        /// 評估飲料品質
        /// </summary>
        public static DrinkEvaluation Evaluate(DrinkInfo drinkInfo)
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

            // 計算酒精濃度
            evaluation.alcoholContent = CalculateAlcoholContent(drinkInfo.ingredients);

            // 檢查是否為有效雞尾酒
            evaluation.isValidCocktail = IsValidCocktail(drinkInfo.cocktailName);
            if (evaluation.isValidCocktail)
            {
                evaluation.score += ValidCocktailBonus;
            }

            // 評估容量
            evaluation.score += EvaluateVolume(drinkInfo.volume);

            // 評估酒精濃度
            evaluation.score += EvaluateAlcoholContent(evaluation.alcoholContent);

            // 評估成分平衡
            evaluation.score += EvaluateBalance(drinkInfo.ingredients);

            // 限制分數範圍
            evaluation.score = Mathf.Clamp(evaluation.score, 0, 100);

            // 產生評價訊息
            evaluation.message = GenerateMessage(evaluation);

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
        /// 產生評價訊息
        /// </summary>
        private static string GenerateMessage(DrinkEvaluation evaluation)
        {
            if (evaluation.score >= 90)
            {
                return "Perfect! This is exactly what I wanted!";
            }
            else if (evaluation.score >= 80)
            {
                return "Excellent work! Very well made.";
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
