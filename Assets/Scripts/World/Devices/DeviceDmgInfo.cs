using Commons;
using TMPro;
using UnityEngine;

namespace World.Devices
{
    public class DeviceDmgInfo : MonoBehaviour
    {
        public Device device;

        public TextMeshProUGUI dmg_name;
        public TextMeshProUGUI dmg_data;
        public TextMeshProUGUI dmg_ave_data;
        public TextMeshProUGUI dmg_time;

        public void Init(Device device)
        {
            this.device = device;
            dmg_name.text = $"{Localization_Utility.get_localization(device.desc.name)}:";
        }

        public void SetData(float data, float ave_dmg,float _time)
        {
            dmg_data.text = $"{data:F2}";
            dmg_ave_data.text = $"{ave_dmg:F2}";
            dmg_time.text = $"{_time:F2}s";
        }
    }
}
