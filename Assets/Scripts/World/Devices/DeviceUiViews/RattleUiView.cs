using UnityEngine;
using World.Devices.Device_AI;
using World.Devices.DeviceUiViews.Attachments;

namespace World.Devices.DeviceUiViews
{
    public class RattleUiView : DeviceUiView
    {
        public RattleGrinder rattle_grinder;
        public EnergySlider es;

        public override void attach(Device owner)
        {
            base.attach(owner);
            rattle_grinder.Init(owner as Rattle);
            es.ie = owner as IEnergy;
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();
            es.UpdateSlider();
        }
    }
}
