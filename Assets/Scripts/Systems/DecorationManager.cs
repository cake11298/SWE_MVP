using UnityEngine;
using BarSimulator.Data;

namespace BarSimulator.Systems
{
    /// <summary>
    /// Manages decoration visibility based on purchase status
    /// Hides decorations until they are unlocked/purchased
    /// </summary>
    public class DecorationManager : MonoBehaviour
    {
        #region Singleton

        private static DecorationManager instance;

        public static DecorationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<DecorationManager>();
                }
                return instance;
            }
        }

        #endregion

        #region Serialized Fields

        [Header("Decoration References")]
        [Tooltip("Speaker decoration objects (SM_Speakers, SM_Speakers2)")]
        [SerializeField] private GameObject[] speakerObjects;

        [Tooltip("Plant decoration objects")]
        [SerializeField] private GameObject[] plantObjects;

        [Tooltip("Painting decoration objects")]
        [SerializeField] private GameObject[] paintingObjects;

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
        }

        private void Start()
        {
            // Auto-find decorations if not assigned
            if (speakerObjects == null || speakerObjects.Length == 0)
            {
                FindSpeakers();
            }

            // Subscribe to purchase events
            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.OnDecorationPurchased += OnDecorationPurchased;
            }

            // Initialize visibility based on current purchase status
            UpdateAllDecorationVisibility();
        }

        private void OnDestroy()
        {
            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.OnDecorationPurchased -= OnDecorationPurchased;
            }
        }

        #endregion

        #region Auto-Find Decorations

        /// <summary>
        /// Automatically find speaker objects in the scene
        /// </summary>
        private void FindSpeakers()
        {
            // Find all objects with "Speaker" in their name
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            System.Collections.Generic.List<GameObject> speakers = new System.Collections.Generic.List<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("SM_Speakers"))
                {
                    speakers.Add(obj);
                    Debug.Log($"DecorationManager: Found speaker object: {obj.name}");
                }
            }

            speakerObjects = speakers.ToArray();
        }

        #endregion

        #region Decoration Visibility

        /// <summary>
        /// Update all decoration visibility based on purchase status
        /// </summary>
        public void UpdateAllDecorationVisibility()
        {
            if (PersistentGameData.Instance == null) return;

            // Update speakers
            bool speakersPurchased = PersistentGameData.Instance.IsDecorationPurchased(DecorationType.Speaker);
            SetDecorationVisibility(speakerObjects, speakersPurchased);

            // Update plants
            bool plantsPurchased = PersistentGameData.Instance.IsDecorationPurchased(DecorationType.Plant);
            SetDecorationVisibility(plantObjects, plantsPurchased);

            // Update paintings
            bool paintingsPurchased = PersistentGameData.Instance.IsDecorationPurchased(DecorationType.Painting);
            SetDecorationVisibility(paintingObjects, paintingsPurchased);

            Debug.Log($"DecorationManager: Updated visibility - Speakers: {speakersPurchased}, Plants: {plantsPurchased}, Paintings: {paintingsPurchased}");
        }

        /// <summary>
        /// Set visibility for a group of decoration objects
        /// </summary>
        private void SetDecorationVisibility(GameObject[] objects, bool visible)
        {
            if (objects == null) return;

            foreach (GameObject obj in objects)
            {
                if (obj != null)
                {
                    obj.SetActive(visible);
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when a decoration is purchased
        /// </summary>
        private void OnDecorationPurchased(DecorationType type)
        {
            Debug.Log($"DecorationManager: Decoration purchased: {type}");

            switch (type)
            {
                case DecorationType.Speaker:
                    SetDecorationVisibility(speakerObjects, true);
                    break;
                case DecorationType.Plant:
                    SetDecorationVisibility(plantObjects, true);
                    break;
                case DecorationType.Painting:
                    SetDecorationVisibility(paintingObjects, true);
                    break;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually set speaker objects
        /// </summary>
        public void SetSpeakerObjects(GameObject[] speakers)
        {
            speakerObjects = speakers;
            UpdateAllDecorationVisibility();
        }

        /// <summary>
        /// Manually set plant objects
        /// </summary>
        public void SetPlantObjects(GameObject[] plants)
        {
            plantObjects = plants;
            UpdateAllDecorationVisibility();
        }

        /// <summary>
        /// Manually set painting objects
        /// </summary>
        public void SetPaintingObjects(GameObject[] paintings)
        {
            paintingObjects = paintings;
            UpdateAllDecorationVisibility();
        }

        #endregion
    }
}
