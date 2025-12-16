using UnityEngine;
using System;
using System.Collections;

namespace BarSimulator.Systems
{
    /// <summary>
    /// Shaker QTE (Quick Time Event) System
    /// 搖酒器QTE系統 - 處理搖酒時的技能檢查
    /// </summary>
    public class ShakerQTESystem : MonoBehaviour
    {
        #region Events
        
        public event Action OnQTEStart;
        public event Action OnQTEEnd;
        public event Action<bool> OnQTEComplete; // true = success, false = fail
        public event Action<bool> OnSkillCheckResult; // true = hit, false = miss
        public event Action<float> OnNeedleRotation; // 指針旋轉角度 (0-360)
        
        #endregion

        #region Serialized Fields
        
        [Header("QTE Settings")]
        [Tooltip("QTE持續時間（秒）")]
        [SerializeField] private float qteDuration = 20f;
        
        [Tooltip("技能檢查觸發次數")]
        [SerializeField] private int totalSkillChecks = 4;
        
        [Tooltip("需要成功的次數")]
        [SerializeField] private int requiredSuccesses = 3;
        
        [Tooltip("技能檢查間隔（秒）")]
        [SerializeField] private float skillCheckInterval = 1f;
        
        [Tooltip("每個技能檢查的持續時間（秒）")]
        [SerializeField] private float skillCheckDuration = 3f;
        
        [Tooltip("技能檢查判定區域大小（度）")]
        [SerializeField] private float successZoneSize = 6f; // 5-7度
        
        [Tooltip("從起始到判定區域的最小角度")]
        [SerializeField] private float minAngleToSuccessZone = 100f;
        
        [Tooltip("指針旋轉速度（度/秒）")]
        [SerializeField] private float needleRotationSpeed = 360f; // 1秒轉一圈
        
        #endregion

        #region Private Fields
        
        private bool isQTEActive;
        private float qteTimer;
        private int currentSkillCheckIndex;
        private int successfulChecks;
        private bool isSkillCheckActive;
        private float needleAngle; // 當前指針角度 (0-360)
        private float successZoneAngle; // 成功區域的角度
        private bool canTriggerNextCheck;
        private float nextCheckTimer;
        private float skillCheckTimer; // 當前技能檢查的計時器
        private bool wasInterrupted; // 是否被中斷
        
        #endregion

        #region Public Properties
        
        public bool IsQTEActive => isQTEActive;
        public bool IsSkillCheckActive => isSkillCheckActive;
        public float NeedleAngle => needleAngle;
        public float SuccessZoneAngle => successZoneAngle;
        public float SuccessZoneSize => successZoneSize;
        public int SuccessfulChecks => successfulChecks;
        public int TotalSkillChecks => totalSkillChecks;
        public float QTEProgress => isQTEActive ? (qteTimer / qteDuration) : 0f;
        
        #endregion

        #region Unity Lifecycle
        
