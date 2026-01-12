using TMPro;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Shotgun_2 : DeviceUIViewAddOn_Shooter
    {
        public DevicePanelAttachment_Trigger_Press trigger_for_shooting;

        public TextMeshProUGUI ammo_text;

        new protected DScript_Shooter_Shotgun owner;

        public override void attach(Device owner)
        {
            base.attach(owner);
            trigger_for_shooting.Init(new() { do_shoot });
            this.owner = (DScript_Shooter_Shotgun)owner;
        }

        protected override void attach_owner(Device device_owner)
        {
            base.attach_owner(device_owner);
            owner = device_owner as DScript_Shooter_Shotgun;
        }
        protected override void attach_highlightable()
        {
            autoWorkHighlight_shoot.Add(trigger_for_shooting);
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            ammo_text.text = $"{owner.current_ammo}/{owner.fire_logic_data.capacity}";
            trigger_for_shooting.Highlighted = owner.CanShoot();
            trigger_for_shooting.Interactable = owner.CanShoot();
        }

        protected void do_shoot()
        {
            shoot(true);
        }
    }
}
