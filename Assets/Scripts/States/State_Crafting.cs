using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Data;

namespace BarSimulator.States
{
    /// <summary>
    /// Crafting state - player pours ingredients and prepares drinks.
    /// Listens for shaking requirements and serving requests.
    /// Isolated logic - does NOT directly reference Shaker or Customer.
    /// </summary>
    public class State_Crafting : IGameState
    {
        private float craftingTimer;
        private GameStateManager stateManager;
        private bool drinkIdentified;

        // Parameterless constructor for generic constraint
        public State_Crafting() { }

        public State_Crafting(GameStateManager manager)
        {
            stateManager = manager;
        }

        public void Enter()
        {
            craftingTimer = 0f;
            drinkIdentified = false;

            if (GameData.DebugStateTransitions)
            {
                Debug.Log("[State_Crafting] Player is now crafting drinks...");
            }

            // Subscribe to events
            EventBus.Subscribe<DrinkIdentifiedEvent>(OnDrinkIdentified);
            EventBus.Subscribe<ShakingRequiredEvent>(OnShakingRequired);
            EventBus.Subscribe<RequestServingStateEvent>(OnServingRequested);
        }

        public void Update()
        {
            craftingTimer += Time.deltaTime;

            // Timeout safety - auto-fail if taking too long
            if (craftingTimer >= GameData.MaxCraftingTime)
            {
                Debug.LogWarning("[State_Crafting] Crafting timeout! Forcing idle state.");
                stateManager.TransitionTo<State_Idle>();
            }
        }

        public void Exit()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<DrinkIdentifiedEvent>(OnDrinkIdentified);
            EventBus.Unsubscribe<ShakingRequiredEvent>(OnShakingRequired);
            EventBus.Unsubscribe<RequestServingStateEvent>(OnServingRequested);

            if (GameData.DebugStateTransitions)
            {
                Debug.Log($"[State_Crafting] Exited after {craftingTimer:F1}s");
            }
        }

        private void OnDrinkIdentified(DrinkIdentifiedEvent evt)
        {
            drinkIdentified = true;

            if (GameData.DebugStateTransitions)
            {
                Debug.Log($"[State_Crafting] Drink identified: {evt.IdentifiedRecipe.Name} (Accuracy: {evt.Accuracy:P0})");
            }

            // Check if drink requires shaking
            if (evt.IdentifiedRecipe.RequiresShaking)
            {
                // Transition to Shaking QTE state
                stateManager.TransitionTo<State_Shaking_QTE>();
            }
            // If no shaking required, player can proceed to serving
        }

        private void OnShakingRequired(ShakingRequiredEvent evt)
        {
            // Alternative trigger for shaking (if manually initiated)
            if (GameData.DebugStateTransitions)
            {
                Debug.Log($"[State_Crafting] Shaking required for {evt.Recipe.Name}");
            }

            stateManager.TransitionTo<State_Shaking_QTE>();
        }

        private void OnServingRequested(RequestServingStateEvent evt)
        {
            // Player manually triggered serving
            if (drinkIdentified)
            {
                stateManager.TransitionTo<State_Serving>();
            }
            else
            {
                Debug.LogWarning("[State_Crafting] Cannot serve - no drink identified yet!");
            }
        }
    }
}
