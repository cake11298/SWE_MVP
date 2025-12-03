using UnityEngine;
using BarSimulator.Core;

namespace BarSimulator.Player
{
    /// <summary>
    /// Handles player interaction with objects in the scene.
    /// Raycast-based pickup and drop system.
    /// Attach this to the Player GameObject.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private Transform handPosition;

        [Header("Input Settings")]
        [SerializeField] private KeyCode pickupKey = KeyCode.E;
        [SerializeField] private KeyCode dropKey = KeyCode.Q;
        [SerializeField] private bool useMouseClick = true;

        [Header("Held Object")]
        [SerializeField] private GameObject heldObject;
        [SerializeField] private Rigidbody heldRigidbody;

        [Header("Visual Feedback")]
        [SerializeField] private bool showDebugRay = true;
        [SerializeField] private Color rayHitColor = Color.green;
        [SerializeField] private Color rayMissColor = Color.red;

        [Header("References")]
        private Camera playerCamera;
        private GameObject highlightedObject;

        private void Awake()
        {
            // Get camera (either attached to this GameObject or find main camera)
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (playerCamera == null)
            {
                Debug.LogError("[PlayerInteraction] No camera found!");
                enabled = false;
                return;
            }

            // Create hand position if not assigned
            if (handPosition == null)
            {
                GameObject handObj = new GameObject("HandPosition");
                handObj.transform.SetParent(playerCamera.transform);
                handObj.transform.localPosition = new Vector3(0.3f, -0.3f, 0.6f);
                handPosition = handObj.transform;
            }

            // Setup layer mask if not set
            if (interactableLayer == 0)
            {
                interactableLayer = LayerMask.GetMask("Interactable");
                if (interactableLayer == 0)
                {
                    Debug.LogWarning("[PlayerInteraction] Interactable layer not found. Using Default layer.");
                    interactableLayer = LayerMask.GetMask("Default");
                }
            }
        }

        private void Update()
        {
            // Perform raycast to detect interactable objects
            RaycastHit hit;
            bool hitSomething = PerformRaycast(out hit);

            // Update highlighted object
            if (hitSomething && heldObject == null)
            {
                highlightedObject = hit.collider.gameObject;
            }
            else
            {
                highlightedObject = null;
            }

            // Handle input
            HandleInteractionInput(hit, hitSomething);

            // Update held object position
            if (heldObject != null)
            {
                UpdateHeldObject();
            }
        }

        private bool PerformRaycast(out RaycastHit hit)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

            bool didHit = Physics.Raycast(ray, out hit, interactionDistance, interactableLayer);

            // Debug visualization
            if (showDebugRay)
            {
                Color rayColor = didHit ? rayHitColor : rayMissColor;
                Vector3 endPoint = didHit ? hit.point : ray.origin + ray.direction * interactionDistance;
                Debug.DrawLine(ray.origin, endPoint, rayColor);
            }

