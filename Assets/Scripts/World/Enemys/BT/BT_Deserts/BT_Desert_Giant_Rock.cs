using Commons;
using Foundations.Tickers;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Desert_Giant_Rock : Enemy_BT, IEnemy_BT_Jump
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Idle,
            Jump,
            Land
        }
        EN_FSM m_state;
        public override string state => $"{m_state}";
        #endregion

        #region PRM
        int e_jump_cd;
        int m_jump_cd;

        float e_atk_dis;

        int e_land_tick;
        #endregion

        #region JUMP
        float IEnemy_BT_Jump.jump_velocity_x_max => e_jump_velocity_x_max;
        float e_jump_velocity_x_max;
        Vector2 IEnemy_BT_Jump.jump_height_area => e_jump_height_area;
        Vector2 e_jump_height_area;

        IEnemy_BT_Jump i_jump => this;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_jump_cd = Mathf.RoundToInt(float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "JUMP_CD")) * Config.PHYSICS_TICKS_PER_SECOND);
            e_atk_dis = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATK_DIS"));
            e_land_tick = Mathf.RoundToInt(float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "LAND_SEC")) * Config.PHYSICS_TICKS_PER_SECOND);

            e_jump_velocity_x_max = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "JUMP_VELOCITY_X_MAX"));
            e_jump_height_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "JUMP_HEIGHT_AREA");
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Idle;

            cell.mover.move_type = EN_enemy_move_type.Slide;

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            SeekTarget_Helper.nearest_player_device(cell, out cell.target);
            if (cell.target == null)
            {
                SeekTarget_Helper.select_caravan(out cell.target);
            }

            m_jump_cd++;
            m_jump_cd = Mathf.Min(m_jump_cd, e_jump_cd);

            base.tick(cell);
        }


        #region Idle
        public void do_Idle(Enemy cell)
        {
            if (m_jump_cd == e_jump_cd)
            {
                m_state = EN_FSM.Jump;
            }
        }
        #endregion


        #region Jump
        public void start_Jump(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

            i_jump.@do(cell);
        }


        public void do_Jump(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

            if (i_jump.is_land(cell))
            {
                m_state = EN_FSM.Land;
            }
        }
        #endregion


        #region Land
        public void start_Land(Enemy cell)
        {
            Enemy_Hurt_Helper.do_aoe_hurt_by_x(cell, e_atk_dis);

            Request_Helper.delay_do($"Land_{cell.GUID}", e_land_tick,
                (_) =>
                {
                    m_state = EN_FSM.Idle;
                });
        }
        #endregion
    }
}

