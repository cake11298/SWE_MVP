using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

public class RestoreInteractableItems : EditorWindow
{
    [MenuItem("Tools/Restore Interactable Items")]
    public static void RestoreInteractables()
    {
        int restoredCount = 0;

        // Gin - 金酒瓶
        GameObject gin = GameObject.Find("Gin");
        if (gin != null)
        {
            AddInteractableBottle(gin, "Gin", LiquidType.Gin, 750f);
            restoredCount++;
            Debug.Log("Restored: Gin");
        }

        // Maker_Whiskey - 威士忌瓶
        GameObject makerWhiskey = GameObject.Find("Maker_Whiskey");
        if (makerWhiskey != null)
        {
            AddInteractableBottle(makerWhiskey, "Maker's Mark Whiskey", LiquidType.Whiskey, 750f);
            restoredCount++;
            Debug.Log("Restored: Maker_Whiskey");
        }

        // Shaker - 調酒器
        GameObject shaker = GameObject.Find("Shaker");
        if (shaker != null)
        {
            AddInteractableOther(shaker, "Cocktail Shaker", 0.3f);
            restoredCount++;
            Debug.Log("Restored: Shaker");
        }

        // Jigger - 量酒器
        GameObject jigger = GameObject.Find("Jigger");
        if (jigger != null)
        {
            AddInteractableOther(jigger, "Jigger", 0.1f);
            restoredCount++;
            Debug.Log("Restored: Jigger");
        }

        // 保存場景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log($"<color=green>恢復完成！</color>");
        Debug.Log($"恢復互動物件: {restoredCount} 個");
        
        EditorUtility.DisplayDialog(
            "恢復完成", 
            $"已恢復 {restoredCount} 個物件的互動功能！\n\n" +
            "- Gin (金酒)\n" +
            "- Maker_Whiskey (威士忌)\n" +
            "- Shaker (調酒器)\n" +
            "- Jigger (量酒器)", 
            "確定"
        );
    }

    private static void AddInteractableBottle(GameObject obj, string itemName, LiquidType liquidType, float amount)
    {
        // 添加 InteractableItem
        InteractableItem interactable = obj.GetComponent<InteractableItem>();
        if (interactable == null)
        {
            interactable = obj.AddComponent<InteractableItem>();
        }

        interactable.itemType = ItemType.Bottle;
        interactable.itemName = itemName;
        interactable.liquidType = liquidType;
        interactable.liquidAmount = amount;
        interactable.maxLiquidAmount = amount;

        // 添加 Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
        }
        rb.mass = 0.5f;
        rb.useGravity = true;
        rb.isKinematic = false;

        // 確保有 Collider（已經存在 BoxCollider）
    }

    private static void AddInteractableOther(GameObject obj, string itemName, float mass)
    {
        // 添加 InteractableItem
        InteractableItem interactable = obj.GetComponent<InteractableItem>();
        if (interactable == null)
        {
            interactable = obj.AddComponent<InteractableItem>();
        }

        interactable.itemType = ItemType.Other;
        interactable.itemName = itemName;

        // 添加 Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
        }
        rb.mass = mass;
        rb.useGravity = true;
        rb.isKinematic = false;

        // 確保有 Collider（已經存在 BoxCollider）
    }
}
