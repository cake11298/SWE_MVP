using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace BarSimulator.UI
{
    /// <summary>
    /// Manages UI prompts that appear below the crosshair.
    /// Messages display for 3 seconds and automatically disappear.
    /// New messages override existing ones.
    /// </summary>
    public class UIPromptManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Text promptText;
        [SerializeField] private TextMeshProUGUI promptTextTMP;
        
        [Header("Settings")]
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private bool enableRichText = true;
        
        private Coroutine hideCoroutine;
        
        private static UIPromptManager instance;
        public static UIPromptManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<UIPromptManager>();
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            InitializePromptText();
        }
        
        private void InitializePromptText()
        {
            // Support both legacy Text and TextMeshProUGUI
            if (promptTextTMP != null)
            {
                promptTextTMP.color = textColor;
                promptTextTMP.richText = enableRichText;
                
                // Enable outline for better visibility
                promptTextTMP.outlineWidth = 0.3f;
                promptTextTMP.outlineColor = Color.black;
                
                // Set font size
                promptTextTMP.fontSize = 24;
                
                promptTextTMP.gameObject.SetActive(false);
            }
            else if (promptText != null)
            {
                promptText.color = textColor;
                promptText.supportRichText = enableRichText;
                
                // Set font size
                promptText.fontSize = 24;
                
                // Add Outline component for better visibility
                var outline = promptText.GetComponent<UnityEngine.UI.Outline>();
                if (outline == null)
                {
                    outline = promptText.gameObject.AddComponent<UnityEngine.UI.Outline>();
                }
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(2, -2);
                
                // Add Shadow for even better visibility
                var shadow = promptText.GetComponent<UnityEngine.UI.Shadow>();
                if (shadow == null)
                {
                    shadow = promptText.gameObject.AddComponent<UnityEngine.UI.Shadow>();
                }
                shadow.effectColor = new Color(0, 0, 0, 0.5f);
                shadow.effectDistance = new Vector2(1, -1);
                
                promptText.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("UIPromptManager: PromptText reference is missing!");
            }
        }
        
        /// <summary>
        /// Shows a prompt message below the crosshair for 3 seconds.
        /// If a message is already showing, it will be replaced immediately.
        /// </summary>
        /// <param name="message">The message to display</param>
        public void ShowPrompt(string message)
        {
            if (promptTextTMP == null && promptText == null)
            {
                Debug.LogWarning("UIPromptManager: Cannot show prompt - PromptText is null");
                return;
            }
            
            // Cancel any existing hide coroutine
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }
            
            // Update text and show (support both text types)
            if (promptTextTMP != null)
            {
                promptTextTMP.text = message;
                promptTextTMP.gameObject.SetActive(true);
            }
            else if (promptText != null)
            {
                promptText.text = message;
                promptText.gameObject.SetActive(true);
            }
            
            // Start new hide coroutine
            hideCoroutine = StartCoroutine(HideAfterDelay());
        }
        
        /// <summary>
        /// Immediately hides the prompt
        /// </summary>
        public void HidePrompt()
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
            
            if (promptTextTMP != null)
            {
                promptTextTMP.gameObject.SetActive(false);
            }
            else if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }
        
        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);
            
            if (promptTextTMP != null)
            {
                promptTextTMP.gameObject.SetActive(false);
            }
            else if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
            
            hideCoroutine = null;
        }
        
        /// <summary>
        /// Static method for easy access from anywhere in the code
        /// </summary>
        public static void Show(string message)
        {
            if (Instance != null)
            {
                Instance.ShowPrompt(message);
            }
            else
            {
                Debug.LogWarning("UIPromptManager: Instance not found. Cannot show prompt: " + message);
            }
        }
        
        /// <summary>
        /// Static method to hide the prompt
        /// </summary>
        public static void Hide()
        {
            if (Instance != null)
            {
                Instance.HidePrompt();
            }
        }
    }
}
