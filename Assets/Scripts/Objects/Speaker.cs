using Unity;
using UnityEngine.UI;
using BarSimulator.Interaction;
using UnityEngine;
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
        private int currentTrackIndex = 0;

        #endregion

        public void Awake()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }   
        }

        public void TogglePlay()
        {
            if (audioSource != null && musicClips.Length > 0)
            {
                audioSource.clip = musicClips[currentTrackIndex];
                audioSource.volume = volume;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        public void SwitchMusic()
        {
            if (musicClips.Length == 0 || audioSource == null)
                return;

            currentTrackIndex = (currentTrackIndex + 1) % musicClips.Length;
            audioSource.clip = musicClips[currentTrackIndex];
            audioSource.Play();
        }

        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }
    }
}