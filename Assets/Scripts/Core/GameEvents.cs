using BarSimulator.Data;

namespace BarSimulator.Core
{
    /// <summary>
    /// All event payload classes for the EventBus.
    /// Each event is a simple data container (pure C# class).
    /// Events enable decoupled communication between systems.
    /// </summary>

    // ===================================================================
    // CUSTOMER EVENTS
    // ===================================================================

    /// <summary>
    /// Published when a new customer arrives at the bar.
    /// </summary>
    public class CustomerArrivedEvent
    {
        public int CustomerID { get; set; }
        public DrinkRecipe OrderedRecipe { get; set; }
        public float PatienceTime { get; set; }

        public CustomerArrivedEvent(int customerID, DrinkRecipe orderedRecipe, float patienceTime)
        {
            CustomerID = customerID;
            OrderedRecipe = orderedRecipe;
            PatienceTime = patienceTime;
        }
    }

    /// <summary>
    /// Published when a customer's patience timer updates.
    /// </summary>
    public class CustomerPatienceChangedEvent
    {
        public int CustomerID { get; set; }
        public float RemainingPatience { get; set; }
        public float PatiencePercentage { get; set; }

        public CustomerPatienceChangedEvent(int customerID, float remainingPatience, float patiencePercentage)
        {
            CustomerID = customerID;
            RemainingPatience = remainingPatience;
            PatiencePercentage = patiencePercentage;
        }
    }

    /// <summary>
    /// Published when a customer leaves (happy or angry).
    /// </summary>
    public class CustomerLeftEvent
    {
        public int CustomerID { get; set; }
        public bool WasHappy { get; set; }
        public int TipAmount { get; set; }

        public CustomerLeftEvent(int customerID, bool wasHappy, int tipAmount)
        {
            CustomerID = customerID;
            WasHappy = wasHappy;
            TipAmount = tipAmount;
        }
    }

    // ===================================================================
    // CRAFTING / DRINK EVENTS
    // ===================================================================

    /// <summary>
    /// Published when a drink is successfully identified.
    /// </summary>
    public class DrinkIdentifiedEvent
    {
        public DrinkRecipe IdentifiedRecipe { get; set; }
        public float Accuracy { get; set; }

        public DrinkIdentifiedEvent(DrinkRecipe identifiedRecipe, float accuracy)
        {
            IdentifiedRecipe = identifiedRecipe;
            Accuracy = accuracy;
        }
    }

    /// <summary>
    /// Published when a drink requires shaking (triggers QTE minigame).
    /// </summary>
    public class ShakingRequiredEvent
    {
        public DrinkRecipe Recipe { get; set; }
        public float RequiredShakeTime { get; set; }

        public ShakingRequiredEvent(DrinkRecipe recipe, float requiredShakeTime)
        {
            Recipe = recipe;
            RequiredShakeTime = requiredShakeTime;
        }
    }

    /// <summary>
    /// Published when a drink is served to a customer.
    /// </summary>
    public class DrinkServedEvent
    {
        public int CustomerID { get; set; }
        public DrinkRecipe ServedRecipe { get; set; }
        public bool IsCorrectDrink { get; set; }
        public float ServiceTime { get; set; }

        public DrinkServedEvent(int customerID, DrinkRecipe servedRecipe, bool isCorrectDrink, float serviceTime)
        {
            CustomerID = customerID;
            ServedRecipe = servedRecipe;
            IsCorrectDrink = isCorrectDrink;
            ServiceTime = serviceTime;
        }
    }

    // ===================================================================
    // MINIGAME / QTE EVENTS
    // ===================================================================

    /// <summary>
    /// Published when the QTE minigame starts.
    /// </summary>
    public class MinigameStartedEvent
    {
        public string MinigameType { get; set; }
        public float Duration { get; set; }

        public MinigameStartedEvent(string minigameType, float duration)
        {
            MinigameType = minigameType;
            Duration = duration;
        }
    }

    /// <summary>
    /// Published when the player successfully hits a QTE.
    /// </summary>
    public class QTEHitEvent
    {
        public bool IsPerfect { get; set; }
        public int Score { get; set; }
        public int ComboCount { get; set; }

        public QTEHitEvent(bool isPerfect, int score, int comboCount)
        {
            IsPerfect = isPerfect;
            Score = score;
            ComboCount = comboCount;
        }
    }

    /// <summary>
    /// Published when the player misses a QTE.
    /// </summary>
    public class QTEMissEvent
    {
        public int ComboReset { get; set; }

        public QTEMissEvent(int comboReset)
        {
            ComboReset = comboReset;
        }
    }

    /// <summary>
    /// Published when the QTE minigame completes (success or failure).
    /// </summary>
    public class MinigameCompletedEvent
    {
        public bool Success { get; set; }
        public int TotalScore { get; set; }
        public float Accuracy { get; set; }

        public MinigameCompletedEvent(bool success, int totalScore, float accuracy)
        {
            Success = success;
            TotalScore = totalScore;
            Accuracy = accuracy;
        }
    }

    // ===================================================================
    // STATE TRANSITION EVENTS
    // ===================================================================

    /// <summary>
    /// Published when the game state changes.
    /// </summary>
    public class StateChangedEvent
    {
        public string PreviousState { get; set; }
        public string NewState { get; set; }

        public StateChangedEvent(string previousState, string newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    /// <summary>
    /// Request to transition to crafting state.
    /// </summary>
    public class RequestCraftingStateEvent
    {
        // Empty - just a signal
    }

    /// <summary>
    /// Request to transition to serving state.
    /// </summary>
    public class RequestServingStateEvent
    {
        public int TargetCustomerID { get; set; }

        public RequestServingStateEvent(int targetCustomerID)
        {
            TargetCustomerID = targetCustomerID;
        }
    }

    // ===================================================================
    // SCORE / PROGRESSION EVENTS
    // ===================================================================

    /// <summary>
    /// Published when the player's score changes.
    /// </summary>
    public class ScoreChangedEvent
    {
        public int NewScore { get; set; }
        public int ScoreDelta { get; set; }

        public ScoreChangedEvent(int newScore, int scoreDelta)
        {
            NewScore = newScore;
            ScoreDelta = scoreDelta;
        }
    }

    /// <summary>
    /// Published when coins/money changes.
    /// </summary>
    public class CoinsChangedEvent
    {
        public int NewCoins { get; set; }
        public int CoinsDelta { get; set; }

        public CoinsChangedEvent(int newCoins, int coinsDelta)
        {
            NewCoins = newCoins;
            CoinsDelta = coinsDelta;
        }
    }

    // ===================================================================
    // INPUT EVENTS (Optional - for decoupled input handling)
    // ===================================================================

    /// <summary>
    /// Published when the player presses the interaction key (E).
    /// </summary>
    public class InteractionInputEvent
    {
        // Empty - just a signal
    }

    /// <summary>
    /// Published when the player presses the QTE action key (Space).
    /// </summary>
    public class QTEInputEvent
    {
        // Empty - just a signal
    }
}
