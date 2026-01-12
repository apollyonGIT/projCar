using Commons;
using Foundations.Tickers;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Treetop_Vase: Enemy_BT
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
        int m_create_count;

        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            var e_create_count_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "CREATE_COUNT_AREA");
            m_create_count = Mathf.RoundToInt(Random.Range(e_create_count_area.x, e_create_count_area.y));

            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Idle;

            cell.mover.move_type = EN_enemy_move_type.None;

            SeekTarget_Helper.select_caravan(out cell.target);

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        public override void notify_on_dead(Enemy cell)
        {
            for (int i = 0; i < m_create_count; i++)
            {
                //进行召唤
                var mgr = cell.mgr;
                var pd = mgr.pd;

                var sub_cell_pos = cell.pos;
                sub_cell_pos.x -= mgr.ctx.caravan_pos.x;

                Request_Helper.delay_do($"add_enemy_req_201406", 1,
                    (_) => 
                    {
                        pd.create_single_cell_directly(201407u, sub_cell_pos, "Default");
                    });
            }

            base.notify_on_dead(cell);
        }
    }
}

