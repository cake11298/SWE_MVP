namespace BarSimulator.Systems
{
    /// <summary>
    /// Interface for all minigame implementations.
    /// Allows for multiple minigame types (shaking, stirring, pouring, etc.)
    /// while maintaining a consistent API.
    /// </summary>
    public interface IMinigame
    {
        /// <summary>
        /// Whether the minigame is currently active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Whether the minigame has been completed (success or failure).
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Current score/progress of the minigame.
        /// </summary>
        int CurrentScore { get; }

        /// <summary>
        /// Initialize the minigame with parameters.
        /// </summary>
        /// <param name="duration">Total duration of the minigame (seconds)</param>
        /// <param name="difficulty">Difficulty multiplier (1-5)</param>
        void Initialize(float duration, int difficulty);

        /// <summary>
        /// Start the minigame.
        /// </summary>
        void StartMinigame();

        /// <summary>
        /// Update the minigame logic (called each frame while active).
        /// </summary>
        /// <param name="deltaTime">Time since last frame</param>
        void UpdateMinigame(float deltaTime);

        /// <summary>
        /// Handle player input for the minigame.
        /// </summary>
        void HandleInput();

        /// <summary>
        /// Stop the minigame (cleanup).
        /// </summary>
        void StopMinigame();

        /// <summary>
        /// Get the success state of the completed minigame.
        /// </summary>
        /// <returns>True if player succeeded, false otherwise</returns>
        bool GetResult();
    }
}
