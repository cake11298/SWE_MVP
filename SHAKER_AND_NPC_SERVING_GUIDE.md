# Shaker 和 NPC 服务功能指南

## 功能概述

本次更新实现了以下功能：

### 1. Shaker 容器功能
- ✅ Shaker 可以接收来自酒瓶的液体
- ✅ Shaker 可以倒出液体到玻璃杯
- ✅ Shaker 只有在包含液体时才能倒出
- ✅ Shaker **不能**直接serve给NPC（只有ServeGlass可以）

### 2. NPC 服务提示
- ✅ 拿着 ServeGlass 靠近 NPC 时显示提示文字
- ✅ 提示文字格式：`按下 F 把酒給 [NPC名字]`
- ✅ 提示文字样式：白色文字 + 黑色轮廓（更清晰可见）

## 使用方法

### 使用 Shaker 制作鸡尾酒

1. **拾取酒瓶**
   - 按 `E` 键拾取任意酒瓶（Gin、Whiskey、Vodka等）

2. **倒酒到 Shaker**
   - 拿着酒瓶，看向 Shaker
   - 按住 `左键` 开始倒酒
   - Shaker 最大容量：800ml

3. **添加多种酒类**
   - 放下当前酒瓶（按 `Q` 或 `E`）
   - 拾取另一种酒瓶
   - 重复倒酒步骤
   - Shaker 会累积所有倒入的液体

4. **从 Shaker 倒出到玻璃杯**
   - 拾取 Shaker（按 `E`）
   - 看向 ServeGlass 或其他玻璃杯
   - 按住 `左键` 开始倒出
   - ⚠️ **注意**：Shaker 必须包含液体才能倒出

### 服务 NPC

1. **准备饮料**
   - 使用 Shaker 或直接用酒瓶倒酒到 ServeGlass
   - 确保 ServeGlass 包含液体

2. **拾取 ServeGlass**
   - 按 `E` 键拾取装有液体的 ServeGlass

3. **靠近 NPC**
   - 拿着 ServeGlass 走近任意 NPC（3米范围内）
   - 会自动显示提示：`按下 F 把酒給 [NPC名字]`

4. **服务饮料**
   - 按 `F` 键将饮料给 NPC
   - 获得 200 金币奖励
   - ServeGlass 会被清空

## 技术实现

### 新增组件

1. **ShakerContainer.cs**
   - 位置：`Assets/Scripts/Objects/ShakerContainer.cs`
   - 功能：管理 Shaker 的液体存储和倒出
   - 属性：
     - `maxShakerVolume`: 800ml
     - `pourRate`: 3.5 ml/s

2. **修改的组件**

   **ImprovedInteractionSystem.cs**
   - 添加了 Shaker 倒酒检测
   - 添加了 NPC 服务提示检测
   - 支持从酒瓶倒到 Shaker
   - 支持从 Shaker 倒到玻璃杯

   **UIPromptManager.cs**
   - 添加了文字轮廓效果
   - 白色文字 + 黑色轮廓（使用 Unity UI Outline 组件）
   - 字体大小：24

### 场景配置

- **Shaker 对象**
  - 添加了 `ShakerContainer` 组件
  - 最大容量：800ml
  - 倒出速率：3.5 ml/s

- **UI_Canvas**
  - `UIPromptManager` 文字颜色改为白色
  - 启用富文本支持
  - 自动添加黑色轮廓效果

## 控制键位

| 按键 | 功能 |
|------|------|
| `E` | 拾取物品 / 放回原位 |
| `Q` | 原地放下物品 |
| `左键（按住）` | 倒酒（拿着酒瓶/Shaker时） |
| `F` | 服务饮料给NPC（拿着ServeGlass靠近NPC时） |

## 重要提示

1. **Shaker 不能直接serve给NPC**
   - 必须先从 Shaker 倒到 ServeGlass
   - 然后用 ServeGlass 服务NPC

2. **Shaker 必须有液体才能倒出**
   - 空的 Shaker 无法倒出
   - 先用酒瓶往 Shaker 倒酒

3. **只有 ServeGlass 可以服务NPC**
   - 其他玻璃杯不会触发服务提示
   - 必须是名为 "ServeGlass" 的对象

4. **NPC 服务距离**
   - 必须在 NPC 3米范围内
   - 提示会自动显示最近的 NPC 名字

## 测试步骤

### 测试 Shaker 功能

1. 拾取 Gin 酒瓶
2. 看向 Shaker，按住左键倒酒
3. 放下 Gin，拾取 Whiskey
4. 继续往 Shaker 倒酒
5. 拾取 Shaker
6. 看向 ServeGlass，按住左键倒出
7. ✅ 验证：ServeGlass 应该包含混合液体

### 测试 NPC 服务

1. 确保 ServeGlass 有液体
2. 拾取 ServeGlass
3. 走近 NPC（Gustave_NPC、Seaton_NPC 或 NPC01）
4. ✅ 验证：应该显示 "按下 F 把酒給 [NPC名字]"
5. 按 F 键
6. ✅ 验证：
   - ServeGlass 被清空
   - 获得 200 金币
   - 控制台显示服务成功消息

### 测试文字样式

1. 触发任何提示文字
2. ✅ 验证：
   - 文字是白色
   - 文字有黑色轮廓
   - 文字清晰可见

## 故障排除

### Shaker 无法倒出
- **原因**：Shaker 是空的
- **解决**：先用酒瓶往 Shaker 倒酒

### 无法拾取 ServeGlass
- **原因**：碰撞器可能被禁用
- **解决**：检查 ServeGlass 的 BoxCollider 是否启用

### NPC 服务提示不显示
- **原因1**：ServeGlass 是空的
- **解决**：先往 ServeGlass 倒酒
- **原因2**：距离太远
- **解决**：走近 NPC（3米内）
- **原因3**：拿的不是 ServeGlass
- **解决**：确保拿的是名为 "ServeGlass" 的对象

### 文字看不清楚
- **原因**：轮廓效果未生效
- **解决**：检查 UIPromptManager 的 enableRichText 是否为 true

## 代码参考

### 从 Shaker 倒酒到玻璃杯
```csharp
// 在 ImprovedInteractionSystem.cs 的 TryPourLiquid() 方法中
var heldShaker = heldObject != null ? heldObject.GetComponent<Objects.ShakerContainer>() : null;
if (heldShaker != null && heldShaker.CanPour())
{
    var glassContainer = currentHighlightedObject.GetComponent<Objects.GlassContainer>();
    if (glassContainer != null && !glassContainer.IsFull())
    {
        float pourAmount = Time.deltaTime * 30f;
        heldShaker.PourToGlass(glassContainer, pourAmount);
    }
}
```

### NPC 服务检测
```csharp
// 在 ImprovedInteractionSystem.cs 的 CheckNPCServing() 方法中
if (heldObject != null && heldObject.name == "ServeGlass")
{
    var glassContainer = heldObject.GetComponent<Objects.GlassContainer>();
    if (glassContainer != null && !glassContainer.IsEmpty())
    {
        // 查找最近的 NPC 并显示提示
    }
}
```

## 更新日志

**2024-12-16**
- ✅ 创建 ShakerContainer 组件
- ✅ 修改 ImprovedInteractionSystem 支持 Shaker 倒酒
- ✅ 添加 NPC 服务提示功能
- ✅ 优化 UIPromptManager 文字样式（白色+黑色轮廓）
- ✅ 修复编译错误
- ✅ 添加 ShakerContainer 到场景中的 Shaker 对象
