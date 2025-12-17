using UnityEngine;
using UnityEditor;
using BarSimulator.Data;

namespace BarSimulator.Editor
{
    /// <summary>
    /// Editor utility to setup liquor database with proper levels for base spirits
    /// </summary>
    public class LiquorDatabaseSetup : EditorWindow
    {
        [MenuItem("Bar Simulator/Setup Liquor Database Levels")]
        public static void SetupLiquorLevels()
        {
            // Load the liquor database
            LiquorDatabase database = Resources.Load<LiquorDatabase>("LiquorDataBase");
            
            if (database == null)
            {
                Debug.LogError("LiquorDatabase not found in Resources folder!");
                EditorUtility.DisplayDialog("Error", "LiquorDatabase not found in Resources folder!", "OK");
                return;
            }

            // Ensure the 5 base spirits have level system enabled
            string[] baseSpiritIds = { "vodka", "gin", "whiskey", "rum", "cointreau" };
            int setupCount = 0;

            foreach (var spiritId in baseSpiritIds)
            {
                LiquorData liquor = database.GetLiquor(spiritId);
                
                if (liquor != null)
                {
                    // Set default level to 1
                    if (liquor.level < 1)
                        liquor.level = 1;
                    
                    // Ensure not locked
                    liquor.isLocked = false;
                    
                    // Set upgrade prices if not already set
                    if (liquor.upgradePrices == null || liquor.upgradePrices.Length < 2)
                    {
                        liquor.upgradePrices = new int[] { 500, 1000 };
                    }
                    
                    // Set level descriptions
                    if (liquor.levelDescriptions == null || liquor.levelDescriptions.Length < 3)
                    {
                        liquor.levelDescriptions = new string[]
                        {
                            $"Standard {liquor.displayName}",
                            $"Premium {liquor.displayName} - 1.3x bonus",
                            $"Ultra Premium {liquor.displayName} - 1.4x bonus"
                        };
                    }
                    
                    setupCount++;
                    Debug.Log($"Setup levels for {liquor.displayName} (Level {liquor.level})");
                }
                else
                {
                    Debug.LogWarning($"Base spirit '{spiritId}' not found in database!");
                }
            }

            // Mark database as dirty to save changes
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            
            string message = $"Successfully setup {setupCount} base spirits with level system!\n\n" +
                           "Base spirits: Vodka, Gin, Whiskey, Rum, Cointreau\n" +
                           "Each can be upgraded from Level 1 to Level 3\n" +
                           "Higher levels provide better multipliers for cocktails.";
            
            EditorUtility.DisplayDialog("Success", message, "OK");
            Debug.Log($"[LiquorDatabaseSetup] {message}");
        }

        [MenuItem("Bar Simulator/View Liquor Database Info")]
        public static void ViewLiquorDatabaseInfo()
        {
            LiquorDatabase database = Resources.Load<LiquorDatabase>("LiquorDataBase");
            
            if (database == null)
            {
                Debug.LogError("LiquorDatabase not found in Resources folder!");
                return;
            }

            Debug.Log("=== LIQUOR DATABASE INFO ===");
            Debug.Log($"Total liquors: {database.liquors.Length}");
            
            Debug.Log("\n=== BASE SPIRITS (with levels) ===");
            string[] baseSpiritIds = { "vodka", "gin", "whiskey", "rum", "cointreau" };
            
            foreach (var spiritId in baseSpiritIds)
            {
                LiquorData liquor = database.GetLiquor(spiritId);
                if (liquor != null)
                {
                    Debug.Log($"{liquor.displayName}: Level {liquor.level}, " +
                             $"Locked: {liquor.isLocked}, " +
                             $"Upgrade Prices: [{string.Join(", ", liquor.upgradePrices)}]");
                }
            }
            
            Debug.Log("\n=== ALL LIQUORS ===");
            foreach (var liquor in database.liquors)
            {
                Debug.Log($"{liquor.displayName} ({liquor.id}): " +
                         $"Category: {liquor.category}, " +
                         $"Level: {liquor.level}, " +
                         $"Alcohol: {liquor.alcoholContent}%");
            }
        }
    }
}
