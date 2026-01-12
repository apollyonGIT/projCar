using Commons;
using Foundations.Tickers;
using UnityEngine;
using World.Helpers;
using static World.Enemys.BT.BT_Desert_Snake;

namespace World.Enemys.BT
{
    public class BT_Desert_Snake : Enemy_BT, IEnemy_BT_Attack_Delay<EN_FSM>
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Idle,
            Chase,
            Attack,
            BurrowIn,
            BurrowOut
        }
        EN_FSM m_state;
        public override string state => $"{m_state}";
        #endregion

        #region PRM
        float e_walk_speed;
        
        int e_half_burrow_tick;
        float e_burrow_valid_dis;

        float e_before_attack_sec;
        float e_after_attack_sec;

        float e_begin_atk_dis;
        float e_atk_dis;

        int e_atk_cd;
        int m_atk_cd;
        #endregion

        #region Attack_Delay
        float IEnemy_BT_Attack_Delay<EN_FSM>.before_attack_sec => e_before_attack_sec;
        float IEnemy_BT_Attack_Delay<EN_FSM>.after_attack_sec => e_after_attack_sec;

        EN_FSM IEnemy_BT_Attack_Delay<EN_FSM>.state { get => m_state; set => m_state = value; }
        EN_FSM IEnemy_BT_Attack_Delay<EN_FSM>.charge_next_state => EN_FSM.Idle;

        IEnemy_BT_Attack_Delay<EN_FSM> i_attack_delay => this;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_walk_speed = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "WALK_SPEED"));

            e_half_burrow_tick = Mathf.RoundToInt(float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BURROW_SEC")) / 2f * Config.PHYSICS_TICKS_PER_SECOND);
            e_burrow_valid_dis = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BURROW_VALID_DIS"));

            e_before_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEFORE_ATTACK_SEC"));
            e_after_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "AFTER_ATTACK_SEC"));

            e_begin_atk_dis = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEGIN_ATK_DIS"));
            e_atk_dis = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATK_DIS"));
            e_atk_cd = Mathf.RoundToInt(float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATK_CD")) * Config.PHYSICS_TICKS_PER_SECOND);
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Idle;

            cell.mover.move_type = EN_enemy_move_type.Walk;

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            SeekTarget_Helper.nearest_player_device(cell, out cell.target);
            if (cell.target == null)
            {
                SeekTarget_Helper.select_caravan(out cell.target);
            }

            m_atk_cd++;
            m_atk_cd = Mathf.Min(m_atk_cd, e_atk_cd);

            base.tick(cell);
        }


        #region Idle
        public void do_Idle(Enemy cell)
        {
            cell.speed_expt = 0;

            var dis_to_target = Mathf.Abs(cell.pos.x - cell.target.Position.x);

            if (dis_to_target < e_atk_dis)
            {
                if (m_atk_cd != e_atk_cd) return;

                m_state = EN_FSM.Attack;
                return;
            }

            if (dis_to_target <= e_burrow_valid_dis)
            {
                m_state = EN_FSM.Chase;
                return;
            }

            if (dis_to_target > e_burrow_valid_dis)
            {
                m_state = EN_FSM.BurrowIn;
                return;
            }
        }
        #endregion


        #region Chase
        public void start_Chase(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

            cell.speed_expt = e_walk_speed;
        }


        public void do_Chase(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

            var dis_to_target = Mathf.Abs(cell.pos.x - cell.target.Position.x);

            if (dis_to_target < e_begin_atk_dis)
            {
                if (m_atk_cd == e_atk_cd)
                {
                    m_state = EN_FSM.Attack;
                    return;
                }

                m_state = EN_FSM.Idle;
                return;
            }

            if (dis_to_target > e_burrow_valid_dis)
            {
                m_state = EN_FSM.BurrowIn;
                return;
            }
        }
        #endregion


        #region Attack
        public void start_Attack(Enemy cell)
        {
            //cell.speed_expt = 0f;

            i_attack_delay.@do(cell, attack, attack_delay_callback);

            #region 子函数 attack
            void attack()
            {
                if (Mathf.Abs(cell.pos.x - cell.target.Position.x) <= e_atk_dis)
                {
                    Enemy_Hurt_Helper.do_hurt(cell);
                }
            }
            #endregion

            #region 子函数 attack_delay_callback
            void attack_delay_callback()
            {
                m_atk_cd = 0;
                m_state = EN_FSM.Idle;
            }
            #endregion
        }
        #endregion


        #region BurrowIn
        public void start_BurrowIn(Enemy cell)
        {
            cell.is_ban_select = true;

            Request_Helper.delay_do($"BurrowIn_{cell.GUID}", e_half_burrow_tick,
                (_) => 
                {
                    m_state = EN_FSM.BurrowOut;
                });
        }
        #endregion


        #region BurrowOut
        public void start_BurrowOut(Enemy cell)
        {
            cell.is_ban_select = false;

            SeekTarget_Helper.select_caravan(out var t_caravan);
            cell.pos = t_caravan.Position;
            cell.pos.x += Random.Range(-e_atk_dis, e_atk_dis);
            cell.pos.y = Road_Info_Helper.try_get_altitude(cell.pos.x);

            Request_Helper.delay_do($"BurrowOut_{cell.GUID}", e_half_burrow_tick,
                (_) =>
                {
                    m_state = EN_FSM.Idle;
                });
        }
        #endregion
    }
}

