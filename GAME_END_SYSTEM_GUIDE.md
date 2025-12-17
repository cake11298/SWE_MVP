# Game End System Guide

## 概述

遊戲結算系統已完全重構，包含以下功能：
1. **持久化數據系統** - 金幣、酒類升級、裝飾品購買在遊戲間保留
2. **三欄式結算介面** - 左側升級、中間統計、右側裝飾品
3. **Shaker 容器系統** - 可接收液體但不能直接服務給 NPC

---

## 主要功能

### 1. 持久化遊戲數據 (PersistentGameData)

**位置**: `Assets/Scripts/Data/PersistentGameData.cs`

**功能**:
- 跨遊戲場景保存金幣總數
- 保存基酒升級等級（5種酒，每種最高 Lv.5）
- 保存裝飾品購買狀態（3種裝飾品）

**基酒類型**:
- Vodka（伏特加）
- Gin（琴酒）
- Rum（蘭姆酒）
- Whiskey（威士忌）
- Wine（葡萄酒）

**裝飾品類型**:
- Speaker（音箱）- $3000
- Plant（盆栽）- $3000
- Painting（畫）- $3000

**升級系統**:
- 每種基酒從 Lv.1 開始
- 最高可升級到 Lv.5
- 每次升級消耗 $1000

---

### 2. 遊戲結算介面 (GameEndUI)

**位置**: `Assets/Scripts/UI/GameEndUI.cs`

**佈局**:
```
┌─────────────────────────────────────────────────────────┐
│  Left Section    │   Center Section   │  Right Section   │
│  (Upgrades)      │   (Statistics)     │  (Decorations)   │
├──────────────────┼────────────────────┼──────────────────┤
│                  │                    │                  │
│  Vodka  Lv.1/5   │  Bar Closed!       │  音箱            │
│  [Upgrade $1000] │                    │  未購買          │
│                  │  Total Coins: $X   │  [Purchase $3000]│
│  Gin    Lv.1/5   │                    │                  │
│  [Upgrade $1000] │  Drinks Served: X  │  盆栽            │
│                  │                    │  未購買          │
│  Rum    Lv.1/5   │  [Main Menu]       │  [Purchase $3000]│
│  [Upgrade $1000] │                    │                  │
│                  │  [Next Game]       │  畫              │
│  Whiskey Lv.1/5  │                    │  未購買          │
│  [Upgrade $1000] │                    │  [Purchase $3000]│
│                  │                    │                  │
│  Wine   Lv.1/5   │                    │                  │
│  [Upgrade $1000] │                    │                  │
│                  │                    │                  │
└──────────────────┴────────────────────┴──────────────────┘
```

**按鈕功能**:
- **Main Menu**: 返回主選單
- **Next Game**: 繼承金幣和升級，重新開始遊戲（重載當前場景）
- **Upgrade**: 升級對應的基酒（消耗 $1000）
- **Purchase**: 購買裝飾品（消耗 $3000）

---

### 3. Shaker 容器系統

**位置**: `Assets/Scripts/Objects/ShakerContainer.cs`

**功能**:
- ✅ 可以接收來自酒瓶的液體
- ✅ 可以倒出液體到杯子
- ✅ 累積多種液體（混合調酒）
- ❌ **不能**直接按 F 服務給 NPC
- ✅ 只有當 Shaker 內有液體時才能倒出

**使用方式**:
1. 拿起酒瓶，對準 Shaker，按住滑鼠左鍵倒酒
2. Shaker 會累積所有倒入的液體
3. 拿起 Shaker，對準杯子，按住滑鼠左鍵倒出
4. 倒出的液體會按比例混合

**容量**: 800ml

---

## 遊戲流程

### 正常遊戲流程

1. **遊戲開始**
   - 玩家開始調酒、服務顧客
   - 每次成功服務獲得金幣（根據評分）
   - 金幣自動累積到 PersistentGameData

2. **遊戲結束**
   - 時間到或按 **P 鍵**強制結束
   - 顯示 GameEndPanel
   - 顯示本局統計：總金幣、出杯數

3. **結算畫面**
   - **左側**: 查看並升級基酒
   - **中間**: 查看統計數據
   - **右側**: 購買裝飾品
   - 選擇 Main Menu 或 Next Game

4. **Next Game**
   - 金幣、升級、裝飾品保留
   - 場景重新載入
   - 繼續遊戲

---

## 快捷鍵

