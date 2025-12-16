using UnityEngine;
using UnityEditor;
using BarSimulator.Systems;
using BarSimulator.UI;
using BarSimulator.Player;

namespace BarSimulator.Editor
{
    /// <summary>
    /// Complete game setup - runs all necessary setup steps
    /// </summary>
    public class CompleteGameSetup : MonoBehaviour
    {
        [MenuItem("Bar Simulator/Setup/Complete Game Setup (Run This!)")]
        public static void RunCompleteSetup()
        {
            Debug.Log("========================================");
            Debug.Log("Starting Complete Game Setup...");
            Debug.Log("========================================");

            // Step 1: Fix Shaker Setup (IMPORTANT!)
            FixShakerSetupStep();

            // Step 2: Setup Decoration Manager
            SetupDecorationManagerStep();

            // Step 3: Setup Liquor Levels UI
            SetupLiquorLevelsUIStep();

            // Step 4: Verify GameEndPanel
            VerifyGameEndPanel();

            // Step 5: Verify Shaker QTE Settings
            VerifyShakerQTESettings();

            Debug.Log("========================================");
            Debug.Log("Complete Game Setup Finished!");
            Debug.Log("========================================");
            Debug.Log("Please test the following:");
            Debug.Log("1. Pick up Shaker and hold RIGHT mouse button to shake");
            Debug.Log("2. QTE should last 20 seconds with 4 skill checks");
            Debug.Log("3. Releasing right mouse button should interrupt QTE");
            Debug.Log("4. Hold LEFT mouse button to pour from Shaker");
            Debug.Log("5. Pause (ESC) stops the entire game");
            Debug.Log("6. Liquor levels display shows 5 base liquors");
            Debug.Log("7. Speakers are hidden until purchased");
            Debug.Log("8. Game ends when time reaches 24:00 or P is pressed");
        }

        private static void FixShakerSetupStep()
        {
            Debug.Log("\n--- Step 1: Fixing Shaker Setup ---");
            
            // Find Shaker object
            GameObject shakerObj = GameObject.Find("Shaker");
            if (shakerObj == null)
            {
                Debug.LogError("✗ Shaker object not found in scene!");
                return;
            }

            Debug.Log($"✓ Found Shaker: {shakerObj.name}");

            // Check if Shaker component exists
            var shakerComponent = shakerObj.GetComponent<BarSimulator.Objects.Shaker>();
            if (shakerComponent == null)
            {
                shakerComponent = shakerObj.AddComponent<BarSimulator.Objects.Shaker>();
                Debug.Log("  ✓ Added Shaker component");
            }
            else
            {
                Debug.Log("  ✓ Shaker component exists");
            }

            // Fix QTE System settings
            var qteSystem = shakerObj.GetComponent<ShakerQTESystem>();
            if (qteSystem != null)
            {
                SerializedObject so = new SerializedObject(qteSystem);
                
                var qteDuration = so.FindProperty("qteDuration");
                var totalSkillChecks = so.FindProperty("totalSkillChecks");
                var requiredSuccesses = so.FindProperty("requiredSuccesses");
                var skillCheckDuration = so.FindProperty("skillCheckDuration");
                
                if (qteDuration != null) qteDuration.floatValue = 20f;
                if (totalSkillChecks != null) totalSkillChecks.intValue = 4;
                if (requiredSuccesses != null) requiredSuccesses.intValue = 3;
                if (skillCheckDuration != null) skillCheckDuration.floatValue = 3f;
                
                so.ApplyModifiedProperties();
                Debug.Log("  ✓ Updated QTE settings (20s, 4 checks, 3s each)");
            }

            // Check ShakerController on Player
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                var shakerController = player.GetComponentInChildren<ShakerController>();
                if (shakerController == null)
                {
                    Camera cam = player.GetComponentInChildren<Camera>();
                    if (cam != null)
                    {
                        shakerController = cam.gameObject.AddComponent<ShakerController>();
                        Debug.Log("  ✓ Added ShakerController to Player");
                    }
                }
                else
                {
                    Debug.Log("  ✓ ShakerController exists");
                }
            }

            // Remove old InteractableItem
            var oldItem = shakerObj.GetComponent<BarSimulator.Player.InteractableItem>();
            if (oldItem != null)
            {
                Object.DestroyImmediate(oldItem);
                Debug.Log("  ✓ Removed old InteractableItem");
            }

            EditorUtility.SetDirty(shakerObj);
        }

