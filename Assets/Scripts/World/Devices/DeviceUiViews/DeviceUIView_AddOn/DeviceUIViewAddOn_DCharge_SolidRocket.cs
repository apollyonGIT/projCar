using UnityEngine;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_DCharge_SolidRocket:DeviceUIViewAddOn_DCharge
    {
        public DevicePanelAttachment_Trigger_Press attack_trigger;
        public DevicePanelAttachment_Animator charge_animator;

        protected new DScript_Melee_Charge_Rocket owner;


        public override void attach(Device owner)
        {
            base.attach(owner);
            attack_trigger.Init(new() { try_attacking });
        }

        private void try_attacking()
        {
            owner.Try_Attacking();
        }

        protected override void attach_owner(Device owner)
        {
            base.attach_owner(owner);
            this.owner = owner as DScript_Melee_Charge_Rocket;
        }

        protected override void attach_highlightable()
        {
            base.attach_highlightable();
            autoWorkHighlight_attack.Add(attack_trigger);
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            attack_trigger.Highlighted = owner.CanAttack();
            attack_trigger.Interactable = owner.CanAttack();

            charge_animator.SetFloat("Float_Charge", owner.charge_process);
            charge_animator.tick();
        }
    }
}
