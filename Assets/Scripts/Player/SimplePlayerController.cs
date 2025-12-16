using UnityEngine;

namespace BarSimulator.Player
{
    /// <summary>
    /// 简单的第一人称玩家控制器
    /// WASD移动，鼠标控制视角
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SimplePlayerController : MonoBehaviour
    {
        [Header("移动设置")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundDrag = 5f;

        [Header("视角设置")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        [Header("相机")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private float pitch = 0f;
        private float yaw = 0f;
        private Vector3 velocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            // 如果没有指定相机，尝试找子物件中的相机
            if (cameraTransform == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    cameraTransform = cam.transform;
                }
                else
                {
                    cam = Camera.main;
                    if (cam != null)
                    {
                        cameraTransform = cam.transform;
                    }
                }
            }

            // 锁定并隐藏鼠标
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleMovement();
            HandleMouseLook();

            // ESC键解锁鼠标
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // 点击屏幕重新锁定鼠标
            if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void HandleMovement()
        {
            // 获取输入
            float horizontal = Input.GetAxis("Horizontal"); // A/D
            float vertical = Input.GetAxis("Vertical");     // W/S

            // 计算移动方向（相对于玩家朝向）
            Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
            
            // 限制对角线速度
            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }

            // 应用阻力，减少滑动感
            if (moveDirection.magnitude < 0.1f && controller.isGrounded)
            {
                moveDirection = Vector3.zero;
            }

            // 应用移动
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);

            // 应用重力
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleMouseLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
                return;

            // 获取鼠标输入
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // 更新水平旋转（yaw）
            yaw += mouseX;

            // 更新垂直旋转（pitch）
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            // 应用旋转
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }
    }
}
