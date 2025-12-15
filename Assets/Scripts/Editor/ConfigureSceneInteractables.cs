using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using BarSimulator.Player;

/// <summary>
/// 配置场景中所有可互动物品实例
/// </summary>
public class ConfigureSceneInteractables
{
    [MenuItem("Bar Tools/Configure Scene Interactive Objects")]
    public static void Execute()
    {
        Debug.Log("开始配置场景中的可互动物品...");
        
        List<string> processedObjects = new List<string>();
        List<string> skippedObjects = new List<string>();

        // 查找场景中所有物体
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        int successCount = 0;

        foreach (GameObject obj in allObjects)
        {
            // 只处理场景中的物体（不是预制件）
            if (obj.scene.IsValid() && ShouldConfigureObject(obj.name))
            {
                Debug.Log($"正在配置场景物体: {obj.name}");
                
                if (ConfigureGameObject(obj))
                {
                    successCount++;
                    processedObjects.Add(obj.name);
                    Debug.Log($"✓ 成功配置: {obj.name}");
                }
                else
                {
                    skippedObjects.Add(obj.name);
                    Debug.LogWarning($"✗ 跳过: {obj.name}");
                }
            }
        }

        // 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log("=== 场景配置完成 ===");
        Debug.Log($"总共处理: {successCount} 个场景物体");
        Debug.Log("\n成功配置的物体:");
        foreach (var obj in processedObjects)
        {
            Debug.Log($"  ✓ {obj}");
        }

        if (skippedObjects.Count > 0)
        {
            Debug.LogWarning("\n跳过的物体:");
            foreach (var obj in skippedObjects)
            {
                Debug.LogWarning($"  - {obj}");
            }
        }
    }

    private static bool ShouldConfigureObject(string name)
    {
        // 跳过已经配置好的物体
        if (name == "Gin" || name == "Maker_Whiskey" || name == "Vodka" || 
            name == "Shaker" || name == "Jigger")
            return false;

        // 酒瓶类
        if (name.Contains("Bottle") || name.Contains("bottle"))
            return true;

        // 玻璃杯类
        if (name.Contains("Glass") || name.Contains("glass"))
            return true;

        // 马提尼杯
        if (name.Contains("Martini") || name.Contains("martini"))
            return true;

        // 鸡尾酒杯
        if (name.Contains("Coctail") || name.Contains("Cocktail"))
            return true;

        // 啤酒杯
        if (name.Contains("BeerMug") || name.Contains("Beer"))
            return true;

        // 咖啡杯
        if (name.Contains("CoffeeCup") || name.Contains("Mug"))
            return true;

        // 香槟
        if (name.Contains("Sampagne") || name.Contains("Champagne"))
            return true;

        // 烟灰缸
        if (name.Contains("Ashtray"))
            return true;

        // 杯垫
        if (name.Contains("Coaster"))
            return true;

        return false;
    }

    private static bool ConfigureGameObject(GameObject obj)
    {
        if (obj == null) return false;

        // 检查是否已经有 InteractableItem 组件
        InteractableItem existingInteractable = obj.GetComponent<InteractableItem>();
        if (existingInteractable != null)
        {
            Debug.Log($"  {obj.name} 已经有 InteractableItem 组件，跳过");
            return false;
        }

        // 1. 添加 InteractableItem 组件
        InteractableItem interactable = obj.AddComponent<InteractableItem>();
        ConfigureInteractableItem(interactable, obj.name);

        // 2. 添加或配置 Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
        }
        ConfigureRigidbody(rb, obj.name);

