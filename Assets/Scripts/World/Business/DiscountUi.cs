using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace World.Business
{
    public class DiscountUi : MonoBehaviour
    {
        public TextMeshProUGUI discount_text;
        public TextMeshProUGUI price_text;
        public Image line;

        public void Init(int price, float discount)
        {
            price_text.text = ((int)(price * discount)).ToString();
            price_text.GetComponent<RectTransform>().sizeDelta = new Vector2(60 + (3 - price.ToString().Length) * 20, 30);
            discount_text.text = $"-{((1 - discount) * 100)}%";
            line.GetComponent<RectTransform>().sizeDelta = new Vector2(price.ToString().Length * 20, 4);
        }
    }
}
