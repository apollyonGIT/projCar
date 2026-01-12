using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_FlyOnly : Enemy_BT, IEnemy_BT_FlyAround
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Move,
            Charge,
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        const float C_fly_speed_idle = 5f;
        const float C_fly_speed_brust = 16f;
        const float C_fly_speed_max = 30f;
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
            
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Move;

            cell.mover.move_type = EN_enemy_move_type.Fly1;

            SeekTarget_Helper.random_player_part(out var target);
            cell.target = target;

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            cell.hp = 1000;

            base.tick(cell);
        }


        #region Move
        public void start_Move(Enemy cell)
        {
            i_flyAround.calc_flyAround_pos();
        }


        public void do_Move(Enemy cell)
        {
            i_flyAround.flyAround_on_tick(cell, cell.target.Position);
        }
        #endregion

    }
}

