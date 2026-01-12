using TMPro;
using UnityEngine.UI;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{ 
    public class DeviceUIViewAddOn_DMelee_Claymore : DeviceUIViewAddOn_DMelee
    {
        public DevicePanelAttachment_Trigger_Hold trigger_for_charging;

        public DevicePanelAttachment_DraggableNew dragger_for_sharpe;

        protected new DScript_Melee_Claymore owner;

        public Image fill;

        public Slider sharpness_slider;

        public TextMeshProUGUI coef_text;

        private DeviceSharpCubicle cubicle_sharpe;

        public override void attach(Device owner)
        {
            base.attach(owner);
            trigger_for_charging.Init(new() { null ,attack });
            dragger_for_sharpe.Init(new() {update_sharpness ,reset_sharpness});
        }

        protected override void attach_highlightable()
        {
            base.attach_highlightable();
            autoWorkHighlight_melee.Add(trigger_for_charging);
            autoWorkHighlight_sharp.Add(dragger_for_sharpe);
        }


        protected override void attach_owner(Device owner)
        {
            base.attach_owner(owner);
            this.owner = owner as DScript_Melee_Claymore;
        }

        protected override void attach_cubicle(BasicCubicle cubicle)
        {
            base.attach_cubicle(cubicle);
            if(cubicle is DeviceSharpCubicle)
                cubicle_sharpe = cubicle as DeviceSharpCubicle;
        }

        protected override void update_cubicle_on_tick()
        {
            base.update_cubicle_on_tick();
            DeviceUIView_Common_Action.Set_Highlight_By_If_Cubicle_Has_Worker(cubicle_sharpe, autoWorkHighlight_sharp);
        }

        private void update_sharpness()
        {
            var abs = dragger_for_sharpe.GetMoveAbsRate();

            owner.Try_Sharping(abs);
            if (abs >=1)
            {
                owner.Try_Sharping(-1);
                dragger_for_sharpe.Forced_Drop_Drag();
            }
        }

        private void reset_sharpness()
        {
            owner.Try_Sharping(-1);
        }


        private void charging()
        {
            owner.Try_Charging();
        }

        private void attack()
        {
            owner.Try_Attacking();
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            if(trigger_for_charging.Is_Holding(true))
            {
                charging();
            }

            dragger_for_sharpe.Forced_Set_Relative_Drag_Distance_01(owner.sharp_process);
            fill.fillAmount = (float)owner.charge_process / 1f;
            coef_text.text = $"{owner.Get_Sharpness_Dmg_Coef() * 100}%";
            sharpness_slider.value = owner.Get_Sharpness();

            trigger_for_charging.Highlighted = owner.CanAttack();
            dragger_for_sharpe.Highlighted = owner.Get_Sharpness() != 1;
        }
    }
}
