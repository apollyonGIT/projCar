using Commons;
using Foundations;
using System.Collections.Generic;
using System.Linq;
using World.Devices.Equip;

namespace World.Saves
{
    public class device_data
    {
        public string device_id;
        public string device_current_hp;
        
        public List<(int,string)> cub_worker_data = new();
    }

    public class Save_DS : Singleton<Save_DS>
    {
        public bool need_load_device = false;

        public List<object> caravan_datas = new();
        public Dictionary<string, device_data> device_datas = new();
        public Dictionary<string, List<string>> device_upgrade_datas = new();
        public List<object> backpack_datas = new();
        public int coin_datas;
        public List<uint> relic_datas = new();
        public List<List<object>> character_datas = new();
        public List<device_data> equip_device_datas = new();

        //==================================================================================================

        public void save()
        {
            clear();

            var ctx = WorldContext.instance;

            //车体
            if (Mission.instance.try_get_mgr("CaravanMgr", out Caravans.CaravanMgr caravan_mgr))
            {
                caravan_datas.Add(caravan_mgr.cell._desc.id);
                caravan_datas.Add(ctx.caravan_hp);
                caravan_datas.Add(ctx.caravan_hp_max);
            }

            //设备 & 升级
            if (Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out Devices.DeviceMgr device_mgr))
            {
                foreach (var slot in device_mgr.slots_device.Where(t => t.slot_device != null))
                {
                    var device = slot.slot_device;
                    var slot_name = slot._name;

                    var new_device_data = new device_data() 
                    { 
                        device_id = $"{device.desc.id}",
                        device_current_hp = $"{device.current_hp}",
                    };

                    for (int i = 0; i < device.cubicle_list.Count; i++)
                    {
                        var cub = device.cubicle_list[i];
                        if(cub.worker != null)
                        {
                            new_device_data.cub_worker_data.Add((i, $"{cub.worker.desc.id}"));
                        }
                    }

                    device_datas.Add(slot_name, new_device_data);

                    //装载升级数据
                    var upgrades = device.upgrades;
                    List<string> upgrades_key_list = new();
                    foreach (var upgrade in upgrades.Where(t => t.Applied()))
                    {
                        upgrades_key_list.Add($"{upgrade.desc.id},{upgrade.desc.sub_id}");
                    }

                    device_upgrade_datas.Add(slot_name, upgrades_key_list);
                }
            }

            //背包
            if (Mission.instance.try_get_mgr("BackPack", out BackPack.BackPackMgr backpack_mgr))
            {
                backpack_datas.Add(backpack_mgr.slot_count);
                backpack_datas.Add(backpack_mgr.get_all_loots());

                coin_datas = backpack_mgr.coin_count;
            }

            //遗物
            if (Mission.instance.try_get_mgr(Config.RelicMgr_Name, out Relic.RelicMgr relic_mgr))
            {
                foreach (var relic in relic_mgr.relic_list)
                {
                    relic_datas.Add(relic.desc.id);
                }
            }

            //角色
            if (Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out Characters.CharacterMgr character_mgr))
            {
                foreach (var character in character_mgr.characters)
                {
                    List<object> data = new();
                    data.Add(character.desc.id);

                    List<uint> character_property_keys = new();
                    foreach (var character_property in character.character_properties)
                    {
                        character_property_keys.Add(character_property.desc.id);
                    }
                    data.Add(character_property_keys); //词条

                    character_datas.Add(data);
                }
            }

            //设备背包
            if(Mission.instance.try_get_mgr("EquipMgr",out EquipmentMgr equip_mgr))
            {
                foreach(var equip in equip_mgr.devices)
                {
                    var new_device_data = new device_data()
                    {
                        device_id = $"{equip.desc.id}",
                        device_current_hp = $"{equip.current_hp}",
                    };
                    equip_device_datas.Add(new_device_data);
                }
            }

            need_load_device = true;
        }


        public void clear()
        {
            need_load_device = false;

            coin_datas = 0;
            device_datas.Clear();
            device_upgrade_datas.Clear();
            caravan_datas.Clear();
            backpack_datas.Clear();
            relic_datas.Clear();
            character_datas.Clear();
            equip_device_datas.Clear();
        }
    }
}

