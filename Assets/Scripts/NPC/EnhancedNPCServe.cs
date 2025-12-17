using UnityEngine;
using BarSimulator.Objects;
using BarSimulator.Core;
using BarSimulator.UI;
using BarSimulator.Data;

namespace BarSimulator.NPC
{
    /// <summary>
    /// Enhanced NPC serving system with cocktail recipes and scoring
    /// NPCs order cocktails from the CocktailDatabase
    /// Evaluates drinks based on ingredients and ratios
    /// Provides level-based bonuses for higher quality spirits
    /// </summary>
    public class EnhancedNPCServe : MonoBehaviour
    {
        [Header("Serve Settings")]
        [Tooltip("Interaction distance")]
        [SerializeField] private float interactionDistance = 3f;

        [Tooltip("Cooldown between orders (seconds)")]
        [SerializeField] private float orderCooldown = 60f;

        [Header("References")]
        [Tooltip("Player transform (auto-detected if null)")]
        [SerializeField] private Transform player;

        [Tooltip("Liquor Database for level bonuses")]
        [SerializeField] private LiquorDatabase liquorDatabase;

        // Private state
        private bool playerNearby = false;
        private GlassContainer heldGlass = null;
        private CocktailRecipe currentOrder = null;
        private NPCOrdersUI ordersUI;
        private GameStatsUI statsUI;
        private float nextOrderTime = 0f;
        private bool hasActiveOrder = false;

        private void Start()
        {
            // Auto-detect player
            if (player == null)
            {
                var playerObj = GameObject.Find("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
            }

            // Load liquor database if not assigned
            if (liquorDatabase == null)
            {
                liquorDatabase = Resources.Load<LiquorDatabase>("LiquorDataBase");
                if (liquorDatabase == null)
                {
                    Debug.LogWarning($"EnhancedNPCServe ({gameObject.name}): Could not load LiquorDatabase from Resources");
                }
            }

            // Find UI references
            ordersUI = FindFirstObjectByType<NPCOrdersUI>();
            statsUI = FindFirstObjectByType<GameStatsUI>();

            // Generate first order immediately
            GenerateRandomOrder();
        }

        private void Update()
        {
            CheckPlayerDistance();
            HandleServeInput();
            CheckOrderCooldown();
        }

        /// <summary>
        /// Check if player is within interaction distance
        /// </summary>
        private void CheckPlayerDistance()
        {
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);
            playerNearby = distance <= interactionDistance;
        }

        /// <summary>
        /// Check if it's time to generate a new order
        /// </summary>
        private void CheckOrderCooldown()
        {
            if (!hasActiveOrder && Time.time >= nextOrderTime)
            {
                GenerateRandomOrder();
            }
        }

        /// <summary>
        /// Handle F key input for serving drinks
        /// </summary>
        private void HandleServeInput()
        {
            if (!playerNearby || !hasActiveOrder) return;

            // Check if F key is pressed
            if (Input.GetKeyDown(KeyCode.F))
            {
                // Find ServeGlass in the scene
                var serveGlass = GameObject.Find("ServeGlass");
                if (serveGlass != null)
                {
                    // Check if it's being held by checking if it's near the player
                    float distanceToGlass = Vector3.Distance(serveGlass.transform.position, player.position);
                    
                    // If glass is close to player (within 2 units), assume it's being held
                    if (distanceToGlass < 2f)
                    {
                        // Try to get GlassContainer component
                        heldGlass = serveGlass.GetComponent<GlassContainer>();
                        
                        if (heldGlass != null && !heldGlass.IsEmpty())
                        {
                            // Serve the drink
                            ServeDrink();
                        }
                        else
                        {
                            Debug.Log($"EnhancedNPCServe: Glass is empty! Cannot serve to {gameObject.name}.");
                        }
                    }
                    else
                    {
                        Debug.Log($"EnhancedNPCServe: You need to be holding the glass to serve to {gameObject.name}.");
                    }
                }
            }
        }

        /// <summary>
        /// Serve the drink to NPC - evaluate and give coins based on quality
        /// </summary>
        private void ServeDrink()
        {
            if (heldGlass == null || currentOrder == null) return;

            // Get drink contents
            var glassContents = heldGlass.GetLiquidContents();
            string drinkContentsStr = heldGlass.GetContentsString();

            // Evaluate the drink
            EvaluationResult evaluation = CocktailEvaluator.Evaluate(currentOrder, glassContents, liquorDatabase);

            // Clear the glass
            heldGlass.Clear();

            // Give coins based on evaluation (always give at least something)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(evaluation.coins);
            }
            else if (statsUI != null)
            {
                statsUI.AddMoney(evaluation.coins);
            }

            Debug.Log($"EnhancedNPCServe: {gameObject.name} - {evaluation.feedback} Earned {evaluation.coins} coins!");

            // Show feedback to player
            ShowFeedback(evaluation);

            // Remove order from UI
            if (ordersUI != null)
            {
                ordersUI.RemoveNPCOrder(gameObject.name);
            }

            // Mark order as completed and set cooldown
            hasActiveOrder = false;
            nextOrderTime = Time.time + orderCooldown;

            Debug.Log($"EnhancedNPCServe: {gameObject.name} will order again in {orderCooldown} seconds");
        }

        /// <summary>
        /// Show feedback to the player
        /// </summary>
        private void ShowFeedback(EvaluationResult evaluation)
        {
            string message = $"{gameObject.name}: {evaluation.feedback}";
            
            if (evaluation.hasCorrectIngredients && evaluation.hasCorrectRatios)
            {
                message += $"\n+{evaluation.coins} coins!";
            }
            else if (evaluation.hasCorrectIngredients)
            {
                message += $"\n+{evaluation.coins} coins (ratios off)";
            }
            else
            {
                message += $"\n+{evaluation.coins} coins (wrong drink)";
            }

            Debug.Log(message);
        }

        /// <summary>
        /// Generate a random cocktail order for this NPC
        /// </summary>
        private void GenerateRandomOrder()
        {
            currentOrder = CocktailDatabase.GetRandomRecipe();
            
            if (currentOrder == null)
            {
                Debug.LogWarning($"EnhancedNPCServe: Failed to generate order for {gameObject.name}");
                return;
            }

            hasActiveOrder = true;

            // Build order string
            string orderString = BuildOrderString(currentOrder);

            // Update UI
            if (ordersUI != null)
            {
                ordersUI.SetNPCOrder(gameObject.name, orderString);
            }

            Debug.Log($"EnhancedNPCServe: {gameObject.name} wants {currentOrder.name}");
        }

        /// <summary>
        /// Build a readable order string from recipe
        /// </summary>
        private string BuildOrderString(CocktailRecipe recipe)
        {
            string orderStr = recipe.name + ": ";
            bool first = true;

            foreach (var ingredient in recipe.ingredients)
            {
                if (!first)
                    orderStr += ", ";
                
                orderStr += $"{ingredient.Key} x{ingredient.Value}";
                first = false;
            }

            return orderStr;
        }

        /// <summary>
        /// Get current order
        /// </summary>
        public CocktailRecipe GetCurrentOrder()
        {
            return currentOrder;
        }

        /// <summary>
        /// Check if NPC has an active order
        /// </summary>
        public bool HasActiveOrder()
        {
            return hasActiveOrder;
        }

        /// <summary>
        /// Get time until next order
        /// </summary>
        public float GetTimeUntilNextOrder()
        {
            return Mathf.Max(0f, nextOrderTime - Time.time);
        }

        /// <summary>
        /// Draw interaction range in editor
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}
