using UnityEngine;
using UnityEditor;
using BarSimulator.Objects;

public class AddLiquidContainersToBottles : MonoBehaviour
{
    [MenuItem("Tools/Add Liquid Containers to All Bottles")]
    public static void AddContainers()
    {
        // Find all bottles
        string[] bottleNames = { "Vodka", "Rum" };
        
        foreach (string bottleName in bottleNames)
        {
            GameObject bottle = GameObject.Find(bottleName);
            if (bottle != null)
            {
                // Add LiquidContainer if not exists
                LiquidContainer container = bottle.GetComponent<LiquidContainer>();
                if (container == null)
                {
                    container = bottle.AddComponent<LiquidContainer>();
                    Debug.Log($"Added LiquidContainer to {bottleName}");
                }
                
                // Configure properties
                container.liquidName = bottleName;
                container.isInfinite = true;
                container.currentVolume = 750f;
                container.maxVolume = 750f;
                container.pourRate = 3.5f;
                
                EditorUtility.SetDirty(bottle);
                Debug.Log($"Configured {bottleName}: liquidName={container.liquidName}, isInfinite={container.isInfinite}");
            }
            else
            {
                Debug.LogWarning($"Cannot find {bottleName} in scene");
            }
        }
        
        Debug.Log("Finished adding liquid containers to all bottles!");
    }
}
