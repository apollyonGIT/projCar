using AutoCodes;
using System;


namespace World.Devices.Device_AI
{
    public abstract class Shooter_AutoGun : DScript_Shooter
    {

        public Device_Attachment_Triggerable_Spring Triggerable_Spring = new();

        public Action ui_panel_on_spring_triggered;
        public Action ui_panel_on_spring_moved;

        // -------------------------------------------------------------------------------

        protected bool table_data_auto_load;
        protected int auto_shoot_rapid_fire_times;
        protected bool auto_shoot_rapid_fire_first_shot;

        protected bool _reloading_in_progress; // 工作：装填状态

        // -------------------------------------------------------------------------------

        private bool _gun_hammer_locked; // 击锤是否已就绪
        private int _stiffness_tick; // 因开火后坐力等原因而导致的延迟

        // ===============================================================================

        public override void InitData(device_all rc)
        {
            base.InitData(rc);

            fire_logics.TryGetValue(rc.fire_logic.ToString(), out var record);
            table_data_auto_load = record.auto_load;

            Triggerable_Spring.on_spring_triggered = action_on_spring_triggered;
            Triggerable_Spring.on_spring_moved = action_on_spring_moved;
        }

        // ===============================================================================

        protected override void tick_after_fsm_while_unbroken()
        {
            base.tick_after_fsm_while_unbroken();
            if (_stiffness_tick > 0)
            {
                _stiffness_tick--;
                if (_stiffness_tick == 0)
                    Triggerable_Spring.Spring_Value_01 = 0;
            }
        }

        // ===============================================================================

        protected override bool DeviceBehavior_Shooter_Try_Shoot(bool ui_manual_shoot, bool ignore_post_cast = false, Action shoot_finished = null)
        {
            if (Gun_Hammer_Locked && base.DeviceBehavior_Shooter_Try_Shoot(ui_manual_shoot, ignore_post_cast, Action_On_Shoot))
            {
                Gun_Hammer_Locked = false;
                return true;
            }
            return false;
        }

        // ===============================================================================

        public override void TryToAutoAttack()
        {
            if (auto_shoot_rapid_fire_times > 0)
                DeviceBehavior_Shooter_Try_Shoot(!auto_shoot_rapid_fire_first_shot);
            else
                base.TryToAutoAttack();
        }

        // ===============================================================================

        private void action_on_spring_triggered()
        {
            ui_panel_on_spring_triggered?.Invoke();  // 在 Load_Bullet_Into_Barrel 之前，先抛出空弹壳
            Gun_Hammer_Locked = true;
            DeviceBehavior_Shooter_Try_Load_Bullet_Into_Barrel();
        }

        private void action_on_spring_moved()
        {
            ui_panel_on_spring_moved?.Invoke();
        }

        // ===============================================================================
        // For UI Info

        /// <summary>
        /// 用于判断当前是否正在装填中。
        /// 当NPC开启装填流程后，或玩家手动操作弹匣之后，设置为true。
        /// </summary>
        public bool Reloading_In_Progress
        {
            get { return _reloading_in_progress; }
            set { _reloading_in_progress = value; }
        }

        public bool Gun_Hammer_Locked
        {
            get { return _gun_hammer_locked; }
            set { _gun_hammer_locked = value; }
        }

        public void Set_Stiffness_Tick(int value)
        {
            _stiffness_tick = value;
        }

        public void Action_On_Shoot()
        {
            if (table_data_auto_load)
            {
                Triggerable_Spring.Spring_Value_01 = 1;
                Set_Stiffness_Tick(3);
            }

            if (auto_shoot_rapid_fire_times > 0)
            {
                auto_shoot_rapid_fire_times--;
                auto_shoot_rapid_fire_first_shot = false;
            }
        }

    }
}