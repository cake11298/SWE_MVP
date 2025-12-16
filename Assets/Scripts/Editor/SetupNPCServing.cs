using UnityEngine;
using UnityEditor;
using BarSimulator.NPC;
using BarSimulator.Data;

namespace BarSimulator.Editor
{
    /// <summary>
    /// Editor script to setup NPC serving system
    /// </summary>
    public class SetupNPCServing : EditorWindow
    {
        [MenuItem("Bar/Setup NPC Serving System")]
        public static void Setup()
        {
            Debug.Log("=== Setting up NPC Serving System ===");

            int npcCount = 0;

            // Find all NPCs in the scene
            var allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                // Check if object name contains "NPC"
                if (obj.name.Contains("NPC") || obj.name.Contains("Gustave") || obj.name.Contains("Seaton"))
                {
                    // Add SimpleNPCServe component if not already present
                    if (obj.GetComponent<SimpleNPCServe>() == null)
                    {
                        obj.AddComponent<SimpleNPCServe>();
                        Debug.Log($"Added SimpleNPCServe to {obj.name}");
                        npcCount++;
                    }
                    else
                    {
                        Debug.Log($"{obj.name} already has SimpleNPCServe component");
                    }
                }
            }

            Debug.Log($"=== Setup complete! Added SimpleNPCServe to {npcCount} NPCs ===");

            // Update LiquorDatabase
            UpdateLiquorDatabase();

            // Mark scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }

        [MenuItem("Bar/Update LiquorDatabase with Cointreau")]
        public static void UpdateLiquorDatabase()
        {
            Debug.Log("=== Updating LiquorDatabase ===");

            // Load the LiquorDatabase asset
            var database = Resources.Load<LiquorDatabase>("LiquorDataBase");
            
            if (database == null)
            {
                Debug.LogError("LiquorDatabase not found at Resources/LiquorDataBase.asset");
                return;
            }

            // Check if Cointreau already exists
            var cointreau = database.GetLiquor("cointreau");
            if (cointreau != null)
            {
                Debug.Log("Cointreau already exists in database");
                return;
            }

            // Re-initialize the database to include Cointreau
            database.InitializeDefaults();
            
            // Mark asset as dirty to save changes
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            
            Debug.Log("=== LiquorDatabase updated with Cointreau ===");
        }

        [MenuItem("Bar/Fix Cointreau Bottle LiquidContainer")]
        public static void FixCointreauBottle()
        {
            Debug.Log("=== Fixing Cointreau Bottle ===");

            // Find Cointreau bottle in scene
            var cointreau = GameObject.Find("Cointreau");
            if (cointreau == null)
            {
                Debug.LogError("Cointreau bottle not found in scene");
                return;
            }

            // Get LiquidContainer component
            var liquidContainer = cointreau.GetComponent<BarSimulator.Objects.LiquidContainer>();
            if (liquidContainer == null)
            {
                Debug.LogError("LiquidContainer component not found on Cointreau");
                return;
            }

            // Update liquid name to match database ID (lowercase)
            liquidContainer.liquidName = "cointreau";
            
            Debug.Log($"Updated Cointreau LiquidContainer liquidName to: {liquidContainer.liquidName}");

            // Mark scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log("=== Cointreau Bottle fixed ===");
        }
    }
}
