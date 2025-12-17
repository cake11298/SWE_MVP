using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BarSimulator.Data;

namespace BarSimulator.UI
{
    public class ShopItemView : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI infoText;
        public Button actionButton;
        public TextMeshProUGUI buttonText;
        
        [Header("Item Configuration")]
        public bool isLiquor;
        public BaseLiquorType liquorType;
        public DecorationType decorationType;
    }
}
