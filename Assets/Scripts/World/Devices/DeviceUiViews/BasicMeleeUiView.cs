using UnityEngine;
using UnityEngine.UI;
using World.Devices.Device_AI;
using World.Devices.DeviceUiViews.Attachments;

namespace World.Devices.DeviceUiViews
{
    public class BasicMeleeUiView : DeviceUiView
    {

        public Button sharping, cancel_sharping;
        public Slider sharpen_slider;
        public GameObject sharp_object;
        public Grinder grinder;

        new private NewBasicMelee owner;

        public override void attach(Device owner)
        {
            base.attach(owner);
            this.owner = base.owner as NewBasicMelee;
            grinder.Init(this.owner);
        }

        public override void init()
        {
            base.init();       
        }


        public override void notify_on_tick()
        {
            base.notify_on_tick();

            sharpen_slider.value = owner.Sharpness_Current;
            sharpen_slider.maxValue = (int)(100 * (1 + BattleContext.instance.melee_sharpness_add/1000f));
        }

        public void SetSharpObj()
        {
            bool ret = !owner.Sharping;
            sharping.gameObject.SetActive(ret);
            cancel_sharping.gameObject.SetActive(!ret);

            sharp_object.SetActive(ret);
            if (ret)
                owner.StartSharp();
            else
                owner.EndSharp();
        }
    }
}
