using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Data;

namespace BarSimulator.Systems
{
    /// <summary>
    /// DbD-style skill check minigame for cocktail shaking.
    /// Rotating pointer with success/perfect hit zones.
    /// Uses EventBus for decoupled communication - does NOT know about CustomerSystem.
    /// </summary>
    public class MinigameSystem : MonoBehaviour, IMinigame
    {
        // ===================================================================
        // IMinigame Interface Implementation
        // ===================================================================

        public bool IsActive { get; private set; }
        public bool IsCompleted { get; private set; }
        public int CurrentScore { get; private set; }

        // ===================================================================
        // QTE State
        // ===================================================================

        private float pointerAngle;          // Current pointer rotation (0-360)
        private float hitZoneAngle;          // Center of the success zone
        private float perfectZoneAngle;      // Center of the perfect zone

        private int successfulHits;          // Number of successful hits
        private int comboCount;              // Current combo multiplier
        private float elapsedTime;           // Time since minigame started
        private float totalDuration;         // Maximum time allowed
        private int difficulty;              // Difficulty level (affects speed)

        private bool waitingForInput;        // Whether pointer is in hit window

        // ===================================================================
        // Configuration (from GameData)
        // ===================================================================

        private float pointerSpeed => GameData.QTEPointerSpeed * (1f + (difficulty * 0.2f));
        private float hitZoneSize => GameData.QTEHitZoneSize;
        private float perfectZoneSize => GameData.QTEPerfectZoneSize;
        private int requiredHits => GameData.QTERequiredHits;

        // ===================================================================
        // IMinigame Implementation
        // ===================================================================

        public void Initialize(float duration, int difficultyLevel)
        {
            totalDuration = duration;
            difficulty = Mathf.Clamp(difficultyLevel, 1, 5);

            // Reset state
            pointerAngle = 0f;
            successfulHits = 0;
            comboCount = 0;
            CurrentScore = 0;
            elapsedTime = 0f;
            IsActive = false;
            IsCompleted = false;
            waitingForInput = false;

            RandomizeHitZones();

            if (GameData.DebugQTE)
            {
                Debug.Log($"[MinigameSystem] Initialized - Duration: {duration}s, Difficulty: {difficulty}");
            }
        }

        public void StartMinigame()
        {
            IsActive = true;
            IsCompleted = false;
            elapsedTime = 0f;

            // Publish event
            EventBus.Publish(new MinigameStartedEvent("ShakingQTE", totalDuration));

            if (GameData.DebugQTE)
            {
                Debug.Log("[MinigameSystem] Minigame started!");
            }
        }

        public void UpdateMinigame(float deltaTime)
        {
            if (!IsActive || IsCompleted)
                return;

            elapsedTime += deltaTime;

            // Check timeout
            if (elapsedTime >= totalDuration)
            {
                FailMinigame("Timeout");
                return;
            }

            // Rotate pointer
            pointerAngle += pointerSpeed * deltaTime;
            if (pointerAngle >= 360f)
            {
                pointerAngle -= 360f;
            }

            // Check if pointer is in hit zone
            CheckHitZone();
        }

        public void HandleInput()
        {
            if (!IsActive || IsCompleted)
                return;

            // Check if player pressed input during hit window
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ProcessHit();
            }
        }

        public void StopMinigame()
        {
            IsActive = false;
            IsCompleted = true;

            if (GameData.DebugQTE)
            {
                Debug.Log($"[MinigameSystem] Minigame stopped. Final score: {CurrentScore}");
            }
        }

        public bool GetResult()
        {
            return successfulHits >= requiredHits;
        }

        // ===================================================================
        // QTE Logic
        // ===================================================================

        private void CheckHitZone()
        {
            // Check if pointer is within hit zone range
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(pointerAngle, hitZoneAngle));

