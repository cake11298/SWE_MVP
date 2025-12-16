using UnityEngine;
using System.Collections.Generic;
using System.Text;
using BarSimulator.Data;

namespace BarSimulator.Objects
{
    /// <summary>
    /// Glass container that tracks liquid contents using a Dictionary.
    /// Supports accumulation of multiple liquid types.
    /// </summary>
    public class GlassContainer : MonoBehaviour
    {
        // Reference to LiquorDatabase
        private static LiquorDatabase liquorDatabase;
        [Header("Glass Properties")]
        [Tooltip("Maximum capacity in ml")]
        public float maxGlassVolume = 300f;

        [Header("Current Contents")]
        [Tooltip("Current total volume in ml")]
        public float currentTotalVolume = 0f;

        // Dictionary to track liquid contents: liquidName -> amount (ml)
        private Dictionary<string, float> liquidContents = new Dictionary<string, float>();

        /// <summary>
        /// Add liquid to the glass with accumulation logic.
        /// If the liquid already exists, increment the amount.
        /// If not, add a new entry.
        /// </summary>
        public float AddLiquid(string liquidName, float amount)
        {
            // Check if glass is full
            if (currentTotalVolume >= maxGlassVolume)
            {
                return 0f; // Can't add more
            }

            // Calculate how much we can actually add
            float spaceRemaining = maxGlassVolume - currentTotalVolume;
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

            return actualAmount;
        }

        /// <summary>
        /// Check if the glass is full.
        /// </summary>
        public bool IsFull()
        {
            return currentTotalVolume >= maxGlassVolume;
        }

        /// <summary>
        /// Check if the glass is empty.
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
            return currentTotalVolume / maxGlassVolume;
        }

        /// <summary>
        /// Get the remaining space in ml.
        /// </summary>
        public float GetRemainingSpace()
        {
            return maxGlassVolume - currentTotalVolume;
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
        }

        /// <summary>
        /// Debug display in editor.
        /// </summary>
        private void OnValidate()
        {
            // Clamp values in editor
            currentTotalVolume = Mathf.Max(0f, currentTotalVolume);
            maxGlassVolume = Mathf.Max(1f, maxGlassVolume);
        }
    }
}
