# 系統初始化修復總結

## 問題描述

遊戲中有許多功能的程式碼已經編寫完成，但沒有被正確組裝和初始化，導致這些功能無法運作。

## 主要問題

### 1. 致命的系統缺失 (System Initialization Gaps)

**問題**: SceneSetup.cs 的 CreateSystemManagers() 方法只建立了最基礎的管理器，漏掉了超過一半的系統。

**缺失的管理器**:
- ShopManager (商店介面 B 鍵會沒反應)
- UpgradeSystem (升級邏輯不存在)
- TutorialSystem (新手教學不會跳出來)
- SaveLoadSystem (無法存檔)
- SettingsManager (設定選單無效)
- SceneLoader (場景切換邏輯缺失)
- PlacementPreviewSystem (Q 鍵智能吸附會報錯或失效)

### 2. 物品生成邏輯缺失 (Spawning Gap)

**問題**: 場景中沒有生成任何可互動道具。

**缺失的道具**:
- 酒瓶 (Bottles) - 無法倒酒
- 杯子 (Glasses) - 無法裝酒
- 搖酒器 (Shaker) - 無法搖酒
- 攪拌棒 (Stirrer) - 無法攪拌
- 裝飾站 (GarnishStation) - 無法添加裝飾
- 冰桶 (IceBucket) - 無法取得冰塊

### 3. 資源引用缺失 (Missing References)

**問題**: 因為沒有使用 Prefab，所有資源需要通過 Resources.Load() 動態載入，但程式碼沒有實作。

**缺失的資源載入**:
- AudioManager 的音效庫載入
- 音樂和環境音載入
- UI 圖片資源載入

## 解決方案

### 1. 系統管理器初始化修復

**修改文件**: `Assets/Scripts/Core/SceneSetup.cs`

在 `CreateSystemManagers()` 方法中添加了以下系統的初始化：

```csharp
// ShopManager
if (BarSimulator.UI.ShopManager.Instance == null)
{
    var shopObj = new GameObject("ShopManager");
    shopObj.AddComponent<BarSimulator.UI.ShopManager>();
}

// UpgradeSystem
if (UpgradeSystem.Instance == null)
{
    var upgradeObj = new GameObject("UpgradeSystem");
    upgradeObj.AddComponent<UpgradeSystem>();
}

// TutorialSystem
if (TutorialSystem.Instance == null)
{
    var tutorialObj = new GameObject("TutorialSystem");
    tutorialObj.AddComponent<TutorialSystem>();
}

// SaveLoadSystem
if (SaveLoadSystem.Instance == null)
{
    var saveObj = new GameObject("SaveLoadSystem");
    saveObj.AddComponent<SaveLoadSystem>();
}

// SettingsManager
if (BarSimulator.UI.SettingsManager.Instance == null)
{
    var settingsObj = new GameObject("SettingsManager");
    settingsObj.AddComponent<BarSimulator.UI.SettingsManager>();
}

// SceneLoader
if (SceneLoader.Instance == null)
{
    var loaderObj = new GameObject("SceneLoader");
    loaderObj.AddComponent<SceneLoader>();
}

// PlacementPreviewSystem
if (PlacementPreviewSystem.Instance == null)
{
    var previewObj = new GameObject("PlacementPreviewSystem");
    previewObj.AddComponent<PlacementPreviewSystem>();
}
```

### 2. 場景道具生成修復

**修改文件**: `Assets/Scripts/Core/SceneSetup.cs`

新增 `SpawnBarProps()` 方法及相關輔助方法：

- `SpawnBarProps()` - 主要的道具生成邏輯
- `SpawnBottle()` - 生成酒瓶（Vodka, Gin, Rum）
- `SpawnGlass()` - 生成杯子（透明材質）
- `SpawnShaker()` - 生成搖酒器（銀色金屬材質）
- `SpawnStirrer()` - 生成攪拌棒
- `SpawnGarnishStation()` - 生成裝飾工作站
- `SpawnIceBucket()` - 生成冰桶
- `InitializeBottleAfterCocktailSystem()` - 延遲初始化酒瓶（等待 CocktailSystem）

**生成的道具**:
- 3 個酒瓶（伏特加、琴酒、蘭姆酒）
- 2 個玻璃杯
- 1 個搖酒器
- 1 個攪拌棒
- 1 個裝飾工作站
- 1 個冰桶

