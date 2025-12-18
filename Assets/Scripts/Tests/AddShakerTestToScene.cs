using UnityEngine;
using UnityEditor;

namespace BarSimulator.Tests
{
    public class AddShakerTestToScene
    {
        public static void Execute()
        {
            // 查找或創建測試管理器
            GameObject testManager = GameObject.Find("ShakerPouringTest");
            if (testManager == null)
            {
                testManager = new GameObject("ShakerPouringTest");
                testManager.AddComponent<ShakerPouringTest>();
                Debug.Log("Created ShakerPouringTest GameObject");
            }
            else
            {
                if (testManager.GetComponent<ShakerPouringTest>() == null)
                {
                    testManager.AddComponent<ShakerPouringTest>();
                    Debug.Log("Added ShakerPouringTest component");
                }
                else
                {
                    Debug.Log("ShakerPouringTest already exists");
                }
            }

            // 標記場景為已修改
            #if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            #endif
        }
    }
}
