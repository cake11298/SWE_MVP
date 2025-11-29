using UnityEngine;
using BarSimulator.Interaction;

namespace BarSimulator.Objects
{
    /// <summary>
    /// 冰桶 - 儲存和提供冰塊的容器
    /// </summary>
    public class IceContainer : InteractableBase
    {
        #region 序列化欄位

        [Header("Ice Container Setup")]
        [Tooltip("冰塊預製物")]
        [SerializeField] private GameObject iceCubePrefab;

        [Tooltip("冰塊生成位置")]
        [SerializeField] private Transform spawnPoint;

        [Tooltip("是否有無限冰塊")]
        [SerializeField] private bool infiniteIce = true;

        [Tooltip("初始冰塊數量")]
        [SerializeField] private int initialIceCount = 20;

        #endregion

        #region 私有欄位

        private int currentIceCount;

        #endregion

        #region Unity 生命週期

        protected override void Awake()
        {
            base.Awake();

            interactableType = InteractableType.Station;
            displayName = "冰桶";
            canPickup = false;

            currentIceCount = initialIceCount;
        }

        private void Start()
        {
            // Setup spawn point if not assigned
            if (spawnPoint == null)
            {
                GameObject spawnObj = new GameObject("IceSpawnPoint");
                spawnObj.transform.SetParent(transform);
                spawnObj.transform.localPosition = Vector3.up * 0.15f;
                spawnPoint = spawnObj.transform;
            }
        }

        #endregion

        #region 互動

        public override void OnInteract()
        {
            base.OnInteract();

            // 生成冰塊
            if (infiniteIce || currentIceCount > 0)
            {
                SpawnIceCube();
                if (!infiniteIce)
                {
                    currentIceCount--;
                }
            }
            else
            {
                Debug.Log("IceContainer: No ice left!");
            }
        }

        /// <summary>
        /// 生成冰塊
        /// </summary>
        private void SpawnIceCube()
        {
            GameObject iceObj;

            if (iceCubePrefab != null)
            {
                iceObj = Instantiate(iceCubePrefab, spawnPoint.position, Quaternion.identity);
            }
            else
            {
                // 創建簡單的冰塊
                iceObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                iceObj.name = "IceCube";
                iceObj.transform.position = spawnPoint.position;
                iceObj.transform.localScale = Vector3.one * 0.05f;

                // 設置透明藍色材質
                var renderer = iceObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var material = new Material(Shader.Find("Standard"));
                    material.color = new Color(0.7f, 0.9f, 1f, 0.5f);
                    material.SetFloat("_Mode", 3); // Transparent mode
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    renderer.material = material;
                }

                // 添加 Rigidbody
                var rb = iceObj.AddComponent<Rigidbody>();
                rb.mass = 0.05f;
                rb.useGravity = true;

                // 添加 IceCube 組件
                var iceCube = iceObj.AddComponent<IceCube>();
            }

            Debug.Log("IceContainer: Spawned ice cube");
        }

        #endregion

        #region 公開屬性

        /// <summary>
        /// 剩餘冰塊數量
        /// </summary>
        public int RemainingIce => currentIceCount;

        /// <summary>
        /// 是否有無限冰塊
        /// </summary>
        public bool HasInfiniteIce => infiniteIce;

        #endregion
    }
}
