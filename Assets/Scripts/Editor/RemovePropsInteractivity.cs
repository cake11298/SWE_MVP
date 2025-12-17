using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RemovePropsInteractivity : EditorWindow
{
    [MenuItem("Tools/Remove Props Interactivity")]
    public static void RemoveAllPropsInteractivity()
    {
        // Find the Props parent object
        GameObject propsParent = GameObject.Find("Props");
        
        if (propsParent == null)
        {
            Debug.LogError("Props object not found in the scene!");
            return;
        }

        int objectsProcessed = 0;
        int componentsRemoved = 0;
        List<string> removedComponents = new List<string>();

        // Get all children of Props (including nested children)
        Transform[] allChildren = propsParent.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform child in allChildren)
        {
            if (child == propsParent.transform)
                continue; // Skip the Props parent itself

            GameObject obj = child.gameObject;
            objectsProcessed++;

            // Remove Rigidbody
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Undo.DestroyObjectImmediate(rb);
                componentsRemoved++;
                if (!removedComponents.Contains("Rigidbody"))
                    removedComponents.Add("Rigidbody");
            }

            // Remove Interactable
            Component interactable = obj.GetComponent("Interactable");
            if (interactable != null)
            {
                Undo.DestroyObjectImmediate(interactable);
                componentsRemoved++;
                if (!removedComponents.Contains("Interactable"))
                    removedComponents.Add("Interactable");
            }

            // Remove GrabbableObject
            Component grabbable = obj.GetComponent("GrabbableObject");
            if (grabbable != null)
            {
                Undo.DestroyObjectImmediate(grabbable);
                componentsRemoved++;
                if (!removedComponents.Contains("GrabbableObject"))
                    removedComponents.Add("GrabbableObject");
            }

            // Remove PickupObject
            Component pickup = obj.GetComponent("PickupObject");
            if (pickup != null)
            {
                Undo.DestroyObjectImmediate(pickup);
                componentsRemoved++;
                if (!removedComponents.Contains("PickupObject"))
                    removedComponents.Add("PickupObject");
            }

            // Remove LiquidContainer
            Component liquidContainer = obj.GetComponent("LiquidContainer");
            if (liquidContainer != null)
            {
                Undo.DestroyObjectImmediate(liquidContainer);
                componentsRemoved++;
                if (!removedComponents.Contains("LiquidContainer"))
                    removedComponents.Add("LiquidContainer");
            }

            // Remove any other physics-related components
            Collider[] colliders = obj.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                // Keep colliders but make them non-trigger and static
                col.isTrigger = false;
            }

            // Set object to static for optimization
            obj.isStatic = true;
        }

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log($"<color=green>Props Interactivity Removal Complete!</color>");
        Debug.Log($"Objects processed: {objectsProcessed}");
        Debug.Log($"Components removed: {componentsRemoved}");
        Debug.Log($"Component types removed: {string.Join(", ", removedComponents)}");
        
        EditorUtility.DisplayDialog(
            "完成", 
            $"已處理 {objectsProcessed} 個物件\n移除了 {componentsRemoved} 個組件\n\n移除的組件類型:\n{string.Join("\n", removedComponents)}", 
            "確定"
        );
    }
}
