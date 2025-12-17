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

        [Header("Liquor Levels Display")]
        [Tooltip("Container for liquor level items")]
        public Transform liquorLevelsContainer;

        [Tooltip("Text for displaying liquor levels")]
        public Text liquorLevelsText;

        [Header("Settings")]
        [Tooltip("Update interval in seconds")]
        public float updateInterval = 0.05f;

        // Private state
        private Objects.GlassContainer targetGlass;
        private Objects.Container targetContainer;
        private float updateTimer = 0f;
        private string pouringTargetName = "";

        private void Awake()
        {
            // Show panel by default
            if (infoPanel != null)
            {
                infoPanel.SetActive(true);
            }

            // Initialize liquor levels display
            UpdateLiquorLevelsDisplay();
        }

        private void Update()
        {
            // Always update if we have a target glass
            if (targetGlass != null)
            {
                UpdateUI();
            }

            // Update liquor levels periodically
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                UpdateLiquorLevelsDisplay();
                updateTimer = 0f;
            }
        }

        /// <summary>
        /// Set the target glass to display.
        /// </summary>
        public void SetTargetGlass(Objects.GlassContainer glass, string pouringTarget = "")
        {
            targetGlass = glass;
            targetContainer = null;
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
        /// Set the target container to display (supports Shaker, Glass, etc).
        /// </summary>
        public void SetTargetContainer(Objects.Container container, string pouringTarget = "")
        {
            targetContainer = container;
            targetGlass = null;
            pouringTargetName = pouringTarget;

            if (targetContainer != null)
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
            targetGlass = null;
            targetContainer = null;
            HideUI();
        }

        /// <summary>
        /// Update the UI display.
        /// </summary>
        private void UpdateUI()
        {
            if (targetGlass == null && targetContainer == null)
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
                    if (targetContainer is Objects.Shaker)
                        titleText.text = "Shaker Contents";
                    else
                        titleText.text = "Glass Contents";
                }
            }

            float fillRatio = 0f;
            string contents = "";

            if (targetContainer != null)
            {
                fillRatio = targetContainer.FillRatio;
                contents = targetContainer.GetContentsString();
            }
            else if (targetGlass != null)
            {
                fillRatio = targetGlass.GetFillRatio();
                contents = targetGlass.GetContentsString();
            }

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

        /// <summary>
        /// Update the liquor levels display showing 5 base liquors with their levels
        /// </summary>
        private void UpdateLiquorLevelsDisplay()
        {
            if (liquorLevelsText == null) return;

            var persistentData = Data.PersistentGameData.Instance;
            if (persistentData == null)
            {
                liquorLevelsText.text = "Liquor Levels: N/A";
                return;
            }

            // Get all liquor upgrades
            var upgrades = persistentData.GetAllLiquorUpgrades();
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Base Liquors ===");

            foreach (var upgrade in upgrades)
            {
                string liquorName = upgrade.liquorType.ToString();
                int level = upgrade.level;
                int maxLevel = Data.LiquorUpgradeData.MaxLevel;

                // Create level bar (e.g., "★★★☆☆")
                string levelBar = "";
                for (int i = 0; i < maxLevel; i++)
                {
                    levelBar += (i < level) ? "★" : "☆";
                }

                sb.AppendLine($"{liquorName}: {levelBar} Lv.{level}");
            }

            liquorLevelsText.text = sb.ToString();
        }
    }
}
