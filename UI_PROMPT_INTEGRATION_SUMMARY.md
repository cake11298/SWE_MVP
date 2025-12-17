# UI Prompt System Integration Summary

## 概述
已成功將 UI Prompt System 整合到遊戲的互動系統中，讓玩家清楚知道他們正在與什麼物件互動。

## 整合的系統

### 1. PlayerInteraction.cs
**位置**: `Assets/Scripts/Player/PlayerInteraction.cs`

**功能**:
- 當玩家瞄準可互動物件時，顯示 "按 E 拾取 [物件名稱]"
- 當玩家拾取物件時，顯示 "拾取了 [物件名稱]"
- 當玩家不再瞄準物件時，自動隱藏提示

**整合內容**:
```csharp
// 瞄準物件時顯示提示
private void ShowInteractionPrompt(GameObject obj)
{
    string itemName = GetFriendlyName(obj);
    UIPromptManager.Show($"按 E 拾取 {itemName}");
}

// 拾取物件時顯示反饋
UIPromptManager.Show($"拾取了 {itemName}");

// 不再瞄準時隱藏提示
UIPromptManager.Hide();
```

### 2. ItemInteractionSystem.cs
**位置**: `Assets/Scripts/Player/ItemInteractionSystem.cs`

**功能**:
- 與 PlayerInteraction.cs 類似的功能
- 支援高亮顯示和互動提示
- 支援倒酒互動

**整合內容**:
```csharp
// 高亮物件時顯示提示
private void HighlightObject(GameObject obj)
{
    // ... 高亮邏輯 ...
    ShowInteractionPrompt(obj);
}

// 清除高亮時隱藏提示
private void ClearHighlight()
{
    // ... 清除邏輯 ...
    UIPromptManager.Hide();
}
```

### 3. ImprovedInteractionSystem.cs
**位置**: `Assets/Scripts/Player/ImprovedInteractionSystem.cs`

**功能**:
- 改進版的互動系統
- 支援不同的互動模式（拾取、放回原位、倒酒）
- 根據當前狀態顯示不同的提示

**整合內容**:
```csharp
// 根據持有物品顯示不同提示
private void ShowInteractionPrompt(GameObject obj)
{
    if (heldObject != null && heldItem.itemType == ItemType.Bottle)
    {
        // 持有酒瓶，看著杯子
        UIPromptManager.Show($"按住左鍵倒酒到 {itemName}");
    }
    else
    {
        // 沒有持有物品
        UIPromptManager.Show($"按 E 拾取 {itemName}");
    }
}
```

### 4. InteractableNPC.cs
**位置**: `Assets/Scripts/NPC/InteractableNPC.cs`

**功能**:
- NPC 互動提示
- 對話和給予飲料的反饋

**整合內容**:
```csharp
// 瞄準 NPC 時
public override void OnTargeted()
{
    string npcName = npcController.NPCName;
    UIPromptManager.Show($"按 E 與 {npcName} 互動");
}

// 與 NPC 對話時
UIPromptManager.Show($"正在與 {npcName} 對話...");

// 給 NPC 飲料時
UIPromptManager.Show($"給了 {npcName} {drinkInfo.cocktailName}");
```

### 5. IInteractable.cs (基礎類別)
**位置**: `Assets/Scripts/Interaction/IInteractable.cs`

**功能**:
- 為所有可互動物件提供基礎提示功能
- 自動根據物件類型顯示適當的提示

**整合內容**:
```csharp
public virtual void OnTargeted()
{
    if (!string.IsNullOrEmpty(displayName))
    {
        string actionText = GetActionText();
        UIPromptManager.Show($"{actionText} {displayName}");
    }
}

protected virtual string GetActionText()
{
    if (canPickup)
        return "按 E 拾取";
    else if (interactableType == InteractableType.NPC)
        return "按 E 與";
    else
        return "按 E 互動";
}
```

## 智能名稱識別

所有系統都使用 `GetFriendlyName()` 方法來獲取物件的友好顯示名稱：

```csharp
private string GetFriendlyName(GameObject obj)
{
    // 1. 檢查 InteractableItem 組件
    var interactableItem = obj.GetComponent<InteractableItem>();
    if (interactableItem != null)
        return interactableItem.itemName;

    // 2. 檢查 IInteractable 介面
    var interactable = obj.GetComponent<IInteractable>();
    if (interactable != null)
        return interactable.DisplayName;

    // 3. 檢查 LiquidContainer 組件
    var liquidContainer = obj.GetComponent<LiquidContainer>();
    if (liquidContainer != null)
        return liquidContainer.liquidName;

    // 4. 檢查 GlassContainer 組件
    var glassContainer = obj.GetComponent<GlassContainer>();
    if (glassContainer != null)
        return "玻璃杯";

    // 5. 回退到物件名稱
    return obj.name;
}
```

## 提示訊息範例

### 拾取物件
- "按 E 拾取 Gin"
- "按 E 拾取 Shaker"
- "按 E 拾取 ServeGlass"
- "按 E 拾取 Jigger"

### 拾取反饋
- "拾取了 Gin"
- "拾取了 Shaker"
- "拾取了 ServeGlass"

### NPC 互動
- "按 E 與 Gustave 互動"
- "正在與 Gustave 對話..."
- "給了 Gustave Martini"

### 倒酒
- "按住左鍵倒酒到 ServeGlass"
- "按住左鍵倒酒到 玻璃杯"

## 視覺效果

- **位置**: 準心下方 80 像素
- **字體**: 黑色，32pt
- **對齊**: 居中
- **自動隱藏**: 3 秒後自動消失
- **即時更新**: 新訊息會立即替換舊訊息

## 使用的組件

所有整合都使用 `UIPromptManager` 單例系統：

```csharp
using BarSimulator.UI;

// 顯示提示
UIPromptManager.Show("訊息內容");

// 隱藏提示
UIPromptManager.Hide();
```

## 測試建議

1. **拾取測試**:
   - 瞄準各種物件（酒瓶、杯子、搖酒器等）
   - 確認顯示正確的物件名稱
   - 確認拾取後顯示反饋訊息

2. **NPC 互動測試**:
   - 瞄準 NPC
   - 確認顯示 NPC 名稱
   - 與 NPC 對話
   - 給 NPC 飲料

3. **倒酒測試**:
   - 拾取酒瓶
   - 瞄準杯子
   - 確認顯示倒酒提示

4. **自動隱藏測試**:
   - 確認提示在 3 秒後自動消失
   - 確認移開視線時提示立即消失

## 編譯狀態

✅ 無編譯錯誤
✅ 所有系統已整合
✅ 準備進行遊戲測試

## 相關文件

- `Assets/Scripts/UI/UIPromptManager.cs` - 提示系統核心
- `Assets/Scripts/UI/UIPromptManager_README.md` - 詳細文檔
- `UI_PROMPT_SYSTEM_SUMMARY.md` - 系統總結
