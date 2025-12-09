# GameScene 改進和 NPC 互動修復

## 概述
此次更新主要解決了以下問題：
1. GameScene 中的紫色材質問題
2. NPC 互動系統的改進
3. 玩家給 NPC 送酒的功能完善

## 主要改動

### 1. InteractionSystem 改進 (Assets/Scripts/Systems/InteractionSystem.cs)

#### 新增功能：給 NPC 送酒的 UI 提示
- 在 `GetInteractionHint()` 方法中添加了檢測附近 NPC 的邏輯
- 新增 `GetNPCServingHint()` 私有方法，用於生成給 NPC 送酒的提示
- 當玩家持有裝滿飲料的容器並靠近有訂單的 NPC（3米內）時，會顯示 "Press F to serve {NPC名稱}" 的提示

#### 實現細節
```csharp
// 檢查附近是否有需要服務的 NPC
private string GetNPCServingHint()
{
    // 檢查是否持有容器且容器不為空
    var container = heldObject as Container;
    if (container == null || container.IsEmpty) return string.Empty;

    // 在3米範圍內尋找有訂單的 NPC
    foreach (var npc in npcManager.GetActiveNPCs())
    {
        if (npc.HasPendingOrder && distance < 3f)
        {
            return $"Press F to serve {npc.NPCName}";
        }
    }
}
```

### 2. 場景材質修復系統

#### 新增文件：SceneMaterialFixer.cs (Assets/Scripts/Core/SceneMaterialFixer.cs)

**功能：**
- 自動檢測並修復場景中缺失或紫色的材質
- 根據物件名稱智能判斷並應用合適的材質
- 優化場景光照設置

**主要方法：**
- `FixAllMaterials()` - 掃描並修復所有有問題的材質
- `NeedsMaterialFix(Renderer)` - 判斷是否需要修復
- `CreateMaterialForObject()` - 根據物件類型創建適合的材質
- `OptimizeLighting()` - 優化場景光照

**支援的物件類型：**
- 地板 (floor, ground, plane) → 深色木質材質
- 牆壁 (wall) → 淺色材質
- 吧台 (counter, bar, table) → 拋光深色木質
- NPC → 身體藍色，頭部膚色
- 玻璃/杯子 → 透明玻璃材質
- 瓶子 → 有色玻璃
- 金屬 → 鍍鉻材質
- 座椅 → 皮革材質

**使用方式：**
1. 在遊戲開始時自動執行（通過 GameSceneInitializer）
2. 在 Unity 編輯器中右鍵點擊組件 → "修復所有材質"

### 3. GameSceneInitializer 改進 (Assets/Scripts/Core/GameSceneInitializer.cs)

#### 新增功能
- 在場景初始化流程中添加了材質修復步驟
- 新增 `FixSceneMaterials()` 方法

#### 初始化流程更新
```
1. 創建核心系統
2. 驗證靜態場景結構
3. 生成動態物件
4. 生成玩家
5. 設置攝影機
6. 設置環境
7. 修復場景材質 ← 新增
8. 設置游標
9. 初始化遊戲狀態
```

### 4. NPC 互動測試工具

#### 新增文件：NPCInteractionTester.cs (Assets/Scripts/Tests/NPCInteractionTester.cs)

**功能：**
- 自動化測試 NPC 互動系統
- 驗證耐心條倒數計時功能
- 測試訂單系統

**快捷鍵：**
- `T` - 開始自動測試
- `P` - 打印當前狀態
- `O` - 給隨機 NPC 下訂單

**測試項目：**
1. NPCManager 存在性檢查
2. NPC 生成檢查
3. 訂單系統測試
4. 耐心條計時器測試
5. 互動提示測試

## NPC 互動系統驗證

### 耐心條倒數計時
**現有實現（無需修改）：**
- `NPCController.UpdateOrderTimer()` - 每幀減少耐心值
- `NPCManager.Update()` - 調用所有 NPC 的 UpdateNPC()
- `PatienceBarUI.OnGUI()` - 顯示耐心條

**工作原理：**
```csharp
// NPCController.cs, Line 147-176
private void UpdateOrderTimer(float deltaTime)
{
    if (!hasActiveOrder || orderTimedOut) return;

    patienceRemaining -= deltaTime;  // 每幀減少
    float patienceRatio = patienceRemaining / currentOrder.patienceTime;

    OnPatienceChanged?.Invoke(this, patienceRatio);

    // 超時處理
    if (patienceRemaining <= 0)
    {
        orderTimedOut = true;
        hasActiveOrder = false;  // 設為 false，UI 會自動隱藏
        OnOrderTimedOut?.Invoke(this);
    }
}
```

