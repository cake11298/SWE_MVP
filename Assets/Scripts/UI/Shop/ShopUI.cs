using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
public class ShopUI : MonoBehaviour
{
    public Transform slotPrefab;
    public Transform contentParent;

    public Button closeButton;
    private void Start()
    {
        closeButton.onClick.AddListener(CloseShopUI);
        UpdateShopView();
    }
    void UpdateShopView()
    {
        ClearSlots();
        foreach (ShopItem shopItem in ShopSystem.Instance.upgradeDecorations)
        {
            if (shopItem == null ) continue;
            GameObject inventoryItemSlot = Instantiate(slotPrefab.gameObject, contentParent);
            inventoryItemSlot.name = $"shopItem_{shopItem.name}";

            var placeholder = inventoryItemSlot.GetComponentInChildren<ShopItemPlaceholder>();
            placeholder.UpdateView(shopItem);

            Button purchaceButton = inventoryItemSlot.transform.Find("Button_Purchace").GetComponent<Button>();
            purchaceButton.onClick.AddListener(() => OnPurchaceButtonClicked(shopItem));
        }
    }
    private void ClearSlots()
    {
        if (contentParent == null) return;
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            var child = contentParent.GetChild(i).gameObject;
            Destroy(child);
        }
    }

    void OnPurchaceButtonClicked(ShopItem item)
    {
        bool success = ShopSystem.Instance.TryPurchaseItem(item);

        if (success)
        {
            Debug.Log("購買成功: " + item.name);
            // 購買成功後，重新刷新介面以顯示更新後的庫存或金錢
            UpdateShopView();
        }
    }


    private void OpenShopUI()
    {
        gameObject.SetActive(true);
    }
    private void CloseShopUI()
    {
        gameObject.SetActive(false);
    }
}
