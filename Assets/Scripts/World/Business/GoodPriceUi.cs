using TMPro;
using UnityEngine;

namespace World.Business
{
    public class GoodPriceUi : MonoBehaviour
    {
        public DiscountUi discount_ui;
        public TextMeshProUGUI price_text;
        public void Init(int price,float discount)
        {

            price_text.text = $"{price}";
            if(discount == 1)
            {
                discount_ui.gameObject.SetActive(false);
            }
            else
            {
                discount_ui.gameObject.SetActive(true);
                discount_ui.Init(price, discount);
            }
        }
    }
}
