using UnityEngine;
using System.Collections.Generic;
using BarSimulator.Interaction;

namespace BarSimulator.Systems
{
    /// <summary>
    /// Placement preview system for showing where items will be placed
    /// Also handles smart snap points
    /// </summary>
    public class PlacementPreviewSystem : MonoBehaviour
    {
        #region Singleton

        private static PlacementPreviewSystem instance;
        public static PlacementPreviewSystem Instance => instance;

        #endregion

        #region Serialized Fields

        [Header("Preview Settings")]
        [SerializeField] private Color previewValidColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
        [SerializeField] private Color previewInvalidColor = new Color(0.8f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private bool showPreviewOnHold = true;

        [Header("Snap Points")]
        [SerializeField] private bool enableSnapPoints = true;
        [SerializeField] private float snapDistance = 0.5f;
        [SerializeField] private LayerMask snapSurfaceLayer;

        [Header("Surface Detection")]
        [SerializeField] private float surfaceCheckDistance = 3f;
        [SerializeField] private LayerMask surfaceLayer;

        #endregion

        #region Private Fields

        private Camera mainCamera;
        private GameObject previewObject;
        private MeshRenderer previewRenderer;
        private Material previewMaterial;

        private List<SnapPoint> snapPoints = new List<SnapPoint>();
        private SnapPoint currentSnapPoint;

        private bool isPreviewActive;
        private Vector3 currentPlacementPosition;
        private bool isValidPlacement;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            InitializeSnapPoints();
        }

        private void Start()
        {
            mainCamera = Camera.main;
            CreatePreviewMaterial();
        }

        private void Update()
        {
            if (showPreviewOnHold)
            {
                UpdatePreview();
            }
        }

        #endregion

        #region Initialization

        private void CreatePreviewMaterial()
        {
            previewMaterial = new Material(Shader.Find("Standard"));
            previewMaterial.SetFloat("_Mode", 3); // Transparent
            previewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            previewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            previewMaterial.SetInt("_ZWrite", 0);
            previewMaterial.DisableKeyword("_ALPHATEST_ON");
            previewMaterial.EnableKeyword("_ALPHABLEND_ON");
            previewMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            previewMaterial.renderQueue = 3000;
            previewMaterial.color = previewValidColor;
        }

        private void InitializeSnapPoints()
        {
            // Default bar snap points
            // These can be added dynamically or set in scene

            // Bar counter positions
            AddSnapPoint(new Vector3(-1.5f, 1.1f, -1.5f), SnapPointType.Counter, "Left Counter");
            AddSnapPoint(new Vector3(0f, 1.1f, -1.5f), SnapPointType.Counter, "Center Counter");
            AddSnapPoint(new Vector3(1.5f, 1.1f, -1.5f), SnapPointType.Counter, "Right Counter");

            // Shelf positions
            AddSnapPoint(new Vector3(-2f, 1.8f, -2.5f), SnapPointType.Shelf, "Left Shelf");
            AddSnapPoint(new Vector3(0f, 1.8f, -2.5f), SnapPointType.Shelf, "Center Shelf");
            AddSnapPoint(new Vector3(2f, 1.8f, -2.5f), SnapPointType.Shelf, "Right Shelf");
        }

        #endregion

        #region Preview Update

        private void UpdatePreview()
        {
            var interactionSystem = InteractionSystem.Instance;
            if (interactionSystem == null || !interactionSystem.IsHolding)
            {
                HidePreview();
                return;
            }

            // Show preview when holding Q
            if (Input.GetKey(KeyCode.Q))
            {
                ShowPlacementPreview(interactionSystem.HeldObject);
            }
            else
            {
                HidePreview();
            }
        }

        #endregion

        #region Preview Display

        /// <summary>
        /// Show placement preview for held object
        /// </summary>
        public void ShowPlacementPreview(IInteractable heldObject)
        {
            if (heldObject == null || mainCamera == null) return;

            var mono = heldObject as MonoBehaviour;
            if (mono == null) return;

            // Calculate placement position
            Vector3 placementPos = CalculatePlacementPosition();
            isValidPlacement = IsValidPlacement(placementPos);

            // Create or update preview object
            if (previewObject == null)
            {
                CreatePreviewObject(mono.gameObject);
            }

            // Update preview position and color
            if (previewObject != null)
            {
                previewObject.transform.position = placementPos;
                previewObject.SetActive(true);
                isPreviewActive = true;

                // Update color based on validity
                Color color = isValidPlacement ? previewValidColor : previewInvalidColor;
                UpdatePreviewColor(color);
            }

            currentPlacementPosition = placementPos;
        }

        /// <summary>
        /// Hide placement preview
        /// </summary>
        public void HidePreview()
        {
            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }
            isPreviewActive = false;
            currentSnapPoint = null;
        }

