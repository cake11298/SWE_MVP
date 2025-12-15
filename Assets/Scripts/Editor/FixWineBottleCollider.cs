using UnityEngine;
using UnityEditor;

public class FixWineBottleCollider : MonoBehaviour
{
    public static void Execute()
    {
        GameObject wineBottle = GameObject.Find("SM_WineBottle17");
        
        if (wineBottle == null)
        {
            Debug.LogError("SM_WineBottle17 not found!");
            return;
        }
        
        // Remove MeshCollider
        MeshCollider meshCollider = wineBottle.GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            DestroyImmediate(meshCollider);
            Debug.Log("Removed MeshCollider from SM_WineBottle17");
        }
        
        // Add BoxCollider with appropriate size for a wine bottle
        BoxCollider boxCollider = wineBottle.AddComponent<BoxCollider>();
        
        // Wine bottle typical dimensions (in local space, before scale)
        // Height: ~0.3, Width/Depth: ~0.08
        boxCollider.size = new Vector3(0.08f, 0.3f, 0.08f);
        boxCollider.center = new Vector3(0f, 0.15f, 0f); // Center at half height
        
        Debug.Log("Added BoxCollider to SM_WineBottle17");
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }
}
