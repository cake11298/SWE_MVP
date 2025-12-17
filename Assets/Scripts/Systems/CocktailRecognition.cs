using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BarSimulator.Data;

namespace BarSimulator.Systems
{
    [System.Serializable]
    public class CocktailRecipe
    {
        public string id;
        public string displayName;
        public Dictionary<string, float> ingredients = new Dictionary<string, float>(); // ID -> Ratio (parts)
        public bool requiresShaking;
        public string garnish;
        public string glassType;

        public CocktailRecipe(string id, string name, bool shake = false, string glass = "Glass")
        {
            this.id = id;
            this.displayName = name;
            this.requiresShaking = shake;
            this.glassType = glass;
        }

        public void AddIngredient(string liquidId, float parts)
        {
            if (ingredients.ContainsKey(liquidId))
                ingredients[liquidId] = parts;
            else
                ingredients.Add(liquidId, parts);
        }
    }

    public class CocktailRecognition : MonoBehaviour
    {
        private static CocktailRecognition instance;
        public static CocktailRecognition Instance => instance;

        private List<CocktailRecipe> recipes = new List<CocktailRecipe>();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            InitializeRecipes();
        }

        private void InitializeRecipes()
        {
            // 1. Martini (Gin + Vermouth)
            var martini = new CocktailRecipe("martini", "Martini", false, "MartiniGlass");
            martini.AddIngredient("gin", 6f);
            martini.AddIngredient("vermouth", 1f);
            recipes.Add(martini);

            // 2. Margarita (Tequila + Cointreau + Juice)
            var margarita = new CocktailRecipe("margarita", "Margarita", true, "MargaritaGlass");
            margarita.AddIngredient("tequila", 4f);
            margarita.AddIngredient("cointreau", 2f);
            margarita.AddIngredient("juice", 1.5f);
            recipes.Add(margarita);

            // 3. Cosmopolitan (Vodka + Cointreau + Cranberry + Juice)
            var cosmo = new CocktailRecipe("cosmopolitan", "Cosmopolitan", true, "MartiniGlass");
            cosmo.AddIngredient("vodka", 4f);
            cosmo.AddIngredient("cointreau", 1.5f);
            cosmo.AddIngredient("cranberry_juice", 1.5f);
            cosmo.AddIngredient("juice", 0.5f);
            recipes.Add(cosmo);

            // 4. Negroni (Gin + Campari + Vermouth)
            var negroni = new CocktailRecipe("negroni", "Negroni", false, "RocksGlass");
            negroni.AddIngredient("gin", 1f);
            negroni.AddIngredient("campari", 1f);
            negroni.AddIngredient("vermouth", 1f);
            recipes.Add(negroni);

            // 5. Daiquiri (Rum + Juice + Syrup)
            var daiquiri = new CocktailRecipe("daiquiri", "Daiquiri", true, "CoupeGlass");
            daiquiri.AddIngredient("rum", 6f);
            daiquiri.AddIngredient("juice", 2f);
            daiquiri.AddIngredient("syrup", 1f);
            recipes.Add(daiquiri);

            // 6. Whiskey Sour (Whiskey + Juice + Syrup)
            var whiskeySour = new CocktailRecipe("whiskey_sour", "Whiskey Sour", true, "RocksGlass");
            whiskeySour.AddIngredient("whiskey", 4.5f);
            whiskeySour.AddIngredient("juice", 3f);
            whiskeySour.AddIngredient("syrup", 1.5f);
            recipes.Add(whiskeySour);

            // 7. Manhattan (Whiskey + Vermouth + Bitters)
            var manhattan = new CocktailRecipe("manhattan", "Manhattan", false, "CoupeGlass");
            manhattan.AddIngredient("whiskey", 5f);
            manhattan.AddIngredient("vermouth", 2f);
            // Bitters usually ignored in ratio or treated as small amount
            recipes.Add(manhattan);

            // 8. Old Fashioned (Whiskey + Syrup + Bitters)
            var oldFashioned = new CocktailRecipe("old_fashioned", "Old Fashioned", false, "RocksGlass");
            oldFashioned.AddIngredient("whiskey", 6f);
            oldFashioned.AddIngredient("syrup", 0.5f);
            recipes.Add(oldFashioned);

            // 9. Mojito (Rum + Juice + Syrup)
            var mojito = new CocktailRecipe("mojito", "Mojito", false, "HighballGlass");
            mojito.AddIngredient("rum", 4f);
            mojito.AddIngredient("juice", 2f);
            mojito.AddIngredient("syrup", 1.5f);
            recipes.Add(mojito);

            // 10. Gin Tonic (Gin + Tonic)
            var ginTonic = new CocktailRecipe("gin_tonic", "Gin & Tonic", false, "HighballGlass");
            ginTonic.AddIngredient("gin", 1f);
            ginTonic.AddIngredient("tonic", 3f);
            recipes.Add(ginTonic);

            // 11. Cuba Libre (Rum + Cola + Juice)
            var cubaLibre = new CocktailRecipe("cuba_libre", "Cuba Libre", false, "HighballGlass");
            cubaLibre.AddIngredient("rum", 1f);
            cubaLibre.AddIngredient("cola", 2.5f);
            cubaLibre.AddIngredient("juice", 0.1f);
            recipes.Add(cubaLibre);

            // 12. Tequila Sunrise (Tequila + Orange Juice + Grenadine)
            var tequilaSunrise = new CocktailRecipe("tequila_sunrise", "Tequila Sunrise", false, "HighballGlass");
            tequilaSunrise.AddIngredient("tequila", 3f);
            tequilaSunrise.AddIngredient("orange_juice", 6f);
            tequilaSunrise.AddIngredient("grenadine", 0.5f);
            recipes.Add(tequilaSunrise);

            // 13. Long Island Iced Tea (Vodka+Rum+Gin+Tequila+Cointreau+Juice+Cola)
            var longIsland = new CocktailRecipe("long_island", "Long Island Iced Tea", true, "HighballGlass");
            longIsland.AddIngredient("vodka", 1f);
            longIsland.AddIngredient("rum", 1f);
            longIsland.AddIngredient("gin", 1f);
            longIsland.AddIngredient("tequila", 1f);
            longIsland.AddIngredient("cointreau", 1f);
            longIsland.AddIngredient("juice", 1f);
            longIsland.AddIngredient("cola", 2f);
            recipes.Add(longIsland);

            // 14. White Russian (Vodka + Coffee Liqueur + Cream)
            var whiteRussian = new CocktailRecipe("white_russian", "White Russian", false, "RocksGlass");
            whiteRussian.AddIngredient("vodka", 2f);
            whiteRussian.AddIngredient("coffee_liqueur", 1f);
            // Assuming cream is implemented or approximated
            recipes.Add(whiteRussian);

            // 15. Black Russian (Vodka + Coffee Liqueur)
            var blackRussian = new CocktailRecipe("black_russian", "Black Russian", false, "RocksGlass");
            blackRussian.AddIngredient("vodka", 2f);
            blackRussian.AddIngredient("coffee_liqueur", 1f);
            recipes.Add(blackRussian);

            // 16. Screwdriver (Vodka + Orange Juice)
            var screwdriver = new CocktailRecipe("screwdriver", "Screwdriver", false, "HighballGlass");
            screwdriver.AddIngredient("vodka", 1f);
            screwdriver.AddIngredient("orange_juice", 2f);
            recipes.Add(screwdriver);

            // 17. Gimlet (Gin + Juice + Syrup)
            var gimlet = new CocktailRecipe("gimlet", "Gimlet", true, "CoupeGlass");
            gimlet.AddIngredient("gin", 4f);
            gimlet.AddIngredient("juice", 1f);
            gimlet.AddIngredient("syrup", 1f);
            recipes.Add(gimlet);

            // 18. Tom Collins (Gin + Juice + Syrup)
            var tomCollins = new CocktailRecipe("tom_collins", "Tom Collins", true, "HighballGlass");
            tomCollins.AddIngredient("gin", 3f);
            tomCollins.AddIngredient("juice", 2f);
            tomCollins.AddIngredient("syrup", 1f);
            recipes.Add(tomCollins);

            // 19. Pina Colada (Rum + Pineapple + Coconut)
            var pinaColada = new CocktailRecipe("pina_colada", "Pina Colada", true, "HurricaneGlass");
            pinaColada.AddIngredient("rum", 2f);
            pinaColada.AddIngredient("pineapple_juice", 3f);
            pinaColada.AddIngredient("coconut_cream", 1f);
            recipes.Add(pinaColada);

            // 20. Mai Tai (Rum + Juice + Cointreau + Syrup)
            var maiTai = new CocktailRecipe("mai_tai", "Mai Tai", true, "RocksGlass");
            maiTai.AddIngredient("rum", 3f);
            maiTai.AddIngredient("juice", 1f);
            maiTai.AddIngredient("cointreau", 0.5f);
            maiTai.AddIngredient("syrup", 0.5f);
            recipes.Add(maiTai);

            // 21. Sidecar (Brandy + Cointreau + Juice)
            var sidecar = new CocktailRecipe("sidecar", "Sidecar", true, "CoupeGlass");
            sidecar.AddIngredient("brandy", 4f);
            sidecar.AddIngredient("cointreau", 2f);
            sidecar.AddIngredient("juice", 1.5f);
            recipes.Add(sidecar);

            // 22. Americano (Campari + Vermouth + Syrup)
            var americano = new CocktailRecipe("americano", "Americano", false, "HighballGlass");
            americano.AddIngredient("campari", 1f);
            americano.AddIngredient("vermouth", 1f);
            americano.AddIngredient("syrup", 2f);
            recipes.Add(americano);

            // 23. Boulevardier (Whiskey + Campari + Vermouth)
            var boulevardier = new CocktailRecipe("boulevardier", "Boulevardier", false, "RocksGlass");
            boulevardier.AddIngredient("whiskey", 1.5f);
            boulevardier.AddIngredient("campari", 1f);
            boulevardier.AddIngredient("vermouth", 1f);
            recipes.Add(boulevardier);

            // 24. Kamikaze (Vodka + Cointreau + Juice)
            var kamikaze = new CocktailRecipe("kamikaze", "Kamikaze", true, "ShotGlass");
            kamikaze.AddIngredient("vodka", 1f);
            kamikaze.AddIngredient("cointreau", 1f);
            kamikaze.AddIngredient("juice", 1f);
            recipes.Add(kamikaze);

            // 25. Lemon Drop (Vodka + Juice + Syrup)
            var lemonDrop = new CocktailRecipe("lemon_drop", "Lemon Drop", true, "MartiniGlass");
            lemonDrop.AddIngredient("vodka", 4f);
            lemonDrop.AddIngredient("juice", 1.5f);
            lemonDrop.AddIngredient("syrup", 1f);
            recipes.Add(lemonDrop);
        }

