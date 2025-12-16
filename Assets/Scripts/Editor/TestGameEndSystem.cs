using UnityEngine;
using UnityEditor;
using BarSimulator.Core;
using BarSimulator.Data;

namespace BarSimulator.Editor
{
    /// <summary>
    /// Test script for the game end system
    /// </summary>
    public class TestGameEndSystem
    {
        [MenuItem("Bar/Testing/Add Test Coins (5000)")]
        public static void AddTestCoins()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("TestGameEndSystem: Must be in Play Mode to add coins");
                return;
            }

            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.AddCoins(5000);
                Debug.Log("TestGameEndSystem: Added 5000 test coins");
            }
            else
            {
                Debug.LogError("TestGameEndSystem: PersistentGameData not found");
            }
        }

        [MenuItem("Bar/Testing/Reset Persistent Data")]
        public static void ResetPersistentData()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("TestGameEndSystem: Must be in Play Mode to reset data");
                return;
            }

            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.ResetAllData();
                Debug.Log("TestGameEndSystem: Reset all persistent data");
            }
            else
            {
                Debug.LogError("TestGameEndSystem: PersistentGameData not found");
            }
        }

        [MenuItem("Bar/Testing/Force Game End (Press P in Play Mode)")]
        public static void ShowForceGameEndInfo()
        {
            Debug.Log("TestGameEndSystem: Press P during gameplay to force game end and show the GameEndPanel");
        }

        [MenuItem("Bar/Testing/Show Current Persistent Data")]
        public static void ShowPersistentData()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("TestGameEndSystem: Must be in Play Mode to show data");
                return;
            }

            if (PersistentGameData.Instance != null)
            {
                var instance = PersistentGameData.Instance;
                Debug.Log("=== Persistent Game Data ===");
                Debug.Log($"Total Coins: ${instance.GetTotalCoins()}");
                
                Debug.Log("\n--- Liquor Upgrades ---");
                var upgrades = instance.GetAllLiquorUpgrades();
                foreach (var upgrade in upgrades)
                {
                    Debug.Log($"{upgrade.liquorType}: Level {upgrade.level}/{LiquorUpgradeData.MaxLevel}");
                }

                Debug.Log("\n--- Decorations ---");
                var decorations = instance.GetAllDecorations();
                foreach (var decoration in decorations)
                {
                    string status = decoration.isPurchased ? "Purchased" : "Not Purchased";
                    Debug.Log($"{decoration.decorationType}: {status}");
                }
            }
            else
            {
                Debug.LogError("TestGameEndSystem: PersistentGameData not found");
            }
        }
    }
}
