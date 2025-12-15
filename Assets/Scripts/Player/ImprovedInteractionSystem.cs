using UnityEngine;

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
        }

        private void DetectInteractableObjects()
        {
            // 如果手上有物品
            if (heldObject != null)
            {
                // 如果手持酒瓶，检测玻璃杯
                if (heldItem != null && heldItem.itemType == ItemType.Bottle)
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
            // 从手持位置发射射线检测玻璃杯
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, pourDistance))
            {
                InteractableItem item = hit.collider.GetComponent<InteractableItem>();
                if (item != null && item.itemType == ItemType.Glass)
                {
                    HighlightObject(hit.collider.gameObject);
                    return;
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
        }

        private void HandleInput()
        {
            // E键：拾取或放回原位
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (heldObject == null)
                {
                    TryPickupObject();
                }
                else
                {
                    ReturnToOriginalPosition();
                }
            }

            // Q键：原地放下
            if (Input.GetKeyDown(KeyCode.Q) && heldObject != null)
            {
                DropAtCurrentPosition();
            }

            // 左键：倒酒
            if (Input.GetMouseButton(0) && heldObject != null && heldItem != null)
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

            // 保存原始信息
            originalPosition = currentHighlightedObject.transform.position;
            originalRotation = currentHighlightedObject.transform.rotation;
            originalParent = currentHighlightedObject.transform.parent;

            // 拾取物品
            heldObject = currentHighlightedObject;
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
                // 直接設置位置（更可靠）
                heldObject.transform.position = handPosition.position;
                heldObject.transform.rotation = handPosition.rotation;
            }
        }

        private void TryPourLiquid()
        {
            if (currentHighlightedObject == null)
                return;

            InteractableItem targetItem = currentHighlightedObject.GetComponent<InteractableItem>();
            if (targetItem != null && targetItem.itemType == ItemType.Glass)
            {
                // 获取酒的类型
                string liquidName = heldItem.liquidType.ToString();
                
                Debug.Log($"正在倒酒，倒的是: {liquidName}");

                // 倒酒到杯子
                targetItem.OnReceiveLiquid(heldItem.liquidType, Time.deltaTime * 30f);
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
            if (heldObject != null && heldItem != null && heldItem.itemType == ItemType.Bottle)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * pourDistance);
            }
        }
    }
}
