using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

namespace BarSimulator.UI
{
    /// <summary>
    /// Displays NPC orders in the format: [NPC01: Neat Gin, NPC02: Neat Whiskey]
    /// </summary>
    public class NPCOrdersUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text ordersText;

        [Header("Order Settings")]
        [SerializeField] private float updateInterval = 0.5f;

        // Track NPC orders
        private Dictionary<string, string> npcOrders = new Dictionary<string, string>();
        private float lastUpdateTime;

        private void Start()
        {
            UpdateUI();
        }

        private void Update()
        {
            // Update UI periodically
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateUI();
                lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// Update the orders display
        /// </summary>
        private void UpdateUI()
        {
            if (ordersText == null) return;

            // Build the orders string
            if (npcOrders.Count == 0)
            {
                ordersText.text = "No active orders";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                bool first = true;
                
                foreach (var order in npcOrders)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    sb.Append($"{order.Key}: {order.Value}");
                    first = false;
                }
                
                ordersText.text = $"[{sb}]";
            }
        }

        /// <summary>
        /// Add or update an NPC order
        /// </summary>
        public void SetNPCOrder(string npcName, string orderDescription)
        {
            npcOrders[npcName] = orderDescription;
            UpdateUI();
        }

        /// <summary>
        /// Remove an NPC order (when served)
        /// </summary>
        public void RemoveNPCOrder(string npcName)
        {
            if (npcOrders.ContainsKey(npcName))
            {
                npcOrders.Remove(npcName);
                UpdateUI();
            }
        }

        /// <summary>
        /// Clear all orders
        /// </summary>
        public void ClearAllOrders()
        {
            npcOrders.Clear();
            UpdateUI();
        }

        /// <summary>
        /// Get current orders dictionary
        /// </summary>
        public Dictionary<string, string> GetOrders()
        {
            return new Dictionary<string, string>(npcOrders);
        }
    }
}
