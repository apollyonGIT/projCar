using System.Collections.Generic;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_DMelee : DeviceUIViewAddOn
    {
        protected List<DevicePanelAttachment_Highlightable> autoWorkHighlight_melee = new();
        protected List<DevicePanelAttachment_Highlightable> autoWorkHighlight_sharp = new();
        protected List<DevicePanelAttachment_Highlightable> autoWorkHighlight_repair = new();

        protected new DScript_Melee owner;

        private DeviceAttackCubicle cubicle_attack;

        protected override void attach_owner(Device owner)
        {
            this.owner = owner as DScript_Melee;
        }

        protected override void attach_cubicle(BasicCubicle cubicle)
        {
            if (cubicle is DeviceAttackCubicle)
                cubicle_attack = cubicle as DeviceAttackCubicle;
        }

        protected override void update_cubicle_on_tick()
        {
            DeviceUIView_Common_Action.Set_Highlight_By_If_Cubicle_Has_Worker(cubicle_attack, autoWorkHighlight_melee);
        }

        protected override void attach_highlightable()
        {
            
        }
    }
}
