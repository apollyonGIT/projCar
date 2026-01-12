
using System;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class DScript_Shooter_Gatlin : DScript_Shooter,ICarry
    {

        private const float BARREL_ROTATE_SPEED_MAX = 80F;
        private const float CRANK_ROTATE_SPEED_COEF = 0.8F;

        //枪管旋转速度
        public float barrel_rotate_speed;
        //枪管固定减速
        public float barrel_rotate_deceleration = 0.07f * Commons.Config.PHYSICS_TICK_DELTA_TIME;
        //枪管负反馈变速
        public float barrel_rotate_feedback = 0.997f;
        //枪管棘轮转速系数
        public float crank_rotate_factor = 0.8f * Commons.Config.PHYSICS_TICK_DELTA_TIME;
        //枪管累积角度
        public float barrel_rotate_angle;
        //射击累积角度
        private float stored_angle_for_fire;
        //枪管间隔角度
        public float barrel_rotate_interval_angle = 45f;
        //搬运上限
        public int carry_limit = 10;
        //已搬运次数
        public int carry_count;

        //棘轮
        public Device_Attachment_Ratchet ratchetShoot = new(-90f);

        public float carry_job_process;

        protected override void idle_state_tick()
        {
            base.idle_state_tick();

            if (barrel_rotate_speed > 0)
            {
                barrel_rotate_angle += barrel_rotate_speed;
                stored_angle_for_fire += barrel_rotate_speed;
                barrel_rotate_speed -= barrel_rotate_deceleration;
                barrel_rotate_speed *= barrel_rotate_feedback;
            }

            if (stored_angle_for_fire > barrel_rotate_interval_angle)
            {
                DeviceBehavior_Shooter_Try_Shoot(true);
                stored_angle_for_fire -= barrel_rotate_interval_angle;
            }
        }

        protected override void shoot_state_tick()
        {
            base.shoot_state_tick();

            if (barrel_rotate_speed > 0)
            {
                barrel_rotate_angle += barrel_rotate_speed;
                stored_angle_for_fire += barrel_rotate_speed;
                barrel_rotate_speed -= barrel_rotate_deceleration;
                barrel_rotate_speed *= barrel_rotate_feedback;
            }

            if (stored_angle_for_fire > barrel_rotate_interval_angle)
            {
                DeviceBehavior_Shooter_Try_Shoot(true);
                stored_angle_for_fire -= barrel_rotate_interval_angle;
            }
        }

        protected override bool DeviceBehaviour_Shooter_Try_Start_Reloading()
        {
            if (fsm == Device_FSM_Shooter.idle && current_ammo != fire_logic_data.capacity && carry_count >= 0)
            {
                reload_start_process += (fire_logic_data.reload_trigger_speed + fire_logic_data.reload_trigger_speed * 5);

                if (reload_start_process >= 1)
                {
                    FSM_change_to(Device_FSM_Shooter.reloading);
                    self_reloading_process = 0;
                    return true;
                }
            }

            return false;
        }

        protected override bool DeviceBehavior_Shooter_Try_Reload(bool manual_instantly_reloading)
        {
            var can_reload = manual_instantly_reloading ? can_manual_reload() : can_auto_reload();
            if (can_reload)
            {
                BattleContext.instance.load_event?.Invoke(this);
                var reload_num = fire_logic_data.capacity * (float)carry_count/(float)carry_limit;
                current_ammo = reload_num > 0 ? current_ammo + (int)reload_num : (int)fire_logic_data.capacity;


                current_ammo = Mathf.Min(current_ammo, (int)fire_logic_data.capacity);
                carry_count = 0;
            }
            return can_reload;
        }

        public void Try_Carrying()
        {
            DeviceBehaviour_Shooter_Try_Carry();
        }

        protected virtual bool DeviceBehaviour_Shooter_Try_Carry()
        {
            if (fsm != Device_FSM_Shooter.broken && carry_count < carry_limit)
            {
                carry_count++;
                return true;
            }

            return false;
        }

        public override void TryToAutoReload()
        {
            if (current_ammo != 0 || carry_count != carry_limit)
                return;

            if (reload_job_process < 1)
            {
                reload_job_process += fire_logic_data.job_speed[0];
                return;
            }

            if (Auto_Reload_Job_Content())
                reload_job_process = 0;
        }


        public override void TryToAutoAttack()
        {
            if (target_list.Count == 0 || target_in_radius(target_list[0]) == false)
                return;

            Auto_Attack_Job_Content();

        }

        protected override bool Auto_Attack_Job_Content()
        {
            ratchetShoot.rotate(1, true);
            Speed_Up_Spining(1);
            return true;
        }

        //外界调用
        public void Speed_Up_Spining(float power01)
        {
            Barrel_Rotation_Speed_Current += power01 * crank_rotate_factor;
        }

        public void TryToAutoCarry()
        {
            if(carry_count >= carry_limit)
                return;

            if (carry_job_process < 1)
            {
                carry_job_process += fire_logic_data.job_speed[2];
                return;
            }

            if (Auto_Carry_Job_Content())
                carry_job_process = 0;

        }

        protected virtual bool Auto_Carry_Job_Content()
        {
            return DeviceBehaviour_Shooter_Try_Carry();
        }

        public float Barrel_Rotation_Speed_Current
        {
            get { return barrel_rotate_speed; }
            private set
            {
                barrel_rotate_speed = Mathf.Clamp(value, 0, BARREL_ROTATE_SPEED_MAX);
                ratchetShoot.Rotation_Speed_Limit_In_Right_Dir = barrel_rotate_speed * CRANK_ROTATE_SPEED_COEF;
            }
        }

        public float Barrel_Rotation_Angle
        {
            get { return barrel_rotate_angle; }
            private set { barrel_rotate_speed = value; }
        }
    }
}
