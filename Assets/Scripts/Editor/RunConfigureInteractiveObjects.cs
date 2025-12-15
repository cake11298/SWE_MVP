using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using BarSimulator.Player;

/// <summary>
/// 自动运行配置脚本
/// </summary>
public class RunConfigureInteractiveObjects
{
    [MenuItem("Bar Tools/Run Configure Interactive Objects")]
    public static void Execute()
    {
        Debug.Log("开始配置所有可互动物品...");
        
        List<string> processedPrefabs = new List<string>();
        List<string> skippedPrefabs = new List<string>();
        List<string> errorPrefabs = new List<string>();

        // 查找所有匹配的预制件
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/TheBar/Prefabs" });

        int totalCount = 0;
        int successCount = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string prefabName = System.IO.Path.GetFileNameWithoutExtension(path);

            // 检查是否匹配我们要配置的物品
            if (ShouldConfigurePrefab(prefabName))
            {
                totalCount++;
                Debug.Log($"正在配置: {prefabName}");
                
                if (ConfigurePrefab(path, prefabName))
                {
                    successCount++;
                    processedPrefabs.Add(prefabName);
                    Debug.Log($"✓ 成功配置: {prefabName}");
                }
                else
                {
                    errorPrefabs.Add(prefabName);
                    Debug.LogWarning($"✗ 配置失败: {prefabName}");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("=== 配置完成 ===");
        Debug.Log($"总共处理: {successCount}/{totalCount} 个预制件");
        Debug.Log("\n成功配置的预制件:");
        foreach (var prefab in processedPrefabs)
        {
            Debug.Log($"  ✓ {prefab}");
        }

        if (errorPrefabs.Count > 0)
        {
            Debug.LogWarning("\n配置失败的预制件:");
            foreach (var prefab in errorPrefabs)
            {
                Debug.LogWarning($"  ✗ {prefab}");
            }
        }
    }

    private static bool ShouldConfigurePrefab(string name)
    {
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

    private static bool ConfigurePrefab(string prefabPath, string prefabName)
    {
        try
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"无法加载预制件: {prefabName}");
                return false;
            }

            // 使用 PrefabUtility 来编辑预制件
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

            bool success = ConfigureGameObject(prefabInstance, prefabName);

            if (success)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabInstance);

            return success;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"配置预制件 {prefabName} 时出错: {e.Message}");
            return false;
        }
    }

    private static bool ConfigureGameObject(GameObject obj, string objectName)
    {
        if (obj == null) return false;

        // 1. 添加或配置 InteractableItem 组件
        InteractableItem interactable = obj.GetComponent<InteractableItem>();
        if (interactable == null)
        {
            interactable = obj.AddComponent<InteractableItem>();
        }

        // 根据名称设置物品类型
        ConfigureInteractableItem(interactable, objectName);

        // 2. 添加或配置 Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
        }

        ConfigureRigidbody(rb, objectName);

        // 3. 添加或配置 BoxCollider
        BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = obj.AddComponent<BoxCollider>();
        }

        ConfigureCollider(boxCollider, obj);

        EditorUtility.SetDirty(obj);
        return true;
    }

    private static void ConfigureInteractableItem(InteractableItem item, string objectName)
    {
        // 设置物品名称
        item.itemName = objectName;

        // 根据名称判断物品类型
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

        // 啤酒杯容量较大
        if (lowerName.Contains("beer"))
            return 500f;

        // 马提尼杯容量较小
        if (lowerName.Contains("martini"))
            return 150f;

        // 鸡尾酒杯
        if (lowerName.Contains("coctail") || lowerName.Contains("cocktail"))
            return 200f;

        // 咖啡杯
        if (lowerName.Contains("coffee") || lowerName.Contains("mug"))
            return 250f;

        // 默认葡萄酒杯容量
        return 300f;
    }

    private static void ConfigureRigidbody(Rigidbody rb, string objectName)
    {
        // 根据物品类型设置质量
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

        // 酒瓶（满瓶）
        if (lowerName.Contains("bottle"))
            return 0.5f;

        // 玻璃杯
        if (lowerName.Contains("glass"))
            return 0.2f;

        // 马提尼杯（较轻）
        if (lowerName.Contains("martini"))
            return 0.15f;

        // 啤酒杯（较重）
        if (lowerName.Contains("beer"))
            return 0.3f;

        // 咖啡杯
        if (lowerName.Contains("coffee") || lowerName.Contains("mug"))
            return 0.25f;

        // 小物品（烟灰缸、杯垫）
        if (lowerName.Contains("ashtray") || lowerName.Contains("coaster"))
            return 0.1f;

        // 默认
        return 0.3f;
    }

    private static void ConfigureCollider(BoxCollider collider, GameObject obj)
    {
        // 尝试从子物体获取 MeshFilter 来计算边界
        MeshFilter meshFilter = obj.GetComponentInChildren<MeshFilter>();
        
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Bounds bounds = meshFilter.sharedMesh.bounds;
            
            // 设置碰撞器大小和中心
            collider.center = bounds.center;
            collider.size = bounds.size;
        }
        else
        {
            // 如果没有网格，使用默认值
            collider.center = Vector3.zero;
            collider.size = Vector3.one;
        }

        collider.isTrigger = false;
    }
}
