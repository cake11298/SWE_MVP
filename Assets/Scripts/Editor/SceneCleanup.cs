using UnityEngine;
using UnityEditor;
using BarSimulator.Objects;

public class SceneCleanup : MonoBehaviour
{
    public static void CleanupAndFix()
    {
        // 1. Remove duplicate environment
        DestroyGameObject("Environment_Structure");
        DestroyGameObject("Bar_Area");

        // 2. Replace Primitive Bottles with Prefab
        ReplaceBottles();
    }

    private static void DestroyGameObject(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null)
        {
            DestroyImmediate(go);
            Debug.Log($"Destroyed {name}");
        }
    }

    private static void ReplaceBottles()
    {
        GameObject bottlesRoot = GameObject.Find("Bottles");
        if (bottlesRoot == null) return;

        GameObject bottlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bottle.prefab");
        if (bottlePrefab == null)
        {
            Debug.LogError("Could not find Assets/Prefabs/Bottle.prefab");
            return;
        }

        // Get all children first
        int childCount = bottlesRoot.transform.childCount;
        Transform[] children = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            children[i] = bottlesRoot.transform.GetChild(i);
        }

        foreach (Transform child in children)
        {
            // Skip if already a prefab instance (check if name contains "Bottle" and not "Cylinder" maybe? Or just check if it has the script)
            // My primitive bottles were named "Bottle_liquorId" or custom name.
            // They are primitives, so they are not prefab instances.
            
            Bottle oldScript = child.GetComponent<Bottle>();
            string liquorId = oldScript != null ? oldScript.LiquorId : "whiskey";
            string objName = child.name;

            // Instantiate Prefab
            GameObject newBottle = (GameObject)PrefabUtility.InstantiatePrefab(bottlePrefab);
            newBottle.transform.position = child.position;
            newBottle.transform.rotation = child.rotation;
            newBottle.transform.SetParent(bottlesRoot.transform);
            newBottle.name = objName;
            newBottle.transform.localScale = Vector3.one; // Reset scale, prefab should have correct scale

            // Setup Script
            Bottle newScript = newBottle.GetComponent<Bottle>();
            if (newScript != null)
            {
                newScript.SetLiquor(liquorId);
            }

            // Destroy old
            DestroyImmediate(child.gameObject);
        }
        
        Debug.Log($"Replaced {childCount} bottles with prefabs.");
    }
}
