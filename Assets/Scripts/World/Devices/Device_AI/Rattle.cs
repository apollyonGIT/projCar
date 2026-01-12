using AutoCodes;
using Commons;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class Rattle : Device, IAttack, IAttack_Device,IEnergy
    {
        #region CONST
        private const string ANIM_IDLE = "idle";
        private const string ANIM_ATTACK = "attack_1";
        private const string ANIM_BROKEN = "idle";
        private const string COLLIDER_1 = "collider_1";
        private const string KEY_POINT_1 = "collider_1";
        #endregion

        #region IAttack
        public float Damage_Increase { get; set; }
        public float Knockback_Increase { get; set; }
        public int Attack_Interval { get; set; }
        public int Current_Interval { get; set; }
        public float Attack_Interval_Factor { get; set; }

        #endregion

        #region IEnergy

        public float Current_Energy { get; set; }
        public float Max_Energy { get; set; }
        public float Energy_Recover { get; set; }
        public int Energy_FreezeTick_Current { get; set; }
        public int Energy_FreezeTick_Max { get; set; }
        public float Default_Energy { get; set; }
        public float Energy_Recover_Factor { get ; set ; }

        #endregion

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;

        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;

        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;

        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;

        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;

        #endregion

        private int melee_atk_dmg;
        private float basic_knockback;

        public void TryToAutoAttack()
        {
            
        }

        private enum Device_Rattle_FSM
        {
            idle,
            attack,
            broken,
        }

        private Device_Rattle_FSM fsm;

        public override void InitData(device_all rc)
        {
            bones_direction.Clear();

            melee_logics.TryGetValue(rc.melee_logic.ToString(), out var logic);
            battle_datas.TryGetValue(rc.id.ToString(), out var battle_data);


            base.InitData(rc);

            var col_turn_on_time = 0.1f;
            var col_turn_off_time = 1.0f;

            Default_Energy = 100;
            Max_Energy = Default_Energy + BattleContext.instance.energy_capacity_add;
            Current_Energy = 0;

            #region Anim_Event_Attack
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK,
                percent = col_turn_on_time,
                anim_event = (Device d) =>
                {
                    OpenCollider(COLLIDER_1,rattle_attack);
                }
            });

            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK,
                percent = col_turn_off_time,
                anim_event = (Device d) =>
                {
                    CloseCollider(COLLIDER_1);   
                }
            });

            #endregion

            void rattle_attack(ITarget t)
            {
                int final_dmg = (int)((melee_atk_dmg + Damage_Increase));
                float final_ft = (basic_knockback + Knockback_Increase);
                Attack_Data attack_data = new()
                {
                    atk = final_dmg,
                    critical_chance = battle_data.critical_chance,
                    critical_dmg_rate = battle_data.critical_rate,
                };

                attack_data.calc_device_coef(this);

                t.hurt(this, attack_data, out var dmg_data);
                BattleContext.instance.ChangeDmg(this, dmg_data.dmg);

                if (t.hp < 0)
                {
                    kill_enemy_action?.Invoke(t);
                }

                t.impact(WorldEnum.impact_source_type.melee, (Vector2)key_points[KEY_POINT_1].position, BattleUtility.get_target_colllider_pos(t), final_ft);
            }
        }

        

        private void FSM_change_to(Device_Rattle_FSM target_fsm)
        {
            fsm = target_fsm;
            switch (fsm)
            {
                case Device_Rattle_FSM.idle:
                    // 进入空闲状态
                    ChangeAnim(ANIM_IDLE, true);
                    break;
                case Device_Rattle_FSM.attack:
                    // 进入攻击状态
                    ChangeAnim(ANIM_ATTACK, true);
                    break;
                case Device_Rattle_FSM.broken:
                    // 进入损坏状态
                    ChangeAnim(ANIM_BROKEN, true);
                    break;
                default:
                    break;
            }
        }

        public override void tick()
        {
            if (!is_validate)       //坏了
                FSM_change_to(Device_Rattle_FSM.broken);

            switch (fsm)
            {
                case Device_Rattle_FSM.idle:
                    // 执行空闲状态的逻辑
                    if(Current_Energy > 0)
                    {
                        FSM_change_to(Device_Rattle_FSM.attack);
                    }

                    break;
                case Device_Rattle_FSM.attack:
                    // 执行攻击状态的逻辑

                    EnergeChangeAnimSpeed();

                    Current_Energy -= 5 * Config.PHYSICS_TICK_DELTA_TIME;

                    if(Current_Energy <= 0)
                    {
                        FSM_change_to(Device_Rattle_FSM.idle);
                    }
                    break;
                case Device_Rattle_FSM.broken:
                    // 执行损坏状态的逻辑
                    if (is_validate)
                        FSM_change_to(Device_Rattle_FSM.idle);
                    break;
            }

            base.tick();
        }

        public void TryToAccelerate()
        {
            
        }


        public void AddEnergy(float energy)
        {
            Current_Energy += energy;
            if (Current_Energy > Max_Energy)
                Current_Energy = Max_Energy;
        }


        private void EnergeChangeAnimSpeed()
        {
            var factor = Mathf.Max( -0.3f, - Current_Energy / Max_Energy);
            Attack_Interval_Factor = factor;
            UpdateData();

            foreach (var view in views)
            {
                view.notify_change_anim_speed(1 + Current_Energy / Max_Energy * 2);
            }
        }

        public override void UpdateData()
        {
            melee_logics.TryGetValue(desc.melee_logic.ToString(), out var logic);
            Attack_Interval = (int)(logic.cd * (1 + Attack_Interval_Factor));
            base.UpdateData();
        }
    }
}
