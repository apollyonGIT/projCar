using Addrs;
using AutoCodes;
using Commons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Helpers;

namespace World.Devices.DeviceUpgrades
{
    public class DeviceUpgradeConsumeView : MonoBehaviour
    {
        public Image consume_icon;
        public TextMeshProUGUI amount;
        public TextMeshProUGUI nameText;
        public void Init(uint key, int value)
        {
            loots.TryGetValue(key.ToString(), out var record);

            nameText.text = Localization_Utility.get_localization(record.name);

            Addressable_Utility.try_load_asset<Sprite>(record.view, out var sprite);
            consume_icon.sprite = sprite;

            var back_value = Upgrade_Cost_Helper.GetLootAmount((int)key);
            if(back_value >= value)
            {
                amount.text = $"{back_value}/{value}";
            }
            else
            {
                amount.text = $"<color=red>{back_value}</color>/{value}";
            }
        }
    }
}
