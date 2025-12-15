using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

/// <summary>
/// 修復場景中所有可互動物件的問題
/// - 取消 Static 標記
/// - 修復異常位置
/// - 確保所有組件正確配置
/// </summary>
public class FixInteractableObjects : EditorWindow
{
    [MenuItem("Tools/Fix Interactable Objects")]
    public static void ShowWindow()
    {
        GetWindow<FixInteractableObjects>("Fix Interactable Objects");
    }

    private void OnGUI()
    {
        GUILayout.Label("修復可互動物件", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("修復所有可互動物件", GUILayout.Height(40)))
        {
            FixAllInteractableObjects();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("檢查所有可互動物件狀態", GUILayout.Height(30)))
        {
            CheckAllInteractableObjects();
        }
    }

    private static void FixAllInteractableObjects()
    {
        Debug.Log("=== 開始修復可互動物件 ===");
        int fixedCount = 0;

        // 找到所有帶有 InteractableItem 組件的物件
        InteractableItem[] allItems = GameObject.FindObjectsOfType<InteractableItem>(true);
        
        Debug.Log($"找到 {allItems.Length} 個可互動物件");

        foreach (InteractableItem item in allItems)
        {
            GameObject obj = item.gameObject;
            bool needsFix = false;
            string fixLog = $"檢查物件: {obj.name}";

            // 1. 檢查並取消 Static 標記
            if (obj.isStatic)
            {
                obj.isStatic = false;
                fixLog += "\n  ✓ 取消 Static 標記";
                needsFix = true;
            }

            // 2. 檢查位置是否異常（y < -1000 或 y > 1000）
            if (obj.transform.position.y < -1000f || obj.transform.position.y > 1000f)
            {
                // 嘗試找到合理的位置
                Vector3 newPos = obj.transform.position;
                
                // 如果是酒瓶，放在吧台上（y = 1.4）
                if (item.itemType == ItemType.Bottle)
                {
                    newPos.y = 1.4f;
                }
                // 如果是玻璃杯，放在吧台上（y = 1.4）
                else if (item.itemType == ItemType.Glass)
                {
                    newPos.y = 1.4f;
                }
                // 其他物品
                else
                {
                    newPos.y = 1.4f;
                }

                obj.transform.position = newPos;
                fixLog += $"\n  ✓ 修復異常位置: y = {newPos.y}";
                needsFix = true;
            }

            // 3. 確保有 Rigidbody
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = obj.AddComponent<Rigidbody>();
                rb.mass = item.itemType == ItemType.Bottle ? 0.5f : 0.3f;
                rb.useGravity = true;
                rb.isKinematic = false;
                fixLog += "\n  ✓ 添加 Rigidbody";
                needsFix = true;
            }
            else
            {
                // 確保 Rigidbody 設置正確
                if (rb.isKinematic)
                {
                    rb.isKinematic = false;
                    fixLog += "\n  ✓ 取消 Rigidbody.isKinematic";
                    needsFix = true;
                }
                if (!rb.useGravity)
                {
                    rb.useGravity = true;
                    fixLog += "\n  ✓ 啟用 Rigidbody.useGravity";
                    needsFix = true;
                }
            }

            // 4. 確保有碰撞器
            Collider col = obj.GetComponent<Collider>();
            if (col == null)
            {
                // 優先使用 MeshCollider
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    MeshCollider meshCol = obj.AddComponent<MeshCollider>();
                    meshCol.convex = true;
                    fixLog += "\n  ✓ 添加 MeshCollider";
                }
                else
                {
                    // 如果沒有 MeshFilter，使用 BoxCollider
                    BoxCollider boxCol = obj.AddComponent<BoxCollider>();
                    fixLog += "\n  ✓ 添加 BoxCollider";
                }
                needsFix = true;
            }
            else
            {
                // 確保碰撞器已啟用
                if (!col.enabled)
                {
                    col.enabled = true;
                    fixLog += "\n  ✓ 啟用 Collider";
                    needsFix = true;
                }

                // 如果是 MeshCollider，確保 convex 為 true
                MeshCollider meshCol = col as MeshCollider;
                if (meshCol != null && !meshCol.convex)
                {
                    meshCol.convex = true;
                    fixLog += "\n  ✓ 設置 MeshCollider.convex = true";
                    needsFix = true;
                }
            }

            // 5. 確保 InteractableItem 組件已啟用
            if (!item.enabled)
            {
                item.enabled = true;
                fixLog += "\n  ✓ 啟用 InteractableItem 組件";
                needsFix = true;
            }

            if (needsFix)
            {
                Debug.Log(fixLog);
                fixedCount++;
                EditorUtility.SetDirty(obj);
            }
        }

        if (fixedCount > 0)
        {
            Debug.Log($"=== 修復完成！共修復 {fixedCount} 個物件 ===");
            EditorUtility.DisplayDialog("修復完成", $"成功修復 {fixedCount} 個可互動物件", "確定");
        }
        else
        {
            Debug.Log("=== 所有物件都正常，無需修復 ===");
            EditorUtility.DisplayDialog("檢查完成", "所有可互動物件都正常", "確定");
        }
    }

    private static void CheckAllInteractableObjects()
    {
        Debug.Log("=== 檢查所有可互動物件狀態 ===");

        InteractableItem[] allItems = GameObject.FindObjectsOfType<InteractableItem>(true);
        
        Debug.Log($"找到 {allItems.Length} 個可互動物件\n");

        int problemCount = 0;

        foreach (InteractableItem item in allItems)
        {
            GameObject obj = item.gameObject;
            bool hasProblem = false;
            string status = $"物件: {obj.name}";

            // 檢查 Static
            if (obj.isStatic)
            {
                status += "\n  ❌ 物件被標記為 Static";
                hasProblem = true;
            }
            else
            {
                status += "\n  ✓ Static: 正常";
            }

            // 檢查位置
            if (obj.transform.position.y < -1000f || obj.transform.position.y > 1000f)
            {
                status += $"\n  ❌ 位置異常: y = {obj.transform.position.y}";
                hasProblem = true;
            }
            else
            {
                status += $"\n  ✓ 位置: y = {obj.transform.position.y:F2}";
            }

            // 檢查 Rigidbody
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                status += "\n  ❌ 缺少 Rigidbody";
                hasProblem = true;
            }
            else
            {
                if (rb.isKinematic)
                {
                    status += "\n  ⚠ Rigidbody.isKinematic = true";
                }
                else
                {
                    status += "\n  ✓ Rigidbody: 正常";
                }
            }

            // 檢查 Collider
            Collider col = obj.GetComponent<Collider>();
            if (col == null)
            {
                status += "\n  ❌ 缺少 Collider";
                hasProblem = true;
            }
            else
            {
                if (!col.enabled)
                {
                    status += "\n  ❌ Collider 已禁用";
                    hasProblem = true;
                }
                else
                {
                    status += $"\n  ✓ Collider: {col.GetType().Name}";
                }
            }

            // 檢查 InteractableItem
            if (!item.enabled)
            {
                status += "\n  ❌ InteractableItem 組件已禁用";
                hasProblem = true;
            }
            else
            {
                status += $"\n  ✓ InteractableItem: {item.itemType} - {item.itemName}";
            }

            if (hasProblem)
            {
                Debug.LogWarning(status);
                problemCount++;
            }
            else
            {
                Debug.Log(status);
            }

            Debug.Log("---");
        }

        if (problemCount > 0)
        {
            Debug.LogWarning($"=== 檢查完成！發現 {problemCount} 個物件有問題 ===");
        }
        else
        {
            Debug.Log("=== 檢查完成！所有物件都正常 ===");
        }
    }
}
