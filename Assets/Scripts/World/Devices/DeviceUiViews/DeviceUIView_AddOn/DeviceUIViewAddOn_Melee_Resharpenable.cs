using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Melee_Resharpenable : DeviceUIViewAddOn_Melee
    {
        public Slider sharpeness_indicator;
        public Image img_sharpness_stage;
        public List<Sprite> img_sharpness_stage_sprites;
        public List<TextMeshProUGUI> text_sharpness_mod_dmg_coef;

        public GameObject autoWorkIndicator_sharpen;

        public DevicePanelAttachment_Grinder Grinder;

        public DevicePanelAttachment_Trigger_Press_Anim Widget_Stone;

        // -------------------------------------------------------------------------------

        private DeviceSharpCubicle cubicle_sharpen;
        private int sharpness_stage;

        // ===============================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);

            Grinder.Init(new() { grinding, end_grinding });
            if(Widget_Trigger_Attack != null)
                Widget_Trigger_Attack.Init(new() { temp_attack });
            if(Widget_Stone != null && owner is AnimSharpMelee_Click)
                Widget_Stone.Init(new() { () => { (owner as AnimSharpMelee_Click).Sharpening(); } });

            if (text_sharpness_mod_dmg_coef != null)
                for (int i = 0; i < text_sharpness_mod_dmg_coef.Count; i++)
                    text_sharpness_mod_dmg_coef[i].text = this.owner.UI_Info_Get_Dmg_Text_By_Sharpness_Stage(i);
        }
        
        private void temp_attack()
        {
            owner.UI_Controlled_Attack(true);
        }


        protected override void attach_owner(Device owner)
        {
            this.owner = owner as BasicMelee_Click;
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            sharpeness_indicator.maxValue = (int)(100 * (1 + BattleContext.instance.melee_sharpness_add / 1000f));
            sharpeness_indicator.value = owner.Sharpness_Current;
            sharpness_stage = owner.UI_Info_Get_Sharpness_Stage();
            if (sharpness_stage >= 0)
                img_sharpness_stage.sprite = img_sharpness_stage_sprites[sharpness_stage];

            if(Widget_Stone != null && owner is AnimSharpMelee_Click)
            {
                Widget_Stone.SetAnim((owner as AnimSharpMelee_Click).sharpen_percent);
            }
        }

        // ===============================================================================

        protected override void attach_cubicle(BasicCubicle cubicle)
        {
            base.attach_cubicle(cubicle);
            if (cubicle is DeviceSharpCubicle)
                cubicle_sharpen = cubicle as DeviceSharpCubicle;
        }

        protected override void update_cubicle_on_tick()
        {
            base.update_cubicle_on_tick();
            if (cubicle_sharpen != null)
                autoWorkIndicator_sharpen.SetActive(cubicle_sharpen.worker != null);
        }

        // ===============================================================================

        protected void sharpen(float effect)
        {
            owner.UI_Controlled_Sharpen(effect);
        }

        // ===============================================================================

        private void grinding()
        {
            sharpen(Grinder.Get_Accumulated_Grind());
        }

        private void end_grinding()
        {
            sharpen(0);   //数值为0可以停止播放磨刀音效
        }
    }
}