        // 3. 添加或配置 BoxCollider
        BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = obj.AddComponent<BoxCollider>();
        }
        ConfigureCollider(boxCollider, obj);

        // 4. 移除子物体的碰撞器（避免冲突）
        RemoveChildColliders(obj);

        EditorUtility.SetDirty(obj);
        return true;
    }

    private static void RemoveChildColliders(GameObject parent)
    {
        // 获取所有子物体的碰撞器
        Collider[] childColliders = parent.GetComponentsInChildren<Collider>();
        
        // 创建一个列表来存储需要移除的碰撞器
        List<Collider> collidersToRemove = new List<Collider>();
        
        foreach (Collider collider in childColliders)
        {
            // 不要移除父物体自己的碰撞器
            if (collider != null && collider.gameObject != parent)
            {
                collidersToRemove.Add(collider);
            }
        }
        
        // 移除碰撞器
        foreach (Collider collider in collidersToRemove)
        {
            if (collider != null)
            {
                Debug.Log($"  移除子物体 {collider.gameObject.name} 的碰撞器");
                Object.DestroyImmediate(collider);
            }
        }
    }

    private static void ConfigureInteractableItem(InteractableItem item, string objectName)
    {
        item.itemName = objectName;

        if (IsBottle(objectName))
        {
            item.itemType = ItemType.Bottle;
            item.liquidType = DetermineLiquidType(objectName);
            item.liquidAmount = 750f;
            item.maxLiquidAmount = 750f;
        }
        else if (IsGlass(objectName))
        {
            item.itemType = ItemType.Glass;
            item.liquidType = LiquidType.None;
            item.currentLiquidAmount = 0f;
            item.maxGlassCapacity = DetermineGlassCapacity(objectName);
        }
        else
        {
            item.itemType = ItemType.Other;
        }
    }

    private static bool IsBottle(string name)
    {
        return name.Contains("Bottle") || name.Contains("bottle") || 
               name.Contains("Sampagne") || name.Contains("Champagne");
    }

    private static bool IsGlass(string name)
    {
        return name.Contains("Glass") || name.Contains("glass") ||
               name.Contains("Martini") || name.Contains("martini") ||
               name.Contains("Coctail") || name.Contains("Cocktail") ||
               name.Contains("BeerMug") || name.Contains("Beer") ||
               name.Contains("CoffeeCup") || name.Contains("Mug");
    }

    private static LiquidType DetermineLiquidType(string name)
    {
        string lowerName = name.ToLower();

        if (lowerName.Contains("whiskey") || lowerName.Contains("whisky"))
            return LiquidType.Whiskey;
        if (lowerName.Contains("vodka"))
            return LiquidType.Vodka;
        if (lowerName.Contains("gin"))
            return LiquidType.Gin;
        if (lowerName.Contains("rum"))
            return LiquidType.Rum;
        if (lowerName.Contains("tequila"))
            return LiquidType.Tequila;
        if (lowerName.Contains("wine") || lowerName.Contains("sampagne") || lowerName.Contains("champagne"))
            return LiquidType.Wine;
        if (lowerName.Contains("beer"))
            return LiquidType.Beer;

        return LiquidType.None;
    }

    private static float DetermineGlassCapacity(string name)
    {
        string lowerName = name.ToLower();

        if (lowerName.Contains("beer"))
            return 500f;
        if (lowerName.Contains("martini"))
            return 150f;
        if (lowerName.Contains("coctail") || lowerName.Contains("cocktail"))
            return 200f;
        if (lowerName.Contains("coffee") || lowerName.Contains("mug"))
            return 250f;

        return 300f;
    }

    private static void ConfigureRigidbody(Rigidbody rb, string objectName)
    {
        rb.mass = DetermineMass(objectName);
        rb.drag = 0f;
        rb.angularDrag = 0.05f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    private static float DetermineMass(string name)
    {
        string lowerName = name.ToLower();

        if (lowerName.Contains("bottle"))
            return 0.5f;
        if (lowerName.Contains("glass"))
            return 0.2f;
        if (lowerName.Contains("martini"))
            return 0.15f;
        if (lowerName.Contains("beer"))
            return 0.3f;
        if (lowerName.Contains("coffee") || lowerName.Contains("mug"))
            return 0.25f;
        if (lowerName.Contains("ashtray") || lowerName.Contains("coaster"))
            return 0.1f;

        return 0.3f;
    }

    private static void ConfigureCollider(BoxCollider collider, GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponentInChildren<MeshFilter>();
        
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Bounds bounds = meshFilter.sharedMesh.bounds;
            collider.center = bounds.center;
            collider.size = bounds.size;
        }
        else
        {
            collider.center = Vector3.zero;
            collider.size = Vector3.one;
        }

        collider.isTrigger = false;
    }
}
