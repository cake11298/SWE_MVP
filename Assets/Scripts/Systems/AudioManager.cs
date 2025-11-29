using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using BarSimulator.UI;

namespace BarSimulator.Systems
{
    /// <summary>
    /// 音效管理系統 - 處理背景音樂和音效播放
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton

        private static AudioManager instance;
        public static AudioManager Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("Audio Mixer")]
        [Tooltip("主要Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("Music")]
        [Tooltip("音樂音源")]
        [SerializeField] private AudioSource musicSource;

        [Tooltip("主選單音樂")]
        [SerializeField] private AudioClip mainMenuMusic;

        [Tooltip("遊戲場景音樂")]
        [SerializeField] private AudioClip gameSceneMusic;

        [Tooltip("音樂淡入淡出時間")]
        [SerializeField] private float musicFadeTime = 1.5f;

        [Header("Ambient Sounds")]
        [Tooltip("環境音音源")]
        [SerializeField] private AudioSource ambientSource;

        [Tooltip("酒吧環境音（人群、玻璃碰撞等）")]
        [SerializeField] private AudioClip barAmbience;

        [Header("SFX Pool")]
        [Tooltip("音效音源池大小")]
        [SerializeField] private int sfxPoolSize = 10;

        #endregion

        #region 私有欄位

        // SFX音源池
        private List<AudioSource> sfxPool = new List<AudioSource>();
        private int nextSfxIndex = 0;

        // 音效字典（預載音效）
        private Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();

        // 音量控制
        private float masterVolume = 1.0f;
        private float musicVolume = 0.7f;
        private float sfxVolume = 1.0f;

        // 淡入淡出
        private bool isFadingMusic = false;
        private float musicFadeTimer = 0f;
        private float musicFadeStartVolume = 0f;
        private float musicFadeTargetVolume = 0f;

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

            InitializeAudioSources();
            InitializeSFXPool();
            LoadSFXLibrary();

            // Subscribe to settings changes
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnMasterVolumeChanged += SetMasterVolume;
                SettingsManager.Instance.OnMusicVolumeChanged += SetMusicVolume;
                SettingsManager.Instance.OnSFXVolumeChanged += SetSFXVolume;
            }
        }

        private void OnDestroy()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnMasterVolumeChanged -= SetMasterVolume;
                SettingsManager.Instance.OnMusicVolumeChanged -= SetMusicVolume;
                SettingsManager.Instance.OnSFXVolumeChanged -= SetSFXVolume;
            }
        }

        private void Update()
        {
            if (isFadingMusic)
            {
                UpdateMusicFade();
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化音源組件
        /// </summary>
        private void InitializeAudioSources()
        {
            // Music source
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
            }

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;

            if (audioMixer != null)
            {
                musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
            }

            // Ambient source
            if (ambientSource == null)
            {
                GameObject ambientObj = new GameObject("AmbientSource");
                ambientObj.transform.SetParent(transform);
                ambientSource = ambientObj.AddComponent<AudioSource>();
            }

            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
            ambientSource.volume = sfxVolume * 0.5f;

            if (audioMixer != null)
            {
                ambientSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
            }
        }

        /// <summary>
        /// 初始化音效音源池
        /// </summary>
        private void InitializeSFXPool()
        {
            GameObject poolContainer = new GameObject("SFXPool");
            poolContainer.transform.SetParent(transform);

            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject sfxObj = new GameObject($"SFX_{i}");
                sfxObj.transform.SetParent(poolContainer.transform);
                AudioSource source = sfxObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = sfxVolume;

                if (audioMixer != null)
                {
                    source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
                }

                sfxPool.Add(source);
            }
        }

        /// <summary>
        /// 載入音效庫（從 Resources 或預設）
        /// </summary>
        private void LoadSFXLibrary()
        {
            // Note: In actual implementation, load AudioClips from Resources folder
            // For now, this is a placeholder structure

            // Example of how to load:
            // sfxLibrary["pour"] = Resources.Load<AudioClip>("Audio/SFX/Pour");
            // sfxLibrary["shake"] = Resources.Load<AudioClip>("Audio/SFX/Shake");

            Debug.Log("AudioManager: SFX Library loaded");
        }

        #endregion

        #region 音樂控制

        /// <summary>
        /// 播放音樂
        /// </summary>
        public void PlayMusic(AudioClip clip, bool fadeIn = true)
        {
            if (clip == null) return;

            if (fadeIn && musicSource.isPlaying)
            {
                // Crossfade to new music
                FadeMusic(0f, musicFadeTime / 2f, () =>
                {
                    musicSource.clip = clip;
                    musicSource.Play();
                    FadeMusic(musicVolume, musicFadeTime / 2f);
                });
            }
            else
            {
                musicSource.clip = clip;
                musicSource.volume = fadeIn ? 0f : musicVolume;
                musicSource.Play();

                if (fadeIn)
                {
                    FadeMusic(musicVolume, musicFadeTime);
                }
            }

            Debug.Log($"AudioManager: Playing music {clip.name}");
        }

        /// <summary>
        /// 停止音樂
        /// </summary>
        public void StopMusic(bool fadeOut = true)
        {
            if (!musicSource.isPlaying) return;

            if (fadeOut)
            {
                FadeMusic(0f, musicFadeTime, () =>
                {
                    musicSource.Stop();
                });
            }
            else
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// 暫停音樂
        /// </summary>
        public void PauseMusic()
        {
            musicSource.Pause();
        }

        /// <summary>
        /// 繼續音樂
        /// </summary>
        public void ResumeMusic()
        {
            musicSource.UnPause();
        }

        /// <summary>
        /// 淡入淡出音樂
        /// </summary>
        private void FadeMusic(float targetVolume, float duration, System.Action onComplete = null)
        {
            isFadingMusic = true;
            musicFadeTimer = 0f;
            musicFadeStartVolume = musicSource.volume;
            musicFadeTargetVolume = targetVolume;

            if (onComplete != null)
            {
                StartCoroutine(FadeMusicCoroutine(duration, onComplete));
            }
        }

        /// <summary>
        /// 更新音樂淡入淡出
        /// </summary>
        private void UpdateMusicFade()
        {
            if (musicFadeTimer < musicFadeTime)
            {
                musicFadeTimer += Time.unscaledDeltaTime;
                float t = musicFadeTimer / musicFadeTime;
                musicSource.volume = Mathf.Lerp(musicFadeStartVolume, musicFadeTargetVolume, t);
            }
            else
            {
                musicSource.volume = musicFadeTargetVolume;
                isFadingMusic = false;
            }
        }

        /// <summary>
        /// 音樂淡入淡出協程
        /// </summary>
        private System.Collections.IEnumerator FadeMusicCoroutine(float duration, System.Action onComplete)
        {
            float timer = 0f;
            float startVolume = musicSource.volume;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / duration;
                musicSource.volume = Mathf.Lerp(startVolume, musicFadeTargetVolume, t);
                yield return null;
            }

            musicSource.volume = musicFadeTargetVolume;
            isFadingMusic = false;
            onComplete?.Invoke();
        }

        #endregion

        #region 環境音控制

        /// <summary>
        /// 播放環境音
        /// </summary>
        public void PlayAmbient(AudioClip clip)
        {
            if (clip == null) return;

            ambientSource.clip = clip;
            ambientSource.Play();

            Debug.Log($"AudioManager: Playing ambient {clip.name}");
        }

        /// <summary>
        /// 停止環境音
        /// </summary>
        public void StopAmbient()
        {
            ambientSource.Stop();
        }

        /// <summary>
        /// 播放酒吧環境音
        /// </summary>
        public void PlayBarAmbience()
        {
            if (barAmbience != null)
            {
                PlayAmbient(barAmbience);
            }
        }

        #endregion

        #region 音效播放

        /// <summary>
        /// 播放音效（使用音源池）
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSFXSource();
            source.clip = clip;
            source.volume = sfxVolume * volume;
            source.pitch = pitch;
            source.Play();
        }

        /// <summary>
        /// 播放音效（通過名稱從音效庫）
        /// </summary>
        public void PlaySFX(string sfxName, float volume = 1.0f, float pitch = 1.0f)
        {
            if (sfxLibrary.ContainsKey(sfxName))
            {
                PlaySFX(sfxLibrary[sfxName], volume, pitch);
            }
            else
            {
                Debug.LogWarning($"AudioManager: SFX '{sfxName}' not found in library");
            }
        }

        /// <summary>
        /// 播放音效（3D位置）
        /// </summary>
        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1.0f)
        {
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, sfxVolume * volume);
        }

        /// <summary>
        /// 取得可用的音效音源
        /// </summary>
        private AudioSource GetAvailableSFXSource()
        {
            // 找未在播放的音源
            for (int i = 0; i < sfxPool.Count; i++)
            {
                int index = (nextSfxIndex + i) % sfxPool.Count;
                if (!sfxPool[index].isPlaying)
                {
                    nextSfxIndex = (index + 1) % sfxPool.Count;
                    return sfxPool[index];
                }
            }

            // 如果都在播放，使用下一個（會打斷）
            AudioSource source = sfxPool[nextSfxIndex];
            nextSfxIndex = (nextSfxIndex + 1) % sfxPool.Count;
            return source;
        }

        #endregion

        #region 常用遊戲音效

        /// <summary>
        /// 播放倒酒音效
        /// </summary>
        public void PlayPourSound()
        {
            PlaySFX("pour", 0.8f, Random.Range(0.95f, 1.05f));
        }

        /// <summary>
        /// 播放搖酒音效
        /// </summary>
        public void PlayShakeSound()
        {
            PlaySFX("shake", 1.0f, Random.Range(0.98f, 1.02f));
        }

        /// <summary>
        /// 播放攪拌音效
        /// </summary>
        public void PlayStirSound()
        {
            PlaySFX("stir", 0.7f, Random.Range(0.97f, 1.03f));
        }

        /// <summary>
        /// 播放玻璃碰撞音效
        /// </summary>
        public void PlayGlassCollisionSound(float intensity = 1.0f)
        {
            PlaySFX("glass_collision", Mathf.Clamp01(intensity), Random.Range(0.9f, 1.1f));
        }

        /// <summary>
        /// 播放玻璃破碎音效
        /// </summary>
        public void PlayGlassBreakSound()
        {
            PlaySFX("glass_break", 1.0f, Random.Range(0.95f, 1.05f));
        }

        /// <summary>
        /// 播放冰塊音效
        /// </summary>
        public void PlayIceSound()
        {
            PlaySFX("ice", 0.8f, Random.Range(1.0f, 1.2f));
        }

        /// <summary>
        /// 播放開瓶音效
        /// </summary>
        public void PlayBottleOpenSound()
        {
            PlaySFX("bottle_open", 0.9f);
        }

        /// <summary>
        /// 播放放下物件音效
        /// </summary>
        public void PlayPlaceObjectSound()
        {
            PlaySFX("place_object", 0.6f, Random.Range(0.9f, 1.1f));
        }

        /// <summary>
        /// 播放拾取物件音效
        /// </summary>
        public void PlayPickupSound()
        {
            PlaySFX("pickup", 0.5f, Random.Range(1.0f, 1.1f));
        }

        /// <summary>
        /// 播放UI點擊音效
        /// </summary>
        public void PlayUIClickSound()
        {
            PlaySFX("ui_click", 0.7f);
        }

        /// <summary>
        /// 播放購買音效
        /// </summary>
        public void PlayPurchaseSound()
        {
            PlaySFX("purchase", 0.9f);
        }

        /// <summary>
        /// 播放解鎖音效
        /// </summary>
        public void PlayUnlockSound()
        {
            PlaySFX("unlock", 1.0f);
        }

        #endregion

        #region 音量控制

        /// <summary>
        /// 設定主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// 設定音樂音量
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null && !isFadingMusic)
            {
                musicSource.volume = musicVolume;
            }
        }

        /// <summary>
        /// 設定音效音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (ambientSource != null)
            {
                ambientSource.volume = sfxVolume * 0.5f;
            }

            foreach (var source in sfxPool)
            {
                if (!source.isPlaying)
                {
                    source.volume = sfxVolume;
                }
            }
        }

        #endregion

        #region 場景音樂管理

        /// <summary>
        /// 根據場景播放音樂
        /// </summary>
        public void PlaySceneMusic(string sceneName)
        {
            switch (sceneName)
            {
                case "MainMenu":
                    if (mainMenuMusic != null)
                    {
                        PlayMusic(mainMenuMusic);
                    }
                    break;

                case "GameScene":
                    if (gameSceneMusic != null)
                    {
                        PlayMusic(gameSceneMusic);
                    }
                    PlayBarAmbience();
                    break;

                default:
                    Debug.LogWarning($"AudioManager: No music defined for scene '{sceneName}'");
                    break;
            }
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 是否正在播放音樂
        /// </summary>
        public bool IsPlayingMusic => musicSource != null && musicSource.isPlaying;

        /// <summary>
        /// 當前音樂音量
        /// </summary>
        public float MusicVolume => musicVolume;

        /// <summary>
        /// 當前音效音量
        /// </summary>
        public float SFXVolume => sfxVolume;

        #endregion
    }
}
