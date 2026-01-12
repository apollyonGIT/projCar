using UnityEngine;
using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews.Attachments
{
    public class EnergySlider :MonoBehaviour
    {
        public Slider energy_slider;
        public IEnergy ie;


        public void UpdateSlider()
        {
            if (ie == null || energy_slider == null)
            {
                return;
            }
            energy_slider.value = ie.Current_Energy / ie.Max_Energy;
        }
    }
}
