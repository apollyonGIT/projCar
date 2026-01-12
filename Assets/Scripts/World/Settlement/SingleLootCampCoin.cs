using Addrs;
using Commons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.BackPack;

namespace World.Settlement
{
    public class SingleLootCampCoin : MonoBehaviour
    {
        public Transform content;
        public SingleCampCoin prefab;

        public GameObject broken;

        public Image loot_icon;
        public TextMeshProUGUI loot_text;

        public void Init(SingelLootSettlementData data)
        {
            if (loot_icon != null)
            {
                Addressable_Utility.try_load_asset<Sprite>(data.loot.desc.view, out var loot_sprite);
                loot_icon.sprite = loot_sprite;
            }
            loot_text.text =Localization_Utility.get_localization(data.loot.desc.name);
            if (data.is_broken)
            {
                loot_text.color = Color.red;
                content.gameObject.SetActive(false);
                broken.SetActive(true);
            }
            else
            {
                foreach(var cp in data.camp_coins)
                {
                    var camp_coin = Instantiate(prefab, content, false);
                    camp_coin.Init(cp);
                    camp_coin.gameObject.SetActive(true);
                }
            }
        }
    }
}
