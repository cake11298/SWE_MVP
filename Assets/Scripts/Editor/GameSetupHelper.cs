using UnityEngine;
using UnityEditor;
using BarSimulator.Core;
using BarSimulator.Systems;
using BarSimulator.Objects;
using BarSimulator.Managers;
using BarSimulator.NPC;
using BarSimulator.Player;

namespace BarSimulator.Editor
{
    /// <summary>
    /// 遊戲設置助手 - 幫助設置測試場景<br/>
    /// Unused in game code (?
    /// </summary>
    public class GameSetupHelper : EditorWindow
    {
        [MenuItem("Bar Simulator/Setup Test Scene")]
        public static void SetupTestScene()
        {
            // Create Game Systems
            CreateGameSystems();

            // Create Player
            CreatePlayer();

            // Create Test Objects
            CreateTestObjects();

            // Create UI
            CreateGameUI();

            Debug.Log("GameSetupHelper: Test scene setup complete!");
            Debug.Log("Press Play to test the game. Controls: WASD=Move, Mouse=Look, E=Pickup, R=Drop, LMB=Pour, F=Give to NPC");
        }

        [MenuItem("Bar Simulator/Create Bottle")]
        public static void CreateBottle()
        {
            var bottle = CreateBottleObject("Test Bottle", new Vector3(0, 1, 2), "vodka");
            Selection.activeGameObject = bottle;
        }

        [MenuItem("Bar Simulator/Create Glass")]
        public static void CreateGlass()
        {
            var glass = CreateGlassObject("Test Glass", new Vector3(1, 1, 2));
            Selection.activeGameObject = glass;
        }

        [MenuItem("Bar Simulator/Create NPC")]
        public static void CreateNPC()
        {
            var npc = CreateNPCObject("Test NPC", new Vector3(0, 0, 3));
            Selection.activeGameObject = npc;
        }

        private static void CreateGameSystems()
        {
            // GameManager
            if (Object.FindFirstObjectByType<GameManager>() == null)
            {
                var gmObject = new GameObject("GameManager");
                gmObject.AddComponent<GameManager>();
                Debug.Log("Created GameManager");
            }

            // InteractionSystem
            if (Object.FindFirstObjectByType<InteractionSystem>() == null)
            {
                var isObject = new GameObject("InteractionSystem");
                isObject.AddComponent<InteractionSystem>();
                Debug.Log("Created InteractionSystem");
            }

            // CocktailSystem
            if (Object.FindFirstObjectByType<CocktailSystem>() == null)
            {
                var csObject = new GameObject("CocktailSystem");
                csObject.AddComponent<CocktailSystem>();
                Debug.Log("Created CocktailSystem");
            }

            // NPCManager
            if (Object.FindFirstObjectByType<NPCManager>() == null)
            {
                var nmObject = new GameObject("NPCManager");
                nmObject.AddComponent<NPCManager>();
                Debug.Log("Created NPCManager");
            }

            // LightingManager - adjust scene lighting to be darker (bar atmosphere)
            if (Object.FindFirstObjectByType<LightingManager>() == null)
            {
                var lmObject = new GameObject("LightingManager");
                lmObject.AddComponent<LightingManager>();
                Debug.Log("Created LightingManager to reduce brightness");
            }
        }

        private static void CreatePlayer()
        {
            if (Object.FindFirstObjectByType<FirstPersonController>() != null) return;

            // Create player root
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0, 1.6f, 0);

            // Add CharacterController
            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0, 0.9f, 0);

            // Add FirstPersonController
            player.AddComponent<FirstPersonController>();

            // Add NPCInteraction
            player.AddComponent<NPCInteraction>();

            // Create camera
            var cameraObj = new GameObject("Main Camera");
            cameraObj.transform.SetParent(player.transform);
            cameraObj.transform.localPosition = new Vector3(0, 0.6f, 0);
            cameraObj.tag = "MainCamera";

            var cam = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();

