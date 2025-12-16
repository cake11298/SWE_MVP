using UnityEngine;
using UnityEditor;
using BarSimulator;
using BarSimulator.Player;

public class ForceRemoveAllSMInteractables
{
    public static void Execute()
    {
        Debug.Log("[ForceRemove] Starting to remove ALL interactable components from ALL SM objects...");
        
        // 查找所有 SM 开头的物件
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        int removedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("SM_"))
            {
                bool removed = false;
                
                // 移除 InteractableItem
                InteractableItem interactableItem = obj.GetComponent<InteractableItem>();
                if (interactableItem != null)
                {
                    Object.DestroyImmediate(interactableItem);
                    removed = true;
                    Debug.Log($"Removed InteractableItem from {obj.name}");
                }
                
                // 移除 StaticProp
                StaticProp staticProp = obj.GetComponent<StaticProp>();
                if (staticProp != null)
                {
                    Object.DestroyImmediate(staticProp);
                    removed = true;
                    Debug.Log($"Removed StaticProp from {obj.name}");
                }
                
                // 移除 Rigidbody
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Object.DestroyImmediate(rb);
                    removed = true;
                    Debug.Log($"Removed Rigidbody from {obj.name}");
                }
                
                if (removed)
                {
                    removedCount++;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        // 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log($"[ForceRemove] Complete! Removed components from {removedCount} SM objects");
    }
}
