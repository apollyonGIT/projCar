using UnityEngine;
using World.Helpers;
using static World.Enemys.BT.BT_Common_WalkMelee;

namespace World.Enemys.BT
{
    public class BT_Common_WalkMelee : Enemy_BT, IEnemy_BT_Attack_Delay<EN_FSM>
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Chase,
            Attack
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        float e_walk_speed;

        Vector2 e_atk_area;
        float e_atk_dis;
        #endregion

        #region Attack_Delay
        float e_before_attack_sec;
        float e_after_attack_sec;

        float IEnemy_BT_Attack_Delay<EN_FSM>.before_attack_sec => e_before_attack_sec;
        float IEnemy_BT_Attack_Delay<EN_FSM>.after_attack_sec => e_after_attack_sec;

        EN_FSM IEnemy_BT_Attack_Delay<EN_FSM>.state { get => m_state; set => m_state = value; }
        EN_FSM IEnemy_BT_Attack_Delay<EN_FSM>.charge_next_state => EN_FSM.Chase;

        IEnemy_BT_Attack_Delay<EN_FSM> i_attack_delay => this;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_walk_speed = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "WALK_SPEED"));

            e_atk_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "ATK_AREA");
            e_atk_dis = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATK_DIS"));

            e_before_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEFORE_ATTACK_SEC"));
            e_after_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "AFTER_ATTACK_SEC"));
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Chase;

            cell.mover.move_type = EN_enemy_move_type.Walk;


            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);

            if (cell.mover.move_type == EN_enemy_move_type.Slide)
            {
                cell.mover.move_type = EN_enemy_move_type.Walk;
                cell.is_speed_expt_change = true;
            }

        }


        #region Chase
        public void start_Chase(Enemy cell)
        {
            cell.speed_expt = e_walk_speed;

            seek_target();

            #region 子函数 seek_target
            void seek_target()
            {
                //规则：索最近玩家设备，排除车轮、已损坏的设备
                SeekTarget_Helper.nearest_player_device_without_wheel(cell, out cell.target);
                if (cell.target != null) return;

                //规则：如果全部损坏，转火车体
                SeekTarget_Helper.select_caravan(out cell.target);
            }
            #endregion
        }


        public void do_Chase(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

            var pos_delta = cell.pos - cell.target.Position;
            var is_in_suqare_area = Mathf.Abs(pos_delta.x) <= e_atk_area.x && Mathf.Abs(pos_delta.y) <= e_atk_area.y;

            if (is_in_suqare_area)
            {
                m_state = EN_FSM.Attack;
            }
        }
        #endregion


        #region Attack
        public void start_Attack(Enemy cell)
        {
            cell.speed_expt = 0f;

            i_attack_delay.@do(cell, attack);

            #region 子函数 attack
            void attack()
            {
                if (Mathf.Abs(cell.pos.x - cell.target.Position.x) <= e_atk_dis)
                {
                    Enemy_Hurt_Helper.do_hurt(cell);
                }
            }
            #endregion
        }
        #endregion

    }
}

