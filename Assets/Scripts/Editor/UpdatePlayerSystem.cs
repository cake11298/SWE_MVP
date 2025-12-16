using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

namespace BarSimulator.Editor
{
    /// <summary>
    /// 更新玩家系统
    /// </summary>
    public class UpdatePlayerSystem
    {
        [MenuItem("Bar Simulator/Update Player System")]
        public static void Update()
        {
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("找不到Player物件！");
                return;
            }
            
            // 移除旧的互动系统
            ItemInteractionSystem oldSystem = player.GetComponent<ItemInteractionSystem>();
            if (oldSystem != null)
            {
                Object.DestroyImmediate(oldSystem);
                Debug.Log("✓ 移除了旧的ItemInteractionSystem");
            }
            
            // 添加新的互动系统
            ImprovedInteractionSystem newSystem = player.GetComponent<ImprovedInteractionSystem>();
            if (newSystem == null)
            {
                newSystem = player.AddComponent<ImprovedInteractionSystem>();
                Debug.Log("✓ 添加了新的ImprovedInteractionSystem");
            }
            
            // 调整玩家控制器速度
            SimplePlayerController controller = player.GetComponent<SimplePlayerController>();
            if (controller != null)
            {
                // 使用反射设置私有字段
                var moveSpeedField = typeof(SimplePlayerController).GetField("moveSpeed", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (moveSpeedField != null)
                {
                    moveSpeedField.SetValue(controller, 3f);
                    Debug.Log("✓ 调整了移动速度为3");
                }
            }
            
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
            
            Debug.Log("=== 玩家系统更新完成！===");
        }
    }
}
