# 🍸 分子調酒模擬器 (Molecular Mixology Simulator)

> 一款結合科學與藝術的調酒模擬遊戲，讓玩家在虛擬酒吧中學習調製各種經典雞尾酒。

[![Unity Version](https://img.shields.io/badge/Unity-2021.3+-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Development Status](https://img.shields.io/badge/Status-Active%20Development-yellow.svg)]()

---

## 💡 專案理念

### 為什麼做這個遊戲？

這個專案的核心理念是：**讓學習調酒變得有趣且容易上手**。

我們希望：
- 🎓 **教育性**：玩家可以學習真實的調酒知識和技巧
- 🎮 **娛樂性**：透過遊戲化的方式，讓學習過程不枯燥
- 🧪 **科學性**：展示分子調酒的科學原理
- 🎨 **藝術性**：欣賞雞尾酒的色彩和美學

### 設計哲學

1. **混合架構**：結合 Unity 視覺化編輯和程式化生成的優點
2. **模組化設計**：每個功能都是獨立的組件，易於擴展和維護
3. **玩家友善**：直覺的 UI 和操作，Minecraft 風格的簡潔介面
4. **真實性**：基於真實的酒類資料和調酒配方

---

## 📋 目錄

- [快速開始](#-快速開始)
- [專案架構](#-專案架構)
- [代碼結構](#-代碼結構)
- [功能說明](#-功能說明)
- [開發指南](#-開發指南)
- [如何接續開發](#-如何接續開發)
- [已知問題](#-已知問題)
- [未來計畫](#-未來計畫)

---

## 🚀 快速開始

### 環境需求

- **Unity**: 2021.3 或更高版本
- **TextMeshPro**: 已包含在 Unity 中
- **作業系統**: Windows / macOS / Linux

### 安裝步驟

1. **克隆專案**
   ```bash
   git clone https://github.com/cake11298/SWE_MVP.git
   cd SWE_MVP
   ```

2. **用 Unity 開啟專案**
   - 打開 Unity Hub
   - 點擊「開啟」
   - 選擇專案資料夾

3. **開啟場景**
   - **MainMenu**: `Assets/Scenes/MainMenu.unity`
   - **GameScene**: `Assets/Scenes/GameScene.unity`

4. **執行遊戲**
   - 開啟 `MainMenu.unity`
   - 按下 Play 按鈕

---

## 🏗️ 專案架構

### 架構演進

我們的專案經歷了**重大架構重構**（v2.0），從全動態生成模式改為混合架構。

#### v1.0：全動態生成（Three.js 風格）
```
❌ 所有東西都用程式碼生成
❌ 無法在 Editor 中預覽
❌ 調試困難
❌ 擴展新功能需要大量程式碼
```

#### v2.0：混合架構（當前版本）✨
```
✅ 靜態結構在 Scene 中（視覺化）
✅ 動態內容用程式碼生成（靈活）
✅ 符合 Unity 標準流程
✅ 開發效率提升 3-5 倍
```

詳細架構說明請參考 [`ARCHITECTURE_REFACTOR.md`](ARCHITECTURE_REFACTOR.md)

### 場景架構

```
專案
├── MainMenu.unity（主選單場景）
│   └── 玩家選擇開始遊戲、設定、離開
│
└── GameScene.unity（遊戲場景）
    ├── 靜態結構（手動在 Scene 中創建）
    │   ├── 地板、牆壁、天花板
    │   ├── 吧台
    │   ├── 酒瓶架子
    │   └── 玻璃杯站台
    │
    └── 動態內容（運行時生成）
        ├── 24 個酒瓶
        ├── 6 個玻璃杯
        ├── 調酒器
        └── 6 個 NPC
```

### 系統架構圖

```
┌─────────────────────────────────────────────────────┐
│                   核心系統層                          │
│  GameManager │ SceneLoader │ SaveLoadSystem         │
│  AudioManager │ LightingManager                     │
└─────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────┐
│                   遊戲邏輯層                          │
│  CocktailSystem │ NPCManager │ InteractionSystem    │
│  UpgradeSystem │ TutorialSystem                     │
└─────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────┐
│                   場景管理層                          │
│  MainMenuInitializer │ GameSceneInitializer         │
│  DynamicObjectSpawner │ EnvironmentManager          │
└─────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────┐
│                   UI 層                              │
│  MainMenuManager │ UIManager │ MinecraftButton      │
│  ShopManager │ SettingsManager                      │
└─────────────────────────────────────────────────────┘
```

---

## 📁 代碼結構

### 資料夾組織

```
Assets/
├── Scenes/                    # 場景文件
│   ├── MainMenu.unity        # 主選單場景
│   └── GameScene.unity       # 遊戲場景
│
├── Scripts/
│   ├── Core/                 # 核心系統
│   │   ├── GameManager.cs           # 遊戲主管理器
│   │   ├── BarSceneBuilder.cs       # [舊] 場景建造器（已不建議使用）
│   │   └── GameSceneInitializer.cs  # [新] 遊戲場景初始化器
│   │
│   ├── Systems/              # 遊戲系統
│   │   ├── SceneLoader.cs           # 場景載入系統
│   │   ├── SaveLoadSystem.cs        # 存檔系統
│   │   ├── CocktailSystem.cs        # 調酒系統
│   │   ├── InteractionSystem.cs     # 互動系統
│   │   ├── UpgradeSystem.cs         # 升級系統
│   │   └── TutorialSystem.cs        # 教學系統
│   │
│   ├── Managers/             # 管理器
│   │   ├── AudioManager.cs          # 音效管理
│   │   ├── LightingManager.cs       # 燈光管理
│   │   ├── NPCManager.cs            # NPC 管理
│   │   ├── EnvironmentManager.cs    # 環境管理
│   │   └── MaterialManager.cs       # 材質管理
│   │
│   ├── UI/                   # 使用者介面
│   │   ├── MainMenuManager.cs       # 主選單管理器
│   │   ├── MainMenuInitializer.cs   # 主選單初始化器
│   │   ├── MinecraftButton.cs       # Minecraft 風格按鈕
│   │   ├── UIManager.cs             # UI 主管理器
│   │   ├── UIFactory.cs             # [舊] UI 工廠（已不建議使用）
│   │   ├── ShopManager.cs           # 商店管理
│   │   └── SettingsManager.cs       # 設定管理
│   │
│   ├── Environment/          # 環境相關
│   │   ├── StaticBarStructure.cs    # [新] 靜態結構標記
│   │   ├── DynamicObjectSpawner.cs  # [新] 動態物件生成器
│   │   └── EnvironmentSetup.cs      # 環境設置
│   │
│   ├── Objects/              # 遊戲物件
│   │   ├── Bottle.cs                # 酒瓶
│   │   ├── Glass.cs                 # 玻璃杯
│   │   ├── Shaker.cs                # 調酒器
│   │   └── Container.cs             # 容器基類
│   │
│   ├── Interaction/          # 互動系統
│   │   ├── IInteractable.cs         # 可互動介面
│   │   ├── PlayerInteraction.cs     # 玩家互動
│   │   ├── PickupSystem.cs          # 拾取系統
│   │   └── BottleReturnZone.cs      # [新] 玻璃瓶回收區
│   │
│   ├── NPC/                  # NPC 相關
│   │   ├── NPCController.cs         # NPC 控制器
│   │   ├── InteractableNPC.cs       # 可互動 NPC
│   │   └── CustomerBehavior.cs      # 顧客行為
│   │
│   ├── Player/               # 玩家相關
│   │   ├── PlayerController.cs      # 玩家控制器
│   │   └── PlayerMovement.cs        # 玩家移動
│   │
│   └── Data/                 # 資料結構
│       ├── LiquorData.cs            # 酒類資料
│       ├── RecipeData.cs            # 配方資料
│       ├── LiquorDatabase.cs        # 酒類資料庫
│       └── RecipeDatabase.cs        # 配方資料庫
```

---

## 🎮 功能說明

### 核心功能對應表

| 功能 | 對應代碼 | 說明 |
|------|---------|------|
| **場景切換** | `SceneLoader.cs` | 負責 MainMenu ↔ GameScene 的切換 |
| **主選單** | `MainMenuManager.cs`<br>`MainMenuInitializer.cs` | 管理選單 UI 和初始化 |
| **遊戲初始化** | `GameSceneInitializer.cs` | 初始化遊戲場景、創建系統 |
| **動態生成** | `DynamicObjectSpawner.cs` | 生成酒瓶、玻璃杯、NPC |
| **靜態結構** | `StaticBarStructure.cs` | 標記場景中的靜態物件 |
| **調酒系統** | `CocktailSystem.cs` | 處理配方、混合邏輯 |
| **互動系統** | `InteractionSystem.cs`<br>`PlayerInteraction.cs` | 玩家與物件的互動 |
| **拾取/放置** | `PickupSystem.cs`<br>`PlacementSystem.cs` | 物品拾取和放置邏輯 |
| **NPC 管理** | `NPCManager.cs`<br>`CustomerBehavior.cs` | NPC 生成、行為、對話 |
| **商店系統** | `ShopManager.cs`<br>`UpgradeSystem.cs` | 購買酒類、解鎖配方 |
| **存檔系統** | `SaveLoadSystem.cs` | 遊戲進度保存和讀取 |
| **UI 系統** | `UIManager.cs`<br>`MinecraftButton.cs` | UI 管理和 Minecraft 風格按鈕 |
| **瓶子回收** | `BottleReturnZone.cs` | 空瓶回收獲得獎勵 |

### 遊戲流程圖

```
玩家啟動遊戲
    ↓
MainMenu 場景載入
    ├── MainMenuInitializer 初始化系統
    └── 顯示選單 UI（開始遊戲、設定、離開）
        ↓
    玩家點擊「開始遊戲」
        ↓
    SceneLoader 切換到 GameScene
        ↓
GameScene 場景載入
    ├── GameSceneInitializer 初始化
    │   ├── 創建所有管理器
    │   ├── 驗證靜態結構
    │   └── 生成玩家
    │
    ├── DynamicObjectSpawner 生成物件
    │   ├── 在酒瓶架上生成 24 個酒瓶
    │   ├── 在吧台生成 6 個玻璃杯
    │   ├── 生成調酒器
    │   └── 生成 6 個 NPC 顧客
    │
    └── 遊戲開始
        ↓
玩家操作
    ├── 拿起玻璃杯
    ├── 選擇酒瓶倒酒
    ├── 使用調酒器混合
    ├── 給予 NPC 飲料
    ├── 獲得評分和金錢
    ├── 回收空瓶（BottleReturnZone）
    └── 在商店購買升級
        ↓
按 ESC → 暫停選單 → 返回主選單
```

---

## 💻 開發指南

### 如何添加新酒類

1. **在資料庫中添加**

   編輯 `LiquorDatabase.cs` 或在 Inspector 中添加：
   ```csharp
   new LiquorData {
       name = "Tequila",
       displayName = "龍舌蘭",
       category = LiquorCategory.BaseSpirit,
       alcoholContent = 40f,
       color = new Color(0.95f, 0.95f, 0.85f),
       price = 200,
       description = "墨西哥傳統烈酒"
   }
   ```

2. **自動生成**

   `DynamicObjectSpawner` 會自動讀取資料庫並生成對應的酒瓶

### 如何添加新配方

1. **創建配方資料**

   在 `RecipeDatabase.cs` 中添加：
   ```csharp
   new RecipeData {
       name = "Margarita",
       displayName = "瑪格麗特",
       ingredients = new[] {
           new Ingredient("Tequila", 45),
           new Ingredient("Triple Sec", 15),
           new Ingredient("Lime Juice", 30)
       },
       difficulty = 2,
       unlockPrice = 500
   }
   ```

2. **在商店中顯示**

   `ShopManager` 會自動列出所有配方供玩家解鎖

### 如何添加新 UI 面板

使用**標準 Unity 流程**（不要用 UIFactory）：

1. **在 Scene 中創建 UI**
   - 在 Canvas 下創建 Panel
   - 添加 Button、Text 等元素
   - 使用 `MinecraftButton` 組件美化按鈕

2. **創建管理器腳本**
   ```csharp
   public class NewPanelManager : MonoBehaviour {
       [SerializeField] private GameObject panel;
       [SerializeField] private Button closeButton;

       private void Start() {
           closeButton.onClick.AddListener(ClosePanel);
       }

       public void OpenPanel() {
           panel.SetActive(true);
       }
   }
   ```

3. **在 Inspector 中設置引用**

### 如何添加新功能區域（像 BottleReturnZone）

1. **創建組件腳本**
   ```csharp
   [RequireComponent(typeof(BoxCollider))]
   public class NewZone : MonoBehaviour {
       private void OnTriggerEnter(Collider other) {
           // 進入區域的邏輯
       }
   }
   ```

2. **在 Scene 中設置**
   - 創建 GameObject
   - 添加 BoxCollider（設為 Trigger）
   - 添加組件
   - 調整位置和大小

---

## 🔧 如何接續開發

### 當前開發狀態

✅ **已完成**
- 場景架構重構（v2.0）
- 主選單系統
- 場景自動串接
- 動態物件生成
- 基礎互動系統
- 調酒邏輯
- NPC 系統
- 商店和升級系統
- 存檔系統
- 瓶子回收功能

⚠️ **進行中**
- UI 美化（Minecraft 風格）
- 玩家控制器優化
- 音效系統整合

❌ **待開發**
- 教學系統完善
- 更多配方和酒類
- 成就系統
- 多場景支援
- 視覺特效優化

### 推薦的開發順序

#### 1. **熟悉專案**（1-2 天）
   - 閱讀這份 README
   - 閱讀 `ARCHITECTURE_REFACTOR.md`
   - 運行 MainMenu 和 GameScene
   - 測試主要功能

#### 2. **設置場景**（2-3 天）
   - 在 Unity Editor 中手動創建 MainMenu UI
   - 在 GameScene 中創建靜態結構
   - 測試動態物件生成
   - 調整位置和參數

#### 3. **實現核心功能**（1-2 週）
   - 完善玩家控制器
   - 優化互動系統
   - 添加音效和視覺效果
   - 實現更多調酒配方

#### 4. **UI/UX 優化**（1 週）
   - 使用 MinecraftButton 美化所有按鈕
   - 創建 HUD 介面
   - 添加教學提示
   - 優化使用者體驗

#### 5. **測試和打磨**（持續）
   - 修復 Bug
   - 平衡遊戲難度
   - 優化性能
   - 收集反饋

### 新手任務建議

如果你是第一次加入專案，可以從這些簡單任務開始：

**🌟 簡單任務**
1. 在 `LiquorDatabase` 中添加 3 種新酒類
2. 創建一個新的 Minecraft 風格按鈕
3. 在 GameScene 中放置一個裝飾物件
4. 調整 `BottleReturnZone` 的獎勵金額

**⭐ 中等任務**
1. 創建一個新的配方（需要 3 種材料）
2. 實現一個新的互動物件（例如冰桶）
3. 為 NPC 添加新的對話內容
4. 創建一個新的 UI 面板（例如配方書）

**💫 進階任務**
1. 實現成就系統
2. 添加特殊調酒技巧（例如火焰調酒）
3. 創建新的場景（戶外酒吧）
4. 實現多人模式基礎架構

---

## 🐛 已知問題

### 高優先級

1. **玩家控制器**
   - 移動有時會卡住
   - 攝影機旋轉需要優化
   - **位置**: `PlayerController.cs`

2. **物件位置重置**
   - 物品有時會回到 (0,0,0)
   - **原因**: `originalPosition` 在 transform 設置前被捕捉
   - **解決方案**: 在 `Start()` 而非 `Awake()` 中記錄位置

3. **UI 引用遺失**
   - 如果 MainMenuManager 的 UI 引用未設置會報錯
   - **解決方案**: 已添加 `ValidateUIReferences()` 驗證

### 中優先級

1. **場景切換時的系統重複**
   - 某些系統可能被創建多次
   - **解決方案**: 確保使用 `DontDestroyOnLoad` 和單例檢查

2. **動態生成的物件沒有清理**
   - 切換場景時可能留下殘餘物件
   - **位置**: `DynamicObjectSpawner.cs`

### 低優先級

1. **音效播放重疊**
   - 快速點擊按鈕會播放多個音效
   - **位置**: `MinecraftButton.cs`

2. **材質載入失敗**
   - Resources 資料夾中的材質有時載入不到
   - **臨時方案**: 動態創建材質

---

## 🚀 未來計畫

### 短期目標（1-2 個月）

- [ ] 完成所有 UI 的 Minecraft 風格化
- [ ] 添加 20+ 種酒類和配方
- [ ] 實現完整的教學系統
- [ ] 添加音效和背景音樂
- [ ] 優化玩家控制器
- [ ] 實現成就系統

### 中期目標（3-6 個月）

- [ ] 添加故事模式
- [ ] 創建多個場景（不同風格的酒吧）
- [ ] 實現顧客滿意度系統
- [ ] 添加特殊調酒技巧和小遊戲
- [ ] 實現排行榜系統
- [ ] 優化視覺效果（粒子、光影）

### 長期目標（6-12 個月）

- [ ] 多人合作模式
- [ ] 創意工坊支援（玩家自製配方）
- [ ] VR 模式
- [ ] 移動平台移植
- [ ] 真實物理模擬（液體流動）
- [ ] 社群功能（分享配方、比賽）

---

## 📚 重要文檔

| 文檔 | 說明 |
|------|------|
| `README.md`（本文件） | 專案總覽、開發指南 |
| `ARCHITECTURE_REFACTOR.md` | 架構重構詳細說明 |
| `現有程式問題.md` | 舊架構的問題記錄 |
| `現有程式問題 extend.md` | 問題擴展說明 |

---

## 🤝 貢獻指南

### Git Commit 訊息格式

```
類型: 簡短描述（50 字以內）

詳細描述（可選）
- 改動 1
- 改動 2
```

**類型**:
- `Add`: 新增功能
- `Fix`: 修復 Bug
- `Refactor`: 重構代碼
- `Update`: 更新現有功能
- `Docs`: 文檔更新
- `Style`: 格式調整

### 代碼規範

1. **命名規範**
   - 類別: `PascalCase`（例：`GameManager`）
   - 方法: `PascalCase`（例：`Initialize()`）
   - 變數: `camelCase`（例：`playerHealth`）

2. **註解規範**
   ```csharp
   /// <summary>
   /// 初始化遊戲場景
   /// </summary>
   public void Initialize() { }
   ```

3. **組織規範**
   - 使用 `#region` 組織代碼
   - 順序：序列化欄位 → 私有欄位 → Unity 生命週期 → 公開方法 → 私有方法

---

## 💬 常見問題 FAQ

### Q: 為什麼要重構架構？

**A**: 原本的全動態生成模式（Three.js 風格）在 Unity 中有以下問題：
- 無法在 Editor 中預覽場景
- 調整位置需要改程式碼
- 調試困難

新的混合架構開發效率提升 3-5 倍。

### Q: 我應該使用 BarSceneBuilder 還是新架構？

**A**: **強烈建議使用新架構**：
- `BarSceneBuilder.cs` 已標記為不建議使用
- 使用 `GameSceneInitializer.cs` + `DynamicObjectSpawner.cs`
- 靜態結構在 Scene 中手動創建

### Q: 如何測試場景切換？

**A**:
1. 開啟 `MainMenu.unity`
2. 按 Play
3. 點擊「開始遊戲」按鈕
4. 應該會自動切換到 `GameScene.unity`

---

## 📞 聯絡資訊

- **專案負責人**: cake11298
- **GitHub**: https://github.com/cake11298/SWE_MVP
- **問題回報**: [GitHub Issues](https://github.com/cake11298/SWE_MVP/issues)

---

## 📈 專案統計

- **代碼行數**: ~15,000+
- **腳本文件**: 50+
- **場景數量**: 2
- **酒類種類**: 24+
- **配方數量**: 10+
- **開發時間**: 持續進行中

---

**最後更新**: 2025-12-02
**文檔版本**: v2.0
**專案狀態**: 🟢 積極開發中

---

<div align="center">

**如果這個專案對你有幫助，請給我們一個 ⭐ Star！**

Made with ❤️ by the SWE_MVP Team

</div>
