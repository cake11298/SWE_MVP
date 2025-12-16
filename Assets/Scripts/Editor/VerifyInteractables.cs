using UnityEngine;
using BarSimulator.Player;

public class VerifyInteractables : MonoBehaviour
{
    public static void Execute()
    {
        var allInteractables = FindObjectsOfType<InteractableItem>(true);
        Debug.Log($"=== Verification: Found {allInteractables.Length} InteractableItem objects ===");
        
        foreach (var item in allInteractables)
        {
            GameObject obj = item.gameObject;
            
            // Check components
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            Collider col = obj.GetComponent<Collider>();
            
            string colliderType = "None";
            if (col != null)
            {
                colliderType = col.GetType().Name;
            }
            
            Debug.Log($"✓ {obj.name}:");
            Debug.Log($"  - Type: {item.itemType}");
            Debug.Log($"  - Liquid: {item.liquidType}");
            Debug.Log($"  - Rigidbody: {(rb != null ? "Yes" : "NO - MISSING!")}");
            Debug.Log($"  - Collider: {colliderType}");
            
            if (rb == null)
            {
                Debug.LogWarning($"  WARNING: {obj.name} is missing Rigidbody!");
            }
            if (col == null)
            {
                Debug.LogWarning($"  WARNING: {obj.name} is missing Collider!");
            }
        }
        
        Debug.Log("=== Verification Complete ===");
    }
}
