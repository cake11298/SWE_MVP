using UnityEngine;
using System.Collections.Generic;
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
            GameObject props = GameObject.Find("Props");
            if (props == null) return;

            // Find Speakers
            if (speakersParent == null)
            {
                // Look for objects with "Speaker" in name
                List<Transform> speakers = new List<Transform>();
                foreach (Transform child in props.transform)
                {
                    if (child.name.Contains("Speaker"))
                    {
                        speakers.Add(child);
                    }
                }

                if (speakers.Count > 0)
                {
                    speakersParent = new GameObject("Speakers");
                    speakersParent.transform.SetParent(props.transform);
                    foreach (var speaker in speakers)
                    {
                        speaker.SetParent(speakersParent.transform);
                    }
                }
            }

            // Find Plants (Bamboo)
            if (plantsParent == null)
            {
                // Look for objects with "Bamboo" in name
                List<Transform> plants = new List<Transform>();
                foreach (Transform child in props.transform)
                {
                    if (child.name.Contains("Bamboo"))
                    {
                        plants.Add(child);
                    }
                }

                if (plants.Count > 0)
                {
                    plantsParent = new GameObject("Plants");
                    plantsParent.transform.SetParent(props.transform);
                    foreach (var plant in plants)
                    {
                        plant.SetParent(plantsParent.transform);
                    }
                }
            }

            // Find Paintings (SM_PannelFrame)
            if (paintingsParent == null)
            {
                // Look for objects with "SM_PannelFrame" in name
                List<Transform> paintings = new List<Transform>();
                foreach (Transform child in props.transform)
                {
                    if (child.name.Contains("SM_PannelFrame"))
                    {
                        paintings.Add(child);
                    }
                }

                if (paintings.Count > 0)
                {
                    paintingsParent = new GameObject("Paintings");
                    paintingsParent.transform.SetParent(props.transform);
                    foreach (var painting in paintings)
                    {
                        painting.SetParent(paintingsParent.transform);
                    }
                }
            }

            Debug.Log("DecorationManager: Auto-find complete. Please verify assignments in inspector.");
        }
    }
}
