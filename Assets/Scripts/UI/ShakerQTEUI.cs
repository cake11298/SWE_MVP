using UnityEngine;
using UnityEngine.UI;
using BarSimulator.Systems;

namespace BarSimulator.UI
{
    /// <summary>
    /// Shaker QTE UI - 顯示技能檢查的UI
    /// </summary>
    public class ShakerQTEUI : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("UI References")]
        [Tooltip("QTE UI根物件")]
        [SerializeField] private GameObject qteUIRoot;
        
        [Tooltip("技能檢查UI根物件")]
        [SerializeField] private GameObject skillCheckRoot;
        
        [Tooltip("指針圖像")]
        [SerializeField] private RectTransform needleTransform;
        
        [Tooltip("成功區域圖像")]
        [SerializeField] private Image successZoneImage;
        
        [Tooltip("提示文字")]
        [SerializeField] private Text promptText;
        
        [Tooltip("結果文字")]
        [SerializeField] private Text resultText;
        
        [Header("Visual Settings")]
        [Tooltip("成功區域顏色")]
        [SerializeField] private Color successZoneColor = Color.green;
        
        [Tooltip("指針顏色")]
        [SerializeField] private Color needleColor = Color.white;
        
        [Tooltip("結果顯示時間（秒）")]
        [SerializeField] private float resultDisplayTime = 1.5f;
        
        #endregion

        #region Private Fields
        
        private ShakerQTESystem qteSystem;
        private float resultTimer;
        private bool showingResult;
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            // 初始化時隱藏UI
            if (qteUIRoot != null)
            {
                qteUIRoot.SetActive(false);
            }
            
            if (skillCheckRoot != null)
            {
                skillCheckRoot.SetActive(false);
            }
            
            if (resultText != null)
            {
                resultText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            // 更新結果顯示計時器
            if (showingResult)
            {
                resultTimer -= Time.deltaTime;
                if (resultTimer <= 0f)
                {
                    HideResult();
                }
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// 設置QTE系統引用
        /// </summary>
        public void SetQTESystem(ShakerQTESystem system)
        {
            // 取消訂閱舊系統
            if (qteSystem != null)
            {
                UnsubscribeFromEvents();
            }

            qteSystem = system;

            // 訂閱新系統
            if (qteSystem != null)
            {
                SubscribeToEvents();
            }
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// 訂閱QTE系統事件
        /// </summary>
        private void SubscribeToEvents()
        {
            if (qteSystem == null) return;

            qteSystem.OnQTEStart += OnQTEStart;
            qteSystem.OnQTEEnd += OnQTEEnd;
            qteSystem.OnQTEComplete += OnQTEComplete;
            qteSystem.OnSkillCheckResult += OnSkillCheckResult;
            qteSystem.OnNeedleRotation += OnNeedleRotation;
        }

        /// <summary>
        /// 取消訂閱QTE系統事件
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (qteSystem == null) return;

            qteSystem.OnQTEStart -= OnQTEStart;
            qteSystem.OnQTEEnd -= OnQTEEnd;
            qteSystem.OnQTEComplete -= OnQTEComplete;
            qteSystem.OnSkillCheckResult -= OnSkillCheckResult;
            qteSystem.OnNeedleRotation -= OnNeedleRotation;
        }

        /// <summary>
        /// QTE開始
        /// </summary>
        private void OnQTEStart()
        {
            if (qteUIRoot != null)
            {
                qteUIRoot.SetActive(true);
            }

            UpdatePromptText("Hold Right Mouse Button to shake!");
        }

        /// <summary>
        /// QTE結束
        /// </summary>
        private void OnQTEEnd()
        {
            if (skillCheckRoot != null)
            {
                skillCheckRoot.SetActive(false);
            }
        }

        /// <summary>
        /// QTE完成
        /// </summary>
        private void OnQTEComplete(bool success)
        {
            if (qteUIRoot != null)
            {
                qteUIRoot.SetActive(false);
            }

            // 顯示最終結果
            ShowResult(success ? "Successful shaken!" : "Shake Failed! Please try again.");
        }

        /// <summary>
        /// 技能檢查結果
        /// </summary>
        private void OnSkillCheckResult(bool success)
        {
            if (skillCheckRoot != null)
            {
                skillCheckRoot.SetActive(false);
            }

            // 顯示命中/失誤提示
            ShowResult(success ? "HIT!" : "MISS!");
        }

        /// <summary>
        /// 指針旋轉更新
        /// </summary>
        private void OnNeedleRotation(float angle)
        {
            // 顯示技能檢查UI
            if (skillCheckRoot != null && !skillCheckRoot.activeSelf)
            {
                skillCheckRoot.SetActive(true);
                UpdateSuccessZone();
            }

            // 更新指針旋轉
            if (needleTransform != null)
            {
                needleTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            }

            UpdatePromptText("Press R!");
        }

        /// <summary>
        /// 更新成功區域顯示
        /// </summary>
        private void UpdateSuccessZone()
        {
            if (successZoneImage == null || qteSystem == null) return;

            // 設置成功區域的角度和大小
            float angle = qteSystem.SuccessZoneAngle;
            float size = qteSystem.SuccessZoneSize;

            // 旋轉成功區域到正確位置
            successZoneImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);

            // 設置顏色
            successZoneImage.color = successZoneColor;
        }

        /// <summary>
        /// 更新提示文字
        /// </summary>
        private void UpdatePromptText(string text)
        {
            if (promptText != null)
            {
                promptText.text = text;
            }
        }

        /// <summary>
        /// 顯示結果
        /// </summary>
        private void ShowResult(string text)
        {
            if (resultText != null)
            {
                resultText.text = text;
                resultText.gameObject.SetActive(true);
                showingResult = true;
                resultTimer = resultDisplayTime;
            }
        }

        /// <summary>
        /// 隱藏結果
        /// </summary>
        private void HideResult()
        {
            if (resultText != null)
            {
                resultText.gameObject.SetActive(false);
            }
            showingResult = false;
        }
        
        #endregion

        #region Cleanup
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        #endregion
    }
}
