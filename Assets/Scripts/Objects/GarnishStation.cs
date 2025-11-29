using UnityEngine;
using BarSimulator.Interaction;
using System.Collections.Generic;

namespace BarSimulator.Objects
{
    /// <summary>
    /// 裝飾物工作站 - 儲存和提供各種雞尾酒裝飾物
    /// </summary>
    public class GarnishStation : InteractableBase
    {
        #region 序列化欄位

        [Header("Station Setup")]
        [Tooltip("裝飾物預製物列表")]
        [SerializeField] private List<GarnishSlot> garnishSlots = new List<GarnishSlot>();

        [Tooltip("是否自動補充裝飾物")]
        [SerializeField] private bool autoRefill = true;

        [Tooltip("裝飾物生成位置")]
        [SerializeField] private Transform spawnPoint;

        [Header("UI")]
        [Tooltip("顯示面板")]
        [SerializeField] private GameObject displayPanel;

        [Tooltip("選擇指示器")]
        [SerializeField] private Transform selectionIndicator;

        #endregion

        #region 私有欄位

        private int selectedSlotIndex = 0;
        private bool isPlayerNearby = false;

        #endregion

        #region Unity 生命週期

        protected override void Awake()
        {
            base.Awake();

            interactableType = InteractableType.Station;
            displayName = "Garnish Station";
            canPickup = false;
        }

        private void Start()
        {
            // Initialize display
            if (displayPanel != null)
            {
                displayPanel.SetActive(false);
            }

            // Setup spawn point if not assigned
            if (spawnPoint == null)
            {
                GameObject spawnObj = new GameObject("SpawnPoint");
                spawnObj.transform.SetParent(transform);
                spawnObj.transform.localPosition = Vector3.up * 0.15f;
                spawnPoint = spawnObj.transform;
            }

            // Initialize slots
            InitializeGarnishSlots();
        }

        private void Update()
        {
            if (isPlayerNearby && displayPanel != null && displayPanel.activeSelf)
            {
                // Handle slot selection with number keys
                HandleSlotSelection();
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化裝飾物槽位
        /// </summary>
        private void InitializeGarnishSlots()
        {
            // Add default garnishes if list is empty
            if (garnishSlots.Count == 0)
            {
                // These are placeholder references - actual prefabs should be assigned in Unity Editor
                garnishSlots = new List<GarnishSlot>
                {
                    new GarnishSlot { garnishType = GarnishType.LemonSlice, displayName = "Lemon Slice", count = 10 },
                    new GarnishSlot { garnishType = GarnishType.LimeSlice, displayName = "Lime Slice", count = 10 },
                    new GarnishSlot { garnishType = GarnishType.OrangeSlice, displayName = "Orange Slice", count = 10 },
                    new GarnishSlot { garnishType = GarnishType.Cherry, displayName = "Cherry", count = 10 },
                    new GarnishSlot { garnishType = GarnishType.Olive, displayName = "Olive", count = 10 },
                    new GarnishSlot { garnishType = GarnishType.Mint, displayName = "Mint Leaves", count = 10 }
                };
            }
        }

        #endregion

        #region 槽位選擇

        /// <summary>
        /// 處理槽位選擇（數字鍵）
        /// </summary>
        private void HandleSlotSelection()
        {
            // Number keys 1-6 for quick selection
            for (int i = 0; i < Mathf.Min(6, garnishSlots.Count); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectSlot(i);
                }
            }

            // Arrow keys for navigation
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SelectSlot((selectedSlotIndex - 1 + garnishSlots.Count) % garnishSlots.Count);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SelectSlot((selectedSlotIndex + 1) % garnishSlots.Count);
            }

            // Enter or Space to take selected garnish
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                TakeGarnish(selectedSlotIndex);
            }
        }

        /// <summary>
        /// 選擇槽位
        /// </summary>
        private void SelectSlot(int index)
        {
            if (index < 0 || index >= garnishSlots.Count) return;

            selectedSlotIndex = index;

            // Update selection indicator
            if (selectionIndicator != null)
            {
                // Position indicator at selected slot
                // This would typically be done in UI space
                Debug.Log($"GarnishStation: Selected {garnishSlots[index].displayName}");
            }
        }

        #endregion

