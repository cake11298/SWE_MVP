using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

namespace BarSimulator.Editor
{
    /// <summary>
    /// 修復酒瓶互動問題的編輯器工具
    /// 確保酒瓶可以被正確拾取和移動
    /// </summary>
    public class FixBottleInteraction : EditorWindow
    {
        [MenuItem("Tools/Fix Bottle Interaction")]
        public static void ShowWindow()
        {
            GetWindow<FixBottleInteraction>("Fix Bottle Interaction");
        }

        private void OnGUI()
        {
            GUILayout.Label("修復酒瓶互動問題", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("修復 Vodka 酒瓶"))
            {
                FixBottle("Vodka", LiquidType.Vodka);
            }

            if (GUILayout.Button("修復 Rum 酒瓶"))
            {
                FixBottle("Rum", LiquidType.Rum);
            }

            if (GUILayout.Button("修復 Cointreau 酒瓶"))
            {
                FixBottle("Cointreau", LiquidType.None);
            }

            if (GUILayout.Button("修復 Cognac 酒瓶"))
            {
                FixBottle("Cognac", LiquidType.None);
            }

            if (GUILayout.Button("修復 Taquila 酒瓶"))
            {
                FixBottle("Taquila", LiquidType.Tequila);
            }

            GUILayout.Space(20);

            if (GUILayout.Button("修復所有 SM_WhiskeyBottle"))
            {
                FixAllBottlesWithPrefix("SM_WhiskeyBottle", LiquidType.Whiskey);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("診斷選中的物件"))
            {
                DiagnoseSelectedObject();
            }
        }

        private void FixBottle(string bottleName, LiquidType liquidType)
        {
            GameObject bottle = GameObject.Find(bottleName);
            if (bottle == null)
            {
                Debug.LogError($"找不到物件: {bottleName}");
                return;
            }

            SetupBottleComponents(bottle, bottleName, liquidType);
            Debug.Log($"已修復: {bottleName}");
        }

        private void FixAllBottlesWithPrefix(string prefix, LiquidType liquidType)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int count = 0;

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.StartsWith(prefix))
                {
                    SetupBottleComponents(obj, obj.name, liquidType);
                    count++;
                }
            }

            Debug.Log($"已修復 {count} 個 {prefix} 物件");
        }

        private void SetupBottleComponents(GameObject bottle, string itemName, LiquidType liquidType)
        {
            // 1. 確保有 Rigidbody
            Rigidbody rb = bottle.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = bottle.AddComponent<Rigidbody>();
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
            }
            item.itemType = ItemType.Bottle;
            item.itemName = itemName;
            item.liquidType = liquidType;
            item.liquidAmount = 750f;
            item.maxLiquidAmount = 750f;

            // 3. 處理碰撞器
            // 如果有 MeshCollider，確保它是 convex
            MeshCollider meshCollider = bottle.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.convex = true;
            }

            // 4. 禁用所有子物件的碰撞器（LOD 物件的碰撞器）
            BoxCollider[] childColliders = bottle.GetComponentsInChildren<BoxCollider>();
            foreach (BoxCollider col in childColliders)
            {
                if (col.gameObject != bottle)
                {
                    col.enabled = false;
                    Debug.Log($"禁用子物件碰撞器: {col.gameObject.name}");
                }
            }

            // 5. 標記場景為已修改
            EditorUtility.SetDirty(bottle);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(bottle.scene);
        }

        private void DiagnoseSelectedObject()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("請先選擇一個物件");
                return;
            }

            Debug.Log($"=== 診斷物件: {selected.name} ===");
            Debug.Log($"位置: {selected.transform.position}");
            Debug.Log($"旋轉: {selected.transform.eulerAngles}");
            Debug.Log($"縮放: {selected.transform.localScale}");

            // 檢查組件
            Rigidbody rb = selected.GetComponent<Rigidbody>();
            Debug.Log($"Rigidbody: {(rb != null ? "有" : "無")}");
            if (rb != null)
            {
                Debug.Log($"  - isKinematic: {rb.isKinematic}");
                Debug.Log($"  - useGravity: {rb.useGravity}");
                Debug.Log($"  - mass: {rb.mass}");
            }

            InteractableItem item = selected.GetComponent<InteractableItem>();
            Debug.Log($"InteractableItem: {(item != null ? "有" : "無")}");
            if (item != null)
            {
                Debug.Log($"  - itemType: {item.itemType}");
                Debug.Log($"  - itemName: {item.itemName}");
                Debug.Log($"  - liquidType: {item.liquidType}");
            }

            // 檢查碰撞器
            Collider[] colliders = selected.GetComponents<Collider>();
            Debug.Log($"父物件碰撞器數量: {colliders.Length}");
            foreach (Collider col in colliders)
            {
                Debug.Log($"  - {col.GetType().Name}: enabled={col.enabled}");
            }

            // 檢查子物件
            Transform[] children = selected.GetComponentsInChildren<Transform>();
            Debug.Log($"子物件數量: {children.Length - 1}"); // -1 排除自己

            Collider[] childColliders = selected.GetComponentsInChildren<Collider>();
            Debug.Log($"子物件碰撞器數量: {childColliders.Length - colliders.Length}");
            foreach (Collider col in childColliders)
            {
                if (col.gameObject != selected)
                {
                    Debug.Log($"  - {col.gameObject.name}: {col.GetType().Name}, enabled={col.enabled}");
                }
            }

            // 檢查 LODGroup
            LODGroup lodGroup = selected.GetComponent<LODGroup>();
            Debug.Log($"LODGroup: {(lodGroup != null ? "有" : "無")}");
        }
    }
}
