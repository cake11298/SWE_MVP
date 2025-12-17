using UnityEngine;

namespace BarSimulator.Player
{
    /// <summary>
    /// 物品类型
    /// </summary>
    public enum ItemType
    {
        Bottle,  // 酒瓶
        Glass,   // 玻璃杯
        Shaker,  // 摇酒器
        Other    // 其他
    }

    /// <summary>
    /// 液体类型
    /// </summary>
    public enum LiquidType
    {
        None,
        Vodka,
        Whiskey,
        Rum,
        Gin,
        Tequila,
        Wine,
        Beer,
        Juice,
        Soda
    }

    /// <summary>
    /// 可互动物品组件
    /// 附加到所有可以被拾取和互动的物品上
    /// </summary>
    public class InteractableItem : MonoBehaviour
    {
        [Header("物品设置")]
        public ItemType itemType = ItemType.Other;
        public string itemName = "Item";

        [Header("酒瓶设置（仅当itemType为Bottle时）")]
        public LiquidType liquidType = LiquidType.None;
        public float liquidAmount = 750f; // ml
        public float maxLiquidAmount = 750f; // ml

        [Header("玻璃杯设置（仅当itemType为Glass时）")]
        public float currentLiquidAmount = 0f; // ml
        public float maxGlassCapacity = 300f; // ml

        [Header("视觉反馈")]
        [SerializeField] private GameObject liquidVisual; // 液体的视觉表现（可选）

        private bool isPickedUp = false;

        /// <summary>
        /// 当物品被拾取时调用
        /// </summary>
        public void OnPickedUp()
        {
            isPickedUp = true;
            Debug.Log($"{itemName} 被拾取");
        }

        /// <summary>
        /// 当物品被放下时调用
        /// </summary>
        public void OnDropped()
        {
            isPickedUp = false;
            Debug.Log($"{itemName} 被放下");
        }

        /// <summary>
        /// 接收液体（用于玻璃杯）
        /// </summary>
        /// <param name="liquid">液体类型</param>
        /// <param name="amount">液体量（ml）</param>
        public void OnReceiveLiquid(LiquidType liquid, float amount)
        {
            if (itemType != ItemType.Glass)
                return;

            // 添加液体
            float spaceLeft = maxGlassCapacity - currentLiquidAmount;
            float amountToAdd = Mathf.Min(amount, spaceLeft);
            currentLiquidAmount += amountToAdd;

            Debug.Log($"{itemName} 接收了 {amountToAdd}ml 的 {liquid}，当前总量: {currentLiquidAmount}ml");

            // 更新液体视觉效果
            UpdateLiquidVisual();
        }

        /// <summary>
        /// 倒出液体（用于酒瓶）
        /// </summary>
        /// <param name="amount">要倒出的量（ml）</param>
        /// <returns>实际倒出的量</returns>
        public float PourLiquid(float amount)
        {
            if (itemType != ItemType.Bottle)
                return 0f;

            float amountToPour = Mathf.Min(amount, liquidAmount);
            liquidAmount -= amountToPour;

            Debug.Log($"{itemName} 倒出了 {amountToPour}ml，剩余: {liquidAmount}ml");

            return amountToPour;
        }

        /// <summary>
        /// 更新液体视觉效果
        /// </summary>
        private void UpdateLiquidVisual()
        {
            if (liquidVisual == null)
                return;

            // 根据液体量调整视觉效果
            // 例如：调整液体的高度或透明度
            float fillPercentage = currentLiquidAmount / maxGlassCapacity;
            
            // 简单的缩放示例
            Vector3 scale = liquidVisual.transform.localScale;
            scale.y = fillPercentage;
            liquidVisual.transform.localScale = scale;
        }

        /// <summary>
        /// 检查是否为空瓶
        /// </summary>
        public bool IsEmpty()
        {
            if (itemType == ItemType.Bottle)
                return liquidAmount <= 0f;
            return false;
        }

        /// <summary>
        /// 检查杯子是否已满
        /// </summary>
        public bool IsFull()
        {
            if (itemType == ItemType.Glass)
                return currentLiquidAmount >= maxGlassCapacity;
            return false;
        }

        private void OnValidate()
        {
            // 在编辑器中自动设置物品名称
            if (string.IsNullOrEmpty(itemName))
            {
                itemName = gameObject.name;
            }
        }
    }
}
