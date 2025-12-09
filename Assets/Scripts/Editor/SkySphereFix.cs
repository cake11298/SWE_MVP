using UnityEngine;
using UnityEditor;

public class SkySphereFix
{
    public static void Execute()
    {
        Debug.Log("Starting Sky Sphere Fix...");

        // 1. Create the Sphere
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Visual_Sky_Blocker";
        sphere.transform.position = Vector3.zero;
        sphere.transform.localScale = new Vector3(100f, 100f, 100f); // Huge

        // 2. Remove Collider
        Collider col = sphere.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);

        // 3. Create Material
        Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (unlitShader == null) unlitShader = Shader.Find("Unlit/Color");
        
        Material skyMat = new Material(unlitShader);
        skyMat.name = "SkySphere_Mat";
        skyMat.color = new Color(0.1f, 0.1f, 0.15f); // Dark Blue-Grey
        
        // For URP Unlit, color property might be _BaseColor
        if (skyMat.HasProperty("_BaseColor")) skyMat.SetColor("_BaseColor", new Color(0.1f, 0.1f, 0.15f));

        string matPath = "Assets/Materials/SkySphere_Mat.mat";
        AssetDatabase.CreateAsset(skyMat, matPath);

        // 4. Assign Material
        Renderer r = sphere.GetComponent<Renderer>();
        if (r != null)
        {
            r.sharedMaterial = skyMat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // Don't cast shadows
            r.receiveShadows = false;
        }

        // 5. Ensure Normals are inverted so we see it from inside? 
        // Actually, standard sphere normals point out. If we are inside, we won't see it due to backface culling.
        // We need to invert the mesh triangles or use a shader that turns off culling.
        
        // Let's just use a shader property if possible, or flip the mesh.
        MeshFilter mf = sphere.GetComponent<MeshFilter>();
        if (mf != null)
        {
            Mesh mesh = mf.sharedMesh;
            // Create a copy to not mess up the default sphere
            Mesh newMesh = Object.Instantiate(mesh);
            newMesh.name = "Inverted_Sphere";
            
            int[] triangles = newMesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i];
                triangles[i] = triangles[i + 2];
                triangles[i + 2] = temp;
            }
            newMesh.triangles = triangles;
            newMesh.RecalculateNormals();
            
            mf.sharedMesh = newMesh;
        }

        // 6. Reset Lighting Environment to ensure we have some light
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.3f); // Moderate Grey Ambient

        Debug.Log("Sky Sphere Created and Normals Inverted.");
    }
}
