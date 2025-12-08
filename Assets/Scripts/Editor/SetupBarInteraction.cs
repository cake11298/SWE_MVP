using UnityEngine;
using UnityEditor;
using BarSimulator.Objects;

public class SetupBarInteraction : MonoBehaviour
{
    public static void Setup()
    {
        // 1. Bar Counter Collider
        GameObject barCounter = GameObject.Find("Environment/BarCounter_3/SM_BlockSimple2m");
        if (barCounter != null)
        {
            if (barCounter.GetComponent<Collider>() == null)
            {
                barCounter.AddComponent<BoxCollider>();
                Debug.Log("Added BoxCollider to Bar Counter");
            }
        }
        else
        {
            Debug.LogWarning("Bar Counter not found");
        }

        // 2. Place Glasses
        GameObject glassPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Glass.prefab");
        if (glassPrefab != null)
        {
            // Place 2 glasses
            // Bar top is roughly at y=1.115 based on previous scene builder, 
            // but get_game_object_info says max y is 1.8.
            // Let's use 1.15 as a safe bet and let physics settle, or check bounds.
            // Bounds max y is 1.8. That seems high for a bar counter (usually 1.1m).
            // Maybe the pivot is different or scale.
            // Let's try 1.2f.
            SpawnGlass(glassPrefab, new Vector3(-0.5f, 1.2f, -3.2f));
            SpawnGlass(glassPrefab, new Vector3(0.5f, 1.2f, -3.2f));
        }
        else
        {
            Debug.LogError("Glass prefab not found");
        }

        // 3. Setup Bottles on Shelves
        GameObject environment = GameObject.Find("Environment");
        if (environment != null)
        {
            SetupBottlesRecursive(environment.transform);
        }
    }

    private static void SpawnGlass(GameObject prefab, Vector3 position)
    {
        GameObject glass = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        glass.transform.position = position;
        glass.transform.rotation = Quaternion.identity;
        glass.name = "BarGlass";
        Debug.Log($"Spawned glass at {position}");
    }

    private static void SetupBottlesRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Identify bottles by name
            if (child.name.Contains("Bottle") && !child.name.Contains("LOD") && !child.name.Contains("PourPoint"))
            {
                SetupBottle(child.gameObject);
            }
            SetupBottlesRecursive(child);
        }
    }

    private static void SetupBottle(GameObject go)
    {
        // Add Bottle script
        Bottle bottle = go.GetComponent<Bottle>();
        if (bottle == null)
        {
            bottle = go.AddComponent<Bottle>();
            bottle.SetLiquor("whiskey"); // Default
        }

        // Replace MeshCollider with CapsuleCollider for better physics
        MeshCollider mc = go.GetComponent<MeshCollider>();
        if (mc != null)
        {
            DestroyImmediate(mc);
        }
        
        CapsuleCollider cc = go.GetComponent<CapsuleCollider>();
        if (cc == null)
        {
            cc = go.AddComponent<CapsuleCollider>();
            cc.center = new Vector3(0, 0.185f, 0);
            cc.height = 0.37f;
            cc.radius = 0.04f;
        }
        
        // Ensure Rigidbody
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody>();
        }
        rb.mass = 1f;
        
        Debug.Log($"Setup Bottle: {go.name}");
    }
}