        private static void SetupDecorationManagerStep()
        {
            Debug.Log("\n--- Step 2: Setting up DecorationManager ---");
            
            // Find or create DecorationManager
            DecorationManager manager = FindFirstObjectByType<DecorationManager>();
            if (manager == null)
            {
                GameObject managerObj = new GameObject("DecorationManager");
                manager = managerObj.AddComponent<DecorationManager>();
                Debug.Log("✓ Created new DecorationManager");
            }
            else
            {
                Debug.Log("✓ DecorationManager already exists");
            }

            // Find speaker objects
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            System.Collections.Generic.List<GameObject> speakers = new System.Collections.Generic.List<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("SM_Speakers"))
                {
                    speakers.Add(obj);
                }
            }

            if (speakers.Count > 0)
            {
                manager.SetSpeakerObjects(speakers.ToArray());
                Debug.Log($"✓ Found and assigned {speakers.Count} speaker objects");
                
                // List the speakers
                foreach (var speaker in speakers)
                {
                    Debug.Log($"  - {speaker.name}");
                }
            }
            else
            {
                Debug.LogWarning("⚠ No speaker objects found in the scene");
            }

            EditorUtility.SetDirty(manager);
        }

        private static void SetupLiquorLevelsUIStep()
        {
            Debug.Log("\n--- Step 3: Setting up Liquor Levels UI ---");

            LiquidInfoUI liquidInfoUI = FindFirstObjectByType<LiquidInfoUI>();
            if (liquidInfoUI == null)
            {
                Debug.LogError("✗ LiquidInfoUI not found in scene!");
                return;
            }

            GameObject infoPanel = liquidInfoUI.infoPanel;
            if (infoPanel == null)
            {
                Debug.LogError("✗ Info panel not found on LiquidInfoUI!");
                return;
            }

            // Check if liquor levels text already exists
            Transform existingText = infoPanel.transform.Find("LiquorLevelsText");
            if (existingText != null)
            {
                Debug.Log("✓ LiquorLevelsText already exists");
                var existingTextComponent = existingText.GetComponent<UnityEngine.UI.Text>();
                if (existingTextComponent != null)
                {
                    var field = typeof(LiquidInfoUI).GetField("liquorLevelsText", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(liquidInfoUI, existingTextComponent);
                        Debug.Log("✓ Updated LiquorLevelsText reference");
                    }
                }
                EditorUtility.SetDirty(liquidInfoUI);
                return;
            }

            // Create liquor levels text
            GameObject liquorLevelsTextObj = new GameObject("LiquorLevelsText");
            liquorLevelsTextObj.transform.SetParent(infoPanel.transform, false);

            RectTransform rectTransform = liquorLevelsTextObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0.4f);
            rectTransform.offsetMin = new Vector2(10, 10);
            rectTransform.offsetMax = new Vector2(-10, -10);

            UnityEngine.UI.Text textComponent = liquorLevelsTextObj.AddComponent<UnityEngine.UI.Text>();
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 14;
            textComponent.alignment = TextAnchor.UpperLeft;
            textComponent.color = Color.white;
            textComponent.text = "=== Base Liquors ===\nLoading...";

            var liquorLevelsTextField = typeof(LiquidInfoUI).GetField("liquorLevelsText", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (liquorLevelsTextField != null)
            {
                liquorLevelsTextField.SetValue(liquidInfoUI, textComponent);
                Debug.Log("✓ Created and assigned LiquorLevelsText");
            }

            EditorUtility.SetDirty(liquidInfoUI);
        }

        private static void VerifyGameEndPanel()
        {
            Debug.Log("\n--- Step 4: Verifying GameEndPanel ---");

            GameEndUI gameEndUI = FindFirstObjectByType<GameEndUI>();
            if (gameEndUI == null)
            {
                Debug.LogWarning("⚠ GameEndUI not found in scene");
                return;
            }

            var field = typeof(GameEndUI).GetField("gameEndPanel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                GameObject panel = field.GetValue(gameEndUI) as GameObject;
                if (panel != null)
                {
                    Debug.Log($"✓ GameEndPanel found: {panel.name}");
                    Debug.Log($"  Active: {panel.activeSelf}");
                }
                else
                {
                    Debug.LogError("✗ GameEndPanel reference is null! Please assign it in the inspector.");
                }
            }
        }

        private static void VerifyShakerQTESettings()
        {
            Debug.Log("\n--- Step 5: Verifying Shaker QTE Settings ---");

            ShakerQTESystem[] qteSystems = FindObjectsOfType<ShakerQTESystem>();
            if (qteSystems.Length == 0)
            {
                Debug.LogWarning("⚠ No ShakerQTESystem found in scene");
                return;
            }

            foreach (var qteSystem in qteSystems)
            {
                Debug.Log($"✓ Found ShakerQTESystem on: {qteSystem.gameObject.name}");
                
                // Use reflection to check settings
                var durationField = typeof(ShakerQTESystem).GetField("qteDuration", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var checksField = typeof(ShakerQTESystem).GetField("totalSkillChecks", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (durationField != null && checksField != null)
                {
                    float duration = (float)durationField.GetValue(qteSystem);
                    int checks = (int)checksField.GetValue(qteSystem);
                    
                    Debug.Log($"  Duration: {duration}s (should be 20s)");
                    Debug.Log($"  Skill Checks: {checks} (should be 4)");
                    
                    if (duration != 20f || checks != 4)
                    {
                        Debug.LogWarning("⚠ QTE settings don't match requirements. Please check the inspector.");
                    }
                }
            }
        }
    }
}
