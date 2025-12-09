using UnityEngine;
using BarSimulator.NPC;
using BarSimulator.Managers;
using BarSimulator.Systems;

namespace BarSimulator.Tests
{
    /// <summary>
    /// NPC 互動測試器 - 用於驗證 NPC 互動功能是否正常
    /// </summary>
    public class NPCInteractionTester : MonoBehaviour
    {
        #region 序列化欄位

        [Header("測試設定")]
        [Tooltip("自動開始測試")]
        [SerializeField] private bool autoStartTest = false;

        [Tooltip("測試間隔（秒）")]
        [SerializeField] private float testInterval = 2f;

        [Header("Debug")]
        [Tooltip("顯示詳細日誌")]
        [SerializeField] private bool verboseLogging = true;

        #endregion

        #region 私有欄位

        private NPCManager npcManager;
        private InteractionSystem interactionSystem;
        private float testTimer;
        private int testStage = 0;

        #endregion

        #region Unity 生命週期

        private void Start()
        {
            npcManager = NPCManager.Instance;
            interactionSystem = InteractionSystem.Instance;

            if (autoStartTest)
            {
                StartTesting();
            }
        }

        private void Update()
        {
            if (testStage > 0)
            {
                testTimer -= Time.deltaTime;

                if (testTimer <= 0)
                {
                    RunNextTest();
                    testTimer = testInterval;
                }
            }

            // 按 T 鍵手動開始測試
            if (Input.GetKeyDown(KeyCode.T))
            {
                StartTesting();
            }

            // 按 P 鍵打印當前狀態
            if (Input.GetKeyDown(KeyCode.P))
            {
                PrintCurrentState();
            }

            // 按 O 鍵給隨機 NPC 下訂單
            if (Input.GetKeyDown(KeyCode.O))
            {
                TestRandomNPCOrder();
            }
        }

        #endregion

        #region 測試方法

        /// <summary>
        /// 開始測試
        /// </summary>
        [ContextMenu("開始測試")]
        public void StartTesting()
        {
            Debug.Log("=== NPCInteractionTester: 開始測試 NPC 互動系統 ===");
            testStage = 1;
            testTimer = 0.5f;
        }

        /// <summary>
        /// 執行下一個測試
        /// </summary>
        private void RunNextTest()
        {
            switch (testStage)
            {
                case 1:
                    TestNPCManagerExists();
                    break;
                case 2:
                    TestNPCsSpawned();
                    break;
                case 3:
                    TestNPCOrderSystem();
                    break;
                case 4:
                    TestPatienceTimer();
                    break;
                case 5:
                    TestInteractionHints();
                    break;
                case 6:
                    Debug.Log("=== NPCInteractionTester: 所有測試完成 ===");
                    testStage = 0;
                    return;
            }

            testStage++;
        }

        /// <summary>
        /// 測試 1: NPCManager 是否存在
        /// </summary>
        private void TestNPCManagerExists()
        {
            Debug.Log("--- 測試 1: 檢查 NPCManager ---");

            if (npcManager == null)
            {
                Debug.LogError("❌ NPCManager 不存在！");
                return;
            }

            Debug.Log("✓ NPCManager 存在");
            Debug.Log($"  - 互動距離: {npcManager.InteractionDistance}");
        }

        /// <summary>
        /// 測試 2: NPC 是否已生成
        /// </summary>
        private void TestNPCsSpawned()
        {
            Debug.Log("--- 測試 2: 檢查 NPC 是否已生成 ---");

            if (npcManager == null)
            {
                Debug.LogError("❌ NPCManager 不存在");
                return;
            }

            var npcs = npcManager.NPCs;

            if (npcs == null || npcs.Count == 0)
            {
                Debug.LogWarning("⚠ 場景中沒有 NPC");
                return;
            }

            Debug.Log($"✓ 找到 {npcs.Count} 個 NPC:");

            foreach (var npc in npcs)
            {
                if (npc != null)
                {
                    Debug.Log($"  - {npc.NPCName} ({npc.Role}) at {npc.transform.position}");
                }
            }
        }

