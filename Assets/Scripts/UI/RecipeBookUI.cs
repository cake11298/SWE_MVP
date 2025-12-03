using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using BarSimulator.Data;
using System.Collections.Generic;

namespace BarSimulator.UI
{
    /// <summary>
    /// 配方書UI系統 - Tab鍵查看所有配方
    /// </summary>
    public class RecipeBookUI : MonoBehaviour
    {
        #region Singleton

        private static RecipeBookUI instance;
        public static RecipeBookUI Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("UI References")]
        [Tooltip("配方書面板")]
        [SerializeField] private GameObject recipeBookPanel;

        [Tooltip("配方列表容器")]
        [SerializeField] private Transform recipeListContainer;

        [Tooltip("配方項目預製物")]
        [SerializeField] private GameObject recipeItemPrefab;

        [Tooltip("配方詳細資訊面板")]
        [SerializeField] private GameObject recipeDetailPanel;

        [Tooltip("配方名稱文字")]
        [SerializeField] private TextMeshProUGUI recipeNameText;

        [Tooltip("配方成分列表")]
        [SerializeField] private TextMeshProUGUI ingredientsText;

        [Tooltip("調製方法文字")]
        [SerializeField] private TextMeshProUGUI methodText;

        [Tooltip("配方描述文字")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Tooltip("難度星級容器")]
        [SerializeField] private Transform difficultyStarsContainer;

        [Tooltip("鎖定圖示")]
        [SerializeField] private GameObject lockedIcon;

        [Header("Filter")]
        [Tooltip("全部按鈕")]
        [SerializeField] private Button allButton;

        [Tooltip("已解鎖按鈕")]
        [SerializeField] private Button unlockedButton;

        [Tooltip("已鎖定按鈕")]
        [SerializeField] private Button lockedButton;

        [Header("Data")]
        // NOTE: recipeDatabase removed - use static RecipeDatabase class instead

        [Header("Input")]
        [Tooltip("Input Actions Asset")]
        [SerializeField] private InputActionAsset inputActions;

        #endregion

        #region 私有欄位

        private bool isOpen = false;
        private DrinkRecipe selectedRecipe;
        private RecipeFilter currentFilter = RecipeFilter.All;

        private InputAction toggleAction;
        private bool useFallbackInput = false;

        private List<GameObject> recipeItems = new List<GameObject>();

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

