using System.Collections.Generic;
using AutoCodes;
using System.Linq;
using UnityEngine;


namespace World.Devices.Device_AI
{
    public class AmmoBullet
    {
        public enum AmmoState
        {
            filling,
            filled,
            waiting,
            empty,
        }

        public AmmoState ammo_state = AmmoState.filled;
    }


    public class DScript_Shooter_Revolver : DScript_Shooter
    {
        #region Const
        private const int BULLET_FILLING_TICKS_MAX = Commons.Config.PHYSICS_TICKS_PER_SECOND;
        private const int MAX_AMMO = 6;
        #endregion

        // -------------------------------------------------------------------------------

        //正在上弹的槽位
        public AmmoBullet reloading_ammo;

        //正在队列等待上弹的槽位
        public List<AmmoBullet> waiting_ammo = new();

        //枪的所有弹槽
        public List<AmmoBullet> ammo_bullets = new();

        // ===============================================================================

        public override void InitData(device_all rc)
        {
            base.InitData(rc);

            reloading_ammo = null;

            waiting_ammo.Clear();

            for (int i = 0; i < fire_logic_data.capacity; i++)
            {
                AmmoBullet ammo = new AmmoBullet();
                ammo_bullets.Add(ammo);
            }
        }

        // -------------------------------------------------------------------------------

        protected override void reloading_state_tick()
        {
            DeviceBehavior_Select_Target();
            DeviceBehavior_Rotate_To_Target();
            current_shoot_cd = Mathf.Max(0, current_shoot_cd - 1);

            if (reloading_ammo != null)
            {
                self_reloading_process += fire_logic_data.reload_speed;

                if (self_reloading_process >= 1)
                {
                    self_reloading_process = 0;
                    DeviceBehavior_Shooter_Try_Reload(true);

                    if (current_ammo == fire_logic_data.capacity)
                        FSM_change_to(Device_FSM_Shooter.idle);
                }
            }
            else
            {
                if (waiting_ammo.Count > 0)
                {

                    reloading_ammo = waiting_ammo[0];
                    waiting_ammo.RemoveAt(0);
                    reloading_ammo.ammo_state = AmmoBullet.AmmoState.filling;

                    self_reloading_process += fire_logic_data.reload_speed;
                    if (self_reloading_process >= 1)
                    {
                        self_reloading_process = 0;
                        DeviceBehavior_Shooter_Try_Reload(false);
                        if (current_ammo == fire_logic_data.capacity)
                            FSM_change_to(Device_FSM_Shooter.idle);
                    }
                }
            }
        }



        protected override AnimEvent shoot_event()
        {
            return new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = fire_logic_data.tick_percent,
                anim_event = (Device d) =>
                {
                    single_shoot(fire_logic_data, ammo_velocity_mod);
                    if (barrel_bullet_stage != Barrel_Ammo_Stage.not_necessarily)
                        Barrel_Ammo_Stage_Change(Barrel_Ammo_Stage.shell_remaining);
                    else
                    {
                        for (int i = 0; i < ammo_bullets.Count; i++)
                        {
                            if (ammo_bullets[i].ammo_state == AmmoBullet.AmmoState.filled)
                            {
                                ammo_bullets[i].ammo_state = AmmoBullet.AmmoState.empty;
                                break;
                            }
                        }
                    }

                    current_ammo = ammo_bullets.FindAll(ab => ab.ammo_state == AmmoBullet.AmmoState.filled).Count;

                    shoot_stage = Shoot_Stage.just_fired; //刚刚射击阶段
                    shoot_finished_action?.Invoke();
                }
            };
        }

        /// <summary>
        /// 开火需要确认 是否上弹结束
        /// </summary>
        /// <param name="ignore_post_cast"></param>
        /// <returns></returns>
        protected override bool can_manual_shoot(bool ignore_post_cast = false)
        {
            return base.can_manual_shoot(ignore_post_cast) && can_shoot_check_reloading_over();
        }

        protected override bool can_shoot_check_ammo()
        {
            return ammo_bullets.FindAll(ab => ab.ammo_state == AmmoBullet.AmmoState.filled).Count >= 1;
        }

