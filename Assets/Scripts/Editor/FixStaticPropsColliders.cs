using UnityEngine;
using UnityEditor;

namespace BarSimulator.Editor
{
    /// <summary>
    /// 修復 SM_ 物品的 Collider 設置
    /// 確保 MeshCollider 設置為 convex（可拾取物品需要）
    /// </summary>
    public class FixStaticPropsColliders
    {
        [MenuItem("Bar Simulator/Fix Static Props Colliders")]
        public static void Execute()
        {
            int fixedCount = 0;

            // 找到所有帶有 StaticProp 組件的物品
            StaticProp[] staticProps = Object.FindObjectsOfType<StaticProp>();
            
            foreach (StaticProp prop in staticProps)
            {
                GameObject obj = prop.gameObject;
                
                // 檢查是否有 MeshCollider
                MeshCollider meshCollider = obj.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    // 如果物品可以被拾取，MeshCollider 必須是 convex
                    if (prop.canBePickedUp && !meshCollider.convex)
                    {
                        meshCollider.convex = true;
                        fixedCount++;
                        EditorUtility.SetDirty(obj);
                        
                        Debug.Log($"[Fix Colliders] 修復 {obj.name} 的 MeshCollider (設置為 convex)");
                    }
                }
                
                // 檢查是否有 Rigidbody
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 確保 Rigidbody 設置正確
                    if (!rb.isKinematic || rb.useGravity)
                    {
                        rb.isKinematic = true;
                        rb.useGravity = false;
                        fixedCount++;
                        EditorUtility.SetDirty(obj);
                        
                        Debug.Log($"[Fix Colliders] 修復 {obj.name} 的 Rigidbody 設置");
                    }
                }
            }
            
            Debug.Log($"<color=green>[Fix Static Props Colliders] 完成！修復了 {fixedCount} 個物品</color>");
            
            // 保存場景
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }
    }
}
