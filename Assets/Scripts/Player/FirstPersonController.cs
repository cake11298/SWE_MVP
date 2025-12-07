using UnityEngine;
using UnityEngine.InputSystem;
using BarSimulator.Core;

namespace BarSimulator.Player
{
    /// <summary>
    /// 第一人稱控制器 - 處理玩家移動與視角控制
    /// 參考: src/modules/PlayerController.js
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        #region 序列化欄位

        [Header("移動設定")]
        [Tooltip("移動速度 (m/s)")]
        [SerializeField] private float moveSpeed = Constants.MoveSpeed;

        [Header("視角設定")]
        [Tooltip("滑鼠敏感度")]
        [SerializeField] private float mouseSensitivity = Constants.MouseSensitivity;

        [Tooltip("視角平滑度 (0 = 無平滑, 值越大越平滑)")]
        [SerializeField] private float lookSmoothing = 10f;

        [Tooltip("垂直視角最小值 (度)")]
        [SerializeField] private float minPitch = Constants.MinPitch;

        [Tooltip("垂直視角最大值 (度)")]
        [SerializeField] private float maxPitch = Constants.MaxPitch;

        [Header("攝影機")]
        [Tooltip("玩家攝影機 Transform")]
        [SerializeField] private Transform cameraTransform;

        [Header("Input Actions")]
        [Tooltip("Input Actions Asset")]
        [SerializeField] private InputActionAsset inputActions;

        #endregion

        #region 私有欄位

        private CharacterController controller;
        private InputAction moveAction;
        private InputAction lookAction;

        private Vector2 moveInput;
        private Vector2 lookInput;
        private float pitch;
        private float yaw;
        
        // 平滑處理變數
        private Vector2 currentLookInput;
        private Vector2 lookInputVelocity;

