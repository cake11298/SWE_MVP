using UnityEngine;
using UnityEditor;

public class QuickCleanupProps
{
    [MenuItem("Tools/Quick Cleanup Props (Remove Interactivity)")]
    public static void Execute()
    {
        GameObject propsParent = GameObject.Find("Props");
        
        if (propsParent == null)
        {
            Debug.LogError("找不到 Props 物件！");
            EditorUtility.DisplayDialog("錯誤", "場景中找不到 Props 物件", "確定");
            return;
        }

        int objectsProcessed = 0;
        int rigidbodiesRemoved = 0;
        int interactablesRemoved = 0;
        int otherComponentsRemoved = 0;

        // 獲取所有子物件（包括巢狀的）
        Transform[] allChildren = propsParent.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform child in allChildren)
        {
            if (child == propsParent.transform)
                continue;

            GameObject obj = child.gameObject;
            objectsProcessed++;

            // 移除 Rigidbody（重力）
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Object.DestroyImmediate(rb);
                rigidbodiesRemoved++;
            }

            // 移除所有可能的互動組件
            Component[] allComponents = obj.GetComponents<Component>();
            foreach (Component comp in allComponents)
            {
                if (comp == null) continue;
                
                string typeName = comp.GetType().Name;
                
                // 移除互動相關組件
                if (typeName == "Interactable" || 
                    typeName == "GrabbableObject" || 
                    typeName == "PickupObject" ||
                    typeName == "LiquidContainer" ||
                    typeName == "BottleController" ||
                    typeName == "GlassController" ||
                    typeName == "ShakerController")
                {
                    Object.DestroyImmediate(comp);
                    interactablesRemoved++;
                }
            }

            // 確保 Collider 不是 trigger
            Collider[] colliders = obj.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                if (col != null)
                {
                    col.isTrigger = false;
                }
            }

            // 設為靜態以優化效能
            obj.isStatic = true;
        }

        // 標記場景為已修改
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        string message = $"清理完成！\n\n" +
                        $"處理物件數: {objectsProcessed}\n" +
                        $"移除 Rigidbody: {rigidbodiesRemoved}\n" +
                        $"移除互動組件: {interactablesRemoved}";

        Debug.Log($"<color=green>{message}</color>");
        EditorUtility.DisplayDialog("完成", message, "確定");
    }
}
