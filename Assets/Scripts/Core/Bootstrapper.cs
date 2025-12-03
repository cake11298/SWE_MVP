using UnityEngine;
using BarSimulator.Systems;
using BarSimulator.States;

namespace BarSimulator.Core
{
    /// <summary>
    /// Main entry point for the game. Initializes all core systems in the correct order.
    /// Works with EXISTING scenes (GameScene.unity with purchased assets).
    /// This MonoBehaviour should be the FIRST script to run in the scene.
    /// Set Script Execution Order to -100 or attach to a persistent GameObject.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Scene Integration")]
        [SerializeField] private SceneIntegrator sceneIntegrator;
        [SerializeField] private bool integrateExistingScene = true;

        [Header("Core Systems")]
        [SerializeField] private GameStateManager gameStateManager;

        [Header("Game Systems")]
        [SerializeField] private CustomerSystem customerSystem;
        [SerializeField] private MinigameSystem minigameSystem;

        [Header("Player Setup")]
        [SerializeField] private bool spawnPlayerIfMissing = true;
        [SerializeField] private GameObject playerPrefab;

        [Header("Debug")]
        [SerializeField] private bool verboseLogging = true;

        private void Awake()
        {
            if (verboseLogging)
            {
                Debug.Log("[Bootstrapper] Initializing for existing GameScene...");
            }

            // Phase 0: Scene Integration (NEW - work with existing assets)
            if (integrateExistingScene)
            {
                InitializeSceneIntegration();
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

            // Phase 3: Player Setup
            EnsurePlayerExists();

            // Phase 4: Game systems - Create if not assigned
            InitializeSystems();

            // Phase 5: Start FSM (optional - can be disabled for testing)
            // InitializeStateMachine();

            if (verboseLogging)
            {
                Debug.Log("[Bootstrapper] ✅ All systems initialized successfully!");
                Debug.Log("[Bootstrapper] Scene is ready to play!");
            }
        }

        private void InitializeSceneIntegration()
        {
            // Find or create SceneIntegrator
            if (sceneIntegrator == null)
            {
                sceneIntegrator = FindObjectOfType<SceneIntegrator>();

                if (sceneIntegrator == null)
                {
                    GameObject integratorObj = new GameObject("SceneIntegrator");
                    integratorObj.transform.SetParent(transform);
                    sceneIntegrator = integratorObj.AddComponent<SceneIntegrator>();
                }
            }

            // Initialize scene integration
            sceneIntegrator.Initialize();

            if (verboseLogging)
            {
                Debug.Log("[Bootstrapper] ✓ Scene integration complete");
            }
        }

        private void EnsurePlayerExists()
        {
            // Check if player already exists in scene
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

            if (existingPlayer != null)
            {
                if (verboseLogging)
                {
                    Debug.Log($"[Bootstrapper] ✓ Found existing player: {existingPlayer.name}");
                }

                // Ensure player has PlayerInteraction component
                var interaction = existingPlayer.GetComponentInChildren<BarSimulator.Player.PlayerInteraction>();
                if (interaction == null)
                {
                    // Try to add to camera if it exists
                    Camera playerCamera = existingPlayer.GetComponentInChildren<Camera>();
                    if (playerCamera != null)
                    {
                        playerCamera.gameObject.AddComponent<BarSimulator.Player.PlayerInteraction>();
                        if (verboseLogging)
                        {
                            Debug.Log("[Bootstrapper] ✓ Added PlayerInteraction to existing player camera");
                        }
                    }
                }
                return;
            }

            // No player found - spawn if enabled
            if (spawnPlayerIfMissing && playerPrefab != null)
            {
                Vector3 spawnPosition = Vector3.zero;

                // Try to spawn near the bar counter
                if (sceneIntegrator != null)
                {
                    spawnPosition = sceneIntegrator.GetBarCounterPosition() + Vector3.back * 2f;
                }

                GameObject newPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                newPlayer.name = "Player";

                if (verboseLogging)
                {
                    Debug.Log($"[Bootstrapper] ✓ Spawned player at {spawnPosition}");
                }
            }
            else if (spawnPlayerIfMissing)
            {
                Debug.LogWarning("[Bootstrapper] Player not found and playerPrefab not assigned!");
                Debug.LogWarning("[Bootstrapper] Please assign a Player prefab or place a Player in the scene.");
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
