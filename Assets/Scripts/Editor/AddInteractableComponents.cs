using UnityEngine;
using UnityEditor;
using BarSimulator.Player;

public class AddInteractableComponents : MonoBehaviour
{
    [MenuItem("Tools/Add Interactable Components")]
    public static void Execute()
    {
        // Find objects
        GameObject jigger = GameObject.Find("Jigger");
        GameObject vodka = GameObject.Find("Vodka");
        GameObject rum = GameObject.Find("Rum");
        GameObject taquila = GameObject.Find("Taquila");

        // Setup Jigger
        if (jigger != null)
        {
            SetupInteractable(jigger, ItemType.Other, "Jigger", LiquidType.None);
            Debug.Log("Jigger setup complete");
        }

        // Setup Vodka
        if (vodka != null)
        {
            SetupInteractable(vodka, ItemType.Bottle, "Vodka", LiquidType.Vodka);
            SetConvexCollider(vodka);
            Debug.Log("Vodka setup complete");
        }

        // Setup Rum
        if (rum != null)
        {
            SetupInteractable(rum, ItemType.Bottle, "Rum", LiquidType.Rum);
            SetConvexCollider(rum);
            Debug.Log("Rum setup complete");
        }

        // Setup Taquila
        if (taquila != null)
        {
            SetupInteractable(taquila, ItemType.Bottle, "Taquila", LiquidType.Tequila);
            SetConvexCollider(taquila);
            Debug.Log("Taquila setup complete");
        }

        EditorUtility.SetDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().GetRootGameObjects()[0]);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    private static void SetupInteractable(GameObject obj, ItemType itemType, string itemName, LiquidType liquidType)
    {
        InteractableItem item = obj.GetComponent<InteractableItem>();
        if (item == null)
        {
            item = obj.AddComponent<InteractableItem>();
        }

        item.itemType = itemType;
        item.itemName = itemName;
        item.liquidType = liquidType;
        item.liquidAmount = 750f;
        item.maxLiquidAmount = 750f;

        EditorUtility.SetDirty(obj);
    }

    private static void SetConvexCollider(GameObject obj)
    {
        MeshCollider meshCollider = obj.GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.convex = true;
            EditorUtility.SetDirty(obj);
        }
    }
}
