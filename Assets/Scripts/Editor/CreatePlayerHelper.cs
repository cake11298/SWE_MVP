using UnityEngine;
using UnityEditor;

namespace BarSimulator.Editor
{
    public class CreatePlayerHelper
    {
        [MenuItem("Bar Simulator/Create Player")]
        public static void CreatePlayer()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No main camera found!");
                return;
            }

            GameObject player = new GameObject("Player");
            player.transform.position = new Vector3(-2.22f, 0.9f, -3.3f);
            player.transform.rotation = Quaternion.Euler(0, 181.6f, 0);

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0, 0.9f, 0);

            player.AddComponent(System.Type.GetType("BarSimulator.Player.SimplePlayerController, Assembly-CSharp"));
            player.AddComponent(System.Type.GetType("BarSimulator.Player.ItemInteractionSystem, Assembly-CSharp"));

            GameObject cameraHolder = new GameObject("CameraHolder");
            cameraHolder.transform.SetParent(player.transform);
            cameraHolder.transform.localPosition = new Vector3(0, 1.6f, 0);
            cameraHolder.transform.localRotation = Quaternion.identity;

            mainCamera.transform.SetParent(cameraHolder.transform);
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log("Player created successfully!");
            Selection.activeGameObject = player;
        }
    }
}
