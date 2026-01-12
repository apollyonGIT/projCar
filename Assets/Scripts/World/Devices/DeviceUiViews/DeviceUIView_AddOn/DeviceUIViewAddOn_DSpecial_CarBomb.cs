using System.Collections.Generic;
using UnityEngine;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_DSpecial_CarBomb : DeviceUIViewAddOn
    {
        protected new DScript_Special_CarBomb owner;
        private DeviceAttackCubicle cubicle_attack;
        
        protected List<DevicePanelAttachment_Highlightable> autoWorkHighlight_attack = new();

        public DevicePanelAttachment_Animator carbomb_anim;
        public DevicePanelAttachment_Trigger_Press trigger_for_attack;

        public override void attach(Device owner)
        {
            base.attach(owner);

            carbomb_anim.Init();
            trigger_for_attack.Init(new(){()=> { this.owner.Try_Attacking(); } });
            
            carbomb_anim.SetFloat("Float_AmmoMax",(owner as DScript_Special_CarBomb).usage/6f);
        }
        protected override void attach_cubicle(BasicCubicle cubicle)
        {
            if(cubicle is DeviceAttackCubicle)
                cubicle_attack = cubicle as DeviceAttackCubicle;
        }

        protected override void attach_highlightable()
        {
            autoWorkHighlight_attack.Add(trigger_for_attack);
        }

        protected override void attach_owner(Device owner)
        {
            this.owner = owner as DScript_Special_CarBomb;
        }

        protected override void update_cubicle_on_tick()
        {
            DeviceUIView_Common_Action.Set_Highlight_By_If_Cubicle_Has_Worker(cubicle_attack, autoWorkHighlight_attack);
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            carbomb_anim.SetFloat("Float_Ammo", owner.remain_use_count/6f);
            carbomb_anim.SetFloat("Float_CD", owner.t);

            carbomb_anim.tick();
            trigger_for_attack.Highlighted = owner.CanAttack();
            trigger_for_attack.Interactable = owner.CanAttack();
        }
    }
}
