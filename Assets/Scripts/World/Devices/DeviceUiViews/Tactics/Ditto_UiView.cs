using UnityEngine;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews
{
    public class Ditto_UiView : DeviceUiView
    {
        public Slider energy_slider;

        new Ditto owner;

        //==================================================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);
            this.owner = owner as Ditto;
        }


        public override void notify_on_tick()
        {
            base.notify_on_tick();

            energy_slider.value = owner.energy;
            energy_slider.maxValue = owner.max_energy;
        }


        public void btn_cast()
        {
            owner.try_to_cast();
        }
    }
}

