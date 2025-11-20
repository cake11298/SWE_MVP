# NCU Bar Simulator - Three.js to Unity Migration Map

## 專案概覽

本文件詳細記錄從 Three.js 3D 調酒遊戲移植到 Unity C# 的完整對照表。

**原始專案：** Three.js + Cannon.js (JavaScript)
**目標平台：** Unity 6000 LTS (C#)

---

## 1. 檔案對照表

### 1.1 核心系統

| Three.js 檔案 | 行數 | Unity C# 檔案 | 說明 |
|--------------|------|---------------|------|
| `CocktailSystem.js` | 1445 | `Scripts/Systems/CocktailSystem.cs` | 核心調酒邏輯 |
| | | `ScriptableObjects/LiquorDatabase.asset` | 酒類資料庫 |
| | | `ScriptableObjects/RecipeDatabase.asset` | 配方資料庫 |
| | | `Scripts/Data/LiquorData.cs` | 酒類資料結構 |
| | | `Scripts/Data/RecipeData.cs` | 配方資料結構 |
| | | `Scripts/Systems/PouringSystem.cs` | 倒酒系統 |
| | | `Scripts/Systems/ShakingSystem.cs` | 搖酒系統 |
| `NPCManager.js` | 1102 | `Scripts/Managers/NPCManager.cs` | NPC 管理 |
| | | `Scripts/NPC/NPCController.cs` | 單一 NPC 控制器 |
| | | `Scripts/NPC/DialogueSystem.cs` | 對話系統 |
| | | `Scripts/NPC/DrinkEvaluator.cs` | 飲料評分 |
| | | `ScriptableObjects/NPCData.asset` | NPC 資料 |
| `InteractionSystem.js` | 443 | `Scripts/Systems/InteractionSystem.cs` | 互動系統 |
| | | `Scripts/Interaction/PickupController.cs` | 拾取控制 |
| | | `Scripts/Interaction/IInteractable.cs` | 互動介面 |
| `PlayerController.js` | 227 | `Scripts/Player/FirstPersonController.cs` | 第一人稱控制 |
| | | `Scripts/Player/PlayerInputHandler.cs` | 輸入處理 |
| `PhysicsSystem.js` | 364 | Unity Physics (內建) | 使用 Unity Physics |
| | | `Scripts/Physics/PhysicsSetup.cs` | 物理設定 |
| `LightingSystem.js` | 555 | `Scripts/Environment/LightingManager.cs` | 光照管理 |
| | | `Scripts/Environment/DynamicLights.cs` | 動態光照 |
| `BarEnvironment.js` | 192 | `Scripts/Environment/BarEnvironment.cs` | 場景協調 |
| `index.js` | 483 | `Scripts/Core/GameManager.cs` | 遊戲主控制 |

### 1.2 環境模組

| Three.js 檔案 | 行數 | Unity C# 檔案 | 說明 |
|--------------|------|---------------|------|
| `bar/BarStructure.js` | 151 | `Prefabs/Environment/BarStructure.prefab` | 預製場景結構 |
| `bar/BarBottles.js` | 336 | `Scripts/Objects/BottleSpawner.cs` | 酒瓶生成器 |
| | | `Prefabs/Objects/Bottles/*.prefab` | 酒瓶預製物 |
| `bar/BarTools.js` | 209 | `Prefabs/Objects/Tools/*.prefab` | 工具預製物 |
| `bar/BarDisplays.js` | 347 | `Scripts/Environment/DisplayManager.cs` | 展示櫃管理 |
| `bar/BarFurniture.js` | 449 | `Prefabs/Environment/Furniture/*.prefab` | 家具預製物 |
| `RetirementLounge.js` | 408 | `Prefabs/Environment/RetirementLounge.prefab` | 休息區預製物 |

---

## 2. 資料結構對照

### 2.1 酒類資料庫 (LiquorDatabase)

**Three.js 結構：**
```javascript
database.set('vodka', {
    name: '伏特加',
    displayName: 'Vodka',
    color: 0xf0f0f0,
    alcoholContent: 40,
    category: 'base_spirit'
});
```

**Unity C# 結構：**
```csharp
[System.Serializable]
public class LiquorData
{
    public string id;           // "vodka"
    public string nameZH;       // "伏特加"
    public string displayName;  // "Vodka"
    public Color color;         // Color(0.94f, 0.94f, 0.94f)
    public float alcoholContent; // 40f
    public LiquorCategory category; // LiquorCategory.BaseSpirit
}

public enum LiquorCategory
{
    BaseSpirit,     // 六大基酒
    Mixer,          // 調味料
    Juice,          // 果汁
    Liqueur,        // 利口酒
    FortifiedWine   // 加烈酒
}
```

### 2.2 容器內容追蹤

**Three.js 結構：**
```javascript
this.containerContents = new Map();
// container -> { ingredients: [], color, volume, maxVolume, liquidMesh }
```

**Unity C# 結構：**
```csharp
[System.Serializable]
public class ContainerContents
{
    public List<Ingredient> ingredients = new();
    public Color mixedColor;
    public float volume;
    public float maxVolume;
}

[System.Serializable]
public class Ingredient
{
    public string type;
    public string name;
    public string displayName;
    public float amount;
    public Color color;
}

// 使用 Dictionary 或組件方式
public class ContainerComponent : MonoBehaviour
{
    public ContainerContents contents;
}
```

### 2.3 NPC 資料

**Three.js 結構：**
```javascript
{
    name: 'Gustave',
    position: new THREE.Vector3(2, 0, -5),
    shirtColor: 0x0066cc,
    pantsColor: 0x1a1a1a,
    role: '調酒社創始社長',
    dialogues: ["...", "..."],
    gender: 'male'
}
```

**Unity C# ScriptableObject：**
```csharp
[CreateAssetMenu(fileName = "NPCData", menuName = "Bar/NPC Data")]
public class NPCData : ScriptableObject
{
    public string npcName;
    public Vector3 position;
    public Color shirtColor;
    public Color pantsColor;
    public string role;
    [TextArea(3, 10)]
    public string[] dialogues;
    public Gender gender;
    public float rotation;
}

public enum Gender { Male, Female }
```

---

## 3. 系統邏輯對照

### 3.1 調酒系統核心邏輯

#### 倒酒 (Pour)
```javascript
// Three.js
pour(bottle, targetContainer, liquorType, deltaTime, camera) {
    // 檢查容器是否已滿
    // 檢查距離和視角
    // 計算倒出量 = pourRate * deltaTime
    // 合併同類材料
    // 更新混合顏色
    // 更新視覺效果
}
```

```csharp
// Unity C#
public void Pour(Bottle bottle, Container target, float deltaTime)
{
    if (target.Contents.IsFull) return;

    // 使用 Physics.Raycast 檢查距離和視角
    if (!IsValidPourTarget(target)) return;

    float amountPoured = pourRate * deltaTime;
    target.AddIngredient(bottle.LiquorType, amountPoured);
    target.UpdateMixedColor();
    target.UpdateLiquidVisual();
}
```

#### 顏色混合演算法
```javascript
// Three.js - 加權平均
updateMixedColor(container) {
    let r = 0, g = 0, b = 0;
    let totalAmount = 0;

    contents.ingredients.forEach(ingredient => {
        const color = new THREE.Color(ingredient.color);
        const weight = ingredient.amount;
        r += color.r * weight;
        g += color.g * weight;
        b += color.b * weight;
        totalAmount += weight;
    });

    // 平均
    r /= totalAmount;
    g /= totalAmount;
    b /= totalAmount;
}
```

```csharp
// Unity C#
public Color CalculateMixedColor()
{
    Vector3 rgb = Vector3.zero;
    float totalAmount = 0;

    foreach (var ingredient in ingredients)
    {
        float weight = ingredient.amount;
        rgb.x += ingredient.color.r * weight;
        rgb.y += ingredient.color.g * weight;
        rgb.z += ingredient.color.b * weight;
        totalAmount += weight;
    }

    if (totalAmount > 0)
        rgb /= totalAmount;

    return new Color(rgb.x, rgb.y, rgb.z);
}
```

### 3.2 互動系統

#### Raycasting
```javascript
// Three.js
checkTargeted() {
    this.raycaster.setFromCamera(new THREE.Vector2(0, 0), this.camera);
    const intersects = this.raycaster.intersectObjects(this.interactableObjects, true);
}
```

```csharp
// Unity C#
public IInteractable CheckTargeted()
{
    Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
    {
        return hit.collider.GetComponent<IInteractable>();
    }
    return null;
}
```

### 3.3 玩家控制器

#### 移動邏輯
```javascript
// Three.js
update(deltaTime) {
    const moveVector = new THREE.Vector3();
    if (this.keys['w']) moveVector.z -= 1;
    if (this.keys['s']) moveVector.z += 1;
    if (this.keys['a']) moveVector.x -= 1;
    if (this.keys['d']) moveVector.x += 1;

    moveVector.normalize();
    moveVector.multiplyScalar(this.speed * deltaTime);
    moveVector.applyAxisAngle(new THREE.Vector3(0, 1, 0), this.rotation);

    this.position.add(moveVector);
}
```

```csharp
// Unity C# (使用 New Input System)
public class FirstPersonController : MonoBehaviour
{
    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;

    void Update()
    {
        // 移動
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * speed * Time.deltaTime);

        // 視角
        transform.Rotate(Vector3.up * lookInput.x * sensitivity);
        pitch -= lookInput.y * sensitivity;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
```

---

## 4. Unity 專案結構

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   └── Constants.cs
│   ├── Systems/
│   │   ├── CocktailSystem.cs
│   │   ├── InteractionSystem.cs
│   │   ├── PouringSystem.cs
│   │   └── ShakingSystem.cs
│   ├── Managers/
│   │   ├── NPCManager.cs
│   │   └── UIManager.cs
│   ├── Player/
│   │   ├── FirstPersonController.cs
│   │   └── PlayerInputHandler.cs
│   ├── NPC/
│   │   ├── NPCController.cs
│   │   ├── DialogueSystem.cs
│   │   └── DrinkEvaluator.cs
│   ├── Interaction/
│   │   ├── IInteractable.cs
│   │   ├── PickupController.cs
│   │   └── InteractableObject.cs
│   ├── Objects/
│   │   ├── Bottle.cs
│   │   ├── Glass.cs
│   │   ├── Shaker.cs
│   │   └── Container.cs
│   ├── Environment/
│   │   ├── BarEnvironment.cs
│   │   ├── LightingManager.cs
│   │   └── DynamicLights.cs
│   ├── Data/
│   │   ├── LiquorData.cs
│   │   ├── RecipeData.cs
│   │   ├── NPCData.cs
│   │   └── ContainerContents.cs
│   └── UI/
│       ├── HUDController.cs
│       ├── RecipePanel.cs
│       ├── DialogueBox.cs
│       └── PourProgressUI.cs
├── ScriptableObjects/
│   ├── Liquors/
│   │   ├── LiquorDatabase.asset
│   │   └── [各酒類 SO]
│   ├── Recipes/
│   │   ├── RecipeDatabase.asset
│   │   └── [各配方 SO]
│   └── NPCs/
│       └── [各 NPC 資料 SO]
├── Prefabs/
│   ├── Player/
│   │   └── FirstPersonPlayer.prefab
│   ├── Environment/
│   │   ├── BarStructure.prefab
│   │   └── Furniture/
│   ├── Objects/
│   │   ├── Bottles/
│   │   ├── Glasses/
│   │   └── Tools/
│   ├── NPC/
│   │   └── NPCBase.prefab
│   └── UI/
│       └── [UI 預製物]
├── Materials/
│   ├── Liquids/
│   ├── Glass/
│   └── Environment/
├── Scenes/
│   └── BarScene.unity
├── Input/
│   └── PlayerInputActions.inputactions
└── Resources/
    └── [動態載入資源]
```

---

## 5. 數值參數對照表

### 5.1 遊戲常數

| 參數 | Three.js 值 | Unity 值 | 說明 |
|------|------------|----------|------|
| 倒酒速度 | 30 ml/s | 30f | `pourRate` |
| 杯子容量 | 300 ml | 300f | `maxVolume` |
| Shaker 容量 | 500 ml | 500f | `maxVolume` |
| 互動距離 | 3 m | 3f | `interactionDistance` |
| 移動速度 | 5 m/s | 5f | `moveSpeed` |
| 滑鼠敏感度 | 0.002 | 2f | `mouseSensitivity` |
| 重力加速度 | -9.82 | -9.82f | Unity 預設 |

### 5.2 物理參數

| 材質 | 摩擦力 | 彈性 |
|------|--------|------|
| 默認 | 0.3 | 0.3 |
| 玻璃-地板 | 0.4 | 0.4 |

### 5.3 碰撞群組

| 群組 | Three.js | Unity Layer |
|------|----------|-------------|
| DEFAULT | 1 | Default |
| OBJECT | 2 | Interactable |
| SHELF | 4 | Shelf |

---

## 6. 酒類資料完整清單

### 6.1 六大基酒 (Base Spirit)

| ID | 中文名 | 英文名 | 顏色 (Hex) | 酒精度 |
|----|--------|--------|-----------|--------|
| vodka | 伏特加 | Vodka | 0xf0f0f0 | 40% |
| gin | 琴酒 | Gin | 0xe8f4f8 | 40% |
| rum | 蘭姆酒 | Rum | 0xd4a574 | 40% |
| whiskey | 威士忌 | Whiskey | 0xb87333 | 40% |
| tequila | 龍舌蘭 | Tequila | 0xf5deb3 | 40% |
| brandy | 白蘭地 | Brandy | 0x8b4513 | 40% |

### 6.2 調味料 (Mixer)

| ID | 中文名 | 英文名 | 顏色 (Hex) | 酒精度 |
|----|--------|--------|-----------|--------|
| lemon_juice | 檸檬汁 | Lemon Juice | 0xfff44f | 0% |
| lime_juice | 萊姆汁 | Lime Juice | 0x32cd32 | 0% |
| simple_syrup | 糖漿 | Simple Syrup | 0xffe4b5 | 0% |
| grenadine | 紅石榴糖漿 | Grenadine | 0xff0000 | 0% |
| angostura_bitters | 安格仕苦精 | Angostura Bitters | 0x8b0000 | 44.7% |
| soda_water | 蘇打水 | Soda Water | 0xe0ffff | 0% |
| tonic_water | 通寧水 | Tonic Water | 0xf0ffff | 0% |
| cola | 可樂 | Cola | 0x3e2723 | 0% |
| coconut_cream | 椰漿 | Coconut Cream | 0xfffaf0 | 0% |

### 6.3 果汁類 (Juice)

| ID | 中文名 | 英文名 | 顏色 (Hex) | 酒精度 |
|----|--------|--------|-----------|--------|
| orange_juice | 柳橙汁 | Orange Juice | 0xffa500 | 0% |
| pineapple_juice | 鳳梨汁 | Pineapple Juice | 0xffeb3b | 0% |
| cranberry_juice | 蔓越莓汁 | Cranberry Juice | 0xdc143c | 0% |
| tomato_juice | 番茄汁 | Tomato Juice | 0xff6347 | 0% |
| grapefruit_juice | 葡萄柚汁 | Grapefruit Juice | 0xff69b4 | 0% |

### 6.4 利口酒 & 香艾酒 (Liqueur & Fortified Wine)

| ID | 中文名 | 英文名 | 顏色 (Hex) | 酒精度 |
|----|--------|--------|-----------|--------|
| vermouth_dry | 不甜香艾酒 | Dry Vermouth | 0xe8e8d0 | 18% |
| vermouth_sweet | 甜香艾酒 | Sweet Vermouth | 0x8b4513 | 18% |
| campari | 金巴利 | Campari | 0xdc143c | 25% |
| triple_sec | 橙皮酒 | Triple Sec | 0xffa500 | 40% |
| liqueur | 利口酒 | Liqueur | 0xff6b9d | 20% |

---

## 7. NPC 資料完整清單

| 名稱 | 角色 | 位置 | 衣服顏色 | 褲子顏色 | 性別 | 旋轉 |
|------|------|------|----------|----------|------|------|
| Gustave | 調酒社創始社長 | (2, 0, -5) | 0x0066cc | 0x1a1a1a | Male | 0 |
| Seaton | 調酒社共同創辦人 | (-2, 0, -5) | 0xcc0066 | 0x333333 | Male | 0 |
| 正安 | 公關兼副社長 | (9, 0, 1) | 0xffb6c1 | 0x4169e1 | Female | -π/2 |
| 瑜柔(宅魚) | 學術研究長 | (9, 0, 3) | 0x90ee90 | 0x2f4f4f | Female | -π/2 |
| 恩若 | 美宣長 | (9, 0, -1) | 0xffd700 | 0x8b4513 | Female | -π/2 |
| 旻偉 | 器材長 | (9, 0, 5) | 0x708090 | 0x556b2f | Male | -π/2 |

---

## 8. 移植優先順序

### 階段 1：基礎架構 (第 1-2 週)
- [x] Unity 專案結構設定
- [ ] 第一人稱控制器 (FirstPersonController.cs)
- [ ] 基礎場景設置 (地板、牆壁、天花板)
- [ ] Unity Input System 設定

### 階段 2：核心系統 (第 3-5 週)
- [ ] 資料結構定義 (LiquorData, RecipeData, ContainerContents)
- [ ] ScriptableObject 資料庫建立
- [ ] CocktailSystem.cs - 核心調酒邏輯
- [ ] ContainerController.cs - 容器行為
- [ ] 液體視覺效果 (Shader)

### 階段 3：互動系統 (第 6-7 週)
- [ ] InteractionSystem.cs - Raycast 互動
- [ ] PickupController.cs - 拾取/放置
- [ ] PouringSystem.cs - 倒酒動畫與粒子效果

### 階段 4：NPC 與 UI (第 8-10 週)
- [ ] NPCManager.cs - NPC 管理
- [ ] DialogueSystem.cs - 對話框
- [ ] DrinkEvaluator.cs - 飲料評分
- [ ] RecipePanel.cs - 配方顯示
- [ ] HUD 與其他 UI

### 階段 5：環境與打磨 (第 11-12 週)
- [ ] 場景環境建置
- [ ] LightingManager.cs - 動態光照
- [ ] 音效系統
- [ ] 測試與最佳化

---

## 9. 技術注意事項

### 9.1 Unity 特定考量

1. **Input System**: 使用 Unity New Input System 取代直接鍵盤監聽
2. **Physics**: 使用內建 Unity Physics 取代 Cannon.js
3. **材質**: 使用 URP/HDRP Shader 實現玻璃和液體效果
4. **UI**: 使用 UI Toolkit 或 TextMeshPro
5. **資料管理**: 使用 ScriptableObject 管理靜態資料

### 9.2 效能最佳化

1. **Object Pooling**: 對粒子系統使用物件池
2. **LOD**: 對遠距離物件使用 Level of Detail
3. **Batching**: 合併靜態物件的繪製調用
4. **Occlusion Culling**: 啟用遮擋剔除

### 9.3 跨平台考量

- 桌面 (Windows/Mac): 主要目標
- WebGL: 需要特別處理指標鎖定
- 行動裝置: 需要觸控控制替代方案

---

## 10. 待決定事項

1. **渲染管線**: 選擇 URP
2. **版本控制**: 使用 Git LFS 處理大型資源
3. **Asset Store**: 是否使用現成的第一人稱控制器？ (如果 Claude Code 有能力 則下載你所需要的各種免費 Assets)
4. **音效**: 使用 Unity Audio 還是 FMOD/Wwise？
5. **本地化**: 是否需要多語言支援？

---

## 更新日誌

- **2025-11-19**: 初始版本，完成所有核心系統分析
