using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class ShieldGeneratorUiView : DeviceUiView
    {
        public Slider ShieldEnergyShlider;


        private new Shield_Generator owner;

        public override void attach(Device owner)
        {
            base.attach(owner);
            this.owner = base.owner as Shield_Generator;
        }


        public override void notify_on_tick()
        {
            base.notify_on_tick();
            ShieldEnergyShlider.value = owner.ShieldEnergy_Current;
            ShieldEnergyShlider.maxValue = owner.ShieldEnergy_Max;
        }

        public void Raise_Shield()
        {
            owner.Raise_Shield();
        }

    }
}
