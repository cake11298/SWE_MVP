using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

namespace BarSimulator.Editor
{
    /// <summary>
    /// 修复静态标记的可互动物品
    /// </summary>
    public class FixStaticInteractables
    {
        [MenuItem("Bar Simulator/Fix Static Interactables")]
        public static void FixStatic()
        {
            int count = 0;
            
            // 查找所有InteractableItem组件
            InteractableItem[] items = Object.FindObjectsOfType<InteractableItem>();
            
            foreach (InteractableItem item in items)
            {
                if (item.gameObject.isStatic)
                {
                    item.gameObject.isStatic = false;
                    count++;
                }
            }
            
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
            
            Debug.Log($"✓ 修复了 {count} 个静态可互动物品");
        }
    }
}
