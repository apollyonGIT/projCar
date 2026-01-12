using AutoCodes;
using Commons;
using Foundations;

namespace World.Devices.Equip
{
    public class EquipPD : Producer
    {
        public override IMgr imgr => mgr;
        EquipmentMgr mgr;
        public EquipmentMgrView eview;

        public override void call()
        {
            
        }

        public void use_current_data(int priority)
        {
            mgr = new("EquipMgr", priority);

            var save = Saves.Save_DS.instance;
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            foreach (var device_data in save.equip_device_datas)
            {
                var device_id = device_data.device_id;
                var device_hp = device_data.device_current_hp;
                //设备
                device_alls.TryGetValue($"{device_id},0", out var device_record);
                var device = dmgr.GetDevice(device_record.behavior_script);
                device.InitData(device_record);
                device.current_hp = int.Parse(device_hp);

                mgr.devices.Add(device);
            }
        }

        public void use_default_data(int priority)
        {
            mgr = new("EquipMgr", priority);
        }

        public override void init(int priority)
        {
            if(Saves.Save_DS.instance.need_load_device)
                use_current_data(priority);
            else
                use_default_data(priority);


            mgr.add_view(eview);
            mgr.Init(new string[0]);
        }
    }
}
