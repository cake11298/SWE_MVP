using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Data;
using BarSimulator.Systems;

namespace BarSimulator.States
{
    /// <summary>
    /// Customer Entry state - new customer arrives and places order.
    /// Spawns customer, shows order, then transitions to Crafting.
    /// </summary>
    public class State_CustomerEntry : IGameState
    {
        private float entryTimer;
        private GameStateManager stateManager;
        private CustomerSystem customerSystem;
        private bool customerSpawned;

        // Parameterless constructor for generic constraint
        public State_CustomerEntry() { }

        public State_CustomerEntry(GameStateManager manager, CustomerSystem custSystem)
        {
            stateManager = manager;
            customerSystem = custSystem;
        }

        public void Enter()
        {
            entryTimer = 0f;
            customerSpawned = false;

            if (GameData.DebugStateTransitions)
            {
                Debug.Log("[State_CustomerEntry] Customer arriving...");
            }

            // Subscribe to customer arrived event
            EventBus.Subscribe<CustomerArrivedEvent>(OnCustomerArrived);

            // Spawn customer immediately
            SpawnCustomer();
        }

        public void Update()
        {
            entryTimer += Time.deltaTime;

            // Auto-transition to Crafting after entry duration
            if (customerSpawned && entryTimer >= GameData.CustomerEntryDuration)
            {
                stateManager.TransitionTo<State_Crafting>();
            }
        }

        public void Exit()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<CustomerArrivedEvent>(OnCustomerArrived);

            if (GameData.DebugStateTransitions)
            {
                Debug.Log("[State_CustomerEntry] Customer seated, ready for crafting.");
            }
        }

        private void SpawnCustomer()
        {
            if (customerSystem != null)
            {
                customerSystem.SpawnCustomer();
            }
            else
            {
                Debug.LogError("[State_CustomerEntry] CustomerSystem not found!");
            }
        }

        private void OnCustomerArrived(CustomerArrivedEvent evt)
        {
            customerSpawned = true;

            if (GameData.DebugStateTransitions)
            {
                Debug.Log($"[State_CustomerEntry] Customer #{evt.CustomerID} ordered {evt.OrderedRecipe.Name}");
            }

            // Could show dialogue/order UI here
        }
    }
}