        private void Update()
        {
            if (!isQTEActive) return;

            // 更新QTE計時器
            qteTimer += Time.deltaTime;

            // 更新下一次技能檢查計時器
            if (!isSkillCheckActive && canTriggerNextCheck)
            {
                nextCheckTimer += Time.deltaTime;
            }

            // 更新指針旋轉
            if (isSkillCheckActive)
            {
                skillCheckTimer += Time.deltaTime;
                
                // 檢查技能檢查是否超時
                if (skillCheckTimer >= skillCheckDuration)
                {
                    SkillCheckMissed();
                }
                else
                {
                    UpdateNeedleRotation();
                    CheckSkillCheckInput();
                }
            }

            // 檢查是否該觸發下一個技能檢查
            if (!isSkillCheckActive && canTriggerNextCheck && nextCheckTimer >= skillCheckInterval)
            {
                if (currentSkillCheckIndex < totalSkillChecks)
                {
                    StartSkillCheck();
                }
            }

            // 檢查QTE是否結束
            if (qteTimer >= qteDuration)
            {
                EndQTE();
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// 開始QTE
        /// </summary>
        public void StartQTE()
        {
            if (isQTEActive)
            {
                Debug.LogWarning("ShakerQTESystem: QTE already active!");
                return;
            }

            Debug.Log("ShakerQTESystem: Starting QTE - 20 seconds, 4 skill checks");
            
            isQTEActive = true;
            qteTimer = 0f;
            currentSkillCheckIndex = 0;
            successfulChecks = 0;
            canTriggerNextCheck = true;
            nextCheckTimer = 0f;
            wasInterrupted = false;

            OnQTEStart?.Invoke();

            // 立即觸發第一個技能檢查
            StartSkillCheck();
        }

        /// <summary>
        /// 強制結束QTE（中斷）
        /// </summary>
        public void CancelQTE()
        {
            if (!isQTEActive) return;

            Debug.Log("ShakerQTESystem: QTE cancelled (interrupted)");
            
            wasInterrupted = true;
            isQTEActive = false;
            isSkillCheckActive = false;
            
            // 中斷視為失敗
            OnQTEComplete?.Invoke(false);
            OnQTEEnd?.Invoke();
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// 開始一個技能檢查
        /// </summary>
        private void StartSkillCheck()
        {
            if (currentSkillCheckIndex >= totalSkillChecks)
            {
                return;
            }

            Debug.Log($"ShakerQTESystem: Starting skill check {currentSkillCheckIndex + 1}/{totalSkillChecks}");

            isSkillCheckActive = true;
            needleAngle = 0f;
            canTriggerNextCheck = false;
            nextCheckTimer = 0f;
            skillCheckTimer = 0f;

            // 隨機生成成功區域的角度（確保至少100度的反應時間）
            successZoneAngle = UnityEngine.Random.Range(minAngleToSuccessZone, 360f - successZoneSize);

            currentSkillCheckIndex++;
        }

        /// <summary>
        /// 更新指針旋轉
        /// </summary>
        private void UpdateNeedleRotation()
        {
            // 根據技能檢查持續時間調整旋轉速度，確保在持續時間內轉完一圈
            float adjustedSpeed = 360f / skillCheckDuration;
            needleAngle += adjustedSpeed * Time.deltaTime;

            // 限制在0-360度範圍內
            if (needleAngle >= 360f)
            {
                needleAngle = 360f;
            }

            OnNeedleRotation?.Invoke(needleAngle);
        }

        /// <summary>
        /// 檢查技能檢查輸入
        /// </summary>
        private void CheckSkillCheckInput()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                CheckSkillCheckHit();
            }
        }

        /// <summary>
        /// 檢查是否命中成功區域
        /// </summary>
        private void CheckSkillCheckHit()
        {
            bool isHit = IsNeedleInSuccessZone();

            if (isHit)
            {
                SkillCheckSuccess();
            }
            else
            {
                SkillCheckMissed();
            }
        }

        /// <summary>
        /// 檢查指針是否在成功區域內
        /// </summary>
        private bool IsNeedleInSuccessZone()
        {
            float halfZoneSize = successZoneSize / 2f;
            float zoneStart = successZoneAngle - halfZoneSize;
            float zoneEnd = successZoneAngle + halfZoneSize;

            // 處理角度環繞
            if (zoneStart < 0f)
            {
                return needleAngle >= (360f + zoneStart) || needleAngle <= zoneEnd;
            }
            else if (zoneEnd > 360f)
            {
                return needleAngle >= zoneStart || needleAngle <= (zoneEnd - 360f);
            }
            else
            {
                return needleAngle >= zoneStart && needleAngle <= zoneEnd;
            }
        }

        /// <summary>
        /// 技能檢查成功
        /// </summary>
        private void SkillCheckSuccess()
        {
            Debug.Log($"ShakerQTESystem: Skill check HIT! ({successfulChecks + 1}/{requiredSuccesses})");
            
            successfulChecks++;
            isSkillCheckActive = false;
            canTriggerNextCheck = true;
            nextCheckTimer = 0f;

            OnSkillCheckResult?.Invoke(true);
        }

        /// <summary>
        /// 技能檢查失敗
        /// </summary>
        private void SkillCheckMissed()
        {
            Debug.Log($"ShakerQTESystem: Skill check MISSED! ({successfulChecks}/{requiredSuccesses})");
            
            isSkillCheckActive = false;
            canTriggerNextCheck = true;
            nextCheckTimer = 0f;

            OnSkillCheckResult?.Invoke(false);
        }

        /// <summary>
        /// 結束QTE
        /// </summary>
        private void EndQTE()
        {
            bool success = successfulChecks >= requiredSuccesses;
            
            Debug.Log($"ShakerQTESystem: QTE ended - {(success ? "SUCCESS" : "FAILED")} ({successfulChecks}/{requiredSuccesses})");

            isQTEActive = false;
            isSkillCheckActive = false;

            OnQTEComplete?.Invoke(success);
            OnQTEEnd?.Invoke();
        }
        
        #endregion
    }
}
