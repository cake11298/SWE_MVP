using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class FinalVisualFixer
{
    public static void Execute()
    {
        Debug.Log("Starting Final Visual Fix...");

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) urpLit = Shader.Find("Universal Render Pipeline/Simple Lit");

        // 1. Fix Skybox - Create a brand new one to be sure
        Material newSkybox = new Material(Shader.Find("Skybox/Procedural"));
        newSkybox.name = "Fresh_URP_Skybox";
        newSkybox.SetFloat("_SunSize", 0.04f);
        newSkybox.SetFloat("_AtmosphereThickness", 1.0f);
        newSkybox.SetColor("_SkyTint", new Color(0.5f, 0.5f, 0.5f));
        newSkybox.SetColor("_GroundColor", new Color(0.369f, 0.349f, 0.341f));
        newSkybox.SetFloat("_Exposure", 1.3f);
        
        string skyboxPath = "Assets/Settings/Fresh_URP_Skybox.mat";
        AssetDatabase.CreateAsset(newSkybox, skyboxPath);
        RenderSettings.skybox = newSkybox;
        Debug.Log("Created and assigned new Skybox: Fresh_URP_Skybox");

        // 2. Fix NPC
        GameObject npc = GameObject.Find("NPC_Casual_Placeholder");
        if (npc != null)
        {
            Renderer r = npc.GetComponent<Renderer>();
            if (r != null)
            {
                // Create a simple skin material for the placeholder
                Material npcMat = new Material(urpLit);
                npcMat.name = "NPC_Placeholder_Mat";
                npcMat.SetColor("_BaseColor", new Color(0.8f, 0.6f, 0.5f)); // Skin tone-ish
                npcMat.SetFloat("_Smoothness", 0.2f);
                
                string npcMatPath = "Assets/Materials/NPC_Placeholder_Mat.mat";
                AssetDatabase.CreateAsset(npcMat, npcMatPath);
                
                r.sharedMaterial = npcMat;
                Debug.Log("Assigned new material to NPC_Casual_Placeholder");
            }
        }

        // 3. Ensure Lighting is updated
        DynamicGI.UpdateEnvironment();
        
        AssetDatabase.SaveAssets();
        Debug.Log("Final Visual Fix Complete.");
    }
}
