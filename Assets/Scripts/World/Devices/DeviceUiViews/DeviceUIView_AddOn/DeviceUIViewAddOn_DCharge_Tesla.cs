using System.Collections.Generic;
using UnityEngine;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_DCharge_Tesla : DeviceUIViewAddOn_DCharge
    {
        public DevicePanelAttachment_Animator tesla_anim;
        public DevicePanelAttachment_Trigger_Press attack_trigger;

        public List<DevicePanelAttachment_Trigger_Press> charge_trigger_list = new();

        public override void attach(Device owner)
        {
            base.attach(owner);
            attack_trigger.Init(new() { try_attacking });

            for(int i = 0; i < charge_trigger_list.Count; i++)
            {
                charge_trigger_list[i].Init(new() {try_charging});
            }
        }

        private void try_charging()
        {
            owner.Try_Charging();
        }

        private void try_attacking()
        {
            owner.Try_Attacking();
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

            for(int i = 0; i < charge_trigger_list.Count; i++)
            {
                var ret = owner.charge_process <(i + 1) * (1f / owner.charge_level)&& owner.charge_process>=i* (1f / owner.charge_level);
                charge_trigger_list[i].Highlighted = ret;
                charge_trigger_list[i].Interactable = ret;
            }



            tesla_anim.SetFloat("Float_Charge",owner.charge_process);
            tesla_anim.SetBool("Bool_IsIdle", owner.IsIdle());
            tesla_anim.SetBool("Bool_HasNPC", cubicle_charge.worker != null);

            tesla_anim.tick();
        }
    }
}
