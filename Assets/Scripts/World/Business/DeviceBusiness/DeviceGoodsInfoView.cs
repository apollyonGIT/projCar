using AutoCodes;
using Commons;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using World.Devices;

namespace World.Business
{
    public class DeviceGoodsInfoView:MonoBehaviour
    {
        public TextMeshProUGUI device_name;
        public TextMeshProUGUI device_tag_info;
        public TextMeshProUGUI device_hp;
        public TextMeshProUGUI device_def;
        public TextMeshProUGUI device_weight;
        public TextMeshProUGUI device_Crit_Chance;
        public TextMeshProUGUI device_Crit_Rate;
        public TextMeshProUGUI device_atk_data;

        public void Init(Device device)
        {
            battle_datas.TryGetValue(device.desc.id.ToString(), out var battle_data);

            device_name.text = Localization_Utility.get_localization(device.desc.name);
            device_tag_info.text = get_device_tag_name(device);

            device_hp.text = "HP: " + battle_data.hp.ToString();
            device_def.text = "DEF: " + battle_data.def.ToString();
            device_weight.text = "重量: " + device.desc.weight.ToString() + " kg";
            device_Crit_Chance.text = "CRIT-Chance: " + (battle_data.critical_chance * 0.1f).ToString("F1") + "%";
            device_Crit_Rate.text = "CRIT-Rate: " + (battle_data.critical_rate * 0.1f).ToString("F1") + "%";
            var dmg = get_device_dmg_text(device);
            device_atk_data.text = "ATK: " + dmg;
        }

        private string get_device_tag_name(Device device)
        {
            string str = "";
            foreach (var tags in device.desc.device_tag)
            {
                device_tags.TryGetValue(tags.ToString(), out var device_tags_db);
                str += Localization_Utility.get_localization(device_tags_db.tag_name) + ", ";
            }
            return str;
        }

        private string get_device_dmg_text(Device device)
        {
            if (device.desc.melee_logic != 0)
            {
                melee_logics.TryGetValue(device.desc.melee_logic.ToString(), out var db);
                return convert_device_dmg_to_string(db.damage);
            }
            if (device.desc.fire_logic != 0)
            {
                fire_logics.TryGetValue(device.desc.fire_logic.ToString(), out var db);
                var salvo = db.salvo > 1 ? $"\n齐射: {db.salvo}" : "";
                var text_damage = $"{convert_device_dmg_to_string(db.damage)}";
                var text_reloading = $"\n装填: {(float)db.ui_reloading_ticks / 120:F1} 秒";
                return text_damage + salvo + text_reloading;
            }
            if (device.desc.shield_logic != 0)
            {
                shield_logics.TryGetValue(device.desc.shield_logic.ToString(), out var db);
                return convert_device_dmg_to_string(db.damage);
            }
            if (device.desc.ram_logic != 0)
            {
                ram_logics.TryGetValue(device.desc.ram_logic.ToString(), out var db);
                return convert_device_dmg_to_string(db.damage);
            }
            if (device.desc.other_logic != 0)
            {
                other_logics.TryGetValue(device.desc.other_logic.ToString(), out var db);
                return convert_device_dmg_to_string(db.damage);
            }
            return "--";
        }

        private string convert_device_dmg_to_string(Dictionary<string, int> raw_dmg)
        {
            string dmg_string = "";
            bool first_kv_pair = true;

            if (raw_dmg == null)
                return "0";

            foreach (var dmg in raw_dmg)
            {
                if (first_kv_pair)
                    first_kv_pair = false;
                else
                    dmg_string += " + ";

                string c = dmg.Key switch
                {
                    "blunt" => ColorUtility.ToHtmlStringRGBA(Config.current.blunt_color),
                    "fire" => ColorUtility.ToHtmlStringRGBA(Config.current.fire_color),
                    "acid" => ColorUtility.ToHtmlStringRGBA(Config.current.acid_color),
                    "wrap" => ColorUtility.ToHtmlStringRGBA(Config.current.wrap_color),
                    "flash" => ColorUtility.ToHtmlStringRGBA(Config.current.flash_color),
                    _ => ColorUtility.ToHtmlStringRGBA(Config.current.normal_ui_color),
                };
                dmg_string += $"<color=#{c}>{dmg.Value}</color>";
            }

            return dmg_string == "" ? "--" : dmg_string;
        }
    }
}
