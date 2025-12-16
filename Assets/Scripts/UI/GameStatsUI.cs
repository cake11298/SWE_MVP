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
        [SerializeField] private float realMinutesForFullDay = 12f; // 12 real minutes = 20:00 to 24:00 (4 hours)
        
        // Game time settings
        private const int START_HOUR = 20;  // 20:00:00
        private const int END_HOUR = 24;   // 24:00:00
        private const int TOTAL_GAME_HOURS = END_HOUR - START_HOUR; // 4 hours
        
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
            // realMinutesForFullDay real minutes = TOTAL_GAME_HOURS game hours
            float gameHoursElapsed = (elapsedRealTime / 60f) * (TOTAL_GAME_HOURS / realMinutesForFullDay);
            
            // Calculate current game hour
            int currentHour = START_HOUR + Mathf.FloorToInt(gameHoursElapsed);
            
            // Clamp to end time
            if (currentHour >= END_HOUR)
            {
                return "24:00:00";
            }
            
            // Calculate minutes and seconds
            float remainingHours = gameHoursElapsed - Mathf.Floor(gameHoursElapsed);
            int currentMinute = Mathf.FloorToInt(remainingHours * 60f);
            float remainingMinutes = (remainingHours * 60f) - currentMinute;
            int currentSecond = Mathf.FloorToInt(remainingMinutes * 60f);
            
            return $"{currentHour:D2}:{currentMinute:D2}:{currentSecond:D2}";
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
        /// Check if game should end (reached 24:00:00)
        /// </summary>
        private void CheckGameEnd()
        {
            float elapsedRealTime = Time.time - gameStartTime;
            float gameHoursElapsed = (elapsedRealTime / 60f) * (TOTAL_GAME_HOURS / realMinutesForFullDay);
            int currentHour = START_HOUR + Mathf.FloorToInt(gameHoursElapsed);
            
            if (currentHour >= END_HOUR)
            {
                // Game ended, trigger game over
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerGameEnd();
                }
            }
        }

        /// <summary>
        /// Get current game hour for external use
        /// </summary>
        public int GetCurrentGameHour()
        {
            float elapsedRealTime = Time.time - gameStartTime;
            float gameHoursElapsed = (elapsedRealTime / 60f) * (TOTAL_GAME_HOURS / realMinutesForFullDay);
            return START_HOUR + Mathf.FloorToInt(gameHoursElapsed);
        }
    }
}
