using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_DMelee_Base : DeviceUIViewAddOn_DMelee
    {
        public DevicePanelAttachment_Trigger trigger_for_attacking;

        public override void attach(Device owner)
        {
            base.attach(owner);
            trigger_for_attacking.Init(new() { try_attacking });
        }

        private void try_attacking()
        {
            owner.Try_Attacking();
        }

        protected override void attach_highlightable()
        {
            base.attach_highlightable();

            autoWorkHighlight_melee.Add(trigger_for_attacking);
        }


        public override void notify_on_tick()
        {
            base.notify_on_tick();

            trigger_for_attacking.Interactable = owner.CanAttack();
            trigger_for_attacking.Highlighted = owner.CanAttack();
        }
    }
}
