using UnityEngine;
using UnityEditor;
using BarSimulator;
using BarSimulator.Player;

public class RemoveInteractableFromStaticProps
{
    public static void Execute()
    {
        Debug.Log("[RemoveInteractable] Starting to remove interactable components from static props...");
        
        // 查找所有 SM 开头的物件
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        int removedCount = 0;
        int totalSMObjects = 0;

        // 定义应该保持可互动的物品名称关键词
        string[] interactableKeywords = new string[]
        {
            "Bottle", "Glass", "Cup", "Mug", "Martini", "Wine", "Whiskey", 
            "Vodka", "Gin", "Rum", "Tequila", "Cognac", "Shaker", "Jigger",
            "Coaster", "Ashtray"
        };

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("SM_"))
            {
                totalSMObjects++;
                
                // 检查是否应该保持可互动
                bool shouldBeInteractable = false;
                foreach (string keyword in interactableKeywords)
                {
                    if (obj.name.Contains(keyword))
                    {
                        shouldBeInteractable = true;
                        break;
                    }
                }

                // 如果不应该可互动，移除组件
                if (!shouldBeInteractable)
                {
                    bool removed = false;
                    
                    // 移除 InteractableItem
                    InteractableItem interactableItem = obj.GetComponent<InteractableItem>();
                    if (interactableItem != null)
                    {
                        Object.DestroyImmediate(interactableItem);
                        removed = true;
                    }
                    
                    // 移除 StaticProp
                    StaticProp staticProp = obj.GetComponent<StaticProp>();
                    if (staticProp != null)
                    {
                        Object.DestroyImmediate(staticProp);
                        removed = true;
                    }
                    
                    // 移除 Rigidbody
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Object.DestroyImmediate(rb);
                        removed = true;
                    }
                    
                    // 移除 Collider（如果是我们添加的）
                    Collider collider = obj.GetComponent<Collider>();
                    if (collider != null && collider is BoxCollider)
                    {
                        BoxCollider boxCollider = collider as BoxCollider;
                        // 只移除我们添加的默认大小的 collider
                        if (boxCollider.size == Vector3.one * 0.2f)
                        {
                            Object.DestroyImmediate(collider);
                            removed = true;
                        }
                    }
                    
                    if (removed)
                    {
                        removedCount++;
                        EditorUtility.SetDirty(obj);
                    }
                }
            }
        }

        // 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log($"[RemoveInteractable] Complete! Found {totalSMObjects} SM objects, removed components from {removedCount} static props");
    }
}