        /// <summary>
        /// 測試 3: NPC 訂單系統
        /// </summary>
        private void TestNPCOrderSystem()
        {
            Debug.Log("--- 測試 3: 測試 NPC 訂單系統 ---");

            if (npcManager == null || npcManager.NPCs.Count == 0)
            {
                Debug.LogWarning("⚠ 無法測試：沒有 NPC");
                return;
            }

            // 給第一個 NPC 下訂單
            var npc = npcManager.NPCs[0];

            if (npc != null)
            {
                if (npc.HasActiveOrder)
                {
                    Debug.Log($"✓ {npc.NPCName} 已經有訂單: {npc.CurrentOrder.drinkName}");
                    Debug.Log($"  - 耐心值: {npc.PatienceRatio * 100:F0}%");
                    Debug.Log($"  - 剩餘時間: {npc.PatienceRemaining:F1}s");
                }
                else
                {
                    Debug.Log($"  給 {npc.NPCName} 下訂單...");
                    npc.StartOrderingProcess();

                    if (npc.HasActiveOrder)
                    {
                        Debug.Log($"✓ 訂單已創建: {npc.CurrentOrder.drinkName}");
                        Debug.Log($"  - 要求: {npc.CurrentOrder.GetRequirementsText()}");
                        Debug.Log($"  - 耐心時間: {npc.CurrentOrder.patienceTime}s");
                    }
                    else
                    {
                        Debug.LogError("❌ 訂單創建失敗");
                    }
                }
            }
        }

        /// <summary>
        /// 測試 4: 耐心條計時器
        /// </summary>
        private void TestPatienceTimer()
        {
            Debug.Log("--- 測試 4: 測試耐心條計時器 ---");

            if (npcManager == null || npcManager.NPCs.Count == 0)
            {
                Debug.LogWarning("⚠ 無法測試：沒有 NPC");
                return;
            }

            bool foundActiveOrder = false;

            foreach (var npc in npcManager.NPCs)
            {
                if (npc != null && npc.HasActiveOrder)
                {
                    foundActiveOrder = true;
                    float patience = npc.PatienceRatio;
                    float timeLeft = npc.PatienceRemaining;

                    Debug.Log($"✓ {npc.NPCName} 的訂單狀態:");
                    Debug.Log($"  - 飲料: {npc.CurrentOrder.drinkName}");
                    Debug.Log($"  - 耐心值: {patience * 100:F1}%");
                    Debug.Log($"  - 剩餘時間: {timeLeft:F1}s");
                    Debug.Log($"  - 情緒: {npc.CurrentMood}");

                    // 檢查是否正在減少
                    if (patience < 1.0f)
                    {
                        Debug.Log("  ✓ 耐心值正在減少（正常）");
                    }
                    else
                    {
                        Debug.LogWarning("  ⚠ 耐心值還沒開始減少");
                    }

                    // 檢查是否超時
                    if (npc.OrderTimedOut)
                    {
                        Debug.Log("  ! 訂單已超時");
                    }
                }
            }

            if (!foundActiveOrder)
            {
                Debug.LogWarning("⚠ 沒有 NPC 有活動訂單");
                Debug.Log("  提示：按 O 鍵給隨機 NPC 下訂單");
            }
        }

        /// <summary>
        /// 測試 5: 互動提示
        /// </summary>
        private void TestInteractionHints()
        {
            Debug.Log("--- 測試 5: 測試互動提示系統 ---");

            if (interactionSystem == null)
            {
                Debug.LogError("❌ InteractionSystem 不存在");
                return;
            }

            Debug.Log("✓ InteractionSystem 存在");

            string hint = interactionSystem.GetInteractionHint();

            if (string.IsNullOrEmpty(hint))
            {
                Debug.Log("  - 當前沒有互動提示（正常，取決於玩家位置和狀態）");
            }
            else
            {
                Debug.Log($"  - 當前提示: \"{hint}\"");
            }

            // 測試是否持有物品
            if (interactionSystem.IsHolding)
            {
                Debug.Log($"  - 正在持有: {interactionSystem.HeldObject.DisplayName}");
                Debug.Log($"  - 物品類型: {interactionSystem.GetHeldObjectType()}");
            }
            else
            {
                Debug.Log("  - 未持有任何物品");
            }
        }

