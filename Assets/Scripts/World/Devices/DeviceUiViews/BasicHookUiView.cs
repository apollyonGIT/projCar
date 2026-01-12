using UnityEngine.UI;
using World.Devices.Device_AI;
using World.Devices.DeviceUiViews.Attachments;

namespace World.Devices.DeviceUiViews
{
    public class BasicHookUiView : DeviceUiView
    {
        private const float GEAR_ROTATION_COEF = 0.1F;


        public UiTurnTable htt;
        public Slider spring_tightness_slider;
        public Image IMG_Gear;

        new private NewBasicHook owner;

        public override void attach(Device owner)
        {
            base.attach(owner);
            this.owner = base.owner as NewBasicHook;
            htt.init(base.owner);
        }

        public override void init()
        {
            base.init();
        }


        public override void notify_on_tick()
        {
            base.notify_on_tick();
            htt.tick();
            spring_tightness_slider.value = owner.spring_tightness_01;
            IMG_Gear.transform.eulerAngles = new UnityEngine.Vector3(0, 0, owner.rotate_angle * GEAR_ROTATION_COEF);
        }
    }
}
