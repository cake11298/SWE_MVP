using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

public class FindInteractables : MonoBehaviour
{
    public static void Execute()
    {
        var allObjects = FindObjectsOfType<InteractableItem>(true);
        Debug.Log($"Found {allObjects.Length} InteractableItem objects:");
        
        foreach (var item in allObjects)
        {
            Debug.Log($"  - {GetGameObjectPath(item.gameObject)} (Name: {item.itemName}, Type: {item.itemType})");
        }
    }
    
    private static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return "/" + path;
    }
}