        /// <summary>
        /// 打印當前狀態
        /// </summary>
        [ContextMenu("打印當前狀態")]
        public void PrintCurrentState()
        {
            Debug.Log("=== 當前系統狀態 ===");

            // NPC 狀態
            if (npcManager != null && npcManager.NPCs.Count > 0)
            {
                Debug.Log($"NPC 數量: {npcManager.NPCs.Count}");

                foreach (var npc in npcManager.NPCs)
                {
                    if (npc == null) continue;

                    string status = npc.HasActiveOrder
                        ? $"訂單: {npc.CurrentOrder.drinkName}, 耐心: {npc.PatienceRatio * 100:F0}%"
                        : "無訂單";

                    Debug.Log($"  {npc.NPCName}: {status}");
                }
            }
            else
            {
                Debug.Log("沒有 NPC");
            }

            // 互動系統狀態
            if (interactionSystem != null)
            {
                Debug.Log($"玩家持有物品: {(interactionSystem.IsHolding ? interactionSystem.HeldObject.DisplayName : "無")}");
                Debug.Log($"互動提示: {interactionSystem.GetInteractionHint()}");
            }
        }

        /// <summary>
        /// 給隨機 NPC 下訂單
        /// </summary>
        [ContextMenu("給隨機 NPC 下訂單")]
        public void TestRandomNPCOrder()
        {
            if (npcManager == null || npcManager.NPCs.Count == 0)
            {
                Debug.LogWarning("沒有可用的 NPC");
                return;
            }

            // 找一個沒有訂單的 NPC
            foreach (var npc in npcManager.NPCs)
            {
                if (npc != null && !npc.HasActiveOrder)
                {
                    npc.StartOrderingProcess();
                    Debug.Log($"✓ 給 {npc.NPCName} 下了訂單: {npc.CurrentOrder.drinkName}");
                    return;
                }
            }

            Debug.Log("所有 NPC 都已經有訂單了");
        }

        /// <summary>
        /// 測試耐心條超時
        /// </summary>
        [ContextMenu("測試耐心條超時（縮短時間）")]
        public void TestPatienceTimeout()
        {
            if (npcManager == null || npcManager.NPCs.Count == 0)
            {
                Debug.LogWarning("沒有可用的 NPC");
                return;
            }

            // 給第一個沒有訂單的 NPC 創建一個短時間訂單
            foreach (var npc in npcManager.NPCs)
            {
                if (npc != null && !npc.HasActiveOrder)
                {
                    var order = npc.GenerateRandomOrder();
                    order.patienceTime = 5f; // 只有 5 秒耐心
                    npc.PlaceOrder(order);

                    Debug.Log($"✓ 給 {npc.NPCName} 下了 5 秒快速訂單: {order.drinkName}");
                    Debug.Log("  5 秒後訂單會超時，耐心條應該消失");
                    return;
                }
            }

            Debug.Log("所有 NPC 都已經有訂單了");
        }

        #endregion

        #region 輔助方法

        private void OnGUI()
        {
            if (!verboseLogging) return;

            // 在螢幕上顯示快捷鍵提示
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.yellow;

            string helpText = "NPC 互動測試快捷鍵:\n" +
                              "T - 開始自動測試\n" +
                              "P - 打印當前狀態\n" +
                              "O - 給隨機 NPC 下訂單";

            GUI.Label(new Rect(10, 10, 300, 100), helpText, style);
        }

        #endregion
    }
}