        protected override bool can_auto_shoot()
        {
            return base.can_auto_shoot() && can_shoot_check_reloading_over();
        }

        private bool can_shoot_check_reloading_over()
        {
            return waiting_ammo.Count == 0 && reloading_ammo == null;
        }

        //存在可以上弹的槽位
        protected override bool can_reload_check_ammo()
        {
            return ammo_bullets.Find(ab => ab.ammo_state == AmmoBullet.AmmoState.empty) != null;
        }

        // -------------------------------------------------------------------------------

        protected override bool DeviceBehaviour_Shooter_Try_Start_Reloading()
        {
            var can_reload = (fsm == Device_FSM_Shooter.idle && current_ammo != fire_logic_data.capacity) || (fsm == Device_FSM_Shooter.reloading && can_reload_check_ammo());
            if (can_reload)
            {
                FSM_change_to(Device_FSM_Shooter.reloading);
            }
            return can_reload; ;
        }

        protected override bool DeviceBehavior_Shooter_Try_Reload(bool manual_instantly_reloading)
        {
            var can_reload = (current_ammo != fire_logic_data.capacity && reloading_ammo != null);
            if (can_reload)
            {
                BattleContext.instance.load_event?.Invoke(this);

                reloading_ammo.ammo_state = AmmoBullet.AmmoState.filled;
                reloading_ammo = null;
                current_ammo = ammo_bullets.FindAll(ab => ab.ammo_state == AmmoBullet.AmmoState.filled).Count;
            }
            return can_reload;
        }

        //TryToAuto
        //==============================================================================================
        public override void TryToAutoReload()
        {
            var can_load = (current_ammo == 0 && waiting_ammo.Count == 0 && reloading_ammo == null) ||
                (fsm == Device_FSM_Shooter.reloading && (current_ammo + (reloading_ammo == null ? 0 : 1) + waiting_ammo.Count) < fire_logic_data.capacity);
            if (can_load)
            {
                if (reload_job_process < 1)
                {
                    reload_job_process += fire_logic_data.job_speed[0];
                    return;
                }

                if (Auto_Reload_Job_Content())
                {
                    var ammo = ammo_bullets.Find(ab => ab.ammo_state == AmmoBullet.AmmoState.empty);

                    if (ammo != null)
                    {
                        if (reloading_ammo != null)
                        {
                            waiting_ammo.Add(ammo);
                            ammo.ammo_state = AmmoBullet.AmmoState.waiting;
                        }
                        else
                        {

                            reloading_ammo = ammo;
                            ammo.ammo_state = AmmoBullet.AmmoState.filling;
                        }
                    }

                    reload_job_process = 0;
                }
            }
        }

        //给外部调用的接口
        //===============================================================================================

        public bool Reloading()
        {
            return reloading_ammo != null;
        }

        /// <summary>
        /// 获取当前弹药数量（包括正在上弹的)
        /// </summary>
        /// <returns></returns>
        public int GetAmmoCountIncludeFilling()
        {
            return ammo_bullets.Count(ab => ab.ammo_state != AmmoBullet.AmmoState.empty);
        }

        public bool ReloadSlot(int index)
        {
            if (DeviceBehaviour_Shooter_Try_Start_Reloading())
            {
                if (ammo_bullets[index].ammo_state == AmmoBullet.AmmoState.empty)
                {
                    if (reloading_ammo != null)
                    {
                        waiting_ammo.Add(ammo_bullets[index]);
                        ammo_bullets[index].ammo_state = AmmoBullet.AmmoState.waiting;
                    }
                    else
                    {
                        reloading_ammo = ammo_bullets[index];
                        ammo_bullets[index].ammo_state = AmmoBullet.AmmoState.filling;
                    }



                    return true;
                }
            }

            return false;
        }

        public int ReloadingAmmoIndex()
        {
            if (reloading_ammo == null)
                return -1;
            return ammo_bullets.IndexOf(reloading_ammo);
        }
    }
}