        private void CreatePreviewObject(GameObject original)
        {
            // Create a simple preview (cube or copy of mesh)
            var originalMesh = original.GetComponent<MeshFilter>();
            if (originalMesh != null && originalMesh.mesh != null)
            {
                previewObject = new GameObject("PlacementPreview");
                var meshFilter = previewObject.AddComponent<MeshFilter>();
                meshFilter.mesh = originalMesh.mesh;

                previewRenderer = previewObject.AddComponent<MeshRenderer>();
                previewRenderer.material = previewMaterial;

                previewObject.transform.localScale = original.transform.lossyScale;
            }
            else
            {
                // Fallback to simple cube
                previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                previewObject.name = "PlacementPreview";

                // Remove collider
                var collider = previewObject.GetComponent<Collider>();
                if (collider != null) Destroy(collider);

                previewRenderer = previewObject.GetComponent<MeshRenderer>();
                previewRenderer.material = previewMaterial;

                previewObject.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);
            }
        }

        private void UpdatePreviewColor(Color color)
        {
            if (previewRenderer != null && previewMaterial != null)
            {
                previewMaterial.color = color;
            }
        }

        #endregion

        #region Placement Calculation

        /// <summary>
        /// Calculate where to place the object
        /// </summary>
        public Vector3 CalculatePlacementPosition()
        {
            if (mainCamera == null) return Vector3.zero;

            // Raycast from camera
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, surfaceCheckDistance, surfaceLayer))
            {
                Vector3 hitPoint = hit.point;

                // Check for nearby snap point
                if (enableSnapPoints)
                {
                    SnapPoint nearestSnap = FindNearestSnapPoint(hitPoint);
                    if (nearestSnap != null)
                    {
                        currentSnapPoint = nearestSnap;
                        return nearestSnap.position;
                    }
                }

                // Offset slightly above surface
                return hitPoint + hit.normal * 0.05f;
            }

            // Default position in front of camera
            currentSnapPoint = null;
            return mainCamera.transform.position + mainCamera.transform.forward * 1.5f;
        }

        /// <summary>
        /// Check if placement position is valid
        /// </summary>
        public bool IsValidPlacement(Vector3 position)
        {
            // Check if on a surface
            if (Physics.Raycast(position + Vector3.up * 0.1f, Vector3.down, 0.2f, surfaceLayer))
            {
                return true;
            }

            // Check if at a snap point
            if (currentSnapPoint != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get current placement position
        /// </summary>
        public Vector3 GetPlacementPosition()
        {
            return currentPlacementPosition;
        }

        /// <summary>
        /// Check if current preview is valid
        /// </summary>
        public bool IsCurrentPlacementValid()
        {
            return isPreviewActive && isValidPlacement;
        }

        #endregion

        #region Snap Points

        /// <summary>
        /// Add a snap point
        /// </summary>
        public void AddSnapPoint(Vector3 position, SnapPointType type, string name = "")
        {
            SnapPoint point = new SnapPoint
            {
                position = position,
                type = type,
                name = name,
                isOccupied = false
            };

            snapPoints.Add(point);
        }

        /// <summary>
        /// Find nearest snap point
        /// </summary>
        public SnapPoint FindNearestSnapPoint(Vector3 position)
        {
            SnapPoint nearest = null;
            float nearestDist = snapDistance;

            foreach (var point in snapPoints)
            {
                if (point.isOccupied) continue;

                float dist = Vector3.Distance(position, point.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = point;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Mark snap point as occupied
        /// </summary>
        public void OccupySnapPoint(SnapPoint point)
        {
            if (point != null)
            {
                point.isOccupied = true;
            }
        }

        /// <summary>
        /// Free a snap point
        /// </summary>
        public void FreeSnapPoint(Vector3 position)
        {
            foreach (var point in snapPoints)
            {
                if (Vector3.Distance(point.position, position) < 0.1f)
                {
                    point.isOccupied = false;
                    break;
                }
            }
        }

        /// <summary>
        /// Get all snap points
        /// </summary>
        public List<SnapPoint> GetSnapPoints()
        {
            return snapPoints;
        }

        /// <summary>
        /// Clear all snap points
        /// </summary>
        public void ClearSnapPoints()
        {
            snapPoints.Clear();
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmos()
        {
            // Draw snap points in editor
            foreach (var point in snapPoints)
            {
                Gizmos.color = point.isOccupied ? Color.red : Color.green;
                Gizmos.DrawWireSphere(point.position, 0.1f);

                // Draw type indicator
                switch (point.type)
                {
                    case SnapPointType.Counter:
                        Gizmos.DrawWireCube(point.position, new Vector3(0.2f, 0.05f, 0.2f));
                        break;
                    case SnapPointType.Shelf:
                        Gizmos.DrawWireCube(point.position, new Vector3(0.15f, 0.02f, 0.1f));
                        break;
                }
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (previewObject != null)
            {
                Destroy(previewObject);
            }

            if (previewMaterial != null)
            {
                Destroy(previewMaterial);
            }
        }

        #endregion
    }

    #region Data Classes

    /// <summary>
    /// Snap point type
    /// </summary>
    public enum SnapPointType
    {
        Counter,
        Shelf,
        Table,
        Floor,
        Custom
    }

    /// <summary>
    /// Snap point data
    /// </summary>
    public class SnapPoint
    {
        public Vector3 position;
        public SnapPointType type;
        public string name;
        public bool isOccupied;
    }

    #endregion
}
