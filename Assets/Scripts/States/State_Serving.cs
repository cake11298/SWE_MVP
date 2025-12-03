using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Data;

namespace BarSimulator.States
{
    /// <summary>
    /// Serving state - deliver the finished drink to the customer.
    /// Handles customer satisfaction and transitions back to Idle for next customer.
    /// </summary>
    public class State_Serving : IGameState
    {
        private float servingTimer;
        private GameStateManager stateManager;
        private bool drinkServed;

        // Parameterless constructor for generic constraint
        public State_Serving() { }

        public State_Serving(GameStateManager manager)
        {
            stateManager = manager;
        }

        public void Enter()
        {
            servingTimer = 0f;
            drinkServed = false;

            if (GameData.DebugStateTransitions)
            {
                Debug.Log("[State_Serving] Delivering drink to customer...");
            }

            // Subscribe to events
            EventBus.Subscribe<CustomerLeftEvent>(OnCustomerLeft);

            // Auto-trigger serving (in real game, this would be player-initiated)
            ServeCurrentDrink();
        }

        public void Update()
        {
            servingTimer += Time.deltaTime;

            // Auto-transition after serving animation completes
            if (drinkServed && servingTimer >= GameData.ServingDuration)
            {
                // Return to Idle state to wait for next customer
                stateManager.TransitionTo<State_Idle>();
            }
        }

        public void Exit()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<CustomerLeftEvent>(OnCustomerLeft);

            if (GameData.DebugStateTransitions)
            {
                Debug.Log("[State_Serving] Serving complete, returning to idle.");
            }
        }

        private void ServeCurrentDrink()
        {
            // In a real implementation, this would:
            // 1. Get the currently held drink from InteractionSystem
            // 2. Identify the drink recipe
            // 3. Get the target customer
            // 4. Publish DrinkServedEvent

            // PLACEHOLDER: For now, we'll simulate serving
            // This should be replaced with actual game logic integration

            // Example: Get recipe from some current drink state
            DrinkRecipe servedRecipe = RecipeDatabase.GetRandomRecipe(); // TEMP: should get actual crafted drink

            // Publish serve event (CustomerSystem will handle the rest)
            EventBus.Publish(new DrinkServedEvent(
                customerID: 1, // TEMP: should get actual customer ID
                servedRecipe: servedRecipe,
                isCorrectDrink: true, // TEMP: should check against customer order
                serviceTime: 30f // TEMP: should calculate actual service time
            ));

            drinkServed = true;

            if (GameData.DebugStateTransitions)
            {
                Debug.Log($"[State_Serving] Served {servedRecipe.Name} to customer");
            }
        }

        private void OnCustomerLeft(CustomerLeftEvent evt)
        {
            if (GameData.DebugStateTransitions)
            {
                Debug.Log($"[State_Serving] Customer #{evt.CustomerID} left. Happy: {evt.WasHappy}, Tip: ${evt.TipAmount}");
            }

            // Customer has left, we can transition back to idle
            drinkServed = true;
        }
    }
}
