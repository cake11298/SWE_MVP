using UnityEngine;
using UnityEngine.InputSystem;
using BarSimulator.Core;
using BarSimulator.Interaction;

namespace BarSimulator.Systems
{
    /// <summary>
    /// 互動系統 - 處理 Raycast 檢測與物件互動
    /// 參考: src/modules/InteractionSystem.js
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        #region 單例

        private static InteractionSystem instance;
        public static InteractionSystem Instance => instance;

        #endregion

        #region 序列化欄位

        [Header("射線設定")]
        [Tooltip("互動距離")]
        [SerializeField] private float interactionDistance = Constants.InteractionDistance;

        [Tooltip("可互動物件 Layer")]
        [SerializeField] private LayerMask interactableLayer;

        [Header("攝影機")]
        [Tooltip("主攝影機")]
        [SerializeField] private Camera mainCamera;

        [Header("Input Actions")]
        [Tooltip("Input Actions Asset")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("手持設定")]
        [Tooltip("手持物品偏移位置")]
        [SerializeField] private Vector3 holdOffset = Constants.HoldOffset;

        [Tooltip("手持物品平滑跟隨速度")]
        [SerializeField] private float holdSmoothSpeed = 15f;

        #endregion

        #region 私有欄位

        // 當前狀態
        private IInteractable targetedObject;
        private IInteractable heldObject;
        private Transform heldTransform;
        private bool isHolding;

        // 原始物理狀態
        private Rigidbody heldRigidbody;
        private bool originalKinematic;
        private bool originalGravity;

        // Input Actions
        private InputAction interactAction;
        private InputAction dropAction;
        private InputAction leftClickAction;
        private InputAction rightClickAction;

        // Fallback input state
        private bool useFallbackInput = false;

        // 事件
        public System.Action<IInteractable> OnObjectTargeted;
        public System.Action OnObjectUntargeted;
        public System.Action<IInteractable> OnObjectPickedUp;
        public System.Action<IInteractable> OnObjectDropped;

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

