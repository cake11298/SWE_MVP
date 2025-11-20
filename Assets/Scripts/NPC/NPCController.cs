using UnityEngine;
using BarSimulator.Data;

namespace BarSimulator.NPC
{
    /// <summary>
    /// NPC 控制器 - 控制單一 NPC 的行為和狀態
    /// 參考: NPCManager.js NPC 資料結構和行為
    /// </summary>
    public class NPCController : MonoBehaviour
    {
        #region 序列化欄位

        [Header("NPC 資料")]
        [Tooltip("NPC ScriptableObject 資料")]
        [SerializeField] private NPCData npcData;

        [Header("動畫")]
        [Tooltip("閒置時的輕微擺動")]
        [SerializeField] private bool enableIdleAnimation = true;

        [Tooltip("擺動幅度")]
        [SerializeField] private float idleSwayAmount = 0.02f;

        [Tooltip("擺動速度")]
        [SerializeField] private float idleSwaySpeed = 1f;

        #endregion

        #region 私有欄位

        // 對話索引
        private int currentDialogueIndex;

        // 動畫狀態
        private Vector3 basePosition;
        private float animationTime;

        // 情緒狀態
        private NPCMood currentMood = NPCMood.Neutral;

        #endregion

        #region Unity 生命週期

        private void Start()
        {
            basePosition = transform.position;
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 使用 NPCData 初始化
        /// </summary>
        public void Initialize(NPCData data)
        {
            npcData = data;
            currentDialogueIndex = 0;
            currentMood = NPCMood.Neutral;

            // 設定名稱
            gameObject.name = $"NPC_{data.npcName}";

            // 儲存基礎位置
            basePosition = transform.position;
        }

        #endregion

        #region 更新

        /// <summary>
        /// 更新 NPC（由 NPCManager 呼叫）
        /// </summary>
        public void UpdateNPC(float deltaTime)
        {
            if (enableIdleAnimation)
            {
                UpdateIdleAnimation(deltaTime);
            }
        }

        /// <summary>
        /// 更新閒置動畫
        /// </summary>
        private void UpdateIdleAnimation(float deltaTime)
        {
            animationTime += deltaTime * idleSwaySpeed;

            // 輕微左右擺動
            float swayX = Mathf.Sin(animationTime) * idleSwayAmount;
            float swayZ = Mathf.Cos(animationTime * 0.7f) * idleSwayAmount * 0.5f;

            transform.position = basePosition + new Vector3(swayX, 0f, swayZ);
        }

        #endregion

        #region 對話

        /// <summary>
        /// 取得下一句對話
        /// </summary>
        public string GetNextDialogue()
        {
            if (npcData == null || npcData.dialogues == null || npcData.dialogues.Length == 0)
            {
                return "...";
            }

            string dialogue = npcData.dialogues[currentDialogueIndex];

            // 循環對話
            currentDialogueIndex = (currentDialogueIndex + 1) % npcData.dialogues.Length;

            return dialogue;
        }

        /// <summary>
        /// 取得特定索引的對話
        /// </summary>
        public string GetDialogue(int index)
        {
            if (npcData == null || npcData.dialogues == null)
                return "...";

            if (index < 0 || index >= npcData.dialogues.Length)
                return "...";

            return npcData.dialogues[index];
        }

        /// <summary>
        /// 重置對話索引
        /// </summary>
        public void ResetDialogue()
        {
            currentDialogueIndex = 0;
        }

        #endregion

        #region 飲料反應

        /// <summary>
        /// 對飲料評價做出反應
        /// </summary>
        public void ReactToDrink(DrinkEvaluation evaluation)
        {
            // 根據評分設定情緒
            if (evaluation.score >= 80)
            {
                SetMood(NPCMood.Happy);
            }
            else if (evaluation.score >= 50)
            {
                SetMood(NPCMood.Neutral);
            }
            else
            {
                SetMood(NPCMood.Disappointed);
            }

            // 可以在這裡播放動畫或音效
            Debug.Log($"{NPCName} reacted to drink with mood: {currentMood}");
        }

        /// <summary>
        /// 設定 NPC 情緒
        /// </summary>
        public void SetMood(NPCMood mood)
        {
            currentMood = mood;

            // 可以在這裡觸發表情動畫
            // 例如改變材質顏色、播放動畫等
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// NPC 名稱
        /// </summary>
        public string NPCName => npcData?.npcName ?? "Unknown";

        /// <summary>
        /// NPC 角色/職位
        /// </summary>
        public string Role => npcData?.role ?? "";

        /// <summary>
        /// NPC 性別
        /// </summary>
        public Gender NPCGender => npcData?.gender ?? Gender.Male;

        /// <summary>
        /// 當前情緒
        /// </summary>
        public NPCMood CurrentMood => currentMood;

        /// <summary>
        /// NPC 資料
        /// </summary>
        public NPCData Data => npcData;

        #endregion
    }

    /// <summary>
    /// NPC 情緒狀態
    /// </summary>
    public enum NPCMood
    {
        Neutral,
        Happy,
        Excited,
        Disappointed,
        Angry
    }
}
