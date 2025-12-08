using UnityEngine;
using UnityEditor;

public class RestoreScene : MonoBehaviour
{
    public static void Restore()
    {
        // Delete generated groups
        DestroyGameObject("Environment_Structure");
        DestroyGameObject("Bar_Area");
        DestroyGameObject("Furniture");
        DestroyGameObject("Bottles");
        
        Debug.Log("Scene restored to original state (removed generated objects).");
    }

    private static void DestroyGameObject(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null)
        {
            DestroyImmediate(go);
            Debug.Log($"Destroyed {name}");
        }
        else
        {
            Debug.Log($"Could not find {name} to destroy.");
        }
    }
}
