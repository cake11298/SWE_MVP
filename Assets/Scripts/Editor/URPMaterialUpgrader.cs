using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class URPMaterialUpgrader : EditorWindow
{
    [MenuItem("Tools/Upgrade Materials to URP")]
    public static void ShowWindow()
    {
        GetWindow<URPMaterialUpgrader>("URP Upgrader");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Upgrade Project Materials to URP Lit"))
        {
            UpgradeMaterials();
        }
    }

    public static void UpgradeMaterials()
    {
        string[] folders = new string[] { "Assets/Materials", "Assets/TheBar/Source/Materials" };
        string[] guids = AssetDatabase.FindAssets("t:Material", folders);

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("Could not find URP Lit shader. Is URP installed?");
            return;
        }

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat.shader.name != urpLit.name)
            {
                // Preserve textures
                Texture mainTex = mat.GetTexture("_MainTex");
                Texture bumpMap = mat.GetTexture("_BumpMap");
                Texture emissionMap = mat.GetTexture("_EmissionMap");
                Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                
                // Switch shader
                mat.shader = urpLit;

                // Reassign properties
                if (mainTex != null) mat.SetTexture("_BaseMap", mainTex);
                if (bumpMap != null) mat.SetTexture("_BumpMap", bumpMap);
                if (emissionMap != null) 
                {
                    mat.SetTexture("_EmissionMap", emissionMap);
                    mat.EnableKeyword("_EMISSION");
                    mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    mat.SetColor("_EmissionColor", Color.white);
                }
                
                mat.SetColor("_BaseColor", color);

                EditorUtility.SetDirty(mat);
                count++;
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Upgraded {count} materials to URP Lit.");
    }
}
