using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

namespace BarSimulator.Editor
{
    /// <summary>
    /// 设置特定物品为可互动
    /// </summary>
    public class SetupSpecificItems
    {
        [MenuItem("Bar Simulator/Setup Specific Items")]
        public static void Setup()
        {
            // 设置Gin（酒瓶）
            SetupItem("Gin", ItemType.Bottle, LiquidType.Gin);
            
            // 设置Maker_Whiskey（酒瓶）
            SetupItem("Maker_Whiskey", ItemType.Bottle, LiquidType.Whiskey);
            
            // 设置Shaker（普通物品）
            SetupItem("Shaker", ItemType.Other, LiquidType.None);
            
            // 设置Jigger（普通物品）
            SetupItem("Jigger", ItemType.Other, LiquidType.None);
            
            // 确保所有Props下的玻璃杯都可以互动
            SetupGlasses();
            
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
            
            Debug.Log("✓ 特定物品设置完成！");
        }
        
        private static void SetupItem(string itemName, ItemType type, LiquidType liquidType)
        {
            GameObject obj = GameObject.Find(itemName);
            if (obj == null)
            {
                Debug.LogWarning($"找不到物品: {itemName}");
                return;
            }
            
            // 添加或获取InteractableItem组件
            InteractableItem item = obj.GetComponent<InteractableItem>();
            if (item == null)
            {
                item = obj.AddComponent<InteractableItem>();
            }
            
            item.itemType = type;
            item.itemName = itemName;
            item.liquidType = liquidType;
            
            if (type == ItemType.Bottle)
            {
                item.liquidAmount = 750f;
                item.maxLiquidAmount = 750f;
            }
            
            // 添加Rigidbody
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = obj.AddComponent<Rigidbody>();
            }
            rb.mass = type == ItemType.Bottle ? 0.5f : 0.3f;
            rb.useGravity = true;
            rb.isKinematic = false;
            
            // 添加Collider
            Collider col = obj.GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider boxCol = obj.AddComponent<BoxCollider>();
            }
            
            Debug.Log($"✓ 设置了 {itemName} 为 {type}");
        }
        
        private static void SetupGlasses()
        {
            int count = 0;
            
            // 查找Props下的所有物品
            GameObject propsParent = GameObject.Find("Props");
            if (propsParent == null)
            {
                Debug.LogWarning("找不到Props父物件");
                return;
            }
            
            // 遍历所有子物件
            foreach (Transform child in propsParent.transform)
            {
                string name = child.name;
                
                // 检查是否是玻璃杯（SM_*Glass格式）
                if (name.StartsWith("SM_") && 
                    (name.Contains("Glass") || name.Contains("Martini") || name.Contains("Coctail")))
                {
                    // 确保有InteractableItem组件
                    InteractableItem item = child.GetComponent<InteractableItem>();
                    if (item != null)
                    {
                        // 确保设置正确
                        item.itemType = ItemType.Glass;
                        item.itemName = name;
                        
                        // 确保有Rigidbody和Collider
                        Rigidbody rb = child.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.isKinematic = false;
                            rb.useGravity = true;
                        }
                        
                        Collider col = child.GetComponent<Collider>();
                        if (col != null)
                        {
                            col.enabled = true;
                        }
                        
                        count++;
                    }
                }
            }
            
            Debug.Log($"✓ 确认了 {count} 个玻璃杯可以互动");
        }
    }
}
