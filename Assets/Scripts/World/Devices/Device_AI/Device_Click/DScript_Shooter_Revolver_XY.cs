using AutoCodes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class AmmoBullet_XY 
    {
        public enum SlotState
        {
            empty,
            filling,
            ready_to_shoot,
            already_shot,
        }

        public SlotState slot_state = SlotState.ready_to_shoot;

        public float reload_process;
    }


    public class DScript_Shooter_Revolver_XY : DScript_Shooter
    {
        public List<AmmoBullet_XY> ammo_bullets = new();

        //现在准备开火的槽位

        private int m_current_index;
        public int current_index
        {
            get
            {
                return m_current_index;
            }
            set
            {
                if (value < 0)
                    value = 0;
                if (value >= fire_logic_data.capacity)
                    value = (int)(value - fire_logic_data.capacity);
                m_current_index = value;
            }
        }

        public override void InitData(device_all rc)
        {
            base.InitData(rc);

            for (int i = 0; i < fire_logic_data.capacity; i++)
            {
                AmmoBullet_XY ammo = new AmmoBullet_XY();
                ammo_bullets.Add(ammo);
            }


            current_index = 0;
        }

        protected override void reloading_state_tick()
        {
            DeviceBehavior_Select_Target();
            DeviceBehavior_Rotate_To_Target();
            current_shoot_cd = Mathf.Max(0, current_shoot_cd - 1);

            for(int i=0; i < ammo_bullets.Count; i++)
            {
                var ammo = ammo_bullets[i];
                if (ammo.slot_state == AmmoBullet_XY.SlotState.filling)
                {
                    ammo.reload_process += fire_logic_data.reload_speed;

                    if (ammo.reload_process >= 1.0f)
                    {
                        ammo.slot_state = AmmoBullet_XY.SlotState.ready_to_shoot;
                        ammo.reload_process = 0f;
                    }
                }
            }

            bool end_reloading = true;
            foreach (var ammo in ammo_bullets)
            {
                if (ammo.slot_state == AmmoBullet_XY.SlotState.filling)
                {
                    end_reloading = false;
                    break;
                }
            }

            if (end_reloading)
            {
                FSM_change_to(Device_FSM_Shooter.idle);
            }
        }



        protected override bool can_manual_shoot(bool ignore_post_cast = false)
        {
            return can_shoot_check_cd() && can_shoot_check_fsm() && can_shoot_check_shoot_stage(ignore_post_cast);
        }

        protected override bool can_auto_shoot()
        {
            return can_shoot_check_cd() && can_shoot_check_fsm() && can_shoot_check_shoot_stage(false) && can_shoot_check_error_angle();
        }
        protected override bool DeviceBehavior_Shooter_Try_Shoot(bool manual_shoot, bool ignore_post_cast = false, Action shoot_finished = null)
        {
            var can_shoot = (manual_shoot ? can_manual_shoot(ignore_post_cast) : can_auto_shoot());

            if (can_shoot)
            {
                var bullet = ammo_bullets[current_index];
                if(bullet.slot_state == AmmoBullet_XY.SlotState.ready_to_shoot)
                {
                    current_shoot_cd = fire_logic_data.cd;
                    bullet.slot_state = AmmoBullet_XY.SlotState.already_shot;
                    
                    foreach(var view in views)
                    {
                        view.notify_trigger_event("Trigger_Shoot", current_index );
                    }

                    current_index++;
                    FSM_change_to(Device_FSM_Shooter.shoot);

                    return true;
                }

                current_index++;
            }

            return false;
        }


        protected override bool Auto_Attack_Job_Content()
        {
            DeviceBehavior_Shooter_Try_Shoot(false);


            //无论如何都返回true，因为盘子要转

            return true;
        }

        public override void TryToAutoReload()
        {
            if(ammo_bullets.Count(ab => ab.slot_state == AmmoBullet_XY.SlotState.ready_to_shoot) == 0)
            {
                if (reload_job_process < 1)
                {
                    reload_job_process += fire_logic_data.job_speed[0];
                    return;
                }
                if (Auto_Reload_Job_Content())
                {
                    reload_job_process = 0;
                }
            }
        }

        protected override bool Auto_Reload_Job_Content()
        {
            var start_index = current_index;

            if(ammo_bullets.Count(ab => ab.slot_state == AmmoBullet_XY.SlotState.already_shot) != 0)
            {
                for (int i = 0; i < ammo_bullets.Count; i++)
                {
                    var index = (start_index + i) % ammo_bullets.Count;
                    ClearSlot(index);
                }
                return true;
            }
            else if(ammo_bullets.Count(ab => ab.slot_state == AmmoBullet_XY.SlotState.empty) != 0)
            {
                for (int i = 0; i < ammo_bullets.Count; i++)
                {
                    var index = (start_index + i) % ammo_bullets.Count;
                    ReloadSlot(index);
                }
                FSM_change_to(Device_FSM_Shooter.reloading);
                return true;
            }

            return false;
        }

        // -------------------------------------------------------------------------------
        public int GetAmmoCountIncludeFilling()
        {
            return ammo_bullets.Count(ab => (ab.slot_state != AmmoBullet_XY.SlotState.empty || ab.slot_state!= AmmoBullet_XY.SlotState.already_shot));
        }
        public bool Reloading()
        {
            return ammo_bullets.Count(ab => ab.slot_state == AmmoBullet_XY.SlotState.filling) > 0;
        }

        public bool ReloadSlot(int index)
        {
            if (ammo_bullets[index].slot_state == AmmoBullet_XY.SlotState.empty)
            {
                ammo_bullets[index].slot_state = AmmoBullet_XY.SlotState.filling;
                FSM_change_to(Device_FSM_Shooter.reloading);


                foreach(var view in views)
                {
                    view.notify_trigger_event("Trigger_Load", index);
                }
                return true;
            }
            return false;
        }

        public bool ClearSlot(int index)
        {
            if (ammo_bullets[index].slot_state == AmmoBullet_XY.SlotState.already_shot)
            {
                ammo_bullets[index].slot_state = AmmoBullet_XY.SlotState.empty;

                int num = UnityEngine.Random.Range(1,4);

                foreach(var view in views)
                {
                    view.notify_trigger_event($"Trigger_Release_{num}",index);
                }

                return true;
            }
            return false;
        }

        public AmmoBullet_XY GetSlot(int index)
        {
            return ammo_bullets[index];
        }
    }
}
