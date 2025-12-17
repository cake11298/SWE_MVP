using UnityEngine;

namespace BarSimulator.QTE
{
    /// <summary>
    /// Handles the visual shake animation for the Shaker when QTE is active
    /// </summary>
    public class ShakerShakeAnimation : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float shakeSpeed = 8f;
        [SerializeField] private float shakeAmplitude = 0.15f;
        
        [Header("Position Settings")]
        [SerializeField] private Vector3 centerScreenOffset = new Vector3(0f, 0f, 0.5f);
        [SerializeField] private float transitionSpeed = 10f;
        
        private Transform cameraTransform;
        private Vector3 originalLocalPosition;
        private Vector3 centerPosition;
        private bool isShaking = false;
        private float shakeTimer = 0f;
        
        private void Awake()
        {
            // Find the camera
            cameraTransform = Camera.main.transform;
        }
        
        /// <summary>
        /// Start the shake animation
        /// </summary>
        public void StartShake()
        {
            if (isShaking) return;
            
            isShaking = true;
            shakeTimer = 0f;
            
            // Save original position
            originalLocalPosition = transform.localPosition;
            
            // Calculate center screen position
            centerPosition = centerScreenOffset;
            
            Debug.Log("[ShakerShakeAnimation] Started shake animation");
        }
        
        /// <summary>
        /// Stop the shake animation
        /// </summary>
        public void StopShake()
        {
            if (!isShaking) return;
            
            isShaking = false;
            shakeTimer = 0f;
            
            Debug.Log("[ShakerShakeAnimation] Stopped shake animation");
        }
        
        private void Update()
        {
            if (isShaking)
            {
                UpdateShakeAnimation();
            }
            else
            {
                // Return to original position
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition, 
                    originalLocalPosition, 
                    Time.deltaTime * transitionSpeed
                );
            }
        }
        
        private void UpdateShakeAnimation()
        {
            shakeTimer += Time.deltaTime;
            
            // Calculate shake offset (up and down motion)
            float shakeOffset = Mathf.Sin(shakeTimer * shakeSpeed) * shakeAmplitude;
            
            // Target position is center + shake offset
            Vector3 targetPosition = centerPosition + new Vector3(0f, shakeOffset, 0f);
            
            // Smoothly move to target position
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, 
                targetPosition, 
                Time.deltaTime * transitionSpeed
            );
        }
        
        /// <summary>
        /// Check if currently shaking
        /// </summary>
        public bool IsShaking => isShaking;
    }
}
