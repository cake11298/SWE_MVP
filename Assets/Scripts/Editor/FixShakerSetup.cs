using UnityEngine;
using UnityEditor;
using BarSimulator.Objects;
using BarSimulator.Systems;
using BarSimulator.Player;

namespace BarSimulator.Editor
{
    /// <summary>
    /// Fix Shaker setup - add Shaker component and configure QTE settings
    /// </summary>
    public class FixShakerSetup : MonoBehaviour
    {
        [MenuItem("Bar Simulator/Fix/Fix Shaker Setup")]
        public static void Fix()
        {
            Debug.Log("========================================");
            Debug.Log("Fixing Shaker Setup...");
            Debug.Log("========================================");

            // Find Shaker object
            GameObject shakerObj = GameObject.Find("Shaker");
            if (shakerObj == null)
            {
                Debug.LogError("✗ Shaker object not found in scene!");
                return;
            }

            Debug.Log($"✓ Found Shaker object: {shakerObj.name}");

            // Check if Shaker component exists
            Shaker shakerComponent = shakerObj.GetComponent<Shaker>();
            if (shakerComponent == null)
            {
                Debug.Log("  Adding Shaker component...");
                shakerComponent = shakerObj.AddComponent<Shaker>();
                Debug.Log("  ✓ Added Shaker component");
            }
            else
            {
                Debug.Log("  ✓ Shaker component already exists");
            }

            // Check and fix QTE System settings
            ShakerQTESystem qteSystem = shakerObj.GetComponent<ShakerQTESystem>();
            if (qteSystem != null)
            {
                Debug.Log("  Updating QTE System settings...");
                
                // Use SerializedObject to modify private fields
                SerializedObject so = new SerializedObject(qteSystem);
                
                SerializedProperty qteDuration = so.FindProperty("qteDuration");
                SerializedProperty totalSkillChecks = so.FindProperty("totalSkillChecks");
                SerializedProperty requiredSuccesses = so.FindProperty("requiredSuccesses");
                SerializedProperty skillCheckDuration = so.FindProperty("skillCheckDuration");
                
                if (qteDuration != null) qteDuration.floatValue = 20f;
                if (totalSkillChecks != null) totalSkillChecks.intValue = 4;
                if (requiredSuccesses != null) requiredSuccesses.intValue = 3;
                if (skillCheckDuration != null) skillCheckDuration.floatValue = 3f;
                
                so.ApplyModifiedProperties();
                
                Debug.Log("  ✓ QTE Duration: 20 seconds");
                Debug.Log("  ✓ Skill Checks: 4");
                Debug.Log("  ✓ Required Successes: 3");
                Debug.Log("  ✓ Skill Check Duration: 3 seconds");
            }
            else
            {
                Debug.LogWarning("  ⚠ ShakerQTESystem not found");
            }

            // Check ShakerController on Player
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                ShakerController shakerController = player.GetComponentInChildren<ShakerController>();
                if (shakerController == null)
                {
                    // Try to find camera
                    Camera cam = player.GetComponentInChildren<Camera>();
                    if (cam != null)
                    {
                        shakerController = cam.gameObject.AddComponent<ShakerController>();
                        Debug.Log("  ✓ Added ShakerController to Player camera");
                    }
                    else
                    {
                        Debug.LogWarning("  ⚠ Could not find camera to add ShakerController");
                    }
                }
                else
                {
                    Debug.Log("  ✓ ShakerController already exists on Player");
                }
            }
            else
            {
                Debug.LogWarning("  ⚠ Player object not found");
            }

            // Remove old InteractableItem if it exists (conflicts with new system)
            InteractableItem oldItem = shakerObj.GetComponent<InteractableItem>();
            if (oldItem != null)
            {
                Debug.Log("  Removing old InteractableItem component...");
                DestroyImmediate(oldItem);
                Debug.Log("  ✓ Removed old InteractableItem");
            }

            // Mark scene as dirty
            EditorUtility.SetDirty(shakerObj);
            if (player != null) EditorUtility.SetDirty(player);
            
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log("========================================");
            Debug.Log("Shaker Setup Fixed!");
            Debug.Log("========================================");
            Debug.Log("Now you should be able to:");
            Debug.Log("1. Pick up the shaker");
            Debug.Log("2. Hold RIGHT mouse button to shake (20 seconds, 4 QTE checks)");
            Debug.Log("3. Release RIGHT mouse button to stop (interrupts QTE)");
            Debug.Log("4. Hold LEFT mouse button to pour");
        }
    }
}
