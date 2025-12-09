using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class FloorAndSkyboxFixer
{
    public static void Execute()
    {
        Debug.Log("Starting Floor and Skybox Fix...");

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) urpLit = Shader.Find("Universal Render Pipeline/Simple Lit");

        // 1. Fix Floor Material
        string floorMatPath = "Assets/TheBar/Source/Materials/MI_FloorTiles1.mat";
        Material floorMat = AssetDatabase.LoadAssetAtPath<Material>(floorMatPath);

        if (floorMat != null)
        {
            if (floorMat.shader.name != urpLit.name)
            {
                Debug.Log($"Converting Floor Material '{floorMat.name}' from {floorMat.shader.name} to URP Lit");
                
                // Save properties
                Texture mainTex = null;
                if (floorMat.HasProperty("_MainTex")) mainTex = floorMat.GetTexture("_MainTex");
                else if (floorMat.HasProperty("_BaseMap")) mainTex = floorMat.GetTexture("_BaseMap");

                floorMat.shader = urpLit;
                
                if (mainTex != null)
                {
                    floorMat.SetTexture("_BaseMap", mainTex);
                    floorMat.SetTexture("_MainTex", mainTex);
                }
                EditorUtility.SetDirty(floorMat);
            }
        }
        else
        {
            Debug.LogError($"Could not find material at {floorMatPath}");
        }

        // 2. Assign Floor Material to Floor Object
        GameObject floorObj = GameObject.Find("Environment/Floor");
        if (floorObj != null)
        {
            Renderer renderer = floorObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = floorMat;
                Debug.Log("Assigned MI_FloorTiles1 to Environment/Floor");
            }
        }
        else
        {
            Debug.LogError("Could not find Environment/Floor object");
        }

        // 3. Fix Skybox
        Material skyboxMat = RenderSettings.skybox;
        if (skyboxMat == null || skyboxMat.shader.name == "Hidden/InternalErrorShader" || skyboxMat.shader.name.Contains("Error"))
        {
            Debug.Log("Skybox is missing or broken. Attempting to fix...");
            
            // Try to find the ProceduralSkybox_Fixed.mat mentioned in file list
            string fixedSkyboxPath = "Assets/Settings/ProceduralSkybox_Fixed.mat";
            Material fixedSkybox = AssetDatabase.LoadAssetAtPath<Material>(fixedSkyboxPath);
            
            if (fixedSkybox != null)
            {
                // Ensure it uses a valid shader (Procedural Skybox usually works, but let's check)
                if (fixedSkybox.shader.name.Contains("Error"))
                {
                     Shader skyboxShader = Shader.Find("Skybox/Procedural");
                     if (skyboxShader != null) fixedSkybox.shader = skyboxShader;
                }
                
                RenderSettings.skybox = fixedSkybox;
                Debug.Log($"Assigned Skybox: {fixedSkybox.name}");
            }
            else
            {
                // Create a default skybox if needed
                Debug.LogWarning("Could not find ProceduralSkybox_Fixed.mat. Creating a new default skybox.");
                Material newSkybox = new Material(Shader.Find("Skybox/Procedural"));
                newSkybox.name = "DefaultSkybox";
                AssetDatabase.CreateAsset(newSkybox, "Assets/Settings/DefaultSkybox.mat");
                RenderSettings.skybox = newSkybox;
            }
        }
        else
        {
            Debug.Log($"Current Skybox is valid: {skyboxMat.name} ({skyboxMat.shader.name})");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Floor and Skybox Fix Complete.");
    }
}
