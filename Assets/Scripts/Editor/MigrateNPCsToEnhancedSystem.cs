using UnityEngine;
using UnityEditor;
using BarSimulator.NPC;

namespace BarSimulator.Editor
{
    /// <summary>
    /// Editor utility to migrate NPCs from SimpleNPCServe to EnhancedNPCServe
    /// </summary>
    public class MigrateNPCsToEnhancedSystem : EditorWindow
    {
        [MenuItem("Bar Simulator/Migrate NPCs to Enhanced System")]
        public static void MigrateNPCs()
        {
            // Find all SimpleNPCServe components in the scene
            SimpleNPCServe[] oldComponents = FindObjectsByType<SimpleNPCServe>(FindObjectsSortMode.None);
            
            if (oldComponents.Length == 0)
            {
                EditorUtility.DisplayDialog("Info", "No NPCs with SimpleNPCServe found in the scene.", "OK");
                return;
            }

            int migratedCount = 0;
            
            foreach (var oldComponent in oldComponents)
            {
                GameObject npc = oldComponent.gameObject;
                
                // Add new component
                EnhancedNPCServe newComponent = npc.AddComponent<EnhancedNPCServe>();
                
                // Remove old component
                DestroyImmediate(oldComponent);
                
                migratedCount++;
                Debug.Log($"Migrated {npc.name} to EnhancedNPCServe");
            }

            string message = $"Successfully migrated {migratedCount} NPC(s) to the Enhanced Cocktail System!\n\n" +
                           "NPCs will now:\n" +
                           "- Order from 10 different cocktails\n" +
                           "- Evaluate drinks based on ingredients and ratios\n" +
                           "- Give level-based bonuses for quality spirits\n" +
                           "- Wait 60 seconds between orders";
            
            EditorUtility.DisplayDialog("Migration Complete", message, "OK");
            Debug.Log($"[MigrateNPCs] {message}");
        }

        [MenuItem("Bar Simulator/Add EnhancedNPCServe to Selected")]
        public static void AddEnhancedNPCServeToSelected()
        {
            if (Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Please select one or more GameObjects first.", "OK");
                return;
            }

            int addedCount = 0;
            
            foreach (var obj in Selection.gameObjects)
            {
                // Check if already has the component
                if (obj.GetComponent<EnhancedNPCServe>() != null)
                {
                    Debug.LogWarning($"{obj.name} already has EnhancedNPCServe component");
                    continue;
                }

                // Remove old component if exists
                var oldComponent = obj.GetComponent<SimpleNPCServe>();
                if (oldComponent != null)
                {
                    DestroyImmediate(oldComponent);
                }

                // Add new component
                obj.AddComponent<EnhancedNPCServe>();
                addedCount++;
                Debug.Log($"Added EnhancedNPCServe to {obj.name}");
            }

            string message = $"Added EnhancedNPCServe to {addedCount} GameObject(s)";
            EditorUtility.DisplayDialog("Success", message, "OK");
        }

        [MenuItem("Bar Simulator/Remove All SimpleNPCServe")]
        public static void RemoveAllSimpleNPCServe()
        {
            SimpleNPCServe[] components = FindObjectsByType<SimpleNPCServe>(FindObjectsSortMode.None);
            
            if (components.Length == 0)
            {
                EditorUtility.DisplayDialog("Info", "No SimpleNPCServe components found in the scene.", "OK");
                return;
            }

            bool confirm = EditorUtility.DisplayDialog(
                "Confirm Removal",
                $"Remove SimpleNPCServe from {components.Length} GameObject(s)?",
                "Yes", "Cancel"
            );

            if (!confirm)
                return;

            int removedCount = 0;
            
            foreach (var component in components)
            {
                string objName = component.gameObject.name;
                DestroyImmediate(component);
                removedCount++;
                Debug.Log($"Removed SimpleNPCServe from {objName}");
            }

            EditorUtility.DisplayDialog("Success", $"Removed SimpleNPCServe from {removedCount} GameObject(s)", "OK");
        }
    }
}
