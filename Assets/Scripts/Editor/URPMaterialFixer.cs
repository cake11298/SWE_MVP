using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class URPMaterialFixer
{
    public static void Execute()
    {
        Debug.Log("Starting URP Material Fix...");

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            urpLit = Shader.Find("Universal Render Pipeline/Simple Lit");
        }
        
        if (urpLit == null)
        {
            Debug.LogError("Could not find URP Lit or Simple Lit shader. Ensure URP is installed.");
            return;
        }

        Debug.Log($"Using Shader: {urpLit.name}");

        Renderer[] renderers = Object.FindObjectsOfType<Renderer>();
        int fixedCount = 0;

        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat == null) continue;

                bool needsFix = false;
                if (mat.shader.name == "Standard" || mat.shader.name == "Hidden/InternalErrorShader" || mat.shader.name.Contains("Error"))
                {
                    needsFix = true;
                }

                // Specific check for "TheBar" assets if they are using Standard
                if (renderer.name.Contains("Bar") || renderer.transform.parent?.name.Contains("Bar") == true)
                {
                     if (mat.shader.name != urpLit.name)
                     {
                         needsFix = true;
                     }
                }

                if (needsFix)
                {
                    Debug.Log($"Fixing material '{mat.name}' on object '{renderer.name}' (Old Shader: {mat.shader.name})");

                    // Preserve properties
                    Texture mainTex = null;
                    Color color = Color.white;

                    if (mat.HasProperty("_MainTex")) mainTex = mat.GetTexture("_MainTex");
                    if (mat.HasProperty("_Color")) color = mat.GetColor("_Color");
                    
                    // Also check URP properties in case it was partially converted
                    if (mainTex == null && mat.HasProperty("_BaseMap")) mainTex = mat.GetTexture("_BaseMap");
                    if (mat.HasProperty("_BaseColor")) color = mat.GetColor("_BaseColor");

                    // Assign new shader
                    mat.shader = urpLit;

                    // Re-assign properties
                    if (mainTex != null)
                    {
                        mat.SetTexture("_BaseMap", mainTex);
                        mat.SetTexture("_MainTex", mainTex); // Set both just in case
                    }
                    
                    mat.SetColor("_BaseColor", color);
                    mat.SetColor("_Color", color);

                    fixedCount++;
                }
            }
        }

        Debug.Log($"Fixed {fixedCount} materials.");

        // Lighting Check
        Light[] lights = Object.FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                Debug.Log($"Checking Directional Light: {light.name}");
                if (light.intensity <= 0)
                {
                    light.intensity = 1.0f;
                    Debug.Log("Fixed Directional Light intensity.");
                }
            }
        }
        
        Debug.Log("URP Material Fix Complete.");
    }
}
