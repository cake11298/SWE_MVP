using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;

public class CreateEventSystem : MonoBehaviour
{
    public static void Execute()
    {
        // Check if EventSystem already exists
        EventSystem existingEventSystem = FindObjectOfType<EventSystem>();
        
        if (existingEventSystem != null)
        {
            Debug.Log("EventSystem already exists");
            return;
        }
        
        // Create EventSystem
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();
        
        Debug.Log("Created EventSystem");
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }
}
