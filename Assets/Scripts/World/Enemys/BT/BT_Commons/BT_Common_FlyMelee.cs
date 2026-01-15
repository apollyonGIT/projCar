using Commons;
using UnityEngine;
using World.Helpers;
using static World.Enemys.BT.BT_Common_FlyMelee;

namespace World.Enemys.BT
{
    public class BT_Common_FlyMelee : Enemy_BT, IEnemy_BT_FlyAround, IEnemy_BT_Attack_Delay<EN_FSM>
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Idle,
            Chase1,
            Chase2,
            Attack
        }
        EN_FSM m_state;
        public override string state => $"{m_state}";
        #endregion

        #region PRM
        Vector2 e_flyAround_deg_1;
        Vector2 e_flyAround_radius_1;
        int e_flyAround_count_max_1;
        int e_flyAround_count_min_1;

        Vector2 e_flyAround_deg_2;
        Vector2 e_flyAround_radius_2;
        int e_flyAround_count_max_2;
        int e_flyAround_count_min_2;

        float e_atk_dis;

        int e_atk_cd;
        int m_atk_cd;

        float e_before_attack_sec;
        float e_after_attack_sec;
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
            e_flyAround_deg_1 = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_DEG_1");
            e_flyAround_radius_1 = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_RADIUS_1");
            e_flyAround_count_max_1 = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MAX_1"));
            e_flyAround_count_min_1 = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MIN_1"));

            e_flyAround_deg_2 = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_DEG_2");
            e_flyAround_radius_2 = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_RADIUS_2");
            e_flyAround_count_max_2 = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MAX_2"));
            e_flyAround_count_min_2 = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MIN_2"));

            e_before_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEFORE_ATTACK_SEC"));
            e_after_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "AFTER_ATTACK_SEC"));

            e_atk_dis = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATK_DIS"));
            e_atk_cd = Mathf.RoundToInt(float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATK_CD")) * Config.PHYSICS_TICKS_PER_SECOND);
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Chase1;

            cell.mover.move_type = EN_enemy_move_type.Fly1;

            cell.base_speed = cell._desc.base_speed;

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


        public override void notify_on_set_face_dir(Enemy cell)
        {
            Enemy_BT_Face_CPN.set_face_dir_to_player_only_x(cell);
        }


        #region Chase1
        public void start_Chase1(Enemy cell)
        {
            e_flyAround_deg = e_flyAround_deg_1;
            e_flyAround_radius = e_flyAround_radius_1;
            e_flyAround_count_max = e_flyAround_count_max_1;
            e_flyAround_count_min = e_flyAround_count_min_1;

            m_flyAround_count_limit = Random.Range(e_flyAround_count_min, e_flyAround_count_max + 1);

            i_flyAround.calc_flyAround_pos();
        }


        public void do_Chase1(Enemy cell)
        {
            SeekTarget_Helper.select_caravan(out var t_caravan);
            i_flyAround.flyAround_on_tick(cell, t_caravan.Position);

            if (m_flyAround_count >= m_flyAround_count_limit)
            {
                m_flyAround_count = 0;
                m_state = EN_FSM.Idle;
            }
        }
        #endregion


        #region Idle
        public void do_Idle(Enemy cell)
        {
            if (m_atk_cd == e_atk_cd)
            {
                m_state = EN_FSM.Chase2;
            }
        }
        #endregion


        #region Chase2
        public void start_Chase2(Enemy cell)
        {
            e_flyAround_deg = e_flyAround_deg_2;
            e_flyAround_radius = e_flyAround_radius_2;
            e_flyAround_count_max = e_flyAround_count_max_2;
            e_flyAround_count_min = e_flyAround_count_min_2;

            m_flyAround_count_limit = Random.Range(e_flyAround_count_min, e_flyAround_count_max + 1);

            i_flyAround.calc_flyAround_pos();
        }


        public void do_Chase2(Enemy cell)
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
                m_state = EN_FSM.Chase1;
            }
            #endregion
        }
        #endregion
    }
}

