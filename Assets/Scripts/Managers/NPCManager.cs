using System.Collections.Generic;
using UnityEngine;
using BarSimulator.Data;
using BarSimulator.NPC;
using BarSimulator.Objects;

namespace BarSimulator.Managers
{
    /// <summary>
    /// NPC 管理器 - 管理所有 NPC 的生成、更新和互動
    /// 參考: src/modules/NPCManager.js
    /// </summary>
    public class NPCManager : MonoBehaviour
    {
        #region 單例

        private static NPCManager instance;
        public static NPCManager Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("NPC 資料")]
        [Tooltip("NPC 資料庫")]
        [SerializeField] private NPCDatabase npcDatabase;

        [Header("預製物")]
        [Tooltip("NPC 預製物")]
        [SerializeField] private GameObject npcPrefab;

        [Header("互動設定")]
        [Tooltip("NPC 互動距離")]
        [SerializeField] private float interactionDistance = 3f;

        [Header("音效")]
        [Tooltip("背景音樂 AudioSource")]
        [SerializeField] private AudioSource musicSource;

        #endregion

        #region 私有欄位

        // 所有生成的 NPC
        private List<NPCController> npcs = new List<NPCController>();

        // 音樂狀態
        private bool musicPlaying;

        // 事件
        public System.Action<NPCController, DrinkInfo> OnNPCDrink;
        public System.Action<NPCController, string> OnNPCDialogue;

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
        }

        private void Start()
        {
            // 載入資料庫
            if (npcDatabase == null)
            {
                npcDatabase = Resources.Load<NPCDatabase>("NPCDatabase");
            }

            // 生成所有 NPC
            SpawnAllNPCs();
        }

        private void Update()
        {
            // 更新所有 NPC
            foreach (var npc in npcs)
            {
                npc?.UpdateNPC(Time.deltaTime);
            }
        }

        #endregion

        #region NPC 生成

        /// <summary>
        /// 生成所有 NPC
        /// </summary>
        private void SpawnAllNPCs()
        {
            if (npcDatabase == null || npcDatabase.npcs == null) return;

            foreach (var npcData in npcDatabase.npcs)
            {
                SpawnNPC(npcData);
            }

            Debug.Log($"NPCManager: Spawned {npcs.Count} NPCs");
        }

        /// <summary>
        /// 生成單一 NPC
        /// </summary>
        public NPCController SpawnNPC(NPCData data)
        {
            if (data == null) return null;

            GameObject npcObject;

            if (npcPrefab != null)
            {
                npcObject = Instantiate(npcPrefab, data.position, data.RotationQuaternion);
            }
            else
            {
                // 沒有預製物時建立簡單的 NPC
                npcObject = CreateDefaultNPC(data);
            }

            // 設定 NPC 控制器
            var controller = npcObject.GetComponent<NPCController>();
            if (controller == null)
            {
                controller = npcObject.AddComponent<NPCController>();
            }

            controller.Initialize(data);
            npcs.Add(controller);

            return controller;
        }

        /// <summary>
        /// 建立預設 NPC 物件（沒有預製物時使用）
        /// </summary>
        private GameObject CreateDefaultNPC(NPCData data)
        {
            // 建立根物件
            var npc = new GameObject($"NPC_{data.npcName}");
            npc.transform.position = data.position;
            npc.transform.rotation = data.RotationQuaternion;

            // 建立身體（膠囊）
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(npc.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // 設定身體顏色
            var bodyRenderer = body.GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = data.shirtColor;
            }

            // 建立頭部（球）
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(npc.transform);
            head.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            head.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            // 添加碰撞器（用於互動檢測）
            var collider = npc.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 1f, 0f);
            collider.height = 2f;
            collider.radius = 0.3f;

            return npc;
        }

        #endregion

        #region NPC 互動

        /// <summary>
        /// 檢查玩家附近是否有可互動的 NPC
        /// 參考: NPCManager.js checkInteractions()
        /// </summary>
        public NPCController GetNearbyNPC(Vector3 playerPosition)
        {
            NPCController nearestNPC = null;
            float nearestDistance = interactionDistance;

            foreach (var npc in npcs)
            {
                if (npc == null) continue;

                float distance = Vector3.Distance(playerPosition, npc.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestNPC = npc;
                }
            }

            return nearestNPC;
        }

        /// <summary>
        /// 與 NPC 互動（對話）
        /// </summary>
        public void InteractWithNPC(NPCController npc)
        {
            if (npc == null) return;

            string dialogue = npc.GetNextDialogue();
            OnNPCDialogue?.Invoke(npc, dialogue);

            // 顯示對話框
            var dialogueBox = UI.DialogueBox.Instance;
            if (dialogueBox != null)
            {
                dialogueBox.ShowDialogue(npc.NPCName, dialogue);
            }
        }

        /// <summary>
        /// NPC 喝酒並評分
        /// 參考: NPCManager.js npcDrinkCocktail()
        /// </summary>
        public void NPCDrinkCocktail(NPCController npc, DrinkInfo drinkInfo)
        {
            if (npc == null || drinkInfo == null) return;

            // 評分飲料
            var evaluation = DrinkEvaluator.Evaluate(drinkInfo);

            // NPC 反應
            npc.ReactToDrink(evaluation);

            // 觸發事件
            OnNPCDrink?.Invoke(npc, drinkInfo);

            // 顯示評價對話
            var dialogueBox = UI.DialogueBox.Instance;
            if (dialogueBox != null)
            {
                string response = GetDrinkResponse(npc, evaluation);
                dialogueBox.ShowDialogue(npc.NPCName, response);
            }

            Debug.Log($"NPCManager: {npc.NPCName} drank {drinkInfo.cocktailName}, Score: {evaluation.score}");
        }

        /// <summary>
        /// 取得 NPC 對飲料的評價回應
        /// </summary>
        private string GetDrinkResponse(NPCController npc, DrinkEvaluation evaluation)
        {
            // 根據評分回傳不同的英文回應
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

        #endregion

        #region 音樂控制

        /// <summary>
        /// 播放/停止音樂
        /// </summary>
        public void ToggleMusic()
        {
            if (musicSource == null) return;

            if (musicPlaying)
            {
                musicSource.Stop();
                musicPlaying = false;
            }
            else
            {
                musicSource.Play();
                musicPlaying = true;
            }
        }

        /// <summary>
        /// 播放吉他音效（用於吉他互動）
        /// </summary>
        public void PlayGuitarSound()
        {
            // 可以在這裡播放吉他音效
            Debug.Log("NPCManager: Guitar sound played");
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 所有 NPC 列表
        /// </summary>
        public IReadOnlyList<NPCController> NPCs => npcs.AsReadOnly();

        /// <summary>
        /// 音樂是否正在播放
        /// </summary>
        public bool MusicPlaying => musicPlaying;

        /// <summary>
        /// NPC 互動距離
        /// </summary>
        public float InteractionDistance => interactionDistance;

        #endregion
    }
}
