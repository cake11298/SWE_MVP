namespace BarSimulator.Core
{
    /// <summary>
    /// Interface for all game states in the Finite State Machine.
    /// Each state represents a distinct phase of gameplay (Idle, CustomerEntry, Crafting, QTE, Serving).
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// Called once when entering this state.
        /// Use for initialization, event subscriptions, and setup.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called every frame while in this state.
        /// Contains the state's main logic and update loop.
        /// </summary>
        void Update();

        /// <summary>
        /// Called once when exiting this state.
        /// Use for cleanup, event unsubscriptions, and teardown.
        /// </summary>
        void Exit();
    }
}
