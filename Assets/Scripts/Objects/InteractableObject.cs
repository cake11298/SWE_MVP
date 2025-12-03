using UnityEngine;

namespace BarSimulator.Objects
{
    /// <summary>
    /// Base component for all interactable objects in the scene.
    /// Attach this to bottles, glasses, shakers, etc.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableObject : MonoBehaviour
    {
        [Header("Object Properties")]
        [SerializeField] private string objectName = "Object";
        [SerializeField] private InteractableType objectType = InteractableType.Generic;

        [Header("Interaction Settings")]
        [SerializeField] private bool canBePickedUp = true;
        [SerializeField] private bool canBePlaced = true;

        [Header("Visual Feedback")]
        [SerializeField] private bool highlightOnHover = true;
        [SerializeField] private Color highlightColor = Color.yellow;

        private Renderer objectRenderer;
        private Color originalColor;
        private bool isHighlighted = false;

        public enum InteractableType
        {
            Generic,
            Bottle,
            Glass,
            Shaker,
            Garnish,
            Tool
        }

        private void Awake()
        {
            // Get renderer for highlighting
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer != null && objectRenderer.material != null)
            {
                originalColor = objectRenderer.material.color;
            }

            // Ensure object is on Interactable layer
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            if (gameObject.layer == 0)
            {
                Debug.LogWarning($"[InteractableObject] {gameObject.name}: 'Interactable' layer not found. Create it in Project Settings.");
            }
        }

        // ===================================================================
        // Interaction API
        // ===================================================================

        public virtual void OnPickup()
        {
            if (!canBePickedUp)
            {
                Debug.LogWarning($"[InteractableObject] {objectName} cannot be picked up");
                return;
            }

            Debug.Log($"[InteractableObject] Picked up: {objectName}");
        }

        public virtual void OnPlace(Vector3 position)
        {
            if (!canBePlaced)
            {
                Debug.LogWarning($"[InteractableObject] {objectName} cannot be placed");
                return;
            }

            Debug.Log($"[InteractableObject] Placed: {objectName} at {position}");
        }

        public virtual void OnDrop()
        {
            Debug.Log($"[InteractableObject] Dropped: {objectName}");
        }

        public virtual void OnHoverEnter()
        {
            if (highlightOnHover && objectRenderer != null)
            {
                isHighlighted = true;
                objectRenderer.material.color = highlightColor;
            }
        }

        public virtual void OnHoverExit()
        {
            if (isHighlighted && objectRenderer != null)
            {
                isHighlighted = false;
                objectRenderer.material.color = originalColor;
            }
        }

        // ===================================================================
        // Properties
        // ===================================================================

        public string ObjectName => objectName;
        public InteractableType ObjectType => objectType;
        public bool CanBePickedUp => canBePickedUp;
        public bool CanBePlaced => canBePlaced;
    }
}
