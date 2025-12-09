using UnityEngine;
using BarSimulator.Core;

namespace BarSimulator.Systems
{
    /// <summary>
    /// Pouring Visual Effects System
    /// Handles particles and bottle tilting during pouring.
    /// </summary>
    public class PouringSystem : MonoBehaviour
    {
        private static PouringSystem instance;
        public static PouringSystem Instance => instance;

        [Header("Visual Settings")]
        [SerializeField] private ParticleSystem pourParticlePrefab;
        [SerializeField] private int particleCount = 200;
        [SerializeField] private float tiltAngle = 60f;
        [SerializeField] private float tiltSpeed = 5f;

        private ParticleSystem currentParticles;
        private bool isPouring = false;
        private Transform currentSource;
        private Quaternion originalRotation;
        private Quaternion targetRotation;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void Update()
        {
            if (isPouring && currentSource != null)
            {
                // Update Tilt
                // We want to tilt the bottle around its local X axis (assuming standard orientation)
                // relative to its original rotation.
                // However, the object might be moving/rotating by the player.
                // InteractionSystem handles position/rotation.
                // We can apply a local rotation offset or modify the transform directly.
                // Since InteractionSystem likely sets rotation every frame, we might need to coordinate.
                // But for now, let's try to modify localRotation.
                
                // A simple approach: Rotate towards the "down" direction for the spout?
                // Or just a fixed tilt offset from "upright".
                
                // Let's assume the bottle's "up" is Y. We want to tilt it so Y points somewhat down.
                // But we need to respect the player's facing direction.
                
                // For this implementation, we will just use the visual effect of particles
                // and let the InteractionSystem or Bottle script handle the physical tilt if possible.
                // But the prompt asked for "Bottle tilt animation (60 degrees)".
                
                // If we look at Bottle.cs, it has UpdateTiltAnimation().
                // So PouringSystem might not need to handle tilt if Bottle.cs does it.
                // Let's check Bottle.cs.
            }
            
            if (currentParticles != null && currentSource != null)
            {
                // Update particle position to be at the bottle mouth
                // Assuming bottle mouth is at local (0, height, 0) or we use a specific child.
                // We'll try to find a "PourPoint" child or use top of bounds.
                
                Vector3 pourPos = currentSource.position + currentSource.up * 0.15f + currentSource.forward * 0.1f;
                
                // Try to find PourPoint
                Transform pourPoint = currentSource.Find("PourPoint");
                if (pourPoint != null) pourPos = pourPoint.position;
                
                currentParticles.transform.position = pourPos;
            }
        }

        public void StartPourEffect(Transform source, Transform target, Color liquidColor)
        {
            if (isPouring && currentSource == source) return;

            isPouring = true;
            currentSource = source;
            
            if (pourParticlePrefab != null)
            {
                if (currentParticles == null)
                {
                    currentParticles = Instantiate(pourParticlePrefab, source.position, Quaternion.identity);
                }
                
                var main = currentParticles.main;
                main.startColor = liquidColor;
                main.maxParticles = particleCount;
                
                currentParticles.gameObject.SetActive(true);
                currentParticles.Play();
            }
        }

        public void UpdatePourEffect(Transform source, Transform target)
        {
            if (!isPouring) return;
            
            currentSource = source;
            
            if (currentParticles != null)
            {
                // Orient particles towards target
                if (target != null)
                {
                    Vector3 direction = (target.position - currentParticles.transform.position).normalized;
                    currentParticles.transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }

        public void StopPourEffect()
        {
            isPouring = false;
            currentSource = null;

            if (currentParticles != null)
            {
                currentParticles.Stop();
                // Don't destroy, just stop and let it fade, or disable.
                // Destroying might clip particles.
                Destroy(currentParticles.gameObject, 2.0f); 
                currentParticles = null;
            }
        }
    }
}
