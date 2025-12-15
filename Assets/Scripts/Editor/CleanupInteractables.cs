using UnityEngine;
using UnityEditor;
using BarSimulator.Player;
using System.Collections.Generic;
using System.Linq;

public class CleanupInteractables : MonoBehaviour
{
    public static void Execute()
    {
        // Objects to keep
        HashSet<string> objectsToKeep = new HashSet<string>
        {
            "Gin",
            "Maker_Whiskey",
            "Shaker",
            "Jigger"
        };
        
        // Also keep one SM_WineBottle (we'll keep SM_WineBottle17)
        string wineBottleToKeep = "SM_WineBottle17";
        
        var allInteractables = FindObjectsOfType<InteractableItem>(true);
        Debug.Log($"Found {allInteractables.Length} InteractableItem objects");
        
        int removedCount = 0;
        int fixedCount = 0;
        
        foreach (var item in allInteractables)
        {
            GameObject obj = item.gameObject;
            string objName = obj.name;
            
            // Check if this is an object we want to keep
            bool shouldKeep = objectsToKeep.Contains(objName) || objName == wineBottleToKeep;
            
            if (shouldKeep)
            {
                Debug.Log($"Keeping: {objName}");
                
                // Fix collider if it's a MeshCollider
                MeshCollider meshCollider = obj.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    Debug.Log($"  Fixing MeshCollider on {objName}");
                    
                    // Get bounds for BoxCollider
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Bounds bounds = renderer.bounds;
                        
                        // Remove MeshCollider
                        DestroyImmediate(meshCollider);
                        
                        // Add BoxCollider
                        BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
                        
                        // Set BoxCollider size based on mesh bounds
                        Vector3 localSize = obj.transform.InverseTransformVector(bounds.size);
                        Vector3 localCenter = obj.transform.InverseTransformPoint(bounds.center) - obj.transform.localPosition;
                        
                        boxCollider.size = localSize;
                        boxCollider.center = localCenter;
                        
                        fixedCount++;
                    }
                }
                
                // Ensure it has a Rigidbody
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = obj.AddComponent<Rigidbody>();
                    rb.mass = 0.5f;
                    rb.useGravity = true;
                    rb.isKinematic = false;
                    Debug.Log($"  Added Rigidbody to {objName}");
                }
            }
            else
            {
                // Remove InteractableItem component from objects we don't want
                Debug.Log($"Removing InteractableItem from: {objName}");
                DestroyImmediate(item);
                removedCount++;
            }
        }
        
        Debug.Log($"Cleanup complete! Removed {removedCount} InteractableItem components, Fixed {fixedCount} colliders");
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }
}
