using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BarSimulator.Objects;
using BarSimulator.Data;
using BarSimulator.Systems;

namespace BarSimulator.UI
{
    /// <summary>
    /// Container Info UI - Shows container contents and cocktail name
    /// Reference: CocktailSystem.js showContainerInfo() Line 1058-1096
    /// </summary>
    public class ContainerInfoUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panel")]
        [Tooltip("Main panel canvas group")]
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Content")]
        [Tooltip("Cocktail name text")]
        [SerializeField] private TextMeshProUGUI cocktailNameText;

        [Tooltip("Ingredients list text")]
        [SerializeField] private TextMeshProUGUI ingredientsText;

        [Tooltip("Volume info text")]
        [SerializeField] private TextMeshProUGUI volumeInfoText;

        [Header("Settings")]
        [Tooltip("Fade speed")]
        [SerializeField] private float fadeSpeed = 8f;

        #endregion

        #region Private Fields

        private bool isVisible;
        private CocktailSystem cocktailSystem;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = 0f;
        }

        private void Start()
        {
            cocktailSystem = CocktailSystem.Instance;
        }

        private void Update()
        {
            UpdateVisibility();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show container info
        /// </summary>
        public void ShowContainerInfo(Container container)
        {
            if (container == null || container.IsEmpty)
            {
                Hide();
                return;
            }

            isVisible = true;
            var contents = container.Contents;

            // Update cocktail name
            if (cocktailNameText != null && cocktailSystem != null)
            {
                string name = cocktailSystem.IdentifyCocktail(contents.ingredients.ToArray());
                cocktailNameText.text = name;
            }

            // Update ingredients list
            if (ingredientsText != null)
            {
                string ingredientsList = "";
                foreach (var ingredient in contents.ingredients)
                {
                    if (ingredientsList.Length > 0)
                        ingredientsList += "\n";

                    // Use display name (English)
                    string name = !string.IsNullOrEmpty(ingredient.displayName)
                        ? ingredient.displayName
                        : ingredient.name;

                    ingredientsList += $"- {name}: {ingredient.amount:F0}ml";
                }
                ingredientsText.text = ingredientsList;
            }

            // Update volume info
            if (volumeInfoText != null && cocktailSystem != null)
            {
                float alcoholContent = contents.CalculateAlcoholContent(cocktailSystem.LiquorDatabase);
                volumeInfoText.text = $"Total: {contents.volume:F0}/{contents.maxVolume:F0}ml\n" +
                                     $"Alcohol: {alcoholContent:F1}%";
            }
        }

        /// <summary>
        /// Hide the panel
        /// </summary>
        public void Hide()
        {
            isVisible = false;
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
        }

        #endregion
    }
}
