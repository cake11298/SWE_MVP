using UnityEngine;
using BarSimulator.UI;

namespace BarSimulator.Examples
{
    /// <summary>
    /// Example script showing how to use the UIPromptManager system.
    /// This demonstrates various ways to display prompts below the crosshair.
    /// </summary>
    public class UIPromptExample : MonoBehaviour
    {
        private void Update()
        {
            // Example 1: Show prompt when picking up items
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UIPromptManager.Show("Picked up Gin");
            }
            
            // Example 2: Show prompt when picking up different items
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UIPromptManager.Show("Picked up Shaker");
            }
            
            // Example 3: Show prompt for glass
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UIPromptManager.Show("Picked up ServeGlass");
            }
            
            // Example 4: Show prompt for jigger
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                UIPromptManager.Show("Picked up Jigger");
            }
            
            // Example 5: Show prompt for earning coins
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                UIPromptManager.Show("Get 200 coin");
            }
            
            // Example 6: Show prompt for any custom message
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                UIPromptManager.Show("Custom message here!");
            }
            
            // Example 7: Manually hide prompt (optional, it auto-hides after 3 seconds)
            if (Input.GetKeyDown(KeyCode.H))
            {
                UIPromptManager.Hide();
            }
        }
    }
}
