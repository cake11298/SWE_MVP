using UnityEngine;
using BarSimulator.Data;
using BarSimulator.Objects;
using BarSimulator.UI;

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

        // 點單系統
        private DrinkOrder currentOrder;
        private bool hasActiveOrder;
        private float orderStartTime;
        private float patienceRemaining;
        private bool orderTimedOut;

        // 事件
        public System.Action<NPCController, DrinkOrder> OnOrderPlaced;
        public System.Action<NPCController> OnOrderTimedOut;
        public System.Action<NPCController, float> OnPatienceChanged;

        #endregion

        #region Unity 生命週期

        private void Start()
        {
            basePosition = transform.position;
        }

        #endregion

        #region 私有欄位 - 直接資料

        // 直接儲存的資料（不使用 ScriptableObject 時）
        private string directName;
        private string directRole;
        private string[] directDialogues;

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

        /// <summary>
        /// 直接使用參數初始化（不需要 ScriptableObject）
        /// </summary>
        public void InitializeNPC(string npcName, string role, string[] dialogues)
        {
            directName = npcName;
            directRole = role;
            directDialogues = dialogues;
            currentDialogueIndex = 0;
            currentMood = NPCMood.Neutral;

            // 設定物件名稱
            gameObject.name = $"NPC_{npcName}";

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

            // 更新點單計時器
            UpdateOrderTimer(deltaTime);
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

        /// <summary>
        /// 更新點單計時器
        /// </summary>
        private void UpdateOrderTimer(float deltaTime)
        {
            if (!hasActiveOrder || orderTimedOut) return;

            patienceRemaining -= deltaTime;
            float patienceRatio = patienceRemaining / currentOrder.patienceTime;

            // 觸發耐心值變化事件
            OnPatienceChanged?.Invoke(this, patienceRatio);

            // 根據耐心值改變情緒
            if (patienceRatio < 0.25f)
            {
                SetMood(NPCMood.Angry);
            }
            else if (patienceRatio < 0.5f)
            {
                SetMood(NPCMood.Disappointed);
            }

            // 超時處理
            if (patienceRemaining <= 0)
            {
                orderTimedOut = true;
                hasActiveOrder = false;
                SetMood(NPCMood.Angry);
                OnOrderTimedOut?.Invoke(this);
                Debug.Log($"{NPCName} order timed out! They left disappointed.");
            }
        }

        #endregion

        #region 點單系統

        /// <summary>
        /// 放置新訂單
        /// </summary>
        public void PlaceOrder(DrinkOrder order)
        {
            currentOrder = order;
            hasActiveOrder = true;
            orderStartTime = Time.time;
            patienceRemaining = order.patienceTime;
            orderTimedOut = false;

            SetMood(NPCMood.Neutral);
            OnOrderPlaced?.Invoke(this, order);

            Debug.Log($"{NPCName} ordered: {order.drinkName} with requirements: {order.GetRequirementsText()}");
        }

        /// <summary>
        /// 生成隨機訂單
        /// </summary>
        public DrinkOrder GenerateRandomOrder()
        {
            string[] drinks = {
                "Martini", "Negroni", "Margarita", "Daiquiri", "Old Fashioned",
                "Manhattan", "Mojito", "Whiskey Sour", "Cosmopolitan"
            };

            string drinkName = drinks[Random.Range(0, drinks.Length)];
            DrinkOrder order = new DrinkOrder(drinkName);

            // 隨機添加特殊要求
            if (Random.value > 0.6f)
            {
                order.AddRequirement(DrinkRequirement.ExtraIce);
            }
            if (Random.value > 0.7f)
            {
                order.AddRequirement(DrinkRequirement.LessSugar);
            }
            if (Random.value > 0.75f)
            {
                order.AddRequirement(DrinkRequirement.MoreSour);
            }
            if (Random.value > 0.8f)
            {
                order.AddRequirement(DrinkRequirement.Strong);
            }

            return order;
        }

        /// <summary>
        /// 開始點單流程（詢問 NPC）
        /// </summary>
        public void StartOrderingProcess()
        {
            if (hasActiveOrder) return;

            DrinkOrder order = GenerateRandomOrder();
            PlaceOrder(order);
        }

        /// <summary>
        /// 完成訂單（送酒給 NPC）
        /// </summary>
        public void CompleteOrder()
        {
            if (!hasActiveOrder) return;

            hasActiveOrder = false;
            float timeUsed = Time.time - orderStartTime;
            float patienceRatio = patienceRemaining / currentOrder.patienceTime;

            Debug.Log($"{NPCName} order completed! Time used: {timeUsed:F1}s, Patience remaining: {patienceRatio * 100:F0}%");
        }

        /// <summary>
        /// 接受飲料送達
        /// </summary>
        /// <param name="container">送來的容器（Glass 或 Shaker）</param>
        /// <returns>是否接受飲料</returns>
        public bool ServeDrink(Container container)
        {
            if (!hasActiveOrder || container == null)
            {
                return false;
            }

            // 取得容器中的飲料資訊
            DrinkInfo drinkInfo = container.GetDrinkInfo();

            // 完成訂單
            CompleteOrder();

            // 評分並反應
            if (drinkInfo != null)
            {
                var evaluation = DrinkEvaluator.Evaluate(drinkInfo);
                ReactToDrink(evaluation);

                // 顯示評價
                var dialogueBox = UI.DialogueBox.Instance;
                if (dialogueBox != null)
                {
                    string response = GetDrinkResponse(evaluation);
                    dialogueBox.ShowDialogue(NPCName, response);
                }

                Debug.Log($"{NPCName} received drink: {drinkInfo.cocktailName}, Score: {evaluation.score}");
            }
            else
            {
                SetMood(NPCMood.Disappointed);
                Debug.Log($"{NPCName} received an empty container.");
            }

            // 銷毀容器
            Destroy(container.gameObject);

            return true;
        }

        /// <summary>
        /// 取得對飲料的評價回應
        /// </summary>
        private string GetDrinkResponse(DrinkEvaluation evaluation)
        {
            if (evaluation.score >= 90)
            {
                return $"Excellent! This {evaluation.cocktailName} is perfect! Score: {evaluation.score}/100";
            }
            else if (evaluation.score >= 70)
            {
                return $"Good job! This {evaluation.cocktailName} is pretty good. Score: {evaluation.score}/100";
            }
            else if (evaluation.score >= 50)
            {
                return $"Not bad, but this {evaluation.cocktailName} could be better. Score: {evaluation.score}/100";
            }
            else
            {
                return $"Hmm... This doesn't quite taste right. Score: {evaluation.score}/100";
            }
        }

        /// <summary>
        /// 取消訂單
        /// </summary>
        public void CancelOrder()
        {
            if (!hasActiveOrder) return;

            hasActiveOrder = false;
            currentOrder = null;
            SetMood(NPCMood.Disappointed);

            Debug.Log($"{NPCName}'s order was cancelled.");
        }

        /// <summary>
        /// 獲取訂單描述文本
        /// </summary>
        public string GetOrderDescription()
        {
            if (!hasActiveOrder || currentOrder == null)
            {
                return "I'd like to order something...";
            }

            string desc = $"I'll have a {currentOrder.drinkName}";
            string requirements = currentOrder.GetRequirementsText();

            if (!string.IsNullOrEmpty(requirements))
            {
                desc += $", {requirements}";
            }

            desc += " please!";
            return desc;
        }

        #endregion

        #region 對話

        /// <summary>
        /// 取得下一句對話
        /// </summary>
        public string GetNextDialogue()
        {
            // 優先使用直接資料
            var dialogues = directDialogues ?? npcData?.dialogues;

            if (dialogues == null || dialogues.Length == 0)
            {
                return "...";
            }

            string dialogue = dialogues[currentDialogueIndex];

            // 循環對話
            currentDialogueIndex = (currentDialogueIndex + 1) % dialogues.Length;

            return dialogue;
        }

        /// <summary>
        /// 取得特定索引的對話
        /// </summary>
        public string GetDialogue(int index)
        {
            // 優先使用直接資料
            var dialogues = directDialogues ?? npcData?.dialogues;

            if (dialogues == null)
                return "...";

            if (index < 0 || index >= dialogues.Length)
                return "...";

            return dialogues[index];
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
        public string NPCName => directName ?? npcData?.npcName ?? "Unknown";

        /// <summary>
        /// NPC 角色/職位
        /// </summary>
        public string Role => directRole ?? npcData?.role ?? "";

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

        /// <summary>
        /// 當前訂單
        /// </summary>
        public DrinkOrder CurrentOrder => currentOrder;

        /// <summary>
        /// 是否有活動訂單
        /// </summary>
        public bool HasActiveOrder => hasActiveOrder;

        /// <summary>
        /// 是否有待處理的訂單（HasActiveOrder 的別名）
        /// </summary>
        public bool HasPendingOrder => hasActiveOrder;

        /// <summary>
        /// 剩餘耐心值（0-1）
        /// </summary>
        public float PatienceRatio => hasActiveOrder && currentOrder != null ?
            patienceRemaining / currentOrder.patienceTime : 1f;

        /// <summary>
        /// 剩餘耐心時間（秒）
        /// </summary>
        public float PatienceRemaining => patienceRemaining;

        /// <summary>
        /// 訂單是否超時
        /// </summary>
        public bool OrderTimedOut => orderTimedOut;

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

    /// <summary>
    /// 特殊要求類型
    /// </summary>
    public enum DrinkRequirement
    {
        None,
        ExtraIce,
        NoIce,
        LessSugar,
        MoreSugar,
        MoreSour,
        LessSour,
        Strong,
        Light,
        Neat,
        OnTheRocks
    }

    /// <summary>
    /// 飲料訂單
    /// </summary>
    [System.Serializable]
    public class DrinkOrder
    {
        public string drinkName;
        public float patienceTime;
        public System.Collections.Generic.List<DrinkRequirement> requirements;

        public DrinkOrder(string name, float patience = 120f)
        {
            drinkName = name;
            patienceTime = patience;
            requirements = new System.Collections.Generic.List<DrinkRequirement>();
        }

        public void AddRequirement(DrinkRequirement req)
        {
            if (!requirements.Contains(req))
            {
                requirements.Add(req);
            }
        }

        public bool HasRequirement(DrinkRequirement req)
        {
            return requirements.Contains(req);
        }

        public string GetRequirementsText()
        {
            if (requirements.Count == 0) return "";

            var texts = new System.Collections.Generic.List<string>();
            foreach (var req in requirements)
            {
                switch (req)
                {
                    case DrinkRequirement.ExtraIce: texts.Add("extra ice"); break;
                    case DrinkRequirement.NoIce: texts.Add("no ice"); break;
                    case DrinkRequirement.LessSugar: texts.Add("less sugar"); break;
                    case DrinkRequirement.MoreSugar: texts.Add("more sugar"); break;
                    case DrinkRequirement.MoreSour: texts.Add("more sour"); break;
                    case DrinkRequirement.LessSour: texts.Add("less sour"); break;
                    case DrinkRequirement.Strong: texts.Add("strong"); break;
                    case DrinkRequirement.Light: texts.Add("light"); break;
                    case DrinkRequirement.Neat: texts.Add("neat"); break;
                    case DrinkRequirement.OnTheRocks: texts.Add("on the rocks"); break;
                }
            }

            return string.Join(", ", texts);
        }
    }
}
