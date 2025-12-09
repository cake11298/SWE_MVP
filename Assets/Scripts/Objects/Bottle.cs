using UnityEngine;
using BarSimulator.Core;
using BarSimulator.Data;
using BarSimulator.Interaction;

namespace BarSimulator.Objects
{
    /// <summary>
    /// 酒瓶組件 - 可以倒酒的容器
    /// </summary>
    public class Bottle : InteractableBase
    {
        #region 序列化欄位

        [Header("酒類設定")]
        [Tooltip("酒類 ID")]
        [SerializeField] private string liquorId;

        [Tooltip("酒類資料庫")]
        [SerializeField] private LiquorDatabase liquorDatabase;

        [Header("倒酒設定")]
        [Tooltip("倒酒時的傾斜角度")]
        [SerializeField] private float pourTiltAngle = 60f;

        [Tooltip("傾斜速度")]
        [SerializeField] private float tiltSpeed = 3f;

        [Tooltip("倒酒速度 (ml/s)")]
        [SerializeField] private float pourRate = 20f;

        [Tooltip("倒酒檢測距離")]
        [SerializeField] private float pourCheckDistance = 1.0f;

        [Tooltip("倒酒點 (如果為空則自動尋找 PourPoint 子物件)")]
        [SerializeField] private Transform pourPoint;

        #endregion

        #region 私有欄位

        private LiquorData liquorData;
        private bool isPouring;
        private float currentTilt;
        private Quaternion savedOriginalRotation;

        #endregion

        #region Unity 生命週期

        protected override void Awake()
        {
            base.Awake();

            interactableType = InteractableType.Bottle;
            // Save the original rotation for later restoration
            savedOriginalRotation = transform.rotation;

            if (pourPoint == null)
            {
                var child = transform.Find("PourPoint");
                if (child != null) pourPoint = child;
                else
                {
                    // Create a default pour point at the top
                    GameObject pp = new GameObject("PourPoint");
                    pp.transform.SetParent(transform);
                    pp.transform.localPosition = new Vector3(0, 0.3f, 0); // Approximate top
                    pourPoint = pp.transform;
                }
            }
        }

        private void Start()
        {
            // 載入酒類資料 - deferred to Start to ensure CocktailSystem is initialized
            LoadLiquorData();
        }

        private void Update()
        {
            // 更新傾斜動畫
            UpdateTiltAnimation();

            if (isPouring)
            {
                PerformPouring();
            }
        }

        #endregion

        #region 酒類資料

        /// <summary>
        /// 載入酒類資料
        /// </summary>
        private void LoadLiquorData()
        {
            // 從 CocktailSystem 取得資料庫（避免 Resources.Load 錯誤）
            if (liquorDatabase == null)
            {
                var cocktailSystem = BarSimulator.Systems.CocktailSystem.Instance;
                if (cocktailSystem != null)
                {
                    liquorDatabase = cocktailSystem.LiquorDatabase;
                }
            }

            if (liquorDatabase != null && !string.IsNullOrEmpty(liquorId))
            {
                liquorData = liquorDatabase.GetLiquor(liquorId);
                if (liquorData != null)
                {
                    displayName = liquorData.nameZH;
                }
            }
        }

        /// <summary>
        /// 設定酒類
        /// </summary>
        public void SetLiquor(string id, LiquorDatabase database = null)
        {
            liquorId = id;
            if (database != null)
            {
                liquorDatabase = database;
            }
            LoadLiquorData();
        }

        /// <summary>
        /// 使用 LiquorData 初始化酒瓶
        /// </summary>
        public void Initialize(LiquorData data)
        {
            if (data == null) return;

            liquorData = data;
            liquorId = data.id;
            displayName = data.displayName;

            // 更新視覺顏色
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = data.color;
            }
        }

        #endregion

        #region 倒酒

        /// <summary>
        /// 開始倒酒
        /// </summary>
        public void StartPouring()
        {
            isPouring = true;
        }

        /// <summary>
        /// 停止倒酒
        /// </summary>
        public void StopPouring()
        {
            isPouring = false;
        }

        /// <summary>
        /// 更新傾斜動畫
        /// </summary>
        private void UpdateTiltAnimation()
        {
            float targetTilt = isPouring ? pourTiltAngle : 0f;
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);

            // 應用傾斜（繞 Z 軸）
            transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, currentTilt);
        }

        /// <summary>
        /// 取得倒出的成分
        /// </summary>
        public Ingredient GetPourIngredient(float amount)
        {
            if (liquorData == null) return null;

            return new Ingredient(
                liquorData.id,
                liquorData.nameZH,
                liquorData.displayName,
                amount,
                liquorData.color
            );
        }

        private void PerformPouring()
        {
            if (pourPoint == null || liquorData == null) return;

            // Raycast down from pour point
            RaycastHit hit;
            if (Physics.Raycast(pourPoint.position, Vector3.down, out hit, pourCheckDistance))
            {
                Container container = hit.collider.GetComponent<Container>();
                if (container != null)
                {
                    float amount = pourRate * Time.deltaTime;
                    container.AddLiquor(liquorData, amount);
                    container.SetPouringState(true);
                }
            }
            
            // Debug visualization
            Debug.DrawRay(pourPoint.position, Vector3.down * pourCheckDistance, Color.cyan);
        }

        #endregion

        #region IInteractable 覆寫

        public override void OnUseDown()
        {
            StartPouring();
        }

        public override void OnUseUp()
        {
            StopPouring();
        }

        public override void OnPickup()
        {
            base.OnPickup();
            // Reset for tilt animation when held
            originalRotation = Quaternion.identity;
        }

        public override void OnDrop(bool returnToOriginal)
        {
            base.OnDrop(returnToOriginal);
            isPouring = false;
            currentTilt = 0f;

            if (returnToOriginal)
            {
                // Restore saved original rotation
                originalRotation = savedOriginalRotation;
                transform.rotation = savedOriginalRotation;
            }
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 酒類 ID
        /// </summary>
        public string LiquorId => liquorId;

        /// <summary>
        /// 酒類資料
        /// </summary>
        public LiquorData LiquorData => liquorData;

        /// <summary>
        /// 是否正在倒酒
        /// </summary>
        public bool IsPouring => isPouring;

        /// <summary>
        /// 酒類顏色
        /// </summary>
        public Color LiquorColor => liquorData?.color ?? Color.white;

        #endregion
    }
}
