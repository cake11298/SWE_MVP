using UnityEngine;
using BarSimulator.Objects;
using BarSimulator.Core;
using BarSimulator.UI;

namespace BarSimulator.NPC
{
    /// <summary>
    /// Simple NPC serving system - Press F near NPC with a non-empty glass to serve and earn coins
    /// NPCs randomly order drinks: Neat Gin, Neat Whiskey, or Neat Cointreau
    /// </summary>
    public class SimpleNPCServe : MonoBehaviour
    {
        [Header("Serve Settings")]
        [Tooltip("Coins earned per serve")]
        [SerializeField] private int coinsPerServe = 200;

        [Tooltip("Default coins for incorrect drinks")]
        [SerializeField] private int defaultCoins = 20;

        [Tooltip("Interaction distance")]
        [SerializeField] private float interactionDistance = 3f;

        [Header("References")]
        [Tooltip("Player transform (auto-detected if null)")]
        [SerializeField] private Transform player;

        [Header("Order Settings")]
        [Tooltip("Available drinks to order")]
        [SerializeField] private string[] availableDrinks = { "Neat Gin", "Neat Whiskey", "Neat Cointreau" };

        // Private state
        private bool playerNearby = false;
        private GlassContainer heldGlass = null;
        private string currentOrder = "";
        private NPCOrdersUI ordersUI;
        private GameStatsUI statsUI;

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

            // Find UI references
            ordersUI = FindFirstObjectByType<NPCOrdersUI>();
            statsUI = FindFirstObjectByType<GameStatsUI>();

            // Generate random order for this NPC
            GenerateRandomOrder();
        }

        private void Update()
        {
            CheckPlayerDistance();
            HandleServeInput();
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
        /// Handle F key input for serving drinks
        /// </summary>
        private void HandleServeInput()
        {
            if (!playerNearby) return;

            // Check if F key is pressed
            if (Input.GetKeyDown(KeyCode.F))
            {
                // Get held object from player's interaction system
                var interactionSystem = player.GetComponent<BarSimulator.Player.ImprovedInteractionSystem>();
                if (interactionSystem != null && interactionSystem.HeldObject != null)
                {
                    var heldObj = interactionSystem.HeldObject;
                    heldGlass = heldObj.GetComponent<GlassContainer>();
                    
                    if (heldGlass != null)
                    {
                        if (!heldGlass.IsEmpty())
                        {
                            // Serve the drink
                            ServeDrink();
                        }
                        else
                        {
                            Debug.Log($"SimpleNPCServe: Glass is empty! Cannot serve to {gameObject.name}.");
                        }
                    }
                    else
                    {
                        Debug.Log($"SimpleNPCServe: You need to be holding a glass to serve to {gameObject.name}.");
                    }
                }
                else
                {
                    Debug.Log($"SimpleNPCServe: You need to be holding a glass to serve to {gameObject.name}.");
                }
            }
        }

        /// <summary>
        /// Serve the drink to NPC - clear glass and give coins
        /// </summary>
        private void ServeDrink()
        {
            if (heldGlass == null) return;

            // Get drink info before clearing
            string drinkContents = heldGlass.GetContentsString();
            float totalVolume = heldGlass.currentTotalVolume;

            // Check if drink matches order (simple check based on contents)
            bool isCorrectDrink = CheckIfDrinkMatchesOrder(drinkContents);
            int coinsToGive = isCorrectDrink ? coinsPerServe : defaultCoins;

            // Clear the glass
            heldGlass.Clear();

            // Add coins to GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(coinsToGive);
                
                string feedback = isCorrectDrink 
                    ? $"Perfect! Earned {coinsToGive} coins!" 
                    : $"This isn't what I ordered, but I'll take it. Earned {coinsToGive} coins.";
                
                Debug.Log($"SimpleNPCServe: Served drink ({drinkContents}, {totalVolume:F0}ml) to {gameObject.name}. {feedback}");
            }
            else
            {
                Debug.LogWarning("SimpleNPCServe: GameManager not found! Cannot add coins.");
                
                // Update GameStatsUI directly as backup
                if (statsUI != null)
                {
                    statsUI.AddMoney(coinsToGive);
                }
            }

            // Remove order from UI
            if (ordersUI != null)
            {
                ordersUI.RemoveNPCOrder(gameObject.name);
            }

            // Play feedback (optional - can add sound/animation here)
            string npcMessage = isCorrectDrink 
                ? "Thanks for the drink!" 
                : "This isn't what I ordered, but okay...";
            Debug.Log($"SimpleNPCServe: {gameObject.name} says: {npcMessage}");

            // Generate new order after a delay
            Invoke(nameof(GenerateRandomOrder), 2f);
        }

        /// <summary>
        /// Check if the drink matches the current order
        /// </summary>
        private bool CheckIfDrinkMatchesOrder(string drinkContents)
        {
            if (string.IsNullOrEmpty(currentOrder) || string.IsNullOrEmpty(drinkContents))
                return false;

            // Simple check: if order contains "Gin" and drink contains "gin", it's a match
            string orderLower = currentOrder.ToLower();
            string contentsLower = drinkContents.ToLower();

            // Extract the main ingredient from the order (e.g., "Neat Gin" -> "gin")
            if (orderLower.Contains("gin") && contentsLower.Contains("gin"))
                return true;
            if (orderLower.Contains("whiskey") && contentsLower.Contains("whiskey"))
                return true;
            if (orderLower.Contains("cointreau") && contentsLower.Contains("cointreau"))
                return true;

            return false;
        }

        /// <summary>
        /// Generate a random drink order for this NPC
        /// </summary>
        private void GenerateRandomOrder()
        {
            if (availableDrinks.Length == 0) return;

            // Pick random drink
            currentOrder = availableDrinks[Random.Range(0, availableDrinks.Length)];

            // Update UI
            if (ordersUI != null)
            {
                ordersUI.SetNPCOrder(gameObject.name, currentOrder);
            }

            Debug.Log($"SimpleNPCServe: {gameObject.name} wants {currentOrder}");
        }

        /// <summary>
        /// Get current order
        /// </summary>
        public string GetCurrentOrder()
        {
            return currentOrder;
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
