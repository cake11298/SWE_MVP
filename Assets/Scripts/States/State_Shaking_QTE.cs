using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Data;
using BarSimulator.Systems;

namespace BarSimulator.States
{
    /// <summary>
    /// Shaking QTE state - DbD-style skill check minigame.
    /// Calls MinigameSystem.Update() in its Update() loop.
    /// CRITICAL: This state does NOT know about Customer - it only knows about MinigameSystem.
    /// Communication happens via EventBus (MinigameCompletedEvent).
    /// </summary>
    public class State_Shaking_QTE : IGameState
    {
        private GameStateManager stateManager;
        private MinigameSystem minigameSystem;
        private bool minigameStarted;

        // Parameterless constructor for generic constraint
        public State_Shaking_QTE() { }

        public State_Shaking_QTE(GameStateManager manager, MinigameSystem minigame)
        {
            stateManager = manager;
            minigameSystem = minigame;
        }

        public void Enter()
        {
            minigameStarted = false;

            if (GameData.DebugStateTransitions)
            {
                Debug.Log("[State_Shaking_QTE] Entering shaking minigame...");
            }

            // Subscribe to minigame completion
            EventBus.Subscribe<MinigameCompletedEvent>(OnMinigameCompleted);

            // Initialize and start the minigame
            StartMinigame();
        }

        public void Update()
        {
            if (!minigameStarted)
                return;

            // Call MinigameSystem's update logic
            // Note: MinigameSystem also has its own Update() from MonoBehaviour,
            // but we call it here for explicit control in the state
            if (minigameSystem != null && minigameSystem.IsActive)
            {
                // MinigameSystem handles its own Update() via MonoBehaviour
                // We just check completion here
            }
        }

        public void Exit()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<MinigameCompletedEvent>(OnMinigameCompleted);

            // Stop minigame if still active
            if (minigameSystem != null && minigameSystem.IsActive)
            {
                minigameSystem.StopMinigame();
            }

            if (GameData.DebugStateTransitions)
            {
                Debug.Log("[State_Shaking_QTE] Exited shaking state.");
            }
        }

        private void StartMinigame()
        {
            if (minigameSystem == null)
            {
                Debug.LogError("[State_Shaking_QTE] MinigameSystem not found! Aborting.");
                stateManager.TransitionTo<State_Crafting>();
                return;
            }

            // Initialize with parameters from GameData
            minigameSystem.Initialize(15f, 2); // 15 seconds, medium difficulty

            // Start the minigame
            minigameSystem.StartMinigame();
            minigameStarted = true;

            if (GameData.DebugStateTransitions)
            {
                Debug.Log("[State_Shaking_QTE] Minigame started!");
            }
        }

        private void OnMinigameCompleted(MinigameCompletedEvent evt)
        {
            if (GameData.DebugStateTransitions)
            {
                Debug.Log($"[State_Shaking_QTE] Minigame completed! Success: {evt.Success}, Score: {evt.TotalScore}");
            }

            // Transition back to Crafting state (or directly to Serving)
            // The CustomerSystem will react to the MinigameCompletedEvent independently
            if (evt.Success)
            {
                // Success - can now serve the drink
                stateManager.TransitionTo<State_Serving>();
            }
            else
            {
                // Failed - return to crafting
                Debug.LogWarning("[State_Shaking_QTE] Minigame failed! Returning to crafting.");
                stateManager.TransitionTo<State_Crafting>();
            }
        }
    }
}
