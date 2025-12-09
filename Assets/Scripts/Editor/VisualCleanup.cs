using UnityEngine;
using UnityEditor;

public class VisualCleanup
{
    public static void Execute()
    {
        Debug.Log("Starting Visual Cleanup...");

        // 1. Fix Skybox by falling back to Solid Color (removes pink)
        // Setting skybox to null usually reverts to default, which might be broken.
        // So we set the camera to Solid Color.
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.15f, 0.15f, 0.2f); // Dark Blue-Grey
            Debug.Log("Set Main Camera to Solid Color.");
        }

        // Also try to set the Scene View to use a default skybox or solid color if possible
        // We can't easily control Scene View via script without EditorWindow access, 
        // but setting RenderSettings.skybox to null might help.
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.5f, 0.5f);
        Debug.Log("Cleared RenderSettings.skybox to remove pink artifact.");

        // 2. Fix NPC Material - Make it distinct (Red)
        string npcMatPath = "Assets/Materials/NPC_Placeholder_Mat.mat";
        Material npcMat = AssetDatabase.LoadAssetAtPath<Material>(npcMatPath);
        if (npcMat == null)
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Universal Render Pipeline/Simple Lit");
            npcMat = new Material(urpLit);
            AssetDatabase.CreateAsset(npcMat, npcMatPath);
        }

        npcMat.SetColor("_BaseColor", Color.red);
        npcMat.SetColor("_Color", Color.red);
        Debug.Log("Updated NPC Material to Red.");

        GameObject npc = GameObject.Find("NPC_Casual_Placeholder");
        if (npc != null)
        {
            Renderer r = npc.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = npcMat;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Visual Cleanup Complete.");
    }
}
