using AutoCodes;
using Commons;
using Foundations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using World.Devices;
using World.Helpers;

namespace World.SafeArea
{
    public class Fix_Window : MonoBehaviour
    {
        public Transform fix_content;
        public Fix_Device prefab;

        public List<Fix_Device> fix_devices = new List<Fix_Device>();

        public TextMeshProUGUI coin_text;

        [HideInInspector]
        public int fix_cnt;

        public void Init()
        {
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            foreach(var slot in dmgr.slots_device)
            {
                var device = slot.slot_device;
                if(device == null) continue;
                if (device.current_hp != device.battle_data.hp)
                {
                    var fix_device = Instantiate(prefab, fix_content,false);
                    fix_device.Init(device, this);
                    fix_device.gameObject.SetActive(true);
                    fix_devices.Add(fix_device);
                }
            }

            coin_text.text = Safe_Area_Helper.GetLootCount((int)Config.current.coin_id).ToString();
        }

        public void Reinit()
        {
            foreach (var d in fix_devices)
            {
                Destroy(d.gameObject);
            }
            fix_devices.Clear();

            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            foreach (var slot in dmgr.slots_device)
            {
                var device = slot.slot_device;
                if (device == null) continue;

                battle_datas.TryGetValue(device.desc.id.ToString(), out var battle_data);
                if (device.current_hp != battle_data.hp)
                {
                    var fix_device = Instantiate(prefab, fix_content, false);
                    fix_device.Init(device, this);
                    fix_device.gameObject.SetActive(true);
                    fix_devices.Add(fix_device);
                }
            }

            coin_text.text = Safe_Area_Helper.GetLootCount((int)Config.current.coin_id).ToString();
        }

        public int GetFixCost()
        {
            var ds = Share_DS.instance;
            ds.try_get_value(Game_Mgr.key_scene_index, out int scene_index);
            scene_index = scene_index + 1;
            return Mathf.CeilToInt(fix_cnt * (scene_index * Config.current.shop_repair_cost_scene_parm + Config.current.shop_repair_cost_base));
        }

        public bool TryFix(Device device)
        {
            var coin_id = Config.current.coin_id;
            if (GetFixCost() <= Safe_Area_Helper.GetLootCount((int)coin_id)) 
            {
                Safe_Area_Helper.SpendLootCount((int)coin_id, GetFixCost());
                device.Fix(999999);            //此处偷懒,再读一次表太麻烦了 没必要
                fix_cnt++;
                Reinit();
                return true;
            }
            return false;
        }

        public void Open()
        {
            coin_text.text = Safe_Area_Helper.GetLootCount((int)Config.current.coin_id).ToString();
        }
    }
}
