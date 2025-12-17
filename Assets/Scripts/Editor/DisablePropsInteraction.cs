using UnityEngine;
using UnityEditor;
using BarSimulator.Environment;

/// <summary>
/// Editor utility to disable gravity and interaction for all Props objects
/// </summary>
public class DisablePropsInteraction : MonoBehaviour
{
    [MenuItem("Tools/Disable Props Interaction")]
    public static void Execute()
    {
        // Find Props parent object
        GameObject propsParent = GameObject.Find("Props");
        if (propsParent == null)
        {
            Debug.LogWarning("Props parent object not found!");
            return;
        }

        int processedCount = 0;
        int rigidbodyCount = 0;
        int interactableCount = 0;

        // Process all children recursively
        Transform[] allChildren = propsParent.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform child in allChildren)
        {
            if (child == propsParent.transform)
                continue;

            // Disable Rigidbody gravity
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
                rigidbodyCount++;
            }

            // Remove InteractableItem component
            var interactable = child.GetComponent<BarSimulator.Player.InteractableItem>();
            if (interactable != null)
            {
                DestroyImmediate(interactable);
                interactableCount++;
            }

            // Remove StaticProp component
            var staticProp = child.GetComponent<BarSimulator.StaticProp>();
            if (staticProp != null)
            {
                DestroyImmediate(staticProp);
            }

            processedCount++;
        }

        Debug.Log($"[DisablePropsInteraction] Processed {processedCount} Props objects:");
        Debug.Log($"  - Disabled gravity on {rigidbodyCount} Rigidbodies");
        Debug.Log($"  - Removed {interactableCount} InteractableItem components");
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }
}
