using UnityEngine;
using UnityEditor;
using BarSimulator.Player;
using System.Collections.Generic;

public class ListInteractableObjects
{
    [MenuItem("Tools/List Interactable Objects")]
    public static void Execute()
    {
        Debug.Log("[ListInteractable] Listing all interactable objects...");
        
        InteractableItem[] interactables = Object.FindObjectsOfType<InteractableItem>();
        
        Dictionary<string, int> typeCount = new Dictionary<string, int>();
        
        foreach (InteractableItem item in interactables)
        {
            string prefix = item.gameObject.name.Split('_')[0];
            if (!typeCount.ContainsKey(prefix))
            {
                typeCount[prefix] = 0;
            }
            typeCount[prefix]++;
        }
        
        Debug.Log($"[ListInteractable] Total interactable objects: {interactables.Length}");
        Debug.Log("[ListInteractable] Breakdown by prefix:");
        foreach (var kvp in typeCount)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value} objects");
        }
        
        // List first 20 interactable objects
        Debug.Log("[ListInteractable] First 20 interactable objects:");
        for (int i = 0; i < Mathf.Min(20, interactables.Length); i++)
        {
            Debug.Log($"  {i + 1}. {interactables[i].gameObject.name} ({interactables[i].itemType})");
        }
    }
}
