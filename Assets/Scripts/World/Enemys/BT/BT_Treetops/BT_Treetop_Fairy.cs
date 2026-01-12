using Commons;
using Foundations.Tickers;
using System.Collections.Generic;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Treetop_Fairy : Enemy_BT, IEnemy_BT_FlyAround, IEnemy_BT_FlyFollow
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Chase,
            Hover,
            Summon
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        Vector2 e_create_cd_area;
        int m_create_cd_tick;

        int e_create_interval_tick;

        int e_sub_cell_count_single;
        int m_sub_cell_count_single;

        Vector2 e_sub_cell_speed_area;
        float sub_cell_speed => Random.Range(e_sub_cell_speed_area.x, e_sub_cell_speed_area.y);
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

        #region FlyFollow
        Vector2 IEnemy_BT_FlyFollow.flyFollow_pos => m_flyFollow_pos;
        Vector2 m_flyFollow_pos;

        IEnemy_BT_FlyFollow i_flyFollow => this;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_flyAround_deg = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_DEG");
            e_flyAround_radius = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_RADIUS");
            e_flyAround_count_max = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MAX"));
            e_flyAround_count_min = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MIN"));

            e_create_cd_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "CREATE_CD_AREA");
            
            e_create_interval_tick = Mathf.RoundToInt(float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "CREATE_INTERVAL_SEC")) * Config.PHYSICS_TICKS_PER_SECOND);
            e_sub_cell_count_single = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "SUB_CELL_COUNT_SINGLE"));

            e_sub_cell_speed_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "SUB_CELL_SPEED_AREA");
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
            m_flyAround_count_limit = Random.Range(e_flyAround_count_min, e_flyAround_count_max + 1);

            i_flyAround.calc_flyAround_pos();
        }


        public void do_Chase(Enemy cell)
        {
            i_flyAround.flyAround_on_tick(cell, cell.target.Position);

            if (m_flyAround_count >= m_flyAround_count_limit)
            {
                m_flyAround_count = 0;
                m_state = EN_FSM.Hover;
            }
        }
        #endregion


        #region Hover
        public void start_Hover(Enemy cell)
        {
            m_flyFollow_pos = cell.pos - cell.target.Position;

            m_create_cd_tick = Mathf.RoundToInt(Random.Range(e_create_cd_area.x, e_create_cd_area.y) * Config.PHYSICS_TICKS_PER_SECOND);
            Request_Helper.delay_do($"start_summon_req_201404_{cell.GUID}", m_create_cd_tick,
                (_) => 
                {
                    m_state = EN_FSM.Summon;
                });
        }


        public void do_Hover(Enemy cell)
        {
            i_flyFollow.notify_on_tick(cell, cell.target.Position);
        }
        #endregion


        #region Summon
        public void start_Summon(Enemy cell)
        {
            m_sub_cell_count_single = 0;

            for (int i = 0; i < e_sub_cell_count_single; i++)
            {
                Request_Helper.delay_do($"add_enemy_req_201404_{cell.GUID}", i * e_create_interval_tick,
                (_) => {
                    //进行召唤
                    var mgr = cell.mgr;
                    var pd = mgr.pd;

                    var deg = Random.Range(0, 360);
                    var rad = deg * Mathf.Deg2Rad;

                    var sub_cell_pos = 0.1f * new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) + cell.pos;
                    sub_cell_pos.x -= mgr.ctx.caravan_pos.x;

                    var sub_cell = pd.create_single_cell_directly(201405u, sub_cell_pos, "Default");
                    sub_cell.velocity = cell.velocity + Random.insideUnitCircle.normalized * sub_cell_speed;

                    m_sub_cell_count_single++;
                    if (m_sub_cell_count_single == e_sub_cell_count_single)
                    {
                        m_state = EN_FSM.Hover;
                    }
                });
            }
        }


        public void do_Summon(Enemy cell)
        {
            i_flyFollow.notify_on_tick(cell, cell.target.Position);
        }
        #endregion
    }
}

