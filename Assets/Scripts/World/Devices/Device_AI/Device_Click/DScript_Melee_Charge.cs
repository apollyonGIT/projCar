using AutoCodes;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class DScript_Melee_Charge : Device, IAttack_Device, IAttack_New,ICharge
    {
        #region CONST
        private const string ANIM_IDLE = "idle";
        private const string ANIM_CHARGE = "charge";
        private const string ANIM_ATTACK = "attack ";
        private const string ATK_COLLIDER = "atk_collider";
        private const string KEY_POINT_1 = "collider_1";
        #endregion

        protected enum Device_FSM_Charge
        {
            idle,
            charge,
            attack,
            broken,
        }

        protected Device_FSM_Charge fsm;
        protected other_logic other_logic_data;
        public float charge_process
        {
            get
            {
                return m_charge_process;
            }

            set
            {
                m_charge_process = Mathf.Clamp01(value);
            }
        }
        private float m_charge_process;

        #region diy_prms

        public int atk_gap;
        public float charge_rate;
        public int charge_level;
        public float consume_rate;

        private int current_atk_gap;
        private float charge_job_process;
        #endregion


        // ----------------------------------------------------------------------------------------

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;

        #endregion
        // ========================================================================================

        public override void InitData(device_all rc)
        {
            device_type = DeviceType.other;

            other_logics.TryGetValue(rc.other_logic.ToString(), out other_logic_data);

            atk_gap = other_logic_data.diy_prms.TryGetValue("atk_gap", out var atk_gap_str) ? int.Parse(atk_gap_str) : 1;
            charge_rate = other_logic_data.diy_prms.TryGetValue("charge_rate", out var charge_rate_str) ? float.Parse(charge_rate_str) : 0.1f;
            charge_level = other_logic_data.diy_prms.TryGetValue("charge_level", out var charge_level_str) ? int.Parse(charge_level_str) : 1;
            consume_rate = other_logic_data.diy_prms.TryGetValue("consume_rate", out var consume_rate_str) ? float.Parse(consume_rate_str) : 0.1f;

            charge_process = 0;

            base.InitData(rc);
        }

        public override void tick()
        {
            if (state != DeviceState.stupor)
            {
                switch (fsm)
                {
                    case Device_FSM_Charge.idle:
                        idle_state_tick();
                        break;
                    case Device_FSM_Charge.charge:
                        charge_state_tick();
                        break;
                    case Device_FSM_Charge.attack:
                        attack_state_tick();
                        break;
                    case Device_FSM_Charge.broken:
                        if(is_validate)
                            FSM_change_to(Device_FSM_Charge.idle);
                        break;
                }
            }

            base.tick();
        }

        public virtual void idle_state_tick()
        {

        }

        public virtual void charge_state_tick()
        {
            int last_stage = (int)(charge_process/ (1f / charge_level));
            charge_process += charge_rate;
            int current_stage = (int)(charge_process / (1f / charge_level));
            if (last_stage != current_stage)
            {
                FSM_change_to(Device_FSM_Charge.idle);
            }
        }

        public virtual void attack_state_tick()
        {
            charge_process -= consume_rate;
            if (charge_process == 0)
                FSM_change_to(Device_FSM_Charge.idle);
        }

        protected void FSM_change_to(Device_FSM_Charge target_fsm)
        {
            fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_FSM_Charge.idle:
                    ChangeAnim(ANIM_IDLE,true);
                    CloseCollider(ATK_COLLIDER);
                    break;
                case Device_FSM_Charge.charge:
                    ChangeAnim(ANIM_CHARGE, true);
                    CloseCollider(ATK_COLLIDER);
                    break;
                case Device_FSM_Charge.attack:
                    ChangeAnim(ANIM_ATTACK, true);
                    OpenCollider(ATK_COLLIDER, collider_enter_event, collider_stay_event);
                    break;
                case Device_FSM_Charge.broken:
                    CloseCollider(ATK_COLLIDER);
                    break;
                default:
                    break;
            }
        }

        protected virtual void collider_enter_event(ITarget t)
        {
            if(atk_gap == 0)
            {
                make_dmg_and_knockback(t);
            }
        }

        private void make_dmg_and_knockback(ITarget t) 
        {
            var attack_datas = ExecDmg(t, other_logic_data.damage);

            foreach (var attack_data in attack_datas)
            {
                attack_data.calc_device_coef(this);
                t.hurt(this, attack_data, out var dmg_data);
                BattleContext.instance.ChangeDmg(this, dmg_data.dmg);

                t.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);

                BattleContext.instance.melee_damage_event?.Invoke(dmg_data, this, t);
            }

            BattleContext.instance.melee_attack_event?.Invoke(this, t);

            float final_ft = (other_logic_data.knockback_ft);

            if (t.hp <= 0)
            {
                kill_enemy_action?.Invoke(t);
            }
            t.impact(WorldEnum.impact_source_type.melee, (Vector2)key_points[KEY_POINT_1].position, BattleUtility.get_target_colllider_pos(t), final_ft);
        }

        protected virtual void collider_stay_event(ITarget t)
        {
            if (atk_gap != 0)
            {
                current_atk_gap++;
                if(current_atk_gap >= atk_gap)
                {
                    current_atk_gap -= atk_gap;
                    make_dmg_and_knockback(t);
                }
            }
        }   

        protected virtual bool DeviceBehaviour_Charge_Charge()
        {
            if (charge_process == 1f)
                return false;

            FSM_change_to(Device_FSM_Charge.charge);

            return true;
        }

        protected virtual bool DeviceBehaviour_Charge_Attack()
        {
            if(charge_process > 0)
            {
                FSM_change_to(Device_FSM_Charge.attack);
                return true;
            }

            return false;
        }


        public virtual void TryToAutoAttack()
        {
            if (fsm != Device_FSM_Charge.idle)
            {
                return;
            }

            if(try_get_target())
                DeviceBehaviour_Charge_Attack();
        }

        protected override bool try_get_target()
        {
            var ts = BattleUtility.select_all_target_in_circle(position, desc.basic_range.Item2, faction, (ITarget ts) =>
            {
                return target_can_be_selected(ts);
            });

            return ts.Count>0;
        }

        public virtual void TryToAutoCharge()
        {
            if (fsm != Device_FSM_Charge.idle || charge_process ==1)
            {
                return;
            }
            if(charge_job_process < 1)
            {
                charge_job_process += other_logic_data.job_speed[0];
                return;
            }

            if(Auto_Charge_Job_Content())
            {
                charge_job_process = 0;
            }
        }

        protected virtual bool Auto_Charge_Job_Content()
        {
            return DeviceBehaviour_Charge_Charge();
        }


        //---------------------------------------------------
        #region 给外部调用的接口
        public bool IsIdle()
        {
            return fsm == Device_FSM_Charge.idle;
        }

        public void Try_Charging()
        {
            if (fsm == Device_FSM_Charge.idle)
            {
                DeviceBehaviour_Charge_Charge();
            }
            
        }

        public void Try_Attacking()
        {
            if(fsm == Device_FSM_Charge.idle && charge_process != 0)
            {
                DeviceBehaviour_Charge_Attack();
            }
        }

        public bool CanAttack()
        {
            return fsm == Device_FSM_Charge.idle && charge_process > 0;
        }

        public bool CanCharge()
        {
            return  fsm != Device_FSM_Charge.charge && charge_process < 1f;
        }

        #endregion  
    }
}
