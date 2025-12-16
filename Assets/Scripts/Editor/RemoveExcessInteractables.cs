using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using BarSimulator.Player;

public class RemoveExcessInteractables : EditorWindow
{
    [MenuItem("Tools/Remove Excess Interactables")]
    public static void RemoveInteractables()
    {
        // 保留互動功能的物件名稱
        HashSet<string> keepInteractive = new HashSet<string>
        {
            "Rum",
            "Vodka", 
            "ServeGlass"
        };

        int removedCount = 0;
        int keptCount = 0;

        // 獲取場景中所有的 GameObject
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // 檢查是否應該保留互動功能
            bool shouldKeepInteractive = keepInteractive.Contains(obj.name);

            if (!shouldKeepInteractive)
            {
                // 移除 InteractableItem 組件
                InteractableItem interactable = obj.GetComponent<InteractableItem>();
                if (interactable != null)
                {
                    DestroyImmediate(interactable);
                    removedCount++;
                    Debug.Log($"Removed InteractableItem from: {obj.name}");
                }

                // 移除 Rigidbody 組件（使物件不受重力影響）
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    DestroyImmediate(rb);
                    Debug.Log($"Removed Rigidbody from: {obj.name}");
                }

                // 保留 Collider（用於視覺碰撞，但設為靜態）
                Collider collider = obj.GetComponent<Collider>();
                if (collider != null)
                {
                    // 如果有 Collider，確保物件是靜態的
                    obj.isStatic = true;
                }
            }
            else
            {
                keptCount++;
                Debug.Log($"Kept interactive: {obj.name}");
            }
        }

        // 保存場景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log($"<color=green>清理完成！</color>");
        Debug.Log($"保留互動物件: {keptCount} 個 (Rum, Vodka, ServeGlass)");
        Debug.Log($"移除互動組件: {removedCount} 個物件");
        
        EditorUtility.DisplayDialog(
            "清理完成", 
            $"保留互動物件: {keptCount} 個\n移除互動組件: {removedCount} 個物件\n\n只有 Rum、Vodka 和 ServeGlass 保留互動功能！", 
            "確定"
        );
    }
}
