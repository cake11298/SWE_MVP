using UnityEngine;
using BarSimulator.Objects;
using BarSimulator.Core;

namespace BarSimulator.NPC
{
    /// <summary>
    /// Simple NPC serving system - Press F near NPC with a non-empty glass to serve and earn coins
    /// </summary>
    public class SimpleNPCServe : MonoBehaviour
    {
        [Header("Serve Settings")]
        [Tooltip("Coins earned per serve")]
        [SerializeField] private int coinsPerServe = 200;

        [Tooltip("Interaction distance")]
        [SerializeField] private float interactionDistance = 3f;

        [Header("References")]
        [Tooltip("Player transform (auto-detected if null)")]
        [SerializeField] private Transform player;

        // Private state
        private bool playerNearby = false;
        private GlassContainer heldGlass = null;

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
                            Debug.Log($"SimpleNPCServe: Glass is empty! Cannot serve to {gameObject.name}.");
                        }
                    }
                    else
                    {
                        Debug.Log($"SimpleNPCServe: You need to be holding the glass to serve to {gameObject.name}.");
                    }
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

            // Clear the glass
            heldGlass.Clear();

            // Add coins to GameManager
            if (GameManager.Instance != null)
            {
                var score = GameManager.Instance.GetScore();
                score.totalCoins += coinsPerServe;
                
                Debug.Log($"SimpleNPCServe: Served drink ({drinkContents}, {totalVolume:F0}ml) to {gameObject.name}. Earned {coinsPerServe} coins! Total coins: {score.totalCoins}");
                
                // Note: OnCoinsUpdated event can only be invoked from within GameManager
                // We'll just log the coin update here
            }
            else
            {
                Debug.LogWarning("SimpleNPCServe: GameManager not found! Cannot add coins.");
            }

            // Play feedback (optional - can add sound/animation here)
            Debug.Log($"SimpleNPCServe: {gameObject.name} says: Thanks for the drink!");
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
