using UnityEngine;
using UnityEditor;
using BarSimulator;
using BarSimulator.Player;

public class FixSMObjectsScript
{
    public static void Execute()
    {
        Debug.Log("[FixSMObjectsScript] Starting to fix all SM objects...");
        
        // 查找所有 SM 开头的物件
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        int fixedCount = 0;
        int totalSMObjects = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("SM_"))
            {
                totalSMObjects++;
                if (FixObject(obj))
                {
                    fixedCount++;
                }
            }
        }

        // 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log($"[FixSMObjectsScript] Complete! Found {totalSMObjects} SM objects, fixed {fixedCount} objects");
    }

    private static bool FixObject(GameObject obj)
    {
        bool modified = false;

        // 1. 确保有 Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
            modified = true;
        }

        // 设置 Rigidbody 属性 - 初始状态为 kinematic
        rb.mass = 0.5f;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        // 2. 确保有 Collider
        Collider collider = obj.GetComponent<Collider>();
        if (collider == null)
        {
            // 尝试从子物件获取 MeshFilter 来计算边界
            MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
            
            if (meshFilters.Length > 0)
            {
                // 计算所有子物件的总边界
                Bounds totalBounds = new Bounds(obj.transform.position, Vector3.zero);
                bool boundsInitialized = false;
                
                foreach (MeshFilter mf in meshFilters)
                {
                    if (mf.sharedMesh != null)
                    {
                        Renderer renderer = mf.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            if (!boundsInitialized)
                            {
                                totalBounds = renderer.bounds;
                                boundsInitialized = true;
                            }
                            else
                            {
                                totalBounds.Encapsulate(renderer.bounds);
                            }
                        }
                    }
                }
                
                if (boundsInitialized)
                {
                    // 添加 BoxCollider 并根据总边界设置大小
                    BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
                    
                    // 转换到本地空间
                    Vector3 localCenter = obj.transform.InverseTransformPoint(totalBounds.center);
                    Vector3 localSize = obj.transform.InverseTransformVector(totalBounds.size);
                    
                    boxCollider.center = localCenter;
                    boxCollider.size = new Vector3(
                        Mathf.Abs(localSize.x),
                        Mathf.Abs(localSize.y),
                        Mathf.Abs(localSize.z)
                    );
                    
                    modified = true;
                    Debug.Log($"Added BoxCollider to {obj.name} (size: {boxCollider.size})");
                }
                else
                {
                    AddDefaultCollider(obj);
                    modified = true;
                }
            }
            else
            {
                AddDefaultCollider(obj);
                modified = true;
            }
        }
        else
        {
            collider.isTrigger = false;
        }

        // 3. 确保有 StaticProp 组件
        StaticProp staticProp = obj.GetComponent<StaticProp>();
        if (staticProp == null)
        {
            staticProp = obj.AddComponent<StaticProp>();
            staticProp.canBePickedUp = true;
            staticProp.returnToOriginalPosition = false;
            modified = true;
        }

        // 4. 确保有 InteractableItem 组件
        InteractableItem interactableItem = obj.GetComponent<InteractableItem>();
        if (interactableItem == null)
        {
            interactableItem = obj.AddComponent<InteractableItem>();
            
            // 根据名称设置物品类型
            if (obj.name.Contains("Bottle") || obj.name.Contains("Whiskey") || obj.name.Contains("Wine"))
            {
                interactableItem.itemType = ItemType.Bottle;
                interactableItem.liquidType = DetermineLiquidType(obj.name);
                interactableItem.liquidAmount = 750f;
                interactableItem.maxLiquidAmount = 750f;
            }
            else if (obj.name.Contains("Glass") || obj.name.Contains("Cup") || obj.name.Contains("Mug") || obj.name.Contains("Martini"))
            {
                interactableItem.itemType = ItemType.Glass;
                interactableItem.maxGlassCapacity = 300f;
            }
            else
            {
                interactableItem.itemType = ItemType.Other;
            }
            
            interactableItem.itemName = obj.name;
            modified = true;
        }

        if (modified)
        {
            EditorUtility.SetDirty(obj);
        }

        return modified;
    }

    private static void AddDefaultCollider(GameObject obj)
    {
        BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
        boxCollider.center = Vector3.zero;
        boxCollider.size = Vector3.one * 0.2f;
    }

    private static LiquidType DetermineLiquidType(string objectName)
    {
        string nameLower = objectName.ToLower();
        
        if (nameLower.Contains("vodka")) return LiquidType.Vodka;
        if (nameLower.Contains("whiskey")) return LiquidType.Whiskey;
        if (nameLower.Contains("rum")) return LiquidType.Rum;
        if (nameLower.Contains("gin")) return LiquidType.Gin;
        if (nameLower.Contains("tequila") || nameLower.Contains("taquila")) return LiquidType.Tequila;
        if (nameLower.Contains("wine")) return LiquidType.Wine;
        if (nameLower.Contains("beer")) return LiquidType.Beer;
        
        return LiquidType.None;
    }
}
