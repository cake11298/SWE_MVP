# 如何測試 NPC 服務系統

## 系統已完成的修改

### 1. Cointreau 顯示名稱修正
- ✅ 已將 Cointreau 添加到 LiquorDatabase
- ✅ 已修正 Cointreau 瓶子的 liquidName 為 "cointreau"
- ✅ 已更新 GlassContainer 來從資料庫查詢顯示名稱

### 2. NPC 服務系統
- ✅ 已創建 SimpleNPCServe 組件
- ✅ 已添加到所有 NPC（NPC01, Gustave_NPC, Seaton_NPC）
- ✅ 按 F 鍵服務飲料給 NPC
- ✅ 每次服務獲得 200 金幣
- ✅ 服務後杯子會被清空

## 測試步驟

### 測試 Cointreau 顯示
1. 啟動遊戲
2. 拿起 Cointreau 瓶子（按 E）
3. 拿起 ServeGlass（按 E）
4. 對著 ServeGlass 按住左鍵倒酒
5. **預期結果**：UI 應該顯示 "Cointreau : 40ml"（或類似的量）而不是 "None : 40ml"

### 測試 NPC 服務系統
1. 確保 ServeGlass 裡有酒（任何酒都可以）
2. 拿著 ServeGlass 走近任何一個 NPC（NPC01, Gustave_NPC, 或 Seaton_NPC）
3. 確保距離在 3 單位以內
4. 按 **F 鍵**
5. **預期結果**：
   - Console 會顯示：`SimpleNPCServe: Served drink ([內容], [容量]ml) to [NPC名稱]. Earned 200 coins! Total coins: [總金幣]`
   - ServeGlass 會被清空
   - 金幣數量增加 200

### 如果沒有反應

#### 檢查 1：確認組件已添加
在 Unity Editor 中：
1. 選擇 NPC01、Gustave_NPC 或 Seaton_NPC
2. 在 Inspector 中查看是否有 "Simple NPC Serve" 組件
3. 如果沒有，運行菜單：`Bar > Add SimpleNPCServe to NPCs`

#### 檢查 2：確認距離
- NPC 服務系統的互動距離是 3 單位
- 確保你站在 NPC 附近（很近）

#### 檢查 3：確認杯子有液體
- 確保 ServeGlass 不是空的
- 可以倒任何酒進去測試

#### 檢查 4：確認你拿著杯子
- 系統會檢查 ServeGlass 是否在玩家附近（2 單位內）
- 確保你已經拿起了 ServeGlass

## 當前系統設計

### 工作原理
1. SimpleNPCServe 組件附加在每個 NPC 上
2. 每幀檢查玩家是否在 3 單位範圍內
3. 當玩家按 F 鍵時：
   - 尋找場景中的 "ServeGlass" 物件
   - 檢查它是否在玩家附近（2 單位內）
   - 檢查它是否有液體（GlassContainer.IsEmpty()）
   - 如果都滿足，清空杯子並給予 200 金幣

### 金幣系統
- 金幣存儲在 `GameManager.Instance.GetScore().totalCoins`
- 每次服務增加 200 金幣
- 可以在 Inspector 中修改 `coinsPerServe` 來改變獎勵金額

## 已知限制

1. **只支持 ServeGlass**：目前系統只檢查名為 "ServeGlass" 的物件
2. **簡單的距離檢測**：使用距離來判斷是否拿著杯子，而不是直接檢查交互系統
3. **沒有視覺反饋**：目前只有 Console 日誌，沒有 UI 提示或動畫

## 未來改進建議

1. 添加 UI 提示（"按 F 服務飲料"）
2. 添加 NPC 反應動畫或對話
3. 根據飲料品質給予不同金幣獎勵
4. 添加音效反饋
5. 支持多個杯子類型
6. 添加服務動畫

## 調試信息

如果系統不工作，檢查 Console 日誌：
- `SimpleNPCServe: Glass is empty!` - 杯子是空的
- `SimpleNPCServe: You need to be holding the glass` - 杯子不在玩家附近
- `SimpleNPCServe: Served drink...` - 成功服務

## 文件位置

- 主腳本：`Assets/Scripts/NPC/SimpleNPCServe.cs`
- 編輯器工具：`Assets/Scripts/Editor/AddSimpleNPCServe.cs`
- 資料庫更新：`Assets/Scripts/Data/LiquorData.cs`
- 顯示名稱查詢：`Assets/Scripts/Objects/GlassContainer.cs`
