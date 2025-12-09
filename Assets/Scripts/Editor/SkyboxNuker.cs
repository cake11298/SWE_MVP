using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class SkyboxNuker
{
    public static void Execute()
    {
        Debug.Log("Starting Skybox Nuke...");

        // 1. Remove Skybox from RenderSettings
        RenderSettings.skybox = null;
        Debug.Log("Set RenderSettings.skybox to null.");

        // 2. Set Ambient Mode to Flat (Color)
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientSkyColor = new Color(0.2f, 0.2f, 0.2f); // Dark Grey
        Debug.Log("Set Ambient Mode to Flat (Dark Grey).");

        // 3. Configure Main Camera to Solid Color
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.1f, 0.1f, 0.1f); // Almost Black
            Debug.Log("Set Main Camera ClearFlags to SolidColor.");
        }

        // 4. Force Scene View Repaint and Update
        SceneView.RepaintAll();
        
        AssetDatabase.SaveAssets();
        Debug.Log("Skybox Nuke Complete.");
    }
}
