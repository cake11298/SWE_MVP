using UnityEngine;
using UnityEngine.UI;
using BarSimulator.Core;

namespace BarSimulator.UI
{
    /// <summary>
    /// Displays game statistics: current money and game time
    /// Game time runs from 09:00:00 to 24:00:00 over 45 real minutes (15 real minutes = 6 game hours)
    /// </summary>
    public class GameStatsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text moneyText;
        [SerializeField] private Text timeText;

        [Header("Time Settings")]
        [SerializeField] private float realMinutesForFullDay = 45f; // 45 real minutes = 09:00 to 24:00 (15 hours)
        
        // Game time settings
        private const int START_HOUR = 9;  // 09:00:00
        private const int END_HOUR = 24;   // 24:00:00
        private const int TOTAL_GAME_HOURS = END_HOUR - START_HOUR; // 15 hours
        
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
    }
}
