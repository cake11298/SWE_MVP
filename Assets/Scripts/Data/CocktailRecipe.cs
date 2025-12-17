using System.Collections.Generic;
using UnityEngine;

namespace BarSimulator.Data
{
    /// <summary>
    /// Enhanced cocktail recipe with ingredient ratios
    /// </summary>
    [System.Serializable]
    public class CocktailRecipe
    {
        /// <summary>
        /// Cocktail name
        /// </summary>
        public string name;

        /// <summary>
        /// Ingredient requirements: liquor ID -> required amount (in units, not ml)
        /// Example: "Vodka" -> 2 means 2 units of vodka
        /// </summary>
        public Dictionary<string, int> ingredients;

        /// <summary>
        /// Total units in the recipe (sum of all ingredient units)
        /// </summary>
        public int totalUnits;

        /// <summary>
        /// Difficulty level (affects base reward)
        /// </summary>
        public int difficulty;

        public CocktailRecipe(string name, Dictionary<string, int> ingredients, int difficulty = 1)
        {
            this.name = name;
            this.ingredients = ingredients;
            this.difficulty = difficulty;
            
            // Calculate total units
            totalUnits = 0;
            foreach (var amount in ingredients.Values)
            {
                totalUnits += amount;
            }
        }

        /// <summary>
        /// Get the expected ratio for a specific ingredient (0-1)
        /// </summary>
        public float GetIngredientRatio(string ingredientId)
        {
            if (!ingredients.ContainsKey(ingredientId) || totalUnits == 0)
                return 0f;
            
            return (float)ingredients[ingredientId] / totalUnits;
        }

        /// <summary>
        /// Check if the provided ingredients match this recipe (by type, not ratio)
        /// </summary>
        public bool HasCorrectIngredients(Dictionary<string, float> providedIngredients)
        {
            // Check if all required ingredients are present
            foreach (var requiredIngredient in ingredients.Keys)
            {
                if (!providedIngredients.ContainsKey(requiredIngredient))
                    return false;
            }

            // Check if there are no extra ingredients
            foreach (var providedIngredient in providedIngredients.Keys)
            {
                if (!ingredients.ContainsKey(providedIngredient))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get list of base spirits (spirits with levels) in this recipe
        /// </summary>
        public List<string> GetBaseSpiritIds()
        {
            List<string> baseSpirits = new List<string>();
            string[] baseSpiritNames = { "Vodka", "Gin", "Whiskey", "Rum", "Cointreau" };

            foreach (var ingredient in ingredients.Keys)
            {
                foreach (var baseName in baseSpiritNames)
                {
                    if (ingredient.Equals(baseName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        baseSpirits.Add(ingredient);
                        break;
                    }
                }
            }

            return baseSpirits;
        }
    }

    /// <summary>
    /// Static database of all cocktail recipes
    /// </summary>
    public static class CocktailDatabase
    {
        private static Dictionary<string, CocktailRecipe> recipes;

        public static Dictionary<string, CocktailRecipe> AllRecipes
        {
            get
            {
                if (recipes == null)
                {
                    Initialize();
                }
                return recipes;
            }
        }

        private static void Initialize()
        {
            recipes = new Dictionary<string, CocktailRecipe>();

            // Screwdriver
            recipes["Screwdriver"] = new CocktailRecipe("Screwdriver", new Dictionary<string, int>
            {
                { "Vodka", 2 },
                { "Juice", 3 }
            }, difficulty: 1);

            // Martini
            recipes["Martini"] = new CocktailRecipe("Martini", new Dictionary<string, int>
            {
                { "Gin", 5 },
                { "Wine", 1 }
            }, difficulty: 2);

            // Vodka Martini
            recipes["Vodka Martini"] = new CocktailRecipe("Vodka Martini", new Dictionary<string, int>
            {
                { "Vodka", 5 },
                { "Wine", 1 }
            }, difficulty: 2);

            // Whiskey Highball
            recipes["Whiskey Highball"] = new CocktailRecipe("Whiskey Highball", new Dictionary<string, int>
            {
                { "Whiskey", 1 },
                { "Soda", 3 }
            }, difficulty: 1);

            // Gin and Tonic
            recipes["Gin and Tonic"] = new CocktailRecipe("Gin and Tonic", new Dictionary<string, int>
            {
                { "Gin", 1 },
                { "Soda", 2 }
            }, difficulty: 1);

            // Whiskey Sour
            recipes["Whiskey Sour"] = new CocktailRecipe("Whiskey Sour", new Dictionary<string, int>
            {
                { "Whiskey", 2 },
                { "Juice", 1 },
                { "Soda", 1 }
            }, difficulty: 2);

            // Daiquiri
            recipes["Daiquiri"] = new CocktailRecipe("Daiquiri", new Dictionary<string, int>
            {
                { "Rum", 2 },
                { "Juice", 1 },
                { "Soda", 1 }
            }, difficulty: 2);

            // Sidecar
            recipes["Sidecar"] = new CocktailRecipe("Sidecar", new Dictionary<string, int>
            {
                { "Wine", 2 },
                { "Cointreau", 1 },
                { "Juice", 1 }
            }, difficulty: 3);

            // White Lady
            recipes["White Lady"] = new CocktailRecipe("White Lady", new Dictionary<string, int>
            {
                { "Gin", 2 },
                { "Cointreau", 1 },
                { "Juice", 1 }
            }, difficulty: 3);

            // Cosmopolitan
            recipes["Cosmopolitan"] = new CocktailRecipe("Cosmopolitan", new Dictionary<string, int>
            {
                { "Vodka", 2 },
                { "Cointreau", 1 },
                { "Juice", 1 }
            }, difficulty: 3);

            Debug.Log($"[CocktailDatabase] Initialized with {recipes.Count} cocktail recipes");
        }

        public static CocktailRecipe GetRecipe(string name)
        {
            if (AllRecipes.ContainsKey(name))
                return AllRecipes[name];
            return null;
        }

        public static CocktailRecipe GetRandomRecipe()
        {
            var recipeList = new List<CocktailRecipe>(AllRecipes.Values);
            if (recipeList.Count == 0)
                return null;
            
            return recipeList[Random.Range(0, recipeList.Count)];
        }

        public static List<string> GetAllRecipeNames()
        {
            return new List<string>(AllRecipes.Keys);
        }
    }
}
