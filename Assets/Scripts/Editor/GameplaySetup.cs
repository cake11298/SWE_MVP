using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

namespace BarSimulator.Editor
{
    /// <summary>
    /// 游戏玩法设置工具
    /// 用于快速设置玩家、互动物品和碰撞体
    /// </summary>
    public class GameplaySetup : EditorWindow
    {
        [MenuItem("Bar Simulator/Gameplay Setup")]
        public static void ShowWindow()
        {
            GetWindow<GameplaySetup>("Gameplay Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("游戏玩法设置工具", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("1. 创建玩家角色", GUILayout.Height(30)))
            {
                CreatePlayer();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("2. 标记所有酒瓶为可互动", GUILayout.Height(30)))
            {
                MarkBottlesAsInteractable();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("3. 标记所有玻璃杯为可互动", GUILayout.Height(30)))
            {
                MarkGlassesAsInteractable();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("4. 为墙壁添加碰撞体", GUILayout.Height(30)))
            {
                AddWallColliders();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("5. 为吧台/桌子添加碰撞体", GUILayout.Height(30)))
            {
                AddBarCounterColliders();
            }

            GUILayout.Space(10);
            GUILayout.Label("一键设置", EditorStyles.boldLabel);

            if (GUILayout.Button("执行全部设置", GUILayout.Height(40)))
            {
                CreatePlayer();
                MarkBottlesAsInteractable();
                MarkGlassesAsInteractable();
                AddWallColliders();
                AddBarCounterColliders();
                Debug.Log("全部设置完成！");
            }
        }

        private void CreatePlayer()
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
                Debug.Log("玩家已存在！");
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
            player.AddComponent<SimplePlayerController>();

            // 添加互动系统
            player.AddComponent<ItemInteractionSystem>();

            // 将相机设为玩家的子物件
            GameObject cameraHolder = new GameObject("CameraHolder");
            cameraHolder.transform.SetParent(player.transform);
            cameraHolder.transform.localPosition = new Vector3(0, 1.6f, 0);
            cameraHolder.transform.localRotation = Quaternion.identity;

            mainCamera.transform.SetParent(cameraHolder.transform);
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.Euler(mainCamera.transform.eulerAngles.x, 0, 0);

            // 标记场景为已修改
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log("玩家创建完成！");
            Selection.activeGameObject = player;
        }

        private void MarkBottlesAsInteractable()
        {
            int count = 0;
            
            // 查找所有包含"Bottle"或"bottle"的GameObject
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
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

            Debug.Log($"标记了 {count} 个酒瓶为可互动物品");
        }

        private void MarkGlassesAsInteractable()
        {
            int count = 0;

            // 查找所有包含"Glass"的GameObject
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("glass") || obj.name.ToLower().Contains("cup"))
                {
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

            Debug.Log($"标记了 {count} 个玻璃杯为可互动物品");
        }

        private void AddWallColliders()
        {
            int count = 0;

            // 查找所有包含"Wall"的GameObject
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("wall") || 
                    obj.name.ToLower().Contains("column") ||
                    obj.name.ToLower().Contains("door"))
                {
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

            Debug.Log($"为 {count} 个墙壁添加了碰撞体");
        }

        private void AddBarCounterColliders()
        {
            int count = 0;

            // 查找所有包含"Bar"、"Table"、"Counter"的GameObject
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                string lowerName = obj.name.ToLower();
                if (lowerName.Contains("bar") || 
                    lowerName.Contains("table") || 
                    lowerName.Contains("counter") ||
                    lowerName.Contains("builtin") ||
                    lowerName.Contains("shelve"))
                {
                    // 排除小物件（酒瓶、杯子等）
                    if (lowerName.Contains("bottle") || lowerName.Contains("glass") || 
                        lowerName.Contains("cup") || lowerName.Contains("chair"))
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

            Debug.Log($"为 {count} 个吧台/桌子添加了碰撞体");
        }
    }
}
