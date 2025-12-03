using System.Collections.Generic;
using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Data;
using BarSimulator.Objects;
using BarSimulator.Interaction;

namespace BarSimulator.Systems
{
    /// <summary>
    /// 調酒系統 - 處理倒酒、搖酒、喝酒、雞尾酒識別
    /// 參考: src/modules/CocktailSystem.js
    /// </summary>
    public class CocktailSystem : MonoBehaviour
    {
        #region 單例

        private static CocktailSystem instance;
        public static CocktailSystem Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("資料庫")]
        [Tooltip("酒類資料庫")]
        [SerializeField] private LiquorDatabase liquorDatabase;

        // NOTE: recipeDatabase removed - use static RecipeDatabase class instead

        [Header("倒酒設定")]
        [Tooltip("倒酒速度 (ml/s)")]
        [SerializeField] private float pourRate = Constants.PourRate;

        [Tooltip("倒酒最大距離")]
        [SerializeField] private float pourMaxDistance = Constants.PourDistance;

        [Header("系統引用")]
        [Tooltip("互動系統")]
        [SerializeField] private InteractionSystem interactionSystem;

        [Tooltip("倒酒視覺效果系統")]
        [SerializeField] private PouringSystem pouringSystem;

        #endregion

        #region 私有欄位

        // 倒酒狀態
        private bool isPouringActive;
        private float currentPouringAmount;
        private Bottle currentPouringBottle;
        private Container currentPouringTarget;

        // 事件
        public System.Action<float> OnPourProgress;
        public System.Action<Container, float> OnPourComplete;
        public System.Action<DrinkInfo> OnDrink;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            // 直接建立執行期資料庫 (避免 ExtensionOfNativeClass 錯誤)
            if (liquorDatabase == null)
            {
                Debug.Log("CocktailSystem: 建立執行期 LiquorDatabase");
                liquorDatabase = ScriptableObject.CreateInstance<LiquorDatabase>();
                liquorDatabase.InitializeDefaults();
            }