            SetupInputActions();
        }

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            interactAction?.Enable();
            dropAction?.Enable();
            leftClickAction?.Enable();
            rightClickAction?.Enable();
        }

        private void OnDisable()
        {
            interactAction?.Disable();
            dropAction?.Disable();
            leftClickAction?.Disable();
            rightClickAction?.Disable();
        }

        private void Update()
        {
            // 檢測瞄準物件
            CheckTargeted();

            // 更新手持物品位置
            UpdateHeldObject();

            // 處理輸入
            HandleInput();
        }

        #endregion

        #region Input 設定

        private void SetupInputActions()
        {
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("PlayerInputActions");
            }

            if (inputActions == null)
            {
                Debug.LogWarning("InteractionSystem: PlayerInputActions not found, using fallback input");
                useFallbackInput = true;
                return;
            }

            var playerMap = inputActions.FindActionMap("Player");
            if (playerMap != null)
            {
                interactAction = playerMap.FindAction("Interact");
                dropAction = playerMap.FindAction("Drop");
                leftClickAction = playerMap.FindAction("LeftClick");
                rightClickAction = playerMap.FindAction("RightClick");

                if (interactAction == null || dropAction == null)
                {
                    Debug.LogWarning("InteractionSystem: Some actions not found, using fallback input");
                    useFallbackInput = true;
                }
            }
            else
            {
                Debug.LogWarning("InteractionSystem: Player action map not found, using fallback input");
                useFallbackInput = true;
            }
        }

        private void HandleInput()
        {
            // E 鍵：拾取/互動
            bool interactPressed = false;
            if (useFallbackInput)
            {
                interactPressed = Input.GetKeyDown(KeyCode.E);
            }
            else
            {
                interactPressed = interactAction != null && interactAction.WasPressedThisFrame();
            }

            if (interactPressed)
            {
                if (!isHolding && targetedObject != null)
                {
                    if (targetedObject.CanPickup)
                    {
                        PickupObject(targetedObject);
                    }
                    else
                    {
                        // 特殊互動（如吉他）
                        targetedObject.OnInteract();
                    }
                }
            }

            // R 鍵：放回原位
            bool returnPressed = false;
            if (useFallbackInput)
            {
                returnPressed = Input.GetKeyDown(KeyCode.R);
            }
            else
            {
                returnPressed = dropAction != null && dropAction.WasPressedThisFrame();
            }

            if (returnPressed)
            {
                if (isHolding)
                {
                    DropObject(true); // Return to original position
                }
            }

            // Q 鍵：原地放下
            if (useFallbackInput && Input.GetKeyDown(KeyCode.Q))
            {
                if (isHolding)
                {
                    DropObject(false); // Drop in place
                }
            }
        }

        #endregion

        #region Raycast 檢測

        /// <summary>
        /// 檢測玩家瞄準的物件
        /// 參考: InteractionSystem.js checkTargeted() Line 87-119
        /// </summary>
        public IInteractable CheckTargeted()
        {
            if (mainCamera == null) return null;

            // 從螢幕中心發射射線
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            bool didHit = false;
            RaycastHit hit = default;

            // If interactableLayer is configured (not 0), use it; otherwise raycast all
            if (interactableLayer.value != 0)
            {
                didHit = Physics.Raycast(ray, out hit, interactionDistance, interactableLayer);
            }
            else
            {
                // Fallback: raycast all objects and check for IInteractable
                didHit = Physics.Raycast(ray, out hit, interactionDistance);
            }

            if (didHit)
            {
                // 嘗試取得 IInteractable 組件
                var interactable = hit.collider.GetComponentInParent<IInteractable>();

                if (interactable != null)
                {
                    // 如果目標改變
                    if (targetedObject != interactable)
                    {
                        // 通知舊目標
                        targetedObject?.OnUntargeted();
                        OnObjectUntargeted?.Invoke();

                        // 設定新目標
                        targetedObject = interactable;
                        targetedObject.OnTargeted();
                        OnObjectTargeted?.Invoke(targetedObject);
                    }

                    return targetedObject;
                }
            }

            // 沒有瞄準任何物件
            if (targetedObject != null)
            {
                targetedObject.OnUntargeted();
                OnObjectUntargeted?.Invoke();
                targetedObject = null;
            }

            return null;
        }

        #endregion

        #region 拾取/放下

        /// <summary>
        /// 拾取物件
        /// 參考: InteractionSystem.js pickupObject() Line 126-152
        /// </summary>
        public bool PickupObject(IInteractable obj)
        {
            if (isHolding || obj == null) return false;

            var mono = obj as MonoBehaviour;
            if (mono == null) return false;

            heldObject = obj;
            heldTransform = mono.transform;
            isHolding = true;

            // 儲存並禁用物理
            heldRigidbody = mono.GetComponent<Rigidbody>();
            if (heldRigidbody != null)
            {
                originalKinematic = heldRigidbody.isKinematic;
                originalGravity = heldRigidbody.useGravity;
                heldRigidbody.isKinematic = true;
                heldRigidbody.useGravity = false;
            }

            // 禁用碰撞器（避免與玩家碰撞）
            var collider = mono.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // 通知物件
            obj.OnPickup();
            OnObjectPickedUp?.Invoke(obj);

            Debug.Log($"InteractionSystem: 拾取 {obj.DisplayName}");
            return true;
        }

        /// <summary>
        /// 放下物件
        /// 參考: InteractionSystem.js dropObject() Line 158-215
        /// </summary>
        public void DropObject(bool returnToOriginal = false)
        {
            if (!isHolding || heldObject == null) return;

            var mono = heldObject as MonoBehaviour;

            // 恢復碰撞器
            if (mono != null)
            {
                var collider = mono.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }

            if (returnToOriginal)
            {
                // 放回原位
                if (heldTransform != null)
                {
                    heldTransform.position = heldObject.OriginalPosition;
                }

                // 恢復物理（但保持靜止）
                if (heldRigidbody != null)
                {
                    heldRigidbody.isKinematic = true;
                    heldRigidbody.useGravity = false;
                    heldRigidbody.velocity = Vector3.zero;
                    heldRigidbody.angularVelocity = Vector3.zero;
                }
            }
            else
            {
                // 原地放下（恢復物理）
                if (heldRigidbody != null)
                {
                    heldRigidbody.isKinematic = originalKinematic;
                    heldRigidbody.useGravity = originalGravity;
                }
            }

            // 通知物件
            heldObject.OnDrop(returnToOriginal);
            OnObjectDropped?.Invoke(heldObject);

            Debug.Log($"InteractionSystem: 放下 {heldObject.DisplayName}");

            // 清除狀態
            heldObject = null;
            heldTransform = null;
            heldRigidbody = null;
            isHolding = false;
        }

        #endregion

        #region 手持物品更新

        /// <summary>
        /// 更新手持物品位置
        /// 參考: InteractionSystem.js update() Line 271-300
        /// </summary>
        private void UpdateHeldObject()
        {
            if (!isHolding || heldTransform == null || mainCamera == null) return;

            // 計算目標位置（相機前方偏移）
            Vector3 targetPosition = mainCamera.transform.TransformPoint(holdOffset);

            // 平滑移動
            heldTransform.position = Vector3.Lerp(
                heldTransform.position,
                targetPosition,
                holdSmoothSpeed * Time.deltaTime
            );

            // 計算目標旋轉（面向相機方向）
            Quaternion targetRotation = Quaternion.LookRotation(mainCamera.transform.forward);

            // 平滑旋轉
            heldTransform.rotation = Quaternion.Slerp(
                heldTransform.rotation,
                targetRotation,
                holdSmoothSpeed * Time.deltaTime
            );
        }

        #endregion

        #region 公開屬性與方法

        /// <summary>
        /// 放下手持物件（公開方法）
        /// </summary>
        public void DropHeldObject()
        {
            DropObject(false);
        }

        /// <summary>
        /// 當前瞄準的物件
        /// </summary>
        public IInteractable TargetedObject => targetedObject;

        /// <summary>
        /// 當前手持的物件
        /// </summary>
        public IInteractable HeldObject => heldObject;

        /// <summary>
        /// 手持物件的 Transform
        /// </summary>
        public Transform HeldTransform => heldTransform;

        /// <summary>
        /// 是否正在拿著物件
        /// </summary>
        public bool IsHolding => isHolding;

        /// <summary>
        /// 取得手持物件類型
        /// </summary>
        public InteractableType? GetHeldObjectType()
        {
            return heldObject?.InteractableType;
        }

        /// <summary>
        /// 檢查滑鼠左鍵是否按住
        /// </summary>
        public bool IsLeftClickHeld()
        {
            if (useFallbackInput)
            {
                return Input.GetMouseButton(0);
            }
            return leftClickAction != null && leftClickAction.IsPressed();
        }

        /// <summary>
        /// 檢查滑鼠右鍵是否按下
        /// </summary>
        public bool IsRightClickPressed()
        {
            if (useFallbackInput)
            {
                return Input.GetMouseButtonDown(1);
            }
            return rightClickAction != null && rightClickAction.WasPressedThisFrame();
        }

        /// <summary>
        /// Get interaction hint text (English)
        /// Reference: InteractionSystem.js getInteractionHint() Line 341-412
        /// </summary>
        public string GetInteractionHint()
        {
            if (isHolding && heldObject != null)
            {
                var type = heldObject.InteractableType;
                string typeName = GetTypeNameEnglish(type);

                return type switch
                {
                    InteractableType.Bottle => $"{heldObject.DisplayName} | Hold LMB to pour | Q=Drop R=Return",
                    InteractableType.Glass => $"{typeName} | Press RMB to drink | Q=Drop R=Return",
                    InteractableType.Shaker => $"{typeName} | Hold LMB to shake | Q=Drop R=Return",
                    _ => $"{heldObject.DisplayName} | Q=Drop R=Return"
                };
            }
            else if (targetedObject != null)
            {
                string action = targetedObject.CanPickup ? "pick up" : "interact with";
                return $"Press E to {action} {targetedObject.DisplayName}";
            }

            return string.Empty;
        }

        /// <summary>
        /// Get type name in English
        /// </summary>
        private string GetTypeNameEnglish(InteractableType type)
        {
            return type switch
            {
                InteractableType.Bottle => "Bottle",
                InteractableType.Glass => "Glass",
                InteractableType.Shaker => "Shaker",
                InteractableType.Jigger => "Jigger",
                InteractableType.Guitar => "Guitar",
                _ => "Item"
            };
        }

        /// <summary>
        /// 尋找附近的容器
        /// 參考: index.js findNearbyContainer() Line 247-280
        /// </summary>
        public Transform FindNearbyContainer(Transform source, float maxDistance = 2.5f)
        {
            if (mainCamera == null || source == null) return null;

            // 取得相機方向
            Vector3 cameraDir = mainCamera.transform.forward;

            // 搜尋附近的容器 - 如果沒有設定 layer，使用所有 layer
            Collider[] colliders;
            if (interactableLayer.value != 0)
            {
                colliders = Physics.OverlapSphere(source.position, maxDistance, interactableLayer);
            }
            else
            {
                colliders = Physics.OverlapSphere(source.position, maxDistance);
            }

            foreach (var collider in colliders)
            {
                if (collider.transform == source) continue;

                var interactable = collider.GetComponentInParent<IInteractable>();
                if (interactable == null) continue;

                // 只檢查杯子和搖酒器
                var type = interactable.InteractableType;
                if (type != InteractableType.Glass && type != InteractableType.Shaker) continue;

                // 檢查視角
                Vector3 toContainer = (collider.transform.position - mainCamera.transform.position).normalized;
                float dot = Vector3.Dot(cameraDir, toContainer);

                // 角度必須小於約30度 (cos(30°) ≈ 0.866)
                if (dot >= Constants.PourAngleCos)
                {
                    return collider.transform;
                }
            }

            return null;
        }

        #endregion
    }
}
