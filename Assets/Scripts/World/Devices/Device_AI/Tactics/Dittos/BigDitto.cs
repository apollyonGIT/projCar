using Commons;
using Foundations;
using System.Linq;
using UnityEngine;

namespace World.Devices
{
    public class BigDitto : Ditto
    {

        //==================================================================================================

        public override string calc_ditto_device_id()
        {
            //规则：获取所有范围内的其他设备id
            var records = AutoCodes.device_alls.records;
            var slot_device_id_list = records
                .Where(t => t.Value.behavior_script != null)
                .Where(t => t.Value.device_type.value == Foundations.Excels.Device_Type.EN_Device_Type.Common)
                .Where(t => !ban_id_list.Contains(t.Value.id))
                .Select(t => t.Value.id.ToString())
                .ToList();

            var index = Random.Range(0, slot_device_id_list.Count);
            return slot_device_id_list[index];
        }

    }
}

