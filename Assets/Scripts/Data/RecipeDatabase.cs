using System.Collections.Generic;
using System.Linq;

namespace BarSimulator.Data
{
    /// <summary>
    /// Static database containing all cocktail recipes.
    /// Pure C# class - no MonoBehaviour, no ScriptableObject dependencies.
    /// All recipes are hardcoded for reliability and performance.
    /// </summary>
    public static class RecipeDatabase
    {
        private static List<DrinkRecipe> allRecipes;

        /// <summary>
        /// Get all available recipes in the game.
        /// </summary>
        public static List<DrinkRecipe> AllRecipes
        {
            get
            {
                if (allRecipes == null)
                {
                    Initialize();
                }
                return allRecipes;
            }
        }

        /// <summary>
        /// Initialize the recipe database with hardcoded cocktails.
        /// Called automatically on first access.
        /// </summary>
        private static void Initialize()
        {
            allRecipes = new List<DrinkRecipe>
            {
                // === CLASSIC COCKTAILS ===

                // Martini - The king of cocktails
                new DrinkRecipe(
                    name: "Martini",
                    ingredients: new List<string> { "gin", "vermouth" },
                    shakeTime: 0f,
                    requiresShaking: false, // Traditionally stirred, not shaken
                    difficultyLevel: 2,
                    preferredGlassType: "Cocktail Glass",
                    garnish: "Olive"
                ),

                // Negroni - Perfect bitter Italian cocktail
                new DrinkRecipe(
                    name: "Negroni",
                    ingredients: new List<string> { "gin", "campari", "vermouth" },
                    shakeTime: 0f,
                    requiresShaking: false, // Built in glass, stirred
                    difficultyLevel: 2,
                    preferredGlassType: "Rocks Glass",
                    garnish: "Orange Peel"
                ),

                // Margarita - Classic tequila cocktail
                new DrinkRecipe(
                    name: "Margarita",
                    ingredients: new List<string> { "tequila", "cointreau", "juice" },
                    shakeTime: 3.0f,
                    requiresShaking: true, // REQUIRES SHAKING - triggers QTE
                    difficultyLevel: 3,
                    preferredGlassType: "Cocktail Glass",
                    garnish: "Lime Wedge"
                ),

                // === ADDITIONAL CLASSIC COCKTAILS ===

                // Gin & Tonic - Simple highball
                new DrinkRecipe(
                    name: "Gin & Tonic",
                    ingredients: new List<string> { "gin", "syrup" }, // Replaced tonic_water with syrup as requested
                    shakeTime: 0f,
                    requiresShaking: false,
                    difficultyLevel: 1,
                    preferredGlassType: "Highball",
                    garnish: "Lime Wedge"
                ),

                // Vodka Tonic - Vodka variation
                new DrinkRecipe(
                    name: "Vodka Tonic",
                    ingredients: new List<string> { "vodka", "syrup" }, // Replaced tonic_water with syrup as requested
                    shakeTime: 0f,
                    requiresShaking: false,
                    difficultyLevel: 1,
                    preferredGlassType: "Highball",
                    garnish: "Lime Wedge"
                ),

                // Whiskey Sour - Classic sour cocktail
                new DrinkRecipe(
                    name: "Whiskey Sour",
                    ingredients: new List<string> { "whiskey", "juice", "syrup" },
                    shakeTime: 4.0f,
                    requiresShaking: true, // REQUIRES SHAKING
                    difficultyLevel: 3,
                    preferredGlassType: "Rocks Glass",
                    garnish: "Lemon Wedge"
                ),

                // Cosmopolitan - Modern classic
                new DrinkRecipe(
                    name: "Cosmopolitan",
                    ingredients: new List<string> { "vodka", "cointreau", "juice" }, // Removed cranberry_juice as it's not available
                    shakeTime: 3.5f,
                    requiresShaking: true, // REQUIRES SHAKING
                    difficultyLevel: 4,
                    preferredGlassType: "Cocktail Glass",
                    garnish: "Lime Wedge"
                ),

                // Daiquiri - Classic rum cocktail
                new DrinkRecipe(
                    name: "Daiquiri",
                    ingredients: new List<string> { "rum", "juice", "syrup" },
                    shakeTime: 3.0f,
                    requiresShaking: true, // REQUIRES SHAKING
                    difficultyLevel: 3,
                    preferredGlassType: "Cocktail Glass",
                    garnish: "Lime Wheel"
                ),

                // Mojito - Refreshing Cuban cocktail
                new DrinkRecipe(
                    name: "Mojito",
                    ingredients: new List<string> { "rum", "juice", "syrup" },
                    shakeTime: 0f,
                    requiresShaking: false, // Muddled and stirred
                    difficultyLevel: 3,
                    preferredGlassType: "Highball",
                    garnish: "Mint Leaves"
                ),

                // Old Fashioned - Timeless classic
                new DrinkRecipe(
                    name: "Old Fashioned",
                    ingredients: new List<string> { "whiskey", "syrup", "bitters" }, // Replaced angostura_bitters with bitters
                    shakeTime: 0f,
                    requiresShaking: false, // Stirred
                    difficultyLevel: 2,
                    preferredGlassType: "Rocks Glass",
                    garnish: "Orange Peel"
                )
            };

            UnityEngine.Debug.Log($"[RecipeDatabase] Initialized with {allRecipes.Count} recipes.");
        }

        /// <summary>
        /// Find a recipe by exact name match (case-insensitive).
        /// </summary>
        public static DrinkRecipe GetRecipeByName(string drinkName)
        {
            if (string.IsNullOrEmpty(drinkName))
                return null;

            return AllRecipes.FirstOrDefault(r =>
                r.Name.Equals(drinkName, System.StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// Identify a drink recipe based on provided ingredients (order-independent).
        /// Returns null if no exact match is found.
        /// </summary>
        public static DrinkRecipe IdentifyDrink(List<string> ingredientIDs)
        {
            if (ingredientIDs == null || ingredientIDs.Count == 0)
                return null;

            return AllRecipes.FirstOrDefault(recipe => recipe.MatchesIngredients(ingredientIDs));
        }

        /// <summary>
        /// Get all recipes that require shaking (for QTE minigame).
        /// </summary>
        public static List<DrinkRecipe> GetShakeableRecipes()
        {
            return AllRecipes.Where(r => r.RequiresShaking).ToList();
        }

        /// <summary>
        /// Get recipes by difficulty level.
        /// </summary>
        public static List<DrinkRecipe> GetRecipesByDifficulty(int difficultyLevel)
        {
            return AllRecipes.Where(r => r.DifficultyLevel == difficultyLevel).ToList();
        }

        /// <summary>
        /// Get a random recipe (useful for NPC orders).
        /// </summary>
        public static DrinkRecipe GetRandomRecipe()
        {
            if (AllRecipes.Count == 0)
                return null;

            int randomIndex = UnityEngine.Random.Range(0, AllRecipes.Count);
            return AllRecipes[randomIndex];
        }

        /// <summary>
        /// Get a random recipe filtered by difficulty.
        /// </summary>
        public static DrinkRecipe GetRandomRecipe(int maxDifficulty)
        {
            var filteredRecipes = AllRecipes.Where(r => r.DifficultyLevel <= maxDifficulty).ToList();

            if (filteredRecipes.Count == 0)
                return GetRandomRecipe(); // Fallback to any recipe

            int randomIndex = UnityEngine.Random.Range(0, filteredRecipes.Count);
            return filteredRecipes[randomIndex];
        }
    }
}
