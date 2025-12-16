using UnityEngine;
using BarSimulator.UI;

namespace BarSimulator.Player
{
    /// <summary>
    /// Example integration: Shows how to add UI prompts to the ItemInteractionSystem.
    /// This is a reference implementation showing where to add UIPromptManager.Show() calls.
    /// 
    /// To integrate into your existing ItemInteractionSystem:
    /// 1. Add "using BarSimulator.UI;" at the top
    /// 2. Call UIPromptManager.Show("message") when picking up items
    /// 3. Call UIPromptManager.Show("message") for other game events
    /// </summary>
    public class ItemInteractionWithPrompts : MonoBehaviour
    {
        // Example: When picking up an item
        private void PickUpItem(InteractableItem item)
        {
            // Your existing pickup code here...
            
            // Show prompt based on item type
            string promptMessage = GetPickupMessage(item);
            UIPromptManager.Show(promptMessage);
        }
        
        // Example: When dropping an item
        private void DropItem(InteractableItem item)
        {
            // Your existing drop code here...
            
            // Show prompt
            UIPromptManager.Show($"Dropped {item.itemName}");
        }
        
        // Example: When pouring liquid
        private void PourLiquid(InteractableItem bottle, InteractableItem glass, float amount)
        {
            // Your existing pour code here...
            
            // Show prompt
            UIPromptManager.Show($"Poured {amount}ml of {bottle.liquidType}");
        }
        
        // Example: When earning coins
        private void EarnCoins(int amount)
        {
            // Your existing coin earning code here...
            
            // Show prompt
            UIPromptManager.Show($"Get {amount} coin");
        }
        
        // Example: When serving a drink
        private void ServeDrink(string drinkName)
        {
            // Your existing serve code here...
            
            // Show prompt
            UIPromptManager.Show($"Served {drinkName}");
        }
        
        // Helper method to get appropriate pickup message
        private string GetPickupMessage(InteractableItem item)
        {
            switch (item.itemType)
            {
                case ItemType.Bottle:
                    return $"Picked up {item.liquidType}";
                case ItemType.Glass:
                    return $"Picked up {item.itemName}";
                default:
                    return $"Picked up {item.itemName}";
            }
        }
        
        // Example: Integration points in your existing code
        // 
        // In HandleInteractionInput() method:
        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     if (heldObject == null && currentHighlightedObject != null)
        //     {
        //         InteractableItem item = currentHighlightedObject.GetComponent<InteractableItem>();
        //         if (item != null)
        //         {
        //             PickUpItem(item);
        //             UIPromptManager.Show(GetPickupMessage(item)); // <-- Add this line
        //         }
        //     }
        // }
    }
}
