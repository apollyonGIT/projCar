using AutoCodes;
using Commons;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews
{
    public interface IAltUi
    {
        void Begin();
        void End();
    }


    public class DeviceHoveringInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IAltUi
    {
        private const string DEVICE_INFO_DEFAULT = "按住alt查看更多信息";

        public GameObject hovering_info;
        public TextMeshProUGUI device_name;
        public TextMeshProUGUI device_tag_info;
        public TextMeshProUGUI device_basic_data;
        public TextMeshProUGUI device_atk_data;
        public TextMeshProUGUI alt_text;

        private string alt_text_in_table;

        public Device data;

        // ===============================================================================

        public void Init(Device device)
        {
            battle_datas.TryGetValue(device.desc.id.ToString(), out var battle_data);

            device_name.text = Localization_Utility.get_localization(device.desc.name);
            device_tag_info.text = get_device_tag_name(device);

            var text_hp = "耐久: " + battle_data.hp.ToString();
            var text_def = "护甲: " + battle_data.def.ToString();
            var text_weight = "重量: " + device.desc.weight.ToString() + " kg";

            device_basic_data.text = text_hp + "\n" + text_def + "\n" + text_weight;

            var dmg = get_device_dmg_text(device);
            device_atk_data.text = "伤害: " + dmg;

            alt_text_in_table = Localization_Utility.get_localization(device.desc.alt_info);
            alt_text.text = DEVICE_INFO_DEFAULT;
        }

        // ===============================================================================

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

        // ===============================================================================

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovering_info.SetActive(true);
            if (data != null)
                Init(data);
            alt_resize();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering_info.SetActive(false);
            alt_text.text = DEVICE_INFO_DEFAULT;
        }

        void IAltUi.Begin()
        {
            alt_text.text = alt_text_in_table;
            alt_resize();
        }

        void IAltUi.End()
        {
            alt_text.text = DEVICE_INFO_DEFAULT;
            alt_resize();
        }

        private void alt_resize()
        {
            if (hovering_info.activeSelf == false)
                return;

            alt_text.SetLayoutDirty();
            alt_text.gameObject.GetComponentInParent<VerticalLayoutGroup>().SetLayoutVertical();
            alt_text.gameObject.GetComponentInParent<ContentSizeFitter>().SetLayoutVertical();
            device_basic_data.gameObject.GetComponentInParent<ContentSizeFitter>().SetLayoutVertical();
            device_atk_data.gameObject.GetComponentInParent<ContentSizeFitter>().SetLayoutVertical();
        }

    }
}
