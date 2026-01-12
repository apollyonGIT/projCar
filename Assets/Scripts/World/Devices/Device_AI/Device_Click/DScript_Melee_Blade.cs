using System;

namespace World.Devices.Device_AI
{
    public class DScript_Melee_Blade :DScript_Melee
    {
        public int attack_index = 1;


        protected override void idle_state_tick()
        {
            
        }

        protected override void attack_state_tick()
        {
            
        }


        protected override bool can_auto_attack()
        {
            return can_attack_check_cd()
                && can_attack_check_post_cast()
                && can_attack_check_state();
        }
        protected override bool Auto_Attack_Job_Content()
        {
            DeviceBehavior_Melee_Try_Attack(false, $"attack_", null);
            
            return true;
        }

        public override void Try_Attacking()
        {
            DeviceBehavior_Melee_Try_Attack(true, "attack_", null);
        }

        protected override bool DeviceBehavior_Melee_Try_Attack(bool ui_manual_atk, string atk_anim, Action action_on_hit_enemy = null)
        {
            var can_atk = ui_manual_atk ? can_manual_attack() : can_auto_attack();
            if (can_atk)
            {
                _atk_anim_name = atk_anim + attack_index.ToString();
                attack_index = (attack_index==1)? 2 : 1;
                this.action_on_hit_enemy = action_on_hit_enemy;
                FSM_change_to(Device_FSM_Melee.attacking);
            }
            return can_atk;
        }

        protected override void FSM_change_to(Device_FSM_Melee target_fsm)
        {
            fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_FSM_Melee.idle:
                    ChangeAnim(attack_index == 1?"idle":"idle_2", true);
                    post_cast_finished = true;
                    rotate_speed = rotation_speed_fast;
                    break;
                case Device_FSM_Melee.attacking:
                    ChangeAnim(_atk_anim_name, false);
                    post_cast_finished = false;
                    rotate_speed = rotation_speed_slow;
                    break;
                case Device_FSM_Melee.broken:
                    ChangeAnim(ANIM_BROKEN, true);
                    post_cast_finished = true;
                    rotate_speed = rotation_speed_fast;
                    break;
                default:
                    break;
            }
        }
    }
}
