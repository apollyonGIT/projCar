using UnityEngine.UI;
using UnityEngine;
using World.Devices.Device_AI;
using World.Devices.DeviceUiViews.Attachments;

namespace World.Devices.DeviceUiViews
{
    public class BasicRamUiView : DeviceUiView
    {
        public UiTurnTable htt;

        public Image Gear_indicator;

        new private BasicRam owner;

        public override void init()
        {
            base.init();
            
        }

        public override void attach(Device owner)
        {
            base.attach(owner);
            this.owner = base.owner as BasicRam;
            htt.init(base.owner);
        }


        public override void notify_on_tick()
        {
            base.notify_on_tick();
            htt.tick();

            var a = Vector2.SignedAngle(Vector2.right, owner.ram_dir);
            Gear_indicator.transform.eulerAngles = new Vector3(0, 0, a);
        }
    }
}