            SetupInputActions();
            SetupUI();
        }

        private void OnEnable()
        {
            toggleAction?.Enable();
        }

        private void OnDisable()
        {
            toggleAction?.Disable();
        }

        private void Update()
        {
            // 檢查Tab鍵切換
            bool togglePressed = false;
            if (useFallbackInput)
            {
                togglePressed = Input.GetKeyDown(KeyCode.Tab);
            }
            else
            {
                togglePressed = toggleAction != null && toggleAction.WasPressedThisFrame();
            }

            if (togglePressed)
            {
                ToggleRecipeBook();
            }

            // ESC鍵關閉
            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseRecipeBook();
            }
        }

        #endregion

        #region 初始化

        private void SetupInputActions()
        {
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("PlayerInputActions");
            }

            if (inputActions == null)
            {
                Debug.LogWarning("RecipeBookUI: PlayerInputActions not found, using fallback input");
                useFallbackInput = true;
                return;
            }

            var uiMap = inputActions.FindActionMap("UI");
            if (uiMap != null)
            {
                toggleAction = uiMap.FindAction("ToggleRecipeBook");
                if (toggleAction == null)
                {
                    Debug.LogWarning("RecipeBookUI: ToggleRecipeBook action not found, using fallback input");
                    useFallbackInput = true;
                }
            }
            else
            {
                Debug.LogWarning("RecipeBookUI: UI action map not found, using fallback input");
                useFallbackInput = true;
            }
        }

        private void SetupUI()
        {
            // 初始化隱藏
            if (recipeBookPanel != null)
            {
                recipeBookPanel.SetActive(false);
            }

            if (recipeDetailPanel != null)
            {
                recipeDetailPanel.SetActive(false);
            }

            // 設置篩選按鈕
            if (allButton != null)
            {
                allButton.onClick.AddListener(() => SetFilter(RecipeFilter.All));
            }

            if (unlockedButton != null)
            {
                unlockedButton.onClick.AddListener(() => SetFilter(RecipeFilter.Unlocked));
            }

            if (lockedButton != null)
            {
                lockedButton.onClick.AddListener(() => SetFilter(RecipeFilter.Locked));
            }

            // NOTE: RecipeDatabase is now static - no initialization needed
        }

        #endregion

        #region 開關控制

        /// <summary>
        /// 切換配方書開關
        /// </summary>
        public void ToggleRecipeBook()
        {
            if (isOpen)
            {
                CloseRecipeBook();
            }
            else
            {
                OpenRecipeBook();
            }
        }

        /// <summary>
        /// 開啟配方書
        /// </summary>
        public void OpenRecipeBook()
        {
            if (recipeBookPanel == null) return;

            isOpen = true;
            recipeBookPanel.SetActive(true);
            RefreshRecipeList();

            // 暫停遊戲（可選）
            Time.timeScale = 0f;

            // 鎖定滑鼠指標
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("RecipeBookUI: Opened");
        }

        /// <summary>
        /// 關閉配方書
        /// </summary>
        public void CloseRecipeBook()
        {
            if (recipeBookPanel == null) return;

            isOpen = false;
            recipeBookPanel.SetActive(false);

            // 恢復遊戲
            Time.timeScale = 1f;

            // 恢復滑鼠鎖定
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("RecipeBookUI: Closed");
        }

        #endregion

        #region 配方列表

        /// <summary>
        /// 重新整理配方列表
        /// </summary>
        private void RefreshRecipeList()
        {
            // 清除現有項目
            foreach (var item in recipeItems)
            {
                Destroy(item);
            }
            recipeItems.Clear();

            var recipes = RecipeDatabase.AllRecipes;
            if (recipes == null) return;
            if (recipeListContainer == null || recipeItemPrefab == null) return;

            // 建立配方項目
            foreach (var recipe in recipes)
            {
                // 根據篩選條件判斷是否顯示
                if (!ShouldShowRecipe(recipe)) continue;

                GameObject item = Instantiate(recipeItemPrefab, recipeListContainer);
                recipeItems.Add(item);

                // 設置項目資訊
                SetupRecipeItem(item, recipe);
            }
        }

        /// <summary>
        /// 判斷配方是否應該顯示
        /// </summary>
        private bool ShouldShowRecipe(DrinkRecipe recipe)
        {
            switch (currentFilter)
            {
                case RecipeFilter.Unlocked:
                    return !recipe.IsLocked;
                case RecipeFilter.Locked:
                    return recipe.IsLocked;
                case RecipeFilter.All:
                default:
                    return true;
            }
        }

        /// <summary>
        /// 設置配方項目
        /// </summary>
        private void SetupRecipeItem(GameObject item, DrinkRecipe recipe)
        {
            // 設置名稱
            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = recipe.IsLocked ? "???" : recipe.Name;
            }

            // 設置難度星級
            var difficultyText = item.transform.Find("DifficultyText")?.GetComponent<TextMeshProUGUI>();
            if (difficultyText != null)
            {
                difficultyText.text = recipe.IsLocked ? "" : GetDifficultyStars(recipe.DifficultyLevel);
            }

            // 設置鎖定圖示
            var lockIcon = item.transform.Find("LockIcon")?.gameObject;
            if (lockIcon != null)
            {
                lockIcon.SetActive(recipe.IsLocked);
            }

            // 設置按鈕事件
            var button = item.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => ShowRecipeDetails(recipe));
            }
        }

        /// <summary>
        /// 取得難度星級字串
        /// </summary>
        private string GetDifficultyStars(int difficulty)
        {
            string stars = "";
            for (int i = 0; i < difficulty; i++)
            {
                stars += "★";
            }
            return stars;
        }

        #endregion

        #region 配方詳細資訊

        /// <summary>
        /// 顯示配方詳細資訊
        /// </summary>
        public void ShowRecipeDetails(DrinkRecipe recipe)
        {
            if (recipe == null || recipeDetailPanel == null) return;

            selectedRecipe = recipe;
            recipeDetailPanel.SetActive(true);

            // 鎖定配方只顯示名稱
            if (recipe.IsLocked)
            {
                if (recipeNameText != null)
                {
                    recipeNameText.text = "???";
                }

                if (ingredientsText != null)
                {
                    ingredientsText.text = "This recipe is locked.\nUnlock it in the shop!";
                }

                if (methodText != null)
                {
                    methodText.text = "";
                }

                if (descriptionText != null)
                {
                    descriptionText.text = "";
                }

                if (lockedIcon != null)
                {
                    lockedIcon.SetActive(true);
                }

                return;
            }

            // 顯示完整資訊
            if (lockedIcon != null)
            {
                lockedIcon.SetActive(false);
            }

            // 名稱
            if (recipeNameText != null)
            {
                recipeNameText.text = recipe.Name;
            }

            // 成分列表
            if (ingredientsText != null && recipe.Ingredients != null)
            {
                string ingredientsList = "Ingredients:\n";
                foreach (var ingredient in recipe.Ingredients)
                {
                    ingredientsList += $"- {ingredient}\n";
                }
                ingredientsText.text = ingredientsList;
            }

            // 調製方法
            if (methodText != null)
            {
                string method = recipe.RequiresShaking
                    ? $"Shake ingredients for {recipe.ShakeTime}s, then strain into {recipe.PreferredGlassType}."
                    : $"Build in {recipe.PreferredGlassType} and stir.";
                if (!string.IsNullOrEmpty(recipe.Garnish))
                    method += $" Garnish with {recipe.Garnish}.";
                methodText.text = $"Method:\n{method}";
            }

            // 描述
            if (descriptionText != null && !string.IsNullOrEmpty(recipe.Description))
            {
                descriptionText.text = recipe.Description;
            }

            // 難度星級
            if (difficultyStarsContainer != null)
            {
                // 清除現有星級
                foreach (Transform child in difficultyStarsContainer)
                {
                    Destroy(child.gameObject);
                }

                // TODO: 創建星級圖示
            }

            Debug.Log($"RecipeBookUI: Showing details for {recipe.Name}");
        }

        #endregion

        #region 篩選

        /// <summary>
        /// 設置篩選條件
        /// </summary>
        public void SetFilter(RecipeFilter filter)
        {
            currentFilter = filter;
            RefreshRecipeList();

            Debug.Log($"RecipeBookUI: Filter set to {filter}");
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 配方書是否開啟
        /// </summary>
        public bool IsOpen => isOpen;

        #endregion
    }

    /// <summary>
    /// 配方篩選條件
    /// </summary>
    public enum RecipeFilter
    {
        All,
        Unlocked,
        Locked
    }
}
