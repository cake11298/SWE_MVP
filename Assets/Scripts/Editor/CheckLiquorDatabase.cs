using UnityEngine;
using UnityEditor;
using BarSimulator.Data;

public class CheckLiquorDatabase
{
    public static void Execute()
    {
        var database = Resources.Load<LiquorDatabase>("LiquorDataBase");
        if (database == null)
        {
            Debug.LogError("LiquorDatabase not found!");
            return;
        }

        Debug.Log($"=== LiquorDatabase Contents ===");
        Debug.Log($"Total liquors: {database.liquors.Length}");
        
        foreach (var liquor in database.liquors)
        {
            Debug.Log($"ID: {liquor.id}, Display: {liquor.displayName}, Category: {liquor.category}");
        }
    }
}
