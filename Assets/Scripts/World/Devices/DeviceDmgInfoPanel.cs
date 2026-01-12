
using Commons;
using System.Collections.Generic;
using UnityEngine;

namespace World.Devices
{
    public class DeviceDmgInfoPanel : MonoBehaviour
    {
        public Transform content;
        public DeviceDmgInfo prefab;
        public List<DeviceDmgInfo> deviceDmgInfos = new List<DeviceDmgInfo>();

        public void ResetPanel()
        {
            foreach(var dmi in deviceDmgInfos)
            {
                Destroy(dmi.gameObject);
            }

            deviceDmgInfos.Clear();

            BattleContext.instance.ResetDmg();

            foreach(var dmi in BattleContext.instance.device_dmg_dic)
            {
                DeviceDmgInfo dmi_instance = Instantiate(prefab, content);
                dmi_instance.Init(dmi.Key);

                dmi_instance.gameObject.SetActive(true);
                deviceDmgInfos.Add(dmi_instance);

                dmi_instance.SetData(dmi.Value.GetTotalDmg(),dmi.Value.GetAverDmg(),(float)dmi.Key.equip_time/Config.PHYSICS_TICKS_PER_SECOND);
            }
        }
    }
}
