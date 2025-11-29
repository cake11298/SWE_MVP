using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace BarSimulator.Systems
{
    /// <summary>
    /// 場景載入系統 - 處理場景切換和載入畫面
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        #region Singleton

        private static SceneLoader instance;
        public static SceneLoader Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("Loading Screen")]
        [Tooltip("載入畫面UI")]
        [SerializeField] private GameObject loadingScreen;

        [Tooltip("載入進度條（可選）")]
        [SerializeField] private UnityEngine.UI.Slider progressBar;

        [Tooltip("載入提示文字（可選）")]
        [SerializeField] private TMPro.TextMeshProUGUI loadingText;

        [Header("Settings")]
        [Tooltip("最短載入畫面顯示時間（秒）")]
        [SerializeField] private float minLoadingTime = 1.0f;

        [Tooltip("淡入淡出時間（秒）")]
        [SerializeField] private float fadeTime = 0.5f;

        #endregion

        #region 私有欄位

        private bool isLoading = false;
        private CanvasGroup loadingCanvasGroup;

        // 事件
        public System.Action<string> OnSceneLoadStart;
        public System.Action<string> OnSceneLoadComplete;
        public System.Action<float> OnLoadingProgress;

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

            // Setup loading screen
            if (loadingScreen != null)
            {
                loadingCanvasGroup = loadingScreen.GetComponent<CanvasGroup>();
                if (loadingCanvasGroup == null)
                {
                    loadingCanvasGroup = loadingScreen.AddComponent<CanvasGroup>();
                }
                loadingScreen.SetActive(false);
            }
        }

        #endregion

        #region 場景載入

        /// <summary>
        /// 載入場景（場景名稱）
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (isLoading)
            {
                Debug.LogWarning($"SceneLoader: Already loading a scene, ignoring request for '{sceneName}'");
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneName));
        }

        /// <summary>
        /// 載入場景（場景索引）
        /// </summary>
        public void LoadScene(int sceneIndex)
        {
            if (isLoading)
            {
                Debug.LogWarning($"SceneLoader: Already loading a scene, ignoring request for scene {sceneIndex}");
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneIndex));
        }

        /// <summary>
        /// 重新載入當前場景
        /// </summary>
        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// 載入主選單
        /// </summary>
        public void LoadMainMenu()
        {
            LoadScene("MainMenu");
        }

        /// <summary>
        /// 載入遊戲場景
        /// </summary>
        public void LoadGameScene()
        {
            LoadScene("GameScene");
        }

        /// <summary>
        /// 退出遊戲
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("SceneLoader: Quitting game");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        #endregion

        #region 非同步載入

        /// <summary>
        /// 非同步載入場景（場景名稱）
        /// </summary>
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoading = true;
            float startTime = Time.realtimeSinceStartup;

            OnSceneLoadStart?.Invoke(sceneName);
            Debug.Log($"SceneLoader: Loading scene '{sceneName}'");

            // Show loading screen
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
                yield return StartCoroutine(FadeLoadingScreen(1f));
            }

            // Update loading text
            if (loadingText != null)
            {
                loadingText.text = "Loading...";
            }

            // Start async load
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Wait for loading to complete
            while (!asyncLoad.isDone)
            {
                // Progress is 0-0.9 while loading, 0.9-1.0 when ready
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                // Update progress bar
                if (progressBar != null)
                {
                    progressBar.value = progress;
                }

                OnLoadingProgress?.Invoke(progress);

                // When loading is done (progress >= 0.9), check minimum time
                if (asyncLoad.progress >= 0.9f)
                {
                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    if (elapsedTime >= minLoadingTime)
                    {
                        // Activate scene
                        asyncLoad.allowSceneActivation = true;
                    }
                }

                yield return null;
            }

            // Complete
            if (progressBar != null)
            {
                progressBar.value = 1f;
            }

            OnSceneLoadComplete?.Invoke(sceneName);
            Debug.Log($"SceneLoader: Scene '{sceneName}' loaded");

            // Hide loading screen
            if (loadingScreen != null)
            {
                yield return StartCoroutine(FadeLoadingScreen(0f));
                loadingScreen.SetActive(false);
            }

            isLoading = false;
        }

        /// <summary>
        /// 非同步載入場景（場景索引）
        /// </summary>
        private IEnumerator LoadSceneAsync(int sceneIndex)
        {
            isLoading = true;
            float startTime = Time.realtimeSinceStartup;

            string sceneName = SceneManager.GetSceneByBuildIndex(sceneIndex).name;
            OnSceneLoadStart?.Invoke(sceneName);
            Debug.Log($"SceneLoader: Loading scene index {sceneIndex}");

            // Show loading screen
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
                yield return StartCoroutine(FadeLoadingScreen(1f));
            }

            // Update loading text
            if (loadingText != null)
            {
                loadingText.text = "Loading...";
            }

            // Start async load
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
            asyncLoad.allowSceneActivation = false;

            // Wait for loading to complete
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                if (progressBar != null)
                {
                    progressBar.value = progress;
                }

                OnLoadingProgress?.Invoke(progress);

                if (asyncLoad.progress >= 0.9f)
                {
                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    if (elapsedTime >= minLoadingTime)
                    {
                        asyncLoad.allowSceneActivation = true;
                    }
                }

                yield return null;
            }

            if (progressBar != null)
            {
                progressBar.value = 1f;
            }

            OnSceneLoadComplete?.Invoke(sceneName);
            Debug.Log($"SceneLoader: Scene index {sceneIndex} loaded");

            // Hide loading screen
            if (loadingScreen != null)
            {
                yield return StartCoroutine(FadeLoadingScreen(0f));
                loadingScreen.SetActive(false);
            }

            isLoading = false;
        }

        /// <summary>
        /// 淡入淡出載入畫面
        /// </summary>
        private IEnumerator FadeLoadingScreen(float targetAlpha)
        {
            if (loadingCanvasGroup == null) yield break;

            float startAlpha = loadingCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeTime;
                loadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            loadingCanvasGroup.alpha = targetAlpha;
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 是否正在載入場景
        /// </summary>
        public bool IsLoading => isLoading;

        /// <summary>
        /// 當前場景名稱
        /// </summary>
        public string CurrentSceneName => SceneManager.GetActiveScene().name;

        #endregion
    }
}
