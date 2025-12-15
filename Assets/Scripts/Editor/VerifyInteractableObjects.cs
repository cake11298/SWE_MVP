using UnityEngine;
using UnityEditor;
using BarSimulator.Objects;

public class VerifyInteractableObjects : MonoBehaviour
{
    [MenuItem("Tools/Verify Interactable Objects")]
    public static void Execute()
    {
        string[] objectNames = { "Gin", "Maker_Whiskey", "Shaker", "Jigger" };
        
        Debug.Log("=== Verifying Interactable Objects ===");
        
        foreach (string objName in objectNames)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                Debug.Log($"\n--- {objName} ---");
                Debug.Log($"IsStatic: {obj.isStatic}");
                
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                Debug.Log($"Has Rigidbody: {rb != null}");
                if (rb != null)
                {
                    Debug.Log($"  - Mass: {rb.mass}");
                    Debug.Log($"  - UseGravity: {rb.useGravity}");
                    Debug.Log($"  - IsKinematic: {rb.isKinematic}");
                }
                
                InteractableObject interactable = obj.GetComponent<InteractableObject>();
                Debug.Log($"Has InteractableObject: {interactable != null}");
                
                BottleController bottleController = obj.GetComponent<BottleController>();
                Debug.Log($"Has BottleController: {bottleController != null}");
                
                BoxCollider collider = obj.GetComponent<BoxCollider>();
                Debug.Log($"Has BoxCollider: {collider != null}");
            }
            else
            {
                Debug.LogWarning($"Object not found: {objName}");
            }
        }
        
        Debug.Log("\n=== Verification Complete ===");
    }
}
