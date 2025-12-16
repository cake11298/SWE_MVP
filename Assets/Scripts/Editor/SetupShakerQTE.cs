using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using BarSimulator.Objects;
using BarSimulator.Systems;
using BarSimulator.UI;
using BarSimulator.Player;

public class SetupShakerQTE
{
    [MenuItem("Tools/Setup Shaker QTE System")]
    public static void Execute()
    {
        // 1. 找到Shaker
        GameObject shakerObj = GameObject.Find("Shaker");
        if (shakerObj == null)
        {
            Debug.LogError("Shaker not found in scene!");
            return;
        }

        // 2. 添加ShakerQTESystem组件（如果没有）
        ShakerQTESystem qteSystem = shakerObj.GetComponent<ShakerQTESystem>();
        if (qteSystem == null)
        {
            qteSystem = shakerObj.AddComponent<ShakerQTESystem>();
            Debug.Log("Added ShakerQTESystem to Shaker");
        }

        // 3. 找到Player并添加ShakerController
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player not found in scene!");
            return;
        }

        ShakerController shakerController = player.GetComponent<ShakerController>();
        if (shakerController == null)
        {
            shakerController = player.AddComponent<ShakerController>();
            Debug.Log("Added ShakerController to Player");
        }

        // 4. 创建QTE UI
        GameObject canvas = GameObject.Find("UI_Canvas");
        if (canvas == null)
        {
            Debug.LogError("UI_Canvas not found!");
            return;
        }

        // 检查是否已存在QTE UI
        Transform existingQTE = canvas.transform.Find("ShakerQTE_UI");
        if (existingQTE != null)
        {
            Debug.Log("ShakerQTE_UI already exists, updating...");
            Object.DestroyImmediate(existingQTE.gameObject);
        }

        // 创建QTE UI根对象
        GameObject qteUIRoot = new GameObject("ShakerQTE_UI");
        qteUIRoot.transform.SetParent(canvas.transform, false);
        
        RectTransform qteRootRect = qteUIRoot.AddComponent<RectTransform>();
        qteRootRect.anchorMin = Vector2.zero;
        qteRootRect.anchorMax = Vector2.one;
        qteRootRect.sizeDelta = Vector2.zero;
        qteRootRect.anchoredPosition = Vector2.zero;

        // 添加ShakerQTEUI组件
        ShakerQTEUI qteUI = qteUIRoot.AddComponent<ShakerQTEUI>();

        // 创建技能检查UI
        GameObject skillCheckRoot = new GameObject("SkillCheck");
        skillCheckRoot.transform.SetParent(qteUIRoot.transform, false);
        
        RectTransform skillCheckRect = skillCheckRoot.AddComponent<RectTransform>();
        skillCheckRect.anchorMin = new Vector2(0.5f, 0.5f);
        skillCheckRect.anchorMax = new Vector2(0.5f, 0.5f);
        skillCheckRect.sizeDelta = new Vector2(300f, 300f);
        skillCheckRect.anchoredPosition = Vector2.zero;

        // 创建圆形背景
        GameObject circleBG = new GameObject("CircleBackground");
        circleBG.transform.SetParent(skillCheckRoot.transform, false);
        
        RectTransform circleBGRect = circleBG.AddComponent<RectTransform>();
        circleBGRect.anchorMin = Vector2.zero;
        circleBGRect.anchorMax = Vector2.one;
        circleBGRect.sizeDelta = Vector2.zero;
        circleBGRect.anchoredPosition = Vector2.zero;
        
        Image circleBGImage = circleBG.AddComponent<Image>();
        circleBGImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        circleBGImage.sprite = CreateCircleSprite();

        // 创建成功区域
        GameObject successZone = new GameObject("SuccessZone");
        successZone.transform.SetParent(skillCheckRoot.transform, false);
        
        RectTransform successZoneRect = successZone.AddComponent<RectTransform>();
        successZoneRect.anchorMin = new Vector2(0.5f, 0.5f);
        successZoneRect.anchorMax = new Vector2(0.5f, 0.5f);
        successZoneRect.sizeDelta = new Vector2(300f, 30f);
        successZoneRect.anchoredPosition = new Vector2(0f, 150f);
        successZoneRect.pivot = new Vector2(0.5f, 1f);
        