| 按鍵 | 功能 |
|------|------|
| **P** | 強制結束遊戲，顯示結算畫面 |
| ESC | 暫停遊戲 |
| Tab | 顯示食譜面板 |

---

## 測試功能

在 Unity Editor 中，可以使用以下測試功能：

### 測試選單 (Bar/Testing/)

1. **Add Test Coins (5000)**
   - 在 Play Mode 中添加 5000 測試金幣
   - 用於測試升級和購買功能

2. **Reset Persistent Data**
   - 重置所有持久化數據
   - 金幣歸零，升級重置，裝飾品清空

3. **Show Current Persistent Data**
   - 在 Console 顯示當前持久化數據
   - 包含金幣、升級等級、裝飾品狀態

4. **Force Game End (Press P in Play Mode)**
   - 提示按 P 鍵強制結束遊戲

---

## 技術細節

### 金幣系統整合

金幣在兩個地方追蹤：
1. **GameManager.GameScore** - 本局遊戲的金幣（臨時）
2. **PersistentGameData** - 累積的總金幣（持久）

每次獲得金幣時，兩者都會更新：
```csharp
// GameManager.AddDrinkScore()
score.totalCoins += coinsEarned;  // 本局統計
PersistentGameData.Instance.AddCoins(coinsEarned);  // 持久數據
```

### 場景切換

**Main Menu**:
```csharp
SceneManager.LoadScene("MainMenu");
```

**Next Game**:
```csharp
SceneManager.LoadScene(SceneManager.GetActiveScene().name);
```
- 重載當前場景
- PersistentGameData 因為 DontDestroyOnLoad 而保留
- 金幣和升級繼承到新遊戲

---

## 簡化的統計顯示

結算畫面只顯示兩個統計數據：
1. **Total Coins**: 累積的總金幣
2. **Drinks Served**: 本局出杯數

**移除的統計**:
- ~~Satisfied Customers~~（滿意顧客數）
- ~~Average Rating~~（平均評分）
- ~~Accuracy~~（準確度）

---

## 未來擴展

可以考慮的擴展功能：

1. **酒類等級效果**
   - 高等級基酒提升評分
   - 影響調酒品質

2. **裝飾品效果**
   - 音箱：提升顧客滿意度
   - 盆栽：增加小費
   - 畫：提升評分上限

3. **存檔系統**
   - 將 PersistentGameData 保存到 PlayerPrefs
   - 跨遊戲會話保留進度

4. **更多升級項目**
   - 杯子容量升級
   - 倒酒速度升級
   - 更多裝飾品

---

## 常見問題

**Q: 為什麼 Shaker 不能直接服務給 NPC？**
A: Shaker 是調酒工具，需要先倒入杯子才能服務。這符合真實調酒流程。

**Q: 金幣會在遊戲重啟後消失嗎？**
A: 不會。PersistentGameData 使用 DontDestroyOnLoad，在場景切換時保留。但關閉遊戲後會重置（除非實現存檔系統）。

**Q: 如何測試結算畫面？**
A: 進入 Play Mode，按 **P 鍵**即可強制結束遊戲並顯示結算畫面。

**Q: 升級基酒有什麼效果？**
A: 目前升級只是數據記錄。可以在未來版本中實現效果（如提升評分、增加金幣等）。

**Q: 裝飾品購買後會顯示在場景中嗎？**
A: 目前只是數據記錄。可以在未來版本中實現實際的場景物件生成。

---

## 相關文件

- `Assets/Scripts/Data/PersistentGameData.cs` - 持久化數據系統
- `Assets/Scripts/UI/GameEndUI.cs` - 結算介面
- `Assets/Scripts/Core/GameManager.cs` - 遊戲管理器（整合金幣系統）
- `Assets/Scripts/Objects/ShakerContainer.cs` - Shaker 容器
- `Assets/Scripts/Editor/TestGameEndSystem.cs` - 測試工具

---

## 更新日誌

**2024-12-16**
- ✅ 創建 PersistentGameData 系統
- ✅ 重構 GameEndUI 為三欄式佈局
- ✅ 添加基酒升級系統（5種，Lv.1-5）
- ✅ 添加裝飾品購買系統（3種，$3000）
- ✅ 簡化統計顯示（只顯示金幣和出杯數）
- ✅ 添加 Main Menu 和 Next Game 按鈕
- ✅ 整合 PersistentGameData 與 GameManager
- ✅ 添加 P 鍵強制結束遊戲
- ✅ 確認 Shaker 容器功能正常
