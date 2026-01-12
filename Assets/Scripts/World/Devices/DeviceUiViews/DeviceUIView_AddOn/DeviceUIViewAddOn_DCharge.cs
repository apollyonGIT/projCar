using System.Collections.Generic;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_DCharge : DeviceUIViewAddOn
    {
        protected new DScript_Melee_Charge owner;

        private DeviceAttackCubicle cubicle_attack;
        protected DeviceChargeCubicle cubicle_charge;

        protected List<DevicePanelAttachment_Highlightable> autoWorkHighlight_attack = new();
        protected List<DevicePanelAttachment_Highlightable> autoWorkHighlight_charge = new();

        protected override void attach_cubicle(BasicCubicle cubicle)
        {
            if(cubicle is DeviceAttackCubicle)
                cubicle_attack = cubicle as DeviceAttackCubicle;
            else if (cubicle is DeviceChargeCubicle)
                cubicle_charge = cubicle as DeviceChargeCubicle;
        }

        protected override void attach_highlightable()
        {
            
        }

        protected override void attach_owner(Device owner)
        {
            this.owner = owner as DScript_Melee_Charge;
        }

        protected override void update_cubicle_on_tick()
        {
            DeviceUIView_Common_Action.Set_Highlight_By_If_Cubicle_Has_Worker(cubicle_attack, autoWorkHighlight_attack);
            DeviceUIView_Common_Action.Set_Highlight_By_If_Cubicle_Has_Worker(cubicle_charge, autoWorkHighlight_charge);
        }
    }
}
