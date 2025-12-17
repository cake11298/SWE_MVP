using UnityEngine;
using BarSimulator.UI;
using BarSimulator.QTE;

namespace BarSimulator.Player
{
    /// <summary>
    /// 改进的物品互动系统
    /// - E键：放回原位
    /// - Q键：原地放下
    /// - 左键：倒酒（手持酒瓶时）
    /// - 物品显示在右下角（右手位置）
    /// </summary>
    public class ImprovedInteractionSystem : MonoBehaviour
    {
        [Header("互动设置")]
        [SerializeField] private float interactionDistance = 2f;
        [SerializeField] private float pourDistance = 1.5f;

        [Header("手持位置（右手）")]
        [SerializeField] private Vector3 handOffset = new Vector3(0.4f, -0.4f, 0.6f);

        [Header("高亮设置")]
        [SerializeField] private Color highlightColor = Color.yellow;

        [Header("UI引用")]
        [SerializeField] private UI.LiquidInfoUI liquidInfoUI;
        [SerializeField] private UI.ShakerInfoUI shakerInfoUI;

        private Camera playerCamera;
        private Transform handPosition;
        
        // 当前状态
        private GameObject currentHighlightedObject;
        private GameObject heldObject;
        private InteractableItem heldItem;
        
        // 原始信息（用于放回原位）
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Transform originalParent;
        
        // 高亮材质
        private Renderer currentHighlightedRenderer;
        private Material[] originalMaterials;

        // 搖酒狀態
        private bool isShaking = false;

        /// <summary>
        /// 當前手持的物件
        /// </summary>
        public GameObject HeldObject => heldObject;

        private void Awake()
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            // 创建手持位置
            GameObject handObj = new GameObject("HandPosition");
            handObj.transform.SetParent(playerCamera.transform);
            handObj.transform.localPosition = handOffset;
            handPosition = handObj.transform;

            // 查找LiquidInfoUI
            if (liquidInfoUI == null)
            {
                liquidInfoUI = FindObjectOfType<UI.LiquidInfoUI>();
            }

            // 查找ShakerInfoUI
            if (shakerInfoUI == null)
            {
                shakerInfoUI = FindObjectOfType<UI.ShakerInfoUI>();
            }
        }

        private void Update()
        {
            // 检测可互动物品
            DetectInteractableObjects();

            // 处理输入
            HandleInput();

            // 更新手持物品位置
            if (heldObject != null)
            {
                UpdateHeldObjectPosition();
            }

            // 更新UI显示
            UpdateLiquidUI();
            
            // 检测NPC服务提示
            CheckNPCServing();
        }

        private void DetectInteractableObjects()
        {
            // 如果手上有物品
            if (heldObject != null)
            {
                // 如果手持酒瓶，检测玻璃杯
                if (heldItem != null && heldItem.itemType == ItemType.Bottle || heldItem.itemType == ItemType.Shaker)
                {
                    DetectGlassForPouring();
                }
                else
                {
                    ClearHighlight();
                }
                return;
            }

            // 检测可拾取的物品
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                InteractableItem item = hit.collider.GetComponent<InteractableItem>();
                if (item != null && item.enabled)
                {
                    HighlightObject(hit.collider.gameObject);
                    return;
                }
            }

