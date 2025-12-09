using System;
using System.Collections.Generic;
using UnityEngine;

namespace BarSimulator.Core
{
    /// <summary>
    /// Finite State Machine coordinator for the game loop.
    /// Manages state transitions and ensures only one state is active at a time.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        private IGameState currentState;
        private Dictionary<Type, IGameState> stateCache = new Dictionary<Type, IGameState>();

        /// <summary>
        /// Get the currently active state (for debugging).
        /// </summary>
        public IGameState CurrentState => currentState;

        /// <summary>
        /// Register a state instance for reuse (optional, for performance).
        /// </summary>
        /// <typeparam name="T">State type</typeparam>
        /// <param name="stateInstance">Pre-instantiated state object</param>
        public void RegisterState<T>(T stateInstance) where T : IGameState
        {
            Type stateType = typeof(T);
            if (!stateCache.ContainsKey(stateType))
            {
                stateCache[stateType] = stateInstance;
            }
            else
            {
                Debug.LogWarning($"State {stateType.Name} is already registered. Overwriting.");
                stateCache[stateType] = stateInstance;
            }
        }

        /// <summary>
        /// Transition to a new state. Creates the state if not cached.
        /// </summary>
        /// <typeparam name="T">State type to transition to</typeparam>
        public void TransitionTo<T>() where T : IGameState, new()
        {
            Type stateType = typeof(T);

            // Exit current state
            if (currentState != null)
            {
                Debug.Log($"[FSM] Exiting state: {currentState.GetType().Name}");
                currentState.Exit();
            }

            // Get or create new state
            if (!stateCache.ContainsKey(stateType))
            {
                stateCache[stateType] = new T();
            }

            currentState = stateCache[stateType];

            // Enter new state
            Debug.Log($"[FSM] Entering state: {currentState.GetType().Name}");
            currentState.Enter();
        }

        /// <summary>
        /// Transition to a new state using a pre-instantiated object (for states with dependencies).
        /// </summary>
        /// <param name="newState">State instance to transition to</param>
        public void TransitionTo(IGameState newState)
        {
            if (newState == null)
            {
                Debug.LogError("[FSM] Cannot transition to null state.");
                return;
            }

            // Exit current state
            if (currentState != null)
            {
                Debug.Log($"[FSM] Exiting state: {currentState.GetType().Name}");
                currentState.Exit();
            }

            currentState = newState;

            // Enter new state
            Debug.Log($"[FSM] Entering state: {currentState.GetType().Name}");
            currentState.Enter();
        }

        private void Update()
        {
            // Update the current state every frame
            currentState?.Update();
        }

        private void OnDestroy()
        {
            // Cleanup: exit current state when manager is destroyed
            if (currentState != null)
            {
                currentState.Exit();
                currentState = null;
            }

            stateCache.Clear();
        }
    }
}
