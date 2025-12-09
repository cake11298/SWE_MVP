using UnityEngine;
using System.IO;
using System.Collections.Generic;
using BarSimulator.Data;

namespace BarSimulator.Systems
{
    /// <summary>
    /// 存檔系統 - 處理遊戲進度存檔和讀檔
    /// </summary>
    public class SaveLoadSystem : MonoBehaviour
    {
        #region Singleton

        private static SaveLoadSystem instance;
        public static SaveLoadSystem Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("Save Settings")]
        [Tooltip("存檔檔名")]
        [SerializeField] private string saveFileName = "savegame.json";

        [Tooltip("是否使用加密（簡單混淆）")]
        [SerializeField] private bool useEncryption = false;

        #endregion

        #region 私有欄位

        private string savePath;
        private SaveData currentSaveData;

        // 事件
        public System.Action OnSaveComplete;
        public System.Action OnLoadComplete;
        public System.Action<string> OnSaveError;
        public System.Action<string> OnLoadError;

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

            // Set save path
            savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            Debug.Log($"SaveLoadSystem: Save path is {savePath}");
        }

        #endregion

        #region 存檔

        /// <summary>
        /// 儲存遊戲
        /// </summary>
        public bool SaveGame()
        {
            try
            {
                // Collect save data
                SaveData saveData = CollectSaveData();

                // Convert to JSON
                string json = JsonUtility.ToJson(saveData, true);

                // Encrypt if enabled
                if (useEncryption)
                {
                    json = EncryptString(json);
                }

                // Write to file
                File.WriteAllText(savePath, json);

                currentSaveData = saveData;
                OnSaveComplete?.Invoke();

                Debug.Log($"SaveLoadSystem: Game saved successfully to {savePath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveLoadSystem: Failed to save game - {e.Message}");
                OnSaveError?.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 收集存檔資料
        /// </summary>
        private SaveData CollectSaveData()
        {
            SaveData data = new SaveData();

            // Player progress
            data.playerMoney = UpgradeSystem.Instance?.CurrentMoney ?? 0;
            data.playTime = Time.time; // Simple playtime tracking

            // Liquor data
            var liquorDatabase = UpgradeSystem.Instance?.LiquorDatabase;
            if (liquorDatabase != null && liquorDatabase.liquors != null)
            {
                data.liquorData = new List<LiquorSaveData>();
                foreach (var liquor in liquorDatabase.liquors)
                {
                    data.liquorData.Add(new LiquorSaveData
                    {
                        id = liquor.id,
                        level = liquor.level,
                        isLocked = liquor.isLocked
                    });
                }
            }

            // Recipe data
            var recipes = RecipeDatabase.AllRecipes;
            if (recipes != null)
            {
                data.recipeData = new List<RecipeSaveData>();
                foreach (var recipe in recipes)
                {
                    data.recipeData.Add(new RecipeSaveData
                    {
                        name = recipe.Name,
                        isLocked = recipe.IsLocked
                    });
                }
            }

            // Settings (will be populated by SettingsManager)
            data.settings = new SettingsSaveData
            {
                masterVolume = 1.0f,
                musicVolume = 0.7f,
                sfxVolume = 1.0f,
                mouseSensitivity = 1.0f,
                graphicsQuality = QualitySettings.GetQualityLevel()
            };

            // Metadata
            data.saveVersion = 1;
            data.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return data;
        }

        #endregion

        #region 讀檔

        /// <summary>
        /// 載入遊戲
        /// </summary>
        public bool LoadGame()
        {
            if (!HasSaveFile())
            {
                Debug.LogWarning("SaveLoadSystem: No save file found");
                OnLoadError?.Invoke("No save file found");
                return false;
            }

            try
            {
                // Read file
                string json = File.ReadAllText(savePath);

                // Decrypt if enabled
                if (useEncryption)
                {
                    json = DecryptString(json);
                }

                // Parse JSON
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                // Apply save data
                ApplySaveData(saveData);

                currentSaveData = saveData;
                OnLoadComplete?.Invoke();

                Debug.Log($"SaveLoadSystem: Game loaded successfully from {savePath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveLoadSystem: Failed to load game - {e.Message}");
                OnLoadError?.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 應用存檔資料
        /// </summary>
        private void ApplySaveData(SaveData data)
        {
            // Apply player progress
            if (UpgradeSystem.Instance != null)
            {
                UpgradeSystem.Instance.SetMoney(data.playerMoney);
            }

            // Apply liquor data
            var liquorDatabase = UpgradeSystem.Instance?.LiquorDatabase;
            if (liquorDatabase != null && data.liquorData != null)
            {
                foreach (var liquorSave in data.liquorData)
                {
                    var liquor = liquorDatabase.GetLiquor(liquorSave.id);
                    if (liquor != null)
                    {
                        liquor.level = liquorSave.level;
                        liquor.isLocked = liquorSave.isLocked;
                    }
                }
            }

            // Apply recipe data
            if (data.recipeData != null)
            {
                foreach (var recipeSave in data.recipeData)
                {
                    var recipe = RecipeDatabase.GetRecipeByName(recipeSave.name);
                    if (recipe != null)
                    {
                        recipe.IsLocked = recipeSave.isLocked;
                    }
                }
            }

            // Apply settings (will be handled by SettingsManager)
            if (data.settings != null)
            {
                QualitySettings.SetQualityLevel(data.settings.graphicsQuality);
                // Volume and sensitivity will be applied by SettingsManager
            }

            Debug.Log($"SaveLoadSystem: Applied save data (Version {data.saveVersion}, Saved: {data.saveTime})");
        }

        #endregion

        #region 存檔管理

        /// <summary>
        /// 檢查是否有存檔
        /// </summary>
        public bool HasSaveFile()
        {
            return File.Exists(savePath);
        }

        /// <summary>
        /// 刪除存檔
        /// </summary>
        public bool DeleteSaveFile()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    currentSaveData = null;
                    Debug.Log("SaveLoadSystem: Save file deleted");
                    return true;
                }
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveLoadSystem: Failed to delete save file - {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 取得存檔資訊
        /// </summary>
        public SaveFileInfo GetSaveFileInfo()
        {
            if (!HasSaveFile()) return null;

            try
            {
                FileInfo fileInfo = new FileInfo(savePath);
                return new SaveFileInfo
                {
                    fileName = saveFileName,
                    filePath = savePath,
                    fileSize = fileInfo.Length,
                    lastModified = fileInfo.LastWriteTime
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveLoadSystem: Failed to get save file info - {e.Message}");
                return null;
            }
        }

        #endregion

        #region 加密/解密（簡單混淆）

        /// <summary>
        /// 簡單字串加密（XOR）
        /// </summary>
        private string EncryptString(string text)
        {
            const int key = 129;
            char[] chars = text.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)(chars[i] ^ key);
            }
            return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(chars));
        }

        /// <summary>
        /// 簡單字串解密（XOR）
        /// </summary>
        private string DecryptString(string encryptedText)
        {
            const int key = 129;
            byte[] bytes = System.Convert.FromBase64String(encryptedText);
            char[] chars = System.Text.Encoding.UTF8.GetString(bytes).ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)(chars[i] ^ key);
            }
            return new string(chars);
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 當前存檔資料
        /// </summary>
        public SaveData CurrentSaveData => currentSaveData;

        /// <summary>
        /// 存檔路徑
        /// </summary>
        public string SavePath => savePath;

        #endregion
    }

    #region 存檔資料結構

    /// <summary>
    /// 完整存檔資料
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        public int saveVersion = 1;
        public string saveTime;

        // Player progress
        public int playerMoney;
        public float playTime;

        // Liquor states
        public List<LiquorSaveData> liquorData;

        // Recipe states
        public List<RecipeSaveData> recipeData;

        // Settings
        public SettingsSaveData settings;
    }

    /// <summary>
    /// 酒類存檔資料
    /// </summary>
    [System.Serializable]
    public class LiquorSaveData
    {
        public string id;
        public int level;
        public bool isLocked;
    }

    /// <summary>
    /// 配方存檔資料
    /// </summary>
    [System.Serializable]
    public class RecipeSaveData
    {
        public string name;
        public bool isLocked;
    }

    /// <summary>
    /// 設定存檔資料
    /// </summary>
    [System.Serializable]
    public class SettingsSaveData
    {
        public float masterVolume = 1.0f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 1.0f;
        public float mouseSensitivity = 1.0f;
        public int graphicsQuality = 2;
    }

    /// <summary>
    /// 存檔檔案資訊
    /// </summary>
    public class SaveFileInfo
    {
        public string fileName;
        public string filePath;
        public long fileSize;
        public System.DateTime lastModified;
    }

    #endregion
}
