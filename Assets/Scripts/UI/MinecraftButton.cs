using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace BarSimulator.UI
{
    /// <summary>
    /// Minecraft 風格按鈕組件
    /// 提供方塊狀、像素化的視覺效果和互動反饋
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class MinecraftButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region 序列化欄位

        [Header("按鈕顏色")]
        [Tooltip("正常狀態顏色")]
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        [Tooltip("懸停狀態顏色")]
        [SerializeField] private Color hoverColor = new Color(0.35f, 0.35f, 0.35f, 0.95f);

        [Tooltip("按下狀態顏色")]
        [SerializeField] private Color pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);

        [Tooltip("禁用狀態顏色")]
        [SerializeField] private Color disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        [Header("邊框設定")]
        [Tooltip("邊框圖片（可選）")]
        [SerializeField] private Image borderImage;

        [Tooltip("邊框顏色")]
        [SerializeField] private Color borderColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        [Tooltip("懸停時邊框顏色")]
        [SerializeField] private Color borderHoverColor = new Color(1f, 1f, 0.6f, 1f);

        [Header("文字設定")]
        [Tooltip("按鈕文字")]
        [SerializeField] private TextMeshProUGUI buttonText;

        [Tooltip("正常文字顏色")]
        [SerializeField] private Color textColor = Color.white;

        [Tooltip("懸停文字顏色")]
        [SerializeField] private Color textHoverColor = new Color(1f, 1f, 0.6f, 1f);

        [Header("音效")]
        [Tooltip("懸停音效（可選）")]
        [SerializeField] private AudioClip hoverSound;

        [Tooltip("點擊音效（可選）")]
        [SerializeField] private AudioClip clickSound;

        [Header("動畫")]
        [Tooltip("懸停時縮放")]
        [SerializeField] private float hoverScale = 1.05f;

        [Tooltip("動畫速度")]
        [SerializeField] private float animationSpeed = 10f;

        #endregion

        #region 私有欄位

        private Button button;
        private Image backgroundImage;
        private Vector3 originalScale;
        private Vector3 targetScale;
        private Color currentColor;
        private Color currentBorderColor;
        private Color currentTextColor;
        private bool isHovering = false;
        private bool isPressed = false;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            button = GetComponent<Button>();
            backgroundImage = GetComponent<Image>();
            originalScale = transform.localScale;
            targetScale = originalScale;

            // 自動尋找子物件中的文字
            if (buttonText == null)
            {
                buttonText = GetComponentInChildren<TextMeshProUGUI>();
            }

            // 自動尋找邊框（如果有名為 "Border" 的子物件）
            if (borderImage == null)
            {
                var borderObj = transform.Find("Border");
                if (borderObj != null)
                {
                    borderImage = borderObj.GetComponent<Image>();
                }
            }

            // 初始化顏色
            currentColor = normalColor;
            currentBorderColor = borderColor;
            currentTextColor = textColor;
            ApplyColors();
        }

        private void Update()
        {
            // 平滑縮放動畫
            if (transform.localScale != targetScale)
            {
                transform.localScale = Vector3.Lerp(
                    transform.localScale,
                    targetScale,
                    Time.unscaledDeltaTime * animationSpeed
                );
            }

            // 平滑顏色過渡
            if (backgroundImage != null && backgroundImage.color != currentColor)
            {
                backgroundImage.color = Color.Lerp(
                    backgroundImage.color,
                    currentColor,
                    Time.unscaledDeltaTime * animationSpeed
                );
            }

            if (borderImage != null && borderImage.color != currentBorderColor)
            {
                borderImage.color = Color.Lerp(
                    borderImage.color,
                    currentBorderColor,
                    Time.unscaledDeltaTime * animationSpeed
                );
            }

            if (buttonText != null && buttonText.color != currentTextColor)
            {
                buttonText.color = Color.Lerp(
                    buttonText.color,
                    currentTextColor,
                    Time.unscaledDeltaTime * animationSpeed
                );
            }
        }

        #endregion

        #region 事件處理

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable) return;

            isHovering = true;
            UpdateVisuals();
            PlaySound(hoverSound);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            isPressed = false;
            UpdateVisuals();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable) return;

            isPressed = true;
            UpdateVisuals();
            PlaySound(clickSound);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            UpdateVisuals();
        }

        #endregion

        #region 視覺更新

        private void UpdateVisuals()
        {
            if (!button.interactable)
            {
                // 禁用狀態
                currentColor = disabledColor;
                currentBorderColor = borderColor;
                currentTextColor = textColor;
                targetScale = originalScale;
            }
            else if (isPressed)
            {
                // 按下狀態
                currentColor = pressedColor;
                currentBorderColor = borderHoverColor;
                currentTextColor = textHoverColor;
                targetScale = originalScale * 0.95f;
            }
            else if (isHovering)
            {
                // 懸停狀態
                currentColor = hoverColor;
                currentBorderColor = borderHoverColor;
                currentTextColor = textHoverColor;
                targetScale = originalScale * hoverScale;
            }
            else
            {
                // 正常狀態
                currentColor = normalColor;
                currentBorderColor = borderColor;
                currentTextColor = textColor;
                targetScale = originalScale;
            }

            ApplyColors();
        }

        private void ApplyColors()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = currentColor;
            }

            if (borderImage != null)
            {
                borderImage.color = currentBorderColor;
            }

            if (buttonText != null)
            {
                buttonText.color = currentTextColor;
            }
        }

        #endregion

        #region 音效

        private void PlaySound(AudioClip clip)
        {
            if (clip == null) return;

            // 使用 AudioManager 播放音效（如果存在）
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
            else
            {
                // 否則使用 2D 音效
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
        }

        #endregion

        #region 公開方法

        /// <summary>
        /// 設置按鈕文字
        /// </summary>
        public void SetText(string text)
        {
            if (buttonText != null)
            {
                buttonText.text = text;
            }
        }

        /// <summary>
        /// 設置按鈕是否可互動
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
            UpdateVisuals();
        }

        /// <summary>
        /// 設置自訂顏色主題
        /// </summary>
        public void SetColorTheme(Color normal, Color hover, Color pressed)
        {
            normalColor = normal;
            hoverColor = hover;
            pressedColor = pressed;
            UpdateVisuals();
        }

        #endregion
    }
}
