using UnityEngine;

namespace BarSimulator.Core
{
    /// <summary>
    /// Main entry point for the game. Initializes all core systems in the correct order.
    /// This MonoBehaviour should be the FIRST script to run in the scene.
    /// Set Script Execution Order to -100 or attach to a persistent GameObject.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private GameStateManager gameStateManager;

        [Header("Debug")]
        [SerializeField] private bool verboseLogging = true;

        private void Awake()
        {
            if (verboseLogging)
            {
                Debug.Log("[Bootstrapper] Initializing core systems...");
            }

            // Phase 1: Infrastructure is ready
            // EventBus is static, no initialization needed

            // Phase 2: Data layer (will be populated in Phase 2)
            // RecipeDatabase and GameData are static, no initialization needed

            // Phase 3: Game systems (will be initialized in Phase 3)
            // TODO: Initialize CustomerSystem
            // TODO: Initialize MinigameSystem

            // Phase 4: Start FSM (will be implemented in Phase 4)
            // TODO: Set initial state (e.g., State_CustomerEntry)

            if (verboseLogging)
            {
                Debug.Log("[Bootstrapper] Core systems initialized successfully.");
            }
        }

        private void OnDestroy()
        {
            // Cleanup: Clear EventBus subscriptions when scene unloads
            EventBus.ClearAll();
        }
    }
}
