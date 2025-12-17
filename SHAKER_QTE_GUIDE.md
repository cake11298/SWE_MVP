# Shaker QTE System Guide

## 系统概述

Shaker QTE（Quick Time Event）系统已完全整合到游戏中。当玩家拾起Shaker并按住右键时，会触发一个10秒的QTE挑战。

## 功能特性

### 1. QTE机制
- **触发条件**: 拾起Shaker后按住右键
- **持续时间**: 10秒
- **技能检查**: 3次
- **成功要求**: 至少成功2次
- **判定区域**: 5-7度的白色区域
- **反应时间**: 从起始到判定区域至少100度

### 2. Shaker动画
- **位置**: 搖晃时Shaker会从右下角移到屏幕中央
- **傾斜**: 15度傾斜角度
- **抖動**: 高频率抖动效果（30Hz）
- **視覺**: 平滑的動畫過渡

### 3. UI显示
- **圓形背景**: 深灰色半透明圓形
- **成功區域**: 綠色扇形區域（隨機位置）
- **指針**: 白色指針順時針旋轉
- **提示文字**: 
  - "Hold Right Mouse Button" - 開始時
  - "Press R!" - 技能檢查時
  - "HIT!" / "MISS!" - 結果反饋
  - "Successful shaken!" / "Shake Failed! Please try again." - 最終結果

### 4. Shaken状态追踪
- **ContainerContents.isShaken**: 追踪容器内容是否已搖晃
- **ShakerContainer.isShaken**: Shaker容器的搖晃状态
- **GlassContainer.isShaken**: 玻璃杯中飲品的搖晃状态
- **自動重置**: 添加新成分後自動重置為unshaken

### 5. 倒酒状态传递
- 從Shaker倒到Glass時，shaken状态會一起傳遞
- NPC可以讀取Glass中的shaken状态進行評分

## 游戏时间设置

- **开始时间**: 22:00:00
- **结束时间**: 24:00:00
- **实际时长**: 5分钟（现实世界）
- **时间流速**: 2游戏小时 = 5现实分钟

## 使用流程

1. **拾起Shaker**
   - 走到Shaker前
   - 按E键拾起
   - ShakerController会自动激活

2. **添加酒水**
   - 拾起酒瓶
   - 对准Shaker按住右键倒酒

3. **开始QTE**
   - 确保Shaker中有酒水
   - 按住右键开始QTE
   - QTE UI会自动显示

4. **完成技能检查**
   - 看到白色指针开始旋转
   - 当指针进入绿色区域时按R键
   - 需要成功2/3次才算成功

5. **查看结果**
   - 成功: "Successful shaken!" 
   - 失败: "Shake Failed! Please try again."
   - 成功後Shaker内容标记为shaken

6. **倒入玻璃杯**
   - 拾起Shaker
   - 对准ServeGlass按住右键倒酒
   - shaken状态会传递到玻璃杯

7. **服务NPC**
   - 拾起玻璃杯
   - 走到NPC前按E键服务
   - NPC会读取shaken状态进行评分

## 技术细节

### 组件结构
```
Shaker GameObject
├── ShakerContainer (追踪液体内容和shaken状态)
├── ShakerQTESystem (处理QTE逻辑)
└── LiquidContainer (处理倒酒视觉效果)

Player GameObject
└── ShakerController (处理玩家输入和动画)

UI_Canvas
└── ShakerQTE_UI (QTE用户界面)
    ├── SkillCheck (技能检查UI)
    │   ├── CircleBackground
    │   ├── SuccessZone
    │   └── Needle
    ├── PromptText
    └── ResultText
```

### 关键脚本
- `ShakerQTESystem.cs`: QTE核心逻辑
- `ShakerQTEUI.cs`: QTE UI显示
- `ShakerController.cs`: 玩家控制和动画
- `ContainerContents.cs`: 容器内容数据（含shaken状态）
- `ShakerContainer.cs`: Shaker容器逻辑
- `GlassContainer.cs`: 玻璃杯容器逻辑

### 事件系统
```csharp
// ShakerQTESystem事件
OnQTEStart - QTE开始
OnQTEEnd - QTE结束
OnQTEComplete(bool success) - QTE完成
OnSkillCheckResult(bool success) - 单次技能检查结果
OnNeedleRotation(float angle) - 指针旋转更新
```

## 调试信息

启用调试日志可以看到：
- "ShakerController: Started shaking with QTE"
- "ShakerQTESystem: Starting skill check X/3"
- "ShakerQTESystem: Skill check HIT/MISSED!"
- "ShakerQTESystem: QTE ended - SUCCESS/FAILED"
- "ShakerController: QTE Success! Shaker contents marked as shaken."
- "ShakerContainer: Poured Xml to glass (Shaken: true/false)"

## 已知限制

1. 目前NPC评分系统尚未完全整合shaken状态的评分逻辑
2. 如果在QTE进行中放下Shaker，QTE会被取消
3. 每次添加新酒水后需要重新搖晃

## 未来改进

1. 添加音效反馈
2. 添加更多视觉特效（粒子、屏幕震动等）
3. 根据shaken状态调整NPC评分
4. 添加不同难度的QTE模式
5. 记录玩家的QTE成功率统计