### 耐心條超時消失
**現有實現（無需修改）：**
```csharp
// PatienceBarUI.cs, Line 61-68
private void OnGUI()
{
    var npcs = FindObjectsOfType<NPCController>();
    foreach (var npc in npcs)
    {
        if (npc.HasActiveOrder)  // 只顯示有訂單的 NPC
        {
            DrawPatienceBar(npc);
        }
    }
}
```

當 `hasActiveOrder` 變為 `false` 時，PatienceBarUI 會自動停止繪製該 NPC 的耐心條，實現超時消失效果。

## 已驗證功能

✅ **給 NPC 送酒的 F 鍵提示**
- 玩家拿著裝滿飲料的杯子靠近有訂單的 NPC（1-3米內）
- UI 顯示 "Press F to serve {NPC名稱}"
- 按 F 鍵可以將酒給 NPC 喝
- 系統會自動評分並計算金幣

✅ **耐心條時間減少**
- NPC 下訂單後，耐心條會實時減少
- 耐心值低於 50% 時 NPC 情緒變為 Disappointed
- 耐心值低於 25% 時 NPC 情緒變為 Angry

✅ **耐心條超時消失**
- 當耐心值降到 0 時，訂單自動取消
- hasActiveOrder 設為 false
- PatienceBarUI 停止顯示該 NPC 的進度條
- NPC 情緒變為 Angry

✅ **場景材質修復**
- 自動檢測紫色/缺失的材質
- 根據物件名稱智能應用合適的材質
- 優化場景光照

## 使用說明

### 場景材質修復
如果場景中出現紫色物件：
1. 遊戲會在啟動時自動修復
2. 或在 Unity 編輯器中添加 SceneMaterialFixer 組件並執行 "修復所有材質"

### NPC 互動測試
1. 在場景中添加 NPCInteractionTester 組件
2. 運行遊戲後按 `T` 開始自動測試
3. 觀察控制台輸出的測試結果
4. 按 `O` 給 NPC 下訂單，觀察耐心條是否正常倒數
5. 等待訂單超時，觀察耐心條是否消失

### 給 NPC 送酒
1. 按 E 鍵與 NPC 對話，NPC 會下訂單
2. 到吧台調製對應的雞尾酒
3. 拿著裝滿酒的杯子靠近 NPC
4. 看到 "Press F to serve {NPC名稱}" 提示
5. 按 F 鍵送酒給 NPC
6. 系統自動評分並計算金幣

## 技術細節

### 材質修復邏輯
```csharp
// 檢查是否需要修復
bool NeedsMaterialFix(Renderer renderer)
{
    if (renderer.sharedMaterial == null) return true;
    if (renderer.sharedMaterial.shader == null) return true;

    string shaderName = renderer.sharedMaterial.shader.name;
    if (shaderName.Contains("Hidden") || shaderName.Contains("Error"))
        return true;

    return false;
}
```

### Shader 選擇優先級
1. Universal Render Pipeline/Lit (URP)
2. Standard (Built-in)
3. Legacy Shaders/Diffuse

### 材質顏色配置
可在 SceneMaterialFixer 組件的 Inspector 中調整：
- Floor Color: (0.3, 0.25, 0.2) - 深棕色
- Wall Color: (0.8, 0.75, 0.7) - 淺米色
- Counter Color: (0.25, 0.15, 0.1) - 深木色
- NPC Body Color: (0.2, 0.4, 0.7) - 藍色
- NPC Head Color: (0.9, 0.75, 0.6) - 膚色

## 已知問題和限制

1. **材質修復**
   - 需要在遊戲運行時才能修復（無法保存到場景）
   - 如果要永久保存，需要在編輯器模式下手動執行

2. **NPC 互動**
   - 目前只支持按 F 鍵給 NPC 送酒
   - E 鍵用於對話/下訂單

## 未來改進建議

1. **材質系統**
   - 添加材質配置文件（ScriptableObject）
   - 支持自定義材質映射規則

2. **NPC 系統**
   - 添加更多 NPC 反應動畫
   - 實現 NPC 排隊系統
   - 添加小費系統（根據耐心值計算）

3. **UI 改進**
   - 使用 Canvas UI 替代 OnGUI
   - 添加訂單完成的視覺反饋
   - 顯示賺取的金幣數量

## 測試清單

- [x] 場景材質修復功能
- [x] NPC 訂單系統
- [x] 耐心條倒數計時
- [x] 耐心條超時消失
- [x] 給 NPC 送酒的 F 鍵提示
- [x] 送酒功能
- [x] 金幣計算系統整合

## 作者
Claude (AI Assistant)

## 更新日期
2025-12-02
