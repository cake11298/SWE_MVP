using System.Collections.Generic;
using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Data;

namespace BarSimulator.Systems
{
    /// <summary>
    /// Manages all customer/NPC logic including spawning, orders, patience, and reactions.
    /// Uses EventBus for decoupled communication - does NOT know about MinigameSystem.
    /// Listens to MinigameCompletedEvent to react to shaking success/failure.
    /// </summary>
    public class CustomerSystem : MonoBehaviour
    {
        // ===================================================================
        // Customer Data
        // ===================================================================

        public class CustomerData
        {
            public int ID;
            public DrinkRecipe OrderedRecipe;
            public float MaxPatience;
            public float RemainingPatience;
            public float OrderTime;
            public bool IsWaiting;
        }

        private List<CustomerData> activeCustomers = new List<CustomerData>();
        private int nextCustomerID = 1;

        // ===================================================================
        // Lifecycle
        // ===================================================================

        private void OnEnable()
        {
            // Subscribe to events
            EventBus.Subscribe<MinigameCompletedEvent>(OnMinigameCompleted);
            EventBus.Subscribe<DrinkServedEvent>(OnDrinkServed);
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<MinigameCompletedEvent>(OnMinigameCompleted);
            EventBus.Unsubscribe<DrinkServedEvent>(OnDrinkServed);
        }

        private void Update()
        {
            UpdateCustomerPatience(Time.deltaTime);
        }

        // ===================================================================
        // Public API
        // ===================================================================

        /// <summary>
        /// Spawn a new customer with a random order.
        /// </summary>
        public void SpawnCustomer()
        {
            if (activeCustomers.Count >= GameData.MaxSimultaneousCustomers)
            {
                Debug.LogWarning("[CustomerSystem] Max customers reached, cannot spawn more.");
                return;
            }

            // Get random recipe
            DrinkRecipe recipe = RecipeDatabase.GetRandomRecipe(maxDifficulty: 3);
            if (recipe == null)
            {
                Debug.LogError("[CustomerSystem] No recipes available!");
                return;
            }

            // Calculate patience based on difficulty
            float patience = GameData.GetCustomerPatience(recipe.DifficultyLevel);

            // Create customer data
            CustomerData customer = new CustomerData
            {
                ID = nextCustomerID++,
                OrderedRecipe = recipe,
                MaxPatience = patience,
                RemainingPatience = patience,
                OrderTime = Time.time,
                IsWaiting = true
            };

            activeCustomers.Add(customer);

            // Publish event
            EventBus.Publish(new CustomerArrivedEvent(customer.ID, recipe, patience));

            Debug.Log($"[CustomerSystem] Customer #{customer.ID} arrived, ordered {recipe.Name}, patience: {patience:F0}s");
        }

        /// <summary>
        /// Get the current waiting customer (for serving).
        /// </summary>
        public CustomerData GetWaitingCustomer()
        {
            return activeCustomers.Find(c => c.IsWaiting);
        }

        /// <summary>
        /// Remove a customer (after serving or timeout).
        /// </summary>
        public void RemoveCustomer(int customerID)
        {
            activeCustomers.RemoveAll(c => c.ID == customerID);
        }

        // ===================================================================
        // Private Methods
        // ===================================================================

        private void UpdateCustomerPatience(float deltaTime)
        {
            for (int i = activeCustomers.Count - 1; i >= 0; i--)
            {
                CustomerData customer = activeCustomers[i];

                if (!customer.IsWaiting)
                    continue;

                // Decrease patience
                customer.RemainingPatience -= deltaTime;

                // Calculate percentage
                float percentage = customer.RemainingPatience / customer.MaxPatience;

                // Publish patience update event
                EventBus.Publish(new CustomerPatienceChangedEvent(
                    customer.ID,
                    customer.RemainingPatience,
                    percentage
                ));

                // Check if patience ran out
                if (customer.RemainingPatience <= 0f)
                {
                    HandleCustomerTimeout(customer);
                    activeCustomers.RemoveAt(i);
                }
            }
        }

