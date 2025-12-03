using UnityEngine;
using System.Collections.Generic;

namespace BarSimulator.Core
{
    /// <summary>
    /// Integrates with existing scene objects (purchased assets).
    /// Finds objects by tags/names and ensures they have proper colliders.
    /// Does NOT create primitives - works with what's already in the scene.
    /// </summary>
    public class SceneIntegrator : MonoBehaviour
    {
        [Header("Auto-Detection Settings")]
        [Tooltip("Try to find these objects by tag")]
        public bool autoFindByTags = true;

        [Header("Found Scene Objects (Auto-Populated)")]
        public GameObject barCounter;
        public GameObject liquorShelf;
        public GameObject orderPoint;
        public List<GameObject> existingBottles = new List<GameObject>();
        public List<GameObject> existingGlasses = new List<GameObject>();

        [Header("Physics Settings")]
        public bool autoAddColliders = true;
        public bool makeFloorsStatic = true;

        [Header("Debug")]
        public bool verboseLogging = true;

        public void Initialize()
        {
            if (verboseLogging)
            {
                Debug.Log("[SceneIntegrator] Scanning existing scene...");
            }

            // Find existing objects
            FindSceneObjects();

            // Ensure proper physics
            SetupPhysics();

            // Setup interaction layers
            SetupLayers();

            if (verboseLogging)
            {
                Debug.Log("[SceneIntegrator] ✅ Scene integration complete!");
            }
        }

        private void FindSceneObjects()
        {
            if (!autoFindByTags)
            {
                Debug.LogWarning("[SceneIntegrator] Auto-find disabled. Please assign objects manually in Inspector.");
                return;
            }

            // Find Bar Counter
            if (barCounter == null)
            {
                barCounter = GameObject.FindGameObjectWithTag("Bar");
                if (barCounter == null)
                {
                    // Try by name as fallback
                    barCounter = GameObject.Find("BarCounter")
                              ?? GameObject.Find("Bar")
                              ?? GameObject.Find("Counter");
                }

                if (barCounter != null && verboseLogging)
                {
                    Debug.Log($"[SceneIntegrator] ✓ Found Bar Counter: {barCounter.name}");
                }
            }

            // Find Liquor Shelf
            if (liquorShelf == null)
            {
                liquorShelf = GameObject.FindGameObjectWithTag("LiquorShelf");
                if (liquorShelf == null)
                {
                    liquorShelf = GameObject.Find("LiquorShelf")
                               ?? GameObject.Find("Shelf")
                               ?? GameObject.Find("Shelves");
                }

                if (liquorShelf != null && verboseLogging)
                {
                    Debug.Log($"[SceneIntegrator] ✓ Found Liquor Shelf: {liquorShelf.name}");
                }
            }

            // Find Order Point (where customers stand)
            if (orderPoint == null)
            {
                orderPoint = GameObject.FindGameObjectWithTag("OrderPoint");
                if (orderPoint == null)
                {
                    orderPoint = GameObject.Find("OrderPoint")
                              ?? GameObject.Find("CustomerSpawnPoint");
                }

                if (orderPoint != null && verboseLogging)
                {
                    Debug.Log($"[SceneIntegrator] ✓ Found Order Point: {orderPoint.name}");
                }
                else if (verboseLogging)
                {
                    Debug.LogWarning("[SceneIntegrator] Order Point not found. Create an empty GameObject named 'OrderPoint'.");
                }
            }

            // Find existing Bottles (look for objects with "Bottle" in name)
            existingBottles.Clear();
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("Bottle") || obj.CompareTag("Bottle"))
                {
                    existingBottles.Add(obj);
                }
            }

            if (verboseLogging && existingBottles.Count > 0)
            {
                Debug.Log($"[SceneIntegrator] ✓ Found {existingBottles.Count} existing bottles");
            }

