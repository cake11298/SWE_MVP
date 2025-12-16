using UnityEngine;
using UnityEditor;

namespace BarSimulator.Editor
{
    public class HideGameEndPanel
    {
        [MenuItem("Bar/UI/Hide GameEndPanel")]
        public static void Execute()
        {
            GameObject canvas = GameObject.Find("UI_Canvas");
            if (canvas == null)
            {
                Debug.LogError("UI_Canvas not found!");
                return;
            }

            Transform panelTransform = canvas.transform.Find("GameEndPanel");
            if (panelTransform != null)
            {
                panelTransform.gameObject.SetActive(false);
                Debug.Log("GameEndPanel hidden successfully");
                EditorUtility.SetDirty(panelTransform.gameObject);
            }
            else
            {
                Debug.LogError("GameEndPanel not found!");
            }
        }
    }
}
