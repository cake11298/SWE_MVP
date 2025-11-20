using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BarSimulator.Objects;

namespace BarSimulator.UI
{
    /// <summary>
    /// Pour Progress UI - Shows pouring progress and container volume
    /// Reference: CocktailSystem.js updatePourProgressUI() Line 593-619
    /// </summary>
    public class PourProgressUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panel")]
        [Tooltip("Main panel canvas group")]
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Container Volume")]
        [Tooltip("Container volume progress bar")]
        [SerializeField] private Image containerVolumeBar;

        [Tooltip("Container volume text")]
        [SerializeField] private TextMeshProUGUI containerVolumeText;

        [Header("Pour Amount")]
        [Tooltip("Pour amount progress bar")]
        [SerializeField] private Image pourAmountBar;

        [Tooltip("Pour amount text")]
        [SerializeField] private TextMeshProUGUI pourAmountText;

        [Header("Settings")]
        [Tooltip("Hide delay after pouring stops (seconds)")]
        [SerializeField] private float hideDelay = 5f;

        [Tooltip("Fade speed")]
        [SerializeField] private float fadeSpeed = 5f;

        #endregion

        #region Private Fields

        private bool isVisible;
        private float hideTimer;
        private float currentPourAmount;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = 0f;
        }

        private void Update()
        {
            UpdateVisibility();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update progress display
        /// </summary>
        public void UpdateProgress(Container container, float pourAmount)
        {
            if (container == null) return;

            isVisible = true;
            hideTimer = hideDelay;
            currentPourAmount = pourAmount;

            // Update container volume bar
            float volumePercent = container.FillRatio;
            if (containerVolumeBar != null)
            {
                containerVolumeBar.fillAmount = volumePercent;
            }

            if (containerVolumeText != null)
            {
                containerVolumeText.text = $"{container.Volume:F0}/{container.MaxVolume:F0}ml";
            }

            // Update pour amount bar
            float pourPercent = pourAmount / container.MaxVolume;
            if (pourAmountBar != null)
            {
                pourAmountBar.fillAmount = Mathf.Clamp01(pourPercent);
            }

            if (pourAmountText != null)
            {
                pourAmountText.text = $"{pourAmount:F0}ml";
            }
        }

        /// <summary>
        /// Called when pouring completes
        /// </summary>
        public void OnPourComplete(float totalAmount)
        {
            // Start hide timer
            hideTimer = hideDelay;
            currentPourAmount = 0f;
        }

        /// <summary>
        /// Show the panel
        /// </summary>
        public void Show()
        {
            isVisible = true;
            hideTimer = hideDelay;
        }

        /// <summary>
        /// Hide the panel immediately
        /// </summary>
        public void Hide()
        {
            isVisible = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update visibility with fade
        /// </summary>
        private void UpdateVisibility()
        {
            if (panelCanvasGroup == null) return;

            // Update hide timer
            if (isVisible && hideTimer > 0f)
            {
                hideTimer -= Time.deltaTime;
                if (hideTimer <= 0f)
                {
                    isVisible = false;
                }
            }

            // Fade
            float targetAlpha = isVisible ? 1f : 0f;
            panelCanvasGroup.alpha = Mathf.Lerp(
                panelCanvasGroup.alpha,
                targetAlpha,
                fadeSpeed * Time.deltaTime
            );
        }

        #endregion
    }
}
