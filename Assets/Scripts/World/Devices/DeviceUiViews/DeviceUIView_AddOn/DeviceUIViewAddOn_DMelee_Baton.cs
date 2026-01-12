using UnityEngine;
using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_DMelee_Baton : DeviceUIViewAddOn_DMelee
    {
        public DevicePanelAttachment_Trigger trigger_for_attacking;

        protected new DScript_Melee_Baton owner;

        public Image fill;
        public GameObject broken;

        public override void attach(Device owner)
        {
            base.attach(owner);
            trigger_for_attacking.Init(new() { try_attacking});
        }


        protected override void attach_owner(Device owner)
        {
            base.attach_owner(owner);
            this.owner = owner as DScript_Melee_Baton;
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

            fill.fillAmount = (float)owner.current_durability / (float)owner.max_durability;

            broken.SetActive(owner.current_durability <= 0);

            trigger_for_attacking.Interactable = owner.CanAttack();
            trigger_for_attacking.Highlighted = owner.CanAttack();
        }
    }
}
