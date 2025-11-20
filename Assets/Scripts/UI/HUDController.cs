using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BarSimulator.UI
{
    /// <summary>
    /// HUD Controller - Manages main HUD elements (crosshair, hints, messages)
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Crosshair")]
        [Tooltip("Crosshair image")]
        [SerializeField] private Image crosshair;

        [Tooltip("Default crosshair color")]
        [SerializeField] private Color defaultCrosshairColor = Color.white;

        [Tooltip("Interactive crosshair color")]
        [SerializeField] private Color interactiveCrosshairColor = Color.yellow;

        [Header("Interaction Hint")]
        [Tooltip("Interaction hint text")]
        [SerializeField] private TextMeshProUGUI interactionHintText;

        [Tooltip("Hint background panel")]
        [SerializeField] private CanvasGroup hintCanvasGroup;

        [Tooltip("Hint fade speed")]
        [SerializeField] private float hintFadeSpeed = 5f;

        [Header("Message Display")]
        [Tooltip("Message text")]
        [SerializeField] private TextMeshProUGUI messageText;

        [Tooltip("Message background")]
        [SerializeField] private CanvasGroup messageCanvasGroup;

        [Tooltip("Message display duration")]
        [SerializeField] private float messageDuration = 3f;

        #endregion

        #region Private Fields

        private bool showHint;
        private float messageTimer;
        private bool showMessage;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Initialize UI state
            if (hintCanvasGroup != null)
                hintCanvasGroup.alpha = 0f;

            if (messageCanvasGroup != null)
                messageCanvasGroup.alpha = 0f;
        }

        private void Update()
        {
            UpdateHintVisibility();
            UpdateMessageVisibility();
        }

        #endregion

        #region Hint Management

        /// <summary>
        /// Set interaction hint text
        /// </summary>
        public void SetInteractionHint(string hint)
        {
            if (interactionHintText == null) return;

            if (string.IsNullOrEmpty(hint))
            {
                showHint = false;
                UpdateCrosshairColor(false);
            }
            else
            {
                interactionHintText.text = hint;
                showHint = true;
                UpdateCrosshairColor(true);
            }
        }

        /// <summary>
        /// Update hint visibility with fade
        /// </summary>
        private void UpdateHintVisibility()
        {
            if (hintCanvasGroup == null) return;

            float targetAlpha = showHint ? 1f : 0f;
            hintCanvasGroup.alpha = Mathf.Lerp(
                hintCanvasGroup.alpha,
                targetAlpha,
                hintFadeSpeed * Time.deltaTime
            );
        }

        /// <summary>
        /// Update crosshair color based on interaction state
        /// </summary>
        private void UpdateCrosshairColor(bool isInteractive)
        {
            if (crosshair == null) return;
            crosshair.color = isInteractive ? interactiveCrosshairColor : defaultCrosshairColor;
        }

        #endregion

        #region Message Display

        /// <summary>
        /// Show a temporary message
        /// </summary>
        public void ShowMessage(string message, float duration = -1f)
        {
            if (messageText == null) return;

            messageText.text = message;
            messageTimer = duration > 0 ? duration : messageDuration;
            showMessage = true;
        }

        /// <summary>
        /// Show drink completion message
        /// </summary>
        public void ShowDrinkMessage(string cocktailName, float volume)
        {
            string message = $"You drank {cocktailName}! Volume: {volume:F0}ml";
            ShowMessage(message);
        }

        /// <summary>
        /// Update message visibility
        /// </summary>
        private void UpdateMessageVisibility()
        {
            if (messageCanvasGroup == null) return;

            if (showMessage)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer <= 0f)
                {
                    showMessage = false;
                }
            }

            float targetAlpha = showMessage ? 1f : 0f;
            messageCanvasGroup.alpha = Mathf.Lerp(
                messageCanvasGroup.alpha,
                targetAlpha,
                hintFadeSpeed * Time.deltaTime
            );
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Hide all HUD elements
        /// </summary>
        public void HideAll()
        {
            showHint = false;
            showMessage = false;
        }

        /// <summary>
        /// Set crosshair visibility
        /// </summary>
        public void SetCrosshairVisible(bool visible)
        {
            if (crosshair != null)
                crosshair.enabled = visible;
        }

        #endregion
    }
}
