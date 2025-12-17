# 靜態裝飾物品設置說明

## 問題描述
場景中所有 SM_ 開頭的物品（玻璃杯、酒瓶、裝飾物等）在遊戲開始時會因為重力而倒塌。

## 解決方案
創建了 `StaticProp` 組件來管理這些物品的物理狀態：
- **預設狀態**：物品為 kinematic（不受重力影響），保持靜止
- **拾取時**：啟用物理效果，允許物品移動
- **放下後**：保持物理效果，讓物品自然落下

## 已處理的物品統計
- **總共處理**: 1997 個物品
- **玻璃杯**: 348 個
- **酒瓶**: 105 個
- **其他裝飾物**: 1544 個
- **修復的 Collider**: 911 個

## 新增的腳本

### 1. StaticProp.cs
**位置**: `Assets/Scripts/Environment/StaticProp.cs`

**功能**:
- 管理物品的物理狀態（kinematic/dynamic）
- 記錄原始位置和旋轉
- 支援自動返回原位功能（可選）

**主要屬性**:
```csharp
public bool canBePickedUp = true;              // 是否可以被拾取
public bool returnToOriginalPosition = false;  // 放下後是否自動返回原位
public float returnDelay = 2f;                 // 返回原位的延遲時間
```

**主要方法**:
- `OnPickup()`: 當物品被拾取時調用，啟用物理效果
- `OnDrop()`: 當物品被放下時調用
- `ForceReturnToOriginal()`: 強制返回原位

### 2. SetupStaticProps.cs (編輯器工具)
**位置**: `Assets/Scripts/Editor/SetupStaticProps.cs`

**功能**:
- 自動找到所有 SM_ 開頭的物品
- 添加必要的組件（Rigidbody, Collider, StaticProp, InteractableItem）
- 設置正確的物理參數

**使用方法**:
Unity 編輯器 → `Bar Simulator` → `Setup Static Props (SM_ Objects)`

### 3. FixStaticPropsColliders.cs (編輯器工具)
**位置**: `Assets/Scripts/Editor/FixStaticPropsColliders.cs`

**功能**:
- 修復所有 StaticProp 物品的 Collider 設置
- 將 MeshCollider 設置為 convex（可拾取物品必須）
- 確保 Rigidbody 設置正確

**使用方法**:
Unity 編輯器 → `Bar Simulator` → `Fix Static Props Colliders`

## 修改的現有腳本

### 1. PlayerInteraction.cs
- 在 `TryPickup()` 中添加 StaticProp.OnPickup() 調用
- 在 `TryPlace()` 和 `Drop()` 中添加 StaticProp.OnDrop() 調用

### 2. ItemInteractionSystem.cs
- 在拾取物品時調用 StaticProp.OnPickup()
- 在放下物品時調用 StaticProp.OnDrop()

### 3. ImprovedInteractionSystem.cs
- 在 `TryPickupObject()` 中添加 StaticProp.OnPickup() 調用
- 在 `ReturnToOriginalPosition()` 和 `DropAtCurrentPosition()` 中添加 StaticProp.OnDrop() 調用

## 物理設置詳情

### 初始狀態（未被拾取）
```csharp
Rigidbody.isKinematic = true;   // 不受物理影響
Rigidbody.useGravity = false;   // 不受重力影響
```

### 拾取後
```csharp
Rigidbody.isKinematic = true;   // 仍為 kinematic（跟隨手部位置）
Rigidbody.useGravity = false;   // 不受重力影響
```

### 放下後
```csharp
Rigidbody.isKinematic = false;  // 啟用物理模擬
Rigidbody.useGravity = true;    // 受重力影響
```

## 特殊物品處理

### 玻璃杯（SM_*Glass*, SM_Martini*, SM_Wine*）
- 添加 `InteractableItem` 組件，類型設為 `Glass`
- 可以接收液體
- 最大容量：300ml

