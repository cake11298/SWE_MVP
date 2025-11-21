using UnityEngine;
using BarSimulator.Systems;
using BarSimulator.Objects;
using BarSimulator.Interaction;

namespace BarSimulator.UI
{
    /// <summary>
    /// 遊戲中 HUD - 顯示倒酒進度和杯子內容
    /// 使用 OnGUI 即時顯示，不需要 Canvas 設置
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        #region 單例

        private static GameplayHUD instance;
        public static GameplayHUD Instance => instance;

        #endregion

        #region 私有欄位

        private InteractionSystem interactionSystem;
        private CocktailSystem cocktailSystem;

        // GUI 樣式
        private GUIStyle boxStyle;
        private GUIStyle labelStyle;
        private GUIStyle headerStyle;
        private bool stylesInitialized = false;

        #endregion

        #region Unity 生命週期

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
            interactionSystem = InteractionSystem.Instance;
            cocktailSystem = CocktailSystem.Instance;
        }

        private void OnGUI()
        {
            InitializeStyles();

            // 顯示互動提示
            DrawInteractionHint();

            // 顯示倒酒進度
            if (cocktailSystem != null && cocktailSystem.IsPouringActive)
            {
                DrawPouringProgress();
            }

            // 顯示手持物品資訊
            if (interactionSystem != null && interactionSystem.IsHolding)
            {
                DrawHeldItemInfo();
            }
        }

        #endregion

        #region GUI 初始化

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            // 背景框樣式
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTexture(2, 2, new Color(0f, 0f, 0f, 0.7f));
            boxStyle.padding = new RectOffset(10, 10, 10, 10);

            // 標籤樣式
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 14;
            labelStyle.normal.textColor = Color.white;
            labelStyle.alignment = TextAnchor.MiddleLeft;

            // 標題樣式
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.yellow;
            headerStyle.alignment = TextAnchor.MiddleCenter;

            stylesInitialized = true;
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        #endregion

        #region GUI 繪製

        /// <summary>
        /// 繪製互動提示
        /// </summary>
        private void DrawInteractionHint()
        {
            if (interactionSystem == null) return;

            string hint = interactionSystem.GetInteractionHint();
            if (string.IsNullOrEmpty(hint)) return;

            float width = 400;
            float height = 30;
            float x = (Screen.width - width) / 2;
            float y = Screen.height - 80;

            GUI.Box(new Rect(x - 5, y - 5, width + 10, height + 10), "", boxStyle);
            GUI.Label(new Rect(x, y, width, height), hint, labelStyle);
        }

        /// <summary>
        /// 繪製倒酒進度
        /// </summary>
        private void DrawPouringProgress()
        {
            float amount = cocktailSystem.CurrentPouringAmount;

            float width = 200;
            float height = 60;
            float x = Screen.width - width - 20;
            float y = 100;

            // 背景框
            GUI.Box(new Rect(x, y, width, height), "", boxStyle);

            // 標題
            GUI.Label(new Rect(x + 10, y + 5, width - 20, 25), "Pouring", headerStyle);

            // 倒酒量
            string amountText = $"{amount:F1} ml";
            labelStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(x + 10, y + 30, width - 20, 25), amountText, labelStyle);
            labelStyle.alignment = TextAnchor.MiddleLeft;

            // 進度條
            float barWidth = width - 20;
            float barHeight = 8;
            float barX = x + 10;
            float barY = y + height - 15;

            // 背景
            GUI.DrawTexture(new Rect(barX, barY, barWidth, barHeight),
                MakeTexture(1, 1, new Color(0.3f, 0.3f, 0.3f)));

            // 進度 (最大 300ml)
            float fillRatio = Mathf.Clamp01(amount / 300f);
            Color fillColor = fillRatio < 0.8f ? new Color(0.2f, 0.8f, 0.2f) : new Color(1f, 0.5f, 0f);
            GUI.DrawTexture(new Rect(barX, barY, barWidth * fillRatio, barHeight),
                MakeTexture(1, 1, fillColor));
        }

        /// <summary>
        /// 繪製手持物品資訊
        /// </summary>
        private void DrawHeldItemInfo()
        {
            var heldType = interactionSystem.GetHeldObjectType();
            if (heldType == null) return;

            // 只顯示杯子和搖酒器的內容
            if (heldType != InteractableType.Glass && heldType != InteractableType.Shaker) return;

            var container = interactionSystem.HeldObject as Container;
            if (container == null) return;

            float width = 220;
            float height = 120;
            float x = 20;
            float y = 100;

            // 背景框
            GUI.Box(new Rect(x, y, width, height), "", boxStyle);

            // 標題
            string title = heldType == InteractableType.Glass ? "Glass" : "Shaker";
            GUI.Label(new Rect(x + 10, y + 5, width - 20, 25), title, headerStyle);

            // 容量資訊
            float currentVolume = container.Volume;
            float maxVolume = container.MaxVolume;
            string volumeText = $"Volume: {currentVolume:F0}/{maxVolume:F0} ml";
            GUI.Label(new Rect(x + 10, y + 30, width - 20, 20), volumeText, labelStyle);

            // 填充比例條
            float barWidth = width - 20;
            float barHeight = 12;
            float barX = x + 10;
            float barY = y + 52;

            // 背景
            GUI.DrawTexture(new Rect(barX, barY, barWidth, barHeight),
                MakeTexture(1, 1, new Color(0.2f, 0.2f, 0.2f)));

            // 填充
            float fillRatio = container.FillRatio;
            Color liquidColor = container.IsEmpty ? Color.gray : container.MixedColor;
            GUI.DrawTexture(new Rect(barX, barY, barWidth * fillRatio, barHeight),
                MakeTexture(1, 1, liquidColor));

            // 成分列表
            if (!container.IsEmpty && container.Contents != null)
            {
                var ingredients = container.Contents.ingredients;
                if (ingredients != null && ingredients.Count > 0)
                {
                    string ingredientText = "Contains: ";
                    int count = 0;
                    foreach (var ing in ingredients)
                    {
                        if (count > 0) ingredientText += ", ";
                        ingredientText += $"{ing.displayName}";
                        count++;
                        if (count >= 3)
                        {
                            if (ingredients.Count > 3)
                                ingredientText += $" +{ingredients.Count - 3}";
                            break;
                        }
                    }

                    // 調整樣式以適應較小文字
                    var smallStyle = new GUIStyle(labelStyle);
                    smallStyle.fontSize = 12;
                    smallStyle.wordWrap = true;
                    GUI.Label(new Rect(x + 10, y + 70, width - 20, 40), ingredientText, smallStyle);
                }
            }
            else
            {
                var emptyStyle = new GUIStyle(labelStyle);
                emptyStyle.fontStyle = FontStyle.Italic;
                emptyStyle.normal.textColor = Color.gray;
                GUI.Label(new Rect(x + 10, y + 70, width - 20, 20), "Empty", emptyStyle);
            }
        }

        #endregion
    }
}
