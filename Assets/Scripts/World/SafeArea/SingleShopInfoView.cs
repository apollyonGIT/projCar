using Addrs;
using AutoCodes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace World.SafeArea
{
    public class SingleShopInfoView : MonoBehaviour
    {
        public Image shop_icon;
        public TextMeshProUGUI shop_name;

        public void Init(shop_ui data)
        {
            Addressable_Utility.try_load_asset<Sprite>(data.icon, out var icon_sprite);
            shop_icon.sprite = icon_sprite;
            shop_name.text = Commons.Localization_Utility.get_localization(data.name);
        }


        public void Init(string icon_path,string icon_name)
        {
            Addressable_Utility.try_load_asset<Sprite>(icon_path, out var icon_sprite);
            shop_icon.sprite = icon_sprite;
            shop_name.text = Commons.Localization_Utility.get_localization(icon_name);
        }
    }
}
