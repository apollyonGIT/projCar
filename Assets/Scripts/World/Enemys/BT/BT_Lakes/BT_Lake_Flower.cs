using Commons;
using Foundations.Tickers;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Lake_Flower : Enemy_BT
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Idle,
            Attack,
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        float e_atk_dis;

        float e_atk_interval_sec;
        float m_atk_cd;

        int m_atk_dir_flag = 1; //攻击方向，左负右正

        int e_to_atk_delay_tick;
        int m_to_atk_delay_tick;

        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_atk_dis = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATK_DIS"));
            e_atk_interval_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATK_INTERVAL_SEC"));

            e_to_atk_delay_tick = Mathf.RoundToInt(float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "TO_ATK_DELAY_SEC")) * Config.PHYSICS_TICKS_PER_SECOND);
            m_to_atk_delay_tick = e_to_atk_delay_tick;
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Idle;

            cell.mover.move_type = EN_enemy_move_type.None;

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        #region Idle
        public void start_Idle(Enemy cell)
        {
            SeekTarget_Helper.select_caravan(out cell.target);
        }


        public void do_Idle(Enemy cell)
        {
            if (Mathf.Abs(cell.pos.x - cell.target.Position.x) <= e_atk_dis)
            {
                if (--m_to_atk_delay_tick <= 0)
                {
                    m_state = EN_FSM.Attack;
                    m_to_atk_delay_tick = e_to_atk_delay_tick;
                }

                return;
            }

            m_to_atk_delay_tick = e_to_atk_delay_tick;
        }
        #endregion


        #region Attack
        public void start_Attack(Enemy cell)
        {
            m_atk_cd = e_atk_interval_sec * Config.PHYSICS_TICKS_PER_SECOND;
            m_atk_dir_flag = 1;
        }


        public void do_Attack(Enemy cell)
        {
            if (Mathf.Abs(cell.pos.x - WorldContext.instance.caravan_pos.x) > e_atk_dis)
            {
                m_state = EN_FSM.Idle;
                return;
            }

            if (m_atk_cd-- <= 0)
            {
                seek_target();
                Enemy_Hurt_Helper.do_hurt(cell);

                m_atk_cd = e_atk_interval_sec * Config.PHYSICS_TICKS_PER_SECOND;
                m_atk_dir_flag *= -1;
            }

            #region 子函数 seek_target
            void seek_target()
            {
                //规则：索最近玩家设备
                SeekTarget_Helper.nearest_player_device_without_wheel(cell, out cell.target);

                if (cell.target != null) return;

                //规则：如果全部损坏，转火车体
                SeekTarget_Helper.select_caravan(out cell.target);
            }
            #endregion
        }
        #endregion
    }
}

