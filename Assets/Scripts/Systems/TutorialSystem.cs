using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace BarSimulator.Systems
{
    /// <summary>
    /// 教學系統 - 引導新玩家學習遊戲機制
    /// </summary>
    public class TutorialSystem : MonoBehaviour
    {
        #region Singleton

        private static TutorialSystem instance;
        public static TutorialSystem Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("UI References")]
        [Tooltip("教學面板")]
        [SerializeField] private GameObject tutorialPanel;

        [Tooltip("教學標題")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Tooltip("教學內容")]
        [SerializeField] private TextMeshProUGUI contentText;

        [Tooltip("步驟指示器（1/5）")]
        [SerializeField] private TextMeshProUGUI stepIndicatorText;

        [Tooltip("下一步按鈕")]
        [SerializeField] private Button nextButton;

        [Tooltip("上一步按鈕")]
        [SerializeField] private Button previousButton;

        [Tooltip("跳過教學按鈕")]
        [SerializeField] private Button skipButton;

        [Tooltip("完成教學按鈕")]
        [SerializeField] private Button finishButton;

        [Header("Settings")]
        [Tooltip("是否自動開始教學")]
        [SerializeField] private bool autoStartTutorial = true;

        [Tooltip("教學步驟列表")]
        [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();

        #endregion

        #region 私有欄位

        private int currentStepIndex = 0;
        private bool tutorialCompleted = false;
        private bool tutorialActive = false;

        // 事件
        public System.Action OnTutorialStart;
        public System.Action OnTutorialComplete;
        public System.Action OnTutorialSkip;
        public System.Action<int> OnStepChanged;

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

            SetupUI();
            InitializeDefaultSteps();
        }

        private void Start()
        {
            // Check if tutorial should start
            if (autoStartTutorial && !HasCompletedTutorial())
            {
                StartTutorial();
            }
            else
            {
                HideTutorialPanel();
            }
        }

        #endregion

        #region 初始化

        private void SetupUI()
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }

            // Setup buttons
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(NextStep);
            }

            if (previousButton != null)
            {
                previousButton.onClick.AddListener(PreviousStep);
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(SkipTutorial);
            }

            if (finishButton != null)
            {
                finishButton.onClick.AddListener(FinishTutorial);
            }
        }

        /// <summary>
        /// 初始化預設教學步驟
        /// </summary>
        private void InitializeDefaultSteps()
        {
            if (tutorialSteps.Count > 0) return;

            tutorialSteps = new List<TutorialStep>
            {
                new TutorialStep
                {
                    title = "Welcome to the Bar!",
                    content = "Welcome to the bartending simulator! This tutorial will teach you the basics of making cocktails.\n\nPress Next to continue."
                },
                new TutorialStep
                {
                    title = "Moving Around",
                    content = "Use WASD keys to move around the bar.\n\nUse your mouse to look around.\n\nPress E to interact with objects."
                },
                new TutorialStep
                {
                    title = "Picking Up Objects",
                    content = "Approach an object (like a glass or bottle) and press E to pick it up.\n\n" +
                              "While holding an object:\n" +
                              "- Press R to return it to its original position\n" +
                              "- Press Q to drop it in place with smart placement"
                },
                new TutorialStep
                {
                    title = "Making Drinks",
                    content = "To make a cocktail:\n\n" +
                              "1. Pick up a glass\n" +
                              "2. Pour liquors into the glass\n" +
                              "3. Use the shaker if needed (shake by moving mouse while holding)\n" +
                              "4. Serve the drink to the customer\n\n" +
                              "Press Tab to view the recipe book!"
                },
                new TutorialStep
                {
                    title = "Recipe Book & Shop",
                    content = "Press Tab to open the Recipe Book and view all cocktail recipes.\n\n" +
                              "Press B to open the Shop and upgrade your liquors or unlock new recipes.\n\n" +
                              "Higher quality liquors give better scores!"
                },
                new TutorialStep
                {
                    title = "Scoring System",
                    content = "Customers rate your drinks based on:\n" +
                              "- Correct ingredients\n" +
                              "- Proper proportions\n" +
                              "- Mixing method (shake, stir, build)\n\n" +
                              "Higher scores earn more money!"
                },
                new TutorialStep
                {
                    title = "Ready to Start!",
                    content = "You're ready to start bartending!\n\n" +
                              "Remember:\n" +
                              "- Tab: Recipe Book\n" +
                              "- B: Shop\n" +
                              "- E: Interact\n" +
                              "- R: Return to position\n" +
                              "- Q: Drop with smart placement\n\n" +
                              "Good luck!"
                }
            };
        }

        #endregion

        #region 教學控制

        /// <summary>
        /// 開始教學
        /// </summary>
        public void StartTutorial()
        {
            if (tutorialActive) return;

            tutorialActive = true;
            currentStepIndex = 0;

            ShowTutorialPanel();
            DisplayStep(currentStepIndex);

            OnTutorialStart?.Invoke();
            Debug.Log("TutorialSystem: Tutorial started");
        }

        /// <summary>
        /// 下一步
        /// </summary>
        public void NextStep()
        {
            if (currentStepIndex < tutorialSteps.Count - 1)
            {
                currentStepIndex++;
                DisplayStep(currentStepIndex);
                OnStepChanged?.Invoke(currentStepIndex);
            }
        }

        /// <summary>
        /// 上一步
        /// </summary>
        public void PreviousStep()
        {
            if (currentStepIndex > 0)
            {
                currentStepIndex--;
                DisplayStep(currentStepIndex);
                OnStepChanged?.Invoke(currentStepIndex);
            }
        }

        /// <summary>
        /// 跳過教學
        /// </summary>
        public void SkipTutorial()
        {
            Debug.Log("TutorialSystem: Tutorial skipped");

            tutorialActive = false;
            MarkTutorialCompleted();
            HideTutorialPanel();

            OnTutorialSkip?.Invoke();
        }

        /// <summary>
        /// 完成教學
        /// </summary>
        public void FinishTutorial()
        {
            Debug.Log("TutorialSystem: Tutorial completed");

            tutorialActive = false;
            MarkTutorialCompleted();
            HideTutorialPanel();

            OnTutorialComplete?.Invoke();
        }

        #endregion

        #region UI 顯示

        /// <summary>
        /// 顯示教學面板
        /// </summary>
        private void ShowTutorialPanel()
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(true);
            }

            // Don't pause game during tutorial - let player interact
            // Time.timeScale = 1f;

            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// 隱藏教學面板
        /// </summary>
        private void HideTutorialPanel()
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }

            // Restore cursor state
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// 顯示指定步驟
        /// </summary>
        private void DisplayStep(int index)
        {
            if (index < 0 || index >= tutorialSteps.Count) return;

            var step = tutorialSteps[index];

            // Update title
            if (titleText != null)
            {
                titleText.text = step.title;
            }

            // Update content
            if (contentText != null)
            {
                contentText.text = step.content;
            }

            // Update step indicator
            if (stepIndicatorText != null)
            {
                stepIndicatorText.text = $"{index + 1}/{tutorialSteps.Count}";
            }

            // Update button visibility
            if (previousButton != null)
            {
                previousButton.gameObject.SetActive(index > 0);
            }

            bool isLastStep = index == tutorialSteps.Count - 1;

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(!isLastStep);
            }

            if (finishButton != null)
            {
                finishButton.gameObject.SetActive(isLastStep);
            }
        }

        #endregion

        #region 進度追蹤

        /// <summary>
        /// 檢查是否已完成教學
        /// </summary>
        public bool HasCompletedTutorial()
        {
            return PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
        }

        /// <summary>
        /// 標記教學為已完成
        /// </summary>
        private void MarkTutorialCompleted()
        {
            tutorialCompleted = true;
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 重設教學進度（開發用）
        /// </summary>
        public void ResetTutorialProgress()
        {
            tutorialCompleted = false;
            PlayerPrefs.SetInt("TutorialCompleted", 0);
            PlayerPrefs.Save();
            Debug.Log("TutorialSystem: Tutorial progress reset");
        }

        #endregion

        #region 公開方法

        /// <summary>
        /// 顯示單一提示訊息
        /// </summary>
        public void ShowHint(string title, string content)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (contentText != null)
            {
                contentText.text = content;
            }

            ShowTutorialPanel();

            // Auto-hide after delay
            Invoke(nameof(HideTutorialPanel), 3f);
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 教學是否進行中
        /// </summary>
        public bool IsActive => tutorialActive;

        /// <summary>
        /// 當前步驟索引
        /// </summary>
        public int CurrentStepIndex => currentStepIndex;

        /// <summary>
        /// 總步驟數
        /// </summary>
        public int TotalSteps => tutorialSteps.Count;

        #endregion
    }

    /// <summary>
    /// 教學步驟資料
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        [Tooltip("步驟標題")]
        public string title;

        [Tooltip("步驟內容")]
        [TextArea(3, 10)]
        public string content;

        [Tooltip("高亮顯示的物件（可選）")]
        public GameObject highlightObject;

        [Tooltip("是否需要玩家完成特定動作才能繼續")]
        public bool requiresAction;

        [Tooltip("需要的動作類型")]
        public TutorialActionType requiredAction;
    }

    /// <summary>
    /// 教學動作類型
    /// </summary>
    public enum TutorialActionType
    {
        None,
        PickUpObject,
        PourLiquid,
        ShakeCocktail,
        ServeDrink,
        OpenRecipeBook,
        OpenShop
    }
}
