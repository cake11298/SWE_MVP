using UnityEngine;
using BarSimulator.Data;

namespace BarSimulator.Managers
{
    /// <summary>
    /// Manages decorations in the scene based on purchase status
    /// Enables/disables decorations when the scene loads
    /// </summary>
    public class DecorationManager : MonoBehaviour
    {
        [Header("Decoration References")]
        [Tooltip("Parent object containing all speaker objects")]
        [SerializeField] private GameObject speakersParent;

        [Tooltip("Parent object containing all plant objects")]
        [SerializeField] private GameObject plantsParent;

        [Tooltip("Parent object containing all painting objects")]
        [SerializeField] private GameObject paintingsParent;

        private void Start()
        {
            // Apply decoration states based on purchase status
            ApplyDecorationStates();

            // Subscribe to purchase events
            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.OnDecorationPurchased += OnDecorationPurchased;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (PersistentGameData.Instance != null)
            {
                PersistentGameData.Instance.OnDecorationPurchased -= OnDecorationPurchased;
            }
        }

        /// <summary>
        /// Apply decoration states based on purchase status
        /// </summary>
        private void ApplyDecorationStates()
        {
            if (PersistentGameData.Instance == null)
            {
                Debug.LogWarning("DecorationManager: PersistentGameData not found!");
                return;
            }

            // Apply speaker state
            bool speakersPurchased = PersistentGameData.Instance.IsDecorationPurchased(DecorationType.Speaker);
            if (speakersParent != null)
            {
                speakersParent.SetActive(speakersPurchased);
                Debug.Log($"DecorationManager: Speakers {(speakersPurchased ? "enabled" : "disabled")}");
            }
            else
            {
                Debug.LogWarning("DecorationManager: Speakers parent not assigned! Please assign in inspector.");
            }

            // Apply plant state
            bool plantsPurchased = PersistentGameData.Instance.IsDecorationPurchased(DecorationType.Plant);
            if (plantsParent != null)
            {
                plantsParent.SetActive(plantsPurchased);
                Debug.Log($"DecorationManager: Plants {(plantsPurchased ? "enabled" : "disabled")}");
            }

            // Apply painting state
            bool paintingsPurchased = PersistentGameData.Instance.IsDecorationPurchased(DecorationType.Painting);
            if (paintingsParent != null)
            {
                paintingsParent.SetActive(paintingsPurchased);
                Debug.Log($"DecorationManager: Paintings {(paintingsPurchased ? "enabled" : "disabled")}");
            }
        }

        /// <summary>
        /// Called when a decoration is purchased
        /// Note: This won't take effect until the next game/scene reload
        /// </summary>
        private void OnDecorationPurchased(DecorationType type)
        {
            Debug.Log($"DecorationManager: {type} purchased! Will be enabled in next game.");
        }

        /// <summary>
        /// Find and assign decoration parents automatically
        /// </summary>
        [ContextMenu("Auto-Find Decoration Parents")]
        private void AutoFindDecorationParents()
        {
            // Try to find speakers in Props
            if (speakersParent == null)
            {
                GameObject props = GameObject.Find("Props");
                if (props != null)
                {
                    // Look for objects with "Speaker" in name
                    foreach (Transform child in props.transform)
                    {
                        if (child.name.Contains("Speaker"))
                        {
                            // Create a parent for all speakers if not exists
                            if (speakersParent == null)
                            {
                                speakersParent = new GameObject("Speakers");
                                speakersParent.transform.SetParent(props.transform);
                            }
                            child.SetParent(speakersParent.transform);
                        }
                    }
                }
            }

            Debug.Log("DecorationManager: Auto-find complete. Please verify assignments in inspector.");
        }
    }
}