        private bool isInputEnabled = true;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            // 如果沒有指定攝影機，嘗試找子物件中的攝影機
            if (cameraTransform == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    cameraTransform = cam.transform;
                }
                else
                {
                    // 嘗試尋找主攝影機
                    cam = Camera.main;
                    if (cam != null)
                    {
                        cameraTransform = cam.transform;
                    }
                }
            }

            if (cameraTransform == null)
            {
                Debug.LogError("FirstPersonController: 找不到攝影機 Transform，垂直視角將無法運作");
            }

            SetupInputActions();
        }

        private void OnEnable()
        {
            EnableInput();
        }

        private void OnDisable()
        {
            DisableInput();
        }

        private void Start()
        {
            // 初始化視角
            yaw = transform.eulerAngles.y;

            // 從攝影機當前角度初始化 pitch，避免突然跳轉
            if (cameraTransform != null)
            {
                pitch = cameraTransform.localEulerAngles.x;
                // 處理 Unity 的角度表示 (0-360)，轉換為 -180 到 180
                if (pitch > 180f) pitch -= 360f;
                // 確保 pitch 在有效範圍內
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }
            else
            {
                pitch = 0f;
            }

            // 鎖定滑鼠游標
            LockCursor();
        }

        private void Update()
        {
            if (!isInputEnabled) return;

            HandleMovement();
            HandleLook();
        }

        #endregion

        #region 輸入設定

        /// <summary>
        /// 設定 Input Actions
        /// </summary>
        private void SetupInputActions()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("FirstPersonController: InputActionAsset 未設定，嘗試從 Resources 載入");
                inputActions = Resources.Load<InputActionAsset>("PlayerInputActions");
            }

            if (inputActions == null)
            {
                Debug.LogError("FirstPersonController: 無法載入 InputActionAsset");
                return;
            }

            var playerMap = inputActions.FindActionMap("Player");
            if (playerMap == null)
            {
                Debug.LogError("FirstPersonController: 找不到 'Player' Action Map");
                return;
            }

            moveAction = playerMap.FindAction("Move");
            lookAction = playerMap.FindAction("Look");

            if (moveAction == null)
                Debug.LogError("FirstPersonController: 找不到 'Move' Action");

            if (lookAction == null)
                Debug.LogError("FirstPersonController: 找不到 'Look' Action");
        }

        /// <summary>
        /// 啟用輸入
        /// </summary>
        public void EnableInput()
        {
            moveAction?.Enable();
            lookAction?.Enable();
            isInputEnabled = true;
        }

        /// <summary>
        /// 停用輸入
        /// </summary>
        public void DisableInput()
        {
            moveAction?.Disable();
            lookAction?.Disable();
            isInputEnabled = false;
        }

        #endregion

        #region 移動與視角處理

        /// <summary>
        /// 處理移動輸入
        /// 參考: PlayerController.js Line 128-158
        /// </summary>
        private void HandleMovement()
        {
            if (moveAction == null) return;

            moveInput = moveAction.ReadValue<Vector2>();

            // 計算移動方向（相對於玩家朝向）
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

            // 應用移動
            controller.Move(move * moveSpeed * Time.deltaTime);

            // 應用重力（簡化版本，保持玩家在地面）
            if (!controller.isGrounded)
            {
                controller.Move(Vector3.up * Constants.Gravity * Time.deltaTime);
            }
        }

        /// <summary>
        /// 處理視角輸入
        /// 參考: PlayerController.js Line 64-76
        /// </summary>
        private void HandleLook()
        {
            if (lookAction == null) return;

            Vector2 targetLookInput = lookAction.ReadValue<Vector2>();

            // 應用平滑
            if (lookSmoothing > 0f)
            {
                currentLookInput = Vector2.SmoothDamp(currentLookInput, targetLookInput, ref lookInputVelocity, 1f / lookSmoothing);
            }
            else
            {
                currentLookInput = targetLookInput;
            }

            lookInput = currentLookInput;

            // 水平旋轉（左右）- 旋轉玩家本體
            yaw += lookInput.x * mouseSensitivity * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            // 垂直旋轉（上下）- 只旋轉攝影機
            pitch -= lookInput.y * mouseSensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            if (cameraTransform != null)
            {
                // 確保攝影機只在 pitch（X 軸）上旋轉，Y 和 Z 軸保持為 0
                // 這樣可以防止攝影機有任何側向傾斜或額外旋轉
                cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        #endregion

        #region 游標控制

        /// <summary>
        /// 鎖定滑鼠游標
        /// </summary>
        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// 解鎖滑鼠游標
        /// </summary>
        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// 切換游標鎖定狀態
        /// </summary>
        public void ToggleCursorLock()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        /// <summary>
        /// 檢查游標是否鎖定
        /// </summary>
        public bool IsCursorLocked => Cursor.lockState == CursorLockMode.Locked;

        #endregion

        #region 公開屬性

        /// <summary>
        /// 玩家位置
        /// </summary>
        public Vector3 Position => transform.position;

        /// <summary>
        /// 玩家前方方向
        /// </summary>
        public Vector3 Forward => transform.forward;

        /// <summary>
        /// 攝影機 Transform
        /// </summary>
        public Transform CameraTransform => cameraTransform;

        /// <summary>
        /// 設定滑鼠敏感度
        /// 參考: PlayerController.js Line 184-187
        /// </summary>
        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }

        /// <summary>
        /// 重置玩家位置到初始點
        /// </summary>
        public void ResetPosition()
        {
            transform.position = new Vector3(0f, 1.6f, 5f);
            pitch = 0f;
            yaw = 0f;

            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.identity;
            }

            Debug.Log("FirstPersonController: 玩家位置已重置");
        }

        /// <summary>
        /// 取得當前移動輸入
        /// </summary>
        public Vector2 MoveInput => moveInput;

        /// <summary>
        /// 取得當前視角輸入
        /// </summary>
        public Vector2 LookInput => lookInput;

        #endregion
    }
}
