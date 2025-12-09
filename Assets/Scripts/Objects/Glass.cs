using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Interaction;

namespace BarSimulator.Objects
{
    /// <summary>
    /// 杯子組件 - 可以裝酒和喝酒的容器
    /// </summary>
    public class Glass : Container
    {
        #region 序列化欄位

        [Header("喝酒設定")]
        [Tooltip("喝酒動畫時間")]
        [SerializeField] private float drinkDuration = 1f;

        [Tooltip("喝酒時的抬起高度")]
        [SerializeField] private float drinkLiftHeight = 0.2f;

        [Tooltip("喝酒時的傾斜角度")]
        [SerializeField] private float drinkTiltAngle = 30f;

        #endregion

        #region 私有欄位

        private bool isDrinking;
        private float drinkStartTime;
        private Vector3 drinkStartPosition;
        private Quaternion drinkStartRotation;

        // 事件
        public System.Action<DrinkInfo> OnDrinkCompleted;

        #endregion

        #region Unity 生命週期

        protected override void Awake()
        {
            base.Awake();
            interactableType = InteractableType.Glass;
            maxVolume = Constants.GlassMaxVolume;
        }

        protected override void Update()
        {
            // 呼叫基礎類別的 Update 以更新動態液體效果
            base.Update();

            // 更新喝酒動畫
            if (isDrinking)
            {
                UpdateDrinkAnimation();
            }
        }

        #endregion

        #region 喝酒

        /// <summary>
        /// 開始喝酒
        /// 參考: CocktailSystem.js drink() Line 773-807
        /// </summary>
        public void StartDrinking()
        {
            if (contents.IsEmpty || isDrinking) return;

            isDrinking = true;
            drinkStartTime = Time.time;
            drinkStartPosition = transform.localPosition;
            drinkStartRotation = transform.localRotation;
        }

        /// <summary>
        /// 更新喝酒動畫
        /// 參考: CocktailSystem.js updateDrinkingAnimation() Line 822-860
        /// </summary>
        private void UpdateDrinkAnimation()
        {
            float elapsed = Time.time - drinkStartTime;
            float progress = elapsed / drinkDuration;

            if (progress < 1f)
            {
                // 動畫進行中
                transform.localPosition = drinkStartPosition + Vector3.up * (drinkLiftHeight * progress);
                transform.localRotation = drinkStartRotation * Quaternion.Euler(-drinkTiltAngle * progress, 0f, 0f);
            }
            else
            {
                // 動畫結束
                transform.localPosition = drinkStartPosition;
                transform.localRotation = drinkStartRotation;

                // 取得飲品資訊
                var drinkInfo = new DrinkInfo
                {
                    volume = contents.volume,
                    color = contents.mixedColor,
                    ingredients = contents.ingredients.ToArray(),
                    cocktailName = "調酒" // 將由 CocktailSystem 識別
                };

                // 清空杯子
                Clear();

                // 觸發事件
                OnDrinkCompleted?.Invoke(drinkInfo);

                isDrinking = false;
            }
        }

        /// <summary>
        /// 直接喝掉（不播放動畫）
        /// </summary>
        public DrinkInfo DrinkImmediately()
        {
            if (contents.IsEmpty) return null;

            var drinkInfo = new DrinkInfo
            {
                volume = contents.volume,
                color = contents.mixedColor,
                ingredients = contents.ingredients.ToArray(),
                cocktailName = "調酒"
            };

            Clear();
            return drinkInfo;
        }

        #endregion

        #region 公開方法

        /// <summary>
        /// 取得飲品資訊
        /// </summary>
        public override DrinkInfo GetDrinkInfo()
        {
            if (contents.IsEmpty) return null;

            return new DrinkInfo
            {
                volume = contents.volume,
                color = contents.mixedColor,
                ingredients = contents.ingredients.ToArray(),
                cocktailName = "Cocktail" // 將由 CocktailSystem 識別
            };
        }

        /// <summary>
        /// 清空杯子
        /// </summary>
        public void Empty()
        {
            Clear();
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 是否正在喝酒
        /// </summary>
        public bool IsDrinking => isDrinking;

        public override void OnUseDown()
        {
            StartDrinking();
        }

        #endregion
    }

    /// <summary>
    /// 飲品資訊
    /// </summary>
    [System.Serializable]
    public class DrinkInfo
    {
        public float volume;
        public Color color;
        public Data.Ingredient[] ingredients;
        public string cocktailName;
    }
}
