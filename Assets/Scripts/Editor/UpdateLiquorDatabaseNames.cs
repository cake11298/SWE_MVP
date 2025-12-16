using UnityEngine;
using UnityEditor;
using BarSimulator.Data;

/// <summary>
/// Update liquor database with correct display names
/// </summary>
public class UpdateLiquorDatabaseNames
{
    public static void Execute()
    {
        Debug.Log("=== Updating Liquor Database Display Names ===");

        LiquorDatabase database = Resources.Load<LiquorDatabase>("LiquorDataBase");
        if (database == null)
        {
            Debug.LogError("LiquorDatabase not found at Resources/LiquorDataBase!");
            return;
        }

        bool updated = false;

        // Update display names
        foreach (var liquor in database.liquors)
        {
            switch (liquor.id)
            {
                case "amaretto_syrup":
                    if (liquor.displayName != "Syrup")
                    {
                        liquor.displayName = "Syrup";
                        updated = true;
                        Debug.Log($"Updated {liquor.id} display name to: Syrup");
                    }
                    break;

                case "vermouth_sweet":
                    if (liquor.displayName != "Vermouth")
                    {
                        liquor.displayName = "Vermouth";
                        updated = true;
                        Debug.Log($"Updated {liquor.id} display name to: Vermouth");
                    }
                    break;

                case "cognac":
                    if (liquor.displayName != "Cognac")
                    {
                        liquor.displayName = "Cognac";
                        updated = true;
                        Debug.Log($"Updated {liquor.id} display name to: Cognac");
                    }
                    break;

                case "lemon_juice":
                    if (liquor.displayName != "LemonJuice")
                    {
                        liquor.displayName = "LemonJuice";
                        updated = true;
                        Debug.Log($"Updated {liquor.id} display name to: LemonJuice");
                    }
                    break;
            }
        }

        if (updated)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Debug.Log("Liquor database updated successfully!");
        }
        else
        {
            Debug.Log("No updates needed - all display names are correct");
        }

        Debug.Log("=== Update Complete ===");
    }
}
