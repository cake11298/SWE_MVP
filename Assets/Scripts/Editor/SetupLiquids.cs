using UnityEngine;
using UnityEditor;
using BarSimulator.Objects;
using BarSimulator.Player;

public class SetupLiquids : MonoBehaviour
{
    public static void Execute()
    {
        RemoveLiquidVisual("Shaker");
        RemoveLiquidVisual("CoupeGlass");
        RemoveLiquidVisual("CrystalGlass");
    }

    private static void RemoveLiquidVisual(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            Debug.LogWarning($"{objectName} not found");
            return;
        }

        // Find and destroy Liquid child
        Transform liquidTr = obj.transform.Find("Liquid");
        if (liquidTr != null)
        {
            DestroyImmediate(liquidTr.gameObject);
            Debug.Log($"Removed Liquid visual from {objectName}");
        }

        // Clear references in InteractableItem
        InteractableItem item = obj.GetComponent<InteractableItem>();
        if (item != null)
        {
            SerializedObject so = new SerializedObject(item);
            SerializedProperty prop = so.FindProperty("liquidVisual");
            if (prop != null)
            {
                prop.objectReferenceValue = null;
                so.ApplyModifiedProperties();
            }
        }

        // Clear references in Container/Glass
        Container container = obj.GetComponent<Container>();
        if (container != null)
        {
            SerializedObject so = new SerializedObject(container);
            SerializedProperty propRenderer = so.FindProperty("liquidRenderer");
            SerializedProperty propTransform = so.FindProperty("liquidTransform");
            
            if (propRenderer != null) propRenderer.objectReferenceValue = null;
            if (propTransform != null) propTransform.objectReferenceValue = null;
            
            so.ApplyModifiedProperties();
        }
    }
}