所有道具都會自動：
- 設置正確的大小和顏色
- 添加 Rigidbody 組件（可移動的物品）
- 掛載對應的腳本組件
- 放置在吧台上的正確位置

### 3. 資源載入修復

**修改文件**: `Assets/Scripts/Systems/AudioManager.cs`

新增和修改的方法：

- `LoadSFXLibrary()` - 載入所有音效
- `TryLoadSFX()` - 安全載入單個音效（不存在不報錯）
- `LoadMusicAndAmbience()` - 載入音樂和環境音

**載入的資源**:
- 12 種音效（倒酒、搖酒、攪拌、碰撞等）
- 2 首音樂（主選單、遊戲場景）
- 1 個環境音（酒吧氛圍）

所有資源都會從 `Assets/Resources/` 資料夾動態載入，如果文件不存在會顯示警告但不會崩潰。

## 資源目錄結構

創建了 `Assets/Resources/README.md` 文檔，詳細說明了所需的資源文件結構：

```
Assets/Resources/
├── Audio/
│   ├── Music/
│   │   ├── MainMenu.mp3
│   │   └── GameScene.mp3
│   ├── Ambience/
│   │   └── BarAmbience.mp3
│   └── SFX/
│       ├── Pour.wav
│       ├── Shake.wav
│       └── ... (其他 10 個音效)
└── Sprites/
    └── UI/
```

## 影響範圍

### 修改的文件
1. `Assets/Scripts/Core/SceneSetup.cs` - 添加系統初始化和道具生成
2. `Assets/Scripts/Systems/AudioManager.cs` - 添加資源動態載入

### 新增的文件
1. `Assets/Resources/README.md` - 資源目錄結構說明

### 未修改但相關的文件
以下文件現在會被正確初始化和使用：
- `Assets/Scripts/UI/ShopManager.cs`
- `Assets/Scripts/Systems/UpgradeSystem.cs`
- `Assets/Scripts/Systems/TutorialSystem.cs`
- `Assets/Scripts/Systems/SaveLoadSystem.cs`
- `Assets/Scripts/UI/SettingsManager.cs`
- `Assets/Scripts/Systems/SceneLoader.cs`
- `Assets/Scripts/Systems/PlacementPreviewSystem.cs`
- `Assets/Scripts/Objects/Bottle.cs`
- `Assets/Scripts/Objects/Glass.cs`
- `Assets/Scripts/Objects/Shaker.cs`
- `Assets/Scripts/Objects/Stirrer.cs`
- `Assets/Scripts/Objects/GarnishStation.cs`

## 測試建議

### 系統初始化驗證
1. 啟動遊戲
2. 檢查 Console 確認所有管理器都已初始化
3. 測試商店介面（按 B 鍵）
4. 測試設定選單
5. 測試存檔/讀檔功能

### 場景道具驗證
1. 啟動遊戲進入場景
2. 確認吧台上有 3 個酒瓶、2 個杯子、1 個搖酒器等道具
3. 測試拾取和放下這些道具
4. 測試倒酒、搖酒等基本功能

### 音效驗證
1. 檢查 Console 的音效載入日誌
2. 如果有音效文件，測試各種操作的音效播放
3. 如果沒有音效文件，確認遊戲仍能正常運行

## 已知限制

1. **音效文件**: 目前專案中可能沒有實際的音效文件，需要手動添加到 Resources 資料夾
2. **冰桶功能**: 目前冰桶只是一個靜態物件，可能需要額外的腳本來實現冰塊生成功能
3. **視覺效果**: 所有道具使用基本的幾何形狀（Primitive），可能需要後續替換為更精美的 3D 模型

## 後續改進建議

1. 為所有道具創建專門的 Prefab
2. 添加實際的音效和音樂文件
3. 實現冰桶的冰塊生成功能
4. 為道具添加更精美的 3D 模型和材質
5. 添加更多的裝飾物類型
6. 實現更完整的教程系統引導

## 結論

此次修復解決了遊戲系統初始化的核心問題，使得原本已經編寫好的功能能夠正常運作。現在所有的管理器系統都會被正確初始化，場景中也會生成可互動的道具，遊戲的核心循環應該可以正常進行。
