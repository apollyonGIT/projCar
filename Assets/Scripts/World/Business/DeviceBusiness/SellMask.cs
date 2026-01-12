using Commons;
using TMPro;
using UnityEngine;
using World.Devices;
using World.Helpers;

namespace World.Business
{
    public class SellMask : MonoBehaviour
    {
        public DevicePurchaseView dpv;

        public TextMeshProUGUI price_text;

        public void Sell(Device device)
        {
            Safe_Area_Helper.TryToSellDevice(dpv.owner);
        }

        public void Init(Device data)
        {
            var price_value = Mathf.CeilToInt(data.desc.base_value * Config.current.shop_device_sell_coef);

            price_text.text = ((int)(data.desc.base_value * Config.current.shop_device_sell_coef)).ToString();
        }
    }
}
