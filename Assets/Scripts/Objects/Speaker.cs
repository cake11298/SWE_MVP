using Unity;
using UnityEngine.UI;
using BarSimulator.Interaction;
using UnityEngine;
using BarSimulator.UI;
using System.Linq.Expressions;

namespace BarSimulator.Objects
{
    /// <summary>
    /// 揚聲器物件
    /// </summary>
    public class Speaker : MonoBehaviour
    {
        #region Components

        [Header("揚聲器設置")]
        [SerializeField]
        private AudioSource audioSource; // 揚聲器的音頻源
        [SerializeField]
        private AudioClip[] musicClips; // 可播放的音樂剪輯列表
        [SerializeField]
        private float volume = 0.5f; // 音量設置
        [SerializeField]
        private int currentTrackIndex = 0;
        static readonly string[] MusicNames = { "Jazz Shot", "Relax Whiskey", "Slience Call" };
        #endregion

        public void Start()
        {
            // 確保AudioSource組件存在
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // 播放第一首音樂
            if (musicClips.Length > 0)
            {
                audioSource.clip = musicClips[currentTrackIndex];
                audioSource.volume = volume;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        // 切換播放狀態
        public void TogglePlay()
        {
            if (audioSource.isPlaying)
            {
                Debug.Log("暫停音樂");
                UIPromptManager.Show("音樂已暫停");
                audioSource.Pause();
            }
            else
            {
                Debug.Log("播放音樂");
                UIPromptManager.Show("音樂已播放");
                audioSource.Play();
            }
        }

        public void SwitchMusic()
        {
            if (musicClips.Length == 0 || audioSource == null)
                return;
            
            Debug.Log("切換音樂");
            UIPromptManager.Show($"切換至音樂：{MusicNames[currentTrackIndex]}");
            currentTrackIndex = (currentTrackIndex + 1) % musicClips.Length;
            audioSource.clip = musicClips[currentTrackIndex];
            audioSource.Play();
        }

        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            Debug.Log($"設置音量至 {newVolume}");
            UIPromptManager.Show($"音量大小：{Mathf.RoundToInt(newVolume * 100)}%");
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }
    }
}