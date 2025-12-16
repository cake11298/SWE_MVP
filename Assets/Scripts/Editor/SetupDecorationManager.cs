using UnityEngine;
using UnityEditor;
using BarSimulator.Systems;

namespace BarSimulator.Editor
{
    /// <summary>
    /// Editor script to set up the DecorationManager in the scene
    /// </summary>
    public class SetupDecorationManager : MonoBehaviour
    {
        [MenuItem("Bar Simulator/Setup/Setup Decoration Manager")]
        public static void Setup()
        {
            Debug.Log("Setting up DecorationManager...");

            // Find or create DecorationManager
            DecorationManager manager = FindFirstObjectByType<DecorationManager>();
            if (manager == null)
            {
                GameObject managerObj = new GameObject("DecorationManager");
                manager = managerObj.AddComponent<DecorationManager>();
                Debug.Log("Created new DecorationManager");
            }

            // Find speaker objects
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            System.Collections.Generic.List<GameObject> speakers = new System.Collections.Generic.List<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("SM_Speakers"))
                {
                    speakers.Add(obj);
                    Debug.Log($"Found speaker: {obj.name}");
                }
            }

            // Set speaker objects using reflection
            if (speakers.Count > 0)
            {
                manager.SetSpeakerObjects(speakers.ToArray());
                Debug.Log($"Assigned {speakers.Count} speaker objects to DecorationManager");
            }
            else
            {
                Debug.LogWarning("No speaker objects found in the scene");
            }

            // Mark scene as dirty
            EditorUtility.SetDirty(manager);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log("DecorationManager setup complete!");
        }
    }
}
