using UnityEngine;
using System.Collections.Generic;
using BarSimulator.Objects;
using BarSimulator.Managers;
using BarSimulator.Systems;
using BarSimulator.UI;

namespace BarSimulator.Interaction
{
    /// <summary>
    /// 玻璃瓶回收區
    /// 示範混合架構優勢：在 Scene 中放置此組件，運行時自動處理回收邏輯
    /// 玩家可以將空酒瓶放回此區域以獲得獎勵
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class BottleReturnZone : MonoBehaviour
    {
        #region 序列化欄位

        [Header("回收設定")]
        [Tooltip("每個空瓶的回收獎勵金額")]
        [SerializeField] private int rewardPerBottle = 10;

        [Tooltip("是否自動回收（進入即回收）")]
        [SerializeField] private bool autoReturn = true;

        [Tooltip("手動回收提示文字")]
        [SerializeField] private string returnPrompt = "按 E 回收空瓶";

        [Header("視覺效果")]
        [Tooltip("回收區顏色")]
        [SerializeField] private Color zoneColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);

        [Tooltip("高亮顏色（有瓶子時）")]
        [SerializeField] private Color highlightColor = new Color(0.4f, 1f, 0.4f, 0.5f);

        [Tooltip("回收特效（可選）")]
        [SerializeField] private GameObject returnEffectPrefab;

        [Header("音效")]
        [Tooltip("回收音效")]
        [SerializeField] private AudioClip returnSound;

        [Tooltip("錯誤音效（瓶子不為空）")]
        [SerializeField] private AudioClip errorSound;

        [Header("規則")]
        [Tooltip("是否只回收空瓶")]
        [SerializeField] private bool onlyAcceptEmptyBottles = true;

        [Tooltip("回收後是否銷毀瓶子")]
        [SerializeField] private bool destroyOnReturn = false;

        [Tooltip("不銷毀時的重置位置")]
        [SerializeField] private Transform bottleResetPosition;

        #endregion

        #region 私有欄位

        private BoxCollider triggerZone;
        private MeshRenderer zoneRenderer;
        private Material zoneMaterial;
        private List<Bottle> bottlesInZone = new List<Bottle>();
        private Dictionary<Bottle, float> bottleEnterTime = new Dictionary<Bottle, float>();

