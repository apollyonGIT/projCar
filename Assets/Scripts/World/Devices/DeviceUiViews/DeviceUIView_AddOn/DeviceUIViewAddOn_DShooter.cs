using TMPro;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews.DeviceUIView_AddOn
{
    public class DeviceUIViewAddOn_DShooter : DeviceUIViewAddOn_Shooter
    {
        #region Const
        private const string SE_GUN_NO_AMMO = "se_ag_gun_no_ammo";

        private const bool IGNORE_POST_CAST = true; // 是否忽略后摇
        #endregion

        public DevicePanelAttachment_Trigger_Press trigger_for_shooting;
        public DevicePanelAttachment_Trigger_Hold hold_for_reloading;

        public TextMeshProUGUI ammo_text;

        protected new DScript_Shooter owner;
        public override void attach(Device owner)
        {
            base.attach(owner);

            trigger_for_shooting.Init(new() { shoot_without_parms });
            hold_for_reloading.Init(new() { });
        }

        protected override void attach_owner(Device owner)
        {
            base.attach_owner(owner);
            this.owner = owner as DScript_Shooter;
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            ammo_text.text = $"{owner.current_ammo}/{owner.fire_logic_data.capacity}";

            if(hold_for_reloading.Is_Holding(false))
            {
                start_reloading();
            }

            if(owner.fsm == DScript_Shooter.Device_FSM_Shooter.reloading)
            {
                
            }
        }


        protected override void attach_highlightable()
        {
            autoWorkHighlight_shoot.Add(trigger_for_shooting);
        }

        private void shoot_without_parms()
        {
            if (owner.current_ammo == 0)
            {
                Audio.AudioSystem.instance.PlayOneShot(SE_GUN_NO_AMMO);
                return; // 如果弹仓未关闭，则不射击
            }

            shoot(IGNORE_POST_CAST);
        }
    }
}
