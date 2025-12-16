using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using BarSimulator.Player;

/// <summary>
/// 配置场景中所有可互动物品（酒瓶、玻璃杯等）
/// 使它们可以像 Gin 和 Maker_Whiskey 一样被拾取和互动
/// </summary>
public class ConfigureInteractiveObjects : EditorWindow
{
    private List<string> processedPrefabs = new List<string>();
    private List<string> skippedPrefabs = new List<string>();
    private List<string> errorPrefabs = new List<string>();

    [MenuItem("Bar Tools/Configure Interactive Objects")]
    public static void ShowWindow()
    {
        GetWindow<ConfigureInteractiveObjects>("Configure Interactive Objects");
    }

    private void OnGUI()
    {
        GUILayout.Label("Configure Interactive Objects", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "此工具将配置所有酒瓶、玻璃杯、杯子等物品，使它们可以被拾取和互动。\n\n" +
            "将会添加以下组件：\n" +
            "- InteractableItem (互动脚本)\n" +
            "- Rigidbody (物理)\n" +
            "- BoxCollider (碰撞器)\n\n" +
            "点击下方按钮开始配置。",
            MessageType.Info
        );

        GUILayout.Space(10);

        if (GUILayout.Button("Configure All Drinkware Prefabs", GUILayout.Height(40)))
        {
            ConfigureAllPrefabs();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Configure Scene Objects", GUILayout.Height(40)))
        {
            ConfigureSceneObjects();
        }

        GUILayout.Space(20);

        // 显示结果
        if (processedPrefabs.Count > 0)
        {
            EditorGUILayout.LabelField("Successfully Processed:", EditorStyles.boldLabel);
            foreach (var prefab in processedPrefabs)
            {
                EditorGUILayout.LabelField("✓ " + prefab, EditorStyles.helpBox);
            }
        }

        if (skippedPrefabs.Count > 0)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Skipped:", EditorStyles.boldLabel);
            foreach (var prefab in skippedPrefabs)
            {
                EditorGUILayout.LabelField("- " + prefab, EditorStyles.helpBox);
            }
        }

        if (errorPrefabs.Count > 0)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Errors:", EditorStyles.boldLabel);
            foreach (var prefab in errorPrefabs)
            {
                EditorGUILayout.LabelField("✗ " + prefab, EditorStyles.helpBox);
            }
        }
    }

    private void ConfigureAllPrefabs()
    {
        processedPrefabs.Clear();
        skippedPrefabs.Clear();
        errorPrefabs.Clear();

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
                if (ConfigurePrefab(path, prefabName))
                {
                    successCount++;
                    processedPrefabs.Add(prefabName);
                }
            }
            else
            {
                skippedPrefabs.Add(prefabName + " (不匹配)");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"配置完成！处理了 {successCount}/{totalCount} 个预制件");
        EditorUtility.DisplayDialog("完成", $"成功配置了 {successCount}/{totalCount} 个预制件", "确定");
    }

    private void ConfigureSceneObjects()
    {
        processedPrefabs.Clear();
        skippedPrefabs.Clear();
        errorPrefabs.Clear();

        // 查找场景中所有匹配的物品
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int successCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.IsValid() && ShouldConfigurePrefab(obj.name))
            {
                if (ConfigureGameObject(obj))
                {
                    successCount++;
                    processedPrefabs.Add(obj.name);
                }
            }
        }

        Debug.Log($"场景配置完成！处理了 {successCount} 个物体");
        EditorUtility.DisplayDialog("完成", $"成功配置了 {successCount} 个场景物体", "确定");
    }

    private bool ShouldConfigurePrefab(string name)
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

    private bool ConfigurePrefab(string prefabPath, string prefabName)
    {
        try
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                errorPrefabs.Add(prefabName + " (无法加载)");
                return false;
            }

            // 使用 PrefabUtility 来编辑预制件
            string tempPath = prefabPath;
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(tempPath);

            bool success = ConfigureGameObject(prefabInstance);

            if (success)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, tempPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabInstance);

            return success;
        }
        catch (System.Exception e)
        {
            errorPrefabs.Add(prefabName + " (错误: " + e.Message + ")");
            Debug.LogError($"配置预制件 {prefabName} 时出错: {e.Message}");
            return false;
        }
    }

    private bool ConfigureGameObject(GameObject obj)
    {
        if (obj == null) return false;

        // 1. 添加或配置 InteractableItem 组件
        InteractableItem interactable = obj.GetComponent<InteractableItem>();
        if (interactable == null)
        {
            interactable = obj.AddComponent<InteractableItem>();
        }

        // 根据名称设置物品类型
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

        // 4. 设置 Layer 和 Tag（如果需要）
        // obj.layer = LayerMask.NameToLayer("Interactable");
        // obj.tag = "Interactable";

        EditorUtility.SetDirty(obj);
        return true;
    }

    private void ConfigureInteractableItem(InteractableItem item, string objectName)
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

    private bool IsBottle(string name)
    {
        return name.Contains("Bottle") || name.Contains("bottle") || 
               name.Contains("Sampagne") || name.Contains("Champagne");
    }

    private bool IsGlass(string name)
    {
        return name.Contains("Glass") || name.Contains("glass") ||
               name.Contains("Martini") || name.Contains("martini") ||
               name.Contains("Coctail") || name.Contains("Cocktail") ||
               name.Contains("BeerMug") || name.Contains("Beer") ||
               name.Contains("CoffeeCup") || name.Contains("Mug");
    }

    private LiquidType DetermineLiquidType(string name)
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

    private float DetermineGlassCapacity(string name)
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

    private void ConfigureRigidbody(Rigidbody rb, string objectName)
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

    private float DetermineMass(string name)
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

    private void ConfigureCollider(BoxCollider collider, GameObject obj)
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
