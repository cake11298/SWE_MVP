using UnityEngine;

namespace BarSimulator.Objects
{
    /// <summary>
    /// Liquid container for bottles and shakers.
    /// Handles liquid source for pouring operations.
    /// </summary>
    public class LiquidContainer : MonoBehaviour
    {
        [Header("Liquid Properties")]
        [Tooltip("Name of the liquid (e.g., Gin, Whiskey, Cointreau)")]
        public string liquidName = "Unknown";

        [Tooltip("Is this an infinite source (bottles = true, shaker = false)")]
        public bool isInfinite = true;

        [Tooltip("Current volume in ml")]
        public float currentVolume = 750f;

        [Tooltip("Maximum volume in ml (800ml for shaker, ignored for infinite bottles)")]
        public float maxVolume = 750f;

        [Header("Pouring Settings")]
        [Tooltip("Pour rate in ml per second")]
        public float pourRate = 3.5f;

        [Header("Animation Settings")]
        [Tooltip("Rotation axis for pouring animation (local space)")]
        public Vector3 pourRotationAxis = new Vector3(1f, 0f, 0f);

        [Tooltip("Target rotation angle when pouring (degrees)")]
        public float pourAngle = 60f;

        [Tooltip("Rotation speed for pouring animation")]
        public float rotationSpeed = 5f;

        // Private state
        private Quaternion originalRotation;
        private bool isPouring = false;

        private void Awake()
        {
            // Store original rotation
            originalRotation = transform.localRotation;
        }

        /// <summary>
        /// Attempt to pour liquid from this container.
        /// Returns the actual amount poured.
        /// </summary>
        public float Pour(float amount)
        {
            if (isInfinite)
            {
                // Infinite source, always pour the requested amount
                return amount;
            }
            else
            {
                // Limited source, pour what's available
                float actualAmount = Mathf.Min(amount, currentVolume);
                currentVolume -= actualAmount;
                currentVolume = Mathf.Max(0f, currentVolume); // Clamp to 0
                return actualAmount;
            }
        }

        /// <summary>
        /// Check if this container can pour.
        /// </summary>
        public bool CanPour()
        {
            return isInfinite || currentVolume > 0f;
        }

        /// <summary>
        /// Get the fill ratio (0-1).
        /// </summary>
        public float GetFillRatio()
        {
            if (isInfinite)
                return 1f;
            
            return currentVolume / maxVolume;
        }

        /// <summary>
        /// Start pouring animation.
        /// </summary>
        public void StartPouring()
        {
            isPouring = true;
        }

        /// <summary>
        /// Stop pouring animation.
        /// </summary>
        public void StopPouring()
        {
            isPouring = false;
        }

        /// <summary>
        /// Update pouring animation.
        /// </summary>
        private void Update()
        {
            if (isPouring)
            {
                // Rotate to pour angle
                Quaternion targetRotation = originalRotation * Quaternion.Euler(pourRotationAxis * pourAngle);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                // Return to original rotation
                transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, Time.deltaTime * rotationSpeed);
            }
        }

        /// <summary>
        /// Add liquid to this container (for shaker).
        /// </summary>
        public void AddLiquid(float amount)
        {
            if (!isInfinite)
            {
                currentVolume += amount;
                currentVolume = Mathf.Min(currentVolume, maxVolume);
            }
        }

        /// <summary>
        /// Reset the container rotation immediately.
        /// </summary>
        public void ResetRotation()
        {
            transform.localRotation = originalRotation;
            isPouring = false;
        }
    }
}