            ClearHighlight();
        }

        private void DetectGlassForPouring()
        {
            // 从手持位置发射射线检测玻璃杯或Shaker
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, pourDistance))
            {
                InteractableItem item = hit.collider.GetComponent<InteractableItem>();

                // 如果手持酒瓶，检测玻璃杯或Shaker
                if (heldItem != null && heldItem.itemType == ItemType.Bottle)
                {
                    // 检测玻璃杯
                    if (item != null && item.itemType == ItemType.Glass)
                    {
                        HighlightObject(hit.collider.gameObject);
                        return;
                    }
                    
                    // 检测Shaker（可以接收酒水）
                    var shakerContainer = hit.collider.GetComponent<Objects.ShakerContainer>();
                    var shaker = hit.collider.GetComponent<Objects.Shaker>();
                    if (shakerContainer != null || shaker != null)
                    {
                        HighlightObject(hit.collider.gameObject);
                        return;
                    }
                }
                
                // 如果手持Shaker，检测玻璃杯
                if (heldItem != null && heldItem.itemType == ItemType.Shaker)
                {
                    // 检测玻璃杯
                    if (item != null && item.itemType == ItemType.Glass)
                    {
                        HighlightObject(hit.collider.gameObject);
                        return;
                    }
                }
            }

            ClearHighlight();
        }

        private void HighlightObject(GameObject obj)
        {
            if (currentHighlightedObject == obj)
                return;

            ClearHighlight();

            currentHighlightedObject = obj;
            currentHighlightedRenderer = obj.GetComponent<Renderer>();

            if (currentHighlightedRenderer != null)
            {
                originalMaterials = currentHighlightedRenderer.materials;

                Material[] newMaterials = new Material[originalMaterials.Length];
                for (int i = 0; i < originalMaterials.Length; i++)
                {
                    newMaterials[i] = new Material(originalMaterials[i]);
                    newMaterials[i].EnableKeyword("_EMISSION");
                    newMaterials[i].SetColor("_EmissionColor", highlightColor * 0.5f);
                }
                currentHighlightedRenderer.materials = newMaterials;
            }

            // Show interaction prompt
            ShowInteractionPrompt(obj);
        }

        private void ClearHighlight()
        {
            if (currentHighlightedObject != null && currentHighlightedRenderer != null && originalMaterials != null)
            {
                currentHighlightedRenderer.materials = originalMaterials;
                originalMaterials = null;
            }

            currentHighlightedObject = null;
            currentHighlightedRenderer = null;

            // Hide interaction prompt
            UIPromptManager.Hide();
        }

        private void HandleInput()
        {
            // 按下E键：切換播放/拾取或放回原位
            if (Input.GetKeyDown(KeyCode.E))
            {
                if(currentHighlightedObject?.GetComponent<InteractableItem>().itemType == ItemType.Speaker)
                {
                    // 切換音樂播放
                    var speaker = currentHighlightedObject.GetComponent<Objects.Speaker>();

                    speaker.TogglePlay();
                    Debug.Log("[HandleInput] 切換音樂播放狀態");
                }
                else if (heldObject == null)
                {
                    TryPickupObject();
                }
                else
                {
                    ReturnToOriginalPosition();
                }
            }

            // 按下Q键：輪換歌曲/原地放下
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (currentHighlightedObject?.GetComponent<InteractableItem>().itemType == ItemType.Speaker)
                {
                    // 輪換歌曲
                    var speaker = currentHighlightedObject.GetComponent<Objects.Speaker>();
                    speaker.SwitchMusic();
                    Debug.Log("[HandleInput] 輪換歌曲");
                    return;
                }
                else if(heldObject != null)
                {
                    DropAtCurrentPosition();
                }
            }

            // 按下左键：減小音量/倒酒
            if(Input.GetMouseButton(0))
            {
                if(currentHighlightedObject?.GetComponent<InteractableItem>().itemType == ItemType.Speaker)
                {
                    // 減小音量
                    var speaker = currentHighlightedObject.GetComponent<Objects.Speaker>();
                    speaker.SetVolume(speaker.GetComponent<AudioSource>().volume - 0.01f);
                    Debug.Log("[HandleInput] 減小音量");
                }
                else if (heldObject != null && heldItem != null && (heldItem.itemType == ItemType.Bottle || heldItem.itemType == ItemType.Shaker))
                {
                    TryPourLiquid();
                }
            }


            // 按住右鍵: 增加音量
            if (Input.GetMouseButton(1) && currentHighlightedObject?.GetComponent<InteractableItem>().itemType == ItemType.Speaker)
            {
                var speaker = currentHighlightedObject.GetComponent<Objects.Speaker>();
                speaker.SetVolume(speaker.GetComponent<AudioSource>().volume + 0.01f);
                Debug.Log("[HandleInput] 增加音量");
            }
            // 按下右鍵：開始搖酒
            else if (Input.GetMouseButtonDown(1) && heldObject != null && heldItem != null && heldItem.itemType == ItemType.Shaker)
            {
                // 嘗試獲取 Shaker 組件 (優先) 或 ShakerContainer
                var shaker = heldObject.GetComponent<Objects.Shaker>();
                var shakerContainer = heldObject.GetComponent<Objects.ShakerContainer>();

                if (shaker != null && QTEManager.Instance != null)
                {
                    Debug.Log("[HandleInput] 搖晃 Shaker (New)");
                    // 啟動 Shaker 的視覺效果 (不使用計時器)
                    shaker.StartShaking(false);
                    
                    // 啟動 QTE，並傳入回調
                    QTEManager.Instance.StartShakeQTE((quality) => {
                        if (shaker != null)
                        {
                            shaker.OnShakeFinished(quality);
                        }
                    });
                    isShaking = true;
                }
                else if (shakerContainer != null && QTEManager.Instance != null)
                {
                    Debug.Log("[HandleInput] 搖晃 ShakerContainer (Legacy)");
                    QTEManager.Instance.StartShakeQTE();
                    isShaking = true;
                }
                else
                {
                    Debug.Log("[HandleInput] 無法搖晃: 缺少 Shaker 組件或 QTEManager");
                }
            }
            

            // 鬆開右鍵: 停止搖酒
            if (Input.GetMouseButtonUp(1) && heldObject != null && heldItem != null)
            {
                if(heldItem.itemType == ItemType.Shaker)
                {
                    var shaker = heldObject.GetComponent<Objects.Shaker>();
                    
                    if (shaker != null)
                    {
                        Debug.Log("[HandleInput] 停止搖晃 Shaker");
                        isShaking = false;
                        // 注意：這裡不呼叫 shaker.StopShaking()，因為我們希望 QTE 失敗或中斷時由 QTEManager 處理
                        // 或者如果 QTE 還沒完成就鬆開，視為中斷
                        if (QTEManager.Instance != null)
                        {
                            QTEManager.Instance.StopShakeQTE();
                        }
                        shaker.StopShaking();
                    }
                    else
                    {
                        // 舊版兼容
                        var shakerContainer = heldObject.GetComponent<Objects.ShakerContainer>();
                        if (shakerContainer != null && QTEManager.Instance != null)
                        {
                            Debug.Log("[HandleInput] 中斷 ShakerContainer (Legacy)");
                            QTEManager.Instance.StopShakeQTE();
                            isShaking = false;
                        }
                    }
                }
            }
        }

        private void TryPickupObject()
        {
            if (currentHighlightedObject == null)
                return;

            InteractableItem item = currentHighlightedObject.GetComponent<InteractableItem>();
            if (item == null || !item.enabled)
                return;

            // 保存原始信息
            originalPosition = currentHighlightedObject.transform.position;
            originalRotation = currentHighlightedObject.transform.rotation;
            originalParent = currentHighlightedObject.transform.parent;

            // 拾取物品
            heldObject = currentHighlightedObject;

            // Show pickup prompt
            string itemName = GetFriendlyName(heldObject);
            UIPromptManager.Show($"拾取了 {itemName}");
            heldItem = item;

            // 禁用物理
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 先停止運動再設置 kinematic
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // 禁用所有碰撞器（包括父物件和子物件）
            Collider[] allColliders = heldObject.GetComponentsInChildren<Collider>();
            
            foreach (Collider col in allColliders)
            {
                col.enabled = false;
            }

            // 清除高亮
            ClearHighlight();

            // Notify StaticProp component if exists
            var staticProp = heldObject.GetComponent<StaticProp>();
            if (staticProp != null)
            {
                staticProp.OnPickup();
            }

            // 通知物品被拾取
            item.OnPickedUp();

            Debug.Log($"拾取了: {heldObject.name}，位置: {heldObject.transform.position}");
            Debug.Log($"類別: {item.itemType}");
        }

        private void ReturnToOriginalPosition()
        {
            if (heldObject == null)
                return;

            // 恢复父物件
            heldObject.transform.SetParent(originalParent);

            // 恢复位置和旋转
            heldObject.transform.position = originalPosition;
            heldObject.transform.rotation = originalRotation;

            // 恢复物理
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 恢復所有碰撞器（包括父物件和子物件）
            Collider[] allColliders = heldObject.GetComponentsInChildren<Collider>(true);
            foreach (Collider col in allColliders)
            {
                // 只恢復父物件的主碰撞器，子物件的碰撞器保持禁用
                if (col.gameObject == heldObject)
                {
                    col.enabled = true;
                }
            }

            // Notify StaticProp component if exists
            var staticProp = heldObject.GetComponent<StaticProp>();
            if (staticProp != null)
            {
                staticProp.OnDrop();
            }

            // 通知物品被放下
            if (heldItem != null)
            {
                heldItem.OnDropped();
            }

            Debug.Log($"放回原位: {heldObject.name}，位置: {heldObject.transform.position}");

            heldObject = null;
            heldItem = null;
        }

        private void DropAtCurrentPosition()
        {
            if (heldObject == null)
                return;

            // 在当前位置放下
            Vector3 dropPosition = playerCamera.transform.position + playerCamera.transform.forward * 1f;
            heldObject.transform.position = dropPosition;

            // 恢复物理
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 恢復所有碰撞器（包括父物件和子物件）
            Collider[] allColliders = heldObject.GetComponentsInChildren<Collider>(true);
            foreach (Collider col in allColliders)
            {
                // 只恢復父物件的主碰撞器
                if (col.gameObject == heldObject)
                {
                    col.enabled = true;
                }
            }

            // Notify StaticProp component if exists
            var staticProp = heldObject.GetComponent<StaticProp>();
            if (staticProp != null)
            {
                staticProp.OnDrop();
            }

            // 通知物品被放下
            if (heldItem != null)
            {
                heldItem.OnDropped();
            }

            Debug.Log($"原地放下: {heldObject.name}，位置: {heldObject.transform.position}");

            heldObject = null;
            heldItem = null;
        }

        private void UpdateHeldObjectPosition()
        {
            if (heldObject != null && handPosition != null)
            {
                if (isShaking)
                {
                    // 搖晃時移到畫面中間並上下移動
                    Vector3 centerPos = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
                    // 降低一點高度，避免擋住視線太嚴重
                    centerPos.y -= 0.2f;
                    
                    // 上下搖晃動畫
                    float shakeY = Mathf.Sin(Time.time * 15f) * 0.1f;
                    centerPos.y += shakeY;
                    
                    heldObject.transform.position = centerPos;
                    
                    // 稍微隨機旋轉增加動感
                    float shakeRot = Mathf.Sin(Time.time * 20f) * 5f;
                    heldObject.transform.rotation = Quaternion.Euler(shakeRot, 0, shakeRot) * playerCamera.transform.rotation;
                }
                else
                {
                    // 正常手持位置
                    heldObject.transform.position = handPosition.position;
                    heldObject.transform.rotation = handPosition.rotation;
                }
            }
        }

        private void TryPourLiquid()
        {
            if (currentHighlightedObject == null)
                return;   

            // 检查是否手持酒瓶
            if (heldItem != null && heldItem.itemType == ItemType.Bottle)
            {
                // 倒入玻璃杯
                InteractableItem targetItem = currentHighlightedObject.GetComponent<InteractableItem>();
                if (targetItem != null && targetItem.itemType == ItemType.Glass)
                {
                    // 再次检查 heldItem 是否仍然有效
                    if (heldItem == null) return;
                    
                    // 获取酒的类型 - 优先使用LiquidContainer的liquidName
                    string liquidName = heldItem.liquidType.ToString();
                    var liquidContainer = heldObject.GetComponent<Objects.LiquidContainer>();
                    if (liquidContainer != null && !string.IsNullOrEmpty(liquidContainer.liquidName))
                    {
                        liquidName = liquidContainer.liquidName;
                    }
                    
                    Debug.Log($"正在倒酒，倒的是: {liquidName}");

                    // 倒酒到杯子
                    targetItem.OnReceiveLiquid(heldItem.liquidType, Time.deltaTime * 30f);

                    // 優先嘗試新的 Glass 組件
                    var glass = currentHighlightedObject.GetComponent<Objects.Glass>();
                    if (glass != null)
                    {
                        glass.AddLiquid(liquidName, Time.deltaTime * 30f);
                    }

                    // 同时倒入GlassContainer（如果存在）
                    var glassContainer = currentHighlightedObject.GetComponent<Objects.GlassContainer>();
                    if (glassContainer != null)
                    {
                        float pourAmount = Time.deltaTime * 30f; // 30ml/s
                        glassContainer.AddLiquid(liquidName, pourAmount);
                    }
                    return;
                }
                
                // 倒入Shaker (ShakerContainer)
                var shakerContainer = currentHighlightedObject.GetComponent<Objects.ShakerContainer>();
                if (shakerContainer != null && !shakerContainer.IsFull())
                {
                    // 再次检查 heldItem 是否仍然有效
                    if (heldItem == null) return;
                    
                    // 获取酒的类型 - 优先使用LiquidContainer的liquidName
                    string liquidName = heldItem.liquidType.ToString();
                    var liquidContainer = heldObject.GetComponent<Objects.LiquidContainer>();
                    if (liquidContainer != null && !string.IsNullOrEmpty(liquidContainer.liquidName))
                    {
                        liquidName = liquidContainer.liquidName;
                    }
                    
                    float pourAmount = Time.deltaTime * 30f; // 30ml/s
                    shakerContainer.AddLiquid(liquidName, pourAmount);
                    Debug.Log($"正在倒酒到ShakerContainer: {liquidName}");
                    return;
                }

                // 倒入Shaker (Shaker component)
                var shaker = currentHighlightedObject.GetComponent<Objects.Shaker>();
                if (shaker != null && !shaker.IsFull)
                {
                    if (heldItem == null) return;

                    string liquidName = heldItem.liquidType.ToString();
                    var liquidContainer = heldObject.GetComponent<Objects.LiquidContainer>();
                    if (liquidContainer != null && !string.IsNullOrEmpty(liquidContainer.liquidName))
                    {
                        liquidName = liquidContainer.liquidName;
                    }

                    float pourAmount = Time.deltaTime * 30f;
                    shaker.AddLiquid(liquidName, pourAmount);
                    Debug.Log($"正在倒酒到Shaker: {liquidName}");
                    return;
                }
            }
            
            // 检查是否手持Shaker（可以倒出）
            // 優先檢查 Shaker 組件
            var heldShakerObj = heldObject != null ? heldObject.GetComponent<Objects.Shaker>() : null;
            if (heldShakerObj != null)
            {
                Debug.Log($"手持 Shaker: IsEmpty={heldShakerObj.IsEmpty}, Volume={heldShakerObj.Volume}");
                
                // 只要有液體就可以倒出，不需要檢查是否搖過
                if (!heldShakerObj.IsEmpty)
                {
                    // 倒入玻璃杯
                    InteractableItem targetItem = currentHighlightedObject.GetComponent<InteractableItem>();
                    if (targetItem != null && targetItem.itemType == ItemType.Glass)
                    {
                        // 優先嘗試新的 Glass 組件
                        var glass = currentHighlightedObject.GetComponent<Objects.Glass>();
                        if (glass != null && !glass.IsFull)
                        {
                            float pourAmount = Time.deltaTime * 30f;
                            float transferred = heldShakerObj.TransferTo(glass, pourAmount);
                            if (transferred > 0)
                            {
                                Debug.Log($"正在从Shaker倒酒到Glass: {transferred}ml");
                            }
                        }
                        

                        // 嘗試舊的 GlassContainer
                        var glassContainer = currentHighlightedObject.GetComponent<Objects.GlassContainer>();
                        if (glassContainer != null && !glassContainer.IsFull())
                        {
                            float pourAmount = Time.deltaTime * 30f; // 30ml/s
                            // 使用 Container 的 TransferTo 方法 (需要 Container.cs 支援)
                            float transferred = heldShakerObj.TransferTo(glassContainer, pourAmount);
                            if (transferred > 0)
                            {
                                Debug.Log($"正在从Shaker倒酒到GlassContainer: {transferred}ml");
                            }
                            return;
                        }
                    }
                }
                else
                {
                    // Debug.Log("Shaker is empty, cannot pour.");
                }
            }
        }

        private void UpdateLiquidUI()
        {
            // 1. 處理 ShakerInfoUI (當手持 Shaker 時顯示)
            if (shakerInfoUI != null)
            {
                if (heldObject != null && heldItem != null && heldItem.itemType == ItemType.Shaker)
                {
                    var shaker = heldObject.GetComponent<Objects.Shaker>();
                    if (shaker != null)
                    {
                        shakerInfoUI.SetTargetShaker(shaker);
                    }
                    else
                    {
                        shakerInfoUI.ClearTarget();
                    }
                }
                else
                {
                    shakerInfoUI.ClearTarget();
                }
            }

            if (liquidInfoUI == null)
                return;

            // 2. 處理 LiquidInfoUI (顯示目標容器或手持的其他容器)

            // 如果手持酒瓶并且正在看着玻璃杯或Shaker
            if (heldObject != null && heldItem != null && heldItem.itemType == ItemType.Bottle)
            {
                if (currentHighlightedObject != null)
                {
                    InteractableItem targetItem = currentHighlightedObject.GetComponent<InteractableItem>();
                    
                    // 檢測玻璃杯
                    if (targetItem != null && targetItem.itemType == ItemType.Glass)
                    {
                        // 優先嘗試新的 Glass 組件
                        var glass = currentHighlightedObject.GetComponent<Objects.Glass>();
                        if (glass != null)
                        {
                            // 不傳遞 targetName，避免顯示 "Pouring into..."
                            liquidInfoUI.SetTargetContainer(glass);
                            return;
                        }

                        // 舊版 GlassContainer
                        var glassContainer = currentHighlightedObject.GetComponent<Objects.GlassContainer>();
                        if (glassContainer != null)
                        {
                            string targetName = currentHighlightedObject.name;
                            liquidInfoUI.SetTargetGlass(glassContainer, targetName);
                            return;
                        }
                    }

                    // 檢測 Shaker
                    var shaker = currentHighlightedObject.GetComponent<Objects.Shaker>();
                    if (shaker != null)
                    {
                        string targetName = currentHighlightedObject.name;
                        liquidInfoUI.SetTargetContainer(shaker, targetName);
                        return;
                    }
                }
            }

            // 如果手持玻璃杯
            if (heldObject != null && heldItem != null && heldItem.itemType == ItemType.Glass)
            {
                var glassContainer = heldObject.GetComponent<Objects.GlassContainer>();
                if (glassContainer != null && !glassContainer.IsEmpty())
                {
                    liquidInfoUI.SetTargetGlass(glassContainer);
                    return;
                }
            }
            
            // 如果手持Shaker，且正在看著玻璃杯 (顯示目標玻璃杯的狀態)
            if (heldObject != null && heldItem != null && heldItem.itemType == ItemType.Shaker)
            {
                if (currentHighlightedObject != null)
                {
                    InteractableItem targetItem = currentHighlightedObject.GetComponent<InteractableItem>();
                    if (targetItem != null && targetItem.itemType == ItemType.Glass)
                    {
                        // 優先嘗試新的 Glass 組件
                        var glass = currentHighlightedObject.GetComponent<Objects.Glass>();
                        if (glass != null)
                        {
                            string targetName = currentHighlightedObject.name;
                            liquidInfoUI.SetTargetContainer(glass, targetName);
                            return;
                        }

                        // 舊版 GlassContainer
                        var glassContainer = currentHighlightedObject.GetComponent<Objects.GlassContainer>();
                        if (glassContainer != null)
                        {
                            string targetName = currentHighlightedObject.name;
                            liquidInfoUI.SetTargetGlass(glassContainer, targetName);
                            return;
                        }
                    }
                }
            }

            // 如果正在看着有液体的玻璃杯 (且沒拿東西)
            if (currentHighlightedObject != null && heldObject == null)
            {
                InteractableItem targetItem = currentHighlightedObject.GetComponent<InteractableItem>();
                if (targetItem != null && targetItem.itemType == ItemType.Glass)
                {
                    // 優先嘗試新的 Glass 組件
                    var glass = currentHighlightedObject.GetComponent<Objects.Glass>();
                    if (glass != null && !glass.IsEmpty)
                    {
                        liquidInfoUI.SetTargetContainer(glass);
                        return;
                    }

                    // 舊版 GlassContainer
                    var glassContainer = currentHighlightedObject.GetComponent<Objects.GlassContainer>();
                    if (glassContainer != null && !glassContainer.IsEmpty())
                    {
                        liquidInfoUI.SetTargetGlass(glassContainer);
                        return;
                    }
                }
            }

            // 默认情况：清除UI
            liquidInfoUI.ClearTarget();
        }

        private void OnDrawGizmos()
        {
            if (playerCamera == null)
                return;

            // 绘制互动距离
            Gizmos.color = Color.green;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance);

            // 如果手持物品，绘制倒酒距离
            if (heldObject != null && heldItem != null && heldItem.itemType == ItemType.Bottle)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * pourDistance);
            }
        }

        /// <summary>
        /// Show interaction prompt for the targeted object
        /// </summary>
        private void ShowInteractionPrompt(GameObject obj)
        {
            if (obj == null) return;

            string itemName = GetFriendlyName(obj);
            
            // Different prompts based on what we're holding
            if (heldObject != null && heldItem != null && heldItem.itemType == ItemType.Bottle)
            {
                // Holding bottle, looking at glass
                UIPromptManager.Show($"按住左鍵倒酒到 {itemName}");
            }
            else if (currentHighlightedObject.GetComponent<InteractableItem>().itemType == ItemType.Speaker)
            {
                // Holding speaker, show volume controls
                UIPromptManager.Show($"按住左鍵/右鍵 減小/增加音量，按 E 切換播放，按 Q 輪換歌曲");
            }
            else if (heldObject != null && heldItem != null && heldItem.itemType == ItemType.Shaker)
            {
                // Holding shaker, looking at glass
                UIPromptManager.Show($"按住左鍵倒酒到 {itemName}，按住右鍵搖晃");
            }
            else
            {
                // Not holding anything, looking at item
                UIPromptManager.Show($"按 E 拾取 {itemName}");
            }
        }

        /// <summary>
        /// Get friendly display name for an object
        /// </summary>
        private string GetFriendlyName(GameObject obj)
        {
            // Check for InteractableItem component
            var interactableItem = obj.GetComponent<InteractableItem>();
            if (interactableItem != null && !string.IsNullOrEmpty(interactableItem.itemName))
            {
                return interactableItem.itemName;
            }

            // Check for IInteractable interface
            var interactable = obj.GetComponent<BarSimulator.Interaction.IInteractable>();
            if (interactable != null && !string.IsNullOrEmpty(interactable.DisplayName))
            {
                return interactable.DisplayName;
            }

            // Check for LiquidContainer
            var liquidContainer = obj.GetComponent<Objects.LiquidContainer>();
            if (liquidContainer != null && !string.IsNullOrEmpty(liquidContainer.liquidName))
            {
                return liquidContainer.liquidName;
            }

            // Check for GlassContainer
            var glassContainer = obj.GetComponent<Objects.GlassContainer>();
            if (glassContainer != null)
            {
                return "玻璃杯";
            }

            // Fallback to object name
            return obj.name;
        }

        /// <summary>
        /// Check if holding ServeGlass near NPC and show prompt
        /// </summary>
        private void CheckNPCServing()
        {
            // Only check if holding any glass with liquid
            if (heldObject == null)
                return;

            // Check if holding any glass with liquid (not just ServeGlass)
            var glassContainer = heldObject.GetComponent<Objects.GlassContainer>();
            if (glassContainer == null || glassContainer.IsEmpty())
                return;

            // Find nearby NPCs (check both SimpleNPCServe and EnhancedNPCServe)
            GameObject closestNPC = null;
            float closestDistance = 3f; // NPC interaction distance

            // Check SimpleNPCServe
            var simpleNPCs = FindObjectsOfType<NPC.SimpleNPCServe>();
            foreach (var npc in simpleNPCs)
            {
                float distance = Vector3.Distance(transform.position, npc.transform.position);
                if (distance < closestDistance)
                {
                    closestNPC = npc.gameObject;
                    closestDistance = distance;
                }
            }

            // Check EnhancedNPCServe
            var enhancedNPCs = FindObjectsOfType<NPC.EnhancedNPCServe>();
            foreach (var npc in enhancedNPCs)
            {
                float distance = Vector3.Distance(transform.position, npc.transform.position);
                if (distance < closestDistance)
                {
                    closestNPC = npc.gameObject;
                    closestDistance = distance;
                }
            }

            // Show prompt if near NPC
            if (closestNPC != null)
            {
                string npcName = closestNPC.name;
                UIPromptManager.Show($"按下 F 把酒給 {npcName}");
            }
        }
    }
}
