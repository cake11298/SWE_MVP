using UnityEngine;
using UnityEngine.UI;
using BarSimulator.Core;

namespace BarSimulator.UI
{
    /// <summary>
    /// Displays game statistics: current money and game time
    /// Game time runs from 20:00:00 to 24:00:00 over 12 real minutes (3 real minutes = 1 game hour)
    /// </summary>
    public class GameStatsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text moneyText;
        [SerializeField] private Text timeText;

        [Header("Time Settings")]
        [SerializeField] private float realMinutesForFullDay = 0.5f; // 0.5 real minutes = 20:00 to 20:10 (10 minutes)
        
        // Game time settings
        private const int START_HOUR = 20;  // 20:00:00
        private const int START_MINUTE = 0; // 00
        private const int END_HOUR = 20;   // 20:10:00
        private const int END_MINUTE = 10; // 10
        private const int TOTAL_GAME_MINUTES = 10; // 10 minutes
        
        private float gameStartTime;
        private int currentMoney = 0;

        private void Start()
        {
            gameStartTime = Time.time;
            
            // Subscribe to GameManager events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCoinsUpdated += OnCoinsUpdated;
                
                // Initialize with current money
                var score = GameManager.Instance.GetScore();
                currentMoney = score.totalCoins;
            }
            
            UpdateUI();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCoinsUpdated -= OnCoinsUpdated;
            }
        }

        private void Update()
        {
            UpdateUI();
            CheckGameEnd();
        }

        /// <summary>
        /// Called when coins are updated in GameManager
        /// </summary>
        private void OnCoinsUpdated(int coinsEarned, int totalCoins)
        {
            currentMoney = totalCoins;
        }

        /// <summary>
        /// Update the UI display
        /// </summary>
        private void UpdateUI()
        {
            // Update money display
            if (moneyText != null)
            {
                moneyText.text = $"Money: ${currentMoney}";
            }

            // Update time display
            if (timeText != null)
            {
                string currentTime = GetCurrentGameTime();
                timeText.text = $"Time: {currentTime}";
            }
        }

        /// <summary>
        /// Calculate current game time based on elapsed real time
        /// </summary>
        private string GetCurrentGameTime()
        {
            float elapsedRealTime = Time.time - gameStartTime;
            
            // Convert real time to game time
            // realMinutesForFullDay real minutes = TOTAL_GAME_MINUTES game minutes
            float gameMinutesElapsed = (elapsedRealTime / 60f) * (TOTAL_GAME_MINUTES / realMinutesForFullDay);
            
            // Calculate current minute and second
            int totalMinutes = START_MINUTE + Mathf.FloorToInt(gameMinutesElapsed);
            
            // Clamp to end time
            if (totalMinutes >= END_MINUTE)
            {
                return $"{END_HOUR:D2}:{END_MINUTE:D2}:00";
            }
            
            // Calculate seconds
            float remainingMinutes = gameMinutesElapsed - Mathf.Floor(gameMinutesElapsed);
            int currentSecond = Mathf.FloorToInt(remainingMinutes * 60f);
            
            return $"{START_HOUR:D2}:{totalMinutes:D2}:{currentSecond:D2}";
        }

        /// <summary>
        /// Manually update money (for SimpleNPCServe integration)
        /// </summary>
        public void AddMoney(int amount)
        {
            currentMoney += amount;
        }

        /// <summary>
        /// Get current money
        /// </summary>
        public int GetCurrentMoney()
        {
            return currentMoney;
        }

        /// <summary>
        /// Check if game should end (reached 20:10:00)
        /// </summary>
        private void CheckGameEnd()
        {
            float elapsedRealTime = Time.time - gameStartTime;
            float gameMinutesElapsed = (elapsedRealTime / 60f) * (TOTAL_GAME_MINUTES / realMinutesForFullDay);
            int totalMinutes = START_MINUTE + Mathf.FloorToInt(gameMinutesElapsed);
            
            if (totalMinutes >= END_MINUTE)
            {
                // Game ended, trigger game over
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerGameEnd();
                }
            }
        }

        /// <summary>
        /// Get current game minute for external use
        /// </summary>
        public int GetCurrentGameMinute()
        {
            float elapsedRealTime = Time.time - gameStartTime;
            float gameMinutesElapsed = (elapsedRealTime / 60f) * (TOTAL_GAME_MINUTES / realMinutesForFullDay);
            return START_MINUTE + Mathf.FloorToInt(gameMinutesElapsed);
        }
    }
}
