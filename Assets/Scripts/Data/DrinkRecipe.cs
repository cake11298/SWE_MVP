using System.Collections.Generic;

namespace BarSimulator.Data
{
    /// <summary>
    /// Pure C# class representing a cocktail recipe.
    /// No MonoBehaviour - just data.
    /// </summary>
    public class DrinkRecipe
    {
        /// <summary>
        /// Display name of the cocktail (e.g., "Martini").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of ingredient IDs required for this recipe.
        /// IDs should match LiquorData.liquorID from the existing system.
        /// </summary>
        public List<string> Ingredients { get; set; }

        /// <summary>
        /// Required shake duration in seconds (0 if shaking not required).
        /// </summary>
        public float ShakeTime { get; set; }

        /// <summary>
        /// Whether this drink requires shaking (triggers QTE minigame).
        /// </summary>
        public bool RequiresShaking { get; set; }

        /// <summary>
        /// Difficulty rating (1-5). Affects NPC patience and tip amount.
        /// </summary>
        public int DifficultyLevel { get; set; }

        /// <summary>
        /// Preferred glass type (e.g., "Cocktail Glass", "Highball", "Rocks Glass").
        /// </summary>
        public string PreferredGlassType { get; set; }

        /// <summary>
        /// Optional garnish name (e.g., "Olive", "Lime Wedge", "Orange Peel").
        /// </summary>
        public string Garnish { get; set; }

        /// <summary>
        /// Constructor for easy recipe creation.
        /// </summary>
        public DrinkRecipe(
            string name,
            List<string> ingredients,
            float shakeTime = 0f,
            bool requiresShaking = false,
            int difficultyLevel = 1,
            string preferredGlassType = "Cocktail Glass",
            string garnish = null)
        {
            Name = name;
            Ingredients = ingredients ?? new List<string>();
            ShakeTime = shakeTime;
            RequiresShaking = requiresShaking;
            DifficultyLevel = difficultyLevel;
            PreferredGlassType = preferredGlassType;
            Garnish = garnish;
        }

        /// <summary>
        /// Check if this recipe matches a list of ingredient IDs (order-independent).
        /// </summary>
        public bool MatchesIngredients(List<string> providedIngredients)
        {
            if (providedIngredients == null || providedIngredients.Count != Ingredients.Count)
            {
                return false;
            }

            // Create copies to avoid modifying originals
            List<string> recipeIngredients = new List<string>(Ingredients);
            List<string> provided = new List<string>(providedIngredients);

            // Sort both lists for comparison
            recipeIngredients.Sort();
            provided.Sort();

            // Compare element by element
            for (int i = 0; i < recipeIngredients.Count; i++)
            {
                if (recipeIngredients[i] != provided[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return $"{Name} (Difficulty: {DifficultyLevel}, Ingredients: {string.Join(", ", Ingredients)})";
        }
    }
}
