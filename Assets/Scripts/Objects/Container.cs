using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Data;
using BarSimulator.Interaction;

namespace BarSimulator.Objects
{
    /// <summary>
    /// 容器基礎類別 - 可以裝液體的物件（杯子、搖酒器）
    /// 參考: CocktailSystem.js containerContents
    /// </summary>
    public class Container : InteractableBase
    {
        #region 序列化欄位

        [Header("容器設定")]
        [Tooltip("最大容量 (ml)")]
        [SerializeField] protected float maxVolume = Constants.GlassMaxVolume;

        [Header("液體視覺")]
        [Tooltip("液體 Mesh Renderer")]
        [SerializeField] protected MeshRenderer liquidRenderer;

        [Tooltip("液體 Transform（用於調整高度）")]
        [SerializeField] protected Transform liquidTransform;

        [Tooltip("液體最大高度")]
        [SerializeField] protected float liquidMaxHeight = 0.55f;

        [Tooltip("液體底部 Y 位置")]
        [SerializeField] protected float liquidBaseY = 0.08f;

        #endregion

        #region 私有欄位

        protected ContainerContents contents;
        protected Material liquidMaterial;

        #endregion

        #region Unity 生命週期

        protected override void Awake()
        {
            base.Awake();

            // 初始化容器內容
            contents = new ContainerContents(maxVolume);

            // 取得液體材質
            if (liquidRenderer != null)
            {
                liquidMaterial = liquidRenderer.material;
                liquidRenderer.enabled = false; // 初始隱藏
            }
        }

        #endregion

        #region 容器操作

        /// <summary>
        /// 添加成分
        /// </summary>
        public virtual void AddIngredient(Ingredient ingredient)
        {
            if (contents.IsFull) return;

            // 限制不超過最大容量
            float actualAmount = Mathf.Min(ingredient.amount, contents.RemainingSpace);
            var adjustedIngredient = new Ingredient(
                ingredient.type,
                ingredient.name,
                ingredient.displayName,
                actualAmount,
                ingredient.color
            );

            contents.AddIngredient(adjustedIngredient);
            UpdateLiquidVisual();
        }

        /// <summary>
        /// 添加酒類
        /// </summary>
        public virtual void AddLiquor(LiquorData liquor, float amount)
        {
            if (contents.IsFull) return;

            float actualAmount = Mathf.Min(amount, contents.RemainingSpace);
            contents.AddLiquor(liquor, actualAmount);
            UpdateLiquidVisual();
        }

        /// <summary>
        /// 清空容器
        /// </summary>
        public virtual void Clear()
        {
            contents.Clear();
            UpdateLiquidVisual();
        }

        /// <summary>
        /// 轉移內容到另一個容器
        /// </summary>
        public virtual float TransferTo(Container target, float amount)
        {
            if (contents.IsEmpty || target.IsFull) return 0f;

            float actualAmount = Mathf.Min(amount, contents.volume, target.RemainingSpace);
            float ratio = actualAmount / contents.volume;

            // 轉移每種成分
            foreach (var ingredient in contents.ingredients)
            {
                float transferAmount = ingredient.amount * ratio;
                target.AddIngredient(new Ingredient(
                    ingredient.type,
                    ingredient.name,
                    ingredient.displayName,
                    transferAmount,
                    ingredient.color
                ));

                ingredient.amount -= transferAmount;
            }

            // 更新體積
            contents.volume -= actualAmount;

            // 清理空成分
            contents.ingredients.RemoveAll(i => i.amount <= 0.01f);

            // 更新顏色
            contents.UpdateMixedColor();

            // 更新視覺
            UpdateLiquidVisual();

            return actualAmount;
        }

        #endregion

        #region 視覺更新

        /// <summary>
        /// 更新液體視覺效果
        /// 參考: CocktailSystem.js updateLiquidVisual() Line 319-358
        /// </summary>
        public virtual void UpdateLiquidVisual()
        {
            if (liquidRenderer == null) return;

            float fillRatio = contents.FillRatio;

            if (fillRatio > 0)
            {
                liquidRenderer.enabled = true;

                // 更新顏色
                if (liquidMaterial != null)
                {
                    liquidMaterial.color = contents.mixedColor;
                }

                // 更新高度
                if (liquidTransform != null)
                {
                    float liquidHeight = Mathf.Max(0.01f, liquidMaxHeight * fillRatio);
                    float yPos = liquidBaseY + liquidHeight / 2f;

                    liquidTransform.localPosition = new Vector3(0f, yPos, 0f);
                    liquidTransform.localScale = new Vector3(
                        liquidTransform.localScale.x,
                        liquidHeight,
                        liquidTransform.localScale.z
                    );
                }
            }
            else
            {
                liquidRenderer.enabled = false;
            }
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 容器內容
        /// </summary>
        public ContainerContents Contents => contents;

        /// <summary>
        /// 當前容量
        /// </summary>
        public float Volume => contents.volume;

        /// <summary>
        /// 最大容量
        /// </summary>
        public float MaxVolume => contents.maxVolume;

        /// <summary>
        /// 剩餘空間
        /// </summary>
        public float RemainingSpace => contents.RemainingSpace;

        /// <summary>
        /// 是否為空
        /// </summary>
        public bool IsEmpty => contents.IsEmpty;

        /// <summary>
        /// 是否已滿
        /// </summary>
        public bool IsFull => contents.IsFull;

        /// <summary>
        /// 填充比例
        /// </summary>
        public float FillRatio => contents.FillRatio;

        /// <summary>
        /// 混合顏色
        /// </summary>
        public Color MixedColor => contents.mixedColor;

        #endregion
    }
}
