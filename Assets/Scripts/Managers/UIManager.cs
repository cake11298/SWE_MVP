using UnityEngine;
using BarSimulator.Systems;
using BarSimulator.Objects;

namespace BarSimulator.Managers
{
    /// <summary>
    /// UI Manager - Simplified version without old UI components
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Singleton

        private static UIManager instance;
        public static UIManager Instance => instance;

        #endregion

        #region Serialized Fields

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
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show temporary message (simplified - just logs to console)
        /// </summary>
        public void ShowMessage(string message, float duration = 3f)
        {
            Debug.Log($"[UI Message]: {message}");
        }

        #endregion
    }
}