            // Find existing Glasses
            existingGlasses.Clear();
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("Glass") || obj.CompareTag("Glass"))
                {
                    existingGlasses.Add(obj);
                }
            }

            if (verboseLogging && existingGlasses.Count > 0)
            {
                Debug.Log($"[SceneIntegrator] ✓ Found {existingGlasses.Count} existing glasses");
            }
        }

        private void SetupPhysics()
        {
            if (!autoAddColliders)
                return;

            // Ensure Bar Counter has collider
            if (barCounter != null)
            {
                EnsureCollider(barCounter, true);
            }

            // Ensure Liquor Shelf has collider
            if (liquorShelf != null)
            {
                EnsureCollider(liquorShelf, true);
            }

            // Find floor objects and make them static
            if (makeFloorsStatic)
            {
                GameObject[] floors = GameObject.FindGameObjectsWithTag("Floor");
                foreach (var floor in floors)
                {
                    EnsureCollider(floor, true);
                    floor.isStatic = true;
                }

                if (verboseLogging && floors.Length > 0)
                {
                    Debug.Log($"[SceneIntegrator] ✓ Configured {floors.Length} floor objects");
                }
            }

            // Setup bottles as interactable with Rigidbody
            foreach (var bottle in existingBottles)
            {
                SetupInteractableObject(bottle, "Bottle");
            }

            // Setup glasses as interactable with Rigidbody
            foreach (var glass in existingGlasses)
            {
                SetupInteractableObject(glass, "Glass");
            }
        }

        private void EnsureCollider(GameObject obj, bool isStatic = false)
        {
            if (obj == null)
                return;

            // Check if already has any collider
            Collider existingCollider = obj.GetComponent<Collider>();
            if (existingCollider != null)
            {
                if (verboseLogging)
                {
                    Debug.Log($"[SceneIntegrator] {obj.name} already has {existingCollider.GetType().Name}");
                }
                return;
            }

            // Try to add MeshCollider if it has a MeshFilter
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
                meshCollider.convex = !isStatic; // Non-static objects need convex colliders

                if (verboseLogging)
                {
                    Debug.Log($"[SceneIntegrator] ✓ Added MeshCollider to {obj.name}");
                }
            }
            else
            {
                // Fallback to BoxCollider
                obj.AddComponent<BoxCollider>();

                if (verboseLogging)
                {
                    Debug.Log($"[SceneIntegrator] ✓ Added BoxCollider to {obj.name}");
                }
            }
        }

        private void SetupInteractableObject(GameObject obj, string objectType)
        {
            if (obj == null)
                return;

            // Ensure collider
            EnsureCollider(obj, false);

            // Add Rigidbody if missing
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = obj.AddComponent<Rigidbody>();
                rb.mass = 0.5f;
                rb.drag = 1f;
                rb.angularDrag = 0.5f;

                if (verboseLogging)
                {
                    Debug.Log($"[SceneIntegrator] ✓ Added Rigidbody to {obj.name}");
                }
            }

            // Set layer to Interactable
            obj.layer = LayerMask.NameToLayer("Interactable");
            if (obj.layer == 0) // Layer doesn't exist yet
            {
                Debug.LogWarning($"[SceneIntegrator] 'Interactable' layer not found. Please create it in Edit > Project Settings > Tags and Layers.");
            }

            // Add tag
            if (obj.tag == "Untagged")
            {
                obj.tag = objectType;
            }
        }

        private void SetupLayers()
        {
            // Create layers if they don't exist (Unity will warn, but we document it)
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            int npcLayer = LayerMask.NameToLayer("NPC");

            if (interactableLayer == -1 || npcLayer == -1)
            {
                if (verboseLogging)
                {
                    Debug.LogWarning("[SceneIntegrator] Required layers missing. Please create:");
                    if (interactableLayer == -1)
                        Debug.LogWarning("  - 'Interactable' layer (for bottles, glasses, etc.)");
                    if (npcLayer == -1)
                        Debug.LogWarning("  - 'NPC' layer (for customers)");
                    Debug.LogWarning("Go to: Edit > Project Settings > Tags and Layers");
                }
            }
        }

        // ===================================================================
        // Public Helper Methods
        // ===================================================================

        public Vector3 GetBarCounterPosition()
        {
            return barCounter != null ? barCounter.transform.position : Vector3.zero;
        }

        public Vector3 GetOrderPointPosition()
        {
            return orderPoint != null ? orderPoint.transform.position : Vector3.forward * 2f;
        }

        public void RegisterBottle(GameObject bottle)
        {
            if (!existingBottles.Contains(bottle))
            {
                existingBottles.Add(bottle);
                SetupInteractableObject(bottle, "Bottle");
            }
        }

        public void RegisterGlass(GameObject glass)
        {
            if (!existingGlasses.Contains(glass))
            {
                existingGlasses.Add(glass);
                SetupInteractableObject(glass, "Glass");
            }
        }

        // ===================================================================
        // Debug Visualization
        // ===================================================================

        private void OnDrawGizmos()
        {
            if (barCounter != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(barCounter.transform.position, Vector3.one);
            }

            if (orderPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(orderPoint.transform.position, 0.5f);
            }
        }
    }
}
