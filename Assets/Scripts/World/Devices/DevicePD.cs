using Commons;
using Foundations;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using World.Caravans;
using World.Helpers;

namespace World.Devices
{
    public class DevicePD : Producer
    {
        public override IMgr imgr => mgr;
        public Transform ui_content;
        DeviceMgr mgr;

        //public DeviceWorkingBoard dwb;
        public DeviceMgrView dview;

        //==================================================================================================

        public override void init(int priority)
        {
            if (Saves.Save_DS.instance.need_load_device)
                use_current_data(priority);
            else
                use_default_data(priority);
           // mgr.add_view(dwb);
            mgr.add_view(dview);
            mgr.Init();

            SortUi();
        }


        public void use_current_data(int priority)
        {
            var ctx = WorldContext.instance;

            mgr = new(Config.DeviceMgr_Name, priority);
            mgr.pd = this;


            Mission.instance.try_get_mgr(Config.CaravanMgr_Name, out CaravanMgr caravan_mgr);
            foreach(var device_slot in caravan_mgr.cell._desc.device_slots)
            {
                mgr.AddDevice($"{device_slot.Key}", null, device_slot.Value);
            }
            mgr.AddDevice("device_slot_wheel", null);
            /*mgr.AddDevice("slot_top", null);
            mgr.AddDevice("slot_front_top", null);
            mgr.AddDevice("slot_back_top", null);
            mgr.AddDevice("slot_front", null);
            mgr.AddDevice("slot_back", null);*/

            var save = Saves.Save_DS.instance;
            var device_datas = save.device_datas;
            var upgrade_datas = save.device_upgrade_datas;

            foreach (var (slot_pos, device_data) in device_datas)
            {
                var device_id = device_data.device_id;
                var device_hp = device_data.device_current_hp;

                //设备
                var cell = Device_Slot_Helper.InstallDevice($"{device_id},0", slot_pos);

                cell.current_hp = int.Parse(device_hp);

                //升级
                if (upgrade_datas.TryGetValue(slot_pos, out var upgrade_keys))
                {
                    foreach (var upgrade_key in upgrade_keys)
                    {
                        foreach (var cell_upgrade in cell.upgrades.Where(t => $"{t.desc.id},{t.desc.sub_id}" == upgrade_key))
                        {
                            cell_upgrade.ApplyUpgrade(cell);
                        };
                    }
                }


                //工位安排人
                foreach (var (cub_index, worker_id) in device_data.cub_worker_data)
                {
                    var cub = cell.cubicle_list[cub_index];

                    Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out Characters.CharacterMgr cmgr);
                    var c = cmgr.characters.Find(t => t.desc.id.ToString() == worker_id.ToString());
                    cmgr.select_character = c;

                    Cubicle_Helper.SetCubicle(cub);
                }
            }
        }


        public void use_default_data(int priority)
        {
            var ctx = WorldContext.instance;

            mgr = new(Config.DeviceMgr_Name, priority);
            mgr.pd = this;


            Mission.instance.try_get_mgr(Config.CaravanMgr_Name, out CaravanMgr caravan_mgr);
            foreach (var device_slot in caravan_mgr.cell._desc.device_slots)
            {
                mgr.AddDevice($"{device_slot.Key}", null, device_slot.Value);
            }
            mgr.AddDevice("device_slot_wheel", null);

            if (!Config.current.is_load_devices) return;

            // 这里把轮子放在第一个，使为了使其在下面的UI面板中靠前

            Device_Slot_Helper.InstallDevice(Config.current.init_wheel.ToString() + ",0", "device_slot_wheel");
            if (Config.current.init_device_in_slot_top != 0)
            {
                Device_Slot_Helper.InstallDevice(Config.current.init_device_in_slot_top.ToString() +",0", "slot_top");
            }

            if (Config.current.init_device_in_slot_front_top != 0)
            {
                Device_Slot_Helper.InstallDevice(Config.current.init_device_in_slot_front_top.ToString() + ",0", "slot_front_top");
            }

            if (Config.current.init_device_in_slot_back_top != 0)
            {
                Device_Slot_Helper.InstallDevice(Config.current.init_device_in_slot_back_top.ToString() + ",0", "slot_back_top");
            }

            if (Config.current.init_device_in_slot_front != 0)
            {
                Device_Slot_Helper.InstallDevice(Config.current.init_device_in_slot_front.ToString() + ",0", "slot_front");
            }

            if (Config.current.init_device_in_slot_back != 0)
            {
                Device_Slot_Helper.InstallDevice(Config.current.init_device_in_slot_back.ToString() + ",0", "slot_back");
            }
        }


        private void SortUi()
        {
            if (ui_content != null)
            {
                if (ui_content.childCount == 0)
                    return;
                var start_pos = new Vector2(-1280f + ui_content.GetChild(0).GetComponent<RectTransform>().rect.width/2, 0);

                if(ui_content.childCount == 1)
                {
                    var rect = ui_content.GetChild(0).GetComponent<RectTransform>();
                    start_pos = new Vector2(start_pos.x, 0 + rect.rect.height / 2);
                    rect.anchoredPosition = start_pos;
                }

                for(int i =0;i< ui_content.childCount -1; i++)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ui_content.GetChild(i).GetComponent<RectTransform>());
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ui_content.GetChild(i+1).GetComponent<RectTransform>());

                    var rect = ui_content.GetChild(i).GetComponent<RectTransform>();
                    start_pos = new Vector2(start_pos.x, 0 + rect.rect.height / 2);
                    rect.anchoredPosition = start_pos;

                    start_pos = new Vector2(start_pos.x + rect.rect.width / 2 + ui_content.GetChild(i + 1).GetComponent<RectTransform>().rect.width / 2, start_pos.y);
                }

                if(ui_content.childCount >= 1)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ui_content.GetChild(ui_content.childCount-1).GetComponent<RectTransform>());
                    ui_content.GetChild(ui_content.childCount - 1).GetComponent<RectTransform>().anchoredPosition = start_pos;
                }
            }
        }


        public override void call()
        {
        }
    }
}