        #region 取得裝飾物

        /// <summary>
        /// 取得指定槽位的裝飾物
        /// </summary>
        public GameObject TakeGarnish(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= garnishSlots.Count)
            {
                Debug.LogWarning($"GarnishStation: Invalid slot index {slotIndex}");
                return null;
            }

            var slot = garnishSlots[slotIndex];

            // Check if available
            if (!autoRefill && slot.count <= 0)
            {
                Debug.LogWarning($"GarnishStation: No {slot.displayName} available");
                return null;
            }

            // Create garnish instance
            GameObject garnishObj = CreateGarnishInstance(slot);

            if (garnishObj != null)
            {
                // Decrease count if not auto-refill
                if (!autoRefill)
                {
                    slot.count--;
                }

                Debug.Log($"GarnishStation: Took {slot.displayName} (Remaining: {(autoRefill ? "∞" : slot.count.ToString())})");
                return garnishObj;
            }

            return null;
        }

        /// <summary>
        /// 取得選中的裝飾物
        /// </summary>
        public GameObject TakeSelectedGarnish()
        {
            return TakeGarnish(selectedSlotIndex);
        }

        /// <summary>
        /// 創建裝飾物實例
        /// </summary>
        private GameObject CreateGarnishInstance(GarnishSlot slot)
        {
            GameObject garnishObj;

            // If prefab is assigned, instantiate it
            if (slot.garnishPrefab != null)
            {
                garnishObj = Instantiate(slot.garnishPrefab, spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                // Create procedural garnish
                garnishObj = CreateProceduralGarnish(slot.garnishType);
            }

            // Ensure Garnish component exists
            var garnish = garnishObj.GetComponent<Garnish>();
            if (garnish == null)
            {
                garnish = garnishObj.AddComponent<Garnish>();
            }

            return garnishObj;
        }

        /// <summary>
        /// 創建程序化裝飾物（如果沒有預製物）
        /// </summary>
        private GameObject CreateProceduralGarnish(GarnishType type)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = $"Garnish_{type}";
            obj.transform.position = spawnPoint.position;
            obj.transform.localScale = Vector3.one * 0.02f;

            // Set color based on type
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = GetGarnishColor(type);
                renderer.material = mat;
            }

            return obj;
        }

        /// <summary>
        /// 取得裝飾物顏色
        /// </summary>
        private Color GetGarnishColor(GarnishType type)
        {
            switch (type)
            {
                case GarnishType.LemonSlice:
                    return new Color(1f, 0.95f, 0.3f);
                case GarnishType.LimeSlice:
                    return new Color(0.5f, 0.9f, 0.3f);
                case GarnishType.OrangeSlice:
                    return new Color(1f, 0.6f, 0.2f);
                case GarnishType.Cherry:
                    return new Color(0.8f, 0.1f, 0.1f);
                case GarnishType.Olive:
                    return new Color(0.4f, 0.5f, 0.2f);
                case GarnishType.Mint:
                    return new Color(0.2f, 0.8f, 0.3f);
                default:
                    return Color.white;
            }
        }

        #endregion

        #region IInteractable 覆寫

        public override void OnInteract()
        {
            // Open/close garnish selection UI
            if (displayPanel != null)
            {
                bool isActive = displayPanel.activeSelf;
                displayPanel.SetActive(!isActive);

                if (!isActive)
                {
                    Debug.Log("GarnishStation: Opened garnish selection");
                }
                else
                {
                    Debug.Log("GarnishStation: Closed garnish selection");
                }
            }
            else
            {
                // If no UI, just take selected garnish
                TakeSelectedGarnish();
            }
        }

        #endregion

        #region Trigger 偵測

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = false;
                if (displayPanel != null)
                {
                    displayPanel.SetActive(false);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 裝飾物槽位資料
    /// </summary>
    [System.Serializable]
    public class GarnishSlot
    {
        [Tooltip("裝飾物類型")]
        public GarnishType garnishType;

        [Tooltip("顯示名稱")]
        public string displayName;

        [Tooltip("預製物")]
        public GameObject garnishPrefab;

        [Tooltip("數量（-1 為無限）")]
        public int count = 10;

        [Tooltip("圖示（UI用）")]
        public Sprite icon;
    }
}
