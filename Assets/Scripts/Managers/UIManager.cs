using UnityEngine;
using BarSimulator.Systems;
using BarSimulator.Objects;

namespace BarSimulator.Managers
{
    /// <summary>
    /// UI Manager - Central management for all UI elements
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Singleton

        private static UIManager instance;
        public static UIManager Instance => instance;

        #endregion

        #region Serialized Fields

        [Header("UI References")]
        [Tooltip("HUD Controller")]
        [SerializeField] private UI.HUDController hudController;

        [Tooltip("Pour Progress UI")]
        [SerializeField] private UI.PourProgressUI pourProgressUI;

        [Tooltip("Container Info UI")]
        [SerializeField] private UI.ContainerInfoUI containerInfoUI;

        [Tooltip("Recipe Panel")]
        [SerializeField] private UI.RecipePanel recipePanel;

        [Header("Canvas")]
        [Tooltip("Main UI Canvas")]
        [SerializeField] private Canvas mainCanvas;

        #endregion

        #region Private Fields

        private InteractionSystem interactionSystem;
        private CocktailSystem cocktailSystem;

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
        }

        private void Start()
        {
            interactionSystem = InteractionSystem.Instance;
            cocktailSystem = CocktailSystem.Instance;

            // Subscribe to events
            if (cocktailSystem != null)
            {
                cocktailSystem.OnPourProgress += HandlePourProgress;
                cocktailSystem.OnPourComplete += HandlePourComplete;
                cocktailSystem.OnDrink += HandleDrink;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (cocktailSystem != null)
            {
                cocktailSystem.OnPourProgress -= HandlePourProgress;
                cocktailSystem.OnPourComplete -= HandlePourComplete;
                cocktailSystem.OnDrink -= HandleDrink;
            }
        }

        private void Update()
        {
            UpdateInteractionHint();
            UpdateContainerInfo();
        }

        #endregion

        #region UI Updates

        /// <summary>
        /// Update interaction hint display
        /// </summary>
        private void UpdateInteractionHint()
        {
            if (hudController == null || interactionSystem == null) return;

            string hint = interactionSystem.GetInteractionHint();
            hudController.SetInteractionHint(hint);
        }

        /// <summary>
        /// Update container info display
        /// </summary>
        private void UpdateContainerInfo()
        {
            if (containerInfoUI == null || interactionSystem == null) return;

            if (interactionSystem.IsHolding)
            {
                var heldObject = interactionSystem.HeldObject;

                // Check if holding a container (Glass or Shaker)
                if (heldObject is Container container)
                {
                    if (!container.IsEmpty)
                    {
                        containerInfoUI.ShowContainerInfo(container);
                        return;
                    }
                }
            }

            containerInfoUI.Hide();
        }

        #endregion

        #region Event Handlers

        private void HandlePourProgress(float amount)
        {
            if (pourProgressUI == null) return;

            // Get target container info
            if (interactionSystem != null)
            {
                var targetTransform = interactionSystem.FindNearbyContainer(
                    interactionSystem.HeldTransform, 2.5f);

                if (targetTransform != null)
                {
                    var container = targetTransform.GetComponent<Container>();
                    if (container != null)
                    {
                        pourProgressUI.UpdateProgress(container, amount);
                    }
                }
            }
        }

        private void HandlePourComplete(Container container, float totalAmount)
        {
            if (pourProgressUI != null)
            {
                pourProgressUI.OnPourComplete(totalAmount);
            }
        }

        private void HandleDrink(DrinkInfo drinkInfo)
        {
            if (hudController != null && drinkInfo != null)
            {
                hudController.ShowDrinkMessage(drinkInfo.cocktailName, drinkInfo.volume);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show recipe panel
        /// </summary>
        public void ShowRecipePanel()
        {
            recipePanel?.Show();
        }

        /// <summary>
        /// Hide recipe panel
        /// </summary>
        public void HideRecipePanel()
        {
            recipePanel?.Hide();
        }

        /// <summary>
        /// Toggle recipe panel
        /// </summary>
        public void ToggleRecipePanel()
        {
            if (recipePanel != null)
            {
                if (recipePanel.IsVisible)
                    recipePanel.Hide();
                else
                    recipePanel.Show();
            }
        }

        /// <summary>
        /// Show temporary message
        /// </summary>
        public void ShowMessage(string message, float duration = 3f)
        {
            hudController?.ShowMessage(message, duration);
        }

        #endregion
    }
}
