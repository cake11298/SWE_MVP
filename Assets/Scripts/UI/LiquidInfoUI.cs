using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BarSimulator.UI
{
    /// <summary>
    /// UI display for glass liquid contents.
    /// Shows capacity bar and ingredient list.
    /// Also shows pouring target information.
    /// </summary>
    public class LiquidInfoUI : MonoBehaviour
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

        [Tooltip("Title text for showing pouring target")]
        public Text titleText;

        [Header("Settings")]
        [Tooltip("Update interval in seconds")]
        public float updateInterval = 0.05f;

        // Private state
        private Objects.GlassContainer targetGlass;
        private float updateTimer = 0f;
        private string pouringTargetName = "";

        private void Awake()
        {
            // Hide panel initially
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }

        private void Update()
        {
            // Always update if we have a target glass
            if (targetGlass != null)
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// Set the target glass to display.
        /// </summary>
        public void SetTargetGlass(Objects.GlassContainer glass, string pouringTarget = "")
        {
            targetGlass = glass;
            pouringTargetName = pouringTarget;

            if (targetGlass != null)
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
        /// Clear the target glass.
        /// </summary>
        public void ClearTarget()
        {
            targetGlass = null;
            HideUI();
        }

        /// <summary>
        /// Update the UI display.
        /// </summary>
        private void UpdateUI()
        {
            if (targetGlass == null)
            {
                HideUI();
                return;
            }

            // Update title text with pouring target info
            if (titleText != null)
            {
                if (!string.IsNullOrEmpty(pouringTargetName))
                {
                    titleText.text = $"Pouring into {pouringTargetName}";
                }
                else
                {
                    titleText.text = "Glass Contents";
                }
            }

            // Update capacity slider
            if (capacitySlider != null)
            {
                capacitySlider.value = targetGlass.GetFillRatio();
            }

            // Update content text
            string contents = targetGlass.GetContentsString();
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
