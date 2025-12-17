using UnityEngine;
using System.Collections.Generic;
using BarSimulator.Data;
using BarSimulator.Objects;

namespace BarSimulator.NPC
{
    /// <summary>
    /// Evaluates cocktails and calculates scores with level bonuses
    /// </summary>
    public class CocktailEvaluator
    {
        // Scoring constants
        private const int BASE_COINS = 200;
        private const float RATIO_TOLERANCE = 0.10f; // 10% tolerance for ratios

        /// <summary>
        /// Evaluate a drink against a cocktail recipe
        /// </summary>
        /// <param name="recipe">The expected cocktail recipe</param>
        /// <param name="glassContents">The actual contents of the glass</param>
        /// <param name="liquorDatabase">Reference to liquor database for level info</param>
        /// <returns>Evaluation result with score and coins</returns>
        public static EvaluationResult Evaluate(CocktailRecipe recipe, Dictionary<string, float> glassContents, LiquorDatabase liquorDatabase)
        {
            EvaluationResult result = new EvaluationResult();
            result.recipeName = recipe.name;

            // Step 1: Check if all required ingredients are present
            if (!recipe.HasCorrectIngredients(glassContents))
            {
                result.hasCorrectIngredients = false;
                result.coins = 0;
                result.feedback = "Wrong ingredients!";
                return result;
            }

            result.hasCorrectIngredients = true;

            // Step 2: Calculate total volume
            float totalVolume = 0f;
            foreach (var amount in glassContents.Values)
            {
                totalVolume += amount;
            }

            if (totalVolume <= 0f)
            {
                result.coins = 0;
                result.feedback = "Empty glass!";
                return result;
            }

            // Step 3: Check ratios (within 10% tolerance)
            bool ratiosCorrect = true;
            foreach (var ingredient in recipe.ingredients.Keys)
            {
                float expectedRatio = recipe.GetIngredientRatio(ingredient);
                float actualAmount = glassContents.ContainsKey(ingredient) ? glassContents[ingredient] : 0f;
                float actualRatio = actualAmount / totalVolume;

                float ratioDifference = Mathf.Abs(expectedRatio - actualRatio);
                
                if (ratioDifference > RATIO_TOLERANCE)
                {
                    ratiosCorrect = false;
                    break;
                }
            }

            result.hasCorrectRatios = ratiosCorrect;

            // Step 4: Base reward for correct ingredients
            result.coins = BASE_COINS;

            // Step 5: Calculate level bonus if ratios are correct
            if (ratiosCorrect)
            {
                float levelMultiplier = CalculateLevelMultiplier(recipe, glassContents, liquorDatabase);
                result.levelMultiplier = levelMultiplier;
                result.coins = Mathf.RoundToInt(BASE_COINS * levelMultiplier);
                result.feedback = $"Perfect! Level bonus: {levelMultiplier:F2}x";
            }
            else
            {
                result.feedback = "Correct ingredients, but ratios are off.";
            }

            return result;
        }

        /// <summary>
        /// Calculate level multiplier based on spirit levels
        /// Formula: (1.1 + 0.Y)^X where Y is level (1-3) and X is number of base spirits
        /// </summary>
        private static float CalculateLevelMultiplier(CocktailRecipe recipe, Dictionary<string, float> glassContents, LiquorDatabase liquorDatabase)
        {
            if (liquorDatabase == null)
                return 1.0f;

            List<string> baseSpiritIds = recipe.GetBaseSpiritIds();
            
            if (baseSpiritIds.Count == 0)
                return 1.0f;

            // Calculate average level of base spirits used
            float totalLevelBonus = 0f;
            int spiritCount = 0;

            foreach (var spiritId in baseSpiritIds)
            {
                // Try to find the liquor in the database
                LiquorData liquorData = FindLiquorByName(liquorDatabase, spiritId);
                
                if (liquorData != null)
                {
                    int level = liquorData.level;
                    float levelBonus = 1.1f + (level * 0.1f); // Level 1 = 1.2, Level 2 = 1.3, Level 3 = 1.4
                    totalLevelBonus += levelBonus;
                    spiritCount++;
                }
            }

            if (spiritCount == 0)
                return 1.0f;

            // Calculate average level bonus
            float avgLevelBonus = totalLevelBonus / spiritCount;

            // Apply power based on number of base spirits
            float multiplier = Mathf.Pow(avgLevelBonus, spiritCount);

            return multiplier;
        }

        /// <summary>
        /// Find liquor data by display name (case-insensitive)
        /// </summary>
        private static LiquorData FindLiquorByName(LiquorDatabase database, string name)
        {
            if (database == null || database.liquors == null)
                return null;

            foreach (var liquor in database.liquors)
            {
                if (liquor.displayName.Equals(name, System.StringComparison.OrdinalIgnoreCase) ||
                    liquor.id.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return liquor;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Result of cocktail evaluation
    /// </summary>
    public class EvaluationResult
    {
        public string recipeName;
        public bool hasCorrectIngredients;
        public bool hasCorrectRatios;
        public float levelMultiplier = 1.0f;
        public int coins;
        public string feedback;

        public override string ToString()
        {
            return $"{recipeName}: {feedback} - {coins} coins (x{levelMultiplier:F2})";
        }
    }
}
