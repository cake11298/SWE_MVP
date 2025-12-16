using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

namespace BarSimulator.Editor
{
    /// <summary>
    /// 自动游戏玩法设置
    /// </summary>
    public class AutoGameplaySetup
    {
        [MenuItem("Bar Simulator/Auto Setup All")]
        public static void SetupAll()
        {
            Debug.Log("开始自动设置游戏玩法...");
            
            CreatePlayer();
            MarkBottlesAsInteractable();
            MarkGlassesAsInteractable();
            AddWallColliders();
            AddBarCounterColliders();
            
            Debug.Log("=== 自动设置完成！===");
            Debug.Log("提示：");
            Debug.Log("- 使用 WASD 移动");
            Debug.Log("- 使用鼠标控制视角");
            Debug.Log("- 靠近物品（2米内）会显示黄色高亮");
            Debug.Log("- 按 E 键拾取/放下物品");
            Debug.Log("- 手持酒瓶时，对准杯子（1.5米内）按住右键倒酒");
        }

        private static void CreatePlayer()
        {
            // 查找现有的Camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("找不到主相机！");
                return;
            }

            // 检查是否已经有玩家
            if (mainCamera.GetComponentInParent<SimplePlayerController>() != null)
            {
                Debug.Log("玩家已存在，跳过创建。");
                return;
            }

            // 创建玩家GameObject
            GameObject player = new GameObject("Player");
            player.transform.position = mainCamera.transform.position;
            player.transform.rotation = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0);

            // 添加CharacterController
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0, 0.9f, 0);

            // 添加玩家控制器
            SimplePlayerController playerController = player.AddComponent<SimplePlayerController>();

            // 添加互动系统
            ItemInteractionSystem interactionSystem = player.AddComponent<ItemInteractionSystem>();

            // 将相机设为玩家的子物件
            GameObject cameraHolder = new GameObject("CameraHolder");
            cameraHolder.transform.SetParent(player.transform);
            cameraHolder.transform.localPosition = new Vector3(0, 1.6f, 0);
            cameraHolder.transform.localRotation = Quaternion.identity;

            mainCamera.transform.SetParent(cameraHolder.transform);
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.Euler(mainCamera.transform.eulerAngles.x, 0, 0);

            // 使用反射设置私有字段
            var cameraField = typeof(SimplePlayerController).GetField("cameraTransform", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (cameraField != null)
            {
                cameraField.SetValue(playerController, cameraHolder.transform);
            }

            // 标记场景为已修改
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log("✓ 玩家创建完成！");
            Selection.activeGameObject = player;
        }

        private static void MarkBottlesAsInteractable()
        {
            int count = 0;
            
            // 查找所有包含"Bottle"或"bottle"的GameObject
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("bottle") || obj.name.ToLower().Contains("whiskey"))
                {
                    // 检查是否已经有InteractableItem组件
                    InteractableItem item = obj.GetComponent<InteractableItem>();
                    if (item == null)
                    {
                        item = obj.AddComponent<InteractableItem>();
                        item.itemType = ItemType.Bottle;
                        item.itemName = obj.name;
                        
                        // 根据名称设置液体类型
                        if (obj.name.ToLower().Contains("wine"))
                            item.liquidType = LiquidType.Wine;
                        else if (obj.name.ToLower().Contains("whiskey"))
                            item.liquidType = LiquidType.Whiskey;
                        else if (obj.name.ToLower().Contains("vodka"))
                            item.liquidType = LiquidType.Vodka;
                        else
                            item.liquidType = LiquidType.Vodka; // 默认

                        // 添加Rigidbody（如果没有）
                        if (obj.GetComponent<Rigidbody>() == null)
                        {
                            Rigidbody rb = obj.AddComponent<Rigidbody>();
                            rb.mass = 0.5f;
                        }

                        // 确保有Collider
                        if (obj.GetComponent<Collider>() == null)
                        {
                            BoxCollider collider = obj.AddComponent<BoxCollider>();
                        }

                        count++;
                    }
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log($"✓ 标记了 {count} 个酒瓶为可互动物品");
        }

        private static void MarkGlassesAsInteractable()
        {
            int count = 0;

            // 查找所有包含"Glass"的GameObject
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("glass") || 
                    obj.name.ToLower().Contains("cup") ||
                    obj.name.ToLower().Contains("martini") ||
                    obj.name.ToLower().Contains("coctail"))
                {
                    // 排除LOD对象
                    if (obj.name.Contains("LOD"))
                        continue;

                    // 检查是否已经有InteractableItem组件
                    InteractableItem item = obj.GetComponent<InteractableItem>();
                    if (item == null)
                    {
                        item = obj.AddComponent<InteractableItem>();
                        item.itemType = ItemType.Glass;
                        item.itemName = obj.name;

                        // 添加Rigidbody（如果没有）
                        if (obj.GetComponent<Rigidbody>() == null)
                        {
                            Rigidbody rb = obj.AddComponent<Rigidbody>();
                            rb.mass = 0.2f;
                        }

                        // 确保有Collider
                        if (obj.GetComponent<Collider>() == null)
                        {
                            BoxCollider collider = obj.AddComponent<BoxCollider>();
                        }

                        count++;
                    }
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log($"✓ 标记了 {count} 个玻璃杯为可互动物品");
        }

        private static void AddWallColliders()
        {
            int count = 0;

            // 查找所有包含"Wall"的GameObject
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("wall") || 
                    obj.name.ToLower().Contains("column") ||
                    obj.name.ToLower().Contains("door"))
                {
                    // 排除LOD对象
                    if (obj.name.Contains("LOD"))
                        continue;

                    // 检查是否已经有Collider
                    if (obj.GetComponent<Collider>() == null)
                    {
                        // 尝试从MeshFilter获取网格信息
                        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                        if (meshFilter != null && meshFilter.sharedMesh != null)
                        {
                            // 添加MeshCollider
                            MeshCollider collider = obj.AddComponent<MeshCollider>();
                            collider.convex = false;
                            count++;
                        }
                        else
                        {
                            // 如果没有网格，添加BoxCollider
                            obj.AddComponent<BoxCollider>();
                            count++;
                        }
                    }
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log($"✓ 为 {count} 个墙壁添加了碰撞体");
        }

        private static void AddBarCounterColliders()
        {
            int count = 0;

            // 查找所有包含"Bar"、"Table"、"Counter"的GameObject
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                string lowerName = obj.name.ToLower();
                if (lowerName.Contains("bar") || 
                    lowerName.Contains("table") || 
                    lowerName.Contains("counter") ||
                    lowerName.Contains("builtin") ||
                    lowerName.Contains("shelve") ||
                    lowerName.Contains("block"))
                {
                    // 排除小物件（酒瓶、杯子等）
                    if (lowerName.Contains("bottle") || lowerName.Contains("glass") || 
                        lowerName.Contains("cup") || lowerName.Contains("chair") ||
                        lowerName.Contains("lod"))
                        continue;

                    // 检查是否已经有Collider
                    if (obj.GetComponent<Collider>() == null)
                    {
                        // 尝试从MeshFilter获取网格信息
                        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                        if (meshFilter != null && meshFilter.sharedMesh != null)
                        {
                            // 添加MeshCollider
                            MeshCollider collider = obj.AddComponent<MeshCollider>();
                            collider.convex = false;
                            count++;
                        }
                        else
                        {
                            // 如果没有网格，添加BoxCollider
                            obj.AddComponent<BoxCollider>();
                            count++;
                        }
                    }
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log($"✓ 为 {count} 个吧台/桌子添加了碰撞体");
        }
    }
}
