using UnityEngine;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews
{
    public class Recycler_UiView : DeviceUiView
    {
        public Slider energy_slider;
        public Image slider_fill_pic;

        new Recycler owner;

        //==================================================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);
            this.owner = owner as Recycler;
        }


        public override void notify_on_tick()
        {
            base.notify_on_tick();

            energy_slider.value = owner.energy;
            energy_slider.maxValue = owner.max_energy;

            if (owner.energy < Recycler.C_create_summon_cost)
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

