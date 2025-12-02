using System;
using System.Collections.Generic;

namespace BarSimulator.Core
{
    /// <summary>
    /// Centralized event bus for decoupled system communication.
    /// Supports type-safe pub/sub pattern to eliminate direct dependencies.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> eventSubscriptions = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Subscribe to an event of type T.
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="handler">Callback action when event is published</param>
        public static void Subscribe<T>(Action<T> handler) where T : class
        {
            Type eventType = typeof(T);

            if (!eventSubscriptions.ContainsKey(eventType))
            {
                eventSubscriptions[eventType] = new List<Delegate>();
            }

            if (!eventSubscriptions[eventType].Contains(handler))
            {
                eventSubscriptions[eventType].Add(handler);
            }
        }

        /// <summary>
        /// Unsubscribe from an event of type T.
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="handler">Callback to remove</param>
        public static void Unsubscribe<T>(Action<T> handler) where T : class
        {
            Type eventType = typeof(T);

            if (eventSubscriptions.ContainsKey(eventType))
            {
                eventSubscriptions[eventType].Remove(handler);
            }
        }

        /// <summary>
        /// Publish an event of type T to all subscribers.
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="eventData">Event payload</param>
        public static void Publish<T>(T eventData) where T : class
        {
            Type eventType = typeof(T);

            if (eventSubscriptions.ContainsKey(eventType))
            {
                // Create a copy to avoid modification during iteration
                List<Delegate> handlers = new List<Delegate>(eventSubscriptions[eventType]);

                foreach (Delegate handler in handlers)
                {
                    try
                    {
                        (handler as Action<T>)?.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Error invoking event handler for {eventType.Name}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Clear all subscriptions (useful for scene transitions or testing).
        /// </summary>
        public static void ClearAll()
        {
            eventSubscriptions.Clear();
        }

        /// <summary>
        /// Clear all subscriptions for a specific event type.
        /// </summary>
        /// <typeparam name="T">Event type to clear</typeparam>
        public static void Clear<T>() where T : class
        {
            Type eventType = typeof(T);
            if (eventSubscriptions.ContainsKey(eventType))
            {
                eventSubscriptions.Remove(eventType);
            }
        }

        /// <summary>
        /// Get subscriber count for debugging.
        /// </summary>
        public static int GetSubscriberCount<T>() where T : class
        {
            Type eventType = typeof(T);
            return eventSubscriptions.ContainsKey(eventType) ? eventSubscriptions[eventType].Count : 0;
        }
    }
}
