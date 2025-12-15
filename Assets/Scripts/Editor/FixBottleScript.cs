using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

namespace BarSimulator.Editor
{
    public class FixBottleScript
    {
        public static void Execute(string argsJson)
        {
            string bottleName = "Vodka";
            
            // 簡單解析 JSON（如果提供）
            if (!string.IsNullOrEmpty(argsJson))
            {
                try
                {
                    var data = JsonUtility.FromJson<BottleArgs>(argsJson);
                    if (!string.IsNullOrEmpty(data.bottleName))
                    {
                        bottleName = data.bottleName;
                    }
                }
                catch
                {
                    Debug.LogWarning("無法解析參數，使用預設值");
                }
            }
            
            GameObject bottle = GameObject.Find(bottleName);
            if (bottle == null)
            {
                Debug.LogError($"找不到物件: {bottleName}");
                return;
            }

            Debug.Log($"開始修復: {bottleName}");
            
            // 1. 確保有 Rigidbody
            Rigidbody rb = bottle.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = bottle.AddComponent<Rigidbody>();
                Debug.Log($"添加了 Rigidbody");
            }
            rb.mass = 0.5f;
            rb.drag = 0f;
            rb.angularDrag = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = false;

            // 2. 確保有 InteractableItem
            InteractableItem item = bottle.GetComponent<InteractableItem>();
            if (item == null)
            {
                item = bottle.AddComponent<InteractableItem>();
                Debug.Log($"添加了 InteractableItem");
            }
            
            // 根據名稱設置液體類型
            LiquidType liquidType = LiquidType.None;
            if (bottleName.Contains("Vodka")) liquidType = LiquidType.Vodka;
            else if (bottleName.Contains("Whiskey") || bottleName.Contains("Maker")) liquidType = LiquidType.Whiskey;
            else if (bottleName.Contains("Rum")) liquidType = LiquidType.Rum;
            else if (bottleName.Contains("Gin")) liquidType = LiquidType.Gin;
            else if (bottleName.Contains("Taquila") || bottleName.Contains("Tequila")) liquidType = LiquidType.Tequila;
            
            item.itemType = ItemType.Bottle;
            item.itemName = bottleName;
            item.liquidType = liquidType;
            item.liquidAmount = 750f;
            item.maxLiquidAmount = 750f;

            // 3. 處理碰撞器
            MeshCollider meshCollider = bottle.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.convex = true;
                Debug.Log($"設置 MeshCollider 為 convex");
            }

            // 4. 禁用所有子物件的碰撞器（LOD 物件的碰撞器）
            BoxCollider[] childColliders = bottle.GetComponentsInChildren<BoxCollider>();
            int disabledCount = 0;
            foreach (BoxCollider col in childColliders)
            {
                if (col.gameObject != bottle)
                {
                    col.enabled = false;
                    disabledCount++;
                }
            }
            Debug.Log($"禁用了 {disabledCount} 個子物件碰撞器");

            // 5. 標記場景為已修改
            EditorUtility.SetDirty(bottle);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(bottle.scene);
            
            Debug.Log($"✓ 成功修復: {bottleName}");
            Debug.Log($"  - 液體類型: {liquidType}");
            Debug.Log($"  - Rigidbody: 已設置");
            Debug.Log($"  - InteractableItem: 已設置");
            Debug.Log($"  - 子碰撞器: 已禁用 {disabledCount} 個");
        }
        
        [System.Serializable]
        private class BottleArgs
        {
            public string bottleName;
        }
    }
}
