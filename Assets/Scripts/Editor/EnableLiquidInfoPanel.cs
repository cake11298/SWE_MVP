using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to enable LiquidInfoPanel by default
/// </summary>
public class EnableLiquidInfoPanel : MonoBehaviour
{
    [MenuItem("Tools/Enable LiquidInfoPanel")]
    public static void Enable()
    {
        GameObject panel = GameObject.Find("UI_Canvas/LiquidInfoPanel");
        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log("LiquidInfoPanel enabled");
        }
        else
        {
            Debug.LogError("LiquidInfoPanel not found");
        }
    }
}
