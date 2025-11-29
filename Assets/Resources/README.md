# Resources 資料夾結構說明

此資料夾用於存放遊戲運行時動態載入的資源文件（音效、音樂、圖片等）。

## 必要的資料夾結構

請按照以下結構創建資料夾並放置資源文件：

```
Assets/Resources/
├── Audio/
│   ├── Music/
│   │   ├── MainMenu.mp3 (或 .wav)      # 主選單音樂
│   │   └── GameScene.mp3 (或 .wav)     # 遊戲場景音樂
│   ├── Ambience/
│   │   └── BarAmbience.mp3 (或 .wav)   # 酒吧環境音
│   └── SFX/
│       ├── Pour.wav                     # 倒酒音效
│       ├── Shake.wav                    # 搖酒音效
│       ├── Stir.wav                     # 攪拌音效
│       ├── GlassCollision.wav           # 杯子碰撞音效
│       ├── GlassBreak.wav               # 杯子破碎音效
│       ├── Ice.wav                      # 冰塊音效
│       ├── BottleOpen.wav               # 開瓶音效
│       ├── PlaceObject.wav              # 放置物品音效
│       ├── Pickup.wav                   # 拾取物品音效
│       ├── UIClick.wav                  # UI 點擊音效
│       ├── Purchase.wav                 # 購買音效
│       └── Unlock.wav                   # 解鎖音效
└── Sprites/
    └── UI/
        └── (未來可能需要的 UI 圖片)
```

## 說明

1. **音效格式**: 建議使用 `.wav` 格式的音效文件，以獲得最佳兼容性
2. **音樂格式**: 可使用 `.mp3` 或 `.wav` 格式
3. **檔案不存在的處理**: 如果某些音效文件不存在，遊戲仍會繼續運行，但會在 Console 中顯示警告訊息
4. **動態載入**: 所有資源都會在對應系統初始化時通過 `Resources.Load()` 自動載入

## 目前狀態

目前所有資源載入邏輯已經實現在以下文件中：
- `AudioManager.cs` - 負責載入所有音效、音樂和環境音

## 臨時替代方案

如果您暫時沒有音效文件，遊戲仍可正常運行，只是不會播放聲音。您可以：
1. 使用免費音效資源網站（如 freesound.org）下載臨時音效
2. 暫時忽略音效，專注於遊戲邏輯開發
3. 稍後再添加真實的音效文件

## 注意事項

- Unity 的 Resources 資料夾是特殊資料夾，必須命名為 `Resources`
- 路徑區分大小寫（例如 `Audio/SFX/Pour.wav` 和 `audio/sfx/pour.wav` 是不同的）
- Resources.Load() 路徑不需要包含文件副檔名，但文件必須存在副檔名
