using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using System.Collections.Generic;
using UnityEngine;
using World.Devices;
using World.Devices.Device_AI;
using World.Devices.DeviceViews;
using World.Widgets;

namespace World.Helpers
{
    public class Device_Slot_Helper
    {
        public static Device InstallDevice(string device_id,string slot)
        {
            WorldContext ctx = WorldContext.instance;

            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            device_alls.TryGetValue(device_id, out var rc1);
            var device = dmgr.GetDevice(rc1.behavior_script);

            device.InitData(rc1);
            InstallDevice(device,slot);

            return device;
        }

        public static Device RemoveDevice(string slot_name)
        {
            Device remove_device = null;
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            var slot = dmgr.slots_device.Find(sd => sd._name == slot_name);
            remove_device = slot.slot_device;

            if (remove_device != null)
            {
                dmgr.slots_device.Find(sd => sd._name == slot_name).slot_device = null;
                remove_device.remove_all_views();
            }
            return remove_device;
        }
        
        public static Device InstallDevice(Device device,string slot)
        {
            if(slot == "device_slot_wheel")
            {
                //Widget_Blower_Context.instance.drag_lever_speed = (device as BasicWheel).wheel_desc.blower_charge;
                Widget_Blower_Context.instance.wheel_desc = (device as BasicWheel).wheel_desc;
            }

            WorldContext ctx = WorldContext.instance;

            var remove_device = RemoveDevice(slot);
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);

            Addressable_Utility.try_load_asset<DeviceView>(device.desc.prefeb, out var d1view);
            var device_view = GameObject.Instantiate(d1view, dmgr.pd.transform, false);
            device.add_view(device_view);

            device.key_points.Clear();
            foreach (var kp in device_view.dkp)
            {
                device.key_points.Add(kp.key_name, kp.transform);
            }

            WorldContext.instance.caravan_slots.TryGetValue(slot, out var slot_t);
            if (slot_t != null)
            {
                var v = new Vector2(slot_t.x, slot_t.y);
                var angle = Vector2.SignedAngle(Vector2.right, WorldContext.instance.caravan_dir);
                var new_v = Quaternion.AngleAxis(angle, Vector3.forward) * v;

                var cp = WorldContext.instance.caravan_pos;

                device.position = WorldContext.instance.caravan_pos + new Vector2(new_v.x, new_v.y);
            }
            device.InitPos();

            //规则：设备安装时，添加其重量
            ctx.total_weight += device.desc.weight;

            device.Start();
            dmgr.InstallDevice(slot, device);

            return remove_device;
        }

        public static Device GetDevice(string slot_name)
        {
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            return dmgr.slots_device.Find(sd=>sd._name==slot_name).slot_device;
        }

        public static string GetSlot(Device device)
        {
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            foreach (var slot in dmgr.slots_device)
            {
                if (slot.slot_device == device)
                {
                    return slot._name;
                }
            }
            return null;
        }


        public static List<Device> GetAllDevice()
        {
            List<Device> devices = new List<Device>();
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            foreach (var slot in dmgr.slots_device)
            {
                var device = slot.slot_device;
                if (device != null)
                {
                    devices.Add(device);
                }
            }
            return devices;
        }
    }
}
