using UnityEngine;
using BarSimulator.NPC;

namespace BarSimulator.UI
{
    /// <summary>
    /// Patience bar UI for displaying NPC order timeout
    /// </summary>
    public class PatienceBarUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Bar Settings")]
        [SerializeField] private float barWidth = 100f;
        [SerializeField] private float barHeight = 10f;
        [SerializeField] private float verticalOffset = 2.5f;

        [Header("Colors")]
        [SerializeField] private Color fullColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color midColor = new Color(1f, 0.8f, 0f);
        [SerializeField] private Color lowColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        [Header("Style")]
        [SerializeField] private bool showOrderText = true;
        [SerializeField] private int fontSize = 12;

        #endregion

        #region Private Fields

        private Camera mainCamera;
        private GUIStyle barBackgroundStyle;
        private GUIStyle barFillStyle;
        private GUIStyle textStyle;
        private bool stylesInitialized;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void OnGUI()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            if (!stylesInitialized)
            {
                InitializeStyles();
            }

            // Draw patience bars for all NPCs with active orders
            var npcs = FindObjectsOfType<NPCController>();
            foreach (var npc in npcs)
            {
                if (npc.HasActiveOrder)
                {
                    DrawPatienceBar(npc);
                }
            }
        }

        #endregion

        #region Style Initialization

        private void InitializeStyles()
        {
            barBackgroundStyle = new GUIStyle();
            Texture2D bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, backgroundColor);
            bgTex.Apply();
            barBackgroundStyle.normal.background = bgTex;

            barFillStyle = new GUIStyle();
            Texture2D fillTex = new Texture2D(1, 1);
            fillTex.SetPixel(0, 0, fullColor);
            fillTex.Apply();
            barFillStyle.normal.background = fillTex;

            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = fontSize;
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.normal.textColor = Color.white;
            textStyle.fontStyle = FontStyle.Bold;

            // Add text shadow
            textStyle.normal.background = null;

            stylesInitialized = true;
        }

        #endregion

        #region Drawing

        private void DrawPatienceBar(NPCController npc)
        {
            if (npc == null) return;

            // Get screen position
            Vector3 worldPos = npc.transform.position + Vector3.up * verticalOffset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

            // Check if in front of camera
            if (screenPos.z <= 0) return;

            // Check if on screen
            if (screenPos.x < 0 || screenPos.x > Screen.width ||
                screenPos.y < 0 || screenPos.y > Screen.height) return;

            // Convert to GUI coordinates (Y is flipped)
            float guiY = Screen.height - screenPos.y;

            // Calculate bar position
            Rect bgRect = new Rect(
                screenPos.x - barWidth / 2f,
                guiY - barHeight / 2f,
                barWidth,
                barHeight
            );

            // Get patience ratio
            float patienceRatio = npc.PatienceRatio;

            // Calculate fill width
            float fillWidth = barWidth * Mathf.Clamp01(patienceRatio);
            Rect fillRect = new Rect(
                bgRect.x,
                bgRect.y,
                fillWidth,
                barHeight
            );

            // Get color based on patience
            Color barColor = GetPatienceColor(patienceRatio);

            // Update fill texture color
            Texture2D fillTex = new Texture2D(1, 1);
            fillTex.SetPixel(0, 0, barColor);
            fillTex.Apply();
            barFillStyle.normal.background = fillTex;

            // Draw background
            GUI.Box(bgRect, "", barBackgroundStyle);

            // Draw fill
            GUI.Box(fillRect, "", barFillStyle);

            // Draw border
            DrawBorder(bgRect, Color.black, 1);

            // Draw order text
            if (showOrderText && npc.CurrentOrder != null)
            {
                string orderText = npc.CurrentOrder.drinkName;
                string requirements = npc.CurrentOrder.GetRequirementsText();
                if (!string.IsNullOrEmpty(requirements))
                {
                    orderText += $"\n({requirements})";
                }

                Rect textRect = new Rect(
                    bgRect.x - 50f,
                    bgRect.y - fontSize - 4,
                    barWidth + 100f,
                    fontSize + 4
                );

                // Draw text shadow
                GUI.color = new Color(0, 0, 0, 0.7f);
                Rect shadowRect = new Rect(textRect.x + 1, textRect.y + 1, textRect.width, textRect.height);
                GUI.Label(shadowRect, orderText, textStyle);

                // Draw text
                GUI.color = Color.white;
                GUI.Label(textRect, orderText, textStyle);
            }

            // Draw time remaining
            float timeRemaining = npc.PatienceRemaining;
            string timeText = $"{timeRemaining:F0}s";

            Rect timeRect = new Rect(
                bgRect.x + bgRect.width + 5,
                bgRect.y,
                50,
                barHeight
            );

            textStyle.alignment = TextAnchor.MiddleLeft;
            textStyle.fontSize = fontSize - 2;
            GUI.color = barColor;
            GUI.Label(timeRect, timeText, textStyle);

            // Reset
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.fontSize = fontSize;
            GUI.color = Color.white;
        }

        private void DrawBorder(Rect rect, Color color, int thickness)
        {
            Texture2D borderTex = new Texture2D(1, 1);
            borderTex.SetPixel(0, 0, color);
            borderTex.Apply();

            // Top
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), borderTex);
            // Bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), borderTex);
            // Left
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), borderTex);
            // Right
            GUI.DrawTexture(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), borderTex);

            Object.Destroy(borderTex);
        }

        private Color GetPatienceColor(float ratio)
        {
            if (ratio > 0.5f)
            {
                // Full to mid
                float t = (ratio - 0.5f) * 2f;
                return Color.Lerp(midColor, fullColor, t);
            }
            else
            {
                // Mid to low
                float t = ratio * 2f;
                return Color.Lerp(lowColor, midColor, t);
            }
        }

        #endregion
    }
}
