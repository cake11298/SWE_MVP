using UnityEngine;
using BarSimulator.Objects;
using BarSimulator.Data;

namespace BarSimulator.Tests
{
    /// <summary>
    /// 測試Shaker倒酒功能的腳本
    /// 按T鍵測試倒酒功能
    /// </summary>
    public class ShakerPouringTest : MonoBehaviour
    {
        [Header("測試對象")]
        [SerializeField] private Shaker testShaker;
        [SerializeField] private Glass testGlass;
        
        [Header("測試設定")]
        [SerializeField] private bool autoFindObjects = true;
        [SerializeField] private float testPourAmount = 50f;

        private void Start()
        {
            if (autoFindObjects)
            {
                if (testShaker == null)
                {
                    testShaker = FindObjectOfType<Shaker>();
                }
                
                if (testGlass == null)
                {
                    // 尋找名為 CrystalGlass 的物件
                    GameObject glassObj = GameObject.Find("CrystalGlass");
                    if (glassObj != null)
                    {
                        testGlass = glassObj.GetComponent<Glass>();
                    }
                    
                    // 如果還是找不到，就找第一個Glass
                    if (testGlass == null)
                    {
                        testGlass = FindObjectOfType<Glass>();
                    }
                }
            }

            if (testShaker != null)
            {
                Debug.Log($"[ShakerPouringTest] Found Shaker: {testShaker.name}");
                Debug.Log($"[ShakerPouringTest] Shaker Volume: {testShaker.Volume}ml, IsEmpty: {testShaker.IsEmpty}");
            }
            else
            {
                Debug.LogWarning("[ShakerPouringTest] No Shaker found!");
            }

            if (testGlass != null)
            {
                Debug.Log($"[ShakerPouringTest] Found Glass: {testGlass.name}");
                Debug.Log($"[ShakerPouringTest] Glass Volume: {testGlass.Volume}ml, IsEmpty: {testGlass.IsEmpty}");
            }
            else
            {
                Debug.LogWarning("[ShakerPouringTest] No Glass found!");
            }
        }

        private void Update()
        {
            // 按T鍵測試
            if (Input.GetKeyDown(KeyCode.T))
            {
                TestShakerPouring();
            }

            // 按Y鍵添加測試液體到Shaker
            if (Input.GetKeyDown(KeyCode.Y))
            {
                AddTestLiquidToShaker();
            }

            // 按U鍵清空Shaker
            if (Input.GetKeyDown(KeyCode.U))
            {
                ClearShaker();
            }

            // 按I鍵清空Glass
            if (Input.GetKeyDown(KeyCode.I))
            {
                ClearGlass();
            }

            // 按O鍵顯示狀態
            if (Input.GetKeyDown(KeyCode.O))
            {
                ShowStatus();
            }
        }

        private void TestShakerPouring()
        {
            if (testShaker == null || testGlass == null)
            {
                Debug.LogError("[ShakerPouringTest] Shaker or Glass is null!");
                return;
            }

            if (testShaker.IsEmpty)
            {
                Debug.LogWarning("[ShakerPouringTest] Shaker is empty! Press Y to add test liquid first.");
                return;
            }

            Debug.Log($"[ShakerPouringTest] === Testing Shaker Pouring ===");
            Debug.Log($"[ShakerPouringTest] Before - Shaker: {testShaker.Volume:F1}ml, Glass: {testGlass.Volume:F1}ml");

            // 測試倒酒
            float transferred = testShaker.TransferTo(testGlass, testPourAmount);

            Debug.Log($"[ShakerPouringTest] Transferred: {transferred:F1}ml");
            Debug.Log($"[ShakerPouringTest] After - Shaker: {testShaker.Volume:F1}ml, Glass: {testGlass.Volume:F1}ml");
            
            if (transferred > 0)
            {
                Debug.Log($"[ShakerPouringTest] ✓ SUCCESS: Shaker poured {transferred:F1}ml to Glass!");
            }
            else
            {
                Debug.LogError($"[ShakerPouringTest] ✗ FAILED: No liquid transferred!");
            }
        }

        private void AddTestLiquidToShaker()
        {
            if (testShaker == null)
            {
                Debug.LogError("[ShakerPouringTest] Shaker is null!");
                return;
            }

            // 添加測試液體（Gin + Vermouth）
            var liquorDatabase = Resources.Load<LiquorDatabase>("LiquorDataBase");
            if (liquorDatabase != null)
            {
                var gin = liquorDatabase.GetLiquor("gin");
                var vermouth = liquorDatabase.GetLiquor("vermouth");

                if (gin != null)
                {
                    testShaker.AddLiquor(gin, 50f);
                    Debug.Log($"[ShakerPouringTest] Added 50ml Gin to Shaker");
                }

                if (vermouth != null)
                {
                    testShaker.AddLiquor(vermouth, 20f);
                    Debug.Log($"[ShakerPouringTest] Added 20ml Vermouth to Shaker");
                }

                Debug.Log($"[ShakerPouringTest] Shaker now has {testShaker.Volume:F1}ml");
            }
            else
            {
                Debug.LogError("[ShakerPouringTest] LiquorDatabase not found!");
            }
        }

        private void ClearShaker()
        {
            if (testShaker == null)
            {
                Debug.LogError("[ShakerPouringTest] Shaker is null!");
                return;
            }

            testShaker.Clear();
            Debug.Log($"[ShakerPouringTest] Cleared Shaker");
        }

        private void ClearGlass()
        {
            if (testGlass == null)
            {
                Debug.LogError("[ShakerPouringTest] Glass is null!");
                return;
            }

            testGlass.Clear();
            Debug.Log($"[ShakerPouringTest] Cleared Glass");
        }

        private void ShowStatus()
        {
            Debug.Log($"[ShakerPouringTest] === Current Status ===");
            
            if (testShaker != null)
            {
                Debug.Log($"[ShakerPouringTest] Shaker: {testShaker.Volume:F1}ml / {testShaker.MaxVolume:F1}ml");
                Debug.Log($"[ShakerPouringTest] Shaker Contents: {testShaker.GetContentsString()}");
                Debug.Log($"[ShakerPouringTest] Shaker IsShaken: {testShaker.IsShaken}");
            }
            
            if (testGlass != null)
            {
                Debug.Log($"[ShakerPouringTest] Glass: {testGlass.Volume:F1}ml / {testGlass.MaxVolume:F1}ml");
                Debug.Log($"[ShakerPouringTest] Glass Contents: {testGlass.GetContentsString()}");
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== Shaker Pouring Test ===");
            GUILayout.Label("T - Test Pouring (Shaker -> Glass)");
            GUILayout.Label("Y - Add Test Liquid to Shaker");
            GUILayout.Label("U - Clear Shaker");
            GUILayout.Label("I - Clear Glass");
            GUILayout.Label("O - Show Status");
            GUILayout.Space(10);
            
            if (testShaker != null)
            {
                GUILayout.Label($"Shaker: {testShaker.Volume:F0}ml / {testShaker.MaxVolume:F0}ml");
            }
            
            if (testGlass != null)
            {
                GUILayout.Label($"Glass: {testGlass.Volume:F0}ml / {testGlass.MaxVolume:F0}ml");
            }
            
            GUILayout.EndArea();
        }
    }
}
