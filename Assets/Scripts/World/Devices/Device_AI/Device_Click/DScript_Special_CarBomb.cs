using AutoCodes;
using Commons;
using UnityEngine;
using World.Helpers;
using Foundations.Tickers;
using World.Caravans;

namespace World.Devices.Device_AI
{
    public class DScript_Special_CarBomb : Device,IAttack_New,IAttack_Device
    {
        #region CONST
        private const string ANIM_READY = "ready";
        private const string ANIM_IDLE = "idle";
        private const string ANIM_ATTACK = "attack";
        private const string ATK_COLLIDER = "atk_collider";
        protected const string KEY_POINT_1 = "collider_1";
        #endregion

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => 0;
        int IAttack_Device.ind_atk_add => 0;
        int IAttack_Device.ind_armor_piercing => 0;
        int IAttack_Device.ind_critical_chance =>0;
        int IAttack_Device.ind_critical_dmg_rate => 0;
        #endregion

        public int usage;
        public float burst_speed;
        public float before_atk_sec;
        public float cd;


        public int current_cd_tick = 0;
        //进入attack状态后计时s
        public int attack_state_time;
        protected enum Device_FSM_CarBomb
        {
            idle,
            empty,
            attack,
            broken,
        }

        protected Device_FSM_CarBomb fsm;
        protected other_logic other_logic_data;

        public int remain_use_count;
        public float t;

        private float slot2angle(string slot_name)
        {
            switch (slot_name)
            {
                case "slot_front":
                    return 90f;
                case "slot_front_top":
                    return -90f;
                case "slot_top":
                    return -90f;
                case "slot_back_top":
                    return -30f;
                case "slot_back":
                    return 30f;
            }

            return 0;
        }

        public override void InitData(device_all rc)
        {
            base.InitData(rc);

            other_logics.TryGetValue(rc.id.ToString(), out other_logic_data);

            usage = other_logic_data.diy_prms.TryGetValue("usage", out var usage_str) ? int.Parse(usage_str) : 3;
            burst_speed = other_logic_data.diy_prms.TryGetValue("burst_speed", out var burst_speed_str) ? float.Parse(burst_speed_str) : 4f;
            before_atk_sec = other_logic_data.diy_prms.TryGetValue("before_atk_sec", out var before_atk_sec_str) ? float.Parse(before_atk_sec_str) : 0.1f;
            cd = other_logic_data.diy_prms.TryGetValue("cd", out var cd_str) ? float.Parse(cd_str) : 1f;

            remain_use_count = usage;
        }

        protected void FSM_change_to(Device_FSM_CarBomb target_fsm)
        {
            fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_FSM_CarBomb.idle:
                    ChangeAnim(ANIM_READY, true);
                    break;
                case Device_FSM_CarBomb.empty:
                    ChangeAnim(ANIM_IDLE, true);
                    break;
                case Device_FSM_CarBomb.attack:
                    ChangeAnim(ANIM_ATTACK, false);
                    break;
                case Device_FSM_CarBomb.broken:
                    break;
            }
        }

        public override void tick()
        {
            if(state != DeviceState.stupor)
            {
                switch(fsm)
                {
                    case Device_FSM_CarBomb.idle:
                        idle_state_tick();
                        break;
                    case Device_FSM_CarBomb.empty:
                        empty_state_tick();
                        break;
                    case Device_FSM_CarBomb.attack:
                        attack_state_tick();
                        break;
                    case Device_FSM_CarBomb.broken:
                        if(is_validate)
                            FSM_change_to(Device_FSM_CarBomb.idle);
                        break;
                }
            }

            base.tick();
        }

        public virtual void idle_state_tick()
        {
            current_cd_tick = Mathf.Max(0, current_cd_tick - 1);

            t = current_cd_tick/ (cd * Config.PHYSICS_TICKS_PER_SECOND);
        }

        public virtual void empty_state_tick()
        {
        }

        public virtual void attack_state_tick()
        {
            attack_state_time++;

            if (attack_state_time >= before_atk_sec * Config.PHYSICS_TICKS_PER_SECOND)
            {
                OpenCollider(ATK_COLLIDER,attack_enter);
                attack_state_time = 0;
                remain_use_count--;
                var slot_name = Device_Slot_Helper.GetSlot(this);
                var angle = slot2angle(slot_name);

                var slot_burst_speed = new Vector2(burst_speed * Mathf.Cos(angle * Mathf.Deg2Rad), burst_speed * Mathf.Sin(angle * Mathf.Deg2Rad));

                if(slot_burst_speed.y > 0)
                {
                    CaravanMover.do_jump_input_vy(slot_burst_speed.y + WorldContext.instance.caravan_velocity.y);
                }
                else
                {
                    WorldContext.instance.caravan_velocity += new Vector2(0,slot_burst_speed.y);
                }

                WorldContext.instance.caravan_velocity.x = Mathf.Max(0, WorldContext.instance.caravan_velocity.x + slot_burst_speed.x);
               
                if (remain_use_count <= 0)
                {
                    FSM_change_to(Device_FSM_CarBomb.empty);
                }
                else
                {
                    t = 1;
                    current_cd_tick = Mathf.RoundToInt(cd * Config.PHYSICS_TICKS_PER_SECOND);
                    FSM_change_to(Device_FSM_CarBomb.idle);
                }

                Request_Helper.delay_do("car_bomb_attack_collider_close", 3, (req) =>
                {
                    CloseCollider(ATK_COLLIDER);
                });
            }
        }

        private void attack_enter(ITarget t)
        {
            var attack_datas = ExecDmg(t, other_logic_data.damage);

            foreach (var attack_data in attack_datas)
            {
                attack_data.calc_device_coef(this);
                t.hurt(this, attack_data, out var dmg_data);
                BattleContext.instance.ChangeDmg(this, dmg_data.dmg);
                t.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);
            }

            float final_ft = (other_logic_data.knockback_ft);

            if (t.hp <= 0)
            {
                kill_enemy_action?.Invoke(t);
            }

            t.impact(WorldEnum.impact_source_type.melee, (Vector2)key_points[KEY_POINT_1].position, BattleUtility.get_target_colllider_pos(t), final_ft);
        }

        public virtual void TryToAutoAttack()
        {
            if(fsm!= Device_FSM_CarBomb.idle || current_cd_tick > 0 || try_get_target() == false)
                return;

            DeviceBehaviour_Attack();
        }

        protected virtual bool DeviceBehaviour_Attack()
        {
            FSM_change_to(Device_FSM_CarBomb.attack);
            return true;
        }


        //-------------------------------------------------------------
        #region 给外部调用的接口
        public void Try_Attacking()
        {
            DeviceBehaviour_Attack();
        }

        public bool CanAttack()
        {
            return fsm == Device_FSM_CarBomb.idle && current_cd_tick <= 0;
        }
        #endregion
    }
}
