using UnityEngine;
using UnityEditor;

public class SkyboxTroubleshooter
{
    public static void Execute()
    {
        Debug.Log("Starting Skybox Troubleshooting...");

        // 1. Check Shader
        Shader skyShader = Shader.Find("Skybox/Procedural");
        if (skyShader == null)
        {
            Debug.LogError("Shader 'Skybox/Procedural' not found!");
            // List some available shaders
            /*
            foreach (var s in ShaderUtil.GetAllShaderInfo())
            {
                if (s.name.Contains("Skybox")) Debug.Log("Found Shader: " + s.name);
            }
            */
        }
        else
        {
            Debug.Log("Shader 'Skybox/Procedural' found.");
        }

        // 2. Check Current Skybox Material
        Material currentSkybox = RenderSettings.skybox;
        if (currentSkybox != null)
        {
            Debug.Log($"Current Skybox Material: {currentSkybox.name}, Shader: {currentSkybox.shader.name}");
            if (currentSkybox.shader.name == "Hidden/InternalErrorShader" || currentSkybox.shader.name.Contains("Error"))
            {
                Debug.LogError("Current Skybox is using Error shader!");
            }
        }
        else
        {
            Debug.Log("RenderSettings.skybox is null.");
        }

        // 3. Force Solid Color on Camera as fallback
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Debug.Log($"Main Camera Clear Flags: {mainCam.clearFlags}");
            // If skybox is broken, set to Solid Color temporarily to remove pink
            // mainCam.clearFlags = CameraClearFlags.SolidColor;
            // mainCam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            // Debug.Log("Set Main Camera to Solid Color (Dark Grey) to hide pink skybox.");
        }

        // 4. Try to create a new material with a different shader if Procedural failed, or re-create it
        if (skyShader != null)
        {
            Material newSky = new Material(skyShader);
            newSky.name = "Troubleshoot_Skybox";
            newSky.SetColor("_SkyTint", Color.blue);
            newSky.SetColor("_GroundColor", Color.gray);
            newSky.SetFloat("_Exposure", 1.0f);
            
            string path = "Assets/Settings/Troubleshoot_Skybox.mat";
            AssetDatabase.CreateAsset(newSky, path);
            RenderSettings.skybox = newSky;
            Debug.Log("Assigned 'Troubleshoot_Skybox' to RenderSettings.");
        }
    }
}
