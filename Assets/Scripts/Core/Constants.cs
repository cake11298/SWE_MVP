using UnityEngine;

namespace BarSimulator.Core
{
    /// <summary>
    /// 遊戲常數定義 - 所有數值參數參考 README 第 5 節
    /// </summary>
    public static class Constants
    {
        // === 玩家參數 ===

        /// <summary>玩家移動速度 (m/s)</summary>
        public const float MoveSpeed = 5f;

        /// <summary>滑鼠敏感度</summary>
        public const float MouseSensitivity = 2.5f;

        /// <summary>垂直視角最小值 (度)</summary>
        public const float MinPitch = -90f;

        /// <summary>垂直視角最大值 (度)</summary>
        public const float MaxPitch = 90f;

        // === 互動參數 ===

        /// <summary>互動距離 (m)</summary>
        public const float InteractionDistance = 3f;

        /// <summary>倒酒有效距離 (m)</summary>
        public const float PourDistance = 4f;

        /// <summary>倒酒有效角度餘弦值 (約45度)</summary>
        public const float PourAngleCos = 0.7f;

        // === 調酒參數 ===

        /// <summary>倒酒速度 (ml/s)</summary>
        public const float PourRate = 30f;

        /// <summary>杯子最大容量 (ml)</summary>
        public const float GlassMaxVolume = 300f;

        /// <summary>Shaker 最大容量 (ml)</summary>
        public const float ShakerMaxVolume = 500f;

        /// <summary>Mixing Glass 最大容量 (ml)</summary>
        public const float MixingGlassMaxVolume = 500f;

        // === 物理參數 ===

        /// <summary>重力加速度</summary>
        public const float Gravity = -9.82f;

        /// <summary>預設摩擦力</summary>
        public const float DefaultFriction = 0.3f;

        /// <summary>預設彈性</summary>
        public const float DefaultBounciness = 0.3f;

        // === 手持物品偏移 ===

        /// <summary>手持物品相對攝影機的偏移位置</summary>
        public static readonly Vector3 HoldOffset = new Vector3(0.3f, -0.3f, 0.6f);

        // === Layer 名稱 ===

        /// <summary>可互動物件 Layer</summary>
        public const string InteractableLayer = "Interactable";

        /// <summary>NPC Layer</summary>
        public const string NPCLayer = "NPC";
    }
}
