using System.Collections.Generic;
using UnityEngine;

namespace BarSimulator.Managers
{
    /// <summary>
    /// 音效管理器 - 管理遊戲中所有音效和音樂
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region 單例

        private static AudioManager instance;
        public static AudioManager Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("音源")]
        [Tooltip("背景音樂音源")]
        [SerializeField] private AudioSource musicSource;

        [Tooltip("音效音源")]
        [SerializeField] private AudioSource sfxSource;

        [Tooltip("環境音音源")]
        [SerializeField] private AudioSource ambientSource;

        [Header("音量設定")]
        [Tooltip("主音量")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;

        [Tooltip("音樂音量")]
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.5f;

        [Tooltip("音效音量")]
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 0.7f;

        [Tooltip("環境音音量")]
        [Range(0f, 1f)]
        [SerializeField] private float ambientVolume = 0.3f;

        [Header("音效資源")]
        [Tooltip("倒酒音效")]
        [SerializeField] private AudioClip pourSound;

        [Tooltip("搖酒音效")]
        [SerializeField] private AudioClip shakeSound;

        [Tooltip("喝酒音效")]
        [SerializeField] private AudioClip drinkSound;

        [Tooltip("放下物品音效")]
        [SerializeField] private AudioClip placeSound;

        [Tooltip("拿起物品音效")]
        [SerializeField] private AudioClip pickupSound;

        [Tooltip("玻璃碰撞音效")]
        [SerializeField] private AudioClip glassClinkSound;

        [Tooltip("成功音效")]
        [SerializeField] private AudioClip successSound;

        [Tooltip("UI 點擊音效")]
        [SerializeField] private AudioClip uiClickSound;

        [Header("背景音樂")]
        [Tooltip("吧台背景音樂")]
        [SerializeField] private AudioClip[] barMusicTracks;

        [Tooltip("環境音")]
        [SerializeField] private AudioClip barAmbience;

        #endregion

        #region 私有欄位

        // 音樂狀態
        private int currentTrackIndex;
        private bool isMusicPlaying;
        private bool isMuted;

        // 循環音效
        private Dictionary<string, AudioSource> loopingSounds = new Dictionary<string, AudioSource>();

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            // 建立音源
            CreateAudioSources();
        }

        private void Start()
        {
            // 播放環境音
            PlayAmbience();

            // 自動播放背景音樂
            if (barMusicTracks != null && barMusicTracks.Length > 0)
            {
                PlayMusic(0);
            }
        }

        private void Update()
        {
            // 檢查音樂是否播放完畢，自動播放下一首
            if (isMusicPlaying && musicSource != null && !musicSource.isPlaying)
            {
                PlayNextTrack();
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 建立音源元件
        /// </summary>
        private void CreateAudioSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = false;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            if (ambientSource == null)
            {
                ambientSource = gameObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }

            UpdateVolumes();
        }

        #endregion

        #region 音效播放

        /// <summary>
        /// 播放一次性音效
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip == null || sfxSource == null || isMuted) return;

            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume * volumeMultiplier);
        }

        /// <summary>
        /// 播放倒酒音效
        /// </summary>
        public void PlayPourSound()
        {
            PlaySFX(pourSound);
        }

        /// <summary>
        /// 播放搖酒音效
        /// </summary>
        public void PlayShakeSound()
        {
            PlaySFX(shakeSound);
        }

        /// <summary>
        /// 播放喝酒音效
        /// </summary>
        public void PlayDrinkSound()
        {
            PlaySFX(drinkSound);
        }

        /// <summary>
        /// 播放放置物品音效
        /// </summary>
        public void PlayPlaceSound()
        {
            PlaySFX(placeSound);
        }

        /// <summary>
        /// 播放拾取物品音效
        /// </summary>
        public void PlayPickupSound()
        {
            PlaySFX(pickupSound);
        }

        /// <summary>
        /// 播放玻璃碰撞音效
        /// </summary>
        public void PlayGlassClinkSound()
        {
            PlaySFX(glassClinkSound);
        }

        /// <summary>
        /// 播放成功音效
        /// </summary>
        public void PlaySuccessSound()
        {
            PlaySFX(successSound);
        }

        /// <summary>
        /// 播放 UI 點擊音效
        /// </summary>
        public void PlayUIClick()
        {
            PlaySFX(uiClickSound, 0.5f);
        }

        /// <summary>
        /// 開始循環音效
        /// </summary>
        public void StartLoopingSound(string id, AudioClip clip, float volume = 1f)
        {
            if (clip == null || isMuted) return;

            // 停止已存在的同 ID 音效
            StopLoopingSound(id);

            // 建立新音源
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.volume = volume * sfxVolume * masterVolume;
            source.Play();

            loopingSounds[id] = source;
        }

        /// <summary>
        /// 停止循環音效
        /// </summary>
        public void StopLoopingSound(string id)
        {
            if (loopingSounds.TryGetValue(id, out var source))
            {
                if (source != null)
                {
                    source.Stop();
                    Destroy(source);
                }
                loopingSounds.Remove(id);
            }
        }

        /// <summary>
        /// 停止所有循環音效
        /// </summary>
        public void StopAllLoopingSounds()
        {
            foreach (var kvp in loopingSounds)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.Stop();
                    Destroy(kvp.Value);
                }
            }
            loopingSounds.Clear();
        }

        #endregion

        #region 音樂控制

        /// <summary>
        /// 播放指定索引的音樂
        /// </summary>
        public void PlayMusic(int trackIndex)
        {
            if (barMusicTracks == null || barMusicTracks.Length == 0) return;
            if (musicSource == null) return;

            currentTrackIndex = trackIndex % barMusicTracks.Length;
            var track = barMusicTracks[currentTrackIndex];

            if (track != null)
            {
                musicSource.clip = track;
                musicSource.volume = musicVolume * masterVolume;
                musicSource.Play();
                isMusicPlaying = true;

                Debug.Log($"AudioManager: Playing track {currentTrackIndex + 1}/{barMusicTracks.Length}");
            }
        }

        /// <summary>
        /// 播放下一首
        /// </summary>
        public void PlayNextTrack()
        {
            PlayMusic(currentTrackIndex + 1);
        }

        /// <summary>
        /// 播放上一首
        /// </summary>
        public void PlayPreviousTrack()
        {
            int prevIndex = currentTrackIndex - 1;
            if (prevIndex < 0) prevIndex = barMusicTracks.Length - 1;
            PlayMusic(prevIndex);
        }

        /// <summary>
        /// 暫停/繼續音樂
        /// </summary>
        public void ToggleMusic()
        {
            if (musicSource == null) return;

            if (musicSource.isPlaying)
            {
                musicSource.Pause();
                isMusicPlaying = false;
            }
            else
            {
                musicSource.UnPause();
                isMusicPlaying = true;
            }
        }

        /// <summary>
        /// 停止音樂
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
                isMusicPlaying = false;
            }
        }

        #endregion

        #region 環境音

        /// <summary>
        /// 播放環境音
        /// </summary>
        public void PlayAmbience()
        {
            if (ambientSource == null || barAmbience == null) return;

            ambientSource.clip = barAmbience;
            ambientSource.volume = ambientVolume * masterVolume;
            ambientSource.Play();
        }

        /// <summary>
        /// 停止環境音
        /// </summary>
        public void StopAmbience()
        {
            if (ambientSource != null)
            {
                ambientSource.Stop();
            }
        }

        #endregion

        #region 音量控制

        /// <summary>
        /// 設置主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        /// <summary>
        /// 設置音樂音量
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        /// <summary>
        /// 設置音效音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        /// <summary>
        /// 設置環境音音量
        /// </summary>
        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        /// <summary>
        /// 更新所有音量
        /// </summary>
        private void UpdateVolumes()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;

            if (ambientSource != null)
                ambientSource.volume = ambientVolume * masterVolume;

            // 更新循環音效音量
            foreach (var source in loopingSounds.Values)
            {
                if (source != null)
                    source.volume = sfxVolume * masterVolume;
            }
        }

        /// <summary>
        /// 切換靜音
        /// </summary>
        public void ToggleMute()
        {
            isMuted = !isMuted;

            if (musicSource != null)
                musicSource.mute = isMuted;

            if (sfxSource != null)
                sfxSource.mute = isMuted;

            if (ambientSource != null)
                ambientSource.mute = isMuted;

            Debug.Log($"AudioManager: Muted = {isMuted}");
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 主音量
        /// </summary>
        public float MasterVolume => masterVolume;

        /// <summary>
        /// 音樂音量
        /// </summary>
        public float MusicVolume => musicVolume;

        /// <summary>
        /// 音效音量
        /// </summary>
        public float SFXVolume => sfxVolume;

        /// <summary>
        /// 環境音音量
        /// </summary>
        public float AmbientVolume => ambientVolume;

        /// <summary>
        /// 是否靜音
        /// </summary>
        public bool IsMuted => isMuted;

        /// <summary>
        /// 音樂是否正在播放
        /// </summary>
        public bool IsMusicPlaying => isMusicPlaying;

        /// <summary>
        /// 當前音樂索引
        /// </summary>
        public int CurrentTrackIndex => currentTrackIndex;

        #endregion
    }
}
