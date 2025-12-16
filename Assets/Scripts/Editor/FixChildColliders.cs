using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

/// <summary>
/// 修復可互動物件的子物件碰撞器問題
/// 子物件的碰撞器會阻擋射線檢測，導致無法拾取物品
/// </summary>
public class FixChildColliders
{
    public static void Execute()
    {
        Debug.Log("=== 開始修復子物件碰撞器 ===");
        int fixedCount = 0;

        // 找到所有帶有 InteractableItem 組件的物件
        InteractableItem[] allItems = GameObject.FindObjectsOfType<InteractableItem>(true);
        
        Debug.Log($"找到 {allItems.Length} 個可互動物件");

        foreach (InteractableItem item in allItems)
        {
            GameObject obj = item.gameObject;
            
            // 檢查子物件
            Collider[] childColliders = obj.GetComponentsInChildren<Collider>(true);
            
            foreach (Collider col in childColliders)
            {
                // 跳過父物件自己的碰撞器
                if (col.gameObject == obj)
                    continue;
                
                // 保存子物件引用
                GameObject childObj = col.gameObject;
                string childName = childObj.name;
                string colType = col.GetType().Name;
                
                // 移除子物件的碰撞器
                Object.DestroyImmediate(col);
                
                Debug.Log($"移除 {obj.name} 的子物件 {childName} 的碰撞器 ({colType})");
                fixedCount++;
                EditorUtility.SetDirty(childObj);
            }
        }

        if (fixedCount > 0)
        {
            Debug.Log($"=== 修復完成！共移除 {fixedCount} 個子物件碰撞器 ===");
        }
        else
        {
            Debug.Log("=== 所有物件都正常，無需修復 ===");
        }
    }
}
