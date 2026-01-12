using UnityEngine;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews
{
    public class BlackHole_UiView : DeviceUiView
    {
        public Slider energy_slider;
        public Image slider_fill_pic;

        new BlackHole owner;

        //==================================================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);
            this.owner = owner as BlackHole;
        }


        public override void notify_on_tick()
        {
            base.notify_on_tick();

            energy_slider.value = owner.energy;
            energy_slider.maxValue = owner.max_energy;

            if (owner.energy < BlackHole.C_create_summon_cost)
            {
                slider_fill_pic.color = Color.red;
            }
            else
            {
                slider_fill_pic.color = Color.white;
            }


        }


        public void btn_cancel_cast()
        {

            owner.clear_summon();
        }
    }
}

