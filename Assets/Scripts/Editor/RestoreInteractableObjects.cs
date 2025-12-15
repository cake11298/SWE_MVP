using UnityEngine;
using UnityEditor;
using BarSimulator.Objects;

public class RestoreInteractableObjects : MonoBehaviour
{
    [MenuItem("Tools/Restore Interactable Objects")]
    public static void Execute()
    {
        string[] objectNames = { "Gin", "Maker_Whiskey", "Shaker", "Jigger" };
        
        foreach (string objName in objectNames)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                // Set to non-static
                obj.isStatic = false;
                
                // Configure Rigidbody
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.mass = 1f;
                    rb.drag = 0.5f;
                    rb.angularDrag = 0.5f;
                    rb.useGravity = true;
                    rb.isKinematic = false;
                    rb.interpolation = RigidbodyInterpolation.None;
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                }
                
                // Configure InteractableObject
                var interactable = obj.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    SerializedObject so = new SerializedObject(interactable);
                    
                    SerializedProperty objectNameProp = so.FindProperty("objectName");
                    if (objectNameProp != null)
                        objectNameProp.stringValue = objName;
                    
                    SerializedProperty canBePickedUpProp = so.FindProperty("canBePickedUp");
                    if (canBePickedUpProp != null)
                        canBePickedUpProp.boolValue = true;
                    
                    SerializedProperty canBePlacedProp = so.FindProperty("canBePlaced");
                    if (canBePlacedProp != null)
                        canBePlacedProp.boolValue = true;
                    
                    so.ApplyModifiedProperties();
                }
                
                // Configure BottleController for bottles
                if (objName == "Gin" || objName == "Maker_Whiskey")
                {
                    var bottleController = obj.GetComponent<BottleController>();
                    if (bottleController != null)
                    {
                        SerializedObject so = new SerializedObject(bottleController);
                        
                        SerializedProperty pourAngleProp = so.FindProperty("pourAngle");
                        if (pourAngleProp != null)
                            pourAngleProp.floatValue = 90f;
                        
                        SerializedProperty rotateSpeedProp = so.FindProperty("rotateSpeed");
                        if (rotateSpeedProp != null)
                            rotateSpeedProp.floatValue = 5f;
                        
                        so.ApplyModifiedProperties();
                    }
                }
                
                EditorUtility.SetDirty(obj);
                Debug.Log($"Restored interactable functionality for: {objName}");
            }
            else
            {
                Debug.LogWarning($"Object not found: {objName}");
            }
        }
        
        Debug.Log("Restore complete! All four objects should now be interactable.");
    }
}