        Image successZoneImage = successZone.AddComponent<Image>();
        successZoneImage.color = new Color(0f, 1f, 0f, 0.8f);

        // 创建指针
        GameObject needle = new GameObject("Needle");
        needle.transform.SetParent(skillCheckRoot.transform, false);
        
        RectTransform needleRect = needle.AddComponent<RectTransform>();
        needleRect.anchorMin = new Vector2(0.5f, 0.5f);
        needleRect.anchorMax = new Vector2(0.5f, 0.5f);
        needleRect.sizeDelta = new Vector2(10f, 150f);
        needleRect.anchoredPosition = Vector2.zero;
        needleRect.pivot = new Vector2(0.5f, 0f);
        
        Image needleImage = needle.AddComponent<Image>();
        needleImage.color = Color.white;

        // 创建提示文字
        GameObject promptTextObj = new GameObject("PromptText");
        promptTextObj.transform.SetParent(qteUIRoot.transform, false);
        
        RectTransform promptTextRect = promptTextObj.AddComponent<RectTransform>();
        promptTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        promptTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        promptTextRect.sizeDelta = new Vector2(400f, 50f);
        promptTextRect.anchoredPosition = new Vector2(0f, -200f);
        
        Text promptText = promptTextObj.AddComponent<Text>();
        promptText.text = "Hold Right Mouse Button";
        promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        promptText.fontSize = 24;
        promptText.alignment = TextAnchor.MiddleCenter;
        promptText.color = Color.white;

        // 创建结果文字
        GameObject resultTextObj = new GameObject("ResultText");
        resultTextObj.transform.SetParent(qteUIRoot.transform, false);
        
        RectTransform resultTextRect = resultTextObj.AddComponent<RectTransform>();
        resultTextRect.anchorMin = new Vector2(0.5f, 0.3f);
        resultTextRect.anchorMax = new Vector2(0.5f, 0.3f);
        resultTextRect.sizeDelta = new Vector2(600f, 80f);
        resultTextRect.anchoredPosition = Vector2.zero;
        
        Text resultText = resultTextObj.AddComponent<Text>();
        resultText.text = "";
        resultText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        resultText.fontSize = 36;
        resultText.fontStyle = FontStyle.Bold;
        resultText.alignment = TextAnchor.MiddleCenter;
        resultText.color = Color.yellow;

        // 设置UI引用
        var qteUIType = typeof(ShakerQTEUI);
        var qteUIRootField = qteUIType.GetField("qteUIRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var skillCheckRootField = qteUIType.GetField("skillCheckRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var needleTransformField = qteUIType.GetField("needleTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var successZoneImageField = qteUIType.GetField("successZoneImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var promptTextField = qteUIType.GetField("promptText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var resultTextField = qteUIType.GetField("resultText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (qteUIRootField != null) qteUIRootField.SetValue(qteUI, qteUIRoot);
        if (skillCheckRootField != null) skillCheckRootField.SetValue(qteUI, skillCheckRoot);
        if (needleTransformField != null) needleTransformField.SetValue(qteUI, needleRect);
        if (successZoneImageField != null) successZoneImageField.SetValue(qteUI, successZoneImage);
        if (promptTextField != null) promptTextField.SetValue(qteUI, promptText);
        if (resultTextField != null) resultTextField.SetValue(qteUI, resultText);

        // 连接QTE系统和UI
        qteUI.SetQTESystem(qteSystem);

        // 初始隐藏QTE UI
        qteUIRoot.SetActive(false);

        Debug.Log("Shaker QTE System setup complete!");
        Debug.Log("- QTE UI created at UI_Canvas/ShakerQTE_UI");
        Debug.Log("- ShakerController added to Player");
        Debug.Log("- Ready to test: Pick up Shaker and hold Right Mouse Button!");
    }

    private static Sprite CreateCircleSprite()
    {
        // 创建一个简单的圆形sprite
        Texture2D texture = new Texture2D(256, 256);
        Color[] pixels = new Color[256 * 256];
        
        Vector2 center = new Vector2(128, 128);
        float radius = 120f;
        
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    pixels[y * 256 + x] = Color.white;
                }
                else
                {
                    pixels[y * 256 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
    }
}
