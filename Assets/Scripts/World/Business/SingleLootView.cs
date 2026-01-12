using Addrs;
using AutoCodes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Helpers;

namespace World.Business
{
    public class SingleLootView :MonoBehaviour
    {
        [HideInInspector]
        public int loot_id;

        public Image coin_image;
        public TextMeshProUGUI coin_text;

        public void Init(int id)
        {
            loot_id = id;
            loots.records.TryGetValue(id.ToString(), out var record);
            Addressable_Utility.try_load_asset<Sprite>(record.view, out var sprite);
            coin_image.sprite = sprite;
            coin_text.text = Safe_Area_Helper.GetLootCount(id).ToString();
        }
    }
}