        private void HandleCustomerTimeout(CustomerData customer)
        {
            Debug.Log($"[CustomerSystem] Customer #{customer.ID} left angry (timeout)!");

            // Publish event
            EventBus.Publish(new CustomerLeftEvent(
                customerID: customer.ID,
                wasHappy: false,
                tipAmount: 0
            ));

            // Publish score penalty
            EventBus.Publish(new ScoreChangedEvent(
                newScore: 0, // Will be handled by score system
                scoreDelta: GameData.AngryCustomerPenalty
            ));
        }

        // ===================================================================
        // Event Handlers
        // ===================================================================

        private void OnMinigameCompleted(MinigameCompletedEvent evt)
        {
            // React to minigame result
            if (evt.Success)
            {
                Debug.Log($"[CustomerSystem] Minigame succeeded! Customer should be impressed.");
                // Could add bonus satisfaction here
            }
            else
            {
                Debug.Log($"[CustomerSystem] Minigame failed. Customer might be disappointed.");
                // Could reduce satisfaction here
            }
        }

        private void OnDrinkServed(DrinkServedEvent evt)
        {
            // Find the customer
            CustomerData customer = activeCustomers.Find(c => c.ID == evt.CustomerID);
            if (customer == null)
            {
                Debug.LogWarning($"[CustomerSystem] Customer #{evt.CustomerID} not found!");
                return;
            }

            customer.IsWaiting = false;

            // Calculate satisfaction
            float satisfaction = CalculateSatisfaction(customer, evt);

            // Calculate tip
            int tip = GameData.CalculateTip(satisfaction, evt.ServiceTime);

            // Determine mood
            string mood = GameData.GetMoodFromSatisfaction(satisfaction);
            bool wasHappy = satisfaction >= GameData.NeutralThreshold;

            Debug.Log($"[CustomerSystem] Customer #{customer.ID} received drink. Mood: {mood}, Tip: ${tip}");

            // Publish events
            EventBus.Publish(new CustomerLeftEvent(customer.ID, wasHappy, tip));

            if (tip > 0)
            {
                EventBus.Publish(new CoinsChangedEvent(0, tip)); // Will be handled by economy system
            }

            // Score based on satisfaction
            int scoreGain = Mathf.RoundToInt(GameData.BaseTipAmount * satisfaction);
            EventBus.Publish(new ScoreChangedEvent(0, scoreGain));

            // Remove customer after delay (handled by State_Serving)
        }

        private float CalculateSatisfaction(CustomerData customer, DrinkServedEvent evt)
        {
            float satisfaction = 0.5f; // Base satisfaction

            // Check if correct drink
            if (!evt.IsCorrectDrink)
            {
                satisfaction = 0.2f; // Wrong drink
                return satisfaction;
            }

            // Correct drink bonus
            satisfaction = 0.8f;

            // Speed bonus (served quickly)
            float patiencePercentage = customer.RemainingPatience / customer.MaxPatience;
            if (patiencePercentage > 0.7f)
            {
                satisfaction += 0.2f; // Fast service
            }
            else if (patiencePercentage < 0.3f)
            {
                satisfaction -= 0.1f; // Slow service
            }

            return Mathf.Clamp01(satisfaction);
        }

        // ===================================================================
        // Debug
        // ===================================================================

        private void OnGUI()
        {
            if (!GameData.DebugStateTransitions)
                return;

            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 200));
            GUILayout.Box("=== CUSTOMER SYSTEM ===");
            GUILayout.Label($"Active Customers: {activeCustomers.Count}/{GameData.MaxSimultaneousCustomers}");

            foreach (var customer in activeCustomers)
            {
                GUILayout.Label($"ID: {customer.ID}, Order: {customer.OrderedRecipe.Name}, Patience: {customer.RemainingPatience:F0}s");
            }

            GUILayout.EndArea();
        }
    }
}
