using UnityEngine;
using BarSimulator.Interaction;
using BarSimulator.Objects;

namespace BarSimulator.NPC
{
    /// <summary>
    /// 可互動 NPC - 允許玩家與 NPC 對話和給予飲料
    /// </summary>
    public class InteractableNPC : InteractableBase
    {
        #region 私有欄位

        private NPCController npcController;

        // 對話顯示
        private bool isShowingDialogue;
        private string currentDialogue;
        private float dialogueTimer;
        private const float DialogueDuration = 4f;

        // 事件
        public System.Action<string, string> OnDialogueShown; // NPC名稱, 對話內容

        #endregion

        #region Unity 生命週期

        protected override void Awake()
        {
            base.Awake();

            interactableType = InteractableType.NPC;
            canPickup = false; // NPC 不能被拾取

            npcController = GetComponent<NPCController>();

            if (npcController != null)
            {
                displayName = npcController.NPCName;
            }
        }

        private void Update()
        {
            // 對話計時器
            if (isShowingDialogue)
            {
                dialogueTimer -= Time.deltaTime;
                if (dialogueTimer <= 0f)
                {
                    isShowingDialogue = false;
                }
            }
        }

        #endregion

        #region IInteractable 覆寫

        public override void OnInteract()
        {
            base.OnInteract();

            if (npcController == null) return;

            // 取得下一句對話
            currentDialogue = npcController.GetNextDialogue();
            isShowingDialogue = true;
            dialogueTimer = DialogueDuration;

            // 觸發事件
            OnDialogueShown?.Invoke(npcController.NPCName, currentDialogue);

            Debug.Log($"{npcController.NPCName}: {currentDialogue}");

            // 顯示對話 UI
            ShowDialogueUI();
        }

        public override void OnTargeted()
        {
            base.OnTargeted();
            // 可以添加高亮效果
        }

        public override void OnUntargeted()
        {
            base.OnUntargeted();
            // 取消高亮效果
        }

        #endregion

        #region 對話 UI

        /// <summary>
        /// 顯示對話 UI
        /// </summary>
        private void ShowDialogueUI()
        {
            // 尋找 UIBuilder 顯示對話
            var uiBuilder = UI.UIBuilder.Instance;
            if (uiBuilder != null)
            {
                uiBuilder.ShowMessage($"{npcController.NPCName}: {currentDialogue}");
            }
        }

        #endregion

        #region 給予飲料

        /// <summary>
        /// 給 NPC 飲料
        /// </summary>
        public void GiveDrink(Glass glass)
        {
            if (glass == null || glass.IsEmpty) return;
            if (npcController == null) return;

            // 取得飲料資訊
            var drinkInfo = glass.GetDrinkInfo();

            // 評估飲料
            var evaluation = EvaluateDrink(drinkInfo);

            // NPC 反應
            npcController.ReactToDrink(evaluation);

            // 顯示反應對話
            string reaction = GetReactionDialogue(evaluation);
            currentDialogue = reaction;
            isShowingDialogue = true;
            dialogueTimer = DialogueDuration;

            Debug.Log($"{npcController.NPCName} received drink: {drinkInfo.cocktailName} - {reaction}");

            ShowDialogueUI();

            // 清空杯子
            glass.Empty();
        }

        /// <summary>
        /// 評估飲料
        /// </summary>
        private DrinkEvaluation EvaluateDrink(DrinkInfo drinkInfo)
        {
            var evaluation = new DrinkEvaluation
            {
                cocktailName = drinkInfo.cocktailName,
                volume = drinkInfo.volume,
                isValidCocktail = true
            };

            // 簡單評分邏輯
            if (drinkInfo.cocktailName.Contains("Martini") ||
                drinkInfo.cocktailName.Contains("Negroni") ||
                drinkInfo.cocktailName.Contains("Margarita"))
            {
                evaluation.score = 85;
                evaluation.message = "Perfect!";
            }
            else if (drinkInfo.cocktailName.Contains("Custom") ||
                     drinkInfo.cocktailName.Contains("Mix"))
            {
                evaluation.score = 60;
                evaluation.message = "Interesting...";
            }
            else
            {
                evaluation.score = 70;
                evaluation.message = "Not bad!";
            }

            return evaluation;
        }

        /// <summary>
        /// 根據評分取得反應對話
        /// </summary>
        private string GetReactionDialogue(DrinkEvaluation evaluation)
        {
            if (evaluation.score >= 80)
            {
                return $"Wow! This {evaluation.cocktailName} is amazing! {evaluation.message}";
            }
            else if (evaluation.score >= 60)
            {
                return $"Thanks for the {evaluation.cocktailName}. {evaluation.message}";
            }
            else
            {
                return $"Hmm... this {evaluation.cocktailName} could use some work.";
            }
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// NPC 控制器
        /// </summary>
        public NPCController NPCController => npcController;

        /// <summary>
        /// 是否正在顯示對話
        /// </summary>
        public bool IsShowingDialogue => isShowingDialogue;

        /// <summary>
        /// 當前對話內容
        /// </summary>
        public string CurrentDialogue => currentDialogue;

        #endregion
    }
}