        // 統計
        private int totalBottlesReturned = 0;
        private int totalRewardEarned = 0;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            SetupTriggerZone();
            SetupVisuals();
        }

        private void Update()
        {
            // 手動回收模式：檢測玩家按鍵
            if (!autoReturn && bottlesInZone.Count > 0)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    ReturnAllBottlesInZone();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Bottle bottle = other.GetComponent<Bottle>();
            if (bottle != null && !bottlesInZone.Contains(bottle))
            {
                bottlesInZone.Add(bottle);
                bottleEnterTime[bottle] = Time.time;

                Debug.Log($"BottleReturnZone: {bottle.name} 進入回收區");

                // 自動回收模式
                if (autoReturn)
                {
                    ReturnBottle(bottle);
                }
                else
                {
                    // 顯示提示
                    ShowReturnPrompt(true);
                    UpdateVisuals();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Bottle bottle = other.GetComponent<Bottle>();
            if (bottle != null && bottlesInZone.Contains(bottle))
            {
                bottlesInZone.Remove(bottle);
                bottleEnterTime.Remove(bottle);

                Debug.Log($"BottleReturnZone: {bottle.name} 離開回收區");

                if (bottlesInZone.Count == 0)
                {
                    ShowReturnPrompt(false);
                    UpdateVisuals();
                }
            }
        }

        #endregion

        #region 設置

        private void SetupTriggerZone()
        {
            triggerZone = GetComponent<BoxCollider>();
            triggerZone.isTrigger = true;

            // 確保有合理的大小
            if (triggerZone.size == Vector3.one)
            {
                triggerZone.size = new Vector3(2f, 1f, 2f);
            }
        }

        private void SetupVisuals()
        {
            // 創建視覺化區域（如果沒有 MeshRenderer）
            zoneRenderer = GetComponent<MeshRenderer>();
            if (zoneRenderer == null)
            {
                var meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.mesh = CreatePlaneMesh();

                zoneRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            // 創建半透明材質
            zoneMaterial = new Material(Shader.Find("Standard"));
            zoneMaterial.color = zoneColor;
            zoneMaterial.SetFloat("_Mode", 3); // Transparent
            zoneMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            zoneMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            zoneMaterial.SetInt("_ZWrite", 0);
            zoneMaterial.DisableKeyword("_ALPHATEST_ON");
            zoneMaterial.EnableKeyword("_ALPHABLEND_ON");
            zoneMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            zoneMaterial.renderQueue = 3000;
            zoneRenderer.material = zoneMaterial;
        }

        private Mesh CreatePlaneMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-1, 0, -1),
                new Vector3(1, 0, -1),
                new Vector3(1, 0, 1),
                new Vector3(-1, 0, 1)
            };
            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.normals = new Vector3[]
            {
                Vector3.up, Vector3.up, Vector3.up, Vector3.up
            };
            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            return mesh;
        }

        #endregion

        #region 回收邏輯

        /// <summary>
        /// 回收單個瓶子
        /// </summary>
        private void ReturnBottle(Bottle bottle)
        {
            // 檢查是否為空瓶
            if (onlyAcceptEmptyBottles && !IsBottleEmpty(bottle))
            {
                Debug.Log($"BottleReturnZone: {bottle.name} 不是空瓶，無法回收");
                PlaySound(errorSound);
                ShowMessage("只能回收空瓶！");
                return;
            }

            // 回收成功
            Debug.Log($"BottleReturnZone: 回收 {bottle.name}，獲得 ${rewardPerBottle}");

            // 給予獎勵
            GiveReward(rewardPerBottle);

            // 播放特效和音效
            PlayReturnEffect(bottle.transform.position);
            PlaySound(returnSound);
            ShowMessage($"+${rewardPerBottle}");

            // 統計
            totalBottlesReturned++;
            totalRewardEarned += rewardPerBottle;

            // 處理瓶子
            HandleBottleAfterReturn(bottle);

            // 從列表移除
            bottlesInZone.Remove(bottle);
            bottleEnterTime.Remove(bottle);
        }

        /// <summary>
        /// 回收區域內所有瓶子
        /// </summary>
        private void ReturnAllBottlesInZone()
        {
            if (bottlesInZone.Count == 0) return;

            List<Bottle> bottlesToReturn = new List<Bottle>(bottlesInZone);

            foreach (var bottle in bottlesToReturn)
            {
                ReturnBottle(bottle);
            }

            ShowReturnPrompt(false);
            UpdateVisuals();
        }

        /// <summary>
        /// 處理回收後的瓶子
        /// </summary>
        private void HandleBottleAfterReturn(Bottle bottle)
        {
            if (destroyOnReturn)
            {
                // 銷毀瓶子
                Destroy(bottle.gameObject, 0.5f);
            }
            else if (bottleResetPosition != null)
            {
                // 重置到指定位置
                bottle.transform.position = bottleResetPosition.position;
                bottle.transform.rotation = bottleResetPosition.rotation;

                // 重置瓶子狀態
                if (bottle.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
            else
            {
                // 簡單地向上移動一點（避免重複觸發）
                bottle.transform.position += Vector3.up * 2f;
            }
        }

        #endregion

        #region 輔助方法

        private bool IsBottleEmpty(Bottle bottle)
        {
            // 檢查瓶子是否為空
            // 這裡需要根據你的 Bottle 類實現來判斷
            // 假設 Bottle 有一個 IsEmpty 屬性或方法
            return true; // 暫時返回 true，需要根據實際實現修改
        }

        private void GiveReward(int amount)
        {
            // 給予玩家金錢獎勵
            if (UpgradeSystem.Instance != null)
            {
                UpgradeSystem.Instance.AddMoney(amount);
            }
        }

        private void PlayReturnEffect(Vector3 position)
        {
            if (returnEffectPrefab != null)
            {
                GameObject effect = Instantiate(returnEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            else
            {
                // 簡單的粒子效果（可選）
                Debug.Log($"BottleReturnZone: 播放回收特效於 {position}");
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip == null) return;

            if (Managers.AudioManager.Instance != null)
            {
                Managers.AudioManager.Instance.PlaySFX(clip);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }

        private void ShowMessage(string message)
        {
            // 顯示訊息給玩家
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage(message, 2f);
            }
            else
            {
                Debug.Log($"BottleReturnZone: {message}");
            }
        }

        private void ShowReturnPrompt(bool show)
        {
            if (show)
            {
                // 顯示提示文字
                // 注意：InteractionSystem 沒有 ShowInteractionHint 方法
                // 可以改用 UIManager 或其他方式顯示提示
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMessage(returnPrompt, 0.5f);
                }
            }
            else
            {
                // 隱藏提示
                // UIManager 的訊息會自動消失，不需要手動隱藏
            }
        }

        private void UpdateVisuals()
        {
            if (zoneMaterial != null)
            {
                zoneMaterial.color = bottlesInZone.Count > 0 ? highlightColor : zoneColor;
            }
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            // 繪製回收區域
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);

            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 選中時顯示更詳細的信息
            Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.5f);

            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }

            // 顯示重置位置
            if (bottleResetPosition != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(bottleResetPosition.position, 0.2f);
                Gizmos.DrawLine(transform.position, bottleResetPosition.position);
            }
        }

        #endregion

        #region 公開方法

        /// <summary>
        /// 獲取統計資訊
        /// </summary>
        public string GetStatistics()
        {
            return $"已回收: {totalBottlesReturned} 瓶, 總獲利: ${totalRewardEarned}";
        }

        /// <summary>
        /// 重置統計
        /// </summary>
        public void ResetStatistics()
        {
            totalBottlesReturned = 0;
            totalRewardEarned = 0;
        }

        /// <summary>
        /// 強制回收區域內所有瓶子（用於調試）
        /// </summary>
        [ContextMenu("Force Return All Bottles")]
        public void ForceReturnAll()
        {
            ReturnAllBottlesInZone();
        }

        #endregion

        #region 公開屬性

        public int TotalBottlesReturned => totalBottlesReturned;
        public int TotalRewardEarned => totalRewardEarned;
        public int BottlesInZoneCount => bottlesInZone.Count;

        #endregion
    }
}
