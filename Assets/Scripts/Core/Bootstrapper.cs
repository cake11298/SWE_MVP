using UnityEngine;
using BarSimulator.Systems;
using BarSimulator.States;

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

        [Header("Game Systems")]
        [SerializeField] private CustomerSystem customerSystem;
        [SerializeField] private MinigameSystem minigameSystem;

        [Header("Debug")]
        [SerializeField] private bool verboseLogging = true;

        private void Awake()
        {
            if (verboseLogging)
            {
                Debug.Log("[Bootstrapper] Initializing core systems...");
            }

            // Phase 1: Infrastructure
            // EventBus is static, no initialization needed
            if (verboseLogging)
            {
                Debug.Log("[Bootstrapper] ✓ EventBus ready");
            }

            // Phase 2: Data layer
            // RecipeDatabase and GameData are static, auto-initialize on first access
            var recipeCount = BarSimulator.Data.RecipeDatabase.AllRecipes.Count;
            if (verboseLogging)
            {
                Debug.Log($"[Bootstrapper] ✓ RecipeDatabase ready ({recipeCount} recipes)");
                Debug.Log("[Bootstrapper] ✓ GameData ready");
            }

            // Phase 3: Game systems - Create if not assigned
            InitializeSystems();

            // Phase 4: Start FSM
            InitializeStateMachine();

            if (verboseLogging)
            {
                Debug.Log("[Bootstrapper] ✅ All systems initialized successfully!");
            }
        }

        private void InitializeSystems()
        {
            // Create CustomerSystem if not assigned
            if (customerSystem == null)
            {
                GameObject customerSystemObj = new GameObject("CustomerSystem");
                customerSystemObj.transform.SetParent(transform);
                customerSystem = customerSystemObj.AddComponent<CustomerSystem>();

                if (verboseLogging)
                {
                    Debug.Log("[Bootstrapper] ✓ CustomerSystem created");
                }
            }

            // Create MinigameSystem if not assigned
            if (minigameSystem == null)
            {
                GameObject minigameSystemObj = new GameObject("MinigameSystem");
                minigameSystemObj.transform.SetParent(transform);
                minigameSystem = minigameSystemObj.AddComponent<MinigameSystem>();

                if (verboseLogging)
                {
                    Debug.Log("[Bootstrapper] ✓ MinigameSystem created");
                }
            }
        }

        private void InitializeStateMachine()
        {
            // Create GameStateManager if not assigned
            if (gameStateManager == null)
            {
                GameObject stateManagerObj = new GameObject("GameStateManager");
                stateManagerObj.transform.SetParent(transform);
                gameStateManager = stateManagerObj.AddComponent<GameStateManager>();

                if (verboseLogging)
                {
                    Debug.Log("[Bootstrapper] ✓ GameStateManager created");
                }
            }

            // Register all states with their dependencies
            gameStateManager.RegisterState(new State_Idle(gameStateManager));
            gameStateManager.RegisterState(new State_CustomerEntry(gameStateManager, customerSystem));
            gameStateManager.RegisterState(new State_Crafting(gameStateManager));
            gameStateManager.RegisterState(new State_Shaking_QTE(gameStateManager, minigameSystem));
            gameStateManager.RegisterState(new State_Serving(gameStateManager));

            if (verboseLogging)
            {
                Debug.Log("[Bootstrapper] ✓ All states registered");
            }

            // Start with CustomerEntry state (first customer arrives)
            gameStateManager.TransitionTo<State_CustomerEntry>();

            if (verboseLogging)
            {
                Debug.Log("[Bootstrapper] ✓ FSM started in CustomerEntry state");
            }
        }

        private void OnDestroy()
        {
            // Cleanup: Clear EventBus subscriptions when scene unloads
            EventBus.ClearAll();

            if (verboseLogging)
            {
                Debug.Log("[Bootstrapper] Cleanup complete");
            }
        }
    }
}
