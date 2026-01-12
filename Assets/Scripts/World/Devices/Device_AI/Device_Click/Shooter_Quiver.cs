using AutoCodes;
using Foundations.Tickers;
using System.Collections.Generic;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class Shooter_Quiver : DScript_Shooter
    {
        #region Const
        private const int UI_BULLET_MAX_AMOUNT = 25;    // UI显示的最大弹药数
        protected const int RELOAD_CD = 10;

        private const float RATCHET_ROTATE_SPEED = 11F;
        private const float SHOOT_PER_ANGLE = 180F; // 每转过一定角度，就射出一发子弹
        #endregion

        //只有25个slot是可视的 他们的状态需要被记录
        public class Quiver_Ammo_Slot
        {
            public Shooter_Quiver owner;


            public int ammo_state = 0;         //0:空 1:有子弹 2:正在装填
            public int filling_tick = 0;        //只有正在装填的时候非0

            public const int filling_ticks_need = 5 * Commons.Config.PHYSICS_TICKS_PER_SECOND; //装填一发子弹需要的时间

            public void tick()
            {
                if (ammo_state == 2) //正在装填
                {
                    filling_tick++;
                    if (filling_tick >= filling_ticks_need)
                    {
                        ammo_state = 1; //装填完成
                        filling_tick = 0;

                        owner.current_ammo = (int)Mathf.Min(owner.current_ammo + 1, owner.fire_logic_data.capacity);
                        owner.Loading_Ammo--;
                    }
                }
            }

            public bool TryToReload()
            {
                if (ammo_state == 0) //空的
                {
                    ammo_state = 2; //开始装填
                    filling_tick = 0;
                    return true;
                }
                return false;
            }

        }
        public List<Quiver_Ammo_Slot> ammo_slots = new List<Quiver_Ammo_Slot>();

        public Device_Attachment_Ratchet Shoot_Ratchet_Rotation_Handle = new(-90f, RATCHET_ROTATE_SPEED, 0f);

        public int reload_tick = 0;                     //上次手动上弹的tick
        public int Loading_Ammo = 0;

        private int lever_angle_deduct_count = 0;          //把手扣动次数


        public override void InitData(device_all rc)
        {
            base.InitData(rc);

            for (int i = 0; i < UI_BULLET_MAX_AMOUNT; i++)
            {
                ammo_slots.Add(new Quiver_Ammo_Slot());
            }

            foreach (var ammo in ammo_slots)
            {
                ammo.owner = this;
                ammo.ammo_state = 1;
            }
        }

        protected override void Init_Anim_Event_List(fire_logic record)
        {
            var shoot = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = record.tick_percent,
                anim_event = (Device d) =>
                {
                    single_shoot(record, ammo_velocity_mod);
                    if (barrel_bullet_stage != Barrel_Ammo_Stage.not_necessarily)
                        Barrel_Ammo_Stage_Change(Barrel_Ammo_Stage.shell_remaining);
                    else
                    {
                        current_ammo--;
                        for (int i = 0; i < ammo_slots.Count; i++)
                        {
                            if (ammo_slots[i].ammo_state == 1) //有子弹
                            {
                                ammo_slots[i].ammo_state = 0;
                                break;
                            }
                        }
                    }

                    shoot_stage = Shoot_Stage.just_fired; //刚刚射击阶段
                    shoot_finished_action?.Invoke();
                }
            };
            var end_post_cast = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = record.rapid_fire_tick_percent,
                anim_event = (Device d) =>
                {
                    shoot_stage = Shoot_Stage.after_post_cast; //射击后摇阶段
                }
            };
            var back_to_idle = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = 1f,
                anim_event = (Device d) => FSM_change_to(Device_FSM_Shooter.idle)
            };

            anim_events.Add(shoot);
            anim_events.Add(end_post_cast);
            anim_events.Add(back_to_idle);
        }

        public override void tick()
        {
            base.tick();

            if (reload_tick > 0)
            {
                reload_tick--;
            }

            for (int i = ammo_slots.Count - 1; i >= 0; i--)
            {
                ammo_slots[i].tick();
            }
        }

        protected override void tick_after_fsm_while_unbroken()
        {
            base.tick_after_fsm_while_unbroken();

            if (Shoot_Ratchet_Rotation_Handle.Dir_Angle > SHOOT_PER_ANGLE * lever_angle_deduct_count + 90)
            {
                lever_angle_deduct_count++;
                DeviceBehavior_Shooter_Try_Shoot(true, true);  // 通过旋转角度来判断是否射击，需要忽略默认射击后摇
            }

            if (lever_angle_deduct_count >= 2)
            {
                lever_angle_deduct_count -= 2;
                Shoot_Ratchet_Rotation_Handle.Dir_Angle_Normalize(-1);
            }
        }

        public override void TryToAutoAttack()
        {
           /* var input = SHOOT_PER_ANGLE / Attack_Interval / RATCHET_ROTATE_SPEED;
            Shoot_Ratchet_Rotation_Handle.rotate(input, true);*/
        }

        protected override bool can_reload_check_ammo()
        {
            return current_ammo + Loading_Ammo < fire_logic_data.capacity;
        }

        protected override bool DeviceBehavior_Shooter_Try_Reload(bool manual_instantly_reloading)
        {
            var can_reload = manual_instantly_reloading ? can_manual_reload() : can_auto_reload();
            if (can_reload)
            {
                if (reload_tick <= 0)
                {
                    Loading_Ammo++;
                    reload_tick = RELOAD_CD;
                    BattleContext.instance.load_event?.Invoke(this);

                    if (current_ammo > UI_BULLET_MAX_AMOUNT)
                    {
                        Request_Helper.delay_do("reload", Quiver_Ammo_Slot.filling_ticks_need, (_) => { current_ammo = (int)Mathf.Min(current_ammo + 1, fire_logic_data.capacity); });
                    }
                    else
                    {
                        for (int i = 0; i < ammo_slots.Count; i++)
                        {
                            if (ammo_slots[i].TryToReload()) //空的
                            {
                                break;
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
