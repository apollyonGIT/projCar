using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World.Enemys.BT
{
    public class BT_Treetop_Berry_Bush : Enemy_BT
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Idle
        }
        EN_FSM m_state;
        public override string state => $"{m_state}";
        #endregion

        #region PRM
        public int e_sub_cell_count;

        public Vector2 e_sub_cell_pos_x_area;
        public Vector2 e_sub_cell_pos_y_area;

        public LinkedList<string> sub_cell_flags = new();
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_sub_cell_count = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "SUB_CELL_COUNT"));

            e_sub_cell_pos_x_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "SUB_CELL_POS_X_AREA");
            e_sub_cell_pos_y_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "SUB_CELL_POS_Y_AREA");
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Idle;

            cell.mover.move_type = EN_enemy_move_type.None;
            cell.is_ban_select = true;

            var mgr = cell.mgr;
            var pd = mgr.pd;

            for (int i = 0; i < e_sub_cell_count; i++)
            {
                var sub_cell_pos = cell.pos + new Vector2(Random.Range(e_sub_cell_pos_x_area.x, e_sub_cell_pos_x_area.y), Random.Range(e_sub_cell_pos_y_area.x, e_sub_cell_pos_y_area.y));
                sub_cell_pos.x -= mgr.ctx.caravan_pos.x;

                var sub_cell = pd.create_single_cell_directly(201403u, sub_cell_pos, "Default");
                var sub_cell_bt = sub_cell.bt as BT_Treetop_Berry;
                sub_cell_bt.owner = this;

                sub_cell_flags.AddLast(sub_cell.GUID);
            }

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        #region Idle
        public void do_Idle(Enemy cell)
        {
            if (sub_cell_flags.Any()) return;

            notify_on_dead(cell);
        }
        #endregion
    }
}

