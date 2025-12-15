using UnityEditor;
using UnityEngine;

public class FocusOnHighBallGlass : EditorWindow
{
    [MenuItem("Tools/Focus On HighBall Glass")]
    static void Focus()
    {
        // Find the HighBallGlass object
        GameObject highBallGlass = GameObject.Find("HighBallGlass");
        
        if (highBallGlass != null)
        {
            // Select it
            Selection.activeGameObject = highBallGlass;
            
            // Get the Scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                // Set camera position to view the bar area from a good angle
                sceneView.pivot = new Vector3(-2.5f, 1.5f, -7.5f);
                sceneView.rotation = Quaternion.Euler(20f, 45f, 0f);
                sceneView.size = 3f;
                sceneView.Repaint();
            }
            
            Debug.Log("Focused on HighBallGlass at position: " + highBallGlass.transform.position);
        }
        else
        {
            Debug.LogWarning("HighBallGlass not found in scene!");
        }
    }
}
