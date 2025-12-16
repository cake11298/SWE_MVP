using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ShopItemPlaceholder : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI text_itemName;
    public TextMeshProUGUI text_itemDescription;
    public TextMeshProUGUI text_itemPrice;
    public void UpdateView(ShopItem shopItem)
    {
        itemImage.sprite = shopItem.itemIcon;
        text_itemName.text = shopItem.name;
        text_itemDescription.text = shopItem.description;
        text_itemPrice.text = "$"+shopItem.price.ToString() +"\nPurchace";
    }
}
