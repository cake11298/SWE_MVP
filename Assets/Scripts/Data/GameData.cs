using UnityEngine;

namespace BarSimulator.Data
{
    /// <summary>
    /// Centralized game configuration and constants.
    /// Pure C# static class - all hardcoded values for game systems.
    /// This replaces scattered magic numbers throughout the codebase.
    /// </summary>
    public static class GameData
    {
        // ===================================================================
        // CUSTOMER / NPC PARAMETERS
        // ===================================================================

        /// <summary>Base patience timer for customers (seconds).</summary>
        public const float BaseCustomerPatience = 60f;

        /// <summary>Patience reduction per difficulty level (seconds).</summary>
        public const float PatienceReductionPerDifficulty = 10f;

        /// <summary>Time between customer arrivals (seconds).</summary>
        public const float CustomerSpawnInterval = 15f;

        /// <summary>Maximum number of customers at the bar simultaneously.</summary>
        public const int MaxSimultaneousCustomers = 3;

        /// <summary>Base tip amount for a perfectly served drink (coins).</summary>
        public const int BaseTipAmount = 50;

        /// <summary>Tip multiplier for fast service (under 30s).</summary>
        public const float FastServiceTipMultiplier = 1.5f;

        /// <summary>Customer satisfaction threshold for a "happy" rating (0-1).</summary>
        public const float HappyThreshold = 0.8f;

        /// <summary>Customer satisfaction threshold for a "neutral" rating (0-1).</summary>
        public const float NeutralThreshold = 0.5f;

        // ===================================================================
        // MINIGAME / QTE PARAMETERS
        // ===================================================================

        /// <summary>Shaking QTE pointer rotation speed (degrees/second).</summary>
        public const float QTEPointerSpeed = 180f;

        /// <summary>Size of the success hit zone (degrees).</summary>
        public const float QTEHitZoneSize = 30f;

        /// <summary>Size of the "perfect" hit zone (degrees).</summary>
        public const float QTEPerfectZoneSize = 10f;

        /// <summary>Number of successful hits required to complete shaking.</summary>
        public const int QTERequiredHits = 3;

        /// <summary>Time window to hit the QTE button (seconds).</summary>
        public const float QTEInputWindow = 0.2f;

        /// <summary>Penalty time added for missing a QTE (seconds).</summary>
        public const float QTEMissPenalty = 1f;

        /// <summary>Score bonus for hitting the perfect zone.</summary>
        public const int QTEPerfectBonus = 100;

        /// <summary>Score for hitting the success zone.</summary>
        public const int QTESuccessScore = 50;

        // ===================================================================
        // CRAFTING / COCKTAIL PARAMETERS
        // ===================================================================

        /// <summary>Pour speed (ml/second) - from Constants.cs.</summary>
        public const float PourRate = 30f;

        /// <summary>Time to fully tilt a bottle for pouring (seconds).</summary>
        public const float BottleTiltDuration = 0.3f;

        /// <summary>Ingredient accuracy tolerance for recipe matching (ml).</summary>
        public const float IngredientAccuracyTolerance = 5f;

        /// <summary>Perfect drink score multiplier.</summary>
        public const float PerfectDrinkScoreMultiplier = 2f;

        /// <summary>Good drink score multiplier.</summary>
        public const float GoodDrinkScoreMultiplier = 1.5f;

        /// <summary>Acceptable drink score multiplier.</summary>
        public const float AcceptableDrinkScoreMultiplier = 1f;

        // ===================================================================
        // STATE MACHINE PARAMETERS
        // ===================================================================

        /// <summary>Minimum time in Idle state before allowing customer entry (seconds).</summary>
        public const float MinIdleTime = 2f;

        /// <summary>Time for customer entry animation (seconds).</summary>
        public const float CustomerEntryDuration = 3f;

        /// <summary>Maximum time allowed in Crafting state before auto-serving (seconds).</summary>
        public const float MaxCraftingTime = 120f;

        /// <summary>Time for serving animation/delivery (seconds).</summary>
        public const float ServingDuration = 2f;

        // ===================================================================
        // SCORING / PROGRESSION PARAMETERS
        // ===================================================================

        /// <summary>Starting coins/money.</summary>
        public const int StartingCoins = 500;

        /// <summary>Score required for 1-star rating.</summary>
        public const int ScoreFor1Star = 100;

        /// <summary>Score required for 2-star rating.</summary>
        public const int ScoreFor2Stars = 500;

        /// <summary>Score required for 3-star rating.</summary>
        public const int ScoreFor3Stars = 1000;

        /// <summary>Penalty for serving wrong drink (coins).</summary>
        public const int WrongDrinkPenalty = -25;

        /// <summary>Penalty for customer leaving angry (score).</summary>
        public const int AngryCustomerPenalty = -50;

        // ===================================================================
        // AUDIO PARAMETERS
        // ===================================================================

        /// <summary>Default music volume (0-1).</summary>
        public const float DefaultMusicVolume = 0.5f;

        /// <summary>Default SFX volume (0-1).</summary>
        public const float DefaultSFXVolume = 0.7f;

        /// <summary>Default ambient sound volume (0-1).</summary>
        public const float DefaultAmbientVolume = 0.3f;

        // ===================================================================
        // UI PARAMETERS
        // ===================================================================

        /// <summary>UI fade duration (seconds).</summary>
        public const float UIFadeDuration = 0.3f;

        /// <summary>Notification display time (seconds).</summary>
        public const float NotificationDuration = 3f;

        /// <summary>Patience bar update frequency (seconds).</summary>
        public const float PatienceBarUpdateRate = 0.1f;

        // ===================================================================
        // DEBUG / DEVELOPMENT
        // ===================================================================

        /// <summary>Enable verbose logging for state transitions.</summary>
        public const bool DebugStateTransitions = true;

        /// <summary>Enable verbose logging for events.</summary>
        public const bool DebugEventBus = false;

        /// <summary>Enable on-screen QTE debug display.</summary>
        public const bool DebugQTE = true;

        // ===================================================================
        // HELPER METHODS
        // ===================================================================

        /// <summary>
        /// Calculate customer patience based on drink difficulty.
        /// </summary>
        public static float GetCustomerPatience(int difficultyLevel)
        {
            return Mathf.Max(20f, BaseCustomerPatience - (difficultyLevel * PatienceReductionPerDifficulty));
        }

        /// <summary>
        /// Calculate tip amount based on service quality and speed.
        /// </summary>
        public static int CalculateTip(float satisfactionRating, float serviceTime)
        {
            float tipAmount = BaseTipAmount * satisfactionRating;

            // Fast service bonus
            if (serviceTime < 30f)
            {
                tipAmount *= FastServiceTipMultiplier;
            }

            return Mathf.RoundToInt(tipAmount);
        }

        /// <summary>
        /// Get score multiplier based on drink accuracy.
        /// </summary>
        public static float GetScoreMultiplier(float accuracy)
        {
            if (accuracy >= 0.95f)
                return PerfectDrinkScoreMultiplier;
            else if (accuracy >= 0.8f)
                return GoodDrinkScoreMultiplier;
            else if (accuracy >= 0.6f)
                return AcceptableDrinkScoreMultiplier;
            else
                return 0.5f; // Poor drink
        }

        /// <summary>
        /// Convert satisfaction rating to mood string.
        /// </summary>
        public static string GetMoodFromSatisfaction(float satisfaction)
        {
            if (satisfaction >= HappyThreshold)
                return "Happy";
            else if (satisfaction >= NeutralThreshold)
                return "Neutral";
            else
                return "Angry";
        }
    }
}
