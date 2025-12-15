using UnityEngine;

namespace BarSimulator.Player
{
    /// <summary>
    /// 物品互动系统
    /// - 检测可互动物品（2米内）
    /// - 显示黄色高亮外框
    /// - E键拾取/放下物品
    /// - 右键倒酒（如果手持酒瓶）
    /// </summary>
    public class ItemInteractionSystem : MonoBehaviour
    {
        [Header("互动设置")]
        [SerializeField] private float interactionDistance = 2f;
        [SerializeField] private float pourDistance = 1.5f;
        [SerializeField] private LayerMask interactableLayer = -1; // 所有层

        [Header("手持位置")]
        [SerializeField] private Transform handPosition;
        [SerializeField] private Vector3 handOffset = new Vector3(0.5f, -0.3f, 0.8f);

        [Header("高亮设置")]
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float highlightWidth = 0.02f;

        private Camera playerCamera;
        private GameObject currentHighlightedObject;
        private GameObject heldObject;
        private Rigidbody heldRigidbody;
        private Collider heldCollider;
        private InteractableItem heldItem;

        // 用于高亮效果的材质
        private Material highlightMaterial;
        private Renderer currentHighlightedRenderer;
        private Material[] originalMaterials;

        private void Awake()
        {
            // 获取相机
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            // 创建手持位置
            if (handPosition == null)
            {
                GameObject handObj = new GameObject("HandPosition");
                handObj.transform.SetParent(playerCamera.transform);
                handObj.transform.localPosition = handOffset;
                handPosition = handObj.transform;
            }

            // 创建高亮材质
            CreateHighlightMaterial();
        }

        private void Update()
        {
            // 检测可互动物品
            DetectInteractableObjects();

            // 处理互动输入
            HandleInteractionInput();

            // 更新手持物品位置
            if (heldObject != null)
            {
                UpdateHeldObjectPosition();
            }
        }

        private void CreateHighlightMaterial()
        {
            // 创建一个简单的高亮材质（使用Unlit/Color shader）
            highlightMaterial = new Material(Shader.Find("Unlit/Color"));
            highlightMaterial.color = highlightColor;
        }

        private void DetectInteractableObjects()
        {
            // 如果手上有物品，不检测新物品
            if (heldObject != null)
            {
                ClearHighlight();
                return;
            }

            // 从相机中心发射射线
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
            {
                // 检查是否有InteractableItem组件
                InteractableItem item = hit.collider.GetComponent<InteractableItem>();
                if (item != null && item.enabled)
                {
                    // 高亮这个物品
                    HighlightObject(hit.collider.gameObject);
                    return;
                }
            }

            // 没有检测到物品，清除高亮
            ClearHighlight();
        }

        private void HighlightObject(GameObject obj)
        {
            // 如果已经高亮了这个物品，不需要重复处理
            if (currentHighlightedObject == obj)
                return;

            // 清除之前的高亮
            ClearHighlight();

            // 设置新的高亮物品
            currentHighlightedObject = obj;
            currentHighlightedRenderer = obj.GetComponent<Renderer>();

            if (currentHighlightedRenderer != null)
            {
                // 保存原始材质
                originalMaterials = currentHighlightedRenderer.materials;

                // 添加高亮效果（通过修改emission或者添加outline）
                // 这里使用简单的方法：改变材质颜色的emission
                Material[] newMaterials = new Material[originalMaterials.Length];
                for (int i = 0; i < originalMaterials.Length; i++)
                {
                    newMaterials[i] = new Material(originalMaterials[i]);
                    // 启用emission
                    newMaterials[i].EnableKeyword("_EMISSION");
                    newMaterials[i].SetColor("_EmissionColor", highlightColor * 0.5f);
                }
                currentHighlightedRenderer.materials = newMaterials;
            }
        }

        private void ClearHighlight()
        {
            if (currentHighlightedObject != null && currentHighlightedRenderer != null && originalMaterials != null)
            {
                // 恢复原始材质
                currentHighlightedRenderer.materials = originalMaterials;
                originalMaterials = null;
            }

            currentHighlightedObject = null;
            currentHighlightedRenderer = null;
        }

        private void HandleInteractionInput()
        {
            // E键：拾取/放下物品
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (heldObject == null)
                {
                    // 尝试拾取物品
                    TryPickupObject();
                }
                else
                {
                    // 放下物品
                    DropObject();
                }
            }

            // 右键：倒酒（如果手持酒瓶）
            if (Input.GetMouseButton(1) && heldObject != null && heldItem != null)
            {
                if (heldItem.itemType == ItemType.Bottle)
                {
                    TryPourLiquid();
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

            // 拾取物品
            heldObject = currentHighlightedObject;
            heldItem = item;
            heldRigidbody = heldObject.GetComponent<Rigidbody>();
            heldCollider = heldObject.GetComponent<Collider>();

            // 禁用物理
            if (heldRigidbody != null)
            {
                heldRigidbody.isKinematic = true;
                heldRigidbody.useGravity = false;
            }

            // 禁用碰撞
            if (heldCollider != null)
            {
                heldCollider.enabled = false;
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

            Debug.Log($"拾取了: {heldObject.name}");
        }

        private void DropObject()
        {
            if (heldObject == null)
                return;

            // 恢复物理
            if (heldRigidbody != null)
            {
                heldRigidbody.isKinematic = false;
                heldRigidbody.useGravity = true;
            }

            // 恢复碰撞
            if (heldCollider != null)
            {
                heldCollider.enabled = true;
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

            Debug.Log($"放下了: {heldObject.name}");

            heldObject = null;
            heldItem = null;
            heldRigidbody = null;
            heldCollider = null;
        }

        private void UpdateHeldObjectPosition()
        {
            if (heldObject != null && handPosition != null)
            {
                // 平滑移动到手持位置
                heldObject.transform.position = Vector3.Lerp(
                    heldObject.transform.position,
                    handPosition.position,
                    Time.deltaTime * 15f
                );

                // 保持物品朝向前方
                heldObject.transform.rotation = Quaternion.Lerp(
                    heldObject.transform.rotation,
                    handPosition.rotation,
                    Time.deltaTime * 10f
                );
            }
        }

        private void TryPourLiquid()
        {
            // 从手持位置发射射线检测杯子
            Ray ray = new Ray(handPosition.position, handPosition.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, pourDistance, interactableLayer))
            {
                InteractableItem targetItem = hit.collider.GetComponent<InteractableItem>();
                if (targetItem != null && targetItem.itemType == ItemType.Glass)
                {
                    // 倒酒到杯子
                    Debug.Log($"正在倒酒到: {targetItem.name}");
                    
                    // 这里可以添加倒酒的视觉效果和逻辑
                    // 例如：增加杯子中的液体量，播放倒酒音效等
                    targetItem.OnReceiveLiquid(heldItem.liquidType, Time.deltaTime * 30f); // 每秒30ml
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (playerCamera == null)
                return;

            // 绘制互动距离
            Gizmos.color = Color.green;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance);

            // 如果手持物品，绘制倒酒距离
            if (heldObject != null && handPosition != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(handPosition.position, handPosition.forward * pourDistance);
            }
        }
    }
}
