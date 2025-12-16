using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

public class AddInteractableToServeGlass : EditorWindow
{
    [MenuItem("Tools/Add Interactable To ServeGlass")]
    public static void AddInteractable()
    {
        GameObject serveGlass = GameObject.Find("ServeGlass");
        
        if (serveGlass == null)
        {
            Debug.LogError("ServeGlass not found!");
            return;
        }

        // 添加 InteractableItem 組件
        InteractableItem interactable = serveGlass.GetComponent<InteractableItem>();
        if (interactable == null)
        {
            interactable = serveGlass.AddComponent<InteractableItem>();
        }

        // 設置屬性
        interactable.itemType = ItemType.Glass;
        interactable.itemName = "ServeGlass";
        interactable.currentLiquidAmount = 0f;
        interactable.maxGlassCapacity = 300f;

        // 確保有 Rigidbody
        Rigidbody rb = serveGlass.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = serveGlass.AddComponent<Rigidbody>();
        }
        rb.mass = 0.2f;
        rb.useGravity = true;

        // 確保有 Collider
        BoxCollider collider = serveGlass.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = serveGlass.AddComponent<BoxCollider>();
        }

        // 保存場景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log("<color=green>ServeGlass 互動組件添加完成！</color>");
        
        EditorUtility.DisplayDialog(
            "完成", 
            "ServeGlass 已成功添加互動組件！", 
            "確定"
        );
    }
}
