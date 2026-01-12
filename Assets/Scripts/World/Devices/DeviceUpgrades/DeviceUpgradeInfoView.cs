using AutoCodes;
using Commons;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace World.Devices.DeviceUpgrades
{
    public class DeviceUpgradeInfoView : MonoBehaviour
    {
        public TextMeshProUGUI pre;
        public TextMeshProUGUI incompation;
        public TextMeshProUGUI ef_description;
        public TextMeshProUGUI bg_descripition;

        public Transform content;
        public DeviceUpgradeConsumeView prefab;
        public List<DeviceUpgradeConsumeView> consume_list = new();

        public DeviceUpgrade deviceUpgrade;

        public void Init(DeviceUpgrade deviceUpgrade)
        {
            this.deviceUpgrade = deviceUpgrade;
        }

        public void tick()
        {
            if (deviceUpgrade != null)
            {
                ef_description.text = Localization_Utility.get_localization(deviceUpgrade.desc.desc);
                bg_descripition.text = Localization_Utility.get_localization(deviceUpgrade.desc.story_desc);

                string pre_text = "";
                if (deviceUpgrade.desc.precondition != null)
                {
                    foreach (var pre_id in deviceUpgrade.desc.precondition)
                    {
                        device_upgrades.TryGetValue($"{deviceUpgrade.desc.id},{pre_id}", out var record);
                        pre_text += $"[{Localization_Utility.get_localization(record.name)}] ";
                    }
                }

                string inc_text = "";
                if (deviceUpgrade.desc.incompatible != null)
                {
                    foreach (var inc_id in deviceUpgrade.desc.incompatible)
                    {
                        device_upgrades.TryGetValue($"{deviceUpgrade.desc.id},{inc_id}", out var record);
                        inc_text += $"[{Localization_Utility.get_localization(record.name)}] ";
                    }
                }

                pre.text = pre_text;
                incompation.text = inc_text;

                foreach (var cv in consume_list)
                {
                    Destroy(cv.gameObject);
                }
                consume_list.Clear();

                foreach (var consume in deviceUpgrade.desc.cost)
                {
                    var c = Instantiate(prefab, content, false);
                    c.Init(consume.Key, consume.Value);
                    c.gameObject.SetActive(true);

                    consume_list.Add(c);
                }
            }
        }
    }
}