            // NOTE: RecipeDatabase is now static - no initialization needed
        }

        private void Start()
        {
            if (interactionSystem == null)
            {
                interactionSystem = InteractionSystem.Instance;
            }

            if (pouringSystem == null)
            {
                pouringSystem = PouringSystem.Instance;
            }
        }

        private void Update()
        {
            HandlePouringInput();
            HandleDrinkingInput();
            HandleShakingInput();
        }

        #endregion

        #region 輸入處理

        /// <summary>
        /// 處理倒酒輸入
        /// </summary>
        private void HandlePouringInput()
        {
            if (interactionSystem == null)
            {
                if (Time.frameCount % 300 == 0) Debug.Log("CocktailSystem: InteractionSystem is null");
                return;
            }

            if (!interactionSystem.IsHolding) return;

            var heldType = interactionSystem.GetHeldObjectType();

            // 檢查是否持有酒瓶或搖酒器
            if (heldType != InteractableType.Bottle && heldType != InteractableType.Shaker) return;

            if (interactionSystem.IsLeftClickHeld())
            {
                // Debug: 顯示正在嘗試倒酒
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"CocktailSystem: Trying to pour with {heldType}, searching for container within {pourMaxDistance}m");
                }

                // 尋找目標容器
                var targetTransform = interactionSystem.FindNearbyContainer(
                    interactionSystem.HeldTransform,
                    pourMaxDistance
                );

                if (targetTransform != null)
                {
                    var targetContainer = targetTransform.GetComponent<Container>();
                    if (targetContainer != null && !targetContainer.IsFull)
                    {
                        if (heldType == InteractableType.Bottle)
                        {
                            var bottle = interactionSystem.HeldObject as Bottle;
                            if (bottle != null)
                            {
                                PourFromBottle(bottle, targetContainer, Time.deltaTime);
                            }
                        }
                        else if (heldType == InteractableType.Shaker)
                        {
                            var shaker = interactionSystem.HeldObject as Shaker;
                            if (shaker != null && !shaker.IsEmpty)
                            {
                                PourFromShaker(shaker, targetContainer, Time.deltaTime);
                            }
                        }
                    }
                    else if (targetContainer != null && targetContainer.IsFull)
                    {
                        Debug.Log("CocktailSystem: Target container is full");
                    }
                }
                else if (heldType == InteractableType.Shaker)
                {
                    // 沒有目標容器時，搖酒
                    var shaker = interactionSystem.HeldObject as Shaker;
                    if (shaker != null && !shaker.IsEmpty)
                    {
                        shaker.StartShaking();
                    }
                }
            }
            else
            {
                // 停止倒酒/搖酒
                StopPouring();

                if (heldType == InteractableType.Shaker)
                {
                    var shaker = interactionSystem.HeldObject as Shaker;
                    shaker?.StopShaking();
                }
            }
        }

        /// <summary>
        /// 處理喝酒輸入
        /// </summary>
        private void HandleDrinkingInput()
        {
            if (interactionSystem == null || !interactionSystem.IsHolding) return;

            var heldType = interactionSystem.GetHeldObjectType();
            if (heldType != InteractableType.Glass) return;

            if (interactionSystem.IsRightClickPressed())
            {
                var glass = interactionSystem.HeldObject as Glass;
                if (glass != null && !glass.IsEmpty)
                {
                    glass.OnDrinkCompleted += HandleDrinkCompleted;
                    glass.StartDrinking();
                }
            }
        }

        /// <summary>
        /// 處理搖酒輸入
        /// </summary>
        private void HandleShakingInput()
        {
            // 已整合到 HandlePouringInput
        }

        #endregion

        #region 倒酒

        /// <summary>
        /// 從酒瓶倒酒到容器
        /// 參考: CocktailSystem.js pour() Line 368-463
        /// </summary>
        public void PourFromBottle(Bottle bottle, Container target, float deltaTime)
        {
            if (bottle == null || target == null) return;
            if (bottle.LiquorData == null) return;
            if (target.IsFull) return;

            // 計算倒出量
            float amountPoured = pourRate * deltaTime;
            amountPoured = Mathf.Min(amountPoured, target.RemainingSpace);

            // 添加到容器
            target.AddLiquor(bottle.LiquorData, amountPoured);

            // 更新狀態
            if (!isPouringActive)
            {
                isPouringActive = true;
                currentPouringBottle = bottle;
                currentPouringTarget = target;
                currentPouringAmount = 0f;
                bottle.StartPouring();

                // 設置目標容器的倒酒狀態
                target.SetPouringState(true);

                // 開始倒酒視覺效果
                if (pouringSystem != null)
                {
                    pouringSystem.StartPourEffect(bottle.transform, target.transform, bottle.LiquorData.color);
                }
            }
            else
            {
                // 更新倒酒視覺效果位置
                if (pouringSystem != null)
                {
                    pouringSystem.UpdatePourEffect(bottle.transform, target.transform);
                }
            }

            currentPouringAmount += amountPoured;
            OnPourProgress?.Invoke(currentPouringAmount);
        }

        /// <summary>
        /// 從搖酒器倒酒到容器
        /// 參考: CocktailSystem.js pourFromShaker() Line 503-586
        /// </summary>
        public void PourFromShaker(Shaker shaker, Container target, float deltaTime)
        {
            if (shaker == null || target == null) return;
            if (shaker.IsEmpty || target.IsFull) return;

            // 計算倒出量
            float amountPoured = shaker.TransferTo(target, pourRate * deltaTime);

            // 更新狀態
            if (!isPouringActive)
            {
                isPouringActive = true;
                currentPouringTarget = target;
                currentPouringAmount = 0f;
                shaker.StartPouring();

                // 設置目標容器的倒酒狀態
                target.SetPouringState(true);
            }

            currentPouringAmount += amountPoured;
            OnPourProgress?.Invoke(currentPouringAmount);
        }

        /// <summary>
        /// 停止倒酒
        /// </summary>
        public void StopPouring()
        {
            if (!isPouringActive) return;

            if (currentPouringBottle != null)
            {
                currentPouringBottle.StopPouring();
            }

            // 停止目標容器的倒酒狀態
            if (currentPouringTarget != null)
            {
                currentPouringTarget.SetPouringState(false);
            }

            // 停止倒酒視覺效果
            if (pouringSystem != null)
            {
                pouringSystem.StopPourEffect();
            }

            OnPourComplete?.Invoke(currentPouringTarget, currentPouringAmount);

            isPouringActive = false;
            currentPouringBottle = null;
            currentPouringTarget = null;
            currentPouringAmount = 0f;
        }

        #endregion

        #region 喝酒

        /// <summary>
        /// 處理喝酒完成
        /// </summary>
        private void HandleDrinkCompleted(DrinkInfo drinkInfo)
        {
            if (drinkInfo == null) return;

            // 識別雞尾酒
            drinkInfo.cocktailName = IdentifyCocktail(drinkInfo.ingredients);

            Debug.Log($"CocktailSystem: 喝了 {drinkInfo.cocktailName}，容量 {drinkInfo.volume:F0}ml");

            OnDrink?.Invoke(drinkInfo);
        }

        #endregion

        #region 雞尾酒識別

        /// <summary>
        /// 識別雞尾酒
        /// 參考: CocktailSystem.js identifyCocktail() Line 867-979
        /// </summary>
        public string IdentifyCocktail(Ingredient[] ingredients)
        {
            if (ingredients == null || ingredients.Length == 0)
                return "Empty Glass";

            var types = new List<string>();
            var amounts = new Dictionary<string, float>();

            foreach (var ing in ingredients)
            {
                if (!types.Contains(ing.type))
                    types.Add(ing.type);
                amounts[ing.type] = ing.amount;
            }

            // === Classic Cocktail Identification ===

            // Martini: Gin + Dry Vermouth (2:1 to 3:1)
            if (types.Contains("gin") && types.Contains("vermouth_dry"))
            {
                float ratio = amounts["gin"] / amounts["vermouth_dry"];
                if (ratio >= 2f && ratio <= 3f && !HasOtherSpirits(types, "gin"))
                {
                    return "Martini";
                }
            }

            // Vodka Martini
            if (types.Contains("vodka") && types.Contains("vermouth_dry"))
            {
                float ratio = amounts["vodka"] / amounts["vermouth_dry"];
                if (ratio >= 2f && ratio <= 3f && !HasOtherSpirits(types, "vodka"))
                {
                    return "Vodka Martini";
                }
            }

            // Negroni: Gin + Campari + Sweet Vermouth (1:1:1)
            if (types.Contains("gin") && types.Contains("campari") && types.Contains("vermouth_sweet"))
            {
                float avg = (amounts["gin"] + amounts["campari"] + amounts["vermouth_sweet"]) / 3f;
                bool isBalanced = IsWithinRatio(amounts["gin"], avg, 0.3f) &&
                                  IsWithinRatio(amounts["campari"], avg, 0.3f) &&
                                  IsWithinRatio(amounts["vermouth_sweet"], avg, 0.3f);
                if (isBalanced)
                {
                    return "Negroni";
                }
            }

            // Margarita: Tequila + Triple Sec + Lime Juice
            if (types.Contains("tequila") && types.Contains("triple_sec") && types.Contains("lime_juice"))
            {
                return "Margarita";
            }

            // Daiquiri: Rum + Lime Juice + Simple Syrup
            if (types.Contains("rum") && types.Contains("lime_juice") && types.Contains("simple_syrup"))
            {
                return "Daiquiri";
            }

            // Pina Colada: Rum + Pineapple Juice + Coconut Cream
            if (types.Contains("rum") && types.Contains("pineapple_juice") && types.Contains("coconut_cream"))
            {
                return "Pina Colada";
            }

            // Cosmopolitan: Vodka + Triple Sec + Cranberry + Lime
            if (types.Contains("vodka") && types.Contains("triple_sec") &&
                types.Contains("cranberry_juice") && types.Contains("lime_juice"))
            {
                return "Cosmopolitan";
            }

            // Mojito: Rum + Lime Juice + Simple Syrup
            if (types.Contains("rum") && types.Contains("lime_juice") && types.Contains("simple_syrup"))
            {
                return "Mojito";
            }

            // Whiskey Sour
            if (types.Contains("whiskey") && types.Contains("lemon_juice") && types.Contains("simple_syrup"))
            {
                return "Whiskey Sour";
            }

            // === Simple Classification ===
            if (types.Count == 1)
            {
                var liquor = liquorDatabase?.GetLiquor(types[0]);
                return liquor != null ? $"{liquor.displayName} Neat" : "Single Spirit";
            }
            else if (types.Count == 2)
            {
                return "Two-Ingredient Mix";
            }
            else
            {
                return "Custom Cocktail";
            }
        }

        /// <summary>
        /// 檢查是否有其他基酒
        /// </summary>
        private bool HasOtherSpirits(List<string> types, string exclude)
        {
            string[] spirits = { "vodka", "gin", "rum", "whiskey", "tequila", "brandy", "campari" };
            foreach (var spirit in spirits)
            {
                if (spirit != exclude && types.Contains(spirit))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 檢查數值是否在比例範圍內
        /// </summary>
        private bool IsWithinRatio(float value, float target, float tolerance)
        {
            if (target == 0) return false;
            return Mathf.Abs(value - target) / target < tolerance;
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 酒類資料庫
        /// </summary>
        public LiquorDatabase LiquorDatabase => liquorDatabase;

        // NOTE: RecipeDatabase property removed - use static BarSimulator.Data.RecipeDatabase instead

        /// <summary>
        /// 是否正在倒酒
        /// </summary>
        public bool IsPouringActive => isPouringActive;

        /// <summary>
        /// 當前倒酒量
        /// </summary>
        public float CurrentPouringAmount => currentPouringAmount;

        #endregion

        #region 公開方法

        /// <summary>
        /// 取得酒類資料
        /// </summary>
        public LiquorData GetLiquorData(string id)
        {
            return liquorDatabase?.GetLiquor(id);
        }

        /// <summary>
        /// 計算容器的酒精濃度
        /// </summary>
        public float CalculateAlcoholContent(Container container)
        {
            return container?.Contents?.CalculateAlcoholContent(liquorDatabase) ?? 0f;
        }

        #endregion
    }
}
