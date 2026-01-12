using Addrs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace World.Settlement
{
    public class SingleCampCoin :MonoBehaviour
    {
        public Image camp_coin_icon;
        public TextMeshProUGUI camp_coin_cnt;

        public void Init((int, int) cp)
        {
            AutoCodes.camp_coins.TryGetValue(cp.Item1.ToString(), out var camp_coin_desc);
            Addressable_Utility.try_load_asset<Sprite>(camp_coin_desc.icon, out var icon_sprite);
            camp_coin_icon.sprite = icon_sprite;
            camp_coin_cnt.text = cp.Item2.ToString();
        }
    }
}
