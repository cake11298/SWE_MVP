using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;
using UnityEditor;

public class SetupMainMenuSettings : MonoBehaviour
{
    public static void Execute()
    {
        SimpleMainMenu menu = FindFirstObjectByType<SimpleMainMenu>();
        if (menu == null)
        {
            Debug.LogError("SimpleMainMenu component not found!");
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // 1. Create Settings Button
        GameObject btnObj = new GameObject("SettingsButton");
        btnObj.transform.SetParent(canvas.transform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = Color.white;
        Button btn = btnObj.AddComponent<Button>();
        
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = "Settings";
        txt.fontSize = 24;
        txt.color = Color.black;
        txt.alignment = TextAlignmentOptions.Center;
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(200, 60);
        btnRect.anchoredPosition = new Vector2(0, -100); // Adjust position as needed (assuming center)
        
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        // Link Button to ToggleSettings
        btn.onClick.AddListener(menu.ToggleSettings);

        // 2. Create Settings Panel
        GameObject panelObj = new GameObject("SettingsPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.9f);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelObj.SetActive(false);

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(panelObj.transform, false);
        Image closeImg = closeBtnObj.AddComponent<Image>();
        closeImg.color = Color.red;
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        closeBtn.onClick.AddListener(menu.ToggleSettings);
        
        GameObject closeTxtObj = new GameObject("Text");
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        TextMeshProUGUI closeTxt = closeTxtObj.AddComponent<TextMeshProUGUI>();
        closeTxt.text = "X";
        closeTxt.fontSize = 24;
        closeTxt.alignment = TextAlignmentOptions.Center;
        
        RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-50, -50);
        closeRect.sizeDelta = new Vector2(50, 50);

        // Quality Label
        GameObject labelObj = new GameObject("QualityLabel");
        labelObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI labelTxt = labelObj.AddComponent<TextMeshProUGUI>();
        labelTxt.text = "Quality Settings";
        labelTxt.fontSize = 36;
        labelTxt.color = Color.white;
        labelTxt.alignment = TextAlignmentOptions.Center;
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(0, 100);
        labelRect.sizeDelta = new Vector2(400, 50);

        // Quality Dropdown
        // Creating a TMP Dropdown from scratch via code is painful due to template structure.
        // We will create a minimal functional structure.
        GameObject dropdownObj = DefaultControls.CreateDropdown(new DefaultControls.Resources());
        dropdownObj.name = "QualityDropdown";
        dropdownObj.transform.SetParent(panelObj.transform, false);
        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        // Remove legacy Dropdown, add TMP_Dropdown (Wait, DefaultControls creates legacy UI)
        // Let's just use the legacy one or try to swap. 
        // Actually, SimpleMainMenu uses TMP_Dropdown.
        // Let's destroy the legacy component and add TMP one? No, structure differs.
        
        // Alternative: Just use a simple button cycle for now to save complexity, 
        // OR try to instantiate a TMP Dropdown prefab if available.
        // OR just build the structure manually.
        
        DestroyImmediate(dropdownObj); // Remove legacy
        
        // Create simplified Dropdown structure for TMP
        dropdownObj = new GameObject("QualityDropdown");
        dropdownObj.transform.SetParent(panelObj.transform, false);
        Image ddImg = dropdownObj.AddComponent<Image>();
        dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        
        RectTransform ddRect = dropdownObj.GetComponent<RectTransform>();
        ddRect.sizeDelta = new Vector2(300, 50);
        
        // Label
        GameObject ddLabel = new GameObject("Label");
        ddLabel.transform.SetParent(dropdownObj.transform, false);
        TextMeshProUGUI ddLabelTxt = ddLabel.AddComponent<TextMeshProUGUI>();
        ddLabelTxt.text = "Quality";
        ddLabelTxt.color = Color.black;
        ddLabelTxt.alignment = TextAlignmentOptions.Left;
        RectTransform ddLabelRect = ddLabel.GetComponent<RectTransform>();
        ddLabelRect.anchorMin = Vector2.zero;
        ddLabelRect.anchorMax = Vector2.one;
        ddLabelRect.offsetMin = new Vector2(10, 0);
        ddLabelRect.offsetMax = new Vector2(-30, 0);
        dropdown.captionText = ddLabelTxt;

        // Template (The popup)
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropdownObj.transform, false);
        Image templateImg = template.AddComponent<Image>();
        ScrollRect scroll = template.AddComponent<ScrollRect>();
        template.SetActive(false);
        dropdown.template = template.GetComponent<RectTransform>();
        
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        Image viewportImg = viewport.AddComponent<Image>();
        Mask viewportMask = viewport.AddComponent<Mask>();
        scroll.viewport = viewport.GetComponent<RectTransform>();
        
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        scroll.content = contentRect;
        
        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        Toggle itemToggle = item.AddComponent<Toggle>();
        
        GameObject itemLabel = new GameObject("Item Label");
        itemLabel.transform.SetParent(item.transform, false);
        TextMeshProUGUI itemTxt = itemLabel.AddComponent<TextMeshProUGUI>();
        dropdown.itemText = itemTxt;

        // Assign to script
        SerializedObject so = new SerializedObject(menu);
        so.FindProperty("settingsPanel").objectReferenceValue = panelObj;
        so.FindProperty("qualityDropdown").objectReferenceValue = dropdown;
        so.ApplyModifiedProperties();
    }
}
