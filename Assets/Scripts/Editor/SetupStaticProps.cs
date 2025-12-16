using UnityEngine;
using UnityEditor;

namespace BarSimulator.Editor
{
    /// <summary>
    /// 設置場景中所有 SM_ 開頭的裝飾物品為靜態（kinematic）
    /// 只有在被拾取時才啟用物理效果
    /// </summary>
    public class SetupStaticProps
    {
        [MenuItem("Bar Simulator/Setup Static Props (SM_ Objects)")]
        public static void Execute()
        {
            int propsFixed = 0;
            int glassesFixed = 0;
            int bottlesFixed = 0;

            // 找到所有 SM_ 開頭的物品
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.StartsWith("SM_"))
                {
                    // 檢查是否為玻璃杯或酒瓶
                    bool isGlass = obj.name.Contains("Glass") || obj.name.Contains("Martini") || obj.name.Contains("Wine");
                    bool isBottle = obj.name.Contains("Bottle") || obj.name.Contains("Sampagne") || obj.name.Contains("Whiskey");
                    
                    // 確保有 Rigidbody
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = obj.AddComponent<Rigidbody>();
                    }
                    
                    // 設置為 kinematic（不受重力影響，直到被拾取）
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    
                    // 確保有 Collider
                    if (obj.GetComponent<Collider>() == null)
                    {
                        // 根據物品類型添加適當的 Collider
                        if (isGlass || isBottle)
                        {
                            BoxCollider collider = obj.AddComponent<BoxCollider>();
                            collider.isTrigger = false;
                        }
                        else
                        {
                            MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
                            meshCollider.convex = true;
                        }
                    }
                    
                    // 添加 StaticProp 組件來管理物理狀態
                    if (obj.GetComponent<StaticProp>() == null)
                    {
                        obj.AddComponent<StaticProp>();
                    }
                    
                    // 添加 InteractableItem 組件（如果是玻璃杯或酒瓶）
                    if ((isGlass || isBottle) && obj.GetComponent<BarSimulator.Player.InteractableItem>() == null)
                    {
                        var interactable = obj.AddComponent<BarSimulator.Player.InteractableItem>();
                        
                        if (isGlass)
                        {
                            interactable.itemType = BarSimulator.Player.ItemType.Glass;
                            interactable.itemName = obj.name;
                            glassesFixed++;
                        }
                        else if (isBottle)
                        {
                            interactable.itemType = BarSimulator.Player.ItemType.Bottle;
                            interactable.itemName = obj.name;
                            bottlesFixed++;
                        }
                    }
                    
                    propsFixed++;
                    EditorUtility.SetDirty(obj);
                }
            }
            
            Debug.Log($"<color=green>[Setup Static Props] 完成！</color>");
            Debug.Log($"總共處理: {propsFixed} 個物品");
            Debug.Log($"玻璃杯: {glassesFixed} 個");
            Debug.Log($"酒瓶: {bottlesFixed} 個");
            Debug.Log($"其他裝飾物: {propsFixed - glassesFixed - bottlesFixed} 個");
            
            // 保存場景
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }
    }
}
