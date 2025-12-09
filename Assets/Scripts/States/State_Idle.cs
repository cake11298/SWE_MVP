using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Data;

namespace BarSimulator.States
{
    /// <summary>
    /// Idle state - waiting for next customer or action.
    /// No active customer, player can prepare or wait.
    /// </summary>
    public class State_Idle : IGameState
    {
        private float idleTimer;
        private GameStateManager stateManager;

        // Parameterless constructor for generic constraint
        public State_Idle() { }

        public State_Idle(GameStateManager manager)
        {
            stateManager = manager;
        }

        public void Enter()
        {
            idleTimer = 0f;

            if (GameData.DebugStateTransitions)
            {
                Debug.Log("[State_Idle] Entered idle state - waiting for customer...");
            }

            // Subscribe to events if needed
            EventBus.Subscribe<RequestCraftingStateEvent>(OnCraftingRequested);
        }

        public void Update()
        {
            idleTimer += Time.deltaTime;

            // Auto-transition to CustomerEntry after minimum idle time
            if (idleTimer >= GameData.MinIdleTime)
            {
                stateManager.TransitionTo<State_CustomerEntry>();
            }
        }

        public void Exit()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<RequestCraftingStateEvent>(OnCraftingRequested);

            if (GameData.DebugStateTransitions)
            {
                Debug.Log($"[State_Idle] Exited after {idleTimer:F1}s");
            }
        }

        private void OnCraftingRequested(RequestCraftingStateEvent evt)
        {
            // Player manually started crafting
            stateManager.TransitionTo<State_Crafting>();
        }
    }
}
