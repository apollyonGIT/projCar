using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Treetop_Ghost : Enemy_BT, IEnemy_BT_FlyAround
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Chase
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        int e_attack_cd_tick;
        int m_attack_cd_tick;

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
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_flyAround_deg = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_DEG");
            e_flyAround_radius = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_RADIUS");

            e_attack_cd_tick = Mathf.RoundToInt(float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATTACK_CD_SEC")) * Config.PHYSICS_TICKS_PER_SECOND);
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Chase;

            cell.mover.move_type = EN_enemy_move_type.Fly1;

            SeekTarget_Helper.select_caravan(out cell.target);

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        public override void notify_on_set_face_dir(Enemy cell)
        {
            Enemy_BT_Face_CPN.set_face_dir_to_player_only_x(cell);
        }


        #region Chase
        public void start_Chase(Enemy cell)
        {
            i_flyAround.calc_flyAround_pos();
        }


        public void do_Chase(Enemy cell)
        {
            i_flyAround.flyAround_on_tick(cell, cell.target.Position);

            m_attack_cd_tick--;
            m_attack_cd_tick = Mathf.Max(m_attack_cd_tick, 0);

            if (m_attack_cd_tick == 0 && Vector2.Distance(cell.pos, cell.target.Position) <= 5f)
            {
                if (Enemy_Hurt_Helper.do_aoe_hurt_by_dis(cell, 1f))
                    m_attack_cd_tick = e_attack_cd_tick;
            }
        }
        #endregion
    }
}

