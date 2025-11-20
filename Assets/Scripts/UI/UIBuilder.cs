using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BarSimulator.Systems;

namespace BarSimulator.UI
{
    /// <summary>
    /// UI 建構器 - 程式化生成遊戲 UI
    /// </summary>
    public class UIBuilder : MonoBehaviour
    {
        #region 單例

        private static UIBuilder instance;
        public static UIBuilder Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("UI 設定")]
        [SerializeField] private bool buildOnStart = true;

        [Header("準心設定")]
        [SerializeField] private float crosshairSize = 20f;
        [SerializeField] private Color crosshairColor = Color.white;
        [SerializeField] private Color crosshairInteractColor = Color.yellow;

        [Header("提示文字設定")]
        [SerializeField] private int hintFontSize = 18;
        [SerializeField] private Color hintTextColor = Color.white;

        #endregion

        #region 私有欄位

        private Canvas mainCanvas;
        private Image crosshairImage;
        private TextMeshProUGUI interactionHintText;
        private TextMeshProUGUI drinkInfoText;
        private CanvasGroup hintCanvasGroup;

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
            if (buildOnStart)
            {
                BuildUI();
            }
        }

        private void Update()
        {
            UpdateUI();
        }

        #endregion

        #region UI 建構

        /// <summary>
        /// 建構完整 UI
        /// </summary>
        public void BuildUI()
        {
            Debug.Log("UIBuilder: Building UI...");

            // 建立主 Canvas
            CreateMainCanvas();

            // 建立準心
            CreateCrosshair();

            // 建立互動提示
            CreateInteractionHint();

            // 建立飲料資訊顯示
            CreateDrinkInfo();

            // 建立控制說明
            CreateControlsHelp();

            Debug.Log("UIBuilder: UI built successfully");
        }

        /// <summary>
        /// 建立主 Canvas
        /// </summary>
        private void CreateMainCanvas()
        {
            // 檢查是否已有 Canvas
            mainCanvas = FindAnyObjectByType<Canvas>();
            if (mainCanvas != null) return;

            var canvasObj = new GameObject("MainCanvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 100;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 建立準心
        /// </summary>
        private void CreateCrosshair()
        {
            var crosshairObj = new GameObject("Crosshair");
            crosshairObj.transform.SetParent(mainCanvas.transform, false);

            var rectTransform = crosshairObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(crosshairSize, crosshairSize);

            crosshairImage = crosshairObj.AddComponent<Image>();
            crosshairImage.color = crosshairColor;

            // 建立準心形狀 (十字)
            var texture = CreateCrosshairTexture();
            crosshairImage.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        /// <summary>
        /// 建立準心紋理
        /// </summary>
        private Texture2D CreateCrosshairTexture()
        {
            int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            // 清除為透明
            var clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            // 畫十字
            int center = size / 2;
            int thickness = 2;
            int length = 8;

            // 水平線
            for (int x = center - length; x <= center + length; x++)
            {
                for (int t = -thickness / 2; t <= thickness / 2; t++)
                {
                    if (x >= 0 && x < size)
                        texture.SetPixel(x, center + t, Color.white);
                }
            }

            // 垂直線
            for (int y = center - length; y <= center + length; y++)
            {
                for (int t = -thickness / 2; t <= thickness / 2; t++)
                {
                    if (y >= 0 && y < size)
                        texture.SetPixel(center + t, y, Color.white);
                }
            }

            // 中心空白
            for (int x = center - 2; x <= center + 2; x++)
            {
                for (int y = center - 2; y <= center + 2; y++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            texture.Apply();
            return texture;
        }

        /// <summary>
        /// 建立互動提示
        /// </summary>
        private void CreateInteractionHint()
        {
            // 容器
            var hintContainer = new GameObject("InteractionHint");
            hintContainer.transform.SetParent(mainCanvas.transform, false);

            var containerRect = hintContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.3f);
            containerRect.anchorMax = new Vector2(0.5f, 0.3f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(600, 50);

            hintCanvasGroup = hintContainer.AddComponent<CanvasGroup>();
            hintCanvasGroup.alpha = 0f;

            // 背景
            var bgImage = hintContainer.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);

            // 文字
            var textObj = new GameObject("HintText");
            textObj.transform.SetParent(hintContainer.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);

            interactionHintText = textObj.AddComponent<TextMeshProUGUI>();
            interactionHintText.fontSize = hintFontSize;
            interactionHintText.color = hintTextColor;
            interactionHintText.alignment = TextAlignmentOptions.Center;
            interactionHintText.text = "";
        }

        /// <summary>
        /// 建立飲料資訊顯示
        /// </summary>
        private void CreateDrinkInfo()
        {
            var infoContainer = new GameObject("DrinkInfo");
            infoContainer.transform.SetParent(mainCanvas.transform, false);

            var containerRect = infoContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(0, 1);
            containerRect.pivot = new Vector2(0, 1);
            containerRect.anchoredPosition = new Vector2(20, -20);
            containerRect.sizeDelta = new Vector2(300, 150);

            var bgImage = infoContainer.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.5f);

            // 標題
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(infoContainer.transform, false);

            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -10);
            titleRect.sizeDelta = new Vector2(-20, 30);

            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.fontSize = 16;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.yellow;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.text = "Held Item";

            // 內容
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(infoContainer.transform, false);

            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(10, 10);
            contentRect.offsetMax = new Vector2(-10, -40);

            drinkInfoText = contentObj.AddComponent<TextMeshProUGUI>();
            drinkInfoText.fontSize = 14;
            drinkInfoText.color = Color.white;
            drinkInfoText.alignment = TextAlignmentOptions.TopLeft;
            drinkInfoText.text = "No item held";

            // 初始隱藏
            infoContainer.SetActive(false);
        }

        /// <summary>
        /// 建立控制說明
        /// </summary>
        private void CreateControlsHelp()
        {
            var helpContainer = new GameObject("ControlsHelp");
            helpContainer.transform.SetParent(mainCanvas.transform, false);

            var containerRect = helpContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(1, 0);
            containerRect.anchorMax = new Vector2(1, 0);
            containerRect.pivot = new Vector2(1, 0);
            containerRect.anchoredPosition = new Vector2(-20, 20);
            containerRect.sizeDelta = new Vector2(250, 180);

            var bgImage = helpContainer.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.5f);

            // 控制說明文字
            var textObj = new GameObject("HelpText");
            textObj.transform.SetParent(helpContainer.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            var helpText = textObj.AddComponent<TextMeshProUGUI>();
            helpText.fontSize = 12;
            helpText.color = Color.white;
            helpText.alignment = TextAlignmentOptions.TopLeft;
            helpText.text = @"<b>Controls:</b>
WASD - Move
Mouse - Look around
E - Pick up / Interact
R - Return item
LMB - Pour / Shake
RMB - Drink
ESC - Pause";
        }

        #endregion

        #region UI 更新

        /// <summary>
        /// 更新 UI 狀態
        /// </summary>
        private void UpdateUI()
        {
            UpdateInteractionHint();
            UpdateCrosshairColor();
        }

        /// <summary>
        /// 更新互動提示
        /// </summary>
        private void UpdateInteractionHint()
        {
            if (interactionHintText == null || hintCanvasGroup == null) return;

            var interactionSystem = InteractionSystem.Instance;
            if (interactionSystem == null) return;

            string hint = interactionSystem.GetInteractionHint();

            if (!string.IsNullOrEmpty(hint))
            {
                interactionHintText.text = hint;
                hintCanvasGroup.alpha = Mathf.Lerp(hintCanvasGroup.alpha, 1f, Time.deltaTime * 8f);
            }
            else
            {
                hintCanvasGroup.alpha = Mathf.Lerp(hintCanvasGroup.alpha, 0f, Time.deltaTime * 8f);
            }
        }

        /// <summary>
        /// 更新準心顏色
        /// </summary>
        private void UpdateCrosshairColor()
        {
            if (crosshairImage == null) return;

            var interactionSystem = InteractionSystem.Instance;
            if (interactionSystem == null) return;

            bool isInteractive = interactionSystem.TargetedObject != null || interactionSystem.IsHolding;
            Color targetColor = isInteractive ? crosshairInteractColor : crosshairColor;
            crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, Time.deltaTime * 10f);
        }

        #endregion

        #region 公開方法

        /// <summary>
        /// 顯示訊息
        /// </summary>
        public void ShowMessage(string message)
        {
            Debug.Log($"UI Message: {message}");
            // TODO: 實作訊息顯示
        }

        /// <summary>
        /// 設定飲料資訊
        /// </summary>
        public void SetDrinkInfo(string info)
        {
            if (drinkInfoText != null)
            {
                drinkInfoText.text = info;
            }
        }

        #endregion
    }
}
