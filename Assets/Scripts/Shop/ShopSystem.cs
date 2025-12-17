using BarSimulator.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    public static ShopSystem Instance { get; private set; }
    public List<BarUpgrade_Decoration> upgradeDecorations = new List<BarUpgrade_Decoration>();
    void Start()
    {
        Instance = this;
    }
    
    public bool TryPurchaseItem(ShopItem shopItem)
    {
        // 1. 檢查是否已購買
        if (shopItem.isPurchaced)
        {
            Debug.Log("已擁有這件物品!");
            return false;
        }

        // 2. 檢查錢夠不夠
        if (GameManager.Instance.GetScore().totalCoins < shopItem.price)
        {
            Debug.Log("金錢不足！");
            return false;
        }

        // 3. 執行交易
        GameManager.Instance.GetScore().totalCoins -= shopItem.price; // 扣錢
        shopItem.OnPurchaceSuccess();
        return true; // 購買成功
    }
}



public class ShopItem
{
    public Sprite itemIcon;
    public string name;
    [TextArea]
    public string description;
    public int price;

    public bool isPurchaced = false;
    public virtual void OnPurchaceSuccess()
    {

    }
}

[System.Serializable]
public class BarUpgrade_Decoration : ShopItem
{
    public List<GameObject> unlockedDecorations;

    public override void OnPurchaceSuccess()
    {
        foreach(var deco in unlockedDecorations)
        {
            deco.SetActive(true);
        }
        this.isPurchaced = true;
    }
}