        public RecognitionResult Recognize(Dictionary<string, float> currentIngredients, bool isShaken)
        {
            if (currentIngredients == null || currentIngredients.Count == 0)
                return new RecognitionResult { name = "Empty", isPerfect = false };

            // 標準化成分名稱
            currentIngredients = BarSimulator.Core.LiquorNameMapper.NormalizeIngredients(currentIngredients);

            float totalVolume = currentIngredients.Values.Sum();
            if (totalVolume < 10f) return new RecognitionResult { name = "Trace Liquid", isPerfect = false };

            CocktailRecipe bestMatch = null;
            float bestScore = 0f;
            bool isPerfect = false;

            foreach (var recipe in recipes)
            {
                float score = CalculateMatchScore(recipe, currentIngredients, totalVolume);
                
                // Check shaking requirement
                if (recipe.requiresShaking && !isShaken) score *= 0.8f; // Penalty for not shaking

                if (score > bestScore && score > 0.7f) // Threshold
                {
                    bestScore = score;
                    bestMatch = recipe;
                    isPerfect = score > 0.95f;
                }
            }

            if (bestMatch != null)
            {
                string displayName = bestMatch.displayName;
                if (isPerfect) displayName += " ✨";
                
                return new RecognitionResult 
                { 
                    name = displayName, 
                    recipeId = bestMatch.id,
                    isPerfect = isPerfect,
                    score = bestScore
                };
            }

            // Fallback naming
            string dominant = currentIngredients.OrderByDescending(x => x.Value).First().Key;
            return new RecognitionResult { name = $"Mixed {dominant}", isPerfect = false };
        }

        private float CalculateMatchScore(CocktailRecipe recipe, Dictionary<string, float> currentIngredients, float totalVolume)
        {
            // 1. Check if all required ingredients are present
            foreach (var req in recipe.ingredients)
            {
                if (!currentIngredients.ContainsKey(req.Key)) return 0f;
            }

            // 2. Calculate ratio difference
            float totalError = 0f;
            float recipeTotalParts = recipe.ingredients.Values.Sum();

            foreach (var req in recipe.ingredients)
            {
                float targetRatio = req.Value / recipeTotalParts;
                float currentRatio = currentIngredients[req.Key] / totalVolume;
                
                totalError += Mathf.Abs(targetRatio - currentRatio);
            }

            // Penalize extra ingredients
            foreach (var ing in currentIngredients)
            {
                if (!recipe.ingredients.ContainsKey(ing.Key))
                {
                    totalError += (ing.Value / totalVolume) * 2f; // Heavy penalty for wrong ingredients
                }
            }

            return Mathf.Clamp01(1f - totalError);
        }
    }

    public struct RecognitionResult
    {
        public string name;
        public string recipeId;
        public bool isPerfect;
        public float score;
    }
}
