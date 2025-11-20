using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace BarSimulator.UI
{
    /// <summary>
    /// Dialogue Box - Shows NPC dialogue
    /// Reference: NPCManager.js dialogue system
    /// </summary>
    public class DialogueBox : MonoBehaviour
    {
        #region Singleton

        private static DialogueBox instance;
        public static DialogueBox Instance => instance;

        #endregion

        #region Serialized Fields

        [Header("Panel")]
        [Tooltip("Dialogue panel game object")]
        [SerializeField] private GameObject dialoguePanel;

        [Tooltip("Panel canvas group")]
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Content")]
        [Tooltip("NPC name text")]
        [SerializeField] private TextMeshProUGUI npcNameText;

        [Tooltip("Dialogue text")]
        [SerializeField] private TextMeshProUGUI dialogueText;

        [Tooltip("Continue prompt text")]
        [SerializeField] private TextMeshProUGUI continuePromptText;

        [Header("Settings")]
        [Tooltip("Text typing speed (characters per second)")]
        [SerializeField] private float typingSpeed = 50f;

        [Tooltip("Fade speed")]
        [SerializeField] private float fadeSpeed = 10f;

        [Tooltip("Auto hide delay (seconds, 0 = manual)")]
        [SerializeField] private float autoHideDelay = 0f;

        #endregion

        #region Private Fields

        private bool isVisible;
        private bool isTyping;
        private string currentDialogue;
        private Coroutine typingCoroutine;

        // Events
        public System.Action OnDialogueComplete;
        public System.Action OnDialogueClosed;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

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
        /// Show dialogue
        /// </summary>
        public void ShowDialogue(string npcName, string dialogue)
        {
            if (npcNameText != null)
                npcNameText.text = npcName;

            currentDialogue = dialogue;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            isVisible = true;

            // Start typing animation
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeDialogue(dialogue));
        }

        /// <summary>
        /// Show dialogue without typing animation
        /// </summary>
        public void ShowDialogueInstant(string npcName, string dialogue)
        {
            if (npcNameText != null)
                npcNameText.text = npcName;

            if (dialogueText != null)
                dialogueText.text = dialogue;

            currentDialogue = dialogue;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            isVisible = true;
            isTyping = false;

            UpdateContinuePrompt();
        }

        /// <summary>
        /// Hide dialogue box
        /// </summary>
        public void Hide()
        {
            isVisible = false;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            isTyping = false;
            OnDialogueClosed?.Invoke();
        }

        /// <summary>
        /// Skip typing animation
        /// </summary>
        public void SkipTyping()
        {
            if (!isTyping) return;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            if (dialogueText != null)
                dialogueText.text = currentDialogue;

            isTyping = false;
            UpdateContinuePrompt();
            OnDialogueComplete?.Invoke();
        }

        /// <summary>
        /// Continue or skip dialogue
        /// </summary>
        public void Continue()
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else
            {
                Hide();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Is dialogue visible
        /// </summary>
        public bool IsVisible => isVisible;

        /// <summary>
        /// Is typing in progress
        /// </summary>
        public bool IsTyping => isTyping;

        #endregion

        #region Private Methods

        /// <summary>
        /// Type dialogue character by character
        /// </summary>
        private IEnumerator TypeDialogue(string dialogue)
        {
            isTyping = true;

            if (dialogueText != null)
                dialogueText.text = "";

            if (continuePromptText != null)
                continuePromptText.text = "Press E to skip...";

            foreach (char c in dialogue)
            {
                if (dialogueText != null)
                    dialogueText.text += c;

                yield return new WaitForSeconds(1f / typingSpeed);
            }

            isTyping = false;
            typingCoroutine = null;
            UpdateContinuePrompt();
            OnDialogueComplete?.Invoke();

            // Auto hide
            if (autoHideDelay > 0)
            {
                yield return new WaitForSeconds(autoHideDelay);
                Hide();
            }
        }

        /// <summary>
        /// Update continue prompt text
        /// </summary>
        private void UpdateContinuePrompt()
        {
            if (continuePromptText != null)
            {
                continuePromptText.text = "Press E to close";
            }
        }

        /// <summary>
        /// Update visibility with fade
        /// </summary>
        private void UpdateVisibility()
        {
            if (panelCanvasGroup == null) return;

            float targetAlpha = isVisible ? 1f : 0f;
            panelCanvasGroup.alpha = Mathf.Lerp(
                panelCanvasGroup.alpha,
                targetAlpha,
                fadeSpeed * Time.deltaTime
            );

            // Disable panel when fully hidden
            if (!isVisible && panelCanvasGroup.alpha < 0.01f)
            {
                if (dialoguePanel != null)
                    dialoguePanel.SetActive(false);
            }
        }

        #endregion
    }
}
