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
        [SerializeField] private LayerMask interactableLayer = -1; // Default to all layers
        [SerializeField] private Transform handPosition;
        [SerializeField] private float raycastRadius = 0.1f; // Sphere cast radius for better detection

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

        [Header("Pouring System")]
        [SerializeField] private float pouringDistance = 2f;
        [SerializeField] private UI.LiquidInfoUI liquidInfoUI;

        [Header("References")]
        private Camera playerCamera;
        private GameObject highlightedObject;
        private InteractionHighlight highlightSystem;

        // Pouring state
        private bool isPouring = false;
        private Objects.LiquidContainer heldLiquidContainer;
        private Objects.GlassContainer targetGlass;

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

            // Setup layer mask if not set - use all layers by default
            if (interactableLayer == 0)
            {
                interactableLayer = -1; // All layers
                Debug.LogWarning("[PlayerInteraction] Layer mask not set. Using all layers.");
            }

            // Setup highlight system
            highlightSystem = gameObject.AddComponent<InteractionHighlight>();

            // Find LiquidInfoUI if not assigned
            if (liquidInfoUI == null)
            {
                liquidInfoUI = FindObjectOfType<UI.LiquidInfoUI>();
            }
        }

        private void Update()
        {
            // Perform raycast to detect interactable objects
            RaycastHit hit;
            bool hitSomething = PerformRaycast(out hit);

            // Update highlighted object
            GameObject newHighlight = null;
            if (hitSomething && heldObject == null)
            {
                newHighlight = hit.collider.gameObject;
            }

            // Update highlight visual
            if (newHighlight != highlightedObject)
            {
                highlightedObject = newHighlight;
                if (highlightSystem != null)
                {
                    if (highlightedObject != null)
                    {
                        highlightSystem.HighlightObject(highlightedObject);
                    }
                    else
                    {
                        highlightSystem.ClearHighlight();
                    }
                }
            }

            // Handle input
            HandleInteractionInput(hit, hitSomething);

            // Update held object position
            if (heldObject != null)
            {
                UpdateHeldObject();
            }

            // Handle pouring logic
            HandlePouring(hit, hitSomething);
        }

        private bool PerformRaycast(out RaycastHit hit)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

            // Use SphereCast for better detection
            bool didHit = Physics.SphereCast(ray, raycastRadius, out hit, interactionDistance, interactableLayer);
            
            // If sphere cast didn't hit, try regular raycast as backup
            if (!didHit)
            {
                didHit = Physics.Raycast(ray, out hit, interactionDistance, interactableLayer);
            }
            
            // Only consider objects with Rigidbody or InteractableItem
            if (didHit)
            {
                GameObject hitObj = hit.collider.gameObject;
                if (!IsInteractable(hitObj))
                {
                    didHit = false;
                }
            }

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
            // Pickup with E key
            bool pickupPressed = Input.GetKeyDown(pickupKey);
            // Drop with Q key
            bool dropPressed = Input.GetKeyDown(dropKey);
            
            // Mouse interactions
            bool leftClickDown = useMouseClick && Input.GetMouseButtonDown(0);
            bool leftClickUp = useMouseClick && Input.GetMouseButtonUp(0);
            bool rightClickDown = useMouseClick && Input.GetMouseButtonDown(1);

            // Pickup Logic (E or Left Click on object) - but not if we're pouring
            if ((pickupPressed || leftClickDown) && heldObject == null && hitSomething && !isPouring)
            {
                TryPickup(hit.collider.gameObject);
                return; // Don't process Use input in the same frame as pickup
            }

            // Place Logic (E or Right Click? No, let's keep Place separate or context sensitive)
            // Current logic: Pickup button also places if holding.
            
            if (pickupPressed && heldObject != null && hitSomething)
            {
                TryPlace(hit.point, hit.normal);
            }

            // Drop Logic (Q or Right Click)
            if ((dropPressed || rightClickDown) && heldObject != null)
            {
                Drop();
            }

            // Use Logic (Left Click Hold)
            if (heldObject != null)
            {
                var interactable = heldObject.GetComponent<BarSimulator.Interaction.IInteractable>();
                if (interactable != null)
                {
                    if (leftClickDown)
                    {
                        interactable.OnUseDown();
                    }
                    if (leftClickUp)
                    {
                        interactable.OnUseUp();
                    }
                }
            }
        }

        private void HandlePouring(RaycastHit hit, bool hitSomething)
        {
            // Check if we're looking at a glass (regardless of holding anything)
            Objects.GlassContainer glassInView = null;
            string targetName = "";
            
            if (hitSomething && hit.distance <= pouringDistance)
            {
                glassInView = hit.collider.GetComponent<Objects.GlassContainer>();
                if (glassInView != null)
                {
                    targetName = hit.collider.gameObject.name;
                }
            }

            // Check if we're holding a liquid container
            bool holdingLiquidContainer = heldObject != null && heldLiquidContainer != null;

            // Show UI when looking at a glass OR when holding a glass with liquid
            if (glassInView != null && liquidInfoUI != null)
            {
                // If holding a bottle and looking at glass, show "Pouring into X"
                if (holdingLiquidContainer)
                {
                    liquidInfoUI.SetTargetGlass(glassInView, targetName);
                }
                else
                {
                    liquidInfoUI.SetTargetGlass(glassInView);
                }
            }
            else if (liquidInfoUI != null && !isPouring)
            {
                // Check if we're holding a glass with liquid
                if (heldObject != null)
                {
                    var heldGlass = heldObject.GetComponent<Objects.GlassContainer>();
                    if (heldGlass != null && heldGlass.currentTotalVolume > 0)
                    {
                        liquidInfoUI.SetTargetGlass(heldGlass);
                    }
                    else
                    {
                        liquidInfoUI.ClearTarget();
                    }
                }
                else
                {
                    liquidInfoUI.ClearTarget();
                }
            }

            // Handle pouring logic
            if (holdingLiquidContainer)
            {
                // Handle pouring input (Left Mouse Button held)
                bool leftClickHeld = Input.GetMouseButton(0);

                if (leftClickHeld && glassInView != null && !glassInView.IsFull() && heldLiquidContainer.CanPour())
                {
                    // Start pouring
                    if (!isPouring)
                    {
                        isPouring = true;
                        targetGlass = glassInView;
                        heldLiquidContainer.StartPouring();
                        Debug.Log($"[PlayerInteraction] Started pouring {heldLiquidContainer.liquidName} into {targetGlass.name}");
                    }

                    // Pour liquid
                    float pourAmount = heldLiquidContainer.pourRate * Time.deltaTime;
                    float actualPoured = heldLiquidContainer.Pour(pourAmount);
                    float actualAdded = targetGlass.AddLiquid(heldLiquidContainer.liquidName, actualPoured);

                    // Update UI immediately with pouring target name
                    if (liquidInfoUI != null)
                    {
                        liquidInfoUI.SetTargetGlass(targetGlass, targetName);
                    }
                }
                else
                {
                    // Stop pouring
                    if (isPouring)
                    {
                        isPouring = false;
                        heldLiquidContainer.StopPouring();
                        Debug.Log($"[PlayerInteraction] Stopped pouring");
                        
                        // Keep UI visible if still looking at glass
                        if (glassInView != null && liquidInfoUI != null)
                        {
                            liquidInfoUI.SetTargetGlass(glassInView);
                        }
                        
                        targetGlass = null;
                    }
                }
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

            // Disable physics while holding
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.velocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;

            // Parent to hand position
            heldObject.transform.SetParent(handPosition);

            // Clear highlight
            if (highlightSystem != null)
            {
                highlightSystem.ClearHighlight();
            }

            // Notify StaticProp component if exists
            var staticProp = heldObject.GetComponent<StaticProp>();
            if (staticProp != null)
            {
                staticProp.OnPickup();
            }

            // Notify InteractableItem component if exists
            var interactableItem = heldObject.GetComponent<InteractableItem>();
            if (interactableItem != null)
            {
                interactableItem.OnPickedUp();
            }

            // Check if this is a liquid container
            heldLiquidContainer = heldObject.GetComponent<Objects.LiquidContainer>();

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

            // Notify StaticProp component if exists
            var staticProp = heldObject.GetComponent<StaticProp>();
            if (staticProp != null)
            {
                staticProp.OnDrop();
            }

            // Notify InteractableItem component if exists
            var interactableItem = heldObject.GetComponent<InteractableItem>();
            if (interactableItem != null)
            {
                interactableItem.OnDropped();
            }

            Debug.Log($"[PlayerInteraction] Placed: {heldObject.name} at {position}");

            // Stop pouring if active
            if (isPouring && heldLiquidContainer != null)
            {
                heldLiquidContainer.StopPouring();
                isPouring = false;
            }

            // Clear references
            heldObject = null;
            heldRigidbody = null;
            heldLiquidContainer = null;
            
            // Clear UI
            if (liquidInfoUI != null)
            {
                liquidInfoUI.ClearTarget();
            }
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

            // Notify StaticProp component if exists
            var staticProp = heldObject.GetComponent<StaticProp>();
            if (staticProp != null)
            {
                staticProp.OnDrop();
            }

            // Notify InteractableItem component if exists
            var interactableItem = heldObject.GetComponent<InteractableItem>();
            if (interactableItem != null)
            {
                interactableItem.OnDropped();
            }

            Debug.Log($"[PlayerInteraction] Dropped: {heldObject.name}");

            // Stop pouring if active
            if (isPouring && heldLiquidContainer != null)
            {
                heldLiquidContainer.StopPouring();
                isPouring = false;
            }

            // Clear references
            heldObject = null;
            heldRigidbody = null;
            heldLiquidContainer = null;
            
            // Clear UI
            if (liquidInfoUI != null)
            {
                liquidInfoUI.ClearTarget();
            }
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
            // Check if object has Rigidbody (required for pickup)
            if (obj.GetComponent<Rigidbody>() == null)
                return false;
            
            // Check if object has InteractableItem or StaticProp component
            bool hasInteractableComponent = obj.GetComponent<InteractableItem>() != null || 
                                           obj.GetComponent<StaticProp>() != null;
            
            return hasInteractableComponent;
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
