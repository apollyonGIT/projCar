using UnityEngine.UI;
using World.Devices.Device_AI;
using World.Devices.DeviceUiViews.Attachments;

namespace World.Devices.DeviceUiViews
{
    public class EnergyShooterUiView : DeviceUiView
    {
        public EnergySlider energy_slider;

        public override void attach(Device owner)
        {
            base.attach(owner);
            energy_slider.ie = owner as IEnergy;
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            energy_slider.UpdateSlider();
        }
    }
}
