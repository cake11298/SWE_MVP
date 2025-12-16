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

        [Header("動態液體效果")]
        [Tooltip("液體高度平滑速度")]
        [SerializeField] protected float heightLerpSpeed = 8f;

        [Tooltip("顏色混合平滑速度")]
        [SerializeField] protected float colorLerpSpeed = 6f;

        [Tooltip("波動衰減速度")]
        [SerializeField] protected float wobbleDecay = 3f;

        [Tooltip("最大波動強度")]
        [SerializeField] protected float maxWobbleIntensity = 0.15f;

        [Tooltip("倒酒時的波動強度")]
        [SerializeField] protected float pourWobbleIntensity = 0.08f;

        #endregion

        #region 私有欄位

        protected ContainerContents contents;
        protected Material liquidMaterial;

        // 溫度系統
        protected float temperature = 25f; // 預設室溫 25°C

        // 動態液體變數
        protected float currentLiquidHeight;
        protected float targetLiquidHeight;
        protected Color currentLiquidColor;
        protected Color targetLiquidColor;
        protected float currentWobbleIntensity;
        protected Vector3 lastPosition;
        protected Vector3 velocity;
        protected float wobbleTime;
        protected bool isPouring;

        // Shader 屬性 ID
        protected static readonly int WobbleXProperty = Shader.PropertyToID("_WobbleX");
        protected static readonly int WobbleZProperty = Shader.PropertyToID("_WobbleZ");
        protected static readonly int FillAmountProperty = Shader.PropertyToID("_FillAmount");
        protected static readonly int WobbleIntensityProperty = Shader.PropertyToID("_WobbleIntensity");

        // Advanced shader properties
        protected static readonly int BubbleIntensityProperty = Shader.PropertyToID("_BubbleIntensity");
        protected static readonly int FoamThicknessProperty = Shader.PropertyToID("_FoamThickness");
        protected static readonly int LayerEnabledProperty = Shader.PropertyToID("_LayerEnabled");
        protected static readonly int Layer2ColorProperty = Shader.PropertyToID("_Layer2Color");
        protected static readonly int Layer2HeightProperty = Shader.PropertyToID("_Layer2Height");
        protected static readonly int RefractionStrengthProperty = Shader.PropertyToID("_RefractionStrength");

        // Liquid type effects
        protected float bubbleIntensity = 0f;
        protected float foamThickness = 0f;
        protected bool hasLayering = false;
        protected Color layer2Color = Color.clear;
        protected float layer2Height = 0.5f;

        #endregion

        #region Unity 生命週期

        protected override void Awake()
        {
            base.Awake();

            // 初始化容器內容
            contents = new ContainerContents(maxVolume);

            // 自動尋找液體子物件（如果未指定）
            if (liquidRenderer == null || liquidTransform == null)
            {
                Transform liquidChild = transform.Find("Liquid");
                if (liquidChild != null)
                {
                    liquidTransform = liquidChild;
                    liquidRenderer = liquidChild.GetComponent<MeshRenderer>();
                    Debug.Log($"Container: Auto-found Liquid child object on {gameObject.name}");
                }
            }

            // 取得液體材質
            if (liquidRenderer != null)
            {
                liquidMaterial = liquidRenderer.material;
                liquidRenderer.enabled = false; // 初始隱藏
            }

            // 初始化動態液體變數
            currentLiquidHeight = 0f;
            targetLiquidHeight = 0f;
            currentLiquidColor = Color.clear;
            targetLiquidColor = Color.clear;
            currentWobbleIntensity = 0f;
            lastPosition = transform.position;
        }

        protected virtual void Update()
        {
            UpdateDynamicLiquid();
            UpdateTemperature();
        }

        /// <summary>
        /// 更新動態液體效果
        /// </summary>
        protected virtual void UpdateDynamicLiquid()
        {
            if (liquidRenderer == null || liquidTransform == null) return;

            // 計算容器移動速度
            Vector3 currentPosition = transform.position;
            velocity = (currentPosition - lastPosition) / Time.deltaTime;
            lastPosition = currentPosition;

            // 根據移動產生波動
            float movementMagnitude = velocity.magnitude;
            if (movementMagnitude > 0.1f)
            {
                currentWobbleIntensity = Mathf.Min(currentWobbleIntensity + movementMagnitude * 0.1f, maxWobbleIntensity);
            }

            // 波動衰減
            currentWobbleIntensity = Mathf.Lerp(currentWobbleIntensity, 0f, wobbleDecay * Time.deltaTime);

            // 平滑插值液體高度
            currentLiquidHeight = Mathf.Lerp(currentLiquidHeight, targetLiquidHeight, heightLerpSpeed * Time.deltaTime);

            // 平滑插值液體顏色
            currentLiquidColor = Color.Lerp(currentLiquidColor, targetLiquidColor, colorLerpSpeed * Time.deltaTime);

            // 更新實際視覺
            ApplyLiquidVisual();

            // 更新波動時間
            wobbleTime += Time.deltaTime;
        }

        /// <summary>
        /// 應用液體視覺效果
        /// </summary>
        protected virtual void ApplyLiquidVisual()
        {
            if (liquidRenderer == null || liquidTransform == null) return;

            if (currentLiquidHeight > 0.001f)
            {
                // 顯示液體物件
                if (!liquidTransform.gameObject.activeSelf)
                    liquidTransform.gameObject.SetActive(true);
                liquidRenderer.enabled = true;

                // 更新材質顏色
                if (liquidMaterial != null)
                {
                    liquidMaterial.color = currentLiquidColor;

                    // 更新 Shader 的液體顏色屬性
                    if (liquidMaterial.HasProperty("_LiquidColor"))
                        liquidMaterial.SetColor("_LiquidColor", currentLiquidColor);

                    // 更新波動效果到 Shader
                    float wobbleX = Mathf.Sin(wobbleTime * 4f) * currentWobbleIntensity;
                    float wobbleZ = Mathf.Cos(wobbleTime * 3.5f) * currentWobbleIntensity;

                    if (liquidMaterial.HasProperty(WobbleXProperty))
                        liquidMaterial.SetFloat(WobbleXProperty, wobbleX);
                    if (liquidMaterial.HasProperty(WobbleZProperty))
                        liquidMaterial.SetFloat(WobbleZProperty, wobbleZ);
                    if (liquidMaterial.HasProperty(FillAmountProperty))
                        liquidMaterial.SetFloat(FillAmountProperty, contents.FillRatio);
                    if (liquidMaterial.HasProperty(WobbleIntensityProperty))
                        liquidMaterial.SetFloat(WobbleIntensityProperty, currentWobbleIntensity);

                    // Advanced shader properties
                    if (liquidMaterial.HasProperty(BubbleIntensityProperty))
                        liquidMaterial.SetFloat(BubbleIntensityProperty, bubbleIntensity);
                    if (liquidMaterial.HasProperty(FoamThicknessProperty))
                        liquidMaterial.SetFloat(FoamThicknessProperty, foamThickness);
                    if (liquidMaterial.HasProperty(LayerEnabledProperty))
                        liquidMaterial.SetFloat(LayerEnabledProperty, hasLayering ? 1f : 0f);
                    if (liquidMaterial.HasProperty(Layer2ColorProperty))
                        liquidMaterial.SetColor(Layer2ColorProperty, layer2Color);
                    if (liquidMaterial.HasProperty(Layer2HeightProperty))
                        liquidMaterial.SetFloat(Layer2HeightProperty, layer2Height);
                }

                // 更新高度和位置
                float yPos = liquidBaseY + currentLiquidHeight / 2f;
                liquidTransform.localPosition = new Vector3(0f, yPos, 0f);
                liquidTransform.localScale = new Vector3(
                    liquidTransform.localScale.x,
                    currentLiquidHeight,
                    liquidTransform.localScale.z
                );
            }
            else
            {
                // 隱藏液體物件
                if (liquidTransform.gameObject.activeSelf)
                    liquidTransform.gameObject.SetActive(false);
                liquidRenderer.enabled = false;
            }
        }

        /// <summary>
        /// 設置氣泡效果（用於啤酒、汽水）
        /// </summary>
        public virtual void SetBubbleEffect(float intensity)
        {
            bubbleIntensity = Mathf.Clamp01(intensity);
        }

        /// <summary>
        /// 設置泡沫效果（用於啤酒）
        /// </summary>
        public virtual void SetFoamEffect(float thickness)
        {
            foamThickness = Mathf.Clamp(thickness, 0f, 0.3f);
        }

        /// <summary>
        /// 設置分層效果（用於雞尾酒）
        /// </summary>
        public virtual void SetLayeringEffect(bool enabled, Color secondColor = default, float height = 0.5f)
        {
            hasLayering = enabled;
            layer2Color = secondColor;
            layer2Height = Mathf.Clamp01(height);
        }

        /// <summary>
        /// 根據飲料類型自動設置效果
        /// </summary>
        public virtual void AutoSetEffectsForDrink(string drinkType)
        {
            // Reset effects
            bubbleIntensity = 0f;
            foamThickness = 0f;
            hasLayering = false;

            // Set effects based on drink type
            switch (drinkType.ToLower())
            {
                case "beer":
                case "lager":
                case "ale":
                    bubbleIntensity = 0.6f;
                    foamThickness = 0.15f;
                    break;

                case "soda":
                case "cola":
                case "sparkling":
                case "champagne":
                    bubbleIntensity = 0.8f;
                    break;

                case "b52":
                case "pousse cafe":
                case "layered":
                    hasLayering = true;
                    // Default brown/orange layers
                    layer2Color = new Color(0.6f, 0.3f, 0.1f, 0.8f);
                    layer2Height = 0.5f;
                    break;
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

            // 轉移溫度（混合溫度）
            if (actualAmount > 0)
            {
                float targetTotalVolume = target.Volume;
                float sourceTemp = this.temperature;
                float targetTemp = target.temperature;

                // 體積加權平均溫度
                target.temperature = (targetTemp * (targetTotalVolume - actualAmount) + sourceTemp * actualAmount) / targetTotalVolume;
            }

            // 轉移shaken狀態
            if (contents.isShaken)
            {
                target.contents.isShaken = true;
            }

            // 清理空成分
            contents.ingredients.RemoveAll(i => i.amount <= 0.01f);

            // 更新顏色
            contents.UpdateMixedColor();

            // 更新視覺
            UpdateLiquidVisual();

            return actualAmount;
        }

        /// <summary>
        /// 倒入目標容器（便捷方法）
        /// </summary>
        public virtual void PourInto(Container target, float amount)
        {
            TransferTo(target, amount);
        }

        /// <summary>
        /// 添加液體（通用方法，用於水等非酒類）
        /// </summary>
        public virtual void AddLiquid(string liquidType, float amount)
        {
            if (contents.IsFull) return;

            float actualAmount = Mathf.Min(amount, contents.RemainingSpace);

            // 根據類型設定顏色
            Color liquidColor = Color.white;
            string displayName = liquidType;

            switch (liquidType.ToLower())
            {
                case "water":
                    liquidColor = new Color(0.9f, 0.95f, 1f, 0.3f);
                    displayName = "Water";
                    break;
                case "ice":
                    liquidColor = new Color(0.85f, 0.92f, 1f, 0.4f);
                    displayName = "Melted Ice";
                    break;
            }

            var ingredient = new Ingredient(
                liquidType,
                liquidType,
                displayName,
                actualAmount,
                liquidColor
            );

            contents.AddIngredient(ingredient);
            UpdateLiquidVisual();
        }

        #endregion

        #region 溫度系統

        /// <summary>
        /// 設定溫度
        /// </summary>
        public virtual void SetTemperature(float temp)
        {
            temperature = Mathf.Clamp(temp, -20f, 100f); // -20°C to 100°C range
        }

        /// <summary>
        /// 獲取溫度
        /// </summary>
        public virtual float GetTemperature()
        {
            return temperature;
        }

        /// <summary>
        /// 溫度自然趨向室溫
        /// </summary>
        protected virtual void UpdateTemperature()
        {
            // 自然趨向室溫 (25°C)
            const float roomTemp = 25f;
            const float coolingRate = 0.5f; // degrees per second

            if (Mathf.Abs(temperature - roomTemp) > 0.1f)
            {
                temperature = Mathf.Lerp(temperature, roomTemp, coolingRate * Time.deltaTime / 10f);
            }
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
                // 設定目標高度和顏色（會通過 Update 平滑插值）
                targetLiquidHeight = Mathf.Max(0.01f, liquidMaxHeight * fillRatio);
                targetLiquidColor = contents.mixedColor;

                // 如果正在倒酒，增加波動
                if (isPouring)
                {
                    currentWobbleIntensity = Mathf.Max(currentWobbleIntensity, pourWobbleIntensity);
                }
            }
            else
            {
                targetLiquidHeight = 0f;
                targetLiquidColor = Color.clear;
            }
        }

        /// <summary>
        /// 設置倒酒狀態
        /// </summary>
        public virtual void SetPouringState(bool pouring)
        {
            isPouring = pouring;
            if (pouring)
            {
                // 開始倒酒時觸發波動
                currentWobbleIntensity = Mathf.Max(currentWobbleIntensity, pourWobbleIntensity);
            }
        }

        /// <summary>
        /// 觸發波動效果（例如被碰撞時）
        /// </summary>
        public virtual void TriggerWobble(float intensity = 0.1f)
        {
            currentWobbleIntensity = Mathf.Min(currentWobbleIntensity + intensity, maxWobbleIntensity);
        }

        /// <summary>
        /// 立即設置液體狀態（跳過插值）
        /// </summary>
        public virtual void SetLiquidImmediate()
        {
            float fillRatio = contents.FillRatio;
            if (fillRatio > 0)
            {
                currentLiquidHeight = Mathf.Max(0.01f, liquidMaxHeight * fillRatio);
                targetLiquidHeight = currentLiquidHeight;
                currentLiquidColor = contents.mixedColor;
                targetLiquidColor = currentLiquidColor;
                ApplyLiquidVisual();
            }
            else
            {
                currentLiquidHeight = 0f;
                targetLiquidHeight = 0f;
                currentLiquidColor = Color.clear;
                targetLiquidColor = Color.clear;
                if (liquidRenderer != null)
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

        /// <summary>
        /// 當前溫度（°C）
        /// </summary>
        public float Temperature => temperature;

        #endregion

        #region 飲料資訊

        /// <summary>
        /// 取得飲料資訊（用於評分）
        /// </summary>
        public virtual DrinkInfo GetDrinkInfo()
        {
            if (contents.IsEmpty) return null;

            var info = new DrinkInfo
            {
                volume = contents.volume,
                color = contents.mixedColor,
                ingredients = contents.ingredients.ToArray()
            };

            // 根據成分識別雞尾酒
            info.cocktailName = RecognizeCocktailName();

            return info;
        }

        /// <summary>
        /// 根據成分識別雞尾酒名稱
        /// </summary>
        protected virtual string RecognizeCocktailName()
        {
            // 簡單識別邏輯
            bool hasGin = false;
            bool hasVodka = false;
            bool hasRum = false;
            bool hasTequila = false;
            bool hasWhiskey = false;
            bool hasVermouth = false;
            bool hasTripleSec = false;
            bool hasCitrus = false;

            foreach (var ingredient in contents.ingredients)
            {
                string name = ingredient.name.ToLower();
                if (name.Contains("gin")) hasGin = true;
                if (name.Contains("vodka")) hasVodka = true;
                if (name.Contains("rum")) hasRum = true;
                if (name.Contains("tequila")) hasTequila = true;
                if (name.Contains("whiskey") || name.Contains("bourbon")) hasWhiskey = true;
                if (name.Contains("vermouth")) hasVermouth = true;
                if (name.Contains("triple sec") || name.Contains("cointreau")) hasTripleSec = true;
                if (name.Contains("lime") || name.Contains("lemon")) hasCitrus = true;
            }

            // 識別雞尾酒
            if (hasGin && hasVermouth) return "Martini";
            if (hasTequila && hasTripleSec && hasCitrus) return "Margarita";
            if (hasRum && hasCitrus) return "Daiquiri";
            if (hasVodka && hasTripleSec) return "Cosmopolitan";
            if (hasWhiskey && hasVermouth) return "Manhattan";
            if (hasWhiskey) return "Whiskey Cocktail";
            if (hasGin) return "Gin Cocktail";
            if (hasVodka) return "Vodka Cocktail";
            if (hasRum) return "Rum Cocktail";

            return "Mixed Drink";
        }

        #endregion
    }
}
