using World.Devices.Device_AI;
using World.Devices.DeviceUiViews.Attachments;

namespace World.Devices.DeviceUiViews
{
    public class JetWheelUiView : BasicWheelUiView
    {
        public EnergySlider es;
        public UiTurnTable turn_table;

        public override void attach(Device owner)
        {
            base.attach(owner);
            turn_table.init(base.owner);
            es.ie = owner as IEnergy;
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();
            turn_table.tick();
            es.UpdateSlider();
        }

        public void Accelerate()
        {
            if (owner is JetWheel jet_wheel)
            {
                jet_wheel.Accelerate();
            }
        }
    }
}
