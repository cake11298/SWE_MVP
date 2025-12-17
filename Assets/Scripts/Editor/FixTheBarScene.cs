using UnityEngine;
using UnityEditor;
using BarSimulator.UI;

public class FixTheBarScene : MonoBehaviour
{
    public static void Execute()
    {
        // 1. Disable GameEndPanel
        GameObject gameEndPanel = GameObject.Find("UI_Canvas/GameEndPanel");
        if (gameEndPanel != null)
        {
            gameEndPanel.SetActive(false);
            Debug.Log("Disabled GameEndPanel");
        }
        else
        {
            Debug.LogWarning("GameEndPanel not found");
        }

        // 2. Remove GameEndUI from UI_Canvas
        GameObject uiCanvas = GameObject.Find("UI_Canvas");
        if (uiCanvas != null)
        {
            GameEndUI gameEndUI = uiCanvas.GetComponent<GameEndUI>();
            if (gameEndUI != null)
            {
                DestroyImmediate(gameEndUI);
                Debug.Log("Removed GameEndUI from UI_Canvas");
            }
        }

        // 3. Set GameStatsUI realMinutesForFullDay
        GameObject gameStatsPanel = GameObject.Find("UI_Canvas/GameStatsPanel");
        if (gameStatsPanel != null)
        {
            GameStatsUI statsUI = gameStatsPanel.GetComponent<GameStatsUI>();
            if (statsUI != null)
            {
                SerializedObject so = new SerializedObject(statsUI);
                so.FindProperty("realMinutesForFullDay").floatValue = 5f;
                so.ApplyModifiedProperties();
                Debug.Log("Set realMinutesForFullDay to 5");
            }
        }
    }
}
