using BarSimulator.Core;
using BarSimulator.Player;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

using Random = UnityEngine.Random;

namespace BarSimulator.QTE
{
    // 搖晃狀態
    enum ShakeState
    {
        Wait,
        Shaking,
        STOP,
        Finished
    }

    // QTE管理器
    public class QTEManager: MonoBehaviour
    {
        #region UI 組件
        [SerializeField] private GameObject qteCanvas;
        [SerializeField] private Image pointer;
        [SerializeField] private Image circle;
        #endregion
    
        #region 單例模式

        public static QTEManager Instance { get; private set; }
        
        private void Awake()
        {
            // 正確的單例初始化：確保是場景中的那個組件
            if (Instance == null) { Instance = this; }
            else { Destroy(gameObject); }
            
            // 初始隱藏
            if (qteCanvas != null) qteCanvas.SetActive(false);
        }

        #endregion
        
        private ShakeState shakeState = ShakeState.Wait;
        private ShakeQTE currentShakeQTE = null;
        private float qualityMultiplier = 0.2f;

        // 計算品質倍率並重置狀態
        public float GetQualityAndResetQTE()
        {
            shakeState = ShakeState.Wait;

            return qualityMultiplier;
        }

        // 開始搖晃QTE，由ImproveInteractionSystem.HandleInput呼叫
        public void StartShakeQTE()
        {
            // 等待時創建新的shakeQTE
            if (shakeState == ShakeState.Wait)
            {
                Debug.Log("[QTE] 開始QTE");
                shakeState = ShakeState.Shaking;
                
                currentShakeQTE = new ShakeQTE(qteCanvas, pointer, circle);
                // 設定搖酒完成後的callback
                currentShakeQTE.onFinish += HandleFinishShakeQTE;
                StartCoroutine(currentShakeQTE.StartQTE());
            }
            else if(shakeState == ShakeState.STOP)
            {
                Debug.Log("[QTE] 繼續QTE");
                shakeState = ShakeState.Shaking;
                currentShakeQTE.isShaking = true;
            }
        }

        // 中斷QTE，由ImproveInteractionSystem.HandleInput呼叫
        public void StopShakeQTE()
        {
            if (shakeState != ShakeState.Shaking)
                return;

            Debug.Log("[QTE] 中斷QTE");
            shakeState = ShakeState.STOP;
            currentShakeQTE.isShaking = false;
            
        }

        // 完成搖晃QTE
        public void HandleFinishShakeQTE(float quality)
        {
            Debug.Log($"[QTE] QTE完成, 品質倍率: {quality}");
            qualityMultiplier = quality;
            shakeState = ShakeState.Finished;
            currentShakeQTE = null;
        }
    }

    // 管理本次QTE
    public class ShakeQTE
    {
        private GameObject qteCanvas;
        private Image pointer;
        private Image circle;
        public Action<float> onFinish = null;
        public bool isShaking = true;

        private List<bool> results = new List<bool>();

        public ShakeQTE(GameObject qteCanvas, Image pointer, Image circle)
        {
            this.qteCanvas = qteCanvas;
            this.pointer = pointer;
            this.circle = circle;
            results.Clear();
        }

        public System.Collections.IEnumerator StartQTE()
        {
            return GetQTERoutine();
        }

        public System.Collections.IEnumerator GetQTERoutine()
        {
            // 進行3次QTE
            for(int i = 0; i < 3; i++)
            {
                // 旋轉方向
                float currentDirection = (Random.value > 0.5f) ? 1f : -1f;
                // 選轉速度
                float rotateSpeed = 180f + i * Random.Range(0f, 30f);
                // 判定點角度
                float randomAngle = Random.Range(0f, 360f);
                float currentTimer = 0f;
                float waitTime = Random.Range(1f, 10f);

                pointer.rectTransform.localEulerAngles = new Vector3(0, 0, -currentDirection * 10f);
                circle.rectTransform.localEulerAngles = new Vector3(0, 0, randomAngle);
                float totalRotated = -currentDirection * 10f;

                // 等待隨機時間後開始搖晃
                Debug.Log($"[QTERoutine] 等待 {waitTime:.0f}秒 QTE");
                while (currentTimer < waitTime)
                {
                    yield return null; 

                    if (!isShaking)
                        continue;

                    currentTimer += Time.deltaTime;
                }

                qteCanvas.SetActive(true);
                
                // 指針旋轉直到完成一圈
                Debug.Log($"[QTERoutine] 開始第 {i+1} 回合 QTE");
                while (totalRotated < 360f + currentDirection * 10f)
                {
                    if (!isShaking)
                    {
                        results.Add(false);
                        Debug.Log($"[QTERoutine] QTE被中斷, 第 {i+1} 回合失敗！");
                        break;
                    }
                        

                    // 旋轉
                    float frameRotation = rotateSpeed * Time.deltaTime;
                    pointer.rectTransform.Rotate(Vector3.forward, currentDirection * frameRotation);
                    totalRotated += frameRotation;

                    
                    // 判定QTE
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(pointer.rectTransform.localEulerAngles.z, randomAngle));

                        if (angleDiff <= 30f)
                        {
                            results.Add(true);
                        }
                        else
                        {
                            results.Add(false);
                        }

                        Debug.Log($"[QTERoutine] 第 {i+1} 回合成功！");
                        break;
                    }

                    yield return null; 
                }

                if(i+1 > results.Count)
                {
                    results.Add(false);
                    Debug.Log($"[QTERoutine] 第 {i+1} 回合失敗！");
                }
                
                qteCanvas.SetActive(false);
            }
            
            JudgeQTE();
        }

        // 判定QTE結果
        public void JudgeQTE()
        {
            int successCount = 0;

            foreach (bool result in results)
            {
                if (result)
                    successCount++;
            }

            if (successCount == 3)
                onFinish?.Invoke(1f);
            else if (successCount == 2)
                onFinish?.Invoke(0.8f);
            else
                onFinish?.Invoke(0.5f);
        }
    }
}