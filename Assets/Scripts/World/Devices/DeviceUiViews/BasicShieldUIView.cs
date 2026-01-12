using UnityEngine.UI;
using World.Devices.Device_AI;
using World.Devices.DeviceUiViews.Attachments;

namespace World.Devices.DeviceUiViews
{
    public class BasicShieldUiView : DeviceUiView
    {
        public EnergySlider ShieldEnergyShlider;

        new private BasicShield owner;

        public override void attach(Device owner)
        {
            base.attach(owner);
            this.owner = base.owner as BasicShield;
            ShieldEnergyShlider.ie = this.owner;
        }


        public override void notify_on_tick()
        {
            base.notify_on_tick();
            ShieldEnergyShlider.UpdateSlider();
        }

        public void UI_Controlled_Reset_Shield()
        {
            owner.Def_End_By_UI_Control();
        }
    }
}
