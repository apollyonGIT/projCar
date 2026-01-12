using Addrs;
using AutoCodes;
using Commons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Devices;

namespace World.SafeArea
{
    public class Fix_Device : MonoBehaviour
    {
        public Device data;

        public Image device_icon;
        public Image device_bg;
        public Slider device_hp;

        public TextMeshProUGUI device_text;
        public TextMeshProUGUI hp_text;
        public TextMeshProUGUI fix_cost;

        [HideInInspector]
        public Fix_Window owner;


        public void Init(Device d,Fix_Window owner)
        {
            data = d;
            this.owner = owner;


            var rank = d.desc.rank;
            device_ranks.TryGetValue(rank.ToString(), out var rank_db);
            Addressable_Utility.try_load_asset<Sprite>(rank_db.img_name_bar, out var s);
            device_bg.sprite = s;
            device_text.text = Localization_Utility.get_localization(d.desc.name);

            Addressable_Utility.try_load_asset<Sprite>(d.desc.icon_photo, out var icon);
            device_icon.sprite = icon;

            device_hp.maxValue = data.battle_data.hp;
            device_hp.value = data.current_hp;

            hp_text.text = $"{data.current_hp}/{data.battle_data.hp}";
            fix_cost.text = $"{owner.GetFixCost()}";
        }

        public void Fix()
        {
            owner.TryFix(data);  
        }

    }
}
