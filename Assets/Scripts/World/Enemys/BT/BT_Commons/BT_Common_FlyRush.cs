using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Common_FlyRush : Enemy_BT, IEnemy_BT_FlyAround
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Chase,
            Attack,
        }
        EN_FSM m_state;
        public override string state => $"{m_state}";
        #endregion

        #region PRM
        float e_fly_speed;
        float e_charge_speed;

        int e_max_charge_tick;
        int m_charge_tick;

        float e_attack_dis;
        #endregion

        #region FlyAround
        Vector2 IEnemy_BT_FlyAround.flyAround_deg => e_flyAround_deg;

        Vector2 IEnemy_BT_FlyAround.flyAround_radius => e_flyAround_radius;

        Vector2 IEnemy_BT_FlyAround.flyAround_pos { get => m_flyAround_pos; set => m_flyAround_pos = value; }
        Vector2 m_flyAround_pos;

        int IEnemy_BT_FlyAround.flyAround_count { get => m_flyAround_count; set => m_flyAround_count = value; }
        int m_flyAround_count;

        IEnemy_BT_FlyAround i_flyAround => this;

        Vector2 e_flyAround_deg;
        Vector2 e_flyAround_radius;

        int e_flyAround_count_max;
        int e_flyAround_count_min;
        int m_flyAround_count_limit;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region ¶Á±í²ÎÊý
            e_fly_speed = cell._desc.base_speed;
            e_charge_speed = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "CHARGE_SPEED"));

            e_max_charge_tick = Mathf.RoundToInt(float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "MAX_CHARGE_SEC")) * Config.PHYSICS_TICKS_PER_SECOND);

            e_attack_dis = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATTACK_DIS"));

            e_flyAround_deg = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_DEG");
            e_flyAround_radius = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_RADIUS");

            e_flyAround_count_max = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MAX"));
            e_flyAround_count_min = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MIN"));
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Chase;

            cell.mover.move_type = EN_enemy_move_type.Fly1;

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            SeekTarget_Helper.nearest_player_device(cell, out cell.target);
            if (cell.target == null)
            {
                SeekTarget_Helper.select_caravan(out cell.target);
            }

            base.tick(cell);
        }


        #region Chase
        public void start_Chase(Enemy cell)
        {
            cell.base_speed = e_fly_speed;

            m_flyAround_count_limit = Random.Range(e_flyAround_count_min, e_flyAround_count_max + 1);

            i_flyAround.calc_flyAround_pos();
        }


        public void do_Chase(Enemy cell)
        {
            SeekTarget_Helper.select_caravan(out var t_caravan);
            i_flyAround.flyAround_on_tick(cell, t_caravan.Position);

            if (m_flyAround_count >= m_flyAround_count_limit)
            {
                m_flyAround_count = 0;
                m_state = EN_FSM.Attack;
            }
        }
        #endregion


        #region Attack
        public void start_Attack(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

            cell.base_speed = e_charge_speed;

            m_charge_tick = 0;
        }


        public void do_Attack(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

            if (m_charge_tick > e_max_charge_tick)
            {
                m_state = EN_FSM.Chase;
                return;
            }

            if (Vector2.Distance(cell.target.Position, cell.pos) <= e_attack_dis)
            {
                Enemy_Hurt_Helper.do_hurt(cell);
                Enemy_BT_Audio_CPN.play_attack_audio(cell);

                m_state = EN_FSM.Chase;
                return;
            }

            m_charge_tick++;
        }
        #endregion
    }
}