            return didHit;
        }

        private void HandleInteractionInput(RaycastHit hit, bool hitSomething)
        {
            // Pickup with E key or left mouse button
            bool pickupPressed = Input.GetKeyDown(pickupKey) || (useMouseClick && Input.GetMouseButtonDown(0));

            // Drop with Q key or right mouse button
            bool dropPressed = Input.GetKeyDown(dropKey) || (useMouseClick && Input.GetMouseButtonDown(1));

            if (pickupPressed)
            {
                if (heldObject == null && hitSomething)
                {
                    // Try to pickup the object
                    TryPickup(hit.collider.gameObject);
                }
                else if (heldObject != null && hitSomething)
                {
                    // Try to place on the hit surface
                    TryPlace(hit.point, hit.normal);
                }
            }

            if (dropPressed && heldObject != null)
            {
                // Drop the held object
                Drop();
            }
        }

        private void TryPickup(GameObject target)
        {
            // Check if object is interactable
            if (!IsInteractable(target))
            {
                Debug.Log($"[PlayerInteraction] {target.name} is not interactable");
                return;
            }

            // Get Rigidbody
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogWarning($"[PlayerInteraction] {target.name} has no Rigidbody!");
                return;
            }

            // Pick up the object
            heldObject = target;
            heldRigidbody = rb;

            // Disable physics
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;

            // Parent to hand position
            heldObject.transform.SetParent(handPosition);

            Debug.Log($"[PlayerInteraction] Picked up: {heldObject.name}");

            // Publish event (optional, for EventBus integration)
            if (EventBus.GetSubscriberCount<InteractionInputEvent>() > 0)
            {
                EventBus.Publish(new InteractionInputEvent());
            }
        }

        private void TryPlace(Vector3 position, Vector3 normal)
        {
            if (heldObject == null)
                return;

            // Unparent from hand
            heldObject.transform.SetParent(null);

            // Position slightly above the surface
            heldObject.transform.position = position + normal * 0.1f;

            // Re-enable physics
            heldRigidbody.isKinematic = false;
            heldRigidbody.useGravity = true;

            Debug.Log($"[PlayerInteraction] Placed: {heldObject.name} at {position}");

            // Clear references
            heldObject = null;
            heldRigidbody = null;
        }

        private void Drop()
        {
            if (heldObject == null)
                return;

            // Unparent from hand
            heldObject.transform.SetParent(null);

            // Re-enable physics
            heldRigidbody.isKinematic = false;
            heldRigidbody.useGravity = true;

            // Add slight forward velocity
            heldRigidbody.velocity = playerCamera.transform.forward * 2f;

            Debug.Log($"[PlayerInteraction] Dropped: {heldObject.name}");

            // Clear references
            heldObject = null;
            heldRigidbody = null;
        }

        private void UpdateHeldObject()
        {
            if (heldObject == null || handPosition == null)
                return;

            // Smoothly move to hand position
            heldObject.transform.localPosition = Vector3.Lerp(
                heldObject.transform.localPosition,
                Vector3.zero,
                Time.deltaTime * 10f
            );

            // Smoothly rotate to hand rotation
            heldObject.transform.localRotation = Quaternion.Slerp(
                heldObject.transform.localRotation,
                Quaternion.identity,
                Time.deltaTime * 5f
            );
        }

        private bool IsInteractable(GameObject obj)
        {
            // Check if object is on the Interactable layer
            int objLayer = obj.layer;
            return (interactableLayer & (1 << objLayer)) != 0;
        }

        // ===================================================================
        // Public API
        // ===================================================================

        public bool IsHoldingObject()
        {
            return heldObject != null;
        }

        public GameObject GetHeldObject()
        {
            return heldObject;
        }

        public void ForceRelease()
        {
            if (heldObject != null)
            {
                Drop();
            }
        }

        // ===================================================================
        // GUI Debug
        // ===================================================================

        private void OnGUI()
        {
            if (!showDebugRay)
                return;

            GUILayout.BeginArea(new Rect(Screen.width / 2 - 100, Screen.height - 100, 200, 100));

            if (heldObject != null)
            {
                GUILayout.Label($"Holding: {heldObject.name}");
                GUILayout.Label("Left Click / E: Place");
                GUILayout.Label("Q: Drop");
            }
            else if (highlightedObject != null)
            {
                GUILayout.Label($"Look at: {highlightedObject.name}");
                GUILayout.Label("Left Click / E: Pick up");
            }
            else
            {
                GUILayout.Label("Look at an object to interact");
            }

            GUILayout.EndArea();

            // Crosshair
            float crosshairSize = 10f;
            GUI.Box(new Rect(Screen.width / 2 - crosshairSize / 2, Screen.height / 2 - 1, crosshairSize, 2), "");
            GUI.Box(new Rect(Screen.width / 2 - 1, Screen.height / 2 - crosshairSize / 2, 2, crosshairSize), "");
        }
    }
}
