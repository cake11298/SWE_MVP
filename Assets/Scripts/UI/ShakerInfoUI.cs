using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BarSimulator.UI
{
    /// <summary>
    /// UI display specifically for Shaker contents when held.
    /// </summary>
    public class ShakerInfoUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Slider for capacity visualization")]
        public Slider capacitySlider;

        [Tooltip("Text component for content list (TMP or regular Text)")]
        public TextMeshProUGUI contentTextTMP;

        [Tooltip("Regular Text component (fallback)")]
        public Text contentText;

        [Tooltip("Panel to show/hide")]
        public GameObject infoPanel;

        [Tooltip("Title text")]
        public Text titleText;

        // Private state
        private Objects.Shaker targetShaker;

        private void Awake()
        {
            // Hide panel by default
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (targetShaker != null)
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// Set the target shaker to display.
        /// </summary>
        public void SetTargetShaker(Objects.Shaker shaker)
        {
            targetShaker = shaker;

            if (targetShaker != null)
            {
                ShowUI();
                UpdateUI();
            }
            else
            {
                HideUI();
            }
        }

        /// <summary>
        /// Clear the target.
        /// </summary>
        public void ClearTarget()
        {
            targetShaker = null;
            HideUI();
        }

        /// <summary>
        /// Update the UI display.
        /// </summary>
        private void UpdateUI()
        {
            if (targetShaker == null)
            {
                HideUI();
                return;
            }

            if (titleText != null)
            {
                titleText.text = "Shaker Contents";
            }

            float fillRatio = targetShaker.FillRatio;
            string contents = targetShaker.GetContentsString();

            // Update capacity slider
            if (capacitySlider != null)
            {
                capacitySlider.value = fillRatio;
            }

            // Update content text
            if (contentTextTMP != null)
            {
                contentTextTMP.text = contents;
            }
            else if (contentText != null)
            {
                contentText.text = contents;
            }
        }

        /// <summary>
        /// Show the UI panel.
        /// </summary>
        private void ShowUI()
        {
            if (infoPanel != null && !infoPanel.activeSelf)
            {
                infoPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the UI panel.
        /// </summary>
        private void HideUI()
        {
            if (infoPanel != null && infoPanel.activeSelf)
            {
                infoPanel.SetActive(false);
            }
        }
    }
}
