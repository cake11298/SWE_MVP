using UnityEngine;

namespace BarSimulator
{
    /// <summary>
    /// 管理場景裝飾物品的物理狀態
    /// 預設為靜態（kinematic），只有在被放下時才啟用物理效果
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class StaticProp : MonoBehaviour
    {
        private Rigidbody rb;
        private bool isPickedUp = false;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        
        [Header("設置")]
        [Tooltip("是否可以被拾取")]
        public bool canBePickedUp = true;
        
        [Tooltip("放下後是否自動返回原位")]
        public bool returnToOriginalPosition = false;
        
        [Tooltip("返回原位的時間（秒）")]
        public float returnDelay = 2f;
        
        private float dropTime = 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            
            // 記錄原始位置和旋轉
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            
            // 確保初始狀態為 kinematic
            SetKinematic(true);
        }

        private void Update()
        {
            // 如果設置了自動返回，且物品已放下
            if (returnToOriginalPosition && !isPickedUp && dropTime > 0f)
            {
                if (Time.time - dropTime >= returnDelay)
                {
                    ReturnToOriginalPosition();
                    dropTime = 0f;
                }
            }
        }

        /// <summary>
        /// 當物品被拾取時調用
        /// </summary>
        public void OnPickup()
        {
            if (!canBePickedUp) return;
            
            isPickedUp = true;
            dropTime = 0f;
            
            // 保持 kinematic 狀態，避免在手中受重力影響
            SetKinematic(true);
            
            Debug.Log($"[StaticProp] {gameObject.name} 被拾取");
        }

        /// <summary>
        /// 當物品被放下時調用
        /// </summary>
        public void OnDrop()
        {
            isPickedUp = false;
            dropTime = Time.time;
            
            // 啟用物理效果，讓物品自然落下
            SetKinematic(false);
            
            Debug.Log($"[StaticProp] {gameObject.name} 被放下");
        }

        /// <summary>
        /// 設置 Rigidbody 的 kinematic 狀態
        /// </summary>
        private void SetKinematic(bool kinematic)
        {
            if (rb != null)
            {
                // 先停止運動（在設置 kinematic 之前）
                if (!rb.isKinematic && kinematic)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                
                rb.isKinematic = kinematic;
                rb.useGravity = !kinematic;
            }
        }

        /// <summary>
        /// 返回到原始位置
        /// </summary>
        private void ReturnToOriginalPosition()
        {
            // 先設置為 kinematic 以避免物理干擾
            SetKinematic(true);
            
            // 返回原位
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            
            Debug.Log($"[StaticProp] {gameObject.name} 返回原位");
        }

        /// <summary>
        /// 強制返回原位（可從外部調用）
        /// </summary>
        public void ForceReturnToOriginal()
        {
            isPickedUp = false;
            dropTime = 0f;
            ReturnToOriginalPosition();
        }

        /// <summary>
        /// 在編輯器中顯示原始位置
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (returnToOriginalPosition && Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(originalPosition, 0.1f);
                Gizmos.DrawLine(transform.position, originalPosition);
            }
        }
    }
}
