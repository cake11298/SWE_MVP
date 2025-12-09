using UnityEngine;
using UnityEditor;
using BarSimulator.Player;
using UnityEngine.InputSystem;

public class PlayerSetup : MonoBehaviour
{
    public static void Setup()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        // Fix FirstPersonController
        FirstPersonController fpc = player.GetComponent<FirstPersonController>();
        if (fpc != null)
        {
            fpc.SetMouseSensitivity(2.5f);
            
            // Assign Input Actions
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Input/PlayerInputActions.inputactions");
            if (inputActions != null)
            {
                SerializedObject so = new SerializedObject(fpc);
                so.FindProperty("inputActions").objectReferenceValue = inputActions;
                so.FindProperty("mouseSensitivity").floatValue = 2.5f;
                so.ApplyModifiedProperties();
            }
        }

        // Fix Camera Position
        Transform camTransform = player.transform.Find("MainCamera");
        if (camTransform != null)
        {
            camTransform.localPosition = new Vector3(0, 0.6f, 0);
            camTransform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("MainCamera not found under Player");
        }
        
        // Ensure Player Position
        player.transform.position = new Vector3(0, 1.0f, -5.0f);
        player.transform.rotation = Quaternion.identity;
        
        Debug.Log("Player setup complete.");
    }
}
