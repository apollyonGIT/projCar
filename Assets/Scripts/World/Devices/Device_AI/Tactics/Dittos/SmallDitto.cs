using Commons;
using Foundations;
using System.Linq;
using UnityEngine;

namespace World.Devices
{
    public class SmallDitto : Ditto
    {

        //==================================================================================================

        public override string calc_ditto_device_id()
        {
            //规则：获取已安装的其他设备id
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr mgr);

            var slot_device_id_list = mgr.slots_device
                .Where(t => t.slot_device != null)
                .Where(t => t.slot_device.desc.device_type.value == Foundations.Excels.Device_Type.EN_Device_Type.Common)
                .Where(t => !ban_id_list.Contains(t.slot_device.desc.id))
                .Select(t => t.slot_device.desc.id.ToString())
                .ToList();

            var index = Random.Range(0, slot_device_id_list.Count);
            return slot_device_id_list[index];
        }

    }
}

