using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BarSimulator.Data;
using BarSimulator.Core;

namespace BarSimulator.UI
{
    /// <summary>
    /// Recipe Panel - Shows cocktail recipes
    /// Reference: index.js loadRecipesToUI() Line 346-370
    /// </summary>
    public class RecipePanel : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panel")]
        [Tooltip("Main panel game object")]
        [SerializeField] private GameObject panelObject;

        [Tooltip("Panel canvas group")]
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Content")]
        [Tooltip("Recipe content container")]
        [SerializeField] private Transform recipeContainer;

        [Tooltip("Recipe card prefab")]
        [SerializeField] private GameObject recipeCardPrefab;

        [Header("Data")]
        // NOTE: recipeDatabase removed - use static RecipeDatabase class instead

        [Header("Settings")]
        [Tooltip("Fade speed")]
        [SerializeField] private float fadeSpeed = 10f;

        #endregion

        #region Private Fields

        private bool isVisible;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (panelObject != null)
                panelObject.SetActive(false);

            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = 0f;
        }

        private void Start()
        {
            // NOTE: RecipeDatabase is now static - no initialization needed
            LoadRecipes();
        }

        private void Update()
        {
            UpdateVisibility();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show the recipe panel
        /// </summary>
        public void Show()
        {
            isVisible = true;
            if (panelObject != null)
                panelObject.SetActive(true);

            // Pause game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Pause();
            }
        }

        /// <summary>
        /// Hide the recipe panel
        /// </summary>
        public void Hide()
        {
            isVisible = false;

            // Resume game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Resume();
            }
        }

        /// <summary>
        /// Toggle panel visibility
        /// </summary>
        public void Toggle()
        {
            if (isVisible)
                Hide();
            else
                Show();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Is the panel visible
        /// </summary>
        public bool IsVisible => isVisible;

        #endregion

        #region Private Methods

        /// <summary>
        /// Load recipes into UI
        /// </summary>
        private void LoadRecipes()
        {
            if (recipeContainer == null) return;

            // Clear existing cards
            foreach (Transform child in recipeContainer)
            {
                Destroy(child.gameObject);
            }

            // Create recipe cards
            var recipes = RecipeDatabase.AllRecipes;
            if (recipes == null) return;

            foreach (var recipe in recipes)
            {
                CreateRecipeCard(recipe);
            }
        }

        /// <summary>
        /// Create a recipe card UI element
        /// </summary>
        private void CreateRecipeCard(RecipeData recipe)
        {
            if (recipe == null) return;

            GameObject card;

            if (recipeCardPrefab != null)
            {
                card = Instantiate(recipeCardPrefab, recipeContainer);
            }
            else
            {
                // Create simple card without prefab
                card = CreateDefaultRecipeCard();
            }

            // Find and set text components
            var nameText = card.transform.Find("RecipeName")?.GetComponent<TextMeshProUGUI>();
            var ingredientsText = card.transform.Find("Ingredients")?.GetComponent<TextMeshProUGUI>();
            var methodText = card.transform.Find("Method")?.GetComponent<TextMeshProUGUI>();

            if (nameText != null)
            {
                nameText.text = recipe.name;
            }

            if (ingredientsText != null)
            {
                string ingredientsList = "";
                foreach (var ing in recipe.ingredients)
                {
                    if (ingredientsList.Length > 0)
                        ingredientsList += "\n";
                    ingredientsList += $"- {ing.amount} {ing.name}";
                }
                ingredientsText.text = ingredientsList;
            }

            if (methodText != null)
            {
                methodText.text = recipe.method;
            }
        }

        /// <summary>
        /// Create a default recipe card (fallback without prefab)
        /// </summary>
        private GameObject CreateDefaultRecipeCard()
        {
            var card = new GameObject("RecipeCard");
            card.transform.SetParent(recipeContainer, false);

            // Add layout element
            var layoutElement = card.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 280f;
            layoutElement.preferredHeight = 200f;

            // Add background
            var image = card.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            // Add vertical layout
            var vertLayout = card.AddComponent<VerticalLayoutGroup>();
            vertLayout.padding = new RectOffset(10, 10, 10, 10);
            vertLayout.spacing = 5f;
            vertLayout.childControlHeight = false;
            vertLayout.childControlWidth = true;

            // Create name text
            CreateTextChild(card, "RecipeName", 18, FontStyles.Bold);
            CreateTextChild(card, "Ingredients", 12, FontStyles.Normal);
            CreateTextChild(card, "Method", 10, FontStyles.Italic);

            return card;
        }

        /// <summary>
        /// Create a text child object
        /// </summary>
        private void CreateTextChild(GameObject parent, string name, int fontSize, FontStyles style)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);

            var rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(260f, fontSize == 18 ? 30f : 60f);

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
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
                if (panelObject != null)
                    panelObject.SetActive(false);
            }
        }

        #endregion
    }
}