### 酒瓶（SM_*Bottle*, SM_Sampagne*, SM_Whiskey*）
- 添加 `InteractableItem` 組件，類型設為 `Bottle`
- 可以倒出液體
- 初始容量：750ml
- 根據名稱自動識別液體類型（Wine, Whiskey 等）

### 其他裝飾物
- 只添加 `StaticProp` 組件
- 可以被拾取和移動
- 不具備特殊功能

## 使用流程

### 首次設置（已完成）
1. 執行 `Bar Simulator` → `Setup Static Props (SM_ Objects)`
2. 執行 `Bar Simulator` → `Fix Static Props Colliders`
3. 保存場景

### 添加新的 SM_ 物品
1. 將物品放入場景
2. 執行 `Bar Simulator` → `Setup Static Props (SM_ Objects)`
3. 執行 `Bar Simulator` → `Fix Static Props Colliders`

### 自定義設置
在 Inspector 中選擇物品，調整 `StaticProp` 組件的屬性：
- `Can Be Picked Up`: 是否允許拾取
- `Return To Original Position`: 是否自動返回原位
- `Return Delay`: 返回原位的延遲時間（秒）

## 測試建議

1. **基本拾取測試**
   - 走近任何 SM_ 物品
   - 按 E 鍵拾取
   - 確認物品跟隨手部移動
   - 按 Q 鍵放下
   - 確認物品自然落下

2. **玻璃杯測試**
   - 拾取玻璃杯
   - 拾取酒瓶
   - 對準玻璃杯倒酒
   - 確認液體量變化

3. **物理碰撞測試**
   - 放下物品時確認它們會自然落下
   - 確認物品之間會發生碰撞
   - 確認物品不會穿透地面或其他物體

4. **性能測試**
   - 確認場景啟動時所有物品保持靜止
   - 確認沒有不必要的物理計算

## 注意事項

1. **MeshCollider 必須為 Convex**
   - 可拾取的物品必須使用 convex collider
   - 已通過 `FixStaticPropsColliders` 工具自動修復

2. **不要手動修改 Rigidbody 設置**
   - StaticProp 組件會自動管理 Rigidbody 狀態
   - 手動修改可能導致物品行為異常

3. **場景保存**
   - 執行編輯器工具後記得保存場景
   - 確保所有修改都被保存

4. **性能考慮**
   - 大量物品同時啟用物理效果可能影響性能
   - 建議只在需要時拾取物品
   - 放下後物品會保持動態狀態，直到靜止

## 故障排除

### 問題：物品仍然在遊戲開始時倒下
**解決方案**:
1. 檢查 Rigidbody 的 `isKinematic` 是否為 true
2. 檢查 `useGravity` 是否為 false
3. 重新執行 `Setup Static Props` 工具

### 問題：無法拾取物品
**解決方案**:
1. 檢查物品是否有 `StaticProp` 組件
2. 檢查 `canBePickedUp` 是否為 true
3. 檢查 Collider 是否存在且啟用
4. 檢查 MeshCollider 是否為 convex

### 問題：物品拾取後行為異常
**解決方案**:
1. 檢查 MeshCollider 是否為 convex
2. 重新執行 `Fix Static Props Colliders` 工具
3. 檢查 Rigidbody 的質量設置是否合理

### 問題：物品穿透地面或其他物體
**解決方案**:
1. 檢查 Collider 設置
2. 調整 Rigidbody 的 Collision Detection 為 Continuous
3. 確保地面有正確的 Collider

## 未來改進建議

1. **物品分類系統**
   - 為不同類型的物品設置不同的物理參數
   - 例如：玻璃杯更輕，酒瓶更重

2. **音效反饋**
   - 拾取時播放音效
   - 放下時根據材質播放不同音效

3. **視覺反饋**
   - 高亮可拾取的物品
   - 顯示拾取提示

4. **自動整理功能**
   - 一鍵將所有物品返回原位
   - 清理掉落在地上的物品

5. **物品耐久度**
   - 玻璃杯掉落可能破碎
   - 酒瓶掉落可能漏酒
