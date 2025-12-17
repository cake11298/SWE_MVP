# DecorationManager 設置指南

## 快速設置步驟

### 1. 創建 DecorationManager 物件
1. 在 TheBar 場景中，右鍵點擊 Hierarchy
2. 選擇 `Create Empty`
3. 命名為 `DecorationManager`
4. 添加 `DecorationManager` 組件

### 2. 設置音箱（Speakers）
由於場景中的音箱物件可能分散在 Props 下，需要將它們組織起來：

#### 方法 A：手動設置
1. 在 Props 下創建一個空物件命名為 `Speakers`
2. 找到所有音箱相關的物件（名稱包含 "Speaker" 的物件）
3. 將這些物件拖到 `Speakers` 父物件下
4. 在 Inspector 中取消勾選 `Speakers` 物件（預設禁用）
5. 將 `Speakers` 物件拖到 `DecorationManager` 的 `Speakers Parent` 欄位

#### 方法 B：使用自動查找（推薦）
1. 選擇 `DecorationManager` 物件
2. 在 Inspector 中找到 `DecorationManager` 組件
3. 右鍵點擊組件標題
4. 選擇 `Auto-Find Decoration Parents`
5. 這會自動找到所有包含 "Speaker" 的物件並組織它們
6. 檢查 Inspector 確認 `Speakers Parent` 已正確指定
7. 確保 `Speakers` 父物件預設是禁用的

### 3. 測試
1. 進入遊戲
2. 確認音箱預設是隱藏的
3. 在 GameEndPanel 購買音箱（2000 金幣）
4. 重新開始遊戲（按繼續遊戲按鈕）
5. 確認音箱現在顯示了

## 場景結構示例

```
TheBar Scene
├── Props
│   ├── Speakers (父物件，預設禁用)
│   │   ├── SM_Speaker1
│   │   ├── SM_Speaker2
│   │   └── SM_Speaker3
│   ├── SM_Table1
│   └── ...
├── DecorationManager (組件已添加)
├── Player
└── UI_Canvas
```

## Inspector 設置

### DecorationManager 組件
```
DecorationManager (Script)
├── Speakers Parent: Speakers (GameObject)
├── Plants Parent: (可選，未來使用)
└── Paintings Parent: (可選，未來使用)
```

## 常見問題

### Q: 音箱購買後還是沒顯示？
A: 確認以下幾點：
1. `Speakers` 父物件已正確指定給 `DecorationManager`
2. 購買後需要重新開始遊戲（不是重新載入場景）
3. 檢查 Console 是否有錯誤訊息

### Q: 找不到音箱物件？
A: 在 Hierarchy 中搜尋 "Speaker"，應該會找到相關物件

### Q: 自動查找功能沒有作用？
A: 確認：
1. 場景中有名為 "Props" 的物件
2. Props 下有包含 "Speaker" 的子物件
3. 如果沒有，請使用手動設置方法

## 添加更多裝飾品

### 盆栽（Plants）
1. 創建 `Plants` 父物件
2. 將所有盆栽物件放入
3. 預設禁用
4. 指定給 `DecorationManager.plantsParent`

### 畫（Paintings）
1. 創建 `Paintings` 父物件
2. 將所有畫物件放入
3. 預設禁用
4. 指定給 `DecorationManager.paintingsParent`

## 調試技巧

### 檢查購買狀態
在 Console 中查看以下訊息：
```
DecorationManager: Speakers enabled/disabled
PersistentGameData: Purchased Speaker
```

### 強制啟用音箱（測試用）
1. 選擇 `Speakers` 父物件
2. 在 Inspector 中勾選啟用
3. 這只是臨時測試，不會保存購買狀態

---

**提示：** 如果場景中沒有音箱物件，可以先跳過這個設置。系統會正常運作，只是購買音箱後不會有視覺變化。