            Debug.Log("Created Player with FirstPersonController and Camera");
        }

        private static void CreateTestObjects()
        {
            // Create floor
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(2, 1, 2);
            var floorRenderer = floor.GetComponent<Renderer>();
            if (floorRenderer != null)
            {
                floorRenderer.material.color = new Color(0.3f, 0.25f, 0.2f);
            }

            // Create bar counter with matte brown material
            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "Bar Counter";
            counter.transform.position = new Vector3(0, 0.5f, 2);
            counter.transform.localScale = new Vector3(4, 1, 1);
            var counterRenderer = counter.GetComponent<Renderer>();
            if (counterRenderer != null)
            {
                // Create matte brown material with no reflection
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.35f, 0.2f, 0.1f); // Dark brown
                mat.SetFloat("_Glossiness", 0.1f); // Low smoothness = matte
                mat.SetFloat("_Metallic", 0f); // No metallic = no reflection
                counterRenderer.material = mat;
            }

            // Create bottles
            CreateBottleObject("Vodka Bottle", new Vector3(-1f, 1.2f, 2), "vodka");
            CreateBottleObject("Gin Bottle", new Vector3(-0.5f, 1.2f, 2), "gin");
            CreateBottleObject("Rum Bottle", new Vector3(0f, 1.2f, 2), "rum");
            CreateBottleObject("Whiskey Bottle", new Vector3(0.5f, 1.2f, 2), "whiskey");

            // Create glasses
            CreateGlassObject("Glass 1", new Vector3(1f, 1.1f, 2));
            CreateGlassObject("Glass 2", new Vector3(1.5f, 1.1f, 2));

            // Create NPC
            CreateNPCObject("Customer", new Vector3(0, 0, 4));

            Debug.Log("Created test objects: bottles, glasses, and NPC");
        }

        private static GameObject CreateBottleObject(string name, Vector3 position, string liquorId)
        {
            // Create bottle visual
            var bottle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bottle.name = name;
            bottle.transform.position = position;
            bottle.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);

            // Set color based on liquor type
            var renderer = bottle.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = liquorId switch
                {
                    "vodka" => new Color(0.9f, 0.9f, 1f, 0.5f),
                    "gin" => new Color(0.8f, 0.95f, 0.95f, 0.5f),
                    "rum" => new Color(0.95f, 0.85f, 0.7f, 0.5f),
                    "whiskey" => new Color(0.8f, 0.5f, 0.2f, 0.5f),
                    _ => Color.white
                };
                renderer.material.color = color;
            }

            // Add Rigidbody for physics (kinematic by default so it stays in place)
            var rb = bottle.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Add Bottle component
            var bottleComponent = bottle.AddComponent<Bottle>();

            // Set liquor ID via serialized field reflection
            var liquorIdField = typeof(Bottle).GetField("liquorId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (liquorIdField != null)
            {
                liquorIdField.SetValue(bottleComponent, liquorId);
            }

            // Set display name
            var displayNameField = typeof(Bottle).BaseType.GetField("displayName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (displayNameField != null)
            {
                displayNameField.SetValue(bottleComponent, name);
            }

            return bottle;
        }

        private static GameObject CreateGlassObject(string name, Vector3 position)
        {
            // Create glass visual (using cylinder)
            var glass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glass.name = name;
            glass.transform.position = position;
            glass.transform.localScale = new Vector3(0.08f, 0.1f, 0.08f);

            // Set transparent material for glass
            var glassRenderer = glass.GetComponent<Renderer>();
            if (glassRenderer != null)
            {
                var glassMat = new Material(Shader.Find("Standard"));
                glassMat.color = new Color(0.9f, 0.95f, 1f, 0.2f);
                glassMat.SetFloat("_Mode", 3); // Transparent
                glassMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                glassMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                glassMat.SetInt("_ZWrite", 0);
                glassMat.DisableKeyword("_ALPHATEST_ON");
                glassMat.EnableKeyword("_ALPHABLEND_ON");
                glassMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                glassMat.renderQueue = 3000;
                glassRenderer.material = glassMat;
            }

            // Create liquid cylinder inside the glass
            var liquid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            liquid.name = "Liquid";
            liquid.transform.SetParent(glass.transform);
            liquid.transform.localPosition = new Vector3(0f, 0.3f, 0f); // Start position
            liquid.transform.localScale = new Vector3(0.8f, 0.01f, 0.8f); // Initial tiny scale

            // Remove collider from liquid
            Object.DestroyImmediate(liquid.GetComponent<Collider>());

            // Set liquid material
            var liquidRenderer = liquid.GetComponent<MeshRenderer>();
            if (liquidRenderer != null)
            {
                var liquidMat = new Material(Shader.Find("Standard"));
                liquidMat.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                liquidMat.SetFloat("_Glossiness", 0.9f);
                liquidRenderer.material = liquidMat;
                liquidRenderer.enabled = false; // Initially hidden
            }

            // Add Rigidbody for physics (kinematic by default so it stays in place)
            var rb = glass.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Add Glass component
            var glassComponent = glass.AddComponent<Glass>();

            // Set liquid references via reflection
            var containerType = typeof(Glass).BaseType; // Container

            var liquidRendererField = containerType.GetField("liquidRenderer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (liquidRendererField != null && liquidRenderer != null)
            {
                liquidRendererField.SetValue(glassComponent, liquidRenderer);
            }

            var liquidTransformField = containerType.GetField("liquidTransform",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (liquidTransformField != null)
            {
                liquidTransformField.SetValue(glassComponent, liquid.transform);
            }

            // Set display name
            var displayNameField = typeof(Glass).BaseType.BaseType.GetField("displayName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (displayNameField != null)
            {
                displayNameField.SetValue(glassComponent, name);
            }

            return glass;
        }

        private static GameObject CreateNPCObject(string name, Vector3 position)
        {
            // Create NPC root
            var npc = new GameObject($"NPC_{name}");
            npc.transform.position = position;

            // Create body (capsule)
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(npc.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Remove collider from body
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // Set body color
            var bodyRenderer = body.GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = new Color(0.2f, 0.5f, 0.8f);
            }

            // Create head
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(npc.transform);
            head.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            head.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            // Remove collider from head
            Object.DestroyImmediate(head.GetComponent<Collider>());

            // Add main collider
            var collider = npc.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 1f, 0f);
            collider.height = 2f;
            collider.radius = 0.3f;

            // Add NPCController
            var controller = npc.AddComponent<NPCController>();

            // Initialize NPC data
            var npcData = new BarSimulator.Data.NPCData
            {
                npcName = name,
                position = position,
                dialogue = Resources.Load<NPCDialogue>("TestNPCDialogue")
            };
            controller.Initialize(npcData);

            return npc;
        }

        private static void CreateGameUI()
        {
            // Check if Canvas exists
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                // Create EventSystem first
                if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    var eventSystem = new GameObject("EventSystem");
                    eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }

                // Create Canvas
                var canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // Old UI components removed - GameUIController and GameplayHUD no longer exist
            Debug.Log("Canvas created - ready for new simple UI system");
        }
    }
}
