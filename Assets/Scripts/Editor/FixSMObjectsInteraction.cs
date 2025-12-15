using UnityEngine;
using UnityEditor;
using BarSimulator;
using BarSimulator.Player;

namespace BarSimulator.Editor
{
    /// <summary>
    /// 修复 SM 开头物件的互动问题
    /// </summary>
    public class FixSMObjectsInteraction : EditorWindow
    {
        [MenuItem("Tools/Fix SM Objects Interaction")]
        public static void ShowWindow()
        {
            GetWindow<FixSMObjectsInteraction>("Fix SM Objects");
        }

        private void OnGUI()
        {
            GUILayout.Label("Fix SM Objects Interaction", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("This will fix all SM_* objects in the scene:");
            GUILayout.Label("- Ensure proper Rigidbody settings");
            GUILayout.Label("- Add/fix Colliders");
            GUILayout.Label("- Add StaticProp component");
            GUILayout.Label("- Add InteractableItem component");
            GUILayout.Space(10);

            if (GUILayout.Button("Fix All SM Objects", GUILayout.Height(40)))
            {
                FixAllSMObjects();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Fix Selected Objects", GUILayout.Height(30)))
            {
                FixSelectedObjects();
            }
        }

        private static void FixAllSMObjects()
        {
            // 查找所有 SM 开头的物件
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int fixedCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.StartsWith("SM_"))
                {
                    if (FixObject(obj))
                    {
                        fixedCount++;
                    }
                }
            }

            Debug.Log($"[FixSMObjects] Fixed {fixedCount} SM objects");
            EditorUtility.DisplayDialog("Complete", $"Fixed {fixedCount} SM objects", "OK");
        }

        private static void FixSelectedObjects()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "No objects selected", "OK");
                return;
            }

            int fixedCount = 0;
            foreach (GameObject obj in selectedObjects)
            {
                if (FixObject(obj))
                {
                    fixedCount++;
                }
            }

            Debug.Log($"[FixSMObjects] Fixed {fixedCount} selected objects");
            EditorUtility.DisplayDialog("Complete", $"Fixed {fixedCount} objects", "OK");
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
                Debug.Log($"[FixSMObjects] Added Rigidbody to {obj.name}");
            }

            // 设置 Rigidbody 属性
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
                MeshFilter meshFilter = obj.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    // 添加 BoxCollider 并根据 mesh 边界设置大小
                    BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
                    Bounds bounds = meshFilter.sharedMesh.bounds;
                    
                    // 考虑子物件的 transform
                    Vector3 scale = meshFilter.transform.lossyScale;
                    boxCollider.center = Vector3.zero;
                    boxCollider.size = new Vector3(
                        bounds.size.x * scale.x,
                        bounds.size.y * scale.y,
                        bounds.size.z * scale.z
                    );
                    
                    modified = true;
                    Debug.Log($"[FixSMObjects] Added BoxCollider to {obj.name}");
                }
                else
                {
                    // 如果没有 mesh，添加默认大小的 BoxCollider
                    BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
                    boxCollider.center = Vector3.zero;
                    boxCollider.size = Vector3.one * 0.1f;
                    modified = true;
                    Debug.Log($"[FixSMObjects] Added default BoxCollider to {obj.name}");
                }
            }
            else
            {
                // 确保 Collider 不是 trigger
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
                Debug.Log($"[FixSMObjects] Added StaticProp to {obj.name}");
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
                else if (obj.name.Contains("Glass") || obj.name.Contains("Cup") || obj.name.Contains("Mug"))
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
                Debug.Log($"[FixSMObjects] Added InteractableItem to {obj.name}");
            }

            if (modified)
            {
                EditorUtility.SetDirty(obj);
            }

            return modified;
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
}
