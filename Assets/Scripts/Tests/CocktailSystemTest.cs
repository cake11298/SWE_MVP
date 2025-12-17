using UnityEngine;
using System.Collections.Generic;
using BarSimulator.Data;
using BarSimulator.NPC;

namespace BarSimulator.Tests
{
    /// <summary>
    /// Test script to demonstrate the cocktail evaluation system
    /// Attach to any GameObject and press T to run tests
    /// </summary>
    public class CocktailSystemTest : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LiquorDatabase liquorDatabase;

        private void Start()
        {
            if (liquorDatabase == null)
            {
                liquorDatabase = Resources.Load<LiquorDatabase>("LiquorDataBase");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                RunTests();
            }
        }

        private void RunTests()
        {
            Debug.Log("=== COCKTAIL SYSTEM TEST ===\n");

            // Test 1: Perfect Martini with Level 1 Gin
            TestCocktail("Martini", new Dictionary<string, float>
            {
                { "Gin", 50f },
                { "Wine", 10f }
            }, "Perfect Martini (Level 1 Gin)");

            // Test 2: Perfect Martini with Level 3 Gin (simulated)
            TestCocktailWithLevel("Martini", new Dictionary<string, float>
            {
                { "Gin", 50f },
                { "Wine", 10f }
            }, "gin", 3, "Perfect Martini (Level 3 Gin)");

            // Test 3: Screwdriver with correct ingredients but wrong ratios
            TestCocktail("Screwdriver", new Dictionary<string, float>
            {
                { "Vodka", 50f },
                { "Juice", 50f }
            }, "Screwdriver (Wrong Ratios)");

            // Test 4: Perfect Cosmopolitan
            TestCocktail("Cosmopolitan", new Dictionary<string, float>
            {
                { "Vodka", 50f },
                { "Cointreau", 25f },
                { "Juice", 25f }
            }, "Perfect Cosmopolitan");

            // Test 5: Wrong ingredients
            TestCocktail("Martini", new Dictionary<string, float>
            {
                { "Vodka", 50f },
                { "Juice", 10f }
            }, "Wrong Ingredients for Martini");

            // Test 6: List all available cocktails
            Debug.Log("\n=== AVAILABLE COCKTAILS ===");
            var allRecipes = CocktailDatabase.GetAllRecipeNames();
            foreach (var recipeName in allRecipes)
            {
                var recipe = CocktailDatabase.GetRecipe(recipeName);
                Debug.Log($"{recipe.name} (Difficulty {recipe.difficulty}): {GetRecipeString(recipe)}");
            }
        }

        private void TestCocktail(string recipeName, Dictionary<string, float> contents, string testName)
        {
            Debug.Log($"\n--- {testName} ---");
            
            var recipe = CocktailDatabase.GetRecipe(recipeName);
            if (recipe == null)
            {
                Debug.LogError($"Recipe '{recipeName}' not found!");
                return;
            }

            var result = CocktailEvaluator.Evaluate(recipe, contents, liquorDatabase);
            Debug.Log($"Result: {result}");
            Debug.Log($"Correct Ingredients: {result.hasCorrectIngredients}");
            Debug.Log($"Correct Ratios: {result.hasCorrectRatios}");
            Debug.Log($"Coins Earned: {result.coins}");
        }

        private void TestCocktailWithLevel(string recipeName, Dictionary<string, float> contents, string liquorId, int level, string testName)
        {
            Debug.Log($"\n--- {testName} ---");
            
            // Temporarily set liquor level
            var liquor = liquorDatabase?.GetLiquor(liquorId);
            int originalLevel = 1;
            
            if (liquor != null)
            {
                originalLevel = liquor.level;
                liquor.level = level;
            }

            var recipe = CocktailDatabase.GetRecipe(recipeName);
            if (recipe == null)
            {
                Debug.LogError($"Recipe '{recipeName}' not found!");
                return;
            }

            var result = CocktailEvaluator.Evaluate(recipe, contents, liquorDatabase);
            Debug.Log($"Result: {result}");
            Debug.Log($"Level Multiplier: {result.levelMultiplier:F2}x");
            Debug.Log($"Coins Earned: {result.coins}");

            // Restore original level
            if (liquor != null)
            {
                liquor.level = originalLevel;
            }
        }

        private string GetRecipeString(CocktailRecipe recipe)
        {
            string result = "";
            bool first = true;
            
            foreach (var ingredient in recipe.ingredients)
            {
                if (!first) result += ", ";
                result += $"{ingredient.Key} x{ingredient.Value}";
                first = false;
            }
            
            return result;
        }
    }
}
