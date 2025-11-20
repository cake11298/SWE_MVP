using UnityEngine;
using BarSimulator.Data;
using BarSimulator.Managers;
using BarSimulator.Objects;
using BarSimulator.Systems;
using BarSimulator.Interaction;

namespace BarSimulator.NPC
{
    /// <summary>
    /// NPC 互動處理器 - 管理玩家與 NPC 之間的互動
    /// 參考: NPCManager.js 互動邏輯
    /// </summary>
    public class NPCInteraction : MonoBehaviour
    {
        #region 序列化欄位

        [Header("互動設定")]
        [Tooltip("互動距離")]
        [SerializeField] private float interactionDistance = 3f;

        [Tooltip("互動按鍵提示")]
        [SerializeField] private string interactionKey = "E";

        [Header("系統引用")]
        [Tooltip("互動系統")]
        [SerializeField] private InteractionSystem interactionSystem;

        [Tooltip("NPC 管理器")]
        [SerializeField] private NPCManager npcManager;

        #endregion

        #region 私有欄位

        // 當前可互動的 NPC
        private NPCController nearbyNPC;

        // 互動狀態
        private bool isInteracting;

        // 事件
        public System.Action<NPCController> OnNPCNearby;
        public System.Action OnNPCLeft;
        public System.Action<NPCController, string> OnDialogueStarted;
        public System.Action<NPCController, DrinkEvaluation> OnDrinkGiven;

        #endregion

        #region Unity 生命週期

        private void Start()
        {
            // 取得系統引用
            if (interactionSystem == null)
            {
                interactionSystem = InteractionSystem.Instance;
            }
            if (npcManager == null)
            {
                npcManager = NPCManager.Instance;
            }
        }

        private void Update()
        {
            CheckNearbyNPC();
            HandleInteractionInput();
        }

        #endregion

        #region NPC 檢測

        /// <summary>
        /// 檢測附近的 NPC
        /// </summary>
        private void CheckNearbyNPC()
        {
            if (npcManager == null) return;

            Vector3 playerPosition = transform.position;
            NPCController newNearbyNPC = npcManager.GetNearbyNPC(playerPosition);

            // 檢查 NPC 變化
            if (newNearbyNPC != nearbyNPC)
            {
                if (nearbyNPC != null && newNearbyNPC == null)
                {
                    // 離開 NPC 範圍
                    OnNPCLeft?.Invoke();
                }
                else if (newNearbyNPC != null)
                {
                    // 進入新 NPC 範圍
                    OnNPCNearby?.Invoke(newNearbyNPC);
                }

                nearbyNPC = newNearbyNPC;
            }
        }

        #endregion

        #region 互動處理

        /// <summary>
        /// 處理互動輸入
        /// </summary>
        private void HandleInteractionInput()
        {
            if (nearbyNPC == null) return;

            // 按 E 鍵互動
            if (Input.GetKeyDown(KeyCode.E))
            {
                // 檢查玩家是否持有酒杯
                if (interactionSystem != null && interactionSystem.IsHolding)
                {
                    var heldType = interactionSystem.GetHeldObjectType();
                    if (heldType == InteractableType.Glass)
                    {
                        // 給 NPC 飲料
                        GiveDrinkToNPC(nearbyNPC);
                        return;
                    }
                }

                // 與 NPC 對話
                TalkToNPC(nearbyNPC);
            }
        }

        /// <summary>
        /// 與 NPC 對話
        /// </summary>
        public void TalkToNPC(NPCController npc)
        {
            if (npc == null) return;

            isInteracting = true;

            // 取得對話
            string dialogue = npc.GetNextDialogue();

            // 觸發事件
            OnDialogueStarted?.Invoke(npc, dialogue);

            // 通知 NPCManager 處理對話 UI
            npcManager?.InteractWithNPC(npc);

            Debug.Log($"NPCInteraction: Talked to {npc.NPCName}: {dialogue}");
        }

        /// <summary>
        /// 給 NPC 飲料
        /// </summary>
        public void GiveDrinkToNPC(NPCController npc)
        {
            if (npc == null || interactionSystem == null) return;

            // 取得玩家持有的酒杯
            var glass = interactionSystem.HeldObject as Glass;
            if (glass == null || glass.IsEmpty)
            {
                // 顯示提示訊息
                ShowMessage("The glass is empty!");
                return;
            }

            isInteracting = true;

            // 取得飲料資訊
            DrinkInfo drinkInfo = glass.GetDrinkInfo();

            // 識別雞尾酒
            if (CocktailSystem.Instance != null)
            {
                drinkInfo.cocktailName = CocktailSystem.Instance.IdentifyCocktail(drinkInfo.ingredients);
            }

            // 評分飲料
            DrinkEvaluation evaluation = DrinkEvaluator.Evaluate(drinkInfo);

            // NPC 反應
            npc.ReactToDrink(evaluation);

            // 清空酒杯
            glass.Empty();

            // 放下酒杯
            interactionSystem.DropHeldObject();

            // 觸發事件
            OnDrinkGiven?.Invoke(npc, evaluation);

            // 通知 NPCManager 處理評分 UI
            npcManager?.NPCDrinkCocktail(npc, drinkInfo);

            Debug.Log($"NPCInteraction: Gave drink to {npc.NPCName}, Score: {evaluation.score}");
        }

        /// <summary>
        /// 顯示提示訊息
        /// </summary>
        private void ShowMessage(string message)
        {
            // 使用 DialogueBox 顯示訊息
            var dialogueBox = UI.DialogueBox.Instance;
            if (dialogueBox != null)
            {
                dialogueBox.ShowDialogue("System", message);
            }
            else
            {
                Debug.Log($"NPCInteraction: {message}");
            }
        }

        #endregion

        #region 公開方法

        /// <summary>
        /// 取得互動提示文字
        /// </summary>
        public string GetInteractionHint()
        {
            if (nearbyNPC == null) return string.Empty;

            // 檢查玩家是否持有酒杯
            if (interactionSystem != null && interactionSystem.IsHolding)
            {
                var heldType = interactionSystem.GetHeldObjectType();
                if (heldType == InteractableType.Glass)
                {
                    var glass = interactionSystem.HeldObject as Glass;
                    if (glass != null && !glass.IsEmpty)
                    {
                        return $"Press {interactionKey} to give drink to {nearbyNPC.NPCName}";
                    }
                }
            }

            return $"Press {interactionKey} to talk to {nearbyNPC.NPCName}";
        }

        /// <summary>
        /// 檢查是否有附近的 NPC
        /// </summary>
        public bool HasNearbyNPC()
        {
            return nearbyNPC != null;
        }

        /// <summary>
        /// 取得附近的 NPC
        /// </summary>
        public NPCController GetNearbyNPC()
        {
            return nearbyNPC;
        }

        /// <summary>
        /// 結束互動
        /// </summary>
        public void EndInteraction()
        {
            isInteracting = false;
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 是否正在互動
        /// </summary>
        public bool IsInteracting => isInteracting;

        /// <summary>
        /// 互動距離
        /// </summary>
        public float InteractionDistance => interactionDistance;

        #endregion
    }
}