            if (angleDifference <= hitZoneSize / 2f)
            {
                waitingForInput = true;
            }
            else if (waitingForInput)
            {
                // Exited hit zone without pressing - MISS
                waitingForInput = false;
                ProcessMiss();
            }
        }

        private void ProcessHit()
        {
            if (!waitingForInput)
            {
                // Pressed outside hit zone - MISS
                ProcessMiss();
                return;
            }

            // Check if it's a perfect hit
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(pointerAngle, perfectZoneAngle));
            bool isPerfect = angleDifference <= perfectZoneSize / 2f;

            // Calculate score
            int hitScore = isPerfect ? GameData.QTEPerfectBonus : GameData.QTESuccessScore;
            hitScore *= (1 + comboCount); // Combo multiplier
            CurrentScore += hitScore;

            successfulHits++;
            comboCount++;
            waitingForInput = false;

            // Publish hit event
            EventBus.Publish(new QTEHitEvent(isPerfect, hitScore, comboCount));

            if (GameData.DebugQTE)
            {
                Debug.Log($"[MinigameSystem] HIT! Perfect: {isPerfect}, Score: +{hitScore}, Combo: x{comboCount}");
            }

            // Check if minigame is complete
            if (successfulHits >= requiredHits)
            {
                CompleteMinigame();
            }
            else
            {
                // Randomize next hit zone
                RandomizeHitZones();
            }
        }

        private void ProcessMiss()
        {
            // Reset combo
            int previousCombo = comboCount;
            comboCount = 0;

            // Publish miss event
            EventBus.Publish(new QTEMissEvent(previousCombo));

            if (GameData.DebugQTE)
            {
                Debug.Log($"[MinigameSystem] MISS! Combo reset from x{previousCombo}");
            }

            // Randomize hit zone
            RandomizeHitZones();
        }

        private void CompleteMinigame()
        {
            IsCompleted = true;
            IsActive = false;

            // Calculate accuracy
            float accuracy = Mathf.Clamp01((float)successfulHits / requiredHits);

            // Publish completion event
            EventBus.Publish(new MinigameCompletedEvent(
                success: true,
                totalScore: CurrentScore,
                accuracy: accuracy
            ));

            if (GameData.DebugQTE)
            {
                Debug.Log($"[MinigameSystem] SUCCESS! Total Score: {CurrentScore}, Accuracy: {accuracy:P0}");
            }
        }

        private void FailMinigame(string reason)
        {
            IsCompleted = true;
            IsActive = false;

            // Publish failure event
            EventBus.Publish(new MinigameCompletedEvent(
                success: false,
                totalScore: CurrentScore,
                accuracy: 0f
            ));

            if (GameData.DebugQTE)
            {
                Debug.Log($"[MinigameSystem] FAILED! Reason: {reason}");
            }
        }

        private void RandomizeHitZones()
        {
            // Randomize hit zone position (60-300 degrees to avoid spawn position)
            hitZoneAngle = Random.Range(60f, 300f);

            // Perfect zone is at the center of hit zone
            perfectZoneAngle = hitZoneAngle;
        }

        // ===================================================================
        // Debug Visualization (OnGUI)
        // ===================================================================

        private void OnGUI()
        {
            if (!GameData.DebugQTE || !IsActive)
                return;

            // Draw debug overlay
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Box("=== SHAKING QTE DEBUG ===");
            GUILayout.Label($"Pointer Angle: {pointerAngle:F1}°");
            GUILayout.Label($"Hit Zone: {hitZoneAngle:F1}°");
            GUILayout.Label($"Successful Hits: {successfulHits}/{requiredHits}");
            GUILayout.Label($"Combo: x{comboCount}");
            GUILayout.Label($"Score: {CurrentScore}");
            GUILayout.Label($"Time: {elapsedTime:F1}s / {totalDuration:F1}s");
            GUILayout.Label($"Waiting for Input: {waitingForInput}");
            GUILayout.EndArea();
        }

        // ===================================================================
        // Unity Lifecycle
        // ===================================================================

        private void Update()
        {
            if (IsActive && !IsCompleted)
            {
                UpdateMinigame(Time.deltaTime);
                HandleInput();
            }
        }
    }
}
