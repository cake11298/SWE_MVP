using UnityEngine;
using System.Collections.Generic;
using System.Text;
using BarSimulator.Data;

namespace BarSimulator.Objects
{
    /// <summary>
    /// Shaker container that can receive liquids and pour them out.
    /// Cannot be served to NPCs directly.
    /// Can only pour if it contains liquid.
    /// </summary>
    public class ShakerContainer : MonoBehaviour
    {
        // Reference to LiquorDatabase
        private static LiquorDatabase liquorDatabase;
        
        [Header("Shaker Properties")]
        [Tooltip("Maximum capacity in ml")]
        public float maxShakerVolume = 800f;

        [Header("Current Contents")]
        [Tooltip("Current total volume in ml")]
        public float currentTotalVolume = 0f;

        [Header("Pouring Settings")]
        [Tooltip("Pour rate in ml per second")]
        public float pourRate = 3.5f;

        // Dictionary to track liquid contents: liquidName -> amount (ml)
        private Dictionary<string, float> liquidContents = new Dictionary<string, float>();

        /// <summary>
        /// Add liquid to the shaker with accumulation logic.
        /// If the liquid already exists, increment the amount.
        /// If not, add a new entry.
        /// </summary>
        public float AddLiquid(string liquidName, float amount)
        {
            // Check if shaker is full
            if (currentTotalVolume >= maxShakerVolume)
            {
                return 0f; // Can't add more
            }

            // Calculate how much we can actually add
            float spaceRemaining = maxShakerVolume - currentTotalVolume;
            float actualAmount = Mathf.Min(amount, spaceRemaining);

            // Add or accumulate in dictionary
            if (liquidContents.ContainsKey(liquidName))
            {
                // Accumulate - increment existing value
                liquidContents[liquidName] += actualAmount;
            }
            else
            {
                // Add new entry
                liquidContents[liquidName] = actualAmount;
            }

            // Update total volume
            currentTotalVolume += actualAmount;

            Debug.Log($"ShakerContainer: Added {actualAmount:F0}ml of {liquidName}. Total: {currentTotalVolume:F0}ml");

            return actualAmount;
        }

        /// <summary>
        /// Pour liquid from shaker to target container.
        /// Returns the actual amount poured.
        /// </summary>
        public float PourLiquid(float amount)
        {
            if (IsEmpty())
            {
                return 0f; // Can't pour from empty shaker
            }

            // Calculate how much we can actually pour
            float actualAmount = Mathf.Min(amount, currentTotalVolume);

            // Calculate the ratio to pour from each liquid
            float ratio = actualAmount / currentTotalVolume;

            // Store the amounts to pour (we'll modify the dictionary after iteration)
            Dictionary<string, float> amountsToPour = new Dictionary<string, float>();
            
            foreach (var kvp in liquidContents)
            {
                float pourFromThis = kvp.Value * ratio;
                amountsToPour[kvp.Key] = pourFromThis;
            }

            // Update the shaker contents
            List<string> keysToRemove = new List<string>();
            foreach (var kvp in amountsToPour)
            {
                liquidContents[kvp.Key] -= kvp.Value;
                
                // Remove if amount is negligible
                if (liquidContents[kvp.Key] <= 0.01f)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            // Remove empty entries
            foreach (var key in keysToRemove)
            {
                liquidContents.Remove(key);
            }

            // Update total volume
            currentTotalVolume -= actualAmount;
            currentTotalVolume = Mathf.Max(0f, currentTotalVolume);

            return actualAmount;
        }

        /// <summary>
        /// Pour liquid from shaker to a GlassContainer.
        /// Transfers the liquid contents proportionally.
        /// </summary>
        public float PourToGlass(GlassContainer targetGlass, float amount)
        {
            if (IsEmpty() || targetGlass == null || targetGlass.IsFull())
            {
                return 0f;
            }

            // Calculate how much we can actually pour
            float actualAmount = Mathf.Min(amount, currentTotalVolume);
            float spaceInGlass = targetGlass.GetRemainingSpace();
            actualAmount = Mathf.Min(actualAmount, spaceInGlass);

            // Calculate the ratio to pour from each liquid
            float ratio = actualAmount / currentTotalVolume;

            // Pour each liquid proportionally to the glass
            List<string> keysToRemove = new List<string>();
            foreach (var kvp in liquidContents)
            {
                float pourFromThis = kvp.Value * ratio;
                
                // Add to target glass
                targetGlass.AddLiquid(kvp.Key, pourFromThis);
                
                // Reduce from shaker
                liquidContents[kvp.Key] -= pourFromThis;
                
                // Mark for removal if negligible
                if (liquidContents[kvp.Key] <= 0.01f)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            // Remove empty entries
            foreach (var key in keysToRemove)
            {
                liquidContents.Remove(key);
            }

            // Update total volume
            currentTotalVolume -= actualAmount;
            currentTotalVolume = Mathf.Max(0f, currentTotalVolume);

            Debug.Log($"ShakerContainer: Poured {actualAmount:F0}ml to glass. Remaining: {currentTotalVolume:F0}ml");

            return actualAmount;
        }

        /// <summary>
        /// Check if the shaker can pour (has liquid).
        /// </summary>
        public bool CanPour()
        {
            return currentTotalVolume > 0f;
        }

        /// <summary>
        /// Check if the shaker is full.
        /// </summary>
        public bool IsFull()
        {
            return currentTotalVolume >= maxShakerVolume;
        }

        /// <summary>
        /// Check if the shaker is empty.
        /// </summary>
        public bool IsEmpty()
        {
            return currentTotalVolume <= 0f;
        }

        /// <summary>
        /// Get the fill ratio (0-1).
        /// </summary>
        public float GetFillRatio()
        {
            return currentTotalVolume / maxShakerVolume;
        }

        /// <summary>
        /// Get the remaining space in ml.
        /// </summary>
        public float GetRemainingSpace()
        {
            return maxShakerVolume - currentTotalVolume;
        }

        /// <summary>
        /// Get a formatted string of the contents.
        /// Format: "Gin 50ml, Whiskey 20ml"
        /// </summary>
        public string GetContentsString()
        {
            if (liquidContents.Count == 0)
            {
                return "Empty";
            }

            // Load database if not already loaded
            if (liquorDatabase == null)
            {
                liquorDatabase = Resources.Load<LiquorDatabase>("LiquorDataBase");
            }

            StringBuilder sb = new StringBuilder();
            bool first = true;

            foreach (var kvp in liquidContents)
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                // Try to get display name from database
                string displayName = kvp.Key;
                if (liquorDatabase != null)
                {
                    var liquorData = liquorDatabase.GetLiquor(kvp.Key.ToLower());
                    if (liquorData != null)
                    {
                        displayName = liquorData.displayName;
                    }
                }

                sb.Append($"{displayName} {kvp.Value:F0}ml");
                first = false;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get the dictionary of liquid contents (read-only access).
        /// </summary>
        public Dictionary<string, float> GetLiquidContents()
        {
            return new Dictionary<string, float>(liquidContents);
        }

        /// <summary>
        /// Clear all contents.
        /// </summary>
        public void Clear()
        {
            liquidContents.Clear();
            currentTotalVolume = 0f;
            Debug.Log("ShakerContainer: Cleared all contents");
        }

        /// <summary>
        /// Debug display in editor.
        /// </summary>
        private void OnValidate()
        {
            // Clamp values in editor
            currentTotalVolume = Mathf.Max(0f, currentTotalVolume);
            maxShakerVolume = Mathf.Max(1f, maxShakerVolume);
        }
    }
}
