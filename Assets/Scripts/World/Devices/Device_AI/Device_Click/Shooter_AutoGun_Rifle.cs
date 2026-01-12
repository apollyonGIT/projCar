


namespace World.Devices.Device_AI
{
    public class Shooter_AutoGun_Rifle : Shooter_AutoGun
    {

        public Device_Attachment_Tracked_Silo Clip = new();

        // ===============================================================================

        protected override void tick_after_fsm_while_unbroken()
        {
            base.tick_after_fsm_while_unbroken();
            if (Clip.Silo_Fully_Open)
            {
                if (reload_job_process < 1)
                {
                    reload_job_process += fire_logic_data.job_speed[0] * (1 + BattleContext.instance.ranged_reloading_speedup / 1000f);
                    return;
                }
                if (DeviceBehavior_Shooter_Try_Reload(false))
                    reload_job_process = 0;
            }

            if (!Clip.Silo_Fully_Installed)
                auto_shoot_rapid_fire_times = 0;
        }

        // ===============================================================================

        protected override bool can_successfully_load_bullet_into_barrel()
        {
            return base.can_successfully_load_bullet_into_barrel() && Clip.Silo_Fully_Installed;
        }

        // ===============================================================================

        public override void TryToAutoReload()
        {
            if (reload_job_process < 1)
            {
                if (!Clip.Silo_Fully_Open)
                    reload_job_process += fire_logic_data.job_speed[0] * (1 + BattleContext.instance.ranged_reloading_speedup / 1000f);
                return;
            }

            if (Auto_Reload_Job_Content())
                reload_job_process = 0;
        }

        protected override bool Auto_Reload_Job_Content()
        {
            if (current_ammo == 0 && !Clip.Silo_Fully_Open)
            {
                Clip.Track_Value = 1;
                _reloading_in_progress = true;
                return true; // 工作：确保打开弹匣
            }

            if (current_ammo < fire_logic_data.capacity)
            {
                if (Clip.Silo_Fully_Open)
                {
                    _reloading_in_progress = true;
                    return false; // 工作：装填一发子弹。在tick中完成。
                }

                if (_reloading_in_progress)
                {
                    Clip.Track_Value = 1;
                    return true;
                }
            }

            if (current_ammo == fire_logic_data.capacity)
            {
                if (!Clip.Silo_Fully_Installed)
                    Clip.Track_Value = 0f;// 工作：尝试关闭弹匣
                else
                    _reloading_in_progress = false;// 工作：退出装填状态
                return true;
            }

            return false; // 没有装填
        }

        // -------------------------------------------------------------------------------

        protected override bool Auto_Attack_Job_Content()
        {
            switch (barrel_bullet_stage)
            {
                case Barrel_Ammo_Stage.empty:
                    if (Clip.Silo_Fully_Installed && current_ammo > 0)
                        return pull_gun_bolt();
                    return false;
                case Barrel_Ammo_Stage.loaded_into_barrel:
                    if (table_data_auto_load && auto_shoot_rapid_fire_times <= 0)
                    {
                        auto_shoot_rapid_fire_times = UnityEngine.Random.Range(2, 5);
                        auto_shoot_rapid_fire_first_shot = true;
                        return true;
                    }
                    return base.Auto_Attack_Job_Content();
                case Barrel_Ammo_Stage.shell_remaining:
                    return pull_gun_bolt();
                default:
                    return base.Auto_Attack_Job_Content();
            }

            bool pull_gun_bolt()
            {
                Triggerable_Spring.Spring_Value_01 = 1;
                Set_Stiffness_Tick(8);
                return true;
            }
        }

    }
}