using UnityEditor;
using UnityEngine;

public class SetSceneView
{
    public static void Execute()
    {
        SetView();
    }

    public static void SetView()
    {
        SceneView view = SceneView.lastActiveSceneView;
        if (view == null)
        {
            if (SceneView.sceneViews.Count > 0)
                view = SceneView.sceneViews[0] as SceneView;
            else
                view = SceneView.CreateWindow<SceneView>();
        }

        if (view != null)
        {
            // Position the camera inside the room
            // Room is Z: -10 to 10. Bar is at -3.
            // Position at Z=5 (Lounge area) looking at Z=-3 (Bar).
            
            // Force position and rotation directly
            view.pivot = new Vector3(0, 1.7f, -3f);
            view.rotation = Quaternion.Euler(10, 180, 0);
            view.size = 10f;
            
            // This sets the camera position implicitly based on pivot, rotation and size (distance)
            // To be at Z=5 looking at Z=-3 (dist 8), size should be around 8.
            
            view.Repaint();
        }
    }

    private static Transform playerTransform()
    {
        GameObject p = GameObject.Find("Player");
        return p != null ? p.transform : null;
    }
